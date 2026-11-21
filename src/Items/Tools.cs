namespace MiningMaze
{
    /// <summary>A tool deals damage to blocks. Higher efficiency = fewer hits per block.</summary>
    public abstract class ToolItem : Item
    {
        protected ToolItem(ItemKind kind, string name, string description, int value, string icon, int efficiency)
            : base(kind, name, description, value, icon)
        {
            Efficiency = efficiency;
        }

        public int Efficiency { get; protected set; }

        public override ItemCategory Category { get { return ItemCategory.Tool; } }

        public virtual bool CanUpgrade { get { return false; } }

        public virtual int UpgradeCost { get { return 0; } }

        public virtual void Upgrade() { }

        /// <summary>Single-use tools are removed from the inventory after one use.</summary>
        public virtual bool IsConsumable { get { return false; } }
    }

    public class Pickaxe : ToolItem
    {
        public const int MaxLevel = 3;
        private readonly string _baseName;

        public Pickaxe(ItemKind kind, string name, int value, string icon, int efficiency)
            : base(kind, name, "Mine", value, icon, efficiency)
        {
            _baseName = name;
            Level = 1;
        }

        public int Level { get; private set; }

        public override bool CanUpgrade { get { return Level < MaxLevel; } }

        public override int UpgradeCost { get { return 40 * Level; } }

        public override string Details
        {
            get { return "Efficiency " + Efficiency + "  Lv " + Level + "/" + MaxLevel; }
        }

        public override void Upgrade()
        {
            if (!CanUpgrade) return;
            Level++;
            Efficiency += 1;
            Value += 20;
            Name = _baseName + " +" + (Level - 1);
        }
    }

    /// <summary>Blows up every mineable block in a 3x3 area around the selected cell.</summary>
    public class Bomb : ToolItem
    {
        public Bomb() : base(ItemKind.Bomb, "Bomb", "3x3 blast", 60, "item_bomb", 999) { }

        public int Radius { get { return 1; } }

        public override bool IsConsumable { get { return true; } }

        public override string Details { get { return "Single use"; } }
    }
}
