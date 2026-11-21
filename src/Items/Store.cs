using System.Collections.Generic;

namespace MiningMaze
{
    /// <summary>The in-game shop: food to restore stamina and better tools.</summary>
    public class Store
    {
        private readonly List<Item> _catalogue = new List<Item>();

        public Store()
        {
            foreach (ItemKind k in new[] { ItemKind.Burger, ItemKind.Pizza, ItemKind.Soda, ItemKind.Bomb,
                                           ItemKind.GoldPickaxe, ItemKind.DiamondPickaxe })
                _catalogue.Add(ItemFactory.Create(k));
        }

        /// <summary>Display copies of the items for sale (buying creates a fresh item).</summary>
        public IList<Item> Catalogue { get { return _catalogue.AsReadOnly(); } }

        public string Buy(Player player, Item displayItem)
        {
            if (player.Money < displayItem.Value)
                return "Not enough money!";
            if (player.Inventory.IsFull)
                return "Inventory is full!";

            Item bought = ItemFactory.Create(displayItem.Kind);
            player.Inventory.Add(bought);
            player.Money -= displayItem.Value;
            return "Bought " + bought.Name + "!";
        }
    }
}
