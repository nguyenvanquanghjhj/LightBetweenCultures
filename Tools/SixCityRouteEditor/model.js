'use strict';
// Pure design model. No Unity writer, filesystem permission or runtime connection.
window.RouteModel = (() => {
  const S=window.LBC_ROUTE_SOURCE, clone=v=>JSON.parse(JSON.stringify(v));
  const stable=v=>JSON.stringify(v,(_,x)=>x&&typeof x==='object'&&!Array.isArray(x)?Object.fromEntries(Object.keys(x).sort().map(k=>[k,x[k]])):x);
  const city=id=>S.cities.find(c=>c.id===id), node=(c,id)=>c.nodes.find(n=>n.id===id);
  const point=p=>p&&typeof p.x==='number'&&typeof p.y==='number'&&Number.isFinite(p.x)&&Number.isFinite(p.y)&&p.x>=0&&p.x<=1000&&p.y>=0&&p.y<=1000;
  const states=['NOT_STARTED','DRAFT','READY_FOR_REVIEW'];
  function preservedCity(c){return {spawnNodeId:c.spawnNodeId,cityConstraints:clone(c.cityConstraints),coveredWalkways:clone(c.coveredWalkways),sourceImage:c.sourceImage,sourceImageSHA256:c.sourceImageSHA256,spriteGUID:c.spriteGUID,spriteLocalID:c.spriteLocalID};}
  function emptyCity(c){return {cityId:c.id,status:'NOT_STARTED',notes:'',preservedCityData:preservedCity(c),nodes:clone(c.nodes),edges:[],draft:null};}
  function create(){return {format:'LBC_ROUTE_DESIGN',schemaVersion:1,sourceFingerprint:S.fingerprint,coordinateSystem:clone(S.coordinateSystem),intent:'PROPOSAL_ONLY_NOT_APPLIED_TO_UNITY',scope:'all',cities:S.cities.map(emptyCity)};}
  const points=(c,e)=>[node(c,e.from).arrival,...e.waypoints,...(e.to?[node(c,e.to).arrival]:[])];
  const distance=(c,a,b)=>Math.hypot(a.x-b.x,(a.y-b.y)*c.imageHeight/c.imageWidth);
  function length(c,e){const ps=points(c,e);return ps.slice(1).reduce((s,p,i)=>s+distance(c,ps[i],p),0);}
  function freshEdge(c,d,from,to=null){
    let i=1;while(d.edges.some(e=>e.id===`${c.prefix}-N${String(i).padStart(3,'0')}`)||d.draft?.id===`${c.prefix}-N${String(i).padStart(3,'0')}`)i++;
    return {id:`${c.prefix}-N${String(i).padStart(3,'0')}`,from,to,waypoints:[],bidirectional:true,travel:'walk',sourceEdgeId:null,sourceRouteKey:null,constraints:{requiredFlags:[]},reviewStatus:'NEEDS_GAMEPLAY_REVIEW'};
  }
  function copyReference(c,e){return {id:e.id,from:e.from,to:e.to,waypoints:clone(e.waypoints),bidirectional:e.bidirectional,travel:e.travel,sourceEdgeId:e.id,sourceRouteKey:e.sourceRouteKey,constraints:clone(e.constraints),reviewStatus:'NEEDS_GAMEPLAY_REVIEW'};}
  function validateEdge(c,e,draft=false){
    if(!e||typeof e!=='object'||!/^[-A-Za-z0-9_]{1,80}$/.test(e.id))throw Error('Mã tuyến không hợp lệ.');
    if(!node(c,e.from)||(!node(c,e.to)&&!(draft&&e.to===null)))throw Error(`${e.id}: thiếu/sai node đầu hoặc cuối.`);
    if(e.from===e.to)throw Error(`${e.id}: node đầu và cuối trùng nhau.`);
    if(!Array.isArray(e.waypoints)||e.waypoints.length>3000||!e.waypoints.every(point))throw Error(`${e.id}: waypoint không hợp lệ (cần X/Y trong 0–1000).`);
    if(typeof e.bidirectional!=='boolean'||!['walk','ferry'].includes(e.travel))throw Error(`${e.id}: chiều hoặc loại di chuyển không hợp lệ.`);
    if(e.sourceEdgeId!==null){
      const old=c.referenceEdges.find(x=>x.id===e.sourceEdgeId);
      if(!old||e.id!==old.id||e.from!==old.from||e.to!==old.to||e.sourceRouteKey!==old.sourceRouteKey||stable(e.constraints)!==stable(old.constraints)||e.travel!==old.travel)throw Error(`${e.id}: thay đổi tham chiếu/điều kiện nguồn không được phép.`);
    } else if(e.sourceRouteKey!==null||stable(e.constraints)!==stable({requiredFlags:[]}))throw Error(`${e.id}: tuyến mới không được giả làm dữ liệu đã có trong Unity.`);
  }
  function validateImport(raw){
    if(!raw||raw.format!=='LBC_ROUTE_DESIGN'||raw.schemaVersion!==1)throw Error('Sai định dạng hoặc schemaVersion (cần LBC_ROUTE_DESIGN v1).');
    if(raw.sourceFingerprint!==S.fingerprint)throw Error('File dùng source snapshot khác. Không tự ghép với node/route mới; cần đối chiếu lại.');
    if(stable(raw.coordinateSystem)!==stable(S.coordinateSystem))throw Error('Hệ tọa độ khác snapshot.');
    if(!['all','city'].includes(raw.scope)||!Array.isArray(raw.cities)||raw.cities.length!==(raw.scope==='all'?S.cities.length:1))throw Error('Phạm vi thành phố không hợp lệ.');
    const ids=new Set();
    for(const d of raw.cities){
      const c=city(d.cityId);if(!c||ids.has(d.cityId))throw Error('City ID lạ hoặc trùng.');ids.add(d.cityId);
      if(stable(d.nodes)!==stable(c.nodes))throw Error(`${c.name}: node canonical/arrival/tham chiếu đã thay đổi. Từ chối nhập.`);
      if(stable(d.preservedCityData)!==stable(preservedCity(c)))throw Error(`${c.name}: điều kiện thành phố/occlusion/tham chiếu map đã thay đổi. Từ chối nhập.`);
      if(!states.includes(d.status)||typeof d.notes!=='string'||d.notes.length>10000||!Array.isArray(d.edges)||d.edges.length>1000)throw Error(`${c.name}: trạng thái thiết kế không hợp lệ.`);
      const edgeIds=new Set();for(const e of d.edges){validateEdge(c,e);if(edgeIds.has(e.id))throw Error('Edge ID bị trùng: '+e.id);edgeIds.add(e.id);}
      if(d.draft!==null)validateEdge(c,d.draft,true);
    }
    return clone(raw);
  }
  function merge(current,raw){const valid=validateImport(raw);return valid.scope==='all'?{...valid,scope:'all'}:{...clone(current),cities:current.cities.map(d=>d.cityId===valid.cities[0].cityId?valid.cities[0]:clone(d))};}
  function exportFile(project,id=null){const p=clone(project);p.scope=id?'city':'all';if(id)p.cities=p.cities.filter(c=>c.cityId===id);p.exportedUTC=new Date().toISOString();validateImport(p);return p;}
  function adjacency(c,edges,reverse=false){
    const adj=new Map(c.nodes.map(n=>[n.id,[]]));
    for(const e of edges){if(!adj.has(e.from)||!adj.has(e.to))continue;const w=length(c,e);const add=(a,b)=>adj.get(a).push({node:b,id:e.id,w});
      add(reverse?e.to:e.from,reverse?e.from:e.to);if(e.bidirectional)add(reverse?e.from:e.to,reverse?e.to:e.from);}
    return adj;
  }
  function reachable(adj,start){const seen=new Set([start]),queue=[start];while(queue.length){const p=queue.shift();for(const e of adj.get(p)||[])if(!seen.has(e.node)){seen.add(e.node);queue.push(e.node);}}return seen;}
  function shortest(c,edges,start,end){
    if(!node(c,start)||!node(c,end))return null;
    const adj=adjacency(c,edges),dist=new Map([[start,0]]),prev=new Map(),visited=new Set();
    while(true){let here=null,best=Infinity;for(const [id,d]of dist)if(!visited.has(id)&&d<best){best=d;here=id;}if(here===null)break;if(here===end)break;visited.add(here);
      for(const e of adj.get(here))if(best+e.w<(dist.get(e.node)??Infinity)){dist.set(e.node,best+e.w);prev.set(e.node,{from:here,edge:e.id});}}
    if(!dist.has(end))return null;let p=end,ids=[],nodes=[end];while(p!==start){const e=prev.get(p);if(!e)return null;ids.unshift(e.edge);p=e.from;nodes.unshift(p);}
    const direct=distance(c,node(c,start).arrival,node(c,end).arrival);
    return {edges:ids,nodes,length:dist.get(end),direct,ratio:direct>.000001?dist.get(end)/direct:0};
  }
  function inspect(c,d){
    const issues=[],add=(kind,code,text)=>issues.push({kind,code,text}),seen=new Set(),pairs=new Set(),valid=[];
    for(const e of d.edges){try{validateEdge(c,e);}catch(err){add('ERROR','DATA',err.message);continue;}
      valid.push(e);if(seen.has(e.id))add('ERROR','DUPLICATE_ID',e.id+' trùng mã.');seen.add(e.id);
      const arcs=e.bidirectional?[[e.from,e.to],[e.to,e.from]]:[[e.from,e.to]];
      for(const [a,b]of arcs){const k=a+'>'+b;if(pairs.has(k))add('ERROR','DUPLICATE_EDGE',`${e.id}: trùng kết nối ${a} → ${b}.`);pairs.add(k);}
      const direct=distance(c,node(c,e.from).arrival,node(c,e.to).arrival),total=length(c,e);
      if(direct>0&&total/direct>3)add('SUGGESTION','LONG_EDGE',`${e.id}: dài ${total.toFixed(1)}; gấp ${(total/direct).toFixed(1)} lần đường thẳng. Cần xem địa hình, không tự nối tắt.`);
      if(!e.sourceEdgeId&&(node(c,e.from).constraints.revealStage!==node(c,e.to).constraints.revealStage||node(c,e.from).constraints.isKeyGate||node(c,e.to).constraints.isKeyGate))add('RISK','GATE_BYPASS',`${e.id}: nối qua stage/gate; tiến trình NOT_VERIFIED.`);
      if(!e.bidirectional)add('RISK','ONE_WAY',`${e.id}: một chiều; cần kiểm tra đường quay lại.`);
      if(e.travel==='ferry')add('RISK','FERRY',`${e.id}: tuyến đò, không phải đường đi bộ. Cơ chế/điều kiện đò NOT_VERIFIED.`);
    }
    const adj=adjacency(c,valid),seenNodes=reachable(adj,c.spawnNodeId),back=reachable(adjacency(c,valid,true),c.spawnNodeId);
    for(const n of c.nodes){const connected=d.edges.some(e=>e.from===n.id||e.to===n.id);
      if(!connected)add('ERROR','ISOLATED',`${n.id}: chưa có kết nối trong thiết kế mới.`);
      if(!seenNodes.has(n.id)&&n.important)add('RISK','UNREACHABLE',`${n.id}: node có gameplay chưa tới được từ ${c.spawnNodeId} (chưa xét khóa).`);
      if(seenNodes.has(n.id)&&!back.has(n.id))add('RISK','NO_RETURN',`${n.id}: chưa có đường quay lại ${c.spawnNodeId}.`);}
    add('RISK','PROGRESSION_NOT_VERIFIED','Quest/gate/discovery, hai tuyến cốt truyện và địa hình: NOT_VERIFIED. Đây là công cụ thiết kế, không mô phỏng gameplay và không áp dụng vào Unity.');
    return issues;
  }
  return {clone,stable,city,node,point,states,create,points,distance,length,freshEdge,copyReference,validateEdge,validateImport,merge,exportFile,shortest,inspect};
})();
