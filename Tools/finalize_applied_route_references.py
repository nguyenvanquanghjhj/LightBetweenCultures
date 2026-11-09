"""Summarize the bounded route patch and optionally checkpoint its exact payload."""
import argparse, csv, hashlib, html, json, shutil, subprocess, sys
from datetime import datetime, timezone
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/Maps/AppliedRouteReferences-20261019'
V=ROOT/'Validation/AppliedRouteReferences-20261019'
TARGET=Path('D:/GAMEKKK/Backups/CheckpointAppliedSixRouteReferences-20261019')
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def write(p,s):p.write_text(s,encoding='utf-8')
names={'hoi_an':'HoiAn','hue':'Hue','da_lat':'DaLat','jeonju':'Jeonju','gyeongju':'Gyeongju','seoul':'Seoul'}
required=['before01','after01','fresh02','refine01',*['restart-'+c+('-02' if c=='hoi_an' else '-01') for c in names]]
records=[]
for folder in sorted(p for p in V.iterdir() if p.is_dir()):
    path=folder/'smoke-results.txt'
    if not path.exists():continue
    text=path.read_text(encoding='utf-8-sig')
    records.append(dict(run=folder.name,result='PASS' if text.splitlines()[-1]=='ALL CHECKS PASSED' else 'FAIL',passed=text.count('PASS:'),path=path.relative_to(ROOT).as_posix()))
for run in required:
    assert any(x['run']==run and x['result']=='PASS' for x in records),'Required run pending/failed: '+run
    text=(V/run/'player.log').read_text(encoding='utf-8-sig')
    assert 'Exception:' not in text and 'GAMEPLAY FAIL' not in text,run
traces=load(V/'after01/movement-traces.json');assert len(traces)==164 and all(t['arrived'] and t['maxError']<.025 for t in traces)
for trace in traces:trace['sourceRun']='after01'
export=load(ROOT/'Validation/RouteMarkupExport-20261018/edge_mapping.json')['cities']
refined=[]
for city in names:
    model=next(c for c in export if c['cityId']==city)
    ids={n['runtimeIndex']:n['id'] for n in model['nodes']}
    for t in load(V/'refine01'/f'{city}-route35-traces.json'):
        assert t['arrived'] and t['maxDistance']<.025
        refined.append(dict(cityId=city,fromId=ids[t['from']],toId=ids[t['to']],polyline=t['route'],positions=t['positions'],times=t['times'],arrived=t['arrived'],maxError=t['maxDistance'],travel=t['travel'],sourceRun='refine01'))
assert len(refined)==22
all_traces=traces+refined
rows=load(DOC/'edge-diff.json')
current_traces=[]
for row in rows:
    candidates=[t for t in all_traces if t['cityId']==row['City'] and {t['fromId'],t['toId']}=={row['FromNode'],row['ToNode']}]
    model=next(c for c in export if c['cityId']==row['City']);nodes={n['id']:n for n in model['nodes']}
    expected=[nodes[row['FromNode']]['arrivalUnity'],*row['NewWaypoints'],nodes[row['ToNode']]['arrivalUnity']]
    cases=[]
    for trace in candidates:
        route=expected if trace['fromId']==row['FromNode'] else list(reversed(expected))
        if len(route)==len(trace['polyline']) and all(abs(a[k]-b[k])<1e-6 for a,b in zip(route,trace['polyline']) for k in ('x','y')):cases.append(trace)
    assert cases,row['EdgeID']
    current_traces.extend(cases)
    row['QA']='MOTION_PASS_BOTH_DIRECTIONS' if len(cases)>1 else 'MOTION_PASS_FORWARD'
    row['TerrainAcceptance']='BEST_EFFORT_OWNER_VISUAL_REVIEW'
