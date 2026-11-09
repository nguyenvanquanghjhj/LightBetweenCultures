using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class CheckInArtSetup
    {
        [Serializable] sealed class Entry
        {
            public string cityId,nodeId,checkInId,source,assetPath,description,preparedPath;
            public float[] uv;
            public string guid,localId,bindingStatus;
        }
        [Serializable] sealed class Registry {public Entry[] entries;}
        public static void Import()
        {
            var path="Docs/Gameplay/CheckInArt-20261018/source-inventory.json";
            var registry=JsonUtility.FromJson<Registry>(File.ReadAllText(path));
            if(registry.entries.Length!=18||registry.entries.Select(e=>e.checkInId).Distinct().Count()!=18)throw new InvalidDataException("Expected18 unique existing check-ins");
            var art=AdventureUIArt.Load();var images=new System.Collections.Generic.List<CheckInImage>();
            const string previous="Assets/_Game/Art/CheckInLandmarks/20261018_ApprovedSet";
            const string derived="Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived";
            if(AssetDatabase.IsValidFolder(previous)&&!AssetDatabase.IsValidFolder(derived))
            {var error=AssetDatabase.MoveAsset(previous,derived);if(!string.IsNullOrEmpty(error))throw new InvalidDataException(error);}
            var old=art.checkIns.ToDictionary(e=>e.checkInId,e=>AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(e.sprite)));
            foreach(var e in registry.entries)
            {
                // Derived pixels only; source PNG and the existing GUID/meta are retained.
                if(!File.Exists(e.preparedPath)||!File.Exists(e.assetPath))throw new InvalidDataException("Missing reviewed derived target: "+e.assetPath);
                File.Copy(e.preparedPath,e.assetPath,true);
                AssetDatabase.ImportAsset(e.assetPath,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(e.assetPath);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.spritePixelsPerUnit=100;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.sRGBTexture=true;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=1024;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.crunchedCompression=false;
                var android=importer.GetPlatformTextureSettings("Android");android.overridden=true;android.maxTextureSize=1024;
                android.format=TextureImporterFormat.ASTC_6x6;android.textureCompression=TextureImporterCompression.CompressedHQ;android.compressionQuality=100;android.crunchedCompression=false;
                importer.SetPlatformTextureSettings(android);importer.SaveAndReimport();
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(e.assetPath);if(sprite==null)throw new InvalidDataException(e.assetPath);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long local);e.guid=guid;e.localId=local.ToString();
                if(old.TryGetValue(e.checkInId,out var original)&&original!=guid)throw new InvalidDataException("Existing image GUID changed: "+e.checkInId);
                e.bindingStatus="RUNTIME_ART_BOUND; RUNTIME_VARIANT_PENDING_VISUAL_REVIEW; H6_GESTURE_UNCHANGED";
                images.Add(new CheckInImage{checkInId=e.checkInId,source=e.source,sprite=sprite,description=e.description,status=e.bindingStatus,viewUV=new Rect(e.uv[0],e.uv[1],e.uv[2],e.uv[3])});
            }
            // Only the new check-in array is changed; do not touch any canonical identity binding.
            art.checkIns=images.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            File.WriteAllText("Docs/Gameplay/CheckInArt-20261018/native-checkin-art.json",JsonUtility.ToJson(registry,true));
            Debug.Log("CHECKIN ART IMPORT PASS:18 reviewed derived sprites, source/GUIDs preserved;H6 gallery uses source-approved postcard, alignment unchanged.");
        }
    }
}
