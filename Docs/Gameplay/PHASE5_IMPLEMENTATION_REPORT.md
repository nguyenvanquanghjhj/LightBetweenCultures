# Phase 5 — One canonical ending

Chốt 19/10/2026 · **WINDOWS_TARGETED_PASS · VISUAL_ACCEPTANCE_PENDING · STOP**.

Đã triển khai ending thật: **boss resolved + sáu relic restored + nghi thức được xác nhận và hoàn tất hợp lệ**. Thiếu relic vẫn được hoàn thành boss/SQ03, đi World Map và quay lại thu mảnh; không credits sớm, không bắt đánh boss lại.

## Baseline và lỗ hổng đã sửa

Kế thừa source mới nhất sau Phase4, State B20, NPC map markers, Soft SFX, Four-track BGM và checkpoint route-reference review chỉ có docs. Pre-edit snapshot nhỏ: `D:\GAMEKKK\Backups\BeforePhase5-20261018`. Base cuối là `CheckpointSixRouteReferenceReview-20261018`, không checkout source Phase4 cũ.

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
