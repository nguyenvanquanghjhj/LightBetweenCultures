# Phase5 — trạng thái cốt truyện kết thúc hiện hành

Boss battle/repair chỉ giải quyết mối nguy. SQ03 báo tin bình an tại S21 độc lập với sáu di vật. Sau đủ sáu restored, Lumi chủ động xác nhận **Lễ Kết Nối Sáu Miền Ánh Sáng**. Sáu bước Đánh Thức → Xác Thực → Định Hướng → Lưu Giữ → Đọc Ký Ức → Kết Nối đưa về một ending/credits duy nhất. Legacy credits không tự nhận hoàn tất nghi thức mới; thiếu di vật vẫn có thể đi lại và không đánh boss lại.

Nguồn runtime: NodeEventManager.FinalConnection, GameplayPanels.FinalConnection, FinalConnectionPresentation.json và final_festival. Nội dung SQ03 mới chỉ sửa offer/reminder/thanks cho đúng việc xác nhận hậu boss; không đổi điều kiện/reward/vị trí. 29quest/22puzzle/133node/155edge/24fragment/6relic/18check-in giữ nguyên. [QA hiện hành](../QA/PHASE5_ENDING_E2E.md). Các bảng Phase4 bên dưới mô tả nền tảng được kế thừa.

---

# Phase4 — story runtime delta

| City / di vật | Nguồn | Điểm phục dựng | Trial thật | Saved collected / assembled / trial / restored | World Reaction |
|---|---|---|---|---|---|
| hoi_an · Đèn Mắt Phố | H0/H4/H10/H2 | H2 | Dẫn Ánh Đèn cũ; cần H7/HQ01 | 4/4 · true · true · true trong earned final | A/B/C;10 halos giữ nguyên |
| jeonju · Mộc Ấn Hanji | J2/J3/J7 + proof H6 | J2 | Xoay4 lớp hoa văn đúng mẫu | 4/4 · true · true · true trong earned final | A/C; đèn xưởng + dải giấy |
| hue · Ấn Rồng Ngọc Tỉ | U3/U8/U16/U17 | U7 | Rà4 nét + căn hướng mặt ấn | 4/4 · true · true · true trong earned final | A/C; điểm sáng hai bờ |
| da_lat · La Bàn Sương Lam | D0/D2/D8/D12 | D2 | Vuốt sương + tìm4 dấu + căn Đông Bắc | 4/4 · true · true · true trong earned final | A/C; tan lớp sương trang trí |
| gyeongju · Kính Ấn Tinh Tú | G3/G4/G7/G15 | G9 | Rà kính + đọc3 dấu, loại mồi + căn Tây | 4/4 · true · true · true trong earned final | A/C; sao di tích, G7 khác màu theo nhánh |
| seoul · Lăng Kính Tam Relay | S2/S7/S10/S18 | S7 | Kéo3 luồng + xoay hướng, hội tụ hình học | 4/4 · true · true · true trong earned final | A/C; tín hiệu3 relay dạng biểu tượng |

H2 tutorial chỉ ánh sáng cục bộ; H7/HQ01 chỉ beacon/world unlock; Đèn Mắt Phố mới full-city restoration. Các proof giao lưu đã triển khai: H6→Hana J2, actual chili_shared tại J15, văn khắcG14 đã đọc đối chiếuU17, ba nguồn văn hóa trong Lăng Kính. Không đổi quest graph/29quest/22puzzle hoặc vật phẩm story.

Runtime: CulturalRelics.json.story → NodeEventManager/RelicCollection.Story → dialogue/check-in caption thật. Trước/partial/4of4/assembled/restored/revisit theo state đã lưu; lời phản hồi không tự trao thưởng. [Phase4 report](../Gameplay/PHASE4_FIVE_RELICS_IMPLEMENTATION_REPORT.md) · [runtime](../QA/PHASE4_RUNTIME_REVIEW.html).

---

# LIGHT BETWEEN CULTURES — FULL STORY PLAYTEST V1 (6 THÀNH PHỐ / 133 NODE)

**Nền tảng kế thừa (Phase4,18/10/2026):**29 quest/22 puzzle/133node giữ nguyên. Sáu di vật/24mảnh có gameplay thật và side dialogue runtime; hai thứ tự fresh và old-save hậu boss được kiểm thử. Hội An A/B/C; năm city A/C chỉ sau trial hợp lệ. **Phase5 hiện đã có guard ending + Lễ Kết Nối runtime**, QA mới xem PHASE5_ENDING_E2E; không dùng kết quả Phase4 để tự chứng nhận ending.

**Mục đích:** ĐẶC TẢ NỘI DUNG để Codex tích hợp trọn cốt truyện vào Unity cho một lượt **chơi thử xuyên sáu thành phố**. Đây **không phải báo cáo game đã code xong**. Kết hợp nội dung canonical từ `LBC_Gameplay_6Cities_Detailed_v1` và `LBC_HoiAn_StoryExpansion_v1`; phần câu thoại bổ sung cho chỗ trống là **PLAYTEST NEW** có thể chỉnh sau.

## A. Mệnh lệnh triển khai / thứ tự ưu tiên

1. Dùng **project hiện tại**. Không ghi đè Hội An, art canonical, IDs đã phát hành, SaveSystem; trước khi sửa so bản mới nhất, tránh thực hiện lại công việc đã xong.
2. Kết thúc lượt này phải có **hành trình chơi được từ H0 đến S21**, bao gồm các phần truyện ở tất cả 133 node (node định hướng có thể là tương tác mô tả), 24 quest gốc, HQ05–HQ07 và `HA_FOOD_PREP` nếu đã có trong runtime. Không đánh dấu DATA_ONLY là IMPLEMENTED.
3. Cấu hình gốc 31 item / 24 quest; checkpoint hiện tại đã mở rộng đến khoảng **37 item / 28 quest**. **Không reset về con số gốc** hoặc nhân đôi ID khi nhập nội dung.
4. **Ưu tiên**: dữ liệu và save runtime mới nhất đã qua nghiệm thu > story expansion Hội An > kit level design sáu thành phố. Đối chiếu mỗi ID; nếu khác thì giữ ID đã phát hành và ghi manifest chuyển đổi. Câu thoại bổ sung ở đây chỉ để playtest, không ghi là câu gốc.
5. Hạn chế phạm vi lượt này là CONTENT: quest, dialog, node event, chapter gating, cutscene ngắn và test. **Không thiết kế lại boss/bot/marker !**, không đổi tranh map, không sửa mạng đường nếu một prompt graph/UI riêng đã làm hoặc đang làm. Cho phép tạm art thiếu với nhãn rõ; tuyệt đối không tráo nhân vật có danh tính.
6. Một node thường có một câu hỏi/lời thoại/phát hiện, chưa cần một boss. Các ngã rẽ chỉ cần một đoạn mô tả có ý nghĩa và logic đường đi; không thưởng hoặc kích hoạt quest liên tục khi chỉ đi ngang.
7. Mọi thưởng/consumable dùng grant một lần, chỉ thay đổi inventory/flag sau lựa chọn `Xác nhận`; lưu trạng thái city-local và global rõ ràng. Không khóa người chơi khỏi vật phẩm bắt buộc, nhất là quest xuyên thành phố.
8. Khi gặp xung đột trong dữ liệu nguồn (ví dụ bản thiết kế 29 cạnh Hội An nhưng runtime đang giảm degree; Điều kiện U5 cần vật phẩm U11 nằm sau gate; nhánh HQ03 hồi thư), hãy sửa thứ tự/gate tối thiểu để tiến trình chơi được và ghi `PLAYTEST_RESOLUTION`, **không tự đổi node ID, không bypass gate**.

## B. Cốt truyện toàn game (CANONICAL + diễn giải cho playtest)

Lumi là chú gà mang chiếc đèn nối chuyện Việt–Hàn. Lễ hội đèn Hội An gặp sự cố vì nhịp ánh sáng bị can thiệp. Cảnh báo từ Bot Mờ-01 bị cắt: *“Đừng bật cả sáu trạm cùng lúc.”* Lumi chưa biết bot đang đe dọa hay bảo vệ. Người chơi tự chọn hai nhánh sau Hội An: **Hội An → Huế → Đà Lạt** và **Hội An → Jeonju → Gyeongju**, mỗi nhánh có thể làm theo thứ tự bất kỳ và có phụ bản trao đổi quà. Khi bốn beacon Huế–Đà Lạt–Jeonju–Gyeongju sáng, Seoul mở. Ba rơle Seoul dùng mã/tín hiệu từ Đà Lạt, Huế và Jeonju; kính sao Gyeongju làm sáng tỏ bí mật của hệ phòng vệ bị sửa lệnh. Cao trào không đơn giản là tiêu diệt mọi bot: **xác định lệnh giả, cứu hệ bảo vệ khi có thể**, và mở lễ hội kết thúc. Trở lại các map sau credits được.

**Unlock chuẩn từ `world_progression.json`:** H7 `hoi_an.lamp_restored` → Huế + Jeonju; U19 `hue.beacon_lit` → Đà Lạt; J19 `jeonju.beacon_lit` → Gyeongju; Seoul khi **cả** `hue.beacon_lit`, `da_lat.beacon_lit`, `jeonju.beacon_lit`, `gyeongju.beacon_lit` true. Không bật Seoul bằng fixture trong luồng chơi bình thường.

## C. HỘI AN — 22 NODE

**Mở đầu.** Lumi vừa xuống thuyền giữa đêm hội Hội An thì nhận ra một dãy đèn chớp tắt bất thường. Bác lái đò chỉ đường về tiệm Cô Mai. Cô không kết luận bot là thủ phạm: có người đang chỉnh nhịp của trạm đèn. Lumi hứa thu thập Mắt Đèn, Lõi Giấy Sáng và Chìa Kết Nối.

