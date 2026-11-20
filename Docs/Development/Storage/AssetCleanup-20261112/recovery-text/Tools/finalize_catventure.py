"""Summarize retained QA and verify source preservation; create a small delta restore point.
Run after the named verification folders and release builds have finished.
"""
import hashlib, html, json, shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Validation/Catventure-20261107'
BASE=Path('D:/GAMEKKK/Backups/CheckpointPreCatventure-20261107')
DEST=Path('D:/GAMEKKK/Backups/CheckpointCatventure-20261107')
def sha(p):
    h=hashlib.sha256()
    with p.open('rb') as f:
        for b in iter(lambda:f.read(1024*1024),b''):h.update(b)
    return h.hexdigest()
def native(name,restart=False):
    p=OUT/name/('restart-results.txt' if restart else 'smoke-results.txt')
    s=p.read_text(encoding='utf-8-sig')
    assert s.rstrip().endswith('ALL CHECKS PASSED'),p
    assert '\nERROR:' not in s,p
    return dict(folder=name,checks=sum(line.startswith('PASS:') for line in s.splitlines()),firstLine=s.splitlines()[0],sha256=sha(p))
def finalize():
    runs=[native('verified-portrait'),native('verified-desktop'),native('verified-portrait',True),native('legacy-regression')]
    total=sum(r['checks'] for r in runs)
    for name in ['verified-portrait','verified-desktop']:
        save=json.loads((OUT/name/'gameplay-save.json').read_text(encoding='utf-8-sig'))
        assert len(save['catRescues'])==6 and all(p['rescued'] and len(p['completed'])==4 and len(p['supplies'])==3 for p in save['catRescues'])
        jeonju=next(p for p in save['catRescues'] if p['cityId']=='jeonju')
        dress=next(c for c in jeonju['challenges'] if c['challengeId']=='jeonju_hanbok')
        assert dress['paletteChosen'] and dress['palette']==2,'Native JSON did not retain chosen Hanbok palette'
    release=(ROOT/'Builds/Windows-Catventure-Release-20261107/Catventure_Data/Managed/Assembly-CSharp.dll').read_bytes()
    assert b'GameplaySmokeRunner' not in release,'QA runner must be excluded from Release'
    for token in [b'CatRescueManager',b'CatHanbokGraphic',b'CatMiniMapGraphic']:assert token in release
    assert json.loads((OUT/'android-verification.json').read_text())['latestHanbokAndMiniMapTypesPresent']
    for platform in ['windows','windows-release','android']:
        assert (OUT/(platform+'-build.txt')).read_text().startswith('Succeeded'),platform
    baseline=json.loads((BASE/'CURRENT_SOURCE_HASHES.json').read_text(encoding='utf-8'))
    before={e['path'][8:]:e for e in baseline}
    status=ROOT/'Docs/QA/CURRENT_QA_STATUS.md'
    heading=f'''# Catventure — 07/11/2026

Windows Release và APK Android build PASS. Tuyến giải cứu sáu thành phố / 24 thử thách: **{total} kiểm tra Windows Player PASS** (hai kích thước, cold restart và hồi quy save/World Map/shop). **512 + 2 domain checks PASS** trong adapter host, tách khỏi kết quả native.

Android thiết bị thật **NOT_TESTED** vì ADB không có thiết bị; toàn bộ campaign truyện cũ **NOT_RERUN**. [Bàn giao và bằng chứng Catventure](../Catventure/REPORT.md). Các mục RC1/RC2 bên dưới là lịch sử, không phải xác nhận bản Catventure.

---

'''.encode('utf-8')
    old_status=status.read_bytes()
    if not old_status.startswith('# Catventure'.encode('utf-8')):status.write_bytes(heading+old_status)
    allowed={
        'Assets/_Game/Scripts/Core/GameBootstrap.cs','Assets/_Game/Scripts/Core/SaveSystem.cs',
        'Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Scripts/Maps/WorldMap.cs',
        'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs',
        'README.md','RELEASE_FREEZE.md','Docs/QA/CURRENT_QA_STATUS.md',
    }
    files=[p for top in ['Assets','Packages','ProjectSettings','Tools','Docs'] for p in (ROOT/top).rglob('*') if p.is_file() and '__pycache__' not in p.parts and not p.is_relative_to(ROOT/'Tools/CatRescueTests/obj') and not p.is_relative_to(ROOT/'Tools/CatRescueTests/bin')]
    files += [p for p in ROOT.iterdir() if p.is_file() and p.suffix in ['.md','.txt']]
    current=[];delta=[];changed_old=[]
    for p in files:
        rel=p.relative_to(ROOT).as_posix();entry=dict(path='Project/'+rel,bytes=p.stat().st_size,sha256=sha(p));current.append(entry)
        old=before.get(rel)
        if old is None or old['sha256']!=entry['sha256']:
            delta.append((p,entry))
            if old:
                assert rel in allowed,('Unexpected modified pre-existing input',rel)
                changed_old.append(rel)
    deleted=[r for r in before if not (ROOT/r).exists()]
    assert not deleted,('No original source/document should be deleted',deleted)
    artifacts=[]
    for rel in ['Builds/Windows-Catventure-20261107/Catventure.exe','Builds/Windows-Catventure-20261107/Catventure_Data/Managed/Assembly-CSharp.dll','Builds/Windows-Catventure-Release-20261107/Catventure.exe','Builds/Windows-Catventure-Release-20261107/Catventure_Data/Managed/Assembly-CSharp.dll','Builds/Android/Catventure-20261107.apk']:
        p=ROOT/rel;assert p.is_file(),p;artifacts.append(dict(path=rel,bytes=p.stat().st_size,sha256=sha(p)))
    domain_checks=0
    for name in ['results.txt','restart-results.txt']:
        s=(OUT/'domain'/name).read_text(encoding='utf-8-sig');assert 'DOMAIN CHECKS PASSED' in s
        domain_checks+=sum(line.startswith('PASS:') for line in s.splitlines())
    summary=dict(createdUTC=datetime.now(timezone.utc).isoformat(),status='FOCUSED_CATVENTURE_WINDOWS_PASS_ANDROID_BUILD_PASS_DEVICE_NOT_TESTED',
                 chapters=6,challenges=24,cats=6,domainChecks=domain_checks,nativeRuns=runs,artifacts=artifacts,
                 protectedOriginalsUnchanged=len(before)-len(changed_old),modifiedOriginals=changed_old,
                 deletedOriginals=[],device='ADB: no devices connected; no install. All QA used isolated save paths.',
                 limitations=['Android touch/FPS/RAM on a real phone NOT_TESTED','Independent child/human playtest NOT_TESTED','Full original legacy story campaign NOT_RERUN; focused legacy save/world/shop regression recorded'],
                 recoveryBase=str(BASE),checkpoint=str(DEST))
    (OUT/'summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    cards=[]
    for folder in ['verified-portrait','verified-desktop']:
        for p in sorted((OUT/folder).glob('CAT-*.png')):
            rel=p.relative_to(OUT).as_posix();label=html.escape(folder+' / '+p.stem)
            cards.append(f'<figure><a href="{rel}"><img loading="lazy" src="{rel}" alt="{label}"></a><figcaption>{label}</figcaption></figure>')
    gallery='''<!doctype html><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Catventure — runtime evidence</title>
<style>body{font:16px system-ui;background:#fff7e5;color:#24464a;margin:28px}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(230px,1fr));gap:18px}figure{margin:0;background:#fff;border:1px solid #dec89b;border-radius:12px;padding:10px}img{width:100%;height:360px;object-fit:contain}figcaption{font-size:13px;margin-top:8px}a{color:inherit}</style>
<h1>Catventure · ảnh Player thật</h1><p>Hai lượt sáu thành phố: 540×960 và 1280×720. Chọn ảnh để xem kích thước gốc. Đây là bằng chứng Windows, không phải kiểm thử thiết bị Android.</p><p><a href="summary.json">Kết quả và SHA256</a></p><main>'''+''.join(cards)+'</main>'
    (OUT/'gallery.html').write_text(gallery,encoding='utf-8')
    # Add report after hashing production inputs; include it explicitly in the delta below.
    total=sum(r['checks'] for r in runs)
    rows='\n'.join(f"| {r['folder']}{' / restart' if i==2 else ''} | {r['checks']} | PASS |" for i,r in enumerate(runs))
    report=f'''# Catventure — bàn giao ngày 07/11/2026

Đã tích hợp tuyến giải cứu mèo độc lập cho sáu thành phố, 24 thử thách và sáu mèo sưu tập. Có kéo thả, ghép tranh, tìm vật ẩn trong cảnh, mê cung, câu hỏi/tình huống, bát bibimbap tương tác, ghép Hangeul và chọn màu Hanbok. Hội An sáng dần theo nhiệm vụ; từng thành phố có thời tiết. Tiến trình được lưu từng thao tác, phần thưởng có receipt chống lặp; mở thêm tuyến thành phố mà không cấp cờ hoàn thành truyện cũ.

## Chạy game

- **Windows Release:** [Catventure.exe](../../Builds/Windows-Catventure-Release-20261107/Catventure.exe). Chạy ngay trong thư mục này; giữ `Catventure_Data` và các DLL cạnh EXE.
- **Android APK:** [Catventure-20261107.apk](../../Builds/Android/Catventure-20261107.apk), phiên bản `1.1-catventure-20261107`, versionCode 20. Không tự cài đè lên điện thoại trong lượt này.
- [Hướng dẫn chơi](HANDOFF.md) · [Ảnh Player trước/sau và các mini-game](../../Validation/Catventure-20261107/gallery.html) · [Manifest SHA256](../../Validation/Catventure-20261107/summary.json).

## Kiểm chứng

| Lượt Windows Player | Số kiểm tra | Kết quả |
|---|---:|---|
{rows}

Tổng **{total} kiểm tra Player PASS** trong các lượt cuối. Hai lượt chính đi theo đường hiện có, dùng chuột/cảm ứng mô phỏng để lấy vật dụng, thực hiện toàn bộ 24 thử thách, cứu đủ sáu mèo và mở bưu thiếp. Có kiểm tra sai/đúng, đóng/mở giữ mảnh, phối màu, reload, giao diện âm thanh, sơ đồ mini-map và ánh đèn Hội An. Lượt regression đi qua World Map thật, kiểm tra save/recovery, giữ Lumi canonical và giao dịch cửa hàng cũ.

**512 + 2 kiểm tra domain PASS**: biên, thứ tự, vị trí, thiếu vật dụng, lưu dở, lỗi ghi file phải rollback, chống thưởng trùng, backup recovery và tiến trình mới. Các bài này biên dịch file `CatRescue.cs` và `SaveSystem.cs` thật trong host .NET có adapter Unity; không được tính là kiểm thử native Unity. Dữ liệu/graph xác nhận mọi mục tiêu mới có đường từ spawn ở stage 0; mọi mê cung có lời giải.

Windows Development, Windows Release và APK Android đều build thành công. APK được kiểm tra riêng bằng công cụ Android trong `android-verification.txt`. **Chưa có điện thoại ADB kết nối**, nên input/FPS/RAM Android và độ hấp dẫn qua người chơi độc lập vẫn **NOT_TESTED**. Không tuyên bố đã chạy lại toàn bộ campaign truyện cũ.

## Bảo toàn và khôi phục

Đã so sánh SHA256 với bản trước nâng cấp: **{summary['protectedOriginalsUnchanged']} file gốc giữ nguyên**, không xóa file gốc. Mọi artwork/metadata nhân vật cũ, dữ liệu sáu map, navigation, quest/puzzle và Packages/ProjectSettings giữ nguyên. Các file runtime cũ sửa chỉ gồm bootstrap, save, City Map, World Map; QA runner và tài liệu được bổ sung tương ứng. Sửa thêm race khi chuyển thành phố để bản đồ cũ không cập nhật bằng dữ liệu thành phố mới; dời nút âm thanh để tránh chồng HUD.

- Trước sửa: `D:/GAMEKKK/Backups/CheckpointPreCatventure-20261107`.
- Sau sửa: `D:/GAMEKKK/Backups/CheckpointCatventure-20261107`.
- Cả hai là **incremental, cần toàn bộ base chain trong manifest**, không phải bản sao độc lập. Không sao chép Library/cache/build hoặc xóa bản backup nào.

Các lượt raw trước sửa vẫn giữ: lỗi giấy phép/sandbox ở `pilot`, lỗi chuyển map và kéo chưa hoàn tất ở `final-portrait`; chúng không được cộng vào kết quả cuối. Nghiệm thu ở các thư mục `verified-*` sử dụng build đã sửa. Phiên bản source/ảnh cũ của các lượt trước nằm trong manifest `pre-polish-build.json` và log tương ứng.

[Phương án và file tích hợp](IMPLEMENTATION.md) · [Nguồn văn hóa](CULTURAL_SOURCES.md) · [Ảnh mới và prompt](ART_PROVENANCE.md).
'''
    report_path=ROOT/'Docs/Catventure/REPORT.md';report_path.write_text(report,encoding='utf-8')
    report_entry=dict(path='Project/Docs/Catventure/REPORT.md',bytes=report_path.stat().st_size,sha256=sha(report_path))
    delta=[(p,e) for p,e in delta if p!=report_path]+[(report_path,report_entry)]
    size=sum(e['bytes'] for _,e in delta);print('Incremental estimate:',len(delta),'files,',size,'bytes')
    assert size<200*1024*1024,'Inspect unexpectedly large source checkpoint first'
    DEST.mkdir(exist_ok=True)
    manifest=DEST/'INCREMENTAL_MANIFEST.json'
    if manifest.exists():
        existing=json.loads(manifest.read_text(encoding='utf-8'))
        assert {(e['path'],e['sha256']) for e in existing['entries']}=={(e['path'],e['sha256']) for _,e in delta},'Existing checkpoint differs: do not overwrite it'
    else:
        for p,e in delta:
            target=DEST/e['path'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,target);assert sha(target)==e['sha256']
        d=dict(type='INCREMENTAL_REQUIRES_BASE_CHAIN',standalone=False,createdUTC=summary['createdUTC'],base=str(BASE),baseManifest='INCREMENTAL_MANIFEST.json',baseManifestSHA256=sha(BASE/'INCREMENTAL_MANIFEST.json'),baseChainVerified=True,summary='Owner-authorized Catventure six-city extension, source/assets/docs only. Requires pre-upgrade checkpoint and its verified full chain. No Library, builds or historical Validation copied.',entries=[e for _,e in delta],deletedPaths=[])
        manifest.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
        (DEST/'RESTORE.md').write_text('NOT standalone. Restore the pre-Catventure checkpoint and its complete verified base chain, then overlay Project. No original paths deleted. Keep owner saves separate.\n',encoding='utf-8')
    print('Verified checkpoint:',DEST)
    print('Native checks:',total,'; protected unchanged:',summary['protectedOriginalsUnchanged'])
if __name__=='__main__':finalize()
