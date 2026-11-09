#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator SLDrag(Vector2 start,Vector2 end,bool finger=true,float duration=.25f)
        {
            if(finger)InputSystem.QueueStateEvent(touch,new TouchState{touchId=31,phase=UnityEngine.InputSystem.TouchPhase.Began,position=start});
            else{InputSystem.QueueStateEvent(mouse,new MouseState{position=start});yield return null;InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));}
            yield return null;yield return null;
            for(var i=1;i<=16;i++){var p=Vector2.Lerp(start,end,i/16f);if(finger)InputSystem.QueueStateEvent(touch,new TouchState{touchId=31,phase=UnityEngine.InputSystem.TouchPhase.Moved,position=p});else InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left));yield return new WaitForSecondsRealtime(duration/16);}
            if(finger)InputSystem.QueueStateEvent(touch,new TouchState{touchId=31,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=end});else InputSystem.QueueStateEvent(mouse,new MouseState{position=end});yield return null;yield return null;
        }
        Vector2 SLPoint(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        IEnumerator SLHold()
        {
            var point=SLPoint((RectTransform)FindButton("MicroHold").transform);
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=32,phase=UnityEngine.InputSystem.TouchPhase.Began,position=point});
            if(Environment.GetCommandLineArgs().Contains("--game-feel"))
            {
                yield return new WaitForSecondsRealtime(.45f);
                Check(GameObject.Find("ActivityFeedback").GetComponent<Text>().text.Contains("%")&&!game.Puzzles.Activity.Ready,"Holding shows actual partial stamp progress before success");
                yield return Snapshot("GF-stamp-live-progress");yield return new WaitForSecondsRealtime(1.05f);
            }
            else yield return new WaitForSecondsRealtime(1.5f);
            InputSystem.QueueStateEvent(touch,new TouchState{touchId=32,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=point});yield return null;yield return null;
        }
        IEnumerator SolveRevisedPuzzle(string id)
        {
            var spec=ActivityPresentation.For(id);var definition=game.Content.Puzzle(id);var items=game.Save.items.ToArray();var coins=game.Save.coins;var old=definition.rewardItems.Select(i=>game.Inventory.Count(i.itemId)).ToArray();
            if(!PcFull)SLPrerequisites(definition);
            if(Environment.GetCommandLineArgs().Contains("--final-polish-story")&&spec.kind=="LIGHT_STORY_RESOLUTION")yield return Snapshot("FP-"+id+"-action");
            yield return ChapterChoice("puzzle");
            if(spec.kind=="LIGHT_STORY_RESOLUTION")
                Check(GameObject.Find("PuzzlePanel")==null&&GameObject.Find("StoryActivityPanel")==null&&game.Puzzles.Solved(id),"Intentional story action commits old proof without puzzle UI: "+id);
            else
            {
                Check(game.Puzzles.Active&&game.Puzzles.Activity!=null&&!game.Puzzles.Solved(id),"Real activity opens without granting proof: "+id);
                yield return CloseEvent();Check(!game.Puzzles.Solved(id)&&game.Save.items.SequenceEqual(items)&&game.Save.coins==coins,"Cancel before success preserves items/proofs: "+id);yield return Click("Interact",true);yield return ChapterChoice("puzzle");
                yield return Click("MicroConfirm",true);Check(!game.Puzzles.Solved(id)&&game.Save.items.SequenceEqual(items),"Empty confirmation cannot pay or consume: "+id);
                var a=game.Puzzles.Activity;
                for(var i=0;i<a.inspected.Length;i++)yield return Click("MicroInspect"+i,true);
                for(var i=0;i<a.placed.Length;i++)
                {
                    var piece=GameObject.Find("MicroPiece"+i).GetComponent<RectTransform>();var target=GameObject.Find("MicroTarget"+i).GetComponent<RectTransform>();Canvas.ForceUpdateCanvases();
                    if(i==0){yield return SLDrag(SLPoint(piece),SLPoint(target)+new Vector2(-125,90));Check(!a.placed[i],"Wrong drop rejected: "+id);}
                    if(i%2==0)
                    {
                        // Use the held-drag adapter already verified by FinalUX;
                        // observe the snap before sending the release event.
                        var near=RectTransformUtility.WorldToScreenPoint(null,target.TransformPoint(new Vector3(-42,45)));
                        yield return HueHeldDrag(piece,near,Vector2.zero,true);
                        Check(a.placed[i],"Held drag reaches actual matching slot: "+id+" "+i);
                        yield return HueRelease(near,true);
                    }
                    else{yield return Click("MicroPiece"+i,true);yield return Click("MicroTarget"+i,true);}
                    Check(a.placed[i],"Touch drag / tap-select places actual piece: "+id+" "+i);
                    while(a.turns[i]!=spec.targetTurns[i])yield return Click("MicroRotate"+i,true);
                }
                for(var i=0;i<a.dials.Length;i++)while(a.dials[i]!=spec.dialTargets[i])yield return Click("MicroDial"+i,true);
                if(spec.gesture=="hold")yield return SLHold();
                else if(!string.IsNullOrEmpty(spec.gesture))
                {
                    var slider=GameObject.Find("MicroSwipe").GetComponent<Slider>();var rt=(RectTransform)slider.transform;Canvas.ForceUpdateCanvases();
                    yield return SLDrag(SLPoint(slider.handleRect),RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(new Vector2(rt.rect.xMax-1,0))));
                }
                Check(a.Ready&&!game.Puzzles.Solved(id),"Prepared tactile state alone never auto-commits: "+id);
                if(Environment.GetCommandLineArgs().Contains("--final-polish-story")){P6Bounds("MicroConfirm");P6Bounds("ActivityClose");yield return Snapshot("FP-"+id+"-ready");}
                if(id=="puzzle_paper"||id=="puzzle_relay_jeonju")yield return Snapshot("SL-"+id+"-ready");
                yield return Click("MicroConfirm",true);Check(game.Puzzles.Solved(id),"Explicit interaction success records original PuzzleID: "+id);
            }
            Check(game.Save.coins==coins+definition.rewardCoins,"Original coin reward exact: "+id);
            for(var i=0;i<definition.rewardItems.Length;i++)
            {var reward=definition.rewardItems[i];var expected=game.Content.Item(reward.itemId)?.stackLimit==1?Math.Max(old[i],1):old[i]+reward.amount;Check(game.Inventory.Count(reward.itemId)==expected&&game.Save.rewardGrantIds.Count(k=>k=="puzzle:"+id+":"+reward.itemId)==1,"Original item and one-time ledger exact: "+id+" "+reward.itemId);}
            Check((definition.flags??Array.Empty<string>()).All(game.World.Has),"Original flags retained: "+id);
            yield return CloseEvent();var after=JsonUtility.ToJson(game.Save);Check(!game.Puzzles.Start(id)&&JsonUtility.ToJson(game.Save)==after,"Solved ID cannot replay reward: "+id);
        }
        void SLPrerequisites(PuzzleDefinition p)
        {
            // Bounded missing-proof counterexamples at an earned node. Never persisted.
            var saved=JsonUtility.ToJson(game.Save);
            foreach(var flag in p.requiredFlags??Array.Empty<string>())
            {
                var flags=game.Save.flags.ToArray();var accepted=game.Save.accepted.ToArray();var completed=game.Save.completed.ToArray();var solved=game.Save.solvedPuzzles.ToArray();
                try
                {
                    game.Save.flags.RemoveAll(f=>f==flag);
                    if(flag.StartsWith("quest:")){var q=flag.Substring(6);game.Save.accepted.Remove(q);game.Save.completed.Remove(q);}
                    if(flag.StartsWith("puzzle:"))game.Save.solvedPuzzles.Remove(flag.Substring(7));
                    if(!game.World.Has(flag))Check(!game.Puzzles.Start(p.id)&&!game.Events.BuildOptions(game.Save.node).options.First(o=>o.id=="puzzle").enabled,"Missing required proof rejects both UI/backend: "+p.id+" "+flag);
                }
                finally{game.Puzzles.Cancel();game.Save.flags.Clear();game.Save.flags.AddRange(flags);game.Save.accepted.Clear();game.Save.accepted.AddRange(accepted);game.Save.completed.Clear();game.Save.completed.AddRange(completed);game.Save.solvedPuzzles.Clear();game.Save.solvedPuzzles.AddRange(solved);}
            }
            foreach(var item in p.requiredItems??Array.Empty<ItemAmount>())
            {
                var inventory=game.Save.items.ToArray();try{game.Save.items.RemoveAll(x=>x==item.itemId);Check(!game.Puzzles.Start(p.id),"Missing item cannot solve original ID: "+p.id+" "+item.itemId);}
                finally{game.Puzzles.Cancel();game.Save.items.Clear();game.Save.items.AddRange(inventory);}
            }
            Check(JsonUtility.ToJson(game.Save)==saved,"Negative guard checks restore exact earned state: "+p.id);
        }
        string SLCore()
        {var copy=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(game.Save));copy.leisureCompleted.Clear();return JsonUtility.ToJson(copy);}
        IEnumerator SLBeginLeisure(LeisureDefinition def)
        {
            if(def.mechanic=="pick"||def.mechanic=="water"||def.mechanic=="dance")for(var i=0;i<3;i++)yield return Click("LeisureTarget"+i,true);
            else if(def.mechanic=="wish"||def.mechanic=="release"){yield return Click("LeisureAction",true);yield return Click("LeisureTarget0",true);}
            else if(def.mechanic=="fly")
            {
                var point=SLPoint((RectTransform)FindButton("LeisureAction").transform);InputSystem.QueueStateEvent(touch,new TouchState{touchId=32,phase=UnityEngine.InputSystem.TouchPhase.Began,position=point});yield return new WaitForSecondsRealtime(1.5f);InputSystem.QueueStateEvent(touch,new TouchState{touchId=32,phase=UnityEngine.InputSystem.TouchPhase.Ended,position=point});yield return null;
            }
            else yield return Click("LeisureAction",true);
        }
        IEnumerator SLLeisure(string city,string node,bool replay=true)
        {
            yield return CrossEnter(city);Time.timeScale=6;yield return ChapterGo(node);Time.timeScale=1;yield return CloseEvent();
            var def=LeisurePresentation.At(city,node);Check(def!=null,"Approved activity bound to existing node: "+node);
            var prop=GameObject.Find("LeisureProp_"+def.id).GetComponent<Image>();Check(prop.sprite==def.Sprite&&!prop.raycastTarget&&prop.GetComponent<Button>()==null&&prop.GetComponent<Collider2D>()==null,"Static map prop has real sprite and no input/collider: "+node);
            var core=SLCore();var camera=Map.View.Content.anchoredPosition;var zoom=Map.View.Zoom;var anchor=Map.Marker.anchorMin;
            yield return Snapshot("SL-"+node+"-static-prop");
            yield return Click("Interact",true);yield return ChapterChoice("leisure:"+def.id);
            Check(Map.DialogueOpen&&!Map.View.InputEnabled&&!game.Save.leisureCompleted.Contains(def.id),"Separate activity window blocks map and does not win on open: "+node);
            yield return SLBeginLeisure(def);yield return new WaitForSecondsRealtime(.4f);yield return CloseEvent();
            Check(!game.Save.leisureCompleted.Contains(def.id)&&SLCore()==core,"Early close preserves exact story/reward state, no optional clear: "+node);
            for(var run=0;run<(replay?2:1);run++)
            {
                yield return Click("Interact",true);yield return ChapterChoice("leisure:"+def.id);yield return SLBeginLeisure(def);
                yield return new WaitForSecondsRealtime(1.2f);if(run==0)yield return Snapshot("SL-"+node+"-Lumi-prototype");
                yield return Until(()=>!FindAnyObjectByType<LeisureActivityView>().Playing,"short activity finishes",8);yield return CloseEvent();
                Check(game.Save.leisureCompleted.Count(x=>x==def.id)==1&&SLCore()==core,"Optional first-clear unique; replay never pays story rewards: "+node);
            }
            Check(Map.View.Content.anchoredPosition==camera&&Mathf.Abs(Map.View.Zoom-zoom)<.001f&&Map.Marker.anchorMin==anchor&&Map.View.InputEnabled,"Same map/node/arrival/camera/zoom restored: "+node);
        }
        void SLSaveRecovery()
        {
            var original=SaveSystem.OverridePath;var dir=Path.Combine(output,"save-recovery");Directory.CreateDirectory(dir);
            var earned=File.ReadAllText(savePath);var data=JsonUtility.FromJson<SaveData>(earned);var old=earned.Replace("\"leisureCompleted\"","\"unusedOptionalField\"");
            try
            {
                SaveSystem.OverridePath=Path.Combine(dir,"old.json");File.WriteAllText(SaveSystem.PathName,old);var loaded=SaveSystem.Load(23);
                Check(loaded.leisureCompleted.Count==0&&loaded.solvedPuzzles.SequenceEqual(data.solvedPuzzles)&&loaded.items.SequenceEqual(data.items),"Old save without optional history retains exact puzzle/items, zero blanket grants");
                SaveSystem.OverridePath=Path.Combine(dir,"backup.json");File.WriteAllText(SaveSystem.PathName,"{broken");File.WriteAllText(SaveSystem.PathName+".bak",earned);loaded=SaveSystem.Load(23);
                Check(!SaveSystem.RecoveryBlocked&&SaveSystem.RecoverySource.EndsWith(".bak")&&loaded.solvedPuzzles.SequenceEqual(data.solvedPuzzles),"Corrupt main recovers valid backup");
                Check(SaveSystem.Write(loaded)&&File.ReadAllText(SaveSystem.PathName+".bak")==earned&&Directory.GetFiles(dir,"*.unreadable-*").Length==1,"Recovered write preserves valid backup and quarantines corrupt primary");
                SaveSystem.OverridePath=Path.Combine(dir,"interrupted.json");File.WriteAllText(SaveSystem.PathName,"broken");File.WriteAllText(SaveSystem.PathName+".bak","broken-backup");File.WriteAllText(SaveSystem.PathName+".tmp",earned);loaded=SaveSystem.Load(23);
                Check(!SaveSystem.RecoveryBlocked&&SaveSystem.RecoverySource.EndsWith(".tmp")&&SaveSystem.Write(loaded)&&File.ReadAllText(SaveSystem.PathName+".tmp")==earned,"Interrupted-write recovery preserves only valid temporary save");
                SaveSystem.OverridePath=Path.Combine(dir,"all-corrupt.json");File.WriteAllText(SaveSystem.PathName,"broken");File.WriteAllText(SaveSystem.PathName+".bak","broken-backup");loaded=SaveSystem.Load(23);
                Check(SaveSystem.RecoveryBlocked&&!SaveSystem.Write(loaded)&&File.ReadAllText(SaveSystem.PathName)=="broken"&&File.ReadAllText(SaveSystem.PathName+".bak")=="broken-backup","All corrupt blocks writes without overwriting recoverable evidence");
                SaveSystem.OverridePath=Path.Combine(dir,"v1.json");var v1=old.Replace("\"version\": 2","\"version\": 1");File.WriteAllText(SaveSystem.PathName,v1);loaded=SaveSystem.Load(23);
                Check(loaded.version==2&&loaded.items.SequenceEqual(data.items)&&loaded.leisureCompleted.Count==0,"v1 optional initialization preserves old inventory/proofs");
            }
            finally{SaveSystem.OverridePath=original;SaveSystem.Load(game.Data.nodes.Length);}
        }
        IEnumerator SLPortraits()
        {
            yield return CrossEnter("hoi_an");yield return ChapterGo("H9");yield return CloseEvent();
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
            {
                yield return P6Resolution(size.x,size.y);var saved=JsonUtility.ToJson(game.Save);yield return Click("ObjectiveHint",true);
                for(var level=0;level<3;level++){Check(GameObject.Find("GuidanceText").GetComponent<Text>().text==game.Quests.Guidance(level),"Real-state hint level "+level+" at "+size);P6Bounds("ActivityClose");if(level<2)yield return Click("GuidanceMore",true);}
                yield return CloseEvent();Check(JsonUtility.ToJson(game.Save)==saved,"Three hints never mutate progress "+size);
                yield return Click("Interact",true);yield return ChapterChoice("leisure:leisure_hoian_swing");P6Bounds("LeisureAction");P6Bounds("ActivityClose");P6Bounds("LeisureScene");
                yield return Snapshot("SL-portrait-"+size.x+"x"+size.y);yield return CloseEvent();yield return TestMapInput();
            }
        }
        IEnumerator StoryLeisureJourney()
        {
            musicVirtualFocus=true;
            var args=Environment.GetCommandLineArgs();
            if(args.Contains("--story-leisure-captures"))
            {
                yield return CrossEnter("hoi_an");Time.timeScale=4;yield return ChapterGo("H9");
                var chest=game.Events.BuildOptions(game.Save.node).options.FirstOrDefault(o=>o.id=="collect");
                if(chest!=null&&chest.enabled){yield return ChapterChoice("collect");Check(game.Events.Resolved(game.Events.Definition(game.Data.nodes[game.Save.node].eventId)),"H9 original chest still resolves alongside secondary leisure option");}
                yield return CloseEvent();Time.timeScale=1;yield return Click("QuestJournal",true);yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();
                GameObject.Find("JournalViewport").GetComponent<ScrollRect>().verticalNormalizedPosition=0;yield return Snapshot("SL-journal-optional-0of12");yield return Click("CloseJournal",true);
                phase4CityOverride="seoul";yield return Phase4CityJourney();KeepSave("SL-captures-earned");yield break;
            }
            if(args.Contains("--story-leisure-e2e"))
            {
                yield return CrossEnter("hoi_an");yield return Click("QuestJournal",true);yield return Snapshot("SL-initial-journal-0of12");yield return Click("CloseJournal",true);
                yield return Phase4E2E();
                yield return CrossEnter("gyeongju");Time.timeScale=4;yield return ChapterPuzzle("G2");
                if(!game.Puzzles.Solved("puzzle_cracked_seal")){yield return ChapterGo("G12");yield return ChapterChoice("accept:gq02_offer_finish");yield return ChapterPuzzle("G12");}
                yield return ChapterGo("G17");yield return ChapterChoice("accept:gq04_offer_finish");yield return ChapterPuzzle("G17");yield return ChapterGo("G17");yield return ChapterChoice("complete:gq04_offer_finish");Time.timeScale=1;
                Check(game.Content.puzzles.All(p=>game.Puzzles.Solved(p.id)),"Both actual story routes reach all22 preserved proofs through intentional interactions");KeepSave("SL-all22-earned");yield break;
            }
            if(args.Contains("--story-leisure-recheck"))
            {
                foreach(var pair in new[]{("hoi_an","H16"),("hue","U8"),("da_lat","D13"),("da_lat","D9"),("seoul","S1")})yield return SLLeisure(pair.Item1,pair.Item2);
                yield return Click("Interact",true);var def=LeisurePresentation.At(game.Data.cityId,game.Data.nodes[game.Save.node].id);yield return ChapterChoice("leisure:"+def.id);yield return SLBeginLeisure(def);yield return new WaitForSecondsRealtime(4.4f);
                yield return Click("LeisureAction",true);yield return SLBeginLeisure(def);yield return Until(()=>!FindAnyObjectByType<LeisureActivityView>().Playing,"Same-panel replay finishes",8);yield return CloseEvent();
                Check(game.Save.leisureCompleted.Count(x=>x==def.id)==1,"Same-panel replay resets local state without adding duplicate optional record");KeepSave("SL-rechecked");yield break;
            }
            if(args.Contains("--story-leisure-all"))
            {
                Check(game.Content.puzzles.All(p=>game.Puzzles.Solved(p.id)),"Leisure coexistence starts from earned old22-proof save, no injected flags");SLSaveRecovery();
                foreach(var e in LeisurePresentation.Load().entries)yield return SLLeisure(e.cityId,e.nodeId);
                Check(game.Save.leisureCompleted.Distinct().Count()==12,"All12 optional events complete independently of22 old proofs");KeepSave("SL-all12-optional");yield return SLPortraits();KeepSave("SL-final");
                var saved=JsonUtility.ToJson(game.Save);var resetBytes=File.ReadAllBytes(savePath);game.Scenes.Go("Prototype");yield return Scene("Prototype");
                yield return Click("Reset",true);yield return Snapshot("SL-final-reset-warning");yield return Click("CancelReset",true);Check(File.ReadAllBytes(savePath).SequenceEqual(resetBytes)&&JsonUtility.ToJson(game.Save)==saved,"Reset Cancel preserves earned six-city story and12 activities byte for byte");
                yield return Click("Reset",true);yield return Click("ConfirmReset",true);Check(game.Save.completed.Count==0&&game.Save.items.Count==0&&game.Save.leisureCompleted.Count==0&&game.Save.solvedPuzzles.Count==0,"Only second explicit confirmation resets all categories in isolated QA save");
                File.WriteAllText(savePath,saved);game.Reload();Check(JsonUtility.ToJson(game.Save)==saved,"Destructive reset QA restores its own earned copy; user save untouched");yield break;
            }
            if(args.Contains("--story-leisure-props")){yield return SLLeisure("hoi_an","H9");KeepSave("SL-optional-clear");yield break;}
            if(args.Contains("--story-leisure-restart"))
            {var before=JsonUtility.ToJson(game.Save);yield return CrossEnter(game.Save.cityId);Check(game.Save.leisureCompleted.Count>0&&game.Save.solvedPuzzles.Count>0,"New process restores optional history and original puzzle proofs");var coins=game.Save.coins;game.Persist();game.Reload();Check(game.Save.coins==coins,"Restart never grants rewards");yield break;}
            yield return CrossEnter("hoi_an");Time.timeScale=4;yield return Travel(2,true);yield return Click("EventOption_accept",true);yield return Click("DialogueNext");yield return CloseEvent();
            Time.timeScale=1;yield return Travel(3,true);yield return ExploreH3();Time.timeScale=4;
            yield return Travel(2,true);yield return Click("EventOption_tutorial_return",true);yield return CloseEvent();
            yield return ChapterGo("H13");var blocked=game.Events.BuildOptions(game.Save.node).options.First(o=>o.id=="puzzle");Check(!blocked.enabled&&!game.Puzzles.Start("puzzle_paper"),"H13 requires real H4 proof, no arrival auto-grant");yield return CloseEvent();
            yield return ChapterPuzzle("H4");yield return ChapterPuzzle("H13");
            Check(game.Inventory.Count("hanji_core")==1&&!game.Quests.IsCompleted("HQ01")&&!game.IsCityUnlocked("hue"),"Paper success alone cannot bypass H5/H7");
            yield return ChapterPuzzle("H19");yield return ChapterGo("H5");yield return ChapterChoice("alternate");yield return CloseEvent();Check(game.Inventory.Count("link_key")==1,"Original H19/H5 hack path grants one key");
            yield return ChapterGo("H7");yield return ChapterChoice("restore");yield return CloseEvent();Time.timeScale=1;
            Check(game.Quests.IsCompleted("HQ01")&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju")&&game.Inventory.Count("link_key")==0,"Genuinely earned H4/H13/H5/H7 retains HQ01 and world unlock");KeepSave("SL-genuinely-earned-H7");
            yield return SLLeisure("hue","U0");KeepSave("SL-pilot-complete");
            game.Scenes.Go("Prototype");yield return Scene("Prototype");var state=JsonUtility.ToJson(game.Save);var bytes=File.ReadAllBytes(savePath);
            yield return Click("Reset",true);yield return Snapshot("SL-reset-warning");yield return Click("CancelReset",true);
            Check(JsonUtility.ToJson(game.Save)==state&&File.ReadAllBytes(savePath).SequenceEqual(bytes),"Reset Cancel leaves all six-city progress and save bytes unchanged");
        }
    }
}
#endif