dump(DOC/'edge-diff.json',rows)
with (DOC/'edge-diff.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader()
    for row in rows:w.writerow({k:json.dumps(v,ensure_ascii=False) if isinstance(v,list) else v for k,v in row.items()})
subprocess.run([sys.executable,str(ROOT/'Tools/verify_applied_route_references.py')],check=True,cwd=ROOT)
verification=load(DOC/'verification.json')
dump(DOC/'runtime-summary.json',dict(requiredRuns=required,runsIncludingHistoricalHarnessFailure=records,totalExecutedTraversals=len(all_traces),finalSourceMatchingTraversals=len(current_traces),uniqueEdges=155,maxMapPixelError=max(t['maxError'] for t in current_traces),terrain='BEST_EFFORT_NOT_PERFECT_TERRAIN_PASS'))
dump(DOC/'final-source-matching-traces.json',current_traces)
stats='\n'.join(f"| {cid} | {sum(r['City']==cid for r in rows)} | {sum(r['City']==cid and r['Status']=='CHANGED' for r in rows)} | {sum(t['cityId']==cid for t in current_traces)} | [Ảnh sau](../../Validation/AppliedRouteReferences-20261019/{name}_After_Routes.png) |" for cid,name in names.items())
kept='\n'.join(f"| {r['City']} | {r['EdgeID']} | {r['FromNode']} → {r['ToNode']} | {r['Reason']} |" for r in rows if r['Status']=='UNCHANGED')
limits='\n'.join(f"| {r['City']} | {r['EdgeID']} | {r['FromNode']} → {r['ToNode']} | {r['Reason']} |" for r in rows if r['Reason'].startswith('APPROXIMATION'))
run_table='\n'.join(f"| [{x['run']}](../../{x['path']}) | {x['result']} | {x['passed']} |" for x in records)
report=f'''# Apply six approved route references — waypoint patch

19/10/2026 · **IMPLEMENTED / WINDOWS_TARGETED_PASS / BEST_EFFORT_VISUAL_REVIEW / STOP**.

Người dùng đã chính thức chấp nhận sáu ảnh AI, gồm sai nhãn và các đoạn địa hình còn thiếu, cho phép tự đối chiếu best-effort thay cho PRELIMINARY_STOP. Đây là bản triển khai waypoint thật, không phải audit-only. Phê duyệt này không cho thay nền, node, arrival, graph, art, gameplay hoặc audio.

## Kết quả thực tế

**149/155 edge đã đổi waypoints; 6 edge giữ nguyên có lý do. 133 node/155 kết nối nguyên vẹn.** Source runtime thay tại `Assets/_Game/Resources/Content/GameplayContent.json`, và chỉ trường `cities[].edges[].waypoints` khác baseline mới nhất sau Phase5. Không thay status/requiredFlags/travel của edge.

| City | Edge hiện có | Geometry đổi | Lượt Player thực | Overlay canonical |
|---|---:|---:|---:|---|
{stats}

[CSV từng edge trước/sau](AppliedRouteReferences-20261019/edge-diff.csv) · [JSON](AppliedRouteReferences-20261019/edge-diff.json) · [So sánh 6 ảnh](../../Validation/AppliedRouteReferences-20261019/BeforeAfter.html) · [Chọn riêng Edge ID/W0…Wn](../../Validation/AppliedRouteReferences-20261019/RouteReview.html) · [Runtime frames](../../Validation/AppliedRouteReferences-20261019/RuntimeReview.html).

## Cách áp dụng

Đã xem đủ sáu reference và sáu nền canonical ở 1448×1086, đối chiếu đường phố/cầu/bậc thang/kiến trúc cùng actual arrival. Tác giả dựng các hành lang theo landmark trên nền canonical, tái sử dụng đoạn chung, rồi gắn từng edge thật vào hai đầu cố định. Offline drafting network trong `Tools/apply_approved_route_references.py` chỉ phục vụ sinh dữ liệu, **không thêm graph/pathfinder runtime**. Nhãn AI không dùng để đổi ID/node. Graph thật và mapping 155 Edge ID cũ quyết định danh tính cạnh.

Hội An đi theo phố Cô Mai, hẻm, bờ kè, cầu nhỏ và vòng chợ; Huế theo bờ sông/cầu/cổng thành; Đà Lạt theo vòng hồ, phố nhà thờ, vườn và đường biệt thự phía bắc; Jeonju theo phố hanok/cầu kênh/sân thủ công; Gyeongju theo đường di tích/gò mộ/cầu; Seoul theo phố cung điện, công viên, bờ sông và đường lên đồi. Một số edge dài dùng chung hành lang vì graph không được thêm/xóa cạnh. Đi qua một vị trí vẽ của node khác không tạo thêm adjacency hay sự kiện mới; event vẫn xử lý theo graph hiện hành.

Mọi bends đều hữu hạn và trong 0..1. Điểm đầu/cuối do `CityRouteGeometry.Arrival` hiện có cấp; không đổi anchor. Dots và Lumi tiếp tục lấy cùng `CityRouteGeometry.Points`, không có đường trang trí riêng hoặc renderer mới. Speed190/camera/occlusion/collider nguyên vẹn. Khi QA, timeScale được tăng chỉ trong harness để rút thời gian chờ; source movement speed không đổi.

## Sáu edge giữ nguyên

| City | EdgeID | Cạnh | Lý do |
|---|---|---|---|
{kept}

Không biến ba tuyến đò thành walk hoặc tạo cầu/vận chuyển mới. Ba tuyến đi qua phần Chùa Cầu giữ nguyên toàn bộ bends để bảo vệ entry/floor/exit và roof occlusion; coveredWalkways cũng byte/semantic exact.

## Các đoạn không thể bám sát hoàn toàn

| City | EdgeID | Cạnh | Hạn chế cụ thể |
|---|---|---|---|
{limits}

Ngoài các đoạn nước/dốc nêu trên, đầu/cuối canonical có thể khác vị trí ký hiệu AI; không dịch anchor để khớp nhãn. Huế U1–U2/U2–U14, Jeonju J0–J15/J2–J12/J4–J7, Gyeongju G0–G16 và Seoul S19–S20 còn các đoạn sát mái/tường hoặc đoạn nối cuối vào anchor không thể mô phỏng chiều sâu chỉ bằng waypoint. Đà Lạt D0–D12/D5–D11/D12–D10 dùng hành lang vòng nối khu vườn phía tây; không khẳng định mọi bậc/cầu phụ hiện rõ liên tục trên tranh. Một số đoạn gấp khúc còn thấy ở góc phố/vùng nhiều node. Chưa thêm occlusion hay sửa đồ họa để che các hạn chế đó.

Đây là những sai lệch được giữ theo quyết định best-effort, **không là bằng chứng địa hình hoàn hảo hoặc lý do dừng toàn bộ bản vá**. Các edge vẫn di chuyển được và được giao ảnh/data để chủ dự án trực tiếp đánh giá. Không dùng motion PASS để gọi tất cả nước/roof PASS.

## QA Windows

- Windows build final06 **Succeeded**, dùng builder Windows sẵn có (`Phase5Setup.Build` là tên helper cũ, không chạy task Phase5). Build03 chạy toàn bộ geometry ban đầu; build04–05 chỉ sửa QA scene. Kiểm tra hình học cuối bỏ spur quay đầu trên 11 edge, build06 kiểm lại đúng 11 edge hai chiều bằng helper Route35 hiện có. [Log build06](../../Logs/approved-routes-build06.log).
- **186 lượt thực qua CityMap.Move**: 164 lượt bao phủ **155/155 edge** ở bản đầu, sau đó 22 lượt kiểm lại 11 edge tinh chỉnh hai chiều. **{len(current_traces)} traces khớp chính xác source cuối**, bao phủ đủ155 edge; 11 traces cũ trước cleanup không được dùng gọi geometry cuối PASS. Tất cả arrived đúng node, max sai số theo map RectTransform = {max(t['maxError'] for t in current_traces):.9f} local pixels. [Trace khớp source cuối](AppliedRouteReferences-20261019/final-source-matching-traces.json).
- Runtime fixture có khai báo: dùng bản sao earned old full-story save, isolate một cạnh trong bộ nhớ để bắt controller đi đúng cạnh cần test, đặt vị trí đầu test rồi phục hồi exact save trước/sau mỗi thành phố/toàn run. Không gọi nó fresh story hoặc chứng minh lại toàn quest graph. Không sửa save cá nhân và không tiêm flag quest/gate.
- H6 roof entry/covered/exit có frames thật và assertion hide/reveal; dot mesh tiếp tục bỏ vùng cover và không raycast. All edge render samples thuộc open/discovered geometry, không sửa save.
- Fresh02: vào Hội An từ World Map bằng thao tác thật; HIDDEN không có input/path, chỉ vùng đầu hiện; render không làm đổi progress. Không chạy lại main story để mở từng gate vì toàn bộ gate/conditions/progression code exact.
- Sáu Player process restart riêng: mỗi fixture chọn lại flat cityId/node/visited từ cityProgress earned tương ứng, không thêm flags/items/quests/rewards. Restart giữ arrival và progress, không reward/ceremony tự phát; Map/World UI đi lại hoạt động. Đây là location fixtures cho persistence, không phải sáu fresh story runs.
- Sáu cặp runtime overview dùng cùng 1280×960, zoom1, focus(0.5,0.5), node0, cùng input save: before01 so với refine01 của build cuối. Native offline before/after cùng nền/scale/nhãn, chứa đủ mọi edge. Movement frames build03 có ghi rõ giai đoạn trước cleanup; không giả là ảnh build06. Fresh/restarts build04–05 vẫn có giá trị cho state/gate vì build06 chỉ bỏ spur ở 11 polyline, không đổi save/arrival/conditions. Refine01 còn re-entry/snapshot sáu thành phố ở build cuối.
- **{len(verification['checks'])} bounded checks / 119 protected files exact**, gồm sáu PNG nền và .meta, HoiAnData, route/movement/discovery/save code, NPC/art bindings, 18 check-in/H6, Phase4/relic, Phase5 ending, BGM/SFX/MP3 metadata. [Verification + 6 hash đầy đủ](AppliedRouteReferences-20261019/verification.json).

| Run | Kết quả | Assertions PASS |
|---|---|---:|
{run_table}

Fresh01 và restart-hoi_an-01 FAIL do harness đọc Map/renderer khi scene vẫn World Map (NullReferenceException trong QA); sửa duy nhất flow QA vào CityMap qua World Map trước. Fresh02 và restart-hoi_an-02 PASS. Giữ nguyên failure log/ảnh/save; không đổi FAIL cũ thành PASS. Các accepted run không có managed exception/GAMEPLAY FAIL. D3D12/Unity shutdown ComputeBuffer warning có thể vẫn xuất hiện; không dùng task geometry để gọi memory/performance PASS.

**ANDROID_BUILD_NOT_TESTED · ANDROID_DEVICE_NOT_TESTED**. Không chạy lại 29 quest/toàn cốt truyện, không tuyên bố mọi quest hoặc terrain toàn game đã nghiệm thu lại. Bảo toàn gameplay dựa trên semantic comparison toàn bộ non-waypoint data, protected source và targeted movement/discovery/persistence checks.

## Files và tọa độ

Production: duy nhất `Assets/_Game/Resources/Content/GameplayContent.json` (waypoints). Development/Editor QA: thêm `GameplaySmokeRunner.ApprovedRoutes.cs` + meta, một dòng dispatch trong `GameplaySmokeRunner.cs`; không manager/Canvas/EventSystem mới. Tools author/export/verify/launcher/suite/finalize nằm trong `Tools/`; tài liệu/bằng chứng ở `Docs/Maps/AppliedRouteReferences-20261019` và `Validation/AppliedRouteReferences-20261019`. [Changed files](AppliedRouteReferences-20261019/changed-files.json).

CSV/JSON `OldWaypoints`/`NewWaypoints` là Unity normalized0..1, gốc trái dưới, chỉ bends. `OldPolylineMap1000`/`NewPolylineMap1000` gồm cả arrival W0/Wn, gốc trái trên: X=1000u, Y=1000(1-v). Native PNG px=1.448X, py=1.086Y. Không crop/warp/rotate, không lấy pixel AI áp thẳng. 155 Edge ID registry cũ không thay và không thêm vào gameplay. Sáu after PNG là composite kỹ thuật riêng ngoài Assets; source canonical PNG không ghi đè.

## Checkpoint và điểm dừng

Base: `D:\\GAMEKKK\\Backups\\CheckpointPhase5Final-20261019` — bao gồm ending mới cùng mọi thay đổi Phase4/NPC/lighting/audio đã có, không rollback. Pre-edit scoped snapshot `BeforeAppliedRouteReferences-20261019` giữ ba file, không là backup standalone. Final incremental `CheckpointAppliedSixRouteReferences-20261019`, [receipt](AppliedRouteReferences-20261019/checkpoint.json), cần base và chain đã ghi; không copy Library/Temp/Builds, không xóa save/backup/evidence cũ.

**DỪNG sau bản waypoint/QA/checkpoint này. Không Phase5/6/7 hoặc task khác.** Chờ chủ dự án xem sáu route mới.
'''
write(ROOT/'Docs/Maps/APPLIED_SIX_ROUTE_REFERENCES_20261019.md',report)

figures=[]
for cid,name in names.items():
    figures.append(f'<section><h2>{cid} — geometry overlay (offline)</h2><div class="pair"><figure><a href="{name}_Before_Routes.png"><img loading="lazy" src="{name}_Before_Routes.png"></a><figcaption>Before — canonical native1448×1086</figcaption></figure><figure><a href="{name}_After_Routes.png"><img loading="lazy" src="{name}_After_Routes.png"></a><figcaption>After — source waypoint thật</figcaption></figure></div>')
    figures.append(f'<h3>Windows runtime — cùng camera, zoom1, node0, 1280×960</h3><div class="pair"><figure><img loading="lazy" src="before01/Before-{cid}-overview.png"><figcaption>Before — build cũ của task, input save giống nhau</figcaption></figure><figure><img loading="lazy" src="refine01/After-{cid}-overview.png"><figcaption>AFTER — build06 geometry cuối</figcaption></figure></div>')
    shots=sorted((V/'after01').glob('Move-'+cid+'-*.png'))
    figures.append('<h3>Frames Lumi đang đi — build03 trước cleanup spur; geometry cuối có trace kiểm riêng trong report</h3><div class="frames">')
    for p in shots:
        assert p.exists();stamp=next(l for l in (V/'after01/capture-manifest.txt').read_text(encoding='utf-8-sig').splitlines() if ' | '+p.name+' | ' in l)
        figures.append(f'<figure><a href="after01/{p.name}"><img loading="lazy" src="after01/{p.name}"></a><figcaption>{html.escape(stamp)}</figcaption></figure>')
    figures.append('</div></section>')
figures.append('<h2>H6 occlusion — entry/covered/exit</h2><div class="frames">')
for label in ['entry','covered','exit']:
    p=V/'after01'/f'H6-{label}.png';assert p.exists()
    figures.append(f'<figure><img loading="lazy" src="after01/{p.name}"><figcaption>{label}</figcaption></figure>')
figures.append('</div>')
write(V/'RuntimeReview.html','<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>LBC approved routes runtime review</title><style>body{font:16px system-ui;background:#132c35;color:#fae6c2;margin:20px}.pair{display:grid;grid-template-columns:1fr 1fr;gap:10px}.frames{display:grid;grid-template-columns:repeat(3,1fr);gap:8px}figure{margin:0}img{width:100%}figcaption{font-size:12px;overflow-wrap:anywhere}a{color:#9adeec}section{margin-bottom:40px}</style><h1>149 changed edges — six approved route references</h1><p>19/10/2026. Offline canonical overlays are identified separately from Windows Player screenshots. Movement tests confirm polyline motion, not perfect terrain conformity. Owner visual review pending. No AI image imported into runtime.</p><p><a href="../../Docs/Maps/APPLIED_SIX_ROUTE_REFERENCES_20261019.md">Report</a> · <a href="RouteReview.html">Inspect one Edge ID</a></p>'+''.join(figures)+'</html>')

baseline=load(DOC/'baseline.json');before=Path(baseline['before'])
before_manifest=before/'CHECKPOINT_MANIFEST.json'
if not before_manifest.exists():
    for e in baseline['entries']:assert sha(before/e['path'])==e['sha256']
    dump(before_manifest,dict(type='SCOPED_PRE_EDIT_NOT_STANDALONE',baseCheckpoint=baseline['baseCheckpoint'],baseManifestSHA256=baseline['baseManifestSHA256'],entries=baseline['entries'],deletedPaths=[]))
modified=['Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md']
added=['Assets/_Game/Scripts/Validation/GameplaySmokeRunner.ApprovedRoutes.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.ApprovedRoutes.cs.meta',
 'Tools/apply_approved_route_references.py','Tools/export_applied_route_references.py','Tools/verify_applied_route_references.py',
 'Tools/validate_applied_route_references.ps1','Tools/applied_route_reference_suite.ps1','Tools/finalize_applied_route_references.py','Docs/Maps/APPLIED_SIX_ROUTE_REFERENCES_20261019.md']
dump(DOC/'changed-files.json',dict(modified=modified,added=added,deleted=[],productionScope='Only existing edge.waypoints; QA code is conditional DEVELOPMENT_BUILD/UNITY_EDITOR. Exact supporting docs/evidence listed in checkpoint manifest.'))
master=ROOT/'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md';text=master.read_text(encoding='utf-8-sig')
prefix='''> **APPROVED SIX ROUTE REFERENCES —19/10/2026: IMPLEMENTED / WINDOWS_TARGETED_PASS / BEST_EFFORT_VISUAL_REVIEW / STOP.** 149/155 existing edges changed waypoints; 6 retained for covered ChuaCau/ferry preservation. 133 nodes/155 graph links/arrivals/quest/gate/save/art/lighting/NPC/Phase4/Phase5/audio intact. Windows186 traversals, including targeted retest11edges after spur cleanup; final source traces cover155edges. Fresh HIDDEN +six process restarts PASS. Terrain imperfections explicitly accepted as best-effort, not declared fully correct. [Report](../Maps/APPLIED_SIX_ROUTE_REFERENCES_20261019.md) · [Six overlays/runtime](../../Validation/AppliedRouteReferences-20261019/RuntimeReview.html) · [Receipt](../Maps/AppliedRouteReferences-20261019/checkpoint.json). CheckpointAppliedSixRouteReferences-20261019 requires CheckpointPhase5Final-20261019. No new Phase5/6/7 work.

'''
if not text.startswith('> **APPROVED SIX ROUTE REFERENCES'):write(master,prefix+text)

payload=set(modified+added)
payload.update(p.relative_to(ROOT).as_posix() for p in DOC.rglob('*') if p.is_file() and p.name!='checkpoint.json')
# Retain all task evidence in the live folder. Copy only representative images
# required by the gallery, rather than duplicating incidental WorldMap captures.
payload.update(p.relative_to(ROOT).as_posix() for p in V.iterdir() if p.is_file())
for p in V.rglob('*'):
    if p.is_file() and p.suffix in ('.json','.log','.txt','.md'):
        payload.add(p.relative_to(ROOT).as_posix())
import re
gallery=(V/'RuntimeReview.html').read_text(encoding='utf-8-sig')
for name in re.findall(r'src="([^\"]+\.png)"',gallery):payload.add((V/name).relative_to(ROOT).as_posix())
for run in required:
    payload.update(p.relative_to(ROOT).as_posix() for p in (V/run).glob('Restart-map.png'))
payload.add((V/'fresh02/Fresh-HIDDEN.png').relative_to(ROOT).as_posix())
payload.update('Logs/approved-routes-build'+str(i).zfill(2)+'.log' for i in range(1,7))
for name in payload:assert (ROOT/name).is_file(),name
total=sum((ROOT/name).stat().st_size for name in payload)
print('Planned incremental:',len(payload),'files,',round(total/1048576,2),'MiB; no builds/caches/history outside this bounded task.')
parser=argparse.ArgumentParser();parser.add_argument('--checkpoint',action='store_true');args=parser.parse_args()
if not args.checkpoint:raise SystemExit(0)
assert not TARGET.exists(),'Refuse checkpoint overwrite'
base=Path(baseline['baseCheckpoint']);basepath=base/'CHECKPOINT_MANIFEST.json'
assert sha(basepath)==baseline['baseManifestSHA256'];m=load(basepath)
for e in m['entries']:assert sha(base/e['path'])==e['sha256'],e['path']
chain=[dict(path=str(base),manifestSHA256=sha(basepath)),*m.get('baseChain',[])]
for c in chain:assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256']
assert TARGET.resolve().parent==Path('D:/GAMEKKK/Backups').resolve();TARGET.mkdir()
entries=[]
for name in sorted(payload):
    a=ROOT/name;b=TARGET/name;b.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(a,b)
    value=sha(a);assert sha(b)==value;entries.append(dict(path=name,sha256=value,bytes=a.stat().st_size))
guide='''# Incremental waypoint patch restore

NOT a standalone backup. Requires CheckpointPhase5Final-20261019 and every base
listed in baseChain. Phase5 retains Phase4, NPC/lighting, SoftSfx/FourTrackMusic
and the prior reference-review history. Never replace the project with this folder.

Close Unity/Player. Preserve newer work. Recover the verified standalone base into
a separate recovery project, apply its required incremental chain base-to-child,
verify each manifest hash, then overlay only this manifest's entries by relative
path. Verify all copied hashes. deletedPaths is empty. Keep paired .meta GUIDs.
Library/Temp/Builds are excluded; build with Unity6000.6.5f1 using the existing
Windows build helper. No images from References are runtime assets.

Evidence includes all task run logs/results/private QA JSON/provenance, all native
before/after overlays and representative screenshots referenced by RuntimeReview,
plus fresh/restart captures. Incidental/intermediate WorldMap captures remain
untouched in the live Validation folder, but are not duplicated in this checkpoint.
Capture-manifest history is retained in full even for those un-copied extra frames.

All Validation saves and restart-seeds are PRIVATE QA copies/location fixtures;
NEVER copy them to the user's persistent-data save directory. User/history saves
were untouched. BeforeAppliedRouteReferences-20261019 is a three-file scoped
pre-edit snapshot, not a full rollback project. Do not restore it over newer work.

Docs/Maps/AppliedRouteReferences-20261019/checkpoint.json is an external receipt,
excluded from entries to avoid a self-referential manifest hash. Recreate from
the manifest if desired. This is documentation only, not a runtime dependency.
'''
write(TARGET/'RESTORE.md',guide)
dump(TARGET/'CHECKPOINT_MANIFEST.json',dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.now(timezone.utc).isoformat(),baseCheckpoint=str(base),baseManifestSHA256=baseline['baseManifestSHA256'],baseChain=chain,summary='149 edge waypoint geometries updated from approved six references; all155 final-source edges verified across186 traversals (incl22 targeted spur-cleanup retests), six restart/fresh hidden PASS. Original art/gameplay/audio/Phase5 intact. Best-effort terrain remains for owner visual review; STOP.',entries=entries,deletedPaths=[],bytes=total,restoreGuideSHA256=sha(TARGET/'RESTORE.md')))
dump(DOC/'checkpoint.json',dict(checkpoint=str(TARGET),manifestSHA256=sha(TARGET/'CHECKPOINT_MANIFEST.json'),baseCheckpoint=str(base),baseManifestSHA256=baseline['baseManifestSHA256'],standalone=False,files=len(entries),bytes=total,status='WINDOWS_TARGETED_PASS / BEST_EFFORT_VISUAL_REVIEW / STOP',android='NOT_TESTED'))
print('Checkpoint verified:',TARGET)
