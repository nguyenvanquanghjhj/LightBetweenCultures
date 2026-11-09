#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator PolishFrame(string name)
        {
            var file=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(file);
            yield return new WaitForEndOfFrame();yield return null;
            Check(File.Exists(file),"Animation frame written: "+name);
            captures.Add(DateTime.UtcNow.ToString("O")+" | "+Screen.width+"x"+Screen.height+" | "+name+".png");
        }
        IEnumerator PolishMotion(string buttonName)
        {
            var records=new List<string>();
            foreach(var mode in new[]{MarkerMotionMode.Full,MarkerMotionMode.Reduced,MarkerMotionMode.Off})
            {
                NodeMarkerView.SetMotionMode(mode);
                var button=FindButton(buttonName);var visual=button.GetComponent<UIButtonFeedback>();
                Check(visual!=null,"Shared button feedback installed: "+buttonName);
                var rect=(RectTransform)button.transform;var size=rect.rect.size;var p=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector2(rect.rect.xMin+.25f,rect.rect.center.y)));
                InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return null;yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=p}.WithButton(MouseButton.Left));
                yield return new WaitForSecondsRealtime(.14f);
                var pressed=visual.VisualScale;records.Add(mode+" pressed="+pressed+" hit="+rect.rect.size);
                Check(mode==MarkerMotionMode.Off?Mathf.Abs(pressed-1)<.001f:pressed<.995f&&pressed>=.959f,"Button press obeys "+mode+" motion preference");
                Check(rect.rect.size==size,"Button feedback preserves the touch target");
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);
                Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==button,"Actual raycast at the unscaled edge still reaches the pressed button in "+mode);
                yield return PolishFrame("Animation-"+mode+"-pressed");
                InputSystem.QueueStateEvent(mouse,new MouseState{position=p});yield return new WaitForSecondsRealtime(.18f);
                Check(Mathf.Abs(visual.VisualScale-1)<.002f,"Button releases softly in "+mode);
                yield return PolishFrame("Animation-"+mode+"-released");
            }
            File.WriteAllLines(Path.Combine(output,"button-motion-samples.txt"),records);
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
        }
        IEnumerator PolishReceiptMotion()
        {
            var real=JsonUtility.ToJson(game.Save);
            var frame=GameObject.Find("ItemReceipt");Check(frame!=null,"Actual 150-coin award opens a dedicated receipt");
            Check(GameObject.Find("ReceivedAmount_coin").GetComponent<Text>().text=="× 150","Receipt shows the actual source-defined 150 coins");
            Check(FindObjectsByType<Image>(FindObjectsSortMode.None).Count(i=>i.name=="ReceivedIcon_coin")==1,"The 150-coin receipt contains exactly one coin row");
            var dialog=GameObject.Find("NodeEventPanel").GetComponent<CanvasGroup>();
            Check(!dialog.blocksRaycasts&&!dialog.interactable&&dialog.alpha<.25f,"Reward temporarily owns input; dialogue is subordinate");
            yield return PolishFrame("Reward-150-entrance-early");yield return new WaitForSecondsRealtime(.12f);
            yield return PolishFrame("Reward-150-entrance-middle");yield return new WaitForSecondsRealtime(.20f);
            yield return PolishFrame("Reward-150-entrance-settled");
            Check(JsonUtility.ToJson(game.Save)==real,"Reward animation cannot grant coins or change gameplay twice");
            yield return Click("ReceiptContinue",true);
            Check(GameObject.Find("ItemReceipt")==null&&dialog.blocksRaycasts&&dialog.alpha==1,"Explicit receipt confirmation restores the dialogue");
            Check(JsonUtility.ToJson(game.Save)==real,"Closing a receipt is presentation only");
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
        }
        IEnumerator PolishPanelMotion()
        {
            var samples=new List<string>();
            foreach(var mode in new[]{MarkerMotionMode.Full,MarkerMotionMode.Reduced,MarkerMotionMode.Off})
            {
                yield return CloseEvent();NodeMarkerView.SetMotionMode(mode);yield return Click("Interact");
                var group=GameObject.Find("NodeEventPanel").GetComponent<CanvasGroup>();
                var early=group.alpha;samples.Add(mode+" early="+early);
                yield return PolishFrame("Panel-"+mode+"-early");yield return new WaitForSecondsRealtime(.12f);
                samples.Add(mode+" middle="+group.alpha);yield return PolishFrame("Panel-"+mode+"-middle");yield return new WaitForSecondsRealtime(.2f);
                Check(group.alpha>.999f,"Panel entrance settles in "+mode);
                if(mode==MarkerMotionMode.Off)Check(early==1,"Off mode shows the panel immediately");
                yield return PolishFrame("Panel-"+mode+"-settled");yield return Click("DialogueSkipEffect");
                Check(group.alpha==1,"Effect skip finishes presentation without executing a dialogue choice");
            }
            File.WriteAllLines(Path.Combine(output,"panel-motion-samples.txt"),samples);yield return CloseEvent();NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
        }
        IEnumerator UIPolishEvidence()
        {
            Check(game.Quests.IsCompleted("HQ01"),"Visual/route diagnostics resume a legitimately earned chapter save");
            yield return CrossEnter("hoi_an");
            Check(game.Data.nodes.Length==22&&!game.Data.nodes.Any(n=>n.id=="H22"),"No QA H22 in the delivered runtime catalog");
            Check(game.Data.edges.Length==22&&game.Data.nodes.All((n)=>game.Data.edges.Count(e=>e.from==Array.IndexOf(game.Data.nodes,n)||e.to==Array.IndexOf(game.Data.nodes,n))<=3),"Runtime graph has 22 edges and maximum degree 3, including ferry");
            foreach(var view in new[]{("Mai",new Vector2(.38f,.59f),2f),("WestDock",new Vector2(.30f,.29f),2.5f),("CoveredBridge",new Vector2(.72f,.46f),2f),("NorthStreet",new Vector2(.52f,.70f),1.75f)})
            {Map.View.ZoomBy(view.Item3/Map.View.Zoom);Map.View.Focus(view.Item2);yield return Snapshot("After-"+view.Item1);}
            Screen.SetResolution(1152,864,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);Map.View.ResetView();
            yield return Snapshot("After-overview-clean");
            var ui=GameObject.Find("HUD").GetComponent<UIController>();
            foreach(var node in game.Data.nodes)
            {
                var plate=ui.Panel(Map.View.Content,"QA_"+node.id,node.position,node.position,new Color(.05f,.05f,.07f,.9f));plate.sizeDelta=new Vector2(28,18);plate.anchoredPosition=new Vector2(0,-18);plate.GetComponent<Image>().raycastTarget=false;
                ui.Label(plate,"Id",node.id,Vector2.zero,Vector2.one,12,false).color=Color.magenta;
            }
            yield return Snapshot("After-QC-node-anchors");foreach(Transform child in Map.View.Content)if(child.name.StartsWith("QA_"))Destroy(child.gameObject);
            var paths=Map.View.Content.GetComponent<CityMapPaths>();
            Check(Map.View.Content.Find("RouteDots")!=null&&!FindObjectsByType<Image>(FindObjectsSortMode.None).Any(i=>i.name.StartsWith("Path_")),"Batched dot mesh replaces continuous per-segment path Images");
            CheckRoute35Dots();
            // Earn the optional ferry through the real existing quest; no QA
            // world flag is used to turn a locked route into E2E evidence.
            if(!game.Quests.IsCompleted("HQ04"))
            {
                Check(!game.Data.EdgeOpen(game.Data.edges.First(e=>e.travel=="ferry"),game.Save),"Ferry remains closed before HQ04");
                yield return Travel(17);var coins=game.Save.coins;
                yield return FoodChoice("accept");yield return FoodChoice("complete");
                Check(game.Quests.IsCompleted("HQ04")&&game.World.Has("hoi_an.ferry_available")&&game.Save.coins==coins-5,"Actual HQ04 hand-in opens the ferry and consumes exactly five coins");
                KeepSave("polish-ferry-earned");yield return Snapshot("HQ04-ferry-earned");yield return CloseEvent();
            }
            if(Environment.GetCommandLineArgs().Contains("--all-route-diagnostics"))yield return ChapterRouteDiagnostics();
            yield return PolishPanelMotion();
            yield return Travel(10);yield return CloseEvent();
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            yield return PolishMotion("ZoomIn");
            var original=JsonUtility.ToJson(game.Save);
            var panels=(GameplayPanels)typeof(CityMap).GetField("panels",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Map);
            var draw=typeof(GameplayPanels).GetMethod("DrawDialogue",BindingFlags.Instance|BindingFlags.NonPublic);
            foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(540,1200),new Vector2Int(960,540)})
            foreach(var longText in new[]{false,true})
            {
                Screen.SetResolution(resolution.x,resolution.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
                ui=GameObject.Find("HUD").GetComponent<UIController>();if(resolution.y==1200)UIController.SetRect(ui.Root,new Vector2(.02f,.035f),new Vector2(.98f,.94f));Canvas.ForceUpdateCanvases();
                var view=new EventView{title="QA layout fixture",speaker="Bố cục kiểm thử",text=longText?string.Join("\n",Enumerable.Repeat("Đoạn kiểm thử hội thoại dài: lời thoại phải cuộn được, không bị cắt và không thay đổi tiến độ nhiệm vụ.",12)):"Cảm ơn cháu đã quay lại!"};
                for(var i=0;i<(longText?3:1);i++)view.options.Add(new EventOption("qa_"+i,longText?"Lựa chọn dài để kiểm tra xuống dòng và vùng chạm trên điện thoại dọc, không thực thi gameplay":"Tiếp tục câu chuyện"));
                draw.Invoke(panels,new object[]{game.Save.node,view,game.Events.Definition(game.Data.nodes[game.Save.node].eventId),null,false,0});yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
                var sheet=GameObject.Find("NodeEventPanel").GetComponent<RectTransform>();var corners=new Vector3[4];sheet.GetWorldCorners(corners);
                Check(corners.All(c=>RectTransformUtility.RectangleContainsScreenPoint(ui.Root,c,null)),"Dialogue stays inside safe area "+resolution+" long="+longText);
                foreach(var button in sheet.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("EventOption_")))
                {var label=button.GetComponentInChildren<Text>();Check(!label.resizeTextForBestFit&&label.rectTransform.rect.height+1>=label.preferredHeight,"Option wraps at readable size: "+resolution);}
                var scroll=GameObject.Find("DialogueTextViewport").GetComponent<ScrollRect>();if(longText){Check(scroll.content.rect.height>scroll.viewport.rect.height,"Long dialogue actually overflows into a scroll viewport");scroll.verticalNormalizedPosition=0;}
                yield return Snapshot("Dialogue-layout-FIXTURE-"+resolution.x+"x"+resolution.y+"-"+(longText?"long-scrolled":"short"));
                typeof(GameplayPanels).GetMethod("Clear",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(panels,null);
                if(resolution.y==1200)UIController.SetRect(ui.Root,Vector2.zero,Vector2.one);
            }
            Check(JsonUtility.ToJson(game.Save)==original,"Layout stress fixtures preserve the exact earned save");
            game.Persist();
        }
        IEnumerator UIPolishRestart()
        {
            var expected=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(game.Save));
            var city=game.Save.cityId;var node=game.Save.node;var stage=game.Discovery.Stage(city);
            yield return CrossEnter(city);
            // Existing city re-entry deliberately resets the retreat origin.
            expected.previousNode=node;
            Check(JsonUtility.ToJson(game.Save)==JsonUtility.ToJson(expected),"Fresh process re-entry preserves the exact earned save except documented retreat-origin reset");
            Check(game.Discovery.Stage(city)==stage&&game.Save.node==node,"Restart restores city/node/discovery stage "+city+"/"+stage);
            for(var i=0;i<game.Data.nodes.Length;i++)
            {
                var n=game.Data.nodes[i];var visible=n.revealStage<=stage&&(n.requiredFlags??Array.Empty<string>()).All(game.Save.flags.Contains);
                Check(Map.View.Content.Find(n.id).gameObject.activeSelf==visible,"Restart rendering/input preserves saved reveal: "+n.id);
                if(!visible)Check(game.Data.FindPath(node,i,game.Save)==null,"Restart cannot route into a hidden node "+n.id);
            }
            CheckMapActors("Restart "+city+" stage "+stage);
            yield return Snapshot("Restart-"+city+"-stage"+stage);
        }
    }
}
#endif
