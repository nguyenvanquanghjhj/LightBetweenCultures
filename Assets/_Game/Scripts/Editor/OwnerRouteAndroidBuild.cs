using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Packaging only, unchanged project signing/package settings. Never overwrites
    // the protected Test.apk or the previous Android QA acceptance report.
    public static class OwnerRouteAndroidBuild
    {
        [Serializable] class Result
        {public string output,identifier,result,error,startedUTC,finishedUTC,version;public long bytes;public bool customKeystore;public int errors,warnings,versionCode;}
        public static void Build()
        {
            var r=new Result{output="Builds/Android/LightBetweenCultures-OwnerRoutesB-20261019.apk",identifier=PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),customKeystore=PlayerSettings.Android.useCustomKeystore,startedUTC=DateTime.UtcNow.ToString("O")};
            var args=Environment.GetCommandLineArgs();var customOutput=args.FirstOrDefault(a=>a.StartsWith("--qa-android-output="));
            if(customOutput!=null)r.output=customOutput.Substring("--qa-android-output=".Length);
            var customReport=args.FirstOrDefault(a=>a.StartsWith("--qa-android-report="));
            var reportPath=customReport==null?"Docs/Maps/OwnerRoutePhaseB-20261019/android-build.json":customReport.Substring("--qa-android-report=".Length);
            var bundle=EditorUserBuildSettings.buildAppBundle;var oldVersion=PlayerSettings.bundleVersion;var oldCode=PlayerSettings.Android.bundleVersionCode;var exit=1;
            try
            {
                if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android||File.Exists(r.output))throw new InvalidOperationException("Require Android target and a new APK output path");
                var version=args.FirstOrDefault(a=>a.StartsWith("--qa-android-version="));
                var code=args.FirstOrDefault(a=>a.StartsWith("--qa-android-code="));
                if(version!=null)PlayerSettings.bundleVersion=version.Substring("--qa-android-version=".Length);
                if(code!=null)PlayerSettings.Android.bundleVersionCode=int.Parse(code.Substring("--qa-android-code=".Length));
                r.version=PlayerSettings.bundleVersion;r.versionCode=PlayerSettings.Android.bundleVersionCode;
                EditorUserBuildSettings.buildAppBundle=false;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=r.output,target=BuildTarget.Android,options=BuildOptions.None});
                r.result=report.summary.result.ToString();r.bytes=(long)report.summary.totalSize;r.errors=(int)report.summary.totalErrors;r.warnings=(int)report.summary.totalWarnings;
                if(report.summary.result==BuildResult.Succeeded)exit=0;
            }
            catch(Exception e){r.result="FAILED";r.error=e.ToString();Debug.LogException(e);}
            finally
            {
                PlayerSettings.bundleVersion=oldVersion;PlayerSettings.Android.bundleVersionCode=oldCode;
                EditorUserBuildSettings.buildAppBundle=bundle;r.finishedUTC=DateTime.UtcNow.ToString("O");Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
                File.WriteAllText(reportPath,JsonUtility.ToJson(r,true));Debug.Log("OWNER ROUTE ANDROID BUILD: "+r.result);
            }
            EditorApplication.Exit(exit);
        }
    }
}
