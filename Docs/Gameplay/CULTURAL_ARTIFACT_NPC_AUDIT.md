# Cultural Artifact NPC audit — Phase 0

2026-10-18 · AUDIT/DESIGN ONLY. Không sửa registry, native meta, identity, sprite binding hay gameplay. Đối chiếu Resource AdventureUIArt, canonical registry, native-export/manifest/source atlas, các event runtime và ảnh thật. [Chi tiết từng variant: rect/GUID/localID/identity/placement](Phase0-20261018/npc-audit-records.json). [Trang ảnh nguồn mới](Phase0-20261018/SourceReview.html) không phải runtime proof.

## Số lượng có nghĩa

- **27 npcId keys** xuất hiện trong 37 event definitions. Đây không phải 27 người: S14 là thông tin bến phà, D18 là Lumi monologue; 25 vai có nhân dạng/động vật gồm Chó Canh Đường. Không tạo người lái phà/chủ trọ để lấp key.
- **21 binding**: 9 CANONICAL_IN_USE, 12 BOUND_NOT_YET_APPROVED (có chó). 4 vai thực còn thiếu portrait runtime: npc_curator S2, npc_engineer S11, npc_keeper G9, npc_inscription G14. Bốn PNG mới đã có, chưa import/bind/duyệt, không còn gọi là thiếu file nguồn.
- **0 CANONICAL_UNUSED identity** đã xác minh; Cô Mai có một approved variant native chưa bind và các approved concept references của Bà Tư/Phúc, không là người khác.
- Có **4 hình/vùng người chưa gán cần phân loại**, không phải bốn portrait đã runtime-ready: đầu bếp (một native slice), người gánh hàng (chưa tách riêng), lính Huế (chưa tách riêng), khách cầm máy ảnh (chưa xác minh độc lập hoàn toàn với identity đã dùng). Chỉ ba vùng đầu xác định chưa gán; khách là NOT_VERIFIED. Không tự tạo NPC/quest mới.

## Roster và binding hiện tại

