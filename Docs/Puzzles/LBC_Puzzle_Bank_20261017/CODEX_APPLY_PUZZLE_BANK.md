# LBC — TÍCH HỢP NGÂN HÀNG 150 CÂU ĐỐ TỔNG HỢP, CHỌN NGẪU NHIÊN AN TOÀN

Project: `D:\GAMEKKK\LightBetweenCultures`

## Tình trạng và mục tiêu

Đây là **tài liệu bổ sung cho dự án Light Between Cultures**, không thay thế bất cứ prompt Full Story/H3/terrain/portrait đang thực hiện nào. Không chen ngang khi Codex đang xử lý một công việc lớn; triển khai sau checkpoint an toàn. Đọc source và dữ liệu THỰC TẾ trong project, không mặc định ZIP tham khảo là phiên bản mới nhất.

Bộ bàn giao: `LBC_GENERAL_PUZZLE_BANK_V1.json` với **150 câu tiếng Việt thực sự đã được viết**, 15 chủ đề × 10 câu, mỗi câu có ID ổn định, 4 lựa chọn, vị trí đáp án, chuỗi đáp án, gợi ý, giải thích, độ khó 1/2/3. Có CSV để người dùng duyệt. **Dùng nguyên ngân hàng này, đừng tự tạo 150 câu mới bằng AI**; nếu phát hiện một câu sai/đa nghĩa thì liệt kê và sửa cục bộ kèm lý do, không âm thầm thay bằng câu chất lượng thấp.

## 1. Không được phá câu đố cốt truyện

Ở snapshot Assets đã đối chiếu, `Assets/_Game/Resources/Content/GameplayContent.json` có **22 puzzle** với các mode `sequence`, `choice`, `fold`, `rings`, `unconfigured`. `Assets/_Game/Scripts/Content/PuzzleManager.cs` gắn `id`, `requiredFlags`, `requiredItems`, `completionFlag`, `rewardItems`, `rewardCoins` và `Save.solvedPuzzles`.

- **KHÔNG** chọn ngẫu nhiên câu hỏi/đáp án cho `puzzle_paper` tại H13, `puzzle_pottery` H19, các mảnh Hanji, mật mã phá boss, ấn tín, cổng chương, rơle Seoul, key-gate và puzzle canonical phụ thuộc manh mối. Thay câu đố bắt buộc có thể tạo **softlock**.
- **KHÔNG** coi `unconfigured` là được phép thay ngẫu nhiên. Chúng có thể là các puzzle Gyeongju/Seoul đang được triển khai trong Full Story, phải để Full Story xử lý theo canonical.
- Ngân hàng mới chỉ dành cho **optional encounter/mini-quiz/đố vui với NPC, bảng đố, kho tri thức, trạm khám phá** và các điểm đã xác định rõ là không cản tiến độ. Không thêm hàng loạt node mới; chỉ gắn vào node có vai trò phù hợp hoặc thêm tương tác phụ, bảo toàn 133 node gốc.
- Nếu muốn dùng bộ random ở một gate bắt buộc mới trong tương lai, phải thiết kế manh mối, cách chơi lại và khả năng hoàn thành rõ ràng, xin duyệt riêng; **không áp dụng trong lượt này**.

## 2. Tích hợp theo nguồn dữ liệu, không hardcode câu hỏi trong C#

- Giữ JSON như **một resource riêng**, ví dụ `Assets/_Game/Resources/Content/LBC_GENERAL_PUZZLE_BANK_V1.json` hoặc vị trí nhất quán với content loader hiện tại. Ghi rõ đường dẫn final.
- Kiểm tra liệu hạ tầng `PuzzleManager`/Content loader/UI có thể mở rộng gọn hay cần một adapter cực nhỏ cho `single-choice quiz`. Ưu tiên dùng chung UI/dialogue/skin hiện có, không dựng một hệ manager/Canvas mới trùng chức năng.
- Không chép 150 câu vào source `.cs`, không làm thay đổi dữ liệu puzzle canonical 22 câu hoặc quét/ghi đè `GameplayContent.json` hàng loạt.
- Đảm bảo `id` duy nhất, `answerText == options[answerIndex]`, đúng 4 đáp án khác nhau, `hint` và `explanation` hiển thị đúng ngôn ngữ Việt. Không so đáp án bằng index UI sau khi đã xáo lựa chọn mà quên cập nhật mapping.
- Có chế độ debug hiển thị `questionId` để QA, nhưng không lộ đáp án trong UI người chơi.

## 3. Chọn ngẫu nhiên công bằng, không lặp vô lý

- Random theo ngân hàng nhưng phải **đúng ngữ cảnh**. Đề xuất trọng số theo khu vực: Hội An/ Huế ưu tiên `vietnam`, `logic`, `riddles`, `cipher`; Đà Lạt ưu tiên `nature`, `science`, `math`, `spatial`; Jeonju/Gyeongju ưu tiên `korea`, `mystery`, `patterns`, `language`; Seoul ưu tiên `technology`, `science`, `logic`, `world`. Luôn xen câu tổng hợp (không biến quiz thành kiểm tra học thuộc địa danh).
- Hỗ trợ 3 mức dễ/trung bình/khó; mức khó tăng dần theo tiến độ, không đánh đố quá sức ở đầu game. Mỗi phiên chơi có thể điều chỉnh tỉ lệ; không ép tất cả NPC phải ra quiz.
- Không chọn lại câu đã gặp cho đến khi các câu phù hợp đã dùng hết (nếu số câu đã gặp vượt số câu đủ điều kiện, được bắt đầu vòng mới). Có thể dùng shuffle bag theo city/category; tránh trùng ngay liên tiếp.
- **Một câu đã chọn cho một lần thử phải ổn định** qua đóng/mở panel và save/restart. Không cho reroll để tìm câu dễ chỉ bằng đóng panel. Khi cần lưu tiến độ mới, đảm bảo tương thích save cũ: trường mới tùy chọn + mặc định an toàn, hoặc tận dụng cơ chế lưu hiện tại nếu đủ; không phá schema.
- Dùng ID và seed ổn định, tránh phụ thuộc vào `string.GetHashCode()` vốn không luôn ổn định giữa tiến trình.
- Shuffle cả thứ tự lựa chọn mỗi lượt nhưng luôn kiểm tra đúng bằng **option ID/answer text gốc**, không lấy index trước khi shuffle.

