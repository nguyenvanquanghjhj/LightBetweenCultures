"""Verify the existing restore chain; estimate/create a source-only shop delta."""
from pathlib import Path
import argparse, datetime, hashlib, json, shutil

ROOT = Path(__file__).resolve().parents[1]
VAL = ROOT / 'Validation/JourneyShop-20261021'
DEST = Path('D:/GAMEKKK/Backups/CheckpointJourneyShop-20261021')
CHAIN = [
    ('LBC_SourceVerified-20261020', 'SOURCE_MANIFEST.json', '53305d534a5d5c5860003f6526d730cd412a0294a1f8c7b52831b74a54ace7a6'),
    ('CheckpointTouchNpcDialogue-20261020', 'INCREMENTAL_MANIFEST.json', 'b2f63c5eb392cfc4ac9d1793546611557d4520aa4ef132e0291c6f68d3064456'),
    ('CheckpointNpcApproachButtons-20261020', 'INCREMENTAL_MANIFEST.json', '5c9009f720f8cd601ca1652eec52960f4e06f7dc7b7a440a795762f641f0ecb1'),
    ('CheckpointGelButtons-20261020', 'INCREMENTAL_MANIFEST.json', '6b6597a57aa41e790517d7509a8a8e0f00d7f9d0498fb35180db2091e856e837'),
]
read = lambda p: json.loads(p.read_text(encoding='utf-8-sig'))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
dump = lambda p, d: p.write_text(json.dumps(d, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
parser = argparse.ArgumentParser(); parser.add_argument('--create', action='store_true'); args = parser.parse_args()
effective = {}
for name, manifest, digest in CHAIN:
    folder = DEST.parent/name; file = folder/manifest
    assert sha(file) == digest, f'Changed base manifest: {file}'
    data = read(file)
    for entry in data['entries']:
        assert sha(folder/entry['path']) == entry['sha256'], f'Base file mismatch: {entry["path"]}'
        effective[entry['path'].removeprefix('Project/')] = entry['sha256']
    for deleted in data.get('deletedPaths', []): effective.pop(deleted.removeprefix('Project/'), None)
current = {p.relative_to(ROOT).as_posix(): p for name in ('Assets', 'Packages', 'ProjectSettings')
           for p in (ROOT/name).rglob('*') if p.is_file()}
changed = {rel for rel, p in current.items() if effective.get(rel) != sha(p)}
deleted = [rel for rel in effective if rel.split('/')[0] in ('Assets','Packages','ProjectSettings') and rel not in current]
freeze = read(ROOT/'Validation/ComprehensiveQA-20261020/source-freeze.json')
allowed = {'Assets/_Game/Scripts/'+s for s in (
    'Core/GameBootstrap.cs','Core/QuestManager.cs','Maps/CityMap.cs',
    'Editor/OwnerRouteAndroidBuild.cs','Validation/GameplaySmokeRunner.cs',
    'Validation/GameplaySmokeRunner.Lighting.cs','Validation/GameplaySmokeRunner.OwnerRoutesB.cs',
    'Validation/GameplaySmokeRunner.Relics.cs')}
protected_changes = [e['path'] for e in freeze['files'] if e['path'] not in allowed and
                     (not (ROOT/e['path']).exists() or sha(ROOT/e['path']) != e['sha256'])]
assert not protected_changes, f'Unexpected protected input changes: {protected_changes}'
runtime = VAL/'runtime01/smoke-results.txt'
assert runtime.read_text(encoding='utf-8-sig').rstrip().endswith('ALL CHECKS PASSED')
paths = changed | {
    'Tools/journey_shop_audit.py','Tools/checkpoint_journey_shop.py',
    'Docs/QA/JOURNEY_COIN_SHOP.md','Docs/QA/COMPREHENSIVE_QA_CHECKPOINT_20261021.md',
    'Docs/QA/CURRENT_QA_STATUS.md',
    'Validation/JourneyShop-20261021/economy-audit.json',
    'Validation/JourneyShop-20261021/item-classification.csv',
    'Validation/JourneyShop-20261021/runtime01/smoke-results.txt',
    'Validation/JourneyShop-20261021/runtime01/capture-manifest.txt',
    'Validation/JourneyShop-20261021/runtime01/gameplay-save.json',
    'Validation/JourneyShop-20261021/restart01/restart-results.txt',
    'Validation/JourneyShop-20261021/adb-devices.txt',
    'Validation/JourneyShop-20261021/android-build.json',
    'Validation/JourneyShop-20261021/qa-summary.json',
    'Validation/JourneyShop-20261021/gallery.html',
}
paths = sorted(p for p in paths if (ROOT/p).is_file())
size = sum((ROOT/p).stat().st_size for p in paths)
info = {'checkpoint': str(DEST), 'files': len(paths), 'bytes': size,
        'sourceChanges': sorted(changed), 'deletedPaths': deleted,
        'protectedInputsUnchanged': True, 'protectedFilesCompared': len(freeze['files'])-len(allowed),
        'baseChainVerified': True, 'standalone': False}
dump(VAL/'checkpoint-plan.json', info)
print(json.dumps(info, ensure_ascii=True))
if not args.create: raise SystemExit
assert not DEST.exists(), 'Refusing to overwrite a checkpoint'
DEST.mkdir(parents=True)
entries = []
for rel in paths:
    target = DEST/'Project'/rel; target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(ROOT/rel, target)
    assert sha(target) == sha(ROOT/rel)
    entries.append({'path': 'Project/'+rel, 'sha256': sha(target), 'bytes': target.stat().st_size})
base = DEST.parent/CHAIN[-1][0]
manifest = {'type': 'INCREMENTAL_REQUIRES_BASE_CHAIN', 'standalone': False,
            'createdUTC': datetime.datetime.now(datetime.timezone.utc).isoformat(),
            'base': str(base), 'baseManifest': CHAIN[-1][1], 'baseManifestSHA256': CHAIN[-1][2],
            'entries': entries, 'deletedPaths': deleted, 'bytes': size,
            'summary': 'Journey Coin Shop, focused Windows QA, and inherited unfinished comprehensive QA adapters. No art/audio/route/scene/schema change. Read reports for Android/blind-playtest limitations.'}
dump(DEST/'INCREMENTAL_MANIFEST.json', manifest)
(DEST/'RESTORE.md').write_text('Requires the entire verified base chain recorded in INCREMENTAL_MANIFEST.json. Restore the standalone source point, then TouchNpcDialogue, NpcApproachButtons, GelButtons and this delta in order. Apply only listed deletedPaths inside the restored project. Builds, Unity caches and screenshots are deliberately excluded; evidence remains in the live Validation folders. This is not a standalone project backup.\n',encoding='utf-8')
info['manifestSHA256'] = sha(DEST/'INCREMENTAL_MANIFEST.json'); dump(VAL/'checkpoint.json', info)
print(json.dumps({'created': str(DEST), 'manifestSHA256': info['manifestSHA256']}, ensure_ascii=True))
