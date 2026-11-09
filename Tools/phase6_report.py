"""Generate the bounded Phase6 acceptance report from this task's own evidence."""
import json,hashlib,html
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/QA/Phase6-VisualAndroid';RUNS=ROOT/'Validation/Phase6-20261019'
read=lambda p:p.read_text(encoding='utf-8-sig')
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
summary=[]
for folder in sorted(RUNS.iterdir()):
 p=folder/'smoke-results.txt'
 if p.exists():
  s=read(p);summary.append(dict(run=folder.name,result='PASS' if s.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL',assertions=sum(l.startswith('PASS:') for l in s.splitlines())))
required=['before02','main02','battle01','restore01','ending01','restart01','fresh02']
assert all(any(e['run']==name and e['result']=='PASS' for e in summary) for name in required),summary
captures=[]
for size in ['540x960','720x1280','1080x1920','1080x2400']:
 for panel in ['map','dialogue','inventory']:
  p=RUNS/'main02'/f'{size}-{panel}.png';w,h=Image.open(p).size
  assert f'{w}x{h}'==size
  captures.append(dict(path=p.relative_to(ROOT).as_posix(),width=w,height=h,sha256=digest(p)))
for folder in RUNS.iterdir():
 p=folder/'player.log'
 if p.exists():
  text=read(p)
  assert 'NullReferenceException' not in text or folder.name=='before01'
(OUT/'run-summary.json').write_text(json.dumps(dict(runs=summary,required=required,nativePortraitRenders=captures,android='ANDROID_NOT_TESTED'),indent=2),encoding='utf-8')

comparisons=[('World Map – hai thumbnail bị nhiễu','WorldMap.png'),('Jeonju – ba mảnh bị dính hình bên cạnh','Assembly-jeonju.png'),('Seoul – khung relay và ô thả','Assembly-seoul.png')]
gallery=['<!doctype html><meta charset="utf-8"><title>LBC Phase6 runtime review</title><style>body{background:#142e34;color:#fff3cf;font:16px sans-serif;margin:24px}img{max-width:100%;max-height:85vh;object-fit:contain}section{margin:32px 0}.pair{display:grid;grid-template-columns:1fr 1fr;gap:20px}a{color:#ffdc8a}figure{margin:8px}figcaption{margin:8px}details{margin:24px 0}</style><h1>Phase6 · runtime Windows mới, 19/10/2026</h1><p>Trước/sau cùng state QA, cùng 540×960 và camera/zoom. PNG canonical nguyên vẹn. Ảnh native portrait mới ở main02; main01 supersize chỉ là chẩn đoán, không dùng để nghiệm thu độ nét. Android chưa test.</p>']
def src(run,name):return '../../../Validation/Phase6-20261019/'+run+'/'+name
used=[]
for title,name in comparisons:
 gallery.append('<section><h2>'+title+'</h2><div class="pair">')
 for run,label in [('before02','TRƯỚC'),('main02','SAU')]:
  gallery.append(f'<figure><a href="{src(run,name)}"><img src="{src(run,name)}"></a><figcaption>{label} · {run}/{name}</figcaption></figure>');used.append('Validation/Phase6-20261019/'+run+'/'+name)
 gallery.append('</div></section>')
for run,name,title in [
 ('main02','Map-hoi_an.png','City Map Việt Nam'),('main02','Map-seoul.png','City Map Hàn Quốc'),
 ('main02','Fragments-jeonju.png','Jeonju – chi tiết di vật'),('main02','Fragments-seoul.png','Seoul – ba icon còn ART_PENDING'),
 ('main02','1080x2400-dialogue.png','Hội thoại Cô Mai – framebuffer native1080×2400'),
 ('main02','Simulated-safe-area-dialogue.png','Safe area mô phỏng trên Windows'),
 ('main02','CheckIn.png','Postcard xem lớn, không ghi lại reward'),('main02','PhotoJournal.png','Sổ ảnh – 18 định nghĩa giữ nguyên'),
 ('main02','Collection.png','Collection – không thay ending state'),
 ('battle01','Battle-hoi_an.png','Battle Việt Nam'),('battle01','Battle-jeonju.png','Battle Hàn Quốc'),
 ('restore01','P4-jeonju-trial-correct-before-confirm.png','Trial Hanji đúng, chưa xác nhận'),
 ('ending01','P5-credits.png','Credits sau chủ động xác nhận/Skip hợp lệ'),('fresh02','Fresh-H2-touch.png','Fresh save – tap H2')]:
 p=RUNS/run/name
 if p.exists():
  gallery.append(f'<details><summary>{html.escape(title)}</summary><a href="{src(run,name)}"><img loading="lazy" src="{src(run,name)}"></a></details>');used.append(p.relative_to(ROOT).as_posix())
for size in ['540x960','720x1280','1080x1920','1080x2400']:
 gallery.append('<details><summary>Native '+size+' · Map / Dialogue / Inventory</summary>')
 for panel in ['map','dialogue','inventory']:
  name=size+'-'+panel+'.png';gallery.append(f'<a href="{src("main02",name)}"><img loading="lazy" src="{src("main02",name)}"></a>');used.append('Validation/Phase6-20261019/main02/'+name)
 gallery.append('</details>')
(OUT/'RuntimeReview.html').write_text('\n'.join(gallery),encoding='utf-8')
(OUT/'acceptance-images.json').write_text(json.dumps(sorted(set(used)),indent=2),encoding='utf-8')
rows='\n'.join(f"| {e['run']} | {e['result']} | {e['assertions']} |" for e in summary)
report='''# Phase6 — Visual / Android presentation polish

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
| fragment_seoul_relay_frame | -1596328452 (xem native catalog nếu khác) | 890,4,360,425 | 49 điểm | PASS: bỏ vệt cạnh trái |

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
'''+rows+'''

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
'''
# Use exact localID from the authoritative unchanged native catalog, never a guessed number.
native=json.loads(read(ROOT/'Docs/Art/native-art-bindings.json'))['entries']
relay=next(e for e in native if e['id']=='fragment_seoul_relay_frame' and e['role']=='relic_fragment')
report=report.replace('-1596328452 (xem native catalog nếu khác)',str(relay['localId']))
(ROOT/'Docs/Gameplay/PHASE6_VISUAL_ANDROID_POLISH_REPORT.md').write_text(report,encoding='utf-8')
print('Report/gallery generated;',sum(e['assertions'] for e in summary),'PASS assertions across retained runs; native portrait files=',len(captures))
