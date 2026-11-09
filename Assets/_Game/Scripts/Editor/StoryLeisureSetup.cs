using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class StoryLeisureSetup
    {
        public static void ImportAndBuild()
        {
            var files=Directory.GetFiles("Assets/_Game/Resources/LeisureProps","*.png");
            if(files.Length!=12)throw new InvalidDataException("Expected exactly12 approved props");
            foreach(var path in files)
            {
                var source="ReviewPacks/10Story12Leisure-20261019/Sprites_12/"+Path.GetFileName(path);
                if(!File.ReadAllBytes(path).SequenceEqual(File.ReadAllBytes(source)))throw new InvalidDataException("Source pixels changed: "+path);
                var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;
                t.alphaIsTransparency=true;t.spritePixelsPerUnit=100;
                var settings=new TextureImporterSettings();t.ReadTextureSettings(settings);settings.spritePivot=new Vector2(.5f,0);settings.spriteAlignment=(int)SpriteAlignment.Custom;t.SetTextureSettings(settings);
                t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.maxTextureSize=1024;t.textureCompression=TextureImporterCompression.Uncompressed;t.crunchedCompression=false;t.SaveAndReimport();
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long id);
                Debug.Log("APPROVED LEISURE SPRITE "+path+" "+guid+":"+id+" "+sprite.rect);
            }
            var art=AdventureUIArt.Load();
            art.activityGround=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Environment/HoiAn/H3/H3_StoneGround_Continuous_v1.png");
            art.activityWall=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Environment/HoiAn/H3/H3_Garden_Wall.asset");
            art.activityPinkPot=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Environment/HoiAn/H3/H3_Pink_Pot.asset");
            art.activityCitrusPot=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Environment/HoiAn/H3/H3_Citrus_Pot.asset");
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Phase5Setup.Build();
        }
    }
}
