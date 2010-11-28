using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;

namespace Core
{
    public class ConfigurationPart : ComposablePart
    {
        public Type ConfigurationType { get; private set; }
        private readonly object _configurationInstance;
        private readonly ExportDefinition _exportDefinition;

        public ConfigurationPart(Type configurationType, object configurationInstance)
        {
            ConfigurationType = configurationType;
            _configurationInstance = configurationInstance;
            var metadata = new Dictionary<string, object>()
                               {
                                   {"ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(ConfigurationType)}
                               };

            var contractName = AttributedModelServices.GetContractName(ConfigurationType);
            _exportDefinition = new ExportDefinition(contractName, metadata);
        }

        public override object GetExportedValue(ExportDefinition definition)
        {
            if (!definition.Equals(_exportDefinition))
            {
                throw new NotImplementedException(string.Format("Wrong export definition. Is '{0}', should be '{1}'",
                                                                definition, _exportDefinition));
            }

            return _configurationInstance;
        }

        public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
        {
            // NO-OP
        }

        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get { return new[] {_exportDefinition}; }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get { return new ImportDefinition[] {}; }
        }
    }
}