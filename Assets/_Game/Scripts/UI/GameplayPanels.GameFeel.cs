using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplayPanels
    {
        string BattleCounterName()
        {
            var b=game.Battles;var skills=b.Enemy.skills;
            return skills!=null&&skills.Length>0?skills[(b.TurnNumber-1)%skills.Length]:"Tấn công";
        }
        void BattleResponse(string action,int playerHP,int enemyHP,int phase)
        {
            var sheet=overlay?.Find("BattlePanel");if(sheet==null)return;
            var b=game.Battles;var damage=Mathf.Max(0,enemyHP-b.EnemyHP);
            var enemyText=b.Phase!=phase?"PHA 2 · Lõi đã lộ":damage>0?"−"+damage+" HP":action=="attack"||action=="light"?"Khiên đang đóng":"";
            var playerText=b.Outcome=="loss"?"Hồi sức · Có thể thử lại":action=="defend"?"Che chắn":action=="repair"?"Sửa mạch "+b.RepairProgress+"/3":action=="heal"?"Đã uống trà":playerHP>b.PlayerHP?"−"+(playerHP-b.PlayerHP)+" HP":"";
            if(!string.IsNullOrEmpty(enemyText))
            {
                var label=ui.Label(sheet,"BattleEnemyResponse",enemyText,new Vector2(.52f,.56f),new Vector2(.97f,.64f),20);label.color=DesignKitArt.Gold;label.raycastTarget=false;
                label.gameObject.AddComponent<BattleResponseMotion>().Configure(sheet.Find("BattleEnemy").GetComponent<RectTransform>(),damage>0?1:0);
            }
            if(!string.IsNullOrEmpty(playerText))
            {
                var label=ui.Label(sheet,"BattlePlayerResponse",playerText,new Vector2(.03f,.56f),new Vector2(.48f,.64f),19);label.color=DesignKitArt.Cream;label.raycastTarget=false;
                label.gameObject.AddComponent<BattleResponseMotion>().Configure(sheet.Find("BattleLumi").GetComponent<RectTransform>(),action=="defend"||action=="heal"?0:-1);
            }
        }
    }
    // Presentation beat only; the next action is available immediately.
    public sealed class BattleResponseMotion:MonoBehaviour
    {
        RectTransform subject,label;Vector2 home,labelHome;float age;int direction;Color color;
        public void Configure(RectTransform target,int nudge)
        {subject=target;home=target.anchoredPosition;direction=nudge;label=(RectTransform)transform;labelHome=label.anchoredPosition;color=GetComponent<Text>().color;}
        void Update()
        {
            age+=Time.unscaledDeltaTime;var t=Mathf.Clamp01(age/.55f);var mode=NodeMarkerView.MotionMode;
            if(subject!=null)subject.anchoredPosition=home+(mode==MarkerMotionMode.Off?Vector2.zero:Vector2.right*Mathf.Sin(t*Mathf.PI*4)*(1-t)*direction*(mode==MarkerMotionMode.Reduced?2:7));
            label.anchoredPosition=labelHome+(mode==MarkerMotionMode.Off?Vector2.zero:Vector2.up*t*9);
            GetComponent<Text>().color=new Color(color.r,color.g,color.b,color.a*Mathf.Clamp01((1-t)*3));
            if(t>=1){if(subject!=null)subject.anchoredPosition=home;Destroy(gameObject);}
        }
        void OnDisable(){if(subject!=null)subject.anchoredPosition=home;}
    }
}
