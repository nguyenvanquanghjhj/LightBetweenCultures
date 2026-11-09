using System;
using System.Linq;
using UnityEngine;
namespace LightBetweenCultures
{
    [Serializable] public sealed class StoryInteractionDefinition
    {
        public string id,kind,title,scene,actionLabel,successText,gesture,introText;
        public bool enabled;
        public string[] itemIds=Array.Empty<string>();
        public int[] targetTurns=Array.Empty<int>(),dialTargets=Array.Empty<int>();
        public int inspectCount;
    }
    [Serializable] public sealed class LeisureDefinition
    {
        public string id,cityId,nodeId,title,spritePath,poseStatus,mechanic;
        public bool enabled,hasMapPosition;public Vector2 mapSize,offset,mapPosition;public float duration=4;
        public Sprite Sprite=>Resources.Load<Sprite>(spritePath);
    }
    [Serializable] public sealed class ActivityPresentation
    {
        public StoryInteractionDefinition[] entries=Array.Empty<StoryInteractionDefinition>();
        static ActivityPresentation cached;
        public static ActivityPresentation Load()=>cached??=JsonUtility.FromJson<ActivityPresentation>(Resources.Load<TextAsset>("Content/StoryInteractions").text);
        public static StoryInteractionDefinition For(string id)=>Load().entries.FirstOrDefault(e=>e.id==id&&e.enabled);
    }
    [Serializable] public sealed class LeisurePresentation
    {
        public LeisureDefinition[] entries=Array.Empty<LeisureDefinition>();
        static LeisurePresentation cached;
        public static LeisurePresentation Load()=>cached??=JsonUtility.FromJson<LeisurePresentation>(Resources.Load<TextAsset>("Content/LeisureActivities").text);
        public static LeisureDefinition At(string city,string node)=>Load().entries.FirstOrDefault(e=>e.enabled&&e.cityId==city&&e.nodeId==node);
    }
    // One transient gesture model inside PuzzleManager; owns no save, quest or reward ledger.
    public sealed class StoryActivity
    {
        public readonly StoryInteractionDefinition definition;
        public readonly bool[] placed,inspected;
        public readonly int[] turns,dials;
        public float progress {get;private set;}
        public bool EasyPlacement=>definition.id=="puzzle_market_code";
        public StoryActivity(StoryInteractionDefinition definition)
        {this.definition=definition;placed=new bool[definition.itemIds.Length];turns=new int[placed.Length];dials=new int[definition.dialTargets.Length];inspected=new bool[definition.inspectCount];}
        public bool Prepared=>placed.All(p=>p)&&Enumerable.Range(0,turns.Length).All(i=>turns[i]==definition.targetTurns[i])&&Enumerable.Range(0,dials.Length).All(i=>dials[i]==definition.dialTargets[i])&&inspected.All(p=>p);
        public bool Ready=>Prepared&&(string.IsNullOrEmpty(definition.gesture)||progress>=.99f);
        public bool Drop(int index,Vector2 error)
        {
            // At Hue U16, inspection is still required by Prepared/Ready, but
            // does not silently reject an otherwise correctly placed stamp.
            if(index<0||index>=placed.Length||error.magnitude>.10f||!EasyPlacement&&!inspected.All(p=>p))return false;
            placed[index]=true;return true;
        }
        public void Rotate(int index){if(index>=0&&index<turns.Length){turns[index]=(turns[index]+1)%4;progress=0;}}
        public void Dial(int index){if(index>=0&&index<dials.Length){dials[index]=(dials[index]+1)%4;progress=0;}}
        public void Inspect(int index){if(index>=0&&index<inspected.Length)inspected[index]=true;}
        public bool Gesture(float value){if(!Prepared)return false;progress=Mathf.Clamp01(value);return true;}
    }
}
