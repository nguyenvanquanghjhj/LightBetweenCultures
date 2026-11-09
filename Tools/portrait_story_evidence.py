"""Review gallery, protected-input verification and checkpoint for task 2."""
import datetime,hashlib,json,re,shutil,sys
from pathlib import Path
from portrait_story_audit import R,V,OUT,ART,load,dump,sha,main as audit
B=R.parent/'Backups/CheckpointTerrainRoutes-20261017'
ASM=R/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
RESTARTS=['Stage1','Stage2','Stage3','Stage4','Ferry','Hue','Jeonju','DaLat']

def gallery():
    data=load(OUT/'node-audit.json');npcs=load(ART/'npc-portrait-audit.json')
    page='''<!doctype html><meta charset="utf-8"><title>LBC — Portrait & Story review</title>
<style>body{margin:24px;background:#173e48;color:#ffe4af;font:16px system-ui}a{color:#73dfca}section{margin:24px 0;padding:16px;background:#214d55}img{max-width:100%}.pair{display:flex;gap:16px}.pair>div{width:50%}.pair img,#motion{max-height:780px}button,select,input{font:inherit;margin:8px}table{border-collapse:collapse;width:100%}td,th{border-bottom:1px solid #517678;text-align:left;padding:8px;vertical-align:top}tr{cursor:pointer}pre{white-space:pre-wrap;overflow-wrap:anywhere}#detail{max-height:600px;overflow:auto}</style>
<h1>Portrait NPC, hội thoại và kiểm toán cốt truyện</h1>
<p>Build cuối: portrait/dialogue PASS; cốt truyện chưa hoàn thiện sáu thành phố. Gyeongju/Seoul DATA_ONLY. Art tạm không trở thành canonical. Bảng dưới phân biệt trạng thái runtime đã chứng minh và nội dung còn thiếu.</p>
<p><a href="PortraitStoryReport.md">Báo cáo / file sửa / kiểm thử</a> · <a href="Docs/Art/PortraitStory-20261017/npc-portrait-audit.md">NPC và art còn thiếu</a> · <a href="Docs/Story/PortraitStory-20261017/quest-audit.md">Quest chính/phụ</a> · <a href="Docs/Story/PortraitStory-20261017/node-audit.csv">CSV 133 node</a></p>
<p>Ảnh trước/sau Bà Tư và ông Phúc chụp mới từ hai binary, cùng vị trí/save/zoom. Vị trí là fixture để đối chiếu hình ảnh, không phải bằng chứng hoàn thành quest. Không thay quest/item/world flag trong fixture. Bằng chứng gameplay đến từ Journey04 và Reverse04 thực sự chơi qua UI. Build 05 chỉ bổ sung QA đo fade; source gameplay/UI và art giữ đúng build 04.</p>'''
    for name,title in [('BaTu','Bà Tư'),('Phuc','Ông Phúc')]:
        page+=f'<section><h2>{title} — cùng vị trí, tiến độ và zoom</h2><div class="pair">'
        for folder,label in [('Before'+name,'BEFORE — binary checkpoint'),('After'+name+'04','AFTER — build 04')]:
            p='Validation/PortraitStory-20261017/'+folder+'/X-Restart-portrait-view-NPC.png'
            page+=f'<div><h3>{label}</h3><a href="{p}"><img src="{p}"></a></div>'
        page+='</div></section>'
    frames=[]
    for direction in ['to-Lumi','return']:
        for p in sorted((V/'Handoff05').glob('Speaker-jeonju-J15-'+direction+'-*.png')):
            frames.append(str(p.relative_to(R)).replace('\\','/'))
    page+='''<section><h2>So-yeon → Lumi → So-yeon, tương tác giao ớt thật</h2>
<p>Một ảnh speaker duy nhất. Handoff05 mở xác nhận giao ớt thật bằng touch rồi hủy, từ save J15 đã kiếm được qua Journey04; không giả quest/flag. Chuyển động Full: người cũ fade out, đổi sprite/slot khi ẩn, người mới fade in. Phát chậm để quan sát; metadata thời điểm/alpha/slot thật nằm trong các file samples.txt cùng thư mục. Reverse04 kiểm tra cùng giao dịch ở chế độ Off, không dùng frame Off để chứng minh fade.</p><button id="play">Phát / Dừng</button><input id="step" type="range" min="0" value="0"><p id="frameName"></p><a id="motionLink"><img id="motion"></a></section>'''
    for res in ['540x960','540x1200','960x540']:
        page+=f'<section><h2>Layout {res} — fixture nội dung ngắn/dài, portrait thật</h2><div class="pair">'
        for mode in ['short','long-scrolled']:
            p=f'Validation/PortraitStory-20261017/Diagnostics04/Portrait-layout-FIXTURE-{res}-{mode}.png'
            page+=f'<div><h3>{mode}</h3><a href="{p}"><img src="{p}"></a></div>'
        page+='</div></section>'
    page+='''<section><h2>133 node — lọc và mở chi tiết</h2><select id="city"><option value="">Mọi thành phố</option></select><select id="status"><option value="">Mọi trạng thái</option><option>IMPLEMENTED</option><option>PARTIAL</option><option>DATA_ONLY</option><option>NOT_VERIFIED</option></select><input id="search" placeholder="Node, NPC, quest, item..."><p id="count"></p><table><thead><tr><th>Node</th><th>Vai trò</th><th>Trạng thái / phần thiếu</th></tr></thead><tbody id="rows"></tbody></table><h3>Chi tiết node đã chọn</h3><pre id="detail"></pre></section>'''
    page+='<script>const nodes='+json.dumps(data,ensure_ascii=False).replace('</',r'<\/')+';const frames='+json.dumps(frames)+';'+'''
const city=document.querySelector('#city'),status=document.querySelector('#status'),query=document.querySelector('#search'),rows=document.querySelector('#rows');
for(const c of [...new Set(nodes.map(n=>n.cityId))])city.add(new Option(c,c));
function update(){rows.replaceChildren();let shown=nodes.filter(n=>(!city.value||n.cityId===city.value)&&(!status.value||n.status===status.value)&&JSON.stringify(n).toLowerCase().includes(query.value.toLowerCase()));document.querySelector('#count').textContent=shown.length+' / 133 node';for(const n of shown){let tr=document.createElement('tr');for(const text of [n.id+' · '+n.name,n.storyRole,n.status+' — '+n.reason]){let td=document.createElement('td');td.textContent=text;tr.append(td)}tr.onclick=()=>document.querySelector('#detail').textContent=JSON.stringify(n,null,2);rows.append(tr)}}city.onchange=status.onchange=query.oninput=update;update();
const step=document.querySelector('#step');step.max=frames.length-1;let timer=null;function show(){let p=frames[Number(step.value)];document.querySelector('#motion').src=p;document.querySelector('#motionLink').href=p;document.querySelector('#frameName').textContent=p;}step.oninput=show;document.querySelector('#play').onclick=()=>{if(timer){clearInterval(timer);timer=null;}else timer=setInterval(()=>{step.value=(Number(step.value)+1)%frames.length;show()},200)};show();</script>'''
    (R/'PortraitStoryReview.html').write_text(page,encoding='utf-8')
    assert len(frames)==18,frames
    for link in re.findall(r'(?:href|src)="([^"]+)"',page):
        if not link.startswith('#'):assert (R/link).exists(),link

