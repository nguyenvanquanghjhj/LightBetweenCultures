# Culture Moments 30 ? Content Review

Ph?m vi: ch?nh n?i dung v? metadata; kh?ng t?ch h?p Unity runtime. B? REWORK30 ???c d?ng l?m b?n hi?n h?nh thay cho b? 32 trong y?u c?u.

## Th?ng k?

| Tr?ng th?i | S? variant | ? ngh?a |
|---|---:|---|
| KEEP | 16 | Gi? n?i dung sau r? so?t; c?n ki?m tra cue/runtime |
| REVISED | 12 | ?? tinh ch?nh ?? t?ng suy lu?n/b?n s?c th?nh ph? |
| NEEDS_RUNTIME_CONTEXT | 2 | Ch?a x?c nh?n ?? cue tr?c quan trong runtime |
| REJECT_CANDIDATE | 0 | Kh?ng c? c?u b? ?? ngh? lo?i ? l??t n?y |

## B?ng t?ng moment

| Moment | Node | Tr?ng th?i | L? do | Manh m?i c?n x?c minh | Feedback |
|---|---|---|---|---|---|
| CM_HA_01 | hoi_an:H2 | REVISED | ?? tinh ch?nh ?? gi?m ??p ?n hi?n nhi?n v? t?ng suy lu?n theo manh m?i. | hoi_an:H2, HQ01, lamp_restored | PROPOSED, ch?a runtime |
| CM_HA_02 | hoi_an:H4 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | hoi_an:H4, puzzle_market, Hoa | PROPOSED, ch?a runtime |
| CM_HA_03 | hoi_an:H18 | REVISED | ?? tinh ch?nh ?? gi?m ??p ?n hi?n nhi?n v? t?ng suy lu?n theo manh m?i. | hoi_an:H10, postcard, jeonju_exchange | PROPOSED, ch?a runtime |
| CM_HU_01 | hue:U8 | REVISED | ?? tinh ch?nh ?? gi?m ??p ?n hi?n nhi?n v? t?ng suy lu?n theo manh m?i. | hue:U3, npc_tam, river_crossing | PROPOSED, ch?a runtime |
| CM_HU_02 | hue:U17 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | hue:U8, seal_evidence | PROPOSED, ch?a runtime |
| CM_DL_01 | da_lat:D2 | REVISED | ?? tinh ch?nh ?? gi?m ??p ?n hi?n nhi?n v? t?ng suy lu?n theo manh m?i. | da_lat:D2, npc_lan, mist_route | PROPOSED, ch?a runtime |
| CM_DL_02 | da_lat:D10 | REVISED | ?? tinh ch?nh ?? gi?m ??p ?n hi?n nhi?n v? t?ng suy lu?n theo manh m?i. | da_lat:D10, hungry_dog, food_interaction | PROPOSED, ch?a runtime |
| CM_JJ_01 | jeonju:J2 | REVISED | ?? tinh ch?nh ?? gi?m ??p ?n hi?n nhi?n v? t?ng suy lu?n theo manh m?i. | jeonju:J2, npc_hana, pattern_clue | PROPOSED, ch?a runtime |
| CM_JJ_02 | jeonju:J7 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | jeonju:J7, hanji_fragment, fold_clue | PROPOSED, ch?a runtime |
| CM_JJ_03 | jeonju:J15 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | jeonju:J15, DQ05, HQ05 | PROPOSED, ch?a runtime |
| CM_GJ_01 | gyeongju:G3 | NEEDS_RUNTIME_CONTEXT | ? t??ng ph? h?p nh?ng ch?a th? x?c nh?n ng??i ch?i c? ?? cue tr?c quan trong runtime. | gyeongju:G3, min, artifact_records | PROPOSED, ch?a runtime |
| CM_GJ_02 | gyeongju:G7 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | gyeongju:G7, repair_or_fight_branch, statue_state | PROPOSED, ch?a runtime |
| CM_SL_01 | seoul:S2 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | seoul:S2, SQ02, document_evidence | PROPOSED, ch?a runtime |
| CM_SL_02 | seoul:S7 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | seoul:S7, relay_readiness, cross_city_artifacts | PROPOSED, ch?a runtime |
| CM_SL_03 | seoul:S18 | KEEP | N?i dung ??t ti?u ch? bi?n t?p; v?n c?n x?c minh cue v? feedback trong runtime. | seoul:S18, relay_state, cross_city_sources | PROPOSED, ch?a runtime |

