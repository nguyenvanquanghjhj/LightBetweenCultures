"""Editable pitch from existing LBC art/runtime evidence. Never modifies game assets."""
from pathlib import Path
import json,zipfile
from pptx import Presentation
from pptx.util import Inches,Pt
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import PP_ALIGN,MSO_ANCHOR
from PIL import Image

R=Path(__file__).resolve().parents[1];OUT=R/'Submission/KGW2026';OUT.mkdir(parents=True,exist_ok=True)
profile=json.loads((OUT/'PROFILE.json').read_text(encoding='utf-8-sig'))
P=Presentation();P.slide_width=Inches(13.333333);P.slide_height=Inches(7.5)
TEAL='163E42';DEEP='102D32';CREAM='FFF4DC';GOLD='EFB963';INK='23494B';MUTED='B9D3CD';WHITE='FFFFFF'
refs=[]
def box(s,x,y,w,h,color=TEAL,radius=False,line=None):
 q=s.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE if radius else MSO_SHAPE.RECTANGLE,Inches(x),Inches(y),Inches(w),Inches(h));q.fill.solid();q.fill.fore_color.rgb=RGBColor.from_string(color)
 if line:q.line.color.rgb=RGBColor.from_string(line)
 else:q.line.fill.background()
 if radius:q.adjustments[0]=.08
 return q
def txt(s,x,y,w,h,value,size=22,color=CREAM,bold=False,align=PP_ALIGN.LEFT):
 q=s.shapes.add_textbox(Inches(x),Inches(y),Inches(w),Inches(h));t=q.text_frame;t.clear();t.word_wrap=True
 t.margin_left=t.margin_right=Inches(.02);t.margin_top=t.margin_bottom=0
 for i,line in enumerate(value.split('\n')):
  p=t.paragraphs[0] if i==0 else t.add_paragraph();p.text=line;p.alignment=align;p.font.name='Arial';p.font.size=Pt(size);p.font.bold=bold;p.font.color.rgb=RGBColor.from_string(color);p.space_after=Pt(size*.35)
 return q
def pic(s,path,x,y,w,h,cover=False):
 path=R/path if not isinstance(path,Path) else path;assert path.is_file(),path
 iw,ih=Image.open(path).size;ratio=iw/ih;target=w/h
 if cover:
  q=s.shapes.add_picture(str(path),Inches(x),Inches(y),width=Inches(w),height=Inches(h))
  if ratio>target:q.crop_left=q.crop_right=(1-target/ratio)/2
  else:q.crop_top=q.crop_bottom=(1-ratio/target)/2
 else:
  dw,dh=(w,w/ratio) if ratio>target else (h*ratio,h)
  q=s.shapes.add_picture(str(path),Inches(x+(w-dw)/2),Inches(y+(h-dh)/2),width=Inches(dw),height=Inches(dh))
 refs.append(path.relative_to(R).as_posix());return q
def slide(title,kicker='LIGHT BETWEEN CULTURES',subtitle=''):
 s=P.slides.add_slide(P.slide_layouts[6]);s.background.fill.solid();s.background.fill.fore_color.rgb=RGBColor.from_string(DEEP)
 box(s,.5,.38,.08,.38,GOLD);txt(s,.75,.36,11,.3,kicker,11,GOLD,True)
 txt(s,.72,.82,11.9,.65,title,30,CREAM,True)
 if subtitle:txt(s,.75,1.43,11.8,.46,subtitle,15,MUTED)
 box(s,.72,7.1,11.85,.012,'3D6264');txt(s,.75,7.19,10,.18,'LIGHT BETWEEN CULTURES  ·  KOREA GAME WEEK 2026',8,MUTED)
 txt(s,12,7.16,.55,.22,f'{len(P.slides):02}',10,GOLD,align=PP_ALIGN.RIGHT);return s
def notes(s,value):s.notes_slide.notes_text_frame.text=value
def shot(s,path,x,y,w=2.7,h=4.8):
 box(s,x-.07,y-.07,w+.14,h+.14,'31585A',True,GOLD);pic(s,path,x,y,w,h)
def card(s,x,y,w,h,title,body):
 box(s,x,y,w,h,TEAL,True);txt(s,x+.2,y+.18,w-.4,.6,title,22,GOLD,True);txt(s,x+.2,y+.95,w-.4,h-1.05,body,18,CREAM)
