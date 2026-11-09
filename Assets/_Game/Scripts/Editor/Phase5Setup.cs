using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class Phase5Setup
    {
        public static void Build()
        {
            // Existing scenes/assets only. No setup generator, art export or audio reimport.
            var requested=System.Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--qa-build-output="));
            var output=requested==null?"Builds/Windows/LightBetweenCultures.exe":requested.Substring("--qa-build-output=".Length);
            var release=System.Environment.GetCommandLineArgs().Contains("--qa-build-release");
            var scriptsOnly=System.Environment.GetCommandLineArgs().Contains("--qa-build-scripts-only");
            var version=System.Environment.GetCommandLineArgs().FirstOrDefault(a=>a.StartsWith("--qa-build-version="));
            var oldVersion=PlayerSettings.bundleVersion;
            try
            {
                if(version!=null)PlayerSettings.bundleVersion=version.Substring("--qa-build-version=".Length);
                var options=release?BuildOptions.None:BuildOptions.Development;
                if(scriptsOnly)options|=BuildOptions.BuildScriptsOnly;
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=output,target=BuildTarget.StandaloneWindows64,options=options});
                Debug.Log("PHASE5 WINDOWS BUILD: "+report.summary.result);
                if(report.summary.result!=BuildResult.Succeeded)EditorApplication.Exit(1);
            }
            finally{PlayerSettings.bundleVersion=oldVersion;}
        }
    }
}
