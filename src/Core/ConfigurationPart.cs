using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using Core.Abstractions;
using Newtonsoft.Json;

namespace Core
{
    public class ConfigurationPart : ComposablePart
    {
        private readonly FileInfo _fileInfo;
        public Type ConfigurationType { get; private set; }
        public object ConfigurationInstance { get; private set; }
        public ExportDefinition ExportDefinition { get; private set; }

        protected ConfigurationPart(FileInfo fileInfo, Type configurationType, object configurationInstance)
        {
            ConfigurationType = configurationType;
            ConfigurationInstance = configurationInstance;
            _fileInfo = fileInfo;
            var metadata = new Dictionary<string, object>()
                               {
                                   {"ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(ConfigurationType)}
                               };

            var contractName = AttributedModelServices.GetContractName(ConfigurationType);
            ExportDefinition = new ExportDefinition(contractName, metadata);
        }

        public static ConfigurationPart FromFile(FileInfo fileInfo)
        {
            var configurationType = Type.GetType(fileInfo.Name, false, true);
            if (configurationType == null)
            {
                throw new InvalidOperationException(
                    string.Format("Type '{0}' not found. Did you forget the assembly name?", fileInfo.Name));
            }

            if (configurationType.GetCustomAttributes(typeof(PluginConfigurationAttribute), true).Length == 0)
            {
                // this is no configuration type, so do nothing
                throw new InvalidOperationException(
                    string.Format("Type '{0}' is not marked with PluginConfiguration attribute", fileInfo.Name));
            }

            var text = File.ReadAllText(fileInfo.FullName);
            object configurationInstance;
            try
            {
                configurationInstance = JsonConvert.DeserializeObject(text, configurationType);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format("Error loading configuration file '{0}':'{1}'", fileInfo.Name, e));
            }
            return new ConfigurationPart(fileInfo, configurationType, configurationInstance);
        }

        public override object GetExportedValue(ExportDefinition definition)
        {
            if (!definition.Equals(ExportDefinition))
            {
                throw new NotImplementedException(string.Format("Wrong export definition. Is '{0}', should be '{1}'",
                                                                definition, ExportDefinition));
            }

            return ConfigurationInstance;
        }

        public override void SetImport(ImportDefinition definition, IEnumerable<Export> exports)
        {
            // NO-OP
        }

        public override IEnumerable<ExportDefinition> ExportDefinitions
        {
            get { return new[] {ExportDefinition}; }
        }

        public override IEnumerable<ImportDefinition> ImportDefinitions
        {
            get { return new ImportDefinition[] {}; }
        }

        public void Reload()
        {
            var text = File.ReadAllText(_fileInfo.FullName);
            try
            {
                var settings = new JsonSerializerSettings();
                settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                JsonConvert.PopulateObject(text, ConfigurationInstance, settings);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(string.Format("Error reloading configuration file '{0}':'{1}'", _fileInfo.Name, e));
            }
        }
    }
}