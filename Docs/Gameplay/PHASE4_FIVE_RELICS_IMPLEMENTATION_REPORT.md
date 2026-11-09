# Phase4 — five relics implementation

**WINDOWS_TARGETED_PASS · VISUAL_PENDING · PHASE5_PENDING · STOP sau Phase4.**18/10/2026. [Runtime review](../QA/PHASE4_RUNTIME_REVIEW.html) · [QA chi tiết](../QA/PHASE4_SIX_RELICS_E2E.md).

Prompt chính thức: [merged Phase4](../Prompts/LightBetweenCultures_Phase4_MERGED_Relics_Reactions_LinkKey_20261018.md), đọc toàn bộ từ attachment và lưu nguyên văn vì file project chưa có. Đã kiểm tra checkpoint18 check-in hoàn tất trước khi bắt đầu; base CheckpointHoiAnLighting-20261018 (manifest7fcbfb540f27597eeafa224af66d01da072cc370117b294ab722f1f78af47723). Small before-snapshot: BeforePhase4Relics-20261018, không full backup.

## Đã triển khai thực tế

| City / di vật | Nguồn | Điểm phục dựng | Trial thật | Saved collected / assembled / trial / restored | World Reaction |
|---|---|---|---|---|---|
| hoi_an · Đèn Mắt Phố | H0/H4/H10/H2 | H2 | Dẫn Ánh Đèn cũ; cần H7/HQ01 | 4/4 · true · true · true trong earned final | A/B/C;10 halos giữ nguyên |
| jeonju · Mộc Ấn Hanji | J2/J3/J7 + proof H6 | J2 | Xoay4 lớp hoa văn đúng mẫu | 4/4 · true · true · true trong earned final | A/C; đèn xưởng + dải giấy |
| hue · Ấn Rồng Ngọc Tỉ | U3/U8/U16/U17 | U7 | Rà4 nét + căn hướng mặt ấn | 4/4 · true · true · true trong earned final | A/C; điểm sáng hai bờ |
| da_lat · La Bàn Sương Lam | D0/D2/D8/D12 | D2 | Vuốt sương + tìm4 dấu + căn Đông Bắc | 4/4 · true · true · true trong earned final | A/C; tan lớp sương trang trí |
| gyeongju · Kính Ấn Tinh Tú | G3/G4/G7/G15 | G9 | Rà kính + đọc3 dấu, loại mồi + căn Tây | 4/4 · true · true · true trong earned final | A/C; sao di tích, G7 khác màu theo nhánh |
| seoul · Lăng Kính Tam Relay | S2/S7/S10/S18 | S7 | Kéo3 luồng + xoay hướng, hội tụ hình học | 4/4 · true · true · true trong earned final | A/C; tín hiệu3 relay dạng biểu tượng |

Một RelicCollection,6 relicId,24 fragmentId. Có nguồn proof thật, thao tác nhận độc lập, assembly kéo4 phần, trial sai/hint/retry/exit/resume và xác nhận thành công. Assembled không đồng nghĩa restored. Phần đặt/điều hướng chưa xác nhận của trial là session tạm; save chỉ giữ collection/assembly/kết quả, mở lại thử lại không mất mảnh. Prism dùng tuyến trong minigame thật, kiểm heading/endpoint/bounds/intersection; không chỉnh waypoint CityMap.

## Story, giao lưu và check-in

