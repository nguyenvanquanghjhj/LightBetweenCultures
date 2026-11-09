"""Compact incremental checkpoint. No cache/build copy, no deletions, no rollback."""
from pathlib import Path
import json, hashlib, shutil, datetime, re

R=Path(__file__).resolve().parents[1]; Q=R/'Docs/QA/Phase7'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,data):p.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

base=read(Q/'baseline.json'); B=Path(base['baseCheckpoint']); M=B/'CHECKPOINT_MANIFEST.json'
assert sha(M)==base['baseManifestSHA256']
bm=read(M)
for e in bm['entries']:
    p=B/e['path'];assert p.is_file() and sha(p)==e['sha256'], 'Base content failed: '+str(p)
for c in bm['baseChain']:
    assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256'], 'Base-chain manifest mismatch'

target=Path('D:/GAMEKKK/Backups/CheckpointPhase7Final-20261019')
assert not target.exists(), 'Never overwrite a checkpoint'
files=set()
for e in read(Q/'final-protected-diff.json')['changed']:files.add(e['path'])
for parent,glob in [('Assets/_Game/Scripts/Editor','Phase7*'),('Assets/_Game/Scripts/Validation','GameplaySmokeRunner.Phase7*'),('Assets/_Game/Art/Characters/Enemies/CleanDerived','*'),('Tools','phase7_*')]:
    for p in (R/parent).glob(glob):
        if p.is_file():files.add(p.relative_to(R).as_posix())
foldermeta=R/'Assets/_Game/Art/Characters/Enemies/CleanDerived.meta'
if foldermeta.exists():files.add(foldermeta.relative_to(R).as_posix())
for p in Q.iterdir():
    if p.is_file() and p.name!='checkpoint-receipt.json':files.add(p.relative_to(R).as_posix())
for e in read(Q/'gallery-index.json'):files.add(e['path'])
for folder in (R/'Validation/Phase7-20261019').iterdir():
    if not folder.is_dir():continue
    for p in folder.iterdir():
        if p.is_file() and p.suffix in ['.txt','.log','.json'] and p.stat().st_size<12*1048576:files.add(p.relative_to(R).as_posix())
for name in ['phase7-bot-portrait-build01.log','phase7-final-readonly-audit.log','phase7-android-native-audit02.log']:
    files.add('Logs/'+name)
# Selected compatible proof used by the reports, never entire historical trees.
for name in ['smoke-results.txt','full-story-final.json']:
    files.add('Validation/FullStory-20261017/Fresh04/'+name)
for name in ['Docs/QA/FOUR_TRACK_MUSIC_INTEGRATION.md','Docs/QA/SOFT_UI_SFX_INTEGRATION.md','Docs/QA/FourTrackMusic-20261018/verification.json','Docs/QA/Phase6-VisualAndroid/run-summary.json']:
    files.add(name)
for run in ['main02','battle01','restore01']:
    files.add(f'Validation/Phase6-20261019/{run}/smoke-results.txt')
assert all((R/p).is_file() for p in files)
assert not any('/Library/' in p or p.startswith(('Library/','Temp/','Builds/')) for p in files)
estimate=sum((R/p).stat().st_size for p in files)
print('Estimated bytes',estimate,'MiB',round(estimate/1048576,2),'files',len(files))
assert estimate<240*1048576,'Unexpected checkpoint growth; review instead of copying'
entries=[dict(path=p,sha256=sha(R/p),bytes=(R/p).stat().st_size) for p in sorted(files)]
target.mkdir()
for e in entries:
    dest=target/e['path'];dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(R/e['path'],dest)
    assert sha(dest)==e['sha256'] and sha(R/e['path'])==e['sha256'], 'Concurrent content change detected'
guide='''# Incremental Phase7 restore point — NOT standalone

Requires CheckpointFinalPolishRecovery-20261019 and its full verified baseChain.
Restore the base chain oldest to newest into a separate recovery directory, then
overlay the entries in this checkpoint preserving paths and .meta files.
Verify each overlay entry against CHECKPOINT_MANIFEST.json SHA256.
Do not overwrite a newer live workspace blindly. No deleted paths.

Contains scoped dialogue portrait repair, four original-pixel derivatives,
QA/editor tools, audit reports, representative native Windows gallery, logs and
private reproducibility saves. It deliberately excludes Build/Library/Temp/cache
and unrelated historical Validation. Rebuild Windows from restored source if needed.

Status AUDIT_INCOMPLETE: Windows functional coverage largely passes; Android
build/device and current higher-resolution/minigame visual acceptance remain
blocked/not tested. See RELEASE_READINESS.md. No asset cleanup occurred.
'''
(target/'RESTORE.md').write_text(guide,encoding='utf-8')
manifest=dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseCheckpoint=str(B),baseManifestSHA256=sha(M),baseChain=[dict(path=str(B),manifestSHA256=sha(M))]+bm['baseChain'],entries=entries,bytes=estimate,deletedPaths=[],restoreGuideSHA256=sha(target/'RESTORE.md'),summary='Phase7 audited source + dialogue-only bot alpha repair; preserve original maps/combat/gameplay; AUDIT_INCOMPLETE with explicit platform/visual limits; zero cleanup')
write(target/'CHECKPOINT_MANIFEST.json',manifest)
receipt=dict(checkpoint=str(target),manifestSHA256=sha(target/'CHECKPOINT_MANIFEST.json'),standalone=False,baseCheckpoint=str(B),baseManifestSHA256=sha(M),files=len(entries),bytes=estimate,MiB=round(estimate/1048576,2),copiedFilesVerified=True,baseContentVerified=True,baseChainManifestVerified=True,deletedPaths=[],status='AUDIT_INCOMPLETE',stop='STOP_WAITING_FOR_OWNER_REVIEW')
write(Q/'checkpoint-receipt.json',receipt)
print(json.dumps(receipt,indent=2))
