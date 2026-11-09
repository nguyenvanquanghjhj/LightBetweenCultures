# QA trước task Shop — checkpoint một phần, 21/10/2026

Không phải FULL PASS. Công việc được chuyển sang Journey Coin Shop theo yêu cầu mới; không chạy lại campaign vì shop không thay dependency của route/story.

| Nhóm đã chạy | Kết quả Windows Player | Evidence dưới `Validation/ComprehensiveQA-20261020/` |
|---|---|---|
| H3 tap item, đường bị chặn, nhặt đúng một lần; hướng dẫn và bốn mảnh/ghép chưa H7 | PASS qua input mô phỏng | `owner-fresh-recheck/smoke-results.txt` |
| Minigame: help, xác nhận sớm, đóng/thử lại; combat rút lui/retry/thắng | PASS qua input mô phỏng | `casual-core-recheck/smoke-results.txt` |
| Cả 26 NPC | PARTIAL: 25 đạt, G14 giữ an toàn/chặn tương tác từ xa | `npc-all-recheck/npc-standing-all.csv`, `standing-summary.json` |
| X không nhận/giao, nhận quest không Next lặp, HQ06 cancel/confirm/receipt, reward/reload | PASS | `npc-buttons-fresh-recheck/smoke-results.txt` |
| R1/R2/R3/R4/R5 đi thuyền cả hai chiều | Các subcase PASS, suite ban đầu dừng sau đó ở adapter junction | `routes/restart-results.txt`, `R*-trace.json` |
| X1/X3/X4 đổi hướng tại junction; sáu city, pan/pinch, bounds, HIDDEN graph | PASS | `junctions-recheck/restart-results.txt` |
| VN-first / Korea-first đến ending | CHƯA HOÀN TẤT | `story-*-recheck/smoke-results.txt` |
| Đủ 12 leisure | CHƯA HOÀN TẤT | `leisure12-earned-recheck/restart-results.txt` |

Các giới hạn cần tiếp tục:

- G14: chưa có đoạn tiếp cận an toàn từ đường hiện có tới chân NPC. Source event là `npc_inscription`, cung cấp `gyeongju.core_rhythm_known`; không phải fragment bắt buộc và không phải quest GQ04. Không tự sửa route để ép PASS.
- Adapter J2 vẫn timeout sau nhận JQ01; sửa receipt đầu tiên chưa đủ cho mọi thời điểm đóng panel. Chưa chứng minh đây là lỗi gameplay. VN-first còn có lượt thất bại căn H6; không kết luận hồi quy H6 từ một lượt automation.
- Adapter leisure cũ tìm `LeisureActivityView` ở xích đu đang dùng `LeisurePilotView`; NullReference thuộc test. Các kiểm tra recovery .bak/.tmp/v1 trước đó đã PASS, nhưng không nhận đủ 12 leisure PASS.
- Một lượt gọi test buttons sai seed và một lượt thiếu đường dẫn route cases đã được thay bằng lượt cấu hình đúng; giữ log lỗi, không biến chúng thành lỗi gameplay.
- Blind human / hành trình người mới 30 phút: NOT_EXECUTED. Automation không chứng minh người chơi thấy vui.
- ADB hiện không thấy thiết bị. Không clear/uninstall hoặc đổi save trên điện thoại.
- APK trước shop: `Builds/Android/LightBetweenCultures-ComprehensiveQA-20261021.apk`, version `1.0-qa20261021`, code 2, build Succeeded, 289.640.664 bytes. `BuildReport.totalSize` 1.529.682.465 bytes bao gồm dữ liệu build; không phải kích thước APK. Không dùng APK này để nhận shop PASS.

Các thay đổi adapter QA được giữ lại cùng checkpoint Shop; không rollback source/gameplay đã có.
