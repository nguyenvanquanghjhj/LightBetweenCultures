"""Generate this task's reports from retained Windows Player evidence only."""
import html, json, re
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/Gameplay/Phase5-20261018'
QA = ROOT / 'Docs/QA'
RUNS = ROOT / 'Validation/Phase5-20261018'
def write(name, text):
    p = ROOT / name; p.parent.mkdir(parents=True, exist_ok=True)
    p.write_text(text, encoding='utf-8')

records = []
for folder in sorted(RUNS.iterdir()):
    result = folder / 'smoke-results.txt'
    if not result.is_file(): continue
    text = result.read_text(encoding='utf-8-sig')
    log = (folder / 'player.log').read_text(encoding='utf-8-sig')
    records.append({'run': folder.name, 'result': 'PASS' if text.splitlines()[-1] == 'ALL CHECKS PASSED' else 'FAIL', 'assertionsPassed': text.count('PASS:'), 'runtimeExceptions': len(re.findall(r'^\w*(?:Exception):', log, re.M)), 'shutdownComputeBufferDiagnostic': 'disposing of ComputeBuffer' in log, 'evidence': str(result.relative_to(ROOT)).replace('\\','/')})
required = ['matrix04','legacy-claim01','tail-vn01','tail-korea01','restart-ready02','restart-interrupted02','restart-completed02','e2e-vn01','e2e-korea01','no-relics02','restart-legacy-zero01','restart-legacy-six01','restart-pre-seoul01','restart-relay01','restart-mid-boss01','restart-battle01','restart-repair01']
assert all(any(r['run'] == name and r['result'] == 'PASS' for r in records) for name in required)
write('Docs/Gameplay/Phase5-20261018/run-summary.json', json.dumps({'acceptedRuns': required, 'allRetainedRunsIncludingFailures': records}, ensure_ascii=False, indent=2)+'\n')

cases = [
('E01','Boss chưa resolved, 0/6','matrix04','Fixture: backend và UI không mở ending.'),
('E02','Boss resolved, 0/6','no-relics02; matrix04','Boss repair thực, SQ03 tại S21, không credits; đi lại sáu thành phố.'),
('E03','Boss resolved, 3/6','matrix04','Fixture: báo tin SQ03, Collection 3/6; legacy ending/finish/skip bị chặn.'),
('E04','5/6 → restored mảnh cuối','matrix04','Fixture 5 restored; thực hiện trial Seoul thật → sẵn sàng, không đánh boss lại.'),
('E05','6/6 trước boss','matrix04; e2e-vn01; e2e-korea01','Fixture guard + hai tuyến fresh phải giải quyết S20 thật.'),
('E06','Boss + 6 + xác nhận','matrix04; tail-vn01; tail-korea01','Đủ 24 giây hoạt động hoặc explicit Skip sau confirm → một completion.'),
('E07','Restart sau boss, thiếu relic','restart-repair01; restart-legacy-zero01','Player process mới: boss không hồi sinh, không global ending.'),
('E08','Restart 6/6 chưa nghi thức','restart-ready02','Không auto animation/credits; người chơi có thể xác nhận sau.'),
('E09','Restart sau ending','restart-completed02','Giữ completion/ledger/inventory; World Map mở được, không tự credits.'),
('E10','VN_FIRST','e2e-vn01; tail-vn01','Fresh H0 → 24 mảnh/6 trial → G7 repair/S20 repair → S21/credits; final-build tail kiểm lại.'),
('E11','KOREA_FIRST','e2e-korea01; tail-korea01','Fresh H0 → nhánh Hàn trước → G7 battle/S20 battle → S21/credits; final-build tail kiểm lại.'),
('E12','Old save thắng boss','legacy-claim01','Save earned lịch sử, 0 relic/old credits: tương tác nhận mảnh H0 thật, không reset lịch sử.'),
('E13','Main story không relic','no-relics02','Save earned trước boss có 0 relic → hai pha/repair → SQ03 ở S21, không ending.'),
('E14','Fragment liên thành phố','legacy-claim01; hai E2E','Old H6 proof → J2 greeting-stamp bằng thao tác claim thật, ledger một lần.'),
('E15','Post-boss NPC/check-in','legacy-claim01; hai E2E','H6/Jeonju NPC vẫn tiếp cận, claim không đổi item/quest/choice/reward cũ.'),
('E16','Boss+6, chưa confirm, reload','restart-ready02; restart-interrupted02','Ready và interrupted đều không tự commit; phải chủ động bắt đầu lại.')]
write('Docs/Gameplay/Phase5-20261018/ending-cases.json', json.dumps([{'id':i,'case':c,'result':'PASS','runs':r,'evidenceLimit':d} for i,c,r,d in cases], ensure_ascii=False, indent=2)+'\n')
case_table = '\n'.join(f'| {i} | {c} | PASS | `{r}` | {d} |' for i,c,r,d in cases)
run_table = '\n'.join(f"| [{r['run']}](../../{r['evidence']}) | {r['result']} | {r['assertionsPassed']} |" for r in records)

