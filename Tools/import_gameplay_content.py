"""Deterministic typed runtime adapter for the supplied LBC gameplay pack.

Source files are read-only. Hoi An calibrated runtime anchors/routes remain the
authority; the other five graphs are explicitly unreviewed design scaffolds.
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "LBC_Gameplay_6Cities_Detailed_v1"
OUT = ROOT / "Assets/_Game/Resources/Content"


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def item(item_id, amount=1):
    return {"itemId": item_id, "amount": amount}


def normalize(x, y, scale=1000):
    return {"x": round(x / scale, 6), "y": round(1 - y / scale, 6)}


def validate(catalog):
    """Reject broken references during import, before Unity ever consumes data."""
    item_ids = {i["id"] for i in catalog["items"]}
    enemy_ids = {e["id"] for e in catalog["enemies"]}
    quest_ids = {q["id"] for q in catalog["quests"]}
    puzzle_ids = {p["id"] for p in catalog["puzzles"]}
    for group in ["items", "enemies", "quests", "puzzles"]:
        values = [v["id"] for v in catalog[group]]
        assert len(values) == len(set(values)), f"Duplicate {group} IDs"
    for city in catalog["cities"]:
        nodes = {n["id"] for n in city["nodes"]}
        events = {e["id"] for e in catalog["events"] if e["cityId"] == city["id"]}
        assert city["spawnNodeId"] in nodes
        assert len(city["gates"]) == 4
        assert all(n["eventId"] in events for n in city["nodes"])
        assert all(0 <= n["revealStage"] <= 4 for n in city["nodes"])
        assert all(0 <= n["position"]["x"] <= 1 and 0 <= n["position"]["y"] <= 1 for n in city["nodes"])
        assert all(e["from"] in nodes and e["to"] in nodes for e in city["edges"])
        assert all(g["nodeId"] in nodes for g in city["gates"])
    for e in catalog["events"]:
        assert not e["enemyId"] or e["enemyId"] in enemy_ids, e["id"]
        assert not e["questId"] or e["questId"] in quest_ids, e["id"]
        assert not e["puzzleId"] or e["puzzleId"] in puzzle_ids, e["id"]
        for amount in e["requiredItems"] + e["rewardItems"]:
            assert amount["itemId"] in item_ids and amount["amount"] > 0, e["id"]
    for q in catalog["quests"]:
        for amount in q["requiredItems"] + q["rewardItems"] + q["acceptRewardItems"]:
            assert amount["itemId"] in item_ids and amount["amount"] > 0, q["id"]


STAGES = {
    "hoi_an": [[0, 1, 2, 11], [3, 4, 9, 10, 12, 13, 21], [5, 14, 16, 18, 19], [6, 7, 15, 17], [8, 20]],
    "hue": [[0, 1, 2, 3, 15, 16], [4, 7, 10, 11, 12, 17], [5, 8, 9, 14], [6, 18, 19, 21], [13, 20]],
    "da_lat": [[0, 1, 2, 3, 4], [5, 6, 7, 8, 18], [9, 10, 11, 12, 13], [14, 15, 16, 17, 19, 20], [21]],
    "jeonju": [[0, 1, 2, 5], [3, 4, 6, 7, 8, 9, 10, 11, 12], [13, 14, 15, 16, 17, 21], [18, 19], [20]],
    "gyeongju": [[0, 1, 2, 3], [4, 5, 6, 9, 10, 11, 16], [7, 8, 12, 14, 15, 17, 21], [13, 18, 19], [20]],
    "seoul": [[0, 1, 2, 4, 7, 12, 13, 14], [3, 5, 6, 8, 9, 16], [10, 11, 17, 18, 22], [15, 19, 20], [21]],
}
# Later-city stages use the same schema, but require route and gate playtesting
# before runtimeReady may be changed. They are not six completed chapters.
GATES = {
    "hoi_an": [("H2", "quest:HQ01"), ("H13", "hoi_an.hanji_core_obtained"), ("H5", "hoi_an.mo01_disabled"), ("H7", "hoi_an.lamp_restored")],
    "hue": [("U3", "hue.crossing_permit"), ("U17", "hue.archive_decoded"), ("U5", "hue.guard_disabled"), ("U19", "hue.beacon_lit")],
    "da_lat": [("D2", "quest:DQ01"), ("D8", "da_lat.fog_cleared"), ("D10", "da_lat.dog_fed"), ("D14", "da_lat.beacon_lit")],
    "jeonju": [("J2", "quest:JQ01"), ("J12", "jeonju.three_patterns_joined"), ("J17", "jeonju.lantern_puzzle_solved"), ("J19", "jeonju.beacon_lit")],
    "gyeongju": [("G3", "quest:GQ01"), ("G4", "gyeongju.three_stars_placed"), ("G7", "gyeongju.stone_guard_friend"), ("G19", "gyeongju.beacon_lit")],
    "seoul": [("S7", "quest:SQ01"), ("S16", "seoul.relay_a_on"), ("S18", "seoul.relays_ready"), ("S20", "seoul.final_cleared")],
}


def main():
    world = read(SOURCE / "Data/world_progression.json")
    world_cities = {c["id"]: c for c in world["cities"]}
    runtime_ha = read(ROOT / "Assets/_Game/Resources/HoiAnData.json")
    catalog = dict(schemaVersion=1, source="LBC_Gameplay_6Cities_Detailed_v1 + reviewed HoiAnData anchors", cities=[], events=[], items=[], enemies=[], quests=[], puzzles=[], resonances=[])
    audit = dict(sourceFiles=[], conflicts=[], runtimeExtensions=[], counts={})
    for p in sorted(SOURCE.rglob("*")):
        if p.is_file():
            audit["sourceFiles"].append(dict(path=str(p.relative_to(ROOT)).replace("\\", "/"), bytes=p.stat().st_size, sha256=hashlib.sha256(p.read_bytes()).hexdigest()))

    types = {"npc": "npc", "boss": "encounter", "final boss": "encounter", "enemy": "encounter", "item": "chest", "puzzle": "puzzle", "beacon": "beacon", "portal": "portal", "free exploration": "exploration", "secret": "secret"}
    for path in sorted((SOURCE / "Data/Cities").glob("*.json")):
        src = read(path)
        cid = src["city_id"]
        stage_groups = STAGES[cid]
        gates = [dict(nodeId=n, condition=c, unlockStage=i + 1) for i, (n, c) in enumerate(GATES[cid])]
        story = src["city_story"]
        city = dict(id=cid, name=src["city_name"], backgroundId="Runtime/CityMaps/" + src["map_background_relative_to_NEW_ART_PACK"].split("/")[1] + "_Map_Overview_A", sourceBackground=src["map_background_relative_to_NEW_ART_PACK"], spawnNodeId=src["spawn_node_id"], completionFlag=src["city_completion_flag"], opening=story["opening"], story=story["main"], twist=story["twist"], ending=story["end"], runtimeReady=cid == "hoi_an", routesReviewed=cid == "hoi_an", unlockFlags=world_cities[cid]["unlocks_when"]["all_flags"], nodes=[], edges=[], gates=gates)
        for sn in src["nodes"]:
            index = int(sn["id"][1:])
            gate = next((g for g in gates if g["nodeId"] == sn["id"]), None)
            stage = next(i for i, indexes in enumerate(stage_groups) if index in indexes)
            node = dict(id=sn["id"], name=sn["name"], kind=sn["kind"], eventId=sn["event_id"], backgroundId="", position=normalize(sn["x"], sn["y"]), revealStage=stage, isKeyGate=gate is not None, gateCondition=gate["condition"] if gate else "", unlockStage=gate["unlockStage"] if gate else 0, requiredFlags=sn["visible_when"]["all_flags"])
            if cid == "hoi_an":
                node["position"] = runtime_ha["nodes"][index]["position"]
                if sn["id"] == "H9": node["requiredFlags"] = ["hoi_an.secret_lane_found"]
            city["nodes"].append(node)
        for se in src["edges"]:
            city["edges"].append(dict(id=se["id"], **{"from": se["from"]}, to=se["to"], bidirectional=se["bidirectional"], waypoints=[normalize(x, y) for x, y in se["waypoints"]], requiredFlags=se["all_flags"], travel=se["travel"], status=se["status"]))
        # Hoi An existing calibrated graph is retained; source graph is preserved
        # separately below for review, never silently substituted for working roads.
        if cid == "hoi_an":
            city["edges"] = [dict(id=f'H{e["from"]}__H{e["to"]}', **{"from": f'H{e["from"]}'}, to=f'H{e["to"]}', bidirectional=True, waypoints=e.get("bends", []), requiredFlags=[], travel="walk", status="existing_calibrated_runtime_route") for e in runtime_ha["edges"]]
            city["edges"].append(dict(id="H17__H5",**{"from":"H17"},to="H5",bidirectional=True,travel="ferry",status="HQ04_ferry_route_reviewed_for_current_anchors",requiredFlags=["hoi_an.ferry_available"],waypoints=[{"x":1305/1448,"y":1-657/1086},{"x":1284/1448,"y":1-693/1086},{"x":1235/1448,"y":1-709/1086}]))
        catalog["cities"].append(city)
        for se in src["events"]:
            event = dict(id=se["id"], cityId=cid, nodeId=se["node"], type=types.get(se["event_type"].lower(), se["event_type"].lower()), title=se["actor"], actor=se["actor"], text=se["dialogue_open"] or se["interaction"], npcId=se["id"] if se["event_type"].lower() == "npc" else "", questId="", itemId="", itemCount=0, enemyId=se["id"] if se["event_type"].lower() in ["enemy", "boss", "final boss"] else "", puzzleId=se["id"] if se["event_type"].lower() == "puzzle" else "", requiredFlags=[], requiredItems=[], rewardItems=[], flags=[], rewardCoins=0, completionFlag="", oneShot=se["one_shot"], mandatory=se["event_type"].lower() in ["boss", "final boss"], autoCollect=False, sourceStatus="runtime_configured_hoi_an" if cid == "hoi_an" else "imported_design_not_playable", backgroundId="")
            for grant in se["grants_when_completed"]:
                if grant["type"] == "flag": event["flags"].append(grant["id"])
                elif grant["type"] == "item":
                    if grant["id"] == "coin": event["rewardCoins"] += int(grant["amount_or_value"])
                    else: event["rewardItems"].append(item(grant["id"], int(grant["amount_or_value"])))
            for condition in se["requirements"]:
                if condition.startswith("inventory."):
                    key, quantity = condition[len("inventory."):].split(">=")
                    event["requiredItems"].append(item(key, int(quantity)))
                else: event["requiredFlags"].append(condition)
            catalog["events"].append(event)

    raw_items = read(SOURCE / "Data/items.json")["items"]
    catalog["items"] = [dict(id=i["id"], name=i["name"], type=i["type"], stackLimit=i["stack_limit"], healHP=14 if i["id"] == "healing_tea" else 0, source="LBC gameplay items.json; healing strength runtime-balanced") for i in raw_items]
    by_name = {c["name"]: c["id"] for c in catalog["cities"]}
    for se in read(SOURCE / "Data/enemies.json")["enemies"]:
        catalog["enemies"].append(dict(id=se["id"], cityId=by_name[se["city"]], name=se["name"], level=se["level"], hp=se["hp"], attack=se["attack"], defense=se["defense"], skills=se["skills"], dropOnce=se["drop_once"], rewardItems=[] if se["drop_once"] in ["coin", "ending"] else [item(se["drop_once"])], rewardCoins=8 if se["drop_once"] == "coin" else 0, alternativeResolution=se["alternative_resolution"], completionFlag="hoi_an.mo01_defeated" if se["id"] == "enemy_mo01" else by_name[se["city"]] + "." + se["id"] + "_defeated", replayable=se["replayable"], balanceProposal=se["stats_are_balance_proposals"]))
    for sq in read(SOURCE / "Data/quests.json")["quests"]:
        quest = dict(id=sq["id"], cityId=sq["city"], title=sq["name"], giverNode=sq["start_node"], completeAtNode=sq["start_node"], completionFlag=sq["completion_flag"], objectives=sq["objectives_ordered"], requiredItems=[], requiredFlags=[], acceptFlags=[], rewardItems=[], acceptRewardItems=[], flags=[], rewardCoins=0, consumeItems=False, rewardOnce=sq["reward_once"], offer=sq["name"] + ": " + sq["reward_description"], reminder="Các mục tiêu còn lại được ghi trong nhật ký.", thanks="Cảm ơn Lumi!", sourceStatus="runtime_configured_hoi_an" if sq["city"] == "hoi_an" else "imported_design_not_playable")
        catalog["quests"].append(quest)
    quests = {q["id"]: q for q in catalog["quests"]}
    quests["HQ01"].update(completeAtNode="H7", requiredItems=[item("lamp_eye"), item("hanji_core"), item("link_key")], requiredFlags=["hoi_an.mo01_disabled"], rewardCoins=150, consumeItems=True, acceptRewardItems=[item("healing_tea", 2)], flags=["city.hue.unlocked", "city.jeonju.unlocked"], offer="Đèn lồng trong phố tắt cùng lúc. Cháu giúp cô tìm Mắt Đèn trong hẻm H3, ghép Lõi Giấy Sáng ở xưởng H13 và lấy Chìa Kết Nối từ bot H5 nhé. Mang cả ba tới trạm H7 để khôi phục ánh sáng.", reminder="Mắt Đèn ở H3; chợ H4 giữ manh mối cho xưởng H13. Ông Phúc biết cách đọc mã gốm ở H19 nếu cháu muốn tránh giao chiến với bot.", thanks="Ánh sáng đã trở về! Huế và Jeonju cùng đáp lại tín hiệu của Hội An. Cháu có thể đi tiếp hoặc ở lại giúp bà con.")
    quests["HQ02"].update(completeAtNode="H10", requiredItems=[item("copper_wire")], requiredFlags=["hoi_an.crab_resolved"], consumeItems=True, acceptRewardItems=[item("meat_portion")], flags=["hoi_an.secret_lane_found"], offer="Con cua kéo cuộn dây đồng của bà xuống mép nước H12. Cháu lấy lại giúp bà nhé. Đây là một phần thức ăn; thử dụ nó ra trước khi phải đánh.", reminder="Cua Trộm ở H12 giữ cuộn dây. Có thể giao chiến hoặc dùng phần thức ăn bà đã đưa.", thanks="Đúng cuộn dây của bà rồi! Đây là dấu chỉ ngõ đèn thất lạc H9; cháu nhớ ghé xem nhé.")
    quests["HQ03"].update(completeAtNode="H9", requiredItems=[item("reply_letter")], offer="Một chiếc đèn lạc đang chờ tin từ Jeonju. Khi có thư hồi âm J21, hãy mang về ngõ H9.", reminder="Thư hồi âm nằm ở Jeonju J21. Nhánh này chưa mở nội dung đầy đủ trong checkpoint hiện tại.", sourceStatus="cross_city_quest_not_playable")
    quests["HQ04"].update(completeAtNode="H17", requiredItems=[item("coin", 5)], consumeItems=True, offer="Bác cần năm xu để sửa mái chèo và mở chuyến đò qua nhánh đông. Khi sẵn sàng, mang tiền lại đây nhé.", reminder="Mang năm xu đến H17; đường đò là lối tắt tùy chọn.", thanks="Mái chèo đã sửa xong. Bác sẽ đưa cháu qua bến H5 bất cứ khi nào cần.")
    events = {(e["cityId"], e["id"]): e for e in catalog["events"]}

    def event(eid, **fields):
        events[("hoi_an", eid)].update(fields)

    event("npc_mai", questId="HQ01", legacyTutorial=True, completionFlag="hoi_an.lamp_restored", text=quests["HQ01"]["offer"])
    event("scene_roof_alley", sceneId="HoiAnDemo", rewardItems=[], itemId="lamp_eye", itemCount=1)
    event("puzzle_market", puzzleId="puzzle_market", completionFlag="hoi_an.market_puzzle_solved", rewardItems=[], flags=[])
    event("puzzle_paper", puzzleId="puzzle_paper", requiredFlags=["hoi_an.market_puzzle_solved"], completionFlag="hoi_an.hanji_core_obtained", rewardItems=[], flags=[])
    event("enemy_mo01", enemyId="enemy_mo01", mandatory=True, completionFlag="hoi_an.mo01_disabled", alternateFlag="hoi_an.mo01_hack_code", alternateChoice="hack", alternateText="Dùng mã Hoa – Trăng – Đèn", rewardItems=[item("link_key")], flags=["hoi_an.mo01_disabled"])
    event("restore_lantern", questId="HQ01", requiredFlags=["hoi_an.mo01_disabled"], requiredItems=[item("lamp_eye"), item("hanji_core"), item("link_key")], completionFlag="hoi_an.lamp_restored", flags=["hoi_an.lamp_restored"], rewardItems=[])
    event("portal_jeonju", destinationCity="jeonju", destinationNode="J0", requiredFlags=["hoi_an.lamp_restored"], text="Cổng đã nhận tín hiệu. Jeonju đang sáng ở phía bên kia. Bạn có thể tiếp tục hoặc ở lại Hội An.")
    event("portal_hue", destinationCity="hue", destinationNode="U0", requiredFlags=["hoi_an.lamp_restored"], text="Cổng đã nhận tín hiệu. Tuyến đi Huế đã mở; bạn vẫn có thể quay lại làm nhiệm vụ phụ.")
    event("chest_secret", type="secret", questId="HQ03", oneShot=True, requiredFlags=["hoi_an.secret_lane_found"], rewardCoins=12, flags=["hoi_an.secret_chest_opened"], completionFlag="hoi_an.secret_chest_opened", text="Theo bản đồ của Bà Tư, Lumi tìm thấy chiếc hòm sau dãy đèn. Một dòng nhắn hướng về Jeonju; cần thư hồi âm để giúp chiếc đèn lạc.")
    event("npc_ba_tu", questId="HQ02", completionFlag="hoi_an.ba_tu_helped", rewardItems=[], flags=[], text=quests["HQ02"]["offer"])
    event("loot_crate", type="chest", completionFlag="hoi_an.dock_crate_opened", flags=["hoi_an.dock_crate_opened"], rewardCoins=6, text="Rương bến nước chứa sáu đồng xu. Số tiền này có thể dùng cho chuyến đò phía đông.")
    event("enemy_crab", enemyId="enemy_crab", mandatory=False, alternateItemId="meat_portion", alternateChoice="bait", alternateText="Dùng thức ăn dụ cua", completionFlag="hoi_an.crab_resolved", rewardItems=[item("copper_wire")], flags=["hoi_an.crab_resolved"])
    event("npc_phuc", type="dialogue", oneShot=True, completionFlag="hoi_an.pottery_clue_known", flags=["hoi_an.pottery_clue_known"], rewardItems=[item("healing_tea")], text="Có người đã thay mã trạm bằng hình gốm. Cháu nhìn ngõ H19: hoa nở trước, trăng lên sau, cuối cùng mới thắp đèn. Đây là chút trà giúp cháu hồi sức.")
    event("enemy_ghost", enemyId="enemy_ghost", mandatory=False, completionFlag="hoi_an.ghost_freed", flags=["hoi_an.ghost_freed"], text="Chiếc đèn yếu đang quay quanh lối đông. Nó chỉ phản ứng với tín hiệu nhiễu; đây là thử thách tùy chọn.")
    event("npc_boatman", questId="HQ04", completionFlag="hoi_an.ferry_available", flags=[], rewardItems=[], text=quests["HQ04"]["offer"])
    event("npc_guest", questId="HQ03", completionFlag="", text="Tôi từng thấy hoa văn ấy ở làng Jeonju. Một chiếc đèn bị gọi là thất lạc có lẽ đang che chở cho ai đó. Nếu tìm thấy thư hồi âm bên kia cổng, hãy đưa về ngõ H9.")
    event("puzzle_pottery", puzzleId="puzzle_pottery", completionFlag="hoi_an.mo01_hack_code", requiredFlags=[], flags=[], rewardItems=[])
    event("loot_attic", type="chest", oneShot=True, rewardCoins=10, flags=["hoi_an.attic_clue_found"], completionFlag="hoi_an.attic_clue_found", text="Trong hòm áp mái có mười xu và mảnh giấy: 'Chợ giữ mẫu đèn, xưởng giữ lõi giấy; mã bot nằm trên gốm.'")
    puzzles = [
        dict(id="puzzle_market", cityId="hoi_an", nodeId="H4", title="Hoa văn chợ ven sông", mode="sequence", clue="Ba tấm biển bên sạp mô tả một buổi tối: hoa nở trước, trăng lên rồi đèn sáng. Chạm ba biểu tượng theo thứ tự đó.", options=["Hoa", "Trăng", "Đèn"], answer="Hoa|Trăng|Đèn", requiredFlags=[], flags=["hoi_an.market_puzzle_solved"], completionFlag="hoi_an.market_puzzle_solved", rewardItems=[], rewardCoins=0, sourceStatus="runtime_authored_solution_for_source_three_pattern_puzzle"),
        dict(id="puzzle_paper", cityId="hoi_an", nodeId="H13", title="Lõi Giấy Sáng", mode="choice", clue="Mẫu đầu tiên tại sạp chợ là hoa. Mảnh giấy đúng phải giữ đúng nét hoa ấy; chọn mẫu Hoa để ghép lõi.", options=["Hoa", "Trăng", "Đèn"], answer="Hoa", requiredFlags=["hoi_an.market_puzzle_solved"], flags=["hoi_an.hanji_core_obtained"], completionFlag="hoi_an.hanji_core_obtained", rewardItems=[item("hanji_core")], rewardCoins=0, sourceStatus="runtime_authored_three_choice_solution_using_H4_clue"),
        dict(id="puzzle_pottery", cityId="hoi_an", nodeId="H19", title="Mật mã ngõ gốm", mode="sequence", clue="Tranh gốm khắc ba cảnh nối tiếp: Hoa – Trăng – Đèn. Sao chép đúng thứ tự để nhận mã vô hiệu hóa Bot Mờ-01.", options=["Hoa", "Trăng", "Đèn"], answer="Hoa|Trăng|Đèn", requiredFlags=[], flags=["hoi_an.mo01_hack_code"], completionFlag="hoi_an.mo01_hack_code", rewardItems=[], rewardCoins=0, sourceStatus="source_exact_sequence"),
    ]
    # Unconfigured puzzle records preserve scope honestly: imported definitions
    # have no invented answer and cannot be advertised as playable content.
    for e in catalog["events"]:
        if e["type"] == "puzzle" and e["cityId"] != "hoi_an":
            puzzles.append(dict(id=e["id"], cityId=e["cityId"], nodeId=e["nodeId"], title=e["title"], mode="unconfigured", clue=e["text"], answer="", options=[], requiredFlags=e["requiredFlags"], flags=e["flags"], rewardItems=e["rewardItems"], rewardCoins=e["rewardCoins"], sourceStatus="design_puzzle_needs_runtime_authoring_and_verification"))
    catalog["puzzles"] = puzzles
    for sr in world["cross_city_resonance"]:
        catalog["resonances"].append(dict(sourceFlag=sr.get("source_flag", ""), sourceItem=sr.get("source_item", ""), destinationCity=sr["destination_city"], effect=sr["effect"]))
    audit["conflicts"] = [
        "New pack exact IDs lamp_eye / hanji_core supersede older lantern_lens / paper_core names; legacy lantern-part remains in tutorial adapter.",
        "Source Hoi An graph has 29 design edges but working HoiAnData has 23 calibrated routes. Runtime retains 23 walking routes and adds the source HQ04 ferry shortcut; source29 remains in SourceCities for review.",
        "H11 description says copper wire, but explicit grants_when_completed says 6coin; choose explicit6coin so HQ02wire still requirescrab.",
        "Source coin stack_limit99 cannot hold HQ01reward150; runtime Inventory treats currency separately withoutstackcap.",
        "New user confirmed chibi map/exploration roles overrides olderartbible canonical-navy-only guidance; this importer does nottouchart.",
        "Latest progressive reveal requires hiding later nodes entirely, replacing source lockicon suggestion and oldfuture gates.",
    ]
    audit["runtimeExtensions"] = [
        "Four reveal gates per city authored from local source events. Hoi An configured for runtime services; remainingfive are metadata only/notplaytested.",
        "HQ01accept grants2healing_tea; tea restores14HP. Balance values authored for first battle, source only namesconsumable.",
        "HQ02accept grants1meat_portion as crab bait; source alternative says bait but doesnotdefineitemidentity/source.",
        "H4andH13source lack exact solution; runtime explicit visibleclues define Hoa-Trang-Den sequence and Hoa choice, respectively.",
        "H14clue writtenfromsource hint and grants1healing_tea once; H21chest10coin,H9chest12coin are authoredquantities.",
        "Otherfive cities keep sourcenodes/routes/events and exactstats,but runtimeReady=false; routesremain design_route_must_be_reviewed.",
    ]
    audit["counts"] = dict(sourceNodes=sum(len(read(p)["nodes"]) for p in (SOURCE / "Data/Cities").glob("*.json")), sourceRoutes=sum(len(read(p)["edges"]) for p in (SOURCE / "Data/Cities").glob("*.json")), cities=len(catalog["cities"]), events=len(catalog["events"]), items=len(catalog["items"]), enemies=len(catalog["enemies"]), quests=len(catalog["quests"]), runtimeHoiAnRoutes=len(next(c for c in catalog["cities"] if c["id"] == "hoi_an")["edges"]))
    assert audit["counts"]["sourceNodes"] == 133 and audit["counts"]["sourceRoutes"] == 165
    assert len(catalog["items"]) == 31 and len(catalog["enemies"]) == 23 and len(catalog["quests"]) == 24
    validate(catalog)
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / "GameplayContent.json").write_text(json.dumps(catalog, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    sources = OUT / "SourceCities"
    sources.mkdir(exist_ok=True)
    for p in (SOURCE / "Data/Cities").glob("*.json"):
        (sources / p.name).write_bytes(p.read_bytes())
    report = ROOT / "Validation/GameplayAB-20261016"
    report.mkdir(parents=True, exist_ok=True)
    (report / "content-import-audit.json").write_text(json.dumps(audit, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(audit["counts"]))


if __name__ == "__main__":
    main()
