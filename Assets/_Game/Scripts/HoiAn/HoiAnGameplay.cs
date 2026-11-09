using UnityEngine;
namespace LightBetweenCultures.HoiAn
{
    public sealed class HoiAnGameplay : MonoBehaviour
    {
        [SerializeField] private HoiAnPlayerController player;
        [SerializeField] private HoiAnItem lanternPart;
        [SerializeField] private Sprite titleFrame, messageFrame, buttonFrame;
        private Transform pickupVisual;
        private Vector3 pickupRest;
        private CanvasGroup hint;
        private float started;
        private UnityEngine.UI.Text hudMessage;
        private string lastMessage;
        private UIController hud;
        private bool wide,layoutReady;
        private bool hadEye;
        private float eyeReactionAt=-100;
        private SpriteRenderer eyeReaction;
        private UnityEngine.UI.Button collectButton;
        private InteractionSystem interaction;
        public void Configure(HoiAnPlayerController p, HoiAnItem item) { player=p; lanternPart=item; }
        public void ConfigurePresentation(Sprite title,Sprite message,Sprite button)
        { titleFrame=title;messageFrame=message;buttonFrame=button; }
        private static void Skin(UnityEngine.UI.Image image,Sprite frame)
        {
            if(frame==null)return;
            image.sprite=frame;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Color.white;
            image.pixelsPerUnitMultiplier=2;image.raycastTarget=false;
            var outline=image.GetComponent<UnityEngine.UI.Outline>();if(outline!=null)outline.enabled=false;
        }
        private void Start()
        {
            var game=GameBootstrap.Ensure(); game.Save.node=3; game.Persist();
            var ui=UIController.Create("HẺM MÁI NGÓI · HỘI AN");
            hud=ui;
            started=Time.unscaledTime;
            var title=ui.Root.Find("Title").GetComponent<UnityEngine.UI.Text>();
            UIController.SetRect(title.rectTransform,new Vector2(.16f,.92f),new Vector2(.84f,.975f));title.fontSize=23;
            var titlePanel=ui.Panel(ui.Root,"H3TitleFrame",new Vector2(.05f,.915f),new Vector2(.95f,.985f),Color.white);
            Skin(titlePanel.GetComponent<UnityEngine.UI.Image>(),titleFrame);titlePanel.SetAsFirstSibling();
            var message=(RectTransform)ui.Root.Find("MessagePanel");
            UIController.SetRect(message,new Vector2(.05f,.81f),new Vector2(.95f,.905f));Skin(message.GetComponent<UnityEngine.UI.Image>(),messageFrame);
            message.GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=4;
            UIController.SetRect(ui.Message.rectTransform,new Vector2(.08f,.18f),new Vector2(.91f,.82f));
            ui.Message.fontSize=18;ui.Message.resizeTextMaxSize=18;ui.Message.color=DesignKitArt.Ink;
            ui.Say("Chạm vào Mắt Đèn để Lumi đi đến gần, rồi bấm Nhặt.\nBạn vẫn có thể chạm mặt đường hoặc dùng WASD để tự đi.");
            hint=message.gameObject.AddComponent<CanvasGroup>();hint.blocksRaycasts=false;
            hudMessage=ui.Message;lastMessage=hudMessage.text;
            lanternPart.Restore();
            interaction=player.GetComponent<InteractionSystem>()??player.gameObject.AddComponent<InteractionSystem>(); interaction.Configure(player,lanternPart,ui);
            var collect=ui.Button(ui.Root,"Collect","Nhặt Mắt Đèn",new Vector2(.31f,.035f),new Vector2(.96f,.115f),()=>{hint.alpha=1;started=Time.unscaledTime;interaction.Interact();});
            Skin(collect.GetComponent<UnityEngine.UI.Image>(),buttonFrame);collect.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            collectButton=collect;collectButton.gameObject.SetActive(interaction.CanInteract);
            var back=ui.Button(ui.Root,"Return","‹  Bản đồ",new Vector2(.04f,.035f),new Vector2(.29f,.115f),()=> { player.Stop(); game.Scenes.Go("CityMap"); });
            Skin(back.GetComponent<UnityEngine.UI.Image>(),titleFrame);back.GetComponent<UnityEngine.UI.Image>().raycastTarget=true;
            back.GetComponentInChildren<UnityEngine.UI.Text>().color=DesignKitArt.Cream;
            var backLabel=back.GetComponentInChildren<UnityEngine.UI.Text>();
            UIController.SetRect(backLabel.rectTransform,new Vector2(.18f,.14f),new Vector2(.86f,.86f));backLabel.resizeTextMaxSize=18;
            pickupVisual=lanternPart.transform.Find("H3PickupVisual");if(pickupVisual!=null)pickupRest=pickupVisual.localPosition;
            if(pickupVisual!=null)
            {
                var glow=new GameObject("H3PickupGlow",typeof(SpriteRenderer));glow.transform.SetParent(lanternPart.transform,false);
                glow.transform.localPosition=new Vector3(0,-.23f,0);glow.transform.localScale=new Vector3(1.15f/lanternPart.transform.lossyScale.x,.4f/lanternPart.transform.lossyScale.y,1);
                var sr=glow.GetComponent<SpriteRenderer>();sr.sprite=PresentationShapes.Shadow;sr.color=new Color(1,.7f,.18f,.32f);sr.sortingOrder=1;
            }
            UpdateLayout();
            hadEye=game.World.Has("hoi_an.lamp_eye_found");
            if(hadEye)ui.Say("Bạn đã tìm thấy Mắt Đèn. Hãy trở về tiệm Cô Mai để tiếp tục.");
            var pulse=new GameObject("H3EyePickupReaction",typeof(SpriteRenderer));pulse.transform.SetParent(player.transform,false);
            pulse.transform.localPosition=new Vector3(0,-.24f,0);pulse.transform.localScale=new Vector3(1.6f/player.transform.lossyScale.x,.65f/player.transform.lossyScale.y,1);
            eyeReaction=pulse.GetComponent<SpriteRenderer>();eyeReaction.sprite=PresentationShapes.WarmGlow;eyeReaction.sortingOrder=-2;eyeReaction.color=Color.clear;
        }
        private void UpdateLayout()
        {
            if(hud==null)return;var landscape=Screen.width>Screen.height;
            if(layoutReady&&landscape==wide)return;wide=landscape;layoutReady=true;
            hud.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution=wide?new Vector2(1000,600):new Vector2(600,1000);
            UIController.SetRect((RectTransform)hud.Root.Find("H3TitleFrame"),new Vector2(wide?.25f:.05f,.915f),new Vector2(wide?.75f:.95f,.985f));
            hud.Root.Find("H3TitleFrame").GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=wide?4:2;
            hud.Root.Find("Title").GetComponent<UnityEngine.UI.Text>().resizeTextMaxSize=wide?20:23;
            UIController.SetRect((RectTransform)hud.Root.Find("MessagePanel"),new Vector2(wide?.20f:.05f,.81f),new Vector2(wide?.80f:.95f,.905f));
            UIController.SetRect((RectTransform)hud.Root.Find("Collect"),new Vector2(wide?.60f:.31f,.035f),new Vector2(.96f,.115f));
            UIController.SetRect((RectTransform)hud.Root.Find("Return"),new Vector2(.04f,.035f),new Vector2(wide?.22f:.29f,.115f));
            hud.Root.Find("Collect").GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=wide?4:2;
            hud.Root.Find("Return").GetComponent<UnityEngine.UI.Image>().pixelsPerUnitMultiplier=wide?4:2;
        }
        private void Update()
        {
            UpdateLayout();
            if(collectButton!=null&&interaction!=null)collectButton.gameObject.SetActive(interaction.CanInteract);
            var found=GameBootstrap.Instance.World.Has("hoi_an.lamp_eye_found");
            if(found&&!hadEye){eyeReactionAt=Time.unscaledTime;player.GetComponent<LumiStepMotion>()?.React();}
            hadEye=found;
            if(eyeReaction!=null)
            {var t=(Time.unscaledTime-eyeReactionAt)/1.2f;eyeReaction.color=new Color(1,.76f,.28f,t>=0&&t<1?.5f*(1-t):0);}
            if(hudMessage!=null&&hudMessage.text!=lastMessage)
            {lastMessage=hudMessage.text;started=Time.unscaledTime;if(hint!=null)hint.alpha=1;}
            if(hint!=null)
            {
                var elapsed=Time.unscaledTime-started;hint.alpha=elapsed<7?1:Mathf.Lerp(hint.alpha,0,Time.unscaledDeltaTime*3);
            }
            if(pickupVisual!=null&&lanternPart.gameObject.activeInHierarchy)
                pickupVisual.localPosition=pickupRest+Vector3.up*(Mathf.Sin(Time.time*2.5f)*.05f);
        }
    }
}
