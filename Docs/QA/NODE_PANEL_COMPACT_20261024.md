# City Map node panel — 24/10/2026

Owner-authorized presentation patch after RC1. No Android build or full campaign.

## Findings and change

- Actual before captures from current RC1 source already omit the large generic badge at H7, H8 and H3; the existing no-NPC condition also covers H19. Do not interpret identical before/after captures as a newly removed icon. An older installed build was not examined in this task.
- `GameplayPanels.DialoguePortrait` now permits only real actor portraits (NPC, Lumi, mapped enemy); removes the `art.Kind` / generic puzzle/enemy fallback. Unknown events and category-only mappings have no large picture.
- `DrawDialogue` creates no empty portrait object, so speaker transitions cannot revive a previous portrait on information-only redraw. No portrait space is reserved when absent.
- Text retains full panel width, 24-unit side margins. Duplicate location/title rows collapse by 22 Canvas units, including H3 and H20; distinct location text remains and can use the full content width. Original skin, title, body, buttons, X and callbacks are preserved.
- Real Cô Mai and Bot Mờ-01 portraits retained. Combat/puzzle/activity panels and receipts untouched.

## Evidence

Windows development Player: focused **151 assertions PASS**, actual framebuffers **540×960 / 720×1280**. Seven panel fixtures at each size: H7 Trạm Đèn Hội Quán, H19 Ngõ gốm hoa, H20 Cổng đi Huế, H8 Cổng giao lưu Jeonju, H3 Hẻm mái ngói, H2 Cô Mai, H5 Bot Mờ-01.

- Panel bounds, text, symmetric margins, >=48-unit control heights; X closes and restores map input without changing the private earned save.
- Real mouse portal callback opens World Map without automatically changing city; simulated touch exploration callback opens HoiAnDemo at H3.
- Immediate NPC → information redraw has no remaining portrait; all 92 non-character event definitions across six cities resolve without a large category badge. This is a binding audit, not six-city gameplay QA.
- Source audit: **888 existing inputs unchanged**, only two existing source files modified plus the new QA partial and its `.meta`. No gameplay/quest/route/data/art/settings files changed. The complete `Execute`-and-following portion of GameplayPanels is byte-equivalent as normalized text.
- Private earned-save location fixtures are used for panel display; no claim of testing approach paths or a campaign. Mouse and simulated touch only; Android device NOT_TESTED and no APK rebuilt.

[Runtime before/after gallery](../../Validation/NodePanelCompact-20261024/preview.html). H19 has after-only images; H20 baseline was captured in its place before the fixture index was corrected. The remaining matched comparisons are real before/after captures. Logs, capture hashes and observations are in the adjacent `before` / `after` folders; `source-audit.json` records protected inputs.

## Files

- Runtime: `Assets/_Game/Scripts/UI/GameplayPanels.cs`.
- Focused QA: `Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs`, `GameplaySmokeRunner.NodePanels.cs` and its `.meta`.
- Documentation: this report; post-RC1 note in `RELEASE_FREEZE.md`.
- Evidence: `Validation/NodePanelCompact-20261024/` (before source copy, build/runtime logs, private saves, screenshots, diff, audit and preview).

Updated runnable Player: `Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe` (development QA build). Existing RC1 release ZIP and APK are unchanged and do not contain this post-RC1 patch. No additional game systems were reopened.

Recovery checkpoint: `D:/GAMEKKK/Backups/CheckpointNodePanelCompact-20261024`, incremental over verified `CheckpointRC1-20261024`; requires the existing full/incremental base chain. Estimated delta 30.1 MiB, limited to changed source/docs and this task's evidence.
