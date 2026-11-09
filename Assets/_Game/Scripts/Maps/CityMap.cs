using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class CityMap : MonoBehaviour
    {
        public bool Moving { get; private set; }
        public RectTransform Marker { get; private set; }
        public MapPanZoom View { get; private set; }
        public int SelectedNode { get; private set; } = -1;
        public RectTransform Tooltip { get; private set; }
        private GameBootstrap game;
        private string renderedCityId;
        private UIController ui;
        private DesignKitArt art;
        private AdventureUIArt presentation;
        private RectTransform map, markerShadow;
        private RectTransform ferryVisual;
        private RectTransform companion;
        public string ActiveTravelMode { get; private set; } = "walk";
        public bool LumiOccluded { get; private set; }
        private CityMapPaths paths;
        private Text tooltipName, tooltipState;
        private Text chapterProgress,overviewHint;
        private readonly List<Button> nodes = new List<Button>();
        private readonly List<Button> mapControls = new List<Button>();
        private Button action, back;
        private Button motionButton;
        private bool dialogue, rendered, restored;
        private float resumeInputAt;
        private GameplayPanels panels;
        private readonly List<bool> shown=new List<bool>();
        private readonly List<Image> actors=new List<Image>();
        private readonly List<MapActorDefinition> actorDefinitions=new List<MapActorDefinition>();
        private readonly List<NodeMarkerView> nodeMarkers=new List<NodeMarkerView>();
        private NodeMarkerResolver markerResolver;
        private float nextMarkerRefresh;
        public IReadOnlyList<NodeMarkerView> NodeMarkers=>nodeMarkers;
        public int PendingDestination { get; private set; } = -1;
        public bool DialogueOpen => dialogue;

        private void Start()
        {
            game = GameBootstrap.Ensure();
            renderedCityId=game.Data.cityId;
            art = DesignKitArt.Load();
            presentation=AdventureUIArt.Load();
            var city=game.Content.City(game.Data.cityId);
            ui = UIController.Create(string.IsNullOrEmpty(city.mapTitle)?city.name.ToUpperInvariant():city.mapTitle, true);
            CompactHeader();
            var viewport = ui.Panel(ui.Root, "MapViewport", Vector2.zero, Vector2.one, DesignKitArt.Hex("193E49"));
            viewport.GetComponent<Image>().color=Color.white;
            MobileUI.Skin(viewport.GetComponent<Image>(),AdventureUIArt.Load().backdrop,2);
            viewport.offsetMin = new Vector2(0,82);
            viewport.offsetMax = new Vector2(0,-66);
            viewport.SetAsFirstSibling();
            viewport.gameObject.AddComponent<RectMask2D>();
            overviewHint=ui.Label(viewport,"OverviewHint","Toàn cảnh · Kéo và chụm hai ngón để xem gần",new Vector2(.07f,.04f),new Vector2(.93f,.09f),15,false);
            overviewHint.color=DesignKitArt.Ink;
            map = new GameObject("CityMap", typeof(RectTransform)).GetComponent<RectTransform>();
            map.SetParent(viewport, false);
            var mapSprite=game.Data.cityId=="hoi_an"?art.hoiAnMap:presentation.City(game.Data.cityId)?.sprite;
            var background = ui.Picture(map,game.Data.cityId=="hoi_an"?"HoiAnMapBackground":"CityMapBackground",mapSprite,Vector2.zero,Vector2.one);
            background.raycastTarget = true;
            Canvas.ForceUpdateCanvases();
            View = viewport.gameObject.AddComponent<MapPanZoom>();
            View.Configure(map, mapSprite.rect.width / mapSprite.rect.height);
            CreateLeisureDecorations();
            View.BackgroundTapped = Deselect;
            paths = map.gameObject.AddComponent<CityMapPaths>();
            paths.Configure(game, ui, map);
            markerResolver=new NodeMarkerResolver(game);
            var markerLayer=new GameObject("NodeMarkerLayer",typeof(RectTransform)).GetComponent<RectTransform>();
            markerLayer.SetParent(map,false);UIController.SetRect(markerLayer,Vector2.zero,Vector2.one);

            for (var i = 0; i < game.Data.nodes.Length; i++)
            {
                var node = i;
                var data = game.Data.nodes[i];
                // Ground the interaction presentation at the existing walkable arrival.
                // Canonical node coordinates, navigation terminals and movement stay untouched.
                var anchor=CityRouteGeometry.Arrival(data);
                var button = ui.Button(map, data.id, "", anchor, anchor, () => TryMove(node));
                ((RectTransform)button.transform).sizeDelta = new Vector2(52,52);
                button.GetComponent<Image>().color = Color.clear;
                button.GetComponent<Outline>().enabled = false;
                button.transform.Find("Label").gameObject.SetActive(false);
                var markerObject=new GameObject("NodeMarker_"+data.id,typeof(RectTransform),typeof(NodeMarkerView));
                markerObject.transform.SetParent(markerLayer,false);
                UIController.SetRect((RectTransform)markerObject.transform,anchor,anchor);
                var markerView=markerObject.GetComponent<NodeMarkerView>();markerView.Configure(ui,data.id,art.available,i,()=>SoftAudio.Action(()=>TryMove(node),null));
                var markerHit=markerObject.AddComponent<MapNodeHitTarget>();markerHit.owner=this;markerHit.index=i;
                nodeMarkers.Add(markerView);button.targetGraphic=markerView.StateImage;
                button.colors = new ColorBlock { normalColor=Color.white, highlightedColor=Color.white,
                    pressedColor=new Color(.8f,.9f,.85f), selectedColor=Color.white, disabledColor=new Color(1,1,1,.8f), colorMultiplier=1, fadeDuration=.1f };
                var actorData=presentation.MapActor(game.Data.cityId,data.id);
                actorDefinitions.Add(actorData);
                Image actor=null;
                if(actorData!=null && presentation.ActorSprite(actorData.artId)!=null)
                {
                    var actorSize=presentation.ActorSize(game.Data.cityId,actorData);
                    var actorScale=presentation.ActorScale(game.Data.cityId,actorData);
                    var shadow=ui.Picture(button.transform,"ActorShadow",PresentationShapes.Shadow,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
                    shadow.rectTransform.sizeDelta=new Vector2(actorSize.x*.55f,9*actorScale);
                    shadow.rectTransform.anchoredPosition=actorData.offset;
                    var actorHit=ui.Panel(button.transform,"ActorHitArea",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Color.clear);
                    actorHit.sizeDelta=new Vector2(Mathf.Max(presentation.mapActorTouchSize,actorSize.x),Mathf.Max(presentation.mapActorTouchSize,actorSize.y));
                    actorHit.anchoredPosition=actorData.offset+Vector2.up*actorSize.y*.5f;
                    actor=ui.Picture(button.transform,"MapActor_"+data.id,presentation.ActorSprite(actorData.artId),new Vector2(.5f,.5f),new Vector2(.5f,.5f));
                    actor.rectTransform.pivot=new Vector2(.5f,0);actor.rectTransform.sizeDelta=actorSize;actor.rectTransform.anchoredPosition=actorData.offset;
                    actor.raycastTarget=true;
                    actor.useSpriteMesh=true;
                    // The image bubbles clicks to the existing node button and shares
                    // its discovery, modal and movement permissions. No second target.
                }
                actors.Add(actor);
                var hit=button.gameObject.AddComponent<MapNodeHitTarget>(); hit.owner=this; hit.index=i;
                nodes.Add(button); shown.Add(false);
                if (game.Data.cityId=="hoi_an"&&i==2) button.gameObject.AddComponent<HoiAn.HoiAnNpc>();
            }
            markerLayer.SetAsLastSibling();
            markerShadow = ui.Picture(map,"LumiFootShadow",PresentationShapes.Shadow,Vector2.zero,Vector2.zero).rectTransform;
            markerShadow.sizeDelta=new Vector2(34,12);
            Marker = ui.Picture(map, "Lumi", art.lumiMapTraveler, Vector2.zero, Vector2.zero).rectTransform;
            Marker.pivot = new Vector2(.5f,0);
            Marker.sizeDelta = new Vector2(44,44);
            companion=ui.Picture(Marker,"MiniLanternCompanion",presentation.Item("lamp_eye")?.sprite,Vector2.zero,Vector2.zero).rectTransform;
            companion.anchorMin=companion.anchorMax=new Vector2(0,.55f);companion.sizeDelta=new Vector2(16,20);companion.anchoredPosition=new Vector2(-10,7);
            companion.GetComponent<Image>().raycastTarget=false;companion.gameObject.SetActive(game.World.Has("hoi_an.lost_lantern_rescued"));
            ferryVisual=ui.Picture(Marker,"FerryTransport",Resources.Load<Sprite>("LeisureProps/Hue_PedalSwanBoat"),Vector2.zero,Vector2.zero).rectTransform;
            ferryVisual.anchorMin=ferryVisual.anchorMax=new Vector2(.5f,0);
            ferryVisual.sizeDelta=new Vector2(72,44);ferryVisual.anchoredPosition=new Vector2(0,-6);
            ferryVisual.GetComponent<Graphic>().raycastTarget=false;ferryVisual.gameObject.SetActive(false);
            CreateFooter();
            panels=new GameplayPanels(ui,game,art,()=>{dialogue=false;resumeInputAt=Time.unscaledTime+.25f;Refresh();},OnEventChanged);
            CreateGuidance();
            CreateJourneyShop();
            CreateCatRescue();
            restored=game.World.Has(city.completionFlag);
            InitializeQuestLight();
            Tooltip = ui.Panel(ui.Root,"NodeTooltip",Vector2.zero,Vector2.zero,new Color(.07f,.19f,.22f,.9f));
            Tooltip.sizeDelta = new Vector2(240,54);
            Tooltip.pivot = new Vector2(.5f,0);
            Tooltip.GetComponent<Image>().raycastTarget = false;
            MobileUI.Round(Tooltip);
            tooltipName = ui.Label(Tooltip,"NodeName","",new Vector2(.05f,.43f),new Vector2(.95f,.94f),17);
            tooltipState = ui.Label(Tooltip,"NodeState","",new Vector2(.05f,.06f),new Vector2(.95f,.44f),12);
            tooltipState.color = new Color(.76f,.86f,.80f);
            Deselect();
            PositionMarker(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));
            // Use more of a tall phone display; Overview always returns the complete image.
            View.ZoomBy(Mathf.Clamp(viewport.rect.height/map.rect.height,1,MapPanZoom.MaxZoom));
            View.Focus(Marker.anchorMin);
            Refresh();
            if(PlayerCopy.AutoGuide(game)){dialogue=true;Refresh();panels.ShowPlayerGuide();}
        }

        private void CompactHeader()
        {
            var title = ui.Root.Find("Title").GetComponent<Text>();
            UIController.SetRect(title.rectTransform,new Vector2(0,1),new Vector2(1,1));
            title.rectTransform.offsetMin = new Vector2(16,-30);
            title.rectTransform.offsetMax = new Vector2(-138,-5);
            title.alignment = TextAnchor.MiddleLeft;
            title.resizeTextForBestFit=false;title.fontSize=20;title.color=DesignKitArt.Ink;
            chapterProgress=ui.Label(ui.Root,"ChapterProgress","",new Vector2(0,1),Vector2.one,14,false);chapterProgress.alignment=TextAnchor.MiddleLeft;chapterProgress.color=DesignKitArt.Teal;
            chapterProgress.rectTransform.offsetMin=new Vector2(16,-53);chapterProgress.rectTransform.offsetMax=new Vector2(-138,-31);
            var panel = (RectTransform)ui.Root.Find("MessagePanel");
            panel.gameObject.SetActive(false);
            var journal=MobileUI.IconButton(ui,ui.Root,"QuestJournal","Nhật ký",Vector2.one,Vector2.one,ShowJournal,AdventureSymbol.Journal);
            var inventory=MobileUI.IconButton(ui,ui.Root,"Inventory","Túi đồ",Vector2.one,Vector2.one,ShowInventory,sprite:AdventureUIArt.Load().backpack);
            var ir=(RectTransform)inventory.transform; ir.offsetMin=new Vector2(-132,-61); ir.offsetMax=new Vector2(-74,-3);
            var jr=(RectTransform)journal.transform; jr.offsetMin=new Vector2(-68,-61); jr.offsetMax=new Vector2(-10,-3);
            var backdrop=ui.Root.parent.Find("Backdrop").GetComponent<Image>();backdrop.color=DesignKitArt.Cream;
        }

        private Button Pill(string name,string label,Vector2 min,Vector2 max,UnityEngine.Events.UnityAction click)
        {
            return MobileUI.Button(ui,ui.Root,name,label,min,max,click,primary:name=="Interact",illustrated:true);
        }

        private void CreateFooter()
        {
            back=MobileUI.IconButton(ui,ui.Root,"Back","Thế giới",Vector2.zero,Vector2.zero,()=>game.Scenes.OpenWorld(),AdventureSymbol.Globe);
            var rect=(RectTransform)back.transform;rect.offsetMin=new Vector2(14,10);rect.offsetMax=new Vector2(76,72);
            action=Pill("Interact","Xem nhiệm vụ",Vector2.zero,new Vector2(1,0),Interact);
            rect=(RectTransform)action.transform;rect.offsetMin=new Vector2(88,17);rect.offsetMax=new Vector2(-14,65);
            MobileUI.Cue(action,AdventureSymbol.Chat);
            var names=new[]{"ZoomOut","ZoomIn","MapOverview","FocusLumi"};
            var symbols=new[]{AdventureSymbol.Minus,AdventureSymbol.Plus,AdventureSymbol.Overview,AdventureSymbol.Overview};
            UnityEngine.Events.UnityAction[] clicks={()=>View.ZoomBy(1/1.3f),()=>View.ZoomBy(1.3f),()=>{View.ResetView();Deselect();},()=>View.Focus(Marker.anchorMin)};
            var x=-244f;
            for(var i=0;i<names.Length;i++)
            {
                var button=MobileUI.IconButton(ui,ui.Root,names[i],new[]{"Thu nhỏ","Phóng to","Toàn cảnh","Lumi"}[i],new Vector2(1,0),new Vector2(1,0),clicks[i],symbols[i],i==3?art.lumiMapTraveler:null);
                rect=(RectTransform)button.transform; rect.offsetMin=new Vector2(x,91); rect.offsetMax=new Vector2(x+52,143);
                var caption=button.transform.Find("Label").GetComponent<Text>();caption.resizeTextForBestFit=true;caption.resizeTextMinSize=10;caption.resizeTextMaxSize=14;
                x+=58; mapControls.Add(button);
            }
            motionButton=MobileUI.IconButton(ui,ui.Root,"MarkerMotion",MotionCaption(),new Vector2(1,0),new Vector2(1,0),()=>
            {
                if(dialogue||Moving)return;dialogue=true;Refresh();panels.ShowMapMotion();
            },AdventureSymbol.Motion);
            rect=(RectTransform)motionButton.transform;rect.offsetMin=new Vector2(-302,91);rect.offsetMax=new Vector2(-250,143);mapControls.Add(motionButton);
            var motionLabel=motionButton.transform.Find("Label").GetComponent<Text>();motionLabel.resizeTextForBestFit=true;motionLabel.resizeTextMinSize=10;motionLabel.resizeTextMaxSize=14;
        }
        private static string MotionCaption()=>"Hiệu ứng";

        public bool IsNearestNode(int index,Vector2 screen)
        {
            var distance=HitDistance(index,screen);
            for(var i=0;i<nodes.Count;i++)
                if(i!=index && nodes[i].gameObject.activeInHierarchy && nodes[i].interactable && HitDistance(i,screen)<distance-.01f) return false;
            return true;
        }
        private float HitDistance(int index,Vector2 screen)
        {
            var rect=(RectTransform)nodes[index].transform;
            var hitArea=nodeMarkers[index].HitArea;
            if(hitArea.gameObject.activeInHierarchy&&RectTransformUtility.RectangleContainsScreenPoint(hitArea,screen,null))
                return Vector2.SqrMagnitude(screen-(Vector2)hitArea.position);
            var actor=actors[index];
            if(actor!=null && actor.gameObject.activeInHierarchy)
            {
                var actorHit=(RectTransform)actor.transform.parent.Find("ActorHitArea");
                if(RectTransformUtility.RectangleContainsScreenPoint(actorHit,screen,null))
                    return Vector2.SqrMagnitude(screen-(Vector2)actorHit.position);
            }
            return RectTransformUtility.RectangleContainsScreenPoint(rect,screen,null) ? Vector2.SqrMagnitude(screen-(Vector2)rect.position) : float.PositiveInfinity;
        }

        private void ShowJournal()
        {
            if(dialogue || Moving) return;
            ShowTextPanel("QuestJournalPanel","NHẬT KÝ HÀNH TRÌNH",PlayerCopy.Journal(game));
        }
        private void ShowInventory()
        {
            if(dialogue || Moving) return;
            dialogue=true;Deselect();Refresh();
            InventoryPanel.Show(ui,game,()=>{dialogue=false;resumeInputAt=Time.unscaledTime+.25f;Refresh();},ShowJournal);
        }
        private void ShowTextPanel(string name,string title,string text)
        {
            dialogue=true; Refresh();
            var panel=ui.Panel(ui.Root,name,new Vector2(.04f,.15f),new Vector2(.96f,.82f),DesignKitArt.Cream); MobileUI.Round(panel);
            panel.gameObject.AddComponent<PortraitPresence>().slide=true;
            ui.Label(panel,"Heading",title,new Vector2(.05f,.88f),new Vector2(.95f,.97f),22).color=DesignKitArt.Ink;
            var viewport=ui.Panel(panel,"JournalViewport",new Vector2(.06f,.16f),new Vector2(.94f,.86f),Color.clear);viewport.gameObject.AddComponent<RectMask2D>();
            var body=ui.Label(viewport,"Body",text,new Vector2(0,1),Vector2.one,18,false);body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperLeft;
            body.rectTransform.pivot=new Vector2(.5f,1);body.rectTransform.sizeDelta=new Vector2(0,200);
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            var exit=MobileUI.Button(ui,panel,"CloseJournal","Đóng nhật ký",new Vector2(.20f,.04f),new Vector2(.80f,.13f),()=>{panel.gameObject.SetActive(false);Destroy(panel.gameObject);dialogue=false;resumeInputAt=Time.unscaledTime+.25f;Refresh();});
            MobileUI.Cue(exit,AdventureSymbol.Close);
            if(name=="QuestJournalPanel")
            {
                UIController.SetRect((RectTransform)exit.transform,new Vector2(.68f,.04f),new Vector2(.95f,.13f));
                var collection=MobileUI.Button(ui,panel,"OpenRelicCollection","Di vật · Bưu thiếp",new Vector2(.04f,.04f),new Vector2(.35f,.13f),()=>{panel.gameObject.SetActive(false);Destroy(panel.gameObject);panels.ShowRelicCollection();},true,true);
                MobileUI.Cue(collection,AdventureSymbol.Check);
                MobileUI.Button(ui,panel,"JournalGuide","Cách chơi",new Vector2(.37f,.04f),new Vector2(.66f,.13f),()=>{panel.gameObject.SetActive(false);Destroy(panel.gameObject);panels.ShowPlayerGuide();},illustrated:true);
            }
        }

        private void OnEventChanged()
        {
            game.Discovery.Refresh(); game.Persist(); Refresh();
            if(!restored && game.World.Has(game.Content.City(game.Data.cityId).completionFlag))
            { restored=true; if(game.Data.cityId!="hoi_an")StartCoroutine(RestoreLight()); }
        }
        private IEnumerator RestoreLight()
        {
            var glow=ui.Panel(View.Viewport,"RestoredLight",Vector2.zero,Vector2.one,new Color(1,.83f,.32f,.3f));
            var picture=glow.GetComponent<Image>(); picture.raycastTarget=false;
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/1.1f)
            { picture.color=new Color(1,.83f,.32f,.3f*(1-t)); yield return null; }
            Destroy(glow.gameObject);
        }
        private IEnumerator RevealNode(Button node)
        {
            var group=node.GetComponent<CanvasGroup>();if(group==null)group=node.gameObject.AddComponent<CanvasGroup>();
            for(float t=0;t<1;t+=Time.unscaledDeltaTime/.55f) { group.alpha=t; yield return null; }
            group.alpha=1;
        }

        private void LateUpdate()
        {
            if(game==null||game.Data.cityId!=renderedCityId)return;
            if (View == null || Tooltip == null) return;
            if(Time.unscaledTime>=nextMarkerRefresh){nextMarkerRefresh=Time.unscaledTime+.2f;Refresh();}
            overviewHint.gameObject.SetActive(View.Zoom<1.05f&&!dialogue);
            // Only positions follow map zoom; touch targets and symbols stay compact.
            var scale = new Vector3(1/View.Zoom,1/View.Zoom,1);
            foreach (var button in nodes) button.transform.localScale = scale;
            foreach(var nodeMarker in nodeMarkers)nodeMarker.SetZoom(View.Zoom);
            for(var i=0;i<actors.Count;i++)PositionGroundedActor(i);
            Marker.localScale = scale; markerShadow.localScale=scale;
            AnimateQuestReaction(scale);
            AnimateRelicPresentation();
            if (View.SuppressSelection) Deselect();
            if(SelectedNode>=0&&!game.Discovery.IsVisible(SelectedNode))Deselect();
            if (SelectedNode < 0 || dialogue || View.SuppressSelection) { Tooltip.gameObject.SetActive(false); return; }
            var world = nodes[SelectedNode].transform.position;
            var point = (Vector2)ui.Root.InverseTransformPoint(world);
            var viewMin = (Vector2)ui.Root.InverseTransformPoint(View.Viewport.TransformPoint(View.Viewport.rect.min));
            var viewMax = (Vector2)ui.Root.InverseTransformPoint(View.Viewport.TransformPoint(View.Viewport.rect.max));
            if (point.x < viewMin.x || point.x > viewMax.x || point.y < viewMin.y || point.y > viewMax.y)
            { Tooltip.gameObject.SetActive(false); return; }
            Tooltip.gameObject.SetActive(true);
            var width = Mathf.Min(260,View.Viewport.rect.width*.68f);
            Tooltip.sizeDelta = new Vector2(width,54);
            var actor=actors[SelectedNode];
            point.y += actor!=null&&actor.gameObject.activeInHierarchy ? actor.rectTransform.anchoredPosition.y+actor.rectTransform.rect.height+8 : SelectedNode==game.Save.node?52:27;
            if (point.y+54 > viewMax.y-8) point.y -= 108;
            point.x = Mathf.Clamp(point.x,viewMin.x+width*.5f+8,viewMax.x-width*.5f-8);
            point.y = Mathf.Clamp(point.y,viewMin.y+8,viewMax.y-62);
            Tooltip.localPosition = new Vector3(point.x,point.y,0);
        }

        public void Deselect()
        { SelectedNode = -1; if (Tooltip != null) Tooltip.gameObject.SetActive(false); if(paths!=null)paths.Highlight(null); }

        public void TryMove(int destination)
        {
            if (dialogue || Time.unscaledTime<resumeInputAt || game.Scenes.Busy || View.SuppressSelection || destination < 0 || destination >= nodes.Count || !game.Discovery.IsVisible(destination)) return;
            if(Moving)
            {
                // Accept a new destination now, turn at the next real junction.
                // Never snap back to the previous canonical gameplay node.
                if(game.Data.navigation!=null&&game.Data.navigation.NodeOpen(game.Data,destination,game.Save))
                {requestedRouteDestination=destination;PendingDestination=destination;SelectedNode=destination;tooltipName.text=game.Data.nodes[destination].name;tooltipState.text="Đổi hướng ở ngã rẽ kế tiếp";}
                return;
            }
            SoftAudio.Cue(SoftCue.UI_Click);
            SelectedNode = destination;
            var data = game.Data.nodes[destination];
            tooltipName.text = data.name;
            var unlocked = game.Data.Unlocked(data.requirement,game.Save);
            tooltipState.text = game.Discovery.State(destination)==MapNodeState.VISIBLE_LOCKED && data.isKeyGate ? "Mốc khám phá · Tương tác để mở chặng" : !unlocked ? "Chưa đủ điều kiện mở đường" :
                destination == game.Save.node ? "Lumi đang ở đây" : KindLabel(data.kind)+" · Đã mở";
            Tooltip.gameObject.SetActive(true);
            if (!unlocked) { SoftAudio.Cue(SoftCue.UI_Invalid); return; }
            if (destination == game.Save.node) { Interact(); return; }
            var path = game.Data.FindPath(game.Save.node,destination,game.Save);
            if (path == null) { SoftAudio.Cue(SoftCue.UI_Invalid); paths.Highlight(null);tooltipState.text = "Tuyến đường đang khóa"; return; }
            var gate=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
            if(gate!=null && gate.mandatory && !game.Events.Resolved(gate) && path.Count>1 &&
                path[1]!=game.Save.previousNode && game.Data.nodes[path[1]].revealStage>=game.Data.nodes[game.Save.node].revealStage)
            { Interact(); return; }
            StartCoroutine(Move(path));
        }

        private static string KindLabel(string kind)
        {
            switch(kind)
            {
                case "npc": return "Nhân vật";
                case "quest": return "Nhiệm vụ";
                case "exploration": return "Khám phá";
                case "bot": case "enemy": case "boss": return "Đối thủ";
                case "puzzle": return "Giải đố";
                case "item": return "Vật phẩm";
                case "portal": return "Cổng thành phố";
                case "secret": return "Bí mật";
                default: return "Điểm dừng";
            }
        }

        public string ActiveNavigationEdgeId {get;private set;}
        public NavigationJourney ActiveJourney {get;private set;}
        public int NavigationTransfers {get;private set;}
        int requestedRouteDestination=-1;
        private IEnumerator Move(List<int> path)
        {
            var destination=path[path.Count-1]; PendingDestination=destination;
            NpcApproachFailure=null;npcStandingMessage=null;
            Moving=true;NavigationTransfers=0;requestedRouteDestination=-1;Refresh();
            yield return ReturnFromNpc();
            var journey=game.Data.FindRoute(game.Save.node,destination,game.Save);ActiveJourney=journey;
            paths.HighlightJourney(journey);
            var legs=journey==null?new List<NavigationLeg>():new List<NavigationLeg>(journey.legs);
            var carriedTime=0f;
            for(var legIndex=0;legIndex<legs.Count;legIndex++)
            {
                var leg=legs[legIndex];
                // Junction IDs are transient navigation, never saved node indices.
                // Recheck permissions before every leg, including transfer legs.
                var graph=game.Data.navigation;
                if(graph!=null&&!graph.EdgeOpen(leg.edge,game.Save))break;
                var route=leg.Points;ActiveNavigationEdgeId=leg.edge.id;
                var mode=leg.edge.travel=="ferry"?"ferry":"walk";
                if(mode!=ActiveTravelMode){NavigationTransfers++;ActiveTravelMode=mode;ferryVisual.gameObject.SetActive(mode=="ferry");carriedTime=0;yield return new WaitForSeconds(.18f);}
                ferryVisual.gameObject.SetActive(ActiveTravelMode=="ferry");markerShadow.gameObject.SetActive(ActiveTravelMode!="ferry");
                var distances=CityRouteGeometry.Measure(route,map.rect.size);
                var duration=Mathf.Max(.025f,distances[distances.Length-1]/190f);
                var t=carriedTime/duration;
                for(;t<1;t+=Time.deltaTime/duration)
                {
                    var point=CityRouteGeometry.SampleMeasured(route,distances,t);
                    PositionMarker(point);
                    Marker.anchoredPosition=new Vector2(0,(ActiveTravelMode=="ferry" ? 5+Mathf.Sin(Time.time*2)*.7f : Mathf.Sin(Time.time*15)*1.2f)/View.Zoom);
                    View.EnsureVisible(point); yield return null;
                }
                carriedTime=Mathf.Max(0,(t-1)*duration);
                PositionMarker(route[route.Count-1]);
                if(requestedRouteDestination>=0&&game.Data.navigation!=null)
                {
                    var target=requestedRouteDestination;requestedRouteDestination=-1;
                    var entry=System.Array.Find(game.Data.navigation.nodes,n=>n.nodeId==game.Data.nodes[target].id);
                    var replacement=entry==null?null:game.Data.navigation.PlanVertices(game.Data,leg.To,entry.vertexId,game.Save);
                    if(replacement!=null)
                    {
                        destination=target;PendingDestination=target;
                        legs.RemoveRange(legIndex+1,legs.Count-legIndex-1);legs.AddRange(replacement.legs);
                        ActiveJourney=replacement;paths.HighlightJourney(replacement);
                    }
                }
                var next=graph?.CanonicalIndex(game.Data,leg.To)??System.Array.FindIndex(game.Data.nodes,n=>n.id==leg.To);
                if(next<0)continue;
                game.Save.previousNode=game.Save.node; game.Save.node=next;
                if(!game.Save.visited.Contains(next)) game.Save.visited.Add(next);
                var stop=game.Events.OnArrive(next,next==destination);
                // A rescue-directed trip opens its own task at the destination.
                // Keep legacy arrival grants and all mandatory story stops intact.
                if(stop&&next==catTravelTarget&&game.Events.Definition(game.Data.nodes[next].eventId)?.mandatory!=true)stop=false;
                game.Persist(); Refresh();
                if(stop)
                {
                    ActiveTravelMode="walk";ActiveNavigationEdgeId=null;ferryVisual.gameObject.SetActive(false);markerShadow.gameObject.SetActive(true);
                    Moving=false; if(next==destination) PendingDestination=-1;
                    Refresh(); Interact(); yield break;
                }
            }
            ActiveTravelMode="walk";ActiveNavigationEdgeId=null;ferryVisual.gameObject.SetActive(false);markerShadow.gameObject.SetActive(true);
            // A cancelled leg returns to the last persisted canonical arrival,
            // matching existing save semantics; never leave a swimmer/junction save.
            PositionMarker(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));
            Moving=false; PendingDestination=-1; paths.HighlightJourney(null);Refresh();
        }

        private void PositionGroundedActor(int index)
        {
            var actor=actors[index];
            if(actor==null)return;
            var definition=actorDefinitions[index];
            if(actor==null||definition==null||!definition.hasMapPosition)return;
            // Node controls counter-scale zoom. Convert map-space distance back
            // into that local space so feet stay on the painting at every zoom.
            var foot=Vector2.Scale(definition.mapPosition-CityRouteGeometry.Arrival(game.Data.nodes[index]),map.rect.size)*View.Zoom+definition.offset;
            actor.rectTransform.anchoredPosition=foot;
            nodes[index].transform.Find("ActorShadow").GetComponent<RectTransform>().anchoredPosition=foot;
            nodes[index].transform.Find("ActorHitArea").GetComponent<RectTransform>().anchoredPosition=foot+Vector2.up*presentation.ActorSize(game.Data.cityId,definition).y*.5f;
            nodeMarkers[index].SetActorFoot(foot);
        }
        private void PositionMarker(Vector2 position)
        {
            Marker.anchorMin = Marker.anchorMax = position;
            Marker.anchoredPosition = Vector2.zero;
            markerShadow.anchorMin=markerShadow.anchorMax=position;
            LumiOccluded=ActiveTravelMode!="ferry"&&CityRouteGeometry.IsCovered(game.Data,position);
            Marker.GetComponent<Image>().enabled=!LumiOccluded;
            markerShadow.GetComponent<Image>().enabled=!LumiOccluded;
        }

        public void EnterAlley() { if (!Moving && game.Data.cityId=="hoi_an"&&game.Save.node == 3) game.Scenes.Go("HoiAnDemo"); }

        public void Interact()
        {
            if(Moving || dialogue || Time.unscaledTime<resumeInputAt) return;
            StartCoroutine(ApproachNpcAndInteract());
        }
        public void ContinueJourney()
        {
            if(PendingDestination<0 || Moving || dialogue || Time.unscaledTime<resumeInputAt) return;
            var destination=PendingDestination;
            var current=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
            if(current!=null && current.mandatory && !game.Events.Resolved(current)) { Interact(); return; }
            TryMove(destination);
        }

        private void Refresh()
        {
            if(game==null||game.Data.cityId!=renderedCityId)return;
            RefreshCatRescue();
            RefreshLeisureDecorations();
            if(nextObjective!=null)nextObjective.text=npcStandingMessage??("Việc tiếp theo · "+game.Quests.Guidance());
            RefreshQuestLight();
            RefreshRelicLights();
            if(companion!=null)companion.gameObject.SetActive(game.World.Has("hoi_an.lost_lantern_rescued")&&!LumiOccluded);
            View.InputEnabled = !dialogue;
            foreach (var control in mapControls) control.interactable = !dialogue;
            paths.Refresh();
            ui.Say(game.Data.cityId=="hoi_an"?game.Quests.Status:game.Content.City(game.Data.cityId).name+" · Xem mục tiêu trong nhật ký nhiệm vụ.");
            chapterProgress.text="Khám phá "+game.Save.visited.Count+" / "+game.Data.nodes.Length+" · Chặng "+(game.Discovery.Stage(game.Save.cityId)+1)+" / 5";
            for (var i = 0; i < nodes.Count; i++)
            {
                var data = game.Data.nodes[i];
                var state=game.Discovery.State(i);
                var visible=state!=MapNodeState.HIDDEN;
                nodes[i].gameObject.SetActive(visible);
                if(visible && !shown[i] && rendered) StartCoroutine(RevealNode(nodes[i]));
                shown[i]=visible;
                // Locked locations can be inspected, but TryMove never travels to them.
                nodes[i].interactable = visible && !dialogue;
                var stateSprite=game.Discovery.DiscoveryState(i)==MapDiscoveryState.PREVIEW ? art.available : i==game.Save.node ? art.current : state==MapNodeState.VISIBLE_LOCKED ? art.locked : game.Save.visited.Contains(i) ? art.visited : art.available;
                var actorData=presentation.MapActor(game.Data.cityId,data.id);
                var actor=actors[i];
                var actorVisible=actor!=null && actorData!=null && visible && (!actorData.hideWhenResolved || !game.Events.Resolved(game.Events.Definition(data.eventId)));
                if(actor!=null)
                {
                    actor.gameObject.SetActive(actorVisible);
                    nodes[i].transform.Find("ActorShadow").gameObject.SetActive(actorVisible);
                    nodes[i].transform.Find("ActorHitArea").gameObject.SetActive(actorVisible);
                }
                nodeMarkers[i].Apply(markerResolver.Resolve(i),stateSprite,i==game.Save.node,actorData,actorVisible,!dialogue);
            }
            action.interactable = back.interactable = !Moving && !dialogue;
            var resume=ui.Root.Find("ContinueJourney");
            if(PendingDestination>=0 && !Moving && !dialogue)
            {
                if(resume==null)
                {
                    var button=MobileUI.Button(ui,ui.Root,"ContinueJourney","Tiếp tục hành trình",new Vector2(.05f,0),new Vector2(.55f,0),ContinueJourney);
                    var rect=(RectTransform)button.transform; rect.offsetMin=new Vector2(0,150);rect.offsetMax=new Vector2(0,198);
                }
                else resume.gameObject.SetActive(true);
            }
            else if(resume!=null) resume.gameObject.SetActive(false);
            action.GetComponentInChildren<Text>().text = "Tương tác · "+game.Data.nodes[game.Save.node].name;
            rendered=true;
        }
    }
}
