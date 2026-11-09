"""Closeout report and offline gallery; no runtime or source mutation."""
from pathlib import Path
import json,html,re,hashlib,datetime
from PIL import Image
R=Path(__file__).resolve().parents[1];D=R/'Docs/QA/PlayerExperience-20261019';V=R/'Validation/PlayerExperience-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,j):p.write_text(json.dumps(j,ensure_ascii=False,indent=2),encoding='utf-8')
summaries=[]
for folder in sorted(V.iterdir()):
 if not folder.is_dir():continue
 results=folder/'smoke-results.txt';text=results.read_text(encoding='utf-8',errors='replace') if results.exists() else ''
 status='PASS' if text.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL' if text.rstrip().endswith('FAILED') else 'BLOCKED_OR_STOPPED'
 photos=[]
 for p in sorted(folder.glob('XP-*.png')):
  with Image.open(p) as im:sz=im.size
  photos.append({'path':str(p.relative_to(R)),'width':sz[0],'height':sz[1]})
 logPath=folder/'editor.log';partialLog=logPath.read_text(encoding='utf-8',errors='replace') if logPath.exists() and not results.exists() else ''
 summaries.append({'run':folder.name,'status':status,'checksPassed':len(re.findall(r'^PASS:',text,re.M)), 'partialAssertionsInLog':len(re.findall(r'^GAMEPLAY PASS:',partialLog,re.M)), 'errors':sorted(set(re.findall(r'^ERROR: ([^\n]+)',text,re.M))),
  'photos':photos,'scope':'Unity Play Mode, not Windows Player or Android','results':str(results.relative_to(R)) if results.exists() else None})
dump(D/'run-summary.json',summaries)
blocks={'windowsPlayer':{'status':'BLOCKED','message':'Start-Process: An Application Control policy has blocked this file.','attempt':'before01','policyChanged':False},
 'beforeAfterComparison':{'status':'NOT_AVAILABLE','reason':'No baseline runtime PNG was captured before production presentation changes. No historical screenshot or QA mockup substituted.'},
 'androidDevice':{'status':'NOT_TESTED'},'extendedMainQuest':{'status':'BLOCKED','reason':'Editor-only extended journeys failed to progress with repeated SoftAudio.TickMusic / CityMap.Refresh NullReference; one run followed a hot-reload at the tutorial end. Final short replay also stalled in H3. Root cause and Player regression are not established. Owned processes stopped; source audio/route/gameplay not patched.'}}
dump(D/'runtime-limitations.json',blocks)
new=['Assets/_Game/Scripts/UI/PlayerCopy.cs','Assets/_Game/Scripts/UI/GameplayPanels.PlayerGuide.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.PlayerExperience.cs','Assets/_Game/Scripts/Editor/PlayerExperienceQA.cs']
protection=read(D/'protection-verification.json');existing=protection['changedExistingFiles']
dump(D/'changed-files.json',{'existing':existing,'newScripts':new,'newScriptMetas':[p+'.meta' for p in new if (R/(p+'.meta')).exists()],
 'tools':['Tools/player_experience_patch_20261019.py','Tools/verify_player_experience_20261019.py','Tools/report_player_experience_20261019.py'],
 'notes':'Documentation/evidence are additive under Docs/QA and Validation/PlayerExperience-20261019. Owner route design work is preserved.'})
# Inventory six cities' data copy candidates; only Hoi An narrative was patched.
catalog=read(R/'Assets/_Game/Resources/Content/GameplayContent.json');candidates=[]
fields={'text','offer','reminder','thanks','description','preview','missingText','completedText','confirmationText','alternateText','alternateCompletedText','battleCompletedText','clue'}
for group in ('events','quests','puzzles','items'):
 for entry in catalog[group]:
  for k,v in entry.items():
   if k in fields and isinstance(v,str) and re.search(r'\b(?:[HUDJGS]\d{1,2}|[HUDJGS]Q\d{2}|TODO|TBD|null|undefined)\b|Assets[/\\]',v):
    candidates.append({'group':group,'id':entry['id'],'city':entry.get('cityId','shared'),'field':k,'text':v,'status':'CANDIDATE_REQUIRES_CONTEXT_NOT_AUTOMATICALLY_A_RENDERED_BUG'})
