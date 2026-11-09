from pathlib import Path
import json, csv
R=Path(r'D:\GAMEKKK\LightBetweenCultures'); O=R/'Docs/Storage/SafeCleanup-20261019'
b=json.loads((O/'before.json').read_text(encoding='utf-8')); a=json.loads((O/'after.json').read_text(encoding='utf-8'))
p=json.loads((O/'cleanup-plan.json').read_text(encoding='utf-8')); c=json.loads((O/'backup-catalog.json').read_text(encoding='utf-8'))
def gib(n):return f'{n/2**30:.3f}'
def gb(n):return f'{n/10**9:.3f}'
def rel(p):return str(Path(p).relative_to(R))
for label,j in [('before',b),('after',a)]:
 for kind,rows in [('directories',j['project']['directories'][1:]),('files',j['project']['largest_files'])]:
  with (O/f'top15-{kind}-{label}.csv').open('w',encoding='utf-8-sig',newline='') as f:
   w=csv.writer(f);w.writerow(['Rank','Path','Bytes','GiB'])
   for n,r in enumerate(rows[:15],1):w.writerow([n,r['path'],r['bytes'],gib(r['bytes'])])
with (O/'backups.csv').open('w',encoding='utf-8-sig',newline='') as f:
 w=csv.DictWriter(f,fieldnames=list(c['backups'][0]));w.writeheader();w.writerows(c['backups'])
assert b['backups']['bytes']==a['backups']['bytes'] and b['backups']['files']==a['backups']['files']
assert b['backups']['directories']==a['backups']['directories']
ad={x['path']:x for x in a['project']['directories']}; bd={x['path']:x for x in b['project']['directories']}
protected=['Assets','Packages','ProjectSettings','Builds','Tools','References','_CheckIn_Import_20261018','Validation']
for name in protected:
 assert bd[str(R/name)]==ad[str(R/name)],name
