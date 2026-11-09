"""Read-only NPC audit. Writes review documents only; never changes Unity assets/approval."""
from pathlib import Path
from collections import Counter
from html import escape as h
from urllib.parse import quote
import csv
import hashlib
import json
import os
import re
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/Art/NpcInventory-20261017'
CITY = {'hoi_an': 'Hội An', 'hue': 'Huế', 'da_lat': 'Đà Lạt',
        'jeonju': 'Jeonju', 'gyeongju': 'Gyeongju', 'seoul': 'Seoul'}
def read(path):
    return json.loads((ROOT / path).read_text(encoding='utf-8-sig'))

registry = read('Docs/Art/NPC_CANONICAL_REGISTRY.json')['identities']
bindings = read('Docs/Art/native-art-bindings.json')['entries']
content = read('Assets/_Game/Resources/Content/GameplayContent.json')
events = content['events']
quest_titles = {q['id']: q['title'] for q in content['quests']}
manifest = {x['path']: x for x in read('Docs/Art/art-asset-manifest.json')['images']}
before = read('Docs/Art/NpcIdentityLock-20261017/before.json')
bound = {(x['guid'], str(x['localId'])) for x in bindings
         if x['role'] not in ('native_sprite', 'scene_prefab_dependency')}
SHEET = 'Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png'
ATLAS = 'Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png'
native = [dict(x) for x in bindings if x['role'] == 'native_sprite' and
          ('/Characters/NPC/' in x['path'] or x['path'] == SHEET or
           x['spriteName'] in ('Characters_MissingSprites_v1_0', 'Concept_Composite_01_0', 'Concept_Composite_02_0'))]

def rect(entry):
    with Image.open(ROOT / entry['path']) as im:
        width, height = im.size
    meta = (ROOT / (entry['path'] + '.meta')).read_text(encoding='utf-8')
    match = re.search(r'name: ' + re.escape(entry['spriteName']) +
                      r'\s*\n\s*rect:\s*\n\s*serializedVersion: \d+\s*\n\s*x: (\S+)\s*\n\s*y: (\S+)\s*\n\s*width: (\S+)\s*\n\s*height: (\S+)', meta)
    if match:
        x, y, w, ht = map(float, match.groups())
        return [x, height-y-ht, w, ht], [width, height]
    assert str(entry['localId']) == '21300000', entry
    return [0, 0, width, height], [width, height]

for e in native:
    e['bound'] = (e['guid'], str(e['localId'])) in bound
    e['rectTopLeft'], e['sourceDimensions'] = rect(e)
    if e['bound']:
        e['classification'] = 'ASSIGNED'
    elif e['path'] != SHEET:
        e['classification'] = 'WHOLE_CONCEPT_SHEET'
    else:
        index = int(e['spriteName'].rsplit('_', 1)[1])
        e['classification'] = {14: 'SINGLE_PERSON_CANDIDATE_NEEDS_REVIEW',
                               19: 'RESERVED_MAI_VARIANT', 21: 'MULTI_PERSON_COMPOSITE',
                               22: 'MULTI_PERSON_COMPOSITE', 24: 'BOT_RESERVED',
                               28: 'BOT_RESERVED'}.get(index, 'AUTO_SLICE_FRAGMENT')

proposals = {
    'npc_innkeeper': 'NOT_REQUIRED_CURRENT_GAMEPLAY: D18 là lời dẫn Lumi/nghỉ ngơi, chưa thể hiện chủ trọ. Không gán đầu bếp để lấp chỗ trống.',
    'npc_keeper': 'NEEDS_NEW_ART: người lính trong roster là lính Hoàng thành Huế, không hợp bối cảnh Gyeongju; không ép gán.',
    'npc_inscription': 'NEEDS_NEW_ART: chưa có nhân vật độc lập phù hợp kho bản khắc Gyeongju; không mượn Phúc/Yoon/Min.',
    'npc_tech': 'Ji-won chưa có ứng viên đủ căn cứ; tuyệt đối không dùng mặt So-yeon.',
    'npc_curator': 'Chưa thấy ứng viên đủ căn cứ cho quản thủ Seoul; không mượn Min, Yoon hoặc Phúc.',
    'npc_engineer': 'Chưa thấy ứng viên kỹ sư độc lập đủ căn cứ; không dùng lại So-yeon.',
    'npc_boat': 'Event mang tên địa điểm Bến phà thông tin; chưa xác định nhân dạng người nói. Không dùng bác lái đò Hội An/Tâm làm người khác.'}

