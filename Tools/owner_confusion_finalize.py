"""Targeted protection, evidence and compact incremental checkpoint. No build/save mutations."""
import datetime,hashlib,html,json,re,shutil,sys
from pathlib import Path
R=Path(__file__).resolve().parents[1]
D=R/'Docs/QA/OwnerConfusion-20261020'
V=R/'Validation/OwnerConfusion-20261020'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def text(p):return p.read_text(encoding='utf-8-sig')
def dump(p,obj):p.write_text(json.dumps(obj,ensure_ascii=False,indent=2),encoding='utf-8')
def source_paths():
 paths=set()
 for f in load(R/'Docs/QA/CasualRuntime-20261020/before.json'):
  p=R/f['path']
  if sha(p)!=f['sha256']:paths.add(p)
 for f,digest in load(D/'before.json').items():
  p=R/f
  if sha(p)!=digest:paths.add(p)
 for f in load(R/'Docs/QA/H3TapPickup-20261020/baseline.json')['sourceBefore']:paths.add(R/f)
 for f in ['UI/GameplayPanels.ContextHelp.cs','UI/RelicGuide.cs','Maps/CityMap.SignatureMoments.cs','Validation/GameplaySmokeRunner.H3Tap.cs','Validation/GameplaySmokeRunner.CasualRuntime.cs','Validation/GameplaySmokeRunner.OwnerConfusion.cs','Editor/OwnerRouteAndroidBuild.cs']:
  paths.add(R/'Assets/_Game/Scripts'/f)
 return paths
def verify():
 checks=[]
 def check(name,ok):checks.append({'name':name,'pass':bool(ok)})
 before=D/'before-source'
 a=load(before/'Assets/_Game/Resources/Content/GameplayContent.json');b=load(R/'Assets/_Game/Resources/Content/GameplayContent.json')
 old=next(i for i in a['items'] if i['id']=='ancient_seal');new=next(i for i in b['items'] if i['id']=='ancient_seal')
 allowed=['iconId','description'];changed=[k for k in new if new[k]!=old[k]]
 check('Only ancient_seal iconId/description changed in gameplay content',set(changed)==set(allowed))
 for k in allowed:new[k]=old[k]
 check('All nodes, legacy edges, quests, events, puzzles, rewards and canonical story otherwise identical',a==b)
 a=load(before/'Assets/_Game/Resources/Content/AdventurePresentation.json');b=load(R/'Assets/_Game/Resources/Content/AdventurePresentation.json')
 for data in [a,b]:data['items']=[i for i in data['items'] if i['id']!='ancient_seal']
 check('Presentation JSON unchanged outside the single seal item',a==b)
 def without_item(s):return re.sub(r'  - id: ancient_seal\n.*?(?=  - id:)', '',s,flags=re.S)
 check('Native bindings unchanged outside ancient_seal',without_item(text(before/'Assets/_Game/Resources/AdventureUIArt.asset'))==without_item(text(R/'Assets/_Game/Resources/AdventureUIArt.asset')))
 a=text(before/'Assets/_Game/Scripts/Maps/CityMap.cs');b=text(R/'Assets/_Game/Scripts/Maps/CityMap.cs')
 def movement(s):return s[s.index('        public void TryMove('):s.index('        private void PositionGroundedActor(')]
 check('Entire TryMove/Dijkstra/boat movement/cancellation block unchanged',movement(a)==movement(b))
 for p,digest in load(D/'before.json').items():
  if p not in [str(q.relative_to(R)).replace('\\','/') for q in source_paths()]:check('Unchanged '+p,sha(R/p)==digest)
 protected=load(R/'Docs/QA/H3TapPickup-20261020/baseline.json')['protected']
 for entry in protected:
  p=entry['path']
  if p not in ['Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Resources/Content/GameplayContent.json']:check('Protected '+p,sha(R/p)==entry['sha256'])
 # Prior checkpoint recorded canonical bitmaps, sound and texture metadata. Check
 # those exact references only, rather than hashing the entire art library.
 prior=load(R/'Docs/QA/CasualAdventure-20261020/baseline.json')
 for entry in prior['entries']:
  p=entry['path']
  if p.endswith(('.png','.mp3','.wav','.meta')):check('Prior art/audio '+p,sha(R/p)==entry['sha256'])
 for name,insertions in [('CityMap.RelicLight.cs',['            TickSignature();','                RequestSignature(r);']),('CityMap.CulturalRestoration.cs',['                RequestSignature(culturalRelic);'])]:
  p=Path('Assets/_Game/Scripts/Maps')/name;old=text(R/'Docs/QA/CasualRuntime-20261020/before-source'/p);now=text(R/p)
  # Compare after removing just the new transient feedback call lines.
  def strip(s):return '\n'.join(re.sub(r'RequestSignature\((?:r|culturalRelic)\);','',line) for line in s.splitlines() if line.strip()!='TickSignature();')
  check('Existing lighting geometry/state intact: '+name,strip(old)==strip(now))
 pairs=[]
 for name in ['hoi_an-overview','hoi_an-detail','hoi_an-crab','hoi_an-prop','hue-overview','hue-detail','seoul-overview','seoul-detail']:
  a=load(V/'before-clarity'/(name+'.json'));b=load(V/'after-clarity'/(name+'.json'))
  fields=['width','height','zoom','canvasScale','contentPosition','contentSize']
  same=all(a[k]==b[k] for k in fields);check('Matched camera '+name,same);pairs.append({'name':name,'sameCamera':same,'before':str((V/'before-clarity'/(name+'.png')).relative_to(R)).replace('\\','/'),'after':str((V/'after-clarity'/(name+'.png')).relative_to(R)).replace('\\','/')})
 dump(D/'comparison-pairs.json',pairs)
 result={'pass':all(c['pass'] for c in checks),'checks':checks};dump(D/'protection.json',result)
 dump(D/'changed-source.json',[{'path':p.relative_to(R).as_posix(),'bytes':p.stat().st_size,'sha256':sha(p)} for p in sorted(source_paths())])
 print(json.dumps({'protectionPass':result['pass'],'failed':[c for c in checks if not c['pass']]}))
 return result['pass']
