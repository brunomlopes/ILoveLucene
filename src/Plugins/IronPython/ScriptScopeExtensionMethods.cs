using IronPython.Runtime;
using Microsoft.Scripting.Hosting;

namespace Plugins.IronPython
{
    public static class ScriptScopeExtensionMethods
    {
        public static ScriptScope InjectType<T>(this ScriptScope scope)
        {
            var name = typeof(T).Name;
            scope.SetVariable(name, ClrModule.GetPythonType(typeof(T)));
            return scope;
        }
    }
}