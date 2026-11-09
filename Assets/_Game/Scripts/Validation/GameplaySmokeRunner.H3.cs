#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        bool WorldReactionQA=>Environment.GetCommandLineArgs().Contains("--world-reactions");
        IEnumerator ReactionMapFrame(string name)
        {
            yield return CloseEvent();NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
            Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(new Vector2(.383287f,.593002f));
            yield return new WaitForSecondsRealtime(.35f);yield return Snapshot(name);
        }
        void CheckLocalPresentation(bool expected)
        {
            Check(Map.LocalQuestLightComplete==expected,"H2 local light derives only from completed mai-lantern: "+expected);
            Check(GameObject.Find("RestoredLight")==null,"No city-wide viewport restoration effect is emitted in Hoi An");
            var layer=GameObject.Find("QuestReactionLayer");
            Check(layer!=null&&!layer.GetComponent<CanvasGroup>().blocksRaycasts&&layer.GetComponentsInChildren<Image>().All(i=>!i.raycastTarget),"Local visual layer never blocks mouse/touch or changes node input");
            var rect=GameObject.Find("QuestLight_H2").GetComponent<RectTransform>();
            Check(rect.rect.width<Map.View.Content.rect.width*.12f&&rect.rect.height<Map.View.Content.rect.height*.14f,"Light remains a small H2 landmark overlay, not whole-map illumination");
            foreach(var i in Enumerable.Range(0,game.Data.nodes.Length).Where(i=>game.Discovery.DiscoveryState(i)==MapDiscoveryState.HIDDEN))
                Check(!Map.View.Content.Find(game.Data.nodes[i].id).gameObject.activeSelf&&game.Data.FindPath(game.Save.node,i,game.Save)==null,"Reaction does not expose hidden node/path: "+game.Data.nodes[i].id);
        }
        IEnumerator ReactionWorldGate(bool expected)
        {
            yield return CloseEvent();yield return Click("Back",true);yield return Scene("WorldMap");
            var city=FindObjectsByType<Button>(FindObjectsSortMode.None).First(b=>b.name.StartsWith("City")&&b.GetComponentsInChildren<Text>().Any(t=>t.text==game.Content.City("jeonju").name));
            yield return Click(city.name,true);Check(FindButton("EnterCity").interactable==expected,"World Map Jeonju gate remains controlled by HQ01, not local light: "+expected);
            yield return Click("Back");yield return Scene("CityMap");
        }
        IEnumerator WorldReactionJourney()
        {
            var restart=Environment.GetCommandLineArgs().Contains("--wr-restart");
            if(!restart)game.ResetSave();
            yield return Click("Start");yield return Scene("WorldMap");yield return Click(HoiAnButton,true);yield return Click("EnterCity");yield return Scene("CityMap");
            if(restart)
            {
                var complete=game.Quests.Completed;CheckLocalPresentation(complete);
                Check(Map.LocalQuestReactionBursts==0,"Restart derives persistent local warmth without replaying the completion reaction");
                yield return ReactionMapFrame("WR-restart-derived-state");
                if(complete)
                {
                    Screen.SetResolution(1280,720,false);yield return new WaitForSecondsRealtime(.5f);
                    CheckLocalPresentation(true);yield return ReactionMapFrame("WR-landscape-local-light");
                    Check(FindButton("Back").interactable&&FindButton("Inventory").interactable,"Landscape retains World/Inventory controls under the visual layer");
                    Screen.SetResolution(600,1000,false);yield return new WaitForSecondsRealtime(.5f);
                }
                var savedCoins=game.Save.coins;var grants=game.Save.rewardGrantIds.ToArray();
                if(!complete)
                {
                    Check(game.World.Has("hoi_an.lamp_eye_found"),"Pickup-only restart uses the genuinely earned Mắt Đèn state");
                    yield return Travel(3,true);yield return Click("EventOption_scene");yield return Scene("HoiAnDemo");
                    Check(!FindAnyObjectByType<HoiAnItem>(FindObjectsInactive.Include).gameObject.activeSelf&&FindAnyObjectByType<LumiStepMotion>().ReactionCount==0,"Pickup restart neither respawns item nor replays its Lumi reaction");
                    yield return Click("Return");yield return Scene("CityMap");
                }
                yield return ReactionWorldGate(game.Quests.IsCompleted("HQ01"));CheckLocalPresentation(complete);
                Check(game.Save.coins==savedCoins&&game.Save.rewardGrantIds.SequenceEqual(grants),"Restart/re-entry and visual state cannot grant a reward again");
                yield break;
            }
            CheckLocalPresentation(false);yield return Travel(2,true);yield return ReactionMapFrame("WR-A-before-first-light");
            yield return Click("Interact");yield return Click("EventOption_accept",true);yield return Click("DialogueNext");yield return CloseEvent();
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Full);
            yield return Travel(3,true);yield return ExploreH3();KeepSave("WR-found-before-return");
            CheckLocalPresentation(false);yield return Travel(2);yield return ReactionMapFrame("WR-picked-not-returned");
            var coins=game.Save.coins;yield return Click("Interact");NodeMarkerView.SetMotionMode(MarkerMotionMode.Full);
            yield return Click("EventOption_tutorial_return",true);
            Check(game.Quests.Completed&&!game.Quests.IsCompleted("HQ01")&&game.Save.coins==coins,"Returning Mắt Đèn completes only the unchanged tutorial, no early HQ01 reward");
            Check(GameObject.Find("EventText").GetComponent<Text>().text.Contains("Đèn Mắt Phố"),"Mai's real dialogue introduces the optional relic while keeping the city unrestored");
            Check(Map.LocalQuestReactionBursts==1,"A real Mai turn-in triggers exactly one local warmth/Lumi reaction");
            yield return Snapshot("WR-Mai-first-light-dialogue");yield return ReactionMapFrame("WR-B-local-first-light");CheckLocalPresentation(true);
            game.Persist();KeepSave("WR-local-complete");
            yield return ReactionWorldGate(false);CheckLocalPresentation(true);
            Check(Map.LocalQuestReactionBursts==0,"World Map re-entry keeps warmth but never replays the quest-complete burst");
            // Direct dependencies only: complete the existing Hoi An main quest
            // through real puzzle/bot/station choices. No fixture flags or items.
            Time.timeScale=4;
            yield return Travel(4,true);yield return SolvePuzzle("puzzle_market");
            yield return Travel(13,true);yield return SolvePuzzle("puzzle_paper");
            yield return Travel(19,true);yield return SolvePuzzle("puzzle_pottery");
            yield return Travel(5,true);yield return Click("EventOption_alternate",true);yield return CloseEvent();
            yield return Travel(7,true);coins=game.Save.coins;yield return Click("EventOption_restore",true);
            Check(game.Quests.IsCompleted("HQ01")&&game.Save.coins==coins+150,"Unchanged HQ01 still completes at H7 and pays exactly150 coins");
            CheckLocalPresentation(true);yield return CloseEvent();Time.timeScale=1;
            yield return ReactionMapFrame("WR-HQ01-network-not-citywide");
            var rewardCoins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();
            Check(!game.Events.Execute(7,"restore").changed&&game.Save.coins==rewardCoins&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Station cannot repeat rewards after presentation changes");
            yield return ReactionWorldGate(true);CheckLocalPresentation(true);
            game.Persist();KeepSave("WR-hq01-complete");
        }
        private IEnumerator H3ArtJourney()
        {
            var restart=Environment.GetCommandLineArgs().Contains("--h3-restart");
            if(!restart)game.ResetSave();
            if(restart)Check(game.Inventory.Count("lamp_eye")==1&&game.Quests.Completed,"Actual process restart retains earned unique Mắt Đèn and tutorial turn-in");
            else Check(game.Save.node==0&&!game.Quests.Active,"H3 acceptance starts H0 without synthetic quest flags");
            yield return Click("Start");yield return Scene("WorldMap");
            if(Environment.GetCommandLineArgs().Contains("--render-quality"))yield return Snapshot("Quality-WorldMap");
            yield return Click(HoiAnButton,true);yield return Click("EnterCity",true);yield return Scene("CityMap");
            if(Environment.GetCommandLineArgs().Contains("--render-quality"))
            {NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(new Vector2(.38f,.59f));yield return Snapshot("Quality-CityMap-Mai");}
            if(!restart)
            {
                yield return Travel(2,true);
                if(Environment.GetCommandLineArgs().Contains("--render-quality"))yield return Snapshot("Quality-Mai-dialogue");
                yield return Click("EventOption_accept",true);
                if(Environment.GetCommandLineArgs().Contains("--render-quality"))yield return Snapshot("Quality-Lumi-dialogue");
                yield return new WaitForSecondsRealtime(.5f);yield return Click("DialogueNext");yield return CloseEvent();
                Check(game.Quests.Active&&game.Quests.IsActive("HQ01"),"Mai's real NPC choice accepts the tutorial and HQ01");
                yield return Travel(3,true);yield return ExploreH3();
                Check(game.Inventory.Count("lamp_eye")==1,"Physical pickup awards exactly one main-story Mắt Đèn");
                if(Environment.GetCommandLineArgs().Contains("--render-quality"))
                {yield return CloseEvent();yield return Click("Inventory",true);yield return Snapshot("Quality-Inventory");yield return Click("CloseInventory",true);}
            }
            yield return Travel(3,true);
            if(FindButton("EventOption_scene")!=null)yield return Click("EventOption_scene",true);
            yield return Scene("HoiAnDemo");
            var player=FindAnyObjectByType<HoiAnPlayerController>();
            var item=FindAnyObjectByType<HoiAnItem>(FindObjectsInactive.Include);
            Check(!item.gameObject.activeSelf,"Entering/restarting H3 cannot respawn the already collected pickup");
            Check(FindObjectsByType<UIController>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"H3 reuses one HUD and one EventSystem");
            var ground=GameObject.Find("H3ContinuousStoneFloor").GetComponent<SpriteRenderer>();
            Check(ground.sprite!=null&&ground.sprite.texture.width>=1024,"Continuous ground-only material has no macro-tile seams; houses/props/player remain separate sprites");
            Check(!HoiAnPlayerController.Walkable(new Vector2(-6.3f,-2.5f))&&!HoiAnPlayerController.Walkable(new Vector2(6.2f,-2.4f)),"Both large plant footprints remain physical obstacles");
            foreach(var point in new[]{new Vector2(-5,1.8f),new Vector2(4.5f,1.8f),new Vector2(-5,3.75f),new Vector2(5,3.75f)})
                Check(!player.MoveTo(point),"Unwalkable house/garden destination rejected: "+point);
            var initial=player.Position;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.RightArrow));yield return new WaitForSecondsRealtime(.12f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Check(player.Position.x>initial.x+.1f,"Arrow-key movement still drives the original Rigidbody controller");
            var destination=player.Position+Vector2.up*.35f;
            Check(HoiAnPlayerController.Walkable(destination),"Mouse QA destination is clear of the preserved plant footprint");
            yield return Pointer(Camera.main.WorldToScreenPoint(destination),false);yield return Until(()=>!player.Walking,"mouse ground path");
            Check(Vector2.Distance(player.Position,destination)<.2f,"Mouse clicks move Lumi over the illustrated ground");
            var count=game.Inventory.Count("lamp_eye");yield return Click("Collect",true);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.E));yield return new WaitForSecondsRealtime(.1f);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            Check(game.Inventory.Count("lamp_eye")==count,"Touch button and E cannot duplicate an already collected item");
            if(restart)Check(FindAnyObjectByType<UIController>().Message.text==game.Quests.Status,"Completed tutorial retains original quest feedback without a new pickup or grant");
            yield return Snapshot(restart?"H3-restart-item-stays-collected":"H3-reentry-no-duplicate");
            if(Environment.GetCommandLineArgs().Contains("--h3-desktop"))
            {
                Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
                Check(FindButton("Collect").interactable&&FindButton("Return").interactable,"Landscape desktop retains accessible collect and return controls");
                yield return Snapshot("H3-desktop-1280x720");
            }
            yield return Click("Return",true);yield return Scene("CityMap");Check(game.Save.node==3,"H3 return preserves the map node after reentry/restart");
            if(!restart)
            {
                yield return Travel(2);yield return Click("EventOption_tutorial_return",true);yield return CloseEvent();
                Check(game.Quests.Completed&&game.Inventory.Count("lamp_eye")==1,"Real Mai turn-in preserves the main HQ01 artifact and original reward");
                game.Persist();KeepSave("h3-earned-final");
            }
            yield return Snapshot("H3-return-map-progress");
        }
    }
}
#endif
