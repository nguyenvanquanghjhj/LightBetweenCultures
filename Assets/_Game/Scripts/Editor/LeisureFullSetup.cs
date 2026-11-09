using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class LeisureFullSetup
    {
        public static void BindAndBuild()
        {
            var map=new[]{("release","HoiAn_LanternRelease_BG"),("seat","Hue_Throne_BG"),("pick","DaLat_Strawberry_BG"),("fly","DaLat_Kite_BG"),("water","Jeonju_Watering_BG"),("tea","Jeonju_Tea_B"),("bell","Gyeongju_Bell_BG"),("wish","Gyeongju_Wishes_"),("snack","Seoul_Tteokbokki_BG"),("dance","Seoul_Kpop_BG")};
            var art=AdventureUIArt.Load();
            art.leisureBackgrounds=map.Select(pair=>
            {
                var entry=LeisurePresentation.Load().entries.Single(e=>e.mechanic==pair.Item1);var path="Assets/_Game/Art/Leisure/Backgrounds/"+pair.Item2+".png";
                if(!File.Exists(path)||entry.Sprite==null)throw new FileNotFoundException("Existing approved leisure asset missing",path);
                var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureCompression=TextureImporterCompression.Uncompressed;t.crunchedCompression=false;t.maxTextureSize=2048;t.filterMode=FilterMode.Bilinear;t.mipmapEnabled=false;t.SaveAndReimport();
                var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s=>s.rect.width==1448&&s.rect.height==1086);
                return new AdventureImage{id=entry.id,sprite=sprite,source=path,status="RUNTIME_BACKDROP_BOUND_EXISTING_PIXELS",description="Existing backdrop only; prop and Lumi are runtime layers."};
            }).ToArray();
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();Phase5Setup.Build();
        }
    }
}
