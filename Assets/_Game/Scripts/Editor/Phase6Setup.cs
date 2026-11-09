using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Explicit, bounded repair. No source PNG writes, re-slicing, binding replacement or scene generation.
    public static class Phase6Setup
    {
        [Serializable] sealed class OutlinePlan {public OutlineEntry[] entries;}
        [Serializable] sealed class SourceRect {public float x,y,width,height;}
        [Serializable] sealed class OutlineEntry {public string id,path;public SourceRect rect;public Vector2[] points;}
        public static void ApplyVisualFixesAndBuild()
        {
            var plan=JsonUtility.FromJson<OutlinePlan>(File.ReadAllText("Docs/QA/Phase6-VisualAndroid/crop-outline-plan.json"));
            if(plan.entries.Length!=4)throw new InvalidDataException("Expected exactly4 reviewed fragment outlines");
            foreach(var group in plan.entries.GroupBy(e=>e.path))
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath(group.Key);
                var refs=AssetDatabase.LoadAllAssetsAtPath(group.Key).OfType<Sprite>().ToDictionary(s=>s.name,s=>Reference(s));
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
                var rects=provider.GetSpriteRects();
                foreach(var entry in group)
                {
                    var sr=rects.Single(s=>s.name==entry.id);
                    var expected=new Rect(entry.rect.x,entry.rect.y,entry.rect.width,entry.rect.height);
                    if(sr.rect!=expected)throw new InvalidDataException("Unexpected source rectangle "+entry.id+": "+sr.rect+" expected "+expected);
                    provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(sr.spriteID,new List<Vector2[]>{entry.points});
                }
                provider.Apply();importer.SaveAndReimport();
                foreach(var s in AssetDatabase.LoadAllAssetsAtPath(group.Key).OfType<Sprite>())
                    if(refs[s.name]!=Reference(s))throw new InvalidDataException("Sprite reference changed: "+s.name);
            }
            foreach(var city in new[]{"Gyeongju","Seoul"})
            {
                var path="Assets/_Game/Art/Maps/"+city+"/Map_"+city+"_Overview_A.png";
                var before=Reference(AssetDatabase.LoadAssetAtPath<Sprite>(path));var t=(TextureImporter)AssetImporter.GetAtPath(path);
                if(t.spriteImportMode!=SpriteImportMode.Single)throw new InvalidDataException("Not an isolated full map: "+path);
                // Reviewed severe minification on WorldMap cards; no tightly packed sheet mipmap bleed.
                t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.SaveAndReimport();
                if(Reference(AssetDatabase.LoadAssetAtPath<Sprite>(path))!=before)throw new InvalidDataException("Map identity changed");
            }
            Phase5Setup.Build();
        }
        static string Reference(Sprite s){AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s,out string guid,out long id);return guid+":"+id;}
    }
}
