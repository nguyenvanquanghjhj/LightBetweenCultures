# Thư viện art Light Between Cultures

Ảnh được chuyển cùng .meta để giữ GUID và được Unity import, đối chiếu lại; không tạo bitmap trùng hoặc thay ảnh đã duyệt.

198 ảnh toàn project; 135 ảnh trong Art; **105 ảnh có bản ghi chuyển đường dẫn, 30 ảnh không có bản ghi, 143 alias đường dẫn**.

## Canonical và các khác biệt phải giữ rõ

- [NPC canonical identity / variants](NPC_CANONICAL_REGISTRY.md): một danh tính, nhiều biến thể. Cô Mai giữ khuôn mặt/tuổi trẻ tương đối; áo đỏ/nón/đèn là mặc định, không khóa mọi trang phục. Mai H3 và Mai áo vàng trẻ trong Composite_Illustrated là APPROVED_VARIANT (crop/concept có trạng thái riêng); bản tóc bạc lớn tuổi trong MissingSprites/Checker là IDENTITY_CONFLICT do tuổi, không do màu áo.
- Lumi dùng duy nhất sprite gà trắng Lumi_MapTraveler theo owner duyệt 21/10/2026 cho map/dialogue/exploration; art cũ giữ làm lịch sử, không còn binding runtime.
- Bot Mờ-01/crab/ghost dùng atlas đã nghiệm thu. Bot xanh/trắng trong sheet mới chưa được duyệt thay thế.
- Bà Tư, ông Phúc, Hương và So-yeon: hình hiện tại đã CANONICAL theo xác nhận 17/10/2026. Giữ native slices/portrait đã PASS; không tiếp tục chờ duyệt vì nhãn tạm cũ. Chất lượng crop, animation và biến thể mới là các tiêu chí riêng.
- Ớt/hành là icon flat tạm; bưu thiếp giữ JPG/credit và là minh họa chung. Derivative bưu thiếp lỗi ở ../_LBC_ARCHIVE/HistoricalArt/NeedsReview không được đăng ký.
- Các itemId ánh xạ qua AdventureUIArt duy nhất; cùng ID dùng cùng Sprite ở mọi thành phố. Một số item dùng icon nhóm có cờ categoryIcon, không phải art riêng đã hoàn thiện.
- Sheet UI/nhân vật/môi trường/postcard mới có nền/chữ baked. FoodCulture có alpha thật nhưng cần kiểm tra từng crop trước khi dùng; chưa đăng ký món mới chỉ để đủ danh sách.
- Atlas dùng chung vẫn giữ một bitmap và các sprite con, không nhân bản atlas vào nhiều nhóm thư mục.
- Hai ảnh Hội An cùng pixels có importer khác nhau (map chính và preview mip/1024); giữ cả hai GUID để bảo toàn chất lượng/tham chiếu. Các gói LBC/DesignKit gốc được giữ nguyên nội dung tại Tools/Import/Sources.

## Asset runtime / sprite đã đăng ký

