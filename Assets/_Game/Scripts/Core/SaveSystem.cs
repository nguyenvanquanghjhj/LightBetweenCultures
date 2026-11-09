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
        // Optional leisure history, independent of the22 legacy puzzle proofs. No schema bump.
        public List<string> leisureCompleted = new List<string>();
        // Optional additive Catventure progress. Old v1/v2 saves remain compatible.
        public List<CatRescueProgress> catRescues = new List<CatRescueProgress>();
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
        public static bool RecoveryBlocked {get;private set;}
        public static string RecoverySource {get;private set;}
        public static string RecoveryMessage {get;private set;}
        static bool recovered;
#if UNITY_ANDROID && !UNITY_EDITOR
        // Android's app sandbox may deny link(2), used by managed Replace/Move
        // implementations. rename(2) replaces a same-directory file atomically
        // without a hard link; leave the temporary file intact on failure.
        [System.Runtime.InteropServices.DllImport("libc",EntryPoint="rename",SetLastError=true)]
        static extern int RenameFile(string source,string destination);
#endif
        static void CommitTemporary(string temporary,string destination)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if(RenameFile(temporary,destination)!=0)
                throw new IOException("Atomic save rename failed (errno "+System.Runtime.InteropServices.Marshal.GetLastWin32Error()+")");
#else
            if(File.Exists(destination))File.Replace(temporary,destination,null);
            else File.Move(temporary,destination);
#endif
        }
        public static string PathName => OverridePath ?? Path.Combine(Application.persistentDataPath, "hoi-an-v1.json");
        public static SaveData Load(int nodeCount)
        {
            RecoveryBlocked=false;RecoverySource=null;RecoveryMessage="";recovered=false;var found=false;
            foreach (var path in new[] { PathName, PathName + ".bak", PathName+".tmp" })
            {
                if (!File.Exists(path)) continue;
                found=true;
                try
                {
                    var json=File.ReadAllText(path);
                    if(!json.TrimStart().StartsWith("{")||!json.Contains("\"version\"")||!json.Contains("\"items\"")||!json.Contains("\"flags\""))throw new InvalidDataException("Missing core save fields");
                    var data = JsonUtility.FromJson<SaveData>(json);
                    if (data == null || data.version < 1 || data.version > 2 || data.items == null || data.collected == null || data.accepted == null || data.completed == null || data.flags == null) throw new InvalidDataException("Invalid save schema");
                    var legacy = data.version == 1;
                    data.version = 2;
                    if(string.IsNullOrEmpty(data.cityId)) data.cityId="hoi_an";
                    data.resolvedEvents ??= new List<string>(); data.defeatedEnemies ??= new List<string>();
                    data.solvedPuzzles ??= new List<string>(); data.choices ??= new List<string>();
                    data.rewardGrantIds ??= new List<string>(); data.cityProgress ??= new List<CityProgress>();
                    data.leisureCompleted ??= new List<string>();
                    data.catRescues ??= new List<CatRescueProgress>();
                    data.catRescues.RemoveAll(p => p == null || string.IsNullOrEmpty(p.cityId));
                    foreach(var rescue in data.catRescues)
                    {
                        rescue.completed ??= new List<string>(); rescue.supplies ??= new List<string>();
                        rescue.challenges ??= new List<CatChallengeProgress>();
                        rescue.challenges.RemoveAll(c => c == null || string.IsNullOrEmpty(c.challengeId));
                        foreach(var challenge in rescue.challenges)
                        { challenge.marks ??= new List<int>(); challenge.path ??= new List<int>(); }
                    }
                    if(legacy) { data.playerHP=100; data.lightEnergy=3; }
                    // Map size comes from gameplay data. Preserve inventory/quests if a
                    // content revision removed the saved location or the value is invalid.
                    if (data.node < 0 || data.node >= nodeCount) data.node = 0;
                    // Optional visual history: old v1 saves have no visited field.
                    if (data.visited == null) data.visited = new List<int> { 0 };
                    data.visited.RemoveAll(n => n < 0 || n >= nodeCount);
                    if (!data.visited.Contains(data.node)) data.visited.Add(data.node);
                    RecoverySource=path;recovered=path!=PathName;
                    if(recovered)RecoveryMessage="Đã khôi phục bản lưu dự phòng. Tiến độ của bạn được giữ lại.";
                    return data;
                }
                catch (Exception e) { Debug.LogWarning("Cannot load " + path + ": " + e.Message); }
            }
            RecoveryBlocked=found;
            if(found)RecoveryMessage="Không đọc được bản lưu chính và dự phòng. Các file được giữ nguyên; trò chơi tạm ngừng lưu để tránh mất tiến độ. Hãy thử đọc lại hoặc khôi phục bản lưu.";
            return new SaveData();
        }
        public static bool Write(SaveData data)
        {
            if(RecoveryBlocked){Debug.LogWarning("Save recovery pending: existing files preserved");return false;}
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PathName));
                var temporary=RecoverySource==PathName+".tmp"?PathName+".write-new":PathName+".tmp";
                File.WriteAllText(temporary, JsonUtility.ToJson(data, true));
                if (File.Exists(PathName))
                {
                    // Never replace the only valid backup with a corrupt primary.
                    if(recovered)File.Copy(PathName,PathName+".unreadable-"+DateTime.UtcNow.ToString("yyyyMMddHHmmssfff")+"-"+Guid.NewGuid().ToString("N"));
                    else File.Copy(PathName,PathName+".bak",true);
                }
                CommitTemporary(temporary,PathName);
                recovered=false;RecoverySource=PathName;
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