## 8 variant ?u ti?n cho pilot/demo

1. `CM_HA_01_V1` ? ?i?u ch?nh g?c treo ??n d?a tr?n v?ng b?ng v? h??ng t? b?n.
2. `CM_HA_02_V1` ? gi? n?t g?c khi v? ??n, g?n v?i H4 nh?ng kh?ng thay puzzle.
3. `CM_HA_03_V1` ? ch?n d?u hi?u k? ??ng c?u chuy?n H?i An.
4. `CM_JJ_02_V1` ? gi? m?nh Hanji c? v? vi?t l?i ch?o tr?n m?nh m?i.
5. `CM_JJ_03_V1` ? gh?p ngu?n d?u ?? L?t v?i l?i ch?o Jeonju.
6. `CM_GJ_02_V1` ? ghi l?i d?u v?t theo nh?nh s?a/??nh t??ng ?? c?.
7. `CM_SL_01_V1` ? ??i chi?u h? s? theo m?c th?i gian v? l?i nh?n g?c.
8. `CM_SL_02_V1` ? x?c minh ?? ngu?n t?n hi?u tr??c khi n?i relay.

### CM_HA_01_V1 ? Đèn sáng cho cả phố
- **T?nh hu?ng/manh m?i:** Đối thoại Cô Mai chỉ hai vị trí treo trên mô hình: sát tường và đầu mái hiên, với vùng bóng đổ.
- **C?u h?i:** Đèn đã được sửa nhưng lối từ bến còn khuất. Điều chỉnh nào dùng lại chiếc đèn mà chiếu được lối đi?
- **Ph??ng ?n:** A. Giữ vị trí cũ, tăng thêm một lớp giấy khuếch tán.; B. Đưa đèn ra đầu mái hiên và xoay mặt sáng về phía bến.; C. Đổi đèn sang mái phía trong nhưng giữ độ cao hiện tại.
- **??p ?n:** B
- **L? do:** Mái hiên vẫn che mưa, còn đèn không bị tường chắn góc nhìn từ bến.
- **Tr?ng th?i:** REVISED; feedback PROPOSED_NOT_RUNTIME_INTEGRATED; cue c?n x?c minh: hoi_an:H2, HQ01, lamp_restored

### CM_HA_02_V1 ? Dấu vết của người thợ
- **T?nh hu?ng/manh m?i:** Hiển thị 3 mô tả mảnh A/B/C qua giao diện quan sát, không thay puzzle H4/H13.
- **C?u h?i:** Nếu phải vá góc thiếu mà vẫn giữ nét gốc làm chuẩn, thợ nên chọn mảnh nào?
- **Ph??ng ?n:** A. Mảnh sao lại toàn bộ hình mới, bỏ phần gốc bị nhòe.; B. Mảnh giữ đúng hướng nét HOA của bản gốc và chỉ vá phần mất.; C. Mảnh đẹp nhất ở ngoài sạp, nhưng nét HOA bị soi gương.
- **??p ?n:** B
- **L? do:** Bảo tồn đặc điểm nhận dạng là hướng nét gốc; vá cục bộ không thay cả mẫu.
- **Tr?ng th?i:** KEEP; feedback PROPOSED_NOT_RUNTIME_INTEGRATED; cue c?n x?c minh: hoi_an:H4, puzzle_market, Hoa

### CM_HA_03_V1 ? Lá thư nối Hội An và Jeonju
- **T?nh hu?ng/manh m?i:** Cho xem 2 mẩu thư/biểu tượng đã thu được ở J21 và H18 trước câu hỏi.
- **C?u h?i:** Hai lá thư khác ngôn ngữ và thời điểm. Dấu hiệu nào giúp xác nhận chúng thuộc cùng một cuộc trao đổi?
- **Ph??ng ?n:** A. Cùng kích thước phong bì, dù dấu niêm khác nhau.; B. Cùng dấu ký hiệu và người nhận, dù nội dung lời chào khác.; C. Cùng màu mực, dù nơi gửi không khớp.
- **??p ?n:** B
- **L? do:** Dấu nhận diện và người nhận liên kết hai thư chắc chắn hơn đặc điểm hình thức.
- **Tr?ng th?i:** REVISED; feedback PROPOSED_NOT_RUNTIME_INTEGRATED; cue c?n x?c minh: hoi_an:H10, postcard, jeonju_exchange

