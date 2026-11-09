# Catventure 1.1.1 — sửa tên mèo và vùng bấm, 07/11/2026

Đã bỏ tên mèo do Codex tự đặt, dùng nhãn theo thành phố và “Mèo” ở đích mê cung. Bỏ nút mèo phụ đè lên điểm đến; hình mèo không nhận input và giữ kích thước khi zoom 1×/2×/4×. **573 kiểm tra Windows Player PASS**: hai lượt đủ sáu thành phố/24 thử thách ở dọc/ngang (285 + 285), cùng nạp save QA bản 1.1 trong process mới (3). Save thật của chủ game không được dùng.

Windows Development/Release và APK code 21 build PASS. APK qua package/version, chữ ký cùng bản code 20, căn chỉnh 16 KB và CRC; **điện thoại thật NOT_TESTED**. [Bản chạy, ảnh Huế và bằng chứng lượt sửa](../Catventure/LABELS_LAYOUT_FIX.md). Các kết quả 633/514 bên dưới là lịch sử bản 1.1.

---

# Catventure — 07/11/2026

Windows Release và APK Android build PASS. Tuyến giải cứu sáu thành phố / 24 thử thách: **633 kiểm tra Windows Player PASS** (hai kích thước, cold restart và hồi quy save/World Map/shop). **512 + 2 domain checks PASS** trong adapter host, tách khỏi kết quả native.

Android thiết bị thật **NOT_TESTED** vì ADB không có thiết bị; toàn bộ campaign truyện cũ **NOT_RERUN**. [Bàn giao và bằng chứng Catventure](../Catventure/REPORT.md). Các mục RC1/RC2 bên dưới là lịch sử, không phải xác nhận bản Catventure.

---

Latest QA ? 25/10/2026: **PARTIAL, ch?a ch?t to?n game PASS**. 23 l??t Windows Player: 16 PASS / 7 FAIL raw; 35 nh?m d? li?u PASS. Ba NPC m?i, panel, h?i tho?i, 12 leisure, save/recovery v? ending/restart ??t ki?m tra ??c l?p. Campaign d?i c?n d?ng ? input k?o/chuy?n WorldMap; Android kh?ng c? thi?t b?. Source v? save owner gi? nguy?n. [B?o c?o ??y ??](FULL_GAME_20261025.md).

---

# Release Candidate RC1 ? 24/10/2026

Windows Release v? APK RC1 build/CRC/signature PASS. Windows save/recovery/separate-process + shop PASS; native Android save ch?a ???c nghi?m thu thi?t b? v? ADB kh?ng c? C71. Source hold, OWNER_ACCEPTANCE_PENDING. [B?o c?o ch?t](RELEASE_CANDIDATE_20261024.md). Kh?ng nh?n full campaign, Android device ho?c ?? vui PASS.

---

# Latest focused checkpoint ? 23/10/2026

Final UX: 447 focused Windows checks PASS; six-city arrival/world access fixtures, story/relic snapping, existing NPC callback regression and audio. Protected art/route/quest/save schema inputs unchanged. Android device and independent human retention NOT_TESTED. Details: [FINAL_UX_20261023.md](FINAL_UX_20261023.md). Build/signing status is recorded in Validation/FinalUX-20261023/summary.json.

---

# LBC — Journey Coin Shop · 21/10/2026

- Shop runtime đã tích hợp: 8 vật phẩm giá 1–3 xu, một quyền HQ04 giá 5 xu, mở từ City Map; đủ danh mục ở cả sáu thành phố.
- Windows focused gameplay/input mô phỏng **PASS**: giao dịch, quest bằng nguyên liệu mua, trả phí đò bằng hai cách, chống bấm trùng, lỗi persist, v1/reload và process restart. [Báo cáo](JOURNEY_COIN_SHOP.md) · [Ảnh thật](../../Validation/JourneyShop-20261021/gallery.html).
- APK mới: `Builds/Android/LightBetweenCultures-JourneyShop-20261021.apk`, `1.0-shop20261021`/code 3. Build PASS; **Android device BLOCKED** vì ADB không thấy thiết bị. Không thay save điện thoại.
- Không nhận toàn game FULL PASS. [QA trước Shop còn dang dở](COMPREHENSIVE_QA_CHECKPOINT_20261021.md): G14 chưa có approach an toàn, hai tuyến ending và 12 leisure chưa hoàn tất qua adapter hiện tại; blind human test chưa chạy.
- Checkpoint source incremental: `D:\GAMEKKK\Backups\CheckpointJourneyShop-20261021`, cần chain base đã ghi trong manifest. Không sao chép Library/Builds/ảnh QA.

---

# LBC — checkpoint button glossy · 20/10/2026

- Style nút tròn, mềm, có sheen/shadow và pressed lún đã tích hợp vào runtime. Giữ callback và vùng chạm; không đổi gameplay.
- Windows build `Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe` PASS. Visual/input 540×960 và callback regression 720×1080 PASS. Không build/test Android trong task này.
- [Báo cáo/file thay đổi](SOFT_GLOSSY_BUTTONS.md) · [Ảnh Player before/after](../../Validation/GelButtons-20261020/gallery.html).
- Checkpoint `D:\GAMEKKK\Backups\CheckpointGelButtons-20261020` là incremental dựa trên checkpoint NPC phía dưới. Các hạn chế gameplay cũ như G14 vẫn giữ nguyên, không thuộc task visual này.

---

# LBC — checkpoint NPC approach / công dụng nút · 20/10/2026

