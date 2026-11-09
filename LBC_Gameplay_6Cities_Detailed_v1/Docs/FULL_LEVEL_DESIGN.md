# LIGHT BETWEEN CULTURES – LEVEL DESIGN ĐẦY ĐỦ SÁU THÀNH PHỐ

**Tài liệu thiết kế (CHƯA phải build có thể chơi).** Đặt node theo 6 ảnh Map_Overview_A trong bộ ảnh trước. Ảnh ghép thành bản đồ giả tưởng, không phải Google Maps và không đảm bảo đúng địa lý ngoài đời.

## Tọa độ và camera

- **X,Y trong [0..1000], gốc (0,0) ở GÓC TRÊN-TRÁI của chính ảnh map A**, x tăng sang phải, y tăng xuống dưới. Ví dụ (250,600) = 25% chiều ngang, 60% chiều cao. KHÔNG phải tọa độ thực thế giới hay pixel.
- Quy đổi qua RectTransform: anchoredNormalized = (x/1000, 1-y/1000) nếu UI dùng gốc dưới trái; tính offsets theo size của đúng background, không theo Screen.width. WorldToMap tương tự qua localRect.
- **Điểm đặt là anchor thiết kế gần vị trí được vẽ**, cần tinh chỉnh khi zoom và kiểm tra góc đường có thật. `waypoints` chỉ là phác thảo, đường nối trong ảnh tô chú giải KHÔNG khẳng định walkability.
- CityMap: di chuyển theo node và polyline có tween; Free Exploration: đi theo colliders/tilemap trong scene riêng. Cả hai chế độ không dùng ảnh phẳng để tính walkability.
- Nhân vật Lumi chỉ yêu cầu một góc nhìn cố định như game đã chốt. Zoom màn dọc không làm icon UI phóng to.

## Quy ước node và điều kiện

- Xanh lam: NPC; tím: đố; vàng: vật phẩm; đỏ/hồng: quái/boss; trắng: nút đường; vàng cam: trạm sáng; xanh ngọc: cổng; xanh lá: điểm bí mật.
- Một cạnh có `all_flags` chỉ được đi nếu ĐÃ thỏa tất cả; khi chưa thỏa marker lock và tooltip yêu cầu. Không được phá vòng gameplay bằng đường vòng không kiểm tra.
- Node `visible_when.all_flags`: chưa đủ thì ẩn node (nếu secret) hoặc hiện khóa mờ (portal), tùy kiểu; combat path phải được soát lại khi code.
- Khi qua node trung gian, chỉ nhặt item nếu là auto-pickup và event đã khai báo; không tự bật hội thoại/phụ cảnh ngoài mục tiêu người chơi chọn.
- Những event đã thưởng `one_shot` không được tự phát lại sau save/load hoặc đi lại.
- Danh sách `grants_when_completed` CHỈ áp dụng sau khi xác nhận tương tác/quest turn-in/chiến thắng, **không áp dụng tự động khi chỉ chạm node NPC**; đặc biệt H10, D2, J16.
- Với node tự do H3, vật phẩm nằm TRONG scene, không nhặt khi chạm node CityMap.

## CỐT TRUYỆN CÙNG HỆ THỐNG THẾ GIỚI

**Bối cảnh giả tưởng:** Lumi đi qua Hội An → lựa chọn 2 nhánh: (Huế → Đà Lạt) và (Jeonju → Gyeongju). Hai nhánh hội tụ ở Seoul. Nhân vật giúp người dân, lấy manh mối, khám phá bot đã bị can thiệp và thắp lại 6 đài đèn.
**Twist:** Một bot đã cảnh báo về việc bật 6 trạm cùng lúc. Phản diện điều khiển các tín hiệu giả để chia cắt người dân; đánh hết bot là không đủ, người chơi cần đồng bộ các giá trị nhận từ cả Việt Nam lẫn Hàn Quốc.

## Lộ trình hoàn thành dự kiến

1. Hội An: H2→H3, H4→H13, H19 hoặc H5, H7; đồng thời mở H20 Huế và H8 Jeonju.
2. Nhánh Việt Nam: U16 mở phép qua sông, U15 lấy thư, U17/U8 giải mã, U19 bật trạm → Đà Lạt. D2→D7 trả kính, D9/D11/D12 đủ phần thức ăn, D10 mở đường, D15→D16, D14 thắp trạm.
3. Nhánh Hàn Quốc: J2 nhận 3 mảnh J7/J6/J11, J12, J17 chỉ mở khi Hội An sáng, J19 bật trạm → Gyeongju. G3 nhận 3 sao G5/G6/G10, G4/G12 (dùng ấn Huế), G18→G19.
4. Seoul: mở chỉ khi các trạm Huế, Đà Lạt, Jeonju, Gyeongju đã sáng. S16 (Đà Lạt), S17 (Huế), S18 (Jeonju), S20 boss 2 pha, S21 kết thúc.
5. Có thể đi quay lại làm quest phụ, không xóa bản đồ đã qua.

## Ghi chú balancing

- Lumi không có đội 6 quái theo kiểu bắt Dynamons; chỉ **một nhân vật** và một số đồng hành tùy nhiệm vụ.
- Giao chiến đơn giản: mỗi lượt **Đánh / Đỡ / Kỹ năng đèn**, boss có cơ chế gỡ khiên. Năng lượng đèn hồi bằng cách nghỉ ở Safe Zone, vật phẩm chỉ có vai trò nhẹ.
- Quy đổi thời gian mục tiêu: Hội An 10–15 phút; Huế 12–18; Đà Lạt 15–20; Jeonju 12–18; Gyeongju 15–20; Seoul 15–25. Tổng 80–115 phút khi chơi lần đầu; với hạn cuộc thi, cắt xuống vertical slice Hội An+Jeonju và teaser Seoul nếu thiếu thời gian.
- Mức HP/atk hiện là giá trị thử nghiệm, điều chỉnh qua playtest và không dùng level cứng để chặn người chơi trong game thiên về khám phá.

## Hội An (22 node, 29 tuyến)

**Nhận diện:** Phố cổ vàng – đèn lồng – sông Hoài. **Spawn:** H0.

**Mở đầu:** Lumi đến Hội An giữa đêm hội, thấy nhiều đèn lồng sáng yếu. Cô Mai không buộc tội ai; cô chỉ nhờ Lumi kiểm tra hệ thống vì ánh sáng bị thu gom một cách kỳ lạ.

