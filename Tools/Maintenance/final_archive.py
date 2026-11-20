"""Non-destructive archive inventory and integrity checks; never deletes files."""
import argparse
import collections
import csv
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess

PROJECT = Path(__file__).resolve().parents[2]
WORKSPACE = PROJECT.parent
EVIDENCE = PROJECT / 'Docs/Development/Storage/FinalArchive-20261120'


def git(*args):
    result = subprocess.run(['git', '-c', 'safe.directory=' + PROJECT.as_posix(), '-C', str(PROJECT), *args], capture_output=True)
    if result.returncode:
        raise RuntimeError(result.stderr.decode('utf-8', 'replace'))
    return result.stdout


def sha(path):
    with Path(path).open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def write(name, value):
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    (EVIDENCE / name).write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')


def walk(root):
    errors = []
    for folder, directories, files in os.walk(root, onerror=lambda e: errors.append(str(e)), followlinks=False):
        directories[:] = [d for d in directories if not (Path(folder) / d).is_symlink()]
        for name in files:
            path = Path(folder) / name
            try:
                yield path, path.stat().st_size
            except OSError as error:
                errors.append(str(error))
    if errors:
        write('walk-errors-'+hashlib.sha256(str(root).encode()).hexdigest()[:8]+'.json', dict(root=str(root),errors=errors))


def inventory():
    groups = collections.defaultdict(lambda: dict(files=0, bytes=0))
    largest = []
    EVIDENCE.mkdir(parents=True, exist_ok=True)
    with (EVIDENCE / 'workspace-files.csv').open('w', newline='', encoding='utf-8-sig') as stream:
        writer = csv.writer(stream)
        writer.writerow(['path', 'bytes', 'category'])
        for path, size in walk(WORKSPACE):
            rel = path.relative_to(WORKSPACE).as_posix()
            parts = rel.split('/')
            category = '/'.join(parts[:2])
            groups[category]['files'] += 1
            groups[category]['bytes'] += size
            writer.writerow([rel, size, category])
            if size >= 50 * 1024**2:
                largest.append(dict(path=rel, bytes=size))
    disks = {}
    for drive in ['C:/', 'D:/', 'G:/']:
        try:
            disks[drive] = shutil.disk_usage(drive)._asdict()
        except OSError as error:
            disks[drive] = dict(error=str(error))
    write('inventory-summary.json', dict(groups=groups, largest=sorted(largest, key=lambda r: -r['bytes']), disks=disks))
    print(json.dumps(dict(groups=groups, disks=disks), indent=2))


def source_files():
    candidates = git('ls-files', '-z', '--cached', '--others', '--exclude-standard').decode('utf-8').split('\0')
    return sorted({p for p in candidates if p and (PROJECT / p).is_file()})


def audit():
    files = source_files()
    summary = collections.defaultdict(lambda: dict(files=0, bytes=0))
    large = []
    secrets = []
    patterns = [r'gh[pousr]_[A-Za-z0-9]{30,}', r'github_pat_[A-Za-z0-9_]{40,}', r'AKIA[0-9A-Z]{16}',
                r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----', r'AIza[0-9A-Za-z_-]{30,}',
                r'(?i)(?:api[_-]?key|password|access[_-]?token|client[_-]?secret)\s*[:=]\s*[\"\'][^\"\'\s]{12,}[\"\']']
    for rel in files:
        path = PROJECT / rel
        size = path.stat().st_size
        group = rel.split('/')[0]
        summary[group]['files'] += 1
        summary[group]['bytes'] += size
        if size >= 50 * 1024**2:
            large.append(dict(path=rel, bytes=size))
        if size < 32 * 1024**2 and path.suffix.lower() in {'.cs', '.py', '.json', '.md', '.txt', '.xml', '.asset', '.yml', '.yaml', '.config', '.csv', '.ps1'}:
            content = path.read_text(encoding='utf-8-sig', errors='replace')
            for number, line in enumerate(content.splitlines(), 1):
                if any(re.search(pattern, line) for pattern in patterns):
                    secrets.append(dict(path=rel, line=number, reason='pattern match; value redacted'))
    meta = {}; duplicates = []; missing_meta = []; orphan_meta = []
    for path in (PROJECT / 'Assets').rglob('*'):
        if path.name.startswith('.'):
            continue
        if path.suffix == '.meta':
            match = re.search(r'^guid: ([a-f0-9]{32})', path.read_text(encoding='utf-8-sig'), re.M)
            if match:
                guid = match[1]
                if guid in meta:
                    duplicates.append([meta[guid], path.relative_to(PROJECT).as_posix()])
                meta[guid] = path.relative_to(PROJECT).as_posix()
            if not Path(str(path)[:-5]).exists():
                orphan_meta.append(path.relative_to(PROJECT).as_posix())
        elif not Path(str(path) + '.meta').exists():
            missing_meta.append(path.relative_to(PROJECT).as_posix())
    ignored = git('ls-files', '--others', '--ignored', '--exclude-standard', '-z', '--', 'Assets', 'Packages', 'ProjectSettings').decode('utf-8').strip('\0').split('\0')
    deleted = git('diff', '--name-only', '--diff-filter=D', 'HEAD', '-z').decode('utf-8').strip('\0').split('\0')
    write('source-audit.json', dict(files=len(files), groups=summary, largeFiles=large, secretCandidates=secrets,
                                  missingMeta=missing_meta, orphanMeta=orphan_meta, duplicateGuids=duplicates,
                                  ignoredRequired=[p for p in ignored if p], deletedPaths=[p for p in deleted if p]))
    (EVIDENCE / 'source-paths.txt').write_text('\n'.join(files) + '\n', encoding='utf-8')
    print(json.dumps(dict(files=len(files), groups=summary, largeFiles=large, secretCandidates=secrets,
                         missingMeta=missing_meta, orphanMeta=orphan_meta, duplicateGuids=duplicates,
                         ignoredRequired=[p for p in ignored if p], deletedCount=len(deleted)), indent=2))