def evidence():
 runs=[]
 for name in ['fresh01','fresh02','seal01','seal02','seal03','resume01','restoration01','main01','before-clarity','after-clarity']:
  f=V/name/'smoke-results.txt'
  if f.exists():
   s=text(f);runs.append({'run':name,'result':'PASS' if s.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL','log':str(f.relative_to(R)).replace('\\','/')})
 dump(D/'runs.json',runs)
 cards=[]
 for pair in load(D/'comparison-pairs.json'):
  title=pair['name']+' | same camera / zoom / 540x960'
  cards.append('<section><h2>'+title+'</h2><div class="pair">'+''.join('<figure><figcaption>'+label+'</figcaption><a href="../../../'+pair[key]+'"><img loading="lazy" src="../../../'+pair[key]+'"></a></figure>' for label,key in [('BEFORE','before'),('AFTER','after')])+'</div></section>')
 highlights=[('fresh02','Owner-fragment-markers-start'),('fresh02','Owner-four-fragments-collection'),('fresh02','Owner-four-fragments-next-step'),('fresh02','Owner-mai-ground-arrival'),('fresh02','Owner-assembly-H7-required'),('seal03','Owner-seal-inventory'),('seal03','Owner-seal-receipt-presentation'),('resume01','Owner-four-fragments-restart'),('main01','Owner-H7-next-fragment'),('main01','Casual-signature-hoi_an')]
 for run,name in highlights:
  p=V/run/(name+'.png')
  if p.exists():
   url='../../../'+p.relative_to(R).as_posix();cards.append('<section><h2>'+html.escape(name)+'</h2><p>Actual Windows Player. Receipt illustration review uses an already owned item; no reward is re-granted.</p><a href="'+url+'"><img loading="lazy" src="'+url+'"></a></section>')
 (D/'gallery.html').write_text('<!doctype html><meta charset="utf-8"><title>LBC Owner UX fixes</title><style>body{background:#16393b;color:#fff2d4;font:16px system-ui;margin:24px}section{margin:30px 0;padding:16px;background:#234c4e}h2{font-size:19px}.pair{display:flex;gap:16px;flex-wrap:wrap}figure{margin:0;flex:1;min-width:260px}img{width:min(100%,540px);height:auto}a{color:#ffe2a0}</style><h1>Owner playtest correction · 2026-10-20</h1><p>Windows runtime evidence; Android device NOT_TESTED. Original map bitmaps preserved.</p>'+''.join(cards),encoding='utf-8')
 print(json.dumps(runs,ensure_ascii=False))
def checkpoint(create):
 assert load(D/'protection.json')['pass'],'Protection check failed'
 B=Path('D:/GAMEKKK/Backups/CheckpointCasualAdventureUI-20261020');m=load(B/'CHECKPOINT_MANIFEST.json')
 assert sha(B/'CHECKPOINT_MANIFEST.json')=='8ce5d1b5b74e80a59f5e79fd84ddd2ca97f827deeeeca3310e3d96c42f2d8ed1'
 for e in m['entries']:assert sha(B/e['path'])==e['sha256'],e['path']
 for e in m['baseChain']:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
 paths=source_paths()
 paths|={Path(str(p)+'.meta') for p in list(paths) if Path(str(p)+'.meta').exists()}
 paths|={R/'Tools/owner_confusion_fix.py',Path(__file__).resolve(),R/'Tools/h3_tap_ux.py',R/'Docs/QA/OWNER_CONFUSION_FIX_20261020.md',R/'Docs/Art/ANCIENT_SEAL_ICON_BINDING_20261020.md'}
 paths.add(V/'seed-provenance.json')
 for folder in [D,R/'Docs/QA/H3TapPickup-20261020',R/'Docs/QA/CasualRuntime-20261020']:
  paths|={p for p in folder.rglob('*') if p.is_file() and p.name!='checkpoint.json'}
 for name in ['owner-ux-build01.log','owner-ux-build02.log','owner-ux-build03.log','owner-ux-build04.log','owner-ux-android01.log','owner-ux-android02.log']:paths.add(R/'Logs'/name)
 for run in load(D/'runs.json'):
  folder=R/Path(run['log']).parent;paths|={p for p in folder.iterdir() if p.is_file() and p.suffix in ['.json','.txt']}
 for url in re.findall(r'(?:href|src)="([^"]+)"',text(D/'gallery.html')):
  if not url.startswith('../../../'):continue
  p=(D/url).resolve();assert p.is_file();paths.add(p)
 paths={p for p in paths if p.is_file()}
 target=Path('D:/GAMEKKK/Backups/CheckpointOwnerConfusionUX-20261020');size=sum(p.stat().st_size for p in paths)
 estimate={'files':len(paths),'MiB':round(size/1048576,2),'baseVerified':True,'target':str(target)};print(json.dumps(estimate))
 if not create:return
 assert not target.exists(),'Refuse to overwrite a checkpoint'
 target.mkdir();entries=[]
 for p in sorted(paths):
  rel=p.relative_to(R);dest=target/rel;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest);digest=sha(p);assert sha(dest)==digest
  entries.append({'path':rel.as_posix(),'bytes':p.stat().st_size,'sha256':digest})
 guide='INCREMENTAL, NOT STANDALONE. Requires CheckpointCasualAdventureUI-20261020 and its verified base chain. Close Unity, restore that base chain in an isolated folder, then overlay exactly the listed entries after verifying their hashes. No deleted paths. No Library/cache/build binaries or normal player saves. Validation saves are isolated QA evidence. This checkpoint preserves earlier H3/shared UX work plus owner-confusion fixes; it is not acceptance of the complete casual overhaul.'
 (target/'RESTORE.md').write_text(guide,encoding='utf-8')
 manifest={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(B),'baseManifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json'),'baseChain':[{'path':str(B),'manifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json')}]+m['baseChain'],'entries':entries,'bytes':size,'deletedPaths':[],'restoreGuideSHA256':sha(target/'RESTORE.md'),'summary':'Live owner playtest fixes: grounded interaction presentation, seal icon/purpose, four-fragment markers and actionable restoration guidance. Preserves prior in-progress H3/context help/journal/leisure/signature source. Not full-overhaul acceptance.'}
 dump(target/'CHECKPOINT_MANIFEST.json',manifest)
 dump(D/'checkpoint.json',{'path':str(target),'files':len(entries),'bytes':size,'manifestSHA256':sha(target/'CHECKPOINT_MANIFEST.json'),'payloadVerified':True,'baseVerified':True,'standalone':False,'requiresBase':str(B)})
if __name__=='__main__':
 command=sys.argv[1]
 if command=='verify':verify()
 elif command=='evidence':evidence()
 elif command in ['estimate','checkpoint']:checkpoint(command=='checkpoint')
