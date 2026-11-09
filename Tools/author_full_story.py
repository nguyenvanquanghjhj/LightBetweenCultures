"""Extend the existing catalog; preserve released chapters and original art/IDs.

Concrete symbols, extra chest supplies and balance adjustments are playtest
proposals. Source records and every intentional dependency resolution are logged.
"""
from pathlib import Path
import json,copy,hashlib,shutil,sys
R=Path(__file__).resolve().parents[1]
B=R.parent/'Backups/CheckpointPortraitDialogueStoryAudit-20261017'
OUT=R/'Docs/Story/FullStory-20261017'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def items(**kw):return [dict(itemId=k,amount=v) for k,v in kw.items()]
def point(x,y):return dict(x=round(x/1448,9),y=round(1-y/1086,9))
def main():
 p=R/'Assets/_Game/Resources/Content/GameplayContent.json';d=read(p);before=read(B/p.relative_to(R));a=read(R/'Assets/_Game/Resources/Content/AdventurePresentation.json')
 spec=R/'Docs/Story/FULL_STORY_PLAYTEST.md';assert spec.is_file()
 assert spec.read_bytes()==(B/'Docs/Story/LBC_FullStory_Playtest_6Cities_133Nodes.md').read_bytes(),'Review changed spec before running authoring'
 def city(cid):return next(c for c in d['cities'] if c['id']==cid)
 def event(cid,n):return next(e for e in d['events'] if e['cityId']==cid and e['nodeId']==n and not e.get('additive'))
 def quest(qid,**kw):
  q=next(q for q in d['quests'] if q['id']==qid);q.update(kw,pending=False,sourceStatus='FULL_STORY_PLAYTEST; canonical_objectives; PLAYTEST_NEW_dialogue');q.setdefault('giverCityId',q['cityId']);q.setdefault('completeCityId',q['cityId']);return q
 def extra(cid,n,key,typ='quest',**kw):
  d['events'][:]=[e for e in d['events'] if not (e['cityId']==cid and e['id']==key)]
  base=event(cid,n);v=dict(id=key,cityId=cid,nodeId=n,type=typ,additive=True,actor=base.get('actor','Lumi'),npcId=base.get('npcId',''),text=base['text'],sourceStatus='FULL_STORY_PLAYTEST; typed_runtime_interaction');v.update(kw);d['events'].append(v)
 def puzzle(cid,n,answer,opts,clue,flags=(),req=(),needs=None,rewards=None,mode='sequence'):
  e=event(cid,n);v=next(v for v in d['puzzles'] if v['id']==e['puzzleId']);v.update(mode=mode,answer=answer,options=opts,clue=clue,flags=list(flags),requiredFlags=list(req),requiredItems=needs or [],rewardItems=rewards or [],sourceStatus='FULL_STORY_PLAYTEST; PLAYTEST_NEW_concrete_symbols');e.update(requiredFlags=list(req),requiredItems=needs or [])
 def setup(cid,stages,gates):
  c=city(cid);c.update(runtimeReady=True,runtimeNodeIds=[],routesReviewed=False,mapTitle='GYEONGJU · NHỊP SAO SILLA' if cid=='gyeongju' else 'SEOUL · NHỮNG NHỊP CẦU ÁNH SÁNG')
  for n in c['nodes']:
   i=int(n['id'][1:]);n.update(revealStage=next(s for s,ids in enumerate(stages) if i in ids),previewStages=[],isKeyGate=i in gates,gateCondition=gates[i][0] if i in gates else '',unlockStage=gates[i][1] if i in gates else 0,requiredFlags=[])
  c['gates']=[dict(nodeId=('G' if cid=='gyeongju' else 'S')+str(i),condition=v[0],unlockStage=v[1]) for i,v in gates.items()]
  for e in d['events']:
   if e['cityId']==cid:e.update(flags=[],oneShot=True,sourceStatus='FULL_STORY_PLAYTEST; canonical_event_id; PLAYTEST_NEW_dialogue')
  return c
 g=setup('gyeongju',[[0,1,2,3,11,16],[4,5,6,9,10,15,17],[7,12,21],[8,13,14,18,19],[20]],{3:('quest:GQ01',1),4:('gyeongju.three_stars_placed',2),7:('gyeongju.guard_resolved',3),19:('gyeongju.beacon_lit',4)})
 E=lambda n:event('gyeongju','G'+str(n))
 # The source puts the third symbol behind G19. Minimal graph correction opens
 # its existing upper-temple branch before the observatory, without moving IDs.
 g['edges']=[e for e in g['edges'] if e['id'] not in ['G2__G11','G11__G12','G4__G19']]
 for left,right in [('G4','G9'),('G18','G19')]:
  if not any(e['id']==left+'__'+right for e in g['edges']):g['edges'].append(dict(id=left+'__'+right,**{'from':left,'to':right},bidirectional=True,waypoints=[],requiredFlags=[],travel='walk',status='PLAYTEST_RESOLUTION_dependency_route'))
 E(0).update(actor='Lumi',text='Jeonju đã gửi một nhịp sáng tới Silla. Min đang chờ ở G3. Những tượng đá chuyển động có lẽ đang bảo vệ di tích khỏi một lệnh sai.')
 E(3).update(questId='GQ01',text='Ba nét sao ở gò tây G5, đường gò G6 và đình đồi G10. Hãy đặt chúng lên đài G4. Tượng đá không tự nhiên trở nên ác; đừng vội phá nó.',onTalkFlags=['gyeongju.secret_herb_found'])
 quest('GQ01',completeAtNode='G4',requiredFlags=['gyeongju.three_stars_placed'],offer=E(3)['text'],reminder='Tìm ba biểu tượng G5/G6/G10; giải đài G4 rồi xác nhận hoàn thành tại đài.',thanks='Ba biểu tượng đã đúng vị trí. Giữ ống kính sao cho hồ sơ ở Seoul.')
 extra('gyeongju','G4','gq01_finish',questId='GQ01')
 puzzle('gyeongju','G2','Sao|Trăng|Sao',['Sao','Trăng'],'Ao phản chiếu: Sao → Trăng → Sao. Chạm theo thứ tự để ghi manh mối, không phát vật phẩm.',flags=['gyeongju.pond_hint'])
 puzzle('gyeongju','G4','Sao|Trăng|Sao',['Sao','Trăng'],'Ba nét trên phiến đá đọc từ trái sang phải: Sao → Trăng → Sao. Cần đủ ba biểu tượng; giữ chúng làm vật chứng.',flags=['gyeongju.three_stars_placed'],req=['quest:GQ01'],needs=items(star_symbol_1=1,star_symbol_2=1,star_symbol_3=1),rewards=items(stargazer_lens=1))
 E(6).update(alternateItemId='meat_portion',alternateFlag='',alternateChoice='bait',alternateText='Dùng một phần thức ăn để cáo trả biểu tượng',completionFlag='gyeongju.fox_resolved')
 extra('gyeongju','G3','min_fox_food','item',title='Nhận phần mồi cho cáo',rewardItems=items(meat_portion=1),requiredFlags=['quest:GQ01'],text='Min gửi đúng một phần mồi. Có thể dùng thay cho giao chiến ở G6.')
 E(7).update(mandatory=True,completionFlag='gyeongju.guard_resolved',alternateFlag='gyeongju.seal_decoded',alternateItemId='',alternateChoice='repair',alternateText='Sửa mạch đá theo nhịp đã giải ở G12',alternateFlags=['gyeongju.stone_guard_friend'],battleFlags=['gyeongju.stone_guard_subdued'],text='Ta giữ người khám phá khỏi lõi sai nhịp. Nếu hiểu bệ G12, hãy sửa đường truyền của ta; hoặc chứng minh có thể vượt qua bằng trận đấu.',completedText='Lối đã an toàn. Tượng nhớ lựa chọn của Lumi; kết bạn chỉ xảy ra khi thật sự sửa mạch.')
 hue_archive=event('hue','U17')['puzzleId']
 puzzle('gyeongju','G12','Ngắn|Dài|Ngắn',['Ngắn','Dài'],'Ấn tín Huế và bản ghi U17 cùng nhịp: Ngắn → Dài → Ngắn. Chỉ đối chiếu, không tiêu ấn tín.',flags=['gyeongju.seal_decoded'],req=['hue.star_script_known','puzzle:'+hue_archive],needs=items(ancient_seal=1))
 E(12)['missingText']='Cần mang Ấn Tín Cổ và đọc nhịp tại thư khố U17 ở Huế. Có thể quay lại bằng World Map.'
 E(7).update(alternateCompletedText='Cảm ơn Lumi đã sửa mạch đá! Tớ sẽ giữ tín hiệu dịu lại để giúp bạn vượt Lõi Tinh Tú. Đường phía trước đã an toàn.',battleCompletedText='Ta đã ngừng giữ lối sau trận đấu. Lumi vẫn có thể tìm hiểu bệ G12; trận thắng không biến ta thành một người bạn đã được sửa mạch.')
 quest('GQ02',giverNode='G12',completeAtNode='G12',requiredItems=items(ancient_seal=1),requiredFlags=['gyeongju.seal_decoded','gyeongju.guard_resolved'],consumeItems=False,offer='Đọc Ấn Tín Cổ từ Huế tại bệ này, rồi cứu hoặc vượt tượng đá G7. Quay lại xác nhận; ấn tín vẫn cần cho Seoul.',reminder='Giải G12 theo manh mối U17, xử lý G7 rồi trở lại bệ. Nếu chưa đi Huế, có thể rời Silla và quay lại.',thanks='Bệ đã nối lại hai nhánh. Ấn tín vẫn nguyên vẹn trong túi để dùng tại S17.')
 extra('gyeongju','G12','gq02_offer_finish',questId='GQ02')
 E(9).update(text='Tượng đá không phải nguồn ác. Ba nét sao nằm ở G5, G6, G10. Đình trên đồi đi theo thềm G15; đừng vượt lõi trước khi hiểu đài quan sát.',onTalkFlags=['gyeongju.keeper_hint'])
 E(14).update(text='Lõi sao đóng khiên theo ba nhịp Sao → Trăng → Sao. Khi biểu tượng đang sáng khớp, chạm để mở khiên rồi mới dùng tia sáng.',onTalkFlags=['gyeongju.core_rhythm_known'])
 puzzle('gyeongju','G15','Thấp|Giữa|Cao',['Thấp','Giữa','Cao'],'Ba bóng đổ được vẽ cạnh bệ: Thấp → Giữa → Cao.',flags=['gyeongju.temple_shadows_solved'])
 puzzle('gyeongju','G17','Hoa|Trăng|Đèn',['Hoa','Trăng','Đèn'],'Hoa văn Jeonju đọc: Hoa → Trăng → Đèn.',flags=['gyeongju.west_lantern_solved'],req=['jeonju.beacon_lit','quest:GQ04'])
 quest('GQ04',requiredFlags=['gyeongju.west_lantern_solved','jeonju.beacon_lit'],rewardCoins=8,offer='Đài bờ tây phản hồi hoa văn Hanji. Nhận việc rồi ghép Hoa – Trăng – Đèn; xác nhận tại đây để nhận thưởng.',reminder='Giải hoa văn tại G17 rồi xác nhận hoàn thành.',thanks='Ánh đèn bờ tây đã tìm lại nhịp; manh mối được ghi và tám xu hỗ trợ chuyến đi.')
 extra('gyeongju','G17','gq04_offer_finish',questId='GQ04')
 E(16).update(rewardItems=items(healing_tea=2),rewardCoins=5,text='Hòm cứu hộ ở hiên đền có hai phần trà và năm xu. Nhận một lần để tiếp tục hành trình.')
 E(21).update(rewardItems=items(healing_tea=2),text='Túi cứu hộ dưới gốc cây có hai phần trà, không tự xuất hiện lại sau restart.')
 E(18).update(mandatory=True,requiredFlags=['gyeongju.three_stars_placed','gyeongju.guard_resolved','quest:GQ03'],completionFlag='gyeongju.star_core_disabled',text='Lõi không muốn làm hại người khám phá, nhưng nhịp sao bị đảo. Khiên theo Sao → Trăng → Sao; chạm ký hiệu đang sáng để mở khiên ba lượt.')
 quest('GQ03',giverNode='G18',completeAtNode='G19',requiredFlags=['gyeongju.three_stars_placed','gyeongju.star_core_disabled'],requiredItems=items(silla_prism=1),consumeItems=False,offer='Ổn định Lõi Tinh Tú G18 rồi mang lăng kính tới trạm G19. Giữ ống kính và vật chứng để tới Seoul.',reminder='Vượt lõi G18 bằng nhịp sao; xác nhận thắp trạm G19.',thanks='Trạm Silla đã sáng. Seoul chỉ mở khi cả bốn trạm nhánh cùng đồng bộ.')
 extra('gyeongju','G18','gq03_offer',questId='GQ03',requiredFlags=['gyeongju.three_stars_placed'])
 E(19).update(questId='GQ03',actor='Lumi',requiredFlags=['gyeongju.three_stars_placed','gyeongju.star_core_disabled'],completedText='Trạm Silla đã đồng bộ. Bốn trạm Huế, Đà Lạt, Jeonju và Gyeongju mới cùng mở Seoul; mình vẫn có thể quay lại giúp mọi người.')
 E(20).update(destinationCity='seoul',actor='Lumi',text='Cổng dẫn Seoul kiểm tra cả bốn trạm. Thiếu nhánh Việt Nam thì quay lại World Map hoàn thành rồi trở lại.',requiredFlags=['gyeongju.beacon_lit'])
 star=next(e for e in d['enemies'] if e['id']=='enemy_star_core');star.update(mechanic='star_cycle',shieldPattern=['Sao','Trăng','Sao'],weaknessFlags=['gyeongju.stone_guard_friend'],weaknessAttackReduction=3,weaknessHpReduction=6)
 s=setup('seoul',[[0,2,4,12,13,15],[1,3,5,6,7,8,9,14,16,18],[10,11,17,22],[19,20],[21]],{2:('quest:SQ02',1),16:('seoul.relay_a_on',2),18:('seoul.relay_reported',3),20:('seoul.final_cleared',4)})
 S=lambda n:event('seoul','S'+str(n))
 # Remove duplicate high-degree shortcuts into the final core; S19 is the
 # sole entrance and its courier cannot be skipped by route finding.
 s['edges']=[e for e in s['edges'] if e['id'] not in ['S7__S20','S6__S20','S7__S8','S1__S5']]
 S(0).update(actor='Lumi',text='Bốn trạm đã gửi tín hiệu thật. Mình phải gặp quản thủ S2, đọc hồ sơ cũ và hỏi Ji-won S7 trước khi đến lõi. Bot Mờ-01 đã cố cảnh báo mình.')
 S(2).update(questId='SQ02',text='Không phải mọi bot đều thù địch. Hồ sơ S4 và câu đố S3 giữ lệnh bảo vệ ban đầu. Mang ống kính Silla tới kỹ sư S11 để đọc nét đã bị sửa.')
 quest('SQ02',completeAtNode='S11',requiredItems=items(stargazer_lens=1),requiredFlags=['seoul.archive_read','seoul.palace_message_decoded','seoul.engineer_explained'],consumeItems=False,offer=S(2)['text'],reminder='Mở hồ sơ S4, giải lời nhắn S3, gặp kỹ sư S11 và xác nhận sự thật. Giữ ống kính.',thanks='Lệnh gốc là bảo vệ đèn khỏi quá tải. Có người đã sửa lệnh và cắt cảnh báo của Bot Mờ-01. Có thể cứu hệ thống.')
 S(4).update(flags=['seoul.archive_read'],text='Hồ sơ: hệ phòng vệ cũ phải ngắt nguồn khi quá tải, không được tấn công cư dân. Một dòng lệnh đã bị chèn. Đối chiếu tại S3.')
 puzzle('seoul','S3','Ngắn|Dài|Ngắn',['Ngắn','Dài'],'Bản ghi Huế và hồ sơ cũ: Ngắn → Dài → Ngắn. Đọc nhịp để khôi phục lời nhắn bị cắt.',flags=['seoul.palace_message_decoded'],req=['seoul.archive_read','quest:SQ02'])
 S(5).update(mandatory=True,completionFlag='seoul.drone_disabled',alternateFlag='seoul.archive_read',alternateChoice='stealth',alternateText='Theo hành lang an toàn ghi trong hồ sơ để tránh drone')
 S(7).update(questId='SQ01',text='Ba nguồn phải do em tự kích hoạt: nước xanh Đà Lạt tại S16, Ấn Tín Cổ Huế tại S17 và mạch Hanji tại S18. Khi đủ cả ba, quay lại đây xác nhận đồng bộ.')
 quest('SQ01',requiredFlags=['seoul.relay_a_on','seoul.relay_b_on','seoul.relay_c_on'],flags=['seoul.relay_reported'],requiredItems=items(blue_water_signal=1,ancient_seal=1,hanji_circuit=1),consumeItems=False,offer=S(7)['text'],reminder='Kích hoạt riêng S16, S17, S18 rồi báo Ji-won tại S7. Các vật chứng chỉ đối chiếu, không tiêu hao.',thanks='Ba relay thật đã đồng bộ. Đường hầm S19 dẫn tới lõi; hãy nhớ còn có lựa chọn sửa hệ thống.')
 S(8).update(completionFlag='seoul.signal_bot_disabled',alternateFlag='seoul.relay_a_on',alternateChoice='relay_pulse',alternateText='Dùng nhịp rơle A ngắt tín hiệu lỗi')
 puzzle('seoul','S10','Sao|Trăng|Sao',['Sao','Trăng'],'Ống kính Silla đọc ba tia: Sao → Trăng → Sao. Kính vẫn được giữ sau khi hiệu chuẩn.',flags=['seoul.tower_calibrated'],needs=items(stargazer_lens=1))
 S(11).update(text='Bot ấy không sinh ra để đánh nhau. Hồ sơ và ống kính cho thấy lệnh bảo vệ đã bị sửa. Hãy xác nhận kết quả rồi phục hồi nguồn thay vì xóa toàn bộ.',onTalkFlags=['seoul.engineer_explained','seoul.hidden_tower_found'])
 extra('seoul','S11','sq02_finish',questId='SQ02')
 S(12).update(rewardItems=items(healing_tea=3),rewardCoins=12,text='Tủ tiếp sức công viên có ba phần trà và mười hai xu; mỗi tủ chỉ nhận một lần.')
 S(14).update(text='Đò công viên chỉ nhận chuyến khi bốn trạm nhánh đã sáng. Lumi ngồi trên thuyền khi vượt nước; những tuyến cầu vẫn đi bộ bình thường.',onTalkFlags=['seoul.ferry_acknowledged'])
 S(15).update(mandatory=True,completionFlag='seoul.bridge_guard_disabled',alternateItemId='ancient_seal',alternateKeepItem=True,alternateChoice='seal_hack',alternateText='Đối chiếu Ấn Tín Cổ · giữ lại ấn tín')
 puzzle('seoul','S16','Nước|Sáng',['Nước','Sáng'],'Tín hiệu nước Đà Lạt ghi hai công tắc: Nước → Sáng. Chạm từng công tắc rồi xác nhận.',flags=['seoul.relay_a_on'],req=['quest:SQ01','da_lat.beacon_lit'],needs=items(blue_water_signal=1),rewards=items(relay_a=1))
 puzzle('seoul','S17','Ngắn|Dài|Ngắn',['Ngắn','Dài'],'Rơle B đọc ấn Huế: Ngắn → Dài → Ngắn. Ấn chỉ dùng xác thực, không tiêu hao.',flags=['seoul.relay_b_on'],req=['quest:SQ01','hue.beacon_lit'],needs=items(ancient_seal=1),rewards=items(relay_b=1))
 puzzle('seoul','S18','Hoa|Trăng|Đèn',['Hoa','Trăng','Đèn'],'Rơle C đọc mạch Hanji: Hoa → Trăng → Đèn. Giữ mạch sau khi đồng bộ.',flags=['seoul.relay_c_on'],req=['quest:SQ01','jeonju.beacon_lit'],needs=items(hanji_circuit=1),rewards=items(relay_c=1))
 S(19).update(mandatory=True,completionFlag='seoul.courier_disabled',alternateFlag='seoul.relays_ready',alternateChoice='relay_shutdown',alternateText='Ngắt nguồn từ ba relay đã đồng bộ',text='Lệnh từ lõi đã bị sửa. Có thể giao chiến hoặc dùng ba relay thật để ngắt đường truyền.')
 S(20).update(mandatory=True,requiredFlags=['seoul.relays_ready','seoul.relay_reported','seoul.courier_disabled','quest:SQ03'],completionFlag='seoul.final_cleared',alternateFlag='',alternateItemId='',alternateFlags=['seoul.system_repaired'],battleFlags=['seoul.system_stabilized'],text='Lệnh ban đầu của ta là bảo vệ những chiếc đèn... Pha đầu phá lớp khiên; khi lõi lộ, có thể đánh, phòng thủ hoặc sửa ba mạch nếu đã tìm ra sự thật.')
 S(20).update(alternateCompletedText='Ba mạch đã được sửa. Lệnh bảo vệ ban đầu được phục hồi; hệ thống sẽ giữ những chiếc đèn an toàn. Hẹn gặp Lumi tại lễ hội S21.',battleCompletedText='Lõi lỗi đã dừng sau hai pha. Những cơ cấu bảo vệ an toàn vẫn còn; Lumi có thể tới S21 để nghe lời kết.')
 quest('SQ03',completeAtNode='S21',requiredFlags=['seoul.final_cleared'],acceptFlags=['seoul.relays_ready','seoul.relay_reported'],offer='Đối mặt Lõi Nhiễu hai pha tại S20. Sau khi xử lý, tới lễ hội S21 và xem hết lời kết/credits; còn có thể quay lại làm quest phụ.',reminder='Vượt hai pha S20 rồi xem epilogue S21. Không có phần thưởng chỉ vì bước tới boss.',thanks='Sáu thành phố tìm lại nhịp chung. Những nhiệm vụ phụ còn lại vẫn chờ Lumi.')
 extra('seoul','S20','sq03_offer',questId='SQ03',requiredFlags=['seoul.relays_ready','seoul.relay_reported'])
 final=next(e for e in d['enemies'] if e['id']=='enemy_final_core');final.update(mechanic='two_phase',phaseOneHP=44,phaseTwoHP=44,repairFlags=['seoul.truth_discovered','seoul.relays_ready'],weaknessFlags=['seoul.relay_a_on','seoul.relay_b_on','seoul.relay_c_on'],weaknessHpReduction=0,weaknessAttackReduction=4,weaknessDefenseReduction=4)
 S(21).update(actor='Lumi',questId='SQ03',requiredFlags=['seoul.final_cleared'],completionFlag='game.ending_seen',endingPages=[
  'Hội An — Cô Mai thắp chiếc đèn đầu tiên. Cảnh báo của Bot Mờ-01 cuối cùng đã được nghe trọn: đừng để những chiếc đèn vượt quá nhịp an toàn.',
  'Huế — Bức thư và Ấn Tín Cổ giúp hai bờ sông tìm lại tín hiệu. Bà Nguyệt, Lê An và những người chèo đò gửi lời chào tới bạn bè Hàn Quốc.',
  'Đà Lạt — Lan đọc rõ biển chỉ đường. Chú chó không còn đói; sương trở thành lời cảnh báo dịu nhẹ bên thác, thay vì một bức tường.',
  'Jeonju — Hana ghép những hoa văn. Một trái ớt, một củ hành và những lá thư đã nối những điều nhỏ bé giữa hai nền văn hóa.',
  'Gyeongju — Min giữ ba biểu tượng sao. Những người canh đá có thể bảo vệ di sản mà không chặn người đến tìm hiểu.',
  'Seoul — Ba nguồn sáng hòa nhịp. Lễ hội sáu thành phố bắt đầu; giúp đỡ và lắng nghe đã đem lại một cách kết thúc khác với phá hủy.',
  'LIGHT BETWEEN CULTURES\nCảm ơn bạn đã cùng Lumi nối những câu chuyện Việt–Hàn.\nThiết kế, cốt truyện và art: bộ tài nguyên LBC của tác giả project.\nBản playtest nội dung — các hình tạm và cân bằng sẽ tiếp tục hoàn thiện.\nBấm tiếp tục để lưu kết thúc và trở lại khám phá.'
 ])
 S(22).update(requiredFlags=['seoul.truth_discovered','quest:SQ04'],completionFlag='seoul.secret_message_read',text='Tin nhắn của Hội Bóng Mờ: hệ phòng vệ đã cố cảnh báo từ Hội An. Kẻ sửa lệnh muốn chia cắt lòng tin; những bản ghi còn lại sẽ dẫn tới câu chuyện tiếp theo.')
 quest('SQ04',acceptFlags=['seoul.truth_discovered'],requiredFlags=['seoul.secret_message_read','seoul.truth_discovered'],offer='Dùng kính ở S22 để đọc bản tin bí mật. Nhận nhiệm vụ, đọc rồi xác nhận tại đây.',reminder='Kiểm tra tin nhắn tại S22 và xác nhận đã đọc.',thanks='Bản tin bí mật đã được lưu; góc nhìn về những người canh đèn đã thay đổi.')
 extra('seoul','S22','sq04_offer_finish',questId='SQ04',requiredFlags=['seoul.truth_discovered'])
 # Existing hidden-node self-dependencies are corrected without deleting IDs.
 g['nodes'][21]['requiredFlags']=['gyeongju.secret_herb_found'];s['nodes'][22]['requiredFlags']=['seoul.hidden_tower_found'];s['nodes'][20]['requiredFlags']=['seoul.relays_ready','seoul.relay_reported'];s['nodes'][21]['requiredFlags']=['seoul.final_cleared']
 # Match all original node-specific dialogue in the supplied playtest spec,
 # preserving the four released chapters' authored interactions/rewards.
 for cid in ['gyeongju','seoul']:
  for n in city(cid)['nodes']:
   e=event(cid,n['id']);e['actor']=e.get('actor') or 'Lumi';e['completedText']=e.get('completedText') or 'Lumi đã ghi lại câu chuyện tại đây. Có thể quay lại hoặc tiếp tục trên những tuyến đã mở.'
 for n in [1,8,11]:E(n)['text']+=' Lumi theo lối lát đá, cầu và bến, không đi tắt qua nước.'
 # Import clean full-resolution originals once. Never replace destination cards.
 for cap,cid in [('Gyeongju','gyeongju'),('Seoul','seoul')]:
  src=R/f'LBC_AnhMoi_BanGiaoCodex_20261015/Maps/{cap}/Map_Overview_A.png';dst=R/f'Assets/_Game/Art/Maps/{cap}/Map_{cap}_Overview_A.png';dst.parent.mkdir(parents=True,exist_ok=True)
  if dst.exists():assert dst.read_bytes()==src.read_bytes()
  else:shutil.copy2(src,dst)
 # Art actors will be bound in the existing registry by FullStorySetup. Missing
 # NPC art is not substituted with another named character.
 for cid in ['gyeongju','seoul']:
  a['mapActors']=[v for v in a['mapActors'] if v['cityId']!=cid]
  for n in city(cid)['nodes']:
   e=event(cid,n['id']);aid=e.get('enemyId') if e['type']=='encounter' else 'npc_min' if n['id']=='G3' else None
   if aid:a['mapActors'].append(dict(cityId=cid,nodeId=n['id'],artId=aid,size=dict(x=52,y=62),offset=dict(x=22,y=8),hideWhenResolved=e['type']=='encounter' and n['id']!='G7'))
 # Unique companion uses existing approved lantern art; a visible optional
 # following light is keyed to the existing HQ03 flag, with no new quest/item.
 original_hq03=copy.deepcopy(next(q for q in before['quests'] if q['id']=='HQ03'))
 original_hq03['flags']=list(dict.fromkeys(original_hq03.get('flags',[])+['hoi_an.mini_lantern_companion']))
 d['quests'][next(i for i,q in enumerate(d['quests']) if q['id']=='HQ03')]=original_hq03
 # Existing four city geometry, IDs and all original quests remain untouched,
 # except the explicit companion completion flag above.
 for c in before['cities']:
  if c['id'] not in ['gyeongju','seoul']:assert c==city(c['id'])
 assert len({n['id'] for c in d['cities'] for n in c['nodes']})==133
 for field in ['quests','enemies','items','puzzles']:assert len({v['id'] for v in d[field]})==len(d[field])
 write(p,d);write(R/'Assets/_Game/Resources/Content/AdventurePresentation.json',a)
 write(OUT/'authoring-manifest.json',dict(spec=str(spec.relative_to(R)),specSHA256=hashlib.sha256(spec.read_bytes()).hexdigest(),baseline=str(B),status='AUTHORED_NOT_RUNTIME_VERIFIED',resolutions=[
  'Preserve all four released city graphs, node IDs/positions and save schema.',
  'G4-G19 replaced by G4-G9; add G18-G19; remove G2-G11/G11-G12 to keep degree3; G12 remains reachable via G16. Third symbol reachable before observatory gate.',
  'G7 gate uses guard_resolved for win OR repair; stone_guard_friend only on peaceful repair.',
  'G12 verifies Hue seal and script without consuming them; GQ02 requires both decoded seal and resolved guard.',
  'S2 acceptance reveals first urban branch; A reveals second; report three real relays opens S19; courier is sole route to S20.',
  'Remove four redundant Seoul shortcuts to avoid degree4 and courier bypass.',
  'Relay readiness is derived from three real flags; quest report requires explicit S7 interaction.',
  'S20 has two 44HP phases (original proposed total88); relay weakness affects only this new boss.',
  'Ending pages and credits require real button advances; ending_seen only on final page; SQ03 finishes there.',
  'Only the user rename of the byte-identical full-story spec differs from the pre-task document path.'
 ],proposals=['Concrete puzzle symbols, additional chest supplies, GQ04 eight coins, final relay defense reduction are PLAYTEST_NEW, not fixed original rewards.'],missingArt=['G9 keeper','G14 inscription keeper','S2 curator','S7 Ji-won','S11 engineer','S14 ferry NPC','G6 fox and S5 drone standalone final sprites'],terrainStatus='New city geometry pending separate terrain authoring/runtime review; no terrain PASS inferred from movement'))
 print('Authored Gyeongju/Seoul, eight canonical quests; preserved all133IDs/38items/29quests and released four city graphs.')
