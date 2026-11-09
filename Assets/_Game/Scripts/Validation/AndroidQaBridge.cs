#if UNITY_ANDROID && LBC_ANDROID_QA
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Read-only telemetry. All gameplay input is delivered by Android's input
    // command, never by calling quest, puzzle, movement, combat or ending APIs.
    public sealed class AndroidQaBridge : MonoBehaviour
    {
        [Serializable] public class Control { public string id,path,text,kind; public float x,y,left,top,right,bottom; public bool enabled,hit; }
        [Serializable] public class Node { public string id,name; public bool visible,reachable; public Vector2 position; }
        [Serializable] public class Snapshot
        {
            public string version,scene,city,node,error,savePath,diskSave,diskHash,travel,edge,guidance,guidanceCity;
            public int protocol=2,guidanceTarget;
            public int androidUser,width,height,frame,stage,selected,pending;
            public bool moving,dialogue,inputEnabled,recoveryBlocked,battleActive,puzzleActive,ending;
            public Vector2 lumi,mapPosition,mapSize; public float zoom;
            public SaveData save;
            public string battleEnemy,battleOutcome,battleLog,shieldSymbol;
            public int enemyHP,enemyMaxHP,battlePhase,turn; public bool shielded,canRepair;
            public string puzzleId,puzzleLog; public int rotations,folds; public int[] rings;
            public StoryInteractionDefinition activityDefinition;
            public bool[] placed,inspected; public int[] turns,dials; public bool prepared,ready; public float progress;
            public List<Control> controls=new List<Control>();
            public List<Node> nodes=new List<Node>();
            public List<string> texts=new List<string>(),panels=new List<string>(),unlockedCities=new List<string>();
            public List<string> errors=new List<string>();
        }
        TcpListener listener; Thread server; volatile bool alive; volatile string snapshot="{\"error\":\"starting\"}";
        int user; float next; readonly Queue<string> errors=new Queue<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            using(var process=new AndroidJavaClass("android.os.Process"))
            {
                // Hard refusal in Owner or any user other than the explicitly
                // provisioned QA user. This separate package never hosts owner saves.
                if(process.CallStatic<int>("myUid")/100000!=11)return;
            }
            if(Application.identifier!="com.DefaultCompany.LightBetweenCultures.androidqa")return;
            new GameObject("AndroidQaReadOnlyBridge").AddComponent<AndroidQaBridge>();
        }
        void Awake()
        {
            DontDestroyOnLoad(gameObject); user=11; alive=true;
            Application.logMessageReceived+=OnLog;
            listener=new TcpListener(IPAddress.Loopback,38777);listener.Start(1);
            server=new Thread(Serve){IsBackground=true,Name="LBC QA read-only state"};server.Start();
        }
        void OnLog(string message,string trace,LogType type)
        {
            if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
            errors.Enqueue(message);while(errors.Count>12)errors.Dequeue();
        }
        void Serve()
        {
            while(alive)try
            {
                using(var client=listener.AcceptTcpClient())
                {client.SendTimeout=2000;var bytes=Encoding.UTF8.GetBytes(snapshot+"\n");client.GetStream().Write(bytes,0,bytes.Length);}
            }catch(SocketException){if(!alive)return;}catch(IOException){}
        }
        static string PathOf(Transform t){return t.parent==null?t.name:PathOf(t.parent)+"/"+t.name;}
        static Control Bounds(RectTransform rect,string kind)
        {
            var corners=new Vector3[4];rect.GetWorldCorners(corners);
            var canvas=rect.GetComponentInParent<Canvas>();var camera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
            var points=corners.Select(v=>RectTransformUtility.WorldToScreenPoint(camera,v)).ToArray();
            var l=points.Min(v=>v.x);var r=points.Max(v=>v.x);var b=points.Min(v=>v.y);var t=points.Max(v=>v.y);
            return new Control{id=rect.name,path=PathOf(rect),kind=kind,left=l,right=r,top=Screen.height-t,bottom=Screen.height-b,x=(l+r)/2,y=Screen.height-(b+t)/2};
        }
        static bool Hit(Control c,GameObject target)
        {
            if(EventSystem.current==null)return false;
            // Return an actually raycastable point rather than a guessed center.
            foreach(var fraction in new[]{new Vector2(.5f,.5f),new Vector2(.3f,.3f),new Vector2(.7f,.7f)})
            {
                var p=new Vector2(Mathf.Lerp(c.left,c.right,fraction.x),Screen.height-Mathf.Lerp(c.top,c.bottom,fraction.y));
                if(p.x<2||p.x>Screen.width-2||p.y<2||p.y>Screen.height-2)continue;
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=p},hits);
                if(hits.Count>0&&(hits[0].gameObject==target||hits[0].gameObject.transform.IsChildOf(target.transform)))
                {c.x=p.x;c.y=Screen.height-p.y;return true;}
            }
            return false;
        }
        void Update()
        {
            if(Time.unscaledTime<next)return;next=Time.unscaledTime+.15f;
            try {snapshot=JsonUtility.ToJson(Read());}catch(Exception e){snapshot=JsonUtility.ToJson(new Snapshot{error=e.ToString()});}
        }
        Snapshot Read()
        {
            var s=new Snapshot{version=Application.version,androidUser=user,width=Screen.width,height=Screen.height,scene=SceneManager.GetActiveScene().name,frame=Time.frameCount,errors=errors.ToList()};
            foreach(var button in FindObjectsByType<Button>(FindObjectsSortMode.None))
            {
                if(!button.gameObject.activeInHierarchy)continue;
                var c=Bounds((RectTransform)button.transform,"button");c.enabled=button.IsInteractable();c.hit=c.enabled&&Hit(c,button.gameObject);
                c.text=string.Join(" | ",button.GetComponentsInChildren<Text>().Select(t=>t.text));s.controls.Add(c);
            }
            foreach(var rect in FindObjectsByType<RectTransform>(FindObjectsSortMode.None))
            {
                if(!rect.gameObject.activeInHierarchy)continue;
                if(rect.name=="MapViewport"||rect.name.StartsWith("Micro")||rect.name.StartsWith("Relic")||rect.name=="MapGestureArea")
                {if(rect.GetComponent<Button>()==null){var c=Bounds(rect,"rect");c.enabled=true;c.hit=Hit(c,rect.gameObject);s.controls.Add(c);}}
                if(rect.name.EndsWith("Panel")||rect.name.EndsWith("Overlay")||rect.name=="ResetConfirmation")s.panels.Add(rect.name);
            }
            s.texts=FindObjectsByType<Text>(FindObjectsSortMode.None).Where(t=>t.gameObject.activeInHierarchy&&!string.IsNullOrEmpty(t.text)).Select(t=>t.name+": "+t.text).Take(160).ToList();
            var game=GameBootstrap.Instance;if(game==null)return s;
            s.save=game.Save;s.city=game.Data.cityId;s.node=game.Data.nodes[game.Save.node].id;
            s.stage=game.Discovery.Stage(s.city);s.recoveryBlocked=SaveSystem.RecoveryBlocked;
            s.guidance=game.Quests.Guidance();s.guidanceTarget=game.Quests.GuidanceTarget;s.guidanceCity=game.Quests.GuidanceCity;
            s.savePath=SaveSystem.PathName;
            if(File.Exists(s.savePath))
            {s.diskSave=File.ReadAllText(s.savePath);using(var sha=SHA256.Create())s.diskHash=BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s.diskSave))).Replace("-","");}
            foreach(var city in game.Content.cities)if(game.IsCityUnlocked(city.id))s.unlockedCities.Add(city.id);
            s.ending=game.World.Has("game.ending_seen");
            var map=FindFirstObjectByType<CityMap>();
            if(map!=null)
            {
                s.moving=map.Moving;s.dialogue=map.DialogueOpen;s.selected=map.SelectedNode;s.pending=map.PendingDestination;
                s.travel=map.ActiveTravelMode;s.edge=map.ActiveNavigationEdgeId;
                if(map.Marker!=null)s.lumi=map.Marker.anchorMin;
                if(map.View!=null&&map.View.Content!=null){s.inputEnabled=map.View.InputEnabled;s.zoom=map.View.Zoom;s.mapPosition=map.View.Content.anchoredPosition;s.mapSize=map.View.Content.rect.size;}
                for(var i=0;i<game.Data.nodes.Length;i++)
                {
                    var n=game.Data.nodes[i];s.nodes.Add(new Node{id=n.id,name=n.name,position=CityRouteGeometry.Arrival(n),visible=game.Discovery.IsVisible(i),reachable=game.Data.FindRoute(game.Save.node,i,game.Save)!=null});
                }
                foreach(var marker in map.NodeMarkers)
                {
                    var n=s.nodes.FirstOrDefault(n=>n.id==marker.NodeId);
                    if(marker.HitArea==null||n==null||!n.visible)continue;
                    var c=Bounds(marker.HitArea,"node");c.id="node:"+marker.NodeId;c.text=n.name;c.enabled=n.reachable&&s.inputEnabled;
                    c.hit=c.enabled&&Hit(c,marker.gameObject);s.controls.Add(c);
                }
            }
            var player=FindFirstObjectByType<HoiAn.HoiAnPlayerController>();
            var item=FindFirstObjectByType<HoiAn.HoiAnItem>();
            if(player!=null)
            {
                s.moving=player.Walking;s.lumi=player.Position;
                if(item!=null&&Camera.main!=null)
                {
                    var target=item.transform.position+Vector3.left*.8f;
                    var point=Camera.main.WorldToScreenPoint(target);
                    var valid=HoiAn.HoiAnPlayerController.FindPath(player.Position,target)!=null;
                    s.controls.Add(new Control{id="world:lamp-eye-approach",path=PathOf(item.transform),kind="ground",x=point.x,y=Screen.height-point.y,left=point.x-2,right=point.x+2,top=Screen.height-point.y-2,bottom=Screen.height-point.y+2,enabled=valid,hit=valid&&point.x>0&&point.x<Screen.width&&point.y>0&&point.y<Screen.height});
                }
            }
            var battle=game.Battles;s.battleActive=battle.Active;s.battleEnemy=battle.Enemy?.id;s.battleOutcome=battle.Outcome;s.battleLog=battle.Log;
            s.enemyHP=battle.EnemyHP;s.enemyMaxHP=battle.EnemyMaxHP;s.battlePhase=battle.Phase;s.turn=battle.TurnNumber;s.shielded=battle.Shielded;s.shieldSymbol=battle.ShieldSymbol;s.canRepair=battle.CanRepair;
            var puzzle=game.Puzzles;s.puzzleActive=puzzle.Active;s.puzzleId=puzzle.Definition?.id;s.puzzleLog=puzzle.Log;s.rotations=puzzle.Rotations;s.folds=puzzle.Folds;s.rings=puzzle.Rings;
            if(puzzle.Activity!=null){var a=puzzle.Activity;s.activityDefinition=a.definition;s.placed=a.placed;s.inspected=a.inspected;s.turns=a.turns;s.dials=a.dials;s.prepared=a.Prepared;s.ready=a.Ready;s.progress=a.progress;}
            return s;
        }
        void OnDestroy(){alive=false;listener?.Stop();Application.logMessageReceived-=OnLog;}
    }
}
#endif
