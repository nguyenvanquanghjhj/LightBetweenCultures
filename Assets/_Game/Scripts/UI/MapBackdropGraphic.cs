using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Editable schematic shapes, not a raster concept board. Replace with a clean map backdrop when supplied.
    public sealed class MapBackdropGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            Rect(mesh,.0f,.0f,1,1,DesignKitArt.Hex("EED8A4"));
            var river = new[] { new Vector2(.35f,0), new Vector2(.42f,.2f), new Vector2(.53f,.4f), new Vector2(.54f,.6f), new Vector2(.64f,.8f), new Vector2(.66f,1) };
            for (var i=1;i<river.Length;i++)
            {
                Ribbon(mesh,river[i-1],river[i],.15f,DesignKitArt.Hex("D9BD82"));
                Ribbon(mesh,river[i-1],river[i],.12f,DesignKitArt.Teal);
                Ribbon(mesh,river[i-1]+new Vector2(.025f,0),river[i]+new Vector2(.025f,0),.007f,DesignKitArt.Hex("6BA8A0"));
            }
            foreach (var p in new[] { new Vector2(.1f,.12f),new Vector2(.12f,.82f),new Vector2(.82f,.87f),new Vector2(.88f,.25f),new Vector2(.34f,.55f),new Vector2(.32f,.92f) })
            {
                Rect(mesh,p.x-.035f,p.y-.025f,.07f,.07f,DesignKitArt.Wall);
                Quad(mesh,new Vector2(p.x-.045f,p.y+.045f),new Vector2(p.x+.045f,p.y+.045f),new Vector2(p.x+.03f,p.y+.075f),new Vector2(p.x-.03f,p.y+.075f),DesignKitArt.Roof);
                Rect(mesh,p.x-.01f,p.y-.025f,.02f,.035f,DesignKitArt.Ink);
            }
            // Independent bridge planks and scattered planting establish Hội An's river/roof rhythm.
            Rect(mesh,.45f,.58f,.19f,.065f,DesignKitArt.Hex("8B6246"));
            for (var i=0;i<8;i++) Rect(mesh,.455f+i*.023f,.584f,.017f,.057f,DesignKitArt.Hex("CFAC75"));
            foreach (var p in new[] {new Vector2(.08f,.3f),new Vector2(.29f,.70f),new Vector2(.79f,.72f),new Vector2(.87f,.08f),new Vector2(.09f,.94f),new Vector2(.91f,.94f)})
            {
                Rect(mesh,p.x-.008f,p.y-.02f,.016f,.045f,DesignKitArt.Hex("8B6246"));
                Disc(mesh,p+new Vector2(0,.025f),.028f,DesignKitArt.Hex("568661"));
                Disc(mesh,p+new Vector2(-.009f,.033f),.018f,DesignKitArt.Hex("7DA276"));
            }
        }
        private Vector3 Point(Vector2 p) => new Vector3(rectTransform.rect.xMin+p.x*rectTransform.rect.width,rectTransform.rect.yMin+p.y*rectTransform.rect.height);
        private void Quad(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color color)
        {
            var start=mesh.currentVertCount;
            foreach (var p in new[]{a,b,c,d}) mesh.AddVert(Point(p),color,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2); mesh.AddTriangle(start+2,start+3,start);
        }
        private void Rect(VertexHelper mesh,float x,float y,float w,float h,Color color) => Quad(mesh,new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h),color);
        private void Ribbon(VertexHelper mesh,Vector2 a,Vector2 b,float width,Color color)
        { var offset=new Vector2(width*.5f,0); Quad(mesh,a-offset,a+offset,b+offset,b-offset,color); }
        private void Disc(VertexHelper mesh,Vector2 p,float radius,Color color)
        {
            var start=mesh.currentVertCount; mesh.AddVert(Point(p),color,Vector2.zero);
            for(var i=0;i<=12;i++)
            {
                var a=i*Mathf.PI/6;
                mesh.AddVert(Point(p+new Vector2(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.7f)),color,Vector2.zero);
                if(i>0) mesh.AddTriangle(start,start+i,start+i+1);
            }
        }
    }
}
