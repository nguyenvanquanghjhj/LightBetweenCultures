"""Bounded guards for the approved waypoint-only patch."""
import hashlib,json,math,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1];OUT=ROOT/'Docs/Maps/AppliedRouteReferences-20261019'
b=json.loads((OUT/'baseline.json').read_text(encoding='utf-8-sig'));before=Path(b['before'])
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
checks=[]
def check(ok,text):
    checks.append(dict(result='PASS' if ok else 'FAIL',check=text))
    assert ok,text
# Narrowly remove only Unity's generated empty platform-icon serialization.
settings='ProjectSettings/ProjectSettings.asset';old=(before/settings).read_bytes();new=(ROOT/settings).read_bytes()
pat=rb'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
check(re.sub(pat,b'',old)==re.sub(pat,b'',new),'ProjectSettings unchanged outside generated platform-icon slots')
if old!=new:(ROOT/settings).write_bytes(old)
for name,expected in b['protected'].items():check(sha(ROOT/name)==expected.lower(),'Protected exact: '+name)
check(sha(Path(b['baseCheckpoint'])/'CHECKPOINT_MANIFEST.json')==b['baseManifestSHA256'],'Phase5 latest base manifest exact')
source='Assets/_Game/Resources/Content/GameplayContent.json';old,new=load(before/source),load(ROOT/source)
check({k:v for k,v in old.items() if k!='cities'}=={k:v for k,v in new.items() if k!='cities'},'All non-city content exact, including quests/NPC/events/boss/relic hooks/check-ins')
changed=[]
for a,c in zip(old['cities'],new['cities']):
    check({k:v for k,v in a.items() if k!='edges'}=={k:v for k,v in c.items() if k!='edges'},'Canonical nodes/arrival/gates/discovery/lighting/map configuration exact: '+c['id'])
    check(len(a['edges'])==len(c['edges']),'Edge count unchanged: '+c['id'])
    for x,y in zip(a['edges'],c['edges']):
        check({k:v for k,v in x.items() if k!='waypoints'}=={k:v for k,v in y.items() if k!='waypoints'},'Graph/conditions/travel/type/id exact: '+c['id']+'/'+x['id'])
        check(all(math.isfinite(p['x']) and math.isfinite(p['y']) and 0<=p['x']<=1 and 0<=p['y']<=1 for p in y['waypoints']),'Finite in-bounds waypoints: '+c['id']+'/'+x['id'])
        if x['waypoints']!=y['waypoints']:changed.append(c['id']+':'+x['id'])
check(len(new['cities'])==6 and sum(len(c['nodes']) for c in new['cities'])==133 and sum(len(c['edges']) for c in new['cities'])==155,'6 maps / 133 nodes / 155 edges exact')
check(len(new['quests'])==29 and len(new['puzzles'])==22 and len(new['checkIns'])==18,'29 quests / 22 puzzles / 18 check-ins exact')
check(len(changed)==149,'149 existing edge geometries actually changed')
export=load(ROOT/'Validation/RouteMarkupExport-20261018/edge_mapping.json')
legacy=load(ROOT/'Assets/_Game/Resources/HoiAnData.json')
maps=[]
for c in export['cities']:
    current=next(city for city in new['cities'] if city['id']==c['cityId'])
    anchors=True
    for n in c['nodes']:
        record=next(node for node in current['nodes'] if node['id']==n['id'])
        position=record['position']
        if c['cityId']=='hoi_an':
            position=next((node['position'] for node in legacy['nodes'] if node['id']==n['id']),position)
        arrival=record['arrivalPosition'] if record.get('hasArrivalPosition') else position
        anchors &= position==n['positionUnity'] and arrival==n['arrivalUnity']
    check(anchors,'Frozen EdgeID export anchors match current GameData.Load inputs: '+c['cityId'])
    name=c['sourcePNG'];check(sha(ROOT/name)==b['protected'][name],'Canonical map PNG byte exact: '+c['cityId'])
    maps.append(dict(cityId=c['cityId'],path=name,sha256=sha(ROOT/name)))
result=dict(result='PASS',protectedFiles=len(b['protected']),changedEdges=changed,maps=maps,checks=checks,scope='Static preservation/finite geometry; NOT a terrain-perfect or runtime assertion')
(OUT/'verification.json').write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print('PASS:',len(checks),'bounded checks,',len(b['protected']),'protected byte-identical files,',len(changed),'changed edges')
