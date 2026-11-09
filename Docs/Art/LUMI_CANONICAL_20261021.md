# Lumi — owner decision 21/10/2026

Current canonical artwork is `Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png`, GUID `9a7d3aad847598b419b6b01ab212a053`, sprite `Lumi_MapTraveler`, local ID `-1754387159`. The owner explicitly approved this exact displayed image, including its existing bag/scarf/lantern. No new Lumi illustration or accessory was generated.

All five `DesignKitArt` Lumi fields now reference that same native sprite. City/World Map already used it. Main menu, dialogue, guide, fragment reaction, story activities, signature moments, ceremony/credits and combat obtain it through those fields. H3's visual prefab and the compatibility map prefab use it too. H3 keeps its .88 world-unit visual height and -.33 foot baseline; player physics, touch target and movement code are untouched.

Leisure hand/foot/beak masks and seat pivot have been recalibrated for this pose. Those are image-space presentation landmarks; activity controls, duration, progression and rewards remain unchanged. No route/arrival/graph/terrain change is involved.

Retained source-only history: `Lumi_DialoguePortrait.png` (brown chicken in navy costume), the original costume concept, the circular legacy markers, and the previous `Lumi_ExplorationIdle.png` white action pose. Their bytes remain intact. The older Design Kit rule demanding the navy costume is superseded by this owner decision. The setup command now applies white Lumi and cannot overwrite the portrait PNG with the retired costume.

The development Windows review build contains an explicit `--appearance-before` fixture to reproduce old bindings in memory. Normal play uses the new serialized defaults. Its temporary Resources asset is removed from source immediately after the review build and is not part of ordinary future builds. Before images are a controlled binding comparison in the same Player, not screenshots falsely attributed to an old executable.

Runtime evidence and limitations: [appearance review](../../Validation/NpcAppearance-20261021/gallery.html). Android is deliberately not rebuilt for this artwork task. Canonical ending/ceremony bindings are source-audited; replaying the entire ending is outside this focused visual test.