**Phát triển.** Trong hẻm H3, Lumi tự đi qua vật cản để nhặt Mắt Đèn; chợ H4 để lộ mật mã Hoa–Trăng–Đèn; xưởng H13 có lõi giấy. Ông Phúc H14 giải thích có người thay mã trạm bằng họa tiết gốm; H19 chứa cách hóa giải Bot Mờ-01 mà không giao chiến. H10–H12 giúp Bà Tư lấy lại dây từ cua; người chơi có thể giúp đèn lồng ma H15 thay vì tiêu diệt.

**Cao trào.** Tại H5, Bot Mờ-01 vừa chống trả vừa phát cảnh báo ngắt quãng: “Không được bật cả sáu trạm cùng...” Người chơi thắng trận hoặc dùng mã học từ H19, đều nhận Chìa Kết Nối duy nhất. H7 chỉ đồng bộ trạm giao lưu theo cơ chế an toàn, không phải Khai Sáng toàn Hội An.

**Kết chương.** Chiếc đèn trước tiệm Cô Mai đã sáng từ lúc trả Mắt Đèn; trạm H7 gửi tín hiệu giao lưu, còn nhiều phố chưa khôi phục. Cô Mai dặn Lumi ghi nhớ lời bot. Huế và Jeonju mở song song; người chơi có thể ở lại làm nhiệm vụ phụ. Ji-eun ở H18 đề xuất trao đổi văn hóa; Bà Tư nhờ mang ớt đến J15 và hành tím đến U1. Nhận thư J21 trở về giúp giải bí ẩn chiếc đèn lạc H9.


### Kịch bản từng node — phải gắn gameplay hoặc mô tả tương tác thật

| Node | Chức năng/sự kiện canonical | Câu thoại gợi ý cho playtest |
|---|---|---|
| **H0 – Bến thuyền khởi hành** | start: Lumi lên bờ tại cầu tàu, học cách đi node | Lumi: “Sông Hoài rực sáng, nhưng có vài chiếc đèn cứ chớp tắt.” |
| **H1 – Ngã ba phố đèn** | junction: Ngã rẽ đèn lồng và phố cổ | Lumi: “Bên trái là tiếng rao chợ; phía trước là tiệm đèn của Cô Mai.” |
| **H2 – Tiệm đèn Cô Mai** | npc: Nhận HQ01; trả Mắt Đèn hoàn tất tutorial mai-lantern, chỉ đèn trước tiệm sáng. HQ01 vẫn lắp ba linh kiện ở H7. | “Đèn lồng nhà cô tắt cùng lúc. Giúp cô tìm mắt đèn trong hẻm, lõi giấy ở xưởng và chìa trạm nhé!” |
| **H3 – Hẻm mái ngói** | exploration: Scene H3 có spawn P, item mắt đèn I, exit E; lấy một lần; không đi xuyên vật cản. | “Lối này hẹp, chú ý thùng gỗ và rãnh nước.” |
| **H4 – Chợ ven sông** | puzzle: Đổi vị trí 3 ô bằng tap; hoàn thành mở xưởng H13. | “Sắp ba mảnh hình theo họa tiết xuất hiện trên mái nhà.” |
| **H5 – Bến gỗ máy canh** | boss: Hai cách: giao chiến 1v1 hoặc có mã H19 để hack. Nhận link_key một lần. | “Dữ liệu bị chỉnh sửa... Đừng để sáu trạm cùng nhấp nháy!” |
| **H6 – Cầu cổ ánh đèn** | junction: Cây cầu đá làm đầu mối hai bờ | Lumi: “Từ cây cầu cổ, các đốm sáng đang tìm được nhịp chung.” |
| **H7 – Trạm đèn hội quán** | beacon: Giữ tiêu thụ/đánh dấu linh kiện và mở H8/H20; chỉ đồng bộ trạm, không bật hiệu ứng Khai Sáng toàn map. | “Ba linh kiện đã đồng bộ. Bạn muốn thắp sáng trạm không?” |
| **H8 – Cổng giao lưu Jeonju** | portal: Cổng Việt–Hàn qua mái đình nhỏ | Lumi: “Qua cổng này là Jeonju. Mình vẫn có thể quay về Hội An.” |
| **H9 – Ngõ đèn thất lạc** | secret: Ngõ ẩn sau khi nhận bản đồ thương nhân | Lumi: “Trong ngõ có một chiếc đèn che chở ai đó, chứ không phải đồ bị đánh cắp.” |
| **H10 – Quầy hàng Bà Tư** | npc: HQ02: nhận dây từ H12, trả nhận bản đồ phát hiện H9. | “Có con cua cứ kéo đồ của bà xuống mép nước. Cho bà xin lại cuộn dây!” |
| **H11 – Rương gỗ bến nước** | item: Nhặt dây đồng sau khi mở rương | Lumi: “Một chiếc rương bến nước. Mở ra chỉ nhận đồ một lần thôi.” |
| **H12 – Cua trộm dưới bờ** | enemy: Combat nhỏ 2 lượt hoặc đưa mồi để cua nhả dây. | “Kẹt kẹt! Đừng lấy kho báu của ta!” |
| **H13 – Xưởng ghép giấy** | puzzle: Chọn trong 3 mẫu giấy; dùng manh mối H4, nhận lõi giấy. | “Đèn phố nào phản chiếu đúng họa tiết ở sạp chợ?” |
| **H14 – Nghệ nhân ông Phúc** | npc: Gợi ý H19; tặng trà sau khi giúp. | “Có người đã thay mã trạm bằng hình gốm. Nhìn hoa văn ở ngõ trên.” |
| **H15 – Lối đèn lập lòe** | enemy: Không phải linh hồn tà ác; có thể dùng lõi nhỏ làm dịu hoặc combat tránh đòn. | “Một chiếc đèn yếu ớt bay vòng vòng.” |
| **H16 – Cầu tàu trung tâm** | junction: Đổi hướng sang bến gỗ | Người gác bến: “Hãy chọn nhánh bến an toàn, đừng đi xuyên mặt nước.” |
| **H17 – Bến đò phía đông** | npc: Mở shortcut sau quest HQ04; không vượt nước bằng đường bộ. | “Đưa bác năm đồng xu hoặc sửa mái chèo là qua nhánh đông được.” |
| **H18 – Tiệm trà khách lạ** | npc: Foreshadow Jeonju, mở sidequest thư hồi âm về Hội An. | “Tôi từng thấy hoa văn ấy tại làng Jeonju...” |
| **H19 – Ngõ gốm hoa** | puzzle: Giải puzzle lấy mã vô hiệu hóa H5; không bắt buộc chiến đấu. | “Sao chép chuỗi Hoa – Trăng – Đèn theo tranh gốm.” |
| **H20 – Cổng đi Huế** | portal: Mở sau khi thắp trạm Hội An | Lumi: “Cổng về phía Huế đã sáng; hành trình mới đang đợi.” |
| **H21 – Rương áp mái** | item: Hòm phụ có tiền và giấy chỉ lối | Lumi: “Có một tờ giấy cũ chỉ về ngõ đèn lạc.” |

**Dùng các `event_id` chính xác trong `Data/Cities/hoi_an.json`.** Nếu một đoạn trong bảng chỉ là mô tả tại ngã rẽ, hiển thị một lời quan sát ngắn, không tự cấp item hoặc complete quest.


### Continuity hiện hành — ba mốc ánh sáng độc lập

- Trả Mắt Đèn tại H2 hoàn tất tutorial `mai-lantern`/`lantern-lit`: chỉ chiếc đèn trước tiệm sáng. Không đồng nghĩa HQ01 hoàn tất; HQ01 vẫn H13/H5/H7 và toàn bộ điều kiện/reward/gate cũ.
- H7/`hoi_an.lamp_restored` là tín hiệu trạm giao lưu, vẫn mở Huế/Jeonju như trước; tên flag lịch sử không là trạng thái Khai Sáng toàn thành phố.
- **Hội An Khai Sáng:** thu đủ bốn mảnh H0/H4/H10/H2 → ghép Đèn Mắt Phố → sau HQ01/H7 mới hoàn tất Dẫn Ánh Đèn. Di vật không là gate mở thành phố hoặc vào boss. Phục dựng đủ sáu di vật là điều kiện của nghi thức kết thúc duy nhất ở Phase 5; không đổi HQ01 hay SQ03.
- Hội An đã có bốn tương tác thu mảnh, ghép đèn, Dẫn Ánh Đèn, overlay Khai Sáng và bưu thiếp H6 trong runtime. Nguồn duy nhất: `RelicCollection` / `CulturalRelics.json`, derive từ proof/flags/ledger hiện có. Năm di vật khác mới là dữ liệu và art, chưa có vòng chơi thu/phục dựng. Không có nguồn trạng thái di vật thứ hai.
- [Truy vết state và lời thoại runtime đã đồng bộ](HOI_AN_LIGHT_RESTORATION_CONTINUITY.md).


### Hội An — lời kể di vật đang được dùng trong runtime

Dữ liệu lời dẫn nằm ở `Assets/_Game/Resources/Content/CulturalRelics.json` → `story`; `RelicCollection.Story` chọn dòng đầu tiên có proof phù hợp. `NodeEventManager.BuildOptions` nối side dialogue sau lời nhắc quest, không thay lựa chọn. `GameplayPanels` dùng cùng nguồn cho check-in, phản ứng Lumi sau khi thực sự thu mảnh, nghi thức và bưu thiếp. Đây là nội dung presentation bổ sung theo quyết định người dùng, không phải quest mới.

