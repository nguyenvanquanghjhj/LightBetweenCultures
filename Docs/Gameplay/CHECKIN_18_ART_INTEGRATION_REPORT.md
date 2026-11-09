# Check-in art integration & Photo Journal ·18/10/2026

**WINDOWS_TECHNICAL_PASS · NEEDS_VISUAL_REVIEW · ANDROID_NOT_TESTED.** Đã tích hợp18 ảnh được chọn, xử lý mép riêng từng ảnh và mở rộng Collection hiện có thành sổ ảnh sáu thành phố. Không nâng cấp Phase4 relics/Phase5 ending hoặc thay gameplay chính.

[Runtime review](CheckInArt-20261018/RuntimeReview.html) · [18 ảnh native/thumbnail trước–sau](../../Validation/CheckInArt-20261018/DerivedReview/index.html) · [Mapping/crop/GUID đủ18](../Art/CHECKIN_LANDMARK_IMAGE_MAPPING.md) · [QA](../QA/CHECKIN_18_RUNTIME_QA.md) · [File/checksum](CheckInArt-20261018/changed-files.json).

## Nguồn, trạng thái đầu vào và những gì được giữ

Đã đọc prompt chính đính kèm d5332fcb và bổ sung chỉnh mép ảnh mới nhất, source/AGENTS/checkpoint Phase3.5, check-in audit và FULL_STORY_PLAYTEST. Không chạy lại Phase0, dọn thư viện hoặc thiết kế tuyến đường. Snapshot tăng dần trước sửa `BeforeCheckInArt-20261018` cần base `CheckpointMapRoute35-20261018`; chỉ gồm24 file trực tiếp liên quan. Kiểm tra tiến trình thấy current Codex executable và Unity Hub, không task Editor/Player khác đang chỉnh source.

Trước task đã có **18 CheckInDefinition/handler/panel/flag/reward**, không thêm ID hoặc điểm check-in. H6 đã có gesture hai biểu tượng và album riêng. Chưa có album chung sáu thành phố; đa số view lấy ROI Map A. Đây là kiểm kê source: không gọi cả18 đã được runtime test trước task. Save hậu boss thật dùng làm đầu vào đã có7 mark cũ. [Audit trước sửa](CheckInArt-20261018/audit-before.md) giữ riêng để không sửa lịch sử nghiệm thu.

GameplayContent, AdventurePresentation, NodeEventManager, RelicCollection và SaveSystem giữ nguyên byte. 133 node,155 route/geometry hiện hành,29 quest,22 puzzle,6 relic/24 fragment, beacon/WorldMap unlock, boss/ending history và NPC identities/bindings được bảo toàn. Không sửa tranh Overview A, Lumi, actor scale, collider hoặc BGM. H6 gesture body, UV căn khớp, hai biểu tượng/hotspot/proof/reward không đổi. Chi tiết guard tại [verification.json](CheckInArt-20261018/verification.json).

##18 PNG và18 bản runtime derived

Nguồn duy nhất: `_CheckIn_Import_20261018/images`, đối chiếu `manifest.csv`. Đủ18 filename/city/node duy nhất; kích thước đúng450×722/726/756, RGB, không file hỏng hoặc blank. Đã xem trực quan18 ảnh, sau đó xem dải mép phóng riêng để xác định phần dính ảnh bên cạnh và separator. Contact sheet chỉ dùng QA, không được bind vào runtime.

