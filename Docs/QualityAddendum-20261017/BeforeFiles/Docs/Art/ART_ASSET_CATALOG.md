# Thư viện art Light Between Cultures

Ảnh được tổ chức thực tế bằng Unity MoveAsset; không tạo bitmap trùng hoặc thay ảnh đã duyệt.

187 ảnh toàn project; 96 ảnh trong Art; **53 di chuyển/đổi tên, 43 giữ nguyên, 0 thao tác bị chặn**.

## Canonical và các khác biệt phải giữ rõ

- [NPC canonical identity / variants](NPC_CANONICAL_REGISTRY.md): một danh tính, nhiều biến thể. Cô Mai giữ khuôn mặt/tuổi trẻ tương đối; áo đỏ/nón/đèn là mặc định, không khóa mọi trang phục. Mai H3 và Mai áo vàng trẻ trong Composite_Illustrated là APPROVED_VARIANT (crop/concept có trạng thái riêng); bản tóc bạc lớn tuổi trong MissingSprites/Checker là IDENTITY_CONFLICT do tuổi, không do màu áo.
- Lumi có ba sprite riêng: map/dialogue/exploration. Huy hiệu cũ chỉ giữ để không làm hỏng prefab tương thích, không dùng trên City Map.
- Bot Mờ-01/crab/ghost dùng atlas đã nghiệm thu. Bot xanh/trắng trong sheet mới chưa được duyệt thay thế.
- Bà Tư, ông Phúc, Hương và So-yeon: hình hiện tại đã CANONICAL theo xác nhận 17/10/2026. Giữ native slices/portrait đã PASS; không tiếp tục chờ duyệt vì nhãn tạm cũ. Chất lượng crop, animation và biến thể mới là các tiêu chí riêng.
- Ớt/hành là icon flat tạm; bưu thiếp giữ JPG/credit và là minh họa chung. Derivative bưu thiếp lỗi ở References/NeedsReview không được đăng ký.
- Các itemId ánh xạ qua AdventureUIArt duy nhất; cùng ID dùng cùng Sprite ở mọi thành phố. Một số item dùng icon nhóm có cờ categoryIcon, không phải art riêng đã hoàn thiện.
- Sheet UI/nhân vật/môi trường/postcard mới có nền/chữ baked. FoodCulture có alpha thật nhưng cần kiểm tra từng crop trước khi dùng; chưa đăng ký món mới chỉ để đủ danh sách.
- Atlas dùng chung vẫn giữ một bitmap và các sprite con, không nhân bản atlas vào nhiều nhóm thư mục.
- Hai ảnh Hội An cùng pixels có importer khác nhau (map chính và preview mip/1024); giữ cả hai GUID để bảo toàn chất lượng/tham chiếu. Các bản LBC/DesignKit gốc ngoài Assets không di chuyển.

## Asset runtime / sprite đã đăng ký

