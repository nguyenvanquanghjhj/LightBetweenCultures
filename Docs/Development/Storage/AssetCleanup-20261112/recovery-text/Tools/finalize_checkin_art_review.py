"""Bounded check-in evidence/manifest review. No gameplay/art/save writes."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,re,html
from PIL import Image,ImageChops

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Gameplay/CheckInArt-20261018'
QA=ROOT/'Validation/CheckInArt-20261018'
BEFORE=Path('D:/GAMEKKK/Backups/BeforeCheckInArt-20261018')

def read(p):return json.loads(p.read_text('utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n','utf-8')

def main():
    inventory=read(OUT/'source-inventory.json')['entries'];native=read(OUT/'native-checkin-art.json')['entries'];previous=read(OUT/'native-before-derived.json')['entries']
    assert len(inventory)==len(native)==len(previous)==18
    guards=[]
    for rel in ['Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/Content/AdventurePresentation.json',
      'Assets/_Game/Scripts/Core/SaveSystem.cs','Assets/_Game/Scripts/Content/RelicCollection.cs','Assets/_Game/Scripts/Content/NodeEventManager.cs',
      'Docs/Art/NPC_CANONICAL_REGISTRY.md','Docs/Art/NPC_CANONICAL_REGISTRY.json','ProjectSettings/ProjectSettings.asset']:
        assert sha(ROOT/rel)==sha(BEFORE/rel),rel
        guards.append(dict(path=rel,sha256=sha(ROOT/rel),status='BYTE_IDENTICAL_TO_BEFORE'))
    route=read(Path('D:/GAMEKKK/Backups/CheckpointMapRoute35-20261018/CHECKPOINT_MANIFEST.json'))
    for e in route['entries']:
        if e['path'] in ['Assets/_Game/Scripts/Maps/CityMapPaths.cs','Assets/_Game/Scripts/Core/GameData.cs','Assets/_Game/Scripts/Content/ContentDefinitions.cs']:
            assert sha(ROOT/e['path'])==e['sha256'];guards.append(dict(path=e['path'],sha256=e['sha256'],status='ROUTE35_BYTE_IDENTICAL'))
    maps=read(ROOT/'Docs/Maps/RouteRedesign35-20261018/canonical-maps.json')
    for m in maps:assert sha(ROOT/m['runtime'])==m['runtimeSHA256']
    old=(BEFORE/'Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs').read_text('utf-8-sig');new=(ROOT/'Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs').read_text('utf-8-sig')
    assert old.split('void ShowRelicPhoto()',1)[1].split('public void ShowRelicCollection',1)[0]==new.split('void ShowRelicPhoto()',1)[1].split('public void ShowRelicCollection',1)[0]
    assert old.split('void SetPostcardUV',1)[1].split('void ShowStoredPostcard',1)[0]==new.split('void SetPostcardUV',1)[1].split('void ShowStoredPostcard',1)[0]
    old=(BEFORE/'Assets/_Game/Scripts/UI/GameplayPanels.cs').read_text('utf-8-sig');new=(ROOT/'Assets/_Game/Scripts/UI/GameplayPanels.cs').read_text('utf-8-sig')
    for start,end in [('void ShowEnding','public void ShowPuzzle'),('void DrawDialogue','void Execute')]:
        assert old.split(start,1)[1].split(end,1)[0]==new.split(start,1)[1].split(end,1)[0]
    bindings=read(ROOT/'Docs/Art/native-art-bindings.json')['entries'];prior=read(BEFORE/'Docs/Art/native-art-bindings.json')['entries']
    assert all(e in bindings for e in prior)
    assert len([e for e in bindings if e['role']=='checkin'])==18
    assert sha(ROOT/'Validation/FullStory-20261017/Fresh04/full-story-final.json')=='83a82674c50d8e2b3ab48981913af24dc9cd33d788b8ab5c0e2cc3d4f146555f'
    runs=[]
    for name in ['Journey02','Fresh01','Relay01','RelicFresh01',*[f'Restart-{c}' for c in ['hoi_an','hue','da_lat','jeonju','gyeongju','seoul']],'RestartFinal-hoi_an','RestartFinal-seoul','CrossAlbum01','RelicRestart01']:
        restart=name.startswith('Restart') or name in ['CrossAlbum01','RelicRestart01']
        file='restart-results.txt' if restart else 'smoke-results.txt';text=(QA/name/file).read_text('utf-8-sig')
        assert text.splitlines()[-1]=='ALL CHECKS PASSED',name
        runs.append(dict(run=name,assertions=sum(x.startswith('PASS:') for x in text.splitlines()),status='PASS',path=str((QA/name/file).relative_to(ROOT)).replace('\\','/')))
    captures=0
    for name in [r['run'] for r in runs]:
        manifest=QA/name/('restart-captures.txt' if name.startswith('Restart') or name in ['CrossAlbum01','RelicRestart01'] else 'capture-manifest.txt')
        for line in manifest.read_text('utf-8-sig').splitlines():
            m=re.search(r'\|\s*([^|]+\.png)\s*\| SHA256 ([A-Fa-f0-9]{64})',line)
            if m:assert sha(manifest.parent/m[1].strip())==m[2].lower();captures+=1
    failed=(QA/'Journey01/smoke-results.txt').read_text('utf-8-sig');assert failed.splitlines()[-1]=='FAILED'
    content=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json');definitions={c['id']:c for c in content['checkIns']}
    final_save=read(QA/'Journey02/checkin-after-seoul.json');assert all(c['completionFlag'] in final_save['flags'] for c in definitions.values())
    source_save=read(ROOT/'Validation/FullStory-20261017/Fresh04/full-story-final.json')
    assert final_save['coins']-source_save['coins']==28
    mapping=[]
    for e,n,b in zip(inventory,native,previous):
        assert e['checkInId']==n['checkInId']==b['checkInId']
        assert n['guid']==b['guid'] and n['localId']==b['localId']
        assert sha(ROOT/e['source'])==e['sourceSHA256']
        assert sha(ROOT/e['assetPath'])==e['derivedSHA256']
        src=Image.open(ROOT/e['source']).convert('RGB');dst=Image.open(ROOT/e['assetPath']).convert('RGB');footer=e['sceneryPixels'][3]
        assert ImageChops.difference(src.crop((e['cropLeft'],footer,src.width-e['cropRight'],src.height)),dst.crop((0,footer-e['cropTop'],dst.width,dst.height))).getbbox() is None
        assert f"guid: {n['guid']}" in (ROOT/(e['assetPath']+'.meta')).read_text('utf-8-sig')
        runtime='RestartFinal-'+e['cityId'] if e['cityId'] in ['hoi_an','seoul'] else 'Restart-'+e['cityId']
        assert (QA/runtime/('Postcard-'+e['nodeId']+'.png')).exists()
        mapping.append(dict(**e,guid=n['guid'],localId=n['localId'],spriteName=Path(e['assetPath']).stem,
          bindingStatus='RUNTIME_ART_BOUND',runtimeStatus='RUNTIME_ART_TESTED; WINDOWS_RUNTIME_PASS',saveStatus='WINDOWS_PROCESS_RESTART_PASS',
          visualApproval='NEEDS_VISUAL_REVIEW; SOURCE_IMAGE_APPROVED_BY_USER; NOT_FINAL_ART_APPROVED',runtimeEvidence=f'Validation/CheckInArt-20261018/{runtime}/Postcard-{e["nodeId"]}.png'))
    dump(OUT/'verified-mapping.json',dict(entries=mapping))
    dump(OUT/'verification.json',dict(verifiedUTC=datetime.now(timezone.utc).isoformat(),status='WINDOWS_TECHNICAL_PASS_VISUAL_REVIEW_PENDING',
      imported=18,bound=18,derived=18,sourcePNGsUnchanged=True,GUIDsAndLocalIDsPreserved=18,approvedSourceOnly=True,noUpscaling=True,
      protectedGuards=guards,oldNativeReferencesPreserved=len(prior),h6GestureBodyUnchanged=True,endingAndNpcDialogueBodiesUnchanged=True,
      runs=runs,assertions=sum(r['assertions'] for r in runs),verifiedRuntimeCaptures=captures,
      oldMarks=7,newExplicitMarks=11,coinsDelta=28,questRelicSaveSchemaUnchanged=True,derivedFootersUnmodified=True,crossCityAlbumWhileRemainingInSeoul='WINDOWS_RUNTIME_PASS',
      failureRetained='Journey01: QA clicked off-viewport last card, hit Close. Fixed QA scrolling; Journey02 PASS. No production change for this failure.',
      limitations=['ANDROID_NOT_TESTED','LIMITED_SOURCE_RESOLUTION','No physical touch/pinch device or FPS/VRAM measurement',
        'No full133-node regression; current task targeted18 sites,6 restarts, fresh HoiAn and relayC']))

    headers=['# Check-in landmark image mapping ·18/10/2026','\n18 PNG nguồn được giữ nguyên;18 bản runtime derived dùng đúng bộ được chọn. Không resize/upscale, không AI. '
      'Crop theo từng ảnh, Unsharp Mask0.55/25/3 chỉ vùng phong cảnh; chân thẻ được giữ trong PNG và loại bằng UV ở UI. '
      'H6 dùng ảnh mới cho gallery theo bổ sung mới nhất; ảnh hai biểu tượng/hotspot ở bài căn khớp không đổi.',
      '\n| CityId | NodeId | CheckInId | ImagePath | ExistingState | BindingStatus | RuntimeStatus | SaveStatus | VisualApproval |',
      '|---|---|---|---|---|---|---|---|---|']
    for e in mapping:headers.append(f"| {e['cityId']} | {e['nodeId']} | {e['checkInId']} | {e['assetPath']} | ALREADY_PLAYABLE_HANDLER; chưa có album chung | BOUND | WINDOWS_RUNTIME_PASS | PROCESS_RESTART_PASS | NEEDS_VISUAL_REVIEW |")
    headers+=['\n## Crop, nguồn và native reference','\n| Filename gốc | L/R/T px | Trước → sau | GUID / localID |', '|---|---|---|---|']
    for e in mapping:headers.append(f"| {Path(e['source']).name} | {e['cropLeft']}/{e['cropRight']}/{e['cropTop']} | {e['width']}×{e['height']} → {e['derivedWidth']}×{e['derivedHeight']} | {e['guid']} / {e['localId']} |")
    headers+=['\n[Mapping JSON đầy đủ, source hashes và ảnh runtime](../Gameplay/CheckInArt-20261018/verified-mapping.json) · '
      '[18 cặp native/thumbnail trước–sau](../../Validation/CheckInArt-20261018/DerivedReview/index.html) · '
      '[Gallery runtime](../Gameplay/CheckInArt-20261018/RuntimeReview.html).',
      '\nLIMITED_SOURCE_RESOLUTION: nguồn450px ngang đã upscale/sharpen từ collage theo README của gói; không có chi tiết high-resolution mới. '
      'UI fit aspect, ảnh lớn thực tế khoảng360–400px ngang trên màn540px, không kéo lên fullscreen ngang. '
      'Không tự ghi FINAL_ART_APPROVED; cần người dùng duyệt crop, sắc nét và bố cục Unity.']
    (ROOT/'Docs/Art/CHECKIN_LANDMARK_IMAGE_MAPPING.md').write_text('\n'.join(headers)+'\n','utf-8')

    prefix='../../../Validation/CheckInArt-20261018/'
    gallery=['<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>18 check-in runtime</title><style>body{font:16px/1.5 system-ui;background:#17383c;color:#f5e6c6;max-width:1120px;margin:20px auto;padding:16px}a{color:#8de2ce}img{max-width:100%;height:auto}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:16px}.grid img{max-height:720px;width:auto}details{margin:16px 0}</style>',
      '<h1>18 postcard — runtime và crop review</h1><p>WINDOWS_RUNTIME_PASS · NEEDS_VISUAL_REVIEW · ANDROID_NOT_TESTED. Nguồn giữ nguyên, không upscale. Ảnh Map A chỉ còn ở bài căn khớp H6 đã PASS; cả18 gallery dùng nguồn được chọn.</p>',
      '<p><a href="../../../Validation/CheckInArt-20261018/DerivedReview/index.html">18 cặp native/thumbnail trước–sau</a> · <a href="../../Art/CHECKIN_LANDMARK_IMAGE_MAPPING.md">Bảng crop/GUID</a> · <a href="verification.json">QA chi tiết</a></p><div class="grid">']
    for e in mapping:
        url='../../../'+e['runtimeEvidence'];gallery.append(f'<section><h2>{html.escape(e["title"])} · {e["nodeId"]}</h2><a href="{url}"><img loading="lazy" src="{url}"></a></section>')
    gallery.append('</div><h2>Thumbnail trong Photo Journal</h2><div class="grid">')
    for city in ['hoi_an','hue','da_lat','jeonju','gyeongju','seoul']:
        run='RestartFinal-'+city if city in ['hoi_an','seoul'] else 'Restart-'+city
        gallery.append(f'<section><h3>{city}</h3><img loading="lazy" src="{prefix}{run}/Restart-album-{city}.png"></section>')
    gallery.append('</div><h2>Runtime thumbnail trước/sau chỉnh mép</h2><p>Cùng540×960, cùng12 postcard ở save Hội An; nền map phía sau có thể khác focus sau re-entry. Trái: ảnh chưa sửa mép, phải: derived cuối. Không phải ảnh mockup.</p><div class="grid">')
    for url in ['Journey02/Journal-hoi_an.png','RestartFinal-hoi_an/Restart-album-hoi_an.png']:gallery.append(f'<img loading="lazy" src="{prefix}{url}">')
    gallery.append('</div><h2>H6 — tương tác gốc được bảo vệ</h2><p>Ảnh dưới là bài căn hai biểu tượng từ fresh journey, không phải artwork gallery thay thế.</p>')
    gallery.append(f'<img loading="lazy" src="{prefix}RelicFresh01/Relic-H6-postcard-aligned.png">')
    gallery.append('<h2>NPC và relay cùng node</h2><p>NPC frames Journey02/build02 trước bước xử lý mép, NPC portrait/dialogue source/binding giữ nguyên ở build04. Relay01/build04 là check-in trước rồi giải puzzle thật; không fixture flag.</p><div class="grid">')
    for url in ['Journey02/NPC-and-checkin-G14.png','Journey02/NPC-and-checkin-S2.png','Relay01/Scenery-S18.png','Relay01/S18-relay-after-real-puzzle.png']:
        if (QA/url).exists():gallery.append(f'<img loading="lazy" src="{prefix}{url}">')
    gallery.append('</div><h2>Fresh save: không lộ cảnh khóa</h2><div class="grid">')
    for city in ['hoi_an','seoul']:gallery.append(f'<img loading="lazy" src="{prefix}Fresh01/Fresh-journal-{city}.png">')
    gallery.append('</div><h2>Ở Seoul, xem lại cả sáu thành phố</h2><p>CrossAlbum01: click18 postcard từ save đã kiếm được, city/node Lumi giữ nguyên.</p><div class="grid">')
    for city in ['hoi_an','seoul']:gallery.append(f'<img loading="lazy" src="{prefix}CrossAlbum01/Cross-city-album-{city}-while-at-seoul.png">')
    gallery.append('</div></html>');(OUT/'RuntimeReview.html').write_text('\n'.join(gallery),'utf-8')
    print(json.dumps(dict(mapped=len(mapping),assertions=sum(r['assertions'] for r in runs),captures=captures,guards='PASS'),ensure_ascii=False))

if __name__=='__main__':main()
