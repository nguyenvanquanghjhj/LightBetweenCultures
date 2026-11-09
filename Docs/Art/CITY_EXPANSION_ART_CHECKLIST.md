# Kiểm soát art khi mở rộng City Map — 16/10/2026

Áp dụng cho mọi đợt mở thêm nội dung Huế, Đà Lạt, Jeonju, Gyeongju và Seoul.
Đây là bổ sung quy trình cho framework đang có; không chạy lại dọn thư viện
Giai đoạn A hoặc chỉnh tỷ lệ Giai đoạn B. Phần đã PASS được giữ nguyên.

## Kiểm tra trước khi sửa

1. Kiểm tra source, file đang sửa, process Unity/player, log và checkpoint mới nhất.
   Chỉ bắt đầu đợt mới khi tác vụ trước đã hoàn tất và có checkpoint. Nếu đang chạy,
   tiếp nối tác vụ đó; không hủy hoặc khởi động một bản triển khai khác.
2. Đối chiếu [catalog](ART_ASSET_CATALOG.md), [registry export](native-art-bindings.json),
   art pack/ASSET_MANIFEST.csv, NPC/Enemy roster, `_SourceSheets`, DesignKit/Art Bible,
   `LBC_Gameplay_6Cities_Detailed_v1/Data/Cities/<cityId>.json` cùng quest/enemy/item JSON.
   StoryExpansion và ý định người dùng mới hơn được ưu tiên khi có xung đột;
   ghi rõ khác biệt, không sửa ngược nội dung đã nghiệm thu.
3. Chạy `python Tools/art_asset_catalog.py --city-preflight` tại thư mục project.
   Lệnh chỉ đọc Unity assets, ghi [snapshot](city-art-preflight.json) trong Docs.
   Kiểm tra hash input của snapshot để tránh dùng kết quả cũ sau khi source thay đổi.
   Snapshot không thay cho việc mở xem ảnh và chơi thử.
4. Lưu backup/Git trước khi thay đổi runtime. Giữ GUID/.meta/local sprite ID khi di chuyển;
   dùng registry hiện tại, không thêm manager/Canvas/EventSystem hoặc bitmap trùng.

## Nền và sprite

- Nền gameplay phải lấy đúng `Maps/<City>/Map_Overview_A.png` đã duyệt của art pack.
  Kiểm tra SHA-256, kích thước gốc, SpriteRect, importer mặc định và platform override
  đang dùng. Không phóng ảnh nhỏ, đổi ảnh B, kéo méo hoặc vẽ lại bản đồ.
  ASSET_MANIFEST nguồn còn ghi chung reference-only cho map/concept. Với Overview A,
  dùng xác nhận mới hơn của người dùng; không sửa manifest gốc hoặc áp ngoại lệ này
  cho mọi concept NPC/bot.
- Tên `DestinationPreview_A` hiện không đồng nghĩa ảnh nhỏ: cả năm file là bản sao
  nguyên bytes của Overview A **1448×1086**. Huế/Jeonju đã import ở max 2048 và chạy thật.
  Đà Lạt/Gyeongju/Seoul còn DefaultTexturePlatform 1024 cho preview; trước khi bật
  gameplay phải nâng importer trên asset/GUID hiện có và kiểm tra texture trong player.
  Không nhân bản ảnh chỉ để bỏ chữ Preview khỏi tên.
- Mở xem sprite ở cỡ hiển thị thực tế: mặt, bàn tay, viền alpha, chữ/nền baked, padding,
  pivot chân và silhouette. Atlas được phép dùng native slice + outline đã kiểm tra;
  không đưa nguyên bảng concept vào runtime. Crop lỗi ghi NEEDS_REVIEW, không tự gắn.
- Tìm asset tương đương trước khi tách/tạo mới. Giữ ảnh nguồn; mỗi itemId dùng một
  binding nhất quán xuyên thành phố. Category icon và hình tạm phải được ghi đúng trạng thái.
- Cô Mai và Ji-eun giữ nhận diện/binding đã duyệt. Nhân vật trở lại ở thành phố khác
  giữ khuôn mặt/đặc điểm nhận diện; thay trang phục không được biến thành người khác.