## 4. Trải nghiệm người chơi

- NPC hoặc điểm đố có lý do để đặt câu hỏi (thử trí nhớ/giải mã/vui chơi), mở đầu ngắn và đúng bản sắc thành phố. Không ép người chơi làm 10 bài tập liên tục để tiến độ game tăng.
- Mỗi thử thách thường 1 câu, thỉnh thoảng 2–3 câu cho hoạt động phụ có thưởng. Câu hỏi và bốn đáp án hiển thị đẹp trong skin giấy kem–xanh ngọc–viền đồng; wrap và scrollbar theo 9:16, 20:9, landscape.
- Có nút **Gợi ý**, **Đáp án**, **Bỏ qua/Để sau** tùy độ khó và thiết kế; khi sai phải giải thích hoặc cho thử lại, không trừ mất vật phẩm nhiệm vụ. Hiển thị lý do đáp án đúng thay vì đơn thuần báo “Sai”.
- Giữ nút input, animation, âm thanh theo hệ UI hiện có; không che chân dung, không để các nút đáp án chồng lên dialogue. Không làm lại boss/bot/marker hoặc các asset canonical.
- Nếu hiển thị biểu tượng kiến thức (sao/đèn) thì dùng asset đã duyệt hoặc UI dùng chung; không tạo 150 ảnh minh họa bắt buộc.

## 5. Phần thưởng và chống khai thác

- Mini-quiz chủ yếu là nội dung khám phá vui, không bắt buộc thưởng xu/item. Nếu có thưởng, giới hạn số lần và chỉ thưởng một lần cho mỗi encounter/chu kỳ đã thiết kế, sử dụng cơ chế `GrantOnce`/ledger chính thức.
- Không viết thưởng trực tiếp vào callback hiệu ứng/nút đóng panel; không nhân đôi coin hoặc item khi reload/cancel/thử lại.
- Không cho farm xu vô hạn bằng cách đổi câu hoặc đóng/mở, không thay main quest item, `completionFlag`, `requiredFlags`, gate hoặc điều kiện boss.
- Người chơi có thể bỏ qua quiz phụ mà vẫn hoàn thành cốt truyện canonical.

## 6. Trình tự triển khai tiết kiệm tài nguyên

1. **Audit ngắn** source hiện tại, liệt kê chính xác node/NPC optional phù hợp; không scan lặp toàn dự án hoặc rebuild trước khi có sửa đổi.
2. Import dữ liệu và chạy validator đơn giản: đếm 150 ID, 4 lựa chọn, đáp án hợp lệ, Unicode tiếng Việt, chủ đề và độ khó; đối chiếu 22 puzzle canonical không bị sửa.
3. Thử tích hợp **một cuộc đố vui tùy chọn ở một node đã có**, chứng minh mở đúng panel, random/seed/shuffle, đáp án/hint, thoát, save/restart.
4. Sau khi phần mẫu PASS, mở rộng tới số điểm quiz phụ hợp lý (không bắt buộc cả 133 node). Ghi danh sách vị trí và lý do đặt câu đố, không thêm hàng chục điểm lặp.
5. Kiểm tra ba thành phố đại diện và một lượt quay lại thành phố cũ; kiểm tra không trùng ngay, không rớt câu hỏi, không farm thưởng, không bỏ qua gate, không cần full regression nhiều giờ nếu gameplay canonical không đổi.
6. Build cuối, ảnh nghiệm thu gọn: câu logic, văn hóa Việt, văn hóa Hàn, câu khó, lựa chọn bị shuffle, gợi ý, giải thích đúng/sai, một chuỗi save/restart. Chỉ báo PASS dựa trên runtime thật.

## 7. Bàn giao

- Bảng mapping `node/npc → category weights → số câu có thể xuất hiện`, phân biệt câu hỏi optional và puzzle canonical.
- Changelog code/data; manifest câu hỏi; ảnh/clip ngắn trước–sau; log test ngắn; danh sách câu cần người dùng duyệt vì đa nghĩa hoặc factual uncertainty.
- Không tự sửa nội dung cốt truyện, các map, nhân vật hoặc asset đã chốt.
- Không tạo full backup nhiều GB; chỉ checkpoint an toàn phù hợp quy trình đang có, không xóa checkpoint cũ.
- Khi còn Full Story/H3 đang chạy, **để phụ lục này ở trạng thái QUEUED/DOCUMENTED**, triển khai sau checkpoint an toàn, không chen ngang.

**ĐIỀU KIỆN HOÀN THÀNH:** Mọi câu được chọn từ bank thật, có đáp án đúng được kiểm chứng, hiển thị giải thích, có độ đa dạng, không lặp liên tục, không làm đổi bất kỳ puzzle/gate cốt truyện nào; bản build chạy được sau save/restart.
