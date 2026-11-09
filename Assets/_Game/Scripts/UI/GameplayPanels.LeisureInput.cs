using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class LeisureActivityView
    {
        public Vector2 ReleaseDestination {get;private set;}=new Vector2(.84f,.34f);
        public RectTransform ActivityTool=>tool;
        public Vector2 KiteAnchor {get;private set;}=new Vector2(.72f,.66f);
        public float BellStrength {get;private set;}=1;
        Vector2 wishFrom;RectTransform wishGuide;
        LeisureThreadGraphic aim;
        void ConfigureAgency()
        {
            switch(definition.mechanic)
            {
                case "release":ConfigureRelease();break;
                case "fly":ConfigureKite();break;
                case "bell":ConfigureBell();break;
                case "wish":ConfigureWish();break;
            }
        }
        void ResetAgencyReplay(){wishFrom=toolHome;if(wishGuide!=null)wishGuide.gameObject.SetActive(false);BellStrength=1;}
        LeisurePropGesture PropGesture(Vector2 at)
        {
            var hit=Group(tool,"LeisureGesture",at,new Vector2(84,84));hit.gameObject.AddComponent<Image>().color=Color.clear;
            var gesture=hit.gameObject.AddComponent<LeisurePropGesture>();gesture.canceled=()=>{if(!Playing){ResetAgencyReplay();tool.anchoredPosition=toolHome;Pose(0);}};return gesture;
        }
        void AgencyInstructions(string text){transform.Find("ActivityInstructions").GetComponent<Text>().text=text;status.text=text;}
        void ConfigureRelease()
        {
            var instructions=transform.Find("ActivityInstructions").GetComponent<Text>();
            instructions.text="Kéo hoa đăng ra mặt sông rồi thả tay để chọn hướng trôi. Cũng có thể chạm hoa hoặc chọn Thả hoa đăng.";
            play.GetComponentInChildren<Text>().text="Thả hoa đăng";
            status.text="Chọn một hướng trên mặt nước · Không có điểm số hay phần thưởng.";
            var line=Group(scene,"LeisureAimLine",new Vector2(.5f,.5f),Vector2.zero);
            aim=line.gameObject.AddComponent<LeisureThreadGraphic>();aim.raycastTarget=false;line.gameObject.SetActive(false);
            var input=PropGesture(new Vector2(.5f,.5f));input.accepts=()=>!Playing;
            input.began=e=>{aim.gameObject.SetActive(true);PreviewRelease(e);};
            input.moved=PreviewRelease;
            input.ended=e=>
            {
                aim.gameObject.SetActive(false);
                var at=ScenePoint(e);
                if(at.x<.61f||at.x>.96f||at.y<.20f||at.y>.52f){status.text="Thả hoa trên phần mặt sông bên phải Lumi nhé. Có thể thử lại ngay.";return;}
                PreviewRelease(e);aim.gameObject.SetActive(false);SoftAudio.Cue(SoftCue.UI_Confirm);Begin();
            };
            input.tapped=e=>{SoftAudio.Cue(SoftCue.UI_Confirm);Begin();};
        }
        void ConfigureKite()
        {
            AgencyInstructions("Kéo cánh diều trên trời để chọn hướng bay; dây vẫn ở trong cánh Lumi. Có thể bắt đầu lại bất cứ lúc nào.");
            var input=PropGesture(new Vector2(.5f,.5f));input.accepts=()=>true;
            Action<PointerEventData> steer=e=>
            {
                var p=ScenePoint(e);KiteAnchor=new Vector2(Mathf.Clamp(p.x,.50f,.86f),Mathf.Clamp(p.y,.54f,.80f));
                toolHome=Vector2.Scale(KiteAnchor-new Vector2(.72f,.66f),new Vector2(w,h));Pose(Elapsed);status.text="Diều đang theo hướng bạn kéo · Dây vẫn nối với Lumi.";
            };
            input.began=e=>{if(!Playing){SoftAudio.Cue(SoftCue.UI_Confirm);Begin();}steer(e);};input.moved=steer;input.ended=steer;
            input.tapped=e=>{if(!Playing){SoftAudio.Cue(SoftCue.UI_Confirm);Begin();}};
        }
        void ConfigureBell()
        {
            AgencyInstructions("Kéo chày chuông rồi thả tay. Kéo ngắn để gióng nhẹ, kéo xa hơn để chày đung đưa rộng hơn.");
            var input=PropGesture(new Vector2(660/sprite.rect.width,1-580/sprite.rect.height));input.accepts=()=>!Playing;
            Action<PointerEventData> strength=e=>
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(scene,e.pressPosition,e.pressEventCamera,out var start);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(scene,e.position,e.pressEventCamera,out var end);
                BellStrength=Mathf.Clamp(.45f+Vector2.Distance(start,end)/w*4,.45f,1.2f);
                tool.localEulerAngles=new Vector3(0,0,-14*BellStrength);status.text=BellStrength>.9f?"Thả tay để gióng một nhịp rộng.":"Thả tay để gióng một nhịp nhẹ.";
            };
            input.began=strength;input.moved=strength;input.ended=e=>{strength(e);SoftAudio.Cue(SoftCue.UI_Confirm);Begin();};
            input.tapped=e=>{SoftAudio.Cue(SoftCue.UI_Confirm);Begin();};
        }
        void ConfigureWish()
        {
            wishFrom=toolHome;AgencyInstructions("Kéo thẻ ước lên ô sáng trên giá rồi thả tay. Thả lệch có thể thử lại, không mất vật phẩm.");
            wishGuide=ui.Picture(scene,"LeisureDropTarget",AdventureUIArt.Load().slot,tool.anchorMin,tool.anchorMax).rectTransform;
            wishGuide.sizeDelta=new Vector2(56,64);wishGuide.anchoredPosition=toolTarget;wishGuide.GetComponent<Image>().color=new Color(1,.88f,.5f,.40f);wishGuide.GetComponent<Image>().raycastTarget=false;wishGuide.SetSiblingIndex(tool.GetSiblingIndex());
            var input=PropGesture(new Vector2(.5f,.5f));input.accepts=()=>!Playing;
            Action<PointerEventData> move=e=>{wishGuide.gameObject.SetActive(true);tool.anchoredPosition=Vector2.Scale(ScenePoint(e)-tool.anchorMin,new Vector2(w,h));};
            input.began=move;input.moved=move;input.ended=e=>
            {
                move(e);if(Vector2.Distance(tool.anchoredPosition,toolTarget)>56){tool.anchoredPosition=wishFrom=toolHome;status.text="Đưa thẻ đến ô sáng trên giá nhé. Có thể thử lại ngay.";return;}
                wishFrom=tool.anchoredPosition;wishGuide.gameObject.SetActive(false);SoftAudio.Cue(SoftCue.UI_Confirm);Begin();
            };
            input.tapped=e=>{ResetAgencyReplay();SoftAudio.Cue(SoftCue.UI_Confirm);Begin();};
        }
        Vector2 ScenePoint(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(scene,e.position,e.pressEventCamera,out var local);
            return new Vector2((local.x-scene.rect.xMin)/w,(local.y-scene.rect.yMin)/h);
        }
        void PreviewRelease(PointerEventData e)
        {
            var at=ScenePoint(e);ReleaseDestination=new Vector2(Mathf.Clamp(at.x,.65f,.90f),Mathf.Clamp(at.y,.25f,.43f));
            toolTarget=toolHome+Vector2.Scale(ReleaseDestination-new Vector2(.58f,.18f),new Vector2(w,h));
            aim.Start=aim.rectTransform.InverseTransformPoint(tool.position);
            aim.End=aim.rectTransform.InverseTransformPoint(scene.TransformPoint(scene.rect.min+Vector2.Scale(ReleaseDestination,scene.rect.size)));
            aim.SetVerticesDirty();status.text="Thả tay để gửi ánh đèn theo hướng vừa chọn.";
        }
    }

    // A disposable input surface on an existing prop. No progression or rewards.
    public sealed class LeisurePropGesture:MonoBehaviour,IPointerDownHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,IPointerClickHandler
    {
        public Func<bool> accepts;
        public Action<PointerEventData> began,moved,ended,tapped;
        public Action canceled;
        bool dragging,suppressClick;int owner;uint epoch;PointerTapIntent tap;
        bool Owns(PointerEventData e)=>e.pointerId==owner&&epoch==TouchInputPolicy.GestureEpoch&&!TouchInputPolicy.Canceled(e);
        public void OnPointerDown(PointerEventData e){TouchInputPolicy.Sample();if(tap.Begin(e)){suppressClick=false;owner=e.pointerId;epoch=TouchInputPolicy.GestureEpoch;}}
        public void OnBeginDrag(PointerEventData e){TouchInputPolicy.Sample();dragging=Owns(e)&&TouchInputPolicy.ActiveTouches<2&&e.button==PointerEventData.InputButton.Left&&accepts?.Invoke()==true;if(dragging){suppressClick=true;began?.Invoke(e);}}
        public void OnDrag(PointerEventData e){TouchInputPolicy.Sample();if(dragging&&Owns(e)&&accepts?.Invoke()==true)moved?.Invoke(e);}
        public void OnEndDrag(PointerEventData e){TouchInputPolicy.Sample();if(e.pointerId!=owner)return;if(dragging){if(Owns(e)&&accepts?.Invoke()==true)ended?.Invoke(e);else canceled?.Invoke();}dragging=false;}
        public void OnPointerClick(PointerEventData e){if(!suppressClick&&tap.Accept(e)&&accepts?.Invoke()==true)tapped?.Invoke(e);}
        void OnDisable(){dragging=false;suppressClick=false;}
    }
}
