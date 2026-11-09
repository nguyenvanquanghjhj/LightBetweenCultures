# Artwork checkpoint — 21/10/2026

Windows focused runtime: **297 checks PASS**, 540×960. [Before/after gallery](../../Validation/NpcAppearance-20261021/gallery.html) · [test results](../../Validation/NpcAppearance-20261021/final01/restart-results.txt) · [exact changed source files](../../Validation/NpcAppearance-20261021/modified-files.txt).

| Identity | Placement | New map + portrait asset |
|---|---|---|
| Bà Nguyệt — `npc_nguyet` | Huế U2 | `Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_nguyet.png` |
| Nghệ nhân Hana — `npc_hana` | Jeonju J2 | `Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_hana.png` |
| Người giữ chuyện — `npc_elder` | Jeonju J16 | `Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/npc_elder.png` |

Matched old sprites visually and by native GUID/local ID, then reconciled identity registry and event placements. J16's main elder event and JQ04 offer remain the same NPC. Hana's old shared atlas stays untouched for other actors. Only these three NPC bindings changed. Real map travel, character touch targets, portraits and closing dialogue were verified. NPC artwork alpha has no background square/black fringe in reviewed frames. New imports use uncompressed textures, max 1024, mipmaps/trilinear to reduce the minification aliasing observed in the first Windows captures.

Lumi: exact owner-approved existing `Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png`, sprite `Lumi_MapTraveler`. All five serialized Lumi roles and both prefabs use it. Six City Maps, World Map transitions, main menu, H3, guide, combat presentation and all 12 leisure views were exercised. Seat/wing/feet/beak presentation landmarks were updated for this pose. The collider, .88-unit H3 display height, node click targets and movement/route source remain unchanged. Signature/fragment/story/ending/ceremony/credits callers were source-audited through the same canonical roles; full ending was not replayed.

Source audit: **859 original files unchanged**, including all original PNGs, six map backgrounds, canonical quest/save/combat data and route geometry. Full `AdventureUIArt` outside the three NPC entries is semantically identical (Unity normalized some equivalent Unicode escapes). [Audit](../../Validation/NpcAppearance-20261021/source-audit.json).

Limits: NPC cutouts were derived with imagegen, not pixel-identical background matting. The supplied Hana source cuts off the skirt bottom; the extraction rounded the hem slightly and needs owner visual approval. Elder feet are hidden by the hanbok. Quest completion end-to-end, Android device testing and the canonical ending were not repeated for this artwork-only revision. No Android APK was built or changed. This is automated focused runtime QA, not a blind human playtest.

Before frames use an explicit old-binding fixture within the same development Player; source was never rolled back. That fixture asset is removed from Assets after building. Normal play uses only the new defaults. An initial test falsely counted the held watering-can mesh as Lumi; that failed evidence is retained, the predicate was corrected and the final full focused run passes.

Windows: `Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe` (existing folder reused). Incremental checkpoint: `D:/GAMEKKK/Backups/CheckpointAppearance-20261021`, requires the verified JourneyShop base chain. Review gallery and Hana hem before folding this artwork into the final Android build.
