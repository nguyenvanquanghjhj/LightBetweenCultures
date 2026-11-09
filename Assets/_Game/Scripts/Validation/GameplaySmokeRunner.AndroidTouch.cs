#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Profiling;
using Unity.Profiling;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        readonly List<string> touchFindings=new List<string>();
        bool TouchBaseline=>Environment.GetCommandLineArgs().Contains("--touch-baseline");
        void TouchObserve(bool ok,string message)
        {touchFindings.Add((ok?"PASS: ":"FAIL: ")+message);if(!TouchBaseline)Check(ok,message);}
        void TouchQueue(int id,UnityEngine.InputSystem.TouchPhase phase,Vector2 point)
        {InputSystem.QueueStateEvent(touch,new TouchState{touchId=id,phase=phase,position=point});}
        Vector2 TouchPoint(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        IEnumerator TouchPair(Vector2 center,bool finish=true)
        {
            TouchQueue(71,UnityEngine.InputSystem.TouchPhase.Began,center+Vector2.left*42);TouchQueue(72,UnityEngine.InputSystem.TouchPhase.Began,center+Vector2.right*42);yield return null;yield return null;
            for(var i=1;i<=8;i++)
            {TouchQueue(71,UnityEngine.InputSystem.TouchPhase.Moved,center+Vector2.left*(42+i*5));TouchQueue(72,UnityEngine.InputSystem.TouchPhase.Moved,center+Vector2.right*(42+i*5));yield return null;}
            TouchQueue(72,UnityEngine.InputSystem.TouchPhase.Ended,center+Vector2.right*82);yield return new WaitForSecondsRealtime(.3f);
            if(finish){TouchQueue(71,UnityEngine.InputSystem.TouchPhase.Ended,center+Vector2.left*82);yield return new WaitForSecondsRealtime(.2f);}
        }
        IEnumerator TouchFrameSample(string name)
        {
            var times=new List<float>();var memory=Profiler.GetMonoUsedSizeLong();var collections=GC.CollectionCount(0);
            using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread"))
            using(var alloc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame"))
            {
            long cpuTotal=0,allocTotal=0;var validSamples=0;
            for(var i=0;i<180;i++){yield return null;times.Add(Time.unscaledDeltaTime*1000);if(cpu.Valid&&alloc.Valid){cpuTotal+=cpu.LastValue;allocTotal+=alloc.LastValue;validSamples++;}}
            File.WriteAllText(Path.Combine(output,name+"-profiler.json"),"{\"validSamples\":"+validSamples+",\"mainThreadTotalNanoseconds\":"+cpuTotal+",\"gcAllocatedTotalBytes\":"+allocTotal+",\"device\":\"WINDOWS_NOT_ANDROID\"}");
            }
            times.Sort();File.WriteAllText(Path.Combine(output,name+"-desktop-frame-sample.json"),"{\"frames\":180,\"medianMs\":"+times[90].ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"p95Ms\":"+times[171].ToString(System.Globalization.CultureInfo.InvariantCulture)+",\"monoUsedDeltaBytes\":"+(Profiler.GetMonoUsedSizeLong()-memory)+",\"gcCollections\":"+(GC.CollectionCount(0)-collections)+",\"device\":\"WINDOWS_SIMULATED_TOUCH_NOT_ANDROID\"}");
        }
        IEnumerator AndroidTouchJourney()
        {
            musicVirtualFocus=true;TouchAudit("main-menu");yield return CrossEnter("hoi_an");yield return CloseEvent();
            if(Environment.GetCommandLineArgs().Contains("--android-touch-restart"))
            {Check(Map.View.InputEnabled&&FullView==null&&FullPilot==null,"Separate process reload restores node/save without panel or gesture locks");yield return Snapshot("TOUCH-restart");yield break;}
            Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(new Vector2(.5f,.5f));yield return new WaitForSecondsRealtime(.3f);
            var name=TouchBaseline?"before":"after";var original=JsonUtility.ToJson(game.Save);
            yield return Click("AudioSettings",true);var zoom=Map.View.Zoom;var position=Map.View.Content.anchoredPosition;
            yield return TouchPair(TouchPoint(Map.View.Viewport)+Vector2.up*120);
            TouchObserve(Mathf.Abs(Map.View.Zoom-zoom)<.001f&&Vector2.Distance(position,Map.View.Content.anchoredPosition)<.1f,"Settings overlay owns both touches; map cannot pinch underneath");
            yield return Snapshot("TOUCH-"+name+"-settings");yield return Click("AudioSettingsClose",true);Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(new Vector2(.5f,.5f));
            yield return TouchPair(TouchPoint(Map.View.Viewport),false);
            TouchObserve(Map.View.SuppressSelection,"Remaining finger stays consumed after a pinch, even beyond the old 150ms timeout");
            TouchQueue(71,UnityEngine.InputSystem.TouchPhase.Ended,TouchPoint(Map.View.Viewport)+Vector2.left*82);yield return new WaitForSecondsRealtime(.25f);
            TouchObserve(Map.View.WithinBounds()&&Map.View.Zoom<=MapPanZoom.MaxZoom&&Map.View.Zoom>=MapPanZoom.MinZoom,"Repeated pinch stays within map bounds");
            yield return Snapshot("TOUCH-"+name+"-map");yield return TouchFrameSample(name);
            TouchObserve(JsonUtility.ToJson(game.Save)==original,"Pure camera/settings gestures preserve complete gameplay save");
            File.WriteAllLines(Path.Combine(output,"touch-findings.txt"),touchFindings);
            if(TouchBaseline)yield break;
            yield return AndroidTouchExtended();
        }
        IEnumerator AndroidTouchExtended()
        {
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(432,960)})
            {
                yield return PilotResolution(size.x,size.y);Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(new Vector2(.5f,.5f));
                for(var i=0;i<4;i++){yield return TouchPair(TouchPoint(Map.View.Viewport));Check(Map.View.WithinBounds(),"Repeated pinch clamps at both viewport limits");}
                var index=NodeIndex("H2");var node=Map.View.Content.Find("H2").GetComponent<RectTransform>();Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[index]));yield return null;
                var saved=JsonUtility.ToJson(game.Save);var at=TouchPoint(node);
                TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Began,at);yield return null;yield return null;
                Check(NodeMarker("H2").StateImage.transform.localScale.x>1,"Node has visual feedback while touch is still down, before movement");
                for(var i=1;i<=8;i++){TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Moved,at+Vector2.right*i*12);yield return null;}
                TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Ended,at+Vector2.right*96);yield return new WaitForSecondsRealtime(.2f);
                Check(!Map.Moving&&JsonUtility.ToJson(game.Save)==saved,"Drag starting on node ends outside without node selection/reward");
                Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[index]));at=TouchPoint(node);
                TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Began,at);yield return new WaitForSecondsRealtime(.8f);TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Ended,at);yield return null;yield return null;
                Check(!Map.Moving&&JsonUtility.ToJson(game.Save)==saved,"Long press on a node does not become an accidental travel tap");
                TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Began,at);yield return null;TouchQueue(75,UnityEngine.InputSystem.TouchPhase.Canceled,at);yield return null;yield return null;
                Check(!Map.Moving&&JsonUtility.ToJson(game.Save)==saved,"Canceled touch cannot select node or mutate save");
                yield return Click("AudioSettings",true);var requested=Application.targetFrameRate;yield return Click("FrameRate",true);Check(Application.targetFrameRate!=(requested==30?30:60),"30/60 frame preference responds through real UI");yield return Click("FrameRate",true);yield return Snapshot("TOUCH-settings-"+size.x);yield return Click("AudioSettingsClose",true);
                TouchAudit("map-"+size.x);
            }
            // Real one-finger camera input while Lumi is travelling; a second
            // finger on HUD must not become a map pinch or a button activation.
            Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[NodeIndex("H16")]));yield return null;yield return Click("H16",true);
            Check(Map.Moving,"Touch starts a live route for pan/retarget acceptance");
            var panAt=TouchPoint(Map.View.Viewport);TouchQueue(76,UnityEngine.InputSystem.TouchPhase.Began,panAt);yield return null;yield return null;
            for(var i=1;i<=8;i++){TouchQueue(76,UnityEngine.InputSystem.TouchPhase.Moved,panAt+new Vector2(i*9,i*3));yield return null;}
            var heldAt=Map.View.Content.anchoredPosition;Map.View.EnsureVisible(new Vector2(.05f,.95f));
            Check(Vector2.Distance(heldAt,Map.View.Content.anchoredPosition)<.1f,"Lumi autofollow cannot override active one-finger pan");
            yield return Pointer(TouchPoint((RectTransform)FindButton("AudioSettings").transform),true);
            Check(GameObject.Find("AudioSettingsOverlay")==null,"Second finger on HUD during map drag is consumed, not an accidental button click");
            TouchQueue(76,UnityEngine.InputSystem.TouchPhase.Ended,panAt+new Vector2(72,24));yield return new WaitForSecondsRealtime(.2f);
            var redirects=0;
            foreach(var target in new[]{"H4","H10","H2"})
            {
                Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[NodeIndex(target)]));yield return null;
                if(Map.Moving)redirects++;yield return Click(target,true);
            }
            yield return Until(()=>!Map.Moving,"last retarget arrival",35);yield return CloseEvent();
            Check(redirects>0&&game.Save.node==NodeIndex("H2"),"Repeated live destination changes reach the last canonical target through existing junction planning");
            yield return Snapshot("TOUCH-pan-retarget-arrival");
            // Geometry remains mathematically identical; measure the changed hot path.
            var worst=game.Content.cities.SelectMany(c=>GameData.Load(c.id).navigation.edges).OrderByDescending(e=>e.points.Length).First();var points=new List<Vector2>(worst.points);var mapSize=Map.View.Content.rect.size;var distances=CityRouteGeometry.Measure(points,mapSize);
            var maxError=0f;for(var i=0;i<=1000;i++)maxError=Mathf.Max(maxError,Vector2.Distance(CityRouteGeometry.Sample(points,mapSize,i/1000f),CityRouteGeometry.SampleMeasured(points,distances,i/1000f)));
            Check(maxError<.00001f,"Measured sampler preserves existing polyline at 1001 progress values");
            var timer=System.Diagnostics.Stopwatch.StartNew();Vector2 sink=Vector2.zero;
            for(var i=0;i<20000;i++)sink+=CityRouteGeometry.Sample(points,mapSize,(i%1000)/1000f);timer.Stop();var oldTicks=timer.ElapsedTicks;timer.Restart();
            for(var i=0;i<20000;i++)sink+=CityRouteGeometry.SampleMeasured(points,distances,(i%1000)/1000f);timer.Stop();
            File.WriteAllText(Path.Combine(output,"geometry-desktop-benchmark.json"),"{\"calls\":20000,\"oldTicks\":"+oldTicks+",\"newTicks\":"+timer.ElapsedTicks+",\"frequency\":"+System.Diagnostics.Stopwatch.Frequency+",\"source\":\"same Windows Player function benchmark, not device FPS\"}");
            var visited=new HashSet<string>();
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();
                foreach(var node in game.Data.nodes.Where(n=>game.Discovery.IsVisible(NodeIndex(n.id))&&game.Data.Unlocked(n.requirement,game.Save)).Take(4).ToArray())
                {Time.timeScale=4;yield return ChapterGo(node.id);Time.timeScale=1;yield return CloseEvent();visited.Add(node.id);}
            }
            Check(visited.Count>=20,"At least 20 distinct canonical nodes accepted real touch travel/interact without changing graph");
            yield return Click("Inventory",true);TouchAudit("inventory");yield return Click("CloseInventory",true);yield return Click("QuestJournal",true);TouchAudit("journal");yield return Click("OpenRelicCollection",true);TouchAudit("collection");yield return CloseEvent();
            foreach(var entry in LeisurePresentation.Load().entries.Where(e=>e.enabled))
            {
                yield return CrossEnter(entry.cityId);Time.timeScale=4;yield return ChapterGo(entry.nodeId);Time.timeScale=1;yield return CloseEvent();yield return LeisureFullOpen(entry);TouchAudit("leisure-"+entry.nodeId);
                var saved=JsonUtility.ToJson(game.Save);yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.15f);yield return CloseEvent();
                Check(Map.View.InputEnabled&&JsonUtility.ToJson(game.Save)==saved,"Touch open/start/close preserves save for leisure "+entry.nodeId);
            }
            KeepSave("Touch-accepted-earned");
        }
        void TouchAudit(string panel)
        {
            Canvas.ForceUpdateCanvases();var rows=new List<string>{"panel,name,widthPixels,heightPixels,x,y"};
            foreach(var b in FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if(!b.gameObject.activeInHierarchy||!b.interactable||b.GetComponent<MapNodeHitTarget>()!=null)continue;
                var rect=(RectTransform)b.transform;var corners=new Vector3[4];rect.GetWorldCorners(corners);var w=Vector3.Distance(corners[0],corners[3]);var h=Vector3.Distance(corners[0],corners[1]);
                rows.Add(panel+","+b.name+","+w+","+h+","+rect.position.x+","+rect.position.y);
            }
            File.WriteAllLines(Path.Combine(output,"touch-targets-"+panel+".csv"),rows);
        }
    }
}
#endif
