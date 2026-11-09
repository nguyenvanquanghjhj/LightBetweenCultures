# Light Between Cultures — Culture Moments REWORK30 (17/10/2026)

Bản mới thay cho **đề xuất chưa tích hợp** `LBC_CultureMoments_32_20261017`, KHÔNG phải thay 22 puzzle cốt truyện đang có. **Không được nói đã tích hợp hoặc gameplay PASS** khi chỉ có ZIP này.

## Tại sao viết lại?

Bộ cũ bị phản biện vì đáp án sai phi lý, kiểu "chọn điều đạo đức hiển nhiên", thiếu quan sát thật và feedback yếu. Bản mới nhấn vào manh mối/nguồn–đích/dấu vết, giữ 30 câu và 15 vị trí để đánh giá trước khi triển khai.

## File
- `REVIEW_ALL_30.md`: toàn bộ 30 câu, manh mối và đáp án để duyệt.
- `CULTURE_MOMENTS_30.json`: bản dữ liệu đọc máy, 15 moment × 2 variants.
- `CODEX_STEER_OR_TASK.md`: prompt triển khai theo giai đoạn, không tự xây 15 điểm ngay.
- `validate_bank.py`: validator độc lập, KHÔNG phải runtime test.
- `QA_ACCEPTANCE.md`: tiêu chí quyết định câu được đưa vào runtime.

## Quy tắc trọng yếu
- Trước mỗi moment người chơi PHẢI có cách xem manh mối (đã có trong gameplay, hoặc bổ sung biểu đạt rất nhỏ được duyệt). Nếu không: SKIPPED.
- **Pilot ba moment:** Hội An H2, Jeonju J15, Gyeongju G7. QA trước khi mở rộng. Không đảm bảo hiện tại những node đã có hook phụ an toàn.
- Độ ngẫu nhiên: 2 variants ở cùng moment khi **cả hai cue** được thấy; nếu chỉ 1 cue hợp lệ, chỉ dùng 1. Demo dùng deterministic; gameplay thường dùng random ổn định theo phiên chứ không đổi liên tục mỗi khi popup mở.
- 32 câu cũ / 150 câu cũ là tài liệu, KHÔNG tự xóa/hủy backup. Chỉ **không sử dụng** chúng làm nguồn triển khai; đề xuất thu dọn riêng sau khi nghiệm thu.

## Cảnh báo đặc biệt
Một số cue trong bank là ý tưởng thể hiện bằng panel/đối thoại nhỏ, **không phải sự kiện lịch sử hay asset đã được xác minh trong source**. Codex phải rà soát story/quest và xác định điều gì thực tế tồn tại trước. Không gán câu để rồi chế ra hiện vật, dấu vết, hay manh mối từ không khí.
