#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        GameplayPanels UXPanels=>(GameplayPanels)typeof(CityMap).GetField("panels",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Map);
        IEnumerator UXFixture(string json)
        {
            game.Scenes.Go("Prototype");yield return Scene("Prototype");
            File.WriteAllText(savePath,json);game.Reload();yield return null;
        }
        IEnumerator FinalUXJourney()
        {
            musicVirtualFocus=true;var earned=JsonUtility.ToJson(game.Save);
            if(!Environment.GetCommandLineArgs().Contains("--final-ux-tail"))
            {
            var blank=JsonUtility.ToJson(new SaveData());PlayerPrefs.SetInt(PlayerCopy.GuideKey,1);
            yield return UXFixture(blank);
            Check(game.Content.cities.Length==6&&game.Content.cities.Sum(c=>c.nodes.Length)==133,"All six cities / 133 canonical nodes are loaded");
            foreach(var c in game.Content.cities)
            {
                Check(c.runtimeReady&&AdventureUIArt.Load().City(c.id)?.sprite!=null,"Runtime and map artwork exist: "+c.id);
                if(c.id!="hoi_an")Check(!game.IsCityUnlocked(c.id)&&!game.EnterCity(c.id),"Fresh save cannot bypass world unlock: "+c.id);
            }
            Check(game.Quests.CityTravelHint("hue").Contains("Hội An")&&game.Quests.CityTravelHint("da_lat").Contains("Huế")&&game.Quests.CityTravelHint("gyeongju").Contains("Jeonju")&&game.Quests.CityTravelHint("seoul").Contains("Đà Lạt"),"Locked destinations explain actual prerequisite cities");
            var cityRows=new System.Collections.Generic.List<string>();
            foreach(var c in game.Content.cities)
            {
                // Focused arrival fixture, not an automated campaign. Only the
                // canonical predecessor flags are supplied in a private save.
                var s=new SaveData();s.flags.AddRange(c.unlockFlags??Array.Empty<string>());
                yield return UXFixture(JsonUtility.ToJson(s));yield return CrossEnter(c.id);yield return CloseEvent();
                var mapImage=GameObject.Find(c.id=="hoi_an"?"HoiAnMapBackground":"CityMapBackground").GetComponent<Image>();
                Check(mapImage.sprite==(c.id=="hoi_an"?DesignKitArt.Load().hoiAnMap:AdventureUIArt.Load().City(c.id).sprite),"Own map art rendered: "+c.id);
                Check(game.Data.nodes.Length==(c.id=="seoul"?23:22),"Correct node count: "+c.id);
                var goal=game.Quests.Guidance();var target=game.Quests.GuidanceTarget;
                Check(target>=0&&game.Discovery.IsVisible(target)&&goal.Contains(game.Data.nodes[target].name),"Arrival guidance names a visible actionable place: "+c.id+" / "+goal);
                Check(LeisurePresentation.Load().entries.Count(e=>e.enabled&&e.cityId==c.id)==2,"Two existing cultural activities retained: "+c.id);
                var save=JsonUtility.ToJson(game.Save);yield return Click("ObjectiveHint",true);yield return null;
                var body=GameObject.Find("GuidanceText").GetComponent<Text>();
                Check(body.text.Contains(goal)&&body.text.Contains(game.Quests.Guidance(1)),"Goal, place and known requirements appear together: "+c.id);
                yield return Click("GuidanceMore",true);
                Check(FindButton("GuidanceMore")==null&&GameObject.Find("GuidanceText").GetComponent<Text>().text.Contains(game.Quests.Guidance(2)),"One optional hint expansion, no extra location-only page: "+c.id);
                if(c.id=="hue"||c.id=="seoul")yield return Snapshot("UX-guidance-"+c.id);
                yield return Click("ActivityClose",true);Check(JsonUtility.ToJson(game.Save)==save,"Guidance read/close grants and consumes nothing: "+c.id);
                // Check every NPC menu in a fully revealed fixture below too;
                // no mutation is allowed while deriving or pruning the view.
                cityRows.Add(c.id+" | arrival="+goal+" | cultural activities=2 | PASS");
            }
            yield return UXFixture(earned);
            int menus=0,removed=0;
            foreach(var c in game.Content.cities)
            {
                yield return CrossEnter(c.id);yield return CloseEvent();
                foreach(var e in game.Content.events.Where(e=>e.cityId==c.id&&e.type=="npc"&&!e.additive))
                {
                    var i=NodeIndex(e.nodeId);if(i<0)continue;var save=JsonUtility.ToJson(game.Save);
                    var v=game.Events.BuildOptions(i);var ids=v.options.Select(o=>o.id).ToArray();
                    game.Events.PruneRepeatedOptions(i,v,e);
                    var gone=ids.Except(v.options.Select(o=>o.id)).ToArray();
                    Check(gone.All(id=>id=="talk"||id=="rest"||id.StartsWith("progress:")),"Only proven information/rest duplicates pruned: "+c.id+":"+e.nodeId);
                    Check(JsonUtility.ToJson(game.Save)==save,"Menu audit never accepts/delivers/rewards: "+c.id+":"+e.nodeId);menus++;removed+=gone.Length;
                }
            }
            cityRows.Add("NPC menus audited="+menus+"; repeated information options suppressed="+removed);
            File.WriteAllLines(Path.Combine(output,"six-city-results.txt"),cityRows);
            // A coin change previously left Guidance cached by identical list counts.
            yield return UXFixture(earned);yield return CrossEnter("hoi_an");yield return CloseEvent();
            game.Save.completed.Remove("HQ04");game.Save.flags.Remove("hoi_an.ferry_open");
            if(!game.Save.accepted.Contains("HQ04"))game.Save.accepted.Add("HQ04");
            game.Save.coins=0;game.Quests.Guidance();var cache=typeof(QuestManager).GetField("guidance",BindingFlags.Instance|BindingFlags.NonPublic);var old=cache.GetValue(game.Quests);
            game.Save.coins=5;game.Quests.Guidance();Check(!ReferenceEquals(old,cache.GetValue(game.Quests)),"Coin changes invalidate guidance without changing any list count");
            game.Save.flags.Add("qa.old");game.Quests.Guidance();old=cache.GetValue(game.Quests);
            game.Save.flags[game.Save.flags.IndexOf("qa.old")]="qa.new";game.Quests.Guidance();Check(!ReferenceEquals(old,cache.GetValue(game.Quests)),"Replacing a flag with the same list length invalidates guidance");
            game.Save.accepted.Clear();game.Save.accepted.Add("HQ05");game.Save.completed.Remove("HQ05");
            game.World.Set("relic.restored.relic_hoi_an_lantern");game.Inventory.Add("hoi_an_fresh_chili",1);
            Check(game.Quests.GuidanceCity=="jeonju"&&game.Quests.Guidance().Contains("Jeonju")&&game.Quests.GuidanceTarget<0,"Ready cross-city delivery points to destination city, not old giver");
            // Real input through every existing story workbench, with earned
            // prerequisites retained but this puzzle reset only in the fixture.
            foreach(var d in ActivityPresentation.Load().entries.Where(d=>d.enabled&&d.kind=="STORY_MINIGAME"&&d.itemIds.Length>0))
            {
                yield return UXFixture(earned);var p=game.Content.Puzzle(d.id);
                game.Save.solvedPuzzles.Remove(p.id);game.Save.flags.Remove("puzzle:"+p.id);
                foreach(var f in p.requiredFlags)game.World.Set(f);
                foreach(var item in p.requiredItems)if(!game.Inventory.Has(item.itemId,item.amount))game.Inventory.Add(item.itemId,item.amount-game.Inventory.Count(item.itemId));
                yield return CrossEnter(p.cityId);yield return CloseEvent();
                game.Save.node=NodeIndex(p.nodeId);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return CloseEvent();
                Check(game.Puzzles.Start(p.id),"Existing puzzle starts with earned prerequisites: "+p.id);
                var a=game.Puzzles.Activity;UXPanels.ShowPuzzle(game.Save.node);yield return null;
                for(var i=0;i<a.inspected.Length;i++)yield return Click("MicroInspect"+i,true);
                var unchanged=JsonUtility.ToJson(game.Save);
                var first=GameObject.Find("MicroPiece0").GetComponent<RectTransform>();var home=first.position;
                yield return SLDrag(SLPoint(first),SLPoint(first)+Vector2.down*110,true);
                Check(!a.placed[0]&&Vector3.Distance(first.position,home)<.1f,"Wrong drop retries only current piece: "+p.id);
                for(var i=0;i<a.placed.Length;i++)
                {
                    var piece=GameObject.Find("MicroPiece"+i).GetComponent<RectTransform>();var slot=GameObject.Find("MicroTarget"+i).GetComponent<RectTransform>();
                    var near=RectTransformUtility.WorldToScreenPoint(null,slot.TransformPoint(new Vector3(-42,45)));
                    yield return HueHeldDrag(piece,near,Vector2.zero,i%2==0);
                    Check(a.placed[i]&&piece.GetComponent<RelicDragHandle>().Locked&&piece.parent==slot,"Near edge snaps during drag and locks: "+p.id+" piece "+i);
                    yield return HueRelease(near,i%2==0);
                }
                Check(JsonUtility.ToJson(game.Save)==unchanged&&!game.Puzzles.Solved(p.id),"Placement alone grants no proof/reward: "+p.id);
                Check((FindButton("MicroRotate0")!=null)==d.targetTurns.Any(t=>t!=0),"Rotation control exists exactly when a different orientation is required: "+p.id);
                if(p.id=="puzzle_archive")yield return Snapshot("UX-Hue-archive-snapped");
                yield return Click("ActivityClose",true);Check(JsonUtility.ToJson(game.Save)==unchanged,"Exit keeps quest/items/ledger unchanged: "+p.id);
            }
            // Four-piece assembly on all six relics, preserving every trial gate.
            foreach(var relic in game.Relics.Catalog.relics)
            {
                yield return UXFixture(earned);yield return CrossEnter(relic.cityId);yield return CloseEvent();
                game.Save.flags.Remove("relic.assembled."+relic.id);game.Save.flags.Remove("relic.restored."+relic.id);
                foreach(var f in relic.fragmentIds)game.World.Set("relic.fragment."+f);
                game.Save.node=NodeIndex(relic.restorationNode);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return CloseEvent();
                var save=JsonUtility.ToJson(game.Save);
                typeof(GameplayPanels).GetMethod("ShowRelicAssembly",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(UXPanels,new object[]{relic.id});yield return null;
                var piece=GameObject.Find("RelicPiece0").GetComponent<RectTransform>();var slot=GameObject.Find("RelicTarget0").GetComponent<RectTransform>();
                var near=RectTransformUtility.WorldToScreenPoint(null,slot.TransformPoint(new Vector3(-53,58)));
                yield return HueHeldDrag(piece,near,Vector2.zero,true);yield return HueRelease(near,true);
                Check(piece.GetComponent<RelicDragHandle>().Locked&&piece.parent==slot,"Relic piece accepts edge and locks: "+relic.cityId);
                yield return Click("RelicAssemble",true);Check(!game.Relics.Assembled(relic.id)&&JsonUtility.ToJson(game.Save)==save,"One piece never bypasses four-piece assembly/trial/reward: "+relic.cityId);
                yield return Click("RelicExit",true);Check(JsonUtility.ToJson(game.Save)==save,"Relic exit grants nothing: "+relic.cityId);
            }
            }
            yield return UXFixture(earned);yield return CrossEnter("seoul");yield return CloseEvent();
            var prism=game.Relics.Catalog.relics.First(r=>r.cityId=="seoul");
            foreach(var f in prism.fragmentIds)game.World.Set("relic.fragment."+f);
            game.World.Set("relic.assembled."+prism.id);game.Save.flags.Remove("relic.restored."+prism.id);
            game.Save.node=NodeIndex(prism.restorationNode);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return CloseEvent();
            typeof(GameplayPanels).GetMethod("ShowRelicAssembly",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(UXPanels,new object[]{prism.id});yield return null;
            var prismSave=JsonUtility.ToJson(game.Save);
            for(var i=0;i<3;i++)
            {
                var piece=GameObject.Find("RelicPiece"+i).GetComponent<RectTransform>();var slot=GameObject.Find("RelicTarget"+i).GetComponent<RectTransform>();
                Canvas.ForceUpdateCanvases();
                var hits=new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){position=SLPoint(piece)},hits);
                Debug.Log("UX PRISM BEGIN "+i+" at="+SLPoint(piece)+" target="+SLPoint(slot)+" hits="+string.Join(",",hits.Select(h=>h.gameObject.name)));
                yield return SLDrag(SLPoint(piece),SLPoint(slot),true);
                Debug.Log("UX PRISM END "+i+" position="+GameObject.Find("RelicPiece"+i).transform.position+" touches="+TouchInputPolicy.ActiveTouches);
                Check(GameObject.Find("RelicPiece"+i).GetComponent<RelicDragHandle>().Locked,"Prism connection snaps and remains locked after redraw: "+i);
            }
            Check(JsonUtility.ToJson(game.Save)==prismSave,"Connecting prism streams does not skip rotations, trial confirmation or rewards");
            CloseAppearancePanel(UXPanels);yield return null;
            // Actual audio bus, bounded voice count, existing mute/volume controls.
            var audio=SoftAudio.Ensure();audio.SetMuted(false);audio.SetVolumes(.4f,.2f);yield return new WaitForSecondsRealtime(.8f);
            var cues=audio.PlayedCount;SoftAudio.Action(()=>game.Save.completed.Add("qa.presentation-completion"));yield return null;
            Check(audio.PlayedCount==cues+1&&audio.LastCue==SoftCue.Puzzle_Success,"Quest completion uses one existing success cue");
            Check(audio.MusicTrack==2&&audio.CurrentMusic.clip!=null&&audio.CurrentMusic.isPlaying,"Korean map background music plays through existing music service");
            for(var i=0;i<30;i++)SoftAudio.Cue(SoftCue.UI_Click);
            Check(audio.PlayingVoices<=3,"Repeated feedback is bounded by existing voice pool");
            yield return Click("AudioSettings",true);GameObject.Find("MusicVolume").GetComponent<Slider>().value=.23f;GameObject.Find("SfxVolume").GetComponent<Slider>().value=.43f;
            Check(Mathf.Abs(audio.MusicVolume-.23f)<.01f&&Mathf.Abs(audio.SfxVolume-.43f)<.01f,"Existing volume UI controls both buses");
            yield return Click("AudioMute",true);cues=audio.PlayedCount;SoftAudio.Cue(SoftCue.Puzzle_Success);yield return null;
            Check(audio.Muted&&audio.PlayedCount==cues&&audio.PlayingVoices==0,"Mute stops/rejects effect playback");yield return Click("AudioSettingsClose",true);
            audio.SetMuted(false);game.Persist();var serialized=JsonUtility.ToJson(game.Save);game.Reload();Check(JsonUtility.ToJson(game.Save)==serialized,"Save/reload retains exact focused fixture progress and ledger");
        }
    }
}
#endif
