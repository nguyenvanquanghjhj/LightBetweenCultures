using System.IO;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class HoiAnMapSetup
    {
        public const string SpritePath = "Assets/_Game/Art/Maps/HoiAn/Map_HoiAn_Overview_A.png";
        [MenuItem("Tools/Light Between Cultures/Import Hoi An Illustrated Map")]
        public static void Apply()
        {
            const string source = "LBC_AnhMoi_BanGiaoCodex_20261015/Maps/HoiAn/Map_Overview_A.png";
            Directory.CreateDirectory(Path.GetDirectoryName(SpritePath));
            if (!File.Exists(SpritePath) || System.Convert.ToBase64String(File.ReadAllBytes(source)) != System.Convert.ToBase64String(File.ReadAllBytes(SpritePath)))
                File.Copy(source,SpritePath,true);
            AssetDatabase.Refresh();
            var importer = (TextureImporter)AssetImporter.GetAtPath(SpritePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.filterMode = FilterMode.Bilinear;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var art = AssetDatabase.LoadAssetAtPath<DesignKitArt>("Assets/_Game/Resources/DesignKitArt.asset");
            art.hoiAnMap = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets();
            Debug.Log("HoiAn Map A imported intact and linked to the existing CityMap; no source art or H3 scene changed.");
        }
    }
}
