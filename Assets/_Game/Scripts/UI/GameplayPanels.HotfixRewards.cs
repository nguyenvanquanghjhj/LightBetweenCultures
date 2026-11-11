using System;
using System.Collections.Generic;
using System.Linq;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        int lastStableDialogueNode=-1;
        private sealed class ActualRewardWatch
        {
            readonly GameBootstrap game;
            readonly Dictionary<string,int> before=new Dictionary<string,int>();
            readonly int coins;
            public ActualRewardWatch(GameBootstrap game,params ItemAmount[][] sources)
            {
                this.game=game;coins=game.Save.coins;
                foreach(var source in sources??Array.Empty<ItemAmount[]>())
                    foreach(var item in source??Array.Empty<ItemAmount>())
                        if(item!=null&&!string.IsNullOrEmpty(item.itemId)&&!before.ContainsKey(item.itemId))
                            before[item.itemId]=game.Inventory.Count(item.itemId);
            }
            public ItemAmount[] Earned()
            {
                var actual=new List<ItemAmount>();
                foreach(var entry in before)
                {
                    var delta=game.Inventory.Count(entry.Key)-entry.Value;
                    if(delta>0)actual.Add(new ItemAmount{itemId=entry.Key,amount=delta});
                }
                var coinDelta=game.Save.coins-coins;
                if(coinDelta>0)actual.Add(new ItemAmount{itemId="coin",amount=coinDelta});
                return actual.ToArray();
            }
        }
        private ActualRewardWatch WatchActualRewards(params ItemAmount[][] sources)
        {return new ActualRewardWatch(game,sources);}
    }
}
