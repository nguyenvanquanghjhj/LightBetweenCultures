"""Compact leisure delta and current-task evidence; requires the verified pilot base."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QA/LeisureActivitiesFullReview-20261019';VAL=ROOT/'Validation/LeisureActivitiesFullReview-20261019'
DEST=Path('D:/GAMEKKK/Backups/CheckpointLeisureActivitiesFullReview-20261019')
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
parser=argparse.ArgumentParser();parser.add_argument('--create',action='store_true');args=parser.parse_args()
b=read(DOC/'baseline.json');preserve=read(DOC/'preservation.json');qa=read(DOC/'run-summary.json')
assert preserve['result']=='PASS' and qa['windows']=='TARGETED_PASS'
base=Path(b['baseCheckpoint']);assert sha(base/'CHECKPOINT_MANIFEST.json')==b['baseManifestSHA256']
bm=read(base/'CHECKPOINT_MANIFEST.json');chain=[dict(path=str(base),manifestSHA256=b['baseManifestSHA256']),*bm['baseChain']]
for c in chain:assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256']
paths=set(preserve['changedExistingFiles'])
for stem in ['Assets/_Game/Scripts/Editor/LeisureFullSetup.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.LeisureFull.cs']:
    paths.update([stem,stem+'.meta'])
paths.update('Tools/'+n for n in ['leisure_full_validation.ps1','verify_leisure_full.py','finalize_leisure_full.py','checkpoint_leisure_full.py'])
paths.add('Docs/QA/LEISURE_ACTIVITIES_FULL_REVIEW_20261019.md')
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in DOC.iterdir() if p.is_file() and p.name!='checkpoint.json')
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in VAL.iterdir() if p.is_file())
# Final acceptance images plus small logs; older trial PNGs stay in the project.
for run in ['player01','player02','player03','player04','restart01']:
    paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in (VAL/run).iterdir() if p.is_file() and p.suffix.lower()!='.png')
for e in read(DOC/'evidence.json')['entries']:
    paths.add(str((VAL/e['run']/e['file']).relative_to(ROOT)).replace('\\','/'))
for name in ['FULL-J13-720-motion-2.3.png','FULL-G11-720-motion-2.3.png','FULL-H16-540-motion-0.6.png']:
    paths.add(str((VAL/'player01'/name).relative_to(ROOT)).replace('\\','/'))
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in (ROOT/'Logs').glob('leisure-full-build*.log'))
paths={p.replace('\\','/') for p in paths};assert all(not p.startswith(('Builds/','Temp/','Library/')) and '..' not in Path(p).parts for p in paths)
entries=[dict(path=p,sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(paths)]
size=sum(e['bytes'] for e in entries);print('Incremental estimate:',len(entries),'files,',round(size/1024**2,2),'MiB')
assert size<180*1024**2,'Unexpected scope: inspect before copying'
if not args.create:raise SystemExit(0)
assert not DEST.exists() and DEST.parent.resolve()==Path('D:/GAMEKKK/Backups').resolve()
DEST.mkdir()
for e in entries:
    p=ROOT/e['path'];q=DEST/e['path'];q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q)
    assert sha(p)==e['sha256'] and sha(q)==e['sha256'],'Concurrent change during incremental checkpoint'
(DEST/'RESTORE.md').write_text('''# Incremental checkpoint — NOT standalone

Restore CheckpointLeisureAnimationPilot-20261019 and its manifest base chain,
then overlay these project-relative files. No deleted paths. Do not overwrite newer
unrelated work or restore while Editor/Player is active. No Library/Temp/Builds,
global saves or old evidence trees copied. The gallery's 120 final native PNGs,
24 motion strips, run logs/saves and three before-fix diagnostics are included.
Unselected trial PNGs remain in the original live project; historical raw frame
lists may reference them. Final evidence.json and all gallery links are complete.
Original 12 background PNGs/prop PNGs
already exist in the base chain and were not duplicated or altered here.

Ten remaining leisure panels composed/animated from approved existing art.
The two previous pilot implementations and their backdrop imports are byte-exact.
All 12 activities reviewed from real Windows Player at 540x960 and 720x1280,
with bounded visual rechecks and a separate process restart. Read the QA report
and visual-review.json: a functional PASS does not certify a dedicated seated,
reaching or dance pose. Owner visual acceptance is pending. Android NOT_TESTED.

To reproduce: existing Phase5Setup.Build uses the restored background references;
LeisureFullSetup.BindAndBuild can rebuild those ten bindings from existing PNGs.
Its build helper name does not authorize or implement story Phase5.

STOP after reviewing this leisure system. No story minigame/route/relic/ending work.
''',encoding='utf-8')
manifest=dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],baseChain=chain,entries=entries,bytes=size,deletedPaths=[],restoreGuideSHA256=sha(DEST/'RESTORE.md'),summary='Ten remaining playable leisure animations + review of all12; no reward/history/save changes; two existing pilots retained byte-exact. Windows functional QA passed; individual visual classifications and art needs recorded; owner review pending.')
dump(DEST/'CHECKPOINT_MANIFEST.json',manifest)
info=dict(checkpoint=str(DEST),manifestSHA256=sha(DEST/'CHECKPOINT_MANIFEST.json'),baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],standalone=False,files=len(entries),bytes=size,windows=qa['windows'],android='NOT_TESTED',visual=qa['visualCounts'],stop='STOP_WAITING_FOR_OWNER_LEISURE_REVIEW')
dump(DOC/'checkpoint.json',info);print(json.dumps(info,ensure_ascii=False))