| Tên | Vai trò / ID | Đường dẫn | Canonical | Vấn đề |
|---|---|---|---|---|
| RescueCats.png | source / compatibility / native sprite | [Assets/_Game/Art/Characters/Cats/RescueCats.png](../../Assets/_Game/Art/Characters/Cats/RescueCats.png) | Không / chưa duyệt |  |
| Lumi_MapTraveler.png | map, dialogue, exploration, lumiPortrait, lumiExploration, lumiMapTraveler, lumiCity, lumiCitySmall, Assets/_Game/Scenes\HoiAnDemo.unity, Assets/_Game/Prefabs\DesignKit\LumiExplorationSprite.prefab, Assets/_Game/Prefabs\DesignKit\LumiMapToken.prefab | [Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png](../../Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png) | Có |  |
| energy_core_Clean.png | source / compatibility / native sprite | [Assets/_Game/Art/Enemies/CleanDerived/energy_core_Clean.png](../../Assets/_Game/Art/Enemies/CleanDerived/energy_core_Clean.png) | Không / chưa duyệt |  |
| shadow_cat_Clean.png | source / compatibility / native sprite | [Assets/_Game/Art/Enemies/CleanDerived/shadow_cat_Clean.png](../../Assets/_Game/Art/Enemies/CleanDerived/shadow_cat_Clean.png) | Không / chưa duyệt |  |
| shadow_courier_Clean.png | source / compatibility / native sprite | [Assets/_Game/Art/Enemies/CleanDerived/shadow_courier_Clean.png](../../Assets/_Game/Art/Enemies/CleanDerived/shadow_courier_Clean.png) | Không / chưa duyệt |  |
| stone_guard_Clean.png | source / compatibility / native sprite | [Assets/_Game/Art/Enemies/CleanDerived/stone_guard_Clean.png](../../Assets/_Game/Art/Enemies/CleanDerived/stone_guard_Clean.png) | Không / chưa duyệt |  |
| enemy_fog_boss.png | enemy_fog_boss | [Assets/_Game/Art/Enemies/DaLat/enemy_fog_boss.png](../../Assets/_Game/Art/Enemies/DaLat/enemy_fog_boss.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_fog_boss |
| enemy_glass_thief.png | enemy_glass_thief, enemy_drone | [Assets/_Game/Art/Enemies/DaLat/enemy_glass_thief.png](../../Assets/_Game/Art/Enemies/DaLat/enemy_glass_thief.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_glass_thief, enemy_drone |
| enemy_mist_wolf.png | enemy_mist_wolf | [Assets/_Game/Art/Enemies/DaLat/enemy_mist_wolf.png](../../Assets/_Game/Art/Enemies/DaLat/enemy_mist_wolf.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_mist_wolf |
| HoiAn_Enemies_Atlas.png | enemy_crab, enemy_ghost, enemy_mo01 | [Assets/_Game/Art/Enemies/HoiAn_Enemies_Atlas.png](../../Assets/_Game/Art/Enemies/HoiAn_Enemies_Atlas.png) | Có |  |
| enemy_archive_guard.png | enemy_archive_guard | [Assets/_Game/Art/Enemies/Hue/enemy_archive_guard.png](../../Assets/_Game/Art/Enemies/Hue/enemy_archive_guard.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_archive_guard |
| enemy_courier.png | enemy_courier | [Assets/_Game/Art/Enemies/Hue/enemy_courier.png](../../Assets/_Game/Art/Enemies/Hue/enemy_courier.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_courier |
| enemy_gate_guard.png | enemy_gate_guard, enemy_night_guard | [Assets/_Game/Art/Enemies/Hue/enemy_gate_guard.png](../../Assets/_Game/Art/Enemies/Hue/enemy_gate_guard.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_gate_guard, enemy_night_guard |
| enemy_river_wisp.png | enemy_river_wisp | [Assets/_Game/Art/Enemies/Hue/enemy_river_wisp.png](../../Assets/_Game/Art/Enemies/Hue/enemy_river_wisp.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_river_wisp |
| enemy_crane_boss.png | enemy_crane_boss | [Assets/_Game/Art/Enemies/Jeonju/enemy_crane_boss.png](../../Assets/_Game/Art/Enemies/Jeonju/enemy_crane_boss.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_crane_boss |
| enemy_paper_crane.png | enemy_paper_crane | [Assets/_Game/Art/Enemies/Jeonju/enemy_paper_crane.png](../../Assets/_Game/Art/Enemies/Jeonju/enemy_paper_crane.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_paper_crane |
| enemy_parcel_bot.png | enemy_parcel_bot | [Assets/_Game/Art/Enemies/Jeonju/enemy_parcel_bot.png](../../Assets/_Game/Art/Enemies/Jeonju/enemy_parcel_bot.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_parcel_bot |
| enemy_pattern_bot.png | enemy_pattern_bot, enemy_signal_bot | [Assets/_Game/Art/Enemies/Jeonju/enemy_pattern_bot.png](../../Assets/_Game/Art/Enemies/Jeonju/enemy_pattern_bot.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_pattern_bot, enemy_signal_bot |
| Seoul_Bridge_Bot_Banana.png | enemy_bridge | [Assets/_Game/Art/Enemies/Seoul/Seoul_Bridge_Bot_Banana.png](../../Assets/_Game/Art/Enemies/Seoul/Seoul_Bridge_Bot_Banana.png) | Có |  |
| Enemies_Roster_Concept.png | enemy_stone_guard, enemy_star_core, enemy_final_core, enemy_shadow_courier, enemy_fox | [Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png](../../Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; PROPOSED / TEMPORARY bindings, not canonical/final: enemy_stone_guard, enemy_star_core, enemy_final_core, enemy_shadow_courier, enemy_fox |
| UnitSprite.png | Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Environment/Common/UnitSprite.png](../../Assets/_Game/Art/Environment/Common/UnitSprite.png) | Không / chưa duyệt |  |
| ArchitectureAtlas.png | H3_02_Environment_Architecture_Props_0, Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Environment/HoiAn/H3/ArchitectureAtlas.png](../../Assets/_Game/Art/Environment/HoiAn/H3/ArchitectureAtlas.png) | Không / chưa duyệt |  |
| H3_StoneGround_Continuous_v1.png | H3_StoneGround_Continuous_v1, Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Environment/HoiAn/H3/H3_StoneGround_Continuous_v1.png](../../Assets/_Game/Art/Environment/HoiAn/H3/H3_StoneGround_Continuous_v1.png) | Không / chưa duyệt |  |
| Item_Healing_Tea.png | healing_tea | [Assets/_Game/Art/Items/Consumables/Item_Healing_Tea.png](../../Assets/_Game/Art/Items/Consumables/Item_Healing_Tea.png) | Có |  |
| Item_HoiAn_Chili.png | hoi_an_fresh_chili | [Assets/_Game/Art/Items/Food/Item_HoiAn_Chili.png](../../Assets/_Game/Art/Items/Food/Item_HoiAn_Chili.png) | Có | flat graphic temporary style; differs from painted LBC icons |
| Item_HoiAn_Shallot.png | hoi_an_purple_shallot | [Assets/_Game/Art/Items/Food/Item_HoiAn_Shallot.png](../../Assets/_Game/Art/Items/Food/Item_HoiAn_Shallot.png) | Có | flat graphic temporary style; differs from painted LBC icons |
| Item_Copper_Wire.png | copper_wire | [Assets/_Game/Art/Items/Materials/Item_Copper_Wire.png](../../Assets/_Game/Art/Items/Materials/Item_Copper_Wire.png) | Có |  |
| Item_Hanji_Core.png | hanji_core | [Assets/_Game/Art/Items/Quest/Item_Hanji_Core.png](../../Assets/_Game/Art/Items/Quest/Item_Hanji_Core.png) | Có |  |
| Interactables_Atlas.png | lamp_eye, lantern-part, link_key, coin, lan_glasses, meat_portion, night_flower, silla_prism, lantern_stamp, river_pass, royal_herb, lost_letter, astral_script, fog_battery, blue_water_signal, hanji_pattern_1, hanji_pattern_2, hanji_pattern_3, missing_parcel, hanji_circuit, reply_letter, star_symbol_1, star_symbol_2, star_symbol_3, relay_a, relay_b, relay_c, festival_friendship_postcard, hoi_an_bridge_postcard, jeonju_recipe_card, hue_market_note, Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Items/Shared/Interactables_Atlas.png](../../Assets/_Game/Art/Items/Shared/Interactables_Atlas.png) | Có |  |
| ItemSheet_VNKR_FoodCulture_v1.png | da_lat_strawberry | [Assets/_Game/Art/Items/Shared/ItemSheet_VNKR_FoodCulture_v1.png](../../Assets/_Game/Art/Items/Shared/ItemSheet_VNKR_FoodCulture_v1.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: da_lat_strawberry |
| DaLat_Kite_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/DaLat_Kite_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/DaLat_Kite_BG.png) | Không / chưa duyệt |  |
| DaLat_Strawberry_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/DaLat_Strawberry_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/DaLat_Strawberry_BG.png) | Không / chưa duyệt |  |
| Gyeongju_Bell_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Gyeongju_Bell_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/Gyeongju_Bell_BG.png) | Không / chưa duyệt |  |
| Gyeongju_Wishes_.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Gyeongju_Wishes_.png](../../Assets/_Game/Art/Leisure/Backgrounds/Gyeongju_Wishes_.png) | Không / chưa duyệt |  |
| HoiAn_LanternRelease_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/HoiAn_LanternRelease_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/HoiAn_LanternRelease_BG.png) | Không / chưa duyệt |  |
| HoiAn_Swing_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/HoiAn_Swing_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/HoiAn_Swing_BG.png) | Không / chưa duyệt |  |
| Hue_PedalBoat_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Hue_PedalBoat_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/Hue_PedalBoat_BG.png) | Không / chưa duyệt |  |
| Hue_Throne_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Hue_Throne_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/Hue_Throne_BG.png) | Không / chưa duyệt |  |
| Jeonju_Tea_B.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Jeonju_Tea_B.png](../../Assets/_Game/Art/Leisure/Backgrounds/Jeonju_Tea_B.png) | Không / chưa duyệt |  |
| Jeonju_Watering_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Jeonju_Watering_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/Jeonju_Watering_BG.png) | Không / chưa duyệt |  |
| Seoul_Kpop_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Seoul_Kpop_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/Seoul_Kpop_BG.png) | Không / chưa duyệt |  |
| Seoul_Tteokbokki_BG.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Backgrounds/Seoul_Tteokbokki_BG.png](../../Assets/_Game/Art/Leisure/Backgrounds/Seoul_Tteokbokki_BG.png) | Không / chưa duyệt |  |
| DaLat_KiteRack.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/DaLat_KiteRack.png](../../Assets/_Game/Art/Leisure/Props/DaLat_KiteRack.png) | Không / chưa duyệt |  |
| DaLat_StrawberryPatch.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/DaLat_StrawberryPatch.png](../../Assets/_Game/Art/Leisure/Props/DaLat_StrawberryPatch.png) | Không / chưa duyệt |  |
| Gyeongju_BellExhibit.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Gyeongju_BellExhibit.png](../../Assets/_Game/Art/Leisure/Props/Gyeongju_BellExhibit.png) | Không / chưa duyệt |  |
| Gyeongju_WishTags.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Gyeongju_WishTags.png](../../Assets/_Game/Art/Leisure/Props/Gyeongju_WishTags.png) | Không / chưa duyệt |  |
| HoiAn_LanternRelease_Stand.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/HoiAn_LanternRelease_Stand.png](../../Assets/_Game/Art/Leisure/Props/HoiAn_LanternRelease_Stand.png) | Không / chưa duyệt |  |
| HoiAn_Swing.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/HoiAn_Swing.png](../../Assets/_Game/Art/Leisure/Props/HoiAn_Swing.png) | Không / chưa duyệt |  |
| Hue_PedalSwanBoat.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Hue_PedalSwanBoat.png](../../Assets/_Game/Art/Leisure/Props/Hue_PedalSwanBoat.png) | Không / chưa duyệt |  |
| Hue_ThroneReplica.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Hue_ThroneReplica.png](../../Assets/_Game/Art/Leisure/Props/Hue_ThroneReplica.png) | Không / chưa duyệt |  |
| Jeonju_TeaTable.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Jeonju_TeaTable.png](../../Assets/_Game/Art/Leisure/Props/Jeonju_TeaTable.png) | Không / chưa duyệt |  |
| Jeonju_WateringCan_Garden.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Jeonju_WateringCan_Garden.png](../../Assets/_Game/Art/Leisure/Props/Jeonju_WateringCan_Garden.png) | Không / chưa duyệt |  |
| Seoul_KpopStage.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Seoul_KpopStage.png](../../Assets/_Game/Art/Leisure/Props/Seoul_KpopStage.png) | Không / chưa duyệt |  |
| Seoul_TteokbokkiStall.png | source / compatibility / native sprite | [Assets/_Game/Art/Leisure/Props/Seoul_TteokbokkiStall.png](../../Assets/_Game/Art/Leisure/Props/Seoul_TteokbokkiStall.png) | Không / chưa duyệt |  |
| DaLatStation.png | source / compatibility / native sprite | [Assets/_Game/Art/Maps/DaLat/DaLatStation.png](../../Assets/_Game/Art/Maps/DaLat/DaLatStation.png) | Không / chưa duyệt |  |
| Map_DaLat_DestinationPreview_A.png | da_lat | [Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png) | Có |  |
| Map_Gyeongju_Overview_A.png | gyeongju | [Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png](../../Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png) | Có |  |
| Map_HoiAn_DestinationPreview_A.png | hoi_an | [Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png) | Có |  |
| Map_HoiAn_Overview_A.png | hoi_an, hoiAnMap | [Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png](../../Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png) | Có |  |
| Map_Hue_DestinationPreview_A.png | hue | [Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png) | Có |  |
| Map_Jeonju_DestinationPreview_A.png | jeonju | [Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png) | Có |  |
| Map_Seoul_Overview_A.png | seoul | [Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png](../../Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png) | Có |  |
| npc_gardener.png | npc_gardener | [Assets/_Game/Art/NPC/DaLat/npc_gardener.png](../../Assets/_Game/Art/NPC/DaLat/npc_gardener.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_gardener |
| npc_hungry_dog.png | npc_hungry_dog | [Assets/_Game/Art/NPC/DaLat/npc_hungry_dog.png](../../Assets/_Game/Art/NPC/DaLat/npc_hungry_dog.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_hungry_dog |
| npc_lan.png | npc_lan | [Assets/_Game/Art/NPC/DaLat/npc_lan.png](../../Assets/_Game/Art/NPC/DaLat/npc_lan.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_lan |
| npc_thuy.png | npc_thuy | [Assets/_Game/Art/NPC/DaLat/npc_thuy.png](../../Assets/_Game/Art/NPC/DaLat/npc_thuy.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_thuy |
| NPC_Gyeongju_Inscription.png | npc_inscription | [Assets/_Game/Art/NPC/Gyeongju/NPC_Gyeongju_Inscription.png](../../Assets/_Game/Art/NPC/Gyeongju/NPC_Gyeongju_Inscription.png) | Có |  |
| NPC_Gyeongju_Keeper.png | npc_keeper | [Assets/_Game/Art/NPC/Gyeongju/NPC_Gyeongju_Keeper.png](../../Assets/_Game/Art/NPC/Gyeongju/NPC_Gyeongju_Keeper.png) | Có |  |
| NPC_HoiAn_Boatman.png | npc_boatman | [Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_Boatman.png](../../Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_Boatman.png) | Có |  |
| NPC_HoiAn_CoMai_Portrait.png | npc_mai, mai, maiPortrait | [Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png](../../Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png) | Có |  |
| NPC_HoiAn_MarketMerchant.png | npc_market_merchant | [Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png](../../Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png) | Có |  |
| npc_tam.png | npc_tam | [Assets/_Game/Art/NPC/Hue/npc_tam.png](../../Assets/_Game/Art/NPC/Hue/npc_tam.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_tam |
| npc_park.png | npc_park | [Assets/_Game/Art/NPC/Jeonju/npc_park.png](../../Assets/_Game/Art/NPC/Jeonju/npc_park.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_park |
| npc_yoon.png | npc_yoon | [Assets/_Game/Art/NPC/Jeonju/npc_yoon.png](../../Assets/_Game/Art/NPC/Jeonju/npc_yoon.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_yoon |
| npc_elder.png | npc_elder | [Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_elder.png](../../Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_elder.png) | Có |  |
| npc_hana.png | npc_hana | [Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_hana.png](../../Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_hana.png) | Có |  |
| npc_nguyet.png | npc_nguyet | [Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_nguyet.png](../../Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_nguyet.png) | Có |  |
| NPC_Seoul_Engineer.png | npc_engineer | [Assets/_Game/Art/NPC/Seoul/NPC_Seoul_Engineer.png](../../Assets/_Game/Art/NPC/Seoul/NPC_Seoul_Engineer.png) | Có |  |
| Seoul_Curator_Transparent.png | npc_curator | [Assets/_Game/Art/NPC/Seoul/Seoul_Curator_Transparent.png](../../Assets/_Game/Art/NPC/Seoul/Seoul_Curator_Transparent.png) | Không / chưa duyệt | Runtime sprite differs from the registry's approved GUID/localID; identity registry was not changed |
| HoiAnNpcAtlas.png | npc_ba_tu, npc_phuc, npc_tech, npc_choi_eunsu, npc_han_doyun | [Assets/_Game/Art/NPC/Shared/HoiAnNpcAtlas.png](../../Assets/_Game/Art/NPC/Shared/HoiAnNpcAtlas.png) | Có |  |
| NPC_TransparentAtlas.png | npc_guest, npc_huong, npc_soyeon, npc_scholar, npc_min, npc_kang_taeho | [Assets/_Game/Art/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/NPC/Shared/NPC_TransparentAtlas.png) | Hỗn hợp / xem từng sprite | PROPOSED / TEMPORARY bindings, not canonical/final: npc_scholar, npc_min |
| Artifact_DaLat_MistCompass_Source.png | relic_dalat_mist_compass, fragment_da_lat_lake_glass, fragment_da_lat_pine_direction, fragment_da_lat_mist_bell, fragment_da_lat_blue_signal_seed | [Assets/_Game/Art/Relics/Artifacts/Artifact_DaLat_MistCompass_Source.png](../../Assets/_Game/Art/Relics/Artifacts/Artifact_DaLat_MistCompass_Source.png) | Không / chưa duyệt |  |
| Artifact_Gyeongju_StarlightLens_Source.png | stargazer_lens, relic_gyeongju_star_lens, fragment_gyeongju_min_record, fragment_gyeongju_star_stone, fragment_gyeongju_statue_reflection, fragment_gyeongju_heritage_ring | [Assets/_Game/Art/Relics/Artifacts/Artifact_Gyeongju_StarlightLens_Source.png](../../Assets/_Game/Art/Relics/Artifacts/Artifact_Gyeongju_StarlightLens_Source.png) | Không / chưa duyệt |  |
| Artifact_HoiAn_Lantern_Source.png | relic_hoi_an_lantern, fragment_hoi_an_bamboo_frame, fragment_hoi_an_old_flower_paper, fragment_hoi_an_roof_hook, fragment_hoi_an_eye_reflection | [Assets/_Game/Art/Relics/Artifacts/Artifact_HoiAn_Lantern_Source.png](../../Assets/_Game/Art/Relics/Artifacts/Artifact_HoiAn_Lantern_Source.png) | Không / chưa duyệt |  |
| Artifact_Hue_DragonSeal_Source.png | ancient_seal, relic_hue_dragon_seal, fragment_hue_river_jade, fragment_hue_dragon_face, fragment_hue_ritual_cord, fragment_hue_rhythm_inscription | [Assets/_Game/Art/Relics/Artifacts/Artifact_Hue_DragonSeal_Source.png](../../Assets/_Game/Art/Relics/Artifacts/Artifact_Hue_DragonSeal_Source.png) | Không / chưa duyệt |  |
| Artifact_Jeonju_HanjiSeal_Source.png | relic_jeonju_hanji_stamp, fragment_jeonju_hanok_pattern, fragment_jeonju_dyed_thread, fragment_jeonju_old_hanji_core, fragment_jeonju_greeting_stamp | [Assets/_Game/Art/Relics/Artifacts/Artifact_Jeonju_HanjiSeal_Source.png](../../Assets/_Game/Art/Relics/Artifacts/Artifact_Jeonju_HanjiSeal_Source.png) | Không / chưa duyệt |  |
| Artifact_Seoul_ThreeRelayPrism_Source.png | relic_seoul_trirelay_prism, fragment_seoul_relay_frame | [Assets/_Game/Art/Relics/Artifacts/Artifact_Seoul_ThreeRelayPrism_Source.png](../../Assets/_Game/Art/Relics/Artifacts/Artifact_Seoul_ThreeRelayPrism_Source.png) | Không / chưa duyệt |  |
| Fragment_HeritageData.png | fragment_seoul_heritage_data | [Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_HeritageData.png](../../Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_HeritageData.png) | Không / chưa duyệt |  |
| Fragment_JiwonSync.png | fragment_seoul_jiwon_sync | [Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_JiwonSync.png](../../Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_JiwonSync.png) | Không / chưa duyệt |  |
| Fragment_StellarRing.png | fragment_seoul_stellar_ring | [Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_StellarRing.png](../../Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_StellarRing.png) | Không / chưa duyệt |  |
| LBC_Android_AdaptiveBackground.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/AppIcon/LBC_Android_AdaptiveBackground.png](../../Assets/_Game/Art/UI/AppIcon/LBC_Android_AdaptiveBackground.png) | Không / chưa duyệt |  |
| LBC_Android_AdaptiveForeground.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/AppIcon/LBC_Android_AdaptiveForeground.png](../../Assets/_Game/Art/UI/AppIcon/LBC_Android_AdaptiveForeground.png) | Không / chưa duyệt |  |
| LBC_Android_LegacyRound.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/AppIcon/LBC_Android_LegacyRound.png](../../Assets/_Game/Art/UI/AppIcon/LBC_Android_LegacyRound.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| BlankPanelsAtlas.png | H3_05_UI_BlankPanels_Sheet_0, Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/UI/H3/BlankPanelsAtlas.png](../../Assets/_Game/Art/UI/H3/BlankPanelsAtlas.png) | Không / chưa duyệt |  |
| UI_World_Globe.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/Icons/UI_World_Globe.png](../../Assets/_Game/Art/UI/Icons/UI_World_Globe.png) | Không / chưa duyệt |  |
| Marker_BossAlert.png | BossAlert | [Assets/_Game/Art/UI/Markers/Marker_BossAlert.png](../../Assets/_Game/Art/UI/Markers/Marker_BossAlert.png) | Có |  |
| Marker_Camera.png | Camera | [Assets/_Game/Art/UI/Markers/Marker_Camera.png](../../Assets/_Game/Art/UI/Markers/Marker_Camera.png) | Có |  |
| Marker_Completed.png | Completed | [Assets/_Game/Art/UI/Markers/Marker_Completed.png](../../Assets/_Game/Art/UI/Markers/Marker_Completed.png) | Có |  |
| Marker_Current.png | Current | [Assets/_Game/Art/UI/Markers/Marker_Current.png](../../Assets/_Game/Art/UI/Markers/Marker_Current.png) | Có |  |
| Marker_Ground.png | Ground | [Assets/_Game/Art/UI/Markers/Marker_Ground.png](../../Assets/_Game/Art/UI/Markers/Marker_Ground.png) | Có |  |
| Marker_NpcNeedItem.png | NpcNeedItem | [Assets/_Game/Art/UI/Markers/Marker_NpcNeedItem.png](../../Assets/_Game/Art/UI/Markers/Marker_NpcNeedItem.png) | Có |  |
| Marker_NpcTalk.png | NpcTalk | [Assets/_Game/Art/UI/Markers/Marker_NpcTalk.png](../../Assets/_Game/Art/UI/Markers/Marker_NpcTalk.png) | Có |  |
| Marker_Puzzle.png | Puzzle | [Assets/_Game/Art/UI/Markers/Marker_Puzzle.png](../../Assets/_Game/Art/UI/Markers/Marker_Puzzle.png) | Có |  |
| Marker_Quest.png | Quest | [Assets/_Game/Art/UI/Markers/Marker_Quest.png](../../Assets/_Game/Art/UI/Markers/Marker_Quest.png) | Có |  |
| Marker_Reward.png | Reward | [Assets/_Game/Art/UI/Markers/Marker_Reward.png](../../Assets/_Game/Art/UI/Markers/Marker_Reward.png) | Có |  |
| node_available.png | available | [Assets/_Game/Art/UI/Markers/Navigation/node_available.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_available.png) | Không / chưa duyệt |  |
| node_current.png | current | [Assets/_Game/Art/UI/Markers/Navigation/node_current.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_current.png) | Không / chưa duyệt |  |
| node_enemy.png | enemy | [Assets/_Game/Art/UI/Markers/Navigation/node_enemy.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_enemy.png) | Không / chưa duyệt |  |
| node_item.png | item, Assets/_Game/Scenes\HoiAnDemo.unity, Assets/_Game/Prefabs\DesignKit\LanternPartMarker.prefab | [Assets/_Game/Art/UI/Markers/Navigation/node_item.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_item.png) | Không / chưa duyệt |  |
| node_locked.png | locked | [Assets/_Game/Art/UI/Markers/Navigation/node_locked.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_locked.png) | Không / chưa duyệt |  |
| node_npc.png | npc, Assets/_Game/Scenes\HoiAnDemo.unity, Assets/_Game/Prefabs\DesignKit\NpcMarker.prefab | [Assets/_Game/Art/UI/Markers/Navigation/node_npc.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_npc.png) | Không / chưa duyệt |  |
| node_portal.png | portal | [Assets/_Game/Art/UI/Markers/Navigation/node_portal.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_portal.png) | Không / chưa duyệt |  |
| node_puzzle.png | puzzle | [Assets/_Game/Art/UI/Markers/Navigation/node_puzzle.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_puzzle.png) | Không / chưa duyệt |  |
| node_quest.png | quest | [Assets/_Game/Art/UI/Markers/Navigation/node_quest.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_quest.png) | Không / chưa duyệt |  |
| node_secret.png | secret | [Assets/_Game/Art/UI/Markers/Navigation/node_secret.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_secret.png) | Không / chưa duyệt |  |
| node_visited.png | visited | [Assets/_Game/Art/UI/Markers/Navigation/node_visited.png](../../Assets/_Game/Art/UI/Markers/Navigation/node_visited.png) | Không / chưa duyệt |  |
| LBC_UI_SkinAtlas.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/Panels/LBC_UI_SkinAtlas.png](../../Assets/_Game/Art/UI/Panels/LBC_UI_SkinAtlas.png) | Không / chưa duyệt |  |
| 01_hoi_an_H2_tiem_den_co_mai.png | hoi_an.h2 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/01_hoi_an_H2_tiem_den_co_mai.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/01_hoi_an_H2_tiem_den_co_mai.png) | Không / chưa duyệt |  |
| 02_hoi_an_H3_hem_mai_ngoi.png | hoi_an.h3 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/02_hoi_an_H3_hem_mai_ngoi.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/02_hoi_an_H3_hem_mai_ngoi.png) | Không / chưa duyệt |  |
| 03_hoi_an_H4_cho_ven_song.png | hoi_an.h4 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/03_hoi_an_H4_cho_ven_song.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/03_hoi_an_H4_cho_ven_song.png) | Không / chưa duyệt |  |
| 04_hoi_an_H6_cau_co_anh_den.png | hoi_an.h6 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/04_hoi_an_H6_cau_co_anh_den.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/04_hoi_an_H6_cau_co_anh_den.png) | Không / chưa duyệt |  |
| 05_hoi_an_H7_tram_anh_sang_hoi_an.png | hoi_an.h7 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/05_hoi_an_H7_tram_anh_sang_hoi_an.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/05_hoi_an_H7_tram_anh_sang_hoi_an.png) | Không / chưa duyệt |  |
| 06_hoi_an_H17_ben_do_pho_co.png | hoi_an.h17 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/06_hoi_an_H17_ben_do_pho_co.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/06_hoi_an_H17_ben_do_pho_co.png) | Không / chưa duyệt |  |
| 07_hue_U4_dau_cau_vao_thanh.png | hue_u4_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/07_hue_U4_dau_cau_vao_thanh.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/07_hue_U4_dau_cau_vao_thanh.png) | Không / chưa duyệt |  |
| 08_hue_U6_cua_ngo_mon_cach_dieu.png | hue_u6_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/08_hue_U6_cua_ngo_mon_cach_dieu.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/08_hue_U6_cua_ngo_mon_cach_dieu.png) | Không / chưa duyệt |  |
| 09_da_lat_D0_ben_ho_xuan_huong.png | dalat_checkin_0 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/09_da_lat_D0_ben_ho_xuan_huong.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/09_da_lat_D0_ben_ho_xuan_huong.png) | Không / chưa duyệt |  |
| 10_da_lat_D14_dai_dan_sang_nui.png | dalat_checkin_14 | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/10_da_lat_D14_dai_dan_sang_nui.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/10_da_lat_D14_dai_dan_sang_nui.png) | Không / chưa duyệt |  |
| 11_jeonju_J8_cau_da_tay.png | jeonju_j8_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/11_jeonju_J8_cau_da_tay.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/11_jeonju_J8_cau_da_tay.png) | Không / chưa duyệt |  |
| 12_jeonju_J11_dai_ngam_mai_ngoi.png | jeonju_j11_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/12_jeonju_J11_dai_ngam_mai_ngoi.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/12_jeonju_J11_dai_ngam_mai_ngoi.png) | Không / chưa duyệt |  |
| 13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png | gyeongju_g4_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png) | Không / chưa duyệt |  |
| 14_gyeongju_G14_kho_ban_khac_di_tich.png | gyeongju_g14_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/14_gyeongju_G14_kho_ban_khac_di_tich.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/14_gyeongju_G14_kho_ban_khac_di_tich.png) | Không / chưa duyệt |  |
| 15_gyeongju_G17_dai_long_den_bo_tay.png | gyeongju_g17_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/15_gyeongju_G17_dai_long_den_bo_tay.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/15_gyeongju_G17_dai_long_den_bo_tay.png) | Không / chưa duyệt |  |
| 16_seoul_S2_cong_thanh_xua_ben_song_han.png | seoul_s2_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/16_seoul_S2_cong_thanh_xua_ben_song_han.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/16_seoul_S2_cong_thanh_xua_ben_song_han.png) | Không / chưa duyệt |  |
| 17_seoul_S10_chan_thap_n_seoul.png | seoul_s10_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/17_seoul_S10_chan_thap_n_seoul.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/17_seoul_S10_chan_thap_n_seoul.png) | Không / chưa duyệt |  |
| 18_seoul_S18_role_anh_den_hanji.png | seoul_s18_checkin | [Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/18_seoul_S18_role_anh_den_hanji.png](../../Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/18_seoul_S18_role_anh_den_hanji.png) | Không / chưa duyệt |  |

