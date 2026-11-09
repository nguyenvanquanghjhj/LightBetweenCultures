#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        [Serializable] class QualityImage
        { public string name,sprite,texture,format;public Vector2 sourcePixels,screenPixels;public float opacity; }
        [Serializable] class QualityFrame
        { public string view,pipeline;public int width,height,mipmapLimit;public QualityImage[] images; }
        void QualitySample(string name)
        {
            Canvas.ForceUpdateCanvases();
            var images=FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.sprite!=null).Select(i=>
            {
                var s=i.sprite;var corners=new Vector3[4];i.rectTransform.GetWorldCorners(corners);
                var size=(Vector2)(corners[2]-corners[0]);
                if(i.preserveAspect){var ratio=s.rect.width/s.rect.height;size=size.x/size.y>ratio?new Vector2(size.y*ratio,size.y):new Vector2(size.x,size.x/ratio);}
                return new QualityImage{name=i.name,sprite=s.name,texture=s.texture.name,format=s.texture.format.ToString(),sourcePixels=s.rect.size,screenPixels=size,opacity=i.canvasRenderer.GetInheritedAlpha()};
            }).ToList();
            foreach(var r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.sprite!=null))
            {
                var c=Camera.main;if(c==null)continue;var s=r.sprite;
                var a=c.WorldToScreenPoint(r.bounds.min);var b=c.WorldToScreenPoint(r.bounds.max);
                images.Add(new QualityImage{name=r.name,sprite=s.name,texture=s.texture.name,format=s.texture.format.ToString(),sourcePixels=s.rect.size,screenPixels=(Vector2)(b-a),opacity=r.color.a});
            }
            File.WriteAllText(Path.Combine(output,name+"-sampling.json"),JsonUtility.ToJson(new QualityFrame{view=name,width=Screen.width,height=Screen.height,mipmapLimit=QualitySettings.globalTextureMipmapLimit,pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.name??"BuiltIn",images=images.ToArray()},true));
        }
    }
}
#endif