- H3: bỏ30px trái gồm mảnh ảnh bên cạnh + separator,8px phải. H17: bỏ21px trái. Các ảnh khác loại đúng dải trắng/antialias khác nhau; không crop hàng loạt cùng số pixel.
- H2/H6 không crop bố cục. Các dải trắng phía trên một số ảnh Huế/Đà Lạt/Jeonju/Gyeongju/Seoul được loại đúng15–16px hoặc6–7px; không cắt phần tranh. Bảng đầy đủ ghi cả L/R/T và kích thước trước/sau.
- Unsharp Mask radius0.55, percent25, threshold3 chỉ vùng phong cảnh. Không chỉnh saturation/contrast toàn ảnh, không AI, không resize/upscale. Chân thẻ có tên được giữ trong cả nguồn và derived; UI dùng UV riêng bỏ phần chân chữ để không lặp tiêu đề. Thumbnail trong ảnh QA được downsample bằng Lanczos.
- Unity path: `Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/`. Folder copy ban đầu của cùng task được đổi tên bằng `AssetDatabase.MoveAsset`;18 GUID/localID giữ nguyên, không giữ hai bộ sprite runtime trùng nhau. Nguồn18 PNG gốc không bị ghi đè.
- Sprite Single/FullRect, sRGB, Bilinear, Clamp, no mipmaps/no Crunch, PPU100; Default max1024 và Uncompressed để giữ nét Windows. Android override max1024/ASTC6×6/high quality; **chưa build/test Android**. Không cấp texture lớn hơn nguồn để giả chi tiết.

**LIMITED_SOURCE_RESOLUTION:** README gói nêu các crop collage đã upscale/sharpen trước bàn giao. Derived vẫn chỉ412–450px ngang, không tạo chi tiết thật mới. Ảnh lớn trên540×960 thường hiển thị khoảng360–400px ngang, giữ aspect và letterbox; không kéo giãn fullscreen ngang. Cần người dùng duyệt crop/sắc nét/bố cục; không tự ghi FINAL_ART_APPROVED.

## Luồng chơi và sổ ảnh

`GameplayPanels` hiện có được mở rộng bằng partial `.CheckIns.cs`, không manager/Canvas/EventSystem/SaveSystem thứ hai. `AdventureUIArt.checkIns` chỉ là mapping ảnh theo CheckInId đã tồn tại, không phải nguồn trạng thái gameplay mới.

Luồng: tới node đã mở → chọn camera/check-in từ tương tác hiện có → xem tranh → Ghi dấu hoặc Để sau. Trước đây lựa chọn check-in tại một số NPC có thể record ngay trước khi xem; nay mở scenery trước và chỉ gọi transaction cũ sau nút xác nhận. H6 vẫn chuyển vào bài căn hai biểu tượng như trước. Không thêm puzzle, NPC, quest hoặc tiêu hao.

Camera là lựa chọn phụ trong menu event có paging hiện hành. H2 vẫn ưu tiên quest Cô Mai; H3 vẫn có Vào khám phá; H7 vẫn có beacon; G14/S2 vẫn có NPC; S18 vẫn có puzzle rơle. Marker resolver chỉ bổ sung camera fallback khi marker đang Normal và check-in khả dụng, không đổi marker chính của NPC/boss/quest/relay hoặc lộ HIDDEN.

Sổ ảnh mở từ **Nhật ký → Di vật/Bưu thiếp → Sổ ảnh văn hóa**. Sáu tab theo city, lưới hai cột có scroll;6/2/2/2/3/3 entry đúng18. Card chưa hoàn thành không mở ảnh lớn hoặc nhận thưởng. Node chưa khám phá chỉ có placeholder “Chưa khám phá”, không tên địa điểm/ảnh bí mật. Card hoàn thành lấy flag cũ để xem lớn và quay lại album; không yêu cầu đứng ở node. Không lưu bitmap vào save, không xin camera/thư viện ảnh, không phát BGM mới.

### H6: tách gallery khỏi ảnh tương tác

Theo prompt ban đầu H6 mới từng được để candidate; **bổ sung mới nhất cho phép thay artwork gallery nếu bảo vệ căn khớp**, nên final gallery/thumbnail H6 dùng derived từ `04_hoi_an_H6_cau_co_anh_den.png`. Gallery không dùng Map A hoặc ảnh từ thư mục khác.

