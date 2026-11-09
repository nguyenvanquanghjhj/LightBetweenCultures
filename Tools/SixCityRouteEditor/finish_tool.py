"""Read-only source comparison and handoff metadata. No Unity importer."""
from pathlib import Path
import hashlib,json,html,datetime
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[1];OUT=ROOT/'Validation/SixCityRouteEditor-20261019';DOC=ROOT/'Docs/QA/SixCityRouteEditor-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,v):p.parent.mkdir(exist_ok=True,parents=True);p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def main():
    DOC.mkdir(exist_ok=True,parents=True);s=read(HERE/'data/source.json');m=read(HERE/'data/export-manifest.json');qa=read(OUT/'run07/results.json');persistence=read(OUT/'persistence/results.json')
    assert qa['status']=='PASS' and persistence['status']=='PASS'
    content=read(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json');legacy=read(ROOT/'Assets/_Game/Resources/HoiAnData.json');ids=read(ROOT/'Validation/RouteMarkupExport-20261018/edge-id-registry.json')
    for e in m['protected']:assert sha(ROOT/e['path'])==e['sha256'],e['path']
    for e in m['sourceFiles']:assert sha(ROOT/e['path'])==e['sha256'],e['path']
    assert s['fingerprint']==hashlib.sha256(json.dumps(m['sourceFiles'],sort_keys=True).encode()).hexdigest()
    for c in s['cities']:
        raw=next(x for x in content['cities'] if x['id']==c['id'])
        records=list(legacy['nodes'])+[n for n in raw['nodes'] if n['id'] not in {x['id'] for x in legacy['nodes']}] if c['id']=='hoi_an' else raw['nodes']
        assert [n['id'] for n in c['nodes']]==[n['id'] for n in records]
        for node,n in zip(c['nodes'],records):
            record=next(x for x in raw['nodes'] if x['id']==n['id']);arrival=record['arrivalPosition'] if record.get('hasArrivalPosition') else n['position']
            assert node['position']=={'x':n['position']['x']*1000,'y':(1-n['position']['y'])*1000}
            assert node['arrival']=={'x':arrival['x']*1000,'y':(1-arrival['y'])*1000}
            assert node['kind']==n.get('kind',record.get('kind','normal'))
        for edge,e in zip(c['referenceEdges'],raw['edges']):
            assert edge['sourceRecord']==e and edge['id']==ids[c['id']+':'+e['id']]
            assert edge['waypoints']==[{'x':p['x']*1000,'y':(1-p['y'])*1000} for p in e.get('waypoints',[])]
        assert sha(HERE/c['image'])==sha(ROOT/c['sourceImage'])==c['sourceImageSHA256']
    schema={'$schema':'https://json-schema.org/draft/2020-12/schema','$id':'urn:lbc:route-design:v1','title':'LBC route design proposal v1','type':'object','required':['format','schemaVersion','sourceFingerprint','coordinateSystem','scope','cities'],'properties':{'format':{'const':'LBC_ROUTE_DESIGN'},'schemaVersion':{'const':1},'sourceFingerprint':{'const':s['fingerprint']},'coordinateSystem':{'const':s['coordinateSystem']},'scope':{'enum':['all','city']},'cities':{'type':'array','minItems':1,'maxItems':6,'items':{'$ref':'#/$defs/city'}}},'$defs':{'point':{'type':'object','required':['x','y'],'properties':{k:{'type':'number','minimum':0,'maximum':1000} for k in ['x','y']}},'edge':{'type':'object','required':['id','from','to','waypoints','bidirectional','travel','sourceEdgeId','sourceRouteKey','constraints'],'properties':{'id':{'type':'string','pattern':'^[-A-Za-z0-9_]{1,80}$'},'from':{'type':'string'},'to':{'type':['string','null']},'waypoints':{'type':'array','maxItems':3000,'items':{'$ref':'#/$defs/point'}},'bidirectional':{'type':'boolean'},'travel':{'enum':['walk','ferry']},'sourceEdgeId':{'type':['string','null']},'sourceRouteKey':{'type':['string','null']},'constraints':{'type':'object'}}},'city':{'type':'object','required':['cityId','status','notes','preservedCityData','nodes','edges','draft'],'properties':{'cityId':{'enum':[c['id'] for c in s['cities']]},'status':{'enum':['NOT_STARTED','DRAFT','READY_FOR_REVIEW']},'notes':{'type':'string','maxLength':10000},'preservedCityData':{'type':'object'},'nodes':{'type':'array'},'edges':{'type':'array','maxItems':1000,'items':{'$ref':'#/$defs/edge'}},'draft':{'anyOf':[{'type':'null'},{'$ref':'#/$defs/edge'}]}}}},'$comment':'model.js additionally enforces exact canonical node/arrival/city metadata and source-edge constraints; complete edges require a known non-null end; draft may be unfinished. This schema is shipped locally and never fetched by the application.'}
    dump(HERE/'schema-v1.json',schema)
    summaries=[{'city':c['id'],'name':c['name'],'nodes':len(c['nodes']),'edges':len(c['referenceEdges']),'waypoints':sum(len(e['waypoints']) for e in c['referenceEdges']),'PNG':[c['imageWidth'],c['imageHeight']]} for c in s['cities']]
    summary={'status':'TOOL_QA_PASS','createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'cities':summaries,'sourceFingerprint':s['fingerprint'],'browser':'Microsoft Edge real headless file://; no HTTP(S)','functionalAssertions':len(qa['tests'])-len(m['protected'])-6,'protectedFiles':len(m['protected']),'mapHashChecks':6,'persistenceChecks':len(persistence['checks']),'productionChanged':[],'runtimeImporter':'NOT_IMPLEMENTED_BY_DESIGN','gameplayProgression':'NOT_VERIFIED','terrain':'NOT_VERIFIED','AndroidBuild':'NOT_RUN_IN_THIS_TASK','sourcePath':'Tools/SixCityRouteEditor/index.html'}
    dump(DOC/'verification.json',summary)
    sourceReport='''# Six-City Route Editor — bàn giao

**TOOL_QA_PASS. Chưa áp dụng bất kỳ tuyến mới nào vào Unity.**

Mở `Tools/SixCityRouteEditor/index.html` bằng Edge/Chrome. Hướng dẫn dành cho chủ dự án nằm tại `HUONG_DAN.html` và `README.vi.md` cùng thư mục. Không cần server, mạng, tài khoản, Node.js hay Unity.

## Kiểm kê source thật

| Thành phố | Node | Cạnh hiện tại | Waypoint trung gian |
|---|---:|---:|---:|
'''
    sourceReport+='\n'.join(f"|{c['name']}|{c['nodes']}|{c['edges']}|{c['waypoints']}|" for c in summaries)
    sourceReport+='''

Tổng133node/155cạnh,29quest,22tương tác,12leisure,6di vật/24mảnh,18check-in. Map đúng sprite runtime: HộiAn dùng DesignKitArt.hoiAnMap; năm thành phố còn lại dùng AdventureUIArt.cities[].sprite. PNG nguyên byte1448×1086, không thay bằng ảnhAI hay phóng lên4K giả. Hệtọađộxuất0–1000 gốc trên trái; đổi về Unity x=X/1000,y=1-Y/1000.

GameData.Load giữ vị trí/index/tên/kind HộiAn từ HoiAnData rồi lấy topology/waypoint/arrival/gate từ GameplayContent. Hai phân loại H5bot/boss và H7quest/beacon được xuất riêng runtime/catalog, không sửa game để ép khớp. CityRouteGeometry dựng arrival→waypoint→arrival; renderer chấm lấy cùng geometry, vùng occlusion chỉ ẩn chấm, không thay đường đi. Tất cả155cạnh và waypoint đã được so lại với source, giữ SourceRouteKey/JSONpointer/EdgeID cũ. Portal là event chuyển thành phố, không phải walkingedge. Tuyếnferry giữ kiểu riêng.

Runtime hiện dùng BFS theo số cạnh, không trọng số chiều dài; graph/geometry cũng có thể khiến đi vòng. Công cụ dùng Dijkstra theo chiều dài polyline đúng tỷ lệ ảnh để chủ dự án xem lựa chọn, không khẳng định BFS là nguyên nhân duy nhất của mọi detour và không sửa thuật toán runtime.

## Chức năng đã chạy thật trên trình duyệt

- Đủ6map/133node/155polyline tham khảo, nhãn/tooltip/lọc node, xem refs/khóa và điểm đứng khác icon.
- Wheel zoom tại con trỏ; pan; Fit/Reset; tọa độ map↔screen đúng sau biến đổi. Canonicalnode và arrival khóa.
- Bản thiết kế mới để trống, không sao chép tự động. Tạo cạnh/chọn đầu-cuối/click điểm uốn/kéo/chèn bằng nhấp đúp/xóa waypoint/xóa cạnh/chọn lại đường/Undo/Redo.
- Màu riêng cho source/thiết kế/đang chỉnh/tuyến thử. Có thể lấy riêng một tuyến nguồn bằng hành động chủ động; không có nút nhận cả graph.
- JSONv1 cả6hoặc1thành phố, nguồn fingerprint, node canonical, điều kiện city/gate/occlusion và refs, EdgeID, from/to, waypoints, direction, travel, trạng thái và phần đang vẽ dở. Nhập1city giữ5citykhác; file lỗi không ghi đè bản đang mở.
- Tự lưu localStorage; đóng hẳn/reopen Edge giữ nháp. Corruptstorage không bị ghi đè bằng bản trắng; nhập file hợp lệ khôi phục. Hướng dẫn yêu cầu tảiJSON định kỳ vì browserstorage có thể bị xóa.
- Dijkstra có testcase chọn2cạnh ngắn thay cho1cạnh dài. Cảnh báo node cô lập/quan trọng chưa tới/cạnh trùng/thiếuđầu-cuối/toạđộlỗi/mộtchiềukhôngquaylại/đi vòng/stagegate. Không tự sửa cảnh báo.

## Bằng chứng và giới hạn

Edge thật chạy bằng file://, chặnHTTP(S), profileQA độc lập. Kết quả cuối `Validation/SixCityRouteEditor-20261019/run07/results.json`:524checks gồm51kiểm tra chức năng,467hashproduction và6hashmap. `persistence/results.json`:8kiểm tra bổ sung, có đóng/mở browser thực. BảyảnhUI trongrun07; gallery trongDocs/QA/SixCityRouteEditor-20261019. Ảnh ví dụ có đườngQA không phù hợp địa hình chỉ để thử thao tác; **không phải tuyến đề xuất và không được đưa vào game**. Công cụ mở mới vẫn trống.

Trong phát triển đã sửa lỗi nhấp đúp do pointercapture/SVGredraw không phát nativeclick, lỗi encodingUTF8 ở script test. Có một lượt Edge đóng bất ngờ; lượt sau và bản cuối PASS. Các capture trước đó được giữ, không xóa dữ liệu lịch sử.

Quest/gate/discovery/save, hai tuyến story và địa hình đều **NOT_VERIFIED** trong công cụ. Giao nhau hình học không tự là junction: waypoint thuộc riêng một cạnh, không có sharednavigationnode mới. Không tự phát hiện vật cản từ pixel, không tự thêm cầu/đò. One-way là dữ liệu thiết kế cho bước nhập sau; runtime hiện vẫn bidirectional. Undo tối đa100thao tác trong phiên; đóng/reopen giữ hình/draft, không giữ Undohistory. Chỉ kiểm chứng EdgeWindows; trình duyệt khác chưa được test.

Không có importerUnity theo đúng phạm vi. READY_FOR_REVIEW chỉ là dấu chủ dự án đặt, không phải gameplayPASS. Thành phố NOT_STARTED/DRAFT hoặc graph chưa hoàn chỉnh không được hiểu là lệnh xóa route. Chỉ nhiệm vụ sau mới đối chiếu sourcefingerprint, duyệtgraph và nhập/QA/buildAPK.

## Bảo toàn

467fileproduction/script/asset/settings đã được soSHA256 trước/sau, không thay đổi; sáuPNG nguồn giữ nguyên. Không đổi node,graph,waypoint,arrival,collider,UIAndroid,quest,NPC,battle,relic,ending,audio,save. Không chạy Unity hay buildAPK. Không xóa cache,asset,backup,save hoặc QA cũ. Chỉ thêm Tools, dữ liệu xuất, hướng dẫn và bằng chứng QA. Checkpoint tăng dần của riêng bộ công cụ được tạo trên baseAndroidmới nhất, không fullbackup/cache/browserprofile.

**IMPLEMENT EDITOR → VERIFIED → REPORT → STOP. Chờ chủ dự án vẽ và gửi JSON.**
'''
    (ROOT/'Docs/QA/SIX_CITY_ROUTE_EDITOR_20261019.md').write_text(sourceReport,encoding='utf-8')
    links=[]
    for filename in qa['screenshots']:
        path=OUT/'run07'/filename;relative='../../../Validation/SixCityRouteEditor-20261019/run07/'+filename
        note='Ảnh UI thật từ Edge offline. Không phải Windows Player/Android runtime.' if not filename.startswith('QA_ONLY') else 'VÍ DỤ QA: đường vẽ tự động để test thao tác, không phải đề xuất địa hình; chưa áp dụng vào game.'
        links.append(f'<figure><a href="{relative}"><img loading="lazy" src="{relative}" width="100%"></a><figcaption>{html.escape(filename)} — {note}</figcaption></figure>')
    (DOC/'gallery.html').write_text('<!doctype html><html lang="vi"><meta charset="utf-8"><title>LBC Route Editor QA</title><style>body{font:16px Segoe UI;background:#102830;color:#eef0e5;margin:25px}figure{margin:20px 0}a{color:#edcf8f}figcaption{padding:8px}</style><h1>Route Editor — ảnh trình duyệt thật</h1><p>TOOL_QA_PASS; không thay route game. Mở ảnh để xem đủ1600×1000.</p>'+''.join(links)+'</html>',encoding='utf-8')
    print(json.dumps(summary,ensure_ascii=True,indent=2))
if __name__=='__main__':main()
