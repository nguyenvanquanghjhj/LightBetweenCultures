# NPC canonical identity và visual variants

Cập nhật 2026-10-25 theo đính chính và phê duyệt của người dùng. Một npcId = một danh tính; có nhiều visual variants. Đây là registry nhận diện/duyệt art trong Docs, không tạo thêm manager hoặc registry runtime.

## Quy tắc

- Khóa cấu trúc khuôn mặt, độ tuổi tương đối, nét đặc trưng và danh tính. Trang phục, biểu cảm, cử chỉ, tư thế, hướng nhìn, phụ kiện và kiểu tóc hợp lý được thay đổi.
- CANONICAL: hình mặc định đã chốt. APPROVED_VARIANT: nhận diện hợp lệ của cùng người, dù khác trang phục. IDENTITY_CONFLICT chỉ khi khuôn mặt/tuổi/nét cốt lõi thay đổi rõ.
- Duyệt nhận diện độc lập với chất lượng crop. APPROVED_VARIANT vẫn có thể NEEDS_CROP_REVIEW; không tự đưa concept hay crop lỗi vào game.
- Không dùng cùng khuôn mặt cho hai NPC độc lập; Ji-won không dùng mặt So-yeon. npc_guest là Ji-eun, không tạo một Ji-eun thứ hai.
- Runtime AdventureUIArt, portrait binding, sprite GUID/localID và UI giữ nguyên. status cũ trong asset là chú thích lịch sử tích hợp; registry này là nguồn duyệt danh tính hiện tại. Catalog dùng overlay theo cả npcId + sprite, không duyệt tự động một hình khác chỉ vì cùng ID.
- Bà Tư, ông Phúc, Hương, So-yeon hiện tại đã CANONICAL theo xác nhận 17/10. Các hình có xác nhận cũ tiếp tục CANONICAL. Không tự coi mọi art đề xuất chưa được nhắc tới là đã duyệt.

[Dữ liệu chỉnh sửa](NPC_CANONICAL_REGISTRY.json) · [Đối chiếu trực quan bằng ảnh nguồn, không tạo bitmap](IdentityPolicy-20261017/VisualReview.html)

## npc_ba_tu — Bà Tư

Danh tính: `npc_ba_tu` · Visual status: **CANONICAL**.

Phụ nữ lớn tuổi, mặt tròn, mắt cười cong, tóc bạc lộ ở trán/mai. Khăn đỏ và giỏ rau là trang phục/đạo cụ mặc định; được thay khi vẫn nhận ra Bà Tư.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_ba_tu.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png)#H3_07_Characters_NPC_Concept_23 | ab62ba1e558f7274984500bb1cac18f6 / 8637574531497076435 | hoi_an; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |

- Reference `npc_ba_tu.missing_sheet_pose` — **APPROVED_VARIANT**, concept-only, chưa có sprite riêng: [Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png](../../Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png); Top row second character. Phụ nữ lớn tuổi, tóc bạc, mặt tròn/mắt cười, khăn đỏ và giỏ; nhất quán với Bà Tư.
## npc_boat — Bến phà thông tin

Danh tính: `npc_boat` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
## npc_boatman — Bác Lái Đò

Danh tính: `npc_boatman` · Visual status: **CANONICAL**.

Nam đứng tuổi, mặt tròn, tóc mai xám, ria cong và chòm râu cằm xám. Giữ nhận diện bác lái đò; không đồng nhất với npc_tam chỉ vì cùng nghề.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_boatman.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png](../../Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png)#NPC_HoiAn_Boatman | 3835aa72bd8c88b41af32d4aabd40b98 / 21300000 | hoi_an; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_curator — Quản thủ bảo tàng

Danh tính: `npc_curator` · Visual status: **CANONICAL**.

Giữ nguyên nhận diện, khuôn mặt, trang phục và binding Phase4 đã được chủ dự án chấp nhận nguyên trạng. Không mở rộng phê duyệt sang hình hoặc variant khác.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_curator.review_20261018 | CANONICAL / WINDOWS_RUNTIME_BINDING_PASS; OWNER_APPROVED_AS_IS_20261018 | [Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Curator.png](../../Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Curator.png)#NPC_Seoul_Curator | 6b7b12ad613bda14d969e359a2f08d59 / 21300000 | seoul; CityMap dialogue; Exact existing Phase4 runtime binding approved as-is by owner; no face, costume, crop or sprite change. |
## npc_elder — Người giữ chuyện

