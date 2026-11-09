"""Read-only economy/registry audit; output stays in the named shop QA folder."""
import csv, hashlib, json, statistics
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Validation/JourneyShop-20261021'
OUT.mkdir(parents=True, exist_ok=True)
source = ROOT / 'Assets/_Game/Resources/Content/GameplayContent.json'
data = json.loads(source.read_text(encoding='utf-8-sig'))
prices = {'hoi_an_fresh_chili': 1, 'hoi_an_purple_shallot': 1,
          'meat_portion': 1, 'healing_tea': 2, 'royal_herb': 2,
          'night_flower': 2, 'lan_glasses': 3, 'da_lat_strawberry': 2}
rows = []
for item in data['items']:
    key = item['id']
    rows.append({'ItemID': key, 'Name': item['name'], 'Group': 'A' if key in prices else 'C',
                 'Price': prices.get(key, ''), 'StackLimit': item['stackLimit'],
                 'Reason': 'Ordinary supply; original delivery/interaction still required' if key in prices
                 else 'Currency itself' if key == 'coin'
                 else 'Authored story proof/key, signed correspondence, cultural reward or memento; not an ordinary supply',
                 'RequiredBy': ','.join(q['id'] for q in data['quests'] if any(x['itemId'] == key for x in q.get('requiredItems', [])))})
    if key == 'copper_wire':
        rows[-1].update(Group='A_VIA_BAIT', Reason='HQ02 also requires crab_resolved; selling a wire alone would be a misleading purchase. Buy the one-coin bait, then use the existing peaceful crab interaction to earn wire and canonical proof.')
with (OUT / 'item-classification.csv').open('w', encoding='utf-8-sig', newline='') as f:
    writer = csv.DictWriter(f, fieldnames=rows[0]); writer.writeheader(); writer.writerows(rows)
sources = []
for group in ('quests', 'events', 'puzzles', 'enemies', 'checkIns', 'checkins'):
    for entry in data.get(group, []):
        coins = entry.get('rewardCoins', 0)
        if coins:
            sources.append({'kind': group, 'id': entry['id'], 'coins': coins,
                            'oneShot': entry.get('oneShot'), 'rewardOnce': entry.get('rewardOnce'),
                            'replayable': entry.get('replayable'),
                            'ledger_or_guard': 'Inspect existing completed/resolved/solved/check-in guard; no source modified'})
audit = {'contentSHA256': hashlib.sha256(source.read_bytes()).hexdigest(),
         'itemClassification': rows, 'permission': {'id': 'ferry_hq04', 'group': 'B', 'price': 5,
         'canonicalQuest': 'HQ04', 'canonicalFlag': 'hoi_an.ferry_available',
         'reason': 'Optional fare; identical to the existing five-coin manual payment; no story chapter skipped'},
         'coinSources': sources,
         'sourceSummary': {g: {'count': len(v := [e['coins'] for e in sources if e['kind'] == g]),
                               'min': min(v), 'max': max(v), 'median': statistics.median(v), 'sum': sum(v)}
                           for g in sorted({e['kind'] for e in sources})},
         'balance': {'earlyH11Chest': 6, 'ordinaryPrices': [1, 2, 3],
                     'foodPrepSupplyAndReserves': {'price': 5, 'canonicalReward': 8, 'rewardOnce': True},
                     'rewardChanges': [], 'timeToCollect': 'Not independently timed; no fabricated minutes or grind estimates',
                     'farming': 'No repeatable coin source added; finite canonical rewards retained'},
         'countingCaution': 'Do not sum source groups into a guaranteed budget: encounter enemy/event definitions can describe the same reward; access and optional choices also differ.',
         'excluded': ['all six relics', 'all 24 relic fragments', 'final boss/ending/ceremony flags',
                      'mandatory story and world-city unlocks', 'all 22 puzzle proofs']}
(OUT / 'economy-audit.json').write_text(json.dumps(audit, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'items': len(rows), 'soldItems': len(prices), 'permissions': 1,
                  'sourceSummary': audit['sourceSummary']}, ensure_ascii=True))
