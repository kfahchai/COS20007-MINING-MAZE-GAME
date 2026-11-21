using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>Title screen: pick a level or read the instructions.</summary>
    public class MenuState : IGameState
    {
        private readonly Game _game;
        private readonly UiButton[] _levels = new UiButton[LevelFactory.LevelCount];
        private readonly UiButton _instructions;

        public MenuState(Game game)
        {
            _game = game;
            for (int i = 0; i < _levels.Length; i++)
                _levels[i] = new UiButton("LEVEL " + (i + 1), 410, 205 + i * 62, 180, 46, Ui.Blue, Ui.BlueDark, Color.White, 14);
            _instructions = new UiButton("INSTRUCTION", 395, 545, 210, 44, Ui.Yellow, Ui.YellowDark, Color.White, 13);
        }

        public void Enter()
        {
            SoundManager.Instance.StartMusic();
        }

        public void Update(double dt)
        {
            for (int i = 0; i < _levels.Length; i++)
                if (_levels[i].WasClicked())
                    _game.ChangeState(new PlayState(_game, LevelFactory.Create(i + 1)));

            if (_instructions.WasClicked())
                _game.ChangeState(new InstructionState(_game));
        }

        public void Draw()
        {
            SplashKit.DrawBitmap(Assets.Bmp("bg_menu"), 0, 0);
            Ui.ShadowedCentered("MINING-MAZE GAME", Ui.Ink, Assets.TitleFont, 30, 500, 128);
            Ui.Centered("Dig. Collect diamonds. Beat the clock.", Ui.Ink, Assets.BodyFont, 26, 500, 168);

            for (int i = 0; i < _levels.Length; i++)
            {
                _levels[i].Draw();
                int best = _game.HighScores.Get(i + 1);
                if (best > 0)
                    Ui.Text("Best: " + best, Ui.Ink, Assets.BodyFont, 24, 605, _levels[i].Y + 10);
            }
            _instructions.Draw();
        }
    }
}
