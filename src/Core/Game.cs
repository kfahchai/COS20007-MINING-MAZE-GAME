using System;
using System.Diagnostics;
using System.IO;
using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>A screen of the game (menu, instructions, play, result).</summary>
    public interface IGameState
    {
        void Enter();
        void Update(double dt);
        void Draw();
    }

    /// <summary>
    /// Owns the window and the main loop. State pattern: the game is always in exactly one
    /// IGameState, and screens ask the game to switch to the next one.
    /// </summary>
    public class Game
    {
        public const int ScreenWidth = 1000;
        public const int ScreenHeight = 600;

        private IGameState _current;
        private IGameState _next;
        private bool _quit;

        public Game()
        {
            Window = new Window("Mining-Maze Game", ScreenWidth, ScreenHeight);
            Assets.LoadAll();
            SoundManager.Instance.LoadAll(Assets.ResourceDir);
            HighScores = new HighScoreStore(Path.Combine(AppContext.BaseDirectory, "highscores.txt"));
            Random = new Random();
        }

        public Window Window { get; private set; }
        public HighScoreStore HighScores { get; private set; }
        public Random Random { get; private set; }

        public void ChangeState(IGameState state)
        {
            _next = state;
        }

        public void Quit()
        {
            _quit = true;
        }

        public void Run()
        {
            ChangeState(new MenuState(this));
            SoundManager.Instance.StartMusic();

            Stopwatch clock = Stopwatch.StartNew();
            double last = 0;
            while (!_quit)
            {
                SplashKit.ProcessEvents();
                if (SplashKit.QuitRequested() || SplashKit.WindowCloseRequested(Window)) break;

                if (_next != null)
                {
                    _current = _next;
                    _next = null;
                    _current.Enter();
                }

                double now = clock.Elapsed.TotalSeconds;
                double dt = Math.Min(0.05, now - last);
                last = now;

                if (SplashKit.KeyTyped(KeyCode.F12Key))
                    SplashKit.TakeScreenshot(Window, "mining-maze");

                _current.Update(dt);
                SplashKit.ClearScreen(Color.Black);
                _current.Draw();
                SplashKit.RefreshScreen(60);
            }
            SoundManager.Instance.StopMusic();
            SplashKit.CloseAllWindows();
        }
    }
}