**Nhiệm vụ chính:** Cô Mai cần Mắt Đèn (H3), Lõi Giấy Sáng (H13) và Chìa Kết Nối (H5). Có thể hạ Bot Mờ-01 hoặc giải mật mã tại H19 để vô hiệu hóa nó. Lắp đủ ở H7 để đèn cả phố sáng trở lại.

**Twist:** Bot Mờ-01 phát lời nhắn “đừng thắp tất cả cùng lúc”; sau này tại Seoul mới hiểu nó là cảnh báo bị cắt ghép từ hệ thống phòng vệ. Đèn Hội An hé lộ chuỗi ký hiệu trên đèn hanji ở Jeonju.

**Kết quả:** Mở Huế và Jeonju song song; người chơi tự chọn hướng đi.

| Node | (X,Y) | Kiểu | Nội dung khi tới |
|---|---:|---|---|
| H0 – Bến thuyền khởi hành | (565,735) | start | Lumi lên bờ tại cầu tàu, học cách đi node |
| H1 – Ngã ba phố đèn | (365,442) | junction | Ngã rẽ đèn lồng và phố cổ |
| H2 – Tiệm đèn Cô Mai | (332,421) | npc | Nhận HQ01; mỗi món trả một lần, đèn tại tiệm sáng dần. |
| H3 – Hẻm mái ngói | (420,317) | exploration | Scene H3 có spawn P, item mắt đèn I, exit E; lấy một lần; không đi xuyên vật cản. |
| H4 – Chợ ven sông | (225,619) | puzzle | Đổi vị trí 3 ô bằng tap; hoàn thành mở xưởng H13. |
| H5 – Bến gỗ máy canh | (783,724) | boss | Hai cách: giao chiến 1v1 hoặc có mã H19 để hack. Nhận link_key một lần. |
| H6 – Cầu cổ ánh đèn | (725,520) | junction | Cây cầu đá làm đầu mối hai bờ |
| H7 – Trạm đèn hội quán | (432,143) | beacon | Tiêu thụ/đánh dấu linh kiện; bật ảnh ánh sáng overlay, mở H8 và H20. |
| H8 – Cổng giao lưu Jeonju | (158,164) | portal | Cổng Việt–Hàn qua mái đình nhỏ |
| H9 – Ngõ đèn thất lạc | (133,366) | secret | Ngõ ẩn sau khi nhận bản đồ thương nhân |
| H10 – Quầy hàng Bà Tư | (160,550) | npc | HQ02: nhận dây từ H12, trả nhận bản đồ phát hiện H9. |
| H11 – Rương gỗ bến nước | (290,725) | item | Nhặt dây đồng sau khi mở rương |
| H12 – Cua trộm dưới bờ | (525,671) | enemy | Combat nhỏ 2 lượt hoặc đưa mồi để cua nhả dây. |
| H13 – Xưởng ghép giấy | (520,430) | puzzle | Chọn trong 3 mẫu giấy; dùng manh mối H4, nhận lõi giấy. |
| H14 – Nghệ nhân ông Phúc | (640,341) | npc | Gợi ý H19; tặng trà sau khi giúp. |
| H15 – Lối đèn lập lòe | (919,428) | enemy | Không phải linh hồn tà ác; có thể dùng lõi nhỏ làm dịu hoặc combat tránh đòn. |
| H16 – Cầu tàu trung tâm | (680,676) | junction | Đổi hướng sang bến gỗ |
| H17 – Bến đò phía đông | (945,586) | npc | Mở shortcut sau quest HQ04; không vượt nước bằng đường bộ. |
| H18 – Tiệm trà khách lạ | (537,396) | npc | Foreshadow Jeonju, mở sidequest thư hồi âm về Hội An. |
| H19 – Ngõ gốm hoa | (695,223) | puzzle | Giải puzzle lấy mã vô hiệu hóa H5; không bắt buộc chiến đấu. |
| H20 – Cổng đi Huế | (496,106) | portal | Mở sau khi thắp trạm Hội An |
| H21 – Rương áp mái | (304,284) | item | Hòm phụ có tiền và giấy chỉ lối |

**Kịch bản lời thoại / sự kiện:**

- **H2 – Cô Mai:** “Đèn lồng nhà cô tắt cùng lúc. Giúp cô tìm mắt đèn trong hẻm, lõi giấy ở xưởng và chìa trạm nhé!” Cơ chế: Nhận HQ01; mỗi món trả một lần, đèn tại tiệm sáng dần. Kết quả: `quest:HQ01`.
- **H3 – Hẻm mái ngói:** “Lối này hẹp, chú ý thùng gỗ và rãnh nước.” Cơ chế: Scene H3 có spawn P, item mắt đèn I, exit E; lấy một lần; không đi xuyên vật cản. Kết quả: `item:lamp_eye`.
- **H4 – Người bán hàng:** “Sắp ba mảnh hình theo họa tiết xuất hiện trên mái nhà.” Cơ chế: Đổi vị trí 3 ô bằng tap; hoàn thành mở xưởng H13. Kết quả: `flag:hoi_an.market_puzzle_solved`.
- **H5 – Bot Mờ-01:** “Dữ liệu bị chỉnh sửa... Đừng để sáu trạm cùng nhấp nháy!” Cơ chế: Hai cách: giao chiến 1v1 hoặc có mã H19 để hack. Nhận link_key một lần. Kết quả: `item:link_key|flag:hoi_an.mo01_disabled`.
- **H7 – Trạm Đèn Hội Quán:** “Ba linh kiện đã đồng bộ. Bạn muốn thắp sáng trạm không?” Cơ chế: Tiêu thụ/đánh dấu linh kiện; bật ảnh ánh sáng overlay, mở H8 và H20. Kết quả: `flag:hoi_an.lamp_restored`.
- **H10 – Bà Tư:** “Có con cua cứ kéo đồ của bà xuống mép nước. Cho bà xin lại cuộn dây!” Cơ chế: HQ02: nhận dây từ H12, trả nhận bản đồ phát hiện H9. Kết quả: `quest:HQ02`.
- **H12 – Cua Trộm:** “Kẹt kẹt! Đừng lấy kho báu của ta!” Cơ chế: Combat nhỏ 2 lượt hoặc đưa mồi để cua nhả dây. Kết quả: `item:copper_wire`.
- **H13 – Xưởng ghép giấy:** “Đèn phố nào phản chiếu đúng họa tiết ở sạp chợ?” Cơ chế: Chọn trong 3 mẫu giấy; dùng manh mối H4, nhận lõi giấy. Kết quả: `item:hanji_core`.
- **H14 – Ông Phúc:** “Có người đã thay mã trạm bằng hình gốm. Nhìn hoa văn ở ngõ trên.” Cơ chế: Gợi ý H19; tặng trà sau khi giúp. Kết quả: `hint:puzzle_pottery`.
- **H15 – Đèn Lồng Ma:** “Một chiếc đèn yếu ớt bay vòng vòng.” Cơ chế: Không phải linh hồn tà ác; có thể dùng lõi nhỏ làm dịu hoặc combat tránh đòn. Kết quả: `flag:hoi_an.ghost_freed`.
- **H17 – Bác Lái Đò:** “Đưa bác năm đồng xu hoặc sửa mái chèo là qua nhánh đông được.” Cơ chế: Mở shortcut sau quest HQ04; không vượt nước bằng đường bộ. Kết quả: `flag:hoi_an.ferry_available`.
- **H18 – Khách lạ Hàn Quốc:** “Tôi từng thấy hoa văn ấy tại làng Jeonju...” Cơ chế: Foreshadow Jeonju, mở sidequest thư hồi âm về Hội An. Kết quả: `hint:jeonju_lantern`.
- **H19 – Ngõ gốm hoa:** “Sao chép chuỗi Hoa – Trăng – Đèn theo tranh gốm.” Cơ chế: Giải puzzle lấy mã vô hiệu hóa H5; không bắt buộc chiến đấu. Kết quả: `flag:hoi_an.mo01_hack_code`.

