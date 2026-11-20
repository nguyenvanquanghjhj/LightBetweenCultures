"""Bounded evidence/protection for the casual-adventure presentation task."""
from pathlib import Path
import sys,json,hashlib,shutil,datetime
R=Path(__file__).resolve().parents[1];D=R/'Docs/QA/CasualAdventure-20261020'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
command=sys.argv[1]
if command=='prepare':
 assert not(D/'baseline.json').exists(),'Baseline already exists; do not overwrite'
 D.mkdir(parents=True,exist_ok=True)
 source=list((R/'Assets/_Game/Scripts/UI').glob('*.cs'))+[R/'Assets/_Game/Scripts/Maps/CityMap.cs',R/'Assets/_Game/Scripts/Maps/MainMenu.cs',R/'Assets/_Game/Scripts/Maps/CityMap.Guidance.cs',R/'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs']
 for p in source:
  target=D/'before-source'/p.relative_to(R);target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,target)
 protected=list((R/'Assets/_Game/Resources/Content').glob('*.json'))+list((R/'Assets/_Game/Scripts/Content').glob('*.cs'))+list((R/'Assets/_Game/Scripts/Core').glob('*.cs'))
 protected+=[R/'Assets/_Game/Scripts/Maps'/n for n in ['CityMapPaths.cs','CityRouteGeometry.cs','CityMap.CulturalRestoration.cs','CityMap.RelicLight.cs','CityMap.HoiAnLighting.cs']if(R/'Assets/_Game/Scripts/Maps'/n).exists()]
 protected+=[R/'Tools/SixCityRouteEditor/data/source.json',R/'ProjectSettings/ProjectSettings.asset',R/'ProjectSettings/QualitySettings.asset',R/'ProjectSettings/EditorSettings.asset']
 protected+=list((R/'Assets/_Game/Resources').glob('HoiAnData.json'))
 maps=read(R/'Docs/Maps/OwnerRoutePhaseB-20261019/production-before.json')
 for entry in maps:
  if entry['path'].lower().endswith('.png'):protected.append(R/entry['path'])
 clarity=read(R/'Docs/QA/MapClarity-20261019/import-selection.json')
 protected+=[R/(p+'.meta')for p in clarity['paths']]
 entries=[{'path':p.relative_to(R).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)}for p in dict.fromkeys(protected+source)]
 dump(D/'baseline.json',{'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'entries':entries,'protected':[p.relative_to(R).as_posix()for p in dict.fromkeys(protected)],'baseCheckpoint':'D:/GAMEKKK/Backups/CheckpointOwnerRoutesPhaseB-20261019'})
 shutil.copy2(Path('C:/Users/admin/.codex/attachments/b469af73-d871-444c-b74f-2627a1a3d8bf/Pasted text.txt'),D/'REQUEST.md')
 print('Baseline recorded:',len(entries),'bounded files; source snapshot bytes:',sum(p.stat().st_size for p in source))
elif command=='verify':
 b=read(D/'baseline.json');changed=[];fail=[]
 for e in b['entries']:
  p=R/e['path']
  if not p.exists()or sha(p)!=e['sha256']:
   changed.append(e['path'])
   if e['path']in b['protected']:fail.append(e['path'])
 result={'protectedPass':not fail,'protectedUnexpectedChanges':fail,'changedExistingFiles':changed}
 dump(D/'protection.json',result);print(json.dumps(result,indent=2))
 if fail:sys.exit(1)
elif command=='finalize':
 v=R/'Validation/CasualAdventure-20261020';runs={}
 for name in ['before','after-ui01','delivery01']:
  text=(v/name/'smoke-results.txt').read_text(encoding='utf-8-sig')
  runs[name]={'result':'PASS'if text.rstrip().endswith('ALL CHECKS PASSED')else'FAILED_RETAINED','assertions':sum(s.startswith('PASS:')for s in text.splitlines())}
 assert runs['after-ui01']['result']==runs['delivery01']['result']=='PASS'
 photos=[('after-ui01/XP-mai-before-quest.png','Cô Mai: X đỏ góc phải, không có nút đóng trùng phía dưới'),('after-ui01/XP-lumi-promise.png','Lumi: nút tiếp chỉ chuyển nội dung'),('after-ui01/XP-objective.png','Việc cần làm: gợi ý chỉ focus, không tự đi'),('after-ui01/XP-mai-return-eye.png','Trả Mắt Đèn bằng hành động hiện hữu'),('after-ui01/XP-mai-local-light.png','Chỉ hoàn tất tutorial, không mở thành phố sớm'),('after-ui01/XP-journal.png','Nhật ký sau tutorial'),('delivery01/Portrait-jeonju-J15-stage4.png','So-yeon: portrait canonical và đóng góc phải'),('delivery01/Speaker-jeonju-J15-to-Lumi-08.png','Xác nhận giao: Hủy giữ vật phẩm')]
 page='''<!doctype html><meta charset="utf-8"><title>LBC — UI đang dang dở đã kiểm thử</title><style>body{background:#18343b;color:#fff3d7;font:17px/1.6 system-ui;margin:24px}a{color:#91e2d2}article{margin:32px 0}img{max-width:100%;height:auto;width:720px}</style><h1>UI checkpoint — Windows Player thật</h1><p>Ảnh gốc 720×1080 theo kích thước Player thực tế của lượt này. Không coi là Android device hoặc blind playtest. Không repaint hoặc sửa map.</p><a href="REPORT.md">Báo cáo ngắn</a>'''
 for rel,title in photos:
  p=v/rel;assert p.is_file(),p;url='../../../'+p.relative_to(R).as_posix()
  page+=f'<article><h2>{title}</h2><a href="{url}"><img src="{url}"></a></article>'
 (D/'gallery.html').write_text(page,encoding='utf-8')
 dump(D/'runs.json',runs)
 report='''# Tiếp tục UI đang dang dở — 2026-10-20

**Checkpoint kỹ thuật của phần UI, không phải COMPLETE toàn Thái Thượng Prompt.** Không chạy Phase 7 hoặc task blind playtest riêng.

[Ảnh runtime Windows Player](gallery.html) · [Kết quả lượt chạy](runs.json) · [Kiểm tra bảo toàn](protection.json) · [Checkpoint](checkpoint.json)

Đã sửa:

- X đỏ góc trên phải, vùng chạm 52×52 đơn vị Canvas. Chỉ gọi Close hiện hữu. Bỏ nút đóng trùng dưới hội thoại thông thường; nút Tiếp chỉ xuất hiện khi có bước chuyển speaker/lựa chọn.
- Xác nhận giao vẫn có hai hành động riêng: xác nhận và hủy giữ vật phẩm. X cũng đóng an toàn, không submit.
- Header dành chỗ cho tên, địa điểm và X. Lật trang lựa chọn có vùng chạm cao 48 đơn vị và không đè lên nội dung.
- Lớp tên hiển thị hỗ trợ node/quest/relic/fragment của sáu thành phố; giữ nguyên mọi ID trong dữ liệu và callback. Có dữ liệu mô tả bản sắc thành phố chuẩn bị ở helper; chưa gắn phần mô tả mới ấy vào UI và không tính là đã triển khai signature moments.
- Adapter QA EventClose nhận nút X mới để tiếp tục kiểm thử qua pointer thật; không đổi xử lý gameplay.

4 file source thay đổi: `UI/GameplayPanels.cs`, `UI/MobileUI.cs`, `UI/PlayerCopy.cs`, `Validation/GameplaySmokeRunner.cs` dưới `Assets/_Game/Scripts/`. Thêm tool bounded `Tools/casual_adventure.py` và thư mục evidence này. Không thêm manager, scene, Canvas hoặc EventSystem.

Kiểm thử:

| Phạm vi | Kết quả |
|---|---|
| Compile và Windows build | PASS — `Logs/casual-adventure-build01.log` |
| Fresh tutorial Hội An qua UI pointer | PASS — `after-ui01`, 221 assertions, không phải khảo sát người mới |
| Đóng Cô Mai trước nhận quest | PASS, save không đổi |
| Nhặt Mắt Đèn và trả Cô Mai | PASS, chỉ hoàn tất tutorial; HQ01/world unlock không bị mở sớm |
| Gợi ý / nhật ký / reload | PASS, không tự đi hoặc thêm reward |
| Jeonju xác nhận → Hủy → mở lại | PASS — `delivery01`, 66 assertions, giữ nguyên vật phẩm/quest; đúng portrait |
| Giao và nhận reward liên thành phố toàn chuỗi sau sửa | NOT_TESTED trong lượt này; lượt này kiểm tra nhánh mở/hủy xác nhận |
| Runtime UI còn lại ở bốn thành phố | NOT_TESTED mới; lớp tên dùng chung đã mở rộng, không nhận PASS toàn sáu city |
| Android build/device của bản UI mới | NOT_TESTED / NOT_BUILT; giữ APK trước, không tuyên bố APK cũ chứa sửa UI mới |
| Blind/human playtest sau đại tu | Chưa thực hiện; automation này không thay thế task riêng |

Lượt `before` bị lỗi quyền ghi PlayerPrefs trong sandbox; giữ nguyên log FAILED. Chạy lại Player với quyền truy cập settings phù hợp thì hai lượt trên hoàn tất, không sửa Audio để che lỗi quyền. Kích thước ảnh thực tế **720×1080**, không ghi nhầm thành 720×1280.

Giữ nguyên mạng route mới, 133 node/arrival, quest, puzzle, relic, save schema, H6/check-in, art, ánh sáng, BGM/SFX và cấu hình import sắc nét. `protection.json` ghi hash đối chiếu phạm vi bảo vệ. Không tạo ảnh before giả: ảnh bàn giao là trạng thái sau sửa; source trước sửa được lưu nhỏ trong before-source.

Phần chưa làm của yêu cầu lớn: tiếp tục các cải tiến contextual tutorial, button clarity ngoài hội thoại, minigame/combat UX, nhật ký mở rộng, lời thoại/bản sắc và đánh giá sáu signature moments theo thứ tự đã giao. Intro/guidance/journal đã có được giữ nguyên; lượt này không làm lại hoặc tự nhận các nhóm ấy COMPLETE.

Bước kỹ thuật đang dang dở đã chốt bằng build và hai lượt Player. Không reset/checkout/revert hay ghi đè graph Phase B.
'''
 (D/'REPORT.md').write_text(report,encoding='utf-8')
 print(json.dumps(runs,indent=2))
elif command in ['estimate','checkpoint']:
 b=read(D/'baseline.json');protection=read(D/'protection.json');assert protection['protectedPass']
 base=Path(b['baseCheckpoint']);manifest=read(base/'CHECKPOINT_MANIFEST.json')
 assert sha(base/'CHECKPOINT_MANIFEST.json')=='ca3c46355483f0d4c4cd5a997fb592fe7ce26c7463886d151290d3d67d3b5bc5'
 for e in manifest['entries']:assert sha(base/e['path'])==e['sha256'],e['path']
 for item in manifest['baseChain']:assert sha(Path(item['path'])/'CHECKPOINT_MANIFEST.json')==item['manifestSHA256']
 paths={R/p for p in protection['changedExistingFiles']};paths|={Path(str(p)+'.meta')for p in list(paths)}
 paths.add(Path(__file__).resolve())
 paths|={p for p in D.rglob('*')if p.is_file()and p.name!='checkpoint.json'}
 v=R/'Validation/CasualAdventure-20261020'
 for name in ['before','after-ui01','delivery01']:
  paths|={p for p in (v/name).iterdir()if p.is_file()and p.suffix in ['.txt','.json','.log']}
 import re
 for url in re.findall(r'(?:src|href)="([^"]+)"',(D/'gallery.html').read_text(encoding='utf-8')):
  p=(D/url).resolve();assert p.is_file();paths.add(p)
 paths.add(R/'Logs/casual-adventure-build01.log')
 size=sum(p.stat().st_size for p in paths);target=Path('D:/GAMEKKK/Backups/CheckpointCasualAdventureUI-20261020')
 print(json.dumps({'files':len(paths),'MiB':round(size/1048576,2),'baseVerified':True,'target':str(target)}))
 if command=='estimate':sys.exit(0)
 assert not target.exists(),'Refuse checkpoint overwrite'
 target.mkdir();entries=[]
 for p in sorted(paths):
  rel=p.relative_to(R);dest=target/rel;dest.parent.mkdir(parents=True,exist_ok=True);digest=sha(p);shutil.copy2(p,dest);assert sha(dest)==digest
  entries.append({'path':rel.as_posix(),'bytes':p.stat().st_size,'sha256':digest})
 restore='INCREMENTAL — REQUIRES BASE. Not a full project. Restore the verified base chain through CheckpointOwnerRoutesPhaseB-20261019, then overlay the manifest paths. Close Unity first. Prefer an isolated restore; do not overwrite later work. No deletions. No Library, build binaries or player saves. Validation saves here are QA evidence only. Verify payload hashes before restoring.'
 (target/'RESTORE.md').write_text(restore,encoding='utf-8')
 result={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(base),'baseManifestSHA256':sha(base/'CHECKPOINT_MANIFEST.json'),'baseChain':[{'path':str(base),'manifestSHA256':sha(base/'CHECKPOINT_MANIFEST.json')}]+manifest['baseChain'],'entries':entries,'bytes':size,'deletedPaths':[],'restoreGuideSHA256':sha(target/'RESTORE.md'),'summary':'Resume interrupted dialogue UI: red upper-right close, distinct next/cancel, six-city player names. Four source files and targeted Windows tutorial/delivery-cancel QA. Full casual overhaul remains pending; routes and canonical data protected.'}
 dump(target/'CHECKPOINT_MANIFEST.json',result)
 receipt={'path':str(target),'bytes':size,'files':len(entries),'manifestSHA256':sha(target/'CHECKPOINT_MANIFEST.json'),'payloadVerified':True,'baseVerified':True,'standalone':False,'requiresBase':str(base)}
 dump(D/'checkpoint.json',receipt);print(json.dumps(receipt,indent=2))
