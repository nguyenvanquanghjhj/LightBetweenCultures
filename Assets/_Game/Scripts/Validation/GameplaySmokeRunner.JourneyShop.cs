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
        IEnumerator ShopClick(string name)
        {
            var button=FindButton(name);Check(button!=null,"Shop control exists: "+name);
            var scroll=GameObject.Find("ShopViewport")?.GetComponent<ScrollRect>();
            if(scroll!=null&&button.transform.IsChildOf(scroll.content))
            {
                Canvas.ForceUpdateCanvases();var rect=(RectTransform)button.transform;
                var local=(Vector2)scroll.content.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
                var range=scroll.content.rect.height-scroll.viewport.rect.height;
                scroll.verticalNormalizedPosition=range>0?1-Mathf.Clamp01((-local.y-scroll.viewport.rect.height*.5f)/range):1;
                yield return null;yield return null;
            }
            yield return Click(name,true);
        }
        bool ShopBuy(string id)=>game.Shop.TryBuy(id,Guid.NewGuid().ToString("N"),out _);
        IEnumerator ShopToolbarJourney()
        {
            musicVirtualFocus=true;
            yield return CrossEnter("hoi_an");yield return CloseEvent();
            var button=FindButton("JourneyShop");var rect=(RectTransform)button.transform;
            var objective=(RectTransform)FindButton("ObjectiveHint").transform;
            Check(Mathf.Abs(rect.rect.width-100)<.01f&&Mathf.Abs((1-rect.rect.width/138)*100-27.536f)<.02f,"Shop width is reduced 27.5%, with a 100-unit touch target");
            Check(rect.rect.height==objective.rect.height&&rect.rect.height==52,"Shop keeps ObjectiveHint's height and 52-unit finger target");
            Check(Mathf.Abs(rect.offsetMin.x-objective.offsetMax.x-8)<.01f,"Only shop moves: compact 8-unit gap after ObjectiveHint");
            var label=button.transform.Find("Label").GetComponent<Text>();var icon=button.transform.Find("Icon").GetComponent<Image>();
            Check(label.text=="Cửa hàng"&&label.alignment==TextAnchor.MiddleCenter,"Shop caption stays centered and unchanged");
            Check(Mathf.Abs((icon.rectTransform.anchorMin.x+icon.rectTransform.anchorMax.x)*.5f-.5f)<.001f&&!icon.raycastTarget,"Existing coin icon is centered without blocking touch");
            Check(button.targetGraphic is SoftButtonGraphic,"Existing rounded glossy button graphic is retained");
            var saved=JsonUtility.ToJson(game.Save);
            yield return Click("JourneyShop");Check(FindButton("ShopClose")!=null,"Mouse opens the existing shop callback");
            yield return Click("ShopClose");
            yield return Click("JourneyShop",true);Check(FindButton("ShopClose")!=null,"Simulated touch opens the existing shop callback");
            yield return Click("ShopClose",true);
            Check(JsonUtility.ToJson(game.Save)==saved&&Map.View.InputEnabled,"Open/close leaves gameplay and save untouched");
            yield return Snapshot("SHOP-toolbar-compact");
        }
        IEnumerator ShopLoad(string file)
        {
            game.Scenes.Go("Prototype");yield return Scene("Prototype");
            File.Copy(file,savePath,true);game.Reload();
        }
        IEnumerator JourneyShopJourney()
        {
            musicVirtualFocus=true;
            yield return CrossEnter("hoi_an");
            var initial=JsonUtility.ToJson(game.Save);yield return Click("JourneyShop",true);
            Check(JourneyShop.Catalog.Count==9&&JourneyShop.Catalog.All(e=>FindButton("ShopBuy_"+e.id)!=null),"Fresh player sees all nine offers, including other cities, without unlock gates");
            Check(JourneyShop.Catalog.All(e=>!FindButton("ShopBuy_"+e.id).interactable),"Insufficient funds keeps every offer visible and disables purchase");
            Check(GameObject.Find("ShopCoins").GetComponent<Text>().text.Contains("0"),"Shop displays actual existing coin balance");
            Check(!ShopBuy("tea")&&JsonUtility.ToJson(game.Save)==initial,"Backend also rejects insufficient funds without mutation");
            yield return Snapshot("SHOP-fresh-all-visible");yield return Click("ShopClose",true);
            Check(JsonUtility.ToJson(game.Save)==initial&&!Map.DialogueOpen&&Map.View.InputEnabled,"Open/close and X never charge coins or change progress");
            // Earn the first balance through real current node callbacks, not a coin fixture.
            yield return Travel(NodeIndex("H2"),true);yield return Click("EventOption_accept",true);yield return CloseEvent();
            yield return ChapterGo("H11");yield return ChapterChoice("collect");yield return CloseEvent();
            Check(game.Save.coins==6,"Existing early H11 chest earns six real coins: enough for several ordinary purchases");
            yield return Click("JourneyShop",true);yield return ShopClick("ShopBuy_chili");
            Check(game.Save.coins==5&&game.Inventory.Count("hoi_an_fresh_chili")==1,"One real touch debits one coin and grants one ingredient");
            yield return ShopClick("ShopBuy_chili");
            Check(game.Save.coins==5&&game.Inventory.Count("hoi_an_fresh_chili")==1,"Rapid repeated touch is debounced, no second debit/grant");
            yield return new WaitForSecondsRealtime(.4f);yield return ShopClick("ShopBuy_shallot");
            Check(game.Save.coins==4&&game.Inventory.Count("hoi_an_purple_shallot")==1,"Next deliberate purchase uses the same global fixed price");
            yield return new WaitForSecondsRealtime(.4f);yield return ShopClick("ShopBuy_herb");
            Check(game.Save.coins==2&&game.Inventory.Has("royal_herb")&&!game.Quests.IsCompleted("UQ02"),"Buying a quest item never automatically accepts/completes its delivery quest");
            Check(!FindButton("ShopBuy_herb").interactable,"Owned unique item cannot be bought uselessly a second time");
            yield return ShopClick("ShopFilter_permissions");
            Check(FindButton("ShopBuy_ferry_hq04")!=null&&!FindButton("ShopBuy_ferry_hq04").interactable&&GameObject.Find("ShopNotice")!=null,"Permission remains visible even before its city/quest timing and shows insufficient funds");
            yield return Click("ShopClose",true);KeepSave("SHOP-earned-items");
            var earned=JsonUtility.ToJson(game.Save);game.Reload();
            Check(JsonUtility.ToJson(game.Save)==earned&&game.Shop.Owned(game.Shop.Find("herb")),"Reload preserves exact balance, inventory and unique ownership in existing schema");
            foreach(var id in new[]{"lamp_eye","link_key","hanji_core","ancient_seal","relic_hoi_an_lantern","fragment_hoi_an_roof_hook","SQ03","seoul.final_cleared","game.ending_seen"})
            {var before=JsonUtility.ToJson(game.Save);Check(!ShopBuy(id)&&JsonUtility.ToJson(game.Save)==before,"Backend allowlist rejects forbidden/unknown purchase: "+id);}
            // Existing owner-earned partial save, modified only inside the QA sandbox.
            var args=Environment.GetCommandLineArgs();var partial=args.First(a=>a.StartsWith("--shop-partial-save=")).Substring("--shop-partial-save=".Length);
            yield return ShopLoad(partial);
            game.Save.items.RemoveAll(x=>x=="hoi_an_fresh_chili"||x=="hoi_an_purple_shallot");game.Save.coins=30;game.Persist();
            Check(game.Quests.IsActive("HA_FOOD_PREP")&&!game.Quests.IsCompleted("HA_FOOD_PREP"),"Delivery fixture retains the genuinely accepted optional food quest");
            for(var i=0;i<3;i++)Check(ShopBuy("chili"),"Purchase required chilli/reserve "+i);
            for(var i=0;i<2;i++)Check(ShopBuy("shallot"),"Purchase required shallot/reserve "+i);
            Check(game.Save.coins==25&&game.Quests.CanComplete("HA_FOOD_PREP"),"Five purchased ingredients satisfy the same canonical requirements and reserves");
            Check(game.Quests.Complete("HA_FOOD_PREP","H10")&&game.Save.coins==33&&game.Inventory.Count("hoi_an_fresh_chili")==1&&game.Inventory.Count("hoi_an_purple_shallot")==1,"Canonical manual delivery consumes purchased ingredients and grants the old reward once");
            var paid=JsonUtility.ToJson(game.Save);Check(!game.Quests.Complete("HA_FOOD_PREP","H10")&&JsonUtility.ToJson(game.Save)==paid,"A bought ingredient cannot farm repeat quest rewards");
            var request=Guid.NewGuid().ToString("N");Check(game.Shop.TryBuy("meat",request,out _),"First transaction token accepted");
            paid=JsonUtility.ToJson(game.Save);Check(!game.Shop.TryBuy("meat",request,out _)&&JsonUtility.ToJson(game.Save)==paid,"Repeated identical transaction token cannot charge/grant twice");
            game.Reload();Check(!game.Shop.TryBuy("meat",request,out _)&&JsonUtility.ToJson(game.Save)==paid,"Persistent ledger rejects the same request after reload");
            // One optional shortcut, remote and always displayed; no chapter/boss/relic flags.
            var world=args.First(a=>a.StartsWith("--shop-world-save=")).Substring("--shop-world-save=".Length);
            yield return ShopLoad(world);
            game.Save.completed.Remove("HQ04");game.Save.accepted.Remove("HQ04");
            game.Save.flags.RemoveAll(f=>f=="quest:HQ04"||f=="hoi_an.ferry_available");game.Save.coins=30;game.Persist();
            yield return CrossEnter("seoul");yield return CloseEvent();
            var core=JsonUtility.ToJson(game.Save);yield return Click("JourneyShop",true);yield return ShopClick("ShopFilter_permissions");yield return ShopClick("ShopBuy_ferry_hq04");
            Check(FindButton("ShopConfirmBuy")!=null&&JsonUtility.ToJson(game.Save)==core,"Permission confirmation has no side effect");
            yield return Click("ShopConfirmCancel",true);Check(JsonUtility.ToJson(game.Save)==core,"Cancel preserves every coin, flag and quest");
            yield return new WaitForSecondsRealtime(.4f);yield return ShopClick("ShopBuy_ferry_hq04");yield return Click("ShopConfirmBuy",true);
            Check(game.Save.coins==25&&game.Quests.IsCompleted("HQ04")&&game.World.Has("hoi_an.ferry_available"),"Remote paid fare commits exactly five coins and canonical HQ04/ferry state immediately");
            var old=JsonUtility.FromJson<SaveData>(core);
            Check(game.Save.completed.Except(old.completed).SequenceEqual(new[]{"HQ04"})&&game.Save.items.SequenceEqual(old.items)&&game.Save.solvedPuzzles.SequenceEqual(old.solvedPuzzles)&&game.Save.defeatedEnemies.SequenceEqual(old.defeatedEnemies)&&game.Save.flags.Except(old.flags).All(f=>f=="quest:HQ04"||f=="hoi_an.ferry_available"),"Paid permission touches no other quest, boss, puzzle, relic, item or ending state");
            paid=JsonUtility.ToJson(game.Save);Check(!ShopBuy("ferry_hq04")&&JsonUtility.ToJson(game.Save)==paid,"Completed permission cannot be purchased twice");
            yield return Click("ShopClose",true);game.Reload();Check(JsonUtility.ToJson(game.Save)==paid,"Purchased canonical permission survives reload without duplicate cost/reward");
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();var saved=JsonUtility.ToJson(game.Save);
                yield return Click("JourneyShop",true);yield return ShopClick("ShopFilter_items");
                Check(JourneyShop.Catalog.Where(e=>!e.Permission).All(e=>FindButton("ShopBuy_"+e.id)!=null),"All item offers present in "+city.id);
                var scroll=GameObject.Find("ShopViewport").GetComponent<ScrollRect>();scroll.verticalNormalizedPosition=0;yield return null;
                Check(scroll.content.rect.height>scroll.viewport.rect.height,"Portrait shop has a real scrollable list: "+city.id);
                if(city.id=="seoul")yield return Snapshot("SHOP-seoul-items-scrolled");
                yield return Click("ShopClose",true);
                Check(JsonUtility.ToJson(game.Save)==saved&&Map.View.InputEnabled,"Shop leaves exact map/save/input intact: "+city.id);
            }
            var restore=JsonUtility.ToJson(game.Save);
            yield return ShopLoad(world);game.Save.cityId="hoi_an";game.Save.node=17;game.Save.coins=6;
            game.Save.completed.Remove("HQ04");game.Save.accepted.Remove("HQ04");game.Save.flags.RemoveAll(f=>f=="quest:HQ04"||f=="hoi_an.ferry_available");game.Persist();game.Reload();
            Check(game.Quests.Accept("HQ04")&&game.Quests.Complete("HQ04","H17")&&game.Save.coins==1&&game.World.Has("hoi_an.ferry_available"),"Original manual HQ04 acceptance/payment still works at the canonical NPC");
            var manual=JsonUtility.ToJson(game.Save);Check(!ShopBuy("ferry_hq04")&&JsonUtility.ToJson(game.Save)==manual,"Manual completion is recognized as owned in the shop, no useless charge");
            File.WriteAllText(savePath,restore);game.Reload();
            // Write failure fixture: no live/player data, only a new QA directory as an invalid file target.
            var path=SaveSystem.OverridePath;var bad=Path.Combine(output,"write-failure-target");Directory.CreateDirectory(bad);
            SaveSystem.OverridePath=bad;paid=JsonUtility.ToJson(game.Save);bool failed;
            Application.logMessageReceived-=OnLog;
            try{failed=!ShopBuy("meat");}finally{Application.logMessageReceived+=OnLog;SaveSystem.OverridePath=path;}
            Check(failed&&JsonUtility.ToJson(game.Save)==paid,"Failed persistence rolls back coin, inventory and ledger in memory");
            Check(File.ReadAllText(bad+".tmp")==JsonUtility.ToJson(game.Save,true),"Failed transaction temporary payload contains the original state, not an unacknowledged debit");
            // Current v1/v2 loader, no new fields or migration.
            var v1=JsonUtility.FromJson<SaveData>(paid);v1.version=1;File.WriteAllText(savePath,JsonUtility.ToJson(v1,true));game.Reload();
            Check(game.Save.version==2&&game.Save.coins==v1.coins&&game.Save.items.SequenceEqual(v1.items)&&game.Quests.IsCompleted("HQ04"),"Existing v1 compatibility preserves shop balance/items/permission without schema changes");
            KeepSave("SHOP-final");
        }
    }
}
#endif