## NPC liên thành phố đã chốt

Chị Hương và So-yeon đã được người dùng xác nhận CANONICAL. Hai gương mặt khác nhau; không thay Mai hoặc Ji-eun, không dùng mặt So-yeon cho Ji-won. Các status tích hợp cũ trong Unity asset là chú thích lịch sử; duyệt danh tính hiện hành theo NPC_CANONICAL_REGISTRY.

| ID | Nhân vật | Hình giữ nguyên | Trạng thái |
|---|---|---|---|
| npc_huong | Chị Hương, U1 | Cô gái áo xanh, mũ hoa/giỏ hoa, hàng trên atlas | CANONICAL |
| npc_soyeon | So-yeon, J15 | Cô gái kính, áo xanh cầm tablet, hàng dưới atlas | CANONICAL |

Hai native sprite dùng outline theo alpha + UI sprite mesh để loại pixels nhân vật bên cạnh. Không tạo bitmap trùng, không sửa nguồn; giữ GUID và sprite npc_guest của Ji-eun. Tọa độ/outline trong CrossCitySpriteSlices.json.

## Hình đề xuất cho các chương mới

Các binding dưới đã có sprite cụ thể, nhưng chưa được người dùng duyệt canonical/final. Native slice theo vai trò roster và ảnh dẫn xuất AI được ghi riêng; đây là cutout tĩnh, chưa phải animation. Không thay Cô Mai hoặc Ji-eun.

