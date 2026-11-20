using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Explicit audit, not a deletion tool. Output and save fixtures stay in the requested QA directory.
    public static class AssetCleanupAudit
    {
        // GameplaySmokeRunner owns the isolated save, result file and Editor exit code.
        public static void RunValidationJourney()
        {
            if (!Environment.GetCommandLineArgs().Contains("--gameplay-smoke"))
                throw new ArgumentException("Explicit --gameplay-smoke is required");
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype.unity", OpenSceneMode.Single);
            EditorApplication.delayCall += EditorApplication.EnterPlaymode;
        }

        // Separate opt-in QA build. Never call production Prepare/setup or overwrite a delivery.
        public static void BuildValidationPlayer()
        {
            var arg = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--workspace-player-output="));
            if (arg == null) throw new ArgumentException("Provide --workspace-player-output=<new EXE path>");
            var output = Path.GetFullPath(arg.Substring("--workspace-player-output=".Length));
            var allowed = Path.GetFullPath("../_LBC_WORK/QA") + Path.DirectorySeparatorChar;
            if (!output.StartsWith(allowed, StringComparison.OrdinalIgnoreCase)) throw new IOException("QA output must be under _LBC_WORK/QA");
            var directory = Path.GetDirectoryName(output);
            if (Directory.Exists(directory)) throw new IOException("Existing QA Player is protected: " + directory);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                throw new InvalidOperationException("Isolated QA Player build failed: " + report.summary.result);
            Debug.Log("UNUSED ASSET QA PLAYER BUILD SUCCEEDED: " + directory);
        }

        public static void VerifyLayout()
        {
            Export();
            RouteGeometryRefactorChecks.Run();
            ArtLibrarySetup.Export();
        }

        [Serializable] private sealed class Row
        {
            public string path, guid;
            public bool runtimeDependency;
            public string[] dependencies;
            public string importer, standalone, android;
            public string[] sprites;
        }
        [Serializable] private sealed class Issue
        { public string consumer, reference; public bool runtimeDependency; }
        [Serializable] private sealed class Report
        {
            public string unity, buildTarget;
            public string[] roots;
            public Row[] assets;
            public Issue[] missingGuids, missingScripts, missingObjectReferences, resourceChecks;
            public int saveAssertions;
        }

        public static void Export()
        {
            var argument = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--asset-audit-output="));
            if (argument == null) throw new ArgumentException("Provide --asset-audit-output=<new QA directory>");
            var output = argument.Substring("--asset-audit-output=".Length);
            if (Directory.Exists(output)) throw new IOException("Existing evidence is protected: " + output);
            Directory.CreateDirectory(output);
            var paths = AssetDatabase.GetAllAssetPaths().Where(p => p.StartsWith("Assets/") && !AssetDatabase.IsValidFolder(p)).OrderBy(p => p).ToArray();
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            var roots = scenes.Concat(paths.Where(p => p.Contains("/Resources/"))).Concat(new[] { "ProjectSettings/GraphicsSettings.asset", "ProjectSettings/QualitySettings.asset", "ProjectSettings/ProjectSettings.asset" }).ToArray();
            var runtime = new HashSet<string>(AssetDatabase.GetDependencies(roots, true));
            var missingGuids = new List<Issue>();
            var missingScripts = new List<Issue>();
            var missingObjects = new List<Issue>();
            var resources = new List<Issue>();
            var rows = new List<Row>();
            foreach (var path in paths)
            {
                var row = new Row { path = path, guid = AssetDatabase.AssetPathToGUID(path), runtimeDependency = runtime.Contains(path), dependencies = AssetDatabase.GetDependencies(path, false) };
                if (AssetImporter.GetAtPath(path) is TextureImporter texture)
                {
                    row.importer = EditorJsonUtility.ToJson(texture);
                    row.standalone = JsonUtility.ToJson(texture.GetPlatformTextureSettings("Standalone"));
                    row.android = JsonUtility.ToJson(texture.GetPlatformTextureSettings("Android"));
                    row.sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Select(s =>
                    {
                        AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out string guid, out long id);
                        return s.name + "|" + guid + "|" + id + "|" + s.rect + "|" + s.pivot + "|" + s.border;
                    }).ToArray();
                }
                rows.Add(row);
                var extension = Path.GetExtension(path);
                if (new[] { ".unity", ".prefab", ".asset", ".mat", ".controller", ".anim", ".spriteatlas", ".spriteatlasv2" }.Contains(extension))
                {
                    foreach (Match match in Regex.Matches(File.ReadAllText(path), @"guid: ([a-f0-9]{32})"))
                    {
                        var guid = match.Groups[1].Value;
                        if (!guid.StartsWith("00000000") && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))
                            missingGuids.Add(new Issue { consumer = path, reference = guid, runtimeDependency = runtime.Contains(path) });
                    }
                    if (extension != ".unity")
                        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path)) InspectObject(asset, path, runtime, missingObjects);
                }
                if (extension == ".prefab") InspectHierarchy(AssetDatabase.LoadAssetAtPath<GameObject>(path), path, runtime, missingScripts, missingObjects);
            }
            foreach (var path in paths.Where(p => p.EndsWith(".unity")))
            {
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                foreach (var root in scene.GetRootGameObjects()) InspectHierarchy(root, path, runtime, missingScripts, missingObjects);
            }
            foreach (var path in paths.Where(p => p.EndsWith(".cs")))
            {
                var source = File.ReadAllText(path);
                foreach (Match match in Regex.Matches(source, "Resources\\.Load(?:<([^>]+)>)?\\(\"([^\"]+)\"\\s*\\)"))
                {
                    var key = match.Groups[2].Value;
                    var type = match.Groups[1].Value;
                    var value = type == "TextAsset" ? Resources.Load<TextAsset>(key) : Resources.Load(key);
                    resources.Add(new Issue { consumer = path, reference = key + "|" + type + "|" + (value != null ? "OK" : "MISSING"), runtimeDependency = !path.Contains("/Editor/") });
                }
            }
            foreach (var path in paths.Where(p => p.EndsWith(".json") && p.Contains("/Resources/")))
            {
                foreach (Match match in Regex.Matches(File.ReadAllText(path), "\"spritePath\"\\s*:\\s*\"([^\"]+)\"|\"leisure:([^\"]+)\""))
                {
                    var key = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                    resources.Add(new Issue { consumer = path, reference = "catalog:" + key + "|Sprite|" + (GameArtCatalog.Load().Leisure(key) != null ? "OK" : "MISSING"), runtimeDependency = true });
                }
            }
            var report = new Report { unity = Application.unityVersion, buildTarget = EditorUserBuildSettings.activeBuildTarget.ToString(), roots = roots, assets = rows.ToArray(), missingGuids = missingGuids.ToArray(), missingScripts = missingScripts.ToArray(), missingObjectReferences = missingObjects.ToArray(), resourceChecks = resources.ToArray(), saveAssertions = CheckSaves(output) };
            File.WriteAllText(Path.Combine(output, "native-audit.json"), JsonUtility.ToJson(report, true));
            Debug.Log("ASSET CLEANUP AUDIT: assets=" + rows.Count + " missingGuids=" + missingGuids.Count + " missingScripts=" + missingScripts.Count + " missingObjects=" + missingObjects.Count + " dynamicMissing=" + resources.Count(r => r.reference.EndsWith("|MISSING")) + " saveAssertions=" + report.saveAssertions);
        }

        private static void InspectHierarchy(GameObject root, string path, HashSet<string> runtime, List<Issue> scripts, List<Issue> objects)
        {
            if (root == null) return;
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
            {
                var count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                if (count > 0) scripts.Add(new Issue { consumer = path, reference = transform.name + "|" + count, runtimeDependency = runtime.Contains(path) });
                foreach (var component in transform.GetComponents<Component>()) InspectObject(component, path, runtime, objects);
            }
        }
        private static void InspectObject(UnityEngine.Object target, string path, HashSet<string> runtime, List<Issue> issues)
        {
            if (target == null) return;
            var iterator = new SerializedObject(target).GetIterator();
            while (iterator.Next(true))
                if (iterator.propertyType == SerializedPropertyType.ObjectReference && iterator.objectReferenceValue == null && iterator.objectReferenceEntityIdValue != default)
                    issues.Add(new Issue { consumer = path, reference = target.name + "|" + iterator.propertyPath, runtimeDependency = runtime.Contains(path) });
        }
        private static int CheckSaves(string output)
        {
            var original = SaveSystem.OverridePath;
            var assertions = 0;
            try
            {
                foreach (var version in new[] { 1, 2 })
                {
                    SaveSystem.OverridePath = Path.GetFullPath(Path.Combine(output, "legacy-v" + version + ".json"));
                    File.WriteAllText(SaveSystem.OverridePath, "{\"version\":" + version + ",\"node\":1,\"items\":[\"lamp_eye\"],\"collected\":[\"lamp_eye\"],\"accepted\":[\"HQ01\"],\"completed\":[],\"flags\":[\"quest:HQ01\"],\"coins\":17}");
                    var loaded = SaveSystem.Load(100);
                    if (loaded.version != 2 || loaded.node != 1 || loaded.coins != 17 || !loaded.items.SequenceEqual(new[] { "lamp_eye" }) || !loaded.accepted.Contains("HQ01") || !loaded.flags.Contains("quest:HQ01")) throw new InvalidDataException("Legacy save load changed");
                    assertions++;
                    if (!SaveSystem.Write(loaded)) throw new IOException("Isolated save write failed");
                    var second = SaveSystem.Load(100);
                    if (JsonUtility.ToJson(second) != JsonUtility.ToJson(loaded)) throw new InvalidDataException("Save round-trip changed");
                    assertions++;
                }
            }
            finally { SaveSystem.OverridePath = original; }
            return assertions;
        }
    }
}
