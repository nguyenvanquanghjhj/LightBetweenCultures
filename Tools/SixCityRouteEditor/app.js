'use strict';
(() => {
  const M=window.RouteModel,S=window.LBC_ROUTE_SOURCE,$=id=>document.getElementById(id),svg=$('map'),scene=$('scene');
  const NS='http://www.w3.org/2000/svg',storageKey='LBC_SIX_CITY_ROUTE_DESIGN_V1_'+S.fingerprint;
  let project=M.create(),cityId=S.cities[0].id,workspace='design',tool='pan',selectedEdge='',selectedNode='',selectedPoint=null,selectedRef='',pathResult=null,pathPick=null;
  let history=[],future=[],pan={x:0,y:0},zoom=1,fitZoom=1,pressed=null,space=false,storageWritable=true,lastEdgeHit=null,lastMapAdd=null;
  const city=()=>M.city(cityId),design=()=>project.cities.find(d=>d.cityId===cityId),clone=M.clone;
  function notify(text,error=false){$('notice').textContent=text;$('notice').classList.toggle('error',error);}
  function snapshot(){return clone({project,cityId,selectedEdge,selectedNode,selectedPoint});}
  function push(){history.push(snapshot());if(history.length>100)history.shift();future=[];}
  function restore(v){project=v.project;cityId=v.cityId;selectedEdge=v.selectedEdge;selectedNode=v.selectedNode;selectedPoint=v.selectedPoint;selectedRef='';pathResult=null;fillCity();render();persist();}
  function change(fn){push();fn();pathResult=null;render();persist();}
  function undo(){if(!history.length)return;future.push(snapshot());restore(history.pop());notify('Đã hoàn tác.');}
  function redo(){if(!future.length)return;history.push(snapshot());restore(future.pop());notify('Đã làm lại.');}
  function persist(manual=false){
    if(!storageWritable){$('saveState').textContent='Không ghi đè bản nháp lỗi. Hãy tải JSON hiện tại; nhập bản nháp hợp lệ để tiếp tục tự lưu.';return;}
    try{localStorage.setItem(storageKey,JSON.stringify({project:M.exportFile(project),lastCity:cityId}));$('saveState').textContent='Đã lưu trong trình duyệt · '+new Date().toLocaleTimeString('vi-VN')+'. Nên tải JSON dự phòng.';if(manual)notify('Đã lưu nháp trên trình duyệt. Hãy tải JSON để có bản sao độc lập.');}
    catch(e){$('saveState').textContent='Trình duyệt không cho tự lưu. Dùng nút Tải JSON để tránh mất bản vẽ.';if(manual)notify('Không tự lưu được. Hãy tải JSON.',true);}
  }
  function options(select,values,empty=null){const old=select.value;select.replaceChildren();if(empty!==null)select.add(new Option(empty,''));for(const [id,label]of values)select.add(new Option(label,id));if([...select.options].some(o=>o.value===old))select.value=old;}
  options($('city'),S.cities.map(c=>[c.id,c.name]));
  const categories=[...new Set(S.cities.flatMap(c=>c.nodes.flatMap(n=>n.categories)))].sort();
  const typeNames={npc:'NPC',bot:'Bot / đối thủ',quest:'Nhiệm vụ',puzzle:'Puzzle',leisure:'Leisure',portal:'Cổng chuyển thành phố',checkin:'Check-in',item:'Vật phẩm',junction:'Giao lộ',landmark:'Địa danh',encounter:'Chạm trán'};
  for(const k of categories)$('filter').add(new Option(typeNames[k]||k,k));
  function fillCity(){
    $('city').value=cityId;const nodes=city().nodes.map(n=>[n.id,n.id+' · '+n.name]);options($('from'),nodes);options($('to'),nodes);
    if($('from').value===$('to').value&&nodes.length>1)$('to').selectedIndex=1;
    options($('refSelect'),city().referenceEdges.map(e=>[e.id,`${e.id}: ${e.from} ${e.bidirectional?'↔':'→'} ${e.to}${e.travel==='ferry'?' · đò':''}`]),'Tất cả tuyến cũ');
    $('refSelect').value=selectedRef;$('designStatus').value=design().status;$('notes').value=design().notes;$('warnings').replaceChildren();
  }
  function toModel(clientX,clientY){const r=svg.getBoundingClientRect();return{x:(clientX-r.left-pan.x)/zoom,y:(clientY-r.top-pan.y)/zoom/(city().imageHeight/city().imageWidth)};}
  function toClient(p){const r=svg.getBoundingClientRect();return{x:r.left+pan.x+p.x*zoom,y:r.top+pan.y+p.y*city().imageHeight/city().imageWidth*zoom};}
  const rounded=p=>({x:Math.round(p.x*1e6)/1e6,y:Math.round(p.y*1e6)/1e6});
  function fit(){const r=svg.getBoundingClientRect(),height=1000*city().imageHeight/city().imageWidth;fitZoom=Math.min(r.width/1000,r.height/height)*.94;zoom=fitZoom;pan={x:(r.width-1000*zoom)/2,y:(r.height-height*zoom)/2};render();}
  function zoomAt(factor,x,y){const p=toModel(x,y),r=svg.getBoundingClientRect();zoom=Math.max(fitZoom*.25,Math.min(fitZoom*18,zoom*factor));pan={x:x-r.left-p.x*zoom,y:y-r.top-p.y*(city().imageHeight/city().imageWidth)*zoom};render();}
  function element(tag,attrs={},parent=scene,text=null){const e=document.createElementNS(NS,tag);for(const[k,v]of Object.entries(attrs))e.setAttribute(k,String(v));if(text!==null)e.textContent=text;parent.append(e);return e;}
  const xy=p=>[p.x,p.y*city().imageHeight/city().imageWidth];
  function stroke(e,kind,color,width=1.5,opacity=1){
    const p=M.points(city(),e).map(xy).map(p=>p.join(',')).join(' '),g=element('g',{'data-edge':e.id,'data-kind':kind});
    element('polyline',{points:p,fill:'none',stroke:color,'stroke-width':width/zoom,opacity,'stroke-linejoin':'round','stroke-linecap':'round',...(kind==='ref'?{'stroke-dasharray':`${6/zoom} ${4/zoom}`}:{})},g);
    element('polyline',{points:p,fill:'none',stroke:'transparent','stroke-width':12/zoom,'pointer-events':'stroke',class:'edgeHit'},g);
    if(!e.bidirectional){const ps=M.points(city(),e),a=xy(ps[ps.length-2]),b=xy(ps[ps.length-1]),angle=Math.atan2(b[1]-a[1],b[0]-a[0]),l=10/zoom;
      element('path',{d:`M${b[0]-l*Math.cos(angle-.45)},${b[1]-l*Math.sin(angle-.45)} L${b} L${b[0]-l*Math.cos(angle+.45)},${b[1]-l*Math.sin(angle+.45)}`,stroke:color,'stroke-width':2/zoom,fill:'none'},g);}
  }
  function render(){
    const c=city(),d=design(),ratio=c.imageHeight/c.imageWidth;scene.replaceChildren();scene.setAttribute('transform',`translate(${pan.x},${pan.y}) scale(${zoom})`);
    element('image',{id:'background',href:c.image,x:0,y:0,width:1000,height:1000*ratio,preserveAspectRatio:'xMidYMid meet'});
    if($('showOld').checked)for(const e of c.referenceEdges)stroke(e,'ref',e.travel==='ferry'?'#d3b3f6':'#bcd8f5',e.id===selectedRef?3:1.25,selectedRef&&e.id!==selectedRef?.24:.82);
    if($('showNew').checked)for(const e of d.edges)stroke(e,'design',e.travel==='ferry'?'#cbb1ff':'#62efaa',e.id===selectedEdge?3:2.3,.95);
    if(pathResult){const edges=$('pathNetwork').value==='reference'?c.referenceEdges:d.edges;for(const id of pathResult.edges){const e=edges.find(e=>e.id===id);if(e)stroke(e,'path','#ff8cdd',4,.95);}}
    const ref=c.referenceEdges.find(e=>e.id===selectedRef);
    if(ref&&$('showRefPoints').checked&&$('showOld').checked)M.points(c,ref).forEach((p,i)=>{const[x,y]=xy(p);element('circle',{cx:x,cy:y,r:3.5/zoom,fill:'#e3efff',stroke:'#405a80','stroke-width':1/zoom});element('text',{x:x+5/zoom,y:y-5/zoom,fill:'#f6f5ff','font-size':11/zoom,'paint-order':'stroke',stroke:'#183345','stroke-width':2/zoom},scene,'W'+i);});
    if(d.draft){stroke(d.draft,'draft','#ffd16a',3);d.draft.waypoints.forEach((p,i)=>{const[x,y]=xy(p);element('circle',{cx:x,cy:y,r:(i===selectedPoint?8:6)/zoom,fill:i===selectedPoint?'#fff5cd':'#ffd16a',stroke:'#203746','stroke-width':2/zoom,'data-waypoint':i,class:'waypoint'});element('text',{x:x+8/zoom,y:y-7/zoom,fill:'#ffe4a4','font-size':11/zoom,'pointer-events':'none','paint-order':'stroke',stroke:'#16333d','stroke-width':2/zoom},scene,'W'+(i+1));});}
    for(const n of c.nodes){
      if($('filter').value!=='all'&&!n.categories.includes($('filter').value))continue;
      const[x,y]=xy(n.position),g=element('g',{'data-node':n.id,class:'node'}),r=(n.id===selectedNode?8.5:6)/zoom;
      if(n.hasArrivalPosition&&$('showArrivals').checked){const[a,b]=xy(n.arrival),s=4/zoom;element('line',{x1:x,y1:y,x2:a,y2:b,stroke:'#b9f1ed','stroke-width':1/zoom,'stroke-dasharray':`${3/zoom} ${3/zoom}`},g);element('path',{d:`M${a},${b-s}l${s},${s}l${-s},${s}l${-s},${-s}Z`,fill:'#d4faf6',stroke:'#203a44','stroke-width':1/zoom},g);}
      const color=n.categories.includes('bot')?'#ff9e8d':n.categories.includes('npc')?'#f5cf81':n.categories.includes('puzzle')?'#d3b4f0':'#dbf1e3';
      element('circle',{cx:x,cy:y,r,fill:color,stroke:n.id===selectedNode?'#fff':'#183640','stroke-width':2/zoom},g);
      if($('showLabels').checked)element('text',{x:x+9/zoom,y:y-8/zoom,fill:'#fff7dc','font-size':13/zoom,'stroke-width':3/zoom},g,n.id);
    }
    options($('edgeSelect'),d.edges.map(e=>[e.id,`${e.id}: ${e.from} ${e.bidirectional?'↔':'→'} ${e.to}`]),'Chọn một đường');$('edgeSelect').value=selectedEdge;
    $('zoomLabel').textContent=Math.round(zoom/fitZoom*100)+'%';$('mapInfo').textContent=`${c.nodes.length} node · ${c.referenceEdges.length} cạnh cũ · ${d.edges.length} cạnh mới`;
    $('undo').disabled=!history.length;$('redo').disabled=!future.length;
    const draft=d.draft;$('saveEdge').disabled=!draft||!draft.to;$('cancelDraft').disabled=!draft;$('deletePoint').disabled=!draft||selectedPoint===null;$('twoWay').checked=draft?draft.bidirectional:d.edges.find(e=>e.id===selectedEdge)?.bidirectional??true;$('twoWay').disabled=!draft;
    $('editEdge').disabled=!d.edges.some(e=>e.id===selectedEdge);$('deleteEdge').disabled=!selectedEdge;$('copyRef').disabled=!selectedRef;
    $('draftInfo').textContent=draft?`${draft.id}: ${draft.from} → ${draft.to||'chọn node đích'} · ${draft.waypoints.length} waypoint. Bấm Lưu đường khi xong.`:d.edges.length?`${d.edges.length} đường đã lưu trong bản thiết kế. Chưa áp dụng vào game.`:'Chưa vẽ đường mới. Đường cũ chỉ là lớp tham khảo.';
    $('panTool').classList.toggle('active',tool==='pan');$('drawTool').classList.toggle('active',tool==='draw');$('pathTool').classList.toggle('active',tool==='path');$('designMode').classList.toggle('active',workspace==='design');$('referenceMode').classList.toggle('active',workspace==='reference');
    for(const id of ['begin','drawTool','saveEdge','copyRef','editEdge','deleteEdge','cancelDraft','deletePoint'])if(workspace==='reference')$(id).disabled=true;else if(['begin','drawTool'].includes(id))$(id).disabled=false;
    svg.style.cursor=pressed?.panning?'grabbing':tool==='draw'?'crosshair':tool==='path'?'pointer':'grab';renderInfo();
  }
  function infoLine(parent,label,value){const p=document.createElement('p');const b=document.createElement('b');b.textContent=label+': ';p.append(b,document.createTextNode(value));parent.append(p);}
  function renderInfo(){
    const box=$('info'),c=city(),d=design();box.replaceChildren();const n=M.node(c,selectedNode),e=d.draft?.id===selectedEdge?d.draft:d.edges.find(e=>e.id===selectedEdge),ref=c.referenceEdges.find(e=>e.id===selectedRef);
    if(n){const title=document.createElement('strong');title.textContent=n.id+' · '+n.name;box.append(title);infoLine(box,'Loại',n.categories.map(t=>typeNames[t]||t).join(', '));infoLine(box,'Tọa độ icon',`${n.position.x.toFixed(3)}, ${n.position.y.toFixed(3)}`);infoLine(box,'Lumi đứng',`${n.arrival.x.toFixed(3)}, ${n.arrival.y.toFixed(3)} (khóa)`);infoLine(box,'Cạnh cũ',c.referenceEdges.filter(e=>e.from===n.id||e.to===n.id).map(e=>e.id).join(', ')||'Không');infoLine(box,'Cạnh mới',d.edges.filter(e=>e.from===n.id||e.to===n.id).map(e=>e.id).join(', ')||'Chưa vẽ');if(n.portals.length)infoLine(box,'Chuyển thành phố',n.portals.map(e=>e.destinationCity).join(', ')+' · sự kiện riêng, không phải đường đi bộ');details(box,'Khóa / gameplay cần bảo toàn',{constraints:n.constraints,references:n.refs,portals:n.portals});}
    else if(e||ref){const value=e||ref;infoLine(box,'Tuyến',`${value.id}: ${value.from} → ${value.to||'chưa chọn'}`);infoLine(box,'Lớp',e?'Bản thiết kế / đang sửa':'Tham khảo từ Unity (không ghi đè)');infoLine(box,'Di chuyển',value.travel==='ferry'?'Đò · không phải đi bộ':'Đi bộ');infoLine(box,'Chiều',value.bidirectional?'Hai chiều':'Một chiều');infoLine(box,'Chiều dài',M.length(c,value).toFixed(2)+' đơn vị theo tỷ lệ tranh');details(box,'Waypoint / điều kiện',value);}
    else{infoLine(box,c.name,`${c.nodes.length} node, ${c.referenceEdges.length} cạnh tham khảo`);box.append(document.createTextNode('Chọn node để xem tên/khóa. Chọn một tuyến để xem waypoint. Tranh và tọa độ lấy từ source mới nhất.'));details(box,'Thông tin nguồn',{source:c.sourceImage,size:[c.imageWidth,c.imageHeight],sha256:c.sourceImageSHA256,cityConstraints:c.cityConstraints});}
  }
  function details(parent,title,value){const d=document.createElement('details'),s=document.createElement('summary'),p=document.createElement('pre');s.textContent=title;p.textContent=JSON.stringify(value,null,2);d.append(s,p);parent.append(d);}
  function permitDraft(){if(design().draft){notify('Bạn đang có một đường chưa lưu. Lưu đường hoặc Bỏ sửa trước khi mở đường khác.',true);return false;}return workspace==='design';}
  function begin(from,to=null){if(!permitDraft())return;change(()=>{design().draft=M.freshEdge(city(),design(),from,to);design().status='DRAFT';selectedEdge=design().draft.id;selectedNode='';selectedPoint=null;tool='draw';});$('designStatus').value='DRAFT';notify('Click để đặt điểm uốn. Kéo waypoint để sửa; chọn node đích rồi Lưu đường.');}
  function edit(id){if(!permitDraft())return;const e=design().edges.find(e=>e.id===id);if(!e)return;change(()=>{design().draft=clone(e);selectedEdge=id;selectedNode='';selectedPoint=null;tool='pan';});notify('Đang sửa bản nháp của tuyến. Nhấp đúp lên đoạn để thêm waypoint, hoặc chọn Vẽ đường để thêm ở cuối.');}
  function removePoint(index){if(design().draft&&index>=0&&index<design().draft.waypoints.length)change(()=>{design().draft.waypoints.splice(index,1);selectedPoint=null;});}
  function nodeClick(id){selectedNode=id;selectedRef='';
    if(tool==='path'){if(!pathPick){$('from').value=id;pathPick=id;notify('Đã chọn A '+id+'. Chọn node B.');}else{$('to').value=id;pathPick=null;testPath();}render();return;}
    if(tool==='draw'&&workspace==='design'){
      const d=design();if(!d.draft){begin(id);return;}
      if(d.draft.sourceEdgeId){notify('Tuyến nguồn giữ nguyên hai đầu. Hãy chỉnh waypoint; muốn đổi kết nối hãy tạo cạnh mới.',true);render();return;}
      if(id===d.draft.from){notify('Hãy chọn node đích khác node đầu.',true);render();return;}
      change(()=>{d.draft.to=id;$('to').value=id;});notify('Đã chọn node đích. Tiếp tục chỉnh điểm hoặc bấm Lưu đường.');return;
    }render();}
  function nearestSegment(e,p){const pts=M.points(city(),e);let best=Infinity,index=0;for(let i=1;i<pts.length;i++){const a=xy(pts[i-1]),b=xy(pts[i]),q=xy(p),dx=b[0]-a[0],dy=b[1]-a[1],t=Math.max(0,Math.min(1,((q[0]-a[0])*dx+(q[1]-a[1])*dy)/(dx*dx+dy*dy||1))),v=Math.hypot(q[0]-a[0]-t*dx,q[1]-a[1]-t*dy);if(v<best){best=v;index=i-1;}}return index;}
  function edgeClick(id,kind){selectedNode='';if(kind==='ref'){selectedRef=id;$('refSelect').value=id;notify('Đang xem tuyến cũ. Dùng “Lấy riêng tuyến này làm bản sửa” nếu muốn chỉnh.');}else{selectedEdge=id;selectedRef='';}render();}
  svg.addEventListener('pointerdown',e=>{
    if(e.button!==0&&e.button!==1&&e.button!==2)return;
    const wp=e.target.closest('[data-waypoint]'),n=e.target.closest('[data-node]'),edge=e.target.closest('[data-edge]');
    if(e.button===2&&!space&&wp){e.preventDefault();removePoint(Number(wp.dataset.waypoint));return;}
    pressed={x:e.clientX,y:e.clientY,pan:{...pan},moved:false,panning:e.button!==0||space||tool==='pan',node:n?.dataset.node,edge:edge?.dataset.edge,kind:edge?.dataset.kind,point:wp?Number(wp.dataset.waypoint):null};
    if(wp&&e.button===0&&!space&&workspace==='design'){push();selectedPoint=pressed.point;pressed.dragPoint=true;pressed.panning=false;}
    svg.setPointerCapture(e.pointerId);
  });
  svg.addEventListener('pointermove',e=>{
    if(pressed){const dx=e.clientX-pressed.x,dy=e.clientY-pressed.y;if(Math.hypot(dx,dy)>3)pressed.moved=true;
      if(pressed.dragPoint){const p=toModel(e.clientX,e.clientY);if(M.point(p)){design().draft.waypoints[pressed.point]=rounded(p);render();}}
      else if(pressed.panning&&pressed.moved){pan={x:pressed.pan.x+dx,y:pressed.pan.y+dy};render();}
      return;
    }
    const n=city().nodes.find(n=>n.id===e.target.closest('[data-node]')?.dataset.node);$('hover').hidden=!n;
    if(n){const r=$('viewport').getBoundingClientRect();$('hover').textContent=n.id+' · '+n.name+'\n'+n.categories.join(', ');$('hover').style.left=Math.min(r.width-265,Math.max(5,e.clientX-r.left+12))+'px';$('hover').style.top=Math.min(r.height-75,Math.max(5,e.clientY-r.top+12))+'px';}
  });
  svg.addEventListener('pointerup',e=>{
    if(!pressed)return;const was=pressed;pressed=null;if(svg.hasPointerCapture(e.pointerId))svg.releasePointerCapture(e.pointerId);
    if(was.dragPoint){render();persist();return;}
    if(was.moved)return;
    if(was.node){nodeClick(was.node);return;}
    if(was.edge&&tool!=='draw'){
      // Pointer capture + SVG redraw can suppress native click/dblclick events.
      // Recognize two actual pointer releases on the same segment instead.
      const twice=lastEdgeHit&&lastEdgeHit.id===was.edge&&performance.now()-lastEdgeHit.time<400&&Math.hypot(e.clientX-lastEdgeHit.x,e.clientY-lastEdgeHit.y)<5;
      lastEdgeHit={id:was.edge,kind:was.kind,x:e.clientX,y:e.clientY,time:performance.now()};
      if(twice&&workspace==='design'&&['design','draft'].includes(was.kind)){
        if(!design().draft)edit(was.edge);const p=toModel(e.clientX,e.clientY);
        if(design().draft?.id===was.edge&&M.point(p))change(()=>{const index=nearestSegment(design().draft,p);design().draft.waypoints.splice(index,0,rounded(p));selectedPoint=index;});
        lastEdgeHit=null;
      }else edgeClick(was.edge,was.kind);
      return;
    }
    if(tool==='draw'&&workspace==='design'){
      if(!design().draft){notify('Chọn node xuất phát trước.',true);return;}
      const p=toModel(e.clientX,e.clientY);if(!M.point(p)){notify('Điểm nằm ngoài tranh, không thêm.',true);return;}
      if(lastMapAdd&&performance.now()-lastMapAdd.time<350&&Math.hypot(e.clientX-lastMapAdd.x,e.clientY-lastMapAdd.y)<3)return;
      lastMapAdd={x:e.clientX,y:e.clientY,time:performance.now()};
      change(()=>{design().draft.waypoints.push(rounded(p));selectedPoint=design().draft.waypoints.length-1;});
    }
  });
  svg.addEventListener('pointercancel',()=>{pressed=null;render();});
  svg.addEventListener('contextmenu',e=>e.preventDefault());
  svg.addEventListener('pointerleave',()=>{$('hover').hidden=true;});
  svg.addEventListener('dblclick',e=>e.preventDefault());
  svg.addEventListener('wheel',e=>{e.preventDefault();zoomAt(Math.exp(-e.deltaY*.0015),e.clientX,e.clientY);},{passive:false});
  window.addEventListener('keydown',e=>{if(['INPUT','TEXTAREA','SELECT'].includes(document.activeElement.tagName))return;if(e.code==='Space'){space=true;e.preventDefault();}if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='z'){e.preventDefault();e.shiftKey?redo():undo();}if((e.ctrlKey||e.metaKey)&&e.key.toLowerCase()==='y'){e.preventDefault();redo();}if(e.key==='Delete'&&selectedPoint!==null){e.preventDefault();removePoint(selectedPoint);}});
  window.addEventListener('keyup',e=>{if(e.code==='Space')space=false;});window.addEventListener('blur',()=>{space=false;pressed=null;});
  $('city').onchange=()=>{persist();cityId=$('city').value;selectedEdge='';selectedNode='';selectedPoint=null;selectedRef='';pathResult=null;pathPick=null;fillCity();fit();persist();notify('Đã mở '+city().name+'. Bản nháp thành phố khác được giữ nguyên.');};
  for(const id of ['showOld','showNew','showLabels','showArrivals','showRefPoints','filter'])$(id).onchange=render;
  $('designMode').onclick=()=>{workspace='design';render();};$('referenceMode').onclick=()=>{workspace='reference';tool='pan';render();notify('Chế độ tham khảo: chỉ xem source, không chỉnh.');};
  $('panTool').onclick=()=>{tool='pan';render();};$('drawTool').onclick=()=>{tool='draw';render();notify('Click node đầu, click các điểm uốn, click node cuối.');};$('pathTool').onclick=()=>{tool='path';pathPick=null;render();notify('Chọn node A rồi B để thử tuyến.');};
  $('begin').onclick=()=>begin($('from').value,$('to').value);$('editEdge').onclick=()=>edit(selectedEdge);$('undo').onclick=undo;$('redo').onclick=redo;
  $('edgeSelect').onchange=()=>{selectedEdge=$('edgeSelect').value;selectedNode='';selectedRef='';render();};
  $('refSelect').onchange=()=>{selectedRef=$('refSelect').value;selectedNode='';selectedEdge='';render();};
  $('copyRef').onclick=()=>{if(!permitDraft())return;const e=city().referenceEdges.find(e=>e.id===selectedRef);if(!e)return;if(design().edges.some(v=>v.id===e.id)){notify('Tuyến này đã có trong bản thiết kế. Chọn nó ở Đường thiết kế để sửa.',true);return;}change(()=>{design().draft=M.copyReference(city(),e);selectedEdge=e.id;selectedNode='';selectedPoint=null;tool='pan';design().status='DRAFT';});$('designStatus').value='DRAFT';notify('Chỉ sao chép một tuyến đã chọn vào bản nháp, không thay đổi source.');};
  $('saveEdge').onclick=()=>{const e=design().draft;if(!e)return;try{M.validateEdge(city(),e);}catch(err){notify(err.message,true);return;}change(()=>{const index=design().edges.findIndex(v=>v.id===e.id);if(index>=0)design().edges[index]=clone(e);else design().edges.push(clone(e));selectedEdge=e.id;design().draft=null;design().status='DRAFT';selectedPoint=null;tool='pan';});$('designStatus').value='DRAFT';notify('Đã lưu đường vào bản thiết kế. Chưa nhập vào Unity.');};
  $('cancelDraft').onclick=()=>{if(design().draft&&confirm('Bỏ phần đang sửa của tuyến này? Đường đã lưu trước đó vẫn giữ nguyên.'))change(()=>{design().draft=null;selectedPoint=null;tool='pan';});};
  $('deletePoint').onclick=()=>removePoint(selectedPoint);
  $('deleteEdge').onclick=()=>{if(selectedEdge&&confirm('Xóa '+selectedEdge+' khỏi bản thiết kế? Đường production không đổi.'))change(()=>{design().edges=design().edges.filter(e=>e.id!==selectedEdge);if(design().draft?.id===selectedEdge)design().draft=null;selectedEdge='';selectedPoint=null;});};
  $('twoWay').onchange=()=>{if(design().draft)change(()=>design().draft.bidirectional=$('twoWay').checked);};
  $('designStatus').onchange=()=>change(()=>design().status=$('designStatus').value);$('notes').onchange=()=>change(()=>design().notes=$('notes').value);
  function testPath(){pathResult=M.shortest(city(),$('pathNetwork').value==='reference'?city().referenceEdges:design().edges,$('from').value,$('to').value);$('pathInfo').textContent=pathResult?`${pathResult.nodes.join(' → ')}\nDài ${pathResult.length.toFixed(2)}; thẳng ${pathResult.direct.toFixed(2)}; tỷ lệ ${pathResult.ratio.toFixed(2)}×. ${pathResult.ratio>3?'Đi vòng lớn — cần xem lại địa hình.':''} Chưa xét khóa/quest.`:'Không có tuyến nối trong mạng đã chọn. Không tự tạo đường tắt.';render();}
  $('testPath').onclick=testPath;$('pathNetwork').onchange=()=>{pathResult=null;render();};
  $('validate').onclick=()=>{const issues=M.inspect(city(),design()),box=$('warnings');box.replaceChildren();for(const v of issues){const p=document.createElement('div');p.className='warning '+v.kind;p.textContent=({ERROR:'Lỗi dữ liệu',RISK:'Rủi ro cần kiểm chứng',SUGGESTION:'Gợi ý'})[v.kind]+': '+v.text;box.append(p);}notify(`Rà soát: ${issues.filter(v=>v.kind==='ERROR').length} lỗi, ${issues.filter(v=>v.kind==='RISK').length} rủi ro. Không tự sửa dữ liệu.`);};
  $('fit').onclick=fit;$('resetView').onclick=fit;for(const[id,factor]of [['zoomIn',1.3],['zoomOut',1/1.3]])$(id).onclick=()=>{const r=svg.getBoundingClientRect();zoomAt(factor,r.left+r.width/2,r.top+r.height/2);};
  function download(id=null){try{const data=M.exportFile(project,id),blob=new Blob([JSON.stringify(data,null,2)],{type:'application/json'}),url=URL.createObjectURL(blob),a=document.createElement('a');a.href=url;a.download=`LBC_Routes_${id||'6Cities'}_${new Date().toISOString().replace(/[:.]/g,'-')}.json`;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);notify('Đã tải JSON. File chứa node canonical, cạnh, waypoint, chiều, khóa nguồn và phần đang vẽ dở. Chưa áp dụng vào game.');}catch(err){notify(err.message,true);}}
  $('exportAll').onclick=()=>download();$('exportCity').onclick=()=>download(cityId);$('saveDraft').onclick=()=>persist(true);$('import').onclick=()=>$('importFile').click();
  $('importFile').onchange=async()=>{const file=$('importFile').files[0];if(!file)return;try{if(file.size>15*1024*1024)throw Error('File quá lớn (tối đa15MiB).');const incoming=JSON.parse(await file.text()),merged=M.merge(project,incoming);change(()=>{project=merged;selectedEdge='';selectedPoint=null;selectedNode='';});storageWritable=true;fillCity();render();persist();notify(incoming.scope==='city'?'Đã nhập một thành phố; bản vẽ của năm thành phố khác được giữ.':'Đã nhập lại đầy đủ bản thiết kế.');}catch(err){notify('Không nhập: '+err.message+'. Bản vẽ đang mở vẫn nguyên vẹn.',true);}finally{$('importFile').value='';}};
  try{const saved=localStorage.getItem(storageKey);if(saved){const raw=JSON.parse(saved);project=M.validateImport(raw.project);if(M.city(raw.lastCity))cityId=raw.lastCity;notify('Đã khôi phục bản nháp trên trình duyệt. Hãy tải JSON định kỳ.');}else notify('Sáu bản đồ sẵn sàng. Thiết kế mới để trống; đường cũ chỉ tham khảo.');}
  catch(err){storageWritable=false;notify('Không khôi phục được bộ nhớ trình duyệt: '+err.message+'. Không ghi đè dữ liệu cũ; hãy mở JSON dự phòng.',true);}
  fillCity();requestAnimationFrame(fit);new ResizeObserver(()=>requestAnimationFrame(fit)).observe($('viewport'));
  window.RouteEditor={getProject:()=>clone(project),getCity:()=>clone(city()),getDraft:()=>clone(design().draft),mapToClient:toClient,clientToMap:toModel,storageKey,validate:()=>M.inspect(city(),design())};
})();
