#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator GelButtonsJourney()
        {
            musicVirtualFocus=true;
            var styled=Environment.GetCommandLineArgs().Contains("--gel-after");
            var original=JsonUtility.ToJson(game.Save);
            yield return Scene("Prototype");yield return Snapshot("Gel-Main");
            yield return Click("MainGuide",true);yield return new WaitForSecondsRealtime(.35f);
            yield return Snapshot("Gel-Help");
            var button=FindButton("GuideNext");var rect=(RectTransform)button.transform;
            var hitSize=rect.rect.size;var hitPosition=rect.position;var oldText=GameObject.Find("GuideBody").GetComponent<Text>().text;
            var begin=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
            if(styled)Check(button.targetGraphic.GetType().Name=="SoftButtonGraphic","Shared native gel style reaches the real help button");
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Full);
            InputSystem.QueueStateEvent(mouse,new MouseState{position=begin});yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=begin}.WithButton(MouseButton.Left));yield return new WaitForSecondsRealtime(.16f);
            if(styled)
            {
                Check(button.GetComponent<UIButtonFeedback>().VisualScale<.98f,"Pressed face visibly depresses without scaling its hit area");
                Check(rect.rect.size==hitSize&&rect.position==hitPosition,"Press leaves the button hit target and layout fixed");
            }
            yield return Snapshot("Gel-Pressed");
            var outside=begin+Vector2.up*180;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=outside}.WithButton(MouseButton.Left));yield return null;yield return null;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=outside});yield return new WaitForSecondsRealtime(.18f);
            Check(GameObject.Find("GuideBody").GetComponent<Text>().text==oldText,"Cancelling a visual press never advances the guide");
            button.interactable=false;yield return new WaitForSecondsRealtime(.25f);yield return Snapshot("Gel-Disabled");
            TouchQueue(81,UnityEngine.InputSystem.TouchPhase.Began,begin);yield return null;yield return null;
            TouchQueue(81,UnityEngine.InputSystem.TouchPhase.Ended,begin);yield return new WaitForSecondsRealtime(.15f);
            Check(GameObject.Find("GuideBody").GetComponent<Text>().text==oldText,"Disabled visual fixture cannot execute its real callback");
            button.interactable=true;yield return new WaitForSecondsRealtime(.2f);yield return Click("GuideNext",true);
            Check(GameObject.Find("GuideBody").GetComponent<Text>().text!=oldText,"Enabled guide Next still performs the original action exactly once");
            yield return Click("GuideSkip",true);Check(JsonUtility.ToJson(game.Save)==original,"Guide visual/state tests leave gameplay save untouched");
            yield return CrossEnter("hoi_an");Time.timeScale=4;yield return Travel(NodeIndex("H10"),true);Time.timeScale=1;yield return CloseEvent();
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);Map.View.ZoomBy(3/Map.View.Zoom);
            var actor=MapActor("H10");Map.View.Focus((CityRouteGeometry.Arrival(game.Data.nodes[NodeIndex("H10")])+StandingFoot(actor))*.5f);
            yield return Snapshot("Gel-Map");
            var saveBefore=JsonUtility.ToJson(game.Save);yield return Click("Interact",true);yield return Snapshot("Gel-Dialogue");
            Check(FindButton("EventMore")==null,"Visual styling preserves the simplified menu with no 1/2 pagination");
            yield return Click("DialogueCloseX",true);Check(JsonUtility.ToJson(game.Save)==saveBefore,"Styled dialogue X cannot consume, accept or grant anything");
            // Separate QA copy: a real unfinished story activity, without replaying
            // a completed puzzle or altering the owner's actual save.
            game.Scenes.Go("Prototype");yield return Scene("Prototype");
            var seed=Environment.GetCommandLineArgs().First(a=>a.StartsWith("--gel-partial-save=")).Substring("--gel-partial-save=".Length);
            File.Copy(seed,savePath,true);game.Reload();yield return CrossEnter("hoi_an");
            Time.timeScale=4;yield return Travel(NodeIndex("H4"),true);Time.timeScale=1;
            yield return Click("EventOption_puzzle",true);
            Check(game.Puzzles.Solved("puzzle_market"),"The existing short H4 interaction supplies the required clue for H13");
            yield return CloseEvent();Time.timeScale=4;yield return Travel(NodeIndex("H13"),true);Time.timeScale=1;
            yield return Click("EventOption_puzzle",true);yield return new WaitForSecondsRealtime(.3f);
            Check(GameObject.Find("StoryActivityPanel")!=null||GameObject.Find("PuzzlePanel")!=null,"Original puzzle action opens an actual playable activity panel");
            yield return Snapshot("Gel-Minigame");
            saveBefore=JsonUtility.ToJson(game.Save);yield return CloseEvent();
            Check(!Map.DialogueOpen&&!Map.Moving&&JsonUtility.ToJson(game.Save)==saveBefore,"Styled activity exit restores input without granting completion or rewards");
            Check(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Styling creates no extra EventSystem");
            KeepSave("Gel-unchanged-progress");
        }
    }
}
#endif
