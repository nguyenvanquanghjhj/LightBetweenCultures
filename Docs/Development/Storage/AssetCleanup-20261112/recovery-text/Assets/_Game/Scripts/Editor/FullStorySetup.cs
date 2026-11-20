using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Targeted binding/import utility. Does not recreate scenes or existing art.
    public static class FullStorySetup
    {
        [Serializable] sealed class NativeSlices {public string sheet;public Cut[] slices;}
        [Serializable] sealed class Cut {public string name;public int x,y,width,height;public Vector2[] outline;}
        [Serializable] sealed class Presentation {public MapActorDefinition[] mapActors;}
        static TextureImporter Configure(string path,bool multiple)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=multiple?SpriteImportMode.Multiple:SpriteImportMode.Single;
            importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.crunchedCompression=false;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.filterMode=FilterMode.Bilinear;
            importer.SaveAndReimport();return importer;
        }
        static void Add(List<AdventureImage> entries,string id,Sprite sprite,string source,string status)
        {
            var entry=entries.FirstOrDefault(x=>x.id==id);
            if(entry==null){entry=new AdventureImage{id=id};entries.Add(entry);}
            entry.sprite=sprite;entry.source=source;entry.status=status;
        }
        public static void BuildWindows()
        {
            var art=AdventureUIArt.Load();var oldNpc=art.npcs.ToDictionary(x=>x.id,x=>x.sprite);var oldEnemy=art.enemies.ToDictionary(x=>x.id,x=>x.sprite);
            var cities=art.cities.ToList();
            foreach(var city in new[]{"Gyeongju","Seoul"})
            {
                var path="Assets/_Game/Art/Maps/"+city+"/Map_"+city+"_Overview_A.png";Configure(path,false);
                Add(cities,city.ToLowerInvariant(),AssetDatabase.LoadAssetAtPath<Sprite>(path),"LBC_AnhMoi_BanGiaoCodex_20261015/Maps/"+city+"/Map_Overview_A.png","EXACT_APPROVED_MAP_A_PIXELS; original full resolution");
            }
            art.cities=cities.ToArray();
            var config=JsonUtility.FromJson<NativeSlices>(File.ReadAllText("Docs/Story/FullStory-20261017/native-slices.json"));
            var factory=new SpriteDataProviderFactories();factory.Init();var importer=Configure(config.sheet,true);
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var rects=provider.GetSpriteRects().ToList();
            foreach(var cut in config.slices)
            {
                var rect=rects.FirstOrDefault(x=>x.name==cut.name);if(rect==null){rect=new SpriteRect{name=cut.name,spriteID=GUID.Generate()};rects.Add(rect);}
                rect.rect=new Rect(cut.x,cut.y,cut.width,cut.height);rect.pivot=new Vector2(.5f,0);rect.alignment=SpriteAlignment.Custom;
            }
            provider.SetSpriteRects(rects.ToArray());provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(x=>new SpriteNameFileIdPair(x.name,x.spriteID)));
            foreach(var cut in config.slices)provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(rects.First(x=>x.name==cut.name).spriteID,new List<Vector2[]>{cut.outline});
            provider.Apply();importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(config.sheet).OfType<Sprite>().ToDictionary(x=>x.name);var enemies=art.enemies.ToList();
            foreach(var pair in new[]{("enemy_stone_guard","stone_guard"),("enemy_star_core","energy_core"),("enemy_final_core","energy_core"),("enemy_shadow_courier","shadow_courier"),("enemy_fox","shadow_cat")})
                Add(enemies,pair.Item1,sprites[pair.Item2],config.sheet,"TEMPORARY_USER_CONCEPT_NATIVE_MESH; NOT_CANONICAL; "+(pair.Item1=="enemy_fox"?"cat concept pending actual fox sprite":"needs visual art acceptance"));
            foreach(var pair in new[]{("enemy_night_guard","enemy_gate_guard"),("enemy_bridge","enemy_mo01"),("enemy_drone","enemy_glass_thief"),("enemy_signal_bot","enemy_pattern_bot")})
                Add(enemies,pair.Item1,art.Enemy(pair.Item2).sprite,art.Enemy(pair.Item2).source,"TEMPORARY_SHARED_BOT_ART; NOT_CANONICAL_FOR_NEW_ROLE; alias "+pair.Item2);
            art.enemies=enemies.ToArray();
            const string atlas="Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png";
            var npcImporter=(TextureImporter)AssetImporter.GetAtPath(atlas);var npcProvider=factory.GetSpriteEditorDataProviderFromObject(npcImporter);npcProvider.InitSpriteEditorDataProvider();
            var npcRects=npcProvider.GetSpriteRects().ToList();var oldRects=npcRects.ToDictionary(x=>x.name,x=>x.spriteID);
            if(!npcRects.Any(x=>x.name=="npc_min"))npcRects.Add(new SpriteRect{name="npc_min",spriteID=GUID.Generate(),rect=new Rect(14,86,280,414),pivot=new Vector2(.5f,0),alignment=SpriteAlignment.Custom});
            npcProvider.SetSpriteRects(npcRects.ToArray());npcProvider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(npcRects.Select(x=>new SpriteNameFileIdPair(x.name,x.spriteID)));npcProvider.Apply();npcImporter.SaveAndReimport();
            foreach(var old in oldRects)if(npcProvider.GetSpriteRects().First(x=>x.name==old.Key).spriteID!=old.Value)throw new InvalidDataException("Old NPC slice ID changed: "+old.Key);
            var npcs=art.npcs.ToList();var min=AssetDatabase.LoadAllAssetsAtPath(atlas).OfType<Sprite>().First(x=>x.name=="npc_min");
            Add(npcs,"npc_min",min,atlas,"TEMPORARY_ROLE_MATCH_FROM_UNASSIGNED_ARCHAEOLOGICAL_GUIDE; NOT_CANONICAL_IDENTITY_APPROVAL_PENDING");art.npcs=npcs.ToArray();
            art.mapActors=JsonUtility.FromJson<Presentation>(File.ReadAllText("Assets/_Game/Resources/Content/AdventurePresentation.json")).mapActors;
            foreach(var e in oldNpc)if(art.Npc(e.Key).sprite!=e.Value)throw new InvalidDataException("Existing NPC identity changed: "+e.Key);
            foreach(var e in oldEnemy)if(art.Enemy(e.Key).sprite!=e.Value)throw new InvalidDataException("Approved enemy art changed: "+e.Key);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();ArtLibrarySetup.Export();
            SliceValidation.BuildWindows();
        }
    }
}
