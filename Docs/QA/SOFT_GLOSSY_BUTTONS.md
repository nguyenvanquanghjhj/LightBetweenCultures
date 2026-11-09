# Button mềm, tròn và glossy — 20/10/2026

Đã áp dụng style chung trong runtime, không sửa callback, điều kiện bấm, quest, reward, save, route hay artwork canonical.

## Visual đã đổi

- Bo góc theo 43% cạnh ngắn, tối đa 25 đơn vị Canvas; viền vàng ấm, dày khoảng 2,7 đơn vị.
- Mặt teal/kem giữ bảng màu ấm; gradient tạo lòng nút, glaze nhẹ phía trên, glint mảnh ở mép trên và ba lớp shadow mềm phía dưới.
- Pressed: mặt nút co còn 96% và lún 2,6 đơn vị; Reduced còn 98,5% / 1,2 đơn vị; Off không tween vị trí/kích thước. Vùng raycast và layout gốc giữ nguyên.
- Disabled giảm sắc màu, vẫn giữ chữ rõ; không làm toàn bộ nút mờ trong suốt. Icon và chữ dùng asset/font hiện có.
- Style dùng chung tới menu chính, hội thoại/xác nhận/X, trở lại, cách chơi, hành động minigame, inventory, map toolbar và City Map. Tranh thẻ World Map vẫn dùng sprite cũ; node/actor/marker không bị thay thành nút gel.

UI của project dựng bằng code. Vì vậy sửa lớp presentation chung thay vì thay PNG/prefab. Không tạo ảnh AI, texture, material hoặc shader mới.

## File thực sự sửa

| File dưới `Assets/_Game/Scripts/` | Nội dung |
|---|---|
| `UI/SoftButtonGraphic.cs` + `.meta` — mới | Mesh UI tròn, gradient, viền, sheen, shadow; vẫn là Image để giữ binding sprite/thẻ có sẵn. |
| `UI/MobileUI.cs` | Style dùng chung; palette teal/kem/đỏ nút đóng; giữ thẻ dùng artwork riêng. Không thêm/thay listener. |
| `UI/UIController.cs` | Các nút dựng theo cách cũ ở menu cũng nhận style chung; loại trừ node. Callback cũ giữ nguyên. |
| `UI/UIButtonFeedback.cs` | Nhấn lún/co mặt nút, màu pressed/disabled, giữ vùng chạm cố định và các tùy chọn chuyển động. |
| `Validation/GameplaySmokeRunner.cs`, `.GelButtons.cs` + meta mới | Chế độ QA opt-in trong runner hiện có, save riêng, chụp các mốc visual và kiểm tra callback. |

Đối chiếu **859 input file** theo checkpoint trước: ngoài các file UI/QA trên không có thay đổi hoặc mất file. PNG/audio/content/Packages/ProjectSettings, source NPC approach và dữ liệu gameplay giữ nguyên. [Hash/diff](../../Validation/GelButtons-20261020/source-diff.json).

## Bằng chứng và QA

- **Windows build PASS**: `Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe`, dùng lại thư mục hiện có. Log: `Logs/gel-buttons-after01.log`.
- **Runtime visual/input PASS**, 540×960: menu, guide, pressed, disabled, map toolbar, minigame H13; hủy press không gọi Next; bật lại Next hoạt động một lần; vùng chạm không bị co/dời; đóng activity không grant thưởng, trả lại input. [Kết quả](../../Validation/GelButtons-20261020/after/restart-results.txt).
- **Callback regression PASS**, cửa sổ thực tế 720×1080: nhận nhiệm vụ, receipt, X, giao trực tiếp HA_FOOD_PREP, xác nhận/hủy HQ06, chống thưởng lặp và reload. Dùng lại bài test đã có, không chạy campaign. [Kết quả](../../Validation/GelButtons-20261020/callbacks/smoke-results.txt).
- Ảnh so sánh menu/guide/pressed/disabled/map có cùng kích thước và góc nhìn. Cặp hội thoại H3 dùng ảnh checkpoint trước và bài callback mới với cùng fixture/camera 720×1080. Ảnh minigame H13 là after, không gán một ảnh before giả.
- Collector before dừng ở H4 vì test nhầm H4 là minigame panel; H4 thực tế là short story resolution. Các frame before đã ghi trước đó hợp lệ; không gọi cả lượt before là PASS. Collector after đi đúng H4 → H13 và đã PASS.
- Hai frame tên `Gel-Dialogue` trong collector được chụp khi Lumi còn tiếp cận, **không dùng làm bằng chứng hội thoại**. Gallery dùng frame hội thoại thật từ bài callback hiện có.
- **Android/device/performance profiling: NOT_TESTED**. Không build APK trong task này. Không kết luận tăng FPS. Mesh nút dùng texture trắng chung, không thêm bitmap vào VRAM; chưa đo chi phí GPU trên điện thoại.
- Toàn bộ before/after là frame Windows Player thật. Không coi ảnh đẹp đồng nghĩa gameplay mới hay blind human playtest PASS.

[Mở gallery before/after](../../Validation/GelButtons-20261020/gallery.html).

Checkpoint tăng dần: `D:\GAMEKKK\Backups\CheckpointGelButtons-20261020`, cần checkpoint `CheckpointNpcApproachButtons-20261020` và base chain của nó. Không chứa build, Library hoặc PNG QA. Dừng sau task visual này.
