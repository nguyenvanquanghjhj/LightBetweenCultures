"""Verify scope, route traces and fresh Windows evidence; never edits bitmap assets."""
from pathlib import Path
from datetime import datetime, timezone
import hashlib
import json
import re
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT.parent/'Backups/BeforeCityMapNatural-20261016/Assets_Game'
OUT=ROOT/'Validation/CityMapNatural-20261016'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def record(p):return dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),modifiedUTC=datetime.fromtimestamp(p.stat().st_mtime,timezone.utc).isoformat())

def main():
    changed=load(ROOT/'Docs/Maps/HoiAnRouteReview.json')['changed']
    old=load(BASE/'Resources/HoiAnData.json');new=load(ROOT/'Assets/_Game/Resources/HoiAnData.json')
    assert old['nodes']==new['nodes'] and len(new['nodes'])==22
    assert [(e['from'],e['to']) for e in old['edges']]==[(e['from'],e['to']) for e in new['edges']]
    for a,b in zip(old['edges'],new['edges']):
        a=dict(a);b=dict(b);a.pop('bends');b.pop('bends');assert a==b
    old=load(BASE/'Resources/Content/GameplayContent.json');new=load(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
    assert len(new['items'])==37 and len(new['quests'])==28 and sum(len(c['nodes']) for c in new['cities'])==133
    current=next(c for c in new['cities'] if c['id']=='hoi_an')
    for c in old['cities']+new['cities']:
        if c['id']=='hoi_an':
            for e in c['edges']:e.pop('waypoints',None)
    assert old==new,'Only Hoi An waypoints may change in gameplay content'
    presentation=load(ROOT/'Assets/_Game/Resources/Content/AdventurePresentation.json')
    original=load(BASE/'Resources/Content/AdventurePresentation.json')
    assert presentation.pop('mapActorScale')==.72 and presentation.pop('mapActorTouchSize')==52 and presentation.pop('mapActorStyles')==[]
    assert original==presentation,'No actor identity, base dimension, offset, item or preview changed'
    allowed={'Scripts/UI/AdventureUIArt.cs','Scripts/Editor/AdventureUISetup.cs','Scripts/Maps/CityMap.cs','Scripts/Maps/CityRouteGeometry.cs','Scripts/Maps/NodeMarkerView.cs','Scripts/UI/AdventureIconGraphic.cs','Scripts/Validation/GameplaySmokeRunner.cs','Resources/HoiAnData.json','Resources/Content/GameplayContent.json','Resources/Content/AdventurePresentation.json','Resources/AdventureUIArt.asset'}
    preserved=[];modified=[]
    for old in BASE.rglob('*'):
        if not old.is_file():continue
        rel=old.relative_to(BASE).as_posix();p=ROOT/'Assets/_Game'/rel
        assert p.exists(),rel
        if rel in allowed:modified.append(rel);continue
        assert sha(old)==sha(p),rel
        preserved.append(rel)
    oldrefs=re.findall(r'\{fileID: [-\d]+, guid: [a-f\d]+, type: \d+\}',(BASE/'Resources/AdventureUIArt.asset').read_text(encoding='utf-8-sig'))
    refs=re.findall(r'\{fileID: [-\d]+, guid: [a-f\d]+, type: \d+\}',(ROOT/'Assets/_Game/Resources/AdventureUIArt.asset').read_text(encoding='utf-8-sig'))
    assert oldrefs==refs,'Registry sprite GUID/fileIDs preserved'
    tests={}
    for folder,name in [('Final','smoke-results.txt'),('Final','restart-results.txt'),('After','smoke-results.txt'),('AfterBoss','smoke-results.txt')]:
        lines=(OUT/folder/name).read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED',(folder,name)
        tests[f'{folder}/{name}']=dict(passCount=sum(l.startswith('PASS:') for l in lines),passed=True)
    traces=load(OUT/'After/route-traces.json');assert len(traces)==24
    for t in traces:assert t['arrived'] and t['maxDistance']<.02 and len(t['positions'])>10,t
    comparisons=[]
    for before,after in [('Before','After'),('BeforeBoss','AfterBoss')]:
        for view in ['Mai','WestDock','CoveredBridge','NorthStreet','QC-node-anchors']:
            a=OUT/before/f'Before-{view}.png';b=OUT/after/f'After-{view}.png'
            with Image.open(a) as x,Image.open(b) as y:assert x.size==y.size
            assert sha(a)!=sha(b) and b.stat().st_mtime>a.stat().st_mtime
            comparisons.append(dict(view=view,before=record(a),after=record(b)))
    logs=[ROOT/'Logs'/f'map-natural-{n}.log' for n in ['build','player','restart','after','after-boss']]
    for p in logs:
        text=p.read_text(encoding='utf-8-sig',errors='replace')
        assert 'error CS' not in text and 'NullReferenceException' not in text and 'FAIL:' not in text,p
    assert 'Windows build: Succeeded' in logs[0].read_text(encoding='utf-8-sig')
    evidence=[record(p) for folder in ['Before','BeforeBoss','After','AfterBoss','Final'] for p in (OUT/folder).rglob('*') if p.is_file()]
    sources=[record(p) for p in (ROOT/'Assets/_Game/Scripts').rglob('*.cs')]
    sources+=[record(ROOT/'Assets/_Game'/p) for p in allowed if p.startswith('Resources/')]
    manifest=dict(generatedUTC=datetime.now(timezone.utc).isoformat(),tests=tests,routeTraversals=24,
        maxRouteDistance=max(t['maxDistance'] for t in traces),preserved=preserved,modified=modified,
        comparisons=comparisons,evidence=evidence,source=sources,
        binary=record(ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'),logs=[record(p) for p in logs],
        limits=load(ROOT/'Docs/Maps/HoiAnRouteReview.json')['needsApproval'])
    (OUT/'final-run-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    report=['# Waypoint diff — Hội An, 16/10/2026','',
        'Tọa độ chuẩn hóa: (0,0) góc trái dưới, (1,1) góc phải trên. Ảnh nguồn 1448 × 1086 giữ nguyên. Node/graph/điều kiện không đổi.','',
        'Chỉ các hành lang cầu/bờ phố được hiệu chỉnh; xem danh sách cần duyệt ở cuối, không coi toàn bản đồ đã đạt kiểm tra địa hình.','']
    for e in changed:
        report += [f'## {e["edge"]}',e['note'],'',
            'Cũ: '+ ' → '.join(f'({p["x"]:.6f}, {p["y"]:.6f})' for p in e['old']),'',
            'Mới: '+ ' → '.join(f'({p["x"]:.6f}, {p["y"]:.6f})' for p in e['new']),'']
    report+=['## Cần duyệt địa hình','']+['- '+x for x in manifest['limits']]
    (ROOT/'Docs/Maps/HOIAN_WAYPOINT_DIFF.md').write_text('\n'.join(report)+'\n',encoding='utf-8')
    print(json.dumps(dict(tests=tests,preservedFiles=len(preserved),routeTraversals=24,maxRouteDistance=manifest['maxRouteDistance'],freshCaptures=sum(e['path'].endswith('.png') for e in evidence)),ensure_ascii=False))

if __name__=='__main__':main()
