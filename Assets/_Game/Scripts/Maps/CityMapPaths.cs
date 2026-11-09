using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // One batched mesh, no per-dot GameObjects. Graph and movement remain owned by GameData.
    public sealed class CityMapPaths : MonoBehaviour
    {
        GameBootstrap game; RectTransform map; RouteDotsGraphic graphic;
        CityNavigationGraph graph;
        readonly HashSet<NavigationEdge> highlighted=new HashSet<NavigationEdge>();
        readonly Dictionary<NavigationEdge,float> revealed=new Dictionary<NavigationEdge,float>();
        readonly List<Vector2> samples=new List<Vector2>();
        readonly List<NavigationEdge> owners=new List<NavigationEdge>();
        readonly List<Vector2> vertices=new List<Vector2>();
        readonly List<Color> colors=new List<Color>();
        readonly List<float> radii=new List<float>();
        readonly Dictionary<Vector2Int,List<Vector2>> occupied=new Dictionary<Vector2Int,List<Vector2>>();
        Vector2 previousSize;float previousScale,animateUntil;bool initialized;int lastSignature;
        public int DrawnStrokeCount=>samples.Count;
        public IReadOnlyList<Vector2> DotSamples=>samples;
        public IReadOnlyList<NavigationEdge> DotEdges=>owners;
        public CityNavigationGraph Graph=>graph;
        public void Configure(GameBootstrap owner,UIController ui,RectTransform content)
        {
            game=owner;map=content;graph=game.Data.navigation??CityNavigationGraph.Legacy(game.Data);
            graphic=new GameObject("RouteDots",typeof(RectTransform),typeof(CanvasRenderer)).AddComponent<RouteDotsGraphic>();
            var r=graphic.rectTransform;r.SetParent(map,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.pivot=Vector2.zero;r.offsetMin=r.offsetMax=Vector2.zero;
            graphic.raycastTarget=false;Refresh();
        }
        public void Highlight(IList<int> route)
        {
            highlighted.Clear();
            if(route!=null&&route.Count>1)
            {
                var journey=game.Data.FindRoute(route[0],route[route.Count-1],game.Save);
                if(journey!=null)foreach(var leg in journey.legs)highlighted.Add(leg.edge);
            }
            Refresh();
        }
        public void HighlightJourney(NavigationJourney journey)
        {highlighted.Clear();if(journey!=null)foreach(var leg in journey.legs)highlighted.Add(leg.edge);Refresh();}
        bool Visible(NavigationEdge e)
        {
            if(!graph.EdgeOpen(e,game.Save))return false;
            var a=graph.CanonicalIndex(game.Data,e.from);var b=graph.CanonicalIndex(game.Data,e.to);
            return (a<0||graph.NodeOpen(game.Data,a,game.Save))&&(b<0||graph.NodeOpen(game.Data,b,game.Save));
        }
        public void Refresh()
        {
            if(map==null)return;
            var signature=game.Save.node+17;
            foreach(var e in graph.edges)
            {
                signature=unchecked(signature*31+(Visible(e)?1:0)+(highlighted.Contains(e)?2:0));
                if(!Visible(e)){revealed.Remove(e);continue;}
                if(!revealed.ContainsKey(e)){revealed[e]=initialized?Time.unscaledTime:Time.unscaledTime-1;animateUntil=Time.unscaledTime+.55f;}
            }
            if(!initialized||signature!=lastSignature||previousSize!=map.rect.size){lastSignature=signature;Rebuild();}
            initialized=true;
        }
        void LateUpdate()
        {
            if(map!=null&&(previousSize!=map.rect.size||Mathf.Abs(previousScale-map.lossyScale.x)>.001f||Time.unscaledTime<animateUntil))Rebuild();
        }
        bool Occupy(Vector2 p,float cell)
        {
            var k=new Vector2Int(Mathf.FloorToInt(p.x/cell),Mathf.FloorToInt(p.y/cell));
            for(var x=-1;x<=1;x++)for(var y=-1;y<=1;y++)if(occupied.TryGetValue(k+new Vector2Int(x,y),out var list))
                foreach(var q in list)if((p-q).sqrMagnitude<cell*cell)return false;
            if(!occupied.TryGetValue(k,out var bucket)){bucket=new List<Vector2>();occupied[k]=bucket;}bucket.Add(p);return true;
        }
        void Rebuild()
        {
            previousSize=map.rect.size;previousScale=map.lossyScale.x;
            samples.Clear();owners.Clear();vertices.Clear();colors.Clear();radii.Clear();occupied.Clear();
            var style=game.Data.routeStyle??new RouteStyleDefinition();var scale=Mathf.Max(.01f,previousScale);
            ColorUtility.TryParseHtmlString(style.groundColor,out var ground);
            // Draw selected paths first so an overlapping idle path can never dim them.
            var nearbyVertices=new HashSet<string>();
            foreach(var entry in graph.nodes)if(entry.nodeId==game.Data.nodes[game.Save.node].id)nearbyVertices.Add(entry.vertexId);
            foreach(var e in graph.edges)if(nearbyVertices.Contains(e.from)||nearbyVertices.Contains(e.to)){nearbyVertices.Add(e.from);nearbyVertices.Add(e.to);break;}
            for(var pass=0;pass<2;pass++)foreach(var e in graph.edges)
            {
                var selected=highlighted.Contains(e);if(selected!=(pass==0)||!Visible(e))continue;
                if(!selected&&style.adjacentOnly&&!nearbyVertices.Contains(e.from)&&!nearbyVertices.Contains(e.to))continue;
                var points=CityRouteGeometry.Points(e);if(points.Count<2)continue;
                var length=CityRouteGeometry.Length(points,previousSize);var gap=(selected?style.selectedGapPixels:style.gapPixels)/scale;
                var pad=style.endPaddingPixels/scale;var order=0;
                for(var distance=pad;distance<length-pad&&samples.Count<style.maxDots;)
                {
                    var p=CityRouteGeometry.Sample(points,previousSize,distance/length);var local=Vector2.Scale(p,previousSize);
                    if(!CityRouteGeometry.IsCovered(game.Data,p)&&Occupy(local,style.gapPixels*.48f/scale))
                    {
                        var alpha=selected?style.selectedAlpha:style.idleAlpha;
                        if(NodeMarkerView.MotionMode!=MarkerMotionMode.Off&&revealed.TryGetValue(e,out var time))alpha*=Mathf.Clamp01((Time.unscaledTime-time)/.45f);
                        var radius=(selected?style.selectedRadiusPixels:style.radiusPixels)/scale*(1+(order%3-1)*.055f);
                        samples.Add(p);owners.Add(e);vertices.Add(local);colors.Add(new Color(ground.r,ground.g,ground.b,alpha));radii.Add(radius);
                    }
                    // Vary arc-length spacing only. Never jitter dots away from the actual walk path.
                    distance+=gap*(order%4==0?.87f:order%4==1?1.11f:order%4==2?.97f:1.06f);order++;
                }
            }
            graphic.Draw(vertices,colors,radii,.65f/scale);
        }
    }
    public sealed class RouteDotsGraphic : MaskableGraphic
    {
        readonly List<Vector2> points=new List<Vector2>();readonly List<Color> tints=new List<Color>();readonly List<float> sizes=new List<float>();float shadow;
        public void Draw(List<Vector2> p,List<Color> c,List<float> r,float s)
        {points.Clear();points.AddRange(p);tints.Clear();tints.AddRange(c);sizes.Clear();sizes.AddRange(r);shadow=s;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();for(var i=0;i<points.Count;i++)
            {
                Circle(vh,points[i]+new Vector2(0,-shadow),sizes[i]+shadow*.25f,new Color(0.12f,.10f,.06f,tints[i].a*.42f));
                Circle(vh,points[i],sizes[i],tints[i]);
            }
        }
        static void Circle(VertexHelper vh,Vector2 p,float radius,Color color)
        {
            var start=vh.currentVertCount;vh.AddVert(p,color,Vector2.zero);
            for(var i=0;i<8;i++){var a=i*Mathf.PI/4;vh.AddVert(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius,color,Vector2.zero);}
            for(var i=0;i<8;i++)vh.AddTriangle(start,start+1+i,start+1+(i+1)%8);
        }
    }
}
