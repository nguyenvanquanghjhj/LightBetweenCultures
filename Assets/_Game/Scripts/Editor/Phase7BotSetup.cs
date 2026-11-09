using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Explicit approved alpha repair; preserves originals, IDs, actor scale and anchors.
    public static class Phase7BotSetup
    {
        [Serializable] sealed class Plan {public Entry[] entries;}
        [Serializable] sealed class Entry {public string spriteName,source,preview,destination;public Affected[] affected;}
        [Serializable] sealed class Affected {public string city,node,artId,oldStatus;}
        public static void BindAndBuild()
        {
            var plan=JsonUtility.FromJson<Plan>("{\"entries\":"+File.ReadAllText("Docs/QA/Phase7/bot-alpha-plan.json")+"}");
            if(plan.entries.Length!=4)throw new InvalidDataException("Expected four reviewed source-preserving cutouts");
            var art=AssetDatabase.LoadAssetAtPath<AdventureUIArt>("Assets/_Game/Resources/AdventureUIArt.asset");
            foreach(var e in plan.entries)
            {
                if(!e.source.EndsWith("/Enemies_Roster_Concept.png"))throw new InvalidDataException("Unexpected source sheet");
                Directory.CreateDirectory(Path.GetDirectoryName(e.destination));
                if(File.Exists(e.destination)){if(!File.ReadAllBytes(e.destination).SequenceEqual(File.ReadAllBytes(e.preview)))throw new IOException("Refuse overwrite of different art: "+e.destination);}else File.Copy(e.preview,e.destination);
                AssetDatabase.ImportAsset(e.destination,ImportAssetOptions.ForceSynchronousImport);
                var t=(TextureImporter)AssetImporter.GetAtPath(e.destination);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=SpriteImportMode.Single;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.filterMode=FilterMode.Bilinear;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=512;t.wrapMode=TextureWrapMode.Clamp;t.spritePixelsPerUnit=100;t.SaveAndReimport();
                var clean=AssetDatabase.LoadAssetAtPath<Sprite>(e.destination);if(clean==null)throw new InvalidDataException("Missing imported cutout "+e.spriteName);
                foreach(var id in e.affected.Select(a=>a.artId).Distinct())
                {
                    var entry=art.Enemy(id);if(entry==null||!new[]{e.source,e.destination}.Contains(AssetDatabase.GetAssetPath(entry.sprite)))throw new InvalidDataException("Enemy binding changed concurrently: "+id);
                    var original=AssetDatabase.LoadAllAssetsAtPath(e.source).OfType<Sprite>().Single(s=>s.name==e.spriteName);
                    // Owner clarification: the defect is the dialogue image. Restore
                    // only our interim map binding; map/combat retain the prior sprite.
                    entry.sprite=original;entry.portrait=clean;entry.status=e.affected.First(a=>a.artId==id).oldStatus+"; PHASE7_DIALOGUE_ALPHA_CLEAN; map/combat sprite preserved";
                }
            }
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Phase7Audit.AuditAndAndroid(); // Environment must be LBC_PHASE7_ANDROID=0.
            Phase5Setup.Build();
        }
    }
}
