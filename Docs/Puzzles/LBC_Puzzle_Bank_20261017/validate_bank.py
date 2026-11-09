#!/usr/bin/env python3
"""Validate LBC optional puzzle bank structure, stable IDs and answer shuffle."""
import json, collections, pathlib, random, unicodedata
p=pathlib.Path(__file__).with_name('LBC_GENERAL_PUZZLE_BANK_V1.json')
data=json.loads(p.read_text('utf-8'))
qs=data['questions']
assert data['schemaVersion']==1
assert len(qs)==data['questionCount']==150
seen=set()
for q in qs:
    assert q['id'] not in seen, q['id']; seen.add(q['id'])
    assert q['difficulty'] in {1,2,3},q['id']
    assert len(q['options'])==4, q['id']
    assert len(set(unicodedata.normalize('NFC',v).casefold().strip() for v in q['options']))==4,q['id']
    assert 0<=q['answerIndex']<4,q['id']
    assert q['options'][q['answerIndex']]==q['answerText'],q['id']
    assert all(q.get(k) and isinstance(q[k],str) for k in ('question','hint','explanation')),q['id']
    for seed in range(10):
        opts=q['options'].copy(); random.Random(f"{q['id']}_{seed}").shuffle(opts)
        assert opts.count(q['answerText'])==1,q['id']
assert len(set(q['category'] for q in qs))==15
assert all(v==10 for v in collections.Counter(q['category'] for q in qs).values())
print('PASS: 150 câu, 15 chủ đề, đáp án ổn định sau 10 lần shuffle mỗi câu; không ID/choice trùng.')
print('Difficulty:',dict(sorted(collections.Counter(q['difficulty'] for q in qs).items())))
