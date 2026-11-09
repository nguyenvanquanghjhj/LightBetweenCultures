"""Technical overlays on original canonical pixels; never writes runtime art."""
import copy, hashlib, html, json, math, sys
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/Maps/AppliedRouteReferences-20261019'
OUT=ROOT/'Validation/AppliedRouteReferences-20261019'
SOURCE='Assets/_Game/Resources/Content/GameplayContent.json'
baseline=json.loads((DOC/'baseline.json').read_text(encoding='utf-8-sig'))
before=json.loads((Path(baseline['before'])/SOURCE).read_text(encoding='utf-8-sig'))
after=json.loads((ROOT/SOURCE).read_text(encoding='utf-8-sig'))
models=json.loads((ROOT/'Validation/RouteMarkupExport-20261018/edge_mapping.json').read_text(encoding='utf-8-sig'))['cities']
sys.path.insert(0,str(ROOT/'Tools'))
import export_route_markup as layout
layout.WIDTH,layout.HEIGHT=1448,1086
def xy(p):return (p['x']*1448,(1-p['y'])*1086)
def draw(model,data):
    canvas=Image.open(ROOT/model['sourcePNG']).convert('RGBA')
    overlay=Image.new('RGBA',canvas.size);d=ImageDraw.Draw(overlay);labels=layout.Labels(d)
    font=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',12);nodefont=ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf',17)
    nodes={n['id']:n for n in model['nodes']};polylines=[]
    for ex in model['edges']:
        edge=next(e for e in data['edges'] if e['id']==ex['SourceRouteKey'])
        pts=[xy(nodes[edge['from']]['arrivalUnity']),*[xy(p) for p in edge['waypoints']],xy(nodes[edge['to']]['arrivalUnity'])]
        color=(108,48,157,230) if edge.get('travel')=='ferry' else (18,99,190,240)
        d.line(pts,fill=(255,251,222,195),width=4);d.line(pts,fill=color,width=2)
        polylines.append((ex,pts,color))
    for n in model['nodes']:
        x,y=xy(n['positionUnity']);d.ellipse((x-5,y-5,x+5,y+5),fill='#FFD875',outline='#192D40',width=1)
        a,b=xy(n['arrivalUnity'])
        if math.dist((x,y),(a,b))>2:
            d.line([(x,y),(a,b)],fill=(24,120,130,180),width=1);d.polygon([(a,b-3),(a+3,b),(a,b+3),(a-3,b)],fill='#6BFFFF')
        labels.add(n['id'],(x,y),nodefont,(15,37,53,255),'node',n['id'])
    for ex,pts,color in polylines:
        labels.add(ex['EdgeID'],layout.sample(pts,.5),font,(22,59,111,255),'edge',ex['EdgeID'])
    return Image.alpha_composite(canvas,overlay).convert('RGB')

figures=[];newmodels=[]
for model in models:
    cid=model['cityId'];name=model['nativePNG'].split('/')[1].split('_MapA')[0]
    for stage,content in [('Before',before),('After',after)]:
        city=next(c for c in content['cities'] if c['id']==cid)
        image=draw(model,city);image.save(OUT/f'{name}_{stage}_Routes.png')
    m=copy.deepcopy(model);m['nativePNG']='../../'+model['sourcePNG']
    city=next(c for c in after['cities'] if c['id']==cid)
    for ex in m['edges']:
        edge=next(e for e in city['edges'] if e['id']==ex['SourceRouteKey']);nodes={n['id']:n for n in m['nodes']}
        ex['pointsUnity']=[nodes[edge['from']]['arrivalUnity'],*edge['waypoints'],nodes[edge['to']]['arrivalUnity']]
    newmodels.append(m)
    figures.append(f'<section><h2>{html.escape(model["name"])}</h2><div class="pair"><figure><a href="{name}_Before_Routes.png"><img src="{name}_Before_Routes.png"></a><figcaption>Trước</figcaption></figure><figure><a href="{name}_After_Routes.png"><img src="{name}_After_Routes.png"></a><figcaption>Sau — waypoint source thật</figcaption></figure></div></section>')
layout.browser(OUT,newmodels)
(OUT/'BeforeAfter.html').write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC approved routes before/after</title><style>body{font:16px system-ui;background:#152b35;color:#fce8bf;margin:20px}.pair{display:grid;grid-template-columns:1fr 1fr;gap:12px}figure{margin:0}img{width:100%}a{color:#95d7eb}</style><h1>Sáu tuyến mới — bản vẽ kỹ thuật trên nền canonical</h1><p>Ảnh 1448×1086 native, cùng camera/toạ độ/nhãn. Không ảnh AI/asset runtime mới. Xanh lam: walk; tím: ferry. Chấm vàng: canonical node, hình thoi cyan: arrival cố định. Đây là overlay offline, không phải screenshot Player. Các ngoại lệ nước/dốc đã được người dùng cho phép best-effort; chưa phải terrain PASS toàn bộ.</p><p><a href="RouteReview.html">Chọn từng Edge ID / waypoint</a></p>'+''.join(figures)+'</html>',encoding='utf-8')
(OUT/'README.md').write_text('''# Approved route references — implementation review

Six `*_After_Routes.png` are full native 1448×1086 canonical maps with actual new
runtime polylines. `*_Before_Routes.png` use exactly the same images, anchors,
scale and drawing style, with geometry from BeforeAppliedRouteReferences-20261019.
These are technical plots, not Player screenshots and never runtime assets.
Open BeforeAfter.html for comparisons; RouteReview.html isolates any of the
155 unchanged export Edge IDs and shows each W0..Wn. W0/Wn are fixed arrivals.
Map1000 top-left: X=1000*u, Y=1000*(1-v); pixel=(1448*X/1000,1086*Y/1000).
The original EdgeID registry remains at ../RouteMarkupExport-20261018/.
The approved AI references remain outside Assets in ../SixRouteReferences-20261018/References/.
No graphic, source image, marker style or rendering colour was changed in game.
''',encoding='utf-8')
print('Exported 6 after + 6 before native canonical overlays; 155 stable Edge IDs; interactive per-edge view.')