rows = []
for n in registry:
    ev = [e for e in events if e.get('npcId') == n['npcId']]
    refs = [v for v in n['variants'] if v.get('runtimeBound')]
    rows.append({'npcId': n['npcId'], 'name': n['canonicalIdentity']['name'],
                 'status': n['status'],
                 'locations': sorted({f"{CITY[e['cityId']]} {e['nodeId']}" for e in ev}),
                 'visuals': [dict(v, provenance=manifest.get(v['path'], {}).get('source', ''),
                                  catalogIssues=manifest.get(v['path'], {}).get('issues', [])) for v in refs],
                 'interaction': [{'eventId': e['id'], 'type': e['type'], 'speaker': e['actor'],
                                  'questId': e.get('questId', ''), 'cityId': e['cityId'], 'nodeId': e['nodeId']} for e in ev],
                 'verification': 'SOURCE_CONFIG_VERIFIED; runtime mới NOT VERIFIED',
                 'portraitAssessment': n.get('portraitAssessment', {}),
                 'proposal': proposals.get(n['npcId'], '')})

def link(path):
    return quote(os.path.relpath(ROOT / path, OUT).replace('\\', '/'), safe='/')

def picture(e, crop=None):
    region, size = rect(e) if crop is None else (crop, Image.open(ROOT / e['path']).size)
    x, y, w, ht = region
    return (f'<a href="{link(e["path"])}" target="_blank"><svg viewBox="0 0 {w} {ht}" role="img" '
            f'aria-label="{h(e.get("spriteName", "Source region"))}"><image href="{link(e["path"])}" '
            f'x="{-x}" y="{-y}" width="{size[0]}" height="{size[1]}"/></svg></a>')

def interaction(row):
    return '; '.join(f"{e['eventId']} [{e['type']}]" + (f" → {e['questId']} ({quest_titles.get(e['questId'], '')})" if e['questId'] else '')
                     + (' — Lumi dẫn chuyện, chưa có NPC nói' if e['type'] == 'start' else '')
                     for e in row['interaction'])

def card(row):
    pics = ''.join(picture(v) for v in row['visuals'])
    refs = ''.join(f'<p><a href="{link(v["path"])}">{h(v["spriteName"])}</a><br><small>{h(v["path"])}<br>GUID {v["guid"]} / localID {v["localId"]}<br>Nguồn catalog: {h(v["provenance"])}</small></p>' for v in row['visuals'])
    return f'<article><h3>{h(row["name"])}</h3><b>{h(", ".join(row["locations"]))}</b>{pics}{refs}<p>{h(row["npcId"])} · {h(row["status"])}</p><p>{h(interaction(row))}</p><p>{h(row["proposal"])}</p><p>{h(row["portraitAssessment"].get("assetStatus", ""))}</p></article>'

OUT.mkdir(parents=True, exist_ok=True)
pending = [r for r in rows if r['status'] == 'UNCONFIRMED_VISUAL']
missing = [r for r in rows if r['status'] == 'MISSING_VISUAL']
confirmed = [r for r in rows if r['status'] == 'CANONICAL']
unbound = [e for e in native if not e['bound']]
assert (len(confirmed), len(pending), len(missing), len(native), len(unbound)) == (8, 12, 7, 51, 31)
classification = dict(Counter(e['classification'] for e in unbound))
drafts = sorted((ROOT / 'Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts').glob('npc_*.png'))
data = {'scope': 'Current NPC registry + native binding export + exact NPC source sheets and rejected drafts. Excludes Lumi/enemy art as independent NPCs.',
        'approvalChanges': [], 'bindingChanges': [], 'runtimeTest': 'NOT VERIFIED in this inventory pass',
        'counts': {'canonical': 8, 'assignedUnconfirmed': 12, 'missingPortraitRoles': 7,
                   'nativeNpcScope': 51, 'assignedNative': 20, 'unboundNative': 31,
                   'unboundClassification': classification, 'rejectedNpcDraftSourceFiles': len(drafts)},
        'npcs': rows, 'unboundNative': unbound,
        'rejectedDraftSources': [str(p.relative_to(ROOT)).replace('\\', '/') for p in drafts]}
(OUT / 'inventory.json').write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')
with (OUT / 'NPCs.csv').open('w', newline='', encoding='utf-8-sig') as file:
    writer = csv.writer(file)
    writer.writerow(['Nhóm', 'npcId', 'Tên', 'Node', 'Hình đang dùng', 'Tương tác cấu hình', 'Kiểm thử', 'Đề xuất'])
    for r in pending + missing + confirmed:
        writer.writerow([r['status'], r['npcId'], r['name'], ', '.join(r['locations']),
                         '; '.join(v['path'] + '#' + v['spriteName'] for v in r['visuals']),
                         interaction(r), r['verification'], r['proposal']])

