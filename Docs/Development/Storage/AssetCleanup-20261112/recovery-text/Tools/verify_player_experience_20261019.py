"""Read-only targeted protection verification for the Hoi An UX pilot."""
from pathlib import Path
import json,hashlib,re,difflib
R=Path(__file__).resolve().parents[1];O=R/'Docs/QA/PlayerExperience-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
old=read(O/'before-source/Assets/_Game/Resources/Content/GameplayContent.json')
new=read(R/'Assets/_Game/Resources/Content/GameplayContent.json')
changes=[]
def diff(a,b,path=''):
 if type(a)!=type(b):changes.append((path,a,b));return
 if isinstance(a,dict):
  assert a.keys()==b.keys(),path
  for k in a:diff(a[k],b[k],path+'/'+k)
 elif isinstance(a,list):
  assert len(a)==len(b),path
  for i,(x,y) in enumerate(zip(a,b)):diff(x,y,path+'/'+str(i))
 elif a!=b:changes.append((path,a,b))
diff(old,new)
allowed={'text','offer','reminder','thanks','description','preview','missingText','completedText','confirmationText','alternateText','alternateCompletedText','battleCompletedText','clue'}
assert all(p.rsplit('/',1)[-1] in allowed and isinstance(a,str) and isinstance(b,str) for p,a,b in changes)
checks=[{'name':'GameplayContent changes are player-facing strings only','pass':True,'changedFields':len(changes)}]
for c in new['cities']:
 before=next(x for x in old['cities'] if x['id']==c['id'])
 assert c==before
 checks.append({'name':'Nodes, arrivals, all edges/waypoints/discovery/gates unchanged: '+c['id'],'pass':True})
assert len(new['quests'])==29 and len(new['puzzles'])==22 and sum(len(c['nodes']) for c in new['cities'])==133 and sum(len(c['edges']) for c in new['cities'])==155
checks.append({'name':'133 nodes / 155 old edges / 29 quests / 22 puzzles preserved','pass':True})
for entry in read(R/'Validation/OwnerRouteRedesign-20261019/production-baseline.json'):
 if entry['path'] in ['Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Scripts/Maps/CityMap.cs']:continue
 assert sha(R/entry['path'])==entry['sha256'],entry['path']
 checks.append({'name':'Protected original route/art/config hash: '+entry['path'],'pass':True})
def method(s,name):
 match=re.search(r'public (?:bool|string) '+name+r'\(',s);assert match,name
 start=s.index('{',match.start());depth=0
 for i in range(start,len(s)):
  if s[i]=='{':depth+=1
  elif s[i]=='}':
   depth-=1
   if depth==0:return s[match.start():i+1]
before=(O/'before-source/Assets/_Game/Scripts/Core/QuestManager.cs').read_text(encoding='utf-8-sig')
after=(R/'Assets/_Game/Scripts/Core/QuestManager.cs').read_text(encoding='utf-8-sig')
for name in ['CanAccept','Accept','CanComplete','Complete','TalkToMai','CollectPart']:
 assert method(before,name)==method(after,name),name
 checks.append({'name':'Quest state mutation/eligibility method unchanged: '+name,'pass':True})
before=(O/'before-source/Assets/_Game/Scripts/Maps/CityMap.cs').read_text(encoding='utf-8-sig')
after=(R/'Assets/_Game/Scripts/Maps/CityMap.cs').read_text(encoding='utf-8-sig')
def moveblock(s):return s[s.index('        private IEnumerator Move('):s.index('        public void Interact()')]
assert moveblock(before)==moveblock(after)
checks.append({'name':'Actual movement/geometry/travel implementation unchanged','pass':True})
before=(O/'before-source/Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs').read_text(encoding='utf-8-sig')
after=(R/'Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs').read_text(encoding='utf-8-sig')
assert after.replace('    [RequireComponent(typeof(CanvasRenderer))]\n','')==before
checks.append({'name':'Approved swing/boat poses and animation code unchanged; CanvasRenderer requirements only','pass':True})
before=(O/'before-source/Assets/_Game/Scripts/UI/GameplayPanels.Leisure.cs').read_text(encoding='utf-8-sig')
after=(R/'Assets/_Game/Scripts/UI/GameplayPanels.Leisure.cs').read_text(encoding='utf-8-sig')
assert after.replace('    [RequireComponent(typeof(CanvasRenderer))]\n','')==before
checks.append({'name':'All ten other activity mechanics unchanged; thread renderer requirement only','pass':True})
apk=R/'Builds/Android/LightBetweenCultures-Test.apk'
assert sha(apk)=='ecc062ade3c1967a2d184e555a38098b3269a6b4de602aa1c3c66b1f523b5481'
checks.append({'name':'Latest PASS Android APK unchanged','pass':True})
assert sha(R/'ProjectSettings/EditorSettings.asset')==sha(O/'EditorSettings.before.asset')
checks.append({'name':'Temporary Editor domain-reload setting restored byte-for-byte','pass':True})
base=read(O/'baseline.json');changed=[]
for e in base['files']:
 p=R/e['path']
 if sha(p)!=e['sha256']:
  changed.append(e['path'])
  b=(O/'before-source'/e['path']).read_text(encoding='utf-8-sig');a=p.read_text(encoding='utf-8-sig')
  (O/(p.name+'.diff')).write_text(''.join(difflib.unified_diff(b.splitlines(True),a.splitlines(True),fromfile='before/'+e['path'],tofile='after/'+e['path'])),encoding='utf-8')
report={'status':'TARGETED_STATIC_PROTECTION_PASS','checks':checks,'changedExistingFiles':changed,'contentTextChanges':[{'path':p,'before':a,'after':b} for p,a,b in changes],
 'notes':['No claim of Windows Player or Android execution; see runtime logs separately.','State/graph preservation checks do not prove visual quality.']}
(O/'protection-verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
print('Protection checks:',len(checks));print('Existing files changed:',len(changed));print('Content strings changed:',len(changes))
