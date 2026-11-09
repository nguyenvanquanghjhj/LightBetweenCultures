"""Small, explicit incremental checkpoint. Never copies builds/caches or deletes."""
import argparse, hashlib, json, shutil
from datetime import datetime, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/Gameplay/Phase5-20261018'
TARGET = Path('D:/GAMEKKK/Backups/CheckpointPhase5Final-20261019')
baseline = json.loads((OUT/'baseline.json').read_text(encoding='utf-8-sig'))
before = Path(baseline['before'])
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p, data): p.write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')

base = Path(baseline['baseCheckpoint'])
assert digest(base/'CHECKPOINT_MANIFEST.json') == baseline['baseManifestSHA256']
manifest = json.loads((base/'CHECKPOINT_MANIFEST.json').read_text(encoding='utf-8-sig'))
for entry in manifest['entries']:
    assert digest(base/entry['path']) == entry['sha256'], entry['path']
# Verify only the known manifest chain, not entire historical backup contents.
chain=[]; cursor=base; seen=set()
while cursor not in seen:
    seen.add(cursor)
    m=json.loads((cursor/'CHECKPOINT_MANIFEST.json').read_text(encoding='utf-8-sig'))
    chain.append({'path':str(cursor),'manifestSHA256':digest(cursor/'CHECKPOINT_MANIFEST.json')})
    parent=m.get('baseCheckpoint')
    if not parent: break
    parent=Path(parent)
    expected=m.get('baseManifestSHA256')
    if expected: assert digest(parent/'CHECKPOINT_MANIFEST.json')==expected, str(parent)
    cursor=parent

changed=[]
for entry in baseline['entries']:
    p=ROOT/entry['path']
    if digest(p) != entry['sha256']: changed.append(entry['path'])
supplement='Docs/Art/SIX_RELICS_ART_MANIFEST.md'
assert (ROOT/supplement).read_bytes().startswith((before/supplement).read_bytes())
changed.append(supplement)
new=[
 'Assets/_Game/Scripts/Content/NodeEventManager.FinalConnection.cs',
 'Assets/_Game/Scripts/UI/GameplayPanels.FinalConnection.cs',
 'Assets/_Game/Scripts/Editor/Phase5Setup.cs',
 'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Phase5.cs',
 'Assets/_Game/Resources/Content/FinalConnectionPresentation.json']
new += [p+'.meta' for p in new]
new += ['Tools/phase5_validation.ps1','Tools/phase5_suite.ps1','Tools/phase5_verify.py','Tools/phase5_report.py','Tools/phase5_checkpoint.py',
 'Docs/Prompts/LightBetweenCultures_Phase5_20261018.md','Docs/Gameplay/PHASE5_IMPLEMENTATION_REPORT.md',
 'Docs/QA/PHASE5_ENDING_E2E.md','Docs/QA/PHASE5_RUNTIME_REVIEW.html']
write(OUT/'changed-files.json', {'modified':sorted(set(changed)), 'added':sorted(new), 'deleted':[], 'note':'Task docs, evidence and before/protected records are additionally listed in the exact checkpoint manifest. Receipt is external to avoid self-referential manifest hash.'})

payload=set(changed+new)
payload.update(str(p.relative_to(ROOT)).replace('\\','/') for p in OUT.iterdir() if p.is_file() and p.name!='last-checkpoint.json')
summary=json.loads((OUT/'run-summary.json').read_text(encoding='utf-8-sig'))
for run in summary['allRetainedRunsIncludingFailures']:
    folder=ROOT/'Validation/Phase5-20261018'/run['run']
    for p in folder.iterdir():
        if p.is_file() and p.suffix in ('.txt','.log','.json'):
            payload.add(p.relative_to(ROOT).as_posix())
    # Historical failures retain their specific screenshots in this checkpoint.
    if run['result']=='FAIL':payload.update(p.relative_to(ROOT).as_posix() for p in folder.glob('*.png'))
