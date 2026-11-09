"""Package only Android build tooling and evidence over the verified Phase7 base."""
from pathlib import Path
import json,hashlib,shutil,datetime
R=Path(__file__).resolve().parents[1];Q=R/'Docs/QA/AndroidApk-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
base=read(R/'Docs/QA/Phase7/checkpoint-receipt.json');B=Path(base['checkpoint']);M=B/'CHECKPOINT_MANIFEST.json'
assert sha(M)==base['manifestSHA256']
bm=read(M)
for e in bm['entries']:assert sha(B/e['path'])==e['sha256'],e['path']
for e in bm['baseChain']:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
assert read(Q/'protected-source-verification.json')['changed']==[]
verified=read(Q/'verified-apk.json');assert verified['result']=='PASS'
T=Path('D:/GAMEKKK/Backups/CheckpointAndroidApkBuild-20261019');assert not T.exists(),'Never replace an existing checkpoint'
files={'Assets/_Game/Scripts/Editor/AndroidApkBuild.cs','Assets/_Game/Scripts/Editor/AndroidApkBuild.cs.meta','Tools/verify_android_apk.ps1','Tools/android_apk_closeout.py','Tools/android_apk_checkpoint.py','Docs/QA/ANDROID_APK_BUILD_20261019.md','Logs/android-apk-20261019-build01.log','Logs/android-apk-20261019-build02.log'}
files.update(p.relative_to(R).as_posix() for p in Q.iterdir() if p.is_file() and p.name!='checkpoint-receipt.json')
entries=[{'path':p,'sha256':sha(R/p),'bytes':(R/p).stat().st_size} for p in sorted(files)]
total=sum(e['bytes'] for e in entries)
print('Incremental estimate:',len(entries),'files',round(total/1048576,2),'MiB; APK/cache/build outputs excluded')
assert total<16*1048576,'Unexpected size; review before copying'
T.mkdir()
for e in entries:
 d=T/e['path'];d.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(R/e['path'],d)
 assert sha(d)==e['sha256'] and sha(R/e['path'])==e['sha256']
(T/'RESTORE.md').write_text('''# Incremental Android APK build checkpoint — NOT standalone

Requires CheckpointPhase7Final-20261019 and its complete baseChain.
Restore its bases oldest to newest into a separate recovery directory, then
overlay this manifest's entries, keeping .meta files. Verify SHA256 first.
No deletions. No production game source/assets/settings changed in this task.
Only new Editor packaging tools, scripts, logs and verification reports.

The APK is delivered at Builds/Android/LightBetweenCultures-Test.apk in the
live project and is NOT duplicated into this compact checkpoint. Its measured
size and SHA256 are in verified-apk.json; rebuild using AndroidApkBuild.Build
if restoring source. No private signing key is backed up by this checkpoint.

Build PASS; ANDROID_DEVICE_NOT_TESTED. No phone was attached via ADB.
No Windows rebuild, cache deletion or Phase7/content work performed.
''',encoding='utf-8')
manifest={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(B),'baseManifestSHA256':sha(M),'baseChain':[{'path':str(B),'manifestSHA256':sha(M)}]+bm['baseChain'],'entries':entries,'bytes':total,'deletedPaths':[],'restoreGuideSHA256':sha(T/'RESTORE.md'),'externalArtifact':verified,'summary':'Successful verified debug-signed ARM64 IL2CPP Android APK; zero production source/settings delta; physical device NOT_TESTED'}
write(T/'CHECKPOINT_MANIFEST.json',manifest)
receipt={'checkpoint':str(T),'manifestSHA256':sha(T/'CHECKPOINT_MANIFEST.json'),'standalone':False,'baseCheckpoint':str(B),'baseManifestSHA256':sha(M),'files':len(entries),'bytes':total,'MiB':round(total/1048576,2),'baseContentVerified':True,'baseChainManifestVerified':True,'copiedFilesVerified':True,'deletedPaths':[],'APKDuplicated':False,'stop':'STOP_WAITING_FOR_OWNER_DEVICE_TEST'}
write(Q/'checkpoint-receipt.json',receipt)
print(json.dumps(receipt,indent=2))
