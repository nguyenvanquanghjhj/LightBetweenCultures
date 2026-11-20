# Công cụ hiện hành

Project root được xác định từ vị trí script; chạy từ thư mục nào cũng được. Build dùng Unity 6000.6.5f1 của ProjectVersion.txt. Truyền `--editor` hoặc `UNITY_EDITOR` nếu Editor nằm ở nơi khác.

| Nhóm | Vai trò |
| --- | --- |
| Build | `build.py windows/android` tạo output QA mới dưới workspace `_LBC_WORK`; `verify_android_apk.ps1` đọc APK và ghi kết quả vào thư mục mới do người gọi chỉ định. Không chạy setup/generator và không ghi đè deliverables. |
| Import | `art_asset_catalog.py --inventory-only`, kiểm tra metadata CatRescue, công cụ route editor và công thức SFX. `Sources/` chứa đầu vào authoring được giữ trong repo; build game không phụ thuộc Python/Pillow/NumPy hoặc backup ngoài repo. |
| QA | `run_catventure_domain.py` và project .NET adapter CatRescueTests. Đây là domain tests, không thay thế Unity Player. |
| Maintenance | Hai validator workspace/project, inventory, xuất cây, kiểm tra chain phục hồi và các migration một lượt có guard. |

Các tool đóng băng theo checkpoint/phase cũ ở `../_LBC_ARCHIVE/HistoricalSources`, không còn là entry point đang dùng. File/manifest QA gốc không bị viết lại để giả kết quả mới.

```powershell
python Tools/Maintenance/verify_workspace_layout.py
python Tools/Maintenance/verify_canonical_layout.py
python Tools/Import/art_asset_catalog.py --inventory-only
python Tools/Build/build.py windows --editor 'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe'
python Tools/Build/build.py android --editor 'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe'
```

Native audit: `LightBetweenCultures.Editor.WorkspaceLayoutSetup.Verify`, với `--asset-audit-output=<thư mục mới>` và `--route-refactor-output=<JSON mới>`. Kiểm tra này cần baseline của đợt migration trong Docs/Development/Storage; build thông thường không đọc baseline hoặc kho archive.