- Theo [NPC_CANONICAL_REGISTRY](NPC_CANONICAL_REGISTRY.md), một npcId có một danh tính và nhiều visual variants. Khóa khuôn mặt, tuổi tương đối và nét đặc trưng; cho phép đổi trang phục/biểu cảm/pose/phụ kiện phù hợp. APPROVED_VARIANT không phải IDENTITY_CONFLICT.
- `npc_huong`, `npc_soyeon`, `npc_ba_tu`, `npc_phuc` hiện đã **CANONICAL** theo xác nhận 17/10. Giữ nguyên hình/binding đã PASS; không tiếp tục chờ duyệt vì nhãn tạm cũ. So-yeon và Hương khác mặt; Ji-won không dùng mặt So-yeon. Duyệt danh tính độc lập với crop/animation; không thay khuôn mặt chỉ để đổi trang phục.
- Thiếu binding chỉ có nghĩa chưa tích hợp; kiểm tra roster/source thật trước khi kết luận
  cần vẽ mới. Ghi chính xác ID, node, vai trò portrait/map/combat, nguồn đã kiểm tra và
  trạng thái trong catalog/[danh sách thiếu](MISSING_ASSETS.md). Không coi hình khối là art final.

## Tái sử dụng và địa hình

- Dùng CityMap, AdventureUIArt, MapDiscoveryManager, marker, UI, WorldMap,
  CityRouteGeometry và SaveSystem hiện tại. Global map actor scale **0,72**, vùng bấm
  tối thiểu **52×52**; thành phố/actor được override bằng dữ liệu. Giữ aspect, pivot chân,
  bóng và marker phía trên; không thu nhỏ portrait, combat hoặc Lumi ở chế độ khác.
- Giữ ID/thứ tự 133 node, gate và tiến độ cũ. Chỉ mở `runtimeNodeIds` đã có nội dung
  thực sự; discovery vẫn quyết định khu vực đã khám phá. Marker/actor/input/pathfinding
  của khu chưa mở không được xuất hiện hoặc cho phép vượt gate.
- Author waypoint trên Overview A của chính thành phố đó, đối chiếu sơ đồ tọa độ.
  Mỗi tuyến mới phải có overlay và kiểm tra Lumi đi hai chiều trên cùng geometry dùng
  để vẽ đường. Men phố/cầu/bậc thang; qua nước chỉ khi có cầu hoặc ferry được cấu hình
  và kiểm thử. Không sao chép đường Hội An, không nối thẳng xuyên nhà/hồ/sông.
- Ghi waypoint cũ/mới, đoạn chưa chắc chắn và bằng chứng trong Docs/Maps.
  `routesReviewed: true` hoặc JSON parse thành công không đủ chứng minh tuyến an toàn.
  Không tự bật toàn chương khi chỉ có vài corridor được author.

## Nghiệm thu từng phần/chương

Chụp runtime **mới từ bản build đang bàn giao**: City Map với Lumi/actor/marker,
World Map, NPC portrait, inventory/journal khi có thay đổi; kèm đoạn Lumi đi qua
cầu/đò/vật cản cần chứng minh. Kiểm tra zoom, pan, portrait/landscape, gate reveal,
click/touch, đi lại hai chiều và save/restart ở thành phố vừa mở. Kiểm thử hồi quy
phần gameplay liên quan; Android chưa chạy thật ghi NOT VERIFIED.

Báo rõ node/quest có gameplay thật, node chỉ có dữ liệu, sprite tạm/chưa gắn/thiếu,
PASS/FAIL/NOT VERIFIED, file thay đổi và checkpoint. Mỗi chương bàn giao kèm danh sách
asset thiếu. Không dùng ảnh cũ chứng minh thay đổi mới hoặc tuyên bố hoàn thiện cả
thành phố khi chỉ triển khai phần mở đầu.

## Trạng thái lịch sử tại checkpoint mở đầu 16/10

`CheckpointCrossCityFood-20261016`: U0/U1 Huế, J0/J15 Jeonju và giao HQ05/HQ06/HQ07
đã chạy thật, dùng framework chung và scale 0,72. Các node còn lại của hai thành phố
vẫn chưa triển khai runtime; ba thành phố còn lại chưa bật gameplay. Hai corridor
đã author ghi tại [CrossCityOpeningReview](../Maps/CrossCityOpeningReview.json).
Toàn bộ địa hình năm thành phố chưa được chứng nhận.

[Báo cáo và ảnh runtime của checkpoint](../../CrossCityFoodReport.md) là bằng chứng
của đợt triển khai đó; bổ sung quy trình này không thay đổi runtime và không tạo ảnh mới.

## Cập nhật chương 17/10/2026

Huế/Jeonju đã có vòng chơi 22 node mỗi map trong [báo cáo mới](../../HueJeonjuChaptersReport.md). Checkpoint mở đầu ở trên là bằng chứng lịch sử; terrain và art mới vẫn chờ nghiệm thu. Ba thành phố còn lại chưa bật runtime.

## Milestone Đà Lạt 17/10/2026

[Báo cáo Đà Lạt](../../DaLatChapterReport.md): gameplay Windows đã kiểm thử, terrain FAIL/chờ chỉnh và art đề xuất chưa final. Gyeongju/Seoul vẫn DATA ONLY. Không coi checkpoint này là đã hoàn thành ba chương.
