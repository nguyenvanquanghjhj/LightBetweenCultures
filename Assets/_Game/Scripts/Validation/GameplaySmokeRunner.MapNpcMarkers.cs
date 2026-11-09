#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        Image MapNpcImage(string id)=>Map.View.Content.Find(id)?.Find("MapActor_"+id)?.GetComponent<Image>();
        void CheckNpcActor(int index)
        {
            var node=game.Data.nodes[index];var definition=AdventureUIArt.Load().MapActor(game.Data.cityId,node.id);
            var image=MapNpcImage(node.id);var display=new NodeMarkerResolver(game).Resolve(index);
            if(display.type==NodeMarkerType.NpcTalk||display.type==NodeMarkerType.NpcNeedItem)
                Check(image!=null&&image.gameObject.activeInHierarchy,"Every actual talk/item-wait marker has a visible actor: "+game.Data.cityId+":"+node.id);
            if(image==null)return;
            var visible=game.Discovery.IsVisible(index)&&(!definition.hideWhenResolved||!game.Events.Resolved(game.Events.Definition(game.Data.nodes[index].eventId)));
            Check(image.gameObject.activeInHierarchy==visible,"Actor respects real discovery/resolution: "+node.id);
            if(!visible)return;
            Check(image.sprite==AdventureUIArt.Load().ActorSprite(definition.artId)&&image.preserveAspect&&image.useSpriteMesh,"Real assigned sprite/aspect/outline preserved: "+node.id);
            var hit=image.transform.parent.Find("ActorHitArea").GetComponent<RectTransform>();
            Check(hit.rect.width>=52&&hit.rect.height>=52&&Mathf.Abs(AdventureUIArt.Load().ActorScale(game.Data.cityId,definition)-.72f)<.001f,"Shared72percent scale and52-unit touch area: "+node.id);
            if(!definition.hasMapPosition)return;
            var map=Map.View.Content;var target=map.TransformPoint(map.rect.min+Vector2.Scale(map.rect.size,definition.mapPosition));
            Check(Vector3.Distance(image.rectTransform.position,target)<.05f,"New actor foot remains at its configured map-space ground anchor: "+node.id);
            Check(Vector3.Distance(image.rectTransform.position,image.transform.parent.Find("ActorShadow").position)<.05f,"Shadow follows actor foot: "+node.id);
            var marker=Map.NodeMarkers.First(m=>m.NodeId==node.id);
            if(marker.Badge.gameObject.activeInHierarchy)
            {
                var actorCorners=new Vector3[4];var markerCorners=new Vector3[4];image.rectTransform.GetWorldCorners(actorCorners);marker.Badge.GetWorldCorners(markerCorners);
                Check(markerCorners[0].y>=actorCorners[1].y+2,"Semantic marker clears the actor face: "+node.id);
            }
        }
        IEnumerator MapNpcFrame(string name)
        {
            yield return CloseEvent();yield return new WaitForSecondsRealtime(.35f);
            yield return Snapshot(name);
        }
        IEnumerator ClickNpcSprite(string node)
        {
            yield return CloseEvent();yield return new WaitForSecondsRealtime(.35f);
            var image=MapNpcImage(node);var rect=image.rectTransform;Canvas.ForceUpdateCanvases();
            yield return Pointer(RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center)),true);
            yield return Until(()=>Map.DialogueOpen,"NPC actor pointer opens existing dialogue");
            var evt=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);var bound=AdventureUIArt.Load().Npc(evt.npcId);
            yield return new WaitForSecondsRealtime(.45f);PortraitCheck();
            Check(game.Data.nodes[game.Save.node].id==node&&GameObject.Find("EventPortrait").GetComponent<Image>().sprite==(bound.portrait??bound.sprite),"Sprite tap reaches its own event and existing portrait: "+node);
        }
        IEnumerator MapNpcMarkerJourney()
        {
            var args=Environment.GetCommandLineArgs();
            if(args.Contains("--map-npc-markers-hidden"))
            {
                yield return CrossEnter("hoi_an");var before=JsonUtility.ToJson(game.Save);
                foreach(var id in new[]{"H10","H14"})
                {Check(!game.Discovery.IsVisible(NodeIndex(id))&&!MapNpcImage(id).gameObject.activeInHierarchy,"New NPC is absent behind unopened discovery: "+id);Map.TryMove(NodeIndex(id));Check(!Map.Moving&&Map.SelectedNode!=NodeIndex(id),"Hidden NPC cannot be selected or reached: "+id);}
                Check(!game.IsCityUnlocked("hue")&&!game.IsCityUnlocked("jeonju")&&JsonUtility.ToJson(game.Save)==before,"NPC/portal presentation cannot bypass fresh H7 world gate");KeepSave("npc-hidden-final");yield break;
            }
            if(args.Contains("--map-npc-markers-fresh")||args.Contains("--map-npc-markers-restart"))
            {
                var restart=args.Contains("--map-npc-markers-restart");yield return CrossEnter("hoi_an");Time.timeScale=4;
                yield return ChapterGo("H10");yield return CloseEvent();yield return ClickNpcSprite("H10");
                if(!restart){yield return Snapshot("NPC-H10-before-accept");yield return ChapterChoice("accept");yield return CloseEvent();}
                Check(game.Quests.IsActive("HQ02")&&!game.Quests.IsCompleted("HQ02"),"Real BaTu quest remains pending, never auto-completed by actor visibility");
                yield return CloseEvent();yield return null;yield return new WaitForSecondsRealtime(.25f);
                Check(Map.NodeMarkers.First(m=>m.NodeId=="H10").Type==NodeMarkerType.NpcNeedItem,"BaTu pending-item marker still belongs to the real visible NPC");CheckNpcActor(NodeIndex("H10"));yield return Snapshot("NPC-H10-needs-item"+ (restart?"-restart":""));
                if(!restart)
                {
                    yield return ChapterGo("H14");yield return CloseEvent();yield return ClickNpcSprite("H14");yield return Snapshot("NPC-H14-dialogue");
                    var evt=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);yield return ChapterChoice("talk");yield return CloseEvent();
                    var coins=game.Save.coins;var items=game.Save.items.ToArray();yield return ChapterGo("H14");yield return ChapterChoice("talk");yield return CloseEvent();
                    Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(items)&&game.Events.Resolved(evt),"Existing Phuc dialogue/gift remains one-shot after repeated interaction");
                }
                Check(Map.HoiAnLightingState=="B"&&Mathf.Abs(Map.DistrictWeightedCoverage-.2f)<.001f,"NPC fixes retain real StateB20 after H7, before relic restoration");
                yield return TestMapInput();KeepSave("npc-fresh-final");Time.timeScale=1;yield break;
            }
            var rows=new System.Collections.Generic.List<string>{"City|NodeId|Marker|NpcId|Speaker|ActorId|Sprite|Visible"};
            var coinsBefore=game.Save.coins;var itemsBefore=game.Save.items.ToArray();var questsBefore=game.Save.completed.ToArray();var flagsBefore=game.Save.flags.ToArray();var ledgerBefore=game.Save.rewardGrantIds.ToArray();
            var cityFilter=args.FirstOrDefault(a=>a.StartsWith("--map-npc-markers-city="))?.Substring("--map-npc-markers-city=".Length);
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"}.Where(c=>string.IsNullOrEmpty(cityFilter)||c==cityFilter))
            {
                yield return CrossEnter(city);Time.timeScale=4;yield return new WaitForSecondsRealtime(.25f);
                for(var i=0;i<game.Data.nodes.Length;i++)
                {
                    CheckNpcActor(i);var node=game.Data.nodes[i];var evt=game.Events.Definition(game.Data.nodes[i].eventId);var actor=AdventureUIArt.Load().MapActor(city,node.id);
                    if(evt.type=="npc"||evt.type=="dialogue")rows.Add(string.Join("|",city,node.id,new NodeMarkerResolver(game).Resolve(i).type,evt.npcId,evt.actor,actor?.artId??"",MapNpcImage(node.id)?.sprite?.name??"",MapNpcImage(node.id)?.gameObject.activeInHierarchy??false));
                }
                var focus=city=="hoi_an"?"H10":city=="hue"?"U14":city=="da_lat"?"D2":city=="jeonju"?"J2":city=="gyeongju"?"G9":"S7";
                yield return ChapterGo(focus);yield return CloseEvent();yield return MapNpcFrame("NPC-city-"+city);
                foreach(var actor in AdventureUIArt.Load().mapActors.Where(a=>a.cityId==city&&a.hasMapPosition))
                {
                    yield return ChapterGo(actor.nodeId);yield return CloseEvent();
                    foreach(var zoom in new[]{1.3f,2.4f})
                    {Map.View.ZoomBy(zoom/Map.View.Zoom);Map.View.Focus(actor.mapPosition);yield return new WaitForSecondsRealtime(.3f);CheckNpcActor(NodeIndex(actor.nodeId));}
                    yield return MapNpcFrame("NPC-"+actor.nodeId+"-map");yield return ClickNpcSprite(actor.nodeId);yield return Snapshot("NPC-"+actor.nodeId+"-dialogue");yield return CloseEvent();
                    if(actor.nodeId=="S2"||actor.nodeId=="G14")
                    {
                        var options=game.Events.BuildOptions(game.Save.node).options;
                        Check(options.Any(o=>o.id=="talk"||o.id=="progress"||o.id.StartsWith("progress:"))&&options.Any(o=>o.id=="checkin"||o.id=="checkin_view"),"NPC and check-in both retain their own interaction choices: "+actor.nodeId);
                        var entry=game.Content.checkIns.First(c=>c.cityId==city&&c.nodeId==actor.nodeId);yield return OpenCheckInView(entry);yield return Click("CheckInClose");
                    }
                }
                if(city=="hue"||city=="seoul")
                {
                    var id=city=="hue"?"U14":"S14";yield return ChapterGo(id);
                    Check(new NodeMarkerResolver(game).Resolve(NodeIndex(id)).type==NodeMarkerType.Quest&&MapNpcImage(id)==null,"Location clue uses journal/info marker without a fake person: "+id);yield return Snapshot("NPC-location-"+id);yield return CloseEvent();
                }
                yield return TestMapInput();
            }
            yield return CrossEnter("hoi_an");
            foreach(var pair in new[]{("H8","jeonju"),("H20","hue")})
            {
                yield return ChapterGo(pair.Item1);Check(new NodeMarkerResolver(game).Resolve(NodeIndex(pair.Item1)).type==NodeMarkerType.Portal,"Arch/arrow is a Portal, not bell/quest: "+pair.Item1);
                yield return CloseEvent();yield return MapNpcFrame("Portal-"+pair.Item1);yield return ChapterGo(pair.Item1);yield return ChapterChoice("portal");yield return Scene("WorldMap");
                Check(game.Save.cityId=="hoi_an"&&FindButton("EnterCity").interactable,"Portal opens WorldMap destination preview, preserves existing gate and waits for Enter");yield return Snapshot("Portal-"+pair.Item1+"-WorldMap");
                yield return Click("EnterCity");yield return Scene("CityMap");Check(game.Save.cityId==pair.Item2,"Portal destination enters configured city: "+pair.Item2);yield return CrossEnter("hoi_an");
            }
            Check(game.Save.coins==coinsBefore&&game.Save.items.SequenceEqual(itemsBefore)&&game.Save.completed.SequenceEqual(questsBefore)&&game.Save.flags.SequenceEqual(flagsBefore)&&game.Save.rewardGrantIds.SequenceEqual(ledgerBefore),"Map actor/marker/dialogue-view/portal QA preserves quests/items/coins/world/relic/check-in/reward history");
            File.WriteAllLines(Path.Combine(output,"runtime-npc-audit.csv"),rows);KeepSave("npc-audit-final");Time.timeScale=1;
        }
    }
}
#endif
