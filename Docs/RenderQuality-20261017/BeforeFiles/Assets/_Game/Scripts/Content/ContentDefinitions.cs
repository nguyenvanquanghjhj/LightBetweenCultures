using System;
using System.Linq;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public class ItemAmount { public string itemId; public int amount = 1; }
    [Serializable] public class ItemDefinition
    {
        public string id, name, type, source;
        public int stackLimit = 99, healHP;
        public string description, category, iconId, sourceCity;
        public bool stackable, consumable, questItem;
        public int value;
        public string[] effects=Array.Empty<string>(),relatedQuestIds=Array.Empty<string>();
    }
    [Serializable] public class EnemyDefinition
    {
        public string id, cityId, name, dropOnce, alternativeResolution, completionFlag;
        public int level, hp, attack, defense, rewardCoins;
        public string[] skills = Array.Empty<string>();
        public ItemAmount[] rewardItems = Array.Empty<ItemAmount>();
        public bool replayable, balanceProposal = true;
        public string[] weaknessFlags = Array.Empty<string>();
        public int weaknessHpReduction, weaknessAttackReduction;
        public int weaknessDefenseReduction, phaseOneHP, phaseTwoHP;
        public string mechanic;
        public string[] shieldPattern=Array.Empty<string>(),repairFlags=Array.Empty<string>();
    }
    [Serializable] public class QuestDefinition
    {
        public string id, cityId, title, giverNode, completeAtNode, completionFlag, offer, reminder, thanks, sourceStatus;
        public string[] requiredFlags = Array.Empty<string>(), acceptFlags = Array.Empty<string>(), flags = Array.Empty<string>(), objectives = Array.Empty<string>();
        public ItemAmount[] requiredItems = Array.Empty<ItemAmount>(), rewardItems = Array.Empty<ItemAmount>(), acceptRewardItems = Array.Empty<ItemAmount>();
        public int rewardCoins;
        public bool consumeItems, rewardOnce = true;
        public string giverCityId, completeCityId, pendingReason;
        public bool pending;
        public bool confirmDelivery;
        public string confirmationText;
        public string[] acceptQuests=Array.Empty<string>(),requiredQuests=Array.Empty<string>();
        public ItemAmount[] reserveItems=Array.Empty<ItemAmount>();
        public string[] onAcceptFlags=Array.Empty<string>();
        public string[] requiredFlagLabels=Array.Empty<string>();
    }
    [Serializable] public class PuzzleDefinition
    {
        public string id, cityId, nodeId, title, clue, answer, mode, completionFlag, sourceStatus;
        public string[] options = Array.Empty<string>(), requiredFlags = Array.Empty<string>(), flags = Array.Empty<string>();
        public ItemAmount[] rewardItems = Array.Empty<ItemAmount>();
        public ItemAmount[] requiredItems = Array.Empty<ItemAmount>();
        public int rewardCoins;
    }
    [Serializable] public class EventDefinition
    {
        public string id, cityId, nodeId, type, title, text, actor, npcId, questId, itemId, enemyId, puzzleId;
        public string requirement, completionFlag, rewardFlag, destinationCity, destinationNode;
        public string alternateItemId, alternateFlag, alternateChoice, alternateText, backgroundId, sourceStatus, sceneId;
        public int itemCount, rewardCoins;
        public bool mandatory, autoCollect, oneShot, legacyTutorial;
        public bool additive;
        public bool refillIfMissing;
        public bool alternateKeepItem;
        public bool consumeRequiredItems;
        public int alternateSteps;
        public string[] onTalkFlags=Array.Empty<string>();
        public string[] anyFlags=Array.Empty<string>();
        public string missingText,completedText;
        public string alternateCompletedText,battleCompletedText;
        public string[] alternateFlags=Array.Empty<string>(),battleFlags=Array.Empty<string>(),endingPages=Array.Empty<string>();
        public string[] requiredFlags = Array.Empty<string>(), flags = Array.Empty<string>();
        public ItemAmount[] requiredItems = Array.Empty<ItemAmount>(), rewardItems = Array.Empty<ItemAmount>();
        public ShopOffer[] shopItems = Array.Empty<ShopOffer>();
    }
    [Serializable] public class ShopOffer { public string itemId; public int amount=1,price; }
    [Serializable] public class NodeContentDefinition
    {
        public string id, name, kind, eventId, backgroundId, gateCondition;
        public Vector2 position;
        // Marker position stays canonical; arrival is authored on visible ground.
        // Explicit opt-in keeps existing cities and old saves unchanged.
        public bool hasArrivalPosition;
        public Vector2 arrivalPosition;
        public int revealStage, unlockStage;
        public int[] previewStages = Array.Empty<int>();
        public bool isKeyGate;
        public bool revealWhenPreviouslyVisited;
        public string[] requiredFlags = Array.Empty<string>();
    }
    [Serializable] public class RouteDefinition
    {
        public string id, from, to, travel, status;
        public bool bidirectional = true;
        public Vector2[] waypoints = Array.Empty<Vector2>();
        public string[] requiredFlags = Array.Empty<string>();
    }
    [Serializable] public class GateDefinition
    {
        public string nodeId, condition;
        public int unlockStage;
    }
    [Serializable] public class CoveredWalkwayDefinition
    {
        public string id;
        public Vector2[] polygon = Array.Empty<Vector2>();
    }
    [Serializable] public class CityDefinition
    {
        public string id, name, backgroundId, sourceBackground, spawnNodeId, completionFlag, opening, story, twist, ending;
        public bool runtimeReady, routesReviewed;
        public string mapTitle;
        // Empty preserves the existing full chapter. Partial releases expose only
        // authored nodes, independently of the player's saved discovery stage.
        public string[] runtimeNodeIds=Array.Empty<string>();
        public string[] unlockFlags = Array.Empty<string>();
        public NodeContentDefinition[] nodes = Array.Empty<NodeContentDefinition>();
        public RouteDefinition[] edges = Array.Empty<RouteDefinition>();
        public GateDefinition[] gates = Array.Empty<GateDefinition>();
        // Only actual roofed passages, never a way to conceal invalid terrain.
        public CoveredWalkwayDefinition[] coveredWalkways = Array.Empty<CoveredWalkwayDefinition>();
    }
    [Serializable] public class ResonanceDefinition
    {
        public string sourceFlag, sourceItem, destinationCity, effect;
    }
    [Serializable] public class CheckInDefinition
    {
        public string id, cityId, nodeId, title, description, completionFlag, sourceStatus;
        public string[] requiredFlags=Array.Empty<string>();
        public int rewardCoins;
        public ItemAmount[] rewardItems=Array.Empty<ItemAmount>();
    }

    /// <summary>Shared content catalog. Gameplay services own state; these records remain immutable at runtime.</summary>
    [Serializable] public class ContentDatabase
    {
        public int schemaVersion;
        public string source;
        public CityDefinition[] cities = Array.Empty<CityDefinition>();
        public EventDefinition[] events = Array.Empty<EventDefinition>();
        public ItemDefinition[] items = Array.Empty<ItemDefinition>();
        public EnemyDefinition[] enemies = Array.Empty<EnemyDefinition>();
        public QuestDefinition[] quests = Array.Empty<QuestDefinition>();
        public PuzzleDefinition[] puzzles = Array.Empty<PuzzleDefinition>();
        public ResonanceDefinition[] resonances = Array.Empty<ResonanceDefinition>();
        public CheckInDefinition[] checkIns = Array.Empty<CheckInDefinition>();
        static ContentDatabase cached;

        public static ContentDatabase Load()
        {
            if (cached != null) return cached;
            var json = Resources.Load<TextAsset>("Content/GameplayContent");
            if (json == null) throw new InvalidOperationException("Missing runtime content: Content/GameplayContent.json");
            cached = JsonUtility.FromJson<ContentDatabase>(json.text);
            if (cached == null || cached.cities == null || cached.cities.Length == 0)
                throw new InvalidOperationException("Gameplay content catalog is empty.");
            return cached;
        }

        public CityDefinition City(string id) => cities.FirstOrDefault(value => value.id == id);
        public EventDefinition Event(string cityId, string eventId) => events.FirstOrDefault(value => value.cityId == cityId && value.id == eventId);
        public ItemDefinition Item(string id) => items.FirstOrDefault(value => value.id == id);
        public EnemyDefinition Enemy(string id) => enemies.FirstOrDefault(value => value.id == id);
        public QuestDefinition Quest(string id) => quests.FirstOrDefault(value => value.id == id);
        public PuzzleDefinition Puzzle(string id) => puzzles.FirstOrDefault(value => value.id == id);
        public CheckInDefinition CheckIn(string cityId,string nodeId) => checkIns.FirstOrDefault(value=>value.cityId==cityId&&value.nodeId==nodeId);
    }
}
