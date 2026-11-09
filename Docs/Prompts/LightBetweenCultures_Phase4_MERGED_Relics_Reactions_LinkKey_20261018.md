LIGHT BETWEEN CULTURES — MASTER EXECUTION PHASE 4
FIVE REMAINING CULTURAL RELICS · 20 FRAGMENTS · FIVE REAL MINIGAMES · CROSS-CITY STORY · RESTORATION EFFECTS · LINK_KEY & WORLD MAP UX
PROJECT: D:\GAMEKKK\LightBetweenCultures
NHIỆM VỤ: TRIỂN KHAI THẬT PHASE 4, KHÔNG PHẢI AUDIT-ONLY.
Đây là lệnh tiếp tục chính thức sau Phase 0 và Hội An Relic Pilot Phase 1–3.
Thực hiện sau khi nhiệm vụ chỉnh ánh sáng Hội An, Phase 3.5 về tuyến đường và phiên tích hợp 18 ảnh check-in/Photo Journal (nếu vẫn đang chạy) đã hoàn tất hoặc đạt checkpoint an toàn. Nếu phiên nào đang chỉnh file chung thì chờ, không ghi đè code/asset mới, không rollback 18 check-in hoặc thành tựu album 18/18 nếu đã tích hợp. Kiểm source/checkpoint mới nhất; đừng giả định tính năng vừa giao đã PASS.
CHỐT MỚI NHẤT: chỉ Hội An có ba trạng thái A/B/C (hai lần hồi phục thị giác). Huế, Đà Lạt, Jeonju, Gyeongju, Seoul có hai trạng thái A/C (chưa restored → restored, chỉ MỘT lần phục hồi lớn). Không thêm 5 vật phẩm nhỏ/quest/minigame mới để tạo trạng thái B. Đồng thời kiểm toán và chỉnh UX vật phẩm link_key/Chìa Kết Nối H5→H7 để nó không là vật phẩm vô nghĩa sau khi mở map; KHÔNG sửa điều kiện mở city hay tạo key gate mới. Chi tiết tại mục II-bổ sung và VII-bổ sung.
Tôi muốn bạn hoàn thành hệ di vật tại NĂM thành phố:
1. Jeonju
2. Huế
3. Đà Lạt
4. Gyeongju
5. Seoul
Đây là THỨ TỰ THI CÔNG, không thay đổi thứ tự mở thành phố của người chơi.
Không dừng sau mỗi thành phố để xin tôi cho phép tiếp tục nếu không có blocker nghiêm trọng. Kiểm thử và ghi checkpoint rồi tiếp tục thành phố tiếp theo.
Chỉ dừng sớm nếu phát hiện nguy cơ làm hỏng save cũ, thay đổi progression chính, mất asset canonical hoặc vấn đề thiết kế bắt buộc phải xin quyết định mới.
I. ĐỌC HIỆN TRẠNG TRƯỚC KHI SỬA
Đọc các tài liệu mới nhất thực sự đang tồn tại trong project, ưu tiên:
- AGENTS.md
- Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md
- Docs/Gameplay/ARTIFACT_FRAGMENT_ACCESS_MATRIX.md
- Docs/Gameplay/ARTIFACT_UNLOCK_DECISIONS.md
- Docs/Gameplay/ARTIFACT_FRAGMENT_SOURCE_MATRIX.md
- Docs/Gameplay/SIX_ARTIFACT_PROGRESSION_DESIGN.md
- Docs/Gameplay/SIX_RELICS_SOURCE_MAPPING.md
- Docs/Gameplay/CULTURAL_ARTIFACT_NPC_AUDIT.md
- Docs/Gameplay/CHECKIN_LANDMARK_AUDIT.md
- Docs/Gameplay/CULTURAL_ARTIFACTS_INTEGRATION_REPORT.md
- Docs/Story/ONE_ENDING_SIX_RELICS_CANONICAL.md
- Docs/Story/FULL_STORY_PLAYTEST.md
- Docs/QA/FINAL_ENDING_AND_POST_BOSS_AUDIT.md
- Docs/QA/HOI_AN_VISUAL_AND_POSTCARD_REVISION.md nếu đã được tạo
- Docs/QA/MAP_ROUTE_TERRAIN_REDESIGN.md nếu Phase 3.5 đã hoàn thành
- Docs/QA/RelicPilot-20261018/qa-results.json hoặc đường dẫn QA tương ứng thực tế
Nếu tên hoặc vị trí báo cáo đã thay đổi, tìm đúng tài liệu hiện hành; không tạo một bộ tài liệu thứ hai chỉ vì không thấy đường dẫn gợi ý.
Xác minh production state thật trước khi sửa:
- Unity version, scene đang dùng, content JSON.
- Core Six Cultural Relics của Phase 3.
- Collection / relic ledger / assembly / save.
- Các fragment IDs đã có.
- Minigame Dẫn Ánh Đèn.
- Art native sprites và bindings đã import.
- Các NPC mới và identity canonical.
- Latest Windows build.
- Các thay đổi địa hình/đường đi Phase 3.5 nếu đã có.
KHÔNG sao chép cấu trúc dựa trên báo cáo cũ khi source hiện hành đã đổi.
Baseline cần bảo toàn
- 6 thành phố.
- 133 node canonical.
- 29 quest.
- 22 story puzzles.
- Hội An Relic Pilot đã PASS về chức năng trên Windows.
- World Reactions V2 tại H2/H3/H7.
- Ba trạng thái ánh sáng Hội An nếu bản chỉnh mới đã áp dụng.
- Bốn fragment Hội An đã triển khai.
- Dẫn Ánh Đèn, collection, save, album và H6 postcard.
- World Map unlock như cũ.
- Lối Chùa Cầu và cơ chế che khuất đang đúng.
- Các tuyến đã sửa trong Phase 3.5.
- Các portrait canonical và bốn NPC mới đang chờ phê duyệt ngoại hình.
Không làm lại Hội An Relic Pilot để tạo thành tích mới.
II. QUY TẮC CANONICAL KHÔNG ĐƯỢC PHÁ
1. Chỉ có MỘT kết thúc game
Luật cuối cùng đã được chủ dự án quyết định:
finalBossResolved && allSixRelicsRestored && finalCeremonyConfirmed
Trong đó:
- Thắng hoặc sửa thành công boss Seoul đều là kết quả giải quyết boss hợp lệ.
- Phải phục dựng đủ sáu di vật.
- Người chơi phải chủ động thực hiện nghi thức kết nối cuối tại Seoul.
- Credits chỉ thuộc một ending duy nhất.
Nhưng Phase 4 CHƯA triển khai nghi thức ending. Việc đó thuộc Phase 5.
Ở Phase 4:
- Cần tạo đủ sáu restored relic states để Phase 5 có thể đọc chính xác.
- Có thể tạo thông báo 6/6 restored và mạng kết nối đang sẵn sàng.
- KHÔNG tự phát credits, ceremony hoặc global completion từ sự kiện đạt 6/6.
- Không tạo normal ending, secret ending, true ending hoặc bad ending.
Legacy S21 credits chưa được cập nhật đầy đủ phải được ghi là PHASE5_PENDING, không báo đã đáp ứng luật ending mới.
2. Mở thành phố độc lập với di vật
Giữ nguyên:
- Hội An H7 → mở Huế và Jeonju.
- Huế U19 → mở Đà Lạt.
- Jeonju J19 → mở Gyeongju.
- Bốn beacon Huế, Đà Lạt, Jeonju, Gyeongju → mở Seoul.
Không yêu cầu relic để:
- Mở city.
- Vào city.
- Hoàn thành quest chính.
- Giải puzzle cũ.
- Bật beacon.
- Bật relay.
- Đánh hoặc sửa boss cuối.
Người chơi có quyền thắng boss khi mới có 0–5/6 relic và tiếp tục khám phá sau đó.
2A. CHÌA KẾT NỐI (link_key) — có tác dụng trong truyện, không tạo gate mới
Phát hiện từ kịch bản playtest lịch sử, CHƯA coi là xác minh runtime hiện tại: H5 Bot Mờ-01 có hai cách giải hợp lệ (thắng combat hoặc dùng mã H19 vô hiệu hóa), đều nhận duy nhất link_key (hiển thị từng được gọi “Chìa Kết Nối”). HQ01 có chuỗi find:link_key@H5 → activate:H7. Mốc thật mở Huế + Jeonju là hoi_an.lamp_restored sau H7, không phải H5/boss hay việc link_key đơn thuần nằm trong Inventory. Người dùng thấy sau khi World Map mở, chìa vẫn nằm trong túi như vật phẩm vô dụng. Đây là vấn đề cần audit + sửa UX/mối liên hệ câu chuyện, không cho phép tự diễn giải thành thiết kế vật phẩm khóa các thành phố.
AUDIT BẮT BUỘC TRƯỚC KHI SỬA:
1. Tìm link_key thực tế trong GameplayContent.json, item definitions, H5 event/combat/hack, HQ01 quest requirements, H7 activation, Inventory UI, reward ledger, WorldMap IsCityUnlocked, SaveSystem và old-save compatibility. Xác định item ID, display name, possession/consumption/grant policy và H7 hiện có check gì. Nếu tồn tại vật phẩm tên “Chìa khóa thông hành” khác link_key, không tự hợp nhất ID: lập bảng item ID, nguồn, proof, tác dụng, inventory, tên hiển thị; chỉ chỉnh món liên quan H5→H7 sau khi xác thực.
2. Kiểm H5 combat và hack đều grant đúng một lần. H5 resolved nhưng H7 chưa kích hoạt thì Huế/Jeonju chưa mở. H7 completed mới mở theo rule cũ. Những beacon khác không đòi link_key, không đòi lại key.
3. Kiểm trace/runtime hoặc source để biết H7 thật sự “dùng” chìa làm proof, hay HQ01 chỉ kiểm đã nhặt. Nếu key đã là quest proof, giữ nguyên; không tạo thêm điều kiện phụ.
PATCH NHỎ, CÓ THỂ THỰC HIỆN NẾU SOURCE CHỨNG MINH AN TOÀN:
- Khi người chơi ở H7 có link_key/quest proof hợp lệ, lời thoại hoặc hiệu ứng chỉ rõ Lumi dùng Chìa Kết Nối để đồng bộ/kích hoạt trạm. Sau H7, Inventory (nếu vẫn hiện món đó) có mô tả hoặc trạng thái rõ ràng: “Đã sử dụng để đồng bộ trạm Hội An tại H7 — kỷ vật hành trình”, hoặc đưa vào tab Kỷ vật/Quest history chỉ nếu tab đó đã tồn tại. Không thêm một tab/hệ thống Inventory mới cho một món item.
- Ưu tiên giữ item/counter và flag gốc để không phá HQ01/old saves; “đã dùng” được derive từ hoi_an.lamp_restored/quest completion thật, không cấp lại hay tiêu lại item. Nếu bộ UI có slot vật phẩm nhiệm vụ chưa hoàn tất, có thể ẩn khỏi nhóm đang cần dùng sau khi completed, nhưng vẫn có lịch sử/mô tả cho người chơi.
- Nếu muốn tiêu link_key khỏi Inventory thì KHÔNG tự làm khi có nguy cơ mất proof/reward/save. Báo NEEDS_USER_DECISION cùng phân tích. Không tự biến chìa thành chìa mở Huế/Jeonju/Seoul hay khóa portal, không tự thêm item dùng ở mỗi city, không auto-give key cho old save.
- Nếu key vốn đã được xử lý đúng và chỉ còn là vật phẩm nhiệm vụ mang tính kỷ niệm, chỉ chỉnh text/presentation thật tối thiểu; nếu không có bug thì giữ nguyên.
TIÊU CHUẨN UX: Người chơi hiểu được (a) lấy chìa sau H5; (b) dùng nó trong chuỗi nhiệm vụ trạm H7; (c) World Map mở bằng H7/beacon; (d) chìa đã hoàn thành vai trò, không còn nút “Dùng” giả và không biến thành vật phẩm không rõ mục đích. Đọc code hiện hành trước khi quyết định chính xác mô hình xử lý.
3. Không có mảnh di vật nào được mất vĩnh viễn
Vì 24 mảnh giờ là điều kiện để hoàn thành game, mỗi mảnh phải:
- Có nguồn thật.
- Có tương tác hoặc proof thật.
- Có thể lấy trong cả hai thứ tự chơi.
- Có thể lấy sau khi thắng boss.
- Có cách lấy lại bằng chứng hợp lệ từ save cũ.
- Không bị khóa bởi một lựa chọn battle/repair đã qua.
- Không yêu cầu nộp lại vật phẩm quest cũ đã tiêu.
- Không bị nhân đôi reward.
Không dùng việc đã từng mở một node làm bằng chứng nhận mảnh nếu nguồn đó còn yêu cầu thao tác gameplay riêng.
4. Dùng một hệ relic duy nhất
Dùng core/registry/collection/persistence đã tạo ở Phase 3.
Không tạo:
- RelicManager thứ hai.
- Một hệ collection song song.
- Một inventory mới chỉ cho di vật.
- Một EndingManager mới.
- Một QuestManager thứ hai.
- Một bộ 24 quests mới.
- 24 NPC mới.
- Một hệ check-in hoàn toàn khác.
Các relic_* là ID gameplay canonical. Alias artifact_* chỉ được dùng cho art mapping nếu đã được xác nhận an toàn.
III. MA TRẬN 20 MẢNH CẦN TRIỂN KHAI
Đây là các lựa chọn đã chốt trong quá trình Phase 0. Đối chiếu lại với ACCESS_MATRIX đã được phê duyệt, giữ nguyên các ID thực tế và proof đã thống nhất.
Nếu phát hiện sai khác giữa prompt này và quyết định đã ghi nhận trong production checkpoint, báo rõ sai khác; không tự tạo một danh tính fragment thứ hai.
A. JEONJU — relic_jeonju_hanji_stamp
Tên: Mộc Ấn Hanji
Vai trò: Lưu giữ ký ức và hoa văn giao lưu Việt–Hàn.
Mảnh	Vị trí	Proof cần thiết
Mảnh Hoa Văn Hanok	J2	Tương tác Hana/kiến trúc Hanok có thật
Sợi Chỉ Nhuộm	J3	Puzzle nhuộm đã giải và tương tác nhận evidence
Lõi Hanji Cổ	J7	Puzzle Hanji đã giải, không tiêu story core
Con Dấu Lời Chào	Hội An H6	Photo/check-in H6 theo quyết định D04-A, nối sang Jeonju