| Tương tác / địa điểm | Proof thật và nội dung |
|---|---|
| H0 — Khung Tre Bến Sông | Kéo/căn/xoay rồi xác nhận riêng. Lumi ghi nhớ khung tre, không tự nhận khi đến bến. |
| H4 — Tấm Giấy Hoa Cổ | Sau puzzle chợ (hoặc proof giải chợ hợp lệ trong save cũ), còn phải căn mảnh giấy; lời dẫn phân biệt hai tương tác và không phát thưởng puzzle lại. |
| H10 — Móc Đồng Mái Hiên | HQ01 đã nhận/hoàn thành + tương tác căn hướng móc. Bà Tư vẫn nhắc cuộn dây/gia vị theo quest cũ; mảnh không thay reward HQ02. |
| H3 → H2 — Dấu Phản Chiếu Mắt Đèn | Nhặt Mắt Đèn chỉ nhắc trở về. `completed:mai-lantern` sau trả Cô Mai mới cho căn dấu phản chiếu; H2 ghi dấu đèn đầu tiên ngay sau tutorial. |
| H2 — manh mối và ghép đèn | Hướng dẫn mảnh còn thiếu chỉ gọi địa điểm đã visible. Đủ bốn mảnh cho ghép; đã ghép nhưng chưa H7 vẫn nhắc tiếp tục HQ01, không nói cả phố sáng. |
| H7 → H2 — Dẫn Ánh Đèn | HQ01 hoàn tất + `hoi_an.lamp_restored` mới cho nghi thức thành công. H7 mở Huế/Jeonju dù chưa ghép đèn. |
| Hội An Khai Sáng | Chỉ `relic.restored.relic_hoi_an_lantern` đổi lời cảm ơn Cô Mai, landmark/check-in và phản ứng Lumi. Re-entry derive trạng thái, không nhận lại thưởng. |
| H6 — bưu thiếp / lời chào | Sau H7 được căn đèn + bóng nước. Metadata giữ góc trước hoặc sau Khai Sáng; ảnh cũ không tự bị kể thành ảnh mới. Proof ảnh H6 chuẩn bị cho Con Dấu Lời Chào Jeonju ở Phase 4, chưa claim fragment Jeonju trong Phase 3. |

Thứ tự hỗ trợ: chơi HQ01 trước hay thu bốn mảnh trước đều được; cả hai không thay beacon gate. Lời dẫn không dùng `game.ending_seen` làm bằng chứng phục dựng. Không thêm world flag, reward, item hay điều kiện tiến độ cho nội dung mới.

### Hàng đợi cốt truyện Phase 4–5 — chưa triển khai ở lượt này

| Phase / thành phố | Đồng bộ bắt buộc khi được phép triển khai |
|---|---|
| 4 — Huế | Bốn mảnh theo ACCESS_MATRIX đã duyệt; Mảnh Văn Khắc Nhịp từ U17, đối chiếu G14 chỉ là giao lưu, không thêm vật phẩm thứ hai. NPC hỗ trợ/phản ứng phục dựng theo proof thật; giữ beacon U19 và tuyến Đà Lạt. |
| 4 — Đà Lạt | Bốn mảnh theo matrix; Hạt Tín Hiệu Xanh tại D12, không giữ đến Seoul. Lời dẫn, phản ứng sương/tín hiệu theo tương tác thật, giữ bridge/battle/gate cũ. |
| 4 — Jeonju | Bốn mảnh gồm nguồn H6 Hội An cho Con Dấu Lời Chào. Bảo vệ save check-in H6 cũ bằng tương tác retro-claim hợp lệ; lời giao lưu thủ công, NPC và phục dựng. Không buộc HQ05/HQ07/DQ05 vào main quest. |
| 4 — Gyeongju | Bốn mảnh theo matrix; `guard_resolved` công nhận sửa hoặc đánh tượng, cần tương tác nhận mảnh riêng. Presentation khác theo nhánh, không thêm quest branch/ending. |
| 4 — Seoul (phục dựng) | Giữ bốn milestone/fragment canonical. Ba icon sai nghĩa vẫn ART_PENDING, không dùng ảnh tín hiệu thay bừa. NPC kỹ thuật chưa được duyệt mặt không tự thành VISUAL_APPROVED. |
| 5 — Seoul (hội tụ) | Dialogue readiness, sau boss còn thiếu mảnh, tiếp tục tìm/không đánh lại; SQ03 vẫn xác nhận tại S21. Nghi thức chủ động sau boss + 6 restored, một ending/credits. Persist completion riêng an toàn với v1/v2, không tự công nhận credits cũ; bảo vệ rewards/choices/boss result. |

Tất cả năm chương phải dùng đúng bốn fragment trong `ARTIFACT_FRAGMENT_ACCESS_MATRIX.md`, NPC canonical và quest/beacon cũ; không tự viết lại quest graph. Chưa thực hiện các thay đổi này trong Phase 3.

## C. HUẾ — 22 NODE

**Mở đầu.** Lumi đến bờ nam sông Hương và gặp Chị Hương ở chợ U1. Một kiện thư có dấu đỏ bị máy chuyển thư giữ lại; học giả Lê An ở U7 cần nội dung thư để đọc ký tự trong Hoàng thành. Bot canh cổng nhầm tín hiệu thật với giả, khiến tuyến qua sông tắc nghẽn.

**Phát triển.** Người chơi giải câu đố biểu tượng ở sạp U16 lấy Thẻ Qua Sông, gặp bác lái đò Tâm U3, vượt nước bằng cầu/đò hợp lệ. Có thể giúp Bà Nguyệt U2 tìm thảo dược U10, làm dịu người chèo đò. Ở U15, cứu kiện thư bằng thắng bot hoặc dùng mẹo thư giả; gặp học giả U7, giải thư khố U17 (nhịp ngắn–dài–ngắn), rồi giải ba lá cờ tại U8. U11 giữ Ấn Tín Cổ khắc nét sao — vật chứng để dùng về sau tại G12 và S17, KHÔNG phải chìa mở tất cả cổng.

**Cao trào.** Bot Thân Vệ U5 phát hiện tín hiệu giả; Lumi có thể chiến đấu hoặc vô hiệu hóa khi đủ manh mối hợp lệ, không được buộc có ấn tín nằm phía sau chính cổng chưa vượt. Chỉ khi cứu thư, hiểu mật mã, vượt gate mới có thể đồng bộ trạm U19.

**Kết chương.** Huế sáng hai bờ, `hue.beacon_lit` mở Đà Lạt. Chị Hương nhận một củ Hành tím Hội An ở U1, viết giấy hồi âm `hue_market_note` để Lumi mang về cho Bà Tư; giao dịch độc lập với nhiệm vụ kiện thư U1.


### Kịch bản từng node — phải gắn gameplay hoặc mô tả tương tác thật

| Node | Chức năng/sự kiện canonical | Câu thoại gợi ý cho playtest |
|---|---|---|
| **U0 – Bến sông phía phố** | start: Điểm Lumi tới bờ nam sông Hương | Lumi: “Hai bờ sông đẹp quá, nhưng đường sang thành đang bị khóa.” |
| **U1 – Chợ xưa bờ nam** | npc: Giao nhiệm vụ chuyển kiện thư | Chị Hương: “Bưu kiện thư đỏ bị kẹt ở máy U15. Còn món quà của Bà Tư, tôi sẽ nhận khi cháu xác nhận.” |
| **U2 – Nhà thuốc Bà Nguyệt** | npc: Lấy royal_herb tại U10 và trả U2. | “Thuốc cho người chèo đò sắp hết. Con tìm lá bên vườn phía bắc giúp bà nhé.” |
| **U3 – Bến đò bờ nam** | npc: Nhận thẻ U16. boat animation U3–U4 chỉ thực hiện khi mở. | “Muốn qua sông con cần thẻ đò ở chợ, hoặc giúp ta sửa thuyền.” |
| **U4 – Đầu cầu vào thành** | junction: Đầu cầu đi qua sông Hương | Lumi: “Cầu chỉ đi được khi thẻ qua sông đã được chấp nhận.” |
| **U5 – Bot Thân vệ Cổng** | boss: Thắng combat hoặc dùng ancient_seal + mật mã scholar để hack. | “Cổng đang khóa vì một tín hiệu giả.” |
| **U6 – Cửa Ngọ Môn cách điệu** | junction: Cổng kiến trúc là điểm giao nhiệm vụ | Lumi: “Cửa thành đã ở trước mắt. Manh mối của học giả chắc ở phía trên.” |
| **U7 – Nho sinh Lê An** | npc: Nhận UQ01; thưởng manh mối sao cho Gyeongju. | “Bức thư cũ không thể đọc thiếu ký hiệu ở thư khố. Xin nhờ bạn.” |
| **U8 – Sân điện chính** | puzzle: Puzzle tap 3 lá cờ, không trắc nghiệm lịch sử. | “Ba lá cờ, ba luồng sáng: xếp theo bóng đổ trên sân.” |
| **U9 – Vườn phía tây** | junction: Đường dẫn ra thảo dược | Lumi: “Cây thuốc Bà Nguyệt cần nằm phía vườn bắc.” |
| **U10 – Vườn thuốc bờ bắc** | item: Nhặt royal_herb một lần. | “Mùi thảo dược nhẹ bên rìa vườn.” |
| **U11 – Gốc cây dấu ấn** | item: Nhặt ancient_seal, G12/S17 dùng flag sở hữu. | “Trên ấn có ba nét khắc như chòm sao.” |
| **U12 – Bến sông tây** | enemy: Nhiễu sáng xuất hiện ven bờ | Lumi: “Nhiễu đèn chớp gần bến. Có thể xử lý mà không phá cảnh quan.” |
| **U13 – Bến thuyền hẻm** | secret: Bến ẩn dẫn tới hòm ghi chép | Lumi: “Một bến hẻm kín; chỗ này có bản ghi chuyến thuyền cũ.” |
| **U14 – Cầu ngõ phía đông** | junction: Cầu nhỏ nối nhánh đường dân cư | Lumi: “Qua cầu ngõ mới đến được lối tường phía đông.” |
| **U15 – Máy thu thư** | enemy: Thắng hoặc đưa thư giả qua câu đố U16 để nhận lost_letter. | “Tôi được lệnh giữ kiện giấy có dấu đỏ.” |
| **U16 – Sạp mật mã chợ** | puzzle: Chọn đúng 2/3 biểu tượng nhận river_pass. | “Tìm số đèn có cùng nét khắc trên tấm vải.” |
| **U17 – Thư khố Hoàng thành** | puzzle: Đổi thành nhịp trống U8, lưu hint cho G12. | “Mã ánh sáng có nhịp ngắn – dài – ngắn.” |
| **U18 – Sân thử thách Hoàng môn** | enemy: Bot phụ canh cơ cấu cổ | Bot Hoàng Môn: “Cơ cấu cổ đang ở chế độ bảo vệ. Vui lòng xác minh.” |
| **U19 – Đài đèn thành nội** | beacon: Cần thư, puzzle điện, bot gate; bật hue.beacon_lit. | “Trạm được đồng bộ từ hai bờ.” |
| **U20 – Cổng dẫn Đà Lạt** | portal: Mở nhánh cao nguyên | Lumi: “Tín hiệu từ Huế đã mở tuyến cao nguyên.” |
| **U21 – Lối đi ẩn bên tường** | secret: Chứa Mảnh Chữ Sao cho Gyeongju | Lumi: “Những nét sao này có thể dùng tại di tích Hàn Quốc.” |

