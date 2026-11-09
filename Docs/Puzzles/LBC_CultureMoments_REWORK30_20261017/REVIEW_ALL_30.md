# 30 câu hỏi mới — bản rà soát thủ công (17/10/2026)

**Lưu ý:** Đây là **đề xuất nội dung**, không phải xác nhận UI/ảnh/manh mối đã tồn tại trong runtime. Một số manh mối bổ sung ở `evidencePresentation` chưa chắc game hiện tại có: Codex **bắt buộc kiểm tra** và nếu thiếu thì để SKIPPED/NEEDS_CONTENT_CUE, không ép gắn câu hỏi hoặc tạo bằng chứng giả.

- 15 Culture Moments, 30 biến thể, chỉ 1 câu cho mỗi lần chạm một moment.
- 22 puzzle cốt truyện, đáp án, phần thưởng, flag và save hiện hành: giữ nguyên.
- Được chơi mà không biết kiến thức ngoài game; có thể bấm bỏ qua.
- Ưu tiên câu hỏi dựa trên quan sát, đối chiếu vật chứng, suy luận không gian, quan hệ nguồn–đích, KHÔNG hỏi đạo đức hiển nhiên.
- Những mô tả hiện vật/sơ đồ nếu chưa có thật trong game là **đề xuất cần tạo cue tối thiểu và nghiệm thu**, không được mô tả là đã được người chơi nhìn thấy.


## CM_HA_01 — Đèn sáng cho cả phố (hoi_an, H2)

**Gate cần xác minh:** Chỉ sau hoàn thành HQ01 / hoi_an.lamp_restored; không đặt moment trong luồng nhận Mắt Đèn  
**Gốc cốt truyện:** Cô Mai H2 nhận ba linh kiện; trạm H7 sáng sau phục hồi.  
**Phản hồi gợi ý:** Cô Mai đổi một câu thoại ngắn; một vùng đèn phụ/symbol trên map sáng nếu đã có art phù hợp, không đổi flag hồi phục.

### CM_HA_01_V1

**Manh mối phải thấy trước:** Cửa ngõ gần bến cần được nhìn thấy từ ngoài; mái hiên che đèn mưa nhưng thanh treo sát tường khiến bóng che lối.

**Cách trình bày manh mối (đề xuất):** Đối thoại Cô Mai chỉ hai vị trí treo trên mô hình: sát tường và đầu mái hiên, với vùng bóng đổ.

**Hỏi:** Đèn đã được sửa nhưng lối từ bến còn khuất. Điều chỉnh nào dùng lại chiếc đèn mà chiếu được lối đi?

- A. Giữ vị trí cũ, tăng thêm một lớp giấy khuếch tán.

- B. Đưa đèn ra đầu mái hiên và xoay mặt sáng về phía bến.

- C. Đổi đèn sang mái phía trong nhưng giữ độ cao hiện tại.


**Đáp án:** B — Mái hiên vẫn che mưa, còn đèn không bị tường chắn góc nhìn từ bến.

**Gợi ý:** Hãy xét vật gì che ánh sáng từ phía bến, không chỉ xét độ sáng.

### CM_HA_01_V2

**Manh mối phải thấy trước:** Mô hình Cô Mai có 2 đèn: chiếc A nhìn rõ từ bến nhưng rọi vào cửa nhà; chiếc B rọi đường nhưng bị cành cây khuất; còn một móc treo giữa hai vị trí.

**Cách trình bày manh mối (đề xuất):** Cho thấy sơ đồ mô phỏng 3 móc/2 vệt sáng bằng UI ký hiệu trước khi hỏi.

**Hỏi:** Muốn vừa dẫn đường vừa tránh rọi thẳng vào nhà dân, nên điều chỉnh gì?

- A. Chuyển đèn A sang móc giữa và hướng chùm sáng dọc lối đi.

- B. Giữ đèn B rồi chỉ hạ thấp một chút dưới tán cây.

- C. Đổi chỗ hai đèn mà không thay hướng rọi.


**Đáp án:** A — Móc giữa cho góc nhìn không bị cành cây che và có thể xoay chùm sáng ra lối đi.

**Gợi ý:** Hai yêu cầu độc lập: không bị che và không rọi vào cửa nhà.

## CM_HA_02 — Dấu vết của người thợ (hoi_an, H4)

**Gate cần xác minh:** Chỉ sau khi H4 puzzle_market giải xong; tuyệt đối không sửa mã Hoa–Trăng–Đèn  
**Gốc cốt truyện:** H4 sạp chợ chứa chuỗi Hoa–Trăng–Đèn; H13 dùng mẫu Hoa.  
**Phản hồi gợi ý:** Một vệt sáng đánh dấu mẫu đã hiểu hoặc thoại của thương nhân; KHÔNG cấp hanji_core ở moment này.

### CM_HA_02_V1

**Manh mối phải thấy trước:** Ở bàn thợ có mảnh giấy cũ: nét HOA nhòe một góc nhưng còn trùng với bản mẫu ở H4; hai mảnh mới đều có nét đủ nhưng lệch theo hướng khác.

**Cách trình bày manh mối (đề xuất):** Hiển thị 3 mô tả mảnh A/B/C qua giao diện quan sát, không thay puzzle H4/H13.