V='Validation/OwnerConfusion-20261020/'
C='Validation/ContestReadiness-20261020/'
J='signature-jeonju-final/' if (R/C/'signature-jeonju-final/P4-jeonju-trial-correct-before-confirm.png').exists() else 'signature-jeonju/'
hero='Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png'
lumi='Assets/_Game/Art/Characters/Lumi/Lumi_DialoguePortrait.png'
owner=profile.get('teamName') or profile.get('entrantName') or '[Điền tên cá nhân / đội dự thi]'

s=P.slides.add_slide(P.slide_layouts[6]);s.background.fill.solid();s.background.fill.fore_color.rgb=RGBColor.from_string(DEEP)
pic(s,hero,0,0,8.2,7.5,True);box(s,7.7,0,5.64,7.5,DEEP)
txt(s,8.12,.55,4.7,.35,'KOREA GAME WEEK 2026',13,GOLD,True)
txt(s,8.1,1.2,4.9,2.6,'LIGHT\nBETWEEN\nCULTURES',37,CREAM,True)
txt(s,8.12,4.0,4.55,.6,'Nhịp Cầu Ánh Sáng',23,GOLD,True)
txt(s,8.12,4.8,4.5,.9,'Casual adventure trên Android\nKhám phá văn hóa Việt Nam – Hàn Quốc',16,CREAM)
txt(s,8.12,6.3,3.3,.7,owner,13,MUTED);pic(s,lumi,11.8,5.7,1.1,1.5)
notes(s,'Mở bằng một câu: Tôi muốn người chơi bước vào văn hóa Việt–Hàn bằng những hành động nhỏ, thay vì chỉ đọc thông tin. Đây là gameplay Unity thật. Điền thông tin người dự thi trước khi nộp. Không gọi artwork là bản vẽ tay.')

s=slide('Văn hóa được khám phá bằng hành động',subtitle='Một nhân vật, những cuộc gặp gỡ và một hành trình nối lại ánh sáng.')
shot(s,V+'fresh02/Owner-mai-ground-arrival.png',.85,2.05,2.55,4.53)
card(s,3.95,2.1,4.05,4.45,'Ý tưởng','Cùng Lumi giúp những người giữ nghề, ghép lại các dấu tích và mang ánh sáng trở về từng thành phố.')
card(s,8.25,2.1,4.05,4.45,'Trải nghiệm mong muốn','Chạm để đi.\nThử một thao tác ngắn.\nThấy kết quả của việc mình vừa làm.\nCó thể nghỉ rồi tiếp tục.')
notes(s,'Nêu vấn đề thiết kế: làm một game văn hóa nhưng vẫn có động lực chơi. Nội dung sáu thành phố là chiều sâu; trải nghiệm trước mắt chỉ dẫn một việc cụ thể. Không tuyên bố độ vui đã được người chơi độc lập nghiệm thu.')

s=slide('Lumi và những người giữ câu chuyện',subtitle='Bối cảnh cách điệu, nhân vật canonical đang xuất hiện trong game.')
pic(s,lumi,.95,2.05,3.2,3.5);txt(s,1.05,5.72,3,.7,'Lumi\nNhân vật người chơi điều khiển',19,CREAM,True)
card(s,4.55,2.12,3.65,4.4,'Cô Mai · Hội An','Người giữ tiệm đèn.\nLumi giúp cô tìm Mắt Đèn, rồi nối lại câu chuyện của phố cổ.')
card(s,8.55,2.12,3.65,4.4,'Hana · Jeonju','Người đồng hành trong hoạt động hoa văn và Hanji.\nNhững dấu chào từ Hội An được đưa vào cuộc giao lưu.')
notes(s,'Lumi là nhân vật chính, không phải companion AI. Cô Mai và Hana giữ nguyên danh tính trong source. Di vật và nghi thức là sáng tạo fantasy, không đại diện một nghi lễ lịch sử có thật.')

