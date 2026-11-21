using System;

namespace MiningMaze
{
    public enum MineResult { Nothing, SolidRock, TooTired, Hit, Broken, Collected, InventoryFull, Exploded }

    /// <summary>Result of one swing of the tool, used by the play screen for sounds and messages.</summary>
    public class MineOutcome
    {
        public MineResult Result;
        public int ItemsCollected;
        public Item LastItem;
    }

    /// <summary>
    /// The miner. Holds position, stamina, money, score and the inventory, and knows how to
    /// move through tunnels and dig. It never draws anything (see PlayState / Hud).
    /// </summary>
    public class Player
    {
        public const int MaxStamina = 100;
        public const int StaminaPerHit = 1;
        public const double StepTime = 0.13;   // seconds to walk one cell
        public const int RestCap = 30;          // resting only refills stamina up to this
        public const double RestDelay = 2.0;    // seconds without mining before resting kicks in
        public const double RestRate = 2.0;     // stamina per second while resting

        private double _sinceSwing;
        private double _restBuffer;

        private double _moveProgress = 1.0;
        private int _fromCol, _fromRow;

        public Player(int col, int row)
        {
            Col = _fromCol = col;
            Row = _fromRow = row;
            Facing = Facing.Right;
            LooksRight = true;
            Stamina = MaxStamina;
            Money = 100;
            Inventory = new Inventory();

            ToolItem starter = (ToolItem)ItemFactory.Create(ItemKind.IronPickaxe);
            Inventory.Add(starter);
            Inventory.Add(ItemFactory.Create(ItemKind.Bomb));
            Equipped = starter;
        }

        /// <summary>Observer pattern: raised whenever a resource is added to the inventory.</summary>
        public event Action<Item> ItemCollected;

        public int Col { get; private set; }
        public int Row { get; private set; }
        public Facing Facing { get; private set; }
        public bool LooksRight { get; private set; }
        public int Stamina { get; private set; }
        public int Money { get; set; }
        public int Score { get; set; }
        public Inventory Inventory { get; private set; }
        public ToolItem Equipped { get; private set; }
        public bool IsMoving { get { return _moveProgress < 1.0; } }

        /// <summary>Smoothly interpolated position in cells, used for drawing.</summary>
        public double DrawCol { get { return _fromCol + (Col - _fromCol) * _moveProgress; } }
        public double DrawRow { get { return _fromRow + (Row - _fromRow) * _moveProgress; } }

        public int Diamonds { get { return Inventory.CountOf(ItemKind.Diamond); } }

        public void SelectedCell(out int col, out int row)
        {
            col = Col;
            row = Row;
            Maze.Step(Facing, ref col, ref row);
        }

        public bool IsResting { get { return _sinceSwing >= RestDelay && Stamina < RestCap; } }

        public void Update(double dt)
        {
            if (_moveProgress < 1.0)
                _moveProgress = Math.Min(1.0, _moveProgress + dt / StepTime);

            // Resting slowly restores a little stamina, so the player can never get stuck.
            _sinceSwing += dt;
            if (IsResting)
            {
                _restBuffer += RestRate * dt;
                int gain = (int)_restBuffer;
                _restBuffer -= gain;
                Stamina = Math.Min(RestCap, Stamina + gain);
            }
        }

        /// <summary>Turns to face a direction and walks into the cell if it is an open tunnel.</summary>
        public bool TryMove(Facing dir, Maze maze)
        {
            Facing = dir;
            if (dir == Facing.Left) LooksRight = false;
            if (dir == Facing.Right) LooksRight = true;
            if (IsMoving) return false;

            int c = Col, r = Row;
            Maze.Step(dir, ref c, ref r);
            if (!maze.IsWalkable(c, r)) return false;

            _fromCol = Col;
            _fromRow = Row;
            Col = c;
            Row = r;
            _moveProgress = 0.0;
            return true;
        }

        /// <summary>Swings the equipped tool at the selected cell.</summary>
        public MineOutcome Mine(Maze maze)
        {
            MineOutcome outcome = new MineOutcome();
            int c, r;
            SelectedCell(out c, out r);
            Block target = maze.Get(c, r);

            if (target == null || target.Type == BlockType.Wall)
            {
                outcome.Result = MineResult.SolidRock;
                return outcome;
            }
            if (!target.IsMineable)
            {
                outcome.Result = MineResult.Nothing;
                return outcome;
            }
            if (Stamina < StaminaPerHit)
            {
                outcome.Result = MineResult.TooTired;
                return outcome;
            }

            Stamina -= StaminaPerHit;
            _sinceSwing = 0;

            Bomb bomb = Equipped as Bomb;
            if (bomb != null)
            {
                for (int dx = -bomb.Radius; dx <= bomb.Radius; dx++)
                    for (int dy = -bomb.Radius; dy <= bomb.Radius; dy++)
                    {
                        Block b = maze.Get(c + dx, r + dy);
                        if (b != null && b.IsMineable)
                            Break(maze, c + dx, r + dy, b, outcome);
                    }
                Inventory.Remove(bomb);
                Equip(Inventory.BestPickaxe());
                outcome.Result = MineResult.Exploded;
                return outcome;
            }

            if (!target.Hit(Equipped == null ? 1 : Equipped.Efficiency))
            {
                outcome.Result = MineResult.Hit;
                return outcome;
            }

            Break(maze, c, r, target, outcome);
            if (outcome.Result == MineResult.Nothing) outcome.Result = MineResult.Broken;
            return outcome;
        }

        private void Break(Maze maze, int col, int row, Block block, MineOutcome outcome)
        {
            maze.Set(col, row, BlockFactory.Create(BlockType.Empty));
            ResourceBlock ore = block as ResourceBlock;
            if (ore == null) return;

            ResourceItem item = (ResourceItem)ItemFactory.Create(ore.Drop);
            Score += item.Points;
            if (Inventory.Add(item))
            {
                outcome.ItemsCollected++;
                outcome.LastItem = item;
                outcome.Result = MineResult.Collected;
                if (ItemCollected != null) ItemCollected(item);
            }
            else if (outcome.Result != MineResult.Collected)
            {
                outcome.Result = MineResult.InventoryFull;
            }
        }

        public void Equip(ToolItem tool)
        {
            if (tool == null || Inventory.Contains(tool)) Equipped = tool;
        }

        public bool Eat(FoodItem food)
        {
            if (!Inventory.Contains(food)) return false;
            Inventory.Remove(food);
            Stamina = Math.Min(MaxStamina, Stamina + food.Stamina);
            return true;
        }

        public bool Sell(ResourceItem resource)
        {
            if (!Inventory.Contains(resource)) return false;
            Inventory.Remove(resource);
            Money += resource.Value;
            return true;
        }

        public string Upgrade(ToolItem tool)
        {
            if (!tool.CanUpgrade) return tool.Name + " is fully upgraded";
            if (Money < tool.UpgradeCost) return "Upgrade costs $" + tool.UpgradeCost;
            Money -= tool.UpgradeCost;
            tool.Upgrade();
            return "Upgraded to " + tool.Name + "!";
        }

        /// <summary>Removes an item. Losing the equipped tool switches to the best pickaxe left.</summary>
        public void Discard(Item item)
        {
            Inventory.Remove(item);
            if (item == Equipped) Equip(Inventory.BestPickaxe());
        }

        public void LoseStamina(int amount)
        {
            Stamina = Math.Max(0, Stamina - amount);
        }
    }
}
