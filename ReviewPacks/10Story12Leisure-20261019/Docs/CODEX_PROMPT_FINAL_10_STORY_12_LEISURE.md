# LIGHT BETWEEN CULTURES — CANONICAL REVISED PLAN: 10 STORY MINIGAMES + 12 NODE-TRIGGERED LEISURE EVENTS
## Replace 22 old quiz/puzzle UI without losing any of 22 legacy proofs, preserve six-city story/items/relics, add 12 optional experiences using the 12 user-approved decorative sprites

**Project:** `D:\GAMEKKK\LightBetweenCultures`.
**Official replacement:** This file **SUPERSEDES** all previous prompts titled `22 CULTURAL INTERACTIONS REPLACEMENT`, `22 PLUS 12`, and any instruction requiring 22 new playable minigames. Do not execute the obsolete specifications for all 22 activities. The original source remains authoritative for existing conditions, flags, rewards and route graph, not for obsolete input mechanics.

# 0. THE DESIGN DECISION — READ THIS FIRST

**Exactly TEN small story minigames, plus TWELVE optional leisure events across six cities.** Not 22 story minigames + 12 leisure events, and not 34 new tasks. The original 22 puzzle records/IDs remain *compatibility data*, NOT 22 separate mandatory playable challenges. Of these 22, 10 IDs get new small minigame UI, and the other 12 IDs are resolved by a lightweight context-appropriate *existing-node story interaction*; no new minigame UI or additional quest for those 12. **Retain all 29 pre-existing quests** and all original narrative/dependency logic; “10 nhiệm vụ” here means 10 replacement story minigames, NOT deleting 19 existing main quests.

**TWELVE leisure events** are wholly separate and optional. Two per city. User-supplied transparent decorative sprites are displayed **miniature on canonical City Map, NON-CLICKABLE**. They must have no independent hitbox, clickable marker, extra map pin, or new node/edge. User must select an **already existing, reachable/revealed node**, Lumi moves along the canonical route to that node, the existing node event/dialogue continues to work, and a separate option labeled naturally (e.g. “Trải nghiệm xích đu”) starts a **SEPARATE fullscreen/panel activity scene** similar in interaction pattern to the existing H3 exploration/lantern-pickup experience. In the panel only, Lumi performs the activity, or a faithful limited 2D representation if high-quality pose assets are missing. Close → return to the SAME map, camera, zoom and gameplay state. **Do not demand animation of Lumi seated/pedaling directly on City Map.**

**Absolute constraints:** Never replace/repaint six canonical map backgrounds. Never turn mini-sprite into game background. Keep **133 nodes, 155 edges**, route waypoints from the latest accepted patch, node arrival anchors, existing NPC/camera/check-in priority and all sound sources. Never create 12 new quests, give critical items for leisure, require leisure for six relics/ending, or display 12 extra main-quest markers. Do not confuse the 22 legacy completion records with the optional 12-leisure log. **Do not start until current route task is complete and no other task edits the same project.** Phase 5 ending and Phase 6/7 remain separately scheduled.

# 1. PRE-FLIGHT & SOURCE-OF-TRUTH

1. Audit current workspace at `D:\GAMEKKK\LightBetweenCultures`, latest route changes, Phase4, NPC/marker, SoftSfx, FourTrackMusic, and any finished Phase5 work. The uploaded source snapshot contained `GameplayContent.json` with **22 puzzles, 29 quests, 6 cities, 18 check-ins**, but re-read LIVE workspace; report discrepancies before editing.
2. Read actual `PuzzleManager`, `NodeEventManager`, `QuestManager`, `Inventory.GrantOnce`, save/restart, WorldMap, CityMap, existing H3 mini interaction, `CulturalRelics.json`, and data-dependent events. Audit all `puzzle:<id>` proof checks and world flags in all 24 relic sources. No assumption that descriptions in an old report override runtime.
3. Create minimal file manifest/hash and compact incremental pre-edit checkpoint from latest source. NEVER restore a prior checkpoint onto newer Phase4/NPC/route/audio/ending changes; protect all base dependencies.
4. Verify provided 12 separate transparent PNG sprites exist under `Sprites_12/`. `SourceSheets_6/` are backup/reference only; prefer the extracted 12 PNGs. Do not use AI-generated blue route maps as art. Import only the 12 approved leisure prop images through Unity with correct sprite import (including .meta) and explicit binding; never import them as background.
5. Prepare a 22-row dependency matrix with exact prerequisites, story event, completion flags and one-time reward IDs. `Docs/LEGACY_PUZZLE_DEPENDENCIES_22.csv` is a source-snapshot reference, not license to override live data. Identify duplicate event-versus-puzzle reward paths and preserve one-time semantics.
6. STOP only for a genuine blocking data conflict that cannot be reconciled without deleting gameplay progression. Otherwise implement a pilot, test and continue; document `NEEDS_USER_REVIEW` for uncertain cosmetic placements, not as an excuse to do zero work.

