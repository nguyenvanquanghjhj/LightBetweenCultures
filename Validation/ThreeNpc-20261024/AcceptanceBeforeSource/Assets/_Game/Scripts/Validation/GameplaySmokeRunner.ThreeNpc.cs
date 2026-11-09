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
                Map.Interact();yield return Until(()=>Map.DialogueOpen,"NPC dialogue "+c.Item2,8);
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
                    Check(b[2].y<=textBounds[0].y||b[0].y>=textBounds[2].y,"Text does not overlap button: "+c.Item2+"/"+button.name);
                }
                Check(JsonUtility.ToJson(game.Save)==expected,"Reading dialogue grants nothing: "+c.Item2);
                if(!restart)
                {
                    yield return Snapshot("ThreeNpc-"+c.Item2);
                    yield return Click("EventOption_talk",true);
                    Check(game.Save.coins==coins+c.Item5&&game.Save.items.Count(i=>i=="healing_tea")==teas+c.Item4,"Exact original reward through NPC talk: "+c.Item2);
                    Check(game.Events.Resolved(e)&&NpcStoryState()==story,"Original ledger resolves without story/quest changes: "+c.Item2);
                    yield return CloseEvent();Map.Interact();yield return new WaitForSecondsRealtime(.4f);
                    Check(GameObject.Find("EventText").GetComponent<Text>().text.Contains(e.completedText),"Return greeting replaces initial gift offer: "+c.Item2);
                }
                else Check(game.Events.Resolved(e),"Separate process reload keeps reward resolved: "+c.Item2);
                expected=JsonUtility.ToJson(game.Save);
                Check(!game.Events.BuildOptions(game.Save.node).options.Any(o=>o.id=="collect"),"No old chest collection offered: "+c.Item2);
                game.Events.Execute(game.Save.node,"talk");game.Events.Execute(game.Save.node,"collect");
                Check(JsonUtility.ToJson(game.Save)==expected,"Repeated/stale talk and collect cannot duplicate reward: "+c.Item2);
                yield return CloseEvent();
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
