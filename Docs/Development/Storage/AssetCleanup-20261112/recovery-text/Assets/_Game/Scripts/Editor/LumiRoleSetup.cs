using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering;
using LightBetweenCultures.HoiAn;

namespace LightBetweenCultures.Editor
{
    /// <summary>Assigns the single white Lumi design approved on 2026-10-21.</summary>
    public static class LumiRoleSetup
    {
        private const string Cutout = "Assets/_Game/Art/_SourceSheets/Lumi/Lumi_Character_Cutout_Concept.png";
        private const string PortraitTexture = "Assets/_Game/Art/Characters/Lumi/Lumi_DialoguePortrait.png";
        private const string PortraitName = "Lumi_DialoguePortrait";
        private const string ArtAsset = "Assets/_Game/Resources/DesignKitArt.asset";
        private const string ExplorationPrefab = "Assets/_Game/Prefabs/DesignKit/LumiExplorationSprite.prefab";
        private const string HoiAnScene = "Assets/_Game/Scenes/HoiAnDemo.unity";

        [MenuItem("Tools/Light Between Cultures/Apply Canonical White Lumi")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var art = AssetDatabase.LoadAssetAtPath<DesignKitArt>(ArtAsset);
            if (art == null) throw new InvalidDataException("Apply the provided Design Kit before assigning Lumi roles.");
            ConfigureArt(art);
            var previous = EditorSceneManager.GetSceneManagerSetup();
            var backup = Path.GetFullPath("../Backups/LumiRoleSceneUpdates");
            Directory.CreateDirectory(backup);
            File.Copy(HoiAnScene, Path.Combine(backup, "HoiAnDemo-" + DateTime.Now.ToString("yyyyMMdd-HHmmssfff") + ".unity"));
            var scene = EditorSceneManager.OpenScene(HoiAnScene);
            var player = UnityEngine.Object.FindAnyObjectByType<HoiAnPlayerController>();
            if (player == null) throw new InvalidDataException("H3 player is missing.");
            ApplyPlayerVisual(player);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            if (previous.Length > 0 && Array.TrueForAll(previous, entry => !string.IsNullOrEmpty(entry.path)))
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            else EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype.unity");
            Debug.Log("Lumi roles applied: chibi City traveler, full hand-preserving dialogue, standing H3 chibi. Player physics preserved.");
        }

        public static void ConfigureArt(DesignKitArt art)
        {
            // Do not overwrite art or reintroduce the retired navy outfit.
            var canonical=AssetDatabase.LoadAllAssetsAtPath("Assets/_Game/Art/Characters/Lumi/Lumi_MapTraveler.png").OfType<Sprite>().Single(s=>s.name=="Lumi_MapTraveler");
            art.lumiPortrait=art.lumiExploration=art.lumiMapTraveler=art.lumiCity=art.lumiCitySmall=canonical;
            EditorUtility.SetDirty(art);

            Directory.CreateDirectory(Path.GetDirectoryName(ExplorationPrefab));
            var visual = new GameObject("LumiExplorationSprite", typeof(SpriteRenderer));
            visual.GetComponent<SpriteRenderer>().sprite = art.lumiExploration;
            PrefabUtility.SaveAsPrefabAsset(visual, ExplorationPrefab);
            PrefabUtility.SaveAsPrefabAsset(visual,"Assets/_Game/Prefabs/DesignKit/LumiMapToken.prefab");
            UnityEngine.Object.DestroyImmediate(visual);
            AssetDatabase.SaveAssets();
        }

        private static Sprite ImportSprite(string path,string spriteName,bool feetPivot)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Missing role sprite",path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=256;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true;
            importer.filterMode=FilterMode.Bilinear;
            importer.wrapMode=TextureWrapMode.Clamp;
            importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=2048;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType=SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            // Trim only transparent padding through sprite metadata. Original PNG pixels stay intact.
            var texture=new Texture2D(2,2); texture.LoadImage(File.ReadAllBytes(path));
            var pixels=texture.GetPixels32(); int left=texture.width, bottom=texture.height, right=0, top=0;
            for(int y=0;y<texture.height;y++) for(int x=0;x<texture.width;x++)
                if(pixels[y*texture.width+x].a>16) { left=Math.Min(left,x); right=Math.Max(right,x); bottom=Math.Min(bottom,y); top=Math.Max(top,y); }
            var rect=new Rect(Math.Max(0,left-2),Math.Max(0,bottom-2),Math.Min(texture.width,right+3)-Math.Max(0,left-2),Math.Min(texture.height,top+3)-Math.Max(0,bottom-2));
            if(!feetPivot) rect=new Rect(0,0,texture.width,texture.height);
            UnityEngine.Object.DestroyImmediate(texture);
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var old=provider.GetSpriteRects().FirstOrDefault(r=>r.name==spriteName);
            var sprite=new SpriteRect { name=spriteName,spriteID=old!=null?old.spriteID:GUID.Generate(),rect=rect,
                alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,feetPivot?0:.5f) };
            provider.SetSpriteRects(new[]{sprite});
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(sprite.name,sprite.spriteID)});
            provider.Apply(); importer.SaveAndReimport();
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s=>s.name==spriteName);
        }

        public static void ApplyPlayerVisual(HoiAnPlayerController player)
        {
            var art = AssetDatabase.LoadAssetAtPath<DesignKitArt>(ArtAsset);
            if (art == null) throw new InvalidDataException("The Design Kit art registry is missing.");
            if (art.lumiExploration == null || !File.Exists(ExplorationPrefab)) ConfigureArt(art);
            var root = player.gameObject;
            root.GetComponent<SpriteRenderer>().enabled = false;
            if (root.GetComponent<SortingGroup>() == null) root.AddComponent<SortingGroup>();
            var previous = root.transform.Find("CanonicalLumiVisual");
            GameObject visual;
            if (previous != null && PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(previous.gameObject) == ExplorationPrefab)
                visual = previous.gameObject;
            else
            {
                if (previous != null) UnityEngine.Object.DestroyImmediate(previous.gameObject);
                visual = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(ExplorationPrefab), root.scene);
                visual.name = "CanonicalLumiVisual";
                visual.transform.SetParent(root.transform, false);
            }
            var renderer = visual.GetComponent<SpriteRenderer>();
            renderer.sprite = art.lumiExploration;
            renderer.color = Color.white;
            var scale = root.transform.lossyScale;
            const float worldHeight = .88f;
            const float footBaseline = -.33f;
            var uniformScale = worldHeight / renderer.sprite.bounds.size.y;
            visual.transform.localScale = new Vector3(uniformScale / scale.x, uniformScale / scale.y, 1);
            visual.transform.localPosition = new Vector3(0, (footBaseline - renderer.sprite.bounds.min.y * uniformScale) / scale.y, 0);
            visual.transform.localRotation = Quaternion.identity;
            if (root.GetComponent<LumiStepMotion>() == null) root.AddComponent<LumiStepMotion>();
            var label = root.transform.Find("Lumi label");
            if (label != null) label.gameObject.SetActive(false);
            PrefabUtility.RecordPrefabInstancePropertyModifications(visual.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            EditorSceneManager.MarkSceneDirty(root.scene);
        }
    }
}
