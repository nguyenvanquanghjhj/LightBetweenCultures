using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed class UIController : MonoBehaviour
    {
        public RectTransform Root { get; private set; }
        public Text Message { get; private set; }
        private RectTransform safe;
        private Rect lastSafe;
        private Vector2 lastSize;
        private static Font font;
        public static Font Font => font != null ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public static UIController Create(string title, bool backdrop = false)
        {
            var go = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(600, 1000);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var ui = go.AddComponent<UIController>();
            if (backdrop)
            {
                var background = ui.Panel(go.transform, "Backdrop", Vector2.zero, Vector2.one, DesignKitArt.Ink);
                background.GetComponent<Image>().raycastTarget = false;
            }
            ui.safe = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            ui.safe.SetParent(go.transform, false); ui.Root = ui.safe; ui.ApplySafeArea();
            ui.Label(ui.Root, "Title", title, new Vector2(.04f,.92f), new Vector2(.96f,.99f), 28).color = DesignKitArt.Cream;
            var panel = ui.Panel(ui.Root, "MessagePanel", new Vector2(.03f,.79f), new Vector2(.97f,.91f), DesignKitArt.Cream);
            Border(panel, DesignKitArt.Gold);
            ui.Message = ui.Label(panel, "Message", "", new Vector2(.03f,.04f), new Vector2(.97f,.96f), 22);
            ui.Message.color = DesignKitArt.Ink;
            if (Object.FindAnyObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            var input = Object.FindAnyObjectByType<InputSystemUIInputModule>();
            // Hybrid mouse/touch sessions keep each press/release pair on its
            // own pointer instead of deleting the mouse pointer on touch input.
            if(input != null) input.pointerBehavior = UIPointerBehavior.AllPointersAsIs;
            TouchInputPolicy.Configure(EventSystem.current);
            SoftAudio.Ensure().AttachSettings(ui);
            return ui;
        }
        private void Update() { TouchInputPolicy.Sample(); if (lastSafe != Screen.safeArea || lastSize != new Vector2(Screen.width, Screen.height)) {ApplySafeArea();TouchInputPolicy.Configure(EventSystem.current);} }
        private void OnApplicationFocus(bool focused){if(!focused)TouchInputPolicy.Cancel();}
        private void ApplySafeArea()
        {
            lastSafe = Screen.safeArea; lastSize = new Vector2(Screen.width, Screen.height);
            SetRect(safe, lastSafe.min / lastSize, lastSafe.max / lastSize);
        }
        public void Say(string value) { Message.text = PlayerCopy.Display(value); }
        public Image Picture(Transform parent, string name, Sprite sprite, Vector2 min, Vector2 max)
        {
            var rect = Panel(parent, name, min, max, Color.white);
            var picture = rect.GetComponent<Image>(); picture.sprite = sprite;
            picture.preserveAspect = true; picture.raycastTarget = false;
            return picture;
        }
        public static void Border(RectTransform rect, Color color)
        {
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = color; outline.effectDistance = new Vector2(2,-2);
        }
        public RectTransform Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>(); SetRect(rect,min,max); go.GetComponent<Image>().color = color; return rect;
        }
        public Text Label(Transform parent, string name, string value, Vector2 min, Vector2 max, int size = 22, bool fit = true)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text)); go.transform.SetParent(parent,false);
            var text = go.GetComponent<Text>(); SetRect(text.rectTransform,min,max);
            text.font = Font; text.text = PlayerCopy.Display(value); text.fontSize = size; text.color = DesignKitArt.Cream;
            text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false;
            text.resizeTextForBestFit = fit; text.resizeTextMinSize = 10; text.resizeTextMaxSize = size;
            return text;
        }
        public Button Button(Transform parent, string name, string value, Vector2 min, Vector2 max, UnityAction action)
        {
            var rect = Panel(parent,name,min,max,DesignKitArt.Gold);
            Border(rect, DesignKitArt.Cream);
            var button = rect.gameObject.AddComponent<TouchButton>();button.shortTap=SoftAudio.IsNodeName(name); button.targetGraphic = rect.GetComponent<Image>();
            button.onClick.AddListener(()=>SoftAudio.Action(()=>action?.Invoke(),SoftAudio.IsNodeName(name)?(SoftCue?)null:SoftAudio.ButtonCue(name))); Label(rect,"Label",value, new Vector2(.03f,.04f), new Vector2(.97f,.96f)).color=DesignKitArt.Ink;
            if(!SoftAudio.IsNodeName(name))MobileUI.Style(button,DesignKitArt.Hex("EBC680"));
            return button;
        }
        public static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
        { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    }
}
