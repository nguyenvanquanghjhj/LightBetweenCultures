"""Verify the restore chain, estimate and optionally create a small artwork delta."""
from pathlib import Path
import argparse,datetime,hashlib,json,shutil
ROOT=Path(__file__).resolve().parents[1];BACKUP=ROOT.parent/'Backups';VAL=ROOT/'Validation/NpcAppearance-20261021';DEST=BACKUP/'CheckpointAppearance-20261021'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,d:p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
create=argparse.ArgumentParser();create.add_argument('--create',action='store_true');args=create.parse_args()
chain=[
('LBC_SourceVerified-20261020','SOURCE_MANIFEST.json','53305d534a5d5c5860003f6526d730cd412a0294a1f8c7b52831b74a54ace7a6'),
('CheckpointTouchNpcDialogue-20261020','INCREMENTAL_MANIFEST.json','b2f63c5eb392cfc4ac9d1793546611557d4520aa4ef132e0291c6f68d3064456'),
('CheckpointNpcApproachButtons-20261020','INCREMENTAL_MANIFEST.json','5c9009f720f8cd601ca1652eec52960f4e06f7dc7b7a440a795762f641f0ecb1'),
('CheckpointGelButtons-20261020','INCREMENTAL_MANIFEST.json','6b6597a57aa41e790517d7509a8a8e0f00d7f9d0498fb35180db2091e856e837'),
('CheckpointJourneyShop-20261021','INCREMENTAL_MANIFEST.json','bdf5fa12a77865bbbadbc74563cf543522395b6f73406842f0ebf0aa32e0a1c9')]
effective={}
for name,manifest,digest in chain:
 folder=BACKUP/name;assert sha(folder/manifest)==digest,folder
 for e in read(folder/manifest)['entries']:
  assert sha(folder/e['path'])==e['sha256'],e['path']
  effective[e['path'].removeprefix('Project/')]=e['sha256']
 for rel in read(folder/manifest).get('deletedPaths',[]):effective.pop(rel.removeprefix('Project/'),None)
current={p.relative_to(ROOT).as_posix():p for folder in ('Assets','Packages','ProjectSettings') for p in (ROOT/folder).rglob('*') if p.is_file()}
changed={rel for rel,p in current.items() if sha(p)!=effective.get(rel)}
deleted=[rel for rel in effective if rel.split('/')[0] in ('Assets','Packages','ProjectSettings') and rel not in current]
assert not deleted,deleted
summary=read(VAL/'delivery-summary.json');assert summary['windows']=='FOCUSED_RUNTIME_PASS'
assert (VAL/'final01/restart-results.txt').read_text(encoding='utf-8-sig').rstrip().endswith('ALL CHECKS PASSED')
assert not (ROOT/'Assets/Resources/AppearanceReviewData.asset').exists(),'Temporary QA fixture still in source'
paths=changed|{'Tools/'+name for name in ('art_asset_catalog.py','appearance_revision_audit.py','appearance_delivery.py','checkpoint_appearance_revision.py')}
paths|={'Docs/Art/'+name for name in ('LUMI_CANONICAL_20261021.md','NPC_CANONICAL_REGISTRY.json','NPC_CANONICAL_REGISTRY.md','native-art-bindings.json','ART_ASSET_CATALOG.md','ART_SOURCE_INDEX.md','art-asset-manifest.json','art-asset-manifest.csv','DESIGN_KIT_INTEGRATION_VI.md','IdentityPolicy-20261017/VisualReview.html')}
paths.add('Docs/QA/APPEARANCE_REVISION_20261021.md')
paths|={p.relative_to(ROOT).as_posix() for name in ('before01','final01') for p in (VAL/name).iterdir() if p.name in ('restart-results.txt','capture-manifest.txt','gameplay-save.json') or p.name.startswith('ART-') and p.suffix=='.png'}
paths|={p.relative_to(ROOT).as_posix() for p in VAL.iterdir() if p.suffix in ('.json','.html','.txt') and p.name!='checkpoint-plan.json'}
paths={rel for rel in paths if (ROOT/rel).is_file()};size=sum((ROOT/rel).stat().st_size for rel in paths)
plan=dict(standalone=False,requiresBase=str(BACKUP/chain[-1][0]),baseChainVerified=True,files=len(paths),bytes=size,MiB=round(size/1048576,2),changedSource=sorted(changed),deletedPaths=deleted)
dump(VAL/'checkpoint-plan.json',plan);print(json.dumps(plan))
if not args.create:raise SystemExit
assert not DEST.exists(),'Checkpoint exists; do not overwrite it'
assert size<200*1048576,'Unexpected checkpoint growth; inspect before copying'
DEST.mkdir();entries=[]
for rel in sorted(paths):
 target=DEST/'Project'/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/rel,target)
 digest=sha(ROOT/rel);assert sha(target)==digest
 entries.append(dict(path='Project/'+rel,sha256=digest,bytes=target.stat().st_size))
manifest=dict(type='INCREMENTAL_REQUIRES_BASE_CHAIN',standalone=False,createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),base=str(BACKUP/chain[-1][0]),baseManifest='INCREMENTAL_MANIFEST.json',baseManifestSHA256=chain[-1][2],entries=entries,deletedPaths=[],summary='Exactly three owner-requested NPC artwork replacements and one canonical white Lumi design. Windows focused 297 assertions PASS; no Android build; owner preview pending Hana source hem.',baseChainVerified=True)
dump(DEST/'INCREMENTAL_MANIFEST.json',manifest)
dump(VAL/'checkpoint-result.json',dict(path=str(DEST),manifestSHA256=sha(DEST/'INCREMENTAL_MANIFEST.json'),files=len(entries),bytes=size,standalone=False,baseChainVerified=True))
print('CHECKPOINT VERIFIED '+str(DEST))
