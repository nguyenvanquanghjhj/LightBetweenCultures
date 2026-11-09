"""Owner-authorized waypoint correction. Offline render/diff first; --apply only changes existing bends."""
from pathlib import Path
import argparse,copy,csv,hashlib,html,json,math,re
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Validation/RouteShorteningProposal-20261019';DOC=ROOT/'Docs/QA/RouteShorteningProposal-20261019'
CONTENT=ROOT/'Assets/_Game/Resources/Content/GameplayContent.json'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
NAMES=dict(zip(['hoi_an','hue','da_lat','jeonju','gyeongju','seoul'],['HoiAn','Hue','DaLat','Jeonju','Gyeongju','Seoul']))
MODELS=read(ROOT/'Validation/RouteMarkupExport-20261018/edge_mapping.json')['cities']
HIST={e['EdgeID']:e for e in read(ROOT/'Docs/Maps/AppliedRouteReferences-20261019/edge-diff.json')}
FREEZE=read(DOC/'baseline.json')
BASE=ROOT/'Backups-unused' # Source snapshot is outside this project, never searched recursively.
BEFORE=OUT/'current-content-snapshot.json'
if not BEFORE.exists():
 assert sha(CONTENT)==next(e['sha256'] for e in FREEZE['entries'] if e['path']=='Assets/_Game/Resources/Content/GameplayContent.json')
 BEFORE.write_bytes(CONTENT.read_bytes())
CURRENT=read(BEFORE)
OWNER={}
def owner(keys,text):
 for key in keys.split():OWNER[key]=text
owner('U-E019','U-E019 vòng qua bên kia với mục đích gì? Bỏ ngay.')
owner('U-E018','Kiểm tra tường thành/cổng, bỏ vòng không hợp lý.')
owner('U-E022','U-E022, nhảy qua tường thành à?')
owner('U-E011 U-E015 U-E027','U-E011 và U-E015 và U-E027 quá gần nhau, quá rối, sao không gộp lại một đường chung? Giữ ba edge; dùng chung corridor.')
owner('J-E007','J2 → J5 vòng phía đông quá xa; ưu tiên phố/sân gần nhất.')
owner('J-E029','J17 → J19 vòng qua nhiều công trình rồi trở lại gần đích.')
owner('J-E018','Kiểm tra cầu/kênh; rút ngắn qua lối hợp lệ, không băng nước.')
owner('J-E002','J-E002 lúc trước đã đi đúng lại sửa thành sai, cần chỉnh lại cho thẳng, không nhảy tường.')
owner('J-E028 J-E008 J-E004','J-E028, J-E008, J-E004 quá ẩu, cần lưu ý lối đi hẹp sát J-E004.')
owner('J-E011 J-E016','Đoạn gần J7 cũng nên nhích lên một tí để né mái nhà.')
owner('J-E017 J-E019','Gần J9 vuông thêm một tí để tránh mái nhà.')
owner('J-E025','J-E025 thì đang leo mái kìa.')
owner('J-E030','J-E030 cũng đang leo mái.')
owner('J-E001','Đoạn ra J10 có cái cầu không đi mà lại đi sông à?')
owner('J-E026','J-E026 cũng băng sông.')
owner('D-E025','D20 → D15: bỏ detour lớn; bám bờ hồ/cầu/dốc thật.')
owner('D-E020','D13 → D21: chưa rõ lối xuống lodge; không cắt vách/cây.')
owner('D-E008 D-E015 D-E019','Ở trên khu D14, D19 tự nhiên đang ngon đi sửa thành đường quắn quắn làm gì vậy.')
owner('D-E003 D-E001 D-E023 D-E012','D-E003, D-E001, D-E023, D-E012 chả hiểu bày ra nhiều làm gì, một đường thôi. Giữ graph, dùng corridor chung khi có thật.')
owner('G-E004 G-E006','Kiểm tra bờ ao, cầu và đảo; không sáng tác cầu mới.')
owner('S-E002','S0 → S14 nghi qua nước thiếu cầu; tìm hành lang đất/cầu thật.')
owner('S-E009','Đoạn S-E009 nó sai rõ ràng vậy mà mãi không sửa. Protected ferry vẫn phải kiểm tra.')
owner('S-E016','S-E016 nữa, như siêu nhân.')
owner('S-E017','S-E017 ổn rồi thì lại lên danh sách sửa. Mặc định GIỮ NGUYÊN.')
owner('S-E006 S-E012','S-E006 với S-E012 gần nhau rồi, hợp lý thì không nối cho hai đường liên mạch luôn. Không thêm adjacency.')
owner('H-E012','Đường đi vào H8 phía dưới nên vòng qua một xíu vì đã đi qua rào. Không được nối H7 và H21 trực tiếp như thế; tra graph thật.')
owner('H-E007','Không được nối H7 và H21 trực tiếp như thế. Kiểm tra tuyến giao nhau; không tạo adjacency.')

