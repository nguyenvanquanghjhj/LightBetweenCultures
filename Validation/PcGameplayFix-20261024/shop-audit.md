Shop dùng chung một catalog cho cả sáu thành phố; không có giá riêng theo thành phố.

| Món | Xu | Công dụng và cách dùng thật | Tiêu hao |
|---|---:|---|---|
| Ớt Hội An | 1 | Giao HQ05 ở J15; nguyên liệu HA_FOOD_PREP ở H10. Chọn giao trong hội thoại rồi xác nhận. | Theo lượng nhiệm vụ |
| Hành tím | 1 | Giao HQ06 ở U1; HA_FOOD_PREP ở H10, qua hội thoại/xác nhận giao. | Theo lượng nhiệm vụ |
| Thức ăn làm mồi | 1 | H12: lựa chọn dụ cua; G6: cho cáo ăn. DQ02 tại D10 cần ba phần. Không hồi HP/năng lượng, không uống/ăn trong trận. | 1 làm mồi; 3 giao DQ02 |
| Trà hồi phục | 2 | Nút **Uống trà** trong combat, hồi min(14, 100−HP). Tốn một lượt và có đòn đáp trả bình thường. Không hồi năng lượng. | 1 ly; đầy HP không dùng |
| Thảo dược | 2 | Giao UQ02 cho bà Nguyệt tại U2. Không phải thuốc dùng trực tiếp. | 1 khi xác nhận giao |
| Hoa đêm | 2 | Giao DQ04 tại D13. | 1 khi xác nhận giao |
| Kính Lan | 3 | Trả kính qua tương tác tại D2, phục vụ DQ01 kết thúc tại D8; không tự giải puzzle. | 1 khi xác nhận trả kính; không dùng trong combat |
| Dâu Đà Lạt | 2 | Quà DQ05, nhận việc ở D4 rồi xác nhận giao ở J15. | 1 khi xác nhận giao |

Không có nước uống hay vật phẩm hồi năng lượng khác trong catalog. Cả tám món có consumer trong source; mua trước khi mở được nơi giao không tự hoàn thành quest.

Đã sửa mô tả shop trà ghi sai rằng dùng được ngay trong túi đồ; túi chỉ đọc thông tin. Tên/mô tả thức ăn và hướng dẫn combat nêu rõ vai trò làm mồi. Giữ nguyên giá, chỉ số, AI, sát thương, save schema và receipt.

**Phí đò:** Legacy edge H17–H5 cần `hoi_an.ferry_available`. HQ04 tại H17 tiêu 5 xu một lần để sửa mái chèo, đặt flag này; shop cũ thực hiện cùng giao dịch từ xa. Graph Phase B có hiệu lực thay thế legacy routing: tuyến R1 giữa hai điểm trung chuyển gần H0/H16 mở ở stage 2, không đọc flag hoặc thu xu. Flag còn được check-in H17 sử dụng. Đã bỏ riêng món `ferry_hq04` khỏi catalog bán mới; giữ nguyên HQ04, flag, các giao dịch/receipt lịch sử và graph. Không tạo phí mới hoặc tự hoàn xu.

Nguồn: `Scripts/Core/JourneyShop.cs`, `QuestManager.cs`, `GameData.cs`, `CityNavigation.cs`, `Content/BattleManager.cs`, `UI/InventoryPanel.cs`, `GameplayPanels.cs`; dữ liệu `Resources/Content/GameplayContent.json` và `CityNavigation.json`.

Kết quả runtime nằm trong các tệp `campaign/pc-*.json`, `campaign/pc-campaign-results.txt` và `result.json`; bảng này chỉ mô tả audit source, không tự coi các mục chưa chạy là PASS.
