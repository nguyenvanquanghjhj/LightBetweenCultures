"""Apply reviewed street/bridge waypoint edits. Original node anchors/graph stay fixed.
Pixel coordinates refer to the unchanged 1448x1086 Hoi An Overview A, top-left origin.
This is an explicit authoring tool, never a runtime route generator for other cities.
"""
from pathlib import Path
import json

ROOT = Path(__file__).resolve().parents[1]
BEFORE = ROOT.parent / 'Backups/BeforeCityMapNatural-20261016/Assets_Game'
W, H = 1448, 1086

# Shared corridors inspected on the source image: west stone bridge, central
# waterfront, covered bridge, east quay and lower market promenade.
WEST = [(470,552),(530,561),(585,580),(625,594),(641,578),(641,549)]
QUAY = [(654,558),(700,562),(760,585),(810,594),(865,584),(906,564),(954,555)]
BRIDGE = [(977,550),(1020,559),(1060,575),(1102,598),(1137,632),(1167,655)]
DOCK = [(1187,668),(1227,670),(1265,686),(1268,705),(1235,725),(1207,729)]
MARKET = [(185,641),(170,613),(180,588),(226,566),(285,554),(355,552)]
REVIEW = {
    (1,2): (WEST+[(628,520),(628,494),(596,486),(570,478)], 'West stone bridge and central stairs; final H2 anchor is a roof projection requiring approval.'),
    (1,3): (WEST+[(628,520),(630,497),(680,491),(725,482),(765,467),(794,448),(810,426),(800,408),(776,394)], 'Stone bridge, central street and alley; final H3 anchor is a roof projection requiring approval.'),
    (1,4): (list(reversed(MARKET)), 'West market stairs and bridge approach.'),
    (3,5): ([(770,393),(802,410),(817,432),(813,452),(791,470),(771,484),(791,523),(812,551),(851,574),(893,567),(935,553)]+BRIDGE+DOCK, 'Replaces northeast river shortcut with the existing covered bridge and east wooden quay; H3 endpoint remains a roof projection.'),
    (9,5): ([(246,297),(278,340),(327,372),(365,378),(401,339),(428,296),(449,252),(475,219),(518,213),(560,215),(606,216),(646,219),(696,224),(739,241),(779,263),(825,285),(873,308),(927,324),(970,322),(978,337),(947,369),(915,397),(875,420),(838,440),(799,462),(771,484),(791,523),(812,551),(851,574),(893,567),(935,553)]+BRIDGE+DOCK, 'West shrine stairs, central square, north waterfront street, covered bridge and east quay replace the northeast river shortcut. Roof-projection anchors are unchanged.'),
    (4,6): (MARKET+[(405,551)]+WEST+QUAY, 'West stone bridge and central waterfront to covered bridge; H6 retains the approved roof-level bridge anchor.'),
    (5,6): (list(reversed(DOCK))+list(reversed(BRIDGE[1:])), 'East wooden quay and covered bridge replace the diagonal across open water; H6 roof-level anchor retained.'),
    (6,7): (list(reversed(QUAY))+[(680,491),(725,482),(819,476),(791,426),(736,405),(691,344),(648,267)], 'Corrects water crossing west of covered bridge. Interior approach to H7 remains NOT TERRAIN VERIFIED; existing alley tail retained.'),
    (4,12): ([(250,682),(292,700),(327,728),(361,749),(387,756),(442,767),(500,773),(558,771),(615,765),(667,756),(708,743)], 'Lower market promenade replaces the cut through the west canal.'),
    (1,13): (WEST, 'Follows the existing stone bridge and turns up the central stairs; no open-water segment.'),
    (6,15): (BRIDGE[2:]+[(1196,639),(1241,627),(1288,605),(1322,582),(1344,554),(1361,524),(1376,494),(1370,480)], 'Covered bridge and east bank promenade, never the channel or park roofs.'),
}

def load(p): return json.loads(p.read_text(encoding='utf-8-sig'))
def norm(points): return [dict(x=round(x/W,9), y=round(1-y/H,9)) for x,y in points]

def main():
    old=load(BEFORE/'Resources/HoiAnData.json')
    legacy=load(ROOT/'Assets/_Game/Resources/HoiAnData.json')
    content=load(ROOT/'Assets/_Game/Resources/Content/GameplayContent.json')
    city=next(c for c in content['cities'] if c['id']=='hoi_an')
    assert legacy['nodes']==old['nodes'] and len(legacy['nodes'])==22
    records=[]
    for (a,b),(pixels,note) in REVIEW.items():
        before=next(e for e in old['edges'] if (e['from'],e['to'])==(a,b))
        edge=next(e for e in legacy['edges'] if (e['from'],e['to'])==(a,b))
        canonical=next(e for e in city['edges'] if (e['from'],e['to'])==(f'H{a}',f'H{b}'))
        points=norm(pixels);edge['bends']=points;canonical['waypoints']=points
        records.append(dict(edge=f'H{a}-H{b}', old=before['bends'], new=points, newPixels=pixels, note=note))
    for path,data in [('Assets/_Game/Resources/HoiAnData.json',legacy),('Assets/_Game/Resources/Content/GameplayContent.json',content)]:
        (ROOT/path).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    report=ROOT/'Docs/Maps/HoiAnRouteReview.json';report.parent.mkdir(parents=True,exist_ok=True)
    report.write_text(json.dumps(dict(image='Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png',pixelOrigin='top-left',size=[W,H],changed=records,
        needsApproval=['H0 anchor (460,868) is on open water: H0-H1 and H0-H11 unchanged, no invented footbridge.',
        'H2, H3, H6 and H21 anchors project onto roofs; unchanged by requirement.',
        'H14-H19 and H13-H14 pass dense roof/tree details; unchanged pending a reviewed walking mask.',
        'H2-H9, H7-H8 and H3-H21 upper lanes remain uncertain; unchanged.',
        'H6-H7 interior tail needs a walking-mask review; no claim that all remaining lanes are obstacle-free.',
        'H17-H5 remains the existing story-unlocked ferry route, rendered/travelled as ferry rather than walking.']),ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    config=ROOT/'Assets/_Game/Resources/Content/AdventurePresentation.json';data=load(config)
    data.update(mapActorScale=.72,mapActorTouchSize=52,mapActorStyles=[])
    config.write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(f'{len(records)} explicit route edits; all 22 node anchors unchanged; shared actor scale 0.72.')

if __name__=='__main__': main()
