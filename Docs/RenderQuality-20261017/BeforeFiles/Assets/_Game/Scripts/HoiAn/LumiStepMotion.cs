using UnityEngine;

namespace LightBetweenCultures.HoiAn
{
    // A single standing pose with a small motion cue. This is not a drawn walk cycle.
    public sealed class LumiStepMotion : MonoBehaviour
    {
        private Transform visual;
        private Vector3 rest, previous;
        private void Start()
        {
            visual=transform.Find("CanonicalLumiVisual");
            if (visual==null) { enabled=false; return; }
            rest=visual.localPosition; previous=transform.position;
            var shade=new GameObject("LumiFootShadow",typeof(SpriteRenderer));
            shade.transform.SetParent(transform,false);
            shade.transform.localPosition=new Vector3(0,-.33f,0);
            shade.transform.localScale=new Vector3(.60f/transform.lossyScale.x,.22f/transform.lossyScale.y,1);
            var renderer=shade.GetComponent<SpriteRenderer>(); renderer.sprite=PresentationShapes.Shadow;
            renderer.sortingOrder=-1;
        }
        private void LateUpdate()
        {
            var moving=(transform.position-previous).sqrMagnitude>.000001f;
            previous=transform.position;
            var phase=Time.time*15;
            visual.localPosition=rest+Vector3.up*(moving?Mathf.Abs(Mathf.Sin(phase))*.025f:0);
            visual.localRotation=Quaternion.Euler(0,0,moving?Mathf.Sin(phase)*2.5f:0);
        }
    }
}
