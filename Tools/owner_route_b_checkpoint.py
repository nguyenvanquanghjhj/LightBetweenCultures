"""Compact, explicit Phase B snapshot. Default estimates; --apply copies and verifies."""
from pathlib import Path
import json, hashlib, shutil, sys, re
from datetime import datetime, timezone
R=Path(__file__).resolve().parents[1]
D=R/'Docs/Maps/OwnerRoutePhaseB-20261019'
V=R/'Validation/OwnerRoutePhaseB-20261019'
B=Path('D:/GAMEKKK/Backups/BeforeOwnerRoutesPhaseB-20261019')
T=Path('D:/GAMEKKK/Backups/CheckpointOwnerRoutesPhaseB-20261019')
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
assert read(D/'verification.json')['pass']
m=read(B/'CHECKPOINT_MANIFEST.json')
assert sha(B/'CHECKPOINT_MANIFEST.json')==read(D/'pre-change-checkpoint.json')['manifestSHA256']
for e in m['entries']:assert sha(B/e['path'])==e['sha256'],e['path']
for x in m['baseChain']:assert sha(Path(x['path'])/'CHECKPOINT_MANIFEST.json')==x['manifestSHA256'],x['path']
paths=set()
def add(p):
 p=Path(p)
 if not p.is_absolute():p=R/p
 assert p.is_file(),str(p)
 paths.add(p)
for f in ['Core/GameData.cs','Core/CityNavigation.cs','Maps/CityMap.cs','Maps/CityMapPaths.cs','Maps/CityRouteGeometry.cs','Validation/GameplaySmokeRunner.cs','Validation/GameplaySmokeRunner.Terrain.cs','Validation/GameplaySmokeRunner.OwnerRoutesB.cs','Editor/OwnerRouteAndroidBuild.cs']:
 p=R/'Assets/_Game/Scripts'/f;add(p);add(str(p)+'.meta')
for f in ['Assets/_Game/Resources/Content/CityNavigation.json','Assets/_Game/Resources/Content/CityNavigation.json.meta','Tools/owner_route_phase_b.py','Tools/owner_route_b_delivery.py','Tools/owner_route_b_checkpoint.py']:add(f)
for p in D.iterdir():
 if p.is_file()and p.name!='final-checkpoint.json':add(p)
# All gallery references must exist and be included, preserving their relative paths.
for href in re.findall(r'(?:src|href)="([^"]+)"',(D/'gallery.html').read_text(encoding='utf-8')):
 if '://'not in href and not href.startswith('#'):add((D/href).resolve())
for p in V.glob('*-implementation.json'):add(p)
for name in ['runtime02','windows720','junctions720','story-vietnam','story-korea','story-vietnam-seoul','story-korea-seoul','story-vietnam-seoul02','story-korea-seoul02','mid-boat-close','mid-boat-restart','ending-restart']:
 folder=V/name
 for p in folder.glob('*.json'):add(p)
 for filename in ['smoke-results.txt','player.log','output.log']:
  if(folder/filename).is_file():add(folder/filename)
# Reproducible compiler inputs; original art is not rewritten.
stage=R/'Validation/OwnerRouteRedesign-20261019'
for p in stage.glob('*-ink-strokes.json'):add(p)
for p in (stage/'inputs').glob('*.png'):add(p)
for filename in ['design-questions.json','node-access-review.json']:
 if(stage/filename).is_file():add(stage/filename)
for name in ['owner-route-b-final-build.log','owner-route-b-android.log']:
 add(R/'Logs'/name)
size=sum(p.stat().st_size for p in paths)
print(json.dumps({'files':len(paths),'bytes':size,'MiB':round(size/1048576,2),'target':str(T),'basePayloadVerified':True,'baseChainManifestsVerified':len(m['baseChain'])},indent=2))
if '--apply'not in sys.argv:sys.exit(0)
assert not T.exists(),'Refuse overwrite of existing checkpoint'
assert T.resolve().parent==Path('D:/GAMEKKK/Backups').resolve()
T.mkdir()
entries=[]
for p in sorted(paths):
 rel=p.relative_to(R);dest=T/rel;dest.parent.mkdir(parents=True,exist_ok=True)
 digest=sha(p);shutil.copy2(p,dest)
 assert sha(dest)==digest,str(rel)
 entries.append({'path':rel.as_posix(),'sha256':digest,'bytes':p.stat().st_size})
restore='''# Incremental checkpoint — REQUIRES BASE

This is NOT a standalone Unity project. Restore the verified full base and every incremental dependency in order, ending with BeforeOwnerRoutesPhaseB-20261019, then overlay this checkpoint's manifest entries at project-relative paths.

Close Unity/Player first. Do not blindly overwrite a live project with later work: restore to a separate owner-approved workspace or compare changed files first. No deleted paths. No Assets art, Packages or ProjectSettings changes in this increment. Library, Temp and Builds are excluded.

Production entry: Assets/_Game/Resources/Content/CityNavigation.json and the listed navigation source. Legacy 155-edge Content remains inherited and inactive. Gameplay saves are inherited unchanged; Validation saves included here are isolated QA evidence, not replacements for player saves.

Windows/Android build artifacts remain in the live Builds folder; see Docs/Maps/OwnerRoutePhaseB-20261019/android-apk-file.json for APK SHA and location. Rebuild from restored source if needed. Only selected current-task evidence is included; other historical/failure captures remain in the live Validation folder.

Verify each payload SHA256 against CHECKPOINT_MANIFEST.json before restoring. Base manifests and direct base payload were verified before writing this checkpoint.
'''
(T/'RESTORE.md').write_text(restore,encoding='utf-8')
manifest={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.now(timezone.utc).isoformat(),'baseCheckpoint':str(B),'baseManifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json'),'baseChain':[{'path':str(B),'manifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json')}]+m['baseChain'],'entries':entries,'bytes':size,'deletedPaths':[],'restoreGuideSHA256':sha(T/'RESTORE.md'),'summary':'Owner-drawn routes active in all six CityMaps, shared Dijkstra and walk/ferry movement, preserved canonical content/art/audio. Targeted Windows story/junction/boat/restart evidence and Android build receipt; no build binaries or Unity cache.'}
dump(T/'CHECKPOINT_MANIFEST.json',manifest)
receipt={'path':str(T),'manifestSHA256':sha(T/'CHECKPOINT_MANIFEST.json'),'bytes':size,'files':len(entries),'baseVerified':True,'payloadVerified':True,'standalone':False,'requiresBase':str(B),'createdUTC':manifest['createdUTC']}
dump(D/'final-checkpoint.json',receipt)
print(json.dumps(receipt,indent=2))
