namespace MiningMaze
{
    public enum ItemCategory { Tool, Resource, Food }

    public enum ItemKind
    {
        IronPickaxe, GoldPickaxe, DiamondPickaxe, Bomb,
        Coal, Iron, Gold, Diamond,
        Burger, Pizza, Soda
    }

    /// <summary>
    /// Base class for everything that can live in the player's inventory or the store.
    /// Sub-classes decide which actions the inventory offers (Use / Upgrade / Sell / Eat / Remove).
    /// </summary>
    public abstract class Item
    {
        protected Item(ItemKind kind, string name, string description, int value, string icon)
        {
            Kind = kind;
            Name = name;
            Description = description;
            Value = value;
            Icon = icon;
        }

        public ItemKind Kind { get; private set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        /// <summary>Store price for tools and food, sell price for resources.</summary>
        public int Value { get; protected set; }

        /// <summary>Name of the bitmap used to draw this item.</summary>
        public string Icon { get; private set; }

        public abstract ItemCategory Category { get; }

        /// <summary>Extra line shown in the item description box.</summary>
        public virtual string Details { get { return ""; } }
    }
}
