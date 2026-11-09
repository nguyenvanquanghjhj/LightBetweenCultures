# HƯỚNG DẪN DÀNH CHO CODEX — NHẬP BỘ DESIGN KIT VÀO UNITY

## 0. Kiểm tra trước khi sửa

Project ZIP người dùng đã đưa trong cuộc trò chuyện có Unity **6000.6.5f1**, URP2D, New Input System, cùng các script:

- `Assets/_Game/Scripts/Prototype/PrototypeGame.cs`
- `Assets/_Game/Scripts/Editor/PrototypeSetup.cs`
- `Assets/_Game/Scripts/Editor/HoiAnDemoSetup.cs`
- `Assets/_Game/Scripts/HoiAn/HoiAnGameplay.cs`
- `Assets/_Game/Scripts/HoiAn/HoiAnPlayerController.cs`
- `Assets/_Game/Scripts/HoiAn/HoiAnNpc.cs`
- `Assets/_Game/Scripts/HoiAn/HoiAnItem.cs`

Đó là **snapshot cũ**, phải inspect workspace hiện tại trước. `HoiAnDemoSetup` đã tạo cảnh bằng hình chữ nhật. Không xóa file người dùng, không overwrite scene mà chưa hỏi hoặc sao lưu.

## 1. Đưa design data vào đâu?

Ưu tiên tạo thư mục *không xung đột*:

```
Assets/_Game/Data/DesignKit/
    hoi_an_map.json
    hoi_an_exploration_grid.json
    hoi_an_quests.json
    world_cities.json
Docs/Art/
    # docs + ảnh tham khảo; không tự nhập asset reference thành sprite cuối
```

Không cần copy manifest/docs vào Assets nếu không dùng Runtime.

## 2. Chuyển schema đúng kiểu Unity

Đọc JSON v1 để sinh `ScriptableObject` cho `CityDefinition` / `NodeDefinition` hoặc dùng `Serializable` classes có **arrays/lists**. Không giả định Unity `JsonUtility` hỗ trợ dictionary động. ID ổn định là khóa chính, không dùng tên tiếng Việt làm khóa.

Lưu ý `nodes` chứa normalized `(x, y)` tính từ góc **trên trái**, trong khi Unity UI anchored transform có trục Y khác. Công thức đặt node vào vùng bản đồ:

- `anchoredX = (x - 0.5) * contentWidth`
- `anchoredY = (0.5 - y) * contentHeight`

Nếu dùng SpriteRenderers theo đơn vị thế giới, chuyển tọa độ tương ứng; không để node ra khỏi nền bản đồ khi scale portrait.

## 3. Prototype Unity nên có ba scene hoặc state

- `MainMenu / WorldMap` (có thể chung scene UI đơn giản).
- `HoiAnCityMap` — node + edges + icon player, dữ liệu ngoài; không cần đầy đủ art trước.
- `HoiAn_H3_Free` — Grid/Tilemap với collider, chạm di chuyển và nhặt mắt đèn.

Nối với các phần cũ nếu hợp lý, nhưng **không để hai prototype riêng không giao tiếp**. Một Save/WorldState chịu trách nhiệm trạng thái giữa scene.

## 4. Đường đi

- CityMap: BFS giữa các node qua cạnh đủ điều kiện, tính tuyến trước rồi animate player icon tuần tự; sự kiện kích hoạt ở **node đích**.
- FreeMap: A* hoặc BFS theo grid walkability; tránh rơi vào kiểu nội suy đường thẳng xuyên qua tường.
- Interaction phải kiểm tra khoảng cách; click một vật phẩm ở xa nghĩa là đặt mục tiêu **đi đến gần rồi mới tương tác**, không phải nhặt lập tức.
- Bảo vệ double click/transition re-entrancy, node bị khóa và scene exit sai.

## 5. Dựng asset dần theo thứ tự

1. Dùng placeholder để làm scene chơi được.
2. Thêm đúng gà `Lumi_CANONICAL_user_image` vào UI portrait/menu làm **reference trước**; không phóng nguyên ảnh minh họa làm sprite chạy.
3. Tạo hoặc nhận sprite thật theo `AssetManifest.csv`: 4 hướng/idle/walk; NPC Cô Mai; đường lát; nhà vàng; nước; đèn lồng; icon node.
4. City map nền art riêng không chữ / node; node H0–H9 được overlay bởi hệ thống Unity.
5. Từng asset cần Sprite importer, sorting, pivot và collider đúng; đừng import một bảng ảnh composite và tự động Slice liều.

## 6. Tiêu chí bàn giao từng bước

- [ ] Có scene Unity được lưu thật.
- [ ] Không còn error đỏ trong Console.
- [ ] Click mở world map, chọn Hội An.
- [ ] Lumi đi đúng graph; không nhảy qua cạnh khóa.
- [ ] H2 NPC nhận nhiệm vụ; H3 mở scene free.
- [ ] Player đi tránh vật cản và nhặt mắt đèn một lần.
- [ ] Thoát H3 quay đúng node; trả quest; đèn nhỏ bật; save giữ trạng thái.
- [ ] Test UI portrait 9:16; nút cảm ứng, cam bám player.
- [ ] Chỉ sau đó mới phê duyệt bổ sung quest H4/H5/H7 và art final.

Phân biệt **đã viết code**, **đã kiểm thử tự động**, **đã chạy trong Unity Editor**, **đã build APK** trong báo cáo. Bản Design Kit không chứng minh bước nào trong số đó.