Ảnh Map A ở board căn khớp và màn vào hoạt động H6 vẫn giữ đúng bản đã PASS theo ngoại lệ bảo vệ ảnh tương tác. Hai target, hai symbol, DragPiece, đúng/sai, proof và reward giữ nguyên. Gallery có metadata `photo:stored` khi đã có proof chụp; save cũ chỉ có completionFlag vẫn mở postcard mà không bịa thêm photo proof hoặc nhận thưởng lại. Không đổi ba trạng thái ánh sáng Hội An.

## Kết quả trả lời nghiệm thu

| Nội dung | Kết quả |
|---|---|
| Đọc nguồn/manifest | Đủ18 PNG, đúng filename/dimensions và18 CheckInId; nguồn hash không đổi. |
| Import/bind |18 texture Single,18 mapping native GUID/localID,18 gallery dùng đúng derived. |
| Existing art | Giữ mọi canonical art/NPC/Map A; H6 interactive art/proof/hotspot giữ nguyên. |
| Có gameplay trước task |18 có handler/view/record transaction từ source; H6 có gesture riêng; không tự gọi18 trước đây đã retest. |
| Hoàn thiện lần này |18 presentation/scenery + album toàn game; observe/confirm dùng cơ chế cũ cho17, H6 giữ gesture. |
| Data-only/missing view | Không còn trong18 check-in được yêu cầu. H18 camera-only ngoài18 vẫn không được tự biến thành check-in. |
| Journal |18 entry logic; thumbnail/ảnh lớn đúng, lock/hidden đúng; xem ngoài node và process restart PASS. |
| Primary interactions | Quest/H3/beacon/NPC options được giữ; S18 ảnh không bật relay, sau đó puzzle thật vẫn hoạt động. |
| Reward |7 mark cũ giữ;11 mark mới do xác nhận thật, +28 xu đúng dữ liệu; xem lại/cancel không cấp lại. H6 giữ2 xu +postcard như cũ. |
| Save cũ | Post-boss và H6 completed trước photo metadata vẫn xem được;6 restart độc lập, ledger/quests/items/relics/ending history không reset. |
| Visual/mobile | Windows portrait540×960, mouse +InputSystem touch simulation; không méo, chân chữ không lặp. LIMITED_SOURCE_RESOLUTION và NEEDS_VISUAL_REVIEW. |
| Chưa test | Android device/build, touch/pinch vật lý, FPS/VRAM, v1 standalone, Không chạy lại133 node. |

## QA, file và checkpoint

Main Journey02:432 PASS, đi World Map và Lumi tới18 node thật từ save đã kiếm được, không fixture flag/teleport;11 xác nhận mới. Sáu restart với derived:138 PASS; hai restart caption cuối Hội An/Seoul:61 PASS; Fresh locked album22 PASS; S18 pre-relay31 PASS; Fresh Hội An/H3/di vật/H6 200 PASS; album xuyên city100 PASS và restart trạng thái di vật/H6 mới13 PASS. Tổng997 assertions có phạm vi khác nhau, không là997 kiểm tra art. [QA chi tiết và các build](../QA/CHECKIN_18_RUNTIME_QA.md).

Production sửa `GameplayPanels.cs`, `.Relics.cs`, partial `.CheckIns.cs`, `AdventureUIArt.cs/.asset`, `NodeMarkerResolver.cs`; thêm editor import và đăng ký export trong `ArtLibrarySetup`. QA mở rộng runner hiện có và điều chỉnh bước confirm/scroll; không thêm scene.18 PNG derived +meta, catalog và báo cáo có trong [changed-files](CheckInArt-20261018/changed-files.json).

Checkpoint cuối `D:\GAMEKKK\Backups\CheckpointCheckInArt-20261018`, **INCREMENTAL_REQUIRES_BASE** `CheckpointMapRoute35-20261018` và chuỗi base của nó. Không full backup/cache/build copy hoặc xóa lịch sử. Cần duyệt thị giác18 crop và UI Unity; technical Windows đã có bằng chứng chạy thật. **DỪNG**, không Phase4–7, không nhiệm vụ khác.
