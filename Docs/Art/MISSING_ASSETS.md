# Art còn thiếu / chưa hoàn thiện — cập nhật 17/10/2026

Đối chiếu source hai chương Huế/Jeonju mới nhất, [catalog](ART_ASSET_CATALOG.md) và
[snapshot nguồn/import/binding](city-art-preflight.json). Phân biệt thiếu bản sản xuất,
hình tạm và asset chưa gắn; chưa có binding không có nghĩa art pack không có hình phù hợp.
Trước khi làm từng thành phố phải mở xem roster/source theo
[quy trình kiểm soát art](CITY_EXPANSION_ART_CHECKLIST.md).

Đính chính nhận diện 17/10/2026: [NPC_CANONICAL_REGISTRY](NPC_CANONICAL_REGISTRY.md) là nguồn duyệt danh tính hiện tại. Một danh tính có nhiều biến thể; thay trang phục không tự tạo xung đột. Các mục tiến độ chương bên dưới có phần lưu từ preflight cũ; tiến độ Full Story hiện hành xem [báo cáo Full Story](../../FullStoryReport.md), không suy ra thiếu gameplay từ snapshot art cũ.

## Phần đang chơi được

| Tài nguyên | Trạng thái chính xác / cần bổ sung |
|---|---|
| Chị Hương — npc_huong, U1 | CANONICAL theo xác nhận 17/10: giữ native slice áo xanh/mũ hoa/giỏ hoa. Không còn thiếu xác nhận danh tính; biến thể/animation mới được đánh giá riêng. |
| So-yeon — npc_soyeon, J15 | CANONICAL theo xác nhận 17/10: giữ native slice kính/áo xanh/tablet, khác Hương và Ji-eun. Không dùng mặt này cho Ji-won. |
| Bà Tư — H10; ông Phúc — H14 | CANONICAL theo xác nhận 17/10: native slices/portrait đã chạy và PASS được giữ nguyên. Không còn ghi thiếu portrait canonical mặc định; animation/biến thể mới và crop của hình chưa dùng là tiêu chí riêng. |
| hoi_an_fresh_chili, hoi_an_purple_shallot | Đã có icon alpha chạy được, hình flat tạm khác nét painted LBC. Cần icon đồng bộ; dùng một binding cho mỗi ID trên mọi map. |
| jeonju_recipe_card, hue_market_note | Đang dùng scroll category icon. Cần icon thẻ món ăn Jeonju / giấy nhắn chợ Huế riêng nếu hoàn thiện art. |
| festival_friendship_postcard, hoi_an_bridge_postcard | Dùng JPG minh họa chung có nền hồng/credit, chưa phải bưu thiếp riêng từng địa điểm. Derivative lỗi trong NeedsReview không được gắn. |
| Lumi H3 | Đã có standing sprite đúng vai trò. Cần idle/walk cycle cùng góc nhìn cố định, cùng tỷ lệ/pivot; không yêu cầu bốn hướng và không dùng portrait đưa tay làm walk. |
| Môi trường H3 | Đã nâng cấp scene tại chỗ: hai nhà cổ, tường vườn, cây/chậu, thùng, nền đá liên tục và HUD 9-slice; 10 sprite runtime đăng ký trong Art Registry. Các hình học cũ chỉ còn renderer ẩn/neo collider. Xem [H3 runtime trước/sau](../../H3UpgradeReview.html) và [báo cáo](../../H3UpgradeReport.md). Nền đá là vật liệu AI dẫn xuất 1254×1254; cần người dùng nghiệm thu visual, có thể cần bản artist độ phân giải cao hơn cho close-up. Lumi chưa có walk cycle vẽ riêng. Không coi toàn game đã polish. |
| Enemy Hội An — enemy_mo01, enemy_crab, enemy_ghost | Đã có native sprite alpha được dùng; không còn thiếu icon bot/crab/ghost như báo cáo cũ. Animation/combat presentation vẫn cần hoàn thiện, registry có combat_redesign_pending. |

Cô Mai, Ji-eun, ba vai trò Lumi, marker độc lập, globe/backpack và Overview A đã có
binding. Không thay canonical hoặc tạo lại để lấp danh sách thiếu. Những cutout đã
được tách hỗ trợ AI không được tự tuyên bố thành bộ animation do artist bàn giao.

## Hình mới Huế / Jeonju chưa được duyệt final

