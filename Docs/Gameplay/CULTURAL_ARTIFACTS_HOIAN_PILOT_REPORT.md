# Hội An relic pilot — nghiệm thu Phase 1–3 · 18/10/2026

**FUNCTIONAL WINDOWS PASS / VISUAL ACCEPTANCE PENDING — STOP trước Phase4.**

[Review ảnh runtime mới](RelicPilot-20261018/RuntimeReview.html) · [QA counts/evidence](RelicPilot-20261018/qa-results.json) · [Chi tiết kiến trúc/save](CULTURAL_ARTIFACTS_INTEGRATION_REPORT.md) · [Danh sách file](RelicPilot-20261018/changed-files.json).

## Đã chơi thực tế

| Nguồn / hoạt động | Tương tác và proof | Kết quả |
|---|---|---|
| H0 · Khung Tre Bến Sông | Source mới tại anchor intro: kéo vào hình mờ + xoay đúng; không free khi đến node | PASS actual mouse, wrong drop/angle rejected, grant1 lần |
| H4 · Tấm Giấy Hoa Cổ | Sau puzzle_market thật; thao tác đối chiếu giấy riêng | PASS, không replay reward; flag-only legacy fixture PASS |
| H10 · Móc Đồng Mái Hiên | Side interaction mới: kéo/căn móc, xoay hướng treo; HQ01 proof | PASS touch, HQ02/Bà Tư reward không đổi |
| H2 · Dấu Phản Chiếu Mắt Đèn | Chỉ sau **completed mai-lantern**; pickup-only bị từ chối | PASS; không tạo Mắt Đèn inventory thứ hai |
| Assembly |4 mảnh vào4 slots riêng qua pointer drag | PASS; empty slots/confirm không đủ, assembled persist |
| Dẫn Ánh Đèn |3 gương thật đổi đường tia,2 obstacle cells, hit test tới đích, hint/retry/exit | PASS; sai hướng không restored; đúng trước H7 vẫn bị chặn; đúng sau H7 mới restored |
| H7 / World Map | Main quest cũ,3 item/progress,150 xu, Huế/Jeonju flags cũ | PASS; chưa trial vẫn mở Jeonju. Không thêm relic gate vào map/boss |
| Full city / Lumi / Cô Mai | Derive restored,10 visible landmark halos/lanterns, fade, existing Lumi transform reaction, Mai thêm phản hồi sau restored | PASS visible layer/transition; không raycast, re-entry không celebration lặp; visual taste cần người dùng duyệt |
| H2 check-in | Sau tutorial trước H7; quest/talk trước camera; checkinId/ledger giữ | PASS |
| H6 photo/postcard | Kéo đèn + phản chiếu vào2 dấu; album từ journal, source Map A crop đúng aspect | PASS; view/confirm trống không proof; metadata before/after, reward cũ1 lần |

Không claim node chỉ vì preview/click hoặc class tồn tại. Source/trial dùng real InputSystem mouse và touchscreen events trong Windows Player, không setter làm giả mảnh. H0/H10/H6 là interaction mới được duyệt, không giả thành event cũ đã có.

## Lượt QA đã chạy

| Run / binary | Assertions | Kết quả / bằng chứng |
|---|---:|---|
| Fresh04 / build04 |140 | [ALL CHECKS PASSED](../../Validation/RelicPilot-20261018/Fresh04/smoke-results.txt): H0→Mai→H3 physical pickup→4 sources→assembly trước H7 blocked→H13/H19/H5/H7 cũ→beam→full light→H6→journal |
| Restart01 / build04 |10 | [ALL CHECKS PASSED](../../Validation/RelicPilot-20261018/Restart01/restart-results.txt): process mới từ earned Fresh04 save;4 ownership/restored/photo giữ, landscape/re-entry, không coins/items/ledger lặp |
| Retro01 / build04 |59 | [ALL CHECKS PASSED](../../Validation/RelicPilot-20261018/Retro01/smoke-results.txt): copy earned FullStory Fresh04 đã29 quest/credits/H6 mark; source gesture recovery thật; không puzzle replay;29 completed/items/coins giữ, legacy ending không auto-restored |
| NPC01 / build03 |57 | [ALL CHECKS PASSED](../../Validation/RelicPilot-20261018/NPC01/smoke-results.txt):4 portrait đúng role/speaker/native refs/framing; không approval ngoại hình. Binding/DrawDialogue unchanged ở final04 |

Hai compatibility fixtures trong Retro01 chỉ tạm bỏ solved-list entry hoặc duplicated quest flag nhưng giữ proof đã earned, restore lại ngay trước gesture; ghi rõ là fixture, không giả gameplay route. Other runtime journey dùng tiến độ chơi thật. Save cũ gốc `Validation/FullStory-20261017/Fresh04/full-story-final.json` chỉ đọc/copy, không sửa.

Windows final build: `Logs/relic-pilot-build04.log` → Build Success/Succeeded. Không rebuild cả game vô cớ: build01/02 cho integration/runner dispatch,03 cho presentation,04 sửa lỗi vẽ đã nhìn thấy và compatibility proof. CS0618/UAC0009 baseline warnings còn, không error CS mới. Import01 bị sandbox licensing IPC; import02 ngoài sandbox thành công.

