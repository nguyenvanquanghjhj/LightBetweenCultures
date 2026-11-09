# LIGHT BETWEEN CULTURES — BỘ ẢNH MỚI BỔ SUNG (15/10/2026)

**Đây là gói BỔ SUNG cho `LightBetweenCultures_DesignKit_v1` mà bạn đã có; không thay thế, không ghi đè folder cũ hay Unity project.**

## Hướng dẫn dùng
1. Giải nén ZIP vào **thư mục gốc project Unity**, ngang hàng `Assets/` và thư mục design kit cũ: `LightBetweenCultures/LBC_AnhMoi_BanGiaoCodex_20261015/`.
2. Nhờ Codex đọc `ASSET_MANIFEST.csv`, `GAME_DESIGN_OVERVIEW.md`, `CODEX_IMPLEMENTATION_PROMPT.md` và toàn bộ PNG mới.
3. Các PNG hiện ở **ngoài** `Assets/` để chưa tự động import toàn bộ tham khảo, tránh phình project. Codex phải chủ động **COPY** ảnh được chọn vào `Assets/_Game/Art/` khi được giao tích hợp.
4. BẢN ĐỒ OVERVIEW: có thể làm nền 2D cho City Map theo node, đặt đường nối / marker qua dữ liệu riêng. Cần hiệu chỉnh độ sáng, tỷ lệ, vị trí chạm. Bản đồ đẹp chỉ là art, KHÔNG suy ra sẵn graph chính xác từ pixel.
5. SCENE KHÁM PHÁ TỰ DO: **không dùng một ảnh nền phẳng duy nhất làm level hoàn chỉnh**. Phải có walkable regions, colliders, các đồ vật tương tác tách riêng; cần sprites/prefabs sạch có alpha hoặc phải tạo lại dựa concept. Không cắt tùy tiện các đối tượng chồng lấp rồi gọi là spritesheet production-ready.
6. BẢNG nhân vật/NPC/quái/vật phẩm: là **concept sheet** gồm chữ, nền, bóng, nhiều hình ghép chung; chưa phải file sprite animation. Có thể tách thủ công / thử cho portrait hoặc icon nếu sạch, phải kiểm tra viền + alpha trước. Nếu thiếu phải liệt kê yêu cầu tạo PNG riêng từng asset.
7. Canonical hero đang ở Design Kit cũ `References/Lumi_CANONICAL_user_image.png`. Không tự thay đổi nhân vật gốc nếu người dùng chưa duyệt Lumi phiên bản mới. Tấm `Lumi_Actions_Concept.png` là phương án style để đối chiếu.
8. Nhân vật City Map chỉ cần **một hướng nhìn nhất quán** theo góc máy; không bắt dựng bốn hướng. Idle + 2 frame bước đi + animation nhỏ đủ cho MVP.
9. Giữ ảnh mẫu gốc không bị chỉnh sửa; mọi crop/sprite dẫn xuất lưu file riêng.
10. Cho City Map zoom/pan thật trong Unity: chuột cuộn + pinch (nếu build Android), zoom giới hạn theo thiết kế, marker/đường đi đi theo map, camera clamp, UI giữ nguyên kích thước. Không phải bắt vẽ lại ảnh zoom.
