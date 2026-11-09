"""Targeted Dalat authoring; preserve other five cities and existing IDs/art.

Concrete answers/dialogue, land anchors and rewards unspecified by canonical
design are proposals recorded in Docs/Chapters/DaLatDesignReview.md.
"""
from pathlib import Path
import copy,json
ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT.parent/'Backups/BeforeDaLatChapter-20261017'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def point(x,y):return dict(x=round(x/1448,9),y=round(1-y/1086,9))
def items(**kw):return [dict(itemId=k,amount=v) for k,v in kw.items()]
def main():
 p=ROOT/'Assets/_Game/Resources/Content/GameplayContent.json';d=read(p);before=read(BASE/p.relative_to(ROOT));c=next(c for c in d['cities'] if c['id']=='da_lat');old=copy.deepcopy(c)
 a_path=ROOT/'Assets/_Game/Resources/Content/AdventurePresentation.json';a=read(a_path)
 def e(n):return next(e for e in d['events'] if e['cityId']=='da_lat' and e['nodeId']=='D'+str(n) and not e.get('additive'))
 def q(id,**kw):
  v=next(q for q in d['quests'] if q['id']==id);v.update(kw,pending=False,giverCityId='da_lat',completeCityId='da_lat',sourceStatus='canonical_LBC_v1; concrete_dialogue_and_interaction_proposal');return v
 def extra(id,n,typ,**kw):
  d['events'][:]=[v for v in d['events'] if not(v['cityId']=='da_lat' and v['id']==id)]
  v=dict(id=id,cityId='da_lat',nodeId='D'+str(n),type=typ,additive=True,actor=e(n)['actor'],npcId=e(n).get('npcId',''),text=e(n)['text'],sourceStatus='canonical_mechanic; authored_interaction_proposal');v.update(kw);d['events'].append(v)
 def puzzle(n,answer,options,clue,flags,rewards=(),req=()):
  pid=e(n)['puzzleId'];v=next(v for v in d['puzzles'] if v['id']==pid)
  v.update(mode='sequence',answer=answer,options=options,clue=clue,flags=list(flags),rewardItems=list(rewards),requiredFlags=list(req),sourceStatus='canonical_mechanic; proposed_symbols_and_hint')
 c.update(runtimeReady=True,runtimeNodeIds=[],routesReviewed=False,mapTitle='ĐÀ LẠT · LỐI QUA SƯƠNG')
 stages=[[0,2,3,5,6,7,18],[1,4,8],[9,10,11,12,13,19,20],[14,15,16],[17,21]]
 # Original flat-image anchors are approximate. These pixel anchors use visible
 # path/steps/dock, not water/roofs. Full before/after and review limits retained.
 positions={0:(780,698),1:(916,270),2:(742,495),3:(640,501),4:(985,265),5:(402,662),6:(300,477),7:(527,342),8:(345,401),9:(232,245),10:(622,806),11:(190,802),12:(165,755),13:(1182,505),14:(275,147),15:(964,800),16:(1060,923),17:(1365,902),18:(500,610),19:(290,174),20:(917,474),21:(1204,764)}
 for n in c['nodes']:
  i=int(n['id'][1:]);n['position']=point(*positions[i]);n['revealStage']=next(s for s,ids in enumerate(stages) if i in ids);n['previewStages']=[]
  if i==2:n['gateCondition']='da_lat.glasses_returned'
  if i==16:n['requiredFlags']=['da_lat.mist_boss_cleared']
  if i==17:n['requiredFlags']=['da_lat.dog_fed']
 for i,preview in [(8,[0]),(10,[1]),(15,[2]),(21,[3])]:c['nodes'][i]['previewStages']=preview
 c['gates'][0]['condition']='da_lat.glasses_returned'
 # Pixel corridors follow the actual lower-west steps, upper shore, two stone
 # bridges and waterfall boardwalk; no copied geometry from another city.
 west=[(741,716),(705,748),(666,773),(622,806),(568,793),(533,769),(491,766),(446,773),(407,797),(387,821),(361,830),(308,817),(243,797),(190,802)]
 pine=[(402,662),(414,634),(429,611),(430,589),(443,568),(463,555)]
 upper=[(724,464),(688,466),(649,484),(640,501)]
 streets=[(724,464),(707,444),(706,420),(713,394),(729,369),(754,357),(777,344),(791,318),(804,296),(817,281),(839,271),(878,263),(916,270)]
 east=[(780,698),(741,716),(705,748),(666,773),(622,806),(686,798),(737,809),(788,816),(847,817),(904,810),(964,800),(1024,793),(1062,781),(1100,771),(1140,760),(1167,741),(1174,705),(1186,672),(1200,645),(1209,600),(1201,567),(1188,537),(1182,505),(1118,486),(1086,476),(1053,470),(1007,470),(958,471),(917,474)]
 to_observatory=[(985,265),(952,264),(916,270),(878,263),(839,271),(817,281),(804,296),(791,318),(777,344),(754,357),(729,369),(713,394),(706,420),(688,435),(640,438),(590,442),(547,436),(515,421),(490,400),(458,388),(420,382),(396,371),(375,353),(363,328),(360,294),(347,264),(321,239),(300,215),(285,188),(275,147)]
 paths={
 'D0__D12':west+[(165,755)],
 'D0__D20':[(780,698),(748,695),(719,686),(688,677),(657,661),(635,638),(612,615),(586,600),(562,591),(538,578),(520,553),(537,520),(580,514),(611,505),(640,501),(649,484),(688,466),(724,464),(741,452),(781,449),(823,456),(861,462),(888,469),(917,474)],
 'D0__D5':[(780,698),(748,695),(719,686),(688,677),(657,661),(635,638),(612,615),(586,600),(562,591),(538,578),(520,553),(493,538),(463,555),(443,568),(430,589),(429,611),(414,634),(402,662)],
 'D5__D18':pine,
 'D18__D3':[(463,555),(493,538),(537,520),(580,514),(611,505),(640,501)],
 'D3__D2':list(reversed(upper)),
 'D2__D1':streets,
 'D1__D4':[(916,270),(940,274),(965,274),(985,265)],
 'D1__D20':list(reversed(streets))+[(741,452),(781,449),(823,456),(861,462),(888,469),(917,474)],
 'D5__D6':[(402,662),(421,636),(423,606),(414,577),(387,548),(357,522),(325,498),(300,477)],
 'D6__D7':[(300,477),(333,486),(367,500),(398,504),(426,493),(450,471),(477,448),(501,428),(518,402),(526,374),(527,342)],
 'D7__D8':[(527,342),(517,360),(499,369),(469,368),(436,373),(414,386),(388,398),(365,403),(345,401)],
 'D8__D9':[(345,401),(319,394),(292,383),(264,373),(239,365),(217,358),(202,342),(196,323),(197,304),(211,288),(225,268),(232,245)],
 'D8__D19':[(345,401),(365,379),(375,353),(363,328),(360,294),(347,264),(321,239),(300,215),(290,174)],
 'D5__D11':[(402,662),(381,681),(365,710),(345,735),(308,755),(267,769),(226,782),(190,802)],
 'D11__D10':list(reversed(west[4:]))+[(622,806)],
 'D12__D10':[(165,755)]+list(reversed(west[4:])),
 'D10__D15':east[5:11],
 'D15__D16':[(964,800),(1015,799),(1038,817),(1049,844),(1054,876),(1060,923)],
 'D16__D17':[(1060,923),(1049,890),(1038,859),(1034,831),(1038,806),(1055,783),(1089,771),(1127,766),(1163,764),(1204,764),(1231,781),(1254,807),(1272,832),(1303,857),(1333,876),(1365,902)],
 'D20__D13':[(917,474),(958,471),(1007,470),(1053,470),(1086,476),(1118,486),(1182,505)],
 'D13__D14':[(1182,505),(1188,467),(1171,423),(1169,387),(1183,354),(1215,329),(1240,307),(1241,286),(1218,267),(1180,254),(1137,252),(1097,254),(1055,259),(1015,265)]+to_observatory,
 'D13__D21':[(1182,505),(1188,537),(1201,567),(1209,600),(1200,645),(1186,672),(1174,705),(1167,741),(1140,760),(1204,764)],
 'D4__D14':to_observatory,
 'D20__D15':list(reversed(east[10:])),
 'D21__D17':[(1204,764),(1231,781),(1254,807),(1272,832),(1303,857),(1333,876),(1365,902)]}
 # Runtime coordinate-grid review showed the initial dock exit clipped a temple
 # roof and the inn anchor sat on a blue roof. Follow its front street/steps.
 paths['D0__D5']=[(780,698),(758,705),(735,718),(714,735),(692,745),(667,745),(643,734),(622,716),(610,697),(590,677),(575,658),(560,645),(538,635),(515,625),(492,615),(475,606),(455,613),(445,635),(429,655),(402,662)]
 paths['D5__D18']=[(402,662),(429,655),(445,635),(455,613),(475,606),(500,610)]
 paths['D18__D3']=[(500,610),(488,595),(475,578),(457,563),(434,550),(411,537),(399,523),(413,502),(440,486),(469,467),(497,452),(528,447),(557,458),(586,477),(613,494),(640,501)]
 paths['D3__D2']=[(640,501),(665,510),(692,516),(716,513),(742,495)]
 paths['D0__D20']=paths['D0__D5']+paths['D5__D18'][1:]+paths['D18__D3'][1:]+paths['D3__D2'][1:]+[(781,483),(823,473),(861,469),(888,469),(917,474)]
 paths['D5__D6']=[(402,662),(429,655),(445,635),(455,613),(475,606),(488,595),(475,578),(457,563),(434,550),(411,537),(399,523),(371,508),(344,492),(300,477)]
 paths['D6__D7']=[(300,477),(344,492),(371,508),(399,523),(413,502),(440,486),(469,467),(497,452),(519,433),(528,408),(526,383),(527,342)]
 paths['D8__D19']=paths['D8__D9']+[(245,228),(260,207),(278,190),(290,174)]
 for key in ['D4__D14','D13__D14']:
  arr=paths[key];cut=arr.index((396,371));paths[key]=arr[:cut+1]+[(397,343),(394,312),(387,280),(369,254),(345,231),(321,212),(300,190),(285,168),(275,147)]
 for edge in c['edges']:
  pts=paths[edge['id']];start=positions[int(edge['from'][1:])];end=positions[int(edge['to'][1:])]
  if pts[0]!=start:pts=[start]+pts
  if pts[-1]!=end:pts=pts+[end]
  # Use safe polylines without corner smoothing that can cut into water.
  edge['waypoints']=[point(*p) for p in pts[1:-1]];edge['status']='authored_MapA_path_corridor; runtime_and_visual_review_required'
 for v in d['events']:
  if v['cityId']=='da_lat':v['flags']=[];v['sourceStatus']='canonical_LBC_v1; authored_interaction_pending_runtime_verification'
 e(0).update(actor='Lumi',text='Huế đã gửi nhịp sáng tới cao nguyên. Mình sẽ tìm Lan bên phố hồ; những cột chỉ đường đang chìm trong sương.')
 e(2).update(questId='DQ01',text='Chị làm rơi kính gần trạm quan sát. Bot Giấu Kính đang giữ chúng. Em có thể giao chiến hoặc bật bẫy phản quang hai lần. Mang kính về đây rồi chị sẽ chỉ đường D8.',completedText='Cảm ơn Lumi! Ba cổng trong sương đi theo bản phác của chị: Thông → Đèn → Hồ.')
 q('DQ01',completeAtNode='D8',requiredFlags=['da_lat.glasses_returned','da_lat.fog_path_solved'],requiredFlagLabels=['Giao kính lại cho Lan ở D2','Theo bản phác và giải ba cổng tại D8'],completionFlag='da_lat.fog_cleared',offer=e(2)['text'],reminder='Lấy kính D7, giao lại Lan D2. Theo phác thảo Thông → Đèn → Hồ để giải D8.',thanks='Lan đã nhìn rõ biển chỉ đường, lối sương được mở.',consumeItems=False)
 extra('dalat_return_glasses',2,'item',title='Giao kính lại cho Lan',requiredFlags=['quest:DQ01'],requiredItems=items(lan_glasses=1),consumeRequiredItems=True,flags=['da_lat.glasses_returned'],text='Lan nhận lại kính. Phác thảo của chị ghi: Thông → Đèn → Hồ. Hãy trở lại trụ đèn D8.')
 extra('dalat_dq01_finish',8,'quest',questId='DQ01')
 e(3).update(text='Chị Thủy có một phần thức ăn dự phòng cho chú chó. Nếu tìm được hoa đêm, cho chị xem nhé; em vẫn giữ hoa để giao người làm vườn. Mỗi chuyến chỉ có một phần dự phòng.',completedText='Chị đã gửi phần thức ăn dự phòng. Ba nguồn chính vẫn là rương D9, vườn D11 và cối xay D12.')
 extra('dalat_thuy_food',3,'item',title='Cho Thủy xem hoa đêm · nhận một phần thức ăn',requiredItems=items(night_flower=1),rewardItems=items(meat_portion=1),flags=['da_lat.thuy_food_given'],text='Thủy ngắm bông hoa rồi gửi một phần thức ăn; hoa vẫn dành cho người làm vườn D13.')
 extra('dalat_dog_food_recovery',3,'recovery',questId='DQ02',itemId='meat_portion',itemCount=3,refillIfMissing=True,requiredFlags=['da_lat.mountain_supplies_found','da_lat.west_path_open'],anyFlags=['puzzle:puzzle_windmill','da_lat.thuy_food_given','da_lat.night_flower_done'],completionFlag='da_lat.dog_food_recovered',title='Bù phần thức ăn còn thiếu cho chú chó',text='Em đã kiểm tra các nguồn quanh hồ. Nếu thức ăn đã dùng trên đường, chị bù đúng phần còn thiếu, tối đa đủ ba phần đang giữ; chỉ dành cho chú chó D10, không phát xu.')
 e(4).update(npcId='npc_market_merchant',actor='Thương nhân đường xa',text='Lại gặp Lumi trên cao nguyên! Ba phần thức ăn nằm ở D9, D11 và D12. Thú canh đường chỉ đói, đừng đánh nó. Cối xay theo hướng gió: Đông → Bắc → Tây.',shopItems=[dict(itemId='healing_tea',amount=1,price=8)])
 e(6).update(alternateFlag='da_lat.glasses_returned',alternateText='Soi đèn theo bản phác của Lan để tránh sói',alternateChoice='lamp_stealth',completionFlag='da_lat.wolf_cleared')
 e(7).update(alternateSteps=2,alternateText='Bật tấm phản quang của bẫy sáng',alternateChoice='light_trap',text='Bot lẩn giữa hai tấm phản quang. Bật từng tấm để chặn đường chạy, hoặc giao chiến. Kính chỉ được trả khi bẫy đủ hai bước hoặc Lumi thắng trận.')
 puzzle(8,'Thông|Đèn|Hồ',['Thông','Đèn','Hồ'],'Lan đã đánh dấu ba cổng: Thông → Đèn → Hồ. Chạm các biểu tượng theo thứ tự rồi xác nhận. Sai có thể xóa và thử lại.',['da_lat.fog_path_solved','da_lat.fog_cleared'],req=['da_lat.glasses_returned'])
 e(9).update(flags=['da_lat.mountain_supplies_found'])
 e(10).update(questId='DQ02',text='Chú chó giữ đường xuống cầu đá vì bụng đói. Ba phần thức ăn sẽ giúp nó yên tâm tránh lối.',completedText='Chú chó đã no, vẫy đuôi chỉ đường xuống thác. Lumi có thể quay lại bất cứ lúc nào.')
 q('DQ02',requiredItems=items(meat_portion=3),consumeItems=True,confirmDelivery=True,confirmationText='Lumi đưa đúng ba phần thức ăn cho chú chó, mở lối xuống cầu đá.',offer=e(10)['text'],reminder='Tìm thức ăn ở D9, D11 và D12; có một phần dự phòng tại Thủy D3.',thanks=e(10)['completedText'])
 e(11).update(flags=['da_lat.west_path_open'])
 puzzle(12,'Đông|Bắc|Tây',['Đông','Bắc','Tây'],'Ba ngọn thông nghiêng theo gió Đông → Bắc → Tây. Đặt ba cánh theo hướng đó; cối xay mở hộc thức ăn một lần.',['da_lat.windmill_supplies_found'],items(meat_portion=1))
 next(v for v in d['puzzles'] if v['id']==e(12)['puzzleId'])['mode']='rings'
 e(13).update(questId='DQ04',text='Có hoa chỉ nở khi trụ D8 xua sương. Mang một bông ở D19 về đây, tôi sẽ pha trà hỗ trợ chuyến đi.',completedText='Bông hoa đã về vườn kính. Cảm ơn Lumi đã giữ đường rừng an toàn.')
 q('DQ04',requiredItems=items(night_flower=1),consumeItems=True,rewardItems=items(healing_tea=2),confirmDelivery=True,confirmationText='Giao bông hoa đêm cho người làm vườn?',offer=e(13)['text'],reminder='Lấy hoa đêm tại D19 sau khi giải D8; có thể cho Thủy xem trước khi giao.',thanks=e(13)['completedText'])
 e(14).update(questId='DQ03',actor='Lumi',requiredFlags=['da_lat.fog_cleared','da_lat.dog_fed','da_lat.mist_boss_cleared'],completedText='Mình đã hiểu: những bóng sương là tín hiệu cảnh báo cũ đang tự nhân bản, không phải sinh vật muốn hại mọi người. Giúp Lan và chú chó đã làm chúng dịu lại. Nguồn sương và tín hiệu nước nay đồng bộ, gửi nhịp rơle A tới Seoul. Mình vẫn cần kết quả của cả hai nhánh Việt–Hàn trước khi tới đó.')
 e(15).update(mandatory=True,completionFlag='da_lat.mist_boss_cleared',text='Ta giữ cầu để người đi không rơi xuống thác. Giúp Lan và chú chó làm tín hiệu dịu lại, rồi hãy thử vượt qua.')
 en=next(v for v in d['enemies'] if v['id']=='enemy_fog_boss');en.update(rewardItems=[],weaknessFlags=['da_lat.glasses_returned','da_lat.dog_fed'],weaknessHpReduction=10,weaknessAttackReduction=3)
 e(16).update(flags=['da_lat.water_signal_found'])
 q('DQ03',completeAtNode='D14',requiredItems=items(blue_water_signal=1),requiredFlags=['da_lat.fog_cleared','da_lat.dog_fed','da_lat.mist_boss_cleared'],requiredFlagLabels=['Xua sương tại D8','Cho chó D10 ăn','Vượt Bóng Sương D15'],consumeItems=False,offer='Mang tín hiệu thác nước tới đài đèn D14. Giữ mã nước xanh để đồng bộ rơle A tại Seoul.',reminder='Thu mã nước xanh D16 sau Bóng Sương; kích hoạt đài D14.',thanks=e(14)['completedText'])
 extra('dalat_dq03_offer',16,'quest',questId='DQ03')
 e(17).update(rewardItems=items(healing_tea=1),rewardCoins=5,text='Lối nhìn ra thác mà chú chó chỉ cho Lumi. Một túi trà và ít xu được để trong hòm khô bên lan can.')
 e(18).update(type='start',actor='Lumi',text='Mình có thể nghỉ ở nhà trên dốc để hồi sức trước khi tiếp tục. Kính ở D7, Lan ở D2; xuống thác cần giúp chú chó D10.')
 e(19).update(requiredFlags=['da_lat.fog_cleared'],text='Hoa đêm đã lộ dưới sương mỏng. Chỉ hái một bông theo lời người làm vườn.')
 e(20).update(actor='Lumi',text='Cây cầu đá nối hai bờ hồ. Mình sẽ đi trên mặt cầu, rồi theo đường ven bờ tới vườn kính.')
 e(21).update(destinationCity='seoul',requiredFlags=['da_lat.beacon_lit'],actor='Lumi',text='Đà Lạt đã gửi tín hiệu nước. Seoul chỉ nhận khách khi Huế, Đà Lạt, Jeonju và Gyeongju đều đã đồng bộ. Mình vẫn có thể quay về các thành phố đã mở.')
 # Optional latest-user specialty requirement; separate explicit proposal, no
 # canonical quest/reward changes and no extra prerequisite on the main chapter.
 d['items']=[x for x in d['items'] if x['id']!='da_lat_strawberry']
 d['items'].append(dict(id='da_lat_strawberry',name='Dâu tây Đà Lạt',type='quest',stackLimit=1,healHP=0,description='Quà cao nguyên gửi So-yeon ở J15. Không dùng như thuốc hồi máu.',category='cultural_gift',iconId='strawberry',stackable=False,consumable=False,questItem=True,sourceCity='da_lat',value=0,effects=[],relatedQuestIds=['DQ05'],source='LATEST_USER_SPECIALTY_REQUIREMENT; proposed_optional_exchange; existing_FoodCulture_sheet'))
 d['quests']=[x for x in d['quests'] if x['id']!='DQ05']
 d['quests'].append(dict(id='DQ05',cityId='da_lat',title='Dâu cao nguyên gửi Jeonju',giverNode='D4',giverCityId='da_lat',completeAtNode='J15',completeCityId='jeonju',completionFlag='jeonju.dalat_strawberry_shared',objectives=['receive:da_lat_strawberry@D4','deliver:da_lat_strawberry@J15'],acceptFlags=['da_lat.beacon_lit'],requiredFlags=[],requiredItems=items(da_lat_strawberry=1),acceptRewardItems=items(da_lat_strawberry=1),rewardItems=[],rewardCoins=10,consumeItems=True,confirmDelivery=True,confirmationText='Giao một phần dâu Đà Lạt cho So-yeon làm quà?',offer='Mang một phần dâu cao nguyên tới So-yeon tại J15. Chuyến quà tùy chọn này không chặn cốt truyện.',reminder='Giữ dâu trong túi; giao trực tiếp tại quầy J15 ở Jeonju.',thanks='So-yeon nhận quà dâu cao nguyên và gửi lời cảm ơn về Đà Lạt.',sourceStatus='PROPOSED_OPTIONAL_SPECIALTY_QUEST_NOT_IN_CANONICAL_24',pending=False))
 extra('dalat_strawberry_offer',4,'quest',questId='DQ05')
 d['events']=[x for x in d['events'] if not(x['cityId']=='jeonju' and x['id']=='dalat_strawberry_return')]
 d['events'].append(dict(id='dalat_strawberry_return',cityId='jeonju',nodeId='J15',type='quest',additive=True,questId='DQ05',requiredFlags=['quest:DQ05'],npcId='npc_soyeon',actor='So-yeon',text='Dâu từ cao nguyên Việt Nam mang một hương vị khác với món quà ớt lần trước. Lumi có muốn gửi quà không?',sourceStatus='PROPOSED_OPTIONAL_SPECIALTY_QUEST_NOT_IN_CANONICAL_24'))
 # Shared presentation: actor scale/touch targets/marker effects remain inherited.
 aids={2:'npc_lan',3:'npc_thuy',4:'npc_market_merchant',6:'enemy_mist_wolf',7:'enemy_glass_thief',10:'npc_hungry_dog',13:'npc_gardener',15:'enemy_fog_boss'}
 a['mapActors']=[x for x in a['mapActors'] if x['cityId']!='da_lat'];a['nodeMarkers']=[x for x in a['nodeMarkers'] if x['cityId']!='da_lat']
 for n,aid in aids.items():
  a['mapActors'].append(dict(cityId='da_lat',nodeId='D'+str(n),artId=aid,size=dict(x=52,y=60) if n not in [6,10,15] else dict(x=58,y=48),offset=dict(x=-28 if n==6 else 22,y=8),hideWhenResolved=aid.startswith('enemy')))
 for n in c['nodes']:
  typ=n['kind'];rules=[]
  if typ in ['enemy','boss']:rules=[dict(eventState='resolved',marker='Completed',effect='None')]
  if n['id'] in ['D2','D10','D13']:
   qid={'D2':'DQ01','D10':'DQ02','D13':'DQ04'}[n['id']];rules=[dict(questId=qid,questState='completed',marker='Completed'),dict(optionId='complete',optionState='enabled',marker='NpcTalk',effect='Bounce'),dict(questId=qid,questState='active',marker='NpcNeedItem',effect='Glow')]
  if n['id']=='D2':rules.insert(0,dict(optionId='collect:dalat_return_glasses',optionState='enabled',marker='NpcTalk',effect='Bounce'))
  a['nodeMarkers'].append(dict(cityId='da_lat',nodeId=n['id'],rules=rules))
 for n,title,desc in [(0,'Bến hồ Xuân Hương','Bến hồ trong bản đồ Đà Lạt cách điệu; tranh minh họa, không phải ảnh chụp địa lý.'),(14,'Đài dẫn sáng núi','Đài quan sát trên đồi thông trong bản đồ giả tưởng, không gán tên một di tích có thật.')]:
  cid='dalat_checkin_'+str(n);d['checkIns']=[v for v in d['checkIns'] if v['id']!=cid];d['checkIns'].append(dict(id=cid,cityId='da_lat',nodeId='D'+str(n),title=title,description=desc,completionFlag='da_lat.checkin_'+str(n),requiredFlags=['da_lat.beacon_lit'] if n==14 else [],rewardCoins=3,rewardItems=[],sourceStatus='proposed_checkin_MapA_ROI_not_documentary_photo'))
 # Ensure idempotent authoring cannot reset content of a previously tested city.
 for city in d['cities']:
  if city['id']!='da_lat':assert city==next(v for v in before['cities'] if v['id']==city['id'])
 write(p,d);write(a_path,a)
 write(ROOT/'Docs/Maps/DaLatChapterRoutes.json',dict(status='NEEDS_RUNTIME_AND_VISUAL_REVIEW',background='Maps/DaLat/Map_Overview_A.png',nodes=[dict(id=n['id'],old=old['nodes'][i]['position'],new=n['position'],evidence='visible path/steps/dock on full resolution Overview A; approximate source anchor was on roof/water or not landmark') for i,n in enumerate(c['nodes'])],edges=[dict(id=x['id'],old=next(v for v in old['edges'] if v['id']==x['id'])['waypoints'],new=x['waypoints']) for x in c['edges']]))
 print('Authored Dalat only:',len(c['nodes']),'nodes',len(c['edges']),'edges; other cities unchanged')
if __name__=='__main__':main()
