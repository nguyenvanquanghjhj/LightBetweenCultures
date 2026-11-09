using System;

namespace LightBetweenCultures
{
    /// <summary>Reusable turn combat. Enemy statistics are read from the LBC catalog.</summary>
    public sealed class BattleManager
    {
        private readonly GameBootstrap game;
        private EventDefinition encounter;
        public BattleManager(GameBootstrap owner) { game = owner; }
        public EnemyDefinition Enemy { get; private set; }
        public int EnemyHP { get; private set; }
        public int EnemyMaxHP { get; private set; }
        public int EnemyAttack { get; private set; }
        public int EnemyDefense { get; private set; }
        public int Phase { get; private set; } = 1;
        public int RepairProgress { get; private set; }
        public int ShieldBeat { get; private set; }
        int shieldTurns;
        public bool Shielded=>Active&&Enemy?.mechanic=="star_cycle"&&shieldTurns==0;
        public string ShieldSymbol=>Enemy?.shieldPattern!=null&&Enemy.shieldPattern.Length>0?Enemy.shieldPattern[ShieldBeat%Enemy.shieldPattern.Length]:"";
        public bool CanRepair=>Active&&Phase==2&&Enemy?.mechanic=="two_phase"&&Enemy.repairFlags.Length>0&&Array.TrueForAll(Enemy.repairFlags,game.World.Has);
        public int PlayerHP => game.Save.playerHP;
        public int Energy => game.Save.lightEnergy;
        public int TurnNumber { get; private set; }
        public bool Active { get; private set; }
        public bool HasPlayerTurn { get; private set; }
        public string Outcome { get; private set; } = "";
        public string Log { get; private set; } = "";

        public bool Start(EventDefinition definition)
        {
            if (Active || definition == null || new EncounterManager(game).Resolved(definition)) return false;
            if (definition.cityId != game.Data.cityId || game.Save.node < 0 || game.Save.node >= game.Data.nodes.Length ||
                definition.nodeId != game.Data.nodes[game.Save.node].id || !game.Events.CanUse(definition)) return false;
            var enemy = game.Content.Enemy(definition.enemyId);
            if (enemy == null || enemy.hp <= 0) return false;
            encounter = definition; Enemy = enemy;
            var weakened=enemy.weaknessFlags!=null&&enemy.weaknessFlags.Length>0&&Array.TrueForAll(enemy.weaknessFlags,game.World.Has);
            EnemyMaxHP=EnemyHP=Math.Max(1,enemy.hp-(weakened?enemy.weaknessHpReduction:0));
            EnemyAttack=Math.Max(1,enemy.attack-(weakened?enemy.weaknessAttackReduction:0));
            EnemyDefense=Math.Max(0,enemy.defense-(weakened?enemy.weaknessDefenseReduction:0));
            Phase=1;RepairProgress=ShieldBeat=shieldTurns=0;
            if(enemy.mechanic=="two_phase")EnemyMaxHP=EnemyHP=Math.Max(1,enemy.phaseOneHP);
            if (game.Save.playerHP <= 0) game.Save.playerHP = 50;
            game.Save.lightEnergy = Math.Max(0, Math.Min(3, game.Save.lightEnergy));
            TurnNumber = 1; Outcome = ""; Active = HasPlayerTurn = true;
            Log = enemy.name + " đang chặn đường. Lượt của Lumi.";
            if(weakened)Log+="\nNhững người bạn đã giúp làm tín hiệu dịu lại: đối thủ yếu hơn.";
            return true;
        }

