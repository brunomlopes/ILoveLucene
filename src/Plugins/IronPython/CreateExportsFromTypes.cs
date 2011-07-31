using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using IronPython.Runtime.Types;
using Microsoft.CSharp.RuntimeBinder;

namespace Plugins.IronPython
{
    public class CreateExportsFromTypes
    {
        public IEnumerable<ComposablePart> GetExports(IEnumerable<ExtractTypesFromScript.DefinedType> types)
        {
            foreach (var definedType in types)
            {
                dynamic type = definedType.PythonType;
                IEnumerable<object> exportObjects;
                try
                {
                    exportObjects = ((IEnumerable<object>)type.__exports__);
                }
                catch (RuntimeBinderException e)
                {
                    continue;
                }
                var exports = exportObjects.Cast<PythonType>().Select(o => (Type)o);
                var instance = definedType.Activator();
                yield return new IronPythonComposablePart(instance, exports);
            }
        }
    }
}