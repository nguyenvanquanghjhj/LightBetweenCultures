Bạn là Senior Unity 2D Gameplay Engineer + Technical Artist + Level Designer. Hãy làm việc trực tiếp trên project đang mở, không tự tạo project mới.

ĐỌC TRƯỚC: `LBC_AnhMoi_BanGiaoCodex_20261015/README_VN.md`, `ASSET_MANIFEST.csv`, `GAME_DESIGN_OVERVIEW.md` và `LightBetweenCultures_DesignKit_v1` (nếu có). Đối chiếu scene/script/asset hiện có; không làm lại, không tự ý xóa hoặc thay đổi style đã được duyệt. Tôi đã đưa ảnh concept mới vào **project root ngoài Assets**. Hãy đọc ảnh theo đường dẫn thật và chủ động chọn ảnh để tích hợp.

I. PHÂN LOẠI ẢNH VÀ SỬ DỤNG
- `Maps/<City>/Map_Overview_A/B.png`: 2 bản *tham khảo bố cục* cho mỗi thành phố. Chọn bản phù hợp làm nền City Map 2D ở bước đầu, sao chép file vào `Assets/_Game/Art/Runtime/CityMaps/` khi dùng, thiết lập Sprite (2D and UI), giữ tỉ lệ, hỗ trợ pan/zoom, không crop landmark quan trọng. Giữ một bản còn lại làm art reference, không sử dụng cả hai chồng nhau tùy tiện.
- `Characters/Lumi_Actions_Concept.png`: một góc nhìn nhân vật, nhiều trạng thái. **Không bắt làm 4 hướng**. Chỉ tách nếu crop được sạch, kiểm tra alpha/viền; nếu không, hãy dùng sprite Lumi sạch trong Design Kit cũ hoặc placeholder với nhãn TODO, và ghi công việc cần người dùng cấp sprite thật.
- `Characters/NPC_Roster_Concept.png`, `Enemies_Roster_Concept.png`: nguồn duyệt concept NPC và bot; **không dùng cả sheet làm sprite**, không cắt tự động nếu nền/chữ dính vào. Tạo `CharacterData`/`NPCData`/`EnemyData` có sprite references có thể thay sau.
- `Props/Items_And_Interactables_Concept.png`: source reference để tạo icon trong UI/rương/kính/thịt/cổng; crop riêng CHỈ nếu viền sạch, kiểm tra alpha. Logic chức năng không phụ thuộc hình.
- `Characters/Characters_Moodboard.png`: style guide hỗ trợ; không dùng làm in-game background.
- Hình canonical gà do người dùng duyệt nằm trong Design Kit cũ: không được đổi ngoại hình canonical nếu chưa có đồng ý; hình Lumi trắng mới là concept phương án, không tự động thay.

II. MỤC TIÊU CHƠI ĐƯỢC
WorldMap 6 thành phố: Hội An bắt đầu, các thành phố khác khóa theo tiến trình. Chọn Hội An → CityMap background có graph node thật H0-H9, đường nối và dấu chỉ vị trí. Nhấn một điểm reachable → Lumi chuyển động dọc đường (không teleport) → tự mở event/NPC/battle/puzzle hoặc Free Exploration khi đến. Node và đường không được vẽ cố định lên PNG; phải đặt ở UI/Scene riêng theo data. Câu đố nằm RẢI RÁC, không có "khu giải đố". 
H2: Cô Mai nói chuyện và giao quest. H3: vào Free Exploration có walkable space, colliders, các đồ vật riêng, nhặt một linh kiện khi đứng gần. H4: câu đố nhẹ độc lập, có reward. H5: bot giao diện sự kiện thử. H7: sau khi có đủ đồ mở trạm; H8 sau đó mở Jeonju. Không cần hoàn tất 6 city ngay.
Camera: city map phải zoom bằng wheel/pinch, pan drag và clamp, marker/đường đi được đồng bộ khi zoom; HUD không bị zoom; đảm bảo tablet + 9:16, tránh kéo giãn nền. Camera cảnh tự do theo nhân vật trong map bounds, chỉ khi cần.
Điều khiển: 1 góc nhìn gà là đủ; giả chuyển động bằng idle/walk bounce 2-3 frames, không cần 4 sprites theo 4 hướng. Nhân vật City Map là icon nhỏ đi dọc line, Free Exploration là sprite 2D riêng. Cả hai phải có thể đổi art không đổi gameplay.

