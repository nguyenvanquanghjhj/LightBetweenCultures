using UnityEngine;
using UnityEngine.SceneManagement;
namespace LightBetweenCultures
{
    public sealed partial class SoftAudio
    {
        const float MusicFade=1.2f;
        readonly AudioSource[] bgm=new AudioSource[2];
        readonly float[] envelope=new float[2];
        SoftMusicClips musicClips;
        int selected=-1,currentVoice;
        bool musicPaused;
        public int MusicTrack=>selected;
        public int MusicStarts {get;private set;}
        public int MusicLoops {get;private set;}
        public AudioSource CurrentMusic=>bgm[currentVoice];
        void InitMusic()
        {
            musicClips=Resources.Load<SoftMusicClips>("SoftMusicClips");
            for(var i=0;i<2;i++)
            {
                var child=new GameObject("MusicVoice"+i);child.transform.SetParent(transform,false);
                var source=child.AddComponent<AudioSource>();source.playOnAwake=false;source.spatialBlend=0;
                source.priority=128;source.loop=false;source.volume=0;bgm[i]=source;
            }
        }
        int MusicContext()
        {
            var game=GameBootstrap.Instance;if(game?.Save==null)return -1;
            var scene=SceneManager.GetActiveScene().name;
            if(scene!="CityMap"&&scene!="WorldMap"&&scene!="HoiAnDemo")return -1;
            var city=game.Save.cityId;
            var region=city=="hoi_an"||city=="hue"||city=="da_lat"?0:city=="jeonju"||city=="gyeongju"||city=="seoul"?2:-1;
            if(region<0)return -1;
            // A stale battle service after scene exit must never keep battle music alive.
            var fighting=scene=="CityMap"&&game.Battles!=null&&game.Battles.Active&&GameObject.Find("BattlePanel")!=null;
            return region+(fighting?1:0);
        }
        void TickMusic()
        {
            if(musicClips==null)return;
            SyncMusicPause();if(musicPaused)return;
            var wanted=GameBootstrap.Instance?.Scenes?.Busy==true?selected:MusicContext();
            if(wanted!=selected)
            {
                selected=wanted;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log("MUSIC CONTEXT track="+selected+" suspended="+Suspended+" muted="+Muted);
#endif
                if(selected>=0)StartMusicVoice(selected,false);
            }
            if(selected>=0&&envelope[1-currentVoice]<=0&&bgm[currentVoice].clip!=null)
            {
                if(!bgm[currentVoice].isPlaying||bgm[currentVoice].time>=musicClips.loopEnd[selected]-MusicFade)
                    StartMusicVoice(selected,true);
            }
            for(var i=0;i<2;i++)
            {
                var target=selected>=0&&i==currentVoice?1f:0f;
                envelope[i]=Mathf.MoveTowards(envelope[i],target,Time.unscaledDeltaTime/MusicFade);
                if(target==0&&envelope[i]<=0&&bgm[i].clip!=null){bgm[i].Stop();bgm[i].clip=null;}
            }
            ApplyMusicGains();
        }
        void StartMusicVoice(int track,bool loop)
        {
            currentVoice=1-currentVoice;var source=bgm[currentVoice];source.Stop();envelope[currentVoice]=0;
            source.clip=musicClips.clips[track];source.time=musicClips.loopStart[track];source.volume=0;source.Play();
            MusicStarts++;if(loop)MusicLoops++;
        }
        void ApplyMusicGains()
        {
            for(var i=0;i<2;i++)if(bgm[i]!=null){bgm[i].volume=MusicVolume*.6f*envelope[i];bgm[i].mute=Muted;}
            SyncMusicPause();
        }
        void SyncMusicPause()
        {
            var suspend=Suspended||Muted;
            if(suspend==musicPaused)return;musicPaused=suspend;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("MUSIC SUSPENSION suspended="+suspend+" focused="+focused+" paused="+paused+" muted="+Muted);
#endif
            foreach(var source in bgm)if(source!=null){if(suspend)source.Pause();else source.UnPause();}
        }
    }
}
