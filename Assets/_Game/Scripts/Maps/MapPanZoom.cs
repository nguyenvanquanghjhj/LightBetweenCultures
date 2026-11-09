using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using System.Collections.Generic;

namespace LightBetweenCultures
{
    // A camera-like view over one shared map transform. HUD stays outside this transform.
    public sealed class MapPanZoom : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        public const float MinZoom = 1f;
        public const float MaxZoom = 4f;
        public float Zoom { get; private set; } = 1f;
        public RectTransform Content { get; private set; }
        public RectTransform Viewport { get; private set; }
        bool inputEnabled=true;
        public bool InputEnabled { get=>inputEnabled; set{if(inputEnabled&&!value)CancelGesture();inputEnabled=value;} }
        public System.Action BackgroundTapped;
        public bool SuppressSelection => dragging || pinching || consumedTouch || Time.unscaledTime < suppressUntil;
        public bool ManualControlActive=>dragging||pinching||Time.unscaledTime<manualUntil;
        private float aspect;
        private Vector2 baseSize, previousViewport;
        private bool dragging, pinching;
        private float suppressUntil, pinchDistance;
        private int fingerA, fingerB;
        private Vector2 pinchCenter;
        private int dragOwner=int.MinValue;
        private float manualUntil;
        private bool consumedTouch;
        readonly int[] touchIds=new int[16];
        readonly bool[] mapTouches=new bool[16];
        readonly List<RaycastResult> hits=new List<RaycastResult>(32);
        PointerEventData hitPointer;
        EventSystem hitSystem;

