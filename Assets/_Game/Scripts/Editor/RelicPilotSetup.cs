using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace LightBetweenCultures.Editor
{
    // Explicit command only. Never a global importer or a scene-generation command.
    public static class RelicPilotSetup
    {
        const string Root="Assets/_Game/Art/_SourceSheets/CulturalArtifacts/";
        [Serializable] sealed class NativeEntry {public string id,path,spriteName,guid,localId,status;public Rect rect;}
        [Serializable] sealed class NativeReport {public NativeEntry[] entries;public bool existingNpcBindingsPreserved;}
        static readonly List<NativeEntry> records=new List<NativeEntry>();
        static void Record(string id,Sprite s,string status)
        {AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s,out string guid,out long local);records.Add(new NativeEntry{id=id,path=AssetDatabase.GetAssetPath(s),spriteName=s.name,guid=guid,localId=local.ToString(),rect=s.rect,status=status});}
        static TextureImporter Settings(string path,bool multiple)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Sprite;t.spriteImportMode=multiple?SpriteImportMode.Multiple:SpriteImportMode.Single;
            t.spritePixelsPerUnit=100;t.alphaIsTransparency=true;t.mipmapEnabled=false;t.filterMode=FilterMode.Bilinear;t.maxTextureSize=2048;
            t.textureCompression=TextureImporterCompression.Uncompressed;t.crunchedCompression=false;
            t.spriteImportMode=multiple?SpriteImportMode.Multiple:SpriteImportMode.Single;return t;
        }
        static AdventureImage ImageEntry(string id,Sprite sprite,string source,string status)=>new AdventureImage{id=id,sprite=sprite,portrait=sprite,source=source,status=status};
        static string Ref(Sprite sprite){if(sprite==null)return "null";AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite,out string guid,out long id);return guid+":"+id;}
        public static void ImportArt()
        {
            records.Clear();var art=AdventureUIArt.Load();var before=art.npcs.ToDictionary(n=>n.id,n=>Ref(n.sprite)+"/"+Ref(n.portrait)+"/"+n.status);
            var npcs=art.npcs.ToList();
            foreach(var pair in new[]{("npc_engineer","Seoul/NPC_Seoul_Engineer.png"),("npc_curator","Seoul/NPC_Seoul_Curator.png"),("npc_keeper","Gyeongju/NPC_Gyeongju_Keeper.png"),("npc_inscription","Gyeongju/NPC_Gyeongju_Inscription.png")})
            {
                var path="Assets/_Game/Art/Characters/NPC/"+pair.Item2;var t=Settings(path,false);t.SaveAndReimport();
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null)throw new InvalidDataException(path);
                var npc=npcs.FirstOrDefault(n=>n.id==pair.Item1);if(npc==null){npc=new AdventureImage{id=pair.Item1};npcs.Add(npc);}
                npc.sprite=npc.portrait=sprite;npc.source=path;npc.status="ART_IMPORTED_PENDING_APPROVAL; NOT_CANONICAL; technical binding authorized 2026-10-18";
                Record(pair.Item1,sprite,npc.status);
            }
            art.npcs=npcs.ToArray();
            foreach(var pair in before)if(Ref(art.Npc(pair.Key).sprite)+"/"+Ref(art.Npc(pair.Key).portrait)+"/"+art.Npc(pair.Key).status!=pair.Value)throw new InvalidDataException("Existing NPC modified: "+pair.Key);
            var catalog=JsonUtility.FromJson<RelicCatalog>(File.ReadAllText("Assets/_Game/Resources/Content/CulturalRelics.json"));
            var relicImages=new List<AdventureImage>();var fragments=new List<AdventureImage>();
            foreach(var city in new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"})
            {
                var r=catalog.relics.Single(v=>v.cityId==city);var suffix=city=="hoi_an"?"HoiAn_Lantern":city=="hue"?"Hue_DragonSeal":city=="da_lat"?"DaLat_MistCompass":city=="jeonju"?"Jeonju_HanjiSeal":city=="gyeongju"?"Gyeongju_StarlightLens":"Seoul_ThreeRelayPrism";
                var path=Root+"Artifact_"+suffix+"_Source.png";var t=Settings(path,true);
                var factories=new SpriteDataProviderFactories();factories.Init();var provider=factories.GetSpriteEditorDataProviderFromObject(t);provider.InitSpriteEditorDataProvider();
                var old=provider.GetSpriteRects().ToDictionary(s=>s.name);var rects=new List<SpriteRect>();
                var boxes=Boxes(city);var names=new[]{r.id}.Concat(r.fragmentIds).ToArray();
                for(var i=0;i<5;i++)
                {
                    if(city=="seoul"&&i>0&&i<4)continue; // The three mismatched signal icons stay unbound ART_PENDING.
                    var box=boxes[i];var sr=old.TryGetValue(names[i],out var previous)?previous:new SpriteRect{name=names[i],spriteID=GUID.Generate()};
                    sr.rect=new Rect(box.x,1254-box.y-box.height,box.width,box.height);sr.pivot=new Vector2(.5f,.5f);sr.alignment=SpriteAlignment.Center;rects.Add(sr);
                }
                provider.SetSpriteRects(rects.ToArray());provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
                // Narrow the tassel region of the complete lantern/lens/prism so
                // overlapping component rows never bleed into the full sprite.
                var outline=FullOutline(city);
                if(outline!=null)
                {var sr=rects[0];var b=boxes[0];provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(sr.spriteID,new List<Vector2[]>{outline.Select(v=>new Vector2(v.x-b.x-b.width*.5f,b.y+b.height-v.y-b.height*.5f)).ToArray()});}
                provider.Apply();t.SaveAndReimport();
                foreach(var sr in rects)
                {var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s=>s.name==sr.name);Record(sr.name,sprite,"NATIVE_CROP_TECHNICAL_IMPORT; VISUAL_REVIEW_REQUIRED");var entry=ImageEntry(sr.name,sprite,path,"NATIVE_CROP; pending runtime visual review");if(sr.name==r.id)relicImages.Add(entry);else fragments.Add(entry);}
                foreach(var previous in old)if(rects.Any(s=>s.name==previous.Key)&&rects.First(s=>s.name==previous.Key).spriteID!=previous.Value.spriteID)throw new InvalidDataException("Slice ID changed");
            }
            art.relics=relicImages.ToArray();art.fragments=fragments.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Docs/Gameplay/RelicPilot-20261018");File.WriteAllText("Docs/Gameplay/RelicPilot-20261018/native-art.json",JsonUtility.ToJson(new NativeReport{entries=records.ToArray(),existingNpcBindingsPreserved=true},true));
            ArtLibrarySetup.Export();Debug.Log("RELIC IMPORT PASS: 4 new NPC pending approval; 6 full relics +21 matched fragments; 3 Seoul slots ART_PENDING; old NPC bindings preserved.");
        }
        static Rect[] Boxes(string c)
        {
            if(c=="hoi_an")return new[]{new Rect(330,0,590,866),new Rect(40,810,280,435),new Rect(350,850,275,404),new Rect(650,870,310,360),new Rect(960,875,294,379)};
            if(c=="hue")return new[]{new Rect(290,0,920,825),new Rect(405,840,310,410),new Rect(10,830,390,400),new Rect(660,825,335,429),new Rect(1000,880,250,350)};
            if(c=="da_lat")return new[]{new Rect(300,0,755,785),new Rect(28,790,382,440),new Rect(410,790,280,442),new Rect(700,780,240,470),new Rect(950,790,304,450)};
            if(c=="jeonju")return new[]{new Rect(300,0,650,895),new Rect(380,850,255,390),new Rect(635,850,310,404),new Rect(0,830,385,420),new Rect(930,840,324,410)};
            if(c=="gyeongju")return new[]{new Rect(300,0,690,905),new Rect(630,905,340,340),new Rect(30,875,275,365),new Rect(980,865,260,380),new Rect(305,825,320,429)};
            return new[]{new Rect(300,0,710,880),new Rect(),new Rect(),new Rect(),new Rect(890,825,360,425)};
        }
        static Vector2[] FullOutline(string c)
        {
            if(c=="hoi_an")return new[]{new Vector2(330,0),new Vector2(920,0),new Vector2(920,650),new Vector2(720,710),new Vector2(700,866),new Vector2(560,866),new Vector2(540,710),new Vector2(330,650)};
            if(c=="jeonju")return new[]{new Vector2(300,0),new Vector2(950,0),new Vector2(950,680),new Vector2(760,720),new Vector2(760,895),new Vector2(540,895),new Vector2(540,720),new Vector2(300,680)};
            if(c=="gyeongju")return new[]{new Vector2(300,0),new Vector2(990,0),new Vector2(990,670),new Vector2(830,700),new Vector2(830,905),new Vector2(600,905),new Vector2(600,700),new Vector2(300,670)};
            if(c=="seoul")return new[]{new Vector2(300,0),new Vector2(1010,0),new Vector2(1010,700),new Vector2(750,760),new Vector2(750,880),new Vector2(550,880),new Vector2(550,760),new Vector2(300,700)};
            return null;
        }
        public static void Build()
        {
            ArtLibrarySetup.Export();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Windows/LightBetweenCultures.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
            Debug.Log("RELIC WINDOWS BUILD: "+report.summary.result);if(report.summary.result!=BuildResult.Succeeded)EditorApplication.Exit(1);
        }
    }
}
