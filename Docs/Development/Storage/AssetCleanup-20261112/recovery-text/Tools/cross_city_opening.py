"""Add the authored food-exchange openings; original LBC design packs stay read-only.
No bitmap output. Sprite slice outlines inspect existing alpha and preserve faces.
"""
from pathlib import Path
import json
import numpy as np
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,d):p.parent.mkdir(parents=True,exist_ok=True);p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def point(x,y):return dict(x=round(x/1448,9),y=round(1-y/1086,9))

def hull(points):
    p=sorted(set(points))
    def cross(a,b,c):return (b[0]-a[0])*(c[1]-a[1])-(b[1]-a[1])*(c[0]-a[0])
    lower=[];upper=[]
    for v in p:
        while len(lower)>1 and cross(lower[-2],lower[-1],v)<=0:lower.pop()
        lower.append(v)
    for v in reversed(p):
        while len(upper)>1 and cross(upper[-2],upper[-1],v)<=0:upper.pop()
        upper.append(v)
    return lower[:-1]+upper[:-1]

def slices():
    atlas=ROOT/'Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png'
    alpha=np.array(Image.open(atlas).getchannel('A'));specs=[]
    for name,seed in [('npc_huong',(1000,300)),('npc_soyeon',(440,800))]:
        seen=set();stack=[seed]
        while stack:
            x,y=stack.pop()
            if (x,y) in seen or x<0 or y<0 or y>=alpha.shape[0] or x>=alpha.shape[1] or alpha[y,x]<1:continue
            seen.add((x,y));stack.extend([(x-1,y),(x+1,y),(x,y-1),(x,y+1)])
        assert len(seen)>50000
        x=min(p[0] for p in seen)-2;y=min(p[1] for p in seen)-2
        w=max(p[0] for p in seen)-x+3;h=max(p[1] for p in seen)-y+3
        outline=hull([(xx+dx,yy+dy) for xx,yy in seen for dx,dy in [(-.6,-.6),(.6,-.6),(.6,.6),(-.6,.6)]])
        # Native Sprite Editor outline coordinates are relative to rect center, y-up.
        verts=[dict(x=round(xx-x-w/2,3),y=round(y+h/2-yy,3)) for xx,yy in outline]
        specs.append(dict(id=name,x=x,y=y,width=w,height=h,outline=verts,
            status='TEMPORARY_UNASSIGNED_ROSTER_CAST; NOT_CANONICAL; replace registry reference when approved'))
    write(ROOT/'Docs/Art/CrossCitySpriteSlices.json',dict(atlas=atlas.relative_to(ROOT).as_posix(),slices=specs))

