using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using LightBetweenCultures;
using UnityEngine;

static class Program
{
    static int checks;
    static readonly List<string> log=new List<string>();
    static GameBootstrap game;
    static string root, output, save;
    static void Check(bool value,string label)
    {
        if(!value)throw new Exception("FAIL: "+label);
        checks++;log.Add("PASS: "+label);
    }
    static void Enter(CatRescueChapter c)
    {
        var ids=c.challenges.Select(x=>x.nodeId).Concat(c.supplies.Select(x=>x.nodeId)).Append(c.rescueNode).Distinct().ToArray();
        game.Save.cityId=c.cityId;game.Data=new GameData{cityId=c.cityId,spawnNodeId=ids[0],nodes=ids.Select(id=>new NodeData{id=id,name=id}).ToArray()};game.Save.node=0;
    }
    static void At(string node)=>game.Save.node=Array.FindIndex(game.Data.nodes,n=>n.id==node);
    static void WriteFailure(Action action,string label)
    {
        var before=JsonUtility.ToJson(game.Save);var blocked=Path.Combine(output,"blocked-parent");File.WriteAllText(blocked,"controlled QA file, not a directory");
        SaveSystem.OverridePath=Path.Combine(blocked,"save.json");action();
        Check(JsonUtility.ToJson(game.Save)==before,label);
        SaveSystem.OverridePath=save;
    }
    static List<int> Route(CatRescueChallenge c)
    {
        int w=c.maze[0].Length;var previous=new Dictionary<int,int>{{c.start,-1}};var queue=new Queue<int>();queue.Enqueue(c.start);
        while(queue.Count>0)
        {
            int from=queue.Dequeue();
            foreach(var to in new[]{from-1,from+1,from-w,from+w})
            {
                if(to<0||to>=w*c.maze.Length||Math.Abs(to%w-from%w)+Math.Abs(to/w-from/w)!=1||c.maze[to/w][to%w]=='#'||previous.ContainsKey(to))continue;
                previous[to]=from;queue.Enqueue(to);
            }
        }
        var path=new List<int>();for(int cell=c.exit;cell>=0;cell=previous[cell])path.Add(cell);path.Reverse();return path;
    }
    static void Reload()
    {
        game.Save=SaveSystem.Load(30);game.Cats=new CatRescueManager(game);
    }
    static void Run()
    {
        game=new GameBootstrap{Save=new SaveData()};game.Cats=new CatRescueManager(game);
        // Actual SaveSystem migration, including explicitly null optional fields.
        File.WriteAllText(save,"{\"version\":1,\"items\":[\"lamp_eye\"],\"flags\":[\"legacy.flag\"],\"accepted\":[],\"completed\":[],\"collected\":[],\"coins\":11,\"catRescues\":null}");
        Reload();Check(game.Save.version==2&&game.Save.catRescues.Count==0&&game.Save.coins==11&&game.Save.items.Contains("lamp_eye")&&game.Save.flags.Contains("legacy.flag"),"v1 migration keeps legacy state and normalizes optional cats");
        game.Save=new SaveData();game.Save.items.Add("legacy_item");game.Save.flags.Add("legacy.flag");game.Save.coins=3;
        Check(game.Persist(),"Initial isolated save committed");
        foreach(var c in game.Cats.Catalog.chapters)
        {
            Enter(c);var cats=game.Cats;
            Check(game.IsCityUnlocked(c.cityId),"Previous rescue unlocks city "+c.cityId);
            Check(cats.Progress(c.cityId)==null,"Read-only progress query does not auto-accept "+c.cityId);
            Check(cats.Start(),"Accept "+c.cityId);
            var snapshot=JsonUtility.ToJson(game.Save);
            Check(!cats.Start()&&JsonUtility.ToJson(game.Save)==snapshot,"Double acceptance inert "+c.cityId);
            Check(!cats.Rescue(Vector2.zero),"Rescue before tasks blocked "+c.cityId);
            foreach(var a in c.challenges)
            {
                At(a.nodeId);Check(!cats.Ready(a)&&!cats.Complete(a.id),"No empty completion "+a.id);
                Check(!cats.Complete("unknown-id"),"Unknown challenge rejected "+a.id);
                // Move away even when two tasks share an anchor.
                var actual=game.Save.node;game.Save.node=-1;
                Check(!cats.Match(a.id,0,0)&&!cats.Choose(a.id,a.answer)&&!cats.Find(a.id,0)&&!cats.Walk(a.id,1),"Remote actions rejected "+a.id);game.Save.node=actual;
                if(a.mode=="choice"||a.mode=="scenario")
                {
                    Check(!cats.Choose(a.id,-1)&&!cats.Choose(a.id,(a.answer+1)%a.options.Length)&&!cats.Ready(a),"Wrong/invalid answer preserves retry "+a.id);
                    WriteFailure(()=>Check(!cats.Choose(a.id,a.answer),"Correct choice reports failed write "+a.id),"Choice rolls back failed disk commit "+a.id);
                    Check(cats.Choose(a.id,a.answer),"Correct choice persists "+a.id);
                }
                else if(a.mode=="hidden")
                {
                    Check(!cats.Find(a.id,-1)&&!cats.Find(a.id,a.tokens.Length),"Hidden distractor and bad index rejected "+a.id);
                    for(int i=0;i<a.tokens.Length;i++)Check(cats.Find(a.id,i)&&!cats.Find(a.id,i),"Hidden proof unique "+a.id+":"+i);
                }
                else if(a.mode=="maze")
                {
                    Check(!cats.Walk(a.id,a.exit)&&!cats.Walk(a.id,-1),"Maze cannot teleport or leave board "+a.id);
                    var route=Route(a);Check(cats.Walk(a.id,route[1]),"First maze segment saved "+a.id);
                    Check(cats.ResetMaze(a.id)&&!cats.Ready(a),"Maze retry resets only current path "+a.id);
                    foreach(var cell in route.Skip(1))Check(cats.Walk(a.id,cell),"Adjacent maze segment "+a.id+":"+cell);
                }
                else
                {
                    Check(!cats.Match(a.id,-1,0)&&!cats.Match(a.id,0,1)&&!cats.Match(a.id,a.tokens.Length,a.tokens.Length),"Bad drag target/index rejected "+a.id);
                    WriteFailure(()=>Check(!cats.Match(a.id,0,0),"Placement reports failed write "+a.id),"Partial placement rollback "+a.id);
                    for(int i=0;i<a.tokens.Length;i++)
                    {
                        Check(cats.Match(a.id,i,i),"Correct placement saved "+a.id+":"+i);
                        Check(!cats.Match(a.id,i,i),"Duplicate placement inert "+a.id+":"+i);
                        if(i==0){Reload();cats=game.Cats;Check(cats.State(a).marks.Contains(i),"Partial placement survives disk reload "+a.id);}
                    }
                }
                if(a.mode=="hanbok")
                {
                    Check(!cats.Ready(a)&&!cats.PickHanbokPalette(a.id,-1)&&!cats.PickHanbokPalette(a.id,4),"Hanbok needs a valid palette choice");
                    WriteFailure(()=>Check(!cats.PickHanbokPalette(a.id,2),"Palette reports failed write"),"Palette rolls back failed commit");
                    Check(cats.PickHanbokPalette(a.id,2),"Palette choice saved");Reload();cats=game.Cats;
                    Check(cats.State(a).paletteChosen&&cats.State(a).palette==2,"Palette survives disk reload");
                }
                Check(cats.Ready(a),"Proof ready "+a.id);
                WriteFailure(()=>Check(!cats.Complete(a.id),"Completion reports failed write "+a.id),"Completion rollback keeps earlier progress "+a.id);
                Check(cats.Complete(a.id)&&!cats.Complete(a.id),"Complete exactly once "+a.id);
            }
            At(c.rescueNode);Check(!cats.Rescue(Vector2.zero),"All tasks without supplies cannot rescue "+c.cityId);
            foreach(var s in c.supplies)
            {
                game.Save.node=-1;Check(!cats.Collect(s.id),"Remote supply blocked "+s.id);At(s.nodeId);
                Check(cats.Collect(s.id)&&!cats.Collect(s.id),"Supply unique "+s.id);
            }
            At(c.rescueNode);Check(cats.CanRescue,"Final area opened only with tasks and supplies "+c.cityId);
            Check(!cats.Rescue(new Vector2(.5f,.5f)),"Bad rescue placement rejected "+c.cityId);
            var coins=game.Save.coins;
            WriteFailure(()=>Check(!cats.Rescue(Vector2.zero),"Rescue reports failed write "+c.cityId),"Failed rescue rolls back cat, reward and receipt "+c.cityId);
            Check(cats.Rescue(Vector2.zero)&&cats.Rescued(c.cityId)&&game.Save.coins==coins+c.rewardCoins,"Cat and coins awarded atomically "+c.cityId);
            Check(!cats.Rescue(Vector2.zero)&&game.Save.coins==coins+c.rewardCoins,"Reward cannot be claimed twice "+c.cityId);
            Reload();Check(game.Cats.Rescued(c.cityId),"Rescued cat survives disk reload "+c.cityId);
        }
        Check(game.Cats.RescuedCount==6&&game.Save.rewardGrantIds.Count==6&&game.Save.coins==51,"Six cats, six receipts, exact total rewards");
        Check(game.Save.items.SequenceEqual(new[]{"legacy_item"})&&game.Save.flags.SequenceEqual(new[]{"legacy.flag"})&&game.Save.completed.Count==0&&game.Save.solvedPuzzles.Count==0,"Rescue leaves legacy inventory, flags, quests and puzzles untouched");
        game.Persist();game.Persist();File.WriteAllText(save,"corrupt-primary");Reload();
        Check(game.Cats.RescuedCount==6&&!SaveSystem.RecoveryBlocked&&SaveSystem.RecoverySource==save+".bak","Actual save backup recovers all six cats");
        Check(game.Persist(),"Recovered save commits while preserving corrupt primary evidence");
        var bad=Path.Combine(output,"unreadable.json");File.WriteAllText(bad,"corrupt");SaveSystem.OverridePath=bad;
        var blocked=SaveSystem.Load(30);Check(SaveSystem.RecoveryBlocked&&!SaveSystem.Write(blocked)&&File.ReadAllText(bad)=="corrupt","Unreadable save blocks overwrite");
        SaveSystem.OverridePath=save;Reload();
    }
    static int Main(string[] args)
    {
        root=Path.GetFullPath(args[0]);output=Path.Combine(root,"Validation/Catventure-20261107/domain");Directory.CreateDirectory(output);save=Path.Combine(output,"campaign-save.json");SaveSystem.OverridePath=save;
        Resources.CatalogPath=Path.Combine(root,"Assets/_Game/Resources/Content/CatRescue.json");
        try
        {
            if(args.Contains("--restart"))
            {
                game=new GameBootstrap();Reload();Enter(game.Cats.Catalog.chapters.Last());
                Check(game.Cats.RescuedCount==6&&game.Save.coins==51&&game.Save.rewardGrantIds.Count==6,"Fresh process restores six cats and exact receipts");
                Check(!game.Cats.Rescue(Vector2.zero)&&game.Save.coins==51,"Fresh process cannot replay reward");
            }
            else Run();
            log.Add("ALL "+checks+" DOMAIN CHECKS PASSED (adapter host; Unity Player NOT TESTED)");
            File.WriteAllLines(Path.Combine(output,args.Contains("--restart")?"restart-results.txt":"results.txt"),log);
            Console.WriteLine(log.Last());return 0;
        }
        catch(Exception e)
        {log.Add(e.ToString());File.WriteAllLines(Path.Combine(output,"failure.txt"),log);Console.WriteLine(e);return 1;}
    }
}
