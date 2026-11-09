using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

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
        public bool InputEnabled { get; set; } = true;
        public System.Action BackgroundTapped;
        public bool SuppressSelection => dragging || pinching || Time.unscaledTime < suppressUntil;
        private float aspect;
        private Vector2 baseSize, previousViewport;
        private bool dragging, pinching;
        private float suppressUntil, pinchDistance;
        private int fingerA, fingerB;
        private Vector2 pinchCenter;

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
            var next = Mathf.Clamp(value, MinZoom, MaxZoom);
            Content.anchoredPosition = focus - (focus - Content.anchoredPosition) * (next / Zoom);
            Zoom = next; Content.localScale = new Vector3(Zoom,Zoom,1); Clamp();
        }
        public void PanBy(Vector2 delta) { Content.anchoredPosition += delta; Clamp(); }
        public void Focus(Vector2 normalized)
        { Content.anchoredPosition = -Vector2.Scale(normalized - new Vector2(.5f,.5f), baseSize) * Zoom; Clamp(); }
        public void EnsureVisible(Vector2 normalized)
        {
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
        private void Update()
        {
            if (Content == null || !InputEnabled) { pinching=false; dragging=false; return; }
            TouchControl first=null, second=null;
            var screen=Touchscreen.current;
            if(screen!=null) foreach(var touch in screen.touches)
            {
                if(!touch.press.isPressed) continue;
                if(first==null) first=touch; else { second=touch; break; }
            }
            if(second!=null)
            {
                var a=first.position.ReadValue(); var b=second.position.ReadValue();
                var center=Local((a+b)*.5f); var distance=Vector2.Distance(a,b);
                if(!pinching || fingerA!=first.touchId.ReadValue() || fingerB!=second.touchId.ReadValue())
                {
                    if(!Inside(a)||!Inside(b)) return;
                    pinching=true; dragging=false; fingerA=first.touchId.ReadValue(); fingerB=second.touchId.ReadValue();
                }
                else if(distance>5 && pinchDistance>5) { ZoomAt(Zoom*distance/pinchDistance,pinchCenter); PanBy(center-pinchCenter); }
                pinchCenter=center; pinchDistance=distance; suppressUntil=Time.unscaledTime+.15f; return;
            }
            if(pinching) { pinching=false; suppressUntil=Time.unscaledTime+.15f; }
            var mouse=Mouse.current;
            if(mouse!=null && Inside(mouse.position.ReadValue()))
            {
                var wheel=mouse.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>.01f) ZoomAt(Zoom*Mathf.Exp(wheel/120f*.18f),Local(mouse.position.ReadValue()));
            }
        }
        public void OnBeginDrag(PointerEventData data)
        { if(InputEnabled&&!pinching&&Inside(data.pressPosition)) { dragging=true; suppressUntil=Time.unscaledTime+.15f; } }
        public void OnDrag(PointerEventData data)
        { if(InputEnabled&&dragging&&!pinching) PanBy(Local(data.position)-Local(data.position-data.delta)); }
        public void OnEndDrag(PointerEventData data)
        { dragging=false; suppressUntil=Time.unscaledTime+.15f; }
        public void OnPointerClick(PointerEventData data)
        { if (InputEnabled && !SuppressSelection) BackgroundTapped?.Invoke(); }
    }
}
