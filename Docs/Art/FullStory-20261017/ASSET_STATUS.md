# Full Story — art tạm / còn thiếu

Không thay ngoại hình Cô Mai, Ji-eun, Hương hoặc So-yeon. Không gán So-yeon làm Ji-won. Mọi binding cũ được kiểm tra GUID/localID.

Min dùng native sprite mới trong atlas RGBA đã có, từ hướng dẫn viên khảo cổ chưa gán danh tính; cần duyệt nhận diện. Không tạo PNG nhân vật trùng.

| Thiếu portrait riêng | Vai trò |
|---|---|
| npc_boat | seoul:S14 |
| npc_curator | seoul:S2 |
| npc_engineer | seoul:S11 |
| npc_innkeeper | da_lat:D18 |
| npc_inscription | gyeongju:G14 |
| npc_keeper | gyeongju:G9 |
| npc_tech | seoul:S7 |

G9/G14/S2/S7/S11/S14 có tương tác/quest thật, nhưng chưa có portrait riêng. D18 vẫn dùng nghỉ ngơi và lời tự thoại Lumi đã phát hành. Không dùng khung tròn rỗng hoặc khuôn mặt NPC đã có danh tính để giả làm final.

| Enemy mới | Art tạm |
|---|---|
| enemy_stone_guard | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance · Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png#stone_guard |
| enemy_star_core | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance · Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png#energy_core |
| enemy_final_core | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance · Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png#energy_core |
| enemy_shadow_courier | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; needs visual art acceptance · Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png#shadow_courier |
| enemy_fox | TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; cat concept pending actual fox sprite · Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png#shadow_cat |
| enemy_night_guard | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_gate_guard · Assets/_Game/Art/Characters/Enemies/Hue/enemy_gate_guard.png#enemy_gate_guard |
| enemy_bridge | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_mo01 · Assets/_Game/Art/Characters/Enemies/HoiAn_Enemies_Atlas.png#enemy_mo01 |
| enemy_drone | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_glass_thief · Assets/_Game/Art/Characters/Enemies/DaLat/enemy_glass_thief.png#enemy_glass_thief |
| enemy_signal_bot | TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias enemy_pattern_bot · Assets/_Game/Art/Characters/Enemies/Jeonju/enemy_pattern_bot.png#enemy_pattern_bot |

Lõi sao và lõi cuối tạm dùng chung concept cầu nhiễu, không tạo PNG trùng và không gọi đây là hai boss art final. Cáo tạm dùng concept mèo bóng; drone tạm dùng bot có sẵn: cần đúng sprite cáo/drone trước nghiệm thu art. Tượng đá và bot đưa tin dùng mesh tách từ đúng concept trong nguồn người dùng; vẫn cần duyệt biên/effect.

Hai Map Overview A mới là bản copy nguyên byte, đủ1448×1086, không phóng lớn ảnh preview. Thư viện/catalog chỉ cập nhật inventory; không chạy lại migration thư viện.

Các portrait tạm/chờ duyệt đã có từ checkpoint trước vẫn giữ nguyên; xem asset-status.json để đọc source/status đầy đủ. Android/device và chất lượng terrain chưa được nghiệm thu.
