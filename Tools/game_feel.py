"""Bounded source guards, actual Player evidence and incremental recovery point."""
from pathlib import Path
import datetime, hashlib, html, json, re, shutil, sys
R=Path(__file__).resolve().parents[1];D=R/'Docs/QA/GenuinelyFun-20261020';V=R/'Validation/GenuinelyFun-20261020'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def txt(p):return p.read_text(encoding='utf-8-sig')
def dump(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
def sha(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(1048576),b''):h.update(b)
    return h.hexdigest()
def verify():
    allowed={'UI/GameplayPanels.cs','UI/GameplayPanels.Leisure.cs','UI/GameplayPanels.StoryActivities.cs','UI/PlayerCopy.cs','Validation/GameplaySmokeRunner.cs','Validation/GameplaySmokeRunner.StoryLeisure.cs'}
    allowed={'Assets/_Game/Scripts/'+p for p in allowed};checks=[]
    def ck(n,b):checks.append({'name':n,'pass':bool(b)})
    for p in load(D/'baseline.json')['inputs']:
        if p['path'] not in allowed:ck('Preserved '+p['path'],sha(R/p['path'])==p['sha256'])
    before=D/'before-source/Assets/_Game/Scripts'
    a=txt(before/'UI/GameplayPanels.cs');b=txt(R/'Assets/_Game/Scripts/UI/GameplayPanels.cs')
    ck('Only combat presentation changed in shared GameplayPanels',a.split('        public void ShowBattle(',1)[0]==b.split('        public void ShowBattle(',1)[0] and a.split('        void Health(',1)[1]==b.split('        void Health(',1)[1])
    a=txt(before/'UI/PlayerCopy.cs');b=txt(R/'Assets/_Game/Scripts/UI/PlayerCopy.cs')
    strip=lambda s:'\n'.join(x for x in s.splitlines() if not any('case "'+k+'":' in x for k in ['release','fly','bell','wish']))
    ck('Only four leisure instructions changed; journal/quest/item copy retained',strip(a)==strip(b))
    a=txt(before/'UI/GameplayPanels.Leisure.cs');b=txt(R/'Assets/_Game/Scripts/UI/GameplayPanels.Leisure.cs')
    for mechanic in ['seat','pick','water','tea','snack','dance']:
        branch=lambda s:re.search(r'case "'+mechanic+r'":(.*?)(?=\n\s*case |\n\s*})',s[s.index('        void Pose('):],re.S).group(1)
        ck('Preserve approved pose '+mechanic,branch(a)==branch(b))
    art=load(R/'Docs/QA/FinalGameplayAndroid-20261020/art-audio-guards.json')
    for p in art:ck('Art/audio '+p['path'],sha(R/p['path'])==p['sha256'])
    result={'pass':all(c['pass'] for c in checks),'checks':checks};dump(D/'protection.json',result)
    print(json.dumps({'pass':result['pass'],'failed':[c for c in checks if not c['pass']]}))
def evidence():
    rows=[];queued={x['run']:x for x in load(V/'runs.json')} if (V/'runs.json').exists() else {}
    for p in sorted(V.glob('*/smoke-results.txt')):
        row=queued.get(p.parent.name,{}).copy();row.update(run=p.parent.name,result='PASS' if txt(p).rstrip().endswith('ALL CHECKS PASSED') else 'FAIL',file=p.relative_to(R).as_posix(),
            evidence='VERIFIED_ON_WINDOWS_PLAYER + VERIFIED_BY_AUTOMATION',humanBlind=False,build='windows01' if p.parent.name=='hoi-an01' else 'windows03' if p.parent.name=='star-shield02' else 'windows02')
        if p.parent.name=='star-shield01':row['scopeCorrection']='Original harness selected normal G7 stone guard, not star-cycle G18. Validates normal battle only; excluded from shield acceptance. Original log/captures retained.'
        if row.get('seed'):row['seedSHA256']=sha(Path(row['seed']));row['seedProvenance']='Copy of actual earned QA gameplay, no injected unlocks/rewards'
        rows.append(row)
    dump(D/'runs.json',rows)
    selected=[]
    for run in ['hoi-an02','cultural-actions01','star-shield02','boss-two-phase01']:
        folder=V/run
        for p in sorted(folder.glob('*.png')):
            if p.name.startswith('GF-') and ('720' not in p.name or 'lotus' in p.name):selected.append(p)
            elif run in ['star-shield02','boss-two-phase01'] and p.name.startswith(('Story-G18','P5')):selected.append(p)
    selected=list(dict.fromkeys(selected))
    body=['<!doctype html><meta charset="utf-8"><title>LBC · Game feel</title><style>body{font:17px Arial;color:#fff1da;background:#16383c;margin:24px}main{display:flex;gap:18px;flex-wrap:wrap}figure{margin:0 0 24px;width:min(100%,360px)}img{width:100%;height:auto}a{color:#ffd48b}figcaption{min-height:44px}</style><h1>Runtime · Game feel · 20/10/2026</h1><p>Actual Windows02 captures; G18 uses Windows03 (QA itinerary correction only, production identical). Isolated earned/fresh QA saves, scripted mouse/touch. Not a blind or 30-minute human playtest. No new art; Android-device acceptance pending.</p><main>']
    import os
    for p in selected:
        url=Path(os.path.relpath(p,D)).as_posix();body.append('<figure><figcaption>'+html.escape(p.parent.name+' / '+p.stem)+'</figcaption><a href="'+url+'"><img loading="lazy" src="'+url+'"></a></figure>')
    body.append('</main>');(D/'gallery.html').write_text(''.join(body),encoding='utf-8');dump(D/'gallery-files.json',[p.relative_to(R).as_posix() for p in selected])
    files=[]
    for rel in ['Builds/Windows-GameFeel-20261020/LightBetweenCultures.exe','Builds/Windows-GameFeel-20261020/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll','Builds/Android/LightBetweenCultures-GameFeel-20261020.apk']:
        p=R/rel
        if p.exists():files.append({'path':rel,'bytes':p.stat().st_size,'sha256':sha(p),'modifiedUTC':datetime.datetime.fromtimestamp(p.stat().st_mtime,datetime.timezone.utc).isoformat()})
    dump(D/'artifacts.json',{'source':'Latest live after CheckpointFinalGameplayQA-20261020','unity':'6000.6.5f1','android':load(D/'android-build.json') if (D/'android-build.json').exists() else 'PENDING','device':'ANDROID_DEVICE_NOT_TESTED','deviceFPS_RAM_latency':'NOT_MEASURED: no stable device profiling or input-latency instrumentation','windowsPeakSampledWorkingSetBytes':max((x.get('peakWindowsWorkingSetBytes',0) for x in rows),default=0),'files':files})
    changed=[{'path':p['path'],'beforeSHA256':p['sha256'],'afterSHA256':sha(R/p['path'])} for p in load(D/'baseline.json')['inputs'] if sha(R/p['path'])!=p['sha256']]
    for name in ['UI/GameplayPanels.LeisureInput.cs','UI/GameplayPanels.GameFeel.cs','Validation/GameplaySmokeRunner.GameFeel.cs']:
        p=R/'Assets/_Game/Scripts'/name;changed.append({'path':p.relative_to(R).as_posix(),'new':True,'sha256':sha(p)})
    dump(D/'changed-source.json',changed)
    print(json.dumps({'runs':len(rows),'failed':[x['run'] for x in rows if x['result']!='PASS'],'selectedNativeImages':len(selected)}))
def checkpoint(create):
    assert load(D/'protection.json')['pass']
    baseline=load(D/'baseline.json');base=Path(baseline['baseCheckpoint']);m=load(base/'CHECKPOINT_MANIFEST.json')
    assert sha(base/'CHECKPOINT_MANIFEST.json')==baseline['baseManifestSHA256']
    for e in m['entries']:assert sha(base/e['path'])==e['sha256'],e['path']
    for e in m['baseChain']:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
    names=['UI/GameplayPanels.cs','UI/GameplayPanels.Leisure.cs','UI/GameplayPanels.LeisureInput.cs','UI/GameplayPanels.GameFeel.cs','UI/GameplayPanels.StoryActivities.cs','UI/PlayerCopy.cs','Validation/GameplaySmokeRunner.cs','Validation/GameplaySmokeRunner.StoryLeisure.cs','Validation/GameplaySmokeRunner.GameFeel.cs']
    paths={R/'Assets/_Game/Scripts'/x for x in names};paths|={Path(str(p)+'.meta') for p in list(paths) if Path(str(p)+'.meta').exists()}
    paths|={p for p in D.rglob('*') if p.is_file() and p.name!='checkpoint.json'}
    paths|={Path(__file__).resolve(),R/'Tools/game_feel_runs.ps1',R/'Docs/QA/MAKE_GAME_GENUINELY_FUN_20261020.md'}
    for folder in V.iterdir():
        if folder.is_dir():paths|={p for p in folder.iterdir() if p.is_file() and (p.suffix in ['.json','.txt'] or p.name=='player.log')}
        elif folder.suffix=='.json':paths.add(folder)
    paths|={R/p for p in load(D/'gallery-files.json')}
    for name in ['game-feel-windows01.log','game-feel-windows02.log','game-feel-windows03.log','game-feel-android01.log']:paths.add(R/'Logs'/name)
    paths={p for p in paths if p.is_file()};size=sum(p.stat().st_size for p in paths);target=Path('D:/GAMEKKK/Backups/CheckpointGameFeel-20261020')
    print(json.dumps({'files':len(paths),'MiB':round(size/1048576,2),'target':str(target),'basePayloadVerified':True,'baseManifestsVerified':True}))
    if not create:return
    assert not target.exists(),'Never overwrite checkpoints';target.mkdir();entries=[]
    for p in sorted(paths):
        rel=p.relative_to(R);q=target/rel;q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q);h=sha(p);assert sha(q)==h;entries.append({'path':rel.as_posix(),'bytes':p.stat().st_size,'sha256':h})
    guide='INCREMENTAL, REQUIRES BASE. Verify and restore CheckpointFinalGameplayQA-20261020 and its complete chain in an isolated location with Unity closed, then overlay these verified entries. Not a standalone backup. No deleted paths, original art replacements, Library/cache, Player binaries/APKs or normal owner saves. QA saves are evidence only. Rebuild binaries if restoring; complete native frame folders remain in the live project. This is not HUMAN PLAYTEST or Android-device acceptance.'
    (target/'RESTORE.md').write_text(guide,encoding='utf-8')
    dump(target/'CHECKPOINT_MANIFEST.json',{'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(base),'baseManifestSHA256':sha(base/'CHECKPOINT_MANIFEST.json'),
        'baseChain':[{'path':str(base),'manifestSHA256':sha(base/'CHECKPOINT_MANIFEST.json')}]+m['baseChain'],'entries':entries,'bytes':size,'deletedPaths':[],'restoreGuideSHA256':sha(target/'RESTORE.md'),
        'summary':'Direct manipulation of existing lotus/kite/bell/wish props, numeric combat intent/costs and transient response, live stamp-hold feedback. Canonical gameplay, rewards/save, maps/routes, approved pilot poses and art/audio retained. Targeted Windows QA and APK receipt.'})
    dump(D/'checkpoint.json',{'path':str(target),'bytes':size,'files':len(entries),'manifestSHA256':sha(target/'CHECKPOINT_MANIFEST.json'),'baseVerified':True,'payloadVerified':True,'standalone':False,'requiresBase':str(base)})
if __name__=='__main__':
    c=sys.argv[1]
    if c in ['estimate','checkpoint']:checkpoint(c=='checkpoint')
    else:{'verify':verify,'evidence':evidence}[c]()
