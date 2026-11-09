"""Static checks of six-city Catventure content & gameplay hotfix (not Unity QA)."""
import json
from collections import deque
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'Assets/_Game/Resources/Content'

def load(p):return json.loads((SRC/p).read_text(encoding='utf-8-sig'))
def code(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def reach(start,adj):
    seen={start};q=deque([start])
    while q:
        for nxt in adj.get(q.popleft(),[]):
            if nxt not in seen:seen.add(nxt);q.append(nxt)
    return seen

def main():
    cat=load('CatRescue.json')['chapters']
    source=load('GameplayContent.json')
    navigation=load('CityNavigation.json')['cities']
    assert len(cat)==6 and {c['cityId'] for c in cat}=={c['id'] for c in source['cities']}
    assert sum(len(c['challenges']) for c in cat)==24
    assert len({t['id'] for c in cat for t in c['challenges']})==24
    all_modes=set()
    for chapter in cat:
        city=next(x for x in source['cities'] if x['id']==chapter['cityId'])
        graph=next(x for x in navigation if x['cityId']==chapter['cityId'])
        defs={n['id']:n for n in city['nodes']}
        bindings={n['nodeId']:n for n in graph['nodes']}
        assert len(chapter['supplies'])==3 and len(chapter['challenges'])==4
        assert chapter['rewardCoins']==8 and chapter['catLine'] and chapter['catTrait']
        open_gate=lambda gates: not gates or any(g.get('stage',0)==0 and not g.get('flags') for g in gates)
        adj={}
        for edge in graph['edges']:
            if edge.get('requiredFlags') or not open_gate(edge.get('access')):continue
            adj.setdefault(edge['from'],[]).append(edge['to'])
            if edge.get('bidirectional',True):adj.setdefault(edge['to'],[]).append(edge['from'])
        start=bindings[city['spawnNodeId']]['vertexId'];accessible=reach(start,adj)
        objective_nodes=[chapter['rescueNode']]+[s['nodeId'] for s in chapter['supplies']]+[q['nodeId'] for q in chapter['challenges']]
        for n in objective_nodes:
            assert n in defs and n in bindings,(chapter['cityId'],n,'missing')
            assert defs[n].get('revealStage',0)==0 and not defs[n].get('requiredFlags'),(chapter['cityId'],n,'hidden stage0')
            assert open_gate(bindings[n].get('access')) and bindings[n]['vertexId'] in accessible,(chapter['cityId'],n,'unreachable')
        for trial in chapter['challenges']:
            assert trial['hint'] and trial['fact'] and trial['instructions']
            all_modes.add(trial['mode'])
            if trial['mode'] in ('match','jigsaw','drag','hanbok','hangul'):
                assert len(trial['tokens'])==len(trial['targets']) and len(trial['tokens'])>=3
            if trial['mode'] in ('choice','scenario'):
                assert 0<=trial['answer']<len(trial['options'])
            if trial['mode']=='maze':
                maze=trial['maze'];w=len(maze[0]);h=len(maze)
                assert all(len(row)==w for row in maze)
                edges={}
                for y,row in enumerate(maze):
                    for x,v in enumerate(row):
                        if v=='#':continue
                        u=y*w+x;edges[u]=[]
                        for dx,dy in ((1,0),(-1,0),(0,1),(0,-1)):
                            xx=x+dx;yy=y+dy
                            if 0<=xx<w and 0<=yy<h and maze[yy][xx]!='#':edges[u].append(yy*w+xx)
                assert trial['exit'] in reach(trial['start'],edges),trial['id']
        print('PASS: %-10s: 4 trials, 3 supplies, all objectives stage-0 reachable' % chapter['cityId'])
    assert len(all_modes)==9,all_modes
    encounters=[e for e in source['events'] if e.get('type')=='encounter']
    mirror=[e for e in encounters if e.get('alternateSteps',0)>0]
    assert len(mirror)==1 and mirror[0]['id']=='enemy_glass_thief' and mirror[0]['alternateSteps']==2
    unguarded=[e['id'] for e in encounters if e.get('alternateText') and not (e.get('alternateFlag') or e.get('alternateItemId') or e.get('alternateSteps'))]
    assert not unguarded,('Unexpected unguarded encounter alternate',unguarded)
    menu=code('Assets/_Game/Scripts/Maps/MainMenu.cs')
    events=code('Assets/_Game/Scripts/Content/NodeEventManager.cs')
    weather=code('Assets/_Game/Scripts/UI/CatWeatherGraphic.cs')
    catmap=code('Assets/_Game/Scripts/Maps/CityMap.CatRescue.cs')
    minimap=code('Assets/_Game/Scripts/UI/CatMiniMapGraphic.cs')
    smoke=code('Assets/_Game/Scripts/Validation/GameplaySmokeRunner.DaLat.cs')
    build=code('Assets/_Game/Scripts/Editor/CatRescueBuild.cs')
    assert 'CatventureStart").GetComponent<UnityEngine.UI.Button>().interactable=false' in menu
    assert 'new Vector2(.15f,.52f),new Vector2(.85f,.56f)' in menu
    assert '"reflect:up"' in events and '"reflect:down"' in events and '"reflect:straight"' in events
    assert 'var required=stage==0?"reflect:up":"reflect:down";' in events
    assert 'yield return ChapterChoice("reflect:straight");' in smoke
    assert 'yield return ChapterChoice("reflect:up");' in smoke
    assert 'yield return ChapterChoice("reflect:down");' in smoke
    assert 'Time.unscaledTime+.05f' not in weather and 'OnPopulateMesh' in weather
    assert 'new Vector2(1.2f,15)' not in weather and 'weather=="rain"?' not in weather
    assert 'CityRouteGeometry.Arrival(game.Data.nodes[i])' in minimap
    assert 'var route=game.Data.FindRoute' in minimap
    assert 'ĐI ĐẾN MỤC TIÊU' in catmap and 'StartCoroutine(CatTravel(target))' in catmap
    assert '1.1.3-catventure-20261109' in build and 'bundleVersionCode=23' in build
    print('PASS: 9 interaction modes; only D7 uses preparation; guarded alternate encounters')
    print('PASS: mirror wrong-angle protection, save corruption guard, HUD routing, no falling weather needles, new version')
    print('IMPORTANT: Unity Player, Android and real user gameplay NOT TESTED by this static tool.')

if __name__=='__main__':main()
