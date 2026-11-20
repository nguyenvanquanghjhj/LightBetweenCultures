"""Pixel-preserving white-paper removal previews; no AI, no source PNG edits.
Only four existing concept-sheet sprites with opaque cream background are selected.
Runtime binding is a separate explicit Unity step after Windows before captures.
"""
from pathlib import Path
from collections import deque
import hashlib,json,math,re
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QA/Phase7';OUT=ROOT/'Validation/Phase7-20261019/bot-derived-preview';OUT.mkdir(parents=True,exist_ok=True)
rows=json.loads((DOC/'bot-sprite-inventory.json').read_text(encoding='utf-8'))
selected={}
for r in rows:
 if r['path']=='Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png':selected.setdefault(r['spriteName'],r)
assert set(selected)=={'shadow_cat','stone_guard','energy_core','shadow_courier'}
manifest=[];tiles=[]
for name,row in selected.items():
 source=ROOT/row['path'];sheet=Image.open(source).convert('RGBA');q=row['rect'];x,y,w,h=[int(q[k]) for k in ['x','y','width','height']]
 crop=sheet.crop((x,sheet.height-y-h,x+w,sheet.height-y))
 # Preserve the already approved Sprite mesh silhouette. This excludes the
 # neighboring concept artwork that lies inside the rectangle but outside its mesh.
 metadata=(ROOT/row['meta']).read_text(encoding='utf-8-sig');part=next(p for p in re.split(r'(?=^    - serializedVersion:)',metadata,flags=re.M) if re.search(r'^      name: '+re.escape(name)+r'$',p,re.M))
 outline=re.search(r'^      outline:\n(.*?)(?=^      physicsShape:)',part,re.M|re.S)[1]
 silhouette=Image.new('L',(w,h));draw=ImageDraw.Draw(silhouette)
 for polygon in re.split(r'(?=^      - -)',outline,flags=re.M):
  points=[(float(a)+w/2,h/2-float(b)) for a,b in re.findall(r'\{x: ([-.\d]+), y: ([-.\d]+)\}',polygon)]
  if len(points)>=3:draw.polygon(points,fill=255)
 crop.putalpha(silhouette)
 pixels=list(crop.getdata());mask=bytearray(w*h);queue=deque()
 def paper(i):
  r,g,b,a=pixels[i];return a==0 or min(r,g,b)>=220 and max(r,g,b)-min(r,g,b)<=40
 def add(i):
  if not mask[i] and paper(i):mask[i]=1;queue.append(i)
 for i in range(w):add(i);add((h-1)*w+i)
 for i in range(h):add(i*w);add(i*w+w-1)
 while queue:
  i=queue.popleft();px=i%w;py=i//w
  if px:add(i-1)
  if px<w-1:add(i+1)
  if py:add(i-w)
  if py<h-1:add(i+w)
 result=pixels.copy();edge=0
 for i,removed in enumerate(mask):
  if removed:result[i]=(0,0,0,0);continue
  color=pixels[i];px=i%w;py=i//w
  if min(color[:3])<170 or max(color[:3])-min(color[:3])>45:continue
  ns=[j for j in [i-1 if px else i,i+1 if px<w-1 else i,i-w if py else i,i+w if py<h-1 else i] if mask[j]]
  if not ns:continue
  # De-matte only a one-pixel neutral edge beside verified exterior paper.
  # The inner face/body/highlights remain the original RGB/alpha pixels.
  candidates=[]
  for dy in range(-3,4):
   for dx in range(-3,4):
    a,b=px+dx,py+dy
    if 0<=a<w and 0<=b<h:
     j=b*w+a;v=pixels[j]
     if not mask[j] and (min(v[:3])<150 or max(v[:3])-min(v[:3])>65):candidates.append((dx*dx+dy*dy,v))
  if not candidates:continue
  fore=min(candidates,key=lambda v:v[0])[1];back=pixels[ns[0]]
  numerator=sum((color[k]-back[k])*(fore[k]-back[k]) for k in range(3));denominator=sum((fore[k]-back[k])**2 for k in range(3))
  alpha=max(0,min(1,numerator/max(1,denominator)));result[i]=(*fore[:3],round(color[3]*alpha));edge+=1
 out=Image.new('RGBA',(w,h));out.putdata(result);filename=name+'_Clean.png';out.save(OUT/filename)
 manifest.append(dict(spriteName=name,source=row['path'],sourceSHA256=hashlib.sha256(source.read_bytes()).hexdigest(),sourceRectUnity=q,preview=(OUT/filename).relative_to(ROOT).as_posix(),destination='Assets/_Game/Art/Characters/Enemies/CleanDerived/'+filename,pixels=[w,h],backgroundPixels=sum(mask),deMattedEdgePixels=edge,method='Border-connected near-cream paper alpha removal; one-pixel neutral edge de-matte; no resize/no AI',affected=[dict(city=r['city'],node=r['node'],artId=r['artId']) for r in rows if r.get('spriteName')==name]))
 tile=Image.new('RGB',(600,350),(24,38,43));d=ImageDraw.Draw(tile);d.text((10,8),name+' - SOURCE DIAGNOSTIC, not runtime proof',fill='white')
 for at,im in [(0,crop),(300,out)]:
  im=im.copy();im.thumbnail((280,290),Image.Resampling.LANCZOS);tile.paste(im,(at+(300-im.width)//2,38),im)
 d.text((12,328),'BEFORE',fill='white');d.text((312,328),'DERIVED PREVIEW',fill='white');tiles.append(tile)
contact=Image.new('RGB',(1200,700))
for i,t in enumerate(tiles):contact.paste(t,((i%2)*600,(i//2)*350))
contact.save(OUT/'source-diagnostic.png')
plan_path=DOC/'bot-alpha-plan.json'
# Keep curated pre-repair identity annotations if previews are regenerated.
# Alpha cleanup is never a new canonical-art approval.
prior=json.loads(plan_path.read_text(encoding='utf-8-sig')) if plan_path.exists() else []
statuses={a['artId']:a['oldStatus'] for e in prior for a in e['affected'] if 'oldStatus' in a}
for e in manifest:
 for a in e['affected']:
  if a['artId'] not in statuses:raise ValueError('Missing reviewed original art status: '+a['artId'])
  a['oldStatus']=statuses[a['artId']]
plan_path.write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
print('Prepared four source-preserving previews:',[(r['spriteName'],r['backgroundPixels'],r['deMattedEdgePixels']) for r in manifest])