**Dùng các `event_id` chính xác trong `Data/Cities/hue.json`.** Nếu một đoạn trong bảng chỉ là mô tả tại ngã rẽ, hiển thị một lời quan sát ngắn, không tự cấp item hoặc complete quest.

## C. ĐÀ LẠT — 22 NODE

**Mở đầu.** Sương phủ hồ Xuân Hương; người dẫn đường Lan D2 mất kính nên không thể hướng dẫn du khách. Một chú chó đói ở D10 canh lối gần cầu đá. Lumi nhận lời giúp mọi người trước khi tìm nguồn sương.

**Phát triển.** Tới D7 đối mặt Bot Giấu Kính, có thể giao chiến hoặc dùng bẫy ánh sáng; chỉ khi TRẢ kính cho Lan D2 mới ghi `da_lat.glasses_returned`. D8 là ba lối sương cần ghép theo bản vẽ của Lan. Các phần thức ăn từ D9, D11, D12 (có nguồn thay thế ở D3) phải thu gom thật để cho chó D10; khi ăn no chó giúp chỉ đường D15 và mở lối bí mật D17. D13 giúp nhà vườn tìm hoa nở đêm D19.

**Cao trào.** Bóng Sương D15 trông hung dữ nhưng thực chất là tín hiệu bảo vệ cầu/thác đã tự nhân bản. Có thể giảm khó nếu giúp Lan và chó; không viết lại boss nếu combat đã được duyệt. Sau giải quyết D15, lấy tín hiệu xanh `blue_water_signal` tại thác D16, đồng bộ đài D14.

**Kết chương.** Đường và cây hiện rõ khi sương tan; rơle nguồn A của Seoul được cung cấp, nhưng cổng D21 chỉ GỬI TÍN HIỆU, không cho vào Seoul trước khi đủ điều kiện toàn cầu.


### Kịch bản từng node — phải gắn gameplay hoặc mô tả tương tác thật

| Node | Chức năng/sự kiện canonical | Câu thoại gợi ý cho playtest |
|---|---|---|
| **D0 – Bến hồ Xuân Hương** | start: Điểm Lumi tới khu hồ | Lumi: “Mặt hồ mờ như có màn sương giăng. Mình hỏi người dẫn đường trước.” |
| **D1 – Quảng trường dốc** | junction: Quảng trường nhà cửa ven hồ | Lumi: “Mỗi con đường dốc dường như đều khuất trong sương.” |
| **D2 – Người dẫn đường Lan** | npc: Nhận DQ01; sau trả kính thì nút D8 bớt sương, mở hướng dẫn. | “Không có kính, chị chẳng đọc được cột chỉ dẫn. Tìm giúp chị nhé.” |
| **D3 – Tiệm hoa Thủy** | npc: Một phần thịt dự phòng thay thế cho nguồn tại D12 nếu puzzle khó. | “Bác chó thích thức ăn; chị có phần dự phòng đổi một bông hoa.” |
| **D4 – Chợ trên đồi** | npc: Người bán có manh mối thú hoang | Người bán chợ: “Chú chó ở đầu cầu không dữ đâu, chỉ đói và sợ thôi.” |
| **D5 – Lối rừng thông** | junction: Đường mòn lên rừng | Lumi: “Tiếng gió đi qua rừng thông sẽ giúp mình định hướng.” |
| **D6 – Sói sương mù** | enemy: Combat tự chọn hoặc dùng đèn soi để né; tránh grind. | “Một sinh vật được sương tạo thành.” |
| **D7 – Trạm quan sát sương** | enemy: Combat 1v1 hoặc bẫy ánh sáng tương tác 2 lần; nhận lan_glasses. | “Nó cầm cặp kính và chạy giữa rừng.” |
| **D8 – Ngõ sương vòng** | puzzle: Puzzle đường rẽ 3 cổng, mở D9. | “Chọn cột đèn đúng theo bản phác thảo của Lan.” |
| **D9 – Đài nhìn thông** | item: meat_portion +1, fog_battery. | “Trong rương có một gói đồ ăn và pin cũ.” |
| **D10 – Chó canh đói** | npc: Đưa 3 meat_portion, chó né đường và trở thành NPC trợ giúp. | “Gừ... bụng đói, đừng lại gần.” |
| **D11 – Lối vườn hoa** | item: meat_portion +1. | “Rương ở bên luống hoa tím.” |
| **D12 – Cối xay cạnh hồ** | puzzle: Cơ chế xoay 3 hướng, meat_portion +1. | “Xoay cánh theo hướng gió trên ba ngọn thông.” |
| **D13 – Vườn kính hoa** | npc: Sidequest DQ04, phát hiện D19. | “Có hoa chỉ xuất hiện sau sương...” |
| **D14 – Đài dẫn sáng núi** | beacon: Cần fog_cleared + blue_water_signal, bật trạm. | “Ánh sáng đẩy lùi sương mù.” |
| **D15 – Bóng sương cầu đá** | boss: Có thể giảm độ khó nếu giúp Lan và chó; chiến thắng nhận lối D16. | “Ta chỉ giữ đường tránh người rơi xuống thác.” |
| **D16 – Thác hồi âm** | item: Nhặt blue_water_signal cho Seoul S16. | “Mặt nước phản ánh một mã xanh.” |
| **D17 – Lối bí mật bờ đông** | secret: Đường bí mật sau khi cho chó ăn | Lumi: “Chú chó đã chỉ mình một lối bờ đông an toàn.” |
| **D18 – Nhà nghỉ trên dốc** | npc: Hồi phục và dẫn đường | Chủ nhà nghỉ: “Nghỉ một chút nhé, đừng đi khi sương chưa tan.” |
| **D19 – Vườn hoa đêm** | secret: Chỉ hiện sau khi xua sương | Lumi: “Hoa chỉ nở sau khi sương tan — mang về cho người làm vườn.” |
| **D20 – Cầu dẫn hồ** | junction: Cầu nối bờ đông | Lumi: “Cầu hồ nối sang lối thác; phải đi đúng bờ.” |
| **D21 – Cửa đồng bộ Seoul** | portal: Chỉ gửi tín hiệu, không vào Seoul ngay | Lumi: “Đây là điểm gửi nguồn về Seoul, chưa phải lối tới thành phố ấy.” |

**Dùng các `event_id` chính xác trong `Data/Cities/da_lat.json`.** Nếu một đoạn trong bảng chỉ là mô tả tại ngã rẽ, hiển thị một lời quan sát ngắn, không tự cấp item hoặc complete quest.

## C. JEONJU — 22 NODE

**Mở đầu.** Jeonju chuẩn bị hội đèn Hanji nhưng khuôn hoa văn bị chia thành ba mảnh; một kiện hàng biến mất. Ji-eun đã kể cho mọi người về Lumi và đèn Hội An. Ở J15, So-yeon có gian hàng trao đổi ẩm thực, đồng thời vẫn bán đồ hồi phục như thiết kế gốc.

**Phát triển.** Hana J2 cần ba hoa văn: J7 (gấp giấy), J6 (kiện do bot giữ), J11 (sau khi trả kiện cho Park J5). Thầy Yoon J4 gợi ý thứ tự; J12 ghép được `hanji_circuit`. J17 dùng manh mối từ trạm Hội An để xoay ba vòng đèn. Bot J14 có thể được xử lý bằng hoa văn hoàn chỉnh hoặc chiến đấu. Ở J16 người giữ chuyện kể về người lạ và thư bị kẹt; J21 lấy `reply_letter` để mang ngược tới Ji-eun H18/H9.

**Cao trào.** J18 Hạc Giấy Tối bảo vệ thư; người chơi chọn giải bùa hoặc đánh nếu tài liệu cho phép, không để nhánh này chặn cốt truyện chính vô lý. Trạm J19 được thắp khi ghép đủ hoa văn và giải J17.

**Kết chương.** `jeonju.beacon_lit` mở Gyeongju và đưa mạch giấy Hanji về Seoul. Tại J15, Lumi trao một trái Ớt Hội An theo xác nhận, nhận `jeonju_recipe_card`; cả nhiệm vụ HQ05 và cửa hàng J15 đều còn hoạt động.


### Kịch bản từng node — phải gắn gameplay hoặc mô tả tương tác thật

