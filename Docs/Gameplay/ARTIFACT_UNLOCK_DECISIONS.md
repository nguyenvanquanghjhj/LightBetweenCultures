# Quyết định đã duyệt — 2026-10-18

Phê duyệt D01–D08 giữ nguyên. **Phase4 merged đã được cho phép và triển khai năm di vật còn lại; DỪNG nghiệm thu Phase4. Phase5–7 chưa cho phép.**

| Quyết định | Đã chọn / ràng buộc |
|---|---|
| D01 | A: H0/H4/H10/H2; reflection chỉ completed mai-lantern. Bà Tư reward giữ nguyên. Thu mảnh sớm; hoàn tất Dẫn Ánh Đèn phải HQ01 completed + H7 hoi_an.lamp_restored. Không dùng relic mở map. |
| D02 | A: Hạt Tín Hiệu Xanh D12; Seoul chỉ xác thực/khuếch đại sau. |
| D03 | A: Văn Khắc U17; G14 đối chiếu, không cấp bản thứ hai. |
| D04 | A: Dấu Lời Chào từ H6; hoạt động Jeonju ở Phase 4. Old H6 marks được thao tác photo/claim riêng, không thưởng lại. Không buộc HQ05/HQ07/DQ05. |
| D05 | G7 guard_resolved công nhận cả battle/repair; tương tác claim hợp lệ, presentation theo nhánh, không branch quest/ending mới. |
| D06 | B: giữ milestone Seoul; full prism và frame được import kỹ thuật; ba icon chưa đúng ART_PENDING, không bind nhầm. |
| D07 | Giữ legacy game.ending_seen/SQ03/boss/reward/choices. Global completion mới riêng sau boss battle OR repair + sáu restored + nghi thức Seoul. SQ03 xác nhận tại S21 độc lập relic. Midcredits cũ không tự thành nghi thức mới. **Chỉ thực thi Phase 5**, chưa lượt này. |
| D08 | Import/bind kỹ thuật bốn PNG vào đúng npcId; ART_IMPORTED_PENDING_APPROVAL, chưa canonical/VISUAL_APPROVED. Curator và Inscription giữ file khác, cần người dùng nghiệm thu mặt. |
| H2 check-in | Sau completed mai-lantern, giữ checkinId/reward ledger; quest/talk ưu tiên camera. |
| Persistence | Tối thiểu additive trong flags/choices/ledger v1/v2; source proof + claim riêng + idempotence/retro-claim. Không nâng schema, không viết lại SaveSystem. |
| Identity | relic_* canonical gameplay; artifact_* chỉ alias art. Một core/state registry. |

[ACCESS_MATRIX](ARTIFACT_FRAGMENT_ACCESS_MATRIX.md) là nguồn phân bố đã duyệt. Tương tác H0/H10/H6 là **nội dung mới được cho phép**, không được giả là event cũ. H0/H4/H10/H2 phải có pointer gameplay/proof và QA. H7 giữ nguyên quest/world unlock. Năm city đã có runtime claim/assembly/trial/A-C theo mergedPhase4;3icon Seoul và visual approval vẫn pending. Ending ceremony chờPhase5. Xem PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md.
