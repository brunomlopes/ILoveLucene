using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ILoveLucene.WindowsInterop
{
    public class KeyboardHandler : IDisposable
    {
        public const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Window _mainWindow;
        private readonly Action _callback;
        private readonly WindowInteropHelper _host;

        public KeyboardHandler(Window mainWindow, Action callback)
        {
            _mainWindow = mainWindow;
            _callback = callback;
            _host = new WindowInteropHelper(_mainWindow);

            SetupHotKey(_host.Handle);
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            if (msg.message == WM_HOTKEY)
            {
                try
                {
                    _callback();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                handled = true;
            }
        }

        private void SetupHotKey(IntPtr handle)
        {
            RegisterHotKey(handle, GetType().GetHashCode(), Modifiers.MOD_WIN, Keys.VK_RETURN);
        }

        public void Dispose()
        {
            UnregisterHotKey(_host.Handle, GetType().GetHashCode());
        }

        public class Modifiers
        {
            public const int MOD_ALT = 0x0001; // Either ALT key must be held down.
            public const int MOD_CONTROL = 0x0002; // Either CTRL key must be held down.

            public const int MOD_NOREPEAT = 0x4000;
                             // Changes the hotkey behavior so that the keyboard auto-repeat does not yield multiple hotkey notifications.

            public const int MOD_SHIFT = 0x0004; // Either SHIFT key must be held down.

            public const int MOD_WIN = 0x0008;
                             // Either WINDOWS key was held down. These keys are labeled with the Windows logo. Keyboard shortcuts that involve the WINDOWS key are reserved for use by the operating system.
        }

        public class Keys
        {
            public const int VK_LBUTTON = 0x01; // Left mouse button
            public const int VK_RBUTTON = 0x02; // Right mouse button
            public const int VK_CANCEL = 0x03; // Control-break processing
            public const int VK_MBUTTON = 0x04; // Middle mouse button on a three-button mouse
            public const int VK_BACK = 0x08; // BACKSPACE key
            public const int VK_TAB = 0x09; // TAB key
            public const int VK_CLEAR = 0x0C; // CLEAR key
            public const int VK_RETURN = 0x0D; // ENTER key
            public const int VK_SHIFT = 0x10; // SHIFT key
            public const int VK_CONTROL = 0x11; // CTRL key
            public const int VK_MENU = 0x12; // ALT key
            public const int VK_PAUSE = 0x13; // PAUSE key
            public const int VK_CAPITAL = 0x14; // CAPS LOCK key
            public const int VK_ESCAPE = 0x1B; // ESC key
            public const int VK_SPACE = 0x20; // SPACEBAR
            public const int VK_PRIOR = 0x21; // PAGE UP key
            public const int VK_NEXT = 0x22; // PAGE DOWN key
            public const int VK_END = 0x23; // END key
            public const int VK_HOME = 0x24; // HOME key
            public const int VK_LEFT = 0x25; // LEFT ARROW key
            public const int VK_UP = 0x26; // UP ARROW key
            public const int VK_RIGHT = 0x27; // RIGHT ARROW key
            public const int VK_DOWN = 0x28; // DOWN ARROW key
            public const int VK_SELECT = 0x29; // SELECT key
            public const int VK_EXECUTE = 0x2B; // EXECUTE key
            public const int VK_SNAPSHOT = 0x2C; // PRINT SCREEN key
            public const int VK_INSERT = 0x2D; // INS key
            public const int VK_DELETE = 0x2E; // DEL key
            public const int VK_HELP = 0x2F; // HELP key
            public const int VK_LWIN = 0x5B; // Left Windows key on a Microsoft Natural Keyboard
            public const int VK_RWIN = 0x5C; // Right Windows key on a Microsoft Natural Keyboard
            public const int VK_APPS = 0x5D; // Applications key on a Microsoft Natural Keyboard
            public const int VK_NUMPAD0 = 0x60; // Numeric keypad 0 key
            public const int VK_NUMPAD1 = 0x61; // Numeric keypad 1 key
            public const int VK_NUMPAD2 = 0x62; // Numeric keypad 2 key
            public const int VK_NUMPAD3 = 0x63; // Numeric keypad 3 key
            public const int VK_NUMPAD4 = 0x64; // Numeric keypad 4 key
            public const int VK_NUMPAD5 = 0x65; // Numeric keypad 5 key
            public const int VK_NUMPAD6 = 0x66; // Numeric keypad 6 key
            public const int VK_NUMPAD7 = 0x67; // Numeric keypad 7 key
            public const int VK_NUMPAD8 = 0x68; // Numeric keypad 8 key
            public const int VK_NUMPAD9 = 0x69; // Numeric keypad 9 key
            public const int VK_MULTIPLY = 0x6A; // Multiply key
            public const int VK_ADD = 0x6B; // Add key
            public const int VK_SEPARATOR = 0x6C; // Separator key
            public const int VK_SUBTRACT = 0x6D; // Subtract key
            public const int VK_DECIMAL = 0x6E; // Decimal key
            public const int VK_DIVIDE = 0x6F; // Divide key
            public const int VK_F1 = 0x70; // F1 key
            public const int VK_F2 = 0x71; // F2 key
            public const int VK_F3 = 0x72; // F3 key
            public const int VK_F4 = 0x73; // F4 key
            public const int VK_F5 = 0x74; // F5 key
            public const int VK_F6 = 0x75; // F6 key
            public const int VK_F7 = 0x76; // F7 key
            public const int VK_F8 = 0x77; // F8 key
            public const int VK_F9 = 0x78; // F9 key
            public const int VK_F10 = 0x79; // F10 key
            public const int VK_F11 = 0x7A; // F11 key
            public const int VK_F12 = 0x7B; // F12 key
            public const int VK_NUMLOCK = 0x90; // NUM LOCK key
            public const int VK_SCROLL = 0x91; // SCROLL LOCK key
            public const int VK_LSHIFT = 0xA0; // Left SHIFT
            public const int VK_RSHIFT = 0xA1; // Right SHIFT
            public const int VK_LCONTROL = 0xA2; // Left CTRL
            public const int VK_RCONTROL = 0xA3; // Right CTRL
            public const int VK_LMENU = 0xA4; // Left ALT
            public const int VK_RMENU = 0xA5; // Right ALT
        }
    }
}