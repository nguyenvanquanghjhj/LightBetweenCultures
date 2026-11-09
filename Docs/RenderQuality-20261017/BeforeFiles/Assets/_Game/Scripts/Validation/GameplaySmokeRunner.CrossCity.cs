#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Additional journeys in the existing opt-in runner, not another manager/test scene.
    public sealed partial class GameplaySmokeRunner
    {
        private string CrossCheckpointName=>Path.GetFileName(Environment.GetCommandLineArgs().FirstOrDefault(s=>s.StartsWith("--cross-checkpoint="))?.Substring("--cross-checkpoint=".Length)??"hoi_an-after");
        private int NodeIndex(string id)=>Array.FindIndex(game.Data.nodes,n=>n.id==id);

        private IEnumerator CrossEnter(string city)
        {
            if(Map!=null)yield return CloseEvent();
            if(SceneManager.GetActiveScene().name=="Prototype"){yield return Click("Start");yield return Scene("WorldMap");}
            else if(SceneManager.GetActiveScene().name=="CityMap"){yield return Click("Back",true);yield return Scene("WorldMap");}
            Check(SceneManager.GetActiveScene().name=="WorldMap","City changes go through the actual World Map");
            var before=game.Save.cityId;var node=game.Save.node;
            var button=FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name.StartsWith("City")&&b.GetComponentsInChildren<Text>().Any(t=>t.text==game.Content.City(city).name));
            yield return Click(button.name,true);
            Check(game.Save.cityId==before&&game.Save.node==node,"Selecting "+city+" only previews its destination until Enter is confirmed");
            Check(FindButton("EnterCity").interactable,"Earned world flags enable the "+city+" opening");
            yield return Snapshot("X-WorldMap-select-"+city);
            yield return Click("EnterCity");yield return Scene("CityMap");
            Check(game.Save.cityId==city&&game.Data.cityId==city,"World Map enters the configured "+city+" data in the existing CityMap scene");
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
        }

        private void CrossCheckOpening()
        {
            var city=game.Content.City(game.Data.cityId);var allowed=city.runtimeNodeIds;
            if(allowed.Length==0)
            {CheckChapterDiscovery();return;}
            Check(allowed.Length==2&&game.Data.nodes.Length==22,"Partial "+city.id+" opening retains all 22 nodes and exposes only the two authored locations");
            Check(GameObject.Find("CityMapBackground").GetComponent<Image>().sprite==AdventureUIArt.Load().City(city.id).sprite,"CityMap uses the approved "+city.id+" Map A, rather than the Hoi An background");
            Check(GameObject.Find("Title").GetComponent<Text>().text==city.mapTitle,"City HUD uses its own configured name");
            Check(Map.Marker.GetComponent<Image>().sprite==DesignKitArt.Load().lumiMapTraveler,"The approved small Lumi map sprite is shared across cities");
            Check(game.Discovery.Stage(city.id)==0,"Food exchange cannot advance local main-story gates in "+city.id);
            for(var i=0;i<game.Data.nodes.Length;i++)
            {
                var n=game.Data.nodes[i];var visible=allowed.Contains(n.id);
                Check(game.Discovery.IsVisible(i)==visible&&Map.View.Content.Find(n.id).gameObject.activeSelf==visible&&NodeMarker(n.id).gameObject.activeSelf==visible,"Opening discovery/input/marker agree for "+city.id+" "+n.id);
                if(visible)continue;
                Check(game.Data.FindPath(game.Save.node,i,game.Save)==null&&game.Events.BuildOptions(i).options.Count==0&&!game.Events.OnArrive(i,false),"Unimplemented "+n.id+" is excluded from routing and events, not a playable preview");
            }
            CheckMapActors(city.id+" opening");
            Check(FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length==1,"Cross-city navigation keeps one Canvas/EventSystem/bootstrap");
        }

        private IEnumerator CrossBag(string label,string item)
        {
            yield return CloseEvent();yield return Click("Inventory",true);
            GameObject.Find("ItemViewport").GetComponent<ScrollRect>().verticalNormalizedPosition=0;yield return null;yield return null;
            if(item!=null&&FindButton("Item_"+item)!=null)
            {
                var scroll=GameObject.Find("ItemViewport").GetComponent<ScrollRect>();
                Canvas.ForceUpdateCanvases();
                var cell=(RectTransform)FindButton("Item_"+item).transform;
                var center=scroll.content.InverseTransformPoint(cell.TransformPoint(cell.rect.center));
                var range=scroll.content.rect.height-scroll.viewport.rect.height;
                scroll.verticalNormalizedPosition=range<=0?1:Mathf.Clamp01(1-(-center.y-scroll.viewport.rect.height*.5f)/range);
                yield return null;yield return null;
                yield return Click("Item_"+item,true);
                Check(GameObject.Find("SelectedItemCount").GetComponent<Text>().text.EndsWith(game.Inventory.Count(item).ToString()),"Inventory detail shows the actual cross-city quantity: "+item);
                Check(GameObject.Find("SelectedItemIcon").GetComponent<Image>().sprite==AdventureUIArt.Load().Item(item).sprite,"Cross-city item keeps its shared icon: "+item);
            }
            yield return Snapshot(label);yield return Click("CloseInventory");
        }

        private IEnumerator CrossJournal(string label)
        {
            yield return CloseEvent();yield return Click("QuestJournal");
            var body=GameObject.Find("Body").GetComponent<Text>();
            Check(body.text.Contains(game.Content.Quest("HQ05").title)&&body.text.Contains(game.Content.Quest("HQ06").title),"Journal tracks both delivery quests across cities");
            var scroll=GameObject.Find("JournalViewport").GetComponent<ScrollRect>();scroll.verticalNormalizedPosition=0;
            yield return Snapshot(label);yield return Click("CloseJournal");
        }

        private IEnumerator CrossRecoveryFixture()
        {
            var real=JsonUtility.ToJson(game.Save);yield return Travel(10);yield return CloseEvent();
            foreach(var item in new[]{"hoi_an_fresh_chili","hoi_an_purple_shallot"})game.Inventory.TryConsume(item,game.Inventory.Count(item));
            // Reproduce an old save whose one-time recovery was already used.
            foreach(var q in new[]{"hq05","hq06"})
            {game.World.Set("hoi_an.recovered_"+q);game.Save.resolvedEvents.Add("hoi_an:recover_"+q);}
            game.World.Set("jeonju.chili_shared");game.World.Set("hue.shallot_shared");
            Check(!game.Quests.CanAccept("HQ07"),"Two flags without actual completed HQ05/HQ06 cannot unlock HQ07 (isolated legacy-state fixture)");
            game.Save.flags.Remove("jeonju.chili_shared");game.Save.flags.Remove("hue.shallot_shared");game.Persist();
            var coins=game.Save.coins;yield return Click("Interact");
            yield return FoodChoice("recover:recover_hq05");yield return FoodChoice("recover:recover_hq06");
            Check(game.Inventory.Count("hoi_an_fresh_chili")==1&&game.Inventory.Count("hoi_an_purple_shallot")==1&&game.Save.coins==coins,"Used legacy recovery can refill one missing quest spice without resetting progress or farming coins");
            game.Inventory.TryConsume("hoi_an_fresh_chili");yield return CloseEvent();yield return Click("Interact");yield return FoodChoice("recover:recover_hq05");
            Check(game.Inventory.Count("hoi_an_fresh_chili")==1&&!game.Events.Execute(10,"recover:recover_hq05").changed,"Repeated loss cannot softlock the delivery; recovery remains capped at one owned item");
            yield return Snapshot("X-Recovery-used-legacy-fixture");
            File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==real,"Recovery fixture restores the actual earned inventory and quest state");
        }

        private IEnumerator CrossDeliver(string city,bool negative,bool resumeChapter=false)
        {
            var hue=city=="hue";var id=hue?"U1":"J15";var q=hue?"HQ06":"HQ05";
            var item=hue?"hoi_an_purple_shallot":"hoi_an_fresh_chili";var reward=hue?"hue_market_note":"jeonju_recipe_card";
            var hook=hue?"foodexchange_hue_u1":"foodexchange_jeonju_j15";var npc=hue?"npc_huong":"npc_soyeon";
            yield return CrossEnter(city);CrossCheckOpening();
            if(!resumeChapter)
            {
                Check(game.Save.node==game.Data.SpawnIndex,"First arrival uses the data-defined "+city+" spawn");
                yield return Snapshot("X-"+city+"-arrival");
                yield return Click("Interact");
                Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==DesignKitArt.Load().lumiPortrait,"Entry narration uses Lumi's dialogue portrait, not his map/exploration sprite");
                yield return Snapshot("X-"+city+"-Lumi-intro");yield return CloseEvent();
                yield return TestMapInput();
                if(ChaptersEnabled)yield return ChapterVisit(city);
            }
            else Check(game.World.Has(game.Content.City(city).completionFlag),"Restart resumes a genuinely earned chapter completion without resetting or teleporting to spawn");
            var count=game.Inventory.Count(item);var coins=game.Save.coins;
            Check(!game.Quests.IsCompleted(q)&&!game.Inventory.Has(reward),"Entering "+city+" grants no delivery progress or reward");
            yield return Travel(NodeIndex(id),true);
            Check(game.Inventory.Count(item)==count&&!game.Quests.IsCompleted(q)&&!game.Inventory.Has(reward),"Arriving and opening the NPC at "+id+" does not auto-deliver");
            var portrait=GameObject.Find("EventPortrait").GetComponent<Image>();
            Check(portrait.sprite==AdventureUIArt.Load().Npc(npc).sprite&&portrait.preserveAspect&&portrait.useSpriteMesh==AdventureUIArt.Load().Npc(npc).portraitUseSpriteMesh,"The "+id+" NPC shows its independent temporary portrait with the configured rendering and preserved aspect");
            CheckMarker(id,NodeMarkerType.NpcTalk,NodeMarkerEffect.Bounce);
            KeepSave(city+"-before");yield return Snapshot("X-"+city+"-NPC-before");
            yield return CrossBag("X-"+city+"-inventory-before",item);yield return Click("Interact");
            if(negative)
            {
                var real=JsonUtility.ToJson(game.Save);game.Inventory.TryConsume(item,count);game.Persist();
                yield return CloseEvent();yield return Click("Interact");
                Check(!game.Quests.CanComplete(q)&&!game.Events.Execute(NodeIndex(id),"complete:"+hook).changed&&game.Inventory.Count(item)==0&&!game.Inventory.Has(reward),"Missing exact "+item+" disables delivery and cannot consume/grant anything (isolated fixture)");
                Check(GameObject.Find("EventText").GetComponent<Text>().text.Contains("H10"),"Missing-item NPC gives a recovery direction to H10");
                if(city=="jeonju")
                {
                    game.Save.completed.Add("DQ05");game.World.Set("quest:DQ05");game.World.Set("jeonju.dalat_strawberry_shared");
                    Check(game.Events.BuildOptions(NodeIndex(id)).text.Contains("H10"),"Completed optional strawberry errand cannot hide active HQ05 recovery directions (isolated dialogue-policy fixture)");
                }
                CheckMarker(id,NodeMarkerType.NpcNeedItem,NodeMarkerEffect.Glow);yield return Snapshot("X-"+city+"-NPC-missing-fixture");
                File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return Click("Interact");
            }
            yield return FoodChoice("complete:"+hook);
            Check(FindButton("DeliveryConfirm")!=null&&game.Inventory.Count(item)==count&&!game.Quests.IsCompleted(q),"Opening delivery confirmation leaves inventory and quest untouched");
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==DesignKitArt.Load().lumiPortrait,"Delivery reply highlights the approved Lumi dialogue portrait");
            yield return Snapshot("X-"+city+"-Lumi-confirm");yield return Click("DeliveryCancel");
            Check(game.Inventory.Count(item)==count&&!game.Quests.IsCompleted(q)&&!game.Inventory.Has(reward),"Cancelling delivery retains the exact item quantity and grants no reward");
            if(negative)
            {
                var real=JsonUtility.ToJson(game.Save);yield return FoodChoice("complete:"+hook);game.Inventory.TryConsume(item,count);
                yield return Click("DeliveryConfirm");
                Check(!game.Quests.IsCompleted(q)&&!game.Inventory.Has(reward)&&game.Inventory.Count(item)==0,"Confirm revalidates a stale item count, rather than consuming or rewarding an invalid transaction (isolated fixture)");
                File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return Click("Interact");
            }
            yield return FoodChoice("complete:"+hook);yield return Click("DeliveryConfirm",true);
            Check(game.Quests.IsCompleted(q)&&game.Inventory.Count(item)==count-1&&game.Inventory.Count(reward)==1&&game.Save.coins==coins,"Confirmed "+id+" delivery consumes exactly one spice, grants the designed souvenir once and no extra currency");
            Check(GameObject.Find("ReceivedIcon_"+reward).GetComponent<Image>().sprite==AdventureUIArt.Load().Item(reward).sprite,"NPC receipt uses the existing shared souvenir icon");
            Check(!game.Events.Execute(NodeIndex(id),"complete:"+hook).changed&&game.Inventory.Count(reward)==1,"Replaying the completed delivery cannot consume more or duplicate its reward");
            CheckMarker(id,NodeMarkerType.Completed);
            yield return Snapshot("X-"+city+"-NPC-delivered");KeepSave(city+"-after");
            yield return CrossBag("X-"+city+"-inventory-after",reward);yield return CrossJournal("X-"+city+"-quest-journal");
        }

        private IEnumerator CrossCityJourney(bool resumeJeonju=false)
        {
            Check(game.Quests.IsCompleted("HQ01")&&game.Quests.IsCompleted("HQ02")&&game.Quests.IsActive("HQ05")&&game.Quests.IsActive("HQ06"),"Journey resumes the actual Hoi An quest/item checkpoint without resetting prior gameplay");
            if(!resumeJeonju)Check(!game.Quests.IsCompleted("HQ05")&&!game.Quests.IsCompleted("HQ06")&&!game.Inventory.Has("festival_friendship_postcard"),"Both deliveries and HQ07 reward are initially unclaimed");
            else Check(game.Quests.IsCompleted("JQ03")&&!game.Quests.IsCompleted("HQ05")&&!game.Inventory.Has("festival_friendship_postcard"),"Resume uses the exact earned Jeonju-before-delivery save; no unlock or quest flag is synthesized");
            var art=AdventureUIArt.Load();
            Check(art.Npc("npc_huong").sprite!=art.Npc("npc_soyeon").sprite&&new[]{"npc_huong","npc_soyeon"}.All(id=>art.Npc(id).status.Contains("NOT_CANONICAL")),"Hương and So-yeon have distinct temporary faces, explicitly not canonical/final");
            Check(art.Npc("npc_mai").sprite==DesignKitArt.Load().maiPortrait&&art.Npc("npc_guest").sprite.name=="npc_guest","Existing Mai and Ji-eun identities remain unchanged");
            if(!resumeJeonju){yield return CrossEnter("hoi_an");KeepSave("cross-start");yield return CrossRecoveryFixture();}
            var reverse=Environment.GetCommandLineArgs().Contains("--jeonju-first");
            foreach(var city in reverse?new[]{"jeonju","hue"}:new[]{"hue","jeonju"})
            {
                if(resumeJeonju&&game.Quests.IsCompleted(city=="hue"?"HQ06":"HQ05"))continue;
                yield return CrossDeliver(city,true,resumeJeonju&&city=="jeonju");
                if(FullStoryEnabled&&city=="hue")yield return DaLatJourney();
                if(FullStoryEnabled&&city=="jeonju")yield return CompleteStoryStrawberries();
                if(FullStoryEnabled&&reverse&&city=="jeonju")yield return GyeongjuJourney();
                Check(!game.Inventory.Has("festival_friendship_postcard")&&!game.Quests.IsActive("HQ07"),"A completed visit never auto-accepts HQ07 or pays its final reward");
                if(!game.Quests.IsCompleted("HQ05")||!game.Quests.IsCompleted("HQ06"))
                {
                    yield return CrossEnter("hoi_an");yield return Travel(10,true);
                    Check(!game.Quests.CanAccept("HQ07")&&!game.Events.BuildOptions(10).options.Any(o=>o.id=="accept:food_hq07"),"An actual return to Ba Tu after only one delivery still cannot start HQ07");
                    yield return Snapshot("X-HoiAn-only-one-delivery-HQ07-blocked");
                }
            }
            Check(game.Quests.IsCompleted("HQ05")&&game.Quests.IsCompleted("HQ06")&&game.World.Has("jeonju.chili_shared")&&game.World.Has("hue.shallot_shared"),"Both city orders produce the actual completed quests and world flags");
            yield return CrossEnter("hoi_an");
            Check(game.Discovery.Stage("hoi_an")==4&&game.Quests.IsCompleted("HA_FOOD_PREP")&&game.Quests.IsCompleted("HQ02"),"Returning to Hoi An preserves its discovery and local sidequests");
            if(ChaptersEnabled)yield return ChapterReturnLetters();
            yield return Travel(10,true);Check(game.Quests.CanAccept("HQ07"),"HQ07 becomes available at H10 only after both deliveries and the return journey");
            yield return Snapshot("X-HoiAn-HQ07-offer");yield return FoodChoice("accept:food_hq07");
            Check(game.Quests.IsActive("HQ07")&&!game.Quests.IsCompleted("HQ07")&&!game.Inventory.Has("festival_friendship_postcard"),"Accepting HQ07 records the follow-up without granting its reward");
            yield return FoodChoice("complete:food_hq07");yield return Click("DeliveryCancel");
            Check(!game.Quests.IsCompleted("HQ07")&&!game.Inventory.Has("festival_friendship_postcard"),"Cancelling the final conversation cannot grant the postcard");
            yield return FoodChoice("complete:food_hq07");yield return Click("DeliveryConfirm",true);
            Check(game.Quests.IsCompleted("HQ07")&&game.World.Has("hoi_an.festival_food_exchange_done")&&game.Inventory.Count("festival_friendship_postcard")==1,"Returning and explicitly confirming HQ07 awards the designed friendship postcard once");
            Check(!game.Events.Execute(10,"complete:food_hq07").changed&&game.Inventory.Count("festival_friendship_postcard")==1,"HQ07 replay cannot duplicate the final reward");
            yield return Snapshot("X-HoiAn-HQ07-reward");yield return CrossBag("X-HoiAn-final-inventory","festival_friendship_postcard");yield return CrossJournal("X-HoiAn-final-quest-journal");
            foreach(var city in new[]{"hue","jeonju"})
            {
                yield return CrossEnter(city);CrossCheckOpening();
                var node=city=="hue"?"U1":"J15";
                Check(game.Data.nodes[game.Save.node].id==node,"Revisiting "+city+" restores the last NPC location rather than resetting to spawn");
                CheckMarker(node,NodeMarkerType.Completed);yield return Travel(game.Save.node);yield return Snapshot("X-"+city+"-revisit-after-HQ07");
                Check(game.Inventory.Count("festival_friendship_postcard")==1&&game.Inventory.Count("jeonju_recipe_card")==1&&game.Inventory.Count("hue_market_note")==1,"Revisiting completed destinations preserves all unique rewards");
            }
            yield return CrossEnter("hoi_an");Check(game.Save.node==10,"Final return retains H10 and all pending local quests");KeepSave("hoi_an-after");
        }

        private IEnumerator VerifyCrossCityRestart()
        {
            var checkpoint=Path.Combine(output,CrossCheckpointName+".json");Check(File.Exists(checkpoint),"Cross-city checkpoint exists on disk: "+CrossCheckpointName);
            SaveSystem.OverridePath=checkpoint;game.Reload();var city=game.Save.cityId;var node=game.Save.node;
            var items=game.Save.items.ToArray();var completed=game.Save.completed.ToArray();var flags=game.Save.flags.ToArray();
            Check(game.Quests.IsCompleted("HQ01")&&game.Quests.IsCompleted("HQ02")&&game.Discovery.Stage("hoi_an")==4,"Separate process restart retains the original Hoi An chapter and sidequest progress");
            Check(game.World.Has("hue.shallot_shared")==game.Quests.IsCompleted("HQ06")&&game.World.Has("jeonju.chili_shared")==game.Quests.IsCompleted("HQ05"),"Separate process restart keeps each actual delivery consistent with its world flag");
            Check(game.Inventory.Count("festival_friendship_postcard")==(game.Quests.IsCompleted("HQ07")?1:0),"Separate process restart cannot grant HQ07 early or duplicate its postcard");
            yield return CrossEnter(city);
            Check(game.Save.node==node&&game.Save.items.SequenceEqual(items)&&game.Save.completed.SequenceEqual(completed)&&game.Save.flags.SequenceEqual(flags),"Restart and World Map re-entry preserve exact city/node/inventory/quests/flags: "+city);
            if(city!="hoi_an")CrossCheckOpening();
            if(ChaptersEnabled)CheckChapterSavedProgress();
            yield return Snapshot("X-Restart-"+CrossCheckpointName+"-map");
            yield return Travel(node,true);yield return Snapshot("X-Restart-"+CrossCheckpointName+"-NPC");yield return CloseEvent();
            yield return CrossJournal("X-Restart-"+CrossCheckpointName+"-journal");
            Check(game.Save.items.SequenceEqual(items)&&game.Save.completed.SequenceEqual(completed),"Restarted conversations and journal do not consume or award any item");
        }
    }
}
#endif
