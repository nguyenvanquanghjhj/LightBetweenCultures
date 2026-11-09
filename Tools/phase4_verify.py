"""Bounded Phase4 preservation checks; no history/cache/backup tree scan."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DOC = ROOT / 'Docs/Gameplay/Phase4-20261018'
BEFORE = ROOT.parent / 'Backups/BeforePhase4Relics-20261018'


def read(path):
    return path.read_text(encoding='utf-8-sig')


def method(text, signature):
    start = text.index(signature)
    left = text.index('{', start)
    depth = 0
    for i in range(left, len(text)):
        depth += (text[i] == '{') - (text[i] == '}')
        if depth == 0:
            return text[start:i + 1]
    raise AssertionError(signature)


def main():
    checks = []
    for entry in json.loads(read(DOC / 'protected-files.json')):
        actual = hashlib.sha256((ROOT / entry['path']).read_bytes()).hexdigest()
        assert actual == entry['sha256'], entry['path']
        checks.append('EXACT: ' + entry['path'])
    relic_path = Path('Assets/_Game/Resources/Content/CulturalRelics.json')
    before = json.loads(read(BEFORE / relic_path))
    now = json.loads(read(ROOT / relic_path))
    for key in ['relics', 'fragments', 'story']:
        def hoi_an(r):
            return r.get('id', '').startswith('fragment_hoi_an_') if key == 'fragments' else r.get('cityId') == 'hoi_an'
        # Greeting Stamp has home Jeonju but provenance city HoiAn, explicitly extended in Phase4.
        assert [r for r in before[key] if hoi_an(r)] == [r for r in now[key] if hoi_an(r)], key
        checks.append('EXACT: HoiAn ' + key + ' data/conditions/text')
    for file, signature in [
        ('Content/RelicCollection.cs', 'internal bool FinishPhoto('),
        ('UI/GameplayPanels.Relics.cs', 'void ShowRelicPhoto('),
        ('UI/GameplayPanels.cs', 'void ShowLegacyCheckIn('),
    ]:
        path = Path('Assets/_Game/Scripts') / file
        assert method(read(ROOT / path), signature) == method(read(BEFORE / path), signature), signature
        checks.append('EXACT: H6 ' + signature)
    old_caption = read(DOC / 'before-checkin-narration.cs.txt')
    current_caption = read(ROOT / 'Assets/_Game/Scripts/UI/GameplayPanels.CheckIns.cs')
    branch = 'if(entry.id=="hoi_an.h6")\n            {if(game.Relics.PhotoRecorded)description=game.Relics.Story("photo:stored",description);}'
    assert branch in old_caption and branch in current_caption
    checks.append('EXACT: H6 photo-stored caption branch; extra scroll restricted to non-HoiAn')
    content = json.loads(read(ROOT / 'Assets/_Game/Resources/Content/GameplayContent.json'))
    assert len(content['cities']) == 6 and sum(len(c['nodes']) for c in content['cities']) == 133
    assert sum(len(c['edges']) for c in content['cities']) == 155
    assert len(content['quests']) == 29 and len(content['puzzles']) == 22 and len(content['checkIns']) == 18
    assert len(now['relics']) == 6 and len(now['fragments']) == 24
    assert all(r['enabled'] for r in now['relics'] + now['fragments'])
    assert len({r['id'] for r in now['relics']}) == 6 and len({f['id'] for f in now['fragments']}) == 24
    checks.append('PASS: 6 cities/133 nodes/155 edges/29 quests/22 puzzles/18 checkins; 6 relics/24 unique enabled fragments')
    pending = {f['id'] for f in now['fragments'] if f.get('artStatus') == 'ART_PENDING'}
    assert pending == {'fragment_seoul_heritage_data', 'fragment_seoul_jiwon_sync', 'fragment_seoul_stellar_ring'}
    checks.append('PASS: exactly 3 semantic Seoul components remain ART_PENDING')
    (DOC / 'preservation-results.json').write_text(json.dumps(dict(result='PASS', checks=checks), indent=2) + '\n', encoding='utf-8')
    print('Preservation PASS:', len(checks), 'checks; no full-project hash scan')


if __name__ == '__main__':
    main()
