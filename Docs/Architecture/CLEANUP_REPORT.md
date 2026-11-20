# Dọn tài nguyên và áp dụng cấu trúc thư mục đích

> Đây là số liệu của đợt đầu. Đợt bổ sung 13/11 đã chuyển thêm 18 ảnh lịch sử và một tài liệu ra ngoài project; Assets hiện có 392 asset/140 ảnh. Xem [UNUSED_ASSET_ELIMINATION.md](UNUSED_ASSET_ELIMINATION.md) để biết trạng thái chưa nghiệm thu đầy đủ, ngoại lệ Resources và điều kiện commit/push còn thiếu. Kho References/ArchivedArt được nhắc ở đợt đầu hiện cũng đã chuyển ra kho ngoài project theo manifest mới; không bị xóa vĩnh viễn.

Lượt làm việc bắt đầu ngày 12/11/2026 và kết thúc ngày 13/11/2026 trên nhánh `refactor/codebase-map-20261112`, HEAD vẫn là `9ec07c5e581444d34447598f4c0c6538e40ad9f4`. Chưa commit, push hoặc merge. Đây là phần bổ sung sau đợt refactor nhỏ trong [REFACTOR_REPORT.md](REFACTOR_REPORT.md).

## Kết quả thực tế

| Hạng mục | Trước | Sau |
| --- | ---: | ---: |
| File asset trong `Assets`, không tính `.meta` | 425 | 411 |
| Dung lượng các file asset đó | 251.796.935 byte | 229.194.930 byte |
| Ảnh trong `Assets` | 168 | 158 |
| Nhóm ảnh cùng SHA-256 trong `Assets` | 4 | 2, giữ có lý do |
| Thư mục trực tiếp dưới `Assets` | `_Game`, `_Recovery`, `Welcome`, `Settings`, `Scenes`, `Resources` | `_Game` |
| File asset/template và `.meta` đã xóa | — | 17 file, 9.013.149 byte |
| File sinh lại được khác đã xóa | — | 3 file, 137.796 byte |
| File output Gradle đã xóa | — | 3.132 file, 5.396.413.193 byte |

Tổng file chủ động xóa trong ba manifest là **3.152 file / 5.405.564.138 byte**, khoảng **5,03 GiB**. Con số này là dung lượng file đã xóa, không cộng phần chuyển sang lưu trữ. Các file audit và preimage nhỏ chiếm thêm dung lượng. Snapshot toàn workspace, loại trừ `.git`, ghi nhận từ 20.521.367.411 xuống 15.084.863.831 byte; mức chênh còn bao gồm housekeeping của Unity. Không dùng con số snapshot để giả định toàn bộ chênh lệch đều do lệnh xóa thủ công.

## Cây thư mục đã thực hiện

```text
Assets/_Game/
├── Art/
│   ├── Characters/       # Lumi và nguồn/biến thể phục vụ import
│   ├── NPC/              # Theo thành phố, có atlas dùng chung
│   ├── Enemies/          # Theo thành phố và atlas dùng chung
│   ├── Maps/
│   ├── UI/               # Panels, Markers, Postcards, AppIcon
│   ├── Relics/           # Artifacts và Fragments
│   ├── Environment/
│   ├── Leisure/
│   ├── Items/            # Vật phẩm không phải di vật
│   └── SourceReferences/ # Nguồn/đối chiếu import chưa được chứng minh là thừa
├── Resources/
│   ├── Art/
│   │   ├── Characters/Cats/
│   │   ├── Maps/DaLat/
│   │   └── Leisure/
│   ├── Content/
│   └── các catalog ScriptableObject và HoiAnData.json
├── Audio/Music/
├── Audio/SFX/
├── Scripts/
│   ├── Core/
│   ├── Gameplay/         # Content, Quests, HoiAn, Entry, Inventory, JourneyShop
│   ├── Maps/
│   ├── UI/
│   ├── Persistence/
│   ├── Editor/
│   └── Validation/
├── Scenes/               # Kể cả Template và Recovery, giữ nguyên GUID
├── Prefabs/
└── Rendering/            # Input actions và cấu hình/template render hiện có
```

