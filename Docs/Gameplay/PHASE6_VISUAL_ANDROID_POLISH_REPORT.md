# Phase6 — Visual / Android presentation polish

19/10/2026 · **WINDOWS_TARGETED_PASS / VISUAL_PENDING / ANDROID_NOT_TESTED · STOP**

## Baseline và phạm vi

Đã đọc đầy đủ prompt Phase6, MASTER_EXECUTION_CHECKPOINT, báo cáo Phase4/5, NPC marker, approved routes, SFX và BGM. Phase5 và task waypoint đã dừng; preflight không có Editor/Player hay agent khác đang sửa production. Unity6000.6.5f1. Project hiện không có Git repository, dùng incremental checkpoint.

Base: `D:/GAMEKKK/Backups/CheckpointAppliedSixRouteReferences-20261019`, manifest SHA256 `daa4acf478fc51184cc84efbfc0dc2ef44f7a1dd0c57b4f8f9096b0b6d282b48`. Base phụ thuộc CheckpointPhase5Final-20261019 và chain của nó, không phải full backup độc lập. Scoped pre-edit snapshot `D:/GAMEKKK/Backups/BeforePhase6-20261019`, khoảng2.34MB metadata/source, không copy Library/Builds/cache. Hash baseline334 file liên quan; không quét đệ quy toàn bộ GAMEKKK.

## Những sửa đổi thực tế

| File production | Lỗi đã tái hiện | Thay đổi tối thiểu |
|---|---|---|
| `Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Jeonju_HanjiSeal_Source.png.meta` | Các đảo alpha của vật bên cạnh lọt vào3 mảnh | Custom sprite outlines cho hanok_pattern/dyed_thread/old_hanji_core; giữ rect, pivot, GUID/localID và PNG |
| `Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Seoul_ThreeRelayPrism_Source.png.meta` | Khung relay dính dải của vật cạnh trái | Outline riêng cho relay_frame, không đổi source/binding |
| `Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png.meta` và Seoul tương ứng | Thumbnail WorldMap nhiễu màu khi thu nhỏ mạnh; CityMap gốc bình thường | Bật mipmaps, Bilinear→Trilinear trên đúng2 full-map single sprites; không giảm MaxSize, không nén phá hủy, không đổi pixels |
| `Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs` | Target1/3 protrude khỏi board ở portrait | Clamp tâm target theo chiều rộng board, giữ96×108 target/88×100 piece, giữ game tolerance và callback. Landscape đủ rộng giữ vị trí cũ |

Chỉ5 file production, trong đó4 importer metadata +1 UI script. Thêm Editor-only `Phase6Setup.cs` để áp dụng đúng4 outline, kiểm chứng GUID/localID không đổi rồi build; không reslice/regenerate scene/atlas. Thêm partial QA trong GameplaySmokeRunner hiện có và1 dòng dispatch; không có manager/Canvas/EventSystem mới. Tools/report/checkpoint nằm ngoài runtime.

### Crop / art status

| Sprite | localID giữ nguyên | Rect Unity BL giữ nguyên (x,y,w,h) | Outline | Kết quả runtime |
|---|---|---|---|---|
| fragment_jeonju_hanok_pattern | -1688845438 | 380,14,255,390 | 33 điểm | PASS: bỏ vệt cạnh trên |
| fragment_jeonju_dyed_thread | 412024830 | 635,0,310,404 | 44 điểm | PASS: bỏ vệt trên/trái và phải |
| fragment_jeonju_old_hanji_core | 1835256344 | 0,4,385,420 | 36 điểm | PASS: bỏ vệt cạnh phải |
| fragment_seoul_relay_frame | 1990054730 | 890,4,360,425 | 49 điểm | PASS: bỏ vệt cạnh trái |

GUID Jeonju `c05ae4c2fe3fa6b49ac9787285d03f21`; Seoul `0b39cc9d42dc56640bc206a885645080`. Tool kiểm tra reference trước/sau trên mọi sprite của2 sheet, không tạo sprite runtime mới. Outline lấy convex hull của chủ thể alpha liên tục lớn nhất, nới2 source pixels giữ viền antialias; được kiểm tra bằng ảnh runtime. Không viết lại PNG, không upscale. `crop-outline-plan.json` ghi tọa độ/polygon nguồn chính xác. Các crop khác không đụng. Đây là sửa phần dư, không tái vẽ art.

| Seoul fragment | Nghĩa/proof thật | Binding hiện có | Kết luận |
|---|---|---|---|
| heritage_data | Dữ liệu di sản, archive_read tạiS2 | Không có fragment sprite, dùng slot trung tính | ART_PENDING_USER_APPROVAL: cần biểu tượng dữ liệu di sản |
| jiwon_sync | Lõi đồng bộ Ji-won, relay_reported tạiS7 | Slot trung tính | ART_PENDING_USER_APPROVAL: cần lõi đồng bộ, không gán hình tín hiệu khác |
| stellar_ring | Vòng kính tinh tú sau puzzle_tower/tower_calibrated tạiS10 | Slot trung tính | ART_PENDING_USER_APPROVAL: cần vòng kính, không dùng kính stargazer_lens (item khác) |

