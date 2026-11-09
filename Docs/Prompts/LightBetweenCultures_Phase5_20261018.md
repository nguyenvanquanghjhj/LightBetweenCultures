# LIGHT BETWEEN CULTURES — MASTER EXECUTION PHASE 5
## SEOUL FINAL CONNECTION · ONE CANONICAL ENDING · SIX RELICS CEREMONY · SAVE & STORY INTEGRATION

**PROJECT:** `D:\GAMEKKK\LightBetweenCultures`

**NHIỆM VỤ:** TRIỂN KHAI THẬT PHASE 5 — HOÀN THIỆN KẾT THÚC DUY NHẤT CỦA GAME.

Đây là lệnh triển khai chính thức sau Phase 4. Không phải audit-only hoặc chỉ viết kế hoạch.

**Mục tiêu cao nhất:** Người chơi có thể hoàn thành toàn bộ hành trình từ Hội An đến Seoul, giải quyết boss cuối, phục dựng đủ sáu di vật văn hóa, chủ động thực hiện nghi thức kết nối và xem **MỘT ending duy nhất**.

Không làm lại các phần Phase 0–4 đã PASS.

---

# I. ĐỌC HIỆN TRẠNG VÀ KHÓA PHẠM VI

## 1. Đọc source mới nhất trước khi sửa

Ưu tiên kiểm tra các tài liệu thực sự tồn tại:

- `AGENTS.md`
- `Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md`
- `Docs/Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md`
- `Docs/QA/PHASE4_SIX_RELICS_E2E.md`
- `Docs/QA/PHASE4_RUNTIME_REVIEW.html`
- `Docs/Story/ONE_ENDING_SIX_RELICS_CANONICAL.md`
- `Docs/Gameplay/SEOUL_FINAL_CONNECTION_DESIGN.md`
- `Docs/QA/FINAL_ENDING_AND_POST_BOSS_AUDIT.md`
- `Docs/Story/FULL_STORY_PLAYTEST.md`
- Phase 4 final checkpoint và `last-checkpoint.json`.
- Các thay đổi âm thanh BGM đã được tích hợp sau Phase 4, nếu có.

Kiểm tra source hiện tại của:

- `RelicCollection`
- `NodeEventManager`
- `GameplayPanels` và các partial liên quan
- `GameBootstrap`
- `WorldMap`
- `QuestManager`
- `EncounterManager` / `BattleManager`
- `SaveSystem`
- `GameplayContent.json`
- `CulturalRelics.json`
- Các cấu hình music/audio hiện hành.

Không giả định file, class hoặc flag trong báo cáo cũ vẫn giống source mới nhất.

Nếu đang có agent khác sửa những file trên, không chạy song song.

## 2. Kết quả Phase 4 phải được kế thừa

Phase 4 đã báo cáo:

- Sáu relic ID canonical.
- 24 fragment ID canonical.
- 24 nguồn mảnh có proof.
- Sáu lần lắp ráp và sáu restoration trials.
- Sáu trạng thái `restored` thực.
- Hai hành trình `VN_FIRST` và `KOREA_FIRST` PASS trên Windows.
- Post-boss backtracking và old-save retro-claim PASS trong phạm vi Phase 4.
- Hội An có ba trạng thái ánh sáng A/B/C.
- Năm thành phố còn lại có trạng thái A/C.
- Cơ chế Chìa Kết Nối H5–H7 đã kiểm thử.
- 18 check-in và Photo Journal đang hoạt động.

Không triển khai lại hệ di vật.

Nếu kết quả runtime hiện hành khác báo cáo, ghi rõ bằng chứng và xử lý phần ảnh hưởng trực tiếp tới Phase 5.

---

# II. QUYẾT ĐỊNH PHÊ DUYỆT MỚI NHẤT CỦA CHỦ DỰ ÁN

## 3. Duyệt NPC đã import

Chủ dự án **PHÊ DUYỆT GIỮ NGUYÊN NGOẠI HÌNH VÀ BINDING HIỆN TẠI** của bốn NPC:

| NPC ID | Địa điểm |
|---|---|
| `npc_curator` | Seoul S2 |
| `npc_engineer` | Seoul S11 |
| `npc_keeper` | Gyeongju G9 |
| `npc_inscription` | Gyeongju G14 |

Có thể cập nhật trạng thái approval trong báo cáo/manifest đang hoạt động thành:

`OWNER_APPROVED_AS_IS_20261018`

Phê duyệt này áp dụng cho sprite, portrait và identity binding **đã tồn tại ở checkpoint Phase 4**.

