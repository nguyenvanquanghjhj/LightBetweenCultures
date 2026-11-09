"""Bounded, read-only verification and compact closeout of interrupted final polish.
No build, gameplay mutation, historical cleanup or automatic follow-up tasks.
"""
from pathlib import Path
import argparse, datetime, difflib, hashlib, html, json, re, shutil
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT/'Docs/QA/FinalPolish-20261019'
VAL = ROOT/'Validation/FinalPolish-20261019'
REPORT = ROOT/'Docs/QA/FINAL_POLISH_RECOVERY_20261019.md'
DEST = Path('D:/GAMEKKK/Backups/CheckpointFinalPolishRecovery-20261019')
read = lambda p: json.loads(p.read_text(encoding='utf-8-sig'))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
dump = lambda p, x: p.write_text(json.dumps(x, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
rel = lambda p: p.relative_to(ROOT).as_posix()
parser = argparse.ArgumentParser()
parser.add_argument('--checkpoint', action='store_true')
args = parser.parse_args()
b = read(DOC/'baseline.json'); before = Path(b['before']); checks = []

def check(ok, label):
    checks.append(dict(result='PASS' if ok else 'FAIL', check=label))
    assert ok, label

def source(path, old=False):
    return ((before if old else ROOT)/path).read_text(encoding='utf-8-sig')

changed = [e['path'] for e in b['protected'] if not (ROOT/e['path']).exists() or sha(ROOT/e['path']) != e['sha256']]
check(set(changed).issubset(b['editable']), 'Frozen baseline changed only in the nine declared editable files')
check(len(changed) == 9, 'Nine existing files retained, no undo/discard of interrupted work')

p = 'Assets/_Game/Scripts/Maps/CityMap.CulturalRestoration.cs'
old = source(p, True); expected = old
for a, z in [('.70f,1,.76f', '.70f,1,.30f'), ('1,.8f,.65f', '1,.8f,.26f'),
             ('.81f,.35f,.72f', '.81f,.35f,.28f'), ('.81f,.78f', '.81f,.30f'),
             ('.36f,.78f', '.36f,.30f'), ('new Vector2(.38f,.38f),new Vector2(.62f,.62f)', 'new Vector2(.27f,.27f),new Vector2(.73f,.73f)'),
             ('new Vector2(14,14)', 'new Vector2(10,10)'), ('new Color(.8f,.92f,1,1)', 'new Color(.8f,.92f,1,.60f)'),
             ('map.rect.width*.23f,map.rect.height*.25f', 'map.rect.width*.12f,map.rect.height*.13f')]:
    expected = expected.replace(a, z)
check(expected == source(p), 'CulturalRestoration diff consists exclusively of halo alpha/size and child highlight placement')
p = 'Assets/_Game/Scripts/Maps/CityMap.RelicLight.cs'
old = source(p, True); expected = old
for a, z in [('1,.72f,.26f,.62f', '1,.72f,.26f,.26f'),
             ('new Vector2(.40f,.40f),new Vector2(.60f,.60f)', 'new Vector2(.30f,.30f),new Vector2(.70f,.70f)'),
             ('map.rect.width*.13f,map.rect.height*.14f', 'map.rect.width*.065f,map.rect.height*.07f')]:
    expected = expected.replace(a, z)
check(expected == source(p), 'RelicLight diff consists exclusively of decorative halo alpha/size and lantern placement')
check(old.split('sealed class HoiAnLightingRules', 1)[1] == source(p).split('sealed class HoiAnLightingRules', 1)[1], 'Hoi An A/B/C dim-mask, source states and animation logic byte-equivalent after newline normalization')
p = 'Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs'
check(source(p, True).split('public sealed class LeisureSpriteLayer', 1)[0] == source(p).split('public sealed class LeisureSpriteLayer', 1)[0], 'Approved swing and boat setup/animation implementations preserved')
check('public float VerticalScale=1;' in source(p), 'Shared sprite mesh defaults preserve existing pilot geometry')
p = 'ProjectSettings/ProjectSettings.asset'
pattern = r'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
check(re.sub(pattern, '', source(p, True)) == re.sub(pattern, '', source(p)), 'ProjectSettings changes only Unity-generated empty Android icon slots, no unrelated setting changes')
check('m_Textures: []' in source(p), 'Generated icon slots contain no newly assigned art')
p = 'Assets/_Game/Resources/Content/StoryInteractions.json'
a = json.loads(source(p, True)); c = json.loads(source(p)); text_changes = []
check(len(a['entries']) == len(c['entries']) == 22, 'Same 22 existing story-interaction entries')
for x, y in zip(a['entries'], c['entries']):
    check({k:v for k,v in x.items() if k not in ('introText','successText')} == {k:v for k,v in y.items() if k not in ('introText','successText')}, 'Story mechanic/ID/condition/reward unchanged: '+x['id'])
    text_changes += [(x['id'], k) for k in ('introText','successText') if x.get(k) != y.get(k)]

r = VAL/'recovery-tea-snack01'; restart = VAL/'recovery-restart01'
for folder in (r, restart):
    result = (folder/'smoke-results.txt').read_text(encoding='utf-8-sig')
    check('ALL CHECKS PASSED' in result and not any(s.startswith('FAIL:') for s in result.splitlines()), 'Completed Windows Player run: '+folder.name)
    check(read(folder/'build-provenance.json')['assemblySHA256'].lower() == sha(ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'), 'Used current build02 assembly: '+folder.name)
frames = read(r/'frames.json')
check(len(frames) == 20, 'Exactly twenty fresh map/menu/motion PNGs for the two activities')
contacts = []
for f in frames:
    with Image.open(r/f['file']) as im:
        check(im.size == (f['width'], f['height']), 'Actual native capture dimensions: '+f['file'])
    if f['kind'] == 'motion' and ((f['node']=='J15' and abs(f['elapsed']-2.4)<.15) or (f['node']=='S1' and abs(f['elapsed']-.6)<.15)):
        contacts.append(dict(file=f['file'], elapsed=f['elapsed'], mouthDistancePixels=f['mouthDistancePixels']))
check(len(contacts)==4 and all(f['mouthDistancePixels']<2 for f in contacts), 'Both resolutions: visible cup rim/food reaches beak within two screen pixels')
counts = {folder.name:sum(x.startswith('PASS:') for x in (folder/'smoke-results.txt').read_text(encoding='utf-8-sig').splitlines()) for folder in (r,restart)}

matrix = read(DOC/'content-matrix.json')
for e in matrix:
    e['QA']='NOT_TESTED_CURRENT_CLOSEOUT_OWNER_LIMITED_TO_TEA_AND_SNACK'
dump(DOC/'content-matrix.json', matrix)
dump(DOC/'recovery-preservation.json', dict(result='PASS', unchangedFrozenFiles=len(b['protected'])-len(changed), scopedBaselineFiles=len(b['protected']), changedExistingFiles=changed, repairedDisplayStrings=len(text_changes), checks=checks))
dump(DOC/'recovery-runtime.json', dict(result='TARGETED_WINDOWS_PASS', releaseSignoff='NOT_READY_PENDING_UNRUN_FULL_QA', assertions=counts, frames=frames, peakContacts=contacts, android='NOT_TESTED', glowAfter='NOT_TESTED', storyRegression='NOT_TESTED', note='Source scope checks are not runtime gameplay or visual acceptance. No new build was launched during recovery.'))

diff = []
for p in changed:
    diff.extend(difflib.unified_diff(source(p, True).splitlines(True), source(p).splitlines(True), fromfile='before/'+p, tofile='live/'+p))
(DOC/'recovered-source.diff').write_text(''.join(diff), encoding='utf-8')

# Only current task evidence. Earlier captures are explicitly labelled before,
# never passed off as proof of the recovered build's output.
style = '<style>body{font:17px system-ui;background:#172829;color:#f7edcf;margin:25px}a{color:#9fe3da}img{max-width:100%;height:auto}section{margin:30px 0} .grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(235px,1fr));gap:12px}figure{margin:0;background:#233c3b;padding:10px}figcaption{font-size:14px;padding:8px}h1,h2{color:#ffdc91}</style>'
page = ['<!doctype html><meta charset="utf-8"><title>LBC — Tea / Tteokbokki recovery</title>',style,'<h1>Jeonju & Seoul — kiểm tra sau gián đoạn</h1><p>Windows Player build02. 20 ảnh mới; 540×960 và 720×1280. Không phải mockup. Không triển khai Phase 7.</p>']
page += ['<p>Bàn trà: bỏ shear +0.18; chiều sâu prop ×0.85. Lumi giữ nguyên tỷ lệ. Ly/đồ ăn được đưa tới vị trí mỏ canonical. Đánh giá pose vẫn chờ chủ dự án; không có sprite tay cầm ly chuyên biệt.</p>']
for node, title, peak, previous in [('J15','Thưởng trà', '2.4','2.3'),('S1','Ăn tteokbokki','0.6','0.6')]:
    page += [f'<section><h2>{title} — trước / sau</h2><div class="grid">']
    for folder, stamp, label in [('leisure-polish01', previous, 'TRƯỚC — build01'),('recovery-tea-snack01',peak,'SAU — build02')]:
        name=f'FULL-{node}-720-motion-{stamp}.png'; link=f'{folder}/{name}'
        page += [f'<figure><a href="{link}"><img src="{link}"></a><figcaption>{label} · 720×1280 · t≈{stamp}s. Cùng bố cục/camera; thời điểm chuyển động được ghi riêng.</figcaption></figure>']
    page += ['</div></section>']
    for width in [540,720]:
        page += [f'<section><h2>{title} — 3 thời điểm · {width}px</h2><div class="grid">']
        for f in frames:
            if f['node']==node and f['width']==width and f['kind']=='motion':
                link='recovery-tea-snack01/'+f['file']
                page += [f'<figure><a href="{link}"><img src="{link}"></a><figcaption>{html.escape(f["utc"])}<br>t={f["elapsed"]:.3f}s; khoảng cách contact/mỏ={f["mouthDistancePixels"]:.3f}px</figcaption></figure>']
        page += ['</div></section>']
page += ['<p>PASS thao tác không đồng nghĩa toàn game đã nghiệm thu. Glow sau sửa, full-story regression và Android: NOT_TESTED. DỪNG tại checkpoint.</p>']
(VAL/'gallery.html').write_text('\n'.join(page), encoding='utf-8')

report = f'''# Final polish — chốt phục hồi 2026-10-19

**TARGETED_WINDOWS_PASS. Full release signoff: NOT_READY (QA rộng chưa chạy, không phải kết luận game có lỗi nặng). DỪNG; không triển khai Phase 7.**

## Trạng thái build và nguyên nhân chờ

`Logs/final-polish-build02.log` xác nhận build Succeeded, batchmode exit code 0; log cập nhật cuối 12:08:24 ngày 19/10/2026, UTC+07. Phiên bị ngắt sau khi gọi build nên chưa tiếp tục chạy Player; build không treo. Khi phục hồi lúc khoảng 13:15 không còn Editor build/Player/bee_backend. PID 26752 là dịch vụ Unity Hub (`C:/Program Files/Unity Hub/resources/unity.exe`), giữ nguyên. Không kill tiến trình không liên quan; **không build lại**.

## Đã lưu và kiểm chứng

- Bàn trà J15: bỏ shear +0.18 làm mép hồi sâu bị dốc hơn; vertical prop scale 0.85, tâm y 0.315. Không ép tỷ lệ Lumi theo bàn; chân vẫn theo đệm. Ảnh mới cho thấy mặt bàn thấp/phẳng hơn bản build01.
- Trà: đưa mép ly đến mỏ canonical của Lumi (source PNG pixel 850,606, đã bù Sprite.rect bị trim), giữ ly đủ lớn để nhận biết. Chuyển động lên/xuống vẫn 4.8 giây, không thay gameplay.
- Tteokbokki S1: đưa miếng bánh/chopsticks đến cùng anchor mỏ, không thu nhỏ đồ ăn thành gần như mất hút trước khi tới miệng. Lumi full body cạnh quầy.
- Bốn frame cao điểm tại hai độ phân giải có khoảng cách contact/mỏ <2 screen pixel (thực đo: {', '.join(str(round(f['mouthDistancePixels'],4)) for f in contacts)} px). Đây là kiểm tra căn vị trí; không thay thế nghiệm thu pose bằng mắt.
- Đã xem ảnh Windows Player mới J15-720 lúc 2.4s và S1-540 lúc 0.6s. Ly che phần mỏ tự nhiên khi uống, đồ ăn chạm mỏ; không còn bay bên má. Tay/wing là tween nhẹ từ sprite gốc, không phải bộ pose uống/ăn chuyên dụng. Không tiếp tục chỉnh thẩm mỹ ngoài phạm vi.

## QA thực tế

| Phạm vi | Kết quả | Bằng chứng / giới hạn |
|---|---|---|
| Windows build02 | PASS | Build đã có, không chạy lại |
| J15 và S1: 540×960, 720×1280 | PASS | {counts[r.name]} assertions, 20 PNG mới, 12 motion frames |
| Mở từ node, đúng art, close giữa chừng, replay, reopen | PASS | Pointer/touch simulation của Windows Player; một Canvas/EventSystem |
| Trả về đúng map/node/camera, không lock input | PASS | Save và camera kiểm tra trước/sau từng activity |
| Không tự thưởng, không sửa inventory/progression/history, không restart BGM | PASS | So sánh toàn SaveData / music starts trong từng panel |
| Khởi động Player mới từ save QA sau hai hoạt động | PASS | {counts[restart.name]} assertions; giữ quest/relic/item/ledger, không còn panel/animation/input lock |
| 10 hoạt động leisure khác trên build02 | NOT_TESTED | Không chạy lại theo yêu cầu chốt nhanh. Kết quả trước giữ nguyên, không suy rộng thành PASS build02 |
| 10 story minigames và 12 short interactions | IMPLEMENTED / NOT_TESTED trong lượt chốt | Đã đối chiếu runtime data đủ 22; chưa chạy full regression |
| Hai tuyến truyện / ending / 29 quest toàn game | NOT_TESTED | Không tuyên bố full-story PASS |
| Glow sau chỉnh ở sáu thành phố | NOT_TESTED | Chỉ xác minh diff source + build; chưa chụp bộ after để nghiệm thu |
| Android thiết bị thật | NOT_TESTED | Windows touch simulation không phải Android PASS |

Run mới: `Validation/FinalPolish-20261019/recovery-tea-snack01`, `recovery-restart01`.
Run hợp lệ đã có trước gián đoạn: `leisure-before01` (4 activity baseline); `leisure-polish01` (U8/D13/J13/J15/S1, build01); `maps-before01` (6 city restored-state trước giảm glow). Không dùng các ảnh cũ làm bằng chứng sau sửa. Các log và save cũ giữ nguyên.

## Kiểm tra giảm glow: đúng phạm vi

Nguyên nhân là các UI WarmGlow overlay lớn chồng lên tranh; không sửa ánh sáng đã vẽ trong PNG, không sửa bloom/shader hay marker boss.

| File | Thay đổi đã giữ |
|---|---|
| CityMap.RelicLight.cs | Halo Hội An alpha 0.62→0.26; size 0.13×0.14 map→0.065×0.07. Child lantern từ 20%→40% halo giữ kích thước biểu tượng thực tế. Toàn bộ HoiAnLightingRules/dim mask/state A/B/C và nguồn quest/relic giữ nguyên |
| CityMap.CulturalRestoration.cs | Halo 5 city alpha 0.65–0.78→0.26–0.30; size 0.23×0.25→0.12×0.13 map. Motif giữ gần kích thước cũ; MemoryStar 14→10px, alpha 1→0.60. State trigger, dialogue wait, discovery, fog logic và hai màu nhánh tượng giữ nguyên |

Các kiểm tra diff whitelist khớp toàn bộ file; **source scope PASS, hiệu quả thị giác sau sửa NOT_TESTED**. Không tự đảo các chỉnh sửa này chỉ vì thiếu ảnh QA.

## File production đã thay đổi và được giữ

1. `Assets/_Game/Scripts/UI/GameplayPanels.Leisure.cs`: bàn trà/đưa đồ tới mỏ; giữ các sửa presentation đã lưu trước gián đoạn (full body, arm UV theo trimmed Sprite, fruit mask, dây diều, ngai, quầy).
2. `Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs`: chỉ shared mesh helper thêm ShearY / VerticalScale mặc định 0 / 1. Toàn bộ setup và Pose hai pilot trước class helper giữ nguyên; thuyền đã approve không redesign.
3. `Assets/_Game/Scripts/Maps/CityMap.CulturalRestoration.cs`: halo trang trí như trên.
4. `Assets/_Game/Scripts/Maps/CityMap.RelicLight.cs`: halo Hội An như trên.
5. `Assets/_Game/Resources/Content/StoryInteractions.json`: giữ sửa {len(text_changes)} chuỗi intro/success bị lỗi dấu `?` đã thực hiện trước; IDs, kind, cơ chế, điều kiện và reward không đổi. Runtime regression phần text/story: NOT_TESTED.
6. `ProjectSettings/ProjectSettings.asset`: Unity tự ghi các slot icon Android rỗng trong build; ngoài block ấy file không đổi. Giữ nguyên, không Undo/Discard.

QA/tooling: ba file existing `GameplaySmokeRunner.cs`, `GameplaySmokeRunner.StoryLeisure.cs`, `GameplaySmokeRunner.LeisureFull.cs`; file mới `GameplaySmokeRunner.FinalPolish.cs` (+meta); `Tools/final_polish_validation.ps1`, `Tools/final_polish_recovery_closeout.py`; báo cáo, diff, manifest và gallery của task này. Các runner mới chỉ biên dịch cho Editor/Development, không phải gameplay manager mới.

## Bảo toàn / phần chưa làm

Scoped baseline {len(b['protected'])} file, {len(b['protected'])-len(changed)} file frozen không đổi hash; đúng 9 existing-file delta được giữ. Maps canonical/Lumi PNG, route/arrival/graph, quest manager, inventory, SaveSystem, NPC, relic manager, ending, BGM/SFX không đổi trong baseline liên quan. Không quét hoặc hash toàn project. Không có asset mới hay đổi GUID/import.

Route CLOSED_BY_OWNER — ACCEPTED_WITH_KNOWN_LIMITATIONS: U-E018/U-E022, D-E020/D-E025, J-E026, G-E004/G-E006, S-E002/S-E016/S-E009. Không sửa.

Cleanup: **0 file bị xóa**. Không đủ điều kiện dependency audit sau khi chủ dự án yêu cầu chốt nhanh; giữ production assets, save, backup và lịch sử. Không có BLOCKED kỹ thuật trong hai test mới. QA rộng và glow-after ghi NOT_TESTED vì đã giới hạn phạm vi, không giả thành PASS.

Gallery: `Validation/FinalPolish-20261019/gallery.html`. Native PNG mở trực tiếp, before/after ghi rõ build và timestamp; không mockup/upscale. Đánh giá hình đẹp cuối cùng chờ chủ dự án.

Checkpoint incremental: `D:/GAMEKKK/Backups/CheckpointFinalPolishRecovery-20261019`, phụ thuộc CheckpointSwingStanding-20261019 và base chain trong manifest; không phải full backup độc lập. File `recovery-checkpoint.json` ghi SHA256, dung lượng, danh sách và kiểm tra restore. Không bao gồm Library/Temp/Builds/cache hoặc save người chơi.

**DỪNG tại đây. Phase 7 chưa triển khai.**
'''
REPORT.write_text(report, encoding='utf-8')

# Exact files needed for code restoration and this closeout's linked evidence.
paths = set(changed)
paths.update(['Assets/_Game/Scripts/Validation/GameplaySmokeRunner.FinalPolish.cs', 'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.FinalPolish.cs.meta', 'Tools/final_polish_validation.ps1', 'Tools/final_polish_recovery_closeout.py', rel(REPORT), rel(VAL/'gallery.html'), 'Logs/final-polish-build02.log'])
paths.update(rel(p) for p in DOC.iterdir() if p.is_file() and p.name!='recovery-checkpoint.json')
for folder in [r, restart]: paths.update(rel(p) for p in folder.iterdir() if p.is_file())
for node, stamp in [('J15','2.3'),('S1','0.6')]: paths.add(rel(VAL/'leisure-polish01'/f'FULL-{node}-720-motion-{stamp}.png'))
for folder in ['leisure-before01','leisure-polish01','maps-before01']:
    for name in ['smoke-results.txt','build-provenance.json','input-provenance.json','map-frames.jsonl']:
        p=VAL/folder/name
        if p.exists(): paths.add(rel(p))
entries = [dict(path=p, sha256=sha(ROOT/p), bytes=(ROOT/p).stat().st_size) for p in sorted(paths)]
size = sum(e['bytes'] for e in entries)
print('PASS:', len(checks), 'bounded checks;', counts, '; incremental estimate:', len(entries), 'files,', round(size/1024**2, 2), 'MiB')
if not args.checkpoint: raise SystemExit(0)
assert size < 50*1024**2, 'Unexpected checkpoint growth; inspect first'
assert not DEST.exists() and DEST.parent.resolve()==Path('D:/GAMEKKK/Backups').resolve()
base=Path(b['baseCheckpoint']); bm=read(base/'CHECKPOINT_MANIFEST.json')
chain=[dict(path=str(base), manifestSHA256=b['baseManifestSHA256']), *bm['baseChain']]
for c in chain: assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256'], 'Base manifest mismatch'
for e in bm['entries']: assert sha(base/e['path'])==e['sha256'], 'Immediate base content mismatch'
DEST.mkdir()
for e in entries:
    p=ROOT/e['path']; q=DEST/e['path']; q.parent.mkdir(parents=True, exist_ok=True); shutil.copy2(p,q)
    assert sha(p)==sha(q)==e['sha256'], 'Source changed during incremental snapshot'
guide='''# Incremental — REQUIRES BASE, not standalone

Restore the complete CheckpointSwingStanding-20261019 manifest base chain first,
then overlay these project-relative files. No deleted paths. Do not restore over
newer unrelated work or while Unity/Player is active. The immediate base contents
and all base-chain manifest hashes were verified; every delta was copied and hashed.

Includes current nine-file changes, new development QA/tooling, this report and
all images linked by the closeout gallery. Other historical diagnostic images stay
in the live project; older raw frame lists are not promises of complete history here.
Canonical art, maps and audio are unchanged and reside in the base chain.
No Library, Temp, Builds, caches, global saves or full project duplicate.

To rebuild restored production source, use existing Phase5Setup.Build (helper name
does not authorize a story phase). The completed build02 was reused for this QA.
Only tea/snack and restart were retested; wider story/glow-after/Android NOT_TESTED.
Phase 7 was not implemented. STOP for owner review.
'''
(DEST/'RESTORE.md').write_text(guide, encoding='utf-8')
manifest=dict(type='INCREMENTAL_REQUIRES_BASE', createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(), baseCheckpoint=str(base), baseManifestSHA256=b['baseManifestSHA256'], baseChain=chain, entries=entries, bytes=size, deletedPaths=[], restoreGuideSHA256=sha(DEST/'RESTORE.md'), summary='Interrupted final-polish source retained; tea perspective flattened; cup/food reaches canonical beak; targeted Windows QA and restart PASS; glow scope verified only; full QA and Phase7 not executed.')
dump(DEST/'CHECKPOINT_MANIFEST.json',manifest)
dump(DOC/'recovery-checkpoint.json',dict(checkpoint=str(DEST),manifestSHA256=sha(DEST/'CHECKPOINT_MANIFEST.json'),standalone=False,baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],files=len(entries),bytes=size,copiedFilesVerified=True,baseContentVerified=True,baseChainManifestVerified=True,deletedPaths=[],stop='STOP_WAITING_FOR_OWNER_REVIEW'))
print('CHECKPOINT VERIFIED:', DEST)
