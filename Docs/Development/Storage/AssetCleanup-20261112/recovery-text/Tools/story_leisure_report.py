"""Bounded verification/report for this task only; never modifies gameplay or historical QA."""
from pathlib import Path
from datetime import datetime,timezone
import json,hashlib,re
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/QA/10Story12Leisure'
QA=ROOT/'Validation/10Story12Leisure-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
baseline=read(OUT/'baseline.json')
allowed={
'Assets/_Game/Resources/AdventureUIArt.asset','Assets/_Game/Scripts/Content/NodeEventManager.cs',
'Assets/_Game/Scripts/Content/PuzzleManager.cs','Assets/_Game/Scripts/Core/QuestManager.cs',
'Assets/_Game/Scripts/Core/SaveSystem.cs','Assets/_Game/Scripts/Maps/CityMap.cs',
'Assets/_Game/Scripts/Maps/MainMenu.cs','Assets/_Game/Scripts/UI/AdventureUIArt.cs',
'Assets/_Game/Scripts/UI/GameplayPanels.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Chapters.cs',
'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs'}
tracked={e['path'].replace('\\','/'):e for e in baseline['entries']}
protected=[];changed=[]
for name,e in tracked.items():
 p=ROOT/name;sha=digest(p)
 if sha!=e['sha256']:
  assert name in allowed,name
  changed.append(name)
 else:protected.append(dict(path=name,sha256=sha))
before=Path(baseline['before'])
art=ROOT/'Assets/_Game/Resources/AdventureUIArt.asset'
old=(before/art.relative_to(ROOT)).read_bytes()
assert re.sub(rb'(?m)^  activity(?:Ground|Wall|PinkPot|CitrusPot):[^\r\n]*\r?\n',b'',art.read_bytes())==old
q=ROOT/'Assets/_Game/Scripts/Core/QuestManager.cs'
assert q.read_text(encoding='utf-8-sig').replace('sealed partial class QuestManager','sealed class QuestManager')==(before/q.relative_to(ROOT)).read_text(encoding='utf-8-sig')
content=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
story=read(ROOT/'Assets/_Game/Resources/Content/StoryInteractions.json')['entries']
leisure=read(ROOT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']
assert len(story)==22 and len({e['id'] for e in story})==22 and all(e['enabled'] for e in story)
assert sum(e['kind']=='STORY_MINIGAME' for e in story)==10 and sum(e['kind']=='LIGHT_STORY_RESOLUTION' for e in story)==12
assert {e['id'] for e in story}=={e['id'] for e in content['puzzles']}
assert len(leisure)==12 and all(e['enabled'] and e['mapSize']==dict(x=27,y=30) for e in leisure)
for city in content['cities']:
 entries=[e for e in leisure if e['cityId']==city['id']];assert len(entries)==2
 assert all(e['nodeId'] in {n['id'] for n in city['nodes']} for e in entries)
for e in read(OUT/'leisure-runtime-registry.json')['entries']:
 assert digest(ROOT/e['source'])==digest(ROOT/e['runtime'])==e['sha256']
assert len(content['quests'])==29 and len(content['checkIns'])==18
assert sum(len(c['nodes']) for c in content['cities'])==133 and sum(len(c['edges']) for c in content['cities'])==155
verification=dict(status='PASS',checkedScopedFiles=len(protected),protectedFiles=protected,changed=changed,
 counts=dict(story=10,light=12,leisure=12,quests=29,nodes=133,edges=155,checkins=18),
 artBindings='Only four reused existing scenery fields added; every previous byte retained',
 scope='Explicit task baseline only; not a full project scan')
(OUT/'protected-source-verification.json').write_text(json.dumps(verification,indent=2)+'\n',encoding='utf-8')
runs=[]
required=['pilot02','all12-01','recheck01','e2e-vn01','e2e-korea01','restart-partial01','restart-all12-01']
for name in required:
 p=QA/name/'smoke-results.txt'
 lines=p.read_text(encoding='utf-8-sig').splitlines() if p.exists() else []
 result='PASS' if lines and lines[-1]=='ALL CHECKS PASSED' else 'FAIL' if lines else 'NOT_FINISHED'
 runs.append(dict(run=name,result=result,assertions=sum(s.startswith('PASS:') for s in lines),resultPath=str(p.relative_to(ROOT))))
summary=dict(updatedUTC=datetime.now(timezone.utc).isoformat(),runs=runs,required=required,
 windowsBuild='Logs/story-leisure-build05.log',android='NOT_TESTED',poses='LIMITED_CANONICAL_TRANSFORM / POSE_ART_PENDING',
 full29QuestReplay='NOT_RUN: earned two-route test covers all22 proofs/all24 fragments/6 relics/boss, not every optional errand')
(OUT/'run-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'source':'PASS','protected':len(protected),'runs':[(r['run'],r['result']) for r in runs]},ensure_ascii=False))
