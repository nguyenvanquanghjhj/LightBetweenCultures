# Áp dụng Design Kit v1 vào Unity — báo cáo thực thi

> Báo cáo lịch sử của lần tích hợp DesignKit đầu tiên, không phải trạng thái runtime
> hiện tại. Vai trò Lumi, map, combat/puzzle, số node và thành phố chơi được đã thay đổi
> ở các checkpoint sau. Khi mở rộng, dùng [catalog hiện hành](ART_ASSET_CATALOG.md),
> [quy trình kiểm soát art](CITY_EXPANSION_ART_CHECKLIST.md) và
> [CrossCityFoodReport](../../CrossCityFoodReport.md); không làm lại theo giới hạn cũ bên dưới.

Đã kiểm tra/decode đủ **44 PNG**: **14 ảnh UI dùng trực tiếp** đã được import và gắn vào game; **30 ảnh chỉ tham khảo**. Unity giữ nguyên **6000.6.5f1**, URP 2D và package hiện có. Chỉ cải thiện Hội An; năm thành phố khác vẫn khóa.

## Tài liệu nguồn

Đã đọc README_BAT_DAU.md, Docs/01_ART_BIBLE.md, Data/AssetManifest.csv và hướng dẫn tích hợp/trạng thái asset trong kit.

Hai file được yêu cầu **không có tại workspace**:
- Docs/09_IMAGE_USAGE_MATRIX.md
- Docs/10_PROMPT_CODEX_APPLY_IMAGES.md

Báo cáo này ghi trạng thái ảnh thực tế, không giả định nội dung hai file thiếu. Manifest nguồn có các mục NEEDS_CREATION đã lỗi thời so với marker PNG hiện có; giữ nguyên nguồn và ghi trạng thái mới riêng trong project.

## 14 ảnh đã gắn thật

Đường dẫn nguồn tính từ LightBetweenCultures_DesignKit_v1; đích là Assets/_Game/Art/DesignKit.

| PNG | Vị trí và điều kiện hiển thị |
|---|---|
| Assets/Lumi/Lumi_Character_Cutout_Concept.png | Main Menu: CanonicalLumiPortrait; hội thoại: LumiListening, ghi rõ đây là Lumi nghe |
| Assets/Lumi/Lumi_CityMarker_128.png | Avatar Lumi thực sự di chuyển tuần tự trên graph City Map |
| Assets/Lumi/Lumi_CityMarker_256.png | Thẻ Hội An trong World Map; avatar tạm H3 qua prefab LumiMapToken |
| Assets/Markers/node_available.png | Node mở, chưa đi qua |
| Assets/Markers/node_current.png | Node hiện tại |
| Assets/Markers/node_visited.png | Node đã đi qua; trạm H7 hoàn thành |
| Assets/Markers/node_locked.png | Node khóa và thẻ thành phố khóa |
| Assets/Markers/node_npc.png | Loại NPC tại H2; marker NPC tạm H3 |
| Assets/Markers/node_quest.png | Badge H2 đến khi hoàn thành nhiệm vụ; loại trạm H7 |
| Assets/Markers/node_item.png | Badge H3 khi nhận nhiệm vụ và chưa nhặt; SpriteRenderer vật phẩm H3 |
| Assets/Markers/node_enemy.png | Loại điểm bot dự kiến H5, chưa có combat |
| Assets/Markers/node_puzzle.png | Loại điểm câu đố dự kiến H4, chưa có cơ chế câu đố |
| Assets/Markers/node_portal.png | Loại cổng H8 đi kèm trạng thái khóa |
| Assets/Markers/node_secret.png | Loại ngõ bí mật H9 đi kèm trạng thái khóa |

**Assets/_Game/Resources/DesignKitArt.asset** lưu tham chiếu Sprite thật, xem được trong Inspector. Cả 14 PNG copy giống byte với nguồn; bảng Validation/Art/applied-images.csv có đường dẫn, vị trí gắn và SHA-256.