**Hỏi:** Nếu phải vá góc thiếu mà vẫn giữ nét gốc làm chuẩn, thợ nên chọn mảnh nào?

- A. Mảnh sao lại toàn bộ hình mới, bỏ phần gốc bị nhòe.

- B. Mảnh giữ đúng hướng nét HOA của bản gốc và chỉ vá phần mất.

- C. Mảnh đẹp nhất ở ngoài sạp, nhưng nét HOA bị soi gương.


**Đáp án:** B — Bảo tồn đặc điểm nhận dạng là hướng nét gốc; vá cục bộ không thay cả mẫu.

**Gợi ý:** Đối chiếu hướng nét, đừng chỉ so độ mới của giấy.

### CM_HA_02_V2

**Manh mối phải thấy trước:** Người thợ đưa ba mẫu: mẫu X có HOA đúng chiều nhưng mất một cánh; Y đủ cánh nhưng bị lật; Z là bản in khác tỉ lệ.

**Cách trình bày manh mối (đề xuất):** Cho người chơi xem ký hiệu đối xứng/mất cánh trước trong hội thoại quan sát ở H4.

**Hỏi:** Mẫu nào tốt nhất làm chứng cứ xác nhận nét HOA gốc trước khi phục dựng?

- A. X, vì phần còn lại giữ đúng chiều và tỉ lệ của nét gốc.

- B. Y, vì đủ cánh luôn đáng tin hơn mảnh thiếu.

- C. Z, vì bản in mới rõ nét hơn vết khắc cũ.


**Đáp án:** A — Mảnh thiếu nhưng nguyên gốc vẫn đáng tin để suy ra hướng và tỉ lệ; các bản khác có sai lệch.

**Gợi ý:** Tìm nguồn giữ đúng định hướng chứ không chỉ đủ nét.

## CM_HA_03 — Lá thư nối Hội An và Jeonju (hoi_an, H18)

**Gate cần xác minh:** Chỉ sau khi người chơi có thông tin thư hồi âm J21 và đã đến H18; tránh spoil tuyến Hội An trước Jeonju  
**Gốc cốt truyện:** Ji-eun tại H18, thư J21 có thể mang về để mở ngõ H9.  
**Phản hồi gợi ý:** Ji-eun thêm một câu cảm ơn có ý nghĩa; không tiêu thư, không thay điều kiện H9.

### CM_HA_03_V1

**Manh mối phải thấy trước:** Thư Jeonju ghi hình đèn Hội An và dòng ký hiệu người gửi; Ji-eun giữ lời nhắn đầu chuyến đi, có cùng một dấu nhưng khác lời chào.

**Cách trình bày manh mối (đề xuất):** Cho xem 2 mẩu thư/biểu tượng đã thu được ở J21 và H18 trước câu hỏi.

**Hỏi:** Hai lá thư khác ngôn ngữ và thời điểm. Dấu hiệu nào giúp xác nhận chúng thuộc cùng một cuộc trao đổi?

- A. Cùng kích thước phong bì, dù dấu niêm khác nhau.

- B. Cùng dấu ký hiệu và người nhận, dù nội dung lời chào khác.

- C. Cùng màu mực, dù nơi gửi không khớp.


**Đáp án:** B — Dấu nhận diện và người nhận liên kết hai thư chắc chắn hơn đặc điểm hình thức.

**Gợi ý:** Phân biệt dấu nhận diện với các chi tiết bề ngoài.

### CM_HA_03_V2

**Manh mối phải thấy trước:** Hội An có lời nhờ gửi tin về lễ đèn; thư phản hồi Jeonju nói đã nhận, kèm một câu hỏi mới dành cho Ji-eun.

**Cách trình bày manh mối (đề xuất):** H18 giới thiệu mục đích ban đầu; sau J21 hiển thị phần thư phản hồi, rồi mới mở lựa chọn.

**Hỏi:** Muốn tiếp nối cuộc trò chuyện đúng mạch, Lumi nên chuyển phần nào trước?

- A. Gửi riêng câu hỏi mới và bỏ qua câu xác nhận đã nhận thư.

- B. Kể chuyến đi của mình rồi để Ji-eun tự đoán ai hồi đáp.

- C. Trao thư hồi âm và nói rõ nó đáp lại lời nhờ ở Hội An.


**Đáp án:** C — Lá thư hồi âm là bằng chứng nối hai phía; kể mạch gửi–nhận trước khi mở câu chuyện mới.

**Gợi ý:** Hỏi xem thông tin nào nối được hai đầu câu chuyện.

## CM_HU_01 — Bóng cờ trên sân điện (hue, U8)

**Gate cần xác minh:** Chỉ sau khi U8 puzzle_court đã giải; không lộ thứ tự cờ trước khi giải  
**Gốc cốt truyện:** U8 ba cờ theo manh mối U17; vật chứng ghi nhịp ngắn–dài–ngắn.  
**Phản hồi gợi ý:** Đổi câu thoại người gác hoặc highlight khung ghi chép đã giải; không động vào flag U8.

### CM_HU_01_V1

**Manh mối phải thấy trước:** Hai bản ghi U17 và U8 cùng mô tả nhịp 3 lần, nhưng một là chớp đèn, một là cờ màu; đoạn giữa dài hơn hai đoạn đầu/cuối.

