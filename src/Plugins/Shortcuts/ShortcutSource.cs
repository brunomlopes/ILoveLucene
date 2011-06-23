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
    public class ShortcutSource : BaseItemSource
    {
        private HashSet<FileInfo> _shortcutPaths;

        [Import]
        public ILog Log { get; set; }

        [ImportConfiguration]
        public Configuration Conf { get; set; }

        public ShortcutSource()
        {
            _shortcutPaths = new HashSet<FileInfo>();
        }

        private void ScanDirectoryForShortcuts(string s)
        {
            var currentDir = new DirectoryInfo(s);
            if(!currentDir.Exists)
            {
                Log.Info("Directory '{0}' doesn't exist", s);
                return;
            }

            try
            {
                var fileInfos = currentDir
                    .GetFiles("*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => Conf.Extensions.Contains(f.Extension)).ToList();
                Log.Debug("Found {0} files in {1}", fileInfos.Count(), currentDir);
                _shortcutPaths.UnionWith(fileInfos);
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

        public override Task<IEnumerable<object>> GetItems()
        {
            return Task.Factory.StartNew(() =>
                                             {
                                                 _shortcutPaths = new HashSet<FileInfo>();
                                                 Conf.Directories
                                                     .Select(Environment.ExpandEnvironmentVariables).ToList()
                                                     .ForEach(ScanDirectoryForShortcuts);
                                                 return _shortcutPaths.Cast<object>();
                                             })
                .GuardForException(e => Log.Debug("Exception on task:" + e));
        }
    }
}