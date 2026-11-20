using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LightBetweenCultures
{
    [Serializable] public sealed class CatRescueSupply
    { public string id, name, nodeId, description, artId; }
    [Serializable] public sealed class CatRescueChallenge
    {
        public string id, nodeId, mode, title, instructions, hint, fact, imageId;
        public string[] tokens = Array.Empty<string>(), targets = Array.Empty<string>(), tokenArt = Array.Empty<string>();
        public string[] options = Array.Empty<string>(), maze = Array.Empty<string>();
        public int answer, start, exit;
    }
    [Serializable] public sealed class CatRescueChapter
    {
        public string cityId, title, opening, weather, rescueNode, rescueText, badge, postcard, nextCity;
        public string catId, catName, catLine, catTrait;
        public int spriteIndex, rewardCoins;
        public CatRescueSupply[] supplies = Array.Empty<CatRescueSupply>();
        public CatRescueChallenge[] challenges = Array.Empty<CatRescueChallenge>();
        public string[] sourceUrls = Array.Empty<string>();
    }
    [Serializable] public sealed class CatRescueCatalog
    {
        public int version;
        public CatRescueChapter[] chapters = Array.Empty<CatRescueChapter>();
        static CatRescueCatalog cached;
        public static CatRescueCatalog Load()
        {
            if (cached != null) return cached;
            var file = Resources.Load<TextAsset>("Content/CatRescue");
            if (file == null) throw new InvalidOperationException("Missing CatRescue catalog");
            return cached = JsonUtility.FromJson<CatRescueCatalog>(file.text);
        }
        public CatRescueChapter City(string id) => Array.Find(chapters, c => c.cityId == id);
    }
    [Serializable] public sealed class CatChallengeProgress
    {
        public string challengeId;
        public List<int> marks = new List<int>();
        public List<int> path = new List<int>();
        public bool paletteChosen;
        public int palette;
    }
    [Serializable] public sealed class CatRescueProgress
    {
        public string cityId;
        public bool started, rescued;
        public List<string> completed = new List<string>(), supplies = new List<string>();
        public List<CatChallengeProgress> challenges = new List<CatChallengeProgress>();
    }

    // Additive campaign. Never writes legacy quest, discovery, portal or ending proofs.
    // Every accepted interaction is persisted; a failed write rolls back the transaction.
    public sealed class CatRescueManager
    {
        readonly GameBootstrap game;
        public CatRescueCatalog Catalog { get; }
        public string Message { get; private set; }
        public CatRescueManager(GameBootstrap owner) { game = owner; Catalog = CatRescueCatalog.Load(); }
        public CatRescueChapter Current => Catalog.City(game.Save.cityId);
        public CatRescueProgress Progress(string city) => game.Save.catRescues.Find(p => p.cityId == city);
        public bool Rescued(string city) => Progress(city)?.rescued == true;
        public int RescuedCount => Catalog.chapters.Count(c => Rescued(c.cityId));
        public bool Unlocks(string city) => Catalog.chapters.Any(c => c.nextCity == city && Rescued(c.cityId));
        public CatRescueChallenge Next(CatRescueChapter c) => c == null ? null : c.challenges.FirstOrDefault(x => Progress(c.cityId)?.completed.Contains(x.id) != true);
        public CatChallengeProgress State(CatRescueChallenge c) => c == null ? null : Progress(Current.cityId)?.challenges.Find(s => s.challengeId == c.id);
        public bool At(string node) => game.Save.node >= 0 && game.Save.node < game.Data.nodes.Length && game.Data.nodes[game.Save.node].id == node;
        public int TargetIndex()
        {
            var c = Current; var p = c == null ? null : Progress(c.cityId);
            var target = p?.started != true ? game.Data.spawnNodeId : Next(c)?.nodeId;
            if (p?.started == true && target == null)
                target = c.supplies.FirstOrDefault(s => !p.supplies.Contains(s.id))?.nodeId ?? c.rescueNode;
            return p?.rescued == true ? -1 : Array.FindIndex(game.Data.nodes, n => n.id == target);
        }
        public string Guidance()
        {
            var c = Current; var p = c == null ? null : Progress(c.cityId);
            if (c == null) return "";
            if (p?.rescued == true) return c.catName + " đã an toàn · Xem bộ sưu tập mèo";
            if (p?.started != true) return "Có tiếng mèo gọi! Mở Giải cứu mèo để nhận nhiệm vụ.";
            var next = Next(c);
            if (next != null) return "[" + (p.completed.Count + 1) + "/" + c.challenges.Length + "] " + next.title + " · " + Place(next.nodeId);
            var supply = c.supplies.FirstOrDefault(s => !p.supplies.Contains(s.id));
            return supply != null ? "Tìm " + supply.name + " · " + Place(supply.nodeId) : "Đón " + c.catName + " · " + Place(c.rescueNode);
        }
        public string Place(string node) => Array.Find(game.Data.nodes, n => n.id == node)?.name ?? node;
        CatRescueProgress Mutable()
        {
            var p = Progress(Current.cityId);
            if (p == null) { p = new CatRescueProgress { cityId = Current.cityId }; game.Save.catRescues.Add(p); }
            return p;
        }
        CatChallengeProgress MutableState(CatRescueChallenge c)
        {
            var p = Mutable(); var s = p.challenges.Find(x => x.challengeId == c.id);
            if (s == null) { s = new CatChallengeProgress { challengeId = c.id }; p.challenges.Add(s); }
            return s;
        }
        bool Deny(string message) { Message = message; return false; }
        bool Commit(Action action, string success)
        {
            var before = JsonUtility.ToJson(game.Save);
            action();
            if (game.Persist()) { Message = success; return true; }
            JsonUtility.FromJsonOverwrite(before, game.Save);
            return Deny("Chưa lưu được. Tiến trình và phần thưởng được giữ nguyên; hãy thử lại.");
        }
        public bool Start()
        {
            if (Current == null || !game.IsCityUnlocked(Current.cityId)) return Deny("Thành phố chưa mở.");
            if (Progress(Current.cityId)?.started == true) return Deny("Nhiệm vụ đã được nhận; tiếp tục từ bước đang lưu.");
            return Commit(() => Mutable().started = true, Current.opening);
        }
        public bool Collect(string supplyId)
        {
            var c = Current; var p = c == null ? null : Progress(c.cityId);
            var supply = c?.supplies.FirstOrDefault(s => s.id == supplyId);
            if (p?.started != true || p.rescued || supply == null || !At(supply.nodeId)) return Deny("Đến đúng địa điểm để lấy vật dụng cứu hộ.");
            if (p.supplies.Contains(supplyId)) return Deny("Vật dụng này đã ở trong túi cứu hộ.");
            return Commit(() => p.supplies.Add(supplyId), "Đã cất " + supply.name + " vào túi cứu hộ.");
        }
        bool CanAct(CatRescueChallenge c)
        {
            return c != null && Current != null && Progress(Current.cityId)?.started == true && !Rescued(Current.cityId)
                && ReferenceEquals(Next(Current), c) && At(c.nodeId);
        }
        public bool Match(string id, int piece, int target)
        {
            var c = Next(Current);
            if (c?.id != id || !CanAct(c) || !new[] { "match", "drag", "jigsaw", "hangul", "hanbok" }.Contains(c.mode)) return Deny("Hãy đến địa điểm của thử thách đang mở.");
            if (piece < 0 || piece >= c.tokens.Length || target != piece) return Deny(c.hint);
            if (State(c)?.marks.Contains(piece) == true) return Deny("Mảnh này đã đặt đúng.");
            return Commit(() => MutableState(c).marks.Add(piece), "Đã khớp · " + c.tokens[piece]);
        }
        public bool Find(string id, int index)
        {
            var c = Next(Current);
            if (c?.id != id || !CanAct(c) || c.mode != "hidden") return Deny("Đến địa điểm của thử thách tìm vật ẩn.");
            if (index < 0 || index >= c.tokens.Length) return Deny(c.hint);
            if (State(c)?.marks.Contains(index) == true) return Deny("Bạn đã tìm thấy dấu vết này.");
            return Commit(() => MutableState(c).marks.Add(index), "Tìm thấy · " + c.tokens[index]);
        }
        public bool Choose(string id, int option)
        {
            var c = Next(Current);
            if (c?.id != id || !CanAct(c) || (c.mode != "choice" && c.mode != "scenario")) return Deny("Đến đúng địa điểm để xử lý tình huống.");
            if (option < 0 || option >= c.options.Length || option != c.answer) return Deny(c.hint);
            if (State(c)?.marks.Contains(option) == true) return Deny("Lựa chọn đúng đã được ghi lại.");
            return Commit(() => MutableState(c).marks.Add(option), c.fact);
        }
        public int MazePosition(CatRescueChallenge c) => State(c)?.path.Count > 0 ? State(c).path.Last() : c.start;
        public bool Walk(string id, int cell)
        {
            var c = Next(Current);
            if (c?.id != id || !CanAct(c) || c.mode != "maze") return Deny("Đến đúng điểm bắt đầu tuyến cứu hộ.");
            var width = c.maze[0].Length; var from = State(c)?.path.Count > 0 ? State(c).path.Last() : c.start;
            if (cell < 0 || cell >= width * c.maze.Length || c.maze[cell / width][cell % width] == '#'
                || Math.Abs(cell % width - from % width) + Math.Abs(cell / width - from / width) != 1) return Deny(c.hint);
            return Commit(() => { var s = MutableState(c); if (s.path.Count == 0) s.path.Add(c.start); s.path.Add(cell); }, cell == c.exit ? "Đã tìm được lối đến mèo!" : "Đã đánh dấu đoạn đường an toàn.");
        }
        public bool Ready(CatRescueChallenge c)
        {
            var s = State(c);
            if (s == null) return false;
            if (c.mode == "maze") return s.path.Count > 0 && s.path.Last() == c.exit;
            if (c.mode == "choice" || c.mode == "scenario") return s.marks.Contains(c.answer);
            if (c.mode == "hanbok" && !s.paletteChosen) return false;
            return c.tokens.Length > 0 && Enumerable.Range(0, c.tokens.Length).All(s.marks.Contains);
        }
        public bool PickHanbokPalette(string id,int palette)
        {
            var c=Next(Current);
            if(c?.id!=id||!CanAct(c)||c.mode!="hanbok"||palette<0||palette>3)return Deny("Chọn một phối màu khi đang ở thử thách Hanbok.");
            return Commit(()=>{var s=MutableState(c);s.palette=palette;s.paletteChosen=true;},"Đã lưu phối màu của bạn. Không có một bảng màu bắt buộc cho mọi Hanbok.");
        }
        public bool Complete(string id)
        {
            var c = Next(Current);
            if (c?.id != id || !CanAct(c) || !Ready(c)) return Deny("Còn thao tác chưa hoàn tất. " + (c?.hint ?? ""));
            return Commit(() => Mutable().completed.Add(id), c.fact);
        }
        public bool ResetMaze(string id)
        {
            var c = Next(Current);
            if (c?.id != id || !CanAct(c) || c.mode != "maze") return false;
            return Commit(() => MutableState(c).path.Clear(), "Đã về đầu tuyến; các thử thách trước vẫn được giữ.");
        }
        public bool CanRescue => Current != null && Progress(Current.cityId)?.started == true && !Rescued(Current.cityId)
            && Next(Current) == null && Current.supplies.All(s => Progress(Current.cityId).supplies.Contains(s.id)) && At(Current.rescueNode);
        public bool Rescue(Vector2 placementError)
        {
            if (!CanRescue || placementError.magnitude > .12f) return Deny("Hoàn thành bốn thử thách, mang đủ vật dụng và đưa mèo vào chỗ trú an toàn.");
            var c = Current; var key = "catrescue.reward." + c.catId;
            return Commit(() =>
            {
                Mutable().rescued = true;
                if (!game.Save.rewardGrantIds.Contains(key))
                { game.Save.rewardGrantIds.Add(key); game.Save.coins += c.rewardCoins; }
            }, c.catName + " đã an toàn! " + c.catLine);
        }
    }
}
