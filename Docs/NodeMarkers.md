# Node marker theo tiến độ

Marker là presentation, độc lập với trạng thái gameplay/discovery của node.
`NodeMarkerResolver` chỉ đọc quest, item, event và world flags.
`NodeMarkerView` vẽ/animate, chuyển click tới `CityMap.TryMove` hiện có.
Không có grant reward, accept quest hoặc unlock gate trong hai lớp này.

## Loại marker và hiệu ứng

`NodeMarkerType`: `Normal`, `Camera`, `BossAlert`, `NpcTalk`, `NpcNeedItem`,
`Puzzle`, `Reward`, `Completed`, thêm `Quest` và `Portal` để giữ đúng ngữ nghĩa
trạm nhiệm vụ và cổng thành phố hiện có.

`NodeMarkerEffect`: `None`, `Blink`, `Pulse`, `Glow`, `Bounce`.
Badge có ý nghĩa (trừ `Normal`/`Completed`) trôi hình sin ±4 UI units, chu kỳ 1,9 giây,
lệch pha theo node. Blink nhẹ (alpha 0,94–1), pulse tối đa +2,5%.
Hiệu ứng dừng khi đang mở modal. Nút sóng **Nhịp → Nhẹ → Tắt** ở HUD cho phép
giảm biên độ xuống ±1,4 hoặc tắt cả float/pulse/blink/glow. Lựa chọn lưu bằng
PlayerPrefs `LBC.MarkerMotion`, độc lập save gameplay; runner dùng key QA riêng.
Marker thường chỉ dùng vòng node nhỏ.

Toàn bộ hình marker nằm trong `CityMap/NodeMarkerLayer`, cùng hệ tọa độ map,
độc lập với Button input và sprite actor. Icon mới dùng bộ vector UI hiện có;
Reward tái sử dụng PNG ba lô. Không thêm PNG, font emoji hoặc nhãn text trên map.
Badge mặc định 24 UI units, cấu hình được từ 18 đến 30, giữ kích thước gọn khi zoom.

Discovery do `MapDiscoveryManager` quyết định, tách khỏi `MapNodeState`
(trạng thái khóa/đi qua của gameplay):

| `MapDiscoveryState` | Render | Input / đường đi / event |
|---|---|---|
| `REVEALED` | Vòng nền, marker theo tiến độ, actor nếu có | Theo điều kiện gameplay hiện có |
| `PREVIEW` | Chỉ vòng nền trung tính alpha 0,22; không tên, loại, actor hoặc badge | Không |
| `HIDDEN` | Không hình, không đường | Không |

`IsVisible` hiện có vẫn là predicate **REVEALED** dùng cho input/path/event.
Chỉ renderer marker dùng thêm preview; không mở rộng quyền gameplay của node.
Marker fade 0,55 giây khi khu mới được phát hiện, kể cả chuyển preview → revealed.
Các đường mới xuất hiện lệch nhịp ngắn theo tuyến/waypoint; không đổi hình học tuyến.
Các marker đã mở hiện đầy đủ khi load scene.

Trong `GameplayContent.json`, node có `previewStages: [0]` chỉ hiện vòng mờ
ở stage 0. Đây là opt-in minh thị; bỏ trường/mảng rỗng giữ hidden. Điều kiện
`requiredFlags` vẫn chặn revealed/input/event nhưng không chặn vòng preview
vô danh đã opt-in. Node bí mật H9 không khai báo preview.

| Stage Hội An | Node revealed thêm | Preview vô danh |
|---|---|---|
| 0 | H0, H1, H2, H11 | H3, H4 |
| 1 – nhận HQ01 | H3, H4, H10, H12, H13, H21 | H16, H18 |
| 2 – lấy lõi Hanji | H5, H14, H16, H18, H19 | H6, H17 |
| 3 – thắng/hack H5 | H6, H7, H15, H17 | H20 |
| 4 – khôi phục H7 | H8, H20 | Không |

H9 chỉ mở bằng flag nhiệm vụ phụ, độc lập các stage; H5/H7 không bị lộ qua preview.
API chung hỗ trợ sáu thành phố; chỉ Hội An có frontier preview được cấu hình/
nghiệm thu runtime ở checkpoint này. Không suy ra năm chương khác đã chơi được.

## Cấu hình

File: `Assets/_Game/Resources/Content/AdventurePresentation.json`, trường `nodeMarkers`.
Node không có record riêng vẫn nhận default phù hợp từ loại node và event.
Không phải khai báo danh sách marker cố định bằng code.

```json
{
  "cityId": "hoi_an",
  "nodeId": "H10",
  "size": 24,
  "rules": [
    {"questId":"HQ02","questState":"completed","marker":"Completed"},
    {"optionId":"complete","optionState":"enabled","marker":"NpcTalk","effect":"Bounce"},
    {"questId":"HQ02","questState":"notReady","marker":"NpcNeedItem","effect":"Glow"},
    {"optionId":"accept","optionState":"enabled","marker":"NpcTalk","effect":"Blink"}
  ]
}
```

Các rule xét từ trên xuống; rule đầu đủ điều kiện được áp dụng. Các điều kiện
trong cùng rule nối bằng **AND**. Không có rule khớp thì dùng default của node,
hoặc `marker`/`effect` ở cấp record nếu đã cấu hình.

