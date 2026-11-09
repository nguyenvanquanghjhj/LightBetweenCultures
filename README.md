# Light Between Cultures

## Catventure — 07/11/2026

Đã bổ sung tuyến giải cứu mèo cho sáu thành phố: 24 thử thách văn hóa, túi cứu hộ, bộ sưu tập, huy hiệu/bưu thiếp, thời tiết và lưu tiến trình từng thao tác. Trong City Map, chọn **Mèo 0/6** để bắt đầu; giữ nguyên save đang chơi.

[Hướng dẫn Catventure](Docs/Catventure/HANDOFF.md) · [Kết quả kiểm thử](Docs/Catventure/REPORT.md). Các bản RC1/RC2 bên dưới là lịch sử; xem báo cáo Catventure để lấy đúng bản build mới.


Unity **6000.6.5f1**, game 2D portrait cho Android, c? Windows Player ?? ki?m th?.

## Tr?ng th?i hi?n t?i

C? ?? **s?u th?nh ph?, 133 node**: H?i An, Hu?, ?? L?t, Jeonju, Gyeongju v? Seoul. C?c th?nh ph? d?ng chung scene/script CityMap v? d? li?u ri?ng; th? m?c `Scripts/HoiAn` d?nh cho c?nh m? ??u/H3, kh?ng ph?i danh s?ch to?n b? map.

Release Candidate: **RC1 ? 24/10/2026**, ch? owner nghi?m thu Android/save. [B?o c?o RC1](Docs/QA/RELEASE_CANDIDATE_20261024.md), [quy t?c gi? source](RELEASE_FREEZE.md).

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
- [D?n dung l??ng an to?n](Docs/Storage/SafeCleanup-20261023/AUDIT.md). Ch? cache sinh l?i ???c b? d?n; kh?ng ??i gameplay, asset, GUID/meta ho?c save.

README c? l? t?i li?u giai ?o?n ??u v? ?? l?i th?i; b?n g?c ???c gi? [t?i ??y](Docs/Storage/SafeCleanup-20261023/README.before.md) ?? ??i chi?u l?ch s?.
