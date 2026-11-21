using System;
using System.Collections.Generic;
using SplashKitSDK;

namespace MiningMaze
{
    public enum PanelMode { Inventory, Store, Map }

    /// <summary>
    /// The left-hand board: level, stamina, time, diamonds and money on top, and a tabbed
    /// area below that shows the inventory, the store or the map.
    /// </summary>
    public class SidePanel
    {
        public const int Width = 240;
        private const int SlotSize = 40, SlotGap = 4, Cols = 5, GridX = 12, GridY = 200;

        private readonly Player _player;
        private readonly Maze _maze;
        private readonly Monster _monster;
        private readonly Store _store;
        private readonly LevelConfig _cfg;
        private readonly Action<string> _toast;

        private readonly UiButton _mapTab, _storeTab, _invTab;
        private readonly UiButton _actionA, _actionB, _buy;
        private int _selectedItem = -1;
        private int _selectedStore = -1;

        public SidePanel(Player player, Maze maze, Monster monster, Store store, LevelConfig cfg, Action<string> toast)
        {
            _player = player;
            _maze = maze;
            _monster = monster;
            _store = store;
            _cfg = cfg;
            _toast = toast;
            Mode = PanelMode.Inventory;

            _mapTab = new UiButton("MAP", 6, 556, 68, 32, Ui.Mint, SplashKit.RGBColor(70, 180, 140), Ui.Ink, 9);
            _storeTab = new UiButton("STORE", 80, 556, 72, 32, Ui.Beige, SplashKit.RGBColor(200, 185, 150), Ui.Ink, 9);
            _invTab = new UiButton("INVENTORY", 158, 556, 78, 32, SplashKit.RGBColor(232, 196, 140),
                                   SplashKit.RGBColor(180, 140, 90), Ui.Ink, 7);
            _actionA = new UiButton("USE", 12, 478, 104, 30, Ui.Salmon, Ui.SalmonDark, Color.White, 8);
            _actionB = new UiButton("REMOVE", 124, 478, 104, 30, Ui.Salmon, Ui.SalmonDark, Color.White, 8);
            _buy = new UiButton("BUY", 12, 456, 216, 34, Ui.Salmon, Ui.SalmonDark, Color.White, 10);
        }

        public PanelMode Mode { get; set; }

        public double TimeLeft { get; set; }

        // ------------------------------------------------------------------ input
        public void Update()
        {
            if (_mapTab.WasClicked()) Mode = PanelMode.Map;
            if (_storeTab.WasClicked()) Mode = PanelMode.Store;
            if (_invTab.WasClicked() || (Ui.MouseIn(196, 152, 40, 40) && Ui.Clicked())) Mode = PanelMode.Inventory;

            if (Mode == PanelMode.Inventory) UpdateInventory();
            else if (Mode == PanelMode.Store) UpdateStore();
        }

        private void UpdateInventory()
        {
            int slot = SlotUnderMouse(_player.Inventory.Capacity);
            if (slot >= 0 && Ui.Clicked())
                _selectedItem = slot < _player.Inventory.Count ? slot : -1;

            Item item = _player.Inventory.ItemAt(_selectedItem);
            if (item == null) return;
            ConfigureActions(item);

            if (_actionA.WasClicked()) DoPrimary(item);
            else if (_actionB.WasClicked()) DoSecondary(item);
        }

        private void ConfigureActions(Item item)
        {
            _actionA.Enabled = true;
            _actionB.Label = "REMOVE";
            switch (item.Category)
            {
                case ItemCategory.Tool:
                    ToolItem tool = (ToolItem)item;
                    _actionA.Label = tool == _player.Equipped ? "IN USE" : "USE";
                    _actionA.Enabled = tool != _player.Equipped;
                    if (tool.CanUpgrade) _actionB.Label = "UPGRADE $" + tool.UpgradeCost;
                    break;
                case ItemCategory.Resource:
                    _actionA.Label = "SELL $" + item.Value;
                    break;
                case ItemCategory.Food:
                    _actionA.Label = "EAT";
                    break;
            }
        }

        private void DoPrimary(Item item)
        {
            switch (item.Category)
            {
                case ItemCategory.Tool:
                    _player.Equip((ToolItem)item);
                    _toast("Equipped " + item.Name);
                    break;
                case ItemCategory.Resource:
                    _player.Sell((ResourceItem)item);
                    SoundManager.Instance.Play("buy");
                    _toast("Sold " + item.Name + " for $" + item.Value);
                    _selectedItem = -1;
                    break;
                case ItemCategory.Food:
                    _player.Eat((FoodItem)item);
                    SoundManager.Instance.Play("eat");
                    _toast("Yum! +" + ((FoodItem)item).Stamina + " stamina");
                    _selectedItem = -1;
                    break;
            }
        }

        private void DoSecondary(Item item)
        {
            ToolItem tool = item as ToolItem;
            if (tool != null && tool.CanUpgrade)
            {
                _toast(_player.Upgrade(tool));
                return;
            }
            _player.Discard(item);
            _toast("Removed " + item.Name);
            _selectedItem = -1;
        }

        private void UpdateStore()
        {
            IList<Item> stock = _store.Catalogue;
            int slot = SlotUnderMouse(stock.Count, 224);
            if (slot >= 0 && Ui.Clicked()) _selectedStore = slot;

            if (_selectedStore < 0 || _selectedStore >= stock.Count) return;
            Item item = stock[_selectedStore];
            _buy.Label = "BUY $" + item.Value;
            _buy.Enabled = _player.Money >= item.Value;
            if (_buy.WasClicked())
            {
                string result = _store.Buy(_player, item);
                if (result.StartsWith("Bought")) SoundManager.Instance.Play("buy");
                _toast(result);
            }
        }

        private int SlotUnderMouse(int slots, int gridY = GridY)
        {
            for (int i = 0; i < slots; i++)
            {
                double x, y;
                SlotPos(i, gridY, out x, out y);
                if (Ui.MouseIn(x, y, SlotSize, SlotSize)) return i;
            }
            return -1;
        }

        private static void SlotPos(int index, int gridY, out double x, out double y)
        {
            x = GridX + (index % Cols) * (SlotSize + SlotGap);
            y = gridY + (index / Cols) * (SlotSize + SlotGap);
        }

        // ------------------------------------------------------------------ drawing
        public void Draw()
        {
            DrawBoard();
            SplashKit.FillRectangle(Ui.Panel, 0, 150, Width, 450);
            SplashKit.FillRectangle(SplashKit.RGBColor(46, 110, 46), Width - 4, 0, 4, 600);
            SplashKit.DrawBitmap(Assets.Bmp("backpack"), 196, 152);

            string title = Mode == PanelMode.Inventory ? "INVENTORY" : Mode == PanelMode.Store ? "STORE" : "MAP";
            Ui.Text(title, Ui.Ink, Assets.TitleFont, 11, 12, 166);

            if (Mode == PanelMode.Inventory) DrawInventory();
            else if (Mode == PanelMode.Store) DrawStore();
            else DrawMap();

            _mapTab.Draw();
            _storeTab.Draw();
            _invTab.Draw();
            Color active = Ui.Red;
            UiButton tab = Mode == PanelMode.Map ? _mapTab : Mode == PanelMode.Store ? _storeTab : _invTab;
            SplashKit.FillRectangle(active, tab.X, tab.Y + tab.H + 2, tab.W, 3);
        }

        private void DrawBoard()
        {
            SplashKit.DrawBitmap(Assets.Bmp("panel_wood"), 0, 0);
            SplashKit.DrawBitmap(Assets.Bmp("player_right"), 10, 8);
            Ui.Shadowed("LEVEL " + _cfg.Level, Color.White, Assets.TitleFont, 13, 62, 12);

            // Stamina bar
            double frac = (double)_player.Stamina / Player.MaxStamina;
            SplashKit.FillRectangle(SplashKit.RGBColor(70, 45, 25), 62, 34, 166, 14);
            SplashKit.FillRectangle(frac < 0.25 ? Ui.Red : Ui.Green, 64, 36, 162 * frac, 10);
            Ui.Text("STAMINA " + _player.Stamina + (_player.IsResting ? "  resting..." : ""), Color.White,
                    Assets.BodyFont, 18, 64, 47);

            Ui.Shadowed("Time left: " + (int)Math.Ceiling(TimeLeft), Color.White, Assets.BodyFont, 22, 12, 68);
            Ui.Shadowed("Target Diamond: " + _player.Diamonds + "/" + _cfg.TargetDiamonds, Color.White,
                        Assets.BodyFont, 22, 12, 92);
            Ui.Shadowed("Money: $" + _player.Money, Color.White, Assets.BodyFont, 22, 12, 116);

            SplashKit.FillRectangle(Ui.Beige, 194, 98, 40, 40);
            Ui.Frame(Ui.Ink, 194, 98, 40, 40);
            if (_player.Equipped != null)
                SplashKit.DrawBitmap(Assets.Bmp(_player.Equipped.Icon), 198, 102);
        }

