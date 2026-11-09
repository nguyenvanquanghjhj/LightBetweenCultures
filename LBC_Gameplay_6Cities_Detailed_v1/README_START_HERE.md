# LBC – Bộ thiết kế gameplay chi tiết 6 thành phố (v1)

**Đây là bộ bổ sung MỚI** (không ghi đè ZIP đồ họa cũ hoặc Unity project). Có tổng **133 node**, **165 edge**, **24 quest**, **23 enemy**, **31 item**.

## Đọc đầu tiên

- `Docs/FULL_LEVEL_DESIGN.md`: giải thích cốt truyện, tọa độ và từng node.
- `Docs/QUEST_CHAINS_AND_GATES.md`: chuỗi nhiệm vụ từng thành phố.
- `Docs/PROMPT_CODEX_NEXT_STEP.md`: prompt bổ sung cho Codex.
- `Maps/NodeOverlays/*.jpg`: 6 hình gốc A có tuyến node vẽ CHỒNG lên để dễ thẩm định. KHÔNG phải ảnh làm nền game.
- `Maps/CoordinateGrids/*.jpg`: 6 ảnh lưới 0–1000 để hiệu chỉnh tọa độ.
- `Data/Cities/*.json`: graph, event, NPC, boss, gating từng thành phố.
- `Data/nodes_all_cities.csv`, `edges_all_cities.csv`: bảng nhập/đối soát.
- `Data/quests.json`, `items.json`, `enemies.json`, `world_progression.json`: hệ thống toàn game.

**Cách dùng:** Giải nén thư mục này ngang hàng với Assets và các bộ ảnh cũ, rồi đưa `Docs/PROMPT_CODEX_NEXT_STEP.md` cho Codex sau khi lượt đang chạy kết thúc và bạn đã kiểm thử scene trước. Dữ liệu mới là hướng dẫn THIẾT KẾ, Codex chưa chạy Unity từ bộ này.

**Điều cần lưu ý:**
1. Đường nối và vị trí node chỉ là bản vẽ GAMEPLAY khớp bố cục ảnh, chưa phải navmesh/collider đã kiểm thử. Một số tuyến phải điều chỉnh sau khi nhìn bản map phóng lớn trên màn hình dọc.
2. Các map A là minh họa giả tưởng của 6 nơi, không khẳng định tương ứng quy hoạch thật ngoài đời.
3. Mọi mặt nhân vật trong moodboard có thể khác mascot canonical; game phải giữ Lumi gốc đã duyệt.
4. Để tránh quá tải Codex/giảm rủi ro, ưu tiên Hội An complete → cross-map demo Jeonju → mở rộng lần lượt.
