using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Small UI illustrations extend the existing code-native symbol set.
    // 0 rice, 1 vegetables, 2 egg, 3 gochujang, 4 flower, 5 serving bowl.
    public sealed class CatRescueTokenGraphic : MaskableGraphic
    {
        public int kind;
        VertexHelper mesh;
        Color Ink => new Color(.31f,.35f,.25f);
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            mesh=vh;vh.Clear();
            if(kind==5)
            {
                Oval(.50f,.47f,.49f,.47f,new Color(.29f,.27f,.19f));
                Oval(.50f,.50f,.47f,.46f,new Color(.95f,.83f,.61f));
                Oval(.50f,.50f,.42f,.40f,new Color(1,.96f,.84f));
                Oval(.50f,.48f,.37f,.35f,new Color(.88f,.84f,.68f));return;
            }
            if(kind==4)
            {
                Line(.49f,.05f,.51f,.51f,.055f,new Color(.22f,.44f,.24f));
                Oval(.37f,.25f,.13f,.06f,new Color(.37f,.59f,.30f));
                for(var i=0;i<5;i++){var a=i*Mathf.PI*2/5;Oval(.5f+Mathf.Cos(a)*.22f,.60f+Mathf.Sin(a)*.22f,.17f,.17f,new Color(.63f,.47f,.81f));}
                Oval(.5f,.60f,.12f,.12f,new Color(1,.79f,.25f));Oval(.70f,.79f,.035f,.045f,new Color(.62f,.92f,1));return;
            }
            if(kind==2)
            {
                Oval(.50f,.49f,.42f,.32f,Ink);Oval(.50f,.51f,.40f,.31f,new Color(1,.98f,.86f));
                Oval(.55f,.53f,.17f,.19f,new Color(.98f,.64f,.10f));Oval(.51f,.60f,.06f,.04f,new Color(1,.86f,.33f));return;
            }
            if(kind==1)
            {
                for(var i=0;i<7;i++)Oval(.29f+(i%3)*.17f,.30f+(i/3)*.18f,.16f,.12f,new Color(.22f+i*.03f,.46f+i*.02f,.22f));
                for(var i=0;i<4;i++)Line(.32f+i*.07f,.32f,.60f+i*.07f,.72f,.055f,new Color(.96f,.48f,.15f));return;
            }
            Oval(.5f,.38f,.40f,.22f,Ink);Oval(.5f,.42f,.37f,.20f,new Color(.36f,.56f,.52f));
            if(kind==0)
            {
                Oval(.5f,.57f,.36f,.22f,new Color(1,.98f,.86f));
                for(var i=0;i<14;i++){var x=.25f+(i%5)*.12f;var y=.50f+(i/5)*.09f;Oval(x,y,.035f,.014f,new Color(.84f,.79f,.63f));}
            }
            else
            {
                Oval(.50f,.56f,.35f,.12f,new Color(.65f,.16f,.10f));Oval(.45f,.61f,.14f,.05f,new Color(.84f,.27f,.14f));
                Line(.65f,.60f,.88f,.86f,.045f,new Color(.60f,.43f,.22f));
            }
        }
        Color Tint(Color c)=>new Color(c.r*color.r,c.g*color.g,c.b*color.b,c.a*color.a);
        Vector2 P(float x,float y)=>rectTransform.rect.min+Vector2.Scale(new Vector2(x,y),rectTransform.rect.size);
        void Oval(float x,float y,float rx,float ry,Color c)
        {
            var start=mesh.currentVertCount;mesh.AddVert(P(x,y),Tint(c),Vector2.zero);
            const int n=24;
            for(var i=0;i<n;i++){var a=i*Mathf.PI*2/n;mesh.AddVert(P(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry),Tint(c),Vector2.zero);}
            for(var i=0;i<n;i++)mesh.AddTriangle(start,start+i+1,start+(i+1)%n+1);
        }
        void Line(float ax,float ay,float bx,float by,float width,Color c)
        {
            var a=P(ax,ay);var b=P(bx,by);var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            var start=mesh.currentVertCount;mesh.AddVert(a-n,Tint(c),Vector2.zero);mesh.AddVert(b-n,Tint(c),Vector2.zero);mesh.AddVert(b+n,Tint(c),Vector2.zero);mesh.AddVert(a+n,Tint(c),Vector2.zero);mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
    }
}
