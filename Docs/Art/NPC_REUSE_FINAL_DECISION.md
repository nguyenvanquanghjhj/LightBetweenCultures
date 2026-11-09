# NPC reuse — quyết định cuối, 17/10/2026

Áp dụng attachment **f7150595-fc49-4f4b-8ccf-7c18da6030eb**, bao gồm quyền chốt nhân vật chưa định danh sau khi nguồn, chất lượng, binding và hiển thị đạt. Không duyệt hàng loạt 12 NPC đang dùng.

## Kết quả chính xác

| Mục | Kết quả |
|---|---|
| Trong 6 vị trí được xử lý | **2/6 không còn cần bổ sung ảnh**: S7 có portrait mới; S14 được xác minh là điểm thông tin, không cần tạo người |
| Portrait mới từ nguồn cũ | **1 — Ji-won, npc_tech, Seoul S7** |
| Canonical mới khóa | **1 — Ji-won**, native `JiWon_DialoguePortrait` |
| Vai trò vẫn NEEDS_NEW_ART | **4 — S11, S2, G9, G14** |
| D18 | Lumi dẫn chuyện/nghỉ; không thêm chủ trọ hoặc portrait |
| Tổng sau lượt này | 27 khóa; **21 có hình, 9 canonical, 12 có hình chưa duyệt**, 4 cần art và 2 khóa không cần portrait người |

[Ảnh runtime mới và đối chiếu nguồn/nhận diện](NpcReuse-20261017/Review.html) · [Native reference](NpcReuse-20261017/jiwon-native-reference.json) · [Kiểm tra bảo toàn](NpcReuse-20261017/verification.json).

## Ji-won mặc hanbok — sử dụng được

Nguồn: `Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png`, vùng cô gái Hàn bên trái sprite ghép `_22`. Nhân vật chưa có binding npcId; không lấy nguyên `_22` gồm hai người làm portrait. Đã tạo đúng **một native Sprite sub-asset** trên texture cũ; không có PNG nhân vật mới, không sửa pixel nguồn.

- Sprite: `JiWon_DialoguePortrait`.
- GUID: `ab62ba1e558f7274984500bb1cac18f6`.
- localID: `1851553138`.
- Rect nguồn: Unity bottom-left `(24,66,355,400)`; tương đương top-left `(24,620,355,400)` trên ảnh1448×1086 RGBA.
- Outline riêng giữ đầu/tóc, hai tay, lá cờ và váy; loại phần mũ của khách kế bên. UI dùng `portraitUseSpriteMesh=true`. 30 slice cũ giữ nguyên toàn block, ID/crop/outline.
- Nguồn cao400px, hiển thị trong slot cao≤216px; không upscale concept nhỏ để gọi final. Đã xem ảnh runtime, không thấy nền/chữ/người cạnh hoặc tay bị cắt.

Đối chiếu trực quan ở độ phân giải gốc và trang review: Hana có tóc búi/tết và khuôn mặt/nụ cười hẹp hơn; Ji-won hiện có mặt bầu, má đầy/nụ cười mở, tóc dài buông với cụm hoa bên trái. Ji-eun có mắt mở và kính du khách; So-yeon kính gọng đen/tóc buộc cao. Đây là đánh giá hình ảnh về khả năng phân biệt, không suy từ việc GUID khác hoặc khẳng định máy có thể chứng minh danh tính. **Không sửa hoặc gán lại Hana/Ji-eun/So-yeon**. Hanbok lễ hội được chấp nhận, không ép nhân viên công nghệ mặc đồng phục.

Khóa `npc_tech.festival_hanbok` CANONICAL trong registry theo quyền người dùng đã cấp, sau khi hiển thị S7 PASS. Giữ cốt lõi nhận diện này; biến thể trang phục/pose tương lai phải vẫn nhận ra Ji-won.

## Kỹ sư đeo kính — chưa dùng làm runtime

Vùng nghệ nhân cầm giấy trong `LBC_Composite_Illustrated_v1.png` chỉ khoảng100–115×135–140px và có nền. Nhân vật không bị loại vì cầm giấy hay không mặc đồ kỹ thuật. Vấn đề là **độ phân giải và chưa có cutout RGBA sạch**; slot portrait hiện tại có thể cần cao216px. Giữ **CHARACTER DESIGN REFERENCE**, không phóng lên rồi tuyên bố art final. Cần bản vẽ sạch đủ chi tiết nếu dùng thiết kế này cho S11.

