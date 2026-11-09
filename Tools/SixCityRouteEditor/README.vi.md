# LBC — Tự vẽ đường cho sáu thành phố

## Mở công cụ

Mở **index.html** trong thư mục này bằng Microsoft Edge hoặc Chrome. Có thể bấm đúp file. Không cần Unity, máy chủ, mạng, tài khoản hoặc cài thư viện.

Giữ nguyên cả thư mục `SixCityRouteEditor` nếu chép sang nơi khác: index.html cần source.js, model.js, app.js, style.css và sáu ảnh trong assets. Không chỉ chép mỗi index.html.

Đây là công cụ thiết kế riêng. Không có nút ghi vào Unity, không build APK, không thay đường đang chơi. Những đường bạn vẽ phải được chủ dự án duyệt rồi nhập bằng một nhiệm vụ riêng.

## Dữ liệu thực tế đã xuất

| Thành phố | Node | Cạnh tham khảo |
|---|---:|---:|
| Hội An | 22 | 22 |
| Huế | 22 | 28 |
| Đà Lạt | 22 | 26 |
| Jeonju | 22 | 30 |
| Gyeongju | 22 | 25 |
| Seoul | 23 | 24 |
| Tổng | 133 | 155 |

Nguồn hiện tại còn có 29 quest, 22 tương tác cốt truyện, 12 leisure, 6 di vật/24 mảnh và 18 check-in. Công cụ không thay các hệ thống đó. Sáu PNG là bản sao nguyên byte của ảnh runtime đang bind; mỗi ảnh gốc là 1448×1086, không giả nâng lên 4K.

## Xem map và node

- Chọn thành phố ở thanh trên.
- Lăn chuột để zoom ngay tại con trỏ. Chọn **Kéo / chọn** rồi kéo nền, hoặc giữ Space và kéo / dùng chuột giữa ở bất kỳ chế độ nào.
- **Fit Map** hoặc **Reset View** đưa về toàn cảnh.
- Bật/tắt nhãn node, đường cũ, đường mới; lọc NPC/bot/puzzle/leisure/check-in… ở cột trái.
- Hover/chọn node để xem tên, loại, tọa độ, các cạnh, NPC/quest/puzzle/check-in và điều kiện liên quan.
- Node canonical luôn khóa, không kéo được. Hình thoi nhỏ là **điểm Lumi đứng** nếu khác chấm icon. Đầu/cuối đường dùng điểm đứng đó, không kéo icon trên mái nhà xuống đất.

## Vẽ một đường mới

1. Chọn **Thiết kế mới**.
2. Chọn **Vẽ đường**, click node đầu.
3. Click dọc lối đi trên tranh để đặt các điểm uốn, theo đúng thứ tự Lumi sẽ đi.
4. Click node đích.
5. Bấm **Lưu đường** ở cột trái. Có thể cần cuộn cột trái xuống.

Cách khác: chọn Node đầu/Node cuối trong hai danh sách, bấm **Bắt đầu đường mới**, rồi click các điểm uốn và Lưu đường.

Đường cũ màu xanh nhạt, nét đứt; thiết kế đã lưu màu xanh lá; phần đang chỉnh màu vàng; tuyến thử màu hồng. Thành phố mới mở có **0 cạnh thiết kế**, không tự sao chép mạng cũ.

## Sửa, thêm điểm, xóa và hoàn tác

1. Chọn một đường thiết kế bằng click hoặc danh sách **Đường thiết kế**.
2. Bấm **Chỉnh đường**. Chấm vàng là waypoint có thể kéo.
3. Kéo chấm vàng để sửa vị trí. Không kéo node hoặc hai điểm đứng đầu/cuối.
4. Nhấp đúp lên một đoạn khi đang ở **Kéo / chọn** để chèn điểm giữa đoạn.
5. Chọn waypoint rồi bấm **Xóa waypoint đang chọn**, phím Delete, hoặc chuột phải vào waypoint.
6. Bấm **Lưu đường** để thay riêng tuyến đó. **Bỏ sửa** bỏ bản đang chỉnh, giữ đường đã lưu trước đó.
7. **Xóa cạnh** chỉ xóa trong thiết kế, có xác nhận. Undo/Redo hoặc Ctrl+Z / Ctrl+Y có thể hoàn tác/làm lại.

