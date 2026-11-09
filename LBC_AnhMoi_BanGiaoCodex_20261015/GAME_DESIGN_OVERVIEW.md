# GAME DESIGN OVERVIEW — LIGHT BETWEEN CULTURES

## Hướng cốt lõi
Casual phiêu lưu 2D, chơi trên Android; tham khảo *nhịp độ* chọn địa điểm và gặp sự kiện của Dynamons World, **không sao chép** đồ họa, màn chơi hay quái thú. Một nhân vật gà Lumi, màn dọc 9:16, góc nhìn cố định hơi từ trên xuống, duy trì **một góc nhìn nhân vật** trong trò chơi. 3 cấp: World Map 6 thành phố → City Map node có đường đi phân nhánh → Free Exploration scenes nhỏ tại một số địa điểm; nơi khác kích hoạt NPC, bot, puzzle hoặc rương ngay từ node.

## Cốt truyện chính
Mạng lưới Cổng Ánh Sáng kết nối những địa điểm văn hóa ở Việt Nam và Hàn Quốc bị nhóm phản diện **Hội Bóng Mờ** làm sai lệch. Lumi đi tìm 3 Mảnh Cộng Hưởng (ký ức/giai thoại, trái tim ánh sáng, bản đồ tín hiệu) và 6 đèn hiệu thành phố; kết nối giữa hai nước được khôi phục bằng cách giúp con người và khắc phục hệ thống — không chỉ đánh bot. Các lễ hội và cổng là yếu tố giả tưởng, còn kiến trúc/địa điểm văn hóa phải được trình bày đúng nguồn.

## Trải nghiệm chính
World map: 6 thành phố trên một bức bản đồ minh họa độc lập, đã mở và chưa mở hiển thị riêng. Hội An mở đầu; mở Huế và Jeonju theo hai nhánh, Huế mở Đà Lạt, Jeonju mở Gyeongju; Seoul cần hoàn thành hai nhánh cùng nhiệm vụ then chốt. Cho phép quay lại thành phố cũ, nhớ trạng thái nhiệm vụ và mở đường khác.
City map: background map minh họa là **nền**, node đặt theo đồ thị có tọa độ điều chỉnh bằng Unity Editor. Không làm khu giải đố tách biệt; nhiệm vụ phân tán tự nhiên tại các nút/ngõ/hẻm, và có đường vòng bí mật. Chạm điểm đã mở → Lumi di chuyển dọc các đoạn nối; chỉ kích hoạt event khi đến nơi.
Free scene: màn chơi nhỏ có nền, sprites, walkable regions, collision, NPC/item/bot; người dùng tap-to-move hoặc joystick đơn giản, dùng chung camera và tương tác. Vật phẩm cần lại gần, không nhặt từ xa.
Combat: trận bot nhỏ theo lượt với 2-3 hành động rõ ràng hoặc puzzle-boss; tránh làm combat ARPG lớn. Động vật chắn đường thường là NPC trung lập, dùng thịt để thỏa thuận đi qua, không mặc định phải đánh.

## SÁU thành phố — art, quest, điểm độc đáo

### 1 Hội An (Việt Nam)
Màu vàng tường cổ, mái ngói, đèn lồng, sông Hoài, Chùa Cầu/cầu nhỏ, phố và bến thuyền. H0-Bến thuyền; H1-Phố đèn; H2-Cô Mai tiệm đèn; H3-Hẻm mái ngói (cảnh tự do và linh kiện); H4-Chợ ven sông (câu đố); H5-Bot ở bến gỗ; H6-Cầu cổ; H7-Trạm sáng; H8-Cổng Jeonju; H9-Hẻm bí mật. Câu đố ở rải rác; 1 linh kiện H3, 1 vật phẩm H4, chìa H5, khôi phục H7. Có lối đi tùy thứ tự H3/H4. Gắn với Jeonju: đèn hiệu Hội An làm hiện con dấu trên lồng đèn ở Jeonju.

### 2 Jeonju (Hàn Quốc)
Làng hanok, sân đá, ngõ nhỏ, giấy hanji, nghề truyền thống; nhân vật nghệ nhân gợi liên hệ bằng **mẫu đèn** của Hội An. Quest: thu thập 3 mảnh hoa văn trong khu nhà nghề, sắp thứ tự theo manh mối NPC, giúp người bán tìm kiện hàng và mở cổng Gyeongju. Twist: ánh sáng được sửa tại Hội An là manh mối giải mật mã Jeonju. Puzzle chọn biểu tượng, không bắt người chơi biết tiếng Hàn.

