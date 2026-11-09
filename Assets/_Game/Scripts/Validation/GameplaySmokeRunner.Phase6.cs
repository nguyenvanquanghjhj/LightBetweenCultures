#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    // Opt-in diagnostics on the existing runner and production scenes. Never installed in release.
    public sealed partial class GameplaySmokeRunner
    {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct P6WindowRect {public int left,top,right,bottom;}
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr window,out P6WindowRect rect);
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool GetClientRect(IntPtr window,out P6WindowRect rect);
        [System.Runtime.InteropServices.DllImport("user32.dll")] static extern bool SetWindowPos(IntPtr window,IntPtr insert,int x,int y,int width,int height,uint flags);
#endif
        IEnumerator P6Resolution(int width,int height)
        {
            Screen.SetResolution(width,Mathf.Min(height,960),false);yield return new WaitForSecondsRealtime(.3f);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            // Resize only this opt-in QA Player, allowing its client to extend past
            // the desktop. Unity's Screen.SetResolution otherwise clamps tall windows.
            var handle=System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            if(handle!=IntPtr.Zero&&GetWindowRect(handle,out var outer)&&GetClientRect(handle,out var inner))
                SetWindowPos(handle,IntPtr.Zero,0,0,width+outer.right-outer.left-inner.right,height+outer.bottom-outer.top-inner.bottom,0x0014);
#endif
            yield return new WaitForSecondsRealtime(.6f);
            File.AppendAllText(Path.Combine(output,"resolution-diagnostics.txt"),"requested="+width+"x"+height+" actual="+Screen.width+"x"+Screen.height+"\n");
            Check(Screen.width==width&&Screen.height==height,"Native framebuffer resolution "+width+"x"+height);
        }
        GameplayPanels P6Panels=> (GameplayPanels)typeof(CityMap).GetField("panels",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(Map);
        void P6Show(string method,params object[] args)=>typeof(GameplayPanels).GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).Invoke(P6Panels,args);
        IEnumerator P6Shot(string name,int supersize=1)
        {
            yield return new WaitForSecondsRealtime(.5f);Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            var path=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(path,supersize);
            yield return new WaitForSecondsRealtime(.8f);
            Check(File.Exists(path),"Unity runtime render "+name+" native="+Screen.width+"x"+Screen.height+" supersize="+supersize);
            captures.Add(name+".png | native="+Screen.width+"x"+Screen.height+" | Unity supersize="+supersize);
        }
        void P6Bounds(string name)
        {
            Canvas.ForceUpdateCanvases();var rt=GameObject.Find(name).GetComponent<RectTransform>();var c=new Vector3[4];rt.GetWorldCorners(c);
            Check(c.All(p=>p.x>=-1&&p.y>=-1&&p.x<=Screen.width+1&&p.y<=Screen.height+1),"Screen bounds "+name+" "+Screen.width+"x"+Screen.height);
        }
        IEnumerator Phase6Journey()
        {
            musicVirtualFocus=true;var original=JsonUtility.ToJson(game.Save);var audio=SoftAudio.Ensure();audio.SetMuted(false);
            Phase4Invariants();Check(game.Content.cities.Sum(c=>c.edges.Length)==155&&game.Content.checkIns.Length==18,"Phase6 preserves155 edges/18 check-ins");
            if(Environment.GetCommandLineArgs().Contains("--phase6-fresh"))
            {
                yield return CrossEnter("hoi_an");var before=JsonUtility.ToJson(game.Save);
                foreach(var i in Enumerable.Range(0,game.Data.nodes.Length).Where(i=>game.Discovery.DiscoveryState(i)==MapDiscoveryState.HIDDEN))
                {Map.TryMove(i);Check(!Map.Moving&&FindButton(game.Data.nodes[i].id)==null&&game.Data.FindPath(game.Save.node,i,game.Save)==null,"HIDDEN rejects input/path "+game.Data.nodes[i].id);}
                Check(JsonUtility.ToJson(game.Save)==before,"Hidden input does not change fresh save");yield return TestMapInput();
                Time.timeScale=4;yield return Travel(2,true);Time.timeScale=1;yield return new WaitForSecondsRealtime(.4f);
                var definition=game.Events.Definition(game.Data.nodes[2].eventId);var bound=AdventureUIArt.Load().Npc(definition.npcId);
                var expected=bound?.portrait??bound?.sprite??DesignKitArt.Load().maiPortrait;
                Check(Map.DialogueOpen&&GameObject.Find("EventPortrait").GetComponent<Image>().sprite==expected,"Fresh touch reaches H2 canonical Mai dialogue (existing legacy/variant binding)");
                yield return P6Shot("Fresh-H2-touch");yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--phase6-restore"))
            {
                phase4CityOverride="jeonju";yield return CrossEnter("jeonju");Time.timeScale=4;
                yield return ChapterGo(Phase4Relic.restorationNode);yield return CloseEvent();Time.timeScale=1;
                var coins=game.Save.coins;var ledger=game.Save.rewardGrantIds.ToArray();
                yield return RelicOption("relic_restore:"+Phase4Relic.id);yield return Phase4SolveTrial();yield return RelicExit();
                Check(game.Save.coins==coins&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Valid restoration does not regrant collection/story rewards");
                KeepSave("P6-earned-restoration");yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--phase6-ending"))
            {
                yield return CrossEnter("seoul");yield return CloseEvent();
                Check(game.Events.FinalConnectionReady&&!game.Events.CanonicalEndingCompleted,"Earned ready fixture is uncommitted");
                yield return Click("Interact",true);yield return ChapterChoice("ceremony");yield return RelicExit();
                Check(!game.Events.CeremonyInProgress&&!game.Events.CanonicalEndingCompleted,"Cancel confirmation remains noncommitting");
                yield return Phase5Finish(false);yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--phase6-battle"))
            {
                foreach(var city in new[]{"hoi_an","jeonju"})
                foreach(var outcome in new[]{"run","loss","win"})
                {
                    yield return CrossEnter(city);yield return CloseEvent();var earned=JsonUtility.ToJson(game.Save);
                    var entry=game.Content.events.First(e=>e.cityId==city&&e.type=="encounter"&&game.Content.Enemy(e.enemyId)!=null&&game.Content.Enemy(e.enemyId).mechanic!="two_phase"&&game.Events.CanUse(e));
                    game.Save.node=NodeIndex(entry.nodeId);game.Save.resolvedEvents.Remove(EncounterManager.Key(entry));game.Save.flags.Remove(entry.completionFlag);game.Save.playerHP=outcome=="loss"?1:100;game.Save.lightEnergy=3;
                    Check(game.Battles.Start(entry),"Private encounter fixture starts "+city+" "+outcome);P6Panels.ShowBattle(game.Save.node);
                    yield return new WaitForSecondsRealtime(1.5f);Check(audio.MusicTrack==(city=="hoi_an"?1:3),"Regional battle BGM "+city);
                    P6Bounds("BattleRun");if(outcome=="run")yield return P6Shot("Battle-"+city);
                    if(outcome=="run")yield return Click("BattleRun",true);
                    else if(outcome=="loss")yield return Click("BattleAttack",true);
                    else for(var turn=0;game.Battles.Active&&turn<80;turn++)yield return Click(game.Battles.Energy>0?"BattleLight":"BattleAttack",turn%2==0);
                    Check(!game.Battles.Active&&game.Battles.Outcome==outcome,"Real battle result "+city+" "+outcome);yield return Click("BattleClose",true);yield return new WaitForSecondsRealtime(1.5f);
                    Check(audio.MusicTrack==(city=="hoi_an"?0:2)&&audio.CurrentMusic.isPlaying,"Map BGM resumes "+city+" "+outcome);
                    File.WriteAllText(savePath,earned);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                }
                yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--phase6-restart"))
            {
                yield return CrossEnter(game.Save.cityId);yield return CloseEvent();
                Check(game.Relics.AllRestored&&game.Events.CanonicalEndingCompleted,"Restart retains earned6 relics and canonical completion");
                var before=JsonUtility.ToJson(game.Save);yield return Click("Inventory",true);yield return Click("CloseInventory",true);
                Check(JsonUtility.ToJson(game.Save)==before,"Reload/UI gives no reward or progress mutation");yield break;
            }
            foreach(var city in new[]{"hoi_an","jeonju","seoul"})
            {
                yield return CrossEnter(city);yield return CloseEvent();Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);
                yield return P6Shot("Map-"+city);
            }
            yield return Click("Back",true);yield return Scene("WorldMap");yield return P6Shot("WorldMap");
            var art=AdventureUIArt.Load();File.WriteAllLines(Path.Combine(output,"native-sprite-diagnostics.txt"),art.fragments.Concat(art.cities).Where(e=>e.sprite!=null).Select(e=>e.id+" | "+e.sprite.name+" | rect="+e.sprite.rect+" | bounds="+e.sprite.bounds+" | verts="+string.Join(";",e.sprite.vertices.Select(v=>v.ToString()))+" | UV="+string.Join(";",e.sprite.uv.Select(v=>v.ToString()))));
            foreach(var city in new[]{"jeonju","seoul"})
            {
                yield return CrossEnter(city);yield return CloseEvent();
                var r=game.Relics.Catalog.relics.First(v=>v.cityId==city);var earned=JsonUtility.ToJson(game.Save);
                game.Save.node=NodeIndex(r.restorationNode);game.Persist();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                // Presentation fixture only: owned fragments earned in input save; reopen assembly without altering sources/rewards.
                game.Save.flags.Remove("relic.assembled."+r.id);game.Save.flags.Remove("relic.restored."+r.id);
                P6Show("ShowRelicAssembly",r.id);yield return P6Shot("Assembly-"+city);
                for(var i=0;i<4;i++)
                {
                    var target=GameObject.Find("RelicTarget"+i).GetComponent<RectTransform>();var parent=(RectTransform)target.parent;var corners=new Vector3[4];target.GetWorldCorners(corners);
                    var outside=corners.Any(p=>!RectTransformUtility.RectangleContainsScreenPoint(parent,RectTransformUtility.WorldToScreenPoint(null,p),null));
                    File.AppendAllText(Path.Combine(output,"layout-diagnostics.txt"),city+" target"+i+" outsideBoard="+outside+"\n");
                    if(!Environment.GetCommandLineArgs().Contains("--phase6-before"))Check(!outside,"Assembly target contained in board "+city+" "+i);
                }
                yield return RelicDrag(0,true,false);Check(!game.Relics.Active.Placed(0),"Wrong touch drop rejected "+city);
                for(var i=0;i<4;i++){yield return RelicDrag(i,i%2==0);Check(game.Relics.Active.Placed(i),"Real mouse/touch drop "+city+" piece"+i);}
                yield return RelicExit();File.WriteAllText(savePath,earned);game.Reload();game.Scenes.Go("CityMap");yield return Scene("CityMap");
                P6Show("ShowRelicDetails",r.id);yield return P6Shot("Fragments-"+city);yield return RelicExit();
            }
            if(Environment.GetCommandLineArgs().Contains("--phase6-before"))yield break;
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
            {
                yield return P6Resolution(size.x,size.y);
                yield return CrossEnter("hoi_an");Time.timeScale=6;yield return ChapterGo("H2");yield return CloseEvent();Time.timeScale=1;
                Map.View.ZoomBy(2/Map.View.Zoom);Map.View.Focus(game.Data.nodes[2].position);
                var label=size.x+"x"+size.y;yield return P6Shot(label+"-map");
                yield return Click("Interact",true);yield return new WaitForSecondsRealtime(.4f);P6Bounds("EventClose");P6Bounds("DialogueTextViewport");
                var scroll=GameObject.Find("DialogueTextViewport").GetComponent<ScrollRect>();scroll.verticalNormalizedPosition=0;yield return P6Shot(label+"-dialogue");yield return CloseEvent();
                yield return Click("Inventory",true);P6Bounds("CloseInventory");yield return P6Shot(label+"-inventory");yield return Click("CloseInventory",true);
            }
            Screen.SetResolution(540,960,false);yield return new WaitForSecondsRealtime(.5f);
            yield return TestMapInput();
            var ui=FindAnyObjectByType<UIController>();ui.Root.anchorMin=new Vector2(.04f,.04f);ui.Root.anchorMax=new Vector2(.96f,.95f);
            P6Show("ShowEvent",2,"",null,0);yield return new WaitForSecondsRealtime(.5f);P6Bounds("EventClose");yield return P6Shot("Simulated-safe-area-dialogue");yield return CloseEvent();
            ui.Root.anchorMin=Vector2.zero;ui.Root.anchorMax=Vector2.one;
            yield return Click("QuestJournal",true);yield return Click("OpenRelicCollection",true);P6Bounds("RelicExit");yield return P6Shot("Collection");yield return RelicExit();
            var checkin=game.Content.checkIns.First(e=>e.id=="hoi_an.h2");P6Show("ShowCheckIn",NodeIndex(checkin.nodeId),false,false);yield return P6Shot("CheckIn");yield return Click("CheckInClose",true);
            yield return OpenPhotoJournal();P6Bounds("PhotoExit");yield return P6Shot("PhotoJournal");yield return Click("PhotoExit",true);
            Time.timeScale=6;yield return ChapterGo("H6");yield return CloseEvent();Time.timeScale=1;
            var h6save=JsonUtility.ToJson(game.Save);P6Show("ShowRelicPhoto");
            yield return RelicDrag(0,true);yield return RelicDrag(1,false);
            Check(game.Relics.Active.Placed(0)&&game.Relics.Active.Placed(1),"H6 accepted two-symbol alignment retains real touch/mouse drop targets");
            yield return RelicExit();Check(JsonUtility.ToJson(game.Save)==h6save,"H6 drag/cancel preserves postcard/proof/reward state");
            foreach(var city in new[]{"hoi_an","gyeongju","seoul"})
            {
                yield return CrossEnter(city);yield return CloseEvent();
                foreach(var actor in AdventureUIArt.Load().mapActors.Where(a=>a.cityId==city&&a.hasMapPosition))
                {
                    Time.timeScale=6;yield return ChapterGo(actor.nodeId);yield return CloseEvent();Time.timeScale=1;
                    Map.View.Focus(actor.mapPosition);yield return new WaitForSecondsRealtime(.35f);CheckNpcActor(NodeIndex(actor.nodeId));
                    if(actor.nodeId=="G14"||actor.nodeId=="S2")
                    {yield return ClickNpcSprite(actor.nodeId);var opts=game.Events.BuildOptions(game.Save.node).options;Check(opts.Any(o=>o.id=="talk"||o.id=="progress"||o.id.StartsWith("progress:"))&&opts.Any(o=>o.id=="checkin"||o.id=="checkin_view"),"NPC/check-in choices coexist "+actor.nodeId);yield return CloseEvent();}
                }
            }
            var total=FindObjectsByType<UnityEngine.Object>(FindObjectsSortMode.None).Length;var memory=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong();
            var started=Time.realtimeSinceStartup;for(var i=0;i<120;i++){Map.View.PanBy(new Vector2(i%2==0?2:-2,0));yield return null;}
            File.WriteAllText(Path.Combine(output,"performance-sample.txt"),"Development Player;120 pan frames elapsed="+(Time.realtimeSinceStartup-started)+"s;allocatedNative="+memory+" -> "+UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong()+"; objects="+total+" -> "+FindObjectsByType<UnityEngine.Object>(FindObjectsSortMode.None).Length+"; not Android profiling");
            var count=audio.PlayedCount;game.Reload();yield return new WaitForSecondsRealtime(.3f);Check(audio.PlayedCount==count,"Save load does not replay pickup/puzzle SFX");
            yield return Click("AudioSettings",true);yield return Click("AudioMute",true);Check(audio.Muted,"Existing mute works via simulated touch");yield return Click("AudioMute",true);yield return Click("AudioSettingsClose",true);
            Check(FindObjectsByType<SoftAudio>(FindObjectsSortMode.None).Length==1&&FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"No duplicate audio/EventSystem after repeated scene/panel transitions");
            File.WriteAllText(savePath,original);game.Reload();KeepSave("P6-restored-input");
        }
    }
}
#endif
