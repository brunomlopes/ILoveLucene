using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Plugins.IronPython
{
    public class IronPythonComposablePart : ComposablePart
    {
        // TODO: imports :)
        private readonly object _instance;
        private readonly IList<ExportDefinition> _exports;
        public IronPythonComposablePart(object instance, IEnumerable<Type> exports)
        {
            _instance = instance;
            _exports = new List<ExportDefinition>(exports.Count());
            foreach (var export in exports)
            {
                var metadata = new Dictionary<string, object>()
                                   {
                                       {"ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(export)}
                                   };

                var contractName = AttributedModelServices.GetContractName(export);
                _exports.Add(new ExportDefinition(contractName, metadata));
            }
        }

        public override object GetExportedValue(ExportDefinition definition)
        {
            return _instance;
        }

        public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
        {
            // NO-OP
        }

        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get { return _exports; }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get { return new ImportDefinition[] { }; }
        }
    }
}