"""Read-only engineering export. Writes only a new Validation export directory.

Matches GameData.Load / CityRouteGeometry.Points; never executes route authoring
tools, changes discovery, invokes Unity, or writes production assets or saves.
"""
import argparse
import csv
import hashlib
import html
import json
import math
import re
import shutil
from datetime import datetime, timezone
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
CONTENT = 'Assets/_Game/Resources/Content/GameplayContent.json'
LEGACY = 'Assets/_Game/Resources/HoiAnData.json'
REFERENCE = 'Validation/MapNpcMarkers-20261018/fresh02/npc-fresh-final.json'
ORDER = ['hoi_an', 'hue', 'da_lat', 'jeonju', 'gyeongju', 'seoul']
PREFIX = dict(zip(ORDER, 'HUDJGS'))
NAMES = dict(zip(ORDER, ['HoiAn', 'Hue', 'DaLat', 'Jeonju', 'Gyeongju', 'Seoul']))
WIDTH, HEIGHT = 3840, 2880


def load(path): return json.loads(path.read_text(encoding='utf-8-sig'))
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def dump(path, value): path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
def pixel(p): return (p['x'] * WIDTH, (1 - p['y']) * HEIGHT)
def map1000(p): return {'x': round(p['x'] * 1000, 6), 'y': round((1 - p['y']) * 1000, 6)}
def natural(node): return (node[0], int(node[1:]))


class Labels:
    def __init__(self, draw):
        self.draw, self.boxes, self.records = draw, [], []

    def add(self, text, anchor, font, fill, kind, key):
        bounds = self.draw.textbbox((0, 0), text, font=font)
        w, h = bounds[2] + 12, bounds[3] - bounds[1] + 10
        candidates = []
        # Leaders identify labels, never replace or displace route geometry.
        for radius in [18, 38, 65, 100, 150, 220, 300]:
            for i in range(16):
                angle = i * math.pi / 8
                cx, cy = anchor[0] + math.cos(angle) * radius, anchor[1] + math.sin(angle) * radius
                x = max(3, min(WIDTH - w - 3, cx - w / 2))
                y = max(3, min(HEIGHT - h - 3, cy - h / 2))
                box = (x, y, x + w, y + h)
                overlap = sum(max(0, min(box[2], b[2]) - max(box[0], b[0])) * max(0, min(box[3], b[3]) - max(box[1], b[1])) for b in self.boxes)
                candidates.append((overlap * 10000 + radius, box))
            if any(score < 1000 for score, _ in candidates): break
        _, box = min(candidates, key=lambda c: c[0])
        self.boxes.append(box)
        cx, cy = (box[0] + box[2]) / 2, (box[1] + box[3]) / 2
        self.draw.line([anchor, (cx, cy)], fill=(63, 75, 92, 200), width=1)
        self.draw.rounded_rectangle(box, radius=5, fill=(255, 253, 242, 232), outline=fill, width=1)
        self.draw.text((box[0] + 6, box[1] + 5 - bounds[1]), text, font=font, fill=fill)
        self.records.append({'kind': kind, 'key': key, 'anchorPNG': list(anchor), 'labelRectPNG': list(box)})


def sample(points, fraction):
    lengths = [math.dist(a, b) for a, b in zip(points, points[1:])]
    remaining = sum(lengths) * fraction
    for a, b, length in zip(points, points[1:], lengths):
        if remaining <= length and length > 1e-10:
            t = remaining / length
            return (a[0] + t * (b[0] - a[0]), a[1] + t * (b[1] - a[1]))
        remaining -= length
    return points[-1]


def dashed(draw, points, color, width=3):
    # Continuous arclength dash phase across bends; points themselves unchanged.
    distance = 0.0
    for a, b in zip(points, points[1:]):
        length = math.dist(a, b)
        if length < 1e-10: continue
        at = 0.0
        while at < length:
            phase = distance % 23
            step = min(length - at, (13 if phase < 13 else 23) - phase)
            if step < 1e-8: step = min(length - at, .0001)
            if phase < 13:
                p = lambda t: (a[0] + (b[0] - a[0]) * t / length, a[1] + (b[1] - a[1]) * t / length)
                draw.line([p(at), p(at + step)], fill=color, width=width)
            at += step
            distance += step