| npcId / tên | City/node | Nhóm | Nguồn / Sprite / GUID:localID | Vai trò fragment/check-in đề xuất |
|---|---|---|---|---|
| npc_ba_tu / Bà Tư | hoi_an:H10 | CANONICAL_IN_USE | Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png<br>H3_07_Characters_NPC_Concept_23<br>ab62ba1e558f7274984500bb1cac18f6:8637574531497076435 | fragment_hoi_an_roof_hook |
| npc_boat / Bến phà thông tin | seoul:S14 | NOT_REQUIRED | Không binding; PNG candidate mới nếu thuộc 4 roles; S14/D18 không yêu cầu người | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_boatman / Bác Lái Đò | hoi_an:H17 | CANONICAL_IN_USE | Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_Boatman.png<br>NPC_HoiAn_Boatman<br>3835aa72bd8c88b41af32d4aabd40b98:21300000 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_curator / Quản thủ bảo tàng | seoul:S2 | MISSING_ART | Không binding; PNG candidate mới nếu thuộc 4 roles; S14/D18 không yêu cầu người | fragment_seoul_heritage_data |
| npc_elder / Người giữ chuyện | jeonju:J16 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Jeonju/npc_elder.png<br>npc_elder<br>9e2ba05a32d628045ae4dce1b7b1cb19:245854044 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_engineer / Kỹ sư Trạm Số | seoul:S11 | MISSING_ART | Không binding; PNG candidate mới nếu thuộc 4 roles; S14/D18 không yêu cầu người | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_gardener / Người Làm Vườn | da_lat:D13 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/DaLat/npc_gardener.png<br>npc_gardener<br>cac9b7f04788a1743af1858d523019df:1390191892 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_guest / Ji-eun | hoi_an:H18 | CANONICAL_IN_USE | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png<br>npc_guest<br>c6176fa957f7eb549b9b40703e5038ea:-404209332 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_hana / Nghệ nhân Hana | jeonju:J2 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png<br>npc_hana<br>c6176fa957f7eb549b9b40703e5038ea:-730319472 | fragment_jeonju_hanok_pattern |
| npc_hungry_dog / Chó Canh Đường | da_lat:D10 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/DaLat/npc_hungry_dog.png<br>npc_hungry_dog<br>3df301f30edc9d243899518bb89fb538:37410012 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_huong / Chị Hương | hue:U1 | CANONICAL_IN_USE | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png<br>npc_huong<br>c6176fa957f7eb549b9b40703e5038ea:-1343187348 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_innkeeper / npc_innkeeper | da_lat:D18 | NOT_REQUIRED | Không binding; PNG candidate mới nếu thuộc 4 roles; S14/D18 không yêu cầu người | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_inscription / Kho bản khắc | gyeongju:G14 | MISSING_ART | Không binding; PNG candidate mới nếu thuộc 4 roles; S14/D18 không yêu cầu người | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_keeper / Sân chính di tích | gyeongju:G9 | MISSING_ART | Không binding; PNG candidate mới nếu thuộc 4 roles; S14/D18 không yêu cầu người | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_lan / Người dẫn đường Lan | da_lat:D2 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/DaLat/npc_lan.png<br>npc_lan<br>fb6c63798ffdb6841a4fd318adf72a74:1886636321 | fragment_da_lat_pine_direction |
| npc_mai / Cô Mai | hoi_an:H2 | CANONICAL_IN_USE | Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_CoMai_Portrait.png<br>Mai_DialoguePortrait<br>a6cc838dd76ae7f40902d846693f4a5e:-1732270161 | fragment_hoi_an_eye_reflection |
| npc_market_merchant / Thương nhân đường xa | da_lat:D4, hoi_an:H4 | CANONICAL_IN_USE | Assets/_Game/Art/Characters/NPC/HoiAn/NPC_HoiAn_MarketMerchant.png<br>NPC_HoiAn_MarketMerchant<br>0a91b2dbca846994bbade4807ff62596:21300000 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_min / Hướng dẫn viên Min | gyeongju:G3 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png<br>npc_min<br>c6176fa957f7eb549b9b40703e5038ea:830274255 | fragment_gyeongju_min_record |
| npc_nguyet / Bà Nguyệt | hue:U2 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Hue/npc_nguyet.png<br>npc_nguyet<br>48a2e6c60f7ec0349befacea7c60e927:-1004916199 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_park / Thương nhân Park | jeonju:J5 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Jeonju/npc_park.png<br>npc_park<br>36a0c8ea46b133e40bf8731d313402d7:-606975524 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_phuc / Ông Phúc | hoi_an:H14 | CANONICAL_IN_USE | Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png<br>Phuc_DialoguePortrait<br>ab62ba1e558f7274984500bb1cac18f6:1019635214 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_scholar / Nho Sinh Lê An | hue:U7 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png<br>npc_scholar<br>c6176fa957f7eb549b9b40703e5038ea:-156234960 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_soyeon / So-yeon | jeonju:J15 | CANONICAL_IN_USE | Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png<br>npc_soyeon<br>c6176fa957f7eb549b9b40703e5038ea:464872565 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_tam / Bác Lái Đò Tâm | hue:U3 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Hue/npc_tam.png<br>npc_tam<br>7eb9ff722f5f81748945b06d9e1d05bd:-1705763202 | fragment_hue_river_jade |
| npc_tech / Ji-won | seoul:S7 | CANONICAL_IN_USE | Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png<br>JiWon_DialoguePortrait<br>ab62ba1e558f7274984500bb1cac18f6:1851553138 | fragment_seoul_jiwon_sync |
| npc_thuy / Cô Thủy chủ tiệm hoa | da_lat:D3 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/DaLat/npc_thuy.png<br>npc_thuy<br>33ce1e750bcd0124d9feb5cfcbf3fa66:-857154714 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |
| npc_yoon / Nhà thầy Yoon | jeonju:J4 | BOUND_NOT_YET_APPROVED | Assets/_Game/Art/Characters/NPC/Jeonju/npc_yoon.png<br>npc_yoon<br>9e80ff4e3ffd49044b5dabf92b3d6b27:-321799653 | Giữ vai trò/thoại cũ; có thể clue địa phương nếu user duyệt |

Tên registry “Sân chính di tích”/“Kho bản khắc” là nhãn placement cho npc_keeper/inscription, không tự đặt tên người mới. Tuổi/culture/traits cụ thể lấy canonicalIdentity/coreTraits trong records; unconfirmed chỉ là mô tả ảnh, không tự nâng thành canonical. Không coi trang phục khác là IDENTITY_CONFLICT. Hai reference Mai già tóc bạc trong registry vẫn DO_NOT_REUSE cho Mai trẻ; không thay hai mặt hiện có.

## Unused/art reuse có bằng chứng

| Hình | Nguồn thật | Native/binding | Kết luận / cơ hội |
|---|---|---|---|
| Đầu bếp | H3_07 top-right, `_14` | native localID 4210009647818869598, GUID ab62ba1e558f7274984500bb1cac18f6; chưa có NPC role binding | UNNAMED_ART_AVAILABLE; phù hợp bếp/lễ hội nếu một vai thật cần, không phù hợp mặc định 4 vai mới, không tạo chủ trọ D18 |
| Người gánh hàng trẻ | H3_07, bên phải Ông Phúc trong vùng `_21` | native combined slice không phải cutout độc lập | NEEDS_ART_REVIEW; có thể vai giao hàng nếu có nội dung đã duyệt, không thay thương nhân canonical |
| Lính Hoàng thành | Shared transparent atlas top row second | 6 native portrait named đều đã bind; nhân vật lính chưa có cutout riêng được xác minh | UNNAMED_ART_AVAILABLE (source figure), cultural role Huế; không đưa làm bảo vệ Hàn Quốc |
| Khách cầm máy ảnh | H3_07 dưới, bên phải Ji-won trong `_22` | Combined slice; nửa Ji-won đã canonical, không reuse cả vùng | NOT_VERIFIED identity/crop; có thể nhiếp ảnh gia sau đối chiếu mặt Min/Ji-eun, không chốt mới |
| Nghệ nhân kính nhỏ | Composite_Illustrated_v1/DesignKit | Có nền và nguồn khoảng100–140px theo audit trước | DESIGN_REFERENCE_ONLY; chưa phải portrait rõ, không upscale để coi final |

