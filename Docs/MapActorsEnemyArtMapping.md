# Enemy art mapping — Stage 1, 16/10/2026

Catalog thực tế: `AdventureUIArt.asset`. Roster nguồn: `LBC_AnhMoi_BanGiaoCodex_20261015/Characters/Enemies_Roster_Concept.png`.
Ba cutout Hội An là bản dẫn xuất bằng `image_gen`, đã kiểm tra alpha/viền và runtime.
`portrait` của ba enemy hiện trỏ tới cùng hình toàn thân tĩnh để giai đoạn sau sử dụng;
màn encounter/combat chưa đổi sang art này trong Giai đoạn 1. Không có animation sheet mới.

| enemyId | Sprite | Portrait | Nguồn / thiết kế | Trạng thái |
|---|---|---|---|---|
| `enemy_crab` | `Enemies_HoiAn_TransparentAtlas.png#enemy_crab` | Cùng cutout toàn thân; chưa dùng trong combat | Cua trộm — hàng trên, cột 2 | Đã dùng trên City Map Hội An; runtime PASS |
| `enemy_ghost` | `Enemies_HoiAn_TransparentAtlas.png#enemy_ghost` | Cùng cutout toàn thân; chưa dùng trong combat | Đèn lồng ma — hàng trên, cột 1 | Đã dùng trên City Map Hội An; runtime PASS |
| `enemy_mo01` | `Enemies_HoiAn_TransparentAtlas.png#enemy_mo01` | Cùng cutout toàn thân; chưa dùng trong combat | Bot gác cầu — hàng trên, cột 4 | Đã dùng trên City Map Hội An; runtime PASS |
| `enemy_river_wisp` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_courier` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_archive_guard` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_gate_guard` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_mist_wolf` | Chưa gán | Chưa gán | Sói sương — hàng trên, cột 3 | Concept có; chưa có cutout runtime |
| `enemy_glass_thief` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_fog_boss` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_paper_crane` | Chưa gán | Chưa gán | Chim giấy ma thuật — hàng dưới, cột 5 | Concept có; chưa có cutout runtime |
| `enemy_parcel_bot` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_pattern_bot` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_crane_boss` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_fox` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_stone_guard` | Chưa gán | Chưa gán | Tượng đá thức tỉnh — hàng dưới, cột 3 | Concept có; chưa có cutout runtime |
| `enemy_night_guard` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_star_core` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_drone` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_signal_bot` | Chưa gán | Chưa gán | Chưa đối chiếu được thiết kế riêng đủ chắc chắn | Cần xác nhận/chuẩn bị asset phù hợp |
| `enemy_bridge` | Chưa gán | Chưa gán | Bot gác cầu — hàng trên, cột 4 (chưa gán biến thể Seoul) | Concept có; chưa có cutout runtime |
| `enemy_shadow_courier` | Chưa gán | Chưa gán | Bot đưa tin bóng tối — hàng trên, cột 5 | Concept có; chưa có cutout runtime |
| `enemy_final_core` | Chưa gán | Chưa gán | Linh cầu nhiễu sáng — hàng dưới, cột 1 | Concept có; chưa có cutout runtime |

## Nhân vật đứng trên map

| Node | Nhân vật | Asset | Xuất hiện / kết thúc |
|---|---|---|---|
| H2 | Cô Mai | Mai_DialoguePortrait.png — tái sử dụng toàn thân tĩnh | Khu đầu; ở lại sau quest |
| H4 | Thương nhân chợ | Market_Merchant.png — cutout riêng từ thương nhân rong | Sau Gate 1; ở lại khi puzzle đã giải |
| H5 | Bot Mờ-01 | enemy_mo01 | Sau Gate 2; ẩn khi thắng hoặc dùng mã |
| H12 | Cua trộm | enemy_crab | Sau Gate 1; ẩn khi giải quyết bằng mồi/chiến đấu |
| H15 | Đèn lồng ma | enemy_ghost | Sau Gate 3; ẩn khi giải quyết encounter |
| H17 | Bác lái đò | Boatman_Portrait.png — tái sử dụng | Sau Gate 3; ở lại sau mở chuyến đò |
| H18 | Khách Hàn Quốc | NPC_TransparentAtlas.png#npc_guest — tái sử dụng | Sau Gate 2 |

H4 vẫn là người bán hàng không đặt tên của puzzle hiện có; không dùng thương nhân này để giả làm Bà Tư.
Bà Tư H10 và ông Phúc H14 còn thiếu cutout đúng nhân vật; giữ tương tác/icon hiện có, không tráo portrait.
H13 là xưởng giải đố, H7 là trạm ánh sáng, không tự thêm quái để biến mọi gate thành trận đánh.

Nguồn gốc và prompt chính xác: `MapActorsStage1Prompts.json`. Ảnh nguồn, atlas NPC cũ và ba vai trò Lumi giữ nguyên.
