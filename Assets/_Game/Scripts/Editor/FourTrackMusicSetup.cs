using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class FourTrackMusicSetup
    {
        public static void Build()
        {
            var names=new[]{"LBC_Map_Vietnam","LBC_Battle_Vietnam","LBC_Map_Korea","LBC_Battle_Korea"};
            var clips=new AudioClip[4];
            for(var i=0;i<4;i++)
            {
                var path="Assets/_Game/Audio/Music/"+names[i]+".mp3";
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);
                if(importer==null)throw new InvalidOperationException("Missing music: "+path);
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;
                settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.8f;
                settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;settings.preloadAudioData=false;
                importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.loadInBackground=true;
                foreach(var platform in new[]{"Standalone","Android"})if(importer.ContainsSampleSettingsOverride(platform))importer.SetOverrideSampleSettings(platform,settings);
                importer.SaveAndReimport();clips[i]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
            const string pathCatalog="Assets/_Game/Resources/SoftMusicClips.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<SoftMusicClips>(pathCatalog);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<SoftMusicClips>();AssetDatabase.CreateAsset(catalog,pathCatalog);}
            catalog.clips=clips;catalog.loopStart=new[]{.1f,.1f,.1f,.1f};
            // Original MP3 tail analysis, audible before the near-silent decay. Never edits source audio.
            catalog.loopEnd=new[]{155f,78.4f,0f,145.8f};catalog.loopEnd[2]=clips[2].length-2.2f;
            EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Windows/LightBetweenCultures.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            Debug.Log("FOUR TRACK MUSIC WINDOWS BUILD: "+report.summary.result);
            if(report.summary.result!=BuildResult.Succeeded)EditorApplication.Exit(1);
        }
    }
}
