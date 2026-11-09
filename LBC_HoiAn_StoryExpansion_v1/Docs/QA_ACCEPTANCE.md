# QA – Kiểm thử chương Hội An và hàng giao lưu xuyên quốc gia

## Nghiệm thu tuyến chính

1. Fresh save ở H0 chỉ hiện một vài node đầu; node sâu và marker boss phía sau ẩn, không có click/pathfinding.
2. H2 nhận HQ01 mở stage1; load lại vẫn nhớ nhận quest.
3. H3: vật phẩm phải nhặt *trong* exploration scene, chỉ một lần. Không tặng item chỉ vì đi đến node; va chạm nhà còn hoạt động.
4. H4: puzzle chỉ hợp lệ khi đúng điều kiện, sai không grant flag; đúng mới mở stage2.
5. H13 chỉ phát `hanji_core` sau khi giải điều kiện, một lần.
6. Nhánh A: H5 thắng combat phát `link_key` một lần và mở stage3. Nhánh B: H19 giải mã, đến H5 nhập mã, phát item tương đương một lần. Không cần thắng cả hai.
7. Đi vòng node H5 bằng edge trung gian, touch/zoom/pan không được lách Key Gate.
8. H7 không kích hoạt khi thiếu linh kiện hoặc chưa xử lý bot; khi đủ, mở cả Huế & Jeonju. Không bật cả sáu trạm theo cốt truyện.
9. Quay lại H2, H5, H7: NPC/hệ thống nói khác theo trạng thái, không phát lại item hoặc quest reward.

## Nghiệm thu nhiệm vụ phụ và hai nước

10. H10 nhận HQ02, H12 xử lý cua bằng combat HOẶC mồi. Dây Đồng đúng một bản chính, không phát trùng từ H11.
11. Trả H10: nhận ớt x2, hành tím x2. Kiểm tra icon và số lượng inventory, tiết kiệm save sau restart.
12. HQ05 không bắt người chơi giao ớt trước khi Jeonju mở, J15 vẫn có cửa hàng như cũ.
13. HQ06 không ngăn U1 xử lý quest thư gốc, U1 vẫn hoạt động khi Hội An mới mở xong.
14. Nếu ớt/hành đã dùng hết trong lúc HQ05/HQ06 active, H10 cấp lại đủ 1 cái; không nhân xu/XP.
15. Giao ớt ở J15, hành ở U1, quay H10 nhận bưu thiếp một lần. Thứ tự J15/U1 tùy ý; không cần để mở Seoul.
16. JQ04 và HQ03 chia sẻ đúng `reply_letter`, không cấp bản sao. H9 open nếu quest HQ02 **hoặc** clue Jeonju.
17. NPC hiển thị marker trắng `...`, NPC cần trả đồ nổi nhẹ; boss key `!` đỏ; check-in máy ảnh. Ẩn NPC/bot ở node hidden.

## Migration và các vấn đề kỹ thuật

18. Save cũ còn `lantern-lit` giữ tiến độ và đường đang mở, nhưng HQ01 không bị tự coi hoàn tất nếu chưa làm nhiệm vụ mới.
19. Đọc lại được save tại H10/H13/H19 sau exit/restart. Chưa tích hợp bản đồ Huế/Jeonju thì hook đánh dấu `NOT IMPLEMENTED`, không fake screenshot.
20. Bằng chứng là ảnh video runtime mới và log test, không chỉ JSON parse hợp lệ.

21. Người chơi có thể giải quyết H5 trước H13 (sau H4) mà HQ01 vẫn cập nhật đúng khi trở lại H13; không bắt đánh bot lần hai.
22. HQ03 vẫn nhận được ở H18 dù HQ05 đang hoạt động hoặc đã xong.
23. H17 có hai nhánh khả thi: trả 5 xu HOẶC sửa mái chèo.
