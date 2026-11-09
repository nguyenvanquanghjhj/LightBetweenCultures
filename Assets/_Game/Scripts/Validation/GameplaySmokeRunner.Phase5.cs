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
        string p5Seed;
        IEnumerator Phase5Fixture(int restored,bool boss,bool legacy=false)
        {
            var save=JsonUtility.FromJson<SaveData>(p5Seed);save.cityId="seoul";
            save.flags.RemoveAll(f=>f==NodeEventManager.CanonicalEndingFlag||NodeEventManager.FinalRelicIds.Any(id=>f=="relic.restored."+id)||!legacy&&f=="game.ending_seen");
            for(var i=0;i<restored;i++)save.flags.Add("relic.restored."+NodeEventManager.FinalRelicIds[i]);
            if(!boss){save.flags.RemoveAll(f=>f=="seoul.final_cleared"||f=="seoul.system_repaired"||f=="seoul.system_stabilized");save.resolvedEvents.Remove("seoul:enemy_final_core");save.defeatedEnemies.Remove("enemy_final_core");}
            save.completed.Remove("SQ03");
            save.node=Array.FindIndex(game.Content.City("seoul").nodes,n=>n.id==(boss?"S21":"S20"));
            File.WriteAllText(savePath,JsonUtility.ToJson(save,true));game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);yield return new WaitForSecondsRealtime(.4f);
            Check(game.Events.FinalRestoredCount==restored&&!game.Events.CanonicalEndingCompleted,"Fixture has exact restored count without canonical completion: "+restored);
        }
        void Phase5Denied(string label)
        {
            foreach(var action in new[]{"ending","ending_next","ending_ack","ceremony_confirm","ceremony_finish","ceremony_skip"})
            {var result=game.Events.Execute(NodeIndex("S21"),action);Check(!result.openEnding&&!result.changed&&!game.Events.CanonicalEndingCompleted,label+": backend rejects "+action);}
        }
        IEnumerator Phase5Finish(bool full)
        {
            Screen.SetResolution(540,full?1200:960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);
            yield return CloseEvent();yield return Click("Interact",true);
            if(game.Quests.CanComplete("SQ03"))yield return ChapterChoice("report_victory");
            Check(game.Quests.IsCompleted("SQ03")&&!game.Events.CanonicalEndingCompleted,"S21 explicitly completes SQ03 independently before ceremony");
            var coins=game.Save.coins;var items=game.Save.items.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();
            yield return Snapshot("P5-S21-ready");yield return ChapterChoice("ceremony");yield return Snapshot("P5-confirmation");
            Check(!game.Events.CeremonyInProgress&&!game.Events.CanonicalEndingCompleted,"Confirmation popup alone has no ceremony/completion");
            yield return Click("FinalConfirm",true);
            Check(game.Events.CeremonyInProgress&&!game.Events.CanonicalEndingCompleted,"Only accepted explicit confirmation starts a transient ceremony");
            foreach(var control in new[]{"FinalSkip","RelicExit"})
            {var rect=(RectTransform)FindButton(control).transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);Check(corners.All(c=>c.x>=-1&&c.y>=-1&&c.x<=Screen.width+1&&c.y<=Screen.height+1),"Portrait controls remain in screen: "+Screen.width+"x"+Screen.height+" "+control);}
            Check(!game.Events.Execute(game.Save.node,"ceremony_finish").changed,"Immediate backend finish cannot skip the actual ceremony clock");
            if(full)
            {
                yield return new WaitForSecondsRealtime(1.1f);yield return Snapshot("P5-ceremony-HoiAn");
                yield return new WaitForSecondsRealtime(2.7f);yield return Snapshot("P5-ceremony-Hue");
                foreach(var frame in new[]{new{at=7.5f,name="DaLat"},new{at=10.5f,name="Jeonju"},new{at=13.5f,name="Gyeongju"},new{at=16.5f,name="Seoul"}})
                {yield return Until(()=>game.Events.CeremonyElapsed>=frame.at,"ceremony step "+frame.name,8);yield return Snapshot("P5-ceremony-"+frame.name);}
                yield return Until(()=>game.Events.CeremonyElapsed>=19,"six lights convergence",30);yield return Snapshot("P5-six-lights-converge");
                yield return new WaitForSecondsRealtime(1);yield return Snapshot("P5-Lumi-connection");
                yield return Until(()=>GameObject.Find("EpiloguePanel")!=null,"canonical ending after actual full ceremony",12);
            }
            else yield return Click("FinalSkip",true);
            Check(game.Events.CanonicalEndingCompleted&&!game.Events.CeremonyInProgress,"A valid ceremony or deliberate Skip commits the one canonical ending");
            Check(!game.Events.Execute(game.Save.node,"ceremony_skip").changed&&!game.Events.Execute(game.Save.node,"ceremony_finish").changed,"Repeated Finish/Skip cannot commit twice");
            Check(game.Save.flags.Count(f=>f==NodeEventManager.CanonicalEndingFlag)==1,"Exactly one global completion marker");
            yield return Snapshot("P5-ending-dialogue");
            for(var i=0;i<6;i++)yield return Click("EndingNext",i%2==0);
            Check(GameObject.Find("CreditsPanel")!=null,"Credits only follow the committed six-relic ceremony");yield return Snapshot("P5-credits");
            yield return Click("EndingNext",true);
            Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(items)&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Ceremony/credits never grant or replay item/coin/quest rewards");
            Check(game.World.Has("game.ending_seen"),"Acknowledged credits preserve compatible historical ending evidence");
            KeepSave("P5-ending-completed");yield return Snapshot("P5-post-credits");
        }
        IEnumerator Phase5EarnedBoss()
        {
            if(game.Save.playerHP<90&&game.Data.nodes[game.Save.node].id!="S20"){yield return ChapterGo("S0");yield return ChapterChoice("rest");yield return CloseEvent();}
            yield return ChapterGo("S20");yield return ChapterChoice("battle");yield return new WaitForSecondsRealtime(1.4f);
            Check(game.Battles.Active&&SoftAudio.Instance.MusicTrack==3,"Actual zero-relic Seoul boss starts existing Korea battle BGM");KeepSave("P5-boss-phase1");
            var phase2=false;
            for(var turn=0;game.Battles.Active&&turn<50;turn++)
            {
                if(game.Battles.Phase==2&&!phase2){phase2=true;KeepSave("P5-mid-boss");}
                if(game.Save.playerHP<35&&game.Inventory.Has("healing_tea")){yield return Click("BattleHeal",true);continue;}
                yield return Click(game.Battles.Phase==2?"BattleRepair":game.Battles.Energy>0?"BattleLight":"BattleAttack",true);
            }
            Check(phase2&&game.Battles.Outcome=="win"&&game.World.Has("seoul.system_repaired")&&!game.Save.defeatedEnemies.Contains("enemy_final_core"),"Earned zero-relic boss uses the actual two-phase repair pathway");
            yield return new WaitForSecondsRealtime(1.4f);Check(SoftAudio.Instance.MusicTrack==2,"Boss repair restores existing Korea map music before credits");yield return Click("BattleClose",true);
        }
        IEnumerator Phase5Journey()
        {
            musicVirtualFocus=true;SoftAudio.Ensure().SetMuted(false);p5Seed=JsonUtility.ToJson(game.Save);
            var args=Environment.GetCommandLineArgs();var restart=args.FirstOrDefault(a=>a.StartsWith("--phase5-restart="));
            if(restart!=null)
            {
                var mode=restart.Substring("--phase5-restart=".Length);var before=game.Save.items.ToArray();var coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();
                var completed=mode=="completed";
                Check(game.Events.CanonicalEndingCompleted==completed,"Process restart restores canonical completion only when previously committed: "+mode);
                Check(!game.Events.CeremonyInProgress&&GameObject.Find("CreditsPanel")==null&&GameObject.Find("FinalCeremonyPanel")==null,"Restart never resumes unconfirmed animation/credits automatically");
                if(mode=="pre-seoul")Check(!game.IsCityUnlocked("seoul"),"Pre-Seoul save retains its old world gate");
                else
                {
                    yield return CrossEnter("seoul");
                    if(mode=="relay"||mode=="mid-boss")Check(!game.Events.BossResolved,"Unfinished relay/boss save never becomes a victory");
                    else Check(game.Events.BossResolved&&!game.Battles.Start(game.Content.Event("seoul","enemy_final_core")),"Resolved boss remains resolved after real process restart");
                    if(mode=="ready"||mode=="interrupted"||mode=="legacy-six")Check(game.Events.FinalConnectionReady&&!game.Events.CanonicalEndingCompleted,"Ready/interrupted/old credits save still requires a new explicit ceremony");
                    if(mode=="missing"||mode=="legacy-zero")Check(!game.Events.FinalConnectionReady,"Missing relics cannot become ready by reloading legacy credits");
                }
                Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(before)&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Restart has no repeated reward or item loss");
                yield return Snapshot("P5-restart-"+mode);if(mode=="ready"||mode=="interrupted")yield return Phase5Finish(false);if(mode=="completed"){yield return Click("Back",true);yield return Scene("WorldMap");yield return Snapshot("P5-WorldMap-after-credits");}yield break;
            }
            if(args.Contains("--phase5-retro-claim"))
            {
                Check(game.Events.BossResolved&&game.World.Has("game.ending_seen")&&!game.Events.CanonicalEndingCompleted&&game.Events.FinalRestoredCount==0,"E12 real legacy earned source retains old credits/boss without new completion");
                var coins=game.Save.coins;var items=game.Save.items.ToArray();var quests=game.Save.completed.ToArray();var choices=game.Save.choices.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();
                yield return CrossEnter("hoi_an");var first=game.Relics.Catalog.fragments.First(f=>f.cityId=="hoi_an"&&f.nodeId=="H0");
                yield return ChapterGo("H0");Check(game.Relics.CanClaim(first),"Legacy proof enables a new independent fragment gesture");yield return ClaimRelic(first.id,true);
                yield return ChapterGo("H6");yield return Snapshot("P5-H6-post-boss");yield return CloseEvent();
                yield return CrossEnter("jeonju");yield return ChapterGo("J2");
                var greeting=game.Relics.Fragment("fragment_jeonju_greeting_stamp");Check(game.Relics.CanClaim(greeting),"E14 real legacy H6 check-in proof permits the cross-city J2 claim");
                yield return Snapshot("P5-legacy-NPC-before-claim");yield return ClaimRelic(greeting.id,true);yield return Snapshot("P5-legacy-cross-city-fragment");
                Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(items)&&game.Save.completed.SequenceEqual(quests)&&game.Save.choices.SequenceEqual(choices),"E12/E15 post-boss NPC/check-in/claim preserve all original items/coins/quests/choices");
                Check(game.Save.rewardGrantIds.Count==ledger.Length+2&&ledger.All(game.Save.rewardGrantIds.Contains),"Only two independent fragment claims extend the old ledger; no reward replay");
                Check(!game.Events.CanonicalEndingCompleted&&game.World.Has("game.ending_seen")&&game.Events.BossResolved,"Legacy history remains intact without automatic new ending");KeepSave("P5-legacy-claims-earned");yield break;
            }
            if(args.Contains("--phase5-tail"))
            {yield return CrossEnter("seoul");yield return ChapterGo("S21");yield return CloseEvent();Time.timeScale=1;yield return Phase5Finish(false);yield break;}
            if(args.Contains("--phase5-e2e"))
            {
                yield return Phase4E2E(); // Existing earned H0-to-boss journey, no fixture injections.
                Time.timeScale=1;yield return ChapterGo("S21");yield return CloseEvent();yield return Phase5Finish(false);
                Check(game.Relics.AllRestored&&game.Events.BossResolved&&game.Events.CanonicalEndingCompleted,"Earned full six-city route reaches the sole canonical ending");
                yield return CrossEnter("hoi_an");Check(Map.HoiAnLightingState=="C","Ending leaves approved full HoiAn restoration intact");yield return Snapshot("P5-earned-world-revisit");yield break;
            }
            if(args.Contains("--phase5-no-relics"))
            {
                Check(game.Events.FinalRestoredCount==0&&!game.Events.BossResolved,"Earned legacy before-boss source has zero relics and unresolved boss");
                yield return CrossEnter("seoul");yield return Phase5EarnedBoss();yield return ChapterGo("S21");
                yield return ChapterChoice("report_victory");Check(game.Quests.IsCompleted("SQ03")&&!game.Events.CanonicalEndingCompleted,"Actual two-phase repair with zero relics completes SQ03 only at S21");
                Phase5Denied("E13 earned no-relic story");KeepSave("P5-post-boss-zero-relics");
                foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"}){yield return CrossEnter(city);Check(game.Events.BossResolved&&!game.Events.CanonicalEndingCompleted,"Post-boss free travel without relic/ending gate: "+city);}
                yield break;
            }
            // Targeted matrix fixtures are explicitly distinct from the two fresh earned routes.
            foreach(var boss in new[]{false,true})foreach(var count in new[]{0,3,5,6})
            {
                yield return Phase5Fixture(count,boss);Phase5Denied("E01–E05 "+boss+"/"+count);
                Check(game.Events.FinalConnectionReady==(boss&&count==6),"Readiness is exactly boss AND six restored: "+boss+"/"+count);
                if(boss&&count<6)
                {
                    Check(game.Events.Execute(game.Save.node,"report_victory").changed&&game.Quests.IsCompleted("SQ03"),"SQ03 confirmation accepts boss with only "+count+" restored");
                    Check(!game.Events.Execute(game.Save.node,"report_victory").changed,"SQ03 confirmation is one-shot");
                    if(count==0)KeepSave("P5-boss-zero-fixture");
                    if(count==3){yield return Click("Interact",true);yield return Snapshot("P5-S21-three-relics");yield return ChapterChoice("final_collection");yield return Snapshot("P5-journal-three-relics");yield return RelicExit();}
                }
            }
            yield return Phase5Fixture(6,true,true);Check(game.World.Has("game.ending_seen")&&!game.Events.CanonicalEndingCompleted,"Legacy credits plus six relics never grandfather the new ceremony");KeepSave("P5-legacy-six");
            yield return Phase5Fixture(0,true,true);Check(game.World.Has("game.ending_seen")&&!game.Events.FinalConnectionReady,"Legacy credits with zero relics remain history only");KeepSave("P5-legacy-zero");
            yield return Phase5Fixture(6,true);KeepSave("P5-ready-unconfirmed");
            yield return Click("Interact",true);yield return ChapterChoice("ceremony");yield return RelicExit();
            Check(!game.Events.CeremonyInProgress&&!game.Events.CanonicalEndingCompleted,"Cancel confirmation is never Skip or completion");
            yield return Click("Interact",true);yield return ChapterChoice("ceremony");yield return Click("FinalConfirm",true);
            yield return new WaitForSecondsRealtime(2);
            var activeTime=game.Events.CeremonyElapsed;SoftAudio.Instance.SetSuspended(true);yield return new WaitForSecondsRealtime(1);
            Check(Mathf.Abs(game.Events.CeremonyElapsed-activeTime)<.15f&&!game.Events.CanonicalEndingCompleted,"Paused application does not advance ceremony or implicitly Skip");SoftAudio.Instance.SetSuspended(false);
            KeepSave("P5-ceremony-interrupted");yield return RelicExit();
            Check(!game.Events.CeremonyInProgress&&!game.Events.CanonicalEndingCompleted,"Ordinary close cancels an active ceremony without granting completion");
            // Real final restoration gesture on a bounded five-restored fixture.
            yield return Phase5Fixture(5,true);phase4CityOverride="seoul";yield return ChapterGo("S7");yield return CloseEvent();
            yield return RelicOption("relic_restore:relic_seoul_trirelay_prism");yield return Phase4SolveTrial();yield return RelicExit();
            Check(game.Events.FinalConnectionReady&&game.Events.BossResolved&&!game.Events.CanonicalEndingCompleted,"E04 real last trial makes S21 ready without respawning the resolved boss");
            yield return ChapterGo("S21");yield return CloseEvent();Time.timeScale=1;yield return Phase5Finish(true);
            yield return Click("Interact",true);yield return ChapterChoice("ending");var coinsAfter=game.Save.coins;yield return Click("EndingSkip",true);Check(game.Save.coins==coinsAfter,"Explicit replay is presentation-only");
            yield return CrossEnter("hoi_an");yield return Snapshot("P5-world-map-after-ending");yield return CrossEnter("seoul");
            Check(game.Events.CanonicalEndingCompleted&&GameObject.Find("CreditsPanel")==null,"Post-credits re-entry never auto-plays ending");yield return Snapshot("P5-ending-reentry");
        }
    }
}
#endif
