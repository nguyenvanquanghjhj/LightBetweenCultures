"""Bounded verification and full checkpoint for the terrain correction.
Historical UI-polish evidence remains immutable and separately labelled.
"""
import datetime, hashlib, html, json, shutil, sys
from pathlib import Path
from terrain_routes import ROOT, BASE, OUT, REL, read, write

V=ROOT/'Validation/TerrainRoutes-20261017'
ASM=ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
REASONS={
 'H0-H11':'Rời điểm neo nước, đứng ở chiếu nghỉ bậc thang rồi lên sân lát đá.',
 'H0-H1':'Vòng theo sân dưới, lên bậc thang và qua mặt cầu nhỏ phía tây; bỏ đoạn chéo quầy chợ.',
 'H1-H2':'Theo phố trước dãy cửa hàng; Cô Mai giữ marker cũ, Lumi đứng trước tiệm.',
 'H1-H4':'Đi qua cầu nhỏ, xuống bậc thang rồi theo sân ven bờ tới quảng trường chợ.',
 'H2-H9':'Đi vòng về phố phía tây và sân đền; không cắt qua mái nhà và tán cây giữa hai node.',
 'H2-H13':'Theo mặt phố và sân xưởng; điểm đến nằm trước công trình.',
 'H13-H3':'Men ngõ quanh góc nhà, tránh mái ngói tiền cảnh; dừng trên lối lát đá.',
 'H13-H14':'Ngõ phía trước nhà nối ra đường cong ven sông và chân tượng.',
 'H14-H19':'Men đoạn đường lát đá phía bắc, không chạy qua mái nhà tại marker H19.',
 'H14-H18':'Theo đường cong bờ sông, dừng phía trước tiệm trà.',
 'H18-H5':'Vòng về ngõ, xuống bậc thang, theo bờ kè và qua cầu có mái; bỏ mái nhà và mặt nước.',
 'H5-H6':'Đi trong cầu có mái tới đầu cầu phía tây; sàn khuất được đánh dấu NEEDS_REVIEW.',
 'H6-H7':'Vòng quanh cụm nhà qua ngõ, lên phố ven sông rồi vào sân hội quán từ phía nam; không xuyên dãy nhà giữa.',
 'H6-H15':'Qua cầu có mái và đường ven đảo phía đông; mép tán cây R3 cần nghiệm thu.',
 'H15-H17':'Theo đường lát đá tới đầu cầu, rồi xuống cầu tàu phía đông; bỏ đoạn tắt qua cây.',
 'H7-H8':'Vòng qua phố/ngõ có lối nhìn thấy tới sân đền; không vượt tường kín phía tây hội quán. Bậc bên đền R2 cần nghiệm thu.',
 'H7-H20':'Theo lối lát đá phía trước dãy nhà phía bắc; tách điểm cổng trên tranh khỏi chân Lumi.',
 'H4-H10':'Vòng xuống mép quảng trường rồi theo đường ngoài chợ; không đi xuyên mái quầy hàng.',
 'H11-H12':'Theo sân dưới cây đèn và đường ven bờ; dừng bên ngoài bồn cây của marker cua.',
 'H12-H16':'Men bờ kè phía dưới bồn hoa tới cầu tàu, tránh tán cây và nước.',
 'H3-H21':'Rời ngõ về phố trước tiệm, vòng lên đường phía tây; không xuyên các mái nhà để đến rương áp mái.',
 'H17-H5':'Tuyến đò riêng đi trong lòng kênh giữa hai bến; giữ điều kiện HQ04 và biểu tượng thuyền.'
}

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()

