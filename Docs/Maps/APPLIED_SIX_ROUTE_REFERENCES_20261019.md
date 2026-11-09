# Apply six approved route references — waypoint patch

19/10/2026 · **IMPLEMENTED / WINDOWS_TARGETED_PASS / BEST_EFFORT_VISUAL_REVIEW / STOP**.

Người dùng đã chính thức chấp nhận sáu ảnh AI, gồm sai nhãn và các đoạn địa hình còn thiếu, cho phép tự đối chiếu best-effort thay cho PRELIMINARY_STOP. Đây là bản triển khai waypoint thật, không phải audit-only. Phê duyệt này không cho thay nền, node, arrival, graph, art, gameplay hoặc audio.

## Kết quả thực tế

**149/155 edge đã đổi waypoints; 6 edge giữ nguyên có lý do. 133 node/155 kết nối nguyên vẹn.** Source runtime thay tại `Assets/_Game/Resources/Content/GameplayContent.json`, và chỉ trường `cities[].edges[].waypoints` khác baseline mới nhất sau Phase5. Không thay status/requiredFlags/travel của edge.

| City | Edge hiện có | Geometry đổi | Lượt Player thực | Overlay canonical |
|---|---:|---:|---:|---|
| hoi_an | 22 | 18 | 25 | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/HoiAn_After_Routes.png) |
| hue | 28 | 27 | 32 | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/Hue_After_Routes.png) |
| da_lat | 26 | 26 | 29 | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/DaLat_After_Routes.png) |
| jeonju | 30 | 30 | 34 | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/Jeonju_After_Routes.png) |
| gyeongju | 25 | 25 | 28 | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/Gyeongju_After_Routes.png) |
| seoul | 24 | 23 | 27 | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/Seoul_After_Routes.png) |

[CSV từng edge trước/sau](AppliedRouteReferences-20261019/edge-diff.csv) · [JSON](AppliedRouteReferences-20261019/edge-diff.json) · [So sánh 6 ảnh](../../Validation/AppliedRouteReferences-20261019/BeforeAfter.html) · [Chọn riêng Edge ID/W0…Wn](../../Validation/AppliedRouteReferences-20261019/RouteReview.html) · [Runtime frames](../../Validation/AppliedRouteReferences-20261019/RuntimeReview.html).

## Cách áp dụng

Đã xem đủ sáu reference và sáu nền canonical ở 1448×1086, đối chiếu đường phố/cầu/bậc thang/kiến trúc cùng actual arrival. Tác giả dựng các hành lang theo landmark trên nền canonical, tái sử dụng đoạn chung, rồi gắn từng edge thật vào hai đầu cố định. Offline drafting network trong `Tools/apply_approved_route_references.py` chỉ phục vụ sinh dữ liệu, **không thêm graph/pathfinder runtime**. Nhãn AI không dùng để đổi ID/node. Graph thật và mapping 155 Edge ID cũ quyết định danh tính cạnh.

Hội An đi theo phố Cô Mai, hẻm, bờ kè, cầu nhỏ và vòng chợ; Huế theo bờ sông/cầu/cổng thành; Đà Lạt theo vòng hồ, phố nhà thờ, vườn và đường biệt thự phía bắc; Jeonju theo phố hanok/cầu kênh/sân thủ công; Gyeongju theo đường di tích/gò mộ/cầu; Seoul theo phố cung điện, công viên, bờ sông và đường lên đồi. Một số edge dài dùng chung hành lang vì graph không được thêm/xóa cạnh. Đi qua một vị trí vẽ của node khác không tạo thêm adjacency hay sự kiện mới; event vẫn xử lý theo graph hiện hành.

Mọi bends đều hữu hạn và trong 0..1. Điểm đầu/cuối do `CityRouteGeometry.Arrival` hiện có cấp; không đổi anchor. Dots và Lumi tiếp tục lấy cùng `CityRouteGeometry.Points`, không có đường trang trí riêng hoặc renderer mới. Speed190/camera/occlusion/collider nguyên vẹn. Khi QA, timeScale được tăng chỉ trong harness để rút thời gian chờ; source movement speed không đổi.

