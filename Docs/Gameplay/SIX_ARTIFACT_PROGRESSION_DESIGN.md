# Phase4 — implemented progression / persistence

| City / di vật | Nguồn | Điểm phục dựng | Trial thật | Saved collected / assembled / trial / restored | World Reaction |
|---|---|---|---|---|---|
| hoi_an · Đèn Mắt Phố | H0/H4/H10/H2 | H2 | Dẫn Ánh Đèn cũ; cần H7/HQ01 | 4/4 · true · true · true trong earned final | A/B/C;10 halos giữ nguyên |
| jeonju · Mộc Ấn Hanji | J2/J3/J7 + proof H6 | J2 | Xoay4 lớp hoa văn đúng mẫu | 4/4 · true · true · true trong earned final | A/C; đèn xưởng + dải giấy |
| hue · Ấn Rồng Ngọc Tỉ | U3/U8/U16/U17 | U7 | Rà4 nét + căn hướng mặt ấn | 4/4 · true · true · true trong earned final | A/C; điểm sáng hai bờ |
| da_lat · La Bàn Sương Lam | D0/D2/D8/D12 | D2 | Vuốt sương + tìm4 dấu + căn Đông Bắc | 4/4 · true · true · true trong earned final | A/C; tan lớp sương trang trí |
| gyeongju · Kính Ấn Tinh Tú | G3/G4/G7/G15 | G9 | Rà kính + đọc3 dấu, loại mồi + căn Tây | 4/4 · true · true · true trong earned final | A/C; sao di tích, G7 khác màu theo nhánh |
| seoul · Lăng Kính Tam Relay | S2/S7/S10/S18 | S7 | Kéo3 luồng + xoay hướng, hội tụ hình học | 4/4 · true · true · true trong earned final | A/C; tín hiệu3 relay dạng biểu tượng |

Một RelicCollection, flags relic.proof/fragment/assembled/trial/restored + ledger relic:v1:claim dùng savev1/v2 hiện có, atomic rollback khi persist thất bại, không field/schema mới.24nguồn đã hoạt động và tested hai route/old-save; proofAny thay các literal OR ở metadata dormant. Trial chỉ thay restored sau đúnginput+explicitconfirm. Source one-shot cũ không phải replay reward; claim mới độc lập.

A Adventure = quest/beacon/world/boss cũ, không relic gate. B Collection =24→6restored đã implement. C Completion = boss +6 +nghi thức cuối **Phase5 chưa implement**; legacyS21 còn pending. Các đề xuất/alternatives Phase0 dưới đây là lịch sử, bị quyết địnhD01–D08/mergedPhase4 và runtime phía trên supersede. [Current access](ARTIFACT_FRAGMENT_ACCESS_MATRIX.md) · [QA](../QA/PHASE4_SIX_RELICS_E2E.md).

## Historical Phase0 design (không phải runtime hiện hành)

# Six Artifacts — progression/persistence design (Phase 0)

2026-10-18 · DESIGNED_ONLY. Master mới duy nhất; không áp yêu cầu “relic không liên quan ending” của V2 cũ vào thiết kế mới. Dữ liệu runtime vẫn nguyên, chưa triển khai core/trial/collection.

## Ba trục độc lập

A Adventure: quest/beacon → WorldMap + discovery/gates → relays/boss. Giữ mọi node/29 quest/22 puzzle/item/reward. B Collection: 24 nguồn có proof + interaction → 4/4 từng relic → assembly thật → trial thành công → restored + visual. C Completion: **boss resolved AND đủ sáu restored → ready; player xác nhận nghi thức S21 → sole ending**. Không có relic → vẫn đánh boss/mở đủ map, chưa phá đảo toàn game.

Các `fragment_*`/`relic_*` là tên thiết kế chưa xuất hiện trong runtime; `artifact_*` chỉ alias ảnh. Không thêm 24 quest hoặc item clone. Không dùng việc sở hữu Mắt Đèn/ấn tín/ống kính/relay làm bằng chứng một relic đã phục dựng.

