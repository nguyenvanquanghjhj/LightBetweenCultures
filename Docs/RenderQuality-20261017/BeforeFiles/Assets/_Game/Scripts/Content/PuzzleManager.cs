using System;
using System.Text;

namespace LightBetweenCultures
{
    /// <summary>Data-defined sequence/pattern/code puzzles with a single grant ledger.</summary>
    public sealed class PuzzleManager
    {
        private readonly GameBootstrap game;
        public PuzzleManager(GameBootstrap owner) { game = owner; }
        public PuzzleDefinition Definition { get; private set; }
        public bool Active { get; private set; }
        public string Log { get; private set; } = "";
        public int Rotations { get; private set; }
        public int Folds { get; private set; }
        public int[] Rings { get; private set; } = new int[3];
        public string RingAnswer=>Definition==null?"":string.Join("|",Array.ConvertAll(Rings,i=>Definition.options[i]));
        public bool Solved(string id) => game.Save.solvedPuzzles.Contains(id);

        public bool Start(string id)
        {
            if (Active) { Log = "Hãy hoàn thành hoặc đóng câu đố hiện tại trước."; return false; }
            var definition = game.Content.Puzzle(id);
            if (definition == null || Solved(id)) { Log = "Câu đố đã được giải."; return false; }
            if (!AtLocation(definition)) { Log = "Hãy tới địa điểm có câu đố trước."; return false; }
            foreach (var flag in definition.requiredFlags ?? Array.Empty<string>())
                if (!game.World.Has(flag)) { Log = "Bạn cần tìm manh mối trước khi giải câu đố."; return false; }
            foreach(var item in definition.requiredItems??Array.Empty<ItemAmount>())
                if(!game.Inventory.Has(item.itemId,item.amount)){Log="Chưa có đủ vật phẩm để thử câu đố.";return false;}
            Definition = definition; Active = true; Rotations=Folds=0; Rings=new int[3]; Log = definition.clue;
            return true;
        }

        public bool FoldStep(bool fold)
        {
            if(!Active||Definition?.mode!="fold"||!AtLocation(Definition))return false;
            if(fold){if(Rotations!=2||Folds!=0){Log="Căn hai vạch sáng bằng hai lần xoay trước khi gấp.";return false;}Folds=1;}
            else {if(Folds>0)return false;Rotations=(Rotations+1)%4;}
            Log="Xoay: "+Rotations+" / 2 · Gấp: "+Folds+" / 1";return true;
        }
        public bool RotateRing(int index)
        {
            if(!Active||Definition?.mode!="rings"||index<0||index>=3||Definition.options.Length<2||!AtLocation(Definition))return false;
            Rings[index]=(Rings[index]+1)%Definition.options.Length;Log="Ba vòng hiện tại: "+RingAnswer;return true;
        }
        public void ResetInteraction()
        {if(Active){Rotations=Folds=0;Rings=new int[3];Log=Definition.clue;}}

        public bool Submit(string answer)
        {
            if (!Active || Definition == null || Solved(Definition.id)) return false;
            if (!AtLocation(Definition)) { Active = false; Log = "Bạn đã rời địa điểm có câu đố."; return false; }
            foreach(var flag in Definition.requiredFlags??Array.Empty<string>())if(!game.World.Has(flag))return false;
            foreach(var item in Definition.requiredItems??Array.Empty<ItemAmount>())if(!game.Inventory.Has(item.itemId,item.amount))return false;
            if(Definition.mode=="fold"&&(Rotations!=2||Folds!=1)){Log="Tờ giấy chưa khớp vạch sáng. Hãy xoay hai lần rồi gấp một lần.";return false;}
            if(Definition.mode=="rings"&&Normalize(RingAnswer)!=Normalize(Definition.answer)){Log="Ba cơ cấu chưa đồng bộ. Hãy xoay từng vị trí theo manh mối.";return false;}
            if (Normalize(answer) != Normalize(Definition.answer))
            {
                Log = "Chưa đúng. Hãy đọc lại manh mối và thử lần nữa.";
                return false;
            }
            game.Save.solvedPuzzles.Add(Definition.id);
            if (!string.IsNullOrEmpty(Definition.completionFlag)) game.World.Set(Definition.completionFlag);
            foreach (var flag in Definition.flags ?? Array.Empty<string>()) game.World.Set(flag);
            foreach (var reward in Definition.rewardItems ?? Array.Empty<ItemAmount>())
                game.Inventory.GrantOnce("puzzle:" + Definition.id + ":" + reward.itemId, reward.itemId, reward.amount);
            game.Save.coins += Definition.rewardCoins;
            Active = false; Log = "Chính xác! Manh mối và phần thưởng đã được lưu.";
            game.Persist();
            return true;
        }

        public void Cancel() { Active = false; }
        private bool AtLocation(PuzzleDefinition definition) => definition.cityId == game.Data.cityId &&
            game.Save.node >= 0 && game.Save.node < game.Data.nodes.Length &&
            game.Data.nodes[game.Save.node].id == definition.nodeId;
        private static string Normalize(string value)
        {
            var result = new StringBuilder();
            foreach (var ch in (value ?? "").Normalize(NormalizationForm.FormC).ToLowerInvariant())
                if (char.IsLetterOrDigit(ch)) result.Append(ch);
            return result.ToString();
        }
    }
}