| Tên | Vai trò / ID | Đường dẫn | Canonical | Vấn đề |
|---|---|---|---|---|
| Lumi_CityMarker_128.png | lumiCitySmall | [Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_128.png](../../Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_128.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| Lumi_CityMarker_256.png | lumiCity, Assets/_Game/Prefabs\DesignKit\LumiMapToken.prefab | [Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_256.png](../../Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_256.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| Characters_MissingSprites_v1.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png](../../Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; Review identity per character/region in NPC_CANONICAL_REGISTRY; costume changes alone are not conflicts; concept is not a runtime sprite |
| Checkin_Postcards_VNKR_v1.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/Checkin_Postcards_VNKR_v1.png](../../Assets/_Game/Art/_SourceSheets/Checkin_Postcards_VNKR_v1.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite |
| enemy_fog_boss-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/enemy_fog_boss-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/enemy_fog_boss-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| enemy_glass_thief-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/enemy_glass_thief-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/enemy_glass_thief-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| enemy_mist_wolf-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/enemy_mist_wolf-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/enemy_mist_wolf-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| npc_gardener-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_gardener-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_gardener-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| npc_hungry_dog-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_hungry_dog-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_hungry_dog-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| npc_lan-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_lan-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_lan-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| npc_thuy-draft.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_thuy-draft.png](../../Assets/_Game/Art/_SourceSheets/DaLatRejectedDrafts/npc_thuy-draft.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| Enemies_Roster_Concept.png | enemy_stone_guard, enemy_star_core, enemy_final_core, enemy_shadow_courier, enemy_fox | [Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png](../../Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; PROPOSED / TEMPORARY bindings, not canonical/final: enemy_stone_guard, enemy_star_core, enemy_final_core, enemy_shadow_courier, enemy_fox |
| H3_01_VisualTarget_Reference.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_01_VisualTarget_Reference.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_01_VisualTarget_Reference.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite |
| H3_02_Environment_Architecture_Props.png | Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_02_Environment_Architecture_Props.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_02_Environment_Architecture_Props.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| H3_03_UI_TextBaked_Reference.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_03_UI_TextBaked_Reference.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_03_UI_TextBaked_Reference.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| H3_04_Lantern_LightFX_Concept.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_04_Lantern_LightFX_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_04_Lantern_LightFX_Concept.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| H3_05_UI_BlankPanels_Sheet.png | Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_05_UI_BlankPanels_Sheet.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_05_UI_BlankPanels_Sheet.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| H3_06_Markers_Interactables_Concept.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_06_Markers_Interactables_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_06_Markers_Interactables_Concept.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| H3_07_Characters_NPC_Concept.png | npc_ba_tu, npc_phuc | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png) | Có | transparent sheet; individual crop/padding requires review before registration |
| HoiAn_H3_Environment_v1.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/HoiAn_H3_Environment_v1.png](../../Assets/_Game/Art/_SourceSheets/HoiAn_H3_Environment_v1.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite |
| ItemSheet_VNKR_FoodCulture_v1.png | da_lat_strawberry | [Assets/_Game/Art/_SourceSheets/ItemSheet_VNKR_FoodCulture_v1.png](../../Assets/_Game/Art/_SourceSheets/ItemSheet_VNKR_FoodCulture_v1.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration; PROPOSED / TEMPORARY bindings, not canonical/final: da_lat_strawberry |
| Item_HoiAn_Chili.jpg | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/ItemsOriginals/Item_HoiAn_Chili.jpg](../../Assets/_Game/Art/_SourceSheets/ItemsOriginals/Item_HoiAn_Chili.jpg) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; flat graphic temporary style; differs from painted LBC icons |
| Item_HoiAn_Shallot.jpg | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/ItemsOriginals/Item_HoiAn_Shallot.jpg](../../Assets/_Game/Art/_SourceSheets/ItemsOriginals/Item_HoiAn_Shallot.jpg) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; flat graphic temporary style; differs from painted LBC icons |
| LBC_Composite_CheckerConcept_v2.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png](../../Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; Review identity per character/region in NPC_CANONICAL_REGISTRY; costume changes alone are not conflicts; concept is not a runtime sprite |
| LBC_Composite_Illustrated_v1.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/LBC_Composite_Illustrated_v1.png](../../Assets/_Game/Art/_SourceSheets/LBC_Composite_Illustrated_v1.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite; Review identity per character/region in NPC_CANONICAL_REGISTRY; costume changes alone are not conflicts; concept is not a runtime sprite |
| Lumi_Character_Cutout_Concept.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/Lumi/Lumi_Character_Cutout_Concept.png](../../Assets/_Game/Art/_SourceSheets/Lumi/Lumi_Character_Cutout_Concept.png) | Không / chưa duyệt | transparent sheet; individual crop/padding requires review before registration |
| UI_Adventure_Kit_v1.png | source / compatibility / native sprite | [Assets/_Game/Art/_SourceSheets/UI_Adventure_Kit_v1.png](../../Assets/_Game/Art/_SourceSheets/UI_Adventure_Kit_v1.png) | Không / chưa duyệt | background/labels baked into concept; not an isolated sprite |
| enemy_fog_boss.png | enemy_fog_boss | [Assets/_Game/Art/Characters/Enemies/DaLat/enemy_fog_boss.png](../../Assets/_Game/Art/Characters/Enemies/DaLat/enemy_fog_boss.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_fog_boss |
| enemy_glass_thief.png | enemy_glass_thief, enemy_drone | [Assets/_Game/Art/Characters/Enemies/DaLat/enemy_glass_thief.png](../../Assets/_Game/Art/Characters/Enemies/DaLat/enemy_glass_thief.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_glass_thief, enemy_drone |
| enemy_mist_wolf.png | enemy_mist_wolf | [Assets/_Game/Art/Characters/Enemies/DaLat/enemy_mist_wolf.png](../../Assets/_Game/Art/Characters/Enemies/DaLat/enemy_mist_wolf.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_mist_wolf |
| HoiAn_Enemies_Atlas.png | enemy_crab, enemy_ghost, enemy_mo01, enemy_bridge | [Assets/_Game/Art/Characters/Enemies/HoiAn_Enemies_Atlas.png](../../Assets/_Game/Art/Characters/Enemies/HoiAn_Enemies_Atlas.png) | Hỗn hợp / xem từng sprite | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_bridge |
| enemy_archive_guard.png | enemy_archive_guard | [Assets/_Game/Art/Characters/Enemies/Hue/enemy_archive_guard.png](../../Assets/_Game/Art/Characters/Enemies/Hue/enemy_archive_guard.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_archive_guard |
| enemy_courier.png | enemy_courier | [Assets/_Game/Art/Characters/Enemies/Hue/enemy_courier.png](../../Assets/_Game/Art/Characters/Enemies/Hue/enemy_courier.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_courier |
| enemy_gate_guard.png | enemy_gate_guard, enemy_night_guard | [Assets/_Game/Art/Characters/Enemies/Hue/enemy_gate_guard.png](../../Assets/_Game/Art/Characters/Enemies/Hue/enemy_gate_guard.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_gate_guard, enemy_night_guard |
| enemy_river_wisp.png | enemy_river_wisp | [Assets/_Game/Art/Characters/Enemies/Hue/enemy_river_wisp.png](../../Assets/_Game/Art/Characters/Enemies/Hue/enemy_river_wisp.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_river_wisp |
| enemy_crane_boss.png | enemy_crane_boss | [Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_crane_boss.png](../../Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_crane_boss.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_crane_boss |
| enemy_paper_crane.png | enemy_paper_crane | [Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_paper_crane.png](../../Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_paper_crane.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_paper_crane |
| enemy_parcel_bot.png | enemy_parcel_bot | [Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_parcel_bot.png](../../Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_parcel_bot.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_parcel_bot |
| enemy_pattern_bot.png | enemy_pattern_bot, enemy_signal_bot | [Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_pattern_bot.png](../../Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_pattern_bot.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: enemy_pattern_bot, enemy_signal_bot |
| Lumi_DialoguePortrait.png | dialogue, lumiPortrait | [Assets/_Game/Art/Characters/Lumi/Lumi_DialoguePortrait.png](../../Assets/_Game/Art/Characters/Lumi/Lumi_DialoguePortrait.png) | Có |  |
| Lumi_ExplorationIdle.png | exploration, lumiExploration, Assets/_Game/Scenes\HoiAnDemo.unity, Assets/_Game/Prefabs\DesignKit\LumiExplorationSprite.prefab | [Assets/_Game/Art/Characters/Lumi/Lumi_ExplorationIdle.png](../../Assets/_Game/Art/Characters/Lumi/Lumi_ExplorationIdle.png) | Có |  |
| Lumi_MapTraveler.png | map, lumiMapTraveler | [Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png](../../Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png) | Có |  |
| npc_gardener.png | npc_gardener | [Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_gardener |
| npc_hungry_dog.png | npc_hungry_dog | [Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_hungry_dog |
| npc_lan.png | npc_lan | [Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_lan |
| npc_thuy.png | npc_thuy | [Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_thuy |
| NPC_HoiAn_Boatman.png | npc_boatman | [Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png](../../Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png) | Có |  |
| NPC_HoiAn_CoMai_Portrait.png | npc_mai, mai, maiPortrait | [Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png](../../Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png) | Có |  |
| NPC_HoiAn_MarketMerchant.png | npc_market_merchant | [Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png](../../Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png) | Có |  |
| npc_nguyet.png | npc_nguyet | [Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png](../../Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_nguyet |
| npc_tam.png | npc_tam | [Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png](../../Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_tam |
| npc_elder.png | npc_elder | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_elder |
| npc_park.png | npc_park | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_park |
| npc_yoon.png | npc_yoon | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png) | Không / chưa duyệt | PROPOSED / TEMPORARY bindings, not canonical/final: npc_yoon |
| NPC_TransparentAtlas.png | npc_guest, npc_huong, npc_soyeon, npc_scholar, npc_hana, npc_min | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png) | Hỗn hợp / xem từng sprite | PROPOSED / TEMPORARY bindings, not canonical/final: npc_scholar, npc_hana, npc_min |
| Item_Food_Postcard.jpg | festival_friendship_postcard, hoi_an_bridge_postcard | [Assets/_Game/Art/Checkin/Item_Food_Postcard.jpg](../../Assets/_Game/Art/Checkin/Item_Food_Postcard.jpg) | Không / chưa duyệt | original illustrative photo/credit/pink background retained; not bridge-specific art |
| UnitSprite.png | Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Environment/Common/UnitSprite.png](../../Assets/_Game/Art/Environment/Common/UnitSprite.png) | Không / chưa duyệt |  |
| H3_StoneGround_Continuous_v1.png | H3_StoneGround_Continuous_v1, Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Environment/HoiAn/H3/H3_StoneGround_Continuous_v1.png](../../Assets/_Game/Art/Environment/HoiAn/H3/H3_StoneGround_Continuous_v1.png) | Không / chưa duyệt |  |
| Item_Healing_Tea.png | healing_tea | [Assets/_Game/Art/Items/Consumables/Item_Healing_Tea.png](../../Assets/_Game/Art/Items/Consumables/Item_Healing_Tea.png) | Có |  |
| Item_HoiAn_Chili.png | hoi_an_fresh_chili | [Assets/_Game/Art/Items/Food/Item_HoiAn_Chili.png](../../Assets/_Game/Art/Items/Food/Item_HoiAn_Chili.png) | Có | flat graphic temporary style; differs from painted LBC icons |
| Item_HoiAn_Shallot.png | hoi_an_purple_shallot | [Assets/_Game/Art/Items/Food/Item_HoiAn_Shallot.png](../../Assets/_Game/Art/Items/Food/Item_HoiAn_Shallot.png) | Có | flat graphic temporary style; differs from painted LBC icons |
| Item_Copper_Wire.png | copper_wire | [Assets/_Game/Art/Items/Materials/Item_Copper_Wire.png](../../Assets/_Game/Art/Items/Materials/Item_Copper_Wire.png) | Có |  |
| Item_Hanji_Core.png | hanji_core | [Assets/_Game/Art/Items/Quest/Item_Hanji_Core.png](../../Assets/_Game/Art/Items/Quest/Item_Hanji_Core.png) | Có |  |
| Interactables_Atlas.png | lamp_eye, lantern-part, link_key, coin, lan_glasses, meat_portion, night_flower, silla_prism, stargazer_lens, lantern_stamp, river_pass, royal_herb, lost_letter, ancient_seal, astral_script, fog_battery, blue_water_signal, hanji_pattern_1, hanji_pattern_2, hanji_pattern_3, missing_parcel, hanji_circuit, reply_letter, star_symbol_1, star_symbol_2, star_symbol_3, relay_a, relay_b, relay_c, jeonju_recipe_card, hue_market_note, Assets/_Game/Scenes\HoiAnDemo.unity | [Assets/_Game/Art/Items/Shared/Interactables_Atlas.png](../../Assets/_Game/Art/Items/Shared/Interactables_Atlas.png) | Có |  |
| Map_DaLat_DestinationPreview_A.png | da_lat | [Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png) | Có |  |
| Map_Gyeongju_DestinationPreview_A.png | source / compatibility / native sprite | [Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_DestinationPreview_A.png) | Không / chưa duyệt |  |
| Map_Gyeongju_Overview_A.png | gyeongju | [Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png](../../Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png) | Có |  |
| Map_HoiAn_DestinationPreview_A.png | hoi_an | [Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png) | Có |  |
| Map_HoiAn_Overview_A.png | hoi_an, hoiAnMap | [Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png](../../Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png) | Có |  |
| Map_Hue_DestinationPreview_A.png | hue | [Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png) | Có |  |
| Map_Jeonju_DestinationPreview_A.png | jeonju | [Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png) | Có |  |
| Map_Seoul_DestinationPreview_A.png | source / compatibility / native sprite | [Assets/_Game/Art/Maps/Seoul/Map_Seoul_DestinationPreview_A.png](../../Assets/_Game/Art/Maps/Seoul/Map_Seoul_DestinationPreview_A.png) | Không / chưa duyệt |  |
| Map_Seoul_Overview_A.png | seoul | [Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png](../../Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png) | Có |  |
| node_available.png | available | [Assets/_Game/Art/Markers/LegacyDesignKit/node_available.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_available.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_current.png | current | [Assets/_Game/Art/Markers/LegacyDesignKit/node_current.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_current.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_enemy.png | enemy | [Assets/_Game/Art/Markers/LegacyDesignKit/node_enemy.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_enemy.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_item.png | item, Assets/_Game/Scenes\HoiAnDemo.unity, Assets/_Game/Prefabs\DesignKit\LanternPartMarker.prefab | [Assets/_Game/Art/Markers/LegacyDesignKit/node_item.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_item.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_locked.png | locked | [Assets/_Game/Art/Markers/LegacyDesignKit/node_locked.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_locked.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_npc.png | npc, Assets/_Game/Scenes\HoiAnDemo.unity, Assets/_Game/Prefabs\DesignKit\NpcMarker.prefab | [Assets/_Game/Art/Markers/LegacyDesignKit/node_npc.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_npc.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_portal.png | portal | [Assets/_Game/Art/Markers/LegacyDesignKit/node_portal.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_portal.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_puzzle.png | puzzle | [Assets/_Game/Art/Markers/LegacyDesignKit/node_puzzle.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_puzzle.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_quest.png | quest | [Assets/_Game/Art/Markers/LegacyDesignKit/node_quest.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_quest.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_secret.png | secret | [Assets/_Game/Art/Markers/LegacyDesignKit/node_secret.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_secret.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| node_visited.png | visited | [Assets/_Game/Art/Markers/LegacyDesignKit/node_visited.png](../../Assets/_Game/Art/Markers/LegacyDesignKit/node_visited.png) | Không / chưa duyệt | legacy compatibility art; not the active Lumi map role |
| Marker_BossAlert.png | BossAlert | [Assets/_Game/Art/Markers/Marker_BossAlert.png](../../Assets/_Game/Art/Markers/Marker_BossAlert.png) | Có |  |
| Marker_Camera.png | Camera | [Assets/_Game/Art/Markers/Marker_Camera.png](../../Assets/_Game/Art/Markers/Marker_Camera.png) | Có |  |
| Marker_Completed.png | Completed | [Assets/_Game/Art/Markers/Marker_Completed.png](../../Assets/_Game/Art/Markers/Marker_Completed.png) | Có |  |
| Marker_Current.png | Current | [Assets/_Game/Art/Markers/Marker_Current.png](../../Assets/_Game/Art/Markers/Marker_Current.png) | Có |  |
| Marker_Ground.png | Ground | [Assets/_Game/Art/Markers/Marker_Ground.png](../../Assets/_Game/Art/Markers/Marker_Ground.png) | Có |  |
| Marker_NpcNeedItem.png | NpcNeedItem | [Assets/_Game/Art/Markers/Marker_NpcNeedItem.png](../../Assets/_Game/Art/Markers/Marker_NpcNeedItem.png) | Có |  |
| Marker_NpcTalk.png | NpcTalk | [Assets/_Game/Art/Markers/Marker_NpcTalk.png](../../Assets/_Game/Art/Markers/Marker_NpcTalk.png) | Có |  |
| Marker_Puzzle.png | Puzzle | [Assets/_Game/Art/Markers/Marker_Puzzle.png](../../Assets/_Game/Art/Markers/Marker_Puzzle.png) | Có |  |
| Marker_Quest.png | Quest | [Assets/_Game/Art/Markers/Marker_Quest.png](../../Assets/_Game/Art/Markers/Marker_Quest.png) | Có |  |
| Marker_Reward.png | Reward | [Assets/_Game/Art/Markers/Marker_Reward.png](../../Assets/_Game/Art/Markers/Marker_Reward.png) | Có |  |
| UI_World_Globe.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/Icons/UI_World_Globe.png](../../Assets/_Game/Art/UI/Icons/UI_World_Globe.png) | Không / chưa duyệt |  |
| LBC_UI_SkinAtlas.png | source / compatibility / native sprite | [Assets/_Game/Art/UI/Panels/LBC_UI_SkinAtlas.png](../../Assets/_Game/Art/UI/Panels/LBC_UI_SkinAtlas.png) | Không / chưa duyệt |  |

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
| npc_scholar | npc | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png / npc_scholar | EXISTING_ROSTER_ROLE_MATCH; NOT_CANONICAL; named_identity_assignment_pending_approval |
| npc_hana | npc | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png / npc_hana | EXISTING_ROSTER_ROLE_MATCH; NOT_CANONICAL; named_identity_assignment_pending_approval |
| npc_nguyet | npc | Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png / npc_nguyet | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_tam | npc | Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png / npc_tam | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_yoon | npc | Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png / npc_yoon | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_park | npc | Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png / npc_park | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_elder | npc | Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png / npc_elder | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| npc_lan | npc | Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png / npc_lan | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_thuy | npc | Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png / npc_thuy | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_gardener | npc | Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png / npc_gardener | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_hungry_dog | npc | Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png / npc_hungry_dog | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| npc_min | npc | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png / npc_min | TEMPORARY_ROLE_MATCH_FROM_UNASSIGNED_ARCHAEOLOGICAL_GUIDE; NOT_CANONICAL_IDENTITY_APPROVAL_PENDING |
| enemy_river_wisp | enemy | Assets/_Game/Art/Characters/Enemies/Hue/enemy_river_wisp.png / enemy_river_wisp | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_courier | enemy | Assets/_Game/Art/Characters/Enemies/Hue/enemy_courier.png / enemy_courier | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_archive_guard | enemy | Assets/_Game/Art/Characters/Enemies/Hue/enemy_archive_guard.png / enemy_archive_guard | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_gate_guard | enemy | Assets/_Game/Art/Characters/Enemies/Hue/enemy_gate_guard.png / enemy_gate_guard | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_parcel_bot | enemy | Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_parcel_bot.png / enemy_parcel_bot | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_paper_crane | enemy | Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_paper_crane.png / enemy_paper_crane | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_pattern_bot | enemy | Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_pattern_bot.png / enemy_pattern_bot | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_crane_boss | enemy | Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_crane_boss.png / enemy_crane_boss | AI_DERIVATIVE_PROPOSAL; NOT_CANONICAL; static_cutout_not_animation |
| enemy_glass_thief | enemy | Assets/_Game/Art/Characters/Enemies/DaLat/enemy_glass_thief.png / enemy_glass_thief | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| enemy_fog_boss | enemy | Assets/_Game/Art/Characters/Enemies/DaLat/enemy_fog_boss.png / enemy_fog_boss | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| enemy_mist_wolf | enemy | Assets/_Game/Art/Characters/Enemies/DaLat/enemy_mist_wolf.png / enemy_mist_wolf | AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE |
| enemy_stone_guard | enemy | Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png / stone_guard | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance |
| enemy_star_core | enemy | Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png / energy_core | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance |
| enemy_final_core | enemy | Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png / energy_core | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance |
| enemy_shadow_courier | enemy | Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png / shadow_courier | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance |
| enemy_fox | enemy | Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png / shadow_cat | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; cat concept pending actual fox sprite |
| enemy_night_guard | enemy | Assets/_Game/Art/Characters/Enemies/Hue/enemy_gate_guard.png / enemy_gate_guard | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_gate_guard |
| enemy_bridge | enemy | Assets/_Game/Art/Characters/Enemies/HoiAn_Enemies_Atlas.png / enemy_mo01 | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_mo01 |
| enemy_drone | enemy | Assets/_Game/Art/Characters/Enemies/DaLat/enemy_glass_thief.png / enemy_glass_thief | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_glass_thief |
| enemy_signal_bot | enemy | Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_pattern_bot.png / enemy_pattern_bot | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_pattern_bot |

## Di chuyển thực tế

| Cũ | Mới | GUID |
|---|---|---|
| Assets/_Game/Art/_SourceSheets/Concept_Composite_01.png | Assets/_Game/Art/_SourceSheets/LBC_Composite_Illustrated_v1.png | e2ee55c326f7e2e4a86e7d19b579f740 |
| Assets/_Game/Art/_SourceSheets/Concept_Composite_02.png | Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png | 4ecfe5a65b714bf49b2f39c4788e47d4 |
| Assets/_Game/Art/DesignKit/Lumi/Lumi_Character_Cutout_Concept.png | Assets/_Game/Art/_SourceSheets/Lumi/Lumi_Character_Cutout_Concept.png | 7365c571dda3a8245b7afd2289fd8d2c |
| Assets/_Game/Art/DesignKit/Lumi/Lumi_CityMarker_128.png | Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_128.png | d2181a9171421b34780d442db99658c2 |
| Assets/_Game/Art/DesignKit/Lumi/Lumi_CityMarker_256.png | Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_256.png | 3d4c838d288e1af438ef1a3e538d8364 |
| Assets/_Game/Art/DesignKit/Markers/node_available.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_available.png | 4a9f6b4446c499848bb9d3d1e60ec600 |
| Assets/_Game/Art/DesignKit/Markers/node_current.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_current.png | 0e46fa3a4a730ff4da2af78097a0e7b8 |
| Assets/_Game/Art/DesignKit/Markers/node_enemy.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_enemy.png | ddbeae466d78e5941af1f2d58373540d |
| Assets/_Game/Art/DesignKit/Markers/node_item.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_item.png | 8d87ce1f1365b4545931249b25505c95 |
| Assets/_Game/Art/DesignKit/Markers/node_locked.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_locked.png | fae34817a0d319145baf618ff62d512a |
| Assets/_Game/Art/DesignKit/Markers/node_npc.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_npc.png | e5c90b4c7bb2cc849a556a6729d24107 |
| Assets/_Game/Art/DesignKit/Markers/node_portal.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_portal.png | c0307cd2431a2754b940fbec57653e10 |
| Assets/_Game/Art/DesignKit/Markers/node_puzzle.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_puzzle.png | 41b4328d91767bc459398185aaf68569 |
| Assets/_Game/Art/DesignKit/Markers/node_quest.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_quest.png | ba53cdd81d3eca04eb7229ec95501c1c |
| Assets/_Game/Art/DesignKit/Markers/node_secret.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_secret.png | e8513865616e8b644a97488406359aac |
| Assets/_Game/Art/DesignKit/Markers/node_visited.png | Assets/_Game/Art/Markers/LegacyDesignKit/node_visited.png | 66d590fef2a744d4baca23050d763ecc |
| Assets/_Game/Art/Generated/placeholder.png | Assets/_Game/Art/Environment/Common/UnitSprite.png | 3ce7b373394f4034ca0e0dcd40b1d45a |
| Assets/_Game/Art/Item/Derived/Item_HoiAn_Chili.png | Assets/_Game/Art/Items/Food/Item_HoiAn_Chili.png | 9ca8b1632a045ca419f9b8e28bbd201f |
| Assets/_Game/Art/Item/Derived/Item_HoiAn_Shallot.png | Assets/_Game/Art/Items/Food/Item_HoiAn_Shallot.png | 265c03f325521254a8dff9674cc2b3f7 |
| Assets/_Game/Art/Item/Item_Food_Postcard.jpg | Assets/_Game/Art/Checkin/Item_Food_Postcard.jpg | 881b60dcbeb91924496dc81728ca1d35 |
| Assets/_Game/Art/Item/Item_HoiAn_Chili.jpg | Assets/_Game/Art/_SourceSheets/ItemsOriginals/Item_HoiAn_Chili.jpg | f3f2cf00566b14d468e4402e3ef88afc |
| Assets/_Game/Art/Item/Item_HoiAn_Shallot.jpg | Assets/_Game/Art/_SourceSheets/ItemsOriginals/Item_HoiAn_Shallot.jpg | 841a00003e05ef7419b14bd20833d540 |
| Assets/_Game/Art/Runtime/AdventureUI/Boatman_Portrait.png | Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png | 3835aa72bd8c88b41af32d4aabd40b98 |
| Assets/_Game/Art/Runtime/AdventureUI/City_da_lat.png | Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png | 44c553d406b840740a51a62491818111 |
| Assets/_Game/Art/Runtime/AdventureUI/City_gyeongju.png | Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_DestinationPreview_A.png | f1f6e381ad05edb4da03ce1c821476b0 |
| Assets/_Game/Art/Runtime/AdventureUI/City_hoi_an.png | Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png | add0499b1e25b674f97625fcc443e3e6 |
| Assets/_Game/Art/Runtime/AdventureUI/City_hue.png | Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png | 481d167093be364419d9ebb11b8e4e27 |
| Assets/_Game/Art/Runtime/AdventureUI/City_jeonju.png | Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png | a81827839b4593840a4f9ed1ebc59f9d |
| Assets/_Game/Art/Runtime/AdventureUI/City_seoul.png | Assets/_Game/Art/Maps/Seoul/Map_Seoul_DestinationPreview_A.png | 3f3e71d2ddb346442bd9f232ce09d9b4 |
| Assets/_Game/Art/Runtime/AdventureUI/Copper_Wire.png | Assets/_Game/Art/Items/Materials/Item_Copper_Wire.png | 6292bac9748f22042aff4a6e8a1bcfbf |
| Assets/_Game/Art/Runtime/AdventureUI/Enemies_HoiAn_TransparentAtlas.png | Assets/_Game/Art/Characters/Enemies/HoiAn_Enemies_Atlas.png | 0aa7f368b0015354e9303a88022a2b51 |
| Assets/_Game/Art/Runtime/AdventureUI/Hanji_Core.png | Assets/_Game/Art/Items/Quest/Item_Hanji_Core.png | 9c6ac872d96c048409e5f98912531ebf |
| Assets/_Game/Art/Runtime/AdventureUI/Healing_Tea.png | Assets/_Game/Art/Items/Consumables/Item_Healing_Tea.png | 589849a96242ac74fb324aa67d104496 |
| Assets/_Game/Art/Runtime/AdventureUI/Items_TransparentAtlas.png | Assets/_Game/Art/Items/Shared/Interactables_Atlas.png | b0d1662050b200c4e94c139bfde08b33 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/BossAlert.png | Assets/_Game/Art/Markers/Marker_BossAlert.png | 93c43af33a033414dbbcc8c6a2674d14 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Camera.png | Assets/_Game/Art/Markers/Marker_Camera.png | e6ff7bf47d518024b9b5617581a369ef |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Completed.png | Assets/_Game/Art/Markers/Marker_Completed.png | e58a9cea55b466a419729d26936f1b8c |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Current.png | Assets/_Game/Art/Markers/Marker_Current.png | 39d25111b5b7f4e48b132c979ba458d8 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Ground.png | Assets/_Game/Art/Markers/Marker_Ground.png | eddf12bed32f6ac47ad4a5ab8ad7f0df |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/NpcNeedItem.png | Assets/_Game/Art/Markers/Marker_NpcNeedItem.png | fc55f8d3f64666c40b089a773f4f4a05 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/NpcTalk.png | Assets/_Game/Art/Markers/Marker_NpcTalk.png | 7d6c6ca2f3bb88444969f4378d8f38b6 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Puzzle.png | Assets/_Game/Art/Markers/Marker_Puzzle.png | a18d72cffa21b024d840d7bef6462af9 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Quest.png | Assets/_Game/Art/Markers/Marker_Quest.png | a594009578f391d4eb1ae20fa8d423c4 |
| Assets/_Game/Art/Runtime/AdventureUI/Markers/Reward.png | Assets/_Game/Art/Markers/Marker_Reward.png | cc35585804661c949a22a136240db6d5 |
| Assets/_Game/Art/Runtime/AdventureUI/Market_Merchant.png | Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png | 0a91b2dbca846994bbade4807ff62596 |
| Assets/_Game/Art/Runtime/AdventureUI/NPC_TransparentAtlas.png | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png | c6176fa957f7eb549b9b40703e5038ea |
| Assets/_Game/Art/Runtime/AdventureUI/Theme/LBC_UI_SkinAtlas.png | Assets/_Game/Art/UI/Panels/LBC_UI_SkinAtlas.png | bf36cd9e4ac7cfd409810a756c4dfb99 |
| Assets/_Game/Art/Runtime/AdventureUI/Theme/World_Globe.png | Assets/_Game/Art/UI/Icons/UI_World_Globe.png | a56225f02ce31104daf479ecd89eaea8 |
| Assets/_Game/Art/Runtime/CityMaps/HoiAn_Map_Overview_A.png | Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png | 9f457fcf0b8450940a9493323d9febcc |
| Assets/_Game/Art/Runtime/Lumi/Lumi_DialoguePortrait.png | Assets/_Game/Art/Characters/Lumi/Lumi_DialoguePortrait.png | 1749b1933916d2b4185622c2165926be |
| Assets/_Game/Art/Runtime/Lumi/Lumi_ExplorationIdle.png | Assets/_Game/Art/Characters/Lumi/Lumi_ExplorationIdle.png | 7644234593d8df64f8cad7f5cf5860b4 |
| Assets/_Game/Art/Runtime/Lumi/Lumi_MapTraveler.png | Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png | 9a7d3aad847598b419b6b01ab212a053 |
| Assets/_Game/Art/Runtime/NPCs/Mai_DialoguePortrait.png | Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png | a6cc838dd76ae7f40902d846693f4a5e |

## Trùng pixels / nguồn sao lưu

Những nhóm dưới là bản có cùng hash. Không xóa tự động: nguồn bàn giao và bản runtime/importer có vai trò khác nhau.

- Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_128.png; LightBetweenCultures_DesignKit_v1/Assets/Lumi/Lumi_CityMarker_128.png
- Assets/_Game/Art/_References/LegacyLumi/Lumi_CityMarker_256.png; LightBetweenCultures_DesignKit_v1/Assets/Lumi/Lumi_CityMarker_256.png
- Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png; LBC_AnhMoi_BanGiaoCodex_20261015/Characters/Enemies_Roster_Concept.png
- Assets/_Game/Art/_SourceSheets/Lumi/Lumi_Character_Cutout_Concept.png; Assets/_Game/Art/Characters/Lumi/Lumi_DialoguePortrait.png; LightBetweenCultures_DesignKit_v1/Assets/Lumi/Lumi_Character_Cutout_Concept.png
- Assets/_Game/Art/Maps/DaLat/Map_DaLat_DestinationPreview_A.png; LBC_AnhMoi_BanGiaoCodex_20261015/Maps/DaLat/Map_Overview_A.png
- Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_DestinationPreview_A.png; Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png; LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Gyeongju/Map_Overview_A.png
- Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_DestinationPreview_A.png; Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png; LBC_AnhMoi_BanGiaoCodex_20261015/Maps/HoiAn/Map_Overview_A.png
- Assets/_Game/Art/Maps/Hue/Map_Hue_DestinationPreview_A.png; LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Hue/Map_Overview_A.png
- Assets/_Game/Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png; LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Jeonju/Map_Overview_A.png
- Assets/_Game/Art/Maps/Seoul/Map_Seoul_DestinationPreview_A.png; Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png; LBC_AnhMoi_BanGiaoCodex_20261015/Maps/Seoul/Map_Overview_A.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_available.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_available.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_current.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_current.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_enemy.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_enemy.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_item.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_item.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_locked.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_locked.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_npc.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_npc.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_portal.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_portal.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_puzzle.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_puzzle.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_quest.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_quest.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_secret.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_secret.png
- Assets/_Game/Art/Markers/LegacyDesignKit/node_visited.png; LightBetweenCultures_DesignKit_v1/Assets/Markers/node_visited.png

## Tra cứu

[Manifest JSON](art-asset-manifest.json) · [CSV](art-asset-manifest.csv) · [GUID move log](migration-results.txt) · [Nguồn và canonical preview](ART_SOURCE_INDEX.md)

[Kiểm soát art trước khi mở rộng thành phố](CITY_EXPANSION_ART_CHECKLIST.md) · [Asset còn thiếu hiện hành](MISSING_ASSETS.md) · [Snapshot kiểm tra nguồn/import/binding](city-art-preflight.json)

Không còn tên PNG dạng UUID ở phạm vi ảnh game đã quét. Hai Composite_01/02 đã nhận diện là bảng tổng hợp và đổi tên có nghĩa, không đổi nội dung.