dump(D/'six-city-copy-candidates.json',candidates)
# Only final or explicitly labelled intermediate images; never label as Windows.
chosen=[]
preferred=next((r for r in summaries if r['run']=='final-72001' and r['photos']),None)
if preferred is None:preferred=next((r for r in summaries if r['run']=='after-720-main01' and r['photos']),None)
if preferred:
 for e in preferred['photos']:
  if any(t in Path(e['path']).name for t in ('intro','first-step','mai-before','lumi-promise','objective','mai-return','mai-local','journal','map')):chosen.append((e,preferred['run'],preferred['status']))
earlier=next((r for r in summaries if r['run']=='after-720-main01'),None)
if earlier:
 shown={Path(e['path']).name for e,_,_ in chosen}
 for e in earlier['photos']:
  if Path(e['path']).name not in shown and any(t in Path(e['path']).name for t in ('journal','mai-local','mai-return')):chosen.append((e,earlier['run'],earlier['status']))
leisure=next((r for r in summaries if r['run']=='lantern-editor02'),None)
if leisure:
 for e in leisure['photos']:chosen.append((e,leisure['run'],leisure['status']))
cards=[]
for e,run,status in chosen:
 path=e['path'];href='../../../'+path.replace('\\','/')
 cards.append(f'<article><h2>{html.escape(Path(path).stem)}</h2><p>{html.escape(run)} · {status} · {e["width"]}×{e["height"]}<br>Unity Play Mode capture</p><a href="{href}"><img loading="lazy" src="{href}"></a></article>')
