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
        private readonly DirectoryInfo _configurationDirectory;

        public LoadConfiguration(DirectoryInfo configurationDirectory)
        {
            _configurationDirectory = configurationDirectory;
        }

        public void Load(CompositionContainer container)
        {
            Configurations =
              _configurationDirectory.EnumerateFiles().Select(ConfigurationPart.FromFile).Where(r => r != null);

            container.Compose(new CompositionBatch(Configurations, new ComposablePart[]{}));
        }

        public void Reload()
        {
            foreach (var configurationPart in Configurations)
            {
                configurationPart.Reload();
            }
        }

        protected IEnumerable<ConfigurationPart> Configurations { get; set; }

    }
}