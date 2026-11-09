using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        readonly Dictionary<LeisureDefinition,Image> leisureProps=new Dictionary<LeisureDefinition,Image>();
        void CreateLeisureDecorations()
        {
            var layer=new GameObject("LeisureDecorationLayer",typeof(RectTransform)).GetComponent<RectTransform>();layer.SetParent(map,false);UIController.SetRect(layer,Vector2.zero,Vector2.one);layer.SetSiblingIndex(1);
            foreach(var entry in LeisurePresentation.Load().entries.Where(e=>e.enabled&&e.cityId==game.Data.cityId))
            {
                var node=System.Array.FindIndex(game.Data.nodes,n=>n.id==entry.nodeId);if(node<0)continue;
                var at=entry.hasMapPosition?entry.mapPosition:CityRouteGeometry.Arrival(game.Data.nodes[node])+entry.offset;
                var image=ui.Picture(layer,"LeisureProp_"+entry.id,entry.Sprite,at,at);image.rectTransform.pivot=new Vector2(.5f,0);image.rectTransform.sizeDelta=entry.mapSize;
                image.useSpriteMesh=true;image.raycastTarget=false;leisureProps.Add(entry,image);
            }
        }
        void RefreshLeisureDecorations()
        {
            foreach(var pair in leisureProps)
            {
                var index=System.Array.FindIndex(game.Data.nodes,n=>n.id==pair.Key.nodeId);
                pair.Value.gameObject.SetActive(index>=0&&game.Discovery.IsVisible(index));
                if(index>=0&&index<nodeMarkers.Count)nodeMarkers[index].AddLeisureBadge(ui);
            }
        }
    }
}
