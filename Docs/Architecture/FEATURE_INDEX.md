# Feature index for source readers

This index follows runtime entry points into the owning rule/service code, its data, UI, and available validation journey. Validation rows identify existing harnesses; they do not claim that those runs passed in this refactor.

## Start, continue, and enter a city

1. **What it does:** Opens the main menu, continues a saved journey, or enters an unlocked city.
2. **Main logic:** `Gameplay/Entry/PrototypeGame.Start` → `Core/GameBootstrap.Ensure` → `Maps/MainMenu.Start`; city entry is `Maps/WorldMap.Select` → `GameBootstrap.EnterCity`.
3. **Processing starts at:** `MainMenu.Start` for menu choice; the `EnterCity` button callback for city selection.
4. **Data:** `Resources/Content/GameplayContent.json`, `Resources/Content/CityNavigation.json`, and `Resources/Content/HoiAnData.json`; per-city source JSON is under `Resources/Content/SourceCities/`.
5. **UI:** `UI/UIController`, `Maps/WorldMap`, `Maps/CityMap`.
6. **Validation:** `GameplaySmokeRunner.MapReturnJourney` (`--map-return-qa`), `GameplaySmokeRunner.ApprovedRoutesJourney` (`--approved-routes*`), plus save restart journeys.
7. **Trace a failure:** Check enabled scene order; then `PrototypeGame`/`MainMenu`; inspect `GameBootstrap.Reload` and `EnterCity`; inspect the resource key in `GameData.Load`; finally check `SceneTransition` and the button's enabled/unlocked predicate.

## Map discovery and movement

1. **What it does:** Reveals nodes by city stage/flags, plans paths through the city navigation graph, and moves Lumi along permitted legs.
2. **Main logic:** `MapDiscoveryManager.DiscoveryState`/`Visible`, `GameData.FindRoute`, `CityNavigationGraph.Plan`, `CityMap.TryMove`/`Move`.
3. **Processing starts at:** Node `Button`/`MapNodeHitTarget` callback → `CityMap.TryMove`.
4. **Data:** `GameplayContent.json` city nodes/gates; `CityNavigation.json` vertices, node mappings, edges and points; `HoiAnData.json` is used for Hoi An compatibility and stable node indices.
5. **UI:** `Maps/CityMap.cs`, `CityMapPaths.cs`, `NodeMarkerView.cs`, `MapPanZoom.cs`, `NodeMarkerResolver.cs`.
6. **Validation:** `GameplaySmokeRunner.ApprovedRoutesJourney` (`--approved-routes*`), `MapReturnJourney` (`--map-return-qa`), and `MapNpcMarkerJourney` (`--map-npc-markers*`). `Editor/RouteGeometryRefactorChecks.Run` checks all legacy endpoint pairs against the preserved geometry/travel contract without loading a scene.
7. **Trace a failure:** Verify city/node IDs and runtime availability; check `MapDiscoveryManager`; inspect `CityNavigationGraph.EdgeOpen/Plan`; inspect `CityRouteGeometry`; then follow `CityMap.TryMove` → `Move` and the matching journey assertions.

## Dialogue, NPC interaction, and quests

1. **What it does:** Builds available choices at a node and applies authored dialogue, quest acceptance/completion, delivery, and event effects.
2. **Main logic:** `CityMap.Interact` → `NodeEventManager.BuildOptions`/`Execute`; quest rules live in `QuestManager.CanAccept`, `Accept`, `CanComplete`, and `Complete`.
3. **Processing starts at:** `CityMap.Interact` for direct interaction or `NodeEventManager.OnArrive` for arrival-triggered events.
4. **Data:** `GameplayContent.json` event, quest, item, requirement, and reward definitions; `AdventurePresentation.json` for node presentation rules; activity text may use `StoryInteractions.json`.
5. **UI:** `GameplayPanels.ShowEvent`, `GameplayPanels.Execute`, `GameplayPanels.StoryActivities`, `MaiDialogueController`, and related `GameplayPanels.*` partials.
6. **Validation:** `GameplaySmokeRunner.FullStoryJourney` and chapter helpers in `GameplaySmokeRunner.FullStory.cs`; `GameplaySmokeRunner.MapNpcMarkerJourney` for NPC-to-event binding; `GameplaySmokeRunner.DialogueReleaseJourney` (`--dialogue-release-qa`).
7. **Trace a failure:** Inspect the node's `eventId`; compare `BuildOptions` with `CanUse`; follow the exact option ID into `Execute`; then inspect the relevant `QuestManager` predicate and save flags/items. Confirm the UI callback uses the same option ID.

