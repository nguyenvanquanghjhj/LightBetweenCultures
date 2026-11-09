"""Read-only runtime export. Never writes Unity Assets/settings or existing designs."""
from pathlib import Path
import csv,datetime,hashlib,json,re,shutil
from PIL import Image

HERE=Path(__file__).resolve().parent; ROOT=HERE.parents[1]
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def dump(p,v):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(v,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def pos(p):return {'x':p['x']*1000,'y':(1-p['y'])*1000}

def main():
    dest=HERE/'data/source.json'
    if dest.exists():raise SystemExit('Refuse overwrite of the existing source snapshot. Export a new version explicitly after protecting designs.')
    contentPath=ROOT/'Assets/_Game/Resources/Content/GameplayContent.json'
    content=read(contentPath);legacy=read(ROOT/'Assets/_Game/Resources/HoiAnData.json')
    bindings=read(ROOT/'Docs/Art/native-art-bindings.json')['entries']
    registry=read(ROOT/'Validation/RouteMarkupExport-20261018/edge-id-registry.json')
    leisure=read(ROOT/'Assets/_Game/Resources/Content/LeisureActivities.json')['entries']
    activities=read(ROOT/'Assets/_Game/Resources/Content/StoryInteractions.json')['entries']
    relics=read(ROOT/'Assets/_Game/Resources/Content/CulturalRelics.json')
    prior=read(ROOT/'Docs/QA/AndroidApk-20261019/checkpoint-receipt.json')
    assert sha(Path(prior['checkpoint'])/'CHECKPOINT_MANIFEST.json')==prior['manifestSHA256']
    protected=read(ROOT/'Docs/QA/AndroidApk-20261019/baseline.json')['entries']
    frozen=[{'path':e['path'],'sha256':sha(ROOT/e['path']),'bytes':(ROOT/e['path']).stat().st_size} for e in protected]
    paths=['Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/HoiAnData.json','Assets/_Game/Resources/AdventureUIArt.asset','Assets/_Game/Resources/DesignKitArt.asset','Assets/_Game/Resources/Content/LeisureActivities.json','Assets/_Game/Resources/Content/StoryInteractions.json','Assets/_Game/Resources/Content/CulturalRelics.json','Docs/Art/native-art-bindings.json','Validation/RouteMarkupExport-20261018/edge-id-registry.json','Assets/_Game/Scripts/Core/GameData.cs','Assets/_Game/Scripts/Maps/CityRouteGeometry.cs','Assets/_Game/Scripts/Maps/CityMapPaths.cs','Assets/_Game/Scripts/Maps/CityMap.cs']
    sourceFiles=[{'path':p,'sha256':sha(ROOT/p)} for p in paths]
    cities=[];nodeRows=[];edgeRows=[]
    for cid,prefix in [('hoi_an','H'),('hue','U'),('da_lat','D'),('jeonju','J'),('gyeongju','G'),('seoul','S')]:
        c=next(c for c in content['cities'] if c['id']==cid)
        binding=next(b for b in bindings if b['id']==cid and b['role']==('city_main' if cid=='hoi_an' else 'city_preview'))
        asset=ROOT/('Assets/_Game/Resources/DesignKitArt.asset' if cid=='hoi_an' else 'Assets/_Game/Resources/AdventureUIArt.asset')
        text=asset.read_text(encoding='utf-8-sig')
        if cid=='hoi_an':assert re.search(r'^  hoiAnMap:.*guid: '+binding['guid'],text,re.M)
        else:
            section=re.search(r'^  cities:\n(.*?)(?=^  \w+:)',text,re.M|re.S)[1]
            assert re.search(r'^  - id: '+cid+r'\n.*?^    sprite:.*guid: '+binding['guid'],section,re.M|re.S)
        image=ROOT/binding['path'];width,height=Image.open(image).size
        meta=(ROOT/(binding['path']+'.meta')).read_text(encoding='utf-8-sig');assert 'guid: '+binding['guid'] in meta
        target=HERE/'assets'/f'{cid}.png';target.parent.mkdir(exist_ok=True);shutil.copy2(image,target);assert sha(target)==sha(image)
        sourceFiles+=[{'path':binding['path'],'sha256':sha(image)},{'path':binding['path']+'.meta','sha256':sha(ROOT/(binding['path']+'.meta'))}]
        records=list(legacy['nodes'])+[n for n in c['nodes'] if n['id'] not in {a['id'] for a in legacy['nodes']}] if cid=='hoi_an' else c['nodes']
        nodes=[]
        for index,n in enumerate(records):
            record=next(v for v in c['nodes'] if v['id']==n['id'])
            events=[e for e in content['events'] if e['cityId']==cid and e['nodeId']==n['id']]
            quests=[q for q in content['quests'] if q['cityId']==cid and n['id'] in [q['giverNode'],q['completeAtNode']]]
            puzzles=[p for p in content['puzzles'] if p['cityId']==cid and p['nodeId']==n['id']]
            ls=[e for e in leisure if e['cityId']==cid and e['nodeId']==n['id']]
            ci=[e for e in content['checkIns'] if e['cityId']==cid and e['nodeId']==n['id']]
            categories={record.get('kind') or 'normal',n.get('kind') or 'normal'}
            categories.update(e['type'] for e in events)
            if quests:categories.add('quest')
            if puzzles:categories.add('puzzle')
            if ls:categories.add('leisure')
            if ci:categories.add('checkin')
            if any(e.get('enemyId') for e in events):categories.add('bot')
            if any(e.get('npcId') for e in events):categories.add('npc')
            portals=[{k:e.get(k) for k in ['id','destinationCity','destinationNode','requiredFlags','requiredItems']} for e in events if e['type']=='portal']
            canonical=pos(n['position']);arrival=pos(record['arrivalPosition']) if record.get('hasArrivalPosition') else canonical.copy()
            refs={'eventId':record.get('eventId',''),'eventIds':[e['id'] for e in events],'npcIds':sorted({e['npcId'] for e in events if e.get('npcId')}),'enemyIds':sorted({e['enemyId'] for e in events if e.get('enemyId')}),'questIds':[q['id'] for q in quests],'puzzleIds':[p['id'] for p in puzzles],'leisureIds':[e['id'] for e in ls],'checkInIds':[e['id'] for e in ci],'backgroundId':record.get('backgroundId','')}
            constraints={k:record.get(k) for k in ['revealStage','isKeyGate','gateCondition','unlockStage','requiredFlags','previewStages','revealWhenPreviouslyVisited']}
            constraints['events']=[{k:e.get(k) for k in ['id','type','requiredFlags','requiredItems','mandatory','completionFlag','flags','questId','itemId','enemyId','puzzleId']} for e in events]
            node={'id':n['id'],'name':n['name'],'runtimeIndex':index,'position':canonical,'arrival':arrival,'hasArrivalPosition':bool(record.get('hasArrivalPosition')),'kind':n.get('kind',record.get('kind','normal')),'catalogKind':record.get('kind','normal'),'categories':sorted(categories),'constraints':constraints,'refs':refs,'portals':portals,'important':bool(quests or puzzles or portals or ls or record.get('isKeyGate'))}
            nodes.append(node);nodeRows.append({'City':cid,'NodeID':node['id'],'Name':node['name'],'X1000':canonical['x'],'Y1000':canonical['y'],'ArrivalX1000':arrival['x'],'ArrivalY1000':arrival['y'],'Types':'|'.join(node['categories']),'References':json.dumps(refs,ensure_ascii=False),'Constraints':json.dumps(constraints,ensure_ascii=False)})
        byID={n['id']:n for n in nodes};edges=[]
        for index,e in enumerate(c['edges']):
            key=cid+':'+e['id'];assert key in registry, 'Need explicit stable ID mapping for new source edge '+key
            points=[byID[e['from']]['arrival']]+[pos(p) for p in e.get('waypoints',[])]+[byID[e['to']]['arrival']]
            edge={'id':registry[key],'from':e['from'],'to':e['to'],'waypoints':points[1:-1],'bidirectional':True,'travel':e.get('travel') or 'walk','constraints':{'requiredFlags':e.get('requiredFlags',[])},'sourceRouteKey':e['id'],'sourceJsonPointer':f"/cities/{content['cities'].index(c)}/edges/{index}",'sourceBidirectional':e.get('bidirectional',True),'sourceRecord':e,'points':points}
            edges.append(edge);edgeRows.append({'City':cid,'EdgeID':edge['id'],'FromNode':edge['from'],'ToNode':edge['to'],'CurrentWaypoints':json.dumps(points),'SourceRouteKey':e['id'],'RuntimeBidirectional':True,'SourceBidirectional':edge['sourceBidirectional'],'Travel':edge['travel'],'RequiredFlags':json.dumps(edge['constraints']['requiredFlags'])})
        cities.append({'id':cid,'prefix':prefix,'name':c['name'],'image':f'assets/{cid}.png','imageWidth':width,'imageHeight':height,'sourceImage':binding['path'],'sourceImageSHA256':sha(image),'spriteGUID':binding['guid'],'spriteLocalID':binding['localId'],'spawnNodeId':c['spawnNodeId'],'nodes':nodes,'referenceEdges':edges,'cityConstraints':{k:c.get(k) for k in ['unlockFlags','completionFlag','gates','runtimeNodeIds']},'coveredWalkways':c.get('coveredWalkways',[])})
    sourceFiles.sort(key=lambda e:e['path'])
    fingerprint=hashlib.sha256(json.dumps(sourceFiles,sort_keys=True).encode()).hexdigest()
    payload={'format':'LBC_ROUTE_SOURCE','schemaVersion':1,'createdUTC':datetime.datetime.now(datetime.timezone.utc).isoformat(),'fingerprint':fingerprint,'coordinateSystem':{'range':[0,1000],'origin':'top-left','x':'right','y':'down','unityConversion':'Unity.x=X/1000; Unity.y=1-Y/1000','lengthMetric':'Image-proportional units: hypot(dx,dy*imageHeight/imageWidth), map width=1000'},'runtimeNotes':['GameData.FindPath uses unweighted BFS, not polyline distance. Runtime currently treats every catalog edge as bidirectional, regardless of source bidirectional.','CityRouteGeometry.Points = locked arrival(from) + ordered waypoints + locked arrival(to), reversed for reverse traversal.','CityMapPaths dots sample the same geometry; covered walkway dot suppression does not alter movement.','Portals are event-based city transitions, not city walking edges. No Unity import/apply is included.'],'counts':{'cities':len(cities),'nodes':sum(len(c['nodes']) for c in cities),'edges':sum(len(c['referenceEdges']) for c in cities),'quests':len(content['quests']),'storyInteractions':len(activities),'leisure':len(leisure),'relics':len(relics['relics']),'fragments':sum(len(r['fragmentIds']) for r in relics['relics'])},'cities':cities}
    dump(dest,payload)
    (HERE/'source.js').write_text('/* Generated read-only live Unity snapshot. */\nwindow.LBC_ROUTE_SOURCE='+json.dumps(payload,ensure_ascii=False).replace('</','<\\/')+';\n',encoding='utf-8')
    for name,rows in [('node_inventory.csv',nodeRows),('edge_inventory.csv',edgeRows)]:
        with (HERE/'data'/name).open('w',encoding='utf-8-sig',newline='') as f:
            w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)
    dump(HERE/'data/export-manifest.json',{'baseCheckpoint':prior,'sourceFiles':sourceFiles,'protected':frozen,'sourceFingerprint':fingerprint,'counts':payload['counts'],'productionWritten':False})
    print(json.dumps({'counts':payload['counts'],'cities':[{'id':c['id'],'nodes':len(c['nodes']),'edges':len(c['referenceEdges']),'mapSize':[c['imageWidth'],c['imageHeight']]} for c in cities]},indent=2))

if __name__=='__main__':main()