gallery='''<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC — Hội An Player Experience</title><style>body{background:#122b31;color:#f9edce;font:16px/1.5 system-ui;margin:24px}h1{color:#f5cc7e}.notice{border:2px solid #e9b15a;padding:16px;max-width:1100px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(300px,1fr));gap:18px}article{background:#1e3c43;padding:14px;border-radius:12px}article img{width:100%;height:auto}h2{font-size:18px}a{color:#8be5d0}</style>
<h1>Hội An — Player Experience pilot</h1><p class="notice">Ảnh thật từ Unity Play Mode trên Windows Editor, KHÔNG phải Windows Player hoặc thiết bị Android. Windows Player bị Application Control chặn. Chưa có bộ ảnh BEFORE cùng camera; không dùng ảnh cũ/mockup làm bằng chứng. Một số lượt mở rộng bị dừng; xem trạng thái của từng run.</p><p>Đã sửa source, đang chờ nghiệm thu thị giác và playtest người mới. Thả hoa đăng được kiểm tra riêng; không đổi reward hoặc tiến độ.</p><div class="grid">'''+''.join(cards)+'</div></html>'
(D/'gallery.html').write_text(gallery,encoding='utf-8')
runrows='\n'.join(f'| `{s["run"]}` | {s["status"]} | {s["checksPassed"]} | {len(s["photos"])} |' for s in summaries)
filelist='\n'.join('- `'+p+'`' for p in existing+new)
remaining={cid:sum(e['city']==cid for e in candidates) for cid in ['hoi_an','hue','da_lat','jeonju','gyeongju','seoul','shared']}
finalLog=(R/'Logs/player-experience-final-build.log').read_text(encoding='utf-8',errors='replace')
build='PASS' if 'PHASE5 WINDOWS BUILD: Succeeded' in finalLog else 'NOT_VERIFIED'
doc=f'''# PLAYER EXPERIENCE — pilot Hội An, 2026-10-19

**IMPLEMENTED / PARTIAL ACCEPTANCE.** Source UI/presentation đã sửa và Windows build {build}. Vòng tutorial và thả hoa đăng có kết quả Play Mode riêng. **Không tuyên bố Windows Player PASS, Android UX PASS hoặc toàn bộ HQ01 PASS.**

[Gallery ảnh runtime thật](PlayerExperience-20261019/gallery.html) · [QA chi tiết](PlayerExperience-20261019/run-summary.json) · [Giới hạn](PlayerExperience-20261019/runtime-limitations.json) · [Bảo toàn source](PlayerExperience-20261019/protection-verification.json)

## Hiện trạng đã xác định

- NPC có nút "Tiếp theo" nhưng callback có thể cuộn, đổi speaker hoặc đóng. Nút mũi tên phía trên chỉ bỏ qua hiệu ứng portrait. Chúng không diễn đạt đúng tác dụng cho người chơi.
- "Nhận sau / Trở lại" gộp hai ý; callback thật chỉ đóng. Các icon lựa chọn trước đây dựa nhiều vào vị trí đầu/phụ, thay vì hành động.
- "Hội An · Nhật ký khám phá" là nhãn trong viewport City Map khi zoom toàn cảnh, **không phải màn nhật ký bị render thiếu**. Ảnh ngang được fit toàn bộ trong viewport dọc tạo khoảng trống có chủ đích; tên nhãn làm người chơi hiểu nhầm. Chưa quy kết chính xác vòng điều khiển Android từ hình ảnh chưa tái hiện.
- Đọc lore canonical ở FULL_STORY_PLAYTEST.md: Lumi tới Hội An, giúp Cô Mai, các trạm giao lưu mở hai nhánh Việt–Hàn, di vật là phần kết nối cuối. Phần mở đầu mới chỉ tóm tắt các sự kiện này; không đổi ending/lore graph.
- Play Mode tái hiện lỗi thiếu CanvasRenderer ở custom graphic/marker/đạo cụ và lỗi Unity fake-null của CanvasGroup. Sửa yêu cầu component/null-check, không đổi artwork hoặc animation. Đây là lỗi đã chứng minh trong Editor, **chưa chứng minh là nguyên nhân lỗi Android**.

## Đã áp dụng

1. Hộp thoại có **X — Đóng ở góc trên trái**, cùng nút Trở về bản đồ ở cuối. Close dùng đúng flow hiện có, không tự nhận nhiệm vụ, trao đồ hoặc reward. "Nghe Cô Mai"/"Xem lựa chọn" chỉ đổi về nội dung/lựa chọn tiếp theo, không âm thầm đóng hoặc cuộn. Nội dung dài có viewport cuộn riêng.
2. Nhãn action dựa trên chức năng; disabled action có lời giải thích. Ẩn lựa chọn phục dựng đèn chưa có mảnh nào ở UI để tránh gây nhiễu người mới; điều kiện thực thi và graph không đổi. Dialog giao đồ vẫn cần xác nhận, Hủy giữ nguyên vật phẩm.
3. Giới thiệu bốn trang ngắn: Bạn là Lumi; sự cố đèn Hội An; bước đầu tới Cô Mai; chạm bản đồ/hẻm, nhặt đồ, xem túi/nhật ký. Có Bỏ qua và xem lại từ Main Menu/nhật ký. Chỉ dùng portrait Lumi canonical hiện có. Lưu việc đã xem bằng **PlayerPrefs UI riêng**, không thêm flag/schema save gameplay; QA dùng key khác người chơi.
4. Gợi ý đổi thành **Việc cần làm**; nút Xem địa điểm trên bản đồ focus/highlight tuyến đang hợp lệ, không teleport hoặc tự chạy. Target dùng resolver quest hiện có, tôn trọng discovery/pathfinding; không sửa BFS hoặc route.
5. Nhật ký có mục tiêu hiện tại, quest đã nhận/thực trạng item, địa điểm thật đã ghé và giải thích leisure có thể chơi lại. Bỏ bộ đếm leisure gây hiểu nhầm là tiến độ bắt buộc. Hướng dẫn có thể mở lại ở đây.
6. Bỏ node ID ở tooltip. Sửa 17 trường văn bản thật của GameplayContent, cộng lớp hiển thị Hội An đổi các ID còn sót thành tên runtime, "World Map" thành "bản đồ thế giới". ID nội bộ vẫn giữ nguyên. Nhắc thiếu điều kiện dùng tên nhiệm vụ/thử thách, không in raw flag. Cô Mai thay lời theo chưa nhận / đang tìm / có Mắt Đèn / đã trả; sau trả không nhắc tìm lại Mắt Đèn.
7. Toàn cảnh vẫn xem đủ ảnh; mặc định portrait tận dụng chiều cao, giữ đúng tỷ lệ và kéo ngang. Bỏ nhãn "Nhật ký khám phá" gây hiểu nhầm; thêm tên ngắn cho zoom, toàn cảnh và Lumi. "Nhịp" đổi thành menu chọn mức chuyển động có giải thích, giữ preference cũ.
8. H3 giữ điều khiển chạm đất/collider/pathfinding; nút nhặt gọi đúng InteractionSystem cũ với nhãn Nhặt Mắt Đèn. Không tạo/xóa joystick hoặc hệ input thứ hai.

## Bảng nút

| Trước | Sau | Kết quả thật khi bấm |
|---|---|---|
| Mũi tên bỏ hiệu ứng | X — Đóng | Đóng an toàn, không tiến quest |
| Nhận sau / Trở lại | Trở về bản đồ | Callback Close cũ |
| Tiếp theo ở cuối mọi NPC | Chỉ còn Nghe Cô Mai / Xem lựa chọn khi Lumi đang đáp | Chuyển speaker/lựa chọn, không đóng |
| Vào khám phá | Khám phá hẻm · Tìm Mắt Đèn / Ghé lại hẻm | Mở scene H3 cũ đúng trạng thái |
| Trả linh kiện mở đầu | Trao Mắt Đèn cho Cô Mai | Tutorial return cũ; không hoàn tất HQ01 |
| Lắp linh kiện · Thắp sáng | Đồng bộ trạm ánh sáng | Restore H7 cũ; không phục dựng di vật |
| Ghi dấu địa danh | Chụp ảnh · Lưu vào nhật ký; xác nhận Lưu bưu thiếp vào nhật ký | Mở preview, bước xác nhận cũ mới commit |
| Đến thành phố tiếp theo | Xem thành phố trên bản đồ thế giới | Mở World Map, chưa tự chuyển city |
| Gợi ý | Việc cần làm / Xem địa điểm trên bản đồ | Đọc lời nhắc hoặc focus target đang visible |
| Nhịp | Hiệu ứng | Mở lựa chọn bình thường / nhẹ / tắt; không đổi progression |

## Hoa đăng và leisure

Nguồn thật: LeisureActivities.json → `leisure_hoian_lanternrelease_stand`, **H16**, mechanic `release`. Chỉ hiện tương tác khi node đã được discovery mở và event cho phép. Chọn Trải nghiệm · Thả hoa đăng → Bắt đầu → cảnh nền/đạo cụ/Lumi được ghép và animation chạy → Chơi lại hoặc trở về bản đồ.

Lượt `lantern-editor02` kiểm tra lotus thực sự drift, animation kết thúc, replay, đóng giữa animation và input trở lại. Sửa CanvasRenderer cho lớp đạo cụ/vệt nước và renderer tương đương; **không đổi chuyển động, background, sprite hoặc pose đã duyệt**. Không tái thiết kế thuyền vịt.

**Persistence theo thiết kế đang chạy:** animation leisure không cấp reward hoặc thay save; `leisureCompleted` hiện không được các driver animation ghi thêm. Nhật ký cũ 0/12 không phản ánh số lần đã xem animation. Pilot này bỏ cách trình bày gây hiểu nhầm và giữ nguyên semantics không-progression, không tự thêm luật hoàn thành/history mới. Sau mở lại có thể chơi lại; lịch sử hoàn tất lâu dài là phần chưa triển khai nếu chủ dự án muốn. Chưa kết luận lỗi trên điện thoại thật đã hết. Mười một hoạt động khác chỉ rà source/framework, không chạy lại toàn bộ và không gắn PASS dựa trên script tồn tại.

## QA

| Lượt | Trạng thái | Assertion PASS | Ảnh mới |
|---|---|---:|---:|
{runrows}

- `after-editor-logical04`: tutorial fresh dùng touch/mouse giả lập qua EventSystem thật, đóng không accept, nhận quest, H3/collider/nhặt một lần/trả Cô Mai, chỉ local light/tutorial, xem target, nhật ký và reload cuối lượt. Native 540×960.
- `lantern-editor02`: activity thật trên save QA đã chơi qua game; không tiêm unlock, không sửa save người chơi. Đóng/replay và bảo toàn save được kiểm tra.
- 720×1280 có ảnh runtime; hai lượt main mở rộng bị lỗi lặp SoftAudio/CityMap sau tutorial, dừng đúng PID với deadline. Không sửa audio/route chỉ để vượt test. HQ01 full/battle/relic/ending trong lượt này không có bằng chứng mới đầy đủ.
- Windows build cuối **{build}**, log `Logs/player-experience-final-build.log`. Windows Player baseline bị Application Control chặn; không đổi chính sách, không tải/bỏ chặn binary để ép chạy.
- Android device **NOT_TESTED**. Không build APK; bản APK PASS cũ giữ SHA256.
- **Before/after cùng camera: NOT_AVAILABLE** — chưa chụp được baseline trước sửa. Không lấy ảnh lịch sử hoặc mockup để thay. Gallery chỉ ghi đúng Play Mode và tên run; không coi kiểm thử logic là nghiệm thu đẹp.
- Không có file scene/prefab/map PNG/audio/save gameplay bị chỉnh. 35 kiểm tra bảo toàn source/graph/state methods; thuật toán Move, route graph/waypoint/arrival, canonical art, relic/leisure data, settings và 133/155/29/22 giữ nguyên. EditorSettings QA được khôi phục nguyên byte.

## Văn bản sáu thành phố

Đã kiểm kê các field player-copy trong events/quests/puzzles/items. Số field còn chứa chuỗi giống ID cần đọc trong ngữ cảnh: `{remaining}`. Bảng ở `PlayerExperience-20261019/six-city-copy-candidates.json`. Đây là ứng viên từ dữ liệu, không mặc định là câu đã xuất ra UI. Chỉ sửa narrative Hội An và lớp UI dùng chung; năm thành phố còn lại chưa được triển khai đại trà hoặc kiểm thử UX. Không thay ID trong objectives/flags/answer/quest graph.

## File thực tế đã sửa/thêm

{filelist}

Thêm tool patch/verify/report, meta tương ứng bốn script mới, Docs/QA/PlayerExperience-20261019 và Validation/PlayerExperience-20261019. Diff từng file và copy-changes nằm trong Docs/QA. Baseline trước sửa chỉ khoảng 0,8–1 MB source; không full backup. Tác vụ số hóa tuyến chủ dự án được giữ nguyên và chưa áp dụng.

## Đánh giá nghiệm thu

**Đạt ở mức source + các lượt Play Mode nêu trên:** vai trò Lumi/sứ mệnh/bước đầu rõ; close/next tách đúng; chỉ dẫn focus không teleport; H3/tutorial state giữ đúng; hoa đăng có animation và replay; không đổi core state/reward.

**Chưa đạt nghiệm thu đầy đủ:** chưa có Windows Player chạy được và Android thật; thiếu bộ before/after; full HQ01 mới bị chặn trong Editor; vòng điều khiển to trên Android chưa tái hiện; leisure completion history chưa có trong driver hiện hành; chưa có playtest người mới để chứng minh dễ hiểu. Không tự tuyên bố các phần này PASS.

Checkpoint tăng dần và receipt được xuất riêng sau kiểm tra nguồn. **DỪNG để chủ dự án nghiệm thu pilot Hội An. Không triển khai năm thành phố còn lại, route hoặc Phase 7.**
'''
(R/'Docs/QA/PLAYER_EXPERIENCE_HOI_AN_PILOT_20261019.md').write_text(doc,encoding='utf-8')
print('Gallery images',len(chosen));print('Run statuses',[(s['run'],s['status'],s['checksPassed']) for s in summaries]);print('Windows build',build)
