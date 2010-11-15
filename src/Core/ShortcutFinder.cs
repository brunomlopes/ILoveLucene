using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Core.Extensions;

namespace Core
{
    public class ShortcutFinder
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

        public ShortcutFinder()
        {
            _shortcutPaths = new HashSet<FileInfo>();
            var dirs = new[]
                           {
                               @"%ALLUSERSPROFILE%\Microsoft\Windows\Start Menu",
                               @"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Start Menu",
                               @"%USERPROFILE%\AppData\Roaming\Microsoft\Windows\Recent",
                               @"%APPDATA%\Microsoft\Internet Explorer\Quick Launch",
                               @"C:\Windows\System32",
                               @"%USERPROFILE%\Favorites",
                               @"%HOME%\utils"
                           }.Select(Environment.ExpandEnvironmentVariables).ToList();
            var extensions = new[] {"exe", "bat", "ps1", "ipy", "lnk", "appref-ms"}.Select(x => "."+x).ToList();

            Task.Factory.StartNew(() => dirs.AsParallel()
                                            .ForAll(d => ScanDirectoryForShortcuts(d, extensions)))
                .GuardForException(e => Debug.WriteLine("Exception on task:" + e));
        }

        private void ScanDirectoryForShortcuts(string s, List<string> extensions)
        {
            var currentDir = new DirectoryInfo(s);

            Task.Factory.StartNew(() =>
                                      {
                                          try
                                          {
                                              var fileInfos = currentDir.GetFiles("*.*", SearchOption.TopDirectoryOnly)
                                                  .Where(f => extensions.Contains(f.Extension)).ToList();
                                              Debug.WriteLine("Found {0} files in {1}", fileInfos.Count(), currentDir);
                                              lock (_shortcutPathsLock)
                                              {
                                                  _shortcutPaths.UnionWith(fileInfos);
                                              }
                                          }
                                          catch (UnauthorizedAccessException e)
                                          {
                                              // Oh well, no biggie I guess
                                          }
                                      })
                .GuardForException(e => Debug.WriteLine("Exception on task:" + e));
           
            Task.Factory.StartNew(() =>
                                      {
                                          DirectoryInfo[] directoryInfos;
                                          try
                                          {
                                              directoryInfos = currentDir.GetDirectories();
                                          }
                                          catch(UnauthorizedAccessException e)
                                          {
                                              return;
                                          }
                                          directoryInfos
                                              .AsParallel()
                                              .ForAll(d => ScanDirectoryForShortcuts(d.FullName, extensions));
                                      })
                .GuardForException(e => Debug.WriteLine("Exception on task:" + e));
        }
    }
}