Lưu ý giao lưu:
Con Dấu Lời Chào bắt nguồn từ trải nghiệm văn hóa Hội An, không tự biến thành phần thưởng của DQ05 hoặc HQ05.
Dùng proof H6 đã lưu để mở một hoạt động ghi nhận/giao lưu tại Jeonju nếu đó là cách mapping đã duyệt.
Người chơi đã hoàn thành H6 trong pilot hoặc save cũ phải có cách nhận fragment tương ứng mà không phải reset check-in, nhận lại xu hoặc chơi lại toàn bộ quest.
Không tự nhân đôi postcard H6 hoặc tạo thêm một bảo vật Hội An.
NPC ưu tiên:
- Hana ở J2.
- So-yeon ở J15 khi phù hợp event hiện có.
- Những NPC khác chỉ được dùng khi role/identity đã xác minh.
Minigame: GHÉP HOA VĂN GIAO LƯU
Thiết kế:
- Bốn mảnh được đưa vào bốn vị trí lắp.
- Người chơi kéo, xoay hoặc căn họa tiết giấy Hanji.
- Có các sợi chỉ nhuộm và đường nối hoa văn.
- Cần ghép đúng một mẫu mang dấu ấn giao lưu hai nền văn hóa.
- Sai hướng hoặc sai thứ tự không hoàn thành.
- Có hint rõ ràng, retry, exit/resume.
Đây phải là minigame thao tác thật, không phải bấm nút "Phục dựng" để tự thắng.
Sau khi restored:
- Hoa văn/xưởng Hanji có phản ứng thị giác.
- Một số motif giấy và ánh đèn xuất hiện nhẹ tại khu vực phù hợp.
- Hana/So-yeon có lời thoại ghi nhận sự hoàn thành.
- Có postcard hoặc nội dung văn hóa ngắn phù hợp.
- Có thể nhắc đến sự kết nối với ánh đèn Hội An.
KHÔNG:
- Tự giải J7/J12/J17.
- Thay Hanji circuit story.
- Sửa điều kiện J19.
- Tiêu ớt, dâu hoặc phần thưởng giao hàng lần hai.
- Phát lại thưởng quest khi restored.
B. HUẾ — relic_hue_dragon_seal
Tên: Ấn Rồng Ngọc Tỉ.
Di vật fantasy lấy cảm hứng từ văn hóa cung đình; không tự tuyên bố đây là hiện vật lịch sử có thật.
Vai trò: Xác thực ký ức và dấu ấn văn hóa.
Mảnh	Vị trí	Proof cần thiết
Lõi Ngọc Sông Hương	U3	Tương tác bến/sông khi đã có điều kiện qua lại hợp lệ
Mặt Ấn Rồng	U8	Puzzle cung đình/court đã giải
Dây Buộc Nghi Lễ	U16	Puzzle/mã chợ đã giải
Mảnh Văn Khắc Nhịp	U17	Puzzle kho lưu trữ/archive đã giải


