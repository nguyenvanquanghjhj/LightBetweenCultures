"""Read-only catalog/reference checks for full-game QA."""
from pathlib import Path
import collections, json, math, re
R=Path(__file__).resolve().parents[1]
O=R/'Validation/FullGame-20261025'
def load(n): return json.loads((R/'Assets/_Game/Resources/Content'/n).read_text(encoding='utf-8-sig'))
g=load('GameplayContent.json'); relic=load('CulturalRelics.json'); nav=load('CityNavigation.json')
rows=[]
def check(name, failures):
    failures=list(failures)
    rows.append({'check':name,'status':'FAIL' if failures else 'PASS','failures':failures})
def duplicates(seq): return [k for k,v in collections.Counter(seq).items() if v>1]
cities={c['id']:c for c in g['cities']}
nodes={c['id']:{n['id'] for n in c['nodes']} for c in g['cities']}
check('Six cities and 133 nodes', [] if len(cities)==6 and sum(map(len,nodes.values()))==133 else ['wrong count'])
for group in ['cities','items','quests','puzzles','enemies','checkIns']:
    check('Unique '+group+' IDs',duplicates(x['id'] for x in g[group]))
check('Event IDs unique within city',duplicates((e['cityId'],e['id']) for e in g['events']))
check('Node IDs and spawn references',[c['id'] for c in g['cities'] if len(nodes[c['id']])!=len(c['nodes']) or c['spawnNodeId'] not in nodes[c['id']]])
check('All event/puzzle/check-in city and node references',[str((e.get('cityId'),e.get('nodeId'))) for group in ['events','puzzles','checkIns'] for e in g[group] if e.get('nodeId') not in nodes.get(e.get('cityId'),set())])
check('Canonical edge endpoints',[c['id']+':'+e['id'] for c in g['cities'] for e in c['edges'] if e['from'] not in nodes[c['id']] or e['to'] not in nodes[c['id']]])
check('Finite normalized canonical positions',[c['id']+':'+n['id'] for c in g['cities'] for n in c['nodes'] if not all(isinstance(v,(int,float)) and math.isfinite(v) and 0<=v<=1 for v in n['position'].values())])
for field,group in [('itemId','items'),('questId','quests'),('puzzleId','puzzles'),('enemyId','enemies')]:
    ids={x['id'] for x in g[group]}
    check('Event '+field+' references',[e['cityId']+':'+e['id']+':'+e[field] for e in g['events'] if e.get(field) and e[field] not in ids])
itemids={x['id'] for x in g['items']}
check('Reward and required item references',[group+':'+x['id']+':'+v['itemId'] for group in ['events','quests','puzzles','enemies'] for x in g[group] for field in ['rewardItems','requiredItems','acceptRewardItems'] for v in x.get(field,[]) if v['itemId'] not in itemids])
allnodes=set().union(*nodes.values())
check('Quest giver/completion node references (cross-city hand-ins allowed)',[q['id'] for q in g['quests'] if q.get('giverNode') not in nodes.get(q['cityId'],set()) or q.get('completeAtNode') not in allnodes])
check('Six relics and 24 unique fragments',[] if len(relic['relics'])==6 and len(relic['fragments'])==24 and len({x['id'] for x in relic['fragments']})==24 else ['wrong count'])
fragids={f['id'] for f in relic['fragments']}
check('Four valid fragments per relic',[r['id'] for r in relic['relics'] if len(set(r['fragmentIds']))!=4 or not set(r['fragmentIds'])<=fragids])
check('Relic and fragment city/node references',[r['id'] for r in relic['relics'] if r['restorationNode'] not in nodes.get(r['cityId'],set())]+[f['id'] for f in relic['fragments'] if f['nodeId'] not in nodes.get(f['cityId'],set())])
activities=load('StoryInteractions.json')['entries']; leisure=load('LeisureActivities.json')['entries']
check('22 story activities bound to puzzles',[] if len(activities)==22 and {x['id'] for x in activities}=={x['id'] for x in g['puzzles']} else ['activity/puzzle mismatch'])
check('12 enabled leisure nodes',[x['id'] for x in leisure if not x['enabled'] or x['nodeId'] not in nodes.get(x['cityId'],set())]+([] if len(leisure)==12 else ['wrong count']))
for c in nav['cities']:
    vertices={v['id'] for v in c['vertices']}
    check('Navigation endpoints '+c['cityId'],[e['id'] for e in c['edges'] if e['from'] not in vertices or e['to'] not in vertices])
for node,npc,tea,coins in [('G16','npc_kang_taeho',2,5),('G21','npc_choi_eunsu',2,0),('S12','npc_han_doyun',3,12)]:
    e=next(x for x in g['events'] if x['nodeId']==node and not x.get('additive'))
    check('New NPC identity/reward/one-shot '+node,[] if e['npcId']==npc and e['type']=='npc' and e['oneShot'] and not e['mandatory'] and not e['autoCollect'] and e['rewardCoins']==coins and sum(v['amount'] for v in e['rewardItems'] if v['itemId']=='healing_tea')==tea else [e])
    check('New NPC UTF-8 dialogue '+node,[] if all(not re.search(r'\w\?\w|\?\?|\ufffd',e.get(k,'')) for k in ['text','completedText','actor']) else ['embedded question mark/replacement found'])
result={'scope':'Static data checks, not gameplay assertions or proof of route visual clearance','counts':{k:len(g[k]) for k in ['cities','events','items','quests','puzzles','enemies','checkIns']},'checks':rows}
(O/'content-audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'checks':len(rows),'failed':[x for x in rows if x['status']=='FAIL']}))