Không tự đổi mặt, thay trang phục, gán người khác, tạo variant, re-slice hoặc tạo ảnh AI mới.

Việc Curator và Inscription có nét mặt tương đồng đã được ghi nhận; chủ dự án chấp nhận phiên bản hiện tại, không yêu cầu thay chỉ vì nhận xét đó.

Nếu phát hiện lỗi kỹ thuật thật như missing sprite, mất GUID hoặc gán sai NPC ID, vẫn phải báo và sửa an toàn.

**Không mở rộng phê duyệt này sang NPC mới chưa được xem xét hoặc giao việc audit NPC/marker đang nằm ở task khác.**

## 4. Duyệt hiệu ứng phục dựng Phase 4

Chủ dự án **CHẤP NHẬN GIỮ NGUYÊN** các hiệu ứng phục dựng đang có ở sáu thành phố.

- Hội An: ánh sáng A/B/C.
- Huế: dấu ấn, ánh sáng di sản, phản chiếu hai bờ.
- Đà Lạt: sương trang trí dịu lại.
- Jeonju: ánh đèn xưởng và hoa văn Hanji.
- Gyeongju: dấu sao, ánh sáng di tích, phản ứng theo nhánh G7.
- Seoul: tín hiệu của Lăng Kính Tam Relay.

Ghi owner approval tương ứng cho **bản hiệu ứng Phase 4 hiện tại**.

Không tự thiết kế lại hoặc tăng cường hiệu ứng chỉ để đạt một tiêu chí thẩm mỹ mới.

Nếu Phase 5 cần trình diễn sáu nguồn sáng trong ending, hãy tận dụng các sprite, màu sắc, material và animation đã có.

**Ngoại lệ:** Ba component icon Seoul còn `ART_PENDING` vì sai ý nghĩa vẫn chưa được duyệt làm final. Giữ placeholder chức năng trung tính cho đến khi có ảnh đúng, không đổi tên fragment để khớp artwork.

Việc duyệt thẩm mỹ không thay thế kiểm thử về sorting, touch, save, performance và missing references.

## 5. Bảo vệ âm thanh đã tích hợp

Chủ dự án xác nhận **nhạc nền bản đồ và nhạc chiến đấu đã được tích hợp**.

Kiểm tra source và các audio asset thực tế, sau đó giữ nguyên:

- File âm thanh đang sử dụng.
- Mapping nhạc cho sáu thành phố.
- Nhạc chiến đấu.
- Audio manager và volume settings.
- Cơ chế vào/ra battle.
- Các audio clip GUID và import settings.

Không viết lại BGM system.

Không tải, thay thế, sáng tác hoặc tự chèn bản nhạc khác.

Trong ending, ưu tiên sử dụng một đoạn BGM đã có phù hợp với không khí kết nối. Nếu không có nhạc phù hợp, cho phép ceremony chạy với BGM đang phát; đánh dấu nhu cầu bổ sung âm thanh để xử lý sau.

Không để nhạc chiến đấu tiếp tục phát chồng lên nhạc nền hậu boss.

Nếu âm thanh nút bấm/SFX phụ chưa có, không được vì thế mà chặn Phase 5.

---

# III. LUẬT KẾT THÚC DUY NHẤT — BẮT BUỘC

## 6. Chính xác một ending

Game **CHỈ CÓ MỘT KẾT THÚC CHÍNH THỨC**.

Không có:

- Normal Ending.
- Bad Ending.
- Secret Ending.
- True Ending thứ hai.
- Ending chỉ vì thắng boss.
- Ending chỉ vì có đủ di vật.
- Ending tự động khi mở Seoul.

Điều kiện canonical:

`bossResolved && allSixRelicsRestored && finalCeremonyConfirmed`

Trong đó:

**A. `bossResolved`**

Boss Seoul tại S20 được giải quyết hợp lệ bằng một trong hai cách:

1. Chiến đấu và chiến thắng đúng cơ chế hai pha.
2. Hoàn thành nhánh sửa chữa/hòa giải hợp lệ đã có.

Cả hai được coi là giải quyết boss.

Không dùng duy nhất danh sách `defeatedEnemies` vì nhánh repair có thể không ghi vào đó.

**B. `allSixRelicsRestored`**

Cả sáu relic phải có trạng thái `restored` thực sự trong hệ RelicCollection canonical.

Không đồng nhất:

- Có 24/24 fragment.
- Đã assembled 6/6.
- Đã xem hình 6 di vật.
- Hoàn thành sáu beacon.
- Có sáu check-in bất kỳ.

