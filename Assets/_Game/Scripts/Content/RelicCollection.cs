using System;
using System.Linq;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public sealed class RelicFragment
    {
        public string id,name,cityId,nodeId,proof,instruction,artStatus;
        public bool enabled;
        public string[] proofAny=Array.Empty<string>();
        public string[] claimAnchors=Array.Empty<string>();
        public int sourceTurns;
    }
    [Serializable] public sealed class CulturalRelic
    {
        public string id,name,cityId,restorationNode,trial,requiredFlag,requiredQuest;
        public string trialType,instructions,hint,completionText;
        public int[] trialTurns=Array.Empty<int>();
        public RelicSignal[] signals=Array.Empty<RelicSignal>();
        public RelicStreamRoute[] prismRoutes=Array.Empty<RelicStreamRoute>();
        public Vector2 prismFocus;
        public bool enabled;
        public string[] fragmentIds=Array.Empty<string>();
        public Vector2[] lights=Array.Empty<Vector2>();
        public string[] lightNodes=Array.Empty<string>();
    }
    [Serializable] public sealed class RelicSignal { public string nodeId,relicId; }
    [Serializable] public sealed class RelicStreamRoute { public Vector2[] points=Array.Empty<Vector2>(); }
    [Serializable] public sealed class RelicCatalog
    {
        public CulturalRelic[] relics=Array.Empty<CulturalRelic>();
        public RelicFragment[] fragments=Array.Empty<RelicFragment>();
        public RelicStoryLine[] story=Array.Empty<RelicStoryLine>();
    }
    // Presentation only. First matching line wins; conditions reuse earned save proof.
    [Serializable] public sealed class RelicStoryLine
    {
        public string key,cityId,text;
        public string[] all=Array.Empty<string>(),none=Array.Empty<string>(),visibleNodes=Array.Empty<string>();
    }

    // One collection for all cities. Existing v1/v2 lists are the only persisted state.
    // No story items, currencies, quest rewards, map gates or ending state are written here.
    public sealed class RelicCollection
    {
        readonly GameBootstrap game;
        public RelicCatalog Catalog { get; }
        public RelicCollection(GameBootstrap owner)
        { game=owner;Catalog=JsonUtility.FromJson<RelicCatalog>(Resources.Load<TextAsset>("Content/CulturalRelics").text); }
        public CulturalRelic Definition(string id)=>Catalog.relics.FirstOrDefault(r=>r.id==id);
        public RelicFragment Fragment(string id)=>Catalog.fragments.FirstOrDefault(f=>f.id==id);
        public bool Owned(string id)=>game.Save.flags.Contains("relic.fragment."+id);
        public bool Assembled(string id)=>game.Save.flags.Contains("relic.assembled."+id);
        public bool Restored(string id)=>game.Save.flags.Contains("relic.restored."+id);
        public int Count(CulturalRelic r)=>r.fragmentIds.Count(Owned);
        public bool AllRestored=>Catalog.relics.Length==6&&Catalog.relics.All(r=>Restored(r.id));
        public bool PhotoRecorded=>game.Save.flags.Contains("photo.v1.hoi_an.h6");
        public string Story(string key,string fallback="",string cityId=null)
        {
            var city=cityId??game.Data.cityId;
            var line=(Catalog.story??Array.Empty<RelicStoryLine>()).FirstOrDefault(s=>s.key==key&&s.cityId==city&&
                (s.all??Array.Empty<string>()).All(Proof)&&!(s.none??Array.Empty<string>()).Any(Proof)&&
                (s.visibleNodes??Array.Empty<string>()).All(id=>{var i=Array.FindIndex(game.Data.nodes,n=>n.id==id);return i>=0&&game.Discovery.IsVisible(i);}));
            var text=(line?.text??fallback).Replace("{base}",fallback);var local=Catalog.relics.FirstOrDefault(r=>r.cityId==city);
            return local==null?text:text.Replace("{count}",Count(local).ToString());
        }
        bool At(string city,string node)
        {return game.Data.cityId==city&&game.Save.node>=0&&game.Save.node<game.Data.nodes.Length&&game.Data.nodes[game.Save.node].id==node&&game.Discovery.IsVisible(game.Save.node);}
        bool Proof(string proof)
        {
            if(string.IsNullOrEmpty(proof))return true;
            if(proof.StartsWith("completed:",StringComparison.Ordinal))return game.Save.completed.Contains(proof.Substring(10));
            if(proof.StartsWith("quest:",StringComparison.Ordinal)){var id=proof.Substring(6);return game.Quests.IsActive(id)||game.Quests.IsCompleted(id)||game.World.Has(proof);}
            return game.World.Has(proof);
        }
        public bool CanClaim(RelicFragment f)=>f!=null&&f.enabled&&!Owned(f.id)&&(At(f.cityId,f.nodeId)||(f.claimAnchors??Array.Empty<string>()).Any(anchor=>anchor==game.Data.cityId+":"+game.Data.nodes[game.Save.node].id))&&game.Discovery.IsVisible(game.Save.node)&&Proof(f.proof)&&((f.proofAny??Array.Empty<string>()).Length==0||f.proofAny.Any(Proof));
        public bool CanVisit(CulturalRelic r)=>r!=null&&r.enabled&&At(r.cityId,r.restorationNode);
        public bool CanRitual(CulturalRelic r)=>CanVisit(r)&&Count(r)==4&&Assembled(r.id)&&(string.IsNullOrEmpty(r.requiredQuest)||game.Quests.IsCompleted(r.requiredQuest))&&game.World.Has(r.requiredFlag);
        public bool CanPhoto=>At("hoi_an","H6")&&game.World.Has("hoi_an.lamp_restored");
        public RelicActivity Active { get; private set; }
        public RelicActivity BeginSource(string id)
        {var f=Fragment(id);return Active=CanClaim(f)?new RelicActivity(this,"source",id,game.Data.cityId,game.Save.node,f.sourceTurns):null;}
        public RelicActivity BeginAssembly(string id)
        {var r=Definition(id);return Active=CanVisit(r)&&Count(r)==4?new RelicActivity(this,"assembly",id,game.Data.cityId,game.Save.node,0):null;}
        public RelicActivity BeginPhoto()=>Active=CanPhoto?new RelicActivity(this,"photo","hoi_an.h6",game.Data.cityId,game.Save.node,0):null;
        public bool Valid(RelicActivity a)=>a!=null&&Active==a&&a.cityId==game.Data.cityId&&a.node==game.Save.node&&game.Discovery.IsVisible(a.node);
        public void Cancel(){Active=null;}
        bool Persist(Action apply)
        {
            var flags=game.Save.flags.ToArray();var choices=game.Save.choices.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();
            apply();if(game.Persist())return true;
            game.Save.flags=flags.ToList();game.Save.choices=choices.ToList();game.Save.rewardGrantIds=ledger.ToList();return false;
        }
        void Set(string flag){if(!game.Save.flags.Contains(flag))game.Save.flags.Add(flag);}
        internal bool FinishSource(RelicActivity a)
        {
            var f=Fragment(a.id);if(!Valid(a)||!CanClaim(f)||a.mode!="source"||!a.Placed(0)||a.turns!=f.sourceTurns)return false;
            return Persist(()=>{Set("relic.proof."+f.id);Set("relic.fragment."+f.id);var key="relic:v1:claim:"+f.id;if(!game.Save.rewardGrantIds.Contains(key))game.Save.rewardGrantIds.Add(key);});
        }
        internal bool FinishAssembly(RelicActivity a)
        {
            var r=Definition(a.id);if(!Valid(a)||a.mode!="assembly"||!CanVisit(r)||Count(r)!=4||!Enumerable.Range(0,4).All(a.Placed))return false;
            return Persist(()=>Set("relic.assembled."+r.id));
        }
        internal bool FinishTrial(RelicActivity a)
        {
            var r=Definition(a.id);if(!Valid(a)||a.mode!="assembly"||!CanRitual(r)||!a.TrialSucceeded)return false;
            return Persist(()=>{Set("relic.trial."+r.id);Set("relic.restored."+r.id);});
        }
        internal bool FinishPhoto(RelicActivity a)
        {
            if(!Valid(a)||a.mode!="photo"||!CanPhoto||!a.Placed(0)||!a.Placed(1))return false;
            // Reuse the old transaction/ledger. An already marked save only records
            // the new photo evidence; it never receives the old reward again.
            var ci=game.Content.CheckIn("hoi_an","H6");
            if(!game.World.Has(ci.completionFlag))
            {game.Events.Execute(game.Save.node,"checkin");if(!game.World.Has(ci.completionFlag))return false;}
            return Persist(()=>{Set("photo.v1.hoi_an.h6");Set("photo.v1.hoi_an.h6."+(Restored("relic_hoi_an_lantern")?"illuminated":"beacon"));});
        }
    }

    public sealed class RelicBeamTrace
    {public bool success;public string message;public System.Collections.Generic.List<Vector2> points=new System.Collections.Generic.List<Vector2>();}
    // Ephemeral tactile session; completing/closing a view alone cannot award evidence.
    public sealed partial class RelicActivity
    {
        readonly RelicCollection owner;readonly bool[] placed=new bool[4];
        public readonly string mode,id,cityId;public readonly int node;
        public int turns {get;private set;}
        public readonly bool[] mirrors={false,true,true};
        public RelicActivity(RelicCollection owner,string mode,string id,string city,int node,int targetTurns)
        {this.owner=owner;this.mode=mode;this.id=id;cityId=city;this.node=node;turns=(targetTurns+2)%4;InitializeTrial();}
        public bool Placed(int i)=>i>=0&&i<4&&placed[i];
        public bool Drop(int i,Vector2 normalizedError)
        {if(!owner.Valid(this)||i<0||i>=(mode=="assembly"?4:mode=="photo"?2:1)||normalizedError.magnitude>.10f)return false;placed[i]=true;return true;}
        public void Rotate(){if(owner.Valid(this))turns=(turns+1)%4;}
        public void Mirror(int i){if(owner.Valid(this)&&i>=0&&i<3)mirrors[i]=!mirrors[i];}
        public bool Claim()=>owner.FinishSource(this);
        public bool Assemble()=>owner.FinishAssembly(this);
        public bool Restore()=>owner.FinishTrial(this);
        public bool RecordPhoto()=>owner.FinishPhoto(this);
        public RelicBeamTrace Trace()
        {
            var t=new RelicBeamTrace();var pos=new Vector2Int(0,1);var dir=Vector2Int.right;
            var locations=new[]{new Vector2Int(2,1),new Vector2Int(2,3),new Vector2Int(0,3)};
            t.points.Add(pos);
            for(var step=0;step<40;step++)
            {
                pos+=dir;t.points.Add(pos);
                if(pos==new Vector2Int(0,4)){t.success=true;t.message="Ánh đèn đã đi qua bến, mái hiên và phố cổ.";return t;}
                if(pos.x<0||pos.y<0||pos.x>4||pos.y>4){t.message="Tia sáng lệch khỏi phố. Xoay gương rồi thử lại.";return t;}
                if(pos==new Vector2Int(3,1)||pos==new Vector2Int(1,2)){t.message="Mái nhà chặn tia sáng. Hãy đổi hướng phản chiếu.";return t;}
                var i=Array.IndexOf(locations,pos);if(i>=0)dir=mirrors[i]?new Vector2Int(dir.y,dir.x):new Vector2Int(-dir.y,-dir.x);
            }
            t.message="Tia sáng đang quay vòng. Hãy đổi góc gương.";return t;
        }
    }
}
