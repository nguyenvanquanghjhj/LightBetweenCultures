# NHIỆM VỤ – TOÀN BỘ ĐƯỜNG ĐI / PHẦN THƯỞNG

Nguyên tắc: chỉ trao vật phẩm khi người chơi đã đủ điều kiện, xác thực nhận 1 lần qua một UniqueGrantId. Một yêu cầu `defeat_or_fix` cho phép đường ngoại giao hoặc đường đánh để khỏi chặn người chơi.

| ID | Thành phố | Bắt đầu | Các bước theo thứ tự | Cờ hoàn thành | Thưởng |
|---|---|---|---|---|---|
| HQ01 Đèn phố tối | hoi_an | H2 | `find:lamp_eye@H3` → `find:hanji_core@H13` → `find:link_key@H5` → `activate:H7` | `hoi_an.lamp_restored` | 150 xu, mở Huế & Jeonju |
| HQ02 Hàng của Bà Tư | hoi_an | H10 | `resolve:enemy_crab@H12` → `deliver:copper_wire@H10` | `hoi_an.ba_tu_helped` | bản đồ ngõ ẩn H9 |
| HQ03 Con đèn lạc | hoi_an | H18 | `find:reply_letter@J21` → `interact:H9` | `hoi_an.lost_lantern_rescued` | bạn đồng hành Đèn Lồng Nhỏ |
| HQ04 Đò qua nhánh nước | hoi_an | H17 | `collect:coin*5` → `talk:H17` | `hoi_an.ferry_available` | đường tắt H17–H5 |
| UQ01 Dấu ấn hai bờ | hue | U7 | `get:lost_letter@U15` → `solve:U17` → `solve:U8` → `activate:U19` | `hue.beacon_lit` | mở Đà Lạt |
| UQ02 Thảo dược Bà Nguyệt | hue | U2 | `find:royal_herb@U10` → `deliver:royal_herb@U2` | `hue.nguyet_helped` | trà hồi phục x3 và thẻ bờ bắc |
| UQ03 Qua sông an toàn | hue | U3 | `solve:U16` → `use:river_pass@U3` | `hue.crossing_permit` | mở cầu U4–U5 |
| UQ04 Tấm ấn gửi đi xa | hue | U11 | `find:ancient_seal@U11` → `talk:U7` | `hue.star_script_known` | Gyeongju G12 và Seoul S17 đổi trạng thái |
| DQ01 Kính giữa sương | da_lat | D2 | `defeat:enemy_glass_thief@D7` → `return:lan_glasses@D2` → `solve:D8` | `da_lat.fog_cleared` | mở đường sương D8 và đèn núi |
| DQ02 Bạn chó canh đường | da_lat | D10 | `collect:meat_portion*3@D9,D11,D12` → `give:meat_portion*3@D10` | `da_lat.dog_fed` | mở D10–D15, không đánh thú |
| DQ03 Tín hiệu thác nước | da_lat | D16 | `defeat:enemy_fog_boss@D15` → `find:blue_water_signal@D16` → `activate:D14` | `da_lat.beacon_lit` | mã rơle Seoul A |
| DQ04 Hoa trong mây | da_lat | D13 | `find:night_flower@D19` → `deliver:night_flower@D13` | `da_lat.night_flower_done` | hồi phục và vật phẩm hỗ trợ |
| JQ01 Hoa văn ba miền | jeonju | J2 | `get:hanji_pattern_1@J7` → `get:hanji_pattern_2@J6` → `get:hanji_pattern_3@J11` → `solve:J12` | `jeonju.three_patterns_joined` | mạch đèn hanji |
| JQ02 Kiện hàng thất lạc | jeonju | J5 | `resolve:enemy_parcel_bot@J6` → `return:missing_parcel@J5` | `jeonju.parcel_returned` | mở đường lên đài J11 |
| JQ03 Đèn soi qua hai nước | jeonju | J17 | `have_flag:hoi_an.lamp_restored` → `solve:J17` → `activate:J19` | `jeonju.beacon_lit` | mở Gyeongju, gửi rơle C Seoul |
| JQ04 Thư gửi phố cũ | jeonju | J16 | `find:reply_letter@J21` → `carry:reply_letter@H18` | `jeonju.letter_delivered` | Hội An mở ngõ H9 |
| GQ01 Đường sao Silla | gyeongju | G3 | `get:star_symbol_1@G5` → `get:star_symbol_2@G6` → `get:star_symbol_3@G10` → `solve:G4` | `gyeongju.three_stars_placed` | ống kính sao |
| GQ02 Bệ đá nối hai nước | gyeongju | G12 | `have:ancient_seal` → `solve:G12` → `defeat_or_fix:enemy_stone_guard@G7` | `gyeongju.seal_repaired` | phần thưởng giúp khám phá |
| GQ03 Đêm đài thiên văn | gyeongju | G18 | `have_flag:gyeongju.three_stars_placed` → `defeat:enemy_star_core@G18` → `activate:G19` | `gyeongju.beacon_lit` | mở điều kiện đi Seoul |
| GQ04 Hoa văn bờ tây | gyeongju | G17 | `have_flag:jeonju.beacon_lit` → `solve:G17` | `gyeongju.west_lantern_done` | xu & bí mật |
| SQ01 Ba nhịp đồng bộ | seoul | S7 | `have:blue_water_signal` → `solve:S16` → `have:ancient_seal` → `solve:S17` → `have:hanji_circuit` → `solve:S18` | `seoul.relays_ready` | mở boss S20 |
| SQ02 Lời nhắn cũ | seoul | S2 | `get:stargazer_lens@G4` → `interact:S3` → `talk:S11` | `seoul.truth_discovered` | lựa chọn phục hồi hệ thống |
| SQ03 Lõi sáng cuối | seoul | S20 | `have_flag:seoul.relays_ready` → `boss:enemy_final_core@S20` → `visit:S21` | `seoul.final_cleared` | festival và credits |
| SQ04 Tín hiệu bóng Mờ | seoul | S22 | `have_flag:seoul.truth_discovered` → `read:S22` | `seoul.secret_ending` | đoạn kết mở rộng |

