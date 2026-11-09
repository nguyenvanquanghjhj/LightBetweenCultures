# Storage and checkpoint rules

These rules apply to future work in this project. Preserve the current game and queued requests.

- Before creating a checkpoint, compare the latest manifest and changed source. Reuse the existing checkpoint when there is no material change. Never copy Library, cache, Temp, obj or the entire historical Validation tree into a checkpoint.
- Keep the latest verified full restore point and the independent pre-Full-Story restore point. Prefer a small incremental snapshot for subsequent changes: changed files, deleted-path list, base checkpoint path and manifest SHA256, timestamp, change summary and hashes. Verify the base and changed files. An incremental snapshot must state that it requires its base; it is not a standalone backup.
- Estimate checkpoint size before copying. Explain any multi-GB full snapshot and obtain separate approval for a new full copy when an equivalent recent restore point exists. Never delete any backup/checkpoint without separate user approval.
- Reuse the current Windows build when production inputs and build settings are unchanged. QA-only reruns do not justify copying the build into each run folder.
- Run experiments in a clearly named temporary QA workspace. Retain logs, saves needed for reproducibility, failure-specific captures and representative acceptance images. Do not retain a new screenshot for every assertion by default. Plan captures for requested before/after views and meaningful milestones; preserve all evidence required by an accepted report or manifest.
- At a task's end, inspect intermediate exports. Remove only verified obsolete captures with no report/manifest dependency. Retain diagnostic samples and record exact removed paths/hashes. Do not delete saves, original art, native metadata, pending-request documents or unreviewed H3/terrain evidence.
- Do not delete Unity caches while Editor/Player/build is active. Report potential savings and reimport/build costs before any large cache cleanup. Prefer bounded known caches over deleting all Library.
- Storage audit records live in Docs/Storage. Tools/storage_cleanup.py contains a historical bounded QA cleanup plan; do not rerun its audit/apply automatically on future tasks. Do not rewrite old acceptance manifests to hide removed or failed evidence.

# NPC identity rules

- Read Docs/Art/NPC_CANONICAL_REGISTRY.md and its JSON before adding/reclassifying NPC art. One npcId has one canonical identity and may have many APPROVED_VARIANT sprites/portraits. Face structure, relative age and distinctive traits stay consistent; costume, expression, pose, direction and reasonable accessories/hairstyle changes are allowed.
- Ba Tu, Phuc, Huong and So-yeon's current sprites are CANONICAL by user confirmation on 2026-10-17. Preserve approved Mai/Ji-eun and other confirmed identities. Do not treat historical temporary/provenance tags as a pending identity approval. Runtime bindings and existing PASS UI/gameplay need no rework for this metadata correction.
- Inspect actual images, not only GUID/path/name. Record IDENTITY_CONFLICT only for clear face/age/identity changes. Never use So-yeon's face for Ji-won or share one face between independent people. Keep approval separate from crop/animation quality; concept-only approved variants cannot be bound as whole sheets.
- After a Unity native binding export, run Tools/art_asset_catalog.py --inventory-only to merge curated identity approval by exact sprite reference. AdventureUIArt.status retains historical integration annotations; the Docs registry is authoritative for current identity approval. Never change an existing approved face just to make a costume variant.
