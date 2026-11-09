# ART BIBLE v1 — LIGHT BETWEEN CULTURES

## 1. Hướng hình ảnh chính

- **Thể loại:** 2D casual exploration / node-map adventure, một số scene di chuyển tự do.
- **Góc nhìn city map:** bản đồ minh họa từ trên xuống, nhà/công trình nhìn thấy mái và mặt trước nhẹ, các điểm đi nối bằng đường cong.
- **Góc nhìn free exploration:** top-down 3/4 nhẹ như ảnh người dùng cung cấp (bờ nước, cầu, nhà, nhân vật nhỏ, có lớp chắn và sorting).
- **Phong cách:** vẽ tay hoạt hình 2D sạch, viền nâu đậm tương đối mảnh, khối sáng-tối mềm, màu tươi nhưng nền không cạnh tranh với nhân vật. **Không yêu cầu pixel art thuần pixel-grid** dù ảnh tham khảo giàu chi tiết.
- **Cảm giác:** thân thiện, hài hước, phiêu lưu; có bí ẩn vừa phải nhưng không u ám/horror.
- **Chơi trên Android portrait 9:16**, bố cục UI tương thích tai thỏ và vùng an toàn. Chỉ dùng ảnh minh họa và sprite riêng, không phụ thuộc raster kích thước thiết bị.

## 2. Nguồn nào có quyền quyết định thiết kế?

Thứ tự ưu tiên:
1. `References/Lumi_CANONICAL_user_image.png` = ngoại hình gà **bắt buộc**.
2. Ảnh góc nhìn do người dùng gửi `GameplayPerspective_reference_from_user.png` = camera và tỷ lệ không gian.
3. `Maps/HoiAn_Node_Flow.svg` + `Data/hoi_an_map.json` = **vị trí node/đường đi**.
4. Hai `ArtReference_*_AI.png` = **tham khảo không khí, ô màu, bố cục UI**, KHÔNG xem là thiết kế sprite cuối. Chúng hiển thị gà trắng, không đúng nhân vật gà màu nâu cam.

### Gà Lumi — chi tiết không được thay đổi ngẫu nhiên

- Đầu/gò má nâu cam, hai mắt tròn nâu lớn, mồng đỏ, mỏ nâu, dáng gà chibi dễ thương.
- Trang phục xanh lam/navy đậm, đường viền vàng, thắt lưng xanh ngọc, hoạ tiết trang trí tiết chế; có đèn nhỏ ánh vàng ở thắt lưng.
- Nhiệm vụ tạo sprite in-game: bảo tồn silhouette mồng + đầu lớn + áo dài ngắn cách điệu + dây thắt lưng, nhưng **rút hoa văn siêu nhỏ** để đọc rõ ở tỷ lệ nhân vật 96–128px.
- Không tự đổi Lumi thành gà trắng/mặc áo cam chỉ vì AI moodboard tạo vậy.
- Bổ sung bộ pose: idle/đi xuống, lên, trái, phải (tối thiểu 4 frame mỗi hướng), tương tác, vui, ngạc nhiên. **Cần sprite sheet đồng bộ thật sự**, không cắt ngẫu nhiên các pose từ bảng art concept.
- Outfit hiện tại là trang phục hư cấu pha cách điệu, không mô tả như một cổ phục chính thức của quốc gia nào.

## 3. Thước đo nhất quán

- Free scene: `1 ô logic = 64 px art basis = 1 unit` nếu importer `Pixels Per Unit = 64`.
- Sprite player gợi ý frame 128 × 128px, chân luôn đặt quanh điểm neo đáy; chiều cao hình nhân vật hiển thị khoảng 1.25–1.6 tiles (tuỳ frame).
- NPC frame 128 × 160px, cùng PPU và góc nhìn player.
- Nhà 384–512px art source; pivot ở **chân nhà**; collider chỉ bao footprint, không bao toàn bộ mái.
- Props nhỏ: 64/128px; UI icon 128/192px; background city map đề xuất 1536 × 2048px hoặc 1080 × 1920px, **không chữ và không marker**. Nếu ảnh là background city map, đường node và icon phủ bằng Unity UI/Sprite riêng.
- Camera free scene Orthographic, cho player vào khung hình thật ở portrait; kèm camera follow/bounds. Không giữ camera cố định `(0,0)` khi player ở xa.

