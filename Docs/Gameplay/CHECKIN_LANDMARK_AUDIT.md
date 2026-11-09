> **Cập nhật tích hợp18 ảnh —18/10/2026:**18 CheckInDefinition giữ nguyên, nay có18 derived gallery binding từ bộ nguồn được chọn và Sổ ảnh văn hóa chung sáu city. H6 giữ board/2 symbol/hotspot/proof/reward; chỉ gallery đổi theo bổ sung mới nhất. Trạng thái Windows PASS, visual pending, Android NOT_TESTED. [Báo cáo](CHECKIN_18_ART_INTEGRATION_REPORT.md) · [Mapping](../Art/CHECKIN_LANDMARK_IMAGE_MAPPING.md). Phần Phase0/pilot cũ bên dưới là lịch sử, các số0 gesture/album và mô tả Map A ROI đã được supersede ở phạm vi18 entry; không tự thêm H18.

# Camera / check-in / Cultural Photo Journal — audit & pilot design

2026-10-18 · Phase0 audit plus approved landmark additions; no new quest/relic/save/UI system. [Inventory máy đọc](Phase0-20261018/checkin-marker-inventory.json).

## Đếm đúng cơ chế

**17 node có khả năng hiện Camera theo rule/default** (tùy quest/discovery, không đồng thời), **18 CheckInDefinition có handler thật** (6 Hội An, 2 Huế, 2 Jeonju, 2 Đà Lạt, 3 Gyeongju, 3 Seoul). Union18 vị trí; **H18 chỉ MARKER_ONLY**, chưa có CheckInDefinition. D0/D14 có runtime button/view nhưng không có Camera rule riêng trong presentation. Gyeongju/Seoul nay đã có 3 check-in definition mỗi map, dùng node thật G4/G14/G17 và S2/S10/S18.

12 nơi có panel xem/ghi dấu/reward một lần theo source, **0 nơi có minigame photo gesture**, **0 Cultural Photo Journal album toàn game**, **0 capture screenshot gameplay** trong luồng này. Không coi icon camera hoặc RawImage là đã chụp ảnh. Historical Full Story Coverage03 có tương tác check-in, không dùng để gọi12 nơi vừa test lại trong Phase0.

`GameplayPanels.ShowCheckIn` dùng **RawImage ROI từ Map Overview A** quanh node; loại **GENERATED_POSTCARD_VIEW**, không CAPTURED_SCREENSHOT. NodeEventManager.CanCheckIn kiểm node IsVisible/requiredFlags/chưa completionFlag; Execute checkin đặt flag và grant xu/items bằng ledger. Mark rồi có checkin_view để mở lại, không thưởng. Inventory hiện có ô item/icon/description, chưa có album xem tất cả postcard khung lớn. QA ScreenCapture trong runner không phải chức năng camera người chơi.

