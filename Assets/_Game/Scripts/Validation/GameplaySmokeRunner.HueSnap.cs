#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator HueHeldDrag(RectTransform piece,Vector2 destination,Vector2 grip,bool finger)
        {
            var start=SLPoint(piece)+grip;var end=destination+grip;
            if(finger)TouchQueue(91,UnityEngine.InputSystem.TouchPhase.Began,start);
            else{InputSystem.QueueStateEvent(mouse,new MouseState{position=start});yield return null;InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));}
            yield return null;yield return null;
            for(var i=1;i<=20;i++)
            {
                var p=Vector2.Lerp(start,end,i/20f);
                if(finger)TouchQueue(91,UnityEngine.InputSystem.TouchPhase.Moved,p);
                else InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left));
                yield return new WaitForSecondsRealtime(.015f);
            }
        }
        IEnumerator HueRelease(Vector2 at,bool finger)
        {
            if(finger)TouchQueue(91,UnityEngine.InputSystem.TouchPhase.Ended,at);
            else InputSystem.QueueStateEvent(mouse,new MouseState{position=at});
            yield return null;yield return null;
        }
        IEnumerator HueSnapJourney()
        {
            musicVirtualFocus=true;const string id="puzzle_market_code";
            var definition=game.Content.Puzzle(id);
            // Reset exactly this already-earned puzzle inside the runner's isolated
            // save, leaving the owner's save and every production definition alone.
            game.Save.solvedPuzzles.Remove(id);game.Save.flags.RemoveAll(f=>(definition.flags??Array.Empty<string>()).Contains(f));
            game.Save.items.RemoveAll(s=>s=="river_pass");game.Save.rewardGrantIds.RemoveAll(s=>s.StartsWith("puzzle:"+id+":"));game.Persist();
            yield return CrossEnter("hue");Time.timeScale=6;yield return Travel(NodeIndex("U16"),true);Time.timeScale=1;yield return CloseEvent();
            yield return Click("Interact",true);yield return ChapterChoice("puzzle");
            var a=game.Puzzles.Activity;
            Check(a!=null&&a.EasyPlacement&&a.placed.Length==2,"Exactly Hue U16 two-stamp challenge uses forgiving placement");
            Check(ActivityPresentation.Load().entries.Where(e=>e.id!=id).All(e=>!new StoryActivity(e).EasyPlacement),"Other story activities do not opt into this change");
            var untouched=JsonUtility.ToJson(game.Save);var pieces=new RectTransform[2];var targets=new RectTransform[2];
            for(var i=0;i<2;i++){pieces[i]=GameObject.Find("MicroPiece"+i).GetComponent<RectTransform>();targets[i]=GameObject.Find("MicroTarget"+i).GetComponent<RectTransform>();}
            Canvas.ForceUpdateCanvases();var tray=pieces[1].position;
            yield return SLDrag(SLPoint(pieces[1]),SLPoint(targets[0]),true);
            Check(!a.placed.Any(p=>p)&&Vector3.Distance(pieces[1].position,tray)<.01f,"Wrong matching slot returns only that stamp immediately; no reset or penalty");
            Check(JsonUtility.ToJson(game.Save)==untouched,"Incorrect drop changes no quest, item, coin or proof");
            var corner=targets[0].TransformPoint(new Vector3(-50,56));
            var grip=(Vector2)pieces[0].TransformVector(new Vector3(22,10));
            yield return HueHeldDrag(pieces[0],RectTransformUtility.WorldToScreenPoint(null,corner),grip,false);
            Check(a.placed[0]&&pieces[0].GetComponent<RelicDragHandle>().Locked,"Off-center mouse grip snaps near the target corner before release, even before inspection");
            Check(pieces[0].parent==targets[0]&&pieces[0].anchoredPosition==Vector2.zero,"Accepted stamp is centered and attached to its target");
            yield return HueRelease(SLPoint(targets[0])+grip,false);
            Check(a.placed[0]&&!a.Ready&&!game.Puzzles.Solved(id)&&JsonUtility.ToJson(game.Save)==untouched,"Drop counts immediately once without bypassing inspection/printing/reward conditions");
            yield return SLDrag(SLPoint(pieces[0]),SLPoint(pieces[0])+Vector2.left*140,true);
            Check(a.placed[0]&&pieces[0].parent==targets[0]&&pieces[0].anchoredPosition==Vector2.zero,"Placed stamp cannot be dragged out or undone");
            var near=targets[1].TransformPoint(new Vector3(48,-54));
            yield return HueHeldDrag(pieces[1],RectTransformUtility.WorldToScreenPoint(null,near),Vector2.zero,true);
            Check(a.placed.All(p=>p)&&pieces[1].GetComponent<RelicDragHandle>().Locked,"Second stamp accepts a forgiving touch placement once; first remains locked");
            yield return HueRelease(SLPoint(targets[1]),true);
            yield return PilotResolution(600,1000);Canvas.ForceUpdateCanvases();
            Check(pieces.Select((p,i)=>p.parent==targets[i]&&p.anchoredPosition==Vector2.zero).All(x=>x),"Both stamps remain snapped after portrait canvas resize");
            yield return Click("MicroConfirm",true);Check(!game.Puzzles.Solved(id)&&JsonUtility.ToJson(game.Save)==untouched,"Both placements alone never bypass the original completion gates");
            for(var i=0;i<a.inspected.Length;i++)yield return Click("MicroInspect"+i,true);
            Check(a.Prepared&&!a.Ready,"Original two inspections still required; printing remains a real action");
            yield return Snapshot("HUE-SNAP-locked-stamps");
            yield return SLHold();Check(a.Ready&&!game.Puzzles.Solved(id),"Existing hold-to-print completes the original gesture without automatic reward");
            yield return Click("MicroConfirm",true);
            Check(game.Puzzles.Solved(id)&&(definition.flags??Array.Empty<string>()).All(game.World.Has),"Original confirm records the same canonical proof and flags");
            Check(game.Inventory.Count("river_pass")==1&&game.Save.rewardGrantIds.Count(s=>s=="puzzle:"+id+":river_pass")==1,"Original river pass awarded exactly once");
            yield return CloseEvent();var saved=JsonUtility.ToJson(game.Save);
            Check(!game.Puzzles.Start(id)&&JsonUtility.ToJson(game.Save)==saved,"Completed challenge cannot duplicate its reward");
            game.Reload();Check(JsonUtility.ToJson(game.Save)==saved,"Save/load preserves the completed puzzle and exact reward state");
        }
    }
}
#endif