## Sáu edge giữ nguyên

| City | EdgeID | Cạnh | Lý do |
|---|---|---|---|
| hoi_an | H-E022 | H18 → H5 | UNCHANGED_H6: preserve exact covered bridge entry/floor/exit geometry and occlusion. |
| hoi_an | H-E009 | H5 → H6 | UNCHANGED_H6: preserve exact covered bridge entry/floor/exit geometry and occlusion. |
| hoi_an | H-E011 | H6 → H15 | UNCHANGED_H6: preserve exact covered bridge entry/floor/exit geometry and occlusion. |
| hoi_an | H-E021 | H17 → H5 | UNCHANGED_FERRY: retain existing ferry mechanism and water geometry; do not turn it into walking. |
| hue | U-E006 | U3 → U4 | UNCHANGED_FERRY: retain existing ferry mechanism and water geometry; do not turn it into walking. |
| seoul | S-E009 | S6 → S14 | UNCHANGED_FERRY: retain existing ferry mechanism and water geometry; do not turn it into walking. |

Không biến ba tuyến đò thành walk hoặc tạo cầu/vận chuyển mới. Ba tuyến đi qua phần Chùa Cầu giữ nguyên toàn bộ bends để bảo vệ entry/floor/exit và roof occlusion; coveredWalkways cũng byte/semantic exact.

## Các đoạn không thể bám sát hoàn toàn

| City | EdgeID | Cạnh | Hạn chế cụ thể |
|---|---|---|---|
| da_lat | D-E020 | D13 → D21 | APPROXIMATION_SLOPE: follow reference garden/lodge switchback; canonical stairs not fully visible. |
| gyeongju | G-E004 | G1 → G2 | APPROXIMATION_ENDPOINT: pond-side perimeter and small bridge corridor; discontinuous causeway remains visual limitation. |
| gyeongju | G-E006 | G2 → G17 | APPROXIMATION_WATER: approved reference has pond segment without continuous canonical bridge. |
| seoul | S-E002 | S0 → S14 | APPROXIMATION_WATER: island connector absent; shape follows approved reference, no invented ferry/bridge. |
| seoul | S-E017 | S12 → S13 | APPROXIMATION_WATER: uses same island/west-bank connector as approved S0-S14 reference; no continuous canonical footbridge. |

Ngoài các đoạn nước/dốc nêu trên, đầu/cuối canonical có thể khác vị trí ký hiệu AI; không dịch anchor để khớp nhãn. Huế U1–U2/U2–U14, Jeonju J0–J15/J2–J12/J4–J7, Gyeongju G0–G16 và Seoul S19–S20 còn các đoạn sát mái/tường hoặc đoạn nối cuối vào anchor không thể mô phỏng chiều sâu chỉ bằng waypoint. Đà Lạt D0–D12/D5–D11/D12–D10 dùng hành lang vòng nối khu vườn phía tây; không khẳng định mọi bậc/cầu phụ hiện rõ liên tục trên tranh. Một số đoạn gấp khúc còn thấy ở góc phố/vùng nhiều node. Chưa thêm occlusion hay sửa đồ họa để che các hạn chế đó.

Đây là những sai lệch được giữ theo quyết định best-effort, **không là bằng chứng địa hình hoàn hảo hoặc lý do dừng toàn bộ bản vá**. Các edge vẫn di chuyển được và được giao ảnh/data để chủ dự án trực tiếp đánh giá. Không dùng motion PASS để gọi tất cả nước/roof PASS.

## QA Windows

