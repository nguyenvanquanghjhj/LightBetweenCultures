# Deep storage cleanup · 20/10/2026

| Ổ | Trống trước (GiB) | Trống sau (GiB) | Chênh lệch đo trực tiếp (GiB) |
|---|---:|---:|---:|
| C: | 67.659 | 67.802 | +0.144 |
| D: | 192.019 | 243.206 | +51.187 |

Đây là số free-space đọc trực tiếp từ ổ đĩa, không phải tổng dung lượng ước tính. Số C: có thể biến động do ứng dụng khác; file Unity tạm/dump được xóa khoảng 44,9 MB. Bản backup source mới đã được tính vào chênh lệch ròng trên D:.

- Đã xóa: 100 checkpoint/backup cũ theo xác nhận riêng của chủ dự án (19,886 GiB logical), build Windows/APK cũ, ZIP Windows dư, QA captures/output cũ, log, và 9,555 GiB cache build có thể tái tạo. Bee ban đầu gặp đường dẫn dài 278 ký tự; đã xử lý đúng cache bằng Windows extended-length path.
- Giữ toàn bộ Assets/metadata, Packages, ProjectSettings, source, art gốc, 18 check-in, route editor/dữ liệu thiết kế đường, save, keystore/signing và cấu hình tool đang dùng. Dữ liệu cache import chính/PackageCache còn nguyên. Build sau sẽ dựng lại Bee/PlayerData/Burst; không cần import lại toàn bộ art vì không xóa cả Library.
- Giữ một Windows Player đầy đủ: `D:\GAMEKKK\LightBetweenCultures\Builds\Windows-LeisureVisuals-20261020\LightBetweenCultures.exe`.
- Giữ một APK thành công gần nhất: `D:\GAMEKKK\LightBetweenCultures\Builds\Android\LightBetweenCultures-GameFeel-20261020.apk`. APK chưa có ba sửa leisure sau đó; không tự build lại trong lượt này.
- Backup source độc lập đã xác minh: `D:\GAMEKKK\Backups\LBC_SourceVerified-20261020`, 472,5 MiB, không cần base checkpoint. Có 35 nội dung source lịch sử độc nhất ở `HistoricalUnique`. Ngoài ra giữ checkpoint 1,47 MiB làm input provenance cho route exporter; không coi chuỗi lịch sử đó còn khôi phục được.
- C: giữ Unity Editor/Android SDK/NDK/JDK khoảng 23,719 GiB, Gradle 6,223 GiB (daemon đang hoạt động), Unity cache dùng chung 0,534 GiB và Unity Hub. Không xóa/uninstall cache dùng chung hay công cụ. Zalo/VS Code/ứng dụng khác, Downloads và dữ liệu cá nhân được giữ nguyên.
- Kiểm tra sau dọn: 2679 file trong backup và 849 file Assets/Packages/ProjectSettings đều đúng hash; hash EXE/game DLL/APK giữ nguyên; Windows đủ 376 file dependency. Không build APK, không chạy full QA/Phase 7, không sửa gameplay.

[QA hiện hành](../../QA/CURRENT_QA_STATUS.md) · [15 mục lớn nhất trước dọn](top-15-before.csv) · [Số liệu sau dọn](result.json) · [Build/hash giữ lại](kept-builds.json)

Hoàn tất và DỪNG.