## Từng relic: thời điểm và thao tác sau approval

| Relic | Bốn nguồn đề xuất A | Điểm phục dựng candidate | Sớm nhất trước main boss? | Trial thật / ranh giới |
|---|---|---|---|---|
| Đèn Mắt Phố Hội An | H0,H4,H10,H2 | H2 | Có: sau trả tutorial và H4/H10 stage1; B phải sang J2 rồi quay về | Drag bốn phần vào slot + xoay/guiding ánh đèn; chỉ trial thành công mới full-city overlay; H7 không mở bằng relic |
| Ấn Rồng Ngọc Tỉ | U3,U8,U16,U17 | U7 | Có: sau U8 (U17 đã xong), B chờ G14 | Ghép/xoay lớp triện, rà nét và đóng ấn fantasy; không đổi ancient_seal U11 |
| La Bàn Sương Lam | D0,D2,D8,D12 | D2 | Có: sau D12 stage2; B chờ S16 | Vuốt sương trang trí, xoay hướng từ hồ/thông/chuông; visual hint thay cho audio nếu mute |
| Mộc Ấn Hanji | J2,J3,J7 + H6 | J2 | Có: sau J3/J7 và dấu H6; không phải đợi Huế | Kéo/xoay giấy/chỉ/dấu theo mẫu đã thấy; không solve lại J7/J12/J17 |
| Kính Ấn Tinh Tú | G3,G4,G7,G15 | G9 | Có: sau G7 đã đánh hoặc sửa | Ghép kính rồi rà tia trên nét sao fantasy; hai nhánh tượng cùng nhận được fragment, phản hồi khác |
| Lăng Kính Tam Relay | S2,S7,S10,S18 | S7 | Có: SQ01 đã báo + S10, trước S19/S20 | Bốn collectible milestone vẫn đúng tên; ba luồng chỉ visual minigame; không tự bật relay/boss/ending |

Nguồn chưa là fragment grant; toàn bộ thời điểm là **mô hình đề xuất**, không runtime PASS. Không hứa thời gian chơi cụ thể. Mỗi city cần thử claim 10–30s và restoration 30–90s bằng gesture thật, retry không mất mảnh, exit không complete; vẫn bảo toàn 22 story puzzle riêng. Không thêm puzzle thứ 23 vào catalog truyện để giả có six minigames.

## Mô phỏng hai tuyến A (không fixture player)

Cột đếm là **số nguồn có thể đủ điều kiện sau khi giải quyết nội dung chương** nếu phương án A được duyệt, đã quay lại/thực hiện các interaction và trial tương ứng. Không phải số đã thu thực tế. Ví dụ H6 cấp dấu Jeonju nhưng home vẫn Jeonju; vì vậy sau Hội An có thể có năm evidence slots tiềm năng. Trước khi tới J2 vẫn không phục dựng Jeonju.

