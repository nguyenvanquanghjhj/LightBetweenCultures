# Bổ sung kiểm soát art — 16/10/2026

Kiểm tra mới nhất: project runtime khớp `CheckpointCrossCityFood-20261016`, không có
Unity/player đang chạy. Không bắt đầu chương mới hoặc làm lại Giai đoạn A/B.

## Thay đổi

- Thêm [quy trình mở rộng năm thành phố](CITY_EXPANSION_ART_CHECKLIST.md): kiểm tra
  nguồn/binding/canonical, địa hình riêng, framework chung và nghiệm thu runtime từng phần.
- Bổ sung tùy chọn `--city-preflight` vào `Tools/art_asset_catalog.py` hiện có,
  không tạo hệ thống runtime. Chỉ ghi [snapshot kiểm tra](city-art-preflight.json).
- Cập nhật [MISSING_ASSETS](MISSING_ASSETS.md) theo checkpoint mới: phân biệt sprite
  tạm, thiếu art final và vai trò chưa đăng ký. Không còn liệt kê bot Hội An hoặc
  nền thành phố đã có là asset chưa tồn tại.
- Thêm liên kết ở ART_ASSET_CATALOG và generator; đánh dấu DESIGN_KIT_INTEGRATION_VI
  là báo cáo lịch sử để tránh áp lại giới hạn đã lỗi thời.

File thêm/sửa: `CITY_EXPANSION_ART_CHECKLIST.md`, `city-art-preflight.json`,
`CITY_ART_CONTROL_REPORT.md`, `MISSING_ASSETS.md`, `ART_ASSET_CATALOG.md`,
`DESIGN_KIT_INTEGRATION_VI.md` trong Docs/Art; `Tools/art_asset_catalog.py`.

## Kiểm tra lượt này

| Kiểm tra | Kết quả |
|---|---|
| Overview A so SHA-256 với nguồn và node ID/thứ tự với sáu JSON canonical | PASS — 6 nền nguyên bytes, 1448×1086; giữ 133 node |
| Import đủ độ phân giải cho các map đang chơi | PASS — Hội An/Huế/Jeonju; 3 map preview chưa bật được báo pending |
| GUID/native sprite ID, actor reference, ba vai trò Lumi và hai hình tạm khác binding | PASS |
| Item binding | PASS — 37 item; icon nhóm/hình tạm được ghi riêng |
| Hash input của snapshot | PASS — 140 đường dẫn ảnh/meta/registry/data/source |
| Python syntax, liên kết tài liệu | PASS; không chạy lại dọn/move art |
| Assets/_Game, ProjectSettings, Packages, Builds so checkpoint | PASS — 776 file nguyên bytes, không thêm/xóa trong các phạm vi này |
| Gameplay/build/runtime screenshot mới | Không chạy lại: không có thay đổi runtime, shader, scene, dữ liệu gameplay hoặc importer |

Kết quả gameplay cũ và ảnh của nó nằm trong [CrossCityFoodReport](../../CrossCityFoodReport.md).
Không dùng chúng để tuyên bố đã kiểm thử một chương mới trong lượt bổ sung này.

## Pending

Đà Lạt/Gyeongju/Seoul còn DefaultTexturePlatform max 1024 cho preview. Trước khi
bật gameplay phải nâng importer trên GUID đang có, kiểm tra player và địa hình riêng.
20 enemy/boss của năm thành phố chưa có sprite đăng ký; đây chưa phải kết luận
art pack thiếu 20 ảnh, cần QC nguồn theo từng đợt mở rộng. Hương/So-yeon vẫn tạm;
Mai/Ji-eun không đổi. Toàn bộ terrain/chương năm thành phố và Android vật lý chưa
được chứng nhận. Chỉ phần mở đầu Huế/Jeonju ở checkpoint trước đã chơi được.

Checkpoint bổ sung: `D:/GAMEKKK/Backups/CheckpointCityArtControl-20261016` chỉ lưu
7 file Docs/Tools thay đổi, kèm manifest hash; dùng cùng checkpoint đầy đủ
`CheckpointCrossCityFood-20261016`, không thay thế hoặc ghi đè checkpoint đó.
