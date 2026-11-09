# PROMPT CHO CODEX – TÍCH HỢP CHƯƠNG TRUYỆN HỘI AN (BỘ BỔ SUNG)

Đây là việc BỔ SUNG sau lượt đang chạy. Hãy kiểm tra phiên bản source hiện tại trước khi chỉnh; không tạo project mới, không thay toàn bộ JSON node/route và không tái sử dụng ảnh concept như sprite hoàn chỉnh.

Tôi đã giải nén thư mục `LBC_HoiAn_StoryExpansion_v1` cạnh `Assets`. Đây là **chương truyện và data đặc tả**, chưa phải runtime đã lập trình. Hãy đọc:
- `Docs/STORY_BIBLE_HOI_AN.md`
- `Data/hoi_an_chapter.json`
- `Data/cross_city_food_hooks.json`
- `Data/items_additions.json`
- `Data/marker_rules.json`
- `Docs/QA_ACCEPTANCE.md`

Giữ nguyên H0–H21 cùng tọa độ runtime/waypoints đã tinh chỉnh; file HoiAn trong bộ gameplay gốc chỉ có design anchors gần đúng. GIỮ các thành phần đã chạy: city map, zoom/pan, combat, puzzle, save/load, NPC, animation Lumi. Gộp vào hệ thống event/data đã có, không viết lại sự kiện từ đầu nếu đã hoạt động.

Mục tiêu:
1. Tạo story layer data-driven với trạng thái NPC, dialogue theo world flags, lựa chọn và sự kiện một lần. Cảnh H0 H2 H3 H4 H13 H5 H7 cần chơi được có đầu cuối. Cô Mai mở đầu, vật phẩm H3, câu đố H4, xưởng H13, đối đầu/hack Bot Mờ-01 H5, thắp trạm H7, mở Huế và Jeonju đồng thời. H19 phải có thể tiếp cận để tìm mã TRƯỚC khi H5 chặn. Đừng tự cấp item khi chỉ chạm node.
2. Chia hành trình thành bốn Key Gate H2 H4 H5 H7; đó là bốn thử thách then chốt, KHÔNG phải bắt đánh bốn boss. Progressive reveal đảm bảo đoạn chưa mở ẩn hoàn toàn, preview mờ chỉ ở vùng tiếp theo; NPC/quái chưa reveal không xuất hiện. Kiểm thử pathfinding không bypass Key Gate.
3. Viết hội thoại nhiều trạng thái: chưa nhận, đã nhận, thiếu đồ, đủ đồ, hoàn thành; có nhân vật, portrait, hộp thoại, sự lựa chọn. Không tái sử dụng panel chữ lớn trống; ảnh nhân vật chuẩn từ bộ LBC.
4. NHIỆM VỤ PHỤ LIÊN THÀNH PHỐ: HQ02 Bà Tư tìm Dây Đồng H12 rồi tặng `hoi_an_fresh_chili` x2 và `hoi_an_purple_shallot` x2. HQ05 mang 1 ớt sang gian J15 Jeonju; HQ06 mang 1 hành sang sạp U1 Huế; khi đủ cả hai quay H10 nhận bưu thiếp HQ07. Đây là mini-story giao lưu ẩm thực, không bắt buộc mở cổng, không cản người chơi phá đảo. NPC cũ tại J15 và U1 vẫn giữ chức năng ban đầu, chỉ thêm option.
5. Bảo vệ chống mất đồ/softlock: hàng dự phòng H10 cho quest còn active mà item đã hết (không farm reward). Item thưởng và giao nhận mỗi loại chỉ một lần theo event ID; giữ save cũ `lantern-lit` như mốc tutorial, KHÔNG coi nó đương nhiên là `hoi_an.lamp_restored` để bỏ qua HQ01.
6. HQ03 Thư hồi âm J21 liên hệ H18 và H9; hợp nhất với JQ04 hiện có, không tạo hai loại thư cùng chức năng. H9 mở khi `hoi_an.secret_lane_found` HOẶC `jeonju.hidden_letter_found`. HQ04 bác lái đò là shortcut, không cho đi bộ xuyên sông.
7. Marker mỗi node đổi theo tiến độ: NPC cần đồ `...` trắng nổi nhẹ, boss H5 `!` đỏ, checkin H6 máy ảnh trắng, item/rương/icon cho đúng thiết kế. Chỉ xuất hiện khi node revealed.
8. Khi hoàn tất một story step, cập nhật nhật ký quest, world flags, inventory, save, node marker. Không auto-trigger mọi node khi pathfinding đi ngang; chỉ trigger encounter bắt buộc và item auto đúng thiết kế.

Cách làm: Đầu tiên lập bảng mapping từ các `trigger`/`action` dạng design trong JSON tới runtime hiện có. Các action trong file chỉ là ngôn ngữ thiết kế, KHÔNG tự nhiên được Unity thực thi. Sau đó implement HQ01 + HQ02 có runtime và smoke tests, dừng checkpoint báo kết quả. Giai đoạn tiếp theo tích hợp HQ03–HQ07 và hook Huế/Jeonju, kiểm thử sau khi city maps đã load thật. Không đánh dấu “Done” cho các thành phố chưa có runtime.

Ảnh kiểm thử bắt buộc: mở đầu H0 → H2 → H3 → H4 → H13 → H5 (hai nhánh combat/hack) → H7; quay H10 nhận/tặng đồ; bảng Inventory hiển thị hình ớt/hành; mở World Map thấy cả hai cổng; tự tay chạy một trường hợp save/restart. Nếu thiếu sprite thật, báo asset cần tạo riêng, không dùng ảnh concept crop bẩn hoặc hình tia sét thay boss.

QUAN TRỌNG: Đọc `hoi_an_chapter.json.puzzles` cho bốn puzzle; không tạo nút “sửa mái chèo” mà thiếu minigame/callback. Quest objectives được đối chiếu theo flags khi load/advance; nếu H5 giải quyết trước H13 thì HQ01 phải tự ghi nhận H5 đã xong, không bắt chơi lại trận. HQ03 vẫn nhận được tại H18 dù HQ05 đang active.
