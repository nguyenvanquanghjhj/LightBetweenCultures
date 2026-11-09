# Catventure — bàn giao ngày 07/11/2026

**Bản hiện tại: 1.1.1.** Đã bỏ các tên mèo do Codex tự đặt và bỏ vùng bấm mèo phụ đè lên điểm đến trên bản đồ. [Chi tiết bản sửa, ảnh Huế và kiểm tra mới](LABELS_LAYOUT_FIX.md). Các số kiểm tra 633/514 và gallery bên dưới ghi nhận bản 1.1 trước phản hồi, được giữ làm lịch sử.

Đã tích hợp tuyến giải cứu mèo độc lập cho sáu thành phố, 24 thử thách và sáu mèo sưu tập. Có kéo thả, ghép tranh, tìm vật ẩn trong cảnh, mê cung, câu hỏi/tình huống, bát bibimbap tương tác, ghép Hangeul và chọn màu Hanbok. Hội An sáng dần theo nhiệm vụ; từng thành phố có thời tiết. Tiến trình được lưu từng thao tác, phần thưởng có receipt chống lặp; mở thêm tuyến thành phố mà không cấp cờ hoàn thành truyện cũ.

## Chạy game

- **Windows Release hiện tại:** [Catventure.exe](../../Builds/Windows-Catventure-Fix-Release-20261107/Catventure.exe). Chạy ngay trong thư mục này; giữ `Catventure_Data` và các DLL cạnh EXE.
- **Android APK hiện tại:** [Catventure-Fix-20261107.apk](../../Builds/Android/Catventure-Fix-20261107.apk), phiên bản `1.1.1-catventure-20261107`, versionCode 21. Không tự cài đè lên điện thoại trong lượt này.
- [Hướng dẫn chơi](HANDOFF.md) · [Ảnh Player trước/sau và các mini-game](../../Validation/Catventure-20261107/gallery.html) · [Manifest SHA256](../../Validation/Catventure-20261107/summary.json).

## Kiểm chứng

| Lượt Windows Player | Số kiểm tra | Kết quả |
|---|---:|---|
| verified-portrait | 256 | PASS |
| verified-desktop | 256 | PASS |
| verified-portrait / restart | 3 | PASS |
| legacy-regression | 118 | PASS |

Tổng **633 kiểm tra Player PASS** trong các lượt cuối. Hai lượt chính đi theo đường hiện có, dùng chuột/cảm ứng mô phỏng để lấy vật dụng, thực hiện toàn bộ 24 thử thách, cứu đủ sáu mèo và mở bưu thiếp. Có kiểm tra sai/đúng, đóng/mở giữ mảnh, phối màu, reload, giao diện âm thanh, sơ đồ mini-map và ánh đèn Hội An. Lượt regression đi qua World Map thật, kiểm tra save/recovery, giữ Lumi canonical và giao dịch cửa hàng cũ.

**512 + 2 kiểm tra domain PASS**: biên, thứ tự, vị trí, thiếu vật dụng, lưu dở, lỗi ghi file phải rollback, chống thưởng trùng, backup recovery và tiến trình mới. Các bài này biên dịch file `CatRescue.cs` và `SaveSystem.cs` thật trong host .NET có adapter Unity; không được tính là kiểm thử native Unity. Dữ liệu/graph xác nhận mọi mục tiêu mới có đường từ spawn ở stage 0; mọi mê cung có lời giải.

Windows Development, Windows Release và APK Android đều build thành công. APK được kiểm tra riêng bằng công cụ Android trong `android-verification.txt`. **Chưa có điện thoại ADB kết nối**, nên input/FPS/RAM Android và độ hấp dẫn qua người chơi độc lập vẫn **NOT_TESTED**. Không tuyên bố đã chạy lại toàn bộ campaign truyện cũ.

## Bảo toàn và khôi phục

Đã so sánh SHA256 với bản trước nâng cấp: **1840 file gốc giữ nguyên**, không xóa file gốc. Mọi artwork/metadata nhân vật cũ, dữ liệu sáu map, navigation, quest/puzzle và Packages/ProjectSettings giữ nguyên. Các file runtime cũ sửa chỉ gồm bootstrap, save, City Map, World Map; QA runner và tài liệu được bổ sung tương ứng. Sửa thêm race khi chuyển thành phố để bản đồ cũ không cập nhật bằng dữ liệu thành phố mới; dời nút âm thanh để tránh chồng HUD.

- Trước sửa: `D:/GAMEKKK/Backups/CheckpointPreCatventure-20261107`.
- Sau sửa: `D:/GAMEKKK/Backups/CheckpointCatventure-20261107`.
- Cả hai là **incremental, cần toàn bộ base chain trong manifest**, không phải bản sao độc lập. Không sao chép Library/cache/build hoặc xóa bản backup nào.

Các lượt raw trước sửa vẫn giữ: lỗi giấy phép/sandbox ở `pilot`, lỗi chuyển map và kéo chưa hoàn tất ở `final-portrait`; chúng không được cộng vào kết quả cuối. Nghiệm thu ở các thư mục `verified-*` sử dụng build đã sửa. Phiên bản source/ảnh cũ của các lượt trước nằm trong manifest `pre-polish-build.json` và log tương ứng.

[Phương án và file tích hợp](IMPLEMENTATION.md) · [Nguồn văn hóa](CULTURAL_SOURCES.md) · [Ảnh mới và prompt](ART_PROVENANCE.md).
