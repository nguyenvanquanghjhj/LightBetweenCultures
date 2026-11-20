"""Read-only checkpoint chain verification through the workspace relocation map.

Embedded historical absolute paths and base-manifest hashes remain unchanged.
This checks file integrity and base availability, not an actual restored game run.
"""
import argparse
import json
from pathlib import Path
from workspace_model import PROJECT, EVIDENCE, load, sha, resolve


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--manifest',required=True)
    parser.add_argument('--output',required=True)
    args=parser.parse_args()
    output=Path(args.output).resolve()
    if output.exists():raise SystemExit('Existing evidence is protected')
    visited=set();issues=[];files=0;order=[]
    def verify(manifest):
        nonlocal files
        manifest=manifest.resolve()
        if manifest in visited:return
        visited.add(manifest)
        if not manifest.is_file():issues.append('Missing base/manifest: '+str(manifest));return
        data=load(manifest)
        if data.get('base'):
            base=resolve(data['base'])/data.get('baseManifest','INCREMENTAL_MANIFEST.json')
            if base.exists() and data.get('baseManifestSHA256') and sha(base)!=data['baseManifestSHA256']:
                issues.append('Base manifest hash mismatch: '+str(base))
            verify(base)
        for entry in data.get('entries',[]):
            if not isinstance(entry,dict) or not entry.get('sha256'):continue
            path=manifest.parent/entry['path']
            if not path.is_file() or sha(path)!=entry['sha256']:issues.append('File/hash mismatch: '+str(path))
            else:files+=1
        order.append(dict(manifest=str(manifest),sha256=sha(manifest),deletedPaths=data.get('deletedPaths',[])))
    verify(resolve(args.manifest))
    result=dict(status='BLOCKED' if issues else 'PASS',verifiedFiles=files,restoreOrder=order,issues=issues,
                meaning='Read-only existing checkpoint/base hash verification, not a restore rehearsal')
    output.parent.mkdir(parents=True,exist_ok=True);output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status=result['status'],verifiedFiles=files,issues=issues),indent=2))
    return 1 if issues else 0


if __name__=='__main__':raise SystemExit(main())
