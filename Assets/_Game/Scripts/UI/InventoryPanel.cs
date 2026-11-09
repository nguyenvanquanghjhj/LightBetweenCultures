using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Presentation over the existing inventory. Acquiring/using items stays in gameplay services.
    public sealed class InventoryPanel
    {
        readonly UIController ui;
        readonly GameBootstrap game;
        readonly AdventureUIArt art;
        RectTransform overlay,details;
        Action close,showQuest;
        Button footer;
        string selected;
        readonly System.Collections.Generic.Dictionary<string,Button> slots=new System.Collections.Generic.Dictionary<string,Button>();
        InventoryPanel(UIController ui,GameBootstrap game){this.ui=ui;this.game=game;art=AdventureUIArt.Load();}
        public static void Show(UIController ui,GameBootstrap game,Action close,Action showQuest=null)=>new InventoryPanel(ui,game).Open(close,showQuest);
        int Count(string id)=>game.Inventory.Count(id);
        static void At(RectTransform rect,float x,float top,float width,float height)
        {
            UIController.SetRect(rect,new Vector2(0,1),new Vector2(0,1));
            rect.offsetMin=new Vector2(x,-top-height);rect.offsetMax=new Vector2(x+width,-top);
        }
        Text TextBox(Transform parent,string name,string value,float x,float top,float width,float height,int size,Color color,TextAnchor alignment=TextAnchor.MiddleLeft)
        {
            var text=ui.Label(parent,name,value,Vector2.zero,Vector2.zero,size,false);At(text.rectTransform,x,top,width,height);text.color=color;text.alignment=alignment;return text;
        }
        bool QuestItem(string id)
        {
            var type=game.Content.Item(id)?.type;
            if(game.Content.Item(id)?.questItem==true)return true;
            return type=="key"||type=="quest"||type=="flag_token"||id==game.Data.quest.item||game.Content.quests.Any(q=>game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id)&&(q.requiredItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==id));
        }
        bool HasRelatedQuest()=>game.Content.quests.Any(q=>game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id)&&(q.requiredItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==selected));
        void Exit(){overlay.gameObject.SetActive(false);UnityEngine.Object.Destroy(overlay.gameObject);close?.Invoke();}
        void Open(Action close,Action showQuest)
        {
            this.close=close;this.showQuest=showQuest;
            var owned=game.Content.items.Select(i=>i.id).Concat(game.Save.items).Distinct().Where(id=>
            {var alias=art.Item(id)?.aliasOf;return Count(id)>0&&(string.IsNullOrEmpty(alias)||Count(alias)==0);}).ToArray();
            Canvas.ForceUpdateCanvases();
            var displayScale=Screen.width>Screen.height?Mathf.Clamp(1/Mathf.Max(.1f,ui.GetComponent<Canvas>().scaleFactor),1,1.6f):1;
            var width=Mathf.Min((ui.Root.rect.width-32)/displayScale,760);
            var columns=width>=730?5:4;
            var rows=Mathf.Clamp(Mathf.CeilToInt((float)owned.Length/columns),2,3);
            float gridHeight=rows*116+(rows-1)*8+16;
            var height=Mathf.Min((ui.Root.rect.height-80)/displayScale,104+gridHeight+26+166+78);
            gridHeight=height-374;
            overlay=ui.Panel(ui.Root,"InventoryOverlay",Vector2.zero,Vector2.one,new Color(.025f,.07f,.10f,.56f));
            var panel=MobileUI.Frame(ui,overlay,"InventoryPanel",new Vector2(.5f,.5f),new Vector2(.5f,.5f),art.window,2.5f);panel.sizeDelta=new Vector2(width,height);panel.localScale=Vector3.one*displayScale;
            panel.gameObject.AddComponent<PortraitPresence>().slide=true;
            var bag=ui.Picture(panel,"BagPortrait",art.backpack,Vector2.zero,Vector2.zero);At(bag.rectTransform,25,20,59,65);
            TextBox(panel,"InventoryHeading","TÚI HÀNH TRÌNH",96,22,width-180,32,23,DesignKitArt.Ink);
            TextBox(panel,"InventoryVitals","Lumi · HP "+game.Save.playerHP+" / 100",96,60,width-262,24,16,DesignKitArt.Ink);
            var coin=ui.Picture(panel,"CoinIcon",art.coin,Vector2.zero,Vector2.zero);At(coin.rectTransform,width-162,60,24,24);
            TextBox(panel,"CoinCount",game.Save.coins.ToString(),width-131,60,64,24,17,DesignKitArt.Ink);
            var exit=MobileUI.IconButton(ui,panel,"CloseInventory","",Vector2.zero,Vector2.zero,Exit,AdventureSymbol.Close);At((RectTransform)exit.transform,width-75,20,52,52);
            var viewport=ui.Panel(panel,"ItemViewport",Vector2.zero,Vector2.zero,Color.clear);At(viewport,24,104,width-48,gridHeight);viewport.gameObject.AddComponent<RectMask2D>();
            var content=new GameObject("ItemGrid",typeof(RectTransform),typeof(GridLayoutGroup),typeof(ContentSizeFitter)).GetComponent<RectTransform>();content.SetParent(viewport,false);
            UIController.SetRect(content,new Vector2(0,1),Vector2.one);content.pivot=new Vector2(.5f,1);
            var grid=content.GetComponent<GridLayoutGroup>();grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=columns;
            grid.spacing=new Vector2(8,8);grid.padding=new RectOffset(4,4,8,8);grid.cellSize=new Vector2((width-56-8*(columns-1))/columns,116);
            content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=26;
            foreach(var id in owned)
            {
                var key=id;var entry=art.Item(id);var item=game.Content.Item(id);var label=item?.name??"Linh kiện mở đầu";
                var button=MobileUI.Button(ui,content,"Item_"+id,label,Vector2.zero,Vector2.one,()=>Select(key),illustrated:true);slots[id]=button;MobileUI.Skin((Image)button.targetGraphic,art.slot,4);
                var title=button.transform.Find("Label").GetComponent<Text>();UIController.SetRect(title.rectTransform,new Vector2(.06f,.025f),new Vector2(.94f,.30f));title.fontSize=14;title.color=DesignKitArt.Cream;
                ui.Picture(button.transform,"ItemIcon",entry?.sprite??DesignKitArt.Load().item,new Vector2(.16f,.31f),new Vector2(.84f,.89f));
                if(QuestItem(id))ui.Picture(button.transform,"QuestItemBadge",DesignKitArt.Load().quest,new Vector2(.045f,.75f),new Vector2(.235f,.955f));
                ui.Label(button.transform,"Quantity","×"+Count(id),new Vector2(.59f,.76f),new Vector2(.94f,.96f),15,false).color=DesignKitArt.Cream;
            }
            for(var index=owned.Length;index<columns*rows;index++)
            {var empty=MobileUI.Frame(ui,content,"EmptySlot"+index,Vector2.zero,Vector2.one,art.slot,4);empty.GetComponent<Image>().color=new Color(1,1,1,.70f);empty.GetComponent<Image>().raycastTarget=false;}
            TextBox(panel,"InventoryHint","Chọn vật phẩm · Kéo để xem thêm",24,104+gridHeight,width-48,26,14,DesignKitArt.Ink,TextAnchor.MiddleCenter);
            details=MobileUI.Frame(ui,panel,"ItemDetails",Vector2.zero,Vector2.zero,art.buttonPaper,3.5f);At(details,24,130+gridHeight,width-48,166);
            footer=MobileUI.Button(ui,panel,"InventoryAction","Trở lại hành trình",Vector2.zero,Vector2.zero,()=>{var quest=HasRelatedQuest()&&this.showQuest!=null;Exit();if(quest)this.showQuest();},true,true);At((RectTransform)footer.transform,24,height-72,width-48,52);MobileUI.Cue(footer,AdventureSymbol.Close);
            if(owned.Length>0)Select(owned[0]);
            else TextBox(details,"EmptyBag","Hành trang còn trống. Hãy khám phá và giúp đỡ cư dân!",24,25,width-96,104,18,DesignKitArt.Ink);
        }
        void Select(string id)
        {
            selected=id;
            foreach(var slot in slots)slot.Value.targetGraphic.color=slot.Key==id?new Color(1,.95f,.75f,1):Color.white;
            foreach(Transform child in details){child.gameObject.SetActive(false);UnityEngine.Object.Destroy(child.gameObject);}
            var entry=art.Item(id);var item=game.Content.Item(id);var width=details.rect.width;
            var icon=ui.Picture(details,"SelectedItemIcon",entry?.sprite??DesignKitArt.Load().item,Vector2.zero,Vector2.zero);At(icon.rectTransform,16,28,93,103);
            TextBox(details,"SelectedItemName",item?.name??"Linh kiện mở đầu",126,12,width-144,28,20,DesignKitArt.Ink);
            var category=QuestItem(id)?"VẬT PHẨM NHIỆM VỤ":item?.type=="consumable"?"TIÊU HAO · DÙNG TRONG TRẬN CHIẾN":item?.type=="currency"?"TIỀN TỆ":"NGUYÊN LIỆU HÀNH TRÌNH";
            TextBox(details,"SelectedItemType",category,126,42,width-144,24,14,DesignKitArt.Teal);
            var viewport=ui.Panel(details,"ItemDescriptionViewport",Vector2.zero,Vector2.zero,Color.clear);At(viewport,126,69,width-144,62);viewport.gameObject.AddComponent<RectMask2D>();
            var description=!string.IsNullOrEmpty(item?.description)?item.description:entry?.description??"Vật phẩm thu được trong hành trình.";
            var tasks=game.Content.quests.Where(q=>game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id)&&(q.requiredItems??Array.Empty<ItemAmount>()).Any(i=>i.itemId==id)).Select(q=>q.title).ToArray();
            if(tasks.Length>0)description+="\n\nĐang cần cho: "+string.Join(" · ",tasks)+". Giao tại NPC khi được hỏi; mở túi không tự dùng vật phẩm.";
            if(id=="link_key")description=game.World.Has("hoi_an.lamp_restored")?"Đã sử dụng để đồng bộ trạm Hội An tại H7 — kỷ vật hành trình. Huế và Jeonju mở bằng tín hiệu trạm; không cần dùng lại chìa.":"Nhận từ Bot Mờ-01 bằng chiến đấu hoặc mật mã H19. Dùng cùng Mắt Đèn và Lõi Giấy để đồng bộ trạm H7; chỉ cầm chìa chưa mở World Map.";
            if(id=="meat_portion")description="Thức ăn làm mồi, không phải thuốc hồi HP/năng lượng. Tại H12 chọn Dùng thức ăn dụ cua; tại G6 chọn dùng thức ăn cho cáo (tiêu hao 1 phần). Nhiệm vụ DQ02 tại D10 cần 3 phần, giao bằng lựa chọn hội thoại. Không dùng trong trận đánh bot.";
            if(id=="healing_tea")description="Trong trận đánh bot: bấm Uống trà · +14 HP. Mỗi lần dùng tiêu hao 1 ly và 1 lượt; đối thủ vẫn đáp trả. HP tối đa 100, đầy HP không dùng được. Không hồi năng lượng và không uống trực tiếp từ túi đồ.";
            var desc=ui.Label(viewport,"SelectedItemDescription",description,new Vector2(0,1),Vector2.one,16,false);desc.color=DesignKitArt.Ink;desc.alignment=TextAnchor.UpperLeft;desc.rectTransform.pivot=new Vector2(.5f,1);
            desc.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=desc.rectTransform;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=20;
            TextBox(details,"SelectedItemCount","Đang có: "+Count(id),126,134,width-144,23,15,DesignKitArt.Ink);
            var related=HasRelatedQuest()&&showQuest!=null;
            footer.transform.Find("Label").GetComponent<Text>().text=related?"Xem nhiệm vụ liên quan":"Trở lại hành trình";
            var cue=footer.GetComponentInChildren<AdventureIconGraphic>();cue.symbol=related?AdventureSymbol.Journal:AdventureSymbol.Close;cue.SetVerticesDirty();
        }
    }
}