## Puzzles

1. **What it does:** Checks authored puzzle prerequisites and records an explicit puzzle solution/reward.
2. **Main logic:** `PuzzleManager.Start`, `Submit`, `ResolveStory`, and its persistence `Commit`; node events route puzzle actions through `NodeEventManager`.
3. **Processing starts at:** The puzzle choice in `NodeEventManager.Execute` and the panel's submit callback.
4. **Data:** `GameplayContent.json` puzzle records plus city/node/event prerequisites; some presentation types are read from `StoryInteractions.json`.
5. **UI:** Puzzle panel in `GameplayPanels.cs` and its context-help partial.
6. **Validation:** `GameplaySmokeRunner.FullStoryJourney` chapter puzzle helpers and `GameplaySmokeRunner` phase/city journeys.
7. **Trace a failure:** Check the puzzle ID attached to the event, `PuzzleManager` eligibility and answer handling, then the exact save proof (`solvedPuzzles`/flags) and the smoke journey assertion.

## Combat and encounter rewards

1. **What it does:** Runs a turn-based encounter and grants its authored rewards once after a valid resolution.
2. **Main logic:** `BattleManager.Start`/`Act` → `EncounterManager.Resolve`.
3. **Processing starts at:** `GameplayPanels.ShowBattle` starts the encounter; battle button callbacks invoke `BattleManager.Act`.
4. **Data:** `GameplayContent.json` event/enemy definitions, mechanics, weaknesses, flags, and rewards.
5. **UI:** Battle panel and battle button callbacks in `GameplayPanels.cs`.
6. **Validation:** `GameplaySmokeRunner.FullStory.cs` (`StoryBattle`, `ChapterBattle`, `FullStoryJourney`); phase journeys exercise additional encounter variants.
7. **Trace a failure:** Check the current node/event and `NodeEventManager.CanUse`; inspect `BattleManager.Start` or the selected `Act` case; then inspect `EncounterManager.Resolve`, reward grant IDs, and the corresponding smoke result.

## Cat rescue

1. **What it does:** Runs an optional six-city rescue campaign with supplies, ordered mini-challenges, a final rescue action, and one-time coins.
2. **Main logic:** `CatRescueCatalog.Load`; `CatRescueManager.Start`, `Collect`, challenge action methods, `Ready`, `Complete`, and `Rescue`; persistence is centralized in `Commit`.
3. **Processing starts at:** The menu's Catventure button sets `OpenCatRescueOnNextCity`; the city UI opens the current chapter. Individual actions enter the manager through panel callbacks.
4. **Data:** `Resources/Content/CatRescue.json`; progress is additive `SaveData.catRescues` data.
5. **UI:** `Maps/CityMap.CatRescue.cs`, `UI/GameplayPanels.CatRescue.cs`, and Catventure UI art helpers.
6. **Validation:** `GameplaySmokeRunner.CatRescueJourney` (`--cat-rescue-qa`, `--cat-rescue-pilot`, `--cat-rescue-restart`).
7. **Trace a failure:** Check current city/chapter and node; inspect `Next`/`CanAct`/`Ready`; follow the UI callback and its challenge ID; then inspect `Commit`'s persist/rollback path and rescue reward ledger.

## Relic fragments, assembly, and restoration

1. **What it does:** Records fragments from authored proofs, assembles four pieces, and runs a relic-specific restoration activity.
2. **Main logic:** `RelicCollection.CanClaim/CanRitual`, `BeginSource/BeginAssembly`, `RelicActivity` interactions, and `FinishSource/FinishAssembly/FinishTrial`.
3. **Processing starts at:** A relic/check-in/event interaction opens the relevant panel; explicit activity completion calls the manager's finish method.
4. **Data:** `Resources/Content/CulturalRelics.json` and `GameplayContent.json` quest/check-in proofs; UI art bindings are read through `AdventureUIArt` assets.
5. **UI:** `GameplayPanels.Relics.cs`, `GameplayPanels.StoryActivities.cs`, and `GameplayPanels.FinalConnection.cs`.
6. **Validation:** `GameplaySmokeRunner.Relics.cs`, `GameplaySmokeRunner.Phase6.cs`, and relic-specific journeys in `GameplaySmokeRunner.FullStory.cs` (`--relic-*`, `--full-story-*`).
7. **Trace a failure:** Verify catalog IDs and proof strings; inspect current city/node/discovery; follow `Begin*` → `RelicActivity` interaction → `Finish*`; check persisted relic flags and `rewardGrantIds` without editing a save fixture by hand.

## Inventory and journey shop

