using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        void ShowLeisurePilot(LeisureDefinition entry)
        {
            var skin=AdventureUIArt.Load();var boat=entry.mechanic=="boat";
            var backdrop=boat?skin.leisureBoatBackground:skin.leisureSwingBackground;
            if(backdrop==null||entry.Sprite==null)return; // Never replace a missing approved asset with invented art.
            var sheet=ActivitySheet("LeisureActivityPanel",entry.title,boat?
                "Một vòng thuyền nhẹ trên sông. Chọn Bắt đầu; có thể trở về bản đồ bất cứ lúc nào.":
                "Chơi xích đu cùng Lumi dưới giàn đèn lồng. Chọn Bắt đầu; có thể trở về bản đồ bất cứ lúc nào.");
            UIController.SetRect(sheet.Find("ActivityInstructions").GetComponent<RectTransform>(),new Vector2(.07f,.79f),new Vector2(.93f,.89f));
            var scene=ui.Panel(sheet,"LeisureScene",new Vector2(.5f,.53f),new Vector2(.5f,.53f),Color.clear);
            Canvas.ForceUpdateCanvases();var width=Mathf.Min(sheet.rect.width*.89f,sheet.rect.height*.46f/.75f);
            scene.sizeDelta=new Vector2(width,width*.75f);scene.gameObject.AddComponent<RectMask2D>();scene.GetComponent<Image>().raycastTarget=false;
            ui.Picture(scene,"LeisureBackground",backdrop,Vector2.zero,Vector2.one);
            var status=ui.Label(sheet,"LeisureStatus","Sẵn sàng · Hoạt động thư giãn, không có phần thưởng.",new Vector2(.07f,.205f),new Vector2(.93f,.29f),17);status.color=new Color(.08f,.20f,.22f,1f);
            var play=MobileUI.Button(ui,sheet,"LeisureAction","Bắt đầu",new Vector2(.07f,.105f),new Vector2(.47f,.18f),()=>{},true,true);
            var back=sheet.Find("ActivityClose").GetComponent<RectTransform>();UIController.SetRect(back,new Vector2(.53f,.105f),new Vector2(.93f,.18f));
            back.GetComponentInChildren<Text>().text="Trở về bản đồ";
            sheet.gameObject.AddComponent<LeisurePilotView>().Build(ui,scene,entry.Sprite,art.lumiExploration,play,status,boat);
            LeisureHelpAndProgress(sheet,entry);
            var help=sheet.Find("ContextHelp")?.GetComponent<RectTransform>();
            if(help!=null)UIController.SetRect(help,new Vector2(.70f,.905f),new Vector2(.86f,.975f));
            // Always-on close control, above the picture even at 16:9/ultrawide.
            var quit=MobileUI.CloseButton(ui,sheet,"LeisureExitX",()=>{game.Puzzles.Cancel();Close();});
            quit.transform.SetAsLastSibling();
        }
    }

    // Transient panel animation only: no save, inventory, quest, audio or route authority.
    public sealed class LeisurePilotView:MonoBehaviour
    {
        public bool Playing {get;private set;}
        public float Elapsed {get;private set;}
        public float SwingAngle {get;private set;}
        public bool Boat {get;private set;}
        public RectTransform FixedFrame {get;private set;}
        public RectTransform Rider {get;private set;}
        public RectTransform SeatOrBoat {get;private set;}
        public RectTransform Backdrop {get;private set;}
        public const float Duration=4.4f;
        RectTransform rig,leftRope,rightRope;Vector2 leftHome,rightHome,seatHome,boatHome;
        Vector2 leftAnchor,rightAnchor,leftEnd,rightEnd;float unit;
        Button play;Text status;LeisureWaterWake wake;

        static RectTransform Group(Transform parent,string name,Vector2 min,Vector2 max)
        {
            var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);UIController.SetRect(rt,min,max);return rt;
        }
        static Vector2[] Box(float x,float y,float w,float h)=>new[]{new Vector2(x,y),new Vector2(x+w,y),new Vector2(x+w,y+h),new Vector2(x,y+h)};
        static Vector2[] Polygon(params float[] values)
        {var p=new Vector2[values.Length/2];for(var i=0;i<p.Length;i++)p[i]=new Vector2(values[i*2],values[i*2+1]);return p;}
        LeisureSpriteLayer Layer(Transform parent,string name,Sprite source,bool outside,params Vector2[][] regions)
        {
            var rt=Group(parent,name,Vector2.zero,Vector2.one);var layer=rt.gameObject.AddComponent<LeisureSpriteLayer>();layer.Configure(source,outside,regions);return layer;
        }
        Vector2 Local(Vector2 pixel)=>new Vector2(pixel.x-384,409-pixel.y)*unit;
        void RopePivot(RectTransform rope,Vector2 anchor)
        {
            // Full-source layer with its pivot placed at this rope's actual suspension.
            rope.anchorMin=rope.anchorMax=new Vector2(.5f,.5f);rope.sizeDelta=rig.sizeDelta;
            rope.pivot=new Vector2(anchor.x/768,1-anchor.y/818);rope.anchoredPosition=Local(anchor);
        }
        public void Build(UIController ui,RectTransform scene,Sprite prop,Sprite lumi,Button button,Text text,bool boat)
        {
            Boat=boat;play=button;status=text;Backdrop=scene.Find("LeisureBackground").GetComponent<RectTransform>();
            rig=Group(scene,"PilotRig",new Vector2(boat?.66f:.51f,boat?.59f:.48f),new Vector2(boat?.66f:.51f,boat?.59f:.48f));
            var width=scene.rect.width*(boat?.48f:.56f);rig.sizeDelta=new Vector2(width,width*prop.rect.height/prop.rect.width);
            if(boat)
            {
                var water=Group(scene,"BoatWaterWake",new Vector2(.66f,.31f),new Vector2(.66f,.31f));water.sizeDelta=new Vector2(width*1.12f,28);
                water.SetSiblingIndex(1);wake=water.gameObject.AddComponent<LeisureWaterWake>();wake.raycastTarget=false;
                SeatOrBoat=Group(rig,"BoatAndRider",Vector2.zero,Vector2.one);boatHome=SeatOrBoat.anchoredPosition;
                Layer(SeatOrBoat,"BoatBack",prop,false,Box(0,0,811,716));
                Rider=Group(SeatOrBoat,"ActivityLumi",new Vector2(423f/811,1-450f/716),new Vector2(423f/811,1-450f/716));
                Rider.sizeDelta=new Vector2(width*200/811,width*200/811*lumi.rect.height/lumi.rect.width);
                Layer(Rider,"CanonicalRider",lumi,false,Box(0,0,lumi.rect.width,lumi.rect.height));
                Layer(SeatOrBoat,"BoatFront",prop,false,Box(0,518,811,198),Polygon(490,516,777,355,811,402,811,716,490,716));
            }
            else
            {
                unit=width/768;
                var left=Box(262,170,48,370);var right=Box(490,172,49,366);
                var seat=Polygon(225,539,255,514,499,492,550,518,575,550,575,587,329,624,293,620,226,594);
                leftAnchor=new Vector2(284,174);rightAnchor=new Vector2(507,174);
                leftEnd=new Vector2(284,535);rightEnd=new Vector2(507,535);
                leftRope=Layer(rig,"LeftSuspendedRope",prop,false,left).rectTransform;RopePivot(leftRope,leftAnchor);leftHome=leftRope.anchoredPosition;
                rightRope=Layer(rig,"RightSuspendedRope",prop,false,right).rectTransform;RopePivot(rightRope,rightAnchor);rightHome=rightRope.anchoredPosition;
                SeatOrBoat=Group(rig,"SwingSeatAndRider",Vector2.zero,Vector2.one);seatHome=SeatOrBoat.anchoredPosition;
                Layer(SeatOrBoat,"SwingSeatBack",prop,false,seat);
                Rider=Group(SeatOrBoat,"ActivityLumi",new Vector2(401f/768,1-548f/818),new Vector2(401f/768,1-548f/818));
                // Full approved cutout; its trimmed sole baseline rests on the seat.
                Rider.pivot=new Vector2(.5f,0); // Approved white cutout has a trimmed sole baseline.
                Rider.sizeDelta=new Vector2(width*180/768,width*180/768*lumi.rect.height/lumi.rect.width);
                Layer(Rider,"CanonicalRider",lumi,false,Box(0,0,lumi.rect.width,lumi.rect.height));
                FixedFrame=Layer(rig,"FixedSwingSupport",prop,true,left,right,seat).rectTransform;
            }
            play.onClick.RemoveAllListeners();play.onClick.AddListener(()=>SoftAudio.Action(StartMotion));
            Pose(0);
        }
        public void StartMotion()
        {
            if(Playing)return;Elapsed=0;Playing=true;play.interactable=false;play.GetComponentInChildren<Text>().text="Đang trải nghiệm…";
            status.text=Boat?"Lumi ngồi trong thuyền, trôi nhẹ cùng nhịp nước.":"Giàn đứng yên · Ghế, dây treo và Lumi đung đưa cùng nhau.";Pose(0);
        }
        void Pose(float time)
        {
            if(Boat)
            {
                var t=Mathf.Clamp01(time/Duration);var drift=Mathf.SmoothStep(0,1,t);
                SeatOrBoat.anchoredPosition=boatHome+new Vector2((drift-.5f)*rig.rect.width*.15f,Mathf.Sin(time*3.3f)*1.7f);
                SeatOrBoat.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*2.1f)*.8f);
                if(wake!=null){wake.Phase=time;wake.SetVerticesDirty();}
                return;
            }
            var envelope=Mathf.SmoothStep(0,1,Mathf.Min(time/.22f,(Duration-time)/.22f));
            SwingAngle=9.5f*Mathf.Sin(time*Mathf.PI*2/2.2f)*envelope;
            var rad=SwingAngle*Mathf.Deg2Rad;var delta=new Vector2(Mathf.Sin(rad)*361,(1-Mathf.Cos(rad))*361);
            SeatOrBoat.anchoredPosition=seatHome+delta*unit;
            // Parallel suspension: each top end is fixed, with the same moving-seat endpoint delta.
            SetRope(leftRope,leftAnchor,leftEnd,delta,leftHome);SetRope(rightRope,rightAnchor,rightEnd,delta,rightHome);
        }
        static void SetRope(RectTransform rope,Vector2 anchor,Vector2 end,Vector2 delta,Vector2 home)
        {
            var rest=new Vector2(end.x-anchor.x,anchor.y-end.y);var now=rest+delta;
            rope.anchoredPosition=home;rope.localEulerAngles=new Vector3(0,0,Vector2.SignedAngle(rest,now));
            rope.localScale=new Vector3(1,now.magnitude/rest.magnitude,1);
        }
        void Update()
        {
            if(!Playing)return;Elapsed=Mathf.Min(Duration,Elapsed+Time.unscaledDeltaTime);Pose(Elapsed);
            if(Elapsed<Duration)return;Playing=false;play.interactable=true;play.GetComponentInChildren<Text>().text="Chơi lại";
            status.text="Một khoảng nghỉ cùng Lumi · Chơi lại hoặc trở về bản đồ.";
        }
        void OnDisable(){Playing=false;}
    }

    // Pixel-preserving UI mesh split. Regions use original top-left source pixels.
    // No PNG crops, generated replacement textures, material instances or per-frame texture allocations.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LeisureSpriteLayer:MaskableGraphic
    {
        // Optional prop-only perspective correction. Zero preserves both pilots.
        public float ShearY;
        public float VerticalScale=1;
        Sprite source;readonly List<List<Vector2>> pieces=new List<List<Vector2>>();
        public override Texture mainTexture=>source!=null?source.texture:Texture2D.whiteTexture;
        public void Configure(Sprite sprite,bool outside,Vector2[][] regions)
        {
            source=sprite;raycastTarget=false;
            var pending=new List<List<Vector2>>{new List<Vector2>{Vector2.zero,Vector2.right,Vector2.one,Vector2.up}};
            pieces.Clear();
            foreach(var region in regions)
            {
                var poly=new List<Vector2>();foreach(var p in region)poly.Add(new Vector2(p.x/source.rect.width,1-p.y/source.rect.height));
                float area=0;for(var i=0;i<poly.Count;i++)area+=Cross(poly[i],poly[(i+1)%poly.Count]);if(area<0)poly.Reverse();
                var rest=new List<List<Vector2>>();
                foreach(var original in pending)
                {
                    var inside=original;
                    for(var i=0;i<poly.Count&&inside.Count>=3;i++)
                    {
                        var a=poly[i];var b=poly[(i+1)%poly.Count];var cut=Clip(inside,a,b,false);if(cut.Count>=3)rest.Add(cut);
                        inside=Clip(inside,a,b,true);
                    }
                    if(!outside&&inside.Count>=3)pieces.Add(inside);
                }
                pending=rest;
            }
            if(outside)pieces.AddRange(pending);SetAllDirty();
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static List<Vector2> Clip(List<Vector2> input,Vector2 a,Vector2 b,bool keepInside)
        {
            var output=new List<Vector2>();if(input.Count==0)return output;var prev=input[input.Count-1];var pd=Cross(b-a,prev-a)*(keepInside?1:-1);
            foreach(var next in input)
            {
                var nd=Cross(b-a,next-a)*(keepInside?1:-1);
                if((pd>=0)!=(nd>=0))output.Add(Vector2.LerpUnclamped(prev,next,pd/(pd-nd)));
                if(nd>=0)output.Add(next);prev=next;pd=nd;
            }
            return output;
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();if(source==null)return;var uv=DataUtility.GetOuterUV(source);var rect=rectTransform.rect;
            foreach(var p in pieces)
            {
                var start=vh.currentVertCount;
                foreach(var v in p){var x=rect.x+v.x*rect.width;vh.AddVert(new Vector3(x,(rect.y+v.y*rect.height)*VerticalScale+x*ShearY),color,new Vector2(Mathf.Lerp(uv.x,uv.z,v.x),Mathf.Lerp(uv.y,uv.w,v.y)));}
                for(var i=1;i<p.Count-1;i++)vh.AddTriangle(start,start+i,start+i+1);
            }
        }
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LeisureWaterWake:MaskableGraphic
    {
        public float Phase;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var r=rectTransform.rect;
            for(var ring=0;ring<3;ring++)for(var j=0;j<28;j++)
            {
                var a=(j/28f*Mathf.PI*2);var b=((j+1)/28f*Mathf.PI*2);var w=r.width*(.33f+ring*.075f)+Mathf.Sin(Phase*3-ring)*2;var h=4+ring*3;
                var p=new Vector2(Mathf.Cos(a)*w,Mathf.Sin(a)*h);var q=new Vector2(Mathf.Cos(b)*w,Mathf.Sin(b)*h);
                var c=new Color(.83f,.96f,1,.18f+Mathf.Sin(Phase*3+ring)*.04f);var k=vh.currentVertCount;
                vh.AddVert(p+Vector2.up*.65f,c,Vector2.zero);vh.AddVert(p-Vector2.up*.65f,c,Vector2.zero);vh.AddVert(q-Vector2.up*.65f,c,Vector2.zero);vh.AddVert(q+Vector2.up*.65f,c,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
            }
        }
    }
}
