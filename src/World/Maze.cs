namespace MiningMaze
{
    public enum Facing { Up, Down, Left, Right }

    /// <summary>A grid of blocks. Column = x, row = y.</summary>
    public class Maze
    {
        private readonly Block[,] _cells;

        public Maze(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new Block[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    _cells[x, y] = BlockFactory.Create(BlockType.Wall);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int StartCol { get; set; }
        public int StartRow { get; set; }

        public bool InBounds(int col, int row)
        {
            return col >= 0 && row >= 0 && col < Width && row < Height;
        }

        public Block Get(int col, int row)
        {
            return InBounds(col, row) ? _cells[col, row] : null;
        }

        public void Set(int col, int row, Block block)
        {
            if (InBounds(col, row)) _cells[col, row] = block;
        }

        public bool IsWalkable(int col, int row)
        {
            Block b = Get(col, row);
            return b != null && b.IsWalkable;
        }

        public int CountOf(BlockType type)
        {
            int n = 0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (_cells[x, y].Type == type) n++;
            return n;
        }

        public static void Step(Facing f, ref int col, ref int row)
        {
            switch (f)
            {
                case Facing.Up: row--; break;
                case Facing.Down: row++; break;
                case Facing.Left: col--; break;
                case Facing.Right: col++; break;
            }
        }
    }
}
