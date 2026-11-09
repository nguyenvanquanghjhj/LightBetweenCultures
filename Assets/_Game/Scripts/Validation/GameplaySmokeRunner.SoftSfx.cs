#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator SoftSfxJourney()
        {
            var audio=SoftAudio.Ensure();
            Check(FindObjectsByType<SoftAudio>(FindObjectsSortMode.None).Length==1,"Exactly one persistent audio service");
            if(Environment.GetCommandLineArgs().Contains("--soft-sfx-input"))
            {
                audio.SetMuted(false);yield return CrossEnter("hoi_an");yield return new WaitForSecondsRealtime(.3f);
                var count=audio.PlayedCount;yield return TestMapInput();
                Check(audio.PlayedCount==count+1,"Wheel, pinch and map pan are silent; only the final Overview button emits one cue");
                Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled)==1,"Exactly one active listener after UI scene transitions");
                yield return Snapshot("SFX-map-input");yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--soft-sfx-restart"))
            {
                Check(audio.Muted&&Mathf.Abs(audio.SfxVolume-.43f)<.01f&&Mathf.Abs(audio.MusicVolume-.21f)<.01f,"Mute and separate bus gains survive a real process restart");
                var count=audio.PlayedCount;yield return CrossEnter("hoi_an");yield return new WaitForSecondsRealtime(.3f);
                Check(audio.PlayedCount==count,"Muted restart and save load never auto-play pickup/success/click");
                audio.SetMuted(false);audio.SavePreferences();yield return new WaitForSecondsRealtime(.12f);SoftAudio.Cue(SoftCue.UI_Click);yield return null;
                Check(audio.PlayedCount==count+1,"Only a new accepted cue plays after unmute");yield break;
            }
            audio.SetMuted(false);audio.SetVolumes(.65f,.35f);
            var probe=audio.GetComponentsInChildren<AudioSource>()[0].gameObject.AddComponent<SoftSfxSignalProbe>();
            foreach(SoftCue cue in Enum.GetValues(typeof(SoftCue)))
            {
                yield return new WaitForSecondsRealtime(.9f);var count=audio.PlayedCount;probe.peak=0;probe.blocks=0;
                var samples=new float[256];foreach(var voice in audio.GetComponentsInChildren<AudioSource>())voice.GetOutputData(samples,0);
                SoftAudio.Cue(cue);yield return null;
                Check(audio.PlayedCount==count+1&&audio.LastCue==cue&&audio.PlayingVoices>0,"Native Windows AudioSource plays "+cue);
                float peak=0;var until=Time.realtimeSinceStartup+1;
                while(Time.realtimeSinceStartup<until)
                {foreach(var voice in audio.GetComponentsInChildren<AudioSource>()){voice.GetOutputData(samples,0);foreach(var v in samples)peak=Mathf.Max(peak,Mathf.Abs(v));}peak=Mathf.Max(peak,probe.peak);yield return null;}
                Debug.Log("SFX OUTPUT "+cue+" peak="+peak+" blocks="+probe.blocks+" listeners="+FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(l=>l.enabled)+" listenerVolume="+AudioListener.volume+" paused="+AudioListener.pause+" dsp="+AudioSettings.dspTime);
                Check(peak>0&&peak<.5f,"Rendered nonzero, bounded output samples for "+cue);
            }
            Destroy(probe);
            yield return new WaitForSecondsRealtime(1);var prior=audio.PlayedCount;
            SoftAudio.Action(()=>{SoftAudio.Cue(SoftCue.UI_Click);SoftAudio.Cue(SoftCue.UI_Invalid);SoftAudio.Cue(SoftCue.UI_Click);});
            Check(audio.PlayedCount==prior+1&&audio.LastCue==SoftCue.UI_Invalid,"One action emits exactly one highest-priority cue");
            for(var i=0;i<40;i++)SoftAudio.Cue(SoftCue.UI_Click);
            Check(audio.PlayedCount==prior+1&&audio.PlayingVoices<=3,"Same-frame spam capped without overlapping forty voices");
            audio.SetSuspended(true);yield return new WaitForSecondsRealtime(.15f);SoftAudio.Cue(SoftCue.UI_Click);Check(audio.PlayedCount==prior+1,"Suspended application rejects SFX");audio.SetSuspended(false);
            yield return CrossEnter("hoi_an");
            yield return Click("AudioSettings",true);var slider=GameObject.Find("SfxVolume").GetComponent<Slider>();var track=slider.GetComponent<RectTransform>();
            var start=RectTransformUtility.WorldToScreenPoint(null,track.TransformPoint(track.rect.min+new Vector2(track.rect.width*.2f,track.rect.height*.5f)));
            var end=RectTransformUtility.WorldToScreenPoint(null,track.TransformPoint(track.rect.min+new Vector2(track.rect.width*.8f,track.rect.height*.5f)));
            yield return new WaitForSecondsRealtime(.15f);prior=audio.PlayedCount;
            InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));yield return null;
            for(var i=0;i<12;i++){InputSystem.QueueStateEvent(mouse,new MouseState{position=Vector2.Lerp(start,end,i/11f)}.WithButton(MouseButton.Left));yield return null;}
            InputSystem.QueueStateEvent(mouse,new MouseState{position=end});yield return null;yield return null;
            Check(audio.PlayedCount==prior&&audio.SfxVolume>.7f,"Dragging volume changes gain without per-frame click sounds");
            slider.value=.43f;GameObject.Find("MusicVolume").GetComponent<Slider>().value=.21f;
            var sentinel=new GameObject("QARegisteredMusicBus").AddComponent<AudioSource>();sentinel.playOnAwake=false;audio.RegisterMusic(sentinel);
            Check(Mathf.Abs(sentinel.volume-.21f)<.001f&&audio.GetComponentsInChildren<AudioSource>().All(v=>Mathf.Abs(v.volume-.43f*.7f)<.001f),"Independent Music gain bus and SFX source gains");
            yield return Click("AudioMute",true);Check(audio.Muted&&sentinel.mute&&audio.PlayingVoices==0,"Mute silences registered buses and clears SFX tails");
            yield return Snapshot("SFX-settings-muted");yield return Click("AudioMute",true);yield return Click("AudioSettingsClose",true);Destroy(sentinel.gameObject);
            yield return new WaitForSecondsRealtime(.2f);var snapshot=audio.PlayedCount;Map.TryMove(NodeIndex("H19"));yield return null;Check(audio.PlayedCount==snapshot,"Hidden node refusal produces no accepted click");
            yield return ClaimRelic("fragment_hoi_an_bamboo_frame",true);
            Check(audio.CueCounts[(int)SoftCue.Relic_Place]>=2&&audio.CueCounts[(int)SoftCue.UI_Invalid]>=3,"Real relic wrong/right drag and rejected confirmations route to place/invalid cues");
            yield return Travel(2,true);yield return Click("EventOption_accept",true);yield return Click("DialogueNext");yield return CloseEvent();
            var picked=audio.CueCounts[(int)SoftCue.Item_Pickup];yield return Travel(3,true);yield return ExploreH3();yield return new WaitForSecondsRealtime(.2f);
            Check(audio.CueCounts[(int)SoftCue.Item_Pickup]>picked,"H3 real pickup emits item feedback without changing quest service");
            yield return Travel(2,true);yield return Click("EventOption_tutorial_return",true);yield return CloseEvent();
            yield return Click("Inventory",true);yield return Click("CloseInventory",true);
            yield return Click("QuestJournal",true);yield return Click("OpenRelicCollection",true);yield return RelicExit();
            yield return Travel(4,true);var successes=audio.CueCounts[(int)SoftCue.Puzzle_Success];yield return SolvePuzzle("puzzle_market");yield return new WaitForSecondsRealtime(.2f);
            Check(audio.CueCounts[(int)SoftCue.Puzzle_Success]==successes+1,"Actual puzzle wrong/right submissions emit success exactly once despite item reward");
            yield return CloseEvent();yield return new WaitForSecondsRealtime(.4f);
            var pickupCount=audio.CueCounts[(int)SoftCue.Item_Pickup];successes=audio.CueCounts[(int)SoftCue.Puzzle_Success];game.Reload();yield return new WaitForSecondsRealtime(.3f);
            Check(audio.CueCounts[(int)SoftCue.Item_Pickup]==pickupCount&&audio.CueCounts[(int)SoftCue.Puzzle_Success]==successes,"Reload derives baseline without replaying pickup/puzzle sound");
            yield return CrossEnter("hoi_an");Check(FindObjectsByType<SoftAudio>(FindObjectsSortMode.None).Length==1,"World Map and City Map re-entry never duplicate audio service");
            audio.SetVolumes(.43f,.21f);audio.SetMuted(true);audio.SavePreferences();KeepSave("soft-sfx-final");
            File.WriteAllLines(Path.Combine(output,"sfx-counters.txt"),Enum.GetValues(typeof(SoftCue)).Cast<SoftCue>().Select(c=>c+"="+audio.CueCounts[(int)c]));
        }
    }
    public sealed class SoftSfxSignalProbe:MonoBehaviour
    {
        public volatile float peak;
        public volatile int blocks;
        void OnAudioFilterRead(float[] data,int channels)
        {blocks++;foreach(var v in data){var magnitude=v<0?-v:v;if(magnitude>peak)peak=magnitude;}}
    }
}
#endif
