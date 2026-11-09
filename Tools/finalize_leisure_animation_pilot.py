"""Build review gallery and exact evidence inventory from real Windows captures."""
from pathlib import Path
import csv,hashlib,html,json,math
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QA/LeisureAnimationPilot-20261019';OUT=ROOT/'Validation/LeisureAnimationPilot-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
run=OUT/'player04';frames=read(run/'animation-frames.json');summary=[]
for name in ['player01','player02','player03','player04','restart01']:
    p=OUT/name/'smoke-results.txt';lines=p.read_text(encoding='utf-8-sig').splitlines()
    summary.append(dict(run=name,result='PASS' if lines[-1]=='ALL CHECKS PASSED' else 'FAIL',assertions=sum(s.startswith('PASS:') for s in lines),path=str(p.relative_to(ROOT)),sha256=sha(p)))
assert all(next(r for r in summary if r['run']==n)['result']=='PASS' for n in ['player04','restart01'])
evidence=[]
for f in frames:
    p=run/f['file'];im=Image.open(p);assert im.size==(f['width'],f['height'])
    evidence.append(dict(**f,sha256=sha(p),bytes=p.stat().st_size))
motion=[f for f in frames if f['activity']];assert len(motion)==20
groups=[];body='<p>Ảnh Windows Player thật, bản cuối build03 / player04. Không mockup, không video dựng ngoài game. Chuỗi ảnh có UTC và thời gian animation; nút duyệt bên dưới chỉ hiển thị các frame gốc, không nội suy.</p>'
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18)
for node,activity,title in [('H9','swing','Hội An · Ngồi xích đu'),('U0','boat','Huế · Đạp thuyền vịt')]:
    for width in [540,720]:
        selected=[f for f in frames if f['activity']==activity and f['width']==width];assert len(selected)==5
        assert all(selected[i]['elapsed']<selected[i+1]['elapsed'] for i in range(4))
        background=[f['backdropWorld'] for f in selected];assert all(p==background[0] for p in background)
        if activity=='swing':assert max(f['angle'] for f in selected)>8 and min(f['angle'] for f in selected)<-8
        # Close-up QA crops only; source PNGs and full runtime frames remain untouched.
        strip=Image.new('RGB',(360*5,300),'#173a3c');draw=ImageDraw.Draw(strip)
        for i,f in enumerate(selected):
            im=Image.open(run/f['file']).convert('RGB');w=f['width']*.93*.89;h=w*.75;cx=f['backdropWorld']['x'];cy=f['height']-f['backdropWorld']['y']
            crop=im.crop((round(cx-w/2),round(cy-h/2),round(cx+w/2),round(cy+h/2)));crop.thumbnail((360,270));strip.paste(crop,(i*360,0))
            draw.text((i*360+6,273),f"{f['moment']} / {f['elapsed']:.3f}s",font=font,fill='#fff0cb')
        strip_name=f'{node}-{width}-MotionStrip.png';strip.save(OUT/strip_name)
        ident=f'{node}{width}';data=[dict(src='player04/'+f['file'],label=f"{f['moment']} · {f['elapsed']:.3f}s · {f['utc']}") for f in selected]
        body+=f'<section><h2>{html.escape(title)} — {width}×{selected[0]["height"]}</h2><p><a href="player04/Pilot-{node}-{width}-map.png">Mini prop tại node</a> · <a href="player04/Pilot-{node}-{width}-menu.png">Menu thật</a> · <a href="{strip_name}">Năm thời điểm cạnh nhau</a></p><a href="{strip_name}"><img src="{strip_name}"></a><div class="player" data-id="{ident}"><img id="im{ident}" class="screen" src="{data[0]["src"]}"><p id="caption{ident}">{html.escape(data[0]["label"])}</p><button onclick="step(\'{ident}\',-1)">← Frame trước</button> <button onclick="step(\'{ident}\',1)">Frame tiếp →</button><p>'+''.join(f'<a href="{v["src"]}">{i+1}</a> ' for i,v in enumerate(data))+'</p></div></section>'
        groups.append(dict(id=ident,frames=data))
