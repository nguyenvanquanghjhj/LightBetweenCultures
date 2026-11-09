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
        static string Hash(string path)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
        static void Folder(string path)
        {
            path=path.Replace('\\','/');if(AssetDatabase.IsValidFolder(path))return;
            var parent=path.Substring(0,path.LastIndexOf('/'));Folder(parent);
            AssetDatabase.CreateFolder(parent,path.Substring(path.LastIndexOf('/')+1));
        }
        static Dictionary<string,string> References()
        {
            var result=new Dictionary<string,string>();
            foreach(var path in new[]{"Assets/_Game/Resources/DesignKitArt.asset","Assets/_Game/Resources/AdventureUIArt.asset"})
            {
                var asset=AssetDatabase.LoadMainAssetAtPath(path);
                if(asset==null)throw new InvalidDataException("Missing registry "+path);
                var fields=new SerializedObject(asset).GetIterator();
                while(fields.Next(true))if(fields.propertyType==SerializedPropertyType.ObjectReference)
                {
                    var reference=fields.objectReferenceValue;
                    if(reference==null){result[path+":"+fields.propertyPath]="null";continue;}
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(reference,out string guid,out long local);
                    result[path+":"+fields.propertyPath]=guid+":"+local;
                }
            }
            return result;
        }
        static Dictionary<string,string> ProtectedFiles()=>new[]{"Scenes","Prefabs","Resources/Content"}
            .SelectMany(p=>Directory.GetFiles("Assets/_Game/"+p,"*",SearchOption.AllDirectories))
            .ToDictionary(p=>p,p=>Hash(p));
        static void Validate(Dictionary<string,string> refs,Dictionary<string,string> files)
        {
            var actual=References();
            if(actual.Count!=refs.Count||refs.Any(p=>!actual.ContainsKey(p.Key)||actual[p.Key]!=p.Value))
                throw new InvalidDataException("Registry GUID/local sprite reference changed during migration");
            foreach(var file in files)if(Hash(file.Key)!=file.Value)throw new InvalidDataException("Protected file changed "+file.Key);
        }
        [MenuItem("Tools/Light Between Cultures/Organize Art Library")]
        public static void Apply()
        {
            var refs=References();var files=ProtectedFiles();var log=new List<string>();
            Directory.CreateDirectory("Docs/Art");
            foreach(var group in new[]{"Characters","Maps","Items","UI","Markers","Environment","SourceSheets"})
            {
                foreach(var entry in Moves.entries.Where(e=>e.group==group))
                {
                    if(!entry.oldPath.StartsWith("Assets/_Game/Art/")||!entry.newPath.StartsWith("Assets/_Game/Art/"))throw new InvalidDataException("Move outside Art");
                    var path=AssetDatabase.GUIDToAssetPath(entry.guid);
                    if(path!=entry.oldPath&&path!=entry.newPath)throw new InvalidDataException("Unexpected current GUID path "+entry.guid+": "+path);
                    if(Hash(path)!=entry.sha256)throw new InvalidDataException("Source artwork changed "+path);
                    if(path==entry.oldPath)
                    {
                        Folder(Path.GetDirectoryName(entry.newPath));
                        var error=AssetDatabase.MoveAsset(path,entry.newPath);
                        if(!string.IsNullOrEmpty(error))throw new InvalidDataException(error);
                    }
                    if(AssetDatabase.AssetPathToGUID(entry.newPath)!=entry.guid||Hash(entry.newPath)!=entry.sha256)throw new InvalidDataException("Move verification failed "+entry.newPath);
                    log.Add("PASS "+group+" | "+entry.oldPath+" -> "+entry.newPath+" | "+entry.guid);
                }
                AssetDatabase.SaveAssets();Validate(refs,files);
                log.Add("PASS group "+group+": all registry GUID/local IDs and protected scene/prefab/content files unchanged");
                File.WriteAllLines("Docs/Art/migration-results.txt",log);
            }
            foreach(var city in new[]{"HoiAn","Hue","DaLat","Jeonju","Gyeongju","Seoul"})Folder("Assets/_Game/Art/Characters/NPC/"+city);
            foreach(var p in new[]{"Maps/World","Environment/HoiAn","Environment/OtherCities","Items/Collectibles","UI/Buttons"})Folder("Assets/_Game/Art/"+p);
            // Empty old directory removal only; every image has already been verified.
            foreach(var path in new[]{"Assets/_Game/Art/Runtime","Assets/_Game/Art/DesignKit","Assets/_Game/Art/Item","Assets/_Game/Art/Generated"})
                RemoveEmpty(path);
            AssetDatabase.SaveAssets();Export();Validate(refs,files);
            Debug.Log("Art library: "+Moves.entries.Length+" GUID-preserving image moves; all protected references verified.");
        }
        static void RemoveEmpty(string path)
        {
            if(!path.StartsWith("Assets/_Game/Art/")||!Directory.Exists(path))return;
            foreach(var child in Directory.GetDirectories(path))RemoveEmpty(child.Replace('\\','/'));
            if(Directory.GetFileSystemEntries(path).Length==0)AssetDatabase.DeleteAsset(path);
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
        public static void BuildWindows(){Apply();SliceValidation.BuildWindows();}
    }
}
