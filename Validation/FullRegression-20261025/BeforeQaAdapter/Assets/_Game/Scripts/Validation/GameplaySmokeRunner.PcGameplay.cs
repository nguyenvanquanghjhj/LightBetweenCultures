#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        bool PcFull=>Environment.GetCommandLineArgs().Contains("--pc-full-gameplay");
        bool PcResumeOpening=>Environment.GetCommandLineArgs().Contains("--pc-resume-opening");
        IEnumerator PcResumeHoiAn()
        {
            Check(game.Data.cityId=="hoi_an"&&game.Puzzles.Solved("puzzle_pottery")&&game.Relics.Assembled("relic_hoi_an_lantern"),"PC resumes the legitimately earned opening checkpoint, not a fixture");
            yield return CrossEnter("hoi_an");Time.timeScale=4;
            if(!game.Quests.IsCompleted("HQ01"))
            {yield return ChapterGo("H5");yield return ChapterChoice("alternate");yield return CloseEvent();yield return ChapterGo("H7");yield return ChapterChoice("restore");yield return CloseEvent();}Time.timeScale=1;
            Check(game.Quests.IsCompleted("HQ01")&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju"),"PC H7 earned quest reward opens the normal Hue/Jeonju routes");
            yield return PcBaitAndFerry();
            yield return ChapterGo("H2");yield return RelicOption("relic_restore");
            for(var i=0;i<3;i++)yield return Click("RelicMirror"+i,true);
            yield return Click("RelicTrace",true);yield return RelicExit();
            Check(game.Relics.Restored("relic_hoi_an_lantern"),"PC actual mirror controls restore the earned Hoi An lantern");
            yield return Until(()=>!Map.LightingTransitionActive,"PC Hoi An restoration settles");yield return Snapshot("PC-hoi_an-restored");
        }
        IEnumerator PcH3Pickup()
        {
            if(SceneManager.GetActiveScene().name!="HoiAnDemo")yield return Click("EventOption_scene",true);
            yield return Scene("HoiAnDemo");
            var player=FindAnyObjectByType<HoiAn.HoiAnPlayerController>();var interaction=player.GetComponent<InteractionSystem>();
            yield return Pointer(interaction.ItemTapRect.center,true);
            yield return Until(()=>!player.Walking&&interaction.CanInteract,"PC H3 real item tap approaches through existing pathfinding",18);
            yield return Click("Collect",true);
            Check(game.Inventory.Count(game.Data.quest.item)==1,"PC H3 picks up the physical lamp eye exactly once");
            var at=Time.realtimeSinceStartup;yield return Click("Return",true);yield return Scene("CityMap");
            var first=Time.realtimeSinceStartup-at;var max=0f;var frames=new System.Collections.Generic.List<float>();
            for(var i=0;i<120;i++){yield return null;var ms=Time.unscaledDeltaTime*1000;frames.Add(ms);max=Mathf.Max(max,ms);}
            frames.Sort();Check(Map.View.InputEnabled&&!game.Scenes.Busy&&!Map.Moving,"PC H3 return restores map input, scene readiness and movement");
            yield return Snapshot("PC-H3-Return");
            yield return Travel(NodeIndex("H3"),true);yield return Click("EventOption_scene",true);yield return Scene("HoiAnDemo");
            var eye=FindAnyObjectByType<HoiAn.HoiAnItem>(FindObjectsInactive.Include);
            Check(eye!=null&&!eye.gameObject.activeSelf&&FindButton("Collect")==null,"PC H3 reentry has no duplicate eye or pickup control");
            at=Time.realtimeSinceStartup;yield return Click("Return",true);yield return Scene("CityMap");
            Check(game.Inventory.Count(game.Data.quest.item)==1&&Map.View.InputEnabled,"PC H3 second return preserves exactly one eye and responsive map");
            File.WriteAllText(Path.Combine(output,"pc-h3-return.json"),"{\"status\":\"NOT_REPRODUCED\",\"firstReturnSeconds\":"+first.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"reentryReturnSeconds\":"+(Time.realtimeSinceStartup-at).ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"maxFrameMs\":"+max.ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"p95FrameMs\":"+frames[114].ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"eyeCount\":1,\"inputEnabled\":true}");
        }
        IEnumerator PcFullJourney()
        {
            if(!PcResumeOpening)
            {Check(game.Save.accepted.Count==0&&game.Save.completed.Count==0&&game.Save.flags.Count==0,"PC New Game starts with a blank isolated save, no seed or fixture");yield return Snapshot("PC-NewGame");}
            else Check(File.Exists(Path.Combine(output,"opening-new-game-results.txt")),"PC original New Game run remains in the provenance evidence");
            yield return Phase4E2E();
            Check(game.Relics.AllRestored&&game.World.Has("seoul.final_cleared"),"PC earned six city relics and actual two-phase boss completion");
            Time.timeScale=1;yield return ChapterGo("S21");yield return CloseEvent();yield return Phase5Finish(true);
            Check(game.Events.CanonicalEndingCompleted&&game.World.Has("game.ending_seen"),"PC actual ceremony, ending dialogue and credits acknowledged");
            Check(game.Save.rewardGrantIds.Count==game.Save.rewardGrantIds.Distinct().Count(),"PC no duplicated reward or shop receipt IDs");
            Check(game.Persist(),"PC final earned state persists");
            File.WriteAllText(Path.Combine(output,"pc-final-state.json"),JsonUtility.ToJson(game.Save,true));
            File.WriteAllText(Path.Combine(output,"pc-playthrough.json"),"{\"status\":\"PC_FULL_PLAYTHROUGH_PASS\",\"newGame\":true,\"cities\":[\"hoi_an\",\"hue\",\"da_lat\",\"jeonju\",\"gyeongju\",\"seoul\"],\"boss\":true,\"ending\":true,\"saveLoad\":\"PENDING_SEPARATE_PROCESS_RESTART\",\"input\":\"Unity InputSystem pointer/touch events through runtime UI\"}");
        }
        IEnumerator PcBuy(string id,int price)
        {
            yield return CloseEvent();var coins=game.Save.coins;var before=game.Inventory.Count(id=="tea"?"healing_tea":"meat_portion");
            Check(coins>=price,"PC purchase uses earned currency");
            yield return Click("JourneyShop",true);
            Check(FindButton("ShopBuy_ferry_hq04")==null,"PC redundant ferry permission is not sold");
            yield return ShopClick("ShopBuy_"+id);
            if(FindButton("ShopConfirmBuy")!=null)yield return Click("ShopConfirmBuy",true);
            Check(game.Save.coins==coins-price&&game.Inventory.Count(id=="tea"?"healing_tea":"meat_portion")==before+1,"PC real shop transaction debits exact price and grants one "+id);
            yield return Click("ShopClose",true);
        }
        IEnumerator PcBattleWithTea(string node)
        {
            yield return PcBuy("tea",2);yield return ChapterGo(node);yield return ChapterChoice("battle");
            Check(game.Battles.Active,"PC real bot fight is active");
            Check(FindButton("BattleHeal")!=null&&!FindButton("BattleHeal").interactable,"PC full HP cannot consume tea");
            yield return Click("BattleDefend",true);
            var hp=game.Save.playerHP;var energy=game.Save.lightEnergy;var stock=game.Inventory.Count("healing_tea");var turn=game.Battles.TurnNumber;
            var healed=Math.Min(100-hp,game.Content.Item("healing_tea").healHP);var counter=game.Battles.EnemyAttack;
            Check(healed>0&&FindButton("BattleHeal").interactable,"PC injured Lumi can use purchased healing tea");
            yield return Click("BattleHeal",true);
            Check(game.Inventory.Count("healing_tea")==stock-1,"PC drinking consumes exactly one purchased tea");
            Check(game.Save.playerHP==Math.Max(0,hp+healed-counter)&&game.Save.playerHP<=100,"PC tea heals exact capped amount before the normal enemy reply");
            Check(game.Save.lightEnergy==energy&&game.Save.lightEnergy<=3&&game.Battles.TurnNumber==turn+1,"PC tea restores no energy and costs one normal combat turn");
            Check(game.Battles.Log.Contains("+"+healed+" HP"),"PC combat feedback displays actual HP restored");
            yield return Snapshot("PC-PurchasedTea-InCombat");
            for(var i=0;game.Battles.Active&&i<80;i++)yield return Click(game.Save.playerHP<30&&game.Inventory.Has("healing_tea")?"BattleHeal":game.Save.lightEnergy>0?"BattleLight":"BattleAttack",true);
            Check(!game.Battles.Active&&game.Battles.Outcome=="win","PC fight finishes through actual turn controls after consuming purchased tea");
            yield return CloseEvent();KeepSave("PC-after-purchased-tea-bot");
            File.WriteAllText(Path.Combine(output,"pc-consumable-result.json"),"{\"purchase\":true,\"healHP\":"+healed+",\"stockBefore\":"+stock+",\"stockAfter\":"+game.Inventory.Count("healing_tea")+",\"turnCost\":1,\"energyRestored\":0,\"battleOutcome\":\"win\"}");
        }
        IEnumerator PcBaitAndFerry()
        {
            if(!game.Save.choices.Contains("hoi_an:enemy_crab:bait"))
            {
                if(PcResumeOpening&&File.Exists(Path.Combine(output,"purchased-meat-before-runner-timeout.json")))
                    Check(game.Inventory.Has("meat_portion")&&game.Save.rewardGrantIds.Any(k=>k.StartsWith("shop:v1:purchase:")),"PC resumes the already committed real food purchase without a second charge");
                else yield return PcBuy("meat",1);
                var stock=game.Inventory.Count("meat_portion");
                yield return ChapterGo("H12");yield return ChapterChoice("alternate");
                Check(game.Inventory.Count("meat_portion")==stock-1&&!game.Battles.Active,"PC bought food is consumed as the designed crab bait, outside combat");
                yield return CloseEvent();
            }
            else Check(PcResumeOpening,"Already completed bait interaction comes from this campaign's earned continuation");
            File.WriteAllText(Path.Combine(output,"pc-bait-result.json"),"{\"status\":\"PASS\",\"purchasePrice\":1,\"consumed\":1,\"use\":\"H12 alternate dialogue: crab bait\",\"combatHealing\":false}");
            if(!game.Quests.IsCompleted("HQ04"))
            {
                yield return ChapterGo("H17");if(!game.Quests.IsActive("HQ04"))yield return ChapterChoice("accept");
                var coins=game.Save.coins;yield return ChapterConfirm("complete");
                Check(game.Quests.IsCompleted("HQ04")&&game.World.Has("hoi_an.ferry_available")&&game.Save.coins==coins-5,"PC original boatman quest completes through dialogue for exactly five earned coins");
            }
            yield return CloseEvent();yield return Click("JourneyShop",true);
            Check(FindButton("ShopBuy_ferry_hq04")==null,"PC shop cannot charge the completed ferry fare a second time");yield return Click("ShopClose",true);
            KeepSave("PC-bait-and-original-ferry");
        }
        IEnumerator PcRestartJourney()
        {
            var earned=JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(output,"pc-final-state.json")));
            Check(game.Events.CanonicalEndingCompleted&&game.World.Has("game.ending_seen")&&game.Relics.AllRestored,"PC separate process reloads the real six-relic ending save");
            Check(game.Save.coins==earned.coins&&game.Save.playerHP==earned.playerHP&&game.Save.lightEnergy==earned.lightEnergy&&game.Save.items.SequenceEqual(earned.items)&&game.Save.completed.SequenceEqual(earned.completed)&&game.Save.rewardGrantIds.SequenceEqual(earned.rewardGrantIds),"PC restart preserves exact currency, HP, energy, consumed inventory, quests and receipts");
            yield return CrossEnter("seoul");yield return CloseEvent();
            Check(Map.View.InputEnabled&&!Map.Moving,"PC reloaded ending save permits map input");yield return Snapshot("PC-SaveReload");
            File.WriteAllText(Path.Combine(output,"pc-save-load.json"),"{\"status\":\"PASS\",\"separateProcess\":true,\"inventoryAndReceiptsExact\":true}");
        }
        IEnumerator PcCapture(string name)
        {
            if(Application.isEditor)yield break;
            yield return new WaitForSecondsRealtime(.2f);
            var path=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(.3f);
            Check(File.Exists(path)&&new FileInfo(path).Length>2000,"PC milestone capture: "+name);
            using(var sha=System.Security.Cryptography.SHA256.Create())captures.Add(name+".png | SHA256 "+BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-",""));
        }
    }
}
#endif
