# CHECKLIST TẠO ASSET THẬT CHO GAME

> Dành cho người dùng/AI ảnh/artist. Mỗi yêu cầu tạo ảnh cần xuất thành file riêng và giữ nguyên bản nhân vật chuẩn.

## A. Sprite Lumi

**Ảnh nguồn chính xác:** `References/Lumi_CANONICAL_user_image.png`.

Mô tả cần gửi AI ảnh: *“Chỉnh / dựng lại chính chú gà màu nâu cam trong ảnh nguồn thành sprite 2D top-down 3/4, giữ mồng đỏ, hình đầu, mắt tròn nâu, mỏ, áo navy viền vàng, đai xanh ngọc và đèn lồng vàng, không đổi khuôn mặt, không thay thành gà trắng. Tạo tách biệt các hướng đứng/trái/phải/lưng với cùng kích thước, light shading đồng nhất, nền trong suốt, phù hợp Unity Sprite.”*

Đòi hỏi từng đợt:
1. Front/back/left/right *turnaround* (reference).
2. 4 frame walk DOWN, chuẩn cùng pivot bàn chân.
3. 4 frame walk UP, chuẩn cùng pivot bàn chân.
4. 4 frame LEFT và RIGHT.
5. Mỗi ảnh export PNG RGBA; **kiểm tra sự nhất quán nhân dạng**, không coi sheet tổng hợp AI là sprite sheet có thể cắt sẵn.

## B. Environment — Hội An

- Một backdrop **không nhãn, không node, không đường gameplay chồng lên** cho city map. Các kiến trúc cách điệu như phố vàng, mái ngói, bến sông, cầu nhỏ, đèn lồng.
- Một bộ tile nền **seamless** (đất, đá lát, nước). Các ô tách rời đồng kích thước, có biến thể không thấy lặp quá nhiều.
- Một số props **nền trong suốt**: nhà vàng, cây, thùng, đèn lồng, hàng rào, cầu, thuyền.
- Giữ góc nhìn top-down hơi nghiêng, ánh sáng từ trên-trái và độ dày viền nhất quán với gà.

## C. NPC / bot

- Cô Mai: nữ bán đèn hoạt hình cùng góc nhìn với gà, nét Việt Nam qua bối cảnh/chi tiết trang phục hợp lý; tránh ép một loại cổ phục lịch sử cụ thể nếu chưa đối chiếu.
- Bot Mờ-01: hình tròn/khối dễ thương vừa phải, tông xám tím + lõi cam, rõ silhouette trên nền Hội An; không lấy thiết kế quái của Dynamons World.

## D. UI

- Panel hội thoại màu kem ấm + viền navy, 9-slice; ô túi đồ; nút menu; marker H0–H9.
- Text, tên NPC, node, quest nên vẽ **bằng Unity UI**, không đóng chữ vào PNG.
- Kiểm tra unicode tiếng Việt và tỉ lệ màn hình dọc.

## E. Kiểm tra trước khi nộp asset cho Codex

1. Có độ phân giải và tỷ lệ như `Data/AssetManifest.csv` hoặc tương đương đã chốt.
2. Tách riêng asset, không gộp lẫn bảng hướng dẫn / ký hiệu / chữ.
3. Nền thật sự trong suốt nếu cần; không có ô cờ giả hoặc nền trắng.
4. Mỗi frame cùng hướng nhìn, kích thước, pivot; không bị biến dạng khuôn mặt.
5. Có quyền sử dụng và thông tin nguồn / file gốc.
6. Thử tại tỷ lệ thu nhỏ trong Unity Game View.