- Windows build final06 **Succeeded**, dùng builder Windows sẵn có (`Phase5Setup.Build` là tên helper cũ, không chạy task Phase5). Build03 chạy toàn bộ geometry ban đầu; build04–05 chỉ sửa QA scene. Kiểm tra hình học cuối bỏ spur quay đầu trên 11 edge, build06 kiểm lại đúng 11 edge hai chiều bằng helper Route35 hiện có. [Log build06](../../Logs/approved-routes-build06.log).
- **186 lượt thực qua CityMap.Move**: 164 lượt bao phủ **155/155 edge** ở bản đầu, sau đó 22 lượt kiểm lại 11 edge tinh chỉnh hai chiều. **175 traces khớp chính xác source cuối**, bao phủ đủ155 edge; 11 traces cũ trước cleanup không được dùng gọi geometry cuối PASS. Tất cả arrived đúng node, max sai số theo map RectTransform = 0.000136479 local pixels. [Trace khớp source cuối](AppliedRouteReferences-20261019/final-source-matching-traces.json).
- Runtime fixture có khai báo: dùng bản sao earned old full-story save, isolate một cạnh trong bộ nhớ để bắt controller đi đúng cạnh cần test, đặt vị trí đầu test rồi phục hồi exact save trước/sau mỗi thành phố/toàn run. Không gọi nó fresh story hoặc chứng minh lại toàn quest graph. Không sửa save cá nhân và không tiêm flag quest/gate.
- H6 roof entry/covered/exit có frames thật và assertion hide/reveal; dot mesh tiếp tục bỏ vùng cover và không raycast. All edge render samples thuộc open/discovered geometry, không sửa save.
- Fresh02: vào Hội An từ World Map bằng thao tác thật; HIDDEN không có input/path, chỉ vùng đầu hiện; render không làm đổi progress. Không chạy lại main story để mở từng gate vì toàn bộ gate/conditions/progression code exact.
- Sáu Player process restart riêng: mỗi fixture chọn lại flat cityId/node/visited từ cityProgress earned tương ứng, không thêm flags/items/quests/rewards. Restart giữ arrival và progress, không reward/ceremony tự phát; Map/World UI đi lại hoạt động. Đây là location fixtures cho persistence, không phải sáu fresh story runs.
- Sáu cặp runtime overview dùng cùng 1280×960, zoom1, focus(0.5,0.5), node0, cùng input save: before01 so với refine01 của build cuối. Native offline before/after cùng nền/scale/nhãn, chứa đủ mọi edge. Movement frames build03 có ghi rõ giai đoạn trước cleanup; không giả là ảnh build06. Fresh/restarts build04–05 vẫn có giá trị cho state/gate vì build06 chỉ bỏ spur ở 11 polyline, không đổi save/arrival/conditions. Refine01 còn re-entry/snapshot sáu thành phố ở build cuối.
- **459 bounded checks / 119 protected files exact**, gồm sáu PNG nền và .meta, HoiAnData, route/movement/discovery/save code, NPC/art bindings, 18 check-in/H6, Phase4/relic, Phase5 ending, BGM/SFX/MP3 metadata. [Verification + 6 hash đầy đủ](AppliedRouteReferences-20261019/verification.json).

| Run | Kết quả | Assertions PASS |
|---|---|---:|
| [after-overview01](../../Validation/AppliedRouteReferences-20261019/after-overview01/smoke-results.txt) | PASS | 55 |
| [after01](../../Validation/AppliedRouteReferences-20261019/after01/smoke-results.txt) | PASS | 1433 |
| [before01](../../Validation/AppliedRouteReferences-20261019/before01/smoke-results.txt) | PASS | 55 |
| [fresh01](../../Validation/AppliedRouteReferences-20261019/fresh01/smoke-results.txt) | FAIL | 20 |
| [fresh02](../../Validation/AppliedRouteReferences-20261019/fresh02/smoke-results.txt) | PASS | 31 |
| [refine01](../../Validation/AppliedRouteReferences-20261019/refine01/smoke-results.txt) | PASS | 235 |
| [restart-da_lat-01](../../Validation/AppliedRouteReferences-20261019/restart-da_lat-01/smoke-results.txt) | PASS | 15 |
| [restart-gyeongju-01](../../Validation/AppliedRouteReferences-20261019/restart-gyeongju-01/smoke-results.txt) | PASS | 15 |
| [restart-hoi_an-01](../../Validation/AppliedRouteReferences-20261019/restart-hoi_an-01/smoke-results.txt) | FAIL | 1 |
| [restart-hoi_an-02](../../Validation/AppliedRouteReferences-20261019/restart-hoi_an-02/smoke-results.txt) | PASS | 15 |
| [restart-hue-01](../../Validation/AppliedRouteReferences-20261019/restart-hue-01/smoke-results.txt) | PASS | 15 |
| [restart-jeonju-01](../../Validation/AppliedRouteReferences-20261019/restart-jeonju-01/smoke-results.txt) | PASS | 15 |
| [restart-seoul-01](../../Validation/AppliedRouteReferences-20261019/restart-seoul-01/smoke-results.txt) | PASS | 15 |

