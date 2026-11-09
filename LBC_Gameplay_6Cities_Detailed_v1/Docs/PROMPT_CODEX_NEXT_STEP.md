# PROMPT GỬI CODEX — GAMEPLAY 6 THÀNH PHỐ DỰA TRÊN ẢNH MAP

Tôi có project Unity 6 LightBetweenCultures đang được bạn lập trình. ĐÃ có Design Kit v1 và bộ ảnh LBC_AnhMoi_BanGiaoCodex_20261015. Tôi gửi thêm gói **LBC_Gameplay_6Cities_Detailed_v1** chứa bản thiết kế gameplay, tọa độ, 6 ảnh overlay node/đường, quest data và enemy data. Hãy coi gói này là **thiết kế bổ sung**, KHÔNG phải lệnh làm lại project đang chạy.

**BẮT BUỘC ĐỌC THEO THỨ TỰ:**
1. Docs/FULL_LEVEL_DESIGN.md (chi tiết 6 thành phố).
2. Docs/QUEST_CHAINS_AND_GATES.md (chuỗi nhiệm vụ/soft-lock).
3. Data/world_progression.json, Data/quests.json, Data/items.json, Data/enemies.json.
4. Data/Cities/*.json, Maps/NodeOverlays/*.jpg, Maps/CoordinateGrids/*.jpg.
5. Đối chiếu các ảnh gốc Maps/<city>/Map_Overview_A.png trong bộ ảnh mới trước khi bố trí.

**GHI NHỚ:** Tọa độ x,y 0–1000, top-left ảnh Map A. Ảnh overlay là sơ đồ SƠ BỘ, CÁC ĐOẠN THẲNG CÓ THỂ CẮT QUA NƯỚC/NHÀ; phải chỉnh thành waypoint bám lối đi thật trước khi đưa vào gameplay, KHÔNG dùng làm game background vì có marker/chữ vẽ đè. Node, marker, Lumi, path, NPC và enemy là GameObject/UI độc lập; phải có zoom/pan không lệch tọa độ. Map B là cảnh phụ/reference. Một góc nhìn Lumi duy nhất.

**THIẾT KẾ:** 6 thành phố có safe nodes / branching wild encounters / boss and side quests. Hoi An H0..H9 là ID đã có, giữ tương thích với scenes và save, được phép cập nhật VỊ TRÍ trên ảnh A mới bằng adapter/version nếu cần. Các node H10+ là mới. Mỗi node có event, điều kiện mở, NPC/bot, item và edge. Không biến game thành danh sách nút bấm tức thời: character di chuyển theo tuyến, sự kiện xảy ra khi tới đích; đường qua nước cần cầu hoặc animation thuyền. KHÔNG tự động nhặt item quest chỉ bởi node có item ở gần nếu chưa vào scene/vùng tương tác.

**ƯU TIÊN TRIỂN KHAI KHÔNG LÀM LẠI:**
A. Kiểm tra kết quả phiên trước và ghi rõ scene/code đã có. Giữ prefab/gameplay/save; không xóa hoặc dựng lại vô cớ.
B. Import map Hội An A làm hình nền 2D. Đặt/điều chỉnh H0..H9, đi node theo đường, zoom/pan, giữ H3 free exploration. Đảm bảo vị trí node bám đường, không nhảy qua sông.
C. Thêm H10..H21 bằng dữ liệu và hoàn thành 1 chuỗi HQ01: H2→H3→H4/H13→H19/H5→H7, mở Jeonju/Huế. HUD quest và inventory, save/load. Triển khai H5 bot + mã hack H19, cân bằng một trận demo.
D. Nạp cấu trúc node của 5 thành phố còn lại từ JSON (có thể chỉ scaffold), thực hiện **một giao tiếp xuyên map** H7 thay đổi J17 để xác minh Cộng Hưởng Song Thành.
E. Sau khi vertical slice chạy, mới triển khai từng quest còn lại, bot và boss. Không nói “đã hoàn thành 6 map” nếu chỉ mới vẽ node, phải báo tỷ lệ runtime/event thật.

**SPRITES:** dùng sprite Lumi canonical từ Design Kit gốc (gà có trang phục xanh đậm/đai teal, đèn vàng) khi có; ảnh mặt trắng trong các bảng concept sau chỉ để tham khảo động tác, KHÔNG thay thiết kế canonical. Tách NPC/bot/item nếu PNG nguồn cho phép; không cắt bừa các chữ vào sprite, không tự nhận asset đã sạch nếu chỉ crop. Phân loại used/runtime, reference, missing-art.

**CHẤT LƯỢNG:** Đừng nối thẳng đường cắt qua nhà hoặc nước chỉ vì overlay nhìn giống. Điều chỉnh route waypoints/colliders trong Unity sau khi quan sát map ở nhiều mức zoom. Các điểm overlay là vị trí GỢI Ý, phải review. Kiểm tra BFS graph, flags, đường không soft-lock, item/reward không farm vô hạn, save cũ và Android portrait. Nếu không chạy Unity được, nói rõ chưa kiểm thử trong Editor.

**KHI BÁO CÁO:** nói rõ node thực sự hoạt động, quest đã có điều kiện/reward, NPC và sprite thật đã gắn, vấn đề còn thiếu và các scene cần người dùng tự bấm Play để nghiệm thu. Đừng cố triển khai 100+ node chỉ trong một lượt nếu sẽ làm hỏng prototype.