Chỉ restored đúng sau trial mới được tính.

**C. `finalCeremonyConfirmed`**

Người chơi chủ động lựa chọn thực hiện nghi thức ở Seoul sau khi đạt hai điều kiện trên.

Chỉ xem popup, đến node hoặc mở Collection không được tự xem là xác nhận nghi thức.

## 7. Tách ba trạng thái đáng tin cậy

Triển khai theo kiến trúc hiện tại, phân biệt rõ:

1. `CanStartFinalCeremony`: boss đã resolved và sáu relic restored, nhưng ceremony chưa hoàn thành.
2. `CeremonyInProgress`: phiên trình diễn đang diễn ra, không đồng nghĩa đã hoàn thành.
3. `CanonicalEndingCompleted`: nghi thức đã kết thúc hợp lệ và global completion được commit.

Tên trường và cách lưu phải dựa trên source hiện có. Không bắt buộc dùng đúng các identifier gợi ý trên nếu architecture đã có nơi lưu phù hợp.

Guard kiểm tra phải tồn tại **cả ở UI và backend Execute**, tránh trường hợp gọi event trực tiếp hoặc save cũ vượt qua điều kiện.

Không tạo EndingManager thứ hai nếu NodeEventManager, WorldState và GameplayPanels hiện có đáp ứng được.

---

# IV. BẢO TOÀN BOSS SEOUL VÀ WORLD MAP

## 8. Không thay đổi S20

S20 có boss `enemy_final_core` và hệ hai pha hiện hành.

Tuyệt đối không sửa những thứ đang đúng:

- Boss HP.
- Damage.
- Hai phase.
- Battle actions.
- Repair pathway.
- Relay readiness.
- Victory rewards.
- Encounter ledger.
- Điều kiện truy cập S20.
- Spawn, arena và đường đi.

Không tăng độ khó boss chỉ vì người chơi đã có sáu di vật.

Không làm sáu di vật trở thành vật phẩm bắt buộc để đánh boss.

Không tạo một trận boss thứ hai sau restoration.

## 9. World Map vẫn độc lập với di vật

Bảo toàn progression:

- H7 → mở Huế và Jeonju.
- U19 → mở Đà Lạt.
- J19 → mở Gyeongju.
- Bốn beacon Huế, Đà Lạt, Jeonju, Gyeongju → mở Seoul.

Không biến relic, ceremony hay boss victory thành điều kiện mới để vào các thành phố đã mở.

Người chơi sau khi thắng boss vẫn phải quay lại được Hội An, Huế, Đà Lạt, Jeonju, Gyeongju và Seoul theo quyền truy cập đã đạt.

Không reset khám phá, marker, NPC, puzzle hoặc item khi quay lại.

---

# V. S21 — TÁCH HẬU BOSS VỚI NGHI THỨC KẾT THÚC

## 10. Sửa lỗ hổng credits cũ

Hiện báo cáo Phase 4 xác nhận legacy `final_festival` tại S21 vẫn có thể cho xem credits theo flow cũ dù người chơi chưa restored đủ sáu relic.

**Đây là lỗi P1 bắt buộc sửa trong Phase 5.**

S21 phải phân biệt các trường hợp sau.

### Trường hợp A — Boss đã resolved, còn 0–5/6 relic

S21 vẫn cho tương tác hậu boss.

Lumi/NPC ghi nhận mối nguy đã được giải quyết, nhưng ký ức văn hóa chưa được kết nối trọn vẹn.

Hiển thị tiến độ thực tế:

Ví dụ: `Đã phục dựng 4/6 di vật`.

Cho phép:

- Xem tiến độ Collection.
- Xem những thành phố có di vật chưa restored.
- Tiếp tục khám phá.
- Trở lại khi đã sẵn sàng.

**KHÔNG:**

- Phát ending.
- Phát credits.
- Đặt canonical ending completed.
- Bắt đánh lại boss.
- Khóa World Map.

Không tiết lộ node HIDDEN hoặc vị trí fragment chưa có manh mối.

### Trường hợp B — Đủ 6/6 relic nhưng boss chưa resolved

Vẫn phải giải quyết S20 theo gameplay hiện có.

Không tự hoàn thành boss, không chạy ceremony.

### Trường hợp C — Boss đã resolved và 6/6 relic restored

S21 hiển thị trạng thái:

**“Kết nối sáu miền ánh sáng đã sẵn sàng.”**

Có lựa chọn rõ ràng:

- **Bắt đầu Lễ Kết Nối**
- **Xem lại sáu di vật**
- **Để sau**

