using System;
using System.IO;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // In-place presentation upgrade. Existing scene objects/physics/IDs survive.
    public static class H3ArtSetup
    {
        const string Sheets="Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/";
        const string Env="Assets/_Game/Art/Environment/HoiAn/H3/";
        const string Ui="Assets/_Game/Art/UI/H3/";
        // Editor-only safety verification: invoke the actual legacy entry point,
        // not a mock. It must return without saving/replacing the authored scene.
        public static void VerifyLegacyBuilderGuard()
        {
            const string scene="Assets/_Game/Scenes/HoiAnDemo.unity";
            var before=File.ReadAllBytes(scene);
            var active=EditorSceneManager.GetActiveScene().path;
            HoiAnDemoSetup.Setup();
            if(!before.SequenceEqual(File.ReadAllBytes(scene))||EditorSceneManager.GetActiveScene().path!=active)
                throw new InvalidDataException("Legacy scene setup modified the authored H3 scene or active editor scene.");
            Directory.CreateDirectory("Docs/QualityAddendum-20261017");
            File.WriteAllText("Docs/QualityAddendum-20261017/editor-guard-result.json",
                "{\"status\":\"PASS\",\"actualLegacySetupInvoked\":true,\"sceneBytesUnchanged\":true,\"activeSceneUnchanged\":true,\"playerBuildRequired\":false}");
            Debug.Log("H3 LEGACY BUILDER GUARD PASS: actual setup call preserved authored scene bytes and active scene.");
        }
        static Sprite Cut(string file,string name,Rect topRect,bool ui=false,Vector4 border=default)
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Sheets+file);
            if(texture==null)throw new InvalidDataException(file);
            var path=(ui?Ui:Env)+name+".asset";
            var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(existing!=null)return existing;
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var rect=new Rect(topRect.x,texture.height-topRect.y-topRect.height,topRect.width,topRect.height);
            var sprite=Sprite.Create(texture,rect,ui?new Vector2(.5f,.5f):new Vector2(.5f,0),100,0,SpriteMeshType.FullRect,border);
            sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
        }
        static void Hide(GameObject go)
        {if(go!=null)foreach(var r in go.GetComponentsInChildren<Renderer>(true))r.enabled=false;}
        static GameObject Visual(string name,Sprite sprite,Vector2 foot,float width,int order,Transform parent=null)
        {
            var go=new GameObject(name,typeof(SpriteRenderer));
            if(parent!=null)go.transform.SetParent(parent,false);
            go.transform.position=foot;
            var scale=width/sprite.bounds.size.x;
            var parentScale=parent!=null?parent.lossyScale:Vector3.one;
            go.transform.localScale=new Vector3(scale/parentScale.x,scale/parentScale.y,1);
            var sr=go.GetComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingOrder=order;
            return go;
        }
        public static void ApplyAndBuild(){Apply();SliceValidation.BuildWindows();}
        public static void FrameAndBuild()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/HoiAnDemo.unity");
            var camera=new SerializedObject(Camera.main.GetComponent<FollowCamera2D>());
            camera.FindProperty("framingOffset").vector2Value=new Vector2(0,1.2f);
            camera.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(Camera.main.gameObject.scene);SliceValidation.BuildWindows();
        }
        public static void ContinuousGroundAndBuild()
        {
            const string path=Env+"H3_StoneGround_Continuous_v1.png";
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.maxTextureSize=2048;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.crunchedCompression=false;
            importer.alphaIsTransparency=true;importer.filterMode=FilterMode.Bilinear;
            importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
            importer.SaveAndReimport();
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/HoiAnDemo.unity");
            var ground=GameObject.Find("H3IllustratedGround");
            // These children are only the temporary paving created by this tool;
            // remove their visible joins, not any original physics object.
            foreach(Transform child in ground.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
            var stone=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            var floor=Visual("H3ContinuousStoneFloor",stone,new Vector2(0,-10),20,-200,ground.transform);
            floor.transform.position=Vector3.zero; // imported single sprite uses a centered pivot
            EditorSceneManager.SaveScene(ground.scene);AssetDatabase.SaveAssets();SliceValidation.BuildWindows();
        }
        public static void RefineAndBuild()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/HoiAnDemo.unity");
            var stone=AssetDatabase.LoadAssetAtPath<Sprite>(Env+"H3_Stone_B.asset");
            // One paving material across the alley; do not alternate masonry and
            // cobblestone patches, which produced a visible checkerboard.
            var root=GameObject.Find("H3IllustratedGround").transform;
            foreach(var sr in root.GetComponentsInChildren<SpriteRenderer>())
            {sr.sprite=stone;sr.color=new Color(.97f,.89f,.75f);}
            EditorSceneManager.SaveScene(root.gameObject.scene);AssetDatabase.SaveAssets();SliceValidation.BuildWindows();
        }
        [MenuItem("Tools/Light Between Cultures/Upgrade H3 Presentation In Place")]
        public static void Apply()
        {
            EditorSceneManager.OpenScene("Assets/_Game/Scenes/HoiAnDemo.unity");
            var player=UnityEngine.Object.FindAnyObjectByType<HoiAnPlayerController>();
            var item=UnityEngine.Object.FindAnyObjectByType<HoiAnItem>(FindObjectsInactive.Include);
            var gameplay=UnityEngine.Object.FindAnyObjectByType<HoiAnGameplay>();
            if(GameObject.Find("H3IllustratedGround")!=null)throw new InvalidOperationException("H3 already upgraded; do not duplicate art.");
            var colliders=UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include,FindObjectsSortMode.None)
                .ToDictionary(c=>c,c=>EditorJsonUtility.ToJson(c));
            var spawn=player.transform.position;var pickup=item.transform.position;
            const string env="H3_02_Environment_Architecture_Props.png";
            var houseA=Cut(env,"H3_House_A",new Rect(5,50,340,296));
            var houseB=Cut(env,"H3_House_B",new Rect(354,8,347,338));
            var tileA=Cut(env,"H3_Stone_A",new Rect(305,595,128,112));
            var tileB=Cut(env,"H3_Stone_B",new Rect(460,596,128,112));
            var wall=Cut(env,"H3_Garden_Wall",new Rect(12,370,258,182));
            var flower=Cut(env,"H3_Pink_Pot",new Rect(1138,574,143,163));
            var citrus=Cut(env,"H3_Citrus_Pot",new Rect(1291,568,145,170));
            var crates=Cut(env,"H3_Crates",new Rect(756,582,174,152));
            const string sheetUi="H3_05_UI_BlankPanels_Sheet.png";
            var title=Cut(sheetUi,"H3_Teal_Frame",new Rect(623,367,457,164),true,new Vector4(72,38,72,38));
            var button=Cut(sheetUi,"H3_Gold_Button",new Rect(477,779,311,117),true,new Vector4(55,32,55,32));
            var panel=Cut(sheetUi,"H3_Cream_Frame",new Rect(801,19,630,312),true,new Vector4(98,66,98,66));
            foreach(var name in new[]{"Ground","Road","River_Left","River_Right","Bridge","House_Left","House_Right","Tree_Left","Tree_Right","Lantern_Left","Lantern_Right","Nguoi dan (placeholder)"})Hide(GameObject.Find(name));
            var ground=new GameObject("H3IllustratedGround").transform;
            for(int y=-5;y<5;y++)for(int x=-6;x<6;x++)
            {
                var sr=Visual("Paving "+x+","+y,(x+y)%3==0?tileB:tileA,new Vector2(x*2.0f,-8+y*1.75f+8.75f),2.01f,-200,ground).GetComponent<SpriteRenderer>();
                sr.color=new Color(.92f,.87f,.72f);
            }
            foreach(var pair in new[]{("House_Left",houseA,3.9f),("House_Right",houseB,3.75f)})
            {
                var root=GameObject.Find(pair.Item1);
                Visual("IllustratedFacade",pair.Item2,new Vector2(root.transform.position.x,.5f),pair.Item3,0,root.transform);
            }
            foreach(var name in new[]{"Tree_Left","Tree_Right"})
            {
                var root=GameObject.Find(name);Visual("IllustratedPlant",name.EndsWith("Left")?flower:citrus,
                    new Vector2(root.transform.position.x,root.transform.position.y-.75f),1.6f,0,root.transform);
            }
            var background=new GameObject("H3BackgroundGarden").transform;
            foreach(var x in new[]{-7f,-4.2f,4.2f,7f})Visual("Garden masonry",wall,new Vector2(x,3.0f),2.8f,60,background);
            Visual("Rear house",houseB,new Vector2(0,5.1f),4.2f,40,background);
            var decor=new GameObject("H3Decorations").transform;
            Visual("Crates at western boundary",crates,new Vector2(-8.5f,-.5f),1.5f,110,decor);
            Visual("Bougainvillea beside left door",flower,new Vector2(-6.1f,.6f),.9f,96,decor);
            Visual("Citrus beside right door",citrus,new Vector2(5.5f,.6f),.9f,96,decor);
            Hide(item.gameObject);
            Visual("H3PickupVisual",AdventureUIArt.Load().Item("lamp_eye").sprite,pickup-Vector3.up*.23f,.58f,2,item.transform);
            gameplay.ConfigurePresentation(title,panel,button);
            Camera.main.backgroundColor=DesignKitArt.Hex("466558");
            foreach(var c in UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if(!colliders.TryGetValue(c,out var json)||EditorJsonUtility.ToJson(c)!=json)throw new InvalidDataException("Physics changed: "+c.name);
            if(player.transform.position!=spawn||item.transform.position!=pickup)throw new InvalidDataException("Gameplay anchor changed");
            EditorSceneManager.SaveScene(player.gameObject.scene);AssetDatabase.SaveAssets();
            Debug.Log("H3 art applied in-place. Original physics, canonical Lumi, spawn and pickup anchors preserved; no PNG copied or edited.");
        }
    }
}