**Điều kiện khóa tuyến chính:**
- H7 ↔ H8: cần hoi_an.lamp_restored
- H7 ↔ H20: cần hoi_an.lamp_restored
- H17 ↔ H5: cần hoi_an.ferry_available
- H5 ↔ H6: cần hoi_an.mo01_disabled

## Huế (22 node, 26 tuyến)

**Nhận diện:** Sông Hương chia hai bờ – Kinh thành và khu phố. **Spawn:** U0.

**Mở đầu:** Ở hai bờ sông Hương, một bức thư bị thất lạc khiến học giả không thể đọc ký hiệu tại cổng thành. Lối qua cầu tạm thời bị bot tự động khóa.

**Nhiệm vụ chính:** Lấy Thẻ Qua Sông ở U16 hoặc giúp lái đò U3; cứu kiện thư từ U15; tìm thảo dược cho Bà Nguyệt ở U10; giải mã thư khố U17 và áp dụng vào U8. Hạ/vô hiệu hóa Bot Thân Vệ, thắp U19.

**Twist:** Ấn Tín Cổ ở U11 không phải vật phẩm “mở mọi cửa”; nó chứa mẫu ký tự thiên văn dùng cho bệ phong ấn Gyeongju và rơle Seoul.

**Kết quả:** Mở Đà Lạt; gửi mã Huế sang Seoul và Gyeongju.

| Node | (X,Y) | Kiểu | Nội dung khi tới |
|---|---:|---|---|
| U0 – Bến sông phía phố | (138,620) | start | Điểm Lumi tới bờ nam sông Hương |
| U1 – Chợ xưa bờ nam | (267,610) | npc | Giao nhiệm vụ chuyển kiện thư |
| U2 – Nhà thuốc Bà Nguyệt | (432,653) | npc | Lấy royal_herb tại U10 và trả U2. |
| U3 – Bến đò bờ nam | (535,502) | npc | Nhận thẻ U16. boat animation U3–U4 chỉ thực hiện khi mở. |
| U4 – Đầu cầu vào thành | (666,430) | junction | Đầu cầu đi qua sông Hương |
| U5 – Bot Thân vệ Cổng | (605,382) | boss | Thắng combat hoặc dùng ancient_seal + mật mã scholar để hack. |
| U6 – Cửa Ngọ Môn cách điệu | (603,333) | junction | Cổng kiến trúc là điểm giao nhiệm vụ |
| U7 – Nho sinh Lê An | (773,231) | npc | Nhận UQ01; thưởng manh mối sao cho Gyeongju. |
| U8 – Sân điện chính | (708,153) | puzzle | Puzzle tap 3 lá cờ, không trắc nghiệm lịch sử. |
| U9 – Vườn phía tây | (490,172) | junction | Đường dẫn ra thảo dược |
| U10 – Vườn thuốc bờ bắc | (376,276) | item | Nhặt royal_herb một lần. |
| U11 – Gốc cây dấu ấn | (516,308) | item | Nhặt ancient_seal, G12/S17 dùng flag sở hữu. |
| U12 – Bến sông tây | (266,336) | enemy | Nhiễu sáng xuất hiện ven bờ |
| U13 – Bến thuyền hẻm | (756,848) | secret | Bến ẩn dẫn tới hòm ghi chép |
| U14 – Cầu ngõ phía đông | (768,735) | junction | Cầu nhỏ nối nhánh đường dân cư |
| U15 – Máy thu thư | (325,557) | enemy | Thắng hoặc đưa thư giả qua câu đố U16 để nhận lost_letter. |
| U16 – Sạp mật mã chợ | (242,686) | puzzle | Chọn đúng 2/3 biểu tượng nhận river_pass. |
| U17 – Thư khố Hoàng thành | (819,116) | puzzle | Đổi thành nhịp trống U8, lưu hint cho G12. |
| U18 – Sân thử thách Hoàng môn | (658,258) | enemy | Bot phụ canh cơ cấu cổ |
| U19 – Đài đèn thành nội | (721,95) | beacon | Cần thư, puzzle điện, bot gate; bật hue.beacon_lit. |
| U20 – Cổng dẫn Đà Lạt | (900,273) | portal | Mở nhánh cao nguyên |
| U21 – Lối đi ẩn bên tường | (858,380) | secret | Chứa Mảnh Chữ Sao cho Gyeongju |

**Kịch bản lời thoại / sự kiện:**

