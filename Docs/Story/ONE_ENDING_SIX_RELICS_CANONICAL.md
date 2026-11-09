# One Ending + Six Relics — canonical Phase5

Ngày 18/10/2026. Implementation hiện có; QA mới và giới hạn được ghi tại [Phase5 ending QA](../QA/PHASE5_ENDING_E2E.md). Không lấy báo cáo Phase0 hoặc Phase4 làm bằng chứng cho ending mới.

## Một kết thúc duy nhất

`bossResolved && allSixCanonicalRelicsRestored && playerConfirmedCeremonyCompleted`

- Boss resolved dùng `EncounterManager.Resolved(Content.Event("seoul", "enemy_final_core"))`: hợp lệ cho cả battle và repair. Không dùng riêng defeatedEnemies.
- Sáu restored được đọc từ RelicCollection cho đúng sáu relic ID canonical; 24 fragment, assembly hoặc sáu beacon không thay thế restored.
- `NodeEventManager.CanStartFinalCeremony`: đủ boss + sáu restored, đúng S21/CityMap/discovery, chưa hoàn thành, chưa chạy.
- `CeremonyInProgress`: session tạm sau popup xác nhận; 24 giây gồm sáu nhịp ba giây và hội tụ sáu giây. Không phải trạng thái completed.
- `CanonicalEndingCompleted`: flag mới `game.canonical_ending_completed.v1`, chỉ được commit qua Execute tại kết thúc hợp lệ hoặc Skip có chủ ý trong session đã xác nhận.

## S21 và SQ03

Báo tin bình an tại S21 hoàn tất SQ03 theo điều kiện boss cũ, dù chỉ 0–5 di vật. Chỉ ba câu offer/reminder/thanks của SQ03 được sửa để bỏ yêu cầu credits sai; graph, điều kiện, vị trí và reward không thay đổi. Xác nhận ceremony cũng có thể báo kết quả SQ03 nếu còn đủ điều kiện tại S21, không thêm dependency cho SQ03.

0–5/6: xem tiến độ, Collection hoặc trở lại khám phá; không credits. 6/6 nhưng chưa boss: vẫn giải quyết boss bình thường. Boss + 6/6: popup xác nhận riêng. Để sau không commit. Sau ending: chỉ replay presentation bằng lựa chọn chủ động, không thưởng lại.

`ending`, `ending_next`, `ending_ack`, `ceremony_confirm`, `ceremony_finish`, `ceremony_skip` đều được kiểm tra tại backend. ShowEnding kiểm tra eligibility riêng; việc mở popup không thể tự cấp completion. Hết ceremony mới được xem lời kết rồi credits.

## Save và lịch sử

Không đổi SaveData/SaveSystem/schema v1/v2. Chỉ thêm một flag completion vào list hiện có. `game.ending_seen`, SQ03, boss result, rewards/ledger và choices cũ vẫn là lịch sử; không có grandfather 100% từ credits cũ. Ceremony chưa xong không lưu in-progress: restart quay về trạng thái sẵn sàng, phải chủ động bắt đầu lại. Flag chỉ ghi khi SaveSystem atomic write thành công; nếu lỗi ghi, backend phục hồi flags và chưa công nhận hoàn tất.

Bấm Skip có chủ ý sau confirm là một kết thúc ceremony hợp lệ. Đóng panel thông thường là Cancel. Replay không có reward mới. `game.ending_seen` được ghi tương thích sau khi chủ động khép credits; completion mới đã commit trước đó, không phụ thuộc việc xem hết từng trang.

S22/SQ04 và `seoul.secret_ending` vẫn là thông tin phụ, không có credits hoặc completion riêng. World Map tiếp tục mở bằng beacon cũ, không phụ thuộc di vật/ceremony. Bảo vệ Hội An A/B/C, các city A/C, 18 check-in/H6 và toàn bộ audio đã có.

Nội dung trình diễn: [FinalConnectionPresentation.json](../../Assets/_Game/Resources/Content/FinalConnectionPresentation.json); lời kết/credits: event final_festival trong GameplayContent.json. Không viết lại truyện sáu thành phố, không tạo EndingManager hoặc puzzle thứ 23.