Fresh01 và restart-hoi_an-01 FAIL do harness đọc Map/renderer khi scene vẫn World Map (NullReferenceException trong QA); sửa duy nhất flow QA vào CityMap qua World Map trước. Fresh02 và restart-hoi_an-02 PASS. Giữ nguyên failure log/ảnh/save; không đổi FAIL cũ thành PASS. Các accepted run không có managed exception/GAMEPLAY FAIL. D3D12/Unity shutdown ComputeBuffer warning có thể vẫn xuất hiện; không dùng task geometry để gọi memory/performance PASS.

**ANDROID_BUILD_NOT_TESTED · ANDROID_DEVICE_NOT_TESTED**. Không chạy lại 29 quest/toàn cốt truyện, không tuyên bố mọi quest hoặc terrain toàn game đã nghiệm thu lại. Bảo toàn gameplay dựa trên semantic comparison toàn bộ non-waypoint data, protected source và targeted movement/discovery/persistence checks.

## Files và tọa độ

Production: duy nhất `Assets/_Game/Resources/Content/GameplayContent.json` (waypoints). Development/Editor QA: thêm `GameplaySmokeRunner.ApprovedRoutes.cs` + meta, một dòng dispatch trong `GameplaySmokeRunner.cs`; không manager/Canvas/EventSystem mới. Tools author/export/verify/launcher/suite/finalize nằm trong `Tools/`; tài liệu/bằng chứng ở `Docs/Maps/AppliedRouteReferences-20261019` và `Validation/AppliedRouteReferences-20261019`. [Changed files](AppliedRouteReferences-20261019/changed-files.json).

CSV/JSON `OldWaypoints`/`NewWaypoints` là Unity normalized0..1, gốc trái dưới, chỉ bends. `OldPolylineMap1000`/`NewPolylineMap1000` gồm cả arrival W0/Wn, gốc trái trên: X=1000u, Y=1000(1-v). Native PNG px=1.448X, py=1.086Y. Không crop/warp/rotate, không lấy pixel AI áp thẳng. 155 Edge ID registry cũ không thay và không thêm vào gameplay. Sáu after PNG là composite kỹ thuật riêng ngoài Assets; source canonical PNG không ghi đè.

## Checkpoint và điểm dừng

Base: `D:\GAMEKKK\Backups\CheckpointPhase5Final-20261019` — bao gồm ending mới cùng mọi thay đổi Phase4/NPC/lighting/audio đã có, không rollback. Pre-edit scoped snapshot `BeforeAppliedRouteReferences-20261019` giữ ba file, không là backup standalone. Final incremental `CheckpointAppliedSixRouteReferences-20261019`, [receipt](AppliedRouteReferences-20261019/checkpoint.json), cần base và chain đã ghi; không copy Library/Temp/Builds, không xóa save/backup/evidence cũ.

**DỪNG sau bản waypoint/QA/checkpoint này. Không Phase5/6/7 hoặc task khác.** Chờ chủ dự án xem sáu route mới.
