using UnityEngine;
using System.Linq;

namespace LightBetweenCultures
{
    public sealed class MainMenu : MonoBehaviour
    {
        private void Start()
        {
            var game = GameBootstrap.Ensure();
            var ui = UIController.Create("LIGHT BETWEEN CULTURES\nNhịp Cầu Ánh Sáng", true);
            var continuing=HasJourney(game);
            // First launch is only a presentation decision. Save data is untouched.
            var firstVisit=!continuing && PlayerPrefs.GetInt(PlayerCopy.GuideKey,0)==0;
            var city=game.Content.City(game.Data.cityId);
            var node=game.Save.node>=0&&game.Save.node<game.Data.nodes.Length?game.Data.nodes[game.Save.node].name:"điểm dừng đã lưu";
            ui.Say(continuing?"Tiếp tục tại "+city.name+" · "+node+".":
                "Bạn là Lumi. Bắt đầu ở Hội An, tìm hiểu văn hóa và giúp chú mèo đầu tiên.");
            var hero = ui.Panel(ui.Root,"LumiCard",new Vector2(.12f,.51f),new Vector2(.88f,.78f),DesignKitArt.Hex("17384E"));
            hero.GetComponent<UnityEngine.UI.Image>().raycastTarget=false;
            UIController.Border(hero,DesignKitArt.Teal);
            ui.Picture(hero,"CanonicalLumiPortrait",DesignKitArt.Load().lumiPortrait, new Vector2(.06f,.03f),new Vector2(.94f,.98f));
            ui.Label(ui.Root,"CatventureBuildMark","MỘT HÀNH TRÌNH · HAI NỀN VĂN HÓA",new Vector2(.15f,.52f),new Vector2(.85f,.56f),16,false).color=DesignKitArt.Cream;
            ui.Button(ui.Root,"Start",continuing?"TIẾP TỤC HÀNH TRÌNH":"BẮT ĐẦU PHIÊU LƯU",new Vector2(.15f,.20f),new Vector2(.85f,.30f), () =>
            {
                if(continuing) { game.Scenes.OpenWorld(); return; }
                if(firstVisit) { new GameplayPanels(ui,game,DesignKitArt.Load(),null,null).ShowFirstAdventureIntro(); return; }
                // Replay after an intro skip/completion, with no save state yet.
                game.OpenCatRescueOnNextCity=true;
                game.Scenes.Go("CityMap");
            });
            ui.Button(ui.Root,"CatventureStart","CATVENTURE · GIẢI CỨU MÈO "+game.Cats.RescuedCount+"/6",
                new Vector2(.15f,.315f),new Vector2(.85f,.405f), () =>
                {
                    // Keep existing game progress; open the current city's rescue quest.
                    game.OpenCatRescueOnNextCity=true;
                    game.Scenes.Go("CityMap");
                });
            ui.Button(ui.Root,"MainGuide",firstVisit?"XEM TRƯỚC CÂU CHUYỆN":"Câu chuyện & cách chơi",new Vector2(.15f,.415f),new Vector2(.85f,.485f),()=>
            {
                var guide=new GameplayPanels(ui,game,DesignKitArt.Load(),null,null);
                if(firstVisit)guide.ShowFirstAdventureIntro();else guide.ShowPlayerGuide();
            });
            // Returning players retain the direct Catventure entry. New players
            // only see one main start button and are led to the rescue in context.
            if(firstVisit)ui.Root.Find("CatventureStart").gameObject.SetActive(false);
            if(SaveSystem.RecoveryBlocked)
            {
                ui.Root.Find("Start").GetComponent<UnityEngine.UI.Button>().interactable=false;
                ui.Root.Find("CatventureStart").GetComponent<UnityEngine.UI.Button>().interactable=false;
                ui.Say(SaveSystem.RecoveryMessage);
                ui.Root.Find("CatventureStart").gameObject.SetActive(false);
                ui.Button(ui.Root,"RetrySave","Thử đọc lại bản lưu",new Vector2(.15f,.315f),new Vector2(.85f,.405f),()=>{game.Reload();game.Scenes.Go("Prototype");});
            }
            ui.Button(ui.Root,"QuitApplication","THOÁT GAME",new Vector2(.15f,.009f),new Vector2(.85f,.061f),() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying=false;
#else
                Application.Quit();
#endif
            });
            ui.Button(ui.Root,"Reset","Đặt lại tiến độ",new Vector2(.15f,.07f),new Vector2(.85f,.15f), () => {
                if(ui.Root.Find("ResetConfirmation")!=null)return;
                var confirm = ui.Panel(ui.Root,"ResetConfirmation",new Vector2(.06f,.22f),new Vector2(.94f,.76f),new Color(.06f,.12f,.14f));
                ui.Label(confirm,"Prompt","Xóa TOÀN BỘ hành trình ở 6 thành phố?\n\nNhiệm vụ, vật phẩm, 6 di vật, bưu thiếp và 12 trải nghiệm sẽ bị xóa. Thao tác này không thể hoàn tác.",new Vector2(.06f,.48f),new Vector2(.94f,.94f),20);
                ui.Button(confirm,"ConfirmReset","Xóa toàn bộ và chơi lại",new Vector2(.08f,.10f),new Vector2(.92f,.25f), () => { game.ResetSave();var start=ui.Root.Find("Start").GetComponent<UnityEngine.UI.Button>();start.interactable=true;start.GetComponentInChildren<UnityEngine.UI.Text>().text="Bắt đầu hành trình";ui.Say("Đã đặt lại toàn bộ hành trình. Lumi bắt đầu tại bến thuyền Hội An."); Destroy(confirm.gameObject); });
                ui.Button(confirm,"CancelReset","Hủy — Giữ tiến độ",new Vector2(.08f,.28f),new Vector2(.92f,.43f), () => Destroy(confirm.gameObject));
            });
        }
        static bool HasJourney(GameBootstrap game)=>game.Save.accepted.Count>0||game.Save.visited.Count>1||game.Data.cityId!="hoi_an"||
            game.Save.catRescues.Any(p=>p.started||p.rescued||p.supplies.Count>0||p.completed.Count>0)||
            game.Relics.Catalog.fragments.Any(f=>game.Relics.Owned(f.id));
    }
}
