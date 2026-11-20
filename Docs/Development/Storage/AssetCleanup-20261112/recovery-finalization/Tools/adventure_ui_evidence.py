"""Collect evidence from the opt-in Windows runtime UI/gameplay journey.
Images are inspected, never edited. Run after both gameplay and restart tests.
"""
from pathlib import Path
from datetime import datetime, timezone
import hashlib
import json
import re
import sys
import subprocess
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
OUTPUT = ROOT / "Validation/AdventureUI-20261016/Final"
BEFORE = ROOT.parent / "Backups/BeforeAdventureUI-20261016"
BASELINE = ROOT.parent / "Backups/CheckpointGameplayAB-20261016/Assets_Game"
UI1 = "--ui1" in sys.argv
MAP_ACTORS = "--map-actors" in sys.argv
NODE_MARKERS = "--node-markers" in sys.argv
EXPLORATION = "--exploration" in sys.argv
MARKER_SPRITES = "--marker-sprites" in sys.argv
SPECIALTY_ITEMS = "--specialty-items" in sys.argv
ART_LIBRARY = "--art-library" in sys.argv
if UI1:
    OUTPUT = ROOT / "Validation/UI1ArtDirection-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeUI1ArtDirection-20261016/Assets_Game"
    BASELINE = BEFORE
if MAP_ACTORS:
    OUTPUT = ROOT / "Validation/MapActorsStage1-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeMapActorsStage1-20261016/Assets_Game"
    BASELINE = BEFORE
if NODE_MARKERS:
    OUTPUT = ROOT / "Validation/NodeMarkers-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeNodeMarkers-20261016/Assets_Game"
    BASELINE = BEFORE
if EXPLORATION:
    OUTPUT = ROOT / "Validation/ExplorationMarkers-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeExplorationMarkers-20261016/Assets_Game"
    BASELINE = BEFORE
if MARKER_SPRITES:
    OUTPUT = ROOT / "Validation/MarkerSprites-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeMarkerSprites-20261016/Assets_Game"
    BASELINE = BEFORE
if SPECIALTY_ITEMS:
    OUTPUT = ROOT / "Validation/SpecialtyItems-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeSpecialtyItems-20261016/Assets_Game"
    BASELINE = BEFORE
if ART_LIBRARY:
    OUTPUT = ROOT / "Validation/ArtLibrary-20261016/Final"
    BEFORE = ROOT.parent / "Backups/BeforeArtLibrary-20261016/Assets_Game"
    BASELINE = BEFORE


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def record(path):
    return {"path": str(path.relative_to(ROOT)), "sha256": sha(path),
            "modified_utc": datetime.fromtimestamp(path.stat().st_mtime, timezone.utc).isoformat()}


def relocated(path):
    if not ART_LIBRARY:return path
    rel=path.relative_to(ROOT).as_posix()
    for move in json.loads((ROOT/"Docs/Art/asset-moves.json").read_text(encoding="utf-8"))["entries"]:
        if rel==move["oldPath"] or rel==move["oldPath"]+".meta":
            return ROOT/(move["newPath"]+(".meta" if rel.endswith(".meta") else ""))
    return path


def differences(area):
    current = ROOT / "Assets/_Game" / area
    previous = BEFORE / area
    changed, added = [], []
    for path in sorted(current.rglob("*")):
        if not path.is_file() or path.suffix == ".meta":
            continue
        old = previous / path.relative_to(current)
        if not old.exists():
            added.append(str(path.relative_to(ROOT)))
        elif sha(path) != sha(old):
            changed.append(str(path.relative_to(ROOT)))
    return {"modified": changed, "added": added}