s=slide('Một vòng chơi dễ nắm bắt',subtitle='Người chơi luôn có một bước tiếp theo; phần sâu hơn được mở dần.')
steps=[('01','Chọn địa điểm','Chạm bản đồ để Lumi đi tới.'),('02','Gặp gỡ','Nhận lời nhờ hoặc đọc manh mối.'),('03','Tự thao tác','Quan sát, ghép, xoay hoặc tương tác.'),('04','Nhìn kết quả','Nhân vật phản hồi, ánh sáng thay đổi.'),('05','Giữ hành trình','Lưu tiến độ, di vật và bưu thiếp.')]
for i,(n,t,b) in enumerate(steps):
 x=.78+i*2.53;box(s,x,2.5,2.3,3.25,TEAL,True);txt(s,x+.18,2.68,1.8,.55,n,30,GOLD,True);txt(s,x+.18,3.55,1.95,.65,t,20,CREAM,True);txt(s,x+.18,4.38,1.95,1.05,b,16,MUTED)
txt(s,.85,6.2,11.8,.45,'Nội dung có chiều sâu; từng thao tác vẫn nhỏ và rõ ràng.',22,GOLD,True)
notes(s,'Định vị là casual adventure, không tự đổi thành hyper-casual. Không giới thiệu tất cả hệ thống ngay đầu bài. Thời lượng hoàn thành chương chưa được đo ở người mới; không hứa hoàn thành cả hai thành phố trong ba phút.')

s=slide('Hội An: từ một Mắt Đèn đến phố cổ hồi sáng',subtitle='Ba mốc khác nhau trong gameplay, không tự hoàn tất khi vừa nhặt vật phẩm.')
for x,path,label in [(1.0,V+'fresh02/H3-tap-ready-to-pick.png','Chạm vật phẩm · Nhặt Mắt Đèn'),(5.15,V+'fresh02/Owner-four-fragments-next-step.png','Đủ bốn mảnh · Biết nơi ghép'),(9.3,V+'main01/Casual-signature-hoi_an.png','Qua thử thách · Phố cổ phản hồi')]:
 shot(s,path,x,2.0,2.55,4.53);txt(s,x-.23,6.65,3.0,.3,label,13,GOLD,True,PP_ALIGN.CENTER)
notes(s,'Giải thích chính xác: trả Mắt Đèn cho Cô Mai khôi phục một vùng. H7 đồng bộ trạm và mở Huế/Jeonju theo quest cũ. Di vật đủ bốn mảnh, được ghép và vượt Dẫn Ánh Đèn mới phục dựng toàn Hội An. Ba ảnh là các mốc chơi, không phải bộ ảnh cùng camera để đo ánh sáng.')

s=slide('Jeonju: tự tay ghép hoa văn giao lưu',subtitle='Thủ công trở thành thao tác chơi, với phản hồi khi thử sai và khi khớp mẫu.')
shot(s,C+J+'P4-jeonju-trial-wrong.png',.9,2.05,2.6,4.62)
shot(s,C+J+'P4-jeonju-trial-correct-before-confirm.png',4.05,2.05,2.6,4.62)
card(s,7.35,2.1,4.85,4.5,'Từ quan sát đến hoàn thành','Xoay các thành phần theo mẫu.\nThử sai để nhận phản hồi.\nXác nhận kết quả để ghi nhận phục dựng.\n\nẢnh Windows Player thật, không phải mockup.')
notes(s,'Đây là thử thách Hanji hiện có. Đừng đọc đáp án hoặc mô tả nó là một trò chơi mới tạo cho video. Khi demo, để BGK thấy thao tác và phản hồi thay vì chỉ nhìn màn hình hoàn tất.')

s=slide('Sự kết nối đi xa hơn hai bộ cảnh nền',subtitle='Một dấu chào từ Hội An được dùng trong cuộc giao lưu tại Jeonju.')
shot(s,V+'main01/Casual-signature-hoi_an.png',1.0,2.0,2.55,4.53)
shot(s,C+J+'Casual-signature-jeonju.png',9.4,2.0,2.55,4.53)
txt(s,4.15,2.7,4.55,.8,'HỘI AN  →  JEONJU',25,GOLD,True,PP_ALIGN.CENTER)
txt(s,4.15,3.8,4.55,1.65,'Con Dấu Lời Chào bắt nguồn từ ghi dấu ở Chùa Cầu.\n\nDấu ấy tiếp tục câu chuyện trong hoạt động hoa văn Hanji.',21,CREAM,align=PP_ALIGN.CENTER)
notes(s,'Nguồn thật: fragment_jeonju_greeting_stamp có nguồn H6 và claim anchor giao lưu J2. Phân biệt sự kết nối văn hóa với điều kiện mở bản đồ: di vật không phải chìa mở thành phố hay vào boss. Không trình bày đây là phong tục lịch sử có thật.')