| Node | Chức năng/sự kiện canonical | Câu thoại gợi ý cho playtest |
|---|---|---|
| **J0 – Cổng làng phía nam** | start: Lumi tới Jeonju qua cổng giao lưu | Lumi: “Mái nhà Hanok và ánh đèn giấy đẹp thật. Không biết lễ hội gặp chuyện gì?” |
| **J1 – Ngã ba hanok** | junction: Quảng trường cây phong và tiểu cảnh | Lumi: “Ba lối nhỏ giữa những mái ngói. Mình phải tìm nghệ nhân Hana.” |
| **J2 – Nghệ nhân Hana** | npc: Nhận JQ01, mở J12 sau đủ 3 mảnh. | “Một tấm khuôn bị chia ba trước lễ hội. Giúp tôi tìm lại nhé.” |
| **J3 – Xưởng nhuộm vải** | puzzle: Khôi phục sắc hoa văn | Thợ nhuộm: “Sắc hoa trên tấm vải đã lệch. Giúp tôi chọn lại màu theo mẫu đi.” |
| **J4 – Nhà thầy Yoon** | npc: Đọc hướng dẫn chuỗi họa tiết | Thầy Yoon: “Hoa văn hanji cần nhớ thứ tự ánh sáng, không chỉ hình dáng.” |
| **J5 – Tiệm hàng chuyển** | npc: Nhận JQ02; thắng/đàm phán bot J6 để lấy kiện. | “Kiện hàng bên suối biến mất rồi!” |
| **J6 – Bot Cắp Kiện** | enemy: Thắng 1v1 hoặc giải mật mã kho, thả parcel và hanji_pattern_2. | “Kiện hàng đã vào danh mục kiểm kê lỗi.” |
| **J7 – Thư viện giấy hanji** | puzzle: 2 lần xoay/1 lần gấp, nhận hanji_pattern_1. | “Gấp tờ giấy theo vạch sáng thành bông hoa.” |
| **J8 – Cầu đá tây** | junction: Lối qua suối hẹp | Lumi: “Cầu đá nhỏ dẫn qua suối; mình không thể đi đường tắt trên mặt nước.” |
| **J9 – Bờ suối nước chảy** | enemy: Hạc giấy nhiễu canh đường | Lumi: “Một hạc giấy nhiễu đang bay thấp. Nó hình như chỉ muốn giữ chiếc chuông.” |
| **J10 – Rương ven hồ** | item: Rương có sáp và tiền | Lumi: “Rương có ít sáp làm đèn và vài đồng xu.” |
| **J11 – Đài ngắm mái ngói** | item: Chỉ nhặt sau khi trả kiện cho Park, nhận hanji_pattern_3. | “Trong hộp kính là mảnh hoa văn thứ ba.” |
| **J12 – Sân thêu họa tiết** | puzzle: Cần 3 mảnh, chọn đúng thứ tự theo hint J4/J17. | “Ghép 3 hoa văn, đặt tâm đèn vào vị trí đèn Hội An.” |
| **J13 – Cổng sân cổ** | junction: Điểm hội tụ đường phía bắc | Lumi: “Cổng sân cổ dẫn về phía đài trưng bày.” |
| **J14 – Bot Hoa Văn** | enemy: Có thể dùng hoa văn đã ghép vô hiệu hóa hoặc combat. | “Sân thêu bị khóa từ mã khác.” |
| **J15 – Chợ hanok** | npc: Bán vật phẩm hồi phục | So-yeon: “Ji-eun có báo trước về ớt Hội An. Cháu muốn gửi món quà hay xem quầy hàng trước?” |
| **J16 – Người giữ chuyện** | npc: Kể về cổng từ Hội An | Người giữ chuyện: “Có một lá thư gửi về phố cũ, người nhận đang đợi dưới mái đèn.” |
| **J17 – Đèn song thành** | puzzle: Dựa vào hoi_an.lamp_restored; xoay 3 vòng đèn, mở trạm J19. | “Một nửa họa tiết chỉ thấy khi trạm giao lưu Hội An gửi tín hiệu.” |
| **J18 – Hạc giấy bóng tối** | boss: Boss tùy chọn có thư hồi âm quan trọng, hoặc giải bùa bảo vệ. | “Đừng để ta tan trước khi gửi thư!” |
| **J19 – Trạm ánh sáng hanji** | beacon: Cần 3 hoa văn + đèn J17; mở Gyeongju. | “Sợi giấy nối hai bờ biển qua ánh đèn.” |
| **J20 – Cổng đi Gyeongju** | portal: Mở đường di tích Silla | Lumi: “Jeonju đã sáng. Bây giờ có thể đi đến Gyeongju.” |
| **J21 – Lối thư hồi âm** | secret: Lấy reply_letter cho H18, bật ngõ H9. | “Người lạ nhờ gửi tin về Hội An.” |

**Dùng các `event_id` chính xác trong `Data/Cities/jeonju.json`.** Nếu một đoạn trong bảng chỉ là mô tả tại ngã rẽ, hiển thị một lời quan sát ngắn, không tự cấp item hoặc complete quest.

## C. GYEONGJU — 22 NODE

**Mở đầu.** Tại thành phố di sản Silla, những ký hiệu sao bị nhiễu, tượng đá bỗng chuyển động. Hướng dẫn viên Min G3 không tin rằng các bức tượng tự dưng trở nên ác: chúng đang phản ứng với một lõi lỗi.

**Phát triển.** Lumi tìm ba biểu tượng tại G5/G6/G10, xếp lên đài G4 để nhận `stargazer_lens`. G6 có thể xử lý cáo bóng bằng đồ ăn thay vì chiến đấu. Bệ G12 cần Ấn Tín Cổ từ Huế và nhịp đã đọc ở thư khố U17, qua đó nối trực tiếp hai nhánh quốc gia. Tượng G7 nếu được sửa theo G12 sẽ thành đồng minh giúp đối phó lõi nhiễu; nếu chiến đấu thì phải lưu trạng thái khác nhau, không cấp cờ kết bạn một cách vô điều kiện.

**Cao trào.** Lõi sao G18 đánh theo chu kỳ ba nhịp, khiên mở bằng ký hiệu đúng; nhận `silla_prism`. Trạm G19 chỉ bật khi đã xếp sao và xử lý lõi. G17 là nhánh phụ kết nối ánh đèn Hanji Jeonju.

**Kết chương.** `gyeongju.beacon_lit` cùng trạm Huế, Đà Lạt, Jeonju cho phép mở Seoul. Người chơi hiểu rằng nhiều cơ cấu canh gác là hệ bảo vệ bị lệnh giả thao túng chứ không hẳn kẻ thù.


### Kịch bản từng node — phải gắn gameplay hoặc mô tả tương tác thật

| Node | Chức năng/sự kiện canonical | Câu thoại gợi ý cho playtest |
|---|---|---|
| **G0 – Cổng vào làng di tích** | start: Lumi bắt đầu giữa mái nhà đá | Lumi: “Những mái nhà cũ và gò đất đang giữ bí mật về các vì sao.” |
| **G1 – Cầu lối bắc** | junction: Cầu nối từ làng tới di tích | Lumi: “Cây cầu đá dẫn vào vùng di sản. Mình theo đường đã mở thôi.” |
| **G2 – Đình ao sen** | puzzle: Quan sát phản chiếu để lấy manh mối | Lumi: “Hình phản chiếu ở ao sen khác nét khắc phía trên. Có lẽ là gợi ý.” |
| **G3 – Nhà khảo cổ Min** | npc: Nhận GQ01, có hình minh họa không yêu cầu kiến thức ngoài. | “Ba hình sao là chìa khóa để hiểu đài quan sát.” |
| **G4 – Đài Cheomseongdae cách điệu** | puzzle: Cần 3 symbols; nhận stargazer_lens. | “Đặt ba nét sao theo tia phản chiếu.” |
| **G5 – Gò mộ phía tây** | item: Nhặt star_symbol_1. | “Một ký hiệu mờ ẩn dưới gốc cây.” |
| **G6 – Đường giữa các gò** | enemy: Hạ hoặc dụ bằng đồ ăn để nhận symbol 2. | “Cáo chạy quanh gò đất cùng tấm bia rơi.” |
| **G7 – Tượng đá gác lối** | enemy: Đàm phán sau câu đố G12 hoặc chiến đấu; kết quả hỗ trợ boss. | “Ta bảo vệ người khám phá khỏi sai lầm của ánh sáng.” |
| **G8 – Lối chùa đá** | junction: Đường dẫn quần thể chùa | Lumi: “Lối chùa đá mở thêm hướng tới khu bệ cổ.” |
| **G9 – Sân chính di tích** | npc: Người giữ di tích đưa gợi ý | Người giữ di tích: “Đừng hoảng khi tượng chuyển động; chúng đang cố ngăn điều tồi tệ hơn.” |
| **G10 – Đình trên đồi** | item: Nhặt star_symbol_3. | “Từ đây quan sát được mẫu đèn thứ ba.” |
| **G11 – Vườn đá trung tâm** | junction: Đường rẽ ba hướng | Lumi: “Ba hướng từ vườn đá. Cần một tuyến chính rõ để khỏi lạc.” |
| **G12 – Bệ phong ấn nứt** | puzzle: Cần ancient_seal; nhập nhịp ký hiệu đã đọc U17. | “Dấu này giống ấn tín mang từ Huế.” |
| **G13 – Lính canh đêm** | enemy: Bot canh đài thắp sáng | Bot Canh: “Cổng sao chưa được xác thực.” |
| **G14 – Kho bản khắc** | npc: NPC bổ trợ giải câu đố biểu tượng | Người giữ kho: “Trong bản khắc có cách đặt ba ký hiệu, cháu xem nhé.” |
| **G15 – Thềm điện đá** | puzzle: Sắp thứ tự ba bóng đổ | Lumi: “Ba bóng đổ khác nhau; thứ tự có trong tranh cạnh bệ đá.” |
| **G16 – Đền dưới hồ** | item: Rương hồi phục, cờ cứu hộ | Lumi: “Rương cứu hộ vẫn còn đồ cho người qua đêm.” |
| **G17 – Đài lồng đèn bờ tây** | puzzle: Sidequest cần jeonju.beacon_lit, nhận xu & bản vẽ. | “Các ký hiệu hanji làm sáng phần bờ tây.” |
| **G18 – Lõi canh tinh tú** | boss: Boss có nhịp 3 lượt, khiên mở sau tap sao đúng; nhận silla_prism. | “Dữ liệu sao đang quay đảo...” |
| **G19 – Trạm cộng hưởng Silla** | beacon: Bật trạm cần three_stars_placed + star_core_disabled. | “Ánh sao nối đường đến Seoul.” |
| **G20 – Cổng đi Seoul** | portal: Hiện sau hai nhánh Việt–Hàn | Lumi: “Đến Seoul khi hai nhánh đã hoàn tất, không đi sớm.” |
| **G21 – Lối cỏ dược liệu** | secret: Rương phụ dưới gốc cây | Lumi: “Trong đám cỏ có một túi thuốc cứu hộ.” |