Lịch sử Undo giữ tối đa 100 thao tác trong phiên đang mở. Sau khi mở lại trình duyệt, hình vẽ và đường đang vẽ dở được khôi phục nếu lưu được, nhưng lịch sử Undo của phiên cũ không được lưu. Các ô văn bản giữ phím tắt chỉnh chữ thông thường.

Mỗi waypoint thuộc riêng một cạnh. Hai đường giao nhau hoặc đi qua cùng tọa độ **không tự thành giao lộ graph**. Công cụ không tạo node điều hướng mới, không tự hợp nhất waypoint chung. Nếu cần giao lộ không nằm tại một node canonical, ghi chú rõ để xem xét kiến trúc trong lần nhập sau; đừng mặc định Lumi có thể rẽ ở chỗ hai nét giao nhau.

## Giữ/sửa riêng một tuyến cũ

Chọn Edge ID trong **Tuyến tham khảo**, bật Waypoint tuyến tham khảo nếu cần đọc các điểm. Bấm **Lấy riêng tuyến này làm bản sửa**, chỉnh rồi Lưu đường. Chỉ một tuyến được lấy vào thiết kế. Tham chiếu Edge ID/SourceRouteKey, đầu/cuối, kiểu đò và điều kiện cũ được giữ.

Nếu muốn đổi đầu/cuối của tuyến nguồn, hãy tạo một cạnh thiết kế mới; không sửa nhầm ID/điều kiện của cạnh nguồn. Mã H-E…/U-E… là mã tham khảo ổn định đã có. Mã H-N…/U-N… là cạnh đề xuất mới, chưa phải ID gameplay.

## Một chiều và hai chiều

Khi đang chỉnh đường, bỏ chọn **Hai chiều** để thiết kế đầu → cuối. Rà soát sẽ cảnh báo nguy cơ không quay lại được. Hiện runtime GameData của Unity vẫn xử lý tất cả cạnh hai chiều; việc hỗ trợ chiều mới trong game chưa được triển khai ở nhiệm vụ này.

Tuyến đò có màu riêng và được giữ metadata `travel=ferry`; không được xem như đường đi bộ trên sông. Cổng sang thành phố khác nằm trong dữ liệu sự kiện/portal của node, không nằm trong mạng đường đi bộ và không được đưa vào phép tìm đường nội thành.

## Thử quãng đường

Chọn A và B bằng hai danh sách, hoặc chọn công cụ **Thử tuyến** rồi click hai node. Chọn mạng **Thiết kế mới** hoặc **Đường cũ**, bấm Tính tuyến.

Công cụ dùng Dijkstra, trọng số là tổng chiều dài polyline theo tỷ lệ thật của tranh. Nó hiển thị chiều dài, khoảng cách thẳng và tỷ lệ đi vòng; không chỉ đếm số cạnh. Runtime Unity hiện dùng BFS theo số cạnh — công cụ không sửa thuật toán đó.

Không tìm thấy tuyến thì công cụ báo, không tự nối thêm. Tỷ lệ đi vòng lớn là gợi ý để xem địa hình, không phải bằng chứng phải nối thẳng xuyên nhà/sông.

## Lưu hôm nay, vẽ tiếp hôm sau

1. Bấm **Lưu nháp trên trình duyệt**. Có tự lưu sau các thao tác vẽ.
2. Bấm **Tải JSON cả 6 thành phố**, đặt file ở nơi dễ nhớ, ví dụ `LBC-duong-di-ngay-1.json`.
3. Hôm sau mở đúng index.html bằng cùng trình duyệt. Nếu bộ nhớ còn, bản nháp tự khôi phục.
4. Nếu không tự khôi phục, bấm **Mở bản nháp JSON**, chọn file đã tải. Không cần sửa JSON bằng tay.

Luôn tải JSON cuối mỗi buổi. Chế độ ẩn danh, đổi trình duyệt/đường dẫn hoặc xóa dữ liệu trình duyệt có thể làm mất bản lưu nội bộ. File JSON tải xuống là bản sao độc lập. Bản đang vẽ dở cũng được giữ trong JSON, nhưng cần Lưu đường trước khi coi là một cạnh hoàn chỉnh.

