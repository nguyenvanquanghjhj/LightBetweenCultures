#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        bool PortraitQA=>Environment.GetCommandLineArgs().Contains("--portrait-qa")||Environment.GetCommandLineArgs().Contains("--portrait-diagnostics")||Environment.GetCommandLineArgs().Contains("--portrait-handoff-only");
        readonly HashSet<string> portraitCaptures=new HashSet<string>();
        readonly List<string> portraitObservations=new List<string>();
        void PortraitCheck()
        {
            Check(GameObject.Find("ListenerPortrait")==null,"No ghost/listener portrait exists in the active dialogue");
            var portraits=FindObjectsByType<Image>(FindObjectsSortMode.None).Where(i=>i.name=="EventPortrait").ToArray();
            Check(portraits.Length==1,"One active speaker image owns the dialogue portrait slot");
            var image=portraits[0];var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
            var root=GameObject.Find("HUD").GetComponent<UIController>().Root;
            Check(corners.All(c=>RectTransformUtility.RectangleContainsScreenPoint(root,c,null)),"Full portrait rectangle fits inside the safe area");
            Check(image.sprite!=null&&image.preserveAspect&&image.color.a==1,"Speaker retains a real sprite, correct aspect and full image alpha");
        }
        IEnumerator ObservePortrait()
        {
            var definition=game.Events.Definition(game.Data.nodes[game.Save.node].eventId);
            if(string.IsNullOrEmpty(definition?.npcId))yield break;
            var expected=AdventureUIArt.Load().Npc(definition.npcId);
            if(expected==null)
            {
                var narration=GameObject.Find("EventPortrait")?.GetComponent<Image>();
                Check(definition.actor=="Lumi"?narration!=null&&narration.sprite==DesignKitArt.Load().lumiPortrait:narration==null,"Missing NPC art uses authored Lumi narration or no image, never a substitute final icon: "+definition.npcId);
                portraitObservations.Add(JsonUtility.ToJson(new PortraitObservation{city=game.Data.cityId,node=game.Data.nodes[game.Save.node].id,npcId=definition.npcId,sprite="MISSING",speaker=definition.actor,text=GameObject.Find("EventText").GetComponent<Text>().text}));
                File.WriteAllText(Path.Combine(output,"portrait-runtime-observations.json"),"["+string.Join(",",portraitObservations)+"]");
                yield break;
            }
            yield return new WaitForSecondsRealtime(.42f);PortraitCheck();
            Check(expected!=null,"Speaking NPC has an explicit registry binding: "+definition.npcId);
            var picture=GameObject.Find("EventPortrait").GetComponent<Image>();
            Check(picture.sprite==(expected.portrait??expected.sprite)&&picture.GetComponent<CanvasGroup>().alpha>.999f,"Actual dialogue renders the assigned full-opacity portrait: "+definition.npcId);
            var key=game.Data.cityId+"-"+game.Data.nodes[game.Save.node].id+"-stage"+game.Discovery.Stage(game.Data.cityId);
            portraitObservations.Add(JsonUtility.ToJson(new PortraitObservation{city=game.Data.cityId,node=game.Data.nodes[game.Save.node].id,npcId=definition.npcId,sprite=picture.sprite.name,speaker=GameObject.Find("EventSpeaker").GetComponent<Text>().text,text=GameObject.Find("EventText").GetComponent<Text>().text}));
            File.WriteAllText(Path.Combine(output,"portrait-runtime-observations.json"),"["+string.Join(",",portraitObservations)+"]");
            if(portraitCaptures.Add(key))yield return Snapshot("Portrait-"+key);
        }
        [Serializable] sealed class PortraitObservation {public string city,node,npcId,sprite,speaker,text;}
        IEnumerator PortraitHandoff(string direction)
        {
            var key=game.Data.cityId+"-"+game.Data.nodes[game.Save.node].id+"-"+direction;
            if(!portraitCaptures.Add(key))yield break;
            var state=JsonUtility.ToJson(game.Save);var samples=new List<string>();
            var fullMotion=NodeMarkerView.MotionMode==MarkerMotionMode.Full;var sawOld=false;var sawFractionalAlpha=false;
            var expectedTarget=direction=="to-Lumi"?DesignKitArt.Load().lumiPortrait:AdventureUIArt.Load().Npc(game.Events.Definition(game.Data.nodes[game.Save.node].eventId).npcId).sprite;
            for(var i=0;i<9;i++)
            {
                var image=GameObject.Find("EventPortrait").GetComponent<Image>();
                var alpha=image.GetComponent<CanvasGroup>().alpha;
                sawOld|=image.sprite!=expectedTarget;sawFractionalAlpha|=alpha>0&&alpha<.999f;
                Check(GameObject.Find("ListenerPortrait")==null&&FindObjectsByType<Image>(FindObjectsSortMode.None).Count(p=>p.name=="EventPortrait")==1,"Handoff never stacks the old and new portraits");
                samples.Add(Time.realtimeSinceStartup+" | "+image.sprite.name+" | alpha="+image.GetComponent<CanvasGroup>().alpha+" | slot="+image.rectTransform.anchoredPosition);
                var name="Speaker-"+key+"-"+i.ToString("D2");ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));captures.Add(DateTime.UtcNow.ToString("O")+" | "+name+".png");
                yield return new WaitForSecondsRealtime(.06f);
            }
            PortraitCheck();var portrait=GameObject.Find("EventPortrait").GetComponent<Image>();
            Check(portrait.GetComponent<CanvasGroup>().alpha>.999f,"Speaker transition settles without residual alpha");
            if(fullMotion){Check(sawOld,"Full-motion capture observes the previous speaker fading out before the swap");Check(sawFractionalAlpha,"Full-motion capture observes actual intermediate fade alpha");Check(portrait.sprite==expectedTarget,"Full-motion handoff ends on the intended speaker sprite");}
            if(direction=="to-Lumi")Check(portrait.sprite==DesignKitArt.Load().lumiPortrait,"Real delivery confirmation uses the approved navy Lumi dialogue portrait");
            Check(JsonUtility.ToJson(game.Save)==state,"Portrait handoff changes no quest, item, reward or world flag");
            File.WriteAllLines(Path.Combine(output,"Speaker-"+key+"-samples.txt"),samples);
        }
        IEnumerator PortraitHandoffOnly()
        {
            Check(game.Quests.IsCompleted("JQ01")&&game.Quests.IsActive("HQ05")&&!game.Quests.IsCompleted("HQ05")&&game.Inventory.Has("hoi_an_fresh_chili"),"Handoff resumes an earned Jeonju chapter before its real chili delivery");
            yield return CrossEnter("jeonju");yield return Travel(NodeIndex("J15"),true);
            var state=JsonUtility.ToJson(game.Save);NodeMarkerView.SetMotionMode(MarkerMotionMode.Full);
            yield return new WaitForSecondsRealtime(.42f);PortraitCheck();
            yield return FoodChoice("complete:foodexchange_jeonju_j15");
            Check(FindButton("DeliveryConfirm")!=null,"Full-motion evidence opens the actual delivery confirmation through touch input");
            yield return Click("DeliveryCancel");
            Check(JsonUtility.ToJson(game.Save)==state,"Actual full-motion confirm/cancel leaves earned quest, item and world state unchanged");
            yield return CloseEvent();yield return Click("Interact");yield return new WaitForSecondsRealtime(.42f);PortraitCheck();
            Check(GameObject.Find("EventPortrait").GetComponent<Image>().sprite==AdventureUIArt.Load().Npc("npc_soyeon").sprite,"Closing and reopening restores So-yeon without a stale Lumi portrait");
            KeepSave("handoff-preserved");NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
        }
        IEnumerator PortraitDiagnostics()
        {
            Check(game.Quests.IsCompleted("HQ01")&&game.World.Has("da_lat.beacon_lit"),"Portrait diagnostics resume genuinely completed chapters");
            foreach(var city in new[]{"hoi_an","hue","jeonju","da_lat"})
            {
                yield return CrossEnter(city);
                foreach(var node in game.Data.nodes.Where(n=>!string.IsNullOrEmpty(game.Events.Definition(n.eventId)?.npcId)).ToArray())
                    yield return Travel(NodeIndex(node.id),true);
            }
            yield return CrossEnter("hoi_an");yield return Travel(10);yield return CloseEvent();
            var panels=(GameplayPanels)typeof(CityMap).GetField("panels",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Map);
            var draw=typeof(GameplayPanels).GetMethod("DrawDialogue",BindingFlags.Instance|BindingFlags.NonPublic);
            var state=JsonUtility.ToJson(game.Save);
            foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(540,1200),new Vector2Int(960,540)})
            foreach(var longText in new[]{false,true})
            {
                Screen.SetResolution(resolution.x,resolution.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
                var ui=GameObject.Find("HUD").GetComponent<UIController>();
                if(resolution.y==1200)UIController.SetRect(ui.Root,new Vector2(.02f,.035f),new Vector2(.98f,.94f));
                var view=game.Events.BuildOptions(10);
                if(longText)view.text=string.Join("\n",Enumerable.Repeat(view.text,10));
                draw.Invoke(panels,new object[]{10,view,game.Events.Definition(game.Data.nodes[10].eventId),null,false,0});
                yield return new WaitForSecondsRealtime(.42f);PortraitCheck();Canvas.ForceUpdateCanvases();
                var sheet=GameObject.Find("NodeEventPanel").GetComponent<RectTransform>();var corners=new Vector3[4];sheet.GetWorldCorners(corners);
                Check(corners.All(c=>RectTransformUtility.RectangleContainsScreenPoint(ui.Root,c,null)),"Dialogue panel fits "+resolution+" long="+longText);
                foreach(var b in sheet.GetComponentsInChildren<Button>().Where(b=>b.name.StartsWith("EventOption_")))
                    Check(b.GetComponentInChildren<Text>().preferredHeight<=b.GetComponentInChildren<Text>().rectTransform.rect.height+1,"Option caption wraps without clipping");
                if(longText){var scroll=GameObject.Find("DialogueTextViewport").GetComponent<ScrollRect>();Check(scroll.content.rect.height>scroll.viewport.rect.height,"Long dialogue has real scroll overflow");scroll.verticalNormalizedPosition=0;}
                yield return Snapshot("Portrait-layout-FIXTURE-"+resolution.x+"x"+resolution.y+"-"+(longText?"long-scrolled":"short"));
                typeof(GameplayPanels).GetMethod("Clear",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(panels,null);
                if(resolution.y==1200)UIController.SetRect(ui.Root,Vector2.zero,Vector2.one);
            }
            Check(JsonUtility.ToJson(game.Save)==state,"Portrait/layout fixtures preserve exact earned progress");
        }
    }
}
#endif