**Cách trình bày manh mối (đề xuất):** Cho nhìn hai đoạn log ký hiệu S/D/S và ba lá cờ sau khi giải U8.

**Hỏi:** Nếu bản ghi thứ hai chỉ còn cờ đoạn giữa, phải dò nó với phần nào của bản ghi đầu?

- A. Đoạn ngắn đầu, vì xuất hiện trước.

- B. Đoạn dài ở giữa, vì cùng vị trí và độ dài.

- C. Đoạn ngắn cuối, vì gần kết thúc.


**Đáp án:** B — Độ dài và vị trí cùng xác nhận nó là nhịp giữa, không chỉ thứ tự màu.

**Gợi ý:** So cả vị trí lẫn độ dài, không đoán theo màu.

### CM_HU_01_V2

**Manh mối phải thấy trước:** Sơ đồ sau giải cho thấy cờ giữa từng treo nhầm ở vị trí đầu; nhật ký người gác ghi thời điểm sửa trước buổi lễ.

**Cách trình bày manh mối (đề xuất):** Hiện sơ đồ hiện tại và nhật ký sửa đặt cờ sau khi puzzle U8 hoàn tất.

**Hỏi:** Tài liệu nào nên dùng để giải thích vì sao ảnh cũ khác sân hiện tại?

- A. Ảnh cũ và ghi chú thời điểm hoán đổi cờ.

- B. Chỉ ảnh mới vì vật thể hiện tại luôn đúng lịch sử.

- C. Chỉ màu cờ vì vị trí không liên quan.


**Đáp án:** A — Đối chiếu nguồn theo thời điểm giải thích được sự khác nhau mà không buộc một bên phải sai.

**Gợi ý:** Chú ý thời gian ảnh được chụp và lúc cờ được chuyển.

## CM_HU_02 — Thư khố và dấu xác thực (hue, U17)

**Gate cần xác minh:** Chỉ sau U17 puzzle_archive giải; không hiển thị nhịp chính trước puzzle  
**Gốc cốt truyện:** U15 kiện thư; U17 nhịp ngắn–dài–ngắn; U11 ấn tín dùng liên thành phố.  
**Phản hồi gợi ý:** Tạo feedback giấy/nhịp ngắn không cấp ancient_seal, không ghi lại puzzle state.

### CM_HU_02_V1

**Manh mối phải thấy trước:** Cùng một kiện thư có bản A ghi 3 nhịp, bản B có cùng nội dung nhưng chỉ ghi hai nhịp cuối; bìa B được ghi là bản chép thiếu dòng đầu.

**Cách trình bày manh mối (đề xuất):** Sau U17 cho xem bản chép A/B và dấu ghi chép thiếu dòng.

**Hỏi:** Khi mang manh mối sang Gyeongju, bằng chứng nào nên giữ để tránh mất nhịp đầu?

- A. Bản B vì nó gọn hơn.

- B. Ghép hai nhịp cuối của B rồi tự đoán nhịp còn lại.

- C. Bản A đủ ba nhịp, đối chiếu B như bản kiểm tra.


**Đáp án:** C — Bản đủ thông tin là nguồn chính; bản chép thiếu chỉ dùng đối chiếu, không thể thay nguồn.

**Gợi ý:** Bản nào có đủ ba nhịp và bản nào chỉ là bản chép thiếu?

### CM_HU_02_V2

**Manh mối phải thấy trước:** Người giữ thư khố chỉ ra ấn tín dùng xác thực nguồn, còn bản ghi nhịp mới là thông tin cần đọc.

**Cách trình bày manh mối (đề xuất):** Cho thấy hai ô hồ sơ: "xác thực" và "chuỗi tín hiệu"; đối thoại phân biệt vai trò.

**Hỏi:** Muốn kiểm tra một thông điệp có nguồn đáng tin và giải được nội dung, cần kết hợp gì?

- A. Dùng ấn tín đọc thay toàn bộ nhịp vì cùng có hoa văn.

- B. Đối chiếu ấn tín để xác thực nguồn, rồi đọc nhịp từ bản ghi.

- C. Đọc nhịp trước, không cần quan tâm bản ghi bị sửa hay không.


**Đáp án:** B — Ấn tín và nhịp giải quyết hai câu hỏi khác nhau: ai xác nhận và thông điệp nói gì.

**Gợi ý:** Một vật cho biết nguồn, một vật cho biết nội dung.

## CM_DL_01 — Tìm lối qua màn sương (da_lat, D2)

**Gate cần xác minh:** Sau khi trả kính cho Lan và có thể tiếp cận D8; câu không được thay lời giải của D8  
**Gốc cốt truyện:** Lan mất kính D2; D8 dùng manh mối Thông–Đèn–Hồ.  
**Phản hồi gợi ý:** Lan chỉ lại đúng ký hiệu cầu/đường trên bản đồ nếu overlay an toàn; không mở D8 thay puzzle.

### CM_DL_01_V1

**Manh mối phải thấy trước:** Lan có bản đồ với 3 mốc Thông/Đèn/Hồ. Sương che mốc Đèn; trên giấy, Đèn nằm giữa hai vị trí cố định, đường đi có một ngã rẽ giả.