**Tải JSON thành phố** chỉ xuất thành phố đang mở. Nhập file một thành phố sẽ giữ nguyên năm thành phố khác. File cả sáu thành phố sẽ thay toàn bộ bản thiết kế đang mở, có thể Undo sau khi nhập.

File sai định dạng, đổi node/arrival/khóa canonical, waypoint ngoài 0–1000, ID nguồn sai hoặc dùng source snapshot khác sẽ bị từ chối. Bản đang mở không bị ghi đè khi nhập lỗi. Nếu bộ nhớ trình duyệt hỏng, công cụ không tự ghi đè nó bằng bản trắng; nhập file hợp lệ để khôi phục.

## Kiểm tra trước khi gửi

Bấm **Kiểm tra thiết kế thành phố**. Ba loại thông báo:

- **Lỗi dữ liệu:** node cô lập, ID trùng, cạnh trùng, node đầu/cuối sai, waypoint lỗi.
- **Rủi ro gameplay:** node quan trọng chưa tới được, một chiều không có đường về, kết nối qua stage/gate, tuyến đò.
- **Gợi ý:** tuyến đi vòng quá dài so với đường thẳng.

Công cụ không biết ảnh có nhà/sông/cây ở từng pixel và không chạy quest/gate/discovery, hai tuyến chơi hoặc save. Các phần đó luôn **NOT_VERIFIED**, dù cảnh báo graph đã hết. Không tự sửa theo cảnh báo.

## Gửi bản cuối

Chọn trạng thái **Sẵn sàng gửi duyệt** cho thành phố đã vẽ xong, thêm ghi chú những đoạn cần kiểm tra, rồi Tải JSON. Gửi file cho Codex/chủ dự án ở nhiệm vụ nhập riêng.

Mỗi thành phố trong file là một **mạng thiết kế đề xuất**, không phải lệnh tự động áp dụng. Các cạnh cũ chỉ dùng tham khảo; những cạnh muốn giữ cần được bạn lấy riêng vào thiết kế và Lưu. Thành phố NOT_STARTED/DRAFT hoặc graph chưa hoàn chỉnh tuyệt đối không được hiểu là yêu cầu xóa hết route trong Unity.

## Dữ liệu kỹ thuật để nhập sau

- `format=LBC_ROUTE_DESIGN`, `schemaVersion=1`, `sourceFingerprint` gắn đúng snapshot.
- `cities[].nodes`: toàn bộ node canonical, runtime index, vị trí icon/arrival và tham chiếu/khóa read-only.
- `cities[].preservedCityData`: spawn, unlock/gate, map GUID/localID/hash và coveredWalkways cần bảo toàn.
- `cities[].edges`: cạnh thiết kế với ID, from/to, waypoint theo thứ tự, bidirectional, travel, nguồn và điều kiện; `draft` là phần đang vẽ dở.
- Tọa độ X/Y đều 0–1000, gốc **trên trái**, Y tăng xuống. Đổi về Unity: `x=X/1000`, `y=1-Y/1000`. Đổi ra PNG: `pixelX=X*width/1000`, `pixelY=Y*height/1000`.
- Chiều dài theo tỷ lệ ảnh: `hypot(dx,dy*imageHeight/imageWidth)`, đơn vị bề rộng map=1000. Không kéo méo tranh thành hình vuông để tính đường.
- Đầu/cuối polyline được suy từ arrival khóa của from/to; `waypoints` chỉ chứa điểm trung gian.
- `data/source.json`, node_inventory.csv, edge_inventory.csv và export-manifest.json giữ nguồn/ID/điểm hiện tại. source.js đóng gói dữ liệu để không cần fetch/server.
- H5 và H7 Hội An có phân loại runtime/catalog khác nhau (bot/boss và quest/beacon); cả hai được giữ, không đổi gameplay để thống nhất tên loại.
- schema-v1.json mô tả cấu trúc; model.js kiểm tra thêm node/khóa chính xác theo snapshot, không tải schema từ Internet.

Không có importer Unity trong bộ này. Chỉ sau khi bạn duyệt thiết kế mới được xây bước nhập và QA gameplay/APK.
