"""Validate project structure; require fresh actual Unity audit for serialized-reference gate."""
import json
import re
from pathlib import Path
from workspace_model import PROJECT, WORKSPACE, EVIDENCE, load, sha, resolve, check, write_report, active_path_violations, git


def native_violations():
    selection=EVIDENCE/'native-selection.json'
    if not selection.exists():return ['No selected successful current Unity audit; Python cannot certify serialized references']
    selected=load(selection);directory=PROJECT/selected['directory']
    if selected.get('editorExitCode')!=0:return ['Unity Editor did not exit successfully']
    errors=[]
    before=load(EVIDENCE/'native-before/native-audit.json');after=load(directory/'native-audit.json')
    for row in load(directory/'input-fingerprints.json')['files']:
        path=PROJECT/row['path']
        if not path.is_file() or sha(path)!=row['sha256']:errors.append('Stale native evidence: '+row['path'])
    for key in ('missingGuids','missingScripts','missingObjectReferences'):
        old={(str(resolve(PROJECT.name+'/'+r['consumer'])),r['reference']) for r in before[key]}
        new={(str(PROJECT/r['consumer']),r['reference']) for r in after[key]}
        if new-old:errors.extend(key+': '+str(r) for r in new-old)
    new_by_guid={r['guid']:r for r in after['assets']}
    for old in before['assets']:
        new=new_by_guid.get(old['guid'])
        if new is None:
            if old['runtimeDependency']:errors.append('Lost runtime dependency: '+old['path'])
            continue
        if old.get('sprites'):
            for field in ('sprites','android','standalone'):
                if old[field]!=new[field]:errors.append('Changed '+field+': '+old['path'])
    if load(directory/'catalog-result.json')['status']!='PASS':errors.append('Native catalog check did not pass')
    if after['saveAssertions']!=4:errors.append('Legacy save regression missing')
    return errors


def main():
    checks=[]
    allowed={'Assets','Docs','Tools','Packages','ProjectSettings','AGENTS.md','README.md','.gitignore','.git','.vscode',
             'Library','Temp','Logs','UserSettings','obj','.utmp','build','Assembly-CSharp.csproj','Assembly-CSharp-Editor.csproj','LightBetweenCultures.slnx'}
    checks.append(check('PR-001',[p.name for p in PROJECT.iterdir() if p.name not in allowed]))
    extensions={'.png','.jpg','.jpeg','.webp','.psd','.svg'}
    images=[p for p in (PROJECT/'Assets').rglob('*') if p.suffix.lower() in extensions]
    bad=[p.relative_to(PROJECT).as_posix() for p in images if not p.is_relative_to(PROJECT/'Assets/_Game/Art')]
    checks.append(check('PR-002',bad,'image files under Assets: '+str(len(images))))
    checks.append(check('PR-003',['Resources/Art still exists'] if (PROJECT/'Assets/_Game/Resources/Art').exists() else []))
    banned={'LegacyReferences','LegacyDesignKit','SourceReferences','Source','Before','Backup','Rejected','Unused'}
    checks.append(check('PR-004',[p.relative_to(PROJECT).as_posix() for p in (PROJECT/'Assets/_Game/Art').rglob('*') if p.is_dir() and p.name in banned]))
    imports=[]
    for p in (PROJECT/'Assets/_Game/Scripts').rglob('*.cs'):
        source=p.read_text(encoding='utf-8-sig')
        if re.search(r'Resources\.Load<(Sprite|Texture2D)>\(',source):imports.append(p.relative_to(PROJECT).as_posix()+': texture/sprite Resources loader remains')
        if 'Lumi_DialoguePortrait.png' in source or 'Lumi_ExplorationIdle.png' in source:imports.append(p.relative_to(PROJECT).as_posix()+': legacy Lumi input remains')
    checks.append(check('PR-005',imports))
    data_errors=[]
    for path in (PROJECT/'Assets/_Game/Resources').rglob('*.json'):
        try:load(path)
        except (ValueError,OSError) as e:data_errors.append(str(path)+': '+str(e))
    catalog=(PROJECT/'Assets/_Game/Resources/Catalogs/GameArtCatalog.asset').read_text(encoding='utf-8-sig')
    ids=set(re.findall(r'^  - id: (.+)$',catalog,re.M))
    if len(ids)!=12:data_errors.append('Expected twelve distinct prop IDs')
    for entry in load(PROJECT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']:
        if entry['spritePath'] not in ids:data_errors.append('Invalid leisure ID: '+entry['spritePath'])
    cat=(PROJECT/'Assets/_Game/Resources/Content/CatRescue.json').read_text(encoding='utf-8-sig')
    if 'resource:' in cat:data_errors.append('Old resource path in CatRescue data')
    for value in re.findall(r'"leisure:([^"]+)"',cat):
        if value not in ids:data_errors.append('Invalid CatRescue prop: '+value)
    checks.append(check('PR-006',data_errors))
    try:errors=native_violations()
    except (OSError,ValueError,KeyError) as e:errors=['Native verification unavailable: '+str(e)]
    checks.append(check('PR-007',errors,'fresh actual Unity audit and importer/GUID comparison required'))
    builds=[]
    for rel in ('Tools/Build/build.py','Tools/Build/verify_apk.py','Assets/_Game/Scripts/Editor/AndroidApkBuild.cs'):
        if not (PROJECT/rel).exists():builds.append(rel+' missing')
    builds+=active_path_violations()
    for p in (PROJECT/'Tools').rglob('*.py'):
        try:compile(p.read_text(encoding='utf-8-sig'),str(p),'exec')
        except (SyntaxError,ValueError) as e:builds.append(str(e))
    checks.append(check('PR-008',builds,'build entry points, literal inputs and Python syntax; build execution reported separately'))
    checks.append(check('PR-009',[p.name for p in PROJECT.iterdir() if p.name in ('Backups','Builds','Validation','References','ReviewPacks','Submission') or p.name.startswith(('LBC_','_LBC_'))]))
    ignore=[]
    for path in ('Library/probe','Temp/probe','Logs/probe','UserSettings/probe','obj/probe','.utmp/probe','build/probe','Example.csproj','Example.slnx','Tools/QA/__pycache__/x.pyc'):
        if git('check-ignore','--no-index','-q',path).returncode!=0:ignore.append('Generated pattern not ignored: '+path)
    if git('check-ignore','--no-index','-q','Tools/QA/CatRescueTests/CatRescueTests.csproj').returncode==0:ignore.append('Authored regression project incorrectly ignored')
    tracked=git('ls-files','*.csproj','*.slnx','*__pycache__*').stdout.splitlines()
    ignore += ['Generated file still tracked: '+p for p in tracked if p!='Tools/QA/CatRescueTests/CatRescueTests.csproj' and p!='Tools/CatRescueTests/CatRescueTests.csproj']
    checks.append(check('PR-010',ignore))
    return write_report('canonical-validation.json',checks)


if __name__=='__main__':raise SystemExit(main())
