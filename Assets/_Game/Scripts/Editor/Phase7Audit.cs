using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Explicit read-only inspection plus one requested Android build. No generators,
    // reslicing, binding repair, scene saves or gameplay initialization.
    public static class Phase7Audit
    {
        const string Folder="Docs/QA/Phase7";
        [Serializable] sealed class TextureRow { public string path,type,filter,compression; public int maxSize,width,height;public bool mipmaps,alpha,androidOverride; }
        [Serializable] sealed class GuidRow { public string consumer,guid;public bool runtimeDependency; }
        [Serializable] sealed class Audit { public string unity,activeTarget;public int assets,dependencies,missingScripts;public string[] roots,assemblyDefinitions,streamingAssets,addressables;public TextureRow[] textures;public GuidRow[] missingGuids; }
        [Serializable] sealed class AndroidResult {public string result,error,output,startedUTC,finishedUTC;public ulong bytes;public int errors,warnings;}
        public static void AuditAndAndroid()
        {
            Directory.CreateDirectory(Folder);
            var paths=AssetDatabase.GetAllAssetPaths().Where(p=>p.StartsWith("Assets/")&&!AssetDatabase.IsValidFolder(p)).ToArray();
            var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
            var pipelineRoots=new[]{"ProjectSettings/GraphicsSettings.asset","ProjectSettings/QualitySettings.asset"}.SelectMany(p=>Regex.Matches(File.ReadAllText(p),@"guid: ([a-f0-9]{32})").Cast<Match>()).Select(m=>AssetDatabase.GUIDToAssetPath(m.Groups[1].Value)).Where(p=>!string.IsNullOrEmpty(p));
            var roots=scenes.Concat(paths.Where(p=>p.Contains("/Resources/"))).Concat(pipelineRoots).Distinct().ToArray();
            var dependencies=new HashSet<string>(AssetDatabase.GetDependencies(roots,true));
            var missing=new List<GuidRow>();var textures=new List<TextureRow>();int missingScripts=0;
            foreach(var p in paths.Where(p=>new[]{".unity",".prefab",".asset",".mat",".controller",".anim"}.Contains(Path.GetExtension(p))))
            {
                if(!File.Exists(p))continue;var text=File.ReadAllText(p);
                foreach(Match m in Regex.Matches(text,@"guid: ([a-f0-9]{32})"))
                {var guid=m.Groups[1].Value;if(guid.StartsWith("00000000"))continue;if(string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(guid)))missing.Add(new GuidRow{consumer=p,guid=guid,runtimeDependency=dependencies.Contains(p)});}
            }
            foreach(var p in scenes)
            {
                var s=EditorSceneManager.OpenScene(p,OpenSceneMode.Single);
                foreach(var go in s.GetRootGameObjects())foreach(var t in go.GetComponentsInChildren<Transform>(true))missingScripts+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
            }
            foreach(var p in paths.Where(p=>p.EndsWith(".prefab")))
            {var root=AssetDatabase.LoadAssetAtPath<GameObject>(p);if(root!=null)foreach(var t in root.GetComponentsInChildren<Transform>(true))missingScripts+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);}
            foreach(var p in dependencies.Where(p=>p.StartsWith("Assets/")).OrderBy(p=>p))
            {
                if(!(AssetImporter.GetAtPath(p) is TextureImporter t))continue;
                t.GetSourceTextureWidthAndHeight(out var w,out var h);var android=t.GetPlatformTextureSettings("Android");
                textures.Add(new TextureRow{path=p,type=t.textureType.ToString(),filter=t.filterMode.ToString(),compression=t.textureCompression.ToString(),maxSize=t.maxTextureSize,width=w,height=h,mipmaps=t.mipmapEnabled,alpha=t.alphaIsTransparency,androidOverride=android.overridden});
            }
            var audit=new Audit{unity=Application.unityVersion,activeTarget=EditorUserBuildSettings.activeBuildTarget.ToString(),assets=paths.Length,dependencies=dependencies.Count,missingScripts=missingScripts,roots=roots,missingGuids=missing.ToArray(),textures=textures.ToArray(),assemblyDefinitions=paths.Where(p=>p.EndsWith(".asmdef")).ToArray(),streamingAssets=paths.Where(p=>p.StartsWith("Assets/StreamingAssets/")).ToArray(),addressables=paths.Where(p=>p.Contains("AddressableAssetsData/")).ToArray()};
            File.WriteAllText(Folder+"/native-asset-audit.json",JsonUtility.ToJson(audit,true));
            Debug.Log("PHASE7 NATIVE AUDIT: missingScripts="+missingScripts+" missingGuidRefs="+missing.Count+" textures="+textures.Count);
            if(Environment.GetEnvironmentVariable("LBC_PHASE7_ANDROID")!="1")return;
            var result=new AndroidResult{startedUTC=DateTime.UtcNow.ToString("O"),output="Builds/Android/Phase7-LightBetweenCultures.apk"};
            try
            {
                Directory.CreateDirectory("Builds/Android");
                var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes,locationPathName=result.output,target=BuildTarget.Android,options=BuildOptions.None});
                result.result=report.summary.result.ToString();result.bytes=report.summary.totalSize;result.errors=report.summary.totalErrors;result.warnings=report.summary.totalWarnings;
            }
            catch(Exception e){result.result="BLOCKED";result.error=e.ToString();Debug.LogError("PHASE7 ANDROID: "+e.Message);}
            result.finishedUTC=DateTime.UtcNow.ToString("O");File.WriteAllText(Folder+"/android-build-result.json",JsonUtility.ToJson(result,true));
            Debug.Log("PHASE7 ANDROID RESULT: "+result.result);
        }
    }
}