Đã xem sheet Seoul gốc và đối chiếu native asset catalog. Ba hình tín hiệu trong sheet không tương ứng canonical milestone. Journal/portal/crystal/glasses trong kho hoặc generic vector không phải bản art đã được duyệt cho3 danh tính này; giữ pending. Gameplay24fragment vẫn hoạt động, không thêm item/condition. Hình lăng kính đầy đủ và khung relay giữ binding cũ.

## Bảo toàn

`source-verification.json`:335 bounded checks PASS. `GameplayContent.json`, CulturalRelics/FinalConnectionPresentation, backend/save/quest/battle/discovery, nodes/arrivals/waypoints đều byte-exact. Sáu PNG canonical, các sheet PNG, AdventureUIArt.asset, registry NPC,18 postcard,4MP3/7WAV và audio code giữ nguyên hash. HoiAn A/B20%/C và WorldReactions không sửa. H6/photo/collection/details function bodies byte-exact; H6 target vẫn .62 theo dữ liệu cũ, clamp không thay nó; real touch/mouse2-symbol drag và Cancel giữ exact save.

Bảy actor H10/H14/G9/G14/S2/S7/S11 giữ mặt, sprite và portrait. QA kiểm scale.72, hitarea52, chân/bóng và marker không che mặt; G14/S2 giữ NPC+check-in options và chạm actor mở đúng portrait. Không đổi NPC canonical, dialogue, quest hay H6. Unity có thêm Android icon serialization vào ProjectSettings lúc build; đã xác minh chỉ block generated ấy khác rồi phục hồi đúng bytes trước task, không restore config khác.

## Portrait và touch

Windows framebuffer **native** ở540×960,720×1280,1080×1920,1080×2400, screenshot supersize=1. QA-only Win32 resize window của chính Player vượt chiều cao desktop; `Screen.width/height` và kích thước PNG đều đúng. Không dùng resize Pillow hoặc supersize từ cửa sổ nhỏ làm bằng chứng cuối. `main01` giữ làm chẩn đoán supersize, thay bằng `main02` để nghiệm thu native. Hội thoại ở đúng H2 bằng thao tác đi tới node và Interact thật, có scroll nội dung dài; map/inventory/dialogue đều có ảnh ở4 kích thước, nút Close/body viewport trong bounds. Không có lỗi tràn/cắt nghiêm trọng trong các màn đã xem.

Safe area: inset4% hai bên/dưới,5% trên trên Root hiện có, kiểm/capture Windows simulation; không phải Android notch test. Tap H2 từ fresh save, drag sai/đúng8 mảnh Jeonju/Seoul, H6 hai mảnh, mousewheel/pan/pinch không di chuyển hoặc tự chọn node, HIDDEN không nhận input/path. Giữ InputSystemUIInputModule hiện có; không đổi movement hoặc event logic. Android Back chưa có handler riêng trước task, không tự thêm navigation framework; các nút Back/Close thật được kiểm thử.

## Windows QA

| Run | Kết quả | Assertions PASS |
|---|---|---|
| battle01 | PASS | 76 |
| before01 | FAIL | 34 |
| before02 | PASS | 52 |
| ending01 | PASS | 30 |
| fresh01 | FAIL | 34 |
| fresh02 | PASS | 36 |
| main01 | PASS | 245 |
| main02 | PASS | 267 |
| restart01 | PASS | 11 |
| restore01 | PASS | 22 |

Các fixture chỉ ở `Validation/Phase6-20261019/`, không đọc/ghi save người chơi. Crop assembly fixture reopen assembled/restored flags của bản sao save earned để xem4 mảnh; không được dùng fixture đó để tuyên bố story progression. `restore01` dùng save earned Jeonju trước trial, sai→hint/retry→đúng→xác nhận qua UI thật; restored commit đúng1 lần, không thưởng lặp. Battle fixture reopen một encounter catalog trong private copy ở mỗi vùng, thử run/loss/win, không chứng minh fresh quest progression.

`ending01` dùng ready-unconfirmed fixture Phase5: Cancel không commit, xác nhận rồi chủ động Skip tạo đúng một canonical ending, credits/ack không grant item/coin/quest reward. Không chạy lại cả full story vì backend/quest/graph không đổi. Restart process kiểm flags/6restored/canonical completion và đọc inventory không đổi tiến độ. Audio regression: đúng VN/KR battle/map, chuyển về map sau6 outcomes, mute qua touch, load không phát lại pickup/puzzle,1SoftAudio/2music voices architecture không đổi. Không sáng tác/chỉnh MP3/WAV/loop/crossfade.

