# Loại bỏ asset không sử dụng — 13/11/2026

## Trạng thái nghiệm thu và Git

**Chưa nghiệm thu toàn bộ yêu cầu mở rộng. Chưa commit/push.** Nhánh hiện tại là `refactor/codebase-map-20261112`, HEAD vẫn `9ec07c5e581444d34447598f4c0c6538e40ad9f4`.

Đợt này thực sự loại nguồn lịch sử khỏi Assets và sửa công cụ có thể nhập lại Lumi cũ. Không suy ra asset được sử dụng trong gameplay chỉ từ dependency closure của Resources/catalog. Việc xác minh đầy đủ mọi nhánh gameplay và quy trình Android còn thiếu; điều kiện tự commit/push do chủ dự án đặt ra chưa được đáp ứng.

## Số liệu của đợt bổ sung

| Hạng mục | Trước | Sau |
| --- | ---: | ---: |
| Asset trong Assets, không tính .meta | 411 | 392 |
| Ảnh trong Assets | 158 | 140 |
| Ảnh trong Art | 144 | 126 |
| Ảnh trong Resources/Art | 14 | 14 |
| Nguồn lịch sử chuyển ra ngoài project | 0 trong đợt này | 18 ảnh + 1 tài liệu, 23.963.314 byte |
| File di chuyển kèm metadata | — | 42 |
| Alias đường dẫn cũ được gỡ | — | 25 |

Các số trên không cộng ảnh trong gói nguồn hoặc bằng chứng QA vào số ảnh gameplay. Inventory ghi 5.226 file thuộc các loại hình ảnh/audio/font/asset/scene/script/config/tài liệu trong project trước di chuyển, bao gồm hồ sơ QA và các gói nguồn; loại trừ .git, Library, Temp, obj, Logs, UserSettings, .vs và __pycache__. Inventory này không phải bằng chứng mọi file đều đã được chứng minh reachable/unreachable. File nhị phân build được giữ nguyên, không xem là game asset cần import.

### A. Dung lượng game assets

Đợt này chuyển **23.963.314 byte nguồn** khỏi Assets vào kho ngoài project; đây là giảm dữ liệu được Unity import, **không phải dung lượng xóa khỏi ổ đĩa**. Không xóa vĩnh viễn original art. Ở đợt trước, 17 file asset/template/metadata đã xóa là 9.013.149 byte; chi tiết và hash ở [báo cáo trước](CLEANUP_REPORT.md).

### B. Cache/build output

Đợt này không xóa thêm cache. Con số 5.396.413.193 byte Gradle đã xóa thuộc đợt trước, không dùng để chứng minh đã dọn ảnh. Một Player QA mới được build tại `Validation/UnusedAssets-20261113/Player`, không ghi đè bản nộp và không đưa vào commit.

## Tài nguyên đã chuyển và bằng chứng

Kho nguồn cục bộ: `D:/GAMEKKK/LightBetweenCultures-ArtArchive-20261113/`, ngoài Unity project, không phải runtime/build dependency và không đưa backup/source archive này lên Git theo yêu cầu. Từng đường dẫn, SHA-256, dung lượng nằm trong [archive-result.json](../Development/Storage/UnusedAssets-20261113/archive-result.json). Original bytes và `.meta` được đối chiếu sau di chuyển.

- Lumi: DialoguePortrait nâu, ExplorationIdle cũ, hai CityMarker tròn, Character_Cutout_Concept. Cả năm role trong DesignKitArt vẫn trỏ sprite Lumi_MapTraveler GUID `9a7d3aad847598b419b6b01ab212a053`, localID `-1754387159`.
- NPC: `NPC/Hue/npc_nguyet.png`, `NPC/Jeonju/npc_elder.png`, đã bị thay bởi các bản OwnerRevision hiện tại. Không đổi phê duyệt danh tính hoặc binding runtime; hai record variant lịch sử được đánh dấu archived, có hash, không còn buộc catalog tool phải có nguồn trong Assets.
- Enemy: `Enemies/Hue/Hue_Gate_Guard_Transparent.png`, không có serialized consumer hay đường tải runtime trong source đã kiểm tra.
- SourceReferences: Characters_MissingSprites, Checkin_Postcards, bốn H3 reference/concept sheets, HoiAn_H3_Environment, hai Composite sheets, UI_Adventure_Kit và README nguồn. Ba atlas H3 đang dùng vẫn ở Environment/UI/NPC, không chuyển chúng theo concept.
- Kho `References/ArchivedArt` từ đợt trước (21 file gồm bảy ảnh nháp và metadata) cũng được chuyển ra kho ngoài project: [manifest riêng](../Development/Storage/UnusedAssets-20261113/prior-archive-relocation.json). Không tính đây là ảnh mới loại khỏi Assets trong đợt này.

