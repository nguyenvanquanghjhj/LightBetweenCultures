#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Extends the opt-in existing runner; production scene/manager topology unchanged.
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator DaLatOnly()
        {
            var args=Environment.GetCommandLineArgs();
            if(args.Contains("--dalat-restart"))
            {
                var name=args.FirstOrDefault(s=>s.StartsWith("--dalat-checkpoint="))?.Substring("--dalat-checkpoint=".Length)??"dalat-chapter-complete";
                var path=Path.Combine(output,name+".json");Check(File.Exists(path),"Earned Dalat restart checkpoint exists");
                SaveSystem.OverridePath=path;game.Reload();var json=JsonUtility.ToJson(game.Save);var city=game.Save.cityId;var node=game.Save.node;
                var expected=JsonUtility.FromJson<SaveData>(json);expected.previousNode=node;json=JsonUtility.ToJson(expected);
                yield return CrossEnter(city);
                Check(game.Save.node==node&&JsonUtility.ToJson(game.Save)==json,"Fresh process preserves city/node/quest/items/enemy/puzzle/discovery/choices; World Map entry resets previousNode as existing navigation policy");
                CheckChapterDiscovery();yield return Snapshot("DL-Restart-"+name+"-map");
                yield return Click("QuestJournal");yield return Snapshot("DL-Restart-"+name+"-journal");yield return Click("CloseJournal");
                Check(JsonUtility.ToJson(game.Save)==json,"Viewing restarted journal grants no reward or flags");yield break;
            }
            var seed=args.FirstOrDefault(s=>s.StartsWith("--dalat-seed="))?.Substring("--dalat-seed=".Length);
            Check(seed!=null&&File.Exists(seed),"Continuation uses actual previously earned HoiAn/Hue/Jeonju checkpoint, not synthetic unlock flags");
            File.Copy(seed,savePath,true);game.Reload();
            Check(game.Quests.IsCompleted("UQ01")&&game.Quests.IsCompleted("JQ03")&&game.Quests.IsCompleted("HQ07"),"Earned seed has completed both prior chapters and real food deliveries");
            yield return DaLatJourney();
        }
        IEnumerator DaLatJourney()
        {
            var previousItems=game.Save.items.ToArray();
            yield return CrossEnter("da_lat");Check(game.Save.node==0&&game.Save.items.SequenceEqual(previousItems),"First Dalat entry spawns at D0 and preserves the shared inventory");
            Time.timeScale=4;CheckChapterDiscovery();
            Check(game.Discovery.Stage("da_lat")==0&&!game.Discovery.IsVisible(NodeIndex("D8"))&&game.Discovery.DiscoveryState(NodeIndex("D8"))==MapDiscoveryState.PREVIEW,"Fresh Dalat has an explicit anonymous frontier, not the full network");
            Check(!game.Events.Execute(NodeIndex("D8"),"puzzle").changed&&game.Data.FindPath(0,NodeIndex("D8"),game.Save)==null,"Preview cannot execute an event or route");
            var freshNode=game.Save.node;Map.View.Focus(game.Data.nodes[NodeIndex("D8")].position);yield return new WaitForSecondsRealtime(.2f);
            Canvas.ForceUpdateCanvases();yield return null;
            var previewPoint=RectTransformUtility.WorldToScreenPoint(null,NodeMarker("D8").transform.position);
            var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
            UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=previewPoint},hits);
            File.WriteAllText(Path.Combine(output,"preview-raycast.txt"),"point="+previewPoint+" zoom="+Map.View.Zoom+" hits="+string.Join(",",hits.Select(h=>h.gameObject.name+" parent="+h.gameObject.transform.parent.name)));
            yield return Snapshot("DL-00a-preview-before-touch");yield return Pointer(previewPoint,true);
            Check(game.Save.node==freshNode&&!Map.Moving&&!Map.DialogueOpen,"Actual touch on anonymous preview cannot travel or interact");
            yield return Click("MapOverview");yield return Snapshot("DL-00-fresh-map-preview");yield return TestMapInput();
            yield return ChapterCheckIn("D0");
            yield return ChapterGo("D2");yield return ChapterChoice("accept");
            Check(!game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="collect:dalat_return_glasses"),"Lan cannot receive missing spectacles");
            yield return Snapshot("DL-01-Lan-quest");yield return ChapterPresentation("D7");
            yield return CloseEvent();Screen.SetResolution(540,1200,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);game.Scenes.Go("CityMap");yield return Scene("CityMap");
            var hud=GameObject.Find("HUD").GetComponent<UIController>();UIController.SetRect(hud.Root,new Vector2(.02f,.035f),new Vector2(.98f,.94f));Canvas.ForceUpdateCanvases();
            yield return Snapshot("DL-01b-20x9-safe-area-simulation");yield return TestMapInput();
            UIController.SetRect(hud.Root,Vector2.zero,Vector2.one);Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);game.Scenes.Go("CityMap");yield return Scene("CityMap");
            if(Environment.GetCommandLineArgs().Contains("--dalat-battle-thief"))yield return ChapterBattle("D7",true);
            else
            {
                yield return ChapterGo("D7");
                yield return ChapterChoice("reflect:straight");
                Check(!game.Events.Resolved(game.Events.Definition("enemy_glass_thief"))&&!game.Inventory.Has("lan_glasses"),"Wrong mirror direction never grants loot");
                yield return ChapterChoice("reflect:up");
                Check(!game.Inventory.Has("lan_glasses")&&!game.Events.Resolved(game.Events.Definition("enemy_glass_thief")),"First reflector is real saved preparation, not a free completed encounter");
                KeepSave("dalat-trap-step1");yield return Snapshot("DL-02-light-trap-first-step");
                yield return CloseEvent();game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return Click("Interact");
                yield return ChapterChoice("reflect:up");
                Check(!game.Events.Resolved(game.Events.Definition("enemy_glass_thief")),"Repeating first mirror direction after reload cannot bypass puzzle");
                yield return ChapterChoice("reflect:down");
                Check(game.Inventory.Count("lan_glasses")==1&&!game.Save.defeatedEnemies.Contains("enemy_glass_thief"),"Second actual reflector interaction recovers glasses without a battle, after save/reload");
            }
            yield return ChapterGo("D2");yield return ChapterChoice("collect:dalat_return_glasses");
            Check(game.Inventory.Has("lan_glasses")&&!game.World.Has("da_lat.glasses_returned"),"Delivery prompt does not consume spectacles before confirmation");
            yield return Click("DeliveryCancel");Check(game.Inventory.Has("lan_glasses"),"Cancel retains spectacles");
            yield return ChapterConfirm("collect:dalat_return_glasses");
            Check(!game.Inventory.Has("lan_glasses")&&game.Discovery.Stage("da_lat")==1&&!game.Quests.IsCompleted("DQ01"),"Actual handover opens stage1, while DQ01 still requires the mist puzzle");
            yield return CloseEvent();KeepSave("dalat-gate1");CheckChapterDiscovery();yield return Snapshot("DL-03-Lan-gate-reveal");
            yield return ChapterGo("D6");yield return ChapterChoice("alternate");
            Check(game.World.Has("da_lat.wolf_cleared")&&!game.Save.defeatedEnemies.Contains("enemy_mist_wolf"),"Lan's lamp clue resolves the optional wolf without combat");
            yield return ChapterPuzzle("D8");yield return ChapterGo("D8");yield return ChapterChoice("complete:dalat_dq01_finish");
            Check(game.Quests.IsCompleted("DQ01")&&game.Discovery.Stage("da_lat")==2,"Correct three-gate puzzle plus explicit quest completion clears fog");
            yield return CloseEvent();KeepSave("dalat-gate2");CheckChapterDiscovery();yield return Snapshot("DL-04-mist-gate-reveal");
            yield return ChapterGo("D10");yield return ChapterChoice("accept");
            Check(!game.Quests.CanComplete("DQ02")&&!game.Discovery.IsVisible(NodeIndex("D15")),"Hungry dog blocks the bridge region without three real food portions");yield return Snapshot("DL-05-dog-missing-food");
            yield return ChapterGo("D13");yield return ChapterChoice("accept");
            yield return ChapterGo("D19");yield return ChapterChoice("collect");
            yield return ChapterGo("D3");var food=game.Inventory.Count("meat_portion");yield return ChapterChoice("collect:dalat_thuy_food");
            Check(game.Inventory.Count("meat_portion")==food+1&&game.Inventory.Has("night_flower"),"One backup food is earned by showing the flower; mandatory gardener flower retained");
            Check(!game.Events.Execute(game.Save.node,"collect:dalat_thuy_food").changed,"Backup food cannot be farmed");
            foreach(var id in new[]{"D9","D11"}){yield return ChapterGo(id);yield return ChapterChoice("collect");}
            yield return ChapterPuzzle("D12");yield return CrossBag("DL-06-food-inventory", "meat_portion");
            // Isolated lost-food negative fixture, after all supplies were earned.
            // Restore it before the real dog hand-in; it is not E2E progression.
            var beforeRecovery=JsonUtility.ToJson(game.Save);yield return ChapterGo("D3");
            game.Save.flags.Remove("da_lat.windmill_supplies_found");game.Save.flags.Remove("da_lat.thuy_food_given");
            Check(game.World.Has("puzzle:puzzle_windmill"),"Legacy solved puzzle is recognized without requiring a newly authored flag or resetting save");
            game.Inventory.TryConsume("meat_portion",game.Inventory.Count("meat_portion")-2);game.Persist();yield return CloseEvent();yield return Click("Interact");var recoveryCoins=game.Save.coins;
            yield return ChapterChoice("recover:dalat_dog_food_recovery");
            Check(game.Inventory.Count("meat_portion")==3&&game.Save.coins==recoveryCoins&&!game.Quests.IsCompleted("DQ02"),"Lost-food fixture receives only the missing portion, capped at three, no coins or automatic dog completion");
            Check(!game.Events.Execute(game.Save.node,"recover:dalat_dog_food_recovery").changed,"Recovery cannot stockpile more than the three required food portions");yield return Snapshot("DL-food-recovery-isolated-fixture");yield return CloseEvent();
            File.WriteAllText(savePath,beforeRecovery);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==beforeRecovery,"Lost-food fixture restores exact legitimately earned state before main progression");
            yield return ChapterGo("D10");var portions=game.Inventory.Count("meat_portion");yield return ChapterConfirm("complete");
            Check(game.Inventory.Count("meat_portion")==portions-3&&game.Quests.IsCompleted("DQ02")&&game.Discovery.Stage("da_lat")==3,"Confirm feeds exactly three portions, completing the peaceful dog gate");
            Check(!game.Events.BuildOptions(NodeIndex("D3")).options.Any(o=>o.id=="recover:dalat_dog_food_recovery"),"Completed dog quest permanently closes its supply recovery option");
            yield return CloseEvent();KeepSave("dalat-gate3");CheckChapterDiscovery();yield return Snapshot("DL-07-dog-gate-reveal");
            // D10 -> D15 -> D20 -> D13 is the shortest route. The mandatory
            // intermediate boss must stop it. Choosing a safe street afterwards
            // is the player's decision, not an automatic skip after the modal.
            Map.View.Focus(game.Data.nodes[NodeIndex("D13")].position);yield return new WaitForSecondsRealtime(.2f);yield return Click("D13",true);
            yield return Until(()=>!Map.Moving,"mandatory intermediate fog sentinel");
            Check(game.Save.node==NodeIndex("D15")&&Map.DialogueOpen&&!game.World.Has("da_lat.mist_boss_cleared"),"Clicking beyond the bridge stops at mandatory D15 instead of skipping its encounter");
            yield return Snapshot("DL-07b-mandatory-intermediate-boss-stop");
            yield return ChapterGo("D12");yield return ChapterGo("D0");yield return ChapterGo("D20");
            yield return ChapterGo("D13");yield return ChapterConfirm("complete");
            Check(game.Quests.IsCompleted("DQ04")&&!game.Inventory.Has("night_flower"),"Gardener sidequest consumes its actual flower once");
            yield return ChapterGo("D4");var coins=game.Save.coins;var teas=game.Inventory.Count("healing_tea");yield return ChapterChoice("buy:healing_tea");
            Check(game.Save.coins==coins-8&&game.Inventory.Count("healing_tea")==teas+1,"Travelling merchant uses shared real purchase and icon binding");
            yield return ChapterGo("D14");Check(!game.Quests.CanComplete("DQ03")&&!game.World.Has("da_lat.beacon_lit"),"Station cannot activate before earned waterfall signal and accepted quest");
            yield return ChapterGo("D18");yield return ChapterChoice("rest");
            Check(game.Save.playerHP==100&&game.Save.lightEnergy==3,"Actual inn interaction uses shared rest service and restores HP/energy before the bridge");yield return Snapshot("DL-inn-rested");
            yield return ChapterGo("D15");yield return ChapterChoice("battle");
            Check(game.Battles.EnemyMaxHP==30&&game.Battles.EnemyAttack==6,"Helping Lan and dog weakens the canonical fog boss from 40HP/9ATK to 30HP/6ATK");
            yield return Click("BattleRun");Check(!game.World.Has("da_lat.mist_boss_cleared")&&!game.Inventory.Has("blue_water_signal"),"Retreat leaves the compulsory bridge encounter unresolved");yield return CloseEvent();
            yield return ChapterBattle("D15",true);
            Check(!game.Inventory.Has("blue_water_signal"),"Boss victory opens the route, but waterfall item must be collected at D16");
            yield return ChapterGo("D16");yield return ChapterChoice("collect");yield return ChapterChoice("accept:dalat_dq03_offer");
            Check(game.Inventory.Count("blue_water_signal")==1,"Actual waterfall interaction obtains the unique Seoul relay key");yield return CrossBag("DL-08-water-signal-inventory","blue_water_signal");
            yield return ChapterGo("D14");yield return ChapterChoice("restore");
            Check(game.Quests.IsCompleted("DQ03")&&game.Discovery.Stage("da_lat")==4&&game.World.Has("da_lat.beacon_lit")&&game.Inventory.Count("blue_water_signal")==1,"Explicit restore completes Dalat and keeps the unique signal for Seoul");
            yield return Snapshot("DL-09-station-restored");yield return ChapterCheckIn("D14");
            yield return ChapterGo("D17");yield return ChapterChoice("collect");
            yield return ChapterGo("D21");yield return ChapterChoice("portal");yield return Scene("WorldMap");
            Check(GameObject.Find("DestinationName").GetComponent<Text>().text=="Seoul"&&FindButton("EnterCity").interactable==game.IsCityUnlocked("seoul"),"Dalat portal follows the actual four-beacon condition, including a legitimately completed Korean-first branch");yield return Snapshot("DL-10-Seoul-other-branch-condition");
            yield return Click("Back");yield return Scene("CityMap");
            foreach(var n in game.Data.nodes){yield return Travel(NodeIndex(n.id),true);}
            CheckChapterDiscovery();Check(game.Save.visited.Distinct().Count()==22,"All22 Dalat nodes reached through actual map input");
            yield return DaLatTerrainEvidence();
            yield return CloseEvent();yield return Click("QuestJournal");yield return Snapshot("DL-11-complete-journal");yield return Click("CloseJournal");
            if(Environment.GetCommandLineArgs().Contains("--all-route-diagnostics"))yield return ChapterRouteDiagnostics();
            KeepSave("dalat-chapter-complete");
            yield return ChapterGo("D4");yield return ChapterChoice("accept:dalat_strawberry_offer");
            Check(game.Inventory.Count("da_lat_strawberry")==1&&!game.Quests.IsCompleted("DQ05"),"Optional specialty gift is obtained by actually accepting at D4, not on arrival");
            yield return CrossBag("DL-strawberry-gift-inventory","da_lat_strawberry");
            if(FullStoryEnabled&&!game.World.Has("jeonju.beacon_lit"))
            {KeepSave("dalat-final");Time.timeScale=1;yield break;}
            yield return CrossEnter("jeonju");yield return ChapterGo("J15");yield return ChapterChoice("complete:dalat_strawberry_return");
            Check(game.Inventory.Has("da_lat_strawberry"),"Strawberry gift waits for explicit delivery confirmation");
            yield return Click("DeliveryConfirm");Check(game.Quests.IsCompleted("DQ05")&&!game.Inventory.Has("da_lat_strawberry"),"Actual optional DaLat to Jeonju specialty gift completes once at J15");yield return Snapshot("DL-13-SoYeon-strawberry-delivery");
            var giftCoins=game.Save.coins;Check(!game.Events.Execute(game.Save.node,"complete:dalat_strawberry_return").changed&&game.Save.coins==giftCoins,"Specialty cannot farm rewards");
            yield return CrossEnter("da_lat");var earnedNode=game.Save.node;var earned=game.Save.items.ToArray();
            yield return CrossEnter("hoi_an");yield return CrossEnter("da_lat");
            Check(game.Save.node==earnedNode&&game.Save.items.SequenceEqual(earned)&&game.Discovery.Stage("da_lat")==4,"World Map roundtrip preserves Dalat exact node, items and discovery");
            yield return Snapshot("DL-12-return-to-completed-city");KeepSave("dalat-final");Time.timeScale=1;
        }
        IEnumerator DaLatTerrainEvidence()
        {
            yield return CloseEvent();Screen.SetResolution(1448,1086,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return Click("MapOverview");
            yield return Snapshot("DL-14-complete-map-landscape");
            var ui=GameObject.Find("HUD").GetComponent<UIController>();
            for(var x=100;x<1448;x+=100)for(var y=100;y<1086;y+=100)
            {
                var at=new Vector2(x/1448f,1-y/1086f);
                var plate=ui.Panel(Map.View.Content,"QA_Coord_"+x+"_"+y,at,at,new Color(0,0,0,.7f));plate.sizeDelta=new Vector2(50,16);plate.GetComponent<Image>().raycastTarget=false;
                var label=ui.Label(plate,"Coordinate",x+","+y,Vector2.zero,Vector2.one,10,false);label.raycastTarget=false;
            }
            yield return Snapshot("DL-15-DIAGNOSTIC-pixel-coordinate-grid");
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);game.Scenes.Go("CityMap");yield return Scene("CityMap");
        }
    }
}
#endif
