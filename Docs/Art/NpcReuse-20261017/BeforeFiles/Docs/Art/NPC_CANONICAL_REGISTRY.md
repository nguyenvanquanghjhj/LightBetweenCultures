# NPC canonical identity và visual variants

Cập nhật 17/10/2026 theo đính chính của người dùng. Một npcId = một danh tính; có nhiều visual variants. Đây là registry nhận diện/duyệt art trong Docs, không tạo thêm manager hoặc registry runtime.

## Quy tắc

- Khóa cấu trúc khuôn mặt, độ tuổi tương đối, nét đặc trưng và danh tính. Trang phục, biểu cảm, cử chỉ, tư thế, hướng nhìn, phụ kiện và kiểu tóc hợp lý được thay đổi.
- CANONICAL: hình mặc định đã chốt. APPROVED_VARIANT: nhận diện hợp lệ của cùng người, dù khác trang phục. IDENTITY_CONFLICT chỉ khi khuôn mặt/tuổi/nét cốt lõi thay đổi rõ.
- Duyệt nhận diện độc lập với chất lượng crop. APPROVED_VARIANT vẫn có thể NEEDS_CROP_REVIEW; không tự đưa concept hay crop lỗi vào game.
- Không dùng cùng khuôn mặt cho hai NPC độc lập; Ji-won không dùng mặt So-yeon. npc_guest là Ji-eun, không tạo một Ji-eun thứ hai.
- Runtime AdventureUIArt, portrait binding, sprite GUID/localID và UI giữ nguyên. status cũ trong asset là chú thích lịch sử tích hợp; registry này là nguồn duyệt danh tính hiện tại. Catalog dùng overlay theo cả npcId + sprite, không duyệt tự động một hình khác chỉ vì cùng ID.
- Bà Tư, ông Phúc, Hương, So-yeon hiện tại đã CANONICAL theo xác nhận 17/10. Các hình có xác nhận cũ tiếp tục CANONICAL. Không tự coi mọi art đề xuất chưa được nhắc tới là đã duyệt.

[Dữ liệu chỉnh sửa](NPC_CANONICAL_REGISTRY.json) · [Đối chiếu trực quan bằng ảnh nguồn, không tạo bitmap](IdentityPolicy-20261017/VisualReview.html)

## Đánh giá tận dụng art chưa định danh — cập nhật 17/10/2026

Quyền gán canonical mới chỉ áp dụng khi ảnh chưa thuộc danh tính khác, phù hợp vai trò/bối cảnh và đủ chất lượng sprite runtime. Đối chiếu roster, moodboard, DesignKit và các sheet cũ lần này **không có ứng viên đủ điều kiện để gán mới**; không duyệt thêm 12 NPC đang chờ xác nhận.

- `npc_boat` S14, `npc_curator` S2, `npc_engineer` S11, `npc_inscription` G14, `npc_keeper` G9, `npc_tech` S7: **NEEDS_NEW_ART**. Không dùng So-yeon cho Ji-won/quản thủ/kỹ sư hoặc chuyển lính Hoàng thành Huế thành NPC Gyeongju.
- `npc_innkeeper` D18: **NOT_REQUIRED_CURRENT_GAMEPLAY**. Hiện chỉ Lumi tự thoại/nghỉ ngơi, chưa có nhân vật chủ trọ; không gán đầu bếp để lấp chỗ trống.
- Giữ `status=MISSING_VISUAL` cho bảy binding rỗng để phản ánh đúng dữ liệu runtime. `portraitAssessment.assetStatus` ghi yêu cầu art hiện tại, độc lập với approval danh tính.
- [Lý do theo NPC và ứng viên](NpcInventory-20261017/PortraitReuseAssessment.md) · [Ảnh đối chiếu](NpcInventory-20261017/Review.html) · [Bảng kiểm kê](NpcInventory-20261017/README.md).

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

Danh tính: `npc_curator` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
## npc_elder — Người giữ chuyện

Danh tính: `npc_elder` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_elder.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png](../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png)#npc_elder | 9e2ba05a32d628045ae4dce1b7b1cb19 / 245854044 | jeonju; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
## npc_engineer — Kỹ sư Trạm Số

Danh tính: `npc_engineer` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
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

Danh tính: `npc_hana` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_hana.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png](../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png)#npc_hana | c6176fa957f7eb549b9b40703e5038ea / -730319472 | jeonju; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
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

Danh tính: `npc_inscription` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
## npc_keeper — Sân chính di tích

Danh tính: `npc_keeper` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
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

Danh tính: `npc_nguyet` · Visual status: **UNCONFIRMED_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| npc_nguyet.default | UNCONFIRMED_VISUAL / EXISTING_ASSET_PRESERVED | [Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png](../../Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png)#npc_nguyet | 48a2e6c60f7ec0349befacea7c60e927 / -1004916199 | hue; CityMap, DialoguePanel; Dùng theo binding/event hiện tại; discovery, quest và lựa chọn không thay đổi. |
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

Danh tính: `npc_tech` · Visual status: **MISSING_VISUAL**.

Chưa khóa chi tiết khuôn mặt trong lượt này; giữ hình/binding hiện có, không suy ra xung đột từ trang phục hoặc ID file.

| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |
|---|---|---|---|---|
| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |
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

## Đối chiếu cụ thể / IDENTITY_CONFLICT

- npc_mai · [Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png](../../Assets/_Game/Art/_SourceSheets/Characters_MissingSprites_v1.png) · Top left woman labelled Cô Mai: **IDENTITY_CONFLICT** — Tóc xám búi và khuôn mặt thể hiện phụ nữ lớn tuổi rõ rệt, khác Cô Mai trẻ tóc dài đã chốt; không có time-skip trong cốt truyện. Không do áo vàng.
- npc_mai · [Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png](../../Assets/_Game/Art/_SourceSheets/LBC_Composite_CheckerConcept_v2.png) · Characters_MissingSprites panel, top left: **IDENTITY_CONFLICT** — Lặp lại Cô Mai tóc bạc/búi và lớn tuổi hơn rõ rệt. Không dùng thay Mai canonical.

Mai áo đỏ trong H3: APPROVED_VARIANT; Mai áo vàng trẻ trong Composite_Illustrated: APPROVED_VARIANT concept-only. Không có luật áo vàng = xung đột. Bà Tư/ông Phúc trong MissingSprites giữ tuổi/tóc/râu và nét mặt: APPROVED_VARIANT concept-only, chưa có cutout riêng. Không lấy ID của toàn bảng làm sprite nhân vật.

Các audit/ảnh/test 16–17/10 trước xác nhận là bằng chứng lịch sử, không phải trạng thái duyệt hiện tại. Không chạy lại gameplay hoặc sửa các bằng chứng PASS cũ chỉ để thay nhãn.
