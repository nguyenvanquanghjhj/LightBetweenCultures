#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        string phase4CityOverride;
        bool phase4PreferRepair;
        string Phase4City=>phase4CityOverride??Environment.GetCommandLineArgs().First(a=>a.StartsWith("--phase4-city=")).Substring(14);
        CulturalRelic Phase4Relic=>game.Relics.Catalog.relics.First(r=>r.cityId==Phase4City);
        IEnumerator Phase4Frame(string name)
        {
            yield return CloseEvent();Screen.SetResolution(1280,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            Map.View.ResetView();Map.View.Focus(new Vector2(.5f,.5f));NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
            yield return Until(()=>!Map.CulturalRestorationAnimating,"cultural restoration settled");yield return Snapshot(name);
        }
        IEnumerator Phase4Scan(Vector2 p,bool useTouch)
        {
            var surface=(RectTransform)GameObject.Find("TrialSurveySurface").transform;Canvas.ForceUpdateCanvases();
            var end=RectTransformUtility.WorldToScreenPoint(null,surface.TransformPoint(new Vector2(surface.rect.xMin+p.x*surface.rect.width,surface.rect.yMin+p.y*surface.rect.height)));
            var start=RectTransformUtility.WorldToScreenPoint(null,surface.TransformPoint(new Vector2(surface.rect.xMin+.5f*surface.rect.width,surface.rect.yMin+.5f*surface.rect.height)));
            if(useTouch)InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=start});
            else{InputSystem.QueueStateEvent(mouse,new MouseState{position=start});yield return null;InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));}
            yield return null;yield return null;
            for(var k=1;k<=18;k++){var q=Vector2.Lerp(start,end,k/18f);if(useTouch)InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=q});else InputSystem.QueueStateEvent(mouse,new MouseState{position=q}.WithButton(MouseButton.Left));yield return null;}
            if(useTouch)InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=end});else InputSystem.QueueStateEvent(mouse,new MouseState{position=end});yield return null;yield return null;
        }
        IEnumerator Phase4SolveTrial()
        {
            var r=Phase4Relic;var a=game.Relics.Active;
            Check(a!=null&&a.TrialType==r.trialType,"Actual city restoration uses its distinct tactile rule: "+r.trialType);
            yield return Click("TrialSubmit",true);Check(!game.Relics.Restored(r.id),"Wrong trial submission cannot restore "+r.id);
            yield return Snapshot("P4-"+Phase4City+"-trial-wrong");yield return Click("TrialHint");yield return Click("TrialRetry",true);
            Check(game.Relics.Assembled(r.id)&&game.Relics.Count(r)==4&&!a.TrialSucceeded,"Hint/retry preserve earned assembly and four fragments without solving");
            if(a.TrialType=="hanji")for(var i=0;i<4;i++)while(a.tileTurns[i]!=r.trialTurns[i])yield return Click("TrialTurn"+i,i%2==0);
            else if(a.TrialType=="prism")
            {for(var i=0;i<3;i++){yield return RelicDrag(i,i%2==0);while(a.tileTurns[i]!=r.trialTurns[i])yield return Click("TrialTurn"+i,i%2==0);}}
            else
            {
                for(var i=0;i<4;i++)yield return Phase4Scan(RelicActivity.SurveyPoints[i],i%2==0);
                if(a.TrialType=="lens")
                {yield return Click("StarRead3",true);Check(a.StarSequence==0&&a.WrongReadings==1,"Decoy reading is rejected; retry remains possible");foreach(var i in RelicActivity.StarOrder)yield return Click("StarRead"+i,true);}
                var target=a.TrialType=="mist"?1:a.TrialType=="seal"?2:3;
                while(a.Dial!=target)yield return Click("TrialDial",true);
            }
            Check(a.TrialSucceeded,"Real mouse/touch controls satisfy "+r.trialType+" trial");yield return Snapshot("P4-"+Phase4City+"-trial-correct-before-confirm");
            Check(!game.Relics.Restored(r.id),"Correct arrangement alone cannot auto-confirm restoration");yield return Click("TrialSubmit",true);
            Check(game.Relics.Restored(r.id)&&game.World.Has("relic.trial."+r.id)&&Map.RelicCelebrations==1,"Explicit trial confirmation commits restored state once: "+r.id);
        }
        IEnumerator Phase4Assembly()
        {
            var r=Phase4Relic;yield return RelicOption("relic_restore:"+r.id);
            if(!game.Relics.Assembled(r.id))
            {
                yield return Click("RelicAssemble");Check(!game.Relics.Assembled(r.id),"Empty slots do not assemble "+r.id);
                for(var i=0;i<4;i++)yield return RelicDrag(i,i%2==0);
                yield return Snapshot("P4-"+Phase4City+"-assembly");yield return Click("RelicAssemble",true);
            }
            Check(game.Relics.Assembled(r.id)&&!game.Relics.Restored(r.id),"Assembly is distinct from restored state");yield return RelicExit();KeepSave("phase4-"+Phase4City+"-assembled");
        }
        IEnumerator Phase4CityJourney()
        {
            var r=Phase4Relic;Check(r.enabled,"City relic enabled in shared registry: "+Phase4City);
            if(Environment.GetCommandLineArgs().Contains("--phase4-restore-only")){yield return CrossEnter(Phase4City);}
            else if(Environment.GetCommandLineArgs().Contains("--phase4-guard-continuation"))
            {
                yield return CrossEnter("gyeongju");Time.timeScale=4;
                yield return ChapterGo("G12");yield return ChapterChoice("accept:gq02_offer_finish");yield return ChapterPuzzle("G12");
                yield return ChapterGo("G7");yield return ChapterChoice("alternate");yield return CloseEvent();
                Check(game.World.Has("gyeongju.guard_resolved")&&game.World.Has("gyeongju.stone_guard_friend")&&!game.Save.defeatedEnemies.Contains("enemy_stone_guard"),"Earned pre-guard checkpoint resolves statue by repair, not fake battle proof");
                yield return ClaimRelic(r.fragmentIds[2],true);
            }
            else if(Phase4City=="jeonju")
            {
                yield return CrossEnter("hoi_an");
                if(!game.World.Has("checkin.hoi_an.h6")){yield return ChapterGo("H6");yield return RelicPhoto(false);}
                yield return CrossEnter("jeonju");Time.timeScale=4;
                yield return ChapterGo("J2");
                Check(game.Relics.BeginAssembly(r.id)==null,"Restoration is blocked before four earned fragments");
                if(game.Quests.CanAccept("JQ01"))yield return ChapterChoice("accept");
                foreach(var id in new[]{r.fragmentIds[0],r.fragmentIds[3]})yield return ClaimRelic(id,true);
                yield return ChapterPuzzle("J3");yield return ClaimRelic(r.fragmentIds[1],false);
                yield return ChapterPuzzle("J7");yield return ClaimRelic(r.fragmentIds[2],true);
            }
            else {yield return Phase4PrepareCityAccess();yield return CrossEnter(Phase4City);yield return Phase4EarnCitySources();}
            Check(game.Relics.Count(r)==4&&r.fragmentIds.All(id=>game.Save.rewardGrantIds.Count(k=>k=="relic:v1:claim:"+id)==1),"Four gestures give four unique collection claims, not story rewards");
            yield return ChapterGo(r.restorationNode);yield return CloseEvent();yield return Phase4Assembly();
            yield return Phase4Frame("P4-"+Phase4City+"-A-before-restored");KeepSave("phase4-"+Phase4City+"-before-trial");
            var coins=game.Save.coins;var items=game.Save.items.ToArray();var quests=game.Save.completed.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();var endingSeen=game.World.Has("game.ending_seen");var worldFlags=game.Save.flags.Where(f=>!f.StartsWith("relic.")).ToArray();
            yield return RelicOption("relic_restore:"+r.id);yield return Phase4SolveTrial();yield return RelicExit();yield return Snapshot("P4-"+Phase4City+"-celebration");
            yield return Phase4Frame("P4-"+Phase4City+"-C-restored");
            Check(Map.CulturalRestorationState=="C"&&game.Save.coins==coins&&game.Save.items.SequenceEqual(items)&&game.Save.completed.SequenceEqual(quests)&&game.Save.rewardGrantIds.SequenceEqual(ledger),"World restoration preserves main quests, inventory, coins and reward ledger");
            var layer=GameObject.Find("CulturalRestorationLayer");Check(layer!=null&&!layer.GetComponent<CanvasGroup>().blocksRaycasts,"Actual map restoration layer is present and cannot block touch");
            yield return ChapterGo(r.restorationNode);Check(game.Events.BuildOptions(game.Save.node).text.Contains("phục dựng")||game.Events.BuildOptions(game.Save.node).text.Contains("Phục dựng"),"NPC/runtime side dialogue recognizes actual restoration");yield return CloseEvent();
            yield return Click("QuestJournal");yield return Click("OpenRelicCollection");yield return Snapshot("P4-"+Phase4City+"-collection");
            yield return Click("RelicCard_"+r.id,true);Check(r.fragmentIds.All(id=>GameObject.Find("FragmentCard_"+id)!=null),"Existing collection opens all four fragment details without travel or grants");yield return Snapshot("P4-"+Phase4City+"-four-fragments");yield return Click("RelicCollectionBack");yield return RelicExit();
            Check(game.Save.version==2&&game.World.Has("game.ending_seen")==endingSeen&&game.Save.flags.Where(f=>!f.StartsWith("relic.")).SequenceEqual(worldFlags)&&GameObject.Find("EndingPanel")==null,"No schema upgrade, boss/ending/world progress change from relic restoration");
            if(Phase4City=="seoul")Check(game.Relics.AllRestored&&game.Relics.Catalog.fragments.All(f=>game.Relics.Owned(f.id)),"Earned chain now has24/24 fragments and6/6 restored without automatic credits");
            KeepSave("phase4-"+Phase4City+"-final");Time.timeScale=1;
        }
        IEnumerator Phase4EarnCitySources()
        {
            var r=Phase4Relic;Time.timeScale=4;
            if(Phase4City=="hue")
            {
                yield return ChapterGo("U3");yield return ChapterChoice("accept");
                Check(!game.Relics.CanClaim(game.Relics.Fragment(r.fragmentIds[0])),"U3 jade unavailable before actual crossing permit");
                yield return ChapterPuzzle("U16");yield return ClaimRelic(r.fragmentIds[2],false);
                if(PcFull)yield return PcBattleWithTea("U15");
                else {yield return ChapterGo("U15");yield return ChapterChoice("alternate");yield return CloseEvent();}
                yield return ChapterGo("U3");yield return ChapterConfirm("complete");yield return CloseEvent();yield return ClaimRelic(r.fragmentIds[0],true);
                yield return ChapterGo("U7");yield return ChapterChoice("accept");
                yield return ChapterPuzzle("U17");yield return ClaimRelic(r.fragmentIds[3],false);
                yield return ChapterPuzzle("U8");yield return ClaimRelic(r.fragmentIds[1],true);
                Check(!game.World.Has("hue.beacon_lit")&&!game.IsCityUnlocked("da_lat"),"Hue relic sources do not auto-activate U19 or open Dalat");
            }
            else if(Phase4City=="da_lat")
            {
                yield return ChapterCheckIn("D0");yield return ClaimRelic(r.fragmentIds[0],true);
                yield return ChapterGo("D2");yield return ChapterChoice("accept");
                Check(!game.Relics.CanClaim(game.Relics.Fragment(r.fragmentIds[1])),"Pine evidence waits for actual glasses return");
                yield return ChapterGo("D7");yield return ChapterChoice("prepare");yield return ChapterChoice("prepare");
                yield return ChapterGo("D2");yield return ChapterConfirm("collect:dalat_return_glasses");yield return CloseEvent();yield return ClaimRelic(r.fragmentIds[1],false);
                yield return ChapterPuzzle("D8");yield return ClaimRelic(r.fragmentIds[2],true);
                yield return ChapterGo("D8");yield return ChapterChoice("complete:dalat_dq01_finish");
                yield return ChapterPuzzle("D12");yield return ClaimRelic(r.fragmentIds[3],false);
                Check(!game.World.Has("da_lat.beacon_lit")&&!game.IsCityUnlocked("seoul"),"Windmill fragment belongs to Dalat and does not require/open Seoul");
            }
            else if(Phase4City=="gyeongju")
            {
                yield return ChapterGo("G3");yield return ChapterChoice("accept");yield return ChapterChoice("talk");yield return ChapterChoice("collect:min_fox_food");yield return ClaimRelic(r.fragmentIds[0],true);
                yield return ChapterGo("G5");yield return ChapterChoice("collect");yield return ChapterGo("G6");yield return ChapterChoice("alternate");
                yield return ChapterGo("G9");yield return ChapterChoice("talk");yield return ChapterPuzzle("G15");yield return ClaimRelic(r.fragmentIds[3],false);
                yield return ChapterGo("G10");yield return ChapterChoice("collect");yield return ChapterPuzzle("G4");yield return ClaimRelic(r.fragmentIds[1],true);
                yield return ChapterGo("G4");yield return ChapterChoice("complete:gq01_finish");yield return CloseEvent();KeepSave("phase4-gyeongju-before-guard");
                if(phase4PreferRepair||Environment.GetCommandLineArgs().Contains("--phase4-repair-guard"))
                {yield return ChapterGo("G12");yield return ChapterChoice("accept:gq02_offer_finish");yield return ChapterPuzzle("G12");yield return ChapterGo("G7");yield return ChapterChoice("alternate");yield return CloseEvent();}
                else yield return ChapterBattle("G7");
                Check(game.World.Has("gyeongju.guard_resolved"),"Actual battle/repair resolves statue before reflection evidence");yield return ClaimRelic(r.fragmentIds[2],false);
                Check(!game.World.Has("gyeongju.beacon_lit"),"Statue reflection cannot auto-activate Silla beacon");
            }
            else if(Phase4City=="seoul")
            {
                yield return ChapterGo("S12");yield return ChapterChoice("collect");yield return ChapterGo("S15");yield return ChapterChoice("alternate");
                yield return ChapterGo("S2");yield return ChapterChoice("accept");yield return ChapterGo("S4");yield return ChapterChoice("collect");
                yield return ChapterGo("S2");yield return ClaimRelic(r.fragmentIds[0],true);yield return ChapterPuzzle("S3");
                yield return ChapterGo("S7");yield return ChapterChoice("accept");
                Check(!game.Relics.CanClaim(game.Relics.Fragment(r.fragmentIds[3]))&&!game.World.Has("seoul.relays_ready"),"Relay frame unavailable before actual Hanji relay puzzle");
                yield return ChapterPuzzle("S18");yield return ClaimRelic(r.fragmentIds[3],false);
                yield return ChapterPuzzle("S16");yield return ChapterGo("S5");yield return ChapterChoice("alternate");yield return ChapterPuzzle("S17");
                Check(game.World.Has("seoul.relays_ready")&&!game.Relics.CanClaim(game.Relics.Fragment(r.fragmentIds[1])),"Three real relays still require Ji-won's explicit report");
                yield return ChapterGo("S7");yield return ChapterChoice("complete");yield return CloseEvent();yield return ClaimRelic(r.fragmentIds[1],true);
                yield return ChapterGo("S11");yield return ChapterChoice("talk");yield return ChapterChoice("complete:sq02_finish");
                yield return ChapterPuzzle("S10");yield return ClaimRelic(r.fragmentIds[2],false);
                Check(!game.World.Has("seoul.final_cleared")&&!game.Quests.IsCompleted("SQ03"),"Seoul relic sources do not resolve boss or SQ03");
            }
            else Check(false,"City source journey not yet implemented; do not fake source proof");
        }
        IEnumerator Phase4PrepareCityAccess()
        {
            if(game.IsCityUnlocked(Phase4City))yield break;
            if(Phase4City=="da_lat")
            {
                yield return CrossEnter("hue");Time.timeScale=4;
                Check(game.Relics.Restored("relic_hue_dragon_seal")&&!game.IsCityUnlocked("da_lat"),"Hue relic restoration alone did not open Dalat");
                yield return ChapterGo("U11");yield return ChapterChoice("collect");yield return ChapterChoice("accept:chapter_uq04_offer");
                yield return ChapterGo("U7");yield return ChapterChoice("complete:chapter_uq04_return");
                yield return ChapterGo("U5");yield return ChapterChoice("alternate");
                yield return ChapterGo("U19");yield return ChapterChoice("restore");yield return CloseEvent();
                Check(game.World.Has("hue.beacon_lit")&&game.IsCityUnlocked("da_lat"),"Actual U19 station action opens Dalat by unchanged World Map rule");
            }
            else if(Phase4City=="gyeongju")
            {
                yield return CrossEnter("jeonju");Time.timeScale=4;
                Check(game.Relics.Restored("relic_jeonju_hanji_stamp")&&!game.IsCityUnlocked("gyeongju"),"Hanji relic restoration alone did not open Gyeongju");
                yield return ChapterGo("J4");yield return ChapterChoice("talk");yield return ChapterGo("J5");yield return ChapterChoice("accept");yield return ChapterChoice("talk");
                yield return ChapterGo("J6");yield return ChapterChoice("alternate");yield return ChapterGo("J5");yield return ChapterConfirm("complete");
                yield return ChapterGo("J11");yield return ChapterChoice("collect");yield return ChapterPuzzle("J12");yield return ChapterGo("J12");yield return ChapterChoice("complete:chapter_jq01_finish");
                yield return ChapterGo("J17");yield return ChapterChoice("accept:chapter_jq03_offer");yield return ChapterPuzzle("J17");
                yield return ChapterGo("J14");yield return ChapterChoice("alternate");yield return ChapterGo("J19");yield return ChapterChoice("restore");yield return CloseEvent();
                Check(game.World.Has("jeonju.beacon_lit")&&game.IsCityUnlocked("gyeongju"),"Actual J19 station action opens Gyeongju by unchanged World Map rule");
            }
            else if(Phase4City=="seoul")
            {
                yield return CrossEnter("da_lat");Time.timeScale=4;
                yield return ChapterGo("D10");yield return ChapterChoice("accept");
                foreach(var node in new[]{"D9","D11"}){yield return ChapterGo(node);yield return ChapterChoice("collect");}
                yield return ChapterGo("D10");yield return ChapterConfirm("complete");yield return CloseEvent();
                yield return ChapterBattle("D15");yield return ChapterGo("D16");yield return ChapterChoice("collect");yield return ChapterChoice("accept:dalat_dq03_offer");
                yield return ChapterGo("D14");yield return ChapterChoice("restore");yield return CloseEvent();
                Check(game.World.Has("da_lat.beacon_lit")&&!game.IsCityUnlocked("seoul"),"Three beacons alone cannot open Seoul despite restored relics");
                yield return CrossEnter("gyeongju");yield return ChapterGo("G18");yield return ChapterChoice("accept:gq03_offer");yield return StoryBattle("G18");
                yield return ChapterGo("G19");yield return ChapterChoice("restore");yield return CloseEvent();
                Check(new[]{"hue.beacon_lit","da_lat.beacon_lit","jeonju.beacon_lit","gyeongju.beacon_lit"}.All(game.World.Has)&&game.IsCityUnlocked("seoul")&&game.Relics.Count(Phase4Relic)==0&&game.Inventory.Count("link_key")==0,"Four actual beacons open Seoul with zero Seoul fragments and no Link Key");
            }
            else Check(false,"Missing earned city access preparation; never set unlock flags for QA");
        }
        IEnumerator Phase4Restart()
        {
            var r=Phase4Relic;yield return CrossEnter(Phase4City);var restored=game.Relics.Restored(r.id);var before=JsonUtility.ToJson(game.Save);
            Check(game.Relics.Count(r)==4&&game.Relics.Assembled(r.id),"Process restart preserves four earned fragments and assembly");
            Check(Map.CulturalRestorationState==(restored?"C":"A")&&Map.RelicCelebrations==0&&!Map.CulturalRestorationAnimating,"Restart derives A/C immediately without repeat celebration");
            yield return Phase4Frame("P4-"+Phase4City+"-restart-"+(restored?"C":"A"));
            foreach(var f in r.fragmentIds)Check(!game.Relics.CanClaim(game.Relics.Fragment(f)),"Restart cannot reclaim owned "+f);
            yield return TestMapInput();yield return Click("Inventory",true);yield return Click("CloseInventory");
            Check(JsonUtility.ToJson(game.Save)==before,"Restart queries, pointer input and inventory preserve complete save");
            KeepSave("phase4-restart-final");
        }
    }
}
#endif
