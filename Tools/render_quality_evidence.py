import json,pathlib,hashlib,csv,html,shutil,datetime
ROOT=pathlib.Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/RenderQuality-20261017';VAL=ROOT/'Validation/RenderQuality-20261017'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def verify():
    before=json.loads((DOC/'before-hashes.json').read_text());changed=[]
    for p,h in before.items():
        q=ROOT/p
        assert q.exists(),p
        if sha(q)!=h:changed.append(p)
    allowed={'Assets/_Game/Scripts/UI/GameplayPanels.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.H3.cs'}
    native=json.loads((DOC/'native-after.json').read_text());old={x['path']:x for x in json.loads((DOC/'native-before.json').read_text())['textures']}
    allowed.update(p+'.meta' for p in native['changed'])
    assert set(changed)==allowed,(set(changed)-allowed,allowed-set(changed))
    # GUID and complete sprite slice/mesh data must not change when filtering changes.
    for p in native['changed']:
        a=(ROOT/(p+'.meta')).read_text();b=(DOC/'BeforeFiles'/(p+'.meta')).read_text()
        assert a.split('guid: ')[1].splitlines()[0]==b.split('guid: ')[1].splitlines()[0]
        assert a.split('  spriteSheet:')[1]==b.split('  spriteSheet:')[1],p
        assert sha(ROOT/p)==before[p],p
    with (DOC/'import-changes.csv').open('w',newline='',encoding='utf-8-sig') as f:
        w=csv.writer(f);w.writerow(['asset','old compression','new compression','old filter','new filter','old mip','new mip','source WxH','import WxH'])
        for a in native['textures']:
            if a['path'] in native['changed']:
                b=old[a['path']];w.writerow([a['path'],b['compression'],a['compression'],b['filter'],a['filter'],b['mipmaps'],a['mipmaps'],f"{a['sourceWidth']}x{a['sourceHeight']}",f"{a['width']}x{a['height']}"])
    runs={}
    for folder in ['Before','After','Regression','Restart']:
        p=VAL/folder/'smoke-results.txt'
        if p.exists():
            s=p.read_text(encoding='utf8');runs[folder]={'pass':s.count('PASS:'),'passed':s.rstrip().endswith('ALL CHECKS PASSED'),'path':p.relative_to(ROOT).as_posix()}
    captures=[{'path':p.relative_to(ROOT).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)} for p in VAL.rglob('*.png')]
    sam={}
    for folder in ['Before','After']:
        p=VAL/folder/'Quality-Mai-dialogue-sampling.json'
        if p.exists():sam[folder]=next(i for i in json.loads(p.read_text(encoding='utf8'))['images'] if i['name']=='EventPortrait')
    result={'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'changedOriginalFiles':changed,'unchangedOriginalFiles':len(before)-len(changed),'textureChanges':len(native['changed']),'allOriginalPNGsUnchanged':True,'allScenesPackagesProjectSettingsGameplayDataUnchanged':True,'GUIDsAndSpriteSlicesUnchanged':True,'runs':runs,'maiPortraitSampling':sam,'captures':captures,'androidDevice':'NOT VERIFIED','visualAcceptance':'PENDING USER REVIEW','sourceLimits':['H3 floor 1254px across 20 world units; 2667 display pixels at 540x960: 2.13x magnification. No import setting restores absent detail.','H3 houses ~340px source crops, about 520px displayed.','Packed UI/item atlases stay Bilinear/no mipmaps to avoid neighbour bleeding.','Windows before runtime samples already RGBA32: compressed importer defaults are a platform-risk fix, not proven Windows blur cause.']}
    (DOC/'verification.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf8')
    gallery(captures)
    print(json.dumps({k:v for k,v in result.items() if k not in ['captures','maiPortraitSampling']},ensure_ascii=True))
def gallery(captures):
    rows=['<!doctype html><meta charset="utf-8"><title>LBC Render Quality</title><style>body{background:#152d31;color:#f9efd8;font:16px system-ui;margin:28px}a{color:#f4c871}.pair{display:flex;gap:20px;flex-wrap:wrap}figure{margin:8px}img{max-width:100%;height:auto}figure img{width:324px}h2{margin-top:36px}.crop{position:relative;overflow:hidden;width:400px;height:300px;border:1px solid #9e8154}.crop img{position:absolute;max-width:none;width:1080px;height:1920px}</style><h1>LBC — ảnh runtime mới trước / sau</h1><p>17/10/2026. Cùng bản đồ, vị trí, độ phân giải và zoom. Portrait lớn hơn có chủ đích; camera H3 giữ nguyên. Close-up chỉ là khung xem CSS từ PNG gốc, không phải ảnh vẽ lại hay ảnh đo độ nét.</p>']
    for name in ['Quality-WorldMap','Quality-CityMap-Mai','Quality-Mai-dialogue','Quality-Lumi-dialogue','Quality-Inventory','05-H3-exploration','H3-near-eye-before-pickup','H3-desktop-1280x720']:
        rows.append('<h2>'+html.escape(name)+'</h2><div class="pair">')
        for folder,label in [('Before','Trước'),('After','Sau')]:
            p=VAL/folder/(name+'.png')
            if p.exists():
                link=p.relative_to(ROOT).as_posix();rows.append(f'<figure><figcaption>{label}</figcaption><a href="{link}"><img src="{link}"></a></figure>')
        rows.append('</div>')
    for title,name,x,y in [('NPC portrait','Quality-Mai-dialogue',20,450),('Item / túi đồ','Quality-Inventory',30,300),('UI / H3','05-H3-exploration',20,20),('Lumi / H3','05-H3-exploration',200,540)]:
        rows.append('<h2>Close-up 2×: '+title+'</h2><div class="pair">')
        for folder in ['Before','After']:
            link=f'Validation/RenderQuality-20261017/{folder}/{name}.png'
            rows.append(f'<div><p>{folder}</p><div class="crop"><img src="{link}" style="left:{-2*x}px;top:{-2*y}px"></div></div>')
        rows.append('</div>')
    rows.append('<h2>Kiểm thử / các ảnh khác</h2><ul>')
    for c in captures:
        if '/Regression/' in c['path'] or '/Restart/' in c['path']:rows.append(f'<li><a href="{c["path"]}">{html.escape(c["path"])}</a></li>')
    rows.append('</ul>');(ROOT/'RenderQualityReview.html').write_text('\n'.join(rows),encoding='utf8')
def checkpoint():
    target=ROOT.parent/'Backups/CheckpointRenderQuality-20261017';assert not target.exists()
    base=ROOT.parent/'Backups/CheckpointH3VisualUpgrade-20261017'
    known={};origins={}
    def load_base(p):
        m=json.loads((p/'CHECKPOINT_MANIFEST.json').read_text(encoding='utf-8-sig'))
        if 'baseCheckpoint' in m:
            parent=pathlib.Path(m['baseCheckpoint']);assert sha(parent/'CHECKPOINT_MANIFEST.json')==m['baseManifestSHA256'];load_base(parent)
        # Earlier tasks verified full payloads. Recheck manifest links and only
        # the exact base payloads affected here; do not hash multi-GB backups again.
        for e in m['entries']:
            known[e['path']]=e['sha256'];origins[e['path']]=p/e['path']
        for d in m.get('deletedPaths',[]):known.pop(d,None)
    load_base(base)
    proof=json.loads((DOC/'verification.json').read_text(encoding='utf8'))
    assert all(r['passed'] for r in proof['runs'].values()) and set(proof['runs'])=={'Before','After','Regression','Restart'}
    changed=proof['changedOriginalFiles']
    for rel in changed:
        q=origins[rel];assert q.exists() and sha(q)==known[rel],str(q)
    files=[ROOT/p for p in changed]
    for rel in ['Assets/_Game/Scripts/Editor/RenderQualitySetup.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.RenderQuality.cs']:
        files.extend([ROOT/rel,ROOT/(rel+'.meta')])
    files += [ROOT/'Tools/render_quality_audit.py',ROOT/'Tools/render_quality_evidence.py',ROOT/'RenderQualityReport.md',ROOT/'RenderQualityReview.html']
    files += [p for d in [DOC,VAL] for p in d.rglob('*') if p.is_file() and ('BeforeFiles' not in p.parts or p.relative_to(DOC/'BeforeFiles').as_posix() in changed)]
    files += list((ROOT/'Logs').glob('render-quality*.log'))
    files=sorted(set(p for p in files if known.get(p.relative_to(ROOT).as_posix())!=sha(p)))
    size=sum(p.stat().st_size for p in files);print('Incremental estimate',len(files),size)
    entries=[]
    for p in files:
        rel=p.relative_to(ROOT).as_posix();dest=target/rel;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest);h=sha(p);assert sha(dest)==h;entries.append({'path':rel,'sha256':h,'bytes':p.stat().st_size})
    m={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(base),'baseManifestSHA256':sha(base/'CHECKPOINT_MANIFEST.json'),'baseVerification':'Manifest chain hashes and exact 17 changed base payloads rechecked. Full base integrity verified in prior task, not rescanned here.','restore':'Restore FullStoryComplete, overlay H3VisualUpgrade, then overlay this checkpoint. Library/build regenerated. This is NOT standalone.','changedFiles':len(entries),'totalBytes':size,'entries':entries,'deletedPaths':[]}
    (target/'CHECKPOINT_MANIFEST.json').write_text(json.dumps(m,indent=2),encoding='utf8');print('Verified checkpoint:',target)
if __name__=='__main__':
    import sys
    if '--checkpoint' in sys.argv:checkpoint()
    else:verify()
