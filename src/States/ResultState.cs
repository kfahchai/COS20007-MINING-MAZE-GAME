using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>"YOU WON" / "YOU LOSE" screen with the current and best score for the level.</summary>
    public class ResultState : IGameState
    {
        private readonly Game _game;
        private readonly bool _won;
        private readonly int _score;
        private readonly int _best;
        private readonly string _reason;
        private readonly UiButton _back;

        public ResultState(Game game, int level, bool won, int score, string reason)
        {
            _game = game;
            _won = won;
            _score = score;
            _reason = reason;
            _best = game.HighScores.Submit(level, score);
            _back = new UiButton("BACK TO MENU", 390, 400, 220, 46, Ui.Blue, Ui.BlueDark, Color.White, 12);
        }

        public void Enter()
        {
            SoundManager.Instance.Play(_won ? "win" : "lose");
            SoundManager.Instance.StartMusic();
        }

        public void Update(double dt)
        {
            if (_back.WasClicked() || SplashKit.KeyTyped(KeyCode.ReturnKey))
                _game.ChangeState(new MenuState(_game));
        }

        public void Draw()
        {
            SplashKit.DrawBitmap(Assets.Bmp("bg_menu"), 0, 0);
            Ui.ShadowedCentered(_won ? "YOU WON" : "YOU LOSE", Ui.Ink, Assets.TitleFont, 36, 500, 170);
            Ui.Centered("Highest Score: " + _best, Ui.Ink, Assets.TitleFont, 18, 500, 245);
            Ui.Centered("Current Score: " + _score, Ui.Ink, Assets.TitleFont, 14, 500, 285);
            Ui.Centered(_reason, Ui.Ink, Assets.BodyFont, 26, 500, 325);
            _back.Draw();
        }
    }
}
