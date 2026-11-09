using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Native, texture-free button surface. Remains an Image so existing skin
    // bindings/cards can keep their sprites. No shader/material/texture copies.
    public sealed class SoftButtonGraphic : Image
    {
        Color face=new Color(.94f,.82f,.59f,1);
        bool illustration,disabled;
        public Color Face {get=>face;set{if(face==value)return;face=value;SetVerticesDirty();}}
        public bool PreserveSprite
        {get=>illustration;set{if(illustration==value)return;illustration=value;SetVerticesDirty();SetMaterialDirty();}}
        public void SetDisabled(bool value){if(disabled==value)return;disabled=value;SetVerticesDirty();}
        public override Texture mainTexture=>illustration?base.mainTexture:Texture2D.whiteTexture;

        static Rect Inset(Rect r,float x,float y)=>new Rect(r.x+x,r.y+y,r.width-2*x,r.height-2*y);
        Color Tint(Color c)
        {
            if(disabled)
            {
                var grey=c.r*.28f+c.g*.60f+c.b*.12f;
                c=Color.Lerp(new Color(grey*.97f,grey,grey*.96f,c.a),c,.18f);
            }
            return c*color;
        }
        static Vector2 Corner(Rect r,float radius,int index)
        {
            var corner=index/9;var angle=(corner*90+(index%9)*11.25f)*Mathf.Deg2Rad;
            var center=new Vector2(corner==0||corner==3?r.xMax-radius:r.xMin+radius,corner<2?r.yMax-radius:r.yMin+radius);
            return center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
        }
        void Fill(VertexHelper mesh,Rect r,float radius,Color bottom,Color top)
        {
            if(r.width<=0||r.height<=0)return;
            radius=Mathf.Clamp(radius,0,Mathf.Min(r.width,r.height)*.5f);
            bottom=Tint(bottom);top=Tint(top);
            var start=mesh.currentVertCount;mesh.AddVert(r.center,Color.Lerp(bottom,top,.5f),Vector2.zero);
            for(var i=0;i<36;i++)
            {
                var p=Corner(r,radius,i);mesh.AddVert(p,Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,p.y)),Vector2.zero);
            }
            const float feather=.65f;
            var outer=new Rect(r.x-feather,r.y-feather,r.width+2*feather,r.height+2*feather);
            for(var i=0;i<36;i++){var p=Corner(outer,radius+feather,i);var c=Color.Lerp(bottom,top,Mathf.InverseLerp(r.yMin,r.yMax,p.y));c.a=0;mesh.AddVert(p,c,Vector2.zero);}
            for(var i=0;i<36;i++)
            {
                var next=(i+1)%36;
                mesh.AddTriangle(start,start+1+i,start+1+next);
                mesh.AddTriangle(start+1+i,start+37+i,start+37+next);
                mesh.AddTriangle(start+1+i,start+37+next,start+1+next);
            }
        }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            if(illustration){base.OnPopulateMesh(mesh);return;}
            mesh.Clear();var r=rectTransform.rect;if(r.width<2||r.height<2)return;
            var radius=Mathf.Min(25,Mathf.Min(r.width,r.height)*.43f);
            // Three quiet falloff bands, then a small lower lip and warm rim.
            var shadow=new Rect(r.x-.8f,r.y-4.5f,r.width+1.6f,r.height);
            Fill(mesh,shadow,radius+1,new Color(.025f,.07f,.06f,.045f),new Color(.025f,.07f,.06f,.045f));
            shadow=new Rect(r.x,r.y-3,r.width,r.height);
            Fill(mesh,shadow,radius,new Color(.025f,.07f,.06f,.11f),new Color(.025f,.07f,.06f,.11f));
            shadow=new Rect(r.x+1,r.y-1.8f,r.width-2,r.height);
            Fill(mesh,shadow,radius-1,new Color(.08f,.16f,.13f,.18f),new Color(.08f,.16f,.13f,.18f));
            Fill(mesh,r,radius,new Color(.46f,.33f,.19f,1),new Color(.78f,.61f,.36f,1));
            Fill(mesh,Inset(r,1.2f,1.2f),radius-1.2f,new Color(.74f,.53f,.29f,1),new Color(1,.91f,.70f,1));
            var surface=Inset(r,2.7f,2.7f);surface.yMin+=1.1f;
            var low=Color.Lerp(face,new Color(.07f,.15f,.14f,1),.19f);
            var high=Color.Lerp(face,new Color(1,.96f,.85f,1),.10f);
            Fill(mesh,surface,radius-2.7f,low,high);
            // A broad translucent glaze and a fine upper glint leave text crisp.
            var glaze=Inset(surface,1.2f,1.2f);glaze.yMin=Mathf.Lerp(surface.yMin,surface.yMax,.51f);
            Fill(mesh,glaze,Mathf.Min(radius-3.9f,glaze.height*.65f),new Color(1,.98f,.89f,0),new Color(1,.98f,.89f,.24f));
            var glint=new Rect(r.x+radius*.72f,r.yMax-4.7f,Mathf.Max(1,r.width-radius*1.44f),1.25f);
            Fill(mesh,glint,.625f,new Color(1,.97f,.85f,.18f),new Color(1,.98f,.89f,.52f));
        }
    }
}