1. **What it does:** Shows owned items, counts coins/items, consumes requirements and purchases entries from the journey shop allowlist.
2. **Main logic:** `Gameplay/Inventory.cs` (`Count`, `TryConsume`, `GrantOnce`) and `Gameplay/JourneyShop.cs` (`Unavailable`, `TryBuy`).
3. **Processing starts at:** `InventoryPanel.Show` for the bag; `GameplayPanels.ShowJourneyShop` and its purchase callbacks for the shop.
4. **Data:** `GameplayContent.json` item/quest records; the journey shop allowlist and prices are declared in `JourneyShop.Catalog` in C#, not in a separate shop JSON file.
5. **UI:** `UI/InventoryPanel.cs`, `GameplayPanels.JourneyShop.cs`, `Maps/CityMap.JourneyShop.cs`, `GameplayPanels.HotfixRewards.cs` for actual receipts.
6. **Validation:** `Validation/GameplaySmokeRunner.JourneyShop.cs`, `.ThreeNpc.cs` (inventory/reward persistence), and combat/puzzle journeys that verify rewards.
7. **Trace a failure:** Start with item ID and selected shop entry; check `Inventory.Count`, `JourneyShop.Unavailable`, receipt/request IDs and debit/rollback in `TryBuy`; then inspect the panel's refresh and receipt delta.

## Check-ins and photo journal

1. **What it does:** Records an eligible landmark, displays its scenery/postcard and lists recorded photos by city.
2. **Main logic:** `NodeEventManager.CanCheckIn` and the `checkin` action in `Execute`; Hoi An H6's relic photo proof additionally uses `RelicCollection.FinishPhoto`.
3. **Processing starts at:** `GameplayPanels.ShowEvent` routes eligible check-ins to `ShowCheckIn`; recording calls the event action explicitly.
4. **Data:** `GameplayContent.json` check-in records; `CulturalRelics.json` for associated H6 relic proof. Postcard bindings come through `AdventureUIArt`.
5. **UI:** `GameplayPanels.CheckIns.cs` (`ShowCheckIn`, `ShowScenery`, `ShowPhotoJournal`) and the photo methods in `GameplayPanels.Relics.cs`.
6. **Validation:** `Validation/GameplaySmokeRunner.CheckIns.cs`, `.Relics.cs`, `.Lighting.cs`; map markers also read completion flags through `NodeMarkerResolver`.
7. **Trace a failure:** Check city/node/check-in ID and `completionFlag`; inspect `CanCheckIn` prerequisites and `Execute`; then follow the recorded flag into the journal and resolver. Marker appearance alone does not prove a record action occurred.

## Story activities and leisure

1. **What it does:** Presents authored tactile story interactions and optional leisure scenes.
2. **Main logic:** `Gameplay/Content/StoryActivity.cs` defines activity interaction state and presentation loaders; story puzzle completion is validated by `PuzzleManager.CompleteActivity`/`ResolveStory`.
3. **Processing starts at:** `GameplayPanels.ShowStoryActivity` or `ShowLeisure`, selected by the existing node/puzzle event flow.
4. **Data:** `StoryInteractions.json`, `LeisureActivities.json`, and corresponding puzzle/event records in `GameplayContent.json`.
5. **UI:** `GameplayPanels.StoryActivities.cs`, `.Leisure.cs`, `.LeisureInput.cs`, `.LeisurePilots.cs`, and `Maps/CityMap.Leisure.cs`.
6. **Validation:** `GameplaySmokeRunner.StoryLeisure.cs`, `.LeisurePilot.cs`, `.LeisureFull.cs`, `.LeisureVisuals.cs`, `.PlayerExperience.cs`.
7. **Trace a failure:** Check the enabled presentation ID and kind, then the activity's `Prepared`/`Ready` conditions and gesture state. For rewards, follow back to the owning `PuzzleManager` commit rather than treating animation completion as save proof.

## Save and restore (persistence entry points)

1. **What it does:** Loads the player's existing journey, falls back to recoverable files, and writes progress transactionally.
2. **Main logic:** `SaveSystem.Load`, `Write`, and `CommitTemporary`; `GameBootstrap.Reload` and `Persist` own runtime normalization and derived state.
3. **Processing starts at:** `GameBootstrap.Awake` → `Reload`; writes are requested by manager actions, scene transitions, pause/quit, and explicit `Persist` calls.
4. **Data:** `SaveData` in `Persistence/SaveSystem.cs`, currently version 2; default file is `hoi-an-v1.json` in Unity persistent storage. QA uses `SaveSystem.OverridePath`.
5. **UI:** Main menu continue/recovery/reset controls in `Maps/MainMenu.cs`; most save state is shown by journal/inventory and feature panels.
6. **Validation:** `GameplaySmokeRunner` save/restart cases (`--verify-gameplay-save`, `--verify-cross-city-save`, `--full-story-restart`, `--relic-restart`, etc.); `SliceSmokeRunner` has isolated save checks.
7. **Trace a failure:** Record the isolated path and selected candidate file; inspect load validation and recovery flags; compare serialized `SaveData`; then trace the manager transaction and its `Persist` result. Do not reset or replace a real save while diagnosing.

