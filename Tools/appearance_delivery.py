"""Finalize the focused artwork review, retaining real Player evidence and exact limitations."""
from pathlib import Path
import json,hashlib,html
ROOT=Path(__file__).resolve().parents[1];VAL=ROOT/'Validation/NpcAppearance-20261021'
read=lambda p:json.loads(p.read_text(encoding='utf-8-sig'))
dump=lambda p,d:p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
final=(VAL/'final01/restart-results.txt').read_text(encoding='utf-8-sig')
before=(VAL/'before01/restart-results.txt').read_text(encoding='utf-8-sig')
assert final.rstrip().endswith('ALL CHECKS PASSED') and before.rstrip().endswith('ALL CHECKS PASSED')
assert final.count('Closing leisure visual retains exact save:')==12
registry=read(ROOT/'Docs/Art/NPC_CANONICAL_REGISTRY.json')
ids={'npc_nguyet','npc_elder','npc_hana'}
for n in registry['identities']:
 if n['npcId'] not in ids:continue
 for v in n['variants']:
  if v.get('runtimeBound'):
   v['qualityStatus']='WINDOWS_RUNTIME_VERIFIED; OWNER_PREVIEW_PENDING'
   v['visualReview']='Real Windows Player map/dialogue checks and before/after images. No background square or black fringe observed. Source artwork alpha preserved; mipmaps/trilinear reduce minification aliasing. '+('Original Hana source crops the skirt bottom; rounded cutout hem needs owner review.' if n['npcId']=='npc_hana' else 'Feet/hem/hat/basket visible as provided; elder feet remain hidden under the hanbok.')
  else:v['conditions']='Historical art preserved for recovery; replaced by the owner-authorized 2026-10-21 default, not bound in normal runtime.'
dump(ROOT/'Docs/Art/NPC_CANONICAL_REGISTRY.json',registry)
baseline={e['path']:e['sha256'] for e in read(VAL/'source-before.json')['files']}
current={p.relative_to(ROOT).as_posix():p for folder in ('Assets','Packages','ProjectSettings') for p in (ROOT/folder).rglob('*') if p.is_file()}
changed=[rel for rel,p in current.items() if hashlib.sha256(p.read_bytes()).hexdigest()!=baseline.get(rel)]
(VAL/'modified-files.txt').write_text('\n'.join(sorted(changed))+'\n\nDocumentation/tools: Docs/Art/LUMI_CANONICAL_20261021.md; Docs/Art/NPC_CANONICAL_REGISTRY.json/md; Docs/Art/native-art-bindings.json; Docs/Art/ART_ASSET_CATALOG.md; Docs/Art/ART_SOURCE_INDEX.md; Docs/Art/art-asset-manifest.csv/json; Docs/Art/IdentityPolicy-20261017/VisualReview.html; Docs/Art/DESIGN_KIT_INTEGRATION_VI.md; Tools/art_asset_catalog.py; Tools/appearance_revision_audit.py; Tools/appearance_delivery.py; Tools/checkpoint_appearance_revision.py\n',encoding='utf-8')
cards=[]
for id,title in [('npc_nguyet','Bà Nguyệt · npc_nguyet · Huế U2'),('npc_hana','Nghệ nhân Hana · npc_hana · Jeonju J2'),('npc_elder','Người giữ chuyện · npc_elder · Jeonju J16')]:
 for view in ('map','dialogue'):
  cards.append('<h2>'+html.escape(title+' — '+view)+'</h2><div class="pair">'+''.join('<figure><figcaption>'+label+'</figcaption><a href="'+folder+'/ART-'+id+'-'+view+'-'+kind+'.png"><img loading="lazy" src="'+folder+'/ART-'+id+'-'+view+'-'+kind+'.png"></a></figure>' for label,folder,kind in [('Trước (binding cũ trong cùng Player)','before01','before'),('Sau — bản cuối','final01','after')])+'</div>')
cards.append('<h2>Lumi — menu trước / sau</h2><div class="pair">'+''.join('<figure><figcaption>'+label+'</figcaption><img loading="lazy" src="'+folder+'/ART-Lumi-menu-'+kind+'.png"></figure>' for label,folder,kind in [('Trước','before01','before'),('Sau','final01','after')])+'</div>')
for name in ['H3-after','combat-after','guide-after','leisure-swing','leisure-tea','leisure-dance','map-hoi_an','map-da_lat','map-gyeongju','map-seoul']:
 cards.append('<details><summary>Lumi — '+name+'</summary><img loading="lazy" src="final01/ART-Lumi-'+name+'.png"></details>')
(VAL/'gallery.html').write_text('<!doctype html><meta charset="utf-8"><title>LBC — 3 NPC & canonical Lumi</title><style>body{background:#173f43;color:#fff3d8;font:16px system-ui;max-width:1160px;margin:24px auto;padding:16px}h1,h2{color:#f1cc88}.pair{display:flex;gap:20px;flex-wrap:wrap}figure{margin:0;flex:1;min-width:260px}img{width:100%;max-width:540px}figcaption{padding:12px}a{color:#8fe8d0}details{margin:20px 0}</style><h1>Ba ngoại hình NPC + Lumi trắng</h1><p>Windows runtime · 540×960 · 297 checks PASS · tất cả 12 leisure. Không phải blind human playtest. Không build Android.</p><p>Before: đổi binding trong bộ fixture opt-in của cùng Windows Player; không rollback source hay dùng save thật. After: serialized defaults của bản cuối. Hội thoại/map được mở bằng input runtime. Guide/combat là presentation fixture, không tuyên bố chơi lại toàn campaign/ending.</p><p>Ảnh NPC tách nền bằng imagegen, không phải matte giữ nguyên từng pixel. Hana: nguồn bị cắt sát đáy váy, phần gấu váy cần owner duyệt. Ending/ceremony đã audit binding, chưa replay trong lượt này.</p><p><a href="modified-files.txt">File thực sự sửa</a> · <a href="final01/restart-results.txt">Windows results</a> · <a href="source-audit.json">Source protection audit</a></p>'+''.join(cards),encoding='utf-8')
captures=[]
for folder in ('before01','final01'):
 for p in sorted((VAL/folder).glob('ART-*.png')):captures.append(dict(path=p.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest(),bytes=p.stat().st_size))
summary=dict(windows='FOCUSED_RUNTIME_PASS',assertions=sum(l.startswith('PASS:') for l in final.splitlines()),leisureActivities=12,renderResolution='540x960',beforeComparison='Old bindings in an explicit same-Player QA fixture, not an old executable',android='NOT_BUILT_NOT_TESTED_THIS_ART_REVISION',npcIds=sorted(ids),lumiCanonical='Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png',limitations=['Hana source hem cropped; derived rounded hem awaits owner visual approval','NPC cutouts are imagegen-derived, not pixel-identical matting','Quest completion end-to-end and canonical ending/ceremony not replayed; original logic/data remain byte-identical'],captures=captures,initialQaFailure='after01 includes a false test failure because a held watering-can layer was counted as Lumi. Predicate corrected. after-tail01 and final01 PASS. Failed evidence retained.',windowsPlayer='Builds/Windows-AndroidTouch-20261020/LightBetweenCultures.exe',sourceChanges=sorted(changed))
dump(VAL/'delivery-summary.json',summary)
print(json.dumps({'windows':summary['windows'],'assertions':summary['assertions'],'captures':len(captures),'sourceChangedOrNew':len(changed)}))
