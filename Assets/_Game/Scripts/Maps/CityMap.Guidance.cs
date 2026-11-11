using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        Text nextObjective;
        public void FocusObjective()
        {
            FocusLocation(game.Quests.GuidanceTarget);
        }
        public void FocusLocation(int target)
        {
            if(target<0||target>=game.Data.nodes.Length||!game.Discovery.IsVisible(target))return;
            SelectedNode=target;tooltipName.text=game.Data.nodes[target].name;tooltipState.text="Điểm đến của nhiệm vụ · Chạm để đi tới";
            View.ZoomBy(Mathf.Max(1,1.6f/View.Zoom));View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[target]));
            paths.Highlight(game.Data.FindPath(game.Save.node,target,game.Save));
        }
        void CreateGuidance()
        {
            View.Viewport.offsetMax=new Vector2(0,-91);
            nextObjective=ui.Label(ui.Root,"NextObjective","",new Vector2(0,1),Vector2.one,14,false);nextObjective.color=DesignKitArt.Teal;nextObjective.alignment=TextAnchor.MiddleLeft;
            nextObjective.rectTransform.offsetMin=new Vector2(64,-85);nextObjective.rectTransform.offsetMax=new Vector2(-14,-57);
            // Required primary task-view button: do not remove as a duplicate prompt.
            var hint=MobileUI.IconButton(ui,ui.Root,"ObjectiveHint","Việc cần làm",Vector2.zero,Vector2.zero,()=>{if(dialogue||Moving)return;dialogue=true;Refresh();panels.ShowGuidance();},AdventureSymbol.Journal);
            var rt=(RectTransform)hint.transform;rt.offsetMin=new Vector2(14,91);rt.offsetMax=new Vector2(126,143);mapControls.Add(hint);
        }
    }
}