Với nhóm chuyển: audit Unity trước cho thấy không nằm trong dependency closure; quét GUID không có consumer Assets/ProjectSettings, không có đề cập tải trong runtime C#. Các ảnh ở ngoài Resources. Các tool/chuỗi provenance được kiểm tra riêng, không coi chúng là gameplay consumers. Sau chuyển, closure của mọi asset còn lại và serialized dependencies giữ nguyên.

## Code, catalog và công cụ đã xử lý

- `LumiRoleSetup`: bỏ ba constant và hàm ImportSprite private không có caller; giữ ConfigureArt và ApplyPlayerVisual gắn canonical Lumi.
- `DesignKitSetup`: chỉ nhập thư mục Markers từ gói gốc; không copy/import Lumi lịch sử trở lại Assets. Lấy canonical Lumi trước khi tạo compatibility prefab.
- `H3ArtSetup`: gọi trực tiếp hai atlas đang dùng, không dựa đường thư mục chứa concept đã lưu trữ.
- `build_kgw2026_pitch.py`: dùng Lumi_MapTraveler thay portrait nâu.
- `Docs/Art/asset-moves.json`: gỡ 25 alias tới tài nguyên/folder đã chuyển, lưu bản ghi từng alias tại [retired-path-aliases.json](../Development/Storage/UnusedAssets-20261113/retired-path-aliases.json).
- `art_asset_catalog.py`: registry lịch sử archived được trình bày như hồ sơ, không đòi ảnh gốc phải nằm trong Assets hoặc phải có trên máy clone để lập inventory runtime.
- `AssetCleanupAudit`: thêm launcher Play Mode và build Player QA tách biệt, không chạy setup/Prepare phát hành.
- Các công cụ mới nằm ở `Tools/Maintenance`; không chạy lại các script migration một lượt. Hai validator không sửa game assets.

**Không xóa entry runtime nào** trong DesignKitArt/AdventureUIArt hoặc gameplay JSON vì nhóm ảnh trên không có serialized binding runtime. Không đổi save schema, đường đi, điều kiện ending, quest, portrait chính thức, component hoặc namespace.

## Những phần giữ lại và việc chưa thể kết luận

Inventory có GUID, SHA-256, kích thước, kiểu file, static GUID consumers, native sprite localIDs/importers và nguồn đề cập trong tool. [Inventory đầy đủ](../Development/Storage/UnusedAssets-20261113/inventory.json), [kế hoạch nhóm đã thực hiện](../Development/Storage/UnusedAssets-20261113/archive-plan.json), [audit Unity sau](../Development/Storage/UnusedAssets-20261113/native-after/native-audit.json).

