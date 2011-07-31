using System;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    public static class ScriptScopeExtensionMethods
    {
        public static ScriptScope InjectType<T>(this ScriptScope scope)
        {
            return scope.InjectType(typeof (T));
        }
        
        public static ScriptScope InjectType(this ScriptScope scope, Type t)
        {
            var name = t.Name;
            scope.SetVariable(name, ClrModule.GetPythonType(t));
            return scope;
        }
    }
}