Chỉ khi người chơi chọn bắt đầu mới chuyển vào trình diễn nghi thức.

Nếu chọn Để sau, giữ nguyên trạng thái sẵn sàng sau khi restart.

### Trường hợp D — Ceremony đã hoàn thành

Không tự phát lại credits mỗi lần ghé S21.

Hiển thị lời thoại hậu kết thúc phù hợp.

Nếu có chức năng xem lại ending, phải là lựa chọn chủ động, chỉ replay presentation; không phát thưởng hay commit completion lần nữa.

---

# VI. SQ03 PHẢI HOÀN THÀNH ĐỘC LẬP VỚI SÁU DI VẬT

## 11. Quyết định D07 được áp dụng chính thức

SQ03 thuộc tuyến chính Seoul, **không được biến thành quest bắt buộc thu thập sáu di vật**.

Giữ mốc hoàn thành SQ03 tại S21 theo thiết kế hậu boss.

Ví dụ:

Người chơi thắng boss Seoul, sau đó tới S21, dù mới có 2/6 relic:

- Boss victory được công nhận.
- SQ03 được hoàn thành theo điều kiện canonical cũ.
- Quest reward chỉ được cấp một lần.
- Ceremony chưa sẵn sàng.
- Credits chưa phát.

Không dời SQ03 sang S20 chỉ để tiện lập trình.

Không thêm quest thứ 30 để thay SQ03.

Không nhân đôi completion/reward khi người chơi quay lại S21 để làm ceremony.

Sửa tối thiểu event `final_festival` và các trang ending/quest logic có liên quan.

---

# VII. LỄ KẾT NỐI SÁU MIỀN ÁNH SÁNG

## 12. Tên chính thức

**LỄ KẾT NỐI SÁU MIỀN ÁNH SÁNG**

English title có thể dùng:

**The Six Lights Connection**

Đây là nghi thức kết thúc cốt truyện, không phải một minigame thứ bảy.

Không thêm boss, quest hoặc puzzle mới.

## 13. Hình ảnh và nhịp trình diễn

Sử dụng presentation framework Unity đang có, không cần dựng 3D hoặc làm video bên ngoài.

Thời lượng chuyển động chính gợi ý khoảng **15–30 giây**, chưa tính thời gian người chơi tự đọc lời thoại.

Thiết kế sáu bước ánh sáng:

### Bước 1 — Hội An: Đánh Thức

- Biểu tượng Đèn Mắt Phố xuất hiện.
- Ánh sáng vàng hổ phách gợi phố đèn lồng Hội An.
- Gợi lại hành trình khởi đầu của Lumi.
- Không nhầm H7 beacon với restoration toàn Hội An.

### Bước 2 — Huế: Xác Thực

- Ấn Rồng Ngọc Tỉ sáng lên.
- Hoa văn triện và ánh sáng cung đình hòa vào đường kết nối.
- Nhấn mạnh gìn giữ ký ức di sản.

### Bước 3 — Đà Lạt: Định Hướng

- La Bàn Sương Lam xoay nhẹ.
- Ánh sáng lam xuyên qua làn sương tượng trưng.
- Một đường sáng định hướng tiếp tục nối các miền ký ức.

### Bước 4 — Jeonju: Lưu Giữ

- Mộc Ấn Hanji xuất hiện.
- Hoa văn giấy và ánh đèn tạo nên dấu ấn giao lưu.
- Gợi mối liên hệ giữa nghệ thuật thủ công Hàn Quốc và hành trình Việt–Hàn.

### Bước 5 — Gyeongju: Đọc Ký Ức

- Kính Ấn Tinh Tú chiếu ra hoa văn sao.
- Những ký ức cổ được soi sáng.
- Không yêu cầu nhánh G7 repaired thay vì defeated; cả hai kết quả hợp lệ.

### Bước 6 — Seoul: Kết Nối

- Lăng Kính Tam Relay hội tụ các luồng sáng.
- Sáu biểu tượng xuất hiện trong một bố cục thống nhất.
- Những đường ánh sáng đại diện sáu thành phố kết nối thành một mạng văn hóa.
- Lumi xuất hiện ở trung tâm hoặc vị trí nối các nguồn sáng.
- Màu sắc hài hòa, không che mất nhân vật và hình di vật.

**Đây là cảnh cao trào cuối game.**

Chuyển động cần có nhịp rõ ràng, không đơn thuần bật cùng lúc sáu icon rồi hiện chữ “Hoàn thành”.

