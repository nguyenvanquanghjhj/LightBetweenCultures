using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Additive native slices only: retain every existing sprite ID and bitmap.
    public static class ThreeNpcSetup
    {
        static void Bind(string path,string id,Rect topDown,Vector2[] outline)
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            var factory=new SpriteDataProviderFactories();factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer);provider.InitSpriteEditorDataProvider();
            var rects=provider.GetSpriteRects().ToList();
            var old=rects.ToDictionary(r=>r.name,r=>r.spriteID);
            var cut=rects.FirstOrDefault(r=>r.name==id);
            if(cut==null){cut=new SpriteRect{name=id,spriteID=GUID.Generate()};rects.Add(cut);}
            cut.rect=new Rect(topDown.x,1086-topDown.y-topDown.height,topDown.width,topDown.height);
            cut.alignment=SpriteAlignment.Center;cut.pivot=new Vector2(.5f,.5f);
            provider.SetSpriteRects(rects.ToArray());
            provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID)));
            provider.GetDataProvider<ISpriteOutlineDataProvider>().SetOutlines(cut.spriteID,new List<Vector2[]>{outline.Select(p=>new Vector2(p.x-topDown.center.x,topDown.center.y-p.y)).ToArray()});
            provider.Apply();importer.SaveAndReimport();
            foreach(var r in provider.GetSpriteRects())if(old.ContainsKey(r.name)&&old[r.name]!=r.spriteID)throw new InvalidDataException("Existing sprite ID changed: "+r.name);
            var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single(s=>s.name==id);
            var art=AdventureUIArt.Load();var entries=art.npcs.ToList();
            var entry=entries.FirstOrDefault(e=>e.id==id);
            if(entry==null){entry=new AdventureImage{id=id};entries.Add(entry);}
            entry.sprite=entry.portrait=sprite;entry.portraitUseSpriteMesh=true;
            entry.source=path;entry.status="OWNER_AUTHORIZED_UNUSED_ART_CAST_20261024; native outline; map and dialogue";
            art.npcs=entries.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
        }
        public static void Apply()
        {
            Bind("Assets/_Game/Art/Characters/NPC/Shared/NPC_TransparentAtlas.png","npc_kang_taeho",new Rect(318,140,296,420),new[]{new Vector2(320,140),new Vector2(614,140),new Vector2(614,560),new Vector2(318,560)});
            const string sheet="Assets/_Game/Art/_SourceSheets/H3_Upgrade_20261016/H3_07_Characters_NPC_Concept.png";
            Bind(sheet,"npc_choi_eunsu",new Rect(807,170,349,395),new[]{new Vector2(855,255),new Vector2(864,225),new Vector2(1022,174),new Vector2(1114,202),new Vector2(1116,248),new Vector2(1081,284),new Vector2(1085,314),new Vector2(1134,323),new Vector2(1155,397),new Vector2(1144,461),new Vector2(1105,510),new Vector2(1052,526),new Vector2(1035,560),new Vector2(936,560),new Vector2(922,537),new Vector2(855,537),new Vector2(818,480),new Vector2(809,420),new Vector2(825,359),new Vector2(852,327)});
            Bind(sheet,"npc_han_doyun",new Rect(1126,150,322,410),new[]{new Vector2(1144,150),new Vector2(1340,150),new Vector2(1340,212),new Vector2(1448,212),new Vector2(1448,430),new Vector2(1360,430),new Vector2(1382,500),new Vector2(1354,560),new Vector2(1200,560),new Vector2(1180,496),new Vector2(1140,460),new Vector2(1126,350),new Vector2(1144,283)});
            var art=AdventureUIArt.Load();var actors=art.mapActors.ToList();
            AddMapActor(actors,"gyeongju","G16","npc_kang_taeho",new Vector2(22,5));
            AddMapActor(actors,"gyeongju","G21","npc_choi_eunsu",new Vector2(24,5));
            AddMapActor(actors,"seoul","S12","npc_han_doyun",new Vector2(-24,5));
            art.mapActors=actors.ToArray();EditorUtility.SetDirty(art);AssetDatabase.SaveAssets();
            ArtLibrarySetup.Export();
            Debug.Log("THREE NPC NATIVE BINDINGS: PASS");
        }
        static void AddMapActor(List<MapActorDefinition> actors,string city,string node,string npc,Vector2 offset)
        {
            var actor=actors.FirstOrDefault(a=>a.cityId==city&&a.nodeId==node);
            if(actor==null){actor=new MapActorDefinition{cityId=city,nodeId=node};actors.Add(actor);}
            actor.artId=npc;actor.size=new Vector2(52,62);actor.offset=offset;
            actor.hideWhenResolved=false;actor.hasMapPosition=false;actor.markerClearance=16;
        }
        public static void BuildWindows()
        {
            Apply();
            Phase5Setup.Build();
        }
    }
}
