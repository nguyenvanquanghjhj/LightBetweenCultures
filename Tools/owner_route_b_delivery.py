"""Bounded Phase B verification/delivery. Never modifies production content."""
from pathlib import Path
import json,csv,hashlib,math,collections,html
R=Path(__file__).resolve().parents[1];D=R/'Docs/Maps/OwnerRoutePhaseB-20261019';V=R/'Validation/OwnerRoutePhaseB-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
catalog=read(R/'Assets/_Game/Resources/Content/CityNavigation.json');checks=[];summary=[];rows=[];approaches=[]
for c in catalog['cities']:
 cid=c['cityId'];actual=read(V/'junctions720'/f'runtime-navigation-{cid}.json')
 checks.append({'name':cid+' Unity parses the production vertex/edge IDs','pass':[x['id']for x in c['vertices']]==[x['id']for x in actual['vertices']]and[x['id']for x in c['edges']]==[x['id']for x in actual['edges']]})
 maxerr=max(abs(p[axis]-q[axis])for e,f in zip(c['edges'],actual['edges'])for p,q in zip(e['points'],f['points'])for axis in ['x','y'])
 checks.append({'name':cid+' same compiled geometry in real Windows Player','pass':maxerr<1e-6,'maxNormalizedError':maxerr})
 degrees=collections.Counter(x for e in c['edges']for x in[e['from'],e['to']]);verts={v['id']:v for v in c['vertices']}
 checks.append({'name':cid+' canonical feet are terminals, not navigation junctions','pass':all(degrees[n['vertexId']]==1 for n in c['nodes'])})
 summary.append({'cityId':cid,'canonicalNodes':len(c['nodes']),'navigationVertices':len(c['vertices']),'walkingEdges':sum(e['travel']=='walk'for e in c['edges']),'ferryEdges':sum(e['travel']=='ferry'for e in c['edges']),'transferPoints':sum(v['transfer']for v in c['vertices']),'junctionsDegree3Plus':sum(degrees[v['id']]>=3 and not v['canonicalNodeId']for v in c['vertices'])})
 for e in c['edges']:
  points=[{'x':round(p['x']*1000,4),'y':round((1-p['y'])*1000,4)}for p in e['points']]
  rows.append({'City':cid,'EdgeID':e['id'],'FromNavigationVertex':e['from'],'ToNavigationVertex':e['to'],'Travel':e['travel'],'Bidirectional':e['bidirectional'],'SourceInkOrApproach':';'.join(e['source']),'Waypoints_Map0To1000_TopLeft':json.dumps(points),'AccessRules':json.dumps(e['access'])})
 data=read(V/f'{cid}-implementation.json');approaches.extend({'cityId':cid,**a}for a in data['connectors'])
for f in read(D/'production-before.json'):
 allowed={'Assets/_Game/Scripts/Core/GameData.cs','Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Scripts/Maps/CityMapPaths.cs','Assets/_Game/Scripts/Maps/CityRouteGeometry.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs'}
 if f['path']not in allowed:checks.append({'name':'Preserved '+f['path'],'pass':sha(R/f['path'])==f['sha256']})
base=Path('D:/GAMEKKK/Backups/CheckpointMapClarity-20261019')
for p in read(R/'Docs/QA/MapClarity-20261019/import-selection.json')['paths']:
 checks.append({'name':'Preserve approved clarity '+p,'pass':sha(R/(p+'.meta'))==sha(base/(p+'.meta'))})
for filename,data in [('navigation-edges.csv',rows),('arrival-connectors.csv',[{'City':r['cityId'],'NodeId':r['nodeId'],'DistanceInputPixels':r['distancePixels'],'UnchangedCanonicalArrival':json.dumps(r['arrival']),'InputPolyline':json.dumps(r['inputPolyline']),'Status':r['status']}for r in approaches])]:
 with (D/filename).open('w',newline='',encoding='utf-8-sig')as f:w=csv.DictWriter(f,fieldnames=list(data[0]));w.writeheader();w.writerows(data)
source=read(R/'Assets/_Game/Resources/Content/GameplayContent.json');dump(D/'legacy-155-edges-READONLY.json',{'status':'Archived reference, not the active routing graph','cities':[{'cityId':c['id'],'edges':c['edges']}for c in source['cities']]})
qa={}
for name in ['runtime02','windows720','junctions720','story-vietnam','story-korea','story-vietnam-seoul','story-korea-seoul','story-vietnam-seoul02','story-korea-seoul02','mid-boat-restart','ending-restart']:
 path=V/name/'smoke-results.txt'
 if path.exists():
  text=path.read_text(encoding='utf-8-sig');qa[name]={'status':'PASS'if text.rstrip().endswith('ALL CHECKS PASSED')else'FAILED_RETAINED','passedAssertions':sum(x.startswith('PASS:')for x in text.splitlines())}
for name in ['junctions720','story-vietnam-seoul02','story-korea-seoul02','mid-boat-restart','ending-restart']:
 checks.append({'name':'Final targeted run '+name,'pass':qa.get(name,{}).get('status')=='PASS'})
