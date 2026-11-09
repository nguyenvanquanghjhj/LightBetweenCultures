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
        bool TerrainEvidenceEnabled=>Environment.GetCommandLineArgs().Contains("--terrain-routes");
        [Serializable] sealed class TerrainFrame
        {
            public string file,sequence;
            public Vector2 originalPixels;
            public float realtime;
            public bool covered;
        }
        readonly List<TerrainFrame> terrainFrames=new List<TerrainFrame>();
        readonly Dictionary<string,float> terrainNextFrame=new Dictionary<string,float>();
        void TerrainWalkingFrame(int from,int to,Vector2 p)
        {
            if(game.Data.cityId!="hoi_an")return;
            var pixel=new Vector2(p.x*1448,(1-p.y)*1086);
            string sequence=null;
            if(from==0&&to==1&&pixel.x>410&&pixel.x<620&&pixel.y>510&&pixel.y<590)sequence="west-open-bridge";
            if(from==13&&to==3&&pixel.x>650&&pixel.x<755)sequence="alley-roof-detour";
            if(from==5&&to==6&&pixel.x>900&&pixel.x<1175)sequence="covered-bridge-return";
            if(sequence==null)return;
            if(terrainNextFrame.TryGetValue(sequence,out var next)&&Time.realtimeSinceStartup<next)return;
            terrainNextFrame[sequence]=Time.realtimeSinceStartup+.075f;
            var name=sequence+"-"+terrainFrames.Count(f=>f.sequence==sequence).ToString("D3")+".png";
            ScreenCapture.CaptureScreenshot(Path.Combine(output,name));
            terrainFrames.Add(new TerrainFrame{file=name,sequence=sequence,originalPixels=pixel,realtime=Time.realtimeSinceStartup,covered=Map.LumiOccluded});
            captures.Add(DateTime.UtcNow.ToString("O")+" | "+name);
            Check(Map.Marker.GetComponent<Image>().enabled==!Map.LumiOccluded,"Roof occlusion changes presentation only; actual ground motion continues");
        }
        void WriteTerrainFrames()
        {
            File.WriteAllText(Path.Combine(output,"terrain-walking-frames.json"),"["+string.Join(",",terrainFrames.Select(f=>JsonUtility.ToJson(f)))+"]");
            foreach(var sequence in new[]{"west-open-bridge","alley-roof-detour","covered-bridge-return"})
            {
                var frames=terrainFrames.Where(f=>f.sequence==sequence).ToArray();
                Check(frames.Length>=3&&frames.All(f=>File.Exists(Path.Combine(output,f.file))),"Fresh sequential walking frames exist: "+sequence);
                Check(frames.Zip(frames.Skip(1),(a,b)=>(b.originalPixels-a.originalPixels).sqrMagnitude).All(d=>d>0),"Consecutive frames capture actual motion: "+sequence);
            }
            Check(terrainFrames.Any(f=>f.covered),"Covered bridge includes actual occluded passage frames; no roof walking sprite");
        }
        [Serializable] sealed class Route35Case {public string cityId,fromId,toId;public bool changed;}
        [Serializable] sealed class Route35Cases {public Route35Case[] cases;}
        float Route35Distance(Vector2 p,System.Collections.Generic.List<Vector2> points,Vector2 size)
        {
            float best=float.MaxValue;
            for(var i=1;i<points.Count;i++){var a=Vector2.Scale(points[i-1],size);var d=Vector2.Scale(points[i]-points[i-1],size);var t=d.sqrMagnitude<.000001f?0:Mathf.Clamp01(Vector2.Dot(Vector2.Scale(p,size)-a,d)/d.sqrMagnitude);best=Mathf.Min(best,Vector2.Distance(Vector2.Scale(p,size),a+t*d));}return best;
        }
        void CheckRoute35Dots()
        {
            var renderer=Map.View.Content.GetComponent<CityMapPaths>();var before=JsonUtility.ToJson(game.Save);var max=0f;var safe=true;
            for(var i=0;i<renderer.DotSamples.Count;i++)
            {
                var e=renderer.DotEdges[i];var p=renderer.DotSamples[i];
                safe&=renderer.Graph.EdgeOpen(e,game.Save)&&!CityRouteGeometry.IsCovered(game.Data,p);
                max=Mathf.Max(max,Route35Distance(p,CityRouteGeometry.Points(e),Map.View.Content.rect.size));
            }
            Check(safe&&max<.025f,"Every emitted dot belongs to an open/discovered actual walk polyline and stays outside roof cover; max error="+max);
            Check(renderer.DotSamples.Count<=game.Data.routeStyle.maxDots&&Map.View.Content.Find("RouteDots").GetComponent<Graphic>().raycastTarget==false,"One bounded route mesh cannot intercept touch; dots="+renderer.DotSamples.Count);
            Check(JsonUtility.ToJson(game.Save)==before,"Route rendering cannot mutate quests, rewards, relics or save");
        }
        IEnumerator Route35Movement(Route35Case[] cases)
        {
            var real=JsonUtility.ToJson(game.Save);var traces=new List<RouteTrace>();
            foreach(var c in cases)foreach(var reverse in new[]{false,true})
            {
                yield return CloseEvent();var from=NodeIndex(reverse?c.toId:c.fromId);var to=NodeIndex(reverse?c.fromId:c.toId);
                game.Save.node=from;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                var original=game.Data.edges;var edge=original.First(e=>e.from==from&&e.to==to||e.from==to&&e.to==from);
                var pts=CityRouteGeometry.Points(game.Data,from,to);game.Data.edges=new[]{edge};
                var covered=game.Data.cityId=="hoi_an"&&c.fromId=="H18"&&c.toId=="H5";
                Time.timeScale=covered?1:3;Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[to]));yield return new WaitForSecondsRealtime(.15f);
                yield return Click(game.Data.nodes[to].id,reverse);Check(Map.Moving,"Mouse/touch starts real controller movement: "+c.cityId+" "+from+"->"+to);
                var trace=new RouteTrace{from=from,to=to,route=pts.ToArray(),travel=edge.travel};var size=Map.View.Content.rect.size;var deadline=Time.realtimeSinceStartup+40;
                bool captured=false,entered=false,exited=false;
                while(Map.Moving&&Time.realtimeSinceStartup<deadline)
                {
                    var point=Map.Marker.anchorMin;trace.positions.Add(point);trace.times.Add(Time.realtimeSinceStartup);trace.maxDistance=Mathf.Max(trace.maxDistance,Route35Distance(point,pts,size));
                    if(covered&&!reverse&&(!captured||!entered&&Map.LumiOccluded||entered&&!exited&&!Map.LumiOccluded))
                    {
                        var label=!captured?"entry":!entered?"under-roof":"exit";captured=true;if(Map.LumiOccluded)entered=true;else if(entered)exited=true;
                        var speed=Time.timeScale;Time.timeScale=0;yield return Snapshot("ChuaCau-"+label);Time.timeScale=speed;
                    }
                    yield return null;
                }
                trace.arrived=game.Save.node==to&&!Map.Moving;Check(trace.arrived&&trace.positions.Count>1&&trace.maxDistance<.025f,"Movement follows authored waypoint geometry and arrives in both directions: "+c.cityId+" "+from+"->"+to+" error="+trace.maxDistance);
                if(covered&&!reverse)Check(entered&&exited,"Approved ChuaCau still hides Lumi under roof and restores sprite on exit");
                CheckRoute35Dots();traces.Add(trace);game.Data.edges=original;
                File.WriteAllText(Path.Combine(output,game.Data.cityId+"-route35-traces.json"),"["+string.Join(",",traces.Select(t=>JsonUtility.ToJson(t)))+"]");
            }
            Time.timeScale=1;File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==real,"Edge-only movement fixtures restore exact earned state after "+game.Data.cityId+" diagnostics");
        }
        IEnumerator Route35Journey(bool after)
        {
            Check(game.Quests.IsCompleted("HQ01")&&game.World.Has("game.ending_seen"),"Route review uses a copy of a genuinely earned full-story save, not synthetic gate flags");
            var start=JsonUtility.ToJson(game.Save);
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                yield return CrossEnter(city);yield return CloseEvent();
                // Explicit camera/arrival fixture for comparable presentation, never story evidence.
                game.Save.node=0;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                Screen.SetResolution(1280,960,false);yield return new WaitForSecondsRealtime(.3f);
                Map.View.ZoomBy(1/Map.View.Zoom);Map.View.Focus(new Vector2(.5f,.5f));yield return new WaitForSecondsRealtime(.3f);
                yield return Snapshot((after?"After-":"Before-")+city+"-overview");
                Check(Map.NodeMarkers.Count==game.Data.nodes.Length,"Route view preserves all canonical marker objects: "+city);
                if(after)
                {
                    CheckRoute35Dots();
                    Screen.SetResolution(540,960,false);yield return new WaitForSecondsRealtime(.3f);Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);yield return new WaitForSecondsRealtime(.2f);
                    yield return Snapshot("After-"+city+"-portrait");CheckRoute35Dots();
                    Screen.SetResolution(1280,960,false);yield return new WaitForSecondsRealtime(.3f);
                    var path=Environment.GetCommandLineArgs().First(v=>v.StartsWith("--route35-cases=")).Substring("--route35-cases=".Length);
                    var cases=JsonUtility.FromJson<Route35Cases>(File.ReadAllText(path)).cases.Where(c=>c.cityId==city).ToArray();
                    yield return Route35Movement(cases);
                }
            }
            File.WriteAllText(savePath,start);game.Reload();
            Check(JsonUtility.ToJson(game.Save)==start,"Presentation diagnostics restore exact input save, including old choices/reward ledger");
        }
        IEnumerator TerrainRouteEvidence()
        {
            Check(game.Quests.IsCompleted("HQ01"),"Terrain diagnostics require a genuinely earned chapter save");
            yield return MapPolishEvidence();
            Check(game.Data.nodes.All(n=>n.hasArrivalPosition),"All HA ground arrival anchors are explicitly configured");
            foreach(var city in game.Content.cities.Where(c=>c.id!="hoi_an"))
            {
                var other=GameData.Load(city.id);
                Check(other.nodes.All(n=>CityRouteGeometry.Arrival(n).x>=0&&CityRouteGeometry.Arrival(n).x<=1&&CityRouteGeometry.Arrival(n).y>=0&&CityRouteGeometry.Arrival(n).y<=1),"Each city retains its own explicit walk geometry inside canonical map bounds: "+city.id);
            }
            if(!game.Quests.IsCompleted("HQ04"))
            {
                yield return Travel(17);var before=game.Save.coins;
                yield return FoodChoice("accept");yield return FoodChoice("complete");
                Check(game.Quests.IsCompleted("HQ04")&&game.Save.coins==before-5,"Ferry diagnostics earn the original quest, never inject an unlock flag");
                KeepSave("terrain-ferry-earned");yield return CloseEvent();
            }
            yield return ChapterRouteDiagnostics();
            foreach(var zoom in new[]{1f,2f,3.5f})
            {
                Map.View.ZoomBy(zoom/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);
                Check(Vector2.Distance(Map.Marker.anchorMin,CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]))<.00001f,"Zoom preserves the ground arrival anchor");
                CheckMapActors("Terrain zoom "+zoom);yield return Snapshot("Ground-anchor-zoom-"+zoom);
            }
            Check(FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1,"Terrain changes reuse the existing Canvas");
            File.WriteAllText(Path.Combine(output,"terrain-visual-status.txt"),"AUTOMATED MOTION/STATE CHECKS ONLY. Terrain visual acceptance PENDING. R1 covered bridge floor, R2 temple side steps, R3 eastern canopy edge NEEDS_REVIEW. See original-resolution annotated plots.");
        }
    }
}
#endif
