"""Bounded current-build QA receipts; never modifies production data or user saves."""
from pathlib import Path
import datetime, hashlib, html, json, re, shutil, sys, zipfile

R=Path(__file__).resolve().parents[1]
D=R/'Docs/QA/FinalGameplayAndroid-20261020'
V=R/'Validation/FinalGameplayAndroid-20261020'
B=Path('D:/GAMEKKK/Backups/CheckpointOwnerConfusionUX-20261020')

def sha(p):
    with p.open('rb') as f:
        h=hashlib.sha256()
        for part in iter(lambda:f.read(1048576),b''): h.update(part)
    return h.hexdigest()
def load(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,v):
    p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
def inputs():
    paths=set((R/'Assets/_Game/Scripts').rglob('*.cs'))
    paths|=set((R/'Assets/_Game/Resources/Content').glob('*.json'))
    paths|=set((R/'ProjectSettings').glob('*.asset'))
    paths|={R/'Packages/manifest.json',R/'Packages/packages-lock.json',R/'Assets/_Game/Resources/AdventureUIArt.asset'}
    return sorted(p for p in paths if p.is_file())
def freeze():
    suffix='-retention' if '--retention' in sys.argv else ''
    p=D/('source-freeze'+suffix+'.json')
    assert not p.exists(),'Do not rewrite a frozen QA baseline'
    dump(p,{'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),
        'baseCheckpoint':str(B),'baseManifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json'),
        'inputs':[{'path':x.relative_to(R).as_posix(),'bytes':x.stat().st_size,'sha256':sha(x)} for x in inputs()],
        'scope':'Relevant C# source, runtime JSON, UI binding and settings only; no Library/history/full-project scan',
        'productionDelta':['MainMenu: correct saved-city continuation copy','CulturalTrials: remove one overlapping decorative Hanji label']+(['PlayerCopy: show active quests before compact completed history; read-only presentation'] if suffix else []),
        'qaDelta':'CasualRuntime optional crab itinerary corrected H11 -> H12; no gameplay flags injected'})
    if not (D/'APPROVED_REQUEST.md').exists():shutil.copy2(Path('C:/Users/admin/.codex/attachments/6a50aa86-ab5a-495d-8716-99df902c698f/Pasted text.txt'),D/'APPROVED_REQUEST.md')
    print('Frozen',len(load(p)['inputs']),'bounded source/settings inputs')
def verify():
    frozen=load(D/('source-freeze-retention.json' if (D/'source-freeze-retention.json').exists() else 'source-freeze.json'))
    changed=[x['path'] for x in frozen['inputs'] if sha(R/x['path'])!=x['sha256']]
    old=load(B/'CHECKPOINT_MANIFEST.json')
    allowed={'Assets/_Game/Scripts/Maps/MainMenu.cs','Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CasualRuntime.cs','Assets/_Game/Scripts/UI/PlayerCopy.cs'}
    protected=[x['path'] for x in old['entries'] if x['path'].startswith('Assets/') and x['path'] not in allowed and sha(R/x['path'])!=x['sha256']]
    before=(B/'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CasualRuntime.cs').read_text(encoding='utf-8-sig')
    after=(R/'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CasualRuntime.cs').read_text(encoding='utf-8-sig')
    itineraryOnly=before.replace('ChapterGo("H11");Time.timeScale=1;', 'ChapterGo("H12");Time.timeScale=1;')==after
    art=[]
    for e in load(R/'Docs/QA/CasualAdventure-20261020/baseline.json')['entries']:
        if e['path'].endswith(('.png','.mp3','.wav','.meta')):
            art.append({'path':e['path'],'sha256':sha(R/e['path']),'unchanged':sha(R/e['path'])==e['sha256']})
    dump(D/'art-audio-guards.json',art)
    before=(D/'before-retention/PlayerCopy.cs').read_text(encoding='utf-8-sig')
    after=(R/'Assets/_Game/Scripts/UI/PlayerCopy.cs').read_text(encoding='utf-8-sig')
    span=lambda s:s.split('        public static string Journal(',1)[0]+s.split('        public static string LeisureHelp(',1)[1]
    result={'frozenInputsUnchanged':not changed,'changedSinceFreeze':changed,'ownerCheckpointProtectedInputsUnchanged':not protected,'unexpectedProtectedChanges':protected,'qaOnlyCrabNodeCorrection':itineraryOnly,'journalOnlyPresentationSectionChanged':span(before)==span(after),'knownArtAndAudioUnchanged':all(x['unchanged'] for x in art)}
    dump(D/'protection.json',result);print(json.dumps(result))