def reconcile():
    import sys
    sys.path.insert(0, str(PROJECT / 'Tools/Maintenance'))
    from workspace_model import resolve
    deleted = [p for p in git('diff', '--name-only', '--diff-filter=D', 'HEAD', '-z').decode('utf-8').split('\0') if p]
    index = collections.defaultdict(list)
    private = []; sensitive = []
    protected_ext = {'.keystore', '.jks', '.p12', '.pfx', '.pem', '.key'}
    secret_pattern = re.compile(r'gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{40,}|AKIA[0-9A-Z]{16}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----|AIza[0-9A-Za-z_-]{30,}')
    for root in [PROJECT / 'Assets', PROJECT / 'Tools', PROJECT / 'Docs', WORKSPACE / '_LBC_ARCHIVE', WORKSPACE / '_LBC_DELIVERABLES', WORKSPACE / '_LBC_WORK']:
        for path, size in walk(root):
            if '__pycache__' in path.parts:
                continue
            index[(path.name, size)].append(path)
            if path.suffix.lower() in protected_ext or path.name.startswith('.env'):
                private.append(path.relative_to(WORKSPACE).as_posix())
            if path.suffix.lower() in {'.json', '.cs', '.py', '.md', '.txt', '.xml', '.config', '.ps1'} and size < 32 * 1024**2:
                content = path.read_text(encoding='utf-8-sig', errors='replace')
                if secret_pattern.search(content):
                    sensitive.append(path.relative_to(WORKSPACE).as_posix())
    tree = {}
    for row in git('ls-tree', '-r', '-l', '-z', 'HEAD').decode('utf-8').split('\0'):
        if not row:
            continue
        header, path = row.split('\t', 1)
        fields = header.split()
        tree[path] = (fields[2], int(fields[3]))
    write('archive-secret-scan.json', dict(keyPaths=private, tokenPatternPaths=sensitive, valuesPrinted=False))
    all_paths = sorted({path for paths in index.values() for path in paths})
    blobs = subprocess.run(
        ['git', '-c', 'safe.directory=' + PROJECT.as_posix(), '-C', str(PROJECT), 'hash-object', '--stdin-paths'],
        input=('\n'.join(path.as_posix() for path in all_paths) + '\n').encode('utf-8'), capture_output=True, check=True).stdout
    hashes = dict(zip(all_paths, blobs.decode().splitlines(), strict=True))
    rows = []; unresolved = []
    for rel in deleted:
        oid, size = tree[rel]
        target = resolve(PROJECT.name + '/' + rel)
        if target.is_file():
            rows.append(dict(old=rel, preserved=target.relative_to(WORKSPACE).as_posix(), status='relocation-target-present', unchanged=hashes.get(target) == oid))
            continue
        matches = []
        for path in index[(Path(rel).name, size)]:
            if hashes[path] == oid:
                matches.append(path.relative_to(WORKSPACE).as_posix())
        if matches:
            rows.append(dict(old=rel, preserved=matches, status='identical-content-found'))
        elif rel.endswith(('.csproj', '.slnx', '.pyc')):
            rows.append(dict(old=rel, status='generated-file-removed-from-index'))
        else:
            unresolved.append(rel)
    write('deleted-reconciliation.json', dict(entries=rows, unresolved=unresolved, preservedByGitHistory='HEAD before archival commit retains every deleted tracked blob'))
    write('archive-secret-scan.json', dict(keyPaths=private, tokenPatternPaths=sensitive, valuesPrinted=False, note='Archive-only data needs private storage if any real credentials are found; automated scan cannot prove absence of all secrets.'))
    print(json.dumps(dict(deleted=len(deleted), mapped=len(rows), unresolved=unresolved, keyPaths=private, tokenPatternPaths=sensitive), indent=2))


