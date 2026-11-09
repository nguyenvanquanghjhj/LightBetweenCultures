# Dọn dung lượng an toàn — 2026-10-19

Hoàn tất đúng phạm vi: đo dung lượng, xóa hai thư mục Gradle intermediates đã hết sử dụng, báo cáo và dừng. Không chạy Unity, không build APK, không chạy Phase 7, không sửa game.

## Kết quả đo thực tế

Đơn vị: GB = 1.000.000.000 byte; GiB = 1.073.741.824 byte. Đây là tổng kích thước logic của file, không phải cộng chồng các thư mục cha/con. Không theo junction/symlink; cả hai lần quét không có link bị bỏ qua hoặc lỗi đọc.

| Phạm vi | Trước | Sau |
|---|---:|---:|
| Project | 28.816 GB / 26.837 GiB | 26.068 GB / 24.278 GiB |
| Backups ngoài project | 21.005 GB / 19.563 GiB | 21.005 GB / 19.563 GiB |
| Project + Backups | 46.400 GiB | 43.840 GiB |

- Đã xóa **2,751,755,777 byte = 2.752 GB / 2.563 GiB**, 1.571 file.
- Project giảm ròng 2.749 GB; chênh lệch so với số byte xóa do các file kiểm kê mới trong Docs/Storage. Số đo sau ở thời điểm `after.json`, trước khi xuất báo cáo cuối, sai số bổ sung chỉ vài KiB.
- Dung lượng trống ổ D tăng từ 201.823 lên 204.386 GiB, tăng 2.563 GiB; số này có thể chịu ảnh hưởng tiến trình khác trên ổ đĩa.
- Con số khoảng 46 GiB khớp **project + Backups**, riêng project trước khi dọn là 26,837 GiB.

## Đã xóa chính xác

| Thư mục tương đối trong project | GiB | Số file |
|---|---:|---:|
| `Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\build\intermediates` | 1.592 | 428 |
| `Library\Bee\Android\Prj\IL2CPP\Gradle\launcher\build\intermediates` | 0.970 | 1143 |

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
| 1 | `Library` | 13.308 |
| 2 | `Validation` | 11.729 |
| 3 | `Library\Bee` | 8.133 |
| 4 | `Library\Bee\Android\Prj\IL2CPP` | 4.012 |
| 5 | `Library\Bee\Android\Prj` | 4.012 |
| 6 | `Library\Bee\Android` | 4.012 |
| 7 | `Library\Bee\artifacts` | 3.724 |
| 8 | `Library\Bee\Android\Prj\IL2CPP\Gradle` | 3.463 |
| 9 | `Library\Bee\artifacts\Android` | 2.712 |
| 10 | `Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary` | 2.224 |
| 11 | `Library\DataStore` | 1.741 |
| 12 | `Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\build` | 1.592 |
| 13 | `Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\build\intermediates` | 1.592 |
| 14 | `Validation\CityMapUIPolish-20261017` | 1.466 |
| 15 | `Library\Bee\artifacts\Android\d8kzr` | 1.402 |

## 15 file lớn nhất trước khi dọn

| # | File | GiB |
|---|---|---:|
| 1 | `Library\Bee\Android\Prj\IL2CPP\Gradle\unityLibrary\build\intermediates\local_aar_for_lint\release\out.aar` | 0.623 |
| 2 | `Library\DataStore\UDSData_217.bin` | 0.572 |
| 3 | `Library\DataStore\UDSData_198.bin` | 0.524 |
| 4 | `Library\PlayerDataCache\Win642\Data\resources.assets.resS` | 0.498 |
| 5 | `Builds\Windows\LightBetweenCultures_Data\resources.assets.resS` | 0.498 |
| 6 | `Library\Bee\artifacts\objcopy_5lmi\libil2cpp.dbg.so` | 0.491 |
| 7 | `Library\Bee\artifacts\Android\il2cppOutput\build\arm64-v8a\libil2cpp.dbg.so` | 0.491 |
| 8 | `Library\Bee\artifacts\Android\d8kzr\libil2cpp.so` | 0.491 |
| 9 | `Library\DataStore\UDSData_10.bin` | 0.330 |
| 10 | `Library\Bee\Android\Prj\IL2CPP\Gradle\launcher\build\outputs\apk\release\launcher-release.apk` | 0.266 |
| 11 | `Builds\Android\LightBetweenCultures-Test.apk` | 0.266 |
| 12 | `Library\ArtifactDB` | 0.250 |
| 13 | `Library\Search\98957a664bd18c47a3e41b2a0189ef53.SearchIndexArtifactImporter.262146.b.index` | 0.174 |
| 14 | `Library\SourceAssetDB` | 0.125 |
| 15 | `Library\DataStore\UDSData_2.bin` | 0.104 |

## Dữ liệu lớn được giữ lại

