using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class LeisurePilotSetup
    {
        static Sprite Backdrop(string name)
        {
            var path="Assets/_Game/Art/Leisure/Backgrounds/"+name+".png";
            if(!File.Exists(path))throw new FileNotFoundException("Approved pilot backdrop missing",path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.crunchedCompression=false;
            importer.maxTextureSize=2048;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;
            importer.SaveAndReimport(); // Preserve GUID, Multiple mode, existing full-frame localID and pixels.
            var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s=>s.rect.width==1448&&s.rect.height==1086);
            Debug.Log("PILOT BACKDROP "+path+" existing full-frame sprite "+sprite.name);return sprite;
        }
        public static void BindAndBuild()
        {
            var art=AdventureUIArt.Load();art.leisureSwingBackground=Backdrop("HoiAn_Swing_BG");art.leisureBoatBackground=Backdrop("Hue_PedalBoat_BG");
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();Phase5Setup.Build();
        }
    }
}
