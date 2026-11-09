"""Incremental checkpoint of the offline tool only. No build/cache/browser-profile copy."""
from pathlib import Path
import datetime,hashlib,json,shutil
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[1];DOC=ROOT/'Docs/QA/SixCityRouteEditor-20261019';V=ROOT/'Validation/SixCityRouteEditor-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def main():
    receipt=read(ROOT/'Docs/QA/AndroidApk-20261019/checkpoint-receipt.json');base=Path(receipt['checkpoint']);manifest=base/'CHECKPOINT_MANIFEST.json'
    assert sha(manifest)==receipt['manifestSHA256'];bm=read(manifest)
    for e in bm['entries']:assert sha(base/e['path'])==e['sha256'],e['path']
    for e in bm['baseChain']:assert sha(Path(e['path'])/'CHECKPOINT_MANIFEST.json')==e['manifestSHA256']
    frozen=read(HERE/'data/export-manifest.json')['protected']
    for e in frozen:assert sha(ROOT/e['path'])==e['sha256'],e['path']
    assert read(DOC/'verification.json')['status']=='TOOL_QA_PASS'
    target=Path('D:/GAMEKKK/Backups/CheckpointSixCityRouteEditor-20261019');assert not target.exists()
    files={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts}
    files.update(p.relative_to(ROOT).as_posix() for p in DOC.iterdir() if p.is_file() and p.name!='checkpoint-receipt.json')
    files.add('Docs/QA/SIX_CITY_ROUTE_EDITOR_20261019.md')
    files.update(p.relative_to(ROOT).as_posix() for p in (V/'run07').iterdir() if p.is_file())
    files.update(p.relative_to(ROOT).as_posix() for p in (V/'persistence').iterdir() if p.is_file())
    assert not any('isolated-browser-profile' in p or p.startswith(('Builds/','Library/','Temp/')) for p in files)
    entries=[dict(path=p,bytes=(ROOT/p).stat().st_size,sha256=sha(ROOT/p)) for p in sorted(files)];total=sum(e['bytes'] for e in entries)
    print('Incremental estimate',len(entries),'files',round(total/1048576,2),'MiB; includes required six offline PNG copies, no full project/cache/APK')
    assert total<64*1048576,'Unexpected growth; review before copying'
    target.mkdir()
    for e in entries:
        p=target/e['path'];p.parent.mkdir(exist_ok=True,parents=True);shutil.copy2(ROOT/e['path'],p);assert sha(p)==e['sha256'] and sha(ROOT/e['path'])==e['sha256']
    guide='''# Six-City Route Editor — incremental, NOT standalone game backup

Requires CheckpointAndroidApkBuild-20261019 and its complete baseChain for the
game. Restore bases oldest to newest into a separate recovery workspace,
then overlay this manifest preserving paths and verify SHA256.

This checkpoint adds only the offline Tools/SixCityRouteEditor package,
read-only source snapshot, six exact PNG copies, Vietnamese help and QA.
For using the editor alone, copy the complete Tools/SixCityRouteEditor folder
and open index.html in Edge. No Unity, server, network, npm or Python required
for normal use. Python scripts are optional export/QA tools.

Production source/assets/settings remain byte-exact. No deletions, importer,
route changes, Android build or Unity scene modifications in this task.
QA-only sample drawings are not approved routes and must never be applied.
Owner drawings live in browser storage/downloaded JSON and must be backed
up separately after each drawing session; this checkpoint starts blank.
STOP — wait for the owner to draw and approve JSON for a later import task.
'''
    (target/'RESTORE.md').write_text(guide,encoding='utf-8')
    m={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'baseCheckpoint':str(base),'baseManifestSHA256':sha(manifest),'baseChain':[{'path':str(base),'manifestSHA256':sha(manifest)}]+bm['baseChain'],'entries':entries,'bytes':total,'deletedPaths':[],'restoreGuideSHA256':sha(target/'RESTORE.md'),'summary':'Offline Six-City Route Editor;133 locked nodes/155 reference edges; blank new graph; tool QA PASS; no production/route/app changes'}
    write(target/'CHECKPOINT_MANIFEST.json',m)
    out={'checkpoint':str(target),'manifestSHA256':sha(target/'CHECKPOINT_MANIFEST.json'),'standalone':False,'baseCheckpoint':str(base),'baseManifestSHA256':sha(manifest),'files':len(entries),'bytes':total,'MiB':round(total/1048576,2),'baseContentVerified':True,'baseChainManifestVerified':True,'copiedFilesVerified':True,'productionChanged':[],'deletedPaths':[],'stop':'STOP_WAITING_FOR_OWNER_ROUTE_DESIGN'}
    write(DOC/'checkpoint-receipt.json',out);print(json.dumps(out,indent=2))
if __name__=='__main__':main()
