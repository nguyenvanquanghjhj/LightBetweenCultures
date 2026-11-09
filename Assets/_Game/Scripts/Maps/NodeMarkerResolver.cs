using System;
using System.Linq;
using UnityEngine;

namespace LightBetweenCultures
{
    public enum NodeMarkerType { Normal, Camera, BossAlert, NpcTalk, NpcNeedItem, Puzzle, Reward, Completed, Quest, Portal, FragmentAlert }
    public enum NodeMarkerEffect { None, Blink, Pulse, Glow, Bounce }

    [Serializable] public sealed class NodeMarkerRule
    {
        public string marker, effect, questId, questState, eventState, optionId, optionState;
        public bool hide;
        public string[] requiredFlags = Array.Empty<string>(), forbiddenFlags = Array.Empty<string>();
        public ItemAmount[] requiredItems = Array.Empty<ItemAmount>();
    }
    [Serializable] public sealed class NodeMarkerDefinition
    {
        public string cityId, nodeId, marker, effect, offsetMode;
        public float size = 24;
        public Vector2 offset;
        public NodeMarkerRule[] rules = Array.Empty<NodeMarkerRule>();
    }
    public struct NodeMarkerDisplay
    {
        public NodeMarkerType type;
        public NodeMarkerEffect effect;
        public bool visible;
        public float size;
        public Vector2 offset;
        public bool customOffset;
        public MapDiscoveryState discovery;
        public bool keyGate,readyToTurnIn;
    }

