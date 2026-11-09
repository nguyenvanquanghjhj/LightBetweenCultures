# Phụ lục kiểm soát chất lượng — đối chiếu 4 nhiệm vụ

Đã đọc nguyên văn đủ bốn yêu cầu trong Docs/Requests/20261017 và toàn bộ Docs/Story/FULL_STORY_PLAYTEST.md. Đây là bổ sung cho các milestone đã có, không phải triển khai lại bốn nhiệm vụ. Tiếp tục từ **CheckpointRenderQuality-20261017**.

## Trạng thái mới nhất và xung đột tài liệu

| Mục phụ lục | Đối chiếu hiện tại / cách xử lý |
|---|---|
| Tên tài liệu cốt truyện | Hiện chỉ có `Docs/Story/FULL_STORY_PLAYTEST.md`. Tên `LBC_FullStory_Playtest_6Cities_133Nodes.md` không còn tồn tại. 15 nguồn canonical/story expansion và story spec khớp SHA với snapshot nguồn Full Story. Không tạo bản nội dung thứ hai hoặc đổi truyện |
| 133 node / 29 quest | Giữ đúng ID và thứ tự của 133 node so JSON canonical. Giữ HQ01–HQ07, HA_FOOD_PREP, UQ01–04, DQ01–05, JQ01–04, GQ01–04, SQ01–04. Số 28 trong story spec là thống kê cũ; không xóa quest |
| Gyeongju / Seoul DATA_ONLY | Thông tin phụ lục phản ánh bản cũ. Hiện cả sáu city runtimeReady; không còn event `imported_design_not_playable`. **Bằng chứng gameplay là hành trình/lineage trong FullStoryReport, không phải hai trường dữ liệu này**. Theo audit milestone đó: 126 IMPLEMENTED, 7 PARTIAL về presentation/cơ chế chi tiết; không tuyên bố mọi node hoàn thiện mỹ thuật |
| DQ05 | Nguồn nhận thật D4 sau `da_lat.beacon_lit`; giao J15 có xác nhận, cần/tiêu 1 `da_lat_strawberry`, thưởng 10 xu qua ledger cũ. Các run Fresh04/Reverse03 đã có bằng chứng giao thật và luồng xác nhận. Seoul chỉ yêu cầu bốn beacon, không phụ thuộc DQ05. Không đổi logic/reward/save |
| Đường Hội An | Giữ waypoint/arrival anchor đã sửa, 22 node, degree tối đa 3, H22 không có trong dữ liệu phát hành. R1 mặt sàn cầu có mái, R2 bậc đền, R3 mép tán cây **NEEDS_REVIEW**. Pack01 có ba ảnh đánh dấu cũ đúng nguồn, không gọi render/movement khớp là terrain PASS |
| Nhận diện NPC | Phụ lục nói Hương/So-yeon còn tạm, nhưng xác nhận sau đó của người dùng đã chốt hai nhân vật cùng Bà Tư/ông Phúc là CANONICAL. Áp dụng registry hiện tại, giữ mọi binding NPC. Ji-won vẫn cần hình riêng; không dùng So-yeon |
| Ông Phúc | Runtime/story canonical dùng **H14**, không phải H16 như phụ lục ghi. Giữ nguyên ID/vị trí và binding; H16 vẫn là node bến chuyển hướng |
| Bưu thiếp có credit | Phát hiện không chỉ derivative NeedsCleanup: chính JPG gốc có chữ `studio-lawiene` vẫn được hai item tham chiếu. Đã bỏ hai binding runtime đó, dùng sprite `scroll` sạch sẵn có trong atlas LBC làm **NEEDS_ART_TEMPORARY_CATEGORY_ICON**, không gọi là postcard final. Giữ JPG/credit/derivative nguyên vẹn làm reference; không xóa chữ ghi công |

## Những thay đổi thực tế trong lượt này

1. **HoiAnDemoSetup.cs**: gọi Setup khi scene tồn tại sẽ giữ nguyên, kể cả batch mode. Nếu scene thiếu nhưng art H3 nâng cấp còn tồn tại, dừng với lỗi yêu cầu khôi phục scene; không tạo lại primitive. Bỏ nhánh backup/overwrite không còn hợp lệ. Không xóa legacy creation cho trường hợp bootstrap chưa từng có scene/art nâng cấp.
2. **H3ArtSetup.cs**: thêm phép kiểm Editor gọi thật HoiAnDemoSetup.Setup, so byte scene và active scene trước/sau. Không dùng mock và không gọi công cụ đại tu scene lần nữa.
3. **AdventureUIArt.asset**: chỉ đổi sprite/source/status/categoryIcon của `festival_friendship_postcard` và `hoi_an_bridge_postcard`. Hai item dùng chung sprite `scroll` có sẵn: GUID `b0d1662050b200c4e94c139bfde08b33`, localID `-227745245`. Không thêm bitmap/sprite asset hoặc đổi itemId.
4. **AdventureUISetup.cs**: đường import specialty sau này cũng dùng icon sạch cho hai postcard, tránh rebind JPG có credit. Hàm apply hẹp chỉ sửa hai item được dùng cho build này; không chạy lại toàn bộ import specialty hoặc art-library migration.
5. **GameplaySmokeRunner.cs / .RenderQuality.cs / .CrossCity.cs**: nhánh QA opt-in giới hạn hai ảnh, dùng helper cuộn túi đồ đang có và kiểm cả **tên item**, tránh false positive khi nhiều item dùng chung category icon. Không tạo manager, Canvas, EventSystem hay gameplay mới.
6. Đồng bộ đúng hai item trong **Docs/Art/native-art-bindings.json, art-asset-manifest.json/csv, ART_ASSET_CATALOG.md**; JPG chuyển trạng thái reference-only. Không chạy scan/migration toàn thư viện; các record NPC giữ nguyên.
7. **Tools/quality_addendum.py**, bản nguyên văn `Docs/Requests/20261017/quality-control-addendum.txt`, báo cáo/proof hiện tại và bốn review ZIP chọn lọc. Không sửa báo cáo lịch sử để che khác biệt.

