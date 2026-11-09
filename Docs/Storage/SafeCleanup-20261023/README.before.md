# Light Between Cultures

Checkpoint gameplay mới: **Giai đoạn A + lõi B** — event theo dữ liệu, khám phá từng khu, HQ01/HQ02, bot/puzzle. Xem [GameplayABReport.md](GameplayABReport.md).

Prototype Unity `6000.6.5f1`, Universal 2D và Input System. Luồng hiện tại gồm menu, World Map, City Map Hội An, khám phá H3, nhiệm vụ cô Mai và lưu tiến trình JSON.

## Chạy bản hiện tại

1. Mở project `D:\GAMEKKK\LightBetweenCultures` bằng Unity Hub.
2. Mở [Prototype.unity](Assets/_Game/Scenes/Prototype.unity), nhấn **Play**.
3. Chọn **Bắt đầu khám phá → Hội An**. Các scene đã được thiết lập sẵn; không cần chạy lại công cụ tạo scene.

Hoặc chạy bản Windows: [LightBetweenCultures.exe](Builds/Windows/LightBetweenCultures.exe).

## Thử vòng chơi Hội An

1. Từ H0 chỉ thấy H0/H1/H2/H11. Đến H2, chọn **Nhận nhiệm vụ**; khu hẻm/chợ/xưởng mới hiện.
2. Đến H3, **Vào khám phá**, dùng WASD/click/touch, nhặt Mắt Đèn một lần rồi về H2 trả linh kiện mở đầu.
3. H4 giải thứ tự theo manh mối; H13 chọn mẫu giấy để lấy Lõi Giấy Sáng và mở khu tiếp theo.
4. H5 có Bot Mờ-01: **Giao chiến** với tấn công/phòng thủ/tia sáng/trà/rút lui. Hoặc hỏi ông Phúc H14, giải mật mã H19 rồi quay lại H5 chọn vô hiệu hóa.
5. Khi có Mắt Đèn + Lõi Giấy Sáng + Chìa Kết Nối, đến H7 **Lắp linh kiện · Thắp sáng**. HQ01 thưởng150xu, hiện H8/H20 và mở tuyến Huế/Jeonju; không ép rời Hội An.
6. Nhánh phụ: H10 nhận HQ02, H12 dùng mồi hoặc chiến đấu, đem dây đồng về H10; H9 được phát hiện. H11/H21 có rương một lần; H17 có quest5xu mở tuyến đò.

Nút **Nhiệm vụ** mở nhật ký tiến độ; **Túi đồ** hiển thị số lượng/xu/HP. Node và đường chưa khám phá không hiển thị hay nhận input. Khi gặp encounter bắt buộc giữa đường, Lumi dừng và chờ người chơi quyết định; không tự đi tiếp sau trận.

Save schema2 giữ city/node/visited/stage, quest, item/count, resolved event, puzzle, enemy và nhánh lựa chọn. Savev1 được chuyển tiếp, giữ nhiệm vụ mở đầu và vật phẩm. Kiểm thử dùng file riêng, không thay save người chơi.

## City Map Hội An

Nền sử dụng đúng `Maps/HoiAn/Map_Overview_A.png` từ bộ ảnh bàn giao. Map giữ **22 node H0–H21 và23tuyến đã hiệu chỉnh**, thêm một tuyến đò H17–H5 có điều kiện quest. Không đổi tọa độ22node.

- Khi chưa chọn, node chỉ hiện icon nhỏ; chạm node hiện tên và trạng thái trong tooltip nền tối bán trong suốt. Chạm nền, kéo map hoặc chọn **Toàn cảnh** để ẩn tooltip.
- Kéo để pan; cuộn chuột, chụm hai ngón hoặc nút **+/−** để zoom từ 1× đến 4×. **Toàn cảnh** hiển thị toàn bộ ảnh; **Lumi** đưa camera về vị trí hiện tại.
- Lumi có ba sprite riêng theo vai trò: chibi trắng độc lập trên map; hình navy giơ tay khi Lumi nói (giữ cả bàn tay); chibi đứng trong H3 với pivot chân và bước nhún nhẹ. Cô Mai có portrait speaker riêng.

H10–H21 lấy tên, loại và event ID từ gameplay kit chi tiết. Hội An đã có event runtime và các chuỗi cốt lõi; HQ03 xuyên Jeonju chưa hoàn tất. Năm thành phố khác hiện có catalog/scaffold, chưa chơi được và tuyến chưa được duyệt. Không coi133event được import là133địa điểm đã hoàn thiện. Mỗi node đã có trường `backgroundId` để gán nền riêng ở bước sau.

Chi tiết checkpoint gameplay và ảnh runtime: [GameplayABReport.md](GameplayABReport.md). Milestone01/CityMapPolish là lịch sử.

## Chỉnh dữ liệu và hình ảnh

- Dữ liệu runtime: [HoiAnData.json](Assets/_Game/Resources/HoiAnData.json).
- Chỉnh vị trí node và điểm gấp khúc đường: **Tools → Light Between Cultures → Edit Hoi An Map Layout**.
- Registry hình: [DesignKitArt.asset](Assets/_Game/Resources/DesignKitArt.asset).
- Áp lại ba vai trò Lumi: **Tools → Light Between Cultures → Apply Three Lumi Roles**.
- Báo cáo kiểm thử của lần chỉnh City Map: [Validation/CityMapPolish](Validation/CityMapPolish).
