# Kiểm tra vị trí NPC — 25/10/2026

Kết quả: **29 NPC tại 6 City Map; 17 vị trí đạt và giữ nguyên; 12 vị trí có lỗi hiển thị, trong đó 6 đã sửa và 6 để owner quyết định.** Không tính enemy hoặc marker thông tin là NPC. Chó D10 được tính. G16, G21 và S12 đều được kiểm tra.

Ảnh Windows Player: 720×1280, zoom 2.4, cùng save QA hoàn thành sao chép từ `Validation/PcGameplayFix-20261024/campaign/pc-final-state.json`. Mỗi NPC được đặt fixture tại đúng node canonical để soi chân, sau đó click sprite bằng chuột; hệ thống tiếp cận hiện có tự xử lý. Fixture đặt node không được dùng làm bằng chứng đi hết campaign. Ảnh đo chân dùng mesh sprite thực tế; kết luận nền cảnh được đọc trực quan từ ảnh. Không tuyên bố kiểm thử mọi zoom/tỉ lệ cửa sổ.

## Thay đổi được giữ lại

| NPC / node | Offset cũ → mới (UI unit) | Δx, Δy | Lý do | Chuột / hội thoại / đóng / đi tiếp |
|---|---|---|---|---|
| npc_lan / D2 | [22.0, 8.0] → [28, 32] | [6.0, 24.0] | Đưa chân từ mép lan can/nước về lối đi ven hồ. | PASS |
| npc_thuy / D3 | [22.0, 8.0] → [-12, 30] | [-34.0, 22.0] | Đưa chân từ mép rào về lối lát đá phía trong. | PASS |
| npc_gardener / D13 | [22.0, 8.0] → [22, -5] | [0.0, -13.0] | Hạ chân từ mép luống hoa về mép lối vườn. | PASS |
| npc_park / J5 | [22.0, 8.0] → [22, -8] | [0.0, -16.0] | Hạ chân từ chân tường/cửa xuống nền trước cửa. | PASS |
| npc_min / G3 | [22.0, 8.0] → [52, 25] | [30.0, 17.0] | Đưa chân khỏi bụi cây sang lối cạnh hàng rào đài. | PASS |
| npc_han_doyun / S12 | [-24.0, 5.0] → [-24, 40] | [0.0, 35.0] | Đưa chân khỏi cây/bồn hoa lên lối công viên phía trên. | PASS |

Kích thước, sprite ID, ảnh nguồn, anchor node và phạm vi tương tác được giữ nguyên. Δ dương x là sang phải, Δ dương y là lên trên. Đây là offset UI đã counter-scale theo hệ thống sẵn có, không phải tọa độ canonical.

## Chưa sửa an toàn

| Node | Kết luận |
|---|---|
| U2 | Chân trên mái nhà thuốc; nền đứng phù hợp cách xa, chưa có chỉnh nhỏ được xác nhận. |
| U7 | Chân sát cây và prop ngai; chưa tìm được offset nhỏ đồng thời tránh vật trang trí và Lumi. |
| D4 | Chân sát bồn hoa; chỉnh thử che NPC bởi Lumi, đã hoàn nguyên. |
| D10 | Chân trên tán cây; chỉnh thử vẫn chưa đưa đủ chân ra nền, đã hoàn nguyên. |
| G16 | Chân trên cây/mái bên đền; chỉnh thử bị hệ thống tiếp cận chặn, đã hoàn nguyên. |
| G21 | Chân ở bụi hoa; chỉnh thử vẫn nằm trong mảng cây/bồn hoa, đã hoàn nguyên. |

## Kiểm kê toàn bộ