write('Docs/QA/PHASE5_ENDING_E2E.md', f'''# Phase 5 — Windows ending QA

Chốt 19/10/2026 (Asia/Bangkok). **WINDOWS_TARGETED_PASS · VISUAL_ACCEPTANCE_PENDING · STOP**.

[Ảnh runtime có timestamp](PHASE5_RUNTIME_REVIEW.html) · [Implementation](../Gameplay/PHASE5_IMPLEMENTATION_REPORT.md) · [Protected verification](../Gameplay/Phase5-20261018/protected-verification.json).

## Phương pháp và giới hạn bằng chứng

Windows Player Development thật, Unity 6000.6.5f1, input mouse và Touchscreen simulation hiện có; mỗi restart là process mới và save QA riêng. Không chạm save người chơi. `e2e-vn01` và `e2e-korea01` bắt đầu fresh H0, tương tác/thu 24 mảnh/lắp ráp/6 trial/đánh hoặc repair boss thật; không tiêm progression flags. Chúng không có nghĩa mọi optional quest/check-in đều đã hoàn thành.

`matrix04` là targeted fixture rõ ràng từ bản sao save earned, chủ động tạo 0/3/5/6 restored và boss state để kiểm guard. Không gọi nó fresh playthrough. Lần restored Seoul cuối trong fixture vẫn dùng trial runtime thật. `legacy-claim01` dùng save earned trước hệ relic để kiểm nhận mảnh độc lập sau boss. `tail-*` dùng đúng save earned của hai E2E, không dựng state.

Hai E2E đầy đủ, zero-relic boss và các restart phụ thuộc cũ PASS trên build06. Build07 chỉ đổi clock nghi thức sang thời gian frame hoạt động (không tính pause/hitch) và caption trạng thái Collection; toàn bộ main/relic/boss/world logic giữ nguyên. Đã kiểm lại final build07 bằng matrix04, old-save claims, cả hai earned tails và ready/interrupted/completed process restarts. Không lặp toàn bộ 133 node cho thay đổi presentation này.

## E01–E16

| ID | Tình huống | Kết quả | Run | Bằng chứng/giới hạn |
|---|---|---|---|---|
{case_table}

## Các kiểm tra trực tiếp khác

- PASS: SQ03 hoàn tất một lần tại S21 với 0/3/5 restored; không thêm yêu cầu relic vào quest. Confirm ceremony ở S21 có thể xác nhận SQ03 nếu chưa báo tin, nhưng report riêng luôn độc lập.
- PASS: direct `ending`, `ending_next`, `ending_ack`, `ceremony_confirm`, `ceremony_finish`, `ceremony_skip` không vượt backend trước eligibility/session hợp lệ. Immediate Finish sau confirm bị từ chối.
- PASS: mở confirm rồi Close; đang ceremony rồi Close; pause 1 giây; process restart interrupted: không tự hoàn tất. Đồng hồ không tiến khi app suspended.
- PASS: natural ceremony 24 giây hoạt động, explicit Skip, repeated Finish/Skip, replay, Next/Skip xen kẽ mouse/touch; reward ledger/items/coins không tăng.
- PASS: old `game.ending_seen` với 0 và 6 restored không tự sinh completion mới. Các history flags/choices cũ giữ nguyên.
- PASS: pre-Seoul, relay, mid-boss, battle-resolved và repair-resolved saves không biến thành thắng boss/ceremony mới. Mid-battle retry theo semantics cũ, không thêm lưu enemy HP/phase.
- PASS: actual boss BGM chuyển Korea battle → Korea map sau repair; cả hai E2E kiểm nhánh battle/repair. Ceremony không tạo AudioSource, không đổi track hoặc settings.
- PASS: controls nằm trong khung hình portrait thực tế 540×960 và 540×1080. Request 540×1200 bị Windows giới hạn còn 540×1080; không tuyên bố đã kiểm trên 540×1200 thật hoặc thiết bị Android.
- PASS có giới hạn: H0/H3/H7/H6 và G7 hai lựa chọn được đi lại trong hai E2E; source H6/check-in/relic/lighting/movement giữ nguyên. Không chạy lại toàn bộ 18 artwork/gallery hoặc mọi đường terrain, không dùng source hash để gọi visual PASS.
- PASS: 364 bounded source checks, gồm 106 file bảo vệ byte exact; 6 cities/133 nodes/155 edges/29 quests/22 puzzles/24 fragments/6 relics/18 check-ins. [Chi tiết](../Gameplay/Phase5-20261018/protected-verification.json).

## Kết quả từng run (giữ cả lần lỗi)

| Run / log kết quả | Kết quả | Assertions PASS |
|---|---|---:|
{run_table}

Build01 FAIL compile vì Unity6 SceneHandle obsolete implicit int và helper QA không tồn tại; đã sửa và build02–07 thành công. Matrix01 FAIL do QA bấm liên tiếp trong input cooldown sau Close; sửa helper QA dùng cooldown hiện hữu, không sửa gameplay. No-relics01 FAIL vì QA cố rời mandatory S20 để rest; gate chặn đúng. Sửa hành trình QA dùng HP/tea earned hiện có, no-relics02 PASS. Giữ nguyên log/save/ảnh các lần lỗi, không đổi nhãn lịch sử.

## Build, diagnostics và trạng thái chưa nghiệm thu

Windows final build07: **Succeeded** ([log](../../Logs/phase5-build07.log)). Accepted runs không có managed runtime exception hoặc gameplay assertion FAIL. Unity/D3D12 có diagnostic khi shutdown: ComputeBuffer dispose, một buffer 64 KiB chưa release và MemoryLeaks telemetry khoảng 50–57 KiB; **chưa điều tra nguyên nhân, không gọi memory/performance PASS**.

Ảnh World Map sau credits cho thấy thumbnail Gyeongju/Seoul có nhiễu màu. Source WorldMap/art/bindings không đổi trong Phase5; chưa xác định nguyên nhân hay khẳng định lỗi có từ trước. Ghi P2 cần kiểm tra hiển thị trong task riêng, không tự sửa ảnh canonical.

**ANDROID_BUILD_NOT_TESTED · ANDROID_DEVICE_NOT_TESTED**. Nghe BGM bằng tai, FPS/VRAM trên thiết bị, cinematic thẩm mỹ: NOT_TESTED/PENDING tương ứng. Không tuyên bố Release Ready.
''')

