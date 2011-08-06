using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using IronPython.Hosting;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    public class ExtractTypesFromScript
    {
        private readonly ScriptEngine _engine;

        public ExtractTypesFromScript(ScriptEngine engine)
        {
            _engine = engine;
        }

        public IEnumerable<IronPythonComposablePart> GetPartsFromScript(ScriptSource script)
        {
            return GetParts(GetTypesFromScript(script));
        }

        public IEnumerable<IronPythonTypeWrapper> GetTypesFromScript(ScriptSource script)
        {
            CompiledCode code = script.Compile();
            var types = new[]
                            {
                                typeof (IItem), typeof (IConverter<>), typeof (BaseActOnTypedItem<>),
                                typeof (BaseActOnTypedItemAndReturnTypedItem<,>), typeof (IItem),
                                typeof (IItemSource),
                                typeof (BaseItemSource),
                                typeof (IConverter),
                                typeof (IActOnItem),
                                typeof (IActOnItemWithArguments),
                                typeof (BasePythonItemSource)
                            };
            var scope = _engine.CreateScope();
            foreach (Type type in types)
            {
                scope.InjectType(type);
            }

            // "force" all classes to be new style classes
            dynamic metaclass;
            if(!scope.TryGetVariable("__metaclass__", out metaclass))
            {
                scope.SetVariable("__metaclass__", _engine.GetBuiltinModule().GetVariable("type"));
            }
            
            scope.SetVariable("clr", _engine.GetClrModule());
            code.Execute(scope);

            var pluginClasses = scope.GetItems()
                .Where(kvp => kvp.Value is PythonType && !kvp.Key.StartsWith("__"))
                .Select(kvp => new IronPythonTypeWrapper(_engine, kvp.Key, kvp.Value, scope.GetVariableHandle(kvp.Key)))
                .Where(kvp => !types.Contains(kvp.Type));

            return pluginClasses;
        }
        public IEnumerable<IronPythonComposablePart> GetParts(IEnumerable<IronPythonTypeWrapper> types)
        {
            foreach (var definedType in types)
            {
                dynamic type = definedType.PythonType;
                IEnumerable<object> exportObjects = new List();
                IDictionary<string, object> importObjects = new Dictionary<string, object>();
                PythonDictionary pImportObjects = null;

                try
                {
                    exportObjects = ((IEnumerable<object>)type.__exports__);
                }
                catch (RuntimeBinderException)
                {
                }
                try
                {
                    pImportObjects = type.__imports__;
                    importObjects = pImportObjects.ToDictionary(kvp => kvp.Key.ToString(), kvp => kvp.Value);
                }
                catch (RuntimeBinderException)
                {
                }

                if (importObjects.Count + exportObjects.Count() == 0)
                {
                    continue;
                }

                var exports = exportObjects.Cast<PythonType>().Select(o => (Type)o);
                var imports =
                    importObjects.Keys
                        .Select(key => new KeyValuePair<string, Type>(key, (PythonType)importObjects[key]));
                yield return new IronPythonComposablePart(definedType, exports, imports);
            }
        }
    }
}