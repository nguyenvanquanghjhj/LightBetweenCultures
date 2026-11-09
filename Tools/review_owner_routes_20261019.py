"""Prepare bounded Phase A review evidence; no production writes."""
from pathlib import Path
import json,html,csv,hashlib,math
from PIL import Image,ImageDraw,ImageFont
R=Path(__file__).resolve().parents[1];O=R/'Validation/OwnerRouteRedesign-20261019';D=R/'Docs/Maps/OwnerRouteRedesign-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def pix(p,t):return(p['x']*t['xScale']+t['xOffset'],p['y']*t['yScale']+t['yOffset'])
src=read(R/'Tools/SixCityRouteEditor/data/source.json');summary=read(O/'summary.json');access=read(O/'node-access-review.json')
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',15)
questions=[];cropdir=O/'review-crops';cropdir.mkdir(exist_ok=True)
def crop(im,box,filename,marks):
 box=tuple(int(x) for x in box);box=(max(0,box[0]),max(0,box[1]),min(im.width,box[2]),min(im.height,box[3]))
 pic=im.crop(box).resize(((box[2]-box[0])*2,(box[3]-box[1])*2),Image.Resampling.NEAREST);d=ImageDraw.Draw(pic)
 for p,text,col in marks:
  x=(p[0]-box[0])*2;y=(p[1]-box[1])*2
  d.ellipse((x-10,y-10,x+10,y+10),outline=col,width=3)
  d.text((x+13,y-25),text,font=font,fill=col,stroke_width=2,stroke_fill=(8,17,25))
 pic.save(cropdir/filename)
 return {'image':str((cropdir/filename).relative_to(R)),'cropBoxInputPixels':box,'scale':2,'interpolation':'nearest - inspection only, not runtime art'}
for c in src['cities']:
 cid=c['id'];j=read(O/f'{cid}-ink-strokes.json');t=j['transform'];im=Image.open(R/j['sourceImage']).convert('RGB')
 nodes=c['nodes']
 def nearby(point):
  candidates=[]
  for n in nodes:
   for k in ('arrival','position'):
    candidates.append({'node':n['id'],'anchor':k,'distancePixels':round(math.dist(point,pix(n[k],t)),2)})
  return sorted(candidates,key=lambda a:a['distancePixels'])[:4]
 for stroke in j['strokes']:
  if stroke['travel']!='ferry':continue
  pts=stroke['inputPixels'];a,b=pts[0],pts[-1];xs=[p[0] for p in pts];ys=[p[1] for p in pts]
  qid=f'R{len([q for q in questions if q["kind"]=="FERRY_TRANSITION"])+1}'
  details=crop(im,(min(xs)-75,min(ys)-65,max(xs)+75,max(ys)+65),f'{qid}-{cid}-ferry.png',[(a,'A',(255,240,130)),(b,'B',(255,240,130))])
  questions.append({'id':qid,'kind':'FERRY_TRANSITION','cityId':cid,'strokeId':stroke['strokeId'],
   'startInputPixel':a,'endInputPixel':b,'nearStart':nearby(a),'nearEnd':nearby(b),**details,
   'question':'Xác nhận A/B là điểm lên/xuống thuyền ở giữa tuyến; cần biết tuyến nối những node nào. Không tự gán cả cạnh thành thuyền hoặc đổi arrival.'})
 for row in access:
  if row['city']!=cid or row['status']=='NEAR_EXISTING_ARRIVAL':continue
  p=row['inputMarker'];a=row['inputArrival'];margin=45
  details=crop(im,(min(p[0],a[0])-margin,min(p[1],a[1])-margin,max(p[0],a[0])+margin,max(p[1],a[1])+margin),f'A-{cid}-{row["nodeId"]}.png',
   [(p,row['nodeId']+' marker',(255,247,220)),(a,'arrival',(255,215,64))])
  questions.append({'id':'A-'+row['nodeId'],'kind':'ARRIVAL_CONNECTOR','cityId':cid,'nodeId':row['nodeId'],**details,
   'distancePixels':row['distanceToArrivalPx'],
   'question':'Nét đi qua gần marker, nhưng điểm đứng đã khóa nằm lệch. Có cho phép một đoạn nối tiếp cận ngắn, giữ nguyên điểm đứng, hay cần tuyến khác?'})
