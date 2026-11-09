using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Static low-opacity local atmosphere. No full-screen rain needles, floating
    // points, vertical strips or per-frame UI mesh regeneration over readable maps.
    public sealed class CatWeatherGraphic : MaskableGraphic
    {
        string currentWeather;
        bool isCelebrating;
        public string weather
        {
            get => currentWeather;
            set { if(currentWeather!=value) { currentWeather=value; SetVerticesDirty(); } }
        }
        public bool celebration
        {
            get => isCelebrating;
            set { if(isCelebrating!=value) { isCelebrating=value; SetVerticesDirty(); } }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r=rectTransform.rect;
            // Gentle broad bands are a background tint, not moving obstacles.
            if(isCelebrating)
            {
                Band(vh,r,0,.08f,new Color(1f,.77f,.32f,.07f));
                Band(vh,r,.92f,1,new Color(1f,.77f,.32f,.07f));
            }
            else if(currentWeather=="rain")
            {
                Band(vh,r,0,.11f,new Color(.39f,.63f,.84f,.055f));
                Band(vh,r,.88f,1,new Color(.48f,.70f,.87f,.023f));
            }
            else if(currentWeather=="mist")
            {
                Band(vh,r,0,.16f,new Color(.77f,.88f,.91f,.065f));
            }
            else if(currentWeather=="wind")
            {
                Band(vh,r,.90f,1,new Color(.96f,.87f,.72f,.025f));
            }
        }
        static void Band(VertexHelper vh,Rect r,float low,float high,Color tint)
        {
            var a=new Vector2(r.xMin,r.yMin+r.height*low);
            var b=new Vector2(r.xMax,r.yMin+r.height*high);
            var i=vh.currentVertCount;
            vh.AddVert(new Vector3(a.x,a.y,0f),tint,Vector2.zero);
            vh.AddVert(new Vector3(b.x,a.y,0f),tint,Vector2.zero);
            vh.AddVert(new Vector3(b.x,b.y,0f),tint,Vector2.zero);
            vh.AddVert(new Vector3(a.x,b.y,0f),tint,Vector2.zero);
            vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);
        }
    }
}
