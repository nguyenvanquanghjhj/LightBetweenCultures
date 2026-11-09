using System.IO;
using LightBetweenCultures.HoiAn;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LightBetweenCultures.Editor
{
    public static class HoiAnDemoSetup
    {
        private const string ScenePath = "Assets/_Game/Scenes/HoiAnDemo.unity";
        private const string SpritePath = "Assets/_Game/Art/Environment/Common/UnitSprite.png";

        [MenuItem("Tools/Light Between Cultures/Setup Hoi An Demo")]
        public static void Setup()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;
            if (!Application.isBatchMode && File.Exists(ScenePath) &&
                !EditorUtility.DisplayDialog("Ghi đè HoiAnDemo?", "Chỉ scene HoiAnDemo sẽ được tạo lại.", "Ghi đè", "Hủy"))
                return;

            EnsureFolder("Assets/_Game");
            EnsureFolder("Assets/_Game/Scenes");
            EnsureFolder("Assets/_Game/Art");
            EnsureFolder("Assets/_Game/Art/Environment/Common");
            if (File.Exists(ScenePath))
            {
                var backup = Path.GetFullPath("../Backups/SceneRebuilds");
                Directory.CreateDirectory(backup);
                File.Copy(ScenePath, Path.Combine(backup,"HoiAnDemo-"+System.DateTime.Now.ToString("yyyyMMdd-HHmmss")+".unity"));
            }
            var sprite = CreatePlaceholderSprite();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camera = CreateCamera();
            CreateWorld(sprite);
            CreateNpc(sprite);
            var item = CreateItem(sprite);
            var player = CreatePlayer(sprite);
            
            var gameplayObject = new GameObject("HoiAnGameplay");
            gameplayObject.AddComponent<HoiAnGameplay>().Configure(player, item);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            camera.gameObject.AddComponent<FollowCamera2D>().Configure(player.transform);
            DesignKitSetup.ApplyCurrentHoiAnScene();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);
            Selection.activeGameObject = player.gameObject;
            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog("Light Between Cultures",
                    "HoiAnDemo đã được tạo. Mở scene và nhấn Play.", "OK");
        }

        private static Camera CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
            camera.backgroundColor = new Color(.08f, .15f, .18f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        private static void CreateWorld(Sprite sprite)
        {
            CreateVisual("Ground", sprite, new Vector2(0f, 0f), new Vector2(20f, 14f),
                new Color(.72f, .57f, .37f), -10);
            CreateVisual("Road", sprite, new Vector2(0f, -1.1f), new Vector2(15f, 2.2f),
                new Color(.82f, .69f, .49f), -9);
            CreateObstacle("Wall_W", sprite, new Vector2(-8.25f,0), new Vector2(.5f,10), Color.clear);
            CreateObstacle("Wall_E", sprite, new Vector2(8.25f,0), new Vector2(.5f,10), Color.clear);
            CreateObstacle("Wall_N", sprite, new Vector2(0,5.25f), new Vector2(16,.5f), Color.clear);
            CreateObstacle("Wall_S", sprite, new Vector2(0,-5.25f), new Vector2(16,.5f), Color.clear);
            CreateObstacle("River_Left", sprite, new Vector2(-5.1f, 3.75f), new Vector2(5.8f, 1.5f),
                new Color(.16f, .55f, .68f));
            CreateObstacle("River_Right", sprite, new Vector2(5.1f, 3.75f), new Vector2(5.8f, 1.5f),
                new Color(.16f, .55f, .68f));
            CreateVisual("Bridge", sprite, new Vector2(0f, 3.75f), new Vector2(3.4f, 1.7f),
                new Color(.53f, .28f, .12f), -7);
            CreateObstacle("House_Left", sprite, new Vector2(-5f, 1.8f), new Vector2(3.6f, 2.6f),
                new Color(.87f, .62f, .30f));
            CreateObstacle("House_Right", sprite, new Vector2(4.5f, 1.8f), new Vector2(3.4f, 2.6f),
                new Color(.92f, .72f, .38f));
            CreateObstacle("Tree_Left", sprite, new Vector2(-6.3f, -2.5f), new Vector2(1.1f, 1.5f),
                new Color(.12f, .39f, .17f));
            CreateObstacle("Tree_Right", sprite, new Vector2(6.2f, -2.4f), new Vector2(1.1f, 1.5f),
                new Color(.12f, .39f, .17f));
            CreateVisual("Lantern_Left", sprite, new Vector2(-2.4f, 1.0f), new Vector2(.35f, .7f),
                new Color(1f, .35f, .08f), 2);
            CreateVisual("Lantern_Right", sprite, new Vector2(2.5f, 1.0f), new Vector2(.35f, .7f),
                new Color(1f, .35f, .08f), 2);
        }

        private static HoiAnNpc CreateNpc(Sprite sprite)
        {
            var npcObject = CreateVisual("Nguoi dan (placeholder)", sprite, new Vector2(-2.5f, -1.0f),
                new Vector2(.8f, 1.2f), new Color(.93f, .40f, .18f), 4);
            var collider = npcObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one * 1.8f;
            return npcObject.AddComponent<HoiAnNpc>();
        }

        private static HoiAnItem CreateItem(Sprite sprite)
        {
            var itemObject = CreateVisual("Linh kien den", sprite, new Vector2(5f, -1.0f),
                new Vector2(.55f, .55f), new Color(1f, .86f, .12f), 4);
            var collider = itemObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one * 1.5f;
            var item = itemObject.AddComponent<HoiAnItem>();

            return item;
        }

        private static HoiAnPlayerController CreatePlayer(Sprite sprite)
        {
            var playerObject = CreateVisual("Lumi Player", sprite, new Vector2(-6f, -1.0f),
                new Vector2(.8f, 1.1f), new Color(.95f, .24f, .10f), 5);
            var body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
            var collider = playerObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(.7f, .6f);
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var label = new GameObject("Lumi label");
            label.transform.SetParent(playerObject.transform,false);
            label.transform.localPosition = new Vector3(0,.8f,0);
            var text = label.AddComponent<TextMesh>();
            text.text="Lumi"; text.font=UIController.Font; text.fontSize=40; text.characterSize=.1f;
            text.anchor=TextAnchor.MiddleCenter; text.color=Color.white;
            label.GetComponent<MeshRenderer>().sharedMaterial=text.font.material;
            label.GetComponent<MeshRenderer>().sortingOrder=300;
            return playerObject.AddComponent<HoiAnPlayerController>();
        }

        private static GameObject CreateObstacle(string name, Sprite sprite, Vector2 position,
            Vector2 size, Color color)
        {
            var objectToCreate = CreateVisual(name, sprite, position, size, color, 0);
            objectToCreate.layer = 8;
            var collider = objectToCreate.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            return objectToCreate;
        }

        private static GameObject CreateVisual(string name, Sprite sprite, Vector2 position,
            Vector2 size, Color color, int sortingOrder)
        {
            var objectToCreate = new GameObject(name);
            objectToCreate.transform.position = position;
            objectToCreate.transform.localScale = size;
            var renderer = objectToCreate.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            if (sortingOrder >= 0) objectToCreate.AddComponent<DepthSort2D>();
            return objectToCreate;
        }

        private static Sprite CreatePlaceholderSprite()
        {
            if (!File.Exists(SpritePath))
            {
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
                texture.Apply();
                File.WriteAllBytes(SpritePath, texture.EncodeToPNG());
                Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(SpritePath);
            }
            var importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 2;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.filterMode = FilterMode.Point;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var index = scenes.FindIndex(scene => scene.path == scenePath);
            var entry = new EditorBuildSettingsScene(scenePath, true);
            if (index >= 0)
                scenes[index] = entry;
            else
                scenes.Add(entry);
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path).Replace("\\", "/");
                AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
            }
        }
    }
}