- Jeonju Hana J2: H6 Greeting Stamp thật/old mark có thể ghi nhận mới, không buộc giao ớt hoặc dâu và không cấp lại thưởng H6. J15 chỉ nhắc ớt khi jeonju.chili_shared thật.
- Huế Lê An U7/Bác Tâm và các điểm liên quan: trước/đang thu/4mảnh/assembled/restored. Nhắc văn khắc Gyeongju chỉ khi đã thật sự đọc (gyeongju.core_rhythm_known); không giữ mảnh U17 đến G14.
- Đà Lạt Lan D2: lời nhắc kính và hướng sương, D12 cấp nguồn không đợi Seoul. Gyeongju G9/G14/G7: nguồn tượng nhận được ở cả battle/repair, trạng thái màu/lời thoại khác theo lựa chọn cũ.
- Seoul S2/S7/S11/relay: hồ sơ, báo cáo và3 puzzle cũ làm proof; ba luồng văn hóa là biểu tượng, không gán nhầm3component. Sau boss/6di vật chỉ acknowledgement, không ceremony hoặc credits mới.
- Side narration được lưu trong CulturalRelics.json, append vào hội thoại gameplay thật; giữ29 quest/22 puzzle và nội dung quest gốc. NPC hiện diện/portrait binding cũ giữ nguyên;4 imported NPC vẫn PENDING_VISUAL_APPROVAL.
- **0 check-in/postcard mới**: tái dùng18 entry và18ảnh đã duyệt. Năm city có narration A/C trong caption và Photo Journal; caption dài cuộn riêng, không che nút. Album từ city khác đọc đúng relic của bưu thiếp, không dùng state city đang đứng. H6/Hội An caption/2symbol/ảnh căn khớp/ledger nguyên vẹn.

## World Reactions: một hay hai nấc

Hội An tiếp tục **A/B/C** từ mai-lantern và relic restored: H3 pickup chưa trả vẫnA; H2 returnedB; H7 cònB; valid Đèn Mắt PhốC. Năm city chỉ **A/C**: quest/puzzle/beacon/4mảnh/assembly chưa tạoC. Không dim toàn map40% ở năm city; canonical giữ nguyên. C là layer con map, non-raycast, chỉ gắn tại node REVEALED; fade1.8s lần đầu, reload/re-entry hiệnC ngay không celebration/reward lại. Đà Lạt chỉ bỏ sương trang trí mới, không đụng fog-of-war/collider/bridge/painted fog. Tất cả artwork/visual vẫn chờ người dùng nghiệm thu, không tự VISUAL_APPROVED.

## Chìa Kết Nối H5–H7

| Mốc | Hành vi thực tế / bằng chứng |
|---|---|
| ID / tên | link_key · Chìa Kết Nối; không tạo travel key khác |
| Trước H5 |0 key, Huế/Jeonju khóa; earned private save + restart |
| H5 battle / hack | Mỗi nhánh được đúng1; max-merge drop + encounter:hoi_an:enemy_mo01:link_key ledger cũ, không nhân đôi khi đổi lựa chọn |
| HQ01 / H7 | Existing consumeItems=true: dùng lamp_eye + hanji_core + link_key một lần khi xác nhận H7;150xu reward cũ một lần |
| Inventory trước H7 | Mô tả đích dùng H7, không nút Dùng giả; H5 không mở WorldMap |
| Sau H7 fresh | Key0; dialogue xác nhận đã dùng; Huế và Jeonju mở theo hoi_an.lamp_restored, Enter không đòi key |
| Old retained key | Fixture tương thích có1 key sau H7: giữ item, mô tả Đã sử dụng/kỷ vật; không thu lại hoặc tự cấp mới. Đây là fixture riêng, không phải earned reward |
| Old missing key | Earned hậu boss key0 được vào mọi city; không re-grant, không khóa lại |
| Reload |6 restart cases before-H5/after-H5/after-H7 cho cả hai nhánh + retained fixture PASS, save/coins/ledger exact qua đọc UI |
| City sau | U19→Đà Lạt; J19→Gyeongju;4beacon→Seoul. Source code/whole GameplayContent exact; earned E2E không key, không relic gate mới |

Không đổi backend, consume flag, quest condition, reward, key ID hoặc world unlock. Chỉ patch dialogue/mô tả Inventory đúng lifecycle thực tế.

## File code/data sửa hoặc thêm

- `Assets/_Game/Scripts/Content/RelicCollection.cs`
- `Assets/_Game/Scripts/Content/RelicActivity.Trials.cs`
- `Assets/_Game/Scripts/Content/NodeEventManager.cs`
- `Assets/_Game/Scripts/UI/GameplayPanels.cs`
- `Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs`
- `Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs`
- `Assets/_Game/Scripts/UI/GameplayPanels.CheckIns.cs`
- `Assets/_Game/Scripts/UI/RelicSurveySurface.cs`
- `Assets/_Game/Scripts/UI/InventoryPanel.cs`
- `Assets/_Game/Scripts/Maps/CityMap.RelicLight.cs`
- `Assets/_Game/Scripts/Maps/CityMap.CulturalRestoration.cs`
- `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs`
- `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Phase4.cs`
- `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Phase4Integration.cs`
- `Assets/_Game/Resources/Content/CulturalRelics.json`

