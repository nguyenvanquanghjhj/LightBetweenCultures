# Catventure — tích hợp theo PDF, 07/11/2026

Chủ dự án đã yêu cầu triển khai PDF trong chat ngày 07/11/2026. Phạm vi mới cho phép bổ sung tuyến giải cứu mèo sau RC1; không chạy lại các Phase generator cũ. Các kết quả nghiệm thu RC1/RC2 là lịch sử và không tự áp dụng cho bản mới.

## Khảo sát và phương án trước khi lập trình

- `GameBootstrap` quản lý sáu thành phố; `GameData`, `CityNavigation` và `CityMap` dùng graph đường đi đã duyệt. `MapDiscoveryManager` giới hạn từng chặng. Không sửa tọa độ, cạnh, node, art NPC hoặc scene hiện có.
- `NodeEventManager`, `QuestManager`, `PuzzleManager`, `RelicCollection` đã cung cấp cốt truyện, puzzle và bộ sưu tập di vật. Tuyến mèo có catalog và service riêng để không cấp nhầm các cờ truyện cũ.
- `SaveSystem` dùng JSON v2, bản dự phòng và commit nguyên tử. Bổ sung trường tùy chọn `catRescues`; save v1/v2 cũ được chuẩn hóa thành danh sách trống. Mỗi thao tác đúng lưu ngay, lỗi persist phải rollback. Phần thưởng có receipt duy nhất theo catId.
- Bốn thử thách riêng mỗi thành phố, vật dụng ở các node đã có đường đi từ stage 0, rồi thao tác đưa mèo vào chỗ trú. Dùng lại hit target và di chuyển hiện tại; nhiệm vụ chỉ thực hiện khi Lumi đã tới đúng node. Không ghi cờ mở gate cũ. Cứu mèo bổ sung quyền mở thành phố tiếp theo, song song điều kiện mở cũ.
- Tái sử dụng UI, ảnh thành phố, item và di vật. Bổ sung atlas mèo riêng; hoạt ảnh idle/tìm cứu/chúc mừng và thời tiết là lớp hiển thị không chặn input. Nội dung cứu hộ là tình huống hư cấu trong game.

## File sửa/bổ sung dự kiến

Sửa `Core/GameBootstrap.cs`, `Core/SaveSystem.cs`, `Maps/CityMap.cs` để đăng ký service, nâng tương thích save và gắn HUD. Bổ sung `Content/CatRescue.cs`, `Resources/Content/CatRescue.json`, các partial `CityMap.CatRescue` / `GameplayPanels.CatRescue`, input kéo thả và presentation mèo. Bổ sung QA riêng, không tái sinh scene hay thay các registry art cũ.

## Trình tự và kiểm chứng

Hoàn thiện Hội An trước; kiểm tra guard vị trí, kéo sai/đúng, đóng/mở, lưu từng phần, chống thưởng lặp và lỗi lưu. Khi ổn định, nạp năm chương còn lại qua cùng mô hình. Kiểm tra cấu trúc catalog, đường đi từ spawn với save mới, sáu chuỗi nhiệm vụ, reload/process restart, hồi quy save cũ và mở thành phố. Chạy Windows Player ở kích thước điện thoại và máy tính; Android device chỉ được ghi PASS khi kiểm tra thật.

Bản khôi phục trước thay đổi: `D:/GAMEKKK/Backups/CheckpointPreCatventure-20261107`. Đây là incremental, cần toàn bộ chain ghi trong manifest; đã kiểm tra SHA256. Delta trước sửa: 129 file, 41.967.862 byte; không sao chép Library, build hoặc lịch sử Validation.
