> Cập nhật sau quyết định linh hoạt: Ji-won đã có portrait canonical; S14 không cần người mới; bốn vai còn cần art. [Kết luận mới nhất](../NPC_REUSE_FINAL_DECISION.md). Các số liệu bên dưới là snapshot kiểm kê trước reuse.

# Kiểm kê NPC — 17/10/2026

[Ảnh review (HTML, mở bằng trình duyệt)](Review.html) · [Bảng CSV](NPCs.csv) · [Chi tiết GUID/localID và 31 sprite chưa gán](inventory.json)

Kiểm kê và đánh giá tận dụng ảnh theo quyết định bổ sung của người dùng. Không đổi gameplay/binding/art hoặc tự duyệt 12 hình đang dùng. Registry/catalog chỉ bổ sung kết luận portrait: sáu NEEDS_NEW_ART, D18 NOT_REQUIRED_CURRENT_GAMEPLAY. Không chạy lại gameplay trong bước này: runtime mới **NOT VERIFIED**.

[Kết luận tận dụng art cũ và lý do từng ứng viên](PortraitReuseAssessment.md). Không có ứng viên đủ điều kiện gán canonical mới; số đã giải quyết bằng binding mới: **0**.

## 1. Có hình đang gán nhưng chưa duyệt canonical (12)

| NPC | Node | Sprite hiện tại | Tương tác đang cấu hình |
|---|---|---|---|
| Người giữ chuyện (`npc_elder`) | Jeonju J16 | [npc_elder](../../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png) | npc_elder [dialogue]; chapter_jq04_offer [quest] → JQ04 (Thư gửi phố cũ) |
| Người Làm Vườn (`npc_gardener`) | Đà Lạt D13 | [npc_gardener](../../../Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png) | npc_gardener [npc] → DQ04 (Hoa trong mây) |
| Nghệ nhân Hana (`npc_hana`) | Jeonju J2 | [npc_hana](../../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png) | npc_hana [npc] → JQ01 (Hoa văn ba miền) |
| Chó Canh Đường (`npc_hungry_dog`) | Đà Lạt D10 | [npc_hungry_dog](../../../Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png) | npc_hungry_dog [npc] → DQ02 (Bạn chó canh đường) |
| Người dẫn đường Lan (`npc_lan`) | Đà Lạt D2 | [npc_lan](../../../Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png) | npc_lan [npc] → DQ01 (Kính giữa sương); dalat_return_glasses [item] |
| Hướng dẫn viên Min (`npc_min`) | Gyeongju G3 | [npc_min](../../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png) | npc_min [npc] → GQ01 (Đường sao Silla); min_fox_food [item] |
| Bà Nguyệt (`npc_nguyet`) | Huế U2 | [npc_nguyet](../../../Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png) | npc_nguyet [npc] → UQ02 (Thảo dược Bà Nguyệt) |
| Thương nhân Park (`npc_park`) | Jeonju J5 | [npc_park](../../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png) | npc_parcel [npc] → JQ02 (Kiện hàng thất lạc) |
| Nho Sinh Lê An (`npc_scholar`) | Huế U7 | [npc_scholar](../../../Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png) | npc_scholar [npc] → UQ01 (Dấu ấn hai bờ); chapter_uq04_return [quest] → UQ04 (Tấm ấn gửi đi xa) |
| Bác Lái Đò Tâm (`npc_tam`) | Huế U3 | [npc_tam](../../../Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png) | npc_ferryman [npc] → UQ03 (Qua sông an toàn) |
| Cô Thủy chủ tiệm hoa (`npc_thuy`) | Đà Lạt D3 | [npc_thuy](../../../Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png) | npc_thuy [npc]; dalat_thuy_food [item]; dalat_dog_food_recovery [recovery] → DQ02 (Bạn chó canh đường) |
| Nhà thầy Yoon (`npc_yoon`) | Jeonju J4 | [npc_yoon](../../../Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png) | npc_yoon [npc] |

Các binding này giữ nguyên. Có event/quest trong source không đồng nghĩa đã kiểm thử runtime lại hay được người dùng duyệt danh tính.

## 2. Vai trò chưa có portrait (7)

