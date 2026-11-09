"""Finalize only this focused leisure visual patch; never rebuild or alter game data."""
from pathlib import Path
import argparse, datetime, hashlib, html, json, shutil
ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/QA/LeisureVisualFix-20261020'
VAL=ROOT/'Validation/LeisureVisualFix-20261020'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,value):p.write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
parser=argparse.ArgumentParser();parser.add_argument('--checkpoint',action='store_true');args=parser.parse_args()
b=read(DOC/'baseline.json')
protected=[dict(e,unchanged=sha(ROOT/e['path'])==e['sha256']) for e in b['protected']]
assert all(e['unchanged'] for e in protected),'Protected input changed: inspect, never undo unrelated work'
dump(DOC/'protection.json',dict(result='PASS',checks=protected))
main=(VAL/'player01/smoke-results.txt').read_text(encoding='utf-8-sig')
fresh=(VAL/'fresh01/smoke-results.txt').read_text(encoding='utf-8-sig')
assert 'FAIL: Native pilot framebuffer 720x1280' in main
assert 'Completion preserves full save: G10' in main and 'ALL CHECKS PASSED' in fresh
build=ROOT/'Logs/leisure-visuals-windows01.log'
assert 'PHASE5 WINDOWS BUILD: Succeeded' in build.read_text(encoding='utf-8-sig')
entries=read(ROOT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']
nodes=[dict(city=e['cityId'],node=e['nodeId'],activity=e['title']) for e in entries if e['enabled']]
images=[('Thả diều · viền gốc được giữ đầy đủ','player01/LV-D9-540-after.png'),('Đánh chuông · đầu chày tròn đầy đủ','player01/LV-G10-540-after.png')]
images += [(city,'player01/LV-markers-'+city+'.png') for city in dict.fromkeys(e['city'] for e in nodes)]
images += [('Save mới · không lộ leisure HIDDEN/PREVIEW','fresh01/LV-fresh-no-hidden-leak.png')]
for _,p in images:assert (VAL/p).is_file(),p
summary=dict(build='WINDOWS_PASS',runtime540='TARGETED_PASS',runtime720='BLOCKED_QA_NATIVE_WINDOW_720x960_INSTEAD_OF_720x1280',android='NOT_TESTED',mainRun='PARTIAL: framebuffer assertion after all 540 checks',freshRun='PASS',assertionsPassed=main.count('PASS:')+fresh.count('PASS:'),markers=nodes,protectedInputs=len(protected),sourceArt='UNCHANGED',ownerVisualApproval='PENDING',scope='Kite/bell extraction masks and secondary leisure star badges only')
dump(DOC/'qa-summary.json',summary)
cards=''.join('<figure><figcaption>'+html.escape(t)+'</figcaption><a href="../../../Validation/LeisureVisualFix-20261020/'+p+'"><img loading="lazy" src="../../../Validation/LeisureVisualFix-20261020/'+p+'"></a></figure>' for t,p in images)
(DOC/'gallery.html').write_text('<!doctype html><meta charset="utf-8"><title>LBC · Leisure visual fix</title><style>body{background:#173238;color:#fff5da;font:17px system-ui;margin:24px}main{display:flex;flex-wrap:wrap;gap:22px}figure{margin:0;width:min(100%,540px)}img{width:100%;height:auto}figcaption{padding:12px 0}a{color:#ffd88b}</style><h1>Leisure visual fix · Windows Player</h1><p>Ảnh runtime thật, 540×960. Bấm ảnh để mở PNG gốc. Viền diều và đầu chày lấy nguyên từ sprite đã có; không vẽ lại art.</p><p>Build Windows PASS; thao tác tại 540×960 PASS. 720×1280: BLOCKED do cửa sổ QA chỉ đạt 720×960; Android NOT_TESTED.</p><main>'+cards+'</main>',encoding='utf-8')
changed=[e['path'] for e in b['inputs'] if sha(ROOT/e['path'])!=e['sha256']]
changed += ['Assets/_Game/Scripts/Validation/GameplaySmokeRunner.LeisureVisuals.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.LeisureVisuals.cs.meta','Tools/leisure_visual_fix.py']
dump(DOC/'changed-files.json',[dict(path=p,sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in changed])
print(json.dumps(summary,ensure_ascii=True,indent=2))
if not args.checkpoint:raise SystemExit(0)
base=Path(b['baseCheckpoint']);manifest=read(base/'CHECKPOINT_MANIFEST.json')
assert sha(base/'CHECKPOINT_MANIFEST.json')==b['baseManifestSHA256']
for e in manifest['entries']:assert sha(base/e['path'])==e['sha256']
chain=[dict(path=str(base),manifestSHA256=b['baseManifestSHA256']),*manifest['baseChain']]
for c in chain:assert sha(Path(c['path'])/'CHECKPOINT_MANIFEST.json')==c['manifestSHA256']
paths=set(changed)|{'Docs/QA/LEISURE_VISUAL_FIX_20261020.md','Logs/leisure-visuals-windows01.log'}
paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in DOC.iterdir() if p.is_file() and p.name!='checkpoint.json')
for folder in [VAL/'player01',VAL/'fresh01']:
    paths.update(str(p.relative_to(ROOT)).replace('\\','/') for p in folder.iterdir() if p.is_file())
paths.add('Validation/LeisureVisualFix-20261020/fresh01.log')
payload=[dict(path=p,sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(paths)]
size=sum(e['bytes'] for e in payload)
print('Checkpoint estimate:',len(payload),'files,',round(size/1024**2,2),'MiB')
assert size<30*1024**2
dest=Path('D:/GAMEKKK/Backups/CheckpointLeisureVisualFix-20261020')
assert not dest.exists();dest.mkdir()
for e in payload:
    target=dest/e['path'];target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(ROOT/e['path'],target)
    assert sha(target)==e['sha256'] and sha(ROOT/e['path'])==e['sha256']
(dest/'RESTORE.md').write_text('INCREMENTAL, NOT STANDALONE. Requires CheckpointGameFeel-20261020 and its base chain. Overlay only these project-relative files after restoring the chain. No deleted paths. Do not restore over newer unrelated work. No Library, Builds, caches, normal player saves or historical evidence trees included. Windows build passed; 540x960 targeted runtime passed. 720x1280 blocked by QA window sizing, Android not tested. Stop for owner review.\n',encoding='utf-8')
dump(dest/'CHECKPOINT_MANIFEST.json',dict(type='INCREMENTAL_REQUIRES_BASE',createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseCheckpoint=str(base),baseManifestSHA256=b['baseManifestSHA256'],baseChain=chain,entries=payload,bytes=size,deletedPaths=[],restoreGuideSHA256=sha(dest/'RESTORE.md'),summary=summary['scope']))
dump(DOC/'checkpoint.json',dict(path=str(dest),manifestSHA256=sha(dest/'CHECKPOINT_MANIFEST.json'),files=len(payload),bytes=size,standalone=False,requiresBase=str(base),payloadVerified=True))
print('Checkpoint verified:',dest)