Theo D03-A, fragment Văn Khắc Nhịp lấy tại U17.
G14 Gyeongju chỉ bổ sung giải thích, đối chiếu hoa văn hoặc lời kể sau đó; không bắt người chơi đến Gyeongju để lấy lại fragment Huế.
Không sử dụng ancient_seal U11 làm một mảnh mới hoặc tiêu nó lần hai.
Minigame: KHÔI PHỤC DẤU ẤN
Thiết kế:
- Ghép bốn thành phần vào các vị trí đúng.
- Có tương tác xoay mặt ấn/lớp văn khắc.
- Người chơi chạm hoặc vuốt ánh sáng để tìm những nét khắc còn thiếu.
- Khi khớp văn khắc và hướng mặt ấn, dấu ấn mới được xác thực.
- Sai hướng không hoàn thành.
- Hint dựa trên thông tin thật người chơi đã tìm được.
Đừng biến trò này thành bản sao đổi màu của Dẫn Ánh Đèn Hội An.
Sau restored:
- Một biểu tượng dấu ấn sáng lên.
- Khu vực kiến trúc phù hợp có ánh sáng trang trọng.
- Có phản ứng nhẹ tại Sông Hương hoặc khu di sản.
- NPC Huế có đoạn thoại ngắn về ý nghĩa gìn giữ văn hóa.
- Nếu đã tới Gyeongju, có thể có đoạn đối chiếu ký tự/bản khắc.
NPC Bà Nguyệt, Bác Tâm, Lê An chỉ được sử dụng nếu source xác nhận đúng role và node.
Không thay:
- Quest ancient seal U11.
- U19 beacon.
- Điều kiện mở Đà Lạt.
- Ferry/crossing permit.
- G12 dependency.
- Các puzzle U8/U16/U17.
C. ĐÀ LẠT — relic_dalat_mist_compass
Tên: La Bàn Sương Lam.
Vai trò: Định hướng giữa sương mù và ký ức thiên nhiên.
Mảnh	Vị trí	Proof cần thiết
Mảnh Kính Hồ	D0	Tương tác check-in/quan sát hồ hợp lệ
Lá Thông Dẫn Hướng	D2	Sau nhiệm vụ trả kính và tương tác với NPC phù hợp
Chuông Gió Cầu Sương	D8	Puzzle sương/cầu đã giải
Hạt Tín Hiệu Xanh	D12	Puzzle cối xay gió đã giải


Quyết định D02-A: Hạt Tín Hiệu Xanh phải có nguồn tại D12.
Không giữ mảnh bắt buộc đến Seoul S16/S18.
Seoul sau này có thể xác thực hoặc khuếch đại biểu tượng tín hiệu, nhưng không giữ fragment Đà Lạt.
Không biến Hạt Tín Hiệu Xanh thành blue_water_signal hoặc story relay item.
Minigame: TÌM ĐƯỜNG QUA SƯƠNG
Thiết kế:
- Có một vùng sương che những dấu chỉ dẫn.
- Người chơi vuốt để làm rõ những chi tiết nhỏ.
- Tìm manh mối từ hồ, lá thông, chuông gió và hướng tín hiệu.
- Xoay la bàn về đúng hướng dựa trên các manh mối đã khám phá.
- Có nhiều hướng có thể thử nhưng chỉ hướng hợp lệ mới hoàn thành.
- Sai hướng cho phép thử lại, không phạt mất mảnh.
Sau restored:
- Sương trang trí mỏng dần ở khu vực thích hợp.
- Hình ảnh hồ/đường rừng hoặc cầu có phản hồi dễ nhận thấy.
- Lumi phản ứng khi xác định được hướng.
- Một NPC Đà Lạt có lời thoại cảm ơn hoặc gợi liên hệ văn hóa với Seoul.
- Có dấu hiệu tín hiệu thiên nhiên được ghi trong Nhật ký.
Tuyệt đối không thay đổi:
- Fog-of-war gameplay.
- HIDDEN/REVEALED discovery.
- Logic mở cầu.
- Pathfinding và collider.
- Điều kiện beacon D14.
- DQ01–DQ05.
- Tín hiệu cần cho Seoul.
Sương tan là COSMETIC RESTORATION, không phải mở đường tắt hay tiết lộ node chưa khám phá.
D. GYEONGJU — relic_gyeongju_star_lens
Tên: Kính Ấn Tinh Tú.
Vai trò: Đọc ký ức, thiên văn và dấu tích văn hóa Silla cách điệu.
Mảnh	Vị trí	Proof cần thiết
Mảnh Ghi Chép Min	G3	Tương tác ghi chép/NPC nguồn thật
Mảnh Đá Khắc Sao	G4	Puzzle/quan sát di tích đã giải
Mảnh Phản Chiếu Tượng Đá	G7	Encounter tượng đã resolved: BATTLE HOẶC REPAIR
Vòng Đồng Di Tích	G15	Puzzle/tương tác di sản đã hoàn thành


