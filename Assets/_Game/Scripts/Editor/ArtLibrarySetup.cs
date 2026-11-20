using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Editor migration and audit only. Runtime keeps the existing art registries.
    public static class ArtLibrarySetup
    {
        const string Manifest="Docs/Art/asset-moves.json";
        [Serializable] public sealed class Move
        {public string oldPath,newPath,group,guid,sha256;}
        [Serializable] public sealed class Plan {public Move[] entries;}
        [Serializable] public sealed class Binding
        {public string role,id,path,guid,localId,spriteName,source,status;public bool canonical;}
        [Serializable] public sealed class Bindings {public Binding[] entries;}
        static Plan cached;
        static Plan Moves=>cached??(cached=JsonUtility.FromJson<Plan>(File.ReadAllText(Manifest)));
        public static string ResolveMovedPath(string path)
        {
            if(!File.Exists(Manifest))return path;
            var entry=Moves.entries.FirstOrDefault(e=>e.oldPath==path);
            if(entry==null)return path;
            var current=AssetDatabase.GUIDToAssetPath(entry.guid);
            return string.IsNullOrEmpty(current)?path:current;
        }
        static Binding Bind(string role,string id,Sprite sprite,string source="",string status="",bool canonical=true)
        {
            if(sprite==null)throw new InvalidDataException("Missing sprite: "+role+" "+id);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long local);
            return new Binding{role=role,id=id,path=AssetDatabase.GetAssetPath(sprite),guid=guid,localId=local.ToString(),spriteName=sprite.name,source=source,status=status,canonical=canonical&&!status.Contains("NOT_CANONICAL")};
        }
        public static void Export()
        {
            var rows=new List<Binding>();var art=AdventureUIArt.Load();var kit=DesignKitArt.Load();
            foreach(var group in new[]{("item",art.items),("npc",art.npcs),("enemy",art.enemies),("city_preview",art.cities),("marker",art.markers),("relic",art.relics),("relic_fragment",art.fragments)})
                foreach(var entry in group.Item2)rows.Add(Bind(group.Item1,entry.id,entry.sprite,entry.source,entry.status,!entry.categoryIcon&&group.Item1!="relic"&&group.Item1!="relic_fragment"));
            foreach(var entry in art.checkIns)rows.Add(Bind("checkin",entry.checkInId,entry.sprite,entry.source,entry.status,false));
            rows.Add(Bind("lumi","map",kit.lumiMapTraveler));rows.Add(Bind("lumi","dialogue",kit.lumiPortrait));rows.Add(Bind("lumi","exploration",kit.lumiExploration));
            rows.Add(Bind("canonical_npc","mai",kit.maiPortrait));rows.Add(Bind("city_main","hoi_an",kit.hoiAnMap));
            // H3 crops are native Sprite assets referencing the supplied PNGs;
            // register only scene-used crops, never duplicate source textures.
            var h3Dependencies=new HashSet<string>(AssetDatabase.GetDependencies("Assets/_Game/Scenes/HoiAnDemo.unity",true));
            foreach(var folder in new[]{"Assets/_Game/Art/Environment/HoiAn/H3","Assets/_Game/Art/UI/H3"})
                if(AssetDatabase.IsValidFolder(folder))
                    foreach(var path in AssetDatabase.FindAssets("t:Sprite",new[]{folder}).Select(AssetDatabase.GUIDToAssetPath).Distinct())
                        if(h3Dependencies.Contains(path)&&AssetDatabase.LoadAssetAtPath<Sprite>(path) is Sprite sprite)
                            rows.Add(Bind(folder.Contains("/UI/")?"h3_ui":"h3_environment",sprite.name,sprite,
                                AssetDatabase.GetAssetPath(sprite.texture),path.EndsWith(".png")?"AI_DERIVED_GROUND_ONLY; runtime; visual_acceptance_pending":"USER_SOURCE_NATIVE_CROP; runtime; visual_acceptance_pending",false));
            foreach(var field in typeof(DesignKitArt).GetFields().Where(f=>f.FieldType==typeof(Sprite)))
                if(field.GetValue(kit) is Sprite sprite)rows.Add(Bind("designkit_serialized_reference",field.Name,sprite,"","retained shared/legacy registry reference; active roles listed separately",false));
            foreach(var root in new[]{"Assets/_Game/Scenes","Assets/_Game/Prefabs"}.SelectMany(p=>Directory.GetFiles(p,"*",SearchOption.AllDirectories)).Where(p=>p.EndsWith(".unity")||p.EndsWith(".prefab")))
                foreach(var dependency in AssetDatabase.GetDependencies(root,true).Where(p=>p.StartsWith("Assets/_Game/Art/")))
                    rows.Add(new Binding{role="scene_prefab_dependency",id=root,path=dependency,guid=AssetDatabase.AssetPathToGUID(dependency),localId="texture_dependency",status="serialized dependency; does not imply currently visible"});
            foreach(var path in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/_Game/Art"}).Select(AssetDatabase.GUIDToAssetPath))
                foreach(var sprite in AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>())rows.Add(Bind("native_sprite",sprite.name,sprite,"","registry binding determines runtime usage",false));
            File.WriteAllText("Docs/Art/native-art-bindings.json",JsonUtility.ToJson(new Bindings{entries=rows.ToArray()},true));
        }
    }
}
