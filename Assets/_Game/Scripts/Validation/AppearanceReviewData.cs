#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
namespace LightBetweenCultures
{
    // Temporary development-build evidence only; never a gameplay registry.
    public sealed class AppearanceReviewData:ScriptableObject
    {
        public string[] npcIds;
        public Sprite[] beforeSprites,beforePortraits;
        public Sprite beforeLumiPortrait,beforeLumiExploration,beforeLumiMap;
        public string mapActorsBefore;
    }
}
#endif
