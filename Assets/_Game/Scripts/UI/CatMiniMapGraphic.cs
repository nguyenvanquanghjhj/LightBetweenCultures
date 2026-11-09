using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // A readable schematic of the same open navigation graph, with no texture
    // minification artifacts and no second movement or permission model.
    public sealed class CatMiniMapGraphic : MaskableGraphic
    {
        public GameBootstrap game;
        float refreshAt;
        void Update(){if(Time.unscaledTime>=refreshAt){refreshAt=Time.unscaledTime+.2f;SetVerticesDirty();}}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(game?.Data?.navigation==null)return;
            foreach(var edge in game.Data.navigation.edges)
            {
                if(!game.Data.navigation.EdgeOpen(edge,game.Save))continue;
                var points=edge.points;if(points==null)continue;
                for(var i=1;i<points.Length;i++)Line(vh,points[i-1],points[i],1.2f,new Color(.23f,.48f,.45f,.85f));
            }
            for(var i=0;i<game.Data.nodes.Length;i++)
            {
                if(!game.Discovery.IsVisible(i))continue;
                var p=game.Data.nodes[i].position;var r=rectTransform.rect;var center=r.min+Vector2.Scale(p,r.size);
                var start=vh.currentVertCount;vh.AddVert(center,DesignKitArt.Teal,Vector2.zero);
                for(var j=0;j<10;j++){var a=j*Mathf.PI*2/10;vh.AddVert(center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*1.8f,DesignKitArt.Teal,Vector2.zero);}
                for(var j=0;j<10;j++)vh.AddTriangle(start,start+j+1,start+(j+1)%10+1);
            }
        }
        void Line(VertexHelper vh,Vector2 a,Vector2 b,float width,Color tint)
        {
            var r=rectTransform.rect;a=r.min+Vector2.Scale(a,r.size);b=r.min+Vector2.Scale(b,r.size);
            var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;var i=vh.currentVertCount;
            vh.AddVert(a-n,tint,Vector2.zero);vh.AddVert(b-n,tint,Vector2.zero);vh.AddVert(b+n,tint,Vector2.zero);vh.AddVert(a+n,tint,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
