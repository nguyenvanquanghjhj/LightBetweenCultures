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
        IEnumerator Phase7BotPortraitJourney()
        {
            musicVirtualFocus=true;var original=JsonUtility.ToJson(game.Save);var art=AdventureUIArt.Load();
            yield return PilotResolution(720,1280);
            foreach(var a in art.mapActors.Where(a=>art.Enemy(a.artId)!=null))
            {
                yield return CrossEnter(a.cityId);yield return CloseEvent();var index=NodeIndex(a.nodeId);var enemy=art.Enemy(a.artId);
                foreach(var before in enemy.portrait!=enemy.sprite?new[]{true,false}:new[]{false})
                {
                    var saved=JsonUtility.ToJson(game.Save);P6Show("ShowEvent",index,"",null,0);yield return new WaitForSecondsRealtime(.5f);
                    var image=GameObject.Find("EventPortrait").GetComponent<Image>();
                    Check(image.sprite==(enemy.portrait??enemy.sprite)&&image.useSpriteMesh,"Actual encounter dialogue selects its intended portrait and mesh: "+a.nodeId);
                    if(before){image.sprite=enemy.sprite;image.useSpriteMesh=false;}
                    P6Bounds("EventPortrait");P6Bounds("EventClose");
                    yield return P6Shot("P7-BotTalk-"+a.cityId+"-"+a.nodeId+(before?"-before-renderer-fixture":"-after"));
                    Check(!image.raycastTarget,"Enemy dialogue portrait never blocks touch: "+a.nodeId);yield return CloseEvent();
                    Check(JsonUtility.ToJson(game.Save)==saved,"Portrait open/close preserves quest/reward state: "+a.nodeId);
                }
            }
            File.WriteAllText(savePath,original);game.Reload();KeepSave("P7-portrait-restored-input");
        }
        IEnumerator Phase7BotJourney()
        {
            musicVirtualFocus=true;var original=JsonUtility.ToJson(game.Save);var art=AdventureUIArt.Load();
            var label=Environment.GetCommandLineArgs().First(a=>a.StartsWith("--phase7-bot-label=")).Split('=')[1];
            yield return PilotResolution(720,1280);
            foreach(var a in art.mapActors.Where(a=>art.Enemy(a.artId)!=null))
            {
                yield return CrossEnter(a.cityId);yield return CloseEvent();
                var index=NodeIndex(a.nodeId);Check(game.Discovery.IsVisible(index),"Bot review stays inside earned discovered region: "+a.nodeId);
                var image=FindObjectsByType<Image>(FindObjectsInactive.Include,FindObjectsSortMode.None).First(i=>i.name=="MapActor_"+a.nodeId);
                Check(image.sprite==art.ActorSprite(a.artId)&&image.useSpriteMesh,"Existing map actor uses bound sprite/mesh: "+a.artId);
                // Explicit presentation fixture: show the existing resolved actor,
                // never respawn an encounter or mutate defeated/progression flags.
                var was=image.gameObject.activeSelf;image.gameObject.SetActive(true);
                Map.View.ZoomBy(3/Map.View.Zoom);Map.View.Focus(a.hasMapPosition?a.mapPosition:game.Data.nodes[index].position);
                var saved=JsonUtility.ToJson(game.Save);yield return P6Shot("P7-Bot-"+a.cityId+"-"+a.nodeId+"-"+label);
                Check(JsonUtility.ToJson(game.Save)==saved,"Bot image inspection cannot respawn rewards/encounter: "+a.nodeId);image.gameObject.SetActive(was);
            }
            File.WriteAllText(savePath,original);game.Reload();KeepSave("P7-bot-restored-input");
        }
        // Visual fixtures supplement the separately earned VN/Korea campaigns.
        // Never count reopened private fixtures as newly earned puzzle progress.
        IEnumerator Phase7VisualJourney()
        {
            musicVirtualFocus=true;var original=JsonUtility.ToJson(game.Save);
            Check(ActivityPresentation.Load().entries.All(e=>game.Puzzles.Solved(e.id)),"Visual probe input is the earned22-proof campaign save");
            yield return PilotResolution(540,960);yield return P6Shot("P7-MainMenu");
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
            {
                yield return PilotResolution(size.x,size.y);yield return CrossEnter("hoi_an");Time.timeScale=5;yield return ChapterGo("H2");yield return CloseEvent();Time.timeScale=1;
                var prefix="P7-"+size.x+"x"+size.y;yield return Click("Interact",true);P6Bounds("EventClose");P6Bounds("DialogueTextViewport");yield return P6Shot(prefix+"-dialogue");yield return CloseEvent();
                yield return Click("Inventory",true);P6Bounds("CloseInventory");yield return P6Shot(prefix+"-inventory");yield return Click("CloseInventory",true);
                yield return Click("QuestJournal",true);P6Bounds("CloseJournal");if(size.x==540)yield return P6Shot("P7-QuestJournal");yield return Click("OpenRelicCollection",true);P6Bounds("RelicExit");if(size.x==540)yield return P6Shot("P7-RelicCollection");yield return RelicExit();
                if(FindButton("CloseJournal")!=null)yield return Click("CloseJournal",true);
            }
            yield return PilotResolution(720,1280);Time.timeScale=5;
            foreach(var spec in ActivityPresentation.Load().entries.Where(e=>e.kind=="STORY_MINIGAME"))
            {
                var def=game.Content.Puzzle(spec.id);yield return CrossEnter(def.cityId);yield return ChapterGo(def.nodeId);yield return CloseEvent();
                var earned=JsonUtility.ToJson(game.Save);
                try
                {
                    game.Save.solvedPuzzles.Remove(spec.id);
                    // Explicitly recorded presentation fixture only. Restore any consumed
                    // prerequisites locally; no grants are claimed as gameplay evidence.
                    foreach(var item in def.requiredItems??Array.Empty<ItemAmount>())
                        if(!game.Inventory.Has(item.itemId,item.amount))game.Inventory.Add(item.itemId,item.amount-game.Inventory.Count(item.itemId));
                    Check(game.Puzzles.Start(spec.id),"Earned context can reopen visual fixture: "+spec.id);
                    P6Show("ShowStoryActivity",game.Save.node);P6Bounds("MicroConfirm");P6Bounds("ActivityClose");
                    yield return P6Shot("P7-"+spec.id+"-visual-fixture");yield return CloseEvent();
                    Check(!game.Puzzles.Solved(spec.id),"Rendering/closing fixture cannot complete a story activity: "+spec.id);
                }
                finally{game.Puzzles.Cancel();File.WriteAllText(savePath,earned);game.Reload();}
                game.Scenes.Go("CityMap");yield return Scene("CityMap");
            }
            // Representative short interaction has no puzzle panel; capture actual
            // existing menu/instructions rather than invent a replacement activity.
            yield return CrossEnter("hoi_an");yield return ChapterGo("H4");yield return CloseEvent();
            var local=JsonUtility.ToJson(game.Save);game.Save.solvedPuzzles.Remove("puzzle_market");
            yield return Click("Interact",true);yield return P6Shot("P7-short-story-menu-fixture");yield return CloseEvent();
            File.WriteAllText(savePath,local);game.Reload();Time.timeScale=1;
            yield return Click("Back",true);yield return Scene("WorldMap");yield return P6Shot("P7-WorldMap");
            File.WriteAllText(savePath,original);game.Reload();KeepSave("P7-visual-restored-input");
            Check(JsonUtility.ToJson(game.Save)==original,"All presentation fixtures restore the complete earned input save");
        }
    }
}
#endif
