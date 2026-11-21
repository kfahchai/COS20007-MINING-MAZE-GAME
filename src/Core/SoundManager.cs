using System.Collections.Generic;
using System.IO;
using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>
    /// Singleton pattern: one shared place that owns every sound and the mute setting,
    /// so any screen can play a sound without passing references around.
    /// </summary>
    public sealed class SoundManager
    {
        private static SoundManager _instance;
        private readonly Dictionary<string, SoundEffect> _effects = new Dictionary<string, SoundEffect>();
        private Music _theme;

        private SoundManager() { }

        public static SoundManager Instance
        {
            get
            {
                if (_instance == null) _instance = new SoundManager();
                return _instance;
            }
        }

        public bool Muted { get; private set; }

        public void LoadAll(string resourceDir)
        {
            string dir = Path.Combine(resourceDir, "sounds");
            foreach (string name in new[] { "mine", "break", "collect", "buy", "click", "hurt", "steal",
                                            "bomb", "eat", "win", "lose", "ghost" })
            {
                string file = Path.Combine(dir, name + ".wav");
                if (File.Exists(file)) _effects[name] = new SoundEffect(name, file);
            }
            string theme = Path.Combine(dir, "theme.wav");
            if (File.Exists(theme)) _theme = new Music("theme", theme);
        }

        public void Play(string name)
        {
            SoundEffect fx;
            if (!Muted && _effects.TryGetValue(name, out fx))
                SplashKit.PlaySoundEffect(fx, 0.6);
        }

        public void StartMusic()
        {
            if (Muted || _theme == null || SplashKit.MusicPlaying()) return;
            SplashKit.PlayMusic(_theme, -1, 0.35);
        }

        public void StopMusic()
        {
            SplashKit.StopMusic();
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            if (Muted) StopMusic();
            else StartMusic();
        }
    }
}
