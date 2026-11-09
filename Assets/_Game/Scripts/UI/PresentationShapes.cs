using UnityEngine;

namespace LightBetweenCultures
{
    public static class PresentationShapes
    {
        private static Sprite shadow;
        private static Sprite light;
        // Small shared alpha gradient; no imported art, shader or post-processing.
        public static Sprite WarmGlow
        {
            get
            {
                if(light!=null)return light;
                var texture=new Texture2D(64,64,TextureFormat.RGBA32,false){name="LocalQuestLightGradient",wrapMode=TextureWrapMode.Clamp};
                for(var y=0;y<64;y++)for(var x=0;x<64;x++)
                {var r=new Vector2((x-31.5f)/31.5f,(y-31.5f)/31.5f).sqrMagnitude;var a=Mathf.Clamp01(1-r);texture.SetPixel(x,y,new Color(1,1,1,a*a));}
                texture.Apply();light=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);return light;
            }
        }
        public static Sprite Shadow
        {
            get
            {
                if (shadow != null) return shadow;
                var texture = new Texture2D(64,32,TextureFormat.RGBA32,false);
                texture.name = "ProceduralFootShadow";
                for (var y=0;y<32;y++) for(var x=0;x<64;x++)
                {
                    var d = new Vector2((x-31.5f)/31.5f,(y-15.5f)/15.5f).sqrMagnitude;
                    texture.SetPixel(x,y,new Color(.04f,.08f,.08f,Mathf.Clamp01(1-d)*.26f));
                }
                texture.Apply();
                shadow=Sprite.Create(texture,new Rect(0,0,64,32),new Vector2(.5f,.5f),64);
                return shadow;
            }
        }
    }
}
