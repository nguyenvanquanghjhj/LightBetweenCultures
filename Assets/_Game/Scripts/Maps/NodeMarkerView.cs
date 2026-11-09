using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public enum MarkerMotionMode { Full, Reduced, Off }
    // Presentation and pointer forwarding only; gameplay stays in CityMap.TryMove.
    public sealed class NodeMarkerView : MonoBehaviour, IPointerClickHandler,IPointerDownHandler
    {
        public string NodeId { get; private set; }
        public NodeMarkerType Type { get; private set; }
        public NodeMarkerEffect Effect { get; private set; }
        public Image StateImage { get; private set; }
        public RectTransform Badge { get; private set; }
        public RectTransform HitArea { get; private set; }
        public Image Artwork { get; private set; }
        public Image LeisureBadge { get; private set; }
        public MapDiscoveryState Discovery { get; private set; }
        public Color BadgeColor=>frame.color;
        public Color GlyphColor=>glyph.color;
        public GameObject KeyGateAura { get; private set; }
        public bool ReadyToTurnIn { get; private set; }
        public const float FloatAmplitude=4,FloatPeriod=1.9f;
        public static string MotionPreferenceKey="LBC.MarkerMotion";
        private static string cachedKey;
        private static MarkerMotionMode motionMode;
        public static MarkerMotionMode MotionMode
        {
            get {if(cachedKey!=MotionPreferenceKey){cachedKey=MotionPreferenceKey;motionMode=(MarkerMotionMode)Mathf.Clamp(PlayerPrefs.GetInt(cachedKey,0),0,2);}return motionMode;}
        }
        public static void SetMotionMode(MarkerMotionMode mode)
        {cachedKey=MotionPreferenceKey;motionMode=mode;PlayerPrefs.SetInt(cachedKey,(int)mode);PlayerPrefs.Save();}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPreferences(){MotionPreferenceKey="LBC.MarkerMotion";cachedKey=null;}
        private CanvasGroup group,badgeGroup;
        private Graphic frame;
        private AdventureIconGraphic glyph;
        private Image reward;
        private Vector2 offset;
        private Color frameColor;
        private Outline outline;
        private GameObject readySpark;
        private bool wasVisible,animate,initialized;
        private float shownAt,phase;
        private float revealStartAlpha;
        private bool canSelect;
        private Action select;
        private bool followsActor,currentActor;
        private float actorHeight,actorClearance;
        private PointerTapIntent tapIntent;
        private float pressUntil;

        public void Configure(UIController ui,string id,Sprite initialState,int index,Action onSelect)
        {
            NodeId=id;phase=index*.37f;select=onSelect;
            var root=(RectTransform)transform;root.sizeDelta=new Vector2(52,52);
            group=gameObject.AddComponent<CanvasGroup>();
            StateImage=ui.Picture(root,"NodeState",initialState,new Vector2(.25f,.25f),new Vector2(.75f,.75f));
            HitArea=ui.Panel(root,"MarkerHitArea",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Color.clear);
            HitArea.sizeDelta=Vector2.one*52;
            Badge=new GameObject("SemanticMarker",typeof(RectTransform),typeof(CanvasGroup)).GetComponent<RectTransform>();
            Badge.SetParent(root,false);Badge.anchorMin=Badge.anchorMax=new Vector2(.5f,.5f);
            badgeGroup=Badge.GetComponent<CanvasGroup>();
            frame=Badge.gameObject.AddComponent<RoundedPanelGraphic>();frame.raycastTarget=false;
            var shadow=Badge.gameObject.AddComponent<Shadow>();shadow.effectColor=new Color(.04f,.10f,.12f,.32f);shadow.effectDistance=new Vector2(1.4f,-2.2f);
            outline=Badge.gameObject.AddComponent<Outline>();outline.effectDistance=new Vector2(.8f,-.8f);
            var aura=MobileUI.Symbol(Badge,"KeyGateAura",AdventureSymbol.Aura,new Vector2(-.16f,-.16f),new Vector2(1.16f,1.16f),new Color(1,.76f,.28f,.5f));
            KeyGateAura=aura.gameObject;
            glyph=MobileUI.Symbol(Badge,"MarkerGlyph",AdventureSymbol.Chat,new Vector2(.13f,.13f),new Vector2(.87f,.87f));
            reward=ui.Picture(Badge,"RewardIcon",AdventureUIArt.Load().backpack,new Vector2(.10f,.10f),new Vector2(.90f,.90f));
            readySpark=MobileUI.Symbol(Badge,"ReadySpark",AdventureSymbol.Plus,new Vector2(.80f,.79f),new Vector2(1.08f,1.07f),DesignKitArt.Gold).gameObject;
            Artwork=ui.Picture(Badge,"MarkerArtwork",null,Vector2.zero,Vector2.one);
            KeyGateAura.transform.SetAsFirstSibling();readySpark.transform.SetAsLastSibling();
        }
        public void OnPointerDown(PointerEventData pointer){if(tapIntent.Begin(pointer)&&wasVisible&&canSelect)PressFeedback();}
        public void OnPointerClick(PointerEventData pointer)
        {if(wasVisible&&canSelect&&tapIntent.Accept(pointer,true))select?.Invoke();}
        public void PressFeedback(){pressUntil=Time.unscaledTime+.16f;}
        public void AddLeisureBadge(UIController ui)
        {
            if(LeisureBadge!=null)return;
            // Reuse the existing star badge. It supplements, never replaces, the
            // quest/NPC/check-in marker and inherits the map's inverse zoom scale.
            LeisureBadge=ui.Picture(transform,"LeisureActivityBadge",DesignKitArt.Load().secret,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            LeisureBadge.rectTransform.sizeDelta=Vector2.one*28;
            LeisureBadge.preserveAspect=true;LeisureBadge.raycastTarget=false;
            LeisureBadge.gameObject.SetActive(wasVisible&&Discovery==MapDiscoveryState.REVEALED);
        }
        public void Apply(NodeMarkerDisplay display,Sprite state,bool current,MapActorDefinition actor,bool actorVisible,bool animateEffects)
        {
            if(display.visible&&display.discovery==MapDiscoveryState.REVEALED&&(!wasVisible||Discovery!=MapDiscoveryState.REVEALED))
            {shownAt=initialized?Time.unscaledTime:Time.unscaledTime-.55f;revealStartAlpha=Discovery==MapDiscoveryState.PREVIEW ? .22f : 0;}
            initialized=true;
            Discovery=display.discovery;
            wasVisible=display.visible;gameObject.SetActive(display.visible);
            if(!display.visible)return;
            Type=display.type;Effect=display.effect;animate=animateEffects;
            var catalog=AdventureUIArt.Load();
            StateImage.sprite=catalog.Marker(current&&Discovery==MapDiscoveryState.REVEALED?"Current":"Ground")??state;
            Artwork.sprite=Type==NodeMarkerType.FragmentAlert?null:catalog.Marker(Type.ToString());var illustrated=Artwork.sprite!=null;
            Artwork.gameObject.SetActive(illustrated);frame.enabled=!illustrated;outline.enabled=!illustrated;
            Badge.GetComponent<Shadow>().enabled=!illustrated;
            canSelect=Discovery==MapDiscoveryState.REVEALED;
            if(LeisureBadge!=null)LeisureBadge.gameObject.SetActive(canSelect);
            group.blocksRaycasts=group.interactable=canSelect;
            Badge.gameObject.SetActive(canSelect&&Type!=NodeMarkerType.Normal);
            HitArea.gameObject.SetActive(canSelect&&Type!=NodeMarkerType.Normal);
            Badge.sizeDelta=Vector2.one*display.size*(illustrated?1.4f:1);
            followsActor=actorVisible&&!display.customOffset;currentActor=current;
            actorHeight=actorVisible?catalog.ActorSize(actor.cityId,actor).y:0;
            actorClearance=actorVisible&&actor.markerClearance>0?actor.markerClearance:14;
            offset=display.customOffset?display.offset:actorVisible?new Vector2(actor.offset.x,actor.offset.y+actorHeight+actorClearance):new Vector2(0,34);
            if(current)offset.y=Mathf.Max(offset.y,64);
            HitArea.anchoredPosition=offset; // Never follows the floating visual.
            var white=Type==NodeMarkerType.Camera||Type==NodeMarkerType.NpcTalk||Type==NodeMarkerType.NpcNeedItem||Type==NodeMarkerType.BossAlert;
            frameColor=Type==NodeMarkerType.FragmentAlert?DesignKitArt.Hex("BE3831"):white?DesignKitArt.Hex("FFF7E8"):Type==NodeMarkerType.Quest?DesignKitArt.Hex("916626"):
                Type==NodeMarkerType.Completed?DesignKitArt.Hex("397C67"):DesignKitArt.Hex("26555C");
            glyph.symbol=Symbol(Type);glyph.color=Type==NodeMarkerType.BossAlert?DesignKitArt.Hex("C43E36"):white?DesignKitArt.Teal:DesignKitArt.Cream;glyph.SetVerticesDirty();
            ReadyToTurnIn=display.readyToTurnIn&&Type==NodeMarkerType.NpcTalk;
            readySpark.SetActive(canSelect&&ReadyToTurnIn);
            KeyGateAura.SetActive(canSelect&&Type==NodeMarkerType.BossAlert&&display.keyGate);
            outline.effectColor=Type==NodeMarkerType.BossAlert?DesignKitArt.Hex("B44639"):ReadyToTurnIn?DesignKitArt.Gold:new Color(.18f,.35f,.34f,.8f);
            reward.gameObject.SetActive(!illustrated&&Type==NodeMarkerType.Reward);glyph.gameObject.SetActive(!illustrated&&Type!=NodeMarkerType.Reward);
            Tick();
        }
        public void SetZoom(float zoom)=>transform.localScale=Vector3.one/Mathf.Max(.1f,zoom);
        public void SetActorFoot(Vector2 foot)
        {
            if(!followsActor)return;
            offset=foot+Vector2.up*(actorHeight+actorClearance);
            if(currentActor)offset.y=Mathf.Max(offset.y,foot.y+64);
            HitArea.anchoredPosition=offset;Tick();
        }
        private void Update()=>Tick();
        private void Tick()
        {
            if(!wasVisible)return;
            StateImage.transform.localScale=Vector3.one*(1+.28f*Mathf.Clamp01((pressUntil-Time.unscaledTime)/.16f));
            if(Discovery==MapDiscoveryState.PREVIEW){group.alpha=.22f;return;}
            group.alpha=Mathf.Lerp(revealStartAlpha,1,Mathf.SmoothStep(0,1,Mathf.Clamp01((Time.unscaledTime-shownAt)/.55f)));
            var strength=!animate||MotionMode==MarkerMotionMode.Off ? 0 : MotionMode==MarkerMotionMode.Reduced ? .35f : 1;
            var sine=Mathf.Sin(Time.unscaledTime*Mathf.PI*2/FloatPeriod+phase);var wave=(sine+1)*.5f;
            badgeGroup.alpha=Effect==NodeMarkerEffect.Blink ? 1-.06f*strength*wave : 1;
            Badge.localScale=Vector3.one*(Effect==NodeMarkerEffect.Pulse?1+.025f*strength*wave:1);
            var floats=Type!=NodeMarkerType.Normal&&Type!=NodeMarkerType.Completed;
            Badge.anchoredPosition=offset+Vector2.up*(floats?FloatAmplitude*strength*sine:0);
            if(LeisureBadge!=null)LeisureBadge.rectTransform.anchoredPosition=offset+(Type==NodeMarkerType.Normal?Vector2.zero:new Vector2(27,-2));
            frame.color=Effect==NodeMarkerEffect.Glow?Color.Lerp(frameColor,DesignKitArt.Gold,wave*.05f*strength):frameColor;
        }
        private static AdventureSymbol Symbol(NodeMarkerType type)
        {
            switch(type)
            {
                case NodeMarkerType.Camera:return AdventureSymbol.Camera;
                case NodeMarkerType.BossAlert:return AdventureSymbol.BossAlert;
                case NodeMarkerType.FragmentAlert:return AdventureSymbol.BossAlert;
                case NodeMarkerType.NpcTalk:return AdventureSymbol.TalkDots;
                case NodeMarkerType.NpcNeedItem:return AdventureSymbol.NeedItem;
                case NodeMarkerType.Puzzle:return AdventureSymbol.Puzzle;
                case NodeMarkerType.Completed:return AdventureSymbol.Check;
                case NodeMarkerType.Quest:return AdventureSymbol.Journal;
                case NodeMarkerType.Portal:return AdventureSymbol.Portal;
                default:return AdventureSymbol.Check;
            }
        }
    }
}