- **U2 – Bà Nguyệt:** “Thuốc cho người chèo đò sắp hết. Con tìm lá bên vườn phía bắc giúp bà nhé.” Cơ chế: Lấy royal_herb tại U10 và trả U2. Kết quả: `quest:UQ02`.
- **U3 – Bác Lái Đò Tâm:** “Muốn qua sông con cần thẻ đò ở chợ, hoặc giúp ta sửa thuyền.” Cơ chế: Nhận thẻ U16. boat animation U3–U4 chỉ thực hiện khi mở. Kết quả: `quest:UQ03`.
- **U5 – Bot Thân Vệ Hoàng Môn:** “Cổng đang khóa vì một tín hiệu giả.” Cơ chế: Thắng combat hoặc dùng ancient_seal + mật mã scholar để hack. Kết quả: `flag:hue.guard_disabled`.
- **U7 – Nho Sinh Lê An:** “Bức thư cũ không thể đọc thiếu ký hiệu ở thư khố. Xin nhờ bạn.” Cơ chế: Nhận UQ01; thưởng manh mối sao cho Gyeongju. Kết quả: `quest:UQ01`.
- **U8 – Sân Điện:** “Ba lá cờ, ba luồng sáng: xếp theo bóng đổ trên sân.” Cơ chế: Puzzle tap 3 lá cờ, không trắc nghiệm lịch sử. Kết quả: `flag:hue.court_solved`.
- **U10 – Lá Thuốc:** “Mùi thảo dược nhẹ bên rìa vườn.” Cơ chế: Nhặt royal_herb một lần. Kết quả: `item:royal_herb`.
- **U11 – Ấn Tín Cổ:** “Trên ấn có ba nét khắc như chòm sao.” Cơ chế: Nhặt ancient_seal, G12/S17 dùng flag sở hữu. Kết quả: `item:ancient_seal`.
- **U15 – Bot Chuyển Thư:** “Tôi được lệnh giữ kiện giấy có dấu đỏ.” Cơ chế: Thắng hoặc đưa thư giả qua câu đố U16 để nhận lost_letter. Kết quả: `item:lost_letter`.
- **U16 – Sạp Đố Vải:** “Tìm số đèn có cùng nét khắc trên tấm vải.” Cơ chế: Chọn đúng 2/3 biểu tượng nhận river_pass. Kết quả: `item:river_pass`.
- **U17 – Thư khố:** “Mã ánh sáng có nhịp ngắn – dài – ngắn.” Cơ chế: Đổi thành nhịp trống U8, lưu hint cho G12. Kết quả: `flag:hue.archive_decoded`.
- **U19 – Đài đèn thành nội:** “Trạm được đồng bộ từ hai bờ.” Cơ chế: Cần thư, puzzle điện, bot gate; bật hue.beacon_lit. Kết quả: `flag:hue.beacon_lit`.

**Điều kiện khóa tuyến chính:**
- U3 ↔ U4: cần hue.crossing_permit
- U4 ↔ U5: cần hue.crossing_permit
- U5 ↔ U6: cần hue.guard_disabled
- U10 ↔ U9: cần hue.north_garden_open
- U7 ↔ U20: cần hue.beacon_lit
- U4 ↔ U14: cần hue.crossing_permit

## Đà Lạt (22 node, 26 tuyến)

**Nhận diện:** Hồ – đồi thông – mây sương – nhà mái dốc. **Spawn:** D0.

**Mở đầu:** Sương mù che con đường quanh hồ. Lan mất kính nên không thể dẫn ai đến đài quan sát; một chú chó đói chắn lối an toàn.

**Nhiệm vụ chính:** Lấy kính từ Bot Đánh Cắp ở D7, trả Lan D2 để mở đường qua sương D8. Kiếm ba phần thức ăn ở D9, D11, D12 (có tuyến thay thế D3), cho chó D10, qua D15 tới thác D16 và thắp đài D14.

**Twist:** Bóng sương đáng sợ thật ra là tín hiệu cảnh báo cũ đang tự nhân bản; sau khi giúp chó và Lan, một lối đi không đánh nhau mở ra, và sương tan trên bản đồ.

**Kết quả:** Kích hoạt nguồn năng lượng sương cho một trạm ở Seoul.

| Node | (X,Y) | Kiểu | Nội dung khi tới |
|---|---:|---|---|
| D0 – Bến hồ Xuân Hương | (515,598) | start | Điểm Lumi tới khu hồ |
| D1 – Quảng trường dốc | (632,245) | junction | Quảng trường nhà cửa ven hồ |
| D2 – Người dẫn đường Lan | (532,389) | npc | Nhận DQ01; sau trả kính thì nút D8 bớt sương, mở hướng dẫn. |
| D3 – Tiệm hoa Thủy | (445,456) | npc | Một phần thịt dự phòng thay thế cho nguồn tại D12 nếu puzzle khó. |
| D4 – Chợ trên đồi | (675,220) | npc | Người bán có manh mối thú hoang |
| D5 – Lối rừng thông | (279,570) | junction | Đường mòn lên rừng |
| D6 – Sói sương mù | (203,420) | enemy | Combat tự chọn hoặc dùng đèn soi để né; tránh grind. |
| D7 – Trạm quan sát sương | (374,312) | enemy | Combat 1v1 hoặc bẫy ánh sáng tương tác 2 lần; nhận lan_glasses. |
| D8 – Ngõ sương vòng | (204,287) | puzzle | Puzzle đường rẽ 3 cổng, mở D9. |
| D9 – Đài nhìn thông | (155,236) | item | meat_portion +1, fog_battery. |
| D10 – Chó canh đói | (390,732) | npc | Đưa 3 meat_portion, chó né đường và trở thành NPC trợ giúp. |
| D11 – Lối vườn hoa | (140,725) | item | meat_portion +1. |
| D12 – Cối xay cạnh hồ | (366,635) | puzzle | Cơ chế xoay 3 hướng, meat_portion +1. |
| D13 – Vườn kính hoa | (799,427) | npc | Sidequest DQ04, phát hiện D19. |
| D14 – Đài dẫn sáng núi | (849,114) | beacon | Cần fog_cleared + blue_water_signal, bật trạm. |
| D15 – Bóng sương cầu đá | (671,713) | boss | Có thể giảm độ khó nếu giúp Lan và chó; chiến thắng nhận lối D16. |
| D16 – Thác hồi âm | (707,795) | item | Nhặt blue_water_signal cho Seoul S16. |
| D17 – Lối bí mật bờ đông | (941,783) | secret | Đường bí mật sau khi cho chó ăn |
| D18 – Nhà nghỉ trên dốc | (309,471) | npc | Hồi phục và dẫn đường |
| D19 – Vườn hoa đêm | (147,160) | secret | Chỉ hiện sau khi xua sương |
| D20 – Cầu dẫn hồ | (720,412) | junction | Cầu nối bờ đông |
| D21 – Cửa đồng bộ Seoul | (873,644) | portal | Chỉ gửi tín hiệu, không vào Seoul ngay |

