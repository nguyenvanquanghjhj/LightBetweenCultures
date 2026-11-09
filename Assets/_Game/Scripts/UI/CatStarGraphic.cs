using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed class CatStarGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;var unit=Mathf.Min(r.width,r.height)*.46f;
            vh.AddVert(r.center,color,Vector2.zero);
            for(var i=0;i<10;i++)
            {var a=Mathf.PI*.5f+i*Mathf.PI/5;var radius=unit*(i%2==0?1:.43f);vh.AddVert(r.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);}
            for(var i=0;i<10;i++)vh.AddTriangle(0,i+1,(i+1)%10+1);
        }
    }
}