DRAFT={};NOTES={};CONF={};REASON={}
def draft(key,pts,reason,confidence='MEDIUM'):
 DRAFT[key]=pts;REASON[key]=reason;CONF[key]=confidence
def historical(key,reason,confidence='MEDIUM'):
 draft(key,[(round(p['x']*1448),round((1-p['y'])*1086)) for p in HIST[key]['OldWaypoints']],reason,confidence)
def decision(key,text):NOTES[key]=text

# Streets/steps and shared stair landing visible in the canonical courtyard.
draft('U-E019',[(580,247),(600,229),(623,218),(655,228),(681,220)],'TOO_LONG_DETOUR: bỏ vòng bắc 490,166 → 897,92; theo lối vườn phía nam hồ nhỏ. Cần xem sát mép bồn cây.')
fork=[(1010,264),(1046,246)]
draft('U-E011',[(902,290),(930,282),(974,281),*fork,(1080,246)],'SHOULD_SHARE_CORRIDOR: cùng đường trước sân và chân thang; chỉ tách tại landing 1046,246.','HIGH')
draft('U-E027',[*fork,(1048,222)],'SHOULD_SHARE_CORRIDOR: cùng landing với U-E011/U-E015, lên thang đến U8.','HIGH')
draft('U-E015',[(1048,222),(1046,246),(1080,246)],'SHOULD_SHARE_CORRIDOR: cùng nhánh chân thang; bỏ góc gấp 1077,233.','HIGH')
decision('U-E018','CROSSES_WALL / ENDPOINT_REVIEW: arrival U12=(420,350) nằm ngoài bờ/mép nước trong tranh. Lối qua cổng lớn ở khoảng (860,420) dài hơn nhưng không giải quyết được endpoint. Giữ riêng cạnh, cần duyệt anchor/landing ngoài phạm vi waypoint.')
decision('U-E022','CROSSES_WALL: đoạn (644,393)→(647,311) cắt mặt tường. Arrival U12=(420,350) sát/một phần ngoài mép bờ. Không tự vẽ cổng hoặc đổi anchor. Cần quyết định riêng lối cổng lớn và landing U12.')

