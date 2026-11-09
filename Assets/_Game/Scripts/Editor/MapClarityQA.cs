using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Explicit, read-only import audit. Never rebuilds scenes or changes bindings.
    public static class MapClarityQA
    {
        [Serializable] class TextureInfo
        {
            public string path, format, filter, compression, packingTag, standalone, android, mipFilter;
            public int sourceWidth,sourceHeight,width,height,maxSize,mipmapCount;
            public float mipBias; public bool alpha, mipmaps,crunch;
        }
        [Serializable] class BindingInfo
        {public string role,id,path,sprite,guid;public long localId;public Rect rect;public bool packed;}
        [Serializable] class Audit
        {public string utc,target;public TextureInfo[] textures;public BindingInfo[] bindings;}
        public static void Run()
        {
            var output=Environment.GetCommandLineArgs().First(a=>a.StartsWith("--gameplay-output=")).Substring(18);
            Directory.CreateDirectory(output);
            var art=AdventureUIArt.Load();var kit=DesignKitArt.Load();
            var refs=new List<(string role,string id,Sprite sprite)>();
            refs.Add(("city_main","hoi_an",kit.hoiAnMap));refs.Add(("lumi","map",kit.lumiMapTraveler));
            foreach(var c in art.cities.Where(c=>c.id!="hoi_an"))refs.Add(("city_main",c.id,c.sprite));
            foreach(var actor in art.mapActors)refs.Add(("map_actor",actor.cityId+":"+actor.nodeId+":"+actor.artId,art.ActorSprite(actor.artId)));
            foreach(var m in art.markers)refs.Add(("marker",m.id,m.sprite));
            foreach(var p in LeisurePresentation.Load().entries)refs.Add(("leisure_prop",p.id,p.Sprite));
            var bindings=refs.Where(r=>r.sprite!=null).Select(r=>
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(r.sprite,out string guid,out long local);
                return new BindingInfo{role=r.role,id=r.id,path=AssetDatabase.GetAssetPath(r.sprite.texture),sprite=r.sprite.name,guid=guid,localId=local,rect=r.sprite.rect,packed=r.sprite.packed};
            }).ToArray();
            var textures=bindings.Select(b=>b.path).Distinct().Select(path=>
            {
                var t=(TextureImporter)AssetImporter.GetAtPath(path);var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                t.GetSourceTextureWidthAndHeight(out var w,out var h);
                return new TextureInfo{path=path,sourceWidth=w,sourceHeight=h,width=tex.width,height=tex.height,maxSize=t.maxTextureSize,
                    format=tex.format.ToString(),filter=t.filterMode.ToString(),mipFilter=t.mipmapFilter.ToString(),mipBias=t.mipMapBias,mipmapCount=tex.mipmapCount,
                    mipmaps=t.mipmapEnabled,alpha=t.alphaIsTransparency,compression=t.textureCompression.ToString(),crunch=t.crunchedCompression,
                    packingTag=t.spritePackingTag,standalone=JsonUtility.ToJson(t.GetPlatformTextureSettings("Standalone")),android=JsonUtility.ToJson(t.GetPlatformTextureSettings("Android"))};
            }).ToArray();
            File.WriteAllText(Path.Combine(output,"native-import-audit.json"),JsonUtility.ToJson(new Audit{utc=DateTime.UtcNow.ToString("O"),target=EditorUserBuildSettings.activeBuildTarget.ToString(),textures=textures,bindings=bindings},true));
            Debug.Log("MAP CLARITY AUDIT: "+bindings.Length+" bindings / "+textures.Length+" textures");
            PlayerExperienceQA.Run();
        }
        [Serializable] class ImportSelection {public string[] paths;}
        // Manual scoped command. The allowlist is the audited map-only selection,
        // not an import postprocessor and not a blanket art-library rewrite.
        public static void Apply()
        {
            var paths=JsonUtility.FromJson<ImportSelection>(File.ReadAllText("Docs/QA/MapClarity-20261019/import-selection.json")).paths;
            foreach(var path in paths)
            {
                if(path.Contains("/Maps/")||!(AssetImporter.GetAtPath(path) is TextureImporter t))throw new Exception("Invalid clarity selection: "+path);
                // Kaiser retains small contour detail better than box mip levels.
                // A quarter-level bias is deliberately modest; keep trilinear
                // interpolation rather than point filtering illustrated sprites.
                t.mipmapEnabled=true;t.mipmapFilter=TextureImporterMipFilter.KaiserFilter;
                t.filterMode=FilterMode.Trilinear;t.mipMapBias=-.25f;
                t.SaveAndReimport();Debug.Log("MAP CLARITY IMPORT: "+path);
            }
            AssetDatabase.SaveAssets();Debug.Log("MAP CLARITY APPLY COMPLETE: "+paths.Length);
        }
    }
}
