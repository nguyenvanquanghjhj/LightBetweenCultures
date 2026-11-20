# LBC — Dọn dung lượng an toàn, 17/10/2026

## Kết quả

Đã xóa **1.857 PNG QA trung gian**, **1.370.135.897 byte**. Tổng dung lượng file trong D:\GAMEKKK giảm từ **34,801 GB xuống 33,442 GB**, giảm ròng **1.358.682.867 byte** tại mốc đo sau dọn và mở Unity. Dung lượng trống ổ D tăng quan sát ngay sau xóa: **1.365.823.488 byte**. Chênh lệch do báo cáo/hash kiểm toán mới, log và cache cập nhật khi mở Unity.

GB = 10^9 byte. Dung lượng thư mục là tổng byte file logic, không phải kích thước cấp phát NTFS. Các số được chụp trước khi xuất CSV/báo cáo cuối, phát sinh thêm dưới 1 MB.

**Không xóa backup, checkpoint, save, source, art, metadata Unity, tài liệu, build cuối hoặc cache.** Không sửa gameplay. Full Story, terrain chưa nghiệm thu và H3 đang chờ được giữ nguyên. Không tạo thêm bản sao project.

[Danh sách chính xác file đã xóa, SHA256 và lượt thay thế PASS](deleted-files.csv) · [Bản ghi JSON](deleted-files.json) · [Kiểm kê backup CSV](backup-audit.csv) · [Metadata/thành phần/khác biệt từng backup](backup-audit.json) · [Đề xuất chưa thực hiện](backup-delete-proposal.json).

## Kiểm kê các vùng chính

| Thư mục | Trước GB | Sau GB | Phân loại / quyết định |
|---|---:|---:|---|
| Backups | 18.839 | 18.839 | RESTORE_POINT / UNCERTAIN — giữ 39 backup |
| LightBetweenCultures/Assets | 0.148 | 0.148 | ESSENTIAL — nguyên vẹn |
| LightBetweenCultures/Library | 4.232 | 4.233 | REGENERABLE — chưa dọn |
| LightBetweenCultures/Builds | 0.561 | 0.561 | ESSENTIAL — build Windows cuối |
| LightBetweenCultures/Validation | 10.643 | 9.273 | ESSENTIAL cho bằng chứng cuối; chỉ ảnh dư là REDUNDANT |
| LightBetweenCultures/Logs | 0.251 | 0.251 | ESSENTIAL/UNCERTAIN — giữ log chẩn đoán |
| LightBetweenCultures/Temp | 0.000 | 0.000 | REGENERABLE — không tồn tại |
| LightBetweenCultures/obj | 0.000 | 0.000 | REGENERABLE — không tồn tại |

Ngoài project và Backups không có bản sao dự án khác trực tiếp dưới D:\GAMEKKK. Các bộ LBC, DesignKit, References và UserSettings còn nguyên. Tổng trước kiểm kê: **107.267 file**. Không có lỗi truy cập khi quét; công cụ không đi theo symlink/junction.

## 20 thư mục con lớn nhất

Xếp theo dung lượng trước dọn. Các hàng cha/con chồng lấn; không cộng các hàng thành tổng.

| Thư mục | Trước GB | Sau GB |
|---|---:|---:|
| Backups | 18.839 | 18.839 |
| LightBetweenCultures\Validation | 10.643 | 9.273 |
| LightBetweenCultures\Library | 4.232 | 4.233 |
| Backups\CheckpointPortraitDialogueStoryAudit-20261017 | 3.446 | 3.446 |
| Backups\CheckpointPortraitDialogueStoryAudit-20261017\Validation | 2.551 | 2.551 |
| Backups\CheckpointFullStoryComplete-20261017 | 2.413 | 2.413 |
| Backups\CheckpointTerrainRoutes-20261017 | 2.387 | 2.387 |
| LightBetweenCultures\Validation\CityMapUIPolish-20261017 | 2.076 | 1.574 |
| Backups\CheckpointCityMapUIPolish-20261017 | 1.935 | 1.935 |
| LightBetweenCultures\Validation\PortraitStory-20261017 | 1.785 | 1.397 |
| Backups\CheckpointHueJeonjuGameplay-20261017 | 1.627 | 1.627 |
| Backups\CheckpointTerrainRoutes-20261017\Validation | 1.508 | 1.508 |
| LightBetweenCultures\Validation\FullStory-20261017 | 1.495 | 1.495 |
| Backups\CheckpointFullStoryComplete-20261017\Validation | 1.492 | 1.492 |
| Backups\CheckpointFullStoryComplete-20261017\Validation\FullStory-20261017 | 1.492 | 1.492 |
| Backups\CheckpointFullStoryAuthored-20261017 | 1.332 | 1.332 |
| LightBetweenCultures\Library\PackageCache | 1.304 | 1.304 |
| LightBetweenCultures\Validation\DaLatChapter-20261017 | 1.294 | 1.009 |
| Backups\BeforeCityMapUIPolish-20261017 | 1.196 | 1.196 |
| Backups\CheckpointDaLatChapter-20261017 | 1.196 | 1.196 |

