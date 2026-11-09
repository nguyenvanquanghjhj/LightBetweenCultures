# Phase B — đường vẽ của chủ dự án đã chạy trong Unity

Ngày thực hiện: 2026-10-19 → 2026-10-20. Điểm dừng: bàn giao bản route mới, chờ chủ dự án xem; không triển khai task khác.

**Đã áp dụng vào source và Windows Player**, không chỉ dựng bản HTML. Giữ 133 node, vị trí marker và arrival canonical. Graph 155 cạnh cũ được giữ nguyên trong file lịch sử/compatibility nhưng không còn là graph di chuyển đang hoạt động. Graph mới dùng các giao điểm thật trên nét vẽ, điểm chuyển thuyền và đoạn tiếp cận chân node.

Mở [gallery ảnh Player và sáu bản toàn graph](gallery.html). Bảng [442 cạnh navigation](navigation-edges.csv), [133 arrival và đoạn tiếp cận](arrival-connectors.csv), [kiểm chứng source/Player/protection](verification.json).

## 1. Dữ liệu và cơ chế thực tế

| Thành phố | Node canonical | Đỉnh navigation | Cạnh đi bộ | Cạnh thuyền | Điểm chuyển tiếp |
|---|---:|---:|---:|---:|---:|
| Hội An | 22 | 71 | 72 | 1 | 2 |
| Huế | 22 | 65 | 65 | 0 | 0 |
| Đà Lạt | 22 | 69 | 71 | 1 | 2 |
| Jeonju | 22 | 63 | 64 | 0 | 0 |
| Gyeongju | 22 | 79 | 80 | 2 | 4 |
| Seoul | 23 | 84 | 85 | 1 | 2 |
| **Tổng** | **133** | **431** | **437** | **5** | **10** |

298 đỉnh nội bộ không phải node gameplay, không có NPC/quest mới, không được ghi vào Save.node. Mỗi chân node canonical là terminal một nhánh; người đi qua ngã rẽ không bị kéo vào chân NPC. Những ngã giao thực của nét xanh được nối, không chỉ vẽ chồng hình.

Nguồn production duy nhất mới: `Assets/_Game/Resources/Content/CityNavigation.json`. Phần renderer đường chấm và Lumi cùng đọc polyline này. `GameplayContent.json` vẫn chứa 155 cạnh legacy, byte-identical với trước task; xem [bản lưu read-only](legacy-155-edges-READONLY.json). Không nên dùng bộ 155 Edge ID cũ để chỉnh graph Phase B mới.

Compiler dùng đúng stroke Phase A, hợp nhất điểm chạm gần nhau và cắt giao điểm thật; chỉ nối những khoảng hở ngắn có bằng chứng từ nét xanh hoặc bị vòng tròn node che. Provenance và các khoảng nối nằm trong `Validation/OwnerRoutePhaseB-20261019/*-implementation.json`. Không thay nền bằng ảnh vẽ tay và không import ảnh tham khảo vào runtime.

## 2. Pathfinder, discovery và giao điểm

Dijkstra dùng tổng chiều dài polyline theo tỷ lệ map, không đếm số cạnh. Thời gian boarding ngắn không cộng thành một hình phạt khoảng cách. Cạnh khóa và chiều đi được lọc trước khi tìm đường. Không còn BFS cạnh-count trong CityMap; graph legacy nếu cần fallback cũng dùng Dijkstra chung.

Access contracts lấy từ stage/flag hiện hữu, không lấy hình học cũ. HIDDEN vẫn không chọn được và không vẽ đường bí mật. Khi bấm đích hợp lệ mới trong lúc đang đi, Lumi đến ngã navigation kế tiếp rồi tìm lại đường tại chính ngã đó, không quay về node canonical trước.

| Giao điểm | Thành phố | Vertex runtime | Bậc | Kiểm thử Windows Player |
|---|---|---|---:|---|
| X1 | Hội An | J96 | 3 | H0 hướng H8, đổi sang H21 tại đúng ngã: PASS |
| X3 | Jeonju | J430 | 3 | J16 hướng J5, đổi sang J15: PASS |
| X4 | Gyeongju | J146 | 3 | G0 hướng G1, đổi sang G16: PASS |

X3 dùng giao điểm nét thật tại pixel khoảng (542,422), không dùng tâm khoanh review ước lượng (530,420). [Tọa độ và bằng chứng](junction-proof.json).

## 3. Năm tuyến thuyền và R1 đã đính chính

| Tuyến | Cặp node kiểm thử hai chiều | Kết quả |
|---|---|---|
| R1 Hội An | H0 ↔ H16 | Đi bộ tới bến, qua nước bằng thuyền, xuống bến H16: PASS |
| R2 Đà Lạt | D13 ↔ D21 | PASS |
| R3 Gyeongju | G2 ↔ G17 | PASS |
| R4 Gyeongju | G1 ↔ G2 | PASS |
| R5 Seoul | S12 ↔ S14 | PASS; kiểm tra thêm ở save mới trước flag NPC |

