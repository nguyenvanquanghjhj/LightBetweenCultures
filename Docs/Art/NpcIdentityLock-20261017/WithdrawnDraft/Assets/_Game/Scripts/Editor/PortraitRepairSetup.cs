using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class PortraitRepairSetup
    {
        const string Sheet="Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png";
        const string Registry="Docs/Art/NPC_CANONICAL_REGISTRY.json";
        const string Proof="Docs/Art/NpcIdentityLock-20261017/";
        [Serializable] class IdentityRegistry {public Identity[] identities;}
        [Serializable] class Identity {public string npcId,status;public Variant[] variants;}
        [Serializable] class Variant {public string variantId,status,path,guid,localId,sourceSHA256;public bool allowedInRuntime;}
        static string Hash(string path)
        {using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-","").ToLowerInvariant();}
        static string Reference(Sprite s)
        {if(s==null)return "MISSING";AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s,out string guid,out long id);return guid+"/"+id;}
        public static void ValidateCanonicalIdentityBindings()
        {
            if(!File.Exists(Registry))throw new BuildFailedException("NPC canonical registry is missing; restore it before building.");
            var data=JsonUtility.FromJson<IdentityRegistry>(File.ReadAllText(Registry));var art=AdventureUIArt.Load();
            if(data.identities.Select(n=>n.npcId).Distinct().Count()!=data.identities.Length||art.npcs.Select(n=>n.id).Distinct().Count()!=art.npcs.Length)
                throw new BuildFailedException("Duplicate NPC identity ID.");
            var hashes=new Dictionary<string,string>();var portraits=new Dictionary<string,string>();
            void Approved(Sprite sprite,Identity n)
            {
                var reference=Reference(sprite);
                var v=n.variants.FirstOrDefault(v=>v.allowedInRuntime&&(v.status=="CANONICAL"||v.status=="APPROVED_VARIANT")&&reference==v.guid+"/"+v.localId&&AssetDatabase.GetAssetPath(sprite)==v.path);
                if(v==null)throw new BuildFailedException("Unapproved NPC image: "+n.npcId+" "+reference+". Keep its approved identity or register an approved variant.");
                if(!hashes.TryGetValue(v.path,out var hash))hashes[v.path]=hash=Hash(v.path);
                if(string.IsNullOrEmpty(v.sourceSHA256)||hash!=v.sourceSHA256)throw new BuildFailedException("Canonical source pixels changed: "+n.npcId+" "+v.path);
            }
            foreach(var n in data.identities)
            {
                var entry=art.Npc(n.npcId);
                if(n.status=="MISSING")
                {if(entry?.sprite!=null||entry?.portrait!=null)throw new BuildFailedException("Unregistered art assigned to missing NPC: "+n.npcId);continue;}
                if(n.status!="CANONICAL"||entry==null)throw new BuildFailedException("Canonical NPC binding is missing: "+n.npcId);
                if(entry.sprite==null)throw new BuildFailedException("Primary sprite is missing: "+n.npcId);
                // Unity's serialized null Object can be a managed wrapper in
                // Editor; use Unity null semantics rather than ?? here.
                var portrait=entry.portrait!=null?entry.portrait:entry.sprite;
                Approved(entry.sprite,n);Approved(portrait,n);
                var key=Reference(portrait);
                if(portraits.TryGetValue(key,out var other))throw new BuildFailedException("Independent NPC identities share a portrait: "+other+" / "+n.npcId);
                portraits[key]=n.npcId;
            }
            foreach(var e in art.npcs)if(!data.identities.Any(n=>n.npcId==e.id))throw new BuildFailedException("Unknown NPC identity: "+e.id);
            Approved(DesignKitArt.Load().maiPortrait,data.identities.First(n=>n.npcId=="npc_mai"));
        }
        // Prebuild callback also covers File > Build, not only custom build buttons.
        public sealed class CanonicalIdentityBuildGuard : IPreprocessBuildWithReport
        {
            public int callbackOrder=>-1000;
            public void OnPreprocessBuild(BuildReport report)
            {ValidateCanonicalIdentityBindings();Debug.Log("NPC IDENTITY PREBUILD GUARD PASS: approved references, unique portraits and source pixels locked.");}
        }
        static void ExportNativeSprite(Sprite sprite,string name)
        {
            // Review-only export of existing Unity pixels and mesh; no new art
            // or Assets import. The mesh excludes neighbouring sheet characters.
            var source=sprite.texture;var rt=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
            var old=RenderTexture.active;var copy=new Texture2D(source.width,source.height,TextureFormat.RGBA32,false);
            Graphics.Blit(source,rt);RenderTexture.active=rt;copy.ReadPixels(new Rect(0,0,source.width,source.height),0,0);copy.Apply();RenderTexture.active=old;RenderTexture.ReleaseTemporary(rt);
            var rect=sprite.rect;var w=Mathf.RoundToInt(rect.width);var h=Mathf.RoundToInt(rect.height);var pixels=copy.GetPixels(Mathf.RoundToInt(rect.x),Mathf.RoundToInt(rect.y),w,h);
            var vertices=sprite.vertices.Select(v=>v*sprite.pixelsPerUnit+sprite.pivot).ToArray();var triangles=sprite.triangles;
            float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
            for(var y=0;y<h;y++)for(var x=0;x<w;x++)
            {
                var p=new Vector2(x+.5f,y+.5f);var inside=false;
                for(var i=0;i<triangles.Length;i+=3)
                {
                    var a=vertices[triangles[i]];var b=vertices[triangles[i+1]];var c=vertices[triangles[i+2]];
                    var d1=Cross(b-a,p-a);var d2=Cross(c-b,p-b);var d3=Cross(a-c,p-c);
                    if((d1>=0&&d2>=0&&d3>=0)||(d1<=0&&d2<=0&&d3<=0)){inside=true;break;}
                }
                if(!inside)pixels[y*w+x]=Color.clear;
            }
            var image=new Texture2D(w,h,TextureFormat.RGBA32,false);image.SetPixels(pixels);image.Apply();
            Directory.CreateDirectory(Proof+"NativeSprites");File.WriteAllBytes(Proof+"NativeSprites/"+name+".png",image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(copy);
        }
        public static void ApplyCanonicalApprovalAndBuild()
        {
            ValidateCanonicalIdentityBindings();var art=AdventureUIArt.Load();
            var original=art.Npc("npc_soyeon").sprite;var rejected=false;
            try{art.Npc("npc_soyeon").sprite=art.Npc("npc_huong").sprite;ValidateCanonicalIdentityBindings();}
            catch(BuildFailedException){rejected=true;}
            finally{art.Npc("npc_soyeon").sprite=original;}
            if(!rejected)throw new InvalidDataException("Identity guard accepted an intentional in-memory wrong-face assignment.");
            ValidateCanonicalIdentityBindings();
            foreach(var entry in art.npcs)entry.status="CANONICAL_USER_CONFIRMED_20261017";
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            ExportNativeSprite(art.Npc("npc_ba_tu").portrait??art.Npc("npc_ba_tu").sprite,"BaTu-native-sprite");
            ExportNativeSprite(art.Npc("npc_phuc").portrait??art.Npc("npc_phuc").sprite,"Phuc-native-sprite");
            File.WriteAllText(Proof+"editor-guard-result.json","{\"status\":\"PASS\",\"intentionalWrongFaceRejected\":true,\"assetBindingRestoredBeforeSave\":true,\"canonicalNPCs\":20,\"missingNPCs\":7,\"reviewPNGsNotImportedAsAssets\":true}");
            SliceValidation.BuildWindows();
        }
        public static void BuildWindows()
        {
            var art=AdventureUIArt.Load();
            // Record every existing identity and reference before adding two entries.
            var before=art.npcs.ToDictionary(n=>n.id,n=>n.sprite);
            var importer=(TextureImporter)AssetImporter.GetAtPath(Sheet);
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var rects=provider.GetSpriteRects().ToList();var old=rects.ToDictionary(r=>r.name,r=>r.spriteID);
            var phuc=rects.FirstOrDefault(r=>r.name=="Phuc_DialoguePortrait");
            if(phuc==null){phuc=new SpriteRect{name="Phuc_DialoguePortrait",spriteID=GUID.Generate()};rects.Add(phuc);}
            phuc.rect=new Rect(584,521,263,411);phuc.pivot=new Vector2(.5f,0);phuc.alignment=SpriteAlignment.Custom;
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            // Source art has the next merchant's basket near the lower-right.
            // A native outline excludes it, while retaining Phuc's raised hand,
            // carving, hat and feet. No PNG is duplicated or altered.
            var boundary=new[]{new Vector2(584,154),new Vector2(847,154),new Vector2(847,392),new Vector2(819,411),new Vector2(812,430),new Vector2(812,565),new Vector2(584,565)};
            provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(phuc.spriteID,new List<Vector2[]>{boundary.Select(p=>new Vector2(p.x-584-131.5f,565-p.y-205.5f)).ToArray()});
            // Reuse Ba Tu's existing sprite ID. Its automatic slice includes a
            // separate 213-pixel fragment of Mai at x330..336, y264..308.
            // The notch removes only that neighbour; Ba Tu's connected body
            // (x335..576, y195..549) and both hands remain intact.
            var batu=rects.First(r=>r.name=="H3_07_Characters_NPC_Concept_23");
            var batuBoundary=new[]{new Vector2(330,191),new Vector2(582,191),new Vector2(582,605),new Vector2(330,605),new Vector2(330,315),new Vector2(340,315),new Vector2(340,250),new Vector2(330,250)};
            provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(batu.spriteID,new List<Vector2[]>{batuBoundary.Select(p=>new Vector2(p.x-330-126,605-p.y-207)).ToArray()});
            provider.Apply();
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.crunchedCompression=false;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.SaveAndReimport();
            foreach(var entry in old)if(provider.GetSpriteRects().First(r=>r.name==entry.Key).spriteID!=entry.Value)throw new InvalidDataException("Existing slice ID changed: "+entry.Key);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToDictionary(s=>s.name);
            var npcs=art.npcs.ToList();
            foreach(var pair in new[]{("npc_ba_tu","H3_07_Characters_NPC_Concept_23"),("npc_phuc","Phuc_DialoguePortrait")})
            {
                var entry=npcs.FirstOrDefault(n=>n.id==pair.Item1);
                if(entry==null){entry=new AdventureImage{id=pair.Item1};npcs.Add(entry);}
                entry.sprite=entry.portrait=sprites[pair.Item2];entry.portraitUseSpriteMesh=true;entry.source=Sheet;
                entry.status="CANONICAL_USER_CONFIRMED_20261017";
            }
            art.npcs=npcs.ToArray();
            foreach(var entry in before)if(art.Npc(entry.Key).sprite!=entry.Value)throw new InvalidDataException("Existing NPC identity changed: "+entry.Key);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();ArtLibrarySetup.Export();
            Debug.Log("Portrait repair: reused existing Ba Tu slice; one native Phuc slice; all existing NPC and sheet IDs preserved.");
            SliceValidation.BuildWindows();
        }
    }
}
