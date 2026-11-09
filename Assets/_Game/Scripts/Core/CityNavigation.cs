using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public sealed class NavigationAccess
    { public int stage; public string[] flags=Array.Empty<string>(); }
    [Serializable] public sealed class NavigationVertex
    { public string id,canonicalNodeId;public Vector2 position;public bool transfer; }
    [Serializable] public sealed class NavigationEdge
    {
        public string id,from,to,travel;public bool bidirectional=true;
        public Vector2[] points=Array.Empty<Vector2>();public string[] requiredFlags=Array.Empty<string>(),source=Array.Empty<string>();
        public NavigationAccess[] access=Array.Empty<NavigationAccess>();
    }
    [Serializable] public sealed class NavigationNode
    {public string nodeId,vertexId;public NavigationAccess[] access=Array.Empty<NavigationAccess>();}
    public sealed class NavigationLeg
    {public NavigationEdge edge;public bool reverse;public string From=>reverse?edge.to:edge.from;public string To=>reverse?edge.from:edge.to;public List<Vector2> Points=>CityRouteGeometry.Points(edge,reverse);}
    public sealed class NavigationJourney
    {
        public readonly List<NavigationLeg> legs=new List<NavigationLeg>();
        public readonly List<int> canonicalStops=new List<int>();
        public float cost;
    }
    [Serializable] public sealed class CityNavigationGraph
    {
        public string cityId;public bool enabled;public float imageAspect=4f/3;
        public NavigationVertex[] vertices=Array.Empty<NavigationVertex>();
        public NavigationEdge[] edges=Array.Empty<NavigationEdge>();public NavigationNode[] nodes=Array.Empty<NavigationNode>();
        Dictionary<string,int> index;List<(int to,NavigationEdge edge,bool reverse)>[] adjacency;
        void Prepare()
        {
            if(index!=null)return;
            index=new Dictionary<string,int>();for(var i=0;i<vertices.Length;i++)index.Add(vertices[i].id,i);
            adjacency=new List<(int,NavigationEdge,bool)>[vertices.Length];for(var i=0;i<vertices.Length;i++)adjacency[i]=new List<(int,NavigationEdge,bool)>();
            foreach(var edge in edges)
            {
                var a=index[edge.from];var b=index[edge.to];adjacency[a].Add((b,edge,false));
                if(edge.bidirectional)adjacency[b].Add((a,edge,true));
            }
        }
        public bool AccessOpen(NavigationAccess[] rules,SaveData save)
        {
            if(rules==null||rules.Length==0)return true;
            var stage=save.cityProgress.FirstOrDefault(c=>c.cityId==cityId)?.stage??0;
            return rules.Any(r=>r.stage<=stage&&(r.flags==null||r.flags.All(save.flags.Contains)));
        }
        public bool EdgeOpen(NavigationEdge edge,SaveData save)=>AccessOpen(edge.access,save)&&(edge.requiredFlags==null||edge.requiredFlags.All(save.flags.Contains));
        public bool NodeOpen(GameData data,int canonical,SaveData save)
        {
            if(canonical<0||canonical>=data.nodes.Length||!MapDiscoveryManager.Visible(data,data.nodes[canonical],save)||!data.Unlocked(data.nodes[canonical].requirement,save))return false;
            var entry=Array.Find(nodes,n=>n.nodeId==data.nodes[canonical].id);
            return entry!=null&&AccessOpen(entry.access,save);
        }
        public NavigationVertex Vertex(string id){Prepare();return vertices[index[id]];}
        public int CanonicalIndex(GameData data,string vertexId)=>Array.FindIndex(data.nodes,n=>n.id==Vertex(vertexId).canonicalNodeId);
        public float Length(NavigationEdge edge)=>CityRouteGeometry.Length(new List<Vector2>(edge.points),new Vector2(1000,1000/imageAspect));
        // One Dijkstra core for authored junction graphs and legacy fallback.
        // Cost is actual image-proportional polyline length. Transfer animation
        // adds no distance penalty. No BFS, edge-count heuristic or saved nav IDs.
        public NavigationJourney Plan(GameData data,int start,int destination,SaveData save)
        {
            if(!NodeOpen(data,start,save)||!NodeOpen(data,destination,save))return null;
            var a=Array.Find(nodes,n=>n.nodeId==data.nodes[start].id);var b=Array.Find(nodes,n=>n.nodeId==data.nodes[destination].id);
            return PlanVertices(data,a.vertexId,b.vertexId,save);
        }
        public NavigationJourney PlanVertices(GameData data,string from,string to,SaveData save)
        {
            Prepare();if(!index.TryGetValue(from,out var start)||!index.TryGetValue(to,out var destination))return null;
            var costs=Enumerable.Repeat(float.PositiveInfinity,vertices.Length).ToArray();var done=new bool[vertices.Length];
            var previous=new int[vertices.Length];var via=new NavigationLeg[vertices.Length];costs[start]=0;
            for(var turn=0;turn<vertices.Length;turn++)
            {
                var current=-1;var best=float.PositiveInfinity;
                for(var i=0;i<costs.Length;i++)if(!done[i]&&costs[i]<best){current=i;best=costs[i];}
                if(current<0)break;if(current==destination)break;done[current]=true;
                foreach(var arc in adjacency[current])
                {
                    if(done[arc.to]||!EdgeOpen(arc.edge,save))continue;
                    var canonical=CanonicalIndex(data,vertices[arc.to].id);
                    if(canonical>=0&&!NodeOpen(data,canonical,save))continue;
                    var next=best+Mathf.Max(.0001f,Length(arc.edge));
                    if(next>=costs[arc.to]-.00001f)continue;
                    costs[arc.to]=next;previous[arc.to]=current;via[arc.to]=new NavigationLeg{edge=arc.edge,reverse=arc.reverse};
                }
            }
            if(float.IsPositiveInfinity(costs[destination]))return null;
            var result=new NavigationJourney{cost=costs[destination]};
            for(var v=destination;v!=start;v=previous[v])result.legs.Add(via[v]);result.legs.Reverse();
            var initial=CanonicalIndex(data,from);if(initial>=0)result.canonicalStops.Add(initial);
            foreach(var leg in result.legs){var node=CanonicalIndex(data,leg.To);if(node>=0&&(result.canonicalStops.Count==0||result.canonicalStops.Last()!=node))result.canonicalStops.Add(node);}
            return result;
        }
        public static CityNavigationGraph Legacy(GameData data)
        {
            var graph=new CityNavigationGraph{cityId=data.cityId,enabled=true};
            graph.vertices=data.nodes.Select(n=>new NavigationVertex{id=n.id,canonicalNodeId=n.id,position=CityRouteGeometry.Arrival(n)}).ToArray();
            graph.nodes=data.nodes.Select(n=>new NavigationNode{nodeId=n.id,vertexId=n.id}).ToArray();
            graph.edges=data.edges.Select((e,i)=>new NavigationEdge{id="legacy-"+i,from=data.nodes[e.from].id,to=data.nodes[e.to].id,bidirectional=e.bidirectional,travel=e.travel,
                requiredFlags=(e.requiredFlags??Array.Empty<string>()).Concat(string.IsNullOrEmpty(e.requirement)?Array.Empty<string>():new[]{e.requirement}).ToArray(),points=CityRouteGeometry.Points(data,e.from,e.to).ToArray()}).ToArray();return graph;
        }
    }
    [Serializable] public sealed class CityNavigationCatalog
    {
        public CityNavigationGraph[] cities=Array.Empty<CityNavigationGraph>();static CityNavigationCatalog cached;
        public static CityNavigationGraph Load(string city)
        {
            if(cached==null){var source=Resources.Load<TextAsset>("Content/CityNavigation");cached=source==null?new CityNavigationCatalog():JsonUtility.FromJson<CityNavigationCatalog>(source.text);}
            return Array.Find(cached.cities,c=>c.cityId==city&&c.enabled);
        }
    }
}
