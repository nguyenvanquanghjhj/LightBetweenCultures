from pathlib import Path
from PIL import Image,ImageDraw
import json,re
R=Path(__file__).resolve().parents[2]; O=Path(__file__).parent
load=lambda p:json.loads((R/p).read_text(encoding='utf-8-sig'))
content=load('Assets/_Game/Resources/Content/GameplayContent.json')
bindings=load('Docs/Art/native-art-bindings.json')['entries']
t=(R/'Assets/_Game/Resources/AdventureUIArt.asset').read_text()
blocks=re.findall(r'  - cityId: (\w+)\n    nodeId: (\w+)\n    artId: (npc_\w+)\n(.*?)(?=  - cityId:|  mapActorScale:)',t,re.S)
def pair(s,key):
 m=re.search(key+r': \{x: ([-\d.e]+), y: ([-\d.e]+)\}',s);return tuple(map(float,m.groups()))
records=[]
for city in ['hoi_an','hue','da_lat','jeonju','gyeongju','seoul']:
 c=next(c for c in content['cities'] if c['id']==city)
 bg=next(b for b in bindings if b['id']==('hoiAnMap' if city=='hoi_an' else city) and b['role']==('designkit_serialized_reference' if city=='hoi_an' else 'city_preview'))
 im=Image.open(R/bg['path']).convert('RGB'); W,H=im.size
 tiles=[]
 for ci,node,npc,s in blocks:
  if ci!=city:continue
  n=next(n for n in c['nodes'] if n['id']==node)
  p=n.get('arrivalPosition') if n.get('hasArrivalPosition') else n['position']
  if city=='hoi_an' and not n.get('hasArrivalPosition'):
   p=next((v['position'] for v in load('Assets/_Game/Resources/HoiAnData.json')['nodes'] if v['id']==node),p)
  x,y=(pair(s,'mapPosition') if 'hasMapPosition: 1' in s else (p['x'],p['y']))
  ox,oy=pair(s,'offset')
  # Match 540-wide runtime, zoom 1.8 (node sprites counter-scale).
  x=x*W+ox*W/(540*1.8);y=(1-y)*H-oy*W/(540*1.8)
  tile=im.crop((int(x)-140,int(y)-180,int(x)+140,int(y)+100)).resize((280,280))
  draw=ImageDraw.Draw(tile);draw.line((130,180,150,180),fill='red',width=2);draw.line((140,170,140,190),fill='red',width=2)
  draw.rectangle((0,0,279,25),fill='white');draw.text((6,5),node+' '+npc,fill='black')
  tiles.append(tile);records.append(dict(city=city,node=node,npc=npc,offset=pair(s,'offset'),anchor=pair(s,'mapPosition'),hasMapPosition='hasMapPosition: 1' in s,pixel=[x,y],image=[W,H]))
 sheet=Image.new('RGB',(280*3,280*((len(tiles)+2)//3)), 'white')
 for i,tile in enumerate(tiles):sheet.paste(tile,(i%3*280,i//3*280))
 sheet.save(O/(city+'-ground-review.png'))
(O/'inventory.json').write_text(json.dumps(records,indent=2))