def evidence():
    runs=[]
    queues={}
    for name in ['queue-results.json','followup-results.json','retention-results.json']:
        p=V/name
        if p.exists():
            for item in load(p):queues[item['run']]=item
    for folder in sorted(p for p in V.iterdir() if p.is_dir()):
        candidates=[folder/'smoke-results.txt',folder/'restart-results.txt']
        result=next((p for p in candidates if p.exists()),None)
        if result is None:continue
        s=result.read_text(encoding='utf-8-sig');row=queues.get(folder.name,{}).copy()
        row.update(run=folder.name,result='PASS' if s.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL',results=result.relative_to(R).as_posix(),
            evidenceType=['VERIFIED_ON_WINDOWS_PLAYER','VERIFIED_BY_AUTOMATION'],build='windows02-retention' if folder.name.startswith('retention-') else 'windows01-pre-journal-sorting',
            historicalForFinalBuild=not folder.name.startswith('retention-'),blindHuman=False)
        if row.get('status')=='BLOCKED_NO_RESULT' and row['result']=='PASS':row['classificationNote']='Launcher looked for smoke-results.txt; this restart correctly writes restart-results.txt. Original queue receipt retained.'
        if row.get('seed'):
            seed=Path(row['seed']);row['seedSHA256']=sha(seed);row['seedOrigin']='Earned isolated gameplay save; no injected flags'
        runs.append(row)
    dump(D/'execution-results.json',runs)
    artifacts=[]
    for rel in ['Builds/Windows-FinalGameplayQA-20261020/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll',
        'Builds/Windows-FinalGameplayRetention-20261020/LightBetweenCultures.exe','Builds/Windows-FinalGameplayRetention-20261020/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll',
        'Builds/Android/LightBetweenCultures-FinalGameplay-20261020.apk']:
        p=R/rel
        if p.exists():artifacts.append({'path':rel,'bytes':p.stat().st_size,'sha256':sha(p),'modifiedUTC':datetime.datetime.fromtimestamp(p.stat().st_mtime,datetime.timezone.utc).isoformat()})
    dump(D/'build-artifacts.json',{'unity':'6000.6.5f1','appVersion':'1.0','androidVersionCode':1,'uniqueBuildIdentity':'File name + Assembly-CSharp/APK SHA256 + immutable source freezes; not a versionCode migration',
        'windowsBuilds':['Logs/final-gameplay-windows01.log','Logs/final-gameplay-windows02.log'],
        'androidBuild':load(D/'android-build.json') if (D/'android-build.json').exists() else 'PENDING',
        'androidDevice':'ANDROID_DEVICE_NOT_TESTED: no connected adb device','files':artifacts})
    content=load(R/'Assets/_Game/Resources/Content/GameplayContent.json');activity=load(R/'Assets/_Game/Resources/Content/StoryInteractions.json')
    campaigns={}
    for name in ['fresh-vietnam','fresh-korea']:
        save=load(V/name/'gameplay-save.json')
        campaigns[name]={'start':'Fresh private save; scripted mouse/touch journey; accelerated walking; not human elapsed playtime',
            'canonicalQuestsCompleted':[q['id'] for q in content['quests'] if q['id'] in save['completed']],
            'solvedPuzzles':save['solvedPuzzles'],'notReplayedPuzzles':[q['id'] for q in content['puzzles'] if q['id'] not in save['solvedPuzzles']],
            'restoredRelics':[x for x in save['flags'] if x.startswith('relic.restored.')],
            'visitedByCity':{x['cityId']:len(x['visited']) for x in save['cityProgress']},
            'bossResolved':'seoul.final_cleared' in save['flags'],'endingFlags':[x for x in save['flags'] if 'ending' in x or 'connection' in x]}
    dump(D/'campaign-coverage.json',campaigns)
    apk=R/'Builds/Android/LightBetweenCultures-FinalGameplay-20261020.apk'
    if apk.exists():
        with zipfile.ZipFile(apk) as z:
            groups={}
            for x in z.infolist():
                key='lib' if x.filename.startswith('lib/') else 'assets' if x.filename.startswith('assets/') else 'resources+manifest+other'
                groups[key]=groups.get(key,0)+x.compress_size
            dump(D/'apk-size.json',{'actualAPKBytes':apk.stat().st_size,'MiB':round(apk.stat().st_size/1048576,2),'compressedGroupsBytes':groups,
                'largestEntries':[{'path':x.filename,'compressedBytes':x.compress_size,'uncompressedBytes':x.file_size} for x in sorted(z.infolist(),key=lambda x:x.compress_size,reverse=True)[:15]],
                'limit':'APK grouping cannot reliably attribute packed Unity assets to individual source art; no assets stripped during QA'})
    gallery=[]
    selected=[]
    for node in ['H9','H16','U0','U8','D13','D9','J13','J15','G10','G11','S1','S7']:
        pattern='FULL-'+node+'-540-motion-'+('2.4' if node=='J15' else '0.6')+'.png'
        selected.append(V/'leisure12-native'/pattern)
    for prefix in ['R1-boat','R2-boat','R3-boat','R4-boat','R5-boat','X1-branch-arrival','X3-branch-arrival','X4-branch-arrival']:
        selected.append(V/'routes-boats-junctions'/(prefix+'.png'))
    selected += [V/'owner-fresh'/(x+'.png') for x in ['Owner-fragment-markers-start','Owner-four-fragments-next-step','Owner-assembly-H7-required']]
    selected += [V/'owner-seal/Owner-seal-inventory.png',V/'npc-actors/NPC-H10-map.png',V/'npc-actors/NPC-H14-map.png',V/'photo-album18/Restart-album-seoul.png',
        V/'retention-journal/Retention-journal-active-first-540.png',V/'retention-journal/Retention-journal-active-first-720.png',V/'retention-journal/Retention-journal-cross-city.png',
        V/'retention-menu-restart/Contest-continue-menu.png',V/'retention-fresh-core/Casual-resume-journal.png']
    selected += [R/'Validation/ContestReadiness-20261020'/run/'P4-jeonju-trial-correct-before-confirm.png' for run in ['signature-jeonju','signature-jeonju-final']]
    selected += [V/'fresh-vietnam/P5-credits.png',V/'fresh-korea/P5-credits.png']
    selected=[x for x in selected if x.exists()]
    for p in selected:
        url=Path(__import__('os').path.relpath(p,D)).as_posix()
        gallery.append('<figure><figcaption>'+html.escape(p.parent.name+' / '+p.stem)+'</figcaption><a href="'+url+'"><img loading="lazy" src="'+url+'"></a></figure>')
    (D/'gallery.html').write_text('<!doctype html><meta charset="utf-8"><title>LBC Gameplay QA</title><style>body{background:#15373b;color:#fff0d5;font:17px Arial;margin:24px}main{display:flex;gap:20px;flex-wrap:wrap}figure{width:min(100%,360px);margin:0 0 24px}img{width:100%}figcaption{min-height:45px}a{color:#ffd48b}</style><h1>Gameplay QA · 20/10/2026</h1><p>Actual Windows Player captures. Automated journeys, not human playtests. Android device NOT_TESTED. Images retain their native originals.</p><p>Two campaign runs, leisure and route/NPC probes use Windows01 before journal sorting; retention captures use Windows02. The two Hanji pictures use the earlier contest build before/after the decorative-label fix. Other images are separate test states, not matched before/after pairs.</p><main>'+''.join(gallery)+'</main>',encoding='utf-8')
    dump(D/'gallery-files.json',[p.relative_to(R).as_posix() for p in selected])
    print(json.dumps({'runs':len(runs),'failed':[x['run'] for x in runs if x['result']!='PASS'],'galleryImages':len(selected),'buildArtifacts':len(artifacts)}))