Danh tính: `npc_elder` · Visual status: **CANONICAL**.

Bà tóc bạc, hanbok trắng/hồng, hai tay đặt phía trước; giữ danh tính Người giữ chuyện ở Jeonju J16.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_elder.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png)#npc_elder | 9e2ba05a32d628045ae4dce1b7b1cb19 / 245854044 | jeonju; CityMap, DialoguePanel; Historical art preserved for recovery; replaced by the owner-authorized 2026-10-21 default, not bound in normal runtime. |
| npc_elder.owner20261021 | CANONICAL / WINDOWS_RUNTIME_VERIFIED; OWNER_PREVIEW_PENDING | [Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_elder.png](../../Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_elder.png)#npc_elder_Owner20261021 | 3056bc602bbb42945ba0edc767f09275 / 5509288358854237611 | jeonju; CityMap, DialoguePanel; Replaces only this NPC sprite/portrait; canonical position, identity and gameplay unchanged. |
## npc_engineer — Kỹ sư Trạm Số

Danh tính: `npc_engineer` · Visual status: **CANONICAL**.

Giữ nguyên nhận diện, khuôn mặt, trang phục và binding Phase4 đã được chủ dự án chấp nhận nguyên trạng. Không mở rộng phê duyệt sang hình hoặc variant khác.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_engineer.review_20261018 | CANONICAL / WINDOWS_RUNTIME_BINDING_PASS; OWNER_APPROVED_AS_IS_20261018 | [Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Engineer.png](../../Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Engineer.png)#NPC_Seoul_Engineer | 4f74a41c66ef3364ca044bc04afa2bcb / 21300000 | seoul; CityMap dialogue; Exact existing Phase4 runtime binding approved as-is by owner; no face, costume, crop or sprite change. |
## npc_gardener — Người Làm Vườn

Danh tính: `npc_gardener` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_gardener.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png)#npc_gardener | cac9b7f04788a1743af1858d523019df / 1390191892 | da_lat; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_guest — Ji-eun

Danh tính: `npc_guest` · Visual status: **CANONICAL**.

Ji-eun: nữ trẻ trưởng thành, mắt lớn, tóc nâu đen/tết; pose hiện tại có kính xanh tròn, mũ hoa, áo tím, máy ảnh. Đổi phụ kiện/trang phục phải giữ gương mặt, tuổi và nhận diện Ji-eun.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_guest.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_guest | c6176fa957f7eb549b9b40703e5038ea / -404209332 | hoi_an; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_hana — Nghệ nhân Hana

Danh tính: `npc_hana` · Visual status: **CANONICAL**.

Cô gái tóc đen hai bím, mắt cười nhắm, hanbok trắng/hồng, hai tay phía trước; giữ Nghệ nhân Hana ở Jeonju J2.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_hana.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_hana | c6176fa957f7eb549b9b40703e5038ea / -730319472 | jeonju; CityMap, DialoguePanel; Historical art preserved for recovery; replaced by the owner-authorized 2026-10-21 default, not bound in normal runtime. |
| npc_hana.owner20261021 | CANONICAL / WINDOWS_RUNTIME_VERIFIED; OWNER_PREVIEW_PENDING | [Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_hana.png](../../Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_hana.png)#npc_hana_Owner20261021 | 2683b36a1f2e0f24aa056fc4a46ab1d8 / -3786701452203122310 | jeonju; CityMap, DialoguePanel; Replaces only this NPC sprite/portrait; canonical position, identity and gameplay unchanged. |
## npc_hungry_dog — Chó Canh Đường

Danh tính: `npc_hungry_dog` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_hungry_dog.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png)#npc_hungry_dog | 3df301f30edc9d243899518bb89fb538 / 37410012 | da_lat; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_huong — Chị Hương

Danh tính: `npc_huong` · Visual status: **CANONICAL**.

