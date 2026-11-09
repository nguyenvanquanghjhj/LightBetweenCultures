using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public sealed class SaveData
    {
        public int version = 2;
        public string cityId = "hoi_an";
        public int node;
        public int previousNode;
        public List<int> visited = new List<int> { 0 };
        public List<string> accepted = new List<string>();
        public List<string> completed = new List<string>();
        public List<string> items = new List<string>();
        public List<string> collected = new List<string>();
        public List<string> flags = new List<string>();
        public List<string> resolvedEvents = new List<string>();
        public List<string> defeatedEnemies = new List<string>();
        public List<string> solvedPuzzles = new List<string>();
        public List<string> choices = new List<string>();
        public List<string> rewardGrantIds = new List<string>();
        public List<CityProgress> cityProgress = new List<CityProgress>();
        public int playerHP = 100, lightEnergy = 3, coins;
    }
    [Serializable] public sealed class CityProgress
    {
        public string cityId;
        public int node, stage;
        public List<int> visited = new List<int>();
    }
    public static class SaveSystem
    {
        // Automation supplies a separate path so it never touches the player's save.
        public static string OverridePath;
        public static string PathName => OverridePath ?? Path.Combine(Application.persistentDataPath, "hoi-an-v1.json");
        public static SaveData Load(int nodeCount)
        {
            foreach (var path in new[] { PathName, PathName + ".bak" })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    var data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                    if (data == null || data.version < 1 || data.version > 2 || data.items == null || data.collected == null || data.accepted == null || data.completed == null || data.flags == null) throw new InvalidDataException("Invalid save schema");
                    var legacy = data.version == 1;
                    data.version = 2;
                    if(string.IsNullOrEmpty(data.cityId)) data.cityId="hoi_an";
                    data.resolvedEvents ??= new List<string>(); data.defeatedEnemies ??= new List<string>();
                    data.solvedPuzzles ??= new List<string>(); data.choices ??= new List<string>();
                    data.rewardGrantIds ??= new List<string>(); data.cityProgress ??= new List<CityProgress>();
                    if(legacy) { data.playerHP=100; data.lightEnergy=3; }
                    // Map size comes from gameplay data. Preserve inventory/quests if a
                    // content revision removed the saved location or the value is invalid.
                    if (data.node < 0 || data.node >= nodeCount) data.node = 0;
                    // Optional visual history: old v1 saves have no visited field.
                    if (data.visited == null) data.visited = new List<int> { 0 };
                    data.visited.RemoveAll(n => n < 0 || n >= nodeCount);
                    if (!data.visited.Contains(data.node)) data.visited.Add(data.node);
                    return data;
                }
                catch (Exception e) { Debug.LogWarning("Cannot load " + path + ": " + e.Message); }
            }
            return new SaveData();
        }
        public static bool Write(SaveData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PathName));
                File.WriteAllText(PathName + ".tmp", JsonUtility.ToJson(data, true));
                if (File.Exists(PathName)) File.Replace(PathName + ".tmp", PathName, PathName + ".bak");
                else File.Move(PathName + ".tmp", PathName);
                return true;
            }
            catch (Exception e) { Debug.LogError("Save failed: " + e.Message); return false; }
        }
        public static void Reset()
        {
            foreach (var suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(PathName + suffix)) File.Delete(PathName + suffix);
        }
    }
}
