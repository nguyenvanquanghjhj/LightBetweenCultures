using UnityEngine;
using UnityEngine.EventSystems;

namespace LightBetweenCultures
{
    // Overlapping large touch areas prefer the closest symbol instead of sibling order.
    public sealed class MapNodeHitTarget : MonoBehaviour, ICanvasRaycastFilter,IPointerDownHandler
    {
        public CityMap owner;
        public int index;
        public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left&&owner!=null&&owner.View.InputEnabled&&!owner.View.SuppressSelection&&TouchInputPolicy.ActiveTouches<2)owner.NodeMarkers[index].PressFeedback();}
        public bool IsRaycastLocationValid(Vector2 point,Camera camera)
            => owner != null && owner.IsNearestNode(index,point);
    }
}