write('Docs/Gameplay/PHASE5_IMPLEMENTATION_REPORT.md', '''# Phase 5 — One canonical ending

Chốt 19/10/2026 · **WINDOWS_TARGETED_PASS · VISUAL_ACCEPTANCE_PENDING · STOP**.

Đã triển khai ending thật: **boss resolved + sáu relic restored + nghi thức được xác nhận và hoàn tất hợp lệ**. Thiếu relic vẫn được hoàn thành boss/SQ03, đi World Map và quay lại thu mảnh; không credits sớm, không bắt đánh boss lại.

## Baseline và lỗ hổng đã sửa

Kế thừa source mới nhất sau Phase4, State B20, NPC map markers, Soft SFX, Four-track BGM và checkpoint route-reference review chỉ có docs. Pre-edit snapshot nhỏ: `D:\\GAMEKKK\\Backups\\BeforePhase5-20261018`. Base cuối là `CheckpointSixRouteReferenceReview-20261018`, không checkout source Phase4 cũ.

Trước sửa, S21 `final_festival` mở bảy trang ending/credits theo legacy `ending`/`ending_next`, gộp SQ03 với credits và chưa guard sáu restored. Đã bỏ dispatcher legacy đó trong NodeEventManager/GameplayPanels. Event S21 nay gọi partial cùng class, guard cả Execute và UI. Không thêm EndingManager/JournalManager/save system mới.

## Nguồn sự thật và persistence

- BossResolved dùng EncounterManager.Resolved của `seoul:enemy_final_core`: resolved encounter hoặc `seoul.final_cleared`, công nhận battle và repair; không chỉ đọc defeatedEnemies. S20/HP44+44/actions/relay/reward/repair nguyên vẹn.
- FinalRestoredCount hỏi RelicCollection.Restored trên sáu `relic_*` canonical và AllRestored, không dùng assembled/24 mảnh/beacon/check-in để thay restored.
- SQ03 vẫn complete tại S21 với điều kiện boss hiện hữu, action `report_victory`, một lần. Chỉ sửa offer/reminder/thanks, không sửa quest graph/rewards. Ceremony confirm tại S21 cũng có thể xác nhận SQ03 khi còn pending, không biến relic thành điều kiện SQ03.
- `CanStartFinalCeremony`, `CeremonyInProgress`, `CanonicalEndingCompleted` tách riêng. Confirmation/session gắn đúng Seoul/S21/CityMap/scene hiện tại, discovery visible, không tự bắt đầu khi đến node/load/Collection.
- Chỉ thêm flag **`game.canonical_ending_completed.v1`** vào flags v1/v2 đang có; Persist atomic hiện hành, rollback flag trong RAM nếu ghi thất bại. Không schema mới/migration/ready flag/duplicate relic state.
- `game.ending_seen` và SQ03/boss/reward/choices cũ giữ lịch sử; không suy ra ceremony mới. Khi player acknowledge ending mới, có thể ghi legacy history nếu chưa có. Old saves 0–6 relic vẫn tiếp tục bình thường.
- Close/Cancel không commit; session không serialize. Restart interrupted giữ boss/relic nhưng phải xác nhận lại. Skip chỉ hợp lệ sau explicit confirm và active session, dùng chung commit idempotent với natural finish. Replay chỉ presentation, không reward/commit lại.

## Presentation và cốt truyện runtime

Nghi thức 24 giây hoạt động: Hội An Đánh Thức → Huế Xác Thực → Đà Lạt Định Hướng → Jeonju Lưu Giữ → Gyeongju Đọc Ký Ức → Seoul Kết Nối, mỗi nhịp 3 giây và 6 giây hội tụ quanh Lumi. Transform/fade/beam UI nhẹ, sprite relic và đúng Lumi dialogue portrait đang có. Clock theo frame unscaled, cap delta 0.1 giây, bỏ qua app suspended: pause/hitch không tự Skip. Decorative elements không nhận raycast. Không thay world reactions được duyệt.

Sau commit là sáu trang hồi tưởng và một trang credits, có Next/Khép lời kết/Tiếp tục khám phá. S21 khi thiếu relic nói rõ số restored và cho mở Collection. Collection hiển thị restored count, Seoul an toàn/chưa yên, trạng thái nghi thức và card đang tìm/chờ ghép/chờ phục dựng/đã phục dựng; không lộ node HIDDEN. Runtime data nằm trong `FinalConnectionPresentation.json` và riêng event final_festival/SQ03 wording của GameplayContent, không chỉ sửa Markdown.

BGM Korea map tiếp tục qua ceremony/credits bằng SoftAudio hiện có; battle đóng khôi phục nhạc map như cũ. Không đổi âm thanh/MP3/.meta/volume/AudioMixer, không tạo track/SFX mới.

## File production và metadata thay đổi

| File | Thay đổi |
|---|---|
| `Assets/_Game/Scripts/Content/NodeEventManager.cs` | Partial, dispatch S21, bỏ legacy bypass/side narration lỗi thời tại S21 |
| `Assets/_Game/Scripts/Content/NodeEventManager.FinalConnection.cs` | Guard, SQ03, transient ceremony clock, completion/persist, history/replay |
| `Assets/_Game/Scripts/UI/GameplayPanels.cs` | Dispatch UI cuối, Cancel khi Close, thay ending legacy |
| `Assets/_Game/Scripts/UI/GameplayPanels.FinalConnection.cs` | Confirm, sáu nhịp, Skip, lời kết/credits/replay |
| `Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs` | Header/card trạng thái Collection; các method H6 nguyên vẹn |
| `Assets/_Game/Resources/Content/FinalConnectionPresentation.json` | Sáu caption/màu/relic ID và lời hội tụ |
| `Assets/_Game/Resources/Content/GameplayContent.json` | Chỉ wording S21/ending pages và SQ03 offer/reminder/thanks |
| `Docs/Art/NPC_CANONICAL_REGISTRY.md` / `.json` | Đúng bốn binding Phase4: CANONICAL + OWNER_APPROVED_AS_IS_20261018; không đổi hình/GUID/localID |
| `Docs/Art/SIX_RELICS_ART_MANIFEST.md` | Append approval bốn NPC/sáu effects; ba Seoul icons vẫn ART_PENDING |

QA/build tooling: `GameplaySmokeRunner.cs`, partial `GameplaySmokeRunner.Phase5.cs`, Editor-only `Phase5Setup.cs`, `Tools/phase5_validation.ps1`, `phase5_suite.ps1`, `phase5_verify.py`, `phase5_report.py`, checkpoint tool và `.meta` các file Unity mới. [Danh sách chính xác](Phase5-20261018/changed-files.json) / [checkpoint receipt](Phase5-20261018/last-checkpoint.json).

Docs canonical/design/full-story, ending audit, QA/gallery và Master checkpoint đã đồng bộ. Prompt gốc archive tại `Docs/Prompts/LightBetweenCultures_Phase5_20261018.md`.

## QA và bảo toàn

[E01–E16, run logs và giới hạn](../QA/PHASE5_ENDING_E2E.md) · [Ảnh runtime mới](../QA/PHASE5_RUNTIME_REVIEW.html). Hai fresh earned routes PASS; final matrix/natural ceremony/Skip, old claims, process restarts PASS. Windows final build07 Succeeded. 364 bounded checks, 106 file bảo vệ byte exact; CulturalRelics/24 sources/trials exact, toàn bộ graph, 29 quest semantics/reward và 22 puzzles giữ nguyên. H6/postcard, 18 check-in, bản đồ, lighting, actor binding, audio và SaveSystem giữ nguyên. Không chạy toàn bộ terrain QA hoặc Release Readiness.

## Tồn tại và nghiệm thu

- P0/P1 Phase5: không còn lỗi tái hiện trong phạm vi đã test; không khẳng định toàn game không lỗi.
- P2: thumbnail Gyeongju/Seoul có nhiễu màu trong World Map capture; nguyên nhân chưa xác định, không chỉnh art ở task này. Shutdown Unity/D3D12 ComputeBuffer/native diagnostics chưa profiling/điều tra. Terrain/graph overdegree tồn tại theo báo cáo riêng, không sửa.
- P3 / ART_PENDING: ba icon component Seoul chưa đúng nghĩa, không gán artwork sai; không ảnh/variant mới. Các crop sliver cũ giữ nguyên theo phê duyệt.
- Cinematic mới **chờ chủ dự án duyệt thẩm mỹ**. Approval as-is chỉ áp dụng bốn NPC/sáu effects Phase4, không tự mở rộng sang cinematic.
- ANDROID_BUILD_NOT_TESTED; ANDROID_DEVICE_NOT_TESTED. Physical audio audition và device FPS/VRAM NOT_TESTED.

Đã tạo checkpoint tăng dần, không copy Library/Temp/Builds và không thay backup/save lịch sử. **DỪNG sau Phase5, chờ nghiệm thu ending; không Phase6/7 hoặc task khác.**
''')

