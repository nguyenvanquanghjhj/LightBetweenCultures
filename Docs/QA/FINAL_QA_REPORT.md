# Báo cáo QA cuối

**Dự án:** LightBetweenCultures  
**Ngày:** 2026-11-10  
**Unity:** 6000.6.5f1

## Trạng thái

| Hạng mục | Trạng thái | Kết quả |
| --- | --- | --- |
| Marker check-in Hội An | PASS (kiểm tra tĩnh + compile) | H2, H3, H4, H6, H7, H17 chỉ hiện camera khi có thể ghi nhận; cờ ghi nhận đã lưu chuyển marker thành ✓. H4/H7 không còn dựa riêng vào event resolved. |
| H18 | PASS (kiểm tra tĩnh + compile) | HQ03 hoàn tất hiện marker ✓. H18 không có check-in, cờ check-in hay phần thưởng giả. Không dùng dấu `!`. |
| Camera các thành phố | PASS (kiểm tra dữ liệu) | 16 khai báo camera đều khớp check-in thật. D0/D14 Đà Lạt không có rule camera riêng nhưng vẫn là check-in thật và được resolver xử lý theo trạng thái `CanCheckIn`. Không có camera khai báo nào không khớp check-in; có 18 check-in tổng cộng. |
| Mê cung cứu mèo | IMPLEMENTED; runtime chưa xác nhận | Di chuyển chỉ cập nhật ô trước/ô hiện tại và trạng thái nút hoàn tất. Nút ô không còn ColorTint/press-scale feedback. Kiểm tra dữ liệu xác nhận 4 mê cung 5×5 đều có đường đi. Smoke test mới kiểm tra identity và vị trí của 25 ô sau từng bước/reset nhưng chưa chạy được. |
| Đồ thị đường đi | PASS (kiểm tra dữ liệu) | 442 cạnh trên 6 thành phố có đầu/cuối trỏ tới node hợp lệ; mọi điểm polyline nằm trong tọa độ chuẩn 0–1. Đường đi chưa được traverse trực tiếp trong Player lần này. |
| C# runtime và Editor | PASS | Biên dịch bằng .NET SDK 8.0.318 kèm Unity: 0 lỗi. Runtime có 184 cảnh báo; Editor có 251 cảnh báo, chủ yếu cảnh báo API Unity obsolete và trường deserialize không gán trực tiếp. |
| JSON nội dung | PASS | Cả 9 tệp JSON trong `Assets/_Game/Resources/Content` parse thành công. `git diff --check` không phát hiện lỗi khoảng trắng. |
| Unity Windows EXE | BLOCKED | Unity Editor thoát mã 198 vì chưa có license/entitlement hợp lệ. Không tạo EXE. |
| Android APK | BLOCKED | Chưa chạy build vì Unity Editor chưa có license hợp lệ. |
| Unity EditMode/PlayMode | NOT RUN | Không tìm thấy thư mục hoặc assembly test trong `Assets`; Unity Test Runner cũng cần license. |
| Runtime UI / toàn bộ tuyến / Android device | NOT RUN | Cần Player build. `adb devices` không khởi tạo được Android user config trong môi trường hiện tại nên chưa thể xác nhận thiết bị. |

## Thay đổi liên quan

- Resolver Hội An ưu tiên cờ check-in đã lưu để hiện dấu ✓; event hoàn tất tự nó không giả lập check-in. H18 dùng marker `Completed`, được vẽ bằng dấu ✓.
- Bàn mê cung giữ nguyên các nút và tọa độ qua từng bước/reset; Lumi và trạng thái đường đi được cập nhật tại chỗ.
- Thêm smoke assertions cho H2/H3/H4/H6/H7/H17, trường hợp H18 không có check-in, và identity/vị trí của bàn mê cung trong cả bốn thành phố.
- Bộ chuối trên bot Seoul được giữ nguyên theo xác nhận của người dùng.
- APK builder nhận đường dẫn đầu ra QA riêng để tránh ghi đè artifact cũ.

## Bằng chứng và giới hạn

- Unity CLI chưa đăng nhập (`loggedIn=false`); `license status` báo không liên lạc được licensing client. Lần thử Windows Development build ngày 2026-11-10 kết thúc mã 198 với `No valid Unity Editor license found`. File provenance nằm trong `Builds/FinalQA-20261110/Windows-QA/`; không có EXE ở thư mục đó.
- Compile độc lập kiểm tra mã C# nhưng không thay thế Unity import/build hoặc kiểm thử UI trong Player. Các smoke run cho check-in, bốn mê cung, bot/portrait, tuyến thành phố và shop/battle vẫn cần chạy sau khi Unity Hub kích hoạt license.
- Có runtime smoke lịch sử ngày 2026-11-09 ghi nhận bão Seoul hoạt động khi vào thành phố (`Validation/SlideWeather-20261109/weather-seoul-v2/smoke-results.txt`). Đây không phải kiểm tra trên build mới của báo cáo này.
- Dữ liệu hiện còn `routesReviewed=false` tại Huế, Đà Lạt, Jeonju, Gyeongju và Seoul. Chưa đánh dấu các tuyến này đã được duyệt vì runtime traversal chưa chạy.
- Không mở hoặc sửa save thật. Save QA có sẵn chỉ được đọc; thay đổi trong lần này dùng các đường dẫn QA riêng. Không reset Git hoặc xóa backup.

## Artifact QA

- Compile runtime: `Validation/FinalQA-20261110/dotnet-compile.log`
- Compile Editor: `Validation/FinalQA-20261110/dotnet-editor-compile.log`
- Unity build log: `Validation/FinalQA-20261110/windows-development-build.log`
- Kết quả đầy đủ vẫn pending Unity Hub license và các smoke run trong Player.