Huế/Jeonju đã có sprite cụ thể cho toàn bộ NPC/đối thủ đang cấu hình: Lê An/Hana
là hai native slice từ atlas đã có; Nguyệt, Tâm, Yoon, Park, người giữ chuyện và
tám enemy/boss dùng 13 PNG alpha dẫn xuất riêng. [Provenance](HueJeonjuArtDerivatives.json)
ghi nguồn, hash và trạng thái AI_DERIVATIVE_PROPOSAL / NOT_CANONICAL. Cần người dùng
duyệt ngoại hình, kiểm tra nét ở cỡ nhỏ và artist hoàn thiện animation/combat.
Hương/So-yeon từng có nhãn tạm, nay đã CANONICAL theo xác nhận 17/10. Mai/Ji-eun không đổi. Các nhãn provenance cũ ghi nguồn tạo ảnh, không tự phủ định xác nhận danh tính mới.

Đây không còn là các slot chưa có sprite; chưa coi những hình mới là art final.

## Phần mở rộng — cập nhật Đà Lạt 17/10, hai thành phố còn thiếu runtime

Các mục dưới là **chưa có sprite runtime được đăng ký cho vai trò đó**. Cần tìm và QC
nguồn đã có trước khi yêu cầu vẽ mới; các roster là concept, không tự coi là sprite sạch.
Danh sách NPC theo node và enemy ID đầy đủ được sinh từ dữ liệu thật trong snapshot.

| Thành phố | NPC/actor chưa cấu hình cho phần còn lại | Enemy/boss chưa có binding |
|---|---|---|
| Đà Lạt | D2 Lan, D3 Thủy, D10 chó, D13 người làm vườn đã có sprite đề xuất mới, chưa canonical/final. D4 tái xuất chính thương nhân cũ; D18 là điểm nghỉ với Lumi kể chuyện, chưa có portrait chủ nhà nghỉ riêng | Cả ba enemy đã có sprite dẫn xuất đề xuất; còn cần duyệt art/animation |
| Gyeongju | G3 Min, G9 người trông di tích, G14 người giữ bản khắc | enemy_fox, enemy_stone_guard, enemy_night_guard, enemy_star_core |
| Seoul | S2 quản thủ, S7 kỹ thuật viên, S11 kỹ sư, S14 người bến phà | enemy_drone, enemy_signal_bot, enemy_bridge, enemy_shadow_courier, enemy_final_core |

Không gán lái đò/thương nhân đã có danh tính sang vai trò khác chỉ vì nghề giống nhau.
Nếu xác nhận là cùng người, tái sử dụng nhận diện và ghi rõ liên kết trong catalog.
Không tạo portrait mới cho toàn bộ danh sách khi chưa triển khai các node đó.

Các item còn dùng category art được liệt kê theo itemId trong itemsUsingCategoryArt
của snapshot; đây là nội dung cần hoàn thiện hình ảnh, không phải thiếu logic inventory.
Tất cả 38 item hiện có binding (37 đã phát hành + dâu Đà Lạt đề xuất); không nhân bản icon riêng cho từng thành phố.

Năm Overview A gốc 1448×1086 đã có trong Assets, không thiếu ảnh nền. Huế/Jeonju
và Đà Lạt đã nâng importer để dùng toàn độ phân giải; Gyeongju/Seoul còn mặc định 1024
cho preview, cần nâng trên asset hiện có trước khi bật gameplay. Chưa chứng nhận
toàn bộ waypoint năm thành phố an toàn. Các tuyến Huế/Jeonju mới đã author riêng,
kiểm thử được ghi trong báo cáo hai chương; đo đi trùng geometry không thay thế
chứng nhận toàn bộ địa hình bằng mắt. Chưa có kiểm thử Android vật lý.

## Đà Lạt: giới hạn hình ảnh còn lại

Bảy sprite mới được ghi nguồn/hash/alpha bounds tại [DaLatArtDerivatives](DaLatArtDerivatives.json), prompt dùng built-in image_gen tại [DaLatImagePrompts](DaLatImagePrompts.json). Đây là art ĐỀ XUẤT, không phải canonical/final/animation. Các draft đầu giữ làm nguồn, không được bind vào runtime. Dâu dùng lại native sprite `ItemSheet_VNKR_FoodCulture_v1_9` hiện hữu, không tạo bitmap hay slice trùng. Atisô/cà phê có art trong sheet nhưng chưa có itemId/gameplay Đà Lạt riêng; không gắn bừa làm potion. Chưa có scene exploration Đà Lạt do canonical không yêu cầu node exploration tại đây. Cần review thêm footprint đường đô thị, lối ngắm thác và alpha khi thu nhỏ thực tế.
