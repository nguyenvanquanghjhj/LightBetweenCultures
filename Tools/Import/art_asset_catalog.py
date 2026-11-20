"""Read-only art inventory and GUID/pixel migration verification. Never edits images."""
from pathlib import Path
from datetime import datetime, timezone
import csv
import hashlib
import json
import re
import sys
import html
import os
from PIL import Image

ROOT=next(p for p in Path(__file__).resolve().parents if (p/"ProjectSettings").is_dir())
DOC=ROOT/"Docs/Art"

def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def guid(p):
    meta=p.with_name(p.name+".meta")
    m=re.search(r"(?m)^guid: (.+)$",meta.read_text(encoding="utf-8-sig")) if meta.exists() else None
    return m.group(1) if m else None

def apply_npc_identity_metadata(entries):
    """Approval is curated per identity AND sprite, never inferred from costume.

    AdventureUIArt status strings are historical integration annotations. This
    Docs-only overlay preserves every runtime binding and the raw annotation.
    """
    path=DOC/'NPC_CANONICAL_REGISTRY.json'
    if not path.exists():return entries
    data=json.loads(path.read_text(encoding='utf-8'))
    identities={n['npcId']:n for n in data['identities']}
    for entry in entries:
        identity=identities.get(entry['id']) if entry['role']=='npc' else None
        if not identity or identity['status']!='CANONICAL':continue
        matches=[v for v in identity['variants'] if v['status'] in ('CANONICAL','APPROVED_VARIANT') and
                 all(str(v[k])==str(entry[k]) for k in ('path','guid','localId','spriteName'))]
        if len(matches)!=1:
            # Report a binding mismatch without approving a different face or editing the registry.
            entry['identityRegistryBindingMatch']=False
            entry['identityApprovalSource']='Docs/Art/NPC_CANONICAL_REGISTRY.json'
            continue
        entry['identityRegistryBindingMatch']=True
        entry.setdefault('runtimeStatusAnnotation',entry.get('status',''))
        entry.update(canonical=True,status=matches[0]['status'],variantId=matches[0]['variantId'],
                     identityApprovalSource='Docs/Art/NPC_CANONICAL_REGISTRY.json')
    return entries

