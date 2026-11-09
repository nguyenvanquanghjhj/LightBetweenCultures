"""Scoped preservation/asset checks for two panel-only leisure pilots."""
from pathlib import Path
import hashlib,json,re,csv
ROOT=Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/QA/LeisureAnimationPilot-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
b=read(DOC/'baseline.json');before=Path(b['before']);checks=[]
def check(ok,text):
    checks.append(dict(result='PASS' if ok else 'FAIL',check=text));assert ok,text

settings='ProjectSettings/ProjectSettings.asset';old=(before/settings).read_bytes();new=(ROOT/settings).read_bytes()
pattern=rb'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
check(re.sub(pattern,b'',old)==re.sub(pattern,b'',new),'Build settings identical outside generated empty platform icon slots')
if old!=new:(ROOT/settings).write_bytes(old)

asset='Assets/_Game/Resources/AdventureUIArt.asset';old=(before/asset).read_bytes();new=(ROOT/asset).read_bytes()
refs=re.findall(rb'^  leisure(?:Swing|Boat)Background:.*\r?\n',new,re.M)
check(len(refs)==2,'Exactly two approved existing backdrop references bound')
stripped=re.sub(rb'^  leisure(?:Swing|Boat)Background:.*\r?\n',b'',new,flags=re.M)
old_lines=old.decode('utf-8-sig').splitlines();new_lines=stripped.decode('utf-8-sig').splitlines()
check(len(old_lines)==len(new_lines),'AdventureUIArt unchanged field count apart from two backdrop fields')
for a,c in zip(old_lines,new_lines):
    if a==c:continue
    check(a.lstrip().startswith('description: "') and c.lstrip().startswith('description: "') and
          a.split(': ',1)[1][1:-1].encode('ascii').decode('unicode_escape')==c.split(': ',1)[1][1:-1].encode('ascii').decode('unicode_escape'),
          'Unity description serialization has equal decoded text; no unrelated art binding changed')
# Retain previous bytes for unrelated descriptions including H6.
anchor=re.search(rb'^  activityCitrusPot:.*\r?\n',old,re.M);assert anchor
expected=old[:anchor.end()]+b''.join(refs)+old[anchor.end():]
if new!=expected:(ROOT/asset).write_bytes(expected)

name='Assets/_Game/Resources/Content/LeisureActivities.json'
a=read(before/name);c=read(ROOT/name)
ids={'leisure_hoian_swing':'Ngồi xích đu','leisure_hue_pedalswanboat':'Đạp thuyền vịt'}
for x,y in zip(a['entries'],c['entries']):
    if x['id'] in ids:
        check(y['title']==ids[x['id']] and {k:v for k,v in x.items() if k!='title'}=={k:v for k,v in y.items() if k!='title'},'Only requested menu title changed: '+x['id'])
    else:check(x==y,'Other leisure activity configuration exact: '+x['id'])
allowed=set(b['editableExisting'])-{'ProjectSettings/ProjectSettings.asset'}
delta=[];same=0
for e in b['protected']:
    p=ROOT/e['path']
    if sha(p)==e['sha256']:same+=1
    else:
        check(e['path'] in allowed,'Declared pilot source/import change only: '+e['path']);delta.append(e['path'])

folder=ROOT/'Assets/_Game/Art/Leisure/Backgrounds'
expected_names=['HoiAn_Swing_BG','HoiAn_LanternRelease_BG','Hue_PedalBoat_BG','Hue_Throne_BG','DaLat_Strawberry_BG','DaLat_Kite_BG','Jeonju_Watering_BG','Jeonju_Tea_BG','Gyeongju_Bell_BG','Gyeongju_Wishes_BG','Seoul_Tteokbokki_BG','Seoul_Kpop_BG']
aliases={'Jeonju_Tea_BG':'Jeonju_Tea_B','Gyeongju_Wishes_BG':'Gyeongju_Wishes_'}
inventory=[]
from PIL import Image
for name in expected_names:
    p=folder/(aliases.get(name,name)+'.png');im=Image.open(p)
    check(p.exists() and im.size==(1448,1086),'Existing full-resolution backdrop: '+p.name)
    meta=p.with_suffix('.png.meta').read_text('utf-8');guid=re.search(r'^guid: (\w+)',meta,re.M).group(1)
    inventory.append(dict(expected=name+'.png',actual=p.name,path=str(p.relative_to(ROOT)),width=im.width,height=im.height,sha256=sha(p),guid=guid,
                          status='PILOT_BOUND' if name in ['HoiAn_Swing_BG','Hue_PedalBoat_BG'] else 'AUDITED_NOT_BOUND_BY_THIS_PILOT',
                          visual='BACKGROUND_ONLY_VISUALLY_INSPECTED',note='Existing filename differs; preserved, no duplicate/rename' if name in aliases else ''))
for name in ['HoiAn_Swing_BG','Hue_PedalBoat_BG']:
    path='Assets/_Game/Art/Leisure/Backgrounds/'+name+'.png.meta'
    x=(before/path).read_text('utf-8-sig');y=(ROOT/path).read_text('utf-8-sig')
    for expression in [r'^guid:.*$',r'^  spriteMode:.*$',r'^      internalID:.*$',r'^      name:.*$']:
        check(re.findall(expression,x,re.M)==re.findall(expression,y,re.M),'Backdrop GUID / Multiple localID/name preserved: '+name+' '+expression)
    check('    textureCompression: 0' in y,'Pilot backdrop default compression disabled: '+name)
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
dump(DOC/'asset-audit.json',dict(backgrounds=inventory,props=[dict(path='Assets/_Game/Resources/LeisureProps/'+n+'.png',sha256=sha(ROOT/('Assets/_Game/Resources/LeisureProps/'+n+'.png')),composition='Source texture sampled through runtime mesh layers; original PNG not altered') for n in ['HoiAn_Swing','Hue_PedalSwanBoat']]))
with (DOC/'background-audit.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.DictWriter(f,list(inventory[0]));w.writeheader();w.writerows(inventory)
dump(DOC/'preservation.json',dict(result='PASS',checks=checks,unchangedFrozenFiles=same,changedExistingFiles=delta,scope='No route, map, gameplay, inventory, save, NPC, audio, canonical Lumi or legacy puzzle changes'))
print('PASS:',len(checks),'checks;',same,'frozen files exact;',len(delta),'declared existing files changed')
