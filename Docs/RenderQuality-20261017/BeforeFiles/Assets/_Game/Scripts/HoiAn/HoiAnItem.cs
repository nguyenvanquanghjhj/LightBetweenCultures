using UnityEngine;
namespace LightBetweenCultures.HoiAn
{
    public sealed class HoiAnItem : MonoBehaviour
    {
        public bool Collected => GameBootstrap.Ensure().Save.collected.Contains(GameBootstrap.Instance.Data.quest.item);
        public void Restore() => gameObject.SetActive(!Collected);
        public bool Collect()
        {
            if(!GameBootstrap.Ensure().Quests.CollectPart()) return false;
            gameObject.SetActive(false); return true;
        }
    }
}
