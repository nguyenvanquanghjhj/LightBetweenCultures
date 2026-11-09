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
        System.Collections.IEnumerator QualityAddendumJourney()
        {
            Check(game.Quests.IsCompleted("HQ07")&&game.Inventory.Count("festival_friendship_postcard")==1,"Targeted art QA uses an earned postgame postcard, no synthetic completion flags");
            yield return CrossEnter("hoi_an");
            var saved=JsonUtility.ToJson(game.Save);var art=AdventureUIArt.Load();
            foreach(var id in new[]{"festival_friendship_postcard","hoi_an_bridge_postcard"})
                Check(art.Item(id).sprite.name=="scroll"&&art.Item(id).categoryIcon&&art.Item(id).status.StartsWith("NEEDS_ART"),"Clean existing category icon replaces the credited postcard reference: "+id);
            yield return CrossBag("Postcard-inventory-earned","festival_friendship_postcard");
            var panels=(GameplayPanels)typeof(CityMap).GetField("panels",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(Map);
            // Presentation-only receipt preview of an already earned item. This
            // does not execute a quest/grant and is never E2E completion proof.
            panels.ShowEvent(10,"QA receipt presentation fixture",new[]{new ItemAmount{itemId="festival_friendship_postcard",amount=1}});
            Check(GameObject.Find("ReceivedIcon_festival_friendship_postcard").GetComponent<Image>().sprite==art.Item("festival_friendship_postcard").sprite,"Receipt presentation fixture uses the same clean icon as inventory");
            yield return Snapshot("Postcard-receipt-PRESENTATION-FIXTURE");yield return Click("ReceiptContinue",true);yield return CloseEvent();
            Check(JsonUtility.ToJson(game.Save)==saved,"Inventory and receipt preview leave exact earned quest/items/world state unchanged");
            game.Persist();game.Reload();
            Check(game.Quests.IsCompleted("HQ07")&&game.Inventory.Count("festival_friendship_postcard")==1,"Disk reload preserves HQ07 and exactly one earned postcard");
        }
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