extra = []
for e in unbound:
    if e['classification'] != 'AUTO_SLICE_FRAGMENT':
        extra.append(f'<article><h3>{h(e["spriteName"])}</h3>{picture(e)}<p>{h(e["classification"])}</p><small>{h(e["path"])}<br>GUID {e["guid"]} / localID {e["localId"]}<br>Vùng ảnh trên-trái {e["rectTopLeft"]}</small></article>')
source_candidates = [
    ('Lính chưa gán — SOURCE ONLY', ATLAS, [310,135,280,430], 'Roster xác định lính Hoàng thành Huế. Không phù hợp G9/G14 Gyeongju; không gán sang Hàn Quốc. Chưa có native sprite riêng.'),
    ('Người gánh hàng trẻ — SOURCE ONLY', SHEET, [800,170,330,390], 'Nằm trong ảnh ghép _21 cạnh Phúc; không thay thương nhân canonical, chưa đề xuất vai trò cụ thể.'),
    ('Nhân vật Hàn và khách chụp ảnh — CẦN ĐỐI CHIẾU', SHEET, [29,598,575,442], 'Không đếm là hai NPC tự do: cần phân biệt biến thể Hana/Ji-eun/Min trước khi gán danh tính.')]
source_cards = ''.join(f'<article><h3>{h(title)}</h3>{picture({"path":path}, crop)}<p>{h(note)}</p><small>{h(path)}</small></article>' for title,path,crop,note in source_candidates)
for title, path, note in [
    ('DesignKit cũ — chỉ là tham khảo', 'LightBetweenCultures_DesignKit_v1/References/IndividualConcepts/12_HoiAn_NPC_Concepts.png', 'Ảnh nguyên chỉ 296×338, từng người khoảng 50–70×110–130, nền/chữ baked. Không đủ căn cứ tạo portrait sạch 220×216 từ đây. Mai và khách đeo kính còn phải bảo toàn nhận diện đã có.'),
    ('Roster — đối chiếu vai trò gốc', 'LBC_AnhMoi_BanGiaoCodex_20261015/Characters/NPC_Roster_Concept.png', 'Lính Hoàng thành Huế có vai trò rõ trên nguồn. Gương mặt công nghệ đã gán So-yeon, không lấy cho Ji-won/quản thủ/kỹ sư.'),
    ('Moodboard cũ — không là kho mặt trống', 'LBC_AnhMoi_BanGiaoCodex_20261015/Characters/Characters_Moodboard.png', 'Các archetype khớp những vai trò/nhận diện đã dùng. Khác trang phục/pose không tự thành người mới; không tái gán cô gái Tech-Curator cho NPC khác.')]:
    with Image.open(ROOT/path) as im:
        crop = [0,0,*im.size]
    source_cards += f'<article><h3>{h(title)}</h3>{picture({"path":path},crop)}<p>{h(note)}</p><small>{h(path)}</small></article>'
