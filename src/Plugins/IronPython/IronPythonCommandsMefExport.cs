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
        private ScriptEngine _engine;
        private Dictionary<string, IronPythonFile> _files = new Dictionary<string, IronPythonFile>();
        private FileSystemWatcher _watcher;
            
        [Import]
        public CoreConfiguration CoreConfiguration { get; set; }

        [ImportingConstructor]
        public IronPythonCommandsMefExport(CompositionContainer mefContainer)
        {
            _mefContainer = mefContainer;
        }

        public bool Executed { get; private set; }

        public void Execute()
        {
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _engine = Python.CreateEngine();

                DirectoryInfo directory = GetIronPythonPluginsDirectory();

                var pythonFiles =
                    directory.GetFiles().Where(f => f.Extension.ToLowerInvariant() == ".ipy");

                ExportCommandsFromFilesIntoMef(pythonFiles);
                _watcher = new FileSystemWatcher(directory.FullName, "*.ipy");
                _watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                        | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            }
            finally
            {
                Executed = true;
            }

        }

        private void ExportCommandsFromFilesIntoMef(IEnumerable<FileInfo> pythonFiles)
        {
            foreach (var pythonFile in pythonFiles)
            {
                var file = new IronPythonFile(pythonFile, _engine, _mefContainer, new ExtractTypesFromScript(_engine));
                _files[pythonFile.FullName] = file;
                file.Compose();
            }
        }


        private DirectoryInfo GetIronPythonPluginsDirectory()
        {
            var directory = new DirectoryInfo(CoreConfiguration.PluginsDirectory);;
            while (directory != null
                   && !directory.EnumerateDirectories().Any(d => d.Name.ToLowerInvariant() == "ironpythoncommands")
                   && directory.Root != directory)
            {
                directory = directory.Parent;
            }
            if (directory == null || directory.Root == directory)
            {
                throw new InvalidOperationException(string.Format("No 'IronPythonCommands' directory found in tree of {0}", directory));
            }
            return directory.EnumerateDirectories().Single(d => d.Name.ToLowerInvariant() == "ironpythoncommands");
        }

        public class PythonException : Exception
        {
            public PythonException(string message, Exception innerException)
                : base(message, innerException)
            {
            }
        }

        public class SyntaxErrorExceptionPrettyWrapper : PythonException
        {
            private readonly SyntaxErrorException _innerException;
            public SyntaxErrorExceptionPrettyWrapper(string message, SyntaxErrorException innerException)
                : base(message, innerException)
            {
                _innerException = innerException;
            }

            public override string Message
            {
                get
                {
                    return string.Format("{4}\nLine {0}\n{1}\n{2}^---{3}", _innerException.Line,
                        _innerException.GetCodeLine(),
                                         string.Join("", Enumerable.Repeat(" ", _innerException.Column).ToArray()),
                                         _innerException.Message,
                                         base.Message);
                }
            }
        }
    }
}