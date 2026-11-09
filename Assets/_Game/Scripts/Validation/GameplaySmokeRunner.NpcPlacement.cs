#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        // Bounded visual audit in the existing runner, using an isolated earned save.
        IEnumerator NpcPlacementJourney()
        {
            musicVirtualFocus=true;
            yield return PilotResolution(720,1280);
            var interact=Environment.GetCommandLineArgs().Contains("--npc-placement-interactions");
            var rows=new List<string>{"city,node,npc,visible,footX,footY,width,height,dialogue,close,resume,approachFailure"};
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                yield return CrossEnter(city);yield return CloseEvent();
                var geometry=JsonUtility.ToJson(game.Data.navigation);
                foreach(var a in AdventureUIArt.Load().mapActors.Where(a=>a.cityId==city&&AdventureUIArt.Load().Npc(a.artId)!=null))
                {
                    // Only fixture placement; subsequent clicks and approach are the real UI.
                    game.Save.node=NodeIndex(a.nodeId);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                    yield return new WaitForSecondsRealtime(.6f);
                    var image=MapNpcImage(a.nodeId);var index=game.Save.node;
                    Map.View.ZoomBy(2.4f/Map.View.Zoom);Map.View.Focus(StandingFoot(image));yield return new WaitForSecondsRealtime(.3f);
                    yield return Snapshot("Placement-"+a.nodeId);
                    var foot=RectTransformUtility.WorldToScreenPoint(null,CityMap.VisibleFoot(image));
                    bool opened=false,closed=false,resumed=false;string failure="NOT_RUN";
                    if(interact)
                    {
                        var state=JsonUtility.ToJson(game.Save);
                        Time.timeScale=6;
                        yield return Pointer(RectTransformUtility.WorldToScreenPoint(null,image.rectTransform.TransformPoint(image.rectTransform.rect.center)));
                        var until=Time.realtimeSinceStartup+12;
                        while((Map.Moving||!Map.DialogueOpen)&&Map.NpcApproachFailure==null&&Time.realtimeSinceStartup<until)yield return null;
                        Time.timeScale=1;opened=Map.DialogueOpen;failure=Map.NpcApproachFailure??(opened?"":"NO_DIALOGUE");
                        if(opened)
                        {
                            yield return new WaitForSecondsRealtime(.6f);yield return Click("DialogueCloseX");yield return new WaitForSecondsRealtime(.4f);
                            closed=!Map.DialogueOpen&&Map.View.InputEnabled;
                        }
                        else closed=!Map.DialogueOpen&&Map.View.InputEnabled;
                        Check(state==JsonUtility.ToJson(game.Save),"Reading/closing NPC preserves save: "+a.nodeId);
                        var next=Enumerable.Range(0,game.Data.nodes.Length).Where(i=>i!=index&&game.Discovery.IsVisible(i)&&game.Data.FindPath(index,i,game.Save)?.Count==2).DefaultIfEmpty(-1).First();
                        if(next>=0)
                        {
                            Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[next]));yield return new WaitForSecondsRealtime(.4f);
                            Time.timeScale=8;yield return Click(game.Data.nodes[next].id);
                            until=Time.realtimeSinceStartup+12;while(Map.Moving&&Time.realtimeSinceStartup<until)yield return null;
                            Time.timeScale=1;resumed=game.Save.node==next&&!Map.Moving;
                        }
                        results.Add((opened&&closed&&resumed?"PASS: ":"REVIEW: ")+"NPC mouse/approach/close/resume "+a.nodeId+" "+opened+"/"+closed+"/"+resumed+" "+failure);
                    }
                    rows.Add(string.Join(",",city,a.nodeId,a.artId,image.gameObject.activeInHierarchy,foot.x,foot.y,image.rectTransform.rect.width,image.rectTransform.rect.height,opened,closed,resumed,failure.Replace(",",";")));
                }
                Check(geometry==JsonUtility.ToJson(game.Data.navigation),"Phase B unchanged: "+city);
            }
            File.WriteAllLines(Path.Combine(output,"placement-runtime.csv"),rows);
            // Only the reported bell activity, no full leisure/campaign suite.
            yield return CrossEnter("gyeongju");yield return CloseEvent();
            game.Save.node=NodeIndex("G10");game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            var entry=LeisurePresentation.Load().entries.First(e=>e.nodeId=="G10");yield return LeisureFullOpen(entry);
            var saved=JsonUtility.ToJson(game.Save);var hit=GameObject.Find("LeisureGesture").GetComponent<RectTransform>();
            yield return SLDrag(SLPoint(hit),SLPoint(hit)+Vector2.right*100,false);
            Check(FullView.Playing,"Bell starts with mouse drag");yield return new WaitForSecondsRealtime(.25f);yield return Snapshot("Placement-bell-swing");
            yield return CloseEvent();Check(JsonUtility.ToJson(game.Save)==saved&&Map.View.InputEnabled,"Bell closes and returns controls without changing save");
        }
    }
}
#endif