images = [
('matrix04','P5-S21-three-relics.png','S21 hậu boss, 3/6 restored — fixture guard'),
('matrix04','P5-journal-three-relics.png','Collection 3/6, boss an toàn, còn ba relic chờ phục dựng'),
('matrix04','P5-S21-ready.png','S21 đủ boss + sáu restored'),
('matrix04','P5-confirmation.png','Popup xác nhận; chưa completion'),
*[('matrix04',f'P5-ceremony-{city}.png',f'Nghi thức — {city}') for city in ['HoiAn','Hue','DaLat','Jeonju','Gyeongju','Seoul']],
('matrix04','P5-six-lights-converge.png','Sáu đường sáng hội tụ'),
('matrix04','P5-Lumi-connection.png','Lumi và lời kết nối'),
('matrix04','P5-ending-dialogue.png','Lời hồi tưởng sau commit'),
('matrix04','P5-credits.png','Credits duy nhất sau ceremony'),
('matrix04','P5-post-credits.png','Trở về map sau credits'),
('restart-completed02','P5-restart-completed.png','Process restart — không tự credits'),
('restart-completed02','P5-WorldMap-after-credits.png','World Map thật sau credits — lưu ý thumbnail G/S đang có nhiễu màu'),
('legacy-claim01','P5-H6-post-boss.png','H6 sau boss trên old earned save; không sửa H6'),
('legacy-claim01','P5-legacy-cross-city-fragment.png','Mảnh lời chào J2 sau H6 proof — legacy earned source'),
('e2e-vn01','P5-earned-world-revisit.png','Fresh VN_FIRST: quay lại Hội An C sau ending'),
('e2e-korea01','P5-earned-world-revisit.png','Fresh KOREA_FIRST: quay lại Hội An C sau ending')]
figures=[]
for run, file, label in images:
    p=RUNS/run/file; assert p.is_file(),p
    manifest=(RUNS/run/'capture-manifest.txt').read_text(encoding='utf-8-sig')
    stamp=next(line for line in manifest.splitlines() if f' | {file} | ' in line)
    url=f'../../Validation/Phase5-20261018/{run}/{file}'
    figures.append(f'<figure><a href="{url}"><img loading="lazy" src="{url}" alt="{html.escape(label)}"></a><figcaption><b>{html.escape(label)}</b><br>{html.escape(run)}<br><small>{html.escape(stamp)}</small></figcaption></figure>')
