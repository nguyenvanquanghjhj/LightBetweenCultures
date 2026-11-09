using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Focused in-memory regression only; never opens or writes any player save.
    public static class AndroidQaFerryAudit
    {
        public static void Validate()
        {
            var checks=new List<string>();
            void Check(bool ok,string name){if(!ok)throw new InvalidOperationException("FERRY AUDIT: "+name);checks.Add(name);Debug.Log("FERRY AUDIT PASS: "+name);}
            var data=GameData.Load("hoi_an");var content=ContentDatabase.Load();var quest=content.Quest("HQ04");
            Check(data.navigation!=null&&data.navigation.enabled,"Approved Phase B graph is authoritative");
            Check(!JourneyShop.Catalog.Any(e=>e.id=="ferry_hq04"),"Redundant ferry permission is absent from sellable catalog");
            var prices=new Dictionary<string,int>{{"chili",1},{"shallot",1},{"meat",1},{"tea",2},{"herb",2},{"flower",2},{"glasses",3},{"strawberry",2}};
            Check(JourneyShop.Catalog.Count==8&&JourneyShop.Catalog.All(e=>prices.TryGetValue(e.id,out var price)&&price==e.price),"All eight other shop prices retained");
            Check(quest!=null&&quest.giverNode=="H17"&&quest.completeAtNode=="H17"&&quest.completionFlag=="hoi_an.ferry_available"&&quest.consumeItems&&quest.requiredItems.Length==1&&quest.requiredItems[0].itemId=="coin"&&quest.requiredItems[0].amount==5,"Original HQ04 location, five-coin payment and flag retained");
            var save=new SaveData{cityId="hoi_an"};save.cityProgress.Add(new CityProgress{cityId="hoi_an",stage=2});
            var ferries=data.navigation.edges.Where(e=>e.travel=="ferry").ToArray();
            Check(ferries.Length==1&&ferries[0].source.Contains("R1")&&!ferries[0].requiredFlags.Contains("hoi_an.ferry_available"),"Active R1 ferry does not require the purchased permission");
            Check(data.navigation.EdgeOpen(ferries[0],save),"R1 ferry opens at stage 2 without HQ04 or five coins");
            var from=Array.FindIndex(data.nodes,n=>n.id=="H0");var to=Array.FindIndex(data.nodes,n=>n.id=="H16");
            var journey=data.FindRoute(from,to,save);
            Check(journey!=null&&journey.legs.Any(l=>l.edge.travel=="ferry"),"H0 to H16 legally routes through R1 with zero coins and no HQ04");
            var old=new SaveData{cityId="hoi_an",coins=17};old.completed.Add("HQ04");old.flags.Add("hoi_an.ferry_available");old.rewardGrantIds.Add("shop:v1:unlock:HQ04");old.rewardGrantIds.Add("shop:v1:purchase:00000000-0000-0000-0000-000000000001");
            var restored=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(old));
            Check(restored.coins==17&&restored.completed.SequenceEqual(old.completed)&&restored.flags.SequenceEqual(old.flags)&&restored.rewardGrantIds.SequenceEqual(old.rewardGrantIds),"Historical paid state and both receipts remain serializable unchanged");
            Directory.CreateDirectory("Validation/AndroidAutoPlay-20261024");
            File.WriteAllLines("Validation/AndroidAutoPlay-20261024/ferry-focused-checks.txt",checks);
        }
    }
}
