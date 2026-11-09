"""Phase A only: trace owner's exact pen colours. Never applies Unity changes.

Stroke endpoints and crossings are image observations, NOT gameplay junctions.
No old route graph is used to invent connections in the new design.
"""
from pathlib import Path
import json, hashlib, math, csv, datetime
import numpy as np
from scipy import ndimage as ndi
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Validation/OwnerRouteRedesign-20261019'
DOC=ROOT/'Docs/Maps/OwnerRouteRedesign-20261019'
SOURCE=ROOT/'Tools/SixCityRouteEditor/data/source.json'
CONFIG={
 'hoi_an':('174731',[('H0',270,511),('H8',146,68),('H15',796,269)]),
 'hue':('174630',[('U0',502,603),('U17',1184,164),('U13',1032,898)]),
 'da_lat':('174736',[('D0',441,397),('D14',156,87),('D17',768,511)]),
 'jeonju':('174741',[('J0',412,563),('J21',131,148),('J20',736,134)]),
 'gyeongju':('174745',[('G0',232,520),('G10',650,98),('G20',727,388)]),
 'seoul':('174751',[('S0',402,428),('S3',227,130),('S20',724,331)])}
COLORS={'walk':(63,72,204),'ferry':(136,0,21)}
FONT=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',15)
SMALL=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',11)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,j):p.parent.mkdir(exist_ok=True,parents=True);p.write_text(json.dumps(j,ensure_ascii=False,indent=2),encoding='utf-8')
def fit(city,marks):
 nodes={n['id']:n for n in city['nodes']};coeff=[];res=[]
 for axis,offset in [('x',1),('y',2)]:
  m=np.array([[nodes[p[0]]['position'][axis],1] for p in marks]);v=np.array([p[offset] for p in marks])
  co=np.linalg.lstsq(m,v,rcond=None)[0];coeff.append(co.tolist());res.append(float(np.max(np.abs(m@co-v))))
 return {'xScale':coeff[0][0],'xOffset':coeff[0][1],'yScale':coeff[1][0],'yOffset':coeff[1][1],
         'controlPoints':[{'node':n,'inputPixel':[x,y]} for n,x,y in marks],
         'maxControlResidualPixels':max(res),'method':'Affine scale/offset fitted to visually identified canonical node centres. Approximate raster precision, not new node coordinates.'}
def pixel(p,t):return (p['x']*t['xScale']+t['xOffset'],p['y']*t['yScale']+t['yOffset'])
def units(p,t):return {'x':round((p[0]-t['xOffset'])/t['xScale'],4),'y':round((p[1]-t['yOffset'])/t['yScale'],4)}
def thin(mask):
 a=np.pad(mask.astype(bool),1)
 for _ in range(60):
  changed=False
  for phase in range(2):
   p=[a[:-2,1:-1],a[:-2,2:],a[1:-1,2:],a[2:,2:],a[2:,1:-1],a[2:,:-2],a[1:-1,:-2],a[:-2,:-2]]
   count=sum(v.astype(np.uint8) for v in p)
   transitions=sum((~p[i]&p[(i+1)%8]).astype(np.uint8) for i in range(8))
   if phase==0:keep=(~(p[0]&p[2]&p[4]))&(~(p[2]&p[4]&p[6]))
   else:keep=(~(p[0]&p[2]&p[6]))&(~(p[0]&p[4]&p[6]))
   rm=a[1:-1,1:-1]&(count>=2)&(count<=6)&(transitions==1)&keep
   if rm.any():a[1:-1,1:-1][rm]=False;changed=True
  if not changed:break
 return a[1:-1,1:-1]
def simplify(points,tol=.65):
 if len(points)<3:return points
 q=np.asarray(points,dtype=float);v=q[-1]-q[0];den=float(v@v)
 d=np.linalg.norm(q-q[0],axis=1) if den==0 else np.linalg.norm(q-(q[0]+np.clip((q-q[0])@v/den,0,1)[:,None]*v),axis=1)
 k=int(np.argmax(d))
 if d[k]<=tol:return [points[0],points[-1]]
 return simplify(points[:k+1],tol)[:-1]+simplify(points[k:],tol)
