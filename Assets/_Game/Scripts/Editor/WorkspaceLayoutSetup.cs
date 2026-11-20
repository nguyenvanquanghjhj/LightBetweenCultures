using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class WorkspaceLayoutSetup
    {
        [Serializable] private sealed class OriginalAsset { public string path, guid; public string[] sprites; }
        [Serializable] private sealed class OriginalAudit { public OriginalAsset[] assets; }
        [Serializable] private sealed class Fingerprint { public string path, sha256; }
        [Serializable] private sealed class Fingerprints { public Fingerprint[] files; }

        public static void Verify()
        {
            AssetCleanupAudit.VerifyLayout();
            var outputArg=Environment.GetCommandLineArgs().Single(a=>a.StartsWith("--asset-audit-output="));
            var output=outputArg.Substring("--asset-audit-output=".Length);
            var before=JsonUtility.FromJson<OriginalAudit>(File.ReadAllText("Docs/Development/Storage/WorkspaceRestructure-20261113/native-before/native-audit.json"));
            var catalog=GameArtCatalog.Load();
            if(catalog==null||Directory.Exists("Assets/_Game/Resources/Art"))throw new InvalidDataException("Canonical art catalog/layout missing");
            var images=before.assets.Where(a=>a.path.StartsWith("Assets/_Game/Resources/Art/")&&a.path.EndsWith(".png")).ToArray();
            if(images.Length!=14||catalog.leisureProps.Length!=12)throw new InvalidDataException("Expected fourteen original images and twelve props");
            var assertions=0;
            foreach(var image in images)
            {
                UnityEngine.Object current=image.path.EndsWith("RescueCats.png")?catalog.rescueCats
                    :image.path.EndsWith("DaLatStation.png")?catalog.dalatStation
                    :catalog.Leisure(Path.GetFileNameWithoutExtension(image.path));
                if(current==null)throw new InvalidDataException("Missing original art: "+image.path);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(current,out string guid,out long local);
                if(guid!=image.guid)throw new InvalidDataException("Changed original GUID: "+image.path);
                if(current is Sprite && !image.sprites.Any(s=>s.Split('|')[2]==local.ToString()))throw new InvalidDataException("Changed sprite localID: "+image.path);
                assertions++;
            }
            foreach(var entry in LeisurePresentation.Load().entries)
            {
                if(entry.Sprite==null||entry.Sprite!=catalog.Leisure(entry.spritePath))throw new InvalidDataException("Bad JSON prop ID: "+entry.spritePath);
                assertions++;
            }
            for(var i=0;i<6;i++)
            {
                var sprite=CatRescueArt.Cat(i);var t=catalog.rescueCats;
                var expected=new Rect(i%3*t.width/3f,(1-i/3)*t.height/2f,t.width/3f,t.height/2f);
                if(sprite.texture!=t||sprite.rect!=expected)throw new InvalidDataException("Cat atlas slice changed: "+i);
                assertions++;
            }
            if(CatRescueArt.Image("station")!=catalog.dalatStation||CatRescueArt.Image("leisure:Hue_PedalSwanBoat")!=catalog.Leisure("Hue_PedalSwanBoat"))throw new InvalidDataException("Cat board selection changed");
            assertions+=2;
            File.WriteAllText(Path.Combine(output,"catalog-result.json"),"{\"status\":\"PASS\",\"assertions\":"+assertions+",\"meaning\":\"Native catalog/GUID/localID/JSON and atlas slice checks; not Player UI acceptance\"}");
            var files=Directory.GetFiles("Assets/_Game","*",SearchOption.AllDirectories)
                .Concat(new[]{"ProjectSettings/EditorBuildSettings.asset","ProjectSettings/GraphicsSettings.asset","ProjectSettings/QualitySettings.asset","ProjectSettings/ProjectSettings.asset","Packages/manifest.json","Packages/packages-lock.json"});
            using(var sha=SHA256.Create())
                File.WriteAllText(Path.Combine(output,"input-fingerprints.json"),JsonUtility.ToJson(new Fingerprints{files=files.Select(p=>new Fingerprint{path=p.Replace('\\','/'),sha256=BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p))).Replace("-","").ToLowerInvariant()}).ToArray()},true));
            Debug.Log("WORKSPACE CATALOG REGRESSION PASS: "+assertions+" assertions; original GUIDs, native sprite IDs and cat slicing preserved");
        }

        // Run once before the reviewed asset move. Never changes importers or PNG pixels.
        public static void PrepareArtCatalog()
        {
            const string path = "Assets/_Game/Resources/Catalogs/GameArtCatalog.asset";
            if (File.Exists(path)) throw new IOException("Existing catalog is protected: " + path);
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Resources/Catalogs"))
                AssetDatabase.CreateFolder("Assets/_Game/Resources", "Catalogs");
            var catalog = ScriptableObject.CreateInstance<GameArtCatalog>();
            catalog.rescueCats = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_Game/Art/Characters/Cats/RescueCats.png");
            catalog.dalatStation = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Maps/DaLat/DaLatStation.png");
            catalog.leisureProps = Directory.GetFiles("Assets/_Game/Art/Leisure/Props", "*.png")
                .OrderBy(p => p).Select(p => new GameArtCatalog.Image {
                    id = Path.GetFileNameWithoutExtension(p), sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p)
                }).ToArray();
            if (catalog.rescueCats == null || catalog.dalatStation == null || catalog.leisureProps.Length != 12 || catalog.leisureProps.Any(e => e.sprite == null))
                throw new InvalidDataException("Expected all 14 original resource images");
            AssetDatabase.CreateAsset(catalog, path);
            AssetDatabase.SaveAssets();
            Debug.Log("WORKSPACE ART CATALOG PREPARED: 14 original native references");
        }
    }
}