Docs: access/source matrices, unlock decisions, progression design, FULL_STORY_PLAYTEST, báo cáo này, QA/runtime gallery, SIX_RELICS_ART_MANIFEST. Tools: phase4_checkpoint/validation/verify/report; new source .meta trong manifest. **Không sửa PNG nguồn/map, asset binding, canonical registry, scene/prefab, save schema hoặc quest graph.**

## QA và checkpoint

Hai hành trình fresh VN_FIRST/KOREA_FIRST đi thật H0→H3→H7→các beacon→24/24→6/6→boss repair/battle→backtrack sáu city PASS. Fresh core route không cố hoàn tất mọi side quest; hai save earned29quest lịch sử bổ sung QA hậu boss/old proof. Cả hai old saves nhận24mảnh bằng thao tác mới, giữ exact inventory/coins/29quests/choices/non-relic world history và old ledger +24claim, không replay boss/credits. RestartA/C mỗi city, hai nhánhG7, H5battle/hack và key restart PASS. 56 preservation checks exact bao gồm47protectedfiles + H6 functions/HoiAn data.

Windows build: Logs/phase4-final-build02.log Succeeded. Unity6000.6.5f1 Development Windows, không release/Android certification. Android build/device, FPS/VRAM profiling, mute/audio-device acceptance, v1 historical-file process test: **NOT_TESTED**; v1/v2 SaveSystem source không đổi. Synthetic touch là PC InputSystem, không Android touch device.

City checkpoints đã hoàn tất tuần tự Jeonju→Huế→Đà Lạt→Gyeongju→Seoul, sau đó final checkpoint nhỏ kế thừa Seoul. Tên/base/hash/file-count trong Phase4-20261018/last-checkpoint.json. Không copy Library/Temp/Builds/historical Validation; chỉ file thay đổi và evidence hiện hành. Thất bại QA được giữ, không sửa lịch sử thành PASS.

## Pending / mức độ

- **P1 / PHASE5_PENDING (ngoài phạm vi):** legacy manualS21 vẫn có thể phát credits theo flow cũ. Phase4 không tự phát credits/SQ03/global completion; chưa triển khai guard boss resolved AND6restored AND nghi thức Seoul. Old ending_seen vẫn là lịch sử, không nghi thức mới. Không gọi game hoàn tất chỉ vì6/6.
- **P2 ART_PENDING:**3Seoul icons đúng nghĩa: heritage_data, jiwon_sync, stellar_ring; slot trung tính có nhãn. Full prism/frame giữ art hiện có.
- **P2 NEEDS_CROP_REVIEW:** sliver trên3component Jeonju và relay frame Seoul. Không re-slice trái phạm vi.
- **P2 VISUAL_PENDING:** mức glow/fog/landmark,5trial presentation,4importedNPC cần chủ dự án nghiệm thu hình. Đặc biệt Curator/Inscription có nét mặt tương đồng: khác file/trang phục chưa xác nhận được hai danh tính từ ảnh; gallery có cả hai để đối chiếu. Không tự approve canonical hoặc tự thay mặt.
- **NEEDS_AUDIO:** chưa có bộSFX phục dựng riêng được kiểm duyệt; không tải/sinh audio không rõ giấy phép. Feedback visual hoạt động, không tự tuyên bố audio hoàn chỉnh.
- **Ngoài phạm vi giữ nguyên:**14node quá3nhánh và terrain NEEDS_USER_REVIEW từPhase3.5; không sửa graph/đường trong task này. Android/performance chưa kiểm nghiệm thiết bị.

**DỪNG sau Phase4.** Không Phase5/6/7, không tự chuyển task khác. Người dùng duyệt hình/runtime và cung cấp3icon Seoul trước các bước art final; ending chờ lệnh Phase5 riêng.