def chains(mask):
 sk=thin(mask);vertices={(int(x),int(y)) for y,x in zip(*np.where(sk))};adj={}
 for x,y in vertices:
  neighbours=[]
  for dy in (-1,0,1):
   for dx in (-1,0,1):
    if not(dx or dy) or (x+dx,y+dy) not in vertices:continue
    # Remove redundant corner diagonals, retaining genuine diagonals.
    if dx and dy and ((x+dx,y) in vertices or (x,y+dy) in vertices):continue
    neighbours.append((x+dx,y+dy))
  adj[(x,y)]=sorted(neighbours)
 visited=set();paths=[]
 def edge(a,b):return tuple(sorted((a,b)))
 def trace(a,b):
  line=[a];visited.add(edge(a,b));prev=a;cur=b
  while True:
   line.append(cur)
   if len(adj[cur])!=2:break
   nxt=adj[cur][0] if adj[cur][0]!=prev else adj[cur][1]
   if edge(cur,nxt) in visited:break
   visited.add(edge(cur,nxt));prev,cur=cur,nxt
  if len(line)>=3:paths.append(simplify(line))
 for v in sorted(vertices):
  if len(adj[v])!=2:
   for w in adj[v]:
    if edge(v,w) not in visited:trace(v,w)
 for v in sorted(vertices):
  for w in adj[v]:
   if edge(v,w) not in visited:trace(v,w)
 return paths,[v for v in vertices if len(adj[v])>=3],sk
def label(d,p,text,color=(255,247,220),font=FONT):
 d.text((p[0]+5,p[1]-20),text,fill=color,font=font,stroke_width=2,stroke_fill=(9,27,35))

