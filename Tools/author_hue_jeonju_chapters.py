"""Author the two canonical chapters on the existing runtime. No source art edits.

Puzzle answers, linking dialogue and two garden corridors are explicit proposals
where the design gives a mechanic but no executable interaction/geometry.
"""
from pathlib import Path
import copy,json
ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT.parent/'Backups/BeforeHueJeonjuChapters-20261016'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def point(x,y):return dict(x=round(x/1448,9),y=round(1-y/1086,9))
def items(**values):return [dict(itemId=k,amount=v) for k,v in values.items()]

def main():
    path=ROOT/'Assets/_Game/Resources/Content/GameplayContent.json';d=read(path)
    before=read(BASE/'Assets/_Game/Resources/Content/GameplayContent.json')
    presentation_path=ROOT/'Assets/_Game/Resources/Content/AdventurePresentation.json';a=read(presentation_path)
    def city(id):return next(c for c in d['cities'] if c['id']==id)
    def event(cid,nid):return next(e for e in d['events'] if e['cityId']==cid and e['nodeId']==nid and not e.get('additive'))
    def quest(id,**values):
        q=next(q for q in d['quests'] if q['id']==id);q.update(values,pending=False,sourceStatus='canonical_LBC_v1; authored_runtime_dialogue_and_interaction_proposal')
        q['giverCityId']=q['cityId'];q.setdefault('completeCityId',q['cityId']);return q
    def additional(cid,nid,qid,action):
        key='chapter_'+qid.lower()+'_'+action
        d['events']=[e for e in d['events'] if e['id']!=key]
        base=event(cid,nid)
        d['events'].append(dict(id=key,cityId=cid,nodeId=nid,type='quest',actor=base['actor'],npcId=base.get('npcId',''),questId=qid,additive=True,
                              text=base['text'],sourceStatus='canonical_quest_location; authored_interaction'))
    def puzzle(cid,nid,answer,options,clue,flags=(),rewards=None,required=(),required_items=None,mode='sequence'):
        e=event(cid,nid);pid=e['puzzleId'];p=next(p for p in d['puzzles'] if p['id']==pid)
        p.update(mode=mode,answer=answer,options=options,clue=clue,requiredFlags=list(required),requiredItems=required_items or [],
                 flags=list(flags),rewardItems=rewards or [],sourceStatus='canonical_mechanic_and_reward; concrete_symbols_answer_hint_are_runtime_proposals')
        e.update(requiredFlags=list(required),requiredItems=required_items or [])
    def actor(cid,nid,aid,size=(52,62)):
        a['mapActors']=[v for v in a['mapActors'] if (v['cityId'],v['nodeId'])!=(cid,nid)]
        a['mapActors'].append(dict(cityId=cid,nodeId=nid,artId=aid,size=dict(zip(('x','y'),size)),offset=dict(x=22,y=8),hideWhenResolved=aid.startswith('enemy')))
    reviews=[]
    # Preserve the four reviewed opening anchors. Other source anchors were marked
    # coordinates_are_approximate; align their feet to the visible pavement.
    positions={
        'hue':{2:(665,786),3:(545,543),4:(880,456),5:(872,402),6:(884,300),7:(1110,247),8:(1047,203),9:(700,206),10:(561,266),11:(728,316),12:(416,316),13:(1047,911),14:(1210,780),15:(470,605),16:(346,701),17:(1200,161),18:(974,281),19:(1146,214),20:(1313,322),21:(1209,362)},
        'jeonju':{1:(666,466),2:(961,570),3:(440,396),4:(626,290),5:(976,683),6:(1175,694),7:(483,249),8:(349,479),9:(174,615),10:(227,899),11:(1113,171),12:(1077,375),13:(805,193),14:(1242,441),16:(613,592),17:(782,450),18:(1163,977),19:(880,206),20:(1300,217),21:(219,249)}}
    stages={
        'hue':[[0,1,2,3,14,15,16],[4,7,8,10,11,12,13,17],[5],[6,9,18,19],[20,21]],
        'jeonju':[[0,1,2,16],[3,4,5,6,7,8,9,10,11,12,13,21],[14,15,17,18],[19],[20]]}
    for cid in positions:
        c=city(cid);c.update(runtimeReady=True,runtimeNodeIds=[],routesReviewed=False,mapTitle='HUẾ · DẤU ẤN HAI BỜ' if cid=='hue' else 'JEONJU · HOA VĂN HANJI')
        prefix='U' if cid=='hue' else 'J'
        for n in c['nodes']:
            number=int(n['id'][1:]);n['revealStage']=next(stage for stage,ids in enumerate(stages[cid]) if number in ids);n['previewStages']=[]
            if number in positions[cid]:
                original=next(v for v in next(v for v in before['cities'] if v['id']==cid)['nodes'] if v['id']==n['id'])['position']
                n['position']=point(*positions[cid][number]);reviews.append(dict(cityId=cid,nodeId=n['id'],old=original,new=n['position'],reason='Unverified approximate design anchor aligned to visible pavement/landing; reviewed opening anchors unchanged.'))
        for e in d['events']:
            if e['cityId']==cid and not e.get('additive'):e['sourceStatus']='canonical_LBC_v1; runtime_authored_verification_required'

    # Two explicit links allow scholarly/seal alternatives before the compulsory
    # inner gate. None enters stage 3 or the beacon without resolving U5.
    hue=city('hue')
    for start,end in [('U4','U12'),('U11','U7')]:
        if not any({e['from'],e['to']}=={start,end} for e in hue['edges']):
            hue['edges'].append(dict(id=start+'__'+end,**{'from':start,'to':end},bidirectional=True,travel='walk',requiredFlags=['hue.crossing_permit'],waypoints=[]))
    # Geometry was traced on each original image, not copied across cities.
    corridors={
      'hue':{
        'U1__U2':[(410,696),(450,714),(488,737),(541,756),(596,774)],
        'U1__U15':[(410,696),(435,682),(458,667),(478,649),(494,630),(486,614)],
        'U1__U16':[(369,707),(356,708)],
        'U15__U3':[(494,630),(527,624),(548,602),(558,576),(550,555)],
        'U2__U14':[(705,808),(751,832),(801,835),(832,813),(854,779),(877,752),(922,744),(971,757),(1013,785),(1063,805),(1115,785),(1154,769),(1185,772)],
        'U14__U13':[(1195,789),(1165,813),(1131,834),(1094,846),(1067,872)],
        'U14__U3':[(1185,772),(1154,769),(1115,785),(1063,805),(1013,785),(971,757),(922,744),(877,752),(854,779),(832,813),(801,835),(751,832),(705,808),(665,786),(596,774),(541,756),(488,737),(450,714),(435,682),(458,667),(478,649),(494,630),(527,624),(548,602),(558,576),(550,555)],
        'U3__U4':[(563,525),(596,511),(634,506),(674,507),(716,515),(755,512),(793,496),(831,480),(857,465)],
        'U4__U5':[(887,445),(887,427),(878,412)],
        'U5__U6':[(865,385),(855,369),(860,348),(875,322)],
        'U6__U7':[(913,298),(943,291),(974,281),(1014,269),(1061,258)],
        'U6__U11':[(860,305),(831,313),(800,328),(769,331)],
        'U6__U18':[(913,298),(943,291)],
        'U11__U10':[(700,313),(666,300),(628,281),(593,270)],
        'U10__U9':[(588,253),(612,238),(648,221),(675,210)],
        'U9__U12':[(675,210),(648,221),(612,238),(588,253),(561,266),(533,276),(491,294),(451,308)],
        'U9__U8':[(741,214),(781,229),(823,248),(871,270),(913,280),(956,264),(997,237)],
        'U8__U7':[(1066,218),(1084,233)],
        'U8__U17':[(1070,185),(1095,164),(1132,151),(1168,152)],
        'U17__U19':[(1189,176),(1171,195)],
        'U18__U8':[(995,263),(1020,237)],
        'U7__U20':[(1140,263),(1167,280),(1201,298),(1242,319),(1280,328)],
        'U20__U21':[(1280,328),(1247,339),(1225,350)],
        'U12__U11':[(451,308),(491,294),(533,276),(561,266),(593,270),(628,281),(666,300),(700,313)],
        'U4__U14':[(862,471),(842,487),(817,509),(795,528),(775,545),(737,575),(730,608),(751,642),(789,669),(830,693),(854,718),(877,752),(922,744),(971,757),(1013,785),(1063,805),(1115,785),(1154,769),(1185,772)],
        'U4__U12':[(851,453),(814,441),(766,424),(713,407),(658,388),(601,367),(541,348),(480,328)],
        'U11__U7':[(769,331),(800,328),(831,313),(860,305),(884,300),(913,298),(943,291),(974,281),(1014,269),(1061,258)]},
      'jeonju':{
        'J0__J10':[(696,965),(660,943),(621,916),(581,893),(541,868),(503,850),(462,828),(416,804),(375,792),(337,793),(302,813),(270,845),(246,878)],
        'J0__J16':[(696,965),(660,943),(621,916),(581,893),(541,868),(503,850),(462,828),(435,805),(441,777),(467,748),(499,722),(531,697),(564,667),(587,631)],
        'J16__J1':[(600,565),(608,534),(631,508),(653,486)],
        'J1__J2':[(700,481),(737,491),(778,506),(821,523),(865,541),(913,560)],
        'J1__J3':[(639,482),(605,478),(564,464),(526,444),(491,422),(465,405)],
        'J1__J17':[(700,481),(737,491),(760,478)],
        'J2__J5':[(981,594),(999,618),(1001,641),(989,664)],
        'J5__J6':[(1008,698),(1042,712),(1077,715),(1112,705),(1142,698)],
        'J5__J15':[(989,705),(1012,737),(992,760)],
        'J6__J18':[(1197,714),(1199,742),(1184,765),(1157,791),(1135,820),(1125,854),(1128,888),(1145,928),(1157,956)],
        'J18__J0':[(1135,978),(1103,961),(1072,940),(1032,931),(994,943),(962,961),(922,984),(874,999),(823,1018),(771,1010),(740,996)],
        'J3__J8':[(416,414),(393,439),(371,460)],
        'J8__J9':[(322,473),(295,479),(270,493),(243,515),(213,548),(189,581)],
        'J9__J10':[(173,642),(195,669),(229,695),(263,716),(295,739),(319,763),(312,802),(281,842),(248,878)],
        'J8__J21':[(322,473),(295,462),(267,443),(247,415),(241,381),(237,347),(234,311),(226,276)],
        'J3__J4':[(469,377),(498,355),(531,335),(568,318),(600,301)],
        'J4__J7':[(596,280),(565,270),(531,263),(503,254)],
        'J4__J13':[(653,270),(680,246),(714,225),(748,209),(780,199)],
        'J7__J21':[(454,246),(421,239),(390,226),(356,220),(320,226),(283,238),(248,248)],
        'J13__J19':[(831,198),(855,201)],
        'J13__J12':[(838,212),(871,235),(898,263),(918,295),(939,322),(971,344),(1008,361),(1044,371)],
        'J12__J14':[(1109,387),(1142,403),(1176,421),(1209,434)],
        'J14__J11':[(1250,412),(1254,377),(1246,344),(1229,315),(1204,287),(1176,260),(1149,234),(1127,205)],
        'J11__J20':[(1148,182),(1184,192),(1224,198),(1263,210)],
        'J13__J11':[(838,212),(871,235),(907,235),(945,221),(986,204),(1027,192),(1067,179)],
        'J17__J12':[(803,426),(823,396),(846,365),(871,343),(902,335),(939,341),(971,344),(1008,361),(1044,371)],
        'J17__J19':[(803,426),(823,396),(846,365),(871,343),(886,315),(882,282),(865,250),(860,227)],
        'J2__J12':[(944,544),(943,513),(944,483),(951,451),(966,419),(991,397),(1029,382)],
        'J16__J3':[(600,565),(608,534),(605,478),(564,464),(526,444),(491,422),(465,405)]}}
    for cid,routes in corridors.items():
        c=city(cid)
        for e in c['edges']:
            if e['id'] not in routes:
                assert e['id'] in ['U0__U1','J0__J15'],e['id'];continue
            old=next((v['waypoints'] for v in next(v for v in before['cities'] if v['id']==cid)['edges'] if v['id']==e['id']),None)
            e['waypoints']=[point(*p) for p in routes[e['id']]];e['status']='manually_traced_map_A; runtime_walk_and_visual_review_required'
            reviews.append(dict(cityId=cid,edgeId=e['id'],old=old,new=e['waypoints'],travel=e['travel']))

    # Huế local quests and event state. Key items remain available for Korea/Seoul.
    quest('UQ01',completeAtNode='U19',requiredItems=items(lost_letter=1),requiredFlags=['hue.archive_decoded','hue.court_solved','hue.guard_disabled'],
          offer='Bức thư niêm phong ở U15 chứa cảnh báo bị cắt ghép. Giúp tôi đọc thư khố U17, xếp cờ U8 và đồng bộ trạm U19 nhé.',
          requiredFlagLabels=['Đọc nhịp tại thư khố U17','Xếp cờ sân điện U8','Giải quyết Thân Vệ U5'],reminder='Giữ kiện thư U15. Giải nhịp ngắn–dài–ngắn ở U17, áp dụng tại U8; giải quyết bot U5 rồi kích hoạt U19.',thanks='Hai bờ đã đồng bộ. Tuyến Đà Lạt đủ điều kiện mở; ấn Huế còn được dùng ở Gyeongju và Seoul.')
    quest('UQ02',requiredItems=items(royal_herb=1),consumeItems=True,rewardItems=items(healing_tea=3),flags=['hue.north_garden_open'],confirmDelivery=True,
          confirmationText='Cháu giao lá thuốc bờ bắc cho bà Nguyệt.',offer='Thuốc cho người chèo đò sắp hết. Tìm lá ở U10 giúp bà nhé.',reminder='Qua đò ở U3, men bờ bắc tới U10. Mang lá về U2; đừng dùng lá như vật phẩm hồi máu.',thanks='Bà đã sắc thuốc cho bến đò. Cháu cầm ba phần trà hồi phục; vườn phía tây cũng đã mở.')
    quest('UQ03',requiredItems=items(river_pass=1),offer='Giải hai ký hiệu trên vải ở U16 rồi mang thẻ về bến U3. Bác chỉ đưa cháu qua khi đã kiểm tra thẻ.',reminder='Tìm thẻ qua sông tại U16; quay lại nói chuyện và xác nhận tại U3.',thanks='Thẻ hợp lệ. Bác Tâm mở chuyến đò qua U4; cháu vẫn được quay lại bờ nam.',confirmDelivery=True,confirmationText='Cháu trình thẻ qua sông. Bác kiểm tra giúp cháu nhé.')
    quest('UQ04',completeAtNode='U7',requiredItems=items(ancient_seal=1),offer='Ấn cổ có nét khắc giống chòm sao. Mang đến Lê An ở U7 để đối chiếu; giữ lại ấn cho hành trình xa.',reminder='Giữ Ấn Tín Cổ và nói chuyện Lê An tại U7.',thanks='Lê An ghi nhận ký hiệu. Ấn được giữ lại để đối chiếu bệ đá Gyeongju và rơle Seoul.')
    for nid,qid,npc in [('U2','UQ02','npc_nguyet'),('U3','UQ03','npc_tam'),('U7','UQ01','npc_scholar')]:event('hue',nid).update(questId=qid,npcId=npc)
    event('hue','U11')['questId']='UQ04';additional('hue','U11','UQ04','offer');additional('hue','U7','UQ04','return')
    event('hue','U19').update(questId='UQ01',completedText='Ánh đèn Huế đã đồng bộ. Tuyến Đà Lạt được mở theo cốt truyện; bạn có thể trở lại bất kỳ bờ nào để làm nhiệm vụ phụ.')
    event('hue','U1')['text']+='\nBot chuyển thư ở U15 đang giữ kiện của nho sinh Lê An. Lá thuốc của bà Nguyệt ở U2 cũng sắp hết.'
    event('hue','U0')['text']='Ánh sáng Hội An đưa mình tới sông Hương. Bà Nguyệt U2 cần lá thuốc, còn bác Tâm U3 biết đường qua sông. Chị Hương U1 vẫn chờ món quà của Bà Tư.'
    puzzle('hue','U16','Sen|Đèn',['Sen','Đèn','Mây'],'Tấm vải có SEN hai cánh và ĐÈN hai vạch; MÂY có ba nét. Chọn lần lượt hai ký hiệu có đúng hai dấu: Sen rồi Đèn.',flags=['hue.market_code_known','hue.courier_code_known'],rewards=items(river_pass=1))
    puzzle('hue','U17','Ngắn|Dài|Ngắn',['Ngắn','Dài'],'Kiện thư U15 ghi nhịp: chớp ngắn, chớp dài, chớp ngắn. Gõ ba nhịp theo đúng thứ tự.',flags=['hue.archive_decoded','hue.secret_wall_found','hue.scholar_code_known'],required_items=items(lost_letter=1))
    puzzle('hue','U8','Vàng|Tím|Xanh',['Vàng','Tím','Xanh'],'Bản ghi U17: nhịp ngắn đầu là cờ Vàng, nhịp dài là cờ Tím, nhịp ngắn cuối là cờ Xanh. Chạm ba lá cờ theo nhịp ấy.',flags=['hue.court_solved'],required=['hue.archive_decoded'])
    event('hue','U15').update(alternateFlag='hue.courier_code_known',alternateText='Đối chiếu mã thư U16',alternateChoice='market_code',completionFlag='hue.courier_resolved')
    event('hue','U5').update(mandatory=True,alternateFlag='hue.scholar_code_known',alternateItemId='ancient_seal',alternateText='Ấn cổ + mã thư khố',alternateChoice='seal_code',completionFlag='hue.guard_disabled')
    # The seal is presented, not consumed: it is a cross-city key.
    event('hue','U5')['alternateKeepItem']=True
    event('hue','U13').update(rewardItems=items(healing_tea=1),text='Ghi chép ở bến kín nhắc rằng thư bị giữ bởi lệnh nhiễu, không phải do người đưa thư. Có một phần trà còn nguyên.',sourceStatus='canonical_secret_location; proposed_note_dialogue_and_tea_reward')
    event('hue','U14').update(type='dialogue',text='Từ đầu cầu nhỏ có thể nhìn thấy bến kín U13. Lumi đánh dấu lối xuống bến trước khi đi tiếp.',completedText='Lối xuống bến kín đã được ghi lại; có thể quay lại bất cứ lúc nào.')
    event('hue','U20')['destinationCity']='da_lat'

    # Jeonju main and side quests. Optional crane boss never blocks chili delivery.
    quest('JQ01',completeAtNode='J12',requiredItems=items(hanji_pattern_1=1,hanji_pattern_2=1,hanji_pattern_3=1),requiredFlags=['jeonju.three_patterns_joined'],
          onAcceptFlags=['quest:JQ01'],offer='Một tấm khuôn đã chia ba. Hãy gấp giấy ở J7, lấy mảnh trong kiện J6 và hộp kính J11, rồi ghép ở J12.',reminder='J7: gấp giấy; J6: giải quyết bot; trả kiện Park J5 để mở hộp J11. Ghép ba mảnh tại J12.',thanks='Ba mảnh hanji đã thành một mạch đèn. Mang kết quả tới đèn song thành J17.')
    quest('JQ02',requiredItems=items(missing_parcel=1),consumeItems=True,confirmDelivery=True,confirmationText='Cháu trả kiện hàng nguyên vẹn cho bác Park.',offer='Kiện hàng bên suối biến mất. Bot J6 giữ cả kiện lẫn mảnh hoa văn; sổ ghi mã Hoa – Đèn – Hoa. Hỏi tiến độ để ghi nhận mã.',reminder='Mã trong sổ: Hoa – Đèn – Hoa. Hỏi tiến độ tại J5 để ghi mã; lấy kiện J6 bằng mã hoặc chiến đấu, mang về J5 xác nhận trả.',thanks='Kiện đã về đúng chủ. Bác mở hộp kính trên đài J11 để Lumi lấy mảnh hoa văn thứ ba.')
    quest('JQ03',completeAtNode='J19',requiredItems=items(hanji_circuit=1),requiredFlags=['jeonju.lantern_puzzle_solved','jeonju.three_patterns_joined'],acceptFlags=['hoi_an.lamp_restored'],
          offer='Đèn Hội An đã phản chiếu sang Jeonju. Giải ba vòng ký hiệu ở J17, rồi lắp mạch hanji tại J19.',reminder='Ba mảnh đã ghép ở J12. Xoay ký hiệu J17 theo manh mối Hội An và kích hoạt trạm J19.',thanks='Sợi giấy nối ánh sáng hai thành phố. Tuyến Gyeongju đủ điều kiện mở; mạch hanji được giữ cho Seoul.')
    quest('JQ04',completeAtNode='H18',completeCityId='hoi_an',requiredItems=items(reply_letter=1),offer='Có thư hồi âm tại lối kín J21. Nhờ Lumi mang tới Ji-eun ở H18 Hội An, không cần đánh Hạc Giấy.',reminder='Tìm thư J21 sau khi nghe chuyện ở J16; trở về Ji-eun H18 và xác nhận giao.',thanks='Ji-eun đã đọc thư. Ngõ đèn H9 có thể được cứu; thư vẫn được giữ làm manh mối.',flags=['hoi_an.secret_lane_found'],confirmDelivery=True,confirmationText='Mình đưa thư hồi âm Jeonju cho Ji-eun đọc, rồi sẽ tới ngõ H9.')
    for nid,qid in [('J2','JQ01'),('J5','JQ02'),('J16','JQ04')]:event('jeonju',nid)['questId']=qid
    event('jeonju','J2')['npcId']='npc_hana';event('jeonju','J5')['npcId']='npc_park';event('jeonju','J16')['npcId']='npc_elder';event('jeonju','J4')['npcId']='npc_yoon'
    # A quest-giving NPC only grants its clue through the explicit talk choice.
    event('jeonju','J16').update(type='dialogue',questId='',text='Người giữ chuyện chỉ lối tới J21 và kể rằng một chiếc đèn ở Hội An đang che chở cho người lạc. Chọn Nói chuyện để ghi lại lối đi; nhận JQ04 khi muốn gửi thư.',completedText='Lối J21 đã được ghi lại. Hạc giấy ở J18 là thử thách tùy chọn; không cần thắng nó để gửi thư.')
    additional('jeonju','J16','JQ04','offer');additional('hoi_an','H18','JQ04','return')
    additional('jeonju','J12','JQ01','finish');additional('jeonju','J17','JQ03','offer')
    event('jeonju','J19').update(questId='JQ03',completedText='Đèn Hanji đã sáng. Jeonju gửi tín hiệu tới Gyeongju và Seoul. Bạn được ở lại hoàn thành việc gửi thư hoặc thử Hạc Giấy.')
    event('jeonju','J4').update(text='Yoon chỉ ba nét của khuôn: Hoa → Trăng → Đèn. Họa tiết đèn Hội An nằm ở tâm, không nằm trên mép. Đọc lại manh mối nếu ghép chưa đúng.',flags=['jeonju.yoon_clue_known'],completedText='Hoa → Trăng → Đèn. Nét Hội An đặt ở tâm; giữ ba mảnh để đối chiếu.')
    event('jeonju','J5')['onTalkFlags']=['jeonju.parcel_code_known']
    event('jeonju','J5')['text']='Park mở sổ: mã kho là Hoa – Đèn – Hoa. Bot J6 chỉ giữ kiện vì lệnh kiểm kê lỗi. Nhận nhiệm vụ rồi chọn Hỏi tiến độ để ghi mã vào nhật ký.'
    puzzle('jeonju','J3','Tím|Vàng|Xanh',['Tím','Vàng','Xanh'],'Mẫu vải có ba lớp từ ngoài vào: Tím của cánh hoa, Vàng của đèn, Xanh của lá. Chạm theo thứ tự đó.',flags=['jeonju.dye_restored'],rewards=items(healing_tea=1))
    puzzle('jeonju','J7','folded',[],'Tờ Hanji có hai vạch sáng. Xoay 90° hai lần để căn vạch, rồi gấp một lần. Thử gấp sớm sẽ chưa khớp; không mất giấy.',rewards=items(hanji_pattern_1=1),mode='fold')
    puzzle('jeonju','J12','Hoa|Trăng|Đèn',['Hoa','Trăng','Đèn'],'Yoon J4 ghi: Hoa → Trăng → Đèn. Đặt nét đèn Hội An tại tâm khuôn. Cần đủ ba mảnh thật trước khi ghép.',flags=['jeonju.three_patterns_joined'],rewards=items(hanji_circuit=1),required=['jeonju.yoon_clue_known','hoi_an.lamp_restored'],required_items=items(hanji_pattern_1=1,hanji_pattern_2=1,hanji_pattern_3=1))
    puzzle('jeonju','J17','Hoa|Trăng|Đèn',['Hoa','Trăng','Đèn'],'Ba vòng trên đèn mang Hoa, Trăng, Đèn. Khi Hội An sáng, nét tâm hiện ra: xoay từng vòng, từ trái sang phải thành Hoa → Trăng → Đèn như mẫu Yoon và khuôn J12.',flags=['jeonju.lantern_puzzle_solved'],required=['hoi_an.lamp_restored','jeonju.three_patterns_joined'],required_items=items(hanji_circuit=1),mode='rings')
    event('jeonju','J6').update(alternateFlag='jeonju.parcel_code_known',alternateText='Đọc mã kho của Park',alternateChoice='parcel_code',completionFlag='jeonju.parcel_bot_resolved')
    event('jeonju','J14').update(alternateFlag='jeonju.three_patterns_joined',alternateText='Đối chiếu khuôn đã ghép',alternateChoice='pattern',completionFlag='jeonju.pattern_bot_disabled')
    event('jeonju','J18').update(mandatory=False,alternateFlag='jeonju.lantern_puzzle_solved',alternateText='Gỡ bùa bằng đèn song thành',alternateChoice='paper_charm',completionFlag='jeonju.paper_crane_cleared')
    event('jeonju','J9').update(alternateFlag='jeonju.dye_restored',alternateText='Làm dịu bằng mẫu nhuộm',alternateChoice='painted_pattern',completionFlag='jeonju.paper_crane_pacified')
    event('jeonju','J10').update(rewardItems=items(healing_tea=1),rewardCoins=8,text='Dưới lớp sáp niêm phong còn nguyên là tám xu và một phần trà. Lumi ghi lại dấu trên nắp để hỏi thầy Yoon.',sourceStatus='canonical_chest_location; proposed_tea_and_8_coins_no_new_item_id')
    event('jeonju','J15')['shopItems']=[dict(itemId='healing_tea',amount=1,price=8)]
    event('jeonju','J0')['text']='Mình đã tới làng hanok. Người giữ chuyện J16 biết đường tới Hana J2. Hãy giúp ghép khuôn Hanji để tới chợ So-yeon J15; không cần đánh Hạc Giấy tùy chọn.'
    event('jeonju','J20')['destinationCity']='gyeongju'
    # Do not leave the return-letter dependencies pending after J21 is playable.
    next(n for n in city('jeonju')['nodes'] if n['id']=='J15')['revealWhenPreviouslyVisited']=True
    hq03=next(q for q in d['quests'] if q['id']=='HQ03');hq03.update(reminder='Nghe chuyện J16, lấy thư J21 Jeonju; mang tới Ji-eun H18 rồi ghé ngõ H9.',sourceStatus='canonical_cross_city_letter_now_playable',giverCityId='hoi_an',completeCityId='hoi_an')
    for cid,nid,aid in [('hue','U2','npc_nguyet'),('hue','U3','npc_tam'),('hue','U7','npc_scholar'),('jeonju','J2','npc_hana'),('jeonju','J4','npc_yoon'),('jeonju','J5','npc_park'),('jeonju','J16','npc_elder')]:actor(cid,nid,aid)
    for e in d['events']:
        if e['cityId'] in positions and e['type']=='encounter':actor(e['cityId'],e['nodeId'],e['enemyId'],(55,62))
    # Keep the accepted food-specific marker rules. Other nodes use the shared resolver.
    for cid,nid in [('hue','U4'),('hue','U6'),('jeonju','J8'),('jeonju','J11')]:
        id=cid+'_'+nid.lower()+'_checkin'
        d['checkIns']=[v for v in d['checkIns'] if v['id']!=id]
        n=next(v for v in city(cid)['nodes'] if v['id']==nid)
        d['checkIns'].append(dict(id=id,cityId=cid,nodeId=nid,title=n['name'],description='Ghi dấu trên bản đồ giả tưởng lấy cảm hứng từ di sản '+('Huế: cầu, cổng thành và sông Hương.' if cid=='hue' else 'Jeonju: cầu đá và mái ngói hanok.')+' Minh họa lấy đúng vùng của Overview A, không phải ảnh địa lý ngoài đời.',completionFlag='checkin:'+id,sourceStatus='canonical_landmark; proposed_optional_checkin_3_coins',requiredFlags=[],rewardCoins=3,rewardItems=[]))
        a['nodeMarkers']=[v for v in a['nodeMarkers'] if (v['cityId'],v['nodeId'])!=(cid,nid)]
        rules=[dict(optionId='collect',optionState='enabled',marker='Reward',effect='Glow')] if nid=='J11' else []
        a['nodeMarkers'].append(dict(cityId=cid,nodeId=nid,marker='Camera',effect='None',size=24,rules=rules))
    assert sum(len(c['nodes']) for c in d['cities'])==133
    assert len(d['items'])==37 and len(d['enemies'])==23
    for old in before['cities']:
        assert [n['id'] for n in old['nodes']]==[n['id'] for n in city(old['id'])['nodes']]
        if old['id'] not in positions:assert old==city(old['id'])
    write(path,d);write(presentation_path,a)
    write(ROOT/'Docs/Maps/HueJeonjuChapterRoutes.json',dict(changes=reviews,terrainCertification='NOT VERIFIED until final runtime review; routesReviewed remains false',
         proposals=['Two Hue garden corridors U4-U12/U11-U7 give access to scholar/seal before the inner bot gate; stage3 remains inaccessible without guard_disabled.',
                    'J13 is available before assembling J12, so J11 third piece does not depend on its own assembly gate.',
                    'J16 is in the starting zone; J15 is stage2 for new saves, after the three-pattern gate; existing food deliveries remain completed.',
                    'Exact puzzle symbols, connecting dialogue, optional check-ins and unspecified chest contents are proposals, not verbatim design.']))
    print('Authored Huế/Jeonju chapters: all 44 IDs retained; 8 local quests, 7 interactive puzzles; other cities unchanged.')
if __name__=='__main__':main()
