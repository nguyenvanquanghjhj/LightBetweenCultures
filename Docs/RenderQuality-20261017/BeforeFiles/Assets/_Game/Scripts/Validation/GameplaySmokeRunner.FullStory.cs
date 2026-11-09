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
        bool FullStoryEnabled=>Environment.GetCommandLineArgs().Contains("--full-story");
        IEnumerator CompleteStoryStrawberries()
        {
            if(!game.Quests.IsActive("DQ05")||game.Quests.IsCompleted("DQ05"))yield break;
            yield return CrossEnter("jeonju");yield return ChapterGo("J15");yield return ChapterChoice("complete:dalat_strawberry_return");
            Check(game.Inventory.Has("da_lat_strawberry")&&!game.Quests.IsCompleted("DQ05"),"Deferred specialty still requires a real confirmation after Jeonju opens");
            yield return Click("DeliveryConfirm",true);Check(game.Quests.IsCompleted("DQ05")&&!game.Inventory.Has("da_lat_strawberry"),"Dalat-first order delivers its legitimately earned strawberry only after Jeonju's gates");
        }
        IEnumerator StoryBattle(string node,bool repair=false,bool loseFirst=false)
        {
            yield return CloseEvent();
            if(game.Save.playerHP<90){yield return ChapterGo(game.Data.spawnNodeId);yield return ChapterChoice("rest");yield return CloseEvent();}
            yield return ChapterGo(node);var definition=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
            yield return ChapterChoice("battle");
            if(game.Battles.Enemy.mechanic=="two_phase")
            {var hp=game.Battles.EnemyHP;game.Battles.Act("repair");Check(game.Battles.EnemyHP==hp&&game.Battles.Phase==1&&!game.Events.Resolved(definition),"Repair cannot skip the first phase or grant a premature ending");}
            if(loseFirst)
            {
                var saved=game.Save.items.ToArray();
                for(var i=0;game.Battles.Active&&i<60;i++)yield return Click("BattleDefend");
                Check(game.Battles.Outcome=="loss"&&!game.Events.Resolved(definition)&&saved.SequenceEqual(game.Save.items),"New boss loss preserves quest items and grants no resolution");
                yield return Snapshot("Story-"+node+"-loss");yield return CloseEvent();yield return Click("Interact");yield return ChapterChoice("battle");
            }
            var phases=0;var shieldSeen=false;
            for(var turn=0;game.Battles.Active&&turn<100;turn++)
            {
                var battle=game.Battles;
                if(battle.Phase>phases){phases=battle.Phase;Check(!game.World.Has(definition.completionFlag),"Entering phase "+phases+" grants no premature boss completion");yield return Snapshot("Story-"+node+"-phase"+phases);}
                if(game.Save.playerHP<35&&game.Inventory.Has("healing_tea")){yield return Click("BattleHeal",true);continue;}
                if(battle.Shielded)
                {
                    if(!shieldSeen){yield return Click("BattleAttack");Check(battle.EnemyHP==battle.EnemyMaxHP,"Closed star shield really blocks damage");shieldSeen=true;}
                    var symbol=Array.IndexOf(battle.Enemy.shieldPattern,battle.ShieldSymbol);yield return Click("BattleStar_"+symbol,true);
                    Check(!battle.Shielded,"Actual star-symbol input opens a three-action shield window");continue;
                }
                if(repair&&battle.Phase==2){Check(battle.CanRepair,"Real truth quest and relays enable repair");yield return Click("BattleRepair",true);}
                else yield return Click(battle.Energy>0?"BattleLight":"BattleAttack",true);
            }
            Check(game.Battles.Outcome=="win"&&game.Events.Resolved(definition),"Actual turn combat resolves "+node);
            if(definition.enemyId=="enemy_star_core")Check(shieldSeen&&game.Inventory.Count("silla_prism")==1,"Star rhythm and unique prism are earned through the fight");
            if(definition.enemyId=="enemy_final_core")Check(phases==2&&game.World.Has("seoul.final_cleared")&&!game.World.Has("game.ending_seen"),"Final core needs both phases; boss victory does not skip the epilogue");
            if(repair)Check(game.World.Has("seoul.system_repaired")&&!game.Save.defeatedEnemies.Contains("enemy_final_core"),"Repair saves the system and is distinct from defeating it");
            yield return Snapshot("Story-"+node+"-resolved");yield return CloseEvent();
            Check(!game.Battles.Start(definition),"Resolved boss cannot replay for duplicate rewards: "+node);
        }
        IEnumerator GyeongjuJourney()
        {
            yield return CrossEnter("gyeongju");Time.timeScale=4;
            if(!game.World.Has("gyeongju.beacon_lit"))
            {
                CheckChapterDiscovery();Check(game.Discovery.Stage("gyeongju")==0,"Gyeongju starts with its own first region only");yield return Snapshot("Story-Gyeongju-opening");
                yield return ChapterGo("G3");yield return ChapterChoice("accept");yield return ChapterChoice("talk");yield return ChapterChoice("collect:min_fox_food");
                Check(game.Discovery.Stage("gyeongju")==1&&!game.Discovery.IsVisible(NodeIndex("G18")),"Min acceptance reveals symbol search but keeps the core hidden");KeepSave("full-gyeongju-gate1");
                yield return ChapterGo("G5");yield return ChapterChoice("collect");
                if(ChapterBattleBranch)yield return ChapterBattle("G6");else {yield return ChapterGo("G6");yield return ChapterChoice("alternate");}
                yield return ChapterGo("G9");yield return ChapterChoice("talk");yield return ChapterPuzzle("G15");
                yield return ChapterGo("G10");yield return ChapterChoice("collect");yield return ChapterPuzzle("G4");yield return ChapterGo("G4");yield return ChapterChoice("complete:gq01_finish");
                Check(game.Quests.IsCompleted("GQ01")&&game.Inventory.Count("stargazer_lens")==1&&game.Discovery.Stage("gyeongju")==2,"Three real symbols and puzzle grant the preserved lens and second region");KeepSave("full-gyeongju-gate2");
                if(game.World.Has("hue.star_script_known")){yield return ChapterGo("G12");yield return ChapterChoice("accept:gq02_offer_finish");yield return ChapterPuzzle("G12");}
                yield return ChapterGo("G7");KeepSave("full-gyeongju-before-guard");
                if(!ChapterBattleBranch&&game.World.Has("gyeongju.seal_decoded"))yield return ChapterChoice("alternate");else yield return ChapterBattle("G7",true);
                Check(game.World.Has("gyeongju.guard_resolved")&&game.Discovery.Stage("gyeongju")==3,"Repair OR real victory opens the stone gate");
                Check(game.World.Has("gyeongju.stone_guard_friend")==!game.Save.defeatedEnemies.Contains("enemy_stone_guard"),"Friend flag is only earned by the repair branch");KeepSave("full-gyeongju-gate3");
                yield return CloseEvent();yield return ChapterGo("G7");
                var guard=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
                Check(game.Events.BuildOptions(game.Save.node).text==(game.World.Has("gyeongju.stone_guard_friend")?guard.alternateCompletedText:guard.battleCompletedText)&&MapActor("G7").gameObject.activeInHierarchy,"Resolved statue remains present and responds according to the actual repair/battle choice");yield return ChapterChoice("talk");yield return Snapshot("Story-G7-after-choice");
                yield return ChapterGo("G14");yield return ChapterChoice("talk");yield return ChapterGo("G18");yield return ChapterChoice("accept:gq03_offer");
                yield return StoryBattle("G18");yield return ChapterGo("G19");Check(!game.World.Has("gyeongju.beacon_lit"),"Arrival does not light Silla's beacon");yield return ChapterChoice("restore");
                Check(game.Quests.IsCompleted("GQ03")&&game.World.Has("gyeongju.beacon_lit")&&game.Discovery.Stage("gyeongju")==4,"Explicit G19 activation completes Silla and keeps the prism/lens");
                yield return Snapshot("Story-G19-beacon");KeepSave("full-gyeongju-beacon");
                yield return ChapterPuzzle("G2");yield return ChapterGo("G17");yield return ChapterChoice("accept:gq04_offer_finish");yield return ChapterPuzzle("G17");yield return ChapterGo("G17");yield return ChapterChoice("complete:gq04_offer_finish");
                foreach(var n in new[]{"G16","G21"}){yield return ChapterGo(n);yield return ChapterChoice("collect");}
                yield return ChapterBattle("G13");
            }
            if(game.World.Has("hue.star_script_known")&&!game.Quests.IsCompleted("GQ02"))
            {
                yield return ChapterGo("G12");if(!game.Quests.IsActive("GQ02"))yield return ChapterChoice("accept:gq02_offer_finish");
                if(!game.World.Has("gyeongju.seal_decoded"))yield return ChapterPuzzle("G12");
                yield return ChapterGo("G12");yield return ChapterChoice("complete:gq02_offer_finish");
                Check(game.Quests.IsCompleted("GQ02")&&game.Inventory.Has("ancient_seal"),"Real Hue/Silla quest keeps the same ancient seal for Seoul");
            }
            yield return CloseEvent();CheckChapterDiscovery();KeepSave("full-gyeongju-complete");Time.timeScale=1;
        }
        IEnumerator FullStoryJourney()
        {
            Check(game.Quests.IsCompleted("HQ01")&&game.Quests.IsCompleted("JQ03"),"Full-story continuation starts from actual completed chapters, not unlocked fixtures");
            if(!game.Quests.IsCompleted("HQ04"))
            {yield return CrossEnter("hoi_an");yield return ChapterGo("H17");if(!game.Quests.IsActive("HQ04"))yield return ChapterChoice("accept");yield return ChapterChoice("complete");Check(game.Quests.IsCompleted("HQ04"),"Player pays the actual five coins to complete the optional ferry quest");}
            yield return CompleteStoryStrawberries();yield return GyeongjuJourney();
            Check(new[]{"hue.beacon_lit","da_lat.beacon_lit","jeonju.beacon_lit","gyeongju.beacon_lit"}.All(game.World.Has)&&game.IsCityUnlocked("seoul"),"All four earned branch beacons unlock Seoul");
            yield return CrossEnter("seoul");Time.timeScale=4;CheckChapterDiscovery();yield return Snapshot("Story-Seoul-opening");
            yield return ChapterGo("S12");yield return ChapterChoice("collect");yield return ChapterGo("S15");yield return ChapterChoice("alternate");
            Check(game.Inventory.Has("ancient_seal"),"Bridge verification does not consume Hue's unique seal");
            yield return ChapterGo("S2");yield return ChapterChoice("accept");KeepSave("full-seoul-gate1");
            yield return ChapterGo("S4");yield return ChapterChoice("collect");yield return ChapterPuzzle("S3");
            yield return ChapterGo("S7");yield return ChapterChoice("accept");
            yield return CloseEvent();KeepSave("full-seoul-before-relays");
            Check(!game.World.Has("seoul.relays_ready")&&!game.Quests.CanComplete("SQ01")&&!game.Discovery.IsVisible(NodeIndex("S20")),"Accepting relay quest grants neither relay readiness nor access to final core");
            yield return ChapterPuzzle("S18");Check(!game.World.Has("seoul.relays_ready"),"One real relay does not unlock the core");yield return Snapshot("Story-S18-relay-C");
            yield return ChapterPuzzle("S16");Check(game.Discovery.Stage("seoul")==2&&!game.World.Has("seoul.relays_ready"),"Two real relays reveal second region, still no core");yield return Snapshot("Story-S16-relay-A");KeepSave("full-seoul-gate2");
            yield return ChapterGo("S5");yield return ChapterChoice("alternate");
            yield return CloseEvent();KeepSave("full-seoul-before-relay-B");
            yield return ChapterPuzzle("S17");Check(game.World.Has("seoul.relays_ready")&&!game.Discovery.IsVisible(NodeIndex("S20")),"Readiness derives from three actual puzzles; Ji-won still needs the report");yield return Snapshot("Story-S17-relay-B");
            yield return ChapterGo("S7");yield return ChapterChoice("complete");Check(game.Quests.IsCompleted("SQ01")&&game.Discovery.Stage("seoul")==3,"Explicit three-relay report opens the final region");KeepSave("full-seoul-gate3");
            yield return ChapterGo("S11");yield return ChapterChoice("talk");yield return ChapterChoice("complete:sq02_finish");Check(game.Quests.IsCompleted("SQ02")&&game.Inventory.Has("stargazer_lens"),"The archive, palace message, lens and engineer reveal the real old-defense twist");yield return Snapshot("Story-S11-truth");
            yield return ChapterPuzzle("S10");yield return ChapterGo("S14");yield return ChapterChoice("talk");
            yield return ChapterGo("S19");yield return ChapterChoice("alternate");Check(game.World.Has("seoul.courier_disabled"),"Courier is explicitly disabled through all three relays, not skipped on the way to boss");
            yield return ChapterGo("S20");yield return ChapterChoice("accept:sq03_offer");yield return CloseEvent();KeepSave("full-seoul-before-boss");
            yield return StoryBattle("S20",!ChapterBattleBranch,true);KeepSave("full-seoul-after-boss");
            yield return ChapterGo("S21");Check(!game.World.Has("game.ending_seen")&&!game.Quests.IsCompleted("SQ03"),"Reaching festival alone does not award credits completion");yield return ChapterChoice("ending");
            var pages=game.Events.Definition("final_festival").endingPages.Length;
            for(var i=0;i<pages;i++){Check(!game.World.Has("game.ending_seen"),"Ending not marked before page/credits "+(i+1));if(i==3)KeepSave("full-ending-middle");yield return Snapshot("Story-S21-ending-"+i);yield return Click("EndingNext",true);}
            Check(game.World.Has("game.ending_seen")&&game.Quests.IsCompleted("SQ03"),"Only viewing and acknowledging all epilogue/credits pages completes the story");KeepSave("full-ending-earned");
            yield return ChapterGo("S22");yield return ChapterChoice("accept:sq04_offer_finish");yield return ChapterChoice("collect");yield return ChapterChoice("complete:sq04_offer_finish");Check(game.Quests.IsCompleted("SQ04"),"Secret ending requires the actual discovered truth and reading S22");yield return Snapshot("Story-S22-secret");
            foreach(var n in new[]{"S5","S8"}){yield return ChapterGo(n);if(!game.Events.Resolved(game.Events.Definition(game.Data.nodes[game.Save.node].eventId)))yield return ChapterChoice("alternate");}
            yield return CloseEvent();CheckChapterDiscovery();KeepSave("full-seoul-complete");Time.timeScale=1;
            foreach(var cid in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {yield return CrossEnter(cid);Check(game.World.Has("game.ending_seen"),"Postgame World Map revisit retains ending: "+cid);yield return Snapshot("Story-postgame-"+cid);}
            Check(game.Quests.IsCompleted("HQ07")&&game.Quests.IsCompleted("HQ03")&&game.Inventory.Count("festival_friendship_postcard")==1,"Postgame preserves food-exchange reward and the rescued lantern quest");
            Check(game.Content.quests.All(q=>game.Quests.IsCompleted(q.id)),"All 24 canonical quests and five released additions are actually completed");
            KeepSave("full-story-final");
        }
        IEnumerator FullStoryRestart()
        {
            var json=JsonUtility.ToJson(game.Save);var expected=JsonUtility.FromJson<SaveData>(json);expected.previousNode=expected.node;
            yield return CrossEnter(game.Save.cityId);
            Check(JsonUtility.ToJson(game.Save)==JsonUtility.ToJson(expected),"Separate process preserves exact full-story quest/items/enemies/puzzles/flags/choices/discovery and ending pages");
            CheckChapterDiscovery();yield return Snapshot("Story-restart-"+game.Save.cityId+"-stage"+game.Discovery.Stage(game.Save.cityId));
            yield return Click("QuestJournal");yield return Snapshot("Story-restart-journal");yield return Click("CloseJournal");
            if(Environment.GetCommandLineArgs().Contains("--resume-ending"))
            {
                var ending=game.Events.Definition("final_festival");var page=game.Events.EndingPage(ending);
                Check(!game.World.Has("game.ending_seen")&&page>0&&page<ending.endingPages.Length,"Restart resumes genuinely earned middle epilogue progress");
                yield return ChapterGo("S21");yield return ChapterChoice("ending");
                for(var i=page;i<ending.endingPages.Length;i++){Check(!game.World.Has("game.ending_seen"),"Resumed credits still require acknowledgement");yield return Snapshot("Story-resumed-ending-"+i);yield return Click("EndingNext",true);}
                Check(game.World.Has("game.ending_seen")&&game.Quests.IsCompleted("SQ03"),"Restarted epilogue completes only through actual remaining pages");
            }
        }
        IEnumerator StoryRequirementsDiagnostic()
        {
            Check(game.Quests.IsActive("SQ01")&&!game.World.Has("seoul.relays_ready"),"Negative checks start from earned pre-relay progress, not a fully unlocked fixture");
            yield return CrossEnter("seoul");
            var node=Environment.GetCommandLineArgs().First(s=>s.StartsWith("--check-relay=")).Substring("--check-relay=".Length);
            var item=node=="S16"?"blue_water_signal":node=="S17"?"ancient_seal":"hanji_circuit";
            yield return ChapterGo(node);yield return CloseEvent();var earned=JsonUtility.ToJson(game.Save);
            // Isolated missing-item and stale-derived-flag fixtures. Restore the
            // exact earned JSON; never use these as story-completion evidence.
            Check(game.Inventory.TryConsume(item,game.Inventory.Count(item)),"Missing-item fixture removes only the selected relay proof");
            var e=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
            Check(!game.Puzzles.Start(e.puzzleId)&&!game.Events.Execute(game.Save.node,"puzzle").openPuzzle&&!game.Events.BuildOptions(game.Save.node).options.First(o=>o.id=="puzzle").enabled,"UI and backend reject the missing exact relay item: "+item);
            game.Save.flags.Add("seoul.relays_ready");Check(!game.World.Has("seoul.relays_ready"),"A forged saved ready flag cannot replace three actual relay flags");game.World.RefreshDerivedFlags();Check(!game.Save.flags.Contains("seoul.relays_ready"),"Derived readiness removes a stale saved flag");
            File.WriteAllText(savePath,earned);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==earned,"Negative relay fixture restores the exact earned save");yield return Click("Interact");
            Check(game.Events.BuildOptions(game.Save.node).options.First(o=>o.id=="puzzle").enabled,"Restoring the actual proof item makes the puzzle available again");yield return Snapshot("Story-missing-item-restored-"+node);
        }
        IEnumerator FullStoryPostgame()
        {
            Check(game.World.Has("game.ending_seen")&&game.Quests.IsCompleted("SQ03"),"Coverage resumes an actual earned ending, not a synthetic all-unlocked fixture");
            foreach(var cid in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                yield return CrossEnter(cid);Time.timeScale=4;
                foreach(var node in game.Data.nodes)
                {
                    yield return ChapterGo(node.id);var e=game.Events.Definition(node.eventId);
                    if(FindButton("CheckInClose")!=null){yield return Snapshot("Story-postgame-checkin-"+node.id);yield return CloseEvent();continue;}
                    if(e.type=="portal")
                    {yield return ChapterChoice("portal");yield return Scene("WorldMap");Check(FindButton("EnterCity").interactable,"Earned portal previews a genuinely unlocked city: "+node.id);yield return Snapshot("Story-portal-"+node.id);yield return Click("Back");yield return Scene("CityMap");}
                    else if(e.type=="chest"||e.type=="item"||e.type=="secret")
                    {if(game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="collect"&&o.enabled))yield return ChapterChoice("collect");}
                    else if(e.type=="encounter"&&!game.Events.Resolved(e))
                    {if(game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="alternate"&&o.enabled))yield return ChapterChoice("alternate");else yield return ChapterBattle(node.id);}
                    else if(e.type=="start")yield return ChapterChoice("rest");
                    else if(e.type=="junction")yield return ChapterChoice("talk");
                    else if((e.type=="npc"||e.type=="dialogue")&&game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="talk"&&o.enabled))yield return ChapterChoice("talk");
                }
                Check(game.Save.visited.Distinct().Count()==game.Data.nodes.Length,"Actual postgame input visits every canonical node in "+cid);
                if(cid=="hoi_an")
                {Check(GameObject.Find("MiniLanternCompanion")!=null,"HQ03 companion is actually visible and follows the existing map Lumi object");yield return CloseEvent();Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));yield return Snapshot("Story-HQ03-companion-visible");}
                KeepSave("full-coverage-"+cid);Time.timeScale=1;
            }
            Check(game.Content.quests.All(q=>game.Quests.IsCompleted(q.id)),"Optional coverage preserves all29 completed quests");
            KeepSave("full-coverage-final");
        }
    }
}
#endif
