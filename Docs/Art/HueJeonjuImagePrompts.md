# Prompt set và provenance hình dẫn xuất — 17/10/2026

Dùng built-in image_gen của skill imagegen, transparent_background=true; không dùng
CLI/API fallback. Nguồn tham khảo chỉ là NPC_Roster_Concept.png / Enemies_Roster_Concept.png
trong LBC_AnhMoi_BanGiaoCodex_20261015. Ảnh nguồn không bị sửa; PNG được copy vào
Characters/NPC/Hue, NPC/Jeonju, Enemies/Hue, Enemies/Jeonju. Danh sách đường dẫn/hash:
[HueJeonjuArtDerivatives.json](HueJeonjuArtDerivatives.json).

Spec chung: ONE standalone full-body transparent 2D chibi sprite for Light Between
Cultures, proposed derivative pending approval. Match supplied painted LBC roster
style, clean dark outlines, soft warm lantern lighting, jade/ochre accents, readable
at 45px. Keep whole face, hands, feet, hats, oars, held props and wings within frame,
generous clear padding. No text, labels, floor, ground shadow, scenery, checkerboard
or watermark. A new proposed face never replaces Cô Mai/Ji-eun or other approved identities.

| ID | Subject bổ sung vào spec |
|---|---|
| npc_nguyet | Bà Nguyệt, elderly Vietnamese herbalist, grey bun, kind round face, muted Hue purple áo dài, bamboo basket of medicinal leaves; distinct from Mai |
| npc_tam | Middle-aged Vietnamese ferryman, indigo shirt, rolled trousers, salt-and-pepper hair, bamboo oar, conical hat on back; distinct from existing moustached boatman |
| npc_yoon | Elderly Korean craft teacher, silver hair/beard, sage green hanbok, paper-pattern book |
| npc_park | Korean middle-aged parcel merchant, black hair, ochre vest/cream hanbok, tied paper parcel; no backpack of scrolls |
| npc_elder | Elderly Korean story keeper, silver braided bun, lavender hanbok/ivory skirt, small lantern and letter; distinct from Hana/So-yeon |
| enemy_river_wisp | Gentle jade-blue river light spirit, droplet body, warm lantern eyes, curling water ribbons; no wolf/human |
| enemy_courier | Small brass mail robot, cream/red satchel, sealed letter, amber eyes, short mechanical legs |
| enemy_archive_guard | Aged bronze/moss clockwork archive guardian, scroll shield, amber eyes, imperial motifs |
| enemy_gate_guard | Navy/gold Hue imperial guardian robot, lantern eyes/dark visor, flower shield, ceremonial spear; match reference bridge-bot role |
| enemy_parcel_bot | Small wooden/brass parcel robot, paper box in mechanical hands, jade indicators, hanok-roof helmet |
| enemy_pattern_bot | Teal/ochre hanji loom guardian, amber eyes, two round plum-flower pattern discs as shields, squat legs |
| enemy_paper_crane | Folded ivory hanji crane, ochre/red floral patterns, spread wings, luminous eyes, hovering |
| enemy_crane_boss | Large protective ivory hanji crane, indigo/crimson patterns, violet ribbons, amber eyes, full wings |

Hai yêu cầu cuối bị gián đoạn trước khi có output trong lượt đầu, chỉ gọi lại các
output chưa tồn tại. Không copy lại hình trùng. Lê An/Hana dùng native SpriteRect
và outline trong [ChapterSpriteSlices](ChapterSpriteSlices.json), không sinh bitmap mới.
Đây là sprite tĩnh đề xuất chờ duyệt, không phải canonical/final hoặc walk animation.
