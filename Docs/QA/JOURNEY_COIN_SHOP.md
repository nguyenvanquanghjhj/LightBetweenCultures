# Cửa hàng Hành Trình — 21/10/2026

Trạng thái: **WINDOWS_FOCUSED_PASS**, 142 assertions qua runtime/input mô phỏng. Android device còn BLOCKED vì ADB không nhận điện thoại; không nhận device PASS.

Mở từ nút **Cửa hàng** ở dãy điều khiển City Map. Một cửa hàng chung, luôn hiện đủ danh mục ở cả sáu thành phố. Bộ lọc chỉ sắp xếp danh sách; không khóa hàng theo tiến độ. Dùng nút glossy hiện có, Canvas/safe area hiện có, không tạo node/NPC hoặc tiền tệ mới.

## Danh mục và giá cố định

| Mặt hàng | Xu / phần | Công dụng |
|---|---:|---|
| Ớt tươi Hội An | 1 | Chuẩn bị đêm hội, giao lưu |
| Hành tím Hội An | 1 | Chuẩn bị đêm hội, giao lưu Huế |
| Phần thức ăn | 1 | Mồi cua/động vật; ba phần cho chó Đà Lạt |
| Trà hồi phục | 2 | Vật phẩm hồi 14 HP hiện có |
| Thảo dược bờ bắc | 2 | Tự giao cho bà Nguyệt, UQ02 |
| Hoa đêm | 2 | Tự giao cho người làm vườn, DQ04 |
| Kính của Lan | 3 | Tự trả Lan; không giải trụ sương thay người chơi |
| Dâu Đà Lạt | 2 | Tự thực hiện yêu cầu giao lưu DQ05 |
| Quyền trả phí đò Hội An | 5, một lần | Hoàn tất HQ04 và `hoi_an.ferry_available` ngay, kể cả mua từ thành phố khác |

Vật phẩm không tự nhận/trả quest. Quyền HQ04 là ngoại lệ được xác nhận rõ trước khi mua: đây là phí đò tùy chọn, cùng giá với cách trả trực tiếp tại H17. Người đã có quyền theo cách cũ không bị thu thêm xu. Không mở World Map/thành phố hoặc thêm tuyến.

Vật phẩm thông thường mua lại được theo stack limit canonical; vật phẩm độc nhất đã có/đã dùng để hoàn thành yêu cầu không được mua thừa. Mua vật phẩm chỉ có phản hồi trong shop, không chuỗi popup. Hết tiền vẫn thấy hàng và số xu còn thiếu.

## Cơ sở giá và ngoại lệ

- Nguồn runtime: 18 check-in trả 2–3 xu; 12 enemy definitions trả 8 xu; 7 rương/bí mật trả 5–12 xu; 4 quest có thưởng xu (8, 8, 10, 150). Không cộng các nhóm thành ngân sách chắc chắn vì encounter/event có thể mô tả cùng phần thưởng.
- Rương H11 đầu chương trả 6 xu, đủ mua nhiều nguyên liệu giá 1–3. Sau HQ01 còn nguồn thưởng canonical lớn hơn. Không tăng thưởng hoặc thêm chỗ cày xu.
- Ba ớt + hai hành, gồm phần phải giữ lại, tốn 5 xu; HA_FOOD_PREP thưởng 8 xu **một lần**. Không mở lại quest để farm. Dâu phục vụ DQ05 cũng chỉ nhận thưởng một lần.
- Không bịa số phút tìm đồ/chơi thử. Chưa có phép đo thời gian người mới độc lập cho từng món; giá dựa trên lượng xu thực và số lượng quest cần.
- Dây Đồng cần cả proof xử lý cua, nên bán riêng dây sẽ gây hiểu nhầm. Shop bán mồi 1 xu để dùng nhánh dụ cua hiện có, nhận dây và proof đúng nguồn.
- Không bán đồ cốt truyện bắt buộc như Mắt Đèn/Lõi Giấy/Chìa Kết Nối, bộ hoa văn/sao/relay, thư có danh tính người gửi, kỷ vật và dấu văn hóa. Không bán sáu relic/24 fragment, boss result, ritual/ending flags, puzzle proofs hoặc quyền bỏ một chương bắt buộc.
- [Kiểm kê đủ 38 item](../../Validation/JourneyShop-20261021/item-classification.csv), [nguồn xu và hash dữ liệu](../../Validation/JourneyShop-20261021/economy-audit.json).

## Giao dịch và tương thích