write('Docs/QA/PHASE5_RUNTIME_REVIEW.html','''<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>LBC Phase5 runtime review</title><style>body{margin:24px;background:#10292b;color:#f5e7c5;font:16px/1.5 system-ui}a{color:#95ded3}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(290px,1fr));gap:20px}figure{margin:0;background:#183b3a;padding:14px;border:1px solid #a58444;border-radius:12px}img{width:100%;max-height:760px;object-fit:contain}small{font-size:11px;overflow-wrap:anywhere}h1{font-size:28px}</style><h1>Lễ Kết Nối Sáu Miền Ánh Sáng — Phase 5</h1><p>Ảnh Windows Player thật của task 18–19/10/2026. Click ảnh để xem nguyên bản. Timestamp UTC trong manifest. Visual acceptance chờ chủ dự án; Android chưa test.</p><p>Matrix04 là fixture có khai báo, không phải fresh story. Hai fresh earned routes và legacy claims có nhãn riêng. Chuỗi sáu frame có timestamp cho thấy các nhịp trước hội tụ; không dùng mockup/video dựng sẵn.</p><p><a href="PHASE5_ENDING_E2E.md">QA, failures và giới hạn</a> · <a href="../Gameplay/PHASE5_IMPLEMENTATION_REPORT.md">Implementation</a></p><main>'''+''.join(figures)+'</main></html>')