def main():
    path=ROOT/'Assets/_Game/Resources/Content/GameplayContent.json';d=read(path)
    hooks=read(ROOT/'LBC_HoiAn_StoryExpansion_v1/Data/cross_city_food_hooks.json')['cross_city_events']
    source_quests=read(ROOT/'LBC_HoiAn_StoryExpansion_v1/Data/hoi_an_chapter.json')
    assert len(d['items'])==37 and len(d['quests'])==28 and sum(len(c['nodes']) for c in d['cities'])==133
    baseline=read(ROOT.parent/'Backups/BeforeCrossCityFood-20261016/Assets_Game/Resources/Content/GameplayContent.json')
    records=[]
    for cid,ids,title in [('hue',['U0','U1'],'HUẾ · CHỢ BỜ NAM'),('jeonju',['J0','J15'],'JEONJU · CHỢ HANOK')]:
        c=next(c for c in d['cities'] if c['id']==cid);c.update(runtimeReady=True,runtimeNodeIds=ids,mapTitle=title)
        nodes={n['id']:n for n in c['nodes']}
        # Huế: grounded landing at the foot of the existing river bridge/stairs.
        # Jeonju: retain south gate, move only the market roof anchor to its frontage.
        adjustments={'U0':(504,610),'U1':(387,705)} if cid=='hue' else {'J15':(974,777)}
        for nid,pixel in adjustments.items():
            old=next(n for c0 in baseline['cities'] if c0['id']==cid for n in c0['nodes'] if n['id']==nid)['position'];nodes[nid]['position']=point(*pixel)
            records.append(dict(city=cid,node=nid,old=old,new=nodes[nid]['position'],pixel=pixel,reason='Raw design anchors are approximate; align entry/merchant feet with paved frontage. IDs/order unchanged.'))
        for nid in ids:nodes[nid]['revealStage']=0
        edge=next(e for e in c['edges'] if e['from']==ids[0] and e['to']==ids[1])
        pixels=[(494,630),(478,649),(458,667),(435,682),(410,696)] if cid=='hue' else [(750,964),(780,940),(807,918),(839,900),(875,884),(913,869),(954,848),(994,827),(1031,811),(1065,803),(1090,788),(1108,769),(1122,750),(1111,732),(1082,717),(1048,714),(1012,737)]
        records.append(dict(city=cid,edge=edge['id'],old=next(e for c0 in baseline['cities'] if c0['id']==cid for e in c0['edges'] if e['id']==edge['id'])['waypoints'],new=[point(*p) for p in pixels],pixels=pixels,
            reason='Author only the opening corridor: Hue market stairs; Jeonju street and the existing east stone bridge. Other routes are NOT VERIFIED.'))
        edge.update(waypoints=[point(*p) for p in pixels],status='opening_corridor_authored_runtime_verification_required')
        intro=next(e for e in d['events'] if e['cityId']==cid and e['nodeId']==ids[0]);intro.update(actor='Lumi',sourceStatus='playable_opening; authored_bridge_dialogue_proposal')
        intro['text']='Mình đã tới bờ nam sông Hương. Chợ ở ngay cuối bậc thang; chị Hương đang chờ gói hành của Bà Tư.' if cid=='hue' else 'Cổng làng Jeonju đã hiện ra. Mình sẽ men theo phố và qua cầu đá tới quầy So-yeon, mang lời chào từ Hội An.'
    next(c for c in d['cities'] if c['id']=='hoi_an')['mapTitle']='HỘI AN · PHỐ ĐÈN'
    for qid,eid,npcid,item in [('HQ05','foodexchange_jeonju_j15','npc_soyeon','hoi_an_fresh_chili'),('HQ06','foodexchange_hue_u1','npc_huong','hoi_an_purple_shallot')]:
        q=next(q for q in d['quests'] if q['id']==qid);hook=next(h for h in hooks if h['id']==eid)
        q.update(pending=False,pendingReason='',confirmDelivery=True,confirmationText=hook['dialogue'][1]['text'],sourceStatus='LBC_HoiAn_StoryExpansion_v1; playable_cross_city_delivery')
        extra=next(e for e in d['events'] if e['id']==eid);extra.update(text=hook['dialogue'][0]['text'],completedText=hook['dialogue'][2]['text'],sourceStatus='cross_city_food_hooks.json; playable_confirmation_delivery',missingText='Chưa có đủ món quà. Hãy quay lại Bà Tư ở H10 Hội An để xin đúng một món cần giao; cháu không phải bỏ nhiệm vụ.')
        base=next(e for e in d['events'] if not e.get('additive') and e['cityId']==extra['cityId'] and e['nodeId']==extra['nodeId'])
        base.update(actor=extra['actor'],npcId=npcid,text=hook['dialogue'][0]['text'],sourceStatus='playable_food_npc; original_local_shop_or_letter_quest_preserved_pending')
        recovery=next(e for e in d['events'] if e['id']=='recover_'+qid.lower());recovery['refillIfMissing']=True
    q=next(q for q in d['quests'] if q['id']=='HQ07');hook=next(h for h in hooks if h['quest']=='HQ07')
    q.update(confirmDelivery=True,confirmationText=hook['dialogue'][1]['text'],acceptQuests=['HQ05','HQ06'],requiredQuests=['HQ05','HQ06'])
    next(e for e in d['events'] if e['id']=='food_hq07')['completedText']=hook['dialogue'][0]['text']
    write(path,d)
    p=ROOT/'Assets/_Game/Resources/Content/AdventurePresentation.json';a=read(p)
    for cid,nid,aid,qid in [('hue','U1','npc_huong','HQ06'),('jeonju','J15','npc_soyeon','HQ05')]:
        actor=dict(cityId=cid,nodeId=nid,artId=aid,size=dict(x=52,y=62),offset=dict(x=22,y=8),hideWhenResolved=False)
        a['mapActors']=[x for x in a['mapActors'] if (x['cityId'],x['nodeId'])!=(cid,nid)]+[actor]
        marker=dict(cityId=cid,nodeId=nid,marker='NpcTalk',effect='Pulse',size=24,rules=[
            dict(questId=qid,questState='completed',marker='Completed',effect='None'),
            dict(questId=qid,questState='ready',marker='NpcTalk',effect='Bounce'),
            dict(questId=qid,questState='notReady',marker='NpcNeedItem',effect='Glow')])
        a['nodeMarkers']=[x for x in a['nodeMarkers'] if (x['cityId'],x['nodeId'])!=(cid,nid)]+[marker]
    write(p,a);slices()
    write(ROOT/'Docs/Maps/CrossCityOpeningReview.json',dict(scope='Two opening corridors, not complete chapters; all 133 nodes retained',changes=records,
        conflicts=['J15 moves from reveal stage 2 to 0: newer food hook requires only HQ01 restoration + accepted HQ05, not two Jeonju main gates.',
        'City runtimeNodeIds explicitly limits this release to U0/U1 and J0/J15. Local quest/gate definitions and remaining nodes stay in data; not marked complete.',
        'Chị Hương and So-yeon use two distinct unassigned NPC roster faces TEMPORARILY, per user authorization. Mai/Ji-eun unchanged.',
        'Source hook unique grant keys are represented by the existing quest:<id>:<itemId> ledger; rewards and counts are unchanged.']))
    print('Authored U0/U1, J0/J15, HQ05/HQ06/HQ07; two native sprite slices; no bitmap output.')

if __name__=='__main__':main()