# Observed examples requiring connectivity decisions. A crossing does not become
# a new gameplay node; these coordinates are review annotations only.
crossings=[('X1','hoi_an',(189,254),'Nhánh tây, giữa H1 / H10 / H21 / H9'),
 ('X3','jeonju',(530,420),'Nét gặp nhau phía tây J15 / J5, trên đường từ J16'),
 ('X4','gyeongju',(341,518),'Nét gặp nhau phía đông G0, gần lối đến G16')]
for qid,cid,p,title in crossings:
 j=read(O/f'{cid}-ink-strokes.json');im=Image.open(R/j['sourceImage']).convert('RGB')
 details=crop(im,(p[0]-85,p[1]-75,p[0]+85,p[1]+75),f'{qid}-{cid}-junction.png',[(p,qid,(255,215,64))])
 questions.append({'id':qid,'kind':'CONNECTIVITY_REVIEW','cityId':cid,'inputPixel':p,'title':title,**details,
  'question':'Nét chạm/cắt nhau ở đây có cho phép rẽ chuyển tuyến, hay chỉ đi theo từng nét mà không nối? Không tự xem giao điểm là giao lộ.'})
dump(O/'design-questions.json',questions)
with (O/'stroke-inventory.csv').open('w',encoding='utf-8-sig',newline='') as f:
 w=csv.writer(f);w.writerow(['City','StrokeID','Travel','PointCount','MapPoints','Status'])
 for c in src['cities']:
  for s in read(O/f'{c["id"]}-ink-strokes.json')['strokes']:
   w.writerow([c['id'],s['strokeId'],s['travel'],len(s['pointsMap0To1000']),json.dumps(s['pointsMap0To1000']),s['assignment']])
def url(p):return (R/p).resolve().as_uri()
cards=[]
for c in src['cities']:
 j=read(O/f'{c["id"]}-ink-strokes.json');preview=O/f'{c["id"]}-canonical-trace-preview.png'
 cards.append(f'<section><h2>{html.escape(c["name"])}</h2><p>Trái: ảnh chủ dự án. Phải: nét đã truy vết lên nền runtime gốc. Xanh ngọc = đi bộ, đỏ = thuyền; ô vàng = điểm đứng đã khóa. Đây chưa phải graph được duyệt.</p><div class="pair"><a href="{url(j["sourceImage"])}"><img src="{url(j["sourceImage"])}"></a><a href="{preview.as_uri()}"><img src="{preview.as_uri()}"></a></div></section>')
qcards=[]
for q in questions:
 description=q.get('title') or (f'{q["nodeId"]}: lệch arrival {q["distancePixels"]:.1f}px' if q['kind']=='ARRIVAL_CONNECTOR' else f'{q["strokeId"]}: A/B là hai đầu nét đỏ')
 qcards.append(f'<article id="{q["id"]}"><h3>{html.escape(q["id"]+" · "+q["cityId"])}</h3><p>{html.escape(description)}</p><a href="{url(q["image"])}"><img src="{url(q["image"])}"></a><p>{html.escape(q["question"])}</p></article>')
