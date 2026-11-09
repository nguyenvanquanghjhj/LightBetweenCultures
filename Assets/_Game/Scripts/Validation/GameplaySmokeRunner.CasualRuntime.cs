#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator CasualHelp(string content)
        {
            var before=JsonUtility.ToJson(game.Save);yield return Click("ContextHelp",true);
            Check(GameObject.Find("ContextHelpCard")!=null&&GameObject.Find("HelpText").GetComponent<Text>().text.Contains(content),"Context help describes the actual current interaction: "+content);
            yield return Snapshot("Casual-help-"+game.Data.cityId+"-"+game.Data.nodes[game.Save.node].id);
            yield return Click("HelpDismiss",true);
            Check(GameObject.Find("ContextHelpCard")==null&&JsonUtility.ToJson(game.Save)==before,"Help dismisses locally without cancelling activity or changing gameplay state");
        }
        IEnumerator CasualRuntimeJourney()
        {
            musicVirtualFocus=true;
            if(Environment.GetCommandLineArgs().Contains("--casual-signature")){yield return CasualSignature();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--casual-core"))
            {
                yield return H3TapJourney();yield return Travel(NodeIndex("H2"),true);yield return Click("EventOption_tutorial_return",true);yield return CloseEvent();
                yield return Click("QuestJournal",true);
                var diary=GameObject.Find("Body").GetComponent<Text>().text;
                Check(diary.Contains("LẦN DỪNG CHÂN NÀY")&&diary.Contains("ẢNH ĐÃ LƯU")&&diary.Contains("SỔ DI VẬT")&&!diary.Contains("HQ01"),"Journal resumes from real city, quest, photo and relic state with player-facing names");
                yield return Snapshot("Casual-resume-journal");yield return Click("CloseJournal",true);
                yield return Travel(NodeIndex("H4"),true);yield return ChapterChoice("puzzle");yield return CloseEvent();
                yield return Travel(NodeIndex(game.Content.Puzzle("puzzle_paper").nodeId),true);yield return ChapterChoice("puzzle");
                var a=game.Puzzles.Activity;Check(a!=null&&!a.Ready,"Existing paper activity starts without an automatic solution");
                yield return CasualHelp("Chạm mảnh");Check(ReferenceEquals(a,game.Puzzles.Activity),"Context help keeps the same live minigame instance");
                yield return Click("MicroConfirm",true);Check(!game.Puzzles.Solved("puzzle_paper"),"Premature minigame confirmation remains a safe retry");
                yield return CloseEvent();yield return Click("Interact",true);yield return SolveRevisedPuzzle("puzzle_paper");
                var encounter=game.Data.nodes.Select((n,i)=>new{n,i}).First(x=>game.Discovery.IsVisible(x.i)&&game.Events.Definition(x.n.eventId)?.type=="encounter"&&!new EncounterManager(game).Resolved(game.Events.Definition(x.n.eventId)));
                yield return Travel(encounter.i,true);yield return ChapterChoice("battle");yield return CasualHelp("Phòng thủ");
                yield return Click("BattleRun",true);Check(FindButton("BattleRetry")!=null&&!game.Battles.Active,"Retreat offers an explicit retry without resolving the enemy");
                yield return Click("BattleRetry",true);Check(game.Battles.Active,"Retry starts the same unresolved encounter through BattleManager");
                yield return Snapshot("Casual-combat-actions");
                while(game.Battles.Active)yield return Click(game.Battles.Energy>0?"BattleLight":"BattleAttack",true);
                Check(game.Battles.Outcome=="win"&&FindButton("BattleRetry")==null,"Victory uses the existing result and does not offer an invalid reward replay");
                yield return Click("BattleClose",true);KeepSave("Casual-core-earned");yield break;
            }
            Check(game.Quests.IsCompleted("HQ01"),"Leisure/shared UI review starts from an earned completed save");
            if(!game.World.Has("hoi_an.secret_lane_found"))
            {
                yield return CrossEnter("hoi_an");Time.timeScale=5;yield return ChapterGo("H10");Time.timeScale=1;
                if(!game.Quests.IsActive("HQ02"))yield return ChapterChoice("accept");
                yield return CloseEvent();Time.timeScale=5;yield return ChapterGo("H12");Time.timeScale=1;
                var encounter=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
                if(!new EncounterManager(game).Resolved(encounter))
                {
                    if(game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="alternate"&&o.enabled))yield return ChapterChoice("alternate");
                    else
                    {yield return ChapterChoice("battle");while(game.Battles.Active)yield return Click(game.Battles.Energy>0?"BattleLight":"BattleAttack",true);Check(game.Battles.Outcome=="win","Earned crab result unlocks the original optional quest");yield return Click("BattleClose",true);}
                }
                yield return CloseEvent();Time.timeScale=5;yield return ChapterGo("H10");Time.timeScale=1;yield return ChapterChoice("complete");
                if(FindButton("DeliveryConfirm")!=null)yield return Click("DeliveryConfirm",true);
                Check(game.Quests.IsCompleted("HQ02")&&game.World.Has("hoi_an.secret_lane_found"),"Existing Ba Tu quest really opens the swing's hidden lane; no visibility flags injected");
                yield return CloseEvent();KeepSave("Casual-earned-optional-lane");
            }
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return Click("QuestJournal",true);
                var diary=GameObject.Find("Body").GetComponent<Text>().text;
                Check(diary.Contains(PlayerCopy.CityIdentity(city.id))&&diary.Contains("SỔ DI VẬT"),"City identity and persistent travel summary are actually displayed: "+city.id);
                yield return Snapshot("Casual-journal-"+city.id);yield return Click("CloseJournal",true);
                foreach(var e in LeisurePresentation.Load().entries.Where(e=>e.cityId==city.id))
                {
                    Time.timeScale=5;yield return ChapterGo(e.nodeId);Time.timeScale=1;yield return CloseEvent();
                    yield return LeisureFullOpen(e);var state=JsonUtility.ToJson(game.Save);
                    var pilot=FindAnyObjectByType<LeisurePilotView>();var activity=FindAnyObjectByType<LeisureActivityView>();
                    yield return CasualHelp("không phải thử thách");
                    Check(pilot==FindAnyObjectByType<LeisurePilotView>()&&activity==FindAnyObjectByType<LeisureActivityView>(),"Leisure help retains the approved animation driver: "+e.id);
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.6f);
                    var progress=FindAnyObjectByType<LeisureSessionProgress>();Check(progress!=null&&progress.Value>0&&progress.Value<1,"Actual animation clock advances visible progress: "+e.id);
                    yield return Until(()=>pilot!=null?!pilot.Playing:!activity.Playing,"leisure completes "+e.id,7);
                    Check(progress.Value>.98f&&FindButton("LeisureAction").GetComponentInChildren<Text>().text=="Chơi lại","Real completion offers replay and full progress: "+e.id);
                    Check(JsonUtility.ToJson(game.Save)==state,"Optional leisure presentation changes no gameplay or save: "+e.id);
                    if(e==LeisurePresentation.Load().entries.First(x=>x.cityId==city.id))yield return Snapshot("Casual-leisure-"+city.id);
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.1f);yield return CloseEvent();
                    Check(FindAnyObjectByType<LeisureSessionProgress>()==null&&Map.View.InputEnabled,"Mid-replay close destroys feedback and restores map input: "+e.id);
                }
            }
            KeepSave("Casual-six-cities-preserved");
        }
        IEnumerator CasualSignature()
        {
            var city=game.Save.cityId;phase4CityOverride=city;
            var relic=game.Relics.Catalog.relics.First(r=>r.cityId==city);
            Check(game.Relics.Assembled(relic.id)&&!game.Relics.Restored(relic.id),"Signature test uses an actually earned assembled, pre-trial save");
            yield return CrossEnter(city);Time.timeScale=5;yield return ChapterGo(relic.restorationNode);Time.timeScale=1;
            yield return RelicOption(city=="hoi_an"?"relic_restore":"relic_restore:"+relic.id);
            if(city=="hoi_an")
            {
                var a=game.Relics.Active;
                for(var mask=0;mask<8&&!game.Relics.Restored(relic.id);mask++)
                {for(var i=0;i<3;i++)if(a.mirrors[i]!=((mask&(1<<i))!=0))yield return Click("RelicMirror"+i,true);yield return Click("RelicTrace",true);}
                Check(game.Relics.Restored(relic.id),"Actual mirror controls and existing station proof restore Hoi An");
            }
            else yield return Phase4SolveTrial();
            var restored=JsonUtility.ToJson(game.Save);yield return RelicExit();
            yield return Until(()=>GameObject.Find("SignatureMoment")!=null,"real signature feedback "+city,5);
            Check(Map.SignatureReactions==1&&!GameObject.Find("SignatureMoment").GetComponent<CanvasGroup>().blocksRaycasts,"One real milestone reaction never intercepts touch: "+city);
            yield return Snapshot("Casual-signature-"+city);yield return new WaitForSecondsRealtime(4.2f);
            Check(JsonUtility.ToJson(game.Save)==restored,"Signature effect changes no reward, gate, quest or ending state: "+city);
            yield return CrossEnter(city);yield return new WaitForSecondsRealtime(.4f);
            Check(Map.SignatureReactions==0&&GameObject.Find("SignatureMoment")==null,"Re-entry derives restored lights without replaying celebration: "+city);
            KeepSave("Casual-signature-"+city+"-restored");
        }
    }
}
#endif
