using System;

namespace MiningMaze
{
    /// <summary>
    /// Factory pattern: the only place that knows how to build each kind of item,
    /// so the store, the inventory and the mining code never call constructors directly.
    /// </summary>
    public static class ItemFactory
    {
        public static Item Create(ItemKind kind)
        {
            switch (kind)
            {
                case ItemKind.IronPickaxe:
                    return new Pickaxe(kind, "Iron Pickaxe", 10, "item_pickaxe_iron", 2);
                case ItemKind.GoldPickaxe:
                    return new Pickaxe(kind, "Gold Pickaxe", 150, "item_pickaxe_gold", 4);
                case ItemKind.DiamondPickaxe:
                    return new Pickaxe(kind, "Diamond Pickaxe", 300, "item_pickaxe_diamond", 6);
                case ItemKind.Bomb:
                    return new Bomb();
                case ItemKind.Coal:
                    return new ResourceItem(kind, "Coal", "Dusty but useful", 10, "item_coal", 5);
                case ItemKind.Iron:
                    return new ResourceItem(kind, "Iron", "Strong metal", 25, "item_iron", 10);
                case ItemKind.Gold:
                    return new ResourceItem(kind, "Gold", "Shiny and valuable", 60, "item_gold", 20);
                case ItemKind.Diamond:
                    return new ResourceItem(kind, "Diamond", "Needed to win!", 100, "item_diamond", 30);
                case ItemKind.Burger:
                    return new FoodItem(kind, "Burger", "Yummy", 50, "item_burger", 40);
                case ItemKind.Pizza:
                    return new FoodItem(kind, "Pizza", "Cheesy slice", 35, "item_pizza", 25);
                case ItemKind.Soda:
                    return new FoodItem(kind, "Soda", "Quick boost", 20, "item_soda", 12);
                default:
                    throw new ArgumentOutOfRangeException("kind", kind, "Unknown item kind");
            }
        }
    }
}
