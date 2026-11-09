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
        string NpcStoryState()=>string.Join("|",game.Save.accepted)+"/"+string.Join("|",game.Save.completed)+"/"+string.Join("|",game.Save.flags)+"/"+string.Join("|",game.Save.solvedPuzzles)+"/"+string.Join("|",game.Save.defeatedEnemies)+"/"+string.Join("|",game.Save.choices);
        IEnumerator ThreeNpcJourney()
        {
            musicVirtualFocus=true;
            var restart=Environment.GetCommandLineArgs().Contains("--three-npc-restart");
            var cases=new[]{("gyeongju","G16","npc_kang_taeho",2,5),("gyeongju","G21","npc_choi_eunsu",2,0),("seoul","S12","npc_han_doyun",3,12)};
            if(!restart)
            {
                // Copy of an earned pre-change Windows save; never reads/writes owner storage.
                var legacy=JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(output,"legacy-source.json")));
                foreach(var key in new[]{"gyeongju:loot_lake","gyeongju:secret_herb","seoul:loot_park"})
                {legacy.resolvedEvents.RemoveAll(k=>k==key);legacy.rewardGrantIds.RemoveAll(k=>k.StartsWith("event:"+key+":"));}
                yield return UXFixture(JsonUtility.ToJson(legacy));
            }
            foreach(var c in cases)
            {
                yield return CrossEnter(c.Item1);yield return CloseEvent();
                game.Save.node=NodeIndex(c.Item2);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                var e=game.Content.events.Single(x=>x.cityId==c.Item1&&x.nodeId==c.Item2&&!x.additive);
                Check(e.npcId==c.Item3&&e.type=="npc"&&!e.mandatory&&!e.autoCollect&&e.oneShot,"Optional NPC keeps original one-time interaction: "+c.Item2);
                var coins=game.Save.coins;var teas=game.Save.items.Count(i=>i=="healing_tea");var story=NpcStoryState();
                var expected=JsonUtility.ToJson(game.Save);
                CheckNpcActor(game.Save.node);
                var actor=MapNpcImage(c.Item2);
                Check(actor!=null&&actor.gameObject.activeInHierarchy&&actor.sprite==AdventureUIArt.Load().Npc(c.Item3).sprite,"Visible map NPC uses its own existing sprite: "+c.Item2);
                Map.View.ZoomBy(1.7f/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));
                yield return new WaitForSecondsRealtime(.4f);
                if(!restart)yield return Snapshot("ThreeNpc-Map-"+c.Item2);
                var actorRect=actor.rectTransform;
                yield return Pointer(RectTransformUtility.WorldToScreenPoint(null,actorRect.TransformPoint(actorRect.rect.center)),true);
                yield return Until(()=>Map.DialogueOpen,"NPC sprite tap opens dialogue "+c.Item2,8);
                Check(Map.NpcApproachFailure==null&&!Map.Moving,"Lumi reaches NPC without a new route: "+c.Item2);
                yield return new WaitForSecondsRealtime(.5f);Canvas.ForceUpdateCanvases();
                var portrait=GameObject.Find("EventPortrait")?.GetComponent<Image>();
                Check(portrait!=null&&portrait.sprite==AdventureUIArt.Load().Npc(c.Item3).portrait&&portrait.useSpriteMesh,"Correct native NPC portrait: "+c.Item2);
                Check(GameObject.Find("EventSpeaker").GetComponent<Text>().text==e.actor,"Correct Korean name: "+c.Item2);
                Check(GameObject.Find("EventText").GetComponent<Text>().text.Contains(restart?e.completedText:e.text),"Initial/return dialogue: "+c.Item2);
                var panel=GameObject.Find("NodeEventPanel").GetComponent<RectTransform>();var root=GameObject.Find("HUD").GetComponent<UIController>().Root;
                var corners=new Vector3[4];panel.GetWorldCorners(corners);
                Check(corners.All(p=>RectTransformUtility.RectangleContainsScreenPoint(root,p,null)),"Dialogue panel fits screen: "+c.Item2);
                var textRect=GameObject.Find("DialogueTextViewport").GetComponent<RectTransform>();
                var textBounds=new Vector3[4];textRect.GetWorldCorners(textBounds);
                foreach(var button in panel.GetComponentsInChildren<Button>())
                {
                    var b=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(b);
                    if(button.name=="DialogueCloseX")
                    {
                        Check(!RectTransformUtility.RectangleContainsScreenPoint(textRect,(b[0]+b[2])*.5f,null),"Close control center is outside reading area: "+c.Item2);
                        continue; // The existing X touch padding can extend below its visible artwork.
                    }
                    Check(b[2].y<=textBounds[0].y||b[0].y>=textBounds[2].y,"Text does not overlap button: "+c.Item2+"/"+button.name);
                }
                Check(JsonUtility.ToJson(game.Save)==expected,"Reading dialogue grants nothing: "+c.Item2);
                if(!restart)
                {
                    // Closing before accepting the gift must remain read-only.
                    yield return Click("EventClose",true);
                    Check(JsonUtility.ToJson(game.Save)==expected&&!Map.DialogueOpen,"Close before gift preserves state and restores map: "+c.Item2);
                    Map.Interact();yield return Until(()=>Map.DialogueOpen,"reopen NPC "+c.Item2,8);
                    yield return new WaitForSecondsRealtime(.4f);
                    yield return Snapshot("ThreeNpc-Dialogue-"+c.Item2);
                    yield return Click("EventOption_talk",true);
                    Check(game.Save.coins==coins+c.Item5&&game.Save.items.Count(i=>i=="healing_tea")==teas+c.Item4,"Exact original reward through NPC talk: "+c.Item2);
                    Check(GameObject.Find("ReceivedAmount_healing_tea")?.GetComponent<Text>().text=="× "+c.Item4,"Tea receipt has exact amount: "+c.Item2);
                    Check(c.Item5==0?GameObject.Find("ReceivedAmount_coin")==null:GameObject.Find("ReceivedAmount_coin")?.GetComponent<Text>().text=="× "+c.Item5,"Coin receipt has exact amount: "+c.Item2);
                    var granted=JsonUtility.ToJson(game.Save);
                    for(var repeat=0;repeat<8;repeat++)game.Events.Execute(game.Save.node,"talk");
                    Check(JsonUtility.ToJson(game.Save)==granted,"Rapid stale callbacks while receipt is open cannot duplicate reward: "+c.Item2);
                    yield return Click("ReceiptClose",true);
                    Check(game.Events.Resolved(e)&&NpcStoryState()==story,"Original ledger resolves without story/quest changes: "+c.Item2);
                    yield return CloseEvent();Map.Interact();yield return Until(()=>Map.DialogueOpen,"return NPC "+c.Item2,8);yield return new WaitForSecondsRealtime(.4f);
                    Check(GameObject.Find("EventText").GetComponent<Text>().text.Contains(e.completedText),"Return greeting replaces initial gift offer: "+c.Item2);
                }
                else Check(game.Events.Resolved(e),"Separate process reload keeps reward resolved: "+c.Item2);
                expected=JsonUtility.ToJson(game.Save);
                Check(!game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="collect"),"No old chest collection offered: "+c.Item2);
                game.Events.Execute(game.Save.node,"talk");game.Events.Execute(game.Save.node,"collect");
                Check(JsonUtility.ToJson(game.Save)==expected,"Repeated/stale talk and collect cannot duplicate reward: "+c.Item2);
                Check(FindButton("ReceiptContinue")==null,"Return/reload does not replay a reward receipt: "+c.Item2);
                yield return CloseEvent();
                Check(!Map.DialogueOpen&&Map.View.InputEnabled,"Panel closes and restores map controls: "+c.Item2);
                var index=game.Save.node;
                var adjacent=Enumerable.Range(0,game.Data.nodes.Length).Where(i=>i!=index&&game.Discovery.IsVisible(i))
                    .Select(i=>new{index=i,path=game.Data.FindPath(index,i,game.Save)})
                    .Where(p=>p.path!=null&&p.path.Count==2).FirstOrDefault();
                Check(adjacent!=null,"Original graph still has a reachable neighbor: "+c.Item2);
                Time.timeScale=5;yield return Travel(adjacent.index,true);yield return CloseEvent();yield return Travel(index,true);Time.timeScale=1;
                yield return CloseEvent();Check(game.Save.node==index&&Map.NpcApproachFailure==null,"Lumi moves out and returns on original routes: "+c.Item2);
            }
            if(!restart)
            {
                game.Persist();File.Copy(savePath,Path.Combine(output,"earned-after.json"),true);
                // Legacy claimed-save fixtures use the exact pre-change event keys.
                // S12 is already claimed in the actual source; G16/G21 are explicit fixtures.
                var legacy=JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(output,"legacy-source.json")));
                foreach(var key in new[]{"gyeongju:loot_lake","gyeongju:secret_herb","seoul:loot_park"})
                {if(!legacy.resolvedEvents.Contains(key))legacy.resolvedEvents.Add(key);if(!legacy.rewardGrantIds.Contains("event:"+key+":healing_tea"))legacy.rewardGrantIds.Add("event:"+key+":healing_tea");}
                File.WriteAllText(Path.Combine(output,"legacy-claimed-fixture.json"),JsonUtility.ToJson(legacy));
                yield return UXFixture(JsonUtility.ToJson(legacy));
                foreach(var c in cases)
                {
                    yield return CrossEnter(c.Item1);yield return CloseEvent();var i=NodeIndex(c.Item2);
                    var e=game.Content.events.Single(x=>x.cityId==c.Item1&&x.nodeId==c.Item2&&!x.additive);
                    var before=JsonUtility.ToJson(game.Save);
                    Check(game.Events.Resolved(e)&&game.Events.BuildOptions(i).text.Contains(e.completedText),"Old claimed-save key selects return greeting: "+c.Item2);
                    game.Events.Execute(i,"talk");game.Events.Execute(i,"collect");
                    Check(JsonUtility.ToJson(game.Save)==before,"Old claimed save receives no additional items/coins: "+c.Item2);
                }
                yield return UXFixture(File.ReadAllText(Path.Combine(output,"earned-after.json")));game.Persist();
            }
            else
            {
                var earned=JsonUtility.FromJson<SaveData>(File.ReadAllText(Path.Combine(output,"earned-after.json")));
                Check(game.Save.coins==earned.coins&&game.Save.items.SequenceEqual(earned.items)&&game.Save.resolvedEvents.SequenceEqual(earned.resolvedEvents)&&game.Save.rewardGrantIds.SequenceEqual(earned.rewardGrantIds),"Separate Windows process retains exact inventory, coins and reward ledgers");
            }
        }
    }
}
#endif
