#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator OwnerPriorityJourney()
        {
            musicVirtualFocus=true;
            if(Environment.GetCommandLineArgs().Contains("--owner-priority-partial"))
            {
                yield return CrossEnter("hoi_an");Time.timeScale=4;yield return ChapterGo("H10");Time.timeScale=1;yield return CloseEvent();
                var node=NodeIndex("H10");var definition=game.Events.Definition(game.Data.nodes[node].eventId);
                var view=game.Events.BuildOptions(node);game.Events.PruneRepeatedOptions(node,view,definition);
                Check(!view.options.Any(o=>o.id=="talk"),"Owner's real partial save has no generic old primary-quest talk button at Ba Tu");
                var delivery=view.options.First(o=>o.id.StartsWith("complete:")&&!o.enabled);
                var reason=PlayerCopy.Unavailable(game,node,delivery,definition);
                Check(reason.Contains("Ớt Hội An × 1")&&reason.Contains("Hành tím Hội An × 1"),"Actual additive delivery explains both missing reserve-aware quantities");
                var state=JsonUtility.ToJson(game.Save);yield return Click("Interact",true);
                Check(FindButton("EventOption_talk")==null&&FindButton("EventOption_"+delivery.id)!=null,"Actual partial-save dialogue removes obsolete talk and retains the guarded delivery");
                var button=FindButton("EventOption_"+delivery.id);
                Check(!button.interactable&&button.GetComponentInChildren<Text>().text.Contains(reason),"Visible disabled delivery gives the exact missing-item reason");
                yield return Pointer(SLPoint((RectTransform)button.transform),true);
                Check(JsonUtility.ToJson(game.Save)==state,"Tapping blocked delivery consumes no ingredients and grants no reward");
                yield return Snapshot("Priority-partial-BaTu-dialogue");yield return Click("DialogueCloseX",true);yield break;
            }
            var rows=new System.Collections.Generic.List<string>{"city,node,zoom,oldGap,newGap,limit"};
            var callbacks=new System.Collections.Generic.List<string>{"city,node,option,result"};
            var checkedNpcs=0;var removed=0;var proofs=0;
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();
                var art=AdventureUIArt.Load();var original=JsonUtility.ToJson(game.Save);
                foreach(var zoom in new[]{1f,2f,4f})
                {
                    Map.View.ZoomBy(zoom/Map.View.Zoom);yield return null;yield return null;
                    foreach(var a in art.mapActors.Where(a=>a.cityId==city.id&&art.Npc(a.artId)!=null))
                    {
                        var i=NodeIndex(a.nodeId);var image=MapActor(a.nodeId);Check(image!=null,"Every assigned NPC has its real map actor: "+a.nodeId);
                        var gap=image.rectTransform.anchoredPosition.magnitude;
                        var limit=Mathf.Max(30,(art.ActorSize(city.id,a).x+Map.Marker.sizeDelta.x)*.5f+3);
                        var old=a.hasMapPosition?(Vector2.Scale(a.mapPosition-CityRouteGeometry.Arrival(game.Data.nodes[i]),Map.View.Content.rect.size)*zoom+a.offset).magnitude:a.offset.magnitude;
                        Check(gap<=limit+.1f,"Actual NPC foot stays within conversation distance at zoom "+zoom+": "+a.nodeId);
                        rows.Add(city.id+","+a.nodeId+","+zoom+","+old+","+gap+","+limit);
                        if(zoom==1)checkedNpcs++;
                    }
                }
                foreach(var node in game.Data.nodes.Select((n,i)=>new{n,i}).Where(n=>game.Discovery.IsVisible(n.i)))
                {
                    // Explicit isolated callback fixture, not a claimed walk/playthrough.
                    game.Save.node=node.i;game.Persist();var definition=game.Events.Definition(node.n.eventId);
                    var view=game.Events.BuildOptions(node.i);var beforeOptions=view.options.ToArray();
                    var fixture=JsonUtility.ToJson(game.Save);game.Events.PruneRepeatedOptions(node.i,view,definition);
                    Check(fixture==JsonUtility.ToJson(game.Save),"Dialogue filtering is read-only: "+node.n.id);
                    foreach(var option in beforeOptions.Where(o=>o.enabled&&!view.options.Any(v=>v.id==o.id)))
                    {
                        var result=game.Events.Execute(node.i,option.id);
                        Check(!result.changed&&!result.openBattle&&!result.openPuzzle&&JsonUtility.ToJson(game.Save)==fixture,"Removed choice really has no callback state effect: "+node.n.id+"/"+option.id);
                        callbacks.Add(city.id+","+node.n.id+","+option.id+",NO_EFFECT_CONFIRMED");removed++;
                    }
                    if(definition!=null&&(definition.onTalkFlags?.Length??0)>0&&beforeOptions.Any(o=>o.id=="talk"&&o.enabled))
                    {
                        foreach(var flag in definition.onTalkFlags)game.Save.flags.Remove(flag);
                        var proofView=game.Events.BuildOptions(node.i);game.Events.PruneRepeatedOptions(node.i,proofView,definition);
                        Check(proofView.options.Any(o=>o.id=="talk"&&o.enabled),"Real pending talk proof remains actionable: "+node.n.id);
                        var result=game.Events.Execute(node.i,"talk");
                        Check(result.changed&&definition.onTalkFlags.All(game.World.Has),"Real talk callback grants its original proof: "+node.n.id);
                        callbacks.Add(city.id+","+node.n.id+",talk,PROOF_CALLBACK_CONFIRMED");proofs++;
                    }
                    System.IO.File.WriteAllText(savePath,original);game.Reload();
                }
            }
            System.IO.File.WriteAllLines(System.IO.Path.Combine(output,"npc-distance-all.csv"),rows);
            System.IO.File.WriteAllLines(System.IO.Path.Combine(output,"dialogue-callbacks.csv"),callbacks);
            Check(checkedNpcs>20&&removed>0&&proofs>0,"Whole assigned NPC set and both redundant/meaningful callbacks were exercised");
            foreach(var pair in new[]{new[]{"hoi_an","H10"},new[]{"gyeongju","G14"}})
            {
                yield return CrossEnter(pair[0]);Time.timeScale=4;yield return ChapterGo(pair[1]);Time.timeScale=1;yield return CloseEvent();
                Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));yield return null;
                yield return Snapshot("Priority-"+pair[1]+"-near-npc");yield return Click("Interact",true);
                Check(GameObject.Find("NodeEventPanel")!=null&&FindButton("DialogueCloseX")!=null,"Actual node interaction still opens a closable conversation: "+pair[1]);
                yield return Snapshot("Priority-"+pair[1]+"-dialogue");yield return Click("DialogueCloseX",true);
                Check(Map.View.InputEnabled,"Closing conversation restores map input: "+pair[1]);
            }
            KeepSave("Priority-earned");
        }
        void OwnerMarkers()
        {
            var resolver=new NodeMarkerResolver(game);var state=JsonUtility.ToJson(game.Save);
            foreach(var n in game.Data.nodes.Select((n,i)=>new{n,i}))
            {
                var display=resolver.Resolve(n.i);
                if(game.Discovery.DiscoveryState(n.i)!=MapDiscoveryState.REVEALED)
                    Check(display.type!=NodeMarkerType.FragmentAlert,"Hidden/preview location does not disclose fragment: "+n.n.id);
                else Check((display.type==NodeMarkerType.FragmentAlert)==RelicGuide.PendingAt(game,n.n.id),"Fragment marker reflects real outstanding ownership: "+n.n.id);
                var actual=GameObject.Find("NodeMarker_"+n.n.id)?.GetComponent<RectTransform>();
                if(actual!=null)Check(Vector2.Distance(actual.anchorMin,CityRouteGeometry.Arrival(n.n))<.0001f,"Interaction marker uses the existing ground arrival: "+n.n.id);
            }
            Check(state==JsonUtility.ToJson(game.Save),"Marker/guidance reads do not mutate saves or rewards");
        }
        IEnumerator OwnerCollection()
        {yield return CloseEvent();yield return Click("QuestJournal",true);yield return Click("OpenRelicCollection",true);}
        IEnumerator OwnerConfusionJourney()
        {
            musicVirtualFocus=true;
            if(Environment.GetCommandLineArgs().Contains("--owner-main"))
            {
                yield return CrossEnter("hoi_an");Check(game.Quests.Completed&&!game.Quests.IsCompleted("HQ01"),"Main-stage review resumes from an earned local-light save");
                Time.timeScale=5;
                if(!game.Puzzles.Solved("puzzle_market"))yield return ChapterPuzzle("H4");
                if(!game.Puzzles.Solved("puzzle_paper"))yield return ChapterPuzzle("H13");
                yield return ChapterGo("H14");yield return CloseEvent();
                if(!game.Puzzles.Solved("puzzle_bot_code"))yield return ChapterPuzzle("H19");
                yield return ChapterGo("H5");yield return ChapterChoice("alternate");yield return CloseEvent();
                Check(game.Quests.GuidanceTarget==NodeIndex("H7"),"With actual main items collected, guidance points to the station rather than an optional errand");
                yield return ChapterGo("H7");yield return ChapterChoice("restore");Time.timeScale=1;yield return CloseEvent();
                Check(game.Quests.IsCompleted("HQ01")&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju"),"Unchanged main station opens both cities without a restored relic");
                OwnerMarkers();yield return Snapshot("Owner-H7-next-fragment");
                var mainRelic=RelicGuide.Local(game);
                while(game.Relics.Count(mainRelic)<4)
                {
                    var target=game.Quests.GuidanceTarget;Check(target>=0&&RelicGuide.PendingAt(game,game.Data.nodes[target].id),"Post-station next step is a concrete missing-fragment location");
                    Time.timeScale=5;yield return Travel(target,true);Time.timeScale=1;
                    var fragment=mainRelic.fragmentIds.Select(game.Relics.Fragment).First(f=>!game.Relics.Owned(f.id)&&RelicGuide.AtSource(game,f,game.Data.nodes[target].id));
                    yield return ClaimRelic(fragment.id,true);OwnerMarkers();
                }
                Time.timeScale=5;yield return Travel(NodeIndex(mainRelic.restorationNode),true);Time.timeScale=1;
                yield return RelicOption("relic_restore");for(var i=0;i<4;i++)yield return RelicDrag(i,true);
                yield return Click("RelicAssemble",true);yield return RelicExit();
                KeepSave("Owner-main-assembled");yield return CasualSignature();yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--owner-seal"))
            {
                yield return CrossEnter("hue");OwnerMarkers();
                Check(game.Inventory.Has("ancient_seal"),"Seal presentation uses an actually earned Hue inventory");
                var before=JsonUtility.ToJson(game.Save);var art=AdventureUIArt.Load();
                Check(art.Item("ancient_seal").sprite==art.Fragment("fragment_hue_dragon_face")&&art.Item("ancient_seal").sprite!=art.Item("link_key").sprite,"Ancient seal uses the existing seal illustration, distinct from the connection key");
                yield return Click("Inventory",true);yield return Click("Item_ancient_seal",true);
                Check(GameObject.Find("SelectedItemDescription").GetComponent<Text>().text.Contains("không tiêu hao"),"Inventory explains seal use without implying a universal key");
                yield return Snapshot("Owner-seal-inventory");yield return Click("CloseInventory",true);
                // The real receipt presenter, with an owned item. No reward/event is replayed.
                var presenter=(GameplayPanels)typeof(CityMap).GetField("panels",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(Map);
                presenter.ShowEvent(game.Save.node,received:new[]{new ItemAmount{itemId="ancient_seal",amount=1}});
                Check(GameObject.Find("ReceivedIcon_ancient_seal").GetComponent<Image>().sprite==art.Item("ancient_seal").sprite,"Actual reward receipt shares the corrected seal binding");
                Check(GameObject.Find("ReceivedPurpose").GetComponent<Text>().text.Contains("Giữ lại ấn"),"Receipt immediately explains what to do with the seal");
                yield return Snapshot("Owner-seal-receipt-presentation");yield return Click("ReceiptContinue",true);yield return CloseEvent();
                Check(before==JsonUtility.ToJson(game.Save),"Receipt/inventory display grants no repeat reward");
                Time.timeScale=5;yield return ChapterGo("U11");Time.timeScale=1;yield return CloseEvent();
                Map.View.ZoomBy(1.8f/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));
                yield return Snapshot("Owner-hue-arrival");KeepSave("Owner-seal-reviewed");yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--owner-restoration"))
            {yield return CasualSignature();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--owner-resume"))
            {
                yield return CrossEnter("hoi_an");var r=RelicGuide.Local(game);OwnerMarkers();
                Check(game.Relics.Count(r)==4&&!game.Relics.Assembled(r.id)&&!game.Relics.Restored(r.id),"Restart preserves four owned fragments without inventing assembly/restoration");
                Check(game.Quests.Guidance().Contains("4 mảnh")&&game.Quests.GuidanceTarget==NodeIndex("H2"),"Restart clearly directs the player to Mai for assembly");
                yield return OwnerCollection();yield return Click("RelicCard_"+r.id,true);yield return Snapshot("Owner-four-fragments-restart");yield return RelicExit();yield break;
            }
            yield return H3TapJourney();
            Check(game.Quests.Guidance().Contains("Mang Mắt Đèn trả Cô Mai"),"After pickup the objective names the item and recipient");
            yield return Snapshot("Owner-eye-return-objective");yield return Travel(NodeIndex("H2"),true);yield return Click("EventOption_tutorial_return",true);yield return CloseEvent();
            OwnerMarkers();yield return RelicMapFrame("Owner-fragment-markers-start");
            yield return ClaimRelic("fragment_hoi_an_eye_reflection",true);OwnerMarkers();
            Time.timeScale=5;yield return Travel(NodeIndex("H0"),true);Time.timeScale=1;
            yield return ClaimRelic("fragment_hoi_an_bamboo_frame",true);OwnerMarkers();
            Time.timeScale=5;yield return Travel(NodeIndex("H4"),true);Time.timeScale=1;yield return SolvePuzzle("puzzle_market");
            yield return ClaimRelic("fragment_hoi_an_old_flower_paper",true);OwnerMarkers();
            Time.timeScale=5;yield return Travel(NodeIndex("H10"),true);Time.timeScale=1;
            yield return ClaimRelic("fragment_hoi_an_roof_hook",true);OwnerMarkers();
            var relic=RelicGuide.Local(game);
            Check(game.Relics.Count(relic)==4&&!game.Relics.Assembled(relic.id)&&!game.Relics.Restored(relic.id),"Four real source interactions leave assembly/trial explicitly pending");
            Check(game.Quests.GuidanceTarget==NodeIndex("H2")&&game.Quests.Guidance().Contains("4 mảnh"),"Four fragments override unrelated optional errands and point to Mai");
            KeepSave("Owner-four-fragments-earned");
            yield return OwnerCollection();yield return Snapshot("Owner-four-fragments-collection");yield return Click("RelicCard_"+relic.id,true);
            Check(GameObject.Find("RelicInstructions").GetComponent<Text>().text.Contains("Tiệm đèn Cô Mai")&&FindButton("RelicDestination")!=null,"Relic details name the assembly location and expose a real CTA");
            yield return Snapshot("Owner-four-fragments-next-step");yield return Click("RelicDestination",true);
            Check(Map.SelectedNode==NodeIndex("H2")&&!Map.DialogueOpen,"CTA focuses the existing destination without teleporting or altering quest state");
            Time.timeScale=5;yield return Travel(NodeIndex("H2"),true);Time.timeScale=1;yield return CloseEvent();
            Map.View.ZoomBy(1.8f/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));yield return Snapshot("Owner-mai-ground-arrival");
            yield return OwnerCollection();yield return Click("RelicCard_"+relic.id,true);yield return Click("RelicDestination",true);
            for(var i=0;i<4;i++)yield return RelicDrag(i,true);
            yield return Click("RelicAssemble",true);
            Check(game.Relics.Assembled(relic.id)&&!game.Relics.Restored(relic.id)&&!game.Quests.IsCompleted("HQ01"),"Collection CTA reuses real assembly; missing H7 still blocks restoration");
            Check(GameObject.Find("RelicInstructions").GetComponent<Text>().text.Contains("đồng bộ trạm"),"Blocked trial explains the actual H7 prerequisite");
            yield return Snapshot("Owner-assembly-H7-required");yield return RelicExit();KeepSave("Owner-assembled-before-H7");
        }
    }
}
#endif
