using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Hangeul h/a/n and the syllable han, without relying on OS CJK fonts.
    public sealed class CatHangulGraphic : MaskableGraphic
    {
        public int letter;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            if(letter==3){H(vh,new Rect(.09f,.40f,.37f,.52f));A(vh,new Rect(.57f,.39f,.30f,.53f));N(vh,new Rect(.18f,.08f,.65f,.23f));}
            else if(letter==0)H(vh,new Rect(.16f,.10f,.68f,.80f));
            else if(letter==1)A(vh,new Rect(.26f,.10f,.60f,.80f));
            else N(vh,new Rect(.20f,.17f,.60f,.65f));
        }
        Vector2 P(Rect r,float x,float y) => rectTransform.rect.min+Vector2.Scale(r.min+Vector2.Scale(new Vector2(x,y),r.size),rectTransform.rect.size);
        void H(VertexHelper v,Rect r)
        {
            Line(v,P(r,.27f,1),P(r,.73f,1));Line(v,P(r,.1f,.78f),P(r,.9f,.78f));
            for(var i=0;i<28;i++){var a=i*Mathf.PI*2/28;var b=(i+1)*Mathf.PI*2/28;Line(v,P(r,.5f+Mathf.Cos(a)*.35f,.33f+Mathf.Sin(a)*.28f),P(r,.5f+Mathf.Cos(b)*.35f,.33f+Mathf.Sin(b)*.28f));}
        }
        void A(VertexHelper v,Rect r){Line(v,P(r,.3f,0),P(r,.3f,1));Line(v,P(r,.3f,.55f),P(r,.95f,.55f));}
        void N(VertexHelper v,Rect r){Line(v,P(r,.1f,.9f),P(r,.1f,.1f));Line(v,P(r,.1f,.1f),P(r,.9f,.1f));}
        void Line(VertexHelper v,Vector2 a,Vector2 b)
        {
            var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.025f;
            var i=v.currentVertCount;v.AddVert(a-n,color,Vector2.zero);v.AddVert(b-n,color,Vector2.zero);v.AddVert(b+n,color,Vector2.zero);v.AddVert(a+n,color,Vector2.zero);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);
        }
    }
}
