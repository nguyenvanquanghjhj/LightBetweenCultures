using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    /// <summary>Views over the shared event, combat and puzzle services; owns no gameplay state.</summary>
    public sealed class GameplayPanels
    {
        readonly UIController ui;
        readonly GameBootstrap game;
        readonly DesignKitArt art;
        readonly Action closed, changed;
        RectTransform overlay,receipt;
        readonly List<CanvasGroup> receiptBackground=new List<CanvasGroup>();

        public GameplayPanels(UIController ui, GameBootstrap game, DesignKitArt art, Action closed, Action changed)
        { this.ui = ui; this.game = game; this.art = art; this.closed = closed; this.changed = changed; }

        RectTransform Sheet(string name, string title)
        {
            Clear();
            overlay = ui.Panel(ui.Root, "GameplayOverlay", Vector2.zero, Vector2.one, new Color(.035f,.09f,.12f,.58f));
            var sheet = ui.Panel(overlay, name, new Vector2(.035f,.115f), new Vector2(.965f,.89f), DesignKitArt.Ink);
            MobileUI.Round(sheet);
            ui.Label(sheet, "PanelEyebrow", "LIGHT BETWEEN CULTURES", new Vector2(.06f,.94f), new Vector2(.94f,.98f), 13).color = DesignKitArt.Gold;
            ui.Label(sheet, "PanelTitle", title, new Vector2(.06f,.84f), new Vector2(.94f,.94f), 26);
            return sheet;
        }

        public void ShowEvent(int node, string notice = null,ItemAmount[] received=null,int optionPage=0)
        {
            var view = game.Events.BuildOptions(node);
            var definition = game.Events.Definition(game.Data.nodes[node].eventId);
            var checkIn=game.Events.CheckInAt(node);
            if((definition?.type=="junction"||definition?.type=="checkin")&&checkIn!=null&&
                (game.Events.CanCheckIn(node)||game.World.Has(checkIn.completionFlag)))
            {ShowCheckIn(node,false,game.Events.CanCheckIn(node));return;}
            DrawDialogue(node,view,definition,notice,false,optionPage);
            ShowReceipt(received);
        }

        static void Place(RectTransform rect,float x,float top,float width,float height)
        {
            rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(x,-top);rect.sizeDelta=new Vector2(width,height);
        }
        float DisplayScale=>Screen.width>Screen.height?Mathf.Clamp(1/Mathf.Max(.1f,ui.GetComponent<Canvas>().scaleFactor),1,1.6f):1;
        float TextHeight(string text,float width,int size)
        {
            var settings=new TextGenerationSettings{font=UIController.Font,fontSize=size,lineSpacing=1,
                scaleFactor=1,richText=true,textAnchor=TextAnchor.UpperLeft,verticalOverflow=VerticalWrapMode.Overflow,
                horizontalOverflow=HorizontalWrapMode.Wrap,generationExtents=new Vector2(width,0)};
            return new TextGenerator().GetPreferredHeight(text??"",settings);
        }
        void DrawDialogue(int node,EventView view,EventDefinition definition,string notice,bool lumiSpeaking,int optionPage=0)
        {
            var oldPortrait=overlay!=null?overlay.Find("EventPortrait")?.GetComponent<Image>():null;
            var oldSprite=oldPortrait!=null?oldPortrait.sprite:null;
            var oldPosition=oldPortrait!=null?oldPortrait.rectTransform.anchoredPosition:Vector2.zero;
            var oldSize=oldPortrait!=null?oldPortrait.rectTransform.sizeDelta:Vector2.zero;
            var oldMesh=oldPortrait!=null&&oldPortrait.useSpriteMesh;
            Clear();Canvas.ForceUpdateCanvases();
            overlay=ui.Panel(ui.Root,"GameplayOverlay",Vector2.zero,Vector2.one,new Color(.025f,.07f,.10f,.26f));
            var scale=DisplayScale;var width=Mathf.Min(700,ui.Root.rect.width/scale-32);
            var pages=Math.Max(1,(view.options.Count+2)/3);optionPage=Mathf.Clamp(optionPage,0,pages-1);
            var count=lumiSpeaking?0:Math.Min(3,view.options.Count-optionPage*3);
            var text=view.text??"";var feedbackText=notice??"";
            if(string.IsNullOrWhiteSpace(feedbackText)||text.Contains(feedbackText)||feedbackText==text)feedbackText="";
            else if(feedbackText.Length>100){if(!feedbackText.Contains(text))text+="\n\n"+feedbackText;else text=feedbackText;feedbackText="";}
            var optionHeights=new float[count];float optionsHeight=0;
            for(var i=0;i<count;i++){optionHeights[i]=Mathf.Max(52,TextHeight(view.options[optionPage*3+i].label,width*.80f-36,17)+20);optionsHeight+=optionHeights[i]+8;}
            var feedbackHeight=string.IsNullOrEmpty(feedbackText)?0:Mathf.Max(26,TextHeight(feedbackText,width-48,15)+6);
            var availableBody=Mathf.Max(56,ui.Root.rect.height/scale-220-optionsHeight-feedbackHeight-160);
            var bodyHeight=Mathf.Clamp(TextHeight(text,width-48,20)+8,56,Mathf.Min(156,availableBody));
            var height=82+bodyHeight+feedbackHeight+12+optionsHeight+64;
            var sheet=MobileUI.Frame(ui,overlay,"NodeEventPanel",Vector2.zero,Vector2.zero,AdventureUIArt.Load().window,2.8f);
            sheet.anchorMin=sheet.anchorMax=new Vector2(.5f,0);sheet.pivot=new Vector2(.5f,0);sheet.sizeDelta=new Vector2(width,height);sheet.anchoredPosition=new Vector2(0,18);sheet.localScale=Vector3.one*scale;
            var entrance=sheet.gameObject.AddComponent<PortraitPresence>();entrance.slide=true;
            var portrait=definition!=null&&definition.legacyTutorial?art.maiPortrait:definition?.type=="encounter"?art.enemy:definition?.type=="puzzle"?art.puzzle:art.Kind(game.Data.nodes[node].kind);
            var npcArt=AdventureUIArt.Load().Npc(definition?.npcId);
            var npcPortrait=npcArt?.portrait??npcArt?.sprite;if(npcPortrait!=null)portrait=npcPortrait;
            if(!string.IsNullOrEmpty(definition?.npcId)&&npcPortrait==null&&!definition.legacyTutorial)portrait=null;
            if(definition?.type=="encounter")portrait=AdventureUIArt.Load().Enemy(definition.enemyId)?.sprite??portrait;
            if(definition?.actor=="Lumi")portrait=art.lumiPortrait;
            var portraitHeight=Mathf.Min(168,(ui.Root.rect.height-18-height*scale)/scale-8);
            portraitHeight=Mathf.Max(90,portraitHeight);
            var active=ui.Picture(overlay,"EventPortrait",lumiSpeaking?art.lumiPortrait:portrait,Vector2.zero,Vector2.zero);
            active.rectTransform.anchorMin=active.rectTransform.anchorMax=new Vector2(.5f,0);active.rectTransform.pivot=new Vector2(.5f,0);
            active.rectTransform.sizeDelta=new Vector2(175,portraitHeight)*scale;
            active.rectTransform.anchoredPosition=new Vector2((lumiSpeaking?width*.30f:-width*.30f)*scale,18+height*scale-5);
            active.useSpriteMesh=!lumiSpeaking&&npcArt!=null&&npcArt.portraitUseSpriteMesh;
            active.raycastTarget=false;
            active.gameObject.AddComponent<PortraitPresence>().TransitionFrom(oldSprite,oldPosition,oldSize,oldMesh);
            active.gameObject.SetActive(active.sprite!=null);
            var nameplate=ui.Panel(sheet,"SpeakerPlate",Vector2.zero,Vector2.zero,DesignKitArt.Hex("315F64"));Place(nameplate,20,14,width-84,34);MobileUI.Round(nameplate);nameplate.GetComponent<Image>().raycastTarget=false;
            MobileUI.Symbol(nameplate,"SpeechIcon",AdventureSymbol.Chat,new Vector2(.025f,.10f),new Vector2(.10f,.90f),DesignKitArt.Gold);
            var speaker=ui.Label(nameplate,"EventSpeaker",lumiSpeaking?"Lumi":string.IsNullOrEmpty(view.speaker)?view.title:view.speaker,new Vector2(.13f,.05f),new Vector2(.97f,.95f),20,false);speaker.alignment=TextAnchor.MiddleLeft;
            var skip=MobileUI.IconButton(ui,sheet,"DialogueSkipEffect","",Vector2.zero,Vector2.zero,()=>{foreach(var p in overlay.GetComponentsInChildren<PortraitPresence>())p.Skip();},AdventureSymbol.Arrow);Place((RectTransform)skip.transform,width-60,8,52,52);
            var location=ui.Label(sheet,"EventLocation",game.Data.nodes[node].name,Vector2.zero,Vector2.zero,13,false);location.color=DesignKitArt.Teal;location.alignment=TextAnchor.MiddleLeft;Place(location.rectTransform,24,54,width-48,20);
            if(!lumiSpeaking&&pages>1)
            {
                Place(nameplate,20,14,width-164,34);
                var more=MobileUI.Button(ui,sheet,"EventMore",(optionPage+1)+" / "+pages+" ›",Vector2.zero,Vector2.zero,()=>ShowEvent(node,notice,null,(optionPage+1)%pages),illustrated:true);Place((RectTransform)more.transform,width-140,8,76,52);
            }
            var textViewport=ui.Panel(sheet,"DialogueTextViewport",Vector2.zero,Vector2.zero,Color.clear);Place(textViewport,24,82,width-48,bodyHeight);textViewport.gameObject.AddComponent<RectMask2D>();
            var body=ui.Label(textViewport,"EventText",text,new Vector2(0,1),Vector2.one,20,false);body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperLeft;
            body.rectTransform.pivot=new Vector2(.5f,1);body.rectTransform.sizeDelta=new Vector2(0,bodyHeight);
            var fitter=body.gameObject.AddComponent<ContentSizeFitter>();fitter.verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=textViewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=textViewport;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=22;
            var feedback=ui.Label(sheet,"EventNotice",feedbackText,Vector2.zero,Vector2.zero,15,false);feedback.color=DesignKitArt.Teal;feedback.alignment=TextAnchor.MiddleLeft;Place(feedback.rectTransform,24,82+bodyHeight,width-48,feedbackHeight);
            var y=82+bodyHeight+feedbackHeight+12;
            for(var i=0;i<count;i++)
            {
                var option=view.options[optionPage*3+i];
                var button=MobileUI.Button(ui,sheet,"EventOption_"+option.id,option.label,Vector2.zero,Vector2.zero,()=>Execute(node,option.id),i==0,true);
                Place((RectTransform)button.transform,24,y,width-48,optionHeights[i]);MobileUI.Cue(button,option.id=="checkin"||option.id=="checkin_view"?AdventureSymbol.Camera:i==0?AdventureSymbol.Check:AdventureSymbol.Chat);
                var label=button.GetComponentInChildren<Text>();label.resizeTextForBestFit=false;label.fontSize=17;label.horizontalOverflow=HorizontalWrapMode.Wrap;
                button.interactable=option.enabled;y+=optionHeights[i]+8;
            }
            var hasNext=lumiSpeaking||definition?.type=="npc"||!string.IsNullOrEmpty(definition?.npcId)||definition?.actor=="Lumi";
            var exit=MobileUI.Button(ui,sheet,"EventClose",count>0?"Nhận sau / Trở lại":"Trở lại hành trình",Vector2.zero,Vector2.zero,Close,illustrated:true);Place((RectTransform)exit.transform,24,height-60,hasNext?(width-56)*.54f:width-48,48);MobileUI.Cue(exit,AdventureSymbol.Close);
            if(hasNext)
            {
                var next=MobileUI.Button(ui,sheet,"DialogueNext","Tiếp theo",Vector2.zero,Vector2.zero,()=>
                {
                    foreach(var p in overlay.GetComponentsInChildren<PortraitPresence>())p.Skip();
                    if(body.rectTransform.rect.height>textViewport.rect.height+2&&scroll.verticalNormalizedPosition>.05f)scroll.verticalNormalizedPosition=Mathf.Max(0,scroll.verticalNormalizedPosition-.65f);
                    else if(lumiSpeaking)ShowEvent(node);else Close();
                },lumiSpeaking,true);Place((RectTransform)next.transform,32+(width-56)*.54f,height-60,(width-56)*.46f,48);MobileUI.Cue(next,AdventureSymbol.Arrow);
            }
        }

        void Execute(int node, string choice,bool confirmed=false)
        {
            var definition=game.Events.Definition(game.Data.nodes[node].eventId);
            var questId=definition?.questId;
            if(choice.StartsWith("complete:"))questId=Array.Find(game.Content.events,e=>e.cityId==game.Data.cityId&&e.nodeId==game.Data.nodes[node].id&&e.id==choice.Substring("complete:".Length))?.questId;
            var quest=game.Content.Quest(questId);
            var exchange=choice.StartsWith("collect:")?Array.Find(game.Content.events,e=>e.cityId==game.Data.cityId&&e.nodeId==game.Data.nodes[node].id&&e.id==choice.Substring("collect:".Length)):null;
            if(!confirmed&&exchange!=null&&exchange.consumeRequiredItems)
            {
                var reply=game.Events.BuildOptions(node);reply.text=exchange.text;
                foreach(var item in exchange.requiredItems)reply.text+="\nGiao "+(game.Content.Item(item.itemId)?.name??item.itemId)+" × "+item.amount;
                DrawDialogue(node,reply,definition,"Chỉ giao vật phẩm khi bạn xác nhận.",true);
                var panel=overlay.Find("NodeEventPanel");
                var confirm=panel.Find("DialogueNext").GetComponent<Button>();confirm.name="DeliveryConfirm";confirm.GetComponentInChildren<Text>().text="Xác nhận giao";
                confirm.onClick.RemoveAllListeners();confirm.onClick.AddListener(()=>Execute(node,choice,true));
                var cancel=panel.Find("EventClose").GetComponent<Button>();cancel.name="DeliveryCancel";cancel.GetComponentInChildren<Text>().text="Giữ lại / Nói chuyện tiếp";
                cancel.onClick.RemoveAllListeners();cancel.onClick.AddListener(()=>ShowEvent(node));return;
            }
            if(!confirmed&&(choice=="complete"||choice.StartsWith("complete:"))&&quest!=null&&quest.confirmDelivery)
            {
                var reply=game.Events.BuildOptions(node);reply.text=quest.confirmationText;
                foreach(var item in quest.requiredItems)reply.text+="\nGiao "+(game.Content.Item(item.itemId)?.name??item.itemId)+" × "+item.amount;
                DrawDialogue(node,reply,definition,"Chỉ giao vật phẩm khi bạn xác nhận.",true);
                var panel=overlay.Find("NodeEventPanel");
                var confirm=panel.Find("DialogueNext").GetComponent<Button>();confirm.name="DeliveryConfirm";confirm.GetComponentInChildren<Text>().text="Xác nhận giao";
                confirm.onClick.RemoveAllListeners();confirm.onClick.AddListener(()=>Execute(node,choice,true));
                var cancel=panel.Find("EventClose").GetComponent<Button>();cancel.name="DeliveryCancel";cancel.GetComponentInChildren<Text>().text="Giữ lại / Nói chuyện tiếp";
                cancel.onClick.RemoveAllListeners();cancel.onClick.AddListener(()=>ShowEvent(node));
                return;
            }
            var result = game.Events.Execute(node, choice);
            changed?.Invoke();
            if(!string.IsNullOrEmpty(result.checkInId)){ShowCheckIn(node,result.changed);ShowReceipt(result.receivedItems);return;}
            if (result.openBattle) { ShowBattle(node); return; }
            if (result.openPuzzle) { ShowPuzzle(node); return; }
            if (result.openEnding) { ShowEnding(node); return; }
            if (!string.IsNullOrEmpty(result.scene))
            { Close(); game.Scenes.Go(result.scene); return; }
            if (!string.IsNullOrEmpty(result.portalCity))
            {
                Close(); game.Scenes.OpenWorld(result.portalCity);
                return;
            }
            if(choice=="accept" && result.changed && definition!=null && definition.legacyTutorial)
            {
                var reply=game.Events.BuildOptions(node);
                reply.text="Cháu sẽ tìm Mắt Đèn trong hẻm, ghép lõi giấy và tìm cách mở khóa bot. Cháu sẽ trở lại khi đủ linh kiện!";
                DrawDialogue(node,reply,definition,"Đã nhận HQ01 · Nhật ký đã cập nhật.",true);
            }
            else ShowEvent(node,result.message,result.receivedItems);
        }
        void DismissReceipt()
        {
            if(receipt==null)return;
            receipt.gameObject.SetActive(false);UnityEngine.Object.Destroy(receipt.gameObject);receipt=null;
            foreach(var group in receiptBackground)if(group!=null)
            {
                group.alpha=1;group.interactable=group.blocksRaycasts=true;
                var entrance=group.GetComponent<PortraitPresence>();if(entrance!=null){entrance.enabled=true;entrance.Skip();}
            }
            receiptBackground.Clear();
        }
        void ShowReceipt(ItemAmount[] items)
        {
            if(items==null||items.Length==0||overlay==null)return;
            DismissReceipt();
            foreach(Transform child in overlay)
            {
                var group=child.GetComponent<CanvasGroup>()??child.gameObject.AddComponent<CanvasGroup>();
                var entrance=child.GetComponent<PortraitPresence>();if(entrance!=null)entrance.enabled=false;
                group.alpha=.18f;group.interactable=group.blocksRaycasts=false;receiptBackground.Add(group);
            }
            receipt=ui.Panel(overlay,"ReceiptLayer",Vector2.zero,Vector2.one,new Color(.025f,.07f,.10f,.32f));
            var scale=DisplayScale;var width=Mathf.Min(440,ui.Root.rect.width/scale-40);
            var rowHeight=items.Any(i=>i.itemId.Contains("postcard"))?112:88;
            var rowsHeight=Mathf.Min(rowHeight*items.Length,Mathf.Min(352,ui.Root.rect.height/scale-220));
            var height=68+rowsHeight+80;
            var frame=MobileUI.Frame(ui,receipt,"ItemReceipt",Vector2.zero,Vector2.zero,AdventureUIArt.Load().window,2.8f);
            frame.anchorMin=frame.anchorMax=frame.pivot=new Vector2(.5f,.5f);frame.sizeDelta=new Vector2(width,height);frame.localScale=Vector3.one*scale;
            var glow=MobileUI.Symbol(frame,"ReceiptGlow",AdventureSymbol.Aura,new Vector2(-.05f,-.05f),new Vector2(1.05f,1.05f),DesignKitArt.Gold);glow.transform.SetAsFirstSibling();
            var motion=frame.gameObject.AddComponent<PortraitPresence>();motion.important=items.Any(i=>i.itemId.Contains("postcard")||i.itemId=="coin"&&i.amount>=100);
            if(!motion.important)glow.gameObject.SetActive(false);
            var title=ui.Label(frame,"ReceiptTitle","ĐÃ NHẬN",Vector2.zero,Vector2.zero,22,false);title.color=DesignKitArt.Teal;Place(title.rectTransform,24,18,width-90,36);
            var close=MobileUI.IconButton(ui,frame,"ReceiptClose","",Vector2.zero,Vector2.zero,DismissReceipt,AdventureSymbol.Close);Place((RectTransform)close.transform,width-64,10,52,52);
            var viewport=ui.Panel(frame,"ReceiptViewport",Vector2.zero,Vector2.zero,Color.clear);Place(viewport,20,68,width-40,rowsHeight);viewport.gameObject.AddComponent<RectMask2D>();
            var content=new GameObject("ReceiptRows",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(viewport,false);Place(content,0,0,width-40,rowHeight*items.Length);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=24;
            for(var i=0;i<items.Length;i++)
            {
                var item=items[i];var row=ui.Panel(content,"ReceiptRow_"+i,Vector2.zero,Vector2.zero,Color.clear);Place(row,0,i*rowHeight,width-40,rowHeight);
                var icon=ui.Picture(row,"ReceivedIcon_"+item.itemId,item.itemId=="coin"?AdventureUIArt.Load().coin:AdventureUIArt.Load().Item(item.itemId)?.sprite,Vector2.zero,Vector2.zero);Place(icon.rectTransform,4,5,rowHeight-10,rowHeight-10);
                var label=ui.Label(row,"ReceivedName_"+item.itemId,item.itemId=="coin"?"Xu hành trình":game.Content.Item(item.itemId)?.name??item.itemId,Vector2.zero,Vector2.zero,20,false);label.color=DesignKitArt.Ink;label.alignment=TextAnchor.MiddleLeft;Place(label.rectTransform,rowHeight+8,4,width-rowHeight-56,rowHeight-36);
                var amount=ui.Label(row,"ReceivedAmount_"+item.itemId,"× "+item.amount,Vector2.zero,Vector2.zero,22,false);amount.color=DesignKitArt.Teal;amount.alignment=TextAnchor.MiddleLeft;Place(amount.rectTransform,rowHeight+8,rowHeight-32,width-rowHeight-56,30);
            }
            var next=MobileUI.Button(ui,frame,"ReceiptContinue","Tiếp tục",Vector2.zero,Vector2.zero,DismissReceipt,true,true);Place((RectTransform)next.transform,24,height-68,width-48,52);MobileUI.Cue(next,AdventureSymbol.Check);
        }

        void ShowCheckIn(int node,bool newlyRecorded,bool offer=false)
        {
            var entry=game.Events.CheckInAt(node);if(entry==null)return;
            Clear();
            overlay=ui.Panel(ui.Root,"GameplayOverlay",Vector2.zero,Vector2.one,new Color(.025f,.07f,.10f,.28f));
            var sheet=MobileUI.Frame(ui,overlay,"CheckInPanel",new Vector2(.065f,.20f),new Vector2(.935f,.76f),AdventureUIArt.Load().window,2.8f);
            sheet.gameObject.AddComponent<PortraitPresence>().slide=true;
            ui.Label(sheet,"CheckInTitle",entry.title,new Vector2(.07f,.86f),new Vector2(.93f,.97f),23,false).color=DesignKitArt.Ink;
            var picture=new GameObject("CheckInIllustration",typeof(RectTransform),typeof(RawImage)).GetComponent<RawImage>();
            var photoRect=picture.rectTransform;photoRect.SetParent(sheet,false);
            UIController.SetRect(photoRect,new Vector2(.07f,.36f),new Vector2(.93f,.84f));picture.raycastTarget=false;
            var sprite=game.Data.cityId=="hoi_an"?art.hoiAnMap:AdventureUIArt.Load().City(game.Data.cityId)?.sprite;
            picture.texture=sprite?.texture;
            Canvas.ForceUpdateCanvases();
            if(picture.texture!=null)
            {
                var width=.40f;var height=width*picture.texture.width/picture.texture.height/(photoRect.rect.width/photoRect.rect.height);
                var position=game.Data.nodes[node].position;
                picture.uvRect=new Rect(Mathf.Clamp(position.x-width*.5f,0,1-width),Mathf.Clamp(position.y-height*.5f,0,1-height),width,height);
            }
            var body=ui.Label(sheet,"CheckInDescription",entry.description,new Vector2(.08f,.19f),new Vector2(.92f,.34f),17,false);body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperLeft;
            ui.Picture(sheet,"CheckInCoin",AdventureUIArt.Load().coin,new Vector2(.08f,.105f),new Vector2(.15f,.18f));
            ui.Label(sheet,"CheckInReward",offer?"Khám phá địa danh · "+entry.rewardCoins+" xu":newlyRecorded?"Đã ghi dấu · +"+entry.rewardCoins+" xu":"Địa danh đã lưu · Phần thưởng đã nhận",new Vector2(.17f,.105f),new Vector2(.92f,.18f),17,false).color=DesignKitArt.Teal;
            if(offer)
            {
                var record=MobileUI.Button(ui,sheet,"CheckInRecord","Ghi dấu địa danh",new Vector2(.07f,.025f),new Vector2(.63f,.10f),()=>Execute(node,"checkin"),true,true);MobileUI.Cue(record,AdventureSymbol.Camera);
            }
            var exit=MobileUI.Button(ui,sheet,"CheckInClose",offer?"Để sau":"Tiếp tục khám phá",new Vector2(offer ? .66f : .10f,.025f),new Vector2(.93f,.10f),Close,!offer,true);MobileUI.Cue(exit,offer?AdventureSymbol.Close:AdventureSymbol.Camera);
        }

        public void ShowBattle(int node)
        {
            var battle = game.Battles;
            if (battle.Enemy == null)
                battle.Start(game.Events.Definition(game.Data.nodes[node].eventId));
            var sheet = Sheet("BattlePanel", battle.Enemy?.name ?? "Chạm trán");
            ui.Picture(sheet, "BattleLumi", art.lumiExploration, new Vector2(.10f,.53f), new Vector2(.38f,.77f));
            ui.Picture(sheet, "BattleEnemy", AdventureUIArt.Load().Enemy(battle.Enemy?.id)?.sprite??art.enemy, new Vector2(.63f,.54f), new Vector2(.88f,.76f)).useSpriteMesh=true;
            ui.Label(sheet, "BattleLumiName", "Lumi", new Vector2(.07f,.76f), new Vector2(.43f,.82f), 21);
            ui.Label(sheet, "BattleEnemyName", battle.Enemy?.name ?? "", new Vector2(.56f,.76f), new Vector2(.94f,.82f), 21);
            if(!string.IsNullOrEmpty(battle.Enemy?.mechanic))
            {
                UIController.SetRect(sheet.Find("BattleLumiName").GetComponent<RectTransform>(),new Vector2(.07f,.72f),new Vector2(.43f,.77f));
                UIController.SetRect(sheet.Find("BattleEnemyName").GetComponent<RectTransform>(),new Vector2(.56f,.72f),new Vector2(.94f,.77f));
                if(battle.Shielded)
                {
                    for(var i=0;i<battle.Enemy.shieldPattern.Length;i++)
                    {var glyph=i;BattleButton(sheet,node,"BattleStar_"+i,battle.Enemy.shieldPattern[i],"resonate:"+glyph,.07f+i*.29f,.34f+i*.29f,.775f,.835f);}
                }
                else ui.Label(sheet,"BattleMechanic",battle.Enemy.mechanic=="two_phase"?"PHA "+battle.Phase+" / 2 · "+(battle.Phase==2?"Lõi đã lộ":"Phá lớp khiên"):"Khiên mở · Nhịp "+(battle.ShieldBeat%3+1)+" / 3",new Vector2(.07f,.775f),new Vector2(.93f,.835f),17).color=DesignKitArt.Gold;
            }
            Health(sheet, "PlayerHP", battle.PlayerHP, 100, new Vector2(.07f,.48f), new Vector2(.44f,.53f), DesignKitArt.Teal);
            Health(sheet, "EnemyHP", battle.EnemyHP, battle.EnemyMaxHP, new Vector2(.56f,.48f), new Vector2(.93f,.53f), DesignKitArt.Roof);
            var turn = battle.Active ? "Lượt " + battle.TurnNumber + " · Lumi hành động" : battle.Outcome == "win" ? "CHIẾN THẮNG" : battle.Outcome == "loss" ? "LUMI ĐÃ HỒI SỨC · CÓ THỂ THỬ LẠI" : "ĐÃ RÚT LUI";
            ui.Label(sheet, "BattleTurn", turn + "\nNăng lượng ánh sáng: " + battle.Energy + "/3", new Vector2(.06f,.38f), new Vector2(.94f,.47f), 18).color = DesignKitArt.Gold;
            var log = ui.Label(sheet, "BattleLog", battle.Log, new Vector2(.07f,.23f), new Vector2(.93f,.37f), 20);
            log.alignment = TextAnchor.MiddleLeft;
            if (battle.Active)
            {
                BattleButton(sheet,node,"BattleAttack","Tấn công","attack",.07f,.35f,.135f,.22f);
                BattleButton(sheet,node,"BattleDefend","Phòng thủ","defend",.36f,.64f,.135f,.22f);
                BattleButton(sheet,node,"BattleLight","Tia sáng","light",.65f,.93f,.135f,.22f).interactable = battle.Energy > 0;
                BattleButton(sheet,node,"BattleHeal","Trà hồi phục × " + game.Inventory.Count("healing_tea"),"heal",.07f,battle.CanRepair?.35f:.62f,.035f,.12f).interactable = battle.PlayerHP < 100 && game.Inventory.Has("healing_tea");
                if(battle.CanRepair)BattleButton(sheet,node,"BattleRepair","Sửa mạch "+battle.RepairProgress+" / 3","repair",.36f,.64f,.035f,.12f);
                BattleButton(sheet,node,"BattleRun","Rút lui","run",.65f,.93f,.035f,.12f);
            }
            else MobileUI.Button(ui,sheet,"BattleClose","Trở về bản đồ · Tự chọn hướng đi",new Vector2(.07f,.05f),new Vector2(.93f,.145f),Close,true);
        }

        Button BattleButton(RectTransform parent,int node,string name,string label,string action,float left,float right,float bottom,float top)
        {
            return MobileUI.Button(ui,parent,name,label,new Vector2(left,bottom),new Vector2(right,top),() =>
            { game.Battles.Act(action); changed?.Invoke(); ShowBattle(node); },action=="light");
        }

        void Health(RectTransform parent,string name,int value,int maximum,Vector2 min,Vector2 max,Color color)
        {
            var bar=ui.Panel(parent,name+"Bar",min,max,new Color(.08f,.16f,.20f,1));
            var ratio=Mathf.Clamp01((float)value/Math.Max(1,maximum));
            var fill=ui.Panel(bar,"Fill",Vector2.zero,new Vector2(ratio,1),color); fill.GetComponent<Image>().raycastTarget=false;
            ui.Label(bar,name,value+" / "+maximum+" HP",Vector2.zero,Vector2.one,17);
        }

        void ShowEnding(int node,int replayPage=0)
        {
            var definition=game.Events.Definition(game.Data.nodes[node].eventId);
            var complete=game.World.Has(definition.completionFlag);
            var index=complete?replayPage:game.Events.EndingPage(definition);
            index=Mathf.Clamp(index,0,definition.endingPages.Length-1);
            var sheet=Sheet("EpiloguePanel",index==6?"LIGHT BETWEEN CULTURES":"Lễ hội sáu thành phố");
            var cities=new[]{"hoi_an","hue","da_lat","jeonju","gyeongju","seoul"};
            var sprite=index<6?AdventureUIArt.Load().City(cities[index])?.sprite:art.lumiPortrait;
            ui.Picture(sheet,"EndingCityImage",sprite,new Vector2(.12f,.49f),new Vector2(.88f,.82f));
            var text=definition.endingPages[index];
            if(index==4)text+=game.World.Has("gyeongju.stone_guard_friend")?" Tượng đá đã trở thành người bạn sau khi được sửa.":" Tượng đá ghi nhớ trận đấu và đã dừng giữ lối.";
            if(index==5)text+=game.World.Has("seoul.system_repaired")?" Lumi đã cứu cả hệ phòng vệ.":" Lõi lỗi đã dừng, những cơ cấu an toàn vẫn được giữ lại.";
            if(index==3&&game.Quests.IsCompleted("HQ07"))text+=" Bà Tư giữ bưu thiếp giao lưu từ hai chuyến quà.";
            var body=ui.Label(sheet,"EndingText",text,new Vector2(.07f,.19f),new Vector2(.93f,.46f),20);body.alignment=TextAnchor.UpperLeft;
            ui.Label(sheet,"EndingPage",(index+1)+" / "+definition.endingPages.Length,new Vector2(.07f,.135f),new Vector2(.93f,.18f),16).color=DesignKitArt.Gold;
            MobileUI.Button(ui,sheet,"EndingNext",index+1==definition.endingPages.Length?"Lưu lời kết · Tiếp tục khám phá":"Tiếp tục câu chuyện",new Vector2(.07f,.04f),new Vector2(.93f,.12f),()=>
            {
                if(!complete){game.Events.Execute(node,"ending_next");changed?.Invoke();}
                if(index+1==definition.endingPages.Length)Close();else ShowEnding(node,index+1);
            },true);
        }

        public void ShowPuzzle(int node)
        {
            var puzzle = game.Puzzles;
            if (puzzle.Definition == null)
                puzzle.Start(game.Events.Definition(game.Data.nodes[node].eventId)?.puzzleId);
            var definition = puzzle.Definition;
            var sheet = Sheet("PuzzlePanel", definition?.title ?? "Câu đố");
            var clue=ui.Label(sheet,"PuzzleClue",definition?.clue ?? "Câu đố chưa được cấu hình.",new Vector2(.07f,.60f),new Vector2(.93f,.82f),22);
            clue.alignment=TextAnchor.UpperLeft;
            var feedback=ui.Label(sheet,"PuzzleLog",puzzle.Log,new Vector2(.07f,.48f),new Vector2(.93f,.59f),19);
            feedback.color=DesignKitArt.Gold;
            var inputPanel=ui.Panel(sheet,"PuzzleAnswer",new Vector2(.07f,.35f),new Vector2(.93f,.44f),DesignKitArt.Cream);
            MobileUI.Round(inputPanel);
            var answer=inputPanel.gameObject.AddComponent<InputField>();
            var answerText=ui.Label(inputPanel,"AnswerText","",new Vector2(.04f,.08f),new Vector2(.96f,.92f),22);
            answerText.color=DesignKitArt.Ink; answerText.alignment=TextAnchor.MiddleLeft;
            answer.textComponent=answerText; answer.characterLimit=80;
            var placeholder=ui.Label(inputPanel,"Placeholder","Chạm biểu tượng hoặc nhập đáp án",new Vector2(.04f,.08f),new Vector2(.96f,.92f),18);
            placeholder.color=new Color(.15f,.30f,.35f,.6f); placeholder.alignment=TextAnchor.MiddleLeft; answer.placeholder=placeholder;
            answer.interactable=puzzle.Active;
            var options=definition?.options ?? Array.Empty<string>();
            for(var i=0;i<options.Length;i++)
            {
                if(definition.mode=="rings")break;
                var index=i; var width=.86f/Math.Max(1,options.Length);
                var button=MobileUI.Button(ui,sheet,"PuzzleOption_"+i,options[i],new Vector2(.07f+i*width,.24f),new Vector2(.07f+(i+1)*width-.012f,.325f),()=>
                { answer.text=definition.mode=="sequence" && answer.text.Length>0?answer.text+"|"+options[index]:options[index]; });
                button.interactable=puzzle.Active;
            }
            if(definition?.mode=="fold")
            {
                inputPanel.gameObject.SetActive(false);
                var paper=ui.Picture(sheet,"HanjiPaper",AdventureUIArt.Load().window,new Vector2(.12f,.27f),new Vector2(.38f,.46f));
                MobileUI.Skin(paper,AdventureUIArt.Load().window,2);
                ui.Panel(paper.transform,"FoldGuide",new Vector2(.49f,.10f),new Vector2(.51f,.90f),new Color(.12f,.57f,.54f,.8f)).GetComponent<Image>().raycastTarget=false;
                paper.rectTransform.localEulerAngles=new Vector3(0,0,puzzle.Rotations*90);
                paper.rectTransform.localScale=new Vector3(puzzle.Folds>0?.55f:1,1,1);
                var rotate=MobileUI.Button(ui,sheet,"HanjiRotate","Xoay 90°",new Vector2(.45f,.35f),new Vector2(.93f,.44f),()=>{puzzle.FoldStep(false);ShowPuzzle(node);});rotate.interactable=puzzle.Active;MobileUI.Cue(rotate,AdventureSymbol.Arrow);
                MobileUI.Button(ui,sheet,"HanjiFold","Gấp theo vạch sáng",new Vector2(.45f,.24f),new Vector2(.93f,.33f),()=>{puzzle.FoldStep(true);ShowPuzzle(node);}).interactable=puzzle.Active;
            }
            if(definition?.mode=="rings")
            {
                inputPanel.gameObject.SetActive(false);
                for(var i=0;i<3;i++)
                {
                    var ring=i;var left=.08f+i*.29f;
                    var wheel=ui.Picture(sheet,"RingVisual_"+i,AdventureUIArt.Load().Marker("Ground"),new Vector2(left,.26f),new Vector2(left+.24f,.46f));
                    wheel.rectTransform.localEulerAngles=new Vector3(0,0,puzzle.Rings[i]*120);
                    var control=MobileUI.Button(ui,sheet,"LanternRing_"+i,options[puzzle.Rings[i]],new Vector2(left,.27f),new Vector2(left+.24f,.36f),()=>{puzzle.RotateRing(ring);ShowPuzzle(node);});control.interactable=puzzle.Active;MobileUI.Cue(control,AdventureSymbol.Arrow);
                }
            }
            var submit=MobileUI.Button(ui,sheet,"PuzzleSubmit","Xác nhận",new Vector2(.38f,.135f),new Vector2(.93f,.22f),()=>
            {
                var solved=puzzle.Submit(definition?.mode=="fold"?definition.answer:definition?.mode=="rings"?puzzle.RingAnswer:answer.text); feedback.text=puzzle.Log;
                if(solved) { changed?.Invoke(); ShowPuzzle(node); }
            },true);
            submit.interactable=puzzle.Active;
            MobileUI.Cue(submit,AdventureSymbol.Check);
            MobileUI.Button(ui,sheet,"PuzzleClear","Xóa",new Vector2(.07f,.135f),new Vector2(.35f,.22f),()=>{if(definition?.mode=="fold"||definition?.mode=="rings"){puzzle.ResetInteraction();ShowPuzzle(node);}else answer.text="";}).interactable=puzzle.Active;
            MobileUI.Button(ui,sheet,"PuzzleClose","Trở về bản đồ",new Vector2(.07f,.035f),new Vector2(.93f,.12f),()=>{puzzle.Cancel();Close();});
        }

        void Clear()
        {
            receipt=null;receiptBackground.Clear();
            if(overlay==null)return;
            overlay.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(overlay.gameObject); overlay=null;
        }
        void Close()
        {
            Debug.Log("Gameplay panel close at "+game.Data.nodes[game.Save.node].id);
            Clear(); changed?.Invoke(); closed?.Invoke();
        }
    }
}
