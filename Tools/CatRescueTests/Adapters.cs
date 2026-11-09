// Test-host adapters only. Production CatRescue.cs and SaveSystem.cs compile unchanged.
// This host checks domain and disk behavior; it cannot validate Unity native JSON,
// rendering, scene lifecycle or pointer input. Those require the Player QA suite.
using System;
using System.IO;
using System.Text.Json;

namespace UnityEngine
{
    public struct Vector2
    {
        public float x,y;
        public Vector2(float x,float y){this.x=x;this.y=y;}
        public float magnitude => (float)Math.Sqrt(x*x+y*y);
        public static Vector2 zero => new Vector2(0,0);
    }
    public static class Application { public static string persistentDataPath=Directory.GetCurrentDirectory(); }
    public static class Debug
    {
        public static void LogWarning(object value) { }
        public static void LogError(object value) { }
    }
    public static class JsonUtility
    {
        static readonly JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true};
        public static T FromJson<T>(string json)=>JsonSerializer.Deserialize<T>(json,options);
        public static string ToJson(object value,bool pretty=false)=>JsonSerializer.Serialize(value,value.GetType(),options);
        public static void FromJsonOverwrite(string json,object target)
        {
            var source=JsonSerializer.Deserialize(json,target.GetType(),options);
            foreach(var f in target.GetType().GetFields())f.SetValue(target,f.GetValue(source));
        }
    }
    public sealed class TextAsset {public string text;}
    public static class Resources
    {
        public static string CatalogPath;
        public static T Load<T>(string path) where T:class => new TextAsset{text=File.ReadAllText(CatalogPath)} as T;
    }
}
namespace LightBetweenCultures
{
    public sealed class NodeData {public string id,name;}
    public sealed class GameData
    {public string cityId,spawnNodeId;public NodeData[] nodes;}
    public sealed class GameBootstrap
    {
        public SaveData Save;
        public GameData Data;
        public CatRescueManager Cats;
        public bool Persist()=>SaveSystem.Write(Save);
        public bool IsCityUnlocked(string id)=>id=="hoi_an"||Cats?.Unlocks(id)==true;
    }
}
