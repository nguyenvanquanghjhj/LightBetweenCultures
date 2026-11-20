# CODEX — CULTURE MOMENTS 30 CÂU (BẢN VIẾT LẠI)

Project: `D:\GAMEKKK\LightBetweenCultures`
Input: `Docs/Puzzles/LBC_CultureMoments_REWORK30_20261017/`

Đây là bộ 30 câu MỚI, thay đề xuất Culture Moments 32 câu cũ và ngân hàng 150 câu **ở mức lựa chọn nội dung**. Không tự xóa thư mục cũ hay bất kỳ dữ liệu lịch sử nào. Không thể thay trực tiếp 22 puzzle vì chúng là sequence/rings/fold/choice liên kết quest.

## Bước 0 — kiểm tra thực tế, không tin số liệu cũ như CURRENT

Đọc `AGENTS.md`, Full Story hiện hành, các báo cáo puzzle, Quest Data, 22 puzzle và UI/dialogue hiện hành. Đọc `README_FIRST.md`, `REVIEW_ALL_30.md`, `CULTURE_MOMENTS_30.json`, `QA_ACCEPTANCE.md`. Nếu còn task đang sửa NPC/H3/quest/graphics, không đụng các file xung đột.

## Bước 1 — đối chiếu content chưa triển khai

Đối với mỗi `momentId` và `variantId`, tìm ra:
- Event/node tương tác hiện có và trạng thái quest bắt buộc.
- Chứng cứ/manh mối đã **thực sự hiện** trong gameplay hay chưa (không chỉ nằm trong JSON đề xuất).
- Có hook để thêm tương tác PHỤ không chen ngang quest bắt buộc?
- Chứng cứ nào thiếu hoặc mâu thuẫn? Đánh dấu READY, NEEDS_CONTENT_CUE, BLOCKED hoặc REWRITE.

Không tự nhận đoạn trong `evidenceRequired` là thứ người chơi đã được xem. Không lấy lời giải puzzle chính đặt trước puzzle. Không thêm NPC/node/quest/item, không đổi quest, gate, reward hay save schema.

## Bước 2 — pilot, TUYỆT ĐỐI không tự triển khai hàng loạt

Chọn pilot `CM_HA_01` tại H2 (chỉ sau hoàn thành HQ01), `CM_JJ_03` tại J15 (sau hoàn thành HQ05), `CM_GJ_02` tại G7 (chỉ sau khi giải quyết nhánh). Nếu cue chưa có, PHẢI báo rõ và đề xuất một cách trình bày cue tối thiểu an toàn (đối thoại kiểm chứng / panel xem vật chứng) trước khi động code. **Dừng xin phê duyệt** nếu việc tạo cue đòi art mới, đổi story hoặc sửa hệ thống đáng kể.

Nếu có hook và cue thực tế phù hợp, được tạo một mẫu tương tác phụ có UI touch-friendly, lựa chọn 3 đáp án theo ID, skip, hint, giải thích, phản hồi có nghĩa. Không tự set cờ chính hoặc tạo reward. QA random 1/2 chỉ khi cả hai cue hợp lệ. Chọn cố định bản demo; gameplay thường mới random (không re-roll khi popup reopen). Lưu ổn định trong phiên hoặc cache vốn có, không đổi save schema khi không cần thiết.

## Bước 3 — xác nhận trước khi mở rộng

- Tự kiểm thử một cách tái hiện được 3 pilot, câu đúng/sai/skip, reconnect UI, NPC/puzzle, save/restart, hai thứ tự nhánh (nếu liên quan), tình huống thiếu cue. Không tuyên bố Android PASS nếu chưa có device.
- Ảnh review tối đa vài ảnh mỗi node, nếu có.
- Viết `Docs/Puzzles/CULTURE_MOMENTS_30_PILOT_REPORT.md`, bảng 30 variants với trạng thái READY/NEEDS_CONTENT_CUE/BLOCKED/REWRITE, 3 pilot với runtime PASS/FAIL/NOT_TESTED; nêu rõ phản hồi thực tế.
- **DỪNG sau pilot**. KHÔNG nhân rộng cả 15 moment cho đến khi người dùng duyệt chất lượng, thấy có ích cho game.

## Phần cần tránh

- Không tự ghi rằng 30 câu đã vào runtime chỉ vì JSON valid.
- Không đánh tráo puzzle H4/H13/H19, U8/U17, J7/J12/J17, G4/G12/G7, S16/S17/S18.
- Không random khắp game, không menu quiz toàn cục, không điểm XP hay money.
- Không tự xóa 150/32 cũ trước khi có nghiệm thu pilot và người dùng cho phép thu dọn.
- Không full backup nhiều GB, không chạy lại 6,536 asserts nếu chỉ đụng UI/optional event nhỏ.
- Cẩn thận hậu quả của lựa chọn G7 sửa/đánh, không đổi nhánh và không cấp cờ kết bạn khi đánh.
