LBC H3 — ASSET SHEETS (source/reference only)

Destination in Unity project:
D:\GAMEKKK\LightBetweenCultures\Assets\_Game\Art\_SourceSheets\H3_Upgrade_20261016\

This pack has 7 UNIQUE images, not 8: the two generated building sheets are byte-for-byte identical.
No old scene, GUID, .meta or prefab was modified. These source sheets are NOT finished Unity runtime sprites.

Existing Unity assets checked from the user provided Art(1).zip:
- _SourceSheets/HoiAn_H3_Environment_v1.png: old source concept with baked captions.
- Environment/HoiAn/: no separate production-ready sprites in provided ZIP.
- Characters/Lumi/: approved map/dialogue/exploration sprite roles exist. Keep.
- Characters/Enemies/HoiAn_Enemies_Atlas.png exists and is approved. Keep.
- Markers/ already contains approved markers. Do not replace.

REQUIRED CODEX WORKFLOW:
1) Inspect project at its CURRENT state; newer steps may have integrated some of these already.
2) Compare new sheets with existing assets and retain originals.
3) Crop only useful new elements. Inspect alpha channel, halos, neighboring sprites, shadows, clipping.
4) Export runtime RGBA sprites into Assets/_Game/Art/Environment/HoiAn/H3/,
   UI/H3/ and, only if appropriate, dedicated FX folders. Keep unique names and .meta.
5) Use Unity Editor/AssetDatabase for moving/renaming existing assets. Do not orphan references.
6) Build a layered, traversable H3, not one flattened screenshot: ground/path/background,
   walls/colliders, decorations/foreground/occlusion, item glow separate, Lumi independent.
7) Use existing Lumi_ExplorationIdle, HoiAn gameplay scripts and current pickup/quest logic.
8) UI blank panels can be 9-sliced; all labels should be runtime text; never use buttons with baked text.
9) Check H3 on mobile portrait and Windows; verify actual movement, pickup and save/restart.
10) Audit other placeholder environments and report them, but DO NOT redesign canonical maps/bosses/markers.

MANIFEST:
 - H3_01_VisualTarget_Reference.png: Composition concept only. Contains character and baked text; not a runtime single-layer background.
 - H3_02_Environment_Architecture_Props.png: Buildings, walls, doors, paths, vegetation; crop/alpha-clean for environment runtime.
 - H3_03_UI_TextBaked_Reference.png: Reference only: title and buttons contain baked text, do not use for dynamic localization.
 - H3_04_Lantern_LightFX_Concept.png: Lantern, sparkles and rings. Validate alpha and runtime layering; use already-approved icon if exists.
 - H3_05_UI_BlankPanels_Sheet.png: Frames and textless button bases; crop and use 9-slice with runtime TextMeshPro.
 - H3_06_Markers_Interactables_Concept.png: Potential interaction cues only; do not overwrite approved Boss/NPC marker artwork.
 - H3_07_Characters_NPC_Concept.png: Character art reference only; do not replace canonical Lumi/Mai/enemies or assign names without audit.
