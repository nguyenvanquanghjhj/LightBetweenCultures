#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        [Serializable] sealed class ApprovedRouteTrace
        {
            public string cityId,fromId,toId,travel;
            public bool arrived,enteredCover,exitedCover;
            public float maxError;
            public Vector2[] polyline;
            public List<Vector2> positions=new List<Vector2>();
            public List<float> times=new List<float>();
        }
        IEnumerator ApprovedRoutesJourney()
        {
            musicVirtualFocus=true;
            var args=Environment.GetCommandLineArgs();
            // Optional bounded route regression; production graph/controller are unchanged.
            var filterArg=args.FirstOrDefault(a=>a.StartsWith("--approved-routes-filter="));
            var selected=filterArg==null?null:new HashSet<string>(File.ReadAllLines(filterArg.Substring("--approved-routes-filter=".Length)).Where(s=>!string.IsNullOrWhiteSpace(s)));
            var original=JsonUtility.ToJson(game.Save);
            if(args.Contains("--approved-routes-fresh"))
            {
                // A genuinely new save opens WorldMap. Enter the city through
                // its real UI before inspecting map graphics/input targets.
                yield return CrossEnter("hoi_an");yield return CloseEvent();
                original=JsonUtility.ToJson(game.Save);
                Check(game.Save.completed.Count==0,"Discovery regression begins with a genuinely fresh QA save");
                var visible=Enumerable.Range(0,game.Data.nodes.Length).Where(game.Discovery.IsVisible).ToArray();
                Check(visible.Length<game.Data.nodes.Length,"Fresh map does not reveal the full graph after waypoint change");
                foreach(var i in Enumerable.Range(0,game.Data.nodes.Length).Except(visible))
                    Check(FindButton(game.Data.nodes[i].id)==null&&game.Data.FindPath(game.Save.node,i,game.Save)==null,"HIDDEN node remains unclickable/unreachable: "+game.Data.nodes[i].id);
                CheckRoute35Dots();yield return Snapshot("Fresh-HIDDEN");
                Check(JsonUtility.ToJson(game.Save)==original,"Fresh discovery/dot diagnostics do not mutate progress");yield break;
            }
            Check(game.Quests.IsCompleted("HQ01")&&game.World.Has("game.ending_seen"),"Uses an earned full-story save copy; no gate or quest flags injected");
            if(args.Contains("--approved-routes-restart"))
            {
                var loaded=JsonUtility.FromJson<SaveData>(original);
                yield return CrossEnter(loaded.cityId);yield return CloseEvent();
                Check(!Map.Moving,"Real Player restart restores a stationary saved arrival");
                Check(game.Save.node==loaded.node,"World Map re-entry returns the exact saved city node");
                Check(Vector2.Distance(Map.Marker.anchorMin,CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]))<.00001f,"Restart keeps canonical arrival anchor");
                CheckRoute35Dots();yield return Snapshot("Restart-map");
                Check(game.Save.accepted.SequenceEqual(loaded.accepted)&&game.Save.completed.SequenceEqual(loaded.completed)&&game.Save.flags.SequenceEqual(loaded.flags)&&game.Save.items.SequenceEqual(loaded.items)&&game.Save.choices.SequenceEqual(loaded.choices)&&game.Save.rewardGrantIds.SequenceEqual(loaded.rewardGrantIds)&&game.Save.coins==loaded.coins,"Restart and World Map re-entry do not change quest/reward/relic/history state");yield break;
            }
            var traces=new List<ApprovedRouteTrace>();var count=0;
            var capturesByCity=new Dictionary<string,string>{{"hoi_an","H0__H1"},{"hue","U1__U2"},{"da_lat","D13__D21"},{"jeonju","J2__J12"},{"gyeongju","G4__G9"},{"seoul","S8__S9"}};
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                yield return CrossEnter(city);yield return CloseEvent();
                if(selected!=null){game.Save.node=0;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");}
                var citySave=JsonUtility.ToJson(game.Save);
                var ids=game.Content.City(city).edges.Select(e=>e.id).Where(id=>selected==null||selected.Contains(city+":"+id)).ToArray();
                if(ids.Length==0)continue;
                if(selected!=null)capturesByCity[city]=ids[0];
                Screen.SetResolution(1280,960,false);yield return new WaitForSecondsRealtime(.3f);
                Map.View.ZoomBy(1/Map.View.Zoom);Map.View.Focus(new Vector2(.5f,.5f));yield return new WaitForSecondsRealtime(.3f);
                yield return Snapshot("After-"+city+"-overview");
                CheckRoute35Dots();
                foreach(var id in ids)
                {
                    var record=game.Content.City(city).edges.First(e=>e.id==id);
                    var reverseCases=selected!=null||id==ids[0]||city=="hoi_an"&&id=="H18__H5"||city=="hue"&&id=="U3__U4"||city=="seoul"&&id=="S6__S14";
                    foreach(var reverse in reverseCases?new[]{false,true}:new[]{false})
                    {
                        yield return CloseEvent();File.WriteAllText(savePath,citySave);game.Reload();
                        var from=NodeIndex(reverse?record.to:record.from);var to=NodeIndex(reverse?record.from:record.to);
                        game.Save.node=from;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                        var all=game.Data.edges;var edge=all.First(e=>e.from==from&&e.to==to||e.from==to&&e.to==from);
                        Check(game.Discovery.IsVisible(from)&&game.Discovery.IsVisible(to)&&game.Data.EdgeOpen(edge,game.Save),"Earned visibility/requirements permit actual edge "+city+" "+id);
                        var points=CityRouteGeometry.Points(game.Data,from,to);game.Data.edges=new[]{edge};
                        var cover=city=="hoi_an"&&id=="H18__H5"&&!reverse;
                        Time.timeScale=cover?1:8;Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[to]));yield return new WaitForSecondsRealtime(.15f);
                        var trace=new ApprovedRouteTrace{cityId=city,fromId=game.Data.nodes[from].id,toId=game.Data.nodes[to].id,travel=edge.travel,polyline=points.ToArray()};
                        var size=Map.View.Content.rect.size;var duration=Mathf.Max(.65f,CityRouteGeometry.Length(points,size)/190f);var started=Time.time;
                        // QA acceleration only; leave enough real frames for a complete
                        // simulated pointer press/release on very short existing edges.
                        if(!cover)Time.timeScale=Mathf.Min(8,duration/.4f);
                        yield return Click(game.Data.nodes[to].id,count%2==1);Check(Map.Moving,"Real mouse/touch starts CityMap.Move: "+city+" "+id+" reverse="+reverse);
                        var deadline=Time.realtimeSinceStartup+60;var frame=0;var shot=0;
                        while(Map.Moving&&Time.realtimeSinceStartup<deadline)
                        {
                            var p=Map.Marker.anchorMin;trace.positions.Add(p);trace.times.Add(Time.realtimeSinceStartup);
                            trace.maxError=Mathf.Max(trace.maxError,Route35Distance(p,points,size));
                            var was=trace.enteredCover;if(Map.LumiOccluded)trace.enteredCover=true;else if(was)trace.exitedCover=true;
                            if(cover&&(frame==0||!was&&trace.enteredCover||was&&trace.exitedCover&&shot<3))
                            {
                                var label=frame==0?"entry":Map.LumiOccluded?"covered":"exit";shot++;
                                var speed=Time.timeScale;Time.timeScale=0;yield return Snapshot("H6-"+label);Time.timeScale=speed;
                            }
                            if(!reverse&&id==capturesByCity[city]&&shot<3&&(Time.time-started)/duration>new[]{.18f,.45f,.73f}[shot])
                            {var speed=Time.timeScale;Time.timeScale=0;yield return Snapshot("Move-"+city+"-"+shot);Time.timeScale=speed;shot++;}
                            frame++;yield return null;
                        }
                        trace.arrived=game.Save.node==to&&!Map.Moving;
                        Check(trace.arrived&&trace.positions.Count>1&&trace.maxError<.025f,"Actual movement follows waypoint polyline to canonical arrival: "+city+" "+id+" reverse="+reverse+" error="+trace.maxError);
                        Check(Vector2.Distance(Map.Marker.anchorMin,CityRouteGeometry.Arrival(game.Data.nodes[to]))<.00001f,"Exact endpoint retained: "+city+" "+id);
                        if(cover)Check(trace.enteredCover&&trace.exitedCover,"H6 covered passage still hides/reveals Lumi without changing occlusion");
                        CheckRoute35Dots();traces.Add(trace);count++;game.Data.edges=all;
                        File.WriteAllText(Path.Combine(output,"movement-traces.json"),"["+string.Join(",",traces.Select(t=>JsonUtility.ToJson(t)))+"]");
                    }
                }
                Time.timeScale=1;File.WriteAllText(savePath,citySave);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                Screen.SetResolution(540,960,false);yield return new WaitForSecondsRealtime(.3f);Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);
                CheckRoute35Dots();yield return Snapshot("Portrait-"+city);
                Check(JsonUtility.ToJson(game.Save)==citySave,"Edge fixtures restore exact earned city state, rewards and choices: "+city);
            }
            Time.timeScale=1;File.WriteAllText(savePath,original);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==original,"Entire movement QA restores exact input save; no gameplay state migration");
            KeepSave("route-review-restored-input");
            if(selected==null)Check(traces.Count>=155,"All 155 existing edges traversed in real CityMap controller, plus reverse cases");
            else Check(traces.Count==selected.Count*2,"Every selected changed/protected edge traversed in both directions using the real CityMap controller");
        }
    }
}
#endif
