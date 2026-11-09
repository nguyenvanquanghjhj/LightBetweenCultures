using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Shared policy on the existing EventSystem/Canvas, not another input manager.
    public static class TouchInputPolicy
    {
        public static uint GestureEpoch {get;private set;}
        static bool multiple;
        public static int ActiveTouches {get;private set;}
        public static int DragPixels(float dpi,int width,int height)
        {
            var shortSide=Mathf.Max(1,Mathf.Min(width,height));
            var physical=dpi>=100&&dpi<=1000?dpi*1.5f/25.4f:shortSide*.018f;
            return Mathf.RoundToInt(Mathf.Clamp(physical,shortSide*.012f,shortSide*.045f));
        }
        public static void Configure(EventSystem events)
        {if(events!=null)events.pixelDragThreshold=DragPixels(Screen.dpi,Screen.width,Screen.height);}
        public static void Sample()
        {
            var count=0;var screen=Touchscreen.current;
            if(screen!=null)foreach(var t in screen.touches)if(t.press.isPressed)count++;
            if(count>1&&!multiple)GestureEpoch++;
            multiple=count>1;ActiveTouches=count;
        }
        public static void Cancel(){GestureEpoch++;multiple=false;ActiveTouches=0;}
        public static bool Canceled(PointerEventData e)
        {
            var data=e as ExtendedPointerEventData;
            if(data==null||!(data.device is Touchscreen screen))return false;
            foreach(var t in screen.touches)
                if(t.touchId.ReadValue()==data.touchId)return t.phase.ReadValue()==UnityEngine.InputSystem.TouchPhase.Canceled;
            return false;
        }
    }
    public struct PointerTapIntent
    {
        int owner;uint epoch;float began;Vector2 start;bool valid,isTouch;
        public bool Begin(PointerEventData e)
        {
            TouchInputPolicy.Sample();
            valid=e.button==PointerEventData.InputButton.Left&&TouchInputPolicy.ActiveTouches<2;
            owner=e.pointerId;epoch=TouchInputPolicy.GestureEpoch;began=Time.unscaledTime;start=e.position;
            isTouch=e is ExtendedPointerEventData data&&data.device is Touchscreen;return valid;
        }
        public bool Accept(PointerEventData e,bool shortTap=false)
        {
            TouchInputPolicy.Sample();var threshold=EventSystem.current!=null?EventSystem.current.pixelDragThreshold:TouchInputPolicy.DragPixels(Screen.dpi,Screen.width,Screen.height);
            var ok=valid&&e.pointerId==owner&&e.button==PointerEventData.InputButton.Left&&!TouchInputPolicy.Canceled(e)&&
                (!isTouch||(epoch==TouchInputPolicy.GestureEpoch&&TouchInputPolicy.ActiveTouches<2))&&
                (e.position-start).sqrMagnitude<=threshold*threshold&&(!shortTap||Time.unscaledTime-began<.65f);
            valid=false;return ok;
        }
    }
    // Same Button callbacks; a canceled gesture cannot turn into an accepted click.
    public sealed class TouchButton:Button
    {
        public bool shortTap;
        PointerTapIntent intent;
        public override void OnPointerDown(PointerEventData e){if(intent.Begin(e))base.OnPointerDown(e);}
        public override void OnPointerClick(PointerEventData e){if(intent.Accept(e,shortTap))base.OnPointerClick(e);}
        protected override void OnDisable(){intent=default;base.OnDisable();}
    }
    public sealed class TouchSlider:Slider
    {
        int owner;uint epoch;bool held;
        public override void OnPointerDown(PointerEventData e)
        {TouchInputPolicy.Sample();if(held||TouchInputPolicy.ActiveTouches>1||e.button!=PointerEventData.InputButton.Left)return;owner=e.pointerId;epoch=TouchInputPolicy.GestureEpoch;held=true;base.OnPointerDown(e);}
        public override void OnDrag(PointerEventData e)
        {TouchInputPolicy.Sample();if(held&&e.pointerId==owner&&epoch==TouchInputPolicy.GestureEpoch&&!TouchInputPolicy.Canceled(e))base.OnDrag(e);}
        public override void OnPointerUp(PointerEventData e){if(e.pointerId!=owner)return;held=false;base.OnPointerUp(e);}
        protected override void OnDisable(){held=false;base.OnDisable();}
    }
}
