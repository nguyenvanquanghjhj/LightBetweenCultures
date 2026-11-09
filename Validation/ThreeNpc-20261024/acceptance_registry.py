from pathlib import Path
import json
import importlib.util

root = Path(__file__).resolve().parents[2]
path = root / "Docs/Art/NPC_CANONICAL_REGISTRY.json"
data = json.loads(path.read_text(encoding="utf-8"))
traits = {
    "npc_kang_taeho": "Nam trẻ, trang phục canh giữ xanh, mũ xanh và giáo; giữ nguyên gương mặt người canh giữ đã chọn.",
    "npc_choi_eunsu": "Nam trẻ, tóc nâu, áo xanh, nón và hai giỏ cây; khác thương nhân trung niên có râu canonical.",
    "npc_han_doyun": "Nam trẻ, mũ đầu bếp trắng và chảo; giữ nguyên artwork đã chọn, nhân vật chăm góc xanh và giúp việc ở quầy công viên."
}
results = root / "Validation/ThreeNpc-20261024/acceptance-runtime-results.txt"
restart = root / "Validation/ThreeNpc-20261024/acceptance-restart-results.txt"
passed = all(p.exists() and p.read_text(encoding="utf-8-sig").rstrip().endswith("ALL CHECKS PASSED") for p in (results, restart))
bindings = json.loads((root / "Docs/Art/native-art-bindings.json").read_text(encoding="utf-8-sig"))
for identity in data["identities"]:
    npc = identity["npcId"]
    if npc not in traits:
        continue
    identity["canonicalIdentity"]["coreTraits"] = traits[npc]
    identity["canonicalIdentity"]["approval"] = "Owner authorized existing-art casting 2026-10-24 and map integration 2026-10-25; existing identity retained."
    identity["status"] = "OWNER_AUTHORIZED_CAST_RUNTIME_VERIFIED" if passed else "OWNER_AUTHORIZED_CAST_RUNTIME_PENDING"
    actual = next(b for b in bindings["entries"] if b["role"] == "npc" and b["id"] == npc)
    for variant in identity["variants"]:
        for key in ("path", "spriteName", "guid", "localId"):
            variant[key] = str(actual[key])
        variant["status"] = identity["status"]
        variant["scenes"] = ["CityMap", "DialoguePanel"]
        variant["conditions"] = "Existing map actor renderer at the same canonical node; original one-time reward keys retained."
        variant["qualityStatus"] = "WINDOWS_MAP_PORTRAIT_SAVE_RUNTIME_PASS" if passed else "NATIVE_IMPORTED_RUNTIME_REVIEW_PENDING"
        variant["visualReview"] = "Same source art, native sprite mesh, no bitmap generation; see Validation/ThreeNpc-20261024 runtime captures."
data["updated"] = "2026-10-25"
path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

# The full inventory command was attempted separately and stopped on an old Hue
# migration hash. Apply the existing exact-reference metadata merge, without
# changing that historical manifest or bypassing its failure status.
spec = importlib.util.spec_from_file_location("art_asset_catalog", root / "Tools/art_asset_catalog.py")
catalog = importlib.util.module_from_spec(spec)
spec.loader.exec_module(catalog)
catalog.apply_npc_identity_metadata(bindings["entries"])
(root / "Docs/Art/native-art-bindings.json").write_text(json.dumps(bindings, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
catalog.write_npc_identity_registry()
print("Three NPC registry UTF-8 and exact-reference approval merge updated; runtime verified:", passed)
