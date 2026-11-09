using System;
using System.Linq;
namespace LightBetweenCultures
{
    public sealed partial class QuestManager
    {
        int guidanceKey=int.MinValue;string[] guidance;int guidanceTarget=-1;
        public int GuidanceTarget {get {Guidance();return guidanceTarget;}}
        // Read-only, cached by actual story state. Leisure is deliberately excluded.
        public string Guidance(int level=0)
        {
            var key=game.Save.node*397^game.Data.cityId.GetHashCode()^game.Save.flags.Count*31^game.Save.completed.Count*7919^game.Save.accepted.Count;
            foreach(var item in game.Save.items)key=unchecked(key*31+item.GetHashCode());
            if(guidance==null||guidanceKey!=key){guidanceKey=key;guidance=BuildGuidance();}
            return guidance[Math.Max(0,Math.Min(2,level))];
        }
        string[] BuildGuidance()
        {
            var target=-1;string action=null;
            if(game.Data.cityId=="hoi_an"&&!Completed)
            {target=GuidanceNode(!Active||game.Inventory.Has(game.Data.quest.item)?"H2":"H3");action=target>=0&&game.Data.nodes[target].id=="H3"?"Tìm Mắt Đèn tại ":game.Inventory.Has(game.Data.quest.item)?"Mang Mắt Đèn trả Cô Mai tại ":"Gặp Cô Mai nhận việc tại ";}
            var relic=RelicGuide.Local(game);
            if(target<0&&relic!=null&&game.Relics.Count(relic)==4&&!game.Relics.Restored(relic.id)&&
                (!game.Relics.Assembled(relic.id)||string.IsNullOrEmpty(relic.requiredQuest)||IsCompleted(relic.requiredQuest)))
            {
                target=GuidanceNode(relic.restorationNode);
                if(target>=0){guidanceTarget=target;return new[]{"Đủ 4 mảnh · Đến "+game.Data.nodes[target].name+(game.Relics.Assembled(relic.id)?" làm thử thách.":" để ghép hình."),RelicGuide.Next(game,relic),"Chạm địa điểm, chọn Phục dựng "+relic.name+". Đủ mảnh chưa tự hoàn tất: cần ghép và vượt thử thách "+relic.trial+"."};}
            }
            if(target<0&&relic!=null&&!game.Relics.Restored(relic.id)&&game.Relics.Count(relic)<4&&game.World.Has(game.Content.City(game.Data.cityId).completionFlag))
            {
                foreach(var f in relic.fragmentIds.Select(game.Relics.Fragment).Where(f=>!game.Relics.Owned(f.id)))
                {
                    target=RelicGuide.KnownSource(game,f);if(target<0)continue;
                    guidanceTarget=target;
                    return new[]{"Tìm "+f.name+" tại "+game.Data.nodes[target].name+".",game.Relics.Count(relic)+"/4 mảnh · "+relic.name,"Theo dấu ! đỏ. Hoàn thành tương tác tại "+game.Data.nodes[target].name+", rồi chọn Ghi lại mảnh. Sau đủ bốn mảnh, trở lại "+RelicGuide.Place(game,relic)+" để ghép và phục dựng."};
                }
            }
            foreach(var quest in game.Content.quests.Where(q=>IsActive(q.id)&&!IsCompleted(q.id)).OrderBy(q=>q.id.EndsWith("01",StringComparison.Ordinal)?0:1))
            {
                if(target>=0)break;
                if(quest.cityId!=game.Data.cityId&&quest.completeCityId!=game.Data.cityId&&quest.giverCityId!=game.Data.cityId)continue;
                if(CanComplete(quest.id))target=GuidanceNode(quest.completeAtNode);
                if(target>=0){action="Giao kết quả tại ";break;}
                foreach(var item in quest.requiredItems??Array.Empty<ItemAmount>())
                {var reserve=Array.Find(quest.reserveItems??Array.Empty<ItemAmount>(),i=>i.itemId==item.itemId)?.amount??0;if(game.Inventory.Count(item.itemId)<item.amount+reserve){target=GuidanceItem(item.itemId,0);if(target>=0){action="Tìm "+(game.Content.Item(item.itemId)?.name??"vật phẩm còn thiếu")+" tại ";break;}}}
                if(target<0)foreach(var flag in quest.requiredFlags??Array.Empty<string>())if(!game.World.Has(flag)){target=GuidanceFlag(flag,0);if(target>=0)break;}
                if(target<0&&quest.cityId==game.Data.cityId)target=GuidanceNode(quest.giverNode);
            }
            if(target<0)
            {
                if(relic!=null&&!game.Relics.Restored(relic.id))foreach(var f in relic.fragmentIds.Select(game.Relics.Fragment).Where(f=>!game.Relics.Owned(f.id)))
                {target=RelicGuide.KnownSource(game,f);if(target>=0){action="Tìm "+f.name+" tại ";break;}}
            }
            if(target<0)
            {
                foreach(var entry in game.Content.events.Where(e=>e.cityId==game.Data.cityId&&e.type=="npc"&&game.Quests.CanAccept(e.questId)).OrderBy(e=>e.questId.EndsWith("01",StringComparison.Ordinal)?0:1))
                {target=GuidanceNode(entry.nodeId);if(target>=0){action="Trao đổi tại ";break;}}
            }
            guidanceTarget=target;
            if(target<0)return new[]{"Khám phá những khu vực đã mở.",game.Content.City(game.Data.cityId).name+" · Các tuyến đã khám phá vẫn có thể quay lại.","Xem nhật ký nhiệm vụ và sổ di vật. Những trải nghiệm thư giãn luôn là tùy chọn."};
            var node=game.Data.nodes[target];var entryAt=game.Events.Definition(node.eventId);var presentation=ActivityPresentation.For(entryAt?.puzzleId);
            var goal=(action??(presentation?.kind=="LIGHT_STORY_RESOLUTION"?"Quan sát tại ":entryAt?.type=="puzzle"?"Hoàn thiện tại ":"Đến "))+node.name+".";
            var hint=presentation!=null?presentation.kind=="LIGHT_STORY_RESOLUTION"?presentation.actionLabel+" để ghi lại manh mối.":presentation.title+" · Quan sát hình mờ và vạch hướng trên bàn thao tác.":"Chạm vào địa điểm được chỉ dẫn để Lumi đi tới, rồi chọn hành động trong khung trò chuyện. Nhật ký lưu lại việc còn thiếu.";
            return new[]{goal,game.Content.City(game.Data.cityId).name+" · "+node.name,hint};
        }
        int GuidanceNode(string id)
        {
            var i=Array.FindIndex(game.Data.nodes,n=>n.id==id);
            return i>=0&&game.Discovery.IsVisible(i)&&(i==game.Save.node||game.Data.FindPath(game.Save.node,i,game.Save)!=null)?i:-1;
        }
        int GuidancePuzzle(PuzzleDefinition p,int depth)
        {
            if(p.cityId!=game.Data.cityId||depth>5||game.Puzzles.Solved(p.id))return -1;
            foreach(var flag in p.requiredFlags??Array.Empty<string>())if(!game.World.Has(flag))return GuidanceFlag(flag,depth+1);
            foreach(var item in p.requiredItems??Array.Empty<ItemAmount>())if(!game.Inventory.Has(item.itemId,item.amount))return GuidanceItem(item.itemId,depth+1);
            return GuidanceNode(p.nodeId);
        }
        int GuidanceFlag(string flag,int depth)
        {
            if(depth>5)return -1;
            foreach(var p in game.Content.puzzles)
                if(flag=="puzzle:"+p.id||p.completionFlag==flag||(p.flags??Array.Empty<string>()).Contains(flag)){var i=GuidancePuzzle(p,depth+1);if(i>=0)return i;}
            foreach(var e in game.Content.events.Where(e=>e.cityId==game.Data.cityId))
                if(e.completionFlag==flag||(e.flags??Array.Empty<string>()).Contains(flag)||(e.onTalkFlags??Array.Empty<string>()).Contains(flag))
                {var i=GuidanceEvent(e,depth+1);if(i>=0)return i;}
            return -1;
        }
        int GuidanceEvent(EventDefinition e,int depth)
        {
            if(depth>5)return -1;
            foreach(var f in e.requiredFlags??Array.Empty<string>())if(!game.World.Has(f))return GuidanceFlag(f,depth+1);
            foreach(var item in e.requiredItems??Array.Empty<ItemAmount>())if(!game.Inventory.Has(item.itemId,item.amount))return GuidanceItem(item.itemId,depth+1);
            return GuidanceNode(e.nodeId);
        }
        int GuidanceItem(string id,int depth)
        {
            if(depth>5)return -1;
            foreach(var p in game.Content.puzzles.Where(p=>(p.rewardItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==id)))
            {var i=GuidancePuzzle(p,depth+1);if(i>=0)return i;}
            foreach(var e in game.Content.events.Where(e=>e.cityId==game.Data.cityId&&(e.itemId==id||(e.rewardItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==id)||(game.Content.Enemy(e.enemyId)?.rewardItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==id))))
            {var i=GuidanceEvent(e,depth+1);if(i>=0)return i;}
            return -1;
        }
    }
}