## 4. Bảng màu nền chính

| Phần | Hex | Quy tắc |
|---|---|---|
| Tường cổ vàng ấm | `#F4C368` | House background / Hội An |
| Vàng ánh đèn | `#E7A53F` | Ánh sáng và phần thưởng |
| Đỏ son | `#D15C3D` | Đèn lồng, chi tiết nổi bật |
| Xanh ngọc | `#367F79` | Sông / UI node thường |
| Xanh xanh than | `#224C65` | Viền UI / phần tối |
| Kem UI | `#FFF3D9` | Lời thoại, panel |
| Xám khóa | `#617081` | Node bị khóa, không lấn điểm tương tác |

Các vùng khác dùng `Data/world_cities.json`. Mỗi thành phố có palette riêng để người chơi nhận ra ngay; tỷ lệ sáng/tối của UI và marker phải giữ độ tương phản.

## 5. Nguyên tắc dễ đọc trên điện thoại

- Nhân vật, NPC và vật phẩm tương tác có đường viền rõ hơn nền 10–20%.
- NPC có icon hội thoại nhỏ; nhiệm vụ có `!`; bot có dấu cảnh báo; rương bí mật **không luôn lộ ngay**.
- Không đặt tên địa điểm thành chữ cố định trong ảnh nền. Dùng Unity TextMeshPro/UI đọc từ dữ liệu.
- Nút tương tác vùng chạm tối thiểu khoảng 44–48 px vật lý tương đương trên thiết bị mục tiêu; kiểm tra thực tế nhiều máy.
- Zoom cảnh bản đồ vừa đủ thấy 3–5 điểm liên quan, cho kéo/scroll mượt; không ép cả 10 node vào một viewport nhỏ.

## 6. Layer / sorting khi dựng Unity

**Free exploration** (thứ tự đề xuất): `Ground`, `Road`, `Water`, `BuildingsBase`, `PropsBehind`, `Actors`, `PropsFront`, `AboveActors`, `VFX`, `UI`.

- Di chuyển sau mái/qua cầu cần đúng theo *feet Y* hoặc `SortingGroup` theo đối tượng; không chỉ đặt `sortingOrder` cứng cho mọi nhà.
- Collider phục vụ logic đi lại chỉ đặt vào chân nhà, tường, nước, vật cản; vùng đèn trang trí không nhất thiết collider.
- Nhiệm vụ, marker và hội thoại nằm ở UI/Canvas/Sorting tương ứng, không trộn hết vào cùng SpriteRenderer.

## 7. Tài nguyên AI vs tài nguyên sử dụng thật

Hai bảng tổng hợp AI đi kèm là **concept moodboard**; có thể có chữ sai, nhân vật chưa giống 100%, tile chưa seamless và viền cắt chưa chuẩn. Không auto-slice thành asset rồi đưa thẳng vào dự thi.

Quy trình làm art thật:
1. Tạo **mỗi asset một ảnh riêng** trên nền trong suốt (nhân vật/prop), hoặc seamless tile chuẩn (đất/đường/nước).
2. Kiểm tra kích thước, pivot, ánh sáng, hướng camera và tỷ lệ.
3. Import Sprite (2D and UI), cấu hình PPU theo nhóm, lọc Bilinear cho painted art, Point chỉ khi chuyển hẳn pixel art.
4. Tạo prefab/sprite atlas phù hợp; chỉnh collider thủ công những điểm quan trọng.
5. Chạy test trong Game View trên máy thật; asset nào không rõ bản quyền hoặc xuất xứ thì không dùng.