# 2. CLASSIFICATION — EXACTLY TEN STORY MINIGAMES

The following TEN original PuzzleIDs retain UI-based mechanics, accessible through their ORIGINAL nodes and guards. “UI activity” means **one short self-contained interaction** using existing interaction components, not another text question:

| # | City | Existing node | Keep PuzzleID | Replacement playable microgame | Must preserve |
|---|---|---|---|---|---|
| 1 | Hội An | H13 | `puzzle_paper` | Press/align flower-paper template, manufacture lantern core | Require H4 proof; `hoi_an.hanji_core_obtained`; `hanji_core ×1`, original HQ01 H7 gate |
| 2 | Huế | U16 | `puzzle_market_code` | Match two engraved patterns and stamp river travel pass | `hue.market_code_known`, `hue.courier_code_known`, `river_pass ×1`, U15/UQ03 |
| 3 | Huế | U17 | `puzzle_archive` | Reassemble torn letter and reveal faded ink | Have `lost_letter ×1`; `hue.archive_decoded`, `hue.secret_wall_found`, `hue.scholar_code_known` per actual live source; G12 and U8 downstream |
| 4 | Đà Lạt | D8 | `puzzle_mist` | Rotate/trace reflected light through 3 mist posts | Require `da_lat.glasses_returned`; `da_lat.fog_path_solved`, `da_lat.fog_cleared`, D14 chain |
| 5 | Jeonju | J7 | `puzzle_hanji` | Physically align and fold Hanji paper | `hanji_pattern_1 ×1`; `puzzle:puzzle_hanji` proof; later J12 |
| 6 | Jeonju | J12 | `puzzle_embroidery` | Fit three saved Hanji motifs into matching slots | Require `jeonju.yoon_clue_known`, `hoi_an.lamp_restored`, 3 `hanji_pattern` items; `jeonju.three_patterns_joined`, `hanji_circuit ×1` |
| 7 | Gyeongju | G4 | `puzzle_observatory` | Place three star marks and align observatory ring | Require `quest:GQ01`, 3 `star_symbol` items; `gyeongju.three_stars_placed`, `stargazer_lens ×1` |
| 8 | Gyeongju | G12 | `puzzle_cracked_seal` | Align Ancient Seal imprint with cracked inscription | Require `hue.star_script_known`, `puzzle:puzzle_archive`, `ancient_seal ×1`; `gyeongju.seal_decoded`; keep Korea-first reachable later |
| 9 | Seoul | S10 | `puzzle_tower` | Calibrate stargazer lens and converge 3 light rays | Require `stargazer_lens ×1`; `seoul.tower_calibrated`; Seoul stellar fragment proof |
| 10 | Seoul | S18 | `puzzle_relay_jeonju` | Bridge three gaps in Hanji relay circuit | Require `quest:SQ01`, `jeonju.beacon_lit`, `hanji_circuit ×1`; `seoul.relay_c_on`, `relay_c ×1`; preserve S16/S17 other two relays |

**Each must be genuinely interactable** by touch/mouse; do not auto mark it solved when the panel opens. Each must be completable in ~15–45 seconds with no tight timer; correct state commits once only. Use 4–6 reusable patterns (`DRAG_PLACE`, `INSPECT_REVEAL`, `ROTATE_ALIGN`, `CONNECT_TRACE`, `ASSEMBLE_FOLD`, `SLIDER_TUNE`) inside existing Canvas/Panel architecture; do not write ten unrelated frameworks. No A/B/C/D or code-word guessing. Existing six relic restoration trials remain independent and unchanged.

## Play sequence detail for these TEN

