using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public enum AdventureSymbol { Globe, Journal, Check, Close, Arrow, Chat, Plus, Minus, Overview, Camera, BossAlert, TalkDots, NeedItem, Puzzle, Portal, Aura, Motion, Boat }

    /// <summary>Resolution independent HUD symbols; no font glyph or platform emoji dependency.</summary>
    public sealed class AdventureIconGraphic : MaskableGraphic
    {
        public AdventureSymbol symbol;
        VertexHelper mesh;
        Vector2 origin;
        float unit;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); mesh=vh; unit=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height);
            origin=rectTransform.rect.center-Vector2.one*unit*.5f;
            switch(symbol)
            {
                case AdventureSymbol.Boat:
                    Polygon(new[]{new Vector2(-.45f,.45f),new Vector2(1.45f,.45f),new Vector2(1.12f,.14f),new Vector2(-.12f,.14f)},DesignKitArt.Teal);
                    Line(-.45f,.45f,1.45f,.45f,.065f);Line(1.45f,.45f,1.12f,.14f,.065f);Line(1.12f,.14f,-.12f,.14f,.065f);Line(-.12f,.14f,-.45f,.45f,.065f);
                    Line(-.2f,.29f,1.2f,.29f,.04f);Line(.1f,.45f,.1f,.65f,.04f);Line(.9f,.45f,.9f,.65f,.04f);break;
                case AdventureSymbol.Aura:Ellipse(.5f,.5f,.46f,.46f);break;
                case AdventureSymbol.Motion:
                    for(var i=0;i<24;i++){var a=.1f+i*.8f/24;var b=.1f+(i+1)*.8f/24;Line(a,.5f+Mathf.Sin((a-.1f)*Mathf.PI*2/.8f)*.24f,b,.5f+Mathf.Sin((b-.1f)*Mathf.PI*2/.8f)*.24f,.07f);}break;
                case AdventureSymbol.Camera:
                    Line(.12f,.22f,.88f,.22f,.065f);Line(.88f,.22f,.88f,.71f,.065f);Line(.88f,.71f,.12f,.71f,.065f);Line(.12f,.71f,.12f,.22f,.065f);
                    Line(.27f,.71f,.32f,.83f);Line(.32f,.83f,.61f,.83f);Line(.61f,.83f,.67f,.71f);Ellipse(.50f,.46f,.18f,.18f);Dot(.76f,.61f,.035f);break;
                case AdventureSymbol.BossAlert:Line(.5f,.37f,.5f,.85f,.13f);Dot(.5f,.18f,.075f);break;
                case AdventureSymbol.TalkDots:case AdventureSymbol.NeedItem:
                    Chat();Dot(.32f,.54f,.045f);Dot(.50f,.54f,.045f);Dot(.68f,.54f,.045f);
                    if(symbol==AdventureSymbol.NeedItem){Line(.65f,.08f,.91f,.08f);Line(.91f,.08f,.91f,.31f);Line(.91f,.31f,.65f,.31f);Line(.65f,.31f,.65f,.08f);Ellipse(.78f,.33f,.06f,.06f);}break;
                case AdventureSymbol.Puzzle:
                    Line(.2f,.16f,.8f,.16f,.065f);Line(.8f,.16f,.8f,.43f,.065f);Ellipse(.8f,.55f,.12f,.12f);
                    Line(.8f,.67f,.8f,.8f,.065f);Line(.8f,.8f,.63f,.8f,.065f);Ellipse(.5f,.8f,.13f,.12f);
                    Line(.37f,.8f,.2f,.8f,.065f);Line(.2f,.8f,.2f,.16f,.065f);break;
                case AdventureSymbol.Portal:
                    Line(.22f,.12f,.22f,.62f,.07f);Line(.78f,.12f,.78f,.62f,.07f);Line(.16f,.12f,.84f,.12f,.07f);
                    for(var i=0;i<24;i++){var a=i*Mathf.PI/24;var b=(i+1)*Mathf.PI/24;Line(.5f+Mathf.Cos(a)*.28f,.62f+Mathf.Sin(a)*.28f,.5f+Mathf.Cos(b)*.28f,.62f+Mathf.Sin(b)*.28f,.06f);}break;
                case AdventureSymbol.Plus: Line(.17f,.5f,.83f,.5f,.08f);Line(.5f,.17f,.5f,.83f,.08f);break;
                case AdventureSymbol.Minus: Line(.17f,.5f,.83f,.5f,.08f);break;
                case AdventureSymbol.Overview:
                    Line(.17f,.65f,.17f,.83f);Line(.17f,.83f,.35f,.83f);Line(.65f,.83f,.83f,.83f);Line(.83f,.83f,.83f,.65f);
                    Line(.83f,.35f,.83f,.17f);Line(.83f,.17f,.65f,.17f);Line(.35f,.17f,.17f,.17f);Line(.17f,.17f,.17f,.35f);break;
                case AdventureSymbol.Globe:
                    Ellipse(.5f,.5f,.41f,.41f); Ellipse(.5f,.5f,.20f,.41f);
                    Line(.09f,.5f,.91f,.5f); Line(.16f,.29f,.84f,.29f); Line(.16f,.71f,.84f,.71f);
                    Polygon(new[]{new Vector2(.23f,.75f),new Vector2(.40f,.82f),new Vector2(.49f,.70f),new Vector2(.39f,.60f),new Vector2(.29f,.63f)},new Color(.55f,.82f,.64f));
                    Polygon(new[]{new Vector2(.56f,.51f),new Vector2(.72f,.55f),new Vector2(.79f,.40f),new Vector2(.66f,.22f),new Vector2(.57f,.34f)},new Color(.55f,.82f,.64f));
                    break;
                case AdventureSymbol.Journal:
                    Line(.18f,.12f,.18f,.84f); Line(.18f,.84f,.80f,.84f); Line(.80f,.84f,.80f,.12f); Line(.18f,.12f,.80f,.12f);
                    Line(.29f,.12f,.29f,.84f); Line(.43f,.65f,.67f,.65f); Line(.43f,.49f,.67f,.49f); Line(.43f,.33f,.59f,.33f); break;
                case AdventureSymbol.Check: Line(.18f,.49f,.41f,.25f,.085f); Line(.41f,.25f,.85f,.78f,.085f); break;
                case AdventureSymbol.Close: Line(.25f,.25f,.75f,.75f,.075f); Line(.25f,.75f,.75f,.25f,.075f); break;
                case AdventureSymbol.Arrow: Line(.15f,.5f,.82f,.5f,.075f); Line(.55f,.23f,.82f,.5f,.075f); Line(.55f,.77f,.82f,.5f,.075f); break;
                case AdventureSymbol.Chat:
                    Chat();break;
            }
        }
        void Chat(){Line(.15f,.28f,.15f,.80f);Line(.15f,.80f,.85f,.80f);Line(.85f,.80f,.85f,.28f);Line(.85f,.28f,.48f,.28f);Line(.48f,.28f,.27f,.10f);Line(.27f,.10f,.27f,.28f);Line(.27f,.28f,.15f,.28f);}
        void Dot(float x,float y,float radius)
        {
            var points=new Vector2[16];for(var i=0;i<points.Length;i++){var a=i*Mathf.PI*2/points.Length;points[i]=new Vector2(x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius);}Polygon(points,color);
        }
        void Line(float x1,float y1,float x2,float y2,float width=.045f)
        {
            var a=new Vector2(x1,y1);var b=new Vector2(x2,y2);var n=new Vector2(-(b-a).y,(b-a).x).normalized*width*.5f;
            Polygon(new[]{a+n,b+n,b-n,a-n},color);
        }
        void Ellipse(float x,float y,float rx,float ry)
        {
            for(int i=0;i<48;i++)
            {var a=i*Mathf.PI*2/48;var b=(i+1)*Mathf.PI*2/48;Line(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry,x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry,.035f);}
        }
        void Polygon(Vector2[] points,Color tint)
        {
            var start=mesh.currentVertCount;
            foreach(var p in points)mesh.AddVert(origin+p*unit,tint,Vector2.zero);
            for(int i=1;i<points.Length-1;i++)mesh.AddTriangle(start,start+i,start+i+1);
        }
    }
}
