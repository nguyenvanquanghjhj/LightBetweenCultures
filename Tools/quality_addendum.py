"""Bounded addendum audit and small review packs. Never scans backup/cache trees."""
from pathlib import Path
import json,hashlib,zipfile,datetime,html,csv,shutil,re
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QualityAddendum-20261017';PACK=ROOT/'ReviewPacks/QualityAddendum-20261017'
def read(p):return json.loads((ROOT/p).read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def audit():
    content=read('Assets/_Game/Resources/Content/GameplayContent.json')
    expected={f'{p}Q{i:02}' for p,n in [('H',7),('U',4),('D',5),('J',4),('G',4),('S',4)] for i in range(1,n+1)}|{'HA_FOOD_PREP'}
    assert {q['id'] for q in content['quests']}==expected and len(expected)==29
    assert sum(len(c['nodes']) for c in content['cities'])==133
    assert all(c['runtimeReady'] for c in content['cities'])
    assert not any(e.get('sourceStatus')=='imported_design_not_playable' for e in content['events'])
    canonical=ROOT/'LBC_Gameplay_6Cities_Detailed_v1/Data'
    for c in content['cities']:
        design=json.loads((canonical/'Cities'/(c['id']+'.json')).read_text(encoding='utf-8-sig'))
        assert [n['id'] for n in c['nodes']]==[n['id'] for n in design['nodes']],c['id']
    sources=read('Docs/Story/FullStory-20261017/source-records.json')
    source_checks=[{'path':r['path'],'unchanged':sha(ROOT/r['path'])==r['sha256']} for r in sources]
    assert all(r['unchanged'] for r in source_checks),'Canonical source changed; inspect before using old story evidence'
    old=read('Docs/QualityAddendum-20261017/before.json');changes=[]
    for r in old:
        if sha(ROOT/r['path'])!=r['sha256']:changes.append(r['path'])
    allowed={'Assets/_Game/Scripts/Editor/HoiAnDemoSetup.cs','Assets/_Game/Scripts/Editor/H3ArtSetup.cs','Assets/_Game/Scripts/Editor/AdventureUISetup.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.RenderQuality.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CrossCity.cs','Assets/_Game/Resources/AdventureUIArt.asset'}
    assert set(changes)==allowed,changes
    rel='Assets/_Game/Resources/AdventureUIArt.asset'
    def without_postcards(s):return re.sub(r'(?ms)^  - id: (?:festival_friendship_postcard|hoi_an_bridge_postcard)\n.*?(?=^  - id: |^  [A-Za-z]\w*:|\Z)','',s)
    assert without_postcards((ROOT/rel).read_text(encoding='utf8'))==without_postcards((DOC/'BeforeFiles'/rel).read_text(encoding='utf8')),'Unexpected art binding changes'
    guard=read('Docs/QualityAddendum-20261017/editor-guard-result.json');assert guard['status']=='PASS'
    q=next(q for q in content['quests'] if q['id']=='DQ05')
    assert q['giverNode']=='D4' and q['completeAtNode']=='J15' and q['confirmDelivery'] and q['consumeItems']
    assert q['requiredItems']==[{'itemId':'da_lat_strawberry','amount':1}]
    seoul=next(c for c in content['cities'] if c['id']=='seoul');assert len(seoul['unlockFlags'])==4 and all('DQ05' not in f for f in seoul['unlockFlags'])
    ha=next(c for c in content['cities'] if c['id']=='hoi_an');degree={n['id']:0 for n in ha['nodes']}
    for e in ha['edges']:
        for k in ['from','to']:
            key=e[k];key=ha['nodes'][key]['id'] if isinstance(key,int) else key;degree[key]+=1
    assert max(degree.values())<=3 and 'H22' not in degree
    phuc=[e['nodeId'] for e in content['events'] if e.get('npcId')=='npc_phuc']
    qa=ROOT/'Validation/QualityAddendum-20261017/Postcard02/smoke-results.txt';s=qa.read_text(encoding='utf8');assert s.rstrip().endswith('ALL CHECKS PASSED')
    assert 'Inventory selects the requested item by name' in s
    binary='Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
    buildlog=ROOT/'Logs/quality-addendum-postcard-build-02.log';assert 'Windows build: Succeeded' in buildlog.read_text(encoding='utf8')
    (DOC/'build-manifest.json').write_text(json.dumps({'status':'WINDOWS_BUILD_PASS','log':buildlog.relative_to(ROOT).as_posix(),'logSHA256':sha(buildlog),'assembly':binary,'assemblySHA256':sha(ROOT/binary),'changedScope':'Two postcard art references, Editor safety, opt-in QA; gameplay unchanged','freshStoryRerun':False},indent=2),encoding='utf8')
    diagnostic=[]
    for rel in ['Logs/quality-addendum-editor-guard.log','Logs/quality-addendum-postcard-build-02.log','Logs/quality-addendum-postcard-qa-02.log']:
        lines=(ROOT/rel).read_text(encoding='utf8').splitlines();matched=[line for line in lines if any(word in line for word in ['H3 LEGACY BUILDER GUARD PASS','HoiAnDemo already exists','Windows build: Succeeded','GAMEPLAY PASS: Inventory selects the requested item','GAMEPLAY PASS: Disk reload','GarbageCollector disposing of ComputeBuffer','Unreleased non-evictable'])]
        diagnostic.extend([rel,*matched,''])
    (DOC/'diagnostics.txt').write_text('\n'.join(diagnostic),encoding='utf8')
    result={'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'status':'TARGETED_GUARD_AND_POSTCARD_RUNTIME_PASS','changedFiles':changes,'nodes':133,'quests':29,'runtimeCities':6,'unplayableImportedEvents':0,'canonicalSourceChecks':source_checks,'storyDocument':'Docs/Story/FULL_STORY_PLAYTEST.md','oldAlternateStoryPathExists':(ROOT/'Docs/Story/LBC_FullStory_Playtest_6Cities_133Nodes.md').exists(),'hoiAnMaxDegree':max(degree.values()),'phucRuntimeNodes':phuc,'DQ05':'D4 -> confirmed J15; one strawberry; optional, not a Seoul gate','newRuntimePlaytest':{'run':qa.relative_to(ROOT).as_posix(),'assertions':s.count('PASS:'),'receipt':'PRESENTATION FIXTURE ONLY, no item granted','inventory':'Real UI selecting an actually earned HQ07 postcard','assemblySHA256':sha(ROOT/binary)},'androidPhysical':'NOT VERIFIED','terrain':'R1/R2/R3 NEEDS_REVIEW','preservation':'Scene, gameplay content, all NPC/enemy/Lumi/marker bindings, and 30 package/project configuration files unchanged; only two postcard item art bindings changed','guard':guard}
    (DOC/'verification.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf8')
    print(json.dumps({k:v for k,v in result.items() if k!='canonicalSourceChecks'},ensure_ascii=True))
def catalog():
    files=['Docs/Art/native-art-bindings.json','Docs/Art/art-asset-manifest.json','Docs/Art/art-asset-manifest.csv','Docs/Art/ART_ASSET_CATALOG.md']
    for rel in files:
        dst=DOC/'BeforeFiles'/rel;assert not dst.exists();dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/rel,dst)
    patch=read('Docs/QualityAddendum-20261017/postcard-binding.json');ids={'festival_friendship_postcard','hoi_an_bridge_postcard'}
    def update(rows):
        for e in rows:
            if e.get('role')=='item' and e.get('id') in ids:e.update(patch,source=patch['path'],categoryIcon=True)
    p=ROOT/files[0];d=read(files[0]);npc_before=[dict(e) for e in d['entries'] if e.get('role')=='npc'];update(d['entries']);assert npc_before==[e for e in d['entries'] if e.get('role')=='npc'];p.write_text(json.dumps(d,indent=2,ensure_ascii=False),encoding='utf8')
    p=ROOT/files[1];d=read(files[1]);update(d['bindings'])
    replacements=[e for e in d['bindings'] if e.get('role')=='item' and e.get('id') in ids]
    for image in d['images']:
        image['bindings']=[e for e in image['bindings'] if not(e.get('role')=='item' and e.get('id') in ids)]
        if image['path']==patch['path']:image['bindings'].extend(replacements)
        if image['path'].endswith('/Item_Food_Postcard.jpg'):
            image['runtime']=False;image['status']='REFERENCE_ONLY_CREDIT_PRESERVED_DO_NOT_BIND';image['role']=['reference']
    p.write_text(json.dumps(d,indent=2,ensure_ascii=False),encoding='utf8')
    p=ROOT/files[2]
    with p.open(encoding='utf-8-sig',newline='') as f:r=csv.DictReader(f);fields=r.fieldnames;rows=list(r)
    for e in rows:
        if e['path'].endswith('/Item_Food_Postcard.jpg'):e['runtime']='False';e['status']='REFERENCE_ONLY_CREDIT_PRESERVED_DO_NOT_BIND'
    with p.open('w',encoding='utf-8-sig',newline='') as f:w=csv.DictWriter(f,fieldnames=fields);w.writeheader();w.writerows(rows)
    p=ROOT/files[3];s=p.read_text(encoding='utf8');s=s.replace('| Item_Food_Postcard.jpg | festival_friendship_postcard, hoi_an_bridge_postcard |','| Item_Food_Postcard.jpg | REFERENCE_ONLY — credit preserved, not runtime |');s+='\n\n## Quality addendum — postcard art safety, 2026-10-17\n\n`festival_friendship_postcard` và `hoi_an_bridge_postcard` dùng chung sprite sạch có sẵn `scroll` từ `Assets/_Game/Art/Items/Shared/Interactables_Atlas.png` (GUID '+patch['guid']+', localID '+patch['localId']+'). Trạng thái **NEEDS_ART_TEMPORARY_CATEGORY_ICON_CLEAN_PACK_SCROLL**, không phải art bưu thiếp final. Giữ JPG gốc và ghi công nguyên vẹn làm reference; không dùng derivative NeedsCleanup. Không đổi itemId, reward hoặc NPC identity.\n';p.write_text(s,encoding='utf8')
def packs():
    if PACK.exists():
        import sys
        assert '--finalize-packs' in sys.argv
        # Finalize only these unaccepted working drafts; never touch older packs.
        draft=json.loads((PACK/'index.json').read_text(encoding='utf8'))
        for r in draft:assert sha(PACK/r['zip'])==r['sha256']
        (DOC/'pack-draft-index.json').write_text(json.dumps(draft,indent=2),encoding='utf8')
    else:PACK.mkdir(parents=True)
    tasks=[
    ('01-CityMapUI','CityMapUIPolishTask1Report.md','Validation/CityMapUIPolish-20261017/final-evidence-manifest.json','CheckpointCityMapUIPolish-20261017',[
        'Validation/CityMapUIPolish-20261017/BeforeClean/S1-03-NPC-and-bridge-bot-on-map.png',
        'Validation/CityMapUIPolish-20261017/Final04-Journey/S1-03-NPC-and-bridge-bot-on-map.png',
        'Validation/CityMapUIPolish-20261017/Final04-Journey/Reward-150-entrance-settled.png',
        'Validation/CityMapUIPolish-20261017/Final04-Journey/X-HoiAn-HQ07-reward.png',
        *['Docs/Maps/TerrainRoutes-20261017/'+x+'-NEEDS_REVIEW.png' for x in ['R1','R2','R3']]],
        ['Docs/Maps/CityMapUIPolish-20261017/graph-before.svg','Docs/Maps/CityMapUIPolish-20261017/graph-after.svg','Docs/Maps/TerrainRoutes-20261017/needs-review.json']),
    ('02-Portraits','PortraitStoryReport.md','Validation/PortraitStory-20261017/evidence-manifest.json','CheckpointPortraitDialogueStoryAudit-20261017',[
        *['Validation/PortraitStory-20261017/'+x+'/X-Restart-portrait-view-NPC.png' for x in ['BeforeBaTu','AfterBaTu04','BeforePhuc','AfterPhuc04']],
        *['Validation/PortraitStory-20261017/Handoff05/Speaker-jeonju-J15-to-Lumi-'+x+'.png' for x in ['00','04','08']],
        'Validation/PortraitStory-20261017/Diagnostics04/Portrait-layout-FIXTURE-540x960-long-scrolled.png'],
        ['Docs/Art/NPC_CANONICAL_REGISTRY.md','Docs/Art/NPC_CANONICAL_REGISTRY.json','Docs/Art/FullStory-20261017/ASSET_STATUS.md']),
    ('03-FullStory','FullStoryReport.md','Validation/FullStory-20261017/evidence-manifest.json','CheckpointFullStoryComplete-20261017',[
        *['Validation/FullStory-20261017/Fresh02/'+x+'.png' for x in ['17-station-light-restored','Chapter-Hue-restored','DL-09-station-restored','Chapter-Jeonju-restored']],
        *['Validation/FullStory-20261017/Fresh04/'+x+'.png' for x in ['Story-G19-beacon','Story-S16-relay-A','Story-S17-relay-B','Story-S18-relay-C','Story-S20-phase2','Story-S21-ending-6','Story-S22-secret']]],
        ['Docs/Story/FullStory-20261017/node-audit.csv','Docs/Story/FullStory-20261017/quest-audit.md','Docs/Story/FullStory-20261017/audit-summary.json','Docs/Art/FullStory-20261017/ASSET_STATUS.md']),
    ('04-H3','H3UpgradeReport.md','Docs/H3Upgrade-20261017/verification.json','CheckpointH3VisualUpgrade-20261017',[
        'Validation/H3Upgrade-20261017/Before/05-H3-exploration.png',
        'Validation/H3Upgrade-20261017/Touch07/05-H3-exploration.png',
        'Validation/H3Upgrade-20261017/Touch07/H3-near-eye-before-pickup.png',
        'Validation/H3Upgrade-20261017/Desktop06/H3-desktop-1280x720.png',
        'Validation/H3Upgrade-20261017/RestartTouch03/H3-restart-item-stays-collected.png',
        'Validation/RenderQuality-20261017/After/05-H3-exploration.png',
        'Validation/RenderQuality-20261017/After/Quality-Mai-dialogue.png',
        'Validation/QualityAddendum-20261017/Postcard02/Postcard-inventory-earned.png',
        'Validation/QualityAddendum-20261017/Postcard02/Postcard-receipt-PRESENTATION-FIXTURE.png'],
        ['Docs/H3Upgrade-20261017/PLACEHOLDER_AUDIT.md','RenderQualityReport.md','Docs/RenderQuality-20261017/verification.json','Docs/RenderQuality-20261017/import-changes.csv'])]
    index=[]
    for name,report,evidence,cp,images,extra in tasks:
        files=[report,evidence,*images,*extra,'Docs/QualityAddendum-20261017/README.md','Docs/QualityAddendum-20261017/verification.json','Docs/QualityAddendum-20261017/editor-guard-result.json','Docs/QualityAddendum-20261017/postcard-binding.json','Docs/QualityAddendum-20261017/build-manifest.json','Docs/QualityAddendum-20261017/diagnostics.txt','Validation/QualityAddendum-20261017/Postcard02/smoke-results.txt']
        files=list(dict.fromkeys(files));entries=[]
        for rel in files:
            p=ROOT/rel;assert p.is_file(),rel;entries.append({'path':rel,'bytes':p.stat().st_size,'sha256':sha(p)})
        overview={'milestone':name,'evidenceOrigin':'Selected existing milestone captures plus two explicitly named new postcard UI captures in pack04. Historical report identity statuses are superseded by NPC_CANONICAL_REGISTRY. Receipt screenshot is a presentation fixture, not a fresh quest award.','checkpoint':str(ROOT.parent/'Backups'/cp),'latestSafetyCheckpoint':str(ROOT.parent/'Backups/CheckpointQualityAddendum-20261017'),'PASS':['Prior functional milestone tests (see evidence manifest)','New actual legacy builder guard','New targeted postcard inventory/receipt/save reload'], 'FAILED':[],'NOT_VERIFIED':['Physical Android','Fresh six-city E2E rerun in this addendum (not rerun for an icon binding change)'],'NEEDS_REVIEW':['R1/R2/R3 terrain','Remaining art/source quality, as documented','Two postcard icons use temporary clean scroll category art'],'entries':entries}
        gallery='<!doctype html><meta charset="utf-8"><title>'+name+'</title><style>body{font:16px system-ui;background:#19383b;color:#ffefcf}img{max-width:400px;width:100%;height:auto}a{color:#f6cb85}figure{display:inline-block;vertical-align:top;width:420px;margin:10px}</style><h1>'+name+'</h1><p>Selected historical milestone evidence — not a new runtime playtest. Read REVIEW_MANIFEST.json and current addendum README first.</p>'
        for im in images:gallery+='<figure><a href="'+im+'"><img src="'+im+'"></a><figcaption>'+html.escape(im)+'</figcaption></figure>'
        out=PACK/(name+'.zip')
        with zipfile.ZipFile(out,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
            for rel in files:z.write(ROOT/rel,rel)
            z.writestr('REVIEW_MANIFEST.json',json.dumps(overview,indent=2,ensure_ascii=False));z.writestr('OPEN_REVIEW.html',gallery)
            z.writestr('README.txt','Open OPEN_REVIEW.html after extracting the ZIP. The historical full report may link to omitted evidence; use this selected gallery and manifest. No game binary, save, cache, backup payload or entire Validation tree is included.\n')
        with zipfile.ZipFile(out) as z:
            assert z.testzip() is None
            for e in entries:assert hashlib.sha256(z.read(e['path'])).hexdigest()==e['sha256']
        index.append({'name':name,'zip':out.name,'bytes':out.stat().st_size,'sha256':sha(out),'images':len(images),'historicalCheckpoint':cp})
    (PACK/'index.json').write_text(json.dumps(index,indent=2),encoding='utf8')
    (PACK/'README.md').write_text('# Review packs chọn lọc\n\nBốn ZIP tương ứng bốn yêu cầu; giải nén rồi mở OPEN_REVIEW.html. Ảnh giữ nguyên từ đúng milestone, không phải kiểm thử mới. Báo cáo lịch sử không thay thế registry nhận diện mới nhất.\n\n'+'\n'.join('- ['+r['name']+']('+r['zip']+') — '+str(round(r['bytes']/1e6,2))+' MB, '+str(r['images'])+' ảnh.' for r in index),encoding='utf8')
    print(json.dumps(index))
def checkpoint():
    target=ROOT.parent/'Backups/CheckpointQualityAddendum-20261017';assert not target.exists()
    base=ROOT.parent/'Backups/CheckpointRenderQuality-20261017';known={};origin={}
    def layer(p):
        m=json.loads((p/'CHECKPOINT_MANIFEST.json').read_text(encoding='utf-8-sig'))
        if 'baseCheckpoint' in m:
            b=Path(m['baseCheckpoint']);assert sha(b/'CHECKPOINT_MANIFEST.json')==m['baseManifestSHA256'];layer(b)
        for e in m['entries']:known[e['path']]=e['sha256'];origin[e['path']]=p/e['path']
    layer(base)
    proof=read('Docs/QualityAddendum-20261017/verification.json');changed=proof['changedFiles']
    catalogs=['Docs/Art/native-art-bindings.json','Docs/Art/art-asset-manifest.json','Docs/Art/art-asset-manifest.csv','Docs/Art/ART_ASSET_CATALOG.md']
    for rel in changed+catalogs:assert sha(origin[rel])==known[rel],rel
    files=[ROOT/rel for rel in changed+catalogs+['Tools/quality_addendum.py','Docs/Requests/20261017/quality-control-addendum.txt']]
    files += [p for p in DOC.rglob('*') if p.is_file() and ('BeforeFiles' not in p.parts or p.relative_to(DOC/'BeforeFiles').as_posix() in changed+catalogs)]
    files += [p for p in PACK.glob('*') if p.is_file()]
    files += [p for p in (ROOT/'Validation/QualityAddendum-20261017').rglob('*') if p.is_file()]
    files += list((ROOT/'Logs').glob('quality-addendum-*.log'))
    files=sorted(set(p for p in files if known.get(p.relative_to(ROOT).as_posix())!=sha(p)));size=sum(p.stat().st_size for p in files)
    print('Incremental estimate',len(files),'files,',size,'bytes');entries=[]
    for p in files:
        rel=p.relative_to(ROOT).as_posix();dest=target/rel;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest);h=sha(p);assert sha(dest)==h;entries.append({'path':rel,'sha256':h,'bytes':p.stat().st_size})
    (target/'CHECKPOINT_MANIFEST.json').write_text(json.dumps({'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(base),'baseManifestSHA256':sha(base/'CHECKPOINT_MANIFEST.json'),'restore':'Restore FullStoryComplete, overlay H3VisualUpgrade, RenderQuality, then this checkpoint. Not standalone; regenerate Library/build if needed.','baseVerification':'Manifest chain plus exactly changed source/catalog payloads checked; no full backup rescan','changedFiles':len(entries),'totalBytes':size,'entries':entries,'deletedPaths':[]},indent=2),encoding='utf8')
    print('Verified',target)
if __name__=='__main__':
    import sys
    if '--checkpoint' in sys.argv:checkpoint()
    elif '--catalog' in sys.argv:catalog()
    elif '--packs' in sys.argv or '--finalize-packs' in sys.argv:packs()
    else:audit()