Cực kỳ quan trọng: G7 phải xử lý đúng hai nhánh.
Nếu người chơi đã:
- Chiến đấu với tượng và thắng; HOẶC
- Sửa tượng theo nhánh hòa giải
thì cả hai đều phải có cơ hội nhận cùng một fragment evidence.
Đọc nguồn resolved event thực tế, không yêu cầu stone_guard_friend một cách độc quyền.
Không yêu cầu đánh lại tượng đã repaired hoặc đã defeated.
Không buộc NPC biến mất xuất hiện lại để trao item.
NPC:
- npc_keeper G9.
- npc_inscription G14.
Dùng đúng portrait mới đã import, nhưng giữ trạng thái PENDING_VISUAL_APPROVAL nếu ngoại hình chưa được người dùng chốt.
Minigame: RÀ ÁNH SÁNG TRÊN DI TÍCH
Thiết kế:
- Ghép Kính Ấn Tinh Tú từ bốn mảnh.
- Dùng thao tác kéo/di chuyển vùng sáng qua bản khắc.
- Tìm các ký hiệu thiên văn và đường nối giữa chúng.
- Phân biệt dấu sao/trăng thật với nhiễu hoặc dấu mờ.
- Người chơi phải đối chiếu manh mối rồi căn kính đúng.
- Có trạng thái sai/thử lại/hint.
Minigame không được tự giải puzzle G4 hoặc sao–trăng–sao thuộc cốt truyện chính.
Sau restored:
- Một hình chiếu sao hoặc ký ức ánh sáng hiện lên ở khu di tích.
- NPC phản ứng theo việc tượng đã repaired hay defeated.
- Hai nhánh có thể có lời thoại khác nhưng không tạo hai ending.
- Nội dung văn hóa có thể đối chiếu dấu ấn Huế nếu đã có proof tương ứng.
- Không bắt người chơi quay Huế để hoàn thành Kính Ấn Tinh Tú.
Bảo toàn:
- Lựa chọn sửa/đánh tượng.
- guard_resolved.
- G12 gate.
- Các quest và puzzle Gyeongju.
- Điều kiện mở beacon và Seoul.
E. SEOUL — relic_seoul_trirelay_prism
Tên: Lăng Kính Tam Relay.
Vai trò: Kết nối ký ức văn hóa giữa sáu thành phố.
Mảnh	Vị trí	Proof cần thiết
Mảnh Dữ Liệu Di Sản	S2	Tương tác curator/hồ sơ di sản hợp lệ
Lõi Đồng Bộ Ji-won	S7	Báo cáo/đồng bộ thật từ Ji-won
Vòng Kính Tinh Tú	S10	Puzzle/quan sát tại tháp đã hoàn thành
Khung Relay Seoul	S18	Relay Hanji đã hoàn thành bằng gameplay thật


Giữ đúng bốn milestone canonical.
ASSET SEMANTIC MISMATCH:
Bản source sheet Seoul có ba component đầu không khớp ý nghĩa của ba mảnh canonical.
Do đó:
- Không đổi tên fragment để khớp hình.
- Không gán bừa icon nước xanh/triện/Hanji thành Dữ Liệu Di Sản, Lõi Đồng Bộ Ji-won hoặc Vòng Kính Tinh Tú.
- Không tự gen hình AI mới.
- Có thể dùng full prism đã được import và những visual trung tính/slot silhouette rõ ràng cho chức năng, với trạng thái ART_PENDING.
- Nếu đã có ba icon mới được duyệt trong project, xác minh nguồn rồi mới bind.
- Không tuyên bố ART_FINAL nếu vẫn dùng placeholder.
Minigame: NỐI BA LUỒNG SÁNG
Thiết kế:
- Người chơi đã lắp bốn mảnh thật của Lăng Kính.
- Có ba luồng tín hiệu mang tính biểu tượng:
  1. Đà Lạt — thiên nhiên và định hướng.
  2. Huế — di sản và dấu ấn.
  3. Jeonju — giấy Hanji và lưu giữ ký ức.
- Người chơi kéo/xoay các đường dẫn sáng để ba luồng hội tụ trong Lăng Kính.
- Có điều kiện giao nhau/đúng hướng thật, không auto-win.
- Có feedback hợp lệ, sai hướng, hint, retry, exit/resume.
Ba luồng biểu tượng không phải ba fragment thay thế.
Không tự kích hoạt các relay chính S16/S17/S18 bằng minigame di vật.
Nếu relay thật chưa hoàn thành, fragment S18 chưa được nhận hoặc phải chờ đúng proof.
Nếu người chơi đã hoàn thành relay trước khi Phase 4 được cài, phải có retro-claim hợp lệ.
Sau restored:
- Lăng Kính phát tín hiệu văn hóa kết nối các thành phố.
- Có phản ứng thị giác tại các điểm phù hợp của Seoul.
- Ji-won và NPC liên quan có lời thoại xác nhận.
- Nhật ký ghi đã restored 6/6 nếu các relic khác hoàn tất.
Không được:
- Đánh boss hộ người chơi.
- Tự đặt seoul.final_cleared.
- Tự hoàn thành SQ03.
- Tự hoàn thành nghi thức cuối.
- Phát ending/credits.
- Tự thay đổi S21.
- Tạo ending thứ hai.
Việc sáu di vật đã restored mới chỉ khiến người chơi đủ điều kiện phần di vật cho Phase 5, không phải hoàn thành toàn game.
IV. CHẤT LƯỢNG CỦA NĂM MINIGAME
Tôi không muốn năm màn hình giống hệt nhau chỉ khác ảnh và tên.
Yêu cầu thiết kế:
City	Tương tác chính	Cảm giác
Jeonju	Kéo, xoay, ghép hoa văn Hanji	Thủ công, sáng tạo
Huế	Quét sáng, xoay và khớp dấu ấn	Trang trọng, khám phá
Đà Lạt	Vuốt sương, tìm dấu, định hướng	Thiên nhiên, bí ẩn
Gyeongju	Rà kính và giải đọc bản khắc sao	Khảo cổ, suy luận
Seoul	Nối ba luồng sáng qua lăng kính	Hội tụ, kết nối


