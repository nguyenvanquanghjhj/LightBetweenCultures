using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Explicit packaging entry point. No gameplay/asset/setup generators.
    public static class AndroidApkBuild
    {
        const string Folder = "Docs/QA/AndroidApk-20261019";
        [Serializable] sealed class Result
        {
            public string startedUTC, finishedUTC, unity, result, error, output;
            public string identifier, backend, architectures, orientation, graphics;
            public int minSDK, targetSDK;
            public bool customKeystore, appBundleBefore;
            public string[] scenes;
            public ulong bytes;
            public int errors, warnings;
            public double buildSeconds;
        }

        public static void Build()
        {
            Directory.CreateDirectory(Folder);
            var r = new Result
            {
                startedUTC = DateTime.UtcNow.ToString("O"), unity = Application.unityVersion,
                output = "Builds/Android/LightBetweenCultures-Test.apk",
                identifier = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android),
                backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android).ToString(),
                architectures = PlayerSettings.Android.targetArchitectures.ToString(),
                orientation = PlayerSettings.defaultInterfaceOrientation.ToString(),
                graphics = string.Join(",", PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).Select(a => a.ToString())),
                minSDK = (int)PlayerSettings.Android.minSdkVersion,
                targetSDK = (int)PlayerSettings.Android.targetSdkVersion,
                customKeystore = PlayerSettings.Android.useCustomKeystore,
                appBundleBefore = EditorUserBuildSettings.buildAppBundle,
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray()
            };
            File.WriteAllText(Folder + "/build-config.json", JsonUtility.ToJson(r, true));
            Debug.Log("ANDROID APK CONFIG: " + JsonUtility.ToJson(r));
            int exit = 1;
            try
            {
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                    throw new InvalidOperationException("Launch with -buildTarget Android; do not switch platforms implicitly here.");
                if (File.Exists(r.output))
                    throw new IOException("Refuse to overwrite an existing APK; retain its evidence first.");
                Directory.CreateDirectory("Builds/Android");
                EditorUserBuildSettings.buildAppBundle = false;
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = r.scenes, locationPathName = r.output,
                    target = BuildTarget.Android, options = BuildOptions.None
                });
                r.result = report.summary.result.ToString();
                r.bytes = report.summary.totalSize;
                r.errors = report.summary.totalErrors;
                r.warnings = report.summary.totalWarnings;
                r.buildSeconds = report.summary.totalTime.TotalSeconds;
                if (report.summary.result == BuildResult.Succeeded && File.Exists(r.output)) exit = 0;
            }
            catch (Exception e) { r.result = "FAILED"; r.error = e.ToString(); Debug.LogException(e); }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = r.appBundleBefore;
                r.finishedUTC = DateTime.UtcNow.ToString("O");
                File.WriteAllText(Folder + "/build-result.json", JsonUtility.ToJson(r, true));
                Debug.Log("ANDROID APK RESULT: " + r.result + "; exit=" + exit);
            }
            EditorApplication.Exit(exit);
        }
    }
}