# Reuse owner-preferred old corridor only on these edges, never rollback the city.
historical('J-E002','OWNER_PREFERRED_OLD_CORRIDOR: khôi phục lối cũ ở mặt tiền/tuyến nam; bỏ đường mới nhảy tường phía bắc khu nhà.')
historical('J-E030','CROSSES_ROOF: bỏ đường mới qua mái trung tâm; quay về lối phía nam block theo corridor cũ.')
historical('J-E025','CROSSES_ROOF: dùng đường vòng phía đông công viên, bám ngoài tường/gate thay vì cắt mái phía tây.')
historical('J-E018','TOO_LONG_DETOUR: lối hẹp ven bờ phía tây, tránh vòng qua J4/J7 và mái hanok.')
draft('J-E007',[(943,588),(923,620),(921,652),(944,674)],'TOO_LONG_DETOUR: bỏ vòng đông 1211,641; đi sát lối phía tây block, vào sân J5. Mép mái tại đầu J2 cần xem cận cảnh.')
draft('J-E029',[(815,437),(847,414),(855,389),(837,361),(834,331),(847,307),(881,287),(896,263),(891,235)],'TOO_LONG_DETOUR: theo ngõ đông của block và tường ngoài, bỏ vòng qua J1/J4/J7. Đầu J19 giữ nguyên, không đi xuyên tường.')
gate=[(1056,532),(1098,508),(1113,472),(1097,443),(1077,424),(1077,400)]
draft('J-E008',[(990,555),(1018,540),*gate],'CROSSES_ROOF: đi mặt tiền/phố bên đông rồi qua cửa sân J12; không cắt đỉnh mái 1012,480.')
draft('J-E028',[(820,462),(851,474),(888,492),(931,511),(980,532),(1018,540),*gate],'SHOULD_SHARE_CORRIDOR: dùng phố mặt tiền chung với J-E004/J-E008; vào cùng cổng sân J12.')
draft('J-E004',[(680,506),(724,534),(770,559),(815,580),(860,600),(905,620),(945,632),(976,615),(975,590)],'CROSSES_ROOF / SHOULD_SHARE_CORRIDOR: theo phố mặt tiền phía nam dãy hanok, không vòng bắc xuyên mái. Đoạn hẻm cuối J2 vẫn cần owner xem cận cảnh.')
draft('J-E011',[(644,278),(612,269),(574,266),(535,264),(510,267)],'CROSSES_ROOF: nhích đoạn quanh J7 về phía sân/thềm cửa, bỏ spur 647,264 rồi quay về.','MEDIUM')
draft('J-E016',[(457,250),(437,225),(414,239),(391,251),(354,252),(317,249),(281,247),(247,246)],'CROSSES_ROOF: nâng hành lang phía bắc nhà quanh J7; giữ node J7 và bờ tây.','MEDIUM')
draft('J-E017',[(318,493),(307,516),(279,540),(242,554),(203,568),(174,584),(157,594),(157,612)],'CROSSES_ROOF: cua rõ theo mép hiên J9, không cắt góc mái cuối đoạn.')
draft('J-E019',[(157,632),(167,654),(195,681),(231,703),(273,723),(304,748),(315,777),(297,801),(269,823),(247,849),(235,877)],'CROSSES_WATER / BRIDGE_APPROACH: bỏ đoạn đâm qua mép nhà; qua đầu cầu 304,748 rồi lối thềm phía tây tới J10. Mái che cầu cần art review, không thêm occlusion.')
draft('J-E001',[(695,1008),(651,982),(602,957),(551,934),(503,909),(453,882),(405,857),(356,834),(317,820),(282,836),(258,863),(241,890)],'BRIDGE_APPROACH: corridor hiện có ra J10 được giữ, chỉ bỏ đầu cua 227,899 dư sát arrival. Cầu thực ở hành lang tây, không tự thêm bridge.', 'HIGH')
decision('J-E026','CROSSES_WATER: travel=walk; arrival J16=(613,592) nằm trên mép tường/cạnh kênh. Đường mới đang chéo qua kênh tới J1. Cầu thật ở khoảng (585,665) yêu cầu hành lang đầu J16 chưa rõ. Giữ riêng để duyệt landing/đường xuống cầu; không tự đổi ferry.')

# Shared foreground street, rather than unrelated detours through the houses.
entry=[(750,713),(716,733),(687,760),(670,722),(640,690),(610,662),(576,637),(539,620)]
spine=[(541,609),(585,587),(622,558),(639,531)]
draft('D-E001',[*entry,(500,610),(470,608),(449,619),(431,639),(420,649)],'SHOULD_SHARE_CORRIDOR: phố trước dãy nhà từ bến đến D18; dùng cùng đoạn D-E003.')
draft('D-E003',[*entry,(500,610),*spine,(640,501),(670,497),(694,489),(717,495),(742,495),(773,483),(811,474),(850,470),(886,471)],'SHOULD_SHARE_CORRIDOR: thay đường lên thẳng xuyên dãy mái bằng phố D0–D18, rồi dùng chung D18–D3 với D-E023.')
draft('D-E023',spine,'SHARED_CORRIDOR: giữ chính hành lang D18–D3 đã hợp lý, đồng nhất với suffix D-E003.','HIGH')
draft('D-E012',[(329,479),(355,490),(375,507),(401,495),(420,472),(455,450),(491,427),(529,393)],'TOO_LONG_DETOUR: theo lối bậc thang/đường quanh hông nhà thờ, bỏ vòng qua D18/D3. Không buộc bốn cạnh đi chung một đoạn không liên quan.')
historical('D-E015','OVERCOMPLEX_WAYPOINTS: bỏ vòng lặp gần D19, giữ đường cong trên đường nhựa hông nhà thờ.','HIGH')
draft('D-E008',[(975,270),(956,277),(940,277),(915,270),(915,249),(933,230),(927,207),(909,187),(885,167),(849,163),(811,159),(774,145),(746,120),(717,121),(707,144),(719,169),(681,179),(640,176),(603,186),(557,194),(536,207),(491,206),(449,199),(398,185),(371,175),(348,163),(326,157),(302,151)],'OVERCOMPLEX_WAYPOINTS: giữ đường bắc, chỉ bỏ vòng chéo thừa trước D14; không làm lại toàn tuyến.')
historical('D-E019','TOO_LONG_DETOUR: trở lại corridor cũ qua phố/đầu cầu bắc, bỏ vòng xuống nửa hồ rồi ngược về D14. Cần review sát nhà thờ, không nhận terrain PASS tuyệt đối.')
decision('D-E020','AMBIGUOUS_TERRAIN: không nhìn thấy liên tục lối xuống lodge giữa D13–D21; vách đá/cây che cầu thang. Giữ geometry, cần owner xác nhận hành lang.')
decision('D-E025','TOO_LONG_DETOUR nhưng lối bờ đông để rút ngắn cần đi qua đoạn dốc D13–D21 chưa rõ. Giữ riêng thay vì thay bằng đường xuyên hồ hoặc bịa cầu thang; cần owner đánh dấu lối xuống liên tục.')