def checkpoint(create):
    protection=load(D/'protection.json')
    assert all(protection[k] for k in ['frozenInputsUnchanged','ownerCheckpointProtectedInputsUnchanged','qaOnlyCrabNodeCorrection','journalOnlyPresentationSectionChanged','knownArtAndAudioUnchanged'])
    old=load(B/'CHECKPOINT_MANIFEST.json');assert sha(B/'CHECKPOINT_MANIFEST.json')=='c89dcdabd3ea5b98f0265131c36c58b37e826d6ea72d4b82b1f3ce2f6aeda354'
    for x in old['entries']:assert sha(B/x['path'])==x['sha256'],x['path']
    for x in old['baseChain']:assert sha(Path(x['path'])/'CHECKPOINT_MANIFEST.json')==x['manifestSHA256']
    names=['Maps/MainMenu.cs','UI/GameplayPanels.CulturalTrials.cs','UI/PlayerCopy.cs','Validation/GameplaySmokeRunner.cs','Validation/GameplaySmokeRunner.CasualRuntime.cs','Validation/GameplaySmokeRunner.Contest.cs']
    paths={R/'Assets/_Game/Scripts'/x for x in names};paths|={Path(str(p)+'.meta') for p in list(paths) if Path(str(p)+'.meta').exists()}
    paths|={p for p in D.rglob('*') if p.is_file() and p.name!='checkpoint.json'}
    paths|={R/'Tools/final_game_qa_runs.ps1',Path(__file__).resolve(),R/'Docs/QA/FINAL_COMPREHENSIVE_GAMEPLAY_ANDROID_QA_20261020.md'}
    for name in ['final-gameplay-windows01.log','final-gameplay-windows02.log','final-gameplay-android01.log']:paths.add(R/'Logs'/name)
    for folder in V.iterdir():
        if folder.is_dir():paths|={p for p in folder.iterdir() if p.suffix in ['.json','.txt'] or p.name=='player.log'}
        elif folder.suffix=='.json':paths.add(folder)
    paths|={R/p for p in load(D/'gallery-files.json')}
    for p in ['Maps/MainMenu.cs','UI/GameplayPanels.CulturalTrials.cs']:
        paths.add(R/'Docs/Contest/KGW2026/before-source/Assets/_Game/Scripts'/p)
    paths={p for p in paths if p.is_file()};size=sum(p.stat().st_size for p in paths)
    target=Path('D:/GAMEKKK/Backups/CheckpointFinalGameplayQA-20261020')
    print(json.dumps({'files':len(paths),'MiB':round(size/1048576,2),'basePayloadVerified':True,'baseChainManifestsVerified':True,'target':str(target)}),flush=True)
    if not create:return
    assert not target.exists(),'Never overwrite checkpoints'
    target.mkdir();entries=[]
    for p in sorted(paths):
        rel=p.relative_to(R);dest=target/rel;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest);h=sha(p);assert sha(dest)==h
        entries.append({'path':rel.as_posix(),'sha256':h,'bytes':p.stat().st_size})
    guide='INCREMENTAL, NOT STANDALONE. Requires CheckpointOwnerConfusionUX-20261020 and its complete base chain. Verify hashes and restore the chain in an isolated location with Unity closed, then overlay manifest entries. No deletions. No Library/cache, APK, full Player binaries or normal user saves. Included Validation saves are isolated QA evidence, never replacements for owner saves. Selected acceptance images are included; the complete current run frame folders remain in the live project. Rebuild binaries from restored source as needed. Android device and independent human 5/15/30-minute acceptance are still pending.'
    (target/'RESTORE.md').write_text(guide,encoding='utf-8')
    dump(target/'CHECKPOINT_MANIFEST.json',{'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(B),'baseManifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json'),
        'baseChain':[{'path':str(B),'manifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json')}]+old['baseChain'],'entries':entries,'bytes':size,'deletedPaths':[],'restoreGuideSHA256':sha(target/'RESTORE.md'),
        'summary':'Saved-city continuation caption, Hanji overlap removal, active-first compact quest journal, bounded real Windows gameplay/route/leisure/NPC/album/ending QA and new APK receipt. No canonical gameplay, route, art/audio or schema changes. Not full device or human-retention acceptance.'})
    dump(D/'checkpoint.json',{'path':str(target),'files':len(entries),'bytes':size,'manifestSHA256':sha(target/'CHECKPOINT_MANIFEST.json'),'payloadVerified':True,'baseVerified':True,'requiresBase':str(B),'standalone':False})

if __name__=='__main__':
    c=sys.argv[1]
    if c in ['estimate','checkpoint']:checkpoint(c=='checkpoint')
    else:{'freeze':freeze,'verify':verify,'evidence':evidence}[c]()