**Dùng các `event_id` chính xác trong `Data/Cities/gyeongju.json`.** Nếu một đoạn trong bảng chỉ là mô tả tại ngã rẽ, hiển thị một lời quan sát ngắn, không tự cấp item hoặc complete quest.

## C. SEOUL — 23 NODE

**Mở đầu.** Seoul nhận đồng thời tín hiệu từ các thành phố nhưng bot nhận diện nhầm bạn–thù. Quản thủ S2, hồ sơ S4 và kỹ thuật viên Ji-won S7 lần theo hệ thống phòng vệ cũ.

**Phát triển.** S16 rơle A dùng `blue_water_signal` từ Đà Lạt; S17 rơle B dùng `ancient_seal` và trạm Huế; S18 rơle C dùng `hanji_circuit` và trạm Jeonju. Cả ba phải được giải bằng puzzle riêng, bật `seoul.relay_a_on/b_on/c_on`. Người chơi dùng `stargazer_lens` Gyeongju tại S10 hoặc giải bằng manh mối tương đương. S2→S3→S11 làm rõ một bộ phận phòng vệ bị sửa lệnh; Bot Mờ-01 Hội An từng cố cảnh báo nhưng tín hiệu bị cắt.

**Cao trào.** S19 Bot Đưa Tin có thể được ngắt nguồn bằng ba rơle hoặc đánh. S20 Lõi Nhiễu có hai pha: pha 1 phá khiên với rơle, pha 2 lựa chọn Đánh/Sửa/Che chắn theo dữ liệu thiết kế; người chơi không bị softlock vì lựa chọn sai hoặc thiếu vật phẩm vô cớ. Lời lõi: “Lệnh ban đầu của ta là bảo vệ những chiếc đèn...” Phản diện đã lợi dụng lệnh giả nhằm chia cắt lòng tin; không đánh đồng mọi bot với phản diện.

**Kết truyện canonical — Phase5 implementation.** Kết quả boss (đánh hoặc sửa) và xác nhận SQ03 vẫn độc lập tại S21. Chỉ sau boss resolved + sáu di vật đã phục dựng + người chơi kích hoạt nghi thức hội tụ Seoul hợp lệ mới có ending/credits duy nhất. Thắng boss mà thiếu di vật vẫn tiếp tục khám phá, không đánh boss lại. `game.ending_seen`, rewards, choices và credits cũ là lịch sử; không tự xem chúng là hoàn tất nghi thức mới. Legacy S21 nay bị chặn bởi guard Phase5 ở cả backend/view; game.ending_seen vẫn chỉ là lịch sử, canonical completion dùng flag mới. S22/SQ04 là đoạn thông tin bí mật, không phải ending thứ hai.


### Kịch bản từng node — phải gắn gameplay hoặc mô tả tương tác thật

| Node | Chức năng/sự kiện canonical | Câu thoại gợi ý cho playtest |
|---|---|---|
| **S0 – Bến công viên sông Hàn** | start: Đặt chân đến Seoul | Lumi: “Tín hiệu từ nhiều nơi đổ về đây. Vì sao các bot lại hoảng loạn?” |
| **S1 – Quảng trường trung tâm** | junction: Trung tâm chuyển tuyến sang cũ và mới | Lumi: “Quảng trường nối cả phố cổ và khu cao ốc. Ba rơle nằm ở những nhánh khác nhau.” |
| **S2 – Cổng thành xưa** | npc: Mở SQ02, gợi ý xem bản ghi lịch sử S4. | “Điều nguy hiểm không chỉ là kẻ phá hoại, mà là mã bảo vệ cũ.” |
| **S3 – Sân cung điện** | puzzle: Đọc lời nhắn từ Huế | Lumi: “Những nét khắc ở cung điện trông giống bức thư Huế.” |
| **S4 – Hồ sơ lịch sử** | item: Tấm bản ghi mối liên hệ Huế | Lumi: “Bản ghi này nói hệ bảo vệ từng được dựng để giữ đèn khỏi quá tải.” |
| **S5 – Drone trinh sát** | enemy: Đánh hoặc lẩn vào vườn đá, mở tuyến cao ốc. | “Phát hiện ánh sáng không xác thực.” |
| **S6 – Bờ đông sông Hàn** | junction: Điểm rẽ trạm năng lượng | Lumi: “Sang bờ đông cần theo cầu hoặc bến phà hợp lệ.” |
| **S7 – Quảng trường màn hình** | npc: Nhận SQ01; chỉ 3 rơle và điều kiện boss. | “Hãy mang về ba tín hiệu từ Việt Nam và Jeonju.” |
| **S8 – Đường cao ốc** | enemy: Bot trộn dữ liệu trên lối chính | Bot Đường Phố: “Dữ liệu nhận diện ánh sáng đang xung đột.” |
| **S9 – Đường lên N Seoul Tower** | junction: Lối leo đồi có cây hoa | Lumi: “Tòa tháp ở trên đồi, không có lối bay tắt.” |
| **S10 – Chân tháp N Seoul** | puzzle: Cần stargazer_lens hoặc đúng mẫu ký hiệu, làm sáng đỉnh tháp. | “Ba nguồn sáng luân phiên soi chiếc ống kính.” |
| **S11 – Trạm kiểm soát kỹ thuật** | npc: Giải thích twist, mở lựa chọn sửa AI thay vì xóa. | “Bot ấy không được sinh ra để đánh nhau.” |
| **S12 – Đảo công viên xanh** | item: Rương đồng xu và bộ hồi phục | Lumi: “Mình lấy vật phẩm tiếp sức trước khi giải ba rơle.” |
| **S13 – Cầu ánh sáng phía tây** | junction: Cầu nối vùng cổ và đô thị | Lumi: “Cầu ánh sáng nối đường cũ với đường mới.” |
| **S14 – Bến phà thông tin** | npc: Chuyên chở qua sông khi hệ thống sáng | Bác lái phà: “Chỉ vận hành khi đèn tín hiệu được bật.” |
| **S15 – Bot bảo vệ cầu** | enemy: Combat hoặc dùng ancient_seal mở nhanh. | “Mã cầu đã hết hạn.” |
| **S16 – Trạm phát điện bờ đông** | puzzle: Cần blue_water_signal; puzzle xoay 2 công tắc. | “Nguồn nước Đà Lạt ổn định xung số 1.” |
| **S17 – Rơle cung điện** | puzzle: Cần ancient_seal + hue.beacon_lit; puzzle 3 nhịp. | “Nhịp từ ấn tín Huế phù hợp cổng số 2.” |
| **S18 – Rơle ánh đèn hanji** | puzzle: Cần hanji_circuit + jeonju.beacon_lit; puzzle 3 biểu tượng. | “Mạch giấy Jeonju truyền xung số 3.” |
| **S19 – Đường hầm dữ liệu** | enemy: Thắng hoặc dùng 3 rơle ngắt nguồn, mở S20. | “Lệnh gửi đi từ lõi đã bị kẻ xấu sửa.” |
| **S20 – Buồng Lõi Nhiễu** | boss: Pha 1 phá khiên bằng rơle; pha 2 3 lựa chọn đánh/sửa/che chắn; tránh soft-lock. | “Lệnh ban đầu của ta là bảo vệ những chiếc đèn...” |
| **S21 – Lễ hội hai nền văn hóa** | epilogue: xác nhận boss/SQ03 độc lập; boss + 6 restored + popup xác nhận + Lễ Kết Nối hoàn tất mới mở một ending/credits; có Cancel/Skip/replay hợp lệ. | “Mỗi ánh đèn là một câu chuyện; mỗi người bạn giúp là một nhịp cầu.” |
| **S22 – Kính quan sát đồi** | secret: Bí mật đọc tin nhắn của Hội Bóng Mờ | Lumi: “Lời nhắn bí mật xác nhận hệ phòng vệ đã từng cố cảnh báo mình.” |

**Dùng các `event_id` chính xác trong `Data/Cities/seoul.json`.** Nếu một đoạn trong bảng chỉ là mô tả tại ngã rẽ, hiển thị một lời quan sát ngắn, không tự cấp item hoặc complete quest.

## D. TOÀN BỘ QUEST THEO DỮ LIỆU CANONICAL (24 QUEST)

