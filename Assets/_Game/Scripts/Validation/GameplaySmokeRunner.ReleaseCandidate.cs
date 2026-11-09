#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator ReleaseCandidateJourney()
        {
            musicVirtualFocus=true;
            // Reuse existing corruption/backup/temp/v1 tests, on QA files only.
            SLSaveRecovery();
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();
                var expected=JsonUtility.ToJson(game.Save);
                for(var i=0;i<3;i++)Check(game.Persist(),"Repeated overwrite with existing backup persists: "+city.id+" #"+i);
                var disk=SaveSystem.Load(23);
                Check(JsonUtility.ToJson(disk)==expected,"Exact coins/items/quest/relic/ledger roundtrip: "+city.id);
                game.Reload();Check(JsonUtility.ToJson(game.Save)==expected,"Reload and city-state roundtrip: "+city.id);
                Check(!string.IsNullOrEmpty(game.Quests.Guidance())&&game.Data.nodes.Length==(city.id=="seoul"?23:22),"Representative city retains objectives and canonical nodes: "+city.id);
                var kit=DesignKitArt.Load();Check(new[]{kit.lumiPortrait,kit.lumiExploration,kit.lumiMapTraveler,kit.lumiCity,kit.lumiCitySmall}.All(s=>s==kit.lumiMapTraveler),"One approved white Lumi design: "+city.id);
                foreach(var e in game.Content.events.Where(e=>e.cityId==city.id&&e.type=="npc"&&!e.additive))
                {
                    var index=NodeIndex(e.nodeId);var view=game.Events.BuildOptions(index);game.Events.PruneRepeatedOptions(index,view,e);
                    Check(view.options.All(o=>o.label!="1"&&o.label!="2"),"No numbered fake choice labels: "+e.nodeId);
                }
                if(city.id=="hue")
                {
                    var map=GameObject.Find("CityMapBackground").GetComponent<Image>().sprite;
                    Check(map!=null&&map.texture.width>1000,"Applied Hue background loads at runtime");
                    Map.View.ZoomBy(1/Map.View.Zoom);yield return Snapshot("RC-Hue-approved-map");
                }
            }
            var old=JsonUtility.ToJson(game.Save);var coins=game.Save.coins;var meat=game.Inventory.Count("meat_portion");var request=Guid.NewGuid().ToString("N");
            Check(game.Shop.TryBuy("meat",request,out _),"Existing shop transaction saves successfully");
            Check(game.Save.coins==coins-1&&game.Inventory.Count("meat_portion")==meat+1,"One coin buys exactly one item at unchanged price");
            Check(!game.Shop.TryBuy("meat",request,out _),"Repeated request cannot duplicate debit or item");
            var earned=JsonUtility.ToJson(game.Save);game.Reload();Check(JsonUtility.ToJson(game.Save)==earned,"Shop transaction survives exact save/reload");
            var g14=game.Content.events.First(e=>e.cityId=="gyeongju"&&e.nodeId=="G14"&&!e.additive);
            Check(!g14.mandatory&&string.IsNullOrEmpty(g14.questId),"G14 remains optional lore; no quest was bypassed or route altered");
            KeepSave("RC-earned");
        }
    }
}
#endif
