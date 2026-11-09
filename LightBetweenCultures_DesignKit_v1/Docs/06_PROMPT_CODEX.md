# CODEX HANDOFF — LIGHT BETWEEN CULTURES (DESIGN KIT v1)

Bạn đóng vai Senior Unity 2D engineer/technical artist tích hợp bản DesignKit đã được người dùng duyệt ý tưởng vào PROJECT UNITY HIỆN CÓ, không làm lại project. Hãy giao tiếp tiếng Việt, ngắn gọn nhưng nêu rõ kết quả và các thao tác Unity cần người dùng làm.

## Nguyên tắc bắt buộc
1. Project đã có code Copilot, Unity 6000.6.5f1/Universal 2D. **Đọc và đối chiếu workspace hiện tại** trước khi sửa; bản ZIP từng cung cấp chỉ là snapshot cũ, không được coi là trạng thái mới nhất.
2. Kiểm tra file/scene hiện có, Git status. Tạo commit/backup nếu môi trường cho phép. Không xóa Assets, Packages, ProjectSettings; không ghi đè scene đã có mà chưa xác minh.
3. BỘ KIT ở thư mục `LightBetweenCultures_DesignKit_v1/` hoặc `Docs/LightBetweenCultures_DesignKit_v1/` nằm cạnh Assets trong workspace. Nếu không có, báo vị trí cần giải nén, đừng bịa nội dung.
4. Đọc theo thứ tự: README_BAT_DAU.md → Docs/04_UNITY_INTEGRATION.md → Data/hoi_an_map.json → Data/hoi_an_exploration_grid.json → Data/hoi_an_quests.json → Data/world_cities.json → Data/AssetManifest.csv → Docs/01_ART_BIBLE.md.
5. `References/` và `References/IndividualConcepts/` chứa **art direction**. Trên moodboard có gà trắng/biểu tượng không thống nhất: **không thay con gà chính thức**. Canonical duy nhất là `References/Lumi_CANONICAL_user_image.png` (gà màu nâu vàng, áo xanh thẫm, đai xanh, đèn vàng). Có ảnh nền trong suốt ở `Assets/Lumi/Lumi_Character_Cutout_Concept.png` dùng cho giao diện/portrait nếu cần, chưa phải spritesheet di chuyển.
6. `Assets/Markers/*.png` là icon tách riêng có alpha, có thể import thành Unity Sprite 2D. `Maps/HoiAn_Node_Flow.png` là flow chính xác; có chữ nên KHÔNG được gắn làm background thành phố. Các ảnh trong `IndividualConcepts` là crop tham khảo, KHÔNG phải final art/sprite riêng dùng cho gameplay.
7. Không sao chép map, đồ họa, UI, source hoặc nhân vật của Dynamons World. Chỉ học cách tổ chức node khám phá và hệ thống sự kiện.
8. Phải tạo scene `.unity` thực sự (và prefab/data) trong Unity, không chỉ tạo C# không chạy. Phân biệt verify code vs Unity Editor vs Android APK, không tự nhận đã kiểm thử khi chưa làm.

## Milestone A — tích hợp gameplay trước, không mở rộng phạm vi
A1. Rà soát `PrototypeGame`, `PrototypeSetup`, `HoiAnDemoSetup`, các `HoiAnGameplay`, `HoiAnPlayerController`, `HoiAnNpc`, `HoiAnItem` và scene đã sinh ra. Tìm cách **nối các phần đang có** thay vì nhân đôi hai hệ thống.
A2. Main Menu → World Map (6 city nodes, Hội An mở) → Hội An City Map → H3 hẻm tự do → trở về City Map đúng vị trí H3 → H2 cô Mai hoàn thành nhiệm vụ. Test luồng.
A3. Dùng `Data/hoi_an_map.json` tạo graph H0–H9 (10 điểm, 11 cạnh), giữ id cố định. Dùng BFS tìm tuyến được phép; icon gà di chuyển theo tuyến, không teleport xuyên node; chỉ kích hoạt event tại đích. Node khóa/ẩn tôn trọng flags. Normalized coordinates: top-left; UI anchor `x=(nx-.5)*width`, `y=(.5-ny)*height`. Thành phố H8 khóa đến khi đủ điều kiện.
A4. Xây/Hợp nhất H3 Free Exploration theo `grid_rows` 20x14. Legend trong JSON xác định tường, nước, nhà, thùng. Click/touch yêu cầu A* hoặc BFS tránh vật cản **toàn tuyến**. Nhặt item phải ở gần, không click từ xa để nhận tức thì. Item chỉ nhặt một lần. Cửa ra đưa về H3.
A5. Quest slice trước: NPC cô Mai ở H2 yêu cầu `lantern_lens`; người chơi nhặt item H3 rồi trở về hoàn thành; đèn nhỏ ở tiệm thay đổi trạng thái và lưu game. Trạng thái save phải sống qua scene/restart. `hoi_an_quests.json` cũng mô tả main quest dài hạn H4/H5/H7 để sau, không bắt phải đánh boss ở phiên bản slice.
A6. Dùng `Assets/Markers` tạm làm node icons; tạo layer/sorting/canvas/safe-area phù hợp mobile portrait, hỗ trợ chuột PC. Khi cần duyệt visual, dừng và chỉ rõ screenshot/gameplay cần người dùng cung cấp.
A7. Viết editor menu `Tools/Light Between Cultures/Build Hoi An Vertical Slice` có thể rerun an toàn: không xoá scene/assets thủ công, đánh dấu object do script quản lý, tránh duplicate. Tạo và lưu scene hợp lệ, cấu hình scene build KHÔNG được xoá scene khác.
A8. Console biên dịch sạch; có kiểm thử logic đường đi/nhiệm vụ theo khả năng. Nếu không thể chạy Unity trong môi trường Codex, ghi rõ hướng dẫn người dùng test ở Editor. Không chuyển qua 5 thành phố còn lại khi chưa được xác nhận.

## Milestone B — art integration sau khi A đã chạy
B1. Dựng một CityMap Hội An đẹp hơn bằng ảnh nền thành phố mới **không chứa chữ, node, icon** (người dùng/AI art sẽ bổ sung), overlay node và edges bằng Unity; asset replace được mà không sửa logic.
B2. Tách và dùng sprite nhân vật thật có 4 hướng + idle/walk; đừng dùng một ảnh chân dung toàn thân làm animation đi bộ.
B3. Dựa `Data/AssetManifest.csv` để báo asset còn thiếu và spec import (Pixels Per Unit, Sprite Mode, pivot, sorting, collider). Không fake assets hoàn thiện.
B4. Hoàn thiện một cảnh H3 đẹp, có 2 nhà, đèn, cây, thùng, nước, NPC, item đặt trên grid chơi được. Ưu tiên đồng nhất tỉ lệ/palette hơn là nhiều props.

## Cách báo cáo
- (1) Đã đọc file nào / project hiện trạng nào;
- (2) Đã sửa/tạo file & scene nào;
- (3) Đã xác minh được gì, còn gì CHƯA test;
- (4) Cách mở Unity / thao tác menu / nhấn Play / test cảm ứng;
- (5) Bug và việc còn tồn tại;
- (6) Không tự chuyển sang milestone khác.

**Hãy đọc project và bộ kit, nêu kế hoạch tối đa 10 dòng, sau đó thực thi MILESTONE A. Không chỉ lập danh sách TODO.**
