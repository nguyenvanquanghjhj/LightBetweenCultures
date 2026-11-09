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
        bool musicVirtualFocus;
        void Update()
        {
            // Input tests already use virtual devices. Keep lifecycle focus deterministic too,
            // independently of another desktop window stealing focus during unattended QA.
            if(musicVirtualFocus&&SoftAudio.Instance!=null)SoftAudio.Instance.SendMessage("OnApplicationFocus",true);
        }
        string MusicDiagnostic(SoftAudio a)=>" track="+a.MusicTrack+" playing="+a.CurrentMusic.isPlaying+" time="+a.CurrentMusic.time+" suspended="+a.Suspended+" muted="+a.Muted;
        IEnumerator MusicBoundarySamples()
        {
            var samples=new float[512];float peak=0,quiet=0,maxQuiet=0;
            var end=Time.realtimeSinceStartup+2.6f;
            while(Time.realtimeSinceStartup<end)
            {
                AudioListener.GetOutputData(samples,0);float sum=0;
                foreach(var v in samples){sum+=v*v;peak=Mathf.Max(peak,Mathf.Abs(v));}
                if(Mathf.Sqrt(sum/samples.Length)<.0001f)quiet+=Time.unscaledDeltaTime;else quiet=0;
                maxQuiet=Mathf.Max(maxQuiet,quiet);yield return null;
            }
            Debug.Log("MUSIC DSP BOUNDARY peak="+peak+" maxQuietSeconds="+maxQuiet);
            Check(peak>0&&peak<.8f&&maxQuiet<.4f,"Native loop output is nonzero, bounded and has no measured silence over 400ms");
        }
        IEnumerator FourTrackMusicJourney()
        {
            var audio=SoftAudio.Ensure();musicVirtualFocus=true;
            if(Environment.GetCommandLineArgs().Contains("--music-restart"))
            {
                Check(audio.Muted&&Mathf.Abs(audio.MusicVolume-.23f)<.001f,"Music mute/volume restored in a new Windows process");
                yield return CrossEnter("hoi_an");yield return new WaitForSecondsRealtime(1.5f);
                Check(audio.MusicStarts==0&&!audio.CurrentMusic.isPlaying,"Muted restart never starts music");
                audio.SetMuted(false);yield return new WaitForSecondsRealtime(1.5f);
                Check(audio.MusicTrack==0&&audio.CurrentMusic.isPlaying,"Explicit unmute starts the correct map track");yield break;
            }
            audio.SendMessage("OnApplicationFocus",true);audio.SetSuspended(false);audio.SetMuted(false);audio.SetVolumes(.65f,.35f);
            if(!Environment.GetCommandLineArgs().Contains("--music-battles"))
            {
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                var before=audio.MusicStarts;var previous=audio.MusicTrack;
                yield return CrossEnter(city);audio.SendMessage("OnApplicationFocus",true);yield return new WaitForSecondsRealtime(1.5f);
                var expected=city=="hoi_an"||city=="hue"||city=="da_lat"?0:2;
                Check(audio.MusicTrack==expected&&audio.CurrentMusic.isPlaying,"Correct native map BGM: "+city+" track="+audio.MusicTrack+" time="+audio.CurrentMusic.time+" muted="+audio.Muted+" suspended="+audio.Suspended);
                Check(previous!=expected||audio.MusicStarts==before,"Same region preserves playback position: "+city);
                Check(audio.GetComponentsInChildren<AudioSource>().Count(s=>s.name.StartsWith("MusicVoice")&&s.isPlaying)==1,"One settled BGM voice: "+city);
            }
            yield return Click("Inventory",true);var starts=audio.MusicStarts;
            yield return new WaitForSecondsRealtime(.4f);yield return Click("CloseInventory",true);
            Check(audio.MusicStarts==starts&&audio.CurrentMusic.isPlaying,"Inventory and accepted UI SFX never restart BGM");
            yield return Click("QuestJournal",true);yield return Click("OpenRelicCollection",true);yield return RelicExit();
            Check(audio.MusicStarts==starts&&audio.CurrentMusic.isPlaying,"Journal/collection overlays preserve map BGM");
            var presentations=new GameplayPanels(FindAnyObjectByType<UIController>(),game,DesignKitArt.Load(),()=>{},()=>{});
            var npc=game.Content.events.First(e=>e.cityId==game.Data.cityId&&e.type=="npc");
            presentations.ShowEvent(NodeIndex(npc.nodeId));yield return new WaitForSecondsRealtime(.4f);yield return Click("EventClose",true);
            Check(audio.MusicStarts==starts&&audio.CurrentMusic.isPlaying,"Existing NPC dialogue presentation preserves map BGM");
            var puzzle=game.Content.events.First(e=>e.cityId==game.Data.cityId&&e.type=="puzzle");
            presentations.ShowPuzzle(NodeIndex(puzzle.nodeId));yield return new WaitForSecondsRealtime(.4f);yield return Click("PuzzleClose",true);
            Check(audio.MusicStarts==starts&&audio.CurrentMusic.isPlaying,"Existing ordinary puzzle panel preserves map BGM");
            yield return Click("AudioSettings",true);
            GameObject.Find("MusicVolume").GetComponent<Slider>().value=.23f;
            Check(Mathf.Abs(audio.MusicVolume-.23f)<.001f&&Mathf.Abs(audio.SfxVolume-.65f)<.001f,"Music slider is independent of SFX");
            yield return Click("AudioSettingsClose",true);
            var sample=audio.CurrentMusic;var time=sample.time;audio.SetSuspended(true);
            yield return new WaitForSecondsRealtime(.4f);
            Check(!sample.isPlaying&&Mathf.Abs(sample.time-time)<.12f,"Application suspension pauses streaming playback");
            audio.SetSuspended(false);yield return new WaitForSecondsRealtime(.3f);
            Check(sample.isPlaying&&sample.time>time,"Resume continues position without restarting");
            musicVirtualFocus=false;audio.SendMessage("OnApplicationFocus",false);yield return new WaitForSecondsRealtime(.3f);
            Check(audio.Suspended&&!audio.CurrentMusic.isPlaying,"Focus-loss callback pauses BGM");
            audio.SendMessage("OnApplicationFocus",true);musicVirtualFocus=true;yield return new WaitForSecondsRealtime(.3f);
            Check(!audio.Suspended&&audio.CurrentMusic.isPlaying,"Focus-gain callback resumes BGM");
            }
            // Fast-seek is a QA-only acceleration of the real loop boundary, not an authored clip edit.
            var catalog=Resources.Load<SoftMusicClips>("SoftMusicClips");
            foreach(var city in new[]{"hoi_an","jeonju"})
            {
                yield return CrossEnter(city);audio.SendMessage("OnApplicationFocus",true);yield return new WaitForSecondsRealtime(1.5f);
                foreach(var outcome in new[]{"run","loss","win"})
                {
                    // Bounded combat fixture: reopen a catalog encounter in this PRIVATE QA save only.
                    // Exercises real battle service and UI; not proof of story/route progression.
                    game.Reload();
                    var entry=game.Content.events.First(e=>e.cityId==city&&e.type=="encounter"&&game.Content.Enemy(e.enemyId)!=null&&game.Content.Enemy(e.enemyId).mechanic!="two_phase"&&game.Events.CanUse(e));
                    game.Save.node=NodeIndex(entry.nodeId);game.Save.resolvedEvents.Remove(EncounterManager.Key(entry));game.Save.flags.Remove(entry.completionFlag);
                    game.Save.playerHP=outcome=="loss"?1:100;game.Save.lightEnergy=3;
                    Check(game.Battles.Start(entry),"Real catalog battle starts in isolated fixture: "+city+" "+outcome);
                    var ui=FindAnyObjectByType<UIController>();
                    var panels=new GameplayPanels(ui,game,DesignKitArt.Load(),()=>{},()=>{});panels.ShowBattle(game.Save.node);
                    yield return new WaitForSecondsRealtime(1.5f);
                    var track=city=="hoi_an"?1:3;
                    Check(audio.MusicTrack==track&&audio.CurrentMusic.isPlaying,"Actual battle panel selects regional combat track: "+city+" "+outcome);
                    var loops=audio.MusicLoops;audio.CurrentMusic.time=catalog.loopEnd[track]-3f;
                    yield return new WaitForSecondsRealtime(1f);
                    yield return MusicBoundarySamples();
                    Check(audio.MusicLoops==loops+1&&audio.CurrentMusic.isPlaying,"Combat track loops through real crossfade: "+track);
                    if(outcome=="run")yield return Click("BattleRun",true);
                    else if(outcome=="loss")yield return Click("BattleAttack",true);
                    else for(var turn=0;game.Battles.Active&&turn<80;turn++)yield return Click(game.Battles.Energy>0?"BattleLight":"BattleAttack",turn%2==0);
                    Check(!game.Battles.Active&&game.Battles.Outcome==outcome,"Real battle outcome: "+city+" "+outcome);
                    yield return new WaitForSecondsRealtime(1.5f);
                    Check(audio.MusicTrack==track-1&&audio.CurrentMusic.isPlaying,"Map music restored at "+outcome+" before result close"+MusicDiagnostic(audio));
                    yield return Snapshot("BGM-"+city+"-"+outcome);yield return Click("BattleClose",true);
                }
                var loopCount=audio.MusicLoops;var mapTrack=audio.MusicTrack;
                audio.CurrentMusic.time=catalog.loopEnd[mapTrack]-3f;
                yield return new WaitForSecondsRealtime(1f);
                yield return MusicBoundarySamples();
                Check(audio.MusicLoops==loopCount+1,"Map track loops through real crossfade: "+mapTrack);
            }
            Check(FindObjectsByType<SoftAudio>(FindObjectsSortMode.None).Length==1&&audio.GetComponentsInChildren<AudioSource>().Count(s=>s.name.StartsWith("MusicVoice"))==2,"One persistent audio controller, exactly two crossfade voices");
            audio.SetMuted(true);audio.SavePreferences();yield return new WaitForSecondsRealtime(.4f);
            Check(!audio.GetComponentsInChildren<AudioSource>().Any(s=>s.name.StartsWith("MusicVoice")&&s.isPlaying),"Mute pauses both music voices");
            KeepSave("music-final");
        }
    }
}
#endif
