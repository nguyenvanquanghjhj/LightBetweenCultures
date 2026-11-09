# Ấn Tín Cổ — corrected item illustration

`ancient_seal` remains the same non-consumable story item. Its astronomical inscription is compared at Gyeongju's seal platform and Seoul's heritage relay; it is not a universal door key. Name, ID, ownership, rewards and usage guards are unchanged.

- ITEM_ICON_FIXED: replaced the generic key with the already imported dragon-face seal plaque. Source: `Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Hue_DragonSeal_Source.png`; sprite `fragment_hue_dragon_face`; GUID `623cab00f7d17024aa2f930ac3a1ae3d`, localID `-1532298412`.
- This is a **category illustration alias** for the item, not a new fragment or a claim that possessing the seal grants that fragment or restores the complete relic. `categoryIcon` remains true.
- ITEM_DESCRIPTION_FIXED: explains comparison at Gyeongju/Seoul and that the seal is retained. ITEM_NAME: preserved, already consistent with actual gameplay.
- Same native sprite reference in receipt and inventory. No new PNG, crop, AI art, importer or canonical relic art change.
- Synchronized `GameplayContent.json`, `AdventurePresentation.json`, `AdventureUIArt.asset` and the existing optional item-import tool's lookup.

Windows evidence: `Validation/OwnerConfusion-20261020/seal03/Owner-seal-inventory.png` and `Owner-seal-receipt-presentation.png`. Receipt review invokes the real presenter with an already owned item, without granting a reward again.
