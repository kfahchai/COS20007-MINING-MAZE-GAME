using System.Collections.Generic;

namespace MiningMaze
{
    /// <summary>A fixed-size bag of items carried by the player.</summary>
    public class Inventory
    {
        public const int DefaultCapacity = 20;
        private readonly List<Item> _items = new List<Item>();

        public Inventory(int capacity = DefaultCapacity)
        {
            Capacity = capacity;
        }

        public int Capacity { get; private set; }

        public IList<Item> Items { get { return _items.AsReadOnly(); } }

        public int Count { get { return _items.Count; } }

        public bool IsFull { get { return _items.Count >= Capacity; } }

        public bool Add(Item item)
        {
            if (IsFull) return false;
            _items.Add(item);
            return true;
        }

        public bool Remove(Item item)
        {
            return _items.Remove(item);
        }

        public bool Contains(Item item)
        {
            return _items.Contains(item);
        }

        public int CountOf(ItemKind kind)
        {
            int n = 0;
            foreach (Item i in _items)
                if (i.Kind == kind) n++;
            return n;
        }

        public Item ItemAt(int index)
        {
            return index >= 0 && index < _items.Count ? _items[index] : null;
        }

        /// <summary>Best pickaxe in the bag; used when the equipped tool is lost.</summary>
        public ToolItem BestPickaxe()
        {
            Pickaxe best = null;
            foreach (Item i in _items)
            {
                Pickaxe p = i as Pickaxe;
                if (p != null && (best == null || p.Efficiency > best.Efficiency)) best = p;
            }
            return best;
        }
    }
}
