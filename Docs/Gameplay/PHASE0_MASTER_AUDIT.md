# Phase 0 Master Audit — 6 Relics / Map / Ending

**Status:** COMPLETE — waiting for user approval  
**Scope:** Audit and design only. No runtime gameplay, scene, save, NPC binding,
asset import, or build was changed.

## Executive result

| Area | Result |
|---|---|
| Existing map unlock graph | `VALID_ACCESSIBLE` |
| `VN_FIRST` route | `VALID_ACCESSIBLE` |
| `KOREA_FIRST` route | `VALID_ACCESSIBLE` |
| Relic lock-loop safety | Safe only if future sources obey the audit rules |
| 24 fragment runtime sources | `0/24` verified; all remain unassigned |
| Existing NPC reuse | Candidates identified; no new assignment approved |
| Existing check-ins | 12 verified; none grants relic fragments |
| Requested one-ending six-relic rule | `NEEDS_USER_DECISION`; current S21 does not check relics |
| Hội An three light states | Existing H7/local-light ownership preserved; full relic light is not implemented |

## Files

- [MAP_UNLOCK_DEPENDENCY_AUDIT.md](./MAP_UNLOCK_DEPENDENCY_AUDIT.md)
- [ARTIFACT_FRAGMENT_ACCESS_MATRIX.md](./ARTIFACT_FRAGMENT_ACCESS_MATRIX.md)
- [SIX_ARTIFACT_PROGRESSION_DESIGN.md](./SIX_ARTIFACT_PROGRESSION_DESIGN.md)
- [ARTIFACT_UNLOCK_DECISIONS.md](./ARTIFACT_UNLOCK_DECISIONS.md)
- [CULTURAL_ARTIFACT_NPC_AUDIT.md](./CULTURAL_ARTIFACT_NPC_AUDIT.md)
- [CHECKIN_LANDMARK_AUDIT.md](./CHECKIN_LANDMARK_AUDIT.md)
- [ONE_ENDING_SIX_RELICS_CANONICAL.md](../Story/ONE_ENDING_SIX_RELICS_CANONICAL.md)
- [SEOUL_FINAL_CONNECTION_DESIGN.md](./SEOUL_FINAL_CONNECTION_DESIGN.md)
- [FINAL_ENDING_AND_POST_BOSS_AUDIT.md](../QA/FINAL_ENDING_AND_POST_BOSS_AUDIT.md)

## Approval questions

1. Approve Hội An distribution A (local-first) or B (partner-evidence-first)?
2. Approve Đà Lạt distribution A (local signal) or B (Seoul amplification)?
3. Must all six relics be restorable before S20, or may the player backtrack
   after the boss?
4. Approve the requested final rule: boss victory persists, but credits wait
   for all six relics and the final connection ceremony?
5. After approval, authorize Phase 1 only (data/art validation), still
   stopping before the Hội An runtime pilot?

## Stop condition

Phase 0 stops here. No implementation phase is started automatically.
