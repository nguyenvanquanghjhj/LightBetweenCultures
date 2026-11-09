using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    public static class AppearanceRevisionSetup
    {
        const string Review="Validation/NpcAppearance-20261021/";
        const string Fixture="Assets/Resources/AppearanceReviewData.asset";
        const string Baseline="Validation/NpcAppearance-20261021/native-before.json";
        [Serializable] public sealed class Ref {public string id,guid,portraitGuid; public long local,portraitLocal;}
        [Serializable] public sealed class Before {public Ref[] npcs;public Ref lumi;public string mapActors;}
        static Ref Capture(string id,Sprite a,Sprite b)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a,out string guid,out long local);
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(b,out string pg,out long pl);
            return new Ref{id=id,guid=guid,local=local,portraitGuid=pg,portraitLocal=pl};
        }
        static Sprite Resolve(string guid,long local)=>string.IsNullOrEmpty(guid)||local==0?null:AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)).OfType<Sprite>().Single(s=>{AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s,out string g,out long l);return l==local;});
        public static void Apply()
        {
            var art=AdventureUIArt.Load();var kit=DesignKitArt.Load();
            if(!File.Exists(Baseline))File.WriteAllText(Baseline,JsonUtility.ToJson(new Before{
                npcs=art.npcs.Select(n=>Capture(n.id,n.sprite,n.portrait??n.sprite)).ToArray(),
                lumi=Capture("lumi",kit.lumiExploration,kit.lumiPortrait),mapActors=JsonUtility.ToJson(art)},true));
            foreach(var pair in new[]{("npc_nguyet","NEW01-nguyet-cutout-master.png"),("npc_elder","NEW02-elder-cutout-master.png"),("npc_hana","NEW03-hana-cutout-master.png")})
            {
                var source=Review+pair.Item2;if(!File.Exists(source))throw new FileNotFoundException("Missing approved NPC cutout",source);
                var path="Assets/_Game/Art/Characters/NPC/OwnerRevision_20261021/"+pair.Item1+".png";
                Directory.CreateDirectory(Path.GetDirectoryName(path));File.Copy(source,path,true);
                AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Multiple;
                importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
                // These detailed full-body cutouts shrink to phone-size actors and
                // portraits. Mips prevent high-frequency fabric/hat aliasing; no
                // sharpening, recoloring or source-pixel edits are involved.
                importer.mipmapEnabled=true;importer.npotScale=TextureImporterNPOTScale.None;
                importer.filterMode=FilterMode.Trilinear;importer.wrapMode=TextureWrapMode.Clamp;
                importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.spritePixelsPerUnit=256;
                var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                var tex=new Texture2D(2,2);tex.LoadImage(File.ReadAllBytes(path));var pixels=tex.GetPixels32();
                int l=tex.width,r=0,b=tex.height,t=0;
                for(int y=0;y<tex.height;y++)for(int x=0;x<tex.width;x++)if(pixels[y*tex.width+x].a>64){l=Math.Min(l,x);r=Math.Max(r,x);b=Math.Min(b,y);t=Math.Max(t,y);}
                var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
                var old=provider.GetSpriteRects().FirstOrDefault();
                var rect=new SpriteRect{name=pair.Item1+"_Owner20261021",spriteID=old?.spriteID??GUID.Generate(),rect=new Rect(Math.Max(0,l-2),Math.Max(0,b-2),Math.Min(tex.width,r+3)-Math.Max(0,l-2),Math.Min(tex.height,t+3)-Math.Max(0,b-2)),alignment=SpriteAlignment.Custom,pivot=new Vector2(.5f,0)};
                provider.SetSpriteRects(new[]{rect});provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(rect.name,rect.spriteID)});provider.Apply();importer.SaveAndReimport();UnityEngine.Object.DestroyImmediate(tex);
                var entry=art.Npc(pair.Item1);entry.sprite=entry.portrait=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();entry.portraitUseSpriteMesh=false;
                entry.source=source;entry.status="OWNER_DESIGN_REPLACEMENT_20261021; transparent cutout; Windows visual review";
            }
            EditorUtility.SetDirty(art);LumiRoleSetup.ConfigureArt(kit);
            var scene=EditorSceneManager.OpenScene("Assets/_Game/Scenes/HoiAnDemo.unity");
            LumiRoleSetup.ApplyPlayerVisual(UnityEngine.Object.FindAnyObjectByType<HoiAn.HoiAnPlayerController>());
            EditorSceneManager.SaveScene(scene);EditorSceneManager.OpenScene("Assets/_Game/Scenes/Prototype.unity");
            AssetDatabase.SaveAssets();ArtLibrarySetup.Export();Debug.Log("APPEARANCE REVISION APPLIED: exactly 3 NPC identities + canonical white Lumi; no gameplay changes.");
        }
        public static void BuildReviewWindows()
        {
            if(File.Exists(Fixture))throw new InvalidOperationException("Review fixture already exists; do not overwrite unknown asset.");
            var before=JsonUtility.FromJson<Before>(File.ReadAllText(Baseline));var data=ScriptableObject.CreateInstance<AppearanceReviewData>();
            data.npcIds=before.npcs.Select(n=>n.id).ToArray();data.beforeSprites=before.npcs.Select(n=>Resolve(n.guid,n.local)).ToArray();data.beforePortraits=before.npcs.Select(n=>n.portraitLocal==0?Resolve(n.guid,n.local):Resolve(n.portraitGuid,n.portraitLocal)).ToArray();
            data.beforeLumiExploration=Resolve(before.lumi.guid,before.lumi.local);data.beforeLumiPortrait=Resolve(before.lumi.portraitGuid,before.lumi.portraitLocal);data.beforeLumiMap=DesignKitArt.Load().lumiMapTraveler;data.mapActorsBefore=before.mapActors;
            AssetDatabase.CreateAsset(data,Fixture);AssetDatabase.SaveAssets();
            try{Phase5Setup.Build();}finally{AssetDatabase.DeleteAsset(Fixture);AssetDatabase.SaveAssets();}
        }
        public static void ApplyAndBuildReviewWindows(){Apply();BuildReviewWindows();}
    }
}
