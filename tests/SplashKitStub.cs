// Compile-only stand-in for the SplashKit C# API (signatures copied from splashkit.io/api).
// Used to type-check the game with Mono where the real SplashKit library is not installed.
// Not part of the real build (the .csproj excludes tests/**).
namespace SplashKitSDK
{
    public struct Color
    {
        public static Color Black { get { return new Color(); } }
        public static Color White { get { return new Color(); } }
    }
    public class Bitmap { }
    public class Font { }
    public class Window { public Window(string caption, int width, int height) { } }
    public class SoundEffect { public SoundEffect(string name, string filename) { } }
    public class Music { public Music(string name, string filename) { } }
    public enum KeyCode { AKey, DKey, SKey, WKey, UpKey, DownKey, LeftKey, RightKey, SpaceKey, EscapeKey, ReturnKey, F12Key }
    public enum MouseButton { NoButton, LeftButton, MiddleButton, RightButton }

    public static class SplashKit
    {
        public static Bitmap LoadBitmap(string name, string filename) { return null; }
        public static Bitmap BitmapNamed(string name) { return null; }
        public static Font LoadFont(string name, string filename) { return null; }
        public static void DrawBitmap(Bitmap bmp, double x, double y) { }
        public static void DrawText(string text, Color clr, string fnt, int fontSize, double x, double y) { }
        public static int TextWidth(string text, string fnt, int fontSize) { return 0; }
        public static int TextHeight(string text, string fnt, int fontSize) { return 0; }
        public static void FillRectangle(Color clr, double x, double y, double width, double height) { }
        public static void DrawRectangle(Color clr, double x, double y, double width, double height) { }
        public static void FillTriangle(Color clr, double x1, double y1, double x2, double y2, double x3, double y3) { }
        public static void FillCircle(Color clr, double x, double y, double radius) { }
        public static void DrawLine(Color clr, double x1, double y1, double x2, double y2) { }
        public static void ClearScreen(Color clr) { }
        public static void RefreshScreen(uint targetFps) { }
        public static Color RGBColor(int red, int green, int blue) { return new Color(); }
        public static Color RGBColor(double red, double green, double blue) { return new Color(); }
        public static Color RGBAColor(int red, int green, int blue, int alpha) { return new Color(); }
        public static Color RGBAColor(double red, double green, double blue, double alpha) { return new Color(); }
        public static void ProcessEvents() { }
        public static bool QuitRequested() { return false; }
        public static bool WindowCloseRequested(Window wind) { return false; }
        public static void CloseAllWindows() { }
        public static bool KeyDown(KeyCode key) { return false; }
        public static bool KeyTyped(KeyCode key) { return false; }
        public static bool MouseClicked(MouseButton button) { return false; }
        public static float MouseX() { return 0; }
        public static float MouseY() { return 0; }
        public static void TakeScreenshot(Window wind, string basename) { }
        public static void PlaySoundEffect(SoundEffect effect, double volume) { }
        public static void PlayMusic(Music data, int times, double volume) { }
        public static void StopMusic() { }
        public static bool MusicPlaying() { return false; }
    }
}
