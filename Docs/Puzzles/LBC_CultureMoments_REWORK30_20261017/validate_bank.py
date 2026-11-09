from pathlib import Path
import json
p=Path(__file__).with_name('CULTURE_MOMENTS_30.json')
d=json.loads(p.read_text(encoding='utf-8'))
M=d['moments']; assert len(M)==15,len(M)
ids=set(); count=0; bycity={}
for m in M:
 assert m['momentId'] not in ids; ids.add(m['momentId']); assert m['optional'] and m['mainQuestUnaffected']
 assert len(m['variants'])==2; bycity[m['cityId']]=bycity.get(m['cityId'],0)+1
 for v in m['variants']:
  count+=1
  assert v['variantId'] not in ids; ids.add(v['variantId'])
  assert v['evidenceRequired'] and v['evidencePresentation'] and v['question'] and v['explanation'] and v['hint']
  choices=v['choices']; assert len(choices)==3 and [c['id'] for c in choices]==['A','B','C']
  assert len(set(c['text'] for c in choices))==3
  assert v['correctOptionId'] in {c['id'] for c in choices}
  assert v['needsEvidenceInRuntime']
assert count==30 and len(bycity)==6
assert set(d['integrationRules']['pilot']).issubset({m['momentId'] for m in M})
print(f'PASS STRUCTURE: {len(M)} moments, {count} variants, cities={bycity}')
print('NOT a runtime test and NOT proof all contextual cues are present.')
