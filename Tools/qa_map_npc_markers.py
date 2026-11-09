"""Bounded evidence, scope checks and incremental checkpoint for the NPC map fix.

Reads only named task inputs/evidence. Never changes gameplay, source art or saves.
"""
import argparse
import ast
import csv
import hashlib
import html
import json
import re
import shutil
from collections import Counter
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / 'Docs/QA/MapNpcMarkers-20261018'
QA = ROOT / 'Validation/MapNpcMarkers-20261018'
PRODUCTION = [
    'Assets/_Game/Resources/Content/AdventurePresentation.json',
    'Assets/_Game/Resources/AdventureUIArt.asset',
    'Assets/_Game/Scripts/UI/AdventureUIArt.cs',
    'Assets/_Game/Scripts/UI/AdventureIconGraphic.cs',
    'Assets/_Game/Scripts/Maps/CityMap.cs',
    'Assets/_Game/Scripts/Maps/NodeMarkerView.cs',
]
SUPPORT = [
    'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs',
    'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.MapNpcMarkers.cs',
    'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.MapNpcMarkers.cs.meta',
    'Assets/_Game/Scripts/Editor/MapNpcMarkerSetup.cs',
    'Assets/_Game/Scripts/Editor/MapNpcMarkerSetup.cs.meta',
    'Tools/qa_map_npc_markers.ps1', 'Tools/qa_map_npc_markers.py',
]


def load(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p, value): p.write_text(value.rstrip() + '\n', encoding='utf-8')
def dump(p, value): write(p, json.dumps(value, ensure_ascii=False, indent=2))


def normalize_exports(before):
    # Unity adds empty Android icon slots during the Windows build. Preserve
    # settings only when every other byte-equivalent text section is unchanged.
    rel = 'ProjectSettings/ProjectSettings.asset'
    old = (before / rel).read_text(encoding='utf-8-sig')
    new = (ROOT / rel).read_text(encoding='utf-8-sig')
    pattern = r'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
    assert re.sub(pattern, '', old) == re.sub(pattern, '', new)
    (ROOT / rel).write_bytes((before / rel).read_bytes())
    # Native export has already had the curated identity merge applied. There
    # must be no reference or identity changes before restoring export noise.
    rel = 'Docs/Art/native-art-bindings.json'
    assert load(before / rel) == load(ROOT / rel)
    rel = 'Docs/Art/art-asset-manifest.json'
    old, new = load(before / rel), load(ROOT / rel)
    extra = 'Docs/Gameplay/HoiAnLighting-20261018/ABC-runtime-comparison.png'
    new['generated_utc'] = old['generated_utc']
    if not any(e['path'] == extra for e in old['images']):
        removed = [e for e in new['images'] if e['path'] == extra]
        new['images'] = [e for e in new['images'] if e['path'] != extra]
        new['counts']['all_images'] -= len(removed)
    assert old == new, 'Unexpected art inventory delta'
    rel = 'Docs/Art/art-asset-manifest.csv'
    old = (before / rel).read_text(encoding='utf-8-sig')
    new = (ROOT / rel).read_text(encoding='utf-8-sig')
    assert old == ''.join(l for l in new.splitlines(True) if extra not in l)
    rel = 'Docs/Art/ART_ASSET_CATALOG.md'
    old = (before / rel).read_text(encoding='utf-8-sig')
    new = (ROOT / rel).read_text(encoding='utf-8-sig')
    assert old == new.replace('236 ảnh toàn project', '235 ảnh toàn project')
    for rel in ['Docs/Art/native-art-bindings.json', 'Docs/Art/art-asset-manifest.json',
                'Docs/Art/art-asset-manifest.csv', 'Docs/Art/ART_ASSET_CATALOG.md']:
        (ROOT / rel).write_bytes((before / rel).read_bytes())
    # Unity re-escapes three strings (including H6) from \u00xx to \xXX.
    # Verify identical decoded strings, then keep the entire original section.
    rel = 'Assets/_Game/Resources/AdventureUIArt.asset'
    old = (before / rel).read_text(encoding='utf-8-sig')
    new = (ROOT / rel).read_text(encoding='utf-8-sig')
    old_section, new_section = old[old.index('  checkIns:'):], new[new.index('  checkIns:'):]
    def decoded(text):
        return re.sub(r'"(?:\\.|[^"\\])*"', lambda m: json.dumps(ast.literal_eval(m[0]), ensure_ascii=False), text)
    assert decoded(old_section) == decoded(new_section)
    if old_section != new_section:
        write(ROOT / rel, new[:new.index('  checkIns:')] + old_section)


