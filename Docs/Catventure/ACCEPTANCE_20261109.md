# Catventure – bàn giao bản vá source 09/11/2026

Đây là bản sửa source có kiểm soát theo tài liệu yêu cầu Catventure 13 trang. **Không phải bản APK/EXE và không tuyên bố toàn bộ game đã PASS.** File build cũ trên máy không tự thay đổi khi source GitHub/Unity được cập nhật.

## Vấn đề đã xác định từ code

- `MainMenu.cs` trước đây không có lựa chọn Catventure. Người chơi có thể chơi hoàn toàn tuyến cũ, không nhìn thấy nhiệm vụ giải cứu mèo, dù dữ liệu có 6 chương và 24 thử thách.
- `CityMap.CatRescue.cs` trước đây chỉ hiển thị hình mèo và hiệu ứng thời tiết khi nhiệm vụ **đã được nhận**, nên trước lúc biết cách nhận quest người chơi không thấy mèo.
- Gyeongju: Kho báu Phật giáo là một câu trắc nghiệm, chưa sát mong muốn thao tác ghép di sản.
- Bản Windows/Android trong `Builds/` là file binary tạo trước; push source lên GitHub không cập nhật lại binary này.

## Phạm vi bản vá tự động

1. Bổ sung nút CATVENTURE ngay trên màn hình bắt đầu; giữ nguyên lối vào World Map và Hướng dẫn cũ. Nút mới mở thẳng CityMap, bật panel nhiệm vụ cứu mèo.
2. Ghi rõ CATVENTURE – 6 mèo/24 thử thách trên màn hình đầu; nhấn nhầm EXE cũ sẽ dễ nhận ra.
3. WorldMap giới thiệu nhiệm vụ cứu mèo, và cho biết trạng thái đã cứu/chưa cứu theo từng thành phố.
4. Hình mèo và hiệu ứng thời tiết hiện ngay cả trước khi nhận quest; hình mèo vẫn không chặn tương tác với node.
5. Gyeongju: “Kho báu Phật giáo” chuyển từ trắc nghiệm sang kéo-thả ghép **tên di sản với đặc điểm**. Chưa có ghép mảnh ảnh chuẩn, cần bổ sung artwork đã được duyệt để đáp ứng hoàn toàn PDF.
6. Đổi version build thành `1.1.2-catventure-20261109`, Android code 22 và đổi tên output để phân biệt file cũ.

## Chưa hoàn tất so với PDF

- **Hội An:** hiện đèn sáng tăng theo tiến trình thử thách, chưa mở cạnh đường đi thật bằng từng đèn.
- **Huế:** thử thách cứu hộ đang là mê cung, chưa có hệ thống điều khiển thuyền cứu hộ riêng.
- **Đà Lạt:** chuông gọi mèo là vật phẩm trong quest, chưa có thao tác rung chuông độc lập tại cảnh tìm mèo.
- **Jeonju:** tìm mèo trên bảng tìm vật ẩn, chưa thành vùng tìm kiếm di chuyển tự do trong các nhà Hanok.
- **Gyeongju:** còn thiếu ảnh ghép Bulguksa / Dabotap / Seokguram theo đúng tài liệu; bản vá tạm đổi sang kéo-thả di sản.
- **Seoul:** dấu ấn công trình Hàn Quốc chưa có mini-game riêng; hiệu ứng mở các lối bị phong tỏa vẫn cần kiểm thử native.
- **Toàn game:** chưa có nghiệm thu Android trên thiết bị thật, chiến dịch truyện cũ sau cập nhật, trải nghiệm người chơi độc lập, FPS/RAM/cảm ứng thật.

## Cách thực hiện trước khi nộp

Tại máy có Unity 6000.6.5f1, mở project và chạy:

```powershell
python Tools/validate_catventure.py
python Tools/check_catventure_delivery.py
```

Kết quả trên chỉ kiểm tra tĩnh. Phải kiểm tra Unity Console không có lỗi biên dịch sau đó mới build.

- Windows: gọi `LightBetweenCultures.Editor.CatRescueBuild.BuildWindows` trong Unity bằng `-executeMethod`, thêm `--cat-release` để lấy Windows Release. File EXE mặc định mới: `Builds/Windows-Catventure-Visible-Release-20261109/Catventure.exe`.
- Android: chuyển build target Android, kiểm tra signing, rồi gọi `LightBetweenCultures.Editor.CatRescueBuild.BuildAndroid`. APK mặc định mới: `Builds/Android/Catventure-Visible-20261109.apk` (versionCode 22).
- Trên bản mới, mở scene `Assets/_Game/Scenes/Prototype.unity`: nhìn thấy nhãn CATVENTURE ở đầu. Bấm nút cứu mèo → CityMap → bảng nhiệm vụ. Cần xác nhận cả việc dùng Start cũ để vào WorldMap vẫn chạy.
- Ở 6 thành phố, cần thử cả trước/sau khi nhận quest, 3 vật dụng/4 thử thách, 24 mini-game, cứu 6 mèo, xu/receipt, 2 lần restart và tải save cũ, thao tác sai đúng, chuyển map và quest cũ.
- Thử Android thật để xác nhận drag/drop, các vùng chạm, zoom, hoạt ảnh, âm thanh, save, xoay màn hình và tốc độ khung hình. Không được tính `NOT_TESTED` thành `PASS`.

## Bảo toàn

Script tạo backup **trước khi sửa** dưới thư mục `Backups/Catventure-Patch-20261109-*` nằm cạnh project, chỉ sao lưu file có thay đổi. Không xóa `Assets`, `.meta`, `ProjectSettings`, `Packages`, backup, dữ liệu lưu hay Build cũ. Không chạy generator Phase. Không sửa lịch sử QA cũ để hợp thức hóa kết quả mới.
