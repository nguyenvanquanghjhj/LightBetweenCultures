"""Icon-only diff verification, preview/report and small incremental checkpoint."""
from pathlib import Path
import datetime,hashlib,json,re,shutil
R=Path(__file__).resolve().parents[1];Q=R/'Docs/QA/AndroidIcon-20261019';A=R/'Assets/_Game/Art/UI/AppIcon'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def main():
    b=read(Q/'baseline.json');changed=[]
    for e in b['entries']:
        p=R/e['path'];assert p.is_file()
        if sha(p)!=e['sha256']:changed.append(e['path'])
    assert changed==['ProjectSettings/ProjectSettings.asset'],changed
    before=(Q/'ProjectSettings.before.asset').read_text(encoding='utf-8-sig');after=(R/'ProjectSettings/ProjectSettings.asset').read_text(encoding='utf-8-sig')
    iconBlock=r'^  m_BuildTargetIcons:.*?(?=^  m_BuildTargetBatching:)'
    assert re.sub(iconBlock,'ICON_BLOCK\n',before,flags=re.M|re.S)==re.sub(iconBlock,'ICON_BLOCK\n',after,flags=re.M|re.S),'Non-icon PlayerSettings change'
    assert sha(Path(b['source']))==sha(R/b['importedSource'])==b['sourceSHA256']
    binding=read(Q/'icon-bindings.json');assert not binding['apkBuilt']
    assert len(binding['slots'])==7 and sum(s['kind']=='Adaptive' for s in binding['slots'])==6
    rows=[]
    for p in sorted(A.glob('*.png')):
        meta=Path(str(p)+'.meta');text=meta.read_text(encoding='utf-8-sig');guid=re.search(r'^guid: (\w+)',text,re.M)[1]
        assert 'enableMipMap: 0' in text
        rows.append({'path':p.relative_to(R).as_posix(),'sha256':sha(p),'guid':guid,'localID':2800000,'role':'owner original preserved' if 'Original' in p.name else 'icon-only fit/padding derivative','status':'UNITY_ICON_BINDING_VERIFIED; PHONE_NOT_TESTED'})
    write(Q/'icon-manifest.json',{'assets':rows,'existingFilesChanged':changed,'nonIconSettingsChanged':False,'originalPreserved':True,'protectedFiles':len(b['entries']),'slots':binding['slots'],'APKBuild':'NOT_RUN','phone':'NOT_TESTED'})
    (Q/'preview.html').write_text('''<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC Android icon review</title><style>body{font:16px Segoe UI;background:#17232d;color:#eee;padding:25px}section{display:inline-block;margin:15px;vertical-align:top}img{display:block;margin:12px}a{color:#edcd8a}</style><h1>Icon Android — xem trước</h1><p>Tranh gốc giữ nguyên. Chỉ fit/padding, không vẽ lại. Đây là mô phỏng mặt nạ, chưa phải ảnh trên điện thoại. Chưa build APK.</p><section><h2>Icon thường</h2><img src="Legacy-square.png" width="192"><img src="Legacy-square.png" width="48"></section><section><h2>Mặt nạ tròn</h2><img src="Legacy-round.png" width="192"><img src="Legacy-round.png" width="48"></section><section><h2>Adaptive tròn</h2><img src="Adaptive-circle.png" width="192"><img src="Adaptive-circle.png" width="48"></section><section><h2>Adaptive squircle</h2><img src="Adaptive-squircle.png" width="192"><img src="Adaptive-squircle.png" width="48"></section><p>Giữ Chùa Cầu và tháp Seoul trong phần giữa; rìa tranh có thể bị launcher mask. Foreground không giả tách công trình khỏi nền, tránh tạo seam hoặc đổi nội dung tranh.</p></html>''',encoding='utf-8')
    report='''# Android app icon — 2026-10-19

## Đã cấu hình, chưa build APK

Nguồn đúng ảnh chủ dự án gửi: `C:/Users/admin/Downloads/cd0e3fa0-1dd0-405a-826d-0a2691d1a095.png`,941×942RGBA. Bản gốc ở Downloads không sửa; bản `Assets/_Game/Art/UI/AppIcon/LBC_AppIcon_Original.png` có SHA256 giống hệt.

Chỉ đóng gói kỹ thuật bằng UnityEditor: fit giữ tỷ lệ, không crop tranh nguồn, không kéo méo, không vẽ lại/AI, không sharpen hay đổi màu nội dung. Icon512×512 được giảm từ nguồn941×942; không tuyên bố tăng độ phân giải. Nền đệm xanh tímRGB43/49/79 đồng bộ bầu trời.

- `LBC_Android_LegacyRound.png`:512×512opaque, tranh chiếm84%cạnh dài; dùng Default Application Icon/fallback. Default serializedslot128×128 tham chiếu texture512, Unity tự tạo kích thước cần thiết. Default icon cũng là fallback icon của nền tảng khác; không đổi tên ứng dụng hay package.
- `LBC_Android_AdaptiveBackground.png`:512×512opaque, nền màu đồng bộ.
- `LBC_Android_AdaptiveForeground.png`:512×512RGBA, tranh chiếm53%cạnh dài, ngoài tranh trong suốt. Padding giúp Chùa Cầu và tháp Seoul giữ trong vùng giữa của mặt nạ. Không tách/vẽ thêm vật thể hoặc nhân vật.
- TextureDefault, NPOTNone, max1024, uncompressed, bilinear, clamp, không mipmaps; nguồn941×942 không bị ép thành power-of-two. GUID/.meta mới ổn định; không ghi đè asset cũ.

Unity6000.6.5f1/cấu hìnhAndroidhiện tại chỉ trả supportedkindAdaptive. Đã gán6slot432/324/216/162/108/81, hai lớp **Background,Foreground** theo metadata thật của Unity, cùng1DefaultApplicationIcon. Không tự tạo legacy/round platformslot không được provider hỗ trợ. Launcher lấy mặt nạ tròn/squircle từ adaptive; bản icon thường là defaultfallback. Nguồn API: [Unity SetPlatformIcons](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PlayerSettings.SetPlatformIcons.html). Vùng an toàn adaptive cần xét theo mask/lớp: [Android adaptive icons](https://developer.android.com/develop/ui/compose/system/icon_design_adaptive).

## Kiểm tra

Editor compile/configure thành công, exit0; đọc lại7binding hợp lệ. Xem các PNGpreviewtròn/squircle: giữ Chùa Cầu, tháp Seoul; mép ngoài phụ vẫn có thể bị launcher mask. Preview256 và HTML192/48px là mô phỏng, không phải phonePASS. Ảnh cảnh nhiều chi tiết nên tại48px chi tiết phụ nhỏ; giữ đúng artwork chủ dự án, không tự làm logo khác.

467fileđượcđối chiếu: chỉProjectSettings/ProjectSettings.asset thay đổi; phần diff nằm hoàn toàn trong hai khối icon.466filekhácgiữnguyên, bao gồm gameplay/node/route/quest/NPC/relic/ending/audio/save. Không gọiBuildPipeline, không build APK. APK đã cài trên điện thoại vẫn dùng icon cũ cho tới khi có bản build/cập nhật sau.

Lượt cấu hình đầu phát hiện giả định3supportedkinds không đúng provider hiện tại; đổi công cụ để dùng DefaultIcon+Adaptive thật. Không nâng Unity, đổi minSDK, package hay signing để cố tạo slot cũ.

## File

`Assets/_Game/Art/UI/AppIcon/` (4PNGvàmeta), `Assets/_Game/Scripts/Editor/AndroidAppIconSetup.cs`(+meta), `ProjectSettings/ProjectSettings.asset`; công cụ báo cáo/checkpoint `Tools/android_icon_handoff.py`. Bằng chứng `Docs/QA/AndroidIcon-20261019/` gồm icon-bindings.json, icon-manifest.json, settingsdiff, preview.html và4PNGpreview. Log `Logs/android-icon-20261019-setup02.log`.

Không sửa đường đi trong lượt này. Theo quyết định mới của chủ dự án, bỏ qua công cụ route phức tạp: chủ dự án vẽ trực tiếp lên nền và gửi ảnh; Codex đối chiếu/áp dụng waypoint ở nhiệm vụ sau. Không yêu cầu người dùng ghi EdgeID. Các đoạn không đánh dấu được giữ nguyên.
'''
    (R/'Docs/QA/ANDROID_APP_ICON_20261019.md').write_text(report,encoding='utf-8')
    base=b['baseCheckpoint'];B=Path(base['checkpoint']);mf=B/'CHECKPOINT_MANIFEST.json';assert sha(mf)==base['manifestSHA256'];bm=read(mf)
    for e in bm['entries']:assert sha(B/e['path'])==e['sha256']
    for e in bm['baseChain']:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
    T=Path('D:/GAMEKKK/Backups/CheckpointAndroidAppIcon-20261019');assert not T.exists()
    files={p.relative_to(R).as_posix() for p in A.iterdir() if p.is_file()}
    files.update(p.relative_to(R).as_posix() for p in Q.iterdir() if p.is_file() and p.name!='checkpoint-receipt.json')
    files.update(['Assets/_Game/Art/UI/AppIcon.meta','Assets/_Game/Scripts/Editor/AndroidAppIconSetup.cs','Assets/_Game/Scripts/Editor/AndroidAppIconSetup.cs.meta','ProjectSettings/ProjectSettings.asset','Tools/android_icon_handoff.py','Docs/QA/ANDROID_APP_ICON_20261019.md','Logs/android-icon-20261019-setup.log','Logs/android-icon-20261019-setup02.log'])
    entries=[{'path':p,'sha256':sha(R/p),'bytes':(R/p).stat().st_size} for p in sorted(files)];total=sum(e['bytes'] for e in entries)
    print('Incremental estimate',len(entries),'files',round(total/1048576,2),'MiB');assert total<16*1048576
    T.mkdir()
    for e in entries:
        p=T/e['path'];p.parent.mkdir(exist_ok=True,parents=True);shutil.copy2(R/e['path'],p);assert sha(p)==e['sha256']
    (T/'RESTORE.md').write_text('Incremental, NOT standalone. Requires CheckpointSixCityRouteEditor-20261019 and its full baseChain. Restore into separate workspace oldest to newest, overlay these entries preserving meta, verify hashes. Only app icon assets/Editor helper/icon settings and QA. No deleted paths, APK build or route/gameplay change. Owner draws routes directly on map images in a later task.\n',encoding='utf-8')
    manifest={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(B),'baseManifestSHA256':sha(mf),'baseChain':[{'path':str(B),'manifestSHA256':sha(mf)}]+bm['baseChain'],'entries':entries,'bytes':total,'deletedPaths':[],'restoreGuideSHA256':sha(T/'RESTORE.md'),'summary':'Owner PNG app icon; default+Android adaptive6slots; no APK build, gameplay/route unchanged'}
    write(T/'CHECKPOINT_MANIFEST.json',manifest)
    write(Q/'checkpoint-receipt.json',{'checkpoint':str(T),'manifestSHA256':sha(T/'CHECKPOINT_MANIFEST.json'),'standalone':False,'baseCheckpoint':str(B),'baseManifestSHA256':sha(mf),'files':len(entries),'bytes':total,'MiB':round(total/1048576,2),'baseAndChainVerified':True,'copiedFilesVerified':True,'deletedPaths':[],'apkBuilt':False})
    print('Verified: icon-only PlayerSettings delta; original source preserved; no gameplay/route changes; no APK build.')
if __name__=='__main__':main()
