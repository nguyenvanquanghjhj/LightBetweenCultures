"""Finalize bounded Phase4 reports from actual completed Windows runs."""
import html
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
QA = ROOT / 'Validation/Phase4-20261018'
DOC = ROOT / 'Docs/Gameplay/Phase4-20261018'


def load(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def write(rel, value):
    (ROOT / rel).write_text(value.rstrip() + '\n', encoding='utf-8')


def main():
    required = ['e2e-vn01', 'e2e-korea03', 'retro-vn01', 'retro-korea01', 'presentation01', 'hue-culture02', 'key-battle01', 'key-hack01', 'key-retained-fixture']
    required += [f'key-restart-{b}-{s}' for b in ['battle', 'hack'] for s in ['before-H5', 'after-H5', 'after-H7']]
    runs = []
    for folder in sorted(QA.iterdir()):
        if not folder.is_dir():
            continue
        result = next((p for p in [folder / 'smoke-results.txt', folder / 'restart-results.txt'] if p.is_file()), None)
        if result is None:
            continue
        text = result.read_text(encoding='utf-8-sig')
        last = text.splitlines()[-1]
        runs.append(dict(run=folder.name, result='PASS' if last == 'ALL CHECKS PASSED' else 'FAIL' if last == 'FAILED' else 'RUNNING', checks=sum(s.startswith('PASS:') for s in text.splitlines()), resultPath=result.relative_to(ROOT).as_posix()))
    by_name = {r['run']: r for r in runs}
    assert all(by_name.get(name, {}).get('result') == 'PASS' for name in required), [(name, by_name.get(name, {}).get('result')) for name in required]
    preservation = load(DOC / 'preservation-results.json')
    assert preservation['result'] == 'PASS'
    assert 'RELIC WINDOWS BUILD: Succeeded' in (ROOT / 'Logs/phase4-final-build02.log').read_text(encoding='utf-8-sig')
    catalog = load(ROOT / 'Assets/_Game/Resources/Content/CulturalRelics.json')
    relics, fragments = catalog['relics'], catalog['fragments']
    final = load(QA / 'e2e-korea03/P4-E2E-KOREA_FIRST-final.json')
    flags = set(final['flags'])
    assert all('relic.restored.' + r['id'] in flags for r in relics)
    assert all('relic.fragment.' + f['id'] in flags for f in fragments)
    assert 'game.ending_seen' not in flags and 'seoul.final_cleared' in flags
    (DOC / 'final-qa-summary.json').write_text(json.dumps(dict(runs=runs, preservationChecks=len(preservation['checks']), finalFragments=24, finalRestored=6, freshEndingSeen=False, android='NOT_TESTED'), indent=2) + '\n', encoding='utf-8')

    def proof(f):
        return ' AND '.join(p for p in [f.get('proof', ''), '(' + ' OR '.join(f.get('proofAny', [])) + ')' if f.get('proofAny') else ''] if p) or 'Đúng node REVEALED'

    matrix = ['# Phase4 — bảng tiếp cận 24 mảnh đã triển khai', '',
              'Mapping D01–D08 đã duyệt + merged Phase4 chính thức. **Windows targeted / hai tuyến / old-save PASS; VISUAL_PENDING.** Mỗi nguồn cần city mở, node REVEALED, proof thật và một thao tác kéo/căn/xoay/xác nhận mới. PREVIEW/HIDDEN không cấp mảnh. Không gọi lại reward nguồn cũ.', '',
              '| relicId | fragmentId / tên | Nguồn / điểm claim | Proof runtime | VN_FIRST | KOREA_FIRST | Hậu boss / save cũ | Art |',
              '|---|---|---|---|---|---|---|---|']
    source = ['# Phase4 — fragment source matrix', '', 'Generated from runtime CulturalRelics.json; [access matrix](ARTIFACT_FRAGMENT_ACCESS_MATRIX.md) · [QA](../QA/PHASE4_SIX_RELICS_E2E.md).', '', '| Fragment | Relic | Nguồn | Claim bổ sung | Proof | Runtime |', '|---|---|---|---|---|---|']
    for f in fragments:
        owner = next(r for r in relics if f['id'] in r['fragmentIds'])
        anchors = ', '.join(f.get('claimAnchors', [])) or '—'
        art = 'ART_PENDING' if f.get('artStatus') == 'ART_PENDING' else 'IMPORTED; VISUAL_PENDING'
        matrix.append(f"| {owner['id']} | {f['id']} · {f['name']} | {f['cityId']}:{f['nodeId']}; {anchors} | {proof(f)} | PASS | PASS | Hai save 29 quest: thao tác thật, không thưởng lại PASS | {art} |")
        source.append(f"| {f['id']} | {owner['id']} | {f['cityId']}:{f['nodeId']} | {anchors} | {proof(f)} | IMPLEMENTED_WINDOWS_PASS; VISUAL_PENDING |")
    matrix += ['', '## Điều kiện và nguồn bằng chứng', '',
               '- Runtime: `Assets/_Game/Resources/Content/CulturalRelics.json`, `RelicCollection`, `RelicActivity.Trials`, NodeEventManager; proof nguồn cũ nằm trong GameplayContent/PuzzleManager/QuestManager hiện có.',
               '- Claim mới tại H0/H10 (pilot), và giao lưu lời chào ở J2 là hoạt động tactile riêng đã chạy thật; không giả rằng chúng là reward có sẵn trước đây.',
               '- Dấu Lời Chào có provenance H6; nhận qua ảnh H6 thật **hoặc old check-in**. J2 là claim anchor mới; không buộc HQ05/HQ07/DQ05 và không thưởng/căn ảnh H6 lại.',
               '- G7 dùng guard_resolved, battle hoặc repair cùng hợp lệ. Nhánh sửa không cần defeatedEnemies; nhánh đánh không cần stone_guard_friend.',
               '- H2 reflection chỉ completed:mai-lantern. H3 pickup không đủ. Hội An ritual vẫn cần HQ01 completed + H7, không đổi world unlock.',
               '- Hạt xanh lấy tại D12, văn khắc tại U17; không giữ mảnh đến Seoul/G14. Ba icon Seoul không tương ứng vẫn ART_PENDING.',
               '- Nguồn one-shot cũ đã giải vẫn cho claim collection riêng từ proof đã lưu; earned old post-boss saves giữ nguyên 29 quest, items, coins, choices và credits history. Hai route mới đi từ fresh H0 không tiêm progression flags.',
               '- Idempotence: relic:v1:claim:<fragmentId>; source proof và claim flags cùng transaction. Không dùng số item còn lại làm nguồn duy nhất. Schema v1/v2 giữ nguyên; không nâng version.',
               '- Sau 4/4: assembly thật → trial riêng → explicit confirm → restored. Hội An tại H2, Huế U7, Đà Lạt D2, Jeonju J2, Gyeongju G9, Seoul S7.',
               '- Đây là bảng hiện hành thay trạng thái NOT_IMPLEMENTED của Phase0. Các QA/lựa chọn lịch sử không bị sửa. Phase5 ritual/ending vẫn pending.']
    # Keep the original detailed canonical metadata (event/NPC/quest/checkin IDs).
    # SOURCE_MATRIX is the compact generated view; do not discard approved fields.
    access_path = ROOT / 'Docs/Gameplay/ARTIFACT_FRAGMENT_ACCESS_MATRIX.md'
    original = [line for line in access_path.read_text(encoding='utf-8-sig').splitlines() if line.startswith('|')]
    header = [cell.strip() for cell in original[0].strip('|').split('|')]
    old_rows = [[cell.strip() for cell in line.strip('|').split('|')] for line in original[2:]]
    detailed = {row[header.index('fragmentId')]: dict(zip(header, row)) for row in old_rows}
    access = matrix[:3] + ['', '| ' + ' | '.join(header) + ' |', '| ' + ' | '.join('---' for _ in header) + ' |']
    for f in fragments:
        row = detailed[f['id']]
        owner = next(r for r in relics if f['id'] in r['fragmentIds'])
        row.update({
            'requiredQuest/StoryFlag': proof(f),
            'original item vs collection evidence': 'Collection evidence mới bằng thao tác thật; không tiêu/nhân đôi story item hoặc reward nguồn cũ',
            'earliestAcquire': 'City mở + node REVEALED + proof cột bên + gesture/xác nhận; ' + ', '.join(f.get('claimAnchors', [])),
            'earliestRestoration': owner['restorationNode'] + ':4mảnh +4drop assembly + distinct trial + explicit confirm' + ('; HQ01/H7 thật bắt buộc' if owner['cityId'] == 'hoi_an' else '; không cần relic khác hoặc optional food quest'),
            'canAcquireVietnamFirst': 'PASS: earned fresh VN_FIRST',
            'canAcquireKoreaFirst': 'PASS: earned fresh KOREA_FIRST',
            'canAcquireAfterBoss': 'PASS:24 actual gestures trong cả hai historical29quest saves',
            'oldSaveRetroactive': 'PASS:source proof đã lưu, collection claim mới; exact story inventory/coins/quests/choices',
            'duplicateGrantRisk': 'PASS:single relic:v1:claim ledger; không reward nguồn lần hai',
            'lockLoopRisk': 'Không tạo relic→world/boss gate; hai route/retro tested, không terrain certification',
            'sourceEvidence': 'GameplayContent.json existing anchor/proof + CulturalRelics.json/' + f['id'] + '; actual Phase4 UI journey in QA report',
            'status': 'IMPLEMENTED_WINDOWS_PASS; TWO_ROUTES_OLD_SAVE_PASS; VISUAL_PENDING' + ('; ART_PENDING' if f.get('artStatus') == 'ART_PENDING' else ''),
            'runtimeImplemented': 'True',
        })
        if f['id'] == 'fragment_jeonju_greeting_stamp':
            row['proposedInteraction'] = 'IMPLEMENTED: H6 provenance; actual photo OR old check-in proof, tactile claim H6 hoặc Hana J2; không thưởng H6/food lại'
        if row['sourceType'] == 'NEW_CONTENT_REQUIRED':
            row['sourceType'] = 'NEW_AUTHORED_TACTILE_CLAIM_AT_EXISTING_ANCHOR'
        access.append('| ' + ' | '.join(row.get(k, '—') for k in header) + ' |')
    access += matrix[matrix.index('## Điều kiện và nguồn bằng chứng') - 1:]
    write('Docs/Gameplay/ARTIFACT_FRAGMENT_ACCESS_MATRIX.md', '\n'.join(access))
    write('Docs/Gameplay/ARTIFACT_FRAGMENT_SOURCE_MATRIX.md', '\n'.join(source))

    city_rows = [
        ('hoi_an', 'Đèn Mắt Phố', 'H0/H4/H10/H2', 'H2', 'Dẫn Ánh Đèn cũ; cần H7/HQ01', 'A/B/C;10 halos giữ nguyên'),
        ('jeonju', 'Mộc Ấn Hanji', 'J2/J3/J7 + proof H6', 'J2', 'Xoay4 lớp hoa văn đúng mẫu', 'A/C; đèn xưởng + dải giấy'),
        ('hue', 'Ấn Rồng Ngọc Tỉ', 'U3/U8/U16/U17', 'U7', 'Rà4 nét + căn hướng mặt ấn', 'A/C; điểm sáng hai bờ'),
        ('da_lat', 'La Bàn Sương Lam', 'D0/D2/D8/D12', 'D2', 'Vuốt sương + tìm4 dấu + căn Đông Bắc', 'A/C; tan lớp sương trang trí'),
        ('gyeongju', 'Kính Ấn Tinh Tú', 'G3/G4/G7/G15', 'G9', 'Rà kính + đọc3 dấu, loại mồi + căn Tây', 'A/C; sao di tích, G7 khác màu theo nhánh'),
        ('seoul', 'Lăng Kính Tam Relay', 'S2/S7/S10/S18', 'S7', 'Kéo3 luồng + xoay hướng, hội tụ hình học', 'A/C; tín hiệu3 relay dạng biểu tượng'),
    ]
    states = ['| City / di vật | Nguồn | Điểm phục dựng | Trial thật | Saved collected / assembled / trial / restored | World Reaction |', '|---|---|---|---|---|---|']
    for city, name, nodes, node, trial, reaction in city_rows:
        states.append(f'| {city} · {name} | {nodes} | {node} | {trial} | 4/4 · true · true · true trong earned final | {reaction} |')
    states_text = '\n'.join(states)
    files = [
        'Content/RelicCollection.cs', 'Content/RelicActivity.Trials.cs', 'Content/NodeEventManager.cs',
        'UI/GameplayPanels.cs', 'UI/GameplayPanels.Relics.cs', 'UI/GameplayPanels.CulturalTrials.cs',
        'UI/GameplayPanels.CheckIns.cs', 'UI/RelicSurveySurface.cs', 'UI/InventoryPanel.cs',
        'Maps/CityMap.RelicLight.cs', 'Maps/CityMap.CulturalRestoration.cs',
        'Validation/GameplaySmokeRunner.cs', 'Validation/GameplaySmokeRunner.Phase4.cs', 'Validation/GameplaySmokeRunner.Phase4Integration.cs',
    ]
    file_text = '\n'.join('- `Assets/_Game/Scripts/' + f + '`' for f in files) + '\n- `Assets/_Game/Resources/Content/CulturalRelics.json`\n'
    write('Docs/Gameplay/Phase4-20261018/changed-files.json', json.dumps(dict(codeAndData=['Assets/_Game/Scripts/' + f for f in files] + ['Assets/_Game/Resources/Content/CulturalRelics.json'], artModified=[], note='Matching new .meta files and current Docs/Tools included in final incremental manifest; no source PNG/meta binding modified.'), indent=2))
    key_text = '''| Mốc | Hành vi thực tế / bằng chứng |
|---|---|
| ID / tên | link_key · Chìa Kết Nối; không tạo travel key khác |
| Trước H5 |0 key, Huế/Jeonju khóa; earned private save + restart |
| H5 battle / hack | Mỗi nhánh được đúng1; max-merge drop + encounter:hoi_an:enemy_mo01:link_key ledger cũ, không nhân đôi khi đổi lựa chọn |
| HQ01 / H7 | Existing consumeItems=true: dùng lamp_eye + hanji_core + link_key một lần khi xác nhận H7;150xu reward cũ một lần |
| Inventory trước H7 | Mô tả đích dùng H7, không nút Dùng giả; H5 không mở WorldMap |
| Sau H7 fresh | Key0; dialogue xác nhận đã dùng; Huế và Jeonju mở theo hoi_an.lamp_restored, Enter không đòi key |
| Old retained key | Fixture tương thích có1 key sau H7: giữ item, mô tả Đã sử dụng/kỷ vật; không thu lại hoặc tự cấp mới. Đây là fixture riêng, không phải earned reward |
| Old missing key | Earned hậu boss key0 được vào mọi city; không re-grant, không khóa lại |
| Reload |6 restart cases before-H5/after-H5/after-H7 cho cả hai nhánh + retained fixture PASS, save/coins/ledger exact qua đọc UI |
| City sau | U19→Đà Lạt; J19→Gyeongju;4beacon→Seoul. Source code/whole GameplayContent exact; earned E2E không key, không relic gate mới |'''
    report = f'''# Phase4 — five relics implementation

**WINDOWS_TARGETED_PASS · VISUAL_PENDING · PHASE5_PENDING · STOP sau Phase4.**18/10/2026. [Runtime review](../QA/PHASE4_RUNTIME_REVIEW.html) · [QA chi tiết](../QA/PHASE4_SIX_RELICS_E2E.md).

Prompt chính thức: [merged Phase4](../Prompts/LightBetweenCultures_Phase4_MERGED_Relics_Reactions_LinkKey_20261018.md), đọc toàn bộ từ attachment và lưu nguyên văn vì file project chưa có. Đã kiểm tra checkpoint18 check-in hoàn tất trước khi bắt đầu; base CheckpointHoiAnLighting-20261018 (manifest7fcbfb540f27597eeafa224af66d01da072cc370117b294ab722f1f78af47723). Small before-snapshot: BeforePhase4Relics-20261018, không full backup.

## Đã triển khai thực tế

{states_text}

Một RelicCollection,6 relicId,24 fragmentId. Có nguồn proof thật, thao tác nhận độc lập, assembly kéo4 phần, trial sai/hint/retry/exit/resume và xác nhận thành công. Assembled không đồng nghĩa restored. Phần đặt/điều hướng chưa xác nhận của trial là session tạm; save chỉ giữ collection/assembly/kết quả, mở lại thử lại không mất mảnh. Prism dùng tuyến trong minigame thật, kiểm heading/endpoint/bounds/intersection; không chỉnh waypoint CityMap.

## Story, giao lưu và check-in

- Jeonju Hana J2: H6 Greeting Stamp thật/old mark có thể ghi nhận mới, không buộc giao ớt hoặc dâu và không cấp lại thưởng H6. J15 chỉ nhắc ớt khi jeonju.chili_shared thật.
- Huế Lê An U7/Bác Tâm và các điểm liên quan: trước/đang thu/4mảnh/assembled/restored. Nhắc văn khắc Gyeongju chỉ khi đã thật sự đọc (gyeongju.core_rhythm_known); không giữ mảnh U17 đến G14.
- Đà Lạt Lan D2: lời nhắc kính và hướng sương, D12 cấp nguồn không đợi Seoul. Gyeongju G9/G14/G7: nguồn tượng nhận được ở cả battle/repair, trạng thái màu/lời thoại khác theo lựa chọn cũ.
- Seoul S2/S7/S11/relay: hồ sơ, báo cáo và3 puzzle cũ làm proof; ba luồng văn hóa là biểu tượng, không gán nhầm3component. Sau boss/6di vật chỉ acknowledgement, không ceremony hoặc credits mới.
- Side narration được lưu trong CulturalRelics.json, append vào hội thoại gameplay thật; giữ29 quest/22 puzzle và nội dung quest gốc. NPC hiện diện/portrait binding cũ giữ nguyên;4 imported NPC vẫn PENDING_VISUAL_APPROVAL.
- **0 check-in/postcard mới**: tái dùng18 entry và18ảnh đã duyệt. Năm city có narration A/C trong caption và Photo Journal; caption dài cuộn riêng, không che nút. Album từ city khác đọc đúng relic của bưu thiếp, không dùng state city đang đứng. H6/Hội An caption/2symbol/ảnh căn khớp/ledger nguyên vẹn.

## World Reactions: một hay hai nấc

Hội An tiếp tục **A/B/C** từ mai-lantern và relic restored: H3 pickup chưa trả vẫnA; H2 returnedB; H7 cònB; valid Đèn Mắt PhốC. Năm city chỉ **A/C**: quest/puzzle/beacon/4mảnh/assembly chưa tạoC. Không dim toàn map40% ở năm city; canonical giữ nguyên. C là layer con map, non-raycast, chỉ gắn tại node REVEALED; fade1.8s lần đầu, reload/re-entry hiệnC ngay không celebration/reward lại. Đà Lạt chỉ bỏ sương trang trí mới, không đụng fog-of-war/collider/bridge/painted fog. Tất cả artwork/visual vẫn chờ người dùng nghiệm thu, không tự VISUAL_APPROVED.

## Chìa Kết Nối H5–H7

{key_text}

Không đổi backend, consume flag, quest condition, reward, key ID hoặc world unlock. Chỉ patch dialogue/mô tả Inventory đúng lifecycle thực tế.

## File code/data sửa hoặc thêm

{file_text}
Docs: access/source matrices, unlock decisions, progression design, FULL_STORY_PLAYTEST, báo cáo này, QA/runtime gallery, SIX_RELICS_ART_MANIFEST. Tools: phase4_checkpoint/validation/verify/report; new source .meta trong manifest. **Không sửa PNG nguồn/map, asset binding, canonical registry, scene/prefab, save schema hoặc quest graph.**

## QA và checkpoint

Hai hành trình fresh VN_FIRST/KOREA_FIRST đi thật H0→H3→H7→các beacon→24/24→6/6→boss repair/battle→backtrack sáu city PASS. Fresh core route không cố hoàn tất mọi side quest; hai save earned29quest lịch sử bổ sung QA hậu boss/old proof. Cả hai old saves nhận24mảnh bằng thao tác mới, giữ exact inventory/coins/29quests/choices/non-relic world history và old ledger +24claim, không replay boss/credits. RestartA/C mỗi city, hai nhánhG7, H5battle/hack và key restart PASS. {len(preservation['checks'])} preservation checks exact bao gồm47protectedfiles + H6 functions/HoiAn data.

Windows build: Logs/phase4-final-build02.log Succeeded. Unity6000.6.5f1 Development Windows, không release/Android certification. Android build/device, FPS/VRAM profiling, mute/audio-device acceptance, v1 historical-file process test: **NOT_TESTED**; v1/v2 SaveSystem source không đổi. Synthetic touch là PC InputSystem, không Android touch device.

City checkpoints đã hoàn tất tuần tự Jeonju→Huế→Đà Lạt→Gyeongju→Seoul, sau đó final checkpoint nhỏ kế thừa Seoul. Tên/base/hash/file-count trong Phase4-20261018/last-checkpoint.json. Không copy Library/Temp/Builds/historical Validation; chỉ file thay đổi và evidence hiện hành. Thất bại QA được giữ, không sửa lịch sử thành PASS.

## Pending / mức độ

- **P1 / PHASE5_PENDING (ngoài phạm vi):** legacy manualS21 vẫn có thể phát credits theo flow cũ. Phase4 không tự phát credits/SQ03/global completion; chưa triển khai guard boss resolved AND6restored AND nghi thức Seoul. Old ending_seen vẫn là lịch sử, không nghi thức mới. Không gọi game hoàn tất chỉ vì6/6.
- **P2 ART_PENDING:**3Seoul icons đúng nghĩa: heritage_data, jiwon_sync, stellar_ring; slot trung tính có nhãn. Full prism/frame giữ art hiện có.
- **P2 NEEDS_CROP_REVIEW:** sliver trên3component Jeonju và relay frame Seoul. Không re-slice trái phạm vi.
- **P2 VISUAL_PENDING:** mức glow/fog/landmark,5trial presentation,4importedNPC cần chủ dự án nghiệm thu hình. Đặc biệt Curator/Inscription có nét mặt tương đồng: khác file/trang phục chưa xác nhận được hai danh tính từ ảnh; gallery có cả hai để đối chiếu. Không tự approve canonical hoặc tự thay mặt.
- **NEEDS_AUDIO:** chưa có bộSFX phục dựng riêng được kiểm duyệt; không tải/sinh audio không rõ giấy phép. Feedback visual hoạt động, không tự tuyên bố audio hoàn chỉnh.
- **Ngoài phạm vi giữ nguyên:**14node quá3nhánh và terrain NEEDS_USER_REVIEW từPhase3.5; không sửa graph/đường trong task này. Android/performance chưa kiểm nghiệm thiết bị.

**DỪNG sau Phase4.** Không Phase5/6/7, không tự chuyển task khác. Người dùng duyệt hình/runtime và cung cấp3icon Seoul trước các bước art final; ending chờ lệnh Phase5 riêng.
'''
    write('Docs/Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md', report)

    # Replace only our active Phase4 prefixes/current-status text; retain historical chapter design.
    story_path = ROOT / 'Docs/Story/FULL_STORY_PLAYTEST.md'
    story = story_path.read_text(encoding='utf-8-sig')
    story = story[story.index('# LIGHT BETWEEN CULTURES — FULL STORY PLAYTEST'):]
    start = story.index('**Trạng thái hiện tại')
    end = story.index('\n\n', start)
    story = story[:start] + '**Trạng thái hiện tại (Phase4,18/10/2026):**29 quest/22 puzzle/133node giữ nguyên. Sáu di vật/24mảnh có gameplay thật và side dialogue runtime; hai thứ tự fresh và old-save hậu boss được kiểm thử. Hội An A/B/C; năm city A/C chỉ sau trial hợp lệ. **Nghi thức hội tụ và guard ending duy nhất vẫn PHASE5_PENDING**, legacyS21 chưa đổi; phần mô tả lễ hội cũ bên dưới không phải chứng nhận ending mới.' + story[end:]
    prefix = '# Phase4 — story runtime delta\n\n' + states_text + '\n\nH2 tutorial chỉ ánh sáng cục bộ; H7/HQ01 chỉ beacon/world unlock; Đèn Mắt Phố mới full-city restoration. Các proof giao lưu đã triển khai: H6→Hana J2, actual chili_shared tại J15, văn khắcG14 đã đọc đối chiếuU17, ba nguồn văn hóa trong Lăng Kính. Không đổi quest graph/29quest/22puzzle hoặc vật phẩm story.\n\nRuntime: CulturalRelics.json.story → NodeEventManager/RelicCollection.Story → dialogue/check-in caption thật. Trước/partial/4of4/assembled/restored/revisit theo state đã lưu; lời phản hồi không tự trao thưởng. [Phase4 report](../Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md) · [runtime](../QA/PHASE4_RUNTIME_REVIEW.html).\n\n---\n\n'
    write('Docs/Story/FULL_STORY_PLAYTEST.md', prefix + story)
    design_path = ROOT / 'Docs/Gameplay/SIX_ARTIFACT_PROGRESSION_DESIGN.md'
    design = design_path.read_text(encoding='utf-8-sig')
    design = design[design.index('# Six Artifacts —'):]
    write('Docs/Gameplay/SIX_ARTIFACT_PROGRESSION_DESIGN.md', '# Phase4 — implemented progression / persistence\n\n' + states_text + '\n\nMột RelicCollection, flags relic.proof/fragment/assembled/trial/restored + ledger relic:v1:claim dùng savev1/v2 hiện có, atomic rollback khi persist thất bại, không field/schema mới.24nguồn đã hoạt động và tested hai route/old-save; proofAny thay các literal OR ở metadata dormant. Trial chỉ thay restored sau đúnginput+explicitconfirm. Source one-shot cũ không phải replay reward; claim mới độc lập.\n\nA Adventure = quest/beacon/world/boss cũ, không relic gate. B Collection =24→6restored đã implement. C Completion = boss +6 +nghi thức cuối **Phase5 chưa implement**; legacyS21 còn pending. Các đề xuất/alternatives Phase0 dưới đây là lịch sử, bị quyết địnhD01–D08/mergedPhase4 và runtime phía trên supersede. [Current access](ARTIFACT_FRAGMENT_ACCESS_MATRIX.md) · [QA](../QA/PHASE4_SIX_RELICS_E2E.md).\n\n## Historical Phase0 design (không phải runtime hiện hành)\n\n' + design)
    decisions = (ROOT / 'Docs/Gameplay/ARTIFACT_UNLOCK_DECISIONS.md').read_text(encoding='utf-8-sig')
    decisions = decisions.replace('Phê duyệt trực tiếp của chủ dự án: **Phase 1–3**, dừng nghiệm thu Hội An; chưa Phase 4–7.', 'Phê duyệt D01–D08 giữ nguyên. **Phase4 merged đã được cho phép và triển khai năm di vật còn lại; DỪNG nghiệm thu Phase4. Phase5–7 chưa cho phép.**')
    decisions = decisions.replace('Các phase sau chỉ có metadata và art chuẩn bị, không runtime claim/trial mới cho năm thành phố.', 'Năm city đã có runtime claim/assembly/trial/A-C theo mergedPhase4;3icon Seoul và visual approval vẫn pending. Ending ceremony chờPhase5. Xem PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md.')
    write('Docs/Gameplay/ARTIFACT_UNLOCK_DECISIONS.md', decisions)

    art_path = ROOT / 'Docs/Art/SIX_RELICS_ART_MANIFEST.md'
    art = art_path.read_text(encoding='utf-8-sig')
    art = art.replace('Five cities only have data/art preparation in Phase2, not playable new relic sources/trials/restoration.', 'Phase4 current: five cities now have playable sources/trials/restoration, Windows technical PASS; no additional art imported/re-sliced. Visual/crop approval remains separate.')
    art = art.replace('Native crops của17 mảnh thuộc năm city khác chưa được chơi/duyệt riêng; chỉ import/binding chuẩn bị.', 'Historical Phase1–3 scope:17 matched foreign components were only preparation then. Phase4 now renders them through real source/assembly/trial UI; gameplay Windows PASS, art VISUAL_PENDING and listed slivers NEEDS_CROP_REVIEW.')
    write('Docs/Art/SIX_RELICS_ART_MANIFEST.md', art)
    checkpoint_path = ROOT / 'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md'
    checkpoint_text = checkpoint_path.read_text(encoding='utf-8-sig')
    marker = '> **Hội An lighting revision'
    checkpoint_text = checkpoint_text[checkpoint_text.index(marker):]
    write('Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md', '> **PHASE4 WINDOWS_TARGETED_PASS · VISUAL_PENDING · STOP.** Năm di vật đã implement theo merged prompt,24/24nguồn/6trial/6restored, hai tuyến fresh +hai old post-boss +keybothbranches/restart PASS. Hội An A/B/C, nămcity A/C;18check-in/H6/133node/155edge/29quest/22puzzle/save/waypoint exact.3Seoul art +4NPC visual pending; Phase5 ending/legacyS21 pending. Final incremental `CheckpointPhase4Final-20261018` requires `CheckpointPhase4Seoul-20261018`; exact receipt in `Phase4-20261018/last-checkpoint.json`. [Report](PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md) · [QA](../QA/PHASE4_SIX_RELICS_E2E.md) · [Runtime gallery](../QA/PHASE4_RUNTIME_REVIEW.html). Không tự chuyển Phase5/6/7 hoặc task khác.\n\n## Checkpoint history (các lệnh DỪNG cũ đã được user thay bằng phê duyệt Phase4)\n\n' + checkpoint_text)

    qa_rows = ['| Run | Result | PASS assertions (gồm UI/capture checks) | Log |', '|---|---|---:|---|']
    for r in runs:
        qa_rows.append(f"| {r['run']} | {r['result']} | {r['checks']} | [result](../../{r['resultPath']}) |")
    qa_report = '''# Phase4 — Six Relics E2E / Windows QA

**WINDOWS_TARGETED_PASS; VISUAL_PENDING; Android NOT_TESTED; Phase5 pending.** Các dòng PASS là kiểm tra logic/UI/runtime có phạm vi ghi dưới đây, không phải tự nghiệm thu độ đẹp hoặc toàn bộ133node.

## Hành trình và đầu vào

- e2e-vn01: freshH0→tutorial thật/H3pickup/return→H7→Huế→ĐàLạt→Jeonju→Gyeongju→Seoul; G7repair, bossSeoulrepair.24/24+6/6 trước boss, boss không tựSQ03/ending; backtrack6cityC không replay celebration.
- e2e-korea03: freshH0→Jeonju→Gyeongju→Huế→ĐàLạt→Seoul; G7battle, Seoulbossbattle2pha. Cùng kết quả collection, không gate relic, không link_key ở city entry. Không ép hoàn tất mọi sidequest trong fresh core routes.
- retro-vn01: bản sao nguyên trạng Validation/FullStory-20261017/Fresh04/full-story-final.json,29quest/credits/boss thật từ lịch sử; retro-korea01 từ Reverse03 tương đương. Input-provenance.json ghi hash, không ghi file gốc. Mỗi route đi24nguồn với pointergesture mới; oldH6 mark dùng J2 không thưởng lại. Exact coins/items/quests/choices/non-relicflags; ledger cũ+24claim; boss không respawn và credits không replay.
- City source journeys dùng checkpoint earned nối tuần tự, không setflag để đi qua gate. QA chọn đúng đáp án canonical cũ để tạo source proof, sau đó thực hiện claim/assembly/trial thật. Survey dùng InputSystem syntheticmouse/touch trong PlayerWindows.
- key-battle01/key-hack01 từ earned stateB trướcH5; actual resolution và H7 consume/reward/WorldMap đều chạy.6processrestart trước/sauH5/H7; retained-key fixture chỉ thêm1link_key vào bản sao earnedafterH7 để kiểm compatibility UI, **không dùng làm chứng cứ nhận thưởng/progression thật**.
- presentation01 từ earned fresh-final;5NPC/5caption+4pendingNPC+foreignalbum/touch. Caption scroll chỉnonHoiAn, H6 untouched.
- hue-culture02 từ assembled checkpoint của earned retro-korea01: G14 đã thực sự đọc, phục dựng Huế và kiểm actual cultural branch U7 sau restored; không tiêm cờ gặp NPC hoặc replay nguồn cũ.

## Kết quả từng lượt

''' + '\n'.join(qa_rows) + '''

Không cộng số assertions thành coverage133node. Các lượt city trước finalbuild vẫn hợp lệ cho logic/geometry không đổi; finalbuild bổ sung Huế cultural wording/actualG14 condition, caption scroll và QA chờ panel. Không dùng seoul01 ảnh tia đầu làm nghiệm thu geometry cuối; dùng seoul02 và current E2E. Không xóa failed evidence.

## Failures phát hiện và xử lý

- jeonju01: QAhelper giả định H6 CheckInRecord; sửa helper gọi existing2-symbolphoto, không sửa H6.
- e2e-korea01: restored Hue cultural branch chưa ghi rõ “phục dựng”, condition nhắcGyeongju dùngguard quá rộng. Sửa side-data dùng actualG14read +wording; không sửa quest/relic proof.
- e2e-korea02: automaticEventClose pointer lấy vị trí lúc entrance còn trượt, cúbấm hụt. Thêm wait.35s chỉQAphase4 trước đo target. e2e-korea03 chạy lạifresh; không mask assertion/forceclose/gameplayflags.
- Seoul initialrender:3rays chưa hội tụ hìnhảnh. Sửa minigame polylines và physicalvalidator bounds/heading/end/intersections; seoul02/current journey passed. CityMap geometry không đổi.

## Bảo toàn / giới hạn

56 preservation checks PASS:47exact files (wholeGameplayContent,SaveSystem/QuestManager/GameBootstrap/EncounterManager,CityMap/Paths/Geometry/QuestReaction,lightingdata,AdventureUIArt,18PNG+18meta), HoiAnrelic+4fragments+story exact, H6 FinishPhoto/ShowRelicPhoto/ShowLegacyCheckIn/captionbranch exact, counts133/155/29/22/18/24/6. Generatednativeexport merge theoAGENTS trướccheckpoint; không PNG/GUID/portrait đổi. Scene/prefab không sửa.

RestartA/C mỗiforeigncity:4fragment+assembly retained, restoredC không celebration/reward lại; chưarestoredA không auto từquest. HoiAnABC freshcaptures gồm nhặtchưatrảA,H2B,H7stillB,Cđúngtrial. Zoom/pan/pinch/Inventory UI không mutate save. Không chạy lại full133-node regression hoặc266WorldReactions assertions vì dependency không đổi; không tuyên bố terrainPASS từ lượt này.

Windows build **Logs/phase4-final-build02.log: Succeeded**. Android build/device **NOT_TESTED**; physicaltouchdevice/FPS/VRAM/SFX acceptance/v1historical-file process **NOT_TESTED**. Engine shutdown ComputeBuffer/PlayerConnection warnings xuất hiện ở vàiWindowsdevelopment runs, không có gameplay exception ở successful result; chưa performance-certify.

## Hạng mục chưa đạt / cần duyệt

P0 mới: không phát hiện trong phạm vi. P1ngoàiphạmvi: manuallegacyS21credits cầnPhase5guard, giữhistorySQ03/ending_seen. P2:3Seoul iconsART_PENDING,4nativecrop sliversNEEDS_CROP_REVIEW,4NPC +restoration visual chưaduyệt. CặpCurator/Inscription cần review trực quan danh tính vì nét mặt tương đồng; khác binding không phải identity approval, không tự đổi ảnh. NEEDS_AUDIO cho cuephụcdựngriêng.14overdegree/terrainreview từPhase3.5 giữnguyên, không thuộcPhase4. Không tự công bốrelease-ready hoặcGAME_COMPLETED.

**DỪNG Phase4:** [implementation](../Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md) · [24sources](../Gameplay/ARTIFACT_FRAGMENT_ACCESS_MATRIX.md) · [runtime images](PHASE4_RUNTIME_REVIEW.html). Không Phase5/6/7.
'''
    write('Docs/QA/PHASE4_SIX_RELICS_E2E.md', qa_report)

    sections = []
    def image_group(title, paths, note=''):
        cards = []
        for path, label in paths:
            p = QA / path
            assert p.is_file(), p
            url = '../../' + p.relative_to(ROOT).as_posix()
            cards.append(f'<figure><a href="{html.escape(url)}"><img loading="lazy" src="{html.escape(url)}" alt="{html.escape(label)}"></a><figcaption>{html.escape(label)}<br><code>{html.escape(path)}</code></figcaption></figure>')
        sections.append(f'<section><h2>{html.escape(title)}</h2><p>{html.escape(note)}</p><div class="grid">' + ''.join(cards) + '</div></section>')
    image_group('Hội An A / B / C — fresh Phase4 runtime', [(f'e2e-vn01/{name}.png', label) for name, label in [('A-before-tutorial','A — chưa trả Mắt Đèn'), ('B-mai-returned','B — tutorial thực sự completed'), ('C-full-city-restored','C — Đèn Mắt Phố restored')]], 'Cùng1280×960; H7-before-relic vẫnB. Không dùng ảnh từ task lighting cũ.')
    image_group('Hội An H7 vẫn B / góc rộng B', [('e2e-vn01/B-after-H7-before-relic.png','B — sauH7 chưarestored'), ('e2e-vn01/B-wide-district.png','B — góc rộng1600×1000')])
    accepted = {'jeonju':'jeonju03', 'hue':'hue-culture02', 'da_lat':'dalat01', 'gyeongju':'gyeongju01', 'seoul':'seoul02'}
    for city, name, *_ in city_rows[1:]:
        run = accepted[city]
        image_group(city + ' — A / C', [(f'{run}/P4-{city}-A-before-restored.png','A — assembled, chưatrialthànhcông'), (f'{run}/P4-{city}-C-restored.png','C — explicittrialsuccess/restored')], 'Cùng1280×960, focus(0.5,0.5),zoom1; quest/items/coins/worldnonrelic state giữ nguyên giữaA/C. VISUAL_PENDING.')
        detail = f'{run}/P4-{city}-four-fragments.png'
        if not (QA / detail).is_file():
            detail = f'{run}/P4-{city}-collection.png'
        image_group(name + ' — trial / collection', [(f'{run}/P4-{city}-trial-correct-before-confirm.png','Trial: đúnginput, chưaxácnhậnchưađượcrestored'), (detail,'Collection progress; Seoul3iconART_PENDING')])
    for city in accepted:
        image_group(city + ' — runtime dialogue / check-in caption', [(f'presentation01/P4-Presentation-{city}-NPC.png','NPC + trạngtháidi vậtđãlưu'), (f'presentation01/P4-Presentation-{city}-checkin-caption.png','Captioncuộntrênmànhìnhdọc;khôngthưởng')])
    image_group('Bốn NPC imported — PENDING_VISUAL_APPROVAL', [(f'presentation01/P4-Presentation-{npc}.png',npc) for npc in ['npc_keeper','npc_inscription','npc_curator','npc_engineer']], 'Giữđúngbindingcũ,khôngtựchốtngoạihìnhcanonical.')
    image_group('Chìa Kết Nối lifecycle', [('key-battle01/P4-Key-battle-before-H7-inventory.png','H5battle→key1 trướcH7'), ('key-hack01/P4-Key-hack-H7-used.png','H5hack→H7synchronizationused')])
    image_group('Save/re-entry sau boss', [('e2e-vn01/P4-E2E-VN_FIRST-revisit-hoi_an.png','HộiAnC sauquaylại'), ('e2e-korea03/P4-E2E-KOREA_FIRST-revisit-seoul.png','SeoulC/6of6,bossđãgiảiquyết,khôngautocredits')])
    image_group('Photo Journal từ Seoul xem Đà Lạt', [('presentation01/P4-Presentation-Seoul-view-Dalat-postcard.png','ĐúngcaptionLaBànSươngLam,khônggrant')])
    page = '<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC Phase4 runtime review</title><style>body{margin:24px;background:#112d35;color:#f9edce;font:16px/1.5 system-ui}a{color:#7de0ca}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(360px,1fr));gap:16px}figure{margin:0;padding:10px;background:#203f47;border-radius:12px}img{width:100%;height:auto;max-height:740px;object-fit:contain;background:#10242b}code{font-size:11px;overflow-wrap:anywhere}section{margin:40px 0}h1,h2{color:#ffd77b}</style><h1>Phase4 — runtime thật / VISUAL_PENDING</h1><p>Windows6000.6.5f1 Development. 24sources/6trials/6restored; không ending mới. Click ảnh mở nguyênkíchthước. Không mockup/AI hoặc ảnhQA task cũ. Android NOT_TESTED. <a href="PHASE4_SIX_RELICS_E2E.md">QA</a> · <a href="../Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md">Implementation / pending art</a></p>' + ''.join(sections) + '<p>DỪNG Phase4 để chủ dự án nghiệm thu; không tự VISUAL_APPROVED hoặc Phase5.</p></html>'
    write('Docs/QA/PHASE4_RUNTIME_REVIEW.html', page)
    print('Final reports generated from', len(runs), 'completed run results; required journeys PASS')


if __name__ == '__main__':
    main()