### CM_JJ_02_V1 ? Hanji và nét gấp
- **T?nh hu?ng/manh m?i:** Cho xem hai lỗi trên hình mẫu và tờ chuẩn đã hoàn thành.
- **C?u h?i:** Để có cả tâm và vạch khớp, nên ưu tiên kiểm tra gì?
- **Ph??ng ?n:** A. Đổi giấy dày hơn, giữ nếp gấp y nguyên.; B. Kiểm tra lại hướng căn vạch trước khi ép nếp gấp cuối.; C. Kẻ thêm vạch mới đúng vị trí nếp đang lệch.
- **??p ?n:** B
- **L? do:** Căn hướng vạch trước khi gấp giúp tránh giữ nếp sai; đổi giấy hay vẽ thêm làm lệch mẫu gốc.
- **Tr?ng th?i:** KEEP; feedback PROPOSED_NOT_RUNTIME_INTEGRATED; cue c?n x?c minh: jeonju:J7, hanji_fragment, fold_clue

### CM_GJ_02_V1 ? Dấu vết trên tượng
- **T?nh hu?ng/manh m?i:** Cho nhìn 2 lớp vết đánh dấu rõ và phản hồi phù hợp nhánh, không thay kết quả G7.
- **C?u h?i:** Muốn xác định dấu nào xuất hiện trước sự cố ánh sáng, chứng cứ nào đáng tin nhất?
- **Ph??ng ?n:** A. Vết có màu đậm nhất vì đậm là cũ.; B. Vết nằm cao hơn vì ở trên tượng thường xuất hiện trước.; C. Vết nứt dưới lớp nhiễu mới vì hai lớp có quan hệ chồng phủ.
- **??p ?n:** C
- **L? do:** Quan hệ lớp phủ cho biết thứ tự tương đối; màu hay vị trí cao thấp không đủ chứng cứ.
- **Tr?ng th?i:** KEEP; feedback PROPOSED_NOT_RUNTIME_INTEGRATED; cue c?n x?c minh: gyeongju:G7, repair_or_fight_branch, statue_state

### CM_SL_02_V1 ? Ji-won dò tín hiệu
- **T?nh hu?ng/manh m?i:** Cho người chơi xem bảng tuyến A/B/C chính thức trong S7 trước moment.
- **C?u h?i:** Muốn xác định bản tin ghi sai tên nguồn, phải so cặp thông tin nào?
- **Ph??ng ?n:** A. Màu màn hình và độ dài bản tin.; B. Ký hiệu trong tín hiệu với bảng tuyến A/B/C đã đối chiếu.; C. Tên người đọc bản tin với kích thước máy phát.
- **??p ?n:** B
- **L? do:** Tín hiệu nước khớp tuyến Đà Lạt; nhãn Jeonju mâu thuẫn với nội dung.
- **Tr?ng th?i:** KEEP; feedback PROPOSED_NOT_RUNTIME_INTEGRATED; cue c?n x?c minh: seoul:S7, relay_readiness, cross_city_artifacts

## Ch?a ???c x?c minh

- Kh?ng c? Unity runtime integration trong l??t n?y.
- `evidencePresentation` l? y?u c?u/?? xu?t tr?nh b?y, kh?ng ph?i b?ng ch?ng ?? t?n t?i.
- Feedback m?i tr??ng, marker, animation, SFX v? dialogue m?i v?n l? PROPOSED cho t?i khi c? runtime capture.
- Android/device ch?a ???c ki?m tra.

## Ki?m tra

- Validator c?u tr?c s? ch?y sau thay ??i.
- Kh?ng ch?y Unity build.
- Kh?ng s?a gameplay ho?c asset runtime.