| City/node | Definition / điều kiện | Trạng thái | Art/view thật | Replay / liên quan mảnh |
|---|---|---|---|---|
| hoi_an:H2 | hoi_an.h2; ['hoi_an.lamp_restored'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| hoi_an:H3 | hoi_an.h3; ['hoi_an.lamp_eye_found'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| hoi_an:H4 | hoi_an.h4; ['hoi_an.market_puzzle_solved'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| hoi_an:H6 | hoi_an.h6; ['hoi_an.lamp_restored'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Con Dấu Lời Chào đề xuất A |
| hoi_an:H7 | hoi_an.h7; ['hoi_an.lamp_restored'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| hoi_an:H17 | hoi_an.h17; ['hoi_an.ferry_available'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| hoi_an:H18 | Không definition | MARKER_ONLY | Chỉ Camera marker khi rule phù hợp | Không có postcard gameplay tại đây |
| hue:U4 | hue_u4_checkin; [] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| hue:U6 | hue_u6_checkin; [] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| jeonju:J8 | jeonju_j8_checkin; [] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| jeonju:J11 | jeonju_j11_checkin; [] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |
| da_lat:D0 | dalat_checkin_0; [] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Mảnh Kính Hồ đề xuất |
| da_lat:D14 | dalat_checkin_14; ['da_lat.beacon_lit'] | CHECKIN_EXISTING_RUNTIME | Map A ROI, GENERATED_POSTCARD_VIEW | View lại sau mark, không reward lại; Không tự cấp fragment |

## Một pilot Hội An sau approval

**Chọn H6 “Cầu cổ ánh đèn”, giữ tên/ID/vị trí gốc**. Không mặc định đổi thành tên Chùa Cầu nếu node/ảnh chưa được người dùng xác nhận đúng địa danh. Đã có CheckInDefinition `hoi_an.h6`, flag `checkin.hoi_an.h6`, yêu cầu H7 `hoi_an.lamp_restored`, thưởng2xu +1 `hoi_an_bridge_postcard`. Stage3; chỉ mở check-in khi đúng điều kiện cũ, **không thêm relic gate**.

Đề xuất flow 10–20s: người chơi tới H6 → chọn Khám phá góc nhìn → xem crop tranh canonical trong frame postcard → kéo khung căn một chiếc đèn và phản chiếu theo mẫu rõ → một ghi chú văn hóa ngắn lấy DesignKit/nguồn đã duyệt + reaction Lumi → lưu metadata journal. Có exit/retry, không mất quest/item. Không bắt camera OS, không xin quyền máy ảnh/thư viện.

Chỉ sau thao tác hợp lệ mới lưu photo mark mới và, nếu D04-A được duyệt, collection evidence Con Dấu Lời Chào. **Không cấp fragment khi chỉ mở preview/view**. Nếu người chơi đã mark H6 trong save cũ, vẫn có photo/recovery interaction nhưng không gọi grant2xu/postcard lần nữa. Nếu chưa mark, giữ reward ledger của check-in hiện hành; không thêm reward mới. Cần test cả trước/sau restored Đèn Mắt Phố; postcard view có thể phản ánh visual state metadata, không nói chụp được cảnh full light khi chưa tới.

Đây là nâng cấp presentation/interaction trên check-in thật, không manager/Canvas/camera mới. `GameplayPanels`/Journal hiện có là điểm mở rộng sau approval; source data giữ một registry. Dùng metadata: checkinId + viewVersion + lightState + completed gesture/evidence ledger. Ưu tiên flags/metadata nhẹ, không ghi nhiều ảnh bitmap trên Android. Chưa chốt save schema; transaction phải cùng collection/progress ở core an toàn.

## Năm city — đề xuất, không triển khai

| City | Node/landmark đã kiểm | Tương tác đề xuất | Clue/fragment | Status / rủi ro |
|---|---|---|---|---|
| Huế | U4 đầu cầu/U6 cổng cách điệu đã có check-in | Căn nhịp phản chiếu/triện trên view Map A | Đối chiếu dấu sông/nhịp, không tự thay ancient_seal | READY_TO_REUSE handler; gesture DESIGNED_ONLY; không đổi qua sông/gate |
| Đà Lạt | D0 bến hồ đã có check-in | Vuốt sương trang trí để thấy phản chiếu, chọn khung hồ | Mảnh Kính Hồ tại D0 nếu mapping duyệt | READY_TO_REUSE view; không thay fog-of-war/collider; gesture mới |
| Jeonju | J8 cầu đá/J11 mái ngói đã có check-in | Căn hoa văn mái hanok theo mẫu xưởng | Clue Hana/Yoon; không tự cấp mọi pattern hay fragment | READY_TO_REUSE; giữ parcel/quest ở J11 |
| Gyeongju | G4 đài cách điệu, G14 kho bản khắc, G17 đài lồng đèn | Ghi dấu landmark sau khi node được khám phá; không thay puzzle/encounter | Đã thêm 3 CheckInDefinition, chỉ cờ hoàn thành + coin | `concept_photo_pending`; ảnh landmark chưa tạo |
| Seoul | S2 cổng thành, S10 chân tháp, S18 relay Hanji | Ghi dấu landmark sau khi node được khám phá; S18 không tự bật relay | Đã thêm 3 CheckInDefinition, chỉ cờ hoàn thành + coin | `concept_photo_pending`; ảnh landmark chưa tạo |

Không áp số lượng cứng 1 hay2/city; mở rộng theo chất lượng và source đã có. Không thêm node/NPC. H18 Camera-only được ghi rõ, không tự thêm một quiz/photo quest. WorldMap postcard cards hiện có không phải album.

## Art và nghiệm thu cần làm sau duyệt

Map A/illustrated skin/camera/slot sẵn; pilot dùng ROI + overlay nên không cần PNG city mới. Vẫn cần kiểm ROI đúng landmark/không crop vật thể xấu/không phóng preview bé; art full postcard riêng nếu chất lượng crop chưa đủ thì NEEDS_ART. Không tái dùng JPG có credit bị loại; inventory postcards hiện là icon category khác với ảnh Map A trong popup.

Test tương lai: chưa mark/mark cũ/mới/exit/retry/replay, trước-sau relic, hidden/preview reject, touch/mouse/safe-area, quest/refund/reward ledger, save/restart/re-entry/post-boss, memory/IO. **Phase0 không chạy gesture/PhotoJournal vì chưa implement.**
