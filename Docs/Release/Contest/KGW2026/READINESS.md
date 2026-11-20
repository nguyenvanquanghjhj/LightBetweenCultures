# KGW2026 — trạng thái chuẩn bị 20/10/2026

**Có sản phẩm chạy thật và hồ sơ nháp. Chưa nhận SUBMISSION_READY hoặc bảo đảm giải thưởng.**

Thể lệ được chủ dự án dán đầy đủ trong chat. Tra cứu thêm [trang Trung tâm Văn hóa Hàn Quốc](https://vietnam.korean-culture.org/vi) trả nội dung cuộc thi trong chỉ mục tìm kiếm; [form được cung cấp](https://forms.gle/3W35yVpFYtW9iNWz6) không mở trực tiếp được bằng công cụ web ở lượt này. Không giả đã đăng ký hoặc đọc được câu trả lời form.

| Điều kiện/hạng mục | Trạng thái thực tế |
|---|---|
| Nội dung Việt Nam–Hàn Quốc, casual mobile | Game Unity hiện có; pitch định vị casual adventure, không tự nhận hyper-casual hoặc có thang điểm riêng của BTC. |
| Ý tưởng PowerPoint | Đã tạo 13 slide, ảnh art/runtime thật, notes thuyết trình; PowerPoint mở/export PDF và 13 PNG thành công. Thiếu thông tin người dự thi nên giữ DRAFT. |
| APK/video | Có APK Android build thành công; bản chốt trong `artifacts.json`. Chưa tạo video; không gọi slideshow là video gameplay. |
| Test trên máy tính | Windows Player thật, targeted QA; có gói Windows dự thi. |
| Cá nhân/nhóm tối đa 2, học sinh/sinh viên đúng đối tượng | PENDING_OWNER_CONFIRMATION. Chưa suy đoán tên, trường/ngành hoặc tư cách từ tài khoản email. |
| Đăng ký form | PENDING_OWNER_CONFIRMATION. Chưa gửi form/email ra ngoài. |
| Bản quyền tài nguyên | PARTIAL_PROVENANCE; bảng dưới. Không biến “được bàn giao” thành bằng chứng quyền tác giả. |
| Android thiết bị thật | NOT_TESTED. Packaging PASS và Windows touch simulation không thay thế nghiệm thu trên điện thoại. |

## Đã làm thật trong lượt này

- Chạy lại bài kiểm thử context help, journal, minigame thử sai/reopen và combat rút lui/thử lại/thắng: PASS trên build hiện có.
- Jeonju, Gyeongju, Seoul: hoàn thành thử thách bằng UI từ các save QA đã kiếm điều kiện thật, xem phản hồi phục dựng, re-entry không lặp celebration: PASS. Cộng với bằng chứng Hội An/Huế/Đà Lạt đã có, vẫn không coi đây là một lượt blind playtest của người mới.
- Sửa menu đang ghi Hội An dù save ở thành phố khác: đọc city/node đã lưu; nút Tiếp tục giữ đúng hành trình; hủy reset không đổi save. Sau khi reset được người chơi xác nhận, nút chuyển về Bắt đầu. Không sửa chức năng reset/save.
- Phát hiện dòng chữ trang trí `HanjiStitch` chồng nhãn mẫu: bỏ đúng dòng đó; không sửa tile, nút xoay, đáp án, điều kiện thắng, art hoặc quest. Ảnh/trial sau sửa lưu ở lượt `signature-jeonju-final`.
- Tạo PPTX, PDF, hướng dẫn demo/bảo vệ và email **dự thảo**, không gửi thay chủ dự án.

## Nguồn và quyền sử dụng cần chốt

| Nhóm | Đã biết | Còn cần trước khi nộp |
|---|---|---|
| Map, Lumi, NPC, portrait, UI, relic | Có art source index, catalog/binding và các pack do chủ dự án bàn giao. Giữ nguyên ảnh canonical. | Người tạo/công cụ/nguồn gốc, điều khoản cho phép dùng và phân phối, bằng chứng phù hợp. Không tự nhận vẽ tay. |
| 18 check-in, 12 leisure backgrounds/props | Có thư mục nguồn, manifest và runtime derivatives đã duyệt. | Cùng yêu cầu chứng minh quyền sử dụng; không lấy tên file làm bằng chứng giấy phép. |
| Bốn MP3 | Bốn file do chủ dự án cung cấp; mapping và references trong `Docs/QA/FOUR_TRACK_MUSIC_INTEGRATION.md`. | Nguồn/công cụ tạo, tài khoản/gói/điều khoản áp dụng lúc tạo, hoặc giấy phép/chấp thuận của tác giả. Chưa kiểm chứng độc lập. |
| Bảy SFX WAV | Báo cáo `SOFT_UI_SFX_INTEGRATION.md` và synthesis record ghi tổng hợp sóng gốc, không sample ngoài. | Giữ script/provenance để giải thích; chủ dự án xác nhận phần khai báo cuối. |
| Source và package | Có source Unity live và `Packages/manifest.json`. | Khai báo trung thực phần công cụ/AI hỗ trợ; giữ notices/license của dependency nếu có. Chưa có xác nhận riêng của BTC về AI trong lượt này. |

## Giới hạn và bước tiếp theo

Ưu tiên đóng băng nội dung, hoàn thiện phần demo Việt–Hàn và test thiết bị. Không xóa năm thành phố để “vừa cuộc thi”, không thêm mode giả thắng hoặc tự mở gate. Phần thể lệ được cung cấp chưa nêu trần số màn/MB; đây không phải chứng cứ mọi dung lượng đều phù hợp trải nghiệm mobile.

Vẫn còn đánh giá người mới thật, 12-leisure combined review và nghiệm thu Android/performance; không nhận COMPLETE toàn Thái Thượng Prompt. Các pose còn hạn chế không tự đổi art hoặc gọi là animation final.

Không cần chủ dự án thức thêm để dò toàn bộ game. Khi nghỉ đủ và quay lại, ưu tiên điền hồ sơ, nguồn tài nguyên và một lượt thử điện thoại có mục tiêu; báo đúng chỗ vướng. Lịch chung kết 5/12 và 6/12 đang mâu thuẫn giữa hai bản được cung cấp; cần BTC xác nhận khi tới bước chuẩn bị chung kết.
