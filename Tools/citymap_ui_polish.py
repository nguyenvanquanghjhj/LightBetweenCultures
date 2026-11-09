"""Targeted authoring/evidence for task 1; never changes nodes or gameplay records."""
import collections
import copy
import hashlib
import html
import json
import shutil
import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT.parent / 'Backups/BeforeCityMapUIPolish-20261017'
CONTENT = Path('Assets/_Game/Resources/Content/GameplayContent.json')
OUT = ROOT / 'Docs/Maps/CityMapUIPolish-20261017'


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def write(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def key(e):
    return tuple(sorted((e['from'], e['to'])))


def degree(city):
    d = collections.Counter(v for e in city['edges'] for v in (e['from'], e['to']))
    return {n['id']: d[n['id']] for n in city['nodes']}


def graph(city, path, title):
    # Standalone vector diagram, not a modified map or new runtime art asset.
    body = [f'<svg xmlns="http://www.w3.org/2000/svg" width="1100" height="860" viewBox="0 0 1100 860"><rect width="1100" height="860" fill="#173e48"/><text x="24" y="34" fill="#ffe1a4" font-family="sans-serif" font-size="22">{html.escape(title)}</text>']
    pos = {n['id']: (50 + n['position']['x'] * 1000, 70 + (1-n['position']['y']) * 710) for n in city['nodes']}
    for e in city['edges']:
        a, b = pos[e['from']], pos[e['to']]
        body.append(f'<path d="M {a[0]} {a[1]} L {b[0]} {b[1]}" stroke="{ "#73cfd3" if e.get("travel")=="ferry" else "#baa777"}" stroke-width="2" fill="none"/>')
    for n in city['nodes']:
        x, y = pos[n['id']]
        body.append(f'<circle cx="{x}" cy="{y}" r="17" fill="#f6dfae"/><text x="{x}" y="{y+5}" text-anchor="middle" font-family="sans-serif" font-size="13" fill="#173e48">{n["id"]}</text>')
    body.append('</svg>')
    path.write_text('\n'.join(body), encoding='utf-8')


def author():
    old = load(BASE / CONTENT)
    current = load(ROOT / CONTENT)
    before = next(c for c in old['cities'] if c['id'] == 'hoi_an')
    city = next(c for c in current['cities'] if c['id'] == 'hoi_an')
    # Shared physical corridors use exactly the same vertices, so the renderer
    # can draw one stroke while both directions use the same movement geometry.
    quay = [(441,766),(500,772),(558,766),(602,748)]
    stairs = [(628,695),(643,661),(640,636),(637,611),(635,591),(620,577),(592,563),(550,554),(502,548),(454,547)]
    west = [(470,552),(530,561),(586,572),(625,585),(641,578),(641,549),(628,520),(628,494),(596,486),(570,478)]
    alley = [(630,495),(660,492),(685,484),(725,480),(760,467),(791,450),(812,432),(802,410),(770,393)]
    bridge = [(812,432),(813,452),(791,470),(771,484),(791,523),(812,551),(851,574),(893,567),(935,553),(977,550),(1020,561)]
    east = [(1060,575),(1102,598),(1137,632),(1167,655),(1187,668),(1227,670),(1265,686),(1268,705),(1235,725),(1207,729)]
    routes = [
        ('H0','H11',[(430,813),(405,781)]),
        ('H0','H1',[(430,813),(355,743),(271,655),(218,583),(323,539)]),
        ('H1','H2',west),
        ('H1','H4',[(355,552),(285,554),(226,566),(180,588),(170,613),(185,641)]),
        ('H2','H9',[(535,445),(514,460),(485,474),(460,480),(431,477),(405,460),(392,437),(390,413),(391,385),(390,352),(376,330),(354,313),(326,309),(299,312),(282,295),(266,278)]),
        ('H2','H13',[(570,478),(596,486),(628,494)]),
        ('H13','H3',alley),
        ('H13','H14',[(630,495),(660,492),(685,484),(725,480),(760,467),(791,450),(836,433),(877,414),(915,399),(940,377)]),
        ('H14','H19',[(965,336),(990,318),(1010,291),(1028,269),(1040,252)]),
        ('H14','H18',[(940,377),(915,399),(877,414),(836,433)]),
        ('H18','H5',bridge+east),
        ('H5','H6',list(reversed(east))),
        ('H6','H7',[(977,550),(935,553),(893,567),(851,574),(812,551),(791,523),(771,484),(725,480),(685,484),(660,492),(630,495),(656,480),(670,450),(674,420),(670,390),(660,362),(654,336),(649,308),(642,283),(637,258),(636,237),(639,216),(643,195)]),
        ('H6','H15',east[:4]+[(1196,639),(1241,627),(1288,605),(1322,582),(1344,554),(1361,524),(1376,494),(1370,480)]),
        ('H15','H17',[(1370,480),(1376,494),(1361,524),(1344,554),(1322,582)]),
        ('H7','H8',[(605,166),(568,161),(546,147),(531,128),(517,110),(493,102),(469,101),(445,101),(421,100),(399,100),(374,108),(351,120),(326,126),(303,120),(276,111)]),
        ('H7','H20',[(690,172),(727,182)]),
        ('H4','H10',[(185,641),(160,620),(151,594)]),
        ('H11','H12',quay+[(654,738),(707,744)]),
        ('H12','H16',[(782,731),(825,732),(866,741),(901,751),(939,778),(961,794)]),
        ('H3','H21',list(reversed(alley))+[(625,450),(618,420),(615,395),(596,376),(571,368),(545,365),(511,365),(481,358),(455,340),(433,321),(421,300),(421,270),(430,250)]),
    ]
    authored = []
    for a, b, pixels in routes:
        authored.append(dict(id=a+'__'+b, **{'from':a,'to':b}, bidirectional=True,
            waypoints=[dict(x=round(x/1448,9),y=round(1-y/1086,9)) for x,y in pixels],
            requiredFlags=[],travel='walk',status='task1_topology_and_corridor_polish_visual_review'))
    ferry = copy.deepcopy(next(e for e in before['edges'] if e.get('travel') == 'ferry'))
    # Keep the existing western approach to H1. Routing H11-H1 along the eastern
    # stairs made the opening immediately retrace its bridge on H1-H2.
    for i,e in enumerate(authored):
        if key(e)==tuple(sorted(('H0','H1'))):authored[i]=copy.deepcopy(next(n for n in before['edges'] if key(n)==key(e)))
    authored.append(ferry)
    city['edges'] = authored
    # Explicitly preserve node order, positions, gates, all quests, actors and rewards.
    assert city['nodes'] == before['nodes'] and city['gates'] == before['gates']
    candidate = copy.deepcopy(current)
    next(c for c in candidate['cities'] if c['id']=='hoi_an')['edges'] = before['edges']
    assert candidate == old, 'Out of scope modification exists; inspect before authoring'
    assert max(degree(city).values()) <= 3
    write(ROOT / CONTENT, current)
    OUT.mkdir(parents=True,exist_ok=True)
    diff = dict(beforeEdges=len(before['edges']),afterEdges=len(authored),beforeDegree=degree(before),afterDegree=degree(city),
        removed=[e for e in before['edges'] if key(e) not in {key(n) for n in authored}],
        added=[e for e in authored if key(e) not in {key(n) for n in before['edges']}],
        retained=[dict(before=e,after=next(n for n in authored if key(n)==key(e))) for e in before['edges'] if key(e) in {key(n) for n in authored}],
        coordinateSystem='Normalized bottom-left; pixel examples use original 1448x1086 top-left',
        terrainLimits=['Fixed H2/H3/H8/H9/H18 anchors touch illustrated roofs or foliage; their exact ground footprint is occluded.',
          'Central north alley H6-H7 and western lane H2-H9/H3-H21 require visual review of occluded short segments.',
          'H5-H6/H18-H5 cross the existing covered bridge, not open water. H17-H5 remains explicitly ferry-only.'])
    write(OUT/'route-diff.json',diff)
    graph(before,OUT/'graph-before.svg','BEFORE: 24 edges; maximum degree 5 (topology only)')
    graph(city,OUT/'graph-after.svg','AFTER: 22 edges; maximum degree 3 (topology only)')
    print('Authored 22 edges; preserved all nodes/gameplay; wrote full old/new geometry.')


def audit():
    content = load(ROOT / CONTENT)
    city = next(c for c in content['cities'] if c['id']=='hoi_an')
    gates = city['gates']
    before = next(c for c in load(BASE/CONTENT)['cities'] if c['id']=='hoi_an')
    retired = [e for e in before['edges'] if key(e) not in {key(n) for n in city['edges']}]
    results = []
    # Exhaust all independent production edge/node flags at each legal discovery
    # stage. Remove H5 to prove every pre-boss side branch remains reachable.
    optional = sorted({f for n in city['nodes'] for f in n.get('requiredFlags',[])} | {f for e in city['edges'] for f in e.get('requiredFlags',[])})
    for stage in range(5):
        for mask in range(1<<len(optional)):
            flags = {f for i,f in enumerate(optional) if mask&(1<<i)} | {g['condition'] for g in gates if g['unlockStage']<=stage}
            visible = {n['id'] for n in city['nodes'] if n['revealStage']<=stage and set(n.get('requiredFlags',[]))<=flags}
            adj = {n:set() for n in visible}
            for e in city['edges']:
                if e['from'] in visible and e['to'] in visible and set(e.get('requiredFlags',[]))<=flags:
                    adj[e['from']].add(e['to']); adj[e['to']].add(e['from'])
            def reach(block=None):
                seen={'H0'}; todo=['H0']
                while todo:
                    for n in adj[todo.pop()]-seen:
                        if n != block: seen.add(n);todo.append(n)
                return seen
            assert reach()==visible, (stage,mask,visible-reach())
            if stage==2: assert {'H19','H14','H18','H16'}<=reach('H5')
            replacements=[]
            for e in retired:
                if e['from'] not in visible or e['to'] not in visible or not set(e.get('requiredFlags',[]))<=flags:continue
                previous={e['from']:None};todo=collections.deque([e['from']])
                while todo:
                    for n in adj[todo.popleft()]:
                        if n not in previous:previous[n]=True;todo.append(n)
                assert e['to'] in previous
                # Stable BFS reconstructs the actual replacement, not just a boolean.
                previous={e['from']:None};todo=collections.deque([e['from']])
                while todo:
                    parent=todo.popleft()
                    for n in sorted(adj[parent]):
                        if n not in previous:previous[n]=parent;todo.append(n)
                path=[];at=e['to']
                while at is not None:path.append(at);at=previous[at]
                replacements.append(dict(removed=e['id'],replacement=path[::-1]))
            results.append(dict(stage=stage,flags=sorted(flags),reachable=sorted(visible),replacements=replacements))
    assert max(degree(city).values())<=3 and len({key(e) for e in city['edges']})==len(city['edges'])
    write(OUT/'graph-state-audit.json',dict(status='PASS_STATIC_CONNECTIVITY_NOT_RUNTIME_OR_TERRAIN_PROOF',cases=results,degree=degree(city)))
    print(f'PASS: {len(results)} stage/flag connectivity states; all degrees <= 3; H19 accessible before H5.')


def evidence():
    validation=ROOT/'Validation/CityMapUIPolish-20261017'
    current=load(ROOT/CONTENT);old=load(BASE/CONTENT)
    equivalent=copy.deepcopy(current)
    next(c for c in equivalent['cities'] if c['id']=='hoi_an')['edges']=next(c for c in old['cities'] if c['id']=='hoi_an')['edges']
    assert equivalent==old, 'A non-route gameplay/content record changed'
    protected=[];modified=[]
    allowed={str(CONTENT).replace('\\','/'), *('Assets/_Game/Scripts/'+s for s in (
        'Core/GameData.cs','Maps/CityMap.cs','Maps/CityMapPaths.cs','UI/GameplayPanels.cs',
        'UI/InventoryPanel.cs','UI/MobileUI.cs','UI/PortraitPresence.cs',
        'Validation/GameplaySmokeRunner.cs','Validation/GameplaySmokeRunner.Chapters.cs'))}
    for e in load(BASE/'checkpoint-manifest.json')['entries']:
        p=e['path']
        if p.startswith(('Builds/','Logs/','Validation/')):continue
        f=ROOT/p;assert f.exists(),p
        changed=hashlib.sha256(f.read_bytes()).hexdigest()!=e['sha256']
        if changed:assert p in allowed, 'Unexpected protected file change: '+p
        (modified if changed else protected).append(p)
    assembly=ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
    source_inputs=[]
    for f in sorted(list((ROOT/'Assets/_Game/Scripts').rglob('*.cs'))+
                    list((ROOT/'Assets/_Game/Resources').rglob('*.json'))+
                    list((ROOT/'Assets/_Game/Resources').rglob('*.asset'))):
        assert f.stat().st_mtime<=assembly.stat().st_mtime,'Runtime source edited after final build: '+str(f)
        source_inputs.append(dict(path=f.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(f.read_bytes()).hexdigest()))
    runs=[];shots=[]
    dirs=[validation/n for n in ('Final04-Journey','Final04-Reverse','Final04-Diagnostics')]+sorted(validation.glob('Restart04-*'))
    for directory in dirs:
        logs=list(directory.glob('*results.txt'))
        assert len(logs)==1,(directory,logs)
        lines=logs[0].read_text('utf-8-sig').splitlines()
        assert lines[-1]=='ALL CHECKS PASSED',(directory,lines[-1])
        assert not any('FAIL:' in s or s.startswith('ERROR:') for s in lines),directory
        runs.append(dict(directory=str(directory.relative_to(ROOT)),assertions=sum(s.startswith('PASS:') for s in lines),result='PASS'))
        for f in sorted(directory.glob('*.png')):
            assert f.stat().st_mtime>=assembly.stat().st_mtime,f
            shots.append(dict(path=str(f.relative_to(ROOT)),bytes=f.stat().st_size,sha256=hashlib.sha256(f.read_bytes()).hexdigest()))
    traces=load(validation/'Final04-Diagnostics/hoi_an-chapter-route-traces.json')
    assert len(traces)==44 and all(t['arrived'] and len(t['positions'])>1 and t['maxDistance']<.02 for t in traces)
    assert len({(t['from'],t['to']) for t in traces})==44
    assert 'HQ04' in load(validation/'Final04-Diagnostics/polish-ferry-earned.json')['completed']
    for name in ('Final04-Journey','Final04-Reverse'):
        s=load(validation/name/'hoi_an-after.json')
        assert {'HQ01','HQ02','HQ03','HQ05','HQ06','HQ07'}<=set(s['completed'])
        assert s['items'].count('festival_friendship_postcard')==1
    s=load(validation/'Final04-Journey/dalat-final.json');assert {'DQ01','DQ02','DQ03','DQ04','DQ05'}<=set(s['completed'])
    output=dict(status='TASK1_GAMEPLAY_UI_PASS_TERRAIN_OCCLUDED_SEGMENTS_NOT_VERIFIED',
        assemblySHA256=hashlib.sha256(assembly.read_bytes()).hexdigest(),
        buildLog='Logs/citymap-ui-polish-build-04.log',
        sourceInputs=source_inputs,
        seedLineage={'Final04-Journey':'Fresh save via Start / actual gameplay, no pre-unlock fixture',
                     'Final04-Reverse':'BeforeClean/final-save.json, genuinely earned on previous binary; old-save compatibility and reverse city order',
                     'Final04-Diagnostics':'Validation/DaLatChapter-20261017/AcceptanceIntegrated/hoi_an-after.json, genuinely earned prior checkpoint; isolated edge walks and layout fixtures, not fresh E2E'},
        preservedFileCount=len(protected),modifiedProtectedFiles=modified,
        contentInvariant='All non-HoiAn-route records, all 133 nodes/order/positions, quests, items, enemies, gates exactly equal baseline',
        runs=runs,totalAssertions=sum(r['assertions'] for r in runs),
        isolatedBidirectionalRouteWalks=len(traces),maxMovementError=max(t['maxDistance'] for t in traces),
        screenshots=shots,
        limitations=['Occluded ground at fixed illustrated node anchors and narrow northern/western alley sections: NOT VERIFIED terrain clearance.',
                     'Route-only walks and layout stress fixtures are not fresh-save story evidence.',
                     'Android/device testing NOT VERIFIED. Tasks 2, 3, 4 remain queued; Gyeongju/Seoul DATA_ONLY.'])
    write(validation/'final-evidence-manifest.json',output)
    print('Final proof:',sum(r['assertions'] for r in runs),'assertions;',len(shots),'fresh PNG;',len(traces),'route walks')


def checkpoint():
    proof=load(ROOT/'Validation/CityMapUIPolish-20261017/final-evidence-manifest.json')
    backup_root=(ROOT.parent/'Backups').resolve()
    destination=(backup_root/'CheckpointCityMapUIPolish-20261017').resolve()
    assert destination.parent==backup_root and destination!=ROOT and not destination.exists()
    files={e['path'] for e in load(BASE/'checkpoint-manifest.json')['entries']}
    for folder in ('Assets','Packages','ProjectSettings','Docs','Tools','References','Builds/Windows'):
        for f in (ROOT/folder).rglob('*'):
            if f.is_file() and '__pycache__' not in f.parts:files.add(f.relative_to(ROOT).as_posix())
    files.update(f.relative_to(ROOT).as_posix() for f in ROOT.glob('*.md'))
    v=ROOT/'Validation/CityMapUIPolish-20261017'
    selected=[v/n for n in ('BeforeRetry','BeforeClean','BeforeChapters','Final04-Journey','Final04-Reverse','Final04-Diagnostics')]+sorted(v.glob('Restart04-*'))
    for directory in selected:
        files.update(f.relative_to(ROOT).as_posix() for f in directory.rglob('*') if f.is_file())
    for name in ('review.html','final-evidence-manifest.json'):files.add((v/name).relative_to(ROOT).as_posix())
    files.update(f.relative_to(ROOT).as_posix() for f in (ROOT/'Logs').glob('citymap-ui-polish-build-*.log'))
    entries=[]
    for name in sorted(files):
        src=(ROOT/name).resolve();assert src.is_relative_to(ROOT) and src.is_file(),name
        entries.append(dict(path=name,bytes=src.stat().st_size,sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
    destination.mkdir()
    for entry in entries:
        src=ROOT/entry['path'];dst=destination/entry['path'];dst.parent.mkdir(parents=True,exist_ok=True)
        shutil.copy2(src,dst)
        assert hashlib.sha256(dst.read_bytes()).hexdigest()==entry['sha256'],'Backup verification failed: '+entry['path']
    # Detect any concurrent source write during the snapshot as well.
    for entry in entries:assert hashlib.sha256((ROOT/entry['path']).read_bytes()).hexdigest()==entry['sha256'],'Source changed during backup: '+entry['path']
    manifest=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),source=str(ROOT),
        baselineCheckpoint=str(BASE),type='FULL_SOURCE_BUILD_WITH_SELECTED_RUNTIME_EVIDENCE',
        status=proof['status'],fileCount=len(entries),totalBytes=sum(e['bytes'] for e in entries),verifiedSHA256=True,
        tests=dict(assertions=proof['totalAssertions'],routeWalks=44,processRestarts=8,freshRuntimePNGs=len(proof['screenshots'])),
        assemblySHA256=proof['assemblySHA256'],queuedTasks=[2,3,4],limitations=proof['limitations'],entries=entries)
    write(destination/'CHECKPOINT_MANIFEST.json',manifest)
    print('CHECKPOINT VERIFIED:',len(entries),'files;',manifest['totalBytes'],'bytes;',destination)


if __name__=='__main__':
    import sys
    if '--author' in sys.argv: author()
    audit()
    if '--evidence' in sys.argv:evidence()
    if '--checkpoint' in sys.argv:
        evidence()
        checkpoint()
