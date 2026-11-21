using SplashKitSDK;

namespace MiningMaze
{
    /// <summary>Shared colours and text helpers for a consistent pixel-art look.</summary>
    public static class Ui
    {
        public static readonly Color Blue = SplashKit.RGBColor(91, 141, 239);
        public static readonly Color BlueDark = SplashKit.RGBColor(52, 92, 180);
        public static readonly Color Yellow = SplashKit.RGBColor(245, 197, 24);
        public static readonly Color YellowDark = SplashKit.RGBColor(196, 150, 10);
        public static readonly Color Salmon = SplashKit.RGBColor(250, 128, 90);
        public static readonly Color SalmonDark = SplashKit.RGBColor(200, 90, 60);
        public static readonly Color Mint = SplashKit.RGBColor(122, 240, 200);
        public static readonly Color Beige = SplashKit.RGBColor(250, 240, 215);
        public static readonly Color Ink = SplashKit.RGBColor(27, 27, 27);
        public static readonly Color Green = SplashKit.RGBColor(52, 168, 83);
        public static readonly Color Red = SplashKit.RGBColor(220, 50, 47);
        public static readonly Color Grey = SplashKit.RGBColor(140, 140, 140);
        public static readonly Color Panel = SplashKit.RGBColor(255, 255, 255);

        public static void Text(string text, Color colour, string font, int size, double x, double y)
        {
            SplashKit.DrawText(text, colour, font, size, x, y);
        }

        public static int Width(string text, string font, int size)
        {
            return SplashKit.TextWidth(text, font, size);
        }

        public static void Centered(string text, Color colour, string font, int size, double centreX, double y)
        {
            Text(text, colour, font, size, centreX - Width(text, font, size) / 2.0, y);
        }

        /// <summary>Text with a dark drop shadow so it reads well on busy backgrounds.</summary>
        public static void Shadowed(string text, Color colour, string font, int size, double x, double y)
        {
            Text(text, Ink, font, size, x + 2, y + 2);
            Text(text, colour, font, size, x, y);
        }

        public static void ShadowedCentered(string text, Color colour, string font, int size, double centreX, double y)
        {
            Shadowed(text, colour, font, size, centreX - Width(text, font, size) / 2.0, y);
        }

        public static bool MouseIn(double x, double y, double w, double h)
        {
            double mx = SplashKit.MouseX(), my = SplashKit.MouseY();
            return mx >= x && mx <= x + w && my >= y && my <= y + h;
        }

        public static bool Clicked()
        {
            return SplashKit.MouseClicked(MouseButton.LeftButton);
        }

        /// <summary>Chunky pixel frame (2 px border) around a rectangle.</summary>
        public static void Frame(Color colour, double x, double y, double w, double h)
        {
            SplashKit.DrawRectangle(colour, x, y, w, h);
            SplashKit.DrawRectangle(colour, x + 1, y + 1, w - 2, h - 2);
        }
    }

    /// <summary>A clickable pixel-style button with a darker "3D" bottom edge.</summary>
    public class UiButton
    {
        public UiButton(string label, double x, double y, double w, double h, Color fill, Color shade,
                        Color textColour, int fontSize = 12)
        {
            Label = label;
            X = x; Y = y; W = w; H = h;
            Fill = fill;
            Shade = shade;
            TextColour = textColour;
            FontSize = fontSize;
            Font = Assets.TitleFont;
        }

        public string Label { get; set; }
        public double X, Y, W, H;
        public Color Fill, Shade, TextColour;
        public int FontSize;
        public string Font;
        public bool Enabled = true;

        public bool Hovered { get { return Enabled && Ui.MouseIn(X, Y, W, H); } }

        public bool WasClicked()
        {
            if (Hovered && Ui.Clicked())
            {
                SoundManager.Instance.Play("click");
                return true;
            }
            return false;
        }

        public void Draw()
        {
            Color fill = Enabled ? Fill : Ui.Grey;
            double lift = Hovered ? -2 : 0;
            SplashKit.FillRectangle(Enabled ? Shade : Ui.Grey, X, Y + 4, W, H);
            SplashKit.FillRectangle(fill, X, Y + lift, W, H);
            int th = SplashKit.TextHeight(Label, Font, FontSize);
            Ui.Centered(Label, TextColour, Font, FontSize, X + W / 2, Y + lift + (H - th) / 2 + 1);
        }
    }
}