def checks():
    baseline = load(DOC / 'baseline.json')
    before = Path(baseline['beforeCheckpoint'])
    protected = []
    for e in baseline['protected']:
        assert sha(ROOT / e['path']) == e['sha256'], e['path']
        protected.append(dict(e, result='EXACT'))
    rel = 'Assets/_Game/Resources/Content/AdventurePresentation.json'
    old, new = load(before / rel), load(ROOT / rel)
    assert {k for k in old if old[k] != new[k]} == {'mapActors', 'nodeMarkers'}
    assert new['mapActors'][:len(old['mapActors'])] == old['mapActors']
    assert new['nodeMarkers'][:len(old['nodeMarkers'])] == old['nodeMarkers']
    added = new['mapActors'][len(old['mapActors']):]
    assert {a['nodeId'] for a in added} == {'H10', 'H14', 'G9', 'G14', 'S2', 'S7', 'S11'}
    assert {(a['cityId'], a['nodeId'], a['marker']) for a in new['nodeMarkers'][len(old['nodeMarkers']):]} == {('hue', 'U14', 'Quest'), ('seoul', 'S14', 'Quest')}
    # Unity YAML top-level sections, avoiding dependency on an external YAML lib.
    def sections(p):
        text = p.read_text(encoding='utf-8-sig')
        starts = list(re.finditer(r'^  (\w+):', text, re.M))
        return {m[1]: text[m.start():starts[i+1].start() if i+1 < len(starts) else len(text)] for i, m in enumerate(starts)}
    rel = 'Assets/_Game/Resources/AdventureUIArt.asset'
    a, b = sections(before / rel), sections(ROOT / rel)
    assert a.keys() == b.keys()
    assert [k for k in a if a[k] != b[k]] == ['mapActors']
    defaults = re.compile(r'^    (?:hasMapPosition: 0|mapPosition: \{x: 0, y: 0\}|markerClearance: 0)\n', re.M)
    first_new = b['mapActors'].index('  - cityId: hoi_an\n    nodeId: H10\n')
    assert defaults.sub('', b['mapActors'][:first_new]) == a['mapActors']
    result = {'protected': protected, 'originalActorsAndRules': 'EXACT',
              'allNonActorArtBindings': 'EXACT', 'addedActors': added,
              'addedMarkerRules': new['nodeMarkers'][len(old['nodeMarkers']):]}
    dump(DOC / 'scope-verification.json', result)
    return baseline


