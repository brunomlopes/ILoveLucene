using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Shell32;

namespace Plugins.WindowsEnvironment
{
    public class WindowsExplorer
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetWindow(IntPtr hWnd, GetWindow_Cmd uCmd);

        private enum GetWindow_Cmd : uint
        {
            GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6
        }

        /// <summary>
        /// Returns the selected file from windows explorer.
        /// If the selected item is a directory, or if there's no explorer window, then returns null
        /// </summary>
        /// <returns>Selected file from windows explorer, or null if none found</returns>
        public static FileInfo GetTopSelectedFileFromExplorer()
        {
            string path = GetTopSelectedPathFromWindowsExplorer();
            if (path != null && File.Exists(path))
                return new FileInfo(path);
            return null;
        }

        /// <summary>
        /// Returns the selected directory from windows explorer.
        /// If the selected item is a file, returns null
        /// If there's no explorer window, returns null
        /// </summary>
        /// <returns>Selected directory from windows explorer, or null if none found</returns>
        public static DirectoryInfo GetTopSelectedDirectoryFromExplorer()
        {
            string path = GetTopSelectedPathFromWindowsExplorer();
            if (path != null)
            {
                if (Directory.Exists(path))
                    return new DirectoryInfo(path);
            }
            return null;
        }

        private static string GetTopSelectedPathFromWindowsExplorer()
        {
            IShellDispatch5 shell = new Shell();

            var windows = shell.Windows();

            var explorerHandles = new Dictionary<IntPtr, dynamic>();

            foreach (var window in windows)
            {
                explorerHandles.Add((IntPtr) window.HWND, window);
            }

            IntPtr i = GetForegroundWindow();

            while (i != IntPtr.Zero && !explorerHandles.ContainsKey(i))
            {
                i = GetWindow(i, GetWindow_Cmd.GW_HWNDNEXT);
            }

            string path = null;
            if (i != IntPtr.Zero)
            {
                var window = explorerHandles[i];
                path = ((IShellFolderViewDual2) window.Document).FocusedItem.Path;
            }
            return path;
        }
    }
}