"""Reviewed, one-shot workspace relocation. No recursive deletion or backup rewriting.

inventory: capture every file/directory and hash all non-generated files and every moved file.
apply: require recorded hashes, rename on the same volume, then hash every destination.
"""
import argparse
import hashlib
import json
import os
import shutil
import subprocess
from pathlib import Path

PROJECT = Path(__file__).resolve().parents[2]
WORKSPACE = PROJECT.parent
OUT = PROJECT / 'Docs/Development/Storage/WorkspaceRestructure-20261113'
GENERATED = {'.git', 'Library', 'Temp', 'obj', 'Logs', 'UserSettings', '__pycache__', '.vs'}


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def save(name, value):
    (OUT / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def plan():
    moves = []
    def add(old, new, role):
        source = WORKSPACE / old
        if source.exists():
            assert source.resolve() != PROJECT.resolve() and '.git' not in source.relative_to(WORKSPACE).parts
            assert not (WORKSPACE / new).exists(), new
            moves.append(dict(old=old, new=new, role=role))
    for child in sorted((WORKSPACE / 'Backups').iterdir()):
        category = 'Checkpoints' if child.name.startswith('Checkpoint') else 'Backups'
        add('Backups/' + child.name, '_LBC_ARCHIVE/' + category + '/' + child.name, 'protected restore point')
    for name in ('LBC_FINAL_PREPATCH_BACKUP_20261110', 'LBC_SEOUL_STORM_BOT_BACKUP_20261110_2209'):
        add(name, '_LBC_ARCHIVE/Backups/' + name, 'protected submission-era backup')
    add('LightBetweenCultures-ArtArchive-20261113', '_LBC_ARCHIVE/HistoricalArt/SourceArchive-20261113', 'protected original art and metadata')
    add('catventure_checkpoint.py', '_LBC_ARCHIVE/HistoricalSources/CheckpointTools/catventure_checkpoint.py', 'historical checkpoint generator; do not run')
    add('debug.log', '_LBC_WORK/Logs/Workspace-debug.log', 'diagnostic log')
    add('inspect_relic_crop.png', '_LBC_WORK/Debug/inspect_relic_crop.png', 'diagnostic crop; not game art')
    for child in sorted((WORKSPACE / 'Deliveries').iterdir()):
        add('Deliveries/' + child.name, '_LBC_DELIVERABLES/Handoffs/' + child.name, 'original handoff')
    p = PROJECT.name + '/'
    for child in sorted((PROJECT / 'Backups').iterdir()):
        add(p + 'Backups/' + child.name, '_LBC_ARCHIVE/Backups/Project/' + child.name, 'protected project backup')
    for name in ('_LBC_FINAL_BACKUP_20261110', '_LBC_ONE_PATCH_BACKUP_20261110'):
        add(p + name, '_LBC_ARCHIVE/Backups/Project/' + name, 'protected project backup')
    for name in ('LBC_FINAL_ONE_20261110', 'LBC_ONE_PATCH_20261110'):
        add(p + name, '_LBC_ARCHIVE/HistoricalPatches/' + name, 'historical patch bundle')
    for release in sorted((PROJECT / 'Builds').iterdir()):
        if release.is_dir():
            for child in sorted(release.iterdir()):
                category = 'Android' if child.name == 'Android' else 'Windows'
                add(p + child.relative_to(PROJECT).as_posix(), '_LBC_DELIVERABLES/' + category + '/' + release.name + '/' + child.name, 'preserved delivery bytes')
        else:
            add(p + release.relative_to(PROJECT).as_posix(), '_LBC_ARCHIVE/HistoricalSources/ReleaseNotes/' + release.name, 'historical release pointer')
    add(p + 'Submission/KGW2026', '_LBC_DELIVERABLES/CompetitionSubmission/KGW2026', 'competition submission; immutable')
    add(p + 'ReviewPacks', '_LBC_DELIVERABLES/Handoffs/ReviewPacks', 'review handoffs')
    add(p + 'build', '_LBC_WORK/Logs/PreviousBuildOutput', 'previous build log')
    add(p + 'debug.log', '_LBC_WORK/Logs/Project-debug.log', 'diagnostic log')
    add(p + 'FINAL_QA_REPORT.md', p + 'Docs/QA/FINAL_QA_REPORT.md', 'current QA report')
    add(p + 'README_AUDIO_MAPPING.txt', p + 'Docs/Development/README_AUDIO_MAPPING.txt', 'audio authoring reference')
    add(p + 'RELEASE_FREEZE.md', p + 'Docs/Release/RELEASE_FREEZE.md', 'release policy')
    add(p + 'References/ImportSources', p + 'Tools/Import/Sources', 'tracked authoring inputs required for reproducible imports')
    add(p + 'References/MarkerDesign', p + 'Tools/Import/Sources/MarkerDesign', 'tracked marker authoring inputs')
    add(p + 'References/NeedsReview', '_LBC_ARCHIVE/HistoricalArt/NeedsReview', 'unreviewed derivatives preserved')
    recent = ('ArchitectureRefactor-', 'AssetCleanup-', 'UnusedAssets-', 'WorkspaceRestructure-')
    for child in sorted((PROJECT / 'Validation').iterdir()):
        parent = '_LBC_WORK/QA/LightBetweenCultures/' if child.name.startswith(recent) else '_LBC_ARCHIVE/HistoricalQA/Validation/'
        add(p + 'Validation/' + child.name, parent + child.name, 'QA evidence; preserved')
    # Actual source snapshots are archived; authored config and explanatory documents stay tracked.
    for folder in ('Art/NpcIdentityLock-20261017/BeforeFiles', 'Art/NpcIdentityLock-20261017/WithdrawnDraft',
                   'Art/NpcReuse-20261017/BeforeFiles', 'Contest/KGW2026/before-source', 'H3Upgrade-20261017/BeforeFiles',
                   'QualityAddendum-20261017/BeforeFiles', 'RenderQuality-20261017/BeforeFiles'):
        add(p + 'Docs/' + folder, '_LBC_ARCHIVE/HistoricalSources/DocumentSnapshots/' + folder, 'protected historical source snapshot')
    for name in ('Catventure', 'Chapters', 'Gameplay', 'H3Upgrade-20261017', 'Maps', 'Prompts', 'Puzzles',
                 'QualityAddendum-20261017', 'RenderQuality-20261017', 'Requests', 'Story'):
        add(p + 'Docs/' + name, p + 'Docs/Development/' + name, 'development documentation/configuration')
    for child in sorted((PROJECT / 'Docs/Storage').iterdir()):
        add(p + 'Docs/Storage/' + child.name, p + 'Docs/Development/Storage/' + child.name, 'preserved audit evidence')
    add(p + 'Docs/Contest', p + 'Docs/Release/Contest', 'competition/release documentation')
    for child in sorted((PROJECT / 'Docs').iterdir()):
        if child.is_file() and child.name != 'PROJECT_INDEX.md':
            add(p + 'Docs/' + child.name, p + 'Docs/Development/' + child.name, 'development reference')
    imports = {'art_asset_catalog.py', 'import_gameplay_content.py', 'prepare_catventure_assets.py', 'prepare_checkin_runtime.py',
               'synthesize_soft_sfx.py', 'apply_approved_route_references.py', 'export_route_markup.py', 'export_applied_route_references.py'}
    qa = {'run_catventure_domain.py', 'android_autoplay.py', 'verify_applied_route_references.py'}
    build = {'build_kgw2026_pitch.py', 'package_kgw2026.py', 'verify_android_apk.ps1'}
    for child in sorted((PROJECT / 'Tools').iterdir()):
        if child.is_file():
            category = 'Import' if child.name in imports else 'QA' if child.name in qa else 'Build' if child.name in build else None
            target = p + 'Tools/' + category + '/' + child.name if category else '_LBC_ARCHIVE/HistoricalSources/OneTimeTools/' + child.name
            add(p + 'Tools/' + child.name, target, 'maintained pipeline' if category else 'historical one-time tool; no C# pipeline invocation found')
    add(p + 'Tools/CatRescueTests', p + 'Tools/QA/CatRescueTests', 'maintained domain regression')
    add(p + 'Tools/SixCityRouteEditor', p + 'Tools/Import/SixCityRouteEditor', 'maintained route authoring tool')
    for name in ('migrate_project_layout.py', 'finalize_project_layout.py', 'cleanup_generated_gradle.py', 'eliminate_unused_art.py',
                 'verify_project_layout.py', 'verify_unused_assets.py'):
        add(p + 'Tools/Maintenance/' + name, '_LBC_ARCHIVE/HistoricalSources/MaintenanceRuns/' + name, 'completed historical migration/check')
    return moves


def inventory():
    if (OUT / 'inventory.json').exists():
        raise SystemExit('Existing inventory is protected')
    moves = plan()
    groups = {r['old']: r for r in moves}
    files = []
    folders = []
    for current, dirs, names in os.walk(WORKSPACE):
        folder = Path(current)
        rel = folder.relative_to(WORKSPACE)
        folders.append(rel.as_posix())
        for name in names:
            path = folder / name
            key = path.relative_to(WORKSPACE).as_posix()
            group = next((g for g in groups if key == g or key.startswith(g + '/')), None)
            generated = group is None and any(part in GENERATED for part in path.relative_to(PROJECT).parts) if path.is_relative_to(PROJECT) else False
            h = None if generated else digest(path)
            files.append(dict(path=key, bytes=path.stat().st_size, sha256=h, hashReason='regenerable/not moved' if generated else 'SHA-256',
                              moveGroup=group, decision='MOVE' if group else 'KEEP', type=path.suffix.lower()))
        if len(files) % 5000 < len(names):
            print('Inventoried ' + str(len(files)) + ' files', flush=True)
    save('inventory.json', dict(workspace=str(WORKSPACE), project=str(PROJECT), files=files, folders=folders,
                                topLevel=[p.name for p in WORKSPACE.iterdir()], imageCount=sum(r['type'] in ('.png','.jpg','.jpeg','.webp','.psd','.svg') for r in files)))
    save('move-plan.json', moves)
    git = ['git', '-c', 'safe.directory=' + PROJECT.as_posix(), '-C', str(PROJECT)]
    save('git-before.json', {key: subprocess.check_output(git + args, text=True) for key, args in
                            [('branch', ['branch','--show-current']), ('head', ['rev-parse','HEAD']), ('status', ['status','--porcelain=v1','-uall'])]})
    print('Inventory complete: files=' + str(len(files)) + ' moves=' + str(len(moves)), flush=True)


def apply():
    if (OUT / 'move-result.json').exists():
        raise SystemExit('Completed move manifest exists')
    data = json.loads((OUT / 'inventory.json').read_text(encoding='utf-8'))
    moves = json.loads((OUT / 'move-plan.json').read_text(encoding='utf-8'))
    completed = []
    for row in moves:
        source, target = WORKSPACE / row['old'], WORKSPACE / row['new']
        source.resolve().relative_to(WORKSPACE.resolve())
        target.resolve().relative_to(WORKSPACE.resolve())
        assert source.exists() and not target.exists(), row
        entries = [r for r in data['files'] if r['moveGroup'] == row['old']]
        for entry in entries:
            assert entry['sha256'] and digest(WORKSPACE / entry['path']) == entry['sha256'], entry['path']
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(source), str(target))
        for entry in entries:
            suffix = entry['path'][len(row['old']):].lstrip('/')
            actual = target / suffix if suffix else target
            assert digest(actual) == entry['sha256'], str(actual)
        completed.append(dict(**row, files=len(entries), bytes=sum(r['bytes'] for r in entries), status='HASH_VERIFIED'))
        save('move-progress.json', completed)
        print('Moved and verified: ' + row['old'], flush=True)
    save('move-result.json', completed)
    save('deletions.json', [])
    save('backup-preservation.json', [r for r in completed if 'backup' in r['role'] or 'restore point' in r['role'] or 'checkpoint' in r['role']])
    # Only remove verified empty old containers; no recursive delete.
    for folder in ('Backups', 'Deliveries', PROJECT.name + '/Backups', PROJECT.name + '/Builds', PROJECT.name + '/References',
                   PROJECT.name + '/Submission', PROJECT.name + '/Validation', PROJECT.name + '/Docs/Storage'):
        path = WORKSPACE / folder
        if path.is_dir():
            for current, dirs, names in os.walk(path, topdown=False):
                node = Path(current)
                if not any(node.iterdir()):
                    node.rmdir()


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('action', choices=('inventory','apply'))
    args = parser.parse_args()
    OUT.mkdir(parents=True, exist_ok=True)
    inventory() if args.action == 'inventory' else apply()
