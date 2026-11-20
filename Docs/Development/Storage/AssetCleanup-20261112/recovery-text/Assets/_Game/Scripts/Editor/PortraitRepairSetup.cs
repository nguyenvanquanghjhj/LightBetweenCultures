using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class PortraitRepairSetup
    {
        const string Sheet="Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png";
        static string SpriteReference(Sprite sprite)
        {
            if(sprite==null)return "";
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long localId);
            return guid+"/"+localId;
        }
        static string NpcBindingKey(AdventureImage npc)=>npc.id+"|"+SpriteReference(npc.sprite)+"|"+SpriteReference(npc.portrait)+"|"+npc.portraitUseSpriteMesh+"|"+npc.source+"|"+npc.status+"|"+npc.description+"|"+npc.aliasOf+"|"+npc.categoryIcon;
        // Explicit user-authorized reuse. Does not run the historical Ba Tu/Phuc repair.
        public static void BuildJiWonReuse()
        {
            var art=AdventureUIArt.Load();
            var before=art.npcs.ToDictionary(n=>n.id,NpcBindingKey);
            var importer=(TextureImporter)AssetImporter.GetAtPath(Sheet);
            var factories=new SpriteDataProviderFactories();factories.Init();
            var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var rects=provider.GetSpriteRects().ToList();var old=rects.ToDictionary(r=>r.name,r=>r.spriteID);
            const string name="JiWon_DialoguePortrait";
            var entry=rects.FirstOrDefault(r=>r.name==name);
            if(entry==null){entry=new SpriteRect{name=name,spriteID=GUID.Generate()};rects.Add(entry);}
            entry.rect=new Rect(24,66,355,400);entry.pivot=new Vector2(.5f,0);entry.alignment=SpriteAlignment.Custom;
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            // Top-left source coordinates: keep both hands, flag and dress, while
            // excluding the neighbouring traveller's hat at the upper-right.
            var boundary=new[]{new Vector2(24,620),new Vector2(345,620),new Vector2(345,710),new Vector2(379,733),new Vector2(379,797),new Vector2(351,815),new Vector2(351,1020),new Vector2(24,1020)};
            provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(entry.spriteID,new List<Vector2[]>{boundary.Select(v=>new Vector2(v.x-24-177.5f,1020-v.y-200)).ToArray()});
            provider.Apply();importer.SaveAndReimport();
            foreach(var previous in old)if(provider.GetSpriteRects().First(r=>r.name==previous.Key).spriteID!=previous.Value)throw new InvalidDataException("Existing slice ID changed: "+previous.Key);
            var sprite=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().Single(s=>s.name==name);
            if(art.npcs.Any(n=>n.id!="npc_tech"&&(n.sprite==sprite||n.portrait==sprite)))throw new InvalidDataException("Ji-won source already assigned to another NPC");
            var npcs=art.npcs.ToList();var npc=npcs.FirstOrDefault(n=>n.id=="npc_tech");
            if(npc==null){npc=new AdventureImage{id="npc_tech"};npcs.Add(npc);}
            npc.sprite=npc.portrait=sprite;npc.portraitUseSpriteMesh=true;npc.source=Sheet;
            npc.status="CANONICAL_USER_AUTHORIZED_UNASSIGNED_SOURCE_REUSE; Ji-won festival hanbok; static_native_sprite";
            art.npcs=npcs.ToArray();
            foreach(var previous in before)if(previous.Key!="npc_tech"&&NpcBindingKey(art.Npc(previous.Key))!=previous.Value)throw new InvalidDataException("Existing NPC binding changed: "+previous.Key);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long localId);
            Directory.CreateDirectory("Docs/Art/NpcReuse-20261017");
            File.WriteAllText("Docs/Art/NpcReuse-20261017/jiwon-native-reference.json","{\"npcId\":\"npc_tech\",\"spriteName\":\""+name+"\",\"path\":\""+Sheet+"\",\"guid\":\""+guid+"\",\"localId\":\""+localId+"\",\"oldNpcBindingsPreserved\":true,\"oldSliceIdsPreserved\":true}");
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
                entry.status="EXISTING_USER_SOURCE_ROLE_MATCH; NOT_CANONICAL_IDENTITY_APPROVAL_PENDING; native_slice_no_bitmap_copy";
            }
            art.npcs=npcs.ToArray();
            foreach(var entry in before)if(art.Npc(entry.Key).sprite!=entry.Value)throw new InvalidDataException("Existing NPC identity changed: "+entry.Key);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();ArtLibrarySetup.Export();
            Debug.Log("Portrait repair: reused existing Ba Tu slice; one native Phuc slice; all existing NPC and sheet IDs preserved.");
            SliceValidation.BuildWindows();
        }
    }
}
