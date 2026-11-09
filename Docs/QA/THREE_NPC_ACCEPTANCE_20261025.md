# Nghiệm thu ba NPC — 25/10/2026

Unity Editor/license **PASS**. Windows focused runtime **PASS: 157 assertions**, khởi động Player mới **PASS: 102 assertions**. Kiểm tra source giới hạn **186 assertions PASS**. Không chạy full campaign hoặc Android.

| Node | NPC | Map / portrait / thoại / thưởng / save |
|---|---|---|
| G16 | Kang Tae-ho · Người trông đền | PASS / PASS / PASS / 2 trà + 5 xu PASS / PASS |
| G21 | Choi Eun-su · Người hái thuốc | PASS / PASS / PASS / 2 trà PASS / PASS |
| S12 | Han Do-yun · Người chăm công viên | PASS / PASS / PASS / 3 trà + 12 xu PASS / PASS |

## Thay đổi và phạm vixx

- Tiếp tục source ThreeNpc, không reset hoặc thay artwork. Ba map actor dùng `AdventureUIArt.mapActors`, `CityMap` và native sprite mesh hiện có. Tỷ lệ 0,72 và kích thước 52×62 theo NPC sẵn có; offsets G16 (22,5), G21 (24,5), S12 (-24,5). Actor vẫn hiện sau khi nhận quà.
- Không đổi tọa độ canonical/arrival, graph, geometry, world gates hay tuyến thuyền. Đã xem ba ảnh map thật: NPC đứng ở phần đất/lối bên điểm tương tác; Lumi vẫn di chuyển ra rồi quay lại theo tuyến cũ. Không tạo đường hoặc hệ thống render mới.
- Sửa lỗi ghi dấu `?` trong ba lời thoại của lượt trước bằng dữ liệu UTF-8. Do-yun có vai người chăm công viên; giữ đúng sprite đã chọn theo yêu cầu.
- Giữ nguyên `gyeongju:loot_lake`, `gyeongju:secret_herb`, `seoul:loot_park`, phần thưởng, oneShot, save schema và nhánh Grant hiện có. Không sửa NodeEventManager hoặc GameplayPanels.
- Registry chỉ cập nhật ba danh tính mới theo native binding thực tế và bằng chứng runtime; không mở rộng duyệt hoặc thay mặt NPC cũ.

## License và build

Lần trước chạy dưới `codexsandboxoffline`; lượt kiểm tra này dùng đúng môi trường Windows `admin`. Không có Editor/Hub đang mở project khi bắt đầu. Editor thường dưới admin giải quyết được entitlement; sau đó batchmode dưới admin import/build thành công, exit 0. Không sửa license, đăng xuất, cài lại Unity hoặc chạy hai Editor đồng thời. Không cần người dùng đăng nhập/kích hoạt lại.

Chỉ một thư mục build QA: `Builds/Windows-ThreeNpcQA-20261025/LightBetweenCultures.exe`; giữ nguyên các build RC2/acceptance cũ. Sau build đầu có hai lần cập nhật scripts-only trong cùng thư mục để sửa phép kiểm tra X padding và thao tác runner phải chờ debounce đóng panel. Đây là hai lỗi runner, không thay giao diện/gameplay. Hai kết quả thất bại đầu được lưu riêng, không bị biến thành PASS.

## Runtime và save

- Chạm trực tiếp sprite trên map mở đúng dialogue; native sprite/aspect/mesh, chân dung và tên đúng. Sáu ảnh bên dưới đã được mở xem, chữ Việt đọc được, crop không lẫn nhân vật bên cạnh, nút thao tác vẫn hiện.
- Đóng panel trước nhận quà không thay inventory/coins/quest; mở lại bình thường. Lần đầu nói chuyện nhận đúng trà/xu và receipt đúng từng số lượng.
- Tám callback talk liên tiếp khi receipt đang mở và callback collect cũ không cấp thêm. Quay lại có lời chào hoàn thành; không phát lại receipt. Đóng receipt bằng X và đóng/mở dialogue đều hoạt động.
- Fixture chưa nhận dùng bản sao earned save cũ, chỉ xóa ledger của ba event trong save QA; hai event Gyeongju vốn chưa nhận vẫn nhận bình thường. Fixture save cũ đã nhận dùng nguyên khóa cũ cho cả ba: S12 đã được nhận trong save cũ thật của QA; G16/G21 là fixtures claimed được đánh dấu rõ. Cả ba không cấp lại.
- Tắt Player rồi chạy process mới: coins, toàn bộ items, resolvedEvents và rewardGrantIds khớp bản earned-after; cả ba vẫn hoàn thành, không thêm thưởng/receipt. Đây là save/load qua hai process Windows thật.
- Save thật của owner giữ nguyên SHA256 `9AD207C979524747A393E51FCE4485AD78DF1E15436E900B51FFE93712B51299`; tất cả fixture nằm trong `Validation/ThreeNpc-20261024`.
- Static diff giữ nguyên mọi event khác, sáu thành phố/133 node, quests, items/prices, puzzles, relic content, ending, CityNavigation, Lumi/skin và UI NodePanelCompact. H3 vẫn NOT_REPRODUCED; không kiểm tra lại hay đổi thành FIXED.

