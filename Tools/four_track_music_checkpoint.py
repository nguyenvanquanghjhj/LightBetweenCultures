"""Bounded music checkpoint. Requires final Windows results; never scans caches/backups."""
from pathlib import Path
import hashlib,json,re,shutil
from datetime import datetime,timezone
ROOT=Path(__file__).resolve().parents[1]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
 d=ROOT/'Docs/QA/FourTrackMusic-20261018'
 runs=['fresh04','restart01']
 for run in runs:
  p=ROOT/'Validation/FourTrackMusic-20261018'/run/'smoke-results.txt'
  assert p.read_text(encoding='utf-8-sig').splitlines()[-1]=='ALL CHECKS PASSED',run
 checks=json.loads((d/'protected-files.json').read_text())
 for e in checks:assert sha(ROOT/e['path'])==e['sha256'],e['path']
 for e in json.loads((d/'asset-references.json').read_text()):
  assert sha(ROOT/e['path'])==e['sha256']
  assert re.search(r'guid: (\w+)',(ROOT/(e['path']+'.meta')).read_text())[1]==e['guid']
 settings=ROOT/'ProjectSettings/ProjectSettings.asset';before=d/'before/ProjectSettings/ProjectSettings.asset'
 pat=r'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
 assert re.sub(pat,'',settings.read_text(encoding='utf-8-sig'))==re.sub(pat,'',before.read_text(encoding='utf-8-sig')),'Unexpected project settings change'
 settings.write_bytes(before.read_bytes())
 verification=dict(protectedFiles=len(checks),allExact=True,originalMp3Count=4,originalMp3AndGuidsExact=True,projectSettingsExact=sha(settings)==sha(before),runs={r:sum(l.startswith('PASS:') for l in (ROOT/'Validation/FourTrackMusic-20261018'/r/'smoke-results.txt').read_text(encoding='utf-8-sig').splitlines()) for r in runs})
 (d/'verification.json').write_text(json.dumps(verification,indent=2))
 base=ROOT.parent/'Backups/CheckpointSoftSfx-20261018';manifest=base/'CHECKPOINT_MANIFEST.json';expected='a7db9b36613a7d39bce9d681137ba0185b7f0851078ffe5e7c386aa11916f4de'
 assert sha(manifest)==expected
 old=json.loads(manifest.read_text(encoding='utf-8-sig'));known={e['path']:e['sha256'] for e in old['entries']}
 # Verify exactly the pre-edit shared audio/runner files against the safe base.
 for s in ['Assets/_Game/Scripts/UI/SoftAudio.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs']:
  assert sha(base/s)==known[s] and sha(d/'before'/s)==known[s],s
 source=['Assets/_Game/Scripts/UI/SoftAudio.cs','Assets/_Game/Scripts/UI/SoftAudio.Music.cs','Assets/_Game/Scripts/UI/SoftMusicClips.cs','Assets/_Game/Resources/SoftMusicClips.asset','Assets/_Game/Scripts/Editor/FourTrackMusicSetup.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.Music.cs']
 files=[ROOT/s for s in source]+[ROOT/(s+'.meta') for s in source]
 files+=list((ROOT/'Assets/_Game/Audio/Music').glob('*.meta'))
 files += [ROOT/'Tools/qa_four_track_music.ps1',Path(__file__),ROOT/'Docs/QA/FOUR_TRACK_MUSIC_INTEGRATION.md',ROOT/'Docs/Gameplay/MASTER_EXECUTION_CHECKPOINT.md']
 files += [p for p in d.rglob('*') if p.is_file() and p.name!='checkpoint.json']
 for run in runs:files += [p for p in (ROOT/'Validation/FourTrackMusic-20261018'/run).iterdir() if p.is_file()]
 for run in ['fresh01','fresh02','fresh03']:
  files += [ROOT/'Validation/FourTrackMusic-20261018'/run/n for n in ['player.log','smoke-results.txt','input-provenance.json']]
 files+=list((ROOT/'Logs').glob('four-track-music-build*.log'))
 files=sorted(set(p for p in files if p.is_file() and known.get(p.relative_to(ROOT).as_posix())!=sha(p)))
 size=sum(p.stat().st_size for p in files);print('Incremental payload:',len(files),'files;',size,'bytes');assert size<60*1024**2
 dest=ROOT.parent/'Backups/CheckpointFourTrackMusic-20261018';assert not dest.exists()
 entries=[]
 for p in files:
  rel=p.relative_to(ROOT);q=dest/rel;q.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,q);assert sha(p)==sha(q)
  entries.append(dict(path=rel.as_posix(),sha256=sha(p),bytes=p.stat().st_size))
 result=dict(type='INCREMENTAL_REQUIRES_BASE',baseCheckpoint=str(base),baseManifestSHA256=expected,createdUTC=datetime.now(timezone.utc).isoformat(),summary='Four supplied streaming MP3; existing SoftAudio context/crossfade/loop; Windows targeted QA; Android and subjective listening not tested.',entries=entries,deletedPaths=[])
 out=dest/'CHECKPOINT_MANIFEST.json';out.write_text(json.dumps(result,indent=2))
 receipt=dict(checkpoint=str(dest),requiresBase=str(base),manifestSHA256=sha(out),files=len(entries),payloadBytes=size,allCopiedHashesVerified=True)
 (d/'checkpoint.json').write_text(json.dumps(receipt,indent=2));print(json.dumps(receipt,indent=2))
if __name__=='__main__':main()
