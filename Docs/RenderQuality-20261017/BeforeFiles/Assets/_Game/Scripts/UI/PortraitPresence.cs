using UnityEngine;

namespace LightBetweenCultures
{
    /// <summary>One short entrance, shared by portraits and panels; no perpetual motion.</summary>
    public sealed class PortraitPresence : MonoBehaviour
    {
        CanvasGroup group;
        float started;
        public bool slide;
        public bool important;
        Vector2 origin;
        RectTransform rect;
        Vector3 originScale;
        UnityEngine.UI.Image speakerImage;
        Sprite nextSprite;
        Vector2 nextSize;
        bool switching;
        bool nextMesh;
        public void TransitionFrom(Sprite previous,Vector2 previousPosition,Vector2 previousSize,bool previousMesh)
        {
            speakerImage=GetComponent<UnityEngine.UI.Image>();
            if(previous==null||speakerImage==null||previous==speakerImage.sprite)return;
            nextSprite=speakerImage.sprite;nextSize=rect.sizeDelta;
            nextMesh=speakerImage.useSpriteMesh;speakerImage.useSpriteMesh=previousMesh;
            speakerImage.sprite=previous;rect.anchoredPosition=previousPosition;rect.sizeDelta=previousSize;
            switching=true;started=Time.unscaledTime;group.alpha=1;transform.localScale=originScale;
            Tick();
        }
        public void Skip(){started=Time.unscaledTime-1;Tick();}
        void OnEnable()
        {
            group=GetComponent<CanvasGroup>()??gameObject.AddComponent<CanvasGroup>();
            started=Time.unscaledTime;rect=transform as RectTransform;origin=rect!=null?rect.anchoredPosition:Vector2.zero;originScale=transform.localScale;
        }
        void Update()
        {Tick();}
        void OnDisable()
        {
            // A receipt pauses portrait motion. Finish a pending handoff so
            // reopening dialogue cannot revive the previous speaker's sprite.
            if(switching&&speakerImage!=null)
            {speakerImage.sprite=nextSprite;speakerImage.useSpriteMesh=nextMesh;rect.anchoredPosition=origin;rect.sizeDelta=nextSize;}
            switching=false;nextSprite=null;
            if(group!=null)group.alpha=0;
        }
        void Tick()
        {
            var age=Time.unscaledTime-started;
            var mode=NodeMarkerView.MotionMode;
            if(switching)
            {
                var outDuration=mode==MarkerMotionMode.Off?0:mode==MarkerMotionMode.Reduced?.06f:.10f;
                if(age<outDuration){group.alpha=1-Mathf.SmoothStep(0,1,age/outDuration);return;}
                // One image changes slot/sprite only while completely invisible.
                group.alpha=0;speakerImage.sprite=nextSprite;speakerImage.useSpriteMesh=nextMesh;nextSprite=null;
                rect.anchoredPosition=origin;rect.sizeDelta=nextSize;switching=false;
                started+=outDuration;age=Time.unscaledTime-started;
            }
            var t=mode==MarkerMotionMode.Off?1:Mathf.Clamp01(age/(mode==MarkerMotionMode.Reduced?.14f:.28f));
            group.alpha=Mathf.SmoothStep(0,1,t);
            transform.localScale=originScale*Mathf.Lerp(mode==MarkerMotionMode.Reduced?.99f:.96f,1,Mathf.SmoothStep(0,1,t));
            if(rect!=null&&slide)rect.anchoredPosition=origin-Vector2.up*(1-Mathf.SmoothStep(0,1,t))*12;
            if(important){var glow=transform.Find("ReceiptGlow");if(glow!=null){var graphic=glow.GetComponent<UnityEngine.UI.Graphic>();var c=graphic.color;c.a=mode==MarkerMotionMode.Off?0:.32f*Mathf.Sin(Mathf.Clamp01(age/.45f)*Mathf.PI);graphic.color=c;}}
        }
    }
}
