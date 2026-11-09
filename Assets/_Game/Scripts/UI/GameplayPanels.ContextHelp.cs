using System;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        // A child of the current view: opening/closing help must not recreate
        // the activity, cancel a drag session, consume items or change a quest.
        void ContextHelp(RectTransform sheet,string topic,string instructions,string heading="ActivityTitle",bool battle=false)
        {
            var title=sheet.Find(heading)?.GetComponent<Text>();
            if(title!=null){var max=title.rectTransform.anchorMax;max.x=.73f;title.rectTransform.anchorMax=max;}
            var min=new Vector2(.76f,battle?.85f:.905f);var maxAt=new Vector2(.94f,battle?.935f:.975f);
            var key=(System.Environment.GetCommandLineArgs().Length>0&&Array.IndexOf(System.Environment.GetCommandLineArgs(),"--gameplay-smoke")>=0?"LBC.Help.QA.":"LBC.Help.")+topic;
            MobileUI.Button(ui,sheet,"ContextHelp","Cách chơi",min,maxAt,()=>
            {
                if(overlay==null||overlay.Find("ContextHelpOverlay")!=null)return;
                var layer=ui.Panel(overlay,"ContextHelpOverlay",Vector2.zero,Vector2.one,new Color(.02f,.06f,.10f,.42f));
                var card=MobileUI.Frame(ui,layer,"ContextHelpCard",new Vector2(.07f,.23f),new Vector2(.93f,.78f),AdventureUIArt.Load().window,2.8f);
                Action dismiss=()=>{PlayerPrefs.SetInt(key,1);PlayerPrefs.Save();layer.gameObject.SetActive(false);UnityEngine.Object.Destroy(layer.gameObject);};
                MobileUI.CloseButton(ui,card,"HelpCloseX",()=>dismiss());
                ui.Label(card,"HelpTitle","Cùng Lumi thử nhé",new Vector2(.07f,.82f),new Vector2(.75f,.96f),23).color=DesignKitArt.Ink;
                var viewport=ui.Panel(card,"HelpViewport",new Vector2(.08f,.22f),new Vector2(.92f,.80f),Color.clear);viewport.gameObject.AddComponent<RectMask2D>();
                var body=ui.Label(viewport,"HelpText",instructions+"\n\nBạn có thể đóng hướng dẫn để tiếp tục ngay tại chỗ.",new Vector2(0,1),Vector2.one,19,false);body.color=DesignKitArt.Ink;body.alignment=TextAnchor.UpperLeft;body.rectTransform.pivot=new Vector2(.5f,1);
                body.gameObject.AddComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
                var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
                MobileUI.Button(ui,card,"HelpDismiss","Đã hiểu · Tiếp tục",new Vector2(.10f,.055f),new Vector2(.90f,.17f),()=>dismiss(),true,true);
            },illustrated:true);
        }
        string StoryHelp(StoryInteractionDefinition d)
        {
            var text=d.introText;
            if(d.itemIds.Length>0)text+="\n\nChạm mảnh rồi chạm ô có hình mờ tương ứng, hoặc kéo gần ô để tự ghép và giữ lại."+(Array.Exists(d.targetTurns,t=>t!=0)?" Dùng Xoay để khớp nét mẫu.":"");
            if(d.dialTargets.Length>0)text+="\n\nChạm mũi tên để xoay từng bộ phận theo nét hướng trên bàn.";
            if(d.inspectCount>0)text+="\n\nChạm các điểm Quan sát để ghi lại những nét đang thấy.";
            if(d.gesture=="hold")text+="\n\nSau khi khớp mẫu, giữ nút in cho tới khi hoàn tất.";
            else if(!string.IsNullOrEmpty(d.gesture))text+="\n\nKéo thanh thao tác theo hướng chỉ dẫn sau khi các phần đã khớp.";
            return text+"\n\nChọn Hoàn tất để ghi nhận. Thử sai không mất vật phẩm; rời bàn chưa hoàn tất có thể cần làm lại thao tác.";
        }
        string BattleHelp()
        {
            var text="Mỗi lượt, chọn một hành động rồi xem đối thủ phản hồi.\n\nTấn công: đánh thường. Tia sáng: dùng 1 năng lượng cho đòn mạnh hơn. Phòng thủ: giảm sát thương nhận và hồi 1 năng lượng, tối đa 3.\n\nUống trà: hồi tối đa 14 HP (tối đa 100), dùng 1 ly và 1 lượt; đối thủ vẫn đáp trả. Thức ăn làm mồi và nguyên liệu nhiệm vụ không hồi HP/năng lượng, không dùng trong trận. Rút lui trở về bản đồ; đối thủ chưa được giải quyết vẫn giữ đường.";
            if(game.Battles.Shielded)text+="\n\nKhiên đang đóng: quan sát ký hiệu nhịp đang sáng và chọn ký hiệu phù hợp trước khi tấn công.";
            if(game.Battles.Enemy?.mechanic=="two_phase")text+="\n\nĐối thủ có hai pha. Phá khiên chưa phải chiến thắng; xem phản hồi khi lõi lộ ra. Lựa chọn sửa mạch chỉ xuất hiện nếu bạn đã có đủ manh mối.";
            return text;
        }
        void LeisureHelpAndProgress(RectTransform sheet,LeisureDefinition entry)
        {
            ContextHelp(sheet,"leisure-"+entry.mechanic,PlayerCopy.LeisureHelp(entry.mechanic));
            var track=ui.Panel(sheet,"LeisureProgress",new Vector2(.09f,.188f),new Vector2(.91f,.198f),new Color(.18f,.38f,.39f,.30f));track.GetComponent<Image>().raycastTarget=false;
            var fill=ui.Panel(track,"Fill",Vector2.zero,new Vector2(0,1),DesignKitArt.Teal);fill.GetComponent<Image>().raycastTarget=false;
            var progress=sheet.gameObject.AddComponent<LeisureSessionProgress>();progress.fill=fill;progress.pilot=sheet.GetComponent<LeisurePilotView>();progress.activity=sheet.GetComponent<LeisureActivityView>();
        }
    }
    // Read-only feedback over the existing animation clocks. No activity state,
    // timing, pose, reward or save authority is duplicated here.
    public sealed class LeisureSessionProgress:MonoBehaviour
    {
        public RectTransform fill;public LeisurePilotView pilot;public LeisureActivityView activity;
        public float Value{get;private set;}
        void Update()
        {
            Value=pilot!=null?pilot.Elapsed/LeisurePilotView.Duration:activity!=null?activity.Elapsed/LeisureActivityView.Duration:0;
            if(fill!=null)fill.anchorMax=new Vector2(Mathf.Clamp01(Value),1);
        }
    }
}
