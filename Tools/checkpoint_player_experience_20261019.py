"""Compact additive checkpoint. No project cleanup, rollback or full backup."""
from pathlib import Path
import argparse,json,hashlib,shutil,datetime
R=Path(__file__).resolve().parents[1];D=R/'Docs/QA/PlayerExperience-20261019'
DEST=Path(r'D:\GAMEKKK\Backups\CheckpointPlayerExperienceHoiAnPilot-20261019')
BASE=Path(r'D:\GAMEKKK\Backups\CheckpointAndroidAppIcon-20261019')
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
args=argparse.ArgumentParser();args.add_argument('--apply',action='store_true');apply=args.parse_args().apply
assert BASE.is_relative_to(Path(r'D:\GAMEKKK\Backups'))
manifest=read(BASE/'CHECKPOINT_MANIFEST.json');basehash=sha(BASE/'CHECKPOINT_MANIFEST.json')
assert basehash=='c97b0173544fb00379b1fa9ef2fb958d9112e79b09c8d45b9dfa0973ab016c7c'
chain=[{'path':str(BASE),'manifestSHA256':basehash}]+manifest.get('baseChain',[])
for e in chain:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
delta=read(D/'changed-files.json');paths=set(delta['existing']+delta['newScripts']+delta['newScriptMetas']+delta['tools'])
paths|={'Tools/checkpoint_player_experience_20261019.py','Docs/QA/PLAYER_EXPERIENCE_HOI_AN_PILOT_20261019.md',
 'Tools/digitize_owner_routes_20261019.py','Tools/review_owner_routes_20261019.py'}
# Preserve the owner's unfinished route drawings/staging as evidence only.
for folder in [D,R/'Validation/OwnerRouteRedesign-20261019',R/'Docs/Maps/OwnerRouteRedesign-20261019']:
 for p in folder.rglob('*'):
  if p.is_file() and p.name not in ('checkpoint-plan.json','checkpoint-receipt.json'):paths.add(str(p.relative_to(R)))
for run in ['after-editor-logical04','lantern-editor02','after-720-main01','final-72001']:
 folder=R/'Validation/PlayerExperience-20261019'/run
 for p in folder.iterdir():
  if p.suffix=='.png' or p.name in ['smoke-results.txt','capture-manifest.txt','process.json','XP-earned-tutorial.json','XP-leisure-restart.json']:
   paths.add(str(p.relative_to(R)))
paths.add('Logs/player-experience-final-build.log')
entries=[]
for name in sorted(paths):
 p=R/name;assert p.resolve().is_relative_to(R) and p.is_file(),name
 assert p.relative_to(R).parts[0] not in ('Library','Temp','Builds','build'),name
 entries.append({'path':str(p.relative_to(R)).replace('\\','/'),'bytes':p.stat().st_size,'sha256':sha(p)})
plan={'destination':str(DEST),'base':str(BASE),'files':len(entries),'bytes':sum(e['bytes'] for e in entries),'MiB':round(sum(e['bytes'] for e in entries)/2**20,2),
 'standalone':False,'ownerRouteStagingIncluded':True,'fullBuildCacheOrHistoryCopied':False}
dump(D/'checkpoint-plan.json',plan);print(json.dumps(plan,indent=2))
if not apply:raise SystemExit(0)
assert not DEST.exists(),'Never overwrite an existing checkpoint'
DEST.mkdir()
for e in entries:
 p=DEST/e['path'];p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(R/e['path'],p);assert sha(p)==e['sha256'],e['path']
guide='''INCREMENTAL REQUIRES BASE — NOT A STANDALONE PROJECT

1. Restore CheckpointAndroidAppIcon-20261019 using its own full base chain.
2. Overlay only the files in CHECKPOINT_MANIFEST.json. Preserve .meta GUIDs.
3. Do not delete or replace unlisted files, saves, route drafts or backups.
4. OwnerRouteRedesign files are pending design evidence, NOT runtime maps/graph.
5. Source pilot is PARTIAL ACCEPTANCE: Windows build succeeded, Player execution
   blocked by Application Control; Editor tutorial/leisure evidence is scoped.
6. No APK or Windows build folder is bundled. Rebuild from source when needed.
7. Do not run historical patch tools blindly. Inspect current source and report.
8. All 133 nodes/155 existing edges/quests/save/relics/art/audio stay on the base.
   This patch changes presentation/UI only. Stop before further city rollout.
'''
(DEST/'RESTORE_INSTRUCTIONS.txt').write_text(guide,encoding='utf-8')
out={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),
 'baseCheckpoint':str(BASE),'baseManifestSHA256':basehash,'baseChain':chain,'entries':entries,
 'bytes':plan['bytes'],'deletedPaths':[],'restoreGuideSHA256':sha(DEST/'RESTORE_INSTRUCTIONS.txt'),
 'summary':'Hoi An player experience pilot, presentation-only source changes; scoped Play Mode PASS, Windows build PASS, Windows runtime BLOCKED, extended main QA incomplete. Pending owner route design preserved, not applied.'}
dump(DEST/'CHECKPOINT_MANIFEST.json',out)
receipt={**plan,'manifestSHA256':sha(DEST/'CHECKPOINT_MANIFEST.json'),'baseManifestChainVerified':True,'copiedFileHashesVerified':True,
 'status':'SOURCE_IMPLEMENTED_PARTIAL_ACCEPTANCE','windowsBuild':'PASS','windowsPlayer':'BLOCKED','android':'NOT_TESTED'}
dump(D/'checkpoint-receipt.json',receipt);print('CREATED AND VERIFIED',DEST)