def friendly_labels():
 """Presentation-only delta after the gameplay freeze; never changes conditions."""
 p=R/'Assets/_Game/Resources/Content/GameplayContent.json';d=read(p);frozen=read(R/'Validation/FullStory-20261017/Build04-Content.json');rows=[]
 captions={
  ('gyeongju','G17'):'Nhận nhiệm vụ Hoa văn bờ tây tại đây trước khi ghép hoa văn Hanji.',
  ('gyeongju','G18'):'Nhận nhiệm vụ Đêm đài thiên văn tại đây trước khi giao chiến; nhật ký sẽ ghi đường tới trạm G19.',
  ('seoul','S3'):'Hãy mở hồ sơ lịch sử S4 rồi mới giải lời nhắn tại sân cung điện.',
  ('seoul','S16'):'Nhận nhiệm vụ Ba nhịp đồng bộ ở Ji-won S7 và mang Tín Hiệu Nước Xanh từ Đà Lạt.',
  ('seoul','S17'):'Cần nhiệm vụ Ba nhịp đồng bộ và Ấn Tín Cổ từ Huế. Ấn chỉ được đối chiếu, không tiêu hao.',
  ('seoul','S18'):'Nhận nhiệm vụ Ba nhịp đồng bộ ở Ji-won S7 và mang Mạch Đèn Hanji từ Jeonju.',
  ('seoul','S20'):'Nhận nhiệm vụ Lõi sáng cuối tại đây trước khi chạm vào lõi. Ba relay và đường hầm phải được xử lý thật.',
  ('seoul','S22'):'Nhận nhiệm vụ Tín hiệu bóng Mờ tại đây rồi kiểm tra bản tin bằng ống kính.'
 }
 for e in d['events']:
  if not e.get('additive') and (e['cityId'],e['nodeId']) in captions:
   rows.append(dict(city=e['cityId'],node=e['nodeId'],before=e.get('missingText'),after=captions[e['cityId'],e['nodeId']]))
   e['missingText']=captions[e['cityId'],e['nodeId']]
 comparison=copy.deepcopy(d)
 for e,old in zip(comparison['events'],frozen['events']):
  if (e['cityId'],e['nodeId']) in captions and not e.get('additive'):
   if 'missingText' in old:e['missingText']=old['missingText']
   else:e.pop('missingText',None)
 assert comparison==frozen,'Non-presentation gameplay data changed after build04'
 write(p,d);write(OUT/'friendly-labels-delta.json',dict(type='DISPLAY_TEXT_ONLY',gameplayFieldsIdenticalToBuild04=True,changes=rows))
 print('Updated',len(rows),'friendly requirement captions; all gameplay data identical to the frozen build04.')
if __name__=='__main__':
 if '--friendly-labels' in sys.argv:friendly_labels()
 else:main()