def main():
 DOC.mkdir(exist_ok=True,parents=True)
 snapshot=read(SOURCE);live=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
 legacy=read(ROOT/'Assets/_Game/Resources/HoiAnData.json')
 # Prove the existing export is a suitable coordinate snapshot; never update it.
 for city in snapshot['cities']:
  c=next(c for c in live['cities'] if c['id']==city['id']);byid={n['id']:n for n in c['nodes']}
  if city['id']=='hoi_an':positions={n['id']:n['position'] for n in legacy['nodes']}
  else:positions={n['id']:n['position'] for n in c['nodes']}
  for n in city['nodes']:
   for ax,expected in [('x',positions[n['id']]['x']*1000),('y',(1-positions[n['id']]['y'])*1000)]:assert abs(n['position'][ax]-expected)<1e-6
   arr=byid[n['id']].get('arrivalPosition') if byid[n['id']].get('hasArrivalPosition') else positions[n['id']]
   assert abs(n['arrival']['x']-arr['x']*1000)<1e-6 and abs(n['arrival']['y']-(1-arr['y'])*1000)<1e-6
  assert sha(ROOT/city['sourceImage'])==city['sourceImageSHA256']
  assert len(c['edges'])==len(city['referenceEdges'])
  for e,ref in zip(c['edges'],city['referenceEdges']):assert e==ref['sourceRecord']
 baselinepaths=['Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/HoiAnData.json',
   'Assets/_Game/Scripts/Core/GameData.cs','Assets/_Game/Scripts/Maps/CityRouteGeometry.cs','Assets/_Game/Scripts/Maps/CityMap.cs',
   'Assets/_Game/Scripts/Maps/CityMapPaths.cs','Assets/_Game/Resources/Content/StoryInteractions.json',
   'Assets/_Game/Resources/Content/LeisureActivities.json','Assets/_Game/Resources/Content/CulturalRelics.json',
   'Assets/_Game/Resources/AdventureUIArt.asset','Assets/_Game/Resources/DesignKitArt.asset','ProjectSettings/ProjectSettings.asset']
 baselinepaths += [c['sourceImage'] for c in snapshot['cities']]
 baseline=[{'path':p,'sha256':sha(ROOT/p)} for p in baselinepaths]
 dump(OUT/'production-baseline.json',baseline)
 results=[];node_rows=[]
 for c in snapshot['cities']:
  cid=c['id'];stamp,marks=CONFIG[cid];path=OUT/'inputs'/f'Screenshot 2026-10-19 {stamp}.png'
  inp=Image.open(path).convert('RGB');a=np.asarray(inp).astype(np.int16);t=fit(c,marks)
  canonical=Image.open(ROOT/c['sourceImage']).convert('RGB');preview=canonical.copy();d=ImageDraw.Draw(preview)
  owner=inp.copy();od=ImageDraw.Draw(owner);stroke_records=[];observed={};allmask=np.zeros(a.shape[:2],bool)
  for travel,color in COLORS.items():
   mask=np.max(np.abs(a-np.array(color)),axis=2)<=2
   # Only within map content, excluding toolbar legend and outer margins.
   yy,xx=np.indices(mask.shape)
   mask&=(xx>=max(0,t['xOffset']))&(xx<t['xOffset']+1000*t['xScale'])&(yy>=max(0,t['yOffset']))&(yy<t['yOffset']+1000*t['yScale'])
   # Never bridge gaps automatically. Keep owner pen components, exclude single pixels.
   lab,count=ndi.label(mask,np.ones((3,3),int));sizes=np.bincount(lab.ravel());mask&=sizes[lab]>=3
   if mask.any():
    allmask|=mask;paths,junctions,skeleton=chains(mask)
    dist=ndi.distance_transform_edt(~mask)
   else:paths=[];junctions=[];skeleton=mask;dist=np.full(mask.shape,np.inf)
   observed[travel]={'pixels':int(mask.sum()),'components':int(sum(sizes[1:]>=3)),'chains':len(paths),'distance':dist,'mask':mask}
   for i,line in enumerate(paths):
    points=[units(p,t) for p in line];rec={'strokeId':f'{c["prefix"]}-{travel[0].upper()}S{i+1:03d}',
     'travel':travel,'pointsMap0To1000':points,'inputPixels':line,'assignment':'UNASSIGNED_INK_CHAIN_NOT_A_GAME_EDGE'}
    stroke_records.append(rec)
    coords=[(p['x']*canonical.width/1000,p['y']*canonical.height/1000) for p in points]
    if len(coords)>1:d.line(coords,fill=(40,205,173) if travel=='walk' else (234,66,81),width=3)
  detached=[];near_marker_only=[];near_arrival_only=[]
  for n in c['nodes']:
   pp=pixel(n['position'],t);ap=pixel(n['arrival'],t)
   def distance(point,travel):
    x=int(round(point[0]));y=int(round(point[1]));arr=observed[travel]['distance'];return float(arr[np.clip(y,0,arr.shape[0]-1),np.clip(x,0,arr.shape[1]-1)])
   mp=min(distance(pp,k) for k in COLORS);ar=min(distance(ap,k) for k in COLORS)
   # Advisory distance in the owner's original image, normalised to ~840px map.
   threshold=12*t['xScale']/.84
   status='NEEDS_ACCESS_REVIEW' if min(mp,ar)>threshold else 'NEEDS_ARRIVAL_CONNECTOR_REVIEW' if ar>threshold else 'NEAR_EXISTING_ARRIVAL'
   row={'city':cid,'nodeId':n['id'],'name':n['name'],'status':status,'distanceToMarkerPx':round(mp,2),
    'distanceToArrivalPx':round(ar,2),'inputMarker':[round(v,2) for v in pp],'inputArrival':[round(v,2) for v in ap],
    'canonicalMap':n['position'],'arrivalMap':n['arrival'],'thresholdInputPixels':round(threshold,2)}
   node_rows.append(row)
   if status=='NEEDS_ACCESS_REVIEW':detached.append(n['id'])
   elif status=='NEEDS_ARRIVAL_CONNECTOR_REVIEW':near_marker_only.append(n['id'])
   if ar<=threshold and mp>threshold:near_arrival_only.append(n['id'])
   q=(n['position']['x']*canonical.width/1000,n['position']['y']*canonical.height/1000)
   ground=(n['arrival']['x']*canonical.width/1000,n['arrival']['y']*canonical.height/1000)
   col=(255,175,64) if status!='NEAR_EXISTING_ARRIVAL' else (244,247,236)
   d.ellipse((q[0]-7,q[1]-7,q[0]+7,q[1]+7),fill=(11,27,34),outline=col,width=2);label(d,q,n['id'],col)
   d.rectangle((ground[0]-3,ground[1]-3,ground[0]+3,ground[1]+3),fill=(255,239,131))
   if math.dist(q,ground)>8:d.line([q,ground],fill=(159,158,151),width=1)
   if status!='NEAR_EXISTING_ARRIVAL':
    r=15*t['xScale']/.84;od.ellipse((pp[0]-r,pp[1]-r,pp[0]+r,pp[1]+r),outline=(255,190,50),width=2)
  preview.save(OUT/f'{cid}-canonical-trace-preview.png');owner.save(OUT/f'{cid}-access-review.png')
  dump(OUT/f'{cid}-ink-strokes.json',{'cityId':cid,'sourceImage':str(path.relative_to(ROOT)),'transform':t,
    'status':'PHASE_A_DRAFT_NOT_APPROVED_FOR_IMPORT','strokes':stroke_records,
    'warning':'These are centreline chains separated at pixel junctions, not approved node-to-node routes. No crossing connectivity inferred.'})
  results.append({'cityId':cid,'name':c['name'],'nodeCount':len(c['nodes']),'oldEdgeCount':len(c['referenceEdges']),
    'transform':t,'observedWalkPixels':observed['walk']['pixels'],'observedFerryPixels':observed['ferry']['pixels'],
    'tracedChains':len(stroke_records),'pointCount':sum(len(x['pointsMap0To1000']) for x in stroke_records),
    'detachedNodes':detached,'needsArrivalConnector':near_marker_only,'nearArrivalOnly':near_arrival_only,
    'approvedNewEdges':None,'productionApplied':False})
  print(cid,'chains',len(stroke_records),'detached',detached,'arrivalReview',near_marker_only,flush=True)
 with (OUT/'node-access-review.csv').open('w',encoding='utf-8-sig',newline='') as f:
  w=csv.DictWriter(f,fieldnames=list(node_rows[0]));w.writeheader();w.writerows(node_rows)
 dump(OUT/'node-access-review.json',node_rows)
 counts={'nodes':sum(len(c['nodes']) for c in snapshot['cities']),'oldEdges':sum(len(c['edges']) for c in live['cities']),
  'quests':len(live['quests']),'legacyPuzzleDefinitions':len(live['puzzles']),
  'storyInteractions':len(read(ROOT/'Assets/_Game/Resources/Content/StoryInteractions.json')['entries']),
  'leisure':len(read(ROOT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']),
  'relics':len(read(ROOT/'Assets/_Game/Resources/Content/CulturalRelics.json')['relics']),
  'fragments':sum(len(r['fragmentIds']) for r in read(ROOT/'Assets/_Game/Resources/Content/CulturalRelics.json')['relics'])}
 summary={'created':datetime.datetime.now().astimezone().isoformat(),'stage':'A_DIGITIZATION_DRAFT',
  'status':'NEEDS_OWNER_CLARIFICATION_BEFORE_PRODUCTION','counts':counts,'cities':results,'productionApplied':False}
 dump(OUT/'summary.json',summary)
 assert all(sha(ROOT/e['path'])==e['sha256'] for e in baseline)
 print('Protected production files unchanged:',len(baseline));print('Counts:',counts)

if __name__=='__main__':main()