| Nhóm | Dung lượng còn lại | Lý do |
|---|---:|---|
| Library | 10.745 GiB | Giữ import database, DataStore, PackageCache, mã native/cache build; tránh import/build lại lớn |
| Validation | 11.729 GiB | Ảnh, log, save QA, tuyến chủ dự án định vẽ, bằng chứng PASS/FAIL và dữ liệu tái hiện; chưa được duyệt bỏ lịch sử |
| Builds | 0.999 GiB | Windows Player hiện tại và APK PASS mới nhất |
| Logs | 0.359 GiB | Log nghiệm thu/build có giá trị đối chiếu; không coi toàn bộ log cũ là rác |
| Assets và source/art packs | Giữ nguyên | Nội dung canonical và nguồn gốc asset |
| `.utmp` | ~2,16 MiB | Nhỏ, native build scratch có thể liên quan incremental build; chưa cần dọn |
| `build` | <1 MiB | Chỉ log Gradle output, lợi ích nhỏ; giữ chẩn đoán |

Không có thư mục `Temp` hoặc `obj` ở gốc project. Những `obj` bên trong native cache không bị xóa. Không tạo full backup hoặc checkpoint mới vì không có thay đổi gameplay/source cần snapshot; hồ sơ dọn nằm tại Docs/Storage.

## Backups ngoài project — chưa xóa

Root: `D:\GAMEKKK\Backups`, **19.563 GiB**, 92 thư mục. Bảng đầy đủ: [backups.csv](backups.csv), phân loại manifest và chuỗi phụ thuộc: [backup-catalog.json](backup-catalog.json).

| 15 backup lớn nhất | GiB | Quyết định |
|---|---:|---|
| `CheckpointPortraitDialogueStoryAudit-20261017` | 3.209 | Giữ; chưa duyệt xóa |
| `CheckpointFullStoryComplete-20261017` | 2.248 | BASE CHAIN — giữ bắt buộc |
| `CheckpointTerrainRoutes-20261017` | 2.223 | Giữ; chưa duyệt xóa |
| `CheckpointCityMapUIPolish-20261017` | 1.802 | Giữ; chưa duyệt xóa |
| `CheckpointHueJeonjuGameplay-20261017` | 1.515 | Giữ; chưa duyệt xóa |
| `CheckpointFullStoryAuthored-20261017` | 1.240 | Giữ; chưa duyệt xóa |
| `BeforeCityMapUIPolish-20261017` | 1.114 | Giữ; chưa duyệt xóa |
| `CheckpointDaLatChapter-20261017` | 1.114 | Giữ; chưa duyệt xóa |
| `CheckpointCityMapNatural-20261016` | 0.884 | Giữ; chưa duyệt xóa |
| `CheckpointCrossCityFood-20261016` | 0.701 | Giữ; chưa duyệt xóa |
| `CheckpointArtLibrary-20261016` | 0.615 | Giữ; chưa duyệt xóa |
| `CheckpointRenderQuality-20261017` | 0.252 | BASE CHAIN — giữ bắt buộc |
| `CheckpointPhase7Final-20261019` | 0.183 | BASE CHAIN — giữ bắt buộc |
| `CheckpointMapRoute35-20261018` | 0.154 | BASE CHAIN — giữ bắt buộc |
| `CheckpointPhase4Final-20261018` | 0.154 | BASE CHAIN — giữ bắt buộc |

Checkpoint mới nhất `CheckpointAndroidAppIcon-20261019` phụ thuộc một chuỗi 35 checkpoint kể cả chính nó; base cuối là `CheckpointFullStoryComplete-20261017`. Giữ nguyên chuỗi. Không suy diễn rằng checkpoint là bản sao đầy đủ từ tên thư mục. Các thư mục không có manifest gốc được ghi `NO_ROOT_MANIFEST`, chưa xác nhận đủ khả năng phục hồi.

## Cần chủ dự án quyết định nếu muốn dọn thêm

1. **Lịch sử QA**: 11,729 GiB. Chọn đợt nào được phép bỏ, rồi kiểm tra liên kết báo cáo/manifest, save và ảnh quan trọng trước. Hiện chưa tính toàn bộ số này là có thể tiết kiệm an toàn.
2. **Các full snapshot cũ ngoài chuỗi base mới nhất**: `CheckpointPortraitDialogueStoryAudit-20261017` (3,209 GiB), `CheckpointTerrainRoutes-20261017` (2,223 GiB), `CheckpointCityMapUIPolish-20261017` (1,802 GiB), `CheckpointFullStoryAuthored-20261017` (1,240 GiB). Tổng khoảng 8,474 GiB nếu chủ dự án quyết định bỏ cả bốn **sau kiểm tra phụ thuộc và nội dung độc nhất riêng**. Chưa xác nhận an toàn để xóa nguyên các checkpoint này.
3. Manifest năm full snapshot ghi nhận tổng **6.620 GiB** nội dung trùng SHA256+kích thước giữa các lần sao lưu. Đây là ước lượng độ trùng theo manifest đã có, không rehash 10.000+ file và **không phải dung lượng chắc chắn xóa được**. Mỗi snapshot vẫn có thể chứa tài liệu/source/ảnh lịch sử độc nhất.
4. **Cache Library còn lại**: có thể cân nhắc native/Bee cache nếu thật sự cần thêm chỗ và chấp nhận build sau chậm hơn; chưa xóa thêm. Không đề nghị xóa toàn bộ Library khi ổ D hiện còn khoảng 204.386 GiB trống.
5. **Bản APK trùng trong Gradle outputs**: 272,61 MiB, hash khớp bản PASS. Vẫn giữ để bảo toàn đầu ra build; chưa đụng APK nào.

Dừng tại đây. Chưa thực hiện bất kỳ mục đề xuất nào.