Danh sách **từng file** thực tế nằm trong [FINAL_TREE.md](FINAL_TREE.md). Các nhóm ảnh mà mẫu yêu cầu đều có trong `Art`. `Resources/Art/UI` và `Resources/Art/Relics` không được tạo trống: ảnh UI/di vật hiện được giữ bằng serialized references trong catalog, không cần thêm bản sao để tải động. `Resources/Art/Leisure` là nhóm có chức năng thực tế cho các prop được tải bằng key. `Items`, `SourceReferences`, `Rendering`, `Scenes/Recovery` là các nhóm bổ sung có vai trò cụ thể.

Các bộ bàn giao/nguồn import được gom vào `References/ImportSources`, gồm LBC_AnhMoi, DesignKit, dữ liệu 6 thành phố, mở rộng Hội An và nguồn check-in. Bảy ảnh nháp Đà Lạt cùng `.meta` được chuyển vào `References/ArchivedArt/DaLatRejectedDrafts`. Đây là **di chuyển**, không phải xóa hay tạo bản backup ảnh thứ hai. Nội dung nguồn và metadata được đối chiếu hash.

Các thư mục backup/checkpoint, bản vá đã có, `Submission`, `ReviewPacks`, build bàn giao và bằng chứng QA được giữ. `Library`, `Logs`, `UserSettings`, `Builds`, `Validation` vẫn có vai trò phát sinh/lưu bằng chứng ngoài cây source; không giả vờ rằng Unity project sẽ không còn những thư mục đó.

## Những gì đã xóa và lý do

- `Map_Gyeongju_DestinationPreview_A.png` và `Map_Seoul_DestinationPreview_A.png`, cùng `.meta`: trùng SHA-256 với Overview tương ứng; native dependency và tìm GUID không thấy consumer bên ngoài. Asset Overview đang dùng được giữ nguyên importer/GUID/sprite ID. Các alias import cũ được đổi sang canonical Overview; không sửa sprite đang được game sử dụng.
- `Assets/Welcome` và metadata: đồ thị reference khép kín của trang giới thiệu Unity; không có dependency từ scene/build/resource game hay công cụ dự án. Xóa cả ảnh, ScriptableObject, script `Welcome2DScript` và asmdef thuộc nhóm đó.
- `Unity.2D.Welcome.csproj`: project IDE sinh từ assembly Welcome đã xóa. Solution đã bỏ reference; bản text trước xóa được giữ trong preimage nhỏ vì đây là file đã modified từ trước.
- Hai `.pyc` trong `Tools/__pycache__`: có `.py` gốc, sinh lại được.
- Chỉ hai thư mục `Library/Bee/Android/Prj/IL2CPP/Gradle/unityLibrary/build` và `launcher/build`: output Gradle sinh lại được. Trước khi xóa đã xác nhận Unity dừng, kiểm tra đường dẫn, lập danh sách/hash và thông báo chi phí rebuild. Giữ Gradle project, signing, source, package cache, các cache khác và APK/EXE bàn giao. Lần build Android sau sẽ phải tạo lại những output này.

Lần xóa Gradle đầu gặp giới hạn đường dẫn dài trên Windows; manifest PREPARED được giữ, phần còn lại được kiểm tra lại rồi xóa bằng đường dẫn mở rộng. Manifest cuối ghi trạng thái REMOVED. Không đổi sang lệnh xóa rộng hơn hoặc bỏ qua kiểm tra đường dẫn.

## Những nhóm trùng hoặc chưa rõ vẫn giữ

