"""Real Edge/file:// UI tests; isolated browser storage and QA-only example drawings."""
from pathlib import Path
from playwright.sync_api import sync_playwright
import csv,datetime,hashlib,json,math,traceback

HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[1];OUT=ROOT/'Validation/SixCityRouteEditor-20261019/run07'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    assert not (OUT/'results.json').exists(),'Retain existing evidence; use a new run path explicitly'
    OUT.mkdir(exist_ok=True,parents=True);results=[];shots=[];errors=[]
    def check(condition,label,detail=''):
        results.append({'test':label,'status':'PASS' if condition else 'FAIL','detail':detail})
        if not condition:raise AssertionError(label+' '+str(detail))
    with sync_playwright() as p:
        browser=p.chromium.launch(executable_path='C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',headless=True,args=['--disable-background-networking'])
        context=browser.new_context(viewport={'width':1600,'height':1000},accept_downloads=True)
        page=context.new_page();page.on('pageerror',lambda e:errors.append(str(e)))
        page.route('http://**/*',lambda route:route.abort());page.route('https://**/*',lambda route:route.abort())
        page.on('dialog',lambda d:d.accept())
        page.goto((HERE/'index.html').as_uri());page.wait_for_function('Boolean(window.RouteEditor)');page.wait_for_timeout(200)
        data=json.loads((HERE/'data/source.json').read_text(encoding='utf-8-sig'))
        check(page.url.startswith('file:///'),'Opens offline directly as file://; HTTP(S) blocked')
        check(page.evaluate('RouteEditor.getProject().cities.every(c=>c.edges.length===0&&c.draft===null)'),'New designs start empty, not a copy of old graph')
        for c in data['cities']:
            page.select_option('#city',c['id']);page.wait_for_function('document.querySelector("#background").href.baseVal.endsWith("'+c['id']+'.png")')
            check(page.locator('g.node').count()==len(c['nodes']),'All canonical nodes displayed '+c['id'])
            check(page.locator('g[data-kind=ref]').count()==len(c['referenceEdges']),'All current edge polylines displayed '+c['id'])
            mapping=page.evaluate('''()=>{let c=RouteEditor.getCity();return c.nodes.map(n=>{let p=RouteEditor.mapToClient(n.position);let back=RouteEditor.clientToMap(p.x,p.y);return Math.max(Math.abs(n.position.x-back.x),Math.abs(n.position.y-back.y));})}''')
            check(max(mapping)<1e-8,'Map-to-screen coordinate roundtrip '+c['id'])
            page.screenshot(path=str(OUT/(c['id']+'-reference.png')));shots.append(c['id']+'-reference.png')
        page.select_option('#city','hoi_an');page.wait_for_timeout(100)
        canonicalBefore=page.evaluate('JSON.stringify(RouteEditor.getCity().nodes)')
        center=page.locator('#map').bounding_box();cx=center['x']+center['width']/2;cy=center['y']+center['height']/2
        before=page.evaluate('([x,y])=>RouteEditor.clientToMap(x,y)',[cx,cy]);page.mouse.move(cx,cy);page.mouse.wheel(0,-420);page.wait_for_timeout(100)
        after=page.evaluate('([x,y])=>RouteEditor.clientToMap(x,y)',[cx,cy]);check(max(abs(before[k]-after[k]) for k in ['x','y'])<1e-7,'Wheel zoom preserves coordinate beneath cursor')
        page.mouse.move(cx,cy);page.mouse.down(button='middle');page.mouse.move(cx+61,cy+27,steps=5);page.mouse.up(button='middle')
        afterPan=page.evaluate('([x,y])=>RouteEditor.mapToClient({x,y})',[before['x'],before['y']]);check(abs(afterPan['x']-(cx+61))<1e-6 and abs(afterPan['y']-(cy+27))<1e-6,'Middle-drag pan preserves map position')
        page.click('#fit');page.select_option('#from','H0');page.select_option('#to','H1');page.click('#begin')
        def click_map(x,y,count=1):
            s=page.evaluate('p=>RouteEditor.mapToClient(p)',{'x':x,'y':y})
            if count==2:page.mouse.dblclick(s['x'],s['y'])
            else:page.mouse.click(s['x'],s['y'])
        click_map(400,400);click_map(450,450)
        check(len(page.evaluate('RouteEditor.getDraft().waypoints'))==2,'Click adds ordered waypoints')
        page.click('#saveEdge');check(len(page.evaluate('RouteEditor.getProject().cities[0].edges'))==1,'Save actual node-linked design edge')
        page.click('#editEdge');wp=page.evaluate('RouteEditor.getDraft().waypoints[0]');s=page.evaluate('p=>RouteEditor.mapToClient(p)',wp)
        page.mouse.move(s['x'],s['y']);page.mouse.down();page.mouse.move(s['x']+30,s['y']+17,steps=5);page.mouse.up()
        moved=page.evaluate('RouteEditor.getDraft().waypoints[0]');check(moved!=wp,'Drag edits waypoint, canonical node remains locked')
        page.click('#undo');check(page.evaluate('RouteEditor.getDraft().waypoints[0]')==wp,'Undo waypoint drag')
        page.click('#redo');check(page.evaluate('RouteEditor.getDraft().waypoints[0]')==moved,'Redo waypoint drag')
        # Insert into a real segment, not by direct state mutation.
        ps=page.evaluate('RouteModel.points(RouteEditor.getCity(),RouteEditor.getDraft())');mid={'x':(ps[1]['x']+ps[2]['x'])/2,'y':(ps[1]['y']+ps[2]['y'])/2}
        click_map(mid['x'],mid['y'],2);check(len(page.evaluate('RouteEditor.getDraft().waypoints'))==3,'Double-click inserts waypoint into selected segment')
        page.click('#deletePoint');check(len(page.evaluate('RouteEditor.getDraft().waypoints'))==2,'Delete selected waypoint')
        page.click('#undo');check(len(page.evaluate('RouteEditor.getDraft().waypoints'))==3,'Undo waypoint deletion')
        page.click('#saveEdge');page.click('#testPath');check('H0 → H1' in page.locator('#pathInfo').inner_text(),'Shortest-path preview draws saved design')
        page.click('#validate');check('NOT_VERIFIED' in page.locator('#warnings').inner_text(),'Complex gate/progression explicitly NOT_VERIFIED')
        page.screenshot(path=str(OUT/'QA_ONLY_example-design.png'));shots.append('QA_ONLY_example-design.png')
        check(page.evaluate('JSON.stringify(RouteEditor.getCity().nodes)')==canonicalBefore,'Drawing/zoom/pan never changes canonical nodes/arrivals')
        projectBefore=page.evaluate('RouteEditor.getProject()');page.click('#saveDraft');page.reload();page.wait_for_function('Boolean(window.RouteEditor)');page.wait_for_timeout(150)
        restored=page.evaluate('RouteEditor.getProject()');check(restored['cities']==projectBefore['cities'],'Autosave survives browser reload')
        with page.expect_download() as event:page.click('#exportAll')
        exported=OUT/'QA_TEST_ONLY_roundtrip.json';event.value.save_as(str(exported));saved=json.loads(exported.read_text(encoding='utf-8'))
        check(saved['format']=='LBC_ROUTE_DESIGN' and saved['schemaVersion']==1 and saved['scope']=='all','Download complete versioned JSON')
        page.select_option('#edgeSelect',saved['cities'][0]['edges'][0]['id']);page.click('#deleteEdge');check(len(page.evaluate('RouteEditor.getProject().cities[0].edges'))==0,'Delete edge affects design only')
        page.click('#undo');check(len(page.evaluate('RouteEditor.getProject().cities[0].edges'))==1,'Undo edge deletion')
        page.click('#redo');check(len(page.evaluate('RouteEditor.getProject().cities[0].edges'))==0,'Redo edge deletion')
        page.set_input_files('#importFile',str(exported));page.wait_for_function('RouteEditor.getProject().cities[0].edges.length===1')
        check(page.evaluate('RouteEditor.getProject().cities')==saved['cities'],'JSON import restores exact node-linked geometry/constraints for all cities')
        # A partial import must not erase the other five city drafts.
        page.select_option('#city','hue');page.click('#begin');page.click('#saveEdge')
        hueBefore=page.evaluate('RouteEditor.getProject().cities.find(c=>c.cityId==="hue")')
        page.select_option('#city','hoi_an')
        with page.expect_download() as event:page.click('#exportCity')
        cityfile=OUT/'QA_TEST_ONLY_hoian.json';event.value.save_as(str(cityfile));check(len(json.loads(cityfile.read_text(encoding='utf-8'))['cities'])==1,'Download single-city JSON')
        page.set_input_files('#importFile',str(cityfile));page.wait_for_timeout(150)
        check(page.evaluate('RouteEditor.getProject().cities.find(c=>c.cityId==="hue")')==hueBefore,'Single-city import preserves five other city designs')
        for name,mutate in [('wrong-fingerprint',lambda d:d.update(sourceFingerprint='wrong')),('moved-node',lambda d:d['cities'][0]['nodes'][0]['position'].update(x=123)),('bad-waypoint',lambda d:d['cities'][0]['edges'][0]['waypoints'][0].update(x=1001)),('wrong-schema',lambda d:d.update(schemaVersion=99))]:
            bad=json.loads(exported.read_text(encoding='utf-8'));mutate(bad);f=OUT/(name+'.json');f.write_text(json.dumps(bad),encoding='utf-8');prior=page.evaluate('RouteEditor.getProject().cities');page.set_input_files('#importFile',str(f));page.wait_for_timeout(100)
            check('Không nhập:' in page.locator('#notice').inner_text() and page.evaluate('RouteEditor.getProject().cities')==prior,'Reject '+name+' without overwriting open work')
        pure=page.evaluate('''()=>{
          const c={id:'test',imageWidth:1000,imageHeight:1000,spawnNodeId:'A',nodes:['A','B','C'].map((id,i)=>({id,arrival:{x:i*50,y:0},important:true,constraints:{}}))};
          const e=(id,from,to,w=[])=>({id,from,to,waypoints:w,bidirectional:true,travel:'walk',sourceEdgeId:null,sourceRouteKey:null,constraints:{requiredFlags:[]}});
          const edges=[e('long','A','C',[{x:0,y:900}]),e('ab','A','B'),e('bc','B','C')];
          const chosen=RouteModel.shortest(c,edges,'A','C');
          const issueCodes=RouteModel.inspect(c,{edges:[...edges,e('duplicate','B','A'),{...e('oneway','B','C'),bidirectional:false}]}).map(x=>x.code);
          const trap=RouteModel.inspect(c,{edges:[{...e('one','A','B'),bidirectional:false}]}).map(x=>x.code);
          return {chosen,issueCodes,trap};
        }''')
        check(pure['chosen']['edges']==['ab','bc'] and pure['chosen']['length']==100,'Dijkstra uses polyline length, chooses two short edges over one long edge')
        check('DUPLICATE_EDGE' in pure['issueCodes'],'Detect duplicate connectivity')
        check('ISOLATED' in pure['trap'] and 'UNREACHABLE' in pure['trap'] and 'NO_RETURN' in pure['trap'],'Detect isolated/important-unreachable/one-way-return risks')
        page.select_option('#filter','npc');check(0<page.locator('g.node').count()<22,'Filter nodes by actual categories');page.select_option('#filter','all')
        page.uncheck('#showLabels');check(page.locator('g.node text').count()==0,'Toggle node labels without removing nodes');page.check('#showLabels')
        check(not errors,'No browser JavaScript errors',errors)
        browser.close()
    manifest=json.loads((HERE/'data/export-manifest.json').read_text(encoding='utf-8'))
    for e in manifest['protected']:check(sha(ROOT/e['path'])==e['sha256'],'Protected unchanged '+e['path'])
    for c in data['cities']:check(sha(HERE/c['image'])==c['sourceImageSHA256'],'Exported background bit-exact '+c['id'])
    report={'status':'PASS','environment':'Windows Edge headless via real file://, HTTP(S) blocked; isolated localStorage; no Unity/Android build','tests':results,'screenshots':shots,'productionFilesChecked':len(manifest['protected']),'prototypeGeometry':'QA_TEST_ONLY; never imported into Unity; delivered default remains empty','limitations':['Quest/gate/discovery and terrain correctness NOT_VERIFIED. No runtime importer. Shared waypoint intersections do not create navigation nodes.']}
    (OUT/'results.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps({'result':'PASS','assertions':len(results),'screenshots':len(shots),'productionUnchanged':len(manifest['protected'])}))

if __name__=='__main__':
    try:main()
    except Exception:
        OUT.mkdir(exist_ok=True,parents=True)
        (OUT/'failure.txt').write_text(traceback.format_exc(),encoding='utf-8')
        raise
