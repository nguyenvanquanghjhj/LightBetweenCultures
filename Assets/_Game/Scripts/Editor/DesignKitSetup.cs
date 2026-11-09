using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LightBetweenCultures.HoiAn;

namespace LightBetweenCultures.Editor
{
    public static class DesignKitSetup
    {
        private const string ArtRoot = "Assets/_Game/Art/DesignKit";
        private const string ArtAsset = "Assets/_Game/Resources/DesignKitArt.asset";
        private const string PrefabRoot = "Assets/_Game/Prefabs/DesignKit";
        private const string HoiAnScene = "Assets/_Game/Scenes/HoiAnDemo.unity";

        [MenuItem("Tools/Light Between Cultures/Apply Provided Design Kit Art")]
        public static void Apply()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ImportArt();
            var previous = EditorSceneManager.GetSceneManagerSetup();
            var backup = Path.GetFullPath("../Backups/DesignKitSceneUpdates"); Directory.CreateDirectory(backup);
            File.Copy(HoiAnScene,Path.Combine(backup,"HoiAnDemo-"+DateTime.Now.ToString("yyyyMMdd-HHmmssfff")+".unity"));
            var scene = EditorSceneManager.OpenScene(HoiAnScene);
            ApplyCurrentHoiAnScene();
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            if (previous.Length > 0 && Array.TrueForAll(previous, entry => !string.IsNullOrEmpty(entry.path)))
                EditorSceneManager.RestoreSceneManagerSetup(previous);
            else EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype.unity");
            Debug.Log("Provided Design Kit: 14 PNGs imported; art registry, prefabs and HoiAnDemo applied.");
        }