def review():
    diffs=read(OUT/'edge-waypoint-diff.json')
    for d in diffs:d['reason']=REASONS[d['edge']]
    write(OUT/'edge-waypoint-diff.json',diffs)
    rows=[]
    for d in diffs:
        old=[d['oldEndpoints'][0]]+d['oldPixels']+[d['oldEndpoints'][1]]
        new=[d['newEndpoints'][0]]+d['newPixels']+[d['newEndpoints'][1]]
        rows.append(f"| {d['edge']} | {d['travel']} | {REASONS[d['edge']]} | {len(old)} → {len(new)} |")
    text='# Waypoint Hội An — trước/sau\n\nTọa độ pixel ảnh gốc 1448×1086, gốc trên trái. Các điểm đầu/cuối mới là arrival anchor, không đổi marker hoặc ID node.\n\n'
    text+='| Edge | Kiểu | Lý do sửa | Tổng điểm cũ → mới |\n|---|---|---|---|\n'+'\n'.join(rows)+'\n\n'
    for d in diffs:
        text+=f"## {d['edge']}\n\n{d['reason']}\n\nĐiểm đến cũ: `{d['oldEndpoints']}`; mới: `{d['newEndpoints']}`.\n\nWaypoint cũ: `{d['oldPixels']}`\n\nWaypoint mới: `{d['newPixels']}`\n\n"
    (OUT/'edge-waypoints.md').write_text(text,encoding='utf-8')
    frames=read(V/'Diagnostics01/terrain-walking-frames.json')
    page='''<!doctype html><meta charset="utf-8"><title>LBC — Nghiệm thu địa hình</title>
<style>body{margin:24px;background:#173e48;color:#ffe4af;font:16px system-ui}a{color:#73dfca}img{max-width:100%;height:auto}section{margin:24px 0;padding:16px;background:#214d55} .pair{display:flex;gap:12px}.pair>div{width:50%} .pair img{max-height:780px}button,select,input{font:inherit;margin:8px}#frames{max-height:720px}details{margin:12px 0}code{white-space:pre-wrap;overflow-wrap:anywhere}</style>
<h1>Hội An — tuyến theo tranh</h1><p>Chuyển động và trạng thái có kiểm thử riêng. Nghiệm thu địa hình: PENDING; R1/R2/R3: NEEDS_REVIEW. Ảnh runtime bên dưới là capture mới, không chỉnh pixel. Đồ thị kỹ thuật có lớp chú thích; ảnh Map Overview A trong game giữ nguyên.</p>
<section><h2>Đường trên ảnh gốc</h2><a href="../../Docs/Maps/TerrainRoutes-20261017/edge-waypoints.md">Bảng đầy đủ waypoint cũ/mới và lý do</a><div class="pair">'''
    # Review lives at project root so all links are portable within the checkpoint.
    page=page.replace('../../Docs/Maps/','Docs/Maps/')
    for mode in ['before','after']:page+=f'<div><h3>{mode.upper()} — annotation</h3><a href="Docs/Maps/TerrainRoutes-20261017/{mode}-terrain-overlay.png"><img src="Docs/Maps/TerrainRoutes-20261017/{mode}-terrain-overlay.png"></a></div>'
    page+='</div></section>'
    for view in ['Mai','WestDock','CoveredBridge','NorthStreet']:
        page+=f'<section><h2>{view} — runtime cùng vị trí và zoom</h2><div class="pair">'
        for folder,prefix in [('Before','Before'),('Diagnostics01','After')]:
            p=f'Validation/TerrainRoutes-20261017/{folder}/{prefix}-{view}.png'
            page+=f'<div><h3>{prefix}</h3><a href="{p}"><img src="{p}"></a></div>'
        page+='</div></section>'
    page+='''<section><h2>Frame chuyển động mới</h2><p>Phát chậm để quan sát. Tọa độ và thời điểm capture thật được ghi kèm; Lumi khuất trong cầu có mái, xuất hiện lại tại đầu cầu.</p><select id="sequence"></select><button id="play">Phát / Dừng</button><input id="step" type="range" min="0" value="0"><p id="info"></p><a id="full"><img id="frames"></a></section>'''
    page+='<script>const all='+json.dumps(frames)+';'+'''
const seq=document.querySelector('#sequence'),step=document.querySelector('#step'),info=document.querySelector('#info'),im=document.querySelector('#frames');
for(const s of [...new Set(all.map(f=>f.sequence))])seq.add(new Option(s,s));let list=[],timer=null;
function show(){let f=list[Number(step.value)];let p='Validation/TerrainRoutes-20261017/Diagnostics01/'+f.file;im.src=p;document.querySelector('#full').href=p;info.textContent=f.file+' | pixel '+f.originalPixels.x.toFixed(1)+', '+f.originalPixels.y.toFixed(1)+' | realtime '+f.realtime.toFixed(3)+' | covered='+f.covered;}
function select(){list=all.filter(f=>f.sequence===seq.value);step.max=list.length-1;step.value=0;show();}seq.onchange=select;step.oninput=show;document.querySelector('#play').onclick=()=>{if(timer){clearInterval(timer);timer=null;}else timer=setInterval(()=>{step.value=(Number(step.value)+1)%list.length;show()},250)};select();</script>'''
    (ROOT/'TerrainRoutesReview.html').write_text(page,encoding='utf-8')

