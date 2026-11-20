## Cấu trúc mã nguồn và tài nguyên — 13/11/2026

Trạng thái hiện hành: [tái cấu trúc toàn workspace](Docs/Architecture/FULL_WORKSPACE_RESTRUCTURE.md), [cây workspace](Docs/Architecture/WORKSPACE_TREE.md), [cây project](Docs/Architecture/PROJECT_TREE.md), [công cụ đang dùng](Tools/README.md). Các bản bàn giao được bảo tồn ở `../_LBC_DELIVERABLES`, backup/QA lịch sử ở `../_LBC_ARCHIVE`, output QA hiện hành ở `../_LBC_WORK`. Build mới không tự cập nhật hoặc ghi đè bản nộp cũ.

[Bản đồ kiến trúc](Docs/Architecture/CODEBASE_MAP.md) · [Tra cứu tính năng](Docs/Architecture/FEATURE_INDEX.md) · [Cây thư mục hiện tại](Docs/Architecture/FINAL_TREE.md) · [Báo cáo dọn tài nguyên và giới hạn kiểm thử](Docs/Architecture/CLEANUP_REPORT.md)

[Đợt loại nguồn lịch sử khỏi Assets ngày 13/11 và điều kiện commit/push chưa đạt](Docs/Architecture/UNUSED_ASSET_ELIMINATION.md).

## CATVENTURE — Source nâng cấp 09/11/2026 (chưa nghiệm thu native)

**Không nhầm bản build:** Source phải được build lại bằng Unity 6000.6.5f1. Bản EXE/APK cũ trong thư mục Builds sẽ KHÔNG tự cập nhật khi push code. Bản mới hiển thị `CATVENTURE · 6 MÈO · 24 THỬ THÁCH` ở màn hình đầu và có nút `CATVENTURE · GIẢI CỨU MÈO` để vào thẳng nhiệm vụ. Nếu không thấy hai dòng này thì đang chạy build hoặc scene cũ.

Xem [hướng dẫn nghiệm thu bản cập nhật](Docs/Development/Catventure/ACCEPTANCE_20261109.md). Các số PASS ngày 07/11 là kết quả lịch sử, không phải chứng nhận cho source vừa sửa.

---

# Light Between Cultures

## Catventure — 07/11/2026

Đã bổ sung tuyến giải cứu mèo cho sáu thành phố: 24 thử thách văn hóa, túi cứu hộ, bộ sưu tập, huy hiệu/bưu thiếp, thời tiết và lưu tiến trình từng thao tác. Trong City Map, chọn **Mèo 0/6** để bắt đầu; giữ nguyên save đang chơi.

[Hướng dẫn Catventure](Docs/Development/Catventure/HANDOFF.md) · [Kết quả kiểm thử](Docs/Development/Catventure/REPORT.md). Các bản RC1/RC2 bên dưới là lịch sử; xem báo cáo Catventure để lấy đúng bản build mới.


Unity **6000.6.5f1**, game 2D portrait cho Android, c? Windows Player ?? ki?m th?.

## Tr?ng th?i hi?n t?i

C? ?? **s?u th?nh ph?, 133 node**: H?i An, Hu?, ?? L?t, Jeonju, Gyeongju v? Seoul. C?c th?nh ph? d?ng chung scene/script CityMap v? d? li?u ri?ng; th? m?c `Scripts/HoiAn` d?nh cho c?nh m? ??u/H3, kh?ng ph?i danh s?ch to?n b? map.

Release Candidate: **RC1 ? 24/10/2026**, ch? owner nghi?m thu Android/save. [B?o c?o RC1](Docs/QA/RELEASE_CANDIDATE_20261024.md), [quy t?c gi? source](Docs/Release/RELEASE_FREEZE.md).

Checkpoint gameplay tr??c RC: **Final UX ? 23/10/2026**. [K?t qu? v? gi?i h?n ki?m th?](Docs/QA/FINAL_UX_20261023.md), [tr?ng th?i QA](Docs/QA/CURRENT_QA_STATUS.md). 447 focused Windows checks PASS; Android b?n m?i build/ch? k? PASS, thi?t b? th?t v? blind retention ch?a ki?m th?. ??y kh?ng ph?i ch?ng nh?n full campaign.

## M? v? ch?y

1. M? project n?y b?ng Unity Hub, ??ng phi?n b?n 6000.6.5f1.
2. M? [Prototype.unity](Assets/_Game/Scenes/Prototype.unity), nh?n Play.
3. Ch?n B?t ??u kh?m ph?. C?c th?nh ph? m? theo ti?n ??: H?i An ? Hu?/Jeonju ? ?? L?t/Gyeongju ? Seoul.
4. Kh?ng ch?y l?i tool t?o scene, author/import ho?c setup Phase ?? kh?i ??ng project ?? c? d? li?u.

## Build ?ang gi?

- [APK RC1 m?i nh?t](Builds/Android/LightBetweenCultures-RC1-20261024.apk): version 1.0-rc1-20261024, code 6.
- [Windows Release RC1 ZIP](Builds/LightBetweenCultures-RC1-20261024-Windows.zip): gi?i n?n v? ch?y EXE, gi? nguy?n to?n b? th? m?c c?nh EXE. Th? m?c Windows hi?n c? ???c d?ng l?i, kh?ng t?o b?n Player tr?ng.
- ZIP Windows Playtest 21/10 v? APK Playtest tr??c ?? ???c gi? l?m b?n ??i chi?u ?? b?n giao; kh?ng nh?m v?i b?n FinalUX.

## T?m source v? t?i li?u

- [M?c l?c project, c?ng c? v? t?i li?u](Docs/PROJECT_INDEX.md).
- Gameplay runtime: `Assets/_Game/Scripts/`; n?i dung: `Assets/_Game/Resources/Content/`.
- Artwork Lumi/NPC canonical: [Lumi](Docs/Art/LUMI_CANONICAL_20261021.md), [registry NPC](Docs/Art/NPC_CANONICAL_REGISTRY.md).
- [D?n dung l??ng an to?n](Docs/Development/Storage/SafeCleanup-20261023/AUDIT.md). Ch? cache sinh l?i ???c b? d?n; kh?ng ??i gameplay, asset, GUID/meta ho?c save.

README c? l? t?i li?u giai ?o?n ??u v? ?? l?i th?i; b?n g?c ???c gi? [t?i ??y](Docs/Development/Storage/SafeCleanup-20261023/README.before.md) ?? ??i chi?u l?ch s?.
