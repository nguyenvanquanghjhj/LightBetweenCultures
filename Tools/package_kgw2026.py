"""Prepare local contest files, honest QA receipts and a small delta checkpoint."""
from pathlib import Path
import datetime,hashlib,html,json,re,shutil,sys,zipfile
from pptx import Presentation
R=Path(__file__).resolve().parents[1];D=R/'Docs/Contest/KGW2026';S=R/'Submission/KGW2026';V=R/'Validation/ContestReadiness-20261020'
B=Path('D:/GAMEKKK/Backups/CheckpointOwnerConfusionUX-20261020')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,v):p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')
def txt(p):return p.read_text(encoding='utf-8-sig')
def verify():
 checks=[]
 def ck(n,ok):checks.append({'name':n,'pass':bool(ok)})
 base=load(B/'CHECKPOINT_MANIFEST.json')
 ck('Base manifest unchanged',sha(B/'CHECKPOINT_MANIFEST.json')=='c89dcdabd3ea5b98f0265131c36c58b37e826d6ea72d4b82b1f3ce2f6aeda354')
 allowed={'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Maps/MainMenu.cs','Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs'}
 for e in base['entries']:
  if e['path'].startswith('Assets/') and e['path'] not in allowed:ck('Frozen '+e['path'],sha(R/e['path'])==e['sha256'])
 for e in load(R/'Docs/QA/H3TapPickup-20261020/baseline.json')['protected']:
  p=e['path']
  if p not in ['Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Resources/Content/GameplayContent.json']:ck('Canonical '+p,sha(R/p)==e['sha256'])
 old=txt(D/'before-source/Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs');now=txt(R/'Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs')
 ck('Hanji change removes only the overlapping decorative label','\n'.join(x for x in old.splitlines() if '"HanjiStitch"' not in x)=='\n'.join(now.splitlines()))
 old=txt(D/'before-source/Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs');now=txt(R/'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs')
 ck('QA runner only adds one opt-in dispatch','\n'.join(x for x in now.splitlines() if 'Contains("--contest-resume")' not in x)=='\n'.join(old.splitlines()))
 p=Presentation(S/'LightBetweenCultures_KGW2026_DRAFT.pptx');ck('PPTX contains 13 slides',len(p.slides)==13)
 ck('Slide shapes remain on canvas',all(q.left>=0 and q.top>=0 and q.left+q.width<=p.slide_width+9144 and q.top+q.height<=p.slide_height+9144 for sl in p.slides for q in sl.shapes))
 ck('PowerPoint native PDF and all 13 previews exist',(S/'LightBetweenCultures_KGW2026_PREVIEW.pdf').is_file() and all((V/'PitchRender'/('Slide'+str(i)+'.PNG')).is_file() for i in range(1,14)))
 result={'pass':all(c['pass'] for c in checks),'checks':checks};dump(D/'protection.json',result)
 print(json.dumps({'protectionPass':result['pass'],'failed':[x for x in checks if not x['pass']]}));return result['pass']
