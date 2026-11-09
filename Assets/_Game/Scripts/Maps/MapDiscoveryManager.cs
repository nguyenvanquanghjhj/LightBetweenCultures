using System.Linq;

namespace LightBetweenCultures
{
    public enum MapNodeState { HIDDEN, VISIBLE_LOCKED, REVEALED }
    public enum MapDiscoveryState { HIDDEN, PREVIEW, REVEALED }

    // One progression model for every city. Rendering, input and routing consult the same predicate.
    public sealed class MapDiscoveryManager
    {
        private readonly GameBootstrap game;
        public MapDiscoveryManager(GameBootstrap owner) { game=owner; }
        public int Stage(string cityId) => game.Save.cityProgress.FirstOrDefault(p=>p.cityId==cityId)?.stage??0;
        public bool IsVisible(int index) => index>=0 && index<game.Data.nodes.Length && Visible(game.Data,game.Data.nodes[index],game.Save);
        // IsVisible is the existing gameplay predicate: only fully revealed nodes.
        public MapDiscoveryState DiscoveryState(int index) => index<0||index>=game.Data.nodes.Length ? MapDiscoveryState.HIDDEN : DiscoveryState(game.Data,game.Data.nodes[index],game.Save);
        public static MapDiscoveryState DiscoveryState(GameData map,NodeData node,SaveData save)
        {
            if(!RuntimeAvailable(map,node))return MapDiscoveryState.HIDDEN;
            if(Visible(map,node,save))return MapDiscoveryState.REVEALED;
            var stage=save.cityProgress.FirstOrDefault(p=>p.cityId==map.cityId)?.stage??0;
            // Explicit author opt-in shows only an anonymous frontier ring. Entry flags
            // still gate Visible/input/events; secret nodes omit previewStages entirely.
            return node.revealStage>stage && node.previewStages!=null && node.previewStages.Contains(stage)
                ? MapDiscoveryState.PREVIEW : MapDiscoveryState.HIDDEN;
        }
        public MapNodeState State(int index) => index<0 || index>=game.Data.nodes.Length ? MapNodeState.HIDDEN : State(game.Data,game.Data.nodes[index],game.Save);
        public static MapNodeState State(GameData map,NodeData node,SaveData save)
        {
            if(!Visible(map,node,save)) return MapNodeState.HIDDEN;
            var gateBlocked=node.isKeyGate && !string.IsNullOrEmpty(node.gateCondition) && !save.flags.Contains(node.gateCondition);
            return gateBlocked || !map.Unlocked(node.requirement,save) ? MapNodeState.VISIBLE_LOCKED : MapNodeState.REVEALED;
        }
        public static bool Visible(GameData map,NodeData node,SaveData save)
        {
            if(!RuntimeAvailable(map,node))return false;
            var stage=save.cityProgress.FirstOrDefault(p=>p.cityId==map.cityId)?.stage??0;
            var history=map.cityId==save.cityId?save.visited:save.cityProgress.FirstOrDefault(p=>p.cityId==map.cityId)?.visited;
            var retained=node.revealWhenPreviouslyVisited&&history!=null&&history.Contains(System.Array.IndexOf(map.nodes,node));
            return (node.revealStage<=stage||retained) && (node.requiredFlags==null || node.requiredFlags.All(save.flags.Contains));
        }
        private static bool RuntimeAvailable(GameData map,NodeData node)=>map.runtimeNodeIds==null||map.runtimeNodeIds.Length==0||map.runtimeNodeIds.Contains(node.id);
        public bool Refresh()
        {
            var changed=false;
            foreach(var city in game.Content.cities)
            {
                var progress=game.Save.cityProgress.FirstOrDefault(p=>p.cityId==city.id);
                if(progress==null) { progress=new CityProgress { cityId=city.id }; game.Save.cityProgress.Add(progress); }
                // Only contiguous gates may advance discovery. A later world flag alone
                // must not expose earlier, unfinished regions. Keep already saved progress.
                foreach(var gate in city.gates.OrderBy(g=>g.unlockStage))
                {
                    if(gate.unlockStage<=progress.stage) continue;
                    if(gate.unlockStage!=progress.stage+1 || !game.World.Has(gate.condition)) break;
                    progress.stage=gate.unlockStage; changed=true;
                }
            }
            return changed;
        }
    }
}
