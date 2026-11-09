using System;
using System.Collections.Generic;
using System.Linq;

namespace LightBetweenCultures
{
    public sealed class EventOption
    {
        public string id, label;
        public bool enabled;
        public EventOption(string key, string caption, bool available = true) { id = key; label = caption; enabled = available; }
    }
    public sealed class EventView
    {
        public string title, speaker, text;
        public List<EventOption> options = new List<EventOption>();
    }
    public sealed class EventResult
    {
        public string message = "", scene, portalCity;
        public bool changed, openBattle, openPuzzle, openEnding, stop;
        public string checkInId;
        public ItemAmount[] receivedItems=Array.Empty<ItemAmount>();
    }

    /// <summary>Typed event dispatch shared by every city; no node-number switches.</summary>
    public sealed class NodeEventManager
    {
        private readonly GameBootstrap game;
        private readonly EncounterManager encounters;
        public NodeEventManager(GameBootstrap owner) { game = owner; encounters = new EncounterManager(owner); }
        public EventDefinition Definition(string eventId) => game.Content.Event(game.Data.cityId, eventId);
        private EventDefinition At(int node) => node >= 0 && node < game.Data.nodes.Length ? Definition(game.Data.nodes[node].eventId) : null;
        private IEnumerable<EventDefinition> Additional(int node)=>game.Content.events.Where(e=>e.additive&&e.cityId==game.Data.cityId&&e.nodeId==game.Data.nodes[node].id);
        public CheckInDefinition CheckInAt(int node)=>node>=0&&node<game.Data.nodes.Length?game.Content.CheckIn(game.Data.cityId,game.Data.nodes[node].id):null;
        public bool CanCheckIn(int node)
        {
            var entry=CheckInAt(node);
            return game.Discovery.IsVisible(node)&&entry!=null&&!game.World.Has(entry.completionFlag)&&
                Array.TrueForAll(entry.requiredFlags??Array.Empty<string>(),game.World.Has);
        }

        public bool Resolved(EventDefinition definition)
        {
            if (definition == null) return false;
            if (definition.type == "encounter") return encounters.Resolved(definition);
            if (definition.type == "puzzle") return game.Puzzles.Solved(definition.puzzleId);
            if (definition.type == "beacon") return game.Quests.IsCompleted(definition.questId);
            return game.Save.resolvedEvents.Contains(EncounterManager.Key(definition)) ||
                   !string.IsNullOrEmpty(definition.completionFlag) && game.World.Has(definition.completionFlag);
        }

        /// <summary>Called after EVERY traversed edge, before travelling onward.</summary>
        public bool OnArrive(int node, bool final)
        {
            if(!game.Discovery.IsVisible(node)) return false;
            var definition = At(node);
            if (definition == null) return false;
            if (definition.autoCollect && !Resolved(definition) && CanUse(definition))
            {
                Grant(definition);
                return false;
            }
            if (definition.mandatory && !Resolved(definition)) return true;
            // Optional NPCs are signalled at their node and never hijack a route.
            // A final destination opens interaction; junctions are navigation.
            return final && (definition.type != "junction" && definition.type != "start" || CanCheckIn(node));
        }