def evidence():
 runs=[]
 for name,build in [('core01','Windows-OwnerUX-20261020'),('signature-jeonju','Windows-OwnerUX-20261020'),('signature-gyeongju','Windows-OwnerUX-20261020'),('signature-seoul','Windows-OwnerUX-20261020'),('resume01','Windows-Contest-20261020 build01'),('signature-jeonju-final','Windows-Contest-20261020 build02')]:
  p=V/name/'smoke-results.txt';s=txt(p);runs.append({'run':name,'build':build,'result':'PASS' if s.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL','results':p.relative_to(R).as_posix(),'scope':'Automated real Windows UI; isolated save; not a blind human test'})
 dump(D/'runs.json',runs)
 assets=[]
 for name in ['Builds/Windows-Contest-20261020/LightBetweenCultures.exe','Builds/Windows-Contest-20261020/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll','Builds/Android/LightBetweenCultures-Contest-Final-20261020.apk','Submission/KGW2026/LightBetweenCultures_KGW2026_DRAFT.pptx','Submission/KGW2026/LightBetweenCultures_KGW2026_PREVIEW.pdf']:
  p=R/name;assets.append({'path':name,'bytes':p.stat().st_size,'sha256':sha(p)})
 dump(D/'artifacts.json',{'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'androidDevice':'NOT_TESTED: adb devices returned no connected device','windowsBuild':'Logs/contest-windows02.log','androidBuild':'Docs/Contest/KGW2026/android-final-build.json','files':assets})
 # Collect actual source provenance of the isolated QA seeds, not newly invented flags.
 seeds=[('resume01','Validation/OwnerRoutePhaseB-20261019/story-korea-seoul02/OwnerRoutes-story-final.json'),('signature-jeonju','Validation/OwnerRoutePhaseB-20261019/story-vietnam/phase4-jeonju-before-trial.json'),('signature-jeonju-final','Validation/OwnerRoutePhaseB-20261019/story-vietnam/phase4-jeonju-before-trial.json'),('signature-gyeongju','Validation/OwnerRoutePhaseB-20261019/story-vietnam/phase4-gyeongju-before-trial.json'),('signature-seoul','Validation/OwnerRoutePhaseB-20261019/story-korea-seoul02/phase4-seoul-before-trial.json')]
 dump(D/'qa-seed-provenance.json',[{'run':n,'source':f,'sourceSHA256':sha(R/f),'flagsInjected':False} for n,f in seeds])
 gallery=['<!doctype html><meta charset="utf-8"><title>LBC KGW2026</title><style>body{background:#102d32;color:#fff4dc;font:18px Arial;margin:32px}a{color:#efb963}img{width:min(100%,1100px);height:auto}section{margin:28px 0}small{color:#b9d3cd}</style><h1>Light Between Cultures · KGW2026</h1><p>Bộ chuẩn bị dự thi — DRAFT, chờ thông tin người dự thi và xác nhận quyền sử dụng tài nguyên.</p><p><a href="LightBetweenCultures_KGW2026_DRAFT.pptx">PowerPoint</a> · <a href="LightBetweenCultures_KGW2026_PREVIEW.pdf">PDF</a> · <a href="../../Builds/Android/LightBetweenCultures-Contest-Final-20261020.apk">APK</a> · <a href="LightBetweenCultures_Windows_20261020.zip">Windows ZIP</a> · <a href="../../Docs/Contest/KGW2026/READINESS.md">Readiness</a></p>']
 for i in range(1,14):gallery.append('<section><h2>Slide '+str(i)+'</h2><img loading="lazy" src="../../Validation/ContestReadiness-20261020/PitchRender/Slide'+str(i)+'.PNG"></section>')
 for run,name in [('resume01','Contest-continue-menu'),('signature-jeonju','P4-jeonju-trial-correct-before-confirm'),('signature-jeonju-final','P4-jeonju-trial-correct-before-confirm')]:
  gallery.append('<section><h2>'+html.escape(run+' / '+name)+'</h2><small>Actual Windows Player; Android device NOT_TESTED</small><p><img style="max-width:540px" loading="lazy" src="../../Validation/ContestReadiness-20261020/'+run+'/'+name+'.png"></p></section>')
 (S/'index.html').write_text(''.join(gallery),encoding='utf-8');print(json.dumps(runs))
def windows_zip():
 root=R/'Builds/Windows-Contest-20261020';files=[p for p in root.rglob('*') if p.is_file() and not any('DoNotShip' in q or 'DontShip' in q for q in p.parts) and p.suffix.lower()!='.pdb']
 dest=S/'LightBetweenCultures_Windows_20261020.zip'
 print(json.dumps({'WindowsFiles':len(files),'uncompressedMiB':round(sum(p.stat().st_size for p in files)/1048576,2),'output':str(dest)}),flush=True)
 assert not dest.exists(),'Do not overwrite a prepared distribution'
 with zipfile.ZipFile(dest,'w',zipfile.ZIP_DEFLATED,compresslevel=3) as z:
  for p in files:z.write(p,p.relative_to(root).as_posix())
 with zipfile.ZipFile(dest) as z:
  assert z.testzip() is None
  assert 'LightBetweenCultures.exe' in z.namelist() and 'UnityPlayer.dll' in z.namelist()
 dump(D/'windows-package.json',{'output':dest.relative_to(R).as_posix(),'bytes':dest.stat().st_size,'sha256':sha(dest),'files':len(files),'zipIntegrity':'PASS','runtime':'Existing development Windows Player, no QA arguments by default','excluded':'Only debug symbols .pdb and Unity folders explicitly marked DontShip/DoNotShip; originals untouched'})
def checkpoint(create):
 assert load(D/'protection.json')['pass']
 m=load(B/'CHECKPOINT_MANIFEST.json')
 for e in m['entries']:assert sha(B/e['path'])==e['sha256'],e['path']
 for e in m['baseChain']:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
 source=['Assets/_Game/Scripts/Maps/MainMenu.cs','Assets/_Game/Scripts/UI/GameplayPanels.CulturalTrials.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Contest.cs']
 paths={R/p for p in source};paths|={Path(str(p)+'.meta') for p in list(paths) if Path(str(p)+'.meta').exists()}
 paths|={p for p in D.rglob('*') if p.is_file() and p.name!='checkpoint.json'}
 paths|={p for p in S.iterdir() if p.is_file() and p.suffix!='.zip'}
 paths|={R/'Tools/build_kgw2026_pitch.py',Path(__file__).resolve()}
 for log in ['contest-windows01.log','contest-windows02.log','contest-android01.log','contest-android02.log']:paths.add(R/'Logs'/log)
 for run in load(D/'runs.json'):
  f=R/Path(run['results']).parent;paths|={p for p in f.iterdir() if p.is_file() and p.suffix in ['.txt','.json','.png']}
 paths|={p for p in (V/'PitchRender').iterdir() if p.suffix.lower()=='.png'}
 paths={p for p in paths if p.is_file()}
 target=Path('D:/GAMEKKK/Backups/CheckpointContestPreparation-20261020');size=sum(p.stat().st_size for p in paths)
 print(json.dumps({'files':len(paths),'MiB':round(size/1048576,2),'baseVerified':True,'target':str(target)}))
 if not create:return
 assert not target.exists(),'Do not overwrite checkpoints'
 target.mkdir();entries=[]
 for p in sorted(paths):
  rel=p.relative_to(R);q=target/rel;q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q);h=sha(p);assert sha(q)==h;entries.append({'path':rel.as_posix(),'bytes':p.stat().st_size,'sha256':h})
 (target/'RESTORE.md').write_text('INCREMENTAL, NOT STANDALONE. Requires CheckpointOwnerConfusionUX-20261020 and its base chain. Verify hashes, close Unity, restore the base chain in an isolated location, then overlay the listed entries. No deletions. No Library, full build binaries, distribution ZIP or normal user saves. QA saves are evidence only. Rebuild binaries from source if restoring. The PPTX is DRAFT pending entrant/registration/rights confirmation.',encoding='utf-8')
 dump(target/'CHECKPOINT_MANIFEST.json',{'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(B),'baseManifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json'),'baseChain':[{'path':str(B),'manifestSHA256':sha(B/'CHECKPOINT_MANIFEST.json')}]+m['baseChain'],'entries':entries,'bytes':size,'deletedPaths':[],'restoreGuideSHA256':sha(target/'RESTORE.md'),'summary':'Contest preparation: saved-city resume copy and reset-button caption, one Hanji decorative-label overlap fix, targeted Windows QA, editable PPTX/PDF and local submission preparation. Registration, rights and Android-device acceptance pending.'})
 dump(D/'checkpoint.json',{'path':str(target),'files':len(entries),'bytes':size,'manifestSHA256':sha(target/'CHECKPOINT_MANIFEST.json'),'payloadVerified':True,'baseVerified':True,'standalone':False,'requiresBase':str(B)})
if __name__=='__main__':
 c=sys.argv[1]
 if c=='verify':verify()
 elif c=='evidence':evidence()
 elif c=='windows-zip':windows_zip()
 elif c in ['estimate','checkpoint']:checkpoint(c=='checkpoint')
