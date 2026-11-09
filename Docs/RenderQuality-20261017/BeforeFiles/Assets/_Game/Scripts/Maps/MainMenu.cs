using UnityEngine;

namespace LightBetweenCultures
{
    public sealed class MainMenu : MonoBehaviour
    {
        private void Start()
        {
            var game = GameBootstrap.Ensure();
            var ui = UIController.Create("LIGHT BETWEEN CULTURES\nNhịp Cầu Ánh Sáng", true);
            ui.Say("Khám phá Hội An cùng Lumi. " + game.Quests.Status);
            var hero = ui.Panel(ui.Root,"LumiCard",new Vector2(.12f,.34f),new Vector2(.88f,.76f),DesignKitArt.Hex("17384E"));
            hero.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            UIController.Border(hero,DesignKitArt.Teal);
            ui.Picture(hero,"CanonicalLumiPortrait",DesignKitArt.Load().lumiPortrait, new Vector2(.06f,.03f),new Vector2(.94f,.98f));
            ui.Button(ui.Root,"Start","Bắt đầu",new Vector2(.15f,.20f),new Vector2(.85f,.30f), () => game.Scenes.OpenWorld());
            ui.Button(ui.Root,"Reset","Đặt lại tiến độ",new Vector2(.15f,.07f),new Vector2(.85f,.15f), () => {
                var confirm = ui.Panel(ui.Root,"ResetConfirmation",new Vector2(.06f,.22f),new Vector2(.94f,.76f),new Color(.06f,.12f,.14f));
                ui.Label(confirm,"Prompt","Xóa tiến độ Hội An và chơi lại từ H0?",new Vector2(.05f,.55f),new Vector2(.95f,.94f));
                ui.Button(confirm,"ConfirmReset","Xóa save",new Vector2(.08f,.15f),new Vector2(.48f,.40f), () => { game.ResetSave(); ui.Say("Đã đặt lại. Lumi bắt đầu tại H0."); Destroy(confirm.gameObject); });
                ui.Button(confirm,"CancelReset","Hủy",new Vector2(.52f,.15f),new Vector2(.92f,.40f), () => Destroy(confirm.gameObject));
            });
        }
    }
}