| ID | Vai trò | Sprite | Trạng thái |
|---|---|---|---|
| npc_scholar | npc | Assets/_Game/Art/NPC/Shared/NPC_TransparentAtlas.png / npc_scholar | EXISTING_ROSTER_ROLE_MATCH; NOT_CANONICAL; named_identity_assignment_pending_approval |
| npc_tam | npc | Assets/_Game/Art/NPC/Hue/npc_tam.png / npc_tam | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_yoon | npc | Assets/_Game/Art/NPC/Jeonju/npc_yoon.png / npc_yoon | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_park | npc | Assets/_Game/Art/NPC/Jeonju/npc_park.png / npc_park | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_lan | npc | Assets/_Game/Art/NPC/DaLat/npc_lan.png / npc_lan | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_thuy | npc | Assets/_Game/Art/NPC/DaLat/npc_thuy.png / npc_thuy | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_gardener | npc | Assets/_Game/Art/NPC/DaLat/npc_gardener.png / npc_gardener | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_hungry_dog | npc | Assets/_Game/Art/NPC/DaLat/npc_hungry_dog.png / npc_hungry_dog | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_min | npc | Assets/_Game/Art/NPC/Shared/NPC_TransparentAtlas.png / npc_min | TEMPORARY_ROLE_MATCH_FROM_UNASSIGNED_ARCHAEOLOGICAL_GUIDE; NOT_CANONICAL_IDENTITY_APPROVAL_PENDING |
| enemy_river_wisp | enemy | Assets/_Game/Art/Enemies/Hue/enemy_river_wisp.png / enemy_river_wisp | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_courier | enemy | Assets/_Game/Art/Enemies/Hue/enemy_courier.png / enemy_courier | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_archive_guard | enemy | Assets/_Game/Art/Enemies/Hue/enemy_archive_guard.png / enemy_archive_guard | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_gate_guard | enemy | Assets/_Game/Art/Enemies/Hue/enemy_gate_guard.png / enemy_gate_guard | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_parcel_bot | enemy | Assets/_Game/Art/Enemies/Jeonju/enemy_parcel_bot.png / enemy_parcel_bot | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_paper_crane | enemy | Assets/_Game/Art/Enemies/Jeonju/enemy_paper_crane.png / enemy_paper_crane | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_pattern_bot | enemy | Assets/_Game/Art/Enemies/Jeonju/enemy_pattern_bot.png / enemy_pattern_bot | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_crane_boss | enemy | Assets/_Game/Art/Enemies/Jeonju/enemy_crane_boss.png / enemy_crane_boss | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_glass_thief | enemy | Assets/_Game/Art/Enemies/DaLat/enemy_glass_thief.png / enemy_glass_thief | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| enemy_fog_boss | enemy | Assets/_Game/Art/Enemies/DaLat/enemy_fog_boss.png / enemy_fog_boss | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| enemy_mist_wolf | enemy | Assets/_Game/Art/Enemies/DaLat/enemy_mist_wolf.png / enemy_mist_wolf | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| enemy_stone_guard | enemy | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png / stone_guard | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved |
| enemy_star_core | enemy | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png / energy_core | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved |
| enemy_final_core | enemy | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png / energy_core | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved |
| enemy_shadow_courier | enemy | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png / shadow_courier | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved |
| enemy_fox | enemy | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png / shadow_cat | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; cat concept pending actual fox sprite; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved |
| enemy_night_guard | enemy | Assets/_Game/Art/Enemies/Hue/enemy_gate_guard.png / enemy_gate_guard | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_gate_guard |
| enemy_drone | enemy | Assets/_Game/Art/Enemies/DaLat/enemy_glass_thief.png / enemy_glass_thief | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_glass_thief |
| enemy_signal_bot | enemy | Assets/_Game/Art/Enemies/Jeonju/enemy_pattern_bot.png / enemy_pattern_bot | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_pattern_bot |

