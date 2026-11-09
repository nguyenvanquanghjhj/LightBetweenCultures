# Đồng bộ side dialogue di vật — Phase 3, 18/10/2026

Runtime mới: `CulturalRelics.json.story` → `RelicCollection.Story` → `NodeEventManager` / `GameplayPanels`. Lời quest gốc đứng trước; options/requirements không đổi. H0/H4/H10/H2 có manh mối và phản ứng riêng; H3 nhặt Mắt Đèn chưa được nói là trả xong; H2 local / H7 beacon / full-city relic là ba proof độc lập. Check-in H2 mở sau trả tutorial như checkpoint trước; bưu thiếp H6 giữ metadata thời điểm chụp. Xem phần Hội An và hàng đợi Phase 4–5 trong FULL_STORY_PLAYTEST.md.

Phần dưới là báo cáo lịch sử WR V2/pilot, không phải authority để ghi đè trạng thái hiện tại. Những dòng lịch sử “chưa có Six Relics runtime” đã được thay bởi pilot Hội An; năm di vật khác vẫn PENDING.

# Current approved integration —18/10/2026

Pilot thực tế: pickup H3 không restore; completed mai-lantern → local đèn H2; HQ01/H7 → beacon/mở Huế/Jeonju;4 collection mảnh+assembly+Dẫn Ánh Đèn+HQ01/H7 → `relic.restored.relic_hoi_an_lantern`, ánh sáng landmark Hội An. H2 reflection/check-in chỉ sau trả tutorial; H6 photo có metadata, không thưởng lặp. Source of truth là RelicCollection/CulturalRelics.json, không manager thứ hai.

D07 canonical: relic không gate map/boss, nhưng6 restored cần cho global ending cùng boss resolved+Seoul ceremony. **Phase5 chưa implement**, không dùng lời cũ dưới đây về optional ending làm hợp đồng mới. [Pilot](../Gameplay/CULTURAL_ARTIFACTS_HOIAN_PILOT_REPORT.md).

---

## Hồ sơ WR V2 trước tích hợp, giữ làm bối cảnh lịch sử

# Hội An — hai mức phục hồi ánh sáng (World Reactions V2)

Ngày 2026-10-17. Thay thế diễn giải World Reactions V1; không thay progression đã phát hành.

## Story cũ và hiệu chỉnh runtime

Trước đây city story nói H7 làm “đèn cả phố sáng trở lại”, lời cảm ơn và HUD gọi chung là khôi phục ánh sáng; CityMap còn phát một lớp vàng toàn viewport khi `hoi_an.lamp_restored` chuyển true. Dễ hiểu nhầm rằng trả Mắt Đèn hoặc HQ01 đã Khai Sáng toàn Hội An.

Hiện runtime có **hai quest khác nhau**, không gộp lại theo cách diễn đạt của prompt:

- `mai-lantern`: nhận tại H2, pickup vật lý H3, trả Cô Mai H2. `QuestManager.TalkToMai()` hoàn tất tutorial, tiêu hao `lantern-part`, giữ `lamp_eye` cho HQ01. Đèn đầu tiên trước tiệm sáng tại đây.
- `HQ01`: vẫn cần `lamp_eye`, `hanji_core`, `link_key`, giải quyết Bot Mờ-01 và lắp ở H7. Vẫn tiêu hao đúng ba vật phẩm, thưởng 150 xu, mở Huế/Jeonju. Ý nghĩa ánh sáng là **đồng bộ trạm giao lưu**, không phải Khai Sáng di vật.

Mắt Đèn không được đổi itemId, không phải một fragment Six Relics mới và không bị đổi điều kiện thu thập.

## Ba giai đoạn và nguồn sự thật

| Giai đoạn | State runtime hiện có | Hiển thị | Không được suy ra |
|---|---|---|---|
| A — trước pickup | tutorial chưa complete; `hoi_an.lamp_eye_found` false | Vệt trầm nhỏ quanh đèn H2, đèn chưa bật; H3 giữ pickup gốc | Không tối màu toàn ảnh canonical |
| Giữa A/B — đã nhặt, chưa trả | `hoi_an.lamp_eye_found` true; tutorial chưa complete | Glow ngắn dưới Lumi + nhún, HUD gốc chỉ quay về; H2 vẫn chưa sáng | Không complete quest, không mở gate, không thưởng |
| B — trả Cô Mai | `IsCompleted("mai-lantern")`; lịch sử `lantern-lit` vẫn giữ | Một đèn `lamp_eye` hiện có và halo ấm tại góc tiệm H2; Lumi phản ứng ngắn; Mai nhắc Đèn Mắt Phố | Không complete HQ01 sớm, không toàn map sáng |
| H7 — mốc main hiện có | HQ01 complete / `hoi_an.lamp_restored` | Giữ đèn H2, trạm và marker theo main hiện có; bỏ flash phủ viewport Hội An | Không coi flag cũ là relic restored/trial complete |
| C — Hội An Khai Sáng | **Chưa có state Six Relics thật trong project** | **OWNED_BY_SIX_RELICS / NOT_IMPLEMENTED** | Không mượn flag HQ01/tutorial/ending làm trigger |

Các state trên được derive khi load/re-entry. Thời điểm pulse/nhún chỉ nằm trong bộ nhớ, không thêm save flag. Re-entry dựng warmth tĩnh, không gọi reward/pickup hay phát lại burst.

