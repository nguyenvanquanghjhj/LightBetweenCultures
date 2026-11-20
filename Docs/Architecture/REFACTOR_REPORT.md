# Controlled refactor report

## Subsequent asset cleanup and required directory migration

The initial refactor record below describes the first, smaller stage. The owner then explicitly authorized asset deletion, source/asset moves and the required final directory layout. That work has now been performed; its authoritative file manifests, results, retained exceptions and test limits are in [CLEANUP_REPORT.md](CLEANUP_REPORT.md), with the exported actual file tree in [FINAL_TREE.md](FINAL_TREE.md). The initial statements below about no moves/deletions apply only to the initial stage.

## Initial stage: scope and starting state

- Branch created: `refactor/codebase-map-20261112`, based on `main` at `9ec07c5e581444d34447598f4c0c6538e40ad9f4` (`Fix Seoul relic and telescope art`).
- The worktree already contained modified generated `.csproj` files and untracked QA/report/build-backup material before this task. Those files and directories were preserved and left untouched.
- No commit, push, merge, player build, feature/UI/content/save change, file move, delete, class rename, namespace change, or existing `.meta` edit was made. Unity generated one new `.meta` for the Editor validation script.

## Architecture before the refactor

The runtime is organized around `GameBootstrap`, which loads content, city data, and the existing save before constructing stateful services. `CityMap` and `WorldMap` render from those services; `GameplayPanels` partial files share one panel class and dispatch user choices to managers. JSON catalogs author most gameplay and presentation data. `GameplaySmokeRunner` supplies opt-in scene/UI journeys with an isolated save override. Editor scripts cover setup, audits, and platform builds.

The verified wiring and flow diagrams are in [CODEBASE_MAP.md](CODEBASE_MAP.md); file-by-file learner paths and validation entry points are in [FEATURE_INDEX.md](FEATURE_INDEX.md).

## Confirmed issue and code change

`CityRouteGeometry.Travel` and `CityRouteGeometry.Points(GameData, ...)` each implemented the same two-direction endpoint comparison. A script-wide call-site search found no current caller of `Travel`, but the public method was retained. Centralizing the shared invariant prevents the two methods from drifting if both remain in use.

The one code refactor adds private `EndpointsMatchEitherWay(EdgeData, ...)` and uses it in both methods. The condition is grouped explicitly. Callers, data types, routing direction behavior, route points, save state, assets, and public APIs are unchanged.

## Files changed for this task

- `Assets/_Game/Scripts/Maps/CityRouteGeometry.cs` — centralized the duplicated endpoint predicate.
- `Assets/_Game/Scripts/Editor/RouteGeometryRefactorChecks.cs` and its new `.meta` — opt-in Editor-only regression against the original endpoint lookup contract on all six cities, plus first-match/reverse/arrival-anchor cases. The generated GUID is `0bb22b57f4fb11f458ec07ba75568274`.
- `Docs/Architecture/CODEBASE_MAP.md` — documented modules, dependencies, data keys, lifecycle and requested gameplay flows.
- `Docs/Architecture/FEATURE_INDEX.md` — gave learners entry methods, data, UI, validation paths and failure-tracing steps by feature.
- `Docs/Architecture/REFACTOR_REPORT.md` — this audit and verification record.

No existing `.meta`, scene, prefab, save, JSON resource, or build artifact was edited. All pre-existing backups, patches, reports, and logs were retained. The three generated `.csproj` modifications listed in Git status were already present at task start.

## Verification

