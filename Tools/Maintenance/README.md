# Maintenance evidence and commands

The scripts here belong to the owner's verified asset cleanup and required directory migration on 2026-11-12/13. They use the project root two levels above this directory.

| Script | Role | Mutates game files? |
| --- | --- | --- |
| `audit_asset_cleanup.py` | Inventory/hash Assets, duplicates, consumers and workspace sizes into a new Docs/Storage folder | No |
| `verify_project_layout.py` | Compare current files to recorded before-state and native Unity results; export final tree | No game mutations; writes verification/tree documents |
| `migrate_project_layout.py` | Reviewed first migration/deletion group | Yes; one-shot, do not reapply |
| `finalize_project_layout.py` | Reviewed second art/import-source migration group | Yes; one-shot, do not reapply |
| `cleanup_generated_gradle.py` | Only two reviewed Gradle output folders, after separately checking Unity is stopped | Yes; completed manifest prevents reapplying |

The guards and exact manifests are part of the evidence. These are not general-purpose cleanup commands for another project or a future build. Do not run historical cleanup scripts elsewhere in Tools as substitutes.

Follow-up 2026-11-13: `eliminate_unused_art.py` completed its one-time archive outside the project. Do not rerun it. `verify_unused_assets.py` compares before/after native audits and original hashes in that local archive; it does not claim Player/device acceptance. Archive content is preserved locally and is excluded from any source commit. See [follow-up report](../../Docs/Architecture/UNUSED_ASSET_ELIMINATION.md).

Current source map: [CODEBASE_MAP](../../Docs/Architecture/CODEBASE_MAP.md). Cleanup outcome and limits: [CLEANUP_REPORT](../../Docs/Architecture/CLEANUP_REPORT.md).
