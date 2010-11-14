using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ILoveLucene.WindowsInterop
{
    public class FocusHandler
    {
        private readonly Window _window;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        WindowInteropHelper _host;

        public FocusHandler(Window window)
        {
            _window = window;
            _host = new WindowInteropHelper(window);

        }
        public void SetForegroundWindow()
        {
            SetForegroundWindow(_host.Handle);
        }
    }
}