1. **H13 paper**: after the H4 vendor has provided the motif proof, drag the paper into the mold, rotate to match the drawn silhouette, then hold stamp to impress it. Clearly show `hanji_core` obtained only after valid placement. No extra workshop node.
2. **U16 pass**: inspect two pattern zones on cloth, drag the corresponding stamp to matching silhouettes on the permit, visually stamp and issue the existing `river_pass`, without spelling/number quiz. This permit must still work at U15 and not silently bypass ferry progression.
3. **U17 letter**: `lost_letter` must already be held; drag three torn edges until they fit, swipe reveal ink, then one short readable historical/fantasy story message. The letter must not be consumed unless live source explicitly consumes it; `archive_decoded` and any canonical variants must be synchronized by the same existing commit path.
4. **D8 mist**: use drag or tap-to-select reflectors to route light through three posts, confirming the path through an on-panel short mist-clear animation. This is NOT CityMap route geometry or new world lighting.
5. **J7 Hanji**: line up folding guides with a silhouette then swipe fold; if current J7 fold renderer can be reused, reuse it, but replace cryptic rotation count / text-answer UI with visible guide and fold feedback.
6. **J12 merge**: check that three patterns and appropriate intercity signal are really held/unlocked; drag 3 motifs into distinct shaped sockets, insert center symbol, produce one `hanji_circuit`. Do not consume original patterns unless current live game already does so.
7. **G4 observatory**: 3 collected star symbols → 3 matching sockets → align ring, then produce `stargazer_lens`. Preserve quest order and not introduce Huế prerequisite for accessing Gyeongju.
8. **G12 inscription**: show stone, stamp ghost outline, Ancient Seal drag-and-rotate then confirm, decode only when Huế proof and required letter/flag are legally acquired. Keep any existing `GQ02` alternative battle/hack logic intact.
9. **S10 tower**: mount `stargazer_lens`, rotate/calibrate 2–3 beams using simple control and finish; do not reuse or replace the six-relic Seoul restoration minigame.
10. **S18 circuit**: verify `hanji_circuit` and beacon flags, connect three segments then light relay C. Relay A/B stay obtainable independently at S16/S17 via lightweight story interactions; never grant three relays together or prematurely launch S20/S21.

# 3. THE OTHER TWELVE LEGACY PUZZLE IDs — RETIRE THEIR QUIZ UI, KEEP PROOFS AND REWARDS

**Important architecture:** The following TWELVE are NOT 12 extra puzzles, tasks, leisure minigames or required skill challenges. Their prior quiz UI is removed from normal gameplay. Their old `puzzle:<id>` proof / solved ID and exact flags/rewards stay **in the save model**, and are committed once via the legitimate existing event at that same original node. Use a brief normal *story interaction/choice* (a single “Quan sát”, “Xác nhận”, “Lấy”, “Đọc”, or similar action) or integrate with the existing event resolution. No new modal puzzle, no long sequence, no new node, no completion on unrelated game startup, and no rewards merely for being at the node. Do not invent 12 new side quests. Completed saves already holding proofs never replay/re-grant rewards. If source has a separate prerequisite for the fragment, honor it unchanged.

| City / node | Retired old PuzzleID | Legitimate lightweight resolution; no puzzle UI | KEEP existing evidence / item |
|---|---|---|---|
| Hội An H4 | `puzzle_market` | At the existing riverside market event, choose “Quan sát mẫu hoa văn”, show a short inspection/story line | `hoi_an.market_puzzle_solved`; H13 prerequisite; `fragment_hoi_an_old_flower_paper` proof |
| Hội An H19 | `puzzle_pottery` | Choose “Đọc dấu gốm” at pottery alley; learn the optional code alternative | `hoi_an.mo01_hack_code`; keep H5 fight/hack choice, do not set `mo01_disabled` |
| Huế U8 | `puzzle_court` | After decoding archive, view the light/shadow pattern and confirm the court record | require `hue.archive_decoded`; `hue.court_solved`; `fragment_hue_dragon_face` proof |
| Đà Lạt D12 | `puzzle_windmill` | “Mở hộc cối xay” on original D12 interaction, short turning-gear presentation | `da_lat.windmill_supplies_found`, `meat_portion ×1`, D3 recovery and fragment proof |
| Jeonju J3 | `puzzle_dye` | “Nhận mẻ vải đã nhuộm”, quick visual confirm at workshop | `jeonju.dye_restored`, `healing_tea ×1`, dyed-thread fragment proof |
| Jeonju J17 | `puzzle_lantern` | With Hội An beacon and Hanji circuit ready, “Thắp đèn song thành” | require `hoi_an.lamp_restored`, `jeonju.three_patterns_joined`, `hanji_circuit`; flag `jeonju.lantern_puzzle_solved` |
| Gyeongju G2 | `puzzle_pond` | “Quan sát phản chiếu” and read one line | `gyeongju.pond_hint`; no new gate |
| Gyeongju G15 | `puzzle_temple` | “Ghi lại bóng thềm đá” using existing stone terrace event | `gyeongju.temple_shadows_solved`, heritage-ring fragment proof |
| Gyeongju G17 | `puzzle_lantern_west` | With beacon and GQ04 conditions, “Thắp đèn bờ tây” | require `jeonju.beacon_lit`, `quest:GQ04`; `gyeongju.west_lantern_solved` |
| Seoul S3 | `puzzle_palace` | Once S4 archive and SQ02 conditions met, “Đọc thông điệp cung điện” | require `seoul.archive_read`, `quest:SQ02`; `seoul.palace_message_decoded` |
| Seoul S16 | `puzzle_station` | At S16 choose “Kích hoạt rơle A” if all old conditions satisfied; short pulse animation | `quest:SQ01`, `da_lat.beacon_lit`, `blue_water_signal`, `seoul.relay_a_on`, `relay_a ×1` |
| Seoul S17 | `puzzle_relay_hue` | At S17 choose “Xác thực dấu ấn Huế” if legal conditions satisfied | `quest:SQ01`, `hue.beacon_lit`, `ancient_seal`, `seoul.relay_b_on`, `relay_b ×1` |