## Bằng chứng

| Node | Ảnh NPC trên City Map | Ảnh hội thoại |
|---|---|---|
| G16 | [Map G16](../../Validation/ThreeNpc-20261024/ThreeNpc-Map-G16.png) | [Thoại G16](../../Validation/ThreeNpc-20261024/ThreeNpc-Dialogue-G16.png) |
| G21 | [Map G21](../../Validation/ThreeNpc-20261024/ThreeNpc-Map-G21.png) | [Thoại G21](../../Validation/ThreeNpc-20261024/ThreeNpc-Dialogue-G21.png) |
| S12 | [Map S12](../../Validation/ThreeNpc-20261024/ThreeNpc-Map-S12.png) | [Thoại S12](../../Validation/ThreeNpc-20261024/ThreeNpc-Dialogue-S12.png) |

[157 kiểm tra runtime](../../Validation/ThreeNpc-20261024/acceptance-runtime-results.txt) · [102 kiểm tra restart](../../Validation/ThreeNpc-20261024/acceptance-restart-results.txt) · [186 kiểm tra source](../../Validation/ThreeNpc-20261024/acceptance-static-checks.txt) · [Hash save owner](../../Validation/ThreeNpc-20261024/acceptance-owner-save-verified.json) · [Capture manifest](../../Validation/ThreeNpc-20261024/acceptance-runtime-captures.txt).

Log license/import/build: `license-admin-editor.log`, `acceptance-build.log`, `acceptance-scripts-build.log`, `acceptance-scripts-final-build.log`. Log Player: `acceptance-player.log`, `acceptance-restart-player.log`. Lượt đầu đã được sao lưu trước khi chạy restart; log/kết quả hai lần lỗi runner nằm ở `acceptance-first-attempt-*` và `acceptance-second-attempt-*`. Giữ nguyên báo cáo BLOCKED cũ như lịch sử.

## File thực sự sửa trong lượt nghiệm thu

- `Assets/_Game/Resources/Content/GameplayContent.json`: sửa UTF-8 và lời thoại ba node.
- `Assets/_Game/Resources/AdventureUIArt.asset`: thêm ba map actor, binding được Unity import thực.
- `Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png.meta` và `Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png.meta`: Unity xác nhận/serialize native slices, giữ ID và rectangle cũ.
- `Assets/_Game/Scripts/Editor/ThreeNpcSetup.cs`: cấu hình ba map actor và dùng build helper hiện có.
- `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.ThreeNpc.cs`: bổ sung map/receipt/input/di chuyển vào runner đã chuẩn bị, sửa hai lỗi runner.
- `Docs/Art/NPC_CANONICAL_REGISTRY.json`, `Docs/Art/NPC_CANONICAL_REGISTRY.md`, `Docs/Art/native-art-bindings.json`: registry/export thực tế và trạng thái kiểm chứng.
- Báo cáo này; dữ liệu, logs và scripts tái lập nhỏ trong `Validation/ThreeNpc-20261024`. Không tạo full checkpoint, gallery hoặc xóa save/backup/ảnh gốc.

## Vấn đề còn lại

Không còn lỗi runtime được phát hiện trong phạm vi ba NPC. `Tools/art_asset_catalog.py --inventory-only` đã được chạy theo AGENTS.md nhưng dừng ở hash migration lịch sử của `Map_Hue_DestinationPreview_A.png`; không sửa ảnh/manifest Huế ngoài phạm vi. Phần merge metadata theo chính xác sprite reference và dựng registry của tool đã chạy riêng thành công. Full inventory vẫn ghi **FAIL**, không coi là đã PASS; xem `acceptance-art-inventory.log`.