page='''<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC — Đối chiếu đường chủ dự án</title><style>
body{background:#10202a;color:#f6edda;font:16px/1.55 system-ui;margin:24px}h1,h2,h3{color:#f4cf81}.pair{display:grid;grid-template-columns:1fr 1fr;gap:12px}img{max-width:100%;height:auto}.questions{display:grid;grid-template-columns:repeat(auto-fit,minmax(340px,1fr));gap:20px}article,section{background:#19313a;padding:16px;border-radius:12px;margin:18px 0}article img{max-height:380px;object-fit:contain}a{color:#7de5c7} .notice{border:2px solid #e9b05e;padding:14px}</style>
<h1>Số hóa sáu bản vẽ — Phase A</h1><p class="notice">CHƯA ÁP DỤNG VÀO UNITY. Không nối tự động các giao điểm; không lấy graph 155 cạnh cũ để điền tuyến thiếu. Ảnh preview là bằng chứng đối chiếu, không phải asset runtime.</p>
<p>Đã đọc đủ sáu PNG trong ZIP. Bấm ảnh để xem kích thước gốc. Mã R = đoạn thuyền, A = chênh lệch marker/điểm đứng, X = giao điểm cần xác nhận. Không cần vẽ lại toàn bộ.</p>'''+''.join(cards)+'<h2>Các điểm cần chốt trước khi áp dụng</h2><div class="questions">'+''.join(qcards)+'</div></html>'
(D/'review.html').write_text(page,encoding='utf-8')
table='\n'.join(f'| {c["name"]} | {c["nodeCount"]} | {c["oldEdgeCount"]} | {c["tracedChains"]} | {c["pointCount"]} | {c["observedFerryPixels"]} |' for c in summary['cities'])
qtable='\n'.join(f'| {q["id"]} | {q["cityId"]} | {q["kind"]} | [Ảnh cận cảnh]({Path(R/q["image"]).as_uri()}) |' for q in questions)
report=f'''# Đại tu tuyến theo bản vẽ chủ dự án — Phase A, 2026-10-19

**NEEDS_OWNER_CLARIFICATION_BEFORE_PRODUCTION. Chưa áp dụng graph hoặc sửa thuật toán runtime.**

## Đầu vào và source thật

Đã mở đủ sáu PNG trong ZIP `C:\\Users\\admin\\OneDrive\\Pictures\\Screenshots\\Screenshot 2026-10-19 174731.zip`. Bản nguyên byte giữ tại `Validation/OwnerRouteRedesign-20261019/inputs`; SHA256 và kích thước trong input-manifest.json. Không import vào Assets. Thứ tự theo node ID: 174731 Hội An; 174736 Đà Lạt; 174741 Jeonju; 174745 Gyeongju; 174751 Seoul; 174630 Huế.

Source hiện tại xác nhận 133 node, 155 cạnh cũ, 29 quest, 22 puzzle definitions, 22 StoryInteractions, 12 leisure, 6 relic và 24 fragment. Không ép graph mới bằng 155 cạnh. Vị trí/arrival trong export được so trực tiếp với GameplayContent và HoiAnData mới nhất; record của toàn bộ 155 cạnh cũ khớp export. Sáu nền canonical giữ SHA256. Checkpoint baseline hiện hành: CheckpointAndroidAppIcon-20261019, trên base SixCityRouteEditor; không checkout hoặc rollback.

## Đã thực hiện

- Tách nét bút theo màu thực tế: xanh lam RGB(63,72,204), đỏ RGB(136,0,21). Không lấy màu node/sông/cầu/legend làm tuyến.
- Fit hệ tọa độ mỗi ảnh bằng các node canonical được nhận diện trực quan; chuẩn map 0–1000 gốc trên trái. Sai số raster khoảng vài pixel ở ảnh screenshot; không đổi tọa độ node hoặc arrival.
- Truy vết centerline, giản lược lệch tối đa 0,65px ở ảnh đầu vào, giữ uốn cong và không tự nối khoảng đứt. Tách nét tại junction pixel, **không tự coi junction là giao lộ runtime**.
- Xuất sáu preview trên đúng sáu Map Overview runtime nguyên gốc. Preview là file riêng trong Validation; không ghi đè ảnh nguồn.
- Xuất tọa độ chuỗi nét, kiểm tra khoảng cách tới marker/arrival, 5 ảnh nét đỏ A/B, 7 ảnh chênh lệch điểm đứng, 3 ảnh giao điểm. Sau xem cận cảnh đã bỏ X2 (nhánh thực tế đi qua D18) và X5 (không phải giao cắt riêng) khỏi câu hỏi, tránh hỏi lại những điểm có thể giải thích từ ảnh. Hai crop nháp đó không nằm trong gallery hiện hành.

| Thành phố | Node giữ nguyên | Cạnh cũ | Chuỗi nét quan sát | Điểm số hóa | Pixel nét đỏ |
|---|---:|---:|---:|---:|---:|
{table}

**Chuỗi nét không phải cạnh gameplay.** Chưa chốt số cạnh đi bộ/thuyền mới; mỗi nét đỏ có thể chỉ là một đoạn giữa cạnh nhiều phương tiện. Không tự biến hai đầu đoạn nét thành node mới. Xem `stroke-inventory.csv` và `*-ink-strokes.json` trong thư mục Validation cùng lượt.

## Cần chủ dự án xác nhận

1. **Hội An R1**: nét đỏ nối H0 marker trên nước với H0 arrival trên bến đất, tức hai đầu thuộc cùng một node. Giữ arrival hiện tại thì đoạn đỏ nằm ngoài tuyến tiếp cận H0, không thể tự gán thành cạnh nối hai node khác nhau. Cần chủ dự án chốt cách xử lý đoạn riêng này. R2 (Đà Lạt), R3/R4 (Gyeongju), R5 (Seoul) được xuất đủ để kiểm tra A/B chuyển phương tiện; không cần yêu cầu chủ dự án đánh Edge ID.
2. **Điểm đứng A-H10, A-J7, A-G6, A-G7, A-S0, A-S5, A-S8**: nét qua gần marker nhưng lệch điểm đứng. Đề xuất giữ nguyên arrival và thêm đoạn tiếp cận ngắn hợp địa hình từ nét vẽ; cần chủ dự án chấp nhận, không tự kéo thẳng xuyên vật cản.
3. **Giao điểm X1, X3, X4**: cần xác nhận cho rẽ tại nơi nét chạm/cắt hay chỉ đi dọc từng tuyến. Chưa thêm navigation junction hoặc node gameplay. Đây là các ví dụ nổi bật; sau khi chốt quy tắc phải rà toàn bộ junction còn lại, không tự coi mọi điểm khác là đã được duyệt.

| Mã | Thành phố | Vấn đề | Bằng chứng |
|---|---|---|---|
{qtable}

Mở [gallery review](review.html) để xem ảnh nguồn, preview và ảnh khoanh chi tiết. Không cần chủ dự án đánh lại Edge ID hoặc vẽ lại toàn bộ. Không áp dụng production trong khi còn quyết định mơ hồ, theo mục 7A của prompt mới.

## Bước Unity sau khi chốt — CHƯA THỰC HIỆN

- GameData.FindPath hiện dùng Queue/BFS theo số cạnh. Đổi sang Dijkstra theo tổng chiều dài polyline, tính theo tỷ lệ ảnh thật (không dùng khoảng cách node thẳng). Tôn trọng EdgeOpen, discovery, gate và chiều cạnh.
- RouteDefinition có bidirectional, nhưng GameData.Load/FindPath hiện bỏ qua chiều và coi hai chiều. Phải đưa chiều vào runtime cùng đợt thay graph.
- CityMap.Move đã sample CityRouteGeometry cho cả hình vẽ và di chuyển; giữ một nguồn hình học. Travel hiện chỉ có walk/ferry cho **cả cạnh**, chưa hỗ trợ đoạn đa phương tiện/điểm chuyển giữa cạnh. Có ferryVisual và bobbing tối thiểu để kế thừa; không coi icon thuyền này là một bộ sprite thuyền hoàn chỉnh mới.
- Chỉ sau khi đối chiếu xong: checkpoint tăng dần phần source bị tác động, áp dụng graph mới + weighted routing + phân đoạn thuyền, chạy Windows targeted QA và progression hai thứ tự. Không thay art/audio/quest/save hoặc node/arrival.

## QA đúng phạm vi hiện tại

PASS: ZIP đủ sáu PNG; source snapshot khớp runtime records và canonical node/arrival; sáu map SHA256 khớp; 18 file source/config/map có hash không đổi trước/sau Stage A.

PARTIAL: truy vết nét và chuyển hệ tọa độ đã có preview; connectivity và chuyển phương tiện chưa được duyệt. Không dùng khoảng cách tới nét để tuyên bố node đã tiếp cận được.

NOT_TESTED: graph mới trong Unity, Dijkstra runtime, thuyền nhiều đoạn, Windows build, progression, save/restart và Android UX. Chưa sửa production nên không build lại APK/Windows. Không tạo checkpoint gameplay mới khi source chưa đổi.

File thêm: hai tool offline `Tools/digitize_owner_routes_20261019.py`, `Tools/review_owner_routes_20261019.py`; bằng chứng trong Validation/OwnerRouteRedesign-20261019; báo cáo/gallery tại Docs/Maps/OwnerRouteRedesign-20261019. Không xóa ảnh/log/backup hoặc lịch sử đã có.

Dừng trước Stage B để chủ dự án chốt các điểm trên. Đây là bản số hóa nháp, không phải tuyên bố hoàn tất đại tu sáu thành phố.
'''
# Use local relative links in Markdown; the HTML itself uses native file URLs.
import re
report=re.sub(r'\(file:///D:/GAMEKKK/LightBetweenCultures/([^)]*)\)',lambda m:'(../../../'+m.group(1)+')',report)
(D/'REPORT.md').write_text(report,encoding='utf-8')
assert all(hashlib.sha256((R/e['path']).read_bytes()).hexdigest()==e['sha256'] for e in read(O/'production-baseline.json'))
print('Review:',D/'review.html');print('Questions:',len(questions));print('Total ink chains:',sum(c['tracedChains'] for c in summary['cities']));print('Total points:',sum(c['pointCount'] for c in summary['cities']))
