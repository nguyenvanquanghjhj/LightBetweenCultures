"""Catalog/navigation regression: fresh saves must reach every new objective."""
import json
from collections import deque
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
def load(name): return json.loads((ROOT/'Assets/_Game/Resources/Content'/name).read_text(encoding='utf-8-sig'))
def reachable(start, edges):
    seen={start};queue=deque([start])
    while queue:
        for nxt in edges.get(queue.popleft(),[]):
            if nxt not in seen: seen.add(nxt);queue.append(nxt)
    return seen
def validate():
    cats=load('CatRescue.json')['chapters'];game=load('GameplayContent.json');nav=load('CityNavigation.json')['cities']
    assert len(cats)==6 and len({c['catId'] for c in cats})==6
    assert {c['cityId'] for c in cats}=={c['id'] for c in game['cities']}
    ids=set();modes=set();checks=[]
    for cat in cats:
        city=next(c for c in game['cities'] if c['id']==cat['cityId']);graph=next(c for c in nav if c['cityId']==cat['cityId'])
        def open_access(a): return not a or any(r.get('stage',0)==0 and not r.get('flags') for r in a)
        edges={}
        for e in graph['edges']:
            if e.get('requiredFlags') or not open_access(e.get('access')): continue
            edges.setdefault(e['from'],[]).append(e['to'])
            if e.get('bidirectional',True): edges.setdefault(e['to'],[]).append(e['from'])
        bindings={n['nodeId']:n for n in graph['nodes']}
        visited=reachable(bindings[city['spawnNodeId']]['vertexId'],edges)
        assert len(cat['challenges'])==4 and len(cat['supplies'])==3
        for node_id in [cat['rescueNode']]+[s['nodeId'] for s in cat['supplies']]+[c['nodeId'] for c in cat['challenges']]:
            n=next(n for n in city['nodes'] if n['id']==node_id)
            assert n.get('revealStage',0)==0 and not n.get('requiredFlags'),(cat['cityId'],node_id,'hidden')
            assert bindings[node_id]['vertexId'] in visited and open_access(bindings[node_id].get('access')),(cat['cityId'],node_id,'unreachable')
        for c in cat['challenges']:
            assert c['id'] not in ids;ids.add(c['id']);modes.add(c['mode'])
            assert all(c.get(f) for f in ['instructions','hint','fact'])
            if c['mode'] in ['choice','scenario']: assert 0<=c['answer']<len(c['options'])
            if c['mode'] in ['drag','match','hangul','hanbok','jigsaw']: assert len(c['tokens'])==len(c['targets']) and len(c['tokens'])>=3
            if c['mode']=='maze':
                rows=c['maze'];w=len(rows[0]);assert all(len(r)==w for r in rows)
                maze_edges={}
                for y,row in enumerate(rows):
                    for x,value in enumerate(row):
                        if value=='#':continue
                        maze_edges[y*w+x]=[(y+dy)*w+x+dx for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)] if 0<=x+dx<w and 0<=y+dy<len(rows) and rows[y+dy][x+dx]!='#']
                assert c['exit'] in reachable(c['start'],maze_edges),(c['id'],'unsolvable')
        assert cat['rewardCoins']>0 and cat['postcard'] and cat['sourceUrls']
        checks.append(cat['cityId']+': 4 challenges, 3 supplies, fresh navigation and maze solvability PASS')
    assert {'jigsaw','drag','match','hangul','hanbok','hidden','choice','scenario','maze'}<=modes
    output=ROOT/'Validation/Catventure-20261107';output.mkdir(parents=True,exist_ok=True)
    (output/'catalog-checks.txt').write_text('\n'.join(checks+['24 unique trials; 9 interaction modes; all six fresh-city graphs reachable: PASS'])+'\n',encoding='utf-8')
    print('\n'.join(checks))
if __name__=='__main__':validate()
