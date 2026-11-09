"""Create only the two-pilot delta, delivered backdrop sources and current QA evidence."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QA/LeisureAnimationPilot-20261019';VAL=ROOT/'Validation/LeisureAnimationPilot-20261019'
DEST=Path('D:/GAMEKKK/Backups/CheckpointLeisureAnimationPilot-20261019')
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
parser=argparse.ArgumentParser();parser.add_argument('--create',action='store_true');args=parser.parse_args()
b=read(DOC/'baseline.json');base=Path(b['baseCheckpoint']);check=read(DOC/'preservation.json')
assert check['result']=='PASS' and read(DOC/'run-summary.json')['windows']=='TARGETED_PASS'
assert sha(base/'CHECKPOINT_MANIFEST.json')==b['baseManifestSHA256']
bm=read(base/'CHECKPOINT_MANIFEST.json');chain=[dict(path=str(base),manifestSHA256=b['baseManifestSHA256']),*bm['baseChain']]
for c in chain:assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256']
paths=set(check['changedExistingFiles'])
for stem in ['Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs','Assets/_Game/Scripts/Editor/LeisurePilotSetup.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.LeisurePilot.cs']:
    paths.update([stem,stem+'.meta'])
# These owner-delivered source backgrounds are newer than the route checkpoint.
# Include their originals/metas to keep that delivery recoverable, without copying other art/cache.
for p in (ROOT/'Assets/_Game/Art/Leisure/Backgrounds').iterdir():
    if p.is_file():paths.add(str(p.relative_to(ROOT)).replace('\\','/'))
paths.update(['Assets/_Game/Art/Leisure.meta','Assets/_Game/Art/Leisure/Backgrounds.meta'])
paths.update('Tools/'+n for n in ['verify_leisure_animation_pilot.py','leisure_animation_pilot_validation.ps1','finalize_leisure_animation_pilot.py','checkpoint_leisure_animation_pilot.py'])
paths.add('Docs/QA/LUMI_LEISURE_ANIMATION_PILOT_20261019.md')
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in DOC.iterdir() if p.is_file() and p.name!='checkpoint.json')
for folder in [VAL/'player04',VAL/'restart01']:
    paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in folder.iterdir() if p.is_file())
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in VAL.iterdir() if p.is_file())
for old in ['player01','player02','player03']:
    for name in ['player.log','smoke-results.txt','resolution-diagnostics.txt','input-provenance.json']:
        p=VAL/old/name
        if p.exists():paths.add(str(p.relative_to(ROOT)).replace('\\','/'))
paths.add(str((VAL/'player03/Pilot-U0-540-center.png').relative_to(ROOT)).replace('\\','/'))
paths.update('Logs/leisure-animation-pilot-build'+n+'.log' for n in ['01','02','03'])
paths={p.replace('\\','/') for p in paths};assert all(not p.startswith(('Library/','Temp/','Builds/')) and '..' not in Path(p).parts for p in paths)
entries=[dict(path=p,sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(paths)]
size=sum(e['bytes'] for e in entries);print('Incremental estimate:',len(entries),'files,',round(size/1024**2,2),'MiB')
assert size<120*1024**2,'Unexpected size: inspect scope'
if not args.create:raise SystemExit(0)
assert not DEST.exists() and DEST.parent.resolve()==Path('D:/GAMEKKK/Backups').resolve()
DEST.mkdir()
for e in entries:
    p=ROOT/e['path'];q=DEST/e['path'];q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q)
    assert sha(p)==e['sha256'] and sha(q)==e['sha256'],'Concurrent change during checkpoint'
(DEST/'RESTORE.md').write_text('''# Incremental - NOT standalone

Restore CheckpointRoutePlayerCorrection-20261019 and its documented base chain,
then overlay this checkpoint at the recorded project-relative paths. No deleted paths.
Do not restore onto a running Editor/Player or overwrite newer unrelated work.
This restores only two upgraded pilot panels; the previous other-ten prototypes remain unchanged.
It also preserves the 12 newly delivered background PNGs/metas because they were absent
from the preceding route checkpoint. PNGs were never regenerated or altered.
No Library/Temp/Builds, global player saves or historical backup trees copied.
To reproduce Windows: LeisurePilotSetup.BindAndBuild for initial bind, or the existing
Phase5Setup.Build helper when bindings are already restored. This does not implement Phase5.
Complete current acceptance captures are under Validation/LeisureAnimationPilot-20261019.
Windows native 540x960 / 720x1280 targeted QA PASS. Android NOT_TESTED.
Animation visual is internal pilot review; project-owner approval is pending.
STOP after restoring/testing these two activities; do not start the other-ten rollout.
''',encoding='utf-8')
manifest=dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
    baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],baseChain=chain,entries=entries,deletedPaths=[],bytes=size,
    restoreGuideSHA256=sha(DEST/'RESTORE.md'),summary='Two real leisure pilots H9/U0; source prop mesh layers + fixed swing support + coordinated rider/boat; reuse two existing backgrounds. 199 Windows assertions + 9 restart assertions PASS; owner visual review pending.')
dump(DEST/'CHECKPOINT_MANIFEST.json',manifest)
info=dict(checkpoint=str(DEST),manifestSHA256=sha(DEST/'CHECKPOINT_MANIFEST.json'),baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],
    standalone=False,files=len(entries),bytes=size,windows='TARGETED_PASS',android='NOT_TESTED',visual='PILOT_INTERNAL_ANIMATION_PASS_OWNER_REVIEW_PENDING',stop='STOP_AFTER_TWO_PILOTS')
dump(DOC/'checkpoint.json',info);print(json.dumps(info,ensure_ascii=False))
