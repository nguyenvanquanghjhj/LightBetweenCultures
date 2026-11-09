"""Record the owner's naming/input correction without altering historical QA."""
import hashlib
import json
import shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Validation/Catventure-LabelsLayout-20261107'
BASE = ROOT.parent / 'Backups/CheckpointCatventure-20261107'
DEST = ROOT.parent / 'Backups/CheckpointCatventureLabelsLayout-20261107'

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def main():
    manifest = json.loads((BASE / 'INCREMENTAL_MANIFEST.json').read_text())
    for e in manifest['entries']:
        assert sha(BASE / e['path']) == e['sha256'], e['path']
    assert sha(Path(manifest['base']) / manifest['baseManifest']) == manifest['baseManifestSHA256']
    previous = json.loads((Path(manifest['base']) / 'CURRENT_SOURCE_HASHES.json').read_text())
    baseline = {e['path']: e for e in previous}
    baseline.update({e['path']: e for e in manifest['entries']})
    runs = []
    for folder, name in [('portrait-final', 'smoke-results.txt'), ('desktop-final', 'smoke-results.txt'), ('save-upgrade-final', 'restart-results.txt')]:
        log = (OUT / folder / name).read_text(encoding='utf-8-sig')
        assert log.rstrip().endswith('ALL CHECKS PASSED'), folder
        assert 'FAILED' not in log and 'ERROR:' not in log, folder
        runs.append(dict(folder=folder, checks=sum(s.startswith('PASS:') for s in log.splitlines()), result='PASS'))
    for platform in ['windows', 'windows-release', 'android']:
        assert (OUT / (platform + '-build.txt')).read_text().startswith('Succeeded'), platform
        assert (OUT / (platform + '-build.txt')).stat().st_mtime >= (ROOT / 'Assets/_Game/Scripts/Maps/CityMap.CatRescue.cs').stat().st_mtime, platform
    android = json.loads((OUT / 'android-verification.json').read_text())
    assert android['versionCode'] == 21 and android['samePackageAndSignerAsReference']
    release = ROOT / 'Builds/Windows-Catventure-Fix-Release-20261107'
    assert b'GameplaySmokeRunner' not in (release / 'Catventure_Data/Managed/Assembly-CSharp.dll').read_bytes()
    old = json.loads((BASE / 'Project/Assets/_Game/Resources/Content/CatRescue.json').read_text(encoding='utf-8'))
    new = json.loads((ROOT / 'Assets/_Game/Resources/Content/CatRescue.json').read_text(encoding='utf-8'))
    for a, b in zip(old['chapters'], new['chapters']):
        for key in ['cityId', 'catId', 'rescueNode', 'spriteIndex', 'rewardCoins', 'nextCity']:
            assert a[key] == b[key], key
        for key in ['challenges', 'supplies']:
            assert [x['id'] for x in a[key]] == [x['id'] for x in b[key]]
        assert a['catName'] not in json.dumps(b, ensure_ascii=False)
    artifacts = []
    for rel in ['Builds/Windows-Catventure-Fix-Release-20261107/Catventure.exe', 'Builds/Windows-Catventure-Fix-Release-20261107/Catventure_Data/Managed/Assembly-CSharp.dll', 'Builds/Android/Catventure-Fix-20261107.apk']:
        p = ROOT / rel
        artifacts.append(dict(path=rel, bytes=p.stat().st_size, sha256=sha(p)))
    rows = '\n'.join(f"| {r['folder']} | {r['checks']} | PASS |" for r in runs)
    report = f'''# Sửa tên mèo và vùng bấm — 07/11/2026

Các tên Đốm, Mây, Thông, Bori, Sao, Haneul do Codex tự đặt khi triển khai, không phải tên lấy từ PDF. Theo phản hồi của chủ game, đã bỏ toàn bộ tên riêng này khỏi nội dung đang chạy. Bộ sưu tập dùng nhãn theo thành phố (Mèo Huế, Mèo Hội An…), ô đích mê cung chỉ ghi **Mèo**.

Nút mèo phụ trên bản đồ trước đây có vùng bấm 44×48, lệch 30×30 so với điểm đến 52×52, tạo vùng đè lên nhau. Đã thay bằng hình mèo không nhận raycast, lệch ngang 60 đơn vị giao diện và giữ khoảng cách khi zoom. Điểm đến cũ giữ quyền nhận thao tác; nút Mèo trên thanh đầu màn hình mở hành trình. Âm thanh ở thanh dưới; kiểm tra hình chữ nhật màn hình xác nhận nút Mèo/Âm thanh không đè nút khác ở cả hai kích thước.

## Bản chạy hiện tại

- [Windows Release 1.1.1](../../Builds/Windows-Catventure-Fix-Release-20261107/Catventure.exe), giữ thư mục Data và DLL cạnh EXE.
- [Android APK 1.1.1, code 21](../../Builds/Android/Catventure-Fix-20261107.apk), cùng package và signer với bản code 20.
- [Ảnh mê cung Huế](../../Validation/Catventure-LabelsLayout-20261107/portrait-final/CAT-mode-maze.png) · [Thanh nút/bản đồ Huế](../../Validation/Catventure-LabelsLayout-20261107/portrait-final/CAT-Hue-map-controls.png).

## Kiểm tra bản sửa

| Lượt Windows Player | Kiểm tra | Kết quả |
|---|---:|---|
{rows}

Hai lượt đi đủ sáu thành phố và 24 thử thách ở 540×960 và 1280×720, có kiểm tra nhãn đích mê cung, vùng bấm thanh công cụ, hình mèo không chặn input, thao tác chuột/cảm ứng mô phỏng, lưu và thưởng một lần. Lượt save-upgrade nạp bản sao save QA của bản 1.1 trong process mới; không dùng save của chủ game. ID mèo/nhiệm vụ/vật dụng, thứ tự, điểm đến và số thưởng không đổi. Logic save và cứu hộ không sửa.

Đã kiểm tra thêm zoom 1×, 2×, 4×: animation chỉ chạy trên hình con, không ghi đè tỷ lệ của khung mèo. Các thư mục `portrait`, `desktop`, `save-upgrade` giữ lượt trước lần sửa zoom cuối, không cộng vào kết quả trên.

Windows Development, Release và Android build thành công. APK qua chữ ký, package/version, căn chỉnh 16 KB và CRC. **Chưa kiểm tra trên điện thoại thật**. Kết quả 633 Player/514 domain trong báo cáo trước thuộc bản 1.1, không cộng vào lượt này.

## Khôi phục

Base trước sửa: `Backups/CheckpointCatventure-20261107`. Bản sau sửa: `Backups/CheckpointCatventureLabelsLayout-20261107`, chỉ chứa file nguồn/tài liệu thay đổi; **cần base chain, không phải backup độc lập**. Không xóa hay ghi đè backup, bản chạy và bằng chứng cũ. Xem [manifest lượt sửa](../../Validation/Catventure-LabelsLayout-20261107/summary.json).
'''
    (ROOT / 'Docs/Catventure/LABELS_LAYOUT_FIX.md').write_text(report, encoding='utf-8')
    files = [p for top in ['Assets', 'Packages', 'ProjectSettings', 'Tools', 'Docs'] for p in (ROOT/top).rglob('*') if p.is_file() and not {'__pycache__', 'obj', 'bin'}.intersection(p.parts)]
    files += [p for p in ROOT.iterdir() if p.is_file() and p.suffix in ['.md', '.txt']]
    changed = []
    for p in files:
        entry = dict(path='Project/' + p.relative_to(ROOT).as_posix(), bytes=p.stat().st_size, sha256=sha(p))
        if baseline.get(entry['path'], {}).get('sha256') != entry['sha256']:
            changed.append((p, entry))
    deleted = [key for key in baseline if not (ROOT / key.removeprefix('Project/')).exists()]
    assert not deleted, deleted
    expected = {
        'Assets/_Game/Resources/Content/CatRescue.json', 'Assets/_Game/Scripts/Maps/CityMap.CatRescue.cs',
        'Assets/_Game/Scripts/UI/GameplayPanels.CatRescue.cs', 'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.CatRescue.cs',
        'Assets/_Game/Scripts/Editor/CatRescueBuild.cs', 'Tools/author_catventure.py', 'Tools/verify_catventure_apk.py',
        'Tools/finalize_catventure_labels_layout.py', 'Docs/Catventure/HANDOFF.md', 'Docs/Catventure/REPORT.md',
        'Docs/Catventure/LABELS_LAYOUT_FIX.md', 'Docs/QA/CURRENT_QA_STATUS.md',
    }
    assert {e['path'].removeprefix('Project/') for _, e in changed} <= expected, [e['path'] for _, e in changed]
    size = sum(e['bytes'] for _, e in changed)
    print('Incremental estimate:', len(changed), 'files;', size, 'bytes. No build/cache/history copies.')
    assert not DEST.exists(), 'Do not overwrite an existing checkpoint'
    DEST.mkdir()
    for p, e in changed:
        target = DEST / e['path']; target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(p, target); assert sha(target) == e['sha256']
    checkpoint = dict(type='INCREMENTAL_REQUIRES_BASE_CHAIN', standalone=False, createdUTC=datetime.now(timezone.utc).isoformat(), base=str(BASE), baseManifest='INCREMENTAL_MANIFEST.json', baseManifestSHA256=sha(BASE/'INCREMENTAL_MANIFEST.json'), baseVerified=True, summary='Owner feedback: remove invented cat names and duplicate map input; verified builds/QA recorded separately.', entries=[e for _, e in changed], deletedPaths=[])
    (DEST/'INCREMENTAL_MANIFEST.json').write_text(json.dumps(checkpoint, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    (DEST/'RESTORE.md').write_text('Incremental only. Restore the complete base chain first, then overlay Project/. Verify SHA256 values in INCREMENTAL_MANIFEST.json. No source deletion.\n', encoding='utf-8')
    summary = dict(createdUTC=checkpoint['createdUTC'], version='1.1.1-catventure-20261107', nativeRuns=runs, artifacts=artifacts, catLabels=[c['catName'] for c in new['chapters']], stableSaveIdsAndRewards=True, modifiedSource=[e for _, e in changed], deletedPaths=[], checkpoint=str(DEST), androidDevice='NOT_TESTED')
    (OUT/'summary.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print('PASS:', sum(r['checks'] for r in runs), 'Windows checks;', str(DEST))

if __name__ == '__main__':
    main()