Tất cả phải có:
- Gameplay interaction bằng pointer/touch thật.
- Trạng thái chưa giải, thử sai, giải đúng.
- Hướng dẫn rõ và không gây hiểu nhầm.
- Hint và retry.
- Save sau khi nhận đủ mảnh, assembled, restored.
- Không grant khi chỉ xem preview.
- Không auto-pass vì mở popup.
- Không lặp reward khi replay hoặc load save.
Nên tận dụng UI framework Phase 3, nhưng gameplay rule của từng thử thách phải khác nhau.
Không tạo thêm năm hệ minigame độc lập nếu có thể mở rộng cùng một controller/presentation architecture.
Không thêm quảng cáo, timer ép buộc, thanh năng lượng hoặc tiền để mở minigame.
V. STORY INTEGRATION — PHẢI SỬA CỐT TRUYỆN THẬT
Đây là yêu cầu quan trọng ngang gameplay.
Tôi không muốn chỉ có năm minigame và icon đẹp nhưng NPC không biết di vật tồn tại.
Với mỗi thành phố, hãy bổ sung một mạch side-story ngắn, hợp lý:
1. Người chơi gặp NPC hoặc dấu tích văn hóa.
2. Được giới thiệu bí ẩn hoặc ý nghĩa di vật.
3. Tìm được các nguồn fragment qua hoạt động hiện có và tương tác mới vừa đủ.
4. Có lời nhắc tiến độ khi mới tìm được một phần.
5. Sau đủ bốn mảnh, NPC hoặc Nhật ký hướng dẫn phục dựng.
6. Sau restored, địa danh và NPC có phản ứng.
7. Có một liên hệ văn hóa tự nhiên với thành phố khác.
Không cần tạo lại 29 quest chính. Dùng side interactions, dialogue branches, collection descriptions và các cờ relic đã lưu.
Bắt buộc có lời thoại theo trạng thái thật:
- Chưa biết di vật.
- Đã phát hiện manh mối.
- Có 1–3/4 mảnh.
- Có đủ 4/4.
- Đã assembled nhưng chưa hoàn thành trial.
- Đã restored.
- Gặp lại NPC sau restored.
- Có proof trao đổi liên thành phố nếu phù hợp.
Không nhất thiết mọi NPC đều cần bảy đoạn thoại riêng, nhưng nội dung phải thay đổi hợp lý theo tiến độ.
Không để NPC nói “đã phục dựng” khi chưa restored.
Không nói cả thành phố đã khai sáng chỉ vì beacon đã bật.
Không giả định người chơi đi Việt Nam trước hay Hàn Quốc trước.
Không viết thoại nhắc nhân vật chưa gặp như thể đã quen biết.
Giữ phong cách cốt truyện phiêu lưu, ấm áp và có liên hệ Việt–Hàn; tránh thuyết minh lịch sử quá dài.
Không tự khẳng định chi tiết hiện vật fantasy là hiện vật lịch sử chính xác.
Cập nhật:
- Runtime dialogue data thực tế.
- NPC event/interaction descriptions.
- Collection descriptions và hint.
- Check-in/postcard narration liên quan.
- Docs/Story/FULL_STORY_PLAYTEST.md.
- Story continuity/canonical docs đang hoạt động.
Chỉ chỉnh những đoạn cần thiết để tránh mâu thuẫn.
Các tài liệu báo cáo kiểm thử lịch sử được giữ nguyên làm bằng chứng thời điểm cũ, không sửa ngược chúng để tạo cảm giác đã PASS từ trước.
Quy tắc cốt truyện kết thúc:
Sáu di vật là điều kiện của ending duy nhất, không phải điều kiện để mở thành phố hoặc đánh boss.
Ở Phase 4 có thể cập nhật lời dẫn và Journal hướng về cuộc hội tụ cuối, nhưng chưa sửa luồng nghi thức/credits của Phase 5.
VI. CULTURAL EXCHANGE — TƯƠNG TÁC VIỆT–HÀN
Phải có sự liên kết văn hóa thực sự, không chỉ sáu thành phố độc lập.
Giữ các kết nối đã duyệt:
Hội An ↔ Jeonju
- Ánh đèn lồng và giấy/hoa văn Hanji.
- H6 là nguồn liên quan Con Dấu Lời Chào.
- Hana/So-yeon có thể giải thích một kỹ thuật, kỷ niệm hoặc chi tiết văn hóa.
Huế ↔ Gyeongju
- Dấu ấn, văn khắc và ký ức di sản.
- Fragment Văn Khắc Nhịp ở U17.
- G14 là đối chiếu, không giữ fragment Huế.
Đà Lạt ↔ Seoul
- Hạt Tín Hiệu Xanh ở D12.
- Seoul tiếp nhận/khuếch đại liên hệ về định hướng và thiên nhiên.
- Không bắt quay Seoul mới được phục dựng La Bàn.
Jeonju ↔ Hội An/Đà Lạt qua đặc sản
- Có thể thêm clue/dialogue sau những giao dịch có thật.
- HQ05, HQ06, HQ07, DQ05 giữ nguyên item/reward/completion.
- Không bắt buộc toàn bộ food sidequests chỉ để nhận một relic.
Cross-city dialogue phải có điều kiện thật từ save. Người chơi bỏ qua một sidequest vẫn có thể phục dựng đủ sáu relic theo các nguồn đã duyệt.
Không chuyển fragment sang city khác chỉ để tạo cảm giác giao lưu nhiều hơn.
VII. WORLD REACTIONS — KHÔI PHỤC CẢNH QUAN
Mỗi relic restored phải tạo một thay đổi thị giác nhận ra được tại thành phố tương ứng.
Không chỉ thêm một icon ✓ trong Collection.
Jeonju:
- Xưởng/đèn/hoa văn Hanji có hiệu ứng hoàn thiện.
- Có motif giao lưu trên các địa danh đã khám phá.
Huế:
- Dấu ấn rồng hoặc hoa văn nghi lễ sáng lên.
- Có phản ứng nhẹ tại khu di sản và Sông Hương.
Đà Lạt:
- Sương trang trí mỏng đi.
- Một vài điểm thiên nhiên hiện rõ, mang cảm giác đã tìm lại phương hướng.
Gyeongju:
- Hình chiếu sao/ký ức từ kính di vật.
- Có phản hồi khác nhau theo tượng repaired hoặc defeated.
Seoul:
- Lăng Kính phát sáng.
- Có tín hiệu biểu trưng nối các di vật/thành phố.
- Chưa phát cutscene kết thúc hoặc credits.
Yêu cầu kỹ thuật:
- Dựng static restored visuals từ save khi re-entry.
- Celebration chỉ phát trong transition restored lần đầu.
- Không phát lại phần thưởng.
- Không che HUD, node marker, NPC, đường đi hoặc touch.
- Không làm lộ vùng HIDDEN.
- Không ảnh hưởng occlusion Chùa Cầu.
- Không sửa pathfinding/collider.
- Không thay ảnh canonical Map A/B.
- Không dùng shader nặng hoặc tài nguyên không phù hợp Android.
Dùng lớp visual mới theo đúng kiến trúc hiện có, không viết lại CityMap từ đầu.
QUYẾT ĐỊNH CHỐT VỀ SỐ LẦN PHẢN ỨNG THỊ GIÁC — BẮT BUỘC
Đọc trước khi code cả mục VII này. Đây là yêu cầu mới nhất thay cho mọi gợi ý “thêm mốc phản ứng nhỏ” ở năm thành phố sau. Không bổ sung các bảo vật nhỏ, không đặt 5 trạng thái B mới.
HỘI AN — A/B/C (hai bước phục hồi, giữ nguyên vertical slice và yêu cầu 3-state lighting nếu đã được triển khai):
- A: trước khi thực sự hoàn thành mai-lantern ở H2, toàn map thiếu ánh sáng (~40% tối hơn map canonical, vẫn đọc rõ; nhặt Mắt Đèn H3 mà chưa trả chưa sang B).
- B: sau khi trả Mắt Đèn cho Cô Mai và tutorial completed, vùng liên tục khoảng 40–50% khu phố quanh H2 sáng lên rõ rệt, khu khác tối; H7 hoàn tất nhưng relic chưa restored vẫn là B.
- C: chỉ sau đúng 4 fragment, assembly, HQ01/H7 proof và giải thật minigame Dẫn Ánh Đèn để relic_hoi_an_lantern có state restored, toàn bộ map trở về độ sáng canonical và có lễ ánh sáng + landmark halos một lần. Save/re-entry dựng state đúng, không replay celebration.
- Đây là thông số/định hướng để bảo toàn hoặc kiểm tra trong Phase 4; không tự chạy lại/viết lại một task Phase 3 nếu đã PASS. Không sửa H6 postcard, hai icon, interaction minigame hoặc Chùa Cầu; không động trực tiếp ảnh Map A/B.
HUẾ / ĐÀ LẠT / JEONJU / GYEONGJU / SEOUL — A/C (MỘT bước phục hồi lớn):
- A: trước khi di vật của chính thành phố có restored=true, giữ map canonical và presentation hợp lý; vẫn có thể có hiệu ứng quest/npc/beacon hiện hữu và phần thưởng truyền thống, nhưng không có một biến đổi cảnh quan phục hồi chính thứ hai. Không tự giảm sáng map nền 40% như Hội An nếu người dùng chưa duyệt. Không tạo “Mắt Đèn Huế”, “mảnh phục hồi nhỏ”, “bảo vật phụ” hoặc thêm cờ/tầng tiến triển.
- C: chỉ khi bốn mảnh hợp lệ + assembly + thử thách phục dựng thành công, phát một world restoration rõ ràng cho đúng city; hiệu ứng đủ thấy ngay trong map và còn hiện sau reload, kể cả player rời map/trở lại. beacon_lit, boss win, puzzle xong, check-in xong hoặc nhặt 4/4 mảnh không phải restored.
- Huế relic_hue_dragon_seal — Xác thực: dấu ấn/hoa văn di sản sáng ở các công trình hợp lý, phản chiếu Sông Hương; không đổi ferry, gate, ấn tín cốt truyện U11 hay điều kiện U19.
- Đà Lạt relic_dalat_mist_compass — Định hướng: sương trang trí nhẹ tan/giãn, hồ, thông, cầu và lối đi nổi rõ; fog-of-war discovery, node HIDDEN, collider và đường di chuyển không đổi.
- Jeonju relic_jeonju_hanji_stamp — Lưu giữ: ánh đèn xưởng Hanji và một số hoa văn trang trí lan qua khu phố; không sửa progress J19/food quests hay đổi mặt NPC.
- Gyeongju relic_gyeongju_star_lens — Đọc ký ức: vệt chiếu sao/văn khắc ở di tích, theo nhánh G7 BATTLE/REPAIR có thoại/nhấn thị giác khác nhưng cùng một phục hồi và cùng relic state, không split ending.
- Seoul relic_seoul_trirelay_prism — Kết nối: lăng kính và một số relay/địa danh có liên kết ánh sáng mang tính biểu tượng; không tự kích S16/S17/S18, đánh thắng boss, phát ceremony/credits, mở city hoặc hoàn thành ending. Nghi thức sáu di vật sau boss thuộc Phase 5 là sự kiện kết thúc riêng, không phải trạng thái phục hồi nhỏ thứ hai của Seoul.
CHẤT LƯỢNG & QA:
- Overlay/mesh bám bản đồ khi zoom/pan; không phủ UI, portrait, node, Lumi; không chặn raycast/cảm ứng. Không vẽ lên HIDDEN, không sửa 133 node/155 edge/route Phase 3.5. Không thay ảnh canonical, không gen asset nghệ thuật mới, không có shader nặng; nếu art thiếu, fallback nhẹ và ghi ART_PENDING.
- Chụp Hội An A/B/C và mỗi thành phố khác A/C bằng cùng focus/zoom/resolution và cùng trạng thái tiến độ khác ngoài relic, có video hoặc frame lúc celebration; ảnh từ Player thực, không sử dụng mockup/concept để tuyên bố PASS. Nếu chênh lệch không dễ nhận ra, ghi VISUAL_PENDING, không gọi đẹp rồi bỏ qua.
- Kiểm save/restart/re-entry: relic chưa restored vẫn A (hoặc B với Hội An), restored trở lại C, celebration 1 lần; không thưởng trùng hoặc tự restore từ quest/boss.
VIII. PHOTO JOURNAL VÀ CHECK-IN
Kế thừa Photo Journal và 18 check-in/ảnh postcard hiện hành nếu phiên tích hợp mới đã hoàn tất. Không lùi về dữ liệu 12 check-in, không import ảnh tạm cũ đè bản đã chỉnh. Nếu có thành tựu 18/18 mới, chỉ bảo vệ nó; không tạo hệ achievement khác trong Phase 4.
H6 postcard:
- GIỮ NGUYÊN minigame, icon và logic đã PASS.
- Chưa sửa chữ “Đã khớp” đang mờ.
- Không thay đổi hai biểu tượng trong ảnh.
- Đây là công việc UI polish riêng sau.
Không tạo hệ Photo Journal thứ hai.
Ở năm thành phố còn lại:
- Tận dụng check-in hiện có ở Huế, Đà Lạt, Jeonju.
- Xác minh marker Camera-capable và check-in definitions thực tế.
- Chọn những landmark phù hợp để có hoạt động ngắn, đa dạng.
- Không ép tất cả điểm check-in phải có minigame phức tạp.
- Không biến Photo Journal thành điều kiện mở city.
Gợi ý landmark mới:
- Gyeongju G4 hoặc G7.
- Seoul S10 hoặc S18.
Chỉ bổ sung nếu tương tác hợp cảnh quan, proof hợp lệ và không tạo quest/node mới không cần thiết.
Mỗi postcard cần:
- Một thông tin văn hóa ngắn.
- Một thao tác quan sát/chụp kỷ niệm trong game nếu thích hợp.
- Trạng thái hoàn thành.
- Xem lại trong Nhật ký.
- Save/restart.
- Không lặp reward.
- Không cần camera vật lý của điện thoại hay permission OS.
Nếu Photo Journal có thể hỗ trợ clue fragment thì tách claim fragment khỏi thưởng check-in cũ và chống duplicate.
IX. ART, NPC VÀ RESOURCE SAFETY
Art di vật
Ưu tiên sử dụng các sprite native thực tế đã import ở Phase 2/3.
Không tự re-slice các sheet nếu sprite IDs/bindings đã ổn định.
Kiểm tra:
- Native GUID/localID.
- Full relic icon.
- Bốn component icon tương ứng.
- Alpha/transparent padding.
- Độ rõ trên nền sáng/nền tối.
- Kích thước hiển thị trên mobile.
- Không cắt lẹm glow.
- Không nhầm fragment này với relic khác.
Ba icon Seoul còn mismatch phải đánh dấu ART_PENDING.
Nếu thiếu final art:
- Có thể hoàn thiện logic bằng biểu tượng chức năng trung tính, nhận diện rõ là placeholder.
- Không gọi đó là art đã duyệt.
- Không đổi gameplay ID.
- Không lấy hình sai nghĩa làm bản final.
- Không tải/sinh art mới không có phép.
NPC
Tận dụng các NPC hiện có đúng vai trò.
Bốn NPC mới từ Phase 1 có thể đã import và binding đúng, nhưng ngoại hình chưa được user VISUAL_APPROVED.
Giữ:
- NPC Engineer ở Seoul S11.
- NPC Curator ở Seoul S2.
- NPC Keeper ở Gyeongju G9.
- NPC Inscription ở Gyeongju G14.
Không tự đổi mặt, gộp người hoặc thay portrait canonical.
Ji-won S7 giữ đúng identity/sprite đang có.
Không tự lấy ảnh NPC này gán cho NPC khác chỉ để mọi hội thoại có portrait.
X. SAVE, PERSISTENCE VÀ OLD SAVE
Đây là hạng mục bắt buộc.
Giữ architecture v1/v2 hiện có và additive persistence của Phase 3.
Mỗi fragment:
- Stable ID.
- Một nguồn sở hữu thực tế.
- Grant once.
- Có proof riêng.
- Có thể re-entry/restart.
- Không tự biến mất khi city reload.
- Không được cộng lại khi xem journal.
- Không cần nhặt lại sau restored.
Mỗi relic:
- Collected count chính xác.
- Assembled state chính xác.
- Trial completed.
- Restored state tách biệt.
- Completion effect phát một lần.
- Có thể xem lại UI mà không grant reward.
OLD SAVE:
- Người đã hoàn thành puzzle nguồn từ trước không bị buộc giải lại puzzle.
- Người đã đánh hoặc sửa tượng G7 vẫn lấy được evidence.
- Người đã hoàn thành relay Seoul vẫn có đường nhận fragment S18.
- Người đã hoàn thành H6 trước Phase 4 vẫn có thể xử lý Con Dấu Lời Chào Jeonju.
- Người đã xem credits cũ không bị xóa boss victory/quest/reward/history.
- Người đã tiêu story item không phải nộp lại.
- Người thắng boss vẫn quay các thành phố để thu mảnh.
Nếu nguồn one-shot cũ không còn tương tác, tạo claim interaction dựa trên proof thật đã persisted, không phát lại quest cũ.
Không tự thêm restored flag bằng cách suy từ "quest đã xong".
Không phát 24 mảnh từ save fixtures để báo PASS.
XI. THỰC HIỆN TUẦN TỰ, KHÔNG DỪNG VÔ ÍCH
Thứ tự:
STEP 1 — Jeonju
Triển khai 4 fragment, assembly/trial, NPC/dialogue, restored visual, QA và checkpoint.
STEP 2 — Huế
Triển khai tương tự, QA nguồn U3/U8/U16/U17 và old-save recovery.
STEP 3 — Đà Lạt
Triển khai minigame sương/la bàn, QA đặc biệt về discovery, cầu và fog.
STEP 4 — Gyeongju
Triển khai lens, QA đặc biệt về G7 BATTLE/REPAIR và đã gặp tượng từ trước.
STEP 5 — Seoul
Triển khai prism, proof S2/S7/S10/S18, QA relay, post-boss, ART_PENDING.
STEP 6 — Cross-city integration
Kiểm tra:
- H6 → Jeonju.
- Huế ↔ Gyeongju.
- Đà Lạt ↔ Seoul.
- Food sidequests vẫn như cũ.
- Tổng Collection 24/24.
- Sáu restored states chính xác.
- Không auto-ending.
STEP 7 — Regression và báo cáo Phase 4
Chạy QA targeted và cross-city E2E.
Sau mỗi thành phố:
- Ghi file đã sửa.
- Capture ảnh runtime thật.
- Save/restart test.
- Ghi tiến độ vào báo cáo.
- Tạo checkpoint hợp lý, không sao chép lại nhiều GB Library/build.
- Tiếp tục thành phố tiếp theo nếu PASS và không có blocker nghiêm trọng.
Không dừng chỉ vì thiếu audio hoặc ba icon final Seoul nếu gameplay có thể tiếp tục an toàn với ART_PENDING.
Không chạy nhiều agent cùng chỉnh core relic/SaveSystem/GameplayContent.
Không sửa các file đã được Phase 3.5 thay đổi bằng cách checkout phiên bản cũ.
XII. QA BẮT BUỘC
1. Functional QA cho từng thành phố
Kiểm:
- Mỗi fragment lấy bằng source/gesture/proof thật.
- Wrong interaction không grant.
- Duplicate interaction không grant lần hai.
- Assembly chưa đủ mảnh phải bị chặn.
- Trial sai không restored.
- Trial đúng restored một lần.
- Save/restart giữ trạng thái.
- Re-entry dựng visual.
- NPC dialogue đổi đúng theo progress.
- Check-in không nhận lại thưởng.
- Ngắt/reopen minigame không mất mảnh.
1A. Regression chìa khóa H5 → H7 và trạng thái Inventory
- Trước H5: chưa có link_key, cả Huế/Jeonju vẫn khóa.
- H5 chiến đấu thắng: nhận link_key đúng một lần; H7 chưa hoàn thành vẫn chưa mở city.
- H5 hack/vô hiệu hóa hợp lệ: same item/proof, không nhận trùng; H7 vẫn là bước mở map.
- Đến H7 khi đã đạt các proof HQ01: có lời thoại/animation sử dụng hoặc đồng bộ chìa; hoàn thành H7 mới mở Huế+Jeonju cùng lúc. Inventory/hồ sơ thể hiện “đã dùng/đã đồng bộ” dễ hiểu nếu item còn trong túi.
- Reload trước/sau H5, trước/sau H7: item/flag/status khớp; H7 không tiêu thiếu item, không nhân đôi chìa; quest không mất mục tiêu.
- Old save đã hoàn thành H7 nhưng link_key không còn / item vẫn còn / H5 nhánh khác: không ép đánh lại Bot Mờ-01, không hồi khóa map, không cấp key/tiền lại và không phá HQ01.
- Toàn WorldMap: U19→Đà Lạt, J19→Gyeongju, đủ bốn beacon→Seoul vẫn nguyên; KHÔNG cần link_key để Enter các city sau khi đã mở.
- Nếu runtime đã đúng, chỉ cần QA và cải thiện description; không “sửa” logic đang hoạt động.
2. Two-route QA
Tối thiểu kiểm:
- VN_FIRST: Hội An → Huế → Đà Lạt → Jeonju → Gyeongju → Seoul.
- KOREA_FIRST: Hội An → Jeonju → Gyeongju → Huế → Đà Lạt → Seoul.
Cả hai tuyến phải lấy được 24/24 fragments và restored 6/6.
Các city chưa mở không được vào bằng relic UI.
Không dùng forced flags thay thế player progression để chứng minh E2E.
Nếu dùng fixture riêng để kiểm cạnh khó, công bố rõ fixture và không gọi nó là earned journey.
3. Post-boss QA
Tối thiểu:
- Boss resolved nhưng còn thiếu fragment.
- Quay về Huế/Đà Lạt/Jeonju/Gyeongju/Hội An.
- Nhận fragment còn thiếu.
- Restored thêm relic.
- Boss không respawn.
- Rewards không bị nhận lại.
- World Map không bị khóa.
Không đòi defeatedEnemies nếu boss đã resolved qua nhánh repair.
4. Old-save QA
Dùng bản sao của earned save lịch sử.
Test:
- Đã giải puzzle.
- Đã hoàn thành check-in.
- Đã dùng story item.
- G7 battle.
- G7 repair.
- Seoul relays đã xong.
- Boss đã thắng.
- Credits lịch sử đã xem.
- Restart sau khi nhận fragment mới.
Không ghi đè save gốc của người dùng.
5. Visual QA
Chụp runtime thật:
- Collection từng relic.
- Bốn fragment.
- Assembly.
- Minigame trước/sai/đúng.
- City trước/sau restored.
- NPC liên quan.
- Một check-in/postcard mới nếu triển khai.
Ảnh phải từ actual Windows Player hoặc Unity runtime hợp lệ, không phải concept/mockup.
Nếu test trên Windows bằng touchscreen simulation, ghi rõ là simulated input, không gọi Android device PASS.
6. Data invariants
Sau Phase 4:
- 6 cities.
- 133 canonical nodes.
- 29 canonical quests.
- 22 story puzzles.
- World unlock rules giữ nguyên.
- No new mandatory seventh puzzle.
- 24 unique canonical fragment IDs.
- 6 unique relic IDs.
- Không duplicate story item.
- Không duplicate reward.
- Không mất cạnh do Phase 3.5.
- Không khiến route/waypoint/sorting bị hồi quy.
Chỉ báo PASS nếu có runtime/source evidence tương ứng.
XIII. YÊU CẦU SAU KHI HOÀN THÀNH 6/6 RELICS
Khi người chơi đã restored đủ sáu di vật:
- Collection hiển thị 6/6 chính xác.
- Có thể xem toàn bộ sáu relic.
- Mỗi thành phố giữ phản ứng visual của di vật mình.
- Có thể hiển thị mạng biểu tượng sáu thành phố.
- Nhật ký thể hiện phần di vật đã hoàn tất.
NHƯNG:
- Không tự hiện credits.
- Không tự hoàn thành SQ03.
- Không tự bật game.ending_seen nếu chưa có logic Phase 5.
- Không thay boss victory.
- Không mở game ending thứ hai.
- Không tự ghi global completion mới.
Chuẩn bị một API/query xác định chính xác allSixRelicsRestored để Phase 5 đọc.
Không viết kết luận “GAME COMPLETED” chỉ vì 6/6.
Legacy S21 ending hiện còn cần Phase 5 patch — ghi rõ trong báo cáo, không che giấu.
XIV. BÁO CÁO VÀ BẰNG CHỨNG
Tạo/cập nhật báo cáo dưới Docs/Gameplay và Docs/QA, ưu tiên:
- Docs/Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md
- Docs/Gameplay/ARTIFACT_FRAGMENT_ACCESS_MATRIX.md
- Docs/Gameplay/SIX_ARTIFACT_PROGRESSION_DESIGN.md
- Docs/Story/FULL_STORY_PLAYTEST.md
- Docs/QA/PHASE4_SIX_RELICS_E2E.md
- Docs/QA/PHASE4_RUNTIME_REVIEW.html
Có thể dùng tên/đường dẫn báo cáo hiện hành tương đương, không sinh nhiều bản trùng.
Báo cáo phải có:
1. Bảng 24 fragment: nguồn, proof, đã implement hay chưa.
2. Bảng sáu relic: collected/assembled/trial/restored.
3. Các NPC và hội thoại đã sửa.
4. Các hoạt động cross-city được triển khai.
5. Những postcard/check-in mới.
6. File code/data/art thay đổi.
7. Các thử thách minigame thực sự hoạt động.
8. Ảnh Hội An A/B/C và năm city A/C (focus/zoom/resolution tương đồng), frame celebration và trạng thái VISUAL_APPROVED/VISUAL_PENDING.
9. Save/restart và old-save regression.
10. Hai tuyến VN_FIRST/KOREA_FIRST.
11. Post-boss access.
12. Windows build result mới nhất.
13. Android build/device status riêng.
14. Art/SFX/visual approval pending.
15. Lỗi P0/P1/P2/P3 còn tồn tại.
16. Điều kiện duy nhất chưa implement ở Phase 5.
17. Những gì cần chủ dự án duyệt.
18. Bảng link_key: ID/tên/nguồn H5/hack vs combat, mục tiêu HQ01, cách chứng minh đã dùng tại H7, trạng thái sau H7 trong Inventory, old-save outcomes và bằng chứng map unlock không bị thêm gate mới.
19. Xác nhận rõ Hội An chỉ A/B/C; năm city khác chỉ A/C, các mốc triggered đúng relic restored.
Không tự sửa báo cáo lịch sử để biến các lượt QA cũ thành mới.
Không tuyên bố Android PASS khi chưa có device test.
Nếu phải dùng placeholder ba icon Seoul, báo rõ 3/3 ART_PENDING và tạo manifest yêu cầu art chính xác.
XV. ĐIỂM DỪNG VÀ LỆNH CUỐI
Mục tiêu sau Phase 4:
- Hội An: 1 relic đã có từ Phase 3.
- Jeonju: 1 relic mới.
- Huế: 1 relic mới.
- Đà Lạt: 1 relic mới.
- Gyeongju: 1 relic mới.
- Seoul: 1 relic mới.
TỔNG MỤC TIÊU:
- 6/6 relic có gameplay phục dựng.
- 24/24 fragment sources có thể tiếp cận.
- 6/6 trials chơi thật.
- 6/6 restoration states.
- 6 city world reactions.
- Cross-city story hoạt động.
- Collection/Photo Journal thống nhất.
- Save/restart/old-save/post-boss không bị phá.
- World Map và boss độc lập với relic.
- Ending mới vẫn chờ Phase 5.
Giữ nguyên H6 postcard của Hội An, kể cả text "Đã khớp" còn hơi mờ; đó là UI polish dành cho sau.
Bảo toàn ba trạng thái ánh sáng Hội An và các tuyến đường Phase 3.5 nếu đã triển khai.
KHÔNG bắt đầu Phase 5, Phase 6 hoặc Phase 7 trong lượt này.
Sau khi hoàn tất năm thành phố, chạy regression, tạo báo cáo, lưu checkpoint an toàn và DỪNG ĐỂ TÔI NGHIỆM THU PHASE 4.
Nếu gặp blocker thật ở một thành phố:
- Ghi file/điều kiện/nguyên nhân cụ thể.
- Không tự bẻ thiết kế hoặc giả proof.
- Tiếp tục những thành phố/phần độc lập khác nếu an toàn.
- Chỉ dừng toàn lượt nếu nguy cơ hỏng source, save hoặc progression.
KHÔNG CHỈ THIẾT KẾ — HÃY TRIỂN KHAI VÀ KIỂM THỬ GAMEPLAY THẬT.
Tôi muốn Phase 4 hoàn thành phần nội dung năm di vật còn lại, để Phase 5 sau đó chỉ cần tập trung xây dựng một ending duy nhất tại Seoul.