def system_inventory():
    roots = ['C:/Users/admin', 'D:/']
    skip = {'AppData', '.codex', '.git', 'node_modules', 'Library', 'Temp', 'obj', 'bin', '$RECYCLE.BIN', 'System Volume Information', 'Virtual Machines'}
    projects = []; errors = []
    for root in roots:
        for folder, directories, files in os.walk(root, followlinks=False, onerror=lambda e: errors.append(str(e))):
            directories[:] = [d for d in directories if d not in skip and not (Path(folder)/d).is_symlink()]
            if Path(folder).name == 'ProjectSettings' and 'ProjectVersion.txt' in files:
                project = Path(folder).parent
                projects.append(dict(path=str(project), version=(Path(folder)/'ProjectVersion.txt').read_text(encoding='utf-8-sig').strip(), complete=(project/'Assets').is_dir() and (project/'Packages').is_dir()))
    targets = ['C:/Program Files/Unity/Hub/Editor/6000.6.5f1', 'C:/Program Files/Unity Hub',
               'C:/Users/admin/AppData/Local/Unity/cache', 'C:/Users/admin/AppData/Local/Unity/Caches',
               'C:/Users/admin/AppData/LocalLow/Unity/Caches', 'C:/Users/admin/AppData/Local/Temp/Unity']
    sizes = []
    for target in targets:
        entries = list(walk(Path(target))) if Path(target).exists() else []
        sizes.append(dict(path=target, exists=Path(target).exists(), files=len(entries), bytes=sum(n for _, n in entries)))
    import winreg
    software = []
    for hive in [winreg.HKEY_LOCAL_MACHINE, winreg.HKEY_CURRENT_USER]:
        for key in [r'SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall', r'SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall']:
            try:
                with winreg.OpenKey(hive, key) as top:
                    for number in range(winreg.QueryInfoKey(top)[0]):
                        try:
                            with winreg.OpenKey(top, winreg.EnumKey(top, number)) as child:
                                name = winreg.QueryValueEx(child, 'DisplayName')[0]
                                if re.search(r'Unity|Visual Studio|Visual Studio Code|\.NET|JDK|Java|Git|Android', name, re.I):
                                    software.append(name)
                        except OSError:
                            pass
            except OSError:
                pass
    write('system-inventory.json', dict(projectSearchRoots=roots, skippedDirectoryNames=sorted(skip), projects=projects, errors=errors, sizes=sizes, installedRelevantSoftware=sorted(set(software)), limitation='Skipped application/cache/git directories; inaccessible and unmounted drives are not certified free of projects. Shared developer tools and personal data are excluded from cleanup.'))
    print(json.dumps(dict(projects=projects, sizes=sizes, errors=errors, software=sorted(set(software))), ensure_ascii=True, indent=2))


def stage():
    audit_result = json.loads((EVIDENCE/'source-audit.json').read_text(encoding='utf-8'))
    if any(audit_result[key] for key in ['secretCandidates', 'missingMeta', 'orphanMeta', 'duplicateGuids', 'ignoredRequired']):
        raise SystemExit('Resolve source audit findings before staging')
    files = source_files()
    if any((PROJECT/p).stat().st_size >= 100*1024**2 for p in files):
        raise SystemExit('Source file exceeds ordinary GitHub limit')
    deleted = [p for p in git('diff', '--name-only', '--diff-filter=D', '-z').decode('utf-8').split('\0') if p and not (PROJECT/p).exists()]
    paths = sorted(set(files + deleted))
    result = subprocess.run(['git', '-c', 'safe.directory='+PROJECT.as_posix(), '-C', str(PROJECT), 'add', '-A', '--pathspec-from-file=-', '--pathspec-file-nul'], input=('\0'.join(paths)+'\0').encode('utf-8'), capture_output=True)
    stage_log = WORKSPACE/'_LBC_WORK/Logs/FinalArchive-stage.log'
    stage_log.parent.mkdir(parents=True, exist_ok=True)
    stage_log.write_bytes(result.stderr)
    if result.returncode:
        raise SystemExit('Staging failed; inspect _LBC_WORK/Logs/FinalArchive-stage.log')
    print(json.dumps(dict(explicitPaths=len(paths), stagedPaths=len(git('diff', '--cached', '--name-only', '-z').split(b'\0'))-1)))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('mode', choices=['inventory', 'audit', 'reconcile', 'system', 'stage'])
    args = parser.parse_args()
    {'inventory': inventory, 'audit': audit, 'reconcile': reconcile, 'system': system_inventory, 'stage': stage}[args.mode]()
