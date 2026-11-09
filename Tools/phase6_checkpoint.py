"""Small verified incremental checkpoint; no delete, scene rebuild or full-project scan."""
import argparse,hashlib,json,shutil
from datetime import datetime,timezone
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/QA/Phase6-VisualAndroid'
target=Path('D:/GAMEKKK/Backups/CheckpointPhase6VisualAndroid-20261019')
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
baseline=read(OUT/'baseline.json');base=Path(baseline['baseCheckpoint'])
assert digest(base/'CHECKPOINT_MANIFEST.json')==baseline['baseManifestSHA256']
for e in read(base/'CHECKPOINT_MANIFEST.json')['entries']:assert digest(base/e['path'])==e['sha256'],e['path']
assert read(OUT/'source-verification.json')['result']=='PASS'
summary=read(OUT/'run-summary.json')
for name in summary['required']:assert any(e['run']==name and e['result']=='PASS' for e in summary['runs'])
modified=read(OUT/'source-verification.json')['changed']+['Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md']
new=['Assets/_Game/Scripts/Editor/Phase6Setup.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Phase6.cs']
new+= [p+'.meta' for p in new]
new+= ['Tools/phase6_verify.py','Tools/phase6_report.py','Tools/phase6_validation.ps1','Tools/phase6_checkpoint.py','Docs/Gameplay/PHASE6_VISUAL_ANDROID_POLISH_REPORT.md']
(OUT/'changed-files.json').write_text(json.dumps(dict(modified=modified,added=new,deleted=[],note='QA report/baseline/evidence selection additionally recorded in checkpoint manifest'),indent=2),encoding='utf-8')
payload=set(modified+new)
payload.update(p.relative_to(ROOT).as_posix() for p in OUT.iterdir() if p.is_file() and p.name!='checkpoint.json')
payload.update(read(OUT/'acceptance-images.json'))
for folder in (ROOT/'Validation/Phase6-20261019').iterdir():
 for p in folder.iterdir():
  if p.is_file() and p.suffix in ['.log','.txt','.json']:payload.add(p.relative_to(ROOT).as_posix())
 if (folder/'smoke-results.txt').exists() and 'FAILED' in (folder/'smoke-results.txt').read_text(encoding='utf-8-sig'):
  payload.update(p.relative_to(ROOT).as_posix() for p in folder.glob('failure-*.png'))
payload.update('Logs/phase6-build'+f'{i:02}'+'.log' for i in range(1,7))
assert all((ROOT/p).is_file() for p in payload)
size=sum((ROOT/p).stat().st_size for p in payload)
print('Planned:',len(payload),'files;',round(size/1048576,2),'MiB; incremental; requires',base)
args=argparse.ArgumentParser();args.add_argument('--apply',action='store_true')
if not args.parse_args().apply:raise SystemExit(0)
assert not target.exists(),'Refuse overwrite'
assert target.resolve().parent==Path('D:/GAMEKKK/Backups').resolve()
chain=[];cursor=base;seen=set()
while cursor not in seen:
 seen.add(cursor);m=read(cursor/'CHECKPOINT_MANIFEST.json');chain.append(dict(path=str(cursor),manifestSHA256=digest(cursor/'CHECKPOINT_MANIFEST.json')))
 if not m.get('baseCheckpoint'):break
 parent=Path(m['baseCheckpoint'])
 if m.get('baseManifestSHA256'):assert digest(parent/'CHECKPOINT_MANIFEST.json')==m['baseManifestSHA256']
 cursor=parent
target.mkdir();entries=[]
for name in sorted(payload):
 p=ROOT/name;destination=target/name;destination.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,destination)
 sha=digest(p);assert digest(destination)==sha
 entries.append(dict(path=name,sha256=sha,bytes=p.stat().st_size))
guide='''# Phase6 incremental restore — NOT a standalone Unity project

Requires CheckpointAppliedSixRouteReferences-20261019, CheckpointPhase5Final-20261019
and their verified ancestor chain. Never overlay an older checkpoint onto newer live work.
Close Unity/Player. Build a separate recovery folder from the standalone ancestor;
apply incremental ancestors base-to-child using each manifest/deletedPaths. Verify
manifest hashes first, then file hashes. Overlay exactly entries from this checkpoint
and retain paired .meta files/GUIDs. No deleted paths in Phase6. Verify every SHA256.
The script verified latest base payload, ancestor manifest chain and Phase6 payload;
it did not rehash every historical backup file. See baseChain for exact dependencies.
No Library/Temp/cache/Builds included. Open recovery with Unity6000.6.5f1, rebuild
using Phase5Setup.Build (existing BuildPipeline only). Do not rerun art/audio/scene
generators. Phase6Setup is a bounded explicit repair utility, unnecessary for restore
because the repaired importer metadata is already included.
Validation JSON saves are private QA fixtures/earned test copies, never user saves.
Never copy them into normal persistentDataPath. Existing user saves unchanged.
Docs/QA/Phase6-VisualAndroid/checkpoint.json is an external receipt, excluded here to
avoid self-referential hash. Windows targeted QA passed; Android hardware/APK not
tested, three Seoul icons and unknown occlusion/terrain remain pending as reported.
STOP before Phase7. No old backup/save/evidence was removed.
'''
(target/'RESTORE.md').write_text(guide,encoding='utf-8')
manifest=dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.now(timezone.utc).isoformat(),baseCheckpoint=str(base),baseManifestSHA256=baseline['baseManifestSHA256'],baseChain=chain,entries=entries,deletedPaths=[],bytes=size,restoreGuideSHA256=digest(target/'RESTORE.md'),summary='Phase6:4 sprite outline cleanup,2 isolated-map mip/trilinear fixes,portrait target clamp; native4res Windows UI/touch/H6/battle/relic/ending/restart regression. Gameplay/art PNG/audio unchanged. Android untested; stop.')
(target/'CHECKPOINT_MANIFEST.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
receipt=dict(checkpoint=str(target),manifestSHA256=digest(target/'CHECKPOINT_MANIFEST.json'),baseCheckpoint=str(base),baseManifestSHA256=baseline['baseManifestSHA256'],standalone=False,files=len(entries),bytes=size,windows='WINDOWS_TARGETED_PASS',visual='VISUAL_PENDING',android='ANDROID_NOT_TESTED',stop='Phase6 complete at Windows scope; do not start Phase7')
(OUT/'checkpoint.json').write_text(json.dumps(receipt,indent=2),encoding='utf-8');print(json.dumps(receipt,indent=2))
