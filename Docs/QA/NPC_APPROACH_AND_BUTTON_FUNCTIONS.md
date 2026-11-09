# NPC approach và công dụng nút — 20/10/2026

Trạng thái: **PARTIAL — G14 chưa có lối tiếp cận an toàn**. Không tuyên bố toàn bộ NPC đã FIXED. Không đổi node, arrival, graph, waypoint, NPC canonical, art, âm thanh, quest hoặc save schema.

## Sửa cũ thiếu gì

- `CityMap.PositionGroundedActor()`/`ActorWalkingApproach()` ở checkpoint trước kéo NPC về gần arrival và dời marker. Lumi vẫn dừng tại arrival; đó không phải Lumi tiếp cận NPC, còn làm sai standing offset đã duyệt. Lượt này bỏ phần clamp đó, khôi phục cách render chân NPC từ `mapPosition + offset` nguyên gốc.
- `GameplayPanels.DrawDialogue()` vẫn có `EventMore`, ghi `Lựa chọn 1/2`. Callback chỉ đổi trang hành động, không phải hai nhánh cốt truyện. Sửa nhãn trước đây chưa bỏ được thao tác thừa này.
- Nhận nhiệm vụ Cô Mai còn mở lời hứa của Lumi rồi Next về cùng menu. Lượt này bỏ đoạn lặp. Receipt vật phẩm thật khi nhận nhiệm vụ vẫn được giữ, đóng receipt là trở lại chơi ngay.
- Test trước kiểm cấu hình/anchor và save đã hoàn thành nhiều nội dung; chưa chứng minh đúng khoảng cách chân hiển thị và các trạng thái đang nhận/trả nhiệm vụ. Báo cáo lịch sử không thay thế nghiệm thu hiện tại.

## Thay đổi thực tế

| File | Thay đổi |
|---|---|
| `Maps/CityMap.cs` | Giữ NPC tại vị trí canonical; tương tác gọi tiếp cận trước khi mở thoại; khi rời NPC, Lumi đi ngược đoạn tiếp cận trước khi nối lại route cũ. |
| `Maps/CityMap.NpcApproach.cs` | Đo chân sprite thật sau aspect/mesh; dùng Dijkstra và polyline WALK đang mở; không dùng ferry cho đoạn tiếp cận. S11 có đoạn tiếp cận cục bộ trên sân nhà ga. Không có đường an toàn thì giữ Lumi và thông báo trên HUD nhìn thấy được. Không lưu thêm flag/vị trí. |
| `UI/GameplayPanels.cs` | Thay phân trang bằng danh sách hành động trực tiếp, cuộn khi cần. Giữ ID/guard/callback thật. Bỏ lời hứa/Next lặp sau accept; receipt cấp vật phẩm thật đóng thẳng về map. |
| `Validation/GameplaySmokeRunner.cs`, `.NpcStanding.cs` | Bổ sung vào runner hiện có: NPC sáu city, chân sprite, thời điểm mở thoại, canonical actor bất động, X/accept/delivery/cancel/receipt/reload. Save QA riêng. |

Không sửa `NodeEventManager` thêm trong lượt này; kế thừa việc ẩn talk/progress trùng nội dung và rest khi đầy. Không đổi skin/button art trong checkpoint sửa chức năng này. Yêu cầu nút tròn/bóng được giữ riêng.

## NPC — đo trong Windows Player ở zoom tối đa 4

| Node | Khoảng cách chân trước → sau, đơn vị Canvas | Kết quả |
|---|---:|---|
| H10 | 120,22 → 41,72 | Lumi đi theo đường bộ hiện có đến Bà Tư; NPC không dịch chuyển. |
| S2 | 86,43 → 41,72 | Lumi đi theo đường bộ hiện có tới Curator. |
| S11 | 84,25 → 33,11 | Đoạn ngắn trên sân trước nhà ga, không đổi arrival/graph. |
| H14 / G9 / S7 | 23,90 / 46,29 / 35,16 | Đã đủ gần; giữ standing offset. |
| G14 | 228,31 → 228,31 | **BLOCKED_SAFE_NO_APPROACH**. Arrival ở lối phía trên, NPC ở sân thấp hơn; graph WALK hiện hành không có đoạn đủ gần. Không tự nối xuyên mái/cây hoặc kéo NPC lên. Hội thoại NPC không bật từ xa; đây là hạn chế truy cập tương tác G14 cần xử lý riêng. |

Đối chiếu toàn bộ 26 actor, không chỉ bảy node ưu tiên: [bảng actor/event](../../Validation/NpcStandingButtons-20261020/npc-audit.csv). D18 là điểm Lumi nghỉ; S14 là bảng thông tin phà theo registry, không phải hai người bị thiếu actor.

## Công dụng nút đã đối chiếu callback

