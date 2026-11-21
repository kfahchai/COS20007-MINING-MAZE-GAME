using System;
using System.Collections.Generic;
using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>The actual game: maze view, player, ghost, timer, pause menu and the side panel.</summary>
    public class PlayState : IGameState
    {
        public const int CellSize = 40;
        public const int ViewX = SidePanel.Width;
        public const int ViewWidth = Game.ScreenWidth - SidePanel.Width;
        public const int ViewHeight = Game.ScreenHeight;
        private const double MineRepeat = 0.2;
        private const double ToastTime = 2.6;

        private class Toast
        {
            public string Text;
            public double Age;
        }

        private readonly Game _game;
        private readonly LevelConfig _cfg;
        private readonly Maze _maze;
        private readonly Player _player;
        private readonly Monster _monster;
        private readonly SidePanel _panel;
        private readonly List<Toast> _toasts = new List<Toast>();

        private readonly UiButton _resume, _end, _exit;
        private double _timeLeft;
        private double _mineCooldown;
        private double _swing;           // pickaxe swing animation timer
        private double _clock;           // for bobbing / flashing effects
        private bool _paused;
        private bool _finished;

        public PlayState(Game game, LevelConfig cfg)
        {
            _game = game;
            _cfg = cfg;
            _maze = MazeGenerator.Generate(cfg, game.Random);
            _player = new Player(_maze.StartCol, _maze.StartRow);
            _monster = new Monster(cfg.GhostSpeed / CellSize, game.Random,
                                   new IMonsterBehaviour[] { new AttackBehaviour(), new StealBehaviour() });
            _panel = new SidePanel(_player, _maze, _monster, new Store(), cfg, ShowToast);
            _timeLeft = cfg.TimeLimit;

            // Observer: react to every collected resource
            _player.ItemCollected += item => ShowToast("+1 " + item.Name);

            double cx = ViewX + ViewWidth / 2.0 - 90;
            _resume = new UiButton("RESUME", cx, 240, 180, 44, Ui.Blue, Ui.BlueDark, Color.White, 14);
            _end = new UiButton("END", cx, 300, 180, 44, Ui.Blue, Ui.BlueDark, Color.White, 14);
            _exit = new UiButton("EXIT", cx, 360, 180, 44, Ui.Blue, Ui.BlueDark, Color.White, 14);
        }

        public void Enter()
        {
            ShowToast("Collect " + _cfg.TargetDiamonds + " diamonds before time runs out!");
        }

        private void ShowToast(string text)
        {
            foreach (Toast t in _toasts)
                if (t.Text == text) { t.Age = 0; return; }
            _toasts.Add(new Toast { Text = text });
            if (_toasts.Count > 4) _toasts.RemoveAt(0);
        }

        // ================================================================= update
        public void Update(double dt)
        {
            _clock += dt;
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                _toasts[i].Age += dt;
                if (_toasts[i].Age > ToastTime) _toasts.RemoveAt(i);
            }

            if (_paused)
            {
                UpdatePaused();
                return;
            }

            if (SplashKit.KeyTyped(KeyCode.EscapeKey) || MenuButtonClicked())
            {
                _paused = true;
                return;
            }
            if (SoundButtonClicked()) SoundManager.Instance.ToggleMute();

            _timeLeft -= dt;
            _panel.TimeLeft = Math.Max(0, _timeLeft);
            if (_timeLeft <= 0)
            {
                Finish(false, "Time's up! You had " + _player.Diamonds + "/" + _cfg.TargetDiamonds + " diamonds.");
                return;
            }

            _player.Update(dt);
            HandleMovement();
            HandleMining(dt);
            UpdateMonster(dt);
            _panel.Update();

            if (_player.Diamonds >= _cfg.TargetDiamonds)
            {
                int bonus = (int)_timeLeft;
                _player.Score += bonus;
                Finish(true, "Target reached with " + bonus + "s left (+" + bonus + " bonus)!");
            }
        }

        private void HandleMovement()
        {
            Facing dir;
            if (SplashKit.KeyDown(KeyCode.WKey) || SplashKit.KeyDown(KeyCode.UpKey)) dir = Facing.Up;
            else if (SplashKit.KeyDown(KeyCode.SKey) || SplashKit.KeyDown(KeyCode.DownKey)) dir = Facing.Down;
            else if (SplashKit.KeyDown(KeyCode.AKey) || SplashKit.KeyDown(KeyCode.LeftKey)) dir = Facing.Left;
            else if (SplashKit.KeyDown(KeyCode.DKey) || SplashKit.KeyDown(KeyCode.RightKey)) dir = Facing.Right;
            else return;
            _player.TryMove(dir, _maze);
        }

        private void HandleMining(double dt)
        {
            _mineCooldown -= dt;
            _swing = Math.Max(0, _swing - dt);
            bool pressed = SplashKit.KeyTyped(KeyCode.SpaceKey);
            bool held = SplashKit.KeyDown(KeyCode.SpaceKey) && _mineCooldown <= 0;
            if ((!pressed && !held) || _player.IsMoving) return;

            _mineCooldown = MineRepeat;
            MineOutcome outcome = _player.Mine(_maze);
            SoundManager sound = SoundManager.Instance;
            switch (outcome.Result)
            {
                case MineResult.SolidRock:
                    if (pressed) ShowToast("Solid rock! Walls can't be mined.");
                    break;
                case MineResult.TooTired:
                    ShowToast("Too tired! Eat some food from your inventory.");
                    break;
                case MineResult.Hit:
                    _swing = 0.15;
                    sound.Play("mine");
                    break;
                case MineResult.Broken:
                    _swing = 0.15;
                    sound.Play("break");
                    break;
                case MineResult.Collected:
                    _swing = 0.15;
                    sound.Play("collect");
                    break;
                case MineResult.InventoryFull:
                    sound.Play("break");
                    ShowToast("Inventory full! Sell or remove items.");
                    break;
                case MineResult.Exploded:
                    sound.Play("bomb");
                    ShowToast("BOOM! Bomb used.");
                    break;
            }
        }

        private void UpdateMonster(double dt)
        {
            if (!_monster.Active && _timeLeft <= _cfg.TimeLimit / 2)
            {
                _monster.Spawn(_maze, _player);
                SoundManager.Instance.Play("ghost");
                ShowToast("A ghost has appeared! Watch your items...");
            }

            string message = _monster.Update(dt, _maze, _player);
            if (message != null)
            {
                SoundManager.Instance.Play(message.Contains("stole") ? "steal" : "hurt");
                ShowToast(message);
            }
        }

        private void UpdatePaused()
        {
            if (_resume.WasClicked() || SplashKit.KeyTyped(KeyCode.EscapeKey)) _paused = false;
            else if (_end.WasClicked()) Finish(false, "You ended the game.");
            else if (_exit.WasClicked()) _game.Quit();
        }

        private void Finish(bool won, string reason)
        {
            if (_finished) return;
            _finished = true;
            _game.ChangeState(new ResultState(_game, _cfg.Level, won, _player.Score, reason));
        }

        // Top-right buttons over the maze view
        private const double SoundBtnX = Game.ScreenWidth - 76, MenuBtnX = Game.ScreenWidth - 40, TopBtnY = 10, TopBtn = 30;

        private bool SoundButtonClicked() { return Ui.MouseIn(SoundBtnX, TopBtnY, TopBtn, TopBtn) && Ui.Clicked(); }
        private bool MenuButtonClicked() { return Ui.MouseIn(MenuBtnX, TopBtnY, TopBtn, TopBtn) && Ui.Clicked(); }

        // ================================================================= drawing
        public void Draw()
        {
            double camX, camY;
            Camera(out camX, out camY);
            DrawMaze(camX, camY);
            DrawMonster(camX, camY);
            DrawPlayer(camX, camY);
            DrawTopBar();
            DrawToasts();
            _panel.Draw();
            if (_paused) DrawPauseMenu();
        }

        private void Camera(out double camX, out double camY)
        {
            double px = _player.DrawCol * CellSize + CellSize / 2.0;
            double py = _player.DrawRow * CellSize + CellSize / 2.0;
            camX = Clamp(px - ViewWidth / 2.0, 0, _maze.Width * CellSize - ViewWidth);
            camY = Clamp(py - ViewHeight / 2.0, 0, _maze.Height * CellSize - ViewHeight);
        }

        private static double Clamp(double v, double lo, double hi)
        {
            if (hi < lo) return lo;
            return v < lo ? lo : v > hi ? hi : v;
        }

        private void DrawMaze(double camX, double camY)
        {
            int c0 = (int)(camX / CellSize), r0 = (int)(camY / CellSize);
            int selCol, selRow;
            _player.SelectedCell(out selCol, out selRow);

            for (int c = c0; c <= c0 + ViewWidth / CellSize + 1; c++)
                for (int r = r0; r <= r0 + ViewHeight / CellSize + 1; r++)
                {
                    Block b = _maze.Get(c, r);
                    if (b == null) continue;
                    double x = ViewX + c * CellSize - camX, y = r * CellSize - camY;
                    if (b.Texture == null) SplashKit.FillRectangle(Color.Black, x, y, CellSize, CellSize);
                    else SplashKit.DrawBitmap(Assets.Bmp(b.Texture), x, y);

                    bool selected = c == selCol && r == selRow;
                    if (b.IsMineable && (b.IsDamaged || selected))
                        Ui.Shadowed(b.Strength.ToString(), Color.White, Assets.BodyFont, 22,
                                    x + CellSize / 2.0 - 4, y + 9);
                }

            double sx = ViewX + selCol * CellSize - camX, sy = selRow * CellSize - camY;
            Ui.Frame(Ui.Red, sx, sy, CellSize, CellSize);
        }

        private void DrawPlayer(double camX, double camY)
        {
            double x = ViewX + _player.DrawCol * CellSize - camX;
            double y = _player.DrawRow * CellSize - camY;
            SplashKit.DrawBitmap(Assets.Bmp(_player.LooksRight ? "player_right" : "player_left"), x, y);

            if (_swing > 0 && _player.Equipped != null)
            {
                int dc = 0, dr = 0;
                Maze.Step(_player.Facing, ref dc, ref dr);
                SplashKit.DrawBitmap(Assets.Bmp(_player.Equipped.Icon), x + 4 + dc * 22, y + 4 + dr * 22 - 6);
            }
        }

        private void DrawMonster(double camX, double camY)
        {
            if (!_monster.Visible) return;
            double x = ViewX + _monster.X * CellSize - camX;
            double y = _monster.Y * CellSize - camY + Math.Sin(_clock * 4) * 4;
            if (x < ViewX - CellSize || x > Game.ScreenWidth || y < -CellSize || y > ViewHeight) return;
            SplashKit.DrawBitmap(Assets.Bmp("ghost"), x, y);
        }

        private void DrawTopBar()
        {
            Ui.Shadowed("Score: " + _player.Score, Color.White, Assets.BodyFont, 32, SoundBtnX - 170, 8);

            SplashKit.FillRectangle(Ui.Salmon, SoundBtnX, TopBtnY, TopBtn, TopBtn);
            Ui.Frame(Ui.SalmonDark, SoundBtnX, TopBtnY, TopBtn, TopBtn);
            SplashKit.FillRectangle(Color.White, SoundBtnX + 6, TopBtnY + 11, 6, 8);
            SplashKit.FillTriangle(Color.White, SoundBtnX + 10, TopBtnY + 15, SoundBtnX + 19, TopBtnY + 6,
                                   SoundBtnX + 19, TopBtnY + 24);
            if (SoundManager.Instance.Muted)
            {
                SplashKit.DrawLine(Ui.Ink, SoundBtnX + 21, TopBtnY + 10, SoundBtnX + 27, TopBtnY + 20);
                SplashKit.DrawLine(Ui.Ink, SoundBtnX + 27, TopBtnY + 10, SoundBtnX + 21, TopBtnY + 20);
            }
            else
            {
                SplashKit.FillRectangle(Color.White, SoundBtnX + 22, TopBtnY + 11, 2, 8);
                SplashKit.FillRectangle(Color.White, SoundBtnX + 25, TopBtnY + 8, 2, 14);
            }

            SplashKit.FillRectangle(Ui.Salmon, MenuBtnX, TopBtnY, TopBtn, TopBtn);
            Ui.Frame(Ui.SalmonDark, MenuBtnX, TopBtnY, TopBtn, TopBtn);
            SplashKit.FillRectangle(Color.White, MenuBtnX + 9, TopBtnY + 8, 4, 14);
            SplashKit.FillRectangle(Color.White, MenuBtnX + 17, TopBtnY + 8, 4, 14);
        }

        private void DrawToasts()
        {
            double centre = ViewX + ViewWidth / 2.0;
            for (int i = 0; i < _toasts.Count; i++)
            {
                string text = _toasts[i].Text;
                int w = Ui.Width(text, Assets.BodyFont, 24);
                double y = 56 + i * 32;
                SplashKit.FillRectangle(SplashKit.RGBAColor(0, 0, 0, 170), centre - w / 2.0 - 10, y, w + 20, 28);
                Ui.Centered(text, Color.White, Assets.BodyFont, 24, centre, y + 2);
            }
        }

        private void DrawPauseMenu()
        {
            SplashKit.FillRectangle(SplashKit.RGBAColor(0, 0, 0, 150), ViewX, 0, ViewWidth, ViewHeight);
            Ui.ShadowedCentered("PAUSED", Color.White, Assets.TitleFont, 32, ViewX + ViewWidth / 2.0, 170);
            _resume.Draw();
            _end.Draw();
            _exit.Draw();
        }
    }
}
