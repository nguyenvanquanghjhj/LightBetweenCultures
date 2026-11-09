# Ngân hàng 150 câu — kiểm tra phạm vi thay nội dung

**Kết quả: 0 vị trí hỏi đáp độc lập đủ điều kiện thay; 0 câu đã thay.** Không thêm quiz, NPC, resource runtime, adapter, random, UI hoặc gameplay mới.

Đã kiểm tra source hiện tại, 6 thành phố/133 node/164 event/22 puzzle và luồng NPC. Chỉ có một puzzle dạng `choice`: H13, nhưng nó chọn mẫu Hoa từ manh mối H4, cấp Lõi Hanji và tác động tiến độ. Những puzzle còn lại là chuỗi ký hiệu/cơ cấu xoay/gấp giấy; kể cả puzzle phụ cũng thuộc nhóm tương tác phải giữ nguyên.

| Thành phố | Node puzzle hiện tại | Phân loại | ID ngân hàng mới |
|---|---|---|---|
| Hội An | H4, H13, H19 | 2 sequence, 1 choice | — giữ nguyên |
| Huế | U8, U16, U17 | 3 sequence | — giữ nguyên |
| Đà Lạt | D8, D12 | 1 sequence, 1 rings | — giữ nguyên |
| Jeonju | J3, J7, J12, J17 | 2 sequence, 1 fold, 1 rings | — giữ nguyên |
| Gyeongju | G2, G4, G12, G15, G17 | 5 sequence | — giữ nguyên |
| Seoul | S3, S10, S16, S17, S18 | 5 sequence | — giữ nguyên |

| Vị trí có lựa chọn đáp án | Câu hiện tại | Đáp án | Quyết định |
|---|---|---|---|
| Hội An H13 — puzzle_paper | Chọn mẫu giấy giữ nét Hoa từ sạp H4 | Hoa | Giữ nguyên: manh mối H4 → Lõi Hanji → tiến độ/gate H13 |

[Bảng đầy đủ từng puzzle: nội dung cũ, đáp án, lý do giữ nguyên](puzzle-audit.csv) · [Dữ liệu audit và điều kiện/reward](audit.json)

## Kiểm tra đã chạy

- **PASS cấu trúc ngân hàng:** 150 ID duy nhất, 15 chủ đề × 10 câu; 4 lựa chọn khác nhau, answerText khớp answerIndex, đủ hint/explanation. Validator thử 10 lần shuffle/câu để kiểm tra mapping. Đây là kiểm tra dữ liệu, không phải tính năng random đã tích hợp. [Log](bank-validation.txt).
- **PASS kiểm tra tĩnh:** đủ 22 event trỏ đúng puzzle/city/node, đáp án hiện tại khớp tập lựa chọn hoặc chuỗi tương ứng. Không coi đây là kiểm thử gameplay runtime.
- **PASS bảo toàn file được theo dõi:** nội dung runtime, UI, NPC asset, scene, quest, save code và ngân hàng không thay đổi; [hash phạm vi liên quan](integrity.json). Không đọc/ghi save người chơi.
- Kiểm thử đáp án mới đúng/sai, reward/save/restart: **NOT APPLICABLE**, vì không có vị trí được thay. Không chạy lại Unity/build toàn game; không dùng log runtime cũ làm bằng chứng mới.
- Không có câu ngân hàng nào được chọn nên chưa thẩm định đúng/sai kiến thức toàn bộ 150 câu; không tự sửa đáp án hoặc tuyên bố ngân hàng được duyệt nội dung.

## Quy tắc áp dụng

CODEX_APPLY_PUZZLE_BANK.md trong bộ bàn giao đề xuất tạo quiz phụ và adapter/random; yêu cầu trực tiếp mới nhất cấm thêm điểm hỏi đáp/gameplay. Vì vậy không triển khai phần mở rộng đó. Tài liệu ngân hàng cũng chỉ định giữ puzzle_paper H13, puzzle_pottery H19 và puzzle trạm/gate/cốt truyện.

Không thay câu hỏi tại NPC vốn chỉ nói chuyện, không chuyển puzzle sequence/rings/fold thành trắc nghiệm. Ngân hàng giữ ở Docs, chưa nhập vào Resources vì runtime chưa có điểm hỏi đáp độc lập phù hợp.

Checkpoint sản phẩm gần nhất đã đối chiếu: CheckpointQualityAddendum-20261017 (incremental, cần base). Không tạo checkpoint/build trùng khi không đổi production inputs; chỉ thêm hồ sơ audit trong thư mục này.
