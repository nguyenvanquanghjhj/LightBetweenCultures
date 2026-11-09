"""Reproducible terrain authoring/review, pixel coordinates on the approved 1448x1086 art.
No art pixels, topology, quests, gate conditions or save records are changed.
The resulting plots are technical overlays; they are not runtime art assets.
"""
import copy, hashlib, html, json, collections
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT.parent / 'Backups/CheckpointCityMapUIPolish-20261017'
REL = Path('Assets/_Game/Resources/Content/GameplayContent.json')
OUT = ROOT / 'Docs/Maps/TerrainRoutes-20261017'
W, H = 1448, 1086

def read(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,x):
    p.parent.mkdir(parents=True,exist_ok=True)
    p.write_text(json.dumps(x,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def norm(p): return dict(x=round(p[0]/W,9),y=round(1-p[1]/H,9))
def pixel(p): return [round(p['x']*W,3),round((1-p['y'])*H,3)]
def join(*sections):
    result=[]
    for section in sections:
        for p in section:
            if not result or p!=result[-1]: result.append(p)
    return result

# Deliberately follow the exterior market pavement. The apparent shortcut
# through the canopy roofs is not used without evidence of a walkable gap.
outer=[(347,479),(355,503),(351,524),(337,541),(313,550),(279,555),
       (243,559),(216,565),(190,577),(163,593),(146,610),(131,631),
       (120,655),(109,682),(108,708),(119,735),(139,760),(164,781),
       (193,798),(224,813),(260,824),(295,827),(325,829),(348,829)]
market=[(348,829),(352,803),(353,784),(337,770),(333,748),(328,727),(316,702)]
mai=[(347,479),(369,486),(403,490),(449,494),(490,498),(535,501),(570,500)]
workshop=[(570,500),(598,505),(618,510),(628,521)]
alley=[(628,521),(645,502),(663,489),(680,481),(697,467),(704,447),
       (713,433),(732,423),(751,415),(770,407),(792,403),(811,408),(823,417)]
quay=[(823,417),(858,414),(889,406),(916,393),(938,379),(958,365),(968,354)]
north=[(968,354),(980,338),(984,327),(972,318),(950,311),(923,299),
       (894,283),(862,265),(831,247),(801,231),(772,221),(742,218),
       (710,218),(680,217),(655,215),(640,213)]
west_street=[(347,479),(340,460),(334,438),(336,415),(350,390),(369,365),
             (383,337),(398,306),(417,274),(425,240),(430,205),(430,170)]
court_link=[(430,170),(445,177),(467,183),(494,190),(522,195),
            (548,205),(568,214),(600,215),(640,213)]
temple=[(336,415),(311,405),(277,397),(245,391),(219,379),(206,363),
        (201,343),(200,318),(199,294),(202,269),(215,246),(219,223),
        (218,200),(216,177),(211,152),(226,139),(249,132)]
stairs=[(628,521),(635,541),(641,561),(636,581),(629,603),(624,624),(630,638)]
bridge_west=[(630,638),(651,630),(670,621),(692,618),(719,618),(750,615),(780,610),(810,605),(840,599),(865,595),(884,589)]
# Real covered bridge: the logical ground line is inside its enclosure, not
# across the roof ridge or open water. Baked roof/front wall occludes the actor.
covered_bridge=[(884,589),(904,580),(922,569),(938,559),(961,551),(985,550),
                (1008,558),(1038,570),(1069,584),(1100,600),(1133,620),
                (1155,638),(1185,650)]
east_walk=[(1185,650),(1194,633),(1202,614),(1213,596),(1217,580),
           (1221,564),(1234,549),(1256,539),(1286,530),(1313,520),(1330,509),(1340,505)]
east_dock=[(1185,650),(1212,663),(1247,683),(1284,703),(1305,715)]
small_bridge=[(347,479),(355,503),(359,519),(374,533),(397,535),
              (420,527),(459,533),(501,542),(542,554),(579,568),(616,581),
              (633,575),(636,581),(629,603),(624,624),(630,638)]
plaza_stairs=[(419,750),(462,758),(503,763),(547,761),(586,751),(616,736),
              (639,720),(640,695),(640,670),(630,638)]
plaza_market=[(419,750),(395,747),(369,741),(347,734),(328,727),(316,702)]
dock_plaza=[(365,842),(357,813),(352,786),(371,773),(395,762),(419,750)]

arrivals={'H0':(365,842),'H1':(347,479),'H2':(570,500),'H3':(751,415),
          'H4':(316,702),'H5':(1185,650),'H6':(884,589),'H7':(640,213),
          'H8':(249,132),'H9':(202,269),'H10':(146,610),'H11':(419,750),
          'H12':(737,769),'H13':(628,521),'H14':(968,354),'H15':(1340,505),
          'H16':(978,794),'H17':(1305,715),'H18':(823,417),'H19':(1085,280),
          'H20':(780,233),'H21':(425,240)}
routes={
 ('H0','H11'):dock_plaza,
 ('H0','H1'):join(dock_plaza,plaza_stairs,list(reversed(small_bridge))),
 ('H1','H2'):mai,
 ('H1','H4'):join(small_bridge,list(reversed(plaza_stairs)),plaza_market),
 ('H2','H9'):join(list(reversed(mai)),west_street[:4],temple[:10]),
 ('H2','H13'):workshop,
 ('H13','H3'):alley[:9],
 ('H13','H14'):join(alley,quay),
 ('H14','H19'):[(968,354),(980,338),(995,323),(1020,314),(1045,301),(1066,289),(1085,280)],
 ('H14','H18'):list(reversed(quay)),
 ('H18','H5'):join(list(reversed(alley)),stairs,bridge_west,covered_bridge),
 ('H5','H6'):list(reversed(covered_bridge)),
 ('H6','H7'):join(list(reversed(bridge_west)),list(reversed(stairs)),alley,quay,north),
 ('H6','H15'):join(covered_bridge,east_walk),
 ('H15','H17'):join(list(reversed(east_walk)),east_dock),
 ('H7','H8'):join(list(reversed(north)),list(reversed(quay)),list(reversed(alley)),list(reversed(workshop)),list(reversed(mai)),west_street[:4],temple),
 ('H7','H20'):[(640,213),(655,215),(680,217),(710,218),(742,218),(765,224),(780,233)],
 ('H4','H10'):join(list(reversed(market)),list(reversed(outer[10:]))),
 ('H11','H12'):[(419,750),(462,758),(503,763),(547,761),(586,751),(616,736),(639,720),(659,731),(676,750),(696,765),(715,771),(737,769)],
 ('H12','H16'):[(737,769),(779,756),(818,738),(864,724),(904,725),(941,742),(967,765),(978,794)],
 ('H3','H21'):join(list(reversed(alley[:9])),list(reversed(workshop)),list(reversed(mai)),west_street[:10]),
 ('H17','H5'):[(1305,715),(1285,736),(1265,748),(1195,750),(1135,745),(1105,715),(1105,680),(1135,650),(1165,654),(1185,650)]
}

def author():
    before=read(BASE/REL);current=read(ROOT/REL)
    old=next(c for c in before['cities'] if c['id']=='hoi_an')
    city=next(c for c in current['cities'] if c['id']=='hoi_an')
    for n in city['nodes']:
        n['hasArrivalPosition']=True;n['arrivalPosition']=norm(arrivals[n['id']])
    for e in city['edges']:
        k=(e['from'],e['to']);pts=routes.get(k)
        if pts is None:pts=list(reversed(routes[(k[1],k[0])]))
        assert pts[0]==arrivals[e['from']] and pts[-1]==arrivals[e['to']],k
        e['waypoints']=[norm(p) for p in pts[1:-1]]
        e['status']='terrain_authored_pending_visual_acceptance'
    city['coveredWalkways']=[dict(id='hoi_an.covered_bridge',polygon=[norm(p) for p in [
        (920,558),(948,535),(985,533),(1020,549),(1060,570),(1100,589),
        (1140,614),(1152,632),(1145,648),(1126,631),(1093,614),(1056,596),
        (1020,580),(985,565),(958,568),(930,581)]])]
    # Strong guard: only HA arrival metadata, route geometry/status, cover data.
    compare=copy.deepcopy(current);hc=next(c for c in compare['cities'] if c['id']=='hoi_an')
    hc['nodes']=copy.deepcopy(old['nodes']);hc['edges']=copy.deepcopy(old['edges']);hc.pop('coveredWalkways',None)
    assert compare==before,'Unrelated content changed; inspect rather than overwrite.'
    for n,o in zip(city['nodes'],old['nodes']):
        stripped={k:v for k,v in n.items() if k not in ('hasArrivalPosition','arrivalPosition')}
        assert stripped==o
    write(ROOT/REL,current)
    report=[]
    for e,o in zip(city['edges'],old['edges']):
        assert {k:v for k,v in e.items() if k not in ('waypoints','status')}=={k:v for k,v in o.items() if k not in ('waypoints','status')}
        report.append(dict(edge=e['from']+'-'+e['to'],travel=e.get('travel','walk'),
            oldWaypoints=o['waypoints'],newWaypoints=e['waypoints'],
            oldPixels=[pixel(p) for p in o['waypoints']],newPixels=[pixel(p) for p in e['waypoints']],
            oldEndpoints=[pixel(next(n for n in old['nodes'] if n['id']==v)['position']) for v in (e['from'],e['to'])],
            newEndpoints=[list(arrivals[v]) for v in (e['from'],e['to'])],
            reason='Ground arrival separated from marker; follow authored street/quay/steps instead of roofs or water.' if e.get('travel')!='ferry' else 'Boat-only water corridor between actual dock landings; original quest flag retained.',
            terrainStatus='PENDING_VISUAL_ACCEPTANCE'))
    write(OUT/'edge-waypoint-diff.json',report)
    write(OUT/'arrival-anchors.json',[dict(node=n['id'],markerPixels=pixel(n['position']),arrivalPixels=list(arrivals[n['id']])) for n in city['nodes']])
    return old,city

def plots(old,city):
    import matplotlib
    matplotlib.use('Agg')
    import matplotlib.pyplot as plt
    from matplotlib.patches import Rectangle,Polygon
    im=plt.imread(ROOT/'Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png')
    regions=[('R1: covered bridge floor / NEEDS_REVIEW',(904,490,300,160)),
             ('R2: temple side steps / NEEDS_REVIEW',(190,135,48,105)),
             ('R3: eastern canopy edge / NEEDS_REVIEW',(1190,550,60,65))]
    obstacles=[[(450,355),(535,360),(550,455),(410,440)],[(690,290),(775,292),(807,390),(701,390)],
               [(655,225),(727,218),(783,274),(674,300)],[(795,145),(886,148),(925,208),(811,207)],
               [(1130,700),(1195,740),(1080,885),(1000,850)]]
    for label,c in [('before',old),('after',city)]:
        fig,ax=plt.subplots(figsize=(14.48,10.86),dpi=100);ax.imshow(im)
        for poly in obstacles:ax.add_patch(Polygon(poly,color='#ed4545',alpha=.20))
        for e in c['edges']:
            ns={n['id']:n for n in c['nodes']}
            pts=[ns[e['from']].get('arrivalPosition',ns[e['from']]['position'])]+e['waypoints']+[ns[e['to']].get('arrivalPosition',ns[e['to']]['position'])]
            xy=[pixel(p) for p in pts];ax.plot(*zip(*xy),color='#40f7c4' if label=='after' else '#ffdf61',linewidth=2,alpha=.9)
        for n in c['nodes']:
            p=pixel(n.get('arrivalPosition',n['position']));ax.scatter(*p,s=17,c='white',edgecolors='black',zorder=4);ax.text(p[0]+4,p[1]-4,n['id'],fontsize=8,color='white',bbox=dict(facecolor='#173e48',alpha=.8,pad=1))
        for title,(x,y,w,h) in regions:
            ax.add_patch(Rectangle((x,y),w,h,fill=False,edgecolor='#ff6de5',linestyle='--',linewidth=2));ax.text(x,y-5,title,color='#ff6de5',fontsize=8,bbox=dict(facecolor='black',alpha=.75,pad=1))
        ax.set(xlim=(0,W),ylim=(H,0),title=label.upper()+' - original-art coordinates; red = example obstacles, magenta = review regions')
        ax.set_xticks(range(0,W,100));ax.set_yticks(range(0,H,100));ax.grid(alpha=.13)
        fig.tight_layout();fig.savefig(OUT/(label+'-terrain-overlay.png'));plt.close(fig)
    write(OUT/'needs-review.json',[dict(region=t,pixelBox=b,status='NEEDS_REVIEW',reason='Baked art partially occludes ground; no terrain PASS claimed.') for t,b in regions])

if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True)
    old,city=author();plots(old,city)
    print('22 nodes and 22 existing edges retained; only ground geometry/arrival metadata authored.')