        public EventView BuildOptions(int node)
        {
            if(!game.Discovery.IsVisible(node)) return new EventView { title="Chưa khám phá", text="Địa điểm này chưa được phát hiện." };
            var definition = At(node);
            if (definition == null) return new EventView { title = "Địa điểm", text = "Địa điểm này chưa có sự kiện runtime." };
            var view = new EventView { title = definition.title, speaker = definition.actor, text = definition.text };
            var available = CanUse(definition);
            var resolved = Resolved(definition);
            if(resolved&&!string.IsNullOrEmpty(definition.completedText))view.text=definition.completedText;
            if (!available) view.text += "\n" + MissingRequirements(definition);
            switch (definition.type)
            {
                case "npc":
                    var quest = game.Content.Quest(definition.questId);
                    if (quest == null) { view.options.Add(new EventOption("talk", "Nói chuyện", available)); break; }
                    view.text = game.Quests.Progress(quest.id);
                    if (definition.legacyTutorial && game.Quests.Active && !game.Quests.Completed)
                        view.text += "\n" + game.Quests.Status;
                    if (!game.Quests.IsActive(quest.id) && !game.Quests.IsCompleted(quest.id))
                        view.options.Add(new EventOption("accept", "Nhận nhiệm vụ", available && game.Quests.CanAccept(quest.id)));
                    else if (definition.legacyTutorial && !game.Quests.Completed && game.Inventory.Has(game.Data.quest.item))
                        view.options.Add(new EventOption("tutorial_return", "Trả linh kiện mở đầu", available));
                    else if (!game.Quests.IsCompleted(quest.id) && quest.completeAtNode == definition.nodeId)
                        view.options.Add(new EventOption("complete", "Giao vật phẩm / hoàn thành", available && game.Quests.CanComplete(quest.id)));
                    view.options.Add(new EventOption("talk", "Hỏi tiến độ", available));
                    break;
                case "encounter":
                    if (resolved)
                    {
                        view.text=ResolvedEncounterText(definition);
                        if(!string.IsNullOrEmpty(definition.alternateCompletedText)||!string.IsNullOrEmpty(definition.battleCompletedText))view.options.Add(new EventOption("talk","Nghe phản hồi",available));
                    }
                    else
                    {
                        view.options.Add(new EventOption("battle", "Giao chiến", available));
                        if(definition.alternateSteps>0)
                        {
                            var step=PreparationCount(definition);
                            view.text+="\nBẫy sáng: "+step+" / "+definition.alternateSteps+" tấm phản quang.";
                            view.options.Add(new EventOption("prepare",definition.alternateText,available));
                        }
                        if (!string.IsNullOrEmpty(definition.alternateFlag) || !string.IsNullOrEmpty(definition.alternateItemId))
                            view.options.Add(new EventOption("alternate", definition.alternateText,
                                available && CanAlternate(definition)));
                    }
                    break;
                case "puzzle":
                    if (resolved) view.text = "Câu đố đã được giải. Phần thưởng đã được lưu.";
                    else view.options.Add(new EventOption("puzzle", "Giải câu đố", available && CanStartPuzzle(definition.puzzleId)));
                    break;
                case "exploration": view.options.Add(new EventOption("scene", "Vào khám phá", available)); break;
                case "beacon":
                    view.text = resolved ? string.IsNullOrEmpty(definition.completedText)?"Ánh sáng đã được khôi phục. Bạn có thể ở lại khám phá hoặc chọn tuyến trên World Map.":definition.completedText
                        : game.Quests.Progress(definition.questId);
                    if (!resolved) view.options.Add(new EventOption("restore", "Lắp linh kiện · Thắp sáng", available && game.Quests.CanComplete(definition.questId)));
                    break;
                case "portal": view.options.Add(new EventOption("portal", "Đến thành phố tiếp theo", available)); break;
                case "epilogue":
                    view.options.Add(new EventOption("ending",resolved?"Xem lại lễ hội / credits":"Bước vào lễ hội · Xem lời kết",available));
                    if(EndingStarted(definition)&&!resolved)view.options.Add(new EventOption("ending_next","Tiếp tục lời kết",available));
                    break;
                case "chest":
                case "item":
                case "secret":
                    if (resolved) view.text = "Bạn đã kiểm tra nơi này. Rương không còn vật phẩm mới.";
                    else view.options.Add(new EventOption("collect", "Mở / kiểm tra", available));
                    if (!string.IsNullOrEmpty(definition.questId) && game.Content.Quest(definition.questId)?.completeAtNode==definition.nodeId && game.Quests.IsActive(definition.questId) && !game.Quests.IsCompleted(definition.questId))
                        view.options.Add(new EventOption("complete", "Hoàn thành nhiệm vụ", available && game.Quests.CanComplete(definition.questId)));
                    break;
                case "start": view.options.Add(new EventOption("rest", "Nghỉ ngơi · Hồi HP", available)); break;
                default: view.options.Add(new EventOption("talk", "Quan sát / nói chuyện", available)); break;
            }
            var checkIn=CheckInAt(node);
            if(CanCheckIn(node))view.options.Insert(0,new EventOption("checkin","Ghi dấu địa danh"));
            else if(checkIn!=null&&game.World.Has(checkIn.completionFlag))view.options.Add(new EventOption("checkin_view","Xem địa danh đã ghi dấu"));
            var primaryQuest=game.Content.Quest(definition.questId);
            var receivingPriority=primaryQuest==null||!game.Quests.IsActive(primaryQuest.id)?0:game.Quests.IsCompleted(primaryQuest.id)?1:game.Quests.CanComplete(primaryQuest.id)?3:2;
            var missingDeliveryNotes=new List<string>();
            if(primaryQuest!=null&&game.Quests.IsActive(primaryQuest.id)&&!game.Quests.IsCompleted(primaryQuest.id)&&!game.Quests.CanComplete(primaryQuest.id))missingDeliveryNotes.Add(game.Quests.Progress(primaryQuest.id));
            foreach(var extra in Additional(node))
            {
                if(!CanUse(extra))continue;
                if(extra.type=="quest")
                {
                    var q=game.Content.Quest(extra.questId);if(q==null)continue;
                    if(q.completeCityId==game.Data.cityId&&q.completeAtNode==extra.nodeId&&(game.Quests.IsActive(q.id)||game.Quests.IsCompleted(q.id)))
                    {
                        var priority=game.Quests.IsCompleted(q.id)?1:game.Quests.CanComplete(q.id)?3:2;
                        var response=game.Quests.IsCompleted(q.id)?string.IsNullOrEmpty(extra.completedText)?q.thanks:extra.completedText:extra.text+"\n\n"+game.Quests.Progress(q.id);
                        if(game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id)&&!game.Quests.CanComplete(q.id)&&!string.IsNullOrEmpty(extra.missingText))response+="\n"+extra.missingText;
                        if(game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id)&&!game.Quests.CanComplete(q.id))missingDeliveryNotes.Add(response);
                        // Completed optional errands cannot hide current deliveries.
                        if(priority>receivingPriority){view.speaker=extra.actor;view.text=response;receivingPriority=priority;}
                        else if(priority==receivingPriority)view.text+="\n\n"+q.title+"\n"+response;
                    }
                    if(game.Quests.CanAccept(q.id))view.options.Add(new EventOption("accept:"+extra.id,"Nhận · "+q.title));
                    else if(game.Quests.IsActive(q.id)&&!game.Quests.IsCompleted(q.id))
                    {
                        if(q.completeAtNode==extra.nodeId&&(string.IsNullOrEmpty(q.completeCityId)||q.completeCityId==game.Data.cityId))
                            view.options.Insert(0,new EventOption("complete:"+extra.id,"Giao đồ · "+q.title,game.Quests.CanComplete(q.id)));
                        view.options.Add(new EventOption("progress:"+extra.id,"Hỏi · "+q.title));
                    }
                    if(game.Quests.IsActive(q.id)&&(q.completeCityId!=game.Data.cityId||q.completeAtNode!=extra.nodeId))view.text+="\n\n"+q.title+"\n"+game.Quests.Progress(q.id);
                }
                else if(extra.type=="item"&&!Resolved(extra))view.options.Add(new EventOption("collect:"+extra.id,extra.title));
                else if(extra.type=="recovery"&&(!Resolved(extra)||extra.refillIfMissing)&&game.Quests.IsActive(extra.questId)&&!game.Quests.IsCompleted(extra.questId)&&!game.Inventory.Has(extra.itemId,Math.Max(1,extra.itemCount)))
                    view.options.Add(new EventOption("recover:"+extra.id,extra.title));
            }
            foreach(var offer in definition.shopItems??Array.Empty<ShopOffer>())
                view.options.Add(new EventOption("buy:"+offer.itemId,"Mua "+(game.Content.Item(offer.itemId)?.name??offer.itemId)+" · "+offer.price+" xu",available&&offer.price>0&&game.Save.coins>=offer.price));
            // A ready optional errand must not erase another active delivery's
            // missing-item/recovery directions at the same NPC.
            foreach(var note in missingDeliveryNotes)if(!view.text.Contains(note))view.text+="\n\n"+note;
            return view;
        }

        public EventResult Execute(int node, string optionId)
        {
            if(!game.Discovery.IsVisible(node)) return new EventResult { message="Địa điểm này chưa được phát hiện." };
            var definition = At(node);
            var result = new EventResult();
            if (definition == null) { result.message = "Sự kiện chưa được định nghĩa."; return result; }
            if (node != game.Save.node) { result.message = "Hãy tới địa điểm này trước khi tương tác."; return result; }
            // UI is not the security boundary: re-check choices at execution.
            var option = BuildOptions(node).options.Find(value => value.id == optionId);
            if (option == null || !option.enabled) { result.message = "Chưa đủ điều kiện cho lựa chọn này."; return result; }
            var before=game.Content.items.ToDictionary(i=>i.id,i=>game.Inventory.Count(i.id));
            if(optionId.StartsWith("buy:"))
            {
                var offer=Array.Find(definition.shopItems??Array.Empty<ShopOffer>(),o=>o.itemId==optionId.Substring(4));
                if(offer!=null&&offer.price>0&&offer.amount>0&&game.Content.Item(offer.itemId)!=null&&game.Inventory.TryConsume("coin",offer.price))
                {game.Inventory.Add(offer.itemId,offer.amount);result.changed=true;result.message="Đã mua "+game.Content.Item(offer.itemId).name+".";}
                result.receivedItems=ReceivedSince(before);game.Persist();return result;
            }
            if(optionId.Contains(":"))
            {
                var split=optionId.IndexOf(':');var action=optionId.Substring(0,split);
                var extra=Additional(node).FirstOrDefault(e=>e.id==optionId.Substring(split+1));
                if(extra==null||!CanUse(extra))return result;
                switch(action)
                {
                    case "accept":result.changed=game.Quests.Accept(extra.questId);break;
                    case "complete":result.changed=game.Quests.Complete(extra.questId,extra.nodeId);break;
                    case "collect":result.changed=Grant(extra);break;
                    case "recover":
                        if(extra.refillIfMissing&&game.Quests.IsActive(extra.questId)&&!game.Quests.IsCompleted(extra.questId)&&!game.Inventory.Has(extra.itemId,Math.Max(1,extra.itemCount)))
                        {game.Inventory.Add(extra.itemId,Math.Max(1,extra.itemCount)-game.Inventory.Count(extra.itemId));game.World.Set(extra.completionFlag);result.changed=true;}
                        else result.changed=Grant(extra);
                        break;
                }
                result.message=extra.type=="quest"?game.Quests.Progress(extra.questId):extra.text;
                result.receivedItems=ReceivedSince(before);game.Persist();return result;
            }
            switch (optionId)
            {
                case "checkin":
                    var photo=CheckInAt(node);
                    if(CanCheckIn(node)&&game.World.Set(photo.completionFlag))
                    {
                        game.Inventory.GrantOnce("checkin:"+photo.id,"coin",photo.rewardCoins);
                        foreach(var item in photo.rewardItems??Array.Empty<ItemAmount>())game.Inventory.GrantOnce("checkin:"+photo.id+":"+item.itemId,item.itemId,item.amount);
                        result.changed=true;result.checkInId=photo.id;result.message="Đã ghi dấu "+photo.title+".";
                    }
                    break;
                case "checkin_view":result.checkInId=CheckInAt(node)?.id;break;
                case "accept":
                    if (definition.legacyTutorial)
                    {
                        if (!game.Quests.Active) result.message = game.Quests.TalkToMai();
                        result.changed = game.Quests.IsActive(definition.questId) || game.Quests.Accept(definition.questId);
                    }
                    else result.changed = game.Quests.Accept(definition.questId);
                    result.message = game.Quests.Progress(definition.questId);
                    break;
                case "tutorial_return":
                    result.message = game.Quests.TalkToMai(); result.changed = game.Quests.Completed; break;
                case "complete":
                    result.changed = game.Quests.Complete(definition.questId, definition.nodeId);
                    if (result.changed) Grant(definition);
                    result.message = game.Quests.Progress(definition.questId); break;
                case "battle":
                    result.openBattle = game.Battles.Start(definition); result.stop = result.openBattle;
                    result.message = game.Battles.Log; break;
                case "prepare":
                    var prepared=PreparationCount(definition)+1;
                    if(prepared<=definition.alternateSteps)
                    {
                        game.Save.choices.Add(EncounterManager.Key(definition)+":prepare:"+prepared);
                        result.changed=true;
                        result.message="Đã bật tấm phản quang "+prepared+" / "+definition.alternateSteps+".";
                        if(prepared==definition.alternateSteps)
                        {encounters.Resolve(definition,definition.alternateChoice);result.message="Bẫy sáng đã khép lại. Bot để vật phẩm lại rồi rút lui.";}
                    }
                    break;
                case "alternate":
                    if (!CanAlternate(definition)) break;
                    if (!definition.alternateKeepItem && !string.IsNullOrEmpty(definition.alternateItemId) && !game.Inventory.TryConsume(definition.alternateItemId, 1)) break;
                    result.changed = encounters.Resolve(definition, string.IsNullOrEmpty(definition.alternateChoice) ? "alternate" : definition.alternateChoice);
                    result.message = "Thử thách đã được giải quyết bằng " + definition.alternateText + ". Bạn được quyền đi tiếp hoặc quay lại.";
                    break;
                case "puzzle":
                    result.openPuzzle = game.Puzzles.Start(definition.puzzleId); result.stop = result.openPuzzle;
                    result.message = game.Puzzles.Log; break;
                case "scene": result.scene = definition.sceneId; break;
                case "restore":
                    result.changed = game.Quests.Complete(definition.questId, definition.nodeId);
                    if (result.changed) Grant(definition);
                    result.message = result.changed ? string.IsNullOrEmpty(definition.completedText)?"Ánh sáng đã trở lại! Chọn tuyến tiếp theo trên World Map hoặc ở lại khám phá.":definition.completedText : "Vẫn còn thiếu linh kiện hoặc tiến độ nhiệm vụ.";
                    break;
                case "portal": result.portalCity = definition.destinationCity; break;
                case "ending":
                    if(!EndingStarted(definition))game.Save.choices.Add(EncounterManager.Key(definition)+":ending-started");
                    result.openEnding=true;result.changed=true;break;
                case "ending_next":
                    var page=EndingPage(definition);
                    if(page<definition.endingPages.Length)
                    {
                        game.Save.choices.Add(EncounterManager.Key(definition)+":ending-page:"+(page+1));result.changed=true;
                        if(page+1==definition.endingPages.Length)
                        {
                            game.World.Set(definition.completionFlag);
                            game.Quests.Complete(definition.questId,definition.nodeId);
                        }
                    }
                    result.openEnding=true;break;
                case "collect":
                    result.changed = Grant(definition);
                    result.message = result.changed ? "Đã kiểm tra và nhận vật phẩm. Tiến độ được lưu." : "Vật phẩm ở đây đã được nhận.";
                    break;
                case "rest":
                    game.Save.playerHP = 100; game.Save.lightEnergy = 3; result.changed = true;
                    result.message = "Lumi đã nghỉ ngơi: 100 HP, 3 năng lượng."; break;
                case "talk":
                    if(definition.type=="encounter"&&Resolved(definition)){result.message=ResolvedEncounterText(definition);break;}
                    foreach(var flag in definition.onTalkFlags??Array.Empty<string>())result.changed|=game.World.Set(flag);
                    if (string.IsNullOrEmpty(definition.questId)) { result.changed = Grant(definition); result.message = definition.text; }
                    else result.message = game.Quests.Progress(definition.questId);
                    break;
            }
            result.receivedItems=ReceivedSince(before);
            game.Persist();
            return result;
        }
        private ItemAmount[] ReceivedSince(Dictionary<string,int> before)=>game.Content.items.Select(i=>new ItemAmount{itemId=i.id,amount=game.Inventory.Count(i.id)-before[i.id]}).Where(i=>i.amount>0).ToArray();
        string ResolvedEncounterText(EventDefinition definition)
        {
            var text=game.Save.choices.Contains(EncounterManager.Key(definition)+":battle")?definition.battleCompletedText:definition.alternateCompletedText;
            return string.IsNullOrEmpty(text)?"Đối thủ đã được giải quyết. Con đường này an toàn.":text;
        }
        bool EndingStarted(EventDefinition definition)=>game.Save.choices.Contains(EncounterManager.Key(definition)+":ending-started");
        public int EndingPage(EventDefinition definition)
        {
            var count=0;while(count<(definition.endingPages?.Length??0)&&game.Save.choices.Contains(EncounterManager.Key(definition)+":ending-page:"+(count+1)))count++;
            return count;
        }

        public bool CanUse(EventDefinition definition)
        {
            if (definition == null) return false;
            if (!string.IsNullOrEmpty(definition.requirement) && !game.World.Has(definition.requirement)) return false;
            foreach (var flag in definition.requiredFlags ?? Array.Empty<string>()) if (!game.World.Has(flag)) return false;
            if(definition.anyFlags!=null&&definition.anyFlags.Length>0&&!definition.anyFlags.Any(game.World.Has))return false;
            foreach (var item in definition.requiredItems ?? Array.Empty<ItemAmount>())
                if ((item.itemId == "coin" ? game.Save.coins : game.Inventory.Count(item.itemId)) < item.amount) return false;
            return true;
        }

        private bool CanAlternate(EventDefinition definition) =>
            (string.IsNullOrEmpty(definition.alternateFlag) || game.World.Has(definition.alternateFlag)) &&
            (string.IsNullOrEmpty(definition.alternateItemId) || game.Inventory.Has(definition.alternateItemId, 1));

        private int PreparationCount(EventDefinition definition)
        {
            var count=0;var prefix=EncounterManager.Key(definition)+":prepare:";
            while(count<definition.alternateSteps&&game.Save.choices.Contains(prefix+(count+1)))count++;
            return count;
        }

        private bool CanStartPuzzle(string id)
        {
            var puzzle = game.Content.Puzzle(id);
            if (puzzle == null || game.Puzzles.Solved(id)) return false;
            foreach (var flag in puzzle.requiredFlags ?? Array.Empty<string>()) if (!game.World.Has(flag)) return false;
            foreach (var item in puzzle.requiredItems ?? Array.Empty<ItemAmount>()) if(!game.Inventory.Has(item.itemId,item.amount))return false;
            return true;
        }

        private bool Grant(EventDefinition definition)
        {
            var key = EncounterManager.Key(definition);
            if (game.Save.resolvedEvents.Contains(key)) return false;
            // Atomic exchanges use the same one-time ledger as pickups. Recheck at
            // execution, so an old UI choice cannot consume missing quest items.
            if(!CanUse(definition))return false;
            if(definition.consumeRequiredItems)
                foreach(var item in definition.requiredItems??Array.Empty<ItemAmount>())
                    if(!game.Inventory.TryConsume(item.itemId,item.amount))return false;
            game.Save.resolvedEvents.Add(key);
            foreach (var reward in definition.rewardItems ?? Array.Empty<ItemAmount>())
                game.Inventory.GrantOnce("event:" + key + ":" + reward.itemId, reward.itemId, reward.amount);
            if (!string.IsNullOrEmpty(definition.itemId))
                game.Inventory.GrantOnce("event:" + key + ":" + definition.itemId, definition.itemId, Math.Max(1, definition.itemCount));
            game.Save.coins += definition.rewardCoins;
            if (!string.IsNullOrEmpty(definition.completionFlag)) game.World.Set(definition.completionFlag);
            if (!string.IsNullOrEmpty(definition.rewardFlag)) game.World.Set(definition.rewardFlag);
            foreach (var flag in definition.flags ?? Array.Empty<string>()) game.World.Set(flag);
            game.Persist();
            return true;
        }

        private string MissingRequirements(EventDefinition definition)
        {
            if(!string.IsNullOrEmpty(definition.missingText))return definition.missingText;
            var text = "Cần:";
            foreach (var flag in definition.requiredFlags ?? Array.Empty<string>())
                if (!game.World.Has(flag)) text += "\n• " + flag;
            foreach (var item in definition.requiredItems ?? Array.Empty<ItemAmount>())
            {
                var itemName = game.Content.Item(item.itemId)?.name ?? item.itemId;
                text += "\n• " + itemName + " × " + item.amount;
            }
            return text;
        }
    }
}