- `Resources/Art` còn 14 ảnh: rescue cat sheet (Texture2D cắt runtime), trạm Đà Lạt và 12 leisure props có key từ code/JSON. Tải thực tế bằng Resources được audit trong Unity. Chưa đổi toàn bộ sang serialized catalog vì chưa có runtime acceptance cho cơ chế mới; đây là ngoại lệ kỹ thuật còn tồn tại đối với yêu cầu một kho Art duy nhất.
- Chili/shallot JPG, source postcard, app icon gốc: không thuộc closure scene nhưng còn pipeline tạo derivative/icon. Chưa chạy lại pipeline tái tạo trong workspace cô lập để chứng minh có thể loại nguồn; giữ để tránh phá quy trình authoring. Ba icon Android có GUID trong ProjectSettings, không xóa theo kết quả closure âm tính.
- `NPC_Seoul_Curator.png`: không thuộc closure, còn trong RelicPilotSetup lịch sử. Tool này có thể cố gán lại binding cũ rồi kiểm tra/throw. Chưa xác minh thay thế toàn workflow import, nên chưa xóa ảnh/tool hoặc khẳng định nhóm NPC đã sạch.
- `H3_Stone_A/B.asset`: Editor H3 setup còn tạo/đọc chúng. Chưa chứng minh workflow này đã được thay hoàn toàn; chưa xóa.
- Những ảnh còn bị kéo vào Resources catalog cần đối chiếu ID được chọn trong mọi trạng thái quest/save, không coi `unityDependencyClosure=true` là bằng chứng được hiển thị. Inventory đánh dấu `NOT_PROVEN_BY_CLOSURE` để tránh kết luận vượt bằng chứng.
- Scene template còn bật trong build list; recovery scene và native render template giữ nguyên. Muốn loại phải kiểm tra build profile và quy trình tạo scene riêng.
- Script Phase/Hotfix và các harness QA lịch sử giữ nguyên khi chưa chứng minh workflow ngừng dùng. Ví dụ `portrait_story_evidence.py` kiểm tra capture Handoff05 lịch sử chứa tên portrait cũ; không sửa bằng chứng đó thành kết quả mới.
- Gói ImportSources vẫn là đầu vào của các công cụ authoring hiện tại; không xóa vì chỉ thấy ngoài Assets. Các snapshot/backups/Submission/QA acceptance được bảo toàn theo AGENTS.md.

## Kiểm chứng thực tế

| Kiểm tra | Kết quả |
| --- | --- |
| Unity trước và sau archive | Compile và native audit thoát 0, Unity 6000.6.5f1; log `Validation/UnusedAssets-20261113-before.log`, `-after.log`. |
| Native references | 0 missing script; 7 GUID lỗi và 6 object reference lỗi của Renderer2D template giữ nguyên từ baseline; không có lỗi mới. |
| Sprite/importer | Tất cả sprite ID/rect/pivot/border và Android/Standalone overrides của texture còn lại khớp trước; dependencies và GUID không đổi. |
| Resources | Kết quả tải literal keys và keys JSON bằng Unity khớp trước/sau. AppearanceReviewData optional QA fixture vẫn thiếu như baseline. Không tuyên bố đã bao phủ mọi phép ghép chuỗi có thể có. |
| Save | 4 assertions v1/v2 fixture trong Unity đạt; source SaveSystem byte-identical. Không ghi save người chơi. |
| Route | 6 thành phố, 2.949 cặp node, 5.901 assertions đạt trong Unity. |
| Catventure domain | 518 + 2 assertions đạt trong adapter host .NET; không phải Unity Player. |
| Windows QA build | Build thành công, log `Validation/UnusedAssets-20261113-player-build.log`; không sửa version hoặc chạy generator, không ghi đè Builds. |
| Windows Player chạy thực tế | **BLOCKED**: Start-Process báo Application Control policy blocked this file. Không có Player journey PASS. Exit code PowerShell 0 của lần launch lỗi không được hiểu thành game chạy thành công. |
| Editor Play Mode fallback | **FAILED**, Editor thoát 1: `Timed out: scene CityMap`, UI vẫn ở menu. Trước timeout, harness đã chạy kiểm tra catalog, save v1 và xác nhận Lumi canonical. Không có bằng chứng để quy lỗi cho game hay harness batch mode; không tự sửa gameplay để che thất bại. Log `Validation/UnusedAssets-20261113-editor-journey.log`, kết quả `Validation/UnusedAssets-20261113/EditorJourney/smoke-results.txt`. |
| Android build/cài và toàn bộ ending | Chưa được xác minh trong đợt này. |

[Kết quả kiểm chứng riêng](../Development/Storage/UnusedAssets-20261113/verification.json), [cây thư mục hiện tại](FINAL_TREE.md). `git diff --check` cuối đạt. Những lỗi runtime/hạn chế còn tồn tại khiến chưa đủ điều kiện tự commit và push. Không stage, commit, push, force push, merge main hoặc cập nhật bản nộp. Khi đủ điều kiện, phải stage theo whitelist code/asset moves/docs của nhiệm vụ; loại generated .csproj, cache, log, save fixtures, Player QA, recovery preimages, backup và nội dung không liên quan. Lỗi launch EXE là Application Control của hệ điều hành, không phải kết quả auto-review từ chối một lệnh Git.
