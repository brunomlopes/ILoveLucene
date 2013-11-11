using System;
using System.Runtime.InteropServices;
using System;
using System.Runtime.InteropServices;
using Core.Abstractions;

namespace Plugins.Processes
{
    internal class ForegroundTracker : IDisposable
    {
        private readonly Action<int> _changedProcess;
        // Delegate and imports from pinvoke.net:

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
            hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
            uint idThread, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);


        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        // Constants from winuser.h
        private const uint EVENT_OBJECT_NAMECHANGE = 0x0000800c;
        private const uint WINEVENT_OUTOFCONTEXT = 0;

        private WinEventDelegate procDelegate;
        private static IntPtr _winEventHook;

        public ForegroundTracker(Action<int> changedProcess)
        {
            _changedProcess = changedProcess;
            procDelegate = WinEventProc;
        }

        public ForegroundTracker Start()
        {
            // Listen for foreground changes across all processes/threads on current desktop...
            _winEventHook = SetWinEventHook(EVENT_OBJECT_NAMECHANGE, EVENT_OBJECT_NAMECHANGE, IntPtr.Zero,
                procDelegate, 0, 0, WINEVENT_OUTOFCONTEXT);
            return this;
        }


        private void WinEventProc(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (idChild != 0 || idObject != 0) return;
            {
                uint processId;
                GetWindowThreadProcessId(hwnd, out processId);
                if (Marshal.GetLastWin32Error() != 0) return;

                _changedProcess((int)processId);
            };
        }

        public void Dispose()
        {
            UnhookWinEvent(_winEventHook);
        }
    }

}