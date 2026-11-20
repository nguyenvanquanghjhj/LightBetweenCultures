"""Author separate map corridors and native sprite outline data, without editing PNG pixels.
All new terrain remains explicitly pending visual acceptance; never labels motion PASS as terrain PASS.
"""
from pathlib import Path
import json,math,heapq,collections,shutil
from PIL import Image
from author_full_story import R,B,OUT,read,write,point

def outline(image,box):
 # Analysis only: derive a vector boundary for Unity's native sprite mesh.
 # No PNG pixels are replaced or exported. Source/white inner details remain.
 x,y,w,h=box;im=image.crop((x,y,x+w,y+h)).convert('RGB');pixels=im.load()
 fg={(i,j) for j in range(h) for i in range(w) if pixels[i,j][0]<230 or pixels[i,j][1]<215 or pixels[i,j][2]<195}
 components=[]
 while fg:
  seed=fg.pop();q=[seed];component={seed}
  while q:
   i,j=q.pop()
   for z in [(i-1,j),(i+1,j),(i,j-1),(i,j+1)]:
    if z in fg:fg.remove(z);component.add(z);q.append(z)
  components.append(component)
 body=max(components,key=len)
 # Moore tracing on the largest body; separate effects aren't promoted to art.
 dirs=[(-1,0),(-1,-1),(0,-1),(1,-1),(1,0),(1,1),(0,1),(-1,1)]
 start=min(body,key=lambda p:(p[1],p[0]));current=start;back=(start[0]-1,start[1]);path=[];seen=set()
 while (current,back) not in seen:
  seen.add((current,back));path.append(current);dx=back[0]-current[0];dy=back[1]-current[1];k=dirs.index((dx,dy))
  for step in range(1,9):
   t=(k+step)%8;delta=dirs[t];n=(current[0]+delta[0],current[1]+delta[1])
   if n in body:
    before=dirs[(t-1)%8];back=(current[0]+before[0],current[1]+before[1]);current=n;break
  else:break
  if len(path)>w*h*2:raise ValueError('Contour did not close')
 def rdp(ps,eps=1.2):
  if len(ps)<3:return ps
  a,b=ps[0],ps[-1];den=math.dist(a,b)
  ds=[abs((b[0]-a[0])*(a[1]-p[1])-(a[0]-p[0])*(b[1]-a[1]))/den if den else math.dist(a,p) for p in ps]
  i=max(range(len(ds)),key=ds.__getitem__)
  return rdp(ps[:i+1])+rdp(ps[i:])[1:] if ds[i]>eps else [a,b]
 mid=len(path)//2;path=rdp(path[:mid+1])+rdp(path[mid:]+[path[0]])[1:-1]
 return [dict(x=i-w*.5,y=h*.5-j) for i,j in path]

