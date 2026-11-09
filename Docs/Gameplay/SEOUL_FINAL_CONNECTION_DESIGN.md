# Seoul Final Connection — Phase5 implementation

Tên chính thức: **LỄ KẾT NỐI SÁU MIỀN ÁNH SÁNG / The Six Lights Connection**.

## Luồng runtime

S20 battle hoặc repair → S21 báo tin/SQ03 độc lập → tiếp tục thu/restored nếu thiếu → Boss + 6 restored → Bắt đầu Lễ Kết Nối → popup xác nhận → session 24 giây hoặc deliberate Skip → commit `game.canonical_ending_completed.v1` → sáu lời hồi tưởng + một trang credits → tiếp tục khám phá. Đến node, mở Collection, load save hoặc đóng popup không đủ điều kiện commit.

Không đổi S20, HP/phases/repair/readiness/reward, relay, world gates, quest graph hoặc SaveSystem.

## Phân trách nhiệm

- `NodeEventManager.FinalConnection.cs`: eligibility dùng source hiện có, report SQ03, transient confirmation/session gắn scene/S21, guard legacy ending actions, atomic flag commit/rollback. Không manager mới.
- `GameplayPanels.FinalConnection.cs`: popup, coroutine UI sáu nhịp, controls Cancel/Skip/Next/replay và guard ShowEnding. Decorative images/beam không raycast.
- `FinalConnectionPresentation.json`: sáu vai/caption/màu theo thứ tự Hội An → Huế → Đà Lạt → Jeonju → Gyeongju → Seoul; convergence line.
- `GameplayContent.json` event final_festival: lời kết và credits dễ chỉnh; SQ03 chỉ cập nhật wording phù hợp xác nhận hậu boss độc lập.
- `GameplayPanels.Relics.cs`: hiển thị số restored, Seoul an toàn/chưa yên và ceremony chưa sẵn sàng/sẵn sàng/đã xong trong Collection hiện có. Không tiết lộ HIDDEN source nodes.

## Sáu nhịp và nhịp hội tụ

0–3s: Đèn Mắt Phố/amber/Đánh Thức. 3–6s: Ấn Rồng Ngọc Tỉ/teal/Xác Thực. 6–9s: La Bàn Sương Lam/blue/Định Hướng. 9–12s: Mộc Ấn Hanji/warm paper/Lưu Giữ. 12–15s: Kính Ấn Tinh Tú/lilac/Đọc Ký Ức. 15–18s: Lăng Kính Tam Relay/warm light/Kết Nối. 18–24s: đủ sáu biểu tượng và sáu đường sáng về Lumi, lời kết nối, rồi ending. Sprite/material/frame/portrait đều tái sử dụng; chỉ transform/fade UI nhẹ, không ảnh AI, video dựng sẵn hoặc shader nặng.

Đây là presentation ending, không thêm trial/reward/minigame. BGM khám phá Hàn Quốc đang phát tiếp tục; không viết lại SoftAudio, không thêm track/SFX. Sau boss, BattleManager.Active=false đã đưa nhạc về map theo hệ hiện hành.

## Gián đoạn và replay

Session chỉ sống trong NodeEventManager hiện tại, có clock tích lũy unscaled theo frame hoạt động và scene identity; cap delta 0.1 giây/frame, không tính app suspended hoặc thời gian hitch để tránh implicit Skip. Cancel/scene exit/location change/reload không commit. UI không lưu pagekey legacy để tự coi là ceremony. Save giữa ceremony giữ boss/6 relic nhưng chưa completion; vào lại tự chọn làm lại. Nghi thức đã commit thì restart không chạy lại. Replay là lựa chọn chủ động với backend + view guard; không nhận thưởng/đánh boss lại.

Cả normal-completion và deliberate Skip đi chung một nhánh commit idempotent; chỉ chấp nhận khi boss/6/đúng S21/session hợp lệ. Normal-completion còn kiểm clock 24 giây; không thể gọi finish ngay sau confirm. Trạng thái ready derive từ state hiện có, không thêm flag ready trùng lặp.

## Giới hạn

Dùng save flags v1/v2 hiện hành; không migration schema lớn. Mid-battle restart tiếp tục theo semantics cũ: boss chưa resolved vẫn thử lại trận, không tự thắng; không thêm lưu HP/phase đối thủ. Android build/device và profiling thực chưa nghiệm thu. Ba icon component Seoul sai nghĩa vẫn ART_PENDING; cinematic mới chờ chủ dự án duyệt thẩm mỹ.

[Canonical](../Story/ONE_ENDING_SIX_RELICS_CANONICAL.md) · [Implementation](PHASE5_IMPLEMENTATION_REPORT.md) · [QA](../QA/PHASE5_ENDING_E2E.md)