**Kịch bản lời thoại / sự kiện:**

- **D2 – Người dẫn đường Lan:** “Không có kính, chị chẳng đọc được cột chỉ dẫn. Tìm giúp chị nhé.” Cơ chế: Nhận DQ01; sau trả kính thì nút D8 bớt sương, mở hướng dẫn. Kết quả: `quest:DQ01`.
- **D3 – Cô Thủy chủ tiệm hoa:** “Bác chó thích thức ăn; chị có phần dự phòng đổi một bông hoa.” Cơ chế: Một phần thịt dự phòng thay thế cho nguồn tại D12 nếu puzzle khó. Kết quả: `alternate_item:meat_portion`.
- **D6 – Sói Sương:** “Một sinh vật được sương tạo thành.” Cơ chế: Combat tự chọn hoặc dùng đèn soi để né; tránh grind. Kết quả: `flag:da_lat.wolf_cleared`.
- **D7 – Bot Giấu Kính:** “Nó cầm cặp kính và chạy giữa rừng.” Cơ chế: Combat 1v1 hoặc bẫy ánh sáng tương tác 2 lần; nhận lan_glasses. Kết quả: `item:lan_glasses`.
- **D8 – Trụ Đèn Trong Sương:** “Chọn cột đèn đúng theo bản phác thảo của Lan.” Cơ chế: Puzzle đường rẽ 3 cổng, mở D9. Kết quả: `flag:da_lat.fog_path_solved`.
- **D9 – Rương Trên Đồi:** “Trong rương có một gói đồ ăn và pin cũ.” Cơ chế: meat_portion +1, fog_battery. Kết quả: `item:meat_portion|item:fog_battery`.
- **D10 – Chó Canh Đường:** “Gừ... bụng đói, đừng lại gần.” Cơ chế: Đưa 3 meat_portion, chó né đường và trở thành NPC trợ giúp. Kết quả: `flag:da_lat.dog_fed`.
- **D11 – Rương Vườn Hoa:** “Rương ở bên luống hoa tím.” Cơ chế: meat_portion +1. Kết quả: `item:meat_portion`.
- **D12 – Cối Xay:** “Xoay cánh theo hướng gió trên ba ngọn thông.” Cơ chế: Cơ chế xoay 3 hướng, meat_portion +1. Kết quả: `item:meat_portion`.
- **D13 – Người Làm Vườn:** “Có hoa chỉ xuất hiện sau sương...” Cơ chế: Sidequest DQ04, phát hiện D19. Kết quả: `quest:DQ04`.
- **D14 – Trạm Đèn Trên Đồi:** “Ánh sáng đẩy lùi sương mù.” Cơ chế: Cần fog_cleared + blue_water_signal, bật trạm. Kết quả: `flag:da_lat.beacon_lit`.
- **D15 – Bóng Sương Cầu Đá:** “Ta chỉ giữ đường tránh người rơi xuống thác.” Cơ chế: Có thể giảm độ khó nếu giúp Lan và chó; chiến thắng nhận lối D16. Kết quả: `flag:da_lat.mist_boss_cleared`.
- **D16 – Thác Hồi Âm:** “Mặt nước phản ánh một mã xanh.” Cơ chế: Nhặt blue_water_signal cho Seoul S16. Kết quả: `item:blue_water_signal`.

**Điều kiện khóa tuyến chính:**
- D7 ↔ D8: cần da_lat.glasses_returned
- D11 ↔ D10: cần da_lat.west_path_open
- D10 ↔ D15: cần da_lat.dog_fed
- D15 ↔ D16: cần da_lat.mist_boss_cleared
- D13 ↔ D14: cần da_lat.fog_cleared
- D20 ↔ D15: cần da_lat.dog_fed

## Jeonju (22 node, 30 tuyến)

**Nhận diện:** Làng hanok – hanji – mái ngói – sân cổ. **Spawn:** J0.

**Mở đầu:** Làng hanok đang chuẩn bị buổi trưng bày đèn hanji, nhưng khuôn hoa văn bị chia thành ba mảnh và người đưa kiện hàng biến mất.

**Nhiệm vụ chính:** Giúp Hana J2: lấy mảnh 1 từ gấp giấy J7, mảnh 2 trong bưu kiện giữ bởi bot J6, mảnh 3 ở đài J11. Dùng thứ tự mẫu đèn được mở ở Hội An để giải đèn J17, thắp trạm J19.

**Twist:** Lá thư gửi ngược về Hội An ở J21 cho biết một chiếc đèn “bị đánh cắp” đang được dùng che chở cho linh hồn lạc; người chơi mở sidequest cứu đèn tại H9.

**Kết quả:** Mở Gyeongju, gửi mạch đèn hanji đến Seoul.

| Node | (X,Y) | Kiểu | Nội dung khi tới |
|---|---:|---|---|
| J0 – Cổng làng phía nam | (497,907) | start | Lumi tới Jeonju qua cổng giao lưu |
| J1 – Ngã ba hanok | (463,398) | junction | Quảng trường cây phong và tiểu cảnh |
| J2 – Nghệ nhân Hana | (666,473) | npc | Nhận JQ01, mở J12 sau đủ 3 mảnh. |
| J3 – Xưởng nhuộm vải | (312,333) | puzzle | Khôi phục sắc hoa văn |
| J4 – Nhà thầy Yoon | (435,245) | npc | Đọc hướng dẫn chuỗi họa tiết |
| J5 – Tiệm hàng chuyển | (684,566) | npc | Nhận JQ02; thắng/đàm phán bot J6 để lấy kiện. |
| J6 – Bot Cắp Kiện | (822,631) | enemy | Thắng 1v1 hoặc giải mật mã kho, thả parcel và hanji_pattern_2. |
| J7 – Thư viện giấy hanji | (331,202) | puzzle | 2 lần xoay/1 lần gấp, nhận hanji_pattern_1. |
| J8 – Cầu đá tây | (257,455) | junction | Lối qua suối hẹp |
| J9 – Bờ suối nước chảy | (127,518) | enemy | Hạc giấy nhiễu canh đường |
| J10 – Rương ven hồ | (105,793) | item | Rương có sáp và tiền |
| J11 – Đài ngắm mái ngói | (736,130) | item | Chỉ nhặt sau khi trả kiện cho Park, nhận hanji_pattern_3. |
| J12 – Sân thêu họa tiết | (735,303) | puzzle | Cần 3 mảnh, chọn đúng thứ tự theo hint J4/J17. |
| J13 – Cổng sân cổ | (554,181) | junction | Điểm hội tụ đường phía bắc |
| J14 – Bot Hoa Văn | (846,350) | enemy | Có thể dùng hoa văn đã ghép vô hiệu hóa hoặc combat. |
| J15 – Chợ hanok | (659,678) | npc | Bán vật phẩm hồi phục |
| J16 – Người giữ chuyện | (415,551) | npc | Kể về cổng từ Hội An |
| J17 – Đèn song thành | (533,351) | puzzle | Dựa vào hoi_an.lamp_restored; xoay 3 vòng đèn, mở trạm J19. |
| J18 – Hạc giấy bóng tối | (814,845) | boss | Boss tùy chọn có thư hồi âm quan trọng, hoặc giải bùa bảo vệ. |
| J19 – Trạm ánh sáng hanji | (567,115) | beacon | Cần 3 hoa văn + đèn J17; mở Gyeongju. |
| J20 – Cổng đi Gyeongju | (883,155) | portal | Mở đường di tích Silla |
| J21 – Lối thư hồi âm | (152,263) | secret | Lấy reply_letter cho H18, bật ngõ H9. |