def report():
    before = load(DOC / 'before-audit.json')
    with (QA / 'six-cities02/runtime-npc-audit.csv').open(encoding='utf-8-sig', newline='') as f:
        actual = {r['NodeId']: r for r in csv.DictReader(f, delimiter='|')}
    with (QA / 'seoul03/runtime-npc-audit.csv').open(encoding='utf-8-sig', newline='') as f:
        actual.update({r['NodeId']: r for r in csv.DictReader(f, delimiter='|')})
    rows = ['| City | NodeId | Marker runtime (save hoàn tất) | NPC ID | Tên NPC | Sprite nguồn / spriteName | Đã hiển thị trên map | Kết quả |',
            '|---|---|---|---|---|---|---|---|']
    for r in before:
        a = actual[r['node']]
        source = r.get('source') or {}
        src = ('`' + source['path'] + '` / `' + source['spriteName'] + '`') if source else '—'
        location = r['classification'] == 'C_LOCATION_INFO'
        result = 'PASS · thông tin địa điểm, không thêm người' if location else 'PASS · actor + binding'
        if 'PENDING_APPROVAL' in source.get('status', ''):
            result += '; ngoại hình PENDING'
        rows.append('| ' + ' | '.join([r['city'], r['node'], a['Marker'], r['npcId'] or '—', r['name'], src, 'Không cần NPC' if location else a['Visible'], result]) + ' |')
    write(DOC / 'AFTER_NPC_MARKERS.md', '# Sau sửa — sáu thành phố\n\nMarker ở bảng là trạng thái thật của old save đã hoàn tất, không phải mọi giai đoạn cùng lúc. H10 được kiểm riêng NpcTalk → NpcNeedItem → restart; các marker phụ thuộc state khác giữ resolver cũ.\n\n' + '\n'.join(rows))
    inventory = load(DOC / 'unused-native-inventory.json')
    lines = ['# Tài nguyên chưa dùng: không đồng nghĩa nhân vật tự do', '',
             'Không tạo danh tính mới. Bảy vị trí thiếu đã có npcId và sprite riêng nên không cần mượn người khác.', '',
             '| Phân loại | Native entries |', '|---|---|']
    for k, n in Counter(x['classification'] for x in inventory).items(): lines.append(f'| {k} | {n} |')
    lines += ['', '31 native entries trong phạm vi atlas/NPC inventory; chỉ một ứng viên một người (đầu bếp `_14`) còn cần kiểm tra crop và vai trò trước khi dùng. Không gán vì hiện không cần NPC mới. Hai composite nhiều người không được dùng nguyên ảnh làm portrait; 22 auto-slice không phải 22 nhân vật sạch.',
              '', 'Ngoài ra có 12 native entries thuộc bốn ảnh DaLatRejectedDraft: là draft đã loại có danh tính cụ thể, không phải kho người chưa định danh. GUID/localID, đường dẫn từng entry ở hai JSON kèm theo. Các nhân vật Ji-won, Phúc, Mai và hai bot dành riêng không được tính là ứng viên tự do.', '',
              '| spriteName | Nguồn | GUID / localID | Phân loại |', '|---|---|---|---|']
    for e in inventory:
        lines.append(f"| {e['spriteName']} | `{e['path']}` | `{e['guid']}` / `{e['localId']}` | {e['classification']} |")
    write(DOC / 'UNUSED_NPC_ART.md', '\n'.join(lines))
    summaries = []
    for name in ['six-cities01', 'six-cities02', 'seoul03', 'fresh01', 'restart01', 'hidden01', 'fresh02', 'restart02']:
        p = QA / name / 'smoke-results.txt'
        lines = p.read_text(encoding='utf-8-sig').splitlines()
        assert lines[-1] == 'ALL CHECKS PASSED', name
        summaries.append({'run': name, 'assertions': sum(l.startswith('PASS') for l in lines), 'result': 'PASS', 'resultSHA256': sha(p)})
    dump(DOC / 'test-results.json', summaries)
    images = [('fresh02', 'NPC-H10-before-accept', 'H10: trước nhận quest, NPC có thật và dialogue'),
              ('fresh02', 'NPC-H10-needs-item', 'H10: NpcNeedItem, chờ vật phẩm'),
              ('restart02', 'NPC-H10-needs-item-restart', 'H10: process restart giữ quest và actor')]
    for node in ['H14', 'G9', 'G14', 'S2', 'S7', 'S11']:
        images += [('six-cities02', f'NPC-{node}-map', node + ': map actor'), ('six-cities02', f'NPC-{node}-dialogue', node + ': hội thoại thật')]
    images += [('six-cities02', 'NPC-location-U14', 'U14: thông tin cầu, marker journal'),
               ('six-cities02', 'NPC-location-S14', 'S14: thông tin bến, marker journal'),
               ('six-cities02', 'Portal-H8', 'H8: Portal arch + arrow'),
               ('six-cities02', 'Portal-H8-WorldMap', 'H8: mở World Map, chưa tự vào Jeonju'),
               ('six-cities02', 'Portal-H20-WorldMap', 'H20: mở World Map, chưa tự vào Huế')]
    for city in ['hoi_an', 'hue', 'da_lat', 'jeonju', 'gyeongju', 'seoul']:
        images.append(('six-cities02', 'NPC-city-' + city, 'City Map ' + city))
    figures = []
    for run, stem, label in images:
        if stem == 'NPC-city-hoi_an':
            run, stem = 'fresh02', 'NPC-H10-needs-item'
            label = 'City Map Hội An: State B20, vị trí H10 cuối'
        if stem.startswith(('NPC-S', 'NPC-city-seoul', 'Portal-')):
            run = 'seoul03'
        assert (QA / run / (stem + '.png')).exists()
        url = f'../../../Validation/MapNpcMarkers-20261018/{run}/{stem}.png'
        figures.append(f'<figure><a href="{url}"><img loading="lazy" src="{url}" alt="{html.escape(label)}"></a><figcaption>{html.escape(label)}</figcaption></figure>')
    write(DOC / 'RuntimeReview.html', '<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC NPC runtime review</title><style>body{background:#162f34;color:#fff7df;font:16px system-ui;margin:24px}main{display:flex;flex-wrap:wrap;gap:18px}figure{margin:0;width:300px}img{width:100%;height:auto}figcaption{padding:8px}</style><h1>NPC & marker — runtime Windows mới</h1><p>Không có ảnh mockup. Ảnh 540×960; bấm xem nguyên kích thước. Các ảnh trạng thái hoàn tất dùng save đã kiếm thật từ Phase4; H10 fresh/restart dùng save H7 trước relic restored. Bốn NPC Keeper/Inscription/Curator/Engineer chưa được duyệt canonical. G14/S11 đã được chỉnh chân lần hai sau review trực quan; ảnh này là bản cuối.</p><main>' + ''.join(figures) + '</main></html>')