def evidence():
    baseline=read(BASE/'CHECKPOINT_MANIFEST.json')
    changed=[]
    for e in baseline['entries']:
        p=ROOT/e['path']
        assert p.is_file(),e['path']
        if sha(p)!=e['sha256']:changed.append(e['path'])
    allowed={'Assets/_Game/Resources/Content/GameplayContent.json',
       'Assets/_Game/Scripts/Content/ContentDefinitions.cs','Assets/_Game/Scripts/Core/GameData.cs',
       'Assets/_Game/Scripts/Maps/CityRouteGeometry.cs','Assets/_Game/Scripts/Maps/CityMap.cs',
       'Assets/_Game/Scripts/Maps/CityMapPaths.cs','Assets/_Game/Scripts/Validation/SliceSmokeRunner.cs',
       'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Chapters.cs'}
    assert all(p in allowed or p.startswith('Builds/Windows/') for p in changed),changed
    old=read(BASE/REL);new=read(ROOT/REL);oldh=next(c for c in old['cities'] if c['id']=='hoi_an');newh=next(c for c in new['cities'] if c['id']=='hoi_an')
    for a,b in zip(oldh['nodes'],newh['nodes']):assert a=={k:v for k,v in b.items() if k not in ('hasArrivalPosition','arrivalPosition')}
    for a,b in zip(oldh['edges'],newh['edges']):assert {k:v for k,v in a.items() if k not in ('waypoints','status')}=={k:v for k,v in b.items() if k not in ('waypoints','status')}
    stripped=json.loads(json.dumps(new));hh=next(c for c in stripped['cities'] if c['id']=='hoi_an');hh['nodes']=oldh['nodes'];hh['edges']=oldh['edges'];hh.pop('coveredWalkways')
    assert stripped==old,'Quest/gate/other-city content regression'
    runs=[]
    for directory in ['Before','Journey01','Diagnostics01']+['Restart01-'+s for s in ['Stage1','Stage2','Stage3','Stage4','Ferry','Hue','Jeonju','DaLat']]:
        result=V/directory/'smoke-results.txt';lines=result.read_text(encoding='utf-8-sig').splitlines()
        assert lines[-1]=='ALL CHECKS PASSED',result
        assert not any('FAIL:' in l or l.startswith('ERROR:') for l in lines),result
        runs.append(dict(run=directory,assertions=sum(l.startswith('PASS:') for l in lines),resultSHA256=sha(result)))
    traces=read(V/'Diagnostics01/hoi_an-chapter-route-traces.json')
    assert len(traces)==44 and all(t['arrived'] and t['maxDistance']<.02 for t in traces)
    screenshots=[]
    for p in V.rglob('*.png'):
        if p.parent.name!='Before':assert p.stat().st_mtime>=ASM.stat().st_mtime,p
        screenshots.append(dict(path=str(p.relative_to(ROOT)).replace('\\','/'),sha256=sha(p),bytes=p.stat().st_size))
    inputs=[p for p in (ROOT/'Assets/_Game').rglob('*') if p.is_file() and (p.suffix=='.cs' or 'Resources' in p.parts)]
    assert 'Windows build: Succeeded' in (ROOT/'Logs/terrain-routes-build-01.log').read_text(encoding='utf-8-sig')
    for p in inputs:
        if p.suffix in ('.cs','.json'):assert p.stat().st_mtime<=ASM.stat().st_mtime,'Runtime input newer than tested assembly: '+str(p)
    proof=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
        baseline=str(BASE),baselineFilesCompared=len(baseline['entries']),changedBaselineFiles=changed,
        status='MOTION_GAMEPLAY_SAVE_PASS_TERRAIN_PENDING_VISUAL_ACCEPTANCE',
        terrainNeedsReview=read(OUT/'needs-review.json'),assemblySHA256=sha(ASM),
        buildLogSHA256=sha(ROOT/'Logs/terrain-routes-build-01.log'),runs=runs,
        totalAssertions=sum(r['assertions'] for r in runs),routeWalks=44,
        maxRouteDeviationUI=max(t['maxDistance'] for t in traces),
        sourceInputs=[dict(path=str(p.relative_to(ROOT)).replace('\\','/'),sha256=sha(p)) for p in inputs],screenshots=screenshots,
        limitations=['Terrain acceptance requires visual review, not inferred from movement assertions.',
          'Route-only walks isolate edges after earned HQ01/HQ04; not fresh-save story evidence.',
          'R1 covered floor is occluded in baked art. R2 temple side steps and R3 eastern canopy need review.',
          'Other five cities retain original geometry; terrain not reviewed in this task. Android/device NOT VERIFIED.'])
    write(V/'evidence-manifest.json',proof);print('EVIDENCE VERIFIED:',proof['totalAssertions'],'assertions;',len(screenshots),'fresh PNGs; max route error',proof['maxRouteDeviationUI'])
    return proof

