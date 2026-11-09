"""Author only the additive Catventure catalog; never rewrites existing content."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
chapters = []
def chapter(city, title, opening, weather, nodes, cat, badge, postcard, next_city, sources):
    c = dict(cityId=city,title=title,opening=opening,weather=weather,rescueNode=nodes[-1],
             rescueText='Kéo mèo vào giỏ trú an toàn. Lumi và đội cứu hộ địa phương đã chuẩn bị sẵn vật dụng.',
             catId='cat_'+city,catName=cat[0],catLine=cat[1],catTrait=cat[2],spriteIndex=len(chapters),
             rewardCoins=8,badge=badge,postcard=postcard,nextCity=next_city,sourceUrls=sources,
             supplies=[],challenges=[])
    chapters.append(c)
    return c
def supply(c, key, name, node, description, art):
    c['supplies'].append(dict(id=c['cityId']+'_'+key,name=name,nodeId=node,description=description,artId=art))
def trial(c, key, node, mode, title, instructions, hint, fact, **extra):
    c['challenges'].append(dict(id=c['cityId']+'_'+key,nodeId=node,mode=mode,title=title,instructions=instructions,hint=hint,fact=fact,**extra))
def jigsaw(c,key,node,title,instruction,fact,image):
    trial(c,key,node,'jigsaw',title,instruction,'Quan sát tranh mẫu ở góc: các nét của hình phải nối liền nhau.',fact,
          imageId=image,tokens=['Mảnh trên trái','Mảnh trên phải','Mảnh dưới trái','Mảnh dưới phải'],targets=['','','',''])
def maze(c,key,node,title,instruction,fact,rows):
    trial(c,key,node,'maze',title,instruction,'Chỉ đi sang ô kề có đường. Tránh vùng tối và quay lại nếu gặp ngõ cụt.',fact,
          maze=rows,start=0,exit=len(rows)*len(rows[0])-1)

c=chapter('hoi_an','Thắp sáng phố Hội',
    'Mưa vừa dứt, những chiếc đèn đánh dấu lối đi đã tắt. Mèo đang đợi trên mái thấp cạnh bến nước! Cùng Lumi tìm vật dụng, khôi phục đèn và đón bạn về nơi khô ráo.',
    'rain',['H0','H11'],('Mèo Hội An','Meo! Có ánh đèn dẫn đường rồi!','Thích ngắm những chiếc đèn nhiều màu.'),
    'Người thắp đèn phố Hội','Hội An là đô thị thương cảng lịch sử. Những ngôi nhà, đường phố và công trình lưu lại dấu ấn giao lưu văn hóa.',
    'hue',['https://whc.unesco.org/en/list/948/'])
supply(c,'basket','Giỏ cứu hộ','H0','Giỏ khô lót vải mềm để đón mèo.','backpack')
supply(c,'lantern','Đèn dẫn đường','H1','Đèn được giữ trên bờ để đánh dấu lối đi.','relic:relic_hoi_an_lantern')
supply(c,'rope','Dây buộc giỏ','H11','Dây giữ giỏ chắc chắn trong cảnh cứu hộ.','item:copper_wire')
jigsaw(c,'lantern','H11','Sắc màu đèn lồng','Kéo bốn mảnh về đúng vị trí để khôi phục chiếc đèn dẫn đường.',
       'Đèn lồng là hình ảnh quen thuộc của phố Hội. Trong hành trình này, đèn giúp đội cứu hộ nhận biết lối đi đã chuẩn bị.', 'relic:relic_hoi_an_lantern')
trial(c,'bridge','H1','choice','Kiến trúc phố cổ','Công trình nào còn được gọi là cầu Nhật Bản ở Hội An?',
      'Chùa Cầu gắn với lịch sử giao lưu tại Hội An. Ngọ Môn thuộc Huế; Cheomseongdae ở Gyeongju.',
      'Chùa Cầu, còn gọi là cầu Nhật Bản, là một dấu ấn kiến trúc và giao lưu văn hóa tại Hội An.',
      options=['Ngọ Môn','Chùa Cầu','Cheomseongdae'],answer=1,imageId='checkin:hoi_an.h6')
trial(c,'food','H0','match','Hương vị phố Hội','Kéo mỗi thẻ món ăn vào mô tả phù hợp.',
      'Cao lầu có sợi mì và rau; mì Quảng thường có ít nước dùng; bánh mì có vỏ giòn và nhiều loại nhân.',
      'Cao lầu, mì Quảng và bánh mì là những món có thể gặp trong hành trình ẩm thực Hội An.',
      tokens=['Cao lầu','Mì Quảng','Bánh mì'],targets=['Mì, thịt và rau sống','Mì với ít nước dùng','Ổ bánh có vỏ giòn'],imageId='checkin:hoi_an.h4')
trial(c,'river','H1','scenario','Đêm hội hoa đăng','Lumi muốn xem hoa đăng và giữ dòng sông sạch. Bạn chọn cách nào?',
      'Tham gia hoạt động có người tổ chức, theo quy định địa phương và thu gom vật dụng sau hoạt động. Không thả nhựa hay xuống dòng nước.',
      'Có thể tìm hiểu hoa đăng qua mô hình trên bờ và các hoạt động được tổ chức. Trải nghiệm văn hóa cần đi cùng việc bảo vệ môi trường.',
      options=['Tự xuống nước nhặt đèn','Thả cả túi nhựa theo đèn','Xem mô hình và cùng thu gom vật dụng'],answer=2)

c=chapter('hue','Cuộc giải cứu bên sông Hương',
    'Mưa lớn trong câu chuyện làm lối ven sông bị ngập. Mèo đang trú trên một bục cao. Lumi cần chuẩn bị áo phao, tìm thuyền và chọn tuyến đã được đội cứu hộ đánh dấu.',
    'rain',['U0','U14'],('Mèo Huế','Cảm ơn vì đã chờ mình bình tĩnh bước vào giỏ.','Thích nghe tiếng nhạc dịu và nằm bên cửa sổ.'),
    'Bạn nhỏ bên sông Hương','Huế lưu giữ di sản cung đình, kiến trúc và âm nhạc. Quần thể di tích Cố đô Huế nằm bên dòng sông Hương.',
    'da_lat',['https://whc.unesco.org/en/list/678/','https://ich.unesco.org/en/RL/nha-nhac-vietnamese-court-music-00074'])
supply(c,'boat','Thuyền của đội cứu hộ','U0','Thuyền trong câu chuyện do người lớn của đội cứu hộ điều khiển.','resource:LeisureProps/Hue_PedalSwanBoat')
supply(c,'vest','Áo phao','U1','Áo phao dành cho chuyến đi cùng đội cứu hộ.','backpack')
supply(c,'blanket','Khăn khô','U2','Giữ ấm cho mèo sau khi về bờ.','item:healing_tea')
trial(c,'heritage','U1','match','Dấu ấn Cố đô','Nối di sản với thành phố phù hợp.',
      'Ngọ Môn và Hoàng thành thuộc Huế; Chùa Cầu thuộc Hội An.',
      'Ngọ Môn là cổng chính phía nam của Hoàng thành Huế. Chùa Cầu thuộc phố cổ Hội An.',
      tokens=['Ngọ Môn','Hoàng thành','Chùa Cầu'],targets=['Cổng ở Huế','Khu thành ở Huế','Cầu tại Hội An'],imageId='checkin:hue_u6_checkin')
trial(c,'food','U2','match','Hương vị Huế','Ghép món ăn với đặc điểm dễ nhận biết.',
      'Bún bò dùng sợi bún; bánh bèo thường ở chén nhỏ; bánh khoái là bánh chiên giòn.',
      'Bún bò Huế, bánh bèo và bánh khoái là những gợi ý để tìm hiểu ẩm thực xứ Huế.',
      tokens=['Bún bò Huế','Bánh bèo','Bánh khoái'],targets=['Món bún với nước dùng','Bánh trong chén nhỏ','Bánh chiên giòn'])
trial(c,'music','U0','choice','Giai điệu cung đình','Nhã nhạc gắn với không gian văn hóa nào?',
      'Tên gọi Nhã nhạc chỉ âm nhạc cung đình Việt Nam, có truyền thống được gìn giữ tại Huế.',
      'Nhã nhạc là âm nhạc cung đình Việt Nam, từng được dùng trong các nghi lễ của triều đình.',
      options=['Âm nhạc cung đình','Biển báo giao thông','Trò chơi điện tử'],answer=0,imageId='checkin:hue_u6_checkin')
maze(c,'river','U14','Tìm đường cứu hộ','Dẫn thuyền theo các ô đường sáng. Vùng tối là tuyến chưa được đội cứu hộ kiểm tra.',
     'Sông Hương gắn với cảnh quan Huế. Tuyến cứu hộ trong game là mô hình giả tưởng, không phải bản đồ an toàn ngoài đời.',
     ['...##','##.##','...##','.####','.....'])

c=chapter('da_lat','Tiếng chuông trong sương',
    'Sương phủ khu vườn và rừng thông. Mèo đi lạc khi đuổi theo một chiếc lá. Hãy tìm đèn, chuông và thức ăn để nhẹ nhàng gọi bạn về.',
    'mist',['D0','D18'],('Mèo Đà Lạt','Leng keng! Mình nhận ra tiếng chuông rồi.','Rất tò mò trước những chiếc lá thông.'),
    'Người dẫn đường trong sương','Đà Lạt có hồ Xuân Hương, rừng thông, những vườn hoa và nhà ga với hình dáng mái đặc trưng.',
    'jeonju',['https://vietnam.travel/place-to-go/south-central-coast-central-highlands/dalat/','https://dalat.vn/en/nhagadalat'])
supply(c,'lamp','Đèn pin','D0','Soi các biển chỉ đường trong câu chuyện.','item:fog_battery')
supply(c,'bell','Chuông gọi mèo','D5','Gọi nhẹ để mèo nhận ra hướng trở về.','item:stargazer_lens')
supply(c,'food','Thức ăn cho mèo','D3','Một phần ăn nhỏ của đội chăm sóc.','item:meat_portion')
trial(c,'flowers','D3','hidden','Thành phố ngàn hoa','Tìm ba bông hoa có dấu giọt sương trong khu vườn. Chạm để quan sát; đồ trang trí không phải manh mối.',
      'Quan sát những chấm sáng trên hoa. Không hái hoa hay nhầm diều và giỏ với bông hoa.',
      'Đà Lạt được biết đến với những vườn hoa. Quan sát tại chỗ giúp giữ vẻ đẹp của khu vườn.',
      tokens=['Hoa bên trái','Hoa cạnh giỏ','Hoa dưới tán lá'],tokenArt=['flower','flower','flower'],imageId='city:da_lat')
trial(c,'food','D0','match','Hương vị cao nguyên','Ghép món và nông sản với mô tả.',
      'Dâu tây là trái cây; bánh căn là bánh nhỏ nướng trong khuôn; bánh ướt là những lớp bánh mềm.',
      'Dâu tây, bánh căn và bánh ướt lòng gà là những gợi ý khám phá ẩm thực Đà Lạt.',
      tokens=['Dâu tây','Bánh căn','Bánh ướt lòng gà'],targets=['Quả đỏ có hạt nhỏ','Bánh nhỏ nướng khuôn','Bánh mềm với lòng gà'],tokenArt=['item:da_lat_strawberry','',''])
jigsaw(c,'station','D5','Dấu ấn nhà ga','Ghép tranh minh họa Ga Đà Lạt theo ba đỉnh mái nổi bật.',
       'Ga Đà Lạt có ba đỉnh mái đặc trưng. Bức tranh là minh họa giúp nhận biết hình dáng công trình, không phải bản vẽ kiến trúc.', 'station')
maze(c,'fog','D18','Bản đồ trong sương','Theo biển chỉ đường trong vườn để tìm mèo; tránh những ô bị sương che.',
     'Hồ Xuân Hương và rừng thông là những hình ảnh gắn với Đà Lạt. Khi thăm vườn, hãy ở trên lối đi dành cho khách.',
     ['.####','...##','##...','####.','.....'])

c=chapter('jeonju','Lễ hội làng Hanok',
    'Lễ hội sắp bắt đầu, nhưng mèo đã trốn giữa những mái ngói và làm rơi các dải trang trí! Giúp chuẩn bị món ăn, tìm dấu chân và đưa mèo về sân lễ hội.',
    'petals',['J0','J16'],('Mèo Jeonju','Mình sẽ ngồi yên nghe kể chuyện cùng bạn!','Mê những dải ruy băng và thích trốn sau giỏ.'),
    'Bạn của lễ hội Hanok','Jeonju nổi tiếng với làng Hanok, bibimbap và nghề làm giấy Hanji. Hanbok là trang phục truyền thống Hàn Quốc.',
    'gyeongju',['https://hanok.jeonju.go.kr/contents/en_facility','https://english.visitkorea.or.kr/svc/whereToGo/locIntrdn/rgnContentsView.do?vcontsId=110848','https://www.nfm.go.kr/k-box/ui/hanbok/male.do?lang=en'])
supply(c,'basket','Giỏ cứu hộ','J0','Chiếc giỏ quen giúp mèo yên tâm.','backpack')
supply(c,'ribbon','Ruy băng lễ hội','J1','Dải màu dùng để đánh dấu sân hẹn.','item:hanji_pattern_1')
supply(c,'food','Phần ăn cho mèo','J16','Thức ăn riêng của mèo, không lấy từ bát bibimbap.','item:meat_portion')
jigsaw(c,'hanok','J1','Bí mật Hanok','Ghép hình mái ngói làng Hanok. Quan sát đường mái trong tranh mẫu.',
       'Hanok là nhà truyền thống Hàn Quốc. Làng Hanok Jeonju là nơi tìm hiểu kiến trúc và nếp sống truyền thống.', 'checkin:jeonju_j11_checkin')
trial(c,'bibimbap','J0','drag','Đầu bếp nhí Jeonju','Kéo từng nguyên liệu vào phần tương ứng của bát mẫu. Đây là một cách sắp xếp để chơi, không phải quy tắc nấu ăn bắt buộc.',
      'Bát mẫu có cơm, rau, trứng và tương gochujang. Đọc nhãn từng ô rồi kéo nguyên liệu vào.',
      'Bibimbap là cơm trộn với nhiều nguyên liệu. Có nhiều biến thể; người ăn có thể chọn nguyên liệu phù hợp.',
      tokens=['Cơm','Rau củ','Trứng','Gochujang'],targets=['Nền cơm','Phần rau','Phần trứng','Phần tương'],imageId='city:jeonju')
trial(c,'hanbok','J1','hanbok','Sắc màu Hanbok','Ghép tên các phần trang phục, rồi chọn một phối màu cho Hanbok của bạn.',
      'Jeogori là áo, chima là váy và baji là quần. Không có một bảng màu duy nhất áp dụng cho mọi Hanbok.',
      'Hanbok gồm nhiều dạng trang phục: jeogori là áo, chima là váy, baji là quần. Màu và cách phối có thể thay đổi.',
      tokens=['Jeogori','Chima','Baji'],targets=['Áo','Váy','Quần'])
trial(c,'find','J16','hidden','Tìm mèo trong làng','Tìm ba dấu hiệu của mèo giữa sân Hanok: chóp tai, bàn chân và chiếc đuôi.',
      'Quan sát những ô có hình mèo; diều và chậu cây chỉ là đồ trang trí.',
      'Trong làng Hanok, hãy đi nhẹ và tôn trọng không gian sinh hoạt của người dân.',
      tokens=['Chóp tai sau mái','Chân bên giỏ','Đuôi cạnh hiên'],tokenArt=['cat','cat','cat'],imageId='checkin:jeonju_j11_checkin')

c=chapter('gyeongju','Bản đồ sao vương quốc Silla',
    'Mèo theo một vệt sáng rồi lạc vào khu vườn di tích. Những mảnh bản đồ bị gió tách rời. Quan sát di sản Silla, ghép manh mối và tìm mèo mà không chạm vào hiện vật.',
    'stars',['G0','G11'],('Mèo Gyeongju','Mình tìm được bạn nhờ những ngôi sao nhỏ!','Thích nhìn đom đóm từ một khoảng cách an toàn.'),
    'Người giữ dấu sao Silla','Gyeongju lưu giữ nhiều di tích của Silla. Cheomseongdae gắn với thiên văn, còn Bulguksa và Seokguram gắn với nghệ thuật Phật giáo.',
    'seoul',['https://whc.unesco.org/en/list/976/','https://whc.unesco.org/en/list/736/'])
supply(c,'map','Bản đồ khu vườn','G0','Bản sao dành cho khách, không phải hiện vật khai quật.','item:astral_script')
supply(c,'lamp','Đèn nhỏ','G1','Chiếu lên biển chỉ dẫn, không chiếu vào khách khác.','item:stargazer_lens')
supply(c,'basket','Giỏ đón mèo','G2','Đặt trên nền lối đi, cách xa hiện vật.','backpack')
jigsaw(c,'stars','G1','Nhà thiên văn nhí','Ghép tranh Cheomseongdae. Quan sát thân tháp cong và cửa mở trong tranh.',
       'Cheomseongdae là đài thiên văn cổ ở Gyeongju, nằm trong quần thể di tích lịch sử Gyeongju.', 'checkin:gyeongju_g4_checkin')
trial(c,'silla','G2','match','Bí mật vương quốc Silla','Nối mỗi dấu mốc với ý nghĩa của nó.',
      'Cheomseongdae gắn với quan sát trời; Daereungwon là khu lăng mộ; Silla là một vương quốc cổ.',
      'Gyeongju từng là kinh đô Silla. Khu vực lưu giữ di tích, lăng mộ và nghệ thuật của nhiều giai đoạn lịch sử.',
      tokens=['Cheomseongdae','Daereungwon','Silla'],targets=['Đài thiên văn','Khu lăng mộ','Vương quốc cổ'])
trial(c,'buddhist','G0','match','Kho báu Phật giáo','Kéo ba thẻ di sản vào đúng mô tả để khám phá Bulguksa, tháp đá và tượng Phật.',
      'Bulguksa là ngôi chùa; Dabotap là tháp đá trong khuôn viên chùa; Seokguram là hang động có tượng Phật.',
      'Bulguksa và Seokguram là hai di sản Phật giáo nổi bật gần Gyeongju. Dabotap là ngọn tháp đá nổi tiếng tại Bulguksa.',
      tokens=['Chùa Bulguksa','Tháp Dabotap','Seokguram'],
      targets=['Ngôi chùa Phật giáo','Tháp đá trong khuôn viên chùa','Hang động có tượng Phật'])
maze(c,'map','G11','Bản đồ cổ đại','Đi trên lối tham quan để tìm mèo. Các vùng tối là bồn cây và khu vực bảo tồn.',
     'Tôn trọng lối tham quan giúp bảo vệ di sản. Không đào tìm đồ cổ hay lấy mảnh đá khỏi di tích.',
     ['.....','####.','.....','.####','.....'])

c=chapter('seoul','Điểm trú ẩn giữa Seoul',
    'Cơn gió trong câu chuyện làm các biển chỉ dẫn bị lật. Mèo đang núp cạnh công viên. Tìm vật dụng, ghép ký hiệu và chọn cách giúp đỡ lịch sự để đưa bạn tới điểm trú.',
    'wind',['S0','S13'],('Mèo Seoul','Meo! Bên bạn là chỗ trú ấm áp nhất.','Thích ngắm thành phố từ bậu cửa trong nhà.'),
    'Nhịp cầu tình bạn','Seoul kết nối nhịp sống hiện đại với di sản. Hangeul là hệ chữ Hàn Quốc; những lời chào lịch sự giúp mở đầu một cuộc gặp.',
    '',['https://english.visitseoul.net/','https://www.korean.go.kr/eng_hangeul/principle/001.html'])
supply(c,'carrier','Hộp vận chuyển','S0','Hộp có chỗ thông khí và lót mềm.','backpack')
supply(c,'umbrella','Ô của đội hỗ trợ','S12','Vật dụng trong câu chuyện, được thu lại khi gió mạnh.','item:relay_a')
supply(c,'food','Thức ăn cho mèo','S13','Đặt gần hộp để mèo tự bước vào.','item:meat_portion')
trial(c,'storm','S12','hidden','Dấu vết sau cơn gió','Tìm ba mảnh biển có dấu sao để khôi phục chỉ dẫn trong công viên.',
      'Tìm các ký hiệu sao nhỏ. Những chiếc diều không phải mảnh biển chỉ dẫn.',
      'Công viên ven sông Hàn là một phần không gian xanh của Seoul. Giữ lối đi gọn giúp mọi người cùng sử dụng.',
      tokens=['Biển cạnh ghế','Biển sau chậu','Biển gần lối ra'],tokenArt=['star','star','star'],imageId='city:seoul')
trial(c,'hangul','S0','hangul','Hành trình Hangeul','Ghép ba chữ cái theo mẫu để tạo âm tiết han: h ở đầu, a ở bên phải, n ở dưới.',
      'Chữ Hangeul được kết hợp thành khối âm tiết. Mẫu han dùng phụ âm h, nguyên âm a và phụ âm cuối n.',
      'Hangeul kết hợp phụ âm và nguyên âm thành khối âm tiết. Bạn vừa ghép khối han, âm đầu trong tên Hangeul.',
      tokens=['h · hieut','a','n · nieun'],targets=['Phụ âm đầu','Nguyên âm','Phụ âm cuối'])
trial(c,'greeting','S12','scenario','Lời chào và sự giúp đỡ','Lumi gặp một người lớn ở điểm hỗ trợ. Cách mở lời nào lịch sự?',
      'Một lời chào lễ phép, cúi đầu nhẹ và hỏi xin giúp đỡ phù hợp hơn chen ngang hoặc ra lệnh.',
      'Cách chào thay đổi theo hoàn cảnh và quan hệ. Nói lễ phép, chào nhẹ và lắng nghe là lựa chọn phù hợp trong tình huống này.',
      options=['Chen ngang và ra lệnh','Chào, cúi đầu nhẹ rồi hỏi xin hỗ trợ','Giật đồ từ tay người khác'],answer=1)
maze(c,'shelter','S13','Tuyến đường tới điểm trú','Theo các biển sáng tới chỗ mèo; tránh ô có vật cản do gió làm đổ.',
     'Trong câu chuyện, đội hỗ trợ đã kiểm tra lối đi. Biển chỉ dẫn trong game không thay thế hướng dẫn an toàn thực tế.',
     ['...##','.#.##','.#...','.###.','.....'])

if __name__ == '__main__':
    out=ROOT/'Assets/_Game/Resources/Content/CatRescue.json'
    out.write_text(json.dumps(dict(version=1,chapters=chapters),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('Authored',len(chapters),'chapters,',sum(len(c['challenges']) for c in chapters),'challenges:',out)