Nhưng ưu tiên hiệu ứng nhẹ, ổn định trên Android. Không dùng shader nặng, cutscene dài hoặc texture độ phân giải quá lớn.

## 14. Các yêu cầu trình diễn

- Có thể Skip animation bằng thao tác rõ ràng sau khi đã xác nhận bắt đầu.
- Skip có chủ ý phải đưa game về kết quả ceremony hợp lệ, không làm save treo.
- Đóng popup thông thường không được ngầm tính là Skip/hoàn thành.
- Có xử lý gián đoạn giữa chừng và restart.
- Không render effect lên nút UI gây mất tương tác.
- Không che nội dung cần đọc.
- Phù hợp màn hình dọc 9:16 và 20:9.
- Có hỗ trợ mouse và InputSystem touch.
- Không xin camera/microphone permission.

Có thể tận dụng các trang ending đã có thay vì tạo hệ thống cutscene thứ hai.

---

# VIII. CỐT TRUYỆN VÀ HỘI THOẠI KẾT THÚC

## 15. Nội dung cần truyền đạt

Ending phải thể hiện được:

1. Mối nguy từ boss Seoul đã được giải quyết.
2. Sáu di vật tượng trưng cho sáu cách gìn giữ và kết nối ký ức.
3. Việt Nam và Hàn Quốc có bản sắc riêng nhưng có thể gặp nhau qua sự thấu hiểu.
4. Lumi trưởng thành qua những cuộc gặp gỡ, không chỉ nhờ chiến đấu.
5. Ánh sáng trở lại vì những người đã góp phần gìn giữ nó.
6. Hành trình có một kết thúc trọn vẹn nhưng thế giới vẫn còn có thể khám phá.

Giọng văn ấm áp, tự nhiên, có cảm xúc.

Không biến ending thành bài thuyết minh lịch sử dài.

Không dùng nội dung tuyên truyền, khẩu hiệu hoặc các tuyên bố lịch sử thiếu căn cứ.

## 16. Lời thoại phải giống lời nhân vật

Đây là yêu cầu đặc biệt của chủ dự án.

Trong UI mà người chơi nhìn thấy, tuyệt đối không dùng câu kiểu:

- “Bạn đã đến node S21.”
- “Hoàn thành flag restored.”
- “Đã xử lý điều kiện AND.”
- “Thoát không mất tiến độ.”
- “Lưu trạng thái vào SaveSystem.”
- “Bạn đã nhận fragment cuối.”

Những thuật ngữ kỹ thuật chỉ xuất hiện trong log, validator hoặc tài liệu lập trình.

Lời thoại NPC phải có vai trò và ngữ cảnh phù hợp.

Ví dụ cách viết đúng:

**Lumi, sau khi boss được giải quyết nhưng chưa đủ di vật:**

“Mọi người đã an toàn rồi... nhưng những ánh sáng chúng ta tìm thấy vẫn chưa thể hòa làm một. Có lẽ còn những câu chuyện đang chờ được lắng nghe.”

**Lumi, khi đã sẵn sàng làm lễ:**

“Chúng ta đã đi một chặng đường thật dài. Từ những ngọn đèn Hội An đến ánh sáng Seoul... giờ là lúc kết nối tất cả.”

Có thể biên tập lại cho hợp nhịp diễn xuất; không cần dùng nguyên văn.

NPC đã quen biết từ các thành phố khác có thể gửi lời chúc ngắn qua biểu tượng ký ức hoặc tín hiệu. Không bắt NPC dịch chuyển vật lý tới Seoul nếu gameplay chưa có cơ chế đó.

Không nhắc nhân vật chưa từng gặp như thể đã quen.

## 17. Lời kết của game

Đoạn kết cần ngắn, đáng nhớ.

Thông điệp chính:

**“Mỗi nền văn hóa là một ánh sáng riêng. Khi chúng ta thấu hiểu nhau, những ánh sáng ấy có thể tạo thành một nhịp cầu.”**

Có thể dùng câu tiếng Anh tương đương ở phần credits nếu phù hợp phong cách game.

Không bắt buộc nguyên văn câu trên, nhưng phải giữ đúng tinh thần.

---

# IX. CREDITS VÀ TRẠNG THÁI HOÀN THÀNH

## 18. Chỉ phát credits sau ceremony hợp lệ

Luồng chuẩn:

**Boss resolved → Sáu relic restored → Người chơi xác nhận → Ceremony hoàn tất → Ending → Credits.**

Không cho `ending_next`, `ShowEnding`, lựa chọn UI cũ hoặc gọi trực tiếp event bypass điều kiện.

