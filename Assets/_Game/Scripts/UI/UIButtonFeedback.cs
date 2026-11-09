using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Shared presentation only. Animate the background, keeping the touch rect
    // and click callback stable; never invoke gameplay from a tween.
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        Button button;
        bool pressed;
        float scale=1,sink;
        bool? lastEnabled;
        public float VisualScale=>scale;
        public float VisualSink=>sink;
        void Awake()
        {
            button=GetComponent<Button>();var colors=button.colors;
            colors.highlightedColor=new Color(1,.98f,.92f,1);colors.pressedColor=new Color(.83f,.88f,.84f,1);
            colors.disabledColor=new Color(.88f,.90f,.87f,1);colors.fadeDuration=.09f;button.colors=colors;
        }
        void OnDisable(){pressed=false;scale=1;sink=0;lastEnabled=null;if(button!=null&&button.targetGraphic!=null){button.targetGraphic.transform.localScale=Vector3.one;((RectTransform)button.targetGraphic.transform).anchoredPosition=Vector2.zero;}}
        public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&button.IsInteractable())pressed=true;}
        public void OnPointerUp(PointerEventData e)=>pressed=false;
        public void OnPointerEnter(PointerEventData e){}
        public void OnPointerExit(PointerEventData e)=>pressed=false;
        public void OnSelect(BaseEventData e){}
        public void OnDeselect(BaseEventData e)=>pressed=false;
        void Update()
        {
            if(button.targetGraphic==null)return;
            var mode=NodeMarkerView.MotionMode;
            var enabled=button.IsInteractable();
            if(lastEnabled!=enabled){lastEnabled=enabled;if(button.targetGraphic is SoftButtonGraphic soft)soft.SetDisabled(!enabled);}
            var target=enabled&&pressed?(mode==MarkerMotionMode.Reduced?.985f:.96f):1;
            scale=mode==MarkerMotionMode.Off?1:Mathf.Lerp(scale,target,1-Mathf.Exp(-Time.unscaledDeltaTime*30));
            var depth=enabled&&pressed&&mode!=MarkerMotionMode.Off?(mode==MarkerMotionMode.Reduced?1.2f:2.6f):0;
            sink=mode==MarkerMotionMode.Off?0:Mathf.Lerp(sink,depth,1-Mathf.Exp(-Time.unscaledDeltaTime*30));
            button.targetGraphic.transform.localScale=Vector3.one*scale;
            ((RectTransform)button.targetGraphic.transform).anchoredPosition=new Vector2(0,-sink);
        }
    }
}
