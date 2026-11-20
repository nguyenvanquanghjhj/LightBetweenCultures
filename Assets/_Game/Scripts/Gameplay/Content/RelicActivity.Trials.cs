using System;
using System.Linq;
using UnityEngine;
namespace LightBetweenCultures
{
    // Five tactile rules on the existing relic session; no saved progress or second collection.
    public sealed partial class RelicActivity
    {
        public string TrialType=>owner.Definition(id)?.trialType??"";
        public readonly int[] tileTurns=new int[4];
        public readonly bool[] surveyed=new bool[4],joined=new bool[3];
        public readonly bool[] fogCleared=new bool[24];
        public int Dial {get;private set;}
        public int StarSequence {get;private set;}
        public int WrongReadings {get;private set;}
        public static readonly Vector2[] SurveyPoints={new Vector2(.20f,.67f),new Vector2(.75f,.70f),new Vector2(.28f,.27f),new Vector2(.77f,.26f)};
        public static readonly int[] StarOrder={0,2,1};
        void InitializeTrial()
        {for(var i=0;i<4;i++)tileTurns[i]=((owner.Definition(id)?.trialTurns?.ElementAtOrDefault(i)??0)+1)%4;}
        public void TurnTile(int i)
        {if(owner.Valid(this)&&mode=="assembly"&&i>=0&&i<4)tileTurns[i]=(tileTurns[i]+1)%4;}
        public void TurnDial()
        {if(owner.Valid(this)&&mode=="assembly")Dial=(Dial+1)%(TrialType=="mist"?8:4);}
        public void Sweep(Vector2 normalized)
        {
            if(!owner.Valid(this)||mode!="assembly")return;
            for(var i=0;i<4;i++)if(Vector2.Distance(normalized,SurveyPoints[i])<.105f)surveyed[i]=true;
            for(var y=0;y<4;y++)for(var x=0;x<6;x++)if(Vector2.Distance(normalized,new Vector2((x+.5f)/6,(y+.5f)/4))<.20f)fogCleared[y*6+x]=true;
        }
        public bool ReadStar(int index)
        {
            if(!owner.Valid(this)||TrialType!="lens"||StarSequence>=3)return false;
            if(index<0||index>2||!surveyed[index]||index!=StarOrder[StarSequence]){StarSequence=0;WrongReadings++;return false;}
            StarSequence++;return true;
        }
        public bool JoinStream(int index,Vector2 error)
        {if(!owner.Valid(this)||TrialType!="prism"||index<0||index>2||error.magnitude>.10f)return false;joined[index]=true;return true;}
        public void ResetTrial()
        {
            if(!owner.Valid(this))return;
            Array.Clear(surveyed,0,4);Array.Clear(fogCleared,0,24);Array.Clear(joined,0,3);Dial=0;StarSequence=0;WrongReadings=0;InitializeTrial();
        }
        bool OrientationsMatch(int count)
        {
            var target=owner.Definition(id)?.trialTurns??Array.Empty<int>();
            return target.Length>=count&&Enumerable.Range(0,count).All(i=>tileTurns[i]==target[i]);
        }
        public RelicBeamTrace PrismTrace(int stream)
        {
            var trace=new RelicBeamTrace();var r=owner.Definition(id);
            if(stream<0||stream>2||r==null||r.prismRoutes.Length!=3)return trace;
            var route=r.prismRoutes[stream].points;if(route.Length<2)return trace;
            trace.points.Add(route[0]);var directions=new[]{Vector2.right,Vector2.down,Vector2.left,Vector2.up};
            var heading=directions[tileTurns[stream]];
            if(!joined[stream]||Vector2.Dot(heading,(route[1]-route[0]).normalized)<.99f)
            {trace.points.Add(route[0]+heading*.15f);return trace;}
            trace.points.AddRange(route.Skip(1));trace.success=route.All(p=>p.x>=0&&p.x<=1&&p.y>=0&&p.y<=1)&&Vector2.Distance(route[route.Length-1],r.prismFocus)<.005f;return trace;
        }
        bool PrismConverges()
        {
            var routes=Enumerable.Range(0,3).Select(PrismTrace).ToArray();if(routes.Any(t=>!t.success))return false;
            var focus=owner.Definition(id).prismFocus;
            for(var i=0;i<3;i++)for(var j=i+1;j<3;j++)
            for(var a=1;a<routes[i].points.Count;a++)for(var b=1;b<routes[j].points.Count;b++)
            {
                var p=routes[i].points[a-1];var v=routes[i].points[a]-p;var q=routes[j].points[b-1];var w=routes[j].points[b]-q;
                var cross=v.x*w.y-v.y*w.x;var delta=q-p;
                if(Mathf.Abs(cross)<.00001f)
                {
                    if(Mathf.Abs(delta.x*v.y-delta.y*v.x)>.00001f||v.sqrMagnitude<.00001f)continue;
                    var t0=Vector2.Dot(delta,v)/v.sqrMagnitude;var t1=Vector2.Dot(q+w-p,v)/v.sqrMagnitude;
                    var lo=Mathf.Max(0,Mathf.Min(t0,t1));var hi=Mathf.Min(1,Mathf.Max(t0,t1));
                    if(hi>=lo&&(Vector2.Distance(p+lo*v,focus)>.08f||Vector2.Distance(p+hi*v,focus)>.08f))return false;
                    continue;
                }
                var t=(delta.x*w.y-delta.y*w.x)/cross;var u=(delta.x*v.y-delta.y*v.x)/cross;
                if(t>=0&&t<=1&&u>=0&&u<=1&&Vector2.Distance(p+t*v,focus)>.08f)return false;
            }
            return true;
        }
        public bool TrialSucceeded
        {
            get
            {
                if(!owner.Valid(this))return false;
                switch(TrialType)
                {
                    case "hanji":return OrientationsMatch(4);
                    case "seal":return surveyed.All(v=>v)&&Dial==2;
                    case "mist":return surveyed.All(v=>v)&&fogCleared.Count(v=>v)>=12&&Dial==1;
                    case "lens":return surveyed.Take(3).All(v=>v)&&StarSequence==3&&Dial==3;
                    case "prism":return joined.All(v=>v)&&PrismConverges();
                    default:return Trace().success; // Existing Hoi An beam, unchanged.
                }
            }
        }
    }
}
