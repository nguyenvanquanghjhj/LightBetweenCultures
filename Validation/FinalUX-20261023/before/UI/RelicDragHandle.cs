using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    // A control on the existing HUD; works through the existing mouse/touch UI module.
    public sealed class RelicDragHandle : MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler
    {
        public RectTransform target;
        public Func<Vector2,bool> submit;
        public Action<bool> feedback;
        // Opt-in for the reviewed Hue challenge only. Other drag activities keep
        // their existing normalized tolerance, callbacks and gesture behavior.
        public bool forgivingSnap;
        public Func<bool> placed;
        public bool Locked {get;private set;}
        Vector3 home,grabOffset;int pointer;uint epoch;bool dragging;
        public void LockToTarget()
        {
            if(!forgivingSnap||target==null)return;
            dragging=false;Locked=true;
            var rt=(RectTransform)transform;rt.SetParent(target,true);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=Vector2.zero;
            GetComponent<Image>().color=new Color(1,.93f,.65f);
        }
        bool Owns(PointerEventData e)=>dragging&&pointer==e.pointerId&&epoch==TouchInputPolicy.GestureEpoch&&!TouchInputPolicy.Canceled(e);
        public void OnBeginDrag(PointerEventData e)
        {
            TouchInputPolicy.Sample();
            if(dragging||e.button!=PointerEventData.InputButton.Left||TouchInputPolicy.ActiveTouches>1||forgivingSnap&&(Locked||placed?.Invoke()==true))return;
            home=transform.position;pointer=e.pointerId;epoch=TouchInputPolicy.GestureEpoch;dragging=true;grabOffset=Vector3.zero;
            if(forgivingSnap&&RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform.parent,e.pressPosition,e.pressEventCamera,out var p))grabOffset=home-p;
        }
        public void OnDrag(PointerEventData e)
        {
            TouchInputPolicy.Sample();
            if(Owns(e)&&RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform.parent,e.position,e.pressEventCamera,out var p))
            {
                transform.position=p+(forgivingSnap?grabOffset:Vector3.zero);
                if(forgivingSnap&&target!=null)
                {
                    var area=target.rect;area.xMin-=18;area.xMax+=18;area.yMin-=18;area.yMax+=18;
                    if(area.Contains((Vector2)target.InverseTransformPoint(transform.position))&&submit?.Invoke(Vector2.zero)==true)
                    {
                        // Attach to the target so canvas/safe-area resize cannot
                        // pull an already accepted piece back toward its tray anchor.
                        LockToTarget();
                        SoftAudio.Cue(SoftCue.Relic_Place);feedback?.Invoke(true);
                    }
                }
            }
        }
        public void OnEndDrag(PointerEventData e)
        {
            TouchInputPolicy.Sample();if(!dragging||pointer!=e.pointerId)return;
            if(!Owns(e)||target==null){dragging=false;transform.position=home;return;}
            OnDrag(e);
            if(forgivingSnap)
            {
                if(!dragging)return; // Already snapped during drag; never submit twice on release.
                dragging=false;transform.position=home;SoftAudio.Cue(SoftCue.UI_Invalid);feedback?.Invoke(false);return;
            }
            dragging=false;
            var parent=(RectTransform)transform.parent;
            var error=(Vector2)parent.InverseTransformPoint(transform.position)-(Vector2)parent.InverseTransformPoint(target.position);
            error=new Vector2(error.x/parent.rect.width,error.y/parent.rect.height);
            var accepted=submit!=null&&submit(error);
            SoftAudio.Cue(accepted?SoftCue.Relic_Place:SoftCue.UI_Invalid);
            transform.position=accepted?target.position:home;
            GetComponent<Image>().color=accepted?new Color(1,.93f,.65f):Color.white;
            feedback?.Invoke(accepted);
        }
        void OnDisable(){if(dragging)transform.position=home;dragging=false;}
    }
}
