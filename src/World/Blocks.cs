using System;

namespace MiningMaze
{
    public enum BlockType { Empty, Dirt, Wall, Coal, Iron, Gold, Diamond }

    /// <summary>One cell of the maze. Each subclass decides whether it can be walked on or mined.</summary>
    public abstract class Block
    {
        protected Block(int strength)
        {
            MaxStrength = strength;
            Strength = strength;
        }

        public abstract BlockType Type { get; }

        /// <summary>Bitmap used to draw the block, or null for an empty tunnel.</summary>
        public abstract string Texture { get; }

        public int MaxStrength { get; private set; }
        public int Strength { get; private set; }

        public virtual bool IsWalkable { get { return false; } }
        public virtual bool IsMineable { get { return true; } }
        public bool IsDamaged { get { return Strength < MaxStrength; } }

        /// <summary>Applies damage and returns true when the block breaks.</summary>
        public bool Hit(int damage)
        {
            if (!IsMineable) return false;
            Strength = Math.Max(0, Strength - Math.Max(1, damage));
            return Strength == 0;
        }
    }

    /// <summary>A tunnel that has already been dug out. The player can walk here.</summary>
    public class EmptyBlock : Block
    {
        public EmptyBlock() : base(0) { }
        public override BlockType Type { get { return BlockType.Empty; } }
        public override string Texture { get { return null; } }
        public override bool IsWalkable { get { return true; } }
        public override bool IsMineable { get { return false; } }
    }

    /// <summary>Soft soil that fills the maze paths.</summary>
    public class DirtBlock : Block
    {
        public DirtBlock() : base(2) { }
        public override BlockType Type { get { return BlockType.Dirt; } }
        public override string Texture { get { return "tile_dirt"; } }
    }

    /// <summary>Solid rock that forms the maze walls. It can never be mined.</summary>
    public class WallBlock : Block
    {
        public WallBlock() : base(int.MaxValue) { }
        public override BlockType Type { get { return BlockType.Wall; } }
        public override string Texture { get { return "tile_wall"; } }
        public override bool IsMineable { get { return false; } }
    }

    /// <summary>Ore embedded in the rock. Breaking it drops a resource item.</summary>
    public class ResourceBlock : Block
    {
        private readonly BlockType _type;

        public ResourceBlock(BlockType type, int strength, ItemKind drop) : base(strength)
        {
            _type = type;
            Drop = drop;
        }

        public ItemKind Drop { get; private set; }
        public override BlockType Type { get { return _type; } }
        public override string Texture { get { return "tile_" + _type.ToString().ToLower(); } }
    }

    /// <summary>Factory for blocks, keeping the strength of each ore in one place.</summary>
    public static class BlockFactory
    {
        public static Block Create(BlockType type)
        {
            switch (type)
            {
                case BlockType.Empty: return new EmptyBlock();
                case BlockType.Dirt: return new DirtBlock();
                case BlockType.Wall: return new WallBlock();
                case BlockType.Coal: return new ResourceBlock(type, 4, ItemKind.Coal);
                case BlockType.Iron: return new ResourceBlock(type, 6, ItemKind.Iron);
                case BlockType.Gold: return new ResourceBlock(type, 9, ItemKind.Gold);
                case BlockType.Diamond: return new ResourceBlock(type, 12, ItemKind.Diamond);
                default: throw new ArgumentOutOfRangeException("type");
            }
        }
    }
}
