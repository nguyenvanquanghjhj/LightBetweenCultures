"""Gallery from actual Windows Player PNGs only; never renders substitute evidence."""
from pathlib import Path
import collections,csv,hashlib,html,json
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QA/LeisureActivitiesFullReview-20261019';OUT=ROOT/'Validation/LeisureActivitiesFullReview-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assessment=read(DOC/'visual-review.json');selection=read(DOC/'final-runs.json')
entries=read(ROOT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']
runs=[]
for run in sorted(set(selection.values())|{'player01','restart01'}):
    p=OUT/run/'smoke-results.txt';lines=p.read_text('utf-8-sig').splitlines()
    result='PASS' if 'ALL CHECKS PASSED' in lines else 'FAIL'
    runs.append(dict(run=run,result=result,assertions=sum(x.startswith('PASS:') for x in lines),path=str(p.relative_to(ROOT)),sha256=sha(p)))
assert all(x['result']=='PASS' for x in runs)
evidence=[];groups={};body='<p>Ảnh Windows Player mới của lượt này. Cả 12 hoạt động được mở từ node thật, tại 540×960 và 720×1280. Dải ảnh bên dưới chỉ crop vùng scene từ frame gốc để review; không tạo motion hoặc ảnh giả.</p>'
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',16)
for e in entries:
    node=e['nodeId'];run=selection[node];frames=read(OUT/run/'frames.json');review=assessment[node]
    body+=f'<section id="{node}"><h2>{html.escape(e["title"])} · {node}</h2><p class="status">{review["status"]}</p><p>{html.escape(review["review"])}</p>'
    for width,height in [(540,960),(720,1280)]:
        selected=[x for x in frames if x['node']==node and x['width']==width];assert len(selected)==5,(node,width,len(selected))
        motion=[x for x in selected if x['kind']=='motion'];assert len(motion)==3
        assert all(motion[i]['elapsed']<motion[i+1]['elapsed'] for i in [0,1])
        assert all(x['backdrop']==motion[0]['backdrop'] for x in motion)
        for f in selected:
            p=OUT/run/f['file'];assert Image.open(p).size==(width,height)
            evidence.append(dict(**f,run=run,sha256=sha(p),bytes=p.stat().st_size))
        strip=Image.new('RGB',(480*3,392),'#173a3c');d=ImageDraw.Draw(strip)
        for i,f in enumerate(motion):
            im=Image.open(OUT/run/f['file']).convert('RGB');w=width*.93*.89;h=w*.75;cx=f['backdrop']['x'];cy=height-f['backdrop']['y']
            crop=im.crop((round(cx-w/2),round(cy-h/2),round(cx+w/2),round(cy+h/2)));crop.thumbnail((480,360));strip.paste(crop,(i*480,0));d.text((i*480+7,366),f'{f["elapsed"]:.3f}s · {run}',font=font,fill='white')
        name=f'{node}-{width}-MotionStrip.png';strip.save(OUT/name)
        tag=f'{node}{width}';groups[tag]=[dict(src=run+'/'+f['file'],caption=f'{f["elapsed"]:.3f}s · {f["utc"]}') for f in motion]
        body+=f'<details {"open" if width==540 else ""}><summary>{width}×{height} · {run}</summary><p><a href="{run}/FULL-{node}-{width}-map.png">Mini prop trên map</a> · <a href="{run}/FULL-{node}-{width}-menu.png">Menu tại node</a> · <a href="{name}">Ba thời điểm cạnh nhau</a></p><a href="{name}"><img src="{name}" loading="lazy"></a><div class="viewer"><img class="screen" id="im{tag}" src="{groups[tag][0]["src"]}" loading="lazy"><p id="cap{tag}">{groups[tag][0]["caption"]}</p><button onclick="step(\'{tag}\',-1)">← Trước</button> <button onclick="step(\'{tag}\',1)">Tiếp →</button></div></details>'
    body+='</section>'
body+='<p>Không xuất MP4. Frame sequence có timestamp và transform metadata tại từng thư mục Player. Windows touch simulation được dùng; Android-device NOT_TESTED. ANIMATION_PASS là đánh giá nội bộ ở mức animation nhẹ, chờ chủ dự án nghiệm thu ngoại hình.</p>'
script='<script>const frames='+json.dumps(groups,ensure_ascii=False)+';const pos={};function step(id,d){pos[id]=((pos[id]||0)+d+frames[id].length)%frames[id].length;let f=frames[id][pos[id]];document.getElementById("im"+id).src=f.src;document.getElementById("cap"+id).textContent=f.caption;}</script>'
nav=' · '.join(f'<a href="#{e["nodeId"]}">{e["nodeId"]} {html.escape(e["title"])}</a>' for e in entries)
(OUT/'Gallery.html').write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC · 12 leisure activities</title><style>body{background:#173a3c;color:#fff1d2;font:17px system-ui;margin:24px;line-height:1.55;max-width:1500px}a{color:#9ee8de}img{max-width:100%}.screen{max-height:760px;width:auto}button,summary{cursor:pointer}button{padding:10px;background:#ffe7b3;border:0;border-radius:7px}section{margin:35px 0;padding-top:20px;border-top:1px solid #4c7371}.status{color:#f4c56d}details{padding:12px 0}</style><h1>Lumi · Tổng duyệt 12 hoạt động thư giãn</h1><nav>'+nav+'</nav>'+body+script+'</html>',encoding='utf-8')
assert len(evidence)==120
dump(DOC/'evidence.json',dict(runtime='WINDOWS_PLAYER_CURRENT_TASK',primaryPNGs=120,motionPNGs=72,resolutions=['540x960','720x1280'],entries=evidence))
dump(DOC/'run-summary.json',dict(windows='TARGETED_PASS',android='NOT_TESTED',runs=runs,assertions=sum(x['assertions'] for x in runs),selectedRuns=selection,video='NOT_PRODUCED_TIMESTAMPED_REAL_PNG_SEQUENCE_AVAILABLE',visualCounts=dict(collections.Counter(x['status'] for x in assessment.values())),scope='All 12 functional review, targeted visual rechecks after fixes, real restart. No full-story replay.'))
with (DOC/'activity-review.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.writer(f);w.writerow(['Node','Activity','Status','Review','ArtNeeded','FinalRun'])
    for e in entries:
        a=assessment[e['nodeId']];w.writerow([e['nodeId'],e['title'],a['status'],a['review'],a.get('artNeeded',''),selection[e['nodeId']]])
print('Gallery:',OUT/'Gallery.html');print('120 actual PNGs, 72 timestamped motion PNGs;',len(runs),'successful runs')