        public void Configure(RectTransform content, float imageAspect)
        {
            Viewport = (RectTransform)transform; Content = content; aspect = imageAspect;
            Content.anchorMin = Content.anchorMax = Content.pivot = new Vector2(.5f,.5f);
            Layout(); ResetView();
        }
        private void LateUpdate() { if (Viewport != null && previousViewport != Viewport.rect.size) Layout(); }
        private void Layout()
        {
            var center = baseSize.x > 0 ? -Content.anchoredPosition / (baseSize * Zoom) : Vector2.zero;
            previousViewport = Viewport.rect.size;
            var width = Mathf.Min(previousViewport.x, previousViewport.y * aspect);
            baseSize = new Vector2(width, width / aspect);
            Content.sizeDelta = baseSize;
            Content.anchoredPosition = -Vector2.Scale(center, baseSize * Zoom);
            Clamp();
        }
        public void ResetView() { Zoom = MinZoom; Content.localScale = Vector3.one; Content.anchoredPosition = Vector2.zero; Clamp(); }
        public void ZoomBy(float factor) => ZoomAt(Zoom * factor, Vector2.zero);
        private void ZoomAt(float value, Vector2 focus)
        {
            manualUntil=Time.unscaledTime+1.2f;
            var next = Mathf.Clamp(value, MinZoom, MaxZoom);
            Content.anchoredPosition = focus - (focus - Content.anchoredPosition) * (next / Zoom);
            Zoom = next; Content.localScale = new Vector3(Zoom,Zoom,1); Clamp();
        }
        public void PanBy(Vector2 delta) { manualUntil=Time.unscaledTime+1.2f;Content.anchoredPosition += delta; Clamp(); }
        public void Focus(Vector2 normalized)
        { Content.anchoredPosition = -Vector2.Scale(normalized - new Vector2(.5f,.5f), baseSize) * Zoom; Clamp(); }
        public void EnsureVisible(Vector2 normalized)
        {
            if(ManualControlActive||SuppressSelection)return;
            var point = Vector2.Scale(normalized - new Vector2(.5f,.5f), baseSize) * Zoom + Content.anchoredPosition;
            var half = Viewport.rect.size * .38f;
            if (Mathf.Abs(point.x) > half.x || Mathf.Abs(point.y) > half.y) Focus(normalized);
        }
        public bool WithinBounds()
        {
            var limit = Vector2.Max(Vector2.zero,(baseSize*Zoom-Viewport.rect.size)*.5f);
            return Mathf.Abs(Content.anchoredPosition.x) <= limit.x+.01f && Mathf.Abs(Content.anchoredPosition.y) <= limit.y+.01f;
        }
        private void Clamp()
        {
            var limit = Vector2.Max(Vector2.zero,(baseSize*Zoom-Viewport.rect.size)*.5f);
            var position = Content.anchoredPosition;
            Content.anchoredPosition = new Vector2(Mathf.Clamp(position.x,-limit.x,limit.x),Mathf.Clamp(position.y,-limit.y,limit.y));
        }
        private bool Inside(Vector2 screen) => RectTransformUtility.RectangleContainsScreenPoint(Viewport,screen,null);
        private Vector2 Local(Vector2 screen)
        { RectTransformUtility.ScreenPointToLocalPointInRectangle(Viewport,screen,null,out var point); return point; }
        bool Owns(Vector2 point)
        {
            if(!Inside(point)||EventSystem.current==null)return false;
            if(hitPointer==null||hitSystem!=EventSystem.current){hitSystem=EventSystem.current;hitPointer=new PointerEventData(hitSystem);}
            hitPointer.position=point;hits.Clear();EventSystem.current.RaycastAll(hitPointer,hits);
            return hits.Count>0&&(hits[0].gameObject.transform==transform||hits[0].gameObject.transform.IsChildOf(transform));
        }
        void CancelGesture()
        {
            dragging=pinching=consumedTouch=false;dragOwner=int.MinValue;suppressUntil=Time.unscaledTime+.15f;
            System.Array.Clear(touchIds,0,touchIds.Length);System.Array.Clear(mapTouches,0,mapTouches.Length);TouchInputPolicy.Cancel();
        }
        void OnDisable(){CancelGesture();}
        void OnApplicationFocus(bool focused){if(!focused)CancelGesture();}
        private void Update()
        {
            if (Content == null || !InputEnabled) return;
            TouchControl first=null, second=null;
            var screen=Touchscreen.current;var active=0;
            if(screen!=null) for(var i=0;i<screen.touches.Count&&i<touchIds.Length;i++)
            {
                var touch=screen.touches[i];
                if(!touch.press.isPressed){touchIds[i]=0;mapTouches[i]=false;continue;}
                active++;var id=touch.touchId.ReadValue();
                if(touchIds[i]!=id){touchIds[i]=id;mapTouches[i]=touch.phase.ReadValue()==UnityEngine.InputSystem.TouchPhase.Began&&Owns(touch.position.ReadValue());}
                if(!mapTouches[i])continue;
                if(first==null)first=touch;else if(second==null)second=touch;
            }
            if(active>1&&first!=null)consumedTouch=true;
            if(active==0&&consumedTouch){consumedTouch=false;suppressUntil=Time.unscaledTime+.15f;}
            if(second!=null)
            {
                var a=first.position.ReadValue(); var b=second.position.ReadValue();
                var center=Local((a+b)*.5f); var distance=Vector2.Distance(a,b);
                if(!pinching || fingerA!=first.touchId.ReadValue() || fingerB!=second.touchId.ReadValue())
                {
                    pinching=true; dragging=false; fingerA=first.touchId.ReadValue(); fingerB=second.touchId.ReadValue();
                }
                else if(distance>TouchInputPolicy.DragPixels(Screen.dpi,Screen.width,Screen.height) && pinchDistance>TouchInputPolicy.DragPixels(Screen.dpi,Screen.width,Screen.height)) { ZoomAt(Zoom*distance/pinchDistance,pinchCenter); PanBy(center-pinchCenter); }
                pinchCenter=center; pinchDistance=distance; suppressUntil=Time.unscaledTime+.15f; return;
            }
            if(pinching) { pinching=false; suppressUntil=Time.unscaledTime+.15f; }
            var mouse=Mouse.current;
            if(mouse!=null)
            {
                var wheel=mouse.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>.01f&&Owns(mouse.position.ReadValue())) ZoomAt(Zoom*Mathf.Exp(wheel/120f*.18f),Local(mouse.position.ReadValue()));
            }
        }
        public void OnBeginDrag(PointerEventData data)
        { if(InputEnabled&&!pinching&&TouchInputPolicy.ActiveTouches<2&&Owns(data.pressPosition)) { dragging=true;dragOwner=data.pointerId;manualUntil=Time.unscaledTime+1.2f; suppressUntil=Time.unscaledTime+.15f; } }
        public void OnDrag(PointerEventData data)
        {
            if(!InputEnabled||pinching||TouchInputPolicy.ActiveTouches>1)return;
            if(!dragging&&consumedTouch&&Owns(data.pressPosition)){dragging=true;dragOwner=data.pointerId;}
            if(dragging&&dragOwner==data.pointerId)PanBy(Local(data.position)-Local(data.position-data.delta));
        }
        public void OnEndDrag(PointerEventData data)
        {if(dragOwner!=data.pointerId)return;dragging=false;dragOwner=int.MinValue;suppressUntil=Time.unscaledTime+.15f;}
        public void OnPointerClick(PointerEventData data)
        { if (InputEnabled && !SuppressSelection) BackgroundTapped?.Invoke(); }
    }
}
