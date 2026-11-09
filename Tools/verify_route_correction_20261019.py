"""Bounded preservation checks for the owner-authorized waypoint patch."""
from pathlib import Path
import hashlib, json, math, re

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / 'Docs/QA/RouteShorteningProposal-20261019'
OUT = ROOT / 'Validation/RouteShorteningProposal-20261019'
PRE = Path('D:/GAMEKKK/Backups/BeforeRouteCorrection-20261019')
read = lambda p: json.loads(p.read_text(encoding='utf-8-sig'))
sha = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
checks = []

def check(ok, message):
    checks.append(dict(result='PASS' if ok else 'FAIL', check=message))
    assert ok, message

# Unity serializes empty platform icon slots during a Windows build. Restore only
# that known generated block after proving every other settings byte is unchanged.
settings = 'ProjectSettings/ProjectSettings.asset'
old = (PRE / settings).read_bytes()
live = (ROOT / settings).read_bytes()
pattern = rb'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
check(re.sub(pattern, b'', old) == re.sub(pattern, b'', live),
      'ProjectSettings identical outside Unity-generated platform icon slots')
if old != live:
    (ROOT / settings).write_bytes(old)

before = read(OUT / 'current-content-snapshot.json')
after = read(ROOT / 'Assets/_Game/Resources/Content/GameplayContent.json')
check({k:v for k,v in before.items() if k!='cities'} ==
      {k:v for k,v in after.items() if k!='cities'},
      'All non-city gameplay data identical: quests, puzzles, encounters, NPCs, items, rewards, check-ins')
changed = []
degrees = {}
for a, b in zip(before['cities'], after['cities']):
    check({k:v for k,v in a.items() if k!='edges'} ==
          {k:v for k,v in b.items() if k!='edges'},
          'Nodes, positions, arrivals, discovery/gates and city metadata exact: '+b['id'])
    check(len(a['edges']) == len(b['edges']), 'Edge count unchanged: '+b['id'])
    neighbors = {n['id']:set() for n in b['nodes']}
    for x, y in zip(a['edges'], b['edges']):
        check({k:v for k,v in x.items() if k!='waypoints'} ==
              {k:v for k,v in y.items() if k!='waypoints'},
              'Edge identity, connectivity, transport and requirements exact: '+b['id']+'/'+y['id'])
        check(all(math.isfinite(p['x']) and math.isfinite(p['y']) and
                  0<=p['x']<=1 and 0<=p['y']<=1 for p in y['waypoints']),
              'Finite normalized waypoint bounds: '+b['id']+'/'+y['id'])
        if x['waypoints'] != y['waypoints']:
            changed.append(b['id']+':'+y['id'])
        neighbors[y['from']].add(y['to']); neighbors[y['to']].add(y['from'])
    degrees[b['id']] = dict(max=max(map(len,neighbors.values())),
                           aboveThree={n:sorted(v) for n,v in neighbors.items() if len(v)>3})
check(len(after['cities'])==6 and sum(len(c['nodes']) for c in after['cities'])==133 and
      sum(len(c['edges']) for c in after['cities'])==155, 'Six maps / 133 nodes / 155 edges retained')
check(len(after['quests'])==29 and len(after['puzzles'])==22 and len(after['checkIns'])==18,
      '29 quests / 22 canonical puzzle IDs / 18 check-ins retained')
rows = read(OUT / 'waypoint-proposals.json')['rows']
expected = {r['City']+':'+r['SourceRouteKey'] for r in rows if r['Changed']}
check(set(changed)==expected and len(changed)==27, 'Exactly the 27 documented existing edge waypoint lists changed')
freeze = read(DOC / 'baseline.json')
allowed = {'Assets/_Game/Resources/Content/GameplayContent.json',
           'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.ApprovedRoutes.cs'}
same, delta = [], []
for entry in freeze['entries']:
    name = entry['path']; p = ROOT / name
    if sha(p)==entry['sha256']:
        same.append(name)
    else:
        check(name in allowed, 'Only declared route data / QA code differs from audit freeze: '+name)
        delta.append(name)
maps = []
for city in read(ROOT/'Validation/RouteMarkupExport-20261018/edge_mapping.json')['cities']:
    name=city['sourcePNG']; entry=next(e for e in freeze['entries'] if e['path']==name)
    check(sha(ROOT/name)==entry['sha256'], 'Canonical map PNG hash exact: '+city['cityId'])
    maps.append(dict(cityId=city['cityId'],path=name,sha256=entry['sha256']))
result=dict(result='PASS',scope='Static preservation and geometry only; terrain is a separate visual review',
            changedEdges=changed, unchangedFrozenFiles=len(same), modifiedFrozenFiles=delta,
            maps=maps, degrees=degrees, checks=checks)
(DOC/'preservation-verification.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('PASS:',len(checks),'checks;',len(changed),'actual waypoint changes;',len(same),'frozen files unchanged')
print('Degrees:',json.dumps(degrees,ensure_ascii=False))