def verify():
    manifest=load(B/'CHECKPOINT_MANIFEST.json');changed=[]
    allowed={
      'Assets/_Game/Scripts/UI/AdventureUIArt.cs','Assets/_Game/Scripts/UI/GameplayPanels.cs','Assets/_Game/Scripts/UI/PortraitPresence.cs',
      'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CrossCity.cs',
      'Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/AdventureUIArt.asset',
      'Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png.meta',
      'Docs/Art/native-art-bindings.json','Docs/Art/art-asset-manifest.json','Docs/Art/art-asset-manifest.csv',
      'Docs/Art/ART_ASSET_CATALOG.md','Docs/Art/ART_SOURCE_INDEX.md'}
    for e in manifest['entries']:
        p=R/e['path']
        if not p.is_file():
            assert e['path'].startswith('Builds/Windows/'),'Protected source missing: '+e['path']
            changed.append(e['path']);continue
        if sha(p)!=e['sha256']:changed.append(e['path'])
    assert all(p in allowed or p.startswith('Builds/Windows/') for p in changed),changed
    before=load(B/'Assets/_Game/Resources/Content/GameplayContent.json');after=load(R/'Assets/_Game/Resources/Content/GameplayContent.json')
    old=next(e for e in before['events'] if e['cityId']=='hue' and e['nodeId']=='U1' and not e.get('additive'))
    new=next(e for e in after['events'] if e['cityId']=='hue' and e['nodeId']=='U1' and not e.get('additive'))
    assert new['text']=='\n'.join(dict.fromkeys(old['text'].splitlines()))
    new['text']=old['text'];assert after==before,'Gameplay/node/route/gate/quest content changed'
    oldbind=load(B/'Docs/Art/native-art-bindings.json')['entries'];newbind=load(R/'Docs/Art/native-art-bindings.json')['entries']
    bound={(b['role'],b['id'],b['path'],b['spriteName']):b for b in newbind}
    for b in oldbind:
        n=bound[(b['role'],b['id'],b['path'],b['spriteName'])];assert (b['guid'],b['localId'])==(n['guid'],n['localId']),b
    for i in [1,2,3,4,5]:assert 'Windows build: Succeeded' in (R/f'Logs/portrait-story-build-0{i}.log').read_text(encoding='utf-8-sig')
    runs=[]
    actual=['Journey04','Diagnostics04','UI04','Reverse04','Handoff05']+['Restart04-'+x for x in RESTARTS]
    visual=['BeforeBaTu','BeforePhuc','AfterBaTu04','AfterPhuc04']
    for name in actual+visual:
        f=V/name/('restart-portrait-view-results.txt' if name in visual else 'smoke-results.txt');lines=f.read_text(encoding='utf-8-sig').splitlines()
        assert lines[-1]=='ALL CHECKS PASSED' and not any('FAIL:' in l or l.startswith('ERROR:') for l in lines),f
        runs.append(dict(run=name,build='checkpoint_before' if name.startswith('Before') else '05_QA_ONLY_ADDITION' if name=='Handoff05' else '04',kind='VISUAL_POSITION_FIXTURE' if name in visual else 'EARNED_PROGRESS_RESTART' if name.startswith('Restart') else 'LAYOUT_PORTRAIT_DIAGNOSTIC' if name=='Diagnostics04' else 'REAL_UI_HANDOFF_WITHOUT_DELIVERY' if name=='Handoff05' else 'REAL_UI_JOURNEY',assertions=sum(l.startswith('PASS:') for l in lines),resultsSHA256=sha(f)))
    counts=load(OUT/'audit-summary.json');assert counts['nodes']==133 and counts['npcCount']==27 and counts['quests']==29
    assert len(counts['missingNpcBindings'])==8 and counts['nodeStatuses']['DATA_ONLY']==45
    core=[p for p in (R/'Assets/_Game/Scripts').rglob('*.cs') if 'Editor' not in p.relative_to(R/'Assets/_Game/Scripts').parts]
    for p in core:assert p.stat().st_mtime<=ASM.stat().st_mtime,p
    production=[p for p in core if 'Validation' not in p.relative_to(R/'Assets/_Game/Scripts').parts]
    for p in production:assert p.stat().st_mtime<=(R/'Logs/portrait-story-build-04.log').stat().st_mtime,'Production source changed after build04: '+str(p)
    for name in ['BaTu','Phuc']:assert load(V/('Before'+name)/'portrait-view.json')==load(V/('After'+name+'04')/'portrait-view.json'),'Visual fixtures differ: '+name
    for direction in ['to-Lumi','return']:
        samples=(V/'Handoff05'/('Speaker-jeonju-J15-'+direction+'-samples.txt')).read_text(encoding='utf-8-sig')
        assert 'npc_soyeon' in samples and 'Lumi_DialoguePortrait' in samples,samples
        assert any(0<float(x)<.999 for x in re.findall(r'alpha=([\d.]+)',samples)),samples
    screenshots=[]
    finalFolders=['Journey04','Diagnostics04','UI04','Reverse04','AfterBaTu04','AfterPhuc04']+['Restart04-'+x for x in RESTARTS]
    for name in actual+visual:
        for p in (V/name).glob('*.png'):
            if name in finalFolders:assert p.stat().st_mtime>=(R/'Logs/portrait-story-build-04.log').stat().st_mtime,p
            if name=='Handoff05':assert p.stat().st_mtime>=(R/'Logs/portrait-story-build-05.log').stat().st_mtime,p
            screenshots.append(dict(path=str(p.relative_to(R)).replace('\\','/'),build='checkpoint_before' if name.startswith('Before') else '05_QA_ONLY_ADDITION' if name=='Handoff05' else '04',sha256=sha(p),bytes=p.stat().st_size))
    proof=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseline=str(B),baselineEntriesCompared=len(manifest['entries']),changedBaselineFiles=changed,
        status='PORTRAIT_DIALOGUE_PASS_STORY_AUDIT_COMPLETE_GYEONGJU_SEOUL_DATA_ONLY',assemblySHA256=sha(ASM),configurationRestoredAndVerified=True,
        storyBuild04AssemblySHA256='34733cb9049414eae881ff9d7b7608345f0fab2fb5ac41e6ef25069caaca991b',finalBuild='05_QA_ONLY_ADDITION',
        productionSourcesUnchangedAfterBuild04=[dict(path=str(p.relative_to(R)).replace('\\','/'),sha256=sha(p)) for p in production],
        runs=runs,assertions=sum(r['assertions'] for r in runs if r['run'] in actual),screenshots=screenshots,auditSummary=counts,
        sourceInputs=[dict(path=str(p.relative_to(R)).replace('\\','/'),sha256=sha(p)) for p in core],
        limitations=['45 Gyeongju/Seoul nodes and eight canonical quests remain DATA_ONLY; no six-chapter completion claim.',
          '13 nodes PARTIAL; five optional/portal nodes NOT_VERIFIED in fresh story. See node/quest audit.',
          'Eight NPC bindings missing; 15 identities temporary or awaiting approval. D18 currently uses authored Lumi narration.',
          'Before/after NPC view fixtures change position only; never story completion proof. Long layout fixtures are not authored story.',
          'Build04 proves the full released four-city gameplay and portraits after exact configuration restore. Build05 adds QA only and proves Full-motion handoff; production sources/art/configuration are unchanged. Intermediate02/03 runs are historical only.',
          'Android/device NOT_VERIFIED. Terrain review regions from prior checkpoint remain pending.'])
    dump(V/'evidence-manifest.json',proof);print('VERIFIED:',proof['assertions'],'assertions;',len(screenshots),'current-turn runtime PNGs; all prior art GUID/local IDs retained')
    return proof

