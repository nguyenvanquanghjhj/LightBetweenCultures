using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        void ShowLeisure(string id)
        {
            var entry=LeisurePresentation.Load().entries.FirstOrDefault(e=>e.id==id&&e.enabled);
            if(entry==null||entry.cityId!=game.Data.cityId||entry.nodeId!=game.Data.nodes[game.Save.node].id||!game.Discovery.IsVisible(game.Save.node))return;
            if(entry.id=="leisure_hoian_swing"||entry.id=="leisure_hue_pedalswanboat"){ShowLeisurePilot(entry);return;}
            var bg=AdventureUIArt.Load().LeisureBackground(id);if(bg==null||entry.Sprite==null)return;
            var sheet=ActivitySheet("LeisureActivityPanel",entry.title,"Một khoảng nghỉ cùng Lumi · Bắt đầu để trải nghiệm, có thể trở về bản đồ bất cứ lúc nào.");
            UIController.SetRect(sheet.Find("ActivityInstructions").GetComponent<RectTransform>(),new Vector2(.07f,.79f),new Vector2(.93f,.89f));
            var scene=ui.Panel(sheet,"LeisureScene",new Vector2(.5f,.54f),new Vector2(.5f,.54f),Color.clear);
            Canvas.ForceUpdateCanvases();var width=sheet.rect.width*.89f;scene.sizeDelta=new Vector2(width,width*.75f);
            scene.gameObject.AddComponent<RectMask2D>();scene.GetComponent<Image>().raycastTarget=false;ui.Picture(scene,"LeisureBackground",bg,Vector2.zero,Vector2.one);
            var status=ui.Label(sheet,"LeisureStatus","Sẵn sàng · Hoạt động thư giãn, không có phần thưởng.",new Vector2(.07f,.205f),new Vector2(.93f,.29f),17);status.color=DesignKitArt.Teal;
            var play=MobileUI.Button(ui,sheet,"LeisureAction","Bắt đầu",new Vector2(.07f,.105f),new Vector2(.47f,.18f),()=>{},true,true);
            var back=sheet.Find("ActivityClose").GetComponent<RectTransform>();UIController.SetRect(back,new Vector2(.53f,.105f),new Vector2(.93f,.18f));back.GetComponentInChildren<Text>().text="Trở về bản đồ";
            sheet.gameObject.AddComponent<LeisureActivityView>().Build(ui,scene,entry,art.lumiExploration,play,status);
            LeisureHelpAndProgress(sheet,entry);
        }
    }
    // Existing transient optional-panel driver. No save/quest/item/reward authority.
    // Texture splitting uses the shared helper from the two unchanged pilots.
    public sealed partial class LeisureActivityView:MonoBehaviour
    {
        public bool Playing {get;private set;}
        public float Elapsed {get;private set;}
        public const float Duration=4.8f;
        public RectTransform Rider=>actor;
        public RectTransform Backdrop {get;private set;}
        public RectTransform MainProp=>prop;
        public string Mechanic=>definition.mechanic;
        UIController ui;RectTransform scene,prop,actor,hand,tip,tool,extra,footL,footR,mouth;
        LeisureDefinition definition;Sprite sprite,lumi;Button play;Text status,count;
        Vector2 actorHome,toolHome,toolTarget;Vector3 toolScale;
        readonly List<RectTransform> particles=new List<RectTransform>(),fruit=new List<RectTransform>();
        readonly List<Vector2> fruitHome=new List<Vector2>();
        LeisureWaterWake wake;float w,h,unit,propShear,propVerticalScale=1;
        static Vector2[] Box(float x,float y,float width,float height)=>new[]{new Vector2(x,y),new Vector2(x+width,y),new Vector2(x+width,y+height),new Vector2(x,y+height)};
        static Vector2[] Poly(params float[] v){var a=new Vector2[v.Length/2];for(var i=0;i<a.Length;i++)a[i]=new Vector2(v[i*2],v[i*2+1]);return a;}
        static RectTransform Group(Transform parent,string name,Vector2 center,Vector2 size)
        {var rt=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rt.SetParent(parent,false);UIController.SetRect(rt,center,center);rt.sizeDelta=size;return rt;}
        LeisureSpriteLayer Layer(Transform parent,string name,Sprite source,bool outside,params Vector2[][] regions)
        {var rt=Group(parent,name,new Vector2(.5f,.5f),Vector2.zero);UIController.SetRect(rt,Vector2.zero,Vector2.one);var layer=rt.gameObject.AddComponent<LeisureSpriteLayer>();if(parent==prop){layer.ShearY=propShear;layer.VerticalScale=propVerticalScale;}layer.Configure(source,outside,regions);return layer;}
        void Prop(Vector2 at,float scale,float shear=0,float verticalScale=1)
        {propShear=shear;propVerticalScale=verticalScale;var width=w*scale;unit=width/sprite.rect.width;prop=Group(scene,"ActivityProp",at,new Vector2(width,width*sprite.rect.height/sprite.rect.width));}
        Vector2[] LumiRegion(Vector2[] pixels)
        {var origin=new Vector2(lumi.rect.x,lumi.texture.height-lumi.rect.yMax);return pixels.Select(p=>p-origin).ToArray();}
        RectTransform FullPart(string name,Vector2 pivot,params Vector2[][] regions)
        {
            var layer=Layer(prop,name,sprite,false,regions).rectTransform;layer.anchorMin=layer.anchorMax=new Vector2(.5f,.5f);layer.sizeDelta=prop.sizeDelta;
            layer.pivot=new Vector2(pivot.x/sprite.rect.width,1-pivot.y/sprite.rect.height);
            layer.anchoredPosition=new Vector2(pivot.x-sprite.rect.width/2,sprite.rect.height/2-pivot.y)*unit;return layer;
        }
        RectTransform Crop(Transform parent,string name,Sprite source,Rect area,float width,Vector2 at,params Vector2[][] regions)
        {
            var rt=Group(parent,name,at,new Vector2(width,width*area.height/area.width));var scale=width/area.width;
            var image=Group(rt,name+"Pixels",new Vector2(.5f,.5f),new Vector2(source.rect.width*scale,source.rect.height*scale));
            image.anchoredPosition=new Vector2(source.rect.width/2-area.center.x,area.center.y-source.rect.height/2)*scale;
            image.gameObject.AddComponent<LeisureSpriteLayer>().Configure(source,false,regions.Length>0?regions:new[]{Box(area.x,area.y,area.width,area.height)});return rt;
        }
        void Actor(Vector2 at,float scale,bool seated=false,bool splitFeet=false)
        {
            actor=Group(scene,"ActivityLumi",at,new Vector2(w*scale,w*scale*lumi.rect.height/lumi.rect.width));actor.pivot=new Vector2(.5f,0);
            // Masks are authored in source PNG pixels, while Unity uses a trimmed
            // canonical Sprite.rect. Never discard the lower body as a pose cheat.
            // Pixel landmarks belong to the approved MapTraveler pose.
            var arm=LumiRegion(Poly(973,741,1003,754,1034,783,1050,811,1035,836,1011,847,977,834,958,804,953,771));var regions=new List<Vector2[]>{arm};
            var leftFoot=LumiRegion(Poly(455,1031,500,1029,555,1042,588,1065,602,1097,580,1128,472,1135,438,1104,439,1067));
            var rightFoot=LumiRegion(Poly(761,1030,800,1032,847,1054,881,1081,882,1106,860,1127,765,1128,731,1107,733,1072));
            if(splitFeet){regions.Add(leftFoot);regions.Add(rightFoot);}
            hand=Layer(actor,"LumiRightWing",lumi,false,arm).rectTransform;hand.anchorMin=hand.anchorMax=new Vector2(.5f,.5f);hand.sizeDelta=actor.sizeDelta;
            var origin=new Vector2(lumi.rect.x,lumi.texture.height-lumi.rect.yMax);var joint=new Vector2(978,760)-origin;
            hand.pivot=new Vector2(joint.x/lumi.rect.width,1-joint.y/lumi.rect.height);
            hand.anchoredPosition=new Vector2(joint.x-lumi.rect.width/2,lumi.rect.height/2-joint.y)*(actor.sizeDelta.x/lumi.rect.width);
            Layer(actor,"CanonicalLumiBody",lumi,true,regions.ToArray());
            var end=new Vector2(1029,820)-origin;tip=Group(hand,"LumiHandAnchor",new Vector2(end.x/lumi.rect.width,1-end.y/lumi.rect.height),Vector2.zero);
            if(splitFeet){footL=Layer(actor,"LumiLeftFoot",lumi,false,leftFoot).rectTransform;footR=Layer(actor,"LumiRightFoot",lumi,false,rightFoot).rectTransform;}
        }
        void ActorInProp(Vector2 foot,float pixelWidth)
        {Actor(new Vector2(.5f,.5f),pixelWidth*unit/w);actor.pivot=new Vector2(.5f,0);actor.SetParent(prop,false);actor.anchorMin=actor.anchorMax=new Vector2(foot.x/sprite.rect.width,1-foot.y/sprite.rect.height);actor.anchoredPosition=Vector2.up*(((foot.x-sprite.rect.width/2)*propShear+(sprite.rect.height/2-foot.y)*(propVerticalScale-1))*unit);}
        void Wisps()
        {for(var i=0;i<3;i++){var p=ui.Panel(tool,"Steam"+i,new Vector2(.35f+i*.15f,.65f),new Vector2(.35f+i*.15f,.65f),new Color(1,.96f,.85f,.3f));p.sizeDelta=new Vector2(3,15);p.GetComponent<Image>().raycastTarget=false;MobileUI.Round(p);particles.Add(p);}}
        void BringToBeak(float reach,float tilt,float scale=1)
        {
            // Contact is defined against the canonical beak, not a guessed wing offset.
            // Keep the cup/food readable; do not shrink it away before reaching the mouth.
            tool.localEulerAngles=new Vector3(0,0,tilt);
            tool.localScale=toolScale*scale;
            var contact=definition.mechanic=="tea"?new Vector2(-.32f,.30f):Vector2.zero;
            var atMouth=mouth.position-tool.TransformVector(Vector2.Scale(contact,tool.rect.size));
            tool.position=Vector3.Lerp(tip.TransformPoint(toolHome),atMouth,reach);
        }
        public void Build(UIController controller,RectTransform viewport,LeisureDefinition entry,Sprite character,Button button,Text text)
        {
            ui=controller;scene=viewport;definition=entry;sprite=entry.Sprite;lumi=character;play=button;status=text;w=scene.rect.width;h=scene.rect.height;
            Backdrop=scene.Find("LeisureBackground").GetComponent<RectTransform>();var full=Box(0,0,sprite.rect.width,sprite.rect.height);
            switch(entry.mechanic)
            {
                case "release":
                    Prop(new Vector2(.23f,.32f),.34f);Layer(prop,"LanternStand",sprite,true,Box(0,140,18,160));Actor(new Vector2(.47f,.12f),.16f);
                    tool=Crop(scene,"ReleasedLotus",sprite,new Rect(238,444,130,100),w*.095f,new Vector2(.58f,.18f),Poly(241,457,273,440,332,449,366,485,345,529,283,539,238,507));
                    toolHome=tool.anchoredPosition;toolTarget=toolHome+new Vector2(w*.26f,h*.16f);
                    var water=Group(scene,"LotusRipples",new Vector2(.66f,.35f),new Vector2(w*.16f,16));wake=water.gameObject.AddComponent<LeisureWaterWake>();wake.raycastTarget=false;water.SetSiblingIndex(1);break;
                case "seat":
                    Prop(new Vector2(.51f,.44f),.40f,.11f);Layer(prop,"ThroneBack",sprite,true,Poly(142,710,167,702,187,711,195,723,176,736,140,743,135,732));ActorInProp(new Vector2(326,592),220);
                    Layer(prop,"ThroneFront",sprite,false,Poly(47,592,477,637,503,693,35,671));break;
                case "pick":
                    Prop(new Vector2(.65f,.34f),.55f);Layer(prop,"StrawberryBed",sprite,false,full);Actor(new Vector2(.28f,.14f),.19f);
                    var berries=new[]{Poly(144,306,154,300,180,300,196,309,204,320,205,333,196,349,183,361,165,371,155,371,147,358,140,337,136,319),Poly(315,269,328,263,345,264,362,272,371,284,370,299,360,318,351,333,343,337,332,331,318,313,309,295,306,280),Poly(520,217,532,212,549,215,562,223,567,234,563,247,552,261,543,266,535,261,524,248,517,233)};
                    foreach(var outline in berries)
                    {var a=Rect.MinMaxRect(outline.Min(p=>p.x),outline.Min(p=>p.y),outline.Max(p=>p.x),outline.Max(p=>p.y));var at=new Vector2(.65f+(a.center.x/sprite.rect.width-.5f)*.55f,.34f+(.5f-a.center.y/sprite.rect.height)*prop.rect.height/h);var f=Crop(scene,"PickedStrawberry"+fruit.Count,sprite,a,w*.055f,at,outline);fruitHome.Add(at);fruit.Add(f);}
                    var counter=ui.Panel(scene,"VisualPickCount",new Vector2(.035f,.84f),new Vector2(.20f,.96f),new Color(.10f,.24f,.22f,.8f));counter.GetComponent<Image>().raycastTarget=false;MobileUI.Round(counter);
                    count=ui.Label(counter,"LeisureVisualCount","0 / 3",Vector2.zero,Vector2.one,18);count.color=Color.white;break;
                case "fly":
                    Prop(new Vector2(.24f,.27f),.18f);Layer(prop,"KiteRack",sprite,false,full);Actor(new Vector2(.48f,.12f),.17f);
                    // Coordinates are relative to the imported, trimmed sprite (16,16
                    // in the PNG), not the original sheet. Preserve the complete red rim.
                    tool=Crop(scene,"FlyingKite",sprite,new Rect(132,26,442,464),w*.25f,new Vector2(.72f,.66f),Poly(244,29,573,156,475,484,132,294));toolHome=tool.anchoredPosition;
                    extra=Group(scene,"KiteString",new Vector2(.5f,.5f),Vector2.zero);extra.gameObject.AddComponent<LeisureThreadGraphic>().raycastTarget=false;extra.SetSiblingIndex(1);break;
                case "water":
                    Prop(new Vector2(.66f,.38f),.47f);
                    // Source can is embedded over the rock: keep the untouched garden behind
                    // Lumi, whose foreground body covers the original can. Extract only convex
                    // can parts for the hand; never clip a concave contour as a convex polygon.
                    Layer(prop,"GardenAndPots",sprite,true,Poly(700,610,794,585,794,724,700,724));Actor(new Vector2(.515f,.13f),.27f);
                    var canParts=new List<Vector2[]>{Poly(113,414,170,399,248,415,264,538,245,559,126,553,107,537,102,446),Poly(247,467,336,400,358,422,259,518),Poly(339,393,355,387,380,398,394,419,394,433,379,444,359,434,345,415)};
                    for(var i=0;i<12;i++){var a=Mathf.PI*i/12;var z=Mathf.PI*(i+1)/12;canParts.Add(Poly(175+76*Mathf.Cos(a),420-86*Mathf.Sin(a),175+76*Mathf.Cos(z),420-86*Mathf.Sin(z),175+57*Mathf.Cos(z),420-66*Mathf.Sin(z),175+57*Mathf.Cos(a),420-66*Mathf.Sin(a)));}
                    tool=Crop(tip,"HeldWateringCan",sprite,new Rect(98,331,303,238),w*.145f,new Vector2(.5f,.5f),canParts.ToArray());tool.anchoredPosition=new Vector2(15,17);toolHome=tool.anchoredPosition;
                    for(var i=0;i<6;i++){var p=ui.Panel(scene,"WaterDrop"+i,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Color(.63f,.87f,1,.8f));p.sizeDelta=new Vector2(3,7);p.GetComponent<Image>().raycastTarget=false;particles.Add(p);}break;
                case "tea":
                    // Remove the rejected shear: it steepened the receding edge.
                    // Flatten only the prop's perspective, never Lumi's body.
                    Prop(new Vector2(.60f,.315f),.53f,0,.85f);Layer(prop,"TeaTable",sprite,true,Box(0,0,92,470));ActorInProp(new Vector2(174,609),205);
                    Layer(prop,"SeatCushionFront",sprite,false,Poly(19,616,162,656,280,688,219,753,17,678));
                    tool=Crop(tip,"HeldTeaCup",sprite,new Rect(244,278,81,76),w*.065f,new Vector2(.5f,.5f),Poly(245,292,253,283,271,279,295,280,316,286,324,296,322,321,315,340,301,349,281,353,263,348,251,337,246,317));tool.anchoredPosition=new Vector2(-7,9);toolHome=tool.anchoredPosition;Wisps();break;
                case "bell":
                    Prop(new Vector2(.51f,.42f),.48f);var bell=Poly(189,702,213,453,259,344,348,302,427,337,491,470,510,710);
                    // Keep the original rounded end and upper rim; the old mask cut
                    // across the timber. The same contour removes it from the fixed layer.
                    var log=Poly(545,547,551,528,569,519,662,513,716,515,740,527,753,549,754,580,747,601,731,614,701,620,564,609,546,594);var ropes=Box(594,259,80,325);
                    Layer(prop,"BellSupport",sprite,true,bell,log,ropes);extra=FullPart("BellBody",new Vector2(345,312),bell);tool=FullPart("BellMallet",new Vector2(632,260),log,ropes);
                    Actor(new Vector2(.79f,.17f),.17f);actor.localScale=new Vector3(-1,1,1);break;
                case "wish":
                    Prop(new Vector2(.62f,.43f),.45f);var card=Poly(216,365,248,368,259,463,211,464,211,386);Layer(prop,"WishRack",sprite,true,card);Actor(new Vector2(.405f,.165f),.19f);
                    tool=Crop(scene,"WishCard",sprite,new Rect(210,364,50,102),w*.035f,new Vector2(.48f,.265f),card);toolHome=tool.anchoredPosition;
                    toolTarget=new Vector2((.62f-.48f)*w+(235-325)*unit,(.43f-.265f)*h+(360-415)*unit);break;
                case "snack":
                    Prop(new Vector2(.59f,.41f),.56f);Layer(prop,"FoodStall",sprite,false,full);Actor(new Vector2(.30f,.14f),.22f);
                    tool=Crop(tip,"RiceCake",sprite,new Rect(325,354,24,15),w*.032f,new Vector2(.5f,.5f),Poly(326,359,329,355,336,355,345,359,348,364,347,367,340,368,333,365));tool.anchoredPosition=new Vector2(-7,15);toolHome=tool.anchoredPosition;
                    for(var i=0;i<2;i++){var stick=ui.Panel(tool,"Chopstick"+i,new Vector2(.5f,.1f+i*.20f),new Vector2(.5f,.1f+i*.20f),new Color(.91f,.77f,.48f));stick.sizeDelta=new Vector2(23,2);stick.anchoredPosition=new Vector2(12,i*3);stick.localEulerAngles=new Vector3(0,0,-20);stick.GetComponent<Image>().raycastTarget=false;}Wisps();break;
                case "dance":
                    Prop(new Vector2(.52f,.45f),.55f);Layer(prop,"KpopStage",sprite,false,full);Actor(new Vector2(.52f,.30f),.20f,false,true);break;
            }
            if(entry.mechanic=="tea"||entry.mechanic=="snack")
            {
                var origin=new Vector2(lumi.rect.x,lumi.texture.height-lumi.rect.yMax);
                var beak=new Vector2(813,608)-origin;
                mouth=Group(actor,"LumiMouthAnchor",new Vector2(beak.x/lumi.rect.width,1-beak.y/lumi.rect.height),Vector2.zero);
                tool.SetParent(actor,true);
                Group(tool,"MouthContact",entry.mechanic=="tea"?new Vector2(.18f,.80f):new Vector2(.5f,.5f),Vector2.zero);
            }
            actorHome=actor.anchoredPosition;if(tool!=null)toolScale=tool.localScale;
            play.onClick.RemoveAllListeners();play.onClick.AddListener(()=>SoftAudio.Action(()=>{ResetAgencyReplay();Begin();}));Pose(0);ConfigureAgency();
        }
        void Begin()
        {
            if(Playing)return;Playing=true;Elapsed=0;play.interactable=false;play.GetComponentInChildren<Text>().text="Đang trải nghiệm…";
            status.text=definition.mechanic=="pick"?"Lumi chọn ba quả dâu chín cho giỏ nhỏ.":definition.mechanic=="water"?"Nghiêng bình, tưới nhẹ bên góc vườn hanok.":definition.mechanic=="wish"?"Gửi một lời ước lên giá thẻ.":"Một nhịp nghỉ cùng Lumi…";Pose(0);
            if(definition.mechanic=="release")status.text="Hoa đăng đang trôi theo hướng bạn vừa chọn.";
            if(definition.mechanic=="bell")status.text=BellStrength>.9f?"Một nhịp chuông rộng từ tay Lumi.":"Một nhịp chuông nhẹ từ tay Lumi.";
        }
        void Pose(float time)
        {
            var t=Mathf.Clamp01(time/Duration);var wave=Mathf.Sin(time*Mathf.PI*2);var gentle=Mathf.Sin(time*Mathf.PI);var active=time>0;
            actor.anchoredPosition=actorHome;actor.localEulerAngles=Vector3.zero;hand.localEulerAngles=Vector3.zero;
            switch(definition.mechanic)
            {
                case "release":
                    var drift=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.12f)/.88f));tool.anchoredPosition=Vector2.Lerp(toolHome,toolTarget,drift)+Vector2.up*Mathf.Sin(time*3)*1.5f;tool.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*2)*3);
                    actor.anchoredPosition=actorHome+Vector2.right*Mathf.Sin(t*Mathf.PI)*7;hand.localEulerAngles=new Vector3(0,0,Mathf.Sin(t*Mathf.PI)*28);if(wake!=null){wake.Phase=time;wake.SetVerticesDirty();}break;
                case "seat":actor.anchoredPosition=actorHome+Vector2.up*gentle*1.5f;actor.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*1.8f)*1.5f);hand.localEulerAngles=new Vector3(0,0,gentle*4);break;
                case "pick":
                    count.text=Mathf.Clamp(Mathf.FloorToInt(t*3.2f),0,3)+" / 3";actor.anchoredPosition=actorHome+new Vector2(Mathf.Sin(t*Mathf.PI)*w*.12f,Mathf.Sin(t*Mathf.PI)*h*.035f);actor.localEulerAngles=new Vector3(0,0,-Mathf.Abs(wave)*5);hand.localEulerAngles=new Vector3(0,0,Mathf.Abs(wave)*48);
                    var basket=new Vector2(.65f+(605f/753-.5f)*.55f,.34f+(.5f-490f/669)*prop.rect.height/h);
                    for(var i=0;i<fruit.Count;i++){var f=fruit[i];var q=Mathf.Clamp01((t-i*.27f)/.24f);f.gameObject.SetActive(active&&q>0&&q<1);f.anchorMin=f.anchorMax=fruitHome[i];f.anchoredPosition=Vector2.Scale(basket-fruitHome[i],new Vector2(w,h))*q+Vector2.up*Mathf.Sin(q*Mathf.PI)*25;f.localScale=Vector3.one*Mathf.Lerp(1,.65f,q);}break;
                case "fly":
                    tool.anchoredPosition=toolHome+new Vector2(Mathf.Sin(time*1.7f)*w*.045f,Mathf.Sin(time*1.1f)*h*.04f);tool.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*1.7f)*8);actor.anchoredPosition=actorHome+Vector2.right*Mathf.Sin(time*1.7f)*3;hand.localEulerAngles=new Vector3(0,0,22+Mathf.Sin(time*1.7f)*8);
                    var thread=extra.GetComponent<LeisureThreadGraphic>();thread.Start=extra.InverseTransformPoint(tip.position);thread.End=extra.InverseTransformPoint(tool.position);thread.SetVerticesDirty();break;
                case "water":
                    var pour=Mathf.Sin(t*Mathf.PI);hand.localEulerAngles=new Vector3(0,0,20+pour*18);actor.localEulerAngles=new Vector3(0,0,-pour*4);actor.anchoredPosition=actorHome+Vector2.right*pour*5;tool.localEulerAngles=new Vector3(0,0,-pour*90);tool.anchoredPosition=toolHome;
                    var spout=(Vector2)scene.InverseTransformPoint(tool.TransformPoint(new Vector2(tool.rect.width*.42f,tool.rect.height*.15f)));
                    for(var i=0;i<particles.Count;i++){var drop=particles[i];var q=Mathf.Repeat(time*1.8f+i*.17f,1);drop.gameObject.SetActive(active&&pour>.3f);drop.anchoredPosition=spout+new Vector2(q*9,-q*26);}break;
                case "tea":var sip=Mathf.SmoothStep(0,1,Mathf.Pow(Mathf.Sin(t*Mathf.PI),2));hand.localEulerAngles=new Vector3(0,0,-sip*115);actor.anchoredPosition=actorHome+Vector2.up*gentle;BringToBeak(sip,-sip*12);Steam(time);break;
                case "bell":tool.localEulerAngles=new Vector3(0,0,-Mathf.Pow(Mathf.Max(0,Mathf.Sin(time*3)),2)*14*BellStrength);extra.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*9)*1.3f*Mathf.Abs(Mathf.Sin(time*3))*BellStrength);actor.anchoredPosition=actorHome+Vector2.left*Mathf.Abs(Mathf.Sin(time*3))*6;hand.localEulerAngles=new Vector3(0,0,Mathf.Abs(Mathf.Sin(time*3))*30);break;
                case "wish":var hang=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.10f)/.48f));tool.anchoredPosition=Vector2.Lerp(wishFrom,toolTarget,hang);tool.localEulerAngles=new Vector3(0,0,hang>.99f?Mathf.Sin(time*4)*4:0);actor.anchoredPosition=actorHome+Vector2.right*Mathf.Sin(t*Mathf.PI)*8;hand.localEulerAngles=new Vector3(0,0,Mathf.Sin(t*Mathf.PI)*35);break;
                case "snack":var eat=Mathf.SmoothStep(0,1,Mathf.Pow(Mathf.Max(0,Mathf.Sin(time*2.6f)),2));hand.localEulerAngles=new Vector3(0,0,-eat*115);actor.anchoredPosition=actorHome+Vector2.up*eat*2;actor.localEulerAngles=new Vector3(0,0,-eat*2);BringToBeak(eat,0,Mathf.Lerp(1,.80f,eat));Steam(time);break;
                case "dance":
                    var envelope=Mathf.SmoothStep(0,1,Mathf.Min(time/.25f,(Duration-time)/.25f));actor.anchoredPosition=actorHome+new Vector2(Mathf.Sin(time*5)*14,Mathf.Abs(Mathf.Sin(time*5))*8)*envelope;actor.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*5)*5*envelope);hand.localEulerAngles=new Vector3(0,0,Mathf.Sin(time*5)*24*envelope);footL.anchoredPosition=Vector2.up*Mathf.Max(0,Mathf.Sin(time*5))*4*envelope;footR.anchoredPosition=Vector2.up*Mathf.Max(0,-Mathf.Sin(time*5))*4*envelope;break;
            }
        }
        void Steam(float time)
        {for(var i=0;i<particles.Count;i++){var q=Mathf.Repeat(time*.5f+i*.33f,1);particles[i].anchoredPosition=new Vector2(Mathf.Sin(time*2+i)*3,q*19);particles[i].GetComponent<Image>().color=new Color(1,.95f,.8f,(1-q)*.35f);}}
        void Update()
        {if(!Playing)return;Elapsed=Mathf.Min(Duration,Elapsed+Time.unscaledDeltaTime);Pose(Elapsed);if(Elapsed<Duration)return;Playing=false;play.interactable=true;play.GetComponentInChildren<Text>().text="Chơi lại";status.text="Một khoảng nghỉ cùng Lumi · Chơi lại hoặc trở về bản đồ.";}
        void OnDisable(){Playing=false;}
    }
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class LeisureThreadGraphic:MaskableGraphic
    {
        public Vector2 Start,End;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();var before=Start;
            for(var i=1;i<=18;i++){var t=i/18f;var next=Vector2.Lerp(Start,End,t)+Vector2.down*(Mathf.Sin(t*Mathf.PI)*4);var normal=(next-before).normalized;normal=new Vector2(-normal.y,normal.x)*.55f;var n=vh.currentVertCount;var c=new Color(1,.94f,.73f,.7f);vh.AddVert(before+normal,c,Vector2.zero);vh.AddVert(before-normal,c,Vector2.zero);vh.AddVert(next-normal,c,Vector2.zero);vh.AddVert(next+normal,c,Vector2.zero);vh.AddTriangle(n,n+1,n+2);vh.AddTriangle(n,n+2,n+3);before=next;}
        }
    }
}
