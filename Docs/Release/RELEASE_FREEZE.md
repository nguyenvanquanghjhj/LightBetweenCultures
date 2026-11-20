# Light Between Cultures — RC1 source hold

Owner-authorized scope change, 2026-11-07: implement the supplied Catventure PDF as an additive six-city rescue campaign. See `Docs/Catventure/IMPLEMENTATION.md` and `Docs/Catventure/REPORT.md`. This authorization covers the rescue extension, its integration fixes and new validation/builds. Existing RC1/RC2 artifacts and historical acceptance evidence remain immutable. The RC1 hold below describes the earlier release task, not a prohibition on this explicitly requested extension.

Post-RC1 owner-authorized patch, 2026-10-24: compact City Map node panels only. Current source and the unpacked Windows QA Player include this patch; the immutable RC1 ZIP/APK remain the original candidate. See `Docs/QA/NODE_PANEL_COMPACT_20261024.md`. Other freeze restrictions remain in effect.

Version: **1.0-rc1-20261024**. Android versionCode: **6**.

Status: **RC candidate — owner acceptance pending**. Source changes are closed for this task after packaging. This is not Android-device acceptance: ADB has no device connected, and the native Android save commit still needs a realme C71 test.

Authoritative artifact hashes, source fingerprint and acceptance limits: `Validation/ReleaseCandidate-20261024/release-manifest.json`. Short report and owner checklist: `Docs/QA/RELEASE_CANDIDATE_20261024.md`.

Do not add features, rework UI or rerun Phase generators. After owner acceptance, reopen only for crash, startup failure, save loss, story softlock, broken core functions or a submission-blocking issue. Do not label the candidate approved until the outstanding device/save and owner visual checks are actually completed.

Restore source through the verified full base and incremental chain, not by copying old builds or caches. The RC incremental manifest records the exact base checkpoint and changed files.
