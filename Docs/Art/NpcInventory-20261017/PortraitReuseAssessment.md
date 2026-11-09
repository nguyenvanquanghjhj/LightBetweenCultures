> Cập nhật sau quyết định linh hoạt: Ji-won đã có portrait canonical; S14 không cần người mới; bốn vai còn cần art. [Kết luận mới nhất](../NPC_REUSE_FINAL_DECISION.md). Các số liệu bên dưới là snapshot kiểm kê trước reuse.

# Đánh giá tận dụng nhân vật chưa định danh — 17/10/2026

[Ảnh đối chiếu nguồn](Review.html) · [Bảng 27 vai trò](README.md) · [Kết luận máy đọc được](PortraitReuseAssessment.json)

Áp dụng quyết định mới: được gán canonical nếu ứng viên chưa có danh tính khác, phù hợp vai trò/bối cảnh và đủ chất lượng sprite runtime. **Không tìm thấy ứng viên đáp ứng đủ ba điều kiện cho các vị trí thiếu hiện tại.** Không gán mới hoặc duyệt hàng loạt 12 hình đang dùng.

| Kết quả | Số lượng |
|---|---:|
| Portrait thiếu được giải quyết bằng binding mới | 0 |
| Danh tính canonical mới | 0 |
| Vai trò tương tác vẫn cần hình phù hợp (`NEEDS_NEW_ART`) | 6 |
| Khóa không cần portrait ở gameplay hiện tại | 1 — D18 |

## Các vị trí cần hình

| NPC / node | Kết luận và lý do |
|---|---|
| `npc_keeper` — Gyeongju G9 | NEEDS_NEW_ART: người giữ di tích. Ứng viên alpha rõ còn lại là **lính Hoàng thành Huế**, không phù hợp bối cảnh. Không chuyển vai trò/người Hàn thành lính Huế để khớp ảnh. |
| `npc_inscription` — Gyeongju G14 | NEEDS_NEW_ART: người hỗ trợ bản khắc/biểu tượng. Không mượn Phúc, Yoon hoặc Min; người gánh hàng/đầu bếp không khớp vai trò. |
| `npc_curator` — Seoul S2 | NEEDS_NEW_ART: quản thủ/người quản lý di tích. Hình Tech-Curator trong nguồn đã gán cho **So-yeon**; không tái sử dụng thành một người khác. |
| `npc_tech` — Ji-won, Seoul S7 | NEEDS_NEW_ART: nhân viên công nghệ độc lập với So-yeon. Không có ứng viên sạch phù hợp được xác minh. |
| `npc_engineer` — Seoul S11 | NEEDS_NEW_ART: kỹ sư trạm. So-yeon đã có danh tính; nghệ nhân cầm giấy trong composite chỉ là hình nhỏ có nền, chưa có sprite đủ chất lượng. |
| `npc_boat` — Seoul S14 | NEEDS_NEW_ART: tương tác bến phà có thật nhưng nhân dạng người nói chưa được thể hiện. Không đồng nhất với bác lái đò Hội An hoặc Tâm. Hình mới phải hợp bến phà Seoul, không thêm NPC hay đổi lời thoại để khớp ảnh. |
| `npc_innkeeper` — Đà Lạt D18 | **NOT_REQUIRED_CURRENT_GAMEPLAY**: event `type=start`, `actor=Lumi`, nội dung nghỉ ngơi/dẫn đường. Không có chủ trọ xuất hiện/phát biểu. Không gán đầu bếp, không thêm tương tác mới. |

Registry giữ `MISSING_VISUAL` cho bảy khóa chưa có binding; trường mới `portraitAssessment.assetStatus` phân biệt sáu nhu cầu art thực tế với D18 không cần hình. Không đổi `status`, `canonicalIdentity`, `placements`, `variants` hoặc reference của NPC nào.

## Nhân vật/nguồn còn lại và lý do chưa dùng

