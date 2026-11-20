"""Prepare a verified source restore point and an explicit deletion allowlist."""
from pathlib import Path
import argparse, collections, datetime, hashlib, json, os, re, shutil
from deep_storage_cleanup import ROOT,OUT,walk
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(4*1024**2),b''):h.update(b)
    return h.hexdigest()
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
parser=argparse.ArgumentParser();parser.add_argument('--backup',action='store_true');args=parser.parse_args()
inventory=read(OUT/'inventory.json')
keep=set()
for p,_ in walk(ROOT/'Tools'):
    if p.suffix.lower() not in {'.py','.ps1','.cs','.js','.json','.html'}:continue
    for text in re.findall(r'''["']((?:Docs/QA|Validation)/[^"'\r\n]+)["']''',p.read_text('utf-8-sig',errors='replace')):
        if (ROOT/text).is_file() and Path(text).suffix.lower() in {'.json','.csv'}:keep.add(text.replace('\\','/'))
for p in [ROOT/'Docs/QA/Phase7/bot-alpha-plan.json',ROOT/'Docs/QA/Phase6-VisualAndroid/crop-outline-plan.json',ROOT/'Docs/QA/MapClarity-20261019/import-selection.json']:
    if p.exists():keep.add(p.relative_to(ROOT).as_posix())
for e in read(ROOT/'Docs/QA/Phase7/bot-alpha-plan.json'):
    if (ROOT/e['preview']).exists():keep.add(e['preview'])
# Actual route authoring inputs, rather than screenshots of old test runs.
for folder in ['Validation/OwnerRouteRedesign-20261019','Validation/OwnerRoutePhaseB-20261019','Validation/SixCityRouteEditor-20261019','Validation/RouteMarkupExport-20261018','Validation/Art']:
    for p in (ROOT/folder).iterdir():
        if p.is_file() and (p.suffix.lower() in {'.json','.csv'} or p.name.endswith('-reference.png') or folder.endswith('/Art')):keep.add(p.relative_to(ROOT).as_posix())
# Preserve all save-shaped records; normal LocalLow player storage is never touched.
for p,_ in walk(ROOT/'Validation'):
    if p.suffix.lower() in {'.sav','.save','.jks','.keystore'} or (p.suffix.lower()=='.json' and re.search(r'save|earned',p.name,re.I)):keep.add(p.relative_to(ROOT).as_posix())
dump(OUT/'retained-tool-inputs.json',sorted(keep))
excluded={'Library','Temp','obj','Logs','Builds','build','Validation','.utmp','Docs'}
source={}
for top in ROOT.iterdir():
    if top.is_dir() and top.name not in excluded:
        for p,st in walk(top):
            isBuildZip=p.suffix.lower()=='.zip' and any(t in p.name.lower() for t in ['windows','build','checkpoint','backup'])
            if '__pycache__' not in p.parts and not isBuildZip and p.suffix.lower() not in {'.apk','.aab','.exe','.log','.pyc'}:source[p.relative_to(ROOT).as_posix()]=p
    elif top.is_file() and top.suffix.lower() in {'.md','.txt','.json','.gitignore'}:source[top.name]=top
for p,st in walk(ROOT/'Docs'):
    rel=p.relative_to(ROOT).as_posix()
    if not rel.startswith(('Docs/QA/','Docs/Storage/')):source[rel]=p
for rel in keep:source[rel]=ROOT/rel
source.pop('Tools/deep_storage_cleanup.py',None);source.pop('Tools/deep_storage_plan.py',None)
# Scan old source paths, not cache/build/QA payload. Missing unique source is saved
# once per content hash before any old checkpoint can be removed.
backupRoot=ROOT.parent/'Backups';unique={};oldSourceFiles=0
historicalRoots={'Assets','Packages','ProjectSettings','Tools','References','_CheckIn_Import_20261018','LBC_AnhMoi_BanGiaoCodex_20261015','LBC_Gameplay_6Cities_Detailed_v1','LBC_HoiAn_StoryExpansion_v1','LightBetweenCultures_DesignKit_v1'}
for checkpoint in backupRoot.iterdir():
    if not checkpoint.is_dir() or checkpoint.name.startswith('LBC_SourceVerified-'):continue
    for top in checkpoint.iterdir():
        if not top.is_dir() or top.name not in historicalRoots:continue
        for p,st in walk(top):
            rel=p.relative_to(checkpoint).as_posix();oldSourceFiles+=1
            if '__pycache__' in p.parts or (ROOT/rel).exists():continue
            digest=sha(p)
            if digest not in unique:unique[digest]=dict(path=str(p),originalPath=rel,bytes=st.st_size,sha256=digest)
print('Current source:',len(source),'files,',round(sum(p.stat().st_size for p in source.values())/1024**2,2),'MiB')
print('Historical missing source:',len(unique),'unique contents,',round(sum(e['bytes'] for e in unique.values())/1024**2,2),'MiB')
dest=backupRoot/'LBC_SourceVerified-20261020'
dump(OUT/'source-backup-plan.json',dict(destination=str(dest),files=len(source),bytes=sum(p.stat().st_size for p in source.values()),historicalUnique=list(unique.values()),oldSourcePathsInspected=oldSourceFiles))
if not args.backup:raise SystemExit(0)
assert not dest.exists();dest.mkdir()
payload=[]
for rel,p in sorted(source.items()):
    target=dest/'Project'/rel;target.parent.mkdir(parents=True,exist_ok=True)
    digest=sha(p);shutil.copy2(p,target);assert sha(target)==digest and sha(p)==digest
    payload.append(dict(path='Project/'+rel,sha256=digest,bytes=p.stat().st_size))
for digest,e in unique.items():
    p=Path(e['path']);rel='HistoricalUnique/'+digest[:16]+'/'+e['originalPath'];target=dest/rel;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,target);assert sha(target)==digest
    payload.append(dict(path=rel,sha256=digest,bytes=e['bytes']))
dump(dest/'SOURCE_MANIFEST.json',dict(type='STANDALONE_SOURCE_AND_ASSET_RESTORE_POINT',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),entries=payload,bytes=sum(e['bytes'] for e in payload),historicalUnique=list(unique.values()),excludes='Library/Temp/obj/Builds/logs/QA captures; no external base required'))
(dest/'RESTORE.md').write_text('Standalone source restore point. Copy Project contents to an empty Unity project folder, then open with Unity version in ProjectSettings/ProjectVersion.txt. Assets, metadata, Packages and ProjectSettings are complete. Library/build caches regenerate. HistoricalUnique contains preserved files absent from the live source; do not merge those into Assets automatically. This snapshot contains no build/cache/log payload. Original live LocalLow saves and signing credentials were not modified.\n',encoding='utf-8')
dump(OUT/'source-backup.json',dict(path=str(dest),manifestSHA256=sha(dest/'SOURCE_MANIFEST.json'),files=len(payload),bytes=sum(e['bytes'] for e in payload),standalone=True,verified=True))
print('Verified standalone source backup:',dest)
