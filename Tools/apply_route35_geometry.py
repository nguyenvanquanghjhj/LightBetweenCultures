"""One-time Phase3.5 authoring, guarded against overwriting later user changes."""
import argparse,copy,hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/Maps/RouteRedesign35-20261018'
SOURCE='Assets/_Game/Resources/Content/GameplayContent.json'
BASE=Path('D:/GAMEKKK/Backups/BeforeMapRoute35-20261018')
def xy(p):return dict(x=round(p[0]/1448,9),y=round(1-p[1]/1086,9))
def main():
    parser=argparse.ArgumentParser();parser.add_argument('--apply',action='store_true');args=parser.parse_args()
    old=json.loads((BASE/SOURCE).read_text('utf-8-sig'));d=copy.deepcopy(old);cities={c['id']:c for c in d['cities']};recipe=json.loads((OUT/'terrain-edits.json').read_text('utf-8'))
    for a in recipe['arrivals']:
        n=next(n for n in cities[a['cityId']]['nodes'] if n['id']==a['nodeId']);n['hasArrivalPosition']=True;n['arrivalPosition']=xy(a['pixel'])
    for a in recipe['edges']:
        e=next(e for e in cities[a['cityId']]['edges'] if e['id']==a['edgeId']);e['waypoints']=[xy(p) for p in a['pixels']];e['status']='terrain35_authored_corridor; runtime_and_user_visual_acceptance_pending'
    for a in recipe['coveredPassages']:
        cities[a['cityId']].setdefault('coveredWalkways',[]).append(dict(id=a['id'],polygon=[xy(p) for p in a['pixels']]))
    for c in d['cities']:
        c['routeStyle']=dict(groundColor={'hoi_an':'#E7CA91','hue':'#E5CC9A','da_lat':'#E1D4AE','jeonju':'#E1C99E','gyeongju':'#E2D2AA','seoul':'#ECD5B0'}[c['id']],gapPixels=29,selectedGapPixels=24,radiusPixels=1.5,selectedRadiusPixels=2.15,idleAlpha=.45,selectedAlpha=.94,endPaddingPixels=19,adjacentOnly=True,maxDots=1024)
    assert all(old[k]==d[k] for k in old if k!='cities')
    for a,b in zip(old['cities'],d['cities']):
        assert len(a['nodes'])==len(b['nodes']) and len(a['edges'])==len(b['edges'])
        assert all({k:v for k,v in x.items() if k not in ['hasArrivalPosition','arrivalPosition']}=={k:v for k,v in y.items() if k not in ['hasArrivalPosition','arrivalPosition']} for x,y in zip(a['nodes'],b['nodes']))
        assert all({k:v for k,v in x.items() if k not in ['waypoints','status']}=={k:v for k,v in y.items() if k not in ['waypoints','status']} for x,y in zip(a['edges'],b['edges']))
        if a['id']=='hoi_an':
            assert a.get('coveredWalkways')==b.get('coveredWalkways')
            for x,y in zip(a['edges'],b['edges']):
                if x['id'] in ['H18__H5','H5__H6','H6__H15']:assert x==y
    data=json.dumps(d,ensure_ascii=False,indent=2)+'\n';(OUT/'proposed-gameplay.json').write_text(data,'utf-8')
    if args.apply:
        assert hashlib.sha256((ROOT/SOURCE).read_bytes()).digest()==hashlib.sha256((BASE/SOURCE).read_bytes()).digest(),'Production changed since snapshot; review instead of overwriting.'
        (ROOT/SOURCE).write_text(data,'utf-8')
    print(('APPLIED' if args.apply else 'PROPOSED')+': topology, canonical nodes,29 quests22 puzzles, unlocks and relics preserved.')
if __name__=='__main__':main()