## S14 và D18 — không tạo nhân vật để lấp khóa

S14 runtime dùng actor/tên người nói **Bến phà thông tin**, câu thông tin về điều kiện vận hành. Không có nhân dạng người lái phà được thể hiện. Giữ nguyên mọi event, đường đò, chức năng và nội dung; `portraitAssessment.assetStatus=NOT_REQUIRED_LOCATION_INFORMATION`, không chốt một người mới. UI không có vòng portrait rỗng/mượn mặt.

D18 vẫn là `type=start`, `actor=Lumi`, nghỉ/nghỉ dẫn đường. Giữ `NOT_REQUIRED_CURRENT_GAMEPLAY`.

## Nhân vật chưa định danh còn để dành

- Đầu bếp H3_07 `_14`: có native slice người đơn lẻ nhưng không phù hợp bốn vai còn thiếu một cách đủ chắc; không tạo chủ trọ D18.
- Người gánh hàng trẻ cạnh Phúc trong `_21`: chưa cắt độc lập; không thay thương nhân canonical. Để dành, không thêm NPC.
- Lính Hoàng thành Huế trong Shared atlas: chưa gán npcId; nguồn ghi rõ văn hóa/vai trò, không chuyển thành lính Gyeongju.
- Khách cầm máy ảnh bên phải `_22`: còn chưa gán độc lập, cần đối chiếu với Min/Ji-eun trước khi coi là mặt tự do. Cô gái trái `_22` **đã thuộc Ji-won**, không còn là ứng viên trống.
- Nghệ nhân đeo kính trong Composite_Illustrated và các hình nhỏ DesignKit: design reference, chưa phải sprite đủ chất lượng. Các biến thể Mai/Bà Tư/Phúc và archetype đã dùng không là NPC mới. Bốn DaLatRejectedDrafts vẫn là bản nháp của vai đã có.

## Bốn brief gửi người hỗ trợ art

Các tuổi/ngoại hình sau là **đề xuất art**, không bổ sung sự kiện hay sửa danh tính story. Không tự tạo PNG trong nhiệm vụ này. [JSON brief đầy đủ](NpcReuse-20261017/needs-new-art.json).

### npc_engineer — Kỹ sư Trạm Số, Seoul S11

- Vai trò runtime: Giải thích hồ sơ/lệnh phòng vệ và xác nhận SQ02, không thay quest hay ending.
- Tuổi biểu kiến: Nam trưởng thành khoảng 28–40 (đề xuất art, chưa là chi tiết story).
- Ngoại hình: Mặt oval hơi dài, tóc ngắn rẽ bên, kính chữ nhật mảnh; áo xanh than/áo khoác màu đồng nhạt, có thể cầm giấy. Không cần đồng phục kỹ thuật.
- Nhận diện: Giữ đường nét nam trưởng thành, trán thoáng và kính chữ nhật; không dùng mặt học giả Lê An/Min/Yoon/Phúc.
- Tham chiếu: Assets/_Game/Art/_SourceSheets/LBC_Composite_Illustrated_v1.png — nghệ nhân đeo kính/cầm giấy, vùng x≈1000,y≈595,w≈115,h≈140. CHARACTER DESIGN REFERENCE ONLY.
- Phong cách/tỷ lệ: Hoạt hình 2D chibi LBC, khoảng 2.5–3 đầu, nét ấm/viền đồng đều, cùng mức hoàn thiện Mai/Hương/So-yeon; không realistic/3D.
- Kích thước/nền: Khuyến nghị PNG RGBA 1024×1536 hoặc tương đương; nhân vật cao ít nhất 1000px trong nguồn, padding 4–6%, đầy đủ đầu/tay/vạt áo theo pose. Alpha thật, không chữ/nhãn/checkerboard/nền trắng/người khác. Không upscale ảnh concept nhỏ.
- File đích: `Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Engineer.png`.
- Tránh mặt canonical: Mai, Bà Tư, Phúc, Hương, So-yeon, Ji-eun, bác lái đò, thương nhân và Ji-won; cũng không dùng lại 12 mặt đã binding dù chưa duyệt.
- Binding sau khi có ảnh được duyệt: AdventureUIArt.asset → npcs entry đúng npcId, sprite/portrait cùng sprite đã duyệt; PNG độc lập dùng Sprite Single (localID thường 21300000), ghi GUID/localID thực. Không đổi event/quest. Không tự thêm map actor. Chỉ cập nhật registry sau nghiệm thu ảnh. Không hardcode đường PNG vào gameplay.

