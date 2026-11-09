"""Compile the approved Phase A ink into the one runtime navigation graph.

No image re-tracing, canonical anchor edits, quest edits or old-edge copying.
Legacy topology is read ONLY to compile existing progression access contracts.
"""
from pathlib import Path
from collections import defaultdict
import json,math,heapq,hashlib,csv
import numpy as np
from PIL import Image,ImageDraw,ImageFont
from scipy import ndimage as ndi

ROOT=Path(__file__).resolve().parents[1]
STAGE=ROOT/'Validation/OwnerRouteRedesign-20261019'
OUT=ROOT/'Validation/OwnerRoutePhaseB-20261019'
DOC=ROOT/'Docs/Maps/OwnerRoutePhaseB-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,x):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
def distance(a,b):return math.hypot(a[0]-b[0],a[1]-b[1])
def project(p,a,b):
 dx,dy=b[0]-a[0],b[1]-a[1];den=dx*dx+dy*dy
 f=max(0,min(1,((p[0]-a[0])*dx+(p[1]-a[1])*dy)/den))if den else 0
 q=(a[0]+dx*f,a[1]+dy*f);return distance(p,q),q,f
def minimum(rules,new):
 new=frozenset(new)
 if any(r<=new for r in rules):return False
 rules[:]=[r for r in rules if not new<r];rules.append(new);return True
def contracts(city):
 result={n['id']:[] for n in city['nodes']};nodes={n['id']:n for n in city['nodes']}
 for stage in range(5):
  reach={i:[] for i in nodes};reach[city['spawnNodeId']]=[frozenset()]
  changed=True
  while changed:
   changed=False
   for e in city['edges']:
    for a,b in [(e['from'],e['to'])]+([(e['to'],e['from'])]if e.get('bidirectional',True)else[]):
     if nodes[b].get('revealStage',0)>stage:continue
     for flags in list(reach[a]):changed|=minimum(reach[b],flags|set(e.get('requiredFlags',[]))|set(nodes[b].get('requiredFlags',[])))
  for node,flags in reach.items():
   result[node].extend({'stage':stage,'flags':sorted(f)}for f in sorted(flags,key=lambda x:sorted(x)))
 return result

