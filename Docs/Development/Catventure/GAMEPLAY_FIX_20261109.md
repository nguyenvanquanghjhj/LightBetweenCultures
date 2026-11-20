# Catventure 1.1.3 — Gameplay & HUD hotfix (09/11/2026)

## Source target and scope

Target: `LightBetweenCultures`, `main` at `2113cb8` (version 1.1.2). Patch checks exact source fragments and **backs up all edited files before writing** to `D:\GAMEKKK\Backups\Catventure-GameplayFix-*`. No scenes, prefab, character art, `.meta`, save files, database, or old EXE/APK files are deleted. This package **does not auto-commit/push**.

### What changes

1. **Huế/Hội An rain needles and particle specks**: replaces per-frame full-screen line/dot weather mesh with a very faint **static local atmosphere**. Preserves weather identity without falling bars, dots flying across artwork or periodic vertex rebuilds. Celebration now uses a static, gentle warm band rather than confetti specks.
2. **Mini-map as practical navigation:** node dots now use **walkable arrival coordinates**, matching Lumi and target. Draws the currently **available route** (gold line) using existing route planner and flag/gate rules; larger card has the label **ĐI ĐẾN MỤC TIÊU**. Clicking attempts walking via existing movement / NPC / mandatory gate checks; never teleports or silently opens a locked node. Keep the route graph and character/target pips to preserve existing map QA.
3. **Đà Lạt D7 reflector bot:** previously `prepare` twice auto-completed. It now asks the player to steer the reflected light **up to gương 2, then down to the bot**. Wrong angle does not grant the glasses or progress; repeating the same choice after reload cannot win. This is still a **text-and-choice geometry puzzle**, not a fully animated raytracing minigame. A combat option remains.
4. **Main menu:** moves the version mark away from the message panel; prevents entering Catventure while save recovery is blocked and moves the Retry button away from the guide.
5. **Build separation:** new `1.1.3-catventure-20261109` Windows and Android code 23 outputs have distinct names. Existing version 1.1.2 binaries are not overwritten.
6. Updates the existing Da Lat development smoke test to test wrong angle and repeated direction after restart.

## Verification

Run on the root of the patched project:

```powershell
python Tools\validate_catventure.py
python Tools\check_catventure_delivery.py
python Tools\check_gameplay_hotfix.py
```

These are **static/data/source checks**, not a substitute for running Unity. Existing 1.1.2 Windows Release or the old Android build must never be labeled tested as 1.1.3.

## Required native testing after patch

**Use Unity 6000.6.5f1.** Close the Unity editor before a batch build, otherwise Unity may exit early because another editor has locked the project. Run a separate Windows **Development** build for smoke runner (the Release build excludes `GameplaySmokeRunner`).

- First launch: verify new `CATVENTURE · BẢN 1.1.3` mark and that no text overlaps message/Start/Guide; test save corruption block on isolated QA save only.
- Huế & Hội An: check map at different zooms and on portrait/landscape screens. There must be **no vertical falling needles or flying dots**. Verify map dragging and tap areas remain unobstructed.
- Mini-map in every city: compare character/target pins with real positions, confirm it shows a valid golden route and `ĐI ĐẾN MỤC TIÊU` actually moves only on open paths. Locked/mandatory nodes must not be bypassed. If the target is the current node, the objective panel opens.
- D7: wrong angle must do nothing; **up** succeeds stage one, wrong/repeated **up** cannot finish stage two; **down** resolves after stage one; restart between the stages must retain one step and reward one time. Battle path remains valid.
- Run `--gameplay-smoke --cat-rescue-qa` at two resolutions in a **Development Player** and review **actual log**; also run the Dalat old story test and other chapter regression. Automated scripts may fail due to unrelated prior differences; record PASS/FAIL/NOT_TESTED separately.
- Test old save backup recovery, inventory, shop, quests, six cities and original storyline. Never run QA with an owner's production save; `GameplaySmokeRunner` isolates its QA save path.
- Build Windows Release only after Development Player PASS. Build Android (code 23) and test mouse/touch / drag-drop / pinch / FPS / temperature / save on a physical phone. Without phone results, Android = **NOT_TESTED**.

## Still not fully implemented

The original 13-page PDF wants a more immersive game than the current system. **This hotfix does not implement** boat steering in Huế; true lamp-controlled road gates in Hội An; cat lure with bells in Đà Lạt; free exploration of Hanok buildings in Jeonju; actual photo jigsaw at Gyeongju; complete Seoul landmarks minigame; full graphical ray physics at D7; end-to-end native six-city gameplay and Android QA. Those remain separate tasks. Do not call this a 100%-complete project.