### npc_curator — Quản thủ bảo tàng, Seoul S2

- Vai trò runtime: Mở SQ02, hướng dẫn hồ sơ S4 và lời nhắn S3; không thêm nhiệm vụ.
- Tuổi biểu kiến: Người trưởng thành khoảng 45–60, có thể nữ (đề xuất art).
- Ngoại hình: Gương mặt dài mềm, tóc ngắn pha bạc ngang cằm; hanbok tiết chế hoặc áo khoác màu tím xám, có thể cầm sổ.
- Nhận diện: Tóc bob/tóc bạc vừa phải, nếp mắt nhẹ, gương mặt trung niên; tránh bà lão tóc búi của npc_elder/Bà Tư và mặt So-yeon/Hương.
- Tham chiếu: NPC_Roster_Concept Tech-Curator chỉ tham khảo phong cách/vai trò, khuôn mặt đã thuộc So-yeon nên KHÔNG tái sử dụng.
- Phong cách/tỷ lệ: Hoạt hình 2D chibi LBC, khoảng 2.5–3 đầu, nét ấm/viền đồng đều, cùng mức hoàn thiện Mai/Hương/So-yeon; không realistic/3D.
- Kích thước/nền: Khuyến nghị PNG RGBA 1024×1536 hoặc tương đương; nhân vật cao ít nhất 1000px trong nguồn, padding 4–6%, đầy đủ đầu/tay/vạt áo theo pose. Alpha thật, không chữ/nhãn/checkerboard/nền trắng/người khác. Không upscale ảnh concept nhỏ.
- File đích: `Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Curator.png`.
- Tránh mặt canonical: Mai, Bà Tư, Phúc, Hương, So-yeon, Ji-eun, bác lái đò, thương nhân và Ji-won; cũng không dùng lại 12 mặt đã binding dù chưa duyệt.
- Binding sau khi có ảnh được duyệt: AdventureUIArt.asset → npcs entry đúng npcId, sprite/portrait cùng sprite đã duyệt; PNG độc lập dùng Sprite Single (localID thường 21300000), ghi GUID/localID thực. Không đổi event/quest. Không tự thêm map actor. Chỉ cập nhật registry sau nghiệm thu ảnh. Không hardcode đường PNG vào gameplay.

### npc_keeper — Người giữ di tích, Gyeongju G9

- Vai trò runtime: Đưa gợi ý về tượng đá và các biểu tượng sao; giữ lời thoại/nhánh sẵn có.
- Tuổi biểu kiến: Nam khoảng 40–55 (đề xuất art).
- Ngoại hình: Mặt vuông mềm, mắt bình tĩnh, tóc ngắn hơi pha bạc; trang phục truyền thống Hàn hoặc áo trông coi xanh rêu/xám. Không giáp/mũ lính Hoàng thành Huế.
- Nhận diện: Hàm vuông, chân mày ngang, không râu dài; không dùng mặt Phúc/Yoon/bác lái đò/Tâm/Min.
- Tham chiếu: Map Gyeongju và NPC_Roster_Concept dùng cho bối cảnh/phong cách; không có mặt trống đủ phù hợp đã xác minh.
- Phong cách/tỷ lệ: Hoạt hình 2D chibi LBC, khoảng 2.5–3 đầu, nét ấm/viền đồng đều, cùng mức hoàn thiện Mai/Hương/So-yeon; không realistic/3D.
- Kích thước/nền: Khuyến nghị PNG RGBA 1024×1536 hoặc tương đương; nhân vật cao ít nhất 1000px trong nguồn, padding 4–6%, đầy đủ đầu/tay/vạt áo theo pose. Alpha thật, không chữ/nhãn/checkerboard/nền trắng/người khác. Không upscale ảnh concept nhỏ.
- File đích: `Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Keeper.png`.
- Tránh mặt canonical: Mai, Bà Tư, Phúc, Hương, So-yeon, Ji-eun, bác lái đò, thương nhân và Ji-won; cũng không dùng lại 12 mặt đã binding dù chưa duyệt.
- Binding sau khi có ảnh được duyệt: AdventureUIArt.asset → npcs entry đúng npcId, sprite/portrait cùng sprite đã duyệt; PNG độc lập dùng Sprite Single (localID thường 21300000), ghi GUID/localID thực. Không đổi event/quest. Không tự thêm map actor. Chỉ cập nhật registry sau nghiệm thu ảnh. Không hardcode đường PNG vào gameplay.