**No hidden “auto-resolve every retired puzzle when player visits WorldMap”, NO blanket grants/migration for unsolved saves.** Existing node visited + prerequisites met + an intentional story choice/interaction → existing transaction method marks corresponding original PuzzleID solved/flags/reward. On reload, proof persists. The fact that no minigame is presented does not invalidate a legitimate contextual story event. Preserve original quest item consumption semantics, including `lost_letter`, `ancient_seal`, `hanji_circuit`, and flags. In particular **H4 must precede H13** and **U17 must precede U8/G12** whenever current data requires it. The old event should no longer push `mode:sequence` into PuzzlePanel for these 12; treat it as existing lightweight node story event (retaining IDs for validation/save compatibility).

# 4. TWELVE OPTIONAL LEISURE EVENTS — EXACTLY TWO PER CITY, NO MORE

## 4.1 Activation and drawing pipeline — unambiguous

1. `Sprites_12/<asset.png>` is a tiny **decorative image ONLY** on current City Map, stable under pan/zoom. `raycastTarget=false`, no clickable map actor collider/pointer handler, no new floating pin. Size/ground placement based on original art scale and actual walkable terrain. Do not obstruct existing marker, actor, or HUD. Do not modify canonical background texture, original source PNG or Unity import GUID for unrelated assets.
2. Bind the experience to an **EXISTING node ID**. Player clicks/taps the standard marker belonging to that node, not the prop. Lumi travels via ordinary accepted route to its ORIGINAL arrival anchor. Respect HIDDEN/REVEALED/unlock state.
3. Existing node's mandatory storyline/NPC/boss/loot/check-in/etc **MUST NOT BE REPLACED**. If the same node also supports leisure, offer secondary “Trải nghiệm” choice *after the canonical action is available/resolved* or in its normal node action selector; preserve priority and never allow leisure option to steal event trigger. On crowded nodes, use a small node action list/menu rather than a separate map pin. Opening a leisure panel must not grant story flags, loot, money or check-in.
4. Selecting experience opens a **SEPARATE interaction window/Canvas panel** using the precedent of H3's existing mini exploration/lantern pickup (reuse presentation pattern but DO NOT rewrite H3). **Mini sprite on map stays stationary** throughout; animation is in the activity panel only. Keep background scenic and themed, e.g. river, park, courtyard; do not zoom/crop entire canonical City Map as the minigame background.
5. On action completion (usually 3–8 seconds, some 10–15 max), close/replay/continue; restore City Map state, controls, audio, current city, node, camera, zoom, and NPC visibility. Exit early → no first-clear mark. Replay after clear allowed, no repeated collectible/daily reward.
6. Leisure progress can be tracked as **a small optional 0/12 counter in a journal sub-tab** if this fits the current collection architecture. **No 12 new quests, no markers, no additional story gate, no required reward, no new relics, no second ending.** If a subtab would force large UI rewrite, use a simple existing journal list instead; don't block implementing the playable events.

## 4.2 Recommended node assignments — adjust visually if necessary, NEVER add a node

These are recommendations grounded in existing node names, **not permission to shift node positions**. Codex should verify real canonical art, availability and clashes; if proposed spot visually impossible, pick a better EXISTING node in the SAME city and record why. Do not move/create nodes to follow mini prop.