**Cách trình bày manh mối (đề xuất):** Cho người chơi nhìn sơ đồ ba điểm kề nhau và một ngã rẽ giả, không bật lời giải D8.

**Hỏi:** Lúc không thấy mốc giữa, cách nào giúp đối chiếu đường đi mà không tự đoán vị trí?

- A. Đọc quan hệ giữa Thông và Hồ trên sơ đồ để khoanh vùng mốc bị che.

- B. Đi theo đường có ánh sáng mạnh nhất dù không có trên bản đồ.

- C. Đi thẳng theo bóng cây gần nhất rồi quay lại nếu sai.


**Đáp án:** A — Hai mốc còn nhận biết giới hạn vị trí Đèn và loại nhánh giả.

**Gợi ý:** Sơ đồ có hai mốc còn thấy được; hãy dùng cả hai.

### CM_DL_01_V2

**Manh mối phải thấy trước:** Sơ đồ Lan cho biết cầu đá nối hai bờ hồ; đường ven cây cũng tiến gần hồ nhưng kết thúc ở bờ chưa có cầu.

**Cách trình bày manh mối (đề xuất):** Cho người chơi xem sơ đồ đường nối đã tìm được và ký hiệu cầu, không dùng ảnh không có trên map.

**Hỏi:** Nếu cần sang bờ đối diện mà không đi xuyên mặt nước, bằng chứng nào xác nhận tuyến đúng?

- A. Chọn đường ngắn nhất trên tranh dù kết thúc sát mép nước.

- B. Theo đường có cây thông vì cây dễ thấy trong sương.

- C. Theo đường được ký hiệu nối tới cầu đá rồi mới sang bờ.


**Đáp án:** C — Tuyến đúng phải có cầu thật trong sơ đồ địa hình, không chỉ có hướng gần đích.

**Gợi ý:** Chỗ nào thực sự nối hai bờ?

## CM_DL_02 — Dấu chân bên cầu đá (da_lat, D10)

**Gate cần xác minh:** Chỉ sau DQ02/dalat.dog_fed; không dùng moment để nhận lại reward cho chó  
**Gốc cốt truyện:** Chó D10 được cho ăn 3 phần, trở thành trợ giúp chỉ D15/D17.  
**Phản hồi gợi ý:** Chó phản ứng vui và marker lối phụ nháy nhẹ nếu có; không thay flag dog_fed.

### CM_DL_02_V1

**Manh mối phải thấy trước:** Khi no, chó dẫn Lumi đến vệt dấu chân cũ: một vệt chạy tới lùm cây rồi quay ngược, vệt còn lại kéo dài về phía cầu đá.

**Cách trình bày manh mối (đề xuất):** Cho thấy hai vệt trên sơ đồ/đối thoại quan sát sau khi đã cho chó ăn.

**Hỏi:** Nếu đang tìm lối thông qua cầu chứ không tìm chỗ trú, nên kiểm tra vệt nào trước?

- A. Vệt quay đầu ở lùm cây vì nó có nhiều dấu chân hơn.

- B. Vệt tiếp tục tới cầu, vì có dấu hiệu dẫn tới lối thông.

- C. Cả hai đều chứng minh có cầu ở cuối đường.


**Đáp án:** B — Dấu vết tiếp tục theo hướng cầu phù hợp mục tiêu tìm lối đi; lùm cây là đường cụt.

**Gợi ý:** Đừng nhầm nơi dấu chân dày nhất với tuyến còn đi tiếp.

### CM_DL_02_V2

**Manh mối phải thấy trước:** Chó dừng trước ba dấu mốc: vết cào ở gốc cây, dấu chân mới hướng ra hồ và vệt đất khô dẫn tới mặt cầu. Mục tiêu là đi đường ít trơn.

**Cách trình bày manh mối (đề xuất):** Trình bày dấu hiệu môi trường trên bản đồ/mô tả trước lượt hỏi.

**Hỏi:** Trong ba dấu hiệu, dấu nào đáng ưu tiên kiểm tra để tìm mặt đường khô?

- A. Vệt đất khô dẫn tới mặt cầu, nhưng vẫn kiểm tra mép cầu trước khi qua.

- B. Dấu chân mới bên hồ, vì có người vừa đi thì chắc chắn an toàn.

- C. Vết cào ở gốc cây, vì chó đã đứng ở đó lâu.


**Đáp án:** A — Vệt đất khô là dấu trực tiếp về nền đường; việc người khác đi qua hay chó đánh dấu không đảm bảo độ trơn.

**Gợi ý:** Bằng chứng nào nói trực tiếp đến nền đường?

## CM_JJ_01 — Ba mảnh hoa văn (jeonju, J2)

**Gate cần xác minh:** Chỉ sau JQ01/jeonju.three_patterns_joined; không ghi hộ progress J12  
**Gốc cốt truyện:** J2 Hana ghép ba pattern J7,J6,J11 và J12 hoàn chỉnh.  
**Phản hồi gợi ý:** Hana chỉ đúng chi tiết hoa văn và khen người chơi quan sát; không cấp thêm pattern.

### CM_JJ_01_V1