## Quest và logic phải sửa/định nghĩa thêm khi implement

- Mọi điều kiện trong bảng dùng ngữ nghĩa thiết kế, CHƯA là cú pháp parser Unity. Codex tạo ScriptableObject/JSON DTO typed với `HasItem`, `HasFlag`, `CompleteEvent`, `WinBattleOrHack`.
- Với item đa nguồn `meat_portion`, quest đếm số thực trong inventory, không thể cộng lại khi replay puzzle; lựa chọn bonus ở D3 nếu player tiêu dùng nhầm.
- Link_Hue và Link_Jeonju mở đồng thời sau H7; không tạo vòng khóa cần vật phẩm Gyeongju để tới Gyeongju.
- Trước khi cấp item tại H5, U5 hoặc J6, phải kiểm tra kết quả win/hack; không trao item chỉ vì người chơi chạm node.
- `hoi_an.mo01_defeated` và `hoi_an.mo01_hack_code` là phương án OR, không được yêu cầu đồng thời. Chỉ cấp link_key khi chiến thắng H5 hoặc nhập mã H19 vào H5.
- `da_lat.glasses_returned` chỉ bật sau khi giao kính cho D2, KHÔNG bật khi chat lần đầu; HQ02 bật `hoi_an.secret_lane_found` sau khi trả dây ở H10; `jeonju.hidden_letter_found` bật khi J16 kể bí mật, không cần đợi J21.
- `seoul.relays_ready` được DERIVE từ ba cờ `seoul.relay_a_on`, `seoul.relay_b_on`, `seoul.relay_c_on`, tránh trạng thái lệch giữa derived và saved.
- `game.ending_seen` chỉ đặt sau cutscene ending, còn `seoul.final_cleared` đặt sau boss hoàn thành.
- Giữ save bản cũ: nếu đã H3 nhặt `lamp_eye`, map vào cờ mới, không xóa file save.
