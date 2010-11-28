using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Core.Abstractions;
using IronPython.Hosting;
using System.Linq;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using IronPython.Runtime.Types;

namespace Plugins.IronPython
{
    [Export(typeof(IBackgroundStartTask))]
    public class IronPythonCommandsMefExport : IBackgroundStartTask
    {
        private readonly CompositionContainer _mefContainer;
        private ScriptEngine _engine;

        [ImportingConstructor]
        public IronPythonCommandsMefExport(CompositionContainer mefContainer)
        {
            _mefContainer = mefContainer;
        }

        private void ExportCommandsFromFilesIntoMef(IEnumerable<FileInfo> pythonFiles)
        {
            foreach (var pythonFile in pythonFiles)
            {
                var _fileFullName = pythonFile.FullName;

                var script = _engine.CreateScriptSourceFromFile(_fileFullName);

                CompiledCode code;
                try
                {
                    code = script.Compile();
                }
                catch (SyntaxErrorException e)
                {
                    throw new SyntaxErrorExceptionPrettyWrapper(string.Format("Error compiling '{0}", _fileFullName), e);
                }

                ScriptScope scope = _engine.CreateScope();
                scope.InjectType<IItem>()
                    .InjectType<ICommandWithArguments>()
                    .InjectType<ICommandWithAutoCompletedArguments>();
                scope.SetVariable("clr", _engine.GetClrModule());

                try
                {
                    code.Execute(scope);
                }
                catch (UnboundNameException e)
                {
                    throw new PythonException(string.Format("Error executing '{0}'", _fileFullName), e);
                }

                var pluginClasses = scope.GetItems()
                    .Where(kvp => kvp.Value is PythonType)
                    .Where(kvp => typeof (IItem).IsAssignableFrom(((PythonType) kvp.Value).__clrtype__()))
                    .Where(kvp => !kvp.Key.StartsWith("ICommand"));
                var instances =
                    pluginClasses.Select<KeyValuePair<string, object>, object>(nameAndClass => _engine.Operations.Invoke(nameAndClass.Value, new object[] {}))
                        .Cast<IItem>();

                var batch = new CompositionBatch();
                foreach (var instance in instances)
                {
                    batch.AddExportedValue(instance);
                }
                _mefContainer.Compose(batch);
            }
        }

        private DirectoryInfo GetIronPythonPluginsDirectory()
        {
            var location = new FileInfo(Assembly.GetExecutingAssembly().Location);
            var directory = location.Directory;
            while (directory != null
                   && !directory.EnumerateDirectories().Any(d => d.Name.ToLowerInvariant() == "ironpythoncommands")
                   && directory.Root != directory)
            {
                directory = directory.Parent;
            }
            if (directory == null || directory.Root == directory)
            {
                throw new InvalidOperationException(string.Format("No 'IronPythonCommands' directory found in tree of {0}", location));
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

        public bool Executed { get; private set; }

        public void Execute()
        {
            try
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
                _engine = Python.CreateEngine();

                DirectoryInfo directory = GetIronPythonPluginsDirectory();

                var pythonFiles =
                    directory.GetFiles()
                        .Where(f => f.Extension.ToLowerInvariant() == ".ipy");

                ExportCommandsFromFilesIntoMef(pythonFiles);
            }
            finally
            {
                Executed = true;
            }
            
        }
    }
}