**Manh mối phải thấy trước:** Hana đánh dấu ba mảnh nguồn: mảnh 1 có nét HOA bị cắt tại mép phải, mảnh 2 nối tiếp bằng đúng nét còn thiếu, mảnh 3 chỉ có hoa văn đã hoàn chỉnh ở viền ngoài.

**Cách trình bày manh mối (đề xuất):** Cho người chơi xem minh họa đường nối 3 mảnh từ pattern đã thu trước khi hỏi.

**Hỏi:** Muốn phục dựng dấu nét HOA bị đứt, nên ghép cặp nào trước?

- A. Mảnh 1 với mảnh 3 vì viền mảnh 3 đã hoàn chỉnh.

- B. Mảnh 2 với mảnh 3 vì cả hai sắc nét hơn.

- C. Mảnh 1 với mảnh 2 vì hai nét nối liền đúng mép gãy.


**Đáp án:** C — Điểm nối của nét đứt quyết định cặp ghép, không phải mảnh đẹp hoặc hoàn chỉnh nhất.

**Gợi ý:** Tìm hai mép có nét bị ngắt rồi tiếp nối.

### CM_JJ_01_V2

**Manh mối phải thấy trước:** Khuôn sau phục dựng có họa tiết lặp đối xứng trái–phải; một bản sao của người học nghề chỉ bị lật hướng nét ở nửa phải.

**Cách trình bày manh mối (đề xuất):** Cho xem khuôn đã ghép tại J12 và bản sao lỗi bằng mũi tên định hướng.

**Hỏi:** Nếu cần chỉnh ít nhất một thao tác để trở lại đối xứng, nên làm gì?

- A. Thay toàn bộ nửa trái bằng nửa phải đã bị lật.

- B. Lật lại nửa phải theo đường trục giữa, giữ nguyên nửa trái gốc.

- C. Xoay cả khuôn một vòng vì hướng nét không quan trọng.


**Đáp án:** B — Sai lệch nằm ở nửa phải; sửa đúng một nửa giữ khuôn gốc nguyên vẹn.

**Gợi ý:** Đối chiếu phía nào khác khuôn gốc.

## CM_JJ_02 — Hanji và nét gấp (jeonju, J7)

**Gate cần xác minh:** Chỉ sau giải puzzle_hanji J7; không đưa số lần xoay/gấp vào câu hỏi trước khi giải  
**Gốc cốt truyện:** J7 có hai lần xoay, một lần gấp và mảnh pattern1.  
**Phản hồi gợi ý:** Hana hoặc thư viện phản hồi bằng biểu tượng gấp nhẹ, không cấp thêm pattern1.

### CM_JJ_02_V1

**Manh mối phải thấy trước:** Sau khi hoàn thành mẫu đầu tiên, người học nghề cho xem hai bản gấp: A khớp vạch sáng nhưng hoa bị lệch tâm, B gấp đúng tâm nhưng vạch sáng không trùng.

**Cách trình bày manh mối (đề xuất):** Cho xem hai lỗi trên hình mẫu và tờ chuẩn đã hoàn thành.

**Hỏi:** Để có cả tâm và vạch khớp, nên ưu tiên kiểm tra gì?

- A. Đổi giấy dày hơn, giữ nếp gấp y nguyên.

- B. Kiểm tra lại hướng căn vạch trước khi ép nếp gấp cuối.

- C. Kẻ thêm vạch mới đúng vị trí nếp đang lệch.


**Đáp án:** B — Căn hướng vạch trước khi gấp giúp tránh giữ nếp sai; đổi giấy hay vẽ thêm làm lệch mẫu gốc.

**Gợi ý:** Sai lệch bắt đầu trước hay sau khi tạo nếp?

### CM_JJ_02_V2

**Manh mối phải thấy trước:** Hai mẩu hoa văn đặt trên mẫu Hanji: mẩu X giữ vạch nối của mẫu gốc nhưng giấy hơi nhăn; Y phẳng hơn nhưng vạch nối bị lệch một ô.

**Cách trình bày manh mối (đề xuất):** Cho nhìn vạch nối gốc và hai ứng viên cạnh nhau.

**Hỏi:** Nếu dùng làm khuôn truyền tín hiệu cho J12, nên chọn mẩu nào làm chuẩn?

- A. X, vì vạch nối đúng; có thể làm phẳng giấy sau.

- B. Y, vì bề mặt phẳng sẽ tự sửa phần nét lệch.

- C. Cả hai đều như nhau vì tín hiệu không dùng nét nối.


**Đáp án:** A — Nét nối mới quyết định thông tin của khuôn, độ phẳng là lỗi có thể khắc phục sau.

**Gợi ý:** Vật nào mang thông tin cần giữ chính xác?

## CM_JJ_03 — Gói quà qua hai miền (jeonju, J15)

**Gate cần xác minh:** Chỉ sau HQ05 và J15 trao quà từ Hội An; không tiêu/chuyển chili hoặc thay recipe_card  
**Gốc cốt truyện:** J15 So-yeon nhận ớt Hội An, trả jeonju_recipe_card.  
**Phản hồi gợi ý:** Một postcard/thoại giao lưu nếu postcard hiện hữu phù hợp; không cấp thêm jeonju_recipe_card.

### CM_JJ_03_V1

