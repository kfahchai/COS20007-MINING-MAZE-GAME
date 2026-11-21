// Headless tests for the game rules (no window needed).
// Build & run with Mono:  mcs -out:tests.exe tests/LogicTests.cs src/Items/*.cs src/World/*.cs src/Entities/*.cs && mono tests.exe
using System;
using System.Collections.Generic;
using MiningMaze;

public static class LogicTests
{
    private static int _failures;

    private static void Check(bool ok, string what)
    {
        Console.WriteLine((ok ? "  PASS  " : "  FAIL  ") + what);
        if (!ok) _failures++;
    }

    public static int Main()
    {
        for (int level = 1; level <= LevelFactory.LevelCount; level++)
            MazeTests(level);
        ItemTests();
        MonsterTests();
        for (int level = 1; level <= LevelFactory.LevelCount; level++)
            BotPlaysLevel(level, 42 + level);
        Console.WriteLine(_failures == 0 ? "ALL TESTS PASSED" : _failures + " TEST(S) FAILED");
        return _failures == 0 ? 0 : 1;
    }

    private static void MazeTests(int level)
    {
        Console.WriteLine("Maze, level " + level);
        LevelConfig cfg = LevelFactory.Create(level);
        Maze m = MazeGenerator.Generate(cfg, new Random(level));
        Check(m.Width == cfg.MazeSize && m.Height == cfg.MazeSize, "size is " + cfg.MazeSize);
        Check(m.Get(m.StartCol, m.StartRow).IsWalkable, "start cell is an open tunnel");
        Check(m.CountOf(BlockType.Diamond) == cfg.Diamonds, "placed " + cfg.Diamonds + " diamonds");
        Check(m.CountOf(BlockType.Diamond) >= cfg.TargetDiamonds, "enough diamonds to win");
        bool border = true;
        for (int i = 0; i < m.Width; i++)
            border &= m.Get(i, 0).Type == BlockType.Wall && m.Get(i, m.Height - 1).Type == BlockType.Wall
                   && m.Get(0, i).Type == BlockType.Wall && m.Get(m.Width - 1, i).Type == BlockType.Wall;
        Check(border, "outer border is solid wall");

        // Every ore must be reachable: flood-fill through dirt/empty, ores are adjacent to that region.
        bool[,] reach = Flood(m);
        int unreachable = 0;
        for (int x = 0; x < m.Width; x++)
            for (int y = 0; y < m.Height; y++)
                if (m.Get(x, y) is ResourceBlock &&
                    !(At(reach, x - 1, y) || At(reach, x + 1, y) || At(reach, x, y - 1) || At(reach, x, y + 1)))
                    unreachable++;
        Check(unreachable == 0, "every ore block touches a reachable path");
    }

    private static bool At(bool[,] a, int x, int y)
    {
        return x >= 0 && y >= 0 && x < a.GetLength(0) && y < a.GetLength(1) && a[x, y];
    }