        private void DrawSlot(int index, int gridY, Item item, bool selected, bool equipped)
        {
            double x, y;
            SlotPos(index, gridY, out x, out y);
            SplashKit.FillRectangle(Ui.Beige, x, y, SlotSize, SlotSize);
            Ui.Frame(selected ? Ui.Red : SplashKit.RGBColor(200, 190, 170), x, y, SlotSize, SlotSize);
            if (item != null) SplashKit.DrawBitmap(Assets.Bmp(item.Icon), x + 4, y + 4);
            if (equipped)
            {
                SplashKit.FillRectangle(Ui.Yellow, x + SlotSize - 12, y + 2, 10, 10);
                Ui.Text("E", Ui.Ink, Assets.BodyFont, 14, x + SlotSize - 10, y);
            }
        }

        private void DrawDescription(Item item, double y)
        {
            Ui.Text("Name: " + item.Name, Ui.Ink, Assets.BodyFont, 22, 12, y);
            Ui.Text("Value: $" + item.Value, Ui.Ink, Assets.BodyFont, 20, 12, y + 22);
            Ui.Text("Description: " + item.Description, Ui.Ink, Assets.BodyFont, 20, 12, y + 42);
            if (item.Details.Length > 0)
                Ui.Text(item.Details, Ui.Ink, Assets.BodyFont, 20, 12, y + 62);
        }

        private void DrawInventory()
        {
            Inventory inv = _player.Inventory;
            for (int i = 0; i < inv.Capacity; i++)
            {
                Item item = inv.ItemAt(i);
                DrawSlot(i, GridY, item, i == _selectedItem, item != null && item == _player.Equipped);
            }

            Item selected = inv.ItemAt(_selectedItem);
            if (selected != null)
            {
                DrawDescription(selected, 384);
                ConfigureActions(selected);
                _actionA.Draw();
                _actionB.Draw();
            }
            else
            {
                Ui.Text("Click an item to see what", Ui.Grey, Assets.BodyFont, 20, 12, 392);
                Ui.Text("you can do with it.", Ui.Grey, Assets.BodyFont, 20, 12, 412);
            }
            Ui.Text("Slots used: " + inv.Count + "/" + inv.Capacity, Ui.Ink, Assets.BodyFont, 20, 12, 518);
        }

        private void DrawStore()
        {
            // Bunting across the top of the shop
            SplashKit.DrawLine(Ui.Ink, 8, 194, 232, 194);
            Color[] flags = { Ui.Red, Ui.Yellow, Ui.Blue, Ui.Green, Ui.Salmon };
            for (int i = 0; i < 9; i++)
            {
                double fx = 10 + i * 25;
                SplashKit.FillTriangle(flags[i % flags.Length], fx, 194, fx + 20, 194, fx + 10, 214);
            }

            IList<Item> stock = _store.Catalogue;
            for (int i = 0; i < stock.Count; i++)
                DrawSlot(i, 224, stock[i], i == _selectedStore, false);

            if (_selectedStore >= 0 && _selectedStore < stock.Count)
            {
                DrawDescription(stock[_selectedStore], 324);
                _buy.Draw();
            }
            else
            {
                Ui.Text("Click an item to buy it.", Ui.Grey, Assets.BodyFont, 20, 12, 332);
            }
            Ui.Text("You have: $" + _player.Money, Ui.Ink, Assets.BodyFont, 22, 12, 510);
        }

        private void DrawMap()
        {
            SplashKit.DrawBitmap(Assets.Bmp("panel_parchment"), 10, 192);
            int cell = Math.Max(2, 196 / _maze.Width);
            double ox = 10 + (220 - cell * _maze.Width) / 2.0;
            double oy = 200 + (230 - cell * _maze.Height) / 2.0;
            Color tunnel = Color.White, soil = SplashKit.RGBColor(232, 214, 176);

            for (int x = 0; x < _maze.Width; x++)
                for (int y = 0; y < _maze.Height; y++)
                {
                    BlockType t = _maze.Get(x, y).Type;
                    Color c = t == BlockType.Empty ? tunnel : t == BlockType.Dirt ? soil : Ui.Ink;
                    SplashKit.FillRectangle(c, ox + x * cell, oy + y * cell, cell, cell);
                }

            SplashKit.FillRectangle(Ui.Red, ox + _player.Col * cell - 1, oy + _player.Row * cell - 1, cell + 2, cell + 2);
            if (_monster.Visible)
                SplashKit.FillCircle(SplashKit.RGBColor(150, 80, 220), ox + (_monster.X + 0.5) * cell,
                                     oy + (_monster.Y + 0.5) * cell, cell);

            Ui.Text("Red = you", Ui.Ink, Assets.BodyFont, 20, 14, 450);
            Ui.Text("White = dug tunnel", Ui.Ink, Assets.BodyFont, 20, 14, 470);
            if (_monster.Active) Ui.Text("Purple = ghost", Ui.Ink, Assets.BodyFont, 20, 14, 490);
        }
    }
}