Quyết định hiện hành của owner 21/10/2026 thay thế quy tắc cũ: tất cả Lumi dùng `Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png` (gà trắng đúng ảnh owner vừa duyệt). Portrait/avatar/H3/combat/leisure và các vai trò tương thích dùng chung sprite này. Gà nâu áo navy và ảnh nguồn cũ vẫn được giữ để khôi phục; không còn binding hiển thị bình thường.

Portrait chỉ dùng UI. Avatar tròn trong H3 là **biểu tượng nhân vật tạm**, chưa phải sprite đi bốn hướng hay animation.

Importer: Single Sprite, Full Rect, alpha From Input/Alpha Is Transparency, Bilinear, Clamp, không mipmap/nén/scale NPOT, max 2048. PPU marker 192, avatar 128/256, portrait 256. UI giữ aspect; avatar H3 bù scale root để không kéo dẹt.

## 30 ảnh chỉ tham khảo

- 5 bảng ArtReference / Complete Asset Board / Characters Environment / Cities Themes.
- 20 PNG References/IndividualConcepts.
- Canonical gốc và GameplayPerspective_reference_from_user.png.
- Concept_Gallery.png.
- HoiAn_Node_Flow.png và HoiAn_H3_Grid_LevelBlueprint.png.

Không import các bảng/crop thành sprite production, không auto-slice nhà/NPC/tiles từ concept, không dùng nguyên ảnh tĩnh thay cảnh khám phá.

## Thay đổi giao diện và scene

Menu, bản đồ và hội thoại dùng navy/kem/vàng đèn/xanh ngọc theo Art Bible. Portrait và marker PNG thật thay chữ Lumi và biểu tượng vuông cũ.

City Map dùng tọa độ Data/hoi_an_map.json, đổi Y từ gốc trên trái sang Unity UI. Giữ ID H0–H9, graph, các điều kiện khóa và phần thưởng hiện tại. Không áp progression/quest dài hạn của design-only JSON.

Nền City Map hiện là sơ đồ phố cổ bằng hình Unity UI độc lập: sông, cầu, mái ngói/tường vàng và cây. Đây là bố cục proxy, chưa phải backdrop Hội An vẽ tay hoàn chỉnh.

Marker ưu tiên khóa → hiện tại → đã đi qua → mở chưa đi. Badge loại điểm tách riêng. Nhặt xong tắt badge vật phẩm H3; trả đồ tắt badge nhiệm vụ H2; phần thưởng mở các icon tuyến tương ứng.

Save thêm trường visited. Save v1 cũ giữ quest/inventory/vật phẩm/cờ/node. Vì save cũ không ghi toàn bộ lịch sử, chỉ khôi phục được điểm đầu và node hiện tại; các điểm đi qua được ghi đầy đủ từ đợt này.

HoiAnDemo được sửa tại chỗ và lưu bằng Unity Editor API. Avatar, NPC marker và vật phẩm là SpriteRenderer độc lập. Nhà có mái/cửa/cửa sổ/chân nhà; cây có thân/tán; cầu có ván/lan can; đường có mạch lát. Môi trường vẫn là hình học proxy chờ art tách lớp, không được báo là sprite production từ kit.

SortingGroup giữ các phần công trình cùng nhóm, sorting theo chân đối tượng. **14 component Rigidbody2D/BoxCollider2D gốc trong scene giữ nguyên nội dung serialize**, cùng vị trí spawn/vật phẩm và logic physics.

## Mở để xem và chơi

1. Mở project bằng Unity 6000.6.5f1.
2. Mở **Assets/_Game/Scenes/Prototype.unity**, nhấn Play để thấy portrait canonical.
3. Bắt đầu → Hội An → H2 nhận nhiệm vụ → Tiếp tục → H3.
4. Click/chạm mặt đất hoặc WASD; đến gần vật phẩm rồi E / Tương tác.
5. Về H3 → H2 trả nhiệm vụ; xem badge/tuyến mở cập nhật và đi đến H7.
6. Dừng Play / Play lại để kiểm tra save. Đặt lại tiến độ ở menu có xác nhận/Hủy.

