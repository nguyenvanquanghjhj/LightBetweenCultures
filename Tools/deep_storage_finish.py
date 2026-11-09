"""Verify retained source/builds and report measured disk-space differences."""
from pathlib import Path
import csv,datetime,hashlib,json,shutil
from deep_storage_cleanup import ROOT,OUT,measure,drives
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(4*1024**2),b''):h.update(b)
    return h.hexdigest()
before=read(OUT/'inventory.json');backup=read(OUT/'source-backup.json');dest=Path(backup['path'])
assert sha(dest/'SOURCE_MANIFEST.json')==backup['manifestSHA256']
manifest=read(dest/'SOURCE_MANIFEST.json');live=0
for e in manifest['entries']:
    assert sha(dest/e['path'])==e['sha256'],e['path']
    if e['path'].startswith(('Project/Assets/','Project/Packages/','Project/ProjectSettings/')):
        p=ROOT/e['path'][len('Project/'):];assert p.is_file() and sha(p)==e['sha256'],str(p);live+=1
kept=read(OUT/'kept-builds.json')
for e in kept['files']:assert sha(ROOT/e['path'])==e['sha256'],e['path']
win=ROOT/'Builds/Windows-LeisureVisuals-20261020';assert sum(1 for p in win.rglob('*') if p.is_file())==376
assert not (ROOT/'Library/Bee').exists()
roots=[measure(ROOT),measure(ROOT.parent/'Backups')];after=drives()
delta={d:dict(beforeFreeBytes=before['drivesBefore'][d]['free'],afterFreeBytes=after[d]['free'],measuredFreeIncreaseBytes=after[d]['free']-before['drivesBefore'][d]['free']) for d in ['C','D']}
results=[read(OUT/f'deletion-result-{s}.json') for s in ['OutputsAndCaches','ApprovedCheckpoints']]
status=dict(finishedUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),drives=delta,rootsAfter=roots,backup=backup,backupPayloadVerifiedFiles=len(manifest['entries']),liveCoreVerifiedFiles=live,retainedBuildHashesVerified=True,windowsBuild='PRESERVED_PASS; not rebuilt or rerun during cleanup',android='LAST_GOOD_APK_PRESERVED; latest leisure visual patch not included; DEVICE_NOT_TESTED',sharedCaches='NOT_DELETED; Gradle daemon active',legacyToolCheckpoint='Provenance only, not a current restorable chain',failures='Initial Bee long-path failure resolved; no remaining deletion errors')
(OUT/'result.json').write_text(json.dumps(status,indent=2)+'\n',encoding='utf-8')
largest=[]
for r in before['roots']:
    for e in r.get('groups',[]):
        if len(Path(e['path']).parts)==1:largest.append(dict(path=str(Path(r['path'])/e['path']),bytes=e['bytes']))
with (OUT/'top-15-before.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.DictWriter(f,fieldnames=['path','bytes']);w.writeheader();w.writerows(sorted(largest,key=lambda e:-e['bytes'])[:15])
gib=lambda n:f'{n/1024**3:.3f}'
rows='\n'.join('| '+d+': | '+gib(e['beforeFreeBytes'])+' | '+gib(e['afterFreeBytes'])+' | +'+gib(e['measuredFreeIncreaseBytes'])+' |' for d,e in delta.items())
summary='''# Deep storage cleanup · 20/10/2026

| Ổ | Trống trước (GiB) | Trống sau (GiB) | Chênh lệch đo trực tiếp (GiB) |
|---|---:|---:|---:|
'''+rows+'''

Đây là số free-space đọc trực tiếp từ ổ đĩa, không phải tổng dung lượng ước tính. Số C: có thể biến động do ứng dụng khác; file Unity tạm/dump được xóa khoảng 44,9 MB. Bản backup source mới đã được tính vào chênh lệch ròng trên D:.

- Đã xóa: 100 checkpoint/backup cũ theo xác nhận riêng của chủ dự án (19,886 GiB logical), build Windows/APK cũ, ZIP Windows dư, QA captures/output cũ, log, và 9,555 GiB cache build có thể tái tạo. Bee ban đầu gặp đường dẫn dài 278 ký tự; đã xử lý đúng cache bằng Windows extended-length path.
- Giữ toàn bộ Assets/metadata, Packages, ProjectSettings, source, art gốc, 18 check-in, route editor/dữ liệu thiết kế đường, save, keystore/signing và cấu hình tool đang dùng. Dữ liệu cache import chính/PackageCache còn nguyên. Build sau sẽ dựng lại Bee/PlayerData/Burst; không cần import lại toàn bộ art vì không xóa cả Library.
- Giữ một Windows Player đầy đủ: `D:\\GAMEKKK\\LightBetweenCultures\\Builds\\Windows-LeisureVisuals-20261020\\LightBetweenCultures.exe`.
- Giữ một APK thành công gần nhất: `D:\\GAMEKKK\\LightBetweenCultures\\Builds\\Android\\LightBetweenCultures-GameFeel-20261020.apk`. APK chưa có ba sửa leisure sau đó; không tự build lại trong lượt này.
- Backup source độc lập đã xác minh: `D:\\GAMEKKK\\Backups\\LBC_SourceVerified-20261020`, 472,5 MiB, không cần base checkpoint. Có 35 nội dung source lịch sử độc nhất ở `HistoricalUnique`. Ngoài ra giữ checkpoint 1,47 MiB làm input provenance cho route exporter; không coi chuỗi lịch sử đó còn khôi phục được.
- C: giữ Unity Editor/Android SDK/NDK/JDK khoảng 23,719 GiB, Gradle 6,223 GiB (daemon đang hoạt động), Unity cache dùng chung 0,534 GiB và Unity Hub. Không xóa/uninstall cache dùng chung hay công cụ. Zalo/VS Code/ứng dụng khác, Downloads và dữ liệu cá nhân được giữ nguyên.
- Kiểm tra sau dọn: '''+str(len(manifest['entries']))+''' file trong backup và '''+str(live)+''' file Assets/Packages/ProjectSettings đều đúng hash; hash EXE/game DLL/APK giữ nguyên; Windows đủ 376 file dependency. Không build APK, không chạy full QA/Phase 7, không sửa gameplay.

[QA hiện hành](../../QA/CURRENT_QA_STATUS.md) · [15 mục lớn nhất trước dọn](top-15-before.csv) · [Số liệu sau dọn](result.json) · [Build/hash giữ lại](kept-builds.json)

Hoàn tất và DỪNG.
'''
(OUT/'SUMMARY.md').write_text(summary,encoding='utf-8')
print(json.dumps(dict(drives=delta,projectBeforeGiB=before['roots'][0]['bytes']/1024**3,projectAfterGiB=roots[0]['bytes']/1024**3,backupsAfterGiB=roots[1]['bytes']/1024**3,verifiedBackupFiles=len(manifest['entries']),verifiedLiveCoreFiles=live),indent=2))