**Manh mối phải thấy trước:** So-yeon muốn viết nhãn gói quà giới thiệu ớt Hội An cho người Jeonju; một nhãn ghi rõ nơi gửi, nguyên liệu và lời chào, hai nhãn còn lại thiếu thông tin.

**Cách trình bày manh mối (đề xuất):** Hiển thị ba nhãn với nội dung đủ khác biệt trước khi lựa chọn.

**Hỏi:** Nhãn nào kể được câu chuyện của món quà mà người nhận không nhầm xuất xứ?

- A. "Quà cay đặc biệt — ngon nhất mọi nơi".

- B. "Ớt từ Hội An — gửi tới bạn Jeonju cùng lời chào từ Việt Nam".

- C. "Gia vị Jeonju — được Lumi mua trên đường".


**Đáp án:** B — Một nhãn rõ nguồn gốc và người nhận; không gán nhầm xuất xứ khi giao lưu hai nước.

**Gợi ý:** Người nhận cần biết quà đến từ đâu và dành cho ai.

### CM_JJ_03_V2

**Manh mối phải thấy trước:** Trong sổ chợ có hai dòng: người gửi Hội An tặng ớt, người nhận Jeonju hồi đáp bằng công thức mới. Một người viết nháp định ghi cả hai món là đặc sản cùng một nơi.

**Cách trình bày manh mối (đề xuất):** Cho xem sổ nhận và thư hồi đáp trước câu hỏi, sau khi HQ05 đã hoàn thành.

**Hỏi:** Nếu viết thiệp giao lưu chính xác, nên trình bày quan hệ hai món quà thế nào?

- A. Ớt Hội An gửi sang Jeonju; Jeonju chia sẻ công thức để đáp lễ.

- B. Ớt và công thức đều do Hội An tạo nên, Jeonju chỉ giữ hộ.

- C. Công thức là món quà đầu tiên; ớt là hàng mua từ Seoul.


**Đáp án:** A — Hai bên đóng góp khác nhau; ý nghĩa nằm ở việc trao đổi chứ không đồng nhất văn hóa.

**Gợi ý:** Tách phần người gửi đóng góp với phần người nhận hồi đáp.

## CM_GJ_01 — Đọc dấu sao Silla (gyeongju, G3)

**Gate cần xác minh:** Chỉ sau G4 solved và có stargazer_lens; không lộ mã G4 trước  
**Gốc cốt truyện:** Min G3 giới thiệu ba biểu tượng sao G5/G6/G10; G4 ghép thành ống kính.  
**Phản hồi gợi ý:** Min đưa một nhận xét khảo cổ ngắn; highlight ống kính đã có, không cấp vật chứng.

### CM_GJ_01_V1

**Manh mối phải thấy trước:** Min đặt ba bản sao ký hiệu cạnh nhau: A nét sao ở vị trí trên, B nét sao ở vị trí giữa, C nét sao ở vị trí dưới; phần bị mờ trên bản gốc trùng với B.

**Cách trình bày manh mối (đề xuất):** Cho người chơi quan sát các bản sao và mảnh bị mờ bằng panel quan sát sau khi giải G4.

**Hỏi:** Muốn phục hồi chi tiết bị mờ mà không tự chế thêm ký hiệu, nên dựa vào bản nào?

- A. A vì ký hiệu ở cao nhất như sao thật.

- B. B vì vị trí nét trùng vùng còn thấy được trên bản gốc.

- C. C vì khác hai bản còn lại nên dễ phân biệt.


**Đáp án:** B — Vị trí nét còn lại của bản gốc là bằng chứng trực tiếp, không phải sở thích về hình sao.

**Gợi ý:** Đối chiếu vị trí nét còn nguyên trên bản gốc.

### CM_GJ_01_V2

**Manh mối phải thấy trước:** Mảnh ghi chép G5 dùng ký hiệu sao có một nét khuyết ở trái; bản tại G10 có cùng vết khuyết, còn bản tại G6 có hai vết khác.

**Cách trình bày manh mối (đề xuất):** Cho xem chi tiết khuyết của cả ba mảnh đã thu, không làm thay puzzle G4.

**Hỏi:** Để xác nhận có hai bản xuất phát cùng một khuôn, nên bắt đầu đối chiếu cặp nào?

- A. G5 với G10 vì có cùng vết khuyết đặc trưng.

- B. G5 với G6 vì được tìm gần nhau trong hành trình.

- C. G6 với G10 vì tổng số nét khuyết nhiều hơn.


**Đáp án:** A — Vết khuyết hiếm và giống nhau đáng tin hơn thứ tự thu nhặt hoặc số lượng vết.

**Gợi ý:** Dấu lỗi trùng nhau có thể tiết lộ cùng nguồn.

## CM_GJ_02 — Dấu vết trên tượng (gyeongju, G7)

**Gate cần xác minh:** Chỉ sau nhánh G7 đã được giải quyết; đọc trạng thái thực tế sửa/đánh, không buộc một nhánh duy nhất  
**Gốc cốt truyện:** G7 tượng có nhánh sửa sau G12 hoặc chiến đấu; G12 gắn ấn tín Huế.  
**Phản hồi gợi ý:** Thoại Min/Lumi phản ánh nhánh thật, tuyệt đối không set flag kết bạn cho nhánh chiến đấu.

