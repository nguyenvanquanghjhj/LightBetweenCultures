# LEVEL DESIGN — HỘI AN (bản playable slice)

## 1. Cốt lõi trải nghiệm

Người chơi **khám phá theo các điểm** trên bản đồ thành phố như Dynamons World, nhưng có **một số node mở cảnh tự do**. Các tương tác (NPC, bot, puzzle, vật phẩm) **nằm rải rác trong hành trình**, không chia một khu riêng chỉ dành cho giải đố.

Sơ đồ chính xác: `../Maps/HoiAn_Node_Flow.png` và `../Data/hoi_an_map.json`. Bố trí node là thiết kế gameplay, không tái hiện quy hoạch Hội An ngoài đời.

## 2. Mười địa điểm

| ID | Địa điểm | Sự kiện khi tới | Giai đoạn |
|---|---|---|---|
| H0 | Bến thuyền | Tutorial → bắt đầu từ đây | A |
| H1 | Phố đèn | Ngã ba và đường mở | A |
| H2 | Tiệm đèn | Gặp cô Mai, nhận / trả nhiệm vụ | A |
| H3 | Hẻm mái ngói | Vào scene tự do tìm linh kiện | A |
| H4 | Chợ ven sông | Ghép hoa văn / vật phẩm | B |
| H5 | Bến gỗ | Bot Mờ-01 giữ chìa khóa | C |
| H6 | Cầu cổ | Lối hội tụ, đèn đổi sáng khi hoàn thành | B |
| H7 | Trạm ánh sáng | Giao vật phẩm, khôi phục trạm | B/C |
| H8 | Cổng Jeonju | Mở thành phố Hàn Quốc | C/D |
| H9 | Ngõ bí mật | Sidequest / rương, hidden until clue | D |

A = prototype cần làm đầu, B/C/D tăng phạm vi sau kiểm thử. **Đừng code H5 boss và H9 cùng lúc với A.**

## 3. Quy tắc node (bắt buộc)

- WorldMap có 6 thành phố nhưng chỉ Hội An unlocked ban đầu.
- CityMap là **đồ thị có cạnh**, không phải các nút bấm tự do gọi sự kiện. Người chơi chọn đích có thể tới được, hệ thống chạy BFS tìm tuyến ngắn nhất trên những cạnh đủ điều kiện.
- Lumi đi dọc đường nối với chuyển động liên tục, không teleport node.
- Đi qua một node trung gian chỉ kích hoạt những hiệu ứng pass-through thích hợp; **không tự động bật scene H3** khi H3 là node trung gian nếu target đang là H5.
- Khi đến node target thì mới mở event, có debounce ngăn click liên tục.
- H8 cần `hoi_an.light_restored`; H9 ẩn cho đến khi `hoi_an.secret_lane_discovered`.
- Node có 2 biểu diễn: marker có thể click và event thuộc game logic; không gộp chặt với ảnh nền.

## 4. Gameplay slice 3–5 phút

1. Lumi ở H0. Một tooltip ngắn: “Chạm vào điểm được nối để di chuyển”.
2. Đi H1 rồi đến H2, cô Mai giao việc tìm **mắt đèn** ở H3.
3. Theo đường tới H3. Mở cảnh free exploration Hẻm mái ngói: Lumi phải đi **quanh** nhà/thùng/nước, không xuyên qua.
4. Đến gần mắt đèn ở góc phía trên, tương tác để nhặt. Kho đồ +1 và vật phẩm biến mất vĩnh viễn trong save.
5. Rời hẻm tại E, CityMap mở đúng vị trí H3.
6. Quay về H2, cô Mai đổi hội thoại; giao mắt đèn. Một đèn trong tiệm bừng sáng, đánh dấu slice hoàn thành.
7. Nhấn mở lại game: ánh sáng nhỏ và tiến độ quest vẫn ở trạng thái đúng.

**A không cần giả mạo rằng đã giải phóng cổng Jeonju**, vì boss và hai linh kiện còn chưa làm.

## 5. Giai đoạn sau

- Ở H4, làm puzzle ghép các mảnh hoa văn theo manh mối NPC thay vì câu đố chỉ có chữ.
- Ở H5, đánh bot Mờ-01 theo lượt, nhiều nhất 2–3 lệnh cơ bản, không skill tree.
- Khi đủ mắt đèn + lõi đèn + chìa khóa, H7 kích hoạt ánh sáng lớn toàn phố, H8 mở. Cờ trạng thái được lưu và có tác động đến Jeonju.
- Sidequest H9 chỉ xuất hiện sau khi nhận gợi ý qua hội thoại hoặc vật phẩm; có một lối tắt H9→H5.

## 6. Scene tự do H3

File `../Data/hoi_an_exploration_grid.json` chứa bản thiết kế 20 cột × 14 hàng.

Chú giải: `#` viền tường chặn, `H` nền nhà chặn, `C` thùng chặn, `W` nước chặn, `.` nền đi được, `P` spawn, `I` mắt đèn, `N` NPC/prop, `E` exit.

- Chỉ cần **lối đi hợp lệ**, một vật phẩm và exit. Không bắt đầu bằng một level lớn.
- Camera theo gà, đủ nhìn theo màn hình dọc, không nhảy giật ở góc cảnh.
- Tường/nước có collider; phải pathfind quanh chúng chứ không chỉ test điểm đích.
- Kiểm tra không có spawn trong collider; đường từ P tới I và E đã xác nhận được bằng BFS 4 hướng của bản thiết kế.
- Hình thức: sân ngõ cổ với tường vàng, mái ngói đỏ, đèn lồng, một đoạn kênh nhỏ, thùng gỗ và một sân lát đá. Vật cản placement có thể thay đổi nhẹ nếu vẫn giữ đường đi.