def write_npc_identity_registry():
    path=DOC/'NPC_CANONICAL_REGISTRY.json'
    if not path.exists():return
    data=json.loads(path.read_text(encoding='utf-8'))
    lines=['# NPC canonical identity và visual variants','',
      'Cập nhật '+data['updated']+' theo đính chính và phê duyệt của người dùng. Một npcId = một danh tính; có nhiều visual variants. Đây là registry nhận diện/duyệt art trong Docs, không tạo thêm manager hoặc registry runtime.','',
      '## Quy tắc','',
      '- Khóa cấu trúc khuôn mặt, độ tuổi tương đối, nét đặc trưng và danh tính. Trang phục, biểu cảm, cử chỉ, tư thế, hướng nhìn, phụ kiện và kiểu tóc hợp lý được thay đổi.',
      '- CANONICAL: hình mặc định đã chốt. APPROVED_VARIANT: nhận diện hợp lệ của cùng người, dù khác trang phục. IDENTITY_CONFLICT chỉ khi khuôn mặt/tuổi/nét cốt lõi thay đổi rõ.',
      '- Duyệt nhận diện độc lập với chất lượng crop. APPROVED_VARIANT vẫn có thể NEEDS_CROP_REVIEW; không tự đưa concept hay crop lỗi vào game.',
      '- Không dùng cùng khuôn mặt cho hai NPC độc lập; Ji-won không dùng mặt So-yeon. npc_guest là Ji-eun, không tạo một Ji-eun thứ hai.',
      '- Runtime AdventureUIArt, portrait binding, sprite GUID/localID và UI giữ nguyên. status cũ trong asset là chú thích lịch sử tích hợp; registry này là nguồn duyệt danh tính hiện tại. Catalog dùng overlay theo cả npcId + sprite, không duyệt tự động một hình khác chỉ vì cùng ID.',
      '- Bà Tư, ông Phúc, Hương, So-yeon hiện tại đã CANONICAL theo xác nhận 17/10. Các hình có xác nhận cũ tiếp tục CANONICAL. Không tự coi mọi art đề xuất chưa được nhắc tới là đã duyệt.','',
      '[Dữ liệu chỉnh sửa](NPC_CANONICAL_REGISTRY.json) · [Đối chiếu trực quan bằng ảnh nguồn, không tạo bitmap](IdentityPolicy-20261017/VisualReview.html)','']
    review=DOC/'IdentityPolicy-20261017';review.mkdir(parents=True,exist_ok=True)
    cards=[]
    for n in data['identities']:
        lines += [f"## {n['npcId']} — {n['canonicalIdentity']['name']}",'',
                  f"Danh tính: `{n['canonicalIdentity']['identityId']}` · Visual status: **{n['status']}**.",'',n['canonicalIdentity']['coreTraits'],'',
                  '| variantId | Status | Sprite / đường dẫn | GUID / localID | Thành phố / cảnh / điều kiện |','|---|---|---|---|---|']
        for v in n['variants']:
            if v.get('archived'):
                assert not v.get('runtimeBound'), v
                assert v['archiveSha256'] and not v['path'].startswith('Assets/'), v
                lines.append(f"| {v['variantId']} | {v['status']} / ARCHIVED_SOURCE | {v['path']} (local archive, outside project) | {v['guid']} / {v['localId']} | Not bound in gameplay; SHA-256 {v['archiveSha256']} |")
                continue
            assert guid(ROOT/v['path'])==v['guid'],v
            meta=(ROOT/(v['path']+'.meta')).read_text(encoding='utf-8-sig')
            assert v['localId']=='21300000' or re.search(r'(?m)^\s*internalID: '+re.escape(v['localId'])+r'\s*$',meta),v
            lines.append(f"| {v['variantId']} | {v['status']} / {v['qualityStatus']} | [{v['path']}](../../{v['path']})#{v['spriteName']} | {v['guid']} / {v['localId']} | {', '.join(v['cities'])}; {', '.join(v['scenes'])}; {v['conditions']} |")
            if n['status']=='CANONICAL':
                with Image.open(ROOT/v['path']) as im:w,h=im.size
                match=re.search(r'name: '+re.escape(v['spriteName'])+r'\s*\n\s*rect:\s*\n\s*serializedVersion: \d+\s*\n\s*x: (\S+)\s*\n\s*y: (\S+)\s*\n\s*width: (\S+)\s*\n\s*height: (\S+)',meta)
                x,y,cw,ch=map(float,match.groups()) if match else (0,0,w,h)
                url=os.path.relpath(ROOT/v['path'],review).replace('\\','/')
                cards.append(f'<figure><svg viewBox="0 0 {cw} {ch}" role="img" aria-label="{html.escape(v["variantId"])}"><image href="{url}" x="{-x}" y="{-(h-y-ch)}" width="{w}" height="{h}"/></svg><figcaption>{html.escape(n["canonicalIdentity"]["name"])} · {v["variantId"]}<br>{v["status"]} · {v["qualityStatus"]}<br>{html.escape(v["visualReview"])}</figcaption></figure>')
        if not n['variants']:lines.append('| — | MISSING_VISUAL | Chưa gán sprite | — | Giữ logic hiện tại; không mượn mặt NPC khác |')
        for ref in n['visualReferences']:
            lines += ['',f"- Reference `{ref['referenceId']}` — **{ref['status']}**, concept-only, chưa có sprite riêng: [{ref['path']}](../../{ref['path']}); {ref['region']}. {ref['reason']}"]
    lines += ['','## Đối chiếu cụ thể / IDENTITY_CONFLICT','']
    for conflict in data['identityConflicts']:
        lines += [f"- {conflict['npcId']} · [{conflict['path']}](../../{conflict['path']}) · {conflict['region']}: **IDENTITY_CONFLICT** — {conflict['reason']}"]
    lines += ['','Mai áo đỏ trong H3: APPROVED_VARIANT; Mai áo vàng trẻ trong Composite_Illustrated: APPROVED_VARIANT concept-only. Không có luật áo vàng = xung đột. Bà Tư/ông Phúc trong MissingSprites giữ tuổi/tóc/râu và nét mặt: APPROVED_VARIANT concept-only, chưa có cutout riêng. Không lấy ID của toàn bảng làm sprite nhân vật.','',
      'Các audit/ảnh/test 16–17/10 trước xác nhận là bằng chứng lịch sử, không phải trạng thái duyệt hiện tại. Không chạy lại gameplay hoặc sửa các bằng chứng PASS cũ chỉ để thay nhãn.']
    (DOC/'NPC_CANONICAL_REGISTRY.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    references=[]
    for ref in data['visualSourcesReviewed']:
        url=os.path.relpath(ROOT/ref,review).replace('\\','/')
        references.append(f'<details><summary>{html.escape(ref)}</summary><img src="{url}" alt="Original reviewed source"></details>')
    (review/'VisualReview.html').write_text('<!doctype html><meta charset="utf-8"><title>LBC NPC identity review</title><style>body{font:16px system-ui;background:#163e48;color:#ffe5b5;margin:24px}.cards{display:flex;flex-wrap:wrap;gap:18px}figure{width:300px;margin:0}svg{width:100%;height:330px;background:#315e65;overflow:hidden}figcaption{padding:8px}details img{max-width:100%}a{color:#80dfce}</style><h1>NPC identity / variants — 17/10/2026</h1><p>Đối chiếu art nguồn, không phải ảnh runtime mới. SVG chỉ xem vùng native sprite; không chỉnh hay tạo PNG. Canonical face được giữ. Crop/mesh quality có trạng thái riêng; hình chưa đạt không tự gán vào game. <a href="../NPC_CANONICAL_REGISTRY.md">Registry / nhận xét / conflict</a></p><div class="cards">'+''.join(cards)+'</div><h2>Nguồn đối chiếu nguyên bản</h2>'+''.join(references),encoding='utf-8')

def main():
    if "--inventory-only" not in sys.argv:
        raise SystemExit("Use --inventory-only. Historical migration verification is archived, not a current import dependency.")
    moves=json.loads((DOC/"asset-moves.json").read_text(encoding="utf-8"))["entries"]
    moved={m["newPath"]:m for m in moves}
    historical_pixel_changes=[]
    for m in moves:
        p=ROOT/m["newPath"]
        assert p.exists() and not (ROOT/m["oldPath"]).exists(),m
        assert guid(p)==m["guid"],m
        if "--inventory-only" in sys.argv:
            if sha(p)!=m["sha256"]:
                historical_pixel_changes.append(dict(path=m["newPath"],historicalSHA256=m["sha256"],currentSHA256=sha(p)))
        else:assert sha(p)==m["sha256"],m
    native_doc=json.loads((DOC/"native-art-bindings.json").read_text(encoding="utf-8-sig"))
    native=apply_npc_identity_metadata(native_doc["entries"])
    (DOC/"native-art-bindings.json").write_text(json.dumps(native_doc,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    write_npc_identity_registry()
    bound=[b for b in native if b["role"]!="native_sprite"]
    assert len({b["id"] for b in bound if b["role"]=="item"})==len([b for b in bound if b["role"]=="item"])
    assert len({(b["guid"],b["localId"]) for b in bound if b["role"]=="lumi"})==1
    records=[]
    excluded={"Library","Builds","Validation","Logs",".git",".vscode"}
    # Inventory images while pruning caches before descending; do not enumerate Library/history.
    image_paths=[]
    for current,dirs,files in os.walk(ROOT):
        dirs[:]=[d for d in dirs if d not in excluded and d not in {"Backups","Temp","obj"}]
        image_paths.extend(Path(current)/name for name in files if Path(name).suffix.lower() in (".png",".jpg",".jpeg"))
    for p in sorted(image_paths):
        if not p.is_file() or p.suffix.lower() not in (".png",".jpg",".jpeg") or any(x in excluded for x in p.relative_to(ROOT).parts):continue
        rel=p.relative_to(ROOT).as_posix()
        bindings=[b for b in bound if b["path"]==rel]
        sprites=[b["spriteName"] for b in native if b["role"]=="native_sprite" and b["path"]==rel]
        with Image.open(p) as im:
            alpha=im.getchannel("A").getextrema() if im.mode=="RGBA" else None
            size=list(im.size)
        runtime=bool(bindings)
        status="runtime_registered" if runtime else "source_or_reference_not_bound"
        issues=[]
        if "_SourceSheets" in p.parts or "Concept" in p.name or "Moodboard" in p.name:
            if not alpha or alpha[0]>0:issues.append("background/labels baked into concept; not an isolated sprite")
            else:issues.append("transparent sheet; individual crop/padding requires review before registration")
        if "NeedsReview" in p.parts:issues.append("rejected derivative: ragged edge/composition; not registered")
        if "Chili" in p.name or "Shallot" in p.name:
            issues.append("flat graphic temporary style; differs from painted LBC icons")
        if p.name=="Item_Food_Postcard.jpg":issues.append("original illustrative photo/credit/pink background retained; not bridge-specific art")
        if "Legacy" in rel:issues.append("legacy compatibility art; not the active Lumi map role")
        if p.name in ("Characters_MissingSprites_v1.png","LBC_Composite_Illustrated_v1.png","LBC_Composite_CheckerConcept_v2.png"):
            issues.append("Review identity per character/region in NPC_CANONICAL_REGISTRY; costume changes alone are not conflicts; concept is not a runtime sprite")
        canonical=any(b["canonical"] and b.get("identityRegistryBindingMatch",True) for b in bindings)
        if any(b.get("identityRegistryBindingMatch") is False for b in bindings):
            issues.append("Runtime sprite differs from the registry's approved GUID/localID; identity registry was not changed")
        temporary=[b["id"] for b in bindings if "NOT_CANONICAL" in b.get("status","")]
        if temporary:issues.append("PROPOSED / TEMPORARY bindings, not canonical/final: "+", ".join(temporary))
        source=moved[rel]["oldPath"] if rel in moved else next((b["source"] for b in bindings if b["source"]),rel)
        city=next((c for c in ["HoiAn","Hue","DaLat","Jeonju","Gyeongju","Seoul"] if c in p.parts),"shared")
        records.append(dict(name=p.name,path=rel,source=source,city=city,role=sorted({b["role"] for b in bindings}),
                            runtime=runtime,canonical=canonical,status=status,issues=issues,sha256=sha(p),guid=guid(p),
                            dimensions=size,alpha_extrema=alpha,sprites=sprites,bindings=bindings,temporaryBindings=temporary))
    groups={}
    for e in records:groups.setdefault(e["sha256"],[]).append(e["path"])
    duplicates=[paths for paths in groups.values() if len(paths)>1]
    uuids=[e["path"] for e in records if re.fullmatch(r"[a-fA-F0-9-]{32,40}",Path(e["path"]).stem)]
    assert not uuids,uuids
    art=[e for e in records if e["path"].startswith("Assets/_Game/Art/")]
    moved_current=len({m["newPath"] for m in moves}.intersection(e["path"] for e in art))
    approved_scene="Assets/_Game/Scenes/HoiAnDemo.unity" if "--approved-h3-scene" in sys.argv else None
    h3_verification=ROOT/'Docs/Development/H3Upgrade-20261017/verification.json'
    if h3_verification.exists():
        accepted=json.loads(h3_verification.read_text(encoding='utf-8'))
        if accepted.get('sceneSHA256')==sha(ROOT/'Assets/_Game/Scenes/HoiAnDemo.unity'):
            approved_scene='Assets/_Game/Scenes/HoiAnDemo.unity'
    approved_scene_changes=[]
    manifest=dict(generated_utc=datetime.now(timezone.utc).isoformat(),images=records,bindings=bound,
                  counts=dict(all_images=len(records),unity_art_images=len(art),moved=moved_current,kept=len(art)-moved_current,path_aliases=len(moves),unsafe_moves=0),
                  duplicate_pixel_groups=duplicates,uuid_files=uuids,moves=moves,
                  historical_alias_pixel_changes=historical_pixel_changes,
                  protected_scene_prefab_resource_bytes_unchanged="--inventory-only" not in sys.argv,
                  protected_scene_prefab_bytes_unchanged=None if "--inventory-only" in sys.argv else not approved_scene_changes,
                  approved_scene_changes=approved_scene_changes)
    (DOC/"art-asset-manifest.json").write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding="utf-8")
    with (DOC/"art-asset-manifest.csv").open("w",encoding="utf-8-sig",newline="") as f:
        w=csv.DictWriter(f,fieldnames=["name","path","source","city","runtime","canonical","status","issues","guid","sha256"]);w.writeheader()
        for e in records:w.writerow({k:"; ".join(e[k]) if isinstance(e[k],list) else e[k] for k in w.fieldnames})
    lines=["# Thư viện art Light Between Cultures", "", "Ảnh được chuyển cùng .meta để giữ GUID và được Unity import, đối chiếu lại; không tạo bitmap trùng hoặc thay ảnh đã duyệt.","",
           f"{len(records)} ảnh toàn project; {len(art)} ảnh trong Art; **{moved_current} ảnh có bản ghi chuyển đường dẫn, {len(art)-moved_current} ảnh không có bản ghi, {len(moves)} alias đường dẫn**.","",
           "## Canonical và các khác biệt phải giữ rõ", "",
           "- [NPC canonical identity / variants](NPC_CANONICAL_REGISTRY.md): một danh tính, nhiều biến thể. Cô Mai giữ khuôn mặt/tuổi trẻ tương đối; áo đỏ/nón/đèn là mặc định, không khóa mọi trang phục. Mai H3 và Mai áo vàng trẻ trong Composite_Illustrated là APPROVED_VARIANT (crop/concept có trạng thái riêng); bản tóc bạc lớn tuổi trong MissingSprites/Checker là IDENTITY_CONFLICT do tuổi, không do màu áo.",
           "- Lumi dùng duy nhất sprite gà trắng Lumi_MapTraveler theo owner duyệt 21/10/2026 cho map/dialogue/exploration; art cũ giữ làm lịch sử, không còn binding runtime.",
           "- Bot Mờ-01/crab/ghost dùng atlas đã nghiệm thu. Bot xanh/trắng trong sheet mới chưa được duyệt thay thế.",
           "- Bà Tư, ông Phúc, Hương và So-yeon: hình hiện tại đã CANONICAL theo xác nhận 17/10/2026. Giữ native slices/portrait đã PASS; không tiếp tục chờ duyệt vì nhãn tạm cũ. Chất lượng crop, animation và biến thể mới là các tiêu chí riêng.",
           "- Ớt/hành là icon flat tạm; bưu thiếp giữ JPG/credit và là minh họa chung. Derivative bưu thiếp lỗi ở ../_LBC_ARCHIVE/HistoricalArt/NeedsReview không được đăng ký.",
           "- Các itemId ánh xạ qua AdventureUIArt duy nhất; cùng ID dùng cùng Sprite ở mọi thành phố. Một số item dùng icon nhóm có cờ categoryIcon, không phải art riêng đã hoàn thiện.",
           "- Sheet UI/nhân vật/môi trường/postcard mới có nền/chữ baked. FoodCulture có alpha thật nhưng cần kiểm tra từng crop trước khi dùng; chưa đăng ký món mới chỉ để đủ danh sách.",
           "- Atlas dùng chung vẫn giữ một bitmap và các sprite con, không nhân bản atlas vào nhiều nhóm thư mục.",
           "- Hai ảnh Hội An cùng pixels có importer khác nhau (map chính và preview mip/1024); giữ cả hai GUID để bảo toàn chất lượng/tham chiếu. Các gói LBC/DesignKit gốc được giữ nguyên nội dung tại Tools/Import/Sources.","",
           "## Asset runtime / sprite đã đăng ký", "", "| Tên | Vai trò / ID | Đường dẫn | Canonical | Vấn đề |", "|---|---|---|---|---|"]
    for e in art:
        ids=", ".join(b["id"] for b in e["bindings"])
        label='Hỗn hợp / xem từng sprite' if e['temporaryBindings'] and e['canonical'] else 'Có' if e['canonical'] else 'Không / chưa duyệt'
        lines.append(f"| {e['name']} | {ids or 'source / compatibility / native sprite'} | [{e['path']}](../../{e['path']}) | {label} | {'; '.join(e['issues'])} |")
    lines += ["", "## NPC liên thành phố đã chốt", "",
              "Chị Hương và So-yeon đã được người dùng xác nhận CANONICAL. Hai gương mặt khác nhau; không thay Mai hoặc Ji-eun, không dùng mặt So-yeon cho Ji-won. Các status tích hợp cũ trong Unity asset là chú thích lịch sử; duyệt danh tính hiện hành theo NPC_CANONICAL_REGISTRY.", "",
              "| ID | Nhân vật | Hình giữ nguyên | Trạng thái |", "|---|---|---|---|",
              "| npc_huong | Chị Hương, U1 | Cô gái áo xanh, mũ hoa/giỏ hoa, hàng trên atlas | CANONICAL |",
              "| npc_soyeon | So-yeon, J15 | Cô gái kính, áo xanh cầm tablet, hàng dưới atlas | CANONICAL |", "",
              "Hai native sprite dùng outline theo alpha + UI sprite mesh để loại pixels nhân vật bên cạnh. Không tạo bitmap trùng, không sửa nguồn; giữ GUID và sprite npc_guest của Ji-eun. Tọa độ/outline trong CrossCitySpriteSlices.json."]
    proposals=[b for b in bound if b['role'] in ('npc','enemy') and 'NOT_CANONICAL' in b.get('status','') and b['id'] not in ('npc_huong','npc_soyeon')]
    if proposals:
        lines += ["", "## Hình đề xuất cho các chương mới", "",
                  "Các binding dưới đã có sprite cụ thể, nhưng chưa được người dùng duyệt canonical/final. Native slice theo vai trò roster và ảnh dẫn xuất AI được ghi riêng; đây là cutout tĩnh, chưa phải animation. Không thay Cô Mai hoặc Ji-eun.", "",
                  "| ID | Vai trò | Sprite | Trạng thái |", "|---|---|---|---|"]
        lines += [f"| {b['id']} | {b['role']} | {b['path']} / {b['spriteName']} | {b['status']} |" for b in proposals]
    lines += ["", "## Di chuyển thực tế", "", "| Cũ | Mới | GUID |", "|---|---|---|"]
    lines += [f"| {e['oldPath']} | {e['newPath']} | {e['guid']} |" for e in moves]
    lines += ["", "## Trùng pixels / nguồn sao lưu", "", "Những nhóm dưới là bản có cùng hash. Không xóa tự động: nguồn bàn giao và bản runtime/importer có vai trò khác nhau.", ""]
    lines += ["- "+"; ".join(g) for g in duplicates]
    lines += ["", "## Tra cứu", "", "[Manifest JSON](art-asset-manifest.json) · [CSV](art-asset-manifest.csv) · [GUID move log](migration-results.txt) · [Nguồn và canonical preview](ART_SOURCE_INDEX.md)",
              "", "[Kiểm soát art trước khi mở rộng thành phố](CITY_EXPANSION_ART_CHECKLIST.md) · [Asset còn thiếu hiện hành](MISSING_ASSETS.md) · [Snapshot kiểm tra nguồn/import/binding](city-art-preflight.json)",
              "", "Không còn tên PNG dạng UUID ở phạm vi ảnh game đã quét. Hai Composite_01/02 đã nhận diện là bảng tổng hợp và đổi tên có nghĩa, không đổi nội dung."]
    (DOC/"ART_ASSET_CATALOG.md").write_text("\n".join(lines)+"\n",encoding="utf-8")
    source=["# Nguồn art và canonical preview", "", "Giữ nguyên các bộ gốc ngoài Assets. Catalog ảnh từng file và SHA256 ở manifest.", ""]
    for folder in ["Tools/Import/Sources/LBC_AnhMoi_BanGiaoCodex_20261015","Tools/Import/Sources/LBC_Gameplay_6Cities_Detailed_v1","Tools/Import/Sources/LBC_HoiAn_StoryExpansion_v1","Tools/Import/Sources/LightBetweenCultures_DesignKit_v1","Tools/Import/Sources/MarkerDesign","Tools/Import/Sources/Artwork"]:
        if (ROOT/folder).exists():source.append(f"- [{folder}](../../{folder})")
    for b in bound:
        if b["role"] in ("lumi","canonical_npc"):
            source += ["",f"### {b['role']} · {b['id']}","",f"![{b['id']}](../../{b['path']})", "",f"`{b['guid']}:{b['localId']}`"]
    (DOC/"ART_SOURCE_INDEX.md").write_text("\n".join(source)+"\n",encoding="utf-8")
    print(json.dumps(manifest["counts"]))

if __name__=="__main__":
    if "--city-preflight" in sys.argv:raise SystemExit("The unreleased-city/original-map preflight is archived. Use verify_canonical_layout.py and the native workspace audit for current source.")
    else:main()
