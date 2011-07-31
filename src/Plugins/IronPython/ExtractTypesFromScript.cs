using System;
using System.Collections.Generic;
using System.Linq;
using Core.Abstractions;
using IronPython.Hosting;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    public class ExtractTypesFromScript
    {
        public class DefinedType
        {
            public string Name { get; set; }
            public PythonType PythonType { get; set; }
            public Type Type { get; set; }
            public Func<object> Activator { get; set; }
        }
        private readonly ScriptEngine _engine;

        public ExtractTypesFromScript(ScriptEngine engine)
        {
            _engine = engine;
        }

        public IEnumerable<DefinedType> GetTypesFromScript(ScriptSource script)
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

            scope.SetVariable("clr", _engine.GetClrModule());
            code.Execute(scope);

            var pluginClasses = scope.GetItems()
                .Where(kvp => kvp.Value is PythonType)
                .Select(kvp => new DefinedType()
                                   {
                                       Name = kvp.Key,
                                       PythonType = (PythonType)kvp.Value,
                                       Type = (PythonType)kvp.Value,
                                       Activator = () => _engine.Operations.Invoke(kvp.Value, new object[] {})
                                   })
                .Where(kvp => !types.Contains(kvp.Type));

            return pluginClasses;
        }
    }
}