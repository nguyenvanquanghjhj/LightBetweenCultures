# Six Relics source mapping — evidence and art aliases

2026-10-18 · Phase0 AUDIT/DESIGN ONLY. Gameplay placement authority: [ACCESS_MATRIX](ARTIFACT_FRAGMENT_ACCESS_MATRIX.md), SOURCE_MATRIX chỉ view. Đây là mapping **art/source/proof**, không collection runtime thứ hai.

## Hiện trạng được xác minh

Runtime `GameplayContent` giữ 6 city,133 node,29 quest,22 puzzle,23 enemy. Không có `relic_*`/`artifact_*` collection state/registry trong Scripts/Core/Content/Maps hoặc Content JSON; không có six restoration trial/event/ending guard. Chưa có stable artifact runtime save cần migration alias. Do đó dùng sáu relicId mới từ Master cho thiết kế; không thêm vào Resources trong Phase0. Blueprint chứa24 fragmentId globally unique, chưa là item IDs/saved flags thật.

Sáu PNG tại `_SourceSheets/CulturalArtifacts` **đã có thật, đều1254×1254 RGBA alpha0–255**; mỗi ảnh nhìn thấy1 relic lớn +4 components ở dưới. **0/30 native runtime sprites từ bộ này** vì6 ảnh chưa có `.meta`, chưa import/bind/crop; không gọi whole sheet là sprite runtime. Bốn NPC mới1086×1448 cũng chưa meta/binding. [Hash/alphaBBox](Phase0-20261018/new-art-source-audit.json), [mở ảnh nguồn](Phase0-20261018/SourceReview.html).

## Alias art vs gameplay

| Canonical relicId | Alias ảnh (không gameplay ID thứ hai) | Source PNG | Component labels đối chiếu (theo thứ tự fragment canonical) | Review |
|---|---|---|---|---|
| relic_hoi_an_lantern | artifact_hoi_an_lantern | Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_HoiAn_Lantern_Source.png | artifact_hoi_an_bamboo_frame, artifact_hoi_an_craft_paper, artifact_hoi_an_roof_hook, artifact_hoi_an_reflection_core | Đối chiếu visual sơ bộ hợp nghĩa; crop/import/approval còn pending |
| relic_hue_dragon_seal | artifact_hue_dragon_seal | Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Hue_DragonSeal_Source.png | artifact_hue_perfume_river_core, artifact_hue_dragon_face, artifact_hue_ritual_cord, artifact_hue_rhythm_inscription | Đối chiếu visual sơ bộ hợp nghĩa; crop/import/approval còn pending |
| relic_dalat_mist_compass | artifact_da_lat_mist_compass | Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_DaLat_MistCompass_Source.png | artifact_da_lat_lake_glass, artifact_da_lat_pine_direction, artifact_da_lat_mist_bell, artifact_da_lat_blue_signal_seed | Đối chiếu visual sơ bộ hợp nghĩa; crop/import/approval còn pending |
| relic_jeonju_hanji_stamp | artifact_jeonju_hanji_seal | Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Jeonju_HanjiSeal_Source.png | artifact_jeonju_hanok_pattern, artifact_jeonju_dyed_thread, artifact_jeonju_old_hanji_core, artifact_jeonju_greeting_stamp | Đối chiếu visual sơ bộ hợp nghĩa; crop/import/approval còn pending |
| relic_gyeongju_star_lens | artifact_gyeongju_starlight_lens | Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Gyeongju_StarlightLens_Source.png | artifact_gyeongju_min_record, artifact_gyeongju_star_inscription, artifact_gyeongju_statue_reflection, artifact_gyeongju_heritage_ring | Đối chiếu visual sơ bộ hợp nghĩa; crop/import/approval còn pending |
| relic_seoul_trirelay_prism | artifact_seoul_three_relay_prism | Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Seoul_ThreeRelayPrism_Source.png | artifact_seoul_mist_water_core, artifact_seoul_hue_seal_shard, artifact_seoul_hanji_circuit, artifact_seoul_relay_frame | ASSET_SEMANTIC_MISMATCH; chưa gán slot |

Huế story order core→face→cord→inscription khác thứ tự hình dưới sheet face→core→cord→inscription. Jeonju story pattern→thread→core→stamp khác image core→pattern→thread→stamp. Gyeongju story record→stone→reflection→ring khác image stone→ring→record→reflection. **Không auto slice theo thứ tự ảnh rồi gán thứ tự quest**.

Seoul canonical: Heritage Data / Ji-won Sync / Stellar Ring / Relay Frame. Sheet: water-blue shard / purple seal-like shard / Hanji ribbon / relay frame. Không có ảnh chứng minh ba mảnh đầu đúng ba milestone; không rename chúng để ép map. D06 đưa hai hướng an toàn, dừng slot binding Seoul. Full prism có thể là full-art candidate nhưng chưa visual approval.

Alpha thật không bảo đảm crop sạch: nhiều glow/phụ kiện sát mép trên/dưới, một số bbox chạm x0/y0 hoặc biên1254. Không thấy watermark/label concept trong ảnh đã mở; vẫn cần kiểm viền/clip trên nền sáng và teal khi Phase2. Giữ source byte-identical, không cắt/che logo hoặc vẽ lại trong audit. Art fantasy Huế/Silla không được gọi là cổ vật lịch sử xác thực.

## Runtime item/evidence tách biệt

