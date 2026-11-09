# KIỂM THỬ GAMEPLAY — HỘI AN

Mốc bắt buộc trước khi báo đã xong bản Hội An:

| ID | Kiểm thử | Kết quả kỳ vọng |
|---|---|---|
| T01 | Mở game | Menu hiện, nhấn Bắt đầu được |
| T02 | World Map | 6 thành phố, chỉ Hội An mở theo thiết kế phase 1 |
| T03 | Click H0→H2 | Lumi đi qua cạnh H0–H1–H2, không teleport |
| T04 | Click H8 | Không đi qua cổng Jeonju đang khóa |
| T05 | Node H9 | Không nhìn thấy trước khi mở bí mật |
| T06 | H2 hội thoại | Cô Mai giao nhiệm vụ một lần, thay lời thoại đúng trạng thái |
| T07 | H3 scene | Đi vào scene tự do và nhìn thấy Lumi trong camera portrait |
| T08 | Click qua nhà/nước | Gà đi vòng hoặc không di chuyển, tuyệt đối không đi xuyên |
| T09 | Click linh kiện từ xa | Nhân vật đến gần mới nhặt; không thể nhặt ngay khi đứng xa |
| T10 | Nhặt hai lần | Số lượng chỉ +1 một lần |
| T11 | Rời H3 / vào lại | Item không mọc lại, vị trí trở lại đúng H3 |
| T12 | Quay về H2 | Giao thành công và đèn nhỏ được khôi phục |
| T13 | Thoát Play / mở lại | State save còn đúng theo phương pháp thiết kế; có Reset New Game |
| T14 | PC & Android | Click chuột / chạm màn hình đều dùng được |
| T15 | Console | Không có lỗi biên dịch, NullReference, Scene not found |
| T16 | Source / scene | Không làm mất scene cũ, chưa ghi đè asset của user |

**Lưu ý:** Test trên Game View portrait (9:16) và trên một điện thoại Android thật trước APK cuối. Chỉ code review không chứng minh T01–T16 đã qua.