draft('G-E004',[(650,752),(620,737),(580,716),(543,697),(505,675),(470,656),(430,636),(405,615),(390,598),(362,583),(340,568),(315,550)],'CROSSES_WATER: chuyển khỏi vòng dưới ao lên hành lang cầu/bậc đã vẽ; đoạn qua pavilion là mặt cầu, chưa thêm occlusion mới. Ground projection gần G2 cần owner xem.','MEDIUM')
decision('G-E006','CROSSES_WATER: không thấy cầu liên tục từ đảo G2 lên bờ G17. Có đò trang trí không đồng nghĩa ferry gameplay. Giữ riêng và chờ quyết định, không sáng tác cầu mới.')

draft('S-E006',[(394,250),(390,271),(380,301),(369,337),(362,369),(417,369),(478,362),(533,345),(591,326),(641,305),(696,284),(718,276),(701,307),(682,330),(665,350)],'KEEP_CORRIDOR / SHARED_CORRIDOR: hình học tốt được giữ; định nghĩa đoạn chung 718,276–701,307–682,330 cho S-E012.','HIGH')
draft('S-E012',[(778,392),(756,374),(730,355),(702,342),(682,330),(701,307),(718,276),(756,265),(783,260),(829,248),(869,231),(923,208),(965,176),(1013,150),(1031,167),(1082,180),(1085,209),(1100,248),(1124,273)],'SHOULD_SHARE_CORRIDOR: dùng chung đoạn đường gần quảng trường/đường lên đồi với S-E006. Chỉ geometry; không tạo chuyển tuyến/adjacency tại giao điểm.','MEDIUM')
decision('S-E016','CROSSES_ROOF / AMBIGUOUS_TERRAIN: hình hiện tại lướt qua cụm nhà cao tầng; không nhìn rõ lối bộ dưới tầng và đoạn xuống từ đồi. Giữ riêng, không thay bằng đường chéo ngắn nhưng xuyên nhà khác.')
decision('S-E002','CROSSES_WATER: thiếu cầu liên tục nối S0 và S14 trên canonical. Rút ngắn không thể tạo lối đi bộ an toàn với arrival/semantics hiện tại. Giữ riêng; cần quyết định cầu/ferry/landing ngoài phạm vi waypoint.')
decision('S-E009','PROTECTED_ROUTE_REVIEW: ferry thật, có Boat graphic và bỏ bóng chân trong source. Arc hiện tại chạm vùng đất phía nam trước landing S14; cần duyệt riêng đường nước/landing hoặc cách trình bày đò. Không biến ferry thành walk, không sửa protected geometry ở lượt này.')

# Final visual review: retain the unbridged pond edge rather than drawing a new water crossing.
DRAFT.pop('G-E004'); REASON.pop('G-E004'); CONF.pop('G-E004')
decision('G-E004','No continuous bridge joins the G1 mainland and G2 island. Retained individually: changing bends alone cannot create the missing crossing; no ferry or bridge invented.')
# Keep already-good shared spines byte-for-byte; no rounding-only edits.
for unchanged in ['D-E023','S-E006','J-E001']:
 DRAFT.pop(unchanged); REASON.pop(unchanged); CONF.pop(unchanged)
