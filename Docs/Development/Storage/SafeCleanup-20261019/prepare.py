"""Prepare a literal two-folder Gradle-intermediates cleanup plan; no deletion."""
from pathlib import Path
import json, hashlib, datetime, collections
R=Path(r'D:\GAMEKKK\LightBetweenCultures'); O=R/'Docs/Storage/SafeCleanup-20261019'
before=json.loads((O/'before.json').read_text(encoding='utf-8'))
def sha(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for b in iter(lambda:f.read(1024*1024),b''):h.update(b)
 return h.hexdigest()
targets=[R/'Library/Bee/Android/Prj/IL2CPP/Gradle'/n/'build/intermediates' for n in ('unityLibrary','launcher')]
records=[]
for t in targets:
 files=[]
 for f in t.rglob('*'):
  assert not f.is_symlink() and not (f.stat().st_file_attributes & 1024),str(f)
  if f.is_file():
   assert f.suffix.lower() not in ('.apk','.aab','.keystore','.jks'),str(f)
   s=f.stat();files.append({'path':str(f),'bytes':s.st_size,'mtimeTicks':621355968000000000+s.st_mtime_ns//100})
 records.append({'path':str(t),'bytes':sum(x['bytes'] for x in files),'files':files,
                 'reason':'Completed Gradle build intermediates, regenerated from retained Gradle source and Unity/Bee build inputs.'})
guards=[]
for rel in ['Builds/Android/LightBetweenCultures-Test.apk',
 'Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build/outputs/apk/release/launcher-release.apk',
 'Validation/AndroidApk-20261019/android-home/debug.keystore',
 'ProjectSettings/ProjectSettings.asset','Packages/manifest.json','Packages/packages-lock.json',
 'Tools/SixCityRouteEditor/data/source.json','Validation/RouteMarkupExport-20261018/edge-id-registry.json']:
 p=R/rel; guards.append({'path':str(p),'bytes':p.stat().st_size,'sha256':sha(p)})
apk=guards[0];assert apk['sha256']=='ecc062ade3c1967a2d184e555a38098b3269a6b4de602aa1c3c66b1f523b5481'
assert guards[0]['sha256']==guards[1]['sha256']
latest=Path(r'D:\GAMEKKK\Backups\CheckpointAndroidAppIcon-20261019\CHECKPOINT_MANIFEST.json')
m=json.loads(latest.read_text(encoding='utf-8-sig'))
chain=[latest.parent]+[Path(x['path']) for x in m.get('baseChain',[])]
for p in chain:
 q=p/'CHECKPOINT_MANIFEST.json'
 if q.exists():guards.append({'path':str(q),'bytes':q.stat().st_size,'sha256':sha(q)})
plan={'created':datetime.datetime.now().astimezone().isoformat(),'targets':records,'totalBytes':sum(x['bytes'] for x in records),'guards':guards}
(O/'cleanup-plan.json').write_text(json.dumps(plan,ensure_ascii=False,indent=2),encoding='utf-8')
backuprows=[];full=[]
for d in before['backups']['directories']:
 p=Path(d['path'])
 if p.parent!=Path(before['backups']['root']):continue
 q=p/'CHECKPOINT_MANIFEST.json'; x={}
 if q.exists():
  try:x=json.loads(q.read_text(encoding='utf-8-sig'))
  except Exception as e:x={'readError':str(e)}
 row={'path':str(p),'bytes':d['bytes'],'files':d['files'],'manifestExists':q.exists(),
      'type':x.get('type','UNSPECIFIED' if q.exists() else 'NO_ROOT_MANIFEST'),
      'latestRestoreChain':p in chain,'decision':'KEEP_REQUIRES_OWNER_APPROVAL'}
 backuprows.append(row)
 if str(row['type']).startswith('FULL_SOURCE_BUILD'):
  ents=[e for e in x.get('entries',[]) if isinstance(e,dict) and e.get('sha256') and 'bytes' in e]
  full.append((p,ents))
# Quantify content overlap from recorded manifest hashes only. This is not a
# proof that any checkpoint is disposable; unique historical files still matter.
seen={};overlap=0;occurrences=0
for p,es in full:
 for e in es:
  k=(e['sha256'],e['bytes'])
  if k in seen:overlap+=e['bytes'];occurrences+=1
  else:seen[k]=str(p)
catalog={'backups':backuprows,'latestChainPaths':[str(p) for p in chain],
 'manifestOverlapEstimate':{'fullSnapshotCount':len(full),'duplicateOccurrenceCount':occurrences,
 'duplicateBytes':overlap,'method':'Existing manifest SHA256+bytes only; not rehashed, not an authorization or proof of safe whole-checkpoint deletion.'}}
(O/'backup-catalog.json').write_text(json.dumps(catalog,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'deleteBytes':plan['totalBytes'],'deleteGiB':round(plan['totalBytes']/2**30,3),
 'fileCount':sum(len(x['files']) for x in records),'guardCount':len(guards),'latestChainLength':len(chain),
 'backupCount':len(backuprows),'fullSnapshotOverlapBytes':overlap},indent=2))
