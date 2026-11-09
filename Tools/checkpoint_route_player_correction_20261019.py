"""Small incremental checkpoint. Explicit delta only; no cache/build/history copy."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil

ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/QA/RouteShorteningProposal-20261019'
VAL=ROOT/'Validation/RouteShorteningProposal-20261019'
DEST=Path('D:/GAMEKKK/Backups/CheckpointRoutePlayerCorrection-20261019')
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
parser=argparse.ArgumentParser();parser.add_argument('--create',action='store_true');args=parser.parse_args()
b=read(DOC/'baseline.json');base=Path(b['baseCheckpoint']);bm=read(base/'CHECKPOINT_MANIFEST.json')
assert sha(base/'CHECKPOINT_MANIFEST.json')==b['baseManifestSHA256']
chain=[dict(path=str(base),manifestSHA256=b['baseManifestSHA256']),*bm['baseChain']]
for c in chain:
    p=Path(c['path'])/'CHECKPOINT_MANIFEST.json'
    assert p.exists() and sha(p)==c['manifestSHA256'],str(p)

paths=set(b['liveDeltaFromCheckpoint'])
paths.update(['Assets/_Game/Resources/Content/GameplayContent.json',
              'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.ApprovedRoutes.cs'])
for folder in [DOC,ROOT/'Docs/QA/10Story12Leisure']:
    paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in folder.rglob('*') if p.is_file() and p.name!='checkpoint.json')
paths.update('Docs/QA/'+name for name in ['MASTER_ROUTE_AND_UX_AUDIT_20261019.md','POST_PHASE6_PLAYER_EXPERIENCE_FIX_20261019.md','RESTORATION_GUIDANCE_PROPOSAL_20261019.md','TEN_STORY_TWELVE_LEISURE_20261019.md'])
paths.update('Tools/'+name for name in ['route_ux_correction_20261019.py','verify_route_correction_20261019.py','route_correction_followup_20261019.ps1','finalize_route_player_review_20261019.py','checkpoint_route_player_correction_20261019.py','story_leisure_validation.ps1','story_leisure_suite.ps1','story_leisure_report.py'])
paths.add('Logs/route-correction-build01.log')
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in VAL.iterdir() if p.is_file() and p.suffix in ['.json','.csv','.html'])
# Representative visual evidence only. Full fresh galleries remain in the live
# Validation folder, indexed with hashes; this checkpoint primarily restores game source.
for name in ['SixCities-After-Contact.png','Ending-Contact.png','HoiAn_Current_Proposed.png','Hue_Current_Proposed.png',
             'Runtime-H8Frames01/Move-hoi_an-2.png','Runtime-After02/H6-covered.png',
             'Runtime-Ending-Restart01/P5-WorldMap-after-credits.png']:
    paths.add(str((VAL/name).relative_to(ROOT)).replace('\\','/'))
run_names=['Runtime-Before01','Runtime-After01','Runtime-After02','Runtime-AfterOverview01','Runtime-Fresh01','Runtime-H8Frames01',
           *['Runtime-Restart-'+c+'-01' for c in ['hoi_an','hue','da_lat','jeonju','gyeongju','seoul']],
           'Runtime-Ending-Current','Runtime-Ending-Current02','Runtime-Ending-Restart01']
evidence=[]
for name in run_names:
    folder=VAL/name
    for p in folder.iterdir():
        if not p.is_file():continue
        rel=str(p.relative_to(ROOT)).replace('\\','/')
        evidence.append(dict(path=rel,bytes=p.stat().st_size,sha256=sha(p)))
        if p.suffix in ['.json','.txt','.log']:paths.add(rel)
dump(DOC/'evidence-index.json',dict(scope='Exact current-run evidence only; full image set stays in the working Validation folder. Representative images included in checkpoint.',entries=evidence))
paths.add(str((DOC/'evidence-index.json').relative_to(ROOT)).replace('\\','/'))
paths={p.replace('\\','/') for p in paths}
assert all(not p.startswith(('Library/','Temp/','Builds/')) and '..' not in Path(p).parts for p in paths)
assert all((ROOT/p).is_file() for p in paths)
entries=[dict(path=p,bytes=(ROOT/p).stat().st_size,sha256=sha(ROOT/p)) for p in sorted(paths)]
size=sum(e['bytes'] for e in entries)
print('Estimated incremental checkpoint:',len(entries),'files,',round(size/1024**2,2),'MiB')
assert size<150*1024**2,'Unexpected scope/size; inspect before copying'
if not args.create:raise SystemExit(0)
assert DEST.parent.resolve()==Path('D:/GAMEKKK/Backups').resolve()
assert not DEST.exists(),'Never overwrite an existing checkpoint'
assert read(DOC/'preservation-verification.json')['result']=='PASS'
assert read(DOC/'run-summary.json')['windows']=='TARGETED_PASS'
DEST.mkdir()
for e in entries:
    p=ROOT/e['path'];q=DEST/e['path'];q.parent.mkdir(parents=True,exist_ok=True)
    shutil.copy2(p,q);assert sha(q)==e['sha256'] and sha(p)==e['sha256'],'Concurrent source change'
guide='''# Incremental restore - requires its base chain

This is NOT a standalone/full Unity project backup.
Restore CheckpointPhase6VisualAndroid-20261019 and its documented bases first,
then overlay the entries of this checkpoint at the same project-relative paths.
No deleted paths. Do not restore onto an active Unity/Player process.
This contains the pre-existing authorized 10+12 implementation and the newer
27-edge waypoint correction. Do not replace that implementation with Phase6 source.

Library/Temp/Builds and historical backups/saves were not copied or modified.
Rebuild Windows with the existing Phase5Setup.Build helper if a Player is needed.
The helper name does not mean Phase5 gameplay was executed/changed by this task.

Full fresh QA images remain under Validation/RouteShorteningProposal-20261019 in
the working project. evidence-index.json records their hashes; only representative
images are included here. Preserve that live folder for the complete visual gallery.
Movement/source integrity PASS; remaining terrain issues are PARTIAL and documented.
Android physical-device testing NOT_TESTED.
'''
(DEST/'RESTORE.md').write_text(guide,encoding='utf-8')
manifest=dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),
    baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],baseChain=chain,entries=entries,deletedPaths=[],bytes=size,
    restoreGuideSHA256=sha(DEST/'RESTORE.md'),summary='Preserve prior 10+12 working implementation; apply 27 existing edge waypoint lists only; targeted Windows 68 traversals + HIDDEN + six real-process restarts PASS; nine terrain routes retained separately.')
dump(DEST/'CHECKPOINT_MANIFEST.json',manifest)
info=dict(checkpoint=str(DEST),manifestSHA256=sha(DEST/'CHECKPOINT_MANIFEST.json'),baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],
          standalone=False,files=len(entries),bytes=size,windows='TARGETED_PASS',terrain='PARTIAL_BEST_EFFORT',android='NOT_TESTED',stop='STOP; no next phase')
dump(DOC/'checkpoint.json',info)
print(json.dumps(info,ensure_ascii=False))
