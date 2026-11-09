using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        public Action<int> CatDestination;
        Text catFeedback;
        Button catSubmit;
        RectTransform CatSheet(string title, string subtitle, out float w, out float h)
        {
            var sheet=PhotoSheet("CatRescuePanel",title,subtitle,out w,out h);
            sheet.Find("PhotoExit").name="CatClose";
            // Shared header always has the visible close X (no progress is lost).
            sheet.Find("PhotoHeading").GetComponent<Text>().color=new Color(.08f,.14f,.18f,1f);
            sheet.Find("PhotoCity").GetComponent<Text>().color=new Color(.10f,.24f,.28f,1f);
            return sheet;
        }
        Text CatText(Transform parent,string name,string text,Vector2 min,Vector2 max,int size=18)
        {
            var t=ui.Label(parent,name,text,min,max,size);
            t.color=new Color(.08f,.14f,.18f,1f); // Opaque dark text on parchment
            t.resizeTextMinSize=14;
            return t;
        }
        Image CatPortrait(Transform parent,CatRescueChapter c,Vector2 min,Vector2 max,bool happy=false,string name="RescueCat")
        {
            var image=ui.Picture(parent,name,CatRescueArt.Cat(c.spriteIndex),min,max);
            image.gameObject.AddComponent<CatRescueMotion>().rescued=happy;return image;
        }
        void CatGo(int index)
        {
            if(index<0){ShowCatRescue("Không có mục tiêu khả dụng. Hãy xem nhiệm vụ hiện tại.");return;}
            // Do not send players back to the same panel when they are already
            // standing at the target node. Open the actual required interaction.
            if(game.Save.node==index){ShowCatArrival();return;}
            Close();CatDestination?.Invoke(index);
        }
        public void ShowCatArrival()
        {
            var cats=game.Cats;var chapter=cats.Current;
            if(chapter==null){ShowCatRescue("Chưa có nhiệm vụ cứu mèo ở khu vực này.");return;}
            var progress=cats.Progress(chapter.cityId);
            if(progress?.started!=true){ShowCatRescue("Chọn NHẬN NHIỆM VỤ trước khi đi giải cứu mèo.");return;}
            var challenge=cats.Next(chapter);
            if(challenge!=null && cats.At(challenge.nodeId)){ShowCatChallenge(challenge);return;}
            if(cats.CanRescue){ShowCatRescueFinal();return;}
            var supply=chapter.supplies.FirstOrDefault(s=>!progress.supplies.Contains(s.id) && cats.At(s.nodeId));
            if(supply!=null)
            {
                ShowCatRescue("Đã tới nơi! Chạm LẤY "+supply.name+" trong phần Túi cứu hộ.");
                return;
            }
            ShowCatRescue("Đã tới nơi. Xem BƯỚC TIẾP THEO và chọn hành động tương ứng.");
        }
        RectTransform CatScroll(RectTransform sheet,float w,float h,out RectTransform content)
        {
            var viewport=ui.Panel(sheet,"CatScrollViewport",Vector2.zero,Vector2.zero,Color.clear);
            Place(viewport,18,110,w-36,h-352);viewport.gameObject.AddComponent<RectMask2D>();
            content=ui.Panel(viewport,"CatScrollContent",new Vector2(0,1),Vector2.one,Color.clear);
            content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,1000);
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=30;
            return viewport;
        }
        public void ShowCatRescue(string notice=null)
        {
            var cats=game.Cats;var c=cats.Current;if(c==null)return;
            var p=cats.Progress(c.cityId);var next=cats.Next(c);
            var sheet=CatSheet(c.title,game.Content.City(c.cityId).name+" · Giải cứu mèo "+cats.RescuedCount+" / 6",out var w,out var h);
            CatScroll(sheet,w,h,out var content);var cw=w-36;float y=0;
            var image=ui.Picture(content,"RescueCity",CatRescueArt.Image("city:"+c.cityId),Vector2.zero,Vector2.zero);
            Place(image.rectTransform,8,y,cw-16,170);image.color=Color.white;
            var cat=CatPortrait(content,c,Vector2.zero,Vector2.zero,p?.rescued==true);Place(cat.rectTransform,cw-148,y+10,140,158);
            var weather=content.gameObject.AddComponent<CanvasGroup>();weather.blocksRaycasts=true;
            y+=180;
            var opening=p?.rescued==true?c.catName+": “"+c.catLine+"”\n"+c.postcard:c.opening;
            var th=Mathf.Max(88,TextHeight(opening,cw-32,18)+18);
            var story=CatText(content,"CatOpening",opening,Vector2.zero,Vector2.zero,19);story.alignment=TextAnchor.UpperLeft;Place(story.rectTransform,16,y,cw-32,th);y+=th+12;
            var missingSupply=p==null?null:c.supplies.FirstOrDefault(s=>!p.supplies.Contains(s.id));
            var nextAction=p?.started!=true ? "BƯỚC 1: Nhận nhiệm vụ cứu mèo bên dưới." :
                next!=null ? "TIẾP THEO ("+(p.completed.Count+1)+"/4): "+next.title+". Đến "+cats.Place(next.nodeId)+" và chơi thử thách." :
                missingSupply!=null ? "TIẾP THEO: Nhặt "+missingSupply.name+" tại "+cats.Place(missingSupply.nodeId)+"." :
                p.rescued ? "ĐÃ CỨU MÈO: Mở sổ mèo để xem bưu thiếp." :
                "BƯỚC CUỐI: Đến "+cats.Place(c.rescueNode)+", kéo mèo vào giỏ cứu hộ.";
            var stepPanel=ui.Panel(content,"CatCurrentStep",Vector2.zero,Vector2.zero,DesignKitArt.Hex("F2E2BE"));
            Place(stepPanel,10,y,cw-20,88);MobileUI.Round(stepPanel);stepPanel.GetComponent<Image>().raycastTarget=false;
            var stepText=CatText(stepPanel,"CatCurrentStepText",nextAction,new Vector2(.04f,.07f),new Vector2(.96f,.93f),19);
            stepText.alignment=TextAnchor.MiddleLeft;
            y+=100;
            if(p?.started!=true)
            {
                var start=MobileUI.Button(ui,content,"CatStart","Nhận nhiệm vụ · Tìm "+c.catName,Vector2.zero,Vector2.zero,()=>{cats.Start();changed?.Invoke();ShowCatRescue(cats.Message);},true,true);
                Place((RectTransform)start.transform,12,y,cw-24,58);y+=78;
            }
            else
            {
                var header=CatText(content,"CatSupplyHeading","TÚI CỨU HỘ · "+p.supplies.Count+" / "+c.supplies.Length,Vector2.zero,Vector2.zero,17);
                header.alignment=TextAnchor.MiddleLeft;Place(header.rectTransform,16,y,cw-32,30);y+=34;
                if(p.supplies.Count==c.supplies.Length)
                {
                    var allReady=CatText(content,"CatSuppliesReady","ĐÃ ĐỦ "+c.supplies.Length+" VẬT DỤNG — tập trung vào thử thách tiếp theo.",Vector2.zero,Vector2.zero,18);
                    Place(allReady.rectTransform,16,y,cw-32,44);y+=50;
                }
                else foreach(var supply in c.supplies)
                {
                    var s=supply;var owned=p.supplies.Contains(s.id);var atSupply=cats.At(s.nodeId);
                    var label=(owned?"Đã có · ":atSupply?"Lấy · ":"Tìm · ")+s.name+"\n"+cats.Place(s.nodeId);
                    var b=MobileUI.Button(ui,content,"CatSupply_"+s.id,label,Vector2.zero,Vector2.zero,()=>
                    {
                        if(!atSupply){CatGo(Array.FindIndex(game.Data.nodes,n=>n.id==s.nodeId));return;}
                        cats.Collect(s.id);changed?.Invoke();ShowCatRescue(cats.Message);
                    },atSupply&&!owned,true);
                    Place((RectTransform)b.transform,12,y,cw-24,62);b.interactable=!owned&&!p.rescued;
                    var icon=CatRescueArt.Image(s.artId);
                    if(icon!=null)
                    {
                        ui.Picture(b.transform,"SupplyIcon",icon,new Vector2(.02f,.10f),new Vector2(.14f,.90f));
                        UIController.SetRect(b.transform.Find("Label").GetComponent<RectTransform>(),new Vector2(.16f,.06f),new Vector2(.96f,.94f));
                    }
                    y+=70;
                }
                y+=12;var trials=CatText(content,"CatTrialHeading","THỬ THÁCH VĂN HÓA · "+p.completed.Count+" / "+c.challenges.Length,Vector2.zero,Vector2.zero,17);
                trials.alignment=TextAnchor.MiddleLeft;Place(trials.rectTransform,16,y,cw-32,30);y+=34;
                foreach(var challenge in c.challenges)
                {
                    var a=challenge;var done=p.completed.Contains(a.id);var current=next==a;
                    var label=(done?"Đã xong · ":current?"Tiếp theo · ":"Chưa mở · ")+a.title+"\n"+cats.Place(a.nodeId);
                    var b=MobileUI.Button(ui,content,"CatTask_"+a.id,label,Vector2.zero,Vector2.zero,()=>
                    {
                        if(done){ShowCatFact(c,a);return;}
                        if(cats.At(a.nodeId))ShowCatChallenge(a);else CatGo(Array.FindIndex(game.Data.nodes,n=>n.id==a.nodeId));
                    },current,true);
                    Place((RectTransform)b.transform,12,y,cw-24,done?44:68);b.interactable=done||current;
                    if(done) b.GetComponentInChildren<Text>().color=new Color(.12f,.23f,.26f,1f);
                    y+=done?50:76;
                }
                y+=12;
                var rescue=MobileUI.Button(ui,content,"CatRescueAction",p.rescued?c.catName+" đã an toàn":"Đón "+c.catName+" · "+cats.Place(c.rescueNode),Vector2.zero,Vector2.zero,()=>
                {
                    if(cats.CanRescue)ShowCatRescueFinal();else if(next==null&&c.supplies.All(s=>p.supplies.Contains(s.id)))CatGo(Array.FindIndex(game.Data.nodes,n=>n.id==c.rescueNode));
                    else ShowCatRescue("Hoàn thành bốn thử thách và thu đủ ba vật dụng để mở khu vực cứu hộ.");
                },true,true);
                Place((RectTransform)rescue.transform,12,y,cw-24,64);rescue.interactable=!p.rescued;y+=80;
                if(p.rescued)
                {
                    var reward=CatText(content,"CatEarnedRewards",c.badge+"\nBưu thiếp văn hóa · +"+c.rewardCoins+" xu · "+c.catName,Vector2.zero,Vector2.zero,18);
                    Place(reward.rectTransform,12,y,cw-24,84);y+=90;
                }
            }
            content.sizeDelta=new Vector2(0,y+12);
            catFeedback=CatText(sheet,"CatNotice",notice??cats.Guidance(),Vector2.zero,Vector2.zero,17);
            catFeedback.color=new Color(.08f,.20f,.23f,1f);catFeedback.alignment=TextAnchor.MiddleLeft;
            Place(catFeedback.rectTransform,24,h-229,w-48,64);
            var back=MobileUI.Button(ui,sheet,"CatReturnMap","← VỀ BẢN ĐỒ (có thể quay lại sau)",Vector2.zero,Vector2.zero,Close,illustrated:true);
            Place((RectTransform)back.transform,24,h-155,w-48,46);
            var here=game.Save.node==cats.TargetIndex();
            var mainAction=p?.started!=true?"NHẬN NHIỆM VỤ":p?.rescued==true?"XEM SỔ MÈO":
                cats.CanRescue?"CỨU MÈO NGAY":here?"CHƠI / NHẬN TẠI ĐÂY":"ĐI ĐẾN MỤC TIÊU";
            var focus=MobileUI.Button(ui,sheet,"CatFocus",mainAction,Vector2.zero,Vector2.zero,()=>
            {
                if(p?.started!=true){cats.Start();changed?.Invoke();ShowCatRescue(cats.Message);}
                else if(p.rescued)ShowCatCollection(Array.IndexOf(cats.Catalog.chapters,c));
                else CatGo(cats.TargetIndex());
            },true,true);
            Place((RectTransform)focus.transform,24,h-89,(w-56)*.57f,54);
            focus.GetComponentInChildren<Text>().color=Color.white;
            var album=MobileUI.Button(ui,sheet,"CatCollection","Sổ mèo · "+cats.RescuedCount+"/6",Vector2.zero,Vector2.zero,()=>ShowCatCollection(0),illustrated:true);
            Place((RectTransform)album.transform,32+(w-56)*.57f,h-89,(w-56)*.43f,54);
        }
        public void ShowCatCollection(int index)
        {
            var cats=game.Cats;var chapters=cats.Catalog.chapters;index=Mathf.Clamp(index,0,chapters.Length-1);var c=chapters[index];var owned=cats.Rescued(c.cityId);
            var sheet=CatSheet("Sổ mèo · "+cats.RescuedCount+" / 6",game.Content.City(c.cityId).name+" · "+(index+1)+" / 6",out var w,out var h);
            var picture=CatPortrait(sheet,c,new Vector2(.20f,.44f),new Vector2(.80f,.86f),owned);picture.color=owned?Color.white:new Color(.32f,.42f,.43f,.60f);
            var info=owned?c.catName+"\n“"+c.catLine+"”\n"+c.catTrait:c.catName+" đang chờ bạn\nGiải cứu tại "+game.Content.City(c.cityId).name;
            CatText(sheet,"CatProfile",info,new Vector2(.07f,.29f),new Vector2(.93f,.44f),21);
            var badge=CatText(sheet,"CatBadge",owned?"HUY HIỆU · "+c.badge:"Huy hiệu chưa mở",new Vector2(.06f,.235f),new Vector2(.94f,.29f),17);badge.color=DesignKitArt.Teal;
            var prev=MobileUI.Button(ui,sheet,"CatPrevious","Trước",new Vector2(.05f,.06f),new Vector2(.31f,.13f),()=>ShowCatCollection(index-1),illustrated:true);prev.interactable=index>0;
            MobileUI.Button(ui,sheet,"CatAlbumBack","Nhiệm vụ",new Vector2(.35f,.06f),new Vector2(.65f,.13f),()=>ShowCatRescue(),illustrated:true);
            var next=MobileUI.Button(ui,sheet,"CatNext","Sau",new Vector2(.69f,.06f),new Vector2(.95f,.13f),()=>ShowCatCollection(index+1),illustrated:true);next.interactable=index<chapters.Length-1;
            var postcard=MobileUI.Button(ui,sheet,"CatPostcard","Xem bưu thiếp văn hóa",new Vector2(.10f,.15f),new Vector2(.90f,.22f),()=>ShowCatPostcard(c,index),true,true);postcard.interactable=owned;
        }
        void ShowCatPostcard(CatRescueChapter c,int index)
        {
            var sheet=CatSheet("Bưu thiếp · "+game.Content.City(c.cityId).name,c.badge,out var w,out var h);
            ui.Picture(sheet,"CatPostcardArt",CatRescueArt.Image("city:"+c.cityId),new Vector2(.07f,.43f),new Vector2(.93f,.86f));
            CatText(sheet,"CatPostcardText",c.postcard,new Vector2(.08f,.22f),new Vector2(.92f,.42f),21);
            MobileUI.Button(ui,sheet,"CatPostcardBack","Về sổ mèo",new Vector2(.15f,.07f),new Vector2(.85f,.16f),()=>ShowCatCollection(index),illustrated:true);
        }
        public void ShowCatChallenge(CatRescueChallenge c)
        {
            var cats=game.Cats;
            if(c==null||cats.Next(cats.Current)!=c||!cats.At(c.nodeId)){ShowCatRescue("Đến đúng địa điểm để bắt đầu thử thách.");return;}
            var sheet=CatSheet(c.title,cats.Place(c.nodeId)+" · Bước "+(cats.Progress(cats.Current.cityId).completed.Count+1)+" / 4",out var w,out var h);
            var instructions=CatText(sheet,"CatInstructions",c.instructions,Vector2.zero,Vector2.zero,18);Place(instructions.rectTransform,24,106,w-48,78);
            var board=ui.Panel(sheet,"CatChallengeBoard",Vector2.zero,Vector2.zero,DesignKitArt.Hex("DCE6D6"));
            Place(board,24,193,w-48,h-410);MobileUI.Round(board);board.gameObject.AddComponent<RectMask2D>();
            catFeedback=CatText(sheet,"CatFeedback","Mỗi thao tác đúng được lưu. Sai có thể thử lại.",Vector2.zero,Vector2.zero,17);Place(catFeedback.rectTransform,24,h-205,w-48,75);catFeedback.color=DesignKitArt.Teal;
            catSubmit=MobileUI.Button(ui,sheet,"CatSubmit","Hoàn tất thử thách",Vector2.zero,Vector2.zero,()=>
            {
                if(!cats.Complete(c.id)){CatFeedback(false);return;}
                changed?.Invoke();ShowCatFact(cats.Current,c);
            },true,true);
            Place((RectTransform)catSubmit.transform,24,h-76,w-48,50);catSubmit.interactable=cats.Ready(c);
            var hint=MobileUI.Button(ui,sheet,"CatHint","Gợi ý",Vector2.zero,Vector2.zero,()=>catFeedback.text=c.hint,illustrated:true);Place((RectTransform)hint.transform,24,h-125,(w-56)*.45f,42);
            var leave=MobileUI.Button(ui,sheet,"CatLeave","Về nhiệm vụ",Vector2.zero,Vector2.zero,()=>ShowCatRescue(),illustrated:true);Place((RectTransform)leave.transform,32+(w-56)*.45f,h-125,(w-56)*.55f,42);
            if(c.mode=="choice"||c.mode=="scenario")CatChoices(board,c);
            else if(c.mode=="maze")CatMaze(board,c);
            else if(c.mode=="hidden")CatHidden(board,c);
            else if(c.mode=="hanbok")CatDressing(board,c);
            else CatMatching(board,c);
        }
        void CatFeedback(bool ok)
        {
            if(catFeedback==null)return;
            catFeedback.text=game.Cats.Message;catFeedback.color=ok?DesignKitArt.Teal:DesignKitArt.Hex("A34836");
            if(catSubmit!=null)catSubmit.interactable=game.Cats.Ready(game.Cats.Next(game.Cats.Current));
        }
        void CatChoices(RectTransform board,CatRescueChallenge c)
        {
            var picture=CatRescueArt.Image(c.imageId);
            if(picture!=null)ui.Picture(board,"CatQuestionArt",picture,new Vector2(.25f,.67f),new Vector2(.75f,.98f));
            else CatPortrait(board,game.Cats.Current,new Vector2(.36f,.68f),new Vector2(.64f,.98f));
            for(var i=0;i<c.options.Length;i++)
            {
                var j=i;var y=.63f-i*.20f;
                MobileUI.Button(ui,board,"CatChoice"+i,c.options[i],new Vector2(.05f,y-.17f),new Vector2(.95f,y),()=>CatFeedback(game.Cats.Choose(c.id,j)),illustrated:true);
            }
        }
        void CatMatching(RectTransform board,CatRescueChallenge c)
        {
            if(c.mode=="drag"){CatCooking(board,c);return;}
            var puzzle=c.mode=="jigsaw";var count=c.tokens.Length;var rects=new RectTransform[count];
            for(var i=0;i<count;i++)
            {
                var p=puzzle?new Vector2(.32f+(i%2)*.36f,i<2?.76f:.46f):new Vector2(.75f,.83f-i*(.67f/Mathf.Max(1,count-1)));
                var size=puzzle?new Vector2(.34f,.28f):new Vector2(.43f,.17f);
                rects[i]=ui.Panel(board,"CatTarget"+i,p-size*.5f,p+size*.5f,new Color(.18f,.40f,.41f,.35f));
                MobileUI.Round(rects[i]);rects[i].GetComponent<Image>().raycastTarget=false;
                if(puzzle)
                {var ghost=ui.Picture(rects[i],"PuzzleOutline",CatRescueArt.Piece(c.imageId,i),Vector2.zero,Vector2.one);ghost.color=new Color(1,1,1,.24f);}
                else CatText(rects[i],"CatTargetLabel",c.targets[i],new Vector2(.04f,.03f),new Vector2(.96f,.97f),16);
            }
            var order=Enumerable.Range(0,count).OrderBy(i=>(i*3+1)%(count+1)).ToArray();
            for(var slot=0;slot<count;slot++)
            {
                var i=order[slot];var marked=game.Cats.State(c)?.marks.Contains(i)==true;
                var p=puzzle?new Vector2((slot+.5f)/count,.13f):new Vector2(.24f,.83f-slot*(.67f/Mathf.Max(1,count-1)));
                var size=puzzle?new Vector2(.21f,.22f):new Vector2(.39f,.16f);
                var tile=ui.Panel(board,"CatPiece"+i,p-size*.5f,p+size*.5f,DesignKitArt.Hex("EBC680"));MobileUI.Round(tile);
                if(puzzle)ui.Picture(tile,"PuzzleArt",CatRescueArt.Piece(c.imageId,i),new Vector2(.015f,.015f),new Vector2(.985f,.985f));
                else if(c.mode=="hangul")
                {
                    var glyph=new GameObject("HangeulLetter",typeof(RectTransform),typeof(CatHangulGraphic)).GetComponent<CatHangulGraphic>();glyph.transform.SetParent(tile,false);UIController.SetRect(glyph.rectTransform,new Vector2(.06f,.15f),new Vector2(.36f,.85f));glyph.color=DesignKitArt.Ink;glyph.letter=i;glyph.raycastTarget=false;
                    CatText(tile,"CatTokenLabel",c.tokens[i],new Vector2(.38f,.05f),new Vector2(.97f,.95f),15);
                }
                else CatText(tile,"CatTokenLabel",c.tokens[i],new Vector2(.03f,.03f),new Vector2(.97f,.97f),17);
                var drag=tile.gameObject.AddComponent<CatDragHandle>();drag.targets=rects;drag.drop=target=>game.Cats.Match(c.id,i,target);drag.feedback=CatFeedback;
                if(marked){drag.placed=true;tile.SetParent(rects[i],false);UIController.SetRect(tile,new Vector2(.06f,.06f),new Vector2(.94f,.94f));}
            }
            if(puzzle)
            {var sample=ui.Picture(board,"JigsawReference",CatRescueArt.Image(c.imageId),new Vector2(.01f,.88f),new Vector2(.18f,.99f));sample.color=new Color(1,1,1,.9f);}
        }
        CatRescueTokenGraphic CatToken(Transform parent,string name,int kind,Vector2 min,Vector2 max,float alpha=1)
        {
            var graphic=new GameObject(name,typeof(RectTransform),typeof(CatRescueTokenGraphic)).GetComponent<CatRescueTokenGraphic>();graphic.transform.SetParent(parent,false);UIController.SetRect(graphic.rectTransform,min,max);graphic.kind=kind;graphic.raycastTarget=false;graphic.color=new Color(1,1,1,alpha);return graphic;
        }
        void CatCooking(RectTransform board,CatRescueChallenge c)
        {
            CatToken(board,"BibimbapBowl",5,new Vector2(.40f,.20f),new Vector2(.99f,.92f));
            var rects=new RectTransform[c.tokens.Length];
            for(var i=0;i<rects.Length;i++)
            {
                var p=new Vector2(i%2==0?.57f:.82f,i<2?.67f:.40f);
                rects[i]=ui.Panel(board,"CatTarget"+i,p-new Vector2(.11f,.12f),p+new Vector2(.11f,.12f),Color.clear);rects[i].GetComponent<Image>().raycastTarget=false;
                CatToken(rects[i],"IngredientGuide",i,new Vector2(.05f,.23f),new Vector2(.95f,.98f),.22f);
                CatText(rects[i],"BowlSection",c.tokens[i],new Vector2(-.02f,.01f),new Vector2(1.02f,.24f),13).color=DesignKitArt.Teal;
            }
            var order=new[]{2,0,3,1};
            for(var row=0;row<c.tokens.Length;row++)
            {
                var i=order[row];var p=new Vector2(.19f,.85f-row*.23f);
                var tile=ui.Panel(board,"CatPiece"+i,p-new Vector2(.15f,.105f),p+new Vector2(.15f,.105f),DesignKitArt.Hex("F0D69E"));MobileUI.Round(tile);
                CatToken(tile,"IngredientArt",i,new Vector2(.12f,.24f),new Vector2(.88f,.99f));
                CatText(tile,"IngredientName",c.tokens[i],new Vector2(.02f,.01f),new Vector2(.98f,.25f),15);
                var drag=tile.gameObject.AddComponent<CatDragHandle>();drag.targets=rects;drag.drop=target=>game.Cats.Match(c.id,i,target);drag.feedback=CatFeedback;
                if(game.Cats.State(c)?.marks.Contains(i)==true){drag.placed=true;tile.SetParent(rects[i],false);UIController.SetRect(tile,new Vector2(.04f,.04f),new Vector2(.96f,.96f));}
            }
            CatText(board,"BibimbapCaption","Bát bibimbap của bạn",new Vector2(.39f,.04f),new Vector2(.99f,.15f),17).color=DesignKitArt.Teal;
        }
        void CatHidden(RectTransform board,CatRescueChallenge c)
        {
            var bg=ui.Picture(board,"HiddenScenery",CatRescueArt.Image(c.imageId),Vector2.zero,Vector2.one);bg.color=new Color(1,1,1,.90f);
            var cover=bg.gameObject.AddComponent<AspectRatioFitter>();cover.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;cover.aspectRatio=bg.sprite.rect.width/bg.sprite.rect.height;
            var spots=new[]{new Vector2(.17f,.72f),new Vector2(.67f,.35f),new Vector2(.80f,.79f),new Vector2(.40f,.60f),new Vector2(.29f,.27f),new Vector2(.83f,.15f)};
            for(var i=0;i<spots.Length;i++)
            {
                var j=i;var target=i<c.tokens.Length;var found=target&&game.Cats.State(c)?.marks.Contains(i)==true;
                var p=spots[(i+game.Cats.Current.spriteIndex*2)%spots.Length];var button=MobileUI.Button(ui,board,"CatHidden"+i,"",p,p,()=>
                {
                    var ok=game.Cats.Find(c.id,j);CatFeedback(ok);
                    if(ok){var b=board.Find("CatHidden"+j);if(b!=null){b.GetComponent<Button>().interactable=false;var label=b.GetComponentInChildren<Text>();label.text="Đã tìm";label.transform.SetAsLastSibling();}}
                },illustrated:true);
                button.interactable=!found;
                ((RectTransform)button.transform).sizeDelta=new Vector2(66,66);
                button.transform.Find("RoundedBackground").gameObject.SetActive(false);
                var caption=button.GetComponentInChildren<Text>();UIController.SetRect(caption.rectTransform,new Vector2(-.15f,-.32f),new Vector2(1.15f,.03f));caption.color=DesignKitArt.Cream;caption.fontSize=14;
                var type=target&&i<c.tokenArt.Length?c.tokenArt[i]:"decoration";
                if(type=="cat")ui.Picture(button.transform,"HiddenCat",CatRescueArt.Cat(game.Cats.Current.spriteIndex),new Vector2(.03f,.04f),new Vector2(.97f,.96f));
                else if(type=="flower")
                {
                    CatToken(button.transform,"HiddenFlower",4,new Vector2(.02f,.02f),new Vector2(.98f,.98f));
                }
                else if(type=="star")
                {
                    var star=new GameObject("HiddenStar",typeof(RectTransform),typeof(CatStarGraphic)).GetComponent<CatStarGraphic>();star.transform.SetParent(button.transform,false);UIController.SetRect(star.rectTransform,new Vector2(.16f,.16f),new Vector2(.84f,.84f));star.color=DesignKitArt.Gold;star.raycastTarget=false;
                }
                else ui.Picture(button.transform,"HiddenDecoration",i%2==0?AdventureUIArt.Load().activityCitrusPot:Resources.Load<Sprite>("LeisureProps/DaLat_KiteRack"),new Vector2(.02f,.02f),new Vector2(.98f,.98f));
                if(found)button.GetComponentInChildren<Text>().text="Đã tìm";
            }
            CatText(board,"HiddenProgress","Tìm "+c.tokens.Length+" dấu vết · Chạm để quan sát",new Vector2(.04f,.01f),new Vector2(.96f,.10f),15);
        }
        void CatDressing(RectTransform board,CatRescueChallenge c)
        {
            var state=game.Cats.State(c);var rects=new RectTransform[3];
            for(var figure=0;figure<2;figure++)
            {
                var g=new GameObject("HanbokPreview"+figure,typeof(RectTransform),typeof(CatHanbokGraphic)).GetComponent<CatHanbokGraphic>();g.transform.SetParent(board,false);UIController.SetRect(g.rectTransform,new Vector2(.02f+figure*.18f,.33f),new Vector2(.18f+figure*.18f,.97f));g.pants=figure==1;g.palette=state?.palette??0;g.raycastTarget=false;
            }
            CatText(board,"HanbokForms","Áo + váy / quần",new Vector2(.02f,.20f),new Vector2(.36f,.30f),15);
            for(var i=0;i<3;i++)
            {
                var y=.82f-i*.27f;var target=ui.Panel(board,"CatTarget"+i,new Vector2(.70f,y-.09f),new Vector2(.98f,y+.09f),new Color(.20f,.42f,.40f,.25f));rects[i]=target;MobileUI.Round(target);target.GetComponent<Image>().raycastTarget=false;CatText(target,"GarmentTarget",c.targets[i],new Vector2(.03f,.04f),new Vector2(.97f,.96f),16);
            }
            var order=new[]{1,2,0};
            for(var row=0;row<3;row++)
            {
                var i=order[row];var y=.82f-row*.27f;var tile=ui.Panel(board,"CatPiece"+i,new Vector2(.39f,y-.09f),new Vector2(.66f,y+.09f),DesignKitArt.Gold);MobileUI.Round(tile);CatText(tile,"GarmentName",c.tokens[i],new Vector2(.03f,.04f),new Vector2(.97f,.96f),16);
                var drag=tile.gameObject.AddComponent<CatDragHandle>();drag.targets=rects;drag.drop=target=>game.Cats.Match(c.id,i,target);drag.feedback=CatFeedback;
                if(state?.marks.Contains(i)==true){drag.placed=true;tile.SetParent(rects[i],false);UIController.SetRect(tile,new Vector2(.04f,.04f),new Vector2(.96f,.96f));}
            }
            var labels=new[]{"Xanh ngọc","Hồng đào","Tím hoa","Vàng nắng"};
            for(var i=0;i<4;i++)
            {
                var palette=i;var x=.02f+i*.245f;
                var swatch=MobileUI.Button(ui,board,"CatPalette"+i,labels[i],new Vector2(x,.02f),new Vector2(x+.225f,.16f),()=>
                {
                    var ok=game.Cats.PickHanbokPalette(c.id,palette);if(ok){ShowCatChallenge(c);catFeedback.text=game.Cats.Message;}else CatFeedback(false);
                },state?.paletteChosen==true&&state.palette==i,true);
                swatch.GetComponentInChildren<Text>().fontSize=13;
            }
        }
        void CatMaze(RectTransform board,CatRescueChallenge c)
        {
            var width=c.maze[0].Length;var height=c.maze.Length;var state=game.Cats.State(c);var current=state?.path.Count>0?state.path.Last():c.start;
            for(var y=0;y<height;y++)for(var x=0;x<width;x++)
            {
                var cell=y*width+x;var wall=c.maze[y][x]=='#';var px=.055f+x*.89f/width;var py=.96f-y*.77f/height;
                var b=MobileUI.Button(ui,board,"CatCell"+cell,cell==current?"Lumi":cell==c.exit?"Mèo":wall?"":"·",new Vector2(px,py-.70f/height),new Vector2(px+.81f/width,py),()=>
                {
                    var ok=game.Cats.Walk(c.id,cell);var message=game.Cats.Message;
                    if(ok){ShowCatChallenge(c);catFeedback.text=message;}else CatFeedback(false);
                },cell==current,true);
                b.interactable=!wall;
                if(wall){b.targetGraphic.color=new Color(.16f,.29f,.30f,.8f);continue;}
                if(state?.path.Contains(cell)==true&&cell!=current)b.GetComponentInChildren<Text>().text="•";
            }
            MobileUI.Button(ui,board,"CatMazeReset","Về đầu tuyến",new Vector2(.21f,.02f),new Vector2(.79f,.14f),()=>{game.Cats.ResetMaze(c.id);ShowCatChallenge(c);},illustrated:true);
        }
        void ShowCatFact(CatRescueChapter chapter,CatRescueChallenge challenge)
        {
            var sheet=CatSheet("Đã hoàn thành · "+challenge.title,"Một điều vừa khám phá",out var w,out var h);
            var image=CatRescueArt.Image(challenge.imageId)??CatRescueArt.Image("city:"+chapter.cityId);
            ui.Picture(sheet,"CatCulturalArt",image,new Vector2(.09f,.46f),new Vector2(.91f,.84f));
            if(challenge.mode=="hangul")
            {
                sheet.Find("CatCulturalArt").gameObject.SetActive(false);
                var han=new GameObject("HanSyllable",typeof(RectTransform),typeof(CatHangulGraphic)).GetComponent<CatHangulGraphic>();han.transform.SetParent(sheet,false);UIController.SetRect(han.rectTransform,new Vector2(.34f,.50f),new Vector2(.66f,.80f));han.letter=3;han.color=DesignKitArt.Teal;han.raycastTarget=false;
            }
            CatText(sheet,"CatCultureFact",challenge.fact,new Vector2(.08f,.23f),new Vector2(.92f,.45f),22);
            MobileUI.Button(ui,sheet,"CatFactContinue","Tiếp tục hành trình",new Vector2(.12f,.08f),new Vector2(.88f,.18f),()=>ShowCatRescue(),true,true);
        }
        void ShowCatRescueFinal()
        {
            var cats=game.Cats;var c=cats.Current;
            if(!cats.CanRescue){ShowCatRescue("Cần đủ thử thách, vật dụng và đúng điểm cứu hộ.");return;}
            var sheet=CatSheet("Đón "+c.catName+" về nhà",cats.Place(c.rescueNode),out var w,out var h);
            CatText(sheet,"CatFinalInstructions",c.rescueText,new Vector2(.07f,.72f),new Vector2(.93f,.86f),21);
            var board=ui.Panel(sheet,"CatRescueBoard",new Vector2(.06f,.28f),new Vector2(.94f,.70f),DesignKitArt.Hex("DCE6D6"));MobileUI.Round(board);
            var target=ui.Panel(board,"CatShelter",new Vector2(.60f,.17f),new Vector2(.96f,.82f),DesignKitArt.Teal);MobileUI.Round(target);
            ui.Picture(target,"ShelterBasket",AdventureUIArt.Load().backpack,new Vector2(.10f,.10f),new Vector2(.90f,.65f));
            CatText(target,"ShelterLabel","Chỗ trú an toàn",new Vector2(.04f,.72f),new Vector2(.96f,.95f),16).color=DesignKitArt.Cream;
            var cat=CatPortrait(board,c,new Vector2(.04f,.20f),new Vector2(.43f,.83f));cat.raycastTarget=true;
            var drag=cat.gameObject.AddComponent<CatDragHandle>();drag.targets=new[]{target};
            drag.drop=_=>cats.Rescue(Vector2.zero);
            var finished=false;
            drag.feedback=ok=>
            {
                catFeedback.text=cats.Message;
                if(!ok)return;
                finished=true;cat.gameObject.GetComponent<CatRescueMotion>().rescued=true;changed?.Invoke();
                var confetti=new GameObject("CatRescueCelebration",typeof(RectTransform),typeof(CatWeatherGraphic)).GetComponent<CatWeatherGraphic>();confetti.transform.SetParent(board,false);UIController.SetRect(confetti.rectTransform,Vector2.zero,Vector2.one);confetti.celebration=true;confetti.raycastTarget=false;
                sheet.Find("CatFinalContinue").GetComponentInChildren<Text>().text="Nhận bưu thiếp · Xem sổ mèo";
            };
            catFeedback=CatText(sheet,"CatFinalFeedback","Kéo "+c.catName+" vào giỏ bên phải.",new Vector2(.08f,.17f),new Vector2(.92f,.27f),20);
            MobileUI.Button(ui,sheet,"CatFinalContinue","Về nhiệm vụ",new Vector2(.10f,.06f),new Vector2(.90f,.15f),()=>{if(finished)ShowCatCollection(Array.IndexOf(cats.Catalog.chapters,c));else ShowCatRescue();},true,true);
        }
    }
}