`before01` FAIL là lỗi fixture đứng sai node phục dựng nên BeginAssembly từ chối; đã sửa QA location, `before02` PASS trên production trước sửa. `build03` dừng trước thay art vì JsonUtility đọc Rect theo field nội bộ; đã đổi DTO explicit floats, `build04/05/06` thành công; build06 là Windows Player cuối, thay đổi cuối chỉ sửa assertion QA. Giữ log/capture thất bại, không relabel. `main01` PASS nhưng supersize không dùng làm final native evidence và call ShowEvent chéo node chỉ là diagnostic; `main02` thay bằng đi tới H2 thật. `fresh01` FAIL do assertion QA so portrait map npc_mai thay vì binding legacy/variant của event H2; đường touch đã tới đúng H2 và Cô Mai hiển thị đúng. Sửa assertion theo event binding, không sửa production; `fresh02` PASS. Build dùng helper Phase5Setup.Build có tên cũ, chỉ BuildPipeline các scene hiện có; không thực hiện lại Phase5 hay art/audio generators.

## Performance và Android

Native pan probe cuối main02:120frames≈0.833s trong Development Player, active objects1059→1059, total allocated native155879648→155879815bytes. Đây chỉ là probe ngắn tại một scene, không phải kết quả FPS/VRAM Android hoặc chứng minh zero GC. Không thấy object phát sinh mỗi frame trong probe; không có lý do viết lại hệ thống. Hai map có mip chain thêm khoảng4MiB GPU ước lượng ởRGBA32 (không phải đo VRAM thiết bị); đổi lấy minification đúng, không giảm chất lượng source.

Known shutdown diagnostics vẫn có ComputeBuffer/URP và D3D12 resource64KiB, native leak≈50.9KiB như trước task. Chưa điều tra nguyên nhân/long-session mobile; không tuyên bố memory leak đã sửa hoặc memory PASS toàn game.

Android SDK có sẵn, `adb devices` không có device. **ANDROID_NOT_TESTED** cho APK/device, pinch lifecycle/tai thỏ/Back thực tế, FPS/thermal/VRAM và nghe loop trên máy thật. Không gọi Windows touch simulation là Android PASS. Bước tiếp: nối thiết bị, build Android từ đúng checkpoint, kiểm4 tỷ lệ/tai thỏ, tap/pinch/drag, pause-resume/audio và một trận/trial trong phiên thiết bị trước phát hành.

## Route và occlusion còn chờ

Không sửa thêm waypoint/graph/arrival.149 geometry mới và6 tuyến giữ nguyên từ approved-route checkpoint đều còn đúng hash. Các điểm khó D-E020, G-E004/G-E006, S-E002/S-E017 và roof/garden approximations đã được chấp nhận best-effort, tiếp tục **NEEDS_USER_REVIEW** theo [route report](../Maps/APPLIED_SIX_ROUTE_REFERENCES_20261019.md). Không vẽ cầu mới hay rollback tuyến.

**OCCLUSION_NEEDS_USER_MARKUP:** không có vùng đỏ chính xác mới trong yêu cầu này. Không đoán mái/cây cần che. H6 đã PASS giữ nguyên; không tách lớp map hay đưa AI background vào game. Chủ dự án có thể ghi EdgeID+vùng đỏ trên bản export hiện hữu cho patch riêng.

## Các điểm dành cho nghiệm thu/Phase7

- VISUAL_PENDING: chủ dự án duyệt ảnh crop/thumbnail và portrait trên máy; không tự đánh giá art hoàn hảo bằng assertions.
- ART_PENDING_USER_APPROVAL:3 icon Seoul nêu trên; chưa tạo art mới.
- NEEDS_USER_REVIEW: terrain residual và occlusion markup; không chặn phần UI/crop an toàn đã hoàn thành.
- Một số instruction/label vẫn lộ ART_PENDING hoặc wording kỹ thuật, ví dụ catalog heritage_data còn nhắc portrait chờ duyệt mặc dù NPC đã chốt. Ghi lại, không tự biên tập story.
- Portrait rất cao có khoảng nền khung trống phía trên khi focus H2/zoom2 do map4:3 và clamp cũ; không overflow hoặc cản input, giữ camera/movement được duyệt, chờ người dùng đánh giá.
- SFX đã test điều khiển và battle transition trên Windows; không tổ chức lại sound architecture hoặc tự gọi chất lượng nghe Android PASS.

## Bằng chứng / checkpoint / điểm dừng

[Mở gallery trước/sau và ảnh native](../QA/Phase6-VisualAndroid/RuntimeReview.html). [QA summary](../QA/Phase6-VisualAndroid/run-summary.json), [source/hash guard](../QA/Phase6-VisualAndroid/source-verification.json), [crop plan](../QA/Phase6-VisualAndroid/crop-outline-plan.json). Mỗi run giữ kết quả/log/private save/provenance; gallery chỉ dùng ảnh runtime mới.

Checkpoint receipt: [checkpoint.json](../QA/Phase6-VisualAndroid/checkpoint.json). Incremental yêu cầu base AppliedSixRouteReferences và chain; payload nhỏ, không copy builds/cache/Library/toàn bộ Validation lịch sử. Không xóa backup/save/log/ảnh lịch sử. **DỪNG sau Phase6; không bắt đầu Phase7.**
