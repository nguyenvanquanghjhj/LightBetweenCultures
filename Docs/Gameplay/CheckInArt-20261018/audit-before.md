# Check-in art — audit trước triển khai, 18/10/2026

Snapshot trước sửa: `BeforeCheckInArt-20261018`, cần base `CheckpointMapRoute35-20261018`. Source check-in có đủ18 handler/panel/flag/ledger; H6 có gesture2 biểu tượng và stored postcard. Album chung chưa có. Các trạng thái dưới đây là đối chiếu code, không phải18 runtime PASS trước sửa.

18 PNG nguồn 450×722/726/756, RGB, đọc được, không blank/corrupt, filename/size khớp manifest và18 city/node duy nhất. Đã xem contact sheet18 ảnh. Nguồn là crop collage đã upscale/sharpen theo README; giữ LIMITED_SOURCE_RESOLUTION, không tự nâng lên full-resolution. Runtime dùng UV giữ nguyên vùng tranh, bỏ chân chữ; không tạo PNG dẫn xuất hoặc sửa ảnh nguồn.

H6 giữ ảnh căn khớp/StoredPostcard Map A đã PASS. Ảnh mới chỉ candidate pending visual approval. Không đổi hotspot/proof/reward; old completionFlag vẫn đủ mở xem lại từ album, không bịa photo metadata.

| City/node | CheckInId | Trước sửa | Phần cần bổ sung |
|---|---|---|---|
| hoi_an/H2 | hoi_an.h2 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| hoi_an/H3 | hoi_an.h3 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| hoi_an/H4 | hoi_an.h4 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| hoi_an/H6 | hoi_an.h6 | ALREADY_PLAYABLE; H6_PHOTO_ALIGNMENT_AND_STORED_POSTCARD_PRESERVED | Candidate only; giữ UI/ảnh H6; album chung từ flag cũ |
| hoi_an/H7 | hoi_an.h7 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| hoi_an/H17 | hoi_an.h17 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| hue/U4 | hue_u4_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| hue/U6 | hue_u6_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| da_lat/D0 | dalat_checkin_0 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| da_lat/D14 | dalat_checkin_14 | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| jeonju/J8 | jeonju_j8_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| jeonju/J11 | jeonju_j11_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| gyeongju/G4 | gyeongju_g4_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| gyeongju/G14 | gyeongju_g14_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| gyeongju/G17 | gyeongju_g17_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| seoul/S2 | seoul_s2_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| seoul/S10 | seoul_s10_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |
| seoul/S18 | seoul_s18_checkin | ALREADY_PLAYABLE; MAP_A_ROI; HAS_ART_BUT_NOT_BOUND | Bind scenery; xem trước rồi xác nhận; album chung từ flag cũ |

Dependency: GameplayPanels là UI duy nhất cho event/battle/puzzle/relic; mở rộng bằng partial CheckIns. Collection hiện có là entry point qua Nhật ký. SaveSystem v1/v2 đã lưu flag và ledger, không cần schema mới. AdventureUIArt được mở rộng array metadata ảnh; marker Normal mới được camera fallback khi có check-in khả dụng, không thay NPC/boss/quest/relay marker.

NodeEventManager, CheckInDefinition, GameplayContent, AdventurePresentation, RelicCollection, quest/boss/world gates, map/route/collider giữ nguyên. NPC canonical và BGM không sửa. Current process check chỉ thấy current Codex executable + Unity Hub, không Editor/Player task khác đang ghi; import/build của lượt này tuần tự.
