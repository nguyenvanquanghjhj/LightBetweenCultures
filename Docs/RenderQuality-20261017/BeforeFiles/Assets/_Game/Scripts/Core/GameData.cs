using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public sealed class CityData { public string id; public string name; public bool unlocked; }
    [Serializable] public sealed class NodeData
    {
        public string id;
        public string name;
        public Vector2 position;
        public bool hasArrivalPosition;
        public Vector2 arrivalPosition;
        public string requirement;
        public string kind;
        public string preview;
        public string eventId;
        // Empty uses the current city background; a future node scene can resolve this ID.
        public string backgroundId;
        public int revealStage,unlockStage;
        public int[] previewStages = Array.Empty<int>();
        public bool isKeyGate;
        public bool revealWhenPreviouslyVisited;
        public string gateCondition;
        public string[] requiredFlags = new string[0];
    }
    [Serializable] public sealed class EdgeData { public int from; public int to; public string requirement, travel; public Vector2[] bends; public string[] requiredFlags=new string[0]; }
    [Serializable] public sealed class QuestData { public string id; public string item; public string reward; public string offer; public string reminder; public string thanks; }
    [Serializable] public sealed class GameData
    {
        public string cityId = "hoi_an";
        public string[] runtimeNodeIds=Array.Empty<string>();
        public string spawnNodeId;
        public int SpawnIndex => Mathf.Max(0,Array.FindIndex(nodes,n=>n.id==spawnNodeId));
        public CityData[] cities;
        public NodeData[] nodes;
        public EdgeData[] edges;
        public CoveredWalkwayDefinition[] coveredWalkways = Array.Empty<CoveredWalkwayDefinition>();
        public QuestData quest;
        public static GameData Load(string cityId="hoi_an")
        {
            var content=ContentDatabase.Load(); var definition=content.City(cityId);
            if(definition==null) throw new System.InvalidOperationException("Unknown city "+cityId);
            GameData data;
            if(cityId=="hoi_an")
            {
                data=JsonUtility.FromJson<GameData>(Resources.Load<TextAsset>("HoiAnData").text);
                // Keep reviewed legacy positions and save indices stable, then append
                // new IDs supplied by the catalog. No fixed H0-H21 render limit.
                var existing=data.nodes.Select(n=>n.id).ToHashSet();
                data.nodes=data.nodes.Concat(definition.nodes.Where(n=>!existing.Contains(n.id)).Select(n=>new NodeData{id=n.id,name=n.name,position=n.position,kind=n.kind,eventId=n.eventId,backgroundId=n.backgroundId})).ToArray();
            }
            else
            {
                data=new GameData();
                data.nodes=definition.nodes.Select(n=>new NodeData{id=n.id,name=n.name,position=n.position,kind=n.kind,eventId=n.eventId,backgroundId=n.backgroundId}).ToArray();
                data.quest=JsonUtility.FromJson<GameData>(Resources.Load<TextAsset>("HoiAnData").text).quest;
            }
            data.cityId=cityId;
            data.runtimeNodeIds=definition.runtimeNodeIds;data.spawnNodeId=definition.spawnNodeId;
            data.cities=content.cities.Select(c=>new CityData{id=c.id,name=c.name,unlocked=c.id=="hoi_an"}).ToArray();
            foreach(var node in data.nodes)
            {
                var record=definition.nodes.First(n=>n.id==node.id);
                node.hasArrivalPosition=record.hasArrivalPosition;node.arrivalPosition=record.arrivalPosition;
                node.revealStage=record.revealStage; node.isKeyGate=record.isKeyGate; node.gateCondition=record.gateCondition;
                node.unlockStage=record.unlockStage; node.requiredFlags=record.requiredFlags; node.eventId=record.eventId;
                node.previewStages=record.previewStages;
                node.revealWhenPreviouslyVisited=record.revealWhenPreviouslyVisited;
                node.requirement="";
            }
            // The catalog is authoritative for topology AND geometry. Merging the
            // old HoiAnData edges silently resurrected deliberately removed routes.
            // Legacy node positions/indices and tutorial data above remain stable.
            data.edges=definition.edges.Select(e=>new EdgeData {
                from=Array.FindIndex(data.nodes,n=>n.id==e.from),
                to=Array.FindIndex(data.nodes,n=>n.id==e.to),
                bends=e.waypoints,requiredFlags=e.requiredFlags,travel=e.travel
            }).ToArray();
            data.coveredWalkways=definition.coveredWalkways??Array.Empty<CoveredWalkwayDefinition>();
            return data;
        }
        public bool Unlocked(string requirement, SaveData save) => string.IsNullOrEmpty(requirement) || save.flags.Contains(requirement);
        public bool EdgeOpen(EdgeData edge,SaveData save) => Unlocked(edge.requirement,save) && (edge.requiredFlags==null || edge.requiredFlags.All(save.flags.Contains));
        public List<int> FindPath(int start, int destination, SaveData save)
        {
            if (start < 0 || destination < 0 || start >= nodes.Length || destination >= nodes.Length || !Unlocked(nodes[destination].requirement, save) || !MapDiscoveryManager.Visible(this,nodes[destination],save) || !MapDiscoveryManager.Visible(this,nodes[start],save)) return null;
            var previous = new Dictionary<int, int> { [start] = -1 };
            var queue = new Queue<int>(); queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var edge in edges)
                {
                    var next = edge.from == current ? edge.to : edge.to == current ? edge.from : -1;
                    if (next < 0 || next >= nodes.Length || previous.ContainsKey(next) || !EdgeOpen(edge, save) || !Unlocked(nodes[next].requirement, save) || !MapDiscoveryManager.Visible(this,nodes[next],save)) continue;
                    previous[next] = current; queue.Enqueue(next);
                }
            }
            if (!previous.ContainsKey(destination)) return null;
            var path = new List<int>();
            for (var node = destination; node >= 0; node = previous[node]) path.Add(node);
            path.Reverse(); return path;
        }
    }
}
