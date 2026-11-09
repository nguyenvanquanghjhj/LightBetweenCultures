# Adventure UI art provenance — 2026-10-16

## UI-1 art direction checkpoint (latest)

Two new PNGs were added under `Assets/_Game/Art/Runtime/AdventureUI/Theme/`:

- `LBC_UI_SkinAtlas.png`: six painted skins for paper backdrop, window, teal button,
  cream button, item slot and postcard. They use nine-slice sprite metadata;
  the first generated version needed an alpha-only cleanup before import.
- `World_Globe.png`: a painted Earth globe HUD icon. This is new artwork in the
  pack's palette, not an extraction claimed to exist in the original pack.

The built-in `image_gen` tool was used. Exact generation and cleanup prompts,
reference paths and final project paths are saved in `Docs/UI1ArtPrompts.json`.
Alpha is checked by `Tools/adventure_ui_evidence.py --ui1`.

The backpack, scroll, items, NPC portraits, Lumi roles and six city paintings
reuse existing assets. The journal HUD points to the existing extracted scroll;
no second journal/pack/city icon set was created. Existing art files and their
metadata are checked byte-for-byte against the pre-UI-1 backup.

UI-1 stops before dialogue/quest/encounter redesign. The enemy roster was inspected:
Bot gác cầu, Đèn lồng ma and Cua trộm have suitable source concepts, but separate
clean runtime enemy cutouts are still needed for UI-2. They are not considered
finished sprites merely because the concept sheet exists.

Source art was preserved. Runtime copies and generated derivatives live in
`Assets/_Game/Art/Runtime/AdventureUI/`. The built-in `image_gen` tool was used,
following `C:/Users/admin/.codex/skills/.system/imagegen/SKILL.md`.

## Existing art

- Six `City_<cityId>.png` files are byte-identical copies of each city's
  `LBC_AnhMoi_BanGiaoCodex_20261015/Maps/<City>/Map_Overview_A.png`.
- Map previews use Unity mipmaps/trilinear filtering. Source pixels were not edited.
- Lumi and Mai use their already-approved runtime portraits; neither illustration was regenerated.
- Globe, journal, confirm, cancel, arrow and speech symbols use native UI geometry,
  independent of font glyphs and emoji rendering.

## Derived atlas

`Items_TransparentAtlas.png`: background extraction from
`LBC_AnhMoi_BanGiaoCodex_20261015/Props/Items_And_Interactables_Concept.png`.
Unity sprite metadata slices sixteen objects; labels/banner/shadows were removed.
Backpack, lantern, gold key, coin, glasses and meat are used directly in the UI.
The atlas was visually inspected on transparency before import.

Exact built-in prompt:

> Use case: background-extraction. Edit target: the provided Items & Interactables concept sheet. Produce ONE transparent sprite atlas by removing only the cream background, all text labels and their cream label plaques, the top title banner, decorative sparkles, and cast ground shadows. Keep all SIXTEEN actual illustrated objects: five in row 1 (red glowing lantern, golden crystal, gold key with tassel, glasses, meat), five in row 2 (map scroll, coin, leather backpack, treasure chest, lantern post), six in row 3 (lever, portal, barrel, direction sign, flowers, mailbox). Preserve their original exact identities, painting style, black outlines, colors, proportions, positions, and row layout. Fit entire objects, no chopped edges. Do not add objects or text. Empty space must be genuinely transparent alpha, not a checkerboard painting. Output landscape 4:3 matching the input layout. This atlas will be sliced into small Unity inventory UI sprites.

## Three new item illustrations

`Hanji_Core.png`, `Copper_Wire.png`, `Healing_Tea.png` are new illustrations using
the supplied item concept sheet as style reference. They are **not** claimed to be
pre-existing finished sprites in the pack. All three were visually inspected and
imported with their original alpha; no character art or source file was overwritten.

Shared exact prompt prefix:

> Use case: stylized-concept. Asset type: ONE Unity inventory icon, transparent background, square. Reference image is the LBC item art style only: warm painted cartoon mobile adventure, thick dark outline, amber highlights, soft hand-painted shading. Single centered object filling 80 percent of square, all edges intact; no text, no plaque, no sparkle decorations, no shadow backdrop, actual transparent alpha.

Individual suffixes:

- `Hanji_Core`: Subject: A short roll of cream Korean hanji paper tied with a red cord, with a warm subtle light glowing from the paper's core. One roll only; readable as a paper lantern crafting component.
- `Copper_Wire`: Subject: A neat coil of reddish copper wire, with one short loose end. Readable as a simple electrical crafting component.
- `Healing_Tea`: Subject: A small jade-green ceramic teacup with golden tea and two green tea leaves resting alongside, gentle small steam curl. Readable as a healing tea consumable.

## Remaining visual gaps

`AdventurePresentation.json` marks 21 item mappings with `categoryIcon: true`.
These use a relevant illustrated category icon while their own final art is missing.
They mostly belong to the five unfinished chapters. The descriptions and IDs remain
distinct. No claim is made that all 31 items have unique final illustrations.

Mai keeps her approved portrait. The boatman and Korean visitor now use clean
derivatives from the supplied NPC roster, resolved through `npcId`.
Bà Tư and Ông Phúc still need faithful portraits and retain existing type symbols.
A Bà Tư extraction trial changed the costume, was rejected and is kept only in
`Validation/AdventureUI-20261016/RejectedAssets/BaTu-rejected-costume.png`.
Enemy battle art and H3 scenery still have the limitations documented in
`GameplayABReport.md`.

## NPC extraction prompts

`NPC_TransparentAtlas.png` uses `Characters/NPC_Roster_Concept.png` as edit target.
Only the visitor is sliced for runtime; the rest of the atlas remains a derivative
reference. The boatman is imported from a separate cutout to keep the full oar
without stray pixels from adjacent people.
These are AI-assisted cutout derivatives, visually reviewed for the original
identity and clothing; they are not claimed to be lossless pixel extractions.

Exact atlas prompt:

> Use case: background-extraction. Asset type: Unity dialogue portrait derivative. Edit target: provided image. Remove the cream paper, colored backdrop blobs, all title banner and all text labels and plaques. Keep all TEN illustrated characters and their original handheld objects/costumes/poses/faces at the exact same sizes and positions in the original 5-column, 2-row layout. Keep the boatman's oar, the tourist's camera and backpack, and every hat intact. Remove extra environmental props behind/around the people, such as boats, water, foliage, stalls and buildings. Preserve the original artwork style and character identities exactly. No new character design. Output transparent landscape 4:3 sprite atlas. Use genuine transparent alpha.

Exact boatman prompt:

> Use case: background-extraction. Edit target: NPC Roster concept. Extract ONLY the boatman (Bác lái đò), the smiling older Vietnamese man in the SECOND ROW, THIRD COLUMN, with gray mustache, conical hat, beige shirt, dark vest and trousers, holding a long wooden oar diagonally in both hands. Preserve his exact original face, conical hat, costume, colors, full hands and entire oar, and chibi proportions. Remove all other people, boat, water, foliage, lantern and environmental props, all text and plaques, and all backgrounds. Place the complete unchanged illustrated man AND his complete oar centered in a square transparent canvas with enough margins to prevent any clipping. No redesign, no new costume, no added props. Actual transparent alpha, no checkerboard pattern.