| Trường điều kiện | Ý nghĩa |
|---|---|
| `questId` | Quest cần đọc; nếu bỏ trống dùng quest của event |
| `questState` | `offer`, `active`, `completed`, `ready`, `notReady` |
| `eventState` | `resolved`, `unresolved`, `usable`, `unusable` |
| `optionId` + `optionState` | Đọc lựa chọn runtime, ví dụ `complete`/`tutorial_return`; `enabled` hoặc `disabled` |
| `requiredFlags` | Phải có đủ world flags |
| `forbiddenFlags` | Không được có các world flags này |
| `requiredItems` | Mảng `{ "itemId": "...", "amount": 1 }` |
| `hide` | Ẩn toàn bộ marker khi rule khớp; không thay discovery/pathfinding |

Nếu muốn bỏ dấu chat khi NPC tạm không có vai trò nhưng giữ vòng node,
dùng `marker: "Normal"`. `hide: true` dành cho trường hợp muốn giấu cả marker;
nó không thay quyền di chuyển hay sprite actor của node.

Badge NPC (`...`)/camera có nền trắng ngà, viền và bóng nhẹ. Boss là dấu `!` đỏ
trên badge trắng; key gate có vòng aura nhỏ. NPC đủ đồ để giao có điểm nhấn vàng.
Vị trí mặc định nằm trên đầu actor; sprite actor và vòng nền giữ nguyên vị trí.
`MarkerHitArea` 52×52 cố định, riêng với `SemanticMarker` đang trôi. Glyph/frame
không nhận raycast. Cả marker và actor vẫn chuyển input về `CityMap.TryMove`.
Có thể chỉ định offset thủ công:

```json
"offsetMode": "custom", "offset": { "x": 16, "y": 49 }
```

Offset là UI units so với tâm node. Khi Lumi đang đứng tại node, badge được nâng
lên tối thiểu 64 units để tránh che Lumi. Sửa JSON rồi nạp lại City Map/build.

## Các chuyển đổi đã nối vào Hội An

| Node | Tiến độ → marker |
|---|---|
| H2 | Chưa nhận: `NpcTalk/Pulse`; nhận nhưng thiếu Mắt Đèn: `NpcNeedItem/Glow`; có đồ để trả: `NpcTalk/Bounce`; trả tutorial: `Normal`; xong HQ01: `Camera` |
| H3 | HQ01 đang cần Mắt Đèn: `Reward/Glow`; đã nhặt: `Camera` |
| H4 | Puzzle chưa giải: `Puzzle`; giải xong: `Camera` |
| H5 | Bot chưa xử lý: `BossAlert/Pulse` đỏ; thắng hoặc mã H19: `Completed` |
| H10 | Chờ nhận quest: `NpcTalk/Blink`; thiếu dây: `NpcNeedItem/Glow`; đủ dây và flag: `NpcTalk/Bounce`; giao xong: `Completed` |
| H12 | Đang làm HQ02: `Reward/Glow` cạnh cua; xử lý xong: `Completed` |
| H13 | Puzzle → `Completed` |
| H6/H7 | Khi HQ01 hoàn thành: `Camera` |
| H11/H21 | Rương chưa mở: `Reward`; nhận xong: `Completed` |
| H18 | Chưa nhận HQ03: chat; đã nhận và chưa có việc mới tại NPC: `Normal` |

Camera tại H2/H3/H4/H6/H7/H17 có record `checkIns` trong `GameplayContent.json`:
ID, node/city, title/description, requiredFlags, completionFlag và rewardCoins.
`NodeEventManager` kiểm tra node hiện tại, revealed và điều kiện trước khi ghi dấu;
dùng `WorldState`/`Inventory.GrantOnce`/save hiện có. Panel xem chi tiết từ ảnh map
gốc theo UV giữ tỷ lệ, ghi dấu rồi chuyển camera → `Completed`, thưởng đúng một lần.
Có thể mở xem lại; không có nút nhận thưởng lần hai. Mức 2 xu/check-in là đề xuất
cân bằng mới trong dữ liệu, không phải thông số được lấy từ bộ gameplay gốc.
Chưa có album tổng hợp hoặc chức năng xuất ảnh chụp của người chơi.

## Thêm node trên map cũ

1. Thêm node mới vào `cities[].nodes` của `GameplayContent.json`: ID, tên,
   position chuẩn hóa, kind, eventId, revealStage, previewStages và requiredFlags/gate nếu cần.
2. Thêm event tương ứng vào `events`, edge theo ID và waypoint phù hợp ảnh nền.
   Node chỉ điều hướng dùng event `junction`; node có gameplay cần event thực tế.
3. Nếu là địa danh đẹp, `kind: "checkin"` tự nhận camera. Nếu cần đổi marker
   theo tiến độ, thêm record tùy chọn trong `AdventurePresentation.json`.
4. Nếu có nhân vật đứng tại node, dùng cấu hình `mapActors` hiện có và art phù hợp.

Loader Hội An giữ các node/tọa độ đã duyệt trong `HoiAnData.json`, rồi tự nối thêm
ID mới từ catalog. Không cần tăng một hằng số 22 hoặc sửa vòng render.
Map background được giữ nguyên. H0–H21 và numeric save indices cũ không đổi.
Với node mở rộng đã phát hành, giữ thứ tự và thêm node mới ở cuối catalog để
bảo toàn numeric save indices; đổi thứ tự save hiện hữu cần migration riêng.

Test runtime tạm đã thêm H22 chỉ bằng catalog, kiểm tra ẩn theo revealStage,
hiện camera khi mở, bấm camera đi tới H22, rồi gỡ fixture và phục hồi đúng save.
**H22 không phải nội dung được thêm vĩnh viễn trong bản bàn giao.**

Marker được suy ra từ gameplay đã save, không lưu một bản trạng thái UI riêng.
Save/restart vì thế giữ đúng marker theo quest/item/flags; không nâng schema save.
