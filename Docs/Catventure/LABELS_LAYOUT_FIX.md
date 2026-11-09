# Sửa tên mèo và vùng bấm — 07/11/2026

Các tên Đốm, Mây, Thông, Bori, Sao, Haneul do Codex tự đặt khi triển khai, không phải tên lấy từ PDF. Theo phản hồi của chủ game, đã bỏ toàn bộ tên riêng này khỏi nội dung đang chạy. Bộ sưu tập dùng nhãn theo thành phố (Mèo Huế, Mèo Hội An…), ô đích mê cung chỉ ghi **Mèo**.

Nút mèo phụ trên bản đồ trước đây có vùng bấm 44×48, lệch 30×30 so với điểm đến 52×52, tạo vùng đè lên nhau. Đã thay bằng hình mèo không nhận raycast, lệch ngang 60 đơn vị giao diện và giữ khoảng cách khi zoom. Điểm đến cũ giữ quyền nhận thao tác; nút Mèo trên thanh đầu màn hình mở hành trình. Âm thanh ở thanh dưới; kiểm tra hình chữ nhật màn hình xác nhận nút Mèo/Âm thanh không đè nút khác ở cả hai kích thước.

## Bản chạy hiện tại

- [Windows Release 1.1.1](../../Builds/Windows-Catventure-Fix-Release-20261107/Catventure.exe), giữ thư mục Data và DLL cạnh EXE.
- [Android APK 1.1.1, code 21](../../Builds/Android/Catventure-Fix-20261107.apk), cùng package và signer với bản code 20.
- [Ảnh mê cung Huế](../../Validation/Catventure-LabelsLayout-20261107/portrait-final/CAT-mode-maze.png) · [Thanh nút/bản đồ Huế](../../Validation/Catventure-LabelsLayout-20261107/portrait-final/CAT-Hue-map-controls.png).

## Kiểm tra bản sửa

| Lượt Windows Player | Kiểm tra | Kết quả |
|---|---:|---|
| portrait-final | 285 | PASS |
| desktop-final | 285 | PASS |
| save-upgrade-final | 3 | PASS |

Hai lượt đi đủ sáu thành phố và 24 thử thách ở 540×960 và 1280×720, có kiểm tra nhãn đích mê cung, vùng bấm thanh công cụ, hình mèo không chặn input, thao tác chuột/cảm ứng mô phỏng, lưu và thưởng một lần. Lượt save-upgrade nạp bản sao save QA của bản 1.1 trong process mới; không dùng save của chủ game. ID mèo/nhiệm vụ/vật dụng, thứ tự, điểm đến và số thưởng không đổi. Logic save và cứu hộ không sửa.

Đã kiểm tra thêm zoom 1×, 2×, 4×: animation chỉ chạy trên hình con, không ghi đè tỷ lệ của khung mèo. Các thư mục `portrait`, `desktop`, `save-upgrade` giữ lượt trước lần sửa zoom cuối, không cộng vào kết quả trên.

Windows Development, Release và Android build thành công. APK qua chữ ký, package/version, căn chỉnh 16 KB và CRC. **Chưa kiểm tra trên điện thoại thật**. Kết quả 633 Player/514 domain trong báo cáo trước thuộc bản 1.1, không cộng vào lượt này.

## Khôi phục

Base trước sửa: `Backups/CheckpointCatventure-20261107`. Bản sau sửa: `Backups/CheckpointCatventureLabelsLayout-20261107`, chỉ chứa file nguồn/tài liệu thay đổi; **cần base chain, không phải backup độc lập**. Không xóa hay ghi đè backup, bản chạy và bằng chứng cũ. Xem [manifest lượt sửa](../../Validation/Catventure-LabelsLayout-20261107/summary.json).