# H8: center the approach on the visible paving west of the fence.
he=HIST['H-E012']['NewWaypoints']
draft('H-E012',[(round(p['x']*1448),round((1-p['y'])*1086)) for p in he[:30]]+[(192,248),(190,230),(197,216),(200,190),(198,162),(204,143),(225,132)],'Move the H8 approach west of the fence onto the paving; keep the entire earlier corridor, endpoints and graph.','MEDIUM')
draft('J-E011',[(620,270),(588,265),(553,259),(517,252),(495,256)],'Use the paved strip above the hanok roofs; preserve the fixed J7 arrival, whose last roof projection remains an anchor limitation.','MEDIUM')
draft('J-E016',[(495,256),(474,241),(454,228),(437,225),(414,239),(391,251),(354,252),(317,249),(281,247),(247,246)],'Use the north side of the J7 block and the west street; no new cover or arrival anchor.','MEDIUM')
draft('J-E007',[(985,583),(1005,589),(1014,614),(995,640),(974,655),(963,675)],'Replace the long east detour with the nearby east-side alley around the hanok block; retain both arrivals.','MEDIUM')
draft('S-E012',[(792,377),(782,344),(772,317),(756,291),(739,272),(718,276),(756,265),(783,260),(829,248),(869,231),(923,208),(965,176),(1013,150),(1031,167),(1082,180),(1085,209),(1100,248),(1124,273)],'Keep the existing street east of the church instead of the rejected cross-church draft; meet the S-E006 corridor at the hill junction without adding adjacency.','HIGH')
draft('G-E024',[(1129,251),(1105,281),(1079,311),(1046,329),(1008,349),(961,348),(943,367),(939,354),(892,353),(853,350),(819,338)],'Remove the stair up-and-back loop at G18; join the existing G15 approach at the stair landing, retaining the remaining accepted corridor.','HIGH')
# D13 to D14: use the visible eastern garden / northern road rather than the church detour.
draft('D-E019',[(1180,478),(1170,449),(1167,419),(1169,382),(1180,352),(1201,329),(1225,315),(1241,300),(1239,282),(1223,264),(1193,254),(1151,249),(1111,247),(1073,247),(1041,249),(1012,254),(985,265),*DRAFT['D-E008']], 'Replace the half-lake/church detour with the eastern garden stair approach and existing northern road to D14; fixed endpoints retained.','MEDIUM')
# D20 to D15: reuse the existing eastern-bank corridors. The cabin slope remains a documented limitation.
draft('D-E025',[(957,478),(995,478),(1036,464),(1071,452),(1117,450),(1147,466),(1123,484),(1116,511),(1133,528),(1166,539),(1183,526),(1182,505),(1216,577),(1208,597),(1189,610),(1176,627),(1188,648),(1204,666),(1194,684),(1180,702),(1187,725),(1200,745),(1204,764),(1185,780),(1152,791),(1113,793),(1071,792),(1036,797),(998,803),(966,803)], 'Remove the west-shore detour: reuse D20-D13 and the existing D13-D21 eastern-bank corridor, then follow the lower-bank path to D15. Cabin slope visibility is limited; best-effort, not a complete terrain PASS.','MEDIUM')
NOTES.pop('D-E025',None)
PROTECTED={'H-E022','H-E009','H-E011','H-E021','U-E006','S-E009','S-E017'}
def xy(p):return (p['x']*1448,(1-p['y'])*1086)
def norm(p):return {'x':round(p[0]/1448,9),'y':round(1-p[1]/1086,9)}
def length(pts):return sum(math.dist(a,b) for a,b in zip(pts,pts[1:]))
def make_rows():
 rows=[]
 for m in MODELS:
  c=next(c for c in CURRENT['cities'] if c['id']==m['cityId']);N={n['id']:n for n in m['nodes']}
  for reg in m['edges']:
   e=next(e for e in c['edges'] if e['id']==reg['SourceRouteKey']);key=reg['EdgeID'];start=xy(N[e['from']]['arrivalUnity']);end=xy(N[e['to']]['arrivalUnity']);cur=[start,*map(xy,e['waypoints']),end];old=[start,*map(xy,HIST[key]['OldWaypoints']),end]
   bends=[norm(p) for p in DRAFT[key]] if key in DRAFT else copy.deepcopy(e['waypoints'])
   pts=[start,*map(xy,bends),end]
   changed=bends!=e['waypoints']
   if key in PROTECTED:assert not changed,key
   status='WAYPOINT_CORRECTION' if changed else 'NEEDS_USER_DECISION' if key in NOTES else 'KEEP_CURRENT_GOOD' if key=='S-E017' else 'KEEP_CURRENT_NO_CHANGE'
   reason=REASON.get(key,NOTES.get(key,'Không có chỉ đạo/lý do đủ rõ để sửa; giữ nguyên. Chưa nghiệm thu terrain riêng từng cạnh.'))
   if key=='S-E017':reason='OWNER_APPROVED_KEEP: nhận xét mới nhất đã chốt ổn; không lấy nghi vấn nước cũ để sửa.'
   rows.append(dict(City=m['cityId'],EdgeID=key,From=e['from'],To=e['to'],SourceRouteKey=e['id'],Travel=e.get('travel') or 'walk',HistoricalLength=round(length(old),3),CurrentLength=round(length(cur),3),ProposedLength=round(length(pts),3),CurrentWaypoints=e['waypoints'],ProposedWaypoints=bends,CurrentPolylineUnity=[norm(p) for p in cur],ProposedPolylineUnity=[norm(p) for p in pts],HistoricalPolylineUnity=[norm(p) for p in old],Reason=reason,Priority='HIGH' if key in OWNER else 'LOW',TerrainConfidence=CONF.get(key,'OWNER_KEEP' if key=='S-E017' else 'LOW' if key in NOTES else 'NOT_INDIVIDUALLY_VERIFIED'),Status=status,OwnerComment=OWNER.get(key,''),Evidence='Details/'+m['cityId']+'-high-review.png',Changed=changed,Applied=False))
 assert len(rows)==155
 return rows