**Kịch bản lời thoại / sự kiện:**

- **J2 – Nghệ nhân Hana:** “Một tấm khuôn bị chia ba trước lễ hội. Giúp tôi tìm lại nhé.” Cơ chế: Nhận JQ01, mở J12 sau đủ 3 mảnh. Kết quả: `quest:JQ01`.
- **J5 – Thương nhân Park:** “Kiện hàng bên suối biến mất rồi!” Cơ chế: Nhận JQ02; thắng/đàm phán bot J6 để lấy kiện. Kết quả: `quest:JQ02`.
- **J6 – Bot Cắp Kiện:** “Kiện hàng đã vào danh mục kiểm kê lỗi.” Cơ chế: Thắng 1v1 hoặc giải mật mã kho, thả parcel và hanji_pattern_2. Kết quả: `item:missing_parcel|item:hanji_pattern_2`.
- **J7 – Thư viện Hanji:** “Gấp tờ giấy theo vạch sáng thành bông hoa.” Cơ chế: 2 lần xoay/1 lần gấp, nhận hanji_pattern_1. Kết quả: `item:hanji_pattern_1`.
- **J11 – Đài Mái Ngói:** “Trong hộp kính là mảnh hoa văn thứ ba.” Cơ chế: Chỉ nhặt sau khi trả kiện cho Park, nhận hanji_pattern_3. Kết quả: `item:hanji_pattern_3`.
- **J12 – Sân Thêu:** “Ghép 3 hoa văn, đặt tâm đèn vào vị trí đèn Hội An.” Cơ chế: Cần 3 mảnh, chọn đúng thứ tự theo hint J4/J17. Kết quả: `flag:jeonju.three_patterns_joined|item:hanji_circuit`.
- **J14 – Bot Gác Họa Tiết:** “Sân thêu bị khóa từ mã khác.” Cơ chế: Có thể dùng hoa văn đã ghép vô hiệu hóa hoặc combat. Kết quả: `flag:jeonju.pattern_bot_disabled`.
- **J17 – Đèn Song Thành:** “Một nửa họa tiết chỉ thấy khi Hội An sáng.” Cơ chế: Dựa vào hoi_an.lamp_restored; xoay 3 vòng đèn, mở trạm J19. Kết quả: `flag:jeonju.lantern_puzzle_solved`.
- **J18 – Hạc Giấy Tối:** “Đừng để ta tan trước khi gửi thư!” Cơ chế: Boss tùy chọn có thư hồi âm quan trọng, hoặc giải bùa bảo vệ. Kết quả: `flag:jeonju.paper_crane_cleared`.
- **J19 – Trạm Hanji:** “Sợi giấy nối hai bờ biển qua ánh đèn.” Cơ chế: Cần 3 hoa văn + đèn J17; mở Gyeongju. Kết quả: `flag:jeonju.beacon_lit`.
- **J21 – Thư Để Lại:** “Người lạ nhờ gửi tin về Hội An.” Cơ chế: Lấy reply_letter cho H18, bật ngõ H9. Kết quả: `item:reply_letter`.

**Điều kiện khóa tuyến chính:**
- J12 ↔ J14: cần jeonju.pattern_bot_disabled
- J11 ↔ J20: cần jeonju.beacon_lit
- J17 ↔ J19: cần jeonju.lantern_puzzle_solved

## Gyeongju (22 node, 26 tuyến)

**Nhận diện:** Gò mộ Silla – đài thiên văn – đá cổ. **Spawn:** G0.

**Mở đầu:** Các ký hiệu sao trong di tích Silla bị nhiễu. Một số tượng đá tự chuyển động, còn bản chỉ dẫn bị cắt đôi giữa Gyeongju và Huế.

**Nhiệm vụ chính:** Giúp hướng dẫn viên Min G3: lấy ký hiệu ở G5, G6, G10, rồi xếp lên đài G4; đọc Ấn Tín Cổ Huế để mở bệ G12; vượt tượng G7 và boss G18, thắp trạm G19.

**Twist:** Tượng đá không phải nguồn ác; nó bảo vệ đài thiên văn khỏi lõi nhiễu. Khi người chơi chọn sửa cơ cấu thay vì đánh, tượng đá trở thành NPC hỗ trợ.

**Kết quả:** Nếu cả Huế, Đà Lạt, Jeonju và Gyeongju hoàn thành, Seoul được mở.

