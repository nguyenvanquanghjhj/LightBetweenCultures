using UnityEngine;
namespace LightBetweenCultures
{
    public sealed class SoftMusicClips : ScriptableObject
    {
        // Vietnam map, Vietnam battle, Korea map, Korea battle.
        public AudioClip[] clips;
        public float[] loopStart,loopEnd;
    }
}
