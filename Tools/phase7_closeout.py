"""Bounded Phase 7 evidence indexing. Never edits gameplay or historical evidence."""
from pathlib import Path
import json, hashlib, re, difflib
from PIL import Image, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
QA=ROOT/'Docs/QA/Phase7'
RUN=ROOT/'Validation/Phase7-20261019'
def digest(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def write_json(p,v): p.write_text(json.dumps(v,ensure_ascii=False,indent=2),encoding='utf-8')

def normalize_annotations():
    asset=ROOT/'Assets/_Game/Resources/AdventureUIArt.asset'
    prior=Path('D:/GAMEKKK/Backups/CheckpointLeisureActivitiesFullReview-20261019/Assets/_Game/Resources/AdventureUIArt.asset')
    baseline=json.loads((QA/'baseline.json').read_text(encoding='utf-8-sig'))
    assert digest(prior)==next(e['sha256'] for e in baseline['entries'] if e['path']=='Assets/_Game/Resources/AdventureUIArt.asset')
    old=prior.read_text(encoding='utf-8-sig'); live=asset.read_text(encoding='utf-8-sig'); expected=old
    for entry in json.loads((QA/'bot-alpha-plan.json').read_text(encoding='utf-8-sig')):
        meta=(ROOT/(entry['destination']+'.meta')).read_text()
        guid=re.search(r'^guid: (\w+)',meta,re.M)[1]
        for affected in entry['affected']:
            pattern=r'(^  - id: '+re.escape(affected['artId'])+r'\n.*?)(?=^  - id:|^  \w+:|\Z)'
            before=re.search(pattern,old,re.M|re.S)[0]; now=re.search(pattern,live,re.M|re.S)[0]
            assert re.search(r'^    sprite:.*$',before,re.M)[0]==re.search(r'^    sprite:.*$',now,re.M)[0], 'Map/combat reference changed'
            updated=re.sub(r'^    portrait:.*$',f'    portrait: {{fileID: 21300000, guid: {guid}, type: 3}}',before,flags=re.M)
            updated=re.sub(r'^    status:.*$',f"    status: {affected['oldStatus']}; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved",updated,flags=re.M)
            expected=expected.replace(before,updated,1)
    # Allow only the known five statuses and semantically identical Unicode escape serialization.
    def comparable(text):
        text=re.sub(r'^    status:.*PHASE7_DIALOGUE_ALPHA_CLEAN.*$', '    status: PHASE7_ALPHA',text,flags=re.M)
        return re.sub(r'\\(?:u([0-9a-fA-F]{4})|x([0-9a-fA-F]{2}))',lambda m:chr(int(m[1] or m[2],16)),text)
    assert comparable(live)==comparable(expected), 'Unexpected concurrent art change; refuse normalization'
    asset.write_text(expected,encoding='utf-8',newline='\n')
    (QA/'art-binding.diff').write_text(''.join(difflib.unified_diff(old.splitlines(True),expected.splitlines(True),fromfile='Phase7 baseline',tofile='Dialogue-only alpha repair')),encoding='utf-8')

def sheet(paths,name,cols=4,tile=(270,520)):
    if not paths:return
    rows=(len(paths)+cols-1)//cols
    canvas=Image.new('RGB',(cols*tile[0],rows*tile[1]),'#15222b'); draw=ImageDraw.Draw(canvas)
    for i,p in enumerate(paths):
        image=Image.open(p).convert('RGB'); image.thumbnail((tile[0]-8,tile[1]-42),Image.Resampling.LANCZOS)
        x=(i%cols)*tile[0]; y=(i//cols)*tile[1]
        canvas.paste(image,(x+(tile[0]-image.width)//2,y))
        label=p.stem.replace('P7-BotTalk-','').replace('-after','')
        draw.text((x+4,y+tile[1]-38),label[:39],fill='white')
    canvas.save(RUN/name)

def inventory():
    b=json.loads((QA/'baseline.json').read_text(encoding='utf-8-sig'))
    changes=[]; missing=[]
    for e in b['entries']:
        p=ROOT/e['path']
        if not p.exists():missing.append(e['path'])
        elif digest(p)!=e['sha256']:changes.append({'path':e['path'],'before':e['sha256'],'after':digest(p)})
    write_json(QA/'final-protected-diff.json',{'changed':changes,'missing':missing,'checked':len(b['entries'])})
    runs=[]
    for folder in sorted(RUN.iterdir()):
        log=folder/'smoke-results.txt'
        if not log.exists():continue
        text=log.read_text(encoding='utf-8-sig')
        runs.append({'run':folder.name,'status':'PASS' if 'ALL CHECKS PASSED' in text else 'FAIL' if 'FAILED' in text else 'IN_PROGRESS','assertions':sum(s.startswith('PASS:') for s in text.splitlines()),'evidence':str(log.relative_to(ROOT)).replace('\\','/'),'png':len(list(folder.glob('*.png')))})
    write_json(QA/'run-summary.json',runs)
    frames=json.loads((RUN/'leisure01/frames.json').read_text(encoding='utf-8-sig'))
    samples=[]
    for id in dict.fromkeys(f['id'] for f in frames):
        candidates=[f for f in frames if f['id']==id and f['width']==720 and f['kind']=='motion']
        sample=candidates[1];samples.append(RUN/'leisure01'/sample['file'])
    sheet(samples,'leisure-contact-sheet.jpg',4,(360,680))
    sheet(sorted((RUN/'portraits01').glob('*-after.png')),'portrait-contact-sheet.jpg',5,(288,555))
    sheet(sorted((RUN/'maps01').glob('FP-Map-*.png')),'map-contact-sheet.jpg',3,(360,680))
    print(json.dumps({'runs':runs,'changes':[e['path'] for e in changes],'missing':missing},ensure_ascii=False,indent=2))

if __name__=='__main__':
    normalize_annotations(); inventory()
