using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    public class IronPythonFile
    {
        private readonly System.IO.FileInfo _pythonFile;
        private readonly ScriptEngine _engine;
        private readonly CompositionContainer _mefContainer;
        private readonly ExtractTypesFromScript _extractTypesFromScript;
        private IEnumerable<IronPythonComposablePart> _currentParts;

        public IronPythonFile(System.IO.FileInfo pythonFile, ScriptEngine engine, CompositionContainer mefContainer, ExtractTypesFromScript extractTypesFromScript)
        {
            _pythonFile = pythonFile;
            _engine = engine;
            _mefContainer = mefContainer;
            _extractTypesFromScript = extractTypesFromScript;
            _currentParts = new List<IronPythonComposablePart>();
        }

        public void Compose()
        {
            var script = _engine.CreateScriptSourceFromFile(_pythonFile.FullName);
            IEnumerable<IronPythonComposablePart> previousParts = _currentParts;
            IEnumerable<IronPythonComposablePart> newParts = new List<IronPythonComposablePart>();
            try
            {
                newParts = _extractTypesFromScript.GetPartsFromScript(script);
            }
            catch (SyntaxErrorException e)
            {
                throw new IronPythonCommandsMefExport.SyntaxErrorExceptionPrettyWrapper(string.Format("Error compiling '{0}", _pythonFile.FullName), e);
            }
            catch (UnboundNameException e)
            {
                throw new IronPythonCommandsMefExport.PythonException(string.Format("Error executing '{0}'", _pythonFile.FullName), e);
            }
            _currentParts = newParts;
            var batch = new CompositionBatch(_currentParts, previousParts);
            _mefContainer.Compose(batch);
        }

        public void Decompose()
        {
            var batch = new CompositionBatch(new ComposablePart[] {}, _currentParts);
            _mefContainer.Compose(batch);
            _currentParts = new List<IronPythonComposablePart>();
        }
    }
}