def main():
 p=R/'Assets/_Game/Resources/Content/GameplayContent.json';d=read(p);rows=[];baseline=read(B/p.relative_to(R))
 # Each chain is authored on this city's own approved painting. Anchors change
 # only arrival positions, never canonical node marker coordinates or IDs.
 G={0:(580,935),1:(674,765),2:(302,543),3:(517,284),4:(681,269),5:(202,262),6:(403,281),7:(803,457),8:(1102,489),9:(907,414),10:(1146,203),11:(786,565),12:(875,653),13:(1116,695),14:(1284,397),15:(1105,281),16:(735,884),17:(220,399),18:(1152,259),19:(803,309),20:(1270,654),21:(190,827)}
 S={0:(732,778),1:(876,600),2:(429,349),3:(395,233),4:(302,420),5:(775,440),6:(1140,753),7:(1067,533),8:(791,410),9:(1105,299),10:(1041,167),11:(1246,312),12:(650,762),13:(626,542),14:(383,830),15:(590,534),16:(1260,861),17:(675,374),18:(968,577),19:(1282,797),20:(1206,640),21:(465,467),22:(953,237)}
 chains={
 'gyeongju':[
  [G[0],(625,914),(671,886),G[16]],
  [G[16],(715,860),(705,827),(687,793),G[1],(689,724),(718,672),(751,635),(783,602),G[11]],
  [G[11],(771,535),(752,510),(718,482),(683,458),(654,430),(622,408),(600,383),(569,365),(535,352),(500,334),(480,309),G[3],(556,277),(606,273),G[4]],
  [G[3],(482,283),(448,279),G[6],(369,285),(331,290),(293,290),(252,279),G[5]],
  [G[4],(710,283),(752,309),G[19],(840,347),(876,373),G[9]],
  [G[9],(957,399),(997,374),(1038,340),(1075,309),G[15],(1126,247),G[18],G[10]],
  [G[11],(812,535),(834,506),G[7],(856,443),(903,457),(955,478),(1013,498),G[8]],
  [G[8],(1138,474),(1173,450),(1210,421),G[14]],
  [G[11],(823,587),(852,613),G[12],(924,657),(974,671),(1024,681),(1072,688),G[13],(1163,685),(1216,674),G[20]],
  [G[8],(1129,527),(1141,566),(1140,604),(1127,647),G[13]],
  [G[14],(1276,357),(1246,321),(1210,290),G[18]],
  [G[5],(175,290),(151,320),(155,348),(181,378),G[17],(252,409),(266,443),(265,474),(276,508),G[2]],
  [G[2],(263,522),(233,500),(218,465),(201,430),G[17]],
  [G[17],(177,413),(135,445),(102,485),(86,534),(89,581),(115,625),(142,663),(158,710),(163,759),G[21]],
  [G[21],(229,842),(274,859),(315,873),(360,890),(402,917),(449,938),(501,942),G[0]]
 ],
 'seoul':[
  [S[0],(700,775),S[12]],
  [S[12],(610,786),(570,808),(541,827),(515,842),(486,855),(457,852),(427,840),(407,824),S[14]],
  [S[14],(360,793),(322,758),(286,727),(250,700),(231,681),(280,661),(334,644),(389,623),(444,601),(500,580),(550,562),S[13]],
  [S[13],S[15],(550,518),(510,500),S[21],(421,472),(375,460),(334,444),S[4],(318,396),(357,373),(393,359),S[2]],
  [S[2],(423,321),(420,288),(409,260),S[3]],
  [S[3],(441,247),(485,269),(530,291),(575,319),(622,347),S[17]],
  [S[17],(713,396),(749,417),S[5],S[8],(818,377),(850,345),(882,315),(918,290),S[22],(991,231),(1022,207),S[10]],
  [S[22],(998,256),(1049,279),S[9],(1154,306),(1198,315),S[11]],
  [S[11],(1229,350),(1204,389),(1170,424),(1136,463),(1108,502),S[7]],
  [S[7],(1034,558),(1005,575),S[18],(926,587),S[1],(832,593),(785,581),(744,568),(702,557),(662,550),S[13]],
  [S[1],(919,618),(970,638),(1014,659),(1055,682),(1090,714),S[6],(1176,784),(1205,817),(1231,841),S[16]],
  [S[6],(1180,750),(1220,765),S[19],(1268,757),(1251,717),(1230,681),S[20]]
 ]}
 for cid,anchors in [('gyeongju',G),('seoul',S)]:
  c=next(c for c in d['cities'] if c['id']==cid);graph=collections.defaultdict(list)
  for chain in chains[cid]:
   for a,b in zip(chain,chain[1:]):graph[a].append(b);graph[b].append(a)
  for i,xy in anchors.items():c['nodes'][i].update(hasArrivalPosition=True,arrivalPosition=point(*xy))
  def route(a,b):
   q=[(0,a,[a])];cost={a:0}
   while q:
    dist,n,path=heapq.heappop(q)
    if n==b:return path
    for other in graph[n]:
     nextdist=dist+math.dist(n,other)
     if nextdist<cost.get(other,float('inf')):cost[other]=nextdist;heapq.heappush(q,(nextdist,other,path+[other]))
   raise ValueError((a,b))
  for e in c['edges']:
   start=anchors[int(e['from'][1:])];end=anchors[int(e['to'][1:])];original=next((old for oldCity in baseline['cities'] if oldCity['id']==cid for old in oldCity['edges'] if old['id']==e['id']),None);old=original.get('waypoints',[]) if original else None
   if cid=='seoul' and e['id']=='S6__S14':
    # A separately acknowledged ferry across the river, not walking on water.
    xy=[start,(1110,800),(1060,850),(1000,904),(930,952),(850,990),(750,1000),(650,978),(560,940),(470,895),end];e.update(travel='ferry',requiredFlags=['seoul.ferry_acknowledged'])
   else:xy=route(start,end)
   e.update(waypoints=[point(*v) for v in xy[1:-1]],status='PLAYTEST_MapA_corridor; NEEDS_VISUAL_ACCEPTANCE')
   rows.append(dict(cityId=cid,edge=e['id'],oldWaypoints=old,newWaypoints=e['waypoints'],travel=e['travel'],review='NEEDS_REVIEW; original painting/corridor overlay and runtime must be visually accepted'))
  degree=collections.Counter(v for e in c['edges'] for v in [e['from'],e['to']]);assert max(degree.values())<=3,degree
 write(p,d);write(OUT/'route-authoring.json',rows)
 # Native vector boundaries on an exact copy of the user sheet. No AI redraw,
 # no raster cutout, no overwriting the approved existing combat sprites.
 src=R/'LBC_AnhMoi_BanGiaoCodex_20261015/Characters/Enemies_Roster_Concept.png';dst=R/'Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png'
 if dst.exists():assert dst.read_bytes()==src.read_bytes()
 else:shutil.copy2(src,dst)
 im=Image.open(src);slices=[]
 for name,box in [('energy_core',(9,617,284,271)),('stone_guard',(570,611,299,279)),('shadow_courier',(1162,171,284,295)),('shadow_cat',(876,617,291,268))]:
  slices.append(dict(name=name,x=box[0],y=im.height-box[1]-box[3],width=box[2],height=box[3],outline=outline(im,box),status='TEMPORARY_NATIVE_USER_CONCEPT_MESH; NEEDS_EDGE_VISUAL_REVIEW'))
 write(OUT/'native-slices.json',dict(sheet=str(dst.relative_to(R)).replace('\\','/'),slices=slices))
 print('Authored separate G/S corridors; maxdegree3; native vector outlines only; no source bitmap changed.')
if __name__=='__main__':main()
