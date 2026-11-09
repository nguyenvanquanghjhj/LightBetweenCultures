using System;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        void ShowCulturalTrial(RelicActivity a)
        {
            var r=game.Relics.Definition(a.id);var sheet=RelicSheet(r.trial,r.instructions);var board=Board(sheet,"CulturalTrialBoard");
            var feedback=RelicFeedback(sheet,game.Relics.Restored(r.id)?r.completionText:"Thử sai không mất mảnh. Có thể rời đi và tiếp tục từ di vật đã ghép.");feedback.color=DesignKitArt.Teal;
            if(a.TrialType=="hanji")HanjiPattern(board,a,r);
            else if(a.TrialType=="prism")PrismStreams(board,a);
            else SurveyTrial(board,a);
            MobileUI.Button(ui,sheet,"TrialHint","Gợi ý",new Vector2(.05f,.19f),new Vector2(.28f,.26f),()=>Feedback(feedback,r.hint),illustrated:true);
            MobileUI.Button(ui,sheet,"TrialRetry","Thử lại",new Vector2(.30f,.19f),new Vector2(.53f,.26f),()=>{a.ResetTrial();ShowCulturalTrial(a);},illustrated:true);
            MobileUI.Button(ui,sheet,"TrialSubmit","Xác nhận",new Vector2(.55f,.19f),new Vector2(.95f,.26f),()=>
            {
                if(!a.TrialSucceeded){Feedback(feedback,"Chưa khớp. Quan sát manh mối và thử lại; không mất mảnh hay vật phẩm.",true);return;}
                if(game.Relics.Restored(r.id)){Feedback(feedback,"Di vật đã phục dựng; xem lại không nhận thưởng lần hai.");return;}
                if(a.Restore()){changed?.Invoke();Feedback(feedback,r.completionText);}
                else Feedback(feedback,"Chưa đủ điều kiện hoặc chưa lưu được. Thử lại an toàn.");
            },true,true);
        }
        void HanjiPattern(RectTransform board,RelicActivity a,CulturalRelic r)
        {
            for(var i=0;i<4;i++)
            {
                var j=i;var p=new Vector2(i%2==0?.29f:.71f,i<2?.73f:.30f);
                var tile=RelicPicture(board,"HanjiTile"+i,AdventureUIArt.Load().Fragment(r.fragmentIds[i]),p,p);tile.rectTransform.sizeDelta=new Vector2(100,105);tile.rectTransform.localRotation=Quaternion.Euler(0,0,-90*a.tileTurns[i]);tile.raycastTarget=false;
                var guide=ui.Label(board,"PatternGuide"+i,"Mẫu "+(i+1)+" · "+(r.trialTurns[i]*90)+"°",p+new Vector2(-.20f,.15f),p+new Vector2(.20f,.23f),14);guide.color=DesignKitArt.Gold;
                var turn=MobileUI.Button(ui,board,"TrialTurn"+i,"↻ "+(a.tileTurns[i]*90)+"°",p+new Vector2(-.16f,-.17f),p+new Vector2(.16f,-.07f),()=>{a.TurnTile(j);ShowCulturalTrial(a);},illustrated:true);
            }
            ui.Label(board,"HanjiStitch","··· HOA VĂN / SỢI CHỈ / LỜI CHÀO ···",new Vector2(.03f,.46f),new Vector2(.97f,.55f),14).color=DesignKitArt.Gold;
        }
        void SurveyTrial(RectTransform board,RelicActivity a)
        {
            var mist=a.TrialType=="mist";var lens=a.TrialType=="lens";
            RelicPicture(board,"SurveyRelic",AdventureUIArt.Load().Relic(a.id),new Vector2(.33f,.26f),new Vector2(.67f,.78f)).color=new Color(1,1,1,.70f);
            var labels=new Text[4];var fog=new Image[24];
            var names=mist?new[]{"Hồ · phản chiếu","Thông · hướng gió","Chuông · Đông Bắc","Hạt · cùng nhịp"}:lens?new[]{"Sao đầu","Sao bên phải","Trăng thấp","Nhiễu"}:new[]{"Nét ngọc","Dấu rồng","Dây buộc","Văn khắc"};
            for(var i=0;i<4;i++)
            {
                var p=RelicActivity.SurveyPoints[i];labels[i]=ui.Label(board,"SurveyClue"+i,names[i],p-new Vector2(.18f,.065f),p+new Vector2(.18f,.065f),15);labels[i].color=new Color(1,1,1,.12f);labels[i].raycastTarget=false;
            }
            if(mist)for(var y=0;y<4;y++)for(var x=0;x<6;x++)
            {var i=y*6+x;fog[i]=ui.Picture(board,"FogCell"+i,PresentationShapes.WarmGlow,new Vector2(x/6f-.03f,y/4f-.04f),new Vector2((x+1)/6f+.03f,(y+1)/4f+.04f));fog[i].color=new Color(.69f,.82f,.87f,.85f);fog[i].raycastTarget=false;}
            var surface=ui.Panel(board,"TrialSurveySurface",Vector2.zero,Vector2.one,new Color(0,0,0,.001f));var cursor=RelicPicture(surface,"SurveyLight",PresentationShapes.WarmGlow,new Vector2(.5f,.5f),new Vector2(.5f,.5f));cursor.rectTransform.sizeDelta=new Vector2(95,95);cursor.color=new Color(1,.89f,.52f,.85f);cursor.raycastTarget=false;
            Action update=()=>{for(var i=0;i<4;i++)labels[i].color=a.surveyed[i]?DesignKitArt.Gold:new Color(1,1,1,.12f);if(mist)for(var i=0;i<24;i++)fog[i].color=new Color(.69f,.82f,.87f,a.fogCleared[i]?0:.85f);};update();
            surface.gameObject.AddComponent<RelicSurveySurface>().sample=p=>{a.Sweep(p);cursor.rectTransform.anchorMin=cursor.rectTransform.anchorMax=p;update();};
            if(lens)for(var i=0;i<4;i++)
            {var j=i;var p=RelicActivity.SurveyPoints[i]+Vector2.down*.11f;MobileUI.Button(ui,board,"StarRead"+i,i==3?"?":i==2?"☾":"★",p,p,()=>{var ok=a.ReadStar(j);ShowCulturalTrial(a);},illustrated:true).GetComponent<RectTransform>().sizeDelta=new Vector2(55,42);}
            var arrow=mist?new[]{"N","NE","E","SE","S","SW","W","NW"}[a.Dial]:new[]{"N","E","S","W"}[a.Dial];
            MobileUI.Button(ui,board,"TrialDial","↻ "+arrow,new Vector2(.35f,.005f),new Vector2(.65f,.12f),()=>{a.TurnDial();ShowCulturalTrial(a);},illustrated:true);
            if(lens)ui.Label(board,"StarReadProgress",a.StarSequence+" / 3 nét đã đọc",new Vector2(.33f,.83f),new Vector2(.67f,.97f),14).color=DesignKitArt.Gold;
        }
        void PrismStreams(RectTransform board,RelicActivity a)
        {
            var symbols=new[]{"relic_dalat_mist_compass","relic_hue_dragon_seal","relic_jeonju_hanji_stamp"};var labels=new[]{"Thiên nhiên","Di sản","Hanji"};
            RelicPicture(board,"PrismFocus",AdventureUIArt.Load().Relic(a.id),new Vector2(.65f,.24f),new Vector2(.97f,.80f));
            for(var i=0;i<3;i++)
            {
                var j=i;var y=.82f-i*.30f;var end=new Vector2(.53f,y);var start=new Vector2(.13f,y);
                var target=RelicPicture(board,"RelicTarget"+i,AdventureUIArt.Load().slot,end,end);target.rectTransform.sizeDelta=new Vector2(72,72);
                var stream=RelicPicture(board,"RelicPiece"+i,AdventureUIArt.Load().Relic(symbols[i]),a.joined[i]?end:start,a.joined[i]?end:start);stream.rectTransform.sizeDelta=new Vector2(62,67);stream.raycastTarget=true;
                var drag=stream.gameObject.AddComponent<RelicDragHandle>();drag.target=target.rectTransform;drag.submit=e=>a.JoinStream(j,e);drag.feedback=ok=>{if(ok)ShowCulturalTrial(a);};
                ui.Label(board,"StreamName"+i,labels[i],new Vector2(.01f,y-.14f),new Vector2(.27f,y-.07f),13);
                MobileUI.Button(ui,board,"TrialTurn"+i,"↻ "+(a.tileTurns[i]*90)+"°",new Vector2(.28f,y-.07f),new Vector2(.45f,y+.07f),()=>{a.TurnTile(j);ShowCulturalTrial(a);},illustrated:true);
                DrawPrismStream(board,a,i);
            }
        }
        void DrawPrismStream(RectTransform board,RelicActivity a,int stream)
        {
            var trace=a.PrismTrace(stream);var layer=ui.Panel(board,"PrismFlow"+stream,Vector2.zero,Vector2.one,Color.clear);layer.GetComponent<Image>().raycastTarget=false;layer.SetSiblingIndex(1);Canvas.ForceUpdateCanvases();
            var color=trace.success?new[]{new Color(.45f,1,.83f,.9f),new Color(1,.85f,.4f,.9f),new Color(.75f,.66f,1,.9f)}[stream]:new Color(1,.48f,.32f,.65f);
            for(var i=1;i<trace.points.Count;i++)
            {
                var start=Vector2.Scale(trace.points[i-1],layer.rect.size);var end=Vector2.Scale(trace.points[i],layer.rect.size);
                var ray=ui.Panel(layer,"FlowSegment"+i,Vector2.zero,Vector2.zero,color);ray.pivot=new Vector2(0,.5f);ray.anchoredPosition=start;ray.sizeDelta=new Vector2(Vector2.Distance(start,end),3.5f);ray.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(end.y-start.y,end.x-start.x)*Mathf.Rad2Deg);ray.GetComponent<Image>().raycastTarget=false;
            }
        }
    }
}
