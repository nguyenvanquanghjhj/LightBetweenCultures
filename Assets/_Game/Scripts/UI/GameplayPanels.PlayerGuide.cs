using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        public void ShowPlayerGuide(int page=0)
        {
            string[] titles={"Nhịp Cầu Ánh Sáng","Bước đầu ở Hội An","Chạm để khám phá","Hành trang của Lumi"};
            string[] words={
                "Bạn là Lumi, chú gà mang chiếc đèn nối những câu chuyện Việt–Hàn.\n\nĐêm hội Hội An bỗng có những ngọn đèn chớp tắt. Cùng Lumi giúp cư dân tìm lại nhịp ánh sáng, khám phá các thành phố và kết nối những di vật văn hóa.",
                "Lumi vừa xuống thuyền ở Hội An. Trước tiên, hãy ghé Tiệm cô Mai để hỏi về những chiếc đèn đang tắt.\n\nChạm vào địa điểm để Lumi đi tới. Dấu ba chấm phía trên nhân vật báo rằng có thể trò chuyện.",
                "Chạm địa điểm để chọn đường; kéo bản đồ để nhìn quanh và dùng hai ngón tay để thu phóng.\n\nTrong hẻm, chạm nền đất để Lumi đi. Đến gần vật phẩm rồi bấm Nhặt. Hãy mang Mắt Đèn về cho Cô Mai.",
                "Mở Túi đồ để xem vật phẩm và nơi dùng chúng. Nhật ký ghi lại nhiệm vụ đã nhận.\n\nBấm Việc cần làm để đọc lời nhắc và xem địa điểm trên bản đồ. Bạn có thể xem lại hướng dẫn từ Nhật ký bất cứ lúc nào."};
            var sheet=ActivitySheet("PlayerGuidePanel",titles[page],"Lumi · Hành trình kết nối ánh sáng văn hóa");
            var portrait=ui.Picture(sheet,"GuideLumi",art.lumiPortrait,new Vector2(.60f,.60f),new Vector2(.91f,.78f));portrait.raycastTarget=false;
            var body=ui.Label(sheet,"GuideBody",words[page],new Vector2(.09f,.27f),new Vector2(.91f,.72f),21,false);body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperLeft;
            // A dedicated page, short copy and a fixed bottom action area on portrait screens.
            portrait.gameObject.SetActive(page==0);if(page==0)UIController.SetRect(body.rectTransform,new Vector2(.09f,.23f),new Vector2(.91f,.58f));
            var skip=sheet.Find("ActivityClose").GetComponent<Button>();skip.name="GuideSkip";skip.GetComponentInChildren<Text>().text="Bỏ qua · Chơi ngay";
            UIController.SetRect((RectTransform)skip.transform,new Vector2(.07f,.035f),new Vector2(.48f,.115f));skip.onClick.RemoveAllListeners();skip.onClick.AddListener(EndGuide);
            var next=MobileUI.Button(ui,sheet,"GuideNext",page==3?"Bắt đầu khám phá":"Tiếp",new Vector2(.52f,.035f),new Vector2(.93f,.115f),()=>{if(page<3)ShowPlayerGuide(page+1);else EndGuide();},true,true);MobileUI.Cue(next,AdventureSymbol.Arrow);
        }
        void EndGuide(){PlayerPrefs.SetInt(PlayerCopy.GuideKey,1);PlayerPrefs.Save();Close();}
        public void ShowMapMotion()
        {
            var sheet=ActivitySheet("MapMotionPanel","Hiệu ứng marker","Chọn mức nhấp nhô của biểu tượng trên bản đồ. Lựa chọn được ghi nhớ và không đổi tiến độ chơi.");
            var modes=new[]{MarkerMotionMode.Full,MarkerMotionMode.Reduced,MarkerMotionMode.Off};
            var labels=new[]{"Nhấp nhô bình thường","Chuyển động nhẹ","Tắt chuyển động"};
            for(var i=0;i<modes.Length;i++)
            {
                var mode=modes[i];var y=.59f-i*.13f;
                MobileUI.Button(ui,sheet,"MotionChoice_"+mode,labels[i],new Vector2(.10f,y),new Vector2(.90f,y+.09f),()=>{NodeMarkerView.SetMotionMode(mode);Close();},NodeMarkerView.MotionMode==mode,true);
            }
        }
    }
}
