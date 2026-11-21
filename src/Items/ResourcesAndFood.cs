namespace MiningMaze
{
    /// <summary>Something mined from the maze. It can be sold for money.</summary>
    public class ResourceItem : Item
    {
        public ResourceItem(ItemKind kind, string name, string description, int value, string icon, int points)
            : base(kind, name, description, value, icon)
        {
            Points = points;
        }

        /// <summary>Score awarded when this resource is mined.</summary>
        public int Points { get; private set; }

        public bool IsDiamond { get { return Kind == ItemKind.Diamond; } }

        public override ItemCategory Category { get { return ItemCategory.Resource; } }
    }

    /// <summary>Food restores stamina when eaten.</summary>
    public class FoodItem : Item
    {
        public FoodItem(ItemKind kind, string name, string description, int value, string icon, int stamina)
            : base(kind, name, description, value, icon)
        {
            Stamina = stamina;
        }

        public int Stamina { get; private set; }

        public override ItemCategory Category { get { return ItemCategory.Food; } }

        public override string Details { get { return "+" + Stamina + " stamina"; } }
    }
}
