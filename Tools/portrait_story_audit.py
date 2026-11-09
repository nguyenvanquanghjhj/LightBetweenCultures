"""Read canonical source records and actual earned saves; never promotes visits to gameplay.
Updates the existing art catalog separately; no second runtime registry is created.
"""
import collections,csv,hashlib,html,json,re
from pathlib import Path
from PIL import Image
R=Path(__file__).resolve().parents[1]
V=R/'Validation/PortraitStory-20261017'
OUT=R/'Docs/Story/PortraitStory-20261017'
ART=R/'Docs/Art/PortraitStory-20261017'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,d):
    p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
GAPS={
 'H2':'Main quest/tutorial chạy; chưa có ba bước đổi ánh sáng riêng tại tiệm sau mỗi linh kiện như mô tả cũ.',
 'H4':'Puzzle sequence chạy; chưa có thao tác hoán đổi ba ô hình như FULL_LEVEL_DESIGN mô tả.',
 'H9':'Chuỗi cứu đèn/thư có flag thật; chưa có đồng hành Đèn Lồng Nhỏ hiển thị/hoạt động.',
 'H11':'Rương hiện trao 6 xu phục vụ đò; FULL_LEVEL_DESIGN ghi dây đồng. Giữ gameplay checkpoint, báo khác biệt thay vì tự đổi thưởng.',
 'U8':'Chuỗi ba ký hiệu chạy; presentation chưa phải ba lá cờ tương tác trên sân.',
 'U20':'Tuyến vào Đà Lạt qua World Map chạy; thao tác trực tiếp nút portal tại U20 cần proof riêng.',
 'D8':'Puzzle chuỗi thông–đèn–hồ chạy; chưa là cảnh chọn ba cổng rẽ.',
 'D12':'Puzzle chọn chuỗi hướng chạy; chưa có cánh cối xay xoay trực tiếp.',
 'D18':'Rest/HP và lời tự thoại Lumi chạy; chưa có portrait/nội dung riêng của chủ nhà nghỉ.',
 'D21':'Tín hiệu Đà Lạt đã lưu; rơle A/đi Seoul còn thuộc chương DATA_ONLY.',
 'J3':'Khôi phục thứ tự màu bằng câu đố; chưa có tương tác nhuộm vải trực quan.',
 'J19':'Trạm Jeonju chơi được; Gyeongju chưa runtimeReady nên chưa đi tiếp chương đó.',
 'J20':'Cổng có dữ liệu/điều kiện, đích Gyeongju DATA_ONLY; chưa đi tiếp được.'
}