### npc_inscription — Người hỗ trợ bản khắc, Gyeongju G14

- Vai trò runtime: Giải thích ký hiệu/khiên ba nhịp của Lõi Sao; không thêm puzzle.
- Tuổi biểu kiến: Nữ trưởng thành khoảng 30–45 (đề xuất art).
- Ngoại hình: Mặt hơi góc cạnh, tóc ngắn rẽ lệch, phụ kiện kẹp tóc nhỏ đơn sắc; áo truyền thống hoặc áo nghiên cứu màu lam/xám nhạt. Có thể cầm sổ, không bắt buộc hiện vật.
- Nhận diện: Tóc ngắn/gương mặt trưởng thành, không dùng tóc dài-mắt cười của Ji-won/Hana, kính đen/tóc cao của So-yeon hoặc mặt Ji-eun.
- Tham chiếu: Di tích/bản khắc trong map Gyeongju; DesignKit sư thầy/bảo vệ quá nhỏ để làm portrait, không là face mẫu được chốt.
- Phong cách/tỷ lệ: Hoạt hình 2D chibi LBC, khoảng 2.5–3 đầu, nét ấm/viền đồng đều, cùng mức hoàn thiện Mai/Hương/So-yeon; không realistic/3D.
- Kích thước/nền: Khuyến nghị PNG RGBA 1024×1536 hoặc tương đương; nhân vật cao ít nhất 1000px trong nguồn, padding 4–6%, đầy đủ đầu/tay/vạt áo theo pose. Alpha thật, không chữ/nhãn/checkerboard/nền trắng/người khác. Không upscale ảnh concept nhỏ.
- File đích: `Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Inscription.png`.
- Tránh mặt canonical: Mai, Bà Tư, Phúc, Hương, So-yeon, Ji-eun, bác lái đò, thương nhân và Ji-won; cũng không dùng lại 12 mặt đã binding dù chưa duyệt.
- Binding sau khi có ảnh được duyệt: AdventureUIArt.asset → npcs entry đúng npcId, sprite/portrait cùng sprite đã duyệt; PNG độc lập dùng Sprite Single (localID thường 21300000), ghi GUID/localID thực. Không đổi event/quest. Không tự thêm map actor. Chỉ cập nhật registry sau nghiệm thu ảnh. Không hardcode đường PNG vào gameplay.

## File thay đổi và kiểm thử

- `PortraitRepairSetup.cs`: thêm lệnh Editor hẹp `BuildJiWonReuse`, không gọi lại phần sửa BaTu/Phuc lịch sử; bảo vệ binding NPC cũ và slice ID, thêm một slice rồi build.
- H3_07 `.png.meta`: thêm một slice/outline Ji-won; PNG không đổi. `Resources/AdventureUIArt.asset`: thêm đúng entry npc_tech, mọi byte ngoài entry mới giữ nguyên.
- Runner hiện hữu `.cs`/`.Portraits.cs`: nhánh QA opt-in `--npc-reuse-qa`, dùng World Map và tương tác thật S7/S14; không thêm manager/scene/Canvas.
- Registry JSON/MD, native bindings, manifest JSON/CSV và catalog: cập nhật một canonical mới và phân loại S14. Bản kiểm kê cũ được đánh dấu superseded; không xóa bằng chứng lịch sử.
- **Windows build PASS; 30 assertions runtime PASS**, ba ảnh (World Map, S7, S14). Kiểm sprite/speaker/native mesh/talk, không đổi quest/item/reward khi nói, exact persisted gameplay state sau reload. Dùng bản sao save postgame đã kiếm được; không gọi là fresh quest acceptance/E2E.
- Giữ nguyên 8 canonical cũ và 12 binding khác, 30 sprite block cũ, source PNG, 133 node/29 quest, scene H3 và SaveSystem. Không thêm map actor, item hay gameplay. ProjectSettings chỉ bị Unity bổ sung slot icon Android rỗng; đã khôi phục exact byte trước lượt, không rebuild thừa.
- Android và kiểm thử toàn hành trình không chạy lại vì phạm vi chỉ thêm portrait/binding.

Dừng sau lượt reuse này. Không vẽ art mới hoặc viết lại cốt truyện.

Checkpoint: **CheckpointNpcReuse-20261017**, incremental cần CheckpointReleaseReadinessAudit-20261017 và chuỗi base của nó; 27 file, khoảng 2.86 MiB. Đã kiểm hash payload; không copy full build/cache hoặc xóa backup cũ.
