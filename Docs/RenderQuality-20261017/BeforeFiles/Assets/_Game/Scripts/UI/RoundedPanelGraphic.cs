using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Small native UI shape: crisp at any Canvas scale, no extra texture asset.
    public sealed class RoundedPanelGraphic : MaskableGraphic
    {
        public float radius = 14;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            var rect = rectTransform.rect;
            var r = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * .5f);
            mesh.AddVert(rect.center, color, Vector2.zero);
            for (var corner = 0; corner < 4; corner++)
            {
                var center = new Vector2(corner == 0 || corner == 3 ? rect.xMax-r : rect.xMin+r,
                    corner < 2 ? rect.yMax-r : rect.yMin+r);
                for (var step = 0; step <= 6; step++)
                {
                    var angle = (corner * 90 + step * 15) * Mathf.Deg2Rad;
                    mesh.AddVert(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle))*r, color, Vector2.zero);
                }
            }
            for (var i = 1; i <= 28; i++) mesh.AddTriangle(0, i, i == 28 ? 1 : i+1);
        }
    }
}