| Tuyến | Mốc chương | City đã mở | Nguồn /24 tiềm năng | Relic tối đa có thể phục dựng | Còn chờ |
|---|---|---|---:|---:|---|
| VN_FIRST | hoi_an | hoi_an, hue, jeonju | 5 | 1 | da_lat, gyeongju, hue, jeonju, seoul |
| VN_FIRST | hue | da_lat, hoi_an, hue, jeonju | 9 | 2 | da_lat, gyeongju, jeonju, seoul |
| VN_FIRST | da_lat | da_lat, hoi_an, hue, jeonju | 13 | 3 | gyeongju, jeonju, seoul |
| VN_FIRST | jeonju | da_lat, gyeongju, hoi_an, hue, jeonju | 16 | 4 | gyeongju, seoul |
| VN_FIRST | gyeongju | da_lat, gyeongju, hoi_an, hue, jeonju, seoul | 20 | 5 | Seoul relay/tháp (chưa boss) |
| VN_FIRST | Seoul relay/tháp (chưa boss) | da_lat, gyeongju, hoi_an, hue, jeonju, seoul | 24 | 6 | Boss/ceremony nếu chưa làm |
| KOREA_FIRST | hoi_an | hoi_an, hue, jeonju | 5 | 1 | da_lat, gyeongju, hue, jeonju, seoul |
| KOREA_FIRST | jeonju | gyeongju, hoi_an, hue, jeonju | 8 | 2 | da_lat, gyeongju, hue, seoul |
| KOREA_FIRST | gyeongju | gyeongju, hoi_an, hue, jeonju | 12 | 3 | da_lat, hue, seoul |
| KOREA_FIRST | hue | da_lat, gyeongju, hoi_an, hue, jeonju | 16 | 4 | da_lat, seoul |
| KOREA_FIRST | da_lat | da_lat, gyeongju, hoi_an, hue, jeonju, seoul | 20 | 5 | Seoul relay/tháp (chưa boss) |
| KOREA_FIRST | Seoul relay/tháp (chưa boss) | da_lat, gyeongju, hoi_an, hue, jeonju, seoul | 24 | 6 | Boss/ceremony nếu chưa làm |

Mốc Seoul trong mô hình là hoàn tất nguồn/relay/tháp, chưa giả boss đã thắng: `allRelicsRestored` có thể true trước boss nhưng ceremony vẫn false. VN_FIRST/KOREA_FIRST WorldMap flags lấy đúng JSON runtime; eligibility fragment và trial chưa có code. [Projection từng bước](Phase0-20261018/route-design-simulation.json).

B-Hội An: thiếu đúng móc cho tới J2, không giảm quyền mở city/đánh boss. B-Huế: thiếu văn khắc tới G14, không cản U19/D14. B-Đà Lạt: thiếu hạt tới S16, buộc quay lại hồ sau city cuối; không làm cả sáu relic đều chờ Seoul. Jeonju-C dâu kéo completion Jeonju về sau Đà Lạt: khuyến nghị không chọn để food sidequest vẫn optional với collection ending. All 24 nguồn trong mô hình sau cả sáu map đều có thể quay lại, nhưng grant chưa implement nên không gọi là 24/24 PASS.

## SKIP_RELICS / OLD_SAVE_AND_POST_BOSS

SKIP_RELICS: chạy cùng hai tuyến A adventure, thu 0 mảnh, H7/U19/J19/D14/G19 vẫn mở map, relays/story items vẫn dùng bình thường, S20 battle/repair win vẫn lưu. Target dừng ở `BOSS_DEFEATED_PENDING_FINAL_CONNECTION`, không credits. Runtime hiện chưa có guard six-relic này, vẫn cho bấm ending S21; đây là khoảng thiếu cần Phase5, không vá Phase0.

OLD_SAVE/POST_BOSS: đọc `completed`, `solvedPuzzles`, encounter resolution và flags từ save cũ; không đòi item đã tiêu còn trong túi. Quay lại source node và claim mới có điều kiện, không tự chạy reward của event cũ. Hai earned historical postgame saves cho **48/48 anchor reachable bằng static BFS**, không phải fragment grant test. Save V1 chưa có proof puzzle thì không suy diễn đã giải từ tutorial; recovery claim chỉ sau bằng chứng hợp lệ. Old credits/mid-credits cần D07 user decision, không lùi quest/boss/reward.

## Loop/missable audit