Nếu credits có nhiều trang, cần có điều khiển Next/Skip hợp lý, không bị kẹt khi người chơi bấm nhanh hoặc tạm dừng.

Chỉ ghi global canonical completion một lần, theo một đường commit đáng tin cậy.

Không cấp tiền, item hoặc quest reward mới chỉ vì xem lại ending.

Không tự xóa save sau credits.

Sau credits, người chơi có thể quay về các thành phố đã mở theo gameplay hiện tại.

## 19. Giữ S22 đúng vai trò

Flag legacy `seoul.secret_ending` nếu vẫn liên quan SQ04 hoặc một thông điệp phụ thì không được tự hiểu là một ending thứ hai.

Giữ nội dung phụ hợp lệ, nhưng không cho nó phát credits hoặc ghi global completion độc lập.

---

# X. SAVE CŨ VÀ KHẢ NĂNG TIẾP TỤC SAU BOSS

## 20. Bảo vệ save

Phải kiểm thử các save:

- Trước khi mở Seoul.
- Đang làm relay.
- Giữa quá trình đánh boss.
- Boss đã thắng bằng chiến đấu.
- Boss đã được sửa thành công.
- Đã thắng boss nhưng thiếu di vật.
- Đã restored sáu di vật nhưng chưa làm ceremony.
- Đang trong ceremony.
- Đã hoàn thành ending mới.
- Đã xem credits legacy cũ.

Không xóa hoặc reset:

- 29 quest.
- 24 fragment grants.
- Sáu restored states.
- 18 check-in.
- Inventory, coins và reward ledger.
- NPC choices.
- World Map unlock.
- Battle/repair outcomes.
- Trạng thái boss đã giải quyết.

## 21. Chính sách đối với ending legacy

`game.ending_seen = true` trong save cũ **chỉ là bằng chứng người chơi từng xem ending cũ**.

Nó KHÔNG đủ để tự nhận đã hoàn thành nghi thức sáu di vật mới.

Chính sách được chốt:

1. Giữ nguyên lịch sử credits cũ.
2. Giữ boss victory, SQ03, rewards và những thành phố đã mở.
3. Không buộc người chơi đánh lại boss.
4. Nếu thiếu di vật, tiếp tục khám phá để restored đủ.
5. Khi đủ 6/6, cho phép chủ động thực hiện ceremony mới.
6. Sau khi ceremony mới hoàn tất, ghi canonical completion riêng và hợp lệ.

Không biến save cũ thành save mới hoàn thành 100% chỉ vì `ending_seen`.

Không tự trao sáu di vật cho save cũ.

## 22. Gián đoạn ceremony

Nếu đóng game giữa ceremony:

- Không tự cấp thưởng.
- Không tự đánh dấu hoàn thành nếu chưa tới điểm kết thúc hợp lệ.
- Không mất boss victory hoặc sáu relic restored.
- Khi load, có thể trở lại S21 để thực hiện/tiếp tục hợp lệ.
- Không chạy credits tự động chỉ vì load lại game.

Nếu ceremony đã commit thành công trước lúc đóng, reload phải nhận đúng completion và không commit lặp.

Ưu tiên persistence additive tương thích save v1/v2 đang dùng. Không thay schema quy mô lớn nếu không cần thiết.

Nếu bắt buộc phải sửa schema hoặc migration có nguy cơ làm mất save, dừng phần nguy hiểm, báo blocker cụ thể thay vì tự ý triển khai.

---

# XI. COLLECTION, JOURNAL VÀ TRẠNG THÁI ENDING

## 23. Tích hợp với giao diện hiện có

Trong Collection/Journal, cung cấp thông tin dễ hiểu:

- Số di vật đã phục dựng: 0–6/6.
- Trạng thái boss Seoul: chưa hoàn thành / đã giải quyết.
- Nghi thức kết nối: chưa sẵn sàng / sẵn sàng / đã hoàn thành.

Nếu người chơi thiếu di vật, hiển thị tên di vật còn thiếu khi đã có thông tin phù hợp.

Không tiết lộ các node HIDDEN, bí mật chưa khám phá hoặc lời giải minigame.

Không tạo một JournalManager thứ hai.

Không biến check-in 18/18 thành điều kiện ending.

Thành tựu check-in 18/18 là hệ riêng, không ảnh hưởng sáu di vật hoặc final ceremony.

---

# XII. KIỂM THỬ BẮT BUỘC

## 24. Tối thiểu 16 tình huống ending

Phải kiểm thử:

