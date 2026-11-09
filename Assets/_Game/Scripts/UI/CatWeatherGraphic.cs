using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Light, non-interactive local weather. No full-screen opaque fog or input layer.
    public sealed class CatWeatherGraphic : MaskableGraphic
    {
        public string weather;
        public bool celebration;
        float nextFrame;
        void Update()
        {
            if(Time.unscaledTime<nextFrame)return;
            nextFrame=Time.unscaledTime+.05f;SetVerticesDirty();
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            var active=NodeMarkerView.MotionMode==MarkerMotionMode.Full;
            var t=active?Time.unscaledTime:0;
            if(!active&&!celebration)return;
            var count=celebration?28:weather=="mist"?8:20;
            for(var i=0;i<count;i++)
            {
                var x=Mathf.Repeat(i*.618034f+t*(weather=="wind"?.06f:.007f),1);
                var y=Mathf.Repeat(i*.381966f-t*(celebration?.13f:weather=="rain"?.27f:.025f),1);
                var p=r.min+Vector2.Scale(new Vector2(x,y),r.size);
                var tint=celebration?Color.HSVToRGB((i*.12f)%1,.4f,1):weather=="rain"?new Color(.7f,.89f,1,.24f):weather=="mist"?new Color(.88f,.95f,1,.035f):new Color(1,.85f,.53f,.40f);
                var size=weather=="mist"&&!celebration?new Vector2(r.width*.7f,22):weather=="rain"&&!celebration?new Vector2(1.2f,15):weather=="wind"&&!celebration?new Vector2(18,1.2f):new Vector2(4,4);
                Quad(vh,p,size,tint);
            }
        }
        static void Quad(VertexHelper vh,Vector2 p,Vector2 size,Color tint)
        {
            var i=vh.currentVertCount;
            vh.AddVert(p,tint,Vector2.zero);vh.AddVert(p+Vector2.right*size.x,tint,Vector2.zero);
            vh.AddVert(p+size,tint,Vector2.zero);vh.AddVert(p+Vector2.up*size.y,tint,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
