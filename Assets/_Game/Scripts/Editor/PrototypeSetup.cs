using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace LightBetweenCultures.Editor
{
    public static class PrototypeSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/Prototype.unity";

        [MenuItem("Tools/Light Between Cultures/Setup Prototype")]
        public static void SetupPrototype()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!Application.isBatchMode && System.IO.File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Ghi đè prototype?", "Prototype.unity đã tồn tại. Chỉ scene prototype này sẽ bị thay thế.", "Ghi đè", "Hủy"))
                return;

            EnsureFolder("Assets/_Game");
            EnsureFolder("Assets/_Game/Scenes");
            EnsureFolder("Assets/_Game/Scripts");
            EnsureFolder("Assets/_Game/Scripts/Editor");
            EnsureFolder("Assets/_Game/Scripts/Prototype");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.backgroundColor = new Color(.96f, .91f, .78f);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            var bootstrap = new GameObject("PrototypeGame");
            bootstrap.AddComponent<LightBetweenCultures.PrototypeGame>();
            EditorSceneManager.SaveScene(scene, ScenePath);

            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var prototypeIndex = scenes.FindIndex(item => item.path == ScenePath);
            var prototypeScene = new EditorBuildSettingsScene(ScenePath, true);
            if (prototypeIndex >= 0)
                scenes[prototypeIndex] = prototypeScene;
            else
                scenes.Add(prototypeScene);
            EditorBuildSettings.scenes = scenes.ToArray();
            Selection.activeGameObject = bootstrap;
            if (!Application.isBatchMode) EditorUtility.DisplayDialog("Light Between Cultures", "Prototype scene đã được tạo. Nhấn Play để chạy.", "OK");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                var name = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
