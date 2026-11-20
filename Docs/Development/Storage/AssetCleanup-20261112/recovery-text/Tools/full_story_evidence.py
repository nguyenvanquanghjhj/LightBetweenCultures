"""Evidence from actual earned journeys; never promotes catalog parsing to e2e PASS."""
from pathlib import Path
import json,hashlib,collections,csv,datetime,shutil,sys
from author_full_story import R,B,OUT,read,write
V=R/'Validation/FullStory-20261017'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def passed(folder):
 p=V/folder/'smoke-results.txt'
 return p.is_file() and p.read_text(encoding='utf-8-sig').rstrip().endswith('ALL CHECKS PASSED')
def audit():
 d=read(R/'Assets/_Game/Resources/Content/GameplayContent.json');proofs=[]
 folders=['Fresh04','Reverse03','Coverage03']+(['Tail01'] if not passed('Fresh04') and not passed('Reverse03') else [])
 for folder in folders:
  p=V/folder/('full-coverage-final.json' if folder.startswith('Coverage') else 'full-story-final.json')
  if passed(folder) and p.is_file():proofs.append((p,read(p)))
 def proof(field,value):return next((p.relative_to(R).as_posix() for p,s in proofs if value in s.get(field,[])),None)
 nodes=[];quests=[]
 gaps={'H2':'Tiệm chưa có ba mức sáng riêng; main quest nhiều bước vẫn chạy.','H4':'Dùng puzzle chuỗi hiện có thay thao tác đổi ba ô.','H11':'Giữ thưởng6xu của checkpoint mới, không đổi về dây đồng trong v1.','U8':'Puzzle chuỗi có logic thật; chưa có cảnh ba lá cờ.','D8':'Puzzle ba lựa chọn; chưa có cảnh ba cổng rẽ.','J3':'Puzzle màu có logic thật; chưa có cảnh nhuộm vải.','D18':'Có nghỉ hồi HP; portrait/nội dung riêng chủ trọ còn thiếu.'}
 for c in d['cities']:
  canonical=read(R/f'LBC_Gameplay_6Cities_Detailed_v1/Data/Cities/{c["id"]}.json');sourceNodes={n['id']:n for n in canonical['nodes']};sourceEvents={e['node']:e for e in canonical['events']}
  for i,n in enumerate(c['nodes']):
   events=[e for e in d['events'] if e['cityId']==c['id'] and e['nodeId']==n['id']];primary=next(e for e in events if e['id']==n['eventId'])
   related=[q for q in d['quests'] if ((q.get('giverCityId') or q['cityId'])==c['id'] and q['giverNode']==n['id']) or ((q.get('completeCityId') or q['cityId'])==c['id'] and q['completeAtNode']==n['id'])]
   visited=next((p.relative_to(R).as_posix() for p,s in proofs if any(cp['cityId']==c['id'] and i in cp['visited'] for cp in s['cityProgress'])),None)
   evidence=[]
   for e in events:
    for field,val in [('resolvedEvents',c['id']+':'+e['id']),('flags',e.get('completionFlag')),('solvedPuzzles',e.get('puzzleId')),('completed',e.get('questId'))]:
     if val and (p:=proof(field,val)):evidence.append(dict(field=field,value=val,path=p))
   for q in related:
    if p:=proof('completed',q['id']):evidence.append(dict(field='completed',value=q['id'],path=p))
   if n['id']=='H3' and (p:=proof('rewardGrantIds','h3-lamp-eye')):evidence.append(dict(field='rewardGrantIds',value='h3-lamp-eye',path=p))
   portal=V/'Coverage03'/('Story-portal-'+n['id']+'.png')
   if primary['type']=='portal' and passed('Coverage03') and portal.is_file():evidence.append(dict(field='actualPortalUI',value=n['id'],path=portal.relative_to(R).as_posix()))
   verified=bool(evidence) or primary['type'] in ['junction','start'] and bool(visited)
   status='PARTIAL' if n['id'] in gaps and verified else 'IMPLEMENTED' if verified else 'NOT_VERIFIED'
   nodes.append(dict(cityId=c['id'],id=n['id'],name=n['name'],status=status,reason=gaps.get(n['id'],'Actual earned interaction/navigation proof.' if verified else 'Runtime configured; no matching actual interaction proof yet.'),storyRole=sourceEvents[n['id']].get('interaction',n['kind']),canonicalNode=sourceNodes[n['id']],runtimeNode=n,events=events,quests=related,handler='NodeEventManager.BuildOptions/Execute + '+('BattleManager.Act/EncounterManager.Resolve' if primary['type']=='encounter' else 'PuzzleManager.Start/Submit' if primary['type']=='puzzle' else 'QuestManager.Accept/Complete' if related else 'CityMap.Move/Interact'),dialogueBefore={q['id']:q.get('offer') for q in related},dialogueDuring={q['id']:q.get('reminder') for q in related},dialogueAfter={q['id']:q.get('thanks') for q in related},visitedProof=visited,stateProof=evidence,terrainStatus='NEEDS_VISUAL_ACCEPTANCE' if c['id'] in ['gyeongju','seoul'] else 'PRESERVED_PREVIOUS_CHECKPOINT; terrain acceptance separate'))
 for q in d['quests']:quests.append(dict(**q,status='IMPLEMENTED' if proof('completed',q['id']) else 'NOT_VERIFIED',actualCompletionProof=proof('completed',q['id'])))
 write(OUT/'node-audit.json',nodes);write(OUT/'quest-audit.json',quests)
 with (OUT/'node-audit.csv').open('w',newline='',encoding='utf-8-sig') as f:
  w=csv.DictWriter(f,fieldnames=['cityId','id','name','status','reason','handler']);w.writeheader();w.writerows({k:n[k] for k in w.fieldnames} for n in nodes)
 lines=['# Full Story — 133 node','', '| City | Node | Trạng thái | Vai trò / giới hạn |','|---|---|---|---|']
 for n in nodes:lines.append('| '+ ' | '.join([n['cityId'],n['id']+' '+n['name'],n['status'],n['reason']])+' |')
 (OUT/'node-audit.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
 lines=['# Quest — hoàn thành thật qua UI','', '| Quest | Trạng thái | Nhận → trả | Bằng chứng |','|---|---|---|---|']
 for q in quests:lines.append('| '+' | '.join([q['id']+' '+q['title'],q['status'],q['giverNode']+' → '+q['completeAtNode'],q['actualCompletionProof'] or 'NOT_VERIFIED'])+' |')
 (OUT/'quest-audit.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
 sourcePaths=list((R/'LBC_Gameplay_6Cities_Detailed_v1/Data').rglob('*.json'))+list((R/'LBC_HoiAn_StoryExpansion_v1/Data').glob('*.json'))+[R/'Docs/Story/FULL_STORY_PLAYTEST.md']
 write(OUT/'source-records.json',[dict(path=p.relative_to(R).as_posix(),sha256=sha(p),content=read(p) if p.suffix=='.json' else p.read_text(encoding='utf-8')) for p in sourcePaths])
 summary=dict(nodes=len(nodes),nodeStatuses=dict(collections.Counter(n['status'] for n in nodes)),quests=len(quests),questStatuses=dict(collections.Counter(q['status'] for q in quests)),proofs=[p.relative_to(R).as_posix() for p,s in proofs],freshE2E=passed('Fresh04'),reverseE2E=passed('Reverse03'),lineage='Fresh02 H0 -> Fresh03 earned jeonju-before (before negative fixture) -> Fresh04; Reverse02 H0 -> exact earned Reverse03. No synthetic progress fields.')
 write(OUT/'audit-summary.json',summary);print(json.dumps(summary));return summary

def protect():
 m=read(B/'CHECKPOINT_MANIFEST.json');changed=[]
 allowed={'Assets/_Game/Scripts/Content/ContentDefinitions.cs','Assets/_Game/Scripts/Content/BattleManager.cs','Assets/_Game/Scripts/Content/EncounterManager.cs','Assets/_Game/Scripts/Content/NodeEventManager.cs','Assets/_Game/Scripts/Core/WorldState.cs','Assets/_Game/Scripts/Core/GameBootstrap.cs','Assets/_Game/Scripts/UI/GameplayPanels.cs','Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Chapters.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CrossCity.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.DaLat.cs','Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/Content/AdventurePresentation.json','Assets/_Game/Resources/AdventureUIArt.asset','Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png.meta','Docs/Art/native-art-bindings.json','Docs/Art/art-asset-manifest.json','Docs/Art/art-asset-manifest.csv','Docs/Art/ART_ASSET_CATALOG.md','Docs/Art/ART_SOURCE_INDEX.md'}
 oldSpec='Docs/Story/LBC_FullStory_Playtest_6Cities_133Nodes.md'
 for e in m['entries']:
  p=R/e['path']
  if e['path']==oldSpec:
   assert sha(R/'Docs/Story/FULL_STORY_PLAYTEST.md')==e['sha256'];continue
  if e['path'].startswith('Builds/Windows/'):continue
  assert p.is_file(),e['path']
  if sha(p)!=e['sha256']:assert e['path'] in allowed,e['path'];changed.append(e['path'])
 before=read(B/'Assets/_Game/Resources/Content/GameplayContent.json');after=read(R/'Assets/_Game/Resources/Content/GameplayContent.json')
 for c in before['cities']:
  new=next(x for x in after['cities'] if x['id']==c['id']);assert [n['id'] for n in c['nodes']]==[n['id'] for n in new['nodes']];assert [n['position'] for n in c['nodes']]==[n['position'] for n in new['nodes']]
  if c['id'] not in ['gyeongju','seoul']:assert c==new
 assert before['items']==after['items']
 for q in before['quests']:
  if q['cityId'] in ['gyeongju','seoul']:continue
  new=next(x for x in after['quests'] if x['id']==q['id'])
  if q['id']=='HQ03':
   new=dict(new);new['flags']=q.get('flags',[]);new['sourceStatus']=q.get('sourceStatus');
   for key in ['giverCityId','completeCityId']:
    if key not in q:new.pop(key,None)
  assert q==new,q['id']
 old=read(B/'Docs/Art/native-art-bindings.json')['entries'];new=read(R/'Docs/Art/native-art-bindings.json')['entries'];bindings={(x['role'],x['id'],x['path'],x['spriteName']):x for x in new}
 for x in old:
  if x['role'].startswith('city_') and x['id'] in ['gyeongju','seoul']:
   cap=x['id'].capitalize();assert sha(R/f'Assets/_Game/Art/Maps/{cap}/Map_{cap}_Overview_A.png')==sha(R/f'LBC_AnhMoi_BanGiaoCodex_20261015/Maps/{cap}/Map_Overview_A.png');continue
  y=bindings.get((x['role'],x['id'],x['path'],x['spriteName']));assert y is not None,(x['role'],x['id'],x['path']);assert (x['guid'],x['localId'])==(y['guid'],y['localId']),x
 return changed

def gallery():
 # New runtime shots only. Historical/task2 pictures never serve as new proof.
 run='Fresh04' if passed('Fresh04') else 'Reverse03' if passed('Reverse03') else 'Tail01';names=['Story-Gyeongju-opening','Story-G19-beacon','Story-Seoul-opening','Story-S16-relay-A','Story-S17-relay-B','Story-S18-relay-C','Story-S20-phase1','Story-S20-phase2','Story-S20-resolved','Story-S21-ending-5','Story-S21-ending-6','Story-S22-secret']
 html='<!doctype html><meta charset="utf-8"><title>LBC Full Story runtime</title><style>body{background:#163e48;color:#ffe5b5;font:17px system-ui;margin:24px}a{color:#80dfce}.cards{display:flex;flex-wrap:wrap;gap:18px}figure{margin:0;width:320px}img{max-width:100%}figcaption{padding:8px}</style><h1>Light Between Cultures — Full Story</h1><p>Ảnh runtime mới của lượt Full Story. Hành trình từ H0 được nối bằng các save kiếm được: Fresh02 → Fresh03 (snapshot trước fixture) → Fresh04 và Reverse02 → Reverse03. Các assertion QA lỗi thời được báo riêng, không che là PASS. Tail01 là seeded continuation, không dùng để chứng minh fresh end-to-end. Art tạm/terrain chờ duyệt không được coi là final.</p><p><a href="FullStoryReport.md">Báo cáo / file sửa</a> · <a href="Docs/Story/FullStory-20261017/node-audit.md">133 node</a> · <a href="Docs/Story/FullStory-20261017/quest-audit.md">29 quest</a> · <a href="Docs/Art/FullStory-20261017/ASSET_STATUS.md">Art thiếu/tạm</a></p><div class="cards">'
 for folder,pattern in [('Fresh02','*HoiAn*stage0*'),('Fresh02','17*'),('Fresh02','Chapter-Hue-restored.png'),('Fresh02','DL-09-station-restored.png'),('Fresh02','Chapter-Jeonju-restored.png'),('Fresh04','X-WorldMap-select-seoul.png'),('Coverage03','Story-HQ03-companion-visible.png')]:
  for p in sorted((V/folder).glob(pattern))[:1]:
   if p.suffix.lower()=='.png':link=p.relative_to(R).as_posix();html+=f'<figure><a href="{link}"><img src="{link}"></a><figcaption>{folder} · {p.stem}</figcaption></figure>'
 for name in names:
  p=V/run/(name+'.png')
  if p.exists():link=p.relative_to(R).as_posix();html+=f'<figure><a href="{link}"><img src="{link}"></a><figcaption>{run} · {name}</figcaption></figure>'
 html+='</div>';(R/'FullStoryReview.html').write_text(html,encoding='utf-8')

def assets():
 art=read(R/'Docs/Art/native-art-bindings.json')['entries'];d=read(R/'Assets/_Game/Resources/Content/GameplayContent.json');bound={x['id']:x for x in art if x['role']=='npc'}
 ids=sorted({e['npcId'] for e in d['events'] if e.get('npcId')});missing=[x for x in ids if x not in bound]
 out=R/'Docs/Art/FullStory-20261017';out.mkdir(parents=True,exist_ok=True)
 write(out/'asset-status.json',dict(npcBindings=[bound[x] for x in ids if x in bound],missingNpc=missing,newEnemyBindings=[x for x in art if x['role']=='enemy' and x['id'] in ['enemy_fox','enemy_stone_guard','enemy_night_guard','enemy_star_core','enemy_drone','enemy_signal_bot','enemy_bridge','enemy_shadow_courier','enemy_final_core']],maps=[dict(city=cap,path=f'Assets/_Game/Art/Maps/{cap}/Map_{cap}_Overview_A.png',sha256=sha(R/f'Assets/_Game/Art/Maps/{cap}/Map_{cap}_Overview_A.png'),originalUnchanged=True) for cap in ['Gyeongju','Seoul']]))
 lines=['# Full Story — art tạm / còn thiếu','', 'Không thay ngoại hình Cô Mai, Ji-eun, Hương hoặc So-yeon. Không gán So-yeon làm Ji-won. Mọi binding cũ được kiểm tra GUID/localID.','', 'Min dùng native sprite mới trong atlas RGBA đã có, từ hướng dẫn viên khảo cổ chưa gán danh tính; cần duyệt nhận diện. Không tạo PNG nhân vật trùng.','', '| Thiếu portrait riêng | Vai trò |','|---|---|']
 for npc in missing:
  places=sorted({e['cityId']+':'+e['nodeId'] for e in d['events'] if e.get('npcId')==npc});lines.append('| '+npc+' | '+', '.join(places)+' |')
 lines+=['','G9/G14/S2/S7/S11/S14 có tương tác/quest thật, nhưng chưa có portrait riêng. D18 vẫn dùng nghỉ ngơi và lời tự thoại Lumi đã phát hành. Không dùng khung tròn rỗng hoặc khuôn mặt NPC đã có danh tính để giả làm final.','','| Enemy mới | Art tạm |','|---|---|']
 for x in art:
  if x['role']=='enemy' and x['id'] in ['enemy_fox','enemy_stone_guard','enemy_night_guard','enemy_star_core','enemy_drone','enemy_signal_bot','enemy_bridge','enemy_shadow_courier','enemy_final_core']:lines.append('| '+x['id']+' | '+x['status']+' · '+x['path']+'#'+x['spriteName']+' |')
 lines+=['','Lõi sao và lõi cuối tạm dùng chung concept cầu nhiễu, không tạo PNG trùng và không gọi đây là hai boss art final. Cáo tạm dùng concept mèo bóng; drone tạm dùng bot có sẵn: cần đúng sprite cáo/drone trước nghiệm thu art. Tượng đá và bot đưa tin dùng mesh tách từ đúng concept trong nguồn người dùng; vẫn cần duyệt biên/effect.','','Hai Map Overview A mới là bản copy nguyên byte, đủ1448×1086, không phóng lớn ảnh preview. Thư viện/catalog chỉ cập nhật inventory; không chạy lại migration thư viện.','','Các portrait tạm/chờ duyệt đã có từ checkpoint trước vẫn giữ nguyên; xem asset-status.json để đọc source/status đầy đủ. Android/device và chất lượng terrain chưa được nghiệm thu.']
 (out/'ASSET_STATUS.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
 native=read(OUT/'native-slices.json');sheet=R/native['sheet'];html='<!doctype html><meta charset="utf-8"><title>Native user concept meshes</title><style>body{background:#183f49;color:#ffe4b5;font:16px system-ui}svg{width:320px;max-height:420px;background:#315e65;margin:16px}</style><h1>Native meshes — source PNG pixels unchanged</h1><p>Reference review only. Vector clipping mirrors the Unity native outline; no raster was edited or exported. These new enemy crops remain TEMPORARY pending visual acceptance.</p>'
 for i,cut in enumerate(native['slices']):
  w,h=cut['width'],cut['height'];points=' '.join(str(p['x']+w/2)+','+str(h/2-p['y']) for p in cut['outline']);top=1086-cut['y']-h
  html+=f'<h2>{cut["name"]}</h2><svg viewBox="0 0 {w} {h}"><defs><clipPath id="cut{i}"><polygon points="{points}"/></clipPath></defs><image x="{-cut["x"]}" y="{-top}" width="1448" height="1086" href="../../../{native["sheet"]}" clip-path="url(#cut{i})"/><polygon points="{points}" fill="none" stroke="#e9c16d" stroke-width=".5"/></svg>'
 (out/'NativeMeshReview.html').write_text(html,encoding='utf-8')
 print('Art audit:',len(bound),'NPC bindings;',len(missing),'missing portraits; no approved sprite changed.')

def verify():
 changed=protect();summary=audit();gallery();assets()
 assert summary['freshE2E'] and summary['reverseE2E'] and passed('Coverage03')
 assert summary['nodes']==133 and summary['quests']==29 and summary['questStatuses']=={'IMPLEMENTED':29}
 assert summary['nodeStatuses'].get('NOT_VERIFIED',0)==0
 runs=['Fresh04','Reverse03','Coverage03','Negative-S16','Negative-S17','Negative-S18','Restart-HA','Restart-Hue','Restart-DaLat','Restart-Jeonju','Restart-Gyeongju','Restart-BeforeBoss','Restart-Epilogue','Restart-Ending'];records=[];images=[]
 for name in runs:
  p=V/name/('restart-beacon-results.txt' if name=='Restart-HA' else 'smoke-results.txt');lines=p.read_text(encoding='utf-8-sig').splitlines()
  assert lines[-1]=='ALL CHECKS PASSED' and not any('FAIL:' in l or l.startswith('ERROR:') for l in lines),name
  records.append(dict(run=name,type='NEGATIVE_FIXTURE_RESTORED_NOT_STORY_PROOF' if name.startswith('Negative') else 'EARNED_SAVE_PROCESS_RESTART' if name.startswith('Restart') else 'EARNED_POSTGAME_INTERACTIONS' if name.startswith('Coverage') else 'EARNED_FRESH_ORIGIN_CONTINUATION',assertions=sum(l.startswith('PASS:') for l in lines),resultsSHA256=sha(p)))
 for child,parent,seed in [('Fresh03','Fresh02','gameplay-save.json'),('Fresh04','Fresh03','jeonju-before.json'),('Reverse03','Reverse02','gameplay-save.json')]:
  lineage=read(V/child/'lineage.json');assert lineage['seedSHA256'].lower()==sha(V/parent/seed);assert lineage['progressFieldsEdited'] is False
 for name in ['Fresh02','Reverse02']:
  text=(V/name/'smoke-results.txt').read_text(encoding='utf-8-sig');assert 'Gyeongju unlock condition is earned while its absent runtime remains disabled' in text;assert 'Fresh isolated save starts H0 with no accepted tutorial quest' in text and 'Fresh arrival: discovery stage 0' in text
 assert 'Missing-item NPC gives a recovery direction to H10' in (V/'Fresh03/smoke-results.txt').read_text(encoding='utf-8-sig')
 genuine=read(V/'Fresh03/jeonju-before.json');assert genuine['items'].count('hoi_an_fresh_chili')==1 and genuine['items'].count('da_lat_strawberry')==1 and 'HQ05' not in genuine['completed']
 fresh=read(V/'Fresh04/full-story-final.json');reverse=read(V/'Reverse03/full-story-final.json');coverage=read(V/'Coverage03/full-coverage-final.json')
 for s in [fresh,reverse,coverage]:
  assert all(f in s['flags'] for f in ['hoi_an.lamp_restored','hue.beacon_lit','da_lat.beacon_lit','jeonju.beacon_lit','gyeongju.beacon_lit','seoul.relay_a_on','seoul.relay_b_on','seoul.relay_c_on','seoul.final_cleared','game.ending_seen'])
  assert not any(f.startswith(('qa:','qa.','QA_')) for f in s['flags'])
  for item in ['ancient_seal','stargazer_lens','blue_water_signal','hanji_circuit','festival_friendship_postcard']:assert s['items'].count(item)==1,(s['cityId'],item)
  assert s['items'].count('da_lat_strawberry')==0
 assert 'gyeongju.stone_guard_friend' in fresh['flags'] and 'seoul.system_repaired' in fresh['flags'] and 'enemy_final_core' not in fresh['defeatedEnemies']
 assert 'gyeongju.stone_guard_friend' not in reverse['flags'] and 'seoul.system_repaired' not in reverse['flags'] and 'enemy_final_core' in reverse['defeatedEnemies']
 early=read(V/'Reverse03/full-gyeongju-beacon.json');assert 'hue.beacon_lit' not in early['flags'] and 'da_lat.beacon_lit' not in early['flags'] and 'gyeongju.beacon_lit' in early['flags']
 middle=read(V/'Reverse03/full-ending-middle.json');assert 'game.ending_seen' not in middle['flags'] and 'SQ03' not in middle['completed'];assert 'game.ending_seen' in read(V/'Restart-Epilogue/gameplay-save.json')['flags']
 d=read(R/'Assets/_Game/Resources/Content/GameplayContent.json');frozen=read(V/'Build04-Content.json');display=read(OUT/'friendly-labels-delta.json');allowed={(r['city'],r['node']) for r in display['changes']}
 for e,old in zip(d['events'],frozen['events']):
  if (e['cityId'],e['nodeId']) in allowed and not e.get('additive'):
   if 'missingText' in old:e['missingText']=old['missingText']
   else:e.pop('missingText',None)
 assert d==frozen,'Gameplay data changed after the build04 freeze'
 asm=R/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
 for p in (R/'Assets/_Game/Scripts').rglob('*.cs'):
  if 'Editor' not in p.parts:assert p.stat().st_mtime<=asm.stat().st_mtime,p
 for name in runs+['Fresh02','Reverse02','Fresh03']:
  build='04' if name in ['Fresh02','Reverse02'] else '05' if name in ['Fresh03','Reverse03'] else '06'
  for p in (V/name).glob('*.png'):
   assert p.stat().st_mtime>=(R/f'Logs/full-story-build-{build}.log').stat().st_mtime,p
   images.append(dict(path=p.relative_to(R).as_posix(),build=build,sha256=sha(p)))
 for i in range(1,7):assert 'Windows build: Succeeded' in (R/f'Logs/full-story-build-0{i}.log').read_text(encoding='utf-8-sig')
 result=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),status='SIX_CHAPTER_STORY_E2E_PASS_WITH_EARNED_SAVE_RESUMES_ART_TERRAIN_PENDING',finalBuild='06',assemblySHA256=sha(asm),changedBaselineFiles=changed,audit=summary,runs=records,assertions=sum(r['assertions'] for r in records),screenshots=images,lineage='Fresh02 -> Fresh03 before-fixture checkpoint -> Fresh04; Reverse02 -> Reverse03. Historical failed assertions disclosed; no synthetic completion flags.',limitations=['Seven nodes retain presentation differences; seven NPC portraits missing; new enemy art temporary.','Terrain geometry awaits visual acceptance. Android/device NOT_VERIFIED.','Reverse end-to-end used build05; build06 changes display-only NPC missing-item notes and QA check-in coverage. Fresh04 and all coverage/negative/restart checks use06.'])
 write(V/'evidence-manifest.json',result);print('FINAL VERIFIED',result['assertions'],'checks;',len(images),'fresh runtime PNGs; all29 quests; both earned-origin orders.');return result

def checkpoint(stage):
 target=R.parent/'Backups'/('CheckpointFullStory'+stage+'-20261017');assert not target.exists(),target
 changed=protect();summary=audit();gallery();assets()
 if stage=='Complete':verify()
 paths={e['path'] for e in read(B/'CHECKPOINT_MANIFEST.json')['entries'] if not e['path'].startswith(('Builds/','Validation/')) and (R/e['path']).is_file()}
 for folder in ['Assets','Packages','ProjectSettings','Docs','Tools','References','Builds/Windows']:
  paths.update(p.relative_to(R).as_posix() for p in (R/folder).rglob('*') if p.is_file() and '__pycache__' not in p.parts)
 paths.update(p.relative_to(R).as_posix() for p in R.glob('*.md'));paths.update(['FullStoryReview.html'])
 closed={p.parent.name for p in V.glob('*/smoke-results.txt') if p.read_text(encoding='utf-8-sig').rstrip().endswith(('ALL CHECKS PASSED','FAILED'))}
 for p in V.rglob('*'):
  if p.is_file() and (len(p.relative_to(V).parts)==1 or p.relative_to(V).parts[0] in closed):paths.add(p.relative_to(R).as_posix())
 paths.update(p.relative_to(R).as_posix() for p in (R/'Logs').glob('full-story-build-*.log'))
 entries=[]
 for name in sorted(paths):
  src=R/name;dst=target/name;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);digest=sha(src);assert digest==sha(dst),name;entries.append(dict(path=name,bytes=src.stat().st_size,sha256=digest))
 for e in entries:assert sha(R/e['path'])==e['sha256'],'Source changed during checkpoint: '+e['path']
 write(target/'CHECKPOINT_MANIFEST.json',dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),source=str(R),type='FULL_SOURCE_BUILD_SELECTED_RUNTIME_EVIDENCE',stage=stage,status='FULL_STORY_E2E_PASS' if stage=='Complete' else 'AUTHORED_BUILD_PASS_FRESH_E2E_PENDING',verifiedSHA256=True,fileCount=len(entries),totalBytes=sum(e['bytes'] for e in entries),baseline=str(B),changedBaselineFiles=changed,audit=summary,queuedTasks=[4],limitations=['Temporary art and seven presentation differences are listed separately; terrain acceptance and Android device remain pending.','Intermediate snapshots are not completion of the whole-game request.'],entries=entries))
 print('CHECKPOINT VERIFIED',target,len(entries),sum(e['bytes'] for e in entries))
if __name__=='__main__':
 if '--checkpoint' in sys.argv:checkpoint(sys.argv[sys.argv.index('--checkpoint')+1])
 elif '--verify' in sys.argv:verify()
 else:audit();gallery();assets();print('Protected file changes:',protect())
