# Đại tu tuyến theo bản vẽ chủ dự án — Phase A, 2026-10-19

**NEEDS_OWNER_CLARIFICATION_BEFORE_PRODUCTION. Chưa áp dụng graph hoặc sửa thuật toán runtime.**

## Đầu vào và source thật

Đã mở đủ sáu PNG trong ZIP `C:\Users\admin\OneDrive\Pictures\Screenshots\Screenshot 2026-10-19 174731.zip`. Bản nguyên byte giữ tại `Validation/OwnerRouteRedesign-20261019/inputs`; SHA256 và kích thước trong input-manifest.json. Không import vào Assets. Thứ tự theo node ID: 174731 Hội An; 174736 Đà Lạt; 174741 Jeonju; 174745 Gyeongju; 174751 Seoul; 174630 Huế.

Source hiện tại xác nhận 133 node, 155 cạnh cũ, 29 quest, 22 puzzle definitions, 22 StoryInteractions, 12 leisure, 6 relic và 24 fragment. Không ép graph mới bằng 155 cạnh. Vị trí/arrival trong export được so trực tiếp với GameplayContent và HoiAnData mới nhất; record của toàn bộ 155 cạnh cũ khớp export. Sáu nền canonical giữ SHA256. Checkpoint baseline hiện hành: CheckpointAndroidAppIcon-20261019, trên base SixCityRouteEditor; không checkout hoặc rollback.

## Đã thực hiện

- Tách nét bút theo màu thực tế: xanh lam RGB(63,72,204), đỏ RGB(136,0,21). Không lấy màu node/sông/cầu/legend làm tuyến.
- Fit hệ tọa độ mỗi ảnh bằng các node canonical được nhận diện trực quan; chuẩn map 0–1000 gốc trên trái. Sai số raster khoảng vài pixel ở ảnh screenshot; không đổi tọa độ node hoặc arrival.
- Truy vết centerline, giản lược lệch tối đa 0,65px ở ảnh đầu vào, giữ uốn cong và không tự nối khoảng đứt. Tách nét tại junction pixel, **không tự coi junction là giao lộ runtime**.
- Xuất sáu preview trên đúng sáu Map Overview runtime nguyên gốc. Preview là file riêng trong Validation; không ghi đè ảnh nguồn.
- Xuất tọa độ chuỗi nét, kiểm tra khoảng cách tới marker/arrival, 5 ảnh nét đỏ A/B, 7 ảnh chênh lệch điểm đứng, 3 ảnh giao điểm. Sau xem cận cảnh đã bỏ X2 (nhánh thực tế đi qua D18) và X5 (không phải giao cắt riêng) khỏi câu hỏi, tránh hỏi lại những điểm có thể giải thích từ ảnh. Hai crop nháp đó không nằm trong gallery hiện hành.

| Thành phố | Node giữ nguyên | Cạnh cũ | Chuỗi nét quan sát | Điểm số hóa | Pixel nét đỏ |
|---|---:|---:|---:|---:|---:|
| Hội An | 22 | 22 | 32 | 830 | 167 |
| Huế | 22 | 28 | 35 | 1138 | 0 |
| Đà Lạt | 22 | 26 | 35 | 858 | 125 |
| Jeonju | 22 | 30 | 28 | 1037 | 0 |
| Gyeongju | 22 | 25 | 44 | 824 | 442 |
| Seoul | 23 | 24 | 45 | 891 | 202 |

**Chuỗi nét không phải cạnh gameplay.** Chưa chốt số cạnh đi bộ/thuyền mới; mỗi nét đỏ có thể chỉ là một đoạn giữa cạnh nhiều phương tiện. Không tự biến hai đầu đoạn nét thành node mới. Xem `stroke-inventory.csv` và `*-ink-strokes.json` trong thư mục Validation cùng lượt.

## Cần chủ dự án xác nhận

1. **Hội An R1**: nét đỏ nối H0 marker trên nước với H0 arrival trên bến đất, tức hai đầu thuộc cùng một node. Giữ arrival hiện tại thì đoạn đỏ nằm ngoài tuyến tiếp cận H0, không thể tự gán thành cạnh nối hai node khác nhau. Cần chủ dự án chốt cách xử lý đoạn riêng này. R2 (Đà Lạt), R3/R4 (Gyeongju), R5 (Seoul) được xuất đủ để kiểm tra A/B chuyển phương tiện; không cần yêu cầu chủ dự án đánh Edge ID.
2. **Điểm đứng A-H10, A-J7, A-G6, A-G7, A-S0, A-S5, A-S8**: nét qua gần marker nhưng lệch điểm đứng. Đề xuất giữ nguyên arrival và thêm đoạn tiếp cận ngắn hợp địa hình từ nét vẽ; cần chủ dự án chấp nhận, không tự kéo thẳng xuyên vật cản.
3. **Giao điểm X1, X3, X4**: cần xác nhận cho rẽ tại nơi nét chạm/cắt hay chỉ đi dọc từng tuyến. Chưa thêm navigation junction hoặc node gameplay. Đây là các ví dụ nổi bật; sau khi chốt quy tắc phải rà toàn bộ junction còn lại, không tự coi mọi điểm khác là đã được duyệt.