## Di chuyển thực tế

| Cũ | Mới | GUID |
|---|---|---|
| Assets/_Game/Art/DesignKit/Markers/node_available.png | Assets/_Game/Art/UI/Markers/Navigation/node_available.png | 4a9f6b4446c499848bb9d3d1e60ec600 |
| Assets/_Game/Art/DesignKit/Markers/node_current.png | Assets/_Game/Art/UI/Markers/Navigation/node_current.png | 0e46fa3a4a730ff4da2af78097a0e7b8 |
| Assets/_Game/Art/DesignKit/Markers/node_enemy.png | Assets/_Game/Art/UI/Markers/Navigation/node_enemy.png | ddbeae466d78e5941af1f2d58373540d |
| Assets/_Game/Art/DesignKit/Markers/node_item.png | Assets/_Game/Art/UI/Markers/Navigation/node_item.png | 8d87ce1f1365b4545931249b25505c95 |
| Assets/_Game/Art/DesignKit/Markers/node_locked.png | Assets/_Game/Art/UI/Markers/Navigation/node_locked.png | fae34817a0d319145baf618ff62d512a |
| Assets/_Game/Art/DesignKit/Markers/node_npc.png | Assets/_Game/Art/UI/Markers/Navigation/node_npc.png | e5c90b4c7bb2cc849a556a6729d24107 |
| Assets/_Game/Art/DesignKit/Markers/node_portal.png | Assets/_Game/Art/UI/Markers/Navigation/node_portal.png | c0307cd2431a2754b940fbec57653e10 |
| Assets/_Game/Art/DesignKit/Markers/node_puzzle.png | Assets/_Game/Art/UI/Markers/Navigation/node_puzzle.png | 41b4328d91767bc459398185aaf68569 |
| Assets/_Game/Art/DesignKit/Markers/node_quest.png | Assets/_Game/Art/UI/Markers/Navigation/node_quest.png | ba53cdd81d3eca04eb7229ec95501c1c |
| Assets/_Game/Art/DesignKit/Markers/node_secret.png | Assets/_Game/Art/UI/Markers/Navigation/node_secret.png | e8513865616e8b644a97488406359aac |
| Assets/_Game/Art/DesignKit/Markers/node_visited.png | Assets/_Game/Art/UI/Markers/Navigation/node_visited.png | 66d590fef2a744d4baca23050d763ecc |
| Assets/_Game/Art/Generated/placeholder.png | Assets/_Game/Art/Environment/Common/UnitSprite.png | 3ce7b373394f4034ca0e0dcd40b1d45a |
| Assets/_Game/Art/Item/Derived/Item_HoiAn_Chili.png | Assets/_Game/Art/Items/Food/Item_HoiAn_Chili.png | 9ca8b1632a045ca419f9b8e28bbd201f |
| Assets/_Game/Art/Item/Derived/Item_HoiAn_Shallot.png | Assets/_Game/Art/Items/Food/Item_HoiAn_Shallot.png | 265c03f325521254a8dff9674cc2b3f7 |
| Assets/_Game/Art/Runtime/AdventureUI/Boatman_Portrait.png | Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_Boatman.png | 3835aa72bd8c88b41af32d4aabd40b98 |
| Assets/_Game/Art/Runtime/AdventureUI/City_da_lat.png | Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png | 44c553d406b840740a51a62491818111 |
| Assets/_Game/Art/Runtime/AdventureUI/City_gyeongju.png | Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png | 8303af4dae6df924f9d9779aa1272596 |
| Assets/_Game/Art/Runtime/AdventureUI/City_hoi_an.png | Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png | add0499b1e25b674f97625fcc443e3e6 |
| Assets/_Game/Art/Runtime/AdventureUI/City_hue.png | Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png | 481d167093be364419d9ebb11b8e4e27 |
| Assets/_Game/Art/Runtime/AdventureUI/City_jeonju.png | Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png | a81827839b4593840a4f9ed1ebc59f9d |
| Assets/_Game/Art/Runtime/AdventureUI/City_seoul.png | Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png | c863c93d8325d8f46a10cfa990f0229e |
| Assets/_Game/Art/Runtime/AdventureUI/Copper_Wire.png | Assets/_Game/Art/Items/Materials/Item_Copper_Wire.png | 6292bac9748f22042aff4a6e8a1bcfbf |
| Assets/_Game/Art/Runtime/AdventureUI/Enemies_HoiAn_TransparentAtlas.png | Assets/_Game/Art/Enemies/HoiAn_Enemies_Atlas.png | 0aa7f368b0015354e9303a88022a2b51 |
| Assets/_Game/Art/Runtime/AdventureUI/Hanji_Core.png | Assets/_Game/Art/Items/Quest/Item_Hanji_Core.png | 9c6ac872d96c048409e5f98912531ebf |
| Assets/_Game/Art/Runtime/AdventureUI/Healing_Tea.png | Assets/_Game/Art/Items/Consumables/Item_Healing_Tea.png | 589849a96242ac74fb324aa67d104496 |
| Assets/_Game/Art/Runtime/AdventureUI/Items_TransparentAtlas.png | Assets/_Game/Art/Items/Shared/Interactables_Atlas.png | b0d1662050b200c4e94c139bfde08b33 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/BossAlert.png | Assets/_Game/Art/UI/Markers/Marker_BossAlert.png | 93c43af33a033414dbbcc8c6a2674d14 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Camera.png | Assets/_Game/Art/UI/Markers/Marker_Camera.png | e6ff7bf47d518024b9b5617581a369ef |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Completed.png | Assets/_Game/Art/UI/Markers/Marker_Completed.png | e58a9cea55b466a419729d26936f1b8c |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Current.png | Assets/_Game/Art/UI/Markers/Marker_Current.png | 39d25111b5b7f4e48b132c979ba458d8 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Ground.png | Assets/_Game/Art/UI/Markers/Marker_Ground.png | eddf12bed32f6ac47ad4a5ab8ad7f0df |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/NpcNeedItem.png | Assets/_Game/Art/UI/Markers/Marker_NpcNeedItem.png | fc55f8d3f64666c40b089a773f4f4a05 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/NpcTalk.png | Assets/_Game/Art/UI/Markers/Marker_NpcTalk.png | 7d6c6ca2f3bb88444969f4378d8f38b6 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Puzzle.png | Assets/_Game/Art/UI/Markers/Marker_Puzzle.png | a18d72cffa21b024d840d7bef6462af9 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Quest.png | Assets/_Game/Art/UI/Markers/Marker_Quest.png | a594009578f391d4eb1ae20fa8d423c4 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Reward.png | Assets/_Game/Art/UI/Markers/Marker_Reward.png | cc35585804661c949a22a136240db6d5 |
| Assets/_Game/Art/Runtime/AdventureUI/Market_Merchant.png | Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png | 0a91b2dbca846994bbade4807ff62596 |
| Assets/_Game/Art/Runtime/AdventureUI/NPC_TransparentAtlas.png | Assets/_Game/Art/NPC/Shared/NPC_TransparentAtlas.png | c6176fa957f7eb549b9b40703e5038ea |
| Assets/_Game/Art/Runtime/AdventureUI/Theme/LBC_UI_SkinAtlas.png | Assets/_Game/Art/UI/Panels/LBC_UI_SkinAtlas.png | bf36cd9e4ac7cfd409810a756c4dfb99 |
| Assets/_Game/Art/Runtime/AdventureUI/Theme/World_Globe.png | Assets/_Game/Art/UI/Icons/UI_World_Globe.png | a56225f02ce31104daf479ecd89eaea8 |
| Assets/_Game/Art/Runtime/CityMaps/HoiAn_Map_Overview_A.png | Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png | 9f457fcf0b8450940a9493323d9febcc |
| Assets/_Game/Art/Runtime/Lumi/Lumi_MapTraveler.png | Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png | 9a7d3aad847598b419b6b01ab212a053 |
| Assets/_Game/Art/Runtime/NPCs/Mai_DialoguePortrait.png | Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png | a6cc838dd76ae7f40902d846693f4a5e |
| Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png | Assets/_Game/Art/NPC/DaLat/npc_gardener.png | cac9b7f04788a1743af1858d523019df |
| Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png | Assets/_Game/Art/NPC/DaLat/npc_hungry_dog.png | 3df301f30edc9d243899518bb89fb538 |
| Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png | Assets/_Game/Art/NPC/DaLat/npc_lan.png | fb6c63798ffdb6841a4fd318adf72a74 |
| Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png | Assets/_Game/Art/NPC/DaLat/npc_thuy.png | 33ce1e750bcd0124d9feb5cfcbf3fa66 |
| Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Inscription.png | Assets/_Game/Art/NPC/Gyeongju/NPC_Gyeongju_Inscription.png | 399ff7e49a026ed42b9cb627929f5b35 |
| Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Keeper.png | Assets/_Game/Art/NPC/Gyeongju/NPC_Gyeongju_Keeper.png | 880215f55ae56f2418894f7baed7f6cb |
| Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png | Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_Boatman.png | 3835aa72bd8c88b41af32d4aabd40b98 |
| Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png | Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png | a6cc838dd76ae7f40902d846693f4a5e |
| Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png | Assets/_Game/Art/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png | 0a91b2dbca846994bbade4807ff62596 |
| Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png | Assets/_Game/Art/NPC/Hue/npc_tam.png | 7eb9ff722f5f81748945b06d9e1d05bd |
| Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png | Assets/_Game/Art/NPC/Jeonju/npc_park.png | 36a0c8ea46b133e40bf8731d313402d7 |
| Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png | Assets/_Game/Art/NPC/Jeonju/npc_yoon.png | 9e80ff4e3ffd49044b5dabf92b3d6b27 |
| Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_elder.png | Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_elder.png | 3056bc602bbb42945ba0edc767f09275 |
| Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_hana.png | Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_hana.png | 2683b36a1f2e0f24aa056fc4a46ab1d8 |
| Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_nguyet.png | Assets/_Game/Art/NPC/OwnerRevision_20261021/npc_nguyet.png | 90853a883999d1a4090495dd15aa99d0 |
| Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Curator.png | ../_LBC_ARCHIVE/HistoricalArt/RetiredAuthoring/NPC_Seoul_Curator.png | 6b7b12ad613bda14d969e359a2f08d59 |
| Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Engineer.png | Assets/_Game/Art/NPC/Seoul/NPC_Seoul_Engineer.png | 4f74a41c66ef3364ca044bc04afa2bcb |
| Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png | Assets/_Game/Art/NPC/Shared/NPC_TransparentAtlas.png | c6176fa957f7eb549b9b40703e5038ea |
| Assets/_Game/Art/Characters/Enemies/CleanDerived/energy_core_Clean.png | Assets/_Game/Art/Enemies/CleanDerived/energy_core_Clean.png | e608fc7a2b904ee4383edf2411cdb09c |
| Assets/_Game/Art/Characters/Enemies/CleanDerived/shadow_cat_Clean.png | Assets/_Game/Art/Enemies/CleanDerived/shadow_cat_Clean.png | d4b77d201c42ac94ab4101dc85438292 |
| Assets/_Game/Art/Characters/Enemies/CleanDerived/shadow_courier_Clean.png | Assets/_Game/Art/Enemies/CleanDerived/shadow_courier_Clean.png | 4fe019f7e16bb0e4a8558bd3c9865422 |
| Assets/_Game/Art/Characters/Enemies/CleanDerived/stone_guard_Clean.png | Assets/_Game/Art/Enemies/CleanDerived/stone_guard_Clean.png | 1bbc6c35ab0344e45ad815f08cd1f948 |
| Assets/_Game/Art/Characters/Enemies/DaLat/enemy_fog_boss.png | Assets/_Game/Art/Enemies/DaLat/enemy_fog_boss.png | 02f4eb7b51161784eaf10c79ca3bac67 |
| Assets/_Game/Art/Characters/Enemies/DaLat/enemy_glass_thief.png | Assets/_Game/Art/Enemies/DaLat/enemy_glass_thief.png | b38571563586e3541b1be7cac75c2a0f |
| Assets/_Game/Art/Characters/Enemies/DaLat/enemy_mist_wolf.png | Assets/_Game/Art/Enemies/DaLat/enemy_mist_wolf.png | 34574e7c8d1443043a96fb1bf85be2be |
| Assets/_Game/Art/Characters/Enemies/HoiAn_Enemies_Atlas.png | Assets/_Game/Art/Enemies/HoiAn_Enemies_Atlas.png | 0aa7f368b0015354e9303a88022a2b51 |
| Assets/_Game/Art/Characters/Enemies/Hue/enemy_archive_guard.png | Assets/_Game/Art/Enemies/Hue/enemy_archive_guard.png | 6e45e2d614a36a14cb99af2ff09bb551 |
| Assets/_Game/Art/Characters/Enemies/Hue/enemy_courier.png | Assets/_Game/Art/Enemies/Hue/enemy_courier.png | 19833cb28726e514e94c269bc5be0f65 |
| Assets/_Game/Art/Characters/Enemies/Hue/enemy_gate_guard.png | Assets/_Game/Art/Enemies/Hue/enemy_gate_guard.png | ab1b66b2c62e1ef479ccdb953644d767 |
| Assets/_Game/Art/Characters/Enemies/Hue/enemy_river_wisp.png | Assets/_Game/Art/Enemies/Hue/enemy_river_wisp.png | e6d7429b54bd9344582006412f6b095d |
| Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_crane_boss.png | Assets/_Game/Art/Enemies/Jeonju/enemy_crane_boss.png | 9a0778a8c427a2243857a0d6bf9aaf14 |
| Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_paper_crane.png | Assets/_Game/Art/Enemies/Jeonju/enemy_paper_crane.png | 6bada230f9ce7964d80e3e565afd80fc |
| Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_parcel_bot.png | Assets/_Game/Art/Enemies/Jeonju/enemy_parcel_bot.png | 24efe2bdb706ca641bdb839342c631ce |
| Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_pattern_bot.png | Assets/_Game/Art/Enemies/Jeonju/enemy_pattern_bot.png | 2162ef03eff775741acd49e8d094d377 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_available.png | Assets/_Game/Art/UI/Markers/Navigation/node_available.png | 4a9f6b4446c499848bb9d3d1e60ec600 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_current.png | Assets/_Game/Art/UI/Markers/Navigation/node_current.png | 0e46fa3a4a730ff4da2af78097a0e7b8 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_enemy.png | Assets/_Game/Art/UI/Markers/Navigation/node_enemy.png | ddbeae466d78e5941af1f2d58373540d |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_item.png | Assets/_Game/Art/UI/Markers/Navigation/node_item.png | 8d87ce1f1365b4545931249b25505c95 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_locked.png | Assets/_Game/Art/UI/Markers/Navigation/node_locked.png | fae34817a0d319145baf618ff62d512a |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_npc.png | Assets/_Game/Art/UI/Markers/Navigation/node_npc.png | e5c90b4c7bb2cc849a556a6729d24107 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_portal.png | Assets/_Game/Art/UI/Markers/Navigation/node_portal.png | c0307cd2431a2754b940fbec57653e10 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_puzzle.png | Assets/_Game/Art/UI/Markers/Navigation/node_puzzle.png | 41b4328d91767bc459398185aaf68569 |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_quest.png | Assets/_Game/Art/UI/Markers/Navigation/node_quest.png | ba53cdd81d3eca04eb7229ec95501c1c |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_secret.png | Assets/_Game/Art/UI/Markers/Navigation/node_secret.png | e8513865616e8b644a97488406359aac |
| Assets/_Game/Art/Markers/LegacyDesignKit/node_visited.png | Assets/_Game/Art/UI/Markers/Navigation/node_visited.png | 66d590fef2a744d4baca23050d763ecc |
| Assets/_Game/Art/Markers/Marker_BossAlert.png | Assets/_Game/Art/UI/Markers/Marker_BossAlert.png | 93c43af33a033414dbbcc8c6a2674d14 |
| Assets/_Game/Art/Markers/Marker_Camera.png | Assets/_Game/Art/UI/Markers/Marker_Camera.png | e6ff7bf47d518024b9b5617581a369ef |
| Assets/_Game/Art/Markers/Marker_Completed.png | Assets/_Game/Art/UI/Markers/Marker_Completed.png | e58a9cea55b466a419729d26936f1b8c |
| Assets/_Game/Art/Markers/Marker_Current.png | Assets/_Game/Art/UI/Markers/Marker_Current.png | 39d25111b5b7f4e48b132c979ba458d8 |
| Assets/_Game/Art/Markers/Marker_Ground.png | Assets/_Game/Art/UI/Markers/Marker_Ground.png | eddf12bed32f6ac47ad4a5ab8ad7f0df |
| Assets/_Game/Art/Markers/Marker_NpcNeedItem.png | Assets/_Game/Art/UI/Markers/Marker_NpcNeedItem.png | fc55f8d3f64666c40b089a773f4f4a05 |
| Assets/_Game/Art/Markers/Marker_NpcTalk.png | Assets/_Game/Art/UI/Markers/Marker_NpcTalk.png | 7d6c6ca2f3bb88444969f4378d8f38b6 |
| Assets/_Game/Art/Markers/Marker_Puzzle.png | Assets/_Game/Art/UI/Markers/Marker_Puzzle.png | a18d72cffa21b024d840d7bef6462af9 |
| Assets/_Game/Art/Markers/Marker_Quest.png | Assets/_Game/Art/UI/Markers/Marker_Quest.png | a594009578f391d4eb1ae20fa8d423c4 |
| Assets/_Game/Art/Markers/Marker_Reward.png | Assets/_Game/Art/UI/Markers/Marker_Reward.png | cc35585804661c949a22a136240db6d5 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/01_hoi_an_H2_tiem_den_co_mai.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/01_hoi_an_H2_tiem_den_co_mai.png | 247b1cc4f2d741c4db59cad9b0edc181 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/02_hoi_an_H3_hem_mai_ngoi.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/02_hoi_an_H3_hem_mai_ngoi.png | 95b7493cc9ff63f4db475f4f2d464733 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/03_hoi_an_H4_cho_ven_song.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/03_hoi_an_H4_cho_ven_song.png | 1911a6933858d6446a1261fdf486cb60 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/04_hoi_an_H6_cau_co_anh_den.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/04_hoi_an_H6_cau_co_anh_den.png | 268a3773f25105c4c979418f8837ec23 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/05_hoi_an_H7_tram_anh_sang_hoi_an.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/05_hoi_an_H7_tram_anh_sang_hoi_an.png | ae6538a3f6575f14298485c0870eea9c |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/06_hoi_an_H17_ben_do_pho_co.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/06_hoi_an_H17_ben_do_pho_co.png | 9f215b865808620448a73ee116fd41df |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/07_hue_U4_dau_cau_vao_thanh.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/07_hue_U4_dau_cau_vao_thanh.png | 565a4706fcd54b249af63ec5c5deb070 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/08_hue_U6_cua_ngo_mon_cach_dieu.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/08_hue_U6_cua_ngo_mon_cach_dieu.png | dda4f78bbb33012448b98e8fb40afe94 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/09_da_lat_D0_ben_ho_xuan_huong.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/09_da_lat_D0_ben_ho_xuan_huong.png | aced49c3bdf688e45ab038ba9e678a3f |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/10_da_lat_D14_dai_dan_sang_nui.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/10_da_lat_D14_dai_dan_sang_nui.png | 0eb05e0bc784f9549b4836c5b12da2e9 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/11_jeonju_J8_cau_da_tay.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/11_jeonju_J8_cau_da_tay.png | e634ae5d35e5e0a43b35c0966ee2fbe9 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/12_jeonju_J11_dai_ngam_mai_ngoi.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/12_jeonju_J11_dai_ngam_mai_ngoi.png | 4c7b955967a6db1439f4ee2cef1905ab |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png | 2cf0df9048ae3c04483427ac114f401e |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/14_gyeongju_G14_kho_ban_khac_di_tich.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/14_gyeongju_G14_kho_ban_khac_di_tich.png | 8007ee7ad206d924db38f7b6dba892c4 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/15_gyeongju_G17_dai_long_den_bo_tay.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/15_gyeongju_G17_dai_long_den_bo_tay.png | d57f4953075ec7340b53013023e3836e |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/16_seoul_S2_cong_thanh_xua_ben_song_han.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/16_seoul_S2_cong_thanh_xua_ben_song_han.png | 5de406e3254225c44942fb630633f2a3 |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/17_seoul_S10_chan_thap_n_seoul.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/17_seoul_S10_chan_thap_n_seoul.png | 3bc3df9f61adb74429d57747026c401f |
| Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/18_seoul_S18_role_anh_den_hanji.png | Assets/_Game/Art/UI/Postcards/20261018_RuntimeDerived/18_seoul_S18_role_anh_den_hanji.png | 506b1e7dde280494cbce489125f17063 |
| Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_DaLat_MistCompass_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_DaLat_MistCompass_Source.png | 5673f2c46b03183448ec8d5c8fb67cde |
| Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Gyeongju_StarlightLens_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Gyeongju_StarlightLens_Source.png | b1c38408f24cc5b4a84e45fff7d26a0f |
| Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_HoiAn_Lantern_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_HoiAn_Lantern_Source.png | 350907f3172eebd4c9340a84002a3501 |
| Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Hue_DragonSeal_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Hue_DragonSeal_Source.png | 623cab00f7d17024aa2f930ac3a1ae3d |
| Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Jeonju_HanjiSeal_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Jeonju_HanjiSeal_Source.png | c05ae4c2fe3fa6b49ac9787285d03f21 |
| Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Seoul_ThreeRelayPrism_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Seoul_ThreeRelayPrism_Source.png | 0b39cc9d42dc56640bc206a885645080 |
| Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png | f68243708771f8746a5f7a7cc75cc0f3 |
| Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_02_Environment_Architecture_Props.png | Assets/_Game/Art/Environment/HoiAn/H3/ArchitectureAtlas.png | 991ec0ec835754040ab8e94e9b86e8e7 |
| Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_05_UI_BlankPanels_Sheet.png | Assets/_Game/Art/UI/H3/BlankPanelsAtlas.png | 0ad78a1911d980f49b6dcd106132c0ae |
| Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png | Assets/_Game/Art/NPC/Shared/HoiAnNpcAtlas.png | ab62ba1e558f7274984500bb1cac18f6 |
| Assets/_Game/Art/_SourceSheets/ItemSheet_VNKR_FoodCulture_v1.png | Assets/_Game/Art/Items/Shared/ItemSheet_VNKR_FoodCulture_v1.png | 2cedb033602689a4d820165ed4b034c7 |
| Assets/_Game/Art/Characters/LBCFinalHotfix/Seoul_Curator_Transparent.png | Assets/_Game/Art/NPC/Seoul/Seoul_Curator_Transparent.png | f0c9bb7901be44e3a8bb69f9af1976dc |
| Assets/_Game/Art/Characters/LBCFinalHotfix/Seoul_Bridge_Bot_Banana.png | Assets/_Game/Art/Enemies/Seoul/Seoul_Bridge_Bot_Banana.png | 3c6b3beec4cc4e8db7d975a3b2fb0c5d |
| Assets/_Game/Art/Items/Collectibles/Seoul/Fragment_HeritageData.png | Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_HeritageData.png | 3d334be31874eca40a4cf79ba05a03b3 |
| Assets/_Game/Art/Items/Collectibles/Seoul/Fragment_JiwonSync.png | Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_JiwonSync.png | d263e3258cfe5a948b80272b7aac09fd |
| Assets/_Game/Art/Items/Collectibles/Seoul/Fragment_StellarRing.png | Assets/_Game/Art/Relics/Fragments/Seoul/Fragment_StellarRing.png | 37a459f297446c847b0742e1a74db5e2 |
| Assets/_Game/Art/SourceReferences/CulturalArtifacts/Artifact_DaLat_MistCompass_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_DaLat_MistCompass_Source.png | 5673f2c46b03183448ec8d5c8fb67cde |
| Assets/_Game/Art/SourceReferences/CulturalArtifacts/Artifact_Gyeongju_StarlightLens_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Gyeongju_StarlightLens_Source.png | b1c38408f24cc5b4a84e45fff7d26a0f |
| Assets/_Game/Art/SourceReferences/CulturalArtifacts/Artifact_HoiAn_Lantern_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_HoiAn_Lantern_Source.png | 350907f3172eebd4c9340a84002a3501 |
| Assets/_Game/Art/SourceReferences/CulturalArtifacts/Artifact_Hue_DragonSeal_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Hue_DragonSeal_Source.png | 623cab00f7d17024aa2f930ac3a1ae3d |
| Assets/_Game/Art/SourceReferences/CulturalArtifacts/Artifact_Jeonju_HanjiSeal_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Jeonju_HanjiSeal_Source.png | c05ae4c2fe3fa6b49ac9787285d03f21 |
| Assets/_Game/Art/SourceReferences/CulturalArtifacts/Artifact_Seoul_ThreeRelayPrism_Source.png | Assets/_Game/Art/Relics/Artifacts/Artifact_Seoul_ThreeRelayPrism_Source.png | 0b39cc9d42dc56640bc206a885645080 |
| Assets/_Game/Art/SourceReferences/Enemies_Roster_Concept.png | Assets/_Game/Art/Enemies/Shared/Enemies_Roster_Concept.png | f68243708771f8746a5f7a7cc75cc0f3 |
| Assets/_Game/Art/SourceReferences/ItemSheet_VNKR_FoodCulture_v1.png | Assets/_Game/Art/Items/Shared/ItemSheet_VNKR_FoodCulture_v1.png | 2cedb033602689a4d820165ed4b034c7 |
| Assets/_Game/Art/SourceReferences/H3_Upgrade_20261016/H3_02_Environment_Architecture_Props.png | Assets/_Game/Art/Environment/HoiAn/H3/ArchitectureAtlas.png | 991ec0ec835754040ab8e94e9b86e8e7 |
| Assets/_Game/Art/SourceReferences/H3_Upgrade_20261016/H3_05_UI_BlankPanels_Sheet.png | Assets/_Game/Art/UI/H3/BlankPanelsAtlas.png | 0ad78a1911d980f49b6dcd106132c0ae |
| Assets/_Game/Art/SourceReferences/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png | Assets/_Game/Art/NPC/Shared/HoiAnNpcAtlas.png | ab62ba1e558f7274984500bb1cac18f6 |

