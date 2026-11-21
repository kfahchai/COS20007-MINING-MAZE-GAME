using System;
using System.Collections.Generic;

namespace MiningMaze
{
    /// <summary>Strategy pattern: what the ghost does when it catches the player.</summary>
    public interface IMonsterBehaviour
    {
        bool CanAct(Player player);
        string Act(Player player, Random rnd);
    }

    public class AttackBehaviour : IMonsterBehaviour
    {
        public const int Damage = 20;

        public bool CanAct(Player player) { return true; }

        public string Act(Player player, Random rnd)
        {
            player.LoseStamina(Damage);
            return "The ghost attacked you! -" + Damage + " stamina";
        }
    }

    public class StealBehaviour : IMonsterBehaviour
    {
        public bool CanAct(Player player)
        {
            foreach (Item i in player.Inventory.Items)
                if (i != player.Equipped) return true;
            return false;
        }

        public string Act(Player player, Random rnd)
        {
            List<Item> loot = new List<Item>();
            foreach (Item i in player.Inventory.Items)
                if (i != player.Equipped) loot.Add(i);
            Item stolen = loot[rnd.Next(loot.Count)];
            player.Discard(stolen);
            return "The ghost stole your " + stolen.Name + "!";
        }
    }

    /// <summary>
    /// A ghost that appears once half the time is gone. It floats straight through walls
    /// towards the player, uses one of its behaviours, then vanishes and reappears elsewhere.
    /// </summary>
    public class Monster
    {
        public const double CatchDistance = 0.55;  // in cells
        public const double Cooldown = 4.0;        // seconds hidden after acting

        private readonly List<IMonsterBehaviour> _behaviours;
        private readonly Random _rnd;
        private double _hiddenFor;

        public Monster(double speedCellsPerSecond, Random rnd, IEnumerable<IMonsterBehaviour> behaviours)
        {
            Speed = speedCellsPerSecond;
            _rnd = rnd;
            _behaviours = new List<IMonsterBehaviour>(behaviours);
        }

        public bool Active { get; private set; }
        public double X { get; private set; }   // position in cells (can be fractional)
        public double Y { get; private set; }
        public double Speed { get; private set; }
        public bool Visible { get { return Active && _hiddenFor <= 0; } }

        public void Spawn(Maze maze, Player player)
        {
            Active = true;
            Teleport(maze, player);
        }

        /// <summary>Moves the ghost; returns a message when it does something to the player.</summary>
        public string Update(double dt, Maze maze, Player player)
        {
            if (!Active) return null;
            if (_hiddenFor > 0)
            {
                _hiddenFor -= dt;
                return null;
            }

            double dx = player.DrawCol - X, dy = player.DrawRow - Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            if (dist > CatchDistance)
            {
                double step = Math.Min(dist, Speed * dt);
                X += dx / dist * step;
                Y += dy / dist * step;
                return null;
            }

            List<IMonsterBehaviour> usable = new List<IMonsterBehaviour>();
            foreach (IMonsterBehaviour b in _behaviours)
                if (b.CanAct(player)) usable.Add(b);
            string message = usable.Count == 0 ? null : usable[_rnd.Next(usable.Count)].Act(player, _rnd);

            Teleport(maze, player);
            _hiddenFor = Cooldown;
            return message;
        }

        private void Teleport(Maze maze, Player player)
        {
            int minDist = Math.Max(6, maze.Width / 3);
            for (int attempt = 0; attempt < 200; attempt++)
            {
                int x = _rnd.Next(maze.Width), y = _rnd.Next(maze.Height);
                if (Math.Abs(x - player.Col) + Math.Abs(y - player.Row) >= minDist)
                {
                    X = x;
                    Y = y;
                    return;
                }
            }
            X = maze.Width - 2;
            Y = maze.Height - 2;
        }
    }
}
