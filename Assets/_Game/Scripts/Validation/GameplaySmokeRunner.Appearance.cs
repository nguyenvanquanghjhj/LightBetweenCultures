#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        void CloseAppearancePanel(GameplayPanels panels)=>typeof(GameplayPanels).GetMethod("Close",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(panels,null);
        IEnumerator AppearanceJourney()
        {
            musicVirtualFocus=true;
            var fixture=Resources.Load<AppearanceReviewData>("AppearanceReviewData");
            Check(fixture!=null,"Development-only before/after sprite fixture exists");
            var before=Environment.GetCommandLineArgs().Contains("--appearance-before");var label=before?"before":"after";
            var tail=Environment.GetCommandLineArgs().Contains("--appearance-tail");
            var art=AdventureUIArt.Load();var kit=DesignKitArt.Load();var canonical=kit.lumiMapTraveler;
            var changed=new[]{"npc_nguyet","npc_elder","npc_hana"};
            var initial=ScriptableObject.CreateInstance<AdventureUIArt>();JsonUtility.FromJsonOverwrite(fixture.mapActorsBefore,initial);
            Check(art.mapActors.Length==initial.mapActors.Length&&art.mapActors.Zip(initial.mapActors,(a,b)=>JsonUtility.ToJson(a)==JsonUtility.ToJson(b)).All(x=>x),"All canonical NPC placements, offsets, standing targets and map scale metadata are unchanged");
            Destroy(initial);
            for(var i=0;i<fixture.npcIds.Length;i++)
            {
                var n=art.Npc(fixture.npcIds[i]);
                Check(changed.Contains(n.id)?n.sprite!=fixture.beforeSprites[i]&&n.portrait==n.sprite:n.sprite==fixture.beforeSprites[i]&&(n.portrait?n.portrait:n.sprite)==fixture.beforePortraits[i],"Exactly requested NPC binding policy: "+n.id);
                if(before&&changed.Contains(n.id)){n.sprite=fixture.beforeSprites[i];n.portrait=fixture.beforePortraits[i];}
            }
            if(before){kit.lumiPortrait=fixture.beforeLumiPortrait;kit.lumiExploration=fixture.beforeLumiExploration;}
            else Check(new[]{kit.lumiPortrait,kit.lumiExploration,kit.lumiMapTraveler,kit.lumiCity,kit.lumiCitySmall}.All(s=>s==canonical),"All five Lumi registry roles resolve to the exact owner-approved white cutout");
            if(!tail)
            {
            // Main menu was created before the opt-in baseline swap. Reload only that view.
            game.Scenes.Go("Prototype");yield return Scene("Prototype");yield return new WaitForSecondsRealtime(.2f);
            Check(GameObject.Find("CanonicalLumiPortrait").GetComponent<Image>().sprite==kit.lumiPortrait,"Main menu uses selected actual Lumi portrait");
            yield return Snapshot("ART-Lumi-menu-"+label);
            foreach(var city in game.Content.cities)
            {
                yield return CrossEnter(city.id);yield return CloseEvent();
                Check(Map.Marker.GetComponent<Image>().sprite==canonical,"White Lumi map traveler in "+city.id);
                var nodes=city.id=="hue"?new[]{"U2"}:city.id=="jeonju"?new[]{"J2","J16"}:Array.Empty<string>();
                foreach(var node in nodes)
                {
                    Time.timeScale=5;yield return Travel(NodeIndex(node),true);Time.timeScale=1;
                    yield return CloseEvent();var index=NodeIndex(node);var actor=MapActor(node);var binding=art.Npc(art.MapActor(city.id,node).artId);
                    Check(actor!=null&&actor.sprite==binding.sprite&&actor.raycastTarget,"Map sprite and pointer target match identity: "+binding.id);
                    var hit=actor.transform.parent.Find("ActorHitArea").GetComponent<RectTransform>();
                    Check(hit.rect.width>=52&&hit.rect.height>=52,"Original touch region retained: "+binding.id);
                    Map.View.ZoomBy(3/Map.View.Zoom);Map.View.Focus((StandingFoot(actor)+CityRouteGeometry.Arrival(game.Data.nodes[index]))*.5f);
                    yield return Snapshot("ART-"+binding.id+"-map-"+label);
                    var saved=JsonUtility.ToJson(game.Save);yield return Click("Interact",true);yield return new WaitForSecondsRealtime(.3f);
                    var portrait=GameObject.Find("EventPortrait").GetComponent<Image>();
                    Check(portrait.sprite==binding.portrait,"Real NPC dialogue uses same new design: "+binding.id);
                    yield return Snapshot("ART-"+binding.id+"-dialogue-"+label);
                    yield return Click("DialogueCloseX",true);Check(JsonUtility.ToJson(game.Save)==saved,"Dialogue X preserves all quest/item/reward/save state: "+binding.id);
                }
                if(!before&&nodes.Length==0)yield return Snapshot("ART-Lumi-map-"+city.id);
            }
            }
            if(before)yield break;
            if(!tail)
            {
            yield return CrossEnter("hoi_an");yield return CloseEvent();
            // Presentation fixture calls the existing view, with no quest/route grants.
            var panels=(GameplayPanels)typeof(CityMap).GetField("panels",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(Map);
            var savedState=JsonUtility.ToJson(game.Save);panels.ShowPlayerGuide();yield return null;
            Check(GameObject.Find("GuideLumi").GetComponent<Image>().sprite==canonical,"How-to / journal guide uses approved white portrait");
            yield return Snapshot("ART-Lumi-guide-after");CloseAppearancePanel(panels);yield return null;
            var battleNode=Enumerable.Range(0,game.Data.nodes.Length).First(i=>game.Events.Definition(game.Data.nodes[i].eventId)?.type=="encounter");
            panels.ShowBattle(battleNode);yield return null;
            Check(GameObject.Find("BattleLumi").GetComponent<Image>().sprite==canonical,"Combat view uses white Lumi, not the old action pose");
            yield return Snapshot("ART-Lumi-combat-after");CloseAppearancePanel(panels);yield return null;
            Check(JsonUtility.ToJson(game.Save)==savedState,"Presentation-only guide/combat preview grants no item, progress or reward");
            }
            // Every leisure builds its real runtime meshes. Only three captures are retained.
            foreach(var entry in LeisurePresentation.Load().entries.Where(e=>e.enabled&&(!tail||e.cityId=="jeonju"||e.cityId=="gyeongju"||e.cityId=="seoul")))
            {
                yield return CrossEnter(entry.cityId);Time.timeScale=6;yield return ChapterGo(entry.nodeId);Time.timeScale=1;yield return CloseEvent();
                yield return LeisureFullOpen(entry);
                Check(FullPilot!=null||FullView!=null,"Existing leisure opens: "+entry.id);
                // Held tools are children of the wing, but are not Lumi artwork.
                var layers=FullRider.GetComponentsInChildren<LeisureSpriteLayer>().Where(l=>l.name=="CanonicalLumiBody"||l.name=="CanonicalRider"||l.name.StartsWith("Lumi")).ToArray();
                Check(layers.Length>0&&layers.All(l=>l.mainTexture==canonical.texture),"Every Lumi mesh uses approved white texture: "+entry.id);
                var saved=JsonUtility.ToJson(game.Save);yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.65f);
                Check(FullPlaying,"Existing leisure animation starts: "+entry.id);
                if(entry.mechanic=="tea"||entry.mechanic=="dance"||entry.mechanic=="swing")yield return Snapshot("ART-Lumi-leisure-"+entry.mechanic);
                yield return CloseEvent();Check(JsonUtility.ToJson(game.Save)==saved,"Closing leisure visual retains exact save: "+entry.id);
            }
            yield return CrossEnter("hoi_an");yield return CloseEvent();
            game.Scenes.Go("HoiAnDemo");yield return Scene("HoiAnDemo");
            var player=FindAnyObjectByType<HoiAn.HoiAnPlayerController>();var visual=player.transform.Find("CanonicalLumiVisual").GetComponent<SpriteRenderer>();
            Check(visual.sprite==canonical&&player.GetComponent<BoxCollider2D>()!=null,"H3 white Lumi uses original player collider");
            Check(Mathf.Abs(visual.bounds.size.y-.88f)<.01f,"H3 display height preserved, no stretched sprite");
            yield return Snapshot("ART-Lumi-H3-after");
            KeepSave("ART-earned-after");
        }
    }
}
#endif
