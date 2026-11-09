"""Generate bounded, evidence-backed Phase7 reports and an offline native-frame gallery."""
from pathlib import Path
import csv, json, hashlib, re, html, os, difflib
from datetime import datetime, timezone

R=Path(__file__).resolve().parents[1]; Q=R/'Docs/QA/Phase7'; V=R/'Validation/Phase7-20261019'
def load(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def out(name,text):(Q/name).write_text(text.strip()+'\n',encoding='utf-8')
def dump(name,data):out(name,json.dumps(data,ensure_ascii=False,indent=2))
def table(headers,rows):return '| '+' | '.join(headers)+' |\n|'+'|'.join('---' for _ in headers)+'|\n'+'\n'.join('| '+' | '.join(str(c).replace('|',' / ').replace('\n',' ') for c in row)+' |' for row in rows)
def rel(p):return p.relative_to(R).as_posix()
def csvfile(name,headers,rows):
    with (Q/name).open('w',encoding='utf-8-sig',newline='') as f:
        w=csv.DictWriter(f,fieldnames=headers);w.writeheader();w.writerows(rows)

data=load(Q/'static-inventory.json'); content=load(R/'Assets/_Game/Resources/Content/GameplayContent.json')
runs=load(Q/'run-summary.json'); base=load(Q/'baseline.json');delta=load(Q/'final-protected-diff.json')
native=load(Q/'native-asset-audit.json'); platform=load(Q/'platform-inventory.json')
frames=load(V/'leisure01/frames.json'); vn=load(V/'vn01/FP-story-final.json'); kr=load(V/'korea01/FP-story-final.json')
hist=Path('D:/GAMEKKK/Backups/CheckpointFullStoryComplete-20261017'); old=load(hist/'Assets/_Game/Resources/Content/GameplayContent.json')
historical=load(R/'Validation/FullStory-20261017/Fresh04/full-story-final.json')
assert set(q['id'] for q in content['quests'])<=set(historical['completed'])
quest_text_changes=[]
for q in content['quests']:
    earlier=next(e for e in old['quests'] if e['id']==q['id'])
    fields=[k for k in q.keys()|earlier.keys() if q.get(k)!=earlier.get(k)]
    assert set(fields)<=set(['offer','thanks','reminder']), (q['id'],fields)
    if fields:quest_text_changes.append({'quest':q['id'],'textOnly':fields})
inventory_path='Assets/_Game/Scripts/Core/Inventory.cs'; quest_path='Assets/_Game/Scripts/Core/QuestManager.cs'
assert sha(R/inventory_path)==sha(hist/inventory_path)
oldquest=(hist/quest_path).read_text(encoding='utf-8-sig'); nowquest=(R/quest_path).read_text(encoding='utf-8-sig')
questdiff=''.join(difflib.unified_diff(oldquest.splitlines(True),nowquest.splitlines(True)))
out('quest-manager-compatibility.diff',questdiff)
normalized=oldquest.replace('sealed class QuestManager','sealed partial class QuestManager').replace('Trạm Hội An đã sáng','Trạm giao lưu sẵn sàng')
assert normalized==nowquest, 'Reused quest proof needs a new dependency assessment'
compat={'origin':'CheckpointFullStoryComplete-20261017','originManifestSHA256':sha(hist/'CHECKPOINT_MANIFEST.json'),'questDataMechanicalFieldsUnchanged':True,'questPresentationChanges':quest_text_changes,'inventoryByteExact':True,'questManagerChanges':'partial declaration and HQ01 presentation text only','oldQuestManagerSHA256':sha(hist/quest_path),'currentQuestManagerSHA256':sha(R/quest_path),'historicalSaveSHA256':sha(R/'Validation/FullStory-20261017/Fresh04/full-story-final.json'),'currentCampaignCompletedCanonicalQuests':sum(q['id'] in vn['completed'] for q in content['quests']),'historicalCanonicalQuests':29}
dump('reused-quest-proof.json',compat)

# Every reference is a real file. Gallery selects representative native frames;
# it never substitutes generated images for gameplay evidence.
gallery=[]
def add(group,path,caption):
    p=R/path if isinstance(path,str) else path
    assert p.is_file(),p
    gallery.append({'group':group,'path':rel(p),'caption':caption})
for p in sorted((V/'visual01').glob('P7-*.png')):add('Current UI — 540×960',p,'Actual Windows Player; visual01 stopped later at window resize.')
for p in sorted((V/'maps01').glob('FP-Map-*.png')):
    city=p.stem.removeprefix('FP-Map-').removesuffix('-phase7')
    add('Glow comparison',f'Validation/FinalPolish-20261019/maps-before01/FP-Map-{city}-before.png',city+' BEFORE — previous recovery run, same earned save, zoom 1 and focus .5/.5 at 720×1280.')
    add('Glow comparison',p,city+' AFTER — current Phase7 runtime; canonical background unchanged. Reduction inherited from final-polish recovery.')
for p in sorted((V/'portraits01').glob('*-after.png')):
    before=p.with_name(p.name.replace('-after.png','-before-renderer-fixture.png'))
    if before.exists():add('Dialogue portraits — A/B',before,'BEFORE RENDERER FIXTURE: actual Player reproducing old sprite + rectangular Image behavior. Not a capture from an older binary.')
    add('Dialogue portraits — A/B',p,'AFTER: actual production DrawDialogue selection/mesh. Map and combat sprite bindings unchanged.')
for e in data['leisure']:
    selected=[f for f in frames if f['id']==e['id'] and f['width']==720]
    for f in selected:add('Leisure — '+e['title'],V/'leisure01'/f['file'],f"{e['nodeId']} · {f['kind']} · {f['elapsed']:.2f}s · native {f['width']}×{f['height']} · UTC {f['utc']}")
for p in sorted((V/'ending01').glob('*.png')):add('Ending — controlled private QA states',p,'Actual Player; explicit private QA fixture for condition matrix/ceremony. Earned ending is separately covered by vn01/korea01.')
for p in sorted((V/'vn01').glob('*.png')):add('VN_FIRST — earned fresh campaign',p,'Fresh H0 campaign; no injected progression flags.')
for p in sorted((V/'korea01').glob('*.png')):add('KOREA_FIRST — earned fresh campaign',p,'Fresh H0 campaign; alternate order and battle choices.')
reused=[('WorldMap.png','main02'),('Assembly-jeonju.png','main02'),('Fragments-seoul.png','main02'),('CheckIn.png','main02'),('PhotoJournal.png','main02'),('Battle-hoi_an.png','battle01'),('Battle-jeonju.png','battle01'),('P4-jeonju-trial-wrong.png','restore01'),('P4-jeonju-trial-correct-before-confirm.png','restore01')]
for name,run in reused:add('Reused compatible Phase6 runtime',f'Validation/Phase6-20261019/{run}/{name}','REUSED CheckpointPhase6VisualAndroid-20261019; related battle/restoration/world/check-in UI logic unchanged. Not new Phase7 photography.')
dump('gallery-index.json',gallery)
body=['<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC Phase7 review</title><style>body{background:#13232b;color:#eee;font:16px system-ui;margin:24px}a{color:#f4d78e}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(260px,1fr));gap:16px}figure{margin:0;background:#203841;padding:10px}img{width:100%;height:480px;object-fit:contain}figcaption{font-size:13px;overflow-wrap:anywhere}h2{margin-top:40px}</style><h1>Phase7 — real Windows runtime review</h1><p>STATUS: AUDIT_INCOMPLETE. Native PNGs open at original resolution when clicked. No AI/mockups. Before-renderer images are explicitly labelled private A/B fixtures. Old Phase6 captures are explicitly marked reused.</p><p>10 story minigames and 12 short interactions have fresh functional proof in both campaigns; their new dedicated visual capture loop was NOT REACHED after the window-resize failure. This gallery does not invent those missing pictures. Android device testing remains NOT_TESTED.</p><a href="FULL_PROJECT_AUDIT.md">Audit report</a> · <a href="QA_TEST_MATRIX.csv">Test matrix</a> · <a href="RELEASE_READINESS.md">Release readiness</a>']
for group in dict.fromkeys(g['group'] for g in gallery):
    body.append('<h2>'+html.escape(group)+'</h2><div class="grid">')
    for e in gallery:
        if e['group']!=group:continue
        url=os.path.relpath(R/e['path'],Q).replace('\\','/')
        body.append(f'<figure><a href="{html.escape(url)}"><img loading="lazy" src="{html.escape(url)}"></a><figcaption>{html.escape(Path(e["path"]).name)}<br>{html.escape(e["caption"])}</figcaption></figure>')
    body.append('</div>')
body.append('</html>');out('gallery.html','\n'.join(body))

matrix=[]
def test(id,system,scenario,env,expected,actual,status,evidence,notes=''):
    matrix.append(dict(zip(['TestID','System','Scenario','Environment','Expected','Actual','Status','Evidence','Notes'],[id,system,scenario,env,expected,actual,status,evidence,notes])))
for r in runs:
    test('RUN-'+r['run'],'Windows runtime',r['run'],'Windows Development Player', 'All scripted assertions complete',f"{r['assertions']} passed assertions; result {r['status']}",r['status'],r['evidence'],'Private QA save/prefs. See build-provenance.json for exact assembly. Bots-before is map diagnostic, not dialogue-fix acceptance.' if r['run']=='bots-before01' else 'No full-story assertion count is treated as visual approval.')
for e in data['activities']:
    p=next(p for p in content['puzzles'] if p['id']==e['id'])
    for order,run in [('VN_FIRST','vn01'),('KOREA_FIRST','korea01')]:
        assert e['id'] in vn['solvedPuzzles'] and e['id'] in kr['solvedPuzzles']
        log=(V/run/'smoke-results.txt').read_text(encoding='utf-8-sig');assert 'Earned runtime proof complete: '+e['kind']+' '+e['id'] in log
        test(order+'-'+e['id'],e['kind'],f"{p['cityId']}/{p['nodeId']} {e.get('title') or p['title']}",'Fresh Windows campaign','Complete through real interaction; commit once','Earned proof persisted; both orders complete','PASS',f'Validation/Phase7-20261019/{run}/smoke-results.txt','10 main-story minigames and 12 short interactions retain canonical puzzle IDs.')
    if e['kind']=='STORY_MINIGAME':test('VISUAL-'+e['id'],'Minigame visual',e.get('title',''),'Windows Player','Dedicated new visual acceptance frame','Capture loop not reached after QA window resize failed','NOT_TESTED','Validation/Phase7-20261019/visual01/smoke-results.txt','Functional proof above is independent; no beauty PASS inferred.')
for e in data['leisure']:
    for width,height in [(540,960),(720,1280)]:
        found=[f for f in frames if f['id']==e['id'] and f['width']==width]
        assert len(found)==5 and len([f for f in found if f['kind']=='motion'])==3
        test(f"LEISURE-{e['nodeId']}-{width}",'Leisure',e['title'],f'Windows {width}x{height}','Open at node; animate; close mid-motion; replay; reopen; return with input/save intact','5 native frames; three motion samples; suite PASS','PASS','Validation/Phase7-20261019/leisure01/smoke-results.txt; Validation/Phase7-20261019/leisure01/frames.json','Visual reviewer inspected representative current frames for all12; owner art approval remains distinct. No rewards or progress granted.')
for q in content['quests']:
    current=q['id'] in vn['completed'] and q['id'] in kr['completed']
    test('QUEST-'+q['id'],'Quest',q['title'],'Fresh Phase7 Windows' if current else 'Compatible prior Windows proof','Canonical quest completion persisted','Earned in current two campaign saves' if current else '29-quest Fresh04 proof reused; mechanics and inventory verified compatible','PASS','Validation/Phase7-20261019/vn01/FP-story-final.json' if current else 'Validation/FullStory-20261017/Fresh04/full-story-final.json; Docs/QA/Phase7/reused-quest-proof.json','Current campaign is not claimed to finish all optional quests. Reused origin CheckpointFullStoryComplete-20261017; current narrative/puzzle presentation differs.')
test('STATIC-WORLD','World integrity','Frozen scoped source and map data','File SHA256 + data validation','133 nodes/155edges/29quests/22puzzles/18checkins unchanged','456 baseline files inspected; four deliberate/generated existing-file deltas, zero missing','PASS','Docs/QA/Phase7/final-protected-diff.json','C# count121 is pre-task baseline; new QA/editor files are separately listed.')
test('NATIVE-REFS','Unity dependencies','Scene/prefab scripts and GUIDs','Unity 6000.6.5f1 Standalone','No missing scripts; unresolved references surfaced',f"missingScripts={native['missingScripts']}; unresolved renderer refs={len(native['missingGuids'])}",'PASS' if not native['missingScripts'] else 'FAIL','Docs/QA/Phase7/native-asset-audit.json','PASS covers missing scripts only. Unresolved renderer debug/lookup resources remain a separately disclosed risk.')
test('RENDERER-REFS','Render pipeline','7 unresolved renderer references','Native dependency audit','All relevant renderer refs resolved','Six probe-volume debug refs and falloff lookup unresolved; no reproduced visible failure','VISUAL_REVIEW_PENDING','Docs/QA/Phase7/native-asset-audit.json','Inherited. Do not delete or null used pipeline assets blindly.')
test('ANDROID-BUILD','Android','APK build','Installed SDK/NDK/OpenJDK','Completed APK','Stopped verified owned process after 12-minute budget; first-target Sentis shader compilation then Player build','BLOCKED','Docs/QA/Phase7/android-build-result.json','No completed APK, AAB or device PASS claimed. No repeat loop.')
for id,scenario in [('DEVICE','Physical touch/performance'),('PAUSE','Physical device pause/resume'),('MEMORY','Long-session FPS/GC/VRAM profiling')]:test('ANDROID-'+id,'Android',scenario,'No attached physical device','Actual device evidence','No device attached','NOT_TESTED','Docs/QA/Phase7/android-build-result.json')
test('AUDIO','Audio','BGM/SFX/mute/loop integration','Compatible prior Windows + fresh campaigns','Context correct; no duplicate controller','Prior integration logs reused; current campaigns/leisure preserve context','PASS','Docs/QA/FOUR_TRACK_MUSIC_INTEGRATION.md; Docs/QA/SOFT_UI_SFX_INTEGRATION.md','Phase7 baseline audio files unchanged. This is technical integration evidence, not a new physical listening session.')
test('AUDIO-LISTEN','Audio','Human listening at all four loop boundaries','Headphones/speakers','No audible click or awkward silence','Not newly auditioned','NOT_TESTED','Docs/QA/FourTrackMusic-20261018/verification.json','Dsp timing cannot replace subjective listening.')
test('ROUTES','Movement','Approved corrected route traversal','Compatible prior Windows','No geometry regression','Frozen route and movement source retained; 68 prior traversals reused','PASS','Docs/QA/Phase7/final-protected-diff.json','Origin CheckpointRoutePlayerCorrection-20261019. Not a new terrain PASS. Owner-accepted route exceptions remain.')
test('UI-HIGH-RES','UI','Current 720/1080 visual layout loop','Hidden Windows QA window','Requested native framebuffer','Requested720x1280 yielded720x960; probe aborted','BLOCKED','Validation/Phase7-20261019/visual01/resolution-diagnostics.txt','Native720 portrait/all12leisure already PASS; previous Phase6 four-resolution H2/layout proof reusable; this particular new loop still failed.')
test('COMPETITION','Submission','Official competition rules','Not supplied','Verify actual official rules','COMPETITION_REQUIREMENTS_NOT_VERIFIED','NOT_TESTED','Docs/QA/Phase7/RELEASE_READINESS.md')
test('CLEANUP','Storage','Safe removal eligibility','Read-only dependency audit','Only proven disposable files removed','No candidate satisfied eligibility; zero deletion/quarantine','PASS','Docs/QA/Phase7/CLEANUP_RESULT.md','Post-cleanup regression N/A; source-fix regression is covered separately.')
csvfile('QA_TEST_MATRIX.csv',['TestID','System','Scenario','Environment','Expected','Actual','Status','Evidence','Notes'],matrix)

botrows=[]
for e in load(Q/'bot-alpha-plan.json'):
    for a in e['affected']:botrows.append([a['city'],a['node'],a['artId'],e['spriteName'],e['destination'],'Dialogue alpha/mesh PASS; owner visual acceptance pending'])
out('BOT_DIALOGUE_ALPHA_REVIEW.md',f'''# Bot/monster dialogue portrait repair

Owner clarification: the defective image is shown in dialogue. This work does not change map actors or combat art. All 23 encounter-role dialogue portraits across six cities were opened in the actual Windows Player; 363 assertions passed in `portraits01`. Each open/close preserved the complete quest/reward state.

Root cause: several concept-source rectangles contain opaque cream paper (alpha255), which an import transparency checkbox cannot remove. The dialogue code also ignored the available `Enemy.portrait` and rendered the map sprite as a rectangular Image without its sprite mesh. Fixed only DrawDialogue to select `portrait ?? sprite` and use the intended mesh. Battle rendering is untouched.

Four native-size RGBA derivatives (291×268,299×279,284×271,284×295) preserve original pixels/identity. Border-connected near-cream removal uses the existing approved outline to exclude neighboring art, then a one-pixel neutral edge de-matte. No AI, resize, new face or overwritten source. Uncompressed, bilinear, no mipmaps, alpha transparency enabled, max512. Original sheet PNG/meta untouched. Plan with source hashes/rects/GUID-backed import mapping: `bot-alpha-plan.json`.

{table(['City','Node','Enemy ID','Source sprite','Derived portrait','Result'],botrows)}

Other 18 of 23 encounter roles retain their previous portrait assets; they benefit from respecting existing portrait refs/meshes. They were not regenerated. Reviewed the native G7 before/after and S19 after, plus the all23 contact sheet. The large white paper rectangle is absent; inner pale stone/flame/glow highlights are intentionally preserved. Limited original concept resolution remains; this is not a claim of high-resolution new art.

Before A/B images are explicitly labelled `before-renderer-fixture`: current Windows Player recreates the exact old Image assignment/mesh behavior using original art. They are not old-binary captures. After images are unmodified production DrawDialogue. Same 720×1280 camera in each pair.

`enemy_fox` at G6 historically uses a cat concept while awaiting actual fox art. Preserve that existing NOT_CANONICAL annotation; cleaning alpha does not approve identity. The other historical art-acceptance annotations are also retained. No new canonical approval is implied.

Map/combat `sprite` references are byte-identical to baseline for all enemy records. H6, 18 check-ins, route geometry, NPC identities and gameplay remain unchanged. Gallery: [gallery.html](gallery.html).
''')

out('BUG_FIX_LOG.md','''# Confirmed issues and bounded fixes

| ID | Severity | Evidence / cause | Action | Result |
|---|---|---|---|---|
| P7-01 | P2 visual | Opaque cream paper on four concept rectangles; DrawDialogue uses map sprite instead of portrait and ignores enemy mesh | Four original-pixel alpha derivatives; five portrait bindings; DrawDialogue portrait/mesh selection | Windows363 dialogue assertions PASS, before/after reviewed. Original map/combat art preserved. |
| QA-01 | QA harness | New Editor audit helper uint build counters assigned to int | Explicit cast corrected in helper | Subsequent successful Editor compile/Windows build. No gameplay fix involved. |
| QA-02 | QA environment | visual01 requested720×1280 but native framebuffer remained720×960 | Stopped failed visual loop; retained diagnostics; no repeated build or production resolution change | BLOCKED for that loop; all12leisure and23portraits already have current native720 evidence. New ten-minigame beauty captures NOT_TESTED. |
| RISK-01 | P3 potential | Seven unresolved Renderer2D debug/lookup GUIDs | Reported; no blind deletion or pipeline substitution | No reproduced missing sprite/pink-frame failure; technical review pending. |
| RISK-02 | P3 potential | ComputeBuffer/D3D12 release warnings at Player shutdown | Scoped source check; no new game ComputeBuffer allocation introduced | Reproduced diagnostic, long-session VRAM profiling NOT_TESTED. No unsupported zero-leak claim. |
| BUILD-ANDROID | Platform blocker | First Android target import/Sentis shader work exceeded12-minute budget | Verified and stopped only owned Editor process tree | No completed APK/AAB; physical device NOT_TESTED. Do not confuse this with a gameplay failure. |

Inherited final-polish tea flattening/cup-to-beak/food-to-beak and reduced decorative glow remain intact. No route/gameplay fix or new feature was added. Current Phase7 existing-file delta and hashes are in final-protected-diff.json. AdventureUIArt metadata normalization restores original approval annotations and semantically identical Unicode escaping; it does not change tested picture references.
''')

questrows=[[q['id'],q['cityId'],q['title'],'current two orders' if q['id'] in vn['completed'] and q['id'] in kr['completed'] else 'compatible Fresh04 prior proof'] for q in content['quests']]
activityrows=[]
for e in data['activities']:
    p=next(p for p in content['puzzles'] if p['id']==e['id']);activityrows.append([e['id'],p['cityId'],p['nodeId'],e.get('title') or p['title'],e['kind'],'functional PASS in both fresh orders'])
leisurerows=[[e['cityId'],e['nodeId'],e['title'],'PASS540+720; 3 motion frames each; replay/close/save invariant'] for e in data['leisure']]
runrows=[[r['run'],r['status'],r['assertions'],r['png']] for r in runs]
out('FULL_PROJECT_AUDIT.md',f'''# Phase7 full project audit — 2026-10-19

Final status: **AUDIT_INCOMPLETE**. Most targeted Windows functional checks passed. Android build/device validation and the blocked current high-resolution/new minigame visual capture loop prevent a blanket final QA/release PASS. No reproduced critical gameplay blocker was found in the executed Windows tests. This report distinguishes execution, compatible historical evidence and missing visual evidence.

## Baseline and protection

Latest live source inherited from CheckpointFinalPolishRecovery-20261019, not a rollback. No Git repository. Read-only baseline456 relevant files; zero missing. Existing-file deltas: AdventureUIArt.asset (five dialogue portraits/status annotations), GameplayPanels.cs (DrawDialogue only), GameplaySmokeRunner.cs (QA dispatch/capture policy), ProjectSettings.asset (Unity-generated InputSystem preloaded asset and Android SENTIS_ANALYTICS_ENABLED). New files are scoped QA/editor tools and four RGBA derivatives/metas. `final-protected-diff.json` gives hashes. Do not confuse Unity-generated settings with a deliberate gameplay change.

Canonical counts:6cities,133nodes,155edges,29quests,22puzzle IDs,18check-ins,6relics/24fragments,10story minigames,12short interactions,12leisure. Canonical map/source PNGs, nodes, all route data/arrivals/movement, H6/occlusion, NPCs, audio, quest/reward/save/ending services remain frozen. Routes CLOSED_BY_OWNER / ACCEPTED_WITH_KNOWN_LIMITATIONS: U-E018,U-E022,D-E020,D-E025,J-E026,G-E004,G-E006,S-E002,S-E016,S-E009; S-E017 preserved. Graph finite coordinates and ungated connectivity validated, not terrain approval. Existing degree4 nodes were not pruned.

## Source and Unity audit

Bounded source inventory121 baseline scripts; targeted review of bootstrap/transition, CityMap/discovery/movement, NodeEventManager, QuestManager/Guidance, Inventory, PuzzleManager, battle, collection/relic/ending, leisure, dialogue and audio. This is manual critical-path/static pattern review plus runtime testing, not a formal proof of every code path. Save v1/v2 fallback preserves unreadable files; atomic replace/backup and idempotent grant ledger remain. Puzzle commit rolls back on failed persistence. Singleton/sceneLoaded cleanup and animation OnDisable paths retained. No second manager, Canvas, EventSystem, pathfinder or save schema introduced. Editor/platform-only probes remain guarded; no unguarded new UnityEditor/Win32 dependency added to mobile production.

Native Unity asset audit: {native['assets']} Assets entries, {native['dependencies']} dependencies rooted in enabled scenes, all Resources, Graphics/Quality pipeline refs. Missing scripts={native['missingScripts']}. Seven unresolved Renderer2D probe-volume-debug/falloff references remain (native-asset-audit.json); current unlit/UI gameplay did not reproduce a visible failure. No blind GUID cleanup. Dynamic Resources/JSON bindings mean text-search zero is not proof of unused art. No Addressables package/StreamingAssets folder. Runtime texture import inventory retained; four new cutouts uncompressed/bilinear/no mip/max512. Original map/NPC/Phase6 crop imports preserved.

## Actual Windows runs

{table(['Run','Result','Passed assertions','Native PNG count'],runrows)}

Assertions are not independent test-case counts or subjective visual approval. Each run carries build/input provenance. Early campaign/ending runs predate the final portrait-only build but depend on unchanged gameplay services/data; final portrait, all12 leisure and restart runs use the final build. `Logs/phase7-bot-portrait-build01.log` reports Success. `phase7-final-readonly-audit.log` checks final Editor compilation without another Player build. Production status annotation normalization after the Player build changes no runtime-consumed field.

VN_FIRST and KOREA_FIRST start fresh H0, use existing node interactions, H3 collision/pickup/tutorial, unlock cities through legacy beacon/quest logic, earn all22 activity proofs, collect/assemble/trial/restore all6relics, resolve boss using different choices and reach the sole canonical ending. Six restored relics do not open World Map or block boss access. Zero-relic boss run60assertions passed: no credits/soleending, free travel remains, no boss rematch required. Ending matrix142assertions includes old ending history, incomplete relic sets, explicit ceremony, close/pause/skip/full sequence, credits/reentry and idempotence. Matrix uses private fixtures; fresh campaign evidence separately earns actual progress.

Current fresh main routes complete {compat['currentCampaignCompletedCanonicalQuests']}/29 canonical quests, plus tutorial; they do not claim all optional quests newly rerun. Compatible29quest Fresh04 proof is reused only after checking mechanical quest fields, Inventory exact SHA, and QuestManager differences (partial declaration and one presentation string). Origin hash and exact comparison: reused-quest-proof.json. New22 interaction behavior is independently exercised in both current fresh campaigns.

{table(['Quest','City','Title','Evidence origin'],questrows)}

## Implemented10minigames and12short interactions

{table(['ID','City','Node','Name','Runtime type','Functional result'],activityrows)}

These are actual implemented runtime interactions, not previews. New dedicated per-minigame screenshots remain NOT_TESTED because visual01 stopped before that loop. Snapshot filtering in campaign mode retained three ending milestones rather than22activity frames. Functional PASS does not imply beauty approval or fabricated captures. No legacy22 puzzle IDs/quest rewards were redesigned.

## All12leisure and visual inspection

{table(['City','Node','Activity','Runtime result'],leisurerows)}

822assertions,120native frames, both540×960/720×1280; actual node menu/background/prop/Lumi; mid-animation close, replay/reopen, same node/camera/input, single Canvas/EventSystem and full SaveData invariance. Three timed motion frames per activity per resolution. Viewed representative actual frames of all12 and detailed bot before/after. Swing uses owner's fullbody standing rule; approved Hue boat is untouched. Tea reduced table tilt and beak-contact/snack fixes inherited, current leisure component unchanged by Phase7. Lightweight standing/transform poses remain an art-quality limitation for some activities, not permission to create new seated art. User visual acceptance remains pending in gallery.

Six current native map images show the inherited reduced decorative halos; canonical backgrounds unchanged. Compared with recovery before frames at same720×1280/focus/zoom/earned seed. Lighting A/B/C controller/mask is byte-exact to baseline:40% dimA,20% circleB aftercompletedmai-lantern,fullC onlyrestoredrelic. New effects did not alter discovery. 23bot dialogue cases opened; four cutouts/five bindings repaired; white paper gone in reviewed nativeG7/S19 and contact sheet. Details: BOT_DIALOGUE_ALPHA_REVIEW.md.

## UI, localization and audio

Current540×960 dialogue/inventory/quest/collection bounds PASS. Native720×1280 bot/leisure frames PASS. New general layout loop failed native resizing720×1280→720×960; recorded as QA-environment BLOCKED, not silently ignored. Compatible Phase6 native540/720/1080×1920/1080×2400 H2/layout proof can be reused for unchanged branches, but it does not validate every new portrait at1080. Current ten-minigame visual capture loop was not reached. Safe-area device behavior NOT_TESTED. Scrollable long dialogue visibly clips at viewport edge by design; reading full text requires scroll, not an assertion of all text fitting at once.

Vietnamese current frames render correctly in inspected screens; exhaustive localization/cultural factual review and Hangul font coverage NOT_TESTED. Existing phase6 cutouts remain unchanged. BGM/SFX source,clips,MP3/WAV/import/mapping remain baseline-exact. Prior fourtrack/sfx actual Windows tests reused (see integration reports); current campaigns/leisure preserve music context. No new tracks or settings logic. Full listening audition/Android focus/audio behavior NOT_TESTED; DSP timing alone is not an audible-quality approval.

## Platform, performance and packaging

Windows Development build succeeds and exits in executed runs. This is not a signed/distribution release build. APKattempt was bounded12minutes: SDK/NDK/JDK installed; first-target package import/Sentis shader compilation then Playerbuild; stopped verified owned tree, no completed APK/AAB claimed. No attached device. Android performance/touch/resume NOT_TESTED.

Short compatible Phase6 pan probe:120frames≈0.833s, objects1059→1059,nativeallocated155879648→155879815B. It is a short Windows sample, not AndroidFPS/VRAM/zeroGC proof. Current Player exits reproduce ComputeBuffer/D3D12 release diagnostics; no newly introduced game buffer allocation, but long-session leak profiling is pending. Original concept resolution still limits some portraits; alpha repair does not create more source detail.

Unity6000.6.5f1; version1.0; portrait; Androidmin26,ARM64/IL2CPP configured. DefaultCompany/package/icon/signing and release packaging require owner review. SampleScene remains enabled and protected, not blindly deleted. Official competition rules absent:COMPETITION_REQUIREMENTS_NOT_VERIFIED. Current build size is recorded in release-metrics.json (includes debug/runtime files; no APK estimate fabricated).

## Cleanup and closeout

No file deletion/quarantine:0bytes reclaimed. Cleanup eligibility fails completeplatform/referencecertainty; production leisure/shortinteraction assets and uncertain render/package references retained. CLEANUP_CANDIDATES.csv documents conservative decisions. No caches, Backups, saves or historical evidence removed. Post-cleanup regression N/A; source-fix portrait363/leisure822/restart10assertions are valid targeted regression. Reports, real-runtime gallery and incremental checkpoint are the stopping deliverables. No new content/Phase task starts.
''')

candidate_paths=[('Assets/Scenes/SampleScene.unity','enabled build scene','C','KEEP'),('Assets/_Game/Art/_SourceSheets/Enemies_Roster_Concept.png','original source and23actors/derived provenance','C','KEEP'),('Assets/Settings/Renderer2D.asset','Quality/Graphics runtime dependency; unresolved debug refs need review','C','KEEP'),('Packages/manifest.json','AI inference/package shader build cost is not unused proof','C','KEEP'),('Assets/_Game/Scripts/Validation/GameplaySmokeRunner.StoryLeisure.cs','development QA helpers reused by current smoke paths','C','KEEP'),('Assets/_Game/Resources/Content/LeisureActivities.json','all12 production activities executed','C','KEEP'),('Validation/Phase7-20261019/visual01/smoke-results.txt','retains actual failure diagnostics','C','KEEP')]
candidates=[]
for path,why,cat,action in candidate_paths:
    p=R/path;meta=Path(str(p)+'.meta');guid=re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1] if meta.exists() else ''
    candidates.append(dict(Path=path,FileType=p.suffix,Size=p.stat().st_size if p.exists() else 0,GUID=guid,DependencyEvidence=why,Category=cat,CleanupAction=action,Confidence='Required or uncertain; not eligible for deletion',RecoveryPath='Unchanged in live project and checkpoint chain',Reason='No destructive action justified'))
