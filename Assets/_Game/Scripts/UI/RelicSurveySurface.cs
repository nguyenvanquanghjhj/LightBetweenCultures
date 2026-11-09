using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LightBetweenCultures
{
    // Pointer surface for scanning/clearing cosmetic fog in the existing relic panel.
    public sealed class RelicSurveySurface:MonoBehaviour,IPointerDownHandler,IDragHandler
    {
        public Action<Vector2> sample;
        int owner;uint epoch;
        public void OnPointerDown(PointerEventData e){TouchInputPolicy.Sample();if(e.button!=PointerEventData.InputButton.Left||TouchInputPolicy.ActiveTouches>1)return;owner=e.pointerId;epoch=TouchInputPolicy.GestureEpoch;Read(e);}
        public void OnDrag(PointerEventData e){TouchInputPolicy.Sample();if(owner==e.pointerId&&epoch==TouchInputPolicy.GestureEpoch&&!TouchInputPolicy.Canceled(e))Read(e);}
        void Read(PointerEventData e)
        {
            var rect=(RectTransform)transform;
            if(RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var p))
                sample?.Invoke(new Vector2((p.x-rect.rect.xMin)/rect.rect.width,(p.y-rect.rect.yMin)/rect.rect.height));
        }
    }
}
