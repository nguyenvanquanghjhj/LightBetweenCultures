"""Safe static preflight for the Catventure patch. Does not run Unity."""
import json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
def source(path): return (ROOT/path).read_text(encoding='utf-8-sig')

def main():
    game=json.loads(source('Assets/_Game/Resources/Content/CatRescue.json'))
    chapters=game['chapters']
    assert len(chapters)==6, 'Expected six city chapters'
    assert len({c['cityId'] for c in chapters})==6
    assert len({c['catId'] for c in chapters})==6
    assert sum(len(c['challenges']) for c in chapters)==24
    assert sum(len(c['supplies']) for c in chapters)==18
    assert len({t['id'] for c in chapters for t in c['challenges']})==24
    for c in chapters:
        assert len(c['challenges'])==4 and len(c['supplies'])==3
        assert c['catLine'] and c['catTrait'] and c['badge'] and c['postcard']
        assert c['rewardCoins']==8
        assert len({s['id'] for s in c['supplies']})==3
        for t in c['challenges']:
            assert t['instructions'] and t['hint'] and t['fact'], t['id']
            if t['mode'] in ('jigsaw','match','drag','hanbok','hangul'):
                assert len(t['tokens']) == len(t['targets']) and len(t['tokens'])>=3, t['id']
            if t['mode'] in ('choice','scenario'):
                assert 0<=t['answer']<len(t['options']), t['id']
            if t['mode']=='maze':
                maze=t['maze'];width=len(maze[0]);height=len(maze)
                assert all(len(row)==width for row in maze)
                s=t['start'];e=t['exit'];todo=[s];visited={s}
                while todo:
                    u=todo.pop(0)
                    for v in (u-width,u+width,u-1,u+1):
                        if not (0<=v<width*height) or v in visited: continue
                        if abs(u//width-v//width)+abs(u%width-v%width)!=1: continue
                        if maze[v//width][v%width]=='#': continue
                        todo.append(v);visited.add(v)
                assert e in visited, t['id']
    g=next(c for c in chapters if c['cityId']=='gyeongju')
    assert next(t for t in g['challenges'] if t['id']=='gyeongju_buddhist')['mode']=='match'
    m=source('Assets/_Game/Scripts/Maps/MainMenu.cs')
    b=source('Assets/_Game/Scripts/Core/GameBootstrap.cs')
    city=source('Assets/_Game/Scripts/Maps/CityMap.cs')
    cat=source('Assets/_Game/Scripts/Maps/CityMap.CatRescue.cs')
    assert 'CatventureStart' in m and 'CATVENTURE · BẢN 1.1.3' in m
    assert 'OpenCatRescueOnNextCity=true' in m and 'OpenCatRescueOnNextCity { get; set; }' in b
    assert 'OpenCatRescueOnNextCity=false' in city and 'OpenCatRescue();' in city
    assert 'catMapMarker.gameObject.SetActive(p?.rescued!=true)' in cat
    assert 'catWeather.gameObject.SetActive(p?.rescued!=true&&!dialogue)' in cat
    assert 'rewardGrantIds.Contains(key)' in source('Assets/_Game/Scripts/Content/CatRescue.cs')
    build=source('Assets/_Game/Scripts/Editor/CatRescueBuild.cs')
    assert '1.1.3-catventure-20261109' in build and 'bundleVersionCode=23' in build
    print('PASS: 6 chapters, 24 trials, 18 supplies, maze paths, entry CTA, cat marker, reward guard and new build version.')
    print('NATIVE UNITY/ANDROID: NOT TESTED. These must be tested after building.')

if __name__=='__main__': main()
