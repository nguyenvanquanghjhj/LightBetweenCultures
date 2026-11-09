# Catventure — tuyến giải cứu mèo

Mở game như cũ, vào Hội An và chọn nút **Mèo 0/6**. Nhận nhiệm vụ, lấy vật dụng trong **Túi cứu hộ**, rồi chọn thử thách đang mở. Nút **Đi tới mục tiêu** đưa Lumi đi theo đường hiện có; chỉ khi tới đúng địa điểm mới có thể chơi thử thách hoặc lấy vật dụng.

Mỗi thành phố có bốn thử thách. Hoàn thành chúng và mang đủ ba vật dụng để mở cảnh cứu hộ; kéo mèo vào giỏ trú an toàn để nhận mèo, huy hiệu, bưu thiếp và **8 xu**. Phần thưởng chỉ cấp một lần. Nút **Sổ mèo** cho xem bộ sưu tập và bưu thiếp đã mở.

| Thành phố | Mèo | Hành trình |
|---|---|---|
| Hội An | Mèo Hội An | Ghép đèn lồng, Chùa Cầu, món ăn, hoa đăng; phố sáng dần theo thử thách |
| Huế | Mèo Huế | Di tích, món Huế, Nhã nhạc và tuyến thuyền cứu hộ |
| Đà Lạt | Mèo Đà Lạt | Tìm hoa, món ăn, ghép Ga Đà Lạt và mê cung trong sương |
| Jeonju | Mèo Jeonju | Mái Hanok, xếp nguyên liệu bibimbap, ghép tên/phối màu Hanbok, tìm mèo |
| Gyeongju | Mèo Gyeongju | Cheomseongdae, Silla, Bulguksa–Seokguram và bản đồ khu vườn |
| Seoul | Mèo Seoul | Tìm biển chỉ dẫn, ghép Hangeul, lời chào và lối tới điểm trú |

Tuyến mèo mở thành phố theo thứ tự Hội An → Huế → Đà Lạt → Jeonju → Gyeongju → Seoul. Các điều kiện mở thành phố của hành trình cũ vẫn có hiệu lực. Nhật ký, NPC, di vật và các nhiệm vụ cũ tiếp tục dùng riêng; không cần xóa save hoặc chơi lại để có hệ thống mèo.

Mỗi thao tác đúng được lưu, gồm từng mảnh ghép, dấu vết tìm được, đoạn mê cung và màu Hanbok đã chọn. Có thể đóng màn chơi rồi quay lại. Thử sai hiện gợi ý và giữ phần đã làm. Nếu ghi save thất bại, thao tác đó không cấp tiến trình hoặc phần thưởng.

Bản đồ nhỏ hiển thị sơ đồ đường đang mở cùng vị trí Lumi/mục tiêu; nút âm thanh ở thanh công cụ phía dưới. Chế độ giảm/tắt hiệu ứng hiện có cũng áp dụng cho mèo và thời tiết mới.

Hình mèo trên bản đồ chỉ đánh dấu vị trí, không có vùng bấm riêng đè lên điểm đến. Dùng nút **Mèo** trên thanh đầu màn hình để mở hành trình. Các nhãn “Mèo Huế”, “Mèo Hội An”… là nhãn theo địa điểm; không phải tên riêng lấy từ PDF.

## Tài liệu và nguồn

- [Kết quả kiểm thử và giới hạn](REPORT.md).
- [Phương án tích hợp, bản khôi phục trước sửa](IMPLEMENTATION.md).
- [Nguồn văn hóa chính thức](CULTURAL_SOURCES.md).
- [Ảnh mới, nơi lưu và prompt imagegen](ART_PROVENANCE.md).
- [PDF yêu cầu gốc](Requirements-Catventure.pdf).

Để thêm/chỉnh thử thách, sửa `Assets/_Game/Resources/Content/CatRescue.json`. `Tools/author_catventure.py` là nguồn tác giả của JSON này; nếu dùng script thì sửa nội dung trong script rồi xuất lại. Chạy `Tools/validate_catventure.py` để kiểm tra dữ liệu và đường đi từ save mới. Không chạy lại các Phase generator cũ.
