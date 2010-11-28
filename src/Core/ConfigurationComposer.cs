using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Core.Abstractions;
using Newtonsoft.Json;

namespace Core
{
    public class ConfigurationComposer
    {
        private readonly DirectoryInfo _configurationDirectory;

        public ConfigurationComposer(DirectoryInfo configurationDirectory)
        {
            _configurationDirectory = configurationDirectory;

            Configurations =
                configurationDirectory.EnumerateFiles().Select(f => ReadInstanceFromFileInfo(f)).Where(r => r != null);
            
            
        }

        public void Compose(CompositionContainer container)
        {
            var configurationTypes = Configurations;
            var batch = new CompositionBatch();
            
            batch = new CompositionBatch();
            foreach (var part in configurationTypes)
            {
                batch.AddPart(part);
            }
            container.Compose(batch);
            
        }

        protected IEnumerable<ConfigurationPart> Configurations { get; set; }

        private ConfigurationPart ReadInstanceFromFileInfo(FileInfo fileInfo)
        {
            var configurationType = Type.GetType(fileInfo.Name, false, true);
            if(configurationType == null || configurationType.GetCustomAttributes(typeof (PluginConfigurationAttribute), true).Length == 0)
            {
                // this is no configuration type, so do nothing
                return null;
            }

            var text = File.ReadAllText(fileInfo.FullName);
            try
            {
                return new ConfigurationPart(configurationType, JsonConvert.DeserializeObject(text, configurationType));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error loading configuration file '{0}'",e);
                return null;
            }
        }
    }
}