R1 không còn là nhánh cụt quay về A. Giữ nét đỏ A→B, nối tiếp **bằng thuyền** dọc mặt nước đến jetty H16 và dùng lại đường đi bộ H16→H12 có sẵn. Marker H0 trên nước không bị di chuyển, cũng không bị coi là chỗ đứng bộ hoặc bến khác.

Lumi và sprite thuyền đã có `LeisureProps/Hue_PedalSwanBoat` nằm cùng parent khi qua nước; không thêm art. Hai đầu tuyến có chuyển walk/ferry, không đi bộ trên đoạn đỏ. Trace vị trí thực 10 lượt ở `windows720/R*-trace.json`, ảnh trước/đang đi/đã xuống bến trong gallery.

**Vá từ fresh progression:** R5 mới là tuyến qua sông thiết yếu, trong khi flag `seoul.ferry_acknowledged` cũ thuộc shortcut optional của graph đã nghỉ. Không mang flag optional đó sang khóa R5 mới. Giữ nguyên hội thoại/NPC và lịch sử flag; không thay điều kiện World Map hoặc quest. Hai lượt mới đều vào được Seoul và hoàn thành nghi thức sau bản vá.

## 4. Bảy arrival lệch nét và đoạn tiếp cận

Giữ nguyên tọa độ canonical của cả bảy điểm. Chỉ thêm đoạn đi bộ giữa arrival thật và nét xanh; không dồn S8 vào chân S5.

| Node | Khoảng tiếp cận (pixel ảnh vẽ) |
|---|---:|
| H10 | 21.239 |
| J7 | 15.337 |
| G6 | 16.129 |
| G7 | 20.687 |
| S0 | 17.663 |
| S5 | 34.284 |
| S8 | 21.750 |

CSV arrival ghi toàn bộ tọa độ map 0–1000, gốc trên trái và polyline pixel nguồn; không được áp pixel giữa các ảnh khác kích thước. U10 Huế có nét cap rời: nối qua sân đi bộ phía trước đình/pavilion sang hành lang U11. Không tự tạo bến hay thêm node.

## 5. Bảo toàn và độ rõ ảnh kiểm thử

`verification.json` PASS: 133 IDs/arrival giữ nguyên, Unity Player parse đúng ID và hình học production (sai số float dưới 1e-6), mỗi canonical foot là terminal. Các guard gồm sáu nền canonical, Content, SaveSystem, presentation, ánh sáng, H6 và ProjectSettings đều giữ nguyên. **52 texture import metadata từ CheckpointMapClarity giữ byte-identical.** APK Test cũ giữ nguyên hash.

Theo phản hồi của chủ dự án về ảnh kiểm thử mờ: ảnh nhận xét thị giác được chụp trong **Windows Player 720×1280**, zoom QA theo kích thước nguồn để không nhìn toàn map bị thu nhỏ trên nền trống. Gallery có ảnh 720 pixel nguyên gốc để mở trực tiếp. Không thay zoom mặc định của game, không sharpen giả, không dùng ảnh technical overlay làm bằng chứng chất lượng art runtime. Đây là sửa cách chụp QA, **không tuyên bố task route làm art sắc nét hơn**.

Chưa có bộ runtime trước/sau cùng save/camera cho mọi thành phố từ trước khi thay graph; không giả tạo ảnh trước. Bằng chứng cũ Phase A giữ nguyên để đối chiếu, còn ảnh Player và overlay trong gallery là trạng thái mới thực tế.

## 6. QA và giới hạn

| Kiểm tra | Kết quả / bằng chứng |
|---|---|
| Windows compile/build source cuối | **PASS**, `Logs/owner-route-b-final-build.log` |
| 6 graph active, 133 arrival, bounds/connectivity, Dijkstra weighted/one-way/locked shortcut | **PASS**, `junctions720/smoke-results.txt`, runtime-navigation JSON |
| R1–R5 đi hai chiều, cả walk/ferry | **PASS**, 10 traces và ảnh Player |
| X1/X3/X4 đổi hướng tại ngã thật | **PASS**, `junctions720` |
| Fresh tuyến Việt Nam trước và Hàn Quốc trước | **PASS qua các tiến trình nối tiếp**; xem giải thích dưới |
| Boss Seoul hai pha, nhánh repair/battle, 6 relic / 24 fragment, ceremony và ending | **PASS**, `story-vietnam-seoul02`, `story-korea-seoul02` |
| Đóng cửa sổ giữa R1 rồi restart | **PASS**, `mid-boat-close/interruption.json`, `mid-boat-restart` |
| Load ending không phát lại credits/ceremony, không reward lặp | **PASS**, `ending-restart` |
| Zoom/pan/pinch/mouse, HIDDEN, input sau di chuyển | **PASS** Windows, `junctions720` |
| APK Android mới | **BUILD PASS**, 289,557,820 bytes; không phải kích thước total artifacts của BuildPipeline |
| Android device/touch/performance thực | **NOT_TESTED** |
| Chạy lại toàn bộ optional 22 puzzle, 12 leisure animation, 29 quest hoàn tất | **NOT_TESTED toàn bộ**; definitions được bảo toàn, test dependencies hành trình chính |
| Nghiệm thu mỹ thuật toàn bộ 442 cạnh | **Chờ chủ dự án xem**; connectivity/di chuyển không chứng minh mọi đoạn đều đúng địa hình tranh |

Hai lượt bắt đầu fresh thật: `story-vietnam` và `story-korea` đã đi năm thành phố, không fixture bật flag. Lỗi S15 do R5 nói trên được bắt tại Seoul. Sau sửa, dùng **save thực đã kiếm được trước khi vào Seoul**, tiếp tục thành `story-vietnam-seoul02`/`story-korea-seoul02`; provenance có hash, không bơm fragment/quest. Cả hai hoàn thành thành phố sáu, boss và ending. Không mô tả đây là hai lượt một tiến trình không gián đoạn.

Các lần lỗi giữ nguyên: `runtime02`/`windows720` có lỗi QA thao tác debounce/đích HIDDEN; hai lượt fresh đầu bắt được R5; hai lần resume dùng nhầm save sau thất bại được giữ lại, thay bằng đúng checkpoint pre-entry. Không đổi báo cáo cũ thành PASS. Các lượt cuối nêu trên đều `ALL CHECKS PASSED`.

Persistence vẫn dùng save hiện hữu: đang di chuyển thì lưu **node canonical gần nhất đã đến**, không lưu junction hoặc tọa độ giữa sông. Restart trở lại arrival an toàn đó, không resume chính xác vị trí thuyền. Không đổi schema, quest/reward ledger, BGM/SFX, ánh sáng hay check-in.

## 7. Source đã sửa/thêm

- `Assets/_Game/Resources/Content/CityNavigation.json` + `.meta`: graph production mới.
- `Assets/_Game/Scripts/Core/CityNavigation.cs` + `.meta`: model, Dijkstra, access và catalog chung, không manager mới.
- `Core/GameData.cs`: dùng route mới, fallback Dijkstra.
- `Maps/CityMap.cs`: di chuyển polyline, chuyển thuyền, replan tại junction.
- `Maps/CityMapPaths.cs`: renderer hiện hữu lấy graph mới.
- `Maps/CityRouteGeometry.cs`: đọc polyline navigation; giữ cơ chế covered hiện hữu.
- `Validation/GameplaySmokeRunner.cs`, `.Terrain.cs`, `.OwnerRoutesB.cs` + meta mới: targeted QA, ảnh Player và trace.
- `Editor/OwnerRouteAndroidBuild.cs` + `.meta`: entry build Android độc lập, không SetupAll, không ghi đè APK cũ.
- `Tools/owner_route_phase_b.py`, `owner_route_b_delivery.py`, `owner_route_b_checkpoint.py`: compiler từ nét Phase A, xác minh/bàn giao và checkpoint có kiểm tra hash.
- Thư mục Docs này và `Validation/OwnerRoutePhaseB-20261019`: bảng, chứng cứ, saves QA riêng, không đụng save người chơi.

## 8. Build, checkpoint và điểm dừng

- Windows: `Builds/Windows/LightBetweenCultures.exe`.
- APK mới: `Builds/Android/LightBetweenCultures-OwnerRoutesB-20261019.apk`. Package/signing theo cấu hình đang có, không sửa keystore/settings. [Hash, kích thước file thật và build result](android-apk-file.json).
- Trước thay đổi: `D:\GAMEKKK\Backups\BeforeOwnerRoutesPhaseB-20261019`, đã xác minh payload và base chain.
- Checkpoint cuối: `D:\GAMEKKK\Backups\CheckpointOwnerRoutesPhaseB-20261019`, **incremental, cần base**. [Receipt sau copy/verify](final-checkpoint.json) ghi hash manifest và dung lượng thực; không phải full-project backup, không chứa Library hoặc bản build. Ước tính trước copy: 256 file, khoảng 92.29 MiB, gồm source và bằng chứng được gallery sử dụng.

Chờ chủ dự án xem bản route/ảnh sắc nét gốc trong gallery. Không tự làm Phase khác hoặc thay art theo cảm tính.