Giữ failures để trace: Fresh01 FAIL do dispatch QA đặt nhầm helper H3 (đã sửa); Fresh02 FAIL do sandbox PlayerPrefs QA key, không bug game; Fresh03 logic PASS nhưng khi xem ảnh thấy beam nằm dưới RoundedBackground, đã sửa trong04 và có assert+ảnh xác nhận. Không dùng các lượt đó thay proof final04. Không chạy lại266 assertions WR V2 hoặc FullStory133 node.

## Ảnh nghiệm thu mới

Before/after cùng **540×960, zoom1.25, focus(.48,.55), Lumi H2**, H7 completed cả hai. Before4 mảnh+assembled nhưng trial/restored false; after trial/restored true. Không chỉnh PNG screenshot.

| State | Ảnh runtime |
|---|---|
| H7 done, chưa full relic light | [before](../../Validation/RelicPilot-20261018/Fresh04/Relic-before-full-city-H7-done.png) |
| Sau nghi thức, full-city layer | [after cùng view](../../Validation/RelicPilot-20261018/Fresh04/Relic-after-full-city-same-view.png) |
| Beam thật qua3 gương | [Dẫn Ánh Đèn](../../Validation/RelicPilot-20261018/Fresh04/Relic-beam-valid-restoration.png) |
|4 slots bằng drag | [assembly](../../Validation/RelicPilot-20261018/Fresh04/Relic-four-pieces-assembly.png) |
| H6 alignment | [postcard](../../Validation/RelicPilot-20261018/Fresh04/Relic-H6-postcard-aligned.png) |
| Sổ6 relics, chỉ Hội An restored | [collection](../../Validation/RelicPilot-20261018/Fresh04/Relic-collection-six-icons.png) |
| Actual restart landscape | [restart](../../Validation/RelicPilot-20261018/Restart01/Relic-restart-landscape.png) |
|4 NPC mới | [report cùng4 ảnh](../Art/NPC_FOUR_NEW_ART_INTEGRATION_20261017.md) |

Đã mở xem beam/map/collection và cả4 portrait; beam vàng hiện đúng, ảnh giữ tỷ lệ, portrait không trắng/crop tay/chân. Map canonical vốn đã có nhiều đèn sáng trong tranh: thay đổi hiện tại là **overlay ấm tại landmark đã mở**, không phải bản đồ day/night mới. Độ mạnh glow/pacing/style chưa được người dùng VISUAL_APPROVED; không dùng logic PASS để tự khen đồ họa đẹp.

## Chưa đạt / chưa kiểm / art cần thêm

- **ART_PENDING:**3 icon Seoul đúng canonical `fragment_seoul_heritage_data`, `fragment_seoul_jiwon_sync`, `fragment_seoul_stellar_ring`; không lấy3 tín hiệu sai thay thế. Full prism + frame kỹ thuật có thật.
-4 NPC chỉ ART_IMPORTED_PENDING_APPROVAL. Curator–Inscription mặt/tóc gần nhau; Engineer thiếu kính, Curator trẻ hơn brief, Keeper già hơn brief. Không tự đổi mặt, chốt canonical hoặc duyệt12 NPC khác.
-17 component ngoài Hội An mới native-sliced/bound, chưa test riêng trong trial. Vài nguồn glow/alpha sát mép giữ nguyên; người dùng cần duyệt crop/style.
- **NEEDS_AUDIO:** chưa có SFX nhặt mảnh/thắp đèn/hoàn tất phù hợp được xác minh trong Assets/_Game; không tải audio/giấy phép lạ. Chỉ transform/overlay nhẹ, không mới walk-cycle/biểu cảm vẽ lại.
- **NOT_TESTED:** Android build/device/FPS/VRAM/thermal, touch thiết bị thật, minigame landscape đầy đủ, disk-full/crash injection, legacy v1 process riêng. Fresh v2 restart + earned old v2 recovery có runtime proof; SaveSystem v1/v2 code giữ nguyên.
- Thời lượng minigame với người chơi mới chưa đo, không ép timer30s. Miniature roof/beam board là UI hình học chức năng, còn có thể polish art sau nghiệm thu.
- H6 đã lưu photo proof; **Jeonju Greeting Stamp claim/trial vẫn Phase4 pending**, không mở cả năm relics trong pilot.
- **NOT_STARTED:** Phase4 năm city, Phase5 ending mới, Phase6 mở rộng polish, Phase7 Release Readiness. Legacy S21 credits chưa bị đổi theo yêu cầu không triển khai ending lượt này.

## Điểm dừng

Checkpoint: `D:/GAMEKKK/Backups/CheckpointRelicHoiAnPilot-20261018`, incremental **requires CheckpointWorldReactionsV2-20261017 và base chain**; không Library/Temp/build/full-history copy. Giữ BeforeRelicPilot-20261018 để đối chiếu. Các snapshot gốc/save/backup lịch sử không bị xóa hoặc sửa.

**Dừng để người dùng nghiệm thu Hội An, mức glow/mini-game/postcard và4 portrait. Không tự sang Phase4.**