| Mã | Thành phố | Vấn đề | Bằng chứng |
|---|---|---|---|
| R1 | hoi_an | FERRY_TRANSITION | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/R1-hoi_an-ferry.png) |
| A-H10 | hoi_an | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-hoi_an-H10.png) |
| R2 | da_lat | FERRY_TRANSITION | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/R2-da_lat-ferry.png) |
| A-J7 | jeonju | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-jeonju-J7.png) |
| R3 | gyeongju | FERRY_TRANSITION | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/R3-gyeongju-ferry.png) |
| R4 | gyeongju | FERRY_TRANSITION | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/R4-gyeongju-ferry.png) |
| A-G6 | gyeongju | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-gyeongju-G6.png) |
| A-G7 | gyeongju | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-gyeongju-G7.png) |
| R5 | seoul | FERRY_TRANSITION | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/R5-seoul-ferry.png) |
| A-S0 | seoul | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-seoul-S0.png) |
| A-S5 | seoul | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-seoul-S5.png) |
| A-S8 | seoul | ARRIVAL_CONNECTOR | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/A-seoul-S8.png) |
| X1 | hoi_an | CONNECTIVITY_REVIEW | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/X1-hoi_an-junction.png) |
| X3 | jeonju | CONNECTIVITY_REVIEW | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/X3-jeonju-junction.png) |
| X4 | gyeongju | CONNECTIVITY_REVIEW | [Ảnh cận cảnh](../../../Validation/OwnerRouteRedesign-20261019/review-crops/X4-gyeongju-junction.png) |

Mở [gallery review](review.html) để xem ảnh nguồn, preview và ảnh khoanh chi tiết. Không cần chủ dự án đánh lại Edge ID hoặc vẽ lại toàn bộ. Không áp dụng production trong khi còn quyết định mơ hồ, theo mục 7A của prompt mới.

## Bước Unity sau khi chốt — CHƯA THỰC HIỆN

- GameData.FindPath hiện dùng Queue/BFS theo số cạnh. Đổi sang Dijkstra theo tổng chiều dài polyline, tính theo tỷ lệ ảnh thật (không dùng khoảng cách node thẳng). Tôn trọng EdgeOpen, discovery, gate và chiều cạnh.
- RouteDefinition có bidirectional, nhưng GameData.Load/FindPath hiện bỏ qua chiều và coi hai chiều. Phải đưa chiều vào runtime cùng đợt thay graph.
- CityMap.Move đã sample CityRouteGeometry cho cả hình vẽ và di chuyển; giữ một nguồn hình học. Travel hiện chỉ có walk/ferry cho **cả cạnh**, chưa hỗ trợ đoạn đa phương tiện/điểm chuyển giữa cạnh. Có ferryVisual và bobbing tối thiểu để kế thừa; không coi icon thuyền này là một bộ sprite thuyền hoàn chỉnh mới.
- Chỉ sau khi đối chiếu xong: checkpoint tăng dần phần source bị tác động, áp dụng graph mới + weighted routing + phân đoạn thuyền, chạy Windows targeted QA và progression hai thứ tự. Không thay art/audio/quest/save hoặc node/arrival.

## QA đúng phạm vi hiện tại

PASS: ZIP đủ sáu PNG; source snapshot khớp runtime records và canonical node/arrival; sáu map SHA256 khớp; 18 file source/config/map có hash không đổi trước/sau Stage A.

PARTIAL: truy vết nét và chuyển hệ tọa độ đã có preview; connectivity và chuyển phương tiện chưa được duyệt. Không dùng khoảng cách tới nét để tuyên bố node đã tiếp cận được.

NOT_TESTED: graph mới trong Unity, Dijkstra runtime, thuyền nhiều đoạn, Windows build, progression, save/restart và Android UX. Chưa sửa production nên không build lại APK/Windows. Không tạo checkpoint gameplay mới khi source chưa đổi.

File thêm: hai tool offline `Tools/digitize_owner_routes_20261019.py`, `Tools/review_owner_routes_20261019.py`; bằng chứng trong Validation/OwnerRouteRedesign-20261019; báo cáo/gallery tại Docs/Maps/OwnerRouteRedesign-20261019. Không xóa ảnh/log/backup hoặc lịch sử đã có.

Dừng trước Stage B để chủ dự án chốt các điểm trên. Đây là bản số hóa nháp, không phải tuyên bố hoàn tất đại tu sáu thành phố.
