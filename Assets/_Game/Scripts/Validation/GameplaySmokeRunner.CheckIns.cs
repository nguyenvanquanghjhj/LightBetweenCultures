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
        string CheckInCore()=>string.Join("|",game.Save.accepted)+";"+string.Join("|",game.Save.completed)+";"+
            string.Join("|",game.Save.solvedPuzzles)+";"+string.Join("|",game.Save.defeatedEnemies)+";"+
            string.Join("|",game.Save.choices)+";"+string.Join("|",game.Save.flags.Where(f=>!f.StartsWith("checkin")&&!f.StartsWith("da_lat.checkin")))+";"+
            string.Join("|",game.Save.items)+";"+game.Save.version;
        IEnumerator OpenPhotoJournal()
        {yield return Click("QuestJournal",true);yield return Click("OpenRelicCollection",true);yield return Click("OpenPhotoJournal",true);}
        IEnumerator PhotoCardClick(CheckInDefinition entry)
        {
            var list=game.Content.checkIns.Where(c=>c.cityId==entry.cityId).ToArray();var index=Array.IndexOf(list,entry);
            var scroll=GameObject.Find("PhotoJournalViewport").GetComponent<ScrollRect>();Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition=index>=4?0:1;yield return null;yield return null;
            var card=(RectTransform)FindButton("PhotoCard_"+entry.id).transform;var point=RectTransformUtility.WorldToScreenPoint(null,card.TransformPoint(card.rect.center));
            Check(RectTransformUtility.RectangleContainsScreenPoint(scroll.viewport,point),"Album card scrolled inside viewport before pointer input: "+entry.id);
            yield return Click("PhotoCard_"+entry.id,true);
        }
        IEnumerator OpenCheckInView(CheckInDefinition entry)
        {
            if(GameObject.Find("CheckInPanel")==null)yield return RelicOption(game.World.Has(entry.completionFlag)?"checkin_view":"checkin");
            yield return new WaitForSecondsRealtime(.25f);
            Check(GameObject.Find("CheckInPanel")!=null,"Actual landmark view opens: "+entry.id);
            var image=GameObject.Find("CheckInIllustration").GetComponent<RawImage>();
            var binding=AdventureUIArt.Load().CheckIn(entry.id);
            Check(image.texture==(entry.id=="hoi_an.h6"?DesignKitArt.Load().hoiAnMap.texture:binding.sprite.texture),"Correct scenery texture; H6 retains old Map A: "+entry.id);
            if(entry.id!="hoi_an.h6")
            {
                Canvas.ForceUpdateCanvases();var aspect=image.rectTransform.rect.width/image.rectTransform.rect.height;
                Check(Mathf.Abs(aspect-binding.sprite.texture.width*binding.viewUV.width/(binding.sprite.texture.height*binding.viewUV.height))<.01f,"Preserved scenery aspect; footer excluded: "+entry.id);
                Check(image.uvRect==binding.viewUV&&binding.viewUV.y>.1f,"Source footnote not duplicated by UI: "+entry.id);
            }
        }
        IEnumerator CheckInArtFresh()
        {
            game.ResetSave();yield return CrossEnter("hoi_an");var before=JsonUtility.ToJson(game.Save);
            Check(!game.Events.CanCheckIn(2),"Fresh H2 still requires real Mai tutorial, never just a photo preview");
            yield return OpenPhotoJournal();
            Check(game.Content.checkIns.Length==18&&AdventureUIArt.Load().checkIns.Length==18,"Exactly18 existing definitions and18 art mappings, no second collection state");
            foreach(var city in game.Content.cities)
            {
                yield return Click("PhotoCity_"+city.id,true);
                Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.name.StartsWith("PhotoCard_")).All(b=>!b.interactable),"Fresh album cannot open/grant locked postcard: "+city.id);
                Check(GameObject.Find("PhotoThumbnail")==null,"Fresh album never leaks undiscovered scenery: "+city.id);
                if(city.id=="hoi_an"||city.id=="seoul")yield return Snapshot("Fresh-journal-"+city.id);
            }
            yield return Click("PhotoJournalClose",true);
            Check(JsonUtility.ToJson(game.Save)==before,"Fresh gallery navigation leaves quests/items/coins/relics/discovery/ending unchanged");
        }
        IEnumerator CheckInArtJourney()
        {
            Check(game.World.Has("game.ending_seen")&&game.World.Has("checkin.hoi_an.h6"),"Uses genuinely earned historical post-boss save with old H6 mark, not fixture unlocks");
            var core=CheckInCore();var firstCoins=game.Save.coins;var rewards=0;var added=0;
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                yield return CrossEnter(city);Time.timeScale=4;
                foreach(var entry in game.Content.checkIns.Where(c=>c.cityId==city))
                {
                    yield return ChapterGo(entry.nodeId);Time.timeScale=1;
                    var primary=game.Events.BuildOptions(game.Save.node).options.Where(o=>o.id!="checkin"&&o.id!="checkin_view").Select(o=>o.id+":"+o.enabled).ToArray();
                    if(entry.nodeId=="G14"||entry.nodeId=="S2")
                    {
                        Check(GameObject.Find("EventPortrait")?.GetComponent<Image>().sprite==AdventureUIArt.Load().Npc(game.Events.Definition(game.Data.nodes[game.Save.node].eventId).npcId).portrait,"NPC portrait binding and dialogue remain available alongside photo: "+entry.nodeId);
                        yield return Snapshot("NPC-and-checkin-"+entry.nodeId);
                    }
                    var marked=game.World.Has(entry.completionFlag);var coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();
                    var hoiAnMarkerNode=city=="hoi_an"&&new[]{"H2","H3","H4","H6","H7","H17"}.Contains(entry.nodeId);
                    if(hoiAnMarkerNode)
                    {
                        if(marked)CheckMarker(entry.nodeId,NodeMarkerType.Completed);
                        else if(game.Events.CanCheckIn(game.Save.node))CheckMarker(entry.nodeId,NodeMarkerType.Camera);
                        if(entry.nodeId=="H4"||entry.nodeId=="H7")
                        {
                            var resolvedEvent=game.Events.Resolved(game.Events.Definition(game.Data.nodes[game.Save.node].eventId));
                            Check(resolvedEvent,"Resolved Hoi An event is separately identified from its saved check-in: "+entry.nodeId);
                            if(marked)
                            {
                                var flagIndex=game.Save.flags.IndexOf(entry.completionFlag);
                                Check(flagIndex>=0,"Recorded Hoi An check-in is stored as its real completion flag: "+entry.nodeId);
                                if(flagIndex>=0)
                                {
                                    game.Save.flags.RemoveAt(flagIndex);Map.SendMessage("Refresh");yield return null;
                                    var pendingCanCheckIn=game.Events.CanCheckIn(game.Save.node);
                                    var pendingMarker=NodeMarker(entry.nodeId).Type;
                                    game.Save.flags.Insert(flagIndex,entry.completionFlag);Map.SendMessage("Refresh");yield return null;
                                    Check(pendingCanCheckIn&&pendingMarker==NodeMarkerType.Camera,"Resolved event keeps Camera while a real check-in can still be recorded: "+entry.nodeId);
                                    CheckMarker(entry.nodeId,NodeMarkerType.Completed);
                                }
                            }
                            else Check(game.Events.CanCheckIn(game.Save.node)&&NodeMarker(entry.nodeId).Type==NodeMarkerType.Camera,"Resolved event keeps Camera only while a real check-in can still be recorded: "+entry.nodeId);
                        }
                    }
                    yield return OpenCheckInView(entry);
                    Check(game.World.Has(entry.completionFlag)==marked&&game.Save.coins==coins&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Opening scenery does not record or pay: "+entry.id);
                    if(!marked)
                    {
                        Check(FindButton("CheckInRecord")!=null,"Existing shared observation/record interaction available: "+entry.id);
                        yield return Click("CheckInClose",true);Check(!game.World.Has(entry.completionFlag)&&game.Save.coins==coins,"Cancel observation does not award: "+entry.id);
                        yield return Click("Interact",true);yield return OpenCheckInView(entry);yield return Click("CheckInRecord",true);
                        if(FindButton("ReceiptContinue")!=null)yield return Click("ReceiptContinue",true);
                        Check(game.World.Has(entry.completionFlag)&&game.Save.coins==coins+entry.rewardCoins,"Explicit touch records actual landmark and exact existing reward: "+entry.id);rewards+=entry.rewardCoins;added++;
                    }
                    if(new[]{"H2","U6","D0","J11","G14","S2","S18"}.Contains(entry.nodeId))yield return Snapshot("Scenery-"+entry.nodeId);
                    yield return Click("CheckInClose",true);yield return Click("Interact");yield return OpenCheckInView(entry);
                    Check(FindButton("CheckInRecord")==null&&game.Save.coins==coins+(marked?0:entry.rewardCoins),"Reopen is read-only and cannot repeat reward: "+entry.id);
                    if(entry.nodeId=="H6")
                    {
                        Check(!game.Relics.PhotoRecorded,"Old H6 completion remains valid without inventing new alignment metadata");
                        yield return Click("PhotoCompose",true);var a=game.Relics.Active;yield return Click("RelicRecordPhoto");
                        Check(a!=null&&!game.Relics.PhotoRecorded,"H6 unchanged two-symbol interaction rejects empty confirmation");
                        Check(GameObject.Find("PhotoMapView").GetComponent<RawImage>().texture==DesignKitArt.Load().hoiAnMap.texture,"H6 interaction keeps approved art, not imported candidate");
                        yield return Snapshot("H6-original-alignment");yield return RelicExit();
                    }
                    else yield return Click("CheckInClose");
                    if(hoiAnMarkerNode)CheckMarker(entry.nodeId,NodeMarkerType.Completed);
                    Check(game.Events.BuildOptions(game.Save.node).options.Where(o=>o.id!="checkin"&&o.id!="checkin_view").Select(o=>o.id+":"+o.enabled).SequenceEqual(primary),"Primary quest/NPC/puzzle/beacon choices preserved: "+entry.nodeId);
                    Check(CheckInCore()==core,"Check-in cannot mutate story/relic/boss/ending inventory state: "+entry.id);Time.timeScale=4;
                }
                if(city=="hoi_an")
                {
                    Time.timeScale=1;yield return ChapterGo("H18");
                    var h18Index=game.Save.node;
                    Check(game.Quests.IsCompleted("HQ03")&&game.Events.CheckInAt(h18Index)==null,"H18 fixture has completed HQ03 but no check-in definition");
                    Check(!game.World.Has("checkin.hoi_an.h18")&&!game.Events.CanCheckIn(h18Index),"H18 completion does not synthesize a check-in flag or reward");
                    if(FindButton("EventClose")!=null)yield return CloseEvent();
                    CheckMarker("H18",NodeMarkerType.Completed);
                    yield return Snapshot("HoiAn-H18-complete-no-checkin-camera");
                }
                Time.timeScale=1;yield return OpenPhotoJournal();
                Check(FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b=>b.name.StartsWith("PhotoCard_")&&b.interactable)==game.Content.checkIns.Count(c=>c.cityId==city),"All earned city postcards shown in existing collection: "+city);
                yield return Snapshot("Journal-"+city);
                var last=game.Content.checkIns.Last(c=>c.cityId==city);var before=JsonUtility.ToJson(game.Save);
                yield return PhotoCardClick(last);yield return Snapshot("Journal-large-"+last.nodeId);yield return Click("PhotoBackToJournal",true);
                Check(JsonUtility.ToJson(game.Save)==before,"Album large view does not execute gameplay or grant: "+city);
                yield return Click("PhotoJournalClose");game.Persist();KeepSave("checkin-after-"+city);
            }
            Check(game.Content.checkIns.All(c=>game.World.Has(c.completionFlag)),"All18 real existing check-ins now recorded; no generated entries");
            Check(game.Save.coins==firstCoins+rewards&&added==11,"Only11 previously unmarked locations receive their original one-time rewards");
            File.WriteAllText(Path.Combine(output,"checkin-journey-summary.txt"),"Actual WorldMap navigation and Lumi traversal; historical post-boss save7 marks +11 explicit records =18. No fixture flags or teleport. Android NOT_TESTED.");
        }
        IEnumerator CheckInArtRestart()
        {
            var city=game.Save.cityId;var expected=game.Content.checkIns.Where(c=>c.cityId==city&&game.World.Has(c.completionFlag)).ToArray();
            Check(expected.Length>0&&game.World.Has("game.ending_seen"),"Real process restart restores earned postcards and old ending history");
            yield return CrossEnter(city);var before=JsonUtility.ToJson(game.Save);yield return OpenPhotoJournal();
            var allCities=Environment.GetCommandLineArgs().Contains("--checkin-all-city-album");
            foreach(var page in allCities?game.Content.cities:new[]{game.Content.City(city)})
            {
              if(allCities)yield return Click("PhotoCity_"+page.id,true);
              foreach(var entry in game.Content.checkIns.Where(c=>c.cityId==page.id&&game.World.Has(c.completionFlag)))
              {
                yield return PhotoCardClick(entry);
                Check(GameObject.Find("CheckInIllustration")?.GetComponent<RawImage>().texture==AdventureUIArt.Load().CheckIn(entry.id).sprite.texture,"Restart opens correct approved-source postcard without returning to its node: "+entry.id);
                var image=GameObject.Find("CheckInIllustration").GetComponent<RawImage>();var binding=AdventureUIArt.Load().CheckIn(entry.id);Canvas.ForceUpdateCanvases();
                Check(image.uvRect==binding.viewUV&&Mathf.Abs(image.rectTransform.rect.width/image.rectTransform.rect.height-image.texture.width*binding.viewUV.width/(image.texture.height*binding.viewUV.height))<.01f,"Derived scenery UV and aspect hold in actual Player: "+entry.id);
                Check(image.texture!=DesignKitArt.Load().hoiAnMap.texture&&image.texture.width<=450,"Gallery uses approved-source derived art, never Map A or upscale: "+entry.id);
                if(!allCities)yield return Snapshot("Postcard-"+entry.nodeId);
                else Check(game.Save.cityId==city&&game.Save.node==JsonUtility.FromJson<SaveData>(before).node,"Cross-city postcard view never travels or executes node events: "+entry.id);
                yield return Click("PhotoBackToJournal",true);
              }
              if(allCities&&(page.id=="hoi_an"||page.id=="seoul"))yield return Snapshot("Cross-city-album-"+page.id+"-while-at-"+city);
            }
            yield return Snapshot("Restart-album-"+city);yield return Click("PhotoJournalClose");
            Check(JsonUtility.ToJson(game.Save)==before,"Process restart/revisit preserves exact ledger, coins, quests and relic state: "+city);
        }
        IEnumerator CheckInArtRelay()
        {
            yield return CrossEnter("seoul");Check(!game.World.Has("seoul.relay_c_on"),"Earned pre-relay save has relay C off");
            Time.timeScale=4;yield return ChapterGo("S18");Time.timeScale=1;var entry=game.Events.CheckInAt(game.Save.node);
            yield return OpenCheckInView(entry);yield return Click("CheckInRecord",true);yield return Click("CheckInClose",true);
            Check(game.World.Has(entry.completionFlag)&&!game.World.Has("seoul.relay_c_on")&&!game.Puzzles.Solved("puzzle_relay_jeonju"),"Photo records S18 but cannot switch on relay or solve its canonical puzzle");
            yield return ChapterPuzzle("S18");Check(game.World.Has("seoul.relay_c_on"),"Existing relay puzzle still works after independent camera interaction");
            yield return ChapterGo("S18");yield return Snapshot("S18-relay-after-real-puzzle");yield return CloseEvent();game.Persist();KeepSave("checkin-relay-valid");
        }
    }
}
#endif
