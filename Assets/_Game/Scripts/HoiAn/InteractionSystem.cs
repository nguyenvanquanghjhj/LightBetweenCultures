using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
namespace LightBetweenCultures.HoiAn
{
    public sealed class InteractionSystem : MonoBehaviour
    {
        private HoiAnPlayerController player;
        private HoiAnItem item;
        private UIController ui;
        private bool pending;
        public const float PickupRange=1.6f;
        public bool Approaching=>pending;
        public bool CanInteract=>item!=null&&item.gameObject.activeSelf&&GameBootstrap.Instance.Quests.Active&&
            Vector2.Distance(player.Position,item.transform.position)<=PickupRange&&ClearReach(player.Position);
        public void Configure(HoiAnPlayerController p,HoiAnItem target,UIController hud) { player=p; item=target; ui=hud; }
        bool ClearReach(Vector2 from)=>Physics2D.Linecast(from,item.transform.position,HoiAnPlayerController.ObstacleMask).collider==null;
        public Rect ItemTapRect
        {
            get
            {
                var visual=item.transform.Find("H3PickupVisual")?.GetComponent<SpriteRenderer>()??item.GetComponent<SpriteRenderer>();
                var center=(Vector2)Camera.main.WorldToScreenPoint(item.transform.position);
                var rect=new Rect(center-Vector2.one*12,Vector2.one*24);
                if(visual!=null)
                {
                    var a=(Vector2)Camera.main.WorldToScreenPoint(visual.bounds.min);var b=(Vector2)Camera.main.WorldToScreenPoint(visual.bounds.max);
                    rect=Rect.MinMaxRect(Mathf.Min(a.x,b.x),Mathf.Min(a.y,b.y),Mathf.Max(a.x,b.x),Mathf.Max(a.y,b.y));
                }
                var size=Mathf.Clamp(Screen.width/540f*48,48,88);
                var growX=Mathf.Max(6,(size-rect.width)*.5f);var growY=Mathf.Max(6,(size-rect.height)*.5f);
                return Rect.MinMaxRect(rect.xMin-growX,rect.yMin-growY,rect.xMax+growX,rect.yMax+growY);
            }
        }
        void ApproachItem()
        {
            pending=false;
            if(!GameBootstrap.Instance.Quests.Active){ui.Say("Hãy trở về tiệm Cô Mai để nhận việc tìm Mắt Đèn trước nhé.");return;}
            if(CanInteract){player.Stop();ui.Say("Đã đến gần Mắt Đèn. Bấm Nhặt Mắt Đèn để cất vào túi.");return;}
            var target=(Vector2)item.transform.position;var toward=(player.Position-target).normalized;
            if(toward==Vector2.zero)toward=Vector2.down;
            var angle=Mathf.Atan2(toward.y,toward.x);var candidates=new List<Vector2>();
            // Arrival stays on walkable ground around the item, never at an
            // obstructed item centre. Reuse H3's existing collider pathfinder.
            foreach(var radius in new[]{1.35f,.9f})for(var i=0;i<12;i++)
            {var a=angle+i*Mathf.PI/6;candidates.Add(target+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius);}
            candidates.Sort((a,b)=>(a-player.Position).sqrMagnitude.CompareTo((b-player.Position).sqrMagnitude));
            foreach(var point in candidates)
                if(HoiAnPlayerController.Walkable(point)&&ClearReach(point)&&player.MoveTo(point))
                {pending=true;ui.Say("Lumi đang đến gần Mắt Đèn. Khi tới nơi, bấm Nhặt Mắt Đèn.");return;}
            player.Stop();ui.Say("Chưa có lối đi tới Mắt Đèn từ đây. Hãy chạm vào mặt đường để đi vòng rồi thử lại.");
        }
        private void Update()
        {
            if(GameBootstrap.Instance.Scenes.Busy) return;
            if(Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame) Interact();
            if(pending && !player.Walking)
            {pending=false;ui.Say(CanInteract?"Đã đến gần Mắt Đèn. Bấm Nhặt Mắt Đèn để cất vào túi.":"Bạn có thể chạm lại Mắt Đèn để Lumi tìm đường đến gần.");}
            bool pressed=false; Vector2 screen=Vector2.zero;
            var touch=Touchscreen.current;
            if(touch!=null && touch.primaryTouch.press.wasPressedThisFrame) { pressed=true; screen=touch.primaryTouch.position.ReadValue(); }
            else if(Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame) { pressed=true; screen=Mouse.current.position.ReadValue(); }
            if(!pressed || OverUI(screen)) return;
            var world=(Vector2)Camera.main.ScreenToWorldPoint(screen);
            if(item.gameObject.activeSelf && ItemTapRect.Contains(screen)) ApproachItem();
            else { pending=false; if(!player.MoveTo(world)) ui.Say("Vị trí bị chặn. Chạm vào mặt đường để đi vòng."); }
        }
        private static bool OverUI(Vector2 screen)
        {
            if(EventSystem.current==null) return false;
            var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},hits); return hits.Count>0;
        }
        public void Interact()
        {
            if(!GameBootstrap.Instance.Quests.Active) { ui.Say("Hãy trở về tiệm Cô Mai để nhận việc tìm Mắt Đèn trước nhé."); return; }
            if(!item.gameObject.activeSelf) { ui.Say(GameBootstrap.Instance.Quests.Status); return; }
            if(!CanInteract) { ui.Say("Chạm vào Mắt Đèn để Lumi tìm chỗ đứng gần vật phẩm. Khi nút Nhặt hiện lên, bạn có thể nhặt."); return; }
            pending=false;player.Stop();
            if(item.Collect()) ui.Say("Đã nhặt Mắt Đèn! Trở về bản đồ rồi mang về tiệm Cô Mai.");
        }
    }
}