csvfile('CLEANUP_CANDIDATES.csv',['Path','FileType','Size','GUID','DependencyEvidence','Category','CleanupAction','Confidence','RecoveryPath','Reason'],candidates)
out('CLEANUP_RESULT.md','''# Cleanup result

Eligibility: **NOT_ELIGIBLE_FOR_ASSET_REMOVAL**. Android build/device checks and reference/visual completeness are unresolved. Dynamic Resources bindings also prevent treating zero text matches as unused proof.

- Safely deleted: none.
- Quarantined: none; quarantine size0bytes.
- Recovered: **0bytes**.
- Preserved: canonical/source art, all12leisure/12shortinteraction assets, enabled SampleScene, used renderer/settings, package manifest, all backups/saves/history/current acceptance diagnostics.
- Candidate decisions: CLEANUP_CANDIDATES.csv. No broad recursive cache or backup cleanup.
- Restoration: no cleanup to undo. Approved source repair can be restored from incremental Phase7 checkpoint over its verified base chain; never discard unrelated latest work.
- Post-cleanup regression: N/A (zero files moved/deleted). Relevant production-fix regression: portraits01 363assertions, leisure01 822assertions, restart01 10assertions PASS. visual01 remains failed/blocked and is not hidden by a cleanup PASS.
''')
build=R/'Builds/Windows'; total=sum(p.stat().st_size for p in build.rglob('*') if p.is_file())
metrics={'windowsDevelopmentBytes':total,'windowsDevelopmentMiB':round(total/1048576,2),'assemblySHA256':sha(build/'LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'),'exeSHA256':sha(build/'LightBetweenCultures.exe'),'apk':'NOT_BUILT','physicalDevice':'NOT_TESTED'}
dump('release-metrics.json',metrics)
out('RELEASE_READINESS.md',f'''# Release readiness — AUDIT_INCOMPLETE

Windows: successful **Development Player** build, actual two fresh campaign orders and targeted final regressions. Current assemblySHA256 `{metrics['assemblySHA256']}`. Folder size **{metrics['windowsDevelopmentMiB']}MiB** including development/runtime files; not a compressed installer or APK size.

Android: **BLOCKED**. One bounded12-minute real APKattempt reached compilation/Playerbuild, stopped only owned Editor tree; no completedAPK/AAB. SDK/NDK/JDK exist. Physical device/performance/pause/resume **NOT_TESTED**. No repeated build loop.

Windows gameplay: all22 story interactions and all6relics/24fragments earned in both VN_FIRST/KOREA_FIRST, soleending,0relicboss continuation, legacyendingcondition matrix and current restart PASS.29quest mechanical compatibility documented; optional quest proof reused rather than falsely called fresh. No critical gameplay blocker reproduced in executed coverage.

Outstanding before submission:
1. Complete bounded AndroidreleaseAPK/AAB build and real-device smoke/performance/focus/audio tests.
2. Resolve native QA window resizing and finish current higher-resolution/minigame visual acceptance captures. visual01 failed at720×960actual versus720×1280requested. New1080portraitcoverage and device safe-area NOT_TESTED. Do not rebuild repeatedly without a diagnosed reason.
3. Owner visual review of repaired dialogue portraits and lightweight leisure poses. Three Seoulfragment icons still ART_PENDING (`fragment_seoul_heritage_data`, `fragment_seoul_jiwon_sync`, `fragment_seoul_stellar_ring`); relay-frame crop acceptance pending. G6 fox currently uses historical catconcept pending proper fox art; no newidentityapproval granted.
4. Investigate inherited7Renderer2D debug/falloff GUIDrefs and shutdown ComputeBuffer/D3D12 warnings; profile longsessionmemory. No proven runtime crash from those diagnostics, no blanket zero-leak claim.
5. Confirmcompany/packageID/icon/version/signing, release/debugUI stripping and distribution package; current companyDefaultCompany,version1.0,existingSampleScene stillenabled. Approve actualcompetitionrequirements: **COMPETITION_REQUIREMENTS_NOT_VERIFIED**.
6. Human listening review at four BGMloops, full localization and credits/license acceptance. TechnicalaudiointegrationPASSisnot a new subjectiveaudioapproval.

Protected routes remain CLOSED_BY_OWNER / ACCEPTED_WITH_KNOWN_LIMITATIONS, not a submission-blocking redesign task. No new required feature was invented; missing coverage/art are explicit, not labelled implemented-final. Cleanup0files/0bytes. Finalcheckpointis incremental and requires its manifest basechain. STOP after handoff; no newphase/polish work.
''')
dump('closeout-index.json',{'createdUTC':datetime.now(timezone.utc).isoformat(),'status':'AUDIT_INCOMPLETE','matrixRows':len(matrix),'galleryImages':len(gallery),'deletedPaths':[],'cleanupBytes':0,'stop':'STOP_WAITING_FOR_OWNER_REVIEW'})
print(json.dumps({'matrixRows':len(matrix),'galleryImages':len(gallery),'windowsMiB':metrics['windowsDevelopmentMiB'],'status':'AUDIT_INCOMPLETE'}))