| Nguy cơ cụ thể | Loại | Cách cắt trong thiết kế |
|---|---|---|
| Relic Hội An mở Jeonju, móc ở Jeonju | INVALID_LOCK_LOOP nếu thêm cạnh này | Không có cạnh relic→city; giữ H7 mở Jeonju |
| La Bàn mở Seoul, hạt ở S16 | INVALID_LOCK_LOOP nếu thêm cạnh này | Seoul chỉ bốn beacon, B muộn nhưng không cycle |
| G7 phải `stone_guard_friend` mới nhận mảnh | Permanently missable khi đã chọn đánh | Dùng `guard_resolved`, cả battle/repair; friend chỉ visual/combat weakness |
| Source mới chỉ grant khi submit puzzle cũ | Old-save oneShot missable | Claim từ solved/completed proof ở node sau khi giải xong; không replay reward |
| H6 đã check-in, CanCheckIn false | Mất mảnh nếu gắn vào mark-only một lần | Photo revisit/claim riêng, dùng old mark proof; không cấp 2xu/postcard lần nữa |
| Đổi three Seoul visual sources thành 4 collectible | ASSET_SEMANTIC_MISMATCH | Giữ bốn milestone; art pending, three inputs không là fragment inventory |
| DQ05/HQ05 bị ép làm mới cho mảnh | Optional quest thành yêu cầu global, có thể double consume | Dấu H6/interaction greeting độc lập; food chỉ clue, old delivery proof không trừ lần hai |
| Legacy ending_seen suy thành global complete mới | Premature ending/migration ambiguity | Giữ history; chuẩn mới cần AND+ceremony, chờ D07 |

Graph đề xuất không có loop relic→map→fragment vì không tạo relic→map/boss edges. Navigation graph có vòng là bình thường, không gắn nhãn softlock. Rủi ro phần chưa có runtime được ghi BLOCKED_BY_MISSING_RUNTIME_STATE, không chứng nhận an toàn bằng một script tự cấp flags.

## Persistence và kiến trúc extension (chưa code)

Hiện SaveData version2 có flags/resolvedEvents/choices/rewardGrantIds/cityProgress, loader chấp nhận v1/v2, ghi `.tmp` rồi File.Replace/`.bak`. Chưa có collection fields. Khuyến nghị tận dụng **một registry collection** và các khóa namespaced trong flags/ledger hiện có; derive 4/4, assembled, trial success, restored bằng stable IDs, không lưu hai hệ artifact/relic. Các key tương lai chưa được ghi vào save ở Phase0.

Transaction đề xuất: kiểm city/node/discovery/proof + chưa có fragment → thao tác thành công → ghi fragment + ledger cùng snapshot → chỉ UI success sau persistence thành công. Nếu ghi lỗi, không coi RAM-only là đã có mảnh/ending; cần test fail-write/rollback trong core hiện có, không tạo SaveSystem thứ hai. Chưa chốt schema mới hoặc sidecar. Nếu audit implementation phát hiện phải nâng schema lớn, dừng NEEDS_USER_DECISION; không đổi version vượt2 vì loader hiện từ chối >2.

QuestManager/Inventory vẫn chủ sở hữu gameplay cũ; collection grants không gọi GrantOnce cho story item đã tiêu hoặc reward lần nữa. PuzzleManager giữ đúng22 puzzle; restoration dùng definition/type riêng trên core/UI chung sau approval. Existing CityMap partial có local reaction: chỉ source tutorial; full-city visual tương lai chỉ đọc restored relic, không có hai controller quyết định cùng trạng thái. Scene/UI/Input dùng framework hiện có.

Collection hiển thị 0/4–4/4 và six restored count; chưa discovered thì gợi ý vùng chưa biết, không tiết lộ node/route/NPC HIDDEN. WorldMap vẫn chỉ quest/beacon. Journal tách adventure / collection / final connection, không ghi thiếu relic là thua boss. Postcard metadata đủ, không ghi hàng trăm PNG hoặc yêu cầu OS camera.

## Mốc tiếp tục được bảo lưu

Sau user duyệt Phase0: Phase1 bốn portrait, Phase2 source/slicing/core design, Phase3 một Hội An pilot rồi **dừng lần hai**. Phase4 thứ tự thi công J→U→D→G→S không thay thứ tự chơi. Phase5 mới sửa ending guard/migration. Phase6 art/reactions, Phase7 test16case và release audit riêng. Không gộp phase hoặc báo final từ design.
