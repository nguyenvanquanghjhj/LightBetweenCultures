LBC — NGÂN HÀNG 150 CÂU ĐỐ TỔNG HỢP
==================================

BA FILE CHÍNH
1. LBC_GENERAL_PUZZLE_BANK_V1.json — 150 câu đố đã viết bằng tiếng Việt,
   15 thể loại x 10 câu, 4 lựa chọn/câu, đáp án + gợi ý + giải thích.
2. CODEX_APPLY_PUZZLE_BANK.md — prompt đầy đủ cho Codex tích hợp vào Unity.
3. LBC_PUZZLE_INDEX.csv — bảng câu hỏi/đáp án dễ mở với Excel để soát.
4. validate_bank.py — script kiểm tra cấu trúc, ID, đáp án và shuffle.

CÀI ĐẶT
- Giải nén cả thư mục vào:
  D:\GAMEKKK\LightBetweenCultures\Docs\Puzzles\LBC_Puzzle_Bank_20261017\
- Gửi prompt cho Codex: "Đọc CODEX_APPLY_PUZZLE_BANK.md và
  LBC_GENERAL_PUZZLE_BANK_V1.json trong thư mục vừa giải nén.
  Chỉ triển khai sau khi nhiệm vụ đang chạy tới checkpoint an toàn."
- Không trực tiếp thay thế `GameplayContent.json`: game có những puzzle
  gắn gate/quest cần giữ nguyên đáp án và flag.

15 CHỦ ĐỀ
logic: suy luận
patterns: dãy số, quy luật
math: toán cơ bản
nature: tự nhiên
science: khoa học
vietnam: di sản Việt Nam
korea: văn hóa Hàn Quốc
world: địa lý thế giới
language: ngôn ngữ
technology: công nghệ
everyday: tình huống đời sống
riddles: câu đố mẹo
cipher: mật mã
spatial: tư duy không gian
mystery: giải mã manh mối

MẪU ĐỊNH DẠNG
id: ID duy nhất, ổn định
category: nhóm chủ đề
difficulty: 1, 2, 3
question: đề bài
options: 4 phương án
answerIndex: chỉ số 0-based trong options ban đầu
answerText: câu trả lời chính xác, dùng để bảo vệ logic khi shuffle
hint: gợi ý
explanation: giải thích sau khi trả lời

QUAN TRỌNG
- Đây là ngân hàng câu đố bổ sung OPTIONAL, không thay thế
  các bài giải mã trạm, gate, puzzle bắt buộc của 6 thành phố.
- Các dữ kiện, câu hỏi và đáp án đã được biên soạn và kiểm tra cấu trúc.
  Vẫn cần thử UI/Unity, soát nội dung và nghiệm thu thực tế.
