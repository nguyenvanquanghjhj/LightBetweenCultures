#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        bool RelicQA=>Environment.GetCommandLineArgs().Any(a=>a.StartsWith("--relic-",StringComparison.Ordinal));
        IEnumerator RelicOption(string id)
        {
            yield return FoodChoice(id);
        }
        IEnumerator RelicDrag(int index,bool useTouch,bool correct=true)
        {
            Canvas.ForceUpdateCanvases();var piece=(RectTransform)GameObject.Find("RelicPiece"+index).transform;var target=(RectTransform)GameObject.Find("RelicTarget"+index).transform;
            var start=RectTransformUtility.WorldToScreenPoint(null,piece.position);var end=RectTransformUtility.WorldToScreenPoint(null,target.position);
            if(!correct)end+=new Vector2(-140,70);
            if(useTouch)InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=start});
            else{InputSystem.QueueStateEvent(mouse,new MouseState{position=start});yield return null;InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));}
            yield return null;yield return null;
            for(var step=1;step<=12;step++)
            {var p=Vector2.Lerp(start,end,step/12f);if(useTouch)InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=p});else InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left));yield return null;}
            if(useTouch)InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=end});else InputSystem.QueueStateEvent(mouse,new MouseState{position=end});
            yield return null;yield return null;
        }
        IEnumerator RelicExit()
        {yield return Click("RelicExit",true);yield return new WaitForSecondsRealtime(.35f);}
        IEnumerator ClaimRelic(string id,bool touchDrag)
        {
            yield return RelicOption("relic:"+id);var a=game.Relics.Active;
            Check(a!=null&&!game.Relics.Owned(id),"Source opens a real gesture without auto-grant: "+id);
            var coins=game.Save.coins;var items=game.Save.items.ToArray();
            yield return Click("RelicClaim");Check(!game.Relics.Owned(id),"Confirm without placement cannot claim: "+id);
            yield return RelicDrag(0,touchDrag,false);Check(!a.Placed(0),"Wrong drop rejected: "+id);
            yield return RelicDrag(0,touchDrag);Check(a.Placed(0),"Actual mouse/touch drop reaches target: "+id);
            yield return Click("RelicClaim");Check(!game.Relics.Owned(id),"Wrong orientation rejected: "+id);
            yield return Click("RelicRotate",touchDrag);yield return Click("RelicRotate",touchDrag);yield return Click("RelicClaim",touchDrag);
            Check(game.Relics.Owned(id)&&game.World.Has("relic.proof."+id),"Gesture commits distinct proof and ownership: "+id);
            Check(game.Save.rewardGrantIds.Count(k=>k=="relic:v1:claim:"+id)==1&&!a.Claim(),"Claim ledger idempotent after repeated confirmation: "+id);
            Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(items),"Collection never replaces story items or source rewards: "+id);
            if(id.EndsWith("roof_hook",StringComparison.Ordinal))yield return Snapshot("Relic-H10-hook-collected");
            yield return RelicExit();
        }
        IEnumerator RelicMapFrame(string name)
        {
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);Map.View.ZoomBy(1.25f/Map.View.Zoom);Map.View.Focus(new Vector2(.48f,.55f));yield return new WaitForSecondsRealtime(.4f);yield return Snapshot(name);
        }
        IEnumerator RelicAssembly(bool beforeGate)
        {
            yield return RelicOption("relic_restore");
            if(!game.Relics.Assembled("relic_hoi_an_lantern"))
            {
                yield return Click("RelicAssemble");Check(!game.Relics.Assembled("relic_hoi_an_lantern"),"Assembly rejects empty slots");
                for(var i=0;i<4;i++)yield return RelicDrag(i,i%2==0);
                yield return Snapshot("Relic-four-pieces-assembly");yield return Click("RelicAssemble",true);
            }
            Check(game.Relics.Assembled("relic_hoi_an_lantern"),"Four actual drops persist assembled state");
            yield return Click("RelicTrace");Check(!game.Relics.Restored("relic_hoi_an_lantern"),"Wrong beam route cannot restore city");
            yield return Click("RelicHint");
            for(var i=0;i<3;i++)yield return Click("RelicMirror"+i,i%2==0);
            yield return Click("RelicTrace",true);
            if(beforeGate)Check(!game.Relics.Restored("relic_hoi_an_lantern")&&!Map.CityRelicIlluminated,"Correct beam before H7 still cannot complete ritual or full-city light");
            else
            {
                Check(game.Relics.Restored("relic_hoi_an_lantern")&&Map.CityRelicIlluminated,"Correct beam after unchanged H7 unlock restores HoiAn in actual CityMap");
                Check(GameObject.Find("BeamSegments").transform.GetSiblingIndex()>GameObject.Find("LanternBeamBoard").transform.Find("RoundedBackground").gameObject.transform.GetSiblingIndex(),"Golden beam segments render above the rounded board background");
                Check(Map.RelicCelebrations==1,"Full-city transition emits one Lumi celebration");yield return Snapshot("Relic-beam-valid-restoration");
            }
            yield return RelicExit();
        }
        IEnumerator RelicPhoto(bool alreadyMarked)
        {
            var coins=game.Save.coins;var postcard=game.Inventory.Count("hoi_an_bridge_postcard");var ledger=game.Save.rewardGrantIds.ToArray();
            yield return Click("PhotoCompose",true);yield return Click("RelicRecordPhoto");Check(!game.Relics.PhotoRecorded,"Opening photo view or empty confirmation does not record evidence");
            yield return RelicDrag(0,true);yield return RelicDrag(1,false);yield return Click("RelicRecordPhoto",true);
            Check(game.Relics.PhotoRecorded,"H6 two-pointer alignment records postcard metadata");
            if(RelicStoryQA)Check(game.Relics.Story("photo:stored").Contains("sau Dẫn Ánh Đèn"),"Stored postcard remembers the actual illuminated capture state");
            Check(game.Save.coins==coins+(alreadyMarked?0:2)&&game.Inventory.Count("hoi_an_bridge_postcard")==postcard+(alreadyMarked?0:1),"Photo preserves old H6 reward exactly once; old marked save gets no duplicate");
            var once=game.Save.coins;yield return Click("RelicRecordPhoto");Check(game.Save.coins==once,"Photo replay does not farm coins");yield return Snapshot("Relic-H6-postcard-aligned");yield return RelicExit();
        }
        bool RelicStoryQA=>Environment.GetCommandLineArgs().Contains("--relic-story");
        void StoryCheck(int node,string expected)
        {
            var before=JsonUtility.ToJson(game.Save);var lines=game.Relics.Catalog.story;
            game.Relics.Catalog.story=Array.Empty<RelicStoryLine>();
            var original=game.Events.BuildOptions(node);game.Relics.Catalog.story=lines;
            var current=game.Events.BuildOptions(node);
            if(game.Relics.Restored("relic_hoi_an_lantern")&&(node==2||node==7))Check(!current.text.Contains("cả phố cổ còn chờ Đèn Mắt Phố"),"Restored dialogue removes stale waiting-for-relic claim");
            Check(current.options.Select(o=>o.id+":"+o.enabled).SequenceEqual(original.options.Select(o=>o.id+":"+o.enabled)),"Story preserves exact old choices/availability at "+game.Data.nodes[node].id);
            Check(current.text.StartsWith(original.text,StringComparison.Ordinal)&&current.text.Contains(expected),"Runtime dialogue appends correct earned-state story: "+expected);
            Check(JsonUtility.ToJson(game.Save)==before,"Reading story does not mutate progress, inventory, rewards or save at "+game.Data.nodes[node].id);
        }
        IEnumerator StoryCapture(string name,bool baseline=false)
        {
            yield return CloseEvent();var lines=game.Relics.Catalog.story;
            if(baseline)game.Relics.Catalog.story=Array.Empty<RelicStoryLine>();
            yield return Click("Interact",true);yield return new WaitForSecondsRealtime(.35f);
            var scroll=GameObject.Find("DialogueTextViewport")?.GetComponent<ScrollRect>();
            if(scroll!=null)scroll.verticalNormalizedPosition=0;
            yield return Snapshot(name);yield return CloseEvent();game.Relics.Catalog.story=lines;
        }
        IEnumerator RelicStoryRestart()
        {
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton,true);yield return Click("EnterCity");yield return Scene("CityMap");
            var before=JsonUtility.ToJson(game.Save);var full=game.Relics.Restored("relic_hoi_an_lantern");var gate=game.Quests.IsCompleted("HQ01");
            StoryCheck(2,full?"đã dẫn ánh sáng khắp Hội An":gate?"bắt đầu Dẫn Ánh Đèn":"căn dấu phản chiếu Mắt Đèn");
            Check(Map.CityRelicIlluminated==full&&Map.LocalQuestLightComplete,"Restart derives both light layers from independent earned proof");
            Check(game.IsCityUnlocked("hue")==gate&&game.IsCityUnlocked("jeonju")==gate,"Restart city gates depend on HQ01 beacon, never collection");
            Check(Map.RelicCelebrations==0&&Map.LocalQuestReactionBursts==0,"Restart story does not replay reactions");
            for(var n=0;n<game.Data.nodes.Length;n++)if(!game.Discovery.IsVisible(n))Check(game.Events.BuildOptions(n).options.Count==0&&!game.Events.BuildOptions(n).text.Contains("Đèn Mắt Phố"),"Hidden node has no relic story or input: "+n);
            Check(JsonUtility.ToJson(game.Save)==before,"Restart story queries leave the complete save unchanged");
            yield return StoryCapture("Story-restart-"+(full?"restored":gate?"H7-ready":"local"));
            Check(JsonUtility.ToJson(game.Save)==before,"Actual dialogue reopen/close does not grant a second reward");
        }
        IEnumerator RelicPilotJourney()
        {
            game.ResetSave();yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton,true);yield return Click("EnterCity");yield return Scene("CityMap");
            Check(game.Relics.Catalog.relics.Length==6&&game.Relics.Catalog.fragments.Length==24,"One catalog has six canonical relic IDs and24 fragments");
            Check(!game.Relics.AllRestored&&game.Relics.Catalog.relics.Count(r=>r.enabled)==1,"Only HoiAn gameplay enabled; no false six-city completion");
            Check(!game.Relics.CanClaim(game.Relics.Fragment("fragment_hoi_an_eye_reflection")),"Reflection unavailable before tutorial proof");
            if(RelicStoryQA){StoryCheck(0,"bộ nan tre");StoryCheck(2,"Trước hết cháu giúp cô");Check(!game.Discovery.IsVisible(10)&&!game.Events.BuildOptions(2).text.Contains("móc đồng"),"Fresh story does not expose hidden fragment location");}
            yield return Travel(0,true);yield return ClaimRelic("fragment_hoi_an_bamboo_frame",false);
            yield return Travel(2,true);yield return Click("EventOption_accept",true);yield return Click("DialogueNext");yield return CloseEvent();
            yield return Travel(3,true);yield return ExploreH3();
            yield return Travel(2,true);Check(game.World.Has("hoi_an.lamp_eye_found")&&!game.Relics.CanClaim(game.Relics.Fragment("fragment_hoi_an_eye_reflection")),"Pickup alone does not satisfy reflection proof");
            Check(!game.Events.CanCheckIn(2),"H2 check-in cannot claim a light that has not been returned");
            if(RelicStoryQA){StoryCheck(3,"mang nó trong túi chưa làm đèn tiệm sáng");Check(!game.Relics.Story("event:H2").Contains("Chiếc đèn đầu tiên trước tiệm đã sáng"),"Pickup alone cannot produce returned-quest dialogue");}
            yield return Click("EventOption_tutorial_return",true);yield return CloseEvent();
            Check(game.Quests.Completed&&!game.Quests.IsCompleted("HQ01")&&Map.LocalQuestLightComplete&&!Map.CityRelicIlluminated,"Mai turn-in preserves local-only light and incomplete HQ01");
            Check(game.Events.CanCheckIn(2),"H2 check-in becomes available after real tutorial, before H7");
            if(RelicStoryQA){StoryCheck(2,"căn dấu phản chiếu Mắt Đèn");game.Persist();KeepSave("story-local");yield return StoryCapture("Story-local-before-baseline",true);yield return StoryCapture("Story-local-after-guidance");}
            yield return RelicOption("checkin");
            yield return Click("CheckInRecord",true);
            if(RelicStoryQA){Check(GameObject.Find("CheckInDescription").GetComponent<Text>().text.Contains("cả phố chưa được phục dựng"),"Actual H2 check-in describes local light before H7");yield return Snapshot("Story-H2-checkin-local");}
            yield return Click("CheckInClose");
            yield return ClaimRelic("fragment_hoi_an_eye_reflection",true);
            yield return Travel(4,true);yield return SolvePuzzle("puzzle_market");
            if(RelicStoryQA)StoryCheck(4,"Bên sạp còn Tấm Giấy Hoa Cổ");
            yield return ClaimRelic("fragment_hoi_an_old_flower_paper",false);
            yield return Travel(10,true);yield return ClaimRelic("fragment_hoi_an_roof_hook",true);
            Check(!game.Quests.IsCompleted("HQ02"),"Bà Tư collection interaction does not complete or reward HQ02");
            if(RelicStoryQA){StoryCheck(10,"giúp bà cuộn dây");yield return StoryCapture("Story-BaTu-old-quest-preserved");StoryCheck(2,"Bốn mảnh đã đủ");}
            yield return Travel(2,true);yield return RelicAssembly(true);
            if(RelicStoryQA)StoryCheck(2,"nhịp dẫn ánh sáng còn chờ trạm Hội Quán");
            Time.timeScale=4;yield return Travel(13,true);yield return SolvePuzzle("puzzle_paper");yield return Travel(19,true);yield return SolvePuzzle("puzzle_pottery");
            yield return Travel(5,true);yield return Click("EventOption_alternate",true);yield return CloseEvent();yield return Travel(7,true);var coins=game.Save.coins;yield return Click("EventOption_restore",true);yield return CloseEvent();Time.timeScale=1;
            Check(game.Quests.IsCompleted("HQ01")&&game.Save.coins==coins+150&&!Map.CityRelicIlluminated,"H7 reward+150 and city gates remain old quest logic; no full city before relic");
            yield return ReactionWorldGate(true);yield return Travel(2,true);yield return CloseEvent();yield return RelicMapFrame("Relic-before-full-city-H7-done");
            if(RelicStoryQA){StoryCheck(7,"hai tuyến Huế, Jeonju đã mở");StoryCheck(2,"bắt đầu Dẫn Ánh Đèn");game.Persist();KeepSave("story-H7-ready");yield return StoryCapture("Story-H7-ready-before-ritual");}
            coins=game.Save.coins;yield return RelicAssembly(false);Check(game.Save.coins==coins,"Ritual has no duplicate story reward/currency");
            yield return RelicMapFrame("Relic-after-full-city-same-view");
            if(RelicStoryQA){StoryCheck(2,"đã dẫn ánh sáng khắp Hội An");StoryCheck(7,"việc riêng của mình");yield return StoryCapture("Story-Mai-full-city-restored");}
            Check(GameObject.Find("RelicIlluminationLayer").GetComponent<CanvasGroup>().blocksRaycasts==false,"New full-city overlay cannot intercept touch");
            yield return Travel(6,true);yield return RelicPhoto(false);
            yield return Click("QuestJournal",true);yield return Click("OpenRelicCollection",true);yield return Snapshot("Relic-collection-six-icons");yield return Click("PhotoAlbumH6");yield return Snapshot("Relic-photo-album-revisit");yield return Click("CheckInClose",true);
            Check(game.Save.version==2&&!game.World.Has("game.ending_seen")&&!game.Relics.AllRestored,"Pilot leaves save schema and ending unchanged");game.Persist();KeepSave("relic-pilot-final");
        }
        IEnumerator RelicRestartJourney()
        {
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton,true);yield return Click("EnterCity");yield return Scene("CityMap");
            Check(game.Relics.Restored("relic_hoi_an_lantern")&&game.Relics.PhotoRecorded&&Map.CityRelicIlluminated,"Actual process restart preserves collection, ritual, photo and full city");
            Check(Map.RelicCelebrations==0&&Map.LocalQuestReactionBursts==0,"Restart/re-entry do not replay completion celebrations");
            var coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();var items=game.Save.items.ToArray();
            Screen.SetResolution(1280,720,false);yield return new WaitForSecondsRealtime(.5f);yield return RelicMapFrame("Relic-restart-landscape");
            Screen.SetResolution(540,960,false);yield return new WaitForSecondsRealtime(.5f);yield return Travel(2,true);
            Check(!game.Relics.CanClaim(game.Relics.Fragment("fragment_hoi_an_eye_reflection")),"Restart does not re-offer claimed fragments");yield return CloseEvent();
            yield return Click("QuestJournal");yield return Click("OpenRelicCollection");yield return Click("PhotoAlbumH6");yield return RelicExit();
            yield return Click("Back",true);yield return Scene("WorldMap");yield return Click(HoiAnButton,true);yield return Click("EnterCity");yield return Scene("CityMap");
            Check(Map.CityRelicIlluminated&&Map.RelicCelebrations==0,"City re-entry derives full illumination without a new event");
            Check(game.Save.coins==coins&&game.Save.rewardGrantIds.SequenceEqual(ledger)&&game.Save.items.SequenceEqual(items),"Restart/revisit/journal leave inventory and reward ledger exact");
        }
        IEnumerator RelicRetroJourney()
        {
            Check(game.World.Has("game.ending_seen")&&game.World.Has("checkin.hoi_an.h6"),"Retro test uses genuinely earned old ending + H6 mark");
            var completed=game.Save.completed.ToArray();var items=game.Save.items.ToArray();var coins=game.Save.coins;
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton,true);yield return Click("EnterCity");yield return Scene("CityMap");
            foreach(var f in game.Relics.Catalog.fragments.Where(f=>f.enabled).ToArray())
            {
                yield return ChapterGo(f.nodeId);
                if(f.id=="fragment_hoi_an_old_flower_paper")
                {var solved=game.Save.solvedPuzzles.ToArray();game.Save.solvedPuzzles.RemoveAll(p=>p=="puzzle_market");Check(game.Relics.CanClaim(f),"Compatibility fixture: earned market flag alone permits old-save recovery without replaying puzzle");game.Save.solvedPuzzles=solved.ToList();}
                if(f.id=="fragment_hoi_an_roof_hook")
                {var flags=game.Save.flags.ToArray();game.Save.flags.RemoveAll(p=>p=="quest:HQ01");Check(game.Relics.CanClaim(f),"Compatibility fixture: completed/accepted HQ01 proves source even without duplicated quest flag");game.Save.flags=flags.ToList();}
                yield return ClaimRelic(f.id,true);
            }
            yield return ChapterGo("H6");yield return RelicPhoto(true);
            Check(game.Save.completed.SequenceEqual(completed)&&game.Save.items.SequenceEqual(items)&&game.Save.coins==coins,"Old-save claims/photo preserve exact29 quests, story inventory and coins");
            Check(game.World.Has("game.ending_seen")&&!game.Relics.Restored("relic_hoi_an_lantern")&&!game.Relics.AllRestored,"Old credits/old quest proof never auto-restore relic or new completion");game.Persist();KeepSave("relic-retro-final");
        }
        IEnumerator RelicNpcJourney()
        {
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton);yield return Click("EnterCity");yield return Scene("CityMap");
            foreach(var pair in new[]{("seoul","S2","npc_curator"),("seoul","S11","npc_engineer"),("gyeongju","G9","npc_keeper"),("gyeongju","G14","npc_inscription")})
            {
                if(game.Data.cityId!=pair.Item1)yield return CrossEnter(pair.Item1);
                yield return ChapterGo(pair.Item2);yield return new WaitForSecondsRealtime(.5f);PortraitCheck();
                var image=GameObject.Find("EventPortrait").GetComponent<Image>();var expected=AdventureUIArt.Load().Npc(pair.Item3);
                Check(image.sprite==expected.portrait&&expected.status.Contains("PENDING_APPROVAL"),"Correct pending-approval portrait renders, never canonicalized: "+pair.Item3);
                Check(GameObject.Find("EventSpeaker").GetComponent<Text>().text==game.Events.Definition(game.Data.nodes[game.Save.node].eventId).actor,"Speaker remains data canonical role: "+pair.Item3);
                yield return Snapshot("NPC-"+pair.Item2+"-"+pair.Item3);yield return CloseEvent();
            }
            Check(AdventureUIArt.Load().Npc("npc_curator").portrait!=AdventureUIArt.Load().Npc("npc_inscription").portrait,"Curator and Inscription retain distinct source files and bindings, visual identity review still pending");
        }
    }
}
#endif