| Nguồn | Hình / trạng thái | Đánh giá |
|---|---|---|
| H3_07 PNG + meta | Đầu bếp, native `H3_07_Characters_NPC_Concept_14`, GUID `ab62ba1e558f7274984500bb1cac18f6` | Một ứng viên người đơn lẻ chưa gán; localID đầy đủ trong inventory.json. Cần kiểm tra biên chảo/viền trước dùng. Không phù hợp sáu vai trò thiếu hiện tại; D18 chưa có chủ trọ trong gameplay. Không chốt canonical. |
| Shared NPC atlas + roster gốc | Lính Hoàng thành Huế, chưa có native sprite riêng | Có thể có nguồn alpha để tách, nhưng sai bối cảnh Gyeongju/Seoul. Giữ chưa gán; không tạo một NPC Huế mới. |
| H3_07 `_21` | Người gánh hàng trẻ đứng cạnh Phúc | Chưa cắt độc lập. Không đổi thương nhân canonical sang mặt này; chưa có vai trò thiếu phù hợp chắc chắn. |
| H3_07 `_22` | Nữ Hàn mặc hanbok + khách cầm máy ảnh | Hai người trong một sprite. Nhận diện có thể liên quan các variant Hana/Min/Ji-eun; **chưa xác nhận là mặt trống**, không tự gán làm người khác. |
| `Characters_MissingSprites_v1.png` | Ảnh nguyên 544×359, nhân vật nhỏ, chữ/nền dính sẵn | Có trẻ em, đầu bếp, người gánh hàng và khách; không là các sprite runtime sạch. Nhiều hình tương ứng những vai trò/biến thể đã có, không đếm lại thành NPC độc lập. |
| `LBC_Composite_CheckerConcept_v2.png` | Sheet nguyên 1536×1024, vùng nhân vật ở dưới giữa | Concept chứa mặt đã có và các hình khách/đầu bếp/trẻ em. Không dùng nguyên bảng hay gán một variant thành người mới. |
| `LBC_Composite_Illustrated_v1.png` | Nghệ nhân đeo kính cầm giấy, đầu bếp, người gánh hàng, trẻ em và khách Hàn trong vùng nhỏ | Nghệ nhân cầm giấy khoảng 100×135 px, có nền. Chưa đủ chứng cứ là sprite sạch đạt kích thước portrait hiện tại (khung khoảng 220×216 trước display scale); không phóng lớn rồi gọi final. Các hình khác chưa có vai trò thiếu phù hợp. |
| DesignKit `12_HoiAn_NPC_Concepts.png` | Nguyên ảnh chỉ 296×338: 8 hình minh họa (Mai, thương nhân, ngư dân, trẻ em, sư thầy, bà lão, khách, bảo vệ) | Mỗi người khoảng 50–70×110–130 px và có nền/chữ. Không đủ chất lượng/cắt sạch để dùng portrait hiện tại; nhận diện Mai/khách cũng không được tự thay. Đây là **8 hình concept**, không phải 8 sprite có thể gán ngay. |
| `Characters_Moodboard.png` | Mai, guard-scholar, florist, hanbok artisan, archaeology guide, tech-curator, boatman, merchant | Vai trò/nhận diện liên quan các NPC đã gán. Khác pose/trang phục không tự thành NPC mới; đặc biệt Tech-Curator không là mặt trống để dùng lại cho Ji-won. |
| `DaLatRejectedDrafts/npc_*.png` | 4 bản nháp Lan/Thủy/người làm vườn/chó | Thuộc những vai trò đã có; không là 4 NPC chưa định danh và không được tự tái sử dụng. |

**Số đếm không nhập nhằng:** 31 native sprite chưa gán gồm 1 ứng viên người đơn lẻ, 1 biến thể Mai, 2 ảnh ghép nhiều người, 2 bot, 22 mảnh auto-slice và 3 sheet nguyên. Ngoài ra có các vùng hình chưa cắt trong nguồn; không thể cộng các phiên bản lặp qua sheet thành số danh tính độc lập. Danh sách sprite, rect, GUID/localID và nguồn có trong [inventory.json](inventory.json).

## Kiểm tra và bảo toàn

- Đã mở trực quan atlas, H3_07, hai composite, MissingSprites, DesignKit 12, roster và moodboard; đối chiếu cả nhân dạng/vai trò, không chỉ tên file/GUID.
- Đã đối chiếu node G9/G14/S2/S7/S11/S14/D18 với JSON gameplay canonical và event runtime hiện tại.
- Không thêm/sửa portrait runtime: kiểm thử binding/hiển thị **mới không áp dụng**. Không dùng ảnh concept để tuyên bố đã hiển thị trong game.
- Giữ nguyên 8 danh tính đã xác nhận, 12 hình chờ duyệt, gameplay, scene, art, save và build đang đồng bộ. Chỉ bổ sung báo cáo cùng trạng thái nhu cầu art trong registry/catalog.
- Kết quả đối chiếu file giới hạn: [integrity.json](integrity.json). Không build lại chỉ vì thay tài liệu; không tạo full backup/checkpoint trùng.