Native-export có 31 handles H3_07, 28 không có role runtime binding; Shared atlas có6 exported named sprites và cả6 bound. **28 handles không đồng nghĩa28 nhân vật chưa dùng**: gồm đạo cụ, nhiều lát/combined/variant của cùng face. Top-level manifest.bindings thiếu npc_tech trong khi native-art-bindings, per-image bindings, canonical registry và AdventureUIArt đều có: metadata view drift, không phải Ji-won mất runtime binding. Không chạy lại catalog/import để sửa ngoài scope; ghi để đồng bộ ở Phase1 nếu được phép.

## Bốn nguồn mới — chưa phê duyệt

| id/node | File đã kiểm | Nhận xét trực quan | Trạng thái |
|---|---|---|---|
| npc_engineer/S11 | NPC/Seoul/NPC_Seoul_Engineer.png | Nam trẻ, tóc nâu, tablet/cờ-lê, đủ đầu/tay/chân, không kính như brief | PENDING_USER_VISUAL_APPROVAL, chưa meta/GUID/localID |
| npc_curator/S2 | NPC/Seoul/NPC_Seoul_Curator.png | Nữ tóc bob nâu, hanbok/sổ; trẻ hơn brief trung niên tóc bạc | PENDING_USER_VISUAL_APPROVAL |
| npc_keeper/G9 | NPC/Gyeongju/NPC_Gyeongju_Keeper.png | Nam tóc/râu trắng, gậy/đèn; lớn tuổi hơn brief gốc 40–55 | PENDING_USER_VISUAL_APPROVAL, không là conflict với identity runtime chưa từng có ảnh |
| npc_inscription/G14 | NPC/Gyeongju/NPC_Gyeongju_Inscription.png | Nữ tóc bob, sách/cuộn khắc; nét mặt/kiểu tóc khá gần Curator; cần user kiểm độ phân biệt | NEEDS_ART_REVIEW / PENDING_USER_VISUAL_APPROVAL, chưa khẳng định cùng người |

Cả4 PNG 1086×1448 RGBA, alpha0–255, một người, không thấy nhãn/khung hay người cạnh; đây là quan sát nguồn, chưa kiểm import/mesh/blend trong game. Không tự chốt4 canonical. Bảo vệ Ji-won hanbok ở S7. Xem exact alphaBBox/hash/meta status trong new-art-source-audit.json; rect/GUID/localID của bốn ảnh mới **chưa tồn tại**, không bịa `21300000`.

## Shared traveler và lời thoại phụ đề xuất

- `npc_market_merchant` đã là **cùng identity** tại H4 và Đà Lạt D4, phù hợp truyền postcard/câu chuyện vật liệu. Không spawn hai người khác mặt. Actor/data hiện có làm chứng cứ shared traveler; thêm điểm đến khác vẫn cần story/placement approval.
- Ji-eun `npc_guest` hiện H18; có quan hệ Jeonju qua thư J16/J21/H18. Ưu tiên giữ tại H18, dùng thư/lời nhắn đối tác, không tự teleport. Nếu user muốn cameo Jeonju, dùng cùng npcId/face và điều kiện city đã mở + quest proof, outfit variant chỉ khi có art duyệt.
- Hana/Phúc có thể đối chiếu hoa văn/móc qua Lumi mang tin, không cần rời xưởng. Min có thể giải thích dấu Huế trong thoại sau bằng chứng U17; chưa có placement Huế nên chỉ DESIGNED_ONLY, không sửa face/age hay tự cấp NPC du hành.
- Bà Tư/So-yeon/Hương hỗ trợ clue sau giao đặc sản có thật. HQ05/HQ06/HQ07/DQ05 không tiêu món lần hai và không tự là fragment gate. Chó D10 chỉ giữ role động vật, không cho nói như nghệ nhân hay cấp fragment người.

Toàn24 fragment mapping đọc từ ACCESS_MATRIX; một NPC có thể cho nhiều clue, không cần24 NPC. H0/G7 cần source interaction mới chứ không gán người giả; G9/G14/S2 cần art approval nhưng có dialogue role thật. Mọi traveler/clue mới vẫn DESIGNED_ONLY. Dừng tại audit, không sửa registry/binding/quest/save.