    // Read-only presentation policy. It never accepts quests, grants items,
    // resolves events, opens gates or writes saves.
    public sealed class NodeMarkerResolver
    {
        [Serializable] private sealed class Rules { public NodeMarkerDefinition[] nodeMarkers = Array.Empty<NodeMarkerDefinition>(); }
        private readonly GameBootstrap game;
        private readonly NodeMarkerDefinition[] definitions;
        public NodeMarkerResolver(GameBootstrap owner)
        {
            game=owner;
            var json=Resources.Load<TextAsset>("Content/AdventurePresentation");
            definitions=json==null ? Array.Empty<NodeMarkerDefinition>() : JsonUtility.FromJson<Rules>(json.text).nodeMarkers??Array.Empty<NodeMarkerDefinition>();
        }
        public NodeMarkerDisplay Resolve(int index)
        {
            var discovery=game.Discovery.DiscoveryState(index);
            if(discovery!=MapDiscoveryState.REVEALED)
                return new NodeMarkerDisplay {visible=discovery==MapDiscoveryState.PREVIEW,discovery=discovery,type=NodeMarkerType.Normal,size=24};
            var node=game.Data.nodes[index];
            var evt=game.Events.Definition(node.eventId);
            var resolved=evt!=null&&game.Events.Resolved(evt);
            var options=game.Events.BuildOptions(index);
            var result=Default(node,evt,resolved,options);
            result.discovery=discovery;result.keyGate=node.isKeyGate;
            result.readyToTurnIn=Enabled(options,"complete")||Enabled(options,"tutorial_return")||options.options.Any(o=>o.id.StartsWith("complete:")&&o.enabled);
            var definition=definitions.FirstOrDefault(d=>d.cityId==game.Data.cityId&&d.nodeId==node.id);
            if(definition!=null)
            {
                result.type=Parse(definition.marker,result.type);result.effect=Parse(definition.effect,result.effect);
                result.size=Mathf.Clamp(definition.size>0?definition.size:24,18,30);
                result.offset=definition.offset;result.customOffset=definition.offsetMode=="custom";
                foreach(var rule in definition.rules??Array.Empty<NodeMarkerRule>())
                {
                    if(!Matches(rule,evt,resolved,options)) continue;
                    result.type=Parse(rule.marker,result.type);result.effect=Parse(rule.effect,NodeMarkerEffect.None);
                    result.visible=!rule.hide;break;
                }
            }
            var checkIn=game.Content.CheckIn(game.Data.cityId,node.id);
            if(result.type==NodeMarkerType.Normal&&checkIn!=null&&game.Events.CanCheckIn(index))result.type=NodeMarkerType.Camera;
            if(result.type==NodeMarkerType.Camera&&checkIn!=null&&game.World.Has(checkIn.completionFlag))
            {result.type=NodeMarkerType.Completed;result.effect=NodeMarkerEffect.None;}
            if(RelicGuide.PendingAt(game,node.id))
            {result.type=NodeMarkerType.FragmentAlert;result.effect=NodeMarkerEffect.Pulse;result.visible=true;result.readyToTurnIn=false;}
            return result;
        }
        private NodeMarkerDisplay Default(NodeData node,EventDefinition evt,bool resolved,EventView view)
        {
            var result=new NodeMarkerDisplay { visible=true,size=24,type=NodeMarkerType.Normal };
            if(resolved){result.type=NodeMarkerType.Completed;return result;}
            if(node.kind=="checkin"||node.kind=="landmark"){result.type=NodeMarkerType.Camera;return result;}
            if(node.kind=="boss"||evt?.type=="encounter")
            {result.type=NodeMarkerType.BossAlert;result.effect=NodeMarkerEffect.Pulse;return result;}
            switch(evt?.type)
            {
                case "npc": case "dialogue":
                    if(Enabled(view,"complete")||Enabled(view,"tutorial_return"))
                    {result.type=NodeMarkerType.NpcTalk;result.effect=NodeMarkerEffect.Bounce;}
                    else if(Enabled(view,"accept")||string.IsNullOrEmpty(evt.questId))
                    {result.type=NodeMarkerType.NpcTalk;result.effect=NodeMarkerEffect.Pulse;}
                    else if(game.Quests.IsActive(evt.questId)&&!game.Quests.IsCompleted(evt.questId)&&game.Content.Quest(evt.questId)?.completeAtNode==node.id)
                    {result.type=NodeMarkerType.NpcNeedItem;result.effect=NodeMarkerEffect.Glow;}
                    break;
                case "puzzle":result.type=NodeMarkerType.Puzzle;result.effect=Enabled(view,"puzzle")?NodeMarkerEffect.Glow:NodeMarkerEffect.None;break;
                case "item": case "chest": case "secret":result.type=NodeMarkerType.Reward;result.effect=Enabled(view,"collect")?NodeMarkerEffect.Glow:NodeMarkerEffect.None;break;
                case "beacon":result.type=NodeMarkerType.Quest;result.effect=Enabled(view,"restore")?NodeMarkerEffect.Glow:NodeMarkerEffect.None;break;
                case "portal":result.type=NodeMarkerType.Portal;break;
            }
            return result;
        }
        private bool Matches(NodeMarkerRule rule,EventDefinition evt,bool resolved,EventView view)
        {
            if(!(rule.requiredFlags??Array.Empty<string>()).All(game.World.Has) || (rule.forbiddenFlags??Array.Empty<string>()).Any(game.World.Has))return false;
            if(!(rule.requiredItems??Array.Empty<ItemAmount>()).All(i=>game.Inventory.Count(i.itemId)>=i.amount))return false;
            var quest=string.IsNullOrEmpty(rule.questId)?evt?.questId:rule.questId;
            switch(rule.questState)
            {
                case "offer":if(string.IsNullOrEmpty(quest)||!game.Quests.CanAccept(quest))return false;break;
                case "active":if(string.IsNullOrEmpty(quest)||!game.Quests.IsActive(quest)||game.Quests.IsCompleted(quest))return false;break;
                case "completed":if(string.IsNullOrEmpty(quest)||!game.Quests.IsCompleted(quest))return false;break;
                case "ready":if(string.IsNullOrEmpty(quest)||!game.Quests.CanComplete(quest))return false;break;
                case "notReady":if(string.IsNullOrEmpty(quest)||!game.Quests.IsActive(quest)||game.Quests.IsCompleted(quest)||game.Quests.CanComplete(quest))return false;break;
                case null:case "":break;
                default:return false;
            }
            switch(rule.eventState)
            {
                case "resolved":if(!resolved)return false;break;
                case "unresolved":if(resolved)return false;break;
                case "usable":if(evt==null||!game.Events.CanUse(evt))return false;break;
                case "unusable":if(evt==null||game.Events.CanUse(evt))return false;break;
                case null:case "":break;
                default:return false;
            }
            if(!string.IsNullOrEmpty(rule.optionId))
            {
                var option=view.options.Find(o=>o.id==rule.optionId);
                if(option==null || option.enabled!=(rule.optionState!="disabled"))return false;
            }
            return true;
        }
        private static bool Enabled(EventView view,string id)=>view.options.Any(o=>o.id==id&&o.enabled);
        private static T Parse<T>(string value,T fallback) where T:struct=>Enum.TryParse<T>(value,true,out var result)&&Enum.IsDefined(typeof(T),result)?result:fallback;
    }
}