### CM_GJ_02_V1

**Manh mối phải thấy trước:** Sau xử lý G7, trên bệ có hai dấu: vết nứt cũ phủ bụi và vệt nhiễu mới nằm chồng lên mặt đá; Min đánh dấu cả hai trên bản phác.

**Cách trình bày manh mối (đề xuất):** Cho nhìn 2 lớp vết đánh dấu rõ và phản hồi phù hợp nhánh, không thay kết quả G7.

**Hỏi:** Muốn xác định dấu nào xuất hiện trước sự cố ánh sáng, chứng cứ nào đáng tin nhất?

- A. Vết có màu đậm nhất vì đậm là cũ.

- B. Vết nằm cao hơn vì ở trên tượng thường xuất hiện trước.

- C. Vết nứt dưới lớp nhiễu mới vì hai lớp có quan hệ chồng phủ.


**Đáp án:** C — Quan hệ lớp phủ cho biết thứ tự tương đối; màu hay vị trí cao thấp không đủ chứng cứ.

**Gợi ý:** Hãy xem dấu nào nằm dưới dấu nào.

### CM_GJ_02_V2

**Manh mối phải thấy trước:** Lời giải G12 xác nhận nhịp ấn Huế từng giữ trạng thái bảo vệ; hồ sơ tượng G7 ghi đổi hành vi chỉ sau khi lõi lỗi xuất hiện.

**Cách trình bày manh mối (đề xuất):** Cho xem lời ghi G12 và nhật ký sau khi G7 resolved; nội dung tương ứng cả nhánh chiến đấu.

**Hỏi:** Từ hai mẩu bằng chứng, kết luận nào được hỗ trợ tốt nhất?

- A. Tượng luôn là kẻ thù nên không cần xét thời điểm bị nhiễu.

- B. Hành vi bất thường có liên quan thời điểm lõi lỗi, chưa đủ bằng chứng tượng vốn xấu.

- C. Ấn tín Huế đã trực tiếp điều khiển mọi bức tượng từ đầu.


**Đáp án:** B — Thời điểm thay đổi gợi quan hệ với nhiễu; chưa chứng minh bản tính của tượng hay lỗi của ấn tín.

**Gợi ý:** Phân biệt điều dữ liệu chứng minh với điều mình suy đoán thêm.

## CM_SL_01 — Hai lớp hồ sơ (seoul, S2)

**Gate cần xác minh:** Chỉ sau khi đọc hồ sơ tại S4 và trở lại tương tác S2, hoặc nhiệm vụ đã mở thông tin tương đương; không spoil SQ02  
**Gốc cốt truyện:** Quản thủ S2, hồ sơ S4, SQ02 về lệnh phòng vệ cũ bị sửa.  
**Phản hồi gợi ý:** Quản thủ phản hồi và màn hình hồ sơ nhấp nhẹ nếu asset đã tồn tại; không reveal SQ02 sớm.

### CM_SL_01_V1

**Manh mối phải thấy trước:** Hai bản hồ sơ S4: bản lưu gốc ghi "bảo vệ đèn" và mốc thời gian trước biến cố; bản sao mới ghi "ngắt tất cả tín hiệu lạ" với dấu sửa sau biến cố.

**Cách trình bày manh mối (đề xuất):** Cho người chơi ĐỌC bản ghi S4 có mốc trước câu hỏi; nếu hiện không có, ghi NEEDS_CONTENT_CUE.

**Hỏi:** Muốn biết lệnh nào đã tồn tại trước biến cố, nên đối chiếu bộ dữ liệu nào?

- A. Bản có câu dài hơn và chữ in mới hơn.

- B. Chỉ bản mà hệ thống đang thi hành hiện tại.

- C. Nội dung từng phiên bản kèm dấu sửa và mốc thời gian.


**Đáp án:** C — Lịch sử phiên bản phân biệt lệnh gốc với lệnh bị thay sau đó.

**Gợi ý:** Cùng một hồ sơ có thể bị sửa; cần biết khi nào.

### CM_SL_01_V2

**Manh mối phải thấy trước:** Một bản được ký xác nhận trước biến cố, bản kia sao chép sau biến cố nhưng thiếu dấu xác nhận, dù hai bản đều có tiêu đề giống nhau.

**Cách trình bày manh mối (đề xuất):** Cho xem dấu xác nhận của tài liệu nguồn trong S4 và bản sao thiếu dấu.

**Hỏi:** Nếu phải xác minh bản nào gần nguồn gốc đáng tin hơn, làm gì trước?

- A. Đối chiếu dấu xác nhận và chuỗi thời gian của cả hai bản.

- B. Chọn bản mới nhất vì bản sau luôn cập nhật đúng.

- C. Chọn bản có tiêu đề in rõ nhất vì dễ đọc.


**Đáp án:** A — Dấu xác nhận và mốc thời gian giúp phân biệt bản gốc với sao chép chưa được kiểm chứng.

**Gợi ý:** Bản sao có thể mới hơn nhưng không nhất thiết đáng tin hơn.

## CM_SL_02 — Ji-won dò tín hiệu (seoul, S7)

