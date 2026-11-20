"""Compare a remote clone to local committed source, including filtered text and binary bytes."""
import argparse
import collections
import hashlib
import json
from pathlib import Path
import subprocess

PROJECT=Path(__file__).resolve().parents[2]


def git(root,*args,input=None):
    return subprocess.run(['git','-c','safe.directory='+root.as_posix(),'-C',str(root),*args],input=input,capture_output=True,check=True).stdout


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--clone',required=True,type=Path)
    parser.add_argument('--output',required=True,type=Path)
    args=parser.parse_args();clone=args.clone.resolve()
    commit=git(clone,'rev-parse','HEAD').decode().strip()
    rows=[]
    for line in git(PROJECT,'ls-tree','-r','-z',commit).decode().split('\0'):
        if not line:continue
        header,path=line.split('\t',1)
        if path.startswith(('Assets/','Packages/','ProjectSettings/')):rows.append((path,header.split()[2]))
    input=('\n'.join(str(clone/path).replace('\\','/') for path,_ in rows)+'\n').encode('utf-8')
    hashes=git(clone,'hash-object','--stdin-paths',input=input).decode().splitlines()
    mismatches=[path for (path,expected),actual in zip(rows,hashes,strict=True) if expected!=actual]
    binaries=[]
    for path,_ in rows:
        if Path(path).suffix.lower() in {'.png','.jpg','.jpeg','.wav','.ogg','.mp3','.ttf','.otf','.psd'}:
            with (PROJECT/path).open('rb') as stream:a=hashlib.file_digest(stream,'sha256').hexdigest()
            with (clone/path).open('rb') as stream:b=hashlib.file_digest(stream,'sha256').hexdigest()
            binaries.append(dict(path=path,sha256=b,match=a==b))
    groups=dict(collections.Counter(Path(path).suffix for path,_ in rows))
    result=dict(status='PASS' if not mismatches and all(r['match'] for r in binaries) else 'FAIL',commit=commit,clone=str(clone),requiredFiles=len(rows),extensions=groups,committedBlobMismatches=mismatches,binarySHA256=binaries,note='Git filtered blob identity covers text line endings; binary SHA256 is byte-for-byte against original. Unity-generated files are excluded.')
    args.output.parent.mkdir(parents=True,exist_ok=True);args.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(status=result['status'],commit=commit,requiredFiles=len(rows),groups=groups,mismatches=mismatches,binaries=len(binaries))))


if __name__=='__main__':main()
