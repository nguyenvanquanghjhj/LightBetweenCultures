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
        bool pressed, hovered;
        float scale=1;
        public float VisualScale=>scale;
        void Awake()=>button=GetComponent<Button>();
        void OnDisable(){pressed=hovered=false;scale=1;if(button!=null&&button.targetGraphic!=null)button.targetGraphic.transform.localScale=Vector3.one;}
        public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&button.IsInteractable())pressed=true;}
        public void OnPointerUp(PointerEventData e)=>pressed=false;
        public void OnPointerEnter(PointerEventData e)=>hovered=true;
        public void OnPointerExit(PointerEventData e){hovered=pressed=false;}
        public void OnSelect(BaseEventData e)=>hovered=true;
        public void OnDeselect(BaseEventData e){hovered=pressed=false;}
        void Update()
        {
            if(button.targetGraphic==null)return;
            var mode=NodeMarkerView.MotionMode;
            var enabled=button.IsInteractable();
            var target=enabled&&pressed?(mode==MarkerMotionMode.Reduced?.985f:.96f):1;
            scale=mode==MarkerMotionMode.Off?1:Mathf.Lerp(scale,target,1-Mathf.Exp(-Time.unscaledDeltaTime*30));
            button.targetGraphic.transform.localScale=Vector3.one*scale;
            var colors=button.colors;
            colors.highlightedColor=hovered?new Color(1,.96f,.85f):Color.white;
            colors.pressedColor=new Color(.85f,.88f,.86f);
            colors.disabledColor=new Color(.55f,.59f,.59f,.65f);
            button.colors=colors;
        }
    }
}
