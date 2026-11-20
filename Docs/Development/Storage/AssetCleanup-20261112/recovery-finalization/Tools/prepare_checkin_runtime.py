"""User-authorized deterministic edge cleanup; reads only the18 manifest PNGs.
Never overwrites source art. Pixel profiles were visually reviewed individually.
No resize or AI generation. Sharpen only scenery; retain the printed source footer.
"""
from pathlib import Path
import csv, hashlib, json
from PIL import Image, ImageDraw, ImageFilter

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Gameplay/CheckInArt-20261018'
QA=ROOT/'Validation/CheckInArt-20261018/DerivedReview'
# Left/right/top pixels. White separator/antialias pixels are included only where visible.
PROFILES={'H2':(0,0,0),'H3':(30,8,0),'H4':(0,4,0),'H6':(0,0,0),
 'H7':(9,0,0),'H17':(21,0,0),'U4':(0,6,16),'U6':(0,0,16),
 'D0':(10,0,16),'D14':(8,0,15),'J8':(4,0,16),'J11':(5,0,16),
 'G4':(0,0,7),'G14':(11,0,6),'G17':(10,8,6),'S2':(0,2,6),'S10':(1,3,6),'S18':(0,0,6)}

def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    QA.mkdir(parents=True,exist_ok=True);(QA/'Runtime').mkdir(exist_ok=True)
    inventory=json.loads((OUT/'source-inventory.json').read_text('utf-8-sig'))
    rows=list(csv.DictReader((ROOT/'_CheckIn_Import_20261018/manifest.csv').open(encoding='utf-8-sig')))
    assert len(rows)==18
    table=['#18 ảnh check-in — crop và sharp nhẹ',
      '\nNguồn duy nhất: `_CheckIn_Import_20261018/images`, giữ nguyên PNG và chân thẻ gốc. '
      'Không resize/upscale, không AI. Unsharp Mask radius0.55 / percent25 / threshold3 chỉ trên vùng phong cảnh; '
      'giữ nguyên pixel phần chữ sau crop. Crop top chỉ loại dải trắng, không cắt cảnh. '
      'Crop vùng chữ ở UI bằng UV, không xóa chân bưu thiếp khỏi PNG derived. '
      'H6 chỉ thay art gallery; ảnh căn khớp Map A/hotspot/biểu tượng giữ nguyên.',
      '\n| Filename gốc | Node | Crop L/R/T px | Trước → sau | Runtime path | Binding |',
      '|---|---|---|---|---|---|']
    comparisons=[]
    for row,e in zip(rows,inventory['entries']):
        assert row['filename']==Path(e['source']).name
        source=ROOT/e['source'];assert sha(source)==e['sourceSHA256']
        image=Image.open(source).convert('RGB');w,h=image.size;left,right,top=PROFILES[e['nodeId']]
        derived=image.crop((left,top,w-right,h));scene_height=e['sceneryPixels'][3]-top
        scene=derived.crop((0,0,derived.width,scene_height))
        derived.paste(scene.filter(ImageFilter.UnsharpMask(radius=.55,percent=25,threshold=3)),(0,0))
        prepared=QA/'Runtime'/row['filename'];derived.save(prepared)
        previous=e['assetPath'];e['assetPath']=previous.replace('20261018_ApprovedSet','20261018_RuntimeDerived')
        e.update(previousAssetPath=previous,preparedPath=str(prepared.relative_to(ROOT)).replace('\\','/'),
          cropLeft=left,cropRight=right,cropTop=top,derivedWidth=derived.width,derivedHeight=derived.height,
          derivedSHA256=sha(prepared),filter='UnsharpMask(radius=.55,percent=25,threshold=3); scenery only; NO_RESIZE',
          uv=[0,(derived.height-scene_height)/derived.height,1,scene_height/derived.height],
          visualStatus='RUNTIME_VARIANT_PENDING_VISUAL_REVIEW',resolutionStatus='LIMITED_SOURCE_RESOLUTION; NO_UPSCALE; no added detail')
        # Original and after shown at native pixels; thumbnails use Lanczos downsampling only.
        canvas=Image.new('RGB',(940,1000),'#17383c');draw=ImageDraw.Draw(canvas)
        draw.text((10,5),e['nodeId']+' BEFORE / AFTER - native pixels; footer preserved',fill='white')
        canvas.paste(image,(10,30));canvas.paste(derived,(480,30))
        for picture,x in [(image,10),(derived,480)]:
            thumb=picture.copy();thumb.thumbnail((135,215),Image.Resampling.LANCZOS);canvas.paste(thumb,(x,785))
        name=e['nodeId']+'-before-after.png';canvas.save(QA/name);comparisons.append(name)
        table.append(f"| {row['filename']} | {e['nodeId']} | {left}/{right}/{top} | {w}×{h} → {derived.width}×{derived.height} | {e['assetPath']} | PREPARED; native binding pending |")
        assert sha(source)==e['sourceSHA256']
    (OUT/'source-inventory.json').write_text(json.dumps(inventory,ensure_ascii=False,indent=2)+'\n','utf-8')
    (OUT/'runtime-crop-table.md').write_text('\n'.join(table)+'\n','utf-8')
    (QA/'index.html').write_text('<!doctype html><meta charset="utf-8"><title>18 postcard crops</title><style>body{background:#17383c;color:#f9ebcc;font:16px system-ui;max-width:960px;margin:auto}img{max-width:100%;height:auto}</style><h1>18 ảnh: native trước/sau và thumbnail</h1><p>Không upscale. PNG nguồn giữ nguyên; đây là ảnh đối chiếu kỹ thuật, không phải runtime.</p>'+''.join(f'<h2>{n.split("-")[0]}</h2><img loading="lazy" src="{n}">' for n in comparisons),'utf-8')
    print('18 derived prepared;18 source hashes preserved; no upscale; UV excludes footer only in UI')
if __name__=='__main__':main()
