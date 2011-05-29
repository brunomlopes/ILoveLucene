using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Newtonsoft.Json;

namespace Core
{
    public class ConfigurationPart : ComposablePart
    {
        private readonly IEnumerable<FileInfo> _configurationFileInfos;
        public Type ConfigurationType { get; private set; }
        public object ConfigurationInstance { get; private set; }
        public ExportDefinition ExportDefinition { get; private set; }

        protected ConfigurationPart(IEnumerable<FileInfo> configurationFileInfos, Type configurationType, object configurationInstance)
        {
            ConfigurationType = configurationType;
            ConfigurationInstance = configurationInstance;
            _configurationFileInfos = configurationFileInfos;
            var metadata = new Dictionary<string, object>()
                               {
                                   {"ExportTypeIdentity", AttributedModelServices.GetTypeIdentity(ConfigurationType)}
                               };

            var contractName = AttributedModelServices.GetContractName(ConfigurationType);
            ExportDefinition = new ExportDefinition(contractName, metadata);

            PopulateFromFiles(configurationInstance);
        }

        public static ConfigurationPart FromFiles(string name, IEnumerable<FileInfo> fileInfo)
        {
            var configurationType = Type.GetType(name, false, true);
            if (configurationType == null)
            {
                throw new InvalidOperationException(
                    string.Format("Type '{0}' not found. Did you forget the assembly name?", name));
            }

            if (configurationType.GetCustomAttributes(typeof(PluginConfigurationAttribute), true).Length == 0)
            {
                // this is no configuration type, so do nothing
                throw new InvalidOperationException(
                    string.Format("Type '{0}' is not marked with PluginConfiguration attribute", name));
            }
            var instance = Activator.CreateInstance(configurationType);
            return new ConfigurationPart(fileInfo, configurationType, instance);
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
            object configurationInstance = ConfigurationInstance;
            PopulateFromFiles(configurationInstance);
        }

        private void PopulateFromFiles(object configurationInstance)
        {
            foreach (var fileInfo in _configurationFileInfos)
            {
                var text = File.ReadAllText(fileInfo.FullName);
                try
                {
                    var settings = new JsonSerializerSettings();
                    settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
                    JsonConvert.PopulateObject(text, configurationInstance, settings);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(string.Format("Error reloading configuration file '{0}':'{1}'",
                                                                      fileInfo.Name, e));
                }
            }
        }
    }
}