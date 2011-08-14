using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Reflection;
using System.Threading;
using Core;
using Core.Abstractions;
using IronPython.Hosting;
using System.Linq;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    //[Export(typeof(IStartupTask))]
    public class IronPythonCommandsMefExport : IStartupTask
    {
        private readonly CompositionContainer _mefContainer;
        private readonly ILog _log;
        private ScriptEngine _engine;
        private Dictionary<string, IronPythonFile> _files = new Dictionary<string, IronPythonFile>();
        private Dictionary<string,FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public EventHandler RefreshedFiles;

        [ImportConfiguration]
        public CoreConfiguration CoreConfiguration { get; set; }

        [ImportConfiguration]
        public Configuration Configuration { get; set; }

        [ImportingConstructor]
        public IronPythonCommandsMefExport(CompositionContainer mefContainer, ILog log)
        {
            _mefContainer = mefContainer;
            _log = log;
            RefreshedFiles += (e, s) => { };
        }

        public bool Executed { get; private set; }

        public void Execute()
        {
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _engine = Python.CreateEngine();

                foreach (var directory in GetIronPythonPluginsDirectories())
                {
                    var pythonFiles =
                        directory.GetFiles().Where(f => f.Extension.ToLowerInvariant() == ".ipy");

                    foreach (var pythonFile in pythonFiles)
                    {
                        AddIronPythonFile(pythonFile);
                    }

                    var watcher = new FileSystemWatcher(directory.FullName, "*.ipy");
                    watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    watcher.Created += new FileSystemEventHandler(_watcher_Created);
                    watcher.Deleted += new FileSystemEventHandler(_watcher_Deleted);
                    watcher.Changed += new FileSystemEventHandler(_watcher_Changed);
                    watcher.Renamed += new RenamedEventHandler(_watcher_Renamed);
                    watcher.InternalBufferSize = pythonFiles.Count() + 10; // TODO: make this a bit smaller, or cleverer..
                    _watchers[directory.FullName] = watcher;
                }
                foreach (var watcher in _watchers.Values)
                {
                    watcher.EnableRaisingEvents = true;
                }
                
            }
            finally
            {
                Executed = true;
            }

        }

        void _watcher_Renamed(object sender, RenamedEventArgs e)
        {
            if (_files.ContainsKey(e.OldFullPath))
            {
                _files[e.OldFullPath].Decompose();
                _files.Remove(e.OldFullPath);
            }
            else
            {
                _log.Warn("File {0} not found in the ironpython cache but got notified it was renamed");
            }
            if (_files.ContainsKey(e.FullPath))
            {
                _files[e.FullPath].Decompose();
                _files.Remove(e.FullPath);
            }
            AddIronPythonFile(new FileInfo(e.FullPath));
            RefreshedFiles(this, new EventArgs());
        }

        void _watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if(!_files.ContainsKey(e.FullPath))
            {
                _log.Warn("File {0} not found in the ironpython cache but got notified it changed");
                return;
            }
            _files[e.FullPath].Compose();
            RefreshedFiles(this, new EventArgs());

        }

        void _watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            if(!_files.ContainsKey(e.FullPath))
            {
                _log.Warn("File {0} not found in the ironpython cache but got notified it was deleted");
                return;
            }
            var f = _files[e.FullPath];
            _files.Remove(e.FullPath);
            f.Decompose();
            RefreshedFiles(this, new EventArgs());

        }

        void _watcher_Created(object sender, FileSystemEventArgs e)
        {
            AddIronPythonFile(new FileInfo(e.FullPath));
            RefreshedFiles(this, new EventArgs());
        }

        private void AddIronPythonFile(FileInfo pythonFile)
        {
            var file = new IronPythonFile(pythonFile, _engine, _mefContainer, new ExtractTypesFromScript(_engine));
            _files[pythonFile.FullName] = file;
            file.Compose();
        }


        private IEnumerable<DirectoryInfo> GetIronPythonPluginsDirectories()
        {
            foreach (var directoryPath in CoreConfiguration.ExpandPaths(Configuration.ScriptDirectories))
            {
                var directory = new DirectoryInfo(directoryPath);
                if(!directory.Exists)
                {
                    _log.Warn("Directory {0} doesn't exist", directoryPath);
                    continue;
                }
                yield return directory;
            }
        }
    }
}