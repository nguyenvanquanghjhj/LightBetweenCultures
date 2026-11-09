using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public static class MaiDialogueController
    {
        public static void Show(UIController ui,GameBootstrap game,DesignKitArt art,System.Action closed)
        {
            var panel=ui.Panel(ui.Root,"MaiDialogue",new Vector2(.025f,.035f),new Vector2(.975f,.43f),DesignKitArt.Hex("FFF3D9"));
            MobileUI.Round(panel);
            var speaker=ui.Label(panel,"Speaker","CÔ MAI",new Vector2(.36f,.80f),new Vector2(.95f,.94f),22); speaker.color=DesignKitArt.Ink;
            var portrait=ui.Picture(panel,"SpeakerPortrait",art.maiPortrait,new Vector2(.025f,.23f),new Vector2(.34f,.96f));
            var speech=ui.Label(panel,"Dialogue","",new Vector2(.36f,.36f),new Vector2(.95f,.79f),20);
            speech.color=DesignKitArt.Ink; speech.alignment=TextAnchor.MiddleLeft;
            var progress=ui.Label(panel,"QuestProgress","",new Vector2(.36f,.24f),new Vector2(.95f,.35f),14); progress.color=DesignKitArt.Teal;
            var ready=game.Inventory.Has(game.Data.quest.item);
            speech.text=game.Quests.Completed?game.Data.quest.thanks:!game.Quests.Active?game.Data.quest.offer:ready?"Cháu tìm thấy linh kiện rồi à? Mang lại đây, cô sẽ sửa chiếc đèn.":game.Data.quest.reminder;
            progress.text=game.Quests.Completed?"Đã trả linh kiện · Đường mới đã mở":$"Linh kiện đèn  {(ready?1:0)} / 1";
            Button primary=null, hint=null;
            System.Action close=()=>{Object.Destroy(panel.gameObject);closed();};
            primary=MobileUI.Button(ui,panel,"DialogueAction",!game.Quests.Active?"Nhận nhiệm vụ":ready&&!game.Quests.Completed?"Trả linh kiện":"Hẹn gặp lại",new Vector2(.04f,.045f),new Vector2(.43f,.19f),()=>{
                if(game.Quests.Completed || game.Quests.Active&&!ready) { close(); return; }
                var accepting=!game.Quests.Active;
                var result=game.Quests.TalkToMai();
                if(accepting)
                {
                    speaker.text="LUMI"; portrait.sprite=art.lumiPortrait;
                    speech.text="Cháu sẽ tìm linh kiện trong Hẻm mái ngói rồi mang về cho cô!";
                    progress.text="Đã nhận nhiệm vụ · Đến H3";
                }
                else { speech.text=result; progress.text="Đã trả linh kiện · Đường mới đã mở"; }
                primary.gameObject.SetActive(false); hint.gameObject.SetActive(false);
            },true);
            hint=MobileUI.Button(ui,panel,"AskMai","Hỏi thêm",new Vector2(.45f,.045f),new Vector2(.70f,.19f),()=>{speech.text="Hẻm mái ngói ở phía trên phố. Hãy chạm H3; vào hẻm rồi tìm linh kiện gần bờ sông.";});
            MobileUI.Button(ui,panel,"Continue","Đóng",new Vector2(.72f,.045f),new Vector2(.96f,.19f),()=>close());
        }
    }
}
