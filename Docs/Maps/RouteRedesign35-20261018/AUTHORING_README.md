# Historical authoring inputs

`proposed-gameplay.json` là bản đề xuất lịch sử từ snapshot 12 check-in, KHÔNG phải dữ liệu hiện hành. Không copy đè runtime. `apply_route35_geometry.py --apply` có guard hash để từ chối ghi đè các thay đổi sau snapshot. Dữ liệu authoritative: `Assets/_Game/Resources/Content/GameplayContent.json` với đủ 18 check-in được người dùng xác nhận. Bảng cuối: `edge-waypoint-diff.json`; nghiệm thu: `verification.json`. Không reapply recipe sau checkpoint.
