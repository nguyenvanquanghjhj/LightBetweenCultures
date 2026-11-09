#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        bool ChaptersEnabled=>Environment.GetCommandLineArgs().Contains("--with-chapters")||Environment.GetCommandLineArgs().Contains("--chapters-only")||FullStoryEnabled;
        bool ChapterBattleBranch=>Environment.GetCommandLineArgs().Contains("--jeonju-first");
        void CheckChapterDiscovery()
        {
            var city=game.Content.City(game.Data.cityId);
            Check(game.Data.nodes.Length==(city.id=="seoul"?23:22)&&city.runtimeNodeIds.Length==0,"Full chapter retains all canonical node IDs: "+city.id);
            var backgroundName=city.id=="hoi_an"?"HoiAnMapBackground":"CityMapBackground";
            var background=Map.View.Content.Find(backgroundName)?.GetComponent<Image>();
            var expectedBackground=city.id=="hoi_an"?DesignKitArt.Load().hoiAnMap:AdventureUIArt.Load().City(city.id)?.sprite;
            Check(background!=null&&expectedBackground!=null&&background.sprite==expectedBackground,"Correct original Map A is bound in "+city.id);
            Check(Map.Marker.GetComponent<Image>().sprite==DesignKitArt.Load().lumiMapTraveler,"City Lumi keeps its dedicated small map role");
            var stage=game.Discovery.Stage(city.id);
            foreach(var n in game.Data.nodes)
            {
                var i=NodeIndex(n.id);var visible=game.Discovery.IsVisible(i);
                var discovery=game.Discovery.DiscoveryState(i);
                Check(Map.View.Content.Find(n.id).gameObject.activeSelf==visible&&NodeMarker(n.id).gameObject.activeSelf==(discovery!=MapDiscoveryState.HIDDEN),"Actor/input/marker reveal agrees for "+n.id+" at stage "+stage);
                if(discovery==MapDiscoveryState.PREVIEW)Check(!NodeMarker(n.id).Badge.gameObject.activeSelf&&!NodeMarker(n.id).HitArea.gameObject.activeSelf,"Anonymous preview has no semantic badge or input: "+n.id);
                if(!visible)Check(FindButton(n.id)==null&&game.Data.FindPath(game.Save.node,i,game.Save)==null&&game.Events.BuildOptions(i).options.Count==0,"Hidden "+n.id+" has no route/input/event");
            }
            CheckMapActors(city.id+" stage "+stage);
            Check(FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Chapter uses one shared Canvas and EventSystem");
        }
        IEnumerator ChapterGo(string id)
        {yield return Travel(NodeIndex(id),true);if(!Map.DialogueOpen)yield return Click("Interact");}
        IEnumerator ChapterChoice(string id)
        {yield return FoodChoice(id);}
        IEnumerator ChapterConfirm(string choice)
        {yield return ChapterChoice(choice);if(FindButton("DeliveryConfirm")!=null)yield return Click("DeliveryConfirm",true);}

        IEnumerator ChapterPuzzle(string node)
        {
            yield return ChapterGo(node);
            var id=game.Events.Definition(game.Data.nodes[game.Save.node].eventId).puzzleId;
            if(game.Puzzles.Solved(id)){yield return CloseEvent();yield break;}
            if(ActivityPresentation.For(id)!=null){yield return SolveRevisedPuzzle(id);yield break;}
            if(game.Content.Puzzle(id).mode=="rings")
            {
                yield return ChapterChoice("puzzle");var ringItemsBefore=game.Save.items.ToArray();yield return Click("PuzzleSubmit");
                Check(game.Puzzles.Active&&!game.Puzzles.Solved(id)&&game.Save.items.SequenceEqual(ringItemsBefore),"Three identical lantern rings reject submission without granting progress");
                var definition=game.Puzzles.Definition;var answer=definition.answer.Split('|');
                for(var i=0;i<3;i++)while(definition.options[game.Puzzles.Rings[i]]!=answer[i])yield return Click("LanternRing_"+i,true);
                yield return Snapshot("Chapter-"+node+"-three-lantern-rings");yield return Click("PuzzleSubmit");
                Check(game.Puzzles.Solved(id),"Rotating all three actual ring controls solves the cross-city lantern");yield return CloseEvent();yield break;
            }
            if(game.Content.Puzzle(id).mode!="fold"){yield return SolvePuzzle(id);yield break;}
            yield return ChapterChoice("puzzle");
            var before=game.Save.items.ToArray();yield return Click("PuzzleSubmit");
            Check(game.Puzzles.Active&&!game.Puzzles.Solved(id)&&game.Save.items.SequenceEqual(before),"Unfolded Hanji cannot grant a pattern");
            yield return Click("HanjiFold");Check(game.Puzzles.Folds==0,"Premature Hanji fold rejects the wrong operation and allows retry");
            yield return Click("HanjiRotate");yield return Click("HanjiRotate");yield return Click("HanjiFold");
            Check(game.Puzzles.Rotations==2&&game.Puzzles.Folds==1&&GameObject.Find("HanjiPaper")!=null,"Player performs two real paper rotations and one fold");
            yield return Snapshot("Chapter-J7-folded-paper");yield return Click("PuzzleSubmit");
            Check(game.Puzzles.Solved(id)&&game.Inventory.Count("hanji_pattern_1")==1,"Physical paper controls grant the unique first pattern");
            yield return CloseEvent();Check(!game.Puzzles.Start(id),"Reloadable solved paper puzzle cannot pay again");
        }

        IEnumerator ChapterBattle(string node,bool loseFirst=false)
        {
            yield return CloseEvent();
            if(game.Save.playerHP<75){yield return ChapterGo(game.Data.spawnNodeId);yield return ChapterChoice("rest");yield return CloseEvent();}
            yield return ChapterGo(node);
            var e=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
            if(game.Events.Resolved(e)){yield return CloseEvent();yield break;}
            var coins=game.Save.coins;var items=game.Save.items.ToArray();
            yield return ChapterChoice("battle");
            Check(GameObject.Find("BattleEnemy").GetComponent<Image>().sprite==AdventureUIArt.Load().Enemy(e.enemyId).sprite,"Combat shows the correct roster sprite for "+e.enemyId);
            if(loseFirst)
            {
                for(var i=0;game.Battles.Active&&i<60;i++)yield return Click("BattleDefend",true);
                Check(game.Battles.Outcome=="loss"&&!game.Events.Resolved(e)&&game.Save.items.SequenceEqual(items)&&game.Save.coins==coins,"Real defense turns reach defeat without rewarding or destroying quest items");
                yield return Snapshot("Chapter-"+node+"-loss");yield return CloseEvent();yield return Click("Interact");yield return ChapterChoice("battle");
            }
            for(var turn=0;game.Battles.Active&&turn<45;turn++)
            {
                var action=game.Save.playerHP<35&&game.Inventory.Has("healing_tea")?"BattleHeal":game.Save.lightEnergy>0?"BattleLight":"BattleAttack";
                yield return Click(action,turn%2==0);
            }
            Check(game.Battles.Outcome=="win"&&game.Events.Resolved(e)&&game.Save.defeatedEnemies.Contains(e.enemyId),"Real turn combat wins and persists "+e.enemyId);
            yield return Snapshot("Chapter-"+node+"-win");yield return CloseEvent();
            Check(!game.Battles.Start(e),"Defeated "+e.enemyId+" cannot restart for duplicate drops");
        }

        IEnumerator ChapterVisit(string city)
        {
            Time.timeScale=4;
            if(game.World.Has(game.Content.City(city).completionFlag)){yield return CloseEvent();yield break;}
            yield return CloseEvent();CheckChapterDiscovery();
            Check(game.Discovery.Stage(city)==0,"Fresh chapter starts at its first reveal stage");
            var hidden=NodeIndex(city=="hue"?"U19":"J15");var node=game.Save.node;
            Map.View.Focus(game.Data.nodes[hidden].position);yield return new WaitForSecondsRealtime(.2f);
            yield return Pointer(RectTransformUtility.WorldToScreenPoint(null,NodeMarker(game.Data.nodes[hidden].id).transform.position),true);
            Check(!Map.Moving&&game.Save.node==node,"Touching a hidden chapter location cannot select or move Lumi");
            yield return Click("MapOverview");yield return Snapshot("Chapter-"+city+"-stage0");
            yield return TestMapInput();
            if(city=="hue")yield return HueChapter();else yield return JeonjuChapter();
            yield return CloseEvent();CheckChapterDiscovery();
            Check(game.Discovery.Stage(city)==4,"Four canonical key gates open the completed "+city+" chapter");
            for(var i=0;i<game.Data.nodes.Length;i++)
            {Check(game.Discovery.IsVisible(i),"Completed chapter keeps side location reachable: "+game.Data.nodes[i].id);yield return Travel(i,true);}
            Check(game.Save.visited.Distinct().Count()==22,"Player actually visits all 22 "+city+" nodes through map input");
            yield return CloseEvent();yield return Click("MapOverview");yield return Snapshot("Chapter-"+city+"-all-nodes");
            yield return CrossJournal("Chapter-"+city+"-journal");
            if(Environment.GetCommandLineArgs().Contains("--all-route-diagnostics"))yield return ChapterRouteDiagnostics();
            KeepSave(city+"-chapter-complete");Time.timeScale=1;
        }

        IEnumerator HueChapter()
        {
            yield return ChapterGo("U2");yield return ChapterChoice("accept");
            Check(!game.Quests.CanComplete("UQ02"),"Bà Nguyệt requires an actual herb");yield return Snapshot("Chapter-Nguyet-quest");
            yield return ChapterGo("U14");yield return ChapterChoice("talk");
            yield return ChapterGo("U3");yield return ChapterChoice("accept");
            Check(!game.Quests.CanComplete("UQ03")&&!game.Discovery.IsVisible(NodeIndex("U4")),"Talking to Tâm without the river pass cannot cross or reveal the north bank");
            yield return ChapterPuzzle("U16");
            Check(game.Inventory.Count("river_pass")==1&&game.Discovery.Stage("hue")==0,"Solving U16 gives a pass but does not remotely open Tâm's gate");
            if(ChapterBattleBranch)yield return ChapterBattle("U15");else {yield return ChapterGo("U15");yield return ChapterChoice("alternate");}
            Check(game.Inventory.Count("lost_letter")==1,"U16 code resolves courier through explicit interaction and grants the sealed letter once");
            yield return ChapterGo("U3");yield return ChapterConfirm("complete");yield return CloseEvent();
            Check(game.Discovery.Stage("hue")==1&&game.Inventory.Has("river_pass"),"Showing Tâm the pass opens stage1 and retains the unique key");
            KeepSave("hue-gate1");CheckChapterDiscovery();yield return Snapshot("Chapter-Hue-gate1-reveal");
            yield return ChapterGo("U11");yield return ChapterChoice("collect");yield return ChapterChoice("accept:chapter_uq04_offer");
            yield return ChapterGo("U10");yield return ChapterChoice("collect");
            yield return ChapterGo("U2");yield return ChapterConfirm("complete");
            Check(game.Quests.IsCompleted("UQ02")&&!game.Inventory.Has("royal_herb"),"Actual herb return completes UQ02 and consumes only the herb");
            yield return ChapterGo("U7");yield return ChapterChoice("accept");yield return ChapterChoice("complete:chapter_uq04_return");
            Check(game.Quests.IsCompleted("UQ04")&&game.Inventory.Has("ancient_seal"),"Lê An recognizes the seal and preserves it for Gyeongju/Seoul");
            yield return Snapshot("Chapter-LeAn-seal");yield return ChapterPuzzle("U17");
            Check(game.Discovery.Stage("hue")==2&&game.Discovery.IsVisible(NodeIndex("U5"))&&!game.Discovery.IsVisible(NodeIndex("U19")),"Archive gate reveals the compulsory bot while the beacon remains hidden");
            yield return ChapterPuzzle("U8");yield return ChapterPresentation("U5");yield return CloseEvent();KeepSave("hue-before-guard");
            // Replay a genuinely earned checkpoint to prove both branches. This is
            // not a synthetic fixture granting a seal or a completed main quest.
            yield return ChapterBattle("U5",true);KeepSave("hue-battle-branch");
            yield return ReloadCheckpoint("hue-before-guard");Time.timeScale=4;
            yield return ChapterGo("U5");yield return ChapterChoice("alternate");yield return CloseEvent();
            Check(game.Discovery.Stage("hue")==3&&game.World.Has("hue.guard_disabled")&&game.Inventory.Has("ancient_seal")&&!game.Save.defeatedEnemies.Contains("enemy_gate_guard"),"Seal plus archive code opens the same inner gate without combat or consuming the seal");
            yield return Snapshot("Chapter-U5-seal-code");
            yield return ChapterBattle("U12");yield return ChapterBattle("U18");
            yield return ChapterGo("U13");yield return ChapterChoice("collect");
            yield return ChapterGo("U19");Check(!game.World.Has("hue.beacon_lit"),"Arriving at Hue beacon does not auto-complete UQ01");
            yield return ChapterChoice("restore");
            Check(game.Quests.IsCompleted("UQ01")&&game.World.Has("hue.beacon_lit")&&game.Inventory.Has("lost_letter"),"Explicit station activation completes Hue and retains the sealed historical letter");
            yield return Snapshot("Chapter-Hue-restored");
            yield return ChapterGo("U21");yield return ChapterChoice("collect");
            foreach(var id in new[]{"U4","U6"})yield return ChapterCheckIn(id);
            Check(game.IsCityUnlocked("da_lat"),"Hue opens the designed Dalat route");
            yield return ChapterGo("U20");yield return ChapterChoice("portal");yield return Scene("WorldMap");
            Check(GameObject.Find("DestinationName").GetComponent<Text>().text=="Đà Lạt"&&FindButton("EnterCity").interactable==game.Content.City("da_lat").runtimeReady,"World Map matches the actual Dalat chapter readiness");
            yield return Snapshot("Chapter-Dalat-destination-readiness");yield return Click("Back");yield return Scene("CityMap");
        }

        IEnumerator JeonjuChapter()
        {
            Check(!game.Discovery.IsVisible(NodeIndex("J15")),"New Jeonju save cannot access J15 before its canonical discovery gates");
            yield return ChapterGo("J16");yield return ChapterChoice("talk");yield return ChapterChoice("accept:chapter_jq04_offer");
            Check(game.World.Has("jeonju.hidden_letter_found"),"Explicit story interaction reveals the secret letter trail; mere arrival cannot grant it");
            yield return ChapterGo("J2");yield return ChapterChoice("accept");yield return CloseEvent();
            Check(game.Discovery.Stage("jeonju")==1&&!game.Discovery.IsVisible(NodeIndex("J15")),"Accepting Hana's quest reveals materials, not the market behind gate2");
            KeepSave("jeonju-gate1");CheckChapterDiscovery();yield return Snapshot("Chapter-Jeonju-gate1-reveal");
            yield return ChapterGo("J4");yield return ChapterChoice("talk");yield return ChapterPuzzle("J3");yield return ChapterPuzzle("J7");
            yield return ChapterGo("J5");yield return ChapterChoice("accept");yield return ChapterChoice("talk");
            Check(game.World.Has("jeonju.parcel_code_known")&&!game.World.Has("jeonju.parcel_returned"),"Reading Park's code does not falsely complete the missing parcel quest");
            if(ChapterBattleBranch)yield return ChapterBattle("J6");else {yield return ChapterGo("J6");yield return ChapterChoice("alternate");}
            Check(game.Inventory.Count("missing_parcel")==1&&game.Inventory.Count("hanji_pattern_2")==1,"Explicit serial-code resolution returns the parcel and second pattern");
            yield return ChapterGo("J5");yield return ChapterConfirm("complete");
            Check(game.Quests.IsCompleted("JQ02")&&!game.Inventory.Has("missing_parcel"),"Park receives the real parcel exactly once before unlocking pattern3");
            yield return ChapterGo("J11");yield return ChapterChoice("collect");
            Check(game.Inventory.Count("hanji_pattern_3")==1,"Unlocked rooftop box contains the third canonical Hanji pattern");
            yield return ChapterPuzzle("J12");yield return ChapterGo("J12");yield return ChapterChoice("complete:chapter_jq01_finish");
            Check(game.Quests.IsCompleted("JQ01")&&game.Inventory.Count("hanji_circuit")==1&&game.Discovery.Stage("jeonju")==2&&game.Discovery.IsVisible(NodeIndex("J15")),"Three actual patterns and assembly complete JQ01 and reveal the optional food exchange market");
            yield return ChapterPresentation("J14");
            yield return ChapterGo("J21");yield return ChapterChoice("collect");
            Check(game.Inventory.Count("reply_letter")==1,"Hidden J21 letter is obtainable without defeating optional crane boss");
            yield return ChapterGo("J17");yield return ChapterChoice("accept:chapter_jq03_offer");yield return ChapterPuzzle("J17");
            Check(game.Discovery.Stage("jeonju")==3&&game.World.Has("hoi_an.lamp_restored"),"Real Hoi An light and assembled circuit unlock the twin-city lantern gate");
            if(ChapterBattleBranch)yield return ChapterBattle("J14");else {yield return ChapterGo("J14");yield return ChapterChoice("alternate");}
            Check(game.World.Has("jeonju.pattern_bot_disabled"),"Assembled paper pattern disables the correct pattern bot without a second key item");
            yield return ChapterBattle("J9");yield return ChapterBattle("J18");
            Check(game.Inventory.Count("reply_letter")==1,"Optional crane drop cannot duplicate the unique letter already found at J21");
            yield return ChapterGo("J10");yield return ChapterChoice("collect");
            yield return ChapterGo("J19");Check(!game.World.Has("jeonju.beacon_lit"),"Entering Jeonju station never auto-finishes the chapter");
            yield return ChapterChoice("restore");Check(game.Quests.IsCompleted("JQ03")&&game.World.Has("jeonju.beacon_lit")&&game.Inventory.Has("hanji_circuit"),"Explicit activation completes Jeonju and preserves the Seoul relay circuit");
            yield return Snapshot("Chapter-Jeonju-restored");
            yield return ChapterShop();
            foreach(var id in new[]{"J8","J11"})yield return ChapterCheckIn(id);
            Check(game.IsCityUnlocked("gyeongju"),"Actual Jeonju beacon earns the Gyeongju unlock condition; runtime availability is checked on World Map entry");
        }

        IEnumerator ChapterCheckIn(string node)
        {
            yield return ChapterGo(node);var entry=game.Events.CheckInAt(game.Save.node);var coins=game.Save.coins;
            if(FindButton("CheckInRecord")==null)yield return ChapterChoice("checkin");
            yield return Click("CheckInRecord",true);
            Check(game.World.Has(entry.completionFlag)&&game.Save.coins==coins+entry.rewardCoins,"Actual "+node+" check-in records its own landmark and one-time reward");
            yield return Snapshot("Chapter-"+node+"-checkin");yield return CloseEvent();
            Check(!game.Events.Execute(game.Save.node,"checkin").changed,"Check-in cannot pay twice: "+node);
        }
        IEnumerator ChapterPresentation(string focus)
        {
            yield return CloseEvent();var real=JsonUtility.ToJson(game.Save);
            foreach(var portrait in new[]{true,false})
            {
                Screen.SetResolution(portrait?540:960,portrait?960:540,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.4f);game.Scenes.Go("CityMap");yield return Scene("CityMap");
                foreach(var zoom in new[]{1f,2f,4f})
                {
                    Map.View.ZoomBy(zoom/Map.View.Zoom);Map.View.Focus(game.Data.nodes[NodeIndex(focus)].position);
                    yield return new WaitForSecondsRealtime(.2f);CheckMapActors(game.Data.cityId+" "+(portrait?"portrait":"landscape")+" zoom "+zoom);
                    Check(NodeMarker(focus).Badge.gameObject.activeInHierarchy&&MapActor(focus).gameObject.activeInHierarchy,"Unresolved bot and floating alert remain visible at "+focus+" zoom "+zoom);
                    yield return Snapshot("Chapter-"+game.Data.cityId+"-"+(portrait?"portrait":"landscape")+"-zoom"+zoom);
                }
                yield return Click("MapOverview");yield return TestMapInput();
            }
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==real,"Orientation/zoom/input checks preserve exact chapter progress and rewards");
        }
        IEnumerator ChapterShop()
        {
            yield return ChapterGo("J15");var tea=game.Inventory.Count("healing_tea");var coins=game.Save.coins;
            yield return CrossBag("Chapter-J15-inventory-before-shop",null);yield return Click("Interact");
            yield return ChapterChoice("buy:healing_tea");
            Check(game.Inventory.Count("healing_tea")==tea+1&&game.Save.coins==coins-8,"Actual J15 shop purchase consumes eight coins and grants one tea");
            Check(GameObject.Find("ReceivedIcon_healing_tea").GetComponent<Image>().sprite==AdventureUIArt.Load().Item("healing_tea").sprite,"Shop receipt uses the shared tea icon");
            yield return Snapshot("Chapter-J15-shop-receipt");
            yield return ChapterChoice("buy:healing_tea");
            Check(game.Inventory.Count("healing_tea")==tea+2&&game.Save.coins==coins-16,"A second explicit shop purchase also pays full price rather than farming free tea");
            yield return CrossBag("Chapter-J15-inventory-after-shop",null);
            var real=JsonUtility.ToJson(game.Save);game.Save.coins=0;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return Click("Interact");
            var before=game.Inventory.Count("healing_tea");
            Check(!game.Events.BuildOptions(game.Save.node).options.First(o=>o.id=="buy:healing_tea").enabled&&!game.Events.Execute(game.Save.node,"buy:healing_tea").changed&&game.Inventory.Count("healing_tea")==before,"Insufficient-coins fixture rejects UI and backend purchase without granting any item");
            yield return Snapshot("Chapter-J15-shop-insufficient-fixture");yield return CloseEvent();
            File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Check(JsonUtility.ToJson(game.Save)==real,"Shop negative fixture restores exact legitimately purchased inventory");
        }
        IEnumerator ChapterReturnLetters()
        {
            yield return ChapterGo("H18");
            if(game.Quests.CanAccept("HQ03"))yield return ChapterChoice("accept");
            yield return ChapterConfirm("complete:chapter_jq04_return");
            Check(game.Quests.IsCompleted("JQ04")&&game.Inventory.Has("reply_letter"),"Returning the letter to Ji-eun completes the actual intercity JQ04 without consuming the shared clue");
            yield return Snapshot("Chapter-Jieun-letter-return");
            yield return ChapterGo("H9");yield return ChapterChoice("complete");
            Check(game.Quests.IsCompleted("HQ03")&&game.World.Has("hoi_an.lost_lantern_rescued"),"The newly reachable Jeonju letter closes Hoi An HQ03 at the lost-lantern alley");
            yield return Snapshot("Chapter-HQ03-lantern-rescued");
        }
        void CheckChapterSavedProgress()
        {
            foreach(var cid in new[]{"hue","jeonju"})
            {
                var flag=game.Content.City(cid).completionFlag;
                if(game.World.Has(flag))Check(game.Discovery.Stage(cid)==4&&game.Save.cityProgress.First(p=>p.cityId==cid).visited.Distinct().Count()==22,"Process restart retains the completed "+cid+" chapter and all 22 real visits");
            }
            Check(game.Inventory.Count("reply_letter")<=1,"Separate process restart retains unique-letter quantity");
        }

        IEnumerator ChapterRouteDiagnostics()
        {
            yield return CloseEvent();var real=JsonUtility.ToJson(game.Save);var edges=game.Data.edges;var traces=new List<RouteTrace>();
            var speed=Time.timeScale;Time.timeScale=TerrainEvidenceEnabled?1:2;
            // Explicit route-only fixture isolates each existing edge, after the
            // actual chapter was completed. It is not used to claim E2E progress.
            foreach(var edge in edges)foreach(var reverse in new[]{false,true})
            {
                yield return CloseEvent();var from=reverse?edge.to:edge.from;var to=reverse?edge.from:edge.to;
                game.Save.node=from;game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                var points=CityRouteGeometry.Points(game.Data,from,to);game.Data.edges=new[]{edge};
                if(TerrainEvidenceEnabled)Check(Vector2.Distance(Map.Marker.anchorMin,CityRouteGeometry.Arrival(game.Data.nodes[from]))<.00001f,"Scene/load places Lumi at the ground arrival, not the visual marker: "+game.Data.nodes[from].id);
                Map.View.Focus(game.Data.nodes[to].position);yield return new WaitForSecondsRealtime(.2f);
                var trace=new RouteTrace{from=from,to=to,route=points.ToArray(),travel=edge.travel};var size=Map.View.Content.rect.size;
                yield return Click(game.Data.nodes[to].id,reverse);var deadline=Time.realtimeSinceStartup+30;bool photo=false;
                while(Map.Moving&&Time.realtimeSinceStartup<deadline)
                {
                    var p=Map.Marker.anchorMin;trace.positions.Add(p);trace.times.Add(Time.realtimeSinceStartup);float best=float.MaxValue;
                    for(var i=1;i<points.Count;i++)
                    {var start=Vector2.Scale(points[i-1],size);var delta=Vector2.Scale(points[i]-points[i-1],size);var t=delta.sqrMagnitude<.000001f?0:Mathf.Clamp01(Vector2.Dot(Vector2.Scale(p,size)-start,delta)/delta.sqrMagnitude);best=Mathf.Min(best,Vector2.Distance(Vector2.Scale(p,size),start+t*delta));}
                    trace.maxDistance=Mathf.Max(trace.maxDistance,best);
                    if(TerrainEvidenceEnabled)TerrainWalkingFrame(from,to,p);
                    if(!photo&&game.Data.cityId=="hoi_an"&&Environment.GetCommandLineArgs().Contains("--ui-polish")&&
                        edge.from==18&&edge.to==5&&p.x>.69f&&p.x<.73f&&p.y>.46f&&p.y<.50f)
                    {
                        photo=true;var name="HoiAn-Lumi-covered-bridge-"+(reverse?"return":"outbound")+".png";
                        ScreenCapture.CaptureScreenshot(Path.Combine(output,name));captures.Add(DateTime.UtcNow.ToString("O")+" | "+name);
                    }
                    if(!photo&&edge.travel=="ferry")
                    {
                        photo=true;Check(Map.ActiveTravelMode=="ferry"&&Map.Marker.Find("FerryTransport").gameObject.activeSelf,"U3-U4 water travel shows the boat, not walking on water");
                        // Capture without blocking the movement sampler for .6 s.
                        ScreenCapture.CaptureScreenshot(Path.Combine(output,"Chapter-"+game.Data.cityId+"-ferry-"+(reverse?"return":"outbound")+".png"));
                    }
                    yield return null;
                }
                trace.arrived=game.Save.node==to&&!Map.Moving;
                if(TerrainEvidenceEnabled)Check(Vector2.Distance(Map.Marker.anchorMin,CityRouteGeometry.Arrival(game.Data.nodes[to]))<.00001f&&!Map.LumiOccluded,"Arrival finishes visibly on authored ground: "+game.Data.nodes[to].id);
                traces.Add(trace);game.Data.edges=edges;
                File.WriteAllText(Path.Combine(output,game.Data.cityId+"-chapter-route-traces.json"),"["+string.Join(",",traces.Select(t=>JsonUtility.ToJson(t)))+"]");
                Check(trace.arrived&&trace.positions.Count>1&&trace.maxDistance<.02f,"Route-only fixture follows authored geometry and arrives "+game.Data.nodes[from].id+" -> "+game.Data.nodes[to].id+" (arrived="+trace.arrived+", samples="+trace.positions.Count+", error="+trace.maxDistance+")");
            }
            File.WriteAllText(Path.Combine(output,game.Data.cityId+"-chapter-route-traces.json"),"["+string.Join(",",traces.Select(t=>JsonUtility.ToJson(t)))+"]");
            File.WriteAllText(savePath,real);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Time.timeScale=speed;
            Check(JsonUtility.ToJson(game.Save)==real,"Route diagnostics restore exact legitimately earned chapter state");
            if(TerrainEvidenceEnabled)WriteTerrainFrames();
        }
    }
}
#endif
