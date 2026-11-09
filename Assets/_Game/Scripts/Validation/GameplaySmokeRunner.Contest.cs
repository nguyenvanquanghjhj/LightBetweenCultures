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
        IEnumerator ContestResumeJourney()
        {
            if(Environment.GetCommandLineArgs().Contains("--retention-journal")){yield return RetentionJournalJourney();yield break;}
            musicVirtualFocus=true;
            var before=JsonUtility.ToJson(game.Save);var city=game.Save.cityId;var index=game.Save.node;
            var ui=FindAnyObjectByType<UIController>();
            Check(city!="hoi_an"&&game.Save.completed.Count>0,"Resume QA uses an earned save outside Hoi An");
            Check(ui.Message.text.Contains(game.Content.City(city).name)&&!ui.Message.text.StartsWith("Khám phá Hội An"),"Main menu names the actual saved city and stop");
            Check(FindButton("Start").GetComponentInChildren<Text>().text=="Tiếp tục hành trình","Existing journey offers Continue");
            yield return Snapshot("Contest-continue-menu");yield return Click("Reset",true);yield return Click("CancelReset",true);
            Check(before==JsonUtility.ToJson(game.Save),"Cancelling reset preserves the entire earned save");
            yield return CrossEnter(city);Check(game.Save.node==index,"Continue returns to the same canonical node through the existing World Map");
            var saved=JsonUtility.FromJson<SaveData>(before);
            Check(game.Save.items.SequenceEqual(saved.items)&&game.Save.flags.SequenceEqual(saved.flags)&&game.Save.completed.SequenceEqual(saved.completed)&&game.Save.rewardGrantIds.SequenceEqual(saved.rewardGrantIds),"Continue preserves inventory, quest, relic and reward-ledger history");
            yield return Snapshot("Contest-continued-saved-stop");KeepSave("Contest-resume-reviewed");
        }
        IEnumerator RetentionJournalJourney()
        {
            musicVirtualFocus=true;
            Check(game.Quests.IsCompleted("SQ03")&&!game.Quests.IsActive("HQ02"),"Resume review starts with a genuine finished main journey and untouched optional Ba Tu quest");
            yield return CrossEnter("hoi_an");Time.timeScale=5;yield return ChapterGo("H10");Time.timeScale=1;
            yield return ChapterChoice("accept");yield return CloseEvent();
            Check(game.Quests.IsActive("HQ02")&&!game.Quests.IsCompleted("HQ02"),"Real NPC choice accepts the existing optional quest; no injected state");
            foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
            {
                yield return PilotResolution(resolution.x,resolution.y);var saved=JsonUtility.ToJson(game.Save);
                yield return Click("QuestJournal",true);var text=GameObject.Find("Body").GetComponent<Text>().text;
                var task=text.IndexOf("• "+game.Content.Quest("HQ02").title,StringComparison.Ordinal);var history=text.IndexOf("ĐÃ HOÀN THÀNH",StringComparison.Ordinal);
                Check(task>=0&&history>task&&text.Contains("VIỆC TIẾP THEO")&&text.Contains("SỔ DI VẬT"),"Actual journal puts live task before compact completed history and retains next step/relic guidance");
                Check(!text.Contains(game.Quests.Progress("HQ01")),"Completed main-quest thank-you paragraph no longer buries the current task");
                yield return Snapshot("Retention-journal-active-first-"+resolution.x);yield return Click("CloseJournal",true);
                Check(JsonUtility.ToJson(game.Save)==saved,"Reordered journal is read-only, including rewards, accepted quests and relic state");
            }
            yield return CrossEnter("seoul");yield return Click("QuestJournal",true);
            var resumed=GameObject.Find("Body").GetComponent<Text>().text;
            Check(resumed.Contains("Seoul")&&resumed.Contains(game.Content.Quest("HQ02").title),"Cross-city continuation keeps the outstanding quest visible without changing cities or dropping its record");
            yield return Snapshot("Retention-journal-cross-city");yield return Click("CloseJournal",true);KeepSave("Retention-journal-earned");
        }
    }
}
#endif