| City | Event | Initial existing node | Sprite file | Need in panel |
|---|---|---|---|---|
| Hội An | Xích đu | `H9` (ngõ đèn thất lạc area; check space near but don't obstruct H9 chest) | `HoiAn_Swing.png` | Lumi gets on, sways ~3 s, hops off |
| Hội An | Thả đèn hoa đăng | `H16` (cầu tàu trung tâm) | `HoiAn_LanternRelease_Stand.png` | Pick lantern, touch river target, light drifts and glows |
| Huế | Đạp vịt/thiên nga | `U0` (bến sông phía phố) | `Hue_PedalSwanBoat.png` | Boat with Lumi seated, 3–5 s paddle across illustrative pond/river, back |
| Huế | Ngồi ngai vàng **mô phỏng** | `U8` (sân điện chính) | `Hue_ThroneReplica.png` | Lumi briefly sits on a replica/protected tourism photo spot; no sitting on actual heritage artifacts |
| Đà Lạt | Hái dâu | `D13` (vườn kính hoa, garden context) | `DaLat_StrawberryPatch.png` | Tap three ripe strawberries into basket, small hand/fruit animations |
| Đà Lạt | Thả diều | `D9` (đài nhìn thông) | `DaLat_KiteRack.png` | Hold or swipe to launch, kite climbs, floats ~3 s |
| Jeonju | Tưới cây | `J13` (sân cổ/cổng) | `Jeonju_WateringCan_Garden.png` | Tilt watering can; droplets onto 2–3 pots; visual response |
| Jeonju | Thưởng trà | `J15` (chợ hanok; preserve existing merchant/NPC event) | `Jeonju_TeaTable.png` | Pour and take a sip; small steam/pose motion |
| Gyeongju | Gõ chuông trải nghiệm | `G10` (đình trên đồi; exhibit only) | `Gyeongju_BellExhibit.png` | Tap/drag mallet, bell swings slightly, existing SFX or silent visual; not an ancient bell being physically struck |
| Gyeongju | Treo thẻ ước nguyện | `G11` (vườn đá trung tâm) | `Gyeongju_WishTags.png` | Choose decorative blank wish tag, attach to rack, flutter |
| Seoul | Thử tteokbokki | `S1` (quảng trường trung tâm) | `Seoul_TteokbokkiStall.png` | Pick up a piece with chopsticks, Lumi eats and reacts |
| Seoul | Sân khấu K-pop | `S7` (quảng trường màn hình; DO NOT replace Ji-won) | `Seoul_KpopStage.png` | Lumi completes 3 simple rhythm poses, optional prompts, visual disco effects without new copyrighted music |

**No forced distribution changes:** exactly 2 per city, 12 total. If an existing node contains NPC, check-in, boss, puzzle, item or gate, those remain primary; offer activity as secondary choice and test both paths independently. For H9, ensure chest click still resolves; for U8, U8 story proof still resolves; for D13, gardener NPC unaffected; for J15 and S7, merchant/Ji-won dialogue unaffected. Do not add 12 more check-ins.

## 4.3 Asset handling and proportions

The 12 PNG images are **map decoration + optionally zoomed prop art in the separate activity panel**, not animation frames/skeletons and not a replacement for the map. Each existing sprite has alpha transparency; use proper sprite slicing/padding and let its lower edge/footpoint align to flat walkable ground; constrain size based on nearby house/NPC scale. Avoid billboard-sized throne/giant swan. Preserve in-game art theme and only insert what fits. A small mismatch or malformed alpha should be reported, not corrected by painting over the map or replacing the art source. Use the images from `Sprites_12/`; `SourceSheets_6/` is reference. No new AI art in this task.

# 5. ANIMATING LUMI — WHAT CODEx CAN AND CANNOT DO WITHOUT NEW IMAGE ASSETS

**Yes, simple jumping/motion is possible in Unity with code**: tween anchoredPosition/Transform over a short parabolic arc, squash-stretch cautiously, rotate/rock for swinging, slide/float relative to water/boat, scale/shadow easing, loop simple arm motion where available, interpolate UI rig/layers. Use current canonical Lumi sprite/atlas, do not generate a new face/outfit. Existing user art remains authoritative.

**But a single full-body standing PNG cannot produce a real seated pose, pedaling legs, holding a watering can, or detailed three-step dance convincingly by transform alone.** Do NOT claim that translating/rotating the existing full-body image makes genuine limb animation; do NOT draw over Lumi, swap with arbitrary NPC art, or distort anatomy. First implement a **small visual pilot**: Hội An swing, Huế swan boat, and optionally Seoul K-pop dance. Use the 12 approved prop images (may magnify them in the panel), body alignment, seat foreground occlusion/cutout only where existing sprite structure supports it, gentle rocking/jump of Lumi with intact silhouette. For paddling, moving swan boat on the panel with Lumi upright or partly occluded is ACCEPTABLE as a prototype if it looks reasonably natural; label actual seated/pedaling pose `POSE_ART_PENDING` if not.

If a convincing seated pose/dance/fine limb movement needs new Lumi pose sprites, **stop expanding animation and report exactly which pose views are needed** (e.g. Lumi seated-side-facing, seated-forward, arm holding watering can, short jump or dance pose). Request user's separate art approval; do not spontaneously call image generation or fabricate final art. All twelve experiences can have simple environmental animation even when character pose remains limited; do not mark `CHARACTER_POSE_VERIFIED` for a static character. Existing 12 images alone are NOT guaranteed to solve character animation; they are props, not Lumi animation sheets.

**Avoid dynamic animation on City Map** for all 12 leisure actions. Map prop stays mini, static and not touchable. Only the separate activity view animates; after ending, return to map.

# 6. MAIN STORY, THE SIX RELICS AND PROOF COMPATIBILITY — ZERO REGRESSION

- Preserve 29 quests, 18 existing photo check-ins, 23 enemy types, six canonical relics / 24 fragments and their actual source proofs, restoration trials, cross-city exchanges, both VN_FIRST and KOREA_FIRST routes, World Map unlocking, H5 link key, and H6 rooftop/covered passage. Do not block Seoul reach or S20 boss behind all six relics.
- Preserve one ending ceremony decision if Phase5 was already applied; if Phase5 not yet present, do not create a new ending in this task. The old credit flag is not an excuse to auto-complete missing relics.
- A legacy puzzle ID can remain referenced from `proof` or `proofAny` when its UI is retired. Only **legal node story completion** or old completed save makes that proof true. Retained 22 IDs should not appear as 22 independent pending minigames in user-facing quest UI.
- Preserve original fragment dependencies including: H4 `fragment_hoi_an_old_flower_paper`; U8 `fragment_hue_dragon_face`; U16 `fragment_hue_ritual_cord`; U17 `fragment_hue_rhythm_inscription`; D8 `fragment_da_lat_mist_bell`; D12 `fragment_da_lat_blue_signal_seed`; J3 `fragment_jeonju_dyed_thread`; J7 `fragment_jeonju_old_hanji_core`; G4 `fragment_gyeongju_star_stone`; G15 `fragment_gyeongju_heritage_ring`; S10 `fragment_seoul_stellar_ring`; S18 `fragment_seoul_relay_frame`, and any other 24 sources in live canonical data.
- Retired lightweight proof can be a normal story action but may not bypass the original prerequisite inventory/quest/flag/location guard. DO NOT make an optional leisure event complete a legacy puzzle or vice versa. No free grants on new save, WorldMap visit, city enter, save migration or app startup.
- Item handoffs remain exactly as source, including `hanji_core`, `river_pass`, `lost_letter`, `ancient_seal`, `meat_portion`, three `hanji_pattern`, `hanji_circuit`, `stargazer_lens`, `blue_water_signal`, `relay_a`, `relay_b`, `relay_c`, and story one-time currencies/keys. Item consumed only where live source already consumes it.
- Use existing `Save.solvedPuzzles`, world flag setter, `Inventory.GrantOnce`, original reward ledger. Ensure idempotency across repeated node visits, close/reopen, process restart, old saves, and both route orders.

# 7. CRITICAL UX UPGRADES FROM PRIOR PROMPT — DO NOT OMIT

## 7.1 Next objective indicator (highest priority)

A compact existing-HUD line “Nhiệm vụ đang theo / Việc tiếp theo / Khu vực”. Use real QuestManager data and already revealed reachable content only. Example: “Đến xưởng giấy để hoàn thiện Lõi Giấy Sáng.”, “Quay lại gặp Lan sau khi tìm được kính.”, “Hiệu chỉnh ống kính tại tháp.” Never present raw H13/flag/SQ01 codes as primary player text; don't add a parallel quest manager. Leisure events are not prioritized as story objectives. Correctly update when original quest ends or new quest becomes active.

## 7.2 Three levels of optional hint

1) short goal, 2) city/area, 3) accessible hint / known clue; never reveal hidden nodes, teleport, assume a locked city is open, force an order across VN/KOREA branches or leak ending. Provide a button via existing UI and state dependent messages; no AI or external service.

