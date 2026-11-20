using System;
using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Authored H3 is canonical. The retired presentation generator is archived outside Assets.
    public static class H3ArtSetup
    {
        public static void VerifyLegacyBuilderGuard()
        {
            const string scene="Assets/_Game/Scenes/HoiAnDemo.unity";
            var before=File.ReadAllBytes(scene);
            var active=EditorSceneManager.GetActiveScene().path;
            HoiAnDemoSetup.Setup();
            if(!before.SequenceEqual(File.ReadAllBytes(scene))||EditorSceneManager.GetActiveScene().path!=active)
                throw new InvalidDataException("Legacy scene setup modified the authored H3 scene or active editor scene.");
            Directory.CreateDirectory("Docs/Development/QualityAddendum-20261017");
            File.WriteAllText("Docs/Development/QualityAddendum-20261017/editor-guard-result.json",
                "{\"status\":\"PASS\",\"actualLegacySetupInvoked\":true,\"sceneBytesUnchanged\":true,\"activeSceneUnchanged\":true,\"playerBuildRequired\":false}");
            Debug.Log("H3 LEGACY BUILDER GUARD PASS: actual setup call preserved authored scene bytes and active scene.");
        }
    }
}