| Nút/nhóm | Callback và quyết định |
|---|---|
| `EventMore` / `1/2` | Chỉ gọi lại `ShowEvent(...page+1)`. **Bỏ phân trang**, giữ đủ hành động trong list. |
| Next sau accept | Chỉ về cùng menu NPC. **Bỏ đoạn lặp**; không tự nhận nhiệm vụ từ X. |
| `accept`, `accept:*` | `Quests.Accept()`/tutorial flow. **Giữ quyền nhận chủ động** và reward nhận nhiệm vụ canonical. |
| `complete`, `complete:*` | `Quests.Complete()` kiểm vị trí/items/reserves. **Giữ**, chỉ tiêu đồ khi chọn hành động giao. |
| `DeliveryConfirm` / `DeliveryCancel` | Giao thật / quay lại mà giữ đồ. **Giữ cả hai** khi quest có `confirmDelivery` hoặc exchange tiêu đồ yêu cầu xác nhận. Không thêm xác nhận giả cho mọi quest. |
| `DialogueCloseX` | Chỉ `Close()`, không execute/grant/accept. **Giữ**. |
| `ReceiptContinue` / `ReceiptClose` | Chỉ đóng receipt đã nhận; không grant lại. **Giữ** thông tin vật phẩm, không mở lại menu sau receipt nhận nhiệm vụ. |
| `talk`, `progress:*`, `rest` | Giữ lời thoại có proof/flag/thông tin riêng; kế thừa lọc nội dung lặp và rest khi đầy. |
| Inventory / nhật ký / guidance | Mở thông tin; nút inventory đổi sang nhiệm vụ liên quan chỉ khi có liên kết. Locate chỉ focus bản đồ, không đi/teleport. **Giữ**. |
| Puzzle / story / trial | Chọn/xoay/reset/gợi ý/submit gọi thao tác thật; `MicroHold` dùng pointer hold, không phải click no-op. **Giữ**. |
| Leisure | `LeisureAction` được bind vào Begin/replay và gesture, không dừng ở listener rỗng lúc dựng UI. Close dừng hoạt cảnh. **Giữ**. |
| Combat | Attack/defend/light/heal/repair/run/retry có kết quả riêng. **Giữ**, không thiết kế lại combat. |
| Relic / photo / ending | Claim/assemble/trial/record/ceremony có gate và kết quả riêng; X/back/review/skip có công dụng khác. **Giữ**, không đổi reward hoặc ending. |
| Close X + nút trở về | Lối thoát dễ thấy/accessibility; không xóa chỉ vì cùng đóng panel. |

[Danh sách callback tại source](../../Validation/NpcStandingButtons-20261020/button-callback-inventory.txt). Những nhóm không bị đổi logic chỉ được rà soát source trong lượt này, không nhận runtime PASS cho toàn bộ minigame/combat/leisure/ending.

## QA và giới hạn

- NPC: 25/26 đạt khoảng cách/thời điểm mở thoại; một trường hợp G14 BLOCKED. NPC canonical không bị dịch chuyển; inventory, completed quest và reward ledger không đổi do đi/tiếp cận. Graph/polyline JSON không đổi ở sáu city.
- Nhận nhiệm vụ, receipt, X, giao trực tiếp HA_FOOD_PREP, xác nhận/hủy HQ06, chống nhận lặp và reload: **PASS** trong [Windows Player](../../Validation/NpcStandingButtons-20261020/buttons-accepted/smoke-results.txt), không dùng compile làm bằng chứng hành vi. Không thay callback sau lượt này; build 06 chỉ sửa việc HUD định kỳ xóa thông báo đứng an toàn.
- Lượt NPC cuối [kết quả](../../Validation/NpcStandingButtons-20261020/npc-final06/restart-results.txt): 26 actor đã kiểm, G14 BLOCKED đúng chủ ý; thông báo an toàn giữ trên HUD qua refresh. `ALL CHECKS PASSED` trong log chỉ nghĩa là các assertion đạt, **không đổi kết luận tổng NPC PARTIAL**.
- Overflow scroll: NOT_TESTED nếu các hành động thật trong fixture vừa viewport; không tạo lựa chọn giả để đạt số lượng test.
- Windows có chạy input touch mô phỏng; không phải Android device. Cửa sổ yêu cầu 720×1280 bị desktop giới hạn còn 720×1080, ghi kích thước thật theo log. NPC chạy 540×960.
- Không build/cài APK hay thao tác điện thoại trong lượt này. APK OwnerPriority trước đó **chưa chứa** các sửa approach/menu hiện tại; Android nghiệm thu nhóm sửa mới: **NOT_TESTED**.
- Các lượt trung gian thất bại được giữ: giả định nhầm food prep có confirm, giả định menu phải có >3 hành động, rồi hot-swap fixture khác city khi map cũ còn render. Sửa adapter/fixture theo callback thật; không tính các lượt đó là PASS. Test cũng bắt được HUD refresh xóa notice sau 0,2 giây; sửa giữ notice đến khi di chuyển/tương tác tiếp, lượt cuối đã xác minh.

Ảnh trước/sau là frame Player trên cùng map canonical, cùng focus/zoom. Đây là bằng chứng Lumi đổi vị trí, không phải mockup: [gallery](../../Validation/NpcStandingButtons-20261020/gallery.html).

Checkpoint: `D:\GAMEKKK\Backups\CheckpointNpcApproachButtons-20261020`, cần cả `CheckpointTouchNpcDialogue-20261020` và full base `LBC_SourceVerified-20261020`. Manifest ghi hash và danh sách file, không chứa build/Library/PNG QA. Dừng ở checkpoint này, không mở Phase mới.
