using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed class WorldMap : MonoBehaviour
    {
        GameBootstrap game;
        UIController ui;
        AdventureUIArt art;
        RectTransform details;
        readonly System.Collections.Generic.Dictionary<string,Button> cards = new System.Collections.Generic.Dictionary<string,Button>();
        private void Start()
        {
            game=GameBootstrap.Ensure(); ui=UIController.Create("NHỊP CẦU ÁNH SÁNG",true); art=AdventureUIArt.Load();
            var background=ui.Root.parent.Find("Backdrop").GetComponent<Image>();background.color=Color.white;MobileUI.Skin(background,art.backdrop,2);
            ui.Root.Find("MessagePanel").gameObject.SetActive(false);
            var title=ui.Root.Find("Title").GetComponent<Text>();
            UIController.SetRect(title.rectTransform,new Vector2(.14f,.928f),new Vector2(.95f,.98f)); title.alignment=TextAnchor.MiddleLeft;title.resizeTextForBestFit=false;title.fontSize=24;title.color=DesignKitArt.Ink;
            ui.Picture(ui.Root,"WorldEmblem",art.globe,new Vector2(.04f,.926f),new Vector2(.125f,.985f));
            var intro=ui.Label(ui.Root,"WorldIntroduction","Sáu thành phố · Một hành trình kết nối\nChọn điểm đến để xem tuyến đường và tiến độ.",new Vector2(.055f,.845f),new Vector2(.945f,.919f),17,false);
            intro.alignment=TextAnchor.MiddleLeft;intro.color=DesignKitArt.Ink;
            var order=new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"};
            for(var i=0;i<order.Length;i++)
            {
                var city=game.Content.City(order[i]); if(city==null)continue;
                var left=.055f+(i%2)*.46f;var top=.825f-(i/2)*.143f;
                var card=MobileUI.Button(ui,ui.Root,"City"+i,"",new Vector2(left,top-.128f),new Vector2(left+.43f,top),()=>Select(city),illustrated:true);
                MobileUI.Skin((Image)card.targetGraphic,art.postcard,4);
                card.transform.Find("Label").gameObject.SetActive(false); cards[city.id]=card;
                var clip=ui.Panel(card.transform,"CityImageClip",new Vector2(.035f,.05f),new Vector2(.965f,.95f),Color.clear);
                clip.GetComponent<Image>().raycastTarget=false;clip.gameObject.AddComponent<RectMask2D>();
                Cover(clip,"CityPreview",art.City(city.id)?.sprite);
                var unlocked=game.IsCityUnlocked(city.id);
                if(!unlocked) {var shade=ui.Panel(clip,"LockedTint",Vector2.zero,Vector2.one,new Color(.035f,.09f,.12f,.38f));shade.GetComponent<Image>().raycastTarget=false;}
                var band=ui.Panel(clip,"CityCaption",Vector2.zero,new Vector2(1,.46f),new Color(.04f,.12f,.16f,.88f));band.GetComponent<Image>().raycastTarget=false;
                ui.Label(band,"CityName",city.name,new Vector2(.045f,.35f),new Vector2(.92f,.98f),20,false).alignment=TextAnchor.MiddleLeft;
                var status=city.id==game.Save.cityId?"ĐANG Ở ĐÂY":unlocked?"ĐÃ MỞ TUYẾN":"CHƯA MỞ";
                var state=ui.Label(band,"CityState",status,new Vector2(.045f,.025f),new Vector2(.96f,.35f),14,false);state.alignment=TextAnchor.MiddleLeft;state.color=unlocked?DesignKitArt.Hex("EBC680"):DesignKitArt.Hex("B6C9C6");
                if(!unlocked)ui.Picture(clip,"CityLock",DesignKitArt.Load().locked,new Vector2(.79f,.44f),new Vector2(.95f,.80f));
                if(city.id==game.Save.cityId)ui.Picture(clip,"CurrentLumi",DesignKitArt.Load().lumiMapTraveler,new Vector2(.78f,.36f),new Vector2(.98f,.92f));
            }
            var back=MobileUI.Button(ui,ui.Root,"Back",game.Scenes.WorldReturnScene=="CityMap"?"Về "+game.Content.City(game.Save.cityId).name:"Về màn hình đầu",new Vector2(.055f,.025f),new Vector2(.945f,.079f),()=>game.Scenes.Go(game.Scenes.WorldReturnScene),illustrated:true);
            MobileUI.Cue(back,AdventureSymbol.Close);
            Select(game.Content.City(game.Scenes.WorldSelection??game.Save.cityId)??game.Content.City("hoi_an"));
        }
        void Cover(Transform parent,string name,Sprite sprite)
        {
            if(sprite==null)return;
            var image=ui.Picture(parent,name,sprite,Vector2.zero,Vector2.one);
            var fit=image.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=sprite.rect.width/sprite.rect.height;
        }
        void Select(CityDefinition city)
        {
            foreach(var pair in cards)pair.Value.targetGraphic.color=pair.Key==city.id?Color.white:new Color(.62f,.82f,.77f,1);
            if(details!=null){details.gameObject.SetActive(false);Destroy(details.gameObject);}
            details=MobileUI.Frame(ui,ui.Root,"DestinationDetails",new Vector2(.055f,.097f),new Vector2(.945f,.385f),art.window,3);
            var preview=ui.Panel(details,"DestinationPreview",new Vector2(.035f,.47f),new Vector2(.355f,.93f),Color.clear);preview.gameObject.AddComponent<RectMask2D>();Cover(preview,"SelectedCityArt",art.City(city.id)?.sprite);
            var heading=ui.Label(details,"DestinationName",city.name,new Vector2(.39f,.76f),new Vector2(.93f,.92f),24,false);heading.alignment=TextAnchor.MiddleLeft;heading.color=DesignKitArt.Ink;
            var unlocked=game.IsCityUnlocked(city.id);
            var status=city.id==game.Save.cityId?"Thành phố hiện tại":unlocked?"Tuyến giao lưu đã mở":"Tuyến giao lưu chưa mở";
            var statusText=ui.Label(details,"DestinationStatus",status,new Vector2(.39f,.61f),new Vector2(.93f,.76f),16,false);statusText.alignment=TextAnchor.MiddleLeft;statusText.color=DesignKitArt.Teal;
            var progress=game.Save.cityProgress.Find(p=>p.cityId==city.id);
            var visited=city.id==game.Save.cityId?game.Save.visited.Count:progress?.visited.Count??0;
            var progressText=ui.Label(details,"DestinationProgress","Khám phá "+visited+" / "+city.nodes.Length+" địa điểm",new Vector2(.39f,.46f),new Vector2(.93f,.61f),16,false);progressText.alignment=TextAnchor.MiddleLeft;progressText.color=DesignKitArt.Ink;
            var caption=art.City(city.id)?.description??city.opening;
            var note=!unlocked?"Khôi phục các cổng ánh sáng để mở tuyến này.":!city.runtimeReady?"Đã mở tuyến · Chương chơi đang phát triển.":city.runtimeNodeIds.Length>0?"Ghé chợ giao lưu và mang lời nhắn về Hội An.":"Tiếp tục hành trình từ địa điểm đã lưu.";
            var text=ui.Label(details,"DestinationDescription",caption+"\n"+note,new Vector2(.065f,.245f),new Vector2(.935f,.44f),16,false);text.alignment=TextAnchor.UpperLeft;text.color=DesignKitArt.Ink;
            var enter=MobileUI.Button(ui,details,"EnterCity",unlocked&&city.runtimeReady?"Vào "+city.name:!unlocked?"Chưa mở tuyến":"Chương chơi sắp có",new Vector2(.065f,.055f),new Vector2(.935f,.225f),()=>{if(game.EnterCity(city.id))game.Scenes.Go("CityMap");},true,true);
            MobileUI.Cue(enter,AdventureSymbol.Arrow);enter.interactable=unlocked&&city.runtimeReady;
        }
    }
}
