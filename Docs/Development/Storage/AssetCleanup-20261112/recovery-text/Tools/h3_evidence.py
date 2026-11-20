"""Bounded H3 verification and incremental checkpoint; no rebuild or image edits."""
from pathlib import Path
import datetime,hashlib,json,re,shutil,sys
from PIL import Image
R=Path(__file__).resolve().parents[1]
D=R/'Docs/H3Upgrade-20261017'
V=R/'Validation/H3Upgrade-20261017'
B=R.parent/'Backups/CheckpointFullStoryComplete-20261017'
RUNS=['Touch07','Desktop06','RestartTouch03','RestartDesktop03']
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):
 h=hashlib.sha256()
 with p.open('rb') as f:
  for b in iter(lambda:f.read(4*1024*1024),b''):h.update(b)
 return h.hexdigest()
def write(p,x):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def verify():
 before=read(D/'before-hashes.json')
 allowed={'Assets/_Game/Scenes/HoiAnDemo.unity','Assets/_Game/Scripts/Editor/ArtLibrarySetup.cs','Assets/_Game/Scripts/HoiAn/HoiAnGameplay.cs','Assets/_Game/Scripts/HoiAn/FollowCamera2D.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs'}
 changes=[p for p,h in before.items() if sha(R/p)!=h]
 assert set(changes)==allowed,changes
 old=(D/'BeforeFiles/Assets/_Game/Scenes/HoiAnDemo.unity').read_text(encoding='utf-8');new=(R/'Assets/_Game/Scenes/HoiAnDemo.unity').read_text(encoding='utf-8')
 def physics(text):return {m[2]:m[0] for m in re.findall(r'((?:^--- !u!(50|58|60|61) &(\d+).*?))(?=^--- !u!|\Z)',text,re.M|re.S)}
 assert physics(old)==physics(new) and len(physics(old))==14
 native=read(R/'Docs/Art/native-art-bindings.json')['entries'];oldnative=read(B/'Docs/Art/native-art-bindings.json')['entries']
 npcs={e['id']:e for e in native if e['role']=='npc'}
 for b in [e for e in oldnative if e['role'] in ('npc','enemy','lumi','canonical_npc','marker','city_main','city_preview')]:
  actual=next(e for e in native if e['role']==b['role'] and e['id']==b['id'])
  assert all(actual[k]==b[k] for k in ['path','guid','localId','spriteName']),b['id']
 for id in ['npc_ba_tu','npc_phuc','npc_huong','npc_soyeon']:assert npcs[id]['canonical'] and npcs[id]['status']=='CANONICAL'
 h3=[e for e in native if e['role'].startswith('h3_')];assert len(h3)==10
 for e in h3:
  assert (R/e['path']).exists();assert e['guid'] in (R/(e['path']+'.meta')).read_text(encoding='utf-8')
 sources=[]
 for p in (R/'Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016').glob('*.png'):
  with Image.open(p) as im:record=dict(path=p.relative_to(R).as_posix(),mode=im.mode,size=list(im.size),alpha=list(im.getchannel('A').getextrema()) if im.mode=='RGBA' else 'opaque reference RGB')
  record['sha256']=sha(p);assert record['sha256']==before[record['path']];sources.append(record)
 assert len(sources)==7 and len({s['sha256'] for s in sources})==7
 assert all(s['alpha']==[0,255] for s in sources if s['mode']=='RGBA')
 results=[];images=[]
 for run in RUNS:
  p=V/run/'smoke-results.txt';lines=p.read_text(encoding='utf-8-sig').splitlines();assert lines[-1]=='ALL CHECKS PASSED' and not any('FAIL:' in s or s.startswith('ERROR:') for s in lines),run
  results.append(dict(run=run,assertions=sum(s.startswith('PASS:') for s in lines),sha256=sha(p)))
  for p in (V/run).glob('*.png'):
   with Image.open(p) as im:size=list(im.size)
   images.append(dict(path=p.relative_to(R).as_posix(),sha256=sha(p),size=size,build='09'))
 fresh=[read(V/run/'h3-earned-final.json') for run in RUNS[:2]]
 for save in fresh:
  assert save['items'].count('lamp_eye')==1 and 'lantern-lit' in save['flags']
 for dst,src in [('RestartTouch03','Touch07'),('RestartDesktop03','Desktop06')]:
  lineage=read(V/dst/'lineage.json');assert lineage['seedSHA256']==sha(V/src/'h3-earned-final.json') and lineage['progressFieldsEdited'] is False
  assert read(V/dst/'gameplay-save.json')['items'].count('lamp_eye')==1
 for path in [V/'Before/05-H3-exploration.png',V/'Touch07/05-H3-exploration.png']:
  with Image.open(path) as im:assert im.size==(540,960)
 assert 'Windows build: Succeeded' in (R/'Logs/h3-upgrade-build-09.log').read_text(encoding='utf-8-sig')
 result=dict(status='H3_PRESENTATION_GAMEPLAY_WINDOWS_TOUCH_RESTART_PASS_VISUAL_ACCEPTANCE_PENDING',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),finalBuild='09',sceneSHA256=sha(R/'Assets/_Game/Scenes/HoiAnDemo.unity'),physicsBlocksByteIdentical=14,unchangedOriginalAssetsPackagesSettings=len(before)-len(changes),changedOriginalFiles=changes,npcEnemyLumiMarkerCityBindingsUnchanged=True,sources=sources,h3SpriteBindings=h3,runs=results,assertions=sum(x['assertions'] for x in results),images=images,assemblySHA256=sha(R/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'),limitations=['Android physical device not tested.','One approved exploration pose plus existing step cue; no drawn walk cycle.','Floor is a new AI-derived material; user visual approval is separate from gameplay PASS.','Other puzzle/combat art remains outside this H3 overhaul.'])
 write(D/'verification.json',result);return result
def gallery():
 photos=[('Before/05-H3-exploration.png','Trước — build cũ chạy mới trong lượt này, 540×960'),('Touch07/05-H3-exploration.png','Sau — build 09, same world spawn/zoom; camera framing +1.2, 540×960'),('Touch07/H3-near-eye-before-pickup.png','Mắt Đèn riêng + glow, trước khi nhặt'),('Touch07/06-H3-part-picked-up.png','Nhặt bằng cảm ứng — thông báo động'),('Desktop06/H3-desktop-1280x720.png','PC ngang 1280×720'),('RestartTouch03/H3-restart-item-stays-collected.png','Tiến trình mới — đồ không xuất hiện lại'),('Touch07/H3-return-map-progress.png','Trở về City Map và trả cô Mai')]
 html='<!doctype html><meta charset="utf-8"><title>LBC H3 runtime before/after</title><style>body{font:17px system-ui;background:#173f48;color:#ffe5b5;margin:24px}a{color:#80dfce}.cards{display:flex;flex-wrap:wrap;gap:18px}figure{margin:0;width:340px}img{max-width:100%}figcaption{padding:10px}</style><h1>H3 — runtime trước/sau</h1><p>Ảnh mới trong lượt H3, không dùng ảnh QA cũ để chứng minh bản mới. <a href="H3UpgradeReport.md">Báo cáo / phần chưa làm</a></p><div class="cards">'
 for path,caption in photos:
  assert (V/path).is_file();url='Validation/H3Upgrade-20261017/'+path;html+=f'<figure><a href="{url}"><img src="{url}"></a><figcaption>{caption}</figcaption></figure>'
 (R/'H3UpgradeReview.html').write_text(html+'</div>',encoding='utf-8')
def checkpoint():
 result=verify();gallery();manifest=read(B/'CHECKPOINT_MANIFEST.json');base={e['path']:e['sha256'] for e in manifest['entries']}
 paths=set(base)
 for folder in ['Assets','Packages','ProjectSettings','Docs','Tools','References','Builds/Windows']:
  paths.update(p.relative_to(R).as_posix() for p in (R/folder).rglob('*') if p.is_file() and '__pycache__' not in p.parts)
 paths.update(p.relative_to(R).as_posix() for p in R.glob('*.md'));paths.add('H3UpgradeReview.html')
 # Never bundle historical validation trees. Only this task's final proof +
 # compact diagnostic logs and before frame are additions to the full base.
 for run in RUNS:
  paths.update(p.relative_to(R).as_posix() for p in (V/run).rglob('*') if p.is_file())
 paths.update(['Validation/H3Upgrade-20261017/Before/05-H3-exploration.png','Validation/H3Upgrade-20261017/Before/06-H3-part-picked-up.png','Validation/H3Upgrade-20261017/Touch01/smoke-results.txt','Validation/H3Upgrade-20261017/Touch01/failure-runtime.png'])
 paths.update(p.relative_to(R).as_posix() for p in (R/'Logs').glob('h3-*.log'))
 changed=[p for p in sorted(paths) if (R/p).is_file() and (p not in base or sha(R/p)!=base[p])]
 deleted=[p for p in base if not (R/p).is_file()]
 size=sum((R/p).stat().st_size for p in changed);print('INCREMENTAL ESTIMATE',len(changed),size,flush=True)
 assert size<1024**3,'Unexpected >1GiB delta: review before copying'
 target=B.parent/'CheckpointH3VisualUpgrade-20261017';assert not target.exists(),target
 entries=[]
 for name in changed:
  src=R/name;dst=target/name;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);digest=sha(src);assert digest==sha(dst);entries.append(dict(path=name,sha256=digest,bytes=src.stat().st_size))
 for e in entries:assert sha(R/e['path'])==e['sha256'],'Source changed during checkpoint'
 write(target/'CHECKPOINT_MANIFEST.json',dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseCheckpoint=str(B),baseManifestSHA256=sha(B/'CHECKPOINT_MANIFEST.json'),restore='Restore verified base first, overlay this checkpoint entries, then remove only deletedPaths. Library is regenerated; this is not a standalone full backup.',changedFiles=len(entries),totalBytes=size,entries=entries,deletedPaths=deleted,tests=result['runs'],status=result['status'],queuePreserved=True))
 print('CHECKPOINT VERIFIED',target,len(entries),size,flush=True)
if __name__=='__main__':
 if '--checkpoint' in sys.argv:checkpoint()
 else:
  result=verify();gallery();print('PASS',result['assertions'],'checks; source/physics/identity preserved; final runtime proof ready.')