- Windows hiện tại dùng lại `Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe`; build log `Logs/npc-standing-windows06.log`.
- **NPC PARTIAL: 25/26**; Lumi đi tới gần H10/S2/S11, NPC giữ chân canonical. G14 **BLOCKED_SAFE_NO_APPROACH**, không mở thoại từ xa hoặc tự nối xuyên địa hình.
- Callback hội thoại focused Windows **PASS**: X không nhận/giao/thưởng; nhận nhiệm vụ giữ receipt thật và bỏ lời hứa/Next lặp; giao/hủy HQ06, giữ nguyên liệu, chống nhận lặp và reload. `1/2` là phân trang, đã thay bằng danh sách hành động trực tiếp. Overflow drag **NOT_TESTED** trong fixture vừa màn hình.
- [Báo cáo hiện tại](NPC_APPROACH_AND_BUTTON_FUNCTIONS.md) · [Ảnh Player trước/sau](../../Validation/NpcStandingButtons-20261020/gallery.html).
- APK `Builds/Android/LightBetweenCultures-OwnerPriority-20261020.apk` là bản trước các sửa này. **Không build/cài APK lượt này; Android nhóm sửa mới NOT_TESTED.**
- Phần thay đổi UI tròn/bóng được owner yêu cầu sau đó chưa triển khai trong checkpoint sửa chức năng này. Không nhận COMPLETE cho yêu cầu visual đó.
- Các kết quả bên dưới là lịch sử; cách clamp vị trí NPC đã được thay thế, không dùng PASS cũ để kết luận lỗi owner phản ánh đã hết.

---

# LBC historical checkpoint - 2026-10-20 touch / NPC / dialogue

- Windows: `Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe`.
- APK current source (BUILD/INSTALL PASS; final dialogue/device save acceptance BLOCKED/NOT_VERIFIED): `Builds/Android/LightBetweenCultures-OwnerPriority-20261020.apk`; final device result is recorded in `AndroidTouch-20261020/priority-device-results.json`.
- Focused PC: 381 NPC/callback assertions + 14 partial-delivery boundary assertions PASS. Core data/art/settings hashes preserved; no campaign/Phase 7 rerun.
- [Current short report](ANDROID_TOUCH_AND_NPC_DIALOGUE_FIXES.md) ? [Native evidence gallery](../../Validation/AndroidTouch-20261020/gallery.html).
- Previous checkpoint notes below are historical, including their old artifact paths and Android limitations.

---

# LBC — tình trạng QA hiện hành · 20/10/2026

- **Windows được giữ:** `Builds/Windows-LeisureVisuals-20261020/LightBetweenCultures.exe`, build thành công từ source mới nhất. Diều/chày/badge leisure được kiểm tra trong Player 540×960: mở, kéo/chạm mô phỏng, đóng giữa chừng, replay, sáu map, HIDDEN/discovery và không đổi save. 720×1280 BLOCKED bởi cửa sổ QA chỉ đạt 720×960; chưa duyệt hình ảnh trên điện thoại.
- **APK được giữ:** `Builds/Android/LightBetweenCultures-GameFeel-20261020.apk`, build thành công, SHA256 `795cb2062e06bd2295e79c06c82bcbce503a97da278515be82b34b6c0bcae121`. APK có trước ba sửa leisure mới nhất; cần một build Android mới khi chủ dự án yêu cầu. Không build APK trong lượt dọn dung lượng.
- **Kết quả trước đó:** campaign sáu thành phố, hai hướng chơi, relic/ending, H3 pickup, route thuyền hai chiều và 12 leisure đã có scripted Windows QA. Đây là kết quả lịch sử, không phải chạy lại sau cleanup hay blind human playtest. Các log/gallery cũ đã được dọn theo yêu cầu chủ dự án.
- **Còn cần kiểm tra:** Android trên thiết bị thật (input/FPS/RAM/loop audio), hành trình người mới liên tục 5/15/30 phút, nghiệm thu toàn bộ footprint/địa hình và marker theo hình ảnh. `puzzle_pond`/`puzzle_lantern_west` chưa có lượt chạy mới trong QA cuối.
- **Còn cần quyết định/chất lượng:** tám leisure còn dùng hoạt cảnh nhẹ; độ hấp dẫn và cân bằng combat chưa được chứng minh bằng người chơi độc lập. Không coi automation PASS là người chơi thấy vui. Không tự đổi gameplay để xử lý trong lượt cleanup.
- **Bảo toàn:** hash Assets/Packages/ProjectSettings và bộ source backup được kiểm tra lại sau cleanup. Không sửa nội dung game, route, save, signing hoặc audio; không chạy Phase 7/build/full QA.
- **Khôi phục:** `D:\GAMEKKK\Backups\LBC_SourceVerified-20261020`, bản source độc lập, đủ Assets + metadata + Packages + ProjectSettings; xem `RESTORE.md`. 35 nội dung source cũ không còn trong project được giữ trong `HistoricalUnique`, không tự trộn vào Assets.
- **Tooling:** giữ cấu hình JSON/CSV và input art/route mà script còn tham chiếu, toàn bộ `Tools/SixCityRouteEditor`, dữ liệu vẽ/ink/junction và save QA nhỏ. `CheckpointAndroidApkBuild-20261019` chỉ giữ làm provenance cho preflight của route exporter; chuỗi backup lịch sử của nó đã được thay bằng bản độc lập trên, không dùng nó để restore.

[Kết quả dọn dung lượng](../Storage/DeepCleanup-20261020/SUMMARY.md)
