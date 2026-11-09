# Jeonju — toàn bộ 22 node

Gameplay và lượt ghé thăm được xác nhận bằng Windows player, không phải chỉ parse JSON. Các cutout mới vẫn chờ duyệt art; địa hình còn giới hạn ghi trong báo cáo chung.

| Node | Địa điểm | Chức năng thực tế | Quest / enemy / puzzle | Chặng hiện | Kết quả |
|---|---|---|---|---|---|
| J0 | Cổng làng phía nam | start | định hướng / tương tác theo event | 0 | PASS |
| J1 | Ngã ba hanok | junction | định hướng / tương tác theo event | 0 | PASS |
| J2 | Nghệ nhân Hana | npc | JQ01 | 0 | PASS |
| J3 | Xưởng nhuộm vải | puzzle | puzzle_dye | 1 | PASS |
| J4 | Nhà thầy Yoon | npc | định hướng / tương tác theo event | 1 | PASS |
| J5 | Tiệm hàng chuyển | npc | JQ02 | 1 | PASS |
| J6 | Bot Cắp Kiện | encounter | enemy_parcel_bot | 1 | PASS |
| J7 | Thư viện giấy hanji | puzzle | puzzle_hanji | 1 | PASS |
| J8 | Cầu đá tây | junction / check-in | định hướng / tương tác theo event | 1 | PASS |
| J9 | Bờ suối nước chảy | encounter | enemy_paper_crane | 1 | PASS |
| J10 | Rương ven hồ | chest | định hướng / tương tác theo event | 1 | PASS |
| J11 | Đài ngắm mái ngói | chest / check-in | định hướng / tương tác theo event | 1 | PASS |
| J12 | Sân thêu họa tiết | puzzle | JQ01, puzzle_embroidery | 1 | PASS |
| J13 | Cổng sân cổ | junction | định hướng / tương tác theo event | 1 | PASS |
| J14 | Bot Hoa Văn | encounter | enemy_pattern_bot | 2 | PASS |
| J15 | Chợ hanok | npc | HQ05 | 2 | PASS |
| J16 | Người giữ chuyện | dialogue | JQ04 | 0 | PASS |
| J17 | Đèn song thành | puzzle | JQ03, puzzle_lantern | 2 | PASS |
| J18 | Hạc giấy bóng tối | encounter | enemy_crane_boss | 2 | PASS |
| J19 | Trạm ánh sáng hanji | beacon | JQ03 | 3 | PASS |
| J20 | Cổng đi Gyeongju | portal | định hướng / tương tác theo event | 4 | PASS |
| J21 | Lối thư hồi âm | secret | định hướng / tương tác theo event | 1 | PASS |

## Quest

| ID | Nhận | Hoàn thành | Điều kiện | Thưởng |
|---|---|---|---|---|
| JQ01 | J2 | jeonju / J12 | hanji_pattern_1 × 1; hanji_pattern_2 × 1; hanji_pattern_3 × 1; jeonju.three_patterns_joined | jeonju.three_patterns_joined |
| JQ02 | J5 | jeonju / J5 | missing_parcel × 1 | jeonju.parcel_returned |
| JQ03 | J17 | jeonju / J19 | hanji_circuit × 1; jeonju.lantern_puzzle_solved; jeonju.three_patterns_joined | jeonju.beacon_lit |
| JQ04 | J16 | hoi_an / H18 | reply_letter × 1 | jeonju.letter_delivered |

## Key gates

| Node | Điều kiện | Mở chặng |
|---|---|---|
| J2 | quest:JQ01 | 1 |
| J12 | jeonju.three_patterns_joined | 2 |
| J17 | jeonju.lantern_puzzle_solved | 3 |
| J19 | jeonju.beacon_lit | 4 |

## Check-in

- J8: Cầu đá tây, 3 xu một lần; minh họa vùng Overview A.
- J11: Đài ngắm mái ngói, 3 xu một lần; minh họa vùng Overview A.

## NPC / enemy / puzzle

- J2: npc_hana — Nghệ nhân Hana.
- J4: npc_yoon — Nhà thầy Yoon.
- J5: npc_park — Thương nhân Park.
- J15: npc_soyeon — So-yeon.
- J16: npc_elder — Người giữ chuyện.
- enemy_paper_crane: Hạc Giấy Nhiễu, HP 20, ATK 6, DEF 2; giữ thống kê nguồn/balance proposal.
- enemy_parcel_bot: Bot Cắp Kiện, HP 27, ATK 7, DEF 3; giữ thống kê nguồn/balance proposal.
- enemy_pattern_bot: Bot Hoa Văn, HP 30, ATK 7, DEF 4; giữ thống kê nguồn/balance proposal.
- enemy_crane_boss: Hạc Giấy Bóng Tối, HP 39, ATK 9, DEF 4; giữ thống kê nguồn/balance proposal.
- J3: puzzle_dye (sequence), reward [{"itemId": "healing_tea", "amount": 1}]; flags jeonju.dye_restored.
- J7: puzzle_hanji (fold), reward [{"itemId": "hanji_pattern_1", "amount": 1}]; flags .
- J12: puzzle_embroidery (sequence), reward [{"itemId": "hanji_circuit", "amount": 1}]; flags jeonju.three_patterns_joined.
- J17: puzzle_lantern (rings), reward []; flags jeonju.lantern_puzzle_solved.