## 7.3 Accurate destructive-reset warning

Current old line was “Xóa tiến độ Hội An và chơi lại từ H0?” while `ResetSave()` resets ALL cities. Replace with unambiguous warning covering **entire journey**: 6 cities, quests, items, relics, check-ins and 12 leisure progress. Buttons must be `Hủy — Giữ tiến độ` and `Xóa toàn bộ và chơi lại`; no one-tap destructive reset. Only call destructive reset after explicit second confirmation. Test Hủy leaves bytes/progression unchanged.

## 7.4 Safe save recovery

Audit main save and `.bak` fallback for corrupted primary, both corrupted, write interruption, missing optional fields, old completed/partially completed puzzle IDs, and new optional leisure log. Recover valid backup when possible; never silently overwrite a potentially recoverable save with empty new game, never crash due to absent leisure field. Avoid cloud/accounts. If schema evolves, migration must only initialize optional new fields, not auto-grant legacy completion.

## 7.5 Mobile portrait accessibility

Evaluate at 540×960, 720×1280, 1080×1920, 1080×2400; safe area/notch, Vietnamese diacritics/fonts, legible minimum text, scroll for long panels, adequately large touch targets, all H3-like experience windows, touch drag alternative tap-select then tap-target, close/back, focus state, map input disabled while panel open, restored afterwards. Preserve single EventSystem and existing audio service. Windows mouse ≠ real Android PASS. No more than two fingers needed; activities must not depend on keyboard or hover.