| Check | Result |
| --- | --- |
| Endpoint predicate semantics | PASS: compiled the exact baseline and refactored C# expressions in a small PowerShell `Add-Type` probe and compared all 256 endpoint/query combinations for IDs 0–3. The source contains exactly two helper call sites. This is a focused logic check, not a Unity assembly/runtime test. |
| Runtime resource JSON syntax | PASS: `ConvertFrom-Json` parsed all 16 `.json` files under `Assets/_Game/Resources`. |
| Patch whitespace | PASS: `git diff --check`; the changed C# file also retains CRLF line endings. |
| Save schema and resource paths | No save/content/resource files were edited. `SaveSystem.cs` has the same Git blob hash as HEAD: `339b79539d1f50511b0e141fdd3b22ac0ac0de49`. Old-save load/write compatibility was not exercised in Unity. |
| Existing `.meta` GUIDs and asset references | `git diff --name-only -- '*.meta' '*.unity' '*.prefab' Assets/_Game/Resources ProjectSettings Packages` is empty. No existing asset reference/path was edited or moved; the helper is private in the existing static class. |
| Initial Unity attempts | BLOCKED before script compilation. Both sandboxed Unity 6000.6.5f1 attempts logged exit 198, unavailable access token/entitlements and missing `com.unity.editor.headless`. Logs: `Validation/ArchitectureRefactor-20261112-before.log` and `Validation/ArchitectureRefactor-20261112-after.log`. No baseline Unity compile PASS is claimed. |
| Unity compile after retry | PASS outside the sandbox with the machine's license context. `Validation/ArchitectureRefactor-20261112-licensed-retry.log` records compilation/assembly reload and exit 0. Existing UAC0009 deprecation warnings remain. |
| Actual Unity route regression | PASS in Unity 6000.6.5f1: six cities, 2,949 node pairs and 5,901 assertions. Expected travel mode and exact oriented geometry are compared with the original lookup contract, including missing edges, first-match behavior, reverse lookup and explicit arrival anchors. Evidence: `Validation/ArchitectureRefactor-20261112-route-results.json` and `Validation/ArchitectureRefactor-20261112-route-checks.log` (exit 0). |
| Unity Play Mode/UI journey | INCOMPLETE: an attempt to launch the existing fresh-map journey did not create its result file and repeatedly logged `NullReferenceException` at `SoftAudio.TickMusic`, `SoftAudio.Music.cs:61`. The QA Editor process was stopped and its log retained at `Validation/ArchitectureRefactor-20261112-fresh-map.log`. This is not a successful scene/UI journey. The attempted launcher was removed from the new validation script; audio/runtime initialization was left unchanged. |

The focused endpoint comparison was rerun after the final production code edit. After removing the unsuccessful Play Mode launcher, the final validator compiled and ran again with the same result (six cities, 2,949 pairs, 5,901 assertions), exit 0; evidence is `Validation/ArchitectureRefactor-20261112-final-route-results.json` and `Validation/ArchitectureRefactor-20261112-final-route-checks.log`. No APK/EXE was overwritten. The successful Edit Mode regression does not construct `GameBootstrap`, load scenes, write player saves or alter assets.

### Reproduce the bounded Unity regression

Run the following from the project root with a licensed Unity Editor. Choose new result/log paths for subsequent runs; the check refuses to overwrite an existing result file.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\GAMEKKK\LightBetweenCultures' -executeMethod LightBetweenCultures.Editor.RouteGeometryRefactorChecks.Run --route-refactor-output=Validation/ArchitectureRefactor-new-results.json -quit -logFile Validation/ArchitectureRefactor-new.log
```

The full scene/UI regression still requires a working Play Mode/Development Player harness. Do not interpret the small Editor check as end-to-end game acceptance.

## Systems intentionally kept as-is

- `SaveData` remains version 2, and `SaveSystem` still handles v1/v2 input and existing backup/temporary-file recovery.
- Runtime manager behavior, story conditions, the S21 ending flags, route data, node indices, UI, and all `Resources/Content` paths are unchanged.
- Prototype, Hotfix, Phase, and phase-named validation scripts were not removed or renamed. Scene/build references and setup-workflow call sites exist; names alone did not establish that they were obsolete.
- The `GameplayPanels` partials were not mechanically split into new classes: they share one runtime type, fields, and callbacks, and this task did not establish a safe independent service boundary.

## Remaining risks and next steps

- Actual scene/UI journeys remain unverified. The successful Unity regression exercises the changed geometry contract, not combat, story, visuals, performance, ending playback or Android installation.
- The Play Mode attempt exposed a null reference in the unchanged audio code, but no baseline runtime reproduction was completed, so this report does not attribute the cause to either the refactor or a pre-existing gameplay defect. It needs separate investigation of runtime initialization and the Editor QA launcher.
- `GameplaySmokeRunner.FullStoryJourney` requests `ending` immediately after boss completion, whereas current `CanViewCanonicalEnding` requires the canonical ceremony flag. The historical journey needs an explicit ceremony step to test the current S21 contract. No test/gameplay changes were made to conceal that coverage gap.
- Old-save compatibility is supported by unchanged save code and schema, but was not reloaded in Unity for this change.
- A future refactor should first add Unity tests for pure navigation/discovery rules and representative save fixtures, then choose one bounded manager/UI boundary with a passing before/after runtime journey. Any larger `GameplayPanels` or `GameplaySmokeRunner` reorganization should follow that evidence rather than file-name-based cleanup.
