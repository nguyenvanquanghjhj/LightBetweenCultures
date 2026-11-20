# Light Between Cultures

Game phiêu lưu 2D nối văn hóa Việt Nam và Hàn Quốc qua Hội An, Huế, Đà Lạt, Jeonju, Gyeongju và Seoul. Người chơi khám phá 133 node, làm nhiệm vụ, giải đố, phục hồi sáu di vật và kết nối ánh sáng tại Seoul. Catventure bổ sung sáu mèo và 24 thử thách văn hóa.

## Bản lưu trữ 2026

Source được giữ để tiếp tục phát triển. Đây chưa phải bản nghiệm thu toàn bộ gameplay hoặc Android trên thiết bị. Bản nộp Windows/APK 11/11/2026 được giữ nguyên; chúng không tự cập nhật theo source sau tái cấu trúc 13/11.

- [Repository chính](https://github.com/nguyenvanquanghjhj/LightBetweenCultures), nhánh `main`, tag đóng băng dự kiến `lbc-final-archive-2026-r1`.
- [Releases source](https://github.com/nguyenvanquanghjhj/LightBetweenCultures/releases).
- [Kho archive private](https://github.com/nguyenvanquanghjhj/LightBetweenCultures-archive): checkpoint, backup, build, bản nộp, bản nháp và QA. Cần tài khoản có quyền truy cập.
- [Hướng dẫn khôi phục](Docs/Release/RESTORE_FINAL_2026.md), [kiến trúc](Docs/Architecture/ARCHIVE_OVERVIEW_2026.md), [hồ sơ kiểm chứng cuối](Docs/Development/Storage/FinalArchive-20261120/).

## Môi trường và phụ thuộc

Unity **6000.6.5f1**, revision **3ff58d469c8a**, theo `ProjectSettings/ProjectVersion.txt`. Cài Windows Build Support. Android cần Android Build Support, SDK/NDK/OpenJDK tương ứng qua Unity Hub. Python 3.12 phục vụ công cụ; .NET 8 chỉ cần cho regression domain.

Giữ [manifest.json](Packages/manifest.json) và [packages-lock.json](Packages/packages-lock.json). Package chính: URP 17.6.0, Input System 1.20.0, uGUI 2.6.0, 2D Animation 16.0.1, Test Framework 1.8.0, AI Assistant 2.20.0-pre.2, AI Inference 2.6.1. Package Manager cần truy cập registry khi mở project sạch. Không tự nâng Editor/package trong lần khôi phục đầu; dịch vụ AI/registry bên ngoài không được bảo đảm tồn tại vĩnh viễn.

## Tải và mở

```powershell
git clone https://github.com/nguyenvanquanghjhj/LightBetweenCultures.git
cd LightBetweenCultures
git checkout lbc-final-archive-2026-r1
```

Mở thư mục clone bằng Unity Hub với đúng phiên bản, chờ import/C# compile. Mở `Assets/_Game/Scenes/Prototype.unity`, nhấn Play. Giữ thứ tự scene trong Build Settings. Không chạy generator Phase/setup/import để khởi động project đã có dữ liệu. Save thật: `Application.persistentDataPath/hoi-an-v1.json`; giữ bản sao riêng trước khi thử.

## Cấu trúc

| Thư mục | Nội dung |
|---|---|
| `Assets/_Game/Art`, `Audio` | Hình, sprite, atlas, âm thanh |
| `Resources/Catalogs`, `Resources/Content` bên dưới `_Game` | Catalog ScriptableObject; dữ liệu thành phố, quest, puzzle, di vật, Catventure |
| `Assets/_Game/Scenes`, `Prefabs`, `Rendering` | Scene, prefab, input và render pipeline |
| `Assets/_Game/Scripts` | Core, Gameplay, Maps, UI, Persistence, Editor, Validation |
| `Packages`, `ProjectSettings` | Phiên bản phụ thuộc và cấu hình build |
| `Tools/Build`, `Import`, `QA`, `Maintenance` | Tool hiện hành và verifier có guard |
| `Docs` | Kiến trúc, thiết kế, art provenance, QA và hồ sơ bảo toàn |

Asset/folder Unity đi cùng `.meta`; không tạo lại GUID. Workspace còn có `_LBC_ARCHIVE`, `_LBC_DELIVERABLES`, `_LBC_WORK` ở cạnh project; lấy lại từ Releases private. Cache và output build không commit vào source.

Công cụ kiểm kê art có kiểm tra provenance từ `_LBC_ARCHIVE`; để chạy đầy đủ công cụ đó, khôi phục kho lịch sử private cạnh project. Clone source riêng vẫn đủ để Unity import, compile và build; không tự nhập art lịch sử vào Assets để sửa lỗi thiếu provenance của công cụ.

## Build Windows và Android

Trong Editor, mở Build Profiles/Build Settings, chọn platform và toàn bộ scene enabled hiện có, chọn output mới. Windows Release tắt Development Build. Giữ nguyên toàn bộ folder cạnh EXE khi đóng gói.

CLI hiện hành, chạy từ project:

```powershell
python Tools/Build/build.py windows --run-id Restore-Windows-2026
python Tools/Build/build.py android --run-id Restore-Android-2026
```

Output dưới `../_LBC_WORK/QA/LightBetweenCultures/Build-<run-id>/<platform>/`; Windows là Development Player, Android là APK với report riêng. Tool từ chối ghi đè output đã có. Android cần kiểm tra bằng `Tools/Build/verify_apk.py`, rồi cài/chơi trên thiết bị QA. Source hiện không cấu hình custom keystore; debug signing không bảo đảm cập nhật APK ký bằng khóa khác.

## Hạn chế còn giữ

- PASS trong báo cáo cũ chỉ áp dụng cho source/build/phạm vi được nêu ở đó.
- Hồ sơ 13/11 ghi lỗi Editor journey `MiniMapRoads`/`CanvasRenderer`; template render có 7 GUID và 6 object reference lỗi baseline. Xem hồ sơ cuối để biết kết quả chạy lại. `AppearanceReviewData` là fixture QA tạo theo workflow riêng.
- Android thật, hình ảnh và trải nghiệm người chơi độc lập chưa được chứng minh bởi build/checksum.
- `CheckpointBeforeFTUE-20261109-152433` chỉ còn Assets, thiếu manifest; không dùng riêng như standalone backup. Mọi byte còn lại vẫn được lưu.
- Không chạy lại migration/deletion lịch sử. Giữ [AGENTS.md](AGENTS.md) và manifest gốc; không xóa class Hotfix/Phase/Prototype theo tên.

[Bản đồ code](Docs/Architecture/CODEBASE_MAP.md) · [Tra cứu tính năng](Docs/Architecture/FEATURE_INDEX.md) · [Tái cấu trúc trước lưu trữ](Docs/Architecture/FULL_WORKSPACE_RESTRUCTURE.md).