## 7.6 Good user feedback and small activities

Story minis ~15–45 seconds, leisure ~3–8 seconds (up to ~15 only where truly needed); no hard timer, no coin/HP loss for wrong input, no item consumption until committed correct, no repeated reward on replay. Clear pass/fail/cancel state and context-specific one-line “next step” after story action. Accessibility must not rely on color or audio alone. Prefer reuse existing seven SFX and four BGM; do not generate or re-map audio, do not alter BGM track logic.

# 8. PILOT FIRST, THEN EXTEND — TOKEN & RISK CONTROL

**Pilot A (story):** H4 lightweight market story resolution → H13 story minigame → full `hanji_core` / H7 gate test. Demonstrate that old `puzzle_market` proof survives without an extra quiz and H13 is one of only ten remaining mini-games.

**Pilot B (leisure):** H9 swing and U0 pedal-swan. On actual canonical maps show mini art stationary and NON-CLICKABLE with no `raycastTarget`/collider, selection only by clicking H9/U0 standard node, Lumi path/arrival unchanged, separate panel and animation, closing returns to same map, old H9 chest and U0 intro still work. Inspect actual Lumi character animation quality; if seated/pedal pose not credible, mark `POSE_ART_PENDING` without halting all other activities.

**Pilot C (save):** old save already solved H4/H13 and old save unsolved, correct no-double-reward path, process restart, optional 0/12 optional field. Check reset cancel behavior.

If pilot passes: implement remaining 9 story minigames, 11 remaining lightweight old-ID proof resolutions (12 total including H4), and 10 remaining leisure events (12 total including swing+boat). Group by city, targeted tests per group. Never start by creating 22 separate minigame windows. Limit new generic behavior/components; avoid 34 MonoBehaviours and giant test screenshots after each small change.

# 9. QA — DEFINE SUCCESS MEANINGFULLY

### Counting must be explicit

- Exactly **10 STORY_MINIGAME** playable puzzles (listed in §2).
- Exactly **12 LIGHT_STORY_RESOLUTION** retired puzzle UIs (listed in §3); keep all **22 legacy PuzzleIDs/proofs** valid in saves and story references.
- Exactly **12 LEISURE_EVENT** panels (listed in §4), 2 per city, all optional, NO clickable sprites/no extra map pins/new nodes.
- Exactly **6 RESTORATION** original relic trials, 24 fragments and 18 check-ins unchanged.
- `29` canonical quests unchanged; no “34 new mandatory tasks” or “12 leisure quests”. `133 nodes / 155 edges` unchanged, including movement changes already approved.

### For each of 10 story minigames and each of 12 lightweight stories

- Missing prerequisite disables legal completion. Existing accessible event gives informative text rather than a new blocker.
- On genuine success, original PuzzleID and world flags recorded, exact prior reward and one-time grant IDs, no phantom fragments.
- Wrong/cancel/close never completes minigame and never consumes required items.
- Post-complete revisit and new process do not pay again.
- Old saves already complete are recognized; old unsolved saves can complete via new approved route.
- Active required/reward source mapping matches the canonical source version, including G12, the three Seoul relays and rare intercity keys.

