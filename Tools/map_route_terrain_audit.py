"""Bounded engineering plots and graph diagnostics. Never writes art or game data."""
import argparse, collections, hashlib, json, math
from pathlib import Path
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
import matplotlib.patheffects as pe

ROOT=Path(__file__).resolve().parents[1]
ART={'hoi_an':'HoiAn/Map_HoiAn_Overview_A.png','hue':'Hue/Map_Hue_DestinationPreview_A.png',
     'da_lat':'DaLat/Map_DaLat_DestinationPreview_A.png','jeonju':'Jeonju/Map_Jeonju_DestinationPreview_A.png',
     'gyeongju':'Gyeongju/Map_Gyeongju_Overview_A.png','seoul':'Seoul/Map_Seoul_Overview_A.png'}
def pixel(p):return (p['x']*1448,(1-p['y'])*1086)
def path(city,edge):
    nodes={n['id']:n for n in city['nodes']}
    a,b=nodes[edge['from']],nodes[edge['to']]
    return [pixel(a.get('arrivalPosition',a['position']))]+[pixel(p) for p in edge['waypoints']]+[pixel(b.get('arrivalPosition',b['position']))]
def dist(a,b):return math.dist(a,b)
def cross(a,b):return a[0]*b[1]-a[1]*b[0]
def sub(a,b):return (a[0]-b[0],a[1]-b[1])
def intersection(a,b,c,d):
    u,v,w=sub(b,a),sub(d,c),sub(c,a);den=cross(u,v)
    if abs(den)<1e-7:return None
    t,s=cross(w,v)/den,cross(w,u)/den
    if .03<t<.97 and .03<s<.97:return [round(a[0]+t*u[0],2),round(a[1]+t*u[1],2)]
def distance(p,a,b):
    u=sub(b,a);den=u[0]**2+u[1]**2
    t=max(0,min(1,((p[0]-a[0])*u[0]+(p[1]-a[1])*u[1])/den)) if den else 0
    return dist(p,(a[0]+t*u[0],a[1]+t*u[1]))
def samples(points,step=12):
    for a,b in zip(points,points[1:]):
        count=max(1,math.ceil(dist(a,b)/step))
        for i in range(count):yield (a[0]+(b[0]-a[0])*i/count,a[1]+(b[1]-a[1])*i/count)
def audit(city):
    adj={n['id']:set() for n in city['nodes']};edges=city['edges'];paths=[path(city,e) for e in edges]
    for e in edges:adj[e['from']].add(e['to']);adj[e['to']].add(e['from'])
    seen={city['spawnNodeId']};q=list(seen)
    while q:
        for n in adj[q.pop()]-seen:seen.add(n);q.append(n)
    crossings=[];overlaps=[]
    for i,a in enumerate(paths):
        for j,b in enumerate(paths[i+1:],i+1):
            hits=[]
            for x,y in zip(a,a[1:]):
                for z,w in zip(b,b[1:]):
                    p=intersection(x,y,z,w)
                    if p and min(dist(p,k) for k in [a[0],a[-1],b[0],b[-1]])>22 and not any(dist(p,k)<12 for k in hits):hits.append(p)
            if hits:crossings.append(dict(edges=[edges[i]['id'],edges[j]['id']],pixels=hits,status='CANDIDATE_NOT_AUTOMATIC_TERRAIN_FAILURE'))
            candidates=[p for p in samples(a) if min(dist(p,k) for k in [a[0],a[-1],b[0],b[-1]])>25]
            close=[p for p in candidates if min(distance(p,x,y) for x,y in zip(b,b[1:]))<8]
            if len(close)>=5 and len(close)/max(1,len(candidates))>.20:overlaps.append(dict(edges=[edges[i]['id'],edges[j]['id']],nearSamples=len(close),ratio=round(len(close)/len(candidates),3),status='SHARED_CORRIDOR_OR_NEAR_DUPLICATE_REVIEW'))
    return dict(cityId=city['id'],nodes=len(adj),edges=len(edges),maxNeighbors=max(map(len,adj.values())),over3=[dict(nodeId=k,count=len(v),neighbors=sorted(v)) for k,v in adj.items() if len(v)>3],connectedIgnoringGates=len(seen)==len(adj),crossings=crossings,overlaps=overlaps)
def plot(city,output,previous=None):
    fig=plt.figure(figsize=(14.48,10.86),dpi=100);ax=fig.add_axes([0,0,1,1]);ax.imshow(plt.imread(ROOT/'Assets/_Game/Art/Maps'/ART[city['id']]));ax.set_xlim(0,1448);ax.set_ylim(1086,0);ax.axis('off')
    if previous:
        for e in previous['edges']:
            pts=path(previous,e);ax.plot(*zip(*pts),color='#ee3f4b',linewidth=1.4,alpha=.6)
    for i,e in enumerate(city['edges']):
        pts=path(city,e);color='#32ffad' if previous else plt.cm.tab20(i%20)
        ax.plot(*zip(*pts),color=color,linewidth=1.7,alpha=.9)
        p=pts[len(pts)//2];t=ax.text(*p,e['id'].replace('__','-'),fontsize=7,color='white');t.set_path_effects([pe.withStroke(linewidth=2,foreground='black')])
    for n in city['nodes']:
        x,y=pixel(n['position']);ax.plot(x,y,'o',ms=6,mfc='white',mec='black');t=ax.text(x+6,y-6,n['id'],fontsize=10,color='white',weight='bold');t.set_path_effects([pe.withStroke(linewidth=2,foreground='black')])
        if n.get('hasArrivalPosition'):x,y=pixel(n['arrivalPosition']);ax.plot(x,y,'x',ms=5,color='#00ffff')
    fig.savefig(output,dpi=100);plt.close(fig)
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--before',action='store_true');args=parser.parse_args()
    out=ROOT/'Validation/RouteRedesign35-20261018/Analysis';out.mkdir(parents=True,exist_ok=True)
    content=ROOT/'Assets/_Game/Resources/Content/GameplayContent.json'
    before=Path('D:/GAMEKKK/Backups/BeforeMapRoute35-20261018/Assets/_Game/Resources/Content/GameplayContent.json')
    old=json.loads(before.read_text('utf-8-sig'));data=old if args.before else json.loads(content.read_text('utf-8-sig'))
    results=[]
    for c in data['cities']:
        results.append(audit(c));plot(c,out/(c['id']+('-before' if args.before else '-comparison')+'.png'),None if args.before else next(x for x in old['cities'] if x['id']==c['id']))
    (out/('graph-before.json' if args.before else 'graph-after.json')).write_text(json.dumps(results,indent=2),'utf-8')
    print(json.dumps([dict(city=r['cityId'],max=r['maxNeighbors'],over3=len(r['over3']),crossingPairs=len(r['crossings']),overlapPairs=len(r['overlaps'])) for r in results]))
if __name__=='__main__':main()
