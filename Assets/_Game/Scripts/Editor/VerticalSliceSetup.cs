using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class VerticalSliceSetup
    {
        [MenuItem("Tools/Light Between Cultures/Create Missing Vertical Slice Scenes")]
        public static void SetupAll()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!File.Exists("Assets/_Game/Scenes/Prototype.unity")) PrototypeSetup.SetupPrototype();
            if (!File.Exists("Assets/_Game/Scenes/WorldMap.unity")) CreateMap<WorldMap>("WorldMap");
            if (!File.Exists("Assets/_Game/Scenes/CityMap.unity")) CreateMap<CityMap>("CityMap");
            if (!File.Exists("Assets/_Game/Scenes/HoiAnDemo.unity")) HoiAnDemoSetup.Setup();
            var paths = new[] { "Prototype", "WorldMap", "CityMap", "HoiAnDemo" }.Select(n => "Assets/_Game/Scenes/"+n+".unity").ToArray();
            EditorBuildSettings.scenes = paths.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(EditorBuildSettings.scenes.Where(s=>!paths.Contains(s.path))).ToArray();
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.defaultScreenWidth=540; PlayerSettings.defaultScreenHeight=960;
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(paths[0]);
            Debug.Log("Vertical slice scenes ready; existing unrelated build entries preserved.");
        }
        private static void CreateMap<T>(string name) where T:Component
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)); camera.tag="MainCamera";
            camera.transform.position=new Vector3(0,0,-10); camera.GetComponent<Camera>().clearFlags=CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor=new Color(.08f,.15f,.18f);
            new GameObject(name).AddComponent<T>();
            EditorSceneManager.SaveScene(scene,"Assets/_Game/Scenes/"+name+".unity");
        }
    }
}