**Gate cần xác minh:** Chỉ sau khi người chơi đã thấy thông tin từ ít nhất 2 nguồn relay; không cho phép câu hỏi tự hoàn tất relay  
**Gốc cốt truyện:** Ji-won S7 hướng dẫn 3 rơle A Đà Lạt nước, B Huế ấn, C Jeonju Hanji.  
**Phản hồi gợi ý:** Ji-won thay một câu thoại giải thích tuyến; KHÔNG đổi relay_a/b/c.

### CM_SL_02_V1

**Manh mối phải thấy trước:** Ji-won cho bảng kiểm: tín hiệu có ký hiệu nước nhưng ghi nguồn "Jeonju"; bản gốc tuyến ghi nguồn nước từ Đà Lạt, còn Jeonju gửi mạch Hanji.

**Cách trình bày manh mối (đề xuất):** Cho người chơi xem bảng tuyến A/B/C chính thức trong S7 trước moment.

**Hỏi:** Muốn xác định bản tin ghi sai tên nguồn, phải so cặp thông tin nào?

- A. Màu màn hình và độ dài bản tin.

- B. Ký hiệu trong tín hiệu với bảng tuyến A/B/C đã đối chiếu.

- C. Tên người đọc bản tin với kích thước máy phát.


**Đáp án:** B — Tín hiệu nước khớp tuyến Đà Lạt; nhãn Jeonju mâu thuẫn với nội dung.

**Gợi ý:** Nguồn nào được ghi cho từng loại tín hiệu?

### CM_SL_02_V2

**Manh mối phải thấy trước:** Hai bản ghi cùng tín hiệu Hanji: bản X đến từ tuyến C đúng dấu gấp; Y gắn nhãn tuyến A nhưng vẫn mang dấu Hanji.

**Cách trình bày manh mối (đề xuất):** Cho xem nhãn tuyến trong log S7 và dấu Hanji đã biết từ J12/S18.

**Hỏi:** Khi cần kiểm tra tuyến bị gắn nhãn sai, nên ưu tiên xem bản nào?

- A. X vì đã đúng tuyến nên ít bất thường.

- B. Cả hai giống nhau vì hình Hanji xuất hiện ở cả hai.

- C. Y vì dấu Hanji của tuyến C lại xuất hiện dưới nhãn tuyến A.


**Đáp án:** C — Đối chiếu nhãn tuyến với dấu nội dung phát hiện bản Y bất thường.

**Gợi ý:** Điều gì không khớp giữa nhãn nguồn và dấu nội dung?

## CM_SL_03 — Một mạch, nhiều ký ức (seoul, S18)

**Gate cần xác minh:** Chỉ sau S18 puzzle_relay_jeonju solved và người chơi có ba thông tin nguồn; không thay kết quả rơle C  
**Gốc cốt truyện:** S16 relay Đà Lạt, S17 Huế, S18 Jeonju; tín hiệu cross-cities đến Seoul.  
**Phản hồi gợi ý:** Một hoạt ảnh ký hiệu 3 tuyến sáng lần lượt nếu asset hỗ trợ; tuyệt đối không thay công tắc/quest.

### CM_SL_03_V1

**Manh mối phải thấy trước:** Bảng điều khiển Seoul ghi rõ: A dùng tín hiệu nước, B xác thực ấn tín, C đọc mạch Hanji. Một phiếu bảo trì đã tráo nhãn B và C, dù dây nối không đổi.

**Cách trình bày manh mối (đề xuất):** Sau khi đủ điều kiện S18, cho xem bảng A/B/C và nhãn bị tráo trong phần phụ.

**Hỏi:** Muốn sửa phiếu bảo trì mà không động tới hệ thống đã đồng bộ, cách nào đúng?

- A. Ghi lại B=ấn tín Huế, C=mạch Hanji Jeonju như bảng nguồn.

- B. Đổi thật hai rơle B và C để khớp phiếu bị tráo.

- C. Giữ phiếu cũ vì cả hai đều phát ánh sáng.


**Đáp án:** A — Chỉ nhãn trên giấy sai; thay dây rơle thật sẽ phá đồng bộ đang đúng.

**Gợi ý:** Tìm thứ sai: bản ghi hay kết nối vật lý.

### CM_SL_03_V2

**Manh mối phải thấy trước:** Ở bảng ký ức cuối tuyến: Hội An khôi phục đèn, Đà Lạt gửi nguồn nước, Huế xác thực ấn, Jeonju gửi mạch, Gyeongju đưa ống kính quan sát.

**Cách trình bày manh mối (đề xuất):** Cho hiện 5 biểu tượng đã được giới thiệu trong quest trước đó, nếu người chơi chưa đến đủ thì ẩn variant.

**Hỏi:** Một nhãn ghi "ống kính Silla đến từ Jeonju". Nên sửa nó dựa vào dấu vết nào?

- A. Đổi tên ống kính thành mạch Hanji nhưng giữ nguồn Jeonju.

- B. Đổi nguồn thành Gyeongju, nơi Lumi đã ghép ba biểu tượng sao.

- C. Giữ nguyên vì Jeonju cũng có hoa văn và ánh sáng.


**Đáp án:** B — Ống kính gắn với ba biểu tượng sao tại Gyeongju; Hanji là mạch của Jeonju.

**Gợi ý:** Nhớ nơi nào Lumi thực sự ghép các biểu tượng sao.
