using UnityEngine;
namespace LightBetweenCultures
{
    public enum SoftCue { UI_Click, UI_Back, UI_Confirm, UI_Invalid, Item_Pickup, Relic_Place, Puzzle_Success }
    public sealed class SoftAudioClips : ScriptableObject
    {
        public AudioClip[] clips;
        public AudioClip Clip(SoftCue cue)=>clips!=null&&(int)cue<clips.Length?clips[(int)cue]:null;
    }
}