Nữ trẻ trưởng thành, mặt tròn, mắt nâu lớn có một mắt nháy trong pose hiện tại, tóc nâu mái lệch/tết. Mũ hoa, áo xanh và giỏ hoa là visual mặc định đã chốt.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_huong.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_huong | c6176fa957f7eb549b9b40703e5038ea / -1343187348 | hue; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_innkeeper — npc_innkeeper

Danh tính: `npc_innkeeper` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
## npc_inscription — Kho bản khắc

Danh tính: `npc_inscription` · Visual status: **CANONICAL**.

Giữ nguyên nhận diện, khuôn mặt, trang phục và binding Phase4 đã được chủ dự án chấp nhận nguyên trạng. Không mở rộng phê duyệt sang hình hoặc variant khác.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_inscription.review_20261018 | CANONICAL / WINDOWS_RUNTIME_BINDING_PASS; OWNER_APPROVED_AS_IS_20261018 | [Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Inscription.png](../../Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Inscription.png)#NPC_Gyeongju_Inscription | 399ff7e49a026ed42b9cb627929f5b35 / 21300000 | gyeongju; CityMap dialogue; Exact existing Phase4 runtime binding approved as-is by owner; no face, costume, crop or sprite change. |
## npc_keeper — Sân chính di tích

Danh tính: `npc_keeper` · Visual status: **CANONICAL**.

Giữ nguyên nhận diện, khuôn mặt, trang phục và binding Phase4 đã được chủ dự án chấp nhận nguyên trạng. Không mở rộng phê duyệt sang hình hoặc variant khác.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_keeper.review_20261018 | CANONICAL / WINDOWS_RUNTIME_BINDING_PASS; OWNER_APPROVED_AS_IS_20261018 | [Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Keeper.png](../../Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Keeper.png)#NPC_Gyeongju_Keeper | 880215f55ae56f2418894f7baed7f6cb / 21300000 | gyeongju; CityMap dialogue; Exact existing Phase4 runtime binding approved as-is by owner; no face, costume, crop or sprite change. |
## npc_lan — Người dẫn đường Lan

Danh tính: `npc_lan` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_lan.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png)#npc_lan | fb6c63798ffdb6841a4fd318adf72a74 / 1886636321 | da_lat; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_mai — Cô Mai

Danh tính: `npc_mai` · Visual status: **CANONICAL**.

Nữ trẻ trưởng thành theo thiết kế chibi đã chốt; mặt tròn, mắt nâu lớn, tóc nâu đen dài. Không thay bằng phụ nữ tóc bạc lớn tuổi. Áo đỏ, nón lá và đèn lồng là trang phục/đạo cụ mặc định, không phải điều kiện khóa danh tính.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_mai.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png](../../Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png)#Mai_DialoguePortrait | a6cc838dd76ae7f40902d846693f4a5e / -1732270161 | hoi_an; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
| npc_mai.h3_lantern_open_hand | APPROVED_VARIANT / NEEDS_CROP_REVIEW | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png)#H3_07_Characters_NPC_Concept_19 | ab62ba1e558f7274984500bb1cac18f6 / -7400866961864976714 | hoi_an; H3, DialoguePanel; Biến thể nhận diện hợp lệ; chỉ được chọn cho runtime sau khi crop/viền được kiểm tra riêng. Không tự thay binding hiện tại. |

- Reference `npc_mai.yellow_outfit_young` — **APPROVED_VARIANT**, concept-only, chưa có sprite riêng: [Assets/_Game/Art/_SourceSheets/LBC_Composite_Illustrated_v1.png](../../Assets/_Game/Art/_SourceSheets/LBC_Composite_Illustrated_v1.png); Characters_MissingSprites panel, top row, second character. Nhân vật tóc tối dưới nón lá, mặt tròn/mắt lớn và tuổi trẻ tương đối phù hợp; áo vàng là biến thể trang phục, không phải IDENTITY_CONFLICT. Chưa có sprite riêng; không gán localID của cả composite cho NPC.
## npc_market_merchant — Thương nhân đường xa

Danh tính: `npc_market_merchant` · Visual status: **CANONICAL**.

