# Sáu thành phố — cấu trúc hành trình (chưa phải sáu map đã làm)

Sáu địa danh: **Hội An, Huế, Đà Lạt / Jeonju, Gyeongju, Seoul**. Mỗi thành phố là phiên bản game cách điệu, không phải mô phỏng địa lý chính xác.

## Hệ thống mở khóa

- `Hội An` mở sẵn; hoàn thành trạm sáng mở **Jeonju** và **Huế**.
- Hoàn thành nhiệm vụ Jeonju mở **Gyeongju**.
- Hoàn thành nhiệm vụ Huế mở **Đà Lạt**.
- Mở **Seoul** sau khi hoàn thành ba dấu mốc trải khắp hai nước: bot gác thành Huế, trạm cộng hưởng Gyeongju và sương mù Đà Lạt.
- Hệ thống nhiệm vụ phụ có thể hỗ trợ giải boss/tìm bí mật ở thành phố khác. Không đặt trạng thái chỉ trong scene cục bộ.

## Thiết kế từng vùng

### Hội An — mở đầu, phố đèn
10 điểm; NPC cô Mai; đi hẻm tìm mắt đèn; puzzle hoa văn ở chợ; bot bến gỗ; khôi phục đèn mở cổng Jeonju. Ánh vàng và xanh nước sông. **Ưu tiên P0**.

### Jeonju — xưởng thủ công
6 điểm dự kiến: cổng vào, phố hanok, sân xưởng, nhà nghệ nhân, con hẻm, cổng Gyeongju. Một nghệ nhân cần người chơi ghép ba manh mối hoa văn bị xáo; vài manh mối chỉ hiện nếu từng nhặt mảnh ký ức từ Hội An. Cảnh tự do là xưởng thủ công, không nhồi bot vào mọi điểm.

### Huế — thành và sân vườn
7 điểm: cổng, lối đá, vườn, sân, hồ, trạm cổ, cổng Đà Lạt. Một bot gác khóa đường chính; người chơi có thể tìm cơ chế vô hiệu hóa lớp khiên trước khi chiến đấu. Boss gắn với đường mở tới Seoul; không bắt buộc mô tả mọi địa điểm như di tích có thật.

### Gyeongju — di tích và ánh sáng
6 điểm: cổng, hồ, hành lang, vườn, đài cộng hưởng, lối ra. Tập trung giải trình tự biểu tượng theo các mảnh gợi ý; dùng lại logic puzzle có tham số thay vì viết một game riêng.

### Đà Lạt — rừng và sương mù
8 điểm: cổng, đường thông, lều, NPC mất kính, vệt sương, chỗ thú canh, khu câu đố, cổng ra. NPC mất kính giúp dẫn đường qua sương mù; thú cần 3 phần đồ ăn kiếm từ các nhiệm vụ rải rác. Đây là **hai nhiệm vụ có thể làm theo thứ tự linh hoạt**, không đặt vào khu giải đố cố định.

### Seoul — đêm và bot cuối
7 điểm: trạm đến, phố sáng đèn, ngã rẽ, trạm pin, đoạn hầm, phòng điều khiển, quảng trường kết. Tổng hợp các cờ đã hoàn thành; đối đầu bot cuối qua puzzle + chiến đấu cơ bản; kết thúc bằng ánh sáng kết nối hai thế giới. Đồ họa đô thị khác hẳn năm vùng còn lại.

## Giới hạn chất lượng

- **Một thành phố playable > sáu ảnh thành phố có khóa**. Bản sơ loại có thể mô tả tầm nhìn sáu thành phố, nhưng không được giới thiệu tính năng chưa tồn tại là đã hoàn thành.
- Nếu muốn đủ sáu map cho bản nộp, rút số điểm và nhiệm vụ các map phụ, tái sử dụng hệ thống chung, nhưng vẫn cần mỗi map có thử thách thực sự hoạt động.
- Văn hóa phải được tích hợp qua bối cảnh, sự kiện, lời thoại và nhiệm vụ; không chỉ cắm cờ quốc gia. Tham khảo/kiểm chứng họa tiết, tên riêng trước khi phát hành.