### 3 Huế (Việt Nam)
Sông Hương chia hai bên map, **một bên Kinh thành/Ngọ Môn**, bên kia khu phố, bến thuyền và chợ. Quest: tìm thẻ qua sông, giúp học giả trẻ, khám phá mật mã ánh sáng cạnh cổng thành, đối mặt Bot Gác Cầu để lấy chìa. Twist: mở phong ấn Huế làm lộ một đường đến Gyeongju hoặc yếu tố bảo vệ ở Seoul.

### 4 Gyeongju (Hàn Quốc)
Lăng mộ cổ kiểu gò, Cheomseongdae, di tích Silla, ao/vườn và đường đá; không biến thành bản sao Jeonju. Quest: tái lập ba vệt ánh sáng theo chỉ dẫn hình sao và vị trí mộ cổ, nhận Mảnh Cộng Hưởng. Có một NPC khảo cổ cung cấp manh mối. Twist: câu đố giải bằng quan sát môi trường chứ không qua câu hỏi kiến thức dài.

### 5 Đà Lạt (Việt Nam)
Đồi dốc, rừng thông, hồ, sương mù, hoa, nhà mái dốc. Quest 1: gặp người đánh rơi kính, tìm kính ở trạm quan sát hoặc thắng bot ăn cắp để được họ chỉ đường qua sương. Quest 2: thú canh lối đi đói, thu thập 3 phần thịt từ rương/câu đố/NPC để nó nhường đường (không phải quái bắt buộc đánh). Quest cuối: kích hoạt đèn dẫn đường trong sương; một ngõ bí mật mở khi đã giúp đủ hai NPC. Twist: người dẫn đường giúp người chơi nhìn thấy node ẩn, có thể quay lại nhặt mảnh còn thiếu.

### 6 Seoul (Hàn Quốc)
Hai mảng cổ/hiện đại: di tích/cổng cổ, sông Hàn & cầu, skyline, N Seoul Tower gợi hình. Quest: bật ba trạm đồng bộ đã nối từ Huế, Jeonju, Đà Lạt và Gyeongju; boss `Lõi Nhiễu Sáng` canh cổng. BOSS lựa chọn chiến đấu theo lượt với 2 pha hoặc giải 2 công tắc tạo thời điểm vô hiệu hóa khiên. Kết thúc: 6 thành phố đồng loạt sáng lên, mở festival giao lưu giả tưởng. Tránh khẳng định lễ hội game là lễ hội truyền thống có thật.

## UI, zoom và lưu
Map pinch-to-zoom + mouse wheel, pan drag; zoom có min/max và clamp để không nhìn ra ngoài map, các node có icon xuất hiện khi đạt trạng thái; thành phố có dấu ấn riêng. Giao diện overlay không scale theo zoom của map. Camera mặc định cung cấp toàn cảnh nhưng trên điện thoại 9:16 phải có pan để xem mọi phần; không làm méo ảnh. Lưu: city/node current, visited/unlocked, quest flags, inventory, defeated enemies, puzzle solved, events rewarded. Kiểm tra save/load thực tế.

## Cơ chế độc đáo và giới hạn
Đặc trưng: sự kiện ở thành phố A thay đổi thành phố B (*Cộng hưởng Song Thành*); NPC/động vật thay vì chỉ cho điểm sẽ giúp mở đường, gợi ý hoặc đồng hành; đường rẽ và nhiệm vụ phụ tác động tuyến sau. Không thêm online, multiplayer, open-world tự do toàn map, skill tree, crafting phức tạp.

## Ưu tiên làm việc
1) Ảnh map Hội An đẹp + đường H0-H9 + Lumi đi theo node. 2) H3 đi tự do + lấy vật phẩm. 3) nhiệm vụ Cô Mai + rương, inventory, save. 4) mở Jeonju và một demo gắn trạng thái cross-city. 5) nhân bản city node map data cho 5 nơi khác; quest và bot mở rộng khi prototype chạy. 6) polish Android + video + PPTX.