- Hai ảnh Hội An cùng pixel nhưng **đều đang dùng**, có mipmap/filter khác nhau giữa preview và bản đồ. Gộp sẽ thay đổi cách render; giữ cả hai và ghi rõ vai trò.
- Lumi DialoguePortrait và Character_Cutout_Concept cùng pixel nhưng còn vai trò nguồn/Editor. Không coi ảnh có pixel trùng là đồng nghĩa với importer/slicing/nguồn gốc có thể thay thế.
- Ảnh nguồn, biến thể NPC đã duyệt, atlas có slice đang dùng và nguồn import chưa chứng minh thừa vẫn được giữ. `SourceReferences` có vai trò provenance/import, không phải ảnh đang dùng bị bỏ quên ngoài các nhóm chức năng.
- `SourceCities` JSON còn được pipeline import dùng làm nguồn đối chiếu; giữ trong Content. Các file Hotfix/Phase/Prototype runtime có call site, partial class, scene hoặc đường gọi QA. Không hợp nhất hotfix UI khi chưa có regression UI tương đương.
- Không tìm thấy SpriteAtlas asset trong phạm vi asset hiện hành. Native dependency vẫn kiểm tra scene, prefab, ScriptableObject, importer, Resources và cấu hình project.

## Code, key và quy trình import/build đã cập nhật

Giữ nguyên namespace, tên MonoBehaviour, scene component, GUID và sprite localID. Di chuyển `SaveSystem` sang Persistence, các manager Content/quest/inventory/shop sang Gameplay, CityNavigation sang Maps và HoiAn/entry sang Gameplay. Không đổi thuật toán nhiệm vụ, combat, cứu mèo, di vật hoặc save.

| Key ảnh cũ | Key đang dùng |
| --- | --- |
| `LeisureProps/...` | `Art/Leisure/...` |
| `CatRescue/RescueCats` | `Art/Characters/Cats/RescueCats` |
| `CatRescue/DaLatStation` | `Art/Maps/DaLat/DaLatStation` |

Đã cập nhật C#, các JSON spritePath/resource tương ứng, catalog source paths, đường scene trong Build Settings, các import plan hiện hành và công cụ build/import. `CatRescueBuild.Prepare` xử lý hai vị trí ảnh mới; crop H3 đi qua resolver GUID để đọc được atlas đã chuyển. Fixture QA ngoại hình dùng `_Game/Resources` mà vẫn giữ key `AppearanceReviewData`.

`Docs/Art/asset-moves.json` lưu alias đường dẫn theo GUID. Các report nghiệm thu lịch sử được giữ nguyên; các import plan thực sự đang được Editor đọc mới được cập nhật, với preimage trước sửa. Catalog art hiện hành được xuất lại bằng Unity rồi chạy `art_asset_catalog.py --inventory-only`. Tool inventory phân biệt alias có hash lịch sử khác sau các lần sửa art trước đây; không biến việc liệt kê hiện trạng thành tuyên bố pixel chưa từng thay đổi. Một binding NPC curator không khớp registry phê duyệt hiện có được ghi là mismatch metadata, không tự duyệt mặt mới hoặc sửa trạng thái registry.

## Kiểm chứng và giới hạn