draft_cards = ''.join(f'<article><h3>{h(p.name)}</h3>{picture({"path":str(p.relative_to(ROOT)).replace(chr(92),"/"),"spriteName":p.stem}, [0,0,*Image.open(p).size])}<p>REJECTED_DRAFT — thuộc nhân vật đã có, không là danh tính trống, không dùng.</p></article>' for p in drafts)
style = 'body{font:16px/1.5 system-ui;background:#162e32;color:#fff3d5;max-width:1300px;margin:auto;padding:24px}a{color:#8be5d2}h1,h2{color:#fbd07e}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(280px,1fr));gap:18px}article{padding:18px;border:1px solid #64817b;border-radius:14px;background:#223c40}svg{width:100%;height:290px;background:repeating-conic-gradient(#d3dbd5 0 25%,#edf0e9 0 50%) 0/20px 20px;margin-top:12px}small{overflow-wrap:anywhere}table{border-collapse:collapse}td,th{border:1px solid #64817b;padding:8px}aside{border-left:4px solid #ffc562;padding:12px;background:#30474a}'
html = f'<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>LBC — NPC review 17/10/2026</title><style>{style}</style><h1>Kiểm kê NPC — chưa duyệt thêm danh tính</h1><aside>8 canonical giữ nguyên · 12 có hình chờ duyệt · 7 vai trò thiếu portrait. Chỉ đối chiếu ảnh nguồn và cấu hình hiện tại; không phải ảnh runtime mới. Nhấp ảnh để mở PNG gốc. SVG chỉ giới hạn vùng xem, không tạo sprite/art mới.</aside><h2>1. Đã gán hình, chưa duyệt canonical (12)</h2><div class="grid">' + ''.join(card(r) for r in pending) + '</div><h2>2. Vai trò thiếu portrait (7)</h2><p>D18 hiện là lời dẫn Lumi; S14 là địa điểm/bến phà, chưa xác định nhân dạng người nói. Không đồng nhất bảy khóa thiếu ảnh với bảy NPC đang hiển thị.</p><div class="grid">' + ''.join(card(r) for r in missing) + '</div><h2>3. Tài nguyên chưa gán — số lượng thật</h2><p>51 native sprite trong phạm vi NPC; 20 đã gán và 31 chưa gán. 31 gồm: 1 ứng viên người đơn lẻ cần review, 1 biến thể Mai được giữ danh tính, 2 ảnh nhiều người, 2 bot, 22 mảnh auto-slice và 3 sheet nguyên. KHÔNG có 31 NPC sẵn sàng dùng.</p><div class="grid">' + ''.join(extra) + '</div><h2>Ứng viên chỉ có trên ảnh nguồn</h2><div class="grid">' + source_cards + '</div><h2>4 bản nháp NPC loại bỏ — không phải nhân vật tự do</h2><div class="grid">' + draft_cards + '</div><h2>8 danh tính đã xác nhận — giữ binding</h2><div class="grid">' + ''.join(card(r) for r in confirmed) + '</div><p>Vùng xem sheet có thể gồm người lân cận; đây là bằng chứng nguồn, không phải chứng nhận crop sẵn dùng. Danh sách đủ 22 mảnh: inventory.json. Không tự phê duyệt hoặc gán bất kỳ đề xuất nào.</p></html>'
(OUT / 'Review.html').write_text(html, encoding='utf-8')

lines = ['# Kiểm kê NPC — 17/10/2026', '', '[Ảnh review (HTML, mở bằng trình duyệt)](Review.html) · [Bảng CSV](NPCs.csv) · [Chi tiết GUID/localID và 31 sprite chưa gán](inventory.json)', '',
         'Kiểm kê và đánh giá tận dụng ảnh theo quyết định bổ sung của người dùng. Không đổi gameplay/binding/art hoặc tự duyệt 12 hình đang dùng. Registry/catalog chỉ bổ sung kết luận portrait: sáu NEEDS_NEW_ART, D18 NOT_REQUIRED_CURRENT_GAMEPLAY. Không chạy lại gameplay trong bước này: runtime mới **NOT VERIFIED**.', '',
         '[Kết luận tận dụng art cũ và lý do từng ứng viên](PortraitReuseAssessment.md). Không có ứng viên đủ điều kiện gán canonical mới; số đã giải quyết bằng binding mới: **0**.', '',
         '## 1. Có hình đang gán nhưng chưa duyệt canonical (12)', '', '| NPC | Node | Sprite hiện tại | Tương tác đang cấu hình |', '|---|---|---|---|']
for r in pending:
    lines.append(f'| {r["name"]} (`{r["npcId"]}`) | {", ".join(r["locations"])} | ' + '; '.join(f'[{v["spriteName"]}]({link(v["path"])})' for v in r['visuals']) + f' | {interaction(r)} |')
lines += ['', 'Các binding này giữ nguyên. Có event/quest trong source không đồng nghĩa đã kiểm thử runtime lại hay được người dùng duyệt danh tính.', '',
          '## 2. Vai trò chưa có portrait (7)', '', '| Vai trò | Node | Tương tác cấu hình | Ứng viên / giới hạn |', '|---|---|---|---|']
for r in missing:
    lines.append(f'| {r["name"]} (`{r["npcId"]}`) | {", ".join(r["locations"])} | {interaction(r)} | {r["proposal"]} |')
