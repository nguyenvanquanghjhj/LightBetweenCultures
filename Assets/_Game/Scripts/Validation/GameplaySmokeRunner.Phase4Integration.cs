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
        string Phase4Argument(string prefix)=>Environment.GetCommandLineArgs().First(a=>a.StartsWith(prefix)).Substring(prefix.Length);
        string[] Phase4Order(bool korea)=>korea?new[]{"jeonju","gyeongju","hue","da_lat","seoul"}:new[]{"hue","da_lat","jeonju","gyeongju","seoul"};
        void Phase4Invariants()
        {
            Check(game.Content.cities.Length==6&&game.Content.cities.Sum(c=>c.nodes.Length)==133&&game.Content.quests.Length==29&&game.Content.puzzles.Length==22,"Six cities/133 nodes/29 quests/22 story puzzles unchanged");
            Check(game.Relics.Catalog.relics.Select(r=>r.id).Distinct().Count()==6&&game.Relics.Catalog.fragments.Select(f=>f.id).Distinct().Count()==24,"Single collection retains6 canonical relic IDs and24 fragment IDs");
        }
        IEnumerator Phase4E2E()
        {
            var route=Phase4Argument("--phase4-e2e=");var korea=route=="KOREA_FIRST";phase4PreferRepair=!korea;
            Phase4Invariants();
            if(PcResumeOpening){if(!game.Relics.Restored("relic_hoi_an_lantern"))yield return PcResumeHoiAn();}
            else yield return HoiAnLightingJourney();
            foreach(var city in Phase4Order(korea))
            {
                phase4CityOverride=city;
                var alreadyEarned=PcResumeOpening&&game.Relics.Restored(Phase4Relic.id);
                if(!alreadyEarned)yield return Phase4CityJourney();
                if(city=="gyeongju")Check(game.World.Has("gyeongju.stone_guard_friend")==!korea,"Two-route earned statue choice agrees with actual branch: "+route);
                if(!game.IsCityUnlocked("seoul"))
                {var before=JsonUtility.ToJson(game.Save);Check(!game.EnterCity("seoul")&&JsonUtility.ToJson(game.Save)==before,"Enabled relic UI/data cannot bypass locked Seoul city entry");}
                KeepSave("P4-E2E-"+route+"-"+city);
                if(PcFull&&!alreadyEarned)yield return Snapshot("PC-"+city+"-restored");
            }
            Check(game.Relics.AllRestored&&game.Relics.Catalog.fragments.All(f=>game.Relics.Owned(f.id))&&!game.World.Has("seoul.final_cleared")&&!game.World.Has("game.ending_seen"),"Earned "+route+" reaches24/24 and6/6 before boss, with no automatic ending");
            yield return ChapterGo("S19");yield return ChapterChoice("alternate");yield return ChapterGo("S20");yield return ChapterChoice("accept:sq03_offer");yield return CloseEvent();
            yield return StoryBattle("S20",!korea);
            Check(game.World.Has("seoul.final_cleared")&&!game.Quests.IsCompleted("SQ03")&&!game.World.Has("game.ending_seen")&&GameObject.Find("EndingPanel")==null,"Real two-phase boss battle/repair preserves independent S21/SQ03 and no auto credits");
            KeepSave("P4-E2E-"+route+"-after-boss");
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                yield return CrossEnter(city);Check(game.Relics.AllRestored&&Map.RelicCelebrations==0,"Post-boss re-entry preserves6/6 without repeat celebration: "+city);
                if(city=="hoi_an")Check(Map.HoiAnLightingState=="C","New runtime HoiAn still has full light after all other relics");
                else Check(Map.CulturalRestorationState=="C","Saved city-specific restoration remains C: "+city);
                yield return Snapshot("P4-E2E-"+route+"-revisit-"+city);
            }
            Check(!game.Battles.Start(game.Events.Definition("enemy_final_core")),"Resolved final boss cannot respawn after backtracking");
            Phase4Invariants();KeepSave("P4-E2E-"+route+"-final");
        }
        IEnumerator Phase4Retro()
        {
            var route=Phase4Argument("--phase4-retro=");var korea=route=="KOREA_FIRST";
            Check(game.World.Has("seoul.final_cleared")&&game.World.Has("game.ending_seen")&&game.Content.quests.All(q=>game.Quests.IsCompleted(q.id)),"Retro input is an earned historical post-boss29-quest save, not forced progression flags");
            Check(!game.Relics.Catalog.fragments.Any(f=>game.Relics.Owned(f.id)),"Historical earned save starts with missing relic fragments");
            var coins=game.Save.coins;var items=game.Save.items.ToArray();var quests=game.Save.completed.ToArray();var choices=game.Save.choices.ToArray();var oldLedger=game.Save.rewardGrantIds.ToArray();
            var world=game.Save.flags.Where(f=>!f.StartsWith("relic.")).ToArray();Phase4Invariants();
            foreach(var city in new[]{"hoi_an"}.Concat(Phase4Order(korea)))
            {
                phase4CityOverride=city;yield return CrossEnter(city);var r=Phase4Relic;
                foreach(var id in r.fragmentIds)
                {
                    var f=game.Relics.Fragment(id);var node=f.id=="fragment_jeonju_greeting_stamp"?r.restorationNode:f.nodeId;
                    yield return ChapterGo(node);Check(game.Relics.CanClaim(f),"Old real proof permits new gesture without replaying one-shot story: "+f.id);
                    yield return ClaimRelic(f.id,true);
                }
                yield return ChapterGo(r.restorationNode);yield return CloseEvent();
                if(city=="hoi_an"){yield return RelicAssembly(false);yield return Until(()=>!Map.LightingTransitionActive,"old-save HoiAn spread settles");}
                else {yield return Phase4Assembly();yield return RelicOption("relic_restore:"+r.id);yield return Phase4SolveTrial();yield return RelicExit();yield return Until(()=>!Map.CulturalRestorationAnimating,"old-save city spread settles");}
                Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(items)&&game.Save.completed.SequenceEqual(quests)&&game.Save.choices.SequenceEqual(choices),"Post-boss collection preserves old story items/rewards/quests/choices: "+city);
                Check(game.World.Has("game.ending_seen")&&game.World.Has("seoul.final_cleared")&&game.Save.flags.Where(f=>!f.StartsWith("relic.")).SequenceEqual(world),"Legacy boss/credits/world history remains exact after restoration: "+city);
                yield return Snapshot("P4-RETRO-"+route+"-"+city+"-C");KeepSave("P4-RETRO-"+route+"-"+city);
            }
            Check(game.Relics.AllRestored&&game.Save.rewardGrantIds.Count==oldLedger.Length+24&&oldLedger.All(game.Save.rewardGrantIds.Contains),"Old-save24 actual gestures add only24 independent collection claims");
            Check(game.Relics.Catalog.fragments.All(f=>game.Save.rewardGrantIds.Count(k=>k=="relic:v1:claim:"+f.id)==1),"Every new fragment ledger appears exactly once after old-save recovery");
            Check(GameObject.Find("EndingPanel")==null&&!game.Battles.Start(game.Events.Definition("enemy_final_core")),"Collecting missing relics after boss cannot replay credits or boss");
            KeepSave("P4-RETRO-"+route+"-final");
        }
        IEnumerator Phase4Key()
        {
            var mode=Phase4Argument("--phase4-key=");yield return CrossEnter("hoi_an");Time.timeScale=4;
            Check(!game.World.Has("hoi_an.mo01_disabled")&&game.Inventory.Count("link_key")==0&&!game.IsCityUnlocked("hue")&&!game.IsCityUnlocked("jeonju"),"Before H5: no key and both destinations locked");
            yield return ChapterPuzzle("H4");yield return ChapterPuzzle("H13");yield return ChapterPuzzle("H19");KeepSave("key-before-H5");
            if(mode=="battle")yield return ChapterBattle("H5");else {yield return ChapterGo("H5");yield return ChapterChoice("alternate");yield return CloseEvent();}
            var keyLedger="encounter:hoi_an:enemy_mo01:link_key";
            Check(game.Inventory.Count("link_key")==1&&game.Save.rewardGrantIds.Count(k=>k==keyLedger)==1&&!game.IsCityUnlocked("hue")&&!game.IsCityUnlocked("jeonju"),"H5 "+mode+" gives exactly one key without opening cities");
            Check(!new EncounterManager(game).Resolve(game.Events.Definition("enemy_mo01"),mode=="battle"?"battle":"hack")&&game.Inventory.Count("link_key")==1,"Resolved H5 cannot duplicate key by switching choice");
            KeepSave("key-after-H5");yield return Click("Inventory");yield return Click("Item_link_key",true);
            Check(GameObject.Find("SelectedItemDescription").GetComponent<Text>().text.Contains("H7"),"Inventory key describes actual H7 station use");yield return Snapshot("P4-Key-"+mode+"-before-H7-inventory");yield return Click("CloseInventory");
            yield return ChapterGo("H7");Check(game.Events.BuildOptions(game.Save.node).text.Contains("Chìa Kết Nối"),"H7 dialogue explains real key synchronization");
            var coins=game.Save.coins;yield return ChapterChoice("restore");yield return CloseEvent();Time.timeScale=1;
            Check(game.Inventory.Count("link_key")==0&&game.Save.rewardGrantIds.Count(k=>k==keyLedger)==1&&game.Save.coins==coins+150&&game.Quests.IsCompleted("HQ01")&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju"),"H7 consumes key once, awards150 once and opens both cities by original beacon");
            yield return Click("Interact");Check(game.Events.BuildOptions(game.Save.node).text.Contains("Chìa Kết Nối đã được dùng"),"Post-H7 dialogue records the key's completed role");yield return Snapshot("P4-Key-"+mode+"-H7-used");yield return CloseEvent();KeepSave("key-after-H7");
            yield return CrossEnter("hue");yield return CrossEnter("jeonju");Check(game.Inventory.Count("link_key")==0,"World Map enters Hue/Jeonju with no remaining key");KeepSave("key-final");
        }
        IEnumerator Phase4KeyRestart()
        {
            yield return CrossEnter("hoi_an");var h7=game.World.Has("hoi_an.lamp_restored");var h5=game.World.Has("hoi_an.mo01_disabled");var count=game.Inventory.Count("link_key");
            var before=JsonUtility.ToJson(game.Save);var coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();
            Check(game.IsCityUnlocked("hue")==h7&&game.IsCityUnlocked("jeonju")==h7,"Restart world unlock follows H7 only, independent of key presence");
            Check(h7||count==(h5?1:0),"Restart preserves earned before/after H5 key count");
            yield return Click("Inventory",true);
            if(count>0)
            {yield return Click("Item_link_key");var text=GameObject.Find("SelectedItemDescription").GetComponent<Text>().text;Check(h7?text.Contains("Đã sử dụng"):text.Contains("H7"),"Retained/pre-use key description derives real saved station state");}
            else Check(FindButton("Item_link_key")==null,"Consumed/missing key is not automatically re-granted to old save");
            Check(GameObject.Find("InventoryAction").GetComponentInChildren<Text>().text!="Dùng","No fake Use action for the journey key");yield return Snapshot("P4-Key-restart-"+(h7?"H7-used":h5?"H5-key":"before-H5")+"-count"+count);yield return Click("CloseInventory");
            Check(JsonUtility.ToJson(game.Save)==before&&game.Save.coins==coins&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Key UX/restart leaves exact save and reward history unchanged");
            if(h7){yield return CrossEnter("hue");yield return CrossEnter("jeonju");Check(game.Inventory.Count("link_key")==count,"Already opened destinations never consume or demand another key");}
            KeepSave("key-restart-final");
        }
        IEnumerator Phase4Presentation()
        {
            foreach(var city in new[]{"jeonju","hue","da_lat","gyeongju","seoul"})
            {
                yield return CrossEnter(city);Time.timeScale=4;
                var r=game.Relics.Catalog.relics.First(v=>v.cityId==city);
                yield return ChapterGo(r.restorationNode);yield return new WaitForSecondsRealtime(.45f);PortraitCheck();
                var text=game.Relics.Story("event:"+r.restorationNode);
                Check(GameObject.Find("EventText").GetComponent<Text>().text.Contains(text)&&!string.IsNullOrEmpty(text),"Actual dialogue displays city relic state: "+city);
                var dialogue=GameObject.Find("DialogueTextViewport").GetComponent<ScrollRect>();Canvas.ForceUpdateCanvases();dialogue.verticalNormalizedPosition=0;
                yield return Snapshot("P4-Presentation-"+city+"-NPC");yield return CloseEvent();
                var entry=game.Content.checkIns.First(c=>c.cityId==city);
                yield return ChapterGo(entry.nodeId);yield return OpenCheckInView(entry);Canvas.ForceUpdateCanvases();yield return null;
                var body=GameObject.Find("CheckInDescription").GetComponent<Text>();
                var caption=GameObject.Find("CheckInDescriptionViewport").GetComponent<ScrollRect>();
                Check(body.text.Contains(r.name)&&body.text.Contains(game.Relics.Restored(r.id)?"đã phục dựng":"chưa phục dựng"),"Postcard narration derives its own saved city relic state: "+city);
                Check(caption.content==body.rectTransform&&caption.viewport.rect.height==100&&body.rectTransform.rect.height>=body.preferredHeight-2,"Long reactive caption fits scroll content without overlapping postcard controls: "+city);
                var before=JsonUtility.ToJson(game.Save);caption.verticalNormalizedPosition=0;yield return null;
                yield return Snapshot("P4-Presentation-"+city+"-checkin-caption");yield return Click("CheckInClose",true);
                Check(JsonUtility.ToJson(game.Save)==before,"Viewing reactive postcard does not record or reward: "+city);
            }
            foreach(var pair in new[]{("gyeongju","G9","npc_keeper"),("gyeongju","G14","npc_inscription"),("seoul","S2","npc_curator"),("seoul","S11","npc_engineer")})
            {
                yield return CrossEnter(pair.Item1);yield return ChapterGo(pair.Item2);yield return new WaitForSecondsRealtime(.45f);PortraitCheck();
                var binding=AdventureUIArt.Load().Npc(pair.Item3);
                Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==binding.portrait&&binding.status.Contains("PENDING_APPROVAL"),"Existing imported portrait stays pending visual approval: "+pair.Item3);
                yield return Snapshot("P4-Presentation-"+pair.Item3);yield return CloseEvent();
            }
            yield return OpenPhotoJournal();yield return Click("PhotoCity_da_lat",true);
            var d0=game.Content.checkIns.First(c=>c.cityId=="da_lat"&&c.nodeId=="D0");var saved=JsonUtility.ToJson(game.Save);
            yield return PhotoCardClick(d0);
            Check(GameObject.Find("CheckInDescription").GetComponent<Text>().text.Contains("La Bàn Sương Lam đã phục dựng"),"Foreign album viewed from Seoul still derives Dalat relic narration");
            yield return Snapshot("P4-Presentation-Seoul-view-Dalat-postcard");yield return Click("PhotoBackToJournal",true);yield return Click("PhotoJournalClose");
            Check(JsonUtility.ToJson(game.Save)==saved,"Foreign album never changes story/relic/reward state");
            yield return TestMapInput();Phase4Invariants();KeepSave("phase4-presentation-final");Time.timeScale=1;
        }
    }
}
#endif