| ID | Tình huống | Kết quả |
|---|---|---|
| E01 | Boss chưa thắng, 0/6 relic | Không ending |
| E02 | Boss thắng, 0/6 relic | Chơi tiếp, không credits |
| E03 | Boss thắng, 3/6 relic | Chơi tiếp |
| E04 | Boss thắng, 5/6 rồi restored di vật cuối | S21 sẵn sàng, không đánh lại boss |
| E05 | Có 6/6 trước boss | Vẫn phải giải quyết boss |
| E06 | Boss + 6/6 + ceremony xác nhận | Một ending hợp lệ |
| E07 | Restart sau boss, thiếu relic | Boss không hồi sinh |
| E08 | Restart đủ 6/6, chưa ceremony | Không tự kết thúc |
| E09 | Restart sau ending | Giữ completion, không thưởng lại |
| E10 | VN_FIRST | Hoàn thành ending |
| E11 | KOREA_FIRST | Hoàn thành ending |
| E12 | Old save đã thắng boss | Có thể tiếp tục thu mảnh |
| E13 | Main story bỏ qua relic | Vẫn thắng boss, không global ending |
| E14 | Fragment liên thành phố | Vẫn lấy được hợp lệ |
| E15 | Post-boss quay lại NPC/check-in | Không bị khóa |
| E16 | Boss + 6/6 nhưng chưa xác nhận, reload | Không auto-ending |

## 25. Kiểm thử bổ sung

Ngoài E01–E16, phải kiểm tra:

- Boss battle và boss repair đều mở đúng ceremony.
- SQ03 hoàn thành tại S21 dù thiếu relic.
- Reward SQ03 không lặp.
- Gọi trực tiếp `final_festival`/ending action trước điều kiện không vượt guard.
- Không thể mở credits bằng lựa chọn legacy khi mới có 0–5/6.
- Xem và đóng popup chưa tính là xác nhận ceremony.
- Nhấn Next/Skip nhiều lần không phát ending lặp.
- Thoát giữa ceremony rồi restart.
- Old save có `game.ending_seen` nhưng 0–5 relic.
- Old save có cả 6/6 relic.
- Sau credits vào World Map và các thành phố cũ.
- BGM chuyển từ boss sang hậu boss/ceremony không chồng tiếng.
- Unity portrait UI không mất nút xác nhận hoặc Skip.
- Node HIDDEN không bị reveal sai.
- H6 check-in, 18 Photo Journal và các map effects không bị hồi quy.

Kiểm tra cả luồng earned thực sự và targeted fixtures. Nếu dùng fixture, ghi rõ; không gọi fixture là một hành trình fresh hoàn chỉnh.

## 26. Bảo toàn dữ liệu

Sau Phase 5 vẫn phải có:

- 6 thành phố.
- 133 canonical nodes.
- 155 route edges hiện hành nếu source không đổi.
- 29 canonical quests.
- 22 story puzzles.
- 24 fragment IDs.
- 6 relic IDs.
- 18 check-ins.
- Hai route VN_FIRST/KOREA_FIRST.
- Một canonical ending.

Không tạo quest, puzzle, boss hoặc thành phố mới.

Giữ nguyên hệ NPC/marker đang chờ task riêng và các hình check-in đã dùng.

## 27. Kiểm thử build

Chạy Windows build và Windows Player thật.

Kiểm thử:

- Compile.
- Runtime exceptions.
- Save/restart.
- UI mouse/touch simulation.
- E2E hai tuyến.
- Visual ending.
- Audio transitions.
- Regression các file chung bị sửa.

Nếu Android chưa build hoặc chưa có thiết bị thật, ghi rõ:

`ANDROID_BUILD_NOT_TESTED`

`ANDROID_DEVICE_NOT_TESTED`

Không tuyên bố Android PASS từ mouse/touch simulation trên Windows.

---

# XIII. HÌNH ẢNH NGHIỆM THU

## 28. Cung cấp ảnh runtime thật

Chụp ít nhất:

1. S21 sau boss khi mới có 3/6 relic.
2. Journal hiển thị di vật còn thiếu.
3. S21 khi đủ 6/6 và boss resolved.
4. Popup xác nhận bắt đầu ceremony.
5. Ceremony bắt đầu với ánh sáng Hội An/Huế.
6. Ceremony khi sáu nguồn sáng hội tụ.
7. Lumi trong khoảnh khắc kết nối cuối.
8. Ending dialogue và thông điệp kết thúc.
9. Credits.
10. Trạng thái hậu credits và World Map.
11. Save/re-entry khi ending đã hoàn thành.

