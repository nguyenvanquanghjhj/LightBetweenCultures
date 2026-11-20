# Safe project cleanup — audit before deletion

Ngày thực hiện: 23/10/2026. Project: `D:\GAMEKKK\LightBetweenCultures`.

## Kiểm kê và phân loại

| Nhóm | Dung lượng trước (xấp xỉ) | Phân loại / quyết định |
|---|---:|---|
| Assets, Packages, ProjectSettings, UserSettings | 232 MiB | Đang dùng; giữ nguyên 900 file và hash, bao gồm GUID/meta |
| Builds | 1,54 GiB | Giữ Windows runtime hiện hành, APK FinalUX, APK Playtest trước đó và ZIP đã gửi bạn bè; không tạo build mới |
| Validation | 397 MiB | Lưu trữ tại chỗ; giữ save, ảnh nghiệm thu, ảnh lỗi, source-before, log và gallery có phụ thuộc |
| Docs | 63 MiB trước audit | Giữ báo cáo lịch sử; thêm mục lục, không di chuyển đường dẫn đã được tool/manifest dùng |
| Tools | 26 MiB | Giữ toàn bộ công cụ; chỉ xóa một file Python bytecode có thể sinh lại |
| Kit gốc, References, ReviewPacks, Submission | Khoảng 166 MiB | Lưu trữ/nguồn tham chiếu; giữ do tool/import/provenance còn có thể cần |
| Logs | 28 MiB | Giữ vì nhiều báo cáo và tool đọc trực tiếp; lợi ích xóa nhỏ hơn rủi ro mất bằng chứng |
| Library | 12,85 GiB | Cache Unity; chỉ xóa năm mục trong allowlist dưới đây, giữ asset import DB, PackageCache, Android data cache, native IL2CPP inputs |
| `.utmp`, `build`, IDE project files | Nhỏ | Giữ; IDE/Gradle vẫn chạy dịch vụ nền, không đủ lợi ích để xóa thêm |
| Backup/checkpoint ngoài project | Chuỗi phục hồi | Đã xác minh 2.915 file thuộc chín restore point; KHÔNG xóa hoặc di chuyển |

## Danh sách dự kiến tự động dọn

Chi tiết từng file/size/hash nằm trong `planned-deleted-files.json`; allowlist và điều kiện trong `delete-plan.json`.

| Đường dẫn tương đối | Byte | Lý do |
|---|---:|---|
| Library/Bee/Android/Prj/IL2CPP/Gradle/unityLibrary/build | 1.684.310.392 | Intermediate/output Gradle sinh lại được |
| Library/Bee/Android/Prj/IL2CPP/Gradle/launcher/build | 1.321.374.528 | Intermediate APK/lint/dex; APK bàn giao vẫn giữ trong Builds |
| Library/Bee/artifacts/objcopy_5lmi | 646.568.272 | Bản sao symbol/stripped output; native đầu vào còn nguyên |
| Library/PlayerDataCache/Win642 | 566.653.286 | Cache đóng gói dữ liệu Windows; bản Player hoàn chỉnh vẫn giữ |
| Tools/__pycache__ | 6.309 | Bytecode Python; script gốc giữ nguyên |

Tổng dự kiến: **4.218.912.787 byte (~3,93 GiB), 1.627 file**.

**Chi phí tái tạo:** lần build Windows tiếp theo có thể phải đóng gói lại dữ liệu; lần build Android tiếp theo phải tạo lại output Gradle và bản sao symbol. Không đoán số phút, không xóa toàn Library hoặc cache import art. Không build lại chỉ để tái tạo những cache vừa dọn.

## Phục hồi và ràng buộc an toàn

- `recovery.json` ghi chuỗi source độc lập đã xác minh từ `LBC_SourceVerified-20261020` đến `CheckpointFinalUX-20261023`. Source runtime hiện tại trùng với chuỗi này.
- Cache xóa được tái tạo từ source, package và input native còn giữ. Không sao chép lại 3,93 GiB rác thành một backup mới.
- `README.before.md` giữ nguyên README cũ trước cập nhật tài liệu; không có chỉnh sửa gameplay nào cần rollback.
- Xác minh không có Unity/Player/Bee hoặc Unity-Gradle build đang chạy ngay trước khi xóa. Không kill dịch vụ Java/IDE không liên quan.
- Chỉ dùng đường dẫn tuyệt đối đã resolve nằm dưới project và khớp allowlist. Không theo junction/symlink, không mở rộng wildcard.
- Không đụng save, keystore, signing, Assets, `.meta`, scene, prefab, Resources, source, route hoặc checkpoint.
- Không chạy lại script cleanup lịch sử. Giữ nguyên báo cáo cũ, kể cả bằng chứng thất bại.

Kết quả sau dọn/compile/smoke test được bổ sung trong `RESULT.md` và `result.json`.
