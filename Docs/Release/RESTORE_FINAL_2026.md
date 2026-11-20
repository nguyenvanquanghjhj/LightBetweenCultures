# Khôi phục Light Between Cultures — 2026

1. Cài Unity Hub và Unity 6000.6.5f1 (3ff58d469c8a); Windows Build Support, Android Build Support/SDK/NDK/OpenJDK nếu cần. Lần đầu giữ nguyên phiên bản Editor/package.
2. Clone repository chính, checkout `lbc-final-archive-2026-r1`. Bắt buộc giữ `Assets` và `.meta`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings`. Nếu bản sau dùng LFS, chạy `git lfs pull` và `git lfs fsck`; lần lưu trữ này không cần LFS khi mọi file source dưới 100 MiB.
3. Lịch sử/build nằm tại <https://github.com/nguyenvanquanghjhj/LightBetweenCultures-archive/releases>, cần tài khoản có quyền. Tải đủ ZIP và manifest. ZIP là archive độc lập theo nhóm, không nối byte. Giải nén tất cả vào một workspace trống, giữ prefix `_LBC_ARCHIVE`, `_LBC_DELIVERABLES`, `_LBC_WORK`; source giải nén vào project riêng.
4. Kiểm tra ZIP bằng `Get-FileHash -Algorithm SHA256 -LiteralPath '<archive.zip>'` với `SHA256SUMS.txt`/manifest JSON. Kiểm tra file giải nén với manifest nội bộ. Không chạy tự động `RESTORE.bat` hoặc script mutation lịch sử.
5. Git history bundle: `git bundle verify LightBetweenCultures_GitHistory_2026.bundle`, rồi `git clone <file.bundle> RestoredHistory`. Bundle giữ refs khi đóng gói, không force push lên repository khác.
6. Mở project qua Hub, chờ registry/import/C# compile. Manifest/lock xác định dependency nhưng không bảo đảm dịch vụ registry/AI bên ngoài còn hoạt động sau nhiều năm. Ghi rõ lỗi license/registry nếu có; không tự nâng phiên bản để che lỗi khôi phục.
7. Mở `Assets/_Game/Scenes/Prototype.unity`, Play với save QA riêng. Save thật: `Application.persistentDataPath/hoi-an-v1.json`; giữ cả `.bak`/`.tmp` trước thử migration. Archive không tự thu thập save cá nhân hoặc credential ngoài workspace.
8. Build bằng Editor hoặc `python Tools/Build/build.py windows --run-id Restore-UniqueID`; Android tương tự. Chọn output mới, không chạy Phase generator hoặc ghi đè bản nộp cũ.
9. Windows cần toàn bộ folder cạnh EXE, gồm `_Data`, `UnityPlayer.dll`, plugin. Android kiểm tra signature/SDK/alignment rồi cài trên máy QA. Debug-signed APK không bảo đảm cập nhật APK ký khóa khác; không ghi đè save điện thoại thật khi thử.

## Chuỗi checkpoint và khoảng trống lịch sử

Base `LBC_SourceVerified-20261020/SOURCE_MANIFEST.json` cùng incremental chain được giữ nguyên. Dùng relocation map ở `Docs/Development/Storage/WorkspaceRestructure-20261113` để tra đường dẫn cũ; không sửa manifest lịch sử để che lỗi. `CheckpointBeforeFTUE-20261109-152433` thiếu manifest từ trước, chỉ được coi là snapshot file. Restore point độc lập trước Full Story nếu không còn trên đĩa phải ghi là thiếu, không tạo manifest giả hoặc đổi tên snapshot mới thành bản cũ.

Source, build và checkpoint có thể khác phiên bản. Xem release notes/báo cáo cuối cho commit/hash/kết quả chạy. Không chuyển PASS cũ thành nghiệm thu hiện hành. Chỉ dọn bản cục bộ sau khi tải lại, kiểm chứng và chủ dự án phê duyệt riêng danh sách đường dẫn.