### For each of 12 leisure events

- Decorative mini sprite exists in appropriate terrain, rendered with correct sorting and pan/zoom; it is not a pointer target.
- Pointer on mini-sprite ALONE doesn't trigger leisure or steal raycast; ordinary map interactions unaffected.
- Selecting correct existing node causes ordinary Lumi movement to original arrival and then user-selectable activity. Hidden/locked node inaccessible as before.
- Existing story interaction at same node still available; leisure does not swallow NPC, boss, camera, check-in, loot, quest or portal action.
- Panel opens and plays with appropriate prop and believable motion; no immediate on-open win; early exit not counted.
- Complete stores optional record at most once; replay allowed. Nothing unlocks, no story item/coins/relic or 18-photo reward given.
- Map controls/camera/zoom/audio state restored correctly; process restart preserves optional record; 12/12 not required for boss/ending.

### Required regression chains

H4→H13→HQ01/H7; H19 fight/hack alternative; U16→river_pass→UQ03; U17→U8/G12; D8→D14, D12→D10/D3 recovery; J7→J12→J17→J19; G4→S10, G12/15/17 proofs; S16+S17+S18 produce `relay_a`,`relay_b`,`relay_c` independently; S20 win or repair and ability to travel after boss, S21 only Phase5 rules if already implemented. Test both VN_FIRST and KOREA_FIRST for no softlocks and cross-city return; six restored relics remain legally obtainable. Don't simulate all proofs = true and call that fresh-story E2E. At least targeted genuinely earned chains. Use private test saves and no destructive reset of user progress.

### Build / evidence truthfulness

Windows Editor/Player compile PASS is required. Export focused screenshots from actual runtime (not Photoshop/mockup), including a mini prop not clickable, H9/H16/U0/U8 coexisting events, one Lumi animation scene, one story minigame, a Seoul relay, journal 0/12, reset warning. Android physical touch/performance and natural subjective appearance remain `NOT_TESTED` until real device QA; don't call it Android PASS based on Windows touch simulation.

Protected source files must be byte-identical where appropriate: six map PNGs, original BGM MP3s, original SFX WAVs, untouched NPC portraits/sprites, original routes/node positions. Compare 133/155 graph data and actual waypoint arrays with *latest* accepted baseline, not a stale checkpoint. Don't rewrite Phase4/5 source, don't reset saves or artifacts.

# 10. REPORTS, DELIVERABLES & STOP

Create:
- `Docs/Gameplay/10_STORY_12_LEISURE_REPLACEMENT_REPORT.md` — 22 ID matrix (10 new UI + 12 lightweight), 12 leisure assets/node bindings/panel storyboard, original flags/rewards/fragment proof and tests, live version differences, any `POSE_ART_PENDING`.
- `Docs/QA/10Story12Leisure/` — focused final runtime PNGs, short Windows QA logs, touch simulations and protected-file hashes. No fake before/after.
- Incremental checkpoint from **latest** actual workspace with base chain/receipt; not a full copy of Unity Library/build/cache/history. Do not delete earlier backups.
- List exact changed production files and no-op/still pending items. Clearly identify screenshots/models tested; do not describe visual quality as PASS merely because assertions passed.

**Completion criteria:** 10 complete playable story minigames, 12 retired-UI proof paths with no duplicate rewards, 12 functioning optional node-triggered leisure panels, no clickable props, no new nodes, unchanged 29 quests, six relics/24 fragments and 18 check-ins, preserved crosscity items + two story orders + old saves, safe reset and save recovery, compact objective/hints, Windows targeted build and QA, honest Android/pose status. If a cosmetic prop cannot be placed without destroying canonical art, do not improvise a new background; report alternate existing node for approval.

**FINAL REPEAT:** mini prop is STATIC DECORATION, NOT A BUTTON. Existing node is the ONLY entry. Lumi moves to existing node and an activity WINDOW opens; there is NO requirement for Lumi to sit/pedal on the map. Total **10 story minigames + 12 leisure events**, NOT 22+12. Maintain all **22 legacy IDs/proofs**, flags, items and intercity dependencies. **DO NOT TOUCH THE SIX MAP BACKGROUNDS, 133 NODES, 155 EDGES, NPC, 18 CHECK-INS, SIX RELICS, BGM OR SFX.**

**Finish QA → checkpoint → report → STOP. Do not autonomously start Phase5/Phase6/Phase7.**
