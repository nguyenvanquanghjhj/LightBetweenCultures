"""Verify actual chapter journeys and report precise node/content coverage.

No gameplay/save mutation. Run only after the final player and restart runs.
"""
from pathlib import Path
from datetime import datetime,timezone
import csv,hashlib,json,re
from PIL import Image
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Validation/HueJeonjuChapters-20261017'
BASE=ROOT.parent/'Backups/BeforeHueJeonjuChapters-20261016'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def record(p):return dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p))

def main():
    old=read(BASE/'Assets/_Game/Resources/Content/GameplayContent.json')
    data=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
    assert len(data['items'])==37 and len(data['enemies'])==23 and len(data['quests'])==28
    assert sum(len(c['nodes']) for c in data['cities'])==133
    for key in ('items','enemies','resonances'):assert data[key]==old[key],key
    for city in old['cities']:
        now=next(c for c in data['cities'] if c['id']==city['id'])
        assert [n['id'] for n in now['nodes']]==[n['id'] for n in city['nodes']]
        if city['id'] not in ('hue','jeonju'):assert city==now,city['id']
    for id in ('HQ05','HQ06','HQ07'):
        assert next(q for q in old['quests'] if q['id']==id)==next(q for q in data['quests'] if q['id']==id),id
    for area in ('Scenes','Prefabs'):
        for p in (BASE/'Assets/_Game'/area).rglob('*'):
            if p.is_file():assert sha(p)==sha(ROOT/'Assets/_Game'/area/p.relative_to(BASE/'Assets/_Game'/area)),p
    for p in (BASE/'Assets/_Game/Art').rglob('*'):
        if p.suffix.lower() in ('.png','.jpg','.jpeg'):assert sha(p)==sha(ROOT/'Assets/_Game/Art'/p.relative_to(BASE/'Assets/_Game/Art')),p
    assert sha(BASE/'Assets/_Game/Scripts/Core/SaveSystem.cs')==sha(ROOT/'Assets/_Game/Scripts/Core/SaveSystem.cs')
    before=read(BASE/'Docs/Art/native-art-bindings.json')['entries']
    after=read(ROOT/'Docs/Art/native-art-bindings.json')['entries']
    identities={(b['role'],b['id'],b['guid'],b['localId'],b['path']) for b in after}
    for b in before:assert (b['role'],b['id'],b['guid'],b['localId'],b['path']) in identities,(b['role'],b['id'])
    tests={};captures=[];saves=[];route_counts={}
    binary=ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'
    for order in ('HueFirst','JeonjuFirst'):
        folder=OUT/order
        results=folder/'smoke-results.txt';lines=results.read_text(encoding='utf-8-sig').splitlines()
        assert lines[-1]=='ALL CHECKS PASSED',results
        tests[order+'/smoke']=dict(assertions=sum(s.startswith('PASS:') for s in lines),pass_=True)
        save=read(folder/'gameplay-save.json')
        assert save['cityId']=='hoi_an' and save['node']==10
        required=['HQ01','HQ02','HA_FOOD_PREP','HQ03','HQ05','HQ06','HQ07','UQ01','UQ02','UQ03','UQ04','JQ01','JQ02','JQ03','JQ04']
        assert all(q in save['completed'] for q in required),(order,save['completed'])
        for cid in ('hue','jeonju'):
            progress=next(p for p in save['cityProgress'] if p['cityId']==cid)
            assert progress['stage']==4 and len(set(progress['visited']))==22,(order,cid)
        for item in ('reply_letter','ancient_seal','lost_letter','astral_script','hanji_circuit','hanji_pattern_1','hanji_pattern_2','hanji_pattern_3','jeonju_recipe_card','hue_market_note','festival_friendship_postcard'):
            assert save['items'].count(item)==1,(order,item,save['items'].count(item))
        assert save['items'].count('hoi_an_fresh_chili')==0 and save['items'].count('hoi_an_purple_shallot')==1
        for cid in ('hue','jeonju','hoi_an'):
            result=folder/f'restart-{cid}-after-results.txt'
            lines=result.read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED',result
            tests[order+'/'+result.name]=dict(assertions=sum(s.startswith('PASS:') for s in lines),pass_=True)
        for p in folder.glob('*.json'):saves.append(record(p))
        for p in folder.rglob('*.png'):
            assert p.stat().st_mtime>=binary.stat().st_mtime,'Capture predates final player: '+str(p)
            with Image.open(p) as im:assert im.width>=500 and im.height>=500,p
            captures.append(record(p))
    legacy=OUT/'LegacyOpening/restart-jeonju-after-results.txt'
    lines=legacy.read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED'
    tests['LegacyOpening']=dict(assertions=sum(s.startswith('PASS:') for s in lines),pass_=True)
    for p in (OUT/'LegacyOpening').glob('*.png'):
        assert p.stat().st_mtime>=binary.stat().st_mtime,p
        captures.append(record(p))
    for cid in ('hue','jeonju'):
        traces=read(OUT/f'HueFirst/{cid}-chapter-route-traces.json')
        expected=len(next(c for c in data['cities'] if c['id']==cid)['edges'])*2
        assert len(traces)==expected and all(t['arrived'] and len(t['positions'])>1 and t['maxDistance']<.02 for t in traces),cid
        route_counts[cid]=len(traces)
    rows=[]
    for city in data['cities']:
        for node in city['nodes']:
            e=next(e for e in data['events'] if e['cityId']==city['id'] and e['id']==node['eventId'])
            extras=sorted({v['questId'] for v in data['events'] if v['cityId']==city['id'] and v['nodeId']==node['id'] and v.get('additive') and v.get('questId')})
            checkin=next((v['id'] for v in data['checkIns'] if v['cityId']==city['id'] and v['nodeId']==node['id']),'')
            implemented=city['runtimeReady'] and (not city.get('runtimeNodeIds') or node['id'] in city['runtimeNodeIds'])
            if city['id'] in ('hue','jeonju'):
                assert implemented and not e['sourceStatus'].startswith('imported_design'),node['id']
                if e['type']=='puzzle':assert next(p for p in data['puzzles'] if p['id']==e['puzzleId'])['mode']!='unconfigured'
            rows.append(dict(cityId=city['id'],nodeId=node['id'],name=node['name'],kind=node['kind'],eventId=e['id'],eventType=e['type'],
                questId=e.get('questId',''),extraQuestIds=','.join(extras),npcId=e.get('npcId',''),enemyId=e.get('enemyId',''),puzzleId=e.get('puzzleId',''),checkInId=checkin,revealStage=node['revealStage'],gate=node.get('gateCondition',''),
                status='PASS runtime journey' if implemented else 'NOT IMPLEMENTED; data only'))
    doc=ROOT/'Docs/Chapters';doc.mkdir(exist_ok=True)
    with (doc/'six-city-node-coverage.csv').open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    for cid in ('hue','jeonju'):
        city=next(c for c in data['cities'] if c['id']==cid)
        lines=['# '+city['name']+' — toàn bộ 22 node','',
            'Gameplay và lượt ghé thăm được xác nhận bằng Windows player, không phải chỉ parse JSON. Các cutout mới vẫn chờ duyệt art; địa hình còn giới hạn ghi trong báo cáo chung.','',
            '| Node | Địa điểm | Chức năng thực tế | Quest / enemy / puzzle | Chặng hiện | Kết quả |','|---|---|---|---|---|---|']
        for row in (r for r in rows if r['cityId']==cid):
            refs=', '.join(row[k] for k in ('questId','extraQuestIds','enemyId','puzzleId') if row[k])
            role=row['eventType']+(' / check-in' if row['checkInId'] else '')
            lines.append(f"| {row['nodeId']} | {row['name']} | {role} | {refs or 'định hướng / tương tác theo event'} | {row['revealStage']} | PASS |")
        lines+=['','## Quest','', '| ID | Nhận | Hoàn thành | Điều kiện | Thưởng |','|---|---|---|---|---|']
        for q in (q for q in data['quests'] if q['cityId']==cid):
            conditions=[i['itemId']+' × '+str(i['amount']) for i in q['requiredItems']]+q['requiredFlags']
            rewards=[i['itemId']+' × '+str(i['amount']) for i in q['rewardItems']]+[q['completionFlag']]
            lines.append(f"| {q['id']} | {q['giverNode']} | {q.get('completeCityId',cid)} / {q['completeAtNode']} | {'; '.join(conditions)} | {'; '.join(rewards)} |")
        lines+=['','## Key gates','', '| Node | Điều kiện | Mở chặng |','|---|---|---|']
        lines += [f"| {g['nodeId']} | {g['condition']} | {g['unlockStage']} |" for g in city['gates']]
        lines+=['','## Check-in','']+[f"- {p['nodeId']}: {p['title']}, {p['rewardCoins']} xu một lần; minh họa vùng Overview A." for p in data['checkIns'] if p['cityId']==cid]
        lines+=['','## NPC / enemy / puzzle','']
        lines += [f"- {r['nodeId']}: {r['npcId']} — {next(e['actor'] for e in data['events'] if e['cityId']==cid and e['id']==r['eventId'])}." for r in rows if r['cityId']==cid and r['npcId']]
        lines += [f"- {e['id']}: {e['name']}, HP {e['hp']}, ATK {e['attack']}, DEF {e['defense']}; giữ thống kê nguồn/balance proposal." for e in data['enemies'] if e['cityId']==cid]
        lines += [f"- {p['nodeId']}: {p['id']} ({p['mode']}), reward {json.dumps(p['rewardItems'],ensure_ascii=False)}; flags {', '.join(p['flags'])}." for p in data['puzzles'] if p['cityId']==cid]
        (doc/(cid+'-node-coverage.md')).write_text('\n'.join(lines)+'\n',encoding='utf-8')
    manifest=dict(createdUTC=datetime.now(timezone.utc).isoformat(),tests=tests,totalPassAssertions=sum(t['assertions'] for t in tests.values()),
        binary=record(binary),captures=captures,saves=saves,routeDiagnostics=route_counts,
        routeDiagnosticsScope='Isolated edge fixtures after real chapter completion; not proof of terrain walkability or E2E rewards',
        source=[record(p) for p in (ROOT/'Assets/_Game').rglob('*.cs')]+[record(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json'),record(ROOT/'Assets/_Game/Resources/AdventureUIArt.asset')],
        oldIdentitiesPreserved=True,oldImagesUnchanged=True,oldScenesPrefabsUnchanged=True,saveSystemUnchanged=True,
        originalLegacyInput=record(ROOT/'Validation/CrossCityFood-20261016/JeonjuFirst/jeonju-after.json'),
        fullSixCityEndToEnd='NOT COMPLETE: da_lat, gyeongju, seoul have no runtime chapter',android='NOT VERIFIED')
    (OUT/'final-run-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(passAssertions=manifest['totalPassAssertions'],freshPNG=len(captures),routeWalks=route_counts,nodeCoverage=len(rows)),ensure_ascii=False))
if __name__=='__main__':main()
