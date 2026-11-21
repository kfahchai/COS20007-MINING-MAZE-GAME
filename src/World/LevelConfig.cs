using System;

namespace MiningMaze
{
    /// <summary>Everything that changes between difficulty levels.</summary>
    public class LevelConfig
    {
        public int Level;
        public int MazeSize;        // odd number: width = height
        public double TimeLimit;    // seconds
        public int TargetDiamonds;
        public int Diamonds, Gold, Iron, Coal;
        public double GhostSpeed;   // pixels per second
    }

    public static class LevelFactory
    {
        public const int LevelCount = 3;

        public static LevelConfig Create(int level)
        {
            switch (level)
            {
                case 1:
                    return new LevelConfig { Level = 1, MazeSize = 21, TimeLimit = 150, TargetDiamonds = 5,
                                             Diamonds = 8, Gold = 6, Iron = 8, Coal = 12, GhostSpeed = 38 };
                case 2:
                    return new LevelConfig { Level = 2, MazeSize = 31, TimeLimit = 210, TargetDiamonds = 10,
                                             Diamonds = 15, Gold = 10, Iron = 12, Coal = 16, GhostSpeed = 48 };
                case 3:
                    return new LevelConfig { Level = 3, MazeSize = 41, TimeLimit = 270, TargetDiamonds = 15,
                                             Diamonds = 22, Gold = 14, Iron = 16, Coal = 20, GhostSpeed = 58 };
                default:
                    throw new ArgumentOutOfRangeException("level");
            }
        }
    }
}