| Kiểm tra | Kết quả có bằng chứng |
| --- | --- |
| Unity compile trước/sau các nhóm | Đã chạy Unity 6000.6.5f1 ngoài sandbox bằng license máy; các lượt audit thành công thoát 0. Log lỗi compile của lượt đầu viết utility audit được giữ, đã sửa API EntityId rồi chạy lại thành công. |
| Metadata và dữ liệu gốc | 417 `.meta` của asset được giữ có hash không đổi; thêm 201 file metadata được kiểm tra qua manifest di chuyển. 188 file ảnh/audio/scene/prefab/input được giữ nguyên byte; 165 ảnh bao gồm ảnh đã lưu trữ ngoài Assets. |
| Importer Windows/Android và slicing | Đối chiếu 153 texture asset còn lại theo GUID: sprite localID, rect, pivot, border và cấu hình importer Standalone/Android không đổi. |
| Dependency/reference | Không có missing script: 0 trước, 0 sau. 7 GUID lỗi và 6 object reference lỗi trong Renderer2D template đã có từ trước, tập lỗi sau tương đương qua mapping đường dẫn, không có lỗi mới. Không báo toàn project sạch missing reference. |
| Resources | Kiểm tra tải key literal trong source và key động từ JSON thực tế trong Unity. Không có resource thiếu mới. `AppearanceReviewData` là fixture Development QA do công cụ tạo tạm; thiếu cả trước và sau khi chưa chạy workflow review. |
| JSON gameplay/save | Gameplay JSON khớp trước sau khi chuẩn hóa các thay đổi key/đường nguồn đã liệt kê. Source SaveSystem không đổi byte sau khi di chuyển. 4 kiểm tra load/round-trip fixture save v1/v2 đạt trong Unity, không dùng save người chơi. |
| Route regression trong Unity | PASS: 6 thành phố, 2.949 cặp node, 5.901 assertions. |
| Cứu mèo và save/restart | 518 + 2 kiểm tra đạt với source production trong adapter host .NET; log ghi rõ Unity Player NOT TESTED. Output mới riêng, không ghi đè bằng chứng cũ. |
| Tool syntax | 125 script Python parse thành công; không tự chạy các tool checkpoint/setup lịch sử để giả định chúng đã được chấp nhận lại. |
| Player/Android | Chưa chạy lại toàn bộ hành trình UI/ending hoặc cài Android trên thiết bị. Lượt Play Mode trước đó còn lỗi `SoftAudio.Music.cs:61`; không dùng compile/native audit để tự nhận game end-to-end PASS. |
| Build bàn giao | Không ghi đè APK/EXE, không tuyên bố đã build/cài thử cả hai nền tảng. Build entry points được cập nhật và compile Editor, importer hai nền tảng đã được so sánh; build Android tiếp theo cần tái tạo output Gradle. |

## Bằng chứng, phục hồi và chạy lại

Tất cả dưới `Docs/Storage/AssetCleanup-20261112/`:

- `before/inventory.json`, `after/inventory.json`: từng asset, SHA-256, GUID, consumer, ảnh trùng và dung lượng workspace.
- `migration-result.json`, `layout-finalization.json`, `recovery-scene-move.json`: từng lượt di chuyển, hash, file text được sửa và nguồn được lưu trữ.
- `generated-deletions.json`, `gradle-deletions.json`, phần deletions của `migration-result.json`: từng file xóa, hash, dung lượng và lý do.
- `native-before`, `native-after`, `native-final`, `native-acceptance`, `native-post-cache`, `native-layout-final`: audit Unity theo từng giai đoạn, có fixture save riêng.
- `route-layout-final.json`, `verification.json`: kết quả đối chiếu cuối.
- `recovery-text`, `recovery-finalization`: preimage text nhỏ; không phải full snapshot hay nguồn thay thế cho backup/checkpoint. Không tạo thêm bản sao ảnh/Library. File được di chuyển có thể chuyển ngược theo manifest; source đã track có thêm HEAD làm mốc, nhưng không dùng reset/clean.

Log Unity cuối: `Validation/AssetCleanup-20261112-layout-final.log`. Log domain: `Validation/AssetCleanup-20261112-domain/run.log` và `restart.log`.

Các công cụ của lượt này được gom vào `Tools/Maintenance`. Chạy lại kiểm tra không xóa bằng:

```powershell
& 'C:\Users\admin\AppData\Local\Programs\Python\Python312\python.exe' Tools/Maintenance/verify_project_layout.py
```

Các script migration/cleanup là công cụ một lượt có guard; không chạy lại chỉ để làm mới báo cáo. Bước tiếp theo cần xử lý harness Play Mode/audio và kiểm thử UI/ending thực tế trong một nhiệm vụ riêng, rồi mới tiếp tục loại bỏ những phần chưa có bằng chứng an toàn.
