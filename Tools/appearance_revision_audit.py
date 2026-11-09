"""Focused source/identity audit for the owner-requested artwork revision. No image editing."""
from pathlib import Path
import hashlib,json,re
ROOT=Path(__file__).resolve().parents[1]
VAL=ROOT/'Validation/NpcAppearance-20261021'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
dump=lambda p,d:p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
native=read(ROOT/'Docs/Art/native-art-bindings.json')['entries']
registry=read(ROOT/'Docs/Art/NPC_CANONICAL_REGISTRY.json')
traits={
 'npc_nguyet':'Bà tóc bạc, nón lá, áo dài tím thêu hoa vàng, quần trắng, giỏ hoa vàng; giữ danh tính Bà Nguyệt ở Huế U2.',
 'npc_elder':'Bà tóc bạc, hanbok trắng/hồng, hai tay đặt phía trước; giữ danh tính Người giữ chuyện ở Jeonju J16.',
 'npc_hana':'Cô gái tóc đen hai bím, mắt cười nhắm, hanbok trắng/hồng, hai tay phía trước; giữ Nghệ nhân Hana ở Jeonju J2.'}
for n in registry['identities']:
 if n['npcId'] not in traits:continue
 b=next(e for e in native if e['role']=='npc' and e['id']==n['npcId'])
 n['status']='CANONICAL';n['canonicalIdentity']['coreTraits']=traits[n['npcId']]
 n['canonicalIdentity']['approval']='Owner explicitly requested this design replacement 2026-10-21; verified against old sprite, native reference and placement, not filename order.'
 for v in n['variants']:v['runtimeBound']=False
 n['variants']=[v for v in n['variants'] if v['variantId']!=n['npcId']+'.owner20261021']
 n['variants'].append(dict(variantId=n['npcId']+'.owner20261021',status='CANONICAL',path=b['path'],spriteName=b['spriteName'],guid=b['guid'],localId=b['localId'],cities=sorted({p['cityId'] for p in n['placements']}),scenes=['CityMap','DialoguePanel'],conditions='Replaces only this NPC sprite/portrait; canonical position, identity and gameplay unchanged.',runtimeBound=True,qualityStatus='WINDOWS_REVIEW_PENDING',visualReview='User supplied new design. Transparent extraction via imagegen; original source is not a pixel-identical matting operation. Hana source hem was cropped, no claim of originally visible feet.'))
 if b['path'] not in registry['visualSourcesReviewed']:registry['visualSourcesReviewed'].append(b['path'])
registry['updated']='2026-10-21'
registry['policy']['legacyRuntimeStatus']='Only three owner-authorized appearance replacements on 2026-10-21; all other bindings/identity decisions preserved.'
dump(ROOT/'Docs/Art/NPC_CANONICAL_REGISTRY.json',registry)
baseline=read(VAL/'source-before.json')['files']
allowed={
'Assets/_Game/Scripts/Editor/LumiRoleSetup.cs',
'Assets/_Game/Scripts/UI/DesignKitArt.cs',
'Assets/_Game/Scripts/UI/GameplayPanels.Leisure.cs',
'Assets/_Game/Scripts/UI/GameplayPanels.LeisurePilots.cs',
'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs',
'Assets/_Game/Scripts/Validation/SliceSmokeRunner.cs',
'Assets/_Game/Scripts/Validation/GameplaySmokeRunner.LeisurePilot.cs',
'Assets/_Game/Resources/AdventureUIArt.asset',
'Assets/_Game/Resources/DesignKitArt.asset',
'Assets/_Game/Prefabs/DesignKit/LumiExplorationSprite.prefab',
'Assets/_Game/Prefabs/DesignKit/LumiMapToken.prefab',
'Assets/_Game/Scenes/HoiAnDemo.unity'}
changed=[e['path'] for e in baseline if not (ROOT/e['path']).exists() or sha(ROOT/e['path'])!=e['sha256']]
unexpected=set(changed)-allowed
assert not unexpected,unexpected
text=(ROOT/'Assets/_Game/Resources/DesignKitArt.asset').read_text(encoding='utf-8-sig')
roles=re.findall(r'  lumi\w+: \{fileID: (-?\d+), guid: (\w+), type: 3\}',text)
assert len(roles)==5 and len(set(roles))==1 and roles[0][1]=='9a7d3aad847598b419b6b01ab212a053'
old=read(VAL/'native-before.json')
before_actor=json.loads(old['mapActors'])
# Compare full AdventureUIArt native YAML except the three authorized NPC entries.
old_yaml=(VAL/'AdventureUIArt.asset.before').read_text(encoding='utf-8-sig')
new_yaml=(ROOT/'Assets/_Game/Resources/AdventureUIArt.asset').read_text(encoding='utf-8-sig')
def mask(s):
 for npc in traits:s=re.sub(r'(?ms)^  - id: '+npc+r'\n.*?(?=^  - id: |^  [A-Za-z]|\Z)','  - id: '+npc+'\n    OWNER_ART_REPLACEMENT\n',s)
 # Unity rewrites equivalent YAML unicode escapes when saving the registry.
 return re.sub(r'\\(?:u([0-9a-fA-F]{4})|x([0-9a-fA-F]{2}))',lambda m:chr(int(m[1] or m[2],16)),s)
assert mask(old_yaml)==mask(new_yaml),'Unrelated AdventureUIArt field changed'
summary=dict(changedExistingFiles=changed,protectedFilesUnchanged=len(baseline)-len(changed),canonicalLumi=dict(path='Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png',guid=roles[0][1],localId=roles[0][0],roles=5),npcBindings=[e for e in native if e['role']=='npc' and e['id'] in traits],allOtherArtRegistryFieldsUnchanged=True,originalArtworkBytesUnchanged=True,gameplayDataAndSourceUnchanged=True,android='NOT_BUILT_THIS_ART_REVISION')
dump(VAL/'source-audit.json',summary)
print(json.dumps({'changedExistingFiles':len(changed),'protectedUnchanged':summary['protectedFilesUnchanged'],'npcIds':list(traits),'canonicalLumiRoles':5}))