s=slide('Sáu thành phố, mỗi nơi một sắc thái',subtitle='Bản đầy đủ được giữ nguyên; phần trình diễn tập trung vào một lát cắt Việt–Hàn.')
for x,title,rows in [(.85,'VIỆT NAM',[('Hội An','Đèn lồng và phố cổ'),('Huế','Dòng sông và dấu ấn di sản'),('Đà Lạt','Hoa, sương và những tín hiệu nhỏ')]),(6.85,'HÀN QUỐC',[('Jeonju','Hanok, Hanji và thủ công'),('Gyeongju','Di tích và những ký hiệu tinh tú'),('Seoul','Relay, sự hội tụ và kết nối cuối')])]:
 txt(s,x,2.2,5.6,.5,title,23,GOLD,True)
 for i,(name,theme) in enumerate(rows):box(s,x,3.0+i*1.05,5.6,.85,TEAL,True);txt(s,x+.18,3.14+i*1.05,1.65,.5,name,19,CREAM,True);txt(s,x+1.85,3.15+i*1.05,3.5,.5,theme,16,MUTED)
notes(s,'Không dùng số lượng node để thay cho giá trị trải nghiệm. Có sáu thành phố không đồng nghĩa BGK phải chơi hết mới hiểu game. Bản demo/video nên cho thấy Hội An và Jeonju trước; phần còn lại trả lời khi được hỏi.')

s=slide('Thiết kế cho người chơi biết phải làm gì',subtitle='Những sửa đổi xuất phát từ lượt chơi thật của chủ dự án.')
shot(s,V+'fresh02/Owner-fragment-markers-start.png',1,2.05,2.6,4.62)
shot(s,V+'fresh02/Owner-four-fragments-next-step.png',4.18,2.05,2.6,4.62)
card(s,7.4,2.1,4.8,4.5,'Rõ ràng ở từng bước','Mục tiêu chỉ người và địa điểm.\nDấu ! đỏ cho mảnh chưa lấy.\nĐủ 4/4 có nút chỉ nơi phục dựng.\nChạm vật phẩm để tự đến gần.\nNhật ký giúp tiếp tục sau khi nghỉ.')
notes(s,'Đưa ví dụ lỗi đã sửa để thể hiện cách làm việc với phản hồi. Không nói không còn lỗi. Android touch simulation trên Windows không thay thế kiểm thử thiết bị thật.')

s=slide('Game thật trong Unity, có thể tiếp tục hành trình',subtitle='Cấu trúc rõ giúp giải thích và xử lý câu hỏi kỹ thuật khi bảo vệ.')
card(s,.9,2.15,3.7,3.8,'Dữ liệu','Node và quest canonical.\nĐường đi có junction và thuyền.\nMột nguồn trạng thái cho di vật.')
card(s,4.83,2.15,3.7,3.8,'Tương tác','Chạm, kéo thả, xoay.\nPhản hồi đúng/sai và hướng dẫn theo ngữ cảnh.\nUI phản ánh trạng thái thật.')
card(s,8.76,2.15,3.7,3.8,'Lưu tiến độ','Đọc trạng thái đã lưu để hiển thị.\nChống nhận thưởng lặp.\nTiếp tục đúng điểm dừng.')
txt(s,.95,6.34,11.4,.45,'Có Windows Player và APK Android. Kiểm thử thiết bị Android thật còn cần hoàn tất.',17,GOLD)
notes(s,'Có thể giải thích luồng: input → route/service → state/save → UI. Nêu phần AI hỗ trợ trung thực nếu được hỏi. Không khẳng định tự viết tay toàn bộ source. APK khoảng 276 MiB ở bản 20/10, không phải toàn bộ thư mục project nhiều GB.')

