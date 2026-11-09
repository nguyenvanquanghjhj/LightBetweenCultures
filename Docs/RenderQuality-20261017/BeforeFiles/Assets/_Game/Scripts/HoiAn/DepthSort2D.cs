using UnityEngine;
using UnityEngine.Rendering;
namespace LightBetweenCultures.HoiAn
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DepthSort2D : MonoBehaviour
    {
        private SpriteRenderer visual;
        private SortingGroup group;
        private void Awake() { visual=GetComponent<SpriteRenderer>(); group=GetComponent<SortingGroup>(); }
        private void LateUpdate()
        {
            var order=100-Mathf.RoundToInt((transform.position.y-visual.bounds.extents.y)*10);
            if(group!=null) { group.sortingOrder=order; visual.sortingOrder=0; }
            else visual.sortingOrder=order;
        }
    }
}
