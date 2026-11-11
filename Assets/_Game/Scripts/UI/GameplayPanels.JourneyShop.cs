using System;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        RectTransform shopContent,shopSheet,shopConfirm;
        Text shopCoins,shopNotice;
        string shopFilter="all";
        float shopNextPress;
        static void ShopAt(RectTransform r,float x,float top,float width,float height)
        {
            UIController.SetRect(r,new Vector2(0,1),new Vector2(0,1));
            r.offsetMin=new Vector2(x,-top-height);r.offsetMax=new Vector2(x+width,-top);
        }
        Text ShopText(Transform p,string name,string text,float x,float y,float w,float h,int size)
        {
            var label=ui.Label(p,name,text,Vector2.zero,Vector2.zero,size,false);
            ShopAt(label.rectTransform,x,y,w,h);label.color=DesignKitArt.Ink;
            label.alignment=TextAnchor.MiddleLeft;return label;
        }
        public void ShowJourneyShop()
        {
            Clear();shopFilter="all";shopNextPress=0;shopConfirm=null;
            var visuals=AdventureUIArt.Load();Canvas.ForceUpdateCanvases();
            var width=Mathf.Min(760,ui.Root.rect.width-28);var height=Mathf.Min(900,ui.Root.rect.height-28);
            overlay=ui.Panel(ui.Root,"GameplayOverlay",Vector2.zero,Vector2.one,new Color(.025f,.07f,.10f,.62f));
            shopSheet=MobileUI.Frame(ui,overlay,"JourneyShopPanel",new Vector2(.5f,.5f),new Vector2(.5f,.5f),visuals.window,2.5f);
            shopSheet.sizeDelta=new Vector2(width,height);
            ShopText(shopSheet,"ShopHeading","Cửa hàng Hành Trình",24,18,width-110,38,24);
            var exit=MobileUI.IconButton(ui,shopSheet,"ShopClose","",Vector2.zero,Vector2.zero,Close,AdventureSymbol.Close);
            ShopAt((RectTransform)exit.transform,width-72,14,54,54);
            var coin=ui.Picture(shopSheet,"ShopCoin",visuals.coin,Vector2.zero,Vector2.zero);
            ShopAt(coin.rectTransform,26,66,28,28);
            shopCoins=ShopText(shopSheet,"ShopCoins","",62,62,width-90,34,19);
            ShopText(shopSheet,"ShopHint","Tự khám phá hoặc dùng xu mua cho nhanh. Hàng luôn có sẵn.",24,101,width-48,46,15);
            var filters=new[]{"all","items","permissions"};var labels=new[]{"Tất cả","Vật phẩm","Quyền mở lối"};
            for(var i=0;i<filters.Length;i++)
            {
                var f=filters[i];var b=MobileUI.Button(ui,shopSheet,"ShopFilter_"+f,labels[i],Vector2.zero,Vector2.zero,()=>{shopFilter=f;DrawShopRows();});
                ShopAt((RectTransform)b.transform,24+i*(width-48)/3,153,(width-60)/3,48);
            }
            var viewport=ui.Panel(shopSheet,"ShopViewport",Vector2.zero,Vector2.zero,Color.clear);
            ShopAt(viewport,20,214,width-40,height-310);viewport.gameObject.AddComponent<RectMask2D>();
            shopContent=new GameObject("ShopContent",typeof(RectTransform)).GetComponent<RectTransform>();shopContent.SetParent(viewport,false);
            UIController.SetRect(shopContent,new Vector2(0,1),Vector2.one);shopContent.pivot=new Vector2(.5f,1);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=shopContent;
            scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            shopNotice=ShopText(shopSheet,"ShopNotice","Mua vật phẩm không tự nhận hay trả nhiệm vụ.",24,height-85,width-48,66,16);
            DrawShopRows();
        }
        void DrawShopRows(bool preserveScroll=false)
        {
            var old=shopContent.anchoredPosition;
            foreach(Transform child in shopContent){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
            Canvas.ForceUpdateCanvases();var width=shopSheet.sizeDelta.x-48;var y=0f;
            shopCoins.text=game.Save.coins+" Xu Hành Trình";
            foreach(var entry in JourneyShop.Catalog)
            {
                if(shopFilter=="items"&&entry.Permission||shopFilter=="permissions"&&!entry.Permission)continue;
                var e=entry;var row=MobileUI.Frame(ui,shopContent,"ShopRow_"+e.id,Vector2.zero,Vector2.zero,AdventureUIArt.Load().buttonPaper,3);
                ShopAt(row,4,y,width,146);
                var icon=ui.Picture(row,"ShopIcon_"+e.id,e.Permission?AdventureUIArt.Load().coin:AdventureUIArt.Load().Item(e.itemId)?.sprite??art.item,Vector2.zero,Vector2.zero);
                ShopAt(icon.rectTransform,12,20,70,70);
                // Some source items share their illustration. Show a distinct small
                // type label so they are not visually indistinguishable.
                if(!e.Permission)
                {
                    var tag=ShopText(row,"ShopIconCaption_"+e.id,e.name,10,84,86,18,12);
                    tag.alignment=TextAnchor.MiddleCenter;
                }
                ShopText(row,"ShopName_"+e.id,e.name,94,8,width-106,29,19);
                ShopText(row,"ShopDescription_"+e.id,e.description,94,40,width-108,50,15);
                ShopText(row,"ShopPrice_"+e.id,e.price+" xu"+(e.Permission?" · một lần":" · ×1 · Trong túi: "+game.Inventory.Count(e.itemId)),18,100,width-186,32,16);
                var reason=game.Shop.Unavailable(e);var buy=MobileUI.Button(ui,row,"ShopBuy_"+e.id,string.IsNullOrEmpty(reason)?"Mua":reason,Vector2.zero,Vector2.zero,()=>BuyFromShop(e));
                ShopAt((RectTransform)buy.transform,width-162,94,148,44);buy.interactable=string.IsNullOrEmpty(reason);
                var text=buy.GetComponentInChildren<Text>();text.resizeTextForBestFit=true;text.resizeTextMinSize=12;text.resizeTextMaxSize=17;
                y+=154;
            }
            shopContent.sizeDelta=new Vector2(0,y);shopContent.anchoredPosition=preserveScroll?old:Vector2.zero;
        }
        void BuyFromShop(JourneyShopEntry entry)
        {
            if(Time.unscaledTime<shopNextPress||shopConfirm!=null)return;
            shopNextPress=Time.unscaledTime+.35f;
            if(entry.Permission)
            {
                shopConfirm=ui.Panel(overlay,"ShopConfirmation",Vector2.zero,Vector2.one,new Color(.025f,.07f,.10f,.72f));
                var w=Mathf.Min(510,ui.Root.rect.width-44);var sheet=MobileUI.Frame(ui,shopConfirm,"ShopConfirmCard",new Vector2(.5f,.5f),new Vector2(.5f,.5f),AdventureUIArt.Load().window,2.5f);
                sheet.sizeDelta=new Vector2(w,250);
                ShopText(sheet,"ShopConfirmText","Trả 5 xu để hoàn tất phí đò HQ04 ngay? Không mở thành phố hay nhận thưởng.",22,24,w-44,120,20);
                var yes=MobileUI.Button(ui,sheet,"ShopConfirmBuy","Trả 5 xu",Vector2.zero,Vector2.zero,()=>{DismissShopConfirm();CommitShopPurchase(entry);});
                ShopAt((RectTransform)yes.transform,22,170,(w-54)/2,54);
                var no=MobileUI.Button(ui,sheet,"ShopConfirmCancel","Để sau",Vector2.zero,Vector2.zero,DismissShopConfirm);
                ShopAt((RectTransform)no.transform,32+(w-54)/2,170,(w-54)/2,54);
            }
            else CommitShopPurchase(entry);
        }
        void DismissShopConfirm()
        {if(shopConfirm==null)return;shopConfirm.gameObject.SetActive(false);UnityEngine.Object.Destroy(shopConfirm.gameObject);shopConfirm=null;}
        void CommitShopPurchase(JourneyShopEntry entry)
        {
            var watch=WatchActualRewards(entry.Permission?Array.Empty<ItemAmount>():new[]{new ItemAmount{itemId=entry.itemId,amount=1}});
            var success=game.Shop.TryBuy(entry.id,Guid.NewGuid().ToString("N"),out var notice);
            shopNotice.text=notice;shopNotice.color=success?DesignKitArt.Teal:DesignKitArt.Ink;
            RefreshShopRowsInPlace();
            if(success&&!entry.Permission)ShowReceipt(watch.Earned());
        }
    }
}
