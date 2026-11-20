# Đà Lạt D0–D21 — node coverage

Runtime Windows: xem final-run-manifest và báo cáo root. Terrain/art chưa final; Gyeongju/Seoul chưa triển khai.

| Node | Tên / vai trò | Event thật | NPC/enemy | Quest | Item / flag | Khu / gate | Giới hạn |
|---|---|---|---|---|---|---|---|
| D0 | Bến hồ Xuân Hương / start | intro(start) | Lumi / môi trường |  | ;  | 0;  | Đã ghé bằng input; terrain còn review |
| D1 | Quảng trường dốc / junction | square(junction) | Lumi / môi trường |  | ;  | 1;  | Đã ghé bằng input; terrain còn review |
| D2 | Người dẫn đường Lan / npc | npc_lan(npc), dalat_return_glasses(item) | npc_lan, npc_lan | DQ01 | ; da_lat.glasses_returned | 0; da_lat.glasses_returned | Đã ghé bằng input; terrain còn review |
| D3 | Tiệm hoa Thủy / npc | npc_thuy(npc), dalat_thuy_food(item), dalat_dog_food_recovery(recovery) | npc_thuy, npc_thuy, npc_thuy |  | meat_portion×1; da_lat.thuy_food_given | 0;  | Đã ghé bằng input; terrain còn review |
| D4 | Chợ trên đồi / npc | npc_market(npc), dalat_strawberry_offer(quest) | npc_market_merchant, npc_market_merchant | DQ05 | ;  | 1;  | Đã ghé bằng input; terrain còn review |
| D5 | Lối rừng thông / junction | pine_path(junction) | Lumi / môi trường |  | ;  | 0;  | Đã ghé bằng input; terrain còn review |
| D6 | Sói sương mù / enemy | enemy_mist_wolf(encounter) | enemy_mist_wolf |  | ;  | 0;  | Combat riêng D6 chưa test |
| D7 | Trạm quan sát sương / enemy | enemy_glass_thief(encounter) | enemy_glass_thief |  | lan_glasses×1;  | 0;  | Đã ghé bằng input; terrain còn review |
| D8 | Ngõ sương vòng / puzzle | puzzle_mist(puzzle), dalat_dq01_finish(quest) | Lumi / môi trường, Lumi / môi trường | DQ01 | ;  | 1; da_lat.fog_cleared | Đã ghé bằng input; terrain còn review |
| D9 | Đài nhìn thông / item | loot_mountain(chest) | Lumi / môi trường |  | meat_portion×1, fog_battery×1; da_lat.mountain_supplies_found | 2;  | Đã ghé bằng input; terrain còn review |
| D10 | Chó canh đói / npc | npc_hungry_dog(npc) | npc_hungry_dog | DQ02 | ;  | 2; da_lat.dog_fed | Đã ghé bằng input; terrain còn review |
| D11 | Lối vườn hoa / item | loot_meat_garden(chest) | Lumi / môi trường |  | meat_portion×1; da_lat.west_path_open | 2;  | Đã ghé bằng input; terrain còn review |
| D12 | Cối xay cạnh hồ / puzzle | puzzle_windmill(puzzle) | Lumi / môi trường |  | meat_portion×1;  | 2;  | Đã ghé bằng input; terrain còn review |
| D13 | Vườn kính hoa / npc | npc_gardener(npc) | npc_gardener | DQ04 | ;  | 2;  | Đã ghé bằng input; terrain còn review |
| D14 | Đài dẫn sáng núi / beacon | restore_dalat(beacon) | Lumi / môi trường | DQ03 | ;  | 3; da_lat.beacon_lit | Đã ghé bằng input; terrain còn review |
| D15 | Bóng sương cầu đá / boss | enemy_fog_boss(encounter) | enemy_fog_boss |  | ;  | 3;  | Đã ghé bằng input; terrain còn review |
| D16 | Thác hồi âm / item | loot_waterfall(chest), dalat_dq03_offer(quest) | Lumi / môi trường, Lumi / môi trường | DQ03 | blue_water_signal×1; da_lat.water_signal_found | 3;  | Đã ghé bằng input; terrain còn review |
| D17 | Lối bí mật bờ đông / secret | secret_lookout(secret) | Lumi / môi trường |  | healing_tea×1;  | 4;  | Đã ghé bằng input; terrain còn review |
| D18 | Nhà nghỉ trên dốc / npc | npc_innkeeper(start) | npc_innkeeper |  | ;  | 0;  | Đã ghé bằng input; terrain còn review |
| D19 | Vườn hoa đêm / secret | secret_night_bloom(secret) | Lumi / môi trường |  | night_flower×1;  | 2;  | Đã ghé bằng input; terrain còn review |
| D20 | Cầu dẫn hồ / junction | lake_bridge(junction) | Lumi / môi trường |  | ;  | 2;  | Đã ghé bằng input; terrain còn review |
| D21 | Cửa đồng bộ Seoul / portal | portal_seoul(portal) | Lumi / môi trường |  | ;  | 4;  | Đã ghé bằng input; terrain còn review |

## Quest

| ID | Nhận → kết thúc | Yêu cầu | Thưởng / cờ |
|---|---|---|---|
| DQ01 | D2 → D8 | da_lat.glasses_returned, da_lat.fog_path_solved;  | 0 xu; da_lat.fog_cleared |
| DQ02 | D10 → D10 | ; meat_portion×3 | 0 xu; da_lat.dog_fed |
| DQ03 | D16 → D14 | da_lat.fog_cleared, da_lat.dog_fed, da_lat.mist_boss_cleared; blue_water_signal×1 | 0 xu; da_lat.beacon_lit |
| DQ04 | D13 → D13 | ; night_flower×1 | 0 xu; da_lat.night_flower_done |
| DQ05 | D4 → J15 | ; da_lat_strawberry×1 | 10 xu; jeonju.dalat_strawberry_shared |