def render(model,rows,stage):
 import sys
 sys.path.insert(0,str(ROOT/'Tools'));import export_route_markup as layout
 layout.WIDTH,layout.HEIGHT=1448,1086
 source=Image.open(ROOT/model['sourcePNG']).convert('RGBA');assert source.size==(1448,1086)
 layer=Image.new('RGBA',source.size);d=ImageDraw.Draw(layer);labels=layout.Labels(d)
 font=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',12);nf=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',16)
 for r in rows:
  pts=[xy(p) for p in r['CurrentPolylineUnity' if stage=='Current' else 'ProposedPolylineUnity']]
  color=(220,47,53,235) if r['Status']=='NEEDS_USER_DECISION' else (23,111,220,240) if r['Changed'] and stage=='Proposed' else (18,141,80,210)
  d.line(pts,fill=(255,250,227,180),width=4);d.line(pts,fill=color,width=2)
 for n in model['nodes']:
  x,y=xy(n['positionUnity']);d.ellipse((x-5,y-5,x+5,y+5),fill='#FFD879',outline='#263D4B',width=1);a,b=xy(n['arrivalUnity']);d.polygon([(a,b-3),(a+3,b),(a,b+3),(a-3,b)],fill='#63E5E5')
  labels.add(n['id'],(x,y),nf,(20,37,47,255),'node',n['id'])
 for r in rows:labels.add(r['EdgeID'],layout.sample([xy(p) for p in r['CurrentPolylineUnity' if stage=='Current' else 'ProposedPolylineUnity']],.5),font,(23,51,80,255),'edge',r['EdgeID'])
 return Image.alpha_composite(source,layer).convert('RGB')

