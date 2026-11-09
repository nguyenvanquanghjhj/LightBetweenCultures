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
        IEnumerator DialogueReleaseJourney()
        {
            musicVirtualFocus=true;PlayerPrefs.SetInt(PlayerCopy.GuideKey,1);
            var earned=JsonUtility.ToJson(game.Save);
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();
                var e=game.Content.events.First(x=>x.cityId==city.id&&!x.additive&&x.type=="npc");
                var i=NodeIndex(e.nodeId);var state=JsonUtility.ToJson(game.Save);
                UXPanels.ShowEvent(i);yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
                var text=GameObject.Find("EventText").GetComponent<Text>();
                Check(!string.IsNullOrWhiteSpace(text.text)&&FindButton("DialogueCloseX")!=null,"Representative dialogue renders with close control: "+city.id);
                yield return Click("DialogueCloseX",true);
                Check(JsonUtility.ToJson(game.Save)==state,"Read/close preserves quest/items/rewards: "+city.id);
            }
            yield return CrossEnter("hoi_an");yield return CloseEvent();
            var node=NodeIndex("H7");var definition=game.Events.Definition(game.Data.nodes[node].eventId);
            var view=game.Events.BuildOptions(node);UXPanels.ShowEvent(node);yield return new WaitForSecondsRealtime(.35f);Canvas.ForceUpdateCanvases();
            var body=GameObject.Find("EventText").GetComponent<Text>();var original=PlayerCopy.Display(view.text);
            Check(body.text.Length<original.Length&&body.text.Contains("Huế")&&body.text.Contains("Jeonju")&&body.text.Contains("Mắt Đèn")&&body.text.Contains("Lõi Giấy")&&body.text.Contains("Chìa Kết Nối")&&body.text.Contains("Đèn Mắt Phố"),"H7 shorter display preserves destinations, components and separate relic story");
            Check(GameObject.Find("EventPortrait")==null,"NodePanelCompact retained: station has no large category badge");
            var scroll=GameObject.Find("DialogueTextViewport").GetComponent<ScrollRect>();
            Check(scroll.content.rect.height>scroll.viewport.rect.height&&GameObject.Find("DialogueScrollHint")!=null,"Long dialogue visibly indicates more text");
            var before=JsonUtility.ToJson(game.Save);var point=RectTransformUtility.WorldToScreenPoint(null,scroll.viewport.TransformPoint(scroll.viewport.rect.center));
            TouchQueue(49,UnityEngine.InputSystem.TouchPhase.Began,point+Vector2.down*45);yield return null;yield return null;
            for(var s=1;s<=8;s++){TouchQueue(49,UnityEngine.InputSystem.TouchPhase.Moved,point+Vector2.down*45+Vector2.up*s*16);yield return null;}
            TouchQueue(49,UnityEngine.InputSystem.TouchPhase.Ended,point+Vector2.up*83);yield return new WaitForSecondsRealtime(.2f);
            Check(scroll.verticalNormalizedPosition<.95f&&JsonUtility.ToJson(game.Save)==before,"Simulated finger swipe scrolls text without triggering actions");
            scroll.verticalNormalizedPosition=0;yield return null;yield return null;
            Check(GameObject.Find("DialogueScrollHint").GetComponent<Text>().text.Contains("phía trên"),"Reading hint changes direction at end of text");
            yield return CloseEvent();
            node=NodeIndex("H19");UXPanels.ShowEvent(node);yield return new WaitForSecondsRealtime(.35f);
            Check(GameObject.Find("DialogueScrollHint")==null&&GameObject.Find("EventPortrait")==null,"Short information has neither unnecessary hint nor large badge");yield return CloseEvent();
            game.Save.node=NodeIndex("H3");game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Map.Interact();yield return new WaitForSecondsRealtime(.35f);
            Check(FindButton("EventOption_scene").GetComponentInChildren<Text>().text=="Khám phá lại khu hẻm","H3 replay label describes actual exploration action");
            yield return Click("EventOption_scene",true);yield return Scene("HoiAnDemo");
            Check(game.Save.node==3,"Unchanged H3 callback opens existing exploration scene");
            Check(game.Persist(),"Save writes after focused interaction");var saved=JsonUtility.ToJson(game.Save);game.Reload();
            Check(JsonUtility.ToJson(game.Save)==saved,"Exact quest/items/coins/reward state survives reload");
            // Reuse the existing quest accept/decline/hand-in regression on private fixtures.
            yield return UXFixture(JsonUtility.ToJson(new SaveData()));yield return NpcButtonsJourney();
        }
        IEnumerator NodePanelJourney()
        {
            musicVirtualFocus=true;PlayerPrefs.SetInt(PlayerCopy.GuideKey,1);
            var baseline=Environment.GetCommandLineArgs().Contains("--node-panel-before");
            var observations=new System.Collections.Generic.List<string>();
            yield return CrossEnter("hoi_an");yield return CloseEvent();
            foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
            {
                yield return PilotResolution(resolution.x,resolution.y);
                Check(Screen.width==resolution.x&&Screen.height==resolution.y,"Actual rendered resolution: "+resolution);
                // Private earned-save location fixtures; no route/approach test or owner save mutation.
                foreach(var id in new[]{"H7","H19","H20","H8","H3","H2","H5"})
                {
                    game.Save.node=NodeIndex(id);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                    var state=JsonUtility.ToJson(game.Save);
                    Map.Interact();yield return Until(()=>Map.DialogueOpen,"open "+id,8);
                    yield return new WaitForSecondsRealtime(.4f);Canvas.ForceUpdateCanvases();
                    var panel=GameObject.Find("NodeEventPanel").GetComponent<RectTransform>();
                    var portrait=GameObject.Find("EventPortrait")?.GetComponent<Image>();
                    observations.Add(resolution+" | "+id+" | portrait="+(portrait?.sprite?.name??"NONE")+" | panel="+panel.rect.size);
                    if(!baseline&&id!="H2"&&id!="H5")Check(portrait==null,"No large category badge or empty portrait: "+id);
                    if(id=="H2"||id=="H5")Check(portrait!=null&&portrait.sprite!=null,"Real NPC/enemy portrait is retained: "+id);
                    var corners=new Vector3[4];panel.GetWorldCorners(corners);
                    var root=GameObject.Find("HUD").GetComponent<UIController>().Root;
                    Check(corners.All(c=>RectTransformUtility.RectangleContainsScreenPoint(root,c,null)),"Panel fits safe area: "+id+" "+resolution);
                    Check(!string.IsNullOrWhiteSpace(GameObject.Find("EventSpeaker").GetComponent<Text>().text)&&!string.IsNullOrWhiteSpace(GameObject.Find("EventText").GetComponent<Text>().text),"Title and description remain visible: "+id);
                    var textRect=GameObject.Find("DialogueTextViewport").GetComponent<RectTransform>();
                    Check(Mathf.Abs(textRect.rect.width-(panel.rect.width-48))<1&&Mathf.Abs(textRect.anchoredPosition.x-24)<1,"Description uses full width with balanced left/right margins: "+id);
                    if(!baseline&&GameObject.Find("EventLocation")==null)
                        Check(Mathf.Abs(textRect.anchoredPosition.y+60)<1,"Duplicate location row leaves no empty gap: "+id);
                    foreach(var button in panel.GetComponentsInChildren<Button>())
                    {
                        var rect=(RectTransform)button.transform;
                        Check(rect.rect.height>=48,"Action/close keeps finger-height target: "+id+"/"+button.name);
                    }
                    Check(JsonUtility.ToJson(game.Save)==state,"Opening panel is read-only: "+id);
                    if(id!="H5")yield return Snapshot("NodePanel-"+(baseline?"before-":"after-")+id+"-"+resolution.x+"x"+resolution.y);
                    yield return Click("DialogueCloseX",resolution.x==720);
                    Check(!Map.DialogueOpen&&Map.View.InputEnabled&&JsonUtility.ToJson(game.Save)==state,"X closes and restores map without changing progress: "+id);
                }
            }
            if(!baseline)
            {
                var state=JsonUtility.ToJson(game.Save);
                UXPanels.ShowEvent(NodeIndex("H2"));UXPanels.ShowEvent(NodeIndex("H7"));
                Check(!GameObject.Find("GameplayOverlay").GetComponentsInChildren<Image>().Any(i=>i.name=="EventPortrait"),"NPC-to-information redraw never revives a fading portrait or blank image");
                var resolve=typeof(GameplayPanels).GetMethod("DialoguePortrait",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                Check(resolve.Invoke(UXPanels,new object[]{null,false,false})==null,"Unknown/missing event has no fallback category portrait");
                var information=game.Content.events.Where(e=>string.IsNullOrEmpty(e.npcId)&&!e.legacyTutorial&&e.type!="encounter"&&e.actor!="Lumi").ToArray();
                Check(information.All(e=>resolve.Invoke(UXPanels,new object[]{e,false,false})==null),"All "+information.Length+" non-character definitions across six cities omit the large category badge");
                yield return CloseEvent();Check(JsonUtility.ToJson(game.Save)==state,"Portrait redraw diagnostics preserve exact earned progress");
            }
            // Exercise real retained callbacks with both mouse and simulated touch.
            game.Save.node=NodeIndex("H8");game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Map.Interact();yield return new WaitForSecondsRealtime(.35f);
            var portal=game.Events.BuildOptions(game.Save.node).options.First(o=>o.enabled&&o.id=="portal");
            yield return Click("EventOption_"+portal.id);yield return Scene("WorldMap");
            Check(game.Save.cityId=="hoi_an","Portal callback opens travel preview without changing city/save destination");
            yield return CrossEnter("hoi_an");yield return CloseEvent();
            game.Save.node=NodeIndex("H3");game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
            Map.Interact();yield return new WaitForSecondsRealtime(.35f);
            yield return Click("EventOption_scene",true);yield return Scene("HoiAnDemo");
            Check(game.Save.cityId=="hoi_an"&&game.Save.node==3,"Exploration callback still opens existing alley with correct node");
            File.WriteAllLines(Path.Combine(output,"panel-observations.txt"),observations);
        }
    }
}
#endif