gallery=(ROOT/'Docs/QA/PHASE5_RUNTIME_REVIEW.html').read_text(encoding='utf-8-sig')
import re
payload.update(re.findall(r'src="../../([^\"]+\.png)"',gallery))
payload.update(['Logs/phase5-build01.log','Logs/phase5-build07.log'])
for p in payload:assert (ROOT/p).is_file(),p
size=sum((ROOT/p).stat().st_size for p in payload)
print('Planned incremental:',len(payload),'files,',round(size/1048576,2),'MiB; no Library/Temp/Builds; base requires',base)
args=argparse.ArgumentParser();args.add_argument('--apply',action='store_true');options=args.parse_args()
if not options.apply:raise SystemExit(0)
assert not TARGET.exists(), 'Refuse to overwrite checkpoint'
resolved=TARGET.resolve();assert resolved.parent==Path('D:/GAMEKKK/Backups').resolve()
TARGET.mkdir()
entries=[]
for name in sorted(payload):
    source=ROOT/name; destination=TARGET/name
    destination.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source,destination)
    sha=digest(source);assert digest(destination)==sha
    entries.append({'path':name,'sha256':sha,'bytes':source.stat().st_size})
guide='''# Phase5 incremental restore

This is NOT a standalone project. Do not replace the live project with this folder.
Requires CheckpointSixRouteReferenceReview-20261018 and its recorded base chain
(FourTrackMusic -> SoftSfx -> MapNpcMarkers -> HoiAnStateB20 -> Phase4Final -> earlier restore chain).
The exact ancestor manifest paths/hashes are in CHECKPOINT_MANIFEST.json.baseChain.

To restore into a separate recovery folder:
1. Close Unity/Player and preserve any newer work. Start from the verified standalone
   restore point identified by the ancestor manifests. Apply each required incremental
   ancestor in chronological base-to-child order, honoring its deletedPaths.
2. Verify each ancestor manifest SHA256 before applying it. This tool verified the
   latest base payload and the known ancestor manifest chain, not every historical file.
3. Overlay ONLY the relative paths in this manifest's entries onto that recovery project.
   Verify SHA256 of every copied file. This checkpoint has an empty deletedPaths list.
   Keep paired .meta files; do not regenerate GUIDs.
4. No user save is part of the gameplay payload. Validation/*/gameplay-save.json and
   P5/P4 JSON snapshots are private QA fixtures/earned test saves; NEVER copy them into
   the player's normal persistent-data directory. Original user/history saves untouched.
5. Open recovery project with Unity 6000.6.5f1. Build using Phase5Setup.Build or the
   existing build flow. Library/Temp/Builds are intentionally not copied here.
6. Docs/Gameplay/Phase5-20261018/last-checkpoint.json is an external receipt pointing
   at this manifest (excluded from payload to avoid a circular hash). Recreate it from
   the manifest hash if needed; this has no runtime effect.

Evidence selection: all this task's run results/logs/private test saves/provenance,
21 gallery originals, four failed-run diagnostic captures and build01/build07 logs.
Other intermediate captures remain untouched in the live project's Validation folder;
this is not a copy of the entire historical Validation tree. No images/saves deleted.
BeforePhase5-20261018 is a separate scoped pre-edit snapshot, not a standalone backup.
'''
(TARGET/'RESTORE.md').write_text(guide,encoding='utf-8')
result={'type':'INCREMENTAL_REQUIRES_BASE','createdUTC':datetime.now(timezone.utc).isoformat(),
 'baseCheckpoint':str(base),'baseManifestSHA256':baseline['baseManifestSHA256'],'baseChain':chain,
 'summary':'Phase5 one canonical ending; guarded S21, independent SQ03, six-light ceremony24s/Skip/Cancel, additive save flag, preserved art/audio/graph/gameplay. Windows targeted PASS; visual acceptance pending; Android not tested; STOP.',
 'entries':entries,'deletedPaths':[], 'bytes':size,'restoreGuideSHA256':digest(TARGET/'RESTORE.md')}
write(TARGET/'CHECKPOINT_MANIFEST.json',result)
receipt={'checkpoint':str(TARGET),'manifestSHA256':digest(TARGET/'CHECKPOINT_MANIFEST.json'),'baseCheckpoint':str(base),
 'baseManifestSHA256':baseline['baseManifestSHA256'],'standalone':False,'files':len(entries),'bytes':size,
 'result':'WINDOWS_TARGETED_PASS','visualAcceptance':'PENDING','androidBuild':'NOT_TESTED','androidDevice':'NOT_TESTED',
 'stop':'Phase5 only; owner ending acceptance before any other work.'}
write(OUT/'last-checkpoint.json',receipt)
print('Verified checkpoint:',str(TARGET),'manifest',receipt['manifestSHA256'])
