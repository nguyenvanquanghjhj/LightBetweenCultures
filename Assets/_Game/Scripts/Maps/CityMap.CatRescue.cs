using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        Text catStatus;
        Button catButton;
        RectTransform catMapMarker, catMiniPlayer, catMiniTarget;
        CatWeatherGraphic catWeather;
        int catTravelTarget=-1;
        readonly List<Image> catLanternLights=new List<Image>();
        public int CatLitLanterns {get;private set;}
        void CreateCatRescue()
        {
            if(game.Cats?.Current==null)return;
            panels.CatDestination=target=>StartCoroutine(CatTravel(target));
            View.Viewport.offsetMax=new Vector2(0,-153);
            catButton=MobileUI.Button(ui,ui.Root,"CatRescueHUD","Giải cứu mèo",Vector2.one,Vector2.one,OpenCatRescue,true,true);
            var rt=(RectTransform)catButton.transform;rt.anchorMin=Vector2.up;rt.anchorMax=Vector2.up;rt.offsetMin=new Vector2(14,-145);rt.offsetMax=new Vector2(180,-96);
            var icon=ui.Picture(catButton.transform,"CatHUDPortrait",CatRescueArt.Cat(game.Cats.Current.spriteIndex),new Vector2(.02f,.05f),new Vector2(.25f,.95f));
            UIController.SetRect(catButton.transform.Find("Label").GetComponent<RectTransform>(),new Vector2(.27f,.05f),new Vector2(.98f,.95f));
            catStatus=ui.Label(ui.Root,"CatRescueObjective","",Vector2.up,Vector2.one,15);catStatus.alignment=TextAnchor.MiddleLeft;catStatus.color=DesignKitArt.Teal;
            catStatus.rectTransform.offsetMin=new Vector2(190,-146);catStatus.rectTransform.offsetMax=new Vector2(-14,-96);
            var audio=ui.Root.Find("AudioSettings")?.GetComponent<Button>();
            if(audio!=null)
            {
                var ar=(RectTransform)audio.transform;ar.anchorMin=ar.anchorMax=Vector2.zero;ar.offsetMin=new Vector2(242,91);ar.offsetMax=new Vector2(294,143);
                var label=audio.GetComponentInChildren<Text>();label.text="Âm\nthanh";label.fontSize=13;mapControls.Add(audio);
            }
            var mini=MobileUI.Button(ui,View.Viewport,"CatMiniMap","",Vector2.one,Vector2.one,()=>
            {
                if(dialogue||Moving)return;
                var target=game.Cats.TargetIndex();
                if(target>=0)StartCoroutine(CatTravel(target));
                else OpenCatRescue();
            },illustrated:true);
            var mr=(RectTransform)mini.transform;mr.offsetMin=new Vector2(-142,-126);mr.offsetMax=new Vector2(-12,-12);mini.transform.Find("Label").gameObject.SetActive(false);
            var preview=ui.Panel(mini.transform,"MiniMapImage",new Vector2(.06f,.25f),new Vector2(.94f,.95f),DesignKitArt.Cream);preview.GetComponent<Image>().raycastTarget=false;
            var routeLabel=ui.Label(mini.transform,"MiniMapRouteHint","ĐI ĐẾN MỤC TIÊU",new Vector2(.02f,.02f),new Vector2(.98f,.24f),12,false);
            routeLabel.color=DesignKitArt.Ink;routeLabel.raycastTarget=false;
            var graph=new GameObject("MiniMapRoads",typeof(RectTransform),typeof(CatMiniMapGraphic)).GetComponent<CatMiniMapGraphic>();graph.transform.SetParent(preview,false);UIController.SetRect(graph.rectTransform,Vector2.zero,Vector2.one);graph.game=game;graph.raycastTarget=false;
            catMiniTarget=ui.Picture(preview.transform,"MiniMapTarget",PresentationShapes.WarmGlow,Vector2.zero,Vector2.zero).rectTransform;catMiniTarget.sizeDelta=new Vector2(16,16);catMiniTarget.GetComponent<Image>().color=DesignKitArt.Gold;
            catMiniPlayer=ui.Picture(preview.transform,"MiniMapLumi",PresentationShapes.WarmGlow,Vector2.zero,Vector2.zero).rectTransform;catMiniPlayer.sizeDelta=new Vector2(11,11);catMiniPlayer.GetComponent<Image>().color=DesignKitArt.Teal;
            var markerNode=System.Array.FindIndex(game.Data.nodes,n=>n.id==game.Cats.Current.rescueNode);
            var anchor=markerNode>=0?CityRouteGeometry.Arrival(game.Data.nodes[markerNode]):Vector2.one*.5f;
            // A location illustration only: the existing node owns map input.
            // Opening the rescue journey uses the single CatRescueHUD button.
            catMapMarker=ui.Panel(map,"CatOnMap",anchor,anchor,Color.clear);catMapMarker.sizeDelta=new Vector2(44,48);
            catMapMarker.GetComponent<Image>().raycastTarget=false;
            var portrait=ui.Picture(catMapMarker,"WaitingCat",CatRescueArt.Cat(game.Cats.Current.spriteIndex),Vector2.zero,Vector2.one);
            portrait.raycastTarget=false;portrait.gameObject.AddComponent<CatRescueMotion>();
            var weatherObject=new GameObject("CatCityWeather",typeof(RectTransform),typeof(CatWeatherGraphic));weatherObject.transform.SetParent(View.Viewport,false);UIController.SetRect((RectTransform)weatherObject.transform,Vector2.zero,Vector2.one);
            catWeather=weatherObject.GetComponent<CatWeatherGraphic>();catWeather.weather=game.Cats.Current.weather;catWeather.raycastTarget=false;
            if(game.Data.cityId=="hoi_an")
            {
                var relic=game.Relics.Definition("relic_hoi_an_lantern");
                var points=relic?.lights;
                if(points==null||points.Length==0)points=(relic?.lightNodes??System.Array.Empty<string>()).Select(id=>System.Array.Find(game.Data.nodes,n=>n.id==id)).Where(n=>n!=null).Select(n=>n.position).ToArray();
                if(points!=null)
                {
                    var layer=ui.Panel(map,"CatLanternLightLayer",Vector2.zero,Vector2.one,Color.clear);layer.GetComponent<Image>().raycastTarget=false;layer.SetSiblingIndex(1);
                    for(var i=0;i<points.Length;i++)
                    {
                        var glow=ui.Picture(layer,"CatLanternGlow"+i,PresentationShapes.WarmGlow,points[i],points[i]);glow.rectTransform.sizeDelta=new Vector2(110,110);glow.color=new Color(1,.73f,.23f,.32f);catLanternLights.Add(glow);
                    }
                }
            }
        }
        public void OpenCatRescue()
        {
            if(dialogue||Moving||game.Scenes.Busy)return;
            Deselect();dialogue=true;Refresh();panels.ShowCatRescue();
        }
        IEnumerator CatTravel(int target)
        {
            yield return new WaitForSecondsRealtime(.30f);
            if(dialogue||Moving||target<0||target>=game.Data.nodes.Length)yield break;
            FocusLocation(target);
            if(game.Save.node!=target)
            {
                catTravelTarget=target;
                TryMove(target);
                while(Moving)yield return null;
                catTravelTarget=-1;
            }
            if(game.Save.node==target&&!dialogue)OpenCatRescue();
        }
        void RefreshCatRescue()
        {
            if(catStatus==null)return;
            var cats=game.Cats;var c=cats.Current;var p=cats.Progress(c.cityId);
            catStatus.text=cats.Guidance();catButton.GetComponentInChildren<Text>().text="CỨU MÈO "+cats.RescuedCount+"/6";catButton.interactable=!dialogue&&!Moving;
            var target=cats.TargetIndex();catMiniTarget.gameObject.SetActive(target>=0);
            if(target>=0)catMiniTarget.anchorMin=catMiniTarget.anchorMax=CityRouteGeometry.Arrival(game.Data.nodes[target]);
            catMiniPlayer.anchorMin=catMiniPlayer.anchorMax=Marker.anchorMin;
            // The waiting cat must be visible before accepting the rescue quest.
            catMapMarker.gameObject.SetActive(p?.rescued!=true);catMapMarker.localScale=Vector3.one/View.Zoom;
            catMapMarker.anchoredPosition=new Vector2(60,0)/View.Zoom;
            catWeather.gameObject.SetActive(p?.rescued!=true&&!dialogue);
            CatLitLanterns=p?.started==true?Mathf.CeilToInt(catLanternLights.Count*p.completed.Count/(float)c.challenges.Length):0;
            for(var i=0;i<catLanternLights.Count;i++)catLanternLights[i].gameObject.SetActive(i<CatLitLanterns);
        }
    }
}
