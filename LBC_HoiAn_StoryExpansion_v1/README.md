# Light Between Cultures – Story Expansion Hội An v1

**Đây là gói bổ sung CHƯƠNG TRUYỆN cho gameplay hiện có**, không chứa bản đồ hay scene Unity thay thế. Không xóa hoặc ghi đè file cũ.

## Có gì trong gói?

- `Docs/STORY_BIBLE_HOI_AN.md`: Cốt truyện chương Hội An, vai trò 22 node, 7 nhiệm vụ (bao gồm nhiệm vụ mới), liên kết Huế + Jeonju.
- `Data/hoi_an_chapter.json`: Story beats, bốn Key Gate, đề xuất hiện node theo tiến độ, 22 node (tọa độ thiết kế tham khảo), 7 quest, lời thoại có điều kiện/choice.
- `Data/items_additions.json`: Hành tím Hội An, Ớt Hội An, mồi cua, vật phẩm trao đổi xuyên thành phố, bưu thiếp.
- `Data/cross_city_food_hooks.json`: Sự kiện bổ sung cho U1 Huế, J15 Jeonju, H10 Hội An và H18; giữ NPC cũ.
- `Data/marker_rules.json`: Đổi dấu `!`, `...`, máy ảnh, vật phẩm theo trạng thái.
- `Docs/PROMPT_CODEX_IMPORT_STORY.md`: Prompt tích hợp vào source Unity cho Codex.
- `Docs/QA_ACCEPTANCE.md`: 20 tình huống nghiệm thu.

## Cách sử dụng

1. Giải nén `LBC_HoiAn_StoryExpansion_v1` vào gốc project Unity, ngang hàng `Assets`, cùng các thư mục LBC đã có.
2. Đưa nội dung `Docs/PROMPT_CODEX_IMPORT_STORY.md` vào Queue của Codex **sau** lượt đang chạy.
3. Codex phải nhập dữ liệu thông qua event/quest manager runtime hiện có. `trigger`, `condition`, `action` trong JSON là **ngôn ngữ đặc tả**, không phải API hiện thực sẵn. Không copy JSON này đè `HoiAnData.json`.
4. Hoàn thành HQ01 và HQ02, kiểm thử, checkpoint; các hook Huế/Jeonju tiếp tục khi map tương ứng có runtime.

## Giới hạn & lưu ý

- Kịch bản và hội thoại sáng tác dựa trên bản thiết kế LBC; một số nội dung như ớt, hành tím, J15/U1 là bổ sung mới, chưa có trong dữ liệu gốc.
- 22 tọa độ gốc là anchor gần đúng theo ảnh A; bản Unity hiện tại đã chỉnh node/waypoint. **Không tự động thay các vị trí runtime bằng anchor trong gói này**.
- Chưa có bằng chứng Unity build từ gói này. Không coi file JSON là game đã chạy.
- Các NPC ẩm thực và công thức giao lưu là sáng tác cho game, không phải giải thích lịch sử ẩm thực truyền thống.
