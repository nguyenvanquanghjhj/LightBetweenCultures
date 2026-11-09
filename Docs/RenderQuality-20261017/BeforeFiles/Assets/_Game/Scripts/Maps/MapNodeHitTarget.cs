using UnityEngine;

namespace LightBetweenCultures
{
    // Overlapping large touch areas prefer the closest symbol instead of sibling order.
    public sealed class MapNodeHitTarget : MonoBehaviour, ICanvasRaycastFilter
    {
        public CityMap owner;
        public int index;
        public bool IsRaycastLocationValid(Vector2 point,Camera camera)
            => owner != null && owner.IsNearestNode(index,point);
    }
}