def main():
 parser=argparse.ArgumentParser();parser.add_argument('--apply',action='store_true');args=parser.parse_args();rows=make_rows();updated=copy.deepcopy(CURRENT)
 for r in rows:
  if r['Changed']:
   city=next(c for c in updated['cities'] if c['id']==r['City']);edge=next(e for e in city['edges'] if e['id']==r['SourceRouteKey']);edge['waypoints']=r['ProposedWaypoints']
 for a,b in zip(CURRENT['cities'],updated['cities']):
  assert a['nodes']==b['nodes']
  for ea,eb in zip(a['edges'],b['edges']):assert {k:v for k,v in ea.items() if k!='waypoints'}=={k:v for k,v in eb.items() if k!='waypoints'}
 assert len(updated['quests'])==29 and len(updated['puzzles'])==22 and len(updated['checkIns'])==18
 for r in rows:
  assert all(math.isfinite(v) and 0<=v<=1 for p in r['ProposedPolylineUnity'] for v in p.values())
 if args.apply:
  live=read(CONTENT);assert live==CURRENT or live==updated,'Concurrent source change: stop'
  if live!=updated:dump(CONTENT,updated)
  for r in rows:r['Applied']=r['Changed']
 dump(OUT/'waypoint-proposals.json',dict(phase='OWNER_AUTHORIZED_BEST_JUDGMENT_ROUTE_PATCH' if args.apply else 'PROPOSAL_NOT_APPLIED',coordinateSystem='Unity normalized0..1, bottom-left; pixel x=1448u,y=1086(1-v); bends exclude immutable endpoints',sourceSHA256=sha(CONTENT),rows=rows))
 with (OUT/'waypoint-proposals.csv').open('w',encoding='utf-8-sig',newline='') as f:
  keys=[k for k in rows[0] if 'PolylineUnity' not in k];w=csv.DictWriter(f,keys);w.writeheader();w.writerows({k:json.dumps(v,ensure_ascii=False) if isinstance(v,(list,dict)) else v for k,v in r.items() if k in keys} for r in rows)
 figures=[]
 for m in MODELS:
  sub=[r for r in rows if r['City']==m['cityId']];name=NAMES[m['cityId']]
  images=[]
  for stage in ['Current','Proposed']:
   im=render(m,sub,stage);im.save(OUT/f'{name}_{stage}.png');images.append(im)
  pair=Image.new('RGB',(2896,1086));pair.paste(images[0],(0,0));pair.paste(images[1],(1448,0));pair.save(OUT/f'{name}_Current_Proposed.png')
  # High-only image removes label clutter while preserving the same native origin.
  render(m,[r for r in sub if r['Priority']=='HIGH'],'Proposed').save(OUT/'Details'/f'{m["cityId"]}-high-review.png')
  figures.append(f'<section><h2>{html.escape(m["name"])}</h2><a href="{name}_Current_Proposed.png"><img src="{name}_Current_Proposed.png"></a><p><a href="{name}_Current.png">Current</a> · <a href="{name}_Proposed.png">Proposed / corrected overlay</a> · <a href="Details/{m["cityId"]}-high-review.png">HIGH only</a></p></section>')
 (OUT/'RouteReview.html').write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC route corrections</title><style>body{background:#163037;color:#f9e9c8;font:16px system-ui;margin:22px}img{max-width:100%}a{color:#a2e4e1}section{margin:30px 0}table{border-collapse:collapse}td,th{border:1px solid #5d777b;padding:6px}</style><h1>Current → corrected route review</h1><p>Canonical 1448×1086; technical overlay, not Player screenshot. Green=kept, blue=changed, red=retained pending decision. User authorized best judgment after the audit-only prompt; routes only. Terrain confidence is separate from movement PASS.</p><p><a href="waypoint-proposals.csv">155-edge CSV</a> · <a href="waypoint-proposals.json">JSON</a></p>'+''.join(figures)+'<table><tr><th>Edge</th><th>Status</th><th>Length current→new</th><th>Reason</th></tr>'+''.join(f'<tr><td>{r["EdgeID"]}</td><td>{r["Status"]}</td><td>{r["CurrentLength"]} → {r["ProposedLength"]}</td><td>{html.escape(r["Reason"])}</td></tr>' for r in rows if r['Priority']=='HIGH')+'</table></html>',encoding='utf-8')
 dump(DOC/'route-summary.json',dict(total=155,changed=sum(r['Changed'] for r in rows),retained=sum(not r['Changed'] for r in rows),needsDecision=[r['EdgeID'] for r in rows if r['Status']=='NEEDS_USER_DECISION'],protected=sorted(PROTECTED),applied=args.apply,sourceSHA256=sha(CONTENT)))
 print(json.dumps(read(DOC/'route-summary.json'),ensure_ascii=False))

if __name__=='__main__':main()
