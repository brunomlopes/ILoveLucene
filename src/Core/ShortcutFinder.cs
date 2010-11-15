using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Abstractions;
using Core.Extensions;

namespace Core
{
    public class ShortcutFinder : IItemSource<FileInfo>
    {
        private HashSet<FileInfo> _shortcutPaths;
        public HashSet<FileInfo> ShortcutPaths
        {
            get
            {
                HashSet<FileInfo> shortcuts;
                lock (_shortcutPathsLock)
                {
                    shortcuts = new HashSet<FileInfo>(_shortcutPaths);
                }
                return shortcuts;
            }
        }
        private static object _shortcutPathsLock = new object();
        public static string[] _extensions = new[] { ".exe", ".bat", ".ps1", ".ipy", ".lnk", ".appref-ms" };
        private List<string> _dirs;

       
        public ShortcutFinder()
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
                            @"%HOME%\utils"
                        }.Select(Environment.ExpandEnvironmentVariables).ToList();
           
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

        public Task<IEnumerable<FileInfo>> GetItems()
        {
            return Task.Factory.StartNew(() =>
                                             {
                                                 _dirs.ForEach(ScanDirectoryForShortcuts);
                                                 return _shortcutPaths.AsEnumerable();
                                             })
                .GuardForException(e => Debug.WriteLine("Exception on task:" + e));
        }
    }
}