## Phần đã dọn

Chỉ xóa từng PNG có đường dẫn tuyệt đối và SHA256 đã kiểm tra; không xóa đệ quy thư mục. Các lượt thay thế có kết quả ALL CHECKS PASSED. Giữ ảnh được dẫn trong báo cáo/manifest, ảnh trước/sau tham chiếu, ảnh lỗi có tên và các frame đầu/cuối/ghi cuối cùng. Giữ toàn bộ log và save, kể cả lượt FAIL. Không đụng tới FullStory, TerrainRoutes, Art hoặc sprite nguồn.

| Nhóm ảnh trung gian | Số file | Byte đã xóa |
|---|---:|---:|
| CityMapUIPolish-20261017 | 643 | 501863052 |
| PortraitStory-20261017 | 465 | 388106104 |
| DaLatChapter-20261017 | 407 | 284960567 |
| HueJeonjuChapters-20261017 | 342 | 195206174 |

## Tình trạng checkpoint

Đã đọc cả CHECKPOINT_MANIFEST.json, checkpoint-manifest.json và manifest.json; đối chiếu **24.685 entry manifest**, không thiếu/sai hash. Hash trước/sau của **29.873 file trong 39 backup** giống nhau.

Manifest không tự chứng minh một bản khôi phục đầy đủ. Snapshot nhỏ/overlay cần base; thư mục chỉ có Assets/Packages/ProjectSettings nhưng thiếu manifest đầy đủ được ghi UNCERTAIN. Không mở/build từng checkpoint cũ trong lượt này. Thành phần, mốc source/gameplay và ngày tạo UTC từ manifest có trong backup-audit.json; bảng dưới dùng ngày tạo thư mục local.

Giữ bắt buộc:
- **CheckpointFullStoryComplete-20261017**: source/build/evidence cuối, 2,413 GB.
- **CheckpointPortraitDialogueStoryAudit-20261017**: mốc độc lập trước Full Story, 3,446 GB.
- Giữ tất cả mốc khác, gồm CrossCityFood, CityMapNatural, ArtLibrary và BeforeHueJeonjuChapters.

Bản Complete khôi phục source/build và bằng chứng chọn lọc theo manifest; Library cần tái tạo. Checkpoint mới có thêm gameplay không đồng nghĩa đã lưu mọi byte source/art/bằng chứng lịch sử của checkpoint cũ.

