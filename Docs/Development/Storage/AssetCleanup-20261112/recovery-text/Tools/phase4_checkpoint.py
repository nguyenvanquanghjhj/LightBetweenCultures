"""Small incremental Phase4 checkpoints; explicit source/evidence scope, no cache/build scan."""
import hashlib
import json
import re
import shutil
import sys
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BACKUPS = ROOT.parent / 'Backups'
DOC = ROOT / 'Docs/Gameplay/Phase4-20261018'


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def inherited(path):
    m = json.loads((path / 'CHECKPOINT_MANIFEST.json').read_text(encoding='utf-8-sig'))
    parent = m.get('baseCheckpoint')
    # Only the new Phase4 chain plus its immediate lighting base are needed here.
    known = inherited(Path(parent)) if parent and path.name.startswith('CheckpointPhase4') else {}
    known.update({e['path']: e['sha256'] for e in m.get('entries', [])})
    return known


def main():
    city, base_name = sys.argv[1:3]
    base = BACKUPS / base_name
    dest = BACKUPS / ('CheckpointPhase4' + city + '-20261018')
    assert not dest.exists(), 'Never overwrite a checkpoint'
    for e in json.loads((DOC / 'protected-files.json').read_text()):
        assert sha(ROOT / e['path']) == e['sha256'], e['path']
    # Unity editor adds unused Android icon slots even during a Windows build.
    settings = ROOT / 'ProjectSettings/ProjectSettings.asset'
    old_settings = BACKUPS / 'BeforePhase4Relics-20261018/ProjectSettings/ProjectSettings.asset'
    pat = r'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
    assert re.sub(pat, '', settings.read_text(encoding='utf-8-sig')) == re.sub(pat, '', old_settings.read_text(encoding='utf-8-sig'))
    settings.write_bytes(old_settings.read_bytes())
    # Art export must have been merged with inventory-only first. Restore timestamp/QA-preview noise only.
    art_base = BACKUPS / 'CheckpointCheckInArt-20261018'
    for name in ['native-art-bindings.json', 'art-asset-manifest.json']:
        p = ROOT / 'Docs/Art' / name
        a = json.loads((art_base / 'Docs/Art' / name).read_text(encoding='utf-8-sig'))
        b = json.loads(p.read_text(encoding='utf-8-sig'))
        if name == 'art-asset-manifest.json':
            preview = 'Docs/Gameplay/HoiAnLighting-20261018/ABC-runtime-comparison.png'
            added = [e for e in b['images'] if e['path'] == preview]
            b['images'] = [e for e in b['images'] if e['path'] != preview]
            b['counts']['all_images'] -= len(added)
            a.pop('generated_utc', None)
            b.pop('generated_utc', None)
        assert a == b, 'Unexpected real art binding/catalog delta: ' + name
    for name in ['native-art-bindings.json', 'art-asset-manifest.json', 'art-asset-manifest.csv', 'ART_ASSET_CATALOG.md']:
        (ROOT / 'Docs/Art' / name).write_bytes((art_base / 'Docs/Art' / name).read_bytes())
    sources = ['Assets/_Game/Scripts/Content/RelicCollection.cs', 'Assets/_Game/Scripts/Content/RelicActivity.Trials.cs',
               'Assets/_Game/Scripts/Content/NodeEventManager.cs', 'Assets/_Game/Scripts/UI/GameplayPanels.cs',
               'Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs', 'Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs', 'Assets/_Game/Scripts/UI/GameplayPanels.CheckIns.cs',
               'Assets/_Game/Scripts/UI/RelicSurveySurface.cs', 'Assets/_Game/Scripts/UI/InventoryPanel.cs',
               'Assets/_Game/Scripts/Maps/CityMap.RelicLight.cs', 'Assets/_Game/Scripts/Maps/CityMap.CulturalRestoration.cs',
               'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs', 'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Phase4.cs', 'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Phase4Integration.cs',
               'Assets/_Game/Resources/Content/CulturalRelics.json']
    files = [ROOT / s for s in sources]
    files += [ROOT / (s + '.meta') for s in sources if (ROOT / (s + '.meta')).is_file()]
    files += [ROOT / s for s in ['Tools/phase4_checkpoint.py', 'Tools/phase4_validation.ps1', 'Tools/phase4_verify.py', 'Tools/phase4_report.py',
              'Docs/Prompts/LightBetweenCultures_Phase4_MERGED_Relics_Reactions_LinkKey_20261018.md',
              'Docs/Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md', 'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md',
              'Docs/Gameplay/ARTIFACT_FRAGMENT_ACCESS_MATRIX.md', 'Docs/Gameplay/ARTIFACT_FRAGMENT_SOURCE_MATRIX.md',
              'Docs/Gameplay/ARTIFACT_UNLOCK_DECISIONS.md',
              'Docs/Gameplay/SIX_ARTIFACT_PROGRESSION_DESIGN.md', 'Docs/Story/FULL_STORY_PLAYTEST.md']]
    files += [p for p in DOC.rglob('*') if p.is_file() and p.name != 'last-checkpoint.json']
    for run in (ROOT / 'Validation/Phase4-20261018').iterdir():
        if not run.is_dir():
            continue
        result = next((p for p in [run / 'smoke-results.txt', run / 'restart-results.txt'] if p.is_file()), None)
        if result is None or result.read_text(encoding='utf-8-sig').splitlines()[-1] not in ['ALL CHECKS PASSED', 'FAILED']:
            continue  # A live Player can only write its private run; never snapshot a live save/log.
        files += [p for p in run.rglob('*') if p.is_file()]
    files += list((ROOT / 'Logs').glob('phase4-*.log'))
    files += list((ROOT / 'Docs/QA').glob('PHASE4_*'))
    files += [ROOT / 'Docs/Art/SIX_RELICS_ART_MANIFEST.md']
    known = inherited(base)
    files = [p for p in sorted(set(files)) if p.is_file() and known.get(p.relative_to(ROOT).as_posix()) != sha(p)]
    size = sum(p.stat().st_size for p in files)
    print('Checkpoint delta:', len(files), 'files,', round(size / 1024**2, 2), 'MiB')
    # Final delta reviewed: ~157MiB, mostly two fresh routes, retained failure
    # diagnostics and required runtime review images. No build/cache/full copy.
    limit = 175 if city == 'Final' else 150
    assert size < limit * 1024**2, 'Unexpected large delta; review before copying'
    entries = []
    for p in files:
        rel = p.relative_to(ROOT)
        q = dest / rel
        q.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(p, q)
        h = sha(p)
        assert sha(q) == h
        entries.append(dict(path=rel.as_posix(), bytes=p.stat().st_size, sha256=h))
    m = dict(type='INCREMENTAL_REQUIRES_BASE', baseCheckpoint=str(base), baseManifestSHA256=sha(base / 'CHECKPOINT_MANIFEST.json'),
             createdUTC=datetime.now(timezone.utc).isoformat(), summary='Phase4 ' + city + ' checkpoint; see active implementation report for verified scope and pending work.',
             entries=entries, deletedPaths=[], totalPayloadBytes=size, fileCount=len(entries))
    manifest = dest / 'CHECKPOINT_MANIFEST.json'
    manifest.write_text(json.dumps(m, indent=2) + '\n', encoding='utf-8')
    receipt = dict(checkpoint=str(dest), requiresBase=str(base), manifestSHA256=sha(manifest), payloadBytes=size, files=len(entries), allCopiedHashesVerified=True)
    (DOC / 'last-checkpoint.json').write_text(json.dumps(receipt, indent=2) + '\n')
    print(json.dumps(receipt, indent=2))


if __name__ == '__main__':
    main()
