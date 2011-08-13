using System;
using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    public static class ScriptScopeExtensionMethods
    {
        public static void InjectType(this ScriptScope scope, Type t)
        {
            scope.SetVariable(t.Name, ClrModule.GetPythonType(t));
        }
    }
}