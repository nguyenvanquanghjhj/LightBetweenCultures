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
        IEnumerator LightingFrame(string name,int width=1280,int height=960)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            Map.View.ResetView();Map.View.Focus(new Vector2(.5f,.5f));NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);
            yield return Until(()=>!Map.LightingTransitionActive,"lighting transition settles");
            yield return Snapshot(name);
            var corners=new Vector3[4];Map.View.Content.GetWorldCorners(corners);
            var min=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var max=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
            File.AppendAllText(Path.Combine(output,"lighting-measurements.txt"),
                name+" | "+Screen.width+"x"+Screen.height+" | state="+Map.HoiAnLightingState+" zoom="+Map.View.Zoom+
                " mapMin="+min+" mapMax="+max+" dimAlpha="+Map.BackgroundDimOpacity+" fullyBrightCoverage="+Map.DistrictBrightCoverage+
                " weightedCoverage="+Map.DistrictWeightedCoverage+" H2Alpha="+Map.DimmingAlphaAt(game.Data.nodes[2].position)+
                " farAlpha="+Map.DimmingAlphaAt(new Vector2(.94f,.17f))+Environment.NewLine);
        }
        void LightingStateCheck(string state)
        {
            Check(Map.HoiAnLightingState==state,"Actual saved quest/relic state selects lighting "+state);
            var mask=Map.View.Content.GetComponentsInChildren<RawImage>(true).Single(i=>i.name=="HoiAnBackgroundDimming");
            Check(mask.transform.parent==Map.View.Content&&mask.transform.GetSiblingIndex()==1,"Map-space mask draws immediately above canonical painting, beneath actors/routes/markers");
            Check(!mask.raycastTarget&&!mask.GetComponent<CanvasGroup>().blocksRaycasts&&!mask.GetComponent<CanvasGroup>().interactable,"Lighting cannot intercept mouse/touch/UI");
            Check(mask.rectTransform.anchorMin==Vector2.zero&&mask.rectTransform.anchorMax==Vector2.one,"Lighting mask follows complete map bounds, not screen");
            Check(Map.DistrictBrightCoverage>=.17f&&Map.DistrictBrightCoverage<=.20f&&Map.DistrictWeightedCoverage>=.195f&&Map.DistrictWeightedCoverage<=.205f,"Soft circular district restores about20 percent of the painting including its gradient");
            var h2=Map.DimmingAlphaAt(game.Data.nodes[2].position);var far=Map.DimmingAlphaAt(new Vector2(.94f,.17f));
            if(state=="A")Check(h2>.65f&&far>.65f&&!Map.LocalQuestLightComplete&&!Map.CityRelicIlluminated,"A dims both near/far background before actual tutorial hand-in");
            if(state=="B")Check(h2<.01f&&far>.65f&&Map.LocalQuestLightComplete&&!Map.CityRelicIlluminated,"B restores H2 district while distant background stays dim");
            if(state=="B")
            {
                var center=GameObject.Find("QuestLight_H2").GetComponent<RectTransform>().anchorMin;
                Check(Map.DimmingAlphaAt(center)<.01f,"StateB circle is centered on the existing lamp, not the arrival anchor");
                var radialValid=true;
                for(var i=0;i<8;i++)
                {
                    var direction=new Vector2(Mathf.Cos(i*Mathf.PI/4),Mathf.Sin(i*Mathf.PI/4));
                    var edge=center+Vector2.Scale(direction,new Vector2(316.2f/1448,316.2f/1086));
                    // CPU GetPixelBilinear addresses texel indices; compensate the
                    // half texel used by the existing mask's pixel-center raster.
                    var texture=mask.texture;
                    var sample=edge-new Vector2(.5f/texture.width,.5f/texture.height);
                    var alpha=Map.DimmingAlphaAt(sample);
                    File.AppendAllText(Path.Combine(output,"circle-samples.txt"),"center="+center.ToString("F6")+" edge="+edge.ToString("F6")+" rawAlpha="+Map.DimmingAlphaAt(edge)+" renderAlignedAlpha="+alpha+" expected="+Map.BackgroundDimOpacity*.5f+Environment.NewLine);
                    radialValid&=Mathf.Abs(alpha-Map.BackgroundDimOpacity*.5f)<.035f;
                }
                Check(radialValid,"StateB soft circular edge has consistent map-pixel radius; see circle-samples.txt");
            }
            if(state=="C")Check(h2==0&&far==0&&!mask.gameObject.activeSelf&&Map.CityRelicIlluminated,"C disables dim layer and restores canonical full background");
            foreach(var i in Enumerable.Range(0,game.Data.nodes.Length).Where(i=>!game.Discovery.IsVisible(i)))
                Check(game.Events.BuildOptions(i).options.Count==0,"Lighting does not expose hidden event "+game.Data.nodes[i].id);
        }
        IEnumerator LightingUiAndInput()
        {
            var before=JsonUtility.ToJson(game.Save);var sample=Map.DimmingAlphaAt(game.Data.nodes[2].position);
            yield return TestMapInput();
            Check(Mathf.Abs(Map.DimmingAlphaAt(game.Data.nodes[2].position)-sample)<.001f,"Zoom/pan cannot move district relative to H2");
            yield return Click("Inventory",true);yield return Click("CloseInventory");
            yield return Click("QuestJournal");yield return Click("CloseJournal",true);
            Check(JsonUtility.ToJson(game.Save)==before,"Mouse/touch zoom, pan, inventory and quest journal preserve exact save");
            Screen.SetResolution(540,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            Map.View.ZoomBy(2);Map.View.PanBy(new Vector2(-150,80));
            Check(Map.View.WithinBounds()&&Mathf.Abs(Map.DimmingAlphaAt(game.Data.nodes[2].position)-sample)<.001f,"Portrait zoom/pan preserves district anchor and bounds");
            yield return Snapshot("lighting-portrait-"+Map.HoiAnLightingState);
        }
        IEnumerator HoiAnLightingJourney()
        {
            if(!PcFull)game.ResetSave();yield return CrossEnter("hoi_an");
            yield return LightingFrame("A-before-tutorial");LightingStateCheck("A");KeepSave("state-A");
            var hiddenNode=NodeIndex("H19");var current=game.Save.node;Map.TryMove(hiddenNode);yield return null;
            Check(!Map.Moving&&game.Save.node==current&&Map.SelectedNode!=hiddenNode,"Dim background never unlocks hidden H19 movement");
            yield return Travel(0,true);yield return ClaimRelic("fragment_hoi_an_bamboo_frame",false);
            yield return Travel(2,true);yield return Click("EventOption_accept",true);yield return CloseEvent();
            yield return Travel(3,true);yield return ExploreH3();yield return Travel(2,true);yield return CloseEvent();
            Check(game.World.Has("hoi_an.lamp_eye_found")&&!game.Quests.IsCompleted("mai-lantern"),"Picked-up Eye has not completed Mai tutorial");
            yield return LightingFrame("A-eye-picked-not-returned");LightingStateCheck("A");KeepSave("state-A-picked");
            yield return RelicOption("tutorial_return");yield return CloseEvent();
            yield return LightingFrame("B-mai-returned");LightingStateCheck("B");KeepSave("state-B");
            Check(!game.Quests.IsCompleted("HQ01")&&!game.IsCityUnlocked("hue")&&!game.IsCityUnlocked("jeonju"),"Visual district recovery grants neither HQ01 nor world unlock");
            yield return LightingFrame("B-wide-district",1600,1000);
            Screen.SetResolution(1280,960,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            yield return ClaimRelic("fragment_hoi_an_eye_reflection",true);
            yield return Travel(4,true);yield return SolvePuzzle("puzzle_market");yield return ClaimRelic("fragment_hoi_an_old_flower_paper",false);
            yield return Travel(10,true);yield return ClaimRelic("fragment_hoi_an_roof_hook",true);
            yield return Travel(2,true);yield return RelicAssembly(true);
            Check(Map.HoiAnLightingState=="B","Four fragments and assembly before H7 cannot produce full light");
            Time.timeScale=4;yield return Travel(13,true);yield return SolvePuzzle("puzzle_paper");
            yield return Travel(19,true);yield return SolvePuzzle("puzzle_pottery");
            yield return Travel(5,true);yield return Click("EventOption_alternate",true);yield return CloseEvent();
            yield return Travel(7,true);var coins=game.Save.coins;yield return Click("EventOption_restore",true);yield return CloseEvent();Time.timeScale=1;
            Check(game.Quests.IsCompleted("HQ01")&&game.Save.coins==coins+150&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju"),"Unchanged H7 quest awards exactly 150 and opens Hue/Jeonju");
            if(PcFull)yield return PcBaitAndFerry();
            yield return Travel(2,true);yield return CloseEvent();
            yield return LightingFrame("B-after-H7-before-relic");LightingStateCheck("B");KeepSave("state-B-H7");
            yield return RelicOption("relic_restore");yield return Click("RelicTrace");
            Check(!game.Relics.Restored("relic_hoi_an_lantern"),"Wrong beam does not restore relic or switch lighting");
            for(var i=0;i<3;i++)yield return Click("RelicMirror"+i,i%2==0);
            coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();yield return Click("RelicTrace",true);
            Check(game.Relics.Restored("relic_hoi_an_lantern")&&Map.LightingTransitionActive,"Actual successful trial starts full-city spread");
            yield return RelicExit();yield return Snapshot("C-first-restoration-spread");
            yield return LightingFrame("C-full-city-restored");LightingStateCheck("C");KeepSave("state-C");
            Check(game.Save.coins==coins&&game.Save.rewardGrantIds.SequenceEqual(ledger)&&Map.RelicCelebrations==1,"Ritual changes presentation once, without currency or reward replay");
            Check(!game.Relics.PhotoRecorded&&!game.World.Has("checkin.hoi_an.h6"),"QA never opens or completes H6 postcard/check-in");
            yield return LightingUiAndInput();
            var layer=GameObject.Find("RelicIlluminationLayer");Check(layer.transform.childCount==10,"All ten existing landmark halos remain installed");
            KeepSave("lighting-final");
        }
        IEnumerator HoiAnLightingRestart()
        {
            yield return CrossEnter("hoi_an");var state=game.Relics.Restored("relic_hoi_an_lantern")?"C":game.Quests.IsCompleted("mai-lantern")?"B":"A";
            var coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();var items=game.Save.items.ToArray();
            yield return LightingFrame("restart-"+state);LightingStateCheck(state);
            Check(!Map.LightingTransitionActive&&Map.RelicCelebrations==0&&Map.LocalQuestReactionBursts==0,"Process restart applies saved light immediately without celebration");
            yield return LightingUiAndInput();
            var target=game.Data.nodes[game.Save.node].id=="H2"?0:2;yield return Travel(target,true);yield return CloseEvent();
            Check(Map.HoiAnLightingState==state,"Actual Lumi waypoint movement retains saved lighting");
            yield return CrossEnter("hoi_an");LightingStateCheck(state);
            Check(Map.RelicCelebrations==0&&Map.LocalQuestReactionBursts==0&&!Map.LightingTransitionActive,"World Map re-entry cannot replay restoration");
            if(state=="C")yield return LightingBridgeOcclusion();
            Check(game.Save.coins==coins&&game.Save.rewardGrantIds.SequenceEqual(ledger)&&game.Save.items.SequenceEqual(items),"Restart/re-entry/input/movement preserve exact inventory and reward ledger");
            Check(game.Save.version==2,"Lighting does not upgrade save schema");KeepSave("restart-final");
        }
        IEnumerator LightingBridgeOcclusion()
        {
            // Traverse the existing authored bridge route; never open H6/postcard UI or alter graph/state.
            yield return Travel(NodeIndex("H18"),true);yield return CloseEvent();
            Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(new Vector2(.69f,.48f));
            yield return Click("H5",true);var entered=false;var exited=false;var visibilityValid=true;var end=Time.realtimeSinceStartup+45;
            while(Map.Moving&&Time.realtimeSinceStartup<end)
            {
                visibilityValid&=Map.Marker.GetComponent<Image>().enabled==!Map.LumiOccluded;
                if(!entered&&Map.LumiOccluded||entered&&!exited&&!Map.LumiOccluded)
                {
                    if(Map.LumiOccluded)entered=true;else exited=true;
                    Time.timeScale=0;yield return Snapshot(entered&&!exited?"lighting-ChuaCau-under-roof":"lighting-ChuaCau-exit");Time.timeScale=1;
                }
                yield return null;
            }
            Check(visibilityValid,"Lighting preserves actual roof sprite visibility throughout movement");
            Check(entered&&exited&&!Map.Moving&&game.Save.node==NodeIndex("H5"),"Lumi hides under ChuaCau and reappears after real bridge traversal");
            yield return CloseEvent();Check(!game.Relics.PhotoRecorded&&!game.World.Has("checkin.hoi_an.h6"),"Bridge movement never invokes H6 postcard/check-in");
        }
    }
}
#endif
