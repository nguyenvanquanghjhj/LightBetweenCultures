using System.Collections.Generic;
using System.Text;

namespace LightBetweenCultures
{
    public sealed class QuestManager
    {
        private readonly GameBootstrap game;
        public QuestManager(GameBootstrap owner) { game = owner; }
        public bool Active => game.Save.accepted.Contains(game.Data.quest.id);
        public bool Completed => game.Save.completed.Contains(game.Data.quest.id);
        public bool IsActive(string id) => game.Save.accepted.Contains(id);
        public bool IsCompleted(string id) => game.Save.completed.Contains(id);

        public bool CanAccept(string id)
        {
            var definition = game.Content.Quest(id);
            return definition != null && !IsActive(id) && !IsCompleted(id) && HasFlags(definition.acceptFlags) && HasQuests(definition.acceptQuests) &&
                (string.IsNullOrEmpty(definition.giverCityId)||definition.giverCityId==game.Data.cityId);
        }

        public bool Accept(string id)
        {
            var quest = game.Content.Quest(id);
            if (!CanAccept(id)) return false;
            if(!string.IsNullOrEmpty(quest.giverCityId)&&game.Data.nodes[game.Save.node].id!=quest.giverNode)return false;
            game.Save.accepted.Add(id);
            game.World.Set("quest:" + id);
            foreach(var flag in quest.onAcceptFlags??new string[0])game.World.Set(flag);
            foreach (var item in quest.acceptRewardItems ?? new ItemAmount[0])
                game.Inventory.GrantOnce("quest-accept:" + id + ":" + item.itemId, item.itemId, item.amount);
            game.Persist();
            return true;
        }

        public bool CanComplete(string id)
        {
            var quest = game.Content.Quest(id);
            if (quest == null || quest.pending || !IsActive(id) || IsCompleted(id) || !HasFlags(quest.requiredFlags) || !HasQuests(quest.requiredQuests) ||
                (!string.IsNullOrEmpty(quest.completeCityId)&&quest.completeCityId!=game.Data.cityId)) return false;
            foreach (var item in quest.requiredItems ?? new ItemAmount[0])
            {
                var reserved=System.Array.Find(quest.reserveItems??new ItemAmount[0],value=>value.itemId==item.itemId)?.amount??0;
                if (Count(item.itemId) < item.amount+reserved) return false;
            }
            return true;
        }

        // The caller must identify the completion location. Owning an item never
        // completes a delivery quest until the player explicitly hands it over.
        public bool Complete(string id, string nodeId = null)
        {
            var quest = game.Content.Quest(id);
            if (!CanComplete(id) || (!string.IsNullOrEmpty(quest.completeAtNode) && quest.completeAtNode != nodeId)) return false;
            if (game.Save.node < 0 || game.Save.node >= game.Data.nodes.Length || game.Data.nodes[game.Save.node].id != nodeId) return false;
            if (quest.consumeItems)
                foreach (var item in quest.requiredItems ?? new ItemAmount[0]) Consume(item.itemId, item.amount);
            game.Save.completed.Add(id);
            if (!string.IsNullOrEmpty(quest.completionFlag)) game.World.Set(quest.completionFlag);
            foreach (var flag in quest.flags ?? new string[0]) game.World.Set(flag);
            foreach (var item in quest.rewardItems ?? new ItemAmount[0])
                game.Inventory.GrantOnce("quest:" + id + ":" + item.itemId, item.itemId, item.amount);
            game.Save.coins += quest.rewardCoins;
            game.Persist();
            return true;
        }

        public string Progress(string id)
        {
            var quest = game.Content.Quest(id);
            if (quest == null) return "Nhiệm vụ chưa có dữ liệu.";
            if (IsCompleted(id)) return quest.thanks;
            if (!IsActive(id)) return quest.offer;
            var result = new StringBuilder();
            result.Append(quest.reminder);
            for(var i=0;i<(quest.requiredFlagLabels?.Length??0)&&i<(quest.requiredFlags?.Length??0);i++)
                result.Append("\n").Append(game.World.Has(quest.requiredFlags[i])?"✓ ":"• ").Append(quest.requiredFlagLabels[i]);
            if(quest.pending)result.Append("\n").Append(quest.pendingReason);
            foreach (var item in quest.requiredItems ?? new ItemAmount[0])
            {
                var definition = game.Content.Item(item.itemId);
                result.Append("\n").Append(definition == null ? item.itemId : definition.name)
                    .Append(": ").Append(System.Math.Min(Count(item.itemId), item.amount)).Append(" / ").Append(item.amount);
            }
            foreach(var item in quest.reserveItems??new ItemAmount[0])
                result.Append("\nGiữ lại cho chuyến giao lưu: ").Append(game.Content.Item(item.itemId)?.name??item.itemId).Append(" × ").Append(item.amount);
            return result.ToString();
        }

        public string Journal
        {
            get
            {
                var result = new StringBuilder();
                foreach (var id in game.Save.accepted)
                {
                    var quest = game.Content.Quest(id);
                    if (quest == null) continue;
                    result.Append(IsCompleted(id) ? "✓ " : "• ").Append(quest.title).Append("\n")
                        .Append(Progress(id)).Append("\n\n");
                }
                return result.Length == 0 ? "Chưa có nhiệm vụ. Hãy gặp cô Mai ở H2." : result.ToString().TrimEnd();
            }
        }

        private bool HasFlags(string[] flags)
        {
            foreach (var flag in flags ?? new string[0]) if (!game.World.Has(flag)) return false;
            return true;
        }
        private bool HasQuests(string[] quests)
        {
            foreach(var id in quests??new string[0])if(!IsCompleted(id))return false;
            return true;
        }
        private int Count(string id) => id == "coin" ? game.Save.coins : game.Inventory.Count(id);
        private void Consume(string id, int amount)
        {
            if (id == "coin") game.Save.coins -= amount;
            else game.Inventory.TryConsume(id, amount);
        }

        public string TalkToMai()
        {
            var quest = game.Data.quest;
            if (Active && !IsActive("HQ01") && !IsCompleted("HQ01")) Accept("HQ01");
            if (Completed) return quest.thanks;
            if (!Active) { game.Save.accepted.Add(quest.id); Accept("HQ01"); game.Persist(); return quest.offer; }
            // Existing saves can have accepted the tutorial before HQ01 existed.
            if (!IsActive("HQ01") && !IsCompleted("HQ01")) Accept("HQ01");
            if (!game.Inventory.Has(quest.item)) return quest.reminder;
            game.Inventory.Consume(quest.item); game.Save.completed.Add(quest.id);
            if (!game.Save.flags.Contains(quest.reward)) game.Save.flags.Add(quest.reward);
            game.Persist(); return quest.thanks;
        }
        public bool CollectPart()
        {
            if (!Active || Completed || !game.Inventory.Collect(game.Data.quest.item)) return false;
            game.Inventory.GrantOnce("h3-lamp-eye", "lamp_eye", 1);
            game.World.Set("hoi_an.lamp_eye_found");
            game.Persist(); return true;
        }
        public string Status => IsCompleted("HQ01") ? "Trạm Hội An đã sáng · Huế và Jeonju đã mở." : Completed ? "Đèn tiệm đã sáng · Tiếp tục HQ01: lõi giấy H13, chìa H5, trạm H7." : !Active ? "Gặp cô Mai tại H2 để nhận nhiệm vụ." : game.Inventory.Has(game.Data.quest.item) ? "Đã có Mắt Đèn. Trở về H2 gặp cô Mai." : "Tìm Mắt Đèn trong hẻm H3.";
    }
}
