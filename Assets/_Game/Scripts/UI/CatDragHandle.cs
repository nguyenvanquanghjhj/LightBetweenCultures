using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LightBetweenCultures
{
    public sealed class CatDragHandle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform[] targets;
        public Func<int,bool> drop;
        public Action<bool> feedback;
        public bool placed;
        bool dragging;
        int pointer;
        uint epoch;
        Vector3 home, offset;
        public void OnBeginDrag(PointerEventData e)
        {
            TouchInputPolicy.Sample();
            if (placed || dragging || e.button != PointerEventData.InputButton.Left || TouchInputPolicy.ActiveTouches > 1) return;
            home=transform.position;pointer=e.pointerId;epoch=TouchInputPolicy.GestureEpoch;dragging=true;
            offset=Vector3.zero;
            if(RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform.parent,e.pressPosition,e.pressEventCamera,out var p))offset=home-p;
            transform.SetAsLastSibling();
        }
        bool Valid(PointerEventData e) => dragging && pointer==e.pointerId && epoch==TouchInputPolicy.GestureEpoch && !TouchInputPolicy.Canceled(e);
        public void OnDrag(PointerEventData e)
        {
            TouchInputPolicy.Sample();
            if(Valid(e)&&RectTransformUtility.ScreenPointToWorldPointInRectangle((RectTransform)transform.parent,e.position,e.pressEventCamera,out var p))transform.position=p+offset;
        }
        public void OnEndDrag(PointerEventData e)
        {
            TouchInputPolicy.Sample();
            if(!dragging||pointer!=e.pointerId)return;
            var valid=Valid(e);OnDrag(e);dragging=false;
            var chosen=-1;var distance=float.PositiveInfinity;
            if(valid && targets!=null)for(var i=0;i<targets.Length;i++)
            {
                var target=targets[i];if(target==null)continue;
                var p=(Vector2)target.InverseTransformPoint(transform.position);var rect=target.rect;
                var padded=rect;padded.xMin-=10;padded.yMin-=10;padded.xMax+=10;padded.yMax+=10;
                var d=(p-rect.center).sqrMagnitude;
                if(padded.Contains(p)&&d<distance){chosen=i;distance=d;}
            }
            if(chosen>=0 && drop?.Invoke(chosen)==true)
            {
                placed=true;var rt=(RectTransform)transform;rt.SetParent(targets[chosen],true);
                rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=Vector2.zero;
                rt.sizeDelta=targets[chosen].rect.size*.92f;
                SoftAudio.Cue(SoftCue.Relic_Place);feedback?.Invoke(true);
            }
            else {transform.position=home;SoftAudio.Cue(SoftCue.UI_Invalid);feedback?.Invoke(false);}
        }
        void OnDisable(){if(dragging)transform.position=home;dragging=false;}
    }
}
