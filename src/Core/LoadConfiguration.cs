using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using Core.Abstractions;
using System.ComponentModel.Composition;

namespace Core
{
    [Export(typeof(ILoadConfiguration))]
    public class LoadConfiguration : ILoadConfiguration
    {
        private readonly List<DirectoryInfo> _configurationDirectories;


        public LoadConfiguration(DirectoryInfo configurationDirectory)
        {
            _configurationDirectories = new List<DirectoryInfo>();
            _configurationDirectories.Add(configurationDirectory);
        }

        public void Load(CompositionContainer container)
        {
            var previousConfigurations = Configurations;

            Configurations = _configurationDirectories.SelectMany(c => c.GetFiles())
                .GroupBy(c => c.Name)
                .Select((name) => ConfigurationPart.FromFiles(name.Key, name))
                .Where(r => r != null)
                .ToList();

            container.Compose(new CompositionBatch(Configurations, previousConfigurations));
        }

        public void Reload()
        {
            foreach (var configurationPart in Configurations)
            {
                configurationPart.Reload();
            }
        }

        protected IEnumerable<ConfigurationPart> Configurations { get; set; }

        public void AddConfigurationLocation(DirectoryInfo location)
        {
            _configurationDirectories.Add(location);
        }
    }
}