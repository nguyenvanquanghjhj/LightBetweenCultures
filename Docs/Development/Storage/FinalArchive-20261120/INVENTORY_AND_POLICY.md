# Kiểm kê và chính sách lưu trữ — 20/11/2026

Phạm vi toàn bộ `D:\GAMEKKK`, kể cả file ẩn, Git, backup và các nhóm ngoài project. Danh sách file/kích thước ban đầu: `workspace-files.csv`; tổng hợp: `inventory-summary.json`. Các số là logical file bytes, không phải dung lượng đã giải phóng. Không xóa dữ liệu gốc hoặc gỡ phần mềm trong nhiệm vụ này.

## Phân loại

| Nhóm | Quyết định |
|---|---|
| `Assets`, `.meta`, `Packages`, `ProjectSettings` | Bắt buộc trong GitHub source và ZIP source; không dựa vào Library để khôi phục |
| `Tools`, input authoring hiện hành, `Docs`, `README`, `AGENTS.md` | Giữ trên repository chính; công cụ lịch sử có guard không được chạy lại |
| `_LBC_ARCHIVE` | Giữ original art, preimage, nguồn lịch sử, backup, checkpoint và QA trong Releases private |
| `_LBC_DELIVERABLES` | Giữ đầy đủ Windows/APK, bản nháp cuộc thi, PDF/PPTX, handoff và review pack trong Releases private |
| `_LBC_WORK/QA`, `Logs`, `Debug` có trước nhiệm vụ | Giữ bằng chứng, fixture, save QA, lỗi và build còn có vai trò; chỉ loại cache/intermediate được nêu đích danh trong manifest đóng gói |
| `UserSettings` | Giữ 10 file tùy chọn Editor/layout trong ZIP private; Search.index và DragAndDropCache là cache sinh lại |
| `Library`, `.utmp`, root `build`, root Unity Logs, IDE project sinh tự động, bytecode/obj | Có thể tái tạo. Chưa xóa; import và build lại sẽ tốn thời gian |
| File lịch sử/Hotfix/Phase/Legacy chưa chứng minh thừa | Giữ nguyên. Tên cũ không phải tiêu chí xóa |
| Dữ liệu mới của lần diễn tập | Log/report cần kết luận sẽ đính kèm release; Library và Development Player mới có thể tái tạo, không biến thành bản nộp đã nghiệm thu |

Không tìm thấy video demo trong inventory ban đầu. Bản nộp KGW2026 hiện là bản nháp; không suy diễn đã gửi email/nộp thi. Không gửi email hoặc thông báo tới người khác.

## Git và dữ liệu chuyển chỗ

Mốc trước nhiệm vụ: `9ec07c5e581444d34447598f4c0c6538e40ad9f4`, hai nhánh local cùng mốc, remote chỉ có main. Working tree có 5.316 đường dẫn deleted tính theo HEAD trước stage, gồm file đã di chuyển và generated được bỏ theo dõi. `deleted-reconciliation.json` đối chiếu 5.294 đường dẫn; `deleted-resolution-supplement.json` ghi phần còn lại, gồm khác biệt xuống dòng, source đã sửa, folder metadata cũ và manifest lịch sử. Mọi blob tracked trước đó vẫn được giữ bằng lịch sử Git và history bundle; không reset/clean/rebase/force push.

Sửa `.gitignore` để cache/build/QA output chỉ được ignore tại gốc project; tránh bỏ sót source trong `Tools/Build` và `Scripts/Validation`. Không thay asset, GUID, quest, save schema hay gameplay để hoàn tất đóng gói. README cũ được giữ nguyên ở `README.before.md`.

## Checkpoint và quyền riêng tư

Chuỗi standalone 20/10 → Catventure Labels/Layout 07/11: SHA-256 đạt 3.197 file, xem `checkpoint-chain.json`. Checkpoint BeforeFTUE chỉ còn Assets, thiếu manifest từ trước; bảo tồn file, không tự cấp chứng nhận standalone.

Không tìm thấy điểm độc lập trước Full Story trong workspace hiện tại. Hồ sơ `DeepCleanup-20261020/SUMMARY.md` ghi 100 checkpoint/backup cũ đã được xóa theo xác nhận riêng trước đây, thay bằng standalone 20/10 và `HistoricalUnique`. Nhiệm vụ hiện tại không phục dựng giả hoặc che khoảng trống lịch sử này. Toàn bộ restore point còn hiện hữu được giữ nguyên.

Hai `debug.keystore` chỉ xuất hiện trong QA lịch sử và được giữ trong kho private, không commit vào source công khai. Bản nháp nộp thi và thông tin cục bộ cũng chỉ lên kho private. Không tìm thấy token pattern trong source/nhóm lịch sử đã quét; kiểm tra tự động không chứng minh mọi loại bí mật đều vắng mặt. Không thu thập credential hoặc save cá nhân ngoài workspace.

## Hệ thống và giới hạn kiểm kê

`system-inventory.json` ghi Unity Editor/Android support khoảng 25,47 GB, Unity Hub khoảng 0,624 GB và hai cache Unity khoảng 0,523 GB. Android SDK/NDK/OpenJDK trong Editor được tính trong Editor, không cộng hai lần. Chỉ thấy project hiện hành và snapshot của chính nó trong vùng đã quét; Unity Hub cũng chỉ đăng ký project này. Không chứng nhận các ổ chưa mount/không truy cập được hoặc các vùng loại trừ không có dự án khác.

Bảo vệ `D:\PBL4`, mọi dự án đại học khác, VS Code, Visual Studio, .NET, Java/SDK dùng chung, Git, Gradle dùng chung và dữ liệu cá nhân. Các junction tương thích Windows bị từ chối truy cập không phải bằng chứng cần xóa. Lỗi đường dẫn dài của một SDK jar khi đo Editor chỉ giới hạn phép đo dung lượng; không liên quan đến source game.

## Điều kiện dọn máy

Chỉ đề xuất đường dẫn cụ thể sau khi archive được upload, tải lại, đối chiếu SHA-256 và giải nén kiểm tra. ZIP đang nằm trong `_LBC_WORK` chưa phải bằng chứng remote an toàn. Người dùng đã chọn GitHub là nơi remote duy nhất; kho private và source ZIP là bản archive ngoài working tree theo lựa chọn đó, cùng chịu rủi ro quyền truy cập tài khoản/nhà cung cấp.

Checkpoint/backup bắt buộc cần phê duyệt riêng sau khi có bản remote đã kiểm chứng. Không phê duyệt thì giữ nguyên toàn bộ bản cục bộ. Báo cáo sau cùng và danh sách đường dẫn sẽ được đính kèm Releases; không có script tự động xóa/uninstall trong bộ công cụ lưu trữ.