s=slide('Ưu tiên chất lượng trước khi nộp',subtitle='Mốc làm việc của dự án; không phải thang điểm do ban tổ chức công bố.')
for i,(t,b) in enumerate([('DỄ HIỂU','Mở đầu rõ, nút có tác dụng và biết bước tiếp.'),('CHẠY ỔN','Kiểm tra touch, save/restart và phần demo trên điện thoại.'),('ĐÚNG CHỦ ĐỀ','Cho thấy hành động và sự trao đổi Việt–Hàn.'),('HỒ SƠ HỢP LỆ','PPTX, APK/video, đăng ký và chứng cứ nguồn tài nguyên.')]):
 x=.9+(i%2)*6.0;y=2.15+(i//2)*2.15;card(s,x,y,5.55,1.85,t,b)
notes(s,'Đóng băng mở rộng nội dung. Không thêm hệ thống lớn hoặc đại tu cả game trước hạn chỉ để khoe quy mô. Bằng chứng quyền sử dụng art/nhạc và thông tin người dự thi phải được chủ dự án xác nhận trước khi nộp. Ngày chung kết có bản ghi 5 và 6/12 khác nhau; hỏi lại BTC, không tự chốt lịch đi lại.')

s=slide('Người thực hiện & phần trình diễn',subtitle='Thông tin này cần điền chính xác trước khi gửi ban tổ chức.')
card(s,.9,2.2,5.55,3.8,'Cá nhân / đội dự thi',owner+'\n'+(profile.get('schoolMajor') or '[Trường / ngành học]')+'\n'+(profile.get('contact') or '[Thông tin liên hệ]'))
card(s,6.8,2.2,5.55,3.8,'Demo đề xuất','1. Nhận việc từ Cô Mai.\n2. Chạm và nhặt Mắt Đèn.\n3. Xem một bước phục dựng.\n4. Thao tác hoa văn ở Jeonju.\n5. Cho thấy kết quả và lưu tiến độ.')
txt(s,1,6.3,11.3,.45,'Video có thể biên tập chuyển cảnh; không giả thành một lượt chơi liên tục.',16,GOLD)
notes(s,'Điền tên từng thành viên và vai trò nếu thi đội, tối đa hai người theo thể lệ được cung cấp. Chỉ nói đã đăng ký sau khi người dự thi xác nhận. Không điền thông tin cá nhân suy đoán từ địa chỉ email tài khoản Google.')

s=slide('Một đốm sáng, một cuộc gặp, một sự kết nối',subtitle='Light Between Cultures · Nhịp Cầu Ánh Sáng')
pic(s,hero,.9,2.05,7.3,4.58,True);pic(s,lumi,8.7,2.15,3.2,3.7)
txt(s,8.4,6.0,4,.7,'Cảm ơn ban giám khảo.',22,GOLD,True,PP_ALIGN.CENTER)
notes(s,'Kết bằng trải nghiệm muốn đem lại, rồi mời chơi thử bản game. Phần hỏi đáp tập trung vào Unity, quản lý trạng thái, xử lý touch/route, chống thưởng lặp và cách văn hóa tham gia vào gameplay. Không hứa thứ hạng hoặc dựa vào số lượng nội dung để bảo đảm có giải.')

P.core_properties.title='Light Between Cultures — Korea Game Week 2026'
P.core_properties.subject='Editable idea presentation; entrant identity and rights evidence pending'
P.core_properties.author=profile.get('entrantName') or 'Light Between Cultures project owner'
P.core_properties.keywords='KGW2026; DRAFT; casual adventure; Unity; Android; Vietnam; Korea'
dest=OUT/'LightBetweenCultures_KGW2026_DRAFT.pptx';P.save(dest)
check=Presentation(dest);assert len(check.slides)==13
with zipfile.ZipFile(dest) as z:assert z.testzip() is None
(OUT/'SLIDE_SOURCES.json').write_text(json.dumps({'presentation':dest.name,'slides':len(check.slides),'assets':sorted(set(refs)),'sourceArtModified':False,'runtimeEvidence':'Windows Player; not Android-device evidence','entrantProfileComplete':bool(profile.get('entrantName'))},ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'path':str(dest),'slides':len(check.slides),'MiB':round(dest.stat().st_size/1048576,2),'zipValid':True}))