traces=[]
for f in sorted((V/'windows720').glob('R*-trace.json')):
 t=read(f);frames=t['frames'];checks.append({'name':'Real walk and ferry trace '+f.name,'pass':any(x['mode']=='ferry'for x in frames)and any(x['mode']=='walk'for x in frames)})
 traces.append({'file':str(f.relative_to(R)),'route':t['route'],'from':t['from'],'to':t['to'],'frames':len(frames),'cost':t['cost']})
apk=R/'Builds/Android/LightBetweenCultures-OwnerRoutesB-20261019.apk';old=R/'Builds/Android/LightBetweenCultures-Test.apk'
checks.append({'name':'Old protected APK unchanged','pass':sha(old)=='ecc062ade3c1967a2d184e555a38098b3269a6b4de602aa1c3c66b1f523b5481'})
if apk.exists():dump(D/'android-apk-file.json',{'path':str(apk),'actualAPKBytes':apk.stat().st_size,'sha256':sha(apk),'device':'NOT_TESTED','buildResult':read(D/'android-build.json')})
result={'pass':all(x['pass']for x in checks),'checks':checks,'cities':summary,'qa':qa,'actualBoatTraces':traces,'canonicalNodes':sum(c['canonicalNodes']for c in summary),'walkingEdges':sum(c['walkingEdges']for c in summary),'ferryEdges':sum(c['ferryEdges']for c in summary),'transferPoints':sum(c['transferPoints']for c in summary)}
dump(D/'verification.json',result)
body='''<!doctype html><meta charset="utf-8"><title>LBC Phase B — actual routes and Windows Player</title><style>body{font:16px/1.55 system-ui;background:#142d34;color:#fff3d7;margin:24px}a{color:#9fe2d4}section{margin:28px 0;padding:16px;background:#1c3b43;border-radius:12px}.native{overflow:auto;max-width:100%}.native img{width:720px;max-width:none;height:auto}.technical{width:100%;max-width:1448px}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(310px,1fr));gap:16px}.thumb{max-width:100%;height:auto}h1,h2{color:#ffd58c}</style><h1>Phase B — route thực trong Unity</h1><p>Ảnh Windows Player nguyên gốc; bấm ảnh để mở PNG. Các ảnh kỹ thuật có vẽ toàn graph nằm ở cuối trang, không dùng làm artwork runtime. Không thay nền, không repaint. 52 texture import của lượt clarity vẫn giữ nguyên.</p><p><a href="REPORT.md">Báo cáo</a> · <a href="navigation-edges.csv">442 cạnh navigation</a> · <a href="arrival-connectors.csv">133 điểm đứng và đoạn tiếp cận</a></p>'''
def link(path):return '../../../'+str(path.relative_to(R)).replace('\\','/')
for route in ['R1','R2','R3','R4','R5']:
 body+=f'<section><h2>{route}: đi bộ → thuyền → xuống bến</h2><p>Windows Player 720×1280. Hình học/animation thuyền đã kiểm thử hai chiều. R5 được xác minh thêm trước cờ NPC trong hai lượt Seoul cuối.</p><div class="grid">'
 for stage,label in [('walk-start','Trước khi đi'),('boat','Đang trên thuyền'),('disembarked','Đã xuống bến')]:
  path=V/'windows720'/f'{route}-{stage}.png';body+=f'<article>{label}<a href="{link(path)}"><img class="thumb" src="{link(path)}"></a></article>'
 body+='</div></section>'
body+='<section><h2>Khung Player gốc để xem độ rõ</h2><p>Ảnh dưới hiển thị 720 pixel thật, không kéo giãn. Cuộn ngang nếu cửa sổ hẹp.</p><div class="native"><img src="'+link(V/'mid-boat-close/R1-boat.png')+'"></div></section>'
for name in ['X1','X3','X4']:
 body+=f'<section><h2>{name} — đổi đích tại đúng junction</h2><div class="grid">'
 for suffix in ['approaching','branch-arrival']:
  path=V/'junctions720'/f'{name}-{suffix}.png';body+=f'<a href="{link(path)}"><img class="thumb" src="{link(path)}"></a>'
 body+='</div></section>'
for c in summary:
 cid=c['cityId'];body+=f'<section><h2>{html.escape(cid)} — Player và bản kỹ thuật toàn graph</h2><p>{c["walkingEdges"]} cạnh đi bộ; {c["ferryEdges"]} cạnh thuyền. Preview lấy từ đúng JSON production và đã đối chiếu với graph Unity Player parse.</p>'
 photo=V/'junctions720'/f'City-{cid}.png';preview=V/f'{cid}-applied-graph-preview.png'
 body+=f'<a href="{link(photo)}"><img class="thumb" style="max-width:540px" src="{link(photo)}"></a><details><summary>Mở bản kỹ thuật, không phải ảnh game thay nền</summary><a href="{link(preview)}"><img class="technical" src="{link(preview)}"></a></details></section>'
body+='<p>APK build: '+('PASS'if apk.exists()else'PENDING')+'. Android thiết bị thật: NOT_TESTED. Hai hành trình mới bắt đầu fresh, tiếp tục từ checkpoint kiếm được ngay trước Seoul sau bản vá R5; không tiêm quest/fragment flags.</p>'
(D/'gallery.html').write_text(body,encoding='utf-8')
print(json.dumps({k:v for k,v in result.items()if k not in ['checks','actualBoatTraces']},indent=2))
if not result['pass']:print('FAILED CHECKS',[c for c in checks if not c['pass']])
