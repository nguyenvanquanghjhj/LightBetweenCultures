# Cultural artifacts integration — Phase 1–3 · 18/10/2026

**Một core collection; chỉ Hội An có gameplay mới.** [Pilot/report nghiệm thu](CULTURAL_ARTIFACTS_HOIAN_PILOT_REPORT.md) · [24-matrix được duyệt](ARTIFACT_FRAGMENT_ACCESS_MATRIX.md) · [Native art manifest](../Art/SIX_RELICS_ART_MANIFEST.md).

## Kiến trúc thực tế

- `Content/CulturalRelics.json`:6 relic_* identities,24 unique fragments, source/proof/requiredQuest/requiredFlag/enable/artStatus và lightNodes. artifact_* không thành state hoặc hệ thứ hai.
- `RelicCollection`: service duy nhất trên GameBootstrap, sử dụng SaveData flags/ledger v1/v2. Không nâng version hoặc sửa SaveSystem. Chỉ4 nguồn Hội An enabled;20 fragment và5 trial còn lại chưa triển khai.
- `RelicActivity`: phiên tương tác tạm, kiểm current city/node/discovery; wrong drop/rotation/beam/thiếu proof không cấp mảnh. Exit/retry không tiêu mảnh/tiền. Không lưu phiên đang kéo; owned/assembled/restored/photo đã commit vẫn giữ qua restart.
- `GameplayPanels.Relics`: mở từ options event/Quest Journal hiện có, drag qua input module đang dùng. Không Canvas/EventSystem/scene/manager mới. Gallery dùng AdventureUIArt relics/fragments serialized Sprite refs native.
- `CityMap.RelicLight`: layer thị giác không raycast, derive từ restored, chỉ decorate lightNodes đã REVEALED. Fade1.2s, glow giá rẻ dùng gradient64² sẵn; không đổi canonical bitmap, waypoint hoặc discovery. Lumi dùng transform feedback sẵn, không face/walk-cycle mới.

## Persistence / bằng chứng

| State | Key / guard |
|---|---|
| Source chứng minh bằng thao tác thật | `relic.proof.<fragmentId>` |
| Đã sở hữu | `relic.fragment.<fragmentId>`; là nguồn sự thật collection |
| Ledger claim riêng | `relic:v1:claim:<fragmentId>` trong rewardGrantIds; không tạo inventory item story |
| Ghép / trial / restored | `relic.assembled.<relicId>`, `relic.trial.<relicId>`, `relic.restored.<relicId>` |
| H6 photo | `photo.v1.hoi_an.h6` + `beacon`/`illuminated` metadata |
| Reflection proof | **completed mai-lantern**, không pickup/HQ01 giả |
| Paper proof | Solved puzzle_market OR hoi_an.market_puzzle_solved; old flag-only recovery có fixture test |
| H10 proof | Accepted/completed HQ01 hoặc quest:HQ01 flag; không buộc/hoàn thành HQ02 |
| Hoàn tất Hội An |4 claimed + assembled + beam đúng + HQ01 completed + hoi_an.lamp_restored tại H2 |

Source proof/ownership/ledger ghi cùng atomic Persist hiện có; nếu Write trả false thì rollback các list state mới trong RAM. Disk-full/crash injection chưa test, không tuyên bố fault-tolerance PASS mới. Old quest/puzzle one-shot được dùng làm proof, nhưng không auto-grant: phải trở lại source và thực hiện gesture mới. Không tiêu lại lamp_eye/hanji_core/link_key/đồ ăn hoặc thay reward Bà Tư.

H6 giữ nguyên completionFlag và các ledger key `checkin:hoi_an.h6`. New photo trên old mark không phát thưởng lại; unmarked save chỉ nhận2 xu + postcard cũ sau alignment hợp lệ. Photo là illustrated crop từ Map A, giữ aspect, không OS camera/capture PNG. Album lưu metadata và dựng view; xem lại không grant. Greeting Stamp Jeonju vẫn **Phase4 pending**; photo proof đã có để lần sau dùng, không cấp sớm cả hệ Jeonju.

## Art / trạng thái

4 NPC Single kỹ thuật đã render đúng, chưa canonical approval.6 sheet Sprite Multiple →6 full +21 matched components, shared textures và native stable GUID/localID;3 Seoul wrong-semantic icons không bound. Các17 component ngoài Hội An chỉ technical import; chưa nghiệm thu riêng trong trial khác. Sprite/crop source art và full-city glow vẫn chờ visual acceptance. Texture settings/paths/refs tại manifest, không sinh PNG dẫn xuất trùng.

## Bảo toàn và phạm vi dừng

GameplayContent cities/events/items/enemies/quests/puzzles/resonances giữ nguyên cấu trúc giá trị. Chỉ H2 check-in flags chuyển sang lantern-lit và backend yêu cầu completed mai-lantern; H2 NPC/quest options ưu tiên camera. Actor scale0.72/touch52, canonical21 previous NPC bindings,133 nodes/routes,29 quests,22 puzzles, boss/ending/save classes không đổi.

D07 đã ghi vào quyết định nhưng **chưa code Phase5**: legacy S21/credits còn như baseline. Không tuyên bố đã đáp ứng global ending mới.5 city relics, final ceremony, Release Readiness/Android đều chưa triển khai trong lượt này.