UI được tạo khi Play bởi controller hiện tại. Scene View của **Assets/_Game/Scenes/HoiAnDemo.unity** hiển thị trực tiếp SpriteRenderer đã gắn ảnh và các đối tượng độc lập.

Prefab xem riêng:
- Assets/_Game/Prefabs/DesignKit/LumiMapToken.prefab
- Assets/_Game/Prefabs/DesignKit/LanternPartMarker.prefab
- Assets/_Game/Prefabs/DesignKit/NpcMarker.prefab

Prefab chỉ chứa phần hiển thị; physics/controller nằm trên root scene hiện có.

Bản Windows: **Builds/Windows/LightBetweenCultures.exe**, chạy không cần tham số, giữ DLL và thư mục dữ liệu cạnh exe.

Tools > Light Between Cultures > Apply Provided Design Kit Art import/gắn lại ảnh, có backup scene và không tạo trùng nhóm proxy. Setup Hoi An Demo cũ cũng áp art hiện có sau khi tạo scene.

## Ảnh chụp thật

Đã mở xem các ảnh cuối tại **Validation/Art/RunCapture**:
- 01-menu.png, 02-world-map.png, 03-city-map.png
- 04-mai-dialogue.png, 04b-quest-and-item-markers.png
- 05-exploration-portrait.png, 06-exploration-landscape.png
- 07-item-closeup.png, 08-map-after-pickup.png
- 09-map-reward-unlocked.png, 10-map-landscape.png

Đây là ảnh từ bản Windows chạy có render, không phải concept.

## Kiểm thử và bảo toàn

- Unity import/apply và Windows Development build: thành công, exit 0.
- **43 assertion runtime PASS**: 14 Sprite, canonical menu/hội thoại, loại/trạng thái icon, lịch sử node, save cũ, reset/hủy, UTF-8, chuột/cảm ứng mô phỏng/WASD, collision, nhiệm vụ, nhặt một lần, chuyển scene, thưởng, không trùng manager/UI.
- **2 assertion khi mở tiến trình mới PASS**: registry ảnh và khôi phục save sau khi tiến trình cũ đã thoát.
- Camera và ảnh UI kiểm tra 9:16, 16:9.
- Không mất file gốc. SampleScene, version Unity, manifest/lock package, QuestManager, Inventory, PlayerController, InteractionSystem và SceneTransition giữ nguyên byte.
- Physics serialize H3 giữ nguyên.

Kết quả: Validation/Art/windows-smoke-results.txt, windows-restart-results.txt, preservation-checks.txt.
Inventory: Validation/Art/png-inventory.csv, applied-images.csv.
Danh sách file thêm/sửa: Validation/Art/changed-files.txt.
Log cuối: Logs/design-kit-apply.log, design-kit-build-final.log, design-kit-player-final.log, design-kit-restart.log.

## Còn thiếu

Danh sách asset cụ thể, kích thước và ưu tiên: **Docs/Art/MISSING_ASSETS.md**. Ưu tiên sprite Lumi bốn hướng, nhà tách lớp, backdrop sạch, tiles đường/đất, linh kiện thật và chân dung cô Mai; sau đó cây/cầu/nước/NPC/bot.

Chưa chạy Android thật hoặc tạo APK: Unity trên máy mới có Windows/WebGL Support. Cảm ứng trong báo cáo là Touchscreen mô phỏng qua Input System trên Windows.

Không thêm combat, puzzle mới hoặc mở cổng thành phố khác. Marker điểm tương lai không có nghĩa nội dung đã hoàn thành. Không tuyên bố hoàn thiện đồ họa sáu thành phố.

Backup: **D:/GAMEKKK/Backups/BeforeDesignKit-20261015** và **D:/GAMEKKK/Backups/DesignKitSceneUpdates**.
