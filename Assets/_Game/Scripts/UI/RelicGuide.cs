using System;
using System.Linq;
namespace LightBetweenCultures
{
    // Read-only guidance from the one canonical collection. Never grants proof or progression.
    public static class RelicGuide
    {
        public static CulturalRelic Local(GameBootstrap g)=>g.Relics.Catalog.relics.FirstOrDefault(r=>r.enabled&&r.cityId==g.Data.cityId);
        public static bool AtSource(GameBootstrap g,RelicFragment f,string node)=>f.cityId==g.Data.cityId&&f.nodeId==node||(f.claimAnchors??Array.Empty<string>()).Contains(g.Data.cityId+":"+node);
        public static bool PendingAt(GameBootstrap g,string node)
        {
            var r=Local(g);return r!=null&&!g.Relics.Restored(r.id)&&r.fragmentIds.Select(g.Relics.Fragment).Any(f=>f!=null&&f.enabled&&!g.Relics.Owned(f.id)&&AtSource(g,f,node));
        }
        public static string Place(GameBootstrap g,CulturalRelic r)=>g.Content.City(r.cityId)?.nodes.FirstOrDefault(n=>n.id==r.restorationNode)?.name??"địa điểm phục dựng";
        public static string Next(GameBootstrap g,CulturalRelic r)
        {
            if(g.Relics.Restored(r.id))return "Đã phục dựng · Có thể tiếp tục hành trình.";
            var count=g.Relics.Count(r);
            if(count<4)return count+"/4 mảnh. Tìm dấu ! đỏ tại các địa điểm đã khám phá; hoàn thành tương tác ở đó rồi ghi lại mảnh.";
            var place=Place(g,r)+" · "+g.Content.City(r.cityId).name;
            if(!g.Relics.Assembled(r.id))return "Đã đủ 4/4 mảnh. Đến "+place+", chọn Phục dựng để ghép bốn mảnh. Ghép xong còn thử thách "+r.trial+".";
            if(!string.IsNullOrEmpty(r.requiredQuest)&&!g.Quests.IsCompleted(r.requiredQuest))return "Đã ghép đủ. Hoàn thành "+g.Content.Quest(r.requiredQuest).title+" tại Trạm đèn hội quán, rồi trở lại "+place+" làm thử thách "+r.trial+".";
            if(!string.IsNullOrEmpty(r.requiredFlag)&&!g.World.Has(r.requiredFlag))return "Đã ghép đủ. "+PlayerCopy.Requirement(r.requiredFlag)+", rồi trở lại "+place+".";
            return "Đã ghép đủ. Đến "+place+", chọn Phục dựng và hoàn thành "+r.trial+" để thắp sáng di vật.";
        }
        public static int KnownSource(GameBootstrap g,RelicFragment f)
        {
            for(var i=0;i<g.Data.nodes.Length;i++)if(AtSource(g,f,g.Data.nodes[i].id)&&g.Discovery.DiscoveryState(i)==MapDiscoveryState.REVEALED&&(i==g.Save.node||g.Data.FindPath(g.Save.node,i,g.Save)!=null))return i;
            return -1;
        }
    }
}
