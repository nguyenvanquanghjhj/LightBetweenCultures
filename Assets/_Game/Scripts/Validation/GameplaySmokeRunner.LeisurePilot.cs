#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        [Serializable] sealed class PilotFrame
        {
            public string file,utc,activity,moment;
            public int width,height;public float elapsed,angle;
            public Vector3 riderWorld,seatWorld,fixedWorld,backdropWorld,riderInSeat;
        }
        readonly List<PilotFrame> pilotFrames=new List<PilotFrame>();
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        delegate bool PilotWindowVisitor(IntPtr window,IntPtr parameter);
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool EnumWindows(PilotWindowVisitor visitor,IntPtr parameter);
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr window,out uint processId);
        [System.Runtime.InteropServices.DllImport("user32.dll",CharSet=System.Runtime.InteropServices.CharSet.Unicode)] static extern int GetClassName(IntPtr window,System.Text.StringBuilder name,int count);
#endif
        IEnumerator PilotResolution(int width,int height)
        {
            Screen.SetResolution(width,Mathf.Min(height,960),false);yield return new WaitForSecondsRealtime(.4f);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            // A hidden QA Player may have MainWindowHandle=0. Locate only its own
            // Unity window; never resize another application or change desktop settings.
            var pid=(uint)System.Diagnostics.Process.GetCurrentProcess().Id;
            EnumWindows((window,unused)=>
            {
                GetWindowThreadProcessId(window,out var owner);if(owner!=pid)return true;
                var name=new System.Text.StringBuilder(128);GetClassName(window,name,128);
                if(name.ToString()=="UnityWndClass"&&GetWindowRect(window,out var outer)&&GetClientRect(window,out var inner))
                    SetWindowPos(window,IntPtr.Zero,0,0,width+outer.right-outer.left-inner.right,height+outer.bottom-outer.top-inner.bottom,0x0014);
                return true;
            },IntPtr.Zero);
#endif
            yield return new WaitForSecondsRealtime(.7f);
            File.AppendAllText(Path.Combine(output,"resolution-diagnostics.txt"),"requested="+width+"x"+height+" actual="+Screen.width+"x"+Screen.height+"\n");
            Check(Screen.width==width&&Screen.height==height,"Native pilot framebuffer "+width+"x"+height);
        }
        IEnumerator PilotShot(string name,LeisurePilotView view=null,string moment="")
        {
            yield return new WaitForEndOfFrame();var path=Path.Combine(output,name+".png");
            Check(!File.Exists(path),"Pilot capture never overwrites earlier evidence: "+name);
            var frame=new PilotFrame{file=name+".png",utc=DateTime.UtcNow.ToString("O"),width=Screen.width,height=Screen.height,moment=moment};
            if(view!=null)
            {
                frame.activity=view.Boat?"boat":"swing";frame.elapsed=view.Elapsed;frame.angle=view.Boat?Mathf.DeltaAngle(0,view.SeatOrBoat.localEulerAngles.z):view.SwingAngle;
                frame.riderWorld=view.Rider.position;frame.seatWorld=view.SeatOrBoat.position;frame.backdropWorld=view.Backdrop.position;
                frame.fixedWorld=view.FixedFrame!=null?view.FixedFrame.position:Vector3.zero;frame.riderInSeat=view.SeatOrBoat.InverseTransformPoint(view.Rider.position);
            }
            ScreenCapture.CaptureScreenshot(path);yield return null;
            yield return Until(()=>File.Exists(path)&&new FileInfo(path).Length>2000,"Pilot PNG written: "+name,3);
            pilotFrames.Add(frame);File.WriteAllText(Path.Combine(output,"animation-frames.json"),"["+string.Join(",",pilotFrames.Select(f=>JsonUtility.ToJson(f)))+"]");
            captures.Add(frame.utc+" | "+frame.width+"x"+frame.height+" | "+frame.file);
        }
        IEnumerator PilotOpen(LeisureDefinition def)
        {yield return Click("Interact",true);yield return ChapterChoice("leisure:"+def.id);yield return null;Canvas.ForceUpdateCanvases();}
        IEnumerator SwingStandingJourney()
        {
            musicVirtualFocus=true;
            yield return CrossEnter("hoi_an");Time.timeScale=5;yield return ChapterGo("H9");Time.timeScale=1;yield return CloseEvent();
            yield return PilotResolution(720,1280);var def=LeisurePresentation.At("hoi_an","H9");
            Check(def.title=="Chơi xích đu","Only H9 activity wording changes to match standing pose");
            yield return PilotOpen(def);var view=FindAnyObjectByType<LeisurePilotView>();
            Check(view!=null&&!view.Boat&&!view.Playing,"Existing swing panel opens idle at H9; no boat QA");
            var saved=JsonUtility.ToJson(game.Save);var frameAt=view.FixedFrame.position;var backgroundAt=view.Backdrop.position;
            var riderLocal=view.Rider.localPosition;var size=view.Rider.rect.size;
            var canonical=DesignKitArt.Load().lumiExploration;
            Check(Mathf.Abs(size.x/size.y-canonical.rect.width/canonical.rect.height)<.001f,"Full-body canonical Lumi keeps original aspect ratio");
            Check(view.SeatOrBoat.Find("SwingSeatFront")==null,"Seat front occluder removed so body and feet are visible");
            Check(view.Rider.Find("CanonicalRider").GetComponent<LeisureSpriteLayer>().mainTexture==canonical.texture,"Existing full-body canonical texture is used");
            yield return Click("LeisureAction",true);
            var moments=new[]{"right-extreme","center","left-extreme"};var times=new[]{.55f,1.10f,1.65f};
            var contact=new[]{new Vector2(532,1129),new Vector2(806,1127)}; // Approved MapTraveler sole landmarks.
            for(var i=0;i<times.Length;i++)
            {
                while(view.Elapsed<times[i])yield return null;
                Check(view.Rider.parent==view.SeatOrBoat&&Vector3.Distance(view.Rider.localPosition,riderLocal)<.001f,"Standing Lumi stays rigidly attached to seat: "+moments[i]);
                Check(view.FixedFrame.position==frameAt&&Quaternion.Angle(view.FixedFrame.localRotation,Quaternion.identity)<.001f&&view.Backdrop.position==backgroundAt,"Support and backdrop stay fixed: "+moments[i]);
                foreach(var pixel in contact)
                {
                    var sourcePixel=pixel-new Vector2(canonical.rect.x,canonical.texture.height-canonical.rect.yMax);
                    var r=view.Rider.rect;var point=new Vector3(r.x+sourcePixel.x/canonical.rect.width*r.width,r.y+(1-sourcePixel.y/canonical.rect.height)*r.height);
                    var local=view.SeatOrBoat.InverseTransformPoint(view.Rider.TransformPoint(point));var unit=view.SeatOrBoat.rect.width/768;
                    var p=new Vector2(384+local.x/unit,409-local.y/unit);
                    Check(p.x>370&&p.x<440&&p.y>542&&p.y<554,"Both sole contacts remain on seat top at source pixels "+p+": "+moments[i]);
                }
                yield return PilotShot("SwingStand-720-"+moments[i],view,moments[i]);
            }
            Check(pilotFrames.Max(f=>f.angle)>9&&pilotFrames.Min(f=>f.angle)<-9&&Mathf.Abs(pilotFrames.First(f=>f.moment=="center").angle)<1,"Runtime evidence contains left, center and right swing positions");
            yield return Until(()=>!view.Playing,"Standing swing finishes normally",5);
            Check(JsonUtility.ToJson(game.Save)==saved,"Standing presentation grants nothing and preserves entire save");
            yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.2f);yield return CloseEvent();
            Check(Map.View.InputEnabled&&FindAnyObjectByType<LeisurePilotView>()==null&&JsonUtility.ToJson(game.Save)==saved,"Replay then mid-motion close returns cleanly to H9 without progression changes");
        }
        IEnumerator LeisurePilotJourney()
        {
            musicVirtualFocus=true;
            var source=JsonUtility.ToJson(game.Save);
            if(Environment.GetCommandLineArgs().Contains("--leisure-pilot-restart"))
            {
                var loaded=JsonUtility.FromJson<SaveData>(source);yield return CrossEnter(loaded.cityId);yield return CloseEvent();
                Check(FindAnyObjectByType<LeisurePilotView>()==null&&Map.View.InputEnabled,"Restart has no running leisure panel/input lock");
                Check(game.Save.node==loaded.node&&game.Save.items.SequenceEqual(loaded.items)&&game.Save.completed.SequenceEqual(loaded.completed)&&game.Save.flags.SequenceEqual(loaded.flags)&&game.Save.rewardGrantIds.SequenceEqual(loaded.rewardGrantIds)&&game.Save.coins==loaded.coins&&game.Save.leisureCompleted.SequenceEqual(loaded.leisureCompleted),"Restart retains earned location, inventory, proofs and optional history without grants");
                yield return PilotShot("Pilot-restart-map");yield break;
            }
            Check(game.Quests.IsCompleted("HQ01"),"Pilot uses private earned save, no injected unlock flags");
            foreach(var city in new[]{"hoi_an","hue"})
            {
                var node=city=="hoi_an"?"H9":"U0";
                yield return CrossEnter(city);Time.timeScale=5;yield return ChapterGo(node);Time.timeScale=1;yield return CloseEvent();
                var def=LeisurePresentation.At(city,node);Check(def!=null,"Pilot reuses existing node and leisure option: "+node);
                foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
                {
                    yield return PilotResolution(resolution.x,resolution.y);
                    Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));yield return new WaitForSecondsRealtime(.3f);
                    var prop=GameObject.Find("LeisureProp_"+def.id).GetComponent<Image>();
                    Check(prop.sprite==def.Sprite&&!prop.raycastTarget&&prop.GetComponent<Button>()==null&&prop.GetComponent<Collider2D>()==null,"Approved mini prop is decoration only: "+node);
                    var prefix="Pilot-"+node+"-"+resolution.x;yield return PilotShot(prefix+"-map");
                    yield return Click("Interact",true);
                    var option=game.Events.BuildOptions(game.Save.node).options.First(o=>o.id=="leisure:"+def.id);
                    Check(option.label.Contains(city=="hoi_an"?"Chơi xích đu":"Đạp thuyền vịt"),"Real node menu uses requested activity wording: "+node);
                    yield return PilotShot(prefix+"-menu");yield return ChapterChoice("leisure:"+def.id);yield return null;
                    var saved=JsonUtility.ToJson(game.Save);var zoom=Map.View.Zoom;var camera=Map.View.Content.anchoredPosition;var at=Map.Marker.anchorMin;
                    var music=SoftAudio.Ensure();var track=music.MusicTrack;var starts=music.MusicStarts;
                    var canvasCount=FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Length;
                    Check(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"One existing EventSystem; no duplicate activity input system");
                    var view=FindAnyObjectByType<LeisurePilotView>();Check(view!=null&&!view.Playing,"Activity opens idle; no autoplay reward");
                    Check(Mathf.Abs(view.Backdrop.rect.width/view.Backdrop.rect.height-4/3f)<.001f,"Backdrop preserves original 4:3 composition without stretch");
                    P6Bounds("LeisureAction");P6Bounds("ActivityClose");P6Bounds("LeisureScene");
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.30f);
                    Check(view.Playing,"Real touch starts pilot motion");yield return CloseEvent();yield return null;
                    Check(view==null&&Map.View.InputEnabled&&JsonUtility.ToJson(game.Save)==saved,"Mid-animation close destroys driver, unlocks map and preserves entire save");
                    yield return PilotOpen(def);view=FindAnyObjectByType<LeisurePilotView>();
                    var fixedAt=view.FixedFrame!=null?view.FixedFrame.position:Vector3.zero;
                    var backAt=view.Backdrop.position;var riderLocal=view.SeatOrBoat.InverseTransformPoint(view.Rider.position);
                    var start=view.Rider.position;var far=0f;var startIndex=pilotFrames.Count;
                    yield return Click("LeisureAction",true);
                    var moments=new[]{"begin","right-extreme","center","left-extreme","return"};var times=view.Boat?new[]{.0f,.75f,1.50f,2.25f,3.0f}:new[]{.0f,.55f,1.10f,1.65f,2.20f};
                    for(var i=0;i<times.Length;i++)
                    {
                        while(view.Elapsed<times[i])yield return null;
                        Check(Vector3.Distance(view.Backdrop.position,backAt)<.001f,"Background remains stationary: "+moments[i]);
                        Check(Vector3.Distance(view.SeatOrBoat.InverseTransformPoint(view.Rider.position),riderLocal)<.001f,"Lumi remains attached to the same seat/boat group: "+moments[i]);
                        if(!view.Boat)Check(Vector3.Distance(view.FixedFrame.position,fixedAt)<.001f&&Quaternion.Angle(view.FixedFrame.localRotation,Quaternion.identity)<.001f,"Swing support remains fixed: "+moments[i]);
                        far=Mathf.Max(far,Vector3.Distance(view.Rider.position,start));yield return PilotShot(prefix+"-"+moments[i],view,moments[i]);
                    }
                    Check(far>10,"Actual rider motion spans visible screen pixels, not just a status label");
                    var sample=pilotFrames.Skip(startIndex).ToArray();
                    if(!view.Boat)Check(sample.Max(f=>f.angle)>8&&sample.Min(f=>f.angle)<-8,"Swing evidence includes opposite pendulum extrema");
                    yield return Until(()=>!view.Playing,"Pilot finishes naturally in 4.4 seconds",6);
                    Check(FindButton("LeisureAction").GetComponentInChildren<Text>().text=="Chơi lại"&&JsonUtility.ToJson(game.Save)==saved,"Completion offers replay, commits no progression or reward");
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.3f);Check(view.Playing&&view.Elapsed<1,"Same-panel replay restarts the actual motion");
                    yield return CloseEvent();yield return PilotOpen(def);view=FindAnyObjectByType<LeisurePilotView>();
                    Check(!view.Playing,"Reopen after close has a clean idle animation");yield return CloseEvent();
                    Check(Map.View.InputEnabled&&!Map.DialogueOpen&&Map.Marker.anchorMin==at&&Mathf.Abs(Map.View.Zoom-zoom)<.001f&&Map.View.Content.anchoredPosition==camera,"Activity returns to same node, camera, zoom and input context");
                    Check(JsonUtility.ToJson(game.Save)==saved&&FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Length==canvasCount,"Close/replay/reopen preserves complete save and Canvas count");
                    Check(music.MusicTrack==track&&music.MusicStarts==starts,"Pilot does not change/restart existing exploration BGM");
                }
                var neighbor=game.Data.edges.Where(e=>e.from==game.Save.node||e.to==game.Save.node).Where(e=>game.Data.EdgeOpen(e,game.Save)).Select(e=>e.from==game.Save.node?e.to:e.from).First(game.Discovery.IsVisible);
                Time.timeScale=5;yield return Travel(neighbor,true);Time.timeScale=1;yield return CloseEvent();Check(!Map.Moving&&Map.View.InputEnabled,"Lumi still walks normally after activity panel closes: "+city);
                KeepSave("Pilot-"+city+"-after");
            }
            KeepSave("Pilot-final");
        }
    }
}
#endif
