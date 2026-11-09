"""Phase7 bounded baseline/static inventory; no production mutations or deletions."""
from pathlib import Path
import datetime, hashlib, json, math, re, shutil
ROOT=Path(__file__).resolve().parents[1]; DOC=ROOT/'Docs/QA/Phase7'; DOC.mkdir(parents=True,exist_ok=True)
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,v:p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
base=read(ROOT/'Docs/QA/FinalPolish-20261019/recovery-checkpoint.json')
checkpoint=Path(base['checkpoint']); assert sha(checkpoint/'CHECKPOINT_MANIFEST.json')==base['manifestSHA256']
original=Path('C:/Users/admin/.codex/attachments/44c87118-8e2a-43a8-b802-83ccdc084bda/Pasted text.txt')
if not (DOC/'APPROVED_PROMPT.md').exists():shutil.copy2(original,DOC/'APPROVED_PROMPT.md')
assert sha(original)==sha(DOC/'APPROVED_PROMPT.md')
files=set()
prior=read(ROOT/'Docs/QA/FinalPolish-20261019/baseline.json')
files.update(e['path'] for e in prior['protected'])
for parent,patterns in [('Assets/_Game/Scripts',['*.cs','*.meta']),('Assets/_Game/Resources',['*.json','*.asset']),('Assets/_Game/Scenes',['*.unity','*.meta']),('ProjectSettings',['*.asset','ProjectVersion.txt']),('Packages',['manifest.json','packages-lock.json'])]:
 for pattern in patterns:files.update(p.relative_to(ROOT).as_posix() for p in (ROOT/parent).rglob(pattern) if p.is_file())
files.discard('Assets/_Game/Scripts/Editor/Phase7Audit.cs');files.discard('Assets/_Game/Scripts/Editor/Phase7Audit.cs.meta')
entries=[dict(path=p,sha256=sha(ROOT/p),bytes=(ROOT/p).stat().st_size) for p in sorted(files)]
if not (DOC/'baseline.json').exists():dump(DOC/'baseline.json',dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),baseCheckpoint=str(checkpoint),baseManifestSHA256=base['manifestSHA256'],entries=entries,buildAssemblySHA256=sha(ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'),allowProductionChanges=[]))
else:assert all(sha(ROOT/e['path'])==e['sha256'] for e in read(DOC/'baseline.json')['entries']), 'Frozen source changed: inspect before proceeding'
content=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json'); relics=read(ROOT/'Assets/_Game/Resources/Content/CulturalRelics.json')['relics']
findings=[]; cities=[]
for c in content['cities']:
 ids=[n['id'] for n in c['nodes']];adj={x:set() for x in ids}
 assert len(ids)==len(set(ids)) and c['spawnNodeId'] in ids
 for e in c['edges']:
  assert e['from'] in ids and e['to'] in ids
  adj[e['from']].add(e['to'])
  if e.get('bidirectional'):adj[e['to']].add(e['from'])
  for v in e.get('waypoints',[]):assert all(math.isfinite(v[k]) and 0<=v[k]<=1 for k in ['x','y'])
 seen=set();todo=[c['spawnNodeId']]
 while todo:
  n=todo.pop()
  if n in seen:continue
  seen.add(n);todo.extend(adj[n]-seen)
 assert len(seen)==len(ids),c['id']+' disconnected without gates'
 cities.append(dict(city=c['id'],nodes=len(ids),edges=len(c['edges']),maxNeighbors=max(map(len,adj.values())),reachableIgnoringGates=len(seen),unlockFlags=c['unlockFlags'],background=c['sourceBackground']))
activities=read(ROOT/'Assets/_Game/Resources/Content/StoryInteractions.json')['entries']; leisure=read(ROOT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']
assert len(relics)==6 and sum(len(r['fragmentIds']) for r in relics)==24
assert len(activities)==22 and sum(e['kind']=='STORY_MINIGAME' for e in activities)==10 and len(leisure)==12
assert all(any(c['id']==e['cityId'] and any(n['id']==e['nodeId'] for n in c['nodes']) for c in content['cities']) for e in leisure)
for e in leisure:assert (ROOT/'Assets/_Game/Resources'/(e['spritePath']+'.png')).exists()
src=list((ROOT/'Assets/_Game/Scripts').rglob('*.cs'))
for p in src:
 t=p.read_text(encoding='utf-8-sig');rp=p.relative_to(ROOT).as_posix()
 if 'using UnityEditor' in t and '/Editor/' not in rp and '/Validation/' not in rp:findings.append(dict(path=rp,kind='EDITOR_API_REVIEW'))
 if 'DllImport' in t:findings.append(dict(path=rp,kind='PLATFORM_GUARD_REVIEW'))
 if 'Resources.Load' in t:findings.append(dict(path=rp,kind='DYNAMIC_LOADING_PROTECTED'))
counts=dict(cities=6,nodes=sum(c['nodes'] for c in cities),edges=sum(c['edges'] for c in cities),quests=len(content['quests']),puzzles=len(content['puzzles']),checkIns=len(content['checkIns']),items=len(content['items']),events=len(content['events']),relics=len(relics),fragments=sum(len(r['fragmentIds']) for r in relics),storyMinigames=10,shortInteractions=12,leisure=12,scripts=len(src))
dump(DOC/'static-inventory.json',dict(counts=counts,cities=cities,activities=activities,leisure=leisure,sourceReviewCandidates=findings,note='Graph validity is not terrain acceptance. No protected geometry changed.'))
packages=read(ROOT/'Packages/manifest.json')['dependencies']
settings=(ROOT/'ProjectSettings/ProjectSettings.asset').read_text(encoding='utf-8-sig')
fields={k:re.search(r'^  '+k+r':(.*)',settings,re.M)[1].strip() for k in ['companyName','productName','bundleVersion','defaultScreenOrientation','AndroidMinSdkVersion','AndroidTargetSdkVersion','AndroidTargetArchitectures']}
scenes=re.findall(r'^    path: (.*)',(ROOT/'ProjectSettings/EditorBuildSettings.asset').read_text(),re.M)
dump(DOC/'platform-inventory.json',dict(settings=fields,buildScenes=scenes,packages=packages,assemblies=['Assembly-CSharp','Assembly-CSharp-Editor','Unity.2D.Welcome (package welcome)'],git='NOT_CONFIGURED',addressables='No Addressables package in manifest; Resources and serialized bindings used',streamingAssets=(ROOT/'Assets/StreamingAssets').exists()))
(DOC/'PHASE7_BASELINE.md').write_text(f'''# Phase7 baseline — 2026-10-19

Unity 6000.6.5f1, latest live source after CheckpointFinalPolishRecovery-20261019. Base manifest SHA256 `{base['manifestSHA256']}`. No Git repository; no checkout/reset. At preflight only Unity Hub service PID26752, no active Editor/build/Player/concurrent agent edits observed. The private Phase7 Player launched after preflight is ours.

## Frozen world and implemented content

`{counts}`

Runtime data confirms six cities, 133 nodes, 155 edges,29 quests,22 historic IDs split into10 real story minigames and12 short interactions,18 check-ins,six relics/24 fragments,12 leisure panels. JSON/event dispatch and runtime implementation exist; this inventory does not assign execution PASS.

Frozen fingerprint: baseline.json ({len(entries)} scoped files), protected original manifest retained separately. No full project/Backups scan. Includes scripts/content/scene/settings and prior scoped canonical-art/audio hashes. Route CLOSED_BY_OWNER — ACCEPTED_WITH_KNOWN_LIMITATIONS. No terrain redesign authorized.

## Source/build status inherited

All nine existing-file changes from interrupted final polish remain. recovery-preservation.json and recovered-source.diff list exact changes. Latest current Windows Development build02 succeeded 12:08:24 UTC+07; compiled source is compatible. Initial errors:0 compiler errors reported by successful build; raw warning counts will be extracted from actual log, not inferred as zero.

Current assembly SHA256 `{sha(ROOT/'Builds/Windows/LightBetweenCultures_Data/Managed/Assembly-CSharp.dll')}`. Reuse this build for runtime QA until a runtime-source change actually justifies rebuild. Phase7 starts with QA/tooling changes only.

## Inherited evidence and pending checks

- Recovery: tea/snack157 actual Windows assertions,540×960/720×1280, restart, 20 fresh frames. Source unchanged; preserve as compatible evidence.
- Phase6: source/import fix evidence,12 native portrait screenshots covering540×960,720×1280,1080×1920,1080×2400. UI dependencies will be compared before reuse; glow/leisure differences labelled.
- Route correction:68 traversals reported; verify frozen route bytes and evidence rather than rerun geometry work.
- BGM/SFX integration: actual Windows loop/mute/battle results; compare audio dependencies before reuse.
- Newly pending: full fresh VN_FIRST/KOREA_FIRST with current22 interactions, glow-after six cities, all12 leisure current dependent components, ending states/restart. Earlier Phase7-wide `NOT_TESTED` statuses will be updated only from evidence.

## Platform and packaging

{fields}

Build scenes: {scenes}. SampleScene is included by existing build settings, not assumed disposable. Android module/SDK/NDK/OpenJDK exist; genuine build/device checks pending. No release APK or device PASS assumed. DefaultCompany/product/icon and package settings need release review. No supplied competition rulebook identified yet.

## Historical limitations and art

Protected route limitations: U-E018,U-E022,D-E020,D-E025,J-E026,G-E004,G-E006,S-E002,S-E016,S-E009; S-E017 preserved. Standing swing and approved boat retained. Lightweight wing/standing poses are presentation limitations, not permission for new art. Inspect latest registry for heritage_data/jiwon_sync/stellar_ring instead of relying on historical ART_PENDING labels.

Original prompt archived verbatim as APPROVED_PROMPT.md. Milestone A: baseline recorded; static and native reference audits continue. No cleanup performed.
''',encoding='utf-8')
print('Frozen',len(entries),'files; counts',counts)