| Node | (X,Y) | Kiểu | Nội dung khi tới |
|---|---:|---|---|
| G0 – Cổng vào làng di tích | (284,832) | start | Lumi bắt đầu giữa mái nhà đá |
| G1 – Cầu lối bắc | (450,695) | junction | Cầu nối từ làng tới di tích |
| G2 – Đình ao sen | (213,492) | puzzle | Quan sát phản chiếu để lấy manh mối |
| G3 – Nhà khảo cổ Min | (406,257) | npc | Nhận GQ01, có hình minh họa không yêu cầu kiến thức ngoài. |
| G4 – Đài Cheomseongdae cách điệu | (447,191) | puzzle | Cần 3 symbols; nhận stargazer_lens. |
| G5 – Gò mộ phía tây | (145,199) | item | Nhặt star_symbol_1. |
| G6 – Đường giữa các gò | (280,230) | enemy | Hạ hoặc dụ bằng đồ ăn để nhận symbol 2. |
| G7 – Tượng đá gác lối | (579,394) | enemy | Đàm phán sau câu đố G12 hoặc chiến đấu; kết quả hỗ trợ boss. |
| G8 – Lối chùa đá | (724,421) | junction | Đường dẫn quần thể chùa |
| G9 – Sân chính di tích | (663,309) | npc | Người giữ di tích đưa gợi ý |
| G10 – Đình trên đồi | (797,136) | item | Nhặt star_symbol_3. |
| G11 – Vườn đá trung tâm | (484,507) | junction | Đường rẽ ba hướng |
| G12 – Bệ phong ấn nứt | (609,611) | puzzle | Cần ancient_seal; nhập nhịp ký hiệu đã đọc U17. |
| G13 – Lính canh đêm | (760,649) | enemy | Bot canh đài thắp sáng |
| G14 – Kho bản khắc | (884,352) | npc | NPC bổ trợ giải câu đố biểu tượng |
| G15 – Thềm điện đá | (706,271) | puzzle | Sắp thứ tự ba bóng đổ |
| G16 – Đền dưới hồ | (521,770) | item | Rương hồi phục, cờ cứu hộ |
| G17 – Đài lồng đèn bờ tây | (119,378) | puzzle | Sidequest cần jeonju.beacon_lit, nhận xu & bản vẽ. |
| G18 – Lõi canh tinh tú | (811,233) | boss | Boss có nhịp 3 lượt, khiên mở sau tap sao đúng; nhận silla_prism. |
| G19 – Trạm cộng hưởng Silla | (558,229) | beacon | Bật trạm cần three_stars_placed + star_core_disabled. |
| G20 – Cổng đi Seoul | (889,613) | portal | Hiện sau hai nhánh Việt–Hàn |
| G21 – Lối cỏ dược liệu | (104,763) | secret | Rương phụ dưới gốc cây |

**Kịch bản lời thoại / sự kiện:**

- **G3 – Hướng dẫn viên Min:** “Ba hình sao là chìa khóa để hiểu đài quan sát.” Cơ chế: Nhận GQ01, có hình minh họa không yêu cầu kiến thức ngoài. Kết quả: `quest:GQ01`.
- **G4 – Đài Thiên Văn:** “Đặt ba nét sao theo tia phản chiếu.” Cơ chế: Cần 3 symbols; nhận stargazer_lens. Kết quả: `flag:gyeongju.three_stars_placed|item:stargazer_lens`.
- **G5 – Hốc Đá Bên Gò:** “Một ký hiệu mờ ẩn dưới gốc cây.” Cơ chế: Nhặt star_symbol_1. Kết quả: `item:star_symbol_1`.
- **G6 – Cáo Bóng Mờ:** “Cáo chạy quanh gò đất cùng tấm bia rơi.” Cơ chế: Hạ hoặc dụ bằng đồ ăn để nhận symbol 2. Kết quả: `item:star_symbol_2`.
- **G7 – Tượng Đá Thức Tỉnh:** “Ta bảo vệ người khám phá khỏi sai lầm của ánh sáng.” Cơ chế: Đàm phán sau câu đố G12 hoặc chiến đấu; kết quả hỗ trợ boss. Kết quả: `flag:gyeongju.stone_guard_friend`.
- **G10 – Đình Trên Đồi:** “Từ đây quan sát được mẫu đèn thứ ba.” Cơ chế: Nhặt star_symbol_3. Kết quả: `item:star_symbol_3`.
- **G12 – Bệ Chữ Sao:** “Dấu này giống ấn tín mang từ Huế.” Cơ chế: Cần ancient_seal; nhập nhịp ký hiệu đã đọc U17. Kết quả: `flag:gyeongju.seal_repaired`.
- **G17 – Đài Lồng Đèn:** “Các ký hiệu hanji làm sáng phần bờ tây.” Cơ chế: Sidequest cần jeonju.beacon_lit, nhận xu & bản vẽ. Kết quả: `flag:gyeongju.west_lantern_done`.
- **G18 – Lõi Tinh Tú:** “Dữ liệu sao đang quay đảo...” Cơ chế: Boss có nhịp 3 lượt, khiên mở sau tap sao đúng; nhận silla_prism. Kết quả: `item:silla_prism|flag:gyeongju.star_core_disabled`.
- **G19 – Đài Trạm Silla:** “Ánh sao nối đường đến Seoul.” Cơ chế: Bật trạm cần three_stars_placed + star_core_disabled. Kết quả: `flag:gyeongju.beacon_lit`.

**Điều kiện khóa tuyến chính:**
- G10 ↔ G18: cần gyeongju.three_stars_placed
- G18 ↔ G14: cần gyeongju.star_core_disabled
- G13 ↔ G20: cần gyeongju.beacon_lit, hue.beacon_lit, da_lat.beacon_lit, jeonju.beacon_lit

## Seoul (23 node, 28 tuyến)

**Nhận diện:** Cung điện – sông Hàn – skyline – tháp N Seoul. **Spawn:** S0.

**Mở đầu:** Seoul nhận đồng thời tín hiệu đèn Hội An/Jeonju, chìa Huế/Gyeongju và sương Đà Lạt. Các trạm số đang phát tín hiệu chồng lấn khiến một số bot nhầm lẫn bạn–thù.

**Nhiệm vụ chính:** Khôi phục ba rơle: A D16→S16, B U11→S17, C J19→S18. Gặp kỹ thuật viên S7 và quản thủ S2 để truy nguyên. Đột nhập S20 khi ba rơle đã sáng; đánh Lõi Nhiễu 2 pha hoặc giải cơ cấu vô hiệu hóa trong lúc boss lộ lõi.

**Twist:** “Hội Bóng Mờ” được đồn là kẻ phá hoại, nhưng kẻ cầm đầu thực chất lợi dụng một hệ thống bảo vệ cũ bị lỗi; bot ở Hội An đã cố cảnh báo. Lumi phải chọn phục hồi nó thay vì phá hủy toàn bộ.

**Kết quả:** Festival giả tưởng ở S21: sáu thành phố sáng lên, NPC từ cả hai nước gửi lời cảm ơn; sau credit vẫn quay lại làm sidequest được.

