using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class AndroidQaBuild
    {
        public static void Build()
        {
            const string output="Builds/Android/LightBetweenCultures-AndroidQA-20261024.apk";
            var identifier=PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            var version=PlayerSettings.bundleVersion;var code=PlayerSettings.Android.bundleVersionCode;
            var bundle=EditorUserBuildSettings.buildAppBundle;var internet=PlayerSettings.Android.forceInternetPermission;
            var result=1;
            try
            {
                if(EditorUserBuildSettings.activeBuildTarget!=BuildTarget.Android)throw new InvalidOperationException("Android build target required");
                AndroidQaFerryAudit.Validate();
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.DefaultCompany.LightBetweenCultures.androidqa");
                PlayerSettings.bundleVersion="1.0-rc2-androidqa-20261024";PlayerSettings.Android.bundleVersionCode=7;
                PlayerSettings.Android.forceInternetPermission=true;EditorUserBuildSettings.buildAppBundle=false;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.Android,options=BuildOptions.Development,extraScriptingDefines=new[]{"LBC_ANDROID_QA"}});
                Debug.Log("LBC_ANDROID_QA_BUILD "+report.summary.result+" errors="+report.summary.totalErrors+" seconds="+report.summary.totalTime.TotalSeconds);
                if(report.summary.result==BuildResult.Succeeded)result=0;
            }
            catch(Exception e){Debug.LogException(e);}
            finally
            {
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,identifier);PlayerSettings.bundleVersion=version;PlayerSettings.Android.bundleVersionCode=code;
                PlayerSettings.Android.forceInternetPermission=internet;EditorUserBuildSettings.buildAppBundle=bundle;AssetDatabase.SaveAssets();
            }
            EditorApplication.Exit(result);
        }
    }
}
