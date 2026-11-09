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
    // Opt-in integration harness. Uses an isolated save; never changes a real player's progress.
    public sealed class SliceSmokeRunner : MonoBehaviour
    {
        private readonly List<string> results=new List<string>();
        private bool fatal;
        private Mouse mouse;
        private Touchscreen touch;
        private Keyboard keyboard;
        private string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            bool run=Environment.GetCommandLineArgs().Contains("--slice-smoke");
#if UNITY_EDITOR
            run |= UnityEditor.SessionState.GetBool("SliceSmoke",false);
            UnityEditor.SessionState.SetBool("SliceSmoke",false);
#endif
            if(run) new GameObject("SliceSmokeRunner").AddComponent<SliceSmokeRunner>();
        }
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            output=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Validation","M01-20261016-0600")); Directory.CreateDirectory(output);
            SaveSystem.OverridePath=Path.Combine(output,"smoke-save.json");
            Application.logMessageReceived+=OnLog;
            Application.runInBackground=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            // Native desktop motion must not steal .current from the injected devices.
            foreach(var device in InputSystem.devices.ToArray())
                if(device is Mouse || device is Keyboard || device is Touchscreen) InputSystem.DisableDevice(device);
            mouse=InputSystem.AddDevice<Mouse>(); touch=InputSystem.AddDevice<Touchscreen>(); keyboard=InputSystem.AddDevice<Keyboard>();
            StartCoroutine(Guard(Run()));
        }
        private void OnLog(string message,string trace,LogType type)
        { if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert) { fatal=true; results.Add("ERROR: "+message+"\n"+trace); } }
        private void Check(bool condition,string message)
        { if(!condition) throw new Exception("FAIL: "+message); results.Add("PASS: "+message); Debug.Log("SLICE PASS: "+message); }
        private IEnumerator Guard(IEnumerator routine)
        {
            var stack=new Stack<IEnumerator>(); stack.Push(routine);
            while(stack.Count>0)
            {
                object next=null; bool more=false;
                try { more=stack.Peek().MoveNext(); if(more) next=stack.Peek().Current; }
                catch(Exception e) { fatal=true; results.Add(e.ToString()); break; }
                if(!more) { stack.Pop(); continue; }
                if(next is IEnumerator nested) stack.Push(nested); else yield return next;
            }
            var resultName=Environment.GetCommandLineArgs().Contains("--verify-save")?"restart-results.txt":"smoke-results.txt";
            File.WriteAllLines(Path.Combine(output,resultName),results.Concat(new[]{fatal?"FAILED":"ALL CHECKS PASSED"}));
            Application.logMessageReceived-=OnLog;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(fatal?1:0);
#else
            Application.Quit(fatal?1:0);
#endif
        }
        private IEnumerator Until(Func<bool> condition,string label,float timeout=20)
        {
            var end=Time.realtimeSinceStartup+timeout;
            while(!condition()) { if(Time.realtimeSinceStartup>end) throw new Exception("Timed out: "+label); yield return null; }
            yield return null;
        }
        private Button Button(string name) => FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name==name);
        private IEnumerator Click(string name,bool useTouch=false)
        {
            var b=Button(name); Canvas.ForceUpdateCanvases(); var rect=(RectTransform)b.transform;
            var pos=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            yield return Pointer(pos,useTouch);
        }
        private IEnumerator Pointer(Vector2 pos,bool useTouch)
        {
            if(useTouch) InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Began,position=pos});
            else InputSystem.QueueStateEvent(mouse,new MouseState{position=pos}.WithButton(MouseButton.Left));
            yield return null; yield return null;
            if(useTouch) InputSystem.QueueStateEvent(touch,new TouchState{touchId=1,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=pos});
            else InputSystem.QueueStateEvent(mouse,new MouseState{position=pos});
            yield return null; yield return null;
        }
        private IEnumerator Scene(string name)
        { yield return Until(()=>SceneManager.GetActiveScene().name==name && !GameBootstrap.Instance.Scenes.Busy,name); yield return null; }
        private IEnumerator Snapshot(string name)
        {
            if(!Application.isEditor) { yield return new WaitForSecondsRealtime(.2f); ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png")); yield return new WaitForSecondsRealtime(.3f); }
        }
        private void TestExpandedMapData(GameBootstrap game)
        {
            var data=game.Data;
            Check(data.nodes.Length==22 && Enumerable.Range(0,10).All(i=>data.nodes[i].id=="H"+i),"H0-H9 remain the main nodes and the designed H10-H21 expansion is loaded");
            var originalEdges=new[]{"0:1:","1:2:","1:3:","1:4:","2:9:future","3:5:lantern-lit","9:5:future","4:6:lantern-lit","5:6:lantern-lit","6:7:lantern-lit","7:8:future"};
            Check(originalEdges.All(expected=>data.edges.Any(e=>e.from+":"+e.to+":"+e.requirement==expected)),"All eleven original routes retain their endpoints and progression gates");
            Check(data.nodes.All(n=>n.position.x>=0&&n.position.x<=1&&n.position.y>=0&&n.position.y<=1) && data.edges.All(e=>e.from>=0&&e.from<data.nodes.Length&&e.to>=0&&e.to<data.nodes.Length),"All expanded nodes and route endpoints lie within the map data bounds");
            Check(data.nodes.Skip(10).All(n=>!string.IsNullOrWhiteSpace(n.eventId)) && data.nodes.All(n=>n.backgroundId!=null),"Expansion retains design event IDs and supports backgroundId on every node");
            var open=new SaveData();
            open.flags.AddRange(data.nodes.Select(n=>n.requirement).Concat(data.edges.Select(e=>e.requirement)).Where(r=>!string.IsNullOrEmpty(r)).Distinct());
            Check(Enumerable.Range(0,data.nodes.Length).All(i=>data.FindPath(0,i,open)!=null),"All 22 nodes have connected data-driven routes when their gates are open");
            foreach(var stage in new[]{new SaveData(),new SaveData{flags=new List<string>{"lantern-lit"}}})
                Check(Enumerable.Range(0,data.nodes.Length).All(i=>(data.FindPath(0,i,stage)!=null)==data.Unlocked(data.nodes[i].requirement,stage)),"Every node follows its progression gate at "+(stage.flags.Count==0?"quest start":"quest completion"));
            Check(data.FindPath(0,2,new SaveData()).SequenceEqual(new[]{0,1,2}) && data.FindPath(2,3,new SaveData()).SequenceEqual(new[]{2,1,3}),"Expanded routes preserve the original H0-H2-H3 quest approach");
            open.node=data.nodes.Length-1; open.visited.Add(open.node);
            Check(SaveSystem.Write(open),"Expanded-node save written to isolated test storage"); game.Reload();
            Check(game.Save.node==data.nodes.Length-1 && game.Save.visited.Contains(data.nodes.Length-1),"Save reload preserves a current node and visited history above H9");
            var invalidLocation=new SaveData { node=data.nodes.Length+100,
                accepted=new List<string>{data.quest.id}, items=new List<string>{data.quest.item},
                visited=new List<int>{0,data.nodes.Length-1,-1,data.nodes.Length+100} };
            Check(SaveSystem.Write(invalidLocation),"Save with an invalid location written to isolated test storage"); game.Reload();
            Check(game.Save.node==0 && game.Quests.Active && game.Inventory.Has(data.quest.item) && game.Save.visited.SequenceEqual(new[]{0,data.nodes.Length-1}),"Invalid saved location resets to H0 while preserving quest, inventory and valid expanded-node history");
            SaveSystem.Reset();
        }
        private IEnumerator TestNodeTooltip(CityMap map)
        {
            var game=GameBootstrap.Instance;
            Check(map.SelectedNode==-1 && map.Tooltip!=null && !map.Tooltip.gameObject.activeSelf,"City opens with no selected node or tooltip");
            Check(Enumerable.Range(0,game.Data.nodes.Length).All(i=>Button("H"+i).GetComponentsInChildren<Text>().All(t=>string.IsNullOrWhiteSpace(t.text))),"All node names remain hidden until selection, leaving small map icons");
            yield return Click("MapOverview");
            var current=game.Save.node;
            yield return Click("H8",true);
            Check(map.SelectedNode==8 && map.Tooltip.gameObject.activeSelf && map.Tooltip.GetComponentsInChildren<Text>().Any(t=>t.text.Contains(game.Data.nodes[8].name)),"Tapping a locked node reveals only its selected-node tooltip");
            Check(!map.Moving && game.Save.node==current,"Selecting a locked node never moves Lumi or changes progress");
            Canvas.ForceUpdateCanvases();
            var image=map.Tooltip.GetComponent<Image>();
            Check(image!=null && map.Tooltip.GetComponentInChildren<RoundedPanelGraphic>().color.a>0 && map.Tooltip.GetComponentInChildren<RoundedPanelGraphic>().color.a<1 && map.Tooltip.rect.height<=map.View.Viewport.rect.height*.2f && map.Tooltip.rect.width<=map.View.Viewport.rect.width*.7f,"Selected-node tooltip is compact and has a translucent background");
            yield return Snapshot("03a-map-selected-tooltip");
            var empty=RectTransformUtility.WorldToScreenPoint(null,map.View.Viewport.TransformPoint(new Vector2(0,map.View.Viewport.rect.yMax-8)));
            yield return Pointer(empty,false);
            Check(map.SelectedNode==-1 && !map.Tooltip.gameObject.activeSelf,"Deselect hides the tooltip and returns to an icon-only map");
            yield return Snapshot("03c-map-overview");
        }
        private IEnumerator TestMapView(CityMap city)
        {
            var view=city.View;
            var background=GameObject.Find("HoiAnMapBackground").GetComponent<Image>();
            var art=DesignKitArt.Load();
            Check(art.hoiAnMap!=null&&background.sprite==art.hoiAnMap&&art.hoiAnMap.texture.width==1448&&art.hoiAnMap.texture.height==1086,"Provided Hoi An Map A is the actual runtime background at original resolution");
            Check(Mathf.Abs(view.Content.rect.width/view.Content.rect.height-4f/3)<.001f,"Landscape art fits portrait viewport without stretching");
            var hud=Button("Back").GetComponent<RectTransform>();
            var hudSize=hud.rect.size; var hudScale=hud.lossyScale;
            var center=RectTransformUtility.WorldToScreenPoint(null,view.Viewport.TransformPoint(view.Viewport.rect.center));
            yield return Click("H8");
            var initial=view.Zoom;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=center,scroll=new Vector2(0,120)});
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=center}); yield return null;
            Check(view.Zoom>initial,"Mouse wheel zooms actual map content");
            var beforePinch=view.Zoom;
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=11,phase=UnityEngine.InputSystem.TouchPhase.Began,position=center+Vector2.left*40});
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=12,phase=UnityEngine.InputSystem.TouchPhase.Began,position=center+Vector2.right*40});
            yield return null; yield return null;
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=11,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=center+Vector2.left*90+new Vector2(20,-10)});
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=12,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=center+Vector2.right*90+new Vector2(20,-10)});
            yield return null; yield return null;
            Check(view.Zoom>beforePinch*1.5f,"Two-finger touch pinch zooms the actual map");
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=11,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=center+Vector2.left*90});
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=12,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=center+Vector2.right*90});
            yield return new WaitForSecondsRealtime(.25f);
            var beforeDrag=view.Content.anchoredPosition;
            var start=center+new Vector2(-70,-75);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start+new Vector2(-90,55)}.WithButton(MouseButton.Left));
            yield return null; yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start+new Vector2(-90,55)});
            yield return new WaitForSecondsRealtime(.25f);
            Check(Vector2.Distance(beforeDrag,view.Content.anchoredPosition)>20,"Mouse drag pans map through EventSystem");
            Check(GameBootstrap.Instance.Save.node==0&&!city.Moving,"Pinch and drag do not trigger node travel");
            Check(!city.Tooltip.gameObject.activeSelf,"Pan and pinch leave the map free of a selection tooltip");
            Check(hud.rect.size==hudSize&&hud.lossyScale==hudScale,"HUD size remains fixed during map zoom and pan");
            var expectedAnchor=CityRouteGeometry.Arrival(GameBootstrap.Instance.Data.nodes[0]);
            Check(city.Marker.parent==view.Content && city.Marker.anchorMin==expectedAnchor && city.Marker.anchorMax==expectedAnchor && background.transform.parent==view.Content,"Lumi, nodes and background remain anchored in the same map coordinate space");
            view.ZoomBy(100); view.PanBy(new Vector2(100000,-100000));
            Check(Mathf.Abs(view.Zoom-MapPanZoom.MaxZoom)<.001f&&view.WithinBounds(),"Maximum zoom and extreme pan are clamped to map bounds");
            view.ZoomBy(.00001f); Check(Mathf.Abs(view.Zoom-MapPanZoom.MinZoom)<.001f&&view.WithinBounds(),"Minimum zoom clamps to full-map fit");
            yield return Click("ZoomIn"); yield return Click("ZoomIn"); yield return Click("FocusLumi");
            yield return Snapshot("03b-map-zoom-pan");
            yield return Click("MapOverview");
            Check(Mathf.Abs(view.Zoom-1)<.001f&&view.Content.anchoredPosition==Vector2.zero,"Overview button restores the entire uncropped illustration");
        }
        private IEnumerator Run()
        {
            yield return null; yield return null;
            if(!Application.isEditor)
            {
                // Recreate the swapchain: a process started hidden can otherwise capture a black initial buffer.
                Screen.SetResolution(541,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.2f);
                Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            }
            var game=GameBootstrap.Ensure();
            var art=DesignKitArt.Load();
            Check(art!=null && new[]{art.lumiPortrait,art.lumiExploration,art.lumiMapTraveler,art.maiPortrait,art.lumiCity,art.lumiCitySmall,art.available,art.current,art.visited,art.locked,art.npc,art.quest,art.item,art.enemy,art.puzzle,art.portal,art.secret,art.hoiAnMap}.All(s=>s!=null),"All runtime art roles are imported and referenced");
            bool verify=Environment.GetCommandLineArgs().Contains("--verify-save");
            if(verify)
            {
                Check(game.Quests.Completed&&game.Save.flags.Contains("lantern-lit")&&game.Save.collected.Count==1&&!game.Inventory.Has(game.Data.quest.item),"Save restored after process restart");
                yield break;
            }
            SaveSystem.Reset();
            TestExpandedMapData(game);
            Check(new[]{art.lumiMapTraveler,art.lumiPortrait,art.lumiExploration}.Distinct().Count()==3,"Lumi map marker, dialogue portrait and exploration sprite use three distinct sprites");
            Check(art.lumiPortrait.rect.xMin==0 && art.lumiPortrait.rect.width==art.lumiPortrait.texture.width && art.lumiExploration.pivot.y<1 && art.lumiMapTraveler!=art.lumiCitySmall,"Dialogue preserves the whole hand; exploration has a foot pivot; map uses a chibi, not a badge");
            File.WriteAllText(SaveSystem.PathName,"{\"version\":1,\"node\":2,\"accepted\":[\"mai-lantern\"],\"completed\":[],\"items\":[],\"collected\":[],\"flags\":[]}");
            game.Reload();
            Check(game.Quests.Active&&game.Save.node==2&&game.Save.visited.Contains(2),"Legacy v1 save migrates optional visited history without losing quest");
            yield return Click("Reset"); yield return Click("CancelReset");
            Check(game.Quests.Active&&game.Save.node==2,"Cancel reset preserves save");
            yield return Click("Reset"); yield return Click("ConfirmReset");
            Check(GameObject.Find("CanonicalLumiPortrait").GetComponent<Image>().sprite==art.lumiPortrait,"Main menu displays the dedicated canonical Lumi portrait");
            yield return Snapshot("01-menu");
            Check(game.Save.node==0&&!game.Quests.Active,"Fresh save starts at H0");
            Check(game.Data.FindPath(0,5,game.Save)==null&&game.Data.FindPath(0,8,game.Save)==null,"Locked nodes and edges rejected");
            Check(!game.Quests.CollectPart(),"Cannot collect before quest acceptance");
            string vietnamese="Hội An Huế Đà Lạt Cô Mai linh kiện đèn Nhịp Cầu Ánh Sáng";
            Check(vietnamese.All(c=>c==' '||UIController.Font.HasCharacter(c)),"Bundled runtime font contains Vietnamese glyphs");
            yield return Click("Start"); yield return Scene("WorldMap");
            yield return Snapshot("02-world-map");
            Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b=>b.name.StartsWith("City")&&!b.interactable)==5,"Five cities remain locked");
            yield return Click("City0",true); yield return Scene("CityMap");
            var map=FindAnyObjectByType<CityMap>(); var before=map.Marker.position;
            Check(GameObject.Find("Title").GetComponent<Text>().text.Contains("HỘI AN") && !string.IsNullOrWhiteSpace(Button("Interact").GetComponentInChildren<Text>().text),"Compact city UI preserves its Vietnamese title and action label");
            Check(map.Marker.GetComponent<Image>().sprite==art.lumiMapTraveler,"City position indicator uses the dedicated small Lumi map marker");
            Check(Button("H0").transform.Find("State").GetComponent<Image>().sprite==art.current && Button("H5").transform.Find("State").GetComponent<Image>().sprite==art.locked,"Current and locked node sprites match gameplay state");
            Check(Button("H2").transform.Find("Kind").GetComponent<Image>().sprite==art.npc && Button("H4").transform.Find("Kind").GetComponent<Image>().sprite==art.puzzle && Button("H5").transform.Find("Kind").GetComponent<Image>().sprite==art.enemy && Button("H8").transform.Find("Kind").GetComponent<Image>().sprite==art.portal && Button("H9").transform.Find("Kind").GetComponent<Image>().sprite==art.secret,"NPC, puzzle, bot, portal and secret PNGs attached to designed nodes");
            Check(map.View.Viewport.rect.height/((RectTransform)map.View.Viewport.parent).rect.height>=.70f,"Map viewport uses at least 70 percent of portrait content");
            Check(Enumerable.Range(0,22).All(i=>Button("H"+i).GetComponent<RectTransform>().rect.width*Button("H"+i).transform.lossyScale.x>=44),"All 22 node touch targets remain at least 44 screen pixels");
            Check(Enumerable.Range(0,22).All(i=>map.IsNearestNode(i,Button("H"+i).transform.position)),"Every node center wins selection even in dense clusters");
            yield return Snapshot("03-city-map");
            Screen.SetResolution(768,1024,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            yield return Snapshot("03d-city-map-tablet");
            Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            yield return TestNodeTooltip(map);
            yield return TestMapView(map);
            yield return Click("H2",true);
            Check(map.Moving&&game.Save.node==0&&map.Marker.position!=before,"Touch begins animated node movement without teleport");
            yield return new WaitForSecondsRealtime(.25f);
            var route=CityRouteGeometry.Points(game.Data,0,1);
            var point=map.Marker.anchorMin; float nearest=float.MaxValue;
            for(var i=1;i<route.Count;i++)
            {
                var delta=route[i]-route[i-1]; var t=Mathf.Clamp01(Vector2.Dot(point-route[i-1],delta)/delta.sqrMagnitude);
                nearest=Mathf.Min(nearest,Vector2.Distance(point,route[i-1]+delta*t));
            }
            Check(route.Count>2&&nearest<.0001f,"Lumi follows the rendered route bends rather than cutting straight across the map");
            yield return Until(()=>!map.Moving&&game.Save.node==2,"Reach H2");
            Check(!game.Quests.Active,"Mai presents the quest before explicit acceptance");
            Check(!map.Tooltip.gameObject.activeSelf,"Node tooltip hides while the NPC dialogue is open");
            Check(Button("H0").transform.Find("State").GetComponent<Image>().sprite==art.visited && game.Save.visited.Contains(1),"Traversed nodes use visited icon and persist history");
            Check(GameObject.Find("SpeakerPortrait").GetComponent<Image>().sprite==art.maiPortrait && GameObject.Find("Speaker").GetComponent<Text>().text=="CÔ MAI","Mai is the displayed speaker with her own portrait");
            yield return Snapshot("04-mai-dialogue");
            yield return Click("DialogueAction");
            Check(game.Quests.Active && GameObject.Find("SpeakerPortrait").GetComponent<Image>().sprite==art.lumiPortrait && GameObject.Find("Speaker").GetComponent<Text>().text=="LUMI","Explicit acceptance shows the hand-preserving Lumi reply");
            yield return Snapshot("04c-lumi-reply");
            yield return Click("Continue");
            Check(Button("H3").transform.Find("Kind").gameObject.activeSelf && Button("H3").transform.Find("Kind").GetComponent<Image>().sprite==art.item,"Active uncollected quest displays supplied item PNG at H3");
            yield return Snapshot("04b-quest-and-item-markers");
            yield return Click("H3"); yield return Scene("HoiAnDemo");
            Check(game.Save.node==3,"Exploration entered from H3");
            var player=FindAnyObjectByType<HoiAnPlayerController>(); var item=FindAnyObjectByType<HoiAnItem>();
            Check(item!=null&&item.gameObject.activeSelf,"Uncollected item exists in world");
            Check(player.transform.Find("CanonicalLumiVisual").GetComponent<SpriteRenderer>().sprite==art.lumiExploration && item.transform.Find("ProvidedItemVisual").GetComponent<SpriteRenderer>().sprite==art.item && player.GetComponent<BoxCollider2D>()!=null,"H3 uses the dedicated Lumi exploration sprite with the original player collider");
            var camera=Camera.main; var follow=camera.GetComponent<FollowCamera2D>();
            Check(follow!=null,"Follow camera serialized in generated scene");
            Check(player.GetComponent<LumiStepMotion>()!=null && art.lumiExploration!=art.lumiPortrait,"H3 uses a standing chibi with a lightweight step cue, independent of the explaining portrait");
            yield return Snapshot("05-exploration-portrait");
            Screen.SetResolution(768,1024,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            yield return Snapshot("05b-exploration-tablet");
            Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            if(!Application.isEditor)
            {
                Screen.SetResolution(1280,720,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
                yield return Snapshot("06-exploration-landscape");
                Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            }
            foreach(var aspect in new[]{9f/16,16f/9})
            {
                camera.aspect=aspect; follow.Snap(); var viewport=camera.WorldToViewportPoint(player.transform.position);
                Check(viewport.x>.1f&&viewport.x<.9f&&viewport.y>.2f&&viewport.y<.79f,"Player visible outside HUD at aspect "+aspect);
            }
            camera.ResetAspect();
            Check(!player.MoveTo(new Vector2(-5,1.8f)),"House collider rejects destination");
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));
            yield return new WaitForSecondsRealtime(.8f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState()); yield return null;
            Check(player.Position.y<.14f&&player.Position.y>-.6f,"WASD movement stops at house collider");
            Check(player.MoveTo(new Vector2(-2,2)),"Path around house found");
            yield return Until(()=>!player.Walking,"Walk around house");
            Check(Vector2.Distance(player.Position,new Vector2(-2,2))<.12f&&HoiAnPlayerController.Walkable(player.Position),"Physical player reached route around obstacle");
            // Inject real touch on visible ground, then use the interaction button near the item.
            var target=player.Position+new Vector2(.8f,0);
            yield return Pointer(camera.WorldToScreenPoint(target),true);
            yield return Until(()=>!player.Walking,"Touch movement");
            Check(Vector2.Distance(player.Position,target)<.15f,"Touchscreen ground input moves physical player");
            target=player.Position+new Vector2(.6f,0);
            yield return Pointer(camera.WorldToScreenPoint(target),false); yield return Until(()=>!player.Walking,"Mouse movement");
            Check(Vector2.Distance(player.Position,target)<.15f,"Mouse ground input moves physical player (target "+target+", actual "+player.Position+", mouse "+Mouse.current.deviceId+"/"+mouse.deviceId+")");
            Check(player.MoveTo((Vector2)item.transform.position+Vector2.left*.8f),"Route to interaction range of lantern part found");
            yield return Until(()=>!player.Walking,"Walk to item");
            yield return Snapshot("07-item-closeup");
            yield return Click("Collect",true);
            Check(!item.gameObject.activeSelf&&game.Inventory.Has(game.Data.quest.item)&&game.Save.collected.Count==1,"Touch interaction collects item once");
            Check(!game.Quests.CollectPart(),"Duplicate pickup rejected");
            yield return Click("Return"); yield return Scene("CityMap");
            Check(game.Save.node==3,"Return restores exactly H3");
            Check(!Button("H3").transform.Find("Kind").gameObject.activeSelf,"Collected-item marker disappears from city map");
            yield return Snapshot("08-map-after-pickup");
            yield return Click("Interact"); yield return Scene("HoiAnDemo");
            Check(FindAnyObjectByType<HoiAnItem>()==null,"Item remains absent after leaving and re-entering scene");
            yield return Click("Return",true); yield return Scene("CityMap");
            yield return Click("H2");
            yield return Until(()=>GameObject.Find("MaiDialogue")!=null,"Arrive at Mai for turn-in");
            yield return Click("DialogueAction");
            yield return Until(()=>game.Quests.Completed,"Turn in at Mai");
            Check(game.Save.flags.Contains("lantern-lit")&&!game.Inventory.Has(game.Data.quest.item),"Turn-in consumes item and unlocks reward route");
            Check(game.Data.FindPath(2,7,game.Save)!=null&&game.Data.FindPath(2,8,game.Save)==null,"Reward nodes open while future content stays locked");
            yield return Click("Continue");
            Check(!Button("H2").transform.Find("QuestBadge").gameObject.activeSelf && Button("H5").transform.Find("State").GetComponent<Image>().sprite==art.available,"Turn-in clears quest badge and updates unlocked node icon");
            yield return Snapshot("09-map-reward-unlocked");
            if(!Application.isEditor)
            {
                Screen.SetResolution(1280,720,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
                yield return Click("MapOverview");
                yield return Snapshot("10-map-landscape");
                var view=FindAnyObjectByType<CityMap>().View;
                Check(view.WithinBounds()&&Mathf.Abs(view.Content.rect.width/view.Content.rect.height-4f/3)<.001f,"Map keeps aspect and bounds after landscape resize");
                Screen.SetResolution(768,1024,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
                yield return Click("MapOverview");
                yield return Snapshot("11-map-tablet");
                Check(view.WithinBounds()&&Mathf.Abs(view.Content.rect.width/view.Content.rect.height-4f/3)<.001f,"Map keeps aspect and bounds at tablet 3:4");
                Screen.SetResolution(540,960,FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            }
            yield return Click("MapOverview");
            yield return Click("H7",true);
            yield return Until(()=>!FindAnyObjectByType<CityMap>().Moving&&game.Save.node==7,"Travel reward route");
            game.Quests.TalkToMai(); Check(game.Save.completed.Count==1,"Completed dialogue does not grant duplicate rewards");
            Check(game.Persist(),"Save written to disk"); game.Reload();
            Check(game.Quests.Completed&&game.Save.node==7&&game.Save.collected.Count==1,"Quest, inventory, world and node restored from disk");
            Check(FindObjectsByType<GameBootstrap>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"No duplicate manager, canvas or EventSystem after transitions");
        }
    }
}
#endif
