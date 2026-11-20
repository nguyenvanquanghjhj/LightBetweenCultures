namespace LightBetweenCultures
{
    public sealed class Inventory
    {
        private readonly SaveData save;
        public Inventory(SaveData data) { save = data; }
        public int Count(string item) => item == "coin" ? save.coins : save.items.FindAll(value=>value==item).Count;
        public bool Has(string item,int count=1) => Count(item)>=count;
        public void Add(string item,int count=1)
        {
            if(count<=0) return;
            if(item=="coin") { save.coins+=count; return; }
            for(var i=0;i<count;i++) save.items.Add(item);
        }
        public bool TryConsume(string item,int count=1)
        {
            if(count<=0 || !Has(item,count)) return false;
            if(item=="coin") save.coins-=count;
            else for(var i=0;i<count;i++) save.items.Remove(item);
            return true;
        }
        public bool GrantOnce(string key,string item,int count=1)
        {
            if(save.rewardGrantIds.Contains(key)) return false;
            save.rewardGrantIds.Add(key);
            // Several canonical sources can contain the same unique letter/key.
            // Recording both sources must not create a second copy of that key.
            if(item!="coin"&&ContentDatabase.Load().Item(item)?.stackLimit==1)count=System.Math.Min(count,System.Math.Max(0,1-Count(item)));
            Add(item,count); return true;
        }
        public bool Collect(string item)
        {
            if (save.collected.Contains(item)) return false;
            save.collected.Add(item); save.items.Add(item); return true;
        }
        public void Consume(string item) => TryConsume(item);
    }
}
