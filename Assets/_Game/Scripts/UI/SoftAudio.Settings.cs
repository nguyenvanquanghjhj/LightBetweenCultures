using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class SoftAudio
    {
        public void AttachSettings(UIController ui)
        {
            var b=MobileUI.Button(ui,ui.Root,"AudioSettings","♫",new Vector2(0,1),new Vector2(0,1),()=>ShowSettings(ui),illustrated:true);
            var r=(RectTransform)b.transform;r.offsetMin=new Vector2(10,-126);r.offsetMax=new Vector2(66,-70);
        }
        void ShowSettings(UIController ui)
        {
            if(ui.Root.Find("AudioSettingsOverlay")!=null)return;
            var overlay=ui.Panel(ui.Root,"AudioSettingsOverlay",Vector2.zero,Vector2.one,new Color(.02f,.06f,.08f,.7f));
            var sheet=MobileUI.Frame(ui,overlay,"AudioSettingsPanel",new Vector2(.08f,.26f),new Vector2(.92f,.78f),AdventureUIArt.Load().window,3);
            ui.Label(sheet,"AudioTitle","CÀI ĐẶT",new Vector2(.08f,.86f),new Vector2(.92f,.97f),23).color=DesignKitArt.Ink;
            AddSlider(ui,sheet,"SfxVolume","Hiệu ứng",.62f,SfxVolume,v=>SetVolumes(v,MusicVolume));
            AddSlider(ui,sheet,"MusicVolume","Nhạc nền",.40f,MusicVolume,v=>SetVolumes(SfxVolume,v));
            Button frames=null;frames=MobileUI.Button(ui,sheet,"FrameRate","Độ mượt: "+FramePacing.Requested+" FPS",new Vector2(.08f,.19f),new Vector2(.92f,.31f),()=>{FramePacing.Toggle();frames.GetComponentInChildren<Text>().text="Độ mượt: "+FramePacing.Requested+" FPS";},illustrated:true);
            Button mute=null;mute=MobileUI.Button(ui,sheet,"AudioMute",Muted?"Bật âm thanh":"Tắt âm thanh",new Vector2(.08f,.05f),new Vector2(.53f,.16f),()=>{SetMuted(!Muted);mute.GetComponentInChildren<Text>().text=Muted?"Bật âm thanh":"Tắt âm thanh";SavePreferences();},illustrated:true);
            MobileUI.Button(ui,sheet,"AudioSettingsClose","Đóng",new Vector2(.57f,.05f),new Vector2(.92f,.16f),()=>{SavePreferences();overlay.gameObject.SetActive(false);Destroy(overlay.gameObject);},illustrated:true);
        }
        void AddSlider(UIController ui,RectTransform parent,string name,string caption,float y,float value,UnityEngine.Events.UnityAction<float> changed)
        {
            ui.Label(parent,name+"Label",caption,new Vector2(.08f,y+.08f),new Vector2(.92f,y+.20f),18).color=DesignKitArt.Ink;
            var track=ui.Panel(parent,name,new Vector2(.10f,y),new Vector2(.90f,y+.08f),DesignKitArt.Teal);
            var handle=ui.Panel(track,"Handle",new Vector2(0,0),new Vector2(0,1),DesignKitArt.Gold);handle.sizeDelta=new Vector2(24,0);
            var slider=track.gameObject.AddComponent<TouchSlider>();slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.minValue=0;slider.maxValue=1;slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(changed); // No click per slider sample.
        }
    }
}
