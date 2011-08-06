using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
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
            private readonly ScriptEngine _engine;
            private readonly ObjectHandle _classHandle;
            private ObjectHandle _instanceHandle;
            private object _instance;

            public DefinedType(ScriptEngine engine, string name, PythonType pythonType, ObjectHandle classHandle)
            {
                Name = name;
                PythonType = pythonType;
                _engine = engine;
                _classHandle = classHandle;
            }
            
            public string Name { get; protected set; }
            public PythonType PythonType { get; protected set; }
            public Type Type { get { return PythonType; } }
            public object Activator()
            {
                _instanceHandle = _engine.Operations.Invoke(_classHandle, new object[] {});
                _instance = _engine.Operations.Unwrap<object>(_instanceHandle);
                return _instance;
            }

            public void InvokeMethodWithArgument(string methodName, object argument)
            {
                _engine.Operations.InvokeMember(_instance, methodName, argument);
            }
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
                .Select(kvp => new DefinedType(_engine, kvp.Key, kvp.Value, scope.GetVariableHandle(kvp.Key)))
                //.Concat(scope.GetItems()
                //.Where(kvp => kvp.Value is OldClass)
                //.Select(kvp => new DefinedType()
                //{
                //    Name = kvp.Key,
                //    PythonType = new PythonType((OldClass)kvp.Value).,
                //    Type = (OldClass)kvp.Value,
                //    Activator = () => _engine.Operations.Invoke(kvp.Value, new object[] { })
                //})
                
                //)
                .Where(kvp => !types.Contains(kvp.Type));

            return pluginClasses;
        }
    }
}