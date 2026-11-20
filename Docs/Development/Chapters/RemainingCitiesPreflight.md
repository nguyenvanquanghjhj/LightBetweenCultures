# Audit trước mở rộng Đà Lạt → Gyeongju → Seoul, 17/10/2026

Checkpoint HueJeonjuGameplay khớp source/assets/settings/packages/tools trước sửa. Backup BeforeDaLatChapter-20261017. Không có process Unity/player đang chạy. Chưa thay runtime Gyeongju/Seoul trước khi nghiệm thu Đà Lạt.

## Đà Lạt

| Node | Tên | Vai trò | NPC/enemy | Quest | Item | Gate | Edge/waypoint | Marker/actor | Trạng thái |
|---|---|---|---|---|---|---|---|---|---|
| D0 | Bến hồ Xuân Hương | start | — |  |  |  | D0–D12:0wp, D0–D20:3wp, D0–D5:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D1 | Quảng trường dốc | junction | — | DQ02,DQ03,DQ04 |  |  | D2–D1:0wp, D1–D4:0wp, D1–D20:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D2 | Người dẫn đường Lan | npc | npc_lan | DQ01 |  | quest:DQ01 | D3–D2:0wp, D2–D1:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D3 | Tiệm hoa Thủy | npc | npc_thuy |  |  |  | D18–D3:0wp, D3–D2:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D4 | Chợ trên đồi | npc | npc_market |  |  |  | D1–D4:0wp, D4–D14:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D5 | Lối rừng thông | junction | — |  |  |  | D0–D5:0wp, D5–D18:0wp, D5–D6:0wp, D5–D11:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D6 | Sói sương mù | enemy | enemy_mist_wolf |  |  |  | D5–D6:0wp, D6–D7:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D7 | Trạm quan sát sương | enemy | enemy_glass_thief | DQ01 | lan_glasses |  | D6–D7:0wp, D7–D8:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D8 | Ngõ sương vòng | puzzle | — | DQ01 |  | da_lat.fog_cleared | D7–D8:0wp, D8–D9:0wp, D8–D19:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D9 | Đài nhìn thông | item | — | DQ02 | meat_portion,fog_battery |  | D8–D9:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D10 | Chó canh đói | npc | npc_hungry_dog | DQ02 |  | da_lat.dog_fed | D11–D10:0wp, D12–D10:0wp, D10–D15:2wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D11 | Lối vườn hoa | item | — | DQ02 | meat_portion |  | D5–D11:0wp, D11–D10:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D12 | Cối xay cạnh hồ | puzzle | — | DQ02 | meat_portion |  | D0–D12:0wp, D12–D10:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D13 | Vườn kính hoa | npc | npc_gardener | DQ04 |  |  | D20–D13:0wp, D13–D14:0wp, D13–D21:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D14 | Đài dẫn sáng núi | beacon | — | DQ03 |  | da_lat.beacon_lit | D13–D14:0wp, D4–D14:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D15 | Bóng sương cầu đá | boss | enemy_fog_boss | DQ03 |  |  | D10–D15:2wp, D15–D16:0wp, D20–D15:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D16 | Thác hồi âm | item | — | DQ03 | blue_water_signal |  | D15–D16:0wp, D16–D17:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D17 | Lối bí mật bờ đông | secret | — |  |  |  | D16–D17:0wp, D21–D17:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D18 | Nhà nghỉ trên dốc | npc | npc_innkeeper |  |  |  | D5–D18:0wp, D18–D3:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D19 | Vườn hoa đêm | secret | — | DQ04 | night_flower |  | D8–D19:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D20 | Cầu dẫn hồ | junction | — |  |  |  | D0–D20:3wp, D1–D20:0wp, D20–D13:0wp, D20–D15:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| D21 | Cửa đồng bộ Seoul | portal | — |  |  |  | D13–D21:0wp, D21–D17:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |

## Gyeongju

