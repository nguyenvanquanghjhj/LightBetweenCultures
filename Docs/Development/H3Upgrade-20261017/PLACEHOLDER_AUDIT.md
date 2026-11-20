# Rà soát presentation / placeholder ngoài H3

Phạm vi lượt này: đại tu H3; các mục khác chỉ audit. Không suy từ chữ `placeholder` trong InputField thành một scene giả. Không đổi logic đã PASS hoặc marker/City Map đã duyệt.

| Vùng / vị trí chính xác | Hiện trạng / còn thiếu | Xử lý lượt này |
|---|---|---|
| HoiAnDemo.unity, HoiAnGameplay, House_Left/Right, Tree_Left/Right, Ground/Road | Trước là quads màu, đường chia bằng thanh và tán cây hình thoi. Nay dùng nhà/chậu/tường/thùng từ H3_02, nền đá riêng, HUD từ H3_05; sprites độc lập và phân lớp | Đã làm; vật lý cũ nguyên byte. Renderer hình học cũ ẩn để giữ neo/collider, không tuyên bố đã xóa hết chuỗi UnitSprite khỏi scene |
| HoiAnDemo, CanonicalLumiVisual / LumiStepMotion | Có sprite exploration canonical, shadow và motion cue nhẹ | Giữ nguyên. Thiếu chuỗi idle/walk vẽ riêng; không dùng portrait thoại làm walk |
| HoiAnDemo, Nguoi dan (placeholder) | Helper/trigger cũ không có NPC gameplay riêng; trước vẽ marker generic | Ẩn renderer generic; không gán Cô Mai hoặc một NPC mới vào H3. Không ghi đây là NPC hoàn thiện |
| GameplayPanels.ShowPuzzle → PuzzlePanel, PuzzleAnswer, PuzzleOption_* | Câu đố có logic và trạng thái thật, nhưng nhiều mode còn dùng clue/chọn chuỗi/ô nhập trên sheet chung, thiếu sân khấu/đạo cụ riêng | Chưa đại tu. Điểm trình bày cần nâng: H4 chưa là bảng hoán đổi ô; U8 chưa là ba cờ vật lý; D8 chưa có cảnh ba cổng; D12 dùng rings thay vì cối xay thể hiện trực tiếp; J3 chưa có thao tác nhuộm vải trực quan |
| GameplayPanels.ShowPuzzle, HanjiPaper/FoldGuide, RingVisual_* | Có gấp/xoay/ring thật; giấy dùng window art và đường hướng dẫn, vòng dùng marker Ground tái sử dụng | Cần art giấy/vòng/thiết bị riêng và phản hồi chuyển động tốt hơn; không gọi các mode này DATA_ONLY chỉ vì art đơn giản |
| GameplayPanels.ShowBattle → BattlePanel / BattleLumi / BattleEnemy / BattleLog | Đã có combat thật, HP/lượt/kỹ năng/nhánh sửa chữa. Dùng sprite tĩnh trên sheet; chưa có arena/background riêng từng nơi, chưa có attack/hit animation vẽ riêng | Chưa đổi. Hình bot/boss/marker đã duyệt giữ nguyên. Các enemy art tạm của Gyeongju/Seoul vẫn theo báo cáo Full Story |
| BattlePanel khi battle.Active=false / BattleClose | Kết quả thắng/thua hiện trong chính sheet trận, chưa là cảnh kết quả được dàn dựng riêng | Cần victory/defeat presentation và animation/âm thanh; không đổi phần thưởng hoặc luật thua |
| GameplayPanels.ShowReceipt → ItemReceipt | Đã có frame thật, icon item và motion/reading step từ lượt polish trước; không phải primitive world placeholder | Giữ nguyên. Một số item còn category icon/flat icon; danh sách theo Art catalog, không sửa để lấp tên trong lượt H3 |
| GameplayPanels.DrawDialogue / EventPortrait | Khung hội thoại và portrait transition đã PASS; không phải box text mặc định | Giữ nguyên. Bảy portrait còn thiếu theo Full Story/registry hiện hành; Ji-won không dùng mặt So-yeon |
| D18 nghỉ tại nhà trọ; các NPC chưa có portrait riêng | Rest/quest/event chạy nhưng chưa có hình chủ nhà nghỉ riêng hoặc background theo node | Chưa sản xuất art/scene mới. Không tự đổi khuôn mặt nhân vật đã chốt |
| Source backgroundId trong node/event | Kiến trúc đã có tham chiếu cho tương lai, chưa có toàn bộ background theo node | Không sản xuất mọi background trong lượt H3 |
| Prototype.unity / PrototypeGame | Entry point tương thích gọi MainMenu; các lớp ExplorationSurface/ExplorationItem cũ không tự chứng minh có scene exploration đang render | Không tạo cảnh trùng hoặc ghi sai thành scene gameplay mới. Bốn scene nền tảng hiện có là Prototype, WorldMap, CityMap, HoiAnDemo; các thành phố dùng CityMap chung |
| CityMap / WorldMap / dấu ! đỏ / marker NPC, boss | Hệ thống và art đã nghiệm thu trước; các tuyến địa hình chưa duyệt vẫn theo báo cáo terrain trước | Giữ nguyên; không tuyên bố terrain sáu thành phố PASS từ kiểm thử H3 |

Nguồn kiểm tra: scene thực tế; HoiAnDemoSetup/DesignKitSetup (builder cũ, không chạy lại); HoiAnGameplay/PlayerController/Item/InteractionSystem; GameplayPanels.ShowPuzzle/ShowBattle/ShowReceipt; PrototypeGame; Art Registry và các báo cáo Full Story. Bảng này là audit source/presentation; không phải một lượt playtest combat/puzzle mới.
