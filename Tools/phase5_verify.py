"""Bounded Phase 5 verification; no gameplay or historical evidence edits."""
import hashlib, json, re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Docs/Gameplay/Phase5-20261018'
baseline = json.loads((OUT / 'baseline.json').read_text(encoding='utf-8-sig'))
before = Path(baseline['before'])
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def read(p): return json.loads(p.read_text(encoding='utf-8-sig'))
checks = []
def check(ok, text):
    checks.append({'result': 'PASS' if ok else 'FAIL', 'check': text})
    if not ok: raise AssertionError(text)

# Unity adds empty Android icon slots during a Windows build. Restore only when
# every byte outside that serialized block matches the scoped before snapshot.
path = Path('ProjectSettings/ProjectSettings.asset')
old = (before / path).read_bytes()
new = (ROOT / path).read_bytes()
pat = rb'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
check(re.sub(pat, b'', old) == re.sub(pat, b'', new), 'ProjectSettings outside generated platform-icon block unchanged')
if new != old: (ROOT / path).write_bytes(old)
for name, expected in baseline['protected'].items():
    check(digest(ROOT / name) == expected.lower(), 'Protected exact: ' + name)
check(digest(ROOT / 'Assets/_Game/Resources/Content/CulturalRelics.json') == digest(before / 'Assets/_Game/Resources/Content/CulturalRelics.json'), 'Canonical six relics / 24 sources / trials byte exact')
check(digest(Path(baseline['baseCheckpoint']) / 'CHECKPOINT_MANIFEST.json') == baseline['baseManifestSHA256'], 'Latest base manifest SHA256 exact')

name = 'Assets/_Game/Resources/Content/GameplayContent.json'
a, b = read(before / name), read(ROOT / name)
for key in a:
    if key not in ('quests', 'events'): check(a[key] == b[key], 'Content unchanged: ' + key)
for old_event, event in zip(a['events'], b['events']):
    allowed = {'title', 'text', 'completedText', 'endingPages'} if old_event['id'] == 'final_festival' else set()
    check({k:v for k,v in old_event.items() if k not in allowed} == {k:v for k,v in event.items() if k not in allowed}, 'Event semantics preserved: '+event['cityId']+':'+event['id'])
check(len(a['events']) == len(b['events']), 'No event added or removed')
for old_quest, quest in zip(a['quests'], b['quests']):
    allowed = {'offer', 'reminder', 'thanks'} if old_quest['id'] == 'SQ03' else set()
    check({k:v for k,v in old_quest.items() if k not in allowed} == {k:v for k,v in quest.items() if k not in allowed}, 'Quest graph/reward preserved: '+quest['id'])
check(len(a['quests']) == len(b['quests']) == 29, '29 canonical quests unchanged')
check(len(b['cities']) == 6 and sum(len(c['nodes']) for c in b['cities']) == 133 and sum(len(c['edges']) for c in b['cities']) == 155, '6 cities / 133 nodes / 155 edges')
check(len(b['puzzles']) == 22 and len(b['checkIns']) == 18, '22 puzzles / 18 check-ins')

name = 'Docs/Art/NPC_CANONICAL_REGISTRY.json'
a, b = read(before / name), read(ROOT / name)
approved = {'npc_curator', 'npc_engineer', 'npc_keeper', 'npc_inscription'}
for x, y in zip(a['identities'], b['identities']):
    if x['npcId'] not in approved:
        check(x == y, 'Other NPC identity preserved: '+x['npcId'])
        continue
    for vx, vy in zip(x['variants'], y['variants']):
        for key in ('variantId', 'path', 'spriteName', 'guid', 'localId'):
            check(vx.get(key) == vy.get(key), 'Approved binding exact: '+x['npcId']+'/'+key)
    check(y['status'] == 'CANONICAL' and y.get('ownerApproval') == 'OWNER_APPROVED_AS_IS_20261018', 'Owner approval metadata: '+x['npcId'])

# H6 postcard methods reside in a shared file; compare their exact source bodies.
def method(text, marker):
    start = text.index(marker); opening = text.index('{', start); depth = 1; end = opening + 1
    while depth:
        depth += (text[end] == '{') - (text[end] == '}'); end += 1
    return text[start:end]
name = 'Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs'
a, b = [(p / name).read_text(encoding='utf-8-sig') for p in (before, ROOT)]
for marker in ('void ShowRelicPhoto(', 'void SetPostcardUV(', 'void ShowStoredPostcard('):
    check(method(a, marker) == method(b, marker), 'H6 method exact: '+marker)

result = {'scope': 'Phase5 current inputs only; no whole-project hash scan', 'protectedFiles': len(baseline['protected']), 'checks': checks, 'result': 'PASS'}
(OUT / 'protected-verification.json').write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print('PASS:', len(checks), 'bounded checks;', len(baseline['protected']), 'protected files exact')