III. DỰNG MAP & TÁCH LỚP
Đối với CityMap: một ảnh background 2D duy nhất **được phép** làm nền toàn cảnh; đường node, Lumi, icon NPC, bot, dấu nhiệm vụ, trạng thái lock, fog overlay đều là các đối tượng riêng. Unity Editor script để đặt/điều chỉnh node bằng tọa độ normalized để hợp map, lưu đường đi dạng graph; không tự đoán tọa độ graph dựa trên chi tiết ảnh. Nếu đã có `Data/hoi_an_map.json` trong Design Kit, dùng làm nguồn node IDs/connections, điều chỉnh positions mới có biên bản khi cần.
Đối với Free Exploration: không giả vờ ảnh toàn cảnh đã có Collider/độ sâu riêng. Phải tạo walkable mask, layer ground/foreground/characters/interactive/occlusion, collider theo vật thể, tạo props sạch riêng hoặc dùng placeholder cho tới khi có sprite tách nền. Không được crop một ngôi nhà bị đè bởi cây/sông ra khỏi ảnh overview rồi kết luận asset đã sẵn sàng. Có thể tạo art request list cho PNG riêng, yêu cầu alpha + kích thước + góc nhìn nhất quán.

IV. NHIỆM VỤ VÀ CỐT TRUYỆN
Đọc `GAME_DESIGN_OVERVIEW.md` rồi tổ chức quest hệ thống data-driven tối giản, có state và save; đảm bảo quest item không nhặt vô hạn, boss không nhận thưởng lặp. WorldState hỗ trợ hành động Hội An làm đổi trạng thái Jeonju; Huế làm thay đổi mở tuyến đến Seoul; Đà Lạt kính và 3 miếng thịt mở đường qua sương; Gyeongju puzzle di tích; Seoul boss cuối. Không tạo 6 quest maps hoàn chỉnh cùng lúc. Tách câu hỏi / trivia thành data ngoài, có thể bổ sung về sau; hiện tại ưu tiên cơ chế khám phá và thử thách nhỏ.

V. KẾ HOẠCH TRIỂN KHAI, KIỂM THỬ
1) Kiểm tra workspace, scene, script, Editor version 6000.6.5f1 (nếu đúng), version-control / backup; xác nhận ảnh từ gói đã đọc. Tóm tắt kiến trúc + mapping ảnh → use.
2) Chuẩn hóa CityMap Hội An: chọn background, đúng graph, HUD, marker, zoom/pan, icon gà nhỏ; chưa sửa các thành phố khác.
3) Sửa luồng MainMenu → WorldMap → CityMap → H3 FreeScene → CityMap, hội thoại và item H2/H3, save.
4) Chuẩn bị NPC/Enemy/Item data và prefab placeholder có reference, sau đó từng bước nhập sprite sạch nếu có; không tuyên bố tất cả ảnh concept đều đã đưa vào game.
5) Chạy compile hoặc hướng dẫn Unity menu setup nếu không thể trực tiếp chạy Editor; báo rõ build/test nào thực sự hoàn thành. Tạo `ArtIntegrationReport.md` gồm PNG được dùng, PNG chỉ tham khảo, PNG còn thiếu, screenshot dự kiến, lỗi tồn đọng và bước cần tôi hỗ trợ.

Không viết dồn toàn bộ game vào 1 class, không bỏ code hoạt động, không tự làm lại bản đồ từ số không, không dùng AI art khác kiểu. Không làm sai luật chơi bằng cách dùng nút bấm thay gameplay. Yêu cầu kết quả có thể mở Unity Play, tự do thử Hội An và zoom/pan CityMap trước khi chuyển sang năm thành phố còn lại.