Scene, GameplayContent, AdventurePresentation, mọi NPC/enemy/Lumi/marker binding và 30 file Packages/ProjectSettings trong phạm vi bảo vệ giữ nguyên byte. Riêng resource art được so toàn văn sau khi loại đúng hai block postcard: các phần khác không đổi. Không thay đổi topology, quest/battle/puzzle, reward, save schema, art nguồn hay dữ liệu lịch sử.

## Build và kiểm thử mới

- Editor compile/guard **PASS**, gọi thực công cụ legacy và scene/active scene không đổi. Log `Logs/quality-addendum-editor-guard.log`; JSON `editor-guard-result.json`. Nhánh thiếu scene được rà soát source, **không thử bằng cách xóa/di chuyển scene thật**.
- Windows build cuối **PASS**: `Logs/quality-addendum-postcard-build-02.log`; Assembly-CSharp SHA256 trong `verification.json`. Build lại vì đổi binding đóng gói và bổ sung QA chính xác, không phải build lặp khi input không đổi.
- **Postcard02: 18 assertions PASS** từ bản sao nguyên byte save Coverage03 đã kiếm được; lineage lưu cạnh save QA. Mở World Map → Hội An → túi đồ, cuộn/chọn đúng tên Bưu thiếp chợ giao lưu, kiểm icon và số lượng 1; xem popup trình bày rồi đóng; so exact SaveData và reload từ đĩa. Không cấp vật phẩm hoặc cờ hoàn thành giả.
- Ảnh `Postcard-inventory-earned.png` là UI thực trên item đã kiếm được. Ảnh `Postcard-receipt-PRESENTATION-FIXTURE.png` là **fixture trình bày**, gọi view cho một vật phẩm đã có, không execute/grant quest; không dùng nó làm bằng chứng nhận thưởng HQ07 mới.
- Run đầu `Postcard` bị loại khỏi nghiệm thu hình ảnh: click item ngoài viewport chọn nhầm vật phẩm dùng chung icon. Đã xem ảnh, sửa QA bằng helper scroll và assert tên, chạy lại Postcard02; giữ log/save/ảnh run đầu làm chẩn đoán, không cộng kết quả nó vào PASS cuối. Không sửa Inventory gameplay để vượt test.
- Không rerun fresh E2E sáu chương, battle hoặc toàn bộ hàng nghìn assertions vì phần gameplay không đổi. Bằng chứng cũ được kế thừa **đúng milestone và đúng giới hạn**, không gọi là ảnh/test mới của lượt này. Kiểm thử fresh/reverse/relay/credits/sidequest/restart chi tiết trong manifest Full Story được đóng gói.

## Trạng thái nghiệm thu

**PASS mới:** guard H3, hai binding sạch, inventory đúng tên/sprite/quantity, view popup, exact save và reload, build, bảo toàn dữ liệu/identity.

**FAILED cuối:** không có. Run QA đầu không đạt tiêu chí ảnh đúng item, đã chẩn đoán và giữ riêng.

**NOT VERIFIED:** Android vật lý; fresh E2E sáu thành phố chạy lại với binary mới; nhánh guard thiếu scene qua thao tác phá scene thật. Không cần chạy các việc này để chứng minh một thay đổi icon, nhưng không tuyên bố chúng đã PASS mới.

**NEEDS_REVIEW / NEEDS_ART:** R1/R2/R3; 7 node PARTIAL theo FullStoryReport; bảy portrait riêng còn thiếu theo ASSET_STATUS (đặc biệt Ji-won); art enemy tạm và giới hạn nguồn H3; **hai hình postcard final** đang dùng category icon giấy cuộn. Không thay khuôn mặt đã duyệt và không để phần art chưa duyệt chặn gameplay độc lập.

## Review pack và checkpoint

Bốn ZIP trong **ReviewPacks/QualityAddendum-20261017/**, mỗi ZIP theo đúng một yêu cầu. Giải nén rồi mở OPEN_REVIEW.html; manifest liệt kê nguồn, SHA256 và trạng thái. Chỉ pack04 thêm hai ảnh QA postcard mới; các ảnh cũ ghi rõ milestone nguồn. Báo cáo lịch sử có thể nhắc art tạm/đường dẫn đã cũ; bảng đối chiếu này và NPC_CANONICAL_REGISTRY hiện tại có ưu tiên cao hơn các mô tả lịch sử đó.

Không chứa binary, Library, full Validation hoặc backup payload. Bộ Validation/log/save đầy đủ vẫn giữ tại project. Các ZIP chỉ đọc ảnh được chọn trong thư mục run cụ thể; không quét toàn Backups/Validation.

Checkpoint gia tăng **D:/GAMEKKK/Backups/CheckpointQualityAddendum-20261017**, yêu cầu base **CheckpointRenderQuality-20261017** và chuỗi base của nó. Không có checkpoint hoặc save lịch sử nào bị xóa/ghi đè. Đây là checkpoint bổ sung an toàn và bằng chứng, không phải tuyên bố toàn bộ art sáu thành phố final.

**Tiếp tục:** nghiệm thu terrain/portrait/art còn thiếu theo đúng mục đang chờ trong bốn yêu cầu; không làm lại UI/gameplay đã PASS. Ngân hàng 150 câu và Release Readiness Audit là yêu cầu riêng trong Queue, chưa áp dụng trong phụ lục này và không tự đổi thứ tự.
