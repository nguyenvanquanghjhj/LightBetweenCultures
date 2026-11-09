using System;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public sealed class AdventureImage
    {
        public string id, description, aliasOf, source, status;
        public Sprite sprite, portrait;
        public bool categoryIcon;
        // Opt in only for reviewed outline masks separating adjacent sheet actors.
        public bool portraitUseSpriteMesh;
    }

    [Serializable] public sealed class MapActorDefinition
    {
        public string cityId, nodeId, artId;
        public Vector2 size, offset;
        public bool hideWhenResolved;
        [Tooltip("Zero inherits the city/global map actor scale. Does not affect portraits or combat.")]
        public float scaleOverride;
    }

    [Serializable] public sealed class MapActorStyle
    {
        public string cityId;
        public float scale;
    }

    [CreateAssetMenu(menuName="Light Between Cultures/Adventure UI Art")]
    public sealed class AdventureUIArt : ScriptableObject
    {
        public AdventureImage[] cities = Array.Empty<AdventureImage>();
        public AdventureImage[] items = Array.Empty<AdventureImage>();
        public AdventureImage[] npcs = Array.Empty<AdventureImage>();
        public AdventureImage[] enemies = Array.Empty<AdventureImage>();
        public MapActorDefinition[] mapActors = Array.Empty<MapActorDefinition>();
        [Header("Shared City Map actor presentation; zero city/actor overrides inherit")]
        [Range(.1f,2)] public float mapActorScale = .72f;
        public MapActorStyle[] mapActorStyles = Array.Empty<MapActorStyle>();
        public float mapActorTouchSize = 52;
        public Sprite backpack, coin;
        [Header("Illustrated UI-1 skin; existing content art remains shared")]
        public Sprite globe, journal, backdrop, window, buttonTeal, buttonPaper, slot, postcard;
        [Header("Standalone marker sprites from official LBC marker references")]
        public AdventureImage[] markers = Array.Empty<AdventureImage>();
        public static AdventureUIArt Load() => Resources.Load<AdventureUIArt>("AdventureUIArt");
        public AdventureImage City(string id) => Array.Find(cities, entry => entry.id == id);
        public AdventureImage Item(string id) => Array.Find(items, entry => entry.id == id);
        public AdventureImage Npc(string id) => Array.Find(npcs, entry => entry.id == id);
        public AdventureImage Enemy(string id) => Array.Find(enemies, entry => entry.id == id);
        public MapActorDefinition MapActor(string cityId,string nodeId) => Array.Find(mapActors,entry=>entry.cityId==cityId&&entry.nodeId==nodeId);
        public float ActorScale(string cityId,MapActorDefinition actor)
        {
            var style=Array.Find(mapActorStyles,entry=>entry.cityId==cityId);
            return Mathf.Clamp(actor!=null&&actor.scaleOverride>0 ? actor.scaleOverride : style!=null&&style.scale>0 ? style.scale : mapActorScale,.1f,2);
        }
        public Vector2 ActorSize(string cityId,MapActorDefinition actor)=>actor.size*ActorScale(cityId,actor);
        public Sprite ActorSprite(string id) => Npc(id)?.sprite ?? Enemy(id)?.sprite;
        public Sprite Marker(string id) => Array.Find(markers,entry=>entry.id==id)?.sprite;
    }
}