    private static bool[,] Flood(Maze m)
    {
        bool[,] seen = new bool[m.Width, m.Height];
        Queue<int[]> q = new Queue<int[]>();
        q.Enqueue(new[] { m.StartCol, m.StartRow });
        seen[m.StartCol, m.StartRow] = true;
        while (q.Count > 0)
        {
            int[] c = q.Dequeue();
            foreach (int[] d in new[] { new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } })
            {
                int x = c[0] + d[0], y = c[1] + d[1];
                Block b = m.Get(x, y);
                if (b == null || seen[x, y]) continue;
                if (b.Type == BlockType.Dirt || b.Type == BlockType.Empty)
                {
                    seen[x, y] = true;
                    q.Enqueue(new[] { x, y });
                }
            }
        }
        return seen;
    }

    private static void ItemTests()
    {
        Console.WriteLine("Items, store and inventory");
        Player p = new Player(1, 1);
        Check(p.Equipped is Pickaxe && p.Inventory.Count == 2, "starts with an iron pickaxe and a bomb");

        Store store = new Store();
        Item burger = null, diamondPick = null;
        foreach (Item i in store.Catalogue)
        {
            if (i.Kind == ItemKind.Burger) burger = i;
            if (i.Kind == ItemKind.DiamondPickaxe) diamondPick = i;
        }
        Check(store.Buy(p, diamondPick) == "Not enough money!", "cannot buy what you can't afford");
        Check(store.Buy(p, burger).StartsWith("Bought") && p.Money == 50, "buying a burger costs $50");

        p.LoseStamina(70);
        FoodItem food = null;
        foreach (Item i in p.Inventory.Items) if (i is FoodItem) food = (FoodItem)i;
        p.Eat(food);
        Check(p.Stamina == 70 && p.Inventory.CountOf(ItemKind.Burger) == 0, "eating restores 40 stamina");

        Pickaxe pick = (Pickaxe)p.Equipped;
        int eff = pick.Efficiency;
        string msg = p.Upgrade(pick);
        Check(pick.Efficiency == eff + 1 && p.Money == 10, "upgrade: " + msg);

        p.Money = 0;
        ResourceItem gold = (ResourceItem)ItemFactory.Create(ItemKind.Gold);
        p.Inventory.Add(gold);
        p.Sell(gold);
        Check(p.Money == gold.Value && !p.Inventory.Contains(gold), "selling gold gives $" + gold.Value);

        Inventory inv = new Inventory(2);
        inv.Add(ItemFactory.Create(ItemKind.Coal));
        inv.Add(ItemFactory.Create(ItemKind.Coal));
        Check(!inv.Add(ItemFactory.Create(ItemKind.Coal)) && inv.IsFull, "inventory refuses items when full");

        // Bomb clears a 3x3 area and is consumed
        Maze m = new Maze(7, 7);
        for (int x = 1; x < 6; x++) for (int y = 1; y < 6; y++) m.Set(x, y, BlockFactory.Create(BlockType.Dirt));
        m.Set(3, 1, BlockFactory.Create(BlockType.Gold));
        m.Set(1, 2, BlockFactory.Create(BlockType.Empty));
        Player bomber = new Player(1, 2);
        Bomb bomb = null;
        foreach (Item i in bomber.Inventory.Items) if (i is Bomb) bomb = (Bomb)i;
        bomber.Equip(bomb);
        bomber.TryMove(Facing.Right, m);   // just turns: (2,2) is dirt
        MineOutcome o = bomber.Mine(m);
        int cleared = 0;
        for (int x = 1; x <= 3; x++) for (int y = 1; y <= 3; y++) if (m.Get(x, y).IsWalkable) cleared++;
        Check(o.Result == MineResult.Exploded && cleared == 9, "bomb clears the 3x3 area");
        Check(bomber.Inventory.CountOf(ItemKind.Gold) == 1, "bomb collects the ore it destroys");
        Check(!bomber.Inventory.Contains(bomb) && bomber.Equipped is Pickaxe, "bomb is used up, pickaxe re-equipped");

        Player walker = new Player(1, 2);
        walker.TryMove(Facing.Up, m);
        Check(walker.Row == 1, "player walks into dug tunnels");
        Player blocked = new Player(1, 2);
        blocked.TryMove(Facing.Left, m);
        Check(blocked.Col == 1 && blocked.Mine(m).Result == MineResult.SolidRock, "walls block movement and mining");
    }

    private static void MonsterTests()
    {
        Console.WriteLine("Monster behaviours (strategy pattern)");
        Random rnd = new Random(3);
        Player p = new Player(1, 1);
        new AttackBehaviour().Act(p, rnd);
        Check(p.Stamina == Player.MaxStamina - AttackBehaviour.Damage, "attack removes stamina");

        string msg = new StealBehaviour().Act(p, rnd);
        Check(p.Inventory.Count == 1 && p.Inventory.Contains(p.Equipped), "steal: " + msg + " (keeps equipped tool)");
        Check(!new StealBehaviour().CanAct(p), "nothing left to steal");

        Maze m = new Maze(21, 21);
        Monster ghost = new Monster(2.0, rnd, new IMonsterBehaviour[] { new AttackBehaviour() });
        ghost.Spawn(m, p);
        double startDist = Math.Abs(ghost.X - p.Col) + Math.Abs(ghost.Y - p.Row);
        string hit = null;
        for (int i = 0; i < 2000 && hit == null; i++) hit = ghost.Update(0.02, m, p);
        Check(startDist >= 6 && hit != null, "ghost spawns far away, flies through walls and reaches the player");
    }

    // A simple bot: walk/dig towards the nearest diamond, eat or buy food when tired.
    private static void BotPlaysLevel(int level, int seed)
    {
        LevelConfig cfg = LevelFactory.Create(level);
        Random rnd = new Random(seed);
        Maze m = MazeGenerator.Generate(cfg, rnd);
        Player p = new Player(m.StartCol, m.StartRow);
        Store store = new Store();
        double time = 0;
        int swings = 0;
        const double dt = 1.0 / 60;

        while (p.Diamonds < cfg.TargetDiamonds && time < cfg.TimeLimit)
        {
            time += dt;
            p.Update(dt);
            if (p.IsMoving) continue;

            if (p.Stamina < 5) Refuel(p, store);
            bool broke = p.Money < 50 && p.Stamina < 40;
            List<Facing> path = PathToNearest(m, p, broke);
            if (path == null) break;
            if (path.Count == 1 || !m.IsWalkable(Next(p, path[0], 0), Next(p, path[0], 1)))
            {
                p.TryMove(path[0], m);       // face it
                if (p.Mine(m).Result == MineResult.TooTired) { time += 3; p.Update(3); continue; }  // rest
                swings++;
                time += 0.2 - dt;            // a swing every 0.2 s while holding space
            }
            else p.TryMove(path[0], m);
        }
        Check(p.Diamonds >= cfg.TargetDiamonds && time < cfg.TimeLimit,
              "bot wins level " + level + " in " + (int)time + "s of " + cfg.TimeLimit + "s (" + swings +
              " swings, score " + p.Score + ", $" + p.Money + ")");
    }

    private static void Refuel(Player p, Store store)
    {
        foreach (Item i in p.Inventory.Items)
            if (i is FoodItem) { p.Eat((FoodItem)i); return; }
        List<Item> sell = new List<Item>();
        foreach (Item i in p.Inventory.Items)
            if (i is ResourceItem && !((ResourceItem)i).IsDiamond) sell.Add(i);
        foreach (Item i in sell) p.Sell((ResourceItem)i);
        foreach (Item i in store.Catalogue)            // catalogue order: burger, pizza, soda
            if (i is FoodItem && p.Money >= i.Value)
            {
                store.Buy(p, i);
                Refuel(p, store);
                return;
            }
    }

    private static int Next(Player p, Facing f, int axis)
    {
        int c = p.Col, r = p.Row;
        Maze.Step(f, ref c, ref r);
        return axis == 0 ? c : r;
    }

    // BFS through tunnels/dirt; the goal is the nearest diamond (or any ore when money is needed).
    private static List<Facing> PathToNearest(Maze m, Player p, bool anyOre)
    {
        Dictionary<long, long> prev = new Dictionary<long, long>();
        Dictionary<long, Facing> how = new Dictionary<long, Facing>();
        Queue<int[]> q = new Queue<int[]>();
        long start = p.Col * 1000L + p.Row;
        prev[start] = -1;
        q.Enqueue(new[] { p.Col, p.Row });
        Facing[] dirs = { Facing.Up, Facing.Down, Facing.Left, Facing.Right };
        while (q.Count > 0)
        {
            int[] c = q.Dequeue();
            foreach (Facing f in dirs)
            {
                int x = c[0], y = c[1];
                Maze.Step(f, ref x, ref y);
                Block b = m.Get(x, y);
                long key = x * 1000L + y;
                if (b == null || prev.ContainsKey(key)) continue;
                bool goal = b.Type == BlockType.Diamond || (anyOre && b is ResourceBlock);
                bool passable = b.Type == BlockType.Empty || b.Type == BlockType.Dirt;
                if (!goal && !passable) continue;
                prev[key] = c[0] * 1000L + c[1];
                how[key] = f;
                if (goal)
                {
                    List<Facing> path = new List<Facing>();
                    for (long k = key; prev[k] != -1; k = prev[k]) path.Insert(0, how[k]);
                    return path;
                }
                q.Enqueue(new[] { x, y });
            }
        }
        return null;
    }
}
