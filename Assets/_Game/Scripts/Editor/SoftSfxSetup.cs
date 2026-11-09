using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class SoftSfxSetup
    {
        public static void Build()
        {
            var names=Enum.GetNames(typeof(SoftCue));var clips=new AudioClip[names.Length];
            for(var i=0;i<names.Length;i++)
            {
                var path="Assets/_Game/Audio/SFX/"+names[i]+".wav";
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(AudioImporter)AssetImporter.GetAtPath(path);var settings=importer.defaultSampleSettings;
                settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;
                // WAVs are already mono; don't downmix/normalize their authored headroom.
                settings.preloadAudioData=true;importer.defaultSampleSettings=settings;importer.forceToMono=false;importer.loadInBackground=false;importer.SaveAndReimport();
                clips[i]=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if(clips[i]==null)throw new InvalidOperationException("Missing SFX "+path);
            }
            const string catalogPath="Assets/_Game/Resources/SoftAudioClips.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<SoftAudioClips>(catalogPath);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<SoftAudioClips>();AssetDatabase.CreateAsset(catalog,catalogPath);}
            catalog.clips=clips;EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();
            // Existing scenes only. No art export, no scene generation, no MP3 import edits.
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Windows/LightBetweenCultures.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            Debug.Log("SOFT SFX WINDOWS BUILD: "+report.summary.result);
            if(report.summary.result!=BuildResult.Succeeded)EditorApplication.Exit(1);
        }
    }
}
