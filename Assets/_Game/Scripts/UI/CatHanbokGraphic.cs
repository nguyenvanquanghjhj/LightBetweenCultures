using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Garment diagram, without a face or a replacement for any approved NPC.
    public sealed class CatHanbokGraphic : MaskableGraphic
    {
        public bool pants;
        public int palette;
        VertexHelper mesh;
        Rect area;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            mesh=vh;vh.Clear();var r=rectTransform.rect;var w=Mathf.Min(r.width,r.height*.65f);var h=w/.65f;area=new Rect(r.center-new Vector2(w,h)*.5f,new Vector2(w,h));
            var tops=new[]{new Color(.29f,.64f,.59f),new Color(.93f,.62f,.58f),new Color(.68f,.58f,.80f),new Color(.96f,.78f,.39f)};
            var bottoms=new[]{new Color(.91f,.65f,.54f),new Color(.40f,.61f,.67f),new Color(.48f,.67f,.54f),new Color(.59f,.50f,.70f)};
            var i=Mathf.Clamp(palette,0,3);var top=tops[i];var bottom=bottoms[i];
            if(pants)
            {Poly(new[]{new Vector2(.23f,.63f),new Vector2(.77f,.63f),new Vector2(.83f,.08f),new Vector2(.56f,.06f),new Vector2(.50f,.40f),new Vector2(.44f,.06f),new Vector2(.17f,.08f)},bottom);}
            else
            {
                Poly(new[]{new Vector2(.28f,.67f),new Vector2(.72f,.67f),new Vector2(.96f,.08f),new Vector2(.80f,.03f),new Vector2(.20f,.03f),new Vector2(.04f,.08f)},bottom);
                for(var p=0;p<4;p++){var x=.27f+p*.15f;Poly(new[]{new Vector2(x,.58f),new Vector2(x+.035f,.58f),new Vector2(x+.06f,.06f)},new Color(1,1,1,.13f));}
            }
            Poly(new[]{new Vector2(.30f,.98f),new Vector2(.41f,1),new Vector2(.50f,.88f),new Vector2(.59f,1),new Vector2(.70f,.98f),new Vector2(.99f,.73f),new Vector2(.82f,.60f),new Vector2(.69f,.76f),new Vector2(.72f,.57f),new Vector2(.28f,.57f),new Vector2(.31f,.76f),new Vector2(.18f,.60f),new Vector2(.01f,.73f)},top);
            Poly(new[]{new Vector2(.38f,1),new Vector2(.43f,1),new Vector2(.61f,.65f),new Vector2(.56f,.63f)},new Color(1,.96f,.84f));
            Poly(new[]{new Vector2(.57f,1),new Vector2(.62f,1),new Vector2(.50f,.81f),new Vector2(.47f,.86f)},new Color(1,.96f,.84f));
            Poly(new[]{new Vector2(.55f,.72f),new Vector2(.72f,.69f),new Vector2(.65f,.65f)},new Color(.40f,.25f,.29f));
            Poly(new[]{new Vector2(.56f,.69f),new Vector2(.60f,.69f),new Vector2(.66f,.38f),new Vector2(.60f,.40f)},new Color(.40f,.25f,.29f));
        }
        void Poly(Vector2[] points,Color c)
        {
            var start=mesh.currentVertCount;var center=Vector2.zero;foreach(var p in points)center+=p;center/=points.Length;
            var tint=new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
            mesh.AddVert(area.min+Vector2.Scale(center,area.size),tint,Vector2.zero);
            foreach(var p in points)mesh.AddVert(area.min+Vector2.Scale(p,area.size),tint,Vector2.zero);
            for(var i=0;i<points.Length;i++)mesh.AddTriangle(start,start+i+1,start+(i+1)%points.Length+1);
        }
    }
}