def exploration_evidence():
    import numpy as np
    tests, preserved, captures, motion = {}, [], [], []
    binary = ROOT / "Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll"
    for name in ("smoke-results.txt", "restart-results.txt"):
        lines = (OUTPUT / name).read_text(encoding="utf-8-sig").splitlines()
        assert lines[-1] == "ALL CHECKS PASSED", name
        tests[name] = {"pass_count": sum(s.startswith("PASS:") for s in lines), "all_passed": True}
    allowed = {"ContentDefinitions.cs", "NodeEventManager.cs", "GameData.cs", "MapDiscoveryManager.cs",
               "NodeMarkerView.cs", "NodeMarkerResolver.cs", "CityMap.cs", "CityMapPaths.cs",
               "AdventureIconGraphic.cs", "GameplayPanels.cs", "GameplaySmokeRunner.cs"}
    if MARKER_SPRITES:
        allowed = {"AdventureUIArt.cs", "AdventureUISetup.cs", "NodeMarkerView.cs"}
    if SPECIALTY_ITEMS:
        allowed = {"ContentDefinitions.cs","NodeEventManager.cs","QuestManager.cs","NodeMarkerResolver.cs","GameplayPanels.cs","InventoryPanel.cs","AdventureUISetup.cs","GameplaySmokeRunner.cs"}
    if ART_LIBRARY:
        allowed={"AdventureUISetup.cs","DesignKitSetup.cs","LumiRoleSetup.cs","HoiAnMapSetup.cs","HoiAnDemoSetup.cs"}
    for old in sorted(BEFORE.rglob("*")):
        if not old.is_file():
            continue
        path = relocated(ROOT / "Assets/_Game" / old.relative_to(BEFORE))
        if ART_LIBRARY and not path.exists() and old.suffix==".meta" and (old.parent/old.stem).is_dir():
            continue  # Empty obsolete folder metadata; texture GUIDs remain verified separately.
        assert path.exists(), str(path)
        if MARKER_SPRITES and old.relative_to(BEFORE).as_posix() == "Resources/AdventureUIArt.asset":
            assert old.read_text(encoding="utf-8-sig").rstrip() == path.read_text(encoding="utf-8-sig").split("\n  markers:")[0].rstrip()
            continue
        if SPECIALTY_ITEMS and (old.relative_to(BEFORE).as_posix()=="Resources/AdventureUIArt.asset" or old.parent.name=="Item" and old.suffix==".meta"):
            continue
        if old.name in allowed or (not MARKER_SPRITES and not ART_LIBRARY and old.relative_to(BEFORE).as_posix() in (
                "Resources/Content/GameplayContent.json", "Resources/Content/AdventurePresentation.json")):
            continue
        assert sha(old) == sha(path), str(path)
        preserved.append(str(path.relative_to(ROOT)))
    old = json.loads((BEFORE / "Resources/Content/GameplayContent.json").read_text(encoding="utf-8-sig"))
    new = json.loads((ROOT / "Assets/_Game/Resources/Content/GameplayContent.json").read_text(encoding="utf-8-sig"))
    checkins = new.pop("checkIns")
    if SPECIALTY_ITEMS:
        old_checkins=old.pop("checkIns")
        for a,b in zip(old_checkins,checkins):
            b=dict(b);b.pop("rewardItems",None);assert a==b
        for key in ("cities","enemies","puzzles","resonances"):
            assert old[key]==new[key],key
        old_items={i["id"]:i for i in old["items"]};new_items={i["id"]:i for i in new["items"]}
        assert len(new_items)==37 and all(all(new_items[k][f]==v for f,v in entry.items()) for k,entry in old_items.items())
        for q in old["quests"]:
            current=next(i for i in new["quests"] if i["id"]==q["id"])
            assert all(current[k]==v for k,v in q.items() if q["id"]!="HQ02" or k not in ("rewardItems","thanks","flags"))
        assert all(e in new["events"] for e in old["events"])
        old=json.loads(json.dumps(new))
        for city in old["cities"]:
            for node in city["nodes"]:
                node.pop("previewStages",None)
    if MARKER_SPRITES or ART_LIBRARY:
        old.pop("checkIns")
        for city in old["cities"]:
            for node in city["nodes"]:
                node.pop("previewStages", None)
    assert len(checkins) == 6 and len({e["completionFlag"] for e in checkins}) == 6
    previews = {}
    for city in new["cities"]:
        for node in city["nodes"]:
            if "previewStages" in node:
                assert city["id"] == "hoi_an"
                previews[node["id"]] = node.pop("previewStages")
    assert old == new, "Unexpected quest, coordinate, route, item, enemy or event changes"
    for p in ("AdventurePresentation.json",):
        a = json.loads((BEFORE / "Resources/Content" / p).read_text(encoding="utf-8-sig"))
        b = json.loads((ROOT / "Assets/_Game/Resources/Content" / p).read_text(encoding="utf-8-sig"))
        for d in a["nodeMarkers"]:
            d.pop("offset", None); d.pop("offsetMode", None)
        if SPECIALTY_ITEMS:
            assert a["mapActors"]==b["mapActors"] and a["cities"]==b["cities"]
            assert all(i in b["items"] for i in a["items"])
        else:
            assert a == b, "Existing marker rules or actor data changed unexpectedly"
    for folder in ("ProjectSettings", "Packages"):
        for oldfile in sorted((BEFORE.parent / folder).rglob("*")):
            if oldfile.is_file():
                current = ROOT / folder / oldfile.relative_to(BEFORE.parent / folder)
                assert sha(oldfile) == sha(current), str(current)
    mandatory = ["EXP-01-Fresh-revealed-preview-hidden", "EXP-02-After-first-gate-new-boundary-previews",
                 "EXP-03-Boss-sprite-red-floating-alert", "EXP-04-Camera-white-marker",
                 "EXP-04-White-camera-checkin-offer", "EXP-05-Checkin-recorded", "EXP-Motion-off",
                 "S1-Restart-only-region2-and-actors", "UI-04-Lumi-dialogue", "UI-05-NPC-Mai-dialogue"]
    for name in mandatory:
        assert (OUTPUT / (name + ".png")).stat().st_mtime > binary.stat().st_mtime, name
    if SPECIALTY_ITEMS:
        for name in ("FOOD-01-NPC-request-missing-reserved-spice","FOOD-03-Pickup-icons-and-quantities","FOOD-04-Inventory-chili-shallot","FOOD-06-Actual-turn-in-reward","FOOD-07-Pending-cross-city-journal"):
            assert (OUTPUT/(name+".png")).stat().st_mtime>binary.stat().st_mtime,name
    for path in sorted(OUTPUT.glob("*.png")):
        with Image.open(path) as picture:
            captures.append({**record(path), "resolution": list(picture.size)})
    for folder in ("npc-floating", "boss-floating"):
        directory = OUTPUT / folder
        data = json.loads((directory / "motion.json").read_text(encoding="utf-8-sig"))
        frames = data["frames"]
        assert len(frames) == 80
        times = np.array([f["time"] for f in frames])
        assert np.all(np.diff(times) > 0)
        ys = np.array([f["visualOffset"]["y"] for f in frames])
        fit = np.column_stack((np.ones(len(times)), np.sin(times * 2 * np.pi / data["period"]), np.cos(times * 2 * np.pi / data["period"])))
        coeff = np.linalg.lstsq(fit, ys, rcond=None)[0]
        amplitude = float(np.hypot(coeff[1], coeff[2]))
        r2 = float(1 - np.sum((ys - fit @ coeff) ** 2) / np.sum((ys - ys.mean()) ** 2))
        assert abs(amplitude - 4) < .01 and r2 > .999
        phase = float(np.arctan2(coeff[2], coeff[1]))
        expected_phase = int(data["nodeId"][1:]) * .37
        phase_error = float(abs(np.angle(np.exp(1j * (phase - expected_phase)))))
        assert phase_error < .01, "Runtime phase must differ by node"
        for field in ("hitCenter", "nodePosition", "markerAnchor", "actorOffset"):
            assert all(f[field] == frames[0][field] for f in frames), field
        paths = [directory / ("frame-" + str(i).zfill(4) + ".png") for i in range(80)]
        for path in paths:
            assert path.stat().st_mtime > binary.stat().st_mtime
            with Image.open(path) as picture:
                assert picture.size == (540, 960)
        durations = list(np.diff(times)) + [float(np.median(np.diff(times)))]
        concat = directory / "runtime-timing.txt"
        concat.write_text("".join("file '" + p.name + "'\nduration " + str(float(d)) + "\n" for p, d in zip(paths, durations)) + "file '" + paths[-1].name + "'\n", encoding="utf-8")
        video = OUTPUT / (folder + ".mp4")
        subprocess.run(["ffmpeg", "-hide_banner", "-loglevel", "error", "-y", "-f", "concat", "-safe", "0", "-i", str(concat), "-fps_mode", "vfr", "-c:v", "libx264", "-crf", "20", "-pix_fmt", "yuv420p", "-movflags", "+faststart", str(video)], check=True)
        subprocess.run(["ffmpeg", "-v", "error", "-i", str(video), "-f", "null", "-"], check=True)
        motion.append({"node_id": data["nodeId"], "frames": 80, "actual_duration_seconds": float(times[-1] - times[0]),
                       "period_seconds": data["period"], "fitted_amplitude_ui_units": amplitude, "fit_r2": r2,
                       "phase_radians": phase, "phase_error_radians": phase_error,
                       "fixed_input_ground_actor": True, "video": record(video), "telemetry": record(directory / "motion.json"),
                       "runtime_frames": [record(p) for p in paths]})
    logs = [ROOT / "Logs" / (("art-library-"+name+".log") if ART_LIBRARY else ("specialty-items-"+name+".log") if SPECIALTY_ITEMS else ("marker-sprites-" + name + ".log") if MARKER_SPRITES else ("exploration-markers-" + name + "-final.log")) for name in ("build", "player", "restart")]
    for path in logs:
        assert not re.search(r"error CS\d+|NullReferenceException|MissingReferenceException|GAMEPLAY FAIL", path.read_text(encoding="utf-8-sig")), path
    manifest = {"generated_utc": datetime.now(timezone.utc).isoformat(), "runtime_tests": tests,
                "diffs": {area: differences(area) for area in ("Scripts", "Resources", "Art", "Scenes", "Prefabs")},
                "preserved_byte_identical": preserved, "catalog_preserved_except_preview_and_optional_checkins": True,
                "preview_stages": previews, "optional_checkins": checkins, "binary": record(binary),
                "source": [record(p) for p in sorted((ROOT / "Assets/_Game/Scripts").rglob("*.cs"))],
                "logs": [record(p) for p in logs], "captures": captures, "motion": motion}
    if MARKER_SPRITES:
        sprites=[]
        for path in sorted((ROOT / "Assets/_Game/Art/Runtime/AdventureUI/Markers").glob("*.png")):
            with Image.open(path) as picture:
                assert picture.mode=="RGBA" and picture.getchannel("A").getextrema()[0]==0 and picture.getchannel("A").getextrema()[1]>=250
                if path.stem in ("Ground","Current"):
                    assert picture.getpixel((picture.width//2,picture.height//2))[3]==0
                sprites.append({**record(path),"resolution":list(picture.size)})
        assert len(sprites)==10
        manifest["marker_pngs"]=sprites
        manifest["reference_images_preserved"]=[record(p) for p in sorted((ROOT/"References/MarkerDesign").glob("*.png")) if sha(p)==sha(BEFORE.parent/"References/MarkerDesign"/p.name)]
        assert len(manifest["reference_images_preserved"])==2
    if ART_LIBRARY:
        manifest["art_library"]=json.loads((ROOT/"Docs/Art/art-asset-manifest.json").read_text(encoding="utf-8"))["counts"]
    (OUTPUT.parent / "final-run-manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"tests": tests, "captures": len(captures), "motion": [{k: v for k, v in m.items() if k != "runtime_frames"} for m in motion], "preserved_files": len(preserved), "diffs": manifest["diffs"]}, ensure_ascii=False, indent=2))


def main():
    if EXPLORATION or MARKER_SPRITES or SPECIALTY_ITEMS or ART_LIBRARY:
        exploration_evidence()
        return
    results = {}
    for name in ("smoke-results.txt", "restart-results.txt"):
        lines = (OUTPUT / name).read_text(encoding="utf-8-sig").splitlines()
        results[name] = {"pass_count": sum(line.startswith("PASS:") for line in lines),
                         "all_passed": lines[-1] == "ALL CHECKS PASSED"}
        assert results[name]["all_passed"], name
    unchanged = []
    for prefix in ("Scenes", "Prefabs", "Art/Runtime/Lumi", "Art/Runtime/NPCs"):
        for old in sorted((BASELINE / prefix).rglob("*")):
            if old.is_file():
                path = ROOT / "Assets/_Game" / old.relative_to(BASELINE)
                assert path.exists() and sha(path) == sha(old), str(path)
                unchanged.append(str(path.relative_to(ROOT)))
    for name in ("HoiAnData.json", "Content/GameplayContent.json"):
        path = ROOT / "Assets/_Game/Resources" / name
        assert sha(path) == sha(BEFORE / "Resources" / name), name
        unchanged.append(str(path.relative_to(ROOT)))
    cities = {"HoiAn": "hoi_an", "Hue": "hue", "DaLat": "da_lat",
              "Jeonju": "jeonju", "Gyeongju": "gyeongju", "Seoul": "seoul"}
    maps = []
    for source, city_id in cities.items():
        old = ROOT / f"LBC_AnhMoi_BanGiaoCodex_20261015/Maps/{source}/Map_Overview_A.png"
        new = ROOT / f"Assets/_Game/Art/Runtime/AdventureUI/City_{city_id}.png"
        assert sha(old) == sha(new), city_id
        maps.append(record(new))
    binary = ROOT / "Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll"
    mandatory = ["UI-01-CityMap-globe-backpack", "UI-02-WorldMap-from-globe",
                 "UI-03-Inventory-from-backpack", "UI-04-Lumi-dialogue", "UI-05-NPC-Mai-dialogue"]
    if UI1:
        mandatory = ["UI-01-CityMap-globe-backpack", "UI1-01b-CityMap-overview-paper",
                     "UI-02-WorldMap-from-globe", "UI1-03-Inventory-three-earned-items"]
    if MAP_ACTORS:
        mandatory = ["UI-01-CityMap-globe-backpack", "S1-01-Fresh-map-Mai-only-starting-region",
                     "S1-02-Gate1-only-next-region-merchant-crab", "S1-03b-NPC-and-bot-map-detail",
                     "S1-04-Battle-opens-station-region-only", "S1-05-Code-opens-same-region-only",
                     "S1-Restart-only-region2-and-actors"]
    if NODE_MARKERS:
        mandatory = ["MKR-01-Fresh-NPC-talk-and-hidden-boss", "MKR-02-Mai-needs-item-and-alley-reward",
                     "MKR-03-Mai-ready-to-receive-item", "MKR-04-Visible-boss-red-alert",
                     "MKR-05-BaTu-return-with-item", "MKR-06-Completed-locations-become-checkin-markers",
                     "S1-Restart-only-region2-and-actors"]
    for name in mandatory:
        path = OUTPUT / (name + ".png")
        assert path.stat().st_mtime > binary.stat().st_mtime, name
    captures = []
    for path in sorted(OUTPUT.glob("*.png")):
        item = record(path)
        with Image.open(path) as picture:
            item["resolution"] = list(picture.size)
        captures.append(item)
    alpha = []
    for name in ("Items_TransparentAtlas", "Hanji_Core", "Copper_Wire", "Healing_Tea", "NPC_TransparentAtlas", "Boatman_Portrait"):
        path = ROOT / f"Assets/_Game/Art/Runtime/AdventureUI/{name}.png"
        with Image.open(path) as picture:
            channel = picture.getchannel("A")
            assert channel.getextrema() == (0, 255), name
            alpha.append({**record(path), "alpha_extrema": [0, 255]})
    if UI1:
        for name in ("LBC_UI_SkinAtlas", "World_Globe"):
            path = ROOT / f"Assets/_Game/Art/Runtime/AdventureUI/Theme/{name}.png"
            with Image.open(path) as picture:
                extrema = picture.getchannel("A").getextrema()
                assert extrema[0] == 0 and extrema[1] >= 250, name
                alpha.append({**record(path), "alpha_extrema": list(extrema)})
        previous_catalog = (BEFORE / "Resources/AdventureUIArt.asset").read_text(encoding="utf-8-sig").rstrip()
        current_catalog = (ROOT / "Assets/_Game/Resources/AdventureUIArt.asset").read_text(encoding="utf-8-sig").split("\n  globe:")[0].rstrip()
        assert previous_catalog == current_catalog, "Existing city/item/NPC catalog references changed"
        for old in sorted((BEFORE / "Art").rglob("*")):
            if old.is_file():
                path = ROOT / "Assets/_Game/Art" / old.relative_to(BEFORE / "Art")
                assert sha(path) == sha(old), str(path)
                unchanged.append(str(path.relative_to(ROOT)))
        for old in sorted((BEFORE / "Scripts").rglob("*.cs")):
            if old.parent.name in ("Core", "Content", "HoiAn"):
                path = ROOT / "Assets/_Game/Scripts" / old.relative_to(BEFORE / "Scripts")
                assert sha(path) == sha(old), str(path)
                unchanged.append(str(path.relative_to(ROOT)))
    if MAP_ACTORS or NODE_MARKERS:
        # Every pre-existing artwork and importer, including the old NPC atlas,
        # must survive this targeted addition byte-for-byte.
        for area in ("Art", "Scripts/Core", "Scripts/HoiAn"):
            for old in sorted((BEFORE / area).rglob("*")):
                if old.is_file():
                    if NODE_MARKERS and old.relative_to(BEFORE).as_posix() == "Scripts/Core/GameData.cs":
                        continue  # Intentional append support; every other core file is preserved.
                    path = ROOT / "Assets/_Game" / old.relative_to(BEFORE)
                    assert sha(path) == sha(old), str(path)
                    unchanged.append(str(path.relative_to(ROOT)))
        for old in sorted((BEFORE / "Scripts/Content").glob("*.cs")):
            if not NODE_MARKERS and old.name == "NodeEventManager.cs":
                continue
            path = ROOT / "Assets/_Game" / old.relative_to(BEFORE)
            assert sha(path) == sha(old), str(path)
            unchanged.append(str(path.relative_to(ROOT)))
        old_presentation = json.loads((BEFORE / "Resources/Content/AdventurePresentation.json").read_text(encoding="utf-8-sig"))
        new_presentation = json.loads((ROOT / "Assets/_Game/Resources/Content/AdventurePresentation.json").read_text(encoding="utf-8-sig"))
        assert all(new_presentation[key] == value for key, value in old_presentation.items())
        old_catalog = (BEFORE / "Resources/AdventureUIArt.asset").read_text(encoding="utf-8-sig")
        new_catalog = (ROOT / "Assets/_Game/Resources/AdventureUIArt.asset").read_text(encoding="utf-8-sig")
        if NODE_MARKERS:
            assert old_catalog == new_catalog, "Marker rules must not rewrite the art catalog"
            marker_types = {"Normal", "Camera", "BossAlert", "NpcTalk", "NpcNeedItem", "Puzzle", "Reward", "Completed", "Quest", "Portal"}
            effects = {"None", "Blink", "Pulse", "Glow", "Bounce"}
            content = json.loads((ROOT / "Assets/_Game/Resources/Content/GameplayContent.json").read_text(encoding="utf-8-sig"))
            node_keys = {(c["id"], n["id"]) for c in content["cities"] for n in c["nodes"]}
            definitions = new_presentation["nodeMarkers"]
            assert len({(d["cityId"], d["nodeId"]) for d in definitions}) == len(definitions)
            for definition in definitions:
                assert (definition["cityId"], definition["nodeId"]) in node_keys
                for rule in [definition] + definition.get("rules", []):
                    assert rule.get("marker", "Normal") in marker_types
                    assert rule.get("effect", "None") in effects
            for name in ("MapDiscoveryManager.cs", "CityMapPaths.cs", "MapPanZoom.cs", "MapNodeHitTarget.cs", "CityRouteGeometry.cs"):
                path = ROOT / "Assets/_Game/Scripts/Maps" / name
                assert sha(path) == sha(BEFORE / "Scripts/Maps" / name), name
                unchanged.append(str(path.relative_to(ROOT)))

        def entries(catalog, field):
            block = re.search(r"(?ms)^  " + field + r":\n(.*?)(?=^  [A-Za-z_]\w*:|\Z)", catalog).group(1)
            return {key: re.sub(r"(?m)^    (source|status|portrait):.*\n", "", body).rstrip()
                    for key, body in re.findall(r"(?ms)^  - id: (.*?)\n(.*?)(?=^  - id: |\Z)", block)}

        for field in ("cities", "items", "npcs"):
            previous, current = entries(old_catalog, field), entries(new_catalog, field)
            assert all(current[key] == body for key, body in previous.items()), field
        for field in ("backpack", "coin", "globe", "journal", "backdrop", "window", "buttonTeal", "buttonPaper", "slot", "postcard"):
            pattern = r"(?m)^  " + field + r": (.*)$"
            assert re.search(pattern, old_catalog).group(1) == re.search(pattern, new_catalog).group(1), field
        for name in ("Enemies_HoiAn_TransparentAtlas", "Market_Merchant"):
            path = ROOT / f"Assets/_Game/Art/Runtime/AdventureUI/{name}.png"
            with Image.open(path) as picture:
                assert picture.mode == "RGBA" and picture.getchannel("A").getextrema() == (0, 255)
                alpha.append({**record(path), "resolution": list(picture.size), "alpha_extrema": [0, 255]})
        with Image.open(ROOT / "Assets/_Game/Art/Runtime/AdventureUI/Enemies_HoiAn_TransparentAtlas.png") as atlas:
            for box in ((32, 8, 632, 708), (640, 150, 1332, 704), (1420, 8, 2100, 708)):
                channel = atlas.getchannel("A").crop(box)
                bounds = channel.point(lambda p: 255 if p > 12 else 0).getbbox()
                assert bounds and bounds[0] > 2 and bounds[1] > 2 and bounds[2] < channel.width - 2 and bounds[3] < channel.height - 2, box
    unchanged = sorted(set(unchanged))
    manifest = {"generated_utc": datetime.now(timezone.utc).isoformat(), "runtime_tests": results,
                "diffs": {area: differences(area) for area in ("Scripts", "Resources")},
                "preserved_byte_identical": unchanged, "source_maps_preserved": maps,
                "transparent_art": alpha, "binary": record(binary),
                "player": record(ROOT / "Builds/Windows/LightBetweenCultures.exe"),
                "source": [record(p) for p in sorted((ROOT / "Assets/_Game/Scripts").rglob("*.cs"))],
                "logs": [record(ROOT / "Logs" / name) for name in
                         (("node-markers-build-final.log", "node-markers-player-final.log", "node-markers-restart-final.log") if NODE_MARKERS else
                          ("map-actors-stage1-build-final.log", "map-actors-stage1-player-final.log", "map-actors-stage1-restart-final.log") if MAP_ACTORS else
                          ("ui1-art-build-final.log", "ui1-art-player-final.log", "ui1-art-restart-final.log") if UI1 else
                          ("adventure-ui-build-final.log", "adventure-ui-player-final.log", "adventure-ui-restart-final.log"))],
                "captures": captures}
    if NODE_MARKERS:
        manifest["fixture_captures"] = [item["path"] for item in captures if "fixture" in item["path"].lower()]
    if UI1:
        before_output = OUTPUT.parent / "Before"
        before_lines = (before_output / "smoke-results.txt").read_text(encoding="utf-8-sig").splitlines()
        assert before_lines[-1] == "ALL CHECKS PASSED"
        assert all(p.stat().st_mtime < binary.stat().st_mtime for p in before_output.glob("*.png"))
        manifest["before_runtime"] = {"pass_count": sum(s.startswith("PASS:") for s in before_lines),
                                      "captures": [record(p) for p in sorted(before_output.glob("*.png"))]}
    (OUTPUT.parent / "final-run-manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2), encoding="utf-8")
    print(json.dumps({"tests": results, "captures": len(captures), "preserved_files": len(unchanged), "diffs": manifest["diffs"]}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
