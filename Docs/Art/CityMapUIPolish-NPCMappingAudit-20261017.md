# Mapping NPC — audit của nhiệm vụ UI Polish

Đối chiếu trực tiếp source mới nhất, registry `Assets/_Game/Resources/AdventureUIArt.asset`, manifest `native-art-bindings.json`, `CrossCitySpriteSlices.json`, roster gốc và checkpoint BeforeCityMapUIPolish-20261017. **Không thay một binding, sprite hoặc ảnh nhân vật nào trong nhiệm vụ này.**

| NPC ID / địa điểm | Sprite runtime | Vai trò được ghi trên roster concept | Kết luận |
|---|---|---|---|
| npc_huong — Chị Hương, Huế U1 | NPC_TransparentAtlas.png; native sprite npc_huong; GUID c6176fa957f7eb549b9b40703e5038ea; localID -1343187348 | Cô chủ tiệm hoa/cà phê — Đà Lạt; áo xanh, mũ hoa, giỏ hoa | Casting **tạm có chủ ý**, được cho phép ở lượt giao đặc sản; không phải canonical Hương. Giữ nguyên chờ người dùng duyệt. |
| npc_soyeon — So-yeon, Jeonju J15 | Cùng atlas, native sprite npc_soyeon; localID 464872565 | Cô gái công nghệ/bảo tàng — Seoul; kính, tablet, trang phục xanh | Casting **tạm có chủ ý**, hai khuôn mặt khác nhau; không phải canonical So-yeon. Giữ nguyên chờ người dùng duyệt. |
| npc_thuy — Thủy, Đà Lạt D3 | Characters/NPC/DaLat/npc_thuy.png; GUID 33ce1e750bcd0124d9feb5cfcbf3fa66; localID -857154714 | Sprite dẫn xuất đề xuất riêng ở checkpoint Đà Lạt | Không dùng lại khuôn mặt Hương. Trạng thái AI_DERIVED_PROPOSAL_NOT_CANONICAL_NOT_FINAL_STATIC_SPRITE vẫn được giữ. |
| Kỹ thuật viên Ji-won — Seoul S7 | Chưa có binding portrait runtime riêng trong registry | Concept cô gái công nghệ đang dùng tạm cho So-yeon | Seoul còn DATA_ONLY. Chưa có xung đột hai NPC đang chạy, nhưng **không được tự gán lại sprite So-yeon cho Ji-won** khi triển khai Seoul. |

Nguồn được mở và xem trực tiếp: `LBC_AnhMoi_BanGiaoCodex_20261015/Characters/NPC_Roster_Concept.png`. Atlas và metadata sprite hiện hữu được giữ nguyên; không đưa toàn bảng concept vào game.

Không tìm thấy binding canonical riêng đã duyệt cho Hương hoặc So-yeon trong registry/catalog hiện tại. Các sheet/roster có nhân vật khác đã gán danh tính và vai trò khác; việc có một hình tương đối phù hợp không có nghĩa được quyền tráo danh tính. Không tạo hoặc tự chọn hình thay thế trong nhiệm vụ audit này.

Cô Mai và Ji-eun giữ nguyên GUID/localID, ảnh, crop và binding. Cùng thương nhân xuất hiện ở nhiều thành phố dùng cùng ID, có giải thích tái xuất trong nội dung; đó là tái sử dụng một nhân vật, không phải gán cùng khuôn mặt cho một NPC mới.

Việc thay hình sau khi được duyệt chỉ cần đổi reference theo npcId ở registry hiện có; không cần sửa quest/event. Mục portrait bị thiếu và listener mờ thuộc nhiệm vụ số 2, chưa tự triển khai ở nhiệm vụ số 1.
