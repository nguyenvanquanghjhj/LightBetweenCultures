# Workspace restructure — điểm dừng để tiếp tục

Ngày ghi nhận: 13/11/2026. Chủ dự án yêu cầu thu gọn công việc để dành tài nguyên máy cho PBL. Dừng mở rộng cleanup/refactor và không chạy thêm build hoặc kiểm thử nặng.

**Chưa nghiệm thu toàn bộ; chưa commit hoặc push.** Đây là bản bàn giao trạng thái hiện tại, không phải kết luận mọi chức năng đã đạt.

## Nơi tiếp tục

- Workspace: `D:/GAMEKKK`; Unity project: `D:/GAMEKKK/LightBetweenCultures`.
- Nhánh: `refactor/codebase-map-20261112`.
- HEAD được kiểm tra khi dừng: `9ec07c5e581444d34447598f4c0c6538e40ad9f4`.
- Thay đổi đang nằm trong working tree. Không reset, clean hoặc chạy lại migration đã hoàn tất khi tiếp tục.
- Hồ sơ di chuyển và kiểm chứng: [WorkspaceRestructure-20261113](../Development/Storage/WorkspaceRestructure-20261113/).
- Có sáu mục generated được bỏ theo dõi trong index; chưa stage toàn bộ nhiệm vụ. Khi tiếp tục phải kiểm tra lại cả staged và unstaged diff, không dùng `git add .`.

## Công việc đã thực hiện

Workspace đã chia thành bốn nhóm:

```text
D:/GAMEKKK/
├── LightBetweenCultures/
├── _LBC_ARCHIVE/
├── _LBC_DELIVERABLES/
└── _LBC_WORK/
```

- Backup, checkpoint, patch lịch sử, nguồn art lịch sử và QA lịch sử đã được phân loại ra ngoài Unity project. Bản nộp, Windows/Android và handoff được tách khỏi source.
- Có inventory ban đầu và manifest từng nhóm di chuyển, gồm SHA-256. Di chuyển cùng ổ đĩa không được tính là dung lượng ổ đĩa đã giải phóng.
- 14 ảnh đã chuyển từ `Resources/Art` vào `Art` cùng metadata. `GameArtCatalog` giữ binding GUID/localID; mã tải, JSON liên quan và đường dẫn công cụ đã được cập nhật. Không tạo bản sao ảnh cho hai cơ chế tải.
- Các catalog nằm tại `Resources/Catalogs`, dữ liệu gameplay tại `Resources/Content`; ảnh Unity hiện được tổ chức trong `Assets/_Game/Art`.
- Đã chuyển thêm 106 file nguồn import không còn cần cho workflow hiện hành, tổng 70.282.297 byte, vào archive; giữ 40 file đầu vào authoring còn có vai trò. Đây là lưu trữ, không phải xóa vĩnh viễn.
- Một số công cụ/fixture lịch sử đã được lưu trữ, các công cụ hiện hành được chỉnh đường dẫn. Không đổi định dạng save, nội dung quest hoặc điều kiện ending.

Chi tiết từng file nằm trong `move-result.json`, `asset-moves.json`, `marker-moves.json`, `retired-pipelines.json`, `unused-input-moves.json` và các manifest cùng thư mục hồ sơ. Không tạo thêm một bản backup lớn cho lần dừng này.

## Bằng chứng hiện có và giới hạn

| Hạng mục | Trạng thái đã ghi nhận |
| --- | --- |
| Unity compile/native audit cuối | Unity 6000.6.5f1 thoát 0; log `_LBC_WORK/Logs/WorkspaceRestructure-current.log`; dữ liệu tại `native-current/`. |
| Catalog art | `native-current/catalog-result.json`: 34 assertions đạt; không phải nghiệm thu UI trong Player. |
| Route | `route-current.json`: 6 thành phố, 2.949 cặp node, 5.901 assertions đạt trong Unity. |
| Native references/save | 0 missing script; 7 GUID và 6 object reference lỗi của template vẫn còn như baseline; 4 save fixture assertions. Không ghi save người chơi. |
| Cat domain | Adapter host đạt 518 + 2 assertions; không phải Unity Player. |
| Windows | QA build thành công; chạy EXE bị Application Control của hệ điều hành chặn. Không có Player journey PASS. Build này có trước lần lưu trữ cuối của profile QA appearance cũ. |
| Android | QA build thành công; kiểm tra SDK aapt, apksigner và zipalign đạt. Chưa cài/chơi trên thiết bị. |
| Editor journey | Lần mới nhất thất bại với `MissingComponentException` liên quan `CanvasRenderer` trên `MiniMapRoads`. Chưa xác định nguyên nhân bằng baseline tương đương; chưa sửa gameplay để che lỗi. |
| Layout validators | JSON hiện trên đĩa vẫn ghi FAIL: WS-010 báo `build`; PR-001 báo `.utmp` và `build`. Mã xử lý thư mục generated đã được chỉnh nhưng kết quả chưa được chạy lại/chốt sau thay đổi cuối. Không coi kết quả cũ là PASS. |

`native-selection.json` hiện vẫn trỏ `native-acceptance`, trong khi audit cuối đã nằm tại `native-current`. Cần đối chiếu fingerprint và cập nhật lựa chọn trước lần kiểm tra layout tiếp theo. Chưa thực hiện kiểm tra build từ clone sạch hoặc diễn tập khôi phục đầy đủ.

## Thứ tự nối tiếp khi chủ dự án yêu cầu

1. Đọc lại ghi chú này và kiểm tra nhánh, HEAD, staged/unstaged diff; giữ nguyên thay đổi không thuộc nhiệm vụ.
2. Đối chiếu `native-current`, cập nhật lựa chọn audit và chạy lại hai layout validator với phạm vi nhẹ; phân biệt cache Unity tạo lại với dữ liệu lịch sử cần di chuyển.
3. Hoàn thiện cây thư mục và README công cụ. Không chạy lại inventory/hashing toàn workspace chỉ để xuất cây.
4. Điều tra riêng lỗi journey `MiniMapRoads`; kiểm tra tương đương trước/sau trước khi quy lỗi cho refactor. Việc này cần được chủ dự án cho tiếp tục dùng tài nguyên máy.
5. Chốt Git diff, tham chiếu, save và regression trong phạm vi đã thay đổi. Chỉ commit/push nhánh refactor khi các điều kiện chủ dự án đặt ra được đáp ứng; không force push, merge main hoặc ghi đè bản nộp.

Các báo cáo `REFACTOR_REPORT.md`, `CLEANUP_REPORT.md` và `UNUSED_ASSET_ELIMINATION.md` là hồ sơ của các giai đoạn trước. Không sửa kết quả thất bại cũ thành PASS khi tiếp tục.
