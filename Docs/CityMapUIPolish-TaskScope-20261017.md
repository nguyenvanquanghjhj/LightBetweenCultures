# Phạm vi tiếp tục sau gián đoạn

Đã đọc toàn bộ bốn attachment gốc bằng UTF-8, không suy đoán từ tiêu đề hoặc summary.

| Thứ tự | Attachment ID | Số dòng / byte | Phạm vi và trạng thái |
|---|---|---|---|
| 1 | b97e5c86-982b-4579-ae3f-4ba2066ac918 | 200 / 9798 | POLISH CITY MAP VÀ UI: thực hiện lượt hiện tại, graph → popup/hội thoại → animation → QA/checkpoint. |
| 2 | daed50ac-e3b9-4b2e-a6cc-7cbab7dede9c | 154 / 8524 | Portrait thiếu, ghost portrait, audit gameplay 133 node; giữ nguyên trong hàng chờ sau nhiệm vụ 1. |
| 3 | 6777f4ca-29bc-4ead-9887-6a5f99990112 | 170 / 8239 | Full Story end-to-end; chưa triển khai trong nhiệm vụ 1. |
| 4 | 9f39ca52-f702-4924-b67e-a4cb08ac420c | 136 / 6627 | H3 art overhaul và rà placeholder; chưa triển khai trong nhiệm vụ 1. |

Đường dẫn attachment gốc: `C:/Users/admin/.codex/attachments/<ID>/Pasted text.txt`.
Đã giữ bản nguyên byte của cả bốn yêu cầu trong `Docs/Requests/20261017/`; `request-index.json` ghi thứ tự, đường dẫn nguồn, SHA256 và trạng thái chờ. Các bản này giúp lượt tiếp tục đọc lại đầy đủ nội dung gốc, không dựa vào summary.

Kiểm tra Story: đường dẫn chính xác `Docs/Story/FULL_STORY_PLAYTEST.md` chưa tồn tại. Có `Docs/Story/LBC_FullStory_Playtest_6Cities_133Nodes.md`, tiêu đề **FULL STORY PLAYTEST V1 (6 THÀNH PHỐ / 133 NODE)**. Chưa đổi tên, chưa suy ra hai file giống nhau. Phải đọc toàn bộ nội dung và đối chiếu canonical trước nhiệm vụ 3; nếu thiếu phần yêu cầu, báo chính xác phần thiếu.

Checkpoint nguồn: CheckpointDaLatChapter-20261017. SHA256 tất cả source/assets/config/docs/tool trong manifest khớp trước khi sửa; chỉ có cache Python ngoài manifest. Backup trước sửa: BeforeCityMapUIPolish-20261017, 1.578 file, 1.196.109.117 byte, đã kiểm tra hash sau copy.

Yêu cầu dọn backup trước đó không được tự gộp vào lượt này. Không xóa backup, không đổi thứ tự các tác vụ khác.
Yêu cầu audit chất lượng import/render đã gửi trong chat cũng chưa được thực hiện ở nhiệm vụ 1; không bị hủy hoặc gộp vào bản polish graph/UI.
