using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed class CityMapPaths : MonoBehaviour
    {
        private sealed class Segment
        {
            public EdgeData edge;
            public Vector2 from, to;
            public RectTransform rect;
            public Image image;
            public bool visible;
            public float revealedAt;
            public int routeOrder;
            public string stroke;
            public bool selected,draw;
        }
        private readonly List<Segment> segments = new List<Segment>();
        private GameBootstrap game;
        private RectTransform map;
        private Vector2 previousSize;
        private bool initialized;
        private readonly HashSet<EdgeData> highlighted = new HashSet<EdgeData>();
        public int DrawnStrokeCount { get; private set; }
        public void Highlight(IList<int> route)
        {
            highlighted.Clear();
            if(route!=null)for(var i=1;i<route.Count;i++)
                foreach(var edge in game.Data.edges)
                    if(edge.from==route[i-1]&&edge.to==route[i]||edge.to==route[i-1]&&edge.from==route[i])highlighted.Add(edge);
            Refresh();Layout();
        }
        private static string PointKey(Vector2 p)=>Mathf.RoundToInt(p.x*10000)+":"+Mathf.RoundToInt(p.y*10000);
        public void Configure(GameBootstrap owner, UIController ui, RectTransform content)
        {
            game = owner; map = content;
            foreach (var edge in game.Data.edges)
            {
                var points = CityRouteGeometry.Points(game.Data, edge.from, edge.to);
                for (var i = 1; i < points.Count; i++)
                {
                    var rect = ui.Panel(map, "Path_" + edge.from + "_" + edge.to + "_" + i, Vector2.zero, Vector2.zero, Color.white);
                    rect.GetComponent<Image>().raycastTarget = false;
                    UIController.Border(rect, new Color(.12f,.23f,.25f,.38f));
                    rect.GetComponent<Outline>().effectDistance = new Vector2(.5f,-.5f);
                    var a=PointKey(points[i-1]);var b=PointKey(points[i]);
                    segments.Add(new Segment { edge=edge, from=points[i-1], to=points[i], rect=rect, image=rect.GetComponent<Image>(),routeOrder=i-1,stroke=string.CompareOrdinal(a,b)<0?a+"/"+b:b+"/"+a });
                }
            }
            Refresh(); Layout();
        }
        public void Refresh()
        {
            var batch=new Dictionary<EdgeData,int>();
            var strokes=new Dictionary<string,Segment>();
            foreach (var segment in segments)
            {
                var visible=game.Discovery.IsVisible(segment.edge.from)&&game.Discovery.IsVisible(segment.edge.to)&&game.Data.EdgeOpen(segment.edge,game.Save);
                if(visible&&!segment.visible)
                {
                    if(!batch.ContainsKey(segment.edge))batch[segment.edge]=batch.Count;
                    segment.revealedAt=initialized?Time.unscaledTime+Mathf.Min(.32f,batch[segment.edge]*.05f+segment.routeOrder*.015f):Time.unscaledTime-.55f;
                }
                segment.visible=visible; segment.rect.gameObject.SetActive(visible);
                segment.selected=highlighted.Contains(segment.edge);
                segment.draw=visible&&!CityRouteGeometry.IsCovered(game.Data,(segment.from+segment.to)*.5f);
                if(segment.draw&&strokes.TryGetValue(segment.stroke,out var first))
                {
                    first.selected|=segment.selected;segment.draw=false;
                }
                else if(segment.draw)strokes.Add(segment.stroke,segment);
                segment.image.color=segment.edge.travel=="ferry"?new Color(.55f,.86f,.89f,1):new Color(1,.86f,.60f,1);
            }
            DrawnStrokeCount=strokes.Count;
            foreach(var segment in segments)
            {
                segment.image.enabled=segment.draw;
                segment.rect.GetComponent<Outline>().enabled=segment.draw;
            }
            initialized=true;
        }
        private float previousZoom;
        private void LateUpdate()
        {
            if (map != null && (previousSize != map.rect.size || previousZoom != map.localScale.x)) Layout();
            foreach(var segment in segments) if(segment.visible)
            {
                var motion=NodeMarkerView.MotionMode;
                var color=segment.image.color; color.a=(segment.selected?.92f:.30f)*(motion==MarkerMotionMode.Off?1:Mathf.Clamp01((Time.unscaledTime-segment.revealedAt)/(motion==MarkerMotionMode.Reduced?.2f:.55f))); segment.image.color=color;
                var thickness=segment.selected?2.2f:1.15f;
                segment.rect.sizeDelta=new Vector2(segment.rect.sizeDelta.x,thickness/Mathf.Max(1,previousZoom));
            }
        }
        private void Layout()
        {
            previousSize = map.rect.size;
            previousZoom = map.localScale.x;
            foreach (var segment in segments)
            {
                var a = Vector2.Scale(segment.from, previousSize); var b = Vector2.Scale(segment.to, previousSize);
                segment.rect.anchorMin = segment.rect.anchorMax = Vector2.zero;
                segment.rect.anchoredPosition = (a+b)*.5f;
                segment.rect.sizeDelta = new Vector2(Vector2.Distance(a,b), (segment.selected?2.2f:1.15f) / Mathf.Max(1,previousZoom));
                segment.rect.GetComponent<Outline>().effectDistance = new Vector2(.3f,-.3f) / Mathf.Max(1,previousZoom);
                segment.rect.localRotation = Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);
            }
        }
    }
}
