"""Read-only project inventory. Never deletes assets or follows backup directories."""
import argparse
import hashlib
import json
import re
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
TEXT = {'.cs', '.py', '.ps1', '.json', '.asset', '.meta', '.unity', '.prefab',
        '.mat', '.controller', '.anim', '.spriteatlas', '.spriteatlasv2', '.asmdef',
        '.md', '.txt', '.html', '.js', '.xml', '.inputactions', '.scenetemplate'}
GUID = re.compile(r'guid:\s*([a-fA-F0-9]{32})')


def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest()


def relative(path):
    return path.relative_to(ROOT).as_posix()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True)
    args = parser.parse_args()
    output = (ROOT / args.output).resolve()
    output.relative_to(ROOT / 'Docs' / 'Development' / 'Storage')
    if output.exists():
        raise SystemExit('Use a new evidence directory; prior audit is preserved')
    assets = []
    duplicates = defaultdict(list)
    for path in sorted((ROOT / 'Assets').rglob('*')):
        if not path.is_file() or path.suffix == '.meta':
            continue
        meta = Path(str(path) + '.meta')
        meta_text = meta.read_text(encoding='utf-8-sig') if meta.exists() else ''
        own_guid = re.search(r'^guid: (\w+)', meta_text, re.M)
        row = dict(path=relative(path), bytes=path.stat().st_size, sha256=digest(path),
                   guid=own_guid[1] if own_guid else None,
                   metaSha256=digest(meta) if meta.exists() else None)
        assets.append(row)
        duplicates[(row['bytes'], row['sha256'])].append(row['path'])
    references = defaultdict(set)
    documents = []
    # Historical reports are recorded as mentions, never treated as deletable outputs.
    for folder in ('Assets', 'ProjectSettings', 'Packages', 'Tools', 'Docs'):
        for path in (ROOT / folder).rglob('*'):
            if not path.is_file() or path.suffix.lower() not in TEXT:
                continue
            if folder == 'Docs' and 'Storage' in path.relative_to(ROOT / folder).parts:
                continue
            if path.stat().st_size > 12_000_000:
                continue
            text = path.read_text(encoding='utf-8-sig', errors='replace')
            name = relative(path)
            documents.append((name, text))
            for guid in GUID.findall(text):
                references[guid.lower()].add(name)
    for row in assets:
        consumers = sorted(p for p in references.get(row['guid'], ()) if p != row['path'] + '.meta')
        row['guidConsumers'] = consumers
        row['activeGuidConsumers'] = [p for p in consumers if p.startswith(('Assets/', 'ProjectSettings/', 'Packages/'))]
        row['pathMentions'] = sorted(name for name, text in documents
                                     if name not in (row['path'], row['path'] + '.meta')
                                     and (row['path'] in text or Path(row['path']).name in text))
        row['classification'] = ('RESOURCE_REQUIRES_DYNAMIC_AUDIT' if '/Resources/' in row['path']
                                 else 'REFERENCED' if row['activeGuidConsumers']
                                 else 'REVIEW_EDITOR_DYNAMIC_AND_PROVENANCE')
    sizes = []
    for child in sorted(ROOT.iterdir()):
        if child.name == '.git':
            continue
        files = [child] if child.is_file() else (p for p in child.rglob('*') if p.is_file())
        count = size = 0
        for path in files:
            try:
                size += path.stat().st_size
                count += 1
            except OSError:
                pass
        sizes.append(dict(path=child.name, files=count, bytes=size))
    output.mkdir(parents=True)
    result = dict(assets=assets, duplicateGroups=[dict(bytes=key[0], sha256=key[1], paths=paths)
                  for key, paths in duplicates.items() if len(paths) > 1], workspaceSizes=sizes)
    (output / 'inventory.json').write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps(dict(assets=len(assets), assetBytes=sum(a['bytes'] for a in assets),
                         duplicateGroups=len(result['duplicateGroups']), output=relative(output)), indent=2))


if __name__ == '__main__':
    main()
