using System;
using System.Collections.Generic;

namespace MiningMaze
{
    /// <summary>
    /// Builds a new random maze for every game:
    /// 1. A depth-first "recursive backtracker" (with an explicit stack) carves dirt paths between stone walls.
    /// 2. Ore is embedded in wall cells that touch a path, so it can always be reached.
    ///    Diamonds are placed further from the start so the player has to explore.
    /// </summary>
    public static class MazeGenerator
    {
        public static Maze Generate(LevelConfig cfg, Random rnd)
        {
            int size = cfg.MazeSize;
            Maze maze = new Maze(size, size);

            // --- 1. Carve the maze -------------------------------------------------
            bool[,] visited = new bool[size, size];
            Stack<int[]> stack = new Stack<int[]>();
            stack.Push(new[] { 1, 1 });
            visited[1, 1] = true;
            maze.Set(1, 1, BlockFactory.Create(BlockType.Dirt));
            int[][] dirs = { new[] { 0, -2 }, new[] { 0, 2 }, new[] { -2, 0 }, new[] { 2, 0 } };

            while (stack.Count > 0)
            {
                int[] cur = stack.Peek();
                List<int[]> options = new List<int[]>();
                foreach (int[] d in dirs)
                {
                    int nx = cur[0] + d[0], ny = cur[1] + d[1];
                    if (nx > 0 && ny > 0 && nx < size - 1 && ny < size - 1 && !visited[nx, ny])
                        options.Add(new[] { nx, ny, d[0] / 2, d[1] / 2 });
                }
                if (options.Count == 0)
                {
                    stack.Pop();
                    continue;
                }
                int[] next = options[rnd.Next(options.Count)];
                maze.Set(cur[0] + next[2], cur[1] + next[3], BlockFactory.Create(BlockType.Dirt));
                maze.Set(next[0], next[1], BlockFactory.Create(BlockType.Dirt));
                visited[next[0], next[1]] = true;
                stack.Push(new[] { next[0], next[1] });
            }

            // Open a few extra gaps so the maze has loops (less frustrating dead ends).
            int extra = size / 2;
            for (int i = 0; i < extra * 4 && extra > 0; i++)
            {
                int x = rnd.Next(1, size - 1), y = rnd.Next(1, size - 1);
                bool horizontalGap = maze.Get(x - 1, y).Type == BlockType.Dirt && maze.Get(x + 1, y).Type == BlockType.Dirt;
                bool verticalGap = maze.Get(x, y - 1).Type == BlockType.Dirt && maze.Get(x, y + 1).Type == BlockType.Dirt;
                if (maze.Get(x, y).Type == BlockType.Wall && (horizontalGap ^ verticalGap))
                {
                    maze.Set(x, y, BlockFactory.Create(BlockType.Dirt));
                    extra--;
                }
            }

            // The start cell is already dug out.
            maze.StartCol = 1;
            maze.StartRow = 1;
            maze.Set(1, 1, BlockFactory.Create(BlockType.Empty));

            // --- 2. Embed ore in walls next to paths --------------------------------
            List<int[]> candidates = new List<int[]>();
            for (int x = 1; x < size - 1; x++)
                for (int y = 1; y < size - 1; y++)
                    if (maze.Get(x, y).Type == BlockType.Wall && TouchesPath(maze, x, y) && !(x <= 2 && y <= 2))
                        candidates.Add(new[] { x, y });
            Shuffle(candidates, rnd);

            // Diamonds first, from the candidates furthest from the start.
            candidates.Sort((a, b) => (b[0] + b[1]).CompareTo(a[0] + a[1]));
            int farPool = Math.Max(cfg.Diamonds, candidates.Count / 2);
            List<int[]> far = candidates.GetRange(0, Math.Min(farPool, candidates.Count));
            Shuffle(far, rnd);
            HashSet<long> used = new HashSet<long>();
            Place(maze, far, cfg.Diamonds, BlockType.Diamond, used);

            Shuffle(candidates, rnd);
            Place(maze, candidates, cfg.Gold, BlockType.Gold, used);
            Place(maze, candidates, cfg.Iron, BlockType.Iron, used);
            Place(maze, candidates, cfg.Coal, BlockType.Coal, used);
            return maze;
        }

        private static bool TouchesPath(Maze maze, int x, int y)
        {
            return IsPath(maze, x - 1, y) || IsPath(maze, x + 1, y) || IsPath(maze, x, y - 1) || IsPath(maze, x, y + 1);
        }

        private static bool IsPath(Maze maze, int x, int y)
        {
            Block b = maze.Get(x, y);
            return b != null && (b.Type == BlockType.Dirt || b.Type == BlockType.Empty);
        }

        private static void Place(Maze maze, List<int[]> cells, int count, BlockType type, HashSet<long> used)
        {
            int placed = 0;
            foreach (int[] c in cells)
            {
                if (placed >= count) break;
                long key = c[0] * 10000L + c[1];
                if (used.Contains(key)) continue;
                used.Add(key);
                maze.Set(c[0], c[1], BlockFactory.Create(type));
                placed++;
            }
        }

        private static void Shuffle<T>(IList<T> list, Random rnd)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }
    }
}
