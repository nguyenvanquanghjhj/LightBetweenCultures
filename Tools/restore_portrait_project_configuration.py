"""Restore only configuration unintentionally regenerated during Unity startup.
Archive every current configuration first; never delete source scenes or assets.
"""
from pathlib import Path
import hashlib,json,shutil
R=Path(__file__).resolve().parents[1]
B=R.parent/'Backups/CheckpointTerrainRoutes-20261017'
V=R/'Validation/PortraitStory-20261017/UnityAutoSettingsBeforeRestore'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
m=json.loads((B/'CHECKPOINT_MANIFEST.json').read_text(encoding='utf-8-sig'))
entries=[e for e in m['entries'] if e['path'].startswith(('Packages/','ProjectSettings/'))]
known={e['path'] for e in entries};rows=[]
assert not V.exists(),'Configuration archive already exists; inspect before repeating.'
for area in ['Packages','ProjectSettings']:
    for p in (R/area).rglob('*'):
        if not p.is_file():continue
        assert p.resolve().is_relative_to((R/area).resolve()),p
        relative=p.relative_to(R).as_posix();dst=V/relative
        dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dst);assert sha(p)==sha(dst)
        if relative not in known:
            # Preserve generated extras outside the active configuration folder.
            moved=V/'generated-extras'/relative;moved.parent.mkdir(parents=True,exist_ok=True)
            assert moved.resolve().is_relative_to(V.resolve())
            shutil.move(str(p),str(moved));rows.append(dict(path=relative,action='archived_generated_extra',sha256=sha(moved)))
for e in entries:
    src=B/e['path'];dst=R/e['path'];assert sha(src)==e['sha256']
    previous=sha(dst) if dst.is_file() else None
    dst.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(src,dst);assert sha(dst)==e['sha256']
    if previous!=e['sha256']:rows.append(dict(path=e['path'],action='restored_checkpoint_bytes',beforeSHA256=previous,afterSHA256=e['sha256']))
for e in entries:assert sha(R/e['path'])==e['sha256']
(V/'restore-manifest.json').write_text(json.dumps(dict(baseline=str(B),protectedConfigurationFiles=len(entries),changes=rows),indent=2)+'\n',encoding='utf-8')
print('Restored and SHA-verified',len(entries),'original configuration files;',len(rows),'drift entries archived.')
