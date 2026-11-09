using UnityEngine;
namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        void CreateJourneyShop()
        {
            var button=MobileUI.IconButton(ui,ui.Root,"JourneyShop","Cửa hàng",Vector2.zero,Vector2.zero,()=>
            {
                if(dialogue||Moving)return;
                Deselect();dialogue=true;Refresh();panels.ShowJourneyShop();
            },AdventureSymbol.NeedItem,AdventureUIArt.Load().coin);
            var rect=(RectTransform)button.transform;
            // 100 instead of 138 UI units (-27.5%); retain the shared 52-unit
            // toolbar height and an 8-unit gap after ObjectiveHint. The whole
            // button remains the touch target; callback and glossy skin are shared.
            rect.offsetMin=new Vector2(134,91);rect.offsetMax=new Vector2(234,143);
            button.transform.Find("Label").GetComponent<UnityEngine.UI.Text>().alignment=TextAnchor.MiddleCenter;
            mapControls.Add(button);
        }
    }
}