path=ROOT/'Docs/Gameplay/SEOUL_FINAL_CONNECTION_DESIGN.md'
text=path.read_text(encoding='utf-8-sig').replace('có clock unscaled và scene identity','có clock tích lũy unscaled theo frame hoạt động và scene identity; cap delta 0.1 giây/frame, không tính app suspended hoặc thời gian hitch để tránh implicit Skip')
path.write_text(text,encoding='utf-8')
path=ROOT/'Docs/Story/FULL_STORY_PLAYTEST.md'
text=path.read_text(encoding='utf-8-sig').replace('Trạng thái hiện tại (Phase4','Nền tảng kế thừa (Phase4')
path.write_text(text,encoding='utf-8')
path=ROOT/'Docs/QA/FINAL_ENDING_AND_POST_BOSS_AUDIT.md'
text=path.read_text(encoding='utf-8-sig')
prefix='''> **Phase5 implementation — 19/10/2026:** Legacy S21 bypass đã chặn tại backend/UI. Boss battle/repair + 6 restored + explicit ceremony mới ghi `game.canonical_ending_completed.v1`; SQ03 vẫn độc lập tại S21, old history giữ nguyên. Windows build và targeted E01–E16/hai fresh routes/old-save/restart PASS trong phạm vi [QA mới](PHASE5_ENDING_E2E.md), [ảnh runtime](PHASE5_RUNTIME_REVIEW.html). Android chưa test; cinematic chờ duyệt. Nội dung Phase0 bên dưới là lịch sử được giữ nguyên, không đổi FAIL/NOT_TESTED cũ thành PASS.

'''
if not text.startswith('> **Phase5 implementation'):path.write_text(prefix+text,encoding='utf-8')
path=ROOT/'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md'
text=path.read_text(encoding='utf-8-sig')
prefix='''> **PHASE5 — 19/10/2026: WINDOWS_TARGETED_PASS · VISUAL_ACCEPTANCE_PENDING · STOP.** Một ending: boss battle/repair + sáu restored + ceremony xác nhận/hoàn tất; SQ03 báo tin riêng tại S21 dù thiếu relic. Hai fresh earned routes, backend guards, old-save independent claims, natural24s/Skip/Cancel/pause/restarts PASS. Giữ 133node/155edge/29quest/22puzzle/24fragment/6relic/18check-in, art/lighting/routes/audio/save schema. Bốn NPC và sáu effects Phase4 owner-approved as-is; cinematic mới vẫn chờ duyệt, ba Seoul icons ART_PENDING. [Implementation](PHASE5_IMPLEMENTATION_REPORT.md) · [QA](../QA/PHASE5_ENDING_E2E.md) · [Ảnh mới](../QA/PHASE5_RUNTIME_REVIEW.html) · [Receipt](Phase5-20261018/last-checkpoint.json). CheckpointPhase5Final-20261019 incremental cần CheckpointSixRouteReferenceReview-20261018 (bao gồm chain BGM/SFX/Phase4); không tự chuyển Phase6/7/task khác. Những ghi chú Phase4/ending pending bên dưới là lịch sử, được mục này thay thế trong phạm vi Phase5. ANDROID_BUILD_NOT_TESTED · ANDROID_DEVICE_NOT_TESTED.

'''
if not text.startswith('> **PHASE5 —'):path.write_text(prefix+text,encoding='utf-8')
print('Reports/gallery generated from',len(records),'retained runs;',len(images),'representative original captures. No historical evidence changed.')
