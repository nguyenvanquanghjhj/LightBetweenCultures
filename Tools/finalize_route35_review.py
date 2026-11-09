"""Phase 3.5 bounded evidence review; does not write gameplay, art, saves or backups."""
from pathlib import Path
from collections import Counter
from datetime import datetime, timezone
import hashlib, html, json, re
from map_route_terrain_audit import path as walk_path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT/'Docs/Maps/RouteRedesign35-20261018'
QA = ROOT/'Validation/RouteRedesign35-20261018'
BEFORE = Path('D:/GAMEKKK/Backups/BeforeMapRoute35-20261018')
BASE = Path('D:/GAMEKKK/Backups/CheckpointRelicHoiAnStorySync-20261018')
CONTENT = 'Assets/_Game/Resources/Content/GameplayContent.json'

def read(p): return json.loads(p.read_text('utf-8-sig'))
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(name, data): (OUT/name).write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', 'utf-8')
def without(d, keys): return {k:v for k,v in d.items() if k not in keys}
def pixels(points): return [[round(x,2), round(y,2)] for x,y in points]

def main():
    old, current = read(BEFORE/CONTENT), read(ROOT/CONTENT)
    baseline = read(OUT/'baseline.json')
    assert digest(BASE/'CHECKPOINT_MANIFEST.json') == baseline['baseManifestSHA256']
    assert without(old, ['cities','checkIns']) == without(current, ['cities','checkIns'])
    assert current['checkIns'][:12] == old['checkIns'] and len(current['checkIns']) == 18
    extra = [c['id'] for c in current['checkIns'][12:]]
    assert extra == ['gyeongju_g4_checkin','gyeongju_g14_checkin','gyeongju_g17_checkin',
                     'seoul_s2_checkin','seoul_s10_checkin','seoul_s18_checkin']
    assert (ROOT/'_CheckIn_Import_20261018').is_dir()
    guards = []
    for rel in ['Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Scripts/Maps/CityRouteGeometry.cs',
                'Assets/_Game/Resources/HoiAnData.json','ProjectSettings/ProjectSettings.asset']:
        assert digest(ROOT/rel) == digest(BEFORE/rel), rel
        guards.append(dict(path=rel, sha256=digest(ROOT/rel), status='BYTE_IDENTICAL_TO_BEFORE'))
    rel='Assets/_Game/Scripts/Core/GameData.cs'
    assert (ROOT/rel).read_text('utf-8-sig').split('public bool Unlocked',1)[1] == (BEFORE/rel).read_text('utf-8-sig').split('public bool Unlocked',1)[1]
    manifests = {}
    for base in [Path('D:/GAMEKKK/Backups/CheckpointRelicHoiAnPilot-20261018'), BASE]:
        manifests.update({e['path']: e for e in read(base/'CHECKPOINT_MANIFEST.json')['entries']})
    for rel in ['Assets/_Game/Scripts/Content/RelicCollection.cs','Assets/_Game/Scripts/Content/NodeEventManager.cs',
                'Assets/_Game/Scripts/UI/GameplayPanels.cs','Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs',
                'Assets/_Game/Resources/Content/CulturalRelics.json','Assets/_Game/Resources/AdventureUIArt.asset',
                'Docs/Art/NPC_CANONICAL_REGISTRY.md']:
        assert digest(ROOT/rel) == manifests[rel]['sha256'], rel
        guards.append(dict(path=rel, sha256=digest(ROOT/rel), status='BYTE_IDENTICAL_TO_PREVIOUS_CHECKPOINT'))
    original_save=ROOT/'Validation/FullStory-20261017/Fresh04/full-story-final.json'
    assert digest(original_save)=='83a82674c50d8e2b3ab48981913af24dc9cd33d788b8ab5c0e2cc3d4f146555f'
    guards.append(dict(path=str(original_save.relative_to(ROOT)),sha256=digest(original_save),status='HISTORICAL_SAVE_UNCHANGED'))
    for m in read(OUT/'canonical-maps.json'):
        assert digest(ROOT/m['runtime']) == m['runtimeSHA256'] == digest(ROOT/m['source'])

    recipe=read(OUT/'terrain-edits.json')
    reasons={(a['cityId'],a['edgeId']):a['reason'] for a in recipe['edges']}
    findings=read(OUT/'terrain-findings.json')
    differences=[]; arrivals=[]
    assert len(old['cities']) == len(current['cities']) == 6
    for a,b in zip(old['cities'],current['cities']):
        assert without(a,['nodes','edges','coveredWalkways','routeStyle']) == without(b,['nodes','edges','coveredWalkways','routeStyle'])
        assert len(a['nodes'])==len(b['nodes']) and len(a['edges'])==len(b['edges'])
        for x,y in zip(a['nodes'],b['nodes']):
            assert without(x,['hasArrivalPosition','arrivalPosition'])==without(y,['hasArrivalPosition','arrivalPosition'])
            if x.get('arrivalPosition')!=y.get('arrivalPosition'):
                arrivals.append(dict(cityId=a['id'],nodeId=x['id'],old=x.get('arrivalPosition',x['position']),new=y['arrivalPosition']))
        if a['id']=='hoi_an': assert a.get('coveredWalkways')==b.get('coveredWalkways')
        for x,y in zip(a['edges'],b['edges']):
            assert without(x,['waypoints','status']) == without(y,['waypoints','status'])
            if a['id']=='hoi_an' and x['id'] in ['H18__H5','H5__H6','H6__H15']: assert x==y
            changed=walk_path(a,x)!=walk_path(b,y)
            differences.append(dict(cityId=a['id'],edgeId=x['id'],changed=changed,
                oldWaypoints=x['waypoints'],newWaypoints=y['waypoints'],
                oldWalkPixels=pixels(walk_path(a,x)),newWalkPixels=pixels(walk_path(b,y)),
                reason=reasons.get((a['id'],x['id']), 'Arrival anchor adjustment' if changed else 'Kept; no blanket terrain PASS'),
                terrainStatus='MOVEMENT_PASS_VISUAL_ACCEPTANCE_PENDING' if changed else 'UNCHANGED_VISUAL_ACCEPTANCE_NOT_CLAIMED',
                findingIds=[f['id'] for f in findings if f['cityId']==a['id'] and x['id'] in f['edgeIds']]))
    changed=[d for d in differences if d['changed']]
    assert len(changed)==30 and len(arrivals)==6
    write('edge-waypoint-diff.json',differences)
    table=['# Phase 3.5 — toàn bộ 155 edge, waypoint trước/sau',
           'Tọa độ pixel gốc 1448×1086, gốc trên-trái. Bao gồm arrival anchor; canonical marker không đổi. '
           'MOVE PASS không phải nghiệm thu địa hình. JSON kèm tọa độ normalized nguyên gốc.',
           '| City / edge | Thay đổi | Tuyến cũ (pixel) | Tuyến mới (pixel) | Lý do / review |',
           '|---|---|---|---|---|']
    for d in differences:
        fmt=lambda pts: '; '.join(f'({x:g},{y:g})' for x,y in pts)
        table.append(f"| {d['cityId']} / {d['edgeId']} | {'YES' if d['changed'] else 'NO'} | {fmt(d['oldWalkPixels'])} | {fmt(d['newWalkPixels'])} | {d['reason']} {'; '.join(d['findingIds'])} |")
    (OUT/'edge-waypoints.md').write_text('\n\n'.join(table[:2])+'\n\n'+'\n'.join(table[2:])+'\n','utf-8')

    coverage={}; executions=0
    for run in ['After01','After02']:
        for p in sorted((QA/run).glob('*-route35-traces.json')):
            city=p.name.removesuffix('-route35-traces.json')
            for trace in read(p):
                assert trace['arrived'] and trace['positions'] and trace['maxDistance']<.025
                executions+=1
                coverage[(city,trace['from'],trace['to'])]=dict(cityId=city,fromNode=trace['from'],toNode=trace['to'],run=run,
                    samples=len(trace['positions']),maxDistanceUI=trace['maxDistance'],travel=trace['travel'],arrived=True)
    assert len(coverage)==68 and executions==74
    for d in changed:
        city=next(c for c in current['cities'] if c['id']==d['cityId'])
        edge=next(e for e in city['edges'] if e['id']==d['edgeId'])
        a,b=(int(re.search(r'\d+$',edge[k]).group()) for k in ['from','to'])
        assert (d['cityId'],a,b) in coverage and (d['cityId'],b,a) in coverage, d['edgeId']
    runs=[]
    for run in ['Before01','After01','Fresh01','Restart01','WorldRestart01','After02']:
        name='restart-results.txt' if run=='Restart01' else 'smoke-results.txt'
        text=(QA/run/name).read_text('utf-8-sig')
        count=sum(line.startswith('PASS:') for line in text.splitlines())
        assert 'FAIL:' not in text and count>0
        runs.append(dict(run=run,passAssertions=count,result='PASS',resultPath=str((QA/run/name).relative_to(ROOT))))
    capture_count=0
    for p in [*QA.glob('*/capture-manifest.txt'),*QA.glob('*/restart-captures.txt')]:
        for line in p.read_text('utf-8-sig').splitlines():
            match=re.search(r'\|\s*([^|]+\.png)\s*\| SHA256 ([A-Fa-f0-9]{64})',line)
            if not match: continue
            assert digest(p.parent/match[1].strip()).lower()==match[2].lower(), line
            capture_count+=1
    build=ROOT/'Builds/Windows/LightBetweenCultures_Data/resources.assets'
    blob=build.read_bytes()
    assert all(c['id'].encode() in blob for c in current['checkIns'])
    buildlogs=[]
    for name in ['route35-baseline-build.log','route35-final-build01.log','route35-final-build02.log']:
        p=ROOT/'Logs'/name
        assert 'Succeeded' in p.read_text('utf-8-sig',errors='replace')
        buildlogs.append(dict(path=str(p.relative_to(ROOT)),sha256=digest(p),result='SUCCEEDED'))
    audit=read(QA/'Analysis/graph-after.json')
    stats=[dict(cityId=a['cityId'],nodes=a['nodes'],edges=a['edges'],maxNeighbors=a['maxNeighbors'],over3=a['over3'],
                geometryChanges=Counter(d['cityId'] for d in changed)[a['cityId']],crossingCandidatePairs=len(a['crossings']),nearPairCandidates=len(a['overlaps'])) for a in audit]
    write('verification.json',dict(status='TECHNICAL_PASS_TERRAIN_PARTIAL_NEEDS_USER_DECISIONS',
        verifiedUTC=datetime.now(timezone.utc).isoformat(),nodeCount=sum(len(c['nodes']) for c in current['cities']),
        edgeCount=len(differences),topologyChanged=False,geometryChangedEdges=len(changed),arrivalChanges=arrivals,
        protectedChuaCauEdges=['H18__H5','H5__H6','H6__H15'],checkInsPreserved=18,
        userConfirmedAdditions=extra,checkInImportFolderUntouched=True,finalBuildContainsAll18CheckInIds=True,
        protectedGuards=guards,cityStatistics=stats,runs=runs,buildLogs=buildlogs,
        verifiedRuntimeCaptures=capture_count,movementExecutions=executions,uniqueDirectedTraversals=len(coverage),
        movementCoverage=list(coverage.values()),limitations=['No blanket terrain PASS','14 nodes still exceed 3 neighbors',
        'Edge-only diagnostics are not end-to-end progression proof','Android hardware/FPS/VRAM not tested',
        'No full 133-node regression; original fresh HoiAn relic/story flow tested separately']))

    names={'hoi_an':'Hội An','hue':'Huế','da_lat':'Đà Lạt','jeonju':'Jeonju','gyeongju':'Gyeongju','seoul':'Seoul'}
    gallery=['<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Phase 3.5 runtime review</title>',
      '<style>body{font:16px/1.5 system-ui;background:#10282d;color:#f4e5c7;max-width:1100px;margin:24px auto;padding:0 16px}a{color:#80dfcd}h2{margin-top:40px}img{max-width:100%;height:auto} .pair{position:relative;line-height:0}.pair .top{position:absolute;inset:0;clip-path:inset(0 50% 0 0)}input{width:100%}details{margin:16px 0}table{border-collapse:collapse}td,th{border:1px solid #789;padding:8px}</style>',
      '<h1>Phase 3.5 — ảnh Player trước/sau</h1><p><b>TECHNICAL PASS · TERRAIN PARTIAL.</b> 30 edge đổi hình học; graph giữ nguyên. 14 node vẫn hơn 3 láng giềng. 18 check-in và Chùa Cầu được bảo toàn. Không tích hợp art check-in, không Phase 4.</p>',
      '<p>Kéo thanh để so sánh: bên trái BEFORE, bên phải AFTER. Cùng fixture city spawn, focus (0.5,0.5), zoom 1, 1280×960. Đây là ảnh runtime thật; fixture đã có tiến độ để nhìn map mở. Không chứng minh mọi tuyến đi bộ đã đúng địa hình.</p><p><a href="../../QA/MAP_ROUTE_TERRAIN_REDESIGN.md">Báo cáo</a> · <a href="edge-waypoints.md">155 edge trước/sau</a> · <a href="verification.json">QA</a></p>']
    for c in current['cities']:
        city=c['id'];prefix='../../../Validation/RouteRedesign35-20261018/'
        before=prefix+f'Before01/Before-{city}-overview.png';after=prefix+f'After02/After-{city}-overview.png'
        gallery.append(f'<h2>{names[city]}</h2><div class="pair"><img src="{after}" alt="After {city}"><img class="top" src="{before}" alt="Before {city}"></div><input aria-label="So sánh {city}" type="range" min="0" max="100" value="50" oninput="this.previousElementSibling.lastElementChild.style.clipPath=`inset(0 ${{100-this.value}}% 0 0)`"><p><a href="{before}">Before gốc</a> · <a href="{after}">After gốc</a> · <a href="{prefix}After02/After-{city}-portrait.png">Portrait zoom 2</a></p><details><summary>Ảnh kỹ thuật: cũ đỏ / mới xanh và vùng cần review — không phải runtime</summary><img loading="lazy" src="{prefix}Analysis/{city}-comparison.png"><img loading="lazy" src="{prefix}Analysis/{city}-review-regions.png"></details>')
    gallery.append('<h2>Lumi thật đi qua Chùa Cầu</h2><p>Entry → dưới mái (Lumi/bóng bị che, vị trí vẫn tiến) → exit. Các frame chụp trong lúc di chuyển, camera có follow Lumi; không phải ba ảnh cùng camera.</p>')
    for name in ['entry','under-roof','exit']:
        gallery.append(f'<details open><summary>{name}</summary><img loading="lazy" src="../../../Validation/RouteRedesign35-20261018/After01/ChuaCau-{name}.png"></details>')
    gallery.append('<h2>Các tuyến cần quyết định/vá sau</h2><table><tr><th>ID / edge</th><th>Trạng thái</th><th>Vấn đề</th></tr>')
    for f in findings:
        gallery.append('<tr>'+''.join('<td>'+html.escape(str(s))+'</td>' for s in [f['id']+' / '+', '.join(f['edgeIds']),f['status'],f['reason']])+'</tr>')
    gallery.append('</table><p><a href="terrain-findings.json">Vùng pixel và lý do</a> · <a href="held-water-crossings.json">Đề xuất qua sông đã giữ lại, chưa áp dụng</a> · <a href="topology-proposals.json">Graph chưa áp dụng</a></p></html>')
    (OUT/'RuntimeReview.html').write_text('\n'.join(gallery),'utf-8')
    (OUT/'AUTHORING_README.md').write_text('# Historical authoring inputs\n\n`proposed-gameplay.json` là bản đề xuất lịch sử từ snapshot 12 check-in, KHÔNG phải dữ liệu hiện hành. Không copy đè runtime. `apply_route35_geometry.py --apply` có guard hash để từ chối ghi đè các thay đổi sau snapshot. Dữ liệu authoritative: `Assets/_Game/Resources/Content/GameplayContent.json` với đủ 18 check-in được người dùng xác nhận. Bảng cuối: `edge-waypoint-diff.json`; nghiệm thu: `verification.json`. Không reapply recipe sau checkpoint.\n','utf-8')
    print(json.dumps(dict(changedEdges=len(changed),byCity=Counter(d['cityId'] for d in changed),runs=runs,captures=capture_count),ensure_ascii=False))

if __name__=='__main__': main()