def checkpoint(proof):
    target=R.parent/'Backups/CheckpointPortraitDialogueStoryAudit-20261017';assert not target.exists(),target
    # BuildPipeline replaces its own generated files; do not resurrect obsolete
    # player output from an older build. Source scenes remain hash-protected.
    paths={e['path'] for e in load(B/'CHECKPOINT_MANIFEST.json')['entries'] if not e['path'].startswith('Builds/Windows/')}
    for folder in ['Assets','Packages','ProjectSettings','Docs','Tools','References','Builds']:
        paths.update(str(p.relative_to(R)).replace('\\','/') for p in (R/folder).rglob('*') if p.is_file() and '__pycache__' not in p.parts)
    for folder in ['Before','UnityAutoSettingsBeforeRestore','BeforeBaTu','BeforePhuc','Journey04','Diagnostics04','UI04','Reverse04','Handoff05','AfterBaTu04-IncompleteInput','AfterBaTu04','AfterPhuc04']+['Restart04-'+x for x in RESTARTS]:
        paths.update(str(p.relative_to(R)).replace('\\','/') for p in (V/folder).rglob('*') if p.is_file())
    paths.update(str(p.relative_to(R)).replace('\\','/') for p in R.glob('*.md'))
    paths.update(['PortraitStoryReview.html','Validation/PortraitStory-20261017/evidence-manifest.json','Validation/PortraitStory-20261017/UnityAutoSettingsBeforeRestore/restore-manifest.json'])
    paths.update('Logs/portrait-story-build-0'+str(i)+'.log' for i in [1,2,3,4,5])
    entries=[]
    for relative in sorted(paths):
        src=R/relative;dst=target/relative;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);digest=sha(src);assert sha(dst)==digest,relative
        entries.append(dict(path=relative,bytes=src.stat().st_size,sha256=digest))
    for e in entries:assert sha(R/e['path'])==e['sha256'],'Source changed during copy: '+e['path']
    dump(target/'CHECKPOINT_MANIFEST.json',dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),source=str(R),baselineCheckpoint=str(B),
        type='FULL_SOURCE_BUILD_WITH_SELECTED_RUNTIME_EVIDENCE',status=proof['status'],verifiedSHA256=True,fileCount=len(entries),totalBytes=sum(e['bytes'] for e in entries),
        tests=dict(assertions=proof['assertions'],processRestarts=8),assemblySHA256=proof['assemblySHA256'],queuedTasks=[3,4],limitations=proof['limitations'],entries=entries))
    print('CHECKPOINT VERIFIED:',target,';',len(entries),'files;',sum(e['bytes'] for e in entries),'bytes')

if __name__=='__main__':
    audit();gallery()
    if '--verify' in sys.argv or '--checkpoint' in sys.argv:
        proof=verify()
        if '--checkpoint' in sys.argv:checkpoint(proof)