| Item/flag hiện có | Vai trò hiện tại | Collection sau approval |
|---|---|---|
| lantern-part / lamp_eye / h3-lamp-eye ledger | Tutorial H3; lamp_eye cho HQ01 H7 | Reflection evidence đọc completed mai-lantern; không thêm Mắt Đèn2 |
| hanji_core / link_key | HQ01 tiêu tại H7 | Không đổi thành relic hay tiêu lại |
| ancient_seal | U11; G12/S17 xác thực, không tiêu | Ấn Rồng mới là collection khác; proof U17/U8 không lấy ấn khỏi túi |
| blue_water_signal | D16; S16 xác thực, giữ item | Hạt Tín Hiệu là dấu mới, không thay item/relay |
| hanji_pattern_1/2/3 / hanji_circuit | J7/J6/J11→J12→J17/S18 | Lõi Hanji Cổ evidence không sao chép hay tiêu pattern/circuit |
| stargazer_lens / silla_prism | G4/G18, SQ02/S10/G19 | Kính Ấn Tinh Tú không là lens story cũ |
| checkin flags + rewardGrantIds | 12 mark/view events, reward once | New photo/fragment claim sau gesture cần ledger riêng; không thưởng mark lại |
| resolvedEvents / choices / defeatedEnemies | Encounter gồm battle/repair | Boss proof dùng encounter/final_cleared; repair không nhất thiết có defeatedEnemies entry |

## Trao đổi đặc sản: đúng proof, không double-consume

| Quest | Giver → complete | Điều kiện/giao thật | State/reward giữ nguyên | Clue relic hiện có? |
|---|---|---|---|---|
| HQ05 | hoi_an:H18 → jeonju:J15 | requiredQuests=[]; requiredFlags=['hoi_an.lamp_restored']; items=[{'itemId': 'hoi_an_fresh_chili', 'amount': 1}]; consume=True | jeonju.chili_shared; reward=[{'itemId': 'jeonju_recipe_card', 'amount': 1}] coins=0 | Không. Chỉ có thể thêm dialogue/clue đọc proof sau approval |
| HQ06 | hoi_an:H10 → hue:U1 | requiredQuests=[]; requiredFlags=['hoi_an.lamp_restored']; items=[{'itemId': 'hoi_an_purple_shallot', 'amount': 1}]; consume=True | hue.shallot_shared; reward=[{'itemId': 'hue_market_note', 'amount': 1}] coins=0 | Không. Chỉ có thể thêm dialogue/clue đọc proof sau approval |
| HQ07 | hoi_an:H10 → hoi_an:H10 | requiredQuests=['HQ05', 'HQ06']; requiredFlags=['jeonju.chili_shared', 'hue.shallot_shared']; items=[]; consume=False | hoi_an.festival_food_exchange_done; reward=[{'itemId': 'festival_friendship_postcard', 'amount': 1}] coins=0 | Không. Chỉ có thể thêm dialogue/clue đọc proof sau approval |
| HA_FOOD_PREP | hoi_an:H10 → hoi_an:H10 | requiredQuests=[]; requiredFlags=[]; items=[{'itemId': 'hoi_an_fresh_chili', 'amount': 2}, {'itemId': 'hoi_an_purple_shallot', 'amount': 1}]; consume=True | hoi_an.local_food_prepared; reward=[{'itemId': 'healing_tea', 'amount': 1}] coins=8 | Không. Chỉ có thể thêm dialogue/clue đọc proof sau approval |
| DQ05 | da_lat:D4 → jeonju:J15 | requiredQuests=[]; requiredFlags=[]; items=[{'itemId': 'da_lat_strawberry', 'amount': 1}]; consume=True | jeonju.dalat_strawberry_shared; reward=[] coins=10 | Không. Chỉ có thể thêm dialogue/clue đọc proof sau approval |

HQ05/HQ06 accept cần ba_tu_helped, complete cần hoi_an.lamp_restored và đúng city/node/1 item. HQ07 accept/complete đều cần **HQ05 AND HQ06 completed + chili_shared AND shallot_shared**, không flags giả là đủ. HA_FOOD_PREP giữ reserve của các món đi giao, H10 recovery item khi thiếu theo quest cũ. DQ05 accept sau da_lat.beacon_lit, cho dâu theo acceptRewardItems; chỉ giao tại J15 sau city/discovery cho phép, thưởng10xu, không tự complete khi vào Jeonju. Không biến bất kỳ side quest này thành gate WorldMap/boss hoặc auto-fragment. Nếu user chọn source phụ thuộc DQ05, phải duyệt ảnh hưởng ending collection và recovery riêng.

## Nguồn code/persistence và trạng thái tương lai

QuestManager kiểm CanAccept/CanComplete/Complete đúng node; Inventory GrantOnce giữ ledger; PuzzleManager solvedPuzzles + flags; EncounterManager ghi resolvedEvents/choices và completionFlag; SaveSystem version2 load v1/v2, `.tmp`/replace/`.bak`; GameBootstrap giữ cityProgress và legacy tutorial migration. Chưa có collection transaction/assembly/trial storage. Khuyến nghị additive namespaced flags/ledger trên schema2 nếu kiểm chứng an toàn, không rewrite SaveSystem/Inventory hay tạo hệ artifact song song.

Stage0 audit chỉ ghi proposed aliases và proof. Phase2 sau duyệt mới có actual GUID/localID/rect của30 sprites; Phase3 mới có fragment grants/persistence/trial Hội An; Phase4 mở rộng, Phase5 mới ending mới. Nguồn/binding/canonical cũ giữ nguyên. Các văn bản V2 nói relic không liên quan ending được giữ làm lịch sử, **Master 18/10 đã thay chính sách ending**, không suy rằng runtime đã cập nhật.
