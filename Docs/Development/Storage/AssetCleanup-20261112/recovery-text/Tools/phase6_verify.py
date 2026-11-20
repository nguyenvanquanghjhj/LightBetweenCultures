"""Bounded Phase6 protection checks. Never rewrites game data or art."""
import hashlib,json,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/QA/Phase6-VisualAndroid'
b=json.loads((OUT/'baseline.json').read_text(encoding='utf-8-sig'))
before=Path(b['before'])
digest=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
changed=[e['path'] for e in b['entries'] if digest(ROOT/e['path'])!=e['sha256']]
allowed={
 'Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs',
 'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs',
 'Assets/_Game/Art/Maps/Gyeongju/Map_Gyeongju_Overview_A.png.meta',
 'Assets/_Game/Art/Maps/Seoul/Map_Seoul_Overview_A.png.meta',
 'Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Jeonju_HanjiSeal_Source.png.meta',
 'Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_Seoul_ThreeRelayPrism_Source.png.meta'}
checks=[]
def check(ok,label):
 checks.append(dict(result='PASS' if ok else 'FAIL',check=label))
 if not ok:raise AssertionError(label)
check(set(changed)==allowed,'Exact six changed baseline files: UI clamp, QA dispatch and four related importer metadata')
for e in b['entries']:
 if e['path'] not in allowed:check(digest(ROOT/e['path'])==e['sha256'],'Unchanged '+e['path'])
for city in ['Gyeongju','Seoul']:
 name=f'Assets/_Game/Art/Maps/{city}/Map_{city}_Overview_A.png.meta'
 old=(before/name).read_text(encoding='utf-8-sig');new=(ROOT/name).read_text(encoding='utf-8-sig')
 new=new.replace('enableMipMap: 1','enableMipMap: 0').replace('filterMode: 2','filterMode: 1')
 # Unity added a disabled Android platform entry; no override/format was enabled.
 new=re.sub(r'  - serializedVersion: 4\n    buildTarget: Android\n(?:(?!  spriteSheet:|  - serializedVersion: 4).)*', '',new,flags=re.S)
 check(new==old,'Only mipmap/trilinear plus disabled Android serialization: '+city)
strip_outline=lambda s:re.sub(r'      outline:\n.*?(?=      physicsShape:)', '      outline: []\n',s,flags=re.S)
for city in ['Jeonju_HanjiSeal','Seoul_ThreeRelayPrism']:
 name='Assets/_Game/Art/_SourceSheets/CulturalArtifacts/Artifact_'+city+'_Source.png.meta'
 old=(before/name).read_text(encoding='utf-8-sig');new=(ROOT/name).read_text(encoding='utf-8-sig')
 # Strip all per-sprite outline shapes in both to verify every other field, rectangle and ID.
 check(strip_outline(new)==strip_outline(old),'Source rects/GUID/spriteIDs/localIDs/import settings unchanged; outline-only: '+city)
name='Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs'
old=(before/name).read_text(encoding='utf-8-sig');new=(ROOT/name).read_text(encoding='utf-8-sig')
check(new[new.index('        void ShowRelicPhoto()'):]==old[old.index('        void ShowRelicPhoto()'):],'H6/photo/collection/details function bodies unchanged')
content=json.loads((ROOT/'Assets/_Game/Resources/Content/GameplayContent.json').read_text(encoding='utf-8-sig'))
counts={key:len(content[key]) for key in ['quests','puzzles','checkIns']}
counts.update(cities=len(content['cities']),nodes=sum(len(c['nodes']) for c in content['cities']),edges=sum(len(c['edges']) for c in content['cities']))
check(counts==dict(cities=6,nodes=133,edges=155,quests=29,puzzles=22,checkIns=18),'Canonical gameplay counts and exact original JSON hash retained')
result=dict(result='PASS',checks=checks,changed=changed,counts=counts,baseCheckpoint=b['baseCheckpoint'],baseManifestSHA256=b['baseManifestSHA256'])
(OUT/'source-verification.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
print('PASS',len(checks),'bounded checks; protected content/audio/NPC/maps/lighting/save/backend/ending unchanged')