deletes='\n'.join(f"| `{rel(t['path'])}` | {gib(t['bytes'])} | {len(t['files'])} |" for t in p['targets'])
topdirs='\n'.join(f"| {i} | `{rel(d['path'])}` | {gib(d['bytes'])} |" for i,d in enumerate(b['project']['directories'][1:16],1))
topfiles='\n'.join(f"| {i} | `{rel(d['path'])}` | {gib(d['bytes'])} |" for i,d in enumerate(b['project']['largest_files'][:15],1))
backup_table='\n'.join(f"| `{Path(d['path']).name}` | {gib(d['bytes'])} | {'BASE CHAIN — giữ bắt buộc' if d['latestRestoreChain'] else 'Giữ; chưa duyệt xóa'} |" for d in c['backups'][:15])
text=f'''# Dọn dung lượng an toàn — 2026-10-19

Hoàn tất đúng phạm vi: đo dung lượng, xóa hai thư mục Gradle intermediates đã hết sử dụng, báo cáo và dừng. Không chạy Unity, không build APK, không chạy Phase 7, không sửa game.

## Kết quả đo thực tế

Đơn vị: GB = 1.000.000.000 byte; GiB = 1.073.741.824 byte. Đây là tổng kích thước logic của file, không phải cộng chồng các thư mục cha/con. Không theo junction/symlink; cả hai lần quét không có link bị bỏ qua hoặc lỗi đọc.

| Phạm vi | Trước | Sau |
|---|---:|---:|
| Project | {gb(b['project']['bytes'])} GB / {gib(b['project']['bytes'])} GiB | {gb(a['project']['bytes'])} GB / {gib(a['project']['bytes'])} GiB |
| Backups ngoài project | {gb(b['backups']['bytes'])} GB / {gib(b['backups']['bytes'])} GiB | {gb(a['backups']['bytes'])} GB / {gib(a['backups']['bytes'])} GiB |
| Project + Backups | {gib(b['project']['bytes']+b['backups']['bytes'])} GiB | {gib(a['project']['bytes']+a['backups']['bytes'])} GiB |

- Đã xóa **{p['totalBytes']:,} byte = {gb(p['totalBytes'])} GB / {gib(p['totalBytes'])} GiB**, 1.571 file.
- Project giảm ròng {gb(b['project']['bytes']-a['project']['bytes'])} GB; chênh lệch so với số byte xóa do các file kiểm kê mới trong Docs/Storage. Số đo sau ở thời điểm `after.json`, trước khi xuất báo cáo cuối, sai số bổ sung chỉ vài KiB.
- Dung lượng trống ổ D tăng từ {gib(b['disk']['free'])} lên {gib(a['disk']['free'])} GiB, tăng {gib(a['disk']['free']-b['disk']['free'])} GiB; số này có thể chịu ảnh hưởng tiến trình khác trên ổ đĩa.
- Con số khoảng 46 GiB khớp **project + Backups**, riêng project trước khi dọn là 26,837 GiB.

## Đã xóa chính xác

| Thư mục tương đối trong project | GiB | Số file |
|---|---:|---:|
{deletes}

Đây là sản phẩm trung gian Gradle: assets đã merge/compress, dex, manifest/resource đã xử lý, JNI đã merge/strip và AAR phục vụ lint. Nguồn Gradle, native build cache, thư mục outputs, APK và khóa ký đều còn. Không có APK/AAB/keystore/JKS trong hai cây đã xóa. Lần build Android sau cần tạo lại bước đóng gói/Gradle này; không xóa cache import toàn dự án và không cần import lại toàn bộ Assets chỉ vì thao tác này. Không cam kết thời gian build vì không build thử trong task dọn dung lượng.

Danh sách từng file, kích thước và thời gian sửa trước xóa: [cleanup-plan.json](cleanup-plan.json). Kết quả từng thư mục: [deletion-results.json](deletion-results.json). Script thực hiện: [clean-safe-intermediates.ps1](clean-safe-intermediates.ps1), chỉ cho phép đúng hai đường dẫn tuyệt đối; không dùng để dọn diện rộng.

## Xác minh an toàn

- PASS: không có Editor, Player, Java/Gradle, Bee, IL2CPP hoặc build worker hoạt động trước khi xóa. Chỉ Unity Hub, licensing và Hub `unity.exe serve`; không dừng bất kỳ tiến trình nào. [process-check.json](process-check.json).
- PASS: tất cả đường dẫn xóa đã resolve nằm trong đúng Gradle root; không reparse point trên đường dẫn, tổ tiên hoặc con; từng file không thay đổi kích thước/mtime từ lúc lập kế hoạch.
- PASS: 43 kiểm tra SHA256 trước/sau cho APK, khóa ký, PlayerSettings, package manifests, dữ liệu SixCityRouteEditor, Edge ID registry và manifest chuỗi checkpoint. [verification.json](verification.json).
- PASS: APK `Builds/Android/LightBetweenCultures-Test.apk` giữ nguyên 285.852.858 byte; SHA256 `ecc062ade3c1967a2d184e555a38098b3269a6b4de602aa1c3c66b1f523b5481`.
- PASS: bản `launcher-release.apk` trong Gradle outputs có SHA256 giống APK PASS và vẫn giữ nguyên. Chỉ tìm thấy hai APK này trong project, không có APK cũ độc lập để tự dọn.
- PASS: `Assets`, `Packages`, `ProjectSettings`, `Builds`, `Tools`, `References`, `_CheckIn_Import_20261018` và toàn bộ `Validation` có số file/tổng byte không đổi. Không có thao tác ghi vào các thư mục đó. Đây là kiểm tra metadata, không tự nhận đã hash toàn bộ source/art.
- PASS: 92 thư mục backup cấp một, 35.329 file, tổng byte và toàn bộ thống kê thư mục backup không đổi. Không xóa hay chỉnh backup.
- NOT_RUN: gameplay regression, build Windows/Android, Phase 7. Task không sửa runtime và không yêu cầu build lại.

## 15 thư mục lớn nhất trong project trước khi dọn

**Có cả thư mục cha/con để chỉ đúng điểm chiếm chỗ; không cộng các hàng với nhau.** CSV trước/sau và top 15 file đi kèm cùng báo cáo.

| # | Đường dẫn | GiB |
|---|---|---:|
{topdirs}

## 15 file lớn nhất trước khi dọn

| # | File | GiB |
|---|---|---:|
{topfiles}

## Dữ liệu lớn được giữ lại

| Nhóm | Dung lượng còn lại | Lý do |
|---|---:|---|
| Library | {gib(ad[str(R/'Library')]['bytes'])} GiB | Giữ import database, DataStore, PackageCache, mã native/cache build; tránh import/build lại lớn |
| Validation | {gib(ad[str(R/'Validation')]['bytes'])} GiB | Ảnh, log, save QA, tuyến chủ dự án định vẽ, bằng chứng PASS/FAIL và dữ liệu tái hiện; chưa được duyệt bỏ lịch sử |
| Builds | {gib(ad[str(R/'Builds')]['bytes'])} GiB | Windows Player hiện tại và APK PASS mới nhất |
| Logs | {gib(ad[str(R/'Logs')]['bytes'])} GiB | Log nghiệm thu/build có giá trị đối chiếu; không coi toàn bộ log cũ là rác |
| Assets và source/art packs | Giữ nguyên | Nội dung canonical và nguồn gốc asset |
| `.utmp` | ~2,16 MiB | Nhỏ, native build scratch có thể liên quan incremental build; chưa cần dọn |
| `build` | <1 MiB | Chỉ log Gradle output, lợi ích nhỏ; giữ chẩn đoán |

Không có thư mục `Temp` hoặc `obj` ở gốc project. Những `obj` bên trong native cache không bị xóa. Không tạo full backup hoặc checkpoint mới vì không có thay đổi gameplay/source cần snapshot; hồ sơ dọn nằm tại Docs/Storage.

## Backups ngoài project — chưa xóa

Root: `D:\\GAMEKKK\\Backups`, **{gib(a['backups']['bytes'])} GiB**, 92 thư mục. Bảng đầy đủ: [backups.csv](backups.csv), phân loại manifest và chuỗi phụ thuộc: [backup-catalog.json](backup-catalog.json).

| 15 backup lớn nhất | GiB | Quyết định |
|---|---:|---|
{backup_table}

Checkpoint mới nhất `CheckpointAndroidAppIcon-20261019` phụ thuộc một chuỗi 35 checkpoint kể cả chính nó; base cuối là `CheckpointFullStoryComplete-20261017`. Giữ nguyên chuỗi. Không suy diễn rằng checkpoint là bản sao đầy đủ từ tên thư mục. Các thư mục không có manifest gốc được ghi `NO_ROOT_MANIFEST`, chưa xác nhận đủ khả năng phục hồi.

## Cần chủ dự án quyết định nếu muốn dọn thêm

1. **Lịch sử QA**: 11,729 GiB. Chọn đợt nào được phép bỏ, rồi kiểm tra liên kết báo cáo/manifest, save và ảnh quan trọng trước. Hiện chưa tính toàn bộ số này là có thể tiết kiệm an toàn.
2. **Các full snapshot cũ ngoài chuỗi base mới nhất**: `CheckpointPortraitDialogueStoryAudit-20261017` (3,209 GiB), `CheckpointTerrainRoutes-20261017` (2,223 GiB), `CheckpointCityMapUIPolish-20261017` (1,802 GiB), `CheckpointFullStoryAuthored-20261017` (1,240 GiB). Tổng khoảng 8,474 GiB nếu chủ dự án quyết định bỏ cả bốn **sau kiểm tra phụ thuộc và nội dung độc nhất riêng**. Chưa xác nhận an toàn để xóa nguyên các checkpoint này.
3. Manifest năm full snapshot ghi nhận tổng **{gib(c['manifestOverlapEstimate']['duplicateBytes'])} GiB** nội dung trùng SHA256+kích thước giữa các lần sao lưu. Đây là ước lượng độ trùng theo manifest đã có, không rehash 10.000+ file và **không phải dung lượng chắc chắn xóa được**. Mỗi snapshot vẫn có thể chứa tài liệu/source/ảnh lịch sử độc nhất.
4. **Cache Library còn lại**: có thể cân nhắc native/Bee cache nếu thật sự cần thêm chỗ và chấp nhận build sau chậm hơn; chưa xóa thêm. Không đề nghị xóa toàn bộ Library khi ổ D hiện còn khoảng {gib(a['disk']['free'])} GiB trống.
5. **Bản APK trùng trong Gradle outputs**: 272,61 MiB, hash khớp bản PASS. Vẫn giữ để bảo toàn đầu ra build; chưa đụng APK nào.

Dừng tại đây. Chưa thực hiện bất kỳ mục đề xuất nào.
'''
(O/'REPORT.md').write_text(text,encoding='utf-8')
print('REPORT',O/'REPORT.md');print('project_net_freed_bytes',b['project']['bytes']-a['project']['bytes'])
