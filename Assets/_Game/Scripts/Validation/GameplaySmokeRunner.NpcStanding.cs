#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        Vector2 StandingFoot(Image image)
        {var map=Map.View.Content;return ((Vector2)map.InverseTransformPoint(CityMap.VisibleFoot(image))-map.rect.min)/map.rect.size;}
        IEnumerator NpcStandingJourney()
        {
            musicVirtualFocus=true;
            var rows=new List<string>{"city,node,arrivalFootGap,standingFootGap,actorMoved,approachResult"};var blocked=0;var tested=0;
            var wanted=new HashSet<string>{"H10","G14","S11"};
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();Map.View.ZoomBy(MapPanZoom.MaxZoom/Map.View.Zoom);
                var geometry=JsonUtility.ToJson(game.Data.navigation);
                foreach(var actor in AdventureUIArt.Load().mapActors.Where(a=>a.cityId==city.id&&AdventureUIArt.Load().Npc(a.artId)!=null))
                {
                    var index=NodeIndex(actor.nodeId);var image=MapActor(actor.nodeId);if(!image.gameObject.activeInHierarchy)continue;
                    yield return CloseEvent();
                    var fixedFoot=StandingFoot(image);var originalItems=game.Save.items.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();var quests=game.Save.completed.ToArray();
                    var arrival=CityRouteGeometry.Arrival(game.Data.nodes[index]);
                    var originalGap=Vector2.Scale(fixedFoot-arrival,Map.View.Content.rect.size).magnitude*Map.View.Zoom;
                    Map.View.Focus(arrival);yield return null;Time.timeScale=4;
                    yield return Click(actor.nodeId,true);var until=Time.realtimeSinceStartup+25;var captured=false;var prematureDialogue=false;
                    while(Map.Moving&&Time.realtimeSinceStartup<until)
                    {
                        prematureDialogue|=Map.DialogueOpen;
                        if(Map.NpcApproaching&&wanted.Contains(actor.nodeId)&&!captured)
                        {
                            captured=true;Time.timeScale=0;Map.View.Focus((arrival+fixedFoot)*.5f);yield return Snapshot("Standing-"+actor.nodeId+"-before");Time.timeScale=4;
                        }
                        yield return null;
                    }
                    Time.timeScale=1;Check(!Map.Moving&&game.Save.node==index,"Actual route reaches intended canonical node: "+actor.nodeId);
                    Check(!prematureDialogue,"Dialogue remained closed throughout Lumi's movement: "+actor.nodeId);
                    Check(Vector2.Distance(fixedFoot,StandingFoot(image))<.00001f,"Canonical visible NPC foot never moves during Lumi approach: "+actor.nodeId);
                    var status=Map.NpcApproachFailure==null?"PASS":"BLOCKED_SAFE_NO_APPROACH";
                    if(Map.NpcApproachFailure!=null)
                    {blocked++;Check(!Map.DialogueOpen,"Unsafe approach does not open a distant dialogue: "+actor.nodeId);
                        Check(GameObject.Find("NextObjective").GetComponent<Text>().text.Contains("tiếp cận an toàn"),"The blocked approach explanation is visible in the real map HUD: "+actor.nodeId);}
                    else
                    {
                        if(!Map.DialogueOpen)yield return Click("Interact",true);
                        yield return Until(()=>!Map.Moving,"NPC finishes standing "+actor.nodeId,8);
                        Check(Map.DialogueOpen&&Map.NpcFootDistance(index)<56,"Visible sprite feet are close before dialogue opens: "+actor.nodeId);
                    }
                    Check(game.Save.items.SequenceEqual(originalItems)&&game.Save.rewardGrantIds.SequenceEqual(ledger)&&game.Save.completed.SequenceEqual(quests),"Walking/approach alone cannot grant or consume quest/reward state: "+actor.nodeId);
                    rows.Add(city.id+","+actor.nodeId+","+originalGap+","+Map.NpcFootDistance(index)+",false,"+status);tested++;
                    yield return CloseEvent();
                    if(wanted.Contains(actor.nodeId)){Map.View.Focus((arrival+fixedFoot)*.5f);yield return Snapshot("Standing-"+actor.nodeId+"-after");}
                }
                Check(geometry==JsonUtility.ToJson(game.Data.navigation),"Approaches leave the entire Phase B graph and polylines unchanged: "+city.id);
            }
            File.WriteAllLines(Path.Combine(output,"npc-standing-all.csv"),rows);
            File.WriteAllText(Path.Combine(output,"standing-summary.json"),"{\"tested\":"+tested+",\"blocked\":"+blocked+",\"overall\":\""+(blocked>0?"PARTIAL":"PASS")+"\"}");
            KeepSave("Standing-earned");
        }
        IEnumerator NpcButtonsJourney()
        {
            musicVirtualFocus=true;
            yield return CrossEnter("hoi_an");yield return Travel(NodeIndex("H2"),true);
            var state=JsonUtility.ToJson(game.Save);yield return Click("DialogueCloseX",true);
            Check(JsonUtility.ToJson(game.Save)==state&&!game.Quests.Active,"X declines the offered quest without acceptance, proof or reward");
            yield return new WaitForSecondsRealtime(.35f);yield return Click("Interact",true);yield return Click("EventOption_accept",true);
            Check(game.Quests.Active&&FindButton("DialogueNext")==null,"Explicit quest acceptance has no duplicate promise/Next dialogue");
            if(FindButton("ReceiptContinue")!=null)
            {
                var acceptance=JsonUtility.ToJson(game.Save);yield return Click("ReceiptContinue",true);
                Check(JsonUtility.ToJson(game.Save)==acceptance,"The canonical acceptance-item receipt never grants twice on Continue");
            }
            Check(!Map.DialogueOpen,"After the actual acceptance receipt, play resumes without another NPC menu to close");
            Check(game.Inventory.Count("lamp_eye")==0,"Accept never hands out the quest item");
            // Existing earned partial save, copied and modified ONLY inside this QA run.
            var seed=Environment.GetCommandLineArgs().First(a=>a.StartsWith("--buttons-partial-save=")).Substring("--buttons-partial-save=".Length);
            File.Copy(seed,savePath,true);game.Reload();
            if(game.Inventory.Count("hoi_an_fresh_chili")==2)game.Save.items.Add("hoi_an_fresh_chili");game.Persist();
            yield return CrossEnter("hoi_an");yield return Travel(NodeIndex("H10"),true);yield return CloseEvent();yield return Click("Interact",true);
            var id=game.Events.BuildOptions(game.Save.node).options.First(o=>o.id.StartsWith("complete:")&&o.enabled).id;
            var quest=game.Content.Quest(game.Content.events.First(e=>e.cityId=="hoi_an"&&e.nodeId=="H10"&&e.id==id.Substring(9)).questId);
            var before=JsonUtility.ToJson(game.Save);yield return Click("DialogueCloseX",true);
            Check(JsonUtility.ToJson(game.Save)==before,"Reading the ready food quest then closing X never spends ingredients or grants rewards");
            yield return new WaitForSecondsRealtime(.35f);yield return Click("Interact",true);yield return Click("EventOption_"+id,true);
            Check(game.Quests.IsCompleted(quest.id)&&game.Inventory.Count("hoi_an_fresh_chili")==1&&game.Inventory.Count("hoi_an_purple_shallot")==1,"Only explicit deliver consumes 2 chilli and 1 shallot, preserving canonical reserves");
            var earned=game.Save.items.ToArray();var rewards=game.Save.rewardGrantIds.ToArray();yield return CloseEvent();yield return Click("Interact",true);
            Check(game.Save.items.SequenceEqual(earned)&&game.Save.rewardGrantIds.SequenceEqual(rewards),"Reopening completed NPC cannot duplicate rewards");yield return CloseEvent();
            var actionNode=Enumerable.Range(0,game.Data.nodes.Length).Where(i=>game.Discovery.IsVisible(i)).OrderByDescending(i=>
            {var v=game.Events.BuildOptions(i);game.Events.PruneRepeatedOptions(i,v,game.Events.Definition(game.Data.nodes[i].eventId));return v.options.Count;}).First();
            Time.timeScale=4;yield return Travel(actionNode,true);Time.timeScale=1;
            var options=game.Events.BuildOptions(game.Save.node);game.Events.PruneRepeatedOptions(game.Save.node,options,game.Events.Definition(game.Data.nodes[game.Save.node].eventId));
            Check(options.options.Count>0&&FindButton("EventMore")==null,"The old 1/2 page button is gone; genuine actions are directly listed");
            Check(options.options.All(o=>FindButton("EventOption_"+o.id)!=null),"Every genuine action is present in the scrollable list; no hidden page/branch loss");
            var actionScroll=GameObject.Find("EventActionsViewport").GetComponent<ScrollRect>();
            Canvas.ForceUpdateCanvases();var scrollState=JsonUtility.ToJson(game.Save);
            if(actionScroll.content.rect.height>actionScroll.viewport.rect.height+1)
            {
                var point=RectTransformUtility.WorldToScreenPoint(null,actionScroll.viewport.TransformPoint(actionScroll.viewport.rect.center));
                TouchQueue(49,UnityEngine.InputSystem.TouchPhase.Began,point+Vector2.down*65);yield return null;yield return null;
                for(var step=1;step<=8;step++){TouchQueue(49,UnityEngine.InputSystem.TouchPhase.Moved,point+Vector2.down*65+Vector2.up*step*18);yield return null;}
                TouchQueue(49,UnityEngine.InputSystem.TouchPhase.Ended,point+Vector2.up*79);yield return new WaitForSecondsRealtime(.2f);
                Check(actionScroll.verticalNormalizedPosition<.95f&&JsonUtility.ToJson(game.Save)==scrollState&&Map.DialogueOpen&&!Map.Moving,"Real simulated touch scrolls actions without selecting a choice or moving Lumi underneath");
            }
            else results.Add("NOT_TESTED: overflowing action-list touch drag; genuine actions fit this fixture's viewport");
            yield return Snapshot("Standing-actions-all");yield return CloseEvent();
            game.Persist();var saved=File.ReadAllText(savePath);game.Reload();Check(File.ReadAllText(savePath)==saved&&game.Quests.IsCompleted(quest.id),"Reload preserves earned delivery and reserves after simplified dialogue");
            // HQ06, unlike optional food prep, canonically requires a separate
            // confirmation. Unlock only its destination in this isolated fixture.
            var worldSeed=Environment.GetCommandLineArgs().First(a=>a.StartsWith("--buttons-world-save=")).Substring("--buttons-world-save=".Length);
            // Unload the old city's rendered node list before swapping a save
            // from another city; production never hot-swaps global Data beneath
            // a still-rendered CityMap in this way.
            game.Scenes.Go("Prototype");yield return Scene("Prototype");
            File.Copy(worldSeed,savePath,true);game.Reload();
            game.Save.completed.Remove("HQ06");game.Save.rewardGrantIds.RemoveAll(s=>s.Contains("HQ06"));
            game.Save.flags.Remove("quest:HQ06");game.Save.flags.Remove("hue.shallot_shared");
            if(!game.Save.accepted.Contains("HQ06"))game.Save.accepted.Add("HQ06");
            if(!game.Inventory.Has("hoi_an_purple_shallot"))game.Save.items.Add("hoi_an_purple_shallot");game.Persist();
            yield return CrossEnter("hue");yield return Travel(NodeIndex("U1"),true);yield return CloseEvent();yield return Click("Interact",true);
            id="complete:foodexchange_hue_u1";before=JsonUtility.ToJson(game.Save);
            yield return Click("EventOption_"+id,true);
            Check(FindButton("DeliveryConfirm")!=null&&JsonUtility.ToJson(game.Save)==before,"HQ06 confirmation opens without handing over items or receiving rewards");
            yield return Click("DeliveryCancel",true);Check(JsonUtility.ToJson(game.Save)==before,"Not yet / cancel retains every ingredient and reward");
            yield return Click("EventOption_"+id,true);yield return Click("DialogueCloseX",true);Check(JsonUtility.ToJson(game.Save)==before,"X on confirmation never delivers or rewards");
            yield return new WaitForSecondsRealtime(.35f);yield return Click("Interact",true);yield return Click("EventOption_"+id,true);
            var shallots=game.Inventory.Count("hoi_an_purple_shallot");yield return Click("DeliveryConfirm",true);
            Check(game.Quests.IsCompleted("HQ06")&&game.Inventory.Count("hoi_an_purple_shallot")==shallots-1,"Only explicit HQ06 confirm hands over exactly one shallot");
            earned=game.Save.items.ToArray();rewards=game.Save.rewardGrantIds.ToArray();yield return Click("ReceiptContinue",true);
            Check(game.Save.items.SequenceEqual(earned)&&game.Save.rewardGrantIds.SequenceEqual(rewards),"Receipt Continue only dismisses the receipt; it cannot grant twice");
            yield return CloseEvent();yield return Click("Interact",true);yield return CloseEvent();
            Check(game.Save.items.SequenceEqual(earned)&&game.Save.rewardGrantIds.SequenceEqual(rewards),"Completed HQ06 dialogue cannot repeat a delivery reward");
            game.Persist();saved=File.ReadAllText(savePath);game.Reload();Check(File.ReadAllText(savePath)==saved&&game.Quests.IsCompleted("HQ06"),"Confirmed delivery persists across reload without replaying a reward");
            KeepSave("Standing-buttons-earned");
        }
    }
}
#endif
