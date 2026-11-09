using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        CulturalRelic pendingSignature;
        RectTransform signatureToast;
        float signatureAt;
        public int SignatureReactions {get;private set;}
        void RequestSignature(CulturalRelic relic){pendingSignature=relic;}
        void TickSignature()
        {
            // Only the existing false -> restored transition requests this. A
            // loaded restored city remains lit without replaying celebration.
            if(pendingSignature!=null&&!dialogue&&!Moving)
            {
                var relic=pendingSignature;pendingSignature=null;
                if(signatureToast!=null)Destroy(signatureToast.gameObject);
                signatureToast=MobileUI.Frame(ui,ui.Root,"SignatureMoment",new Vector2(.13f,.19f),new Vector2(.87f,.32f),presentation.window,3);
                signatureToast.GetComponent<Image>().raycastTarget=false;
                var group=signatureToast.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
                ui.Picture(signatureToast,"SignatureLumi",art.lumiPortrait,new Vector2(.02f,.06f),new Vector2(.20f,.96f)).gameObject.AddComponent<PortraitPresence>();
                var text=ui.Label(signatureToast,"SignatureText",relic.name+"\nLumi: "+SignatureLine(),new Vector2(.22f,.08f),new Vector2(.96f,.92f),18);text.color=DesignKitArt.Ink;
                signatureAt=Time.unscaledTime;SignatureReactions++;
            }
            if(signatureToast==null)return;
            var age=Time.unscaledTime-signatureAt;
            signatureToast.GetComponent<CanvasGroup>().alpha=Mathf.Clamp01(age*5)*Mathf.Clamp01((4-age)*3);
            if(age>=4){Destroy(signatureToast.gameObject);signatureToast=null;}
        }
        string SignatureLine()
        {
            switch(game.Data.cityId)
            {
                case "hoi_an":return "Đèn đã nối lại những câu chuyện của phố mình!";
                case "hue":return "Dấu khắc bên sông đã có nhịp trở lại.";
                case "da_lat":return "Mình nhìn rõ hướng đi giữa đồi thông rồi.";
                case "jeonju":return "Hoa văn và lời chào đã cùng ở trên một tờ giấy.";
                case "gyeongju":return game.World.Has("gyeongju.stone_guard_friend")?"Người giữ tượng sẽ cùng giữ những vệt sao này.":"Những vì sao vẫn giữ câu chuyện của người xưa.";
                case "seoul":return "Các relay đã cùng giữ một nhịp. Mình còn lời hẹn ở Lễ Kết Nối.";
                default:return "Một phần câu chuyện đã trở lại.";
            }
        }
    }
}
