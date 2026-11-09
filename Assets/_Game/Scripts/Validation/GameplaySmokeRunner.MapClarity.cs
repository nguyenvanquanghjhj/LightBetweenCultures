#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        [Serializable] class ClarityImage
        {
            public string name,sprite,texture,format,filter;public Vector2 spritePixels,texturePixels,screenPixels,screenCenter;
            public float mipBias,alpha;public int mipCount;public bool raycast;
        }
        [Serializable] class ClarityFrame
        {
            public string view,city,runtime,pipeline;public int width,height,mipmapLimit;public float zoom,canvasScale;
            public Vector2 contentPosition,contentSize;public ClarityImage[] images;
        }
        IEnumerator ClarityShot(string name)
        {
            yield return new WaitForSecondsRealtime(.6f);Canvas.ForceUpdateCanvases();
            var images=FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.sprite!=null).Select(i=>
            {
                var s=i.sprite;var t=s.texture;var corners=new Vector3[4];i.rectTransform.GetWorldCorners(corners);
                var size=(Vector2)(corners[2]-corners[0]);
                if(i.preserveAspect){var ratio=s.rect.width/s.rect.height;size=size.x/size.y>ratio?new Vector2(size.y*ratio,size.y):new Vector2(size.x,size.x/ratio);}
                return new ClarityImage{name=i.name,sprite=s.name,texture=t.name,format=t.format.ToString(),filter=t.filterMode.ToString(),
                    spritePixels=s.rect.size,texturePixels=new Vector2(t.width,t.height),screenPixels=size,screenCenter=(corners[0]+corners[2])*.5f,
                    mipBias=t.mipMapBias,mipCount=t.mipmapCount,alpha=i.canvasRenderer.GetInheritedAlpha(),raycast=i.raycastTarget};
            }).ToArray();
            var frame=new ClarityFrame{view=name,city=game.Data.cityId,runtime=Application.isEditor?"Windows Unity Play Mode":"Windows Player",width=Screen.width,height=Screen.height,
                pipeline=UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline?.name??"BuiltIn",mipmapLimit=QualitySettings.globalTextureMipmapLimit,
                zoom=Map.View.Zoom,canvasScale=Map.GetComponentInParent<Canvas>()?.scaleFactor??FindAnyObjectByType<Canvas>().scaleFactor,
                contentPosition=Map.View.Content.anchoredPosition,contentSize=Map.View.Content.rect.size,images=images};
            File.WriteAllText(Path.Combine(output,name+".json"),JsonUtility.ToJson(frame,true));
            var path=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(.4f);
            Check(File.Exists(path),"Real rendered capture exists: "+name);captures.Add(frame.runtime+" | "+path);
        }
        IEnumerator MapClarityJourney()
        {
            musicVirtualFocus=true;PlayerPrefs.SetInt("LBC.PlayerGuide.QA",1);
            foreach(var city in new[]{"hoi_an","hue","seoul"})
            {
                yield return CrossEnter(city);NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
                var saved=JsonUtility.ToJson(game.Save);
                Map.View.ResetView();yield return ClarityShot(city+"-overview");
                var id=city=="hoi_an"?"H2":city=="hue"?"U1":"S5";
                Map.View.ZoomBy(2.6f/Map.View.Zoom);Map.View.Focus(game.Data.nodes[NodeIndex(id)].position);
                yield return ClarityShot(city+"-detail");
                if(city=="hoi_an")
                {
                    Map.View.Focus(game.Data.nodes[NodeIndex("H12")].position);yield return ClarityShot("hoi_an-crab");
                    Map.View.Focus(new Vector2(.112f,.762f));yield return ClarityShot("hoi_an-prop");
                }
                Check(JsonUtility.ToJson(game.Save)==saved,"Zoom/focus and captures never change save/progression: "+city);
                Check(Map.View.InputEnabled&&Map.View.WithinBounds(),"Map input/bounds retained: "+city);
                Check(FindButton("Inventory")!=null&&FindButton("Back")!=null,"Existing HUD controls retained: "+city);
            }
            if(Environment.GetCommandLineArgs().Contains("--map-clarity-checks"))
            {
                yield return CrossEnter("hoi_an");
                var state=JsonUtility.ToJson(game.Save);var zoom=Map.View.Zoom;
                yield return Click("ZoomIn",true);Check(Map.View.Zoom>zoom,"Touch on existing zoom control still works");
                var position=Map.View.Content.anchoredPosition;Map.View.PanBy(new Vector2(24,-18));
                Check(Map.View.WithinBounds(),"Panning retains map bounds with original viewport");
                yield return Click("Inventory",true);
                Check(FindButton("CloseInventory")!=null,"Touch opens existing inventory over map");
                yield return Click("CloseInventory",true);
                Check(JsonUtility.ToJson(game.Save)==state,"Zoom/pan/inventory do not mutate inventory, rewards, quests or progress");
                Time.timeScale=4;yield return Travel(NodeIndex("H2"),true);Time.timeScale=1;
                Check(game.Save.node==NodeIndex("H2")&&!Map.Moving,"Lumi follows existing routes to NPC after import-only changes");
                yield return CloseEvent();yield return Click("Interact",true);
                Check(Map.DialogueOpen,"Existing NPC dialogue still opens via touch");
                yield return CloseEvent();Check(Map.View.InputEnabled,"Closing dialogue restores map input");
                Check(FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"No duplicate EventSystem");
                KeepSave("clarity-ui-movement");
            }
        }
    }
}
#endif
