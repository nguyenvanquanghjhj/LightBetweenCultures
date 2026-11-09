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
            var hero = ui.Panel(ui.Root,"LumiCard",new Vector2(.12f,.43f),new Vector2(.88f,.78f),DesignKitArt.Hex("17384E"));
            hero.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            UIController.Border(hero,DesignKitArt.Teal);
            ui.Picture(hero,"CanonicalLumiPortrait",DesignKitArt.Load().lumiPortrait, new Vector2(.06f,.03f),new Vector2(.94f,.98f));
            ui.Button(ui.Root,"Start",game.Save.accepted.Count==0?"Bắt đầu hành trình":"Tiếp tục hành trình",new Vector2(.15f,.20f),new Vector2(.85f,.30f), () => game.Scenes.OpenWorld());
            ui.Button(ui.Root,"MainGuide","Tôi là Lumi · Xem cách chơi",new Vector2(.15f,.33f),new Vector2(.85f,.41f),()=>new GameplayPanels(ui,game,DesignKitArt.Load(),null,null).ShowPlayerGuide());
            if(SaveSystem.RecoveryBlocked)
            {
                ui.Root.Find("Start").GetComponent<UnityEngine.UI.Button>().interactable=false;ui.Say(SaveSystem.RecoveryMessage);
                ui.Button(ui.Root,"RetrySave","Thử đọc lại bản lưu",new Vector2(.16f,.34f),new Vector2(.84f,.43f),()=>{game.Reload();game.Scenes.Go("Prototype");});
            }
            ui.Button(ui.Root,"Reset","Đặt lại tiến độ",new Vector2(.15f,.07f),new Vector2(.85f,.15f), () => {
                if(ui.Root.Find("ResetConfirmation")!=null)return;
                var confirm = ui.Panel(ui.Root,"ResetConfirmation",new Vector2(.06f,.22f),new Vector2(.94f,.76f),new Color(.06f,.12f,.14f));
                ui.Label(confirm,"Prompt","Xóa TOÀN BỘ hành trình ở 6 thành phố?\n\nNhiệm vụ, vật phẩm, 6 di vật, bưu thiếp và 12 trải nghiệm sẽ bị xóa. Thao tác này không thể hoàn tác.",new Vector2(.06f,.48f),new Vector2(.94f,.94f),20);
                ui.Button(confirm,"ConfirmReset","Xóa toàn bộ và chơi lại",new Vector2(.08f,.10f),new Vector2(.92f,.25f), () => { game.ResetSave(); ui.Root.Find("Start").GetComponent<UnityEngine.UI.Button>().interactable=true;ui.Say("Đã đặt lại toàn bộ hành trình. Lumi bắt đầu tại bến thuyền Hội An."); Destroy(confirm.gameObject); });
                ui.Button(confirm,"CancelReset","Hủy — Giữ tiến độ",new Vector2(.08f,.28f),new Vector2(.92f,.43f), () => Destroy(confirm.gameObject));
            });
        }
    }
}
