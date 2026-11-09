using System.Collections.Generic;
using UnityEngine;

namespace LightBetweenCultures
{
    public static class CityRouteGeometry
    {
        public static Vector2 Arrival(NodeData node)=>node.hasArrivalPosition?node.arrivalPosition:node.position;

        public static bool IsCovered(GameData data,Vector2 point)
        {
            foreach(var cover in data.coveredWalkways)
            {
                var polygon=cover.polygon;if(polygon==null||polygon.Length<3)continue;
                var inside=false;
                for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)
                    if((polygon[i].y>point.y)!=(polygon[j].y>point.y)&&
                        point.x<(polygon[j].x-polygon[i].x)*(point.y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)inside=!inside;
                if(inside)return true;
            }
            return false;
        }
        public static string Travel(GameData data,int from,int to)
        {
            foreach(var edge in data.edges)
                if(edge.from==from&&edge.to==to||edge.from==to&&edge.to==from)return edge.travel=="ferry"?"ferry":"walk";
            return "walk";
        }
        public static List<Vector2> Points(GameData data, int from, int to)
        {
            foreach (var edge in data.edges)
            {
                if (!(edge.from == from && edge.to == to || edge.to == from && edge.from == to)) continue;
                var points = new List<Vector2> { Arrival(data.nodes[edge.from]) };
                if (edge.bends != null) points.AddRange(edge.bends);
                points.Add(Arrival(data.nodes[edge.to]));
                if (edge.from != from) points.Reverse();
                return points;
            }
            return null;
        }
        public static float Length(List<Vector2> points, Vector2 size)
        {
            float length = 0;
            for (var i = 1; i < points.Count; i++) length += Vector2.Scale(points[i] - points[i-1], size).magnitude;
            return length;
        }
        public static Vector2 Sample(List<Vector2> points, Vector2 size, float progress)
        {
            var remaining = Mathf.Clamp01(progress) * Length(points, size);
            for (var i = 1; i < points.Count; i++)
            {
                var length = Vector2.Scale(points[i] - points[i-1], size).magnitude;
                if (remaining <= length && length > .0001f) return Vector2.Lerp(points[i-1], points[i], remaining / length);
                remaining -= length;
            }
            return points[points.Count-1];
        }
    }
}