| Vai trò | Node | Tương tác cấu hình | Ứng viên / giới hạn |
|---|---|---|---|
| Bến phà thông tin (`npc_boat`) | Seoul S14 | npc_boat [npc] | Event mang tên địa điểm Bến phà thông tin; chưa xác định nhân dạng người nói. Không dùng bác lái đò Hội An/Tâm làm người khác. |
| Quản thủ bảo tàng (`npc_curator`) | Seoul S2 | npc_curator [npc] → SQ02 (Lời nhắn cũ) | Chưa thấy ứng viên đủ căn cứ cho quản thủ Seoul; không mượn Min, Yoon hoặc Phúc. |
| Kỹ sư Trạm Số (`npc_engineer`) | Seoul S11 | npc_engineer [npc]; sq02_finish [quest] → SQ02 (Lời nhắn cũ) | Chưa thấy ứng viên kỹ sư độc lập đủ căn cứ; không dùng lại So-yeon. |
| npc_innkeeper (`npc_innkeeper`) | Đà Lạt D18 | npc_innkeeper [start] — Lumi dẫn chuyện, chưa có NPC nói | NOT_REQUIRED_CURRENT_GAMEPLAY: D18 là lời dẫn Lumi/nghỉ ngơi, chưa thể hiện chủ trọ. Không gán đầu bếp để lấp chỗ trống. |
| Kho bản khắc (`npc_inscription`) | Gyeongju G14 | npc_inscription [npc] | NEEDS_NEW_ART: chưa có nhân vật độc lập phù hợp kho bản khắc Gyeongju; không mượn Phúc/Yoon/Min. |
| Sân chính di tích (`npc_keeper`) | Gyeongju G9 | npc_keeper [npc] | NEEDS_NEW_ART: người lính trong roster là lính Hoàng thành Huế, không hợp bối cảnh Gyeongju; không ép gán. |
| Ji-won (`npc_tech`) | Seoul S7 | npc_tech [npc] → SQ01 (Ba nhịp đồng bộ) | Ji-won chưa có ứng viên đủ căn cứ; tuyệt đối không dùng mặt So-yeon. |

## 3. Sprite/ảnh chưa gán

| Phân loại trong 31 native sprite chưa gán | Số lượng | Nguồn / nhận xét |
|---|---:|---|
| Người đơn lẻ, ứng viên cần review | 1 | H3_07 `_14`: đầu bếp; kiểm tra biên phải/chảo, chưa dùng |
| Biến thể danh tính Mai đã có | 1 | H3_07 `_19`, không gán cho người khác; crop cần review |
| Ảnh ghép nhiều người | 2 | H3_07 `_21` (Phúc + người gánh hàng), `_22` (nữ Hàn + khách chụp ảnh); chưa là sprite riêng |
| Bot/thiết kế dành cho bot | 2 | H3_07 `_24`, `_28`, không là ứng viên người |
| Mảnh auto-slice | 22 | H3_07: chi tiết nhỏ/ảnh thiếu bộ phận, không là 22 nhân vật |
| Sheet nguyên | 3 | Characters_MissingSprites_v1, LBC_Composite_CheckerConcept_v2, LBC_Composite_Illustrated_v1; không dùng nguyên sheet làm NPC |

Tổng phạm vi native NPC: **51 sprite records = 20 đang gán + 31 chưa gán**. Atlas Shared có sáu sprite được import hiện tại, cả sáu đã gán; người lính chỉ hiện trong pixels nguồn, chưa có sprite riêng. Người gánh hàng trẻ là ứng viên nguồn khác nhưng chưa có vai trò phù hợp chắc chắn. Hai nhân vật hàng dưới H3 cần đối chiếu nhận diện trước, không tự tính là mặt trống. Những nhân vật trên composite là concept/biến thể, không cộng lại thành NPC độc lập.

Ngoài 51 bản ghi trên còn **4 ảnh nháp NPC** trong `DaLatRejectedDrafts` (Lan, Thủy, người làm vườn, chó). Đây là bản nháp của vai trò đã có, không phải bốn nhân vật tự do. Ảnh review có đối chiếu nguồn; không đưa chúng vào gameplay.

Kiểm kê dùng registry, native bindings, manifest sẵn có; đọc đúng PNG/meta liên quan trong Characters/NPC và các sheet nhân vật. Lumi và enemy giữ vai trò riêng, không coi là NPC chưa dùng. Không quét Backups/Library/Builds.

## 8 danh tính đã xác nhận giữ nguyên

Bà Tư (`npc_ba_tu`), Bác Lái Đò (`npc_boatman`), Ji-eun (`npc_guest`), Chị Hương (`npc_huong`), Cô Mai (`npc_mai`), Thương nhân đường xa (`npc_market_merchant`), Ông Phúc (`npc_phuc`), So-yeon (`npc_soyeon`).

Sáu vai trò thực sự có người/đối tượng phát biểu vẫn NEEDS_NEW_ART. D18 không cần portrait chủ trọ ở gameplay hiện tại. Không tạo NPC, quest, cốt truyện hoặc art mới.

## Kiểm tra bảo toàn

Đối chiếu đúng 46 file được chụp trước lượt: xem [integrity.json](integrity.json). Chỉ ba tài liệu registry/catalog được phép bổ sung kết luận; các file runtime/source/art/settings giữ nguyên. Build Windows được đồng bộ lại sau khi thu hồi nháp duyệt hàng loạt; log `Logs/npc-inventory-restored-build.log`. Không dùng build nháp trước đó hoặc kết quả guard 20 canonical làm bằng chứng. Không tạo checkpoint gameplay mới vì production inputs giữ nguyên; checkpoint gần nhất vẫn là CheckpointQualityAddendum-20261017.
