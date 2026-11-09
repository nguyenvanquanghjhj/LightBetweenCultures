"""Three real swing frames, bounded preservation and a compact optional delta."""
from pathlib import Path
import datetime,hashlib,html,json,re,shutil,sys
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[1];D=ROOT/'Docs/QA/SwingStanding-20261019';V=ROOT/'Validation/SwingStanding-20261019'
read=lambda p:json.loads(p.read_text('utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
b=read(D/'baseline.json');before=Path(b['before']);name='ProjectSettings/ProjectSettings.asset';old=(before/name).read_bytes();new=(ROOT/name).read_bytes()
rx=rb'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)';assert re.sub(rx,b'',old)==re.sub(rx,b'',new)
if old!=new:(ROOT/name).write_bytes(old)
changed=[e['path'] for e in b['protected'] if sha(ROOT/e['path'])!=e['sha256']];assert set(changed)<=set(b['editable'])-{name}
name='Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs';old=(before/name).read_text('utf-8');new=(ROOT/name).read_text('utf-8')
boat=lambda s:s.split('            if(boat)\n            {',1)[1].split('\n            else\n',1)[0]
assert boat(old)==boat(new) and old.split('        void Pose(float time)',1)[1]==new.split('        void Pose(float time)',1)[1]
name='Assets/_Game/Resources/Content/LeisureActivities.json';x=read(before/name);y=read(ROOT/name)
next(e for e in x['entries'] if e['nodeId']=='H9')['title']=next(e for e in y['entries'] if e['nodeId']=='H9')['title'];assert x==y
dump(D/'preservation.json',dict(result='PASS',unchanged=len(b['protected'])-len(changed),changed=changed,boat='APPROVED_BY_OWNER_UNCHANGED_NOT_RETESTED',otherTen='UNCHANGED_NOT_RETESTED',sharedMotionAndLayers='BYTE_EXACT'))
run=V/'player02';lines=(run/'smoke-results.txt').read_text('utf-8-sig').splitlines();assert 'ALL CHECKS PASSED' in lines
frames=read(run/'animation-frames.json');assert len(frames)==3
order=['left-extreme','center','right-extreme'];labels=['Nghiêng trái','Ở giữa','Nghiêng phải'];frames=sorted(frames,key=lambda f:order.index(f['moment']))
evidence=[];body='';strip=Image.new('RGB',(3*480,392),'#173a3c');draw=ImageDraw.Draw(strip);font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',16)
for i,f in enumerate(frames):
 p=run/f['file'];im=Image.open(p);assert im.size==(720,1280)
 evidence.append(dict(**f,sha256=sha(p),bytes=p.stat().st_size));body+=f'<section><h2>{labels[i]}</h2><a href="player02/{f["file"]}"><img src="player02/{f["file"]}"></a><p>{f["elapsed"]:.3f}s · {f["angle"]:.2f}° · {f["utc"]}</p></section>'
 w=720*.93*.89;h=w*.75;cx=f['backdropWorld']['x'];cy=1280-f['backdropWorld']['y'];crop=im.crop((round(cx-w/2),round(cy-h/2),round(cx+w/2),round(cy+h/2)));crop.thumbnail((480,360));strip.paste(crop.convert('RGB'),(480*i,0));draw.text((480*i+7,365),labels[i]+f' · {f["angle"]:.2f}°',font=font,fill='white')
strip.save(V/'SwingStanding-ThreePositions.png')
(V/'Gallery.html').write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>Lumi đứng trên xích đu</title><style>body{background:#173a3c;color:#fff1d2;font:17px system-ui;margin:24px}a{color:#9ee8de}img{max-width:100%}main{display:flex;flex-wrap:wrap;gap:20px}section{max-width:440px}</style><h1>Chơi xích đu · Lumi toàn thân</h1><p>Ba frame Windows Player mới 720×1280. Giàn/nền đứng yên, Lumi cùng parent với ghế. Thuyền vịt APPROVED: giữ nguyên, không chạy lại QA.</p><a href="SwingStanding-ThreePositions.png"><img src="SwingStanding-ThreePositions.png"></a><main>'+body+'</main></html>',encoding='utf-8')
dump(D/'evidence.json',dict(result='TARGETED_WINDOWS_PASS',assertions=sum(l.startswith('PASS:') for l in lines),frames=evidence,boat='NOT_RETESTED_APPROVED_UNCHANGED',android='NOT_TESTED',diagnostic='player01 failed a source-PNG versus Unity sprite-rect contact check. Corrected sole pivot, scale and QA conversion; old capture retained, not used for acceptance.'))
if '--checkpoint' not in sys.argv:print('Gallery + preservation ready');raise SystemExit(0)
base=Path(b['baseCheckpoint']);assert sha(base/'CHECKPOINT_MANIFEST.json')==b['baseManifestSHA256'];bm=read(base/'CHECKPOINT_MANIFEST.json');chain=[dict(path=str(base),manifestSHA256=b['baseManifestSHA256']),*bm['baseChain']]
for c in chain:assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256']
paths=set(changed);paths.add('Tools/finalize_swing_standing.py')
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for folder in [D,V] for p in folder.rglob('*') if p.is_file() and p.name!='checkpoint.json')
paths.update('Logs/swing-standing-build'+n+'.log' for n in ['01','02'])
entries=[dict(path=p,sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(paths)];size=sum(e['bytes'] for e in entries);print('Incremental estimate:',len(entries),'files',round(size/2**20,2),'MiB');assert size<15*2**20
dest=Path('D:/GAMEKKK/Backups/CheckpointSwingStanding-20261019');assert not dest.exists() and dest.parent.resolve()==Path('D:/GAMEKKK/Backups').resolve();dest.mkdir()
for e in entries:
 p=dest/e['path'];p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/e['path'],p);assert sha(p)==e['sha256']
(dest/'RESTORE.md').write_text('INCREMENTAL, NOT STANDALONE. Restore CheckpointLeisureActivitiesFullReview-20261019 and its base chain, then overlay these paths. No deleted paths. Only swing presentation/title and bounded QA changed; approved boat and other ten activities remain unchanged. No source art, Build/Library/Temp, global saves or older QA trees copied. Rebuild using existing Phase5Setup.Build; use --gameplay-smoke --swing-standing --gameplay-output=<private QA folder> and earned H9 seed. STOP after swing review.\n',encoding='utf-8')
dump(dest/'CHECKPOINT_MANIFEST.json',dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],baseChain=chain,entries=entries,deletedPaths=[],bytes=size,restoreGuideSHA256=sha(dest/'RESTORE.md'),summary='H9 full-body canonical Lumi standing on swing seat; existing swing motion retained, boat owner-approved unchanged, three new Windows frames and targeted QA.'))
dump(D/'checkpoint.json',dict(checkpoint=str(dest),manifestSHA256=sha(dest/'CHECKPOINT_MANIFEST.json'),baseCheckpoint=str(base),standalone=False,bytes=size,files=len(entries),result='TARGETED_WINDOWS_PASS',stop='STOP_WAIT_FOR_OWNER_REVIEW'));print(dest)
