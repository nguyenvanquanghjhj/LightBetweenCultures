# Kết quả diễn tập source — 20/11/2026

Source đã được clone trực tiếp từ GitHub vào `D:\GAMEKKK\_LBC_WORK\RestoreTests\GitHubClone-20261120\LightBetweenCultures`; không copy Library/object Git từ project gốc. Clone dùng shallow history cho bài kiểm tra dữ liệu hiện hành; full history được giữ trên main và trong bundle. Mốc đã thử: `dd2c38ca4fdf2e483f171da533cb07f222640c0f`. Những bổ sung tiếp theo của bản lưu trữ là tài liệu/công cụ; không thay Assets/Packages/ProjectSettings đã thử.

| Hạng mục | Kết quả |
|---|---|
| Clone và Git object | Clone thành công 1.673 file ở mốc thử; `git fsck --full` đạt trên bản gốc và clone |
| Nội dung bắt buộc | 879 file Assets/Packages/ProjectSettings khớp Git blob trước khi mở Editor; 146 file binary đối chiếu SHA-256 từng byte với bản gốc |
| Asset/meta | 384 asset, 463 `.meta` trong tập bắt buộc; 192 C#, 6 scene, 4 prefab, 135 PNG; không thiếu/orphan meta hoặc trùng GUID trong kiểm kê |
| Unity import/C# | Unity 6000.6.5f1 import project sạch, tạo cả Assembly-CSharp và Assembly-CSharp-Editor; audit hoàn chỉnh thoát 0 |
| Native art/catalog | 34 assertions PASS; GUID/sprite localID/slicing được giữ |
| Route | 6 thành phố, 2.949 cặp node, 5.901 assertions PASS |
| Save | 4 native fixture assertions; không dùng save cá nhân |
| Catventure domain | 518 + 2 assertions PASS trong .NET adapter, không phải Unity Player |
| So baseline native | Không mất runtime GUID, không đổi sprite/Standalone/Android importer được đối chiếu; 0 lỗi reference mới. Vẫn giữ 7 GUID và 6 object reference lỗi template baseline; 0 missing script, 0 dynamic resource missing |
| Windows build mới từ clone | Build Development Player thành công, Unity thoát 0. Không ghi đè bản nộp |
| Chạy Windows Player | BLOCKED: Windows Application Control chặn EXE. Không đổi chính sách Windows, không nhận Player/gameplay PASS |
| APK bàn giao ArtFix 11/11 | Package/signature/alignment 16 KB PASS; com.DefaultCompany.LightBetweenCultures, 1.1.3/code 23; SHA-256 `44bd221dd45e49b42edc37d4a3efede8189695bbbbd91f04c6b865c8481caf0e` |
| Thiết bị Android, human acceptance | NOT_TESTED |

Unity tự xóa danh sách cache serialized `m_RuntimeSettings.m_List` trong `UniversalRenderPipelineGlobalSettings.asset` ở clone khi import. GUID, asset source gốc và gameplay không bị sửa để che khác biệt này. Bản gốc vẫn giữ file ban đầu. Một số Burst DLL ở clone bị chính sách Windows chặn; audit và build vẫn hoàn tất.

Lần gọi audit đầu import/compile và kiểm tra asset/save xong nhưng thiếu tham số `--route-refactor-output`, nên thoát 1; log được giữ. Lần gọi đúng có output riêng `native-clone-verified`, route và catalog đều đạt, thoát 0. Không sửa log lần lỗi thành PASS.

Lỗi Editor journey MiniMapRoads/CanvasRenderer trong hồ sơ 13/11 chưa được sửa hoặc chứng nhận hết. Nhiệm vụ cuối yêu cầu đóng gói, không nâng cấp gameplay; kết quả compile/build không thay thế nghiệm thu toàn campaign.

`Tools/Import/art_asset_catalog.py --inventory-only` đã được gọi sau native export trên clone; lần đầu dừng vì chưa giải nén nguồn provenance private ở cạnh clone. Đây là phụ thuộc của công cụ authoring lịch sử, không phải asset game thiếu khi compile/build. Khi khôi phục cả kho archive cạnh project, chạy lại và giữ registry NPC đã duyệt theo exact reference.

Bằng chứng JSON ở `Docs/Development/Storage/FinalArchive-20261120`. Log đầy đủ, kiểm tra download/giải nén, manifest SHA-256, commit/tag cuối và báo cáo dọn máy được đính kèm Releases sau khi upload và kiểm tra hoàn tất. Chưa có file hoặc phần mềm nào được phê duyệt xóa/gỡ trong lần diễn tập này.