## Seoul S21 finale

1. **What it does:** Provides the final report/connection ceremony and ending presentation after the authored Seoul and six-relic prerequisites are met.
2. **Main logic:** `NodeEventManager.FinalConnectionReady`, `CanStartFinalCeremony`, `ExecuteFinal`, `TickFinalCeremony`; visual flow in `GameplayPanels.FinalConnection`.
3. **Processing starts at:** `CityMap.Interact` at visible Seoul `S21` → event options → `GameplayPanels.Execute`/`ExecuteFinalView`.
4. **Data:** Seoul `S21` and `final_festival` in `GameplayContent.json`; six relic IDs/proofs in `CulturalRelics.json`; visual board/pages in `FinalConnectionPresentation.json`.
5. **UI:** `GameplayPanels.FinalConnection.cs` (confirmation, animation, ending pages/credits) plus the common event panel.
6. **Validation:** `GameplaySmokeRunner.FullStory.cs` contains historical S21 epilogue/credits and restart checks. Its `FullStoryJourney` requests `ending` directly after boss completion, while current `CanViewCanonicalEnding` also requires the canonical ceremony flag. That journey needs an explicit ceremony step before it can validate the current contract. A separate automated test for the 24-second canonical ceremony was not located in the inspected runner. No end-to-end ceremony coverage is claimed here.
7. **Trace a failure:** Verify active scene/city/current node/discovery; then boss resolution, each final relic ID and `AllRestored`; follow option construction and the selected action in `ExecuteFinal`; finally inspect ending panel acknowledgement and the separate canonical/seen flags.

## Editor setup, builds, and QA

1. **What it does:** Editor scripts provide authored asset setup, QA audits, and Windows/Android build entry points.
2. **Main logic:** Example build entry points are `Phase5Setup.Build`, `SliceValidation.BuildWindows`, `AndroidApkBuild`, and `AndroidQaBuild`; `Phase7Audit` audits assets and attempts its Android build only when its environment gate is enabled.
3. **Processing starts at:** Explicit `MenuItem` methods, batch `-executeMethod` calls, or a caller such as a setup script that invokes `Phase5Setup.Build`.
4. **Data:** `EditorBuildSettings.scenes`, `PlayerSettings`, project settings, and the specific asset/config files named by each setup tool.
5. **UI:** Unity Editor menus/windows, not runtime game panels.
6. **Validation:** `SliceValidation`, `Phase7Audit`, and the opt-in `GameplaySmokeRunner`; inspect each tool's output/log path and whether it mutates assets before using it.
7. **Trace a failure:** Check the command/menu entry point and its guards; confirm active target, enabled scenes, and output path; inspect the `BuildReport`/audit result and Unity log. A build script existing does not imply a successful build.

## Rescue and leisure image lookup

1. **Purpose:** Load the same approved textures/sprites from one Art tree, retaining the original cat atlas slicing and board/prop appearance.
2. **Logic:** `UI/GameArtCatalog.cs`, `UI/CatRescueArt.cs`, `Gameplay/Content/StoryActivity.cs`.
3. **Entry points:** `GameArtCatalog.Load`, `GameArtCatalog.Leisure`, `CatRescueArt.Cat/Image`, `LeisureDefinition.Sprite`.
4. **Data:** `Resources/Catalogs/GameArtCatalog.asset`, `Content/LeisureActivities.json` prop IDs and `Content/CatRescue.json` leisure art IDs.
5. **UI:** `Maps/CityMap.cs` ferry, `UI/GameplayPanels.CatRescue.cs`, existing leisure panels.
6. **Validation:** `Editor/WorkspaceLayoutSetup.Verify` compares fourteen original GUIDs, sprite localIDs, JSON selection and six cat slices. `Tools/QA/run_catventure_domain.py` tests rescue/save logic in an adapter. Player UI acceptance is reported separately.
7. **Trace failures:** JSON ID -> GameArtCatalog entry -> native GUID/localID -> Art image/meta -> consuming UI. Missing references require native audit; file existence alone is insufficient.
