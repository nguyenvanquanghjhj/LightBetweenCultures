"""Explicit cleanup allowlist. This script does not delete anything."""
from pathlib import Path
import datetime,json,os,time
from deep_storage_cleanup import ROOT,OUT,walk
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
backup=read(OUT/'source-backup.json');assert backup['verified'] and backup['standalone']
keep=set(read(OUT/'retained-tool-inputs.json'));items=[]
def add(p,category):
    p=Path(p).resolve();assert p.exists()
    st=p.stat();size=st.st_size if p.is_file() else sum(s.st_size for _,s in walk(p))
    items.append(dict(path=str(p),directory=p.is_dir(),bytes=size,mtimeNS=st.st_mtime_ns,category=category))
def prune(folder,category):
    for p in folder.iterdir():
        rel=p.relative_to(ROOT).as_posix()
        if p.is_dir():
            if any(k.startswith(rel+'/') for k in keep):prune(p,category)
            else:add(p,category)
        elif rel not in keep:add(p,category)
prune(ROOT/'Validation','obsolete QA captures/outputs')
prune(ROOT/'Docs/QA','obsolete QA reports/galleries; referenced inputs preserved')
add(ROOT/'Logs','old logs')
for p in (ROOT/'Builds').iterdir():
    if p.name=='Windows-LeisureVisuals-20261020':continue
    if p.name=='Android':
        for apk in p.iterdir():
            if apk.name!='LightBetweenCultures-GameFeel-20261020.apk':add(apk,'old Android builds')
    else:add(p,'old Windows builds')
legacy='CheckpointAndroidApkBuild-20261019' # tiny manifest dependency of route editor exporter
for p in (ROOT.parent/'Backups').iterdir():
    if p.name not in {Path(backup['path']).name,legacy}:add(p,'superseded checkpoints; unique missing source preserved')
for name in ['Bee','PlayerDataCache','BurstCache','BuildHistory','BuildPlayerData']:
    p=ROOT/'Library'/name
    if p.exists():add(p,'reproducible project build cache')
for name in ['Temp','obj','build','.utmp','Tools/__pycache__']:
    p=ROOT/name
    if p.exists():add(p,'reproducible project temporary output')
p=ROOT/'Submission/KGW2026/LightBetweenCultures_Windows_20261020.zip'
if p.exists():add(p,'obsolete packaged Windows build')
for p in ROOT.iterdir():
    if p.is_file() and (p.name.endswith(('Report.md','Review.html')) or p.name=='AUDIT_VI.md'):add(p,'obsolete root QA reports')
# Known Unity-generated compiler temporaries; never general Temp or shared caches.
temp=Path(os.environ['LOCALAPPDATA'])/'Temp';cutoff=time.time()-3600
for p in temp.glob('UnityVBCSPreWarm_*'):
    if p.is_file() and p.stat().st_mtime<cutoff:add(p,'stale Unity compiler temporary')
p=temp/'UnityCBMCrashes'
if p.is_dir():
    for f,_ in walk(p):
        if f.stat().st_mtime<cutoff:add(f,'old Unity crash diagnostics')
for p in (Path(os.environ['LOCALAPPDATA'])/'CrashDumps').glob('Unity.ILPP.Trigger.exe.*.dmp'):
    if p.stat().st_mtime<cutoff:add(p,'old Unity ILPP compiler crash dump')
summary={}
for e in items:summary[e['category']]=summary.get(e['category'],0)+e['bytes']
plan=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),sourceBackup=backup,keepWindows=str(ROOT/'Builds/Windows-LeisureVisuals-20261020'),keepAPK=str(ROOT/'Builds/Android/LightBetweenCultures-GameFeel-20261020.apk'),retainedLegacyToolDependency=str(ROOT.parent/'Backups'/legacy),items=items,bytes=sum(e['bytes'] for e in items),categories=summary)
(OUT/'delete-plan.json').write_text(json.dumps(plan,indent=2)+'\n',encoding='utf-8')
print('Planned:',len(items),'entries,',round(plan['bytes']/1024**3,3),'GiB')
for k,v in summary.items():print(round(v/1024**3,3),k)