| Node | (X,Y) | Kiểu | Nội dung khi tới |
|---|---:|---|---|
| S0 – Bến công viên sông Hàn | (490,690) | start | Đặt chân đến Seoul |
| S1 – Quảng trường trung tâm | (618,523) | junction | Trung tâm chuyển tuyến sang cũ và mới |
| S2 – Cổng thành xưa | (294,292) | npc | Mở SQ02, gợi ý xem bản ghi lịch sử S4. |
| S3 – Sân cung điện | (274,204) | puzzle | Đọc lời nhắn từ Huế |
| S4 – Hồ sơ lịch sử | (210,380) | item | Tấm bản ghi mối liên hệ Huế |
| S5 – Drone trinh sát | (587,418) | enemy | Đánh hoặc lẩn vào vườn đá, mở tuyến cao ốc. |
| S6 – Bờ đông sông Hàn | (830,666) | junction | Điểm rẽ trạm năng lượng |
| S7 – Quảng trường màn hình | (757,467) | npc | Nhận SQ01; chỉ 3 rơle và điều kiện boss. |
| S8 – Đường cao ốc | (612,367) | enemy | Bot trộn dữ liệu trên lối chính |
| S9 – Đường lên N Seoul Tower | (760,254) | junction | Lối leo đồi có cây hoa |
| S10 – Chân tháp N Seoul | (709,116) | puzzle | Cần stargazer_lens hoặc đúng mẫu ký hiệu, làm sáng đỉnh tháp. |
| S11 – Trạm kiểm soát kỹ thuật | (841,275) | npc | Giải thích twist, mở lựa chọn sửa AI thay vì xóa. |
| S12 – Đảo công viên xanh | (465,641) | item | Rương đồng xu và bộ hồi phục |
| S13 – Cầu ánh sáng phía tây | (344,540) | junction | Cầu nối vùng cổ và đô thị |
| S14 – Bến phà thông tin | (379,801) | npc | Chuyên chở qua sông khi hệ thống sáng |
| S15 – Bot bảo vệ cầu | (411,483) | enemy | Combat hoặc dùng ancient_seal mở nhanh. |
| S16 – Trạm phát điện bờ đông | (872,776) | puzzle | Cần blue_water_signal; puzzle xoay 2 công tắc. |
| S17 – Rơle cung điện | (470,343) | puzzle | Cần ancient_seal + hue.beacon_lit; puzzle 3 nhịp. |
| S18 – Rơle ánh đèn hanji | (660,510) | puzzle | Cần hanji_circuit + jeonju.beacon_lit; puzzle 3 biểu tượng. |
| S19 – Đường hầm dữ liệu | (897,696) | enemy | Thắng hoặc dùng 3 rơle ngắt nguồn, mở S20. |
| S20 – Buồng Lõi Nhiễu | (884,534) | boss | Pha 1 phá khiên bằng rơle; pha 2 3 lựa chọn đánh/sửa/che chắn; tránh soft-lock. |
| S21 – Lễ hội hai nền văn hóa | (318,435) | epilogue | Epilogue 6 map sáng, mở free-roam postgame. |
| S22 – Kính quan sát đồi | (642,208) | secret | Bí mật đọc tin nhắn của Hội Bóng Mờ |

**Kịch bản lời thoại / sự kiện:**

- **S2 – Quản thủ bảo tàng:** “Điều nguy hiểm không chỉ là kẻ phá hoại, mà là mã bảo vệ cũ.” Cơ chế: Mở SQ02, gợi ý xem bản ghi lịch sử S4. Kết quả: `quest:SQ02`.
- **S5 – Drone Trinh Sát:** “Phát hiện ánh sáng không xác thực.” Cơ chế: Đánh hoặc lẩn vào vườn đá, mở tuyến cao ốc. Kết quả: `flag:seoul.drone_disabled`.
- **S7 – Kỹ thuật viên Ji-won:** “Hãy mang về ba tín hiệu từ Việt Nam và Jeonju.” Cơ chế: Nhận SQ01; chỉ 3 rơle và điều kiện boss. Kết quả: `quest:SQ01`.
- **S10 – Đài Tháp N Seoul:** “Ba nguồn sáng luân phiên soi chiếc ống kính.” Cơ chế: Cần stargazer_lens hoặc đúng mẫu ký hiệu, làm sáng đỉnh tháp. Kết quả: `flag:seoul.tower_calibrated`.
- **S11 – Kỹ sư Trạm Số:** “Bot ấy không được sinh ra để đánh nhau.” Cơ chế: Giải thích twist, mở lựa chọn sửa AI thay vì xóa. Kết quả: `hint:seoul_truth`.
- **S15 – Bot Giữ Cầu:** “Mã cầu đã hết hạn.” Cơ chế: Combat hoặc dùng ancient_seal mở nhanh. Kết quả: `flag:seoul.bridge_guard_disabled`.
- **S16 – Rơle A – Hồ Và Sương:** “Nguồn nước Đà Lạt ổn định xung số 1.” Cơ chế: Cần blue_water_signal; puzzle xoay 2 công tắc. Kết quả: `flag:seoul.relay_a_on`.
- **S17 – Rơle B – Kinh Thành:** “Nhịp từ ấn tín Huế phù hợp cổng số 2.” Cơ chế: Cần ancient_seal + hue.beacon_lit; puzzle 3 nhịp. Kết quả: `flag:seoul.relay_b_on`.
- **S18 – Rơle C – Hanji:** “Mạch giấy Jeonju truyền xung số 3.” Cơ chế: Cần hanji_circuit + jeonju.beacon_lit; puzzle 3 biểu tượng. Kết quả: `flag:seoul.relay_c_on`.
- **S19 – Bot Đưa Tin Bóng Mờ:** “Lệnh gửi đi từ lõi đã bị kẻ xấu sửa.” Cơ chế: Thắng hoặc dùng 3 rơle ngắt nguồn, mở S20. Kết quả: `flag:seoul.courier_disabled`.
- **S20 – Lõi Nhiễu Sáng:** “Lệnh ban đầu của ta là bảo vệ những chiếc đèn...” Cơ chế: Pha 1 phá khiên bằng rơle; pha 2 3 lựa chọn đánh/sửa/che chắn; tránh soft-lock. Kết quả: `flag:seoul.final_cleared`.
- **S21 – Lễ Hội Hai Nước:** “Mỗi ánh đèn là một câu chuyện; mỗi người bạn giúp là một nhịp cầu.” Cơ chế: Epilogue 6 map sáng, mở free-roam postgame. Kết quả: `flag:game.ending_seen`.

**Điều kiện khóa tuyến chính:**
- S2 ↔ S21: cần seoul.final_cleared
- S19 ↔ S20: cần seoul.relay_a_on, seoul.relay_b_on, seoul.relay_c_on
