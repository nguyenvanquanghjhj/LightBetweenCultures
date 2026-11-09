using UnityEngine;

namespace LightBetweenCultures
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public GameData Data { get; private set; }
        public SaveData Save { get; private set; }
        public Inventory Inventory { get; private set; }
        public QuestManager Quests { get; private set; }
        public ContentDatabase Content { get; private set; }
        public WorldState World { get; private set; }
        public MapDiscoveryManager Discovery { get; private set; }
        public NodeEventManager Events { get; private set; }
        public BattleManager Battles { get; private set; }
        public PuzzleManager Puzzles { get; private set; }
        public RelicCollection Relics { get; private set; }
        public JourneyShop Shop { get; private set; }
        public CatRescueManager Cats { get; private set; }
        public SceneTransition Scenes { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearStatic() { Instance = null; SaveSystem.OverridePath = null; }
        public static GameBootstrap Ensure()
        {
            if (Instance == null) new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            return Instance;
        }
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this; DontDestroyOnLoad(gameObject);
            FramePacing.Apply();
            Content=ContentDatabase.Load(); Data = GameData.Load(); Reload(); Scenes = gameObject.AddComponent<SceneTransition>();
        }
        public void Reload()
        {
            var nodeLimit=1; foreach(var city in Content.cities) nodeLimit=System.Math.Max(nodeLimit,city.nodes.Length);
            Save = SaveSystem.Load(nodeLimit);
            if(Content.City(Save.cityId)==null) Save.cityId="hoi_an";
            Data=GameData.Load(Save.cityId);
            if(Save.node<0 || Save.node>=Data.nodes.Length) Save.node=Data.SpawnIndex;
            if (!Data.Unlocked(Data.nodes[Save.node].requirement, Save)) Save.node = Data.SpawnIndex;
            if (!Save.visited.Contains(Save.node)) Save.visited.Add(Save.node);
            Inventory = new Inventory(Save); World=new WorldState(this); Quests = new QuestManager(this);
            World.RefreshDerivedFlags();
            Discovery=new MapDiscoveryManager(this);
            // Import the old tutorial without granting its consumed reward twice.
            if(Save.accepted.Contains(Data.quest.id))
            {
                Quests.Accept("HQ01"); World.Set("quest:HQ01");
                if(Save.collected.Contains(Data.quest.item) && !Save.completed.Contains("HQ01"))
                { Inventory.GrantOnce("h3-lamp-eye","lamp_eye",1); World.Set("hoi_an.lamp_eye_found"); }
            }
            Discovery.Refresh();
            if(!Discovery.IsVisible(Save.node)) Save.node=Data.SpawnIndex;
            Battles=new BattleManager(this); Puzzles=new PuzzleManager(this); Events=new NodeEventManager(this); Relics=new RelicCollection(this);
            Shop=new JourneyShop(this);
            Cats=new CatRescueManager(this);
        }
        public bool Persist()
        {
            World?.RefreshDerivedFlags();
            if(Discovery!=null) Discovery.Refresh();
            if(Save.cityProgress!=null)
            {
                var progress=Save.cityProgress.Find(p=>p.cityId==Save.cityId);
                if(progress!=null) { progress.node=Save.node; progress.visited=new System.Collections.Generic.List<int>(Save.visited); }
            }
            return SaveSystem.Write(Save);
        }
        public bool IsCityUnlocked(string cityId)
        {
            var city=Content.City(cityId);
            return city!=null && (cityId=="hoi_an" || System.Array.TrueForAll(city.unlockFlags,World.Has) || Cats?.Unlocks(cityId)==true);
        }
        public bool EnterCity(string cityId)
        {
            var definition=Content.City(cityId);
            if(definition==null || !definition.runtimeReady || !IsCityUnlocked(cityId)) return false;
            Persist(); Save.cityId=cityId;
            var progress=Save.cityProgress.Find(p=>p.cityId==cityId);
            Data=GameData.Load(cityId);
            Save.node=progress?.node??Data.SpawnIndex;
            Save.visited=progress!=null?new System.Collections.Generic.List<int>(progress.visited):new System.Collections.Generic.List<int>();
            if(Save.node<0||Save.node>=Data.nodes.Length||!Discovery.IsVisible(Save.node))Save.node=Data.SpawnIndex;
            if(!Save.visited.Contains(Save.node))Save.visited.Add(Save.node);
            Save.previousNode=Save.node;Persist();return true;
        }
        public void ResetSave() { SaveSystem.Reset(); Reload(); }
        private void OnApplicationPause(bool paused) { if (paused) Persist(); }
        private void OnApplicationQuit() => Persist();
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
