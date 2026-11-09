using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        Image RelicPicture(Transform parent,string name,Sprite sprite,Vector2 min,Vector2 max)
        {var image=ui.Picture(parent,name,sprite,min,max);image.useSpriteMesh=true;return image;}
        RectTransform RelicSheet(string title,string subtitle)
        {
            Clear();overlay=ui.Panel(ui.Root,"GameplayOverlay",Vector2.zero,Vector2.one,new Color(.02f,.06f,.10f,.64f));
            var sheet=MobileUI.Frame(ui,overlay,"RelicPanel",new Vector2(.04f,.075f),new Vector2(.96f,.92f),AdventureUIArt.Load().window,2.8f);
            ui.Label(sheet,"RelicTitle",title,new Vector2(.06f,.89f),new Vector2(.94f,.98f),25).color=DesignKitArt.Ink;
            ui.Label(sheet,"RelicInstructions",subtitle,new Vector2(.07f,.77f),new Vector2(.93f,.89f),17).color=DesignKitArt.Teal;
            MobileUI.Button(ui,sheet,"RelicExit","Để sau · Trở về bản đồ",new Vector2(.07f,.025f),new Vector2(.93f,.095f),()=>{game.Relics.Cancel();Close();},illustrated:true);
            ContextHelp(sheet,"relic","Tìm đủ bốn mảnh bằng những tương tác tại địa điểm được ghi trong sổ. Chạm hoặc kéo mảnh vào hình mờ; dùng Xoay nếu cần.\n\nGhép hình xong còn thử thách phục dựng riêng của mỗi thành phố. Các di vật không dùng để mở thành phố hoặc vào boss.\n\nĐể khép lại hành trình: giải quyết boss, phục dựng sáu di vật và thực hiện Lễ Kết Nối tại Seoul.","RelicTitle");
            return sheet;
        }
        Text RelicFeedback(RectTransform parent,string value)=>ui.Label(parent,"RelicFeedback",value,new Vector2(.06f,.11f),new Vector2(.94f,.185f),18);
        void Feedback(Text text,string value,bool invalid=false){text.text=value;text.color=DesignKitArt.Teal;if(invalid)SoftAudio.Cue(SoftCue.UI_Invalid);}
        RectTransform Board(RectTransform sheet,string name="RelicBoard")
        {var b=ui.Panel(sheet,name,new Vector2(.06f,.27f),new Vector2(.94f,.75f),new Color(.08f,.22f,.25f,.95f));MobileUI.Round(b);return b;}
        void DragPiece(RectTransform board,RelicActivity a,int index,Sprite sprite,Vector2 start,Vector2 end,string caption)
        {
            // Keep the existing touch size and normalized game tolerance. On narrow
            // portrait boards the last target previously protruded past the board.
            Canvas.ForceUpdateCanvases();
            end.x=Mathf.Clamp(end.x,52/board.rect.width,1-52/board.rect.width);
            var slot=RelicPicture(board,"RelicTarget"+index,AdventureUIArt.Load().slot,end,end);slot.rectTransform.sizeDelta=new Vector2(96,108);slot.color=new Color(1,.89f,.5f,.65f);
            var ghost=RelicPicture(slot.transform,"MatchingOutline",sprite,new Vector2(.1f,.1f),new Vector2(.9f,.9f));ghost.color=new Color(1,1,1,.22f);
            var image=RelicPicture(board,"RelicPiece"+index,sprite,start,start);image.rectTransform.sizeDelta=new Vector2(88,100);image.raycastTarget=true;
            var drag=image.gameObject.AddComponent<RelicDragHandle>();drag.target=slot.rectTransform;drag.submit=error=>a.Drop(index,error);
            var note=ui.Label(board,"PieceNote"+index,caption,new Vector2(start.x-.20f,start.y-.14f),new Vector2(start.x+.20f,start.y-.075f),13);
            drag.feedback=ok=>{note.text=ok?"Đã khớp":"Kéo vào hình tương ứng";};
        }
        void ShowRelicSource(string fragmentId)
        {
            var a=game.Relics.BeginSource(fragmentId);if(a==null){ShowEvent(game.Save.node,"Manh mối này chưa sẵn sàng hoặc đã được ghi lại.");return;}
            var f=game.Relics.Fragment(fragmentId);var sheet=RelicSheet(f.name,"Kéo mảnh vào dấu vàng. Xoay cho khớp hướng hình mờ rồi ghi lại manh mối.");var board=Board(sheet);
            var sprite=AdventureUIArt.Load().Fragment(f.id)??AdventureUIArt.Load().slot;DragPiece(board,a,0,sprite,new Vector2(.22f,.55f),new Vector2(.72f,.55f),f.name);
            if(f.artStatus=="ART_PENDING")ui.Label(board,"PendingFragmentArt","ART_PENDING · ký hiệu tạm trung tính",new Vector2(.08f,.82f),new Vector2(.92f,.96f),14);
            var feedback=RelicFeedback(sheet,f.instruction);feedback.color=DesignKitArt.Teal;
            var target=board.Find("RelicTarget0/MatchingOutline");target.localRotation=Quaternion.Euler(0,0,-90*f.sourceTurns);
            var piece=board.Find("RelicPiece0");piece.localRotation=Quaternion.Euler(0,0,-90*a.turns);
            MobileUI.Button(ui,board,"RelicRotate","↻ Xoay",new Vector2(.32f,.04f),new Vector2(.68f,.17f),()=>{a.Rotate();piece.localRotation=Quaternion.Euler(0,0,-90*a.turns);},illustrated:true);
            MobileUI.Button(ui,sheet,"RelicClaim","Ghi lại manh mối",new Vector2(.13f,.19f),new Vector2(.87f,.26f),()=>
            {
                if(!a.Claim()){Feedback(feedback,"Chưa khớp vị trí hoặc hướng. Hãy xem hình mờ và thử lại.",true);return;}
                changed?.Invoke();var found=RelicSheet("Đã tìm thấy · "+f.name,game.Relics.Story("found:"+f.id,"Lumi: Một phần câu chuyện đã trở lại. Mình sẽ giữ nó trong sổ di vật."));
                RelicPicture(found,"FoundFragment",sprite,new Vector2(.19f,.32f),new Vector2(.81f,.72f));
                ui.Picture(found,"LumiFragmentReaction",art.lumiPortrait,new Vector2(.07f,.11f),new Vector2(.25f,.29f)).gameObject.AddComponent<PortraitPresence>();
                var relic=game.Relics.Catalog.relics.First(r=>r.fragmentIds.Contains(f.id));
                ui.Label(found,"CollectionProgress",game.Relics.Count(relic)+" / 4 mảnh · "+relic.name,new Vector2(.10f,.24f),new Vector2(.90f,.32f),20).color=DesignKitArt.Teal;
                found.Find("RelicInstructions").GetComponent<Text>().text=RelicGuide.Next(game,relic);
                if(game.Relics.Count(relic)==4)RelicDestination(found,relic,new Vector2(.27f,.12f),new Vector2(.93f,.21f));
            },true,true);
        }
        void ShowRelicAssembly(string id)
        {
            var a=game.Relics.BeginAssembly(id);if(a==null){ShowEvent(game.Save.node,"Cần đủ bốn mảnh để phục dựng di vật.");return;}
            if(game.Relics.Assembled(id)){ShowRelicTrial(a);return;}
            var r=game.Relics.Definition(id);var sheet=RelicSheet("Ghép "+r.name,"Kéo bốn mảnh vào hình tương ứng. Có thể rời đi mà không mất mảnh.");var board=Board(sheet);
            for(var i=0;i<4;i++)
            {var f=game.Relics.Fragment(r.fragmentIds[i]);DragPiece(board,a,i,AdventureUIArt.Load().Fragment(f.id)??AdventureUIArt.Load().slot,new Vector2(.18f,i<2?.76f:.27f)+new Vector2((i%2)*.31f,0),new Vector2(.68f,i<2?.76f:.27f)+new Vector2((i%2)*.23f,0),f.artStatus=="ART_PENDING"?"ART_PENDING":"");}
            var feedback=RelicFeedback(sheet,"Ghép hình để thấy chiếc đèn hoàn chỉnh.");feedback.color=DesignKitArt.Teal;
            MobileUI.Button(ui,sheet,"RelicAssemble","Hoàn tất ghép",new Vector2(.13f,.19f),new Vector2(.87f,.26f),()=>{if(!a.Assemble()){Feedback(feedback,"Còn mảnh chưa đặt đúng ô.",true);return;}changed?.Invoke();ShowRelicTrial(a);},true,true);
        }
        void ShowRelicTrial(RelicActivity a){if(string.IsNullOrEmpty(a.TrialType))ShowLanternBeam(a);else ShowCulturalTrial(a);}
        void ShowLanternBeam(RelicActivity a)
        {
            var r=game.Relics.Definition(a.id);var ready=game.Relics.CanRitual(r);
            var sheet=RelicSheet("Dẫn Ánh Đèn",game.Relics.Restored(r.id)?"Đèn Mắt Phố đã phục dựng. Có thể thử lại góc phản chiếu; không có thưởng lặp.":ready?"Chạm ba gương để xoay tia từ bến qua mái hiên đến phố cổ. Ô mái nhà chặn ánh sáng.":"Đèn đã ghép. Hãy đồng bộ trạm H7 theo nhiệm vụ Cô Mai trước khi hoàn tất nghi thức.");
            var board=Board(sheet,"LanternBeamBoard");var feedback=RelicFeedback(sheet,ready?"Thử tia sáng hoặc dùng gợi ý, không mất mảnh khi sai.":"H7 mở tuyến Huế/Jeonju như cũ; di vật không chặn hành trình.");feedback.color=DesignKitArt.Teal;
            var points=new[]{new Vector2(.5f,.3f),new Vector2(.5f,.7f),new Vector2(.1f,.7f)};
            for(var x=0;x<5;x++)for(var y=0;y<5;y++){var dot=ui.Panel(board,"Grid"+x+"_"+y,new Vector2(.1f+x*.2f,.1f+y*.2f),new Vector2(.1f+x*.2f,.1f+y*.2f),new Color(1,1,1,.18f));dot.sizeDelta=new Vector2(5,5);dot.GetComponent<Image>().raycastTarget=false;}
            foreach(var block in new[]{new Vector2(.7f,.3f),new Vector2(.3f,.5f)})
            {var wall=ui.Panel(board,"RoofObstacle",block,block,DesignKitArt.Ink);wall.sizeDelta=new Vector2(48,42);wall.GetComponent<Image>().raycastTarget=false;ui.Label(wall,"RoofLabel","Mái",Vector2.zero,Vector2.one,13);}
            RelicPicture(board,"LanternOrigin",AdventureUIArt.Load().Relic(r.id),new Vector2(.01f,.21f),new Vector2(.17f,.39f));
            var goal=ui.Label(board,"LightGoal","PHỐ",new Vector2(.01f,.88f),new Vector2(.19f,1),15);goal.color=DesignKitArt.Gold;
            for(var i=0;i<3;i++){var j=i;var p=points[i];var b=MobileUI.Button(ui,board,"RelicMirror"+i,a.mirrors[i]?"/":"\\",p,p,()=>{a.Mirror(j);ShowLanternBeam(a);},true,true);((RectTransform)b.transform).sizeDelta=new Vector2(58,58);}
            var hint=MobileUI.Button(ui,sheet,"RelicHint","Gợi ý",new Vector2(.07f,.19f),new Vector2(.31f,.26f),()=>Feedback(feedback,"Từ bến: hướng lên → sang trái → lên phố. Gương / và \\ phản chiếu theo hai hướng khác nhau."),illustrated:true);
            MobileUI.Button(ui,sheet,"RelicTrace","Thử tia sáng",new Vector2(.34f,.19f),new Vector2(.93f,.26f),()=>
            {
                var trace=a.Trace();DrawBeam(board,trace.points);Feedback(feedback,trace.message);
                if(!trace.success){SoftAudio.Cue(SoftCue.UI_Invalid);return;}
                if(!ready){Feedback(feedback,"Hướng đã đúng; nghi thức vẫn chờ trạm H7. Chưa Khai Sáng toàn Hội An.");return;}
                if(a.Restore()){changed?.Invoke();Feedback(feedback,game.Relics.Story("ritual:complete","HỘI AN KHAI SÁNG · Đèn Mắt Phố đã dẫn ánh sáng về những mái hiên."));}
                else Feedback(feedback,"Chưa lưu được nghi thức. Có thể thử lại an toàn.");
            },true,true);
        }
        void DrawBeam(RectTransform board,System.Collections.Generic.List<Vector2> points)
        {
            var old=board.Find("BeamSegments");if(old!=null){old.gameObject.SetActive(false);UnityEngine.Object.Destroy(old.gameObject);}
            var layer=ui.Panel(board,"BeamSegments",Vector2.zero,Vector2.one,Color.clear);layer.GetComponent<Image>().raycastTarget=false;layer.SetSiblingIndex(1);Canvas.ForceUpdateCanvases();
            for(var i=1;i<points.Count;i++)
            {var a=Vector2.Scale(new Vector2(.1f,.1f)+points[i-1]*.2f,layer.rect.size);var b=Vector2.Scale(new Vector2(.1f,.1f)+points[i]*.2f,layer.rect.size);var line=ui.Panel(layer,"Beam"+i,Vector2.zero,Vector2.zero,DesignKitArt.Gold);line.pivot=new Vector2(0,.5f);line.anchoredPosition=a;line.sizeDelta=new Vector2(Vector2.Distance(a,b),4);line.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg);line.GetComponent<Image>().raycastTarget=false;}
        }
        void ShowRelicPhoto()
        {
            var a=game.Relics.BeginPhoto();if(a==null)return;
            var sheet=RelicSheet("Bưu thiếp · Cầu cổ ánh đèn","Kéo dấu đèn và bóng phản chiếu vào hai vùng vàng. Ảnh minh họa từ tranh Hội An, không phải ảnh camera.");var board=Board(sheet);
            var image=new GameObject("PhotoMapView",typeof(RectTransform),typeof(RawImage)).GetComponent<RawImage>();image.rectTransform.SetParent(board,false);UIController.SetRect(image.rectTransform,Vector2.zero,Vector2.one);image.texture=art.hoiAnMap.texture;image.raycastTarget=false;
            var node=game.Data.nodes[game.Save.node].position;SetPostcardUV(image,node);
            DragPiece(board,a,0,AdventureUIArt.Load().Relic("relic_hoi_an_lantern"),new Vector2(.18f,.79f),new Vector2(.62f,.67f),"Đèn");
            DragPiece(board,a,1,AdventureUIArt.Load().Fragment("fragment_hoi_an_eye_reflection"),new Vector2(.18f,.25f),new Vector2(.62f,.28f),"Bóng nước");
            var feedback=RelicFeedback(sheet,game.Relics.Story("photo:compose","Đèn trên mái hiên và ánh sáng trên nước cùng kể một câu chuyện."));feedback.color=DesignKitArt.Teal;
            MobileUI.Button(ui,sheet,"RelicRecordPhoto","Lưu bưu thiếp",new Vector2(.13f,.19f),new Vector2(.87f,.26f),()=>
            {if(!a.RecordPhoto()){Feedback(feedback,"Căn đủ hai dấu trước khi lưu.");return;}changed?.Invoke();Feedback(feedback,"Đã lưu trong nhật ký. Có thể xem lại; thưởng ghi dấu chỉ một lần.");},true,true);
        }
        public void ShowRelicCollection()
        {
            game.Relics.Cancel();var sheet=RelicSheet("Sổ di vật & bưu thiếp",game.Events.FinalProgressText);
            UIController.SetRect(sheet.Find("RelicInstructions").GetComponent<RectTransform>(),new Vector2(.07f,.80f),new Vector2(.93f,.89f));
            var i=0;
            foreach(var r in game.Relics.Catalog.relics)
            {
                var col=i%2;var row=i/2;var min=new Vector2(.07f+col*.46f,.60f-row*.17f);var max=min+new Vector2(.40f,.16f);
                var card=MobileUI.Frame(ui,sheet,"RelicCard_"+r.id,min,max,AdventureUIArt.Load().slot,3);
                var relicId=r.id;var inspect=card.gameObject.AddComponent<TouchButton>();inspect.targetGraphic=card.GetComponent<Image>();inspect.onClick.AddListener(()=>SoftAudio.Action(()=>ShowRelicDetails(relicId)));
                RelicPicture(card,"RelicArt",AdventureUIArt.Load().Relic(r.id),new Vector2(.04f,.20f),new Vector2(.38f,.94f));
                ui.Label(card,"RelicName",r.name,new Vector2(.40f,.38f),new Vector2(.97f,.95f),15);
                ui.Label(card,"RelicCount",game.Relics.Count(r)+"/4 · "+(game.Relics.Restored(r.id)?"Đã phục dựng":game.Relics.Assembled(r.id)?"Chạm · Xem thử thách":game.Relics.Count(r)==4?"Chạm · Ghép & phục dựng":r.enabled?"Chạm · Xem nơi tìm mảnh":"Chưa ghi dấu"),new Vector2(.04f,.02f),new Vector2(.96f,.24f),13);
                i++;
            }
            var restored=game.Relics.Catalog.relics.Count(r=>game.Relics.Restored(r.id));
            ui.Label(sheet,"RestoredRelicCount",restored+" / 6 di vật đã phục dựng",new Vector2(.08f,.77f),new Vector2(.92f,.80f),16).color=DesignKitArt.Teal;
            ui.Label(sheet,"PostcardStatus","Bưu thiếp đã ghi dấu: "+game.Content.checkIns.Count(c=>game.World.Has(c.completionFlag))+" / "+game.Content.checkIns.Length,new Vector2(.08f,.10f),new Vector2(.92f,.18f),17).color=DesignKitArt.Teal;
            if(game.Relics.PhotoRecorded)MobileUI.Button(ui,sheet,"PhotoAlbumH6","Bưu thiếp H6",new Vector2(.07f,.19f),new Vector2(.49f,.26f),()=>ShowStoredPostcard(),illustrated:true);
            var journal=MobileUI.Button(ui,sheet,"OpenPhotoJournal","Sổ ảnh văn hóa",new Vector2(.52f,.19f),new Vector2(.93f,.26f),()=>ShowPhotoJournal(),true,true);MobileUI.Cue(journal,AdventureSymbol.Camera);
        }
        void ShowRelicDetails(string id)
        {
            game.Relics.Cancel();var r=game.Relics.Definition(id);var restored=game.Relics.Restored(id);var sheet=RelicSheet(r.name,RelicGuide.Next(game,r));
            for(var i=0;i<4;i++)
            {
                var f=game.Relics.Fragment(r.fragmentIds[i]);var owned=game.Relics.Owned(f.id);var x=i%2==0?.08f:.53f;var y=i<2?.51f:.28f;
                var card=MobileUI.Frame(ui,sheet,"FragmentCard_"+f.id,new Vector2(x,y),new Vector2(x+.39f,y+.21f),AdventureUIArt.Load().slot,3);
                var sprite=owned?AdventureUIArt.Load().Fragment(f.id):null;
                var picture=RelicPicture(card,"FragmentArt",sprite??AdventureUIArt.Load().slot,new Vector2(.04f,.22f),new Vector2(.43f,.96f));picture.color=owned?Color.white:new Color(1,1,1,.35f);
                var index=game.Data.cityId==f.cityId?Array.FindIndex(game.Data.nodes,n=>n.id==f.nodeId):-1;var known=owned||index>=0&&game.Discovery.IsVisible(index);
                ui.Label(card,"FragmentName",known?f.name:"Manh mối chưa ghi",new Vector2(.44f,.22f),new Vector2(.96f,.95f),15);
                var source=RelicGuide.KnownSource(game,f);
                ui.Label(card,"FragmentState",owned?(f.artStatus=="ART_PENDING"?"Đã có · ART_PENDING":"Đã ghi nhận") : source>=0?game.Data.nodes[source].name:"Tiếp tục khám phá",new Vector2(.03f,.01f),new Vector2(.97f,.22f),13);
                if(!owned&&source>=0){var jump=card.gameObject.AddComponent<TouchButton>();jump.targetGraphic=card.GetComponent<Image>();var dest=source;jump.onClick.AddListener(()=>SoftAudio.Action(()=>{Close();UnityEngine.Object.FindAnyObjectByType<CityMap>()?.FocusLocation(dest);}));}
            }
            if(!restored&&game.Relics.Count(r)==4)RelicDestination(sheet,r,new Vector2(.08f,.18f),new Vector2(.92f,.265f));
            MobileUI.Button(ui,sheet,"RelicCollectionBack","← Sổ sáu di vật",new Vector2(.12f,.105f),new Vector2(.88f,.17f),ShowRelicCollection,true,true);
        }
        void RelicDestination(RectTransform sheet,CulturalRelic r,Vector2 min,Vector2 max)
        {
            var here=game.Data.cityId==r.cityId;var target=here?Array.FindIndex(game.Data.nodes,n=>n.id==r.restorationNode):-1;
            var known=target>=0&&game.Discovery.DiscoveryState(target)==MapDiscoveryState.REVEALED;
            var at=known&&target==game.Save.node;
            MobileUI.Button(ui,sheet,"RelicDestination",at?"Bắt đầu · Ghép & phục dựng":here?"Chỉ nơi phục dựng trên bản đồ":"Trở lại "+game.Content.City(r.cityId).name+" bằng Thế giới",min,max,()=>
            {
                if(at){ShowRelicAssembly(r.id);return;}
                Close();if(known)UnityEngine.Object.FindAnyObjectByType<CityMap>()?.FocusLocation(target);
                else if(!here)game.Scenes.Go("WorldMap");
            },true,true);
        }
        void SetPostcardUV(RawImage picture,Vector2 position)
        {
            Canvas.ForceUpdateCanvases();var size=picture.rectTransform.rect.size;var width=.40f;
            var height=Mathf.Min(1,width*picture.texture.width/picture.texture.height/(size.x/size.y));
            picture.uvRect=new Rect(Mathf.Clamp(position.x-width*.5f,0,1-width),Mathf.Clamp(position.y-height*.5f,0,1-height),width,height);
        }
        void ShowStoredPostcard(Action back=null)
        {
            // Latest user approval: new artwork is gallery-only, never the alignment board.
            ShowScenery(game.Content.CheckIn("hoi_an","H6"),-1,false,-1);
        }
    }
}