def main():
    OUT.mkdir(parents=True,exist_ok=True);ART.mkdir(parents=True,exist_ok=True)
    runtime=load(R/'Assets/_Game/Resources/Content/GameplayContent.json')
    canonical={c:load(R/f'LBC_Gameplay_6Cities_Detailed_v1/Data/Cities/{c}.json') for c in ['hoi_an','hue','da_lat','jeonju','gyeongju','seoul']}
    definitions={name:load(R/f'LBC_Gameplay_6Cities_Detailed_v1/Data/{name}.json') for name in ['quests','enemies','items','world_progression']}
    qmap={q['id']:q for q in runtime['quests']};pmap={p['id']:p for p in runtime['puzzles']};enmap={e['id']:e for e in runtime['enemies']}
    # Each proof is a real UI journey. The second starts with the first's earned save.
    proofs=[]
    for path in [V/'Journey04/dalat-final.json',V/'UI04/polish-ferry-earned.json',V/'Reverse04/hoi_an-after.json']:
        if path.exists():proofs.append((str(path.relative_to(R)),load(path)))
    def proved(field,value):return next((path for path,s in proofs if value in s.get(field,[])),None)
    bindings=[b for b in load(R/'Docs/Art/native-art-bindings.json')['entries'] if b['role']=='npc']
    bmap={b['id']:b for b in bindings}
    observations=[]
    for folder in ['Journey04','Diagnostics04','UI04','Reverse04']:
        path=V/folder/'portrait-runtime-observations.json'
        if path.exists():observations += [dict(run=folder,**x) for x in load(path)]
    nodes=[];npcplacements=collections.defaultdict(list)
    for city in runtime['cities']:
        source=canonical[city['id']];cn={n['id']:n for n in source['nodes']};ce={e['node']:e for e in source['events']}
        assert set(cn)=={n['id'] for n in city['nodes']}
        for node in city['nodes']:
            events=[e for e in runtime['events'] if e['cityId']==city['id'] and e['nodeId']==node['id']]
            primary=next((e for e in events if e['id']==node['eventId']),None)
            qs=[q for q in runtime['quests'] if (q.get('giverCityId',q['cityId'])==city['id'] and q['giverNode']==node['id']) or (q.get('completeCityId',q['cityId'])==city['id'] and q['completeAtNode']==node['id'])]
            for e in events:
                if e.get('npcId'):npcplacements[e['npcId']].append((city['id'],node['id'],e.get('actor','')))
            # Runtime node order is stable and may differ from raw canonical order.
            idx=city['nodes'].index(node)
            visited=next((path for path,s in proofs if any(cp['cityId']==city['id'] and idx in cp['visited'] for cp in s['cityProgress'])),None)
            stateEvidence=[]
            for e in events:
                for field,value in [('resolvedEvents',city['id']+':'+e['id']),('defeatedEnemies',e.get('enemyId')),('solvedPuzzles',e.get('puzzleId')),('completed',e.get('questId')),('flags',e.get('completionFlag'))]:
                    if value and (path:=proved(field,value)):stateEvidence.append(dict(field=field,value=value,save=path))
            if primary and primary['type']=='exploration' and primary.get('sceneId')=='HoiAnDemo':
                # Existing H3 pickup code writes its unique grant, rather than
                # resolving the repeatable scene-entry event on the City Map.
                for field,value in [('rewardGrantIds','h3-lamp-eye'),('flags','hoi_an.lamp_eye_found')]:
                    if path:=proved(field,value):stateEvidence.append(dict(field=field,value=value,save=path))
            restarts=[]
            for result in V.glob('Restart04-*/smoke-results.txt'):
                if not result.read_text(encoding='utf-8-sig').rstrip().endswith('ALL CHECKS PASSED'):continue
                restored=load(result.parent/'gameplay-save.json')
                if restored['cityId']==city['id']:restarts.append(dict(result=str(result.relative_to(R)),node=restored['node']))
            if not city['runtimeReady']:status='DATA_ONLY';reason='City runtimeReady=false; không dùng handler/JSON để suy ra đã chơi được.'
            elif node['id'] in GAPS:status='PARTIAL';reason=GAPS[node['id']]
            elif primary and primary['type'] in ('start','junction') and visited:status='IMPLEMENTED';reason='Vai trò điều hướng/spawn đã đi tới thật; không khẳng định mọi hành động phụ được test.'
            elif stateEvidence:status='IMPLEMENTED';reason='Có trạng thái sự kiện/quest/puzzle/encounter được đạt qua UI, không chỉ visit.'
            else:status='NOT_VERIFIED';reason='Có runtime data/handler nhưng chưa có bằng chứng thực thi riêng trong các run mới.'
            before={q['id']:q.get('offer','') for q in qs};during={q['id']:q.get('reminder','') for q in qs};after={q['id']:q.get('thanks','') for q in qs}
            itemflags=[dict(event=e['id'],itemId=e.get('itemId'),rewardItems=e.get('rewardItems',[]),rewardCoins=e.get('rewardCoins',0),flags=e.get('flags',[]),completionFlag=e.get('completionFlag'),oneShot=e.get('oneShot'),autoCollect=e.get('autoCollect')) for e in events]
            row=dict(cityId=city['id'],id=node['id'],name=node['name'],status=status,reason=reason,
                storyRole=ce[node['id']].get('interaction',cn[node['id']].get('description','')),
                canonicalNode=cn[node['id']],canonicalEvent=ce[node['id']],runtimeNode=node,runtimeEvents=events,
                relatedEnemies=[enmap[e['enemyId']] for e in events if e.get('enemyId') in enmap],
                relatedPuzzles=[pmap[e['puzzleId']] for e in events if e.get('puzzleId') in pmap],
                quests=qs,dialogueBefore=before,dialogueDuring=during,dialogueAfter=after,
                eventDialogue=[dict(event=e['id'],before=e.get('text',''),after=e.get('completedText','')) for e in events],
                itemRewardFlags=itemflags,gateReveal={k:node.get(k) for k in ['revealStage','previewStages','isKeyGate','gateCondition','unlockStage','requiredFlags']},
                visitedProof=visited,stateProof=stateEvidence,
                restartStatus=dict(savedProgress='PASS' if restarts and stateEvidence else 'NOT_VERIFIED',
                    restartAtThisNode='PASS' if any(p['node']==idx for p in restarts) else 'NOT_VERIFIED',evidence=restarts),
                npcIds=sorted({e['npcId'] for e in events if e.get('npcId')}))
            nodes.append(row)
    # Include the map merchant at H4 even though the primary event is a puzzle.
    presentation=load(R/'Assets/_Game/Resources/Content/AdventurePresentation.json')
    for actor in presentation['mapActors']:
        if actor['artId'].startswith('npc_'):npcplacements[actor['artId']].append((actor['cityId'],actor['nodeId'],'map actor'))
    npcrows=[]
    for npc in sorted(set(npcplacements)|set(bmap)):
        b=bmap.get(npc);seen=[x for x in observations if x['npcId']==npc and x['sprite']!='MISSING']
        quality={};candidate=''
        if npc=='npc_min':candidate='NPC_Roster_Concept có hướng dẫn viên khảo cổ nhí chưa gán; cần slice và duyệt danh tính Min, không cần vẽ lại roster.'
        if npc=='npc_tech':candidate='Concept cô gái công nghệ đang dùng tạm cho So-yeon; không được tái gán làm Ji-won.'
        if npc=='npc_innkeeper':candidate='D18 đang do Lumi tự thoại; chưa có hình riêng được xác nhận cho chủ nhà nghỉ. Không tự đổi đầu bếp/nhân vật có vai trò khác thành chủ trọ.'
        if b:
            im=Image.open(R/b['path']);alpha=im.getchannel('A') if 'A' in im.getbands() else None
            meta=(R/(b['path']+'.meta')).read_text(encoding='utf-8-sig')
            pattern=r'name: '+re.escape(b['spriteName'])+r'\s+rect:\s+serializedVersion: \d+\s+x: (\S+)\s+y: (\S+)\s+width: (\S+)\s+height: (\S+)'
            m=re.search(pattern,meta);rect=list(map(float,m.groups())) if m else [0,0,*im.size]
            pivot=re.search(r'name: '+re.escape(b['spriteName'])+r'.*?pivot:\s*\{x: ([^,]+), y: ([^}]+)\}',meta,re.S)
            if not pivot:pivot=re.search(r'spritePivot:\s*\{x: ([^,]+), y: ([^}]+)\}',meta)
            quality=dict(mode=im.mode,texturePixels=list(im.size),alphaExtrema=list(alpha.getextrema()) if alpha else None,spriteRect=rect,
                pivot=list(map(float,pivot.groups())) if pivot else 'NEEDS_REVIEW: pivot metadata not found',
                meshPolicy='explicit reviewed sheet mask' if npc in ('npc_ba_tu','npc_phuc') else 'RGBA quad in dialogue; actor mesh unchanged',
                cropReview='Phuc outline excludes adjacent basket; raised hand/hat/feet must be reviewed in fresh runtime' if npc=='npc_phuc' else 'See runtime portraits; metrics alone do not certify edge/crop quality')
        names={p[2] for p in npcplacements[npc] if p[2]!='map actor'}|{x['speaker'] for x in observations if x['npcId']==npc}
        if npc=='npc_guest':names.add('Ji-eun')
        npcrows.append(dict(npcId=npc,names=sorted(names),placements=sorted(set(npcplacements[npc])),binding=b,availableCandidateOrConstraint=candidate,
            identityStatus='MISSING_RUNTIME_BINDING' if not b else 'EXISTING_APPROVED_IDENTITY' if b.get('canonical') else 'TEMPORARY_OR_IDENTITY_APPROVAL_PENDING',
            runtimeStatus='VERIFIED_VISIBLE' if seen else 'NOT_VERIFIED',runtimeObservations=seen,quality=quality,
            sharedWith=[other['id'] for other in bindings if b and other['id']!=npc and (other['guid'],other['localId'])==(b['guid'],b['localId'])]))
    questrows=[];canonicalq={q['id']:q for q in definitions['quests']['quests']}
    for q in runtime['quests']:
        city=next(c for c in runtime['cities'] if c['id']==q['cityId']);proof=proved('completed',q['id'])
        status='DATA_ONLY' if not city['runtimeReady'] else 'IMPLEMENTED' if proof else 'NOT_VERIFIED'
        gaps=[]
        if q['id']=='HQ03':status='PARTIAL';gaps.append('Cứu đèn/flag chạy; đồng hành Đèn Lồng Nhỏ chưa triển khai.')
        if q['id']=='UQ04':gaps.append('Giao/đọc ấn và giữ item chạy; tiêu thụ tín hiệu tại G12/S17 DATA_ONLY.')
        if q['id'] in ('JQ03','DQ03'):gaps.append('Hoàn thành trạm địa phương; chương nhận tín hiệu kế tiếp còn DATA_ONLY.')
        questrows.append(dict(id=q['id'],status=status,earnedSave=proof,canonical=canonicalq.get(q['id']),runtime=q,gaps=gaps))
    sourcefiles=list((R/'LBC_Gameplay_6Cities_Detailed_v1/Data').rglob('*.json'))+[R/'LBC_Gameplay_6Cities_Detailed_v1/Docs/FULL_LEVEL_DESIGN.md',R/'LBC_Gameplay_6Cities_Detailed_v1/Docs/QUEST_CHAINS_AND_GATES.md']
    dump(OUT/'source-records.json',dict(files=[dict(path=str(p.relative_to(R)),sha256=sha(p)) for p in sourcefiles],canonicalCities=canonical,definitions=definitions))
    dump(OUT/'node-audit.json',nodes);dump(OUT/'quest-audit.json',questrows);dump(ART/'npc-portrait-audit.json',npcrows)
    def esc(s):return str(s).replace('|',' / ').replace('\n','<br>')
    lines=['# Kiểm toán gameplay 133 node','', 'IMPLEMENTED chỉ xét vai trò gameplay được chứng minh; không đồng nghĩa art/terrain/chương đã final. PARTIAL ghi rõ khác biệt canonical. DATA_ONLY là thành phố chưa mở runtime. NOT_VERIFIED không được tính là đã hoàn thành.','', '| City / Node | Vai trò / event | NPC / enemy / puzzle | Quest | Gate / reveal | State proof | Trạng thái / thiếu |','|---|---|---|---|---|---|---|']
    for n in nodes:
        events=n['runtimeEvents'];refs=','.join(n['npcIds']+[e.get('enemyId','') or e.get('puzzleId','') for e in events if e.get('enemyId') or e.get('puzzleId')])
        lines.append('| '+' | '.join(map(esc,[n['cityId']+' / '+n['id']+' '+n['name'],n['storyRole']+'; '+','.join(e['id']+':'+e['type'] for e in events),refs,','.join(q['id'] for q in n['quests']),json.dumps(n['gateReveal'],ensure_ascii=False),','.join(p['value'] for p in n['stateProof']) or 'không có proof thực thi riêng',n['status']+' — '+n['reason']]))+' |')
    lines+=['','Dữ liệu đầy đủ trước/trong/sau nhiệm vụ, điều kiện nhận/trả, reward/item/flag và nguồn save theo từng node: [node-audit.json](node-audit.json). Các JSON này là báo cáo, không phải hệ thống runtime thứ hai.']
    (OUT/'node-audit.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    with (OUT/'node-audit.csv').open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=['cityId','id','name','status','storyRole','reason','quests','gateReveal','itemRewardFlags','dialogueBefore','dialogueDuring','dialogueAfter','stateProof']);w.writeheader()
        for n in nodes:w.writerow({k:json.dumps(n[k],ensure_ascii=False) if isinstance(n[k],(list,dict)) else n[k] for k in w.fieldnames})
    questlines=['# Quest chính/phụ — bằng chứng thật','', '| Quest | Trạng thái | Nơi nhận → trả | Điều kiện / reward | Proof / phần thiếu |','|---|---|---|---|---|']
    for q in questrows:
        d=q['runtime'];questlines.append('| '+' | '.join(map(esc,[q['id']+' '+d['title'],q['status'],d['giverNode']+' → '+d['completeAtNode'],json.dumps({k:d.get(k) for k in ['acceptFlags','acceptQuests','requiredItems','requiredFlags','requiredQuests','rewardItems','rewardCoins','completionFlag']},ensure_ascii=False),(q['earnedSave'] or 'Chưa có hoàn thành thật')+'; '+' '.join(q['gaps'])]))+' |')
    (OUT/'quest-audit.md').write_text('\n'.join(questlines)+'\n',encoding='utf-8')
    npclines=['# NPC portrait — sáu thành phố','', 'Cùng GUID atlas chưa có nghĩa cùng khuôn mặt: phải so cả localId/spriteName. Một NPC tái xuất nhiều map giữ cùng ID. Không gán So-yeon cho Ji-won.','', '| NPC / nơi xuất hiện | Sprite / nguồn | Nhận diện | Runtime | Trùng ngoài ý muốn |','|---|---|---|---|---|']
    for n in npcrows:
        b=n['binding'];npclines.append('| '+' | '.join(map(esc,[n['npcId']+' / '+str(n['placements']),(b['path']+'#'+b['spriteName']) if b else 'MISSING; chưa gán khuôn mặt khác',n['identityStatus'],n['runtimeStatus'],n['sharedWith'] or 'Không có binding trùng']))+' |')
    npclines+=['','Chi tiết RGBA/pixel/rect/pivot/mesh/crop và lời thoại runtime: [npc-portrait-audit.json](npc-portrait-audit.json). Những số đo alpha không thay nghiệm thu trực quan.']
    (ART/'npc-portrait-audit.md').write_text('\n'.join(npclines)+'\n',encoding='utf-8')
    summary=dict(nodes=len(nodes),nodeStatuses=dict(collections.Counter(n['status'] for n in nodes)),quests=len(questrows),questStatuses=dict(collections.Counter(q['status'] for q in questrows)),npcCount=len(npcrows),missingNpcBindings=[n['npcId'] for n in npcrows if not n['binding']],temporaryNpc=[n['npcId'] for n in npcrows if n['binding'] and not n['binding']['canonical']],unusedOrUnverifiedPortraits=[n['npcId'] for n in npcrows if n['runtimeStatus']!='VERIFIED_VISIBLE'],fullStoryRequiredPathExists=(R/'Docs/Story/FULL_STORY_PLAYTEST.md').exists())
    dump(OUT/'audit-summary.json',summary);print(json.dumps(summary,ensure_ascii=False))

if __name__=='__main__':main()