| Node | Tên | Vai trò | NPC/enemy | Quest | Item | Gate | Edge/waypoint | Marker/actor | Trạng thái |
|---|---|---|---|---|---|---|---|---|---|
| G0 | Cổng vào làng di tích | start | — |  |  |  | G0–G21:0wp, G0–G1:2wp, G0–G16:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G1 | Cầu lối bắc | junction | — | GQ01,GQ02,GQ03,GQ04 |  |  | G0–G1:2wp, G1–G11:0wp, G1–G2:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G2 | Đình ao sen | puzzle | — |  |  |  | G1–G2:0wp, G2–G17:0wp, G2–G11:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G3 | Nhà khảo cổ Min | npc | npc_min | GQ01 |  | quest:GQ01 | G6–G3:0wp, G3–G4:0wp, G3–G11:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G4 | Đài Cheomseongdae cách điệu | puzzle | — | GQ01 | stargazer_lens | gyeongju.three_stars_placed | G3–G4:0wp, G4–G19:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G5 | Gò mộ phía tây | item | — | GQ01 | star_symbol_1 |  | G17–G5:0wp, G5–G6:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G6 | Đường giữa các gò | enemy | enemy_fox | GQ01 | star_symbol_2 |  | G5–G6:0wp, G6–G3:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G7 | Tượng đá gác lối | enemy | enemy_stone_guard | GQ02 |  | gyeongju.stone_guard_friend | G8–G7:0wp, G7–G11:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G8 | Lối chùa đá | junction | — |  |  |  | G14–G8:0wp, G8–G7:0wp, G8–G13:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G9 | Sân chính di tích | npc | npc_keeper |  |  |  | G19–G9:0wp, G9–G15:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G10 | Đình trên đồi | item | — | GQ01 | star_symbol_3 |  | G15–G10:0wp, G10–G18:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G11 | Vườn đá trung tâm | junction | — |  |  |  | G1–G11:0wp, G3–G11:0wp, G7–G11:0wp, G11–G12:0wp, G2–G11:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G12 | Bệ phong ấn nứt | puzzle | — | GQ02 |  |  | G11–G12:0wp, G12–G13:0wp, G16–G12:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G13 | Lính canh đêm | enemy | enemy_night_guard |  |  |  | G12–G13:0wp, G13–G20:0wp, G8–G13:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G14 | Kho bản khắc | npc | npc_inscription |  |  |  | G18–G14:0wp, G14–G8:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G15 | Thềm điện đá | puzzle | — |  |  |  | G9–G15:0wp, G15–G10:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G16 | Đền dưới hồ | item | — |  |  |  | G0–G16:0wp, G16–G12:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G17 | Đài lồng đèn bờ tây | puzzle | — | GQ04 |  |  | G2–G17:0wp, G17–G5:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G18 | Lõi canh tinh tú | boss | enemy_star_core | GQ03 | silla_prism |  | G10–G18:0wp, G18–G14:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G19 | Trạm cộng hưởng Silla | beacon | — | GQ03 |  | gyeongju.beacon_lit | G4–G19:0wp, G19–G9:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G20 | Cổng đi Seoul | portal | — |  |  |  | G13–G20:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| G21 | Lối cỏ dược liệu | secret | — |  |  |  | G0–G21:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |

## Seoul

| Node | Tên | Vai trò | NPC/enemy | Quest | Item | Gate | Edge/waypoint | Marker/actor | Trạng thái |
|---|---|---|---|---|---|---|---|---|---|
| S0 | Bến công viên sông Hàn | start | — |  |  |  | S0–S12:0wp, S0–S14:2wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S1 | Quảng trường trung tâm | junction | — | SQ01,SQ02 |  |  | S18–S1:0wp, S1–S5:0wp, S1–S6:0wp, S13–S1:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S2 | Cổng thành xưa | npc | npc_curator | SQ02,SQ03,SQ04 |  |  | S4–S2:0wp, S2–S3:0wp, S2–S21:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S3 | Sân cung điện | puzzle | — | SQ02 |  |  | S2–S3:0wp, S3–S17:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S4 | Hồ sơ lịch sử | item | — |  |  |  | S15–S4:0wp, S4–S2:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S5 | Drone trinh sát | enemy | enemy_drone |  |  |  | S17–S5:0wp, S5–S8:0wp, S1–S5:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S6 | Bờ đông sông Hàn | junction | — |  |  |  | S1–S6:0wp, S6–S16:0wp, S6–S20:0wp, S6–S14:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S7 | Quảng trường màn hình | npc | npc_tech | SQ01 |  | quest:SQ01 | S11–S7:0wp, S7–S18:0wp, S7–S20:0wp, S7–S8:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S8 | Đường cao ốc | enemy | enemy_signal_bot |  |  |  | S5–S8:0wp, S8–S9:0wp, S7–S8:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S9 | Đường lên N Seoul Tower | junction | — |  |  |  | S8–S9:0wp, S9–S10:0wp, S9–S11:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S10 | Chân tháp N Seoul | puzzle | — |  |  |  | S9–S10:0wp, S10–S22:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S11 | Trạm kiểm soát kỹ thuật | npc | npc_engineer | SQ02 |  |  | S9–S11:0wp, S11–S7:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S12 | Đảo công viên xanh | item | — |  |  |  | S0–S12:0wp, S12–S13:2wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S13 | Cầu ánh sáng phía tây | junction | — |  |  |  | S12–S13:2wp, S13–S15:0wp, S13–S1:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S14 | Bến phà thông tin | npc | npc_boat |  |  |  | S0–S14:2wp, S6–S14:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S15 | Bot bảo vệ cầu | enemy | enemy_bridge |  |  |  | S13–S15:0wp, S15–S4:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S16 | Trạm phát điện bờ đông | puzzle | — | SQ01 |  | seoul.relay_a_on | S6–S16:0wp, S16–S19:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S17 | Rơle cung điện | puzzle | — | SQ01 |  |  | S3–S17:0wp, S17–S5:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S18 | Rơle ánh đèn hanji | puzzle | — | SQ01 |  | seoul.relays_ready | S7–S18:0wp, S18–S1:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S19 | Đường hầm dữ liệu | enemy | enemy_shadow_courier |  |  |  | S16–S19:0wp, S19–S20:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S20 | Buồng Lõi Nhiễu | boss | enemy_final_core | SQ03 |  | seoul.final_cleared | S19–S20:0wp, S7–S20:0wp, S6–S20:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S21 | Lễ hội hai nền văn hóa | epilogue | — | SQ03 |  |  | S2–S21:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
| S22 | Kính quan sát đồi | secret | — | SQ04 |  |  | S10–S22:0wp | Chung; chưa có binding riêng | DATA ONLY / NOT VERIFIED |