| Quest | Nhận | Nhiệm vụ theo thứ tự (ID canonical) | Flag hoàn thành và thưởng |
|---|---|---|---|
| **HQ01 – Đèn phố tối** | hoi_an:H2 | `find:lamp_eye@H3 → find:hanji_core@H13 → find:link_key@H5 → activate:H7` | `hoi_an.lamp_restored` — 150 xu, mở Huế & Jeonju |
| **HQ02 – Hàng của Bà Tư** | hoi_an:H10 | `resolve:enemy_crab@H12 → deliver:copper_wire@H10` | `hoi_an.ba_tu_helped` — bản đồ ngõ ẩn H9 |
| **HQ03 – Con đèn lạc** | hoi_an:H18 | `find:reply_letter@J21 → interact:H9` | `hoi_an.lost_lantern_rescued` — bạn đồng hành Đèn Lồng Nhỏ |
| **HQ04 – Đò qua nhánh nước** | hoi_an:H17 | `collect:coin*5 → talk:H17` | `hoi_an.ferry_available` — đường tắt H17–H5 |
| **UQ01 – Dấu ấn hai bờ** | hue:U7 | `get:lost_letter@U15 → solve:U17 → solve:U8 → activate:U19` | `hue.beacon_lit` — mở Đà Lạt |
| **UQ02 – Thảo dược Bà Nguyệt** | hue:U2 | `find:royal_herb@U10 → deliver:royal_herb@U2` | `hue.nguyet_helped` — trà hồi phục x3 và thẻ bờ bắc |
| **UQ03 – Qua sông an toàn** | hue:U3 | `solve:U16 → use:river_pass@U3` | `hue.crossing_permit` — mở cầu U4–U5 |
| **UQ04 – Tấm ấn gửi đi xa** | hue:U11 | `find:ancient_seal@U11 → talk:U7` | `hue.star_script_known` — Gyeongju G12 và Seoul S17 đổi trạng thái |
| **DQ01 – Kính giữa sương** | da_lat:D2 | `defeat:enemy_glass_thief@D7 → return:lan_glasses@D2 → solve:D8` | `da_lat.fog_cleared` — mở đường sương D8 và đèn núi |
| **DQ02 – Bạn chó canh đường** | da_lat:D10 | `collect:meat_portion*3@D9,D11,D12 → give:meat_portion*3@D10` | `da_lat.dog_fed` — mở D10–D15, không đánh thú |
| **DQ03 – Tín hiệu thác nước** | da_lat:D16 | `defeat:enemy_fog_boss@D15 → find:blue_water_signal@D16 → activate:D14` | `da_lat.beacon_lit` — mã rơle Seoul A |
| **DQ04 – Hoa trong mây** | da_lat:D13 | `find:night_flower@D19 → deliver:night_flower@D13` | `da_lat.night_flower_done` — hồi phục và vật phẩm hỗ trợ |
| **JQ01 – Hoa văn ba miền** | jeonju:J2 | `get:hanji_pattern_1@J7 → get:hanji_pattern_2@J6 → get:hanji_pattern_3@J11 → solve:J12` | `jeonju.three_patterns_joined` — mạch đèn hanji |
| **JQ02 – Kiện hàng thất lạc** | jeonju:J5 | `resolve:enemy_parcel_bot@J6 → return:missing_parcel@J5` | `jeonju.parcel_returned` — mở đường lên đài J11 |
| **JQ03 – Đèn soi qua hai nước** | jeonju:J17 | `have_flag:hoi_an.lamp_restored → solve:J17 → activate:J19` | `jeonju.beacon_lit` — mở Gyeongju, gửi rơle C Seoul |
| **JQ04 – Thư gửi phố cũ** | jeonju:J16 | `find:reply_letter@J21 → carry:reply_letter@H18` | `jeonju.letter_delivered` — Hội An mở ngõ H9 |
| **GQ01 – Đường sao Silla** | gyeongju:G3 | `get:star_symbol_1@G5 → get:star_symbol_2@G6 → get:star_symbol_3@G10 → solve:G4` | `gyeongju.three_stars_placed` — ống kính sao |
| **GQ02 – Bệ đá nối hai nước** | gyeongju:G12 | `have:ancient_seal → solve:G12 → defeat_or_fix:enemy_stone_guard@G7` | `gyeongju.seal_repaired` — phần thưởng giúp khám phá |
| **GQ03 – Đêm đài thiên văn** | gyeongju:G18 | `have_flag:gyeongju.three_stars_placed → defeat:enemy_star_core@G18 → activate:G19` | `gyeongju.beacon_lit` — mở điều kiện đi Seoul |
| **GQ04 – Hoa văn bờ tây** | gyeongju:G17 | `have_flag:jeonju.beacon_lit → solve:G17` | `gyeongju.west_lantern_done` — xu & bí mật |
| **SQ01 – Ba nhịp đồng bộ** | seoul:S7 | `have:blue_water_signal → solve:S16 → have:ancient_seal → solve:S17 → have:hanji_circuit → solve:S18` | `seoul.relays_ready` — mở boss S20 |
| **SQ02 – Lời nhắn cũ** | seoul:S2 | `get:stargazer_lens@G4 → interact:S3 → talk:S11` | `seoul.truth_discovered` — lựa chọn phục hồi hệ thống |
| **SQ03 – Lõi sáng cuối** | seoul:S20 | `have_flag:seoul.relays_ready → boss:enemy_final_core@S20 → visit:S21` | `seoul.final_cleared` — kết quả boss/SQ03; ceremony/credits độc lập sau đủ sáu restored |
| **SQ04 – Tín hiệu bóng Mờ** | seoul:S22 | `have_flag:seoul.truth_discovered → read:S22` | `seoul.secret_ending` — ID lịch sử của thông tin bí mật, không phải ending riêng |

**Ghi chú quan trọng:** Hai nhiệm vụ HQ03/JQ04 có cách diễn đạt khác nhau giữa nguồn gốc và story expansion về bước trả thư (J21 → H18/H9). Giữ nhiệm vụ đang chạy và bảo đảm một thư hồi âm duy nhất không bị tiêu hai lần, nhân vật Ji-eun nhận được lời kể và H9 mở đúng khi có tín hiệu. J18 là encounter lựa chọn, không khóa thư nếu đã có cách giải bùa hợp lệ. Không dùng string objective như code parser nếu chưa chuyển thành typed DTO.

## E. CÁC QUEST BỔ SUNG HỘI AN / GIAO LƯU VIỆT–HÀN

| ID | Nội dung/điều kiện thực thi | Kết quả |
|---|---|---|
| `HQ05` – Ớt qua biển | H18 Ji-eun gợi ý → H10 Bà Tư → **Jeonju J15 So-yeon**, trao `hoi_an_fresh_chili` ×1 **sau xác nhận**, không ảnh hưởng cửa hàng J15 | `jeonju.chili_shared`, `jeonju_recipe_card` ×1 grant-once |
| `HQ06` – Hành tím gửi cố đô | H10 Bà Tư → **Huế U1 Chị Hương**, trao `hoi_an_purple_shallot` ×1 sau xác nhận; U1 vẫn có nhiệm vụ kiện thư riêng | `hue.shallot_shared`, `hue_market_note` ×1 grant-once |
| `HQ07` – Hai bờ, một mâm cơm | Khi HQ05 và HQ06 đã hoàn thành → **trở lại H10**, Bà Tư nhận tin của cả hai nơi | `hoi_an.festival_food_exchange_done`, `festival_friendship_postcard` ×1 grant-once |
| `HA_FOOD_PREP` (đã thêm runtime) | Bà Tư yêu cầu ớt×2, hành×1; H11 có túi gia vị ớt×1, hành×1; trao tại H10, chỉ trừ đúng số lượng | Trà×1 và 8 xu, thưởng một lần; giữ lại đủ mỗi món cho tuyến liên thành phố |

**Tình trạng save cũ:** nếu đã xong HQ02 trước khi bổ sung đặc sản, có một lựa chọn nhận phần quà còn thiếu duy nhất; nếu thiếu vật phẩm bắt buộc khi HQ05/HQ06 active, Bà Tư có recovery giới hạn cho item đó, không farm xu, không tạo vòng grant vô hạn. `itemId` và tên theo catalog runtime mới nhất. Giữ những icon đã được duyệt/tạm đánh dấu, không dùng hình postcard mờ làm art final.

**Cảnh kết HQ07 mẫu:**
> Bà Tư: “Hàng của bà đã đến cả Huế và Jeonju rồi! Tờ giấy nhắn và thẻ món ăn còn đẹp hơn tiền lời.”
> Lumi: “Không chỉ đèn lồng, cả những việc nhỏ cũng nối hai nước.”

## F. DÀN CẢNH TRỌNG ĐIỂM & ĐỐI THOẠI THEO TRẠNG THÁI (PLAYTEST NEW Ở CHỖ THIẾU)