def render(city, background, detail=False):
    image = background.resize((WIDTH, HEIGHT), Image.Resampling.LANCZOS).convert('RGBA')
    overlay = Image.new('RGBA', image.size)
    d = ImageDraw.Draw(overlay)
    fonts = {size: ImageFont.truetype('C:/Windows/Fonts/segoeuib.ttf', size) for size in [17, 23, 30]}
    labels = Labels(d)
    for edge in city['edges']:
        points = [pixel(p) for p in edge['pointsUnity']]
        color = (89, 54, 156, 245) if edge['travel'] == 'ferry' else (21, 83, 154, 245) if edge['referenceRouteVisible'] else (81, 91, 108, 230)
        d.line(points, fill=(255, 251, 233, 165), width=6)
        if edge['referenceRouteVisible']: d.line(points, fill=color, width=3)
        else: dashed(d, points, color)
        for p in points:
            d.ellipse((p[0] - 3, p[1] - 3, p[0] + 3, p[1] + 3), fill='white', outline=color, width=1)
    # Circles stay at canonical icon anchors. Diamonds stay at real arrivals.
    for node in city['nodes']:
        p = pixel(node['positionUnity'])
        fill = (255, 210, 106, 255) if node['referenceVisible'] else (238, 238, 243, 255)
        d.ellipse((p[0]-13, p[1]-13, p[0]+13, p[1]+13), fill=fill, outline=(28, 45, 63, 255), width=3)
        a = pixel(node['arrivalUnity'])
        if node['positionUnity'] != node['arrivalUnity']:
            d.line([p, a], fill=(60, 120, 156, 160), width=1)
            d.polygon([(a[0], a[1]-7), (a[0]+7, a[1]), (a[0], a[1]+7), (a[0]-7, a[1])], fill=(204, 241, 255, 255), outline=(20, 84, 123, 255))
        labels.add(node['id'], p, fonts[30], (15, 36, 54, 255), 'node', node['id'])
    for edge in city['edges']:
        p = sample([pixel(p) for p in edge['pointsUnity']], .5)
        labels.add(edge['EdgeID'], p, fonts[23], (19, 58, 104, 255), 'edge', edge['EdgeID'])
    if detail:
        grouped = {}
        for edge in city['edges']:
            for index, p in enumerate(edge['pointsUnity']):
                # W0 and final W are arrival anchors; label shared endpoints once.
                text = (f"A-{edge['FromNode']}" if index == 0 else f"A-{edge['ToNode']}" if index == len(edge['pointsUnity']) - 1 else edge['EdgeID'].split('-')[1] + f'.W{index}')
                key = tuple(pixel(p))
                grouped.setdefault(key, set()).add(text)
        for p, texts in grouped.items():
            labels.add(' / '.join(sorted(texts)), p, fonts[17], (40, 67, 101, 255), 'waypoint', ' / '.join(sorted(texts)))
    return Image.alpha_composite(image, overlay).convert('RGB'), labels.records


