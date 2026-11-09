using UnityEngine;

namespace LightBetweenCultures.HoiAn
{
    public sealed class HoiAnNpc : MonoBehaviour
    {
        public string Talk() => GameBootstrap.Ensure().Quests.TalkToMai();
        public bool IsPlayerNearby { get; private set; }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<HoiAnPlayerController>() != null)
                IsPlayerNearby = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<HoiAnPlayerController>() != null)
                IsPlayerNearby = false;
        }
    }
}
