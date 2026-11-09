#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        bool XPAfter=>Environment.GetCommandLineArgs().Contains("--player-experience-after");
        void XPTextAudit()
        {
            if(!XPAfter)return;
            foreach(var t in FindObjectsByType<Text>(FindObjectsSortMode.None))
                Check(!Regex.IsMatch(t.text??"",@"\b(?:H\d{1,2}|HQ\d{2}|TODO|TBD|null|undefined)\b")&&!t.text.Contains("Assets/"),"Player copy hides internal identifiers: "+t.name);
        }
        IEnumerator XPShot(string name)
        {
            yield return new WaitForSecondsRealtime(.45f);XPTextAudit();
            if(Application.isEditor&&Application.isBatchMode)
            {
                Canvas.ForceUpdateCanvases();var path=Path.Combine(output,"XP-"+name+".png");ScreenCapture.CaptureScreenshot(path);
                yield return new WaitForSecondsRealtime(.25f); // Verify files visually before treating these as evidence.
                if(File.Exists(path))captures.Add("UNITY_PLAY_MODE_BATCH_CAPTURE | "+path);
            }
            else yield return Snapshot("XP-"+name);
        }
        IEnumerator PlayerExperienceJourney()
        {
            musicVirtualFocus=true;
            if(XPAfter&&!Environment.GetCommandLineArgs().Contains("--player-experience-leisure"))PlayerPrefs.DeleteKey("LBC.PlayerGuide.QA");
            if(Environment.GetCommandLineArgs().Contains("--player-experience-leisure"))
            {
                yield return CrossEnter("hoi_an");Time.timeScale=5;yield return ChapterGo("H16");Time.timeScale=1;yield return CloseEvent();
                yield return Click("Interact",true);yield return XPShot("lantern-menu");yield return ChapterChoice("leisure:leisure_hoian_lanternrelease_stand");
                yield return XPShot("lantern-idle");var state=JsonUtility.ToJson(game.Save);
                var view=FindAnyObjectByType<LeisureActivityView>();Check(view!=null&&!view.Playing,"Lantern opens real existing activity idle");
                var lotus=GameObject.Find("ReleasedLotus").GetComponent<RectTransform>();var start=lotus.position;
                yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(1.7f);Check(view.Playing&&Vector3.Distance(start,lotus.position)>3,"Released lotus actually drifts in Windows Player");
                yield return XPShot("lantern-drifting");yield return Until(()=>!view.Playing,"Lantern animation completes",7);yield return XPShot("lantern-replay");
                Check(JsonUtility.ToJson(game.Save)==state,"Leisure remains reward-free; exact existing save semantics retained");
                yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.3f);yield return CloseEvent();
                Check(FindAnyObjectByType<LeisureActivityView>()==null&&Map.View.InputEnabled,"Mid-replay close restores input and destroys driver");
                KeepSave("XP-leisure-restart");yield break;
            }
            yield return CrossEnter("hoi_an");
            if(FindButton("GuideSkip")!=null)
            {yield return XPShot("intro");yield return Click("GuideNext",true);yield return XPShot("first-step");yield return Click("GuideSkip",true);}
            Map.View.ZoomBy(1.55f/Map.View.Zoom);Map.View.Focus(new Vector2(.42f,.52f));yield return XPShot("map");
            yield return Travel(NodeIndex("H2"),true);yield return XPShot("mai-before-quest");
            var before=JsonUtility.ToJson(game.Save);
            if(XPAfter){Check(FindButton("DialogueCloseX")!=null&&FindButton("DialogueSkipEffect")==null&&FindButton("DialogueNext")==null,"NPC dialogue has explicit X close and no misleading Next");yield return Click("DialogueCloseX",true);}
            else yield return Click("EventClose",true);
            Check(JsonUtility.ToJson(game.Save)==before&&!game.Quests.Active,"Closing Mai cannot accept quest or reward");
            yield return new WaitForSecondsRealtime(.35f); // Preserve the map's intentional post-close tap debounce.
            yield return Click("Interact",true);yield return Click("EventOption_accept",true);
            Check(game.Quests.Active&&game.Quests.IsActive("HQ01"),"Actual NPC accept starts existing tutorial and main quest");
            yield return XPShot("lumi-promise");yield return Click("DialogueNext",true);yield return CloseEvent();
            if(XPAfter)
            {
                yield return Click("ObjectiveHint",true);yield return XPShot("objective");var node=game.Save.node;yield return Click("GuidanceLocate",true);
                Check(game.Save.node==node&&!Map.Moving,"Show objective focuses only; never teleports or starts travel");
                yield return new WaitForSecondsRealtime(.3f);
            }
            yield return Travel(NodeIndex("H3"),true);yield return ExploreH3();
            Check(!game.Quests.Completed&&!game.IsCityUnlocked("hue"),"Pickup does not finish tutorial or unlock cities");
            yield return Travel(NodeIndex("H2"),true);yield return XPShot("mai-return-eye");yield return Click("EventOption_tutorial_return",true);
            Check(game.Quests.Completed&&!game.Quests.IsCompleted("HQ01")&&!game.IsCityUnlocked("hue"),"Returning Mắt Đèn completes only local tutorial; main quest/world gate unchanged");
            yield return XPShot("mai-local-light");yield return CloseEvent();
            yield return Click("QuestJournal",true);yield return XPShot("journal");yield return Click("CloseJournal",true);
            KeepSave("XP-earned-tutorial");game.Persist();var saved=File.ReadAllText(savePath);
            if(!Environment.GetCommandLineArgs().Contains("--player-experience-main"))
            {
                game.Reload();
                Check(game.Quests.Completed&&!game.Quests.IsCompleted("HQ01")&&File.ReadAllText(savePath)==saved,"Reload preserves earned tutorial without extra grants or writes");
            }
            if(Environment.GetCommandLineArgs().Contains("--player-experience-main"))
            {
                Time.timeScale=4;
                yield return ChapterPuzzle("H4");yield return ChapterPuzzle("H13");
                yield return ChapterGo("H14");yield return CloseEvent();yield return ChapterPuzzle("H19");
                yield return ChapterGo("H5");yield return ChapterChoice("alternate");yield return CloseEvent();
                yield return ChapterGo("H7");yield return ChapterChoice("restore");Time.timeScale=1;
                Check(game.Quests.IsCompleted("HQ01")&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju"),"Earned market, paper, bot-code branch and station preserve main quest/world unlock");
                Check(!game.Relics.Restored("relic_hoi_an_lantern"),"Station cannot silently restore the relic or full city lighting");
                yield return XPShot("station-main-complete");yield return CloseEvent();KeepSave("XP-earned-main");
            }
        }
    }
}
#endif