def checkpoint():
    baseline = checks()
    base = Path(baseline['baseCheckpoint'])
    expected = 'fab1b8c2314890eb2b6c1a7f16894cb7c3c9c83bc5b34aa4ca0975749e728175'
    assert sha(base / 'CHECKPOINT_MANIFEST.json') == expected
    # Verify only the named immediate base; no scan/hash of unrelated backups.
    manifest = load(base / 'CHECKPOINT_MANIFEST.json')
    for e in manifest['entries']: assert sha(base / e['path']) == e['sha256'], e['path']
    paths = {ROOT / p for p in PRODUCTION + SUPPORT}
    paths.update(p for p in DOC.rglob('*') if p.is_file() and p.name != 'checkpoint.json')
    paths.update(p for p in QA.rglob('*') if p.is_file())
    paths.add(ROOT / 'Docs/QA/MAP_NPC_MARKERS_SIX_CITIES.md')
    paths.add(ROOT / 'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md')
    for i in [1, 2, 3, 4, 5]: paths.add(ROOT / f'Logs/map-npc-markers-build0{i}.log')
    assert all(p.is_file() for p in paths)
    total = sum(p.stat().st_size for p in paths)
    print(f'Checkpoint estimate: {len(paths)} files, {total:,} bytes; requires {base}')
    assert total < 150_000_000, 'Unexpected large checkpoint'
    target = ROOT.parent / 'Backups/CheckpointMapNpcMarkers-20261018'
    assert not target.exists(), 'Never overwrite a checkpoint'
    target.mkdir()
    entries = []
    for p in sorted(paths):
        rel = p.relative_to(ROOT).as_posix()
        dst = target / rel
        dst.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(p, dst)
        digest = sha(p)
        assert sha(dst) == digest
        entries.append({'path': rel, 'sha256': digest, 'bytes': p.stat().st_size})
    dump(target / 'CHECKPOINT_MANIFEST.json', {
        'type': 'INCREMENTAL_REQUIRES_BASE', 'baseCheckpoint': str(base), 'baseManifestSHA256': expected,
        'createdUTC': datetime.now(timezone.utc).isoformat(),
        'summary': 'Seven existing NPC map actors; two location-info marker overrides; Portal glyph; targeted Windows evidence. No identities/quests/routes/save changes.',
        'entries': entries, 'deletedPaths': [], 'totalPayloadBytes': total})
    dump(DOC / 'checkpoint.json', {'checkpoint': str(target), 'requiresBase': str(base),
         'manifestSHA256': sha(target / 'CHECKPOINT_MANIFEST.json'), 'files': len(entries),
         'payloadBytes': total, 'allCopiedHashesVerified': True})
    print('Checkpoint created and verified:', target)


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('action', choices=['normalize', 'verify', 'report', 'checkpoint'])
    action = parser.parse_args().action
    if action == 'normalize': normalize_exports(Path(load(DOC / 'baseline.json')['beforeCheckpoint']))
    elif action == 'verify': checks(); print('Scope checks PASS')
    elif action == 'report': checks(); report(); print('Tables and runtime review written')
    else: checkpoint()
