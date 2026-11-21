using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>How to play, plus a legend of tools, resources, the player and the monster.</summary>
    public class InstructionState : IGameState
    {
        private static readonly string[] Lines =
        {
            "HOW TO PLAY:",
            "1. USE WASD OR ARROW KEYS TO MOVE AND SELECT A CELL TO MINE",
            "2. PRESS (OR HOLD) THE SPACE BAR TO MINE THE SELECTED CELL",
            "",
            "",
            "",
            "HOW TO WIN:",
            "1. COLLECT THE TARGET NUMBER OF DIAMONDS WITHIN THE TIME LIMIT",
            "",
            "THINGS TO KNOW:",
            "1. YOU CAN SELL THE RESOURCES YOU COLLECT",
            "2. YOU CAN BUY FOOD AT THE STORE TO REGAIN STAMINA",
            "3. YOU CAN BUY BETTER TOOLS AND BOMBS AT THE STORE",
            "4. YOU CAN UPGRADE YOUR TOOLS",
            "5. A MONSTER APPEARS ONCE HALF THE TIME HAS PASSED",
            "6. THE MONSTER WILL ATTACK YOU OR STEAL YOUR ITEMS",
            "7. YOU CAN VIEW A MAP OF THE MAZE",
            "8. STAND STILL TO REST AND SLOWLY REGAIN A LITTLE STAMINA",
            "9. ESC PAUSES THE GAME"
        };

        private readonly Game _game;
        private readonly UiButton _back;

        public InstructionState(Game game)
        {
            _game = game;
            _back = new UiButton("BACK", 420, 545, 160, 44, Ui.Yellow, Ui.YellowDark, Color.White, 14);
        }

        public void Enter() { }

        public void Update(double dt)
        {
            if (_back.WasClicked() || SplashKit.KeyTyped(KeyCode.EscapeKey))
                _game.ChangeState(new MenuState(_game));
        }

        public void Draw()
        {
            SplashKit.DrawBitmap(Assets.Bmp("bg_menu"), 0, 0);

            // Left: rules on a black panel
            SplashKit.FillRectangle(Color.Black, 30, 25, 640, 505);
            for (int i = 0; i < Lines.Length; i++)
                Ui.Text(Lines[i], Color.White, Assets.BodyFont, 23, 48, 38 + i * 26);

            // Illustration: arrow key -> miner -> dirt cell with its strength
            Ui.Frame(Color.White, 170, 118, 52, 52);
            SplashKit.FillRectangle(Color.White, 182, 141, 22, 6);
            SplashKit.FillTriangle(Color.White, 204, 134, 214, 144, 204, 154);
            SplashKit.DrawBitmap(Assets.Bmp("player_right"), 250, 124);
            SplashKit.DrawBitmap(Assets.Bmp("tile_dirt"), 292, 124);
            Ui.Frame(Ui.Red, 292, 124, 40, 40);
            Ui.Text("2", Ui.Ink, Assets.BodyFont, 22, 307, 132);

            // Right: game elements on a white panel
            SplashKit.FillRectangle(Color.White, 690, 25, 280, 505);
            Ui.Text("GAME ELEMENTS:", Ui.Ink, Assets.BodyFont, 26, 702, 34);

            string[] tools = { "item_pickaxe_iron", "item_pickaxe_gold", "item_pickaxe_diamond", "item_bomb" };
            for (int i = 0; i < tools.Length; i++) SplashKit.DrawBitmap(Assets.Bmp(tools[i]), 742 + i * 46, 70);
            Ui.Centered("TOOLS", Ui.Ink, Assets.BodyFont, 22, 830, 104);

            string[] ores = { "item_coal", "item_iron", "item_gold", "item_diamond" };
            for (int i = 0; i < ores.Length; i++) SplashKit.DrawBitmap(Assets.Bmp(ores[i]), 742 + i * 46, 140);
            Ui.Centered("RESOURCES", Ui.Ink, Assets.BodyFont, 22, 830, 174);

            string[] food = { "item_burger", "item_pizza", "item_soda" };
            for (int i = 0; i < food.Length; i++) SplashKit.DrawBitmap(Assets.Bmp(food[i]), 765 + i * 46, 210);
            Ui.Centered("FOOD", Ui.Ink, Assets.BodyFont, 22, 830, 244);

            SplashKit.DrawBitmap(Assets.Bmp("player_big"), 790, 275);
            Ui.Centered("PLAYER", Ui.Ink, Assets.BodyFont, 22, 830, 357);
            SplashKit.DrawBitmap(Assets.Bmp("ghost_big"), 790, 390);
            Ui.Centered("MONSTER", Ui.Ink, Assets.BodyFont, 22, 830, 472);

            _back.Draw();
        }
    }
}