Ảnh phải đến từ Unity runtime hoặc Windows Player thực tế.

Không dùng concept/mockup để làm bằng chứng runtime.

Nếu animation chuyển động không thể đánh giá bằng ảnh, cung cấp chuỗi frame có timestamp hoặc video ngắn nếu công cụ hiện hành hỗ trợ.

Không tự tuyên bố cinematic đã được chủ dự án duyệt về mặt thẩm mỹ.

---

# XIV. BÁO CÁO VÀ CHECKPOINT

## 29. Tài liệu cần cập nhật

Tạo hoặc cập nhật:

- `Docs/Story/ONE_ENDING_SIX_RELICS_CANONICAL.md`
- `Docs/Gameplay/SEOUL_FINAL_CONNECTION_DESIGN.md`
- `Docs/Story/FULL_STORY_PLAYTEST.md`
- `Docs/QA/FINAL_ENDING_AND_POST_BOSS_AUDIT.md`
- `Docs/Gameplay/PHASE5_IMPLEMENTATION_REPORT.md`
- `Docs/QA/PHASE5_ENDING_E2E.md`
- `Docs/QA/PHASE5_RUNTIME_REVIEW.html`
- `Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md`

Ưu tiên đường dẫn đang có, không tạo nhiều báo cáo trùng nội dung.

Ghi rõ:

1. Source và event trước khi sửa.
2. S21 legacy bypass đã được chặn ở đâu.
3. Sáu restored states được truy vấn thế nào.
4. Boss resolved dùng evidence nào cho battle/repair.
5. SQ03 được hoàn thành thế nào.
6. Điều kiện UI/backend ceremony.
7. Save field mới nếu có, cách tương thích save cũ.
8. Flow Skip/Cancel/Restart.
9. Hình ảnh và nội dung ending.
10. Cách dùng lại BGM hiện hành.
11. File thay đổi và protected-file verification.
12. Kết quả E01–E16 và kiểm thử bổ sung.
13. Windows build.
14. Android status.
15. Các lỗi P0/P1/P2/P3 còn tồn tại.
16. Các hạng mục chờ nghiệm thu thẩm mỹ.

Không sửa ngược báo cáo lịch sử để biến FAIL cũ thành PASS.

## 30. Checkpoint

Tạo checkpoint tăng dần dựa trên **checkpoint mới nhất thực sự đã được áp dụng**, bao gồm Phase 4 và các thay đổi audio hợp lệ.

Không tự checkout source Phase 4 cũ làm mất BGM đã tích hợp.

Không sao chép lại toàn bộ Library, Temp hoặc Builds nhiều GB nếu không cần.

Ghi base checkpoint, hash, changed-files manifest và hướng dẫn restore chính xác.

---

# XV. ĐIỂM DỪNG CUỐI

Sau khi hoàn thành Phase 5, game phải đạt:

**SIX RELICS RESTORED + BOSS RESOLVED + PLAYER-CONFIRMED CEREMONY = ONE CANONICAL ENDING.**

Người chơi không đủ relic vẫn được thắng boss và tiếp tục khám phá, nhưng chưa thể kết thúc toàn game.

Người chơi đủ relic nhưng chưa thắng boss vẫn phải hoàn thành boss.

Người chơi đã hoàn thành ending được giữ save, có thể quay lại khám phá và không bị nhận thưởng lặp.

**Không thực hiện các việc ngoài Phase 5:**

- Không mở lại Phase 4.
- Không làm lại NPC/marker audit.
- Không đổi NPC đã được owner approve.
- Không làm lại sáu hiệu ứng phục dựng Phase 4.
- Không làm lại nhạc nền hoặc nhạc chiến đấu.
- Không chỉnh đường đi/terrain Phase 3.5.
- Không sửa toàn bộ lời thoại sáu thành phố.
- Không tự thêm âm thanh nút bấm.
- Không tự bắt đầu Phase 6 hoặc Phase 7.

Các lỗi câu chữ nhỏ ngoài ending, icon Seoul chưa final, artwork check-in, terrain và các hạng mục polishing khác được ghi backlog cho Phase 6–7, trừ khi trực tiếp gây crash, softlock hoặc làm ending không thể hoàn thành.

**Sau khi triển khai, chạy QA, tạo báo cáo và lưu checkpoint: DỪNG ĐỂ CHỦ DỰ ÁN NGHIỆM THU ENDING.**

**ĐÂY LÀ LỆNH TRIỂN KHAI GAMEPLAY THẬT, KHÔNG PHẢI YÊU CẦU CHỈ VIẾT THIẾT KẾ.**