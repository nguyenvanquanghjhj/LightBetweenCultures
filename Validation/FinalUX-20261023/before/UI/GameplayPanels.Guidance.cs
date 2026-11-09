using UnityEngine;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        public void ShowGuidance(int level=0)
        {
            var sheet=ActivitySheet("GuidancePanel","Việc tiếp theo",PlayerCopy.CityIdentity(game.Data.cityId));
            ui.Label(sheet,"GuidanceText",game.Quests.Guidance(level),new Vector2(.09f,.45f),new Vector2(.91f,.73f),25).color=DesignKitArt.Ink;
            var target=game.Quests.GuidanceTarget;
            if(target>=0)MobileUI.Button(ui,sheet,"GuidanceLocate","Xem địa điểm trên bản đồ",new Vector2(.12f,.32f),new Vector2(.88f,.40f),()=>{Close();Object.FindAnyObjectByType<CityMap>()?.FocusObjective();},true,true);
            if(level<2)MobileUI.Button(ui,sheet,"GuidanceMore",level==0?"Xem khu vực":"Xem manh mối đã biết",new Vector2(.12f,.21f),new Vector2(.88f,.30f),()=>ShowGuidance(level+1),true,true);
        }
    }
}
