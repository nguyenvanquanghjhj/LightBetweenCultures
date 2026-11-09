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
        [Serializable] sealed class LeisureFullFrame
        {
            public string file,utc,id,node,kind;public int width,height;public float elapsed,wingAngle,actorAngle;
            public Vector3 actor,backdrop,beak,contact;public Vector2 sceneSize;public float mouthDistancePixels;
        }
        readonly List<LeisureFullFrame> leisureFullFrames=new List<LeisureFullFrame>();
        LeisurePilotView FullPilot=>FindAnyObjectByType<LeisurePilotView>();
        LeisureActivityView FullView=>FindAnyObjectByType<LeisureActivityView>();
        bool FullPlaying=>FullPilot!=null?FullPilot.Playing:FullView.Playing;
        float FullElapsed=>FullPilot!=null?FullPilot.Elapsed:FullView.Elapsed;
        RectTransform FullRider=>FullPilot!=null?FullPilot.Rider:FullView.Rider;
        RectTransform FullBackdrop=>FullPilot!=null?FullPilot.Backdrop:FullView.Backdrop;
        IEnumerator LeisureFullShot(string name,LeisureDefinition entry,string kind,bool motion=false)
        {
            yield return new WaitForEndOfFrame();var path=Path.Combine(output,name+".png");Check(!File.Exists(path),"New current-run PNG, no reuse: "+name);
            var f=new LeisureFullFrame{file=name+".png",utc=DateTime.UtcNow.ToString("O"),id=entry.id,node=entry.nodeId,kind=kind,width=Screen.width,height=Screen.height};
            if(motion){f.elapsed=FullElapsed;f.actor=FullRider.position;f.actorAngle=Mathf.DeltaAngle(0,FullRider.localEulerAngles.z);f.backdrop=FullBackdrop.position;f.sceneSize=FullBackdrop.rect.size;var hand=FullRider.Find("LumiRightWing");f.wingAngle=hand!=null?Mathf.DeltaAngle(0,hand.localEulerAngles.z):0;}
            if(motion&&(entry.mechanic=="tea"||entry.mechanic=="snack"))
            {
                var beak=FullRider.Find("LumiMouthAnchor");var contact=FullRider.GetComponentsInChildren<RectTransform>().FirstOrDefault(r=>r.name=="MouthContact");
                Check(beak!=null&&contact!=null,"Cup/food has canonical beak and visible contact anchors: "+entry.nodeId);
                f.beak=beak.position;f.contact=contact.position;f.mouthDistancePixels=Vector3.Distance(f.beak,f.contact);
                if((entry.mechanic=="tea"&&Mathf.Abs(f.elapsed-2.4f)<.15f)||(entry.mechanic=="snack"&&Mathf.Abs(f.elapsed-.6f)<.15f))
                    Check(f.mouthDistancePixels<2,"Cup rim/food reaches Lumi's beak within two screen pixels: "+entry.nodeId);
            }
            ScreenCapture.CaptureScreenshot(path);yield return null;yield return Until(()=>File.Exists(path)&&new FileInfo(path).Length>2000,"Full review PNG written "+name,3);
            leisureFullFrames.Add(f);File.WriteAllText(Path.Combine(output,"frames.json"),"["+string.Join(",",leisureFullFrames.Select(x=>JsonUtility.ToJson(x)))+"]");
            captures.Add(f.utc+" | "+f.width+"x"+f.height+" | "+f.file);
        }
        IEnumerator LeisureFullOpen(LeisureDefinition e)
        {yield return Click("Interact",true);yield return ChapterChoice("leisure:"+e.id);yield return null;Canvas.ForceUpdateCanvases();}
        IEnumerator LeisureFullJourney()
        {
            musicVirtualFocus=true;
            var original=JsonUtility.ToJson(game.Save);var args=Environment.GetCommandLineArgs();
            if(args.Contains("--leisure-full-restart"))
            {
                var saved=JsonUtility.FromJson<SaveData>(original);yield return CrossEnter(saved.cityId);yield return CloseEvent();
                Check(FullPilot==null&&FullView==null&&Map.View.InputEnabled,"Real restart contains no leisure panel or animation/input lock");
                Check(game.Save.items.SequenceEqual(saved.items)&&game.Save.completed.SequenceEqual(saved.completed)&&game.Save.flags.SequenceEqual(saved.flags)&&game.Save.rewardGrantIds.SequenceEqual(saved.rewardGrantIds)&&game.Save.leisureCompleted.SequenceEqual(saved.leisureCompleted)&&game.Save.coins==saved.coins,"Real restart preserves rewards, quest, relic and optional history");yield break;
            }
            Check(game.Quests.IsCompleted("HQ01"),"Full leisure review uses earned private save, no injected unlock flags");
            var filter=args.FirstOrDefault(a=>a.StartsWith("--leisure-full-only="));var selected=filter==null?LeisurePresentation.Load().entries:LeisurePresentation.Load().entries.Where(e=>filter.Substring("--leisure-full-only=".Length).Split(',').Contains(e.nodeId)).ToArray();
            foreach(var e in selected)
            {
                yield return CrossEnter(e.cityId);Time.timeScale=5;yield return ChapterGo(e.nodeId);Time.timeScale=1;yield return CloseEvent();
                foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
                {
                    yield return PilotResolution(resolution.x,resolution.y);
                    var prop=GameObject.Find("LeisureProp_"+e.id).GetComponent<Image>();
                    Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus((CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node])+prop.rectTransform.anchorMin)/2);yield return new WaitForSecondsRealtime(.2f);
                    Check(prop.sprite==e.Sprite&&!prop.raycastTarget&&prop.GetComponent<Button>()==null&&prop.GetComponent<Collider2D>()==null,"Canonical prop is map decoration only: "+e.nodeId);
                    var prefix="FULL-"+e.nodeId+"-"+resolution.x;yield return LeisureFullShot(prefix+"-map",e,"map");yield return Click("Interact",true);
                    Check(game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="leisure:"+e.id&&o.enabled),"Existing node menu exposes correct enabled activity: "+e.nodeId);
                    yield return LeisureFullShot(prefix+"-menu",e,"menu");yield return ChapterChoice("leisure:"+e.id);yield return null;Canvas.ForceUpdateCanvases();
                    var saved=JsonUtility.ToJson(game.Save);var camera=Map.View.Content.anchoredPosition;var zoom=Map.View.Zoom;var at=Map.Marker.anchorMin;
                    var music=SoftAudio.Ensure();var track=music.MusicTrack;var starts=music.MusicStarts;
                    Check((FullPilot!=null||FullView!=null)&&!FullPlaying,"Correct separate leisure panel opens idle: "+e.nodeId);
                    var expected=e.mechanic=="swing"?AdventureUIArt.Load().leisureSwingBackground:e.mechanic=="boat"?AdventureUIArt.Load().leisureBoatBackground:AdventureUIArt.Load().LeisureBackground(e.id);
                    Check(FullBackdrop.GetComponent<Image>().sprite==expected&&Mathf.Abs(FullBackdrop.rect.width/FullBackdrop.rect.height-4/3f)<.001f,"Correct existing 4:3 backdrop binding: "+e.nodeId);
                    Check(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<Canvas>(FindObjectsInactive.Exclude,FindObjectsSortMode.None).Length==1,"One existing Canvas and EventSystem: "+e.nodeId);
                    P6Bounds("LeisureAction");P6Bounds("ActivityClose");P6Bounds("LeisureScene");
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.2f);Check(FullPlaying,"Touch starts real activity: "+e.nodeId);
                    yield return CloseEvent();Check(FullView==null&&FullPilot==null&&Map.View.InputEnabled&&JsonUtility.ToJson(game.Save)==saved,"Mid-animation close cancels driver and preserves complete save: "+e.nodeId);
                    yield return LeisureFullOpen(e);var background=FullBackdrop.position;var first=leisureFullFrames.Count;
                    var support=FullPilot!=null?FullPilot.FixedFrame:null;var supportAt=support!=null?support.position:Vector3.zero;
                    yield return Click("LeisureAction",true);
                    foreach(var target in e.mechanic=="pick"?new[]{.60f,1.90f,3.20f}:e.mechanic=="tea"?new[]{.60f,2.40f,4.0f}:new[]{.60f,2.30f,4.0f})
                    {
                        while(FullElapsed<target)yield return null;
                        Check(FullBackdrop.position==background,"Backdrop remains stationary during motion: "+e.nodeId);
                        if(support!=null)Check(support.position==supportAt&&Quaternion.Angle(support.localRotation,Quaternion.identity)<.001f,"Preserved pilot swing support stays fixed");
                        yield return LeisureFullShot(prefix+"-motion-"+target.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture),e,"motion",true);
                    }
                    var sequence=leisureFullFrames.Skip(first).ToArray();Check(sequence.Max(f=>Vector3.Distance(f.actor,sequence[0].actor))>.1f||sequence.Max(f=>Mathf.Abs(f.wingAngle-sequence[0].wingAngle))>2||sequence.Max(f=>Mathf.Abs(f.actorAngle-sequence[0].actorAngle))>.2f,"Lumi really moves/reacts, not a static background sticker: "+e.nodeId);
                    yield return Until(()=>!FullPlaying,"Short scene completes: "+e.nodeId,6);
                    Check(FindButton("LeisureAction").GetComponentInChildren<Text>().text=="Chơi lại"&&JsonUtility.ToJson(game.Save)==saved,"Completion offers replay without any reward/progression/save mutation: "+e.nodeId);
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.25f);Check(FullPlaying&&FullElapsed<1,"Replay restarts live motion: "+e.nodeId);yield return CloseEvent();
                    yield return LeisureFullOpen(e);Check(!FullPlaying,"Second reopen has clean idle state: "+e.nodeId);yield return CloseEvent();
                    Check(JsonUtility.ToJson(game.Save)==saved&&Map.View.InputEnabled&&Map.Marker.anchorMin==at&&Map.View.Content.anchoredPosition==camera&&Mathf.Abs(Map.View.Zoom-zoom)<.001f,"Returns to exact map/node/camera/input with no item or history changes: "+e.nodeId);
                    Check(music.MusicTrack==track&&music.MusicStarts==starts,"Leisure keeps exploration music running: "+e.nodeId);
                }
                KeepSave("FULL-"+e.nodeId+"-after");
            }
            KeepSave("FULL-final");Check(selected.Length==(filter==null?12:selected.Length),"All requested activities reviewed in actual Windows Player");
        }
    }
}
#endif
