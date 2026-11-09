# Chốt chất lượng trước khi chạy 30 câu

Mỗi variant được tích hợp phải trả lời **CÓ** cho các tiêu chí:

1. Người chơi nhìn thấy hoặc nghe rõ manh mối **trước** câu hỏi, ở một tương tác/hiển thị xác định. Không nhét tất cả chứng cứ vào câu hỏi rồi tự nhận là đã khám phá.
2. Có thể suy ra một đáp án tốt nhất từ manh mối, không phải đoán ý người viết. Hai lựa chọn sai đều có vẻ hợp lý nếu bỏ qua một phần chứng cứ.
3. Không rò rỉ đáp án puzzle cốt truyện chưa giải, không lộ bí mật story sớm.
4. Đúng người, node, tên, vật phẩm và cờ theo runtime hiện hành. Trường `safeGateToVerify` không được dùng làm mã giả định mà không kiểm tra.
5. Có nút Bỏ qua, phản hồi sai (hint) và đúng (explanation), không làm mất reward/quest/save, không softlock.
6. Có một phản hồi có ý nghĩa: thay thoại/biểu tượng/hiệu ứng nhỏ **chỉ nếu đã có asset và không làm sai cốt truyện**; thiếu thì báo PENDING_PAYOFF, đừng gọi là visual PASS.
7. Touch Android màn hình dọc dùng được, không cắt chữ. Nếu chưa test Android thật, báo NOT_TESTED.
8. Với random, không random qua thành phố, chọn đáp án theo ID ổn định, ghi log momentId và variantId, demo deterministic để quay video, không đổi câu ngay khi đóng mở popup.
9. Đánh giá lối chơi: người mới hiểu mục tiêu trong 5–10 giây, câu hoàn thành trong 20–60 giây sau khi có cue.
10. Chỉ nhân rộng khi pilot thật sự làm game thú vị hơn thay vì kéo dài trò chơi.

**Thực tế quan trọng:** Bank này mới kiểm tra cấu trúc và biên tập thủ công. Không có Unity runtime PASS, chưa đối chiếu đầy đủ assets của người dùng trong máy họ. 
