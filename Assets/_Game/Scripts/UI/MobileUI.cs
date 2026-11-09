using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public static class MobileUI
    {
        // One meaning: dismiss this view. Never submit an interaction or grant rewards.
        public static Button CloseButton(UIController ui,Transform parent,string name,UnityAction close)
        {
            var b=Button(ui,parent,name,"",Vector2.one,Vector2.one,close);
            var rt=(RectTransform)b.transform;rt.pivot=Vector2.one;rt.sizeDelta=new Vector2(52,52);rt.anchoredPosition=new Vector2(-12,-10);
            if(b.targetGraphic is SoftButtonGraphic soft)soft.Face=DesignKitArt.Hex("B6413D");
            else b.targetGraphic.color=DesignKitArt.Hex("B6413D");
            Symbol(b.transform,"CloseSymbol",AdventureSymbol.Close,new Vector2(.25f,.25f),new Vector2(.75f,.75f),Color.white);
            return b;
        }
        public static AdventureIconGraphic Symbol(Transform parent,string name,AdventureSymbol symbol,Vector2 min,Vector2 max,Color? color=null)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(AdventureIconGraphic));
            go.transform.SetParent(parent,false); UIController.SetRect((RectTransform)go.transform,min,max);
            var icon=go.GetComponent<AdventureIconGraphic>();icon.symbol=symbol;icon.color=color??DesignKitArt.Cream;icon.raycastTarget=false;
            return icon;
        }
        public static Button IconButton(UIController ui,Transform parent,string name,string caption,Vector2 min,Vector2 max,UnityAction click,AdventureSymbol? symbol=null,Sprite sprite=null)
        {
            var button=Button(ui,parent,name,caption,min,max,click,illustrated:true);
            Skin((Image)button.targetGraphic,AdventureUIArt.Load().slot,5);
            var label=button.transform.Find("Label").GetComponent<Text>();
            UIController.SetRect(label.rectTransform,new Vector2(.02f,.02f),new Vector2(.98f,.39f)); label.resizeTextForBestFit=false;label.fontSize=14;label.color=DesignKitArt.Cream;
            if(string.IsNullOrEmpty(caption))label.gameObject.SetActive(false);
            var skin=AdventureUIArt.Load();
            if(sprite==null&&symbol==AdventureSymbol.Globe)sprite=skin.globe;
            if(sprite==null&&symbol==AdventureSymbol.Journal)sprite=skin.journal;
            if(sprite!=null)ui.Picture(button.transform,"Icon",sprite,new Vector2(.13f,.40f),new Vector2(.87f,.97f));
            else if(symbol.HasValue)Symbol(button.transform,"Icon",symbol.Value,new Vector2(.18f,.30f),new Vector2(.82f,.94f),label.color);
            if(string.IsNullOrEmpty(caption)&&button.transform.Find("Icon")!=null)UIController.SetRect(button.transform.Find("Icon").GetComponent<RectTransform>(),new Vector2(.16f,.16f),new Vector2(.84f,.84f));
            return button;
        }
        public static void Cue(Button button,AdventureSymbol symbol)
        {
            Symbol(button.transform,"ActionIcon",symbol,new Vector2(.035f,.22f),new Vector2(.13f,.78f),button.GetComponentInChildren<Text>().color);
            UIController.SetRect(button.transform.Find("Label").GetComponent<RectTransform>(),new Vector2(.15f,.06f),new Vector2(.95f,.94f));
        }
        public static RectTransform Frame(UIController ui,Transform parent,string name,Vector2 min,Vector2 max,Sprite sprite,float borderScale=2)
        {
            var panel=ui.Panel(parent,name,min,max,Color.white);Skin(panel.GetComponent<Image>(),sprite,borderScale);return panel;
        }
        public static void Skin(Image image,Sprite sprite,float borderScale=2)
        {
            image.sprite=sprite;image.type=Image.Type.Sliced;image.preserveAspect=false;image.pixelsPerUnitMultiplier=borderScale;
            if(image is SoftButtonGraphic soft)
            {
                var art=AdventureUIArt.Load();
                soft.PreserveSprite=sprite!=art.buttonTeal&&sprite!=art.buttonPaper&&sprite!=art.slot;
                if(!soft.PreserveSprite)soft.Face=DesignKitArt.Hex(sprite==art.buttonPaper?"F1D49B":sprite==art.slot?"315F62":"2C706C");
            }
        }
        public static SoftButtonGraphic Style(Button button,Color face)
        {
            var outline=button.GetComponent<Outline>();if(outline!=null)outline.enabled=false;
            var hit=button.GetComponent<Image>();hit.enabled=true;hit.color=Color.clear;hit.raycastTarget=true;
            var child=button.transform.Find("RoundedBackground");
            if(child==null){child=new GameObject("RoundedBackground",typeof(RectTransform),typeof(SoftButtonGraphic)).transform;child.SetParent(button.transform,false);child.SetAsFirstSibling();UIController.SetRect((RectTransform)child,Vector2.zero,Vector2.one);}
            var surface=child.GetComponent<SoftButtonGraphic>();surface.Face=face;surface.raycastTarget=false;button.targetGraphic=surface;
            if(button.GetComponent<UIButtonFeedback>()==null)button.gameObject.AddComponent<UIButtonFeedback>();
            return surface;
        }
        public static Button Button(UIController ui,Transform parent,string name,string text,Vector2 min,Vector2 max,UnityAction click,bool primary=false,bool illustrated=false)
        {
            var b=ui.Button(parent,name,text,min,max,click);
            var graphic=Style(b,DesignKitArt.Hex(primary?"EBC680":"28545C"));
            if(illustrated)Skin(graphic,primary?AdventureUIArt.Load().buttonTeal:AdventureUIArt.Load().buttonPaper,3.5f);
            var label=b.GetComponentInChildren<Text>(); label.color=illustrated?(primary?DesignKitArt.Cream:DesignKitArt.Ink):(primary?DesignKitArt.Ink:DesignKitArt.Cream);
            label.resizeTextMaxSize=17;
            if(illustrated){label.resizeTextForBestFit=false;label.fontSize=17;}
            return b;
        }
        public static void Round(RectTransform panel)
        {
            var image=panel.GetComponent<Image>(); var color=image.color; image.color=Color.clear;
            var shape=new GameObject("RoundedBackground",typeof(RectTransform),typeof(RoundedPanelGraphic));
            shape.transform.SetParent(panel,false); shape.transform.SetAsFirstSibling();
            UIController.SetRect((RectTransform)shape.transform,Vector2.zero,Vector2.one);
            var graphic=shape.GetComponent<RoundedPanelGraphic>(); graphic.color=color; graphic.raycastTarget=false;
        }
    }
}
