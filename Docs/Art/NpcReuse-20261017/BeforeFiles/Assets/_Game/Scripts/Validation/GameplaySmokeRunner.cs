#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Explicit opt-in, real scene/UI journey. Keeps every save and capture outside player storage.
    public sealed partial class GameplaySmokeRunner : MonoBehaviour
    {
        private readonly List<string> results = new List<string>();
        private readonly List<string> captures = new List<string>();
        private Mouse mouse;
        private Touchscreen touch;
        private Keyboard keyboard;
        private string output, savePath;
        private bool fatal;
        private GameBootstrap game;
        private bool Restart => Environment.GetCommandLineArgs().Contains("--verify-gameplay-save")||Environment.GetCommandLineArgs().Contains("--verify-cross-city-save");

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (Environment.GetCommandLineArgs().Contains("--gameplay-smoke"))
                new GameObject("GameplaySmokeRunner").AddComponent<GameplaySmokeRunner>();
        }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            var specified = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--gameplay-output="));
            output = specified == null ? Path.GetFullPath(Path.Combine(Application.dataPath,"..","Validation","GameplayAB-"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"))) : specified.Substring("--gameplay-output=".Length);
            Directory.CreateDirectory(output);
            savePath = Path.Combine(output,Environment.GetCommandLineArgs().Contains("--regression-checkpoint")?"final-save.json":"gameplay-save.json");
            SaveSystem.OverridePath = savePath;
            NodeMarkerView.MotionPreferenceKey="LBC.QA.MarkerMotion";
            if(!Restart)PlayerPrefs.DeleteKey(NodeMarkerView.MotionPreferenceKey);
            Application.logMessageReceived += OnLog;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            foreach (var device in InputSystem.devices.ToArray())
                if (device is Mouse || device is Keyboard || device is Touchscreen) InputSystem.DisableDevice(device);
            mouse = InputSystem.AddDevice<Mouse>();
            touch = InputSystem.AddDevice<Touchscreen>();
            keyboard = InputSystem.AddDevice<Keyboard>();
            StartCoroutine(Guard(Run()));
        }

        private void OnLog(string message,string trace,LogType type)
        {
            if (type != LogType.Exception && type != LogType.Error && type != LogType.Assert) return;
            fatal = true; results.Add("ERROR: "+message+"\n"+trace);
        }

        private void Check(bool condition,string message)
        {
            if (!condition) throw new Exception("FAIL: "+message);
            results.Add("PASS: "+message); Debug.Log("GAMEPLAY PASS: "+message);
        }

        private IEnumerator Guard(IEnumerator routine)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(routine);
            while (stack.Count > 0)
            {
                object next = null; bool more = false;
                try { more = stack.Peek().MoveNext(); if (more) next = stack.Peek().Current; }
                catch (Exception e) { fatal = true; results.Add(e.ToString()); break; }
                if (!more) { stack.Pop(); continue; }
                if (next is IEnumerator nested) stack.Push(nested); else yield return next;
            }
            if(fatal&&!Application.isEditor){ScreenCapture.CaptureScreenshot(Path.Combine(output,"failure-runtime.png"));yield return new WaitForSecondsRealtime(.4f);}
            results.Insert(0,"UTC run: "+DateTime.UtcNow.ToString("O")+" | Unity "+Application.unityVersion+" | "+Screen.width+"x"+Screen.height);
            var crossRestart=Environment.GetCommandLineArgs().Contains("--verify-cross-city-save");
            File.WriteAllLines(Path.Combine(output,Restart?(crossRestart?"restart-"+CrossCheckpointName+"-results.txt":"restart-results.txt"):"smoke-results.txt"),results.Concat(new[]{fatal?"FAILED":"ALL CHECKS PASSED"}));
            File.WriteAllLines(Path.Combine(output,Restart?(crossRestart?"restart-"+CrossCheckpointName+"-captures.txt":"restart-captures.txt"):"capture-manifest.txt"),captures);
            Application.logMessageReceived -= OnLog;
            if(Restart){PlayerPrefs.DeleteKey(NodeMarkerView.MotionPreferenceKey);PlayerPrefs.Save();}
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(fatal?1:0);
#else
            Application.Quit(fatal?1:0);
#endif
        }

        private IEnumerator Until(Func<bool> condition,string label,float timeout=50)
        {
            var end = Time.realtimeSinceStartup + timeout;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > end)
                {
                    var detail="Timed out: "+label+"\nModal="+(Map!=null?Map.DialogueOpen.ToString():"no map")+" moving="+(Map!=null?Map.Moving.ToString():"")+" stage="+(game!=null?game.Discovery.Stage("hoi_an").ToString():"");
                    detail+="\nButtons: "+string.Join(",",FindObjectsByType<Button>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(b=>b.name+":"+b.gameObject.activeInHierarchy+":"+b.interactable));
                    File.WriteAllText(Path.Combine(output,"failure-ui-state.txt"),detail);
                    ScreenCapture.CaptureScreenshot(Path.Combine(output,"failure-runtime.png"));
                    yield return new WaitForSecondsRealtime(.3f);
                    throw new Exception(detail);
                }
                yield return null;
            }
            yield return null;
        }

        private Button FindButton(string name) => FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b => b.name == name && b.gameObject.activeInHierarchy);
        private string HoiAnButton => FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name.StartsWith("City")&&b.GetComponentsInChildren<Text>().Any(t=>t.text.Contains("Hội An"))).name;
        private IEnumerator Click(string name,bool useTouch=false)
        {
            // A receipt is a separate reading step. Dismiss it through its real
            // button before continuing the existing journey behind the modal.
            if(!name.StartsWith("Receipt")&&FindButton("ReceiptContinue")!=null)yield return Click("ReceiptContinue",useTouch);
            yield return Until(()=>FindButton(name)!=null && FindButton(name).interactable,"button "+name,8);
            var button=FindButton(name); Canvas.ForceUpdateCanvases();
            var rect=(RectTransform)button.transform;
            if(name=="EventClose")
            {
                var p=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
                var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);
                Debug.Log("Close pointer at "+p+" hits "+string.Join(",",hits.Select(h=>h.gameObject.name+" parent="+h.gameObject.transform.parent.name)));
            }
            yield return Pointer(RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),useTouch);
            if(PortraitQA&&name=="DeliveryCancel")yield return PortraitHandoff("return");
            if(name=="EventClose"||name=="BattleClose"||name=="PuzzleClose"||name=="CloseJournal"||name=="CloseInventory"||name=="InventoryAction"||name=="CheckInClose")
                yield return new WaitForSecondsRealtime(.3f);
        }

        private IEnumerator Pointer(Vector2 point,bool useTouch=false)
        {
            // A real mouse reaches a control before pressing it. Give the UI module
            // a motion frame when switching back from the preceding touch pointer.
            if(!useTouch)
            {
                InputSystem.QueueStateEvent(mouse,new MouseState{position=point});
                yield return null; yield return null;
            }
            if (useTouch) InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=point});
            else InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));
            yield return null; yield return null;
            if (useTouch) InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=point});
            else InputSystem.QueueStateEvent(mouse,new MouseState{position=point});
            yield return null; yield return null;
        }

        private IEnumerator Scene(string name)
        {
            yield return Until(()=>SceneManager.GetActiveScene().name==name && !GameBootstrap.Instance.Scenes.Busy,"scene "+name);
            yield return null; yield return null;
        }

        private IEnumerator Snapshot(string name)
        {
            if (Application.isEditor) yield break;
            yield return new WaitForSecondsRealtime(.25f);
            if(Environment.GetCommandLineArgs().Contains("--render-quality"))QualitySample(name);
            var path=Path.Combine(output,name+".png");
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(.35f);
            Check(File.Exists(path) && new FileInfo(path).Length>2000,"Fresh runtime capture written: "+name);
            using (var sha=System.Security.Cryptography.SHA256.Create())
                captures.Add(DateTime.UtcNow.ToString("O")+" | "+Screen.width+"x"+Screen.height+" | "+name+".png | SHA256 "+BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-",""));
        }

        private CityMap Map => FindAnyObjectByType<CityMap>();
        private IEnumerator CloseEvent()
        {
            if (FindButton("CheckInClose") != null) yield return Click("CheckInClose");
            if (FindButton("PuzzleClose") != null) yield return Click("PuzzleClose");
            if (FindButton("BattleClose") != null) yield return Click("BattleClose");
            if (FindButton("EventClose") != null) yield return Click("EventClose");
            if (FindButton("Continue") != null) yield return Click("Continue");
            yield return null;
            Check(Map==null||!Map.DialogueOpen,"Closing an event or battle returns map input to the player");
        }

        private IEnumerator Travel(int node,bool useTouch=false)
        {
            yield return CloseEvent();
            yield return Until(()=>Map!=null && !Map.Moving,"map ready");
            var id=game.Data.nodes[node].id;
            Check(game.Discovery.IsVisible(node),"Travel target "+id+" is discovered");
            Map.View.Focus(game.Data.nodes[node].position);
            yield return new WaitForSecondsRealtime(.2f);
            if (game.Save.node == node) yield return Click("Interact",useTouch);
            else yield return Click(id,useTouch);
            yield return Until(()=>SceneManager.GetActiveScene().name=="HoiAnDemo" || Map!=null&&!Map.Moving,"travel to "+id);
            Check(game.Save.node==node,"Lumi reaches "+id+" through the real map input");
            if(PortraitQA&&Map!=null&&Map.DialogueOpen)yield return ObservePortrait();
        }

        private void CheckStage(int stage,int[] expected,string label)
        {
            Check(game.Discovery.Stage("hoi_an")==stage,label+": discovery stage "+stage);
            var actual=Enumerable.Range(0,game.Data.nodes.Length).Where(i=>game.Discovery.IsVisible(i)).ToArray();
            Check(actual.SequenceEqual(expected.OrderBy(n=>n)),label+": exact visible nodes ["+string.Join(",",actual)+"]");
            var previews=Enumerable.Range(0,game.Data.nodes.Length).Where(i=>game.Discovery.DiscoveryState(i)==MapDiscoveryState.PREVIEW).ToArray();
            var expectedPreviews=stage==0?new[]{3,4}:stage==1?new[]{16,18}:stage==2?new[]{6,17}:stage==3?new[]{20}:Array.Empty<int>();
            Check(previews.SequenceEqual(expectedPreviews),label+": exact generic preview frontier ["+string.Join(",",previews)+"]");
            foreach (var index in Enumerable.Range(0,game.Data.nodes.Length).Except(actual))
            {
                Check(FindButton("H"+index)==null,label+": hidden H"+index+" has no active input target");
                Check(game.Data.FindPath(game.Save.node,index,game.Save)==null,label+": hidden H"+index+" is excluded from pathfinding");
            }
            foreach (var image in FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.name.StartsWith("Path_")))
            {
                var parts=image.name.Split('_');
                if (parts.Length<3 || !int.TryParse(parts[1],out var a) || !int.TryParse(parts[2],out var b)) continue;
                Check(game.Discovery.IsVisible(a)&&game.Discovery.IsVisible(b),label+": no rendered route enters an undiscovered node ("+a+"-"+b+")");
            }
            CheckMapActors(label);
            Check(Map.NodeMarkers.Count==game.Data.nodes.Length,label+": marker count follows node data without a fixed cap");
            foreach(var marker in Map.NodeMarkers)
            {
                var index=Array.FindIndex(game.Data.nodes,n=>n.id==marker.NodeId);
                var discovery=game.Discovery.DiscoveryState(index);
                Check(marker.gameObject.activeInHierarchy==(discovery!=MapDiscoveryState.HIDDEN),label+": marker layer obeys three discovery states for "+marker.NodeId);
                if(discovery==MapDiscoveryState.PREVIEW)
                    Check(marker.Type==NodeMarkerType.Normal&&!marker.Badge.gameObject.activeSelf&&!marker.HitArea.gameObject.activeSelf&&Mathf.Abs(marker.GetComponent<CanvasGroup>().alpha-.22f)<.001f,label+": preview "+marker.NodeId+" has only a 22-percent neutral ground ring");
            }
        }

        private NodeMarkerView NodeMarker(string id)=>Map.NodeMarkers.First(m=>m.NodeId==id);
        private void CheckMarker(string id,NodeMarkerType type,NodeMarkerEffect effect=NodeMarkerEffect.None)
        {
            var marker=NodeMarker(id);
            Check(marker.gameObject.activeInHierarchy&&marker.Type==type&&marker.Effect==effect,id+" marker reflects current gameplay: "+type+" / "+effect);
        }
        private IEnumerator MarkerPointer(string id,bool useTouch,bool expectDialogue=true)
        {
            var index=Array.FindIndex(game.Data.nodes,n=>n.id==id);Map.View.Focus(game.Data.nodes[index].position);
            yield return new WaitForSecondsRealtime(.25f);
            var badge=NodeMarker(id).Badge;Check(badge.gameObject.activeInHierarchy,"Semantic marker is visible and selectable at "+id);
            var point=RectTransformUtility.WorldToScreenPoint(null,NodeMarker(id).HitArea.position);
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
            Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<NodeMarkerView>()==NodeMarker(id),"Pointer reaches the separate semantic marker at "+id);
            yield return Pointer(point,useTouch);yield return Until(()=>!Map.Moving,"marker travel "+id);
            Check(game.Save.node==index&&Map.DialogueOpen==expectDialogue,"Marker forwards to the existing travel/event flow at "+id);
        }
        private IEnumerator TestMarkerPresentation()
        {
            Check(Map.NodeMarkers.All(m=>m.transform.parent.name=="NodeMarkerLayer"&&m.transform.parent.parent==Map.View.Content),"All marker visuals live in their own map layer");
            Check(Map.NodeMarkers.All(m=>m.GetComponentsInChildren<Text>(true).Length==0),"Node markers contain no text labels or font emoji");
            CheckMarker("H2",NodeMarkerType.NpcTalk,NodeMarkerEffect.Pulse);
            CheckMarker("H11",NodeMarkerType.Reward,NodeMarkerEffect.Glow);
            Check(NodeMarker("H2").BadgeColor.r>.9f&&NodeMarker("H2").BadgeColor.g>.9f,"NPC uses a white badge above its real actor");
            foreach(var index in new[]{3,4})
            {
                Map.View.Focus(game.Data.nodes[index].position);yield return new WaitForSecondsRealtime(.15f);
                yield return Pointer(RectTransformUtility.WorldToScreenPoint(null,NodeMarker("H"+index).transform.position),index==4);
                NodeMarker("H"+index).OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
                Check(game.Save.node==0&&!Map.Moving&&Map.SelectedNode!=index&&game.Events.BuildOptions(index).options.Count==0,"Preview H"+index+" blocks real pointer, selection, arrival and event choices");
            }
            game.World.Set("hoi_an.lamp_eye_found");
            Check(!game.Events.CanCheckIn(3)&&!game.Events.Execute(3,"checkin").changed,"A check-in cannot execute through a preview even with its item flag present");
            game.Save.flags.Remove("hoi_an.lamp_eye_found");
            NodeMarker("H5").OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
            Check(game.Save.node==0&&!Map.Moving,"Hidden marker cannot forward input even through a direct pointer callback");
            var before=JsonUtility.ToJson(game.Save);var resolver=new NodeMarkerResolver(game);
            for(var round=0;round<5;round++)for(var i=0;i<game.Data.nodes.Length;i++)resolver.Resolve(i);
            Check(JsonUtility.ToJson(game.Save)==before,"Repeated marker resolution cannot mutate quests, rewards, world flags or inventory");
            var scales=new List<float>();for(var sample=0;sample<4;sample++){scales.Add(NodeMarker("H2").Badge.localScale.x);yield return new WaitForSecondsRealtime(.12f);}
            Check(scales.Max()-scales.Min()>.0001f&&scales.Max()<1.061f,"NPC pulse animates gently without growing into a large label");
            var size=NodeMarker("H2").Badge.lossyScale.x*NodeMarker("H2").Badge.rect.width;
            Map.View.ZoomBy(1.25f);yield return null;yield return null;
            Check(Mathf.Abs(NodeMarker("H2").Badge.lossyScale.x*NodeMarker("H2").Badge.rect.width-size)<2,"Semantic badge stays compact when zoom changes");
            yield return Snapshot("MKR-01-Fresh-NPC-talk-and-hidden-boss");
            Map.View.ZoomBy(1.75f/Map.View.Zoom);Map.View.Focus(new Vector2(.38f,.5f));
            yield return Snapshot("EXP-01-Fresh-revealed-preview-hidden");
            yield return RecordFloating("H2","npc-floating");
            yield return TestMotionOptions();
        }

        private Image MapActor(string nodeId) => FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None).FirstOrDefault(i=>i.name=="MapActor_"+nodeId);
        private void CheckMapActors(string label)
        {
            var art=AdventureUIArt.Load();
            foreach(var actor in art.mapActors.Where(a=>a.cityId==game.Data.cityId))
            {
                var index=Array.FindIndex(game.Data.nodes,n=>n.id==actor.nodeId);
                var image=MapActor(actor.nodeId);
                var visible=game.Discovery.IsVisible(index)&&(!actor.hideWhenResolved||!game.Events.Resolved(game.Events.Definition(game.Data.nodes[index].eventId)));
                Check(image!=null&&image.sprite==art.ActorSprite(actor.artId),label+": "+actor.nodeId+" uses the configured character sprite "+actor.artId);
                Check(image.gameObject.activeInHierarchy==visible,label+": "+actor.nodeId+" actor obeys discovery and encounter resolution");
                Check(image.transform.parent.name==actor.nodeId&&image.raycastTarget&&image.GetComponentInParent<Button>(true)!=null,label+": "+actor.nodeId+" actor shares the existing node input target");
                var size=art.ActorSize(game.Data.cityId,actor);
                Check(image.rectTransform.sizeDelta==size&&Mathf.Abs(size.x/actor.size.x-size.y/actor.size.y)<.0001f,label+": "+actor.nodeId+" uses the shared scale without changing aspect ratio");
                Check(image.rectTransform.pivot==new Vector2(.5f,0)&&image.rectTransform.anchoredPosition==actor.offset,label+": "+actor.nodeId+" keeps its original ground/foot anchor");
                var hit=(RectTransform)image.transform.parent.Find("ActorHitArea");
                Check(hit!=null&&hit.rect.width>=52&&hit.rect.height>=52&&hit.gameObject.activeInHierarchy==visible,label+": "+actor.nodeId+" retains a 52-unit touch region without leaking into hidden stages");
                var shadow=image.transform.parent.Find("ActorShadow").GetComponent<RectTransform>();
                Check(Vector2.Distance(shadow.sizeDelta,new Vector2(size.x*.55f,9*art.ActorScale(game.Data.cityId,actor)))<.001f&&shadow.anchoredPosition==actor.offset,label+": "+actor.nodeId+" shadow follows the scaled visual at the same feet");
                Check(image.transform.parent.Find("ActorShadow").gameObject.activeInHierarchy==visible,label+": "+actor.nodeId+" shadow cannot leak into an undiscovered region");
            }
        }

        private IEnumerator ActorPointer(string nodeId,bool useTouch)
        {
            var actor=MapActor(nodeId);Check(actor!=null&&actor.gameObject.activeInHierarchy,"Clickable actor exists at "+nodeId);
            var index=Array.FindIndex(game.Data.nodes,n=>n.id==nodeId);
            Map.View.Focus(game.Data.nodes[index].position);yield return new WaitForSecondsRealtime(.2f);
            var point=RectTransformUtility.WorldToScreenPoint(null,actor.rectTransform.TransformPoint(actor.rectTransform.rect.center));
            var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
            Check(hits.Count>0&&hits[0].gameObject==actor.gameObject,"Character image is the actual pointer target at "+nodeId);
            yield return Pointer(point,useTouch);yield return Until(()=>!Map.Moving,"character click travel "+nodeId);
            Check(game.Save.node==index&&Map.DialogueOpen,"Clicking the "+nodeId+" character travels and opens its existing event");
        }

        [Serializable] private sealed class MotionSample
        {
            public int frame; public float time;
            public Vector2 visualOffset,hitCenter,nodePosition,markerAnchor,actorOffset;
        }
        [Serializable] private sealed class MotionRecording
        {
            public string nodeId;public float period,amplitude;public MotionSample[] frames;
        }
        private IEnumerator RecordFloating(string id,string folder)
        {
            var index=Array.FindIndex(game.Data.nodes,n=>n.id==id);
            Map.View.ZoomBy(1.75f/Map.View.Zoom);Map.View.Focus(game.Data.nodes[index].position);
            yield return new WaitForSecondsRealtime(.3f);
            Check(!Map.DialogueOpen&&!Map.Moving&&NodeMarkerView.MotionMode==MarkerMotionMode.Full,"Motion recording starts on a live idle map: "+id);
            var marker=NodeMarker(id);var actor=MapActor(id);var samples=new List<MotionSample>();
            var before=JsonUtility.ToJson(game.Save);var directory=Path.Combine(output,folder);Directory.CreateDirectory(directory);
            for(var i=0;i<80;i++)
            {
                yield return new WaitForEndOfFrame();
                samples.Add(new MotionSample{frame=i,time=Time.unscaledTime,visualOffset=marker.Badge.anchoredPosition,
                    hitCenter=marker.HitArea.position,nodePosition=game.Data.nodes[index].position,
                    markerAnchor=((RectTransform)marker.transform).anchoredPosition,actorOffset=actor.rectTransform.anchoredPosition});
                ScreenCapture.CaptureScreenshot(Path.Combine(directory,"frame-"+i.ToString("D4")+".png"));
                yield return new WaitForSecondsRealtime(.05f);
            }
            yield return new WaitForSecondsRealtime(.35f);
            var first=samples[0];var span=samples.Max(s=>s.visualOffset.y)-samples.Min(s=>s.visualOffset.y);
            Check(span>7.6f&&span<=8.01f,"Actual floating visual has four UI-pixel amplitude: "+id);
            Check(samples.All(s=>Vector2.Distance(s.hitCenter,first.hitCenter)<.001f&&s.nodePosition==first.nodePosition&&s.markerAnchor==first.markerAnchor&&s.actorOffset==first.actorOffset),"Ground anchor, actor and pointer target remain fixed throughout motion: "+id);
            Check(JsonUtility.ToJson(game.Save)==before,"Visual animation cannot mutate gameplay or rewards: "+id);
            Check(Directory.GetFiles(directory,"frame-*.png").Length==80,"80 fresh runtime frames were captured: "+id);
            File.WriteAllText(Path.Combine(directory,"motion.json"),JsonUtility.ToJson(new MotionRecording{nodeId=id,period=NodeMarkerView.FloatPeriod,amplitude=NodeMarkerView.FloatAmplitude,frames=samples.ToArray()},true));
        }
        private IEnumerator TestMotionOptions()
        {
            yield return Click("MarkerMotion");
            Check(NodeMarkerView.MotionMode==MarkerMotionMode.Reduced,"HUD motion control selects reduced motion");
            var positions=new List<float>();
            for(var i=0;i<22;i++){positions.Add(NodeMarker("H2").Badge.anchoredPosition.y);yield return new WaitForSecondsRealtime(.10f);}
            Check(positions.Max()-positions.Min()>2.5f&&positions.Max()-positions.Min()<2.81f,"Reduced motion visibly limits floating to 1.4 UI pixels");
            yield return Click("MarkerMotion",true);yield return null;yield return null;
            Check(NodeMarkerView.MotionMode==MarkerMotionMode.Off,"Touch input switches marker animation off");
            var position=NodeMarker("H2").Badge.anchoredPosition;yield return new WaitForSecondsRealtime(.45f);
            Check(NodeMarker("H2").Badge.anchoredPosition==position&&NodeMarker("H2").Badge.localScale==Vector3.one,"Off mode keeps the badge still and disables pulse");
            yield return Snapshot("EXP-Motion-off");
            yield return Click("MarkerMotion");
            Check(NodeMarkerView.MotionMode==MarkerMotionMode.Full,"HUD motion control restores full gentle animation");
        }
        private IEnumerator TestCheckIn()
        {
            yield return CloseEvent();Map.View.ZoomBy(1.75f/Map.View.Zoom);Map.View.Focus(game.Data.nodes[6].position);
            CheckMarker("H6",NodeMarkerType.Camera);
            Check(NodeMarker("H6").BadgeColor.r>.9f&&NodeMarker("H6").BadgeColor.g>.9f,"Camera landmark uses the requested white badge");
            yield return Snapshot("EXP-04-Camera-white-marker");
            yield return Travel(6);
            Check(GameObject.Find("CheckInPanel")!=null&&FindButton("CheckInRecord")!=null,"A revealed camera landmark offers an actual record interaction");
            var illustration=GameObject.Find("CheckInIllustration").GetComponent<RawImage>();var uv=illustration.uvRect;
            Check(illustration.texture==DesignKitArt.Load().hoiAnMap.texture&&uv.x>=0&&uv.y>=0&&uv.xMax<=1&&uv.yMax<=1,"Landmark illustration uses an undistorted detail from the original map");
            var panel=GameObject.Find("CheckInPanel").GetComponent<RectTransform>();
            Check(panel.anchorMax.y-panel.anchorMin.y<.60f,"Landmark panel preserves visible city scenery");
            yield return Snapshot("EXP-04-White-camera-checkin-offer");
            var coins=game.Inventory.Count("coin");var entry=game.Events.CheckInAt(6);
            yield return Click("CheckInRecord");
            Check(game.World.Has(entry.completionFlag)&&game.Inventory.Count("coin")==coins+entry.rewardCoins,"Recording a landmark sets its world flag and grants the configured reward once");
            yield return Snapshot("EXP-05-Checkin-recorded");yield return CloseEvent();
            CheckMarker("H6",NodeMarkerType.Completed);
            Check(!game.Events.Execute(6,"checkin").changed&&game.Inventory.Count("coin")==coins+entry.rewardCoins,"Repeated check-in cannot duplicate its reward");
            yield return Click("Interact");
            Check(GameObject.Find("CheckInPanel")!=null&&FindButton("CheckInRecord")==null,"A recorded landmark reopens for viewing without a second reward action");
            yield return CloseEvent();yield return Travel(7);yield return CloseEvent();
        }

        private IEnumerator FoodChoice(string id)
        {
            for(var i=0;FindButton("EventOption_"+id)==null&&i<8;i++)
            {Check(FindButton("EventMore")!=null,"Additional NPC options have an accessible page control");yield return Click("EventMore",true);}
            yield return Click("EventOption_"+id,true);
            if(PortraitQA&&FindButton("DeliveryConfirm")!=null)yield return PortraitHandoff("to-Lumi");
        }
        private IEnumerator TestSpecialtyItems()
        {
            const string chili="hoi_an_fresh_chili",onion="hoi_an_purple_shallot";
            Check(game.Inventory.Count(chili)==2&&game.Inventory.Count(onion)==2,"Real HQ02 hand-in grants two chili and two shallots using source IDs");
            yield return Travel(10);yield return FoodChoice("accept:ba_tu_foodprep");
            Check(game.Quests.IsActive("HA_FOOD_PREP")&&!game.Quests.CanComplete("HA_FOOD_PREP"),"Bà Tư accepts the food quest but refuses hand-in without sufficient spices and reserve");
            var coins=game.Save.coins;var savedItems=game.Save.items.ToArray();
            Check(!game.Events.Execute(10,"complete:ba_tu_foodprep").changed&&game.Save.items.SequenceEqual(savedItems)&&game.Save.coins==coins,"Incomplete direct hand-in cannot consume wrong quantities or grant rewards");
            yield return Snapshot("FOOD-01-NPC-request-missing-reserved-spice");
            yield return Travel(11);yield return Snapshot("FOOD-02-Spice-pickup-at-map-node");
            yield return FoodChoice("collect:food_spice_bundle");
            Check(game.Inventory.Count(chili)==3&&game.Inventory.Count(onion)==3&&game.World.Has("hoi_an.spice_bundle_picked"),"Actual map pickup grants one of each spice and persists its one-time flag");
            Check(GameObject.Find("ReceivedIcon_"+chili).GetComponent<Image>().sprite==AdventureUIArt.Load().Item(chili).sprite&&GameObject.Find("ReceivedIcon_"+onion)!=null,"Pickup receipt displays real icons, item names and quantities");
            yield return Snapshot("FOOD-03-Pickup-icons-and-quantities");yield return CloseEvent();
            Check(!game.Events.Execute(11,"collect:food_spice_bundle").changed&&game.Inventory.Count(chili)==3,"Map spice pickup cannot repeat");
            yield return Click("Inventory");
            var scroll=GameObject.Find("ItemViewport").GetComponent<ScrollRect>();scroll.verticalNormalizedPosition=0;yield return null;yield return null;
            Check(new[]{chili,onion}.All(id=>FindButton("Item_"+id).transform.Find("ItemIcon").GetComponent<Image>().sprite==AdventureUIArt.Load().Item(id).sprite),"Inventory shows the actual chili and shallot sprites");
            yield return Click("Item_"+chili);
            Check(GameObject.Find("SelectedItemDescription").GetComponent<Text>().text.Contains("Jeonju"),"Chili inventory description names its future gameplay destination");
            yield return Snapshot("FOOD-04-Inventory-chili-shallot");yield return Click("Item_"+onion,true);
            Check(GameObject.Find("SelectedItemDescription").GetComponent<Text>().text.Contains("Huế"),"Shallot inventory description names Huế");
            yield return Snapshot("FOOD-05-Inventory-shallot");yield return Click("CloseInventory");
            yield return Travel(10);Check(game.Quests.CanComplete("HA_FOOD_PREP"),"Map pickup supplies enough food for the local delivery and both reserved gifts");
            var tea=game.Inventory.Count("healing_tea");coins=game.Save.coins;
            yield return FoodChoice("complete:ba_tu_foodprep");
            Check(game.Quests.IsCompleted("HA_FOOD_PREP")&&game.Inventory.Count(chili)==1&&game.Inventory.Count(onion)==2&&game.Inventory.Count("healing_tea")==tea+1&&game.Save.coins==coins+8,"NPC consumes exactly two chili/one shallot and awards one tea/eight coins, retaining future supplies");
            yield return Snapshot("FOOD-06-Actual-turn-in-reward");yield return CloseEvent();
            var after=JsonUtility.ToJson(game.Save);
            Check(!game.Events.Execute(10,"complete:ba_tu_foodprep").changed&&JsonUtility.ToJson(game.Save)==after,"Completed food quest cannot consume or reward again");
            yield return Click("Interact");yield return FoodChoice("accept:food_hq06");yield return CloseEvent();
            yield return Travel(18);yield return FoodChoice("accept:food_hq05");yield return CloseEvent();
            Check(game.Quests.IsActive("HQ05")&&game.Quests.IsActive("HQ06")&&!game.Quests.CanComplete("HQ05")&&!game.Quests.CanComplete("HQ06")&&!game.Quests.CanAccept("HQ07"),"Cross-city food quests cannot pay at the giver city or unlock HQ07 before both real deliveries");
            Check(game.Quests.CanAccept("HQ03")&&!game.Inventory.Has("festival_friendship_postcard")&&!game.Inventory.Has("jeonju_recipe_card")&&!game.Inventory.Has("hue_market_note"),"Food quest does not suppress Ji-eun's original quest or grant future souvenirs");
            yield return Click("QuestJournal");yield return Snapshot("FOOD-07-Pending-cross-city-journal");yield return Click("CloseJournal");
            KeepSave("food-save");game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(game.Inventory.Count(chili)==1&&game.Inventory.Count(onion)==2&&game.World.Has("hoi_an.spice_bundle_picked")&&game.Quests.IsCompleted("HA_FOOD_PREP"),"Disk save/reload preserves food consumption, unique pickup and quest completion");
            // Explicit isolated lost-item recovery fixture, restored before final save.
            var real=JsonUtility.ToJson(game.Save);
            yield return Travel(10);game.Inventory.TryConsume(chili,game.Inventory.Count(chili));game.Persist();
            yield return CloseEvent();yield return Click("Interact");coins=game.Save.coins;
            yield return FoodChoice("recover:recover_hq05");
            Check(game.Inventory.Count(chili)==1&&game.Save.coins==coins&&!game.Events.Execute(10,"recover:recover_hq05").changed,"Lost-food recovery supplies one reserved chili once without currency farming (isolated fixture)");
            yield return Snapshot("FOOD-Recovery-fixture");yield return CloseEvent();
            File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            yield return Travel(7);yield return CloseEvent();
        }

        private void KeepSave(string name)
        {
            Check(game.Persist(),"Persist checkpoint "+name);
            File.Copy(savePath,Path.Combine(output,name+".json"),true);
        }

        private IEnumerator ReloadCheckpoint(string name)
        {
            yield return CloseEvent();
            File.Copy(Path.Combine(output,name+".json"),savePath,true);
            game.Reload(); game.Scenes.Go("CityMap"); yield return Scene("CityMap");
        }

        private IEnumerator SolvePuzzle(string id,string wrong="wrong-pattern")
        {
            yield return Click("EventOption_puzzle");
            yield return Until(()=>game.Puzzles.Active,"puzzle "+id);
            var puzzle=game.Puzzles.Definition;
            Check(puzzle.id==id,"Correct data-driven puzzle opens: "+id);
            var beforeItems=game.Save.items.Count;
            var input=GameObject.Find("PuzzleAnswer").GetComponent<InputField>();
            if (FindButton("PuzzleOption_0")!=null)
            {
                var first=puzzle.answer.Split('|')[0];
                var incorrect=Array.FindIndex(puzzle.options,option=>option!=first);
                yield return Click("PuzzleOption_"+incorrect,true);
            }
            else input.text=wrong;
            yield return Click("PuzzleSubmit",true);
            Check(game.Puzzles.Active&&!game.Save.solvedPuzzles.Contains(id)&&game.Save.items.Count==beforeItems,"Wrong answer grants no progress or item and permits retry: "+id);
            yield return Snapshot("puzzle-"+id+"-retry");
            if (FindButton("PuzzleOption_0")!=null)
            {
                yield return Click("PuzzleClear");
                foreach (var symbol in puzzle.answer.Split('|'))
                    yield return Click("PuzzleOption_"+Array.IndexOf(puzzle.options,symbol),true);
            }
            else input.text=puzzle.answer;
            yield return Click("PuzzleSubmit");
            Check(game.Save.solvedPuzzles.Contains(id),"Correct UI submission persists solved puzzle: "+id);
            yield return Snapshot("puzzle-"+id+"-solved");
            yield return CloseEvent();
            var inventory=game.Save.items.Count;
            Check(!game.Puzzles.Start(id)&&game.Save.items.Count==inventory,"Solved puzzle cannot pay again: "+id);
        }

        private IEnumerator TestMapInput()
        {
            var map=Map; var view=map.View; var current=game.Save.node;
            var center=RectTransformUtility.WorldToScreenPoint(null,view.Viewport.TransformPoint(view.Viewport.rect.center));
            var zoom=view.Zoom;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=center,scroll=new Vector2(0,120)});
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=center}); yield return null;
            Check(view.Zoom>zoom,"Mouse wheel zoom still works with event/discovery system");
            var beforePinch=view.Zoom;
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=11,phase=UnityEngine.InputSystem.TouchPhase.Began,position=center+Vector2.left*40});
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=12,phase=UnityEngine.InputSystem.TouchPhase.Began,position=center+Vector2.right*40});
            yield return null; yield return null;
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=11,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=center+Vector2.left*80});
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=12,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=center+Vector2.right*80});
            yield return null; yield return null;
            Check(view.Zoom>beforePinch,"Two-finger pinch still zooms the map");
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=11,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=center+Vector2.left*80});
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=12,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=center+Vector2.right*80});
            yield return new WaitForSecondsRealtime(.25f);
            var pan=view.Content.anchoredPosition;
            var start=center+new Vector2(-60,-70);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left)); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start+new Vector2(-80,50)}.WithButton(MouseButton.Left)); yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start+new Vector2(-80,50)}); yield return new WaitForSecondsRealtime(.25f);
            Check(Vector2.Distance(pan,view.Content.anchoredPosition)>15,"Dragging still pans actual map content");
            Check(game.Save.node==current&&!map.Moving,"Pinch and pan cannot trigger hidden node events or movement");
            view.ZoomBy(100); view.PanBy(new Vector2(100000,-100000));
            Check(view.WithinBounds()&&Mathf.Abs(view.Zoom-MapPanZoom.MaxZoom)<.001f,"Map clamps extreme pan and maximum zoom");
            yield return Click("MapOverview");
            Check(Mathf.Abs(view.Content.rect.width/view.Content.rect.height-4f/3)<.001f,"Map A keeps its original illustration aspect ratio");
        }

        private IEnumerator ExploreH3()
        {
            if (SceneManager.GetActiveScene().name!="HoiAnDemo") yield return Click("EventOption_scene");
            yield return Scene("HoiAnDemo");
            var player=FindAnyObjectByType<HoiAnPlayerController>(); var item=FindAnyObjectByType<HoiAnItem>(); var art=DesignKitArt.Load();
            Check(player!=null&&item!=null&&item.gameObject.activeSelf,"H3 opens the existing physical exploration scene and unique item");
            Check(player.GetComponent<BoxCollider2D>()!=null&&player.transform.Find("CanonicalLumiVisual").GetComponent<SpriteRenderer>().sprite==art.lumiExploration,"Exploration keeps its collider and dedicated standing Lumi sprite");
            yield return Snapshot("05-H3-exploration");
            Check(!player.MoveTo(new Vector2(-5,1.8f)),"Original house collider rejects an obstructed destination");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W)); yield return new WaitForSecondsRealtime(.8f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return null;
            Check(player.Position.y<.14f&&player.Position.y>-.6f,"WASD movement still stops at the house collider");
            Check(player.MoveTo(new Vector2(-2,2)),"Physical pathfinding still finds a route around the house");
            yield return Until(()=>!player.Walking,"H3 route around house");
            Check(Vector2.Distance(player.Position,new Vector2(-2,2))<.15f,"Player physically traverses the path around the obstacle");
            var target=player.Position+Vector2.right*.65f;
            yield return Pointer(Camera.main.WorldToScreenPoint(target),true); yield return Until(()=>!player.Walking,"H3 touch movement");
            Check(Vector2.Distance(player.Position,target)<.15f,"H3 touchscreen ground movement works after new services");
            Check(player.MoveTo((Vector2)item.transform.position+Vector2.left*.8f),"Existing H3 path reaches pickup range");
            yield return Until(()=>!player.Walking,"reach unique H3 part");
            if(Environment.GetCommandLineArgs().Contains("--h3-qa"))yield return Snapshot("H3-near-eye-before-pickup");
            if(Environment.GetCommandLineArgs().Contains("--h3-desktop"))
            {
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));yield return new WaitForSecondsRealtime(.12f);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return new WaitForSecondsRealtime(.12f);
            }
            else yield return Click("Collect",true);
            Check(!item.gameObject.activeSelf&&game.Inventory.Has(game.Data.quest.item),"H3 pickup through E or the existing touch Collect button awards the tutorial part once");
            Check(!game.Quests.CollectPart(),"Second physical quest pickup is rejected");
            yield return Snapshot("06-H3-part-picked-up");
            yield return Click("Return"); yield return Scene("CityMap");
            Check(game.Save.node==3,"Returning from exploration restores H3 exactly");
        }

        private IEnumerator TestWorldNavigation()
        {
            var art=AdventureUIArt.Load();
            Check(art!=null&&art.cities.Length==6&&art.cities.All(c=>c.sprite!=null),"Six destination cards use the supplied clean Map A illustrations");
            var globe=FindButton("Back").transform.Find("Icon").GetComponent<Image>();
            Check(globe!=null&&globe.sprite==art.globe,"City World button uses the painted Earth globe, independent of text/font glyphs");
            Check(FindButton("QuestJournal").transform.Find("Icon").GetComponent<Image>().sprite==art.journal,"Quest HUD reuses the existing clean scroll artwork rather than a Unicode character");
            Check(art.window.border.x>0&&art.buttonTeal.border.x>0&&art.slot.border.x>0,"Shared illustrated skin uses imported nine-slice borders");
            var progressText=GameObject.Find("ChapterProgress").GetComponent<Text>();
            Check(progressText.rectTransform.rect.width>200&&progressText.rectTransform.rect.height>=progressText.preferredHeight,"HUD chapter progress has a visible, readable text region");
            foreach(var name in new[]{"Inventory","QuestJournal","Back"})
            {
                var caption=FindButton(name).transform.Find("Label").GetComponent<Text>();
                Check(caption.rectTransform.rect.height>=caption.preferredHeight,"HUD caption fits without auto-shrinking: "+name);
            }
            Check(FindButton("Inventory").transform.Find("Icon").GetComponent<Image>().sprite==art.backpack,"Inventory HUD button uses the clean extracted leather backpack sprite");
            var node=game.Save.node;var visited=game.Save.visited.ToArray();
            yield return Snapshot("UI-01-CityMap-globe-backpack");
            yield return Click("MapOverview");
            Check(GameObject.Find("MapViewport").GetComponent<Image>().sprite==art.backdrop&&GameObject.Find("OverviewHint")!=null,"Overview letterboxing uses illustrated travel paper while retaining Map A aspect ratio");
            yield return Snapshot("UI1-01b-CityMap-overview-paper");
            yield return Click("Back",true);yield return Scene("WorldMap");
            Check(GameObject.Find("DestinationDetails")!=null&&GameObject.Find("CityPreview")!=null,"Globe button opens the separate visual World Map and destination detail view");
            yield return Snapshot("UI-02-WorldMap-from-globe");
            yield return Click("City1",true);
            Check(GameObject.Find("DestinationName").GetComponent<Text>().text=="Huế"&&!FindButton("EnterCity").interactable,"Locked city remains selectable for preview but its Enter control is unavailable");
            Check(game.Save.cityId=="hoi_an"&&game.Save.node==node,"Previewing another city cannot switch the current city or saved position");
            yield return Snapshot("UI-WorldMap-locked-Hue");
            yield return Click("Back");yield return Scene("CityMap");
            Check(game.Save.node==node&&game.Save.visited.SequenceEqual(visited)&&game.Discovery.Stage("hoi_an")==0,"Returning from World Map preserves exact location, visits and discovery progress");
        }

        private IEnumerator TestInventoryPresentation()
        {
            var art=AdventureUIArt.Load();var items=game.Save.items.ToArray();var node=game.Save.node;var hp=game.Save.playerHP;
            Check(game.Content.items.All(i=>art.Item(i.id)?.sprite!=null)&&art.items.Select(i=>i.id).Distinct().Count()==art.items.Length,"Every catalog item has an icon/description mapping with unique presentation IDs");
            yield return Click("Inventory",true);
            Check(GameObject.Find("InventoryPanel").GetComponent<RectTransform>().rect.height<GameObject.Find("HUD").GetComponent<UIController>().Root.rect.height*.72f,"Small inventory uses a compact paper window rather than a nearly full-screen empty modal");
            Check(Map.DialogueOpen&&!FindButton("H2").interactable,"Opening backpack blocks map selection while preserving the City Map underneath");
            var lamp=FindButton("Item_lamp_eye");var tea=FindButton("Item_healing_tea");
            Check(lamp!=null&&tea!=null&&lamp.transform.Find("ItemIcon").GetComponent<Image>().sprite==art.Item("lamp_eye").sprite,"Inventory shows actual owned item slots with illustrated icons");
            Check(lamp.transform.Find("QuestItemBadge")!=null&&tea.transform.Find("QuestItemBadge")==null,"Quest components and consumables have distinct item markings");
            Check(game.Inventory.Has("lantern-part")&&FindButton("Item_lantern-part")==null,"Legacy tutorial alias stays in save for compatibility but does not duplicate the same physical pickup in the visual bag");
            yield return Click("Item_healing_tea",true);
            Check(GameObject.Find("SelectedItemName").GetComponent<Text>().text=="Trà Hồi Phục"&&GameObject.Find("SelectedItemDescription").GetComponent<Text>().text.Contains("HP"),"Touching an item displays its name and gameplay description");
            Check(GameObject.Find("SelectedItemCount").GetComponent<Text>().text.EndsWith(game.Inventory.Count("healing_tea").ToString()),"Selected quantity comes from the live gameplay inventory");
            yield return Snapshot("UI-03-Inventory-from-backpack");
            yield return Click("Item_lamp_eye");
            Check(GameObject.Find("SelectedItemIcon").GetComponent<Image>().sprite==art.Item("lamp_eye").sprite,"Selecting another slot replaces the detail illustration");
            yield return Click("CloseInventory");
            Check(!Map.DialogueOpen&&game.Save.items.SequenceEqual(items)&&game.Save.node==node&&game.Save.playerHP==hp,"Browsing and closing the visual inventory does not consume items or move Lumi");
        }

        private IEnumerator TestEarnedInventory()
        {
            yield return CloseEvent();yield return Click("Inventory",true);
            Check(new[]{"lamp_eye","hanji_core","healing_tea"}.All(id=>FindButton("Item_"+id)!=null),"Three distinct earned items appear together after the real H3/H13 quest interactions");
            yield return Click("Item_hanji_core",true);yield return Snapshot("UI1-03-Inventory-three-earned-items");
            var before=game.Save.items.ToArray();
            yield return Click("InventoryAction",true);
            Check(GameObject.Find("QuestJournalPanel")!=null&&game.Save.items.SequenceEqual(before),"Quest item action opens the existing journal without completing quests or consuming components");
            yield return Click("CloseJournal");
        }

        private IEnumerator TestUI1LayoutsAndScroll()
        {
            yield return CloseEvent();
            var savedItems=game.Save.items.ToList();var node=game.Save.node;
            // Temporary UI fixture exercises a long bag; mandatory art captures use earned items above.
            foreach(var item in game.Content.items.Where(i=>i.id!="coin"))if(!game.Inventory.Has(item.id))game.Inventory.Add(item.id);
            yield return Click("Inventory",true);
            var scroll=GameObject.Find("ItemViewport").GetComponent<ScrollRect>();Canvas.ForceUpdateCanvases();
            Check(scroll.content.rect.height>scroll.viewport.rect.height+100,"A long catalog inventory retains a genuinely scrollable grid");
            var center=RectTransformUtility.WorldToScreenPoint(null,scroll.viewport.TransformPoint(scroll.viewport.rect.center));
            var start=center+Vector2.down*70;
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=41,phase=UnityEngine.InputSystem.TouchPhase.Began,position=start});yield return null;yield return null;
            for(var step=1;step<=8;step++){InputSystem.QueueStateEvent(touch,new TouchState{touchId=41,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=start+Vector2.up*(step*22)});yield return null;}
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=41,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=start+Vector2.up*176});yield return new WaitForSecondsRealtime(.2f);
            Check(scroll.verticalNormalizedPosition<.92f&&game.Save.node==node&&!Map.Moving,"Touch drag scrolls the inventory instead of moving Lumi on the underlying map");
            yield return Snapshot("UI1-Inventory-scroll-fixture");yield return Click("CloseInventory");
            game.Save.items=savedItems;
            Screen.SetResolution(540,1200,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
            var hud=GameObject.Find("HUD").GetComponent<UIController>();
            UIController.SetRect(hud.Root,new Vector2(.02f,.035f),new Vector2(.98f,.94f));Canvas.ForceUpdateCanvases();yield return null;
            yield return Snapshot("UI1-Phone-20x9-safe-area-simulation");
            yield return Click("Inventory",true);
            var panel=GameObject.Find("InventoryPanel").GetComponent<RectTransform>();var corners=new Vector3[4];panel.GetWorldCorners(corners);
            Check(corners.All(c=>RectTransformUtility.RectangleContainsScreenPoint(hud.Root,c,null)),"Illustrated inventory stays inside a simulated phone safe area at 20:9");
            Check(panel.GetComponentsInChildren<Text>().All(t=>!t.resizeTextForBestFit&&t.fontSize>=14),"Inventory text uses fixed readable sizes and never auto-shrinks below 14 UI units");
            yield return Snapshot("UI1-Inventory-safe-area-simulation");yield return Click("CloseInventory");
            UIController.SetRect(hud.Root,Vector2.zero,Vector2.one);
            Screen.SetResolution(960,540,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);yield return Click("Inventory");
            Check(GameObject.Find("ItemGrid").GetComponent<GridLayoutGroup>().constraintCount==5,"Wider layouts reuse the same inventory with five columns");
            var widePanel=GameObject.Find("InventoryPanel").GetComponent<RectTransform>();
            Check(GameObject.Find("SelectedItemDescription").GetComponent<Text>().fontSize*widePanel.lossyScale.y>=13,"Landscape inventory keeps description text at a readable rendered size");
            yield return Snapshot("UI1-Inventory-landscape");yield return Click("CloseInventory");
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
            Check(game.Save.node==node&&game.Save.items.SequenceEqual(savedItems),"Layout and scroll tests preserve all earned inventory and current location");game.Persist();
        }

        private IEnumerator MapPolishEvidence()
        {
            var prefix=Environment.GetCommandLineArgs().Contains("--map-before")?"Before":"After";
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton);yield return Click("EnterCity");yield return Scene("CityMap");
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
            foreach(var view in new[]{("Mai",new Vector2(.38f,.59f),2f),("WestDock",new Vector2(.30f,.29f),2.5f),("CoveredBridge",new Vector2(.72f,.46f),2f),("NorthStreet",new Vector2(.52f,.70f),1.75f)})
            {Map.View.ZoomBy(view.Item3/Map.View.Zoom);Map.View.Focus(view.Item2);yield return Snapshot(prefix+"-"+view.Item1);}
            Screen.SetResolution(1152,864,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);Map.View.ResetView();
            var ui=GameObject.Find("HUD").GetComponent<UIController>();
            foreach(var node in game.Data.nodes)
            {
                var plate=ui.Panel(Map.View.Content,"QA_"+node.id,node.position,node.position,new Color(.05f,.05f,.07f,.9f));plate.sizeDelta=new Vector2(28,18);plate.anchoredPosition=new Vector2(0,-18);plate.GetComponent<Image>().raycastTarget=false;
                ui.Label(plate,"Id",node.id,Vector2.zero,Vector2.one,12,false).color=Color.magenta;
            }
            yield return Snapshot(prefix+"-QC-node-anchors");
            if(Environment.GetCommandLineArgs().Contains("--map-route-tests"))
            {
                foreach(Transform child in Map.View.Content)if(child.name.StartsWith("QA_"))Destroy(child.gameObject);
                yield return TestNaturalRoutes();
            }
        }

        [Serializable] private sealed class RouteTrace
        {
            public int from,to; public string travel;
            public Vector2[] route;public List<Vector2> positions=new List<Vector2>();
            public List<float> times=new List<float>();
            public float maxDistance;public bool arrived;
        }
        private IEnumerator TestNaturalRoutes()
        {
            var original=JsonUtility.ToJson(game.Save);
            var art=AdventureUIArt.Load();
            Check(Mathf.Abs(art.mapActorScale-.72f)<.0001f,"Shared six-city actor presentation uses 72% of the previous visual dimensions");
            var testArt=ScriptableObject.CreateInstance<AdventureUIArt>();
            testArt.mapActorScale=.72f;testArt.mapActorStyles=new[]{new MapActorStyle{cityId="hue",scale=.74f}};
            var fixture=new MapActorDefinition{size=new Vector2(50,60)};
            foreach(var city in game.Content.cities)
                Check(Mathf.Abs(testArt.ActorScale(city.id,fixture)-(city.id=="hue"?.74f:.72f))<.001f,"Common actor scale/city override works for "+city.id);
            fixture.scaleOverride=.71f;
            Check(Mathf.Abs(testArt.ActorScale("hue",fixture)-.71f)<.001f,"Individual actor override takes precedence over city/global scale");
            Destroy(testArt);
            // Use a completed QA save for route inspection, not a production bypass.
            // The ferry presentation fixture explicitly enables its existing story flag.
            game.World.Set("hoi_an.ferry_available");
            var pairs=new[]{(1,2),(1,3),(1,4),(3,5),(9,5),(4,6),(5,6),(6,7),(4,12),(1,13),(6,15),(17,5)};
            var traces=new List<RouteTrace>();
            foreach(var pair in pairs)foreach(var reverse in new[]{false,true})
            {
                var from=reverse?pair.Item2:pair.Item1;var to=reverse?pair.Item1:pair.Item2;
                game.Save.node=from;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.3f);
                NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(game.Data.nodes[to].position);
                yield return new WaitForSecondsRealtime(.3f);
                CheckMapActors("Route H"+from+"-H"+to);
                var points=CityRouteGeometry.Points(game.Data,from,to);var edge=game.Data.edges.First(e=>e.from==from&&e.to==to||e.from==to&&e.to==from);
                var drawn=CityRouteGeometry.Points(game.Data,edge.from,edge.to);
                var size=Map.View.Content.rect.size;
                for(var i=1;i<drawn.Count;i++)
                {
                    var segment=(RectTransform)Map.View.Content.Find("Path_"+edge.from+"_"+edge.to+"_"+i);
                    var a=Vector2.Scale(drawn[i-1],size);var b=Vector2.Scale(drawn[i],size);
                    Check(segment!=null&&segment.gameObject.activeInHierarchy&&Vector2.Distance(segment.anchoredPosition,(a+b)*.5f)<.01f&&Mathf.Abs(segment.rect.width-Vector2.Distance(a,b))<.01f,"Rendered route segment matches movement geometry: H"+from+"-H"+to+" #"+i);
                }
                var trace=new RouteTrace{from=from,to=to,route=points.ToArray(),travel=CityRouteGeometry.Travel(game.Data,from,to)};
                yield return Click("H"+to,reverse);
                Check(Map.Moving,"Actual "+(reverse?"touch":"mouse")+" input starts H"+from+"-H"+to+" travel");
                var deadline=Time.realtimeSinceStartup+35;bool captured=false;
                while(Map.Moving&&Time.realtimeSinceStartup<deadline)
                {
                    var p=Map.Marker.anchorMin;trace.positions.Add(p);trace.times.Add(Time.realtimeSinceStartup);
                    var distance=float.MaxValue;
                    for(var i=1;i<points.Count;i++)
                    {
                        var a=Vector2.Scale(points[i-1],size);var delta=Vector2.Scale(points[i]-points[i-1],size);
                        var t=delta.sqrMagnitude<.000001f?0:Mathf.Clamp01(Vector2.Dot(Vector2.Scale(p,size)-a,delta)/delta.sqrMagnitude);
                        distance=Mathf.Min(distance,Vector2.Distance(Vector2.Scale(p,size),a+delta*t));
                    }
                    trace.maxDistance=Mathf.Max(trace.maxDistance,distance);
                    var bridge=p.x>.66f&&p.x<.76f;
                    var promenade=from==4&&to==12&&p.x>.31f&&p.x<.38f;
                    if(!captured&&!reverse&&(bridge||promenade||trace.travel=="ferry"&&trace.positions.Count>25))
                    {
                        captured=true;Check(Map.ActiveTravelMode==trace.travel,"Movement uses the configured walk/ferry mode");
                        if(trace.travel=="ferry")Check(Map.Marker.Find("FerryTransport").gameObject.activeInHierarchy&&!Map.View.Content.Find("LumiFootShadow").gameObject.activeInHierarchy,"Ferry shows Lumi on a boat without walking feet or ground shadow");
                        yield return Snapshot("Travel-H"+from+"-H"+to+(trace.travel=="ferry"?"-ferry":promenade?"-promenade":"-covered-bridge"));
                    }
                    yield return null;
                }
                trace.arrived=game.Save.node==to&&!Map.Moving;traces.Add(trace);
                Check(trace.arrived&&trace.positions.Count>10&&trace.maxDistance<.02f,"H"+from+"-H"+to+" arrives without a stuck waypoint and follows every rendered segment (error < 0.02 UI units)");
                Check(Map.ActiveTravelMode=="walk"&&!Map.Marker.Find("FerryTransport").gameObject.activeSelf,"Transport presentation resets on arrival");
            }
            File.WriteAllText(Path.Combine(output,"route-traces.json"),"["+string.Join(",",traces.Select(t=>JsonUtility.ToJson(t)))+"]");
            game.Save.node=2;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            foreach(var zoom in new[]{1f,2f,4f})
            {
                Map.View.ZoomBy(zoom/Map.View.Zoom);Map.View.Focus(game.Data.nodes[2].position);yield return new WaitForSecondsRealtime(.3f);
                var image=MapActor("H2");var hit=(RectTransform)image.transform.parent.Find("ActorHitArea");
                var screen=RectTransformUtility.WorldToScreenPoint(null,hit.TransformPoint(new Vector2(hit.rect.xMin+1,0)));
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},hits);
                Check(hits.Count>0&&hits[0].gameObject==hit.gameObject&&!RectTransformUtility.RectangleContainsScreenPoint(image.rectTransform,screen,null),"Zoom "+zoom+": expanded touch margin outside the smaller Mai sprite is selectable");
                yield return Pointer(screen,true);yield return Until(()=>Map.DialogueOpen,"expanded actor touch target",5);
                Check(game.Save.node==2&&Map.DialogueOpen,"Zoom "+zoom+": touch on actor margin opens the existing NPC interaction");
                yield return CloseEvent();
            }
            File.WriteAllText(savePath,original);game.Reload();
            Check(JsonUtility.ToJson(game.Save)==original,"Route QA fixtures restore the original completed save, inventory and quest state");
        }

        private IEnumerator VerifyRestart()
        {
            Check(NodeMarkerView.MotionMode==MarkerMotionMode.Off,"Marker motion preference survives an actual process restart");
            Check(File.Exists(Path.Combine(output,"stage1-save.json")),"Stage-1 checkpoint exists from the preceding runtime process");
            SaveSystem.OverridePath=Path.Combine(output,"stage1-save.json"); game.Reload();
            Check(game.Discovery.Stage("hoi_an")==1&&game.Quests.IsActive("HQ01"),"Separate process reload preserves HQ01 acceptance and only stage 1");
            Check(!game.Discovery.IsVisible(5)&&!game.Discovery.IsVisible(7)&&!game.Discovery.IsVisible(8)&&!game.Discovery.IsVisible(20),"Stages 2-4 remain hidden in the restarted stage-1 save");
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton);yield return Click("EnterCity");yield return Scene("CityMap");
            CheckStage(1,new[]{0,1,2,3,4,10,11,12,13,21},"Restarted stage 1 renderer");
            CheckMarker("H2",NodeMarkerType.NpcNeedItem,NodeMarkerEffect.Glow);CheckMarker("H3",NodeMarkerType.Reward,NodeMarkerEffect.Glow);
            yield return Click("MapOverview");yield return Snapshot("S1-Restart-only-region2-and-actors");
            game.Scenes.Go("Prototype");yield return Scene("Prototype");
            SaveSystem.OverridePath=Path.Combine(output,"battle-branch-save.json"); game.Reload();
            Check(game.Save.defeatedEnemies.Contains("enemy_mo01")&&game.Inventory.Count("link_key")==1&&game.Discovery.Stage("hoi_an")==3,"Separate process reload preserves the battle branch, unique key and stage-3 discovery");
            SaveSystem.OverridePath=savePath; game.Reload();
            Check(game.Quests.IsCompleted("HQ01")&&game.Save.flags.Contains("hoi_an.lamp_restored"),"Separate process reload preserves chapter completion and restored station");
            Check(game.Discovery.Stage("hoi_an")==4&&game.Discovery.IsVisible(8)&&game.Discovery.IsVisible(20),"Separate process reload preserves final discovery stage and both portals");
            Check(game.Save.solvedPuzzles.Contains("puzzle_market")&&game.Save.solvedPuzzles.Contains("puzzle_paper")&&game.Save.solvedPuzzles.Contains("puzzle_pottery"),"Solved puzzle history survives an actual process restart");
            Check(game.Save.flags.Contains("hoi_an.mo01_disabled"),"Alternative bot resolution survives an actual process restart");
            Check(game.Save.choices.Any(s=>s.Contains("enemy_mo01")&&s.Contains("hack"))&&!game.Save.defeatedEnemies.Contains("enemy_mo01"),"Separate process preserves the peaceful hack branch distinctly from combat victory");
            Check(game.Quests.IsCompleted("HQ02"),"Sidequest completion survives an actual process restart");
            yield return Click("Start"); yield return Scene("WorldMap");
            yield return Click(HoiAnButton); yield return Click("EnterCity"); yield return Scene("CityMap");
            yield return Snapshot("restart-restored-HoiAn");
            Check(game.Save.node==7,"Restart preserves the current city node instead of forcing departure");
            CheckMarker("H2",NodeMarkerType.Camera);CheckMarker("H5",NodeMarkerType.Completed);CheckMarker("H10",NodeMarkerType.Completed);
            Check(game.Quests.IsCompleted("HA_FOOD_PREP")&&game.Quests.IsActive("HQ05")&&game.Quests.IsActive("HQ06")&&game.Inventory.Count("hoi_an_fresh_chili")==1&&game.Inventory.Count("hoi_an_purple_shallot")==2,"Restart preserves actual food delivery, reserved spices and pending cross-city quests");
            CheckMarker("H6",NodeMarkerType.Completed);
            Check(game.World.Has("checkin.hoi_an.h6"),"Recorded landmark survives an actual process restart");
        }

        private IEnumerator Run()
        {
            yield return null; yield return null;
            if (!Application.isEditor)
            {
                Screen.SetResolution(541,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.2f);
                Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            }
            game=GameBootstrap.Ensure(); game.Reload();
            if(Environment.GetCommandLineArgs().Contains("--quality-addendum-qa")){yield return QualityAddendumJourney();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--h3-qa")||Environment.GetCommandLineArgs().Contains("--h3-restart")){yield return H3ArtJourney();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--full-story-resume")){yield return CrossCityJourney(true);yield return FullStoryJourney();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--full-story-tail")){yield return FullStoryJourney();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--full-story-postgame")){yield return FullStoryPostgame();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--story-requirements")){yield return StoryRequirementsDiagnostic();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--full-story-restart")){yield return FullStoryRestart();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--portrait-handoff-only")){yield return PortraitHandoffOnly();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--portrait-diagnostics")){yield return PortraitDiagnostics();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--terrain-routes")){yield return TerrainRouteEvidence();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--ui-polish-restart")){yield return UIPolishRestart();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--ui-polish")){yield return UIPolishEvidence();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--dalat-chapter")||Environment.GetCommandLineArgs().Contains("--dalat-restart")){yield return DaLatOnly();yield break;}
            if (Restart) {if(Environment.GetCommandLineArgs().Contains("--verify-cross-city-save"))yield return VerifyCrossCityRestart();else yield return VerifyRestart();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--cross-city-only")||Environment.GetCommandLineArgs().Contains("--chapters-only")){yield return CrossCityJourney();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--map-polish-evidence")){yield return MapPolishEvidence();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--verify-landmark"))
            {
                yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton);yield return Click("EnterCity");yield return Scene("CityMap");
                yield return TestCheckIn();yield break;
            }
            TestCatalogAndLegacyMigration();
            game.ResetSave();
            game.World.Set("hoi_an.lamp_restored");game.Discovery.Refresh();
            Check(game.Discovery.Stage("hoi_an")==0,"An out-of-order final-gate flag alone cannot reveal the entire map");
            game.World.Set("quest:HQ01");game.Discovery.Refresh();
            Check(game.Discovery.Stage("hoi_an")==1&&!game.Discovery.IsVisible(5),"The first gate advances only its contiguous region even when a later flag is present");
            game.ResetSave();
            Check(game.Save.node==0&&!game.Quests.Active,"Fresh isolated save starts H0 with no accepted tutorial quest");
            Check(!game.Quests.CollectPart(),"Tutorial pickup cannot be granted before accepting its quest");
            var art=DesignKitArt.Load();
            Check(new[]{art.lumiMapTraveler,art.lumiPortrait,art.lumiExploration}.Distinct().Count()==3,"Three approved Lumi roles remain distinct after gameplay integration");
            yield return Click("Start"); yield return Scene("WorldMap");
            yield return Click(HoiAnButton,true);
            Check(SceneManager.GetActiveScene().name=="WorldMap"&&GameObject.Find("DestinationName").GetComponent<Text>().text=="Hội An","Selecting a destination shows its details before entering the city");
            yield return Click("EnterCity",true);yield return Scene("CityMap");
            CheckStage(0,new[]{0,1,2,11},"Fresh arrival");
            yield return TestMarkerPresentation();
            Check(game.Discovery.State(2)==MapNodeState.VISIBLE_LOCKED&&game.Discovery.State(0)==MapNodeState.REVEALED&&game.Discovery.State(5)==MapNodeState.HIDDEN,"Fresh map distinguishes hidden, approachable locked gate and revealed locations");
            var hiddenEvent=game.Events.Definition("enemy_mo01");var originalAuto=hiddenEvent.autoCollect;hiddenEvent.autoCollect=true;
            Check(!game.Events.OnArrive(5,false)&&!game.Inventory.Has("link_key")&&!game.World.Has("hoi_an.mo01_disabled"),"A hidden encounter cannot trigger automatic rewards even through a direct arrival call");hiddenEvent.autoCollect=originalAuto;
            Check(game.Events.BuildOptions(5).options.Count==0&&!game.Events.Execute(5,"battle").openBattle,"Hidden actors expose no event choices or executable battle");
            Check(Map.Marker.GetComponent<Image>().sprite==art.lumiMapTraveler,"City position marker remains the small chibi, without the circular badge");
            Check(GameObject.Find("HoiAnMapBackground").GetComponent<Image>().sprite==art.hoiAnMap,"Provided clean Hoi An Map A remains the runtime background");
            yield return Snapshot("01-HoiAn-stage0-hidden-regions");
            yield return Click("MapOverview");yield return Snapshot("S1-01-Fresh-map-Mai-only-starting-region");
            yield return TestWorldNavigation();
            var initial=game.Save.node;
            Map.TryMove(19); yield return null;
            Check(game.Save.node==initial&&!Map.Moving&&Map.SelectedNode!=19,"Direct calls cannot select or walk to an undiscovered H19");
            yield return TestMapInput();
            Map.View.ZoomBy(1.75f/Map.View.Zoom);
            yield return MarkerPointer("H2",true);yield return CloseEvent();
            yield return CloseEvent();yield return ActorPointer("H2",true);
            Check(!game.Quests.IsActive("HQ01"),"Arriving at Mai does not silently accept HQ01");
            NodeMarker("H11").OnPointerClick(new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left});
            Check(game.Save.node==2&&!Map.Moving&&Map.DialogueOpen,"Visible markers cannot bypass the existing dialogue input lock");
            yield return Snapshot("02-Mai-offer");
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==art.maiPortrait,"Mai speaks using her approved character portrait above the compact dialogue frame");
            Check(GameObject.Find("ListenerPortrait")==null,"Only the speaking portrait is visible; no dim listener remains on the map");
            var frame=GameObject.Find("NodeEventPanel").GetComponent<RectTransform>();
            Check(frame.anchorMax.y-frame.anchorMin.y<=.43f&&FindButton("DialogueNext")!=null,"NPC dialogue occupies under 43 percent of the screen and includes a Next control");
            yield return Snapshot("UI-05-NPC-Mai-dialogue");
            yield return Click("EventClose");
            CheckStage(0,new[]{0,1,2,11},"Declining Mai for now");
            yield return Click("Interact"); yield return Click("EventOption_accept",true);
            Check(game.Quests.IsActive("HQ01")&&game.Quests.Active,"Explicit NPC choice accepts HQ01 and preserves the existing tutorial");
            var revealedGroup=FindButton("H3").GetComponent<CanvasGroup>();
            Check(revealedGroup!=null&&revealedGroup.alpha<1,"Newly discovered region fades in after the real NPC choice");
            yield return Until(()=>GameObject.Find("EventPortrait").GetComponent<Image>().sprite==art.lumiPortrait,"previous speaker finishes fading before Lumi enters",2);
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==art.lumiPortrait&&GameObject.Find("EventSpeaker").GetComponent<Text>().text=="Lumi","Accepting HQ01 switches the active speaker to the full hand-preserving navy Lumi portrait");
            var presence=GameObject.Find("EventPortrait").GetComponent<PortraitPresence>();
            Check(presence!=null&&presence.GetComponent<CanvasGroup>().alpha<1,"Active speaker portrait animates a short fade and scale entrance");
            yield return Snapshot("UI-04-Lumi-dialogue");
            yield return Click("DialogueNext",true);
            yield return Until(()=>GameObject.Find("EventPortrait").GetComponent<Image>().sprite==art.maiPortrait,"Lumi finishes fading before Mai returns",2);
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==art.maiPortrait&&game.Quests.IsActive("HQ01"),"Next transitions back to Mai without altering accepted quest progress");
            yield return CloseEvent();
            yield return new WaitForSecondsRealtime(.7f);
            CheckStage(1,new[]{0,1,2,3,4,10,11,12,13,21},"Mai opens the next region");
            CheckMarker("H2",NodeMarkerType.NpcNeedItem,NodeMarkerEffect.Glow);CheckMarker("H3",NodeMarkerType.Reward,NodeMarkerEffect.Glow);
            yield return Snapshot("MKR-02-Mai-needs-item-and-alley-reward");
            Check(revealedGroup.alpha==1,"Node reveal animation reaches full visibility after its short transition");
            KeepSave("stage1-save");
            yield return Snapshot("03-HoiAn-stage1-reveal");
            yield return Click("MapOverview");yield return Snapshot("S1-02-Gate1-only-next-region-merchant-crab");
            Map.View.ZoomBy(1.55f/Map.View.Zoom);Map.View.Focus(new Vector2(.52f,.55f));
            yield return Snapshot("EXP-02-After-first-gate-new-boundary-previews");
            yield return Click("QuestJournal");
            Check(GameObject.Find("QuestJournalPanel")!=null&&GameObject.Find("QuestJournalPanel").GetComponentsInChildren<Text>().Any(t=>t.text.Contains("HQ01")||t.text.Contains("Mắt Đèn")),"Real journal displays the accepted multi-step main quest");
            yield return Snapshot("04-quest-journal"); yield return Click("CloseJournal");
            yield return Travel(3); yield return ExploreH3();
            CheckMarker("H2",NodeMarkerType.NpcTalk,NodeMarkerEffect.Bounce);CheckMarker("H3",NodeMarkerType.Camera);
            Map.View.Focus(game.Data.nodes[2].position);yield return Snapshot("MKR-03-Mai-ready-to-receive-item");
            yield return TestInventoryPresentation();
            yield return Travel(2); yield return Click("EventOption_tutorial_return"); yield return CloseEvent();
            Check(game.Quests.Completed&&game.Save.flags.Contains("lantern-lit"),"Existing H2-H3-pickup-return tutorial still completes and preserves its reward");
            Check(game.Inventory.Has("lamp_eye"),"Tutorial hand-in forwards the Mắt Đèn to HQ01 rather than consuming its main-story component");
            CheckMarker("H2",NodeMarkerType.Normal);CheckMarker("H4",NodeMarkerType.Puzzle,NodeMarkerEffect.Glow);
            yield return CloseEvent();yield return ActorPointer("H4",false);yield return SolvePuzzle("puzzle_market");
            Check(game.Save.flags.Contains("hoi_an.market_puzzle_solved"),"Market puzzle opens the required paper-workshop interaction");
            CheckMarker("H4",NodeMarkerType.Camera);
            yield return Travel(13); yield return SolvePuzzle("puzzle_paper");
            Check(game.Inventory.Has("hanji_core"),"Paper challenge awards the source-designed Lõi Giấy Sáng");
            yield return TestEarnedInventory();
            yield return new WaitForSecondsRealtime(.7f);
            Check(game.Discovery.Stage("hoi_an")==2&&game.Discovery.IsVisible(19)&&!game.Discovery.IsVisible(6),"H13 reveals H19 before H5 while keeping the area beyond the bot hidden");
            var codeRoute=game.Data.FindPath(13,19,game.Save);
            Check(codeRoute!=null&&!codeRoute.Contains(5),"A reachable route to H19 exists before resolving H5; the alternative branch cannot softlock");
            yield return Snapshot("07-HoiAn-stage2-before-bot");
            CheckStage(2,new[]{0,1,2,3,4,5,10,11,12,13,14,16,18,19,21},"After paper gate");
            Check(game.Discovery.State(5)==MapNodeState.VISIBLE_LOCKED&&game.Discovery.State(13)==MapNodeState.REVEALED,"Paper gate changes state while the visible bot remains a locked key gate");
            CheckMarker("H5",NodeMarkerType.BossAlert,NodeMarkerEffect.Pulse);CheckMarker("H13",NodeMarkerType.Completed);
            yield return Click("MapOverview");yield return Snapshot("S1-03-NPC-and-bridge-bot-on-map");
            Map.View.ZoomBy(1.75f/Map.View.Zoom);Map.View.Focus(new Vector2(.59f,.5f));
            yield return Snapshot("S1-03b-NPC-and-bot-map-detail");
            yield return Snapshot("MKR-04-Visible-boss-red-alert");
            Check(NodeMarker("H5").GlyphColor.r>.65f&&NodeMarker("H5").GlyphColor.g<.4f&&NodeMarker("H5").KeyGateAura.activeSelf,"Visible key boss has a red exclamation glyph and subtle aura");
            yield return RecordFloating("H5","boss-floating");
            yield return Snapshot("EXP-03-Boss-sprite-red-floating-alert");
            KeepSave("before-mo01");
            // Branch, battle and sidequest integration continue below after the service/UI contracts.
            yield return RunCombatAndChapter();
            yield return TestCheckIn();
            yield return TestSpecialtyItems();
            yield return TestNewNodeFromData();
            Check(FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Scene transitions leave one bootstrap, canvas and EventSystem");
            KeepSave("final-save");
            if(Environment.GetCommandLineArgs().Contains("--with-cross-city")||ChaptersEnabled)yield return CrossCityJourney();
            if(Environment.GetCommandLineArgs().Contains("--with-dalat")&&!FullStoryEnabled)yield return DaLatJourney();
            if(FullStoryEnabled)yield return FullStoryJourney();
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
        }

        private IEnumerator RunCombatAndChapter()
        {
            Check(!game.Quests.CanComplete("HQ01"),"HQ01 cannot complete with the connection key still missing");
            Check(game.Events.BuildOptions(7).options.Count==0,"Undiscovered station exposes no restoration interaction before the bot gate");
            yield return TestIntermediateEncounter();
            yield return TestIntermediatePickupAndOptionalNpc();

            // The same pre-gate checkpoint exercises both branches; no source data or player save is changed.
            yield return CloseEvent();yield return ActorPointer("H5",true);
            game.Save.playerHP=1; game.Persist();
            yield return Click("EventOption_battle");
            Check(game.Battles.Active&&game.Battles.EnemyHP==26,"H5 starts Bot Mờ-01 with source-designed 26 HP");
            yield return Snapshot("08-battle-before-loss");
            yield return Click("BattleDefend");
            yield return Until(()=>game.Battles.Outcome=="loss","battle defeat");
            Check(!game.Save.flags.Contains("hoi_an.mo01_disabled")&&!game.Inventory.Has("link_key")&&game.Discovery.Stage("hoi_an")==2,"Losing a real battle grants neither key nor gate progress");
            Check(game.Inventory.Has("lamp_eye")&&game.Inventory.Has("hanji_core")&&game.Quests.IsActive("HQ01"),"Battle loss preserves previously earned quest items and main-quest progress");
            Check(game.Save.playerHP>0,"Battle defeat leaves a retryable player instead of a save softlock");
            yield return Snapshot("09-battle-loss-retry");
            yield return CloseEvent();
            Check(game.Save.node==5&&!Map.Moving,"After defeat Lumi remains at the encounter and does not automatically continue");
            Check(MapActor("H5").gameObject.activeInHierarchy,"A failed battle keeps the bridge bot standing at the gate");
            CheckMarker("H5",NodeMarkerType.BossAlert,NodeMarkerEffect.Pulse);
            yield return ReloadCheckpoint("before-mo01");

            yield return Travel(5);
            game.Save.playerHP=45; game.Save.lightEnergy=3;
            Check(game.Inventory.Count("healing_tea")>=2,"Mai supplies the designed starter healing tea before the bot encounter");
            game.Persist();
            yield return Click("EventOption_battle");
            var enemyHP=game.Battles.EnemyHP; var hp=game.Save.playerHP;
            yield return Click("BattleDefend");
            Check(game.Battles.EnemyHP==enemyHP&&game.Save.playerHP<hp&&game.Save.playerHP>hp-7,"Defend consumes a turn and reduces the source bot counterattack");
            yield return Click("BattleAttack");
            Check(game.Battles.EnemyHP<enemyHP,"Attack deals real damage to the enemy HP");
            var tea=game.Inventory.Count("healing_tea"); hp=game.Save.playerHP;
            yield return Click("BattleHeal",true);
            Check(game.Inventory.Count("healing_tea")==tea-1&&game.Save.playerHP>hp,"Heal consumes one tea and restores HP through the real battle UI");
            var energy=game.Save.lightEnergy; enemyHP=game.Battles.EnemyHP;
            yield return Click("BattleLight");
            Check(game.Save.lightEnergy==energy-1&&game.Battles.EnemyHP<enemyHP,"Light skill consumes energy and damages the bot");
            yield return Snapshot("10-battle-actions");
            for (var turn=0;game.Battles.Active&&turn<10;turn++) yield return Click("BattleAttack",turn%2==0);
            Check(game.Battles.Outcome=="win"&&game.Battles.EnemyHP<=0,"Player can win the turn-based H5 encounter");
            Check(game.Save.defeatedEnemies.Contains("enemy_mo01")&&game.Inventory.Count("link_key")==1&&game.Save.flags.Contains("hoi_an.mo01_disabled"),"Battle victory records the opponent, unique connection key and gate flag");
            yield return Snapshot("11-battle-win");
            yield return CloseEvent(); yield return new WaitForSecondsRealtime(.7f);
            Check(game.Save.node==5&&!Map.Moving&&game.Discovery.Stage("hoi_an")==3,"Victory reveals the next region and waits for the player's route choice");
            CheckStage(3,new[]{0,1,2,3,4,5,6,7,10,11,12,13,14,15,16,17,18,19,21},"After battle gate");
            Check(!MapActor("H5").gameObject.activeInHierarchy&&game.Discovery.State(5)==MapNodeState.REVEALED,"Battle victory removes the blocking bot and changes its node to revealed");
            CheckMarker("H5",NodeMarkerType.Completed);
            yield return Click("MapOverview");yield return Snapshot("S1-04-Battle-opens-station-region-only");
            var keyCount=game.Inventory.Count("link_key");
            Check(!game.Battles.Start(game.Events.Definition("enemy_mo01"))&&game.Inventory.Count("link_key")==keyCount,"A defeated encounter cannot restart to farm its unique reward");
            KeepSave("battle-branch-save");

            yield return ReloadCheckpoint("before-mo01");
            Check(game.Discovery.Stage("hoi_an")==2&&!game.Save.flags.Contains("hoi_an.mo01_disabled"),"Alternative path starts from the same unresolved pre-bot checkpoint");
            yield return Travel(14); yield return Click("EventOption_talk");
            Check(game.Save.flags.Contains("hoi_an.pottery_clue_known"),"Talking to Phúc records the pottery clue in world state");
            yield return Snapshot("12-Phuc-clue"); yield return CloseEvent();
            yield return Travel(18,true);
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==AdventureUIArt.Load().Npc("npc_guest").sprite,"Korean visitor uses the extracted tourist portrait matched by npcId");
            yield return Snapshot("UI-NPC-Korean-visitor");
            yield return Travel(19); yield return SolvePuzzle("puzzle_pottery");
            Check(!game.Save.flags.Contains("hoi_an.mo01_disabled")&&game.Discovery.Stage("hoi_an")==2,"Knowing the pottery code does not remotely skip interaction with the bot");
            yield return Travel(5,true);
            yield return Click("EventOption_alternate",true);
            Check(game.Save.flags.Contains("hoi_an.mo01_disabled")&&game.Inventory.Count("link_key")==1&&!game.Save.defeatedEnemies.Contains("enemy_mo01"),"H19 code disables H5 and obtains its key without marking a battle victory");
            Check(game.Save.choices.Count>0,"Alternative encounter choice is saved explicitly");
            yield return Snapshot("13-bot-disabled-by-puzzle");
            yield return CloseEvent(); yield return new WaitForSecondsRealtime(.7f);
            Check(game.Discovery.Stage("hoi_an")==3&&game.Discovery.IsVisible(7)&&!game.Discovery.IsVisible(8)&&!game.Discovery.IsVisible(20),"Either H5 branch opens the station region while portals remain hidden");
            CheckStage(3,new[]{0,1,2,3,4,5,6,7,10,11,12,13,14,15,16,17,18,19,21},"After hack gate");
            Check(!MapActor("H5").gameObject.activeInHierarchy&&game.Discovery.State(7)==MapNodeState.VISIBLE_LOCKED,"Peaceful code removes the same blocker while the station gate stays unresolved");
            CheckMarker("H5",NodeMarkerType.Completed);
            yield return Click("MapOverview");yield return Snapshot("S1-05-Code-opens-same-region-only");

            yield return Travel(11);
            var coins=game.Save.coins;
            if (FindButton("EventOption_collect")!=null) yield return Click("EventOption_collect");
            Check(game.Save.coins>coins||game.Save.resolvedEvents.Any(s=>s.Contains("loot_crate")),"H11 has an actual one-time loot interaction");
            yield return CloseEvent();
            var afterChest=game.Save.coins; yield return Click("Interact");
            if (FindButton("EventOption_collect")!=null&&FindButton("EventOption_collect").interactable) yield return Click("EventOption_collect");
            Check(game.Save.coins==afterChest,"Reopening the H11 chest cannot duplicate its currency reward");
            yield return CloseEvent();
            CheckMarker("H10",NodeMarkerType.NpcTalk,NodeMarkerEffect.Blink);
            yield return Travel(10); yield return Click("EventOption_accept"); yield return CloseEvent();
            CheckMarker("H10",NodeMarkerType.NpcNeedItem,NodeMarkerEffect.Glow);CheckMarker("H12",NodeMarkerType.Reward,NodeMarkerEffect.Glow);
            Check(game.Quests.IsActive("HQ02")&&!game.Quests.CanComplete("HQ02"),"Bà Tư's real sidequest requires resolving the crab and returning copper wire");
            yield return Travel(12);
            Check(game.Inventory.Has("meat_portion"),"The noncombat crab route has reachable bait before the encounter");
            var bait=game.Inventory.Count("meat_portion");
            yield return Click("EventOption_alternate",true);
            Check(game.Inventory.Count("meat_portion")==bait-1&&game.Inventory.Has("copper_wire"),"Bait is consumed and the crab gives back Bà Tư's wire");
            yield return Snapshot("14-crab-bait-resolution"); yield return CloseEvent();
            Check(!MapActor("H12").gameObject.activeInHierarchy,"Bait resolution removes the crab character without leaving its enemy icon behind");
            CheckMarker("H10",NodeMarkerType.NpcTalk,NodeMarkerEffect.Bounce);CheckMarker("H12",NodeMarkerType.Completed);
            Map.View.Focus(game.Data.nodes[10].position);yield return Snapshot("MKR-05-BaTu-return-with-item");
            Check(game.Quests.CanComplete("HQ02"),"Recovering the wire satisfies the sidequest objective");
            yield return Travel(10); yield return Click("EventOption_complete");
            Check(game.Quests.IsCompleted("HQ02")&&!game.Inventory.Has("copper_wire")&&game.Save.flags.Contains("hoi_an.secret_lane_found"),"Returning the wire consumes it, completes HQ02 and discovers the secret lane");
            yield return Snapshot("15-BaTu-turn-in"); yield return CloseEvent();
            CheckMarker("H10",NodeMarkerType.NpcTalk,NodeMarkerEffect.Pulse);
            Check(game.Discovery.IsVisible(9),"The sidequest reveals a real new H9 node without altering the main route");
            var sideCoins=game.Save.coins; var sideItems=game.Save.items.Count;
            Check(!game.Quests.Complete("HQ02")&&game.Save.coins==sideCoins&&game.Save.items.Count==sideItems,"Completed sidequest rewards cannot be granted a second time");

            yield return Travel(7);
            Check(game.Inventory.Has("lamp_eye")&&game.Inventory.Has("hanji_core")&&game.Inventory.Has("link_key"),"Station receives all three designed components through gameplay");
            game.Inventory.TryConsume("link_key");
            var invalidRestore=game.Events.Execute(7,"restore");
            Check(!invalidRestore.changed&&!game.Save.flags.Contains("hoi_an.lamp_restored"),"Even at H7, execution refuses restoration when a required component is absent");
            game.Inventory.Add("link_key"); game.Persist();
            var coinsBeforeRestore=game.Save.coins;
            yield return Snapshot("16-station-ready");
            if(Environment.GetCommandLineArgs().Contains("--polish-qa"))NodeMarkerView.SetMotionMode(MarkerMotionMode.Full);
            yield return Click("EventOption_restore",true);
            if(Environment.GetCommandLineArgs().Contains("--polish-qa"))yield return PolishReceiptMotion();
            Check(game.Quests.IsCompleted("HQ01")&&game.Save.flags.Contains("hoi_an.lamp_restored"),"Station restoration completes HQ01 and writes the cross-city world flag");
            Check(game.Save.coins==coinsBeforeRestore+150,"HQ01 pays its source-defined 150-coin reward once");
            yield return Snapshot("17-station-light-restored");
            yield return CloseEvent(); yield return new WaitForSecondsRealtime(.7f);
            Check(game.Discovery.Stage("hoi_an")==4&&game.Discovery.IsVisible(8)&&game.Discovery.IsVisible(20),"Restoration reveals both Jeonju and Huế portal nodes");
            Check(game.Data.FindPath(7,8,game.Save)!=null&&game.Data.FindPath(7,20,game.Save)!=null,"Both new city routes become reachable after the station gate");
            Check(game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju")&&!game.IsCityUnlocked("da_lat"),"Hội An's world flag unlocks the correct next cities without opening every later chapter");
            Check(game.Data.cityId=="hoi_an"&&game.Save.node==7&&SceneManager.GetActiveScene().name=="CityMap","Opening portals leaves the player free to stay in Hội An");
            yield return Snapshot("18-HoiAn-final-discovery");
            CheckStage(4,Enumerable.Range(0,22).ToArray(),"After station gate");
            CheckMarker("H2",NodeMarkerType.Camera);CheckMarker("H6",NodeMarkerType.Camera);CheckMarker("H7",NodeMarkerType.Camera);
            yield return Snapshot("MKR-06-Completed-locations-become-checkin-markers");
            Screen.SetResolution(768,1024,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            yield return Click("MapOverview"); yield return Snapshot("19-HoiAn-tablet-final");
            Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            yield return Click("Inventory"); yield return Snapshot("20-inventory-final");
            yield return Click("CloseInventory");
            yield return Travel(8,true);yield return Click("EventOption_portal",true);yield return Scene("WorldMap");
            Check(GameObject.Find("DestinationName").GetComponent<Text>().text=="Jeonju"&&game.Save.cityId=="hoi_an"&&game.Save.node==8,"A city portal opens World Map with the intended destination selected instead of directly switching maps");
            Check(GameObject.Find("DestinationStatus").GetComponent<Text>().text.Contains("đã mở")&&FindButton("EnterCity").interactable,"A story-unlocked Jeonju route now enters its authored opening through World Map");
            Check(FindButton("City1").GetComponentsInChildren<Text>().Any(t=>t.text=="ĐÃ MỞ TUYẾN")&&FindButton("City2").GetComponentsInChildren<Text>().Any(t=>t.text=="CHƯA MỞ"),"Destination cards show correct unlocked and still-locked states after the chapter");
            yield return Snapshot("UI-WorldMap-story-routes-open");
            yield return Click(HoiAnButton);yield return Click("EnterCity",true);yield return Scene("CityMap");
            Check(game.Save.node==8&&game.Discovery.Stage("hoi_an")==4&&game.Quests.IsCompleted("HQ02"),"Re-entering Hội An from destination details preserves the portal position, reveal stages and sidequest history");
            yield return Travel(7);yield return CloseEvent();
            yield return Travel(17,true);
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==AdventureUIArt.Load().Npc("npc_boatman").sprite,"Boatman uses his extracted portrait with conical hat and complete oar");
            yield return Snapshot("UI-NPC-boatman");
            yield return Travel(7);yield return CloseEvent();
            var finalCoins=game.Save.coins;
            yield return TestUI1LayoutsAndScroll();
            Check(!game.Quests.Complete("HQ01")&&game.Save.coins==finalCoins,"Main chapter completion cannot pay again");
            Check(game.Persist(),"Full chapter state is written to the isolated disk save"); game.Reload();
            Check(game.Quests.IsCompleted("HQ01")&&game.Quests.IsCompleted("HQ02")&&game.Save.node==7&&game.Discovery.Stage("hoi_an")==4,"In-process reload retains main quest, sidequest, node and reveal stage");
        }

        private IEnumerator TestNewNodeFromData()
        {
            yield return CloseEvent();
            var city=game.Content.City("hoi_an");var oldNodes=city.nodes;var oldEdges=city.edges;var oldEvents=game.Content.events;
            var saved=JsonUtility.ToJson(game.Save,true);var oldIds=game.Data.nodes.Select(n=>n.id).ToArray();
            var oldPositions=game.Data.nodes.Select(n=>n.position).ToArray();
            var extra=new NodeContentDefinition{id="H22",name="QA check-in node",kind="checkin",eventId="qa_checkin",position=new Vector2(.22f,.55f),revealStage=5};
            try
            {
                // Temporary runtime fixture only: append a node using catalog data,
                // even if its records are reordered. No source JSON or art is changed.
                city.nodes=oldNodes.Reverse().Concat(new[]{extra}).ToArray();
                city.edges=oldEdges.Concat(new[]{new RouteDefinition{id="qa_H1_H22",from="H1",to="H22",waypoints=new[]{new Vector2(.25f,.53f)}}}).ToArray();
                game.Content.events=oldEvents.Concat(new[]{new EventDefinition{id="qa_checkin",cityId="hoi_an",nodeId="H22",type="junction",title="QA fixture"}}).ToArray();
                game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                Check(game.Data.nodes.Length==oldIds.Length+1&&Map.NodeMarkers.Count==game.Data.nodes.Length,"Adding catalog H22 creates its runtime node and marker without changing a render limit");
                Check(game.Data.nodes.Take(oldIds.Length).Select(n=>n.id).SequenceEqual(oldIds)&&game.Data.nodes.Take(oldIds.Length).Select(n=>n.position).SequenceEqual(oldPositions),"New catalog nodes preserve all reviewed old positions and numeric save indices even when records are reordered");
                Check(FindButton("H22")==null&&!NodeMarker("H22").gameObject.activeInHierarchy&&game.Data.FindPath(7,oldIds.Length,game.Save)==null,"A newly appended node remains fully hidden and unroutable until its reveal stage");
                extra.revealStage=4;game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                CheckMarker("H22",NodeMarkerType.Camera);
                Check(FindButton("H22")!=null&&game.Data.FindPath(7,oldIds.Length,game.Save)!=null,"A new revealed check-in node has an ID-based input target and a valid data-defined route");
                yield return MarkerPointer("H22",true,false);
                yield return Snapshot("MKR-H22-extension-fixture-not-delivered-content");
            }
            finally
            {
                if(Map!=null)Map.enabled=false;
                city.nodes=oldNodes;city.edges=oldEdges;game.Content.events=oldEvents;
                File.WriteAllText(savePath,saved);game.Reload();
            }
            game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(game.Data.nodes.Select(n=>n.id).SequenceEqual(oldIds)&&Map.NodeMarkers.Count==oldIds.Length&&game.Save.node==7,"Removing the QA fixture restores the delivered 22-node map and original save progress");
            CheckMarker("H2",NodeMarkerType.Camera);
        }

        private IEnumerator TestIntermediateEncounter()
        {
            var original=game.Data.edges;
            try
            {
                // Synthetic visible branch isolates the travel invariant; original city JSON and routes stay intact.
                game.Data.edges=new[]{new EdgeData{from=3,to=5,requirement="",bends=new Vector2[0]},new EdgeData{from=5,to=16,requirement="",bends=new Vector2[0]}};
                game.Save.node=3; game.Persist(); game.Scenes.Go("CityMap"); yield return Scene("CityMap");
                var planned=game.Data.FindPath(3,16,game.Save);
                Check(planned!=null&&planned.SequenceEqual(new[]{3,5,16}),"Intermediate encounter fixture requests a visible multi-node route through H5");
                Map.View.Focus(game.Data.nodes[16].position); yield return new WaitForSecondsRealtime(.2f);
                yield return Click("H16",true);
                yield return Until(()=>!Map.Moving,"intermediate mandatory encounter");
                Check(game.Save.node==5&&!game.Save.flags.Contains("hoi_an.mo01_disabled"),"Actual multi-node movement stops at mandatory intermediate H5 before the requested endpoint");
                Check(FindButton("EventOption_battle")!=null,"Intermediate gate opens the encounter choice instead of merely showing preview text");
                yield return Snapshot("intermediate-H5-stop");
                yield return Click("EventClose"); yield return new WaitForSecondsRealtime(.3f);
                Check(game.Save.node==5&&!Map.Moving,"Closing an intermediate encounter leaves the route paused for a new player decision");
            }
            finally { game.Data.edges=original; }
            yield return ReloadCheckpoint("before-mo01");
        }

        private void TestCatalogAndLegacyMigration()
        {
            var content=game.Content;
            Check(content.cities.Length==6&&content.cities.Sum(c=>c.nodes.Length)==133,"Shared catalog imports the actual six-city, 133-node LBC gameplay dataset");
            Check(content.quests.Length==29&&content.enemies.Length==23&&content.items.Length==38,"Shared catalog retains original 24 quests/31 items plus previously released extras and the optional Dalat strawberry gift");
            foreach (var city in content.cities)
            {
                var map=GameData.Load(city.id);
                Check(map.edges.All(e=>e.from>=0&&e.to>=0&&e.from<map.nodes.Length&&e.to<map.nodes.Length),"No invalid runtime route references in "+city.id);
                Check(map.nodes.All(n=>content.Event(city.id,n.eventId)!=null),"Every node resolves a typed event definition in "+city.id);
                Check(city.gates.Length==4&&city.gates.All(g=>city.nodes.Any(n=>n.id==g.nodeId&&n.isKeyGate)&&!string.IsNullOrWhiteSpace(g.condition)),"Four data-driven key gates are configured for "+city.id);
                var fresh=new SaveData {cityId=city.id,cityProgress=new List<CityProgress>{new CityProgress{cityId=city.id,stage=0}}};
                Check(map.nodes.Where(n=>n.revealStage>0).All(n=>!MapDiscoveryManager.Visible(map,n,fresh)),"Common discovery predicate hides later regions of "+city.id+" without a separate controller");
                var frontier=new NodeData{id=map.runtimeNodeIds?.FirstOrDefault(),revealStage=1,previewStages=new[]{0},requiredFlags=new[]{"qa_entry_flag"}};
                Check(MapDiscoveryManager.DiscoveryState(map,frontier,fresh)==MapDiscoveryState.PREVIEW&&!MapDiscoveryManager.Visible(map,frontier,fresh),"Shared API supports anonymous preview without granting entry in "+city.id+" (synthetic data fixture)");
                frontier.previewStages=Array.Empty<int>();
                Check(MapDiscoveryManager.DiscoveryState(map,frontier,fresh)==MapDiscoveryState.HIDDEN,"No explicit preview opt-in keeps distant/secret nodes fully hidden in "+city.id);
                foreach (var edge in map.edges.Where(e=>e.requiredFlags!=null&&e.requiredFlags.Length>0))
                {
                    Check(!map.EdgeOpen(edge,fresh),"Unmet route flags remain enforced by the common pathfinder in "+city.id);
                    var unlocked=new SaveData{flags=edge.requiredFlags.ToList()};
                    Check(map.EdgeOpen(edge,unlocked),"Required route flags can open the same data-defined edge in "+city.id);
                }
            }
            var legacy=Path.Combine(output,"legacy-v1.json");
            File.WriteAllText(legacy,"{\"version\":1,\"node\":3,\"visited\":[0,1,2,3],\"accepted\":[\"mai-lantern\"],\"completed\":[],\"items\":[\"lantern-part\"],\"collected\":[\"lantern-part\"],\"flags\":[]}");
            SaveSystem.OverridePath=legacy; game.Reload();
            Check(game.Save.version==2&&game.Save.node==3&&game.Quests.Active&&game.Quests.IsActive("HQ01"),"A legacy v1 tutorial save migrates to the chapter quest without losing location or acceptance");
            Check(game.Inventory.Has("lantern-part")&&game.Inventory.Count("lamp_eye")==1&&game.Discovery.Stage("hoi_an")==1,"Legacy pickup migrates into the main quest once and reveals the accepted region");
            Check(game.Persist(),"Migrated v1 save persists as schema v2"); game.Reload();
            Check(game.Inventory.Count("lamp_eye")==1&&game.Inventory.Count("healing_tea")==2,"Repeated migration/reload cannot duplicate the unique eye or starter tea");
            SaveSystem.OverridePath=savePath; game.Reload();
        }

        private IEnumerator TestIntermediatePickupAndOptionalNpc()
        {
            var original=game.Data.edges;
            var pickup=game.Events.Definition("loot_crate"); var originalAuto=pickup.autoCollect;
            try
            {
                // The production chest is click-to-open; this temporary fixture exercises the generic auto-pickup contract.
                pickup.autoCollect=true;
                game.Data.edges=new[]{new EdgeData{from=3,to=11,bends=new Vector2[0]},new EdgeData{from=11,to=14,bends=new Vector2[0]},new EdgeData{from=14,to=16,bends=new Vector2[0]}};
                game.Save.node=3; var coins=game.Save.coins; game.Persist();
                game.Scenes.Go("CityMap"); yield return Scene("CityMap");
                Map.View.Focus(game.Data.nodes[16].position); yield return new WaitForSecondsRealtime(.2f);
                yield return Click("H16"); yield return Until(()=>!Map.Moving,"intermediate auto-pickup and NPC traversal");
                Check(game.Save.node==16&&game.Save.coins==coins+6,"Movement collects an intermediate automatic item event and continues to the requested endpoint");
                Check(game.Save.visited.Contains(11)&&game.Save.visited.Contains(14)&&!game.Save.flags.Contains("hoi_an.pottery_clue_known"),"Passing an optional NPC marks the visit without forcing dialogue or claiming its talk reward");
                Check(FindButton("EventClose")==null,"Optional intermediate NPC does not hijack a multi-node travel route");
                var after=game.Save.coins;
                Check(!game.Events.OnArrive(11,false)&&game.Save.coins==after,"Re-entering the automatic item event cannot duplicate its reward");
            }
            finally { game.Data.edges=original; pickup.autoCollect=originalAuto; }
            yield return ReloadCheckpoint("before-mo01");
        }
    }
}
#endif
