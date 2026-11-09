using System;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // A visual layer of the existing CityMap. Owns no progress, input or reward.
    public sealed partial class CityMap
    {
        [Serializable] sealed class QuestLightRules { public QuestLight[] questLights=Array.Empty<QuestLight>(); }
        [Serializable] sealed class QuestLight
        { public string cityId,nodeId,questId; public Vector2 position,radius; }
        QuestLight questLight;
        RectTransform questLightRoot;
        Image questDusk,questWarm,questLantern;
        bool lightKnown,lightCompleted;
        float questReactionAt=-100;
        public bool LocalQuestLightComplete=>lightCompleted;
        public int LocalQuestReactionBursts { get; private set; }

        void InitializeQuestLight()
        {
            var json=Resources.Load<TextAsset>("Content/AdventurePresentation");
            var rules=json!=null?JsonUtility.FromJson<QuestLightRules>(json.text):null;
            questLight=Array.Find(rules?.questLights??Array.Empty<QuestLight>(),q=>q.cityId==game.Data.cityId);
            if(questLight==null)return;
            var layer=ui.Panel(map,"QuestReactionLayer",Vector2.zero,Vector2.one,Color.clear);
            layer.GetComponent<Image>().raycastTarget=false;
            var group=layer.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
            layer.SetSiblingIndex(1); // Behind routes, nodes, actors and all input.
            questLightRoot=ui.Panel(layer,"QuestLight_"+questLight.nodeId,questLight.position,questLight.position,Color.clear);
            questLightRoot.GetComponent<Image>().raycastTarget=false;
            questDusk=ui.Picture(questLightRoot,"UnrestoredLocalDusk",PresentationShapes.WarmGlow,Vector2.zero,Vector2.one);
            questWarm=ui.Picture(questLightRoot,"RestoredLocalWarmth",PresentationShapes.WarmGlow,Vector2.zero,Vector2.one);
            questLantern=ui.Picture(questLightRoot,"MaiFirstLantern",presentation.Item("lamp_eye")?.sprite,new Vector2(.5f,.5f),new Vector2(.5f,.5f));
            foreach(var image in new[]{questDusk,questWarm,questLantern})image.raycastTarget=false;
            lightCompleted=game.Quests.IsCompleted(questLight.questId);lightKnown=true;
            RefreshQuestLight();
        }
        void RefreshQuestLight()
        {
            if(questLightRoot==null)return;
            var index=Array.FindIndex(game.Data.nodes,n=>n.id==questLight.nodeId);
            var visible=index>=0&&game.Discovery.IsVisible(index);
            questLightRoot.gameObject.SetActive(visible);
            var complete=game.Quests.IsCompleted(questLight.questId);
            if(lightKnown&&!lightCompleted&&complete)
            {questReactionAt=Time.unscaledTime;LocalQuestReactionBursts++;}
            lightCompleted=complete;
            questLightRoot.sizeDelta=Vector2.Scale(map.rect.size,questLight.radius)*2;
            questLantern.rectTransform.sizeDelta=new Vector2(map.rect.width*.015f,map.rect.width*.020f);
            questDusk.color=new Color(.04f,.08f,.18f,complete?0:.45f);
            questWarm.color=new Color(1,.73f,.25f,complete?.68f:0);
            questLantern.color=complete?Color.white:new Color(.35f,.38f,.48f,.70f);
        }
        void AnimateQuestReaction(Vector3 scale)
        {
            if(questLight==null)return;
            var mode=NodeMarkerView.MotionMode;
            var strength=mode==MarkerMotionMode.Off?0:mode==MarkerMotionMode.Reduced?.35f:1;
            var age=Time.unscaledTime-questReactionAt;
            var jump=age>=0&&age<1.15f?Mathf.Sin(age/1.15f*Mathf.PI):0;
            if(!Moving)
            {
                var breath=Mathf.Sin(Time.unscaledTime*1.8f)*.006f;
                Marker.localScale=scale*(1+strength*(breath+jump*.035f));
                Marker.localRotation=Quaternion.Euler(0,0,strength*jump*3);
                Marker.anchoredPosition=Vector2.up*(strength*jump*5/View.Zoom);
            }
            else Marker.localRotation=Quaternion.identity;
            if(lightCompleted)questWarm.color=new Color(1,.73f,.25f,.68f+strength*jump*.20f);
        }
        // City-wide relic illumination is deliberately NOT derived from HQ01 or
        // lantern-lit. The Six Relics task must supply its real completed-trial
        // state and own the city-wide visual. This layer stays local to H2.
    }
}
