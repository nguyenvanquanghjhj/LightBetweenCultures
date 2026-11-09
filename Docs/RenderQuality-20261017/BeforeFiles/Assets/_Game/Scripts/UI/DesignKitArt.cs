using UnityEngine;

namespace LightBetweenCultures
{
    [CreateAssetMenu(menuName = "Light Between Cultures/Design Kit Art")]
    public sealed class DesignKitArt : ScriptableObject
    {
        [Header("Lumi presentation roles")]
        [Tooltip("Full hand-preserving navy Lumi illustration, only when Lumi speaks.")]
        public Sprite lumiPortrait;
        [Tooltip("Standing chibi from the actions sheet; H3 only, not the explaining pose.")]
        public Sprite lumiExploration;
        [Tooltip("White chibi from Characters_Moodboard, independent of the legacy badges.")]
        public Sprite lumiMapTraveler;
        public Sprite maiPortrait;
        [Tooltip("Large badge variant. Never use this badge as the exploration character.")]
        public Sprite lumiCity;
        [Tooltip("Legacy circular badge. Unused by City Map and H3.")]
        public Sprite lumiCitySmall;
        public Sprite hoiAnMap;
        public Sprite available, current, visited, locked;
        public Sprite npc, quest, item, enemy, puzzle, portal, secret;

        public static DesignKitArt Load() => Resources.Load<DesignKitArt>("DesignKitArt");
        public static readonly Color Ink = Hex("224C65");
        public static readonly Color Cream = Hex("FFF3D9");
        public static readonly Color Gold = Hex("E7A53F");
        public static readonly Color Wall = Hex("F4C368");
        public static readonly Color Teal = Hex("367F79");
        public static readonly Color Roof = Hex("D15C3D");
        public static Color Hex(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var color); return color; }

        public Sprite Kind(string kind)
        {
            switch (kind)
            {
                case "npc": return npc;
                case "exploration": return item;
                case "item": return item;
                case "puzzle": return puzzle;
                case "bot": case "enemy": case "boss": return enemy;
                case "quest": return quest;
                case "portal": return portal;
                case "secret": return secret;
                default: return available;
            }
        }
    }
}
