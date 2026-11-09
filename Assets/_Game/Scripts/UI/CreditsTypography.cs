using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Long acknowledgments scroll instead of shrinking to tiny text.
    public sealed class CreditsTypography : MonoBehaviour
    {
        public Text title,tagline,body;
        public string fullTitle;
        int width,height;
        float lastScale;
        void LateUpdate()
        {
            if(title==null||tagline==null||body==null)return;
            var scale=Mathf.Max(.01f,GetComponentInParent<Canvas>().scaleFactor);
            if(width==Screen.width&&height==Screen.height&&Mathf.Approximately(lastScale,scale))return;
            width=Screen.width;height=Screen.height;lastScale=scale;
            var pixels=Mathf.Clamp(height/40f,24,30);
            title.text=width<height?fullTitle.Replace(" CULTURES","\nCULTURES"):fullTitle;
            title.resizeTextForBestFit=tagline.resizeTextForBestFit=body.resizeTextForBestFit=false;
            title.fontSize=Mathf.Max(1,Mathf.CeilToInt(pixels*1.25f/scale));
            tagline.fontSize=Mathf.Max(1,Mathf.CeilToInt(pixels*.86f/scale));
            body.fontSize=Mathf.Max(1,Mathf.CeilToInt(pixels/scale));
        }
    }
}
