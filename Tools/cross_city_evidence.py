"""Verify delivered cross-city journeys, immutable content/art and fresh captures."""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,re
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT.parent/'Backups/BeforeCrossCityFood-20261016'
OUT=ROOT/'Validation/CrossCityFood-20261016'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def record(p):return dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p),modifiedUTC=datetime.fromtimestamp(p.stat().st_mtime,timezone.utc).isoformat())

def main():
    old=load(BASE/'Assets_Game/Resources/Content/GameplayContent.json');new=load(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
    assert len(new['items'])==37 and len(new['quests'])==28 and sum(len(c['nodes']) for c in new['cities'])==133
    for key in ['items','enemies','puzzles','resonances','checkIns']:assert old[key]==new[key],key
    for a,b in zip(old['quests'],new['quests']):
        if a['id'] not in ['HQ05','HQ06','HQ07']:assert a==b,a['id']
        else:
            for key in ['id','title','giverNode','completeAtNode','requiredItems','rewardItems','rewardCoins','consumeItems','completionFlag','offer','reminder','thanks','acceptFlags','requiredFlags']:assert a[key]==b[key],(a['id'],key)
    for a,b in zip(old['cities'],new['cities']):
        assert [n['id'] for n in a['nodes']]==[n['id'] for n in b['nodes']]
        assert a['gates']==b['gates'] and a['unlockFlags']==b['unlockFlags']
        assert [(e['from'],e['to'],e['requiredFlags']) for e in a['edges']]==[(e['from'],e['to'],e['requiredFlags']) for e in b['edges']]
        if a['id']=='hoi_an':
            expected=dict(a,mapTitle='HỘI AN · PHỐ ĐÈN');assert expected==b,'Hoi An gameplay unchanged'
        elif a['id'] in ['hue','jeonju']:
            assert b['runtimeReady'] and not b['routesReviewed'] and len(b['runtimeNodeIds'])==2
        else:assert a==b,a['id']
    allowed={'Resources/AdventureUIArt.asset','Resources/Content/GameplayContent.json','Resources/Content/AdventurePresentation.json',
        'Scripts/Content/ContentDefinitions.cs','Scripts/Core/GameData.cs','Scripts/Core/GameBootstrap.cs','Scripts/Core/QuestManager.cs',
        'Scripts/Content/NodeEventManager.cs','Scripts/Maps/MapDiscoveryManager.cs','Scripts/Maps/CityMap.cs','Scripts/Maps/WorldMap.cs',
        'Scripts/UI/GameplayPanels.cs','Scripts/Editor/AdventureUISetup.cs','Scripts/Editor/ArtLibrarySetup.cs',
        'Scripts/Validation/GameplaySmokeRunner.cs',
        'Art/Characters/NPC/Shared/NPC_TransparentAtlas.png.meta','Art/Maps/Hue/Map_Hue_DestinationPreview_A.png.meta',
        'Art/Maps/Jeonju/Map_Jeonju_DestinationPreview_A.png.meta'}
    preserved=[];changed=[]
    for a in (BASE/'Assets_Game').rglob('*'):
        if not a.is_file():continue
        rel=a.relative_to(BASE/'Assets_Game').as_posix();b=ROOT/'Assets/_Game'/rel;assert b.exists(),rel
        if sha(a)!=sha(b):assert rel in allowed,rel;changed.append(rel)
        else:preserved.append(rel)
    # Every original bitmap is unchanged; no duplicate derivative PNGs are introduced.
    original={p.relative_to(BASE/'Assets_Game/Art').as_posix():sha(p) for p in (BASE/'Assets_Game/Art').rglob('*') if p.suffix.lower() in ['.png','.jpg','.jpeg']}
    current={p.relative_to(ROOT/'Assets/_Game/Art').as_posix():sha(p) for p in (ROOT/'Assets/_Game/Art').rglob('*') if p.suffix.lower() in ['.png','.jpg','.jpeg']}
    assert original==current
    for folder in ['LBC_AnhMoi_BanGiaoCodex_20261015','LBC_Gameplay_6Cities_Detailed_v1','LBC_HoiAn_StoryExpansion_v1','LightBetweenCultures_DesignKit_v1']:
        assert (ROOT/folder).exists()
    before=load(BASE/'Docs/Art/native-art-bindings.json')['entries'];after=load(ROOT/'Docs/Art/native-art-bindings.json')['entries']
    bindings={(e['role'],e['id']):e for e in after}
    references={(e['role'],e['id'],e['guid'],e['localId'],e['path']) for e in after}
    for e in before:
        assert (e['role'],e['id'],e['guid'],e['localId'],e['path']) in references,(e['role'],e['id'])
    for id in ['npc_huong','npc_soyeon']:
        assert bindings['npc',id]['canonical'] is False and 'NOT_CANONICAL' in bindings['npc',id]['status']
    assert bindings['npc','npc_huong']['localId']!=bindings['npc','npc_soyeon']['localId']
    binary=ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
    tests={};captures=[];saves=[]
    for folder in ['HueFirst','JeonjuFirst']:
        p=OUT/folder
        names=['smoke-results.txt']+[f'restart-{city}-after-results.txt' for city in ['hue','jeonju','hoi_an']]
        if folder=='HueFirst':names+=['restart-results.txt']
        for name in names:
            lines=(p/name).read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED',(folder,name)
            tests[folder+'/'+name]=dict(passed=True,assertions=sum(l.startswith('PASS:') for l in lines))
        for file in p.rglob('*.png'):
            with Image.open(file) as im:assert im.width>=500 and im.height>=500,file
            assert file.stat().st_mtime>=binary.stat().st_mtime,'Capture predates the final compiled player: '+str(file)
            captures.append(record(file))
        final=load(p/'gameplay-save.json')
        assert final['cityId']=='hoi_an' and final['node']==10 and all(q in final['completed'] for q in ['HQ01','HQ02','HA_FOOD_PREP','HQ05','HQ06','HQ07'])
        assert all(final['items'].count(i)==1 for i in ['jeonju_recipe_card','hue_market_note','festival_friendship_postcard'])
        assert final['items'].count('hoi_an_fresh_chili')==0 and final['items'].count('hoi_an_purple_shallot')==1
        assert all(next(x for x in final['cityProgress'] if x['cityId']==c)['stage']==0 for c in ['hue','jeonju'])
        saves += [record(f) for f in p.glob('*.json')]
    # Actual old checkpoint was also used by the initial successful compatibility journey.
    legacy=OUT/'Attempt01HueFirst/smoke-results.txt';assert legacy.exists() and legacy.read_text(encoding='utf-8-sig').strip().endswith('ALL CHECKS PASSED')
    binary=ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
    logs=[p for p in (ROOT/'Logs').glob('cross-city-*.log') if 'attempt01' not in p.name]
    for p in logs:
        text=p.read_text(encoding='utf-8-sig',errors='replace');assert 'error CS' not in text and 'GAMEPLAY FAIL' not in text and 'NullReferenceException' not in text,p
    assert 'Windows build: Succeeded' in (ROOT/'Logs/cross-city-build.log').read_text(encoding='utf-8-sig')
    manifest=dict(createdUTC=datetime.now(timezone.utc).isoformat(),tests=tests,protectedFiles=preserved,changed=changed,
        originalArtImagesUnchanged=len(original),bitmapAssetsAdded=0,originalBindingsPreserved=True,
        oldSaveCompatibility=record(legacy),binary=record(binary),source=[record(p) for p in (ROOT/'Assets/_Game').rglob('*.cs')]+[record(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')],
        captures=captures,saves=saves,logs=[record(p) for p in logs],
        scope={'huePlayable':['U0','U1'],'jeonjuPlayable':['J0','J15'],'localChaptersComplete':False,'androidPhysicalTouch':'NOT VERIFIED'})
    (OUT/'final-run-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(tests=tests,preservedFiles=len(preserved),freshPNGs=len(captures),originalArt=len(original)),ensure_ascii=False))

if __name__=='__main__':main()