| Backup | Ngày tạo local | GB | Phạm vi khôi phục | Hash manifest đã kiểm | File / GB độc nhất giữa các backup |
|---|---|---:|---|---:|---|
| BeforeAdventureUI-20261016 | 2026-10-16 | 0.001 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| BeforeArtLibrary-20261016 | 2026-10-16 | 0.079 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 4 / 0.000 GB |
| BeforeAudit-20261015 | 2026-10-15 | 0.005 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 12 / 0.000 GB |
| BeforeCityMapNatural-20261016 | 2026-10-16 | 0.073 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| BeforeCityMapPolish-20261015 | 2026-10-15 | 0.000 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 10 / 0.000 GB |
| BeforeCityMapUIPolish-20261017 | 2026-10-17 | 1.196 | Có manifest; không khẳng định standalone | 1578 | 1 / 0.000 GB |
| BeforeCrossCityFood-20261016 | 2026-10-16 | 0.073 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| BeforeDaLatChapter-20261017 | 2026-10-17 | 0.112 | Có manifest; không khẳng định standalone | 481 | 1 / 0.000 GB |
| BeforeDesignKit-20261015 | 2026-10-15 | 0.005 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 10 / 0.000 GB |
| BeforeExplorationMarkers-20261016 | 2026-10-16 | 0.061 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 9 / 0.000 GB |
| BeforeGameplayAB-20261016 | 2026-10-16 | 0.010 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 3 / 0.000 GB |
| BeforeHoiAnMapPhase1-20261015 | 2026-10-15 | 0.007 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 6 / 0.000 GB |
| BeforeHueJeonjuChapters-20261016 | 2026-10-16 | 0.073 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 1 / 0.000 GB |
| BeforeMapActorsStage1-20261016 | 2026-10-16 | 0.058 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| BeforeMarkerSprites-20261016 | 2026-10-16 | 0.066 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 3 / 0.000 GB |
| BeforeMilestone01-20261016 | 2026-10-16 | 0.007 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 9 / 0.000 GB |
| BeforeNodeMarkers-20261016 | 2026-10-16 | 0.061 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 4 / 0.000 GB |
| BeforeSpecialtyItems-20261016 | 2026-10-16 | 0.071 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 4 / 0.000 GB |
| BeforeUI1ArtDirection-20261016 | 2026-10-16 | 0.055 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| CheckpointAdventureUI-20261016 | 2026-10-16 | 0.055 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 3 / 0.000 GB |
| CheckpointArtLibrary-20261016 | 2026-10-16 | 0.660 | Có manifest; không khẳng định standalone | 1051 | 245 / 0.230 GB |
| CheckpointCityArtControl-20261016 | 2026-10-16 | 0.000 | Có manifest; không khẳng định standalone | 7 | 1 / 0.000 GB |
| CheckpointCityMapNatural-20261016 | 2026-10-16 | 0.949 | Có manifest; không khẳng định standalone | 1400 | 495 / 0.482 GB |
| CheckpointCityMapUIPolish-20261017 | 2026-10-17 | 1.935 | RESTORE_POINT: full source/build, manifest PASS | 2541 | 12 / 0.012 GB |
| CheckpointCrossCityFood-20261016 | 2026-10-16 | 0.753 | Có manifest; không khẳng định standalone | 1284 | 362 / 0.409 GB |
| CheckpointDaLatChapter-20261017 | 2026-10-17 | 1.196 | Có manifest; không khẳng định standalone | 1578 | 1 / 0.000 GB |
| CheckpointFullStoryAuthored-20261017 | 2026-10-17 | 1.332 | RESTORE_POINT: full source/build, manifest PASS | 1729 | 21 / 0.013 GB |
| CheckpointFullStoryComplete-20261017 | 2026-10-17 | 2.413 | RESTORE_POINT: full source/build, manifest PASS | 3187 | 986 / 0.737 GB |
| CheckpointGameplayA-20261016 | 2026-10-16 | 0.001 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 6 / 0.000 GB |
| CheckpointGameplayAB-20261016 | 2026-10-16 | 0.010 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 1 / 0.000 GB |
| CheckpointHueChapter-20261017 | 2026-10-17 | 0.000 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 1 / 0.000 GB |
| CheckpointHueJeonjuGameplay-20261017 | 2026-10-17 | 1.627 | Có manifest; không khẳng định standalone | 2317 | 999 / 1.046 GB |
| CheckpointJeonjuChapter-20261017 | 2026-10-17 | 0.000 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 1 / 0.000 GB |
| CheckpointPortraitDialogueStoryAudit-20261017 | 2026-10-17 | 3.446 | RESTORE_POINT: full source/build, manifest PASS | 4457 | 861 / 1.038 GB |
| CheckpointTerrainRoutes-20261017 | 2026-10-17 | 2.387 | RESTORE_POINT: full source/build, manifest PASS | 3075 | 8 / 0.012 GB |
| CheckpointUI1ArtDirection-20261016 | 2026-10-16 | 0.058 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 3 / 0.000 GB |
| DesignKitSceneUpdates | 2026-10-15 | 0.000 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| LumiRoleSceneUpdates | 2026-10-15 | 0.001 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 0 / 0.000 GB |
| SceneRebuilds | 2026-10-15 | 0.000 | UNCERTAIN: thiếu manifest đầy đủ hoặc là overlay; cần base | 0 | 1 / 0.000 GB |

Cột độc nhất so với các backup khác, không khẳng định các byte đó không có trong project chính. uniquePaths trong backup-audit.json ghi chính xác file khác biệt. Byte trùng ở nhiều backup chưa đủ chứng minh một bản khôi phục có thể bỏ.

### Đề xuất cần đồng ý riêng — CHƯA XÓA

