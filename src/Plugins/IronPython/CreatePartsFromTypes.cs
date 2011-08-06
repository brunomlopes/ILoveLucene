using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using IronPython.Runtime;
using IronPython.Runtime.Types;
using Microsoft.CSharp.RuntimeBinder;

namespace Plugins.IronPython
{
    public class CreatePartsFromTypes
    {
        public IEnumerable<IronPythonComposablePart> GetParts(IEnumerable<ExtractTypesFromScript.DefinedType> types)
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
                
                if(importObjects.Count+exportObjects.Count() == 0 )
                {
                    continue;
                }

                var exports = exportObjects.Cast<PythonType>().Select(o => (Type)o);
                var imports =
                    importObjects.Keys
                        .Select(key => new KeyValuePair<string, Type>(key, (PythonType) importObjects[key]));
                yield return new IronPythonComposablePart(definedType, exports, imports);
            }
        }
    }
}