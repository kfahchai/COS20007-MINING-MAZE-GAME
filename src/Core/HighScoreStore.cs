using System;
using System.Collections.Generic;
using System.IO;

namespace MiningMaze
{
    /// <summary>Keeps the best score for each level in a small text file ("level score" per line).</summary>
    public class HighScoreStore
    {
        private readonly string _file;
        private readonly Dictionary<int, int> _best = new Dictionary<int, int>();

        public HighScoreStore(string file)
        {
            _file = file;
            Load();
        }

        public int Get(int level)
        {
            int score;
            return _best.TryGetValue(level, out score) ? score : 0;
        }

        /// <summary>Records a score and returns the (possibly new) best for that level.</summary>
        public int Submit(int level, int score)
        {
            if (score > Get(level))
            {
                _best[level] = score;
                Save();
            }
            return Get(level);
        }

        private void Load()
        {
            if (!File.Exists(_file)) return;
            foreach (string line in File.ReadAllLines(_file))
            {
                string[] parts = line.Split(' ');
                int level, score;
                if (parts.Length == 2 && int.TryParse(parts[0], out level) && int.TryParse(parts[1], out score))
                    _best[level] = score;
            }
        }

        private void Save()
        {
            try
            {
                List<string> lines = new List<string>();
                foreach (KeyValuePair<int, int> kv in _best)
                    lines.Add(kv.Key + " " + kv.Value);
                File.WriteAllLines(_file, lines);
            }
            catch (IOException e)
            {
                Console.WriteLine("Could not save high scores: " + e.Message);
            }
        }
    }
}
