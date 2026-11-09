using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Explicit audit/fix command, not a global import postprocessor. Sprite IDs/crops stay intact.
    public static class RenderQualitySetup
    {
        [Serializable] class Entry
        {public string path,format,filter,compression;public int sourceWidth,sourceHeight,width,height,max;public bool mipmaps,alpha;public string standalone,android;}
        [Serializable] class Report {public Entry[] textures;public string[] changed;}
        static void Run(bool improve)
        {
            var roots=AssetDatabase.FindAssets("",new[]{"Assets/_Game/Resources","Assets/_Game/Scenes"}).Select(AssetDatabase.GUIDToAssetPath).Where(File.Exists).ToArray();
            var paths=AssetDatabase.GetDependencies(roots,true).Distinct().Where(p=>AssetImporter.GetAtPath(p) is TextureImporter).OrderBy(p=>p).ToArray();
            var changed=new System.Collections.Generic.List<string>();
            if(improve&&File.Exists("Docs/RenderQuality-20261017/native-after.json"))changed.AddRange(JsonUtility.FromJson<Report>(File.ReadAllText("Docs/RenderQuality-20261017/native-after.json")).changed);
            var entries=paths.Select(p=>
            {
                var t=(TextureImporter)AssetImporter.GetAtPath(p);
                // Only actively referenced illustrated sprite sheets suffering lossy defaults.
                if(improve&&t.textureType==TextureImporterType.Sprite&&t.textureCompression!=TextureImporterCompression.Uncompressed)
                {t.textureCompression=TextureImporterCompression.Uncompressed;t.crunchedCompression=false;t.SaveAndReimport();changed.Add(p);}
                // These isolated cutouts shrink by 8-20x in portraits/bag/map.
                // Match the existing mip-filtered Hue/Jeonju actors. Do not mip
                // tightly packed UI/item atlases: lower levels can bleed neighbours.
                if(improve&&(p.Contains("/Characters/Lumi/")||p.Contains("/Characters/NPC/HoiAn/")||
                    p.Contains("/Items/")&&t.spriteImportMode==SpriteImportMode.Single)&&
                    (!t.mipmapEnabled||t.filterMode!=FilterMode.Trilinear))
                {t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.SaveAndReimport();if(!changed.Contains(p))changed.Add(p);}
                var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(p);t.GetSourceTextureWidthAndHeight(out var w,out var h);
                return new Entry{path=p,sourceWidth=w,sourceHeight=h,width=tex.width,height=tex.height,max=t.maxTextureSize,format=tex.format.ToString(),filter=t.filterMode.ToString(),compression=t.textureCompression.ToString(),mipmaps=t.mipmapEnabled,alpha=t.alphaIsTransparency,standalone=JsonUtility.ToJson(t.GetPlatformTextureSettings("Standalone")),android=JsonUtility.ToJson(t.GetPlatformTextureSettings("Android"))};
            }).ToArray();
            Directory.CreateDirectory("Docs/RenderQuality-20261017");File.WriteAllText("Docs/RenderQuality-20261017/native-"+(improve?"after":"before")+".json",JsonUtility.ToJson(new Report{textures=entries,changed=changed.ToArray()},true));
            AssetDatabase.SaveAssets();
        }
        public static void AuditAndBuild(){Run(false);SliceValidation.BuildWindows();}
        public static void ImproveAndBuild(){Run(true);SliceValidation.BuildWindows();}
    }
}
