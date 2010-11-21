using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Extensions;

namespace Plugins.Shortcuts
{
    [Export(typeof (ShortcutSource))]
    [Export(typeof (IItemSource))]
    public class ShortcutSource : IItemSource
    {
        private HashSet<FileInfo> _shortcutPaths;
        private static readonly object _shortcutPathsLock = new object();
        public static string[] _extensions = new[] {".exe", ".bat", ".ps1", ".ipy", ".lnk", ".appref-ms"};
        private readonly List<string> _dirs;

        public ShortcutSource()
        {
            _shortcutPaths = new HashSet<FileInfo>();
            _dirs = new[]
                        {
                            @"%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu",
                            @"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu",
                            @"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Recent",
                            @"%APPDATA%\Microsoft\Internet Explorer\Quick Launch",
                            @"C:\Windows\System32",
                            @"%USERPROFILE%\Favorites",
                            @"%HOME%\utils",
                            @"E:\temp"
                        }.Select(Environment.ExpandEnvironmentVariables).ToList();
            NeedsReindexing = true;
        }

        private void ScanDirectoryForShortcuts(string s)
        {
            var currentDir = new DirectoryInfo(s);

            try
            {
                var fileInfos = currentDir
                    .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => _extensions.Contains(f.Extension)).ToList();
                Debug.WriteLine("Found {0} files in {1}", fileInfos.Count(), currentDir);
                lock (_shortcutPathsLock)
                {
                    _shortcutPaths.UnionWith(fileInfos);
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Oh well, no biggie I guess
            }

            List<DirectoryInfo> directoryInfos;
            try
            {
                directoryInfos = currentDir.GetDirectories().ToList();
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            directoryInfos.ForEach(d => ScanDirectoryForShortcuts(d.FullName));
        }

        public bool NeedsReindexing { get; set; }

        public Task<IEnumerable<object>> GetItems()
        {
            NeedsReindexing = false;
            return Task.Factory.StartNew(() =>
                                             {
                                                 _shortcutPaths = new HashSet<FileInfo>();
                                                 _dirs.ForEach(ScanDirectoryForShortcuts);
                                                 return _shortcutPaths.Cast<object>();
                                             })
                .GuardForException(e => Debug.WriteLine("Exception on task:" + e));
        }
    }
}