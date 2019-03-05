using System;
using System.Collections;
using System.Windows.Interop;
using ILoveLucene.Views;

namespace ILoveLucene.WindowsInterop
{
    class EnvironmentVarListener : IDisposable
    {
        private const int WM_WININICHANGE = 0x001A;
        private const int WM_SETTINGCHANGE = WM_WININICHANGE;
        private MainWindowView mainWindowView;

        public EnvironmentVarListener(MainWindowView mainWindowView)
        {
            this.mainWindowView = mainWindowView;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_WININICHANGE:
                    var changedPart = System.Runtime.InteropServices.Marshal.PtrToStringUni(lParam);
                    if (changedPart != "Environment") return IntPtr.Zero;
                    handled = true;
                    RefreshVariables();
                    break;
            }

            return IntPtr.Zero;
        }

        private static void RefreshVariables()
        {
            var allVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);
            foreach(DictionaryEntry var in allVariables)
            {
                Environment.SetEnvironmentVariable(var.Key.ToString(), var.Value.ToString(), EnvironmentVariableTarget.Process);
            }
        }

        internal void OnLoaded()
        {
            HwndSource.FromHwnd(new WindowInteropHelper(mainWindowView).Handle).AddHook(WndProc);
        }

        public void Dispose()
        {
            // TODO: unhook 
        }
    }
}
