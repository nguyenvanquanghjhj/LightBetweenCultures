# Huế — preview art cục bộ, 21/10/2026

- Unity đang dùng `Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png`, 1448×1086, GUID `481d167093be364419d9ebb11b8e4e27`, qua `AdventureUIArt.asset` / city `hue`.
- Quyết định mới nhất của chủ dự án: Vincom dưới trái đã duyệt mẫu; bên phải dùng nền sân/vườn của crop đã chọn, bỏ Duyệt Thị Đường và không đưa tháp cũ trở lại.
- `Hue-before.png` giữ nguyên ảnh canonical. `Hue-after-preview.png` là bản ghép để duyệt, chưa dùng trong game.
- Art cục bộ xử lý bằng built-in image_gen. Mẫu Vincom đã duyệt được thu nhỏ/ghép trong mask; phần trên chỗ tháp cũ khôi phục bằng cây lân cận. Bên phải dùng crop đã xóa nhà hát. Không dùng toàn bộ mảng nền AI thay map.
- Tool `Tools/hue_two_buildings_preview.ps1` ghép bằng mask cố định; ngoài union hai mask giữ đúng pixel gốc. `pixel-validation.json`: 20.549 pixel thay đổi, 0 pixel thay đổi ngoài mask. Hai mask và ảnh diff được giữ để kiểm tra, không phải asset runtime.
- `A-Vincom-preview.png` và `B-cleared-preview.png` là cận cảnh phóng 3× nearest-neighbor phục vụ xem ranh giới, không tuyên bố tăng độ phân giải thật.
- Chưa chỉnh `Assets`, sprite binding/import, scene, node, arrival, route, quest, NPC, marker, audio hoặc save. Chưa kiểm thử zoom/pan/occlusion trong Windows vì chưa đưa bản ghép vào runtime. Không build APK.
- Các draft có nhà hát hoặc góc nghiêng là lịch sử đã bỏ; không được import. Lượt sửa trục mái đã bị hủy theo chỉ đạo chủ dự án.
- Chờ duyệt bản ghép trước khi tích hợp Unity. Mẫu Vincom được duyệt không đồng nghĩa mọi mép ghép hoặc tỷ lệ trên toàn map đã được nghiệm thu.
