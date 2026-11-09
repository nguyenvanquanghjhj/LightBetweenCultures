using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Explicit QA entry point. Opens an existing scene without saving it.
    public static class PlayerExperienceQA
    {
        const string Session="LBC.PlayerExperience.EditorQA";
        [InitializeOnLoadMethod]
        static void Resume()
        {
            EditorApplication.playModeStateChanged+=state=>
            {
                if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Session,false)&&UnityEngine.Object.FindAnyObjectByType<GameplaySmokeRunner>()==null)
                    new GameObject("GameplaySmokeRunner").AddComponent<GameplaySmokeRunner>();
            };
            EditorApplication.quitting+=()=>{if(SessionState.GetBool(Session,false)){EditorSettings.enterPlayModeOptionsEnabled=SessionState.GetBool(Session+"Original",true);SessionState.SetBool(Session,false);}};
        }
        public static void Run()
        {
            SessionState.SetBool(Session+"Original",EditorSettings.enterPlayModeOptionsEnabled);
            SessionState.SetBool(Session,true);
            // Existing project disables domain reload; a fresh QA process must
            // initialise runtime statics identically to a Player, then restore.
            EditorSettings.enterPlayModeOptionsEnabled=false;
            var asm=typeof(EditorWindow).Assembly;
            var sizesType=asm.GetType("UnityEditor.GameViewSizes");
            var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes=singleton.GetProperty("instance",BindingFlags.Static|BindingFlags.Public|BindingFlags.FlattenHierarchy).GetValue(null);
            var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{(int)BuildTargetGroup.Standalone});
            var total=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
            var sizeType=asm.GetType("UnityEditor.GameViewSize");var kind=asm.GetType("UnityEditor.GameViewSizeType");
            var tall=Environment.GetCommandLineArgs().Contains("--player-experience-720");
            var fixedSize=Activator.CreateInstance(sizeType,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance,null,new object[]{Enum.Parse(kind,"FixedResolution"),tall?720:540,tall?1280:960,tall?"LBC QA 720x1280":"LBC QA 540x960"},null);
            group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{fixedSize});
            var viewType=asm.GetType("UnityEditor.GameView");var view=EditorWindow.GetWindow(viewType);
            viewType.GetProperty("selectedSizeIndex",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance).SetValue(view,total);
            view.Focus();
            EditorSceneManager.OpenScene(EditorBuildSettings.scenes.First(s=>s.enabled).path);
            EditorApplication.EnterPlaymode();
        }
    }
}