def checkpoint(proof):
    dest=ROOT.parent/'Backups/CheckpointTerrainRoutes-20261017';assert not dest.exists(),dest
    paths={e['path'] for e in read(BASE/'CHECKPOINT_MANIFEST.json')['entries']}
    for folder in ['Assets','Packages','ProjectSettings','Docs','Tools','References','Builds','Validation/TerrainRoutes-20261017']:
        paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in (ROOT/folder).rglob('*') if p.is_file())
    paths.update(p.name for p in ROOT.glob('*.md'));paths.add('TerrainRoutesReview.html');paths.add('Logs/terrain-routes-build-01.log')
    entries=[]
    for relative in sorted(paths):
        src=ROOT/relative;target=dest/relative;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,target)
        digest=sha(src);assert sha(target)==digest,relative
        entries.append(dict(path=relative,bytes=src.stat().st_size,sha256=digest))
    for e in entries:assert sha(ROOT/e['path'])==e['sha256'],'Source changed during checkpoint: '+e['path']
    write(dest/'CHECKPOINT_MANIFEST.json',dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
        source=str(ROOT),baselineCheckpoint=str(BASE),type='FULL_SOURCE_BUILD_WITH_SELECTED_RUNTIME_EVIDENCE',
        status=proof['status'],verifiedSHA256=True,fileCount=len(entries),totalBytes=sum(e['bytes'] for e in entries),
        tests=dict(assertions=proof['totalAssertions'],routeWalks=44,processRestarts=8),
        assemblySHA256=proof['assemblySHA256'],queuedTasks=[2,3,4],limitations=proof['limitations'],entries=entries))
    print('CHECKPOINT VERIFIED:',dest,';',len(entries),'files;',sum(e['bytes'] for e in entries),'bytes')

if __name__=='__main__':
    review()
    if '--verify' in sys.argv or '--checkpoint' in sys.argv:
        p=evidence()
        if '--checkpoint' in sys.argv:checkpoint(p)
