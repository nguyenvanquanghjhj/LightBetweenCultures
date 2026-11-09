"""Read-only verification/report for the Dalat chapter checkpoint.

Usage: python Tools/dalat_evidence.py [completed-run-directory ...]
Never modifies gameplay/save data or treats route sampling as terrain proof.
"""
from pathlib import Path
from datetime import datetime,timezone
import hashlib,json,sys
ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT.parent/'Backups/BeforeDaLatChapter-20261017'
OUT=ROOT/'Validation/DaLatChapter-20261017'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def record(p):return dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p))
def main():
 data=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json');old=read(BASE/'Assets/_Game/Resources/Content/GameplayContent.json')
 assert [len(c['nodes']) for c in data['cities']]==[len(c['nodes']) for c in old['cities']]
 assert sum(len(c['nodes']) for c in data['cities'])==133
 for city in old['cities']:
  new=next(v for v in data['cities'] if v['id']==city['id'])
  assert [(n['id'],n['name']) for n in city['nodes']]==[(n['id'],n['name']) for n in new['nodes']]
  if city['id']!='da_lat':assert city==new
 for q in old['quests']:
  if q['cityId']!='da_lat':assert q==next(v for v in data['quests'] if v['id']==q['id'])
 for item in old['items']:assert item==next(v for v in data['items'] if v['id']==item['id'])
 assert len(data['items'])==38 and len(data['quests'])==29 and len(data['enemies'])==23
 for area in ['Scenes','Prefabs']:
  for p in (BASE/'Assets/_Game'/area).rglob('*'):
   if p.is_file():assert sha(p)==sha(ROOT/p.relative_to(BASE)),p
 for p in (BASE/'Assets/_Game/Art').rglob('*'):
  if p.suffix.lower() in ['.png','.jpg','.jpeg']:assert sha(p)==sha(ROOT/p.relative_to(BASE)),p
 assert sha(BASE/'Assets/_Game/Scripts/Core/SaveSystem.cs')==sha(ROOT/'Assets/_Game/Scripts/Core/SaveSystem.cs')
 before=read(BASE/'Docs/Art/native-art-bindings.json')['entries'];after=read(ROOT/'Docs/Art/native-art-bindings.json')['entries']
 bindings={(v['role'],v['id'],v['guid'],v['localId'],v['path']) for v in after}
 for v in before:
  assert (v['role'],v['id'],v['guid'],v['localId'],v['path']) in bindings,(v['role'],v['id'])
 binary=ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll';tests={};captures=[];saves=[];routes={}
 runs=sys.argv[1:] or ['AcceptanceIntegrated','AcceptanceBattleThief']
 for name in runs:
  folder=OUT/name;result=folder/'smoke-results.txt';lines=result.read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED',result
  tests[name]=dict(assertions=sum(x.startswith('PASS:') for x in lines),status='PASS')
  save=read(folder/'dalat-final.json');assert save['cityId']=='da_lat'
  assert all(q in save['completed'] for q in ['HQ01','HQ02','HQ05','HQ06','HQ07','UQ01','JQ03','DQ01','DQ02','DQ03','DQ04','DQ05'])
  progress=next(p for p in save['cityProgress'] if p['cityId']=='da_lat');assert progress['stage']==4 and len(set(progress['visited']))==22
  assert save['items'].count('blue_water_signal')==1 and save['items'].count('da_lat_strawberry')==0
  assert save['items'].count('ancient_seal')==1 and save['items'].count('hanji_circuit')==1
  assert 'da_lat.mist_boss_cleared' in save['flags'] and 'da_lat.beacon_lit' in save['flags'] and 'jeonju.dalat_strawberry_shared' in save['flags']
  assert not any(f in save['flags'] for f in ['gyeongju.beacon_lit','seoul.final_cleared','game.ending_seen'])
  for p in folder.glob('*.png'):
   assert p.stat().st_mtime>=binary.stat().st_mtime,p
   captures.append(record(p))
  for p in folder.glob('*.json'):saves.append(record(p))
  for p in folder.glob('*chapter-route-traces.json'):
   ts=read(p);assert all(t['arrived'] and len(t['positions'])>1 and t['maxDistance']<.02 for t in ts),p
   routes[name+'/'+p.name]=len(ts)
  for p in folder.glob('restart-*.txt'):
   if p.name.endswith('-captures.txt'):continue
   lines=p.read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED',p
   tests[name+'/'+p.name]=dict(assertions=sum(x.startswith('PASS:') for x in lines),status='PASS')
 for folder in sorted(OUT.glob('Restart-*')):
  result=folder/'smoke-results.txt';lines=result.read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED',result
  tests[folder.name]=dict(assertions=sum(x.startswith('PASS:') for x in lines),status='PASS')
  for p in folder.glob('*.png'):
   assert p.stat().st_mtime>=binary.stat().st_mtime,p
   captures.append(record(p))
  for p in folder.glob('*.json'):saves.append(record(p))
 manifest=dict(createdUTC=datetime.now(timezone.utc).isoformat(),tests=tests,totalPassAssertions=sum(t['assertions'] for t in tests.values()),binary=record(binary),captures=captures,saves=saves,isolatedRouteWalks=routes,terrain='FAIL_VISUAL_ACCEPTANCE_KNOWN_ROOF_CROSSINGS_AND_UNCERTAIN_BOARDWALKS',android='NOT_VERIFIED',remainingCities=['gyeongju','seoul'],sourceInputs=[record(ROOT/p) for p in ['Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/Content/AdventurePresentation.json','Assets/_Game/Resources/AdventureUIArt.asset']])
 (OUT/'final-run-manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf-8')
 city=next(v for v in data['cities'] if v['id']=='da_lat');lines=['# Đà Lạt D0–D21 — node coverage','', 'Runtime Windows: xem final-run-manifest và báo cáo root. Terrain/art chưa final; Gyeongju/Seoul chưa triển khai.','', '| Node | Tên / vai trò | Event thật | NPC/enemy | Quest | Item / flag | Khu / gate | Giới hạn |','|---|---|---|---|---|---|---|---|']
 for n in city['nodes']:
  es=[e for e in data['events'] if e['cityId']=='da_lat' and e['nodeId']==n['id']];qs=[q['id'] for q in data['quests'] if q['giverNode']==n['id'] or q['completeAtNode']==n['id']]
  fields=[n['id'],n['name']+' / '+n['kind'],', '.join(e['id']+'('+e['type']+')' for e in es),', '.join(e.get('npcId') or e.get('enemyId') or 'Lumi / môi trường' for e in es),', '.join(qs),', '.join(i['itemId']+'×'+str(i['amount']) for e in es for i in e.get('rewardItems',[]))+'; '+', '.join(f for e in es for f in e.get('flags',[])),str(n['revealStage'])+'; '+n['gateCondition'],'Combat riêng D6 chưa test' if n['id']=='D6' else 'Đã ghé bằng input; terrain còn review']
  lines.append('| '+' | '.join(fields)+' |')
 lines+=['','## Quest','', '| ID | Nhận → kết thúc | Yêu cầu | Thưởng / cờ |','|---|---|---|---|']
 for q in data['quests']:
  if q['cityId']!='da_lat':continue
  lines.append('| '+' | '.join([q['id'],q['giverNode']+' → '+q['completeAtNode'],', '.join(q.get('requiredFlags',[]))+'; '+', '.join(i['itemId']+'×'+str(i['amount']) for i in q.get('requiredItems',[])),str(q.get('rewardCoins',0))+' xu; '+q['completionFlag']])+' |')
 (ROOT/'Docs/Chapters/dalat-node-coverage.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
 print(json.dumps(dict(status='PASS',assertions=manifest['totalPassAssertions'],freshPNGs=len(captures),routeWalks=routes),ensure_ascii=False))
if __name__=='__main__':main()
