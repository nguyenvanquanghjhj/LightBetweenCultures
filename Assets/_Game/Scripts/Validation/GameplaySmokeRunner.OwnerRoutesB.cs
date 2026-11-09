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
        [Serializable] class OwnerBoatCase {public string route,city,from,to;}
        [Serializable] class OwnerJunctionCase {public string id,city,from,to,target,vertex;}
        [Serializable] class OwnerBoatCases {public OwnerBoatCase[] cases;public OwnerJunctionCase[] junctions;}
        [Serializable] class OwnerTravelFrame {public string edge,mode;public Vector2 point;public float seconds;public int savedNode;}
        [Serializable] class OwnerTravelTrace {public string route,from,to;public OwnerTravelFrame[] frames;public float cost;}
        IEnumerator OwnerRouteShot(string name)
        {
            Canvas.ForceUpdateCanvases();var file=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(file);
            yield return new WaitForSecondsRealtime(.15f);Check(File.Exists(file),"Fresh route runtime image "+name);captures.Add(file);
        }
        void OwnerRouteDataAudit()
        {
            var total=0;var allNodes=0;
            foreach(var city in game.Content.cities)
            {
                var data=GameData.Load(city.id);var graph=data.navigation;
                Check(graph!=null&&graph.enabled,"Production uses approved owner navigation: "+city.id);
                File.WriteAllText(Path.Combine(output,"runtime-navigation-"+city.id+".json"),JsonUtility.ToJson(graph,true));
                var flags=game.Content.cities.SelectMany(c=>c.nodes.SelectMany(n=>n.requiredFlags??Array.Empty<string>()).Concat(c.edges.SelectMany(e=>e.requiredFlags??Array.Empty<string>())).Concat(c.gates.Select(g=>g.condition))).Concat(graph.edges.SelectMany(e=>e.requiredFlags.Concat(e.access.SelectMany(r=>r.flags)))).Distinct().ToList();
                var fixture=new SaveData{cityId=city.id,flags=flags};fixture.cityProgress.Add(new CityProgress{cityId=city.id,stage=4});
                Check(graph.nodes.Length==data.nodes.Length&&graph.nodes.All(n=>data.nodes.Any(d=>d.id==n.nodeId)),"Canonical node IDs/count retained: "+city.id);
                Check(graph.nodes.All(n=>Vector2.Distance(graph.Vertex(n.vertexId).position,CityRouteGeometry.Arrival(data.nodes[Array.FindIndex(data.nodes,d=>d.id==n.nodeId)]))<.000001f),"All canonical arrival positions retained: "+city.id);
                Check(graph.edges.All(e=>e.points.Length>=2&&e.points.All(p=>p.x>=0&&p.y>=0&&p.x<=1&&p.y<=1)&&Vector2.Distance(e.points[0],graph.Vertex(e.from).position)<.000001f&&Vector2.Distance(e.points.Last(),graph.Vertex(e.to).position)<.000001f),"Valid connected polyline endpoints and bounds: "+city.id);
                Check(Enumerable.Range(0,data.nodes.Length).All(i=>data.FindRoute(data.SpawnIndex,i,fixture)!=null),"All mandatory/optional nodes reachable in explicitly unlocked GRAPH FIXTURE: "+city.id);
                var fresh=new SaveData{cityId=city.id};fresh.cityProgress.Add(new CityProgress{cityId=city.id,stage=0});
                Check(Enumerable.Range(0,data.nodes.Length).Where(i=>!MapDiscoveryManager.Visible(data,data.nodes[i],fresh)).All(i=>data.FindRoute(data.SpawnIndex,i,fresh)==null),"Fresh graph cannot route to HIDDEN nodes: "+city.id);
                Check(Enumerable.Range(0,data.nodes.Length).Where(i=>graph.NodeOpen(data,i,fresh)).All(i=>data.FindRoute(data.SpawnIndex,i,fresh)!=null),"Every valid fresh destination has a route before optional ferry unlocks: "+city.id);
                total+=graph.edges.Length;allNodes+=data.nodes.Length;
            }
            Check(allNodes==133&&game.Content.quests.Length==29&&ActivityPresentation.Load().entries.Length==22&&LeisurePresentation.Load().entries.Length==12&&game.Relics.Catalog.relics.Length==6&&game.Relics.Catalog.fragments.Length==24,"Source totals:133 nodes/29 quests/22 story interactions/12 leisure/6 relics/24 fragments");
            Debug.Log("OWNER ROUTE B ACTIVE NAVIGATION EDGES: "+total);
            var probe=new GameData{cityId="hoi_an",nodes=new[]{new NodeData{id="a",position=new Vector2(.1f,.1f)},new NodeData{id="b",position=new Vector2(.3f,.1f)},new NodeData{id="c",position=new Vector2(.16f,.12f)},new NodeData{id="d",position=new Vector2(.23f,.12f)},new NodeData{id="far",position=new Vector2(.2f,.9f)}},edges=new[]{new EdgeData{from=0,to=4},new EdgeData{from=4,to=1},new EdgeData{from=0,to=2},new EdgeData{from=2,to=3},new EdgeData{from=3,to=1},new EdgeData{from=0,to=1,bidirectional=false,requiredFlags=new[]{"qa.shortcut"}}}};
            var save=new SaveData();Check(probe.FindPath(0,1,save).SequenceEqual(new[]{0,2,3,1}),"Dijkstra prefers three short polylines over two long edges; closed shortcut rejected");
            save.flags.Add("qa.shortcut");Check(probe.FindPath(0,1,save).SequenceEqual(new[]{0,1})&&probe.FindPath(1,0,save).Count>2,"Open shortcut used forward; one-way direction respected in reverse");
        }
        IEnumerator OwnerRouteTravel(OwnerBoatCase test,bool reverse)
        {
            var from=reverse?test.to:test.from;var to=reverse?test.from:test.to;
            yield return ChapterGo(from);yield return CloseEvent();
            var plan=game.Data.FindRoute(NodeIndex(from),NodeIndex(to),game.Save);
            Check(plan!=null&&plan.legs.Any(l=>l.edge.travel=="ferry"&&l.edge.source.Contains(test.route)),test.route+" shortest permitted route uses real boat: "+from+" -> "+to);
            // Acceptance framing: native map pixels, same full portrait viewport
            // as clarity QA. Do not judge art using the small overview preview.
            Map.View.ZoomBy(Mathf.Clamp(1448f/Screen.width,1,4)/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);
            yield return OwnerRouteShot(test.route+(reverse?"-reverse-start":"-walk-start"));
            Map.View.Focus(game.Data.nodes[NodeIndex(to)].position);yield return new WaitForSecondsRealtime(.35f);yield return Click(to,true);
            yield return Until(()=>Map.Moving||game.Save.node==NodeIndex(to),"boat route touch accepted "+test.route,4);
            var frames=new List<OwnerTravelFrame>();var captured=false;var end=Time.realtimeSinceStartup+35;var maxError=0f;
            while(Map.Moving&&Time.realtimeSinceStartup<end)
            {
                var point=Map.Marker.anchorMin;var edge=Array.Find(game.Data.navigation.edges,e=>e.id==Map.ActiveNavigationEdgeId);
                if(edge!=null)maxError=Mathf.Max(maxError,Route35Distance(point,CityRouteGeometry.Points(edge),Map.View.Content.rect.size));
                frames.Add(new OwnerTravelFrame{point=point,edge=Map.ActiveNavigationEdgeId,mode=Map.ActiveTravelMode,seconds=Time.realtimeSinceStartup,savedNode=game.Save.node});
                if(!captured&&Map.ActiveTravelMode=="ferry")
                {captured=true;Check(Map.Marker.Find("FerryTransport").gameObject.activeInHierarchy,"Actual boat sprite visible on water "+test.route);yield return OwnerRouteShot(test.route+(reverse?"-reverse-boat":"-boat"));}
                yield return null;
            }
            Check(!Map.Moving&&game.Save.node==NodeIndex(to)&&captured&&Map.NavigationTransfers>=2&&maxError<.06f,test.route+" real walking/boarding/boat/disembark reaches locked arrival, error="+maxError);
            yield return CloseEvent();Check(Map.ActiveTravelMode=="walk"&&!Map.Marker.Find("FerryTransport").gameObject.activeInHierarchy&&Map.View.InputEnabled,"Disembark restores feet/map input "+test.route);
            yield return OwnerRouteShot(test.route+(reverse?"-reverse-arrival":"-disembarked"));
            File.WriteAllText(Path.Combine(output,test.route+(reverse?"-reverse":"-forward")+"-trace.json"),JsonUtility.ToJson(new OwnerTravelTrace{route=test.route,from=from,to=to,frames=frames.ToArray(),cost=plan.cost},true));
            CheckRoute35Dots();
        }
        IEnumerator OwnerRouteBJourney()
        {
            musicVirtualFocus=true;OwnerRouteDataAudit();
            if(Environment.GetCommandLineArgs().Contains("--owner-route-b-seoul-tail"))
            {
                var korea=Environment.GetCommandLineArgs().Contains("--owner-route-korea");phase4PreferRepair=!korea;phase4CityOverride="seoul";
                Check(game.Relics.Catalog.relics.Where(r=>r.cityId!="seoul").All(r=>game.Relics.Restored(r.id))&&!game.Events.BossResolved,"Resume actual newly-earned five-city journey at Seoul; no injected quest/fragment flags");
                yield return Phase4CityJourney();
                yield return ChapterGo("S19");yield return ChapterChoice("alternate");yield return ChapterGo("S20");yield return ChapterChoice("accept:sq03_offer");yield return CloseEvent();
                yield return StoryBattle("S20",!korea);Time.timeScale=1;
                yield return ChapterGo("S21");yield return CloseEvent();yield return Phase5Finish(false);
                Check(game.Relics.AllRestored&&game.Events.BossResolved&&game.Events.CanonicalEndingCompleted,"Resumed genuine route reaches all six relics, two-phase boss, final connection and sole ending");
                KeepSave("OwnerRoutes-story-final");yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--owner-route-b-story")){yield return Phase5Journey();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--owner-route-b-fresh")){yield return PlayerExperienceJourney();yield break;}
            var arg=Environment.GetCommandLineArgs().First(a=>a.StartsWith("--owner-route-cases="));
            var cases=JsonUtility.FromJson<OwnerBoatCases>(File.ReadAllText(arg.Substring("--owner-route-cases=".Length)));
            foreach(var test in Environment.GetCommandLineArgs().Contains("--owner-route-b-junctions")?Array.Empty<OwnerBoatCase>():cases.cases)
            {
                yield return CrossEnter(test.city);
                if(test.route=="R5"&&!game.World.Has("seoul.ferry_acknowledged")){yield return ChapterGo("S14");yield return ChapterChoice("talk");yield return CloseEvent();}
                yield return OwnerRouteTravel(test,false);yield return OwnerRouteTravel(test,true);
                KeepSave("OwnerRoutes-"+test.route);
            }
            foreach(var test in cases.junctions)
            {
                yield return CrossEnter(test.city);yield return ChapterGo(test.from);yield return CloseEvent();yield return new WaitForSecondsRealtime(.35f);
                var plan=game.Data.FindRoute(game.Save.node,NodeIndex(test.to),game.Save);
                Check(plan!=null&&plan.legs.Any(l=>l.To==test.vertex),"Initial real route passes confirmed junction "+test.id);
                Check(game.Data.navigation.NodeOpen(game.Data,NodeIndex(test.target),game.Save),"Redirect destination is genuinely revealed "+test.id);
                Map.View.ZoomBy(Mathf.Clamp(1448f/Screen.width,1,4)/Map.View.Zoom);Map.TryMove(NodeIndex(test.to));
                var deadline=Time.realtimeSinceStartup+35;
                while(Map.Moving&&(Map.ActiveJourney==null||!Map.ActiveJourney.legs.Any(l=>l.edge.id==Map.ActiveNavigationEdgeId&&l.To==test.vertex))&&Time.realtimeSinceStartup<deadline)yield return null;
                Check(Map.Moving&&Map.ActiveJourney!=null&&Map.ActiveJourney.legs.Any(l=>l.edge.id==Map.ActiveNavigationEdgeId&&l.To==test.vertex),"Approaching the ACTUAL confirmed junction before redirect "+test.id);
                Map.TryMove(NodeIndex(test.target));yield return OwnerRouteShot(test.id+"-approaching");
                yield return Until(()=>!Map.Moving,"junction redirect "+test.id,35);
                Check(game.Save.node==NodeIndex(test.target)&&Map.ActiveJourney.legs.First().From==test.vertex,"Runtime replanned AT confirmed junction and reaches new branch "+test.id);
                yield return CloseEvent();yield return OwnerRouteShot(test.id+"-branch-arrival");
            }
            foreach(var city in game.Content.cities)
            {yield return CrossEnter(city.id);Map.View.ZoomBy(Mathf.Clamp(1448f/Screen.width,1,4)/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);yield return OwnerRouteShot("City-"+city.id);}
            yield return TestMapInput();KeepSave("OwnerRoutes-final");
        }
    }
}
#endif
