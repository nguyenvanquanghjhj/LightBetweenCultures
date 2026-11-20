# Huế — toàn bộ 22 node

Gameplay và lượt ghé thăm được xác nhận bằng Windows player, không phải chỉ parse JSON. Các cutout mới vẫn chờ duyệt art; địa hình còn giới hạn ghi trong báo cáo chung.

| Node | Địa điểm | Chức năng thực tế | Quest / enemy / puzzle | Chặng hiện | Kết quả |
|---|---|---|---|---|---|
| U0 | Bến sông phía phố | start | định hướng / tương tác theo event | 0 | PASS |
| U1 | Chợ xưa bờ nam | npc | HQ06 | 0 | PASS |
| U2 | Nhà thuốc Bà Nguyệt | npc | UQ02 | 0 | PASS |
| U3 | Bến đò bờ nam | npc | UQ03 | 0 | PASS |
| U4 | Đầu cầu vào thành | junction / check-in | định hướng / tương tác theo event | 1 | PASS |
| U5 | Bot Thân vệ Cổng | encounter | enemy_gate_guard | 2 | PASS |
| U6 | Cửa Ngọ Môn cách điệu | junction / check-in | định hướng / tương tác theo event | 3 | PASS |
| U7 | Nho sinh Lê An | npc | UQ01, UQ04 | 1 | PASS |
| U8 | Sân điện chính | puzzle | puzzle_court | 1 | PASS |
| U9 | Vườn phía tây | junction | định hướng / tương tác theo event | 3 | PASS |
| U10 | Vườn thuốc bờ bắc | chest | định hướng / tương tác theo event | 1 | PASS |
| U11 | Gốc cây dấu ấn | chest | UQ04, UQ04 | 1 | PASS |
| U12 | Bến sông tây | encounter | enemy_river_wisp | 1 | PASS |
| U13 | Bến thuyền hẻm | secret | định hướng / tương tác theo event | 1 | PASS |
| U14 | Cầu ngõ phía đông | dialogue | định hướng / tương tác theo event | 0 | PASS |
| U15 | Máy thu thư | encounter | enemy_courier | 0 | PASS |
| U16 | Sạp mật mã chợ | puzzle | puzzle_market_code | 0 | PASS |
| U17 | Thư khố Hoàng thành | puzzle | puzzle_archive | 1 | PASS |
| U18 | Sân thử thách Hoàng môn | encounter | enemy_archive_guard | 3 | PASS |
| U19 | Đài đèn thành nội | beacon | UQ01 | 3 | PASS |
| U20 | Cổng dẫn Đà Lạt | portal | định hướng / tương tác theo event | 4 | PASS |
| U21 | Lối đi ẩn bên tường | secret | định hướng / tương tác theo event | 4 | PASS |

## Quest

| ID | Nhận | Hoàn thành | Điều kiện | Thưởng |
|---|---|---|---|---|
| UQ01 | U7 | hue / U19 | lost_letter × 1; hue.archive_decoded; hue.court_solved; hue.guard_disabled | hue.beacon_lit |
| UQ02 | U2 | hue / U2 | royal_herb × 1 | healing_tea × 3; hue.nguyet_helped |
| UQ03 | U3 | hue / U3 | river_pass × 1 | hue.crossing_permit |
| UQ04 | U11 | hue / U7 | ancient_seal × 1 | hue.star_script_known |

## Key gates

| Node | Điều kiện | Mở chặng |
|---|---|---|
| U3 | hue.crossing_permit | 1 |
| U17 | hue.archive_decoded | 2 |
| U5 | hue.guard_disabled | 3 |
| U19 | hue.beacon_lit | 4 |

## Check-in

- U4: Đầu cầu vào thành, 3 xu một lần; minh họa vùng Overview A.
- U6: Cửa Ngọ Môn cách điệu, 3 xu một lần; minh họa vùng Overview A.

## NPC / enemy / puzzle

- U1: npc_huong — Chị Hương.
- U2: npc_nguyet — Bà Nguyệt.
- U3: npc_tam — Bác Lái Đò Tâm.
- U7: npc_scholar — Nho Sinh Lê An.
- enemy_river_wisp: Tinh Sông Nhiễu, HP 19, ATK 6, DEF 2; giữ thống kê nguồn/balance proposal.
- enemy_courier: Bot Chuyển Thư, HP 22, ATK 6, DEF 3; giữ thống kê nguồn/balance proposal.
- enemy_archive_guard: Cỗ Máy Điện Cũ, HP 27, ATK 7, DEF 4; giữ thống kê nguồn/balance proposal.
- enemy_gate_guard: Thân Vệ Hoàng Môn, HP 34, ATK 8, DEF 4; giữ thống kê nguồn/balance proposal.
- U8: puzzle_court (sequence), reward []; flags hue.court_solved.
- U16: puzzle_market_code (sequence), reward [{"itemId": "river_pass", "amount": 1}]; flags hue.market_code_known, hue.courier_code_known.
- U17: puzzle_archive (sequence), reward []; flags hue.archive_decoded, hue.secret_wall_found, hue.scholar_code_known.