class Graph:
 def __init__(self):self.points=[];self.edges={};self.grid=defaultdict(list);self.provenance={};self.keep=set();self.canonical=set()
 def vertex(self,p,merge=True):
  p=tuple(map(float,p));cell=(int(p[0]//1.5),int(p[1]//1.5))
  if merge:
   candidates=[i for dx in (-1,0,1)for dy in (-1,0,1)for i in self.grid[(cell[0]+dx,cell[1]+dy)]if i not in self.canonical and distance(self.points[i],p)<=1.45]
   if candidates:return min(candidates,key=lambda i:distance(self.points[i],p))
  i=len(self.points);self.points.append(p);self.grid[cell].append(i);return i
 def add(self,a,b,mode='walk',source=''):
  if a==b:return
  k=(min(a,b),max(a,b),mode)
  if k not in self.edges:self.edges[k]={'a':a,'b':b,'travel':mode,'points':[self.points[a],self.points[b]],'source':[source]}
  elif source not in self.edges[k]['source']:self.edges[k]['source'].append(source)
 def nearest(self,p,mode='walk'):
  return min(((*project(p,self.points[e['a']],self.points[e['b']])[:2],k)for k,e in self.edges.items()if e['travel']==mode and not all(s.startswith('arrival:')for s in e['source'])),key=lambda x:x[0])
 def split(self,key,q):
  e=self.edges.pop(key);v=self.vertex(q)
  for a,b in [(e['a'],v),(v,e['b'])]:
   for source in e['source']:self.add(a,b,e['travel'],source)
  return v
 def components(self):
  adj=defaultdict(list)
  for e in self.edges.values():adj[e['a']].append(e['b']);adj[e['b']].append(e['a'])
  comp={}
  for a in adj:
   if a in comp:continue
   c=len(set(comp.values()));todo=[a];comp[a]=c
   while todo:
    for b in adj[todo.pop()]:
     if b not in comp:comp[b]=c;todo.append(b)
  return comp,adj

def compile_city(snapshot,city):
 cid=city['id'];ink=read(STAGE/f'{cid}-ink-strokes.json');t=ink['transform'];g=Graph();blocked=[];gaps=[];connectors=[]
 def pixel(point):return(point['x']*t['xScale']+t['xOffset'],point['y']*t['yScale']+t['yOffset'])
 def unity(p):return{'x':round((p[0]-t['xOffset'])/t['xScale']/1000,7),'y':round(1-(p[1]-t['yOffset'])/t['yScale']/1000,7)}
 # Work on approved Phase A coordinates. Shared ink vertices become junctions.
 for stroke in ink['strokes']:
  if stroke['travel']!='walk':continue
  vertices=[g.vertex(p)for p in stroke['inputPixels']]
  for a,b in zip(vertices,vertices[1:]):g.add(a,b,'walk',stroke['strokeId'])
 # Split actual geometric intersections, never proximity-only crossings.
 initial=list(g.edges.items());cuts=defaultdict(list)
 for i,(ka,a) in enumerate(initial):
  p,q=g.points[a['a']],g.points[a['b']];rx,ry=q[0]-p[0],q[1]-p[1]
  for kb,b in initial[i+1:]:
   if len({a['a'],a['b'],b['a'],b['b']})<4:continue
   u,v=g.points[b['a']],g.points[b['b']]
   if max(p[0],q[0])<min(u[0],v[0])or max(u[0],v[0])<min(p[0],q[0])or max(p[1],q[1])<min(u[1],v[1])or max(u[1],v[1])<min(p[1],q[1]):continue
   sx,sy=v[0]-u[0],v[1]-u[1];den=rx*sy-ry*sx
   if abs(den)<1e-9:continue
   ta=((u[0]-p[0])*sy-(u[1]-p[1])*sx)/den;tb=((u[0]-p[0])*ry-(u[1]-p[1])*rx)/den
   if 0<=ta<=1 and 0<=tb<=1:
    x=(p[0]+ta*rx,p[1]+ta*ry);cuts[ka].append((ta,x));cuts[kb].append((tb,x))
 for key,items in cuts.items():
  e=g.edges.pop(key);vs=[e['a']]+[g.vertex(q)for _,q in sorted(items)]+[e['b']]
  for a,b in zip(vs,vs[1:]):
   for source in e['source']:g.add(a,b,'walk',source)
 # Phase A omits two-pixel skeleton links. Reconnect only where the ORIGINAL
 # blue pen footprint proves contact, not where separate streets are just near.
 original=np.asarray(Image.open(ROOT/ink['sourceImage']).convert('RGB')).astype(np.int16)
 mask=np.max(np.abs(original-np.array([63,72,204])),axis=2)<=2;inkdistance=ndi.distance_transform_edt(~mask)
 for _ in range(4):
  comp,adj=g.components();changed=False
  for end in [v for v in adj if len(adj[v])==1]:
   a=g.points[end];candidate=[]
   for key,e in g.edges.items():
    if comp.get(e['a'])==comp.get(end):continue
    dist,q,_=project(a,g.points[e['a']],g.points[e['b']])
    if dist<=6:candidate.append((dist,q,key))
   for dist,q,key in sorted(candidate):
    probes=[(a[0]+(q[0]-a[0])*f,a[1]+(q[1]-a[1])*f)for f in np.linspace(0,1,15)]
    if all(inkdistance[round(y),round(x)]<=1.5 for x,y in probes):
     attach=g.split(key,q);g.add(end,attach,'walk','verified-ink-contact');gaps.append({'reason':'original pen footprint contact','inputPixels':[a,q]});changed=True;break
  if not changed:break
 # Ink is hidden by existing node discs. Only reconnect endpoints inside that
 # disc footprint; do not join arbitrary nearby streets or use the old graph.
 for node in snapshot['nodes']:
  marker=pixel(node['position']);comp,adj=g.components()
  ends=[i for i in adj if len(adj[i])==1 and distance(g.points[i],marker)<=13]
  for end in ends:
   candidates=[]
   for key,e in g.edges.items():
    if comp.get(e['a'])==comp.get(end):continue
    dist,q,_=project(g.points[end],g.points[e['a']],g.points[e['b']])
    if dist<=16 and distance(q,marker)<=13:candidates.append((dist,q,key))
   if candidates:
    _,q,key=min(candidates);attach=g.split(key,q);g.add(end,attach,'walk','node-disc:'+node['id']);gaps.append({'nodeId':node['id'],'reason':'ink occluded by node disc','inputPixels':[g.points[end],q]})
 # U10's isolated pen cap is not a complete street. Use the existing visible
 # pedestrian forecourt below its pavilion, joining U11's drawn wall corridor.
 # This is an access spur, not a copied legacy edge or a new gameplay junction.
 if cid=='hue':
  arrival=pixel(next(n for n in snapshot['nodes']if n['id']=='U10')['arrival'])
  route=[arrival,(564,276),(564,301),(604,309),(639,309),(677,319)]
  maincomp,mainadj=g.components();startcomp=maincomp[g.nearest(arrival)[2][0]]
  candidate=[]
  for key,e in g.edges.items():
   if maincomp[e['a']]==startcomp:continue
   dist,q,_=project(route[-1],g.points[e['a']],g.points[e['b']]);candidate.append((dist,q,key))
  _,q,key=min(candidate);attach=g.split(key,q);vertices=[g.vertex(p)for p in route]+[attach]
  for a,b in zip(vertices,vertices[1:]):g.add(a,b,'walk','U10:pavilion-front-access')
  gaps.append({'nodeId':'U10','reason':'pavilion front pedestrian approach to drawn U11 corridor','inputPixels':route+[q]})
 canonical={}
 for node in snapshot['nodes']:
  arrival=pixel(node['arrival']);dist,q,key=g.nearest(arrival)
  attach=g.split(key,q);anchor=g.vertex(arrival,merge=False);canonical[anchor]=node['id'];g.canonical.add(anchor);g.keep.update([anchor,attach]);g.add(anchor,attach,'walk','arrival:'+node['id'])
  connectors.append({'nodeId':node['id'],'distancePixels':round(dist,3),'arrival':node['arrival'],'attachmentInputPixel':q,'inputPolyline':[arrival,q],'status':'IMPLEMENTED_SHORT_APPROACH'if dist>12 else'IMPLEMENTED_ANCHOR_CONNECTOR'})
 # Red endpoint geometry is never snapped or reassigned to a nearest NPC.
 # Owner clarification: R1 is a cross-river service. Preserve its red A-B
 # exactly, then continue on WATER to the opposite existing H16 jetty. The
 # already drawn H16-H12 walk corridor is reused, never duplicated.
 ferry_names={('hoi_an','H-FS001'):'R1',('da_lat','D-FS001'):'R2',('gyeongju','G-FS001'):'R3',('gyeongju','G-FS002'):'R4',('seoul','S-FS001'):'R5'}
 transfers=[]
 for stroke in ink['strokes']:
  if stroke['travel']!='ferry':continue
  rid=ferry_names[(cid,stroke['strokeId'])];pts=stroke['inputPixels']
  if rid=='R1':
   landing=pixel(next(n for n in snapshot['nodes']if n['id']=='H16')['arrival'])
   pts=pts+[(312,530),(387,534),(470,517),(542,501),landing]
  vs=[g.vertex(p,merge=False)for p in pts]
  for a,b in zip(vs,vs[1:]):g.add(a,b,'ferry',rid)
  for end in [vs[0],vs[-1]]:
   dist,q,key=g.nearest(g.points[end]);assert dist<=13,(cid,rid,'landing too far from walking ink',dist)
   attach=g.split(key,q);g.add(end,attach,'walk',rid+':landing');g.keep.update([end,attach]);transfers.append({'route':rid,'vertex':end,'walkAttachment':attach,'gapPixels':round(dist,3),'point':unity(g.points[end])})
 # Degree-two samples become bends, not thousands of navigation objects.
 comp,adj=g.components();important={i for i in adj if len(adj[i])!=2}|g.keep;used=set();compressed=[]
 edgeat=defaultdict(list)
 for k,e in g.edges.items():edgeat[e['a']].append((k,e));edgeat[e['b']].append((k,e))
 for start in sorted(important):
  for key,first in edgeat[start]:
   if key in used:continue
   used.add(key);points=[g.points[start]];current=start;e=first;sources=set();mode=e['travel']
   while True:
    sources.update(e['source']);nxt=e['b']if e['a']==current else e['a'];points.append(g.points[nxt])
    if nxt in important:break
    choices=[(k,f)for k,f in edgeat[nxt]if k not in used]
    if not choices:break
    key,e=choices[0];used.add(key);current=nxt;assert e['travel']==mode
   if start!=nxt:compressed.append({'a':start,'b':nxt,'travel':mode,'pixels':points,'source':sorted(sources)})
 vertex_ids={i:('N:'+canonical[i]if i in canonical else'J'+str(i))for i in important}
 vs=[{'id':vertex_ids[i],'position':unity(g.points[i]),'canonicalNodeId':canonical.get(i,''),'transfer':i in {r['vertex']for r in transfers}}for i in sorted(important)]
 rawadj=defaultdict(list)
 for idx,e in enumerate(compressed):
  length=sum(distance(a,b)for a,b in zip(e['pixels'],e['pixels'][1:]));rawadj[e['a']].append((e['b'],length,idx));rawadj[e['b']].append((e['a'],length,idx))
 def shortest(start,allowed_flags=None):
  ds={start:0};prev={};todo=[(0,start)]
  while todo:
   cost,u=heapq.heappop(todo)
   if cost>ds[u]+1e-7:continue
   for v,l,i in rawadj[u]:
    if cost+l<ds.get(v,float('inf'))-1e-7:ds[v]=cost+l;prev[v]=(u,i);heapq.heappush(todo,(cost+l,v))
  return ds,prev
 can={name:v for v,name in canonical.items()};spawn=can[city['spawnNodeId']];ds,prev=shortest(spawn)
 missing=[name for name,v in can.items()if v not in ds]
 if missing:
  dump(OUT/f'{cid}-disconnected-debug.json',{'missing':missing,'connectors':connectors,'gaps':gaps,'vertices':vs,'edges':[{'from':vertex_ids[e['a']],'to':vertex_ids[e['b']],'source':e['source'],'points':[unity(p)for p in e['pixels']]}for e in compressed]})
  raise RuntimeError((cid,'Disconnected owner ink; inspect node-disc gaps',missing))
 access=contracts(city);rules=[[]for _ in compressed]
 # Compile a reveal corridor for each EXISTING node access clause. The owner's
 # R5 is an essential river crossing, not the retired optional S6-S14 shortcut;
 # carrying that old edge's ferryman flag across would introduce a new main-
 # story prerequisite. Keep the NPC talk flag/history, never gate R5 with it.
 for node,v in can.items():
  for rule in access[node]:
   variants=[set(rule['flags'])]
   for flags in variants:
    _,route_previous=shortest(spawn,flags);cur=v
    while cur!=spawn:
     assert cur in route_previous,(cid,node,'no physical route for existing access contract',rule)
     parent,idx=route_previous[cur];rules[idx].append({'stage':rule['stage'],'flags':sorted(flags)});cur=parent
 distances={name:shortest(v)[0]for name,v in can.items()}
 for idx,e in enumerate(compressed):
  if not rules[idx]:
   nearest=min(can,key=lambda name:min(distances[name].get(e['a'],1e9),distances[name].get(e['b'],1e9)))
   rules[idx].extend(access[nearest])
  uniq={(r['stage'],tuple(r['flags']))for r in rules[idx]};pruned=[]
  for stage,flags in sorted(uniq):
   if not any(x['stage']<=stage and set(x['flags'])<=set(flags)for x in pruned):pruned.append({'stage':stage,'flags':list(flags)})
  e.update(id=f'{snapshot["prefix"]}-N{idx+1:03d}',**{'from':vertex_ids[e['a']],'to':vertex_ids[e['b']]},bidirectional=True,points=[unity(p)for p in e['pixels']],access=pruned,requiredFlags=[])
 edges=[{k:e[k]for k in ['id','from','to','travel','bidirectional','points','access','requiredFlags','source']}for e in compressed]
 nodes=[{'nodeId':n,'vertexId':vertex_ids[v],'access':access[n]}for n,v in can.items()]
 # Never activate an off-map graph or mutate locked canonical anchor positions.
 assert all(0<=p[a]<=1 for e in edges for p in e['points']for a in ['x','y'])
 data={'cityId':cid,'enabled':True,'imageAspect':snapshot['imageWidth']/snapshot['imageHeight'],'vertices':vs,'edges':edges,'nodes':nodes}
 dump(OUT/f'{cid}-implementation.json',{'graph':data,'connectors':connectors,'discGaps':gaps,'transfers':[{**r,'vertexId':vertex_ids[r['vertex']]}for r in transfers],'blocked':blocked,'sourceSHA256':hashlib.sha256((STAGE/f'{cid}-ink-strokes.json').read_bytes()).hexdigest()})
 im=Image.open(ROOT/snapshot['sourceImage']).convert('RGB');draw=ImageDraw.Draw(im)
 for e in edges:
  points=[(p['x']*im.width,(1-p['y'])*im.height)for p in e['points']];draw.line(points,fill=(205,64,78)if e['travel']=='ferry'else(64,137,188),width=3)
 for n in snapshot['nodes']:
  x=n['position']['x']/1000*im.width;y=n['position']['y']/1000*im.height;draw.ellipse((x-4,y-4,x+4,y+4),fill=(250,247,226));draw.text((x+7,y-12),n['id'],fill='white',stroke_width=2,stroke_fill='#183f46')
 im.save(OUT/f'{cid}-applied-graph-preview.png')
 return data

def main():
 OUT.mkdir(exist_ok=True,parents=True);DOC.mkdir(exist_ok=True,parents=True)
 src=read(ROOT/'Tools/SixCityRouteEditor/data/source.json');live=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json');cities=[]
 for snapshot in src['cities']:
  graph=compile_city(snapshot,next(c for c in live['cities']if c['id']==snapshot['id']));cities.append(graph);print(graph['cityId'],len(graph['vertices']),'vertices',len(graph['edges']),'edges')
 catalog={'version':1,'source':'Owner-approved Phase A drawings, Phase B 2026-10-19','costPolicy':'Polyline length in canonical image-proportional units. No modal penalty; boarding animation is presentation only.','cities':cities}
 dump(OUT/'CityNavigation.proposed.json',catalog)
 # Explicit production apply after all cities pass geometry/reference checks.
 import sys
 if '--apply'in sys.argv:dump(ROOT/'Assets/_Game/Resources/Content/CityNavigation.json',catalog)
if __name__=='__main__':main()