def browser(out, cities):
    data = json.dumps(cities, ensure_ascii=False).replace('</', '<\\/')
    text = '''<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC Route Markup Review</title>
<style>body{font:16px system-ui;background:#182c39;color:#fff5d9;margin:16px}select{font:inherit;padding:6px;max-width:65vw}#bar{position:sticky;top:0;background:#182c39;padding:10px;z-index:1}svg{width:100%;background:white}#info{white-space:pre-wrap}a{color:#a2d4ff}</style>
<div id="bar"><select id="city"></select> <select id="edge"></select><label><input id="waypoints" type="checkbox" checked>Waypoint của tuyến đang chọn</label><div id="info"></div></div><svg id="map" viewBox="0 0 3840 2880"></svg>
<script>const data=DATA, city=document.querySelector('#city'), edge=document.querySelector('#edge'), svg=document.querySelector('#map');
for(const c of data)city.add(new Option(c.name,c.cityId));
const pos=p=>[p.x*3840,(1-p.y)*2880];
function elem(tag,attrs,text){const e=document.createElementNS('http://www.w3.org/2000/svg',tag);for(const [k,v] of Object.entries(attrs))e.setAttribute(k,v);if(text)e.textContent=text;svg.append(e);return e;}
function label(t,p,size=27){elem('text',{x:p[0]+9,y:p[1]-9,'font-size':size,'font-family':'Segoe UI','font-weight':700,fill:'#13283e',stroke:'#fff9e8','stroke-width':5,'paint-order':'stroke'},t);}
function draw(){const c=data.find(c=>c.cityId===city.value);svg.replaceChildren();elem('image',{href:c.nativePNG,x:0,y:0,width:3840,height:2880});
for(const e of c.edges){const selected=edge.value===e.EdgeID,opacity=edge.value&&!selected?.14:1;elem('polyline',{points:e.pointsUnity.map(pos).map(p=>p.join(',')).join(' '),fill:'none',stroke:e.travel==='ferry'?'#6940ab':'#19578f','stroke-width':selected?6:3,'stroke-dasharray':e.referenceRouteVisible?'':'13 10',opacity});
if(!edge.value)label(e.EdgeID,pos(e.pointsUnity[Math.floor(e.pointsUnity.length/2)]),23);
if(selected&&document.querySelector('#waypoints').checked)e.pointsUnity.forEach((p,i)=>{let q=pos(p);elem('circle',{cx:q[0],cy:q[1],r:6,fill:'#fff',stroke:'#174b83','stroke-width':2});label('W'+i,q,25);});}
for(const n of c.nodes){let p=pos(n.positionUnity);elem('circle',{cx:p[0],cy:p[1],r:12,fill:n.referenceVisible?'#ffd16a':'#eee',stroke:'#173746','stroke-width':3});label(n.id,p,31);if(n.hasArrivalPosition){p=pos(n.arrivalUnity);elem('path',{d:`M ${p[0]} ${p[1]-8} l 8 8 -8 8 -8 -8 z`,fill:'#cef1ff',stroke:'#175383','stroke-width':2});}}
const e=c.edges.find(e=>e.EdgeID===edge.value);document.querySelector('#info').textContent=c.name+' | reference stage '+c.referenceStage+' | World Map '+(c.referenceWorldUnlocked?'đã mở':'chưa mở')+(e?' | '+e.EdgeID+': '+e.FromNode+' ↔ '+e.ToNode+' | source '+e.SourceRouteKey+' | '+e.travel+' | '+(e.referenceRouteVisible?'tuyến mở':'chưa mở'):' | Chọn một tuyến để đọc rõ W0…Wn. PNG tổng quan/chi tiết nằm trong cùng thư mục.');}
function change(){const c=data.find(c=>c.cityId===city.value);edge.replaceChildren(new Option('Tất cả tuyến',''));for(const e of c.edges)edge.add(new Option(e.EdgeID+' : '+e.FromNode+' ↔ '+e.ToNode,e.EdgeID));draw();}city.onchange=change;edge.onchange=draw;document.querySelector('#waypoints').onchange=draw;change();</script></html>'''
    (out/'RouteReview.html').write_text(text.replace('DATA', data), encoding='utf-8')


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', default='Validation/RouteMarkupExport-20261018')
    parser.add_argument('--reuse-edge-registry', type=Path)
    args = parser.parse_args()
    out = (ROOT / args.output).resolve()
    assert out.is_relative_to(ROOT / 'Validation') and not out.exists(), 'Only a NEW Validation directory is allowed'
    content = load(ROOT / CONTENT)
    legacy = load(ROOT / LEGACY)
    reference = load(ROOT / REFERENCE)
    bindings = load(ROOT / 'Docs/Art/native-art-bindings.json')['entries']
    receipt = load(ROOT / 'Docs/QA/MapNpcMarkers-20261018/checkpoint.json')
    checkpoint = Path(receipt['checkpoint']) / 'CHECKPOINT_MANIFEST.json'
    assert sha(checkpoint) == receipt['manifestSHA256']
    latest = load(checkpoint)
    for e in latest['entries']:
        if e['path'].startswith('Assets/') and e['path'].endswith(('.cs', '.asset', '.json')):
            assert sha(ROOT / e['path']) == e['sha256'], 'Source changed since latest checkpoint: ' + e['path']
    paths = [CONTENT, LEGACY, REFERENCE, 'Assets/_Game/Resources/AdventureUIArt.asset', 'Assets/_Game/Resources/DesignKitArt.asset',
             'Assets/_Game/Resources/Content/AdventurePresentation.json', 'Assets/_Game/Resources/Content/CulturalRelics.json',
             'Assets/_Game/Resources/Content/HoiAnLighting.json', 'Assets/_Game/Scripts/Core/GameData.cs',
             'Assets/_Game/Scripts/Maps/CityRouteGeometry.cs', 'Assets/_Game/Scripts/Maps/CityMapPaths.cs',
             'Assets/_Game/Scripts/Maps/CityMap.cs', 'Assets/_Game/Scripts/Maps/MapDiscoveryManager.cs',
             'Assets/_Game/Scripts/Core/SaveSystem.cs', 'ProjectSettings/ProjectSettings.asset']
    previous = load(args.reuse_edge_registry) if args.reuse_edge_registry else {}
    registry, cities = {}, []
    for cid in ORDER:
        source = next(c for c in content['cities'] if c['id'] == cid)
        binding = next(b for b in bindings if b['id'] == cid and b['role'] == ('city_main' if cid == 'hoi_an' else 'city_preview'))
        image = ROOT / binding['path']
        original = ROOT / 'LBC_AnhMoi_BanGiaoCodex_20261015' / source['sourceBackground']
        a, b = Image.open(image).convert('RGB'), Image.open(original).convert('RGB')
        assert a.size == b.size and a.tobytes() == b.tobytes(), 'Runtime MapA pixels differ from canonical source'
        assert a.size[0] / a.size[1] == WIDTH / HEIGHT
        assert binding['guid'] in (ROOT / (binding['path'] + '.meta')).read_text(encoding='utf-8-sig')
        paths += [binding['path'], binding['path'] + '.meta', original.relative_to(ROOT).as_posix()]
        nodes = []
        source_nodes = list(legacy['nodes']) + [n for n in source['nodes'] if n['id'] not in {a['id'] for a in legacy['nodes']}] if cid == 'hoi_an' else source['nodes']
        stage = next((p['stage'] for p in reference['cityProgress'] if p['cityId'] == cid), 0)
        history = reference['visited'] if reference['cityId'] == cid else next((p['visited'] for p in reference['cityProgress'] if p['cityId'] == cid), [])
        flags = set(reference['flags'])
        for i, n in enumerate(source_nodes):
            record = next(r for r in source['nodes'] if r['id'] == n['id'])
            pos = n['position']
            arrival = record['arrivalPosition'] if record.get('hasArrivalPosition', False) else pos
            retained = record.get('revealWhenPreviouslyVisited', False) and i in history
            available = not source.get('runtimeNodeIds') or n['id'] in source['runtimeNodeIds']
            visible = available and (record.get('revealStage', 0) <= stage or retained) and all(f in flags for f in record.get('requiredFlags', []))
            nodes.append({'id': n['id'], 'name': n['name'], 'runtimeIndex': i, 'positionUnity': pos, 'positionMap1000TL': map1000(pos),
                          'hasArrivalPosition': record.get('hasArrivalPosition', False), 'arrivalUnity': arrival, 'arrivalMap1000TL': map1000(arrival),
                          'revealStage': record.get('revealStage', 0), 'referenceVisible': visible})
        by_id = {n['id']: n for n in nodes}
        assert len(by_id) == len(nodes)
        keys = [e['id'] for e in source['edges']]
        assert len(set(keys)) == len(keys) and all(keys)
        used = [int(v.split('-E')[1]) for k, v in previous.items() if k.startswith(cid + ':')]
        next_id = max(used, default=0) + 1
        ids = {}
        for e in sorted(source['edges'], key=lambda e: (natural(e['from']), natural(e['to']), e['id'])):
            key = cid + ':' + e['id']
            ids[key] = previous.get(key) or PREFIX[cid] + '-E' + str(next_id).zfill(3)
            if key not in previous: next_id += 1
        registry.update(ids)
        edges = []
        for i, e in enumerate(source['edges']):
            start, end = by_id[e['from']], by_id[e['to']]
            points = [start['arrivalUnity']] + e.get('waypoints', []) + [end['arrivalUnity']]
            assert len(points) == len(e.get('waypoints', [])) + 2
            assert all(0 <= p['x'] <= 1 and 0 <= p['y'] <= 1 for p in points)
            edges.append({'City': cid, 'EdgeID': ids[cid + ':' + e['id']], 'FromNode': e['from'], 'ToNode': e['to'],
                          'CurrentWaypoints': [map1000(p) for p in points], 'SourceRouteKey': e['id'],
                          'sourceJsonPointer': f"/cities/{content['cities'].index(source)}/edges/{i}",
                          'pointsUnity': points, 'sourceWaypointsUnity': e.get('waypoints', []),
                          'pointLabels': ['W' + str(k) for k in range(len(points))],
                          'sourceWaypointIndices': [None] + list(range(len(points)-2)) + [None],
                          'travel': e.get('travel') or 'walk', 'bidirectional': True,
                          'sourceBidirectional': e.get('bidirectional', True), 'sourceStatus': e.get('status', ''),
                          'requiredFlags': e.get('requiredFlags', []),
                          'referenceRouteVisible': start['referenceVisible'] and end['referenceVisible'] and all(f in flags for f in e.get('requiredFlags', []))})
        cities.append({'cityId': cid, 'name': source['name'], 'nativePNG': 'Native/' + NAMES[cid] + '_MapA_native.png',
                       'sourcePNG': binding['path'], 'canonicalSourcePNG': original.relative_to(ROOT).as_posix(),
                       'spriteGUID': binding['guid'], 'spriteLocalID': binding['localId'], 'sourceSize': list(a.size),
                       'exportSize': [WIDTH, HEIGHT], 'referenceStage': stage,
                       'referenceWorldUnlocked': cid == 'hoi_an' or all(f in flags for f in source.get('unlockFlags', [])),
                       'nodes': nodes, 'edges': edges, 'existingCoveredWalkways': source.get('coveredWalkways', [])})
    paths += [p.relative_to(ROOT).as_posix() for p in (ROOT/'Assets/_Game/Audio/Music').glob('*.mp3*')]
    before = {p: sha(ROOT / p) for p in sorted(set(paths))}
    out.mkdir(parents=True)
    (out/'Native').mkdir()
    csv_rows, point_rows, layout, proofs = [], [], {}, []
    for city in cities:
        name = NAMES[city['cityId']]
        image = Image.open(ROOT / city['sourcePNG']).convert('RGB')
        shutil.copy2(ROOT / city['sourcePNG'], out / city['nativePNG'])
        for detail in [False, True]:
            result, labels = render(city, image, detail)
            filename = name + ('_Waypoints_3840.png' if detail else '_Routes_3840.png')
            result.save(out/filename, compress_level=6)
            layout[filename] = {'size': [WIDTH, HEIGHT], 'labels': labels}
            assert len([l for l in labels if l['kind'] == 'node']) == len(city['nodes'])
            assert len([l for l in labels if l['kind'] == 'edge']) == len(city['edges'])
        for edge in city['edges']:
            csv_rows.append({k: json.dumps(edge[k], separators=(',', ':')) if k == 'CurrentWaypoints' else edge[k] for k in ['City', 'EdgeID', 'FromNode', 'ToNode', 'CurrentWaypoints', 'SourceRouteKey']})
            for i, p in enumerate(edge['pointsUnity']):
                q = map1000(p); x, y = pixel(p)
                assert abs(q['x']/1000*WIDTH-x) < .00001 and abs(q['y']/1000*HEIGHT-y) < .00001
                point_rows.append({'City': city['cityId'], 'EdgeID': edge['EdgeID'], 'Waypoint': 'W'+str(i),
                                   'SourceWaypointIndex': edge['sourceWaypointIndices'][i], 'X1000': q['x'], 'Y1000': q['y'],
                                   'XUnity': p['x'], 'YUnity': p['y'], 'XPNG3840': round(x, 6), 'YPNG2880': round(y, 6)})
        proofs.append({'cityId': city['cityId'], 'nodes': len(city['nodes']), 'edges': len(city['edges']),
                       'intermediateWaypoints': sum(len(e['sourceWaypointsUnity']) for e in city['edges']),
                       'canonicalPixelsMatch': True, 'allNodeAndEdgeLabelsExported': True, 'allPolylineVerticesPreserved': True,
                       'referenceStage': city['referenceStage'], 'visibleRoutesInReference': sum(e['referenceRouteVisible'] for e in city['edges'])})
        print(str(out / (name + '_Routes_3840.png')))
    for filename, rows in [('edge_mapping.csv', csv_rows), ('waypoint_mapping.csv', point_rows)]:
        with (out/filename).open('w', encoding='utf-8-sig', newline='') as f:
            writer = csv.DictWriter(f, fieldnames=list(rows[0])); writer.writeheader(); writer.writerows(rows)
    dump(out/'edge_mapping.json', {'coordinateSystem': '0..1000; top-left; x right; y down',
         'pointOrder': 'W0=from arrival, W1..W(n-1)=all original bends, Wn=to arrival', 'cities': cities})
    dump(out/'edge-id-registry.json', registry)
    dump(out/'label-layout.json', layout)
    browser(out, cities)
    (out/'README.md').write_text('''# Sáu map để vẽ chỉnh đường — EXPORT ONLY

Mở `*_Routes_3840.png` trong Paint. Mỗi ảnh 3840×2880, toàn bản đồ, không crop/lề/đổi tỷ lệ. `*_Waypoints_3840.png` cùng kích thước và tọa độ, thêm nhãn điểm; dùng khi cần đối chiếu. `Native/` là sáu PNG nền nguyên bản 1448×1086, pixel khớp Map Overview A canonical. Canvas 3840 chỉ phóng Lanczos để nhãn và nét Paint dễ đọc, KHÔNG có thêm chi tiết tranh hay AI.

## Cách đọc

- Vòng tròn + H0/U3/... = vị trí icon node canonical. Hình thoi xanh lam nhạt = arrival thật của Lumi; đường dẫn mảnh từ node tới thoi CHỈ là chỉ dẫn, không phải cạnh mới.
- Đường xanh lam liền = route đã mở trong save tham chiếu. Xám đứt = chưa mở/hidden/thiếu flag. Tím = tuyến đò đã có (liền/đứt vẫn theo trạng thái). Tất cả 155 cạnh đều được xuất dù runtime chỉ vẽ đường gần Lumi. Không coi mọi nét là đi bộ.
- Dấu trắng nhỏ trên tuyến = vertex thật, không dịch tuyến để tránh trùng. Nhãn Edge ID có leader mảnh tới chính tuyến đó, không phải đường mới. Các tuyến chồng nhau vẫn giữ nguyên; không offset geometry cho đẹp.
- `W0` = arrival đầu; `W1` = source `waypoints[0]`; tiếp tục theo source; W cuối = arrival cuối. Bản chi tiết dùng `E001.W7`; prefix H/U/D/J/G/S lấy theo thành phố. Arrival dùng `A-H0` một lần để tránh chồng nhãn. JSON/CSV có đủ W0 và W cuối cho từng cạnh.
- Nếu nhãn dày: mở `RouteReview.html`, chọn thành phố + Edge ID để xem riêng toàn bộ W0…Wn của đúng tuyến. Không dựa vào nhãn gần nhất ở vùng chồng tuyến.
- Reference: `Validation/MapNpcMarkers-20261018/fresh02/npc-fresh-final.json` — save QA thật sau H7, HQ02 đang pending, không phải save cá nhân đang chơi. Stage/world lock của từng thành phố ghi trong JSON. Màu nét chỉ mô tả save này; KHÔNG sửa discovery và không tuyên bố map world-locked đang vào được trong game.

## Vẽ trong Paint

1. Dùng Save As để giữ nguyên PNG export; đừng resize, crop hoặc xoay. Đặt tên ví dụ `HoiAn_Routes_3840_EDIT.png`. Gửi kèm đúng tên bản export đã vẽ; không chuyển qua JPEG.
2. **XANH LÁ:** đường mới Lumi đi. Ghi rõ `H-E012` cạnh nét xanh (chỉ là ví dụ ID; tra bảng thực tế). Toàn tuyến: nối đúng hai arrival/đầu cuối tuyến hiện có, có thể chỉ dẫn bằng node ID. Một phần tuyến: đặt hai chấm xanh trên tuyến cũ rồi nối đường thay thế giữa chúng.
3. **ĐỎ:** chỉ đoạn muốn Lumi bị che bởi mái/cổng/vật thể. Export không tự thêm nét đỏ/occlusion mới. Vùng che hiện có giữ ở `existingCoveredWalkways` trong JSON; không đánh dấu không có nghĩa là xóa nó.
4. Giữ nguyên những phần không đánh dấu. Nếu nhiều route cùng corridor, phải ghi Edge ID, node đầu/cuối và W gần hai điểm sửa. Có thể gửi ảnh khoanh vùng phụ nhưng giữ PNG toàn bản đồ làm hệ tọa độ chuẩn.
5. Nếu không khớp chắc chắn, xử lý tiếp theo phải báo **NEEDS_USER_CLARIFICATION**, không tự đoán tuyến hoặc sửa hàng loạt các cạnh cùng đi qua một đoạn.

## Tọa độ / truy ngược source

Unity: u=x, v=y; gốc trái DƯỚI, 0..1. Bảng `CurrentWaypoints`: X=1000*u, Y=1000*(1-v), gốc trái TRÊN. PNG 3840×2880: px=X*3840/1000, py=Y*2880/1000. Nền native 1448×1086: px=X*1448/1000, py=Y*1086/1000. Giới hạn tọa độ hình học là 0..W/0..H; pixel cuối raster là W-1/H-1. Không có offset/crop/rotation.

JSON giữ cả float Unity gốc, bends gốc, sourceJsonPointer, SourceRouteKey và thứ tự; CSV chính đủ City/EdgeID/FromNode/ToNode/CurrentWaypoints/SourceRouteKey. `waypoint_mapping.csv` có từng W và pixel chính xác. Export ID chỉ nằm trong package, không thêm vào gameplay; `edge-id-registry.json` đóng băng mapping. Khi xuất lại sau này, dùng `--reuse-edge-registry` để giữ mã cũ, cấp mã nối tiếp cho cạnh mới, không renumber.

## Kiểm chứng và giới hạn

Nguồn geometry thực tế: `GameplayContent.json` edges; HoiAn node order/positions từ `HoiAnData.json` theo đúng `GameData.Load`. Geometry = arrival đầu + mọi bends + arrival cuối theo `CityRouteGeometry.Points`. Không smooth, bỏ điểm, thay đoạn cong/gấp khúc bằng chord hoặc tự sửa tuyến sai địa hình.

`EXPORT_MANIFEST.json` ghi hash nguồn, ảnh, checkpoint tham chiếu và kiểm tra trước/sau. Không chạy Unity/build vì không sửa runtime. Không đổi Assets, scene/prefab, quest, save, marker, camera, audio hoặc routes. Đây là sơ đồ kỹ thuật, KHÔNG phải screenshot runtime và KHÔNG nghiệm thu địa hình PASS.

**DỪNG sau export. Chờ ảnh đã vẽ; chưa áp dụng bất kỳ chỉnh sửa đường nào.**
''', encoding='utf-8')
    assert sum(len(c['nodes']) for c in cities) == 133 and sum(len(c['edges']) for c in cities) == 155
    assert len(set(registry.values())) == 155
    after = {p: sha(ROOT/p) for p in before}
    assert before == after, 'Input changed while exporting; do not use this package'
    files = [{'path': p.relative_to(out).as_posix(), 'bytes': p.stat().st_size, 'sha256': sha(p)} for p in sorted(out.rglob('*')) if p.is_file()]
    dump(out/'EXPORT_MANIFEST.json', {'createdUTC': datetime.now(timezone.utc).isoformat(),
         'type': 'READ_ONLY_ROUTE_MARKUP_EXPORT_NOT_RUNTIME_SCREENSHOTS', 'checkpoint': receipt,
         'referenceSave': REFERENCE, 'sourceHashesBeforeAndAfterIdentical': before, 'inputsUnchanged': True,
         'geometryImplementation': 'GameData.Load + CityRouteGeometry.Points; no simplification',
         'cities': proofs, 'files': files})
    print('PASS: 6 maps; 133 nodes; 155 unique Edge IDs; all original vertices; protected inputs unchanged.')


if __name__ == '__main__': main()