**BeforeCityMapUIPolish-20261017: 1.196.437.256 byte (~1,20 GB).** 1.578 file ngoài manifest trùng cả đường dẫn tương đối và SHA256 với **CheckpointDaLatChapter-20261017**. File riêng là checkpoint-manifest.json (328.139 byte).

Trước khi xóa cần giữ manifest riêng như bản ghi audit nhỏ và chuyển baseline của **Tools/citymap_ui_polish.py** sang checkpoint tương đương, đồng thời ghi đường dẫn chuyển tiếp cho tài liệu lịch sử. Chưa thực hiện các thao tác này; chưa có sự đồng ý riêng. Kế hoạch cụ thể nằm trong backup-delete-proposal.json.

Các checkpoint khác có source, art/build hoặc bằng chứng lịch sử riêng: **UNCERTAIN đối với xóa**, chưa đề xuất xóa chắc chắn. Đặc biệt FullStoryAuthored còn 21 file riêng, không chỉ là bản sao vô ích.

## Dữ liệu còn lớn / cache

Backups còn **18,839 GB**, do nhiều full snapshot lặp source/build và Validation. Validation còn **9.273 GB**: bằng chứng cuối, trước/sau, log/save và các nhóm chưa đủ căn cứ loại bỏ. Không xóa thêm chỉ để đạt một mục tiêu GB.

Library còn **4.233 GB**, gồm import/build cache và PackageCache. Những phần có thể tái tạo cần đánh giá riêng: Bee ~441 MB, BurstCache ~290 MB, PlayerDataCache ~343 MB, BuildHistory ~131 MB (số trước mở Unity). Xóa sẽ khiến lần build sau phải tái tạo cache, có thể mất vài phút hoặc lâu hơn; chưa đo thời gian trên máy này.

Không khuyến nghị xóa toàn bộ Library/PackageCache vì phải import hoặc tải lại nhiều dữ liệu. Không xóa riêng DataStore khi chưa xác định quan hệ cache. **Lượt này không xóa cache.**

## Kiểm thử nhẹ

- **PASS:** 2.145 file bảo vệ gồm source/art/meta/config/build/tài liệu Story/Art/Maps/Requests và save JSON khớp SHA256 trước/sau, kể cả sau mở Unity.
- **PASS:** toàn bộ 39 backup / 29.873 file nguyên byte.
- **PASS:** 4.826 tham chiếu evidence trong manifest vẫn đủ file.
- **PASS:** Unity 6000.6.5f1 mở project batch mode, exit 0; log có “Exiting batchmode successfully now!”, không có error CS. [Log Unity](../../../Logs/storage-cleanup-open-20261017.log).
- Build Windows cuối giữ nguyên hash; không build lại hoặc chạy toàn bộ assertion vì chỉ dọn ảnh QA. Gameplay runtime không playtest lại trong lượt này.
- Không có Unity/Player/build/checkpoint đang chạy khi dọn. Chỉ có host Codex của phiên làm việc; đã kiểm tra danh sách lệnh tiến trình ở chế độ chỉ đọc.

## Quy trình chống phình dung lượng

Đã thêm [AGENTS.md](../../../AGENTS.md) ở gốc project:
- Dùng checkpoint gần nhất khi chưa đổi source; ưu tiên snapshot gia tăng có base/hash/manifest và deleted-path list.
- Không gói Library hoặc toàn bộ Validation lịch sử vào checkpoint.
- Không tạo full copy tương đương nếu chưa được duyệt; ước tính dung lượng trước khi copy.
- QA dùng workspace tạm, dùng lại build nếu production inputs không đổi.
- Giữ ảnh đại diện theo tiêu chí nghiệm thu, save/log tái hiện và ảnh chẩn đoán lỗi; không chụp mỗi assertion mặc định.
- Kiểm tra export trung gian ở cuối tác vụ; không tự xóa backup lịch sử.

Không khởi tạo Git hoặc hệ thống checkpoint mới. Quy tắc đã được đặt; screenshot runner gameplay không bị sửa trong lượt dọn này. Không tự chạy lại công cụ lịch sử để tái tạo full checkpoint hoặc các ảnh đã dọn.

Tools/storage_cleanup.py là công cụ kiểm toán giới hạn; không có lệnh xóa backup/cache/source. --apply chỉ sử dụng danh sách PNG cụ thể đã kiểm tra. Không tự chạy lại kế hoạch lịch sử. Các hash/CSV/JSON ở đây là bằng chứng kiểm toán nhỏ, không phải bản sao project.