| Scene ID | Node | Nội dung bắt buộc khi chơi |
|---|---|---|
| `HA-intro` | H0,H2 | Bác lái đò báo đèn chập chờn → Cô Mai nhờ ba linh kiện; lựa chọn Nhận/Để sau; sổ quest cập nhật khi nhận. |
| `HA-hack` | H14,H19,H5 | Ông Phúc cung cấp manh mối Hoa–Trăng–Đèn; H19 giải mã; Bot Mờ-01 cảnh báo và chọn Thắng/Hack; phần thưởng link_key một lần. |
| `HA-end` | H7,H8,H20 | Lắp đủ 3 linh kiện, trạm giao lưu đồng bộ; Cô Mai nhắc cảnh báo, WorldMap Huế/Jeonju unlock đồng thời. |
| `U-begin` | U0,U1,U16,U3 | Lumi đến Huế; Chị Hương kể bưu kiện thư; puzzle U16 lấy thẻ, bác Tâm cho phép qua sông. |
| `U-letter` | U15,U7,U17,U8,U5 | Cứu thư từ bot, nhờ Lê An đọc mật mã, giải U17/U8, xử lý bot U5 đúng thứ tự tiếp cận. |
| `U-finish` | U11,U19,U20 | Nhặt ấn tín sao; làm sáng trạm, mở Đà Lạt, nhắc giữ vật chứng Gyeongju/Seoul. |
| `D-begin` | D0,D2,D7,D8 | Sương mù; Lan nhờ kính; thắng/giải bẫy D7 rồi TRẢ D2 mới mở lối D8. |
| `D-dog` | D9,D11,D12,D3,D10 | Gom đủ 3 thức ăn, có recovery D3; cho chó ăn trước khi mở lối D15. |
| `D-finish` | D15,D16,D14 | Bóng sương nói đang bảo vệ khỏi thác; xử lý encounter, lấy tín hiệu xanh, bật trạm. |
| `J-begin` | J0,J2,J5,J6,J7,J11 | Hana thiếu ba hoa văn; Park thiếu bưu kiện; giải hai nhiệm vụ để lấy pattern 1/2/3. |
| `J-finish` | J12,J17,J19,J20 | Ghép Hanji circuit, kết nối đèn Hội An ở J17, bật trạm Jeonju, mở Gyeongju. |
| `J-letter` | J16,J21,H18,H9 | Người giữ chuyện kể thư; lấy tại J21, trở lại Ji-eun ở H18 và H9 giúp đèn lạc. |
| `G-begin` | G0,G3,G5,G6,G10,G4 | Min hướng dẫn tìm ba biểu tượng sao; ráp G4, nhận kính thiên văn cho Seoul. |
| `G-seal` | U11,U17,G12,G7 | Mang ấn cổ Huế đến G12, giải mã để sửa bệ; chọn giúp tượng đá hoặc đánh; khác cờ nếu khác phương án. |
| `G-finish` | G18,G19,G20 | Lõi sao 3 nhịp, thắp trạm, kiểm tra đủ bốn beacon trước khi mở Seoul. |
| `S-truth` | S0,S2,S3,S4,S11 | Ghép hồ sơ và kính sao để biết bot cảnh báo chứ không hoàn toàn ác, lệnh phòng vệ bị sửa. |
| `S-relays` | S7,S16,S17,S18 | Ji-won yêu cầu nguồn A xanh Đà Lạt, B Ấn tín Huế, C Hanji Jeonju; giải puzzle từng trạm. |
| `S-final` | S19,S20,S21,S22 | Ngắt Bot Đưa Tin; lõi hai pha, nhánh Sửa/Đánh/Che; trả lại khả năng bảo vệ, xác nhận boss/SQ03 tại S21; Lễ Kết Nối chủ động sau đủ sáu restored, secret đọc S22. |

### Quy tắc dialogue state

- Mọi NPC nhận quest phải có lời thoại `not_offered`, `active_missing`, `active_ready`, `completed` và `repeat_talk`. Nếu có giao vật phẩm, phải có `confirm/cancel` và phản hồi khi thiếu đồ; không tự giao vì bấm Next.
- Boss/enemy có lời trước encounter, thua, thắng, và sau xử lý; nhánh dùng mật mã phải phản ánh kết cục khác thắng trận nhưng không tạo reward trùng. Không tự đổi những sprite boss và marker dấu ! đã được duyệt.
- Dialogue speaker portrait **chỉ hiển thị người đang nói**; fade xong phải ẩn ảnh cũ, không có hình ma 10% nằm trên background. Bà Tư/Ông Phúc chưa có art sạch thì dùng placeholder có tên, gắn needs-art, không dùng nhân vật khác đã định danh.
- Trường hợp câu ngắn, panel không chiếm quá nửa màn hình với vùng trống lớn; nút dài tự xuống dòng. Popup nhận thưởng hiển thị theo thứ tự rõ, tránh cùng lúc che dialogue. Nhưng nếu UI polish prompt khác đang sửa, không làm lại UI trong task nội dung này.

## G. NÚT KHÓA & GIẢI QUYẾT XUNG ĐỘT ĐỂ CHƠI THỬ THỰC

- **Hội An:** H4/H13/H19/H5/H7 đều phải truy cập đúng thời điểm; không buộc thắng H5 để đi lấy mã H19. H5 OR win/hack. H7 mới unlock Huế/Jeonju.
- **Huế:** U3/U16 trước U4; U15 cứu thư; U5 có cách đánh không cần U11 nằm phía sau gate. Nhánh hack U5 bằng ấn tín chỉ mở nếu người chơi có vật phẩm trước, không buộc trong lượt đầu. U17 + U8 + guard + thư trước U19.
- **Đà Lạt:** `glasses_returned` chỉ bật sau trả D2; 3 `meat_portion` tồn tại một lần, bổ sung D3 nếu thiếu; D15 giải trước D16; D14 cần `fog_cleared` và `blue_water_signal`.
- **Jeonju:** J11 không cấp pattern_3 trước khi trả missing_parcel; J12 cần đủ 3; J17 cần tín hiệu trạm giao lưu Hội An; J19 cần J12+J17; J15 giao ớt là phụ, không đổi điều kiện J19. J21 phải có lối tiếp cận và có thể quay về Hội An làm HQ03.
- **Gyeongju:** G4 cần 3 symbol; `ancient_seal` và hint U17 có thể được dùng G12, không tiêu vật phẩm cần cho Seoul S17; G7 phương án sửa/đánh không set cờ `stone_guard_friend` giống nhau; G18 rồi G19.
- **Seoul:** ba rơle A/B/C chỉ bật khi có item/tín hiệu thật; `seoul.relays_ready` derive từ ba cờ, không lưu lệch; S20 đúng hai pha; `seoul.final_cleared` sau boss, `game.canonical_ending_completed.v1` chỉ sau ceremony hợp lệ; `game.ending_seen` giữ lịch sử credits. S22 bí mật theo SQ04 khi `seoul.truth_discovered`.
- **Các item cấp bởi event thiết kế** chỉ được phát khi thắng/giải/nhặt/confirm thật, không được phát khi `OnArrive` trừ node có autoCollect được đánh dấu rõ. Kiểm thử không được dùng fixture để tuyên bố e2e PASS.

## H. CHECKLIST NGHIỆM THU — 133 NODE, 29 QUEST, 6 CHƯƠNG

1. Mỗi node H0–H21/U0–U21/D0–D21/J0–J21/G0–G21/S0–S22 có `storyRole`, `eventId`, interaction, trạng thái sau khi làm, và dấu kiểm nếu là node chỉ đường. Không còn `coming soon` trong chương tuyên bố hoàn thành.
2. Có bảng mỗi node `DESIGN_ONLY / IMPLEMENTED / PARTIAL / BLOCKED`, riêng ảnh QA H22 không được ghi vào game thật. Bảng event phải chỉ ra code thực tế thay vì chỉ liệt kê JSON.
3. Fresh H0 → hoàn thành H7 → làm HQ05/HQ06 theo cả hai thứ tự → hoàn thành HQ07 → bốn beacon nhánh → ba rơle → S20 → S21, **không dùng fixture**. Nếu đầu vào thật không đi được, ghi điểm dừng và sửa dependency.
4. Thử nhánh H5 hack, D7 dùng bẫy, D10 thiếu/đủ thức ăn, U5 đánh nếu thiếu ấn tín, J6 giải kho/đánh, G7 cứu/đánh, S20 sửa/đánh. Thử battle thua/retry, puzzle sai/làm lại, player rời map/quay lại, save/restart sau mỗi beacon và trước boss.
5. Các interaction tại U1/J15/H10 không chồng lấn hoặc nuốt event gốc; phát item/tiền/chest/check-in thưởng **một lần**; không cấp vật phẩm quest chưa tìm được.
6. Mọi thành phố hiển thị map art A đã duyệt, có NPC đúng art canonical hoặc nhãn TEMPORARY; không thay boss/dấu !, không đổi GUID/sprite nếu không cần.
7. Bàn giao ảnh runtime câu chuyện mở đầu, key gate, quest thành công, hội thoại NPC, boss, beacon, item qua thành phố, cuối Seoul; build PASS, process restart, manifest, checkpoint và báo rõ Android thật NOT VERIFIED nếu chưa test.
8. Khi thêm nội dung để test, giữ **một phiên bản nội dung rõ ràng**; không tạo quest/DTO/manager mới nếu hệ cũ đáp ứng. Nếu chạy nhiều giờ, checkpoint từng chapter; sau khi có full narrative playable thì quay lại polish đồ họa/UI.

## I. ĐÍNH KÈM TRONG PROJECT (tham khảo không ghi đè)

- `LBC_Gameplay_6Cities_Detailed_v1/Docs/FULL_LEVEL_DESIGN.md`
- `LBC_Gameplay_6Cities_Detailed_v1/Docs/QUEST_CHAINS_AND_GATES.md`
- `LBC_Gameplay_6Cities_Detailed_v1/Data/Cities/*.json`, `Data/quests.json`, `Data/items.json`, `Data/enemies.json`, `Data/world_progression.json`
- `LBC_HoiAn_StoryExpansion_v1/Data/hoi_an_chapter.json`, `Data/cross_city_food_hooks.json`, `Docs/STORY_BIBLE_HOI_AN.md`
- Runtime `Assets/_Game/Resources/Content/GameplayContent.json`, `AdventurePresentation.json`, `AdventureUIArt.asset`, báo cáo checkpoint mới nhất.

**Chú thích:** bản thiết kế cũ có số cạnh từng thành phố nhiều hơn mục tiêu người dùng mới là **tối đa 3 hàng xóm/node**. Đây là công việc topology đang được xử lý bởi prompt riêng; trong task cốt truyện phải tôn trọng graph runtime đã được duyệt/đang chỉnh, bảo toàn reachability, không chép nguyên 165 edge phác thảo vào gameplay.