Nam trung niên, mặt tròn, tóc nâu đen búi, ria cong đậm và chòm râu nhỏ. Thương nhân Hội An/Đà Lạt là cùng npcId; ba lô vải, khăn và trang phục có thể thay.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_market_merchant.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png](../../Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png)#NPC_HoiAn_MarketMerchant | 0a91b2dbca846994bbade4807ff62596 / 21300000 | da_lat, hoi_an; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_min — Hướng dẫn viên Min

Danh tính: `npc_min` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_min.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_min | c6176fa957f7eb549b9b40703e5038ea / 830274255 | gyeongju; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_nguyet — Bà Nguyệt

Danh tính: `npc_nguyet` · Visual status: **CANONICAL**.

Bà tóc bạc, nón lá, áo dài tím thêu hoa vàng, quần trắng, giỏ hoa vàng; giữ danh tính Bà Nguyệt ở Huế U2.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_nguyet.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png](../../Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png)#npc_nguyet | 48a2e6c60f7ec0349befacea7c60e927 / -1004916199 | hue; CityMap, DialoguePanel; Historical art preserved for recovery; replaced by the owner-authorized 2026-10-21 default, not bound in normal runtime. |
| npc_nguyet.owner20261021 | CANONICAL / WINDOWS_RUNTIME_VERIFIED; OWNER_PREVIEW_PENDING | [Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_nguyet.png](../../Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_nguyet.png)#npc_nguyet_Owner20261021 | 90853a883999d1a4090495dd15aa99d0 / 4180976369657288829 | hue; CityMap, DialoguePanel; Replaces only this NPC sprite/portrait; canonical position, identity and gameplay unchanged. |
## npc_park — Thương nhân Park

Danh tính: `npc_park` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_park.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png)#npc_park | 36a0c8ea46b133e40bf8731d313402d7 / -606975524 | jeonju; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_phuc — Ông Phúc

Danh tính: `npc_phuc` · Visual status: **CANONICAL**.

Nam lớn tuổi, lông mày trắng dày, ria và râu trắng, mặt tròn, mắt cười. Khăn trắng, áo thợ và dụng cụ có thể thay theo hoàn cảnh.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_phuc.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png)#Phuc_DialoguePortrait | ab62ba1e558f7274984500bb1cac18f6 / 1019635214 | hoi_an; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |

- Reference `npc_phuc.missing_sheet_pose` — **APPROVED_VARIANT**, concept-only, chưa có sprite riêng: [Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png](../../Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png); Top row third character (Nghệ nhân). Nam lớn tuổi với lông mày/râu trắng, khăn trắng, mặt tròn; nhất quán với ông Phúc. Không đổi tên nghệ nhân khác trong gameplay.
## npc_scholar — Nho Sinh Lê An

Danh tính: `npc_scholar` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_scholar.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_scholar | c6176fa957f7eb549b9b40703e5038ea / -156234960 | hue; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_soyeon — So-yeon

Danh tính: `npc_soyeon` · Visual status: **CANONICAL**.

Nữ trẻ trưởng thành, mặt tròn, mắt lớn sau kính gọng đen, mái nâu và tóc buộc cao. Trang phục văn phòng xanh/trắng và tablet có thể thay. Không dùng khuôn mặt này cho Ji-won.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_soyeon.default | CANONICAL / EXISTING_PASS_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_soyeon | c6176fa957f7eb549b9b40703e5038ea / 464872565 | jeonju; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_tam — Bác Lái Đò Tâm

Danh tính: `npc_tam` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_tam.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png](../../Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png)#npc_tam | 7eb9ff722f5f81748945b06d9e1d05bd / -1705763202 | hue; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_tech — Ji-won

Danh tính: `npc_tech` · Visual status: **CANONICAL**.

Nữ trẻ trưởng thành, mặt bầu/má đầy, mắt cười cong và nụ cười mở; tóc nâu đen dài buông, cụm hoa hồng-vàng bên trái. Diện mạo hiện tại khác Hana búi/tết tóc và nụ cười nhỏ, Ji-eun đeo kính/mắt mở và So-yeon kính gọng đen/tóc buộc cao. Hanbok hồng là trang phục lễ hội, không phải giới hạn nghề nghiệp.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_tech.festival_hanbok | CANONICAL / RUNTIME_VERIFIED_ALPHA_NATIVE_MESH | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png)#JiWon_DialoguePortrait | ab62ba1e558f7274984500bb1cac18f6 / 1851553138 | seoul; DialoguePanel; Ji-won S7, trang phục lễ hội giao lưu được người dùng chấp nhận. Không dùng mặt này cho NPC khác. Không thêm map actor hoặc thay story. |
## npc_thuy — Cô Thủy chủ tiệm hoa

