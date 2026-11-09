using UnityEngine;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        public void ShowGuidance(int level=0)
        {
            var sheet=ActivitySheet("GuidancePanel","Việc tiếp theo",PlayerCopy.CityIdentity(game.Data.cityId));
            // Goal and known requirements belong together. Only the optional
            // clue is expanded on demand; no mandatory three-page hint flow.
            var body=game.Quests.Guidance()+"\n\n"+game.Quests.Guidance(1);
            if(level>0)body+="\n\n"+game.Quests.Guidance(2);
            var viewport=ui.Panel(sheet,"GuidanceViewport",new Vector2(.09f,.43f),new Vector2(.91f,.77f),Color.clear);
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var text=ui.Label(viewport,"GuidanceText",body,new Vector2(0,1),Vector2.one,21,false);text.color=DesignKitArt.Ink;text.alignment=TextAnchor.UpperLeft;text.rectTransform.pivot=new Vector2(.5f,1);
            text.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();scroll.viewport=viewport;scroll.content=text.rectTransform;scroll.horizontal=false;scroll.movementType=UnityEngine.UI.ScrollRect.MovementType.Clamped;
            var target=game.Quests.GuidanceTarget;
            if(target>=0)MobileUI.Button(ui,sheet,"GuidanceLocate","Xem địa điểm trên bản đồ",new Vector2(.12f,.32f),new Vector2(.88f,.40f),()=>{Close();Object.FindAnyObjectByType<CityMap>()?.FocusObjective();},true,true);
            else if(game.Quests.GuidanceCity!=null)MobileUI.Button(ui,sheet,"GuidanceTravel","Xem tuyến ở Thế giới",new Vector2(.12f,.32f),new Vector2(.88f,.40f),()=>{var city=game.Quests.GuidanceCity;Close();game.Scenes.OpenWorld(city);},true,true);
            if(level==0)MobileUI.Button(ui,sheet,"GuidanceMore","Xem manh mối đã biết",new Vector2(.12f,.21f),new Vector2(.88f,.30f),()=>ShowGuidance(1),true,true);
        }
    }
}