        private static void ImportArt()
        {
            var source = Path.GetFullPath(Path.Combine(Application.dataPath,"../LightBetweenCultures_DesignKit_v1/Assets"));
            if (!Directory.Exists(source)) throw new DirectoryNotFoundException(source);
            foreach (var file in Directory.GetFiles(source,"*.png",SearchOption.AllDirectories))
            {
                var relative = file.Substring(source.Length+1).Replace('\\','/');
                var target = ArtLibrarySetup.ResolveMovedPath(ArtRoot+"/"+relative);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                if (!File.Exists(target) || Convert.ToBase64String(File.ReadAllBytes(file)) != Convert.ToBase64String(File.ReadAllBytes(target))) File.Copy(file,target,true);
            }
            AssetDatabase.Refresh();
            foreach (var file in Directory.GetFiles(source,"*.png",SearchOption.AllDirectories).Select(p=>ArtLibrarySetup.ResolveMovedPath(ArtRoot+"/"+p.Substring(source.Length+1).Replace('\\','/'))))
            {
                var path = file.Replace('\\','/');
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                var textureSettings = new TextureImporterSettings();
                importer.ReadTextureSettings(textureSettings);
                textureSettings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(textureSettings);
                importer.spritePixelsPerUnit = path.Contains("Markers/") ? 192 : path.Contains("128") ? 128 : 256;
                importer.alphaSource = TextureImporterAlphaSource.FromInput;
                importer.alphaIsTransparency = true;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.mipmapEnabled = false;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.maxTextureSize = 2048;
                importer.SaveAndReimport();
            }
            var art = AssetDatabase.LoadAssetAtPath<DesignKitArt>(ArtAsset);
            if (art == null) { art=ScriptableObject.CreateInstance<DesignKitArt>(); AssetDatabase.CreateAsset(art,ArtAsset); }
            art.lumiCity = Sprite("Lumi/Lumi_CityMarker_256.png");
            art.lumiCitySmall = Sprite("Lumi/Lumi_CityMarker_128.png");
            art.available = Marker("available"); art.current = Marker("current"); art.visited = Marker("visited");
            art.locked = Marker("locked"); art.npc = Marker("npc"); art.quest = Marker("quest");
            art.item = Marker("item"); art.enemy = Marker("enemy"); art.puzzle = Marker("puzzle");
            art.portal = Marker("portal"); art.secret = Marker("secret");
            EditorUtility.SetDirty(art);
            Directory.CreateDirectory(PrefabRoot); AssetDatabase.Refresh();
            SaveVisualPrefab("LumiMapToken",art.lumiCity);
            SaveVisualPrefab("LanternPartMarker",art.item);
            SaveVisualPrefab("NpcMarker",art.npc);
            LumiRoleSetup.ConfigureArt(art);
            AssetDatabase.SaveAssets();
        }
        private static Sprite Sprite(string path)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtLibrarySetup.ResolveMovedPath(ArtRoot+"/"+path));
            if (sprite == null) throw new InvalidDataException("Sprite import failed: "+path);
            return sprite;
        }
        private static Sprite Marker(string name) => Sprite("Markers/node_"+name+".png");
        private static void SaveVisualPrefab(string name, Sprite sprite)
        {
            var go = new GameObject(name,typeof(SpriteRenderer));
            go.GetComponent<SpriteRenderer>().sprite=sprite;
            PrefabUtility.SaveAsPrefabAsset(go,PrefabRoot+"/"+name+".prefab");
            UnityEngine.Object.DestroyImmediate(go);
        }

        // Presentation only: original physics, spawn and item coordinates remain intact.
        public static void ApplyCurrentHoiAnScene()
        {
            if (AssetDatabase.LoadAssetAtPath<DesignKitArt>(ArtAsset) == null) return;
            var player = UnityEngine.Object.FindAnyObjectByType<HoiAnPlayerController>();
            LumiRoleSetup.ApplyPlayerVisual(player);
            var label=player.transform.Find("Lumi label"); if(label!=null) label.gameObject.SetActive(false);
            var item = UnityEngine.Object.FindAnyObjectByType<HoiAnItem>(FindObjectsInactive.Include);
            AddVisual(item.gameObject,"ProvidedItemVisual","LanternPartMarker",.60f);
            var npc = GameObject.Find("Nguoi dan (placeholder)");
            if(npc!=null) AddVisual(npc,"ProvidedNpcVisual","NpcMarker",.85f);
            Tint("Ground",DesignKitArt.Hex("DBC38F")); Tint("Road",DesignKitArt.Cream);
            Tint("River_Left",DesignKitArt.Teal); Tint("River_Right",DesignKitArt.Teal);
            DecorateHouse("House_Left"); DecorateHouse("House_Right");
            DecorateTree("Tree_Left"); DecorateTree("Tree_Right");
            DecorateBridge(); DecorateRoad();
            Tint("Lantern_Left",DesignKitArt.Roof); Tint("Lantern_Right",DesignKitArt.Gold);
            EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
        }
        private static void AddVisual(GameObject root,string childName,string prefabName,float worldSize)
        {
            root.GetComponent<SpriteRenderer>().enabled=false;
            if(root.GetComponent<SortingGroup>()==null) root.AddComponent<SortingGroup>();
            if(root.transform.Find(childName)!=null) return;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot+"/"+prefabName+".prefab");
            var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root.scene);
            visual.name=childName; visual.transform.SetParent(root.transform,false);
            var scale=root.transform.lossyScale;
            visual.transform.localScale=new Vector3(worldSize/scale.x,worldSize/scale.y,1);
        }
        private static void Tint(string name,Color color) { var go=GameObject.Find(name); if(go!=null) go.GetComponent<SpriteRenderer>().color=color; }
        private static void Part(Transform parent,string name,Vector2 position,Vector2 size,Color color,int order=1,float angle=0)
        {
            var go=new GameObject(name,typeof(SpriteRenderer)); go.transform.SetParent(parent,false);
            go.transform.localPosition=position; go.transform.localScale=new Vector3(size.x,size.y,1);
            go.transform.localRotation=Quaternion.Euler(0,0,angle);
            var renderer=go.GetComponent<SpriteRenderer>();
            renderer.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Game/Art/Environment/Common/UnitSprite.png");
            renderer.color=color; renderer.sortingOrder=order;
        }
        private static Transform DecorRoot(GameObject go)
        {
            if(go.transform.Find("StructureProxy")!=null) return null;
            var root=new GameObject("StructureProxy"); root.transform.SetParent(go.transform,false); return root.transform;
        }
        private static void DecorateHouse(string name)
        {
            var go=GameObject.Find(name); var root=DecorRoot(go); if(root==null) return;
            go.GetComponent<SpriteRenderer>().color=DesignKitArt.Wall;
            if(go.GetComponent<SortingGroup>()==null) go.AddComponent<SortingGroup>();
            Part(root,"Roof silhouette",new Vector2(0,.34f),new Vector2(1.10f,.38f),DesignKitArt.Ink);
            Part(root,"Terracotta roof",new Vector2(0,.37f),new Vector2(1.04f,.28f),DesignKitArt.Roof,2);
            for(var i=0;i<9;i++) Part(root,"Roof joint "+i,new Vector2(-.46f+i*.115f,.37f),new Vector2(.012f,.27f),DesignKitArt.Hex("8B4436"),3);
            Part(root,"Door",new Vector2(0,-.23f),new Vector2(.20f,.52f),DesignKitArt.Ink,3);
            Part(root,"Door panel",new Vector2(0,-.23f),new Vector2(.15f,.45f),DesignKitArt.Hex("8B6246"),4);
            foreach(var x in new[]{-.30f,.30f})
            {
                Part(root,"Window frame",new Vector2(x,-.06f),new Vector2(.21f,.30f),DesignKitArt.Ink,3);
                Part(root,"Window light",new Vector2(x,-.06f),new Vector2(.15f,.23f),DesignKitArt.Gold,4);
            }
            Part(root,"Foundation",new Vector2(0,-.46f),new Vector2(1.02f,.08f),DesignKitArt.Hex("96704E"),5);
        }
        private static void DecorateTree(string name)
        {
            var go=GameObject.Find(name); var root=DecorRoot(go); if(root==null) return;
            go.GetComponent<SpriteRenderer>().enabled=false;
            if(go.GetComponent<SortingGroup>()==null) go.AddComponent<SortingGroup>();
            Part(root,"Trunk",new Vector2(0,-.30f),new Vector2(.18f,.42f),DesignKitArt.Hex("8B6246"));
            Part(root,"Canopy outline",new Vector2(0,.08f),new Vector2(.63f,.63f),DesignKitArt.Hex("315946"),2,45);
            Part(root,"Canopy",new Vector2(0,.12f),new Vector2(.56f,.56f),DesignKitArt.Hex("568661"),3,45);
            Part(root,"Leaf highlight",new Vector2(-.10f,.22f),new Vector2(.27f,.27f),DesignKitArt.Hex("86AD72"),4,45);
        }
        private static void DecorateBridge()
        {
            var go=GameObject.Find("Bridge"); var root=DecorRoot(go); if(root==null) return;
            for(var i=0;i<12;i++) Part(root,"Wood plank "+i,new Vector2(-.46f+i*.083f,0),new Vector2(.069f,.94f),DesignKitArt.Hex("BC9260"),-6);
            Part(root,"Rail left",new Vector2(-.46f,0),new Vector2(.035f,1.02f),DesignKitArt.Ink,-5);
            Part(root,"Rail right",new Vector2(.46f,0),new Vector2(.035f,1.02f),DesignKitArt.Ink,-5);
        }
        private static void DecorateRoad()
        {
            var go=GameObject.Find("Road"); var root=DecorRoot(go); if(root==null) return;
            for(var i=0;i<30;i++) Part(root,"Paving joint "+i,new Vector2(-.49f+i/30f,0),new Vector2(.001f,.96f),DesignKitArt.Hex("D8C5A3"),-8);
            for(var i=0;i<4;i++) Part(root,"Paving row "+i,new Vector2(0,-.48f+i*.32f),new Vector2(1,.006f),DesignKitArt.Hex("D8C5A3"),-8);
        }
    }
}
