using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;

namespace Plugins.IronPython
{
    public class IronPythonContractBasedImportDefinition : ContractBasedImportDefinition
    {
        public string MethodName { get; private set; }

        protected IronPythonContractBasedImportDefinition()
        {
        }

        public IronPythonContractBasedImportDefinition(string methodName, string contractName,
                                                       string requiredTypeIdentity,
                                                       IEnumerable<KeyValuePair<string, Type>> requiredMetadata,
                                                       ImportCardinality cardinality, bool isRecomposable,
                                                       bool isPrerequisite, CreationPolicy requiredCreationPolicy)
            : base(
                contractName, requiredTypeIdentity, requiredMetadata, cardinality, isRecomposable, isPrerequisite,
                requiredCreationPolicy)
        {
            MethodName = methodName;
        }
    }
    public class IronPythonComposablePart : ComposablePart
    {
        private readonly dynamic _instance;
        private readonly Dictionary<string, ImportDefinition> _imports;
        private readonly IList<ExportDefinition> _exports;
        private ExtractTypesFromScript.DefinedType _definedType;

        public IronPythonComposablePart(ExtractTypesFromScript.DefinedType definedType, IEnumerable<Type> exports, IEnumerable<KeyValuePair<string, Type>> imports)
        {
            _definedType = definedType;
            _instance = definedType.Activator();
            _exports = new List<ExportDefinition>(exports.Count());
            _imports = new Dictionary<string, ImportDefinition>(imports.Count());
            foreach (var export in exports)
            {
                var metadata = new Dictionary<string, object>()
                                   {
                                       {"ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(export)}
                                   };

                var contractName = AttributedModelServices.GetContractName(export);
                _exports.Add(new ExportDefinition(contractName, metadata));
            }
            foreach (var import in imports)
            {
                var contractName = AttributedModelServices.GetContractName(import.Value);
                var cardinality = ImportCardinality.ZeroOrMore; // TODO
                var isRecomposable = true;
                var isPrerequisite = true;

                var metadata = new Dictionary<string, Type>();

                _imports[import.Key] = new IronPythonContractBasedImportDefinition(
                    import.Key,
                    contractName,
                    AttributedModelServices.GetTypeIdentity(import.Value),
                    metadata.ToList(),
                    cardinality, isRecomposable, isPrerequisite,
                    CreationPolicy.Any);
                
            }
        }

        public dynamic Instance
        {
            get { return _instance; }
        }

        public override object GetExportedValue(ExportDefinition definition)
        {
            return _instance;
        }

        public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
        {
            var importDefinition = definition as IronPythonContractBasedImportDefinition;
            if (importDefinition == null)
                throw new InvalidOperationException("ImportDefinition should have been an IronPythonContractBasedImportDefinition");
            _definedType.InvokeMethodWithArgument(importDefinition.MethodName, exports.Select(e => e.Value).ToList());
        }

        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get { return _exports; }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get { return _imports.Values; }
        }
    }
}