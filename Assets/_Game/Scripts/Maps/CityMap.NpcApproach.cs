using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        // Transient only: save still records the canonical node, never this path.
        List<Vector2> npcReturn;
        string npcStandingMessage;
        public bool NpcApproaching {get;private set;}
        public string NpcApproachFailure {get;private set;}

        public static Vector3 VisibleFoot(Image image)
        {
            var rect=image.rectTransform.rect;var sprite=image.sprite;
            if(sprite==null)return image.rectTransform.position;
            var size=sprite.rect.size;
            if(image.preserveAspect&&size.y>0&&rect.height>0)
            {
                var ratio=size.x/size.y;
                if(ratio>rect.width/rect.height){var h=rect.width/ratio;rect.y+=(rect.height-h)*image.rectTransform.pivot.y;rect.height=h;}
                else{var w=rect.height*ratio;rect.x+=(rect.width-w)*image.rectTransform.pivot.x;rect.width=w;}
            }
            // Mesh bounds follow the visible sprite, including transparent padding
            // and aspect-fit margins; do not compare empty RectTransform corners.
            var vertices=sprite.vertices;var min=new Vector2(float.MaxValue,float.MaxValue);var max=-min;
            foreach(var v in vertices){min=Vector2.Min(min,v);max=Vector2.Max(max,v);}
            var pixel=new Vector2((min.x+max.x)*.5f,min.y)*sprite.pixelsPerUnit+sprite.pivot;
            var uv=new Vector2(pixel.x/size.x,pixel.y/size.y);
            return image.rectTransform.TransformPoint(rect.min+Vector2.Scale(uv,rect.size));
        }
        Vector2 MapPoint(Vector3 world)
        {var p=(Vector2)map.InverseTransformPoint(world)-map.rect.min;return p/map.rect.size;}
        public float NpcFootDistance(int index)
        {
            var actor=actors[index];if(actor==null)return 0;
            return Vector2.Distance(ui.Root.InverseTransformPoint(VisibleFoot(actor)),ui.Root.InverseTransformPoint(VisibleFoot(Marker.GetComponent<Image>())));
        }
        bool IsNpc(int index)=>actors[index]!=null&&actors[index].gameObject.activeInHierarchy&&presentation.Npc(actorDefinitions[index]?.artId)!=null;
        float NpcDistanceLimit(int index)=>Mathf.Max(38,(presentation.ActorSize(game.Data.cityId,actorDefinitions[index]).x+Marker.sizeDelta.x)*.5f+6);

        // Reuse the authoritative Dijkstra and existing, open WALK polylines.
        // Never add graph vertices, cross a ferry, or invent an off-road shortcut.
        public List<Vector2> NpcStandingPath(int index)
        {
            if(!IsNpc(index)||NpcFootDistance(index)<=NpcDistanceLimit(index))return new List<Vector2>();
            var graph=game.Data.navigation;if(graph==null)return null;
            var binding=Array.Find(graph.nodes,n=>n.nodeId==game.Data.nodes[index].id);if(binding==null)return null;
            // S11's approved actor stands at the station entrance, beyond the
            // route endpoint on the same paved forecourt. This short, reversible
            // interaction approach follows that pavement; it is not a travel edge.
            if(game.Data.cityId=="seoul"&&game.Data.nodes[index].id=="S11")
                return new List<Vector2>{CityRouteGeometry.Arrival(game.Data.nodes[index]),new Vector2(.872f,.7215f),new Vector2(.8826f,.723f)};
            var npc=MapPoint(VisibleFoot(actors[index]));var size=map.rect.size;
            var radius=(NpcDistanceLimit(index)-5)/View.Zoom;
            List<Vector2> best=null;var bestCost=float.PositiveInfinity;
            var plans=new Dictionary<string,NavigationJourney>();
            foreach(var edge in graph.edges)
            {
                if(edge.travel=="ferry"||!graph.EdgeOpen(edge,game.Save))continue;
                for(var segment=1;segment<edge.points.Length;segment++)
                {
                    var a=Vector2.Scale(edge.points[segment-1]-npc,size);var d=Vector2.Scale(edge.points[segment]-edge.points[segment-1],size);
                    if(d.sqrMagnitude<.000001f)continue;
                    var t=Mathf.Clamp01(-Vector2.Dot(a,d)/d.sqrMagnitude);
                    if((a+d*t).magnitude>radius)continue;
                    // Stop beside the NPC, rather than exactly on top of it.
                    var disc=Vector2.Dot(a,d)*Vector2.Dot(a,d)-d.sqrMagnitude*(a.sqrMagnitude-radius*radius);
                    var spread=disc>0?Mathf.Sqrt(disc)/d.sqrMagnitude:0;
                    var center=-Vector2.Dot(a,d)/d.sqrMagnitude;
                    foreach(var at in new[]{Mathf.Clamp01(center-spread),Mathf.Clamp01(center+spread),t})
                    {
                        var target=Vector2.Lerp(edge.points[segment-1],edge.points[segment],at);
                        if(Vector2.Scale(target-npc,size).magnitude>radius+.01f)continue;
                        for(var reverse=0;reverse<(edge.bidirectional?2:1);reverse++)
                        {
                            var from=reverse==0?edge.from:edge.to;
                            if(!plans.TryGetValue(from,out var journey)){journey=graph.PlanVertices(game.Data,binding.vertexId,from,game.Save);plans[from]=journey;}
                            if(journey==null||journey.legs.Exists(l=>l.edge.travel=="ferry"))continue;
                            if(journey.canonicalStops.Exists(i=>i!=index&&game.Events.Definition(game.Data.nodes[i].eventId)?.mandatory==true&&!game.Events.Resolved(game.Events.Definition(game.Data.nodes[i].eventId))))continue;
                            var points=new List<Vector2>{CityRouteGeometry.Arrival(game.Data.nodes[index])};
                            foreach(var leg in journey.legs){var p=leg.Points;points.AddRange(p.GetRange(1,p.Count-1));}
                            if(reverse==0){for(var k=1;k<segment;k++)points.Add(edge.points[k]);}
                            else{for(var k=edge.points.Length-2;k>=segment;k--)points.Add(edge.points[k]);}
                            points.Add(target);var cost=CityRouteGeometry.Length(points,size);
                            if(cost>size.x*.30f||cost>=bestCost)continue;
                            bestCost=cost;best=points;
                        }
                    }
                }
            }
            return best;
        }
        IEnumerator WalkNpcPath(List<Vector2> points)
        {
            if(points==null||points.Count<2)yield break;
            var measured=CityRouteGeometry.Measure(points,map.rect.size);var duration=Mathf.Max(.025f,measured[measured.Length-1]/190f);
            for(float t=0;t<1;t+=Time.deltaTime/duration)
            {PositionMarker(CityRouteGeometry.SampleMeasured(points,measured,t));View.EnsureVisible(Marker.anchorMin);yield return null;}
            PositionMarker(points[points.Count-1]);
        }
        IEnumerator ReturnFromNpc()
        {
            if(npcReturn==null)yield break;
            var reverse=new List<Vector2>(npcReturn);reverse.Reverse();npcReturn=null;
            yield return WalkNpcPath(reverse);
        }
        void NpcStandingNotice(string message)
        {
            // CityMap hides the generic MessagePanel. Use the visible map HUD
            // instead of sending an error to ui.Say where it cannot be read.
            npcStandingMessage=message;nextObjective.text=message;SelectedNode=game.Save.node;
            tooltipName.text=game.Data.nodes[SelectedNode].name;
            tooltipState.text=message;Tooltip.gameObject.SetActive(true);
        }
        IEnumerator ApproachNpcAndInteract()
        {
            var index=game.Save.node;NpcApproachFailure=null;npcStandingMessage=null;
            if(IsNpc(index)&&NpcFootDistance(index)>NpcDistanceLimit(index))
            {
                Moving=NpcApproaching=true;Refresh();yield return ReturnFromNpc();
                var path=NpcStandingPath(index);
                if(path==null)
                {
                    NpcApproachFailure=game.Data.nodes[index].id+": không có điểm đứng gần NPC trên lối đi đang mở.";
                    Moving=NpcApproaching=false;Refresh();NpcStandingNotice("Chưa có lối tiếp cận an toàn · Lumi dừng trên đường.");yield break;
                }
                npcReturn=path;yield return WalkNpcPath(path);Moving=NpcApproaching=false;Refresh();
                if(NpcFootDistance(index)>NpcDistanceLimit(index)+3)
                {NpcApproachFailure=game.Data.nodes[index].id+": chân sprite chưa đủ gần sau tiếp cận.";NpcStandingNotice("Lumi chưa đứng đủ gần để trò chuyện.");yield break;}
                if(requestedRouteDestination>=0&&requestedRouteDestination!=index)
                {var target=requestedRouteDestination;requestedRouteDestination=-1;TryMove(target);yield break;}
            }
            dialogue=true;Deselect();Refresh();panels.ShowEvent(index);
        }
    }
}
