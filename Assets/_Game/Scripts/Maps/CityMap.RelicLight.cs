using System;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class CityMap
    {
        RectTransform relicLightLayer;
        bool relicLightKnown,relicLightRestored;
        int knownFragmentCount;float relicLightAt=-100;
        public int RelicFragmentReactions {get;private set;}
        public bool CityRelicIlluminated=>relicLightRestored;
        public int RelicCelebrations {get;private set;}
        void AnimateRelicPresentation()
        {
            TickSignature();
            AnimateCulturalRestoration();
            AnimateDistrictDimming();
            if(relicLightLayer==null)return;
            relicLightLayer.GetComponent<CanvasGroup>().alpha=relicLightRestored?Mathf.Clamp01((Time.unscaledTime-relicLightAt)/1.2f):0;
        }
        void RefreshRelicLights()
        {
            if(game.Relics==null)return;
            if(game.Data.cityId!="hoi_an"){RefreshCulturalRestoration();return;}
            var r=game.Relics.Definition("relic_hoi_an_lantern");var lit=game.Relics.Restored(r.id);
            var count=game.Relics.Count(r);
            if(relicLightKnown&&count>knownFragmentCount){questReactionAt=Time.unscaledTime;RelicFragmentReactions++;}
            knownFragmentCount=count;
            if(relicLightLayer==null)
            {
                relicLightLayer=ui.Panel(map,"RelicIlluminationLayer",Vector2.zero,Vector2.one,Color.clear);relicLightLayer.SetSiblingIndex(2);
                relicLightLayer.GetComponent<Image>().raycastTarget=false;
                var group=relicLightLayer.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
                foreach(var nodeId in r.lightNodes)
                {
                    var index=Array.FindIndex(game.Data.nodes,n=>n.id==nodeId);if(index<0)continue;var n=game.Data.nodes[index];
                    var halo=ui.Picture(relicLightLayer,"RelicLight_"+nodeId,PresentationShapes.WarmGlow,n.position,n.position);halo.color=new Color(1,.72f,.26f,.26f);halo.raycastTarget=false;
                    var lantern=ui.Picture(halo.transform,"Lantern",presentation.Relic(r.id),new Vector2(.30f,.30f),new Vector2(.70f,.70f));lantern.color=new Color(1,.94f,.72f,.90f);lantern.useSpriteMesh=true;lantern.raycastTarget=false;
                }
            }
            if(relicLightKnown&&!relicLightRestored&&lit){RelicCelebrations++;questReactionAt=Time.unscaledTime;relicLightAt=Time.unscaledTime;RequestSignature(r);}
            relicLightKnown=true;relicLightRestored=lit;relicLightLayer.gameObject.SetActive(lit);
            foreach(Transform t in relicLightLayer)
            {
                var index=Array.FindIndex(game.Data.nodes,n=>"RelicLight_"+n.id==t.name);
                t.gameObject.SetActive(index>=0&&game.Discovery.IsVisible(index));
                ((RectTransform)t).sizeDelta=new Vector2(map.rect.width*.065f,map.rect.height*.07f);
            }
            RefreshDistrictDimming(lit);
        }

        [Serializable] sealed class HoiAnLightingRules
        {
            public float targetDisplayBrightness=.6f,linearBlendExponent=2.2f,featherPixels=32,localSpreadSeconds=1.25f,fullSpreadSeconds=1.8f;
            public int maskWidth=256,maskHeight=192,mapWidth=1448,mapHeight=1086;
            public Vector2[] districtBoundaryPixels=Array.Empty<Vector2>();
        }
        HoiAnLightingRules districtRules;
        RawImage districtDim;
        Texture2D districtMask;
        Color32[] districtPixels;
        float[] districtDistance,districtOriginDistance;
        int districtState=-1;
        float districtAt=-100,districtNextUpload,maxDistrictDistance,maxOriginDistance,dimOpacity;
        bool districtAnimating,districtWaiting;
        public string HoiAnLightingState=>districtState<0?"Inactive":districtState==0?"A":districtState==1?"B":"C";
        public float BackgroundDimOpacity=>dimOpacity;
        public float DistrictBrightCoverage {get;private set;}
        public float DistrictWeightedCoverage {get;private set;}
        public bool LightingTransitionActive=>districtAnimating;
        public float DimmingAlphaAt(Vector2 point)=>districtDim==null||!districtDim.gameObject.activeSelf?0:districtMask.GetPixelBilinear(point.x,point.y).a;

        float DistrictAmount(float signedDistance,float growth=0)
        {
            var t=Mathf.InverseLerp(districtRules.featherPixels,-districtRules.featherPixels,signedDistance-growth);
            return t*t*(3-2*t);
        }
        void InitializeDistrictDimming()
        {
            var source=Resources.Load<TextAsset>("Content/HoiAnLighting");if(source==null)return;
            districtRules=JsonUtility.FromJson<HoiAnLightingRules>(source.text);
            var width=Mathf.Clamp(districtRules.maskWidth,32,512);var height=Mathf.Clamp(districtRules.maskHeight,24,384);
            districtMask=new Texture2D(width,height,TextureFormat.RGBA32,false,true){name="HoiAnThreeStageDimMask",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            districtPixels=new Color32[width*height];districtDistance=new float[width*height];districtOriginDistance=new float[width*height];
            var origin=Array.Find(game.Data.nodes,n=>n.id=="H2").position;
            origin=new Vector2(origin.x*districtRules.mapWidth,(1-origin.y)*districtRules.mapHeight);
            dimOpacity=QualitySettings.activeColorSpace==ColorSpace.Linear?1-Mathf.Pow(districtRules.targetDisplayBrightness,districtRules.linearBlendExponent):1-districtRules.targetDisplayBrightness;
            var fullyBright=0;float coverage=0;
            for(var y=0;y<height;y++)for(var x=0;x<width;x++)
            {
                var p=new Vector2((x+.5f)/width*districtRules.mapWidth,(1-(y+.5f)/height)*districtRules.mapHeight);
                var inside=false;var distance=float.MaxValue;var polygon=districtRules.districtBoundaryPixels;
                for(var j=0;j<polygon.Length;j++)
                {
                    var a=polygon[j];var b=polygon[(j+1)%polygon.Length];
                    if((a.y>p.y)!=(b.y>p.y)&&p.x<(b.x-a.x)*(p.y-a.y)/(b.y-a.y)+a.x)inside=!inside;
                    // Map borders are not a boundary between a lit and an unlit neighborhood.
                    if(a.x==0&&b.x==0||a.y==0&&b.y==0)continue;
                    var v=b-a;var t=Mathf.Clamp01(Vector2.Dot(p-a,v)/Mathf.Max(.001f,v.sqrMagnitude));
                    distance=Mathf.Min(distance,Vector2.Distance(p,a+t*v));
                }
                var index=y*width+x;districtDistance[index]=inside?-distance:distance;districtOriginDistance[index]=Vector2.Distance(p,origin);
                maxDistrictDistance=Mathf.Max(maxDistrictDistance,distance);maxOriginDistance=Mathf.Max(maxOriginDistance,districtOriginDistance[index]);
                var amount=DistrictAmount(districtDistance[index]);coverage+=amount;if(amount>=.95f)fullyBright++;
            }
            DistrictBrightCoverage=(float)fullyBright/districtPixels.Length;DistrictWeightedCoverage=coverage/districtPixels.Length;
            districtDim=new GameObject("HoiAnBackgroundDimming",typeof(RectTransform),typeof(RawImage),typeof(CanvasGroup)).GetComponent<RawImage>();
            districtDim.rectTransform.SetParent(map,false);UIController.SetRect(districtDim.rectTransform,Vector2.zero,Vector2.one);
            districtDim.rectTransform.SetSiblingIndex(1);districtDim.texture=districtMask;districtDim.raycastTarget=false;
            var group=districtDim.GetComponent<CanvasGroup>();group.blocksRaycasts=false;group.interactable=false;
        }
        void RefreshDistrictDimming(bool restoredRelic)
        {
            if(districtDim==null)InitializeDistrictDimming();if(districtDim==null)return;
            // The only sources of truth: completed tutorial and successful canonical relic trial.
            var next=restoredRelic?2:game.Quests.IsCompleted("mai-lantern")?1:0;
            if(next==districtState)return;
            var previous=districtState;districtState=next;districtAt=Time.unscaledTime;districtNextUpload=0;
            districtAnimating=previous>=0&&next>previous;districtWaiting=districtAnimating&&dialogue;
            districtDim.gameObject.SetActive(true);
            UploadDistrictMask(districtAnimating?0:1);
        }
        void AnimateDistrictDimming()
        {
            if(!districtAnimating||districtDim==null)return;
            // Let the player see the spread after closing dialogue/trial; never block input.
            if(districtWaiting){if(dialogue)return;districtWaiting=false;districtAt=Time.unscaledTime;}
            var duration=districtState==2?districtRules.fullSpreadSeconds:districtRules.localSpreadSeconds;
            var t=Mathf.Clamp01((Time.unscaledTime-districtAt)/Mathf.Max(.1f,duration));
            if(t<1&&Time.unscaledTime<districtNextUpload)return;
            districtNextUpload=Time.unscaledTime+.05f;UploadDistrictMask(t);
            if(t>=1)districtAnimating=false;
        }
        void UploadDistrictMask(float progress)
        {
            var eased=progress*progress*(3-2*progress);
            for(var i=0;i<districtPixels.Length;i++)
            {
                var amount=districtState==0?0:DistrictAmount(districtDistance[i],districtState==2?eased*(maxDistrictDistance+districtRules.featherPixels*2):0);
                if(districtState==1&&progress<1)
                {
                    var radius=eased*(maxOriginDistance+districtRules.featherPixels*2);
                    amount*=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(radius-districtRules.featherPixels,radius+districtRules.featherPixels,districtOriginDistance[i]));
                }
                districtPixels[i]=new Color32(0,0,0,(byte)Mathf.RoundToInt(255*dimOpacity*(1-amount)));
            }
            districtMask.SetPixels32(districtPixels);districtMask.Apply(false,false);
            if(districtState==2&&progress>=1)districtDim.gameObject.SetActive(false);
        }
        void OnDestroy()
        {if(districtMask!=null)Destroy(districtMask);}
    }
}
