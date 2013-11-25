using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Reflection;
using System.Threading;
using Core;
using Core.API;
using Core.Abstractions;
using IronPython.Hosting;
using System.Linq;
using IronPython.Runtime;
using IronPythonMef;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Plugins.Commands;

namespace Plugins.IronPython
{
    public class IronPythonCommandsMefExport : IStartupTask
    {
        private const string IronPythonScriptExtension = ".py";
        private readonly CompositionContainer _mefContainer;
        private readonly ILog _log;
        private ScriptEngine _engine;
        private Dictionary<string, IronPythonFile> _files = new Dictionary<string, IronPythonFile>();
        private Dictionary<string,FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public EventHandler RefreshedFiles;

        public static IList<Type> InitialScopeTypes = new[]
            {
                typeof (IItem), typeof (IConverter<>), typeof (BaseActOnTypedItem<>),
                typeof (BaseActOnTypedItemAndReturnTypedItem<,>),
                typeof (IItemSource),
                typeof (BaseItemSource),
                typeof (IConverter),
                typeof (ICommand),
                typeof (IRequest),
                typeof (IActOnItem),
                typeof (IActOnItemWithArguments),
                typeof (IActOnTypedItemWithArguments<>),
                typeof (IActOnTypedItemWithAutoCompletedArguments<>),
                typeof (ICanActOnTypedItem<>),
                typeof (IActOnTypedItemAndReturnItem<>),
                typeof (IActOnTypedItemAndReturnTypedItem<,>),
                typeof (IActOnTypedItemWithArgumentsAndReturnTypedItem<,>),
                typeof (ArgumentAutoCompletionResult),
                typeof (BasePythonItemSource),
                typeof (IronPythonImportDefinition)
            };


        [ImportConfiguration]
        public CoreConfiguration CoreConfiguration { get; set; }

        [ImportConfiguration]
        public Configuration Configuration { get; set; }

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
                        directory.GetFiles().Where(f => f.Extension.ToLowerInvariant() == IronPythonScriptExtension);

                    foreach (var pythonFile in pythonFiles)
                    {
                        AddIronPythonFile(pythonFile);
                    }

                    var watcher = new FileSystemWatcher(directory.FullName, "*"+IronPythonScriptExtension);
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
                _log.Info("Removing and decomposing file {0}", e.OldFullPath);
                _files.Remove(e.OldFullPath);
                _files[e.OldFullPath].Decompose();
            }
            else
            {
                _log.Warn("File {0} not found in the ironpython cache but got notified it was renamed");
            }
            if (_files.ContainsKey(e.FullPath))
            {
                _log.Info("Removing and decomposing file {0}", e.FullPath);
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
            _log.Info("Reloading file {0}", e.FullPath);
            try
            {
                _files[e.FullPath].Compose();

            }
            catch (IronPythonFile.PythonException ex)
            {
                _log.Error(ex, "Error executing file {0}:{1}", e.FullPath, ex.Message);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error executing file {0}:{1}", e.FullPath, ex.Message);
            }
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
            _log.Info("Removing and decomposing file {0}", e.FullPath);
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
            _log.Debug("Loading ironpython file {0}", pythonFile.FullName);
            var file = new IronPythonFile(pythonFile, _engine, _mefContainer, new ExtractTypesFromScript(_engine));
            _files[pythonFile.FullName] = file;
            try
            {
                file.Compose();
            }
            catch (IronPythonFile.PythonException e)
            {
                _log.Error(e, "Error executing file {0}:{1}", pythonFile.FullName, e.Message);
            }
            catch (Exception e)
            {
                _log.Error(e, "Error executing file {0}:{1}", pythonFile.FullName, e.Message);
            }
        }


        private IEnumerable<DirectoryInfo> GetIronPythonPluginsDirectories()
        {
            IEnumerable<DirectoryInfo> directoryInfos = CoreConfiguration
                .ExpandPaths(Configuration.ScriptDirectories)
                .Select(p => Path.GetFullPath(p))
                .Distinct()
                .Select(p => new DirectoryInfo(p));
            foreach (var directory in directoryInfos)
            {
                if(!directory.Exists)
                {
                    _log.Warn("Directory {0} doesn't exist", directory.FullName);
                    continue;
                }
                yield return directory;
            }
        }
    }
}