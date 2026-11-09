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
        IEnumerator TerrainRouteEvidence()
        {
            Check(game.Quests.IsCompleted("HQ01"),"Terrain diagnostics require a genuinely earned chapter save");
            yield return MapPolishEvidence();
            Check(game.Data.nodes.All(n=>n.hasArrivalPosition),"All HA ground arrival anchors are explicitly configured");
            foreach(var city in game.Content.cities.Where(c=>c.id!="hoi_an"))
            {
                var other=GameData.Load(city.id);
                Check(other.nodes.All(n=>CityRouteGeometry.Arrival(n)==n.position),"Unauthored city retains its own original geometry: "+city.id);
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
