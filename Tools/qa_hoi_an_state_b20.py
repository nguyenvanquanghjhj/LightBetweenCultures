"""Bounded StateB20 reporting/checkpoint. Does not edit gameplay or source art."""
import hashlib
import json
import re
import shutil
from datetime import datetime, timezone
from pathlib import Path
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/Gameplay/HoiAnLightingB20-20261018'
QA=ROOT/'Validation/HoiAnLightingB20-20261018'
BEFORE=ROOT.parent/'Backups/BeforeHoiAnStateB20-20261018'

def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,s):p.write_text(s.rstrip()+'\n',encoding='utf-8')

def main():
    baseline=load(DOC/'baseline.json');rel='Assets/_Game/Resources/Content/HoiAnLighting.json'
    old=load(BEFORE/rel);now=load(ROOT/rel)
    assert [k for k in old if old[k]!=now[k]]==['districtBoundaryPixels']
    protected=[]
    for e in baseline['protected']:
        if e['path']==rel:continue
        assert sha(ROOT/e['path'])==e['sha256'],e['path']
        protected.append(e['path'])
    plan=load(DOC/'geometry-plan.json');v=np.array([[p['x'],p['y']] for p in now['districtBoundaryPixels']]);c=np.array(plan['centerMapPixelsTopLeft'])
    assert np.max(np.abs(np.linalg.norm(v-c,axis=1)-plan['radiusPixels']))<.002
    assert np.linalg.norm(v.mean(axis=0)-c)<.002
    runs=[]
    for name in ['before','after03','restart-B','after-H7','check-A','check-C']:
        p=QA/name/'restart-results.txt';s=p.read_text(encoding='utf-8-sig').splitlines()
        assert s[-1]=='ALL CHECKS PASSED',name
        runs.append(dict(run=name,checks=sum(t.startswith('PASS:') for t in s),result='PASS'))
    assert 'RELIC WINDOWS BUILD: Succeeded' in (ROOT/'Logs/hoi-an-state-b20-build03.log').read_text(encoding='utf-8-sig')
    a=load(QA/'before/input-provenance.json');b=load(QA/'after03/input-provenance.json');assert a['sha256'].lower()==b['sha256'].lower()
    measurements=[]
    for name in ['before','after03','restart-B','after-H7','check-A','check-C']:
        line=(QA/name/'lighting-measurements.txt').read_text().splitlines()[0]
        assert '1280x960' in line and 'zoom=1 ' in line and 'mapMin=(94.72, 78.72) mapMax=(1185.28, 896.64)' in line
        measurements.append(dict(run=name,line=line))
    images=[np.asarray(Image.open(QA/n/'restart-B.png').convert('RGB'),dtype=float) for n in ['before','after03']]
    assert all(im.shape==(960,1280,3) for im in images)
    hud=np.concatenate([(images[1][:60]-images[0][:60]).reshape(-1,3),(images[1][895:950]-images[0][895:950]).reshape(-1,3)])
    pixel=dict(HUD_exact_pixel_fraction=float(np.mean(np.max(np.abs(hud),axis=1)==0)),HUD_mean_absolute_difference=float(np.abs(hud).mean()))
    assert pixel['HUD_exact_pixel_fraction']>.99,pixel
    # Build-export noise only: native bindings must remain semantically identical.
    for name in ['native-art-bindings.json','art-asset-manifest.json']:
        x=load(BEFORE/'Docs/Art'/name);y=load(ROOT/'Docs/Art'/name)
        if name=='art-asset-manifest.json':
            x.pop('generated_utc',None);y.pop('generated_utc',None)
            preview='Docs/Gameplay/HoiAnLighting-20261018/ABC-runtime-comparison.png'
            added=[e for e in y['images'] if e['path']==preview and not any(a['path']==preview for a in x['images'])]
            if added:y['images']=[e for e in y['images'] if e not in added];y['counts']['all_images']-=len(added)
        assert x==y,'Unexpected native art delta: '+name
    for name in ['native-art-bindings.json','art-asset-manifest.json','art-asset-manifest.csv','ART_ASSET_CATALOG.md']:
        (ROOT/'Docs/Art'/name).write_bytes((BEFORE/'Docs/Art'/name).read_bytes())
    settings=ROOT/'ProjectSettings/ProjectSettings.asset';prior=BEFORE/'ProjectSettings/ProjectSettings.asset'
    pattern=r'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
    assert re.sub(pattern,'',settings.read_text(encoding='utf-8-sig'))==re.sub(pattern,'',prior.read_text(encoding='utf-8-sig'))
    settings.write_bytes(prior.read_bytes())
    write(DOC/'results.json',json.dumps(dict(runs=runs,protectedExact=protected,onlyRuntimeFieldChanged=rel+':districtBoundaryPixels',geometry=plan,pixels=pixel,measurements=measurements),indent=2))
    page='''<!doctype html><html lang="vi"><meta charset="utf-8"><title>Hội An State B20</title>
<style>body{font:16px/1.5 system-ui;background:#102c32;color:#ffe8bc;margin:24px}a{color:#8fe1ce}.grid{display:grid;grid-template-columns:repeat(2,minmax(0,1fr));gap:18px}img{width:100%;height:auto}figure{margin:0}code{overflow-wrap:anywhere}</style>
<h1>Hội An — State B trước / sau</h1><p>Windows Player mới, cùng bản sao save đã trả Cô Mai, cùng1280×960, zoom1, focus(0.5,0.5). Click ảnh mở PNG gốc. Không chỉnh màu hoặc mockup. StateA/C giữ nguyên.</p><div class="grid">'''
    for folder,label in [('before','Trước — polygon cũ,46,67% gồm gradient'),('after03','Sau — vòng tròn quanh đèn H2,20,0% gồm gradient')]:
        src=f'../../../Validation/HoiAnLightingB20-20261018/{folder}/restart-B.png'
        page+=f'<figure><figcaption>{label}</figcaption><a href="{src}"><img src="{src}"></a></figure>'
    page+='</div><h2>Save/restart và StateA/C</h2><div class="grid">'
    for folder,state in [('restart-B','B'),('after-H7','B'),('check-A','A'),('check-C','C')]:
        src=f'../../../Validation/HoiAnLightingB20-20261018/{folder}/restart-{state}.png'
        page+=f'<figure><figcaption>{folder} — {state}</figcaption><a href="{src}"><img loading="lazy" src="{src}"></a></figure>'
    page+='</div><p><a href="../../QA/HOIAN_STATE_B20.md">Báo cáo và giới hạn</a>. Android NOT_TESTED. DỪNG sau bản sửa này.</p></html>'
    write(DOC/'RuntimeReview.html',page)
    rows='\n'.join(f"| {r['run']} | {r['checks']} | PASS |" for r in runs)
    report=f'''# Hội An — chỉnh nhỏ State B20

**WINDOWS TARGETED PASS · VISUAL_REVIEW_PENDING · DỪNG.**18/10/2026.

[Ảnh runtime trước/sau cùng camera](../Gameplay/HoiAnLightingB20-20261018/RuntimeReview.html). Build03 Windows Succeeded; dùng bản sao QA, không ghi save người chơi.

Chỉ thay `districtBoundaryPixels` trong `Assets/_Game/Resources/Content/HoiAnLighting.json`: polygon khu phố cũ thành vòng tròn128đỉnh, dùng chính raster/mask cũ. Không sửa code ánh sáng hoặc tạo manager/shader/texture asset mới.

| Thông số | Giá trị |
|---|---|
| Tâm | Ngọn đèn cũ QuestLight_H2: normalized(0.383287,0.608002), map pixel top-left(555.000,425.710); không dùng arrival anchor |
| Bán kính | {plan['radiusPixels']:.3f}px trên ảnh1448×1086; đường kính632.283px |
| Diện tích contour tròn | {plan['circleContourPercent']:.3f}% map |
| Diện tích sáng tính trọng số gradient | **20.000%** runtime, trước46.666% |
| Lõi có mức phục hồi≥95% |17.112% map; phần mép mềm làm vùng sáng không phải đĩa cứng |
| Gradient | Giữ±32px, tổng64px; Bilinear mask256×192 như cũ |
| Bên trong / bên ngoài | Gần canonical / giữ target60% độ sáng (ngoài tối khoảng40%, cùng alpha cũ) |
| Chuyển tiếp A→B / B→C |Giữ1.25s /1.8s; StateC settled vẫn tắt dim hoàn toàn |

**StateA/C:** nguyên code chọn state, dimOpacity, mask allocation, timing và halos. Quest mai-lantern/HQ01/relic/world unlock/save/node/routes/H6 không đổi. {len(protected)} file bảo vệ exactSHA, gồm toàn bộCityMap.RelicLight.cs và các nguồn quest/relic/save/route/H6/18ảnh+meta. Chỉ geometry B thay; QA Lighting.cs cập nhật kỳ vọng diện tích và kiểm tròn8hướng.

| Run / scope | PASS assertions (có UI/capture checks) | Result |
|---|---:|---|
{rows}

PASS: restartB thật bằng save kết quả lượt trước; H7 completed nhưng relic chưa restored vẫnB20; StateA còn tối, StateC canonical/10halos và ChùaCầu occlusion; không replay celebration/reward. Zoom/pan/pinchPC, portrait540×960, inventory/journal click và Lumi đi waypoint thật. Mask là conmap, không raycast; nodeHIDDEN giữ nguyên. HUD trước/sau exactpixel fraction={pixel['HUD_exact_pixel_fraction']:.6f}.

Before/after dùng cùng seedSHA, cùng1280×960/focus(0.5,0.5)/zoom1/mapbounds. Lần chạy `after`/`after02` FAIL ở phép đo mép: CPU GetPixelBilinear lấy mẫu lệch nửa texel so với raster pixel-center, đã ghi8giá trị và sửa riêngQA bù(.5/256,.5/192); `after03` và các lượt tiếp theo PASS. Không sửa geometry/render để làm test pass; giữ log thất bại.

File sửa/thêm: HoiAnLighting.json (runtime chỉ1field), GameplaySmokeRunner.Lighting.cs (QA), Tools/qa_hoi_an_state_b20.py/.ps1, Docs/Gameplay/HoiAnLightingB20-20261018/* và báo cáo này. Native export đã merge theoAGENTS; chỉ noise build được đối chiếu và trả về exactbaseline, ProjectSettings giữ nguyên. Không sửa ảnh canonical, NPC hoặc check-in.

Không chạy full-story/133node regression vì logic gameplay không đổi. Android/device/FPS **NOT_TESTED**. Visual cuối chờ chủ dự án nghiệm thu bằng ảnh. Checkpoint nhỏ `CheckpointHoiAnStateB20-20261018` cần base `CheckpointPhase4Final-20261018` và chuỗi base. Không fullbackup/build/cache/historycopy. **DỪNG**, không chuyển task khác.
'''
    report_path=ROOT/'Docs/QA/HOIAN_STATE_B20.md';write(report_path,report)
    master=ROOT/'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md'
    prefix='> **StateB20 —18/10/2026: WINDOWS_TARGETED_PASS · VISUAL_PENDING · STOP.** Chỉ hình học vùng sángB HộiAn: vòngtròn20% cógradient quanhđènH2; A/C/H6/quest/relic/save/routes giữnguyên. [Ảnh trước/sau](HoiAnLightingB20-20261018/RuntimeReview.html) · [QA](../QA/HOIAN_STATE_B20.md). CheckpointHoiAnStateB20-20261018 cầnCheckpointPhase4Final-20261018. Không chuyển task khác.\n\n'
    text=master.read_text(encoding='utf-8-sig')
    if not text.startswith('> **StateB20'):write(master,prefix+text)
    base=Path(baseline['baseCheckpoint']);dest=ROOT.parent/'Backups/CheckpointHoiAnStateB20-20261018';assert not dest.exists()
    files=[ROOT/rel,ROOT/'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Lighting.cs',ROOT/'Tools/qa_hoi_an_state_b20.py',ROOT/'Tools/qa_hoi_an_state_b20.ps1',master,report_path]
    files+=[p for p in DOC.iterdir() if p.is_file() and p.name!='checkpoint.json']
    files+=[p for p in QA.rglob('*') if p.is_file()]
    files+=list((ROOT/'Logs').glob('hoi-an-state-b20-build*.log'))
    files=sorted(set(files));size=sum(p.stat().st_size for p in files);print('Checkpoint estimate:',len(files),'files',round(size/1024**2,2),'MiB');assert size<80*1024**2
    entries=[]
    for p in files:
        rel=p.relative_to(ROOT);q=dest/rel;q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q);assert sha(p)==sha(q)
        entries.append(dict(path=rel.as_posix(),sha256=sha(p),bytes=p.stat().st_size))
    manifest=dest/'CHECKPOINT_MANIFEST.json'
    write(manifest,json.dumps(dict(type='INCREMENTAL_REQUIRES_BASE',baseCheckpoint=str(base),baseManifestSHA256=sha(base/'CHECKPOINT_MANIFEST.json'),createdUTC=datetime.now(timezone.utc).isoformat(),summary='StateB circular20% config-only adjustment; A/C/gameplay/H6 unchanged; Windows targeted PASS',entries=entries,deletedPaths=[],totalPayloadBytes=size),indent=2))
    receipt=dict(checkpoint=str(dest),requiresBase=str(base),manifestSHA256=sha(manifest),files=len(entries),payloadBytes=size,allCopiedHashesVerified=True)
    write(DOC/'checkpoint.json',json.dumps(receipt,indent=2));print(json.dumps(receipt,indent=2))

if __name__=='__main__':main()
