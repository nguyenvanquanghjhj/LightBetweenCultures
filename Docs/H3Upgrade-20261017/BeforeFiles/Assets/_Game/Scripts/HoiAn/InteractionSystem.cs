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
        public void Configure(HoiAnPlayerController p,HoiAnItem target,UIController hud) { player=p; item=target; ui=hud; }
        private void Update()
        {
            if(GameBootstrap.Instance.Scenes.Busy) return;
            if(Keyboard.current!=null && Keyboard.current.eKey.wasPressedThisFrame) Interact();
            if(pending && !player.Walking) { pending=false; Interact(); }
            bool pressed=false; Vector2 screen=Vector2.zero;
            var touch=Touchscreen.current;
            if(touch!=null && touch.primaryTouch.press.wasPressedThisFrame) { pressed=true; screen=touch.primaryTouch.position.ReadValue(); }
            else if(Mouse.current!=null && Mouse.current.leftButton.wasPressedThisFrame) { pressed=true; screen=Mouse.current.position.ReadValue(); }
            if(!pressed || OverUI(screen)) return;
            var world=(Vector2)Camera.main.ScreenToWorldPoint(screen);
            if(item.gameObject.activeSelf && Vector2.Distance(world,item.transform.position)<.7f)
            {
                if(Vector2.Distance(player.Position,item.transform.position)<=1.2f) { Interact(); return; }
                pending=player.MoveTo(item.transform.position);
            }
            else { pending=false; if(!player.MoveTo(world)) ui.Say("Vị trí bị chặn. Chạm vào mặt đường để đi vòng."); }
        }
        private static bool OverUI(Vector2 screen)
        {
            if(EventSystem.current==null) return false;
            var hits=new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},hits); return hits.Count>0;
        }
        public void Interact()
        {
            if(!GameBootstrap.Instance.Quests.Active) { ui.Say("Hãy về H2 gặp cô Mai nhận nhiệm vụ trước."); return; }
            if(!item.gameObject.activeSelf) { ui.Say(GameBootstrap.Instance.Quests.Status); return; }
            if(Vector2.Distance(player.Position,item.transform.position)>1.2f) { ui.Say("Đến gần linh kiện màu vàng rồi nhặt (E / Tương tác)."); return; }
            if(item.Collect()) ui.Say("Đã nhặt linh kiện! Về bản đồ H3 rồi đến H2 gặp cô Mai.");
        }
    }
}