## Trùng pixels / nguồn sao lưu

Những nhóm dưới là bản có cùng hash. Không xóa tự động: nguồn bàn giao và bản runtime/importer có vai trò khác nhau.

- Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png; Tools/Import/SixCityRouteEditor/assets/da_lat.png; Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015/Maps/DaLat/Map_Overview_A.png
- Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png; Tools/Import/SixCityRouteEditor/assets/gyeongju.png; Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Gyeongju/Map_Overview_A.png
- Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png; Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png; Tools/Import/SixCityRouteEditor/assets/hoi_an.png; Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015/Maps/HoiAn/Map_Overview_A.png
- Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png; Tools/Import/SixCityRouteEditor/assets/jeonju.png; Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Jeonju/Map_Overview_A.png
- Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png; Tools/Import/SixCityRouteEditor/assets/seoul.png; Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Seoul/Map_Overview_A.png
- Assets/_Game/Art/UI/Markers/Navigation/node_available.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_available.png
- Assets/_Game/Art/UI/Markers/Navigation/node_current.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_current.png
- Assets/_Game/Art/UI/Markers/Navigation/node_enemy.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_enemy.png
- Assets/_Game/Art/UI/Markers/Navigation/node_item.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_item.png
- Assets/_Game/Art/UI/Markers/Navigation/node_locked.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_locked.png
- Assets/_Game/Art/UI/Markers/Navigation/node_npc.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_npc.png
- Assets/_Game/Art/UI/Markers/Navigation/node_portal.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_portal.png
- Assets/_Game/Art/UI/Markers/Navigation/node_puzzle.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_puzzle.png
- Assets/_Game/Art/UI/Markers/Navigation/node_quest.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_quest.png
- Assets/_Game/Art/UI/Markers/Navigation/node_secret.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_secret.png
- Assets/_Game/Art/UI/Markers/Navigation/node_visited.png; Tools/Import/Sources/LightBetweenCultures_DesignKit_v1/Assets/Markers/node_visited.png
- Tools/Import/SixCityRouteEditor/assets/hue.png; Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Hue/Map_Overview_A.png

## Tra cứu

[Manifest JSON](art-asset-manifest.json) · [CSV](art-asset-manifest.csv) · [GUID move log](migration-results.txt) · [Nguồn và canonical preview](ART_SOURCE_INDEX.md)

[Kiểm soát art trước khi mở rộng thành phố](CITY_EXPANSION_ART_CHECKLIST.md) · [Asset còn thiếu hiện hành](MISSING_ASSETS.md) · [Snapshot kiểm tra nguồn/import/binding](city-art-preflight.json)

Không còn tên PNG dạng UUID ở phạm vi ảnh game đã quét. Hai Composite_01/02 đã nhận diện là bảng tổng hợp và đổi tên có nghĩa, không đổi nội dung.
