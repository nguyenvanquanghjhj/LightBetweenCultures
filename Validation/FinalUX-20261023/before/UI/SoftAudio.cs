using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LightBetweenCultures
{
    // Presentation only: observes committed deltas; never grants items/flags/rewards.
    // Music registration is a separate gain bus, not a second BGM scene controller.
    public sealed partial class SoftAudio : MonoBehaviour
    {
        public static SoftAudio Instance {get;private set;}
        public static string PreferencePrefix {get;private set;}
        public float SfxVolume {get;private set;}
        public float MusicVolume {get;private set;}
        public bool Muted {get;private set;}
        public bool Suspended=>!focused||paused;
        public int PlayedCount {get;private set;}
        public SoftCue LastCue {get;private set;}
        public int[] CueCounts {get;}=new int[7];
        public int PlayingVoices=>voices.Count(v=>v.isPlaying);
        readonly List<AudioSource> voices=new List<AudioSource>();
        readonly List<AudioSource> music=new List<AudioSource>();
        SoftAudioClips catalog;
        AudioListener fallbackListener;
        bool focused=true,paused;
        int depth,lastFrame=-1;
        float lastAt=-100;
        float nextObserve;
        SoftCue? pending;
        SaveData observed;
        Dictionary<string,int> items=new Dictionary<string,int>();
        int coins,solved,restored,accepted,collected;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics(){Instance=null;PreferencePrefix=null;}
        public static SoftAudio Ensure()
        {
            if(Instance!=null)return Instance;
            var existing=FindAnyObjectByType<SoftAudio>();
            return existing!=null?existing:new GameObject("SoftAudio").AddComponent<SoftAudio>();
        }
        void Awake()
        {
            if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);
            var arg=Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--audio-preferences="));
            PreferencePrefix=arg==null?"lbc.audio.v1.":"lbc.audio.qa."+arg.Substring("--audio-preferences=".Length)+".";
            SfxVolume=Mathf.Clamp01(PlayerPrefs.GetFloat(PreferencePrefix+"sfx",.65f));
            MusicVolume=Mathf.Clamp01(PlayerPrefs.GetFloat(PreferencePrefix+"music",.35f));
            Muted=PlayerPrefs.GetInt(PreferencePrefix+"mute",0)==1;
            catalog=Resources.Load<SoftAudioClips>("SoftAudioClips");
            RefreshListener();SceneManager.sceneLoaded+=OnSceneLoaded;
            for(var i=0;i<3;i++)
            {var voice=new GameObject("SfxVoice"+i);voice.transform.SetParent(transform,false);var s=voice.AddComponent<AudioSource>();s.playOnAwake=false;s.loop=false;s.spatialBlend=0;s.priority=180;s.volume=SfxVolume;voices.Add(s);}
            InitMusic();Observe(false);
        }
        public static void Cue(SoftCue cue)
        {
            var a=Ensure();
            if(a.depth>0){if(!a.pending.HasValue||(int)cue>(int)a.pending.Value)a.pending=cue;}
            else a.Play(cue);
        }
        public static void Action(Action action,SoftCue? fallback=SoftCue.UI_Click)
        {
            var a=Ensure();a.depth++;a.Observe(true);
            try{action?.Invoke();if(fallback.HasValue)Cue(fallback.Value);}
            finally{a.Observe(true);a.depth--;if(a.depth==0&&a.pending.HasValue){var cue=a.pending.Value;a.pending=null;a.Play(cue);}}
        }
        public static SoftCue ButtonCue(string name)
        {
            var n=name.ToLowerInvariant();
            return n.Contains("close")||n.Contains("back")||n.Contains("exit")||n.Contains("cancel")?SoftCue.UI_Back:SoftCue.UI_Click;
        }
        public static bool IsNodeName(string name)=>name.Length>1&&"HUDJGS".Contains(name.Substring(0,1))&&int.TryParse(name.Substring(1),out _);
        void Play(SoftCue cue)
        {
            if(Muted||Suspended||SfxVolume<=0||lastFrame==Time.frameCount||Time.unscaledTime-lastAt<.065f)return;
            var clip=catalog!=null?catalog.Clip(cue):null;if(clip==null)return;
            var source=voices.FirstOrDefault(v=>!v.isPlaying);if(source==null)return;
            source.clip=clip;source.volume=SfxVolume*.7f;source.Play();
            lastAt=Time.unscaledTime;lastFrame=Time.frameCount;PlayedCount++;LastCue=cue;CueCounts[(int)cue]++;
        }
        void LateUpdate(){TickMusic();if(Time.unscaledTime>=nextObserve){nextObserve=Time.unscaledTime+.1f;Observe(true);}}
        void Observe(bool audible)
        {
            var save=GameBootstrap.Instance?.Save;if(save==null)return;
            var current=save.items.GroupBy(i=>i).ToDictionary(g=>g.Key,g=>g.Count());
            var nextRestored=save.flags.Count(f=>f.StartsWith("relic.restored."));
            var nextCollected=save.flags.Count(f=>f.StartsWith("relic.fragment.")||f.StartsWith("relic.assembled."));
            if(ReferenceEquals(observed,save)&&audible)
            {
                if(save.solvedPuzzles.Count>solved||nextRestored>restored)Cue(SoftCue.Puzzle_Success);
                else if(save.coins>coins||current.Any(i=>!items.TryGetValue(i.Key,out var count)||i.Value>count))Cue(SoftCue.Item_Pickup);
                else if(save.accepted.Count>accepted||nextCollected>collected)Cue(SoftCue.UI_Confirm);
            }
            observed=save;items=current;coins=save.coins;solved=save.solvedPuzzles.Count;restored=nextRestored;accepted=save.accepted.Count;collected=nextCollected;
        }
        public void RegisterMusic(AudioSource source)
        {if(source!=null&&!music.Contains(source)){music.Add(source);ApplyGains();}}
        public void SetVolumes(float sfx,float bgm)
        {SfxVolume=Mathf.Clamp01(sfx);MusicVolume=Mathf.Clamp01(bgm);ApplyGains();}
        public void SetMuted(bool value)
        {Muted=value;if(value)foreach(var source in voices)source.Stop();ApplyGains();}
        void ApplyGains()
        {
            foreach(var s in voices){s.volume=SfxVolume*.7f;s.mute=Muted;}
            music.RemoveAll(s=>s==null);foreach(var s in music){s.volume=MusicVolume;s.mute=Muted;} ApplyMusicGains();
        }
        public void SavePreferences()
        {PlayerPrefs.SetFloat(PreferencePrefix+"sfx",SfxVolume);PlayerPrefs.SetFloat(PreferencePrefix+"music",MusicVolume);PlayerPrefs.SetInt(PreferencePrefix+"mute",Muted?1:0);PlayerPrefs.Save();}
        public void SetSuspended(bool value)
        {paused=value;SyncMusicPause();if(Suspended)foreach(var source in voices)source.Stop();}
        void OnApplicationPause(bool value){SetSuspended(value);if(value)SavePreferences();}
        void OnApplicationFocus(bool value){focused=value;SyncMusicPause();if(!value){foreach(var source in voices)source.Stop();SavePreferences();}}
        void OnApplicationQuit()=>SavePreferences();
        void OnSceneLoaded(Scene scene,LoadSceneMode mode)=>RefreshListener();
        void RefreshListener()
        {
            var existing=FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Any(l=>l!=fallbackListener&&l.enabled&&l.gameObject.activeInHierarchy);
            if(fallbackListener==null&&!existing)fallbackListener=gameObject.AddComponent<AudioListener>();
            if(fallbackListener!=null)fallbackListener.enabled=!existing;
        }
        void OnDestroy(){SceneManager.sceneLoaded-=OnSceneLoaded;if(Instance==this)Instance=null;}
    }
}
