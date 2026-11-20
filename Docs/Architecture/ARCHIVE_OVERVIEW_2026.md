# Kiến trúc bản lưu trữ 2026

Mô tả source sau tổ chức workspace 13/11/2026; xem [CODEBASE_MAP](CODEBASE_MAP.md) để tra class/call flow chi tiết. Nhiệm vụ lưu trữ không bổ sung gameplay.

## Khởi động và scene

`Prototype` khởi động/menu; `WorldMap` chọn thành phố; `CityMap` dùng runtime chung với dữ liệu riêng của sáu thành phố; `HoiAnDemo` giữ mở đầu/H3. `Template/SampleScene` còn enabled trong Build Settings hiện hành. Không bỏ template theo tên nếu chưa kiểm tra consumer.

`GameBootstrap` tồn tại qua chuyển scene, sở hữu state, dữ liệu và service. `SceneTransition` lưu trước khi load scene. `WorldMap` kiểm tra điều kiện mở; `GameBootstrap.EnterCity` lưu thành phố cũ rồi tải tiến độ/dữ liệu thành phố mới.

## Manager, dữ liệu và luồng gameplay

| Thành phần | Vai trò |
|---|---|
| `GameData` và JSON `Resources/Content` | Node, graph, event, enemy, item, quest, presentation |
| `CityMap`, `MapDiscoveryManager`, `CityNavigationGraph` | Discovery, điều kiện đường, di chuyển và node canonical |
| `NodeEventManager` | Xây lựa chọn event và kiểm tra điều kiện thực thi |
| `QuestManager`, `Inventory`, Journey Shop | Nhiệm vụ, vật phẩm, receipt, giao dịch và reward ledger |
| `BattleManager`, `EncounterManager` | Combat, kết quả encounter và thưởng một lần |
| `RelicCollection`, `RelicActivity` | Proof/fragment, lắp ráp, phục hồi và ảnh sáu di vật |
| `CatRescueManager`, `CatRescueCatalog` | Sáu mèo/24 thử thách, supplies, transaction và rollback khi persist lỗi |
| `GameplayPanels` và partial | UI/hành động; manager vẫn xác thực nghiệp vụ |
| `SaveSystem`, `SaveData` | Save v2, đọc v1/v2, fallback `.bak`/`.tmp`, ghi qua file tạm |
| `GameArtCatalog` và catalog ScriptableObject | Native art reference theo GUID/localID và slicing |

Luồng: chọn node → kiểm tra discovery/route → di chuyển → `NodeEventManager.OnArrive/BuildOptions` → panel → manager kiểm tra hành động → cập nhật state/ledger → `GameBootstrap.Persist` → refresh. Ending S21 Seoul yêu cầu encounter, sáu di vật restored và ceremony; SQ03 báo tin có điều kiện riêng. Không đổi save schema hoặc cơ chế mở cổng để đóng gói.

Ảnh ở `Assets/_Game/Art`, catalog ở `Resources/Catalogs`, JSON ở `Resources/Content`. Khi di chuyển asset, `.meta`/GUID đi cùng; Resources key, sprite localID và serialized reference cần native audit.

## Công cụ, lịch sử và kiểm chứng

`Tools/Build/build.py` gọi entry point build không chạy scene generator. `Tools/Import` giữ input authoring; `Tools/QA` giữ harness/domain adapter. PASS .NET domain không phải PASS Unity Player. `Tools/Maintenance` có script một lượt và verifier: không chạy lại migration/cleanup lịch sử.

Partial Hotfix, tool Phase và Prototype có call site/scene component, được giữ nguyên tên/meta. `_LBC_ARCHIVE` chứa preimage/original art/backup/checkpoint; `_LBC_DELIVERABLES` chứa bàn giao; `_LBC_WORK` chứa log, fixture và bằng chứng chưa nghiệm thu. Incremental cần đúng base, không phải standalone. Không trộn art/snapshot cũ vào Assets khi restore.

Checksum/clone kiểm tra dữ liệu lấy lại được; import/C# compile kiểm tra mở source trong môi trường đã thử; native audit đối chiếu baseline; build/journey có phạm vi riêng. Android thật và owner acceptance vẫn cần riêng. Kết quả cuối ở `Docs/Development/Storage/FinalArchive-20261120`.
