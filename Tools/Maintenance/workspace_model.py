"""Shared filesystem evidence and relocation lookup. No Unity behavior is emulated here."""
import hashlib
import json
import re
import subprocess
from functools import lru_cache
from pathlib import Path

PROJECT = Path(__file__).resolve().parents[2]
WORKSPACE = PROJECT.parent
EVIDENCE = PROJECT / 'Docs/Development/Storage/WorkspaceRestructure-20261113'


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def sha(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream,'sha256').hexdigest()


@lru_cache(maxsize=1)
def moves():
    rows=[]
    for name in ('move-result.json','asset-moves.json','retired-pipelines.json','marker-moves.json','recovery-archive-moves.json','unused-input-moves.json'):
        if (EVIDENCE/name).exists():
            rows.extend(load(EVIDENCE/name))
    return rows


@lru_cache(maxsize=1)
def recorded_workspace():
    path=EVIDENCE/'inventory.json'
    return load(path)['workspace'].replace('\\','/').rstrip('/')+'/' if path.exists() else None


def resolve(key):
    key=str(key).replace('\\','/')
    prefixes=[WORKSPACE.as_posix()+'/']
    if recorded_workspace():
        prefixes.append(recorded_workspace())
    for prefix in prefixes:
        if key.lower().startswith(prefix.lower()):
            key=key[len(prefix):];break
    pairs=sorted([(r['old'],r['new']) for r in moves() if r['old']!=r['new']],key=lambda p:len(p[0]),reverse=True)
    seen=set()
    while key not in seen:
        seen.add(key)
        changed=False
        for old,new in pairs:
            if key==old or key.startswith(old+'/'):
                key=new+key[len(old):];changed=True;break
        if not changed:break
    return WORKSPACE/key


def write_report(name, checks):
    report=dict(status='PASS' if all(r['status']=='PASS' for r in checks) else 'FAIL',checks=checks)
    (EVIDENCE/name).write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    for row in checks:
        print(row['id']+': '+row['status']+' '+str(row.get('detail','')))
        for violation in row.get('violations',[]):print('  '+str(violation))
    return 0 if report['status']=='PASS' else 1


def check(code, violations, detail=''):
    return dict(id=code,status='FAIL' if violations else 'PASS',detail=detail,violations=violations)


def active_path_violations():
    violations=[]
    aliases={r['oldPath']:r['newPath'] for r in load(PROJECT/'Docs/Art/asset-moves.json')['entries']}
    # Literal file reads can be checked without pretending to understand every dynamic C# path.
    for path in (PROJECT/'Assets/_Game/Scripts').rglob('*.cs'):
        source=path.read_text(encoding='utf-8-sig')
        for ref in re.findall(r'File\.ReadAll(?:Text|Bytes)\("([^"]+)"\)',source):
            candidate=PROJECT/ref
            if not candidate.exists():violations.append(path.relative_to(PROJECT).as_posix()+': '+ref)
    for path in (PROJECT/'Tools').rglob('*.py'):
        if 'Maintenance' in path.relative_to(PROJECT/'Tools').parts:continue
        source=path.read_text(encoding='utf-8-sig')
        for ref in re.findall(r'[\'"](Assets/[^\'"\n]+\.(?:json|png|jpg|asset|unity|cs))[\'"]',source):
            if not (PROJECT/ref).exists() and not (PROJECT/aliases.get(ref,ref)).exists():
                violations.append(path.relative_to(PROJECT).as_posix()+': '+ref)
        if re.search(r'D:[/\\]+GAMEKKK',source,re.I):violations.append(path.relative_to(PROJECT).as_posix()+': hardcoded original workspace')
    return violations


def git(*args):
    return subprocess.run(['git','-c','safe.directory='+PROJECT.as_posix(),'-C',str(PROJECT),*args],capture_output=True,text=True)
