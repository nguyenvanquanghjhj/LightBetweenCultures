using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class CatRescueBuild
    {
        const string Version="1.1.2-catventure-20261109";
        static void Prepare()
        {
            foreach(var name in new[]{"RescueCats","DaLatStation"})
            {
                var path="Assets/_Game/Resources/CatRescue/"+name+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=name=="RescueCats"?TextureImporterType.Default:TextureImporterType.Sprite;
                importer.alphaIsTransparency=true;importer.maxTextureSize=2048;importer.mipmapEnabled=false;
                importer.textureCompression=TextureImporterCompression.Uncompressed;
                if(name!="RescueCats"){importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;}
                importer.SaveAndReimport();
            }
        }
        public static void BuildWindows()
        {
            Prepare();
            var args=Environment.GetCommandLineArgs();
            var opt=args.FirstOrDefault(a=>a.StartsWith("--cat-build-output="));
            var release=args.Contains("--cat-release");
            var output=opt==null?(release?"Builds/Windows-Catventure-Visible-Release-20261109/Catventure.exe":"Builds/Windows-Catventure-Visible-20261109/Catventure.exe"):opt.Substring("--cat-build-output=".Length);
            var oldVersion=PlayerSettings.bundleVersion;
            try
            {
                PlayerSettings.bundleVersion=Version;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                    locationPathName=output,target=BuildTarget.StandaloneWindows64,options=release?BuildOptions.None:BuildOptions.Development});
                Record(release?"windows-release":"windows",report);
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Catventure Windows build failed");
            }
            finally{PlayerSettings.bundleVersion=oldVersion;}
        }
        public static void BuildAndroid()
        {
            Prepare();
            var oldVersion=PlayerSettings.bundleVersion;var oldCode=PlayerSettings.Android.bundleVersionCode;
            var bundle=EditorUserBuildSettings.buildAppBundle;
            try
            {
                if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new InvalidOperationException("Android target required");
                PlayerSettings.bundleVersion=Version;PlayerSettings.Android.bundleVersionCode=22;
                EditorUserBuildSettings.buildAppBundle=false;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
                    scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                    locationPathName="Builds/Android/Catventure-Visible-20261109.apk",target=BuildTarget.Android,options=BuildOptions.None});
                Record("android",report);
                if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Catventure Android build failed");
            }
            finally{PlayerSettings.bundleVersion=oldVersion;PlayerSettings.Android.bundleVersionCode=oldCode;EditorUserBuildSettings.buildAppBundle=bundle;}
        }
        static void Record(string platform,BuildReport report)
        {
            Directory.CreateDirectory("Validation/Catventure-Visible-20261109");
            File.WriteAllText("Validation/Catventure-Visible-20261109/"+platform+"-build.txt",report.summary.result+" | version="+Version+" | errors="+report.summary.totalErrors+" | bytes="+report.summary.totalSize+" | seconds="+report.summary.totalTime.TotalSeconds);
        }
    }
}
