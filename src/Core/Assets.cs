using System;
using System.IO;
using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>Loads every bitmap and font once at start-up and hands them out by name.</summary>
    public static class Assets
    {
        public const string TitleFont = "pixel";   // Press Start 2P
        public const string BodyFont = "body";     // VT323

        private static readonly string[] Bitmaps =
        {
            "bg_menu", "panel_wood", "panel_parchment", "backpack",
            "player_right", "player_left", "player_big", "ghost", "ghost_big",
            "tile_dirt", "tile_wall", "tile_coal", "tile_iron", "tile_gold", "tile_diamond",
            "item_coal", "item_iron", "item_gold", "item_diamond", "item_bomb",
            "item_burger", "item_pizza", "item_soda",
            "item_pickaxe_iron", "item_pickaxe_gold", "item_pickaxe_diamond"
        };

        public static string ResourceDir { get; private set; }

        public static void LoadAll()
        {
            ResourceDir = FindResourceDir();
            foreach (string name in Bitmaps)
                SplashKit.LoadBitmap(name, Path.Combine(ResourceDir, "images", name + ".png"));
            SplashKit.LoadFont(TitleFont, Path.Combine(ResourceDir, "fonts", "PressStart2P.ttf"));
            SplashKit.LoadFont(BodyFont, Path.Combine(ResourceDir, "fonts", "VT323.ttf"));
        }

        public static Bitmap Bmp(string name)
        {
            return SplashKit.BitmapNamed(name);
        }

        /// <summary>Works both for "dotnet run" (project folder) and for the published exe.</summary>
        private static string FindResourceDir()
        {
            string[] roots = { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
            foreach (string root in roots)
            {
                string dir = Path.Combine(root, "Resources");
                if (Directory.Exists(Path.Combine(dir, "images"))) return dir;
            }
            return Path.Combine(Directory.GetCurrentDirectory(), "Resources");
        }
    }
}
