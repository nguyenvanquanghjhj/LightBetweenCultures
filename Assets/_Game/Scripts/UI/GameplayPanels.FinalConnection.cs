using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    [Serializable] public sealed class FinalLightStep { public string relicId,title,text,color; }
    [Serializable] public sealed class FinalConnectionPresentation { public FinalLightStep[] steps; public string convergence; }
    public sealed partial class GameplayPanels
    {
        void ExecuteFinalView(int node,string choice)
        {
            var result=game.Events.Execute(node,choice);changed?.Invoke();
            if(!result.stop){ShowEvent(node,result.message);return;}
            if(choice=="ceremony"){ShowFinalConfirmation(node);return;}
            if(choice=="ceremony_confirm"){ShowFinalCeremony(node);return;}
            if(choice=="ceremony_cancel"){Close();return;}
            if(choice=="final_collection"){game.Events.CancelFinalCeremony();ShowRelicCollection();return;}
            if(result.openEnding){ShowEnding(node);return;}
            ShowEvent(node,result.message);
        }
        void ShowFinalConfirmation(int node)
        {
            if(!game.Events.CanStartFinalCeremony){ShowEvent(node);return;}
            var sheet=RelicSheet("Cùng thắp sáu ánh sáng?",game.Events.FinalProgressText);sheet.name="FinalConfirmation";
            RelicPicture(sheet,"FinalLumiPortrait",art.lumiPortrait,new Vector2(.28f,.49f),new Vector2(.72f,.76f));
            ui.Label(sheet,"FinalInvitation","Lumi: Mình muốn gửi lời cảm ơn tới những người đã cùng giữ ánh sáng. Chúng ta bắt đầu nhé?",new Vector2(.09f,.29f),new Vector2(.91f,.48f),21).color=DesignKitArt.Ink;
            MobileUI.Button(ui,sheet,"FinalConfirm","Bắt đầu Lễ Kết Nối",new Vector2(.12f,.19f),new Vector2(.88f,.27f),()=>ExecuteFinalView(node,"ceremony_confirm"),true,true);
        }
        void ShowFinalCeremony(int node)
        {
            if(!game.Events.CeremonyInProgress){ShowEvent(node);return;}
            var script=JsonUtility.FromJson<FinalConnectionPresentation>(Resources.Load<TextAsset>("Content/FinalConnectionPresentation").text);
            var sheet=RelicSheet("Lễ Kết Nối\nSáu Miền Ánh Sáng","Những câu chuyện đã đi cùng Lumi, giờ cùng tìm về một nhịp sáng.");sheet.name="FinalCeremonyPanel";
            var board=ui.Panel(sheet,"FinalLights",new Vector2(.04f,.34f),new Vector2(.96f,.77f),Color.clear);board.GetComponent<Image>().raycastTarget=false;
            var locations=new[]{new Vector2(.18f,.70f),new Vector2(.50f,.88f),new Vector2(.82f,.70f),new Vector2(.82f,.22f),new Vector2(.50f,.07f),new Vector2(.18f,.22f)};
            var icons=new Image[6];var beams=new Image[6];var colors=new Color[6];
            Canvas.ForceUpdateCanvases();
            for(var i=0;i<6;i++)
            {
                ColorUtility.TryParseHtmlString(script.steps[i].color,out colors[i]);
                var p=locations[i];var center=Vector2.Scale(new Vector2(.5f,.47f),board.rect.size);var start=Vector2.Scale(p,board.rect.size);
                var beam=ui.Panel(board,"FinalBeam_"+i,Vector2.zero,Vector2.zero,colors[i]);beam.pivot=new Vector2(0,.5f);beam.anchoredPosition=start;beam.sizeDelta=new Vector2(Vector2.Distance(start,center),3);beam.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(center.y-start.y,center.x-start.x)*Mathf.Rad2Deg);beams[i]=beam.GetComponent<Image>();beams[i].raycastTarget=false;
                icons[i]=RelicPicture(board,"FinalRelic_"+i,AdventureUIArt.Load().Relic(script.steps[i].relicId),p-new Vector2(.12f,.11f),p+new Vector2(.12f,.11f));icons[i].raycastTarget=false;icons[i].canvasRenderer.SetAlpha(.12f);
                beams[i].canvasRenderer.SetAlpha(0);
            }
            var lumi=RelicPicture(board,"FinalLumi",art.lumiPortrait,new Vector2(.35f,.31f),new Vector2(.65f,.64f));lumi.raycastTarget=false;
            var caption=ui.Label(sheet,"FinalCeremonyCaption","",new Vector2(.08f,.20f),new Vector2(.92f,.33f),19);caption.color=DesignKitArt.Ink;
            MobileUI.Button(ui,sheet,"FinalSkip","Bỏ qua chuyển động · Xem lời kết",new Vector2(.08f,.11f),new Vector2(.92f,.185f),()=>ExecuteFinalView(node,"ceremony_skip"),true,true);
            ui.StartCoroutine(AnimateFinalConnection(node,board,icons,beams,caption,script));
        }
        IEnumerator AnimateFinalConnection(int node,RectTransform board,Image[] icons,Image[] beams,Text caption,FinalConnectionPresentation script)
        {
            var lastActiveStep=-1;var lastCaptionStep=-1;var convergenceShown=false;
            while(board!=null&&game.Events.CeremonyInProgress)
            {
                game.Events.TickFinalCeremony();
                var elapsed=game.Events.CeremonyElapsed;var step=Mathf.Min(5,(int)(elapsed/3));
                if(step>lastActiveStep)
                {
                    for(var i=lastActiveStep+1;i<step;i++){icons[i].canvasRenderer.SetAlpha(1);beams[i].canvasRenderer.SetAlpha(.65f);}
                    lastActiveStep=step;
                }
                var light=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-step*3)/1.8f));
                icons[step].canvasRenderer.SetAlpha(.12f+.88f*light);
                beams[step].canvasRenderer.SetAlpha(.65f*light);
                if(elapsed>=18)
                {
                    if(!convergenceShown){caption.text=script.convergence;convergenceShown=true;}
                }
                else if(step!=lastCaptionStep)
                {
                    caption.text=script.steps[step].title+"\n"+script.steps[step].text;lastCaptionStep=step;
                }
                if(elapsed>=NodeEventManager.CeremonyDuration){ExecuteFinalView(node,"ceremony_finish");yield break;}
                yield return null;
            }
        }
        void ShowEnding(int node,int page=0)
        {
            if(!game.Events.CanViewCanonicalEnding){ShowEvent(node);return;}
            var definition=game.Events.Definition(game.Data.nodes[node].eventId);page=Mathf.Clamp(page,0,definition.endingPages.Length-1);
            var credits=page==definition.endingPages.Length-1;
            var sheet=RelicSheet(credits?"LIGHT BETWEEN CULTURES":"Những nhịp cầu ánh sáng","Lumi · "+(credits?"Cảm ơn bạn đã cùng đi hết chặng đường này.":"Mỗi nơi vẫn giữ một màu sáng của riêng mình."));sheet.name=credits?"CreditsPanel":"EpiloguePanel";
            if(credits)LayoutCredits(sheet,definition.endingPages[page]);
            else
            {
                RelicPicture(sheet,"EndingRelic",AdventureUIArt.Load().Relic(NodeEventManager.FinalRelicIds[page]),new Vector2(.27f,.51f),new Vector2(.73f,.76f));
                var body=ui.Label(sheet,"EndingText",definition.endingPages[page],new Vector2(.08f,.29f),new Vector2(.92f,.50f),20);body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperLeft;
                ui.Label(sheet,"EndingPage",(page+1)+" / "+definition.endingPages.Length,new Vector2(.1f,.265f),new Vector2(.9f,.295f),14).color=DesignKitArt.Teal;
            }
            MobileUI.Button(ui,sheet,"EndingNext",credits?"Tiếp tục khám phá":"Tiếp theo",credits?new Vector2(.10f,.045f):new Vector2(.10f,.18f),credits?new Vector2(.90f,.125f):new Vector2(.90f,.25f),()=>
            {
                if(!game.Events.CanViewCanonicalEnding){Close();return;}
                if(credits){var result=game.Events.Execute(node,"ending_ack");if(result.stop)Close();}
                else ShowEnding(node,page+1);
            },true,true);
            if(!credits)MobileUI.Button(ui,sheet,"EndingSkip","Khép lời kết",new Vector2(.18f,.105f),new Vector2(.82f,.165f),()=>{if(game.Events.Execute(node,"ending_ack").stop)Close();},illustrated:true);
        }
        void LayoutCredits(RectTransform sheet,string copy)
        {
            var paragraphs=copy.Split(new[]{"\n\n"},StringSplitOptions.None);
            var title=sheet.Find("RelicTitle").GetComponent<Text>();
            var tagline=sheet.Find("RelicInstructions").GetComponent<Text>();
            sheet.anchorMin=new Vector2(.04f,.05f);sheet.anchorMax=new Vector2(.96f,.95f);
            sheet.Find("ContextHelp").gameObject.SetActive(false);sheet.Find("RelicExit").gameObject.SetActive(false);
            UIController.SetRect(title.rectTransform,new Vector2(.06f,.87f),new Vector2(.94f,.98f));
            UIController.SetRect(tagline.rectTransform,new Vector2(.09f,.775f),new Vector2(.91f,.865f));
            title.text=paragraphs[0];title.fontStyle=FontStyle.Bold;
            tagline.text=paragraphs.Length>1?paragraphs[1]:"";
            RelicPicture(sheet,"EndingRelic",art.lumiPortrait,new Vector2(.38f,.62f),new Vector2(.62f,.77f)).raycastTarget=false;
            var viewport=ui.Panel(sheet,"CreditsViewport",new Vector2(.08f,.15f),new Vector2(.92f,.61f),Color.clear);
            viewport.gameObject.AddComponent<RectMask2D>();
            var bodyCopy=paragraphs.Length==6?paragraphs[2]+"\n\n<b>"+paragraphs[3]+"</b>\n\n"+paragraphs[4]+"\n\n<i>"+paragraphs[5]+"</i>":copy;
            var body=ui.Label(viewport,"EndingText",bodyCopy,new Vector2(0,1),Vector2.one,26,false);
            body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperCenter;body.lineSpacing=1.12f;body.supportRichText=true;
            body.rectTransform.pivot=new Vector2(.5f,1);body.horizontalOverflow=HorizontalWrapMode.Wrap;body.verticalOverflow=VerticalWrapMode.Overflow;
            body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
            var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=body.rectTransform;
            scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=28;
            var typography=sheet.gameObject.AddComponent<CreditsTypography>();typography.title=title;typography.tagline=tagline;typography.body=body;typography.fullTitle=paragraphs[0];
        }
    }
}
