using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class AdventureUISetup
    {
        const string Root="Assets/_Game/Art/Runtime/AdventureUI/";
        [Serializable] sealed class Presentation {public PresentationEntry[] items,cities;public MapActorDefinition[] mapActors;public float mapActorScale=.72f,mapActorTouchSize=52;public MapActorStyle[] mapActorStyles=Array.Empty<MapActorStyle>();}
        [Serializable] sealed class PresentationEntry {public string id,icon,description,aliasOf;public bool categoryIcon;}
        [MenuItem("Tools/Light Between Cultures/Import Adventure UI")]
        public static void Apply()
        {
            var path="Assets/_Game/Resources/AdventureUIArt.asset";
            var art=LoadArt<AdventureUIArt>(path);
            if(art==null){art=ScriptableObject.CreateInstance<AdventureUIArt>();AssetDatabase.CreateAsset(art,path);}
            var atlas=Root+"Items_TransparentAtlas.png";var importer=Configure(atlas,true);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var old=provider.GetSpriteRects();
            // Pixel rectangles on the derived 1448x1086 atlas; source concept is untouched.
            var boxes=new[]{
                Box("lantern",18,98,241,279),Box("crystal",329,140,181,229),Box("key",579,135,258,235),
                Box("glasses",840,177,320,165),Box("meat",1162,128,281,250),
                Box("scroll",2,433,313,249),Box("coin",326,472,207,205),Box("backpack",538,438,293,242),
                Box("chest",831,465,347,221),Box("post",1178,393,269,293),
                Box("lever",0,721,261,291),Box("portal",255,712,292,302),Box("barrel",547,756,209,258),
                Box("sign",749,719,229,295),Box("flowers",977,742,233,272),Box("mailbox",1208,712,240,302)};
            foreach(var box in boxes){var previous=old.FirstOrDefault(s=>s.name==box.name);box.spriteID=previous?.spriteID??GUID.Generate();}
            provider.SetSpriteRects(boxes);
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(boxes.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            var sprites=LoadAllArt(atlas).OfType<Sprite>().ToDictionary(s=>s.name);
            foreach(var name in new[]{"Hanji_Core","Copper_Wire","Healing_Tea"})
            {Configure(Root+name+".png",false);sprites[name]=LoadArt<Sprite>(Root+name+".png");}
            var presentation=JsonUtility.FromJson<Presentation>(File.ReadAllText("Assets/_Game/Resources/Content/AdventurePresentation.json"));
            art.items=presentation.items.Select(p=>new AdventureImage{id=p.id,description=p.description,aliasOf=p.aliasOf,categoryIcon=p.categoryIcon,sprite=sprites.TryGetValue(p.icon,out var icon)?icon:art.Fragment(p.icon)}).ToArray();
            art.cities=presentation.cities.Select(p=>
            {
                var file=Root+"City_"+p.id+".png";var texture=Configure(file,false);
                // Destination cards are much smaller than the original paintings.
                // Mip levels suppress distracting shimmer without modifying source pixels.
                texture.mipmapEnabled=true;texture.filterMode=FilterMode.Trilinear;texture.maxTextureSize=1024;texture.SaveAndReimport();
                return new AdventureImage{id=p.id,description=p.description,sprite=LoadArt<Sprite>(file)};
            }).ToArray();
            art.backpack=sprites["backpack"];art.coin=sprites["coin"];
            var npcAtlas=Root+"NPC_TransparentAtlas.png";var npcImporter=Configure(npcAtlas,true);
            var npcProvider=factory.GetSpriteEditorDataProviderFromObject(npcImporter);npcProvider.InitSpriteEditorDataProvider();
            var oldGuest=npcProvider.GetSpriteRects().FirstOrDefault(s=>s.name=="npc_guest");
            var guest=Box("npc_guest",1164,586,280,419);guest.spriteID=oldGuest?.spriteID??GUID.Generate();
            var npcRects=npcProvider.GetSpriteRects().Where(s=>s.name!="npc_guest").ToList();npcRects.Add(guest);
            npcProvider.SetSpriteRects(npcRects.ToArray());
            npcProvider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(npcRects.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            npcProvider.Apply();npcImporter.SaveAndReimport();
            Configure(Root+"Boatman_Portrait.png",false);
            var preservedNpcs=art.npcs.Where(n=>n.id!="npc_guest"&&n.id!="npc_boatman").ToList();
            preservedNpcs.Add(new AdventureImage{id="npc_guest",sprite=LoadAllArt(npcAtlas).OfType<Sprite>().First(s=>s.name=="npc_guest")});
            preservedNpcs.Add(new AdventureImage{id="npc_boatman",sprite=LoadArt<Sprite>(Root+"Boatman_Portrait.png")});art.npcs=preservedNpcs.ToArray();
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("Adventure UI imported: six clean city previews, sixteen extracted props and three new component/tea icons. Original art and scenes untouched.");
        }
        static SpriteRect Box(string name,int x,int y,int width,int height)=>new SpriteRect{name=name,rect=new Rect(x,1086-y-height,width,height),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};
        static T LoadArt<T>(string path) where T:UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>(ArtLibrarySetup.ResolveMovedPath(path));
        static UnityEngine.Object[] LoadAllArt(string path)=>AssetDatabase.LoadAllAssetsAtPath(ArtLibrarySetup.ResolveMovedPath(path));
        static TextureImporter Configure(string path,bool multiple)
        {
            path=ArtLibrarySetup.ResolveMovedPath(path);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;
            importer.spriteImportMode=multiple?SpriteImportMode.Multiple:SpriteImportMode.Single;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.npotScale=TextureImporterNPOTScale.None;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;importer.SetTextureSettings(settings);
            importer.SaveAndReimport();return importer;
        }
        public static void BuildWindows(){Apply();SliceValidation.BuildWindows();}
        [MenuItem("Tools/Light Between Cultures/Import UI-1 Skin Only")]
        public static void ApplyTheme()
        {
            var art=LoadArt<AdventureUIArt>("Assets/_Game/Resources/AdventureUIArt.asset");
            if(art==null)throw new InvalidDataException("Existing AdventureUIArt catalog is required.");
            var path=Root+"Theme/LBC_UI_SkinAtlas.png";var importer=Configure(path,true);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();var old=provider.GetSpriteRects();
            var entries=new[]{SkinRect("backdrop",58,54,435,420),SkinRect("window",549,53,439,420),SkinRect("buttonTeal",1044,53,440,420),
                SkinRect("buttonPaper",56,525,441,414),SkinRect("slot",550,523,439,416),SkinRect("postcard",1046,521,435,419)};
            foreach(var entry in entries){entry.spriteID=old.FirstOrDefault(s=>s.name==entry.name)?.spriteID??GUID.Generate();if(entry.name=="backdrop")entry.border=new Vector4(160,160,160,160);}
            provider.SetSpriteRects(entries);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(entries.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            var skins=LoadAllArt(path).OfType<Sprite>().ToDictionary(s=>s.name);
            art.backdrop=skins["backdrop"];art.window=skins["window"];art.buttonTeal=skins["buttonTeal"];art.buttonPaper=skins["buttonPaper"];art.slot=skins["slot"];art.postcard=skins["postcard"];
            var globe=Root+"Theme/World_Globe.png";Configure(globe,false);art.globe=LoadArt<Sprite>(globe);
            art.journal=art.Item("river_pass").sprite;
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();Debug.Log("UI-1 skin imported without changing existing item, city or NPC sprite assignments.");
        }
        static SpriteRect SkinRect(string name,int x,int y,int width,int height)=>new SpriteRect{name=name,rect=new Rect(x,1024-y-height,width,height),border=new Vector4(76,76,76,76),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};
        public static void BuildUI1Windows(){ApplyTheme();SliceValidation.BuildWindows();}

        [MenuItem("Tools/Light Between Cultures/Import Map Actors Only")]
        public static void ApplyMapActors()
        {
            var art=LoadArt<AdventureUIArt>("Assets/_Game/Resources/AdventureUIArt.asset");
            if(art==null)throw new InvalidDataException("Existing AdventureUIArt catalog is required.");
            var atlas=Root+"Enemies_HoiAn_TransparentAtlas.png";var importer=Configure(atlas,true);
            importer.maxTextureSize=4096;
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();var old=provider.GetSpriteRects();
            var boxes=new[]{ActorRect("enemy_ghost",32,8,600,700,736),ActorRect("enemy_crab",640,150,692,554,736),ActorRect("enemy_mo01",1420,8,680,700,736)};
            foreach(var box in boxes)box.spriteID=old.FirstOrDefault(s=>s.name==box.name)?.spriteID??GUID.Generate();
            provider.SetSpriteRects(boxes);provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(boxes.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            provider.Apply();importer.SaveAndReimport();
            var sprites=LoadAllArt(atlas).OfType<Sprite>().ToDictionary(s=>s.name);
            const string source="LBC_AnhMoi_BanGiaoCodex_20261015/Characters/Enemies_Roster_Concept.png";
            art.enemies=sprites.Select(s=>new AdventureImage{id=s.Key,sprite=s.Value,portrait=s.Value,source=source,status="derived_static_cutout_map_verified; combat_redesign_pending"}).ToArray();
            // The old atlas has adjacent oar pixels inside the merchant's bounding
            // rectangle. Use a separate clean derivative; keep all old atlas metadata.
            var merchantPath=Root+"Market_Merchant.png";Configure(merchantPath,false);
            var npcs=art.npcs.ToList();
            if(!npcs.Any(n=>n.id=="npc_mai"))npcs.Add(new AdventureImage{id="npc_mai",sprite=DesignKitArt.Load().maiPortrait,source="Characters/NPC_Roster_Concept.png",status="existing_approved_static_cutout"});
            var merchant=npcs.FirstOrDefault(n=>n.id=="npc_market_merchant");
            if(merchant==null){merchant=new AdventureImage{id="npc_market_merchant"};npcs.Add(merchant);}
            merchant.sprite=LoadArt<Sprite>(merchantPath);merchant.source="Characters/NPC_Roster_Concept.png";merchant.status="derived_static_cutout_map_verified";
            art.npcs=npcs.ToArray();
            ApplyMapActorPresentation();
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            Debug.Log("Map actors imported: 3 static enemy cutouts, reused Mai/merchant/boatman/guest. Existing themes, scenes and gameplay remain unchanged.");
        }
        static SpriteRect ActorRect(string name,int x,int y,int width,int height,int canvasHeight)=>new SpriteRect{name=name,rect=new Rect(x,canvasHeight-y-height,width,height),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)};
        public static void BuildMapActorsWindows(){ApplyMapActors();SliceValidation.BuildWindows();}
        [MenuItem("Tools/Light Between Cultures/Apply Map Actor Presentation Only")]
        public static void ApplyMapActorPresentation()
        {
            var art=AdventureUIArt.Load();
            var config=JsonUtility.FromJson<Presentation>(File.ReadAllText("Assets/_Game/Resources/Content/AdventurePresentation.json"));
            art.mapActors=config.mapActors;art.mapActorScale=config.mapActorScale;
            art.mapActorStyles=config.mapActorStyles;art.mapActorTouchSize=Mathf.Max(52,config.mapActorTouchSize);
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }
        public static void BuildMapPresentationWindows(){ApplyMapActorPresentation();SliceValidation.BuildWindows();}
        [MenuItem("Tools/Light Between Cultures/Import Marker Sprites Only")]
        public static void ApplyMarkerSprites()
        {
            var art=LoadArt<AdventureUIArt>("Assets/_Game/Resources/AdventureUIArt.asset");
            var ids=new[]{"Ground","Current","NpcTalk","NpcNeedItem","BossAlert","Camera","Quest","Puzzle","Reward","Completed"};
            art.markers=ids.Select(id=>
            {
                var path=Root+"Markers/"+id+".png";var importer=Configure(path,false);
                importer.maxTextureSize=256;importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.SaveAndReimport();
                return new AdventureImage{id=id,sprite=LoadArt<Sprite>(path),source="References/MarkerDesign/Marker_Design_01.png; Marker_Design_02.png",status="independent_generated_derivative_transparent"};
            }).ToArray();
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }
        public static void BuildMarkerSpritesWindows(){ApplyMarkerSprites();SliceValidation.BuildWindows();}
        [MenuItem("Tools/Light Between Cultures/Import Specialty Items Only")]
        public static void ApplySpecialtyItems()
        {
            var art=LoadArt<AdventureUIArt>("Assets/_Game/Resources/AdventureUIArt.asset");
            foreach(var name in new[]{"Item_HoiAn_Chili","Item_HoiAn_Shallot"})Configure("Assets/_Game/Art/Item/"+name+".jpg",false);
            var chili="Assets/_Game/Art/Item/Derived/Item_HoiAn_Chili.png";var shallot="Assets/_Game/Art/Item/Derived/Item_HoiAn_Shallot.png";
            Configure(chili,false);Configure(shallot,false);
            var paper=LoadAllArt(Root+"Items_TransparentAtlas.png").OfType<Sprite>().First(s=>s.name=="scroll");
            var icons=new System.Collections.Generic.Dictionary<string,Sprite>{{"food_chili",LoadArt<Sprite>(chili)},{"food_shallot",LoadArt<Sprite>(shallot)},{"postcard_food",paper},{"scroll",art.Item("reply_letter").sprite}};
            var ids=new[]{"hoi_an_fresh_chili","hoi_an_purple_shallot","festival_friendship_postcard","hoi_an_bridge_postcard","jeonju_recipe_card","hue_market_note"};
            var presentation=JsonUtility.FromJson<Presentation>(File.ReadAllText("Assets/_Game/Resources/Content/AdventurePresentation.json"));
            var items=art.items.ToList();
            foreach(var id in ids)
            {
                var entry=presentation.items.First(p=>p.id==id);var image=items.FirstOrDefault(i=>i.id==id);
                if(image==null){image=new AdventureImage{id=id};items.Add(image);}
                image.sprite=icons[entry.icon];image.description=entry.description;image.categoryIcon=entry.categoryIcon||entry.icon=="postcard_food";
                image.source="Assets/_Game/Art/Item/"+(entry.icon=="food_chili"?"Item_HoiAn_Chili.jpg":entry.icon=="food_shallot"?"Item_HoiAn_Shallot.jpg":entry.icon=="postcard_food"?"Item_Food_Postcard.jpg":"existing scroll sprite");
                image.status=entry.icon.StartsWith("food_")?"transparent_derivative_flat_style_temporary":entry.icon=="postcard_food"?"NEEDS_ART_TEMPORARY_CATEGORY_ICON_CLEAN_PACK_SCROLL":"shared_category_icon_pending_destination";
                if(entry.icon=="postcard_food")image.source=AssetDatabase.GetAssetPath(paper);
            }
            art.items=items.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }
        public static void BuildSpecialtyItemsWindows(){ApplySpecialtyItems();SliceValidation.BuildWindows();}
        public static void ApplyPostcardSafetyAndBuild()
        {
            var art=LoadArt<AdventureUIArt>("Assets/_Game/Resources/AdventureUIArt.asset");
            var paper=LoadAllArt(Root+"Items_TransparentAtlas.png").OfType<Sprite>().First(s=>s.name=="scroll");
            foreach(var id in new[]{"festival_friendship_postcard","hoi_an_bridge_postcard"})
            {
                var image=art.Item(id);if(image==null)throw new InvalidDataException(id);
                image.sprite=paper;image.categoryIcon=true;image.source=AssetDatabase.GetAssetPath(paper);
                image.status="NEEDS_ART_TEMPORARY_CATEGORY_ICON_CLEAN_PACK_SCROLL";
            }
            EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(paper,out string guid,out long localId);
            Directory.CreateDirectory("Docs/QualityAddendum-20261017");
            File.WriteAllText("Docs/QualityAddendum-20261017/postcard-binding.json","{\"path\":\""+AssetDatabase.GetAssetPath(paper)+"\",\"guid\":\""+guid+"\",\"localId\":\""+localId+"\",\"spriteName\":\"scroll\",\"status\":\"NEEDS_ART_TEMPORARY_CATEGORY_ICON_CLEAN_PACK_SCROLL\"}");
            H3ArtSetup.VerifyLegacyBuilderGuard();SliceValidation.BuildWindows();
        }
        [Serializable] sealed class CrossCitySlices {public string atlas;public CrossCitySlice[] slices;}
        [Serializable] sealed class CrossCitySlice {public string id,status;public int x,y,width,height;public Vector2[] outline;}
        [MenuItem("Tools/Light Between Cultures/Import Cross-City Opening Presentation")]
        public static void ApplyCrossCityOpening()
        {
            ApplyNpcSlices("Docs/Art/CrossCitySpriteSlices.json");
            foreach(var id in new[]{"hue","jeonju"})
            {
                var mapPath=AssetDatabase.GetAssetPath(AdventureUIArt.Load().City(id).sprite);var mapImporter=(TextureImporter)AssetImporter.GetAtPath(mapPath);
                mapImporter.maxTextureSize=2048;mapImporter.SaveAndReimport();
            }
            ApplyMapActorPresentation();ArtLibrarySetup.Export();
        }
        static void ApplyNpcSlices(string configPath)
        {
            var config=JsonUtility.FromJson<CrossCitySlices>(File.ReadAllText(configPath));
            var importer=(TextureImporter)AssetImporter.GetAtPath(config.atlas);
            var factory=new SpriteDataProviderFactories();factory.Init();var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var existing=provider.GetSpriteRects().ToList();
            foreach(var slice in config.slices)
            {
                var rect=existing.FirstOrDefault(s=>s.name==slice.id);
                if(rect==null){rect=new SpriteRect{name=slice.id,spriteID=GUID.Generate()};existing.Add(rect);}
                rect.rect=new Rect(slice.x,1086-slice.y-slice.height,slice.width,slice.height);rect.alignment=SpriteAlignment.Center;rect.pivot=new Vector2(.5f,.5f);
            }
            provider.SetSpriteRects(existing.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(existing.Select(s=>new SpriteNameFileIdPair(s.name,s.spriteID)));
            var outlines=provider.GetDataProvider<ISpriteOutlineDataProvider>();
            foreach(var slice in config.slices)outlines.SetOutlines(existing.First(s=>s.name==slice.id).spriteID,new System.Collections.Generic.List<Vector2[]>{slice.outline});
            provider.Apply();
            var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.Tight;importer.SetTextureSettings(settings);importer.SaveAndReimport();
            var sprites=AssetDatabase.LoadAllAssetsAtPath(config.atlas).OfType<Sprite>().ToDictionary(s=>s.name);
            var art=AdventureUIArt.Load();var npcs=art.npcs.ToList();
            foreach(var slice in config.slices)
            {
                var entry=npcs.FirstOrDefault(n=>n.id==slice.id);if(entry==null){entry=new AdventureImage{id=slice.id};npcs.Add(entry);}
                entry.sprite=entry.portrait=sprites[slice.id];entry.source=config.atlas;entry.status=slice.status;
            }
            art.npcs=npcs.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }
        public static void BuildCrossCityOpeningWindows(){ApplyCrossCityOpening();SliceValidation.BuildWindows();}
        [Serializable] sealed class ChapterArt {public ChapterArtEntry[] entries;}
        [Serializable] sealed class ChapterArtEntry {public string id,path,source,status;public int[] dimensions,alphaBounds;}
        [MenuItem("Tools/Light Between Cultures/Import Hue Jeonju Chapter Art")]
        public static void ApplyChapters()
        {
            ApplyNpcSlices("Docs/Art/ChapterSpriteSlices.json");
            ApplyChapterArt("Docs/Art/HueJeonjuArtDerivatives.json");
            ArtLibrarySetup.Export();
        }
        static void ApplyChapterArt(string configPath)
        {
            var art=AdventureUIArt.Load();var config=JsonUtility.FromJson<ChapterArt>(File.ReadAllText(configPath));
            var npcs=art.npcs.ToList();var enemies=art.enemies.ToList();
            foreach(var e in config.entries)
            {
                var importer=Configure(e.path,true);
                importer.mipmapEnabled=true;importer.filterMode=FilterMode.Trilinear;importer.SaveAndReimport();
                var factories=new SpriteDataProviderFactories();factories.Init();
                var provider=factories.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
                var old=provider.GetSpriteRects().FirstOrDefault(v=>v.name==e.id);
                var box=new SpriteRect{name=e.id,spriteID=old?.spriteID??GUID.Generate(),alignment=SpriteAlignment.Center,pivot=new Vector2(.5f,.5f),
                    rect=new Rect(e.alphaBounds[0],e.dimensions[1]-e.alphaBounds[3],e.alphaBounds[2]-e.alphaBounds[0],e.alphaBounds[3]-e.alphaBounds[1])};
                provider.SetSpriteRects(new[]{box});provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(new[]{new SpriteNameFileIdPair(box.name,box.spriteID)});
                provider.Apply();importer.SaveAndReimport();
                var list=e.id.StartsWith("npc_")?npcs:enemies;var entry=list.FirstOrDefault(v=>v.id==e.id);
                if(entry==null){entry=new AdventureImage{id=e.id};list.Add(entry);}
                entry.sprite=entry.portrait=AssetDatabase.LoadAllAssetsAtPath(e.path).OfType<Sprite>().First(s=>s.name==e.id);entry.source=e.source;entry.status=e.status;
                if(entry.sprite==null)throw new InvalidDataException("Missing chapter sprite "+e.id);
            }
            art.npcs=npcs.ToArray();art.enemies=enemies.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            ApplyMapActorPresentation();
        }
        public static void BuildChaptersWindows(){ApplyChapters();SliceValidation.BuildWindows();}
        public static void ApplyDaLatChapter()
        {
            ApplyChapterArt("Docs/Art/DaLatArtDerivatives.json");
            const string sheet="Assets/_Game/Art/_SourceSheets/ItemSheet_VNKR_FoodCulture_v1.png";
            // Reuse the already extracted clean strawberry slice. Do not create
            // another SpriteRect for the same food icon.
            var art=AdventureUIArt.Load();var items=art.items.Where(i=>i.id!="da_lat_strawberry").ToList();
            items.Add(new AdventureImage{id="da_lat_strawberry",sprite=AssetDatabase.LoadAllAssetsAtPath(sheet).OfType<Sprite>().First(s=>s.name=="ItemSheet_VNKR_FoodCulture_v1_9"),source=sheet,status="EXISTING_NATIVE_ALPHA_SLICE_REUSED; PROPOSED_ITEM_NOT_CANONICAL"});
            art.items=items.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(AdventureUIArt.Load().City("da_lat").sprite));
            importer.maxTextureSize=2048;importer.SaveAndReimport();
            ArtLibrarySetup.Export();
        }
        public static void BuildDaLatWindows(){ApplyDaLatChapter();SliceValidation.BuildWindows();}
    }
}
