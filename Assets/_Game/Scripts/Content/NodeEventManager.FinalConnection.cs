using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace LightBetweenCultures
{
    // Extension of the existing dispatcher, not a second ending/save manager.
    public sealed partial class NodeEventManager
    {
        public const string CanonicalEndingFlag="game.canonical_ending_completed.v1";
        public const float CeremonyDuration=24f;
        public static readonly string[] FinalRelicIds={"relic_hoi_an_lantern","relic_hue_dragon_seal","relic_dalat_mist_compass","relic_jeonju_hanji_stamp","relic_gyeongju_star_lens","relic_seoul_trirelay_prism"};
        bool confirmationOpen;
        Scene ceremonyScene;
        float ceremonyStart=-1;
        float ceremonySeconds;
        int ceremonyFrame=-1;
        public bool BossResolved=>encounters.Resolved(game.Content.Event("seoul","enemy_final_core"));
        public int FinalRestoredCount=>FinalRelicIds.Count(id=>game.Relics.Definition(id)!=null&&game.Relics.Restored(id));
        public bool CanonicalEndingCompleted=>game.World.Has(CanonicalEndingFlag);
        public bool FinalConnectionReady=>BossResolved&&FinalRestoredCount==6&&game.Relics.AllRestored;
        bool AtFinal=>game.Data.cityId=="seoul"&&game.Save.node>=0&&game.Save.node<game.Data.nodes.Length&&game.Data.nodes[game.Save.node].id=="S21"&&game.Discovery.IsVisible(game.Save.node)&&SceneManager.GetActiveScene().name=="CityMap";
        bool SameScene=>ceremonyScene.IsValid()&&ceremonyScene==SceneManager.GetActiveScene();
        public bool CeremonyInProgress=>ceremonyStart>=0&&SameScene&&AtFinal&&!CanonicalEndingCompleted;
        public bool CanStartFinalCeremony=>AtFinal&&FinalConnectionReady&&!CanonicalEndingCompleted&&!CeremonyInProgress;
        public bool CanViewCanonicalEnding=>AtFinal&&FinalConnectionReady&&CanonicalEndingCompleted;
        public float CeremonyElapsed=>CeremonyInProgress?ceremonySeconds:0;
        public void TickFinalCeremony()
        {
            if(!CeremonyInProgress||ceremonyFrame==Time.frameCount)return;
            ceremonyFrame=Time.frameCount;
            // A background pause/hitch must not become an implicit Skip on resume.
            if(SoftAudio.Instance!=null&&SoftAudio.Instance.Suspended)return;
            ceremonySeconds+=Mathf.Min(Time.unscaledDeltaTime,.1f);
        }
        public string FinalProgressText=>"Di vật đã phục dựng: "+FinalRestoredCount+"/6 · Seoul: "+(BossResolved?"đã an toàn":"chưa yên")+"\nLễ Kết Nối: "+(CanonicalEndingCompleted?"đã hoàn thành":FinalConnectionReady?"sẵn sàng":"còn chờ những ánh sáng");
        public void CancelFinalCeremony(){confirmationOpen=false;ceremonyStart=-1;ceremonySeconds=0;ceremonyFrame=-1;ceremonyScene=default;}
        void BuildFinalOptions(EventView view,EventDefinition definition,bool available)
        {
            if(definition.id!="final_festival")return;
            view.speaker="Lumi";
            view.text=(CanonicalEndingCompleted?"Những câu chuyện đã tìm thấy nhau. Mình vẫn còn muốn trở lại chào những người bạn trên đường.":FinalConnectionReady?"Chúng ta đã đi một chặng đường thật dài. Từ những ngọn đèn Hội An đến ánh sáng Seoul… giờ là lúc kết nối tất cả.\n\nKết nối sáu miền ánh sáng đã sẵn sàng.":BossResolved?"Mọi người đã an toàn rồi… nhưng những ánh sáng chúng ta tìm thấy vẫn chưa thể hòa làm một. Có lẽ còn những câu chuyện đang chờ được lắng nghe.":"Những ánh sáng đã tìm về đây. Trước khi cùng thắp lên, mình phải giúp Seoul được bình yên.")+"\n\n"+FinalProgressText;
            if(BossResolved&&game.Quests.CanComplete("SQ03"))view.options.Add(new EventOption("report_victory","Báo tin bình an",available));
            if(CanStartFinalCeremony)view.options.Add(new EventOption("ceremony","Bắt đầu Lễ Kết Nối",available));
            if(CanViewCanonicalEnding)view.options.Add(new EventOption("ending","Nhớ lại Lễ Kết Nối",available));
            view.options.Add(new EventOption("final_collection","Xem lại sáu di vật",available));
            view.options.Add(new EventOption("ceremony_cancel","Để sau",true));
        }
        EventResult ExecuteFinal(int node,string action)
        {
            var result=new EventResult();
            if(!AtFinal||node!=game.Save.node)return result;
            switch(action)
            {
                case "report_victory":
                    if(!BossResolved||!game.Quests.CanComplete("SQ03"))return result;
                    result.changed=game.Quests.Complete("SQ03","S21");result.message="Seoul đã bình yên. Những cuộc hẹn còn lại vẫn đang chờ Lumi.";break;
                case "ceremony":
                    if(!CanStartFinalCeremony)return result;
                    confirmationOpen=true;ceremonyScene=SceneManager.GetActiveScene();break;
                case "ceremony_confirm":
                    if(!confirmationOpen||!SameScene||!CanStartFinalCeremony)return result;
                    // This deliberate S21 confirmation can also acknowledge the existing
                    // main quest. Its independent report action never requires relics.
                    if(game.Quests.CanComplete("SQ03"))game.Quests.Complete("SQ03","S21");
                    confirmationOpen=false;ceremonyScene=SceneManager.GetActiveScene();ceremonyStart=Time.realtimeSinceStartup;ceremonySeconds=0;ceremonyFrame=Time.frameCount;break;
                case "ceremony_finish":
                case "ceremony_skip":
                    if(!CeremonyInProgress||!FinalConnectionReady||(action=="ceremony_finish"&&CeremonyElapsed<CeremonyDuration))return result;
                    var oldFlags=game.Save.flags.ToArray();game.World.Set(CanonicalEndingFlag);
                    if(!game.Persist()){game.Save.flags=oldFlags.ToList();result.message="Ánh sáng chưa kịp ghi lại. Hãy thử kết nối thêm một lần.";return result;}
                    CancelFinalCeremony();result.changed=true;result.openEnding=true;break;
                case "ending":
                case "ending_next": // Legacy calls are presentation-only AFTER the new commit.
                    if(!CanViewCanonicalEnding)return result;
                    result.openEnding=true;break;
                case "ending_ack":
                    if(!CanViewCanonicalEnding)return result;
                    if(!game.World.Has("game.ending_seen"))
                    {
                        game.World.Set("game.ending_seen");
                        if(!game.Persist()){game.Save.flags.Remove("game.ending_seen");return result;}
                        result.changed=true;
                    }
                    break;
                case "final_collection":break;
                case "ceremony_cancel":CancelFinalCeremony();break;
                default:return result;
            }
            result.stop=true;return result;
        }
    }
}
