using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor.Build.Reporting;
using System.Linq;

namespace LightBetweenCultures.Editor
{
    public static class SliceValidation
    {
        public static void Run()
        {
            VerticalSliceSetup.SetupAll();
            SessionState.SetBool("SliceSmoke",true);
            EditorApplication.EnterPlaymode();
        }
        public static void BuildMilestone01()
        { LumiRoleSetup.Apply(); BuildWindows(); }
        public static void BuildWindows()
        {
            VerticalSliceSetup.SetupAll();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                locationPathName="Builds/Windows/LightBetweenCultures.exe",
                target=BuildTarget.StandaloneWindows64, options=BuildOptions.Development
            });
            Debug.Log("Windows build: "+report.summary.result);
            if(report.summary.result!=BuildResult.Succeeded) EditorApplication.Exit(1);
        }
    }
}