- Dùng `SaveData.coins`, `items`, `rewardGrantIds` và quest states hiện có; không thêm field/schema.
- Mỗi giao dịch có token lưu vào ledger; token lặp bị từ chối kể cả sau reload. UI debounce 0,35 giây; không thêm giới hạn mua hàng ngày/restock.
- Kiểm tra số xu, stack limit và trạng thái đã có ngay tại backend. Trừ xu/cấp đồ/ghi ledger trong một lần persist; lỗi lưu hoàn nguyên cả state và payload tạm để recovery không tự thu tiền giao dịch thất bại.
- HQ04 dùng chung hàm commit quest canonical. Chỉ quyền phí đò này được phép hoàn tất từ xa; các guard nhận/giao quest khác giữ nguyên.
- Không thay nội dung gameplay JSON, graph/waypoint, art, audio, NPC approach, leisure, relic hoặc ending.

## Source thay đổi

- `Core/JourneyShop.cs`: danh mục, giá và giao dịch.
- `Core/GameBootstrap.cs`: khởi tạo shop cùng các service hiện có.
- `Core/QuestManager.cs`: tách phần commit dùng chung, thêm đúng quyền trả HQ04.
- `Maps/CityMap.cs`, `Maps/CityMap.JourneyShop.cs`: nút mở shop, khóa input map khi mở panel.
- `UI/GameplayPanels.JourneyShop.cs`: danh sách cuộn, lọc, số xu, xác nhận phí đò, thông báo mua.
- `Validation/GameplaySmokeRunner.cs`, `Validation/GameplaySmokeRunner.JourneyShop.cs`: focused Windows runtime tests; save riêng, không đụng save điện thoại.
- `Tools/journey_shop_audit.py`: kiểm kê read-only và xuất bảng phân loại/nguồn xu.

## Kiểm chứng

| Kiểm tra | Kết quả |
|---|---|
| Shop hiện đủ hàng từ save mới, không đủ xu | PASS: UI và backend từ chối, không đổi state |
| Kiếm 6 xu thật ở H11 rồi mua bằng touch | PASS: hai nguyên liệu và thảo dược, trừ đúng 1/1/2 xu |
| Rapid touch và transaction token lặp sau reload | PASS: không trừ/cấp trùng |
| Vật phẩm độc nhất đã có | PASS: nút khóa, không mua thừa |
| Nguyên liệu mua dùng trả HA_FOOD_PREP | PASS: giữ đúng phần dự trữ, thưởng cũ một lần |
| HQ04 mua từ Seoul; xem/cancel/xác nhận | PASS: chỉ xác nhận mới trừ 5 xu, đủ quest/flag ngay |
| HQ04 làm thủ công tại H17 rồi vào shop | PASS: không phải mua lại |
| Shop cả sáu thành phố | PASS: mở/lọc/cuộn danh sách/đóng, map input và save không đổi |
| ID bị cấm/ending/di vật | PASS: backend whitelist từ chối; mua HQ04 không đổi các state này |
| Persist lỗi giữa giao dịch | PASS trong fixture riêng: rollback cả memory và payload .tmp |
| Save/reload và v1 cũ | PASS trong fixture riêng, không thêm field/schema |
| Process Windows mới đọc save Shop | PASS: coins/items/quest/relic/history giữ nguyên, không còn modal/input lock |
| Android realme C71 | BLOCKED: ADB chưa thấy thiết bị; không cài/xóa save |
| Touch drag cuộn trên thiết bị thật, notch/nav bar | NOT_TESTED; Windows đã kiểm tra ScrollRect, clipping và chọn nút bằng touch mô phỏng |

[Ảnh runtime 540×960](../../Validation/JourneyShop-20261021/gallery.html) · [Log 142 assertions](../../Validation/JourneyShop-20261021/runtime01/smoke-results.txt).

Không test lại toàn campaign/12 leisure. Thời gian 5/15/30 phút và cảm nhận người mới không được suy ra từ suite này. Kiểm kê nguồn xu không thay thế human playtest cân bằng dài hạn.

APK `LightBetweenCultures-ComprehensiveQA-20261021.apk` là build QA **trước** shop. APK shop và manifest/hash sẽ được ghi ở `Validation/JourneyShop-20261021/qa-summary.json`; không lấy compile/build làm Android gameplay PASS.

APK Shop: **[LightBetweenCultures-JourneyShop-20261021.apk](../../Builds/Android/LightBetweenCultures-JourneyShop-20261021.apk)**, `1.0-shop20261021`/code 3, 289.655.644 bytes. Build Succeeded, 0 errors; cảnh báo build được giữ trong `Logs/journey-shop-android01.log`. Giữ package/signing cũ; bundleVersion/code trong project được phục hồi sau đóng gói. ADB check cuối không thấy thiết bị, chưa install hoặc smoke test realme C71.

Checkpoint: `D:\GAMEKKK\Backups\CheckpointJourneyShop-20261021`, incremental cần base chain, không phải full project độc lập. Hash và danh sách thay đổi tại `Validation/JourneyShop-20261021/checkpoint.json`.
