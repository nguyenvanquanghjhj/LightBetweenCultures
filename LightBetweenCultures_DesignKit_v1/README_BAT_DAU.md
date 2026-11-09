# Light Between Cultures — DESIGN KIT v1

**Dành cho dự án Unity 6000.6.5f1 / Universal 2D của bạn.** Bộ này là thiết kế + dữ liệu bàn giao cho Codex, **chưa phải game hoàn chỉnh**.

## Những gì đã có thật trong bộ này

- `References/Lumi_CANONICAL_user_image.png`: **ảnh con gà chính thức** do người dùng đã duyệt trước đó. Phải bảo toàn nhân dạng và outfit. Không thay bằng gà trắng ở hình moodboard.
- `References/ArtReference_*.png`, `References/03_*.png` ... `05_*.png`: năm bảng AI gợi ý đồ họa, đều chỉ là **concept**; đôi chỗ chữ và nhân vật không nhất quán.
- `References/IndividualConcepts/`: 20 ảnh crop riêng từ bảng ý tưởng (6 thành phố, map, scene, NPC, bot, tile, UI...). **Không phải 20 tác phẩm/sprite độc lập được vẽ mới; chỉ là tham chiếu tiện xem.**
- `Concept_Gallery.png`: trang tổng quan các concept riêng.
- `Assets/Lumi/Lumi_Character_Cutout_Concept.png`: bản nền trong suốt của mascot đã duyệt để dùng thử ở UI; KHÔNG phải bộ khung chạy.
- `Assets/Markers/`: 11 icon node PNG nền trong suốt do bộ kit tạo, dùng được làm placeholder trong Unity.
- `References/GameplayPerspective_reference_from_user.png`: kiểu góc nhìn free exploration người dùng chỉ định.
- `Maps/HoiAn_Node_Flow.png` và `.svg`: sơ đồ node **chính xác theo dữ liệu** (khác hình art moodboard, có thể giao cho Codex).
- `Maps/HoiAn_H3_Grid_LevelBlueprint.png`: sơ đồ lưới hẻm H3 đúng 20×14 ô (không phải hình môi trường cuối).
- `Data/hoi_an_map.json`: 10 điểm H0–H9, tọa độ chuẩn hóa 0–1 và 11 tuyến đường.
- `Data/hoi_an_exploration_grid.json`: sơ đồ scene tự do H3 20x14 ô; đã kiểm tra tồn tại đường đi từ spawn đến vật phẩm và exit.
- `Data/hoi_an_quests.json`: nhiệm vụ thử nghiệm trước và nhiệm vụ dài hạn.
- `Data/world_cities.json`: thiết kế & liên kết 6 thành phố.
- `Data/AssetManifest.csv`: danh sách các asset theo ưu tiên và **trạng thái thực tế** (hầu hết cần làm tiếp).
- `Docs/*`: art bible, level design, quy trình dựng Unity, prompt Codex, checklist.

## Làm ngay theo thứ tự

1. **Sao lưu project Unity hiện tại hoặc tạo Git commit.** Không di chuyển/xóa `Assets`, `Packages`, `ProjectSettings`.
2. **Giải nén nguyên folder `LightBetweenCultures_DesignKit_v1` vào thư mục gốc project** (cùng cấp với `Assets`), hoặc đặt trong `Docs` của project. **Không đè bất kỳ file `.cs` nào đang có**.
3. Mở project bằng VS Code, mở Codex Agent. Gửi yêu cầu: `Đọc LightBetweenCultures_DesignKit_v1/Docs/06_PROMPT_CODEX.md và triển khai Milestone A theo README`. Nếu đặt trong `Docs`, sửa đường dẫn tương ứng.
4. Codex **phải đọc project hiện tại trước khi sửa**. Nó kiểm tra script cũ `PrototypeGame`, `HoiAnDemoSetup`, `HoiAnGameplay` và scene thực tế trên máy; ZIP nguồn mẫu chưa có scene generated, không có nghĩa workspace mới cũng thế.
5. Mở Unity Editor, kiểm tra Console + chơi thử. Chỉ khi luồng Main Menu > World Map > Hội An > H3 > nhặt linh kiện > quay về H2 hoạt động, mới bắt đầu import art cuối.

## Chú ý tránh hiểu nhầm

- Hình art board là **mẫu ý tưởng**; chưa phải bản đồ Unity hay sprite đã tách lớp. Không ném nguyên ảnh moodboard làm nền trong game.
- Màu/họa tiết chỉ là định hướng nghệ thuật. Trước khi nộp thi cần rà soát chi tiết văn hóa, quyền sử dụng hình/nhạc/asset và điều kiện sử dụng AI của BTC.
- `Data/*.json` là **hợp đồng dữ liệu thiết kế**, không phải schema Runtime đã được Unity kiểm thử. Codex cần chuyển thành ScriptableObject hoặc cấu trúc Unity-compatible phù hợp, không giả định JsonUtility parse mọi cấu trúc.
- Mục tiêu 6 thành phố được giữ, nhưng *bản có thể chơi được trước hết phải là Hội An*. Không đánh dấu 5 thành phố khác là hoàn thiện khi mới có icon.
- Không chứng nhận Unity compile, không build APK trong bộ này. Cần kiểm tra trên máy người dùng.