| City | Node | NPC | Hiển thị | Tương tác runtime |
|---|---|---|---|---|
| hoi_an | H2 | npc_mai | Đạt; giữ nguyên | PASS |
| hoi_an | H4 | npc_market_merchant | Đạt; giữ nguyên | PASS |
| hoi_an | H17 | npc_boatman | Đạt; giữ nguyên | PASS |
| hoi_an | H18 | npc_guest | Đạt; giữ nguyên | PASS |
| hoi_an | H10 | npc_ba_tu | Đạt; giữ nguyên | PASS |
| hoi_an | H14 | npc_phuc | Đạt; giữ nguyên | PASS |
| hue | U1 | npc_huong | Đạt; giữ nguyên | PASS |
| hue | U2 | npc_nguyet | Owner quyết định | PASS |
| hue | U3 | npc_tam | Đạt; giữ nguyên | PASS |
| hue | U7 | npc_scholar | Owner quyết định | PASS |
| da_lat | D2 | npc_lan | Đã sửa | PASS |
| da_lat | D3 | npc_thuy | Đã sửa | PASS |
| da_lat | D4 | npc_market_merchant | Owner quyết định | PASS |
| da_lat | D10 | npc_hungry_dog | Owner quyết định | PASS |
| da_lat | D13 | npc_gardener | Đã sửa | PASS |
| jeonju | J15 | npc_soyeon | Đạt; giữ nguyên | Mở/đóng PASS; đi tiếp chưa xác nhận |
| jeonju | J2 | npc_hana | Đạt; giữ nguyên | PASS |
| jeonju | J4 | npc_yoon | Đạt; giữ nguyên | PASS |
| jeonju | J5 | npc_park | Đã sửa | PASS |
| jeonju | J16 | npc_elder | Đạt; giữ nguyên | PASS |
| gyeongju | G3 | npc_min | Đã sửa | PASS |
| gyeongju | G9 | npc_keeper | Đạt; giữ nguyên | PASS |
| gyeongju | G14 | npc_inscription | Đạt; giữ nguyên | BLOCKED tiếp cận |
| gyeongju | G16 | npc_kang_taeho | Owner quyết định | Trial bị chặn; offset đã hoàn nguyên |
| gyeongju | G21 | npc_choi_eunsu | Owner quyết định | PASS |
| seoul | S2 | npc_curator | Đạt; giữ nguyên | PASS |
| seoul | S7 | npc_tech | Đạt; giữ nguyên | PASS |
| seoul | S11 | npc_engineer | Đạt; giữ nguyên | PASS |
| seoul | S12 | npc_han_doyun | Đã sửa | PASS |

## Giới hạn và lỗi có sẵn

- Cả 6 offset được giữ lại đều PASS click sprite → tiếp cận → hội thoại → nút X → Lumi đi sang node lân cận. Đọc/đóng không đổi save.
- G14 có nền đứng hợp lý và được giữ nguyên, nhưng tiếp cận bị chặn ngay cả khi không sửa vị trí. Đợt baseline bằng bộ test cũ cũng dừng khi chờ hội thoại G14. Không tự sửa graph.
- J15 mở/đóng hội thoại được; lượt click node lân cận chưa xác nhận đi tiếp. Đây là vị trí giữ nguyên; cần owner kiểm tra riêng. Không kết luận là lỗi đường đi từ một fixture.
- Log runner kết thúc `ALL CHECKS PASSED` nhưng có ba dòng `REVIEW` (J15/G14/G16); vì vậy nghiệm thu toàn bộ được ghi **PARTIAL**, không dùng dòng cuối để tuyên bố 29/29 tương tác đạt.
- Tại vị trí tới node của fixture, Lumi có thể che một phần NPC. Những trường hợp còn chưa xác nhận được khoảng hở hoàn toàn không được tuyên bố PASS về mọi trạng thái tiếp cận.

## Hoạt động đánh chuông

Đã thấy mảnh pixel cột/dây bị tách khi chày đung đưa. Mask chữ nhật cũ bao cả vùng gần cột/móc; trial chia rope mask tạo thêm đoạn dây đứng yên, nên đã hoàn nguyên toàn bộ `GameplayPanels.Leisure.cs`. Chưa giữ lại bản sửa chuông. Chuột kéo/thả, đóng panel và trở lại map vẫn PASS ở bản gốc. Cần tinh chỉnh mask được duyệt và kiểm tra nhiều pha chuyển động trước khi chốt; không đổi bitmap artwork.

## Bằng chứng và tính toàn vẹn

- Mở `review.html` để xem trước/sau **chỉ 6 NPC được giữ sửa**; ảnh gốc nằm trong `before/Placement-<node>.png` và `after/Placement-<node>.png`.
- Sáu contact sheet runtime trong `before/` ghi nhận toàn bộ 29 NPC. Ảnh trial khác là chẩn đoán, không phải bản sửa đã nghiệm thu.
- `after/placement-runtime.csv`, `after/smoke-results.txt`, log Player, và hai build log được giữ nguyên, không sửa kết quả lịch sử.
- `final-integrity.json`: chỉ sáu offset actor khác baseline; toàn bộ 133 node, graph Phase B, junction, ferry, gameplay, quest/reward/save/ending, artwork, ID và kích thước sprite không đổi. Ba file QA runner được thêm/sửa để chạy focused test, không thêm QA framework.
- Hai lượt build Windows phục vụ audit và thử sửa; không Android, không campaign, không checkpoint lớn. Build `Windows-NpcPlacementQA-20261025` là trial chẩn đoán (còn chứa các chỉnh đã loại), **không phát hành như bản cuối**. Source cuối giữ sáu offset đã PASS và hoàn nguyên bốn trial cùng phần chuông. Không rebuild lần ba chỉ để đóng gói các hoàn nguyên.
- Không xóa capture có tham chiếu trong manifest. Không ghi vào save của owner.

Hai file QA khác đổi trong workspace trong lúc audit, không do lượt này sửa và không hoàn nguyên: `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.PcGameplay.cs`, `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.StoryLeisure.cs`. Chúng không thuộc bằng chứng nghiệm thu của task này.