lines += ['', '## 3. Sprite/ảnh chưa gán', '',
          '| Phân loại trong 31 native sprite chưa gán | Số lượng | Nguồn / nhận xét |', '|---|---:|---|',
          '| Người đơn lẻ, ứng viên cần review | 1 | H3_07 `_14`: đầu bếp; kiểm tra biên phải/chảo, chưa dùng |',
          '| Biến thể danh tính Mai đã có | 1 | H3_07 `_19`, không gán cho người khác; crop cần review |',
          '| Ảnh ghép nhiều người | 2 | H3_07 `_21` (Phúc + người gánh hàng), `_22` (nữ Hàn + khách chụp ảnh); chưa là sprite riêng |',
          '| Bot/thiết kế dành cho bot | 2 | H3_07 `_24`, `_28`, không là ứng viên người |',
          '| Mảnh auto-slice | 22 | H3_07: chi tiết nhỏ/ảnh thiếu bộ phận, không là 22 nhân vật |',
          '| Sheet nguyên | 3 | Characters_MissingSprites_v1, LBC_Composite_CheckerConcept_v2, LBC_Composite_Illustrated_v1; không dùng nguyên sheet làm NPC |', '',
          'Tổng phạm vi native NPC: **51 sprite records = 20 đang gán + 31 chưa gán**. Atlas Shared có sáu sprite được import hiện tại, cả sáu đã gán; người lính chỉ hiện trong pixels nguồn, chưa có sprite riêng. Người gánh hàng trẻ là ứng viên nguồn khác nhưng chưa có vai trò phù hợp chắc chắn. Hai nhân vật hàng dưới H3 cần đối chiếu nhận diện trước, không tự tính là mặt trống. Những nhân vật trên composite là concept/biến thể, không cộng lại thành NPC độc lập.', '',
          'Ngoài 51 bản ghi trên còn **4 ảnh nháp NPC** trong `DaLatRejectedDrafts` (Lan, Thủy, người làm vườn, chó). Đây là bản nháp của vai trò đã có, không phải bốn nhân vật tự do. Ảnh review có đối chiếu nguồn; không đưa chúng vào gameplay.', '',
          'Kiểm kê dùng registry, native bindings, manifest sẵn có; đọc đúng PNG/meta liên quan trong Characters/NPC và các sheet nhân vật. Lumi và enemy giữ vai trò riêng, không coi là NPC chưa dùng. Không quét Backups/Library/Builds.', '',
          '## 8 danh tính đã xác nhận giữ nguyên', '',
          ', '.join(f'{r["name"]} (`{r["npcId"]}`)' for r in confirmed) + '.', '',
          'Sáu vai trò thực sự có người/đối tượng phát biểu vẫn NEEDS_NEW_ART. D18 không cần portrait chủ trọ ở gameplay hiện tại. Không tạo NPC, quest, cốt truyện hoặc art mới.', '',
          '## Kiểm tra bảo toàn', '',
          'Đối chiếu đúng 46 file được chụp trước lượt: xem [integrity.json](integrity.json). Chỉ ba tài liệu registry/catalog được phép bổ sung kết luận; các file runtime/source/art/settings giữ nguyên. Build Windows được đồng bộ lại sau khi thu hồi nháp duyệt hàng loạt; log `Logs/npc-inventory-restored-build.log`. Không dùng build nháp trước đó hoặc kết quả guard 20 canonical làm bằng chứng. Không tạo checkpoint gameplay mới vì production inputs giữ nguyên; checkpoint gần nhất vẫn là CheckpointQualityAddendum-20261017.']
(OUT / 'README.md').write_text('\n'.join(lines) + '\n', encoding='utf-8')
differences = [e['path'] for e in before if hashlib.sha256((ROOT / e['path']).read_bytes()).hexdigest() != e['sha256']]
allowed_docs = {'Docs/Art/NPC_CANONICAL_REGISTRY.json', 'Docs/Art/NPC_CANONICAL_REGISTRY.md', 'Docs/Art/ART_ASSET_CATALOG.md'}
production_differences = [p for p in differences if p not in allowed_docs]
original_registry = read('Docs/Art/NpcIdentityLock-20261017/BeforeFiles/Docs/Art/NPC_CANONICAL_REGISTRY.json')['identities']
original_by_id = {n['npcId']:n for n in original_registry}
identity_changes = [n['npcId'] for n in registry if {k:v for k,v in n.items() if k != 'portraitAssessment'} != original_by_id[n['npcId']]]
integrity = {'protectedFilesChecked': len(before), 'documentMetadataChanged': differences, 'productionDifferences': production_differences,
             'identityOrBindingChanges': identity_changes,
             'approvalCounts': dict(Counter(n['status'] for n in registry)),
             'newRuntimeTest': 'NOT VERIFIED; no new runtime binding', 'productionChanges': False if not production_differences else 'REVIEW_REQUIRED'}
(OUT / 'integrity.json').write_text(json.dumps(integrity, ensure_ascii=False, indent=2), encoding='utf-8')
assert not production_differences, production_differences
assert not identity_changes, identity_changes
print(json.dumps({'output': str(OUT), 'counts': data['counts'], 'protectedFilesUnchanged': len(before)-len(differences), 'metadataDocumentsChanged': differences}, ensure_ascii=False))
