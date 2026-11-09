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
        [Serializable] sealed class FinalPolishMapFrame
        {public string city,file,utc,state;public float zoom;public Vector2 focus;public int decorativeHalos;public float maxHaloWidth,maxHaloAlpha;}
        IEnumerator FinalPolishJourney()
        {
            musicVirtualFocus=true;var args=Environment.GetCommandLineArgs();
            if(args.Contains("--final-polish-maps"))
            {
                yield return PilotResolution(720,1280);var label=args.First(a=>a.StartsWith("--final-polish-map-label=")).Split('=')[1];
                foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
                {
                    yield return CrossEnter(city);yield return CloseEvent();Map.View.ZoomBy(1/Map.View.Zoom);Map.View.Focus(new Vector2(.5f,.5f));yield return new WaitForSecondsRealtime(1.5f);
                    var saved=JsonUtility.ToJson(game.Save);var name="FP-Map-"+city+"-"+label;yield return P6Shot(name);
                    var halos=FindObjectsByType<Image>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Where(g=>g.name.StartsWith("RelicLight_")||g.name.StartsWith("CulturalLight_")).ToArray();
                    Check(halos.Length>0&&halos.All(g=>!g.raycastTarget),"Restored landmark highlights exist and never block input: "+city);
                    Check(Map.RelicCelebrations==0,"Re-entry does not repeat restoration celebration: "+city);
                    Check(JsonUtility.ToJson(game.Save)==saved,"Map capture is presentation-only: "+city);
                    var data=new FinalPolishMapFrame{city=city,file=name+".png",utc=DateTime.UtcNow.ToString("O"),state=city=="hoi_an"?Map.HoiAnLightingState:Map.CulturalRestorationState,zoom=Map.View.Zoom,focus=new Vector2(.5f,.5f),decorativeHalos=halos.Length,maxHaloWidth=halos.Max(g=>g.rectTransform.rect.width),maxHaloAlpha=halos.Max(g=>g.color.a)};
                    File.AppendAllText(Path.Combine(output,"map-frames.jsonl"),JsonUtility.ToJson(data)+"\n");
                }
                yield return TestMapInput();yield break;
            }
            if(args.Contains("--final-polish-restart"))
            {
                var saved=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(game.Save));
                Check(ActivityPresentation.Load().entries.All(e=>game.Puzzles.Solved(e.id)),"Restart retains all10 minigame and12 short-interaction proofs");
                Check(game.Events.CanonicalEndingCompleted&&game.Relics.AllRestored&&game.Events.BossResolved,"Restart retains the earned sole ending, six relics and resolved boss");
                yield return CrossEnter(saved.cityId);yield return CloseEvent();var state=JsonUtility.ToJson(game.Save);
                foreach(var e in ActivityPresentation.Load().entries.Where(e=>e.id==game.Content.events.FirstOrDefault(v=>v.cityId==game.Save.cityId&&v.nodeId==game.Data.nodes[game.Save.node].id)?.puzzleId))Check(!game.Puzzles.Start(e.id),"Completed activity cannot start a reward replay: "+e.id);
                Check(game.Save.items.SequenceEqual(saved.items)&&game.Save.coins==saved.coins&&game.Save.rewardGrantIds.SequenceEqual(saved.rewardGrantIds),"Real restart/re-entry grants no duplicate rewards");
                Check(Map.View.InputEnabled&&!Map.DialogueOpen,"Restart has clean map input and no stuck activity");yield break;
            }
            yield return PilotResolution(540,960);
            Check(ActivityPresentation.Load().entries.Count(e=>e.enabled&&e.kind=="STORY_MINIGAME")==10&&ActivityPresentation.Load().entries.Count(e=>e.enabled&&e.kind=="LIGHT_STORY_RESOLUTION")==12,"Latest runtime implements10 minigames and12 short interactions in the existing puzzle flow");
            yield return Phase5Journey();
            // Existing optional proofs not traversed by every main-story route.
            yield return CrossEnter("gyeongju");Time.timeScale=4;
            if(!game.Puzzles.Solved("puzzle_pond"))yield return ChapterPuzzle("G2");
            if(!game.Puzzles.Solved("puzzle_cracked_seal")){yield return ChapterGo("G12");yield return ChapterChoice("accept:gq02_offer_finish");yield return ChapterPuzzle("G12");}
            if(!game.Puzzles.Solved("puzzle_temple"))yield return ChapterPuzzle("G15");
            if(!game.Puzzles.Solved("puzzle_lantern_west")){yield return ChapterGo("G17");yield return ChapterChoice("accept:gq04_offer_finish");yield return ChapterPuzzle("G17");yield return ChapterGo("G17");yield return ChapterChoice("complete:gq04_offer_finish");}
            Time.timeScale=1;yield return CloseEvent();
            foreach(var e in ActivityPresentation.Load().entries)Check(game.Puzzles.Solved(e.id),"Earned runtime proof complete: "+e.kind+" "+e.id);
            Check(game.Events.CanonicalEndingCompleted&&game.Relics.AllRestored&&game.Events.BossResolved,"Both story orders finish with canonical ending conditions, no progression redesign");
            KeepSave("FP-story-final");
        }
    }
}
#endif