## Các câu runtime đã sửa

**16 trường văn bản data + 1 chuỗi HUD = 17 vị trí runtime**, không phải 17 lượt thoại mới. Danh sách đầy đủ old/new tại [runtime-dialogue-diff.json](../QA/WorldReactionsV2-20261017/runtime-dialogue-diff.json).

| Nguồn | Trường | Hiệu chỉnh |
|---|---|---|
| GameplayContent city Hội An | opening, story | Mắt Đèn là manh mối; một góc sáng; H7 đồng bộ trạm; relic là phụ |
| HQ01 | offer, thanks | Giữ ba linh kiện, H7 và cổng; bỏ khẳng định toàn phố đã sáng |
| H2 `npc_mai` | text | Lời nhờ tìm Mắt Đèn về trước, rồi tiếp tục main |
| H7 `restore_lantern` | completedText | Trạm gửi tín hiệu; cả phố cổ vẫn chờ Đèn Mắt Phố |
| Jeonju `puzzle_lantern`, `chapter_jq03_offer` | text | “Hội An sáng” → tín hiệu trạm giao lưu |
| JQ03 | offer | Cùng hiệu chỉnh tín hiệu, không thay nhiệm vụ |
| `puzzle_lantern` | clue | Chỉ sửa ngữ cảnh; giữ Hoa → Trăng → Đèn, mode/options/answer/requirements |
| Check-in H6 | description | Đốm sáng rải rác; không nói cả hai bờ đã khôi phục |
| Seoul city | ending | Lời chào/tín hiệu sáu thành phố, không xác nhận relic |
| `final_festival` | endingPages[0] | Hồi tưởng chiếc đèn đầu tiên; các trang/credits khác giữ nguyên |
| HoiAnData tutorial | offer, reminder, thanks | Tìm Mắt Đèn; đèn trước tiệm; gợi bí ẩn Đèn Mắt Phố |
| QuestManager | Status literal | “Trạm giao lưu sẵn sàng”; toàn bộ hàm/điều kiện giữ nguyên |

`NodeEventManager.BuildOptions` cho Mai nhắc cảm ơn cục bộ khi tutorial đã xong nhưng HQ01 còn tiếp diễn. Giữ lời hỏi tiến độ và lựa chọn cũ; body dùng khung scroll/Next đã có, không xây UI mới.

## Hiệu ứng cục bộ

`AdventurePresentation.questLights` cấu hình H2, questId `mai-lantern`, tâm normalized `(0.383287, 0.608002)`, bán kính `(0.043, 0.055)`. Đây là anchor của đèn hiển thị, không phải node hay arrival anchor. Không thay H2 marker `(0.383287,0.593002)` hoặc đường đi.

`CityMap.QuestReaction.cs` là partial của controller đang có: lớp UI phía sau node/route/actor, không raycast; chỉ hiện nếu H2 đã visible. Dùng item sprite `lamp_eye` hiện có và glow thủ tục nhẹ. Không ghi đè Map Overview A. H3 chỉ thêm renderer feedback dưới chân; player root, collider và điều khiển giữ nguyên.

## Hợp đồng tích hợp Six Cultural Relics

Audit source/runtime và Docs Story liên quan chưa thấy hệ Six Relics thật. Không tạo registry, fragment, quest, flag save hoặc manager thay thế trong task này.

Task Six Relics phải sở hữu nguồn sự thật cho: đủ bốn mảnh Đèn Mắt Phố → phục dựng hợp lệ → **hoàn thành Dẫn Ánh Đèn**. Chỉ sau bước cuối mới được dựng presentation toàn Hội An. Chỉ ghép xong chưa đủ.

Khi tích hợp: dùng state/query/event thật của hệ đó; rebuild hình tĩnh từ save, phát celebration chỉ tại transition hợp lệ, không trao lại reward. Đặt layer riêng với lớp local H2 này, tương thích discovery (không lộ HIDDEN), không chặn touch; tín hiệu Jeonju chỉ presentation. Không thêm relic vào điều kiện main, cổng, boss, readiness hay ending. Người không chơi relic vẫn hoàn tất game cũ.

Không tự định nghĩa tên flag tương lai trước chủ sở hữu task. Không sửa báo cáo của task Six Relics khác. Không có runtime screenshot giai đoạn C để báo PASS.

## Bảo toàn và phần còn thiếu

So sánh trước/sau tại [invariants.json](../QA/WorldReactionsV2-20261017/invariants.json): 133 node/29 quest; toàn bộ node/edge/route/arrival/gate/discovery/reward/objective/item/enemy/đáp án giữ nguyên. SaveSystem, scene H3, InteractionSystem, player controller, canonical registry và AdventureUIArt không đổi hash. Dữ liệu chỉ đổi các trường text đã nêu và thêm cấu hình visual local.

NEEDS_ART: biểu cảm/walk-cycle Lumi được vẽ riêng; lớp đèn/phản chiếu toàn Hội An cho Six Relics cần art direction/trigger thật. Không vẽ đè map hoặc dùng portrait làm walk-cycle.

NEEDS_AUDIO: chưa có audio file phù hợp trong Assets/_Game; cần pickup nhẹ, bật đèn ấm, completion ngắn với nguồn/giấy phép rõ. Không thêm âm thanh giả rồi báo đã hoàn chỉnh.
