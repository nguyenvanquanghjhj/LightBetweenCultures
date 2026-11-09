# Giới hạn nghiệm thu địa hình — 17/10/2026

**Chưa đạt nghiệm thu toàn bộ tuyến theo cảnh vật.** Windows player đã xác nhận
56 lượt Huế + 60 lượt Jeonju đi hai chiều, không kẹt và bám polyline với sai số
dưới 0,02 UI unit. Kết quả đó chỉ xác nhận việc vẽ/đi dùng cùng geometry, không
chứng minh polyline nằm ngoài tất cả mái nhà, tường hoặc vùng nước.

Đã mở xem ảnh runtime ngang/dọc ở zoom2 và đối chiếu Overview A/coordinate grid.
Một số đoạn sát công trình vẫn cần hiệu chỉnh, không được gắn nhãn walkability PASS:

| Khu vực / tuyến cần duyệt lại | Vấn đề |
|---|---|
| Huế U4–U12, U12–U11 và corridor U11–U7 | Điểm nối vườn/thư khố khắc phục vòng khóa logic, nhưng nét đường sát tường thành và ranh giới vườn chưa chứng minh lối qua cổng rõ ràng; có đoạn nhìn cắt tường/công trình |
| Huế U2–U14, U14–U3 | Đường vòng qua khu dân cư và cầu dưới; các đoạn sát mái/quanh bờ cần review từng waypoint trên ảnh gốc |
| Jeonju J1–J3, J3–J4, J4–J7, J17–J12/J19 | Các đường giữa sân/hẻm bị mái hanok che theo phối cảnh; ảnh runtime có đoạn nhìn cắt mái, cần author lại theo đường phố nhìn thấy rõ |
| Jeonju các tuyến quanh J0/J10/J16 | Giao nhiều bờ/khu nhà, chưa đủ bằng chứng tất cả đoạn tránh nước và công trình |

Chuyến đò U3–U4 dùng travel=ferry, có boat visual và không có bóng chân đi bộ;
hai ảnh runtime outbound/return được chụp giữa chuyển động. Không dùng chế độ đò
để hợp thức hóa mọi đường chưa rõ. Những tuyến còn lại không được tự đổi sang ferry.

Anchor mở đầu U0/U1/J0/J15 giữ đúng checkpoint trước. Anchor nguồn còn lại có cờ
coordinates_are_approximate và đã căn thử theo ảnh; đây vẫn là phần author cần duyệt,
không phải tọa độ địa lý chính thức. Tất cả cũ/mới nằm trong
[HueJeonjuChapterRoutes.json](HueJeonjuChapterRoutes.json). Hai CityDefinition vẫn
giữ routesReviewed=false. Không thay bản đồ, vẽ thêm cổng hoặc xóa các đoạn khó
chỉ để báo PASS. Cần review waypoint/anchor trong Editor trước khi nghiệm thu visual.

Hội An giữ nguyên route checkpoint đã có, kể cả những giới hạn đã ghi trong
[CityMapNaturalReport](../../CityMapNaturalReport.md); lượt này không âm thầm sửa H0
hoặc các anchor đã được yêu cầu bảo toàn.
