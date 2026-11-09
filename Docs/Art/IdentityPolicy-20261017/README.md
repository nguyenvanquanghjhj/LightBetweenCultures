# Đính chính nhận diện NPC — 17/10/2026

Đã cập nhật quy tắc một danh tính canonical, nhiều visual variants. Không tạo/thay hình, không sửa runtime, portrait binding hoặc UI đã PASS.

## Kết quả

- Bà Tư, ông Phúc, Hương và So-yeon hiện tại: **CANONICAL** theo xác nhận của người dùng. Giữ Cô Mai, Ji-eun, bác lái đò và thương nhân đã chốt trước đó.
- Registry có 27 danh tính theo dữ liệu/event hiện tại, 20 binding NPC; 8 hình mặc định đã có xác nhận canonical. Những hình đề xuất khác không bị tự duyệt hàng loạt. NPC chưa có hình vẫn có danh tính và danh sách variant trống.
- Cô Mai đỏ trong H3: **APPROVED_VARIANT**, dùng native slice đã tồn tại, nhưng **NEEDS_CROP_REVIEW**, chưa đổi binding. Cô Mai vàng trẻ trong Composite_Illustrated và Bà Tư/ông Phúc trong MissingSprites: APPROVED_VARIANT dạng concept-only; chưa có sprite riêng, không dùng ID cả bảng làm nhân vật.
- Cô Mai vàng tóc bạc/lớn tuổi trong MissingSprites và Composite_Checker: **IDENTITY_CONFLICT** do tuổi/nhận diện khác rõ; không phải vì trang phục vàng.
- Hương và So-yeon khác mặt; Ji-eun giữ hình riêng. Ji-won vẫn chưa gán hình, không mượn mặt So-yeon. Thương nhân H4/D4 dùng một danh tính xuyên thành phố.

## Tài liệu và công cụ đã cập nhật

- [NPC_CANONICAL_REGISTRY.md](../NPC_CANONICAL_REGISTRY.md) và JSON cùng tên: danh tính, variantId, path, GUID/localID, thành phố, cảnh, điều kiện; crop quality tách riêng.
- [VisualReview.html](VisualReview.html): xem cạnh nhau các vùng sprite gốc bằng SVG và mở đầy đủ nguồn đã đối chiếu. Đây là review art, không phải ảnh runtime mới. Không tạo PNG.
- Docs/Art/native-art-bindings.json: metadata duyệt hiện hành; giữ toàn bộ tham chiếu sprite. runtimeStatusAnnotation giữ chú thích tích hợp cũ để truy vết.
- ART_ASSET_CATALOG.md, art-asset-manifest.json/CSV, ART_SOURCE_INDEX.md: đồng bộ metadata bằng công cụ inventory hiện có, không chạy migration.
- MISSING_ASSETS.md, CITY_EXPANSION_ART_CHECKLIST.md: bỏ yêu cầu chờ duyệt của bốn NPC đã chốt; liên kết registry hiện hành.
- PortraitStory-20261017/npc-portrait-audit.md: thêm chú thích snapshot lịch sử; không sửa log/ảnh/kết quả PASS cũ.
- Tools/art_asset_catalog.py: overlay duyệt theo **npcId và tham chiếu sprite chính xác**, tạo registry Markdown/review; không duyệt một mặt khác chỉ vì cùng npcId, không đánh xung đột chung cho cả sheet vì màu áo.
- AGENTS.md: quy tắc nhận diện áp dụng cho các lượt sau.

## Kiểm tra

**PASS:** 493 file trong Assets/Packages/ProjectSettings giữ nguyên SHA256; mọi role/ID/path/GUID/localID/spriteName/source trong export giữ nguyên. 27 npcId duy nhất; 20 binding dùng 20 tham chiếu sprite khác nhau. GUID/localID variant đối chiếu với metadata Unity thật. H4/D4 dùng cùng merchant identity. Các link registry/review tồn tại.

**PASS:** metadata của bốn NPC xác nhận đã CANONICAL; thử một GUID khác cùng npcId bị từ chối kế thừa phê duyệt. Kiểm tra này không thay thế review gương mặt; đã mở ảnh gốc/atlas/source và ghi nhận xét cụ thể trong registry.

Không build lại, không chạy lại gameplay vì không có thay đổi runtime. Không tạo full checkpoint; [change-record.json](change-record.json) chỉ lưu delta metadata cũ và hash bảo vệ, dựa trên CheckpointFullStoryComplete đã có. [Kết quả máy kiểm tra](verification.json).

## Giới hạn

Duyệt danh tính không chứng nhận animation hoặc mọi crop chưa dùng. Các source concept-only chưa được tách thành sprite mới. AdventureUIArt.status trong asset Unity giữ nguyên chú thích tích hợp lịch sử để bảo toàn asset/PASS hiện có; nguồn phê duyệt danh tính hiện hành là registry này. Sau export Unity, inventory tool áp metadata theo registry; không dùng nhãn tạm cũ để yêu cầu chốt lại mặt.
