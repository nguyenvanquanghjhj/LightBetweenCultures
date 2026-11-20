using System;
using System.Collections.Generic;

namespace LightBetweenCultures
{
    /// <summary>One resolution path and one reward ledger for combat and peaceful encounters.</summary>
    public sealed class EncounterManager
    {
        private readonly GameBootstrap game;
        public EncounterManager(GameBootstrap owner) { game = owner; }
        public static string Key(EventDefinition definition) => definition.cityId + ":" + definition.id;
        public bool Resolved(EventDefinition definition) => definition != null &&
            (game.Save.resolvedEvents.Contains(Key(definition)) ||
             !string.IsNullOrEmpty(definition.completionFlag) && game.World.Has(definition.completionFlag));

        public bool Resolve(EventDefinition definition, string choice)
        {
            if (definition == null || Resolved(definition)) return false;
            var enemy = game.Content.Enemy(definition.enemyId);
            var key = Key(definition);
            game.Save.resolvedEvents.Add(key);
            game.Save.choices.Add(key + ":" + choice);
            if (choice == "battle" && enemy != null)
            {
                if (!game.Save.defeatedEnemies.Contains(enemy.id)) game.Save.defeatedEnemies.Add(enemy.id);
                if (!string.IsNullOrEmpty(enemy.completionFlag)) game.World.Set(enemy.completionFlag);
            }
            if (!string.IsNullOrEmpty(definition.completionFlag)) game.World.Set(definition.completionFlag);
            if (!string.IsNullOrEmpty(definition.rewardFlag)) game.World.Set(definition.rewardFlag);
            foreach (var flag in definition.flags ?? Array.Empty<string>()) game.World.Set(flag);
            foreach(var flag in (choice=="battle"?definition.battleFlags:definition.alternateFlags)??Array.Empty<string>())game.World.Set(flag);

            // Source events and enemy drops can describe the same reward. Merge
            // by item ID before granting so a link key or quest parcel is unique.
            var amounts = new Dictionary<string, int>();
            AddRewards(amounts, definition.rewardItems);
            if (enemy != null) AddRewards(amounts, enemy.rewardItems);
            foreach (var reward in amounts)
                game.Inventory.GrantOnce("encounter:" + key + ":" + reward.Key, reward.Key, reward.Value);
            game.Save.coins += Math.Max(definition.rewardCoins, enemy == null ? 0 : enemy.rewardCoins);
            game.Persist();
            return true;
        }

        private static void AddRewards(Dictionary<string, int> amounts, ItemAmount[] rewards)
        {
            foreach (var reward in rewards ?? Array.Empty<ItemAmount>())
                if (!string.IsNullOrEmpty(reward.itemId))
                    amounts[reward.itemId] = amounts.TryGetValue(reward.itemId, out var existing)
                        ? Math.Max(existing, reward.amount) : reward.amount;
        }
    }
}
