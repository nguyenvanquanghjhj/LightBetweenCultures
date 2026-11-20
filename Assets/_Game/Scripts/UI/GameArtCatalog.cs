using System;
using UnityEngine;

namespace LightBetweenCultures
{
    // Only this small registry is loaded through Resources; all textures live under Art.
    [CreateAssetMenu(menuName = "Light Between Cultures/Game Art Catalog")]
    public sealed class GameArtCatalog : ScriptableObject
    {
        [Serializable] public sealed class Image
        {
            public string id;
            public Sprite sprite;
        }

        public Texture2D rescueCats;
        public Sprite dalatStation;
        public Image[] leisureProps = Array.Empty<Image>();

        public static GameArtCatalog Load() => Resources.Load<GameArtCatalog>("Catalogs/GameArtCatalog");
        public Sprite Leisure(string id) => Array.Find(leisureProps, entry => entry.id == id)?.sprite;
    }
}
