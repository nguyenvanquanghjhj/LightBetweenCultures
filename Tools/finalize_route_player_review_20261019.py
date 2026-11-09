"""Offline review artifacts; never writes production content or historical evidence."""
from pathlib import Path
from collections import Counter
import csv, hashlib, html, json
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/QA/RouteShorteningProposal-20261019'
OUT=ROOT/'Validation/RouteShorteningProposal-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
rows=read(OUT/'waypoint-proposals.json')['rows']
names=dict(zip(['hoi_an','hue','da_lat','jeonju','gyeongju','seoul'],['HoiAn','Hue','DaLat','Jeonju','Gyeongju','Seoul']))
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18)

def contact(files,path,cols=3,width=480):
    cells=[]
    for label,p in files:
        im=Image.open(p).convert('RGB');im.thumbnail((width,360))
        cell=Image.new('RGB',(width,396),'#18383c');cell.paste(im,((width-im.width)//2,0))
        ImageDraw.Draw(cell).text((8,365),label,font=font,fill='#ffebbf');cells.append(cell)
    sheet=Image.new('RGB',(cols*width,((len(cells)+cols-1)//cols)*396),'#18383c')
    for i,cell in enumerate(cells):sheet.paste(cell,((i%cols)*width,(i//cols)*396))
    sheet.save(path)

def page(title,body,path):
    path.write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>'+html.escape(title)+'</title><style>body{background:#163037;color:#fff1d4;font:16px system-ui;margin:24px;line-height:1.55}img{max-width:100%}a{color:#9fe8e1}section{margin:30px 0}.pair{display:grid;grid-template-columns:1fr 1fr;gap:8px}td,th{border:1px solid #678;padding:6px}table{border-collapse:collapse}small{color:#cbd}</style><h1>'+html.escape(title)+'</h1>'+body+'</html>',encoding='utf-8')

runs=[]
for name in ['Runtime-Before01','Runtime-After01','Runtime-After02','Runtime-AfterOverview01','Runtime-Fresh01',
             *['Runtime-Restart-'+c+'-01' for c in names], 'Runtime-H8Frames01','Runtime-Ending-Current','Runtime-Ending-Current02','Runtime-Ending-Restart01']:
    result=OUT/name/'smoke-results.txt'
    if not result.exists():continue
    lines=result.read_text(encoding='utf-8-sig').splitlines()
    runs.append(dict(run=name,result='PASS' if lines[-1]=='ALL CHECKS PASSED' else 'FAIL',assertions=sum(s.startswith('PASS:') for s in lines),path=str(result.relative_to(ROOT)),sha256=sha(result)))
required=['Runtime-Before01','Runtime-After02','Runtime-AfterOverview01','Runtime-Fresh01',*['Runtime-Restart-'+c+'-01' for c in names],'Runtime-H8Frames01']
assert all(any(r['run']==name and r['result']=='PASS' for r in runs) for name in required)
traces=read(OUT/'Runtime-After02/movement-traces.json')
assert len(traces)==68 and all(t['arrived'] and t['maxError']<.025 for t in traces)
dump(DOC/'run-summary.json',dict(runs=runs,required=required,actualTraversals=68,changedEdges=27,
    maxPolylineError=max(t['maxError'] for t in traces),windows='TARGETED_PASS',
    terrain='BEST_EFFORT_PARTIAL_OWNER_REVIEW',android='NOT_TESTED',
    failuresRetained={'Runtime-After01':'Earned seed lacked optional ferry permits; controller correctly refused H17-H5. Replaced QA seed only.',
                      'Runtime-Ending-Current':'Legacy credits fixture used a non-legacy seed; assertion correctly failed. Correct historical seed passed in Current02.'}))

body='<p>27 waypoint lists applied. All 133 nodes / 155 edges retained. Blue = corrected; green = kept; red = unresolved and retained. Technical overlays are not Player screenshots.</p><p><a href="waypoint-proposals.csv">155-edge old/new CSV</a> | <a href="waypoint-proposals.json">Exact coordinate JSON</a> | <a href="RuntimeReview.html">Fresh Windows Player comparison</a></p>'
for city,name in names.items():
    n=sum(r['Changed'] for r in rows if r['City']==city)
    body+=f'<section><h2>{name}: {n} changed</h2><a href="{name}_Current_Proposed.png"><img src="{name}_Current_Proposed.png"></a><p><a href="{name}_Current.png">Before</a> | <a href="{name}_Proposed.png">Applied geometry</a> | <a href="Details/{city}-high-review.png">Owner priority edges only</a></p></section>'
body+='<h2>Retained unresolved routes</h2><table><tr><th>Edge</th><th>Reason</th></tr>'+''.join('<tr><td>'+r['EdgeID']+'</td><td>'+html.escape(r['Reason'])+'</td></tr>' for r in rows if r['Status']=='NEEDS_USER_DECISION')+'</table>'
page('Applied route corrections - 2026-10-19',body,OUT/'RouteReview.html')

body='<p>New Windows captures. Before: pre-patch Build05. After: route-correction-build01. Both use the same private earned save, 1280 x 960, zoom 1, focus (0.5,0.5), active node 0. Dynamic marker/glow animation can vary by frame. After files retain the existing QA helper filename prefix "Before"; the folder and build provenance distinguish them.</p>'
contact_files=[]
for city,name in names.items():
    before=f'Runtime-Before01/Before-{city}-overview.png';after=f'Runtime-AfterOverview01/Before-{city}-overview.png'
    assert Image.open(OUT/before).size==Image.open(OUT/after).size==(1280,960)
    body+=f'<section><h2>{name}</h2><div class="pair"><a href="{before}"><img src="{before}"></a><a href="{after}"><img src="{after}"></a></div></section>'
    contact_files.append((name+' - applied / Player',OUT/after))
body+='<h2>Actual movement / fixed anchors</h2><p>Private edge-isolation fixtures with earned requirements; these are real CityMap.Move traversals, not proof of a new full-story playthrough. Mouse and simulated touch alternate; landscape and 540 x 960 portrait / zoom 2 are checked. No new occlusion was added.</p>'
for city in names:
    folder='Runtime-H8Frames01' if city=='hoi_an' else 'Runtime-After02'
    shots=[f'{folder}/Move-{city}-{i}.png' for i in range(3)]
    assert all((OUT/p).exists() for p in shots)
    body+='<section><h3>'+names[city]+'</h3>'+''.join(f'<a href="{p}"><img style="width:32%" src="{p}"></a>' for p in shots)+'</section>'
body+='<h2>Chua Cau protected occlusion</h2>'+''.join(f'<a href="Runtime-After02/H6-{s}.png"><img style="width:32%" src="Runtime-After02/H6-{s}.png"></a>' for s in ['entry','covered','exit'])
body+='<p><a href="Runtime-After02/movement-traces.json">68 actual traversal traces</a>. All 27 changed edges and seven protected/control edges traversed forward/reverse. Finite waypoint bounds and exact arrivals PASS; this does not prove every illustrated terrain segment is physically walkable.</p>'
page('Windows runtime route review',body,OUT/'RuntimeReview.html')
contact(contact_files,OUT/'SixCities-After-Contact.png')

# Read-only inventory audit, quantities from named QA saves, never inventory grants.
content=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
relics=read(ROOT/'Assets/_Game/Resources/Content/CulturalRelics.json')
saved={
 'old_all_sidequests':read(ROOT/'Validation/FullStory-20261017/Fresh04/full-story-final.json'),
 'earned_six_relics':read(OUT/'Runtime-Ending-Current02/P5-ending-completed.json')}
counts={name:Counter(s['items']) for name,s in saved.items()}
items=[]
for item in content['items']:
    refs=[]
    def walk(obj,path):
        if isinstance(obj,dict):
            for k,v in obj.items():
                if isinstance(v,str) and v==item['id']:refs.append(path+'.'+k)
                elif isinstance(v,(dict,list)):walk(v,path+'.'+k)
        elif isinstance(obj,list):
            for i,v in enumerate(obj):walk(v,f'{path}[{i}]')
    for key in ['quests','events','encounters','puzzles','pickups','shops']:walk(content.get(key,[]),key)
    items.append(dict(itemId=item['id'],name=item['name'],type=item.get('type',''),category=item.get('category',''),
        iconId=item.get('iconId',''),description=item.get('description',''),relatedQuests=item.get('relatedQuestIds',[]),
        source=item.get('source',''),dataReferencePaths=refs,
        **{name:counts[name][item['id']] for name in saved}))
with (DOC/'inventory-catalog-audit.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.DictWriter(f,list(items[0]));w.writeheader();w.writerows({k:json.dumps(v,ensure_ascii=False) if isinstance(v,list) else v for k,v in x.items()} for x in items)
dump(DOC/'inventory-catalog-audit.json',dict(items=items,count=len(items),scope='Read-only. Raw saved quantities; alias suppression remains the existing runtime InventoryPanel responsibility.',
 unknownSavedIds={n:sorted(set(s['items'])-{i['id'] for i in content['items']}) for n,s in saved.items()},
 relics=[dict(id=r['id'],name=r['name'],node=r['restorationNode'],fragmentIds=r['fragmentIds']) for r in relics['relics']]))

ending=OUT/'Runtime-Ending-Current02'
shots=[('HoiAn','P5-ceremony-HoiAn.png'),('Hue','P5-ceremony-Hue.png'),('DaLat','P5-ceremony-DaLat.png'),('Jeonju','P5-ceremony-Jeonju.png'),('Gyeongju','P5-ceremony-Gyeongju.png'),('Seoul','P5-ceremony-Seoul.png'),('Six lights','P5-six-lights-converge.png'),('Lumi','P5-Lumi-connection.png'),('Dialogue','P5-ending-dialogue.png'),('Credits','P5-credits.png'),('Return to HoiAn','P5-world-map-after-ending.png'),('WorldMap / real restart','../Runtime-Ending-Restart01/P5-WorldMap-after-credits.png')]
contact([(label,ending/file) for label,file in shots],OUT/'Ending-Contact.png')
page('Current ending - audit evidence', '<p>Unchanged ending, tested in existing Build05 before the waypoint-only patch. Explicit legacy-save matrix fixtures are documented in input-provenance.json; the final ceremony ran naturally for 24 seconds. These captures do not represent a fresh whole-story playthrough. No ending gameplay or rewards changed. No video produced.</p><a href="Ending-Contact.png"><img src="Ending-Contact.png"></a>'+''.join(f'<section><h2>{label}</h2><a href="Runtime-Ending-Current02/{file}"><img src="Runtime-Ending-Current02/{file}"></a></section>' for label,file in shots),OUT/'EndingReview.html')

changed=[r for r in rows if r['Changed']]
dump(DOC/'handoff-summary.json',dict(changedByCity={city:sum(r['Changed'] for r in rows if r['City']==city) for city in names},
    changedEdgeIds=[r['EdgeID'] for r in changed],retained=128,
    unresolved=[r['EdgeID'] for r in rows if r['Status']=='NEEDS_USER_DECISION'],
    currentNativeLength=sum(r['CurrentLength'] for r in changed),newNativeLength=sum(r['ProposedLength'] for r in changed),
    directNeighbors=read(DOC/'preservation-verification.json')['degrees'],
    windowsBuildLog='Logs/route-correction-build01.log',playerSHA256=sha(ROOT/'Builds/Windows/LightBetweenCultures.exe')))
print('Review galleries, route CSV, 38-item audit, and ending contact sheet written.')
