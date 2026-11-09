# Huế / Jeonju — đối chiếu thiết kế và phần author thêm, 17/10/2026

Nguồn: FULL_LEVEL_DESIGN, QUEST_CHAINS_AND_GATES và Data/Cities/quests/items/enemies/
world_progression của LBC_Gameplay_6Cities_Detailed_v1; StoryExpansion Hội An được
giữ cho HQ01–HQ07. Không sửa các bộ nguồn. Framework và checkpoint gần nhất được
đối chiếu trước khi backup BeforeHueJeonjuChapters-20261016.

| Khác biệt trong scaffold | Cách xử lý trong runtime |
|---|---|
| Quest UQ01/JQ03 mặc định hoàn thành ngay tại người giao, chưa có objective typed | Hoàn thành tại U19/J19 đúng bước activate của thiết kế, bắt buộc item/flag thật và người chơi bấm thắp trạm |
| UQ04 mặc định trả ở U11 nhưng objective nguồn yêu cầu nói chuyện U7 | Nhận ở U11, mang ấn đến U7; không tiêu hao ấn dùng xuyên thành phố |
| JQ01 mặc định trả ở J2 dù bước cuối nguồn là solve J12 | Ghép và xác nhận kết thúc tại J12; mạch đèn chỉ do puzzle cấp một lần |
| JQ04 scaffold trả ở J16, objective nguồn yêu cầu H18 | Nhận ở J16, lấy thư J21, xác nhận với Ji-eun H18; sau đó HQ03 giải quyết tại H9 |
| Huế: thư khố/Lê An/ấn nằm sau bot, trong khi bot có nhánh cần mã/ấn | Thêm hai corridor vườn U4–U12, U11–U7. Chúng chỉ đến khu vườn/thư khố ở chặng1, không mở khu trạm/chặng3 trước khi U5 được giải quyết |
| U21 hiện ở chặng3 nhưng chỉ nối qua U20 chặng4 | Chuyển U21 sang chặng4, vẫn cần cờ phát hiện từ thư khố; không hiển thị một bí mật chưa có đường tới |
| Jeonju: J13 ở sau gate ghép khuôn, nhưng J11 mảnh3 cần đi qua J13 | J13 thuộc khu vật liệu chặng1; gate J12 vẫn đòi đủ ba mảnh |
| J16 ở chặng2 nhưng cần nói chuyện để phát hiện J21 | J16 ở khu khởi đầu; chỉ bấm nói chuyện mới ghi cờ tìm lối thư |
| J15 tạm mở chặng0 trong bản chỉ có hai node | Save mới: J15 chặng2 sau gate ba mảnh, không yêu cầu đánh Hạc Giấy. Save cũ đã ghé J15 được giữ riêng node đã biết bằng trường dữ liệu revealWhenPreviouslyVisited; không cấp stage hoặc quest chính |
| Hai nguồn J18/J21 đều có reply_letter | Giữ cả nguồn, GrantOnce tôn trọng stackLimit=1 của key/quest unique; không tạo bản sao thư thứ hai |

IDs, tên, thứ tự node, item/enemy ID, thống kê combat và HQ05/HQ06/HQ07 được giữ.
Tọa độ bốn anchor mở đầu đã kiểm thử U0/U1/J0/J15 giữ nguyên. Những anchor khác
trong nguồn có coordinates_are_approximate được căn lại chân trên ảnh A, ghi đầy đủ
cũ/mới trong [route review](../Maps/HueJeonjuChapterRoutes.json). Hai corridor Huế mới
là bổ sung thiết kế rõ ràng, không giả là cạnh có sẵn trong JSON canonical.

## Nội dung tương tác đề xuất

Nguồn quy định cơ chế nhưng chưa có đáp án máy đọc, giá shop hoặc định lượng rương.
Các phần sau do lượt triển khai này author và cần nghiệm thu:

- U16 chọn Sen–Đèn, U17 Ngắn–Dài–Ngắn, U8 Vàng–Tím–Xanh; có gợi ý ngay trong UI.
- J3 Tím–Vàng–Xanh; J7 xoay 90° hai lần rồi gấp; J12 ghép Hoa–Trăng–Đèn;
  J17 xoay ba vòng về Hoa–Trăng–Đèn. Mục tiêu, reward và liên kết Hội An giữ theo nguồn.
- Park ghi mã kho qua lựa chọn hỏi tiến độ, dùng nó tại J6. Nhánh U5 trình ấn và mã
  thư khố giữ nguyên ấn; không trao chìa/ấn bằng cách chỉ tới node.
- Rương U13: một trà + ghi chép; J10: tám xu + một trà, sáp là chi tiết lời kể vì
  nguồn chưa có itemId sáp. Không tạo ID trùng công dụng hoặc giả có item sáp runtime.
- Shop J15 bán một healing_tea giá tám xu. Đây là giá đề xuất; không biến mọi đặc sản
  thành potion. Bibimbap/trà sen/mè xửng chưa có itemId riêng trong catalog 37 item,
  chưa được tạo như vật phẩm mới trong lượt này.
- Check-in U4/U6/J8/J11, ba xu một lần, dùng đúng vùng minh họa Overview A.
- Lời thoại nối và phản hồi tiến độ mới lưu trong JSON, không rải trong class theo node.
- Hai native cutout Lê An/Hana theo vai trò roster và 13 dẫn xuất AI tĩnh là hình
  đề xuất chờ duyệt danh tính/visual; không tuyên bố canonical hoặc animation final.

## Hành trình sáu thành phố

Hội An mở Huế/Jeonju song song. Huế hoàn thành mới đủ điều kiện Đà Lạt; Jeonju hoàn
thành mới đủ điều kiện Gyeongju. Seoul vẫn cần cả bốn cờ Huế/Đà Lạt/Jeonju/Gyeongju.
Ở trạng thái source trước lượt này, ba chương Đà Lạt/Gyeongju/Seoul **chưa có runtime**,
không có thành quả triển khai cần ghi đè. Giữ toàn bộ 67 node/dữ liệu của chúng và
Enter bị vô hiệu hóa rõ ràng thay vì dẫn người chơi đến map không có gameplay.

Điểm dừng đầu tiên của nhánh Việt Nam: U20 → World Map Đà Lạt, chưa vào được D0.
Nhánh Hàn Quốc: J20 → World Map Gyeongju, chưa vào được G0. Vì thế chưa thể kiếm
blue_water_signal ở D16 hay stargazer_lens/silla_prism ở G4/G18 để kết thúc Seoul.
Không dùng fixture bật cờ tương lai để tuyên bố toàn game end-to-end PASS.

Không có node Free Exploration trong hai JSON U/J; không thêm scene exploration
giả hoặc thay H3. Bộ H3_Upgrade_20261016 mới xuất hiện trong SourceSheets được giữ
nguyên làm nguồn, chưa tích hợp vào cảnh H3 trong lượt hai chương này.