body+='<section><h2>Đánh giá hình ảnh</h2><p>ANIMATION_PASS ở mức pilot: khung xích đu và background cố định; ghế/dây và Lumi phối hợp; thân trước thuyền che phần chân. Tư thế ngồi được tạo bằng occlusion từ sprite canonical, không có sprite chân đạp hay tay nắm dây riêng. Chủ dự án chưa duyệt final visual.</p><p>Không xuất MP4. Các frame gốc, timestamp và tọa độ transform được lưu tại <a href="player04/animation-frames.json">animation-frames.json</a>. Không dùng ảnh player02/player03 làm bằng chứng bố cục thuyền cuối.</p></section>'
script='<script>const groups='+json.dumps({g['id']:g['frames'] for g in groups},ensure_ascii=False)+';const index={};function step(id,d){index[id]=((index[id]||0)+d+groups[id].length)%groups[id].length;const f=groups[id][index[id]];document.getElementById("im"+id).src=f.src;document.getElementById("caption"+id).textContent=f.label;}</script>'
(OUT/'Gallery.html').write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>Lumi leisure animation pilot</title><style>body{background:#173a3c;color:#fff1d2;font:17px system-ui;margin:24px;line-height:1.55}a{color:#9ee8de}img{max-width:100%}.screen{max-height:800px;width:auto}button{padding:10px;background:#ffe7b3;border:0;border-radius:7px}section{margin:40px 0}a,button{cursor:pointer}</style><h1>Lumi · Hai hoạt động thư giãn</h1>'+body+script+'</html>',encoding='utf-8')
dump(DOC/'evidence.json',dict(sourceRun='player04',motionFrames=20,actualResolutions=['540x960','720x1280'],entries=evidence))
dump(DOC/'run-summary.json',dict(runs=summary,required=['player04','restart01'],windows='TARGETED_PASS',android='NOT_TESTED',video='NOT_PRODUCED_TIMESTAMPED_REAL_PNG_SEQUENCE_AVAILABLE',
    visual={'swing':'ANIMATION_PASS_PILOT_INTERNAL_REVIEW; OWNER_REVIEW_PENDING','boat':'ANIMATION_PASS_PILOT_INTERNAL_REVIEW; OWNER_REVIEW_PENDING'},
    knownFailures={'player01':'Earned main-story seed had not opened H9; discovery correctly blocked entry. Used earned all-sidequests source, did not change gates.',
                   'player02':'Hidden Windows MainWindowHandle=0; QA resize stayed at 720x960. Fixed only new pilot QA window lookup.',
                   'player03':'Functional PASS, boat composition was too low over foreground steps; retained diagnostics, final position adjusted in build03/player04.'}))
future=[
 ('H16','Thả hoa đăng','ANIMATION_NEEDS_TECH_FIX','Tách đèn thả thành lớp từ prop có sẵn; đặt trên mặt nước và phối hợp phản ứng Lumi.'),
 ('U8','Ngồi ngai vàng','ANIMATION_NEEDS_ART','Tư thế ngồi rõ đầu gối/chân, góc 3/4 tương ứng ngai; không tự thay canonical.'),
 ('D13','Hái dâu','ANIMATION_NEEDS_TECH_FIX','Gắn tay/điểm nhặt với từng quả và giỏ; đánh giá lại sau khi rig, không coi token bay là pose hoàn thiện.'),
 ('D9','Thả diều','ANIMATION_NEEDS_TECH_FIX','Tách diều/dây từ art đang có, dây gắn đúng tay; pose cầm dây cần review riêng.'),
 ('J13','Tưới cây','ANIMATION_NEEDS_ART','Pose cầm/nghiêng bình và tay; giữ khuôn mặt Lumi, không dùng hình đứng để giả tay rót.'),
 ('J15','Thưởng trà','ANIMATION_NEEDS_ART','Pose ngồi/cầm tách hoặc hai cử chỉ uống; layer bàn che thân dưới.'),
 ('G10','Đánh chuông','ANIMATION_NEEDS_TECH_FIX','Tách chày/chuông từ prop, pivot đúng điểm treo và phối hợp Lumi; cần review khả năng tay chạm.'),
 ('G11','Treo thẻ ước nguyện','ANIMATION_NEEDS_TECH_FIX','Tách thẻ từ art đang có, gắn lên giá qua thao tác; không tự thêm hoạt động mới.'),
 ('S1','Ăn tteokbokki','ANIMATION_NEEDS_ART','Pose tay/cầm đũa đưa thức ăn đến miệng; không chỉ di chuyển icon đồ ăn.'),
 ('S7','Nhảy K-pop','ANIMATION_NEEDS_ART','Ít nhất vài key pose toàn thân có tay/chân khác nhau; không coi lắc sprite đứng là dance hoàn thiện.')]
with (DOC/'future-feasibility.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.writer(f);w.writerow(['Node','Activity','Classification','RequiredWork']);w.writerows(future)
dump(DOC/'future-feasibility.json',dict(scope='Assessment only; not implemented/tested in this pilot. Existing previous prototypes preserved unchanged.',entries=[dict(node=x[0],activity=x[1],status=x[2],requiredWork=x[3],runtime='NOT_TESTED_BY_THIS_PILOT') for x in future]))
print('Final gallery:',OUT/'Gallery.html');print('Runtime frames:',len(frames),'including',len(motion),'motion frames')
