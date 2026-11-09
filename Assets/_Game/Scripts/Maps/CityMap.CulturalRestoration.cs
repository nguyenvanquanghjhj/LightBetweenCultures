using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        RectTransform culturalLayer,decorativeFog;
        CulturalRelic culturalRelic;
        bool culturalKnown,culturalRestored,culturalWaiting,culturalAnimating;
        float culturalAt;
        public bool CulturalRestorationAnimating=>culturalAnimating;
        public string CulturalRestorationState=>culturalRelic==null?"Inactive":culturalRestored?"C":"A";
        void RefreshCulturalRestoration()
        {
            culturalRelic=game.Relics.Catalog.relics.FirstOrDefault(r=>r.cityId==game.Data.cityId&&r.enabled);
            if(culturalRelic==null)return;
            var restored=game.Relics.Restored(culturalRelic.id);
            if(culturalLayer==null)
            {
                culturalLayer=ui.Panel(map,"CulturalRestorationLayer",Vector2.zero,Vector2.one,Color.clear);culturalLayer.SetSiblingIndex(1);
                culturalLayer.GetComponent<Image>().raycastTarget=false;var group=culturalLayer.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
                if(culturalRelic.trialType=="mist")
                {
                    decorativeFog=ui.Panel(map,"RelicCosmeticMist",Vector2.zero,Vector2.one,Color.clear);decorativeFog.SetSiblingIndex(1);decorativeFog.GetComponent<Image>().raycastTarget=false;
                    var fogGroup=decorativeFog.gameObject.AddComponent<CanvasGroup>();fogGroup.blocksRaycasts=false;fogGroup.interactable=false;
                }
                var tint=culturalRelic.trialType=="lens"?new Color(.52f,.70f,1,.30f):culturalRelic.trialType=="mist"?new Color(.62f,1,.8f,.26f):new Color(1,.81f,.35f,.28f);
                foreach(var nodeId in culturalRelic.lightNodes)
                {
                    var index=Array.FindIndex(game.Data.nodes,n=>n.id==nodeId);if(index<0)continue;var p=game.Data.nodes[index].position;
                    var glow=ui.Picture(culturalLayer,"CulturalLight_"+nodeId,PresentationShapes.WarmGlow,p,p);glow.raycastTarget=false;glow.color=tint;
                    if(culturalRelic.trialType=="lens"&&nodeId=="G7")glow.color=game.World.Has("gyeongju.stone_guard_friend")?new Color(.46f,1,.81f,.30f):new Color(1,.69f,.36f,.30f);
                    var motif=ui.Picture(glow.transform,"CulturalMotif",presentation.Relic(culturalRelic.id),new Vector2(.27f,.27f),new Vector2(.73f,.73f));motif.raycastTarget=false;motif.useSpriteMesh=true;motif.color=new Color(1,1,1,.80f);motif.enabled=nodeId==culturalRelic.restorationNode;
                    var signal=(culturalRelic.signals??Array.Empty<RelicSignal>()).FirstOrDefault(s=>s.nodeId==nodeId);
                    if(signal!=null){motif.sprite=presentation.Relic(signal.relicId);motif.enabled=true;}
                    if(culturalRelic.trialType=="hanji")
                    {
                        for(var i=0;i<3;i++){var ribbon=ui.Panel(glow.transform,"PaperRibbon"+i,new Vector2(.44f+i*.05f,.36f),new Vector2(.455f+i*.05f,.47f),new Color(1,.96f,.78f,.90f));ribbon.localRotation=Quaternion.Euler(0,0,(i-1)*12);ribbon.GetComponent<Image>().raycastTarget=false;}
                    }
                    if(culturalRelic.trialType=="lens")
                    {
                        for(var i=0;i<5;i++){var angle=i*Mathf.PI*2/5;var star=ui.Picture(glow.transform,"MemoryStar"+i,PresentationShapes.WarmGlow,new Vector2(.5f+Mathf.Cos(angle)*.37f,.5f+Mathf.Sin(angle)*.37f),new Vector2(.5f+Mathf.Cos(angle)*.37f,.5f+Mathf.Sin(angle)*.37f));star.rectTransform.sizeDelta=new Vector2(10,10);star.raycastTarget=false;star.color=new Color(.8f,.92f,1,.60f);}
                    }
                    if(decorativeFog!=null)
                    {
                        var cloud=ui.Picture(decorativeFog,"CosmeticFog_"+nodeId,PresentationShapes.WarmGlow,p,p);cloud.raycastTarget=false;cloud.color=new Color(.86f,.93f,.96f,.55f);
                    }
                }
            }
            if(culturalKnown&&!culturalRestored&&restored){RelicCelebrations++;culturalAnimating=true;culturalWaiting=dialogue;culturalAt=Time.unscaledTime;RequestSignature(culturalRelic);}
            culturalKnown=true;culturalRestored=restored;relicLightRestored=restored;
            culturalLayer.gameObject.SetActive(restored);culturalLayer.GetComponent<CanvasGroup>().alpha=culturalAnimating?0:restored?1:0;
            foreach(Transform t in culturalLayer)
            {
                var node=t.name.Substring("CulturalLight_".Length);var index=Array.FindIndex(game.Data.nodes,n=>n.id==node);
                t.gameObject.SetActive(index>=0&&game.Discovery.IsVisible(index));((RectTransform)t).sizeDelta=new Vector2(map.rect.width*.12f,map.rect.height*.13f);
                if(culturalRelic.trialType=="lens"&&node=="G7")t.GetComponent<Image>().color=game.World.Has("gyeongju.stone_guard_friend")?new Color(.46f,1,.81f,.30f):new Color(1,.69f,.36f,.30f);
            }
            if(decorativeFog!=null)
            {
                decorativeFog.gameObject.SetActive(!restored||culturalAnimating);
                foreach(Transform t in decorativeFog)
                {var node=t.name.Substring("CosmeticFog_".Length);var index=Array.FindIndex(game.Data.nodes,n=>n.id==node);t.gameObject.SetActive(index>=0&&game.Discovery.IsVisible(index));((RectTransform)t).sizeDelta=new Vector2(map.rect.width*.35f,map.rect.height*.24f);}
            }
        }
        void AnimateCulturalRestoration()
        {
            if(!culturalAnimating||culturalLayer==null)return;
            if(culturalWaiting){if(dialogue)return;culturalWaiting=false;culturalAt=Time.unscaledTime;}
            var age=Mathf.Clamp01((Time.unscaledTime-culturalAt)/1.8f);var alpha=age*age*(3-2*age);
            culturalLayer.GetComponent<CanvasGroup>().alpha=alpha;
            if(decorativeFog!=null)decorativeFog.GetComponent<CanvasGroup>().alpha=1-alpha;
            if(age>=1){culturalAnimating=false;if(decorativeFog!=null)decorativeFog.gameObject.SetActive(false);}
        }
    }
}
