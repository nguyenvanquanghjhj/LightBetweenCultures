#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        private IEnumerator H3ArtJourney()
        {
            var restart=Environment.GetCommandLineArgs().Contains("--h3-restart");
            if(!restart)game.ResetSave();
            if(restart)Check(game.Inventory.Count("lamp_eye")==1&&game.Quests.Completed,"Actual process restart retains earned unique Mắt Đèn and tutorial turn-in");
            else Check(game.Save.node==0&&!game.Quests.Active,"H3 acceptance starts H0 without synthetic quest flags");
            yield return Click("Start");yield return Scene("WorldMap");
            yield return Click(HoiAnButton,true);yield return Click("EnterCity",true);yield return Scene("CityMap");
            if(!restart)
            {
                yield return Travel(2,true);yield return Click("EventOption_accept",true);
                yield return new WaitForSecondsRealtime(.5f);yield return Click("DialogueNext");yield return CloseEvent();
                Check(game.Quests.Active&&game.Quests.IsActive("HQ01"),"Mai's real NPC choice accepts the tutorial and HQ01");
                yield return Travel(3,true);yield return ExploreH3();
                Check(game.Inventory.Count("lamp_eye")==1,"Physical pickup awards exactly one main-story Mắt Đèn");
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