Danh tính: `npc_thuy` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_thuy.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png](../../Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png)#npc_thuy | 33ce1e750bcd0124d9feb5cfcbf3fa66 / -857154714 | da_lat; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_yoon — Nhà thầy Yoon

Danh tính: `npc_yoon` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_yoon.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png)#npc_yoon | 9e80ff4e3ffd49044b5dabf92b3d6b27 / -321799653 | jeonju; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_kang_taeho — Kang Tae-ho

Danh tính: `npc_kang_taeho` · Visual status: **OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED**.

Nam trẻ, trang phục canh giữ xanh, mũ xanh và giáo; giữ nguyên gương mặt người canh giữ đã chọn.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_kang_taeho.default | OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED / WINDOWS_MAP_PORTRAIT_SAVE_RUNTIME_PASS | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_kang_taeho | c6176fa957f7eb549b9b40703e5038ea / 5254145682798913077 | gyeongju; CityMap, DialoguePanel; Existing map actor renderer at the same canonical node; original one-time reward keys retained. |
## npc_choi_eunsu — Choi Eun-su

Danh tính: `npc_choi_eunsu` · Visual status: **OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED**.

Nam trẻ, tóc nâu, áo xanh, nón và hai giỏ cây; khác thương nhân trung niên có râu canonical.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_choi_eunsu.default | OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED / WINDOWS_MAP_PORTRAIT_SAVE_RUNTIME_PASS | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png)#npc_choi_eunsu | ab62ba1e558f7274984500bb1cac18f6 / -5085934906556862197 | gyeongju; CityMap, DialoguePanel; Existing map actor renderer at the same canonical node; original one-time reward keys retained. |
## npc_han_doyun — Han Do-yun

Danh tính: `npc_han_doyun` · Visual status: **OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED**.

Nam trẻ, mũ đầu bếp trắng và chảo; giữ nguyên artwork đã chọn, nhân vật chăm góc xanh và giúp việc ở quầy công viên.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_han_doyun.default | OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED / WINDOWS_MAP_PORTRAIT_SAVE_RUNTIME_PASS | [Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png](../../Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png)#npc_han_doyun | ab62ba1e558f7274984500bb1cac18f6 / 2768669405438306836 | seoul; CityMap, DialoguePanel; Existing map actor renderer at the same canonical node; original one-time reward keys retained. |

## Đối chiếu cụ thể / IDENTITY_CONFLICT

- npc_mai · [Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png](../../Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png) · Top left woman labelled Cô Mai: **IDENTITY_CONFLICT** — Tóc xám búi và khuôn mặt thể hiện phụ nữ lớn tuổi rõ rệt, khác Cô Mai trẻ tóc dài đã chốt; không có time-skip trong cốt truyện. Không do áo vàng.
- npc_mai · [Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png](../../Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png) · Characters_MissingSprites panel, top left: **IDENTITY_CONFLICT** — Lặp lại Cô Mai tóc bạc/búi và lớn tuổi hơn rõ rệt. Không dùng thay Mai canonical.

Mai áo đỏ trong H3: APPROVED_VARIANT; Mai áo vàng trẻ trong Composite_Illustrated: APPROVED_VARIANT concept-only. Không có luật áo vàng = xung đột. Bà Tư/ông Phúc trong MissingSprites giữ tuổi/tóc/râu và nét mặt: APPROVED_VARIANT concept-only, chưa có cutout riêng. Không lấy ID của toàn bảng làm sprite nhân vật.

Các audit/ảnh/test 16–17/10 trước xác nhận là bằng chứng lịch sử, không phải trạng thái duyệt hiện tại. Không chạy lại gameplay hoặc sửa các bằng chứng PASS cũ chỉ để thay nhãn.
