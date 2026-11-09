# Touch, khoảng cách NPC và lựa chọn hội thoại — 20/10/2026

> **Lịch sử, đã được thay thế cho phần NPC approach/hội thoại:** cách đặt lại chân NPC dưới đây không đáp ứng yêu cầu giữ vị trí canonical. Xem [kết quả sửa hành vi hiện tại](NPC_APPROACH_AND_BUTTON_FUNCTIONS.md). Không dùng các PASS cũ để kết luận lỗi owner vừa phản ánh đã hết.

Source hiện hành được giữ; không reset/revert. Không đổi PNG, audio, node, arrival, graph, quest graph hoặc save schema. File/hashes: [source-diff.json](AndroidTouch-20261020/source-diff.json).

## Vì sao lần sửa cũ chưa đủ

| Lỗi | Sửa cũ và phần bỏ sót | Sửa hiện tại |
|---|---|---|
| Lumi xa NPC | `CityMap.Start()` đưa marker tới `CityRouteGeometry.Arrival()`, nhưng `PositionGroundedActor()` vẫn dùng `mapPosition` riêng. Test cũ kiểm marker/anchor được cấu hình, không kiểm khoảng cách tới NPC thật. | Giữ nguyên điểm dừng Lumi. Với NPC có anchor quá xa, đặt chân trên polyline đường bộ hiện có gần arrival; bóng, vùng bấm và marker theo cùng chân. Không sửa geometry hoặc vị trí enemy. Cache đường tiếp cận, không tìm đường mới mỗi frame. |
| Nút lặp nội dung | `PlayerCopy.ActionLabel()` và `GameplayPanels.DrawDialogue()` đã sửa nhãn/Next/thứ tự; `BuildOptions()` vẫn tạo `talk`/`progress` trùng văn bản đang đọc. | `NodeEventManager.PruneRepeatedOptions()` đối chiếu nội dung callback và effect thật trước khi render. Giữ talk có proof/grant; ẩn rest khi HP/năng lượng đã đầy. `TalkMessage()` dùng phản hồi sau hoàn thành, tránh phát lại lời cũ. Sửa `result.changed |= Grant(...)` để không mất kết quả cấp flag. |

## Kiểm chứng PC

- Focused Windows Player cuối: **381 assertions PASS**, không chạy campaign. [Kết quả](../../Validation/AndroidTouch-20261020/owner-priority-final/smoke-results.txt). Lượt đầu mất 22 giây; bản cuối chạy lại đúng dependency sau sửa case nhiệm vụ phụ.
- **26 NPC được gán**, 78 phép đo ở zoom 1/2/4, dùng transform runtime. Ở zoom 2: H10 khoảng cách 60,11 → 33,53; G14 114,19 → 42,38 đơn vị UI. [Toàn bộ NPC](../../Validation/AndroidTouch-20261020/owner-priority-final/npc-distance-all.csv).
- **19 callback** của lựa chọn bị ẩn được thực thi trong save fixture riêng và xác nhận không đổi state. **6 callback proof** vẫn được giữ và cấp flag thật. Đây là callback fixture, không phải một lượt người chơi đi tới mọi node. [Bảng callback](../../Validation/AndroidTouch-20261020/owner-priority-final/dialogue-callbacks.csv).
- Đi thật tới H10/G14, mở và đóng hội thoại, input map phục hồi; có bốn ảnh checkpoint runtime.
- Android candidate đầu vẫn lộ case Bà Tư: nhiệm vụ chính đã xong nhưng nhiệm vụ phụ đang làm. So sánh văn bản đơn thuần còn giữ nút hỏi nhiệm vụ cũ. Đã bỏ nút quest-progress không có effect vì nội dung tiến độ đã được gộp trong body; giữ talk cấp proof. `PlayerCopy.Unavailable()` giờ xử lý cả `complete:...` và reserveItems, nêu số lượng thiếu thật thay vì lời nhắc chung chung.
- Case nhiệm vụ phụ: **14 assertions PASS**, nút cũ biến mất, nguyên liệu thiếu được hiển thị và bấm nút khóa không tiêu đồ. [Kết quả](../../Validation/AndroidTouch-20261020/owner-partial-final/smoke-results.txt). Fixture sao từ save điện thoại, bớt một hành tím **chỉ trong bản QA** để kiểm đồng thời hai nguyên liệu thiếu; [provenance](../../Validation/AndroidTouch-20261020/owner-partial-final/fixture-provenance.json). Không sửa save điện thoại. Đây là phần thiếu của test trước dùng save đã hoàn thành toàn bộ.
- Đối chiếu các node có `npcId`: D18 là Lumi nghỉ ngơi; S14 là bảng thông tin bến phà. Registry đã xác định không cần nhân vật ở hai nơi này. Không mượn mặt NPC khác. NPC có sprite gắn tại puzzle/encounter cũng nằm trong 26 actor được kiểm.

## Touch đã thực hiện trước nhóm sửa NPC

