"""Create bounded independent ZIPs and verify extracted archives. Never deletes data."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import zipfile

PROJECT = Path(__file__).resolve().parents[2]
WORKSPACE = PROJECT.parent
STAGING = WORKSPACE/'_LBC_WORK/FinalArchive-20261120'
UPLOAD = STAGING/'Upload'
LIMIT = 1_400_000_000  # Raw input cap leaves ample space below the 2 GiB Release limit.


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def save(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')


def make_zip(name, paths):
    target = UPLOAD/(name+'.zip')
    if target.exists():
        raise SystemExit('Existing archive protected: '+target.name)
    entries = []
    with zipfile.ZipFile(target, 'x', compression=zipfile.ZIP_DEFLATED, compresslevel=1, allowZip64=True) as archive:
        for path in paths:
            rel = path.relative_to(WORKSPACE).as_posix()
            entries.append(dict(path=rel, bytes=path.stat().st_size, sha256=sha(path)))
            archive.write(path, rel)
        archive.writestr('ArchiveManifests/'+name+'.json', json.dumps(dict(entries=entries), ensure_ascii=False, indent=2))
    if target.stat().st_size >= 2*1024**3:
        raise SystemExit('Release asset is too large: '+target.name)
    row = dict(name=target.name, bytes=target.stat().st_size, sha256=sha(target), files=len(entries), rawBytes=sum(r['bytes'] for r in entries))
    print(json.dumps(row), flush=True)
    return row


def historical():
    UPLOAD.mkdir(parents=True, exist_ok=True)
    groups = {
        'Backups_Checkpoints': ['_LBC_ARCHIVE/Backups', '_LBC_ARCHIVE/Checkpoints'],
        'Art_HistoricalSources': ['_LBC_ARCHIVE/HistoricalArt', '_LBC_ARCHIVE/HistoricalPatches', '_LBC_ARCHIVE/HistoricalSources'],
        'HistoricalQA': ['_LBC_ARCHIVE/HistoricalQA'],
        'Deliverables': ['_LBC_DELIVERABLES'],
        'WorkEvidence': ['_LBC_WORK/QA', '_LBC_WORK/Logs', '_LBC_WORK/Debug'],
    }
    archives = []; exclusions = []
    for group, roots in groups.items():
        paths = []
        for root in roots:
            for path in (WORKSPACE/root).rglob('*'):
                if not path.is_file():
                    continue
                if any(part in {'Library', 'Temp', '__pycache__', 'obj', '.gradle'} for part in path.parts):
                    exclusions.append(dict(path=path.relative_to(WORKSPACE).as_posix(), bytes=path.stat().st_size, reason='regenerable generated cache/intermediate'))
                else:
                    paths.append(path)
        batches = []; batch = []; size = 0
        for path in sorted(paths):
            count = path.stat().st_size
            if count > LIMIT:
                raise SystemExit('Single file requires an explicit split plan: '+str(path))
            if size+count > LIMIT and batch:
                batches.append(batch); batch=[]; size=0
            batch.append(path);size+=count
        if batch:
            batches.append(batch)
        for number, files in enumerate(batches, 1):
            archives.append(make_zip('LightBetweenCultures_'+group+'_2026_%02d'%number, files))
    save(UPLOAD/'HISTORICAL_ARCHIVE_MANIFEST.json', dict(archives=archives, exclusions=exclusions, independentZips=True, restore='Extract all independent ZIPs into one empty workspace; do not concatenate ZIP files.'))


def source(tag):
    UPLOAD.mkdir(parents=True, exist_ok=True)
    target = UPLOAD/'LightBetweenCultures_Source_Final_2026.zip'
    bundle = UPLOAD/'LightBetweenCultures_GitHistory_2026.bundle'
    if target.exists() or bundle.exists():
        raise SystemExit('Existing final source archive is protected')
    command = ['git','-c','safe.directory='+PROJECT.as_posix(),'-C',str(PROJECT)]
    commit = subprocess.check_output(command+['rev-parse',tag+'^{commit}']).decode().strip()
    subprocess.run(command+['archive','--format=zip','--prefix=LightBetweenCultures/','--output='+str(target),tag], check=True)
    entries = []
    with zipfile.ZipFile(target, 'a') as archive:
        for info in archive.infolist():
            if not info.is_dir():
                with archive.open(info) as stream:
                    digest = hashlib.file_digest(stream, 'sha256').hexdigest()
                entries.append(dict(path=info.filename, bytes=info.file_size, sha256=digest))
        archive.writestr('ArchiveManifests/Source_Final_2026.json', json.dumps(dict(commit=commit, entries=entries), indent=2))
    subprocess.run(command+['bundle','create',str(bundle),'--all'], check=True)
    subprocess.run(command+['bundle','verify',str(bundle)], check=True, capture_output=True)
    public = dict(tag=tag,commit=commit,archives=[dict(name=target.name,bytes=target.stat().st_size,sha256=sha(target),files=len(entries))])
    private = json.loads((UPLOAD/'HISTORICAL_ARCHIVE_MANIFEST.json').read_text(encoding='utf-8'))
    private['gitHistory'] = dict(name=bundle.name,bytes=bundle.stat().st_size,sha256=sha(bundle))
    save(UPLOAD/'SOURCE_ARCHIVE_MANIFEST.json',public)
    save(UPLOAD/'HISTORICAL_ARCHIVE_MANIFEST.json',private)
    all_files = public['archives']+private['archives']+[private['gitHistory']]
    (UPLOAD/'SHA256SUMS.txt').write_text(''.join(r['sha256']+'  '+r['name']+'\n' for r in all_files), encoding='utf-8')
    print(json.dumps(dict(source=public,bundle=private['gitHistory']),indent=2),flush=True)


def extract_verify(path, destination):
    destination.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(path) as archive:
        manifests = [p for p in archive.namelist() if p.startswith('ArchiveManifests/') and p.endswith('.json')]
        if len(manifests) != 1:
            raise ValueError('Expected one embedded manifest')
        manifest = json.loads(archive.read(manifests[0]))
        for member in archive.infolist():
            target=(destination/member.filename).resolve()
            if not target.is_relative_to(destination.resolve()):
                raise ValueError('Unsafe ZIP member')
            if target.is_file():
                raise ValueError('Existing extracted file protected: '+str(target))
        archive.extractall(destination)
        failures=[]
        for row in manifest['entries']:
            target = destination/row['path']
            if not target.is_file() or target.stat().st_size!=row['bytes'] or sha(target)!=row['sha256']:
                failures.append(row['path'])
        if failures:
            raise ValueError('Extracted hash mismatch: '+repr(failures))
    return dict(archive=path.name, files=len(manifest['entries']), status='PASS', destination=str(destination))


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('mode',choices=['historical','source','verify'])
    parser.add_argument('--tag',default='lbc-final-archive-2026')
    parser.add_argument('--file',type=Path)
    parser.add_argument('--destination',type=Path)
    args=parser.parse_args()
    if args.mode=='historical':historical()
    elif args.mode=='source':source(args.tag)
    else:print(json.dumps(extract_verify(args.file,args.destination),indent=2))