        public string Act(string action)
        {
            if (!Active || !HasPlayerTurn) return Log;
            bool defending = false;
            var resolution="battle";var shieldAction=false;
            int damage;
            if(action.StartsWith("resonate:"))
            {
                if(Enemy.mechanic!="star_cycle"||!Shielded||!int.TryParse(action.Substring(9),out var selected)||selected<0||selected>=Enemy.shieldPattern.Length)return Log;
                shieldAction=true;defending=true;
                if(Enemy.shieldPattern[selected]==ShieldSymbol){shieldTurns=3;Log="Đúng nhịp "+ShieldSymbol+"! Khiên mở trong ba hành động.";}
                else Log="Sai nhịp; khiên vẫn đóng. Có thể thử ký hiệu đang sáng.";
            }
            else
            switch (action)
            {
                case "attack":
                    damage = Shielded?0:Math.Max(1, 9 - EnemyDefense);
                    EnemyHP = Math.Max(0, EnemyHP - damage);
                    Log = "Lumi tấn công: " + damage + " sát thương.";
                    break;
                case "defend":
                    defending = true;
                    game.Save.lightEnergy = Math.Min(3, game.Save.lightEnergy + 1);
                    Log = "Lumi phòng thủ, giảm nửa sát thương và hồi 1 năng lượng.";
                    break;
                case "light":
                    if (game.Save.lightEnergy < 1) { Log = "Thiếu năng lượng. Phòng thủ để hồi lại."; return Log; }
                    game.Save.lightEnergy--;
                    damage = Shielded?0:Math.Max(1, 14 - EnemyDefense / 2);
                    EnemyHP = Math.Max(0, EnemyHP - damage);
                    Log = "Tia sáng: " + damage + " sát thương.";
                    break;
                case "heal":
                    if (game.Save.playerHP >= 100) { Log = "Lumi đang đầy HP."; return Log; }
                    if (!game.Inventory.TryConsume("healing_tea", 1)) { Log = "Không có Trà Hồi Phục."; return Log; }
                    var tea = game.Content.Item("healing_tea");
                    game.Save.playerHP = Math.Min(100, game.Save.playerHP + Math.Max(1, tea == null ? 35 : tea.healHP));
                    Log = "Lumi uống trà hồi phục.";
                    break;
                case "run":
                    Active = HasPlayerTurn = false; Outcome = "run";
                    Log = "Lumi lùi lại. Đối thủ vẫn giữ đường; bạn có thể thử cách khác.";
                    game.Persist(); return Log;
                case "repair":
                    if(!CanRepair){Log="Chỉ sửa được khi lõi đã lộ ở pha hai và sự thật đã được xác nhận tại S11.";return Log;}
                    RepairProgress++;defending=true;
                    Log="Lumi sửa mạch "+RepairProgress+" / 3 bằng tín hiệu đã xác thực.";
                    if(RepairProgress==3){EnemyHP=0;resolution="repair";}
                    break;
                default: return Log;
            }
            HasPlayerTurn = false;
            if (EnemyHP <= 0)
            {
                if(Enemy.mechanic=="two_phase"&&Phase==1)
                {
                    Phase=2;EnemyMaxHP=EnemyHP=Math.Max(1,Enemy.phaseTwoHP);TurnNumber++;HasPlayerTurn=true;
                    Log+="\nKhiên đã vỡ — PHA 2. Lõi lộ ra: tiếp tục giao chiến, che chắn hoặc sửa ba mạch nếu đã hiểu lệnh gốc. Chưa thắng và chưa nhận thưởng.";
                }
                else
                {
                    Active = false; Outcome = "win";
                    new EncounterManager(game).Resolve(encounter,resolution);
                    Log += resolution=="repair"?"\nBa mạch đã được phục hồi. Hệ phòng vệ được cứu; tiến độ được lưu một lần.":"\nChiến thắng! Đường đã mở; phần thưởng được nhận một lần.";
                }
            }
            else
            {
                // A full turn consists of one player choice and one enemy reply.
                // Skill names cycle deterministically so UI/tests can show the
                // actual counterattack rather than a random damage-only result.
                var counter = Math.Max(1, defending ? (EnemyAttack + 1) / 2 : EnemyAttack);
                game.Save.playerHP = Math.Max(0, game.Save.playerHP - counter);
                var skill = Enemy.skills != null && Enemy.skills.Length > 0
                    ? Enemy.skills[(TurnNumber - 1) % Enemy.skills.Length] : "Tấn công";
                Log += "\n" + Enemy.name + " dùng " + skill + ": " + counter + " sát thương.";
                if (game.Save.playerHP == 0)
                {
                    Active = false; Outcome = "loss";
                    // Defeat does not delete quest items or clear discoveries.
                    game.Save.playerHP = 50;
                    game.Save.lightEnergy = 3;
                    Log += "\nLumi kiệt sức và nghỉ hồi phục 50 HP. Hãy quay lại khi sẵn sàng.";
                }
                else { TurnNumber++; HasPlayerTurn = true; }
            }
            if(Enemy.mechanic=="star_cycle"&&!shieldAction&&shieldTurns>0&&Active)
            {shieldTurns--;if(shieldTurns==0){ShieldBeat++;Log+="\nKhiên khép lại. Nhịp kế: "+ShieldSymbol+".";}}
            game.Persist();
            return Log;
        }
    }
}
