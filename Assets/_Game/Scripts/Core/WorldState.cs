namespace LightBetweenCultures
{
    public sealed class WorldState
    {
        private readonly GameBootstrap game;
        public WorldState(GameBootstrap owner) { game=owner; }
        public bool Has(string flag) => flag=="seoul.relays_ready" ? RelaysReady : string.IsNullOrEmpty(flag) || game.Save.flags.Contains(flag) ||
            flag.StartsWith("puzzle:",System.StringComparison.Ordinal)&&game.Save.solvedPuzzles.Contains(flag.Substring(7));
        bool RelaysReady=>game.Save.flags.Contains("seoul.relay_a_on")&&game.Save.flags.Contains("seoul.relay_b_on")&&game.Save.flags.Contains("seoul.relay_c_on");
        public void RefreshDerivedFlags()
        {
            if(RelaysReady){if(!game.Save.flags.Contains("seoul.relays_ready"))game.Save.flags.Add("seoul.relays_ready");}
            else game.Save.flags.RemoveAll(f=>f=="seoul.relays_ready");
        }
        public bool Set(string flag)
        {
            if(string.IsNullOrEmpty(flag)||Has(flag)) return false;
            game.Save.flags.Add(flag); return true;
        }
    }
}