- Chặn pinch xuyên overlay; giữ ngón còn lại bị tiêu thụ sau pinch; threshold theo DPI/kích thước màn hình; pointer ownership/cancellation cho nút, slider, kéo mảnh và leisure.
- Camera không giành quyền lúc người chơi đang pan. Tap node có phản hồi sớm. Bảo toàn thời gian dư qua junction, cache chiều dài polyline khi di chuyển; không đổi đường/speed/boarding.
- Tùy chọn 30/60 FPS trong settings, lưu PlayerPrefs riêng, không vào save gameplay.
- Windows touch mô phỏng: **PASS** ở 540×960 và 432×960; 24 node, 12 leisure, H3/puzzle/combat và restart. Thuyền hai chiều/junction: PASS dependency trực tiếp. Đây không phải full QA hay blind playtest.
- Android thật: realme C71/RMX5303, Android 16, 720×1604, DPI 320. APK touch đã xác minh pinch xuyên settings được chặn, 30 FPS lưu qua restart, update giữ tiến độ. ADB gửi sự kiện qua Android InputManager; không phải thao tác tay của người chơi.
- Mẫu SurfaceFlinger ngắn, map đứng yên: bản cũ 28,35 FPS (median 33,34 ms / p95 66,44 ms); bản touch 60,00 FPS (16,67 / 16,89 ms); chế độ 30 đạt 30,00 FPS (33,33 / 33,56 ms). Khác biệt có cả thay đổi frame cap, **không chứng minh CPU/GPU tối ưu tương ứng**. Không kết luận 60 FPS toàn game. [Dữ liệu](../../Validation/AndroidTouch-20261020/device-frame-summary.json).

## Tốc độ QA và giới hạn

- Build Android touch đầu: 492,44 giây. Đây là đóng gói game, không phải ADB chờ mỗi nút.
- Batch đọc RAM/CPU/log: 0,294 / 0,093 / 0,139 giây. Các lần screencap thường khoảng 1–2 giây theo thời gian call; chưa tách đủ thời gian truyền ảnh/đọc ảnh để đưa số chính xác.
- Vòng lặp bấm → chụp → đọc từng ảnh và khoảng suy luận giữa lệnh gây chậm. Đã dừng cách này theo yêu cầu chủ dự án. Không có build/QA treo khi kiểm tra lúc 15:09:54; chỉ dịch vụ ADB nền còn chạy.
- Nhóm NPC/hội thoại chỉ build Android một lần sau PC PASS, smoke đại diện theo batch, tối đa hai ảnh checkpoint. Kết quả nằm trong `AndroidTouch-20261020/priority-device-results.json` khi hoàn tất; không dùng kết quả APK touch cũ để nhận PASS cho source NPC mới.
- Chưa đo input latency bằng camera tốc độ cao; chưa có GPU/GC profiling trên Android hoặc test nhiệt/chơi dài. Các màn combat/minigame/leisure đủ loại mới được kiểm trên Windows; không nhận Android toàn game PASS. Một số nút ở portrait hẹp vẫn dưới 44 px theo bảng audit; chưa nhận toàn bộ touch targets đạt chuẩn.
- Hai lượt Windows trung gian thất bại do adapter QA gọi nhầm `Journal`/thiếu `RelicExit`; đã sửa adapter, không coi hai lượt đó là PASS.

Không triển khai thêm nội dung hoặc Phase mới. Checkpoint incremental dựa trên `D:\GAMEKKK\Backups\LBC_SourceVerified-20261020`, không sao chép Library/build hay cây ảnh QA.

## Kết quả chốt

- Windows source cuối: 381 + 14 assertions PASS. APK cuối build/install PASS.
- **Android hội thoại bản cuối: BLOCKED / chưa nghiệm thu**. Batch cuối 37,59 giây gồm install 21,74 giây, chờ cold start 8 giây, ba tap và một screencap 1,96 giây. Frame chốt tối, chưa tới hội thoại. Process còn sống; chưa xác định nguyên nhân và không kết luận crash. Đã dừng, không lặp chờ/chụp hoặc build thêm.
- Log process có SELinux denied `{ link }` với `hoi-an-v1.json`. Chưa xác định File.Replace có fallback thành công hay không; **save persistence của APK cuối chưa được xác minh**. Không sửa/xóa dữ liệu để né lỗi.
- Lượt smoke ứng viên trước chạy batch 14,20 giây, xác nhận NPC gần hơn và phát hiện nút hỏi nhiệm vụ cũ. Kết quả này không thay nghiệm thu APK cuối.
- APK: `Builds/Android/LightBetweenCultures-OwnerPriority-20261020.apk`. SHA256: `2f973feaf03c0ee04f6fed0251ea077c7da91ccd39208eb00b50e45fd8efd3f3`.
- [Kết quả máy đọc](AndroidTouch-20261020/priority-device-results.json) · [Gallery ảnh gốc](../../Validation/AndroidTouch-20261020/gallery.html).
