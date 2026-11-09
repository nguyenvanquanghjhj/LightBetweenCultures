using UnityEngine;
namespace LightBetweenCultures.HoiAn
{
    public sealed class HoiAnGameplay : MonoBehaviour
    {
        [SerializeField] private HoiAnPlayerController player;
        [SerializeField] private HoiAnItem lanternPart;
        public void Configure(HoiAnPlayerController p, HoiAnItem item) { player=p; lanternPart=item; }
        private void Start()
        {
            var game=GameBootstrap.Ensure(); game.Save.node=3; game.Persist();
            var ui=UIController.Create("H3 • HẺM MÁI NGÓI");
            ui.Say("Chạm mặt đất để đi; chạm linh kiện để tới gần và nhặt. PC: WASD / mũi tên, E.\n"+game.Quests.Status);
            lanternPart.Restore();
            var interaction=player.gameObject.AddComponent<InteractionSystem>(); interaction.Configure(player,lanternPart,ui);
            ui.Button(ui.Root,"Collect","Tương tác / Nhặt (E)",new Vector2(.05f,.10f),new Vector2(.95f,.18f),interaction.Interact);
            ui.Button(ui.Root,"Return","Về bản đồ H3",new Vector2(.22f,.01f),new Vector2(.78f,.08f),()=> { player.Stop(); game.Scenes.Go("CityMap"); });
        }
    }
}
