"""Bounded import/reference and frozen gameplay checks for the leisure-only review."""
from pathlib import Path
import csv,hashlib,json,re
from PIL import Image
ROOT=Path(__file__).resolve().parents[1];DOC=ROOT/'Docs/QA/LeisureActivitiesFullReview-20261019'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
dump=lambda p,x:p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
b=read(DOC/'baseline.json');before=Path(b['before']);checks=[]
def check(ok,text):
    checks.append(dict(result='PASS' if ok else 'FAIL',check=text));assert ok,text

# Unity's builder adds platform icon slots and normalizes three unrelated escaped
# description strings. Prove semantic equality before retaining their prior bytes.
settings='ProjectSettings/ProjectSettings.asset';old=(before/settings).read_bytes();new=(ROOT/settings).read_bytes()
pattern=rb'(?ms)^  m_BuildTargetPlatformIcons:.*?(?=^  m_BuildTargetBatching:)'
check(re.sub(pattern,b'',old)==re.sub(pattern,b'',new),'Project settings unchanged outside Unity-generated platform icon slots')
if old!=new:(ROOT/settings).write_bytes(old)
asset='Assets/_Game/Resources/AdventureUIArt.asset';old=(before/asset).read_bytes();new=(ROOT/asset).read_bytes()
pattern=rb'(?ms)^  leisureBackgrounds:.*?(?=^  [A-Za-z]\w*:|\Z)'
block=re.search(pattern,new);check(block is not None,'Ten existing background references serialized')
newlines=re.sub(pattern,b'',new).decode('utf-8-sig').splitlines();oldlines=old.decode('utf-8-sig').splitlines()
check(len(newlines)==len(oldlines),'No unrelated registry fields added or removed')
for x,y in zip(oldlines,newlines):
    if x==y:continue
    check(x.lstrip().startswith('description: "') and y.lstrip().startswith('description: "') and
          x.split(': ',1)[1][1:-1].encode('ascii').decode('unicode_escape')==y.split(': ',1)[1][1:-1].encode('ascii').decode('unicode_escape'),
          'Unrelated description serialization has equal decoded text')
anchor=re.search(rb'^  leisureBoatBackground:.*\r?\n',old,re.M);assert anchor
expected=old[:anchor.end()]+block[0]+old[anchor.end():]
if new!=expected:(ROOT/asset).write_bytes(expected)
content='Assets/_Game/Resources/Content/LeisureActivities.json'
a=read(before/content)['entries'];c=read(ROOT/content)['entries'];check(len(a)==len(c)==12,'Same twelve activity definitions and nodes')
for x,y in zip(a,c):
    if x['mechanic'] in ['seat','bell']:
        check({k:v for k,v in x.items() if k!='title'}=={k:v for k,v in y.items() if k!='title'},'Only requested activity title changed: '+x['nodeId'])
    else:check(x==y,'Activity data exact: '+x['nodeId'])
delta=[];same=0
for e in b['protected']:
    if sha(ROOT/e['path'])==e['sha256']:same+=1
    else:check(e['path'] in set(b['editable'])-{settings},'Only declared source/import delta: '+e['path']);delta.append(e['path'])
check('Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs' not in delta,'Both pilot animation implementations byte-exact')
mapping={'release':'HoiAn_LanternRelease_BG','seat':'Hue_Throne_BG','pick':'DaLat_Strawberry_BG','fly':'DaLat_Kite_BG','water':'Jeonju_Watering_BG','tea':'Jeonju_Tea_B','bell':'Gyeongju_Bell_BG','wish':'Gyeongju_Wishes_','snack':'Seoul_Tteokbokki_BG','dance':'Seoul_Kpop_BG','swing':'HoiAn_Swing_BG','boat':'Hue_PedalBoat_BG'}
inventory=[]
for e in c:
    name=mapping[e['mechanic']];path='Assets/_Game/Art/Leisure/Backgrounds/'+name+'.png';p=ROOT/path;im=Image.open(p)
    meta=p.with_suffix('.png.meta').read_text('utf-8');guid=re.search(r'^guid: (\w+)',meta,re.M)[1]
    check(im.size==(1448,1086),'Native full-resolution background: '+name)
    prior=before/(path+'.meta')
    if not prior.exists():prior=Path(b['baseCheckpoint'])/(path+'.meta')
    oldmeta=prior.read_text('utf-8-sig')
    for rx in [r'^guid:.*$',r'^  spriteMode:.*$',r'^      internalID:.*$',r'^      name:.*$']:
        check(re.findall(rx,oldmeta,re.M)==re.findall(rx,meta,re.M),'Keep GUID / sprite mode / localID: '+name+' '+rx)
    check('    textureCompression: 0' in meta,'Uncompressed background: '+name)
    check(guid.encode() in (ROOT/asset).read_bytes(),'Background GUID really bound: '+name)
    inventory.append(dict(id=e['id'],city=e['cityId'],node=e['nodeId'],mechanic=e['mechanic'],background=path,guid=guid,pixels=f'{im.width}x{im.height}',backgroundSHA256=sha(p),prop='Assets/_Game/Resources/'+e['spritePath']+'.png',propSHA256=sha(ROOT/('Assets/_Game/Resources/'+e['spritePath']+'.png')),status='BOUND_EXISTING_SOURCE',note='Existing alternate filename preserved' if e['mechanic'] in ['tea','wish'] else ''))
dump(DOC/'asset-bindings.json',inventory)
with (DOC/'asset-bindings.csv').open('w',encoding='utf-8-sig',newline='') as f:
    w=csv.DictWriter(f,list(inventory[0]));w.writeheader();w.writerows(inventory)
dump(DOC/'preservation.json',dict(result='PASS',unchangedFrozenFiles=same,changedExistingFiles=delta,checks=checks,scope='Scoped 411-file baseline, not whole-project hashing. Original PNGs, two pilots, maps, graph, routes, save, quest, inventory, NPC, audio and canonical Lumi preserved.'))
print('PASS',len(checks),'checks;',same,'protected files unchanged;',len(delta),'declared modifications')
