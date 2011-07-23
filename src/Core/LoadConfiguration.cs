using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Core.Abstractions;
using System.ComponentModel.Composition;

namespace Core
{
    [Export(typeof(ILoadConfiguration))]
    public class LoadConfiguration : ILoadConfiguration
    {
        private readonly List<DirectoryInfo> _configurationDirectories;
        private Regex TypeNameDeclaration = new Regex(@"(\w+\.)+, (\w+\.)+", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        //, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
        private Regex FullTypeNameDeclaration = new Regex(@"
(\w+\.)*\w+
\s*,\s*
(\w+\.)*\w+
(\s*,\s*\w+=[\w.]+)*", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public LoadConfiguration(DirectoryInfo configurationDirectory)
        {
            _configurationDirectories = new List<DirectoryInfo>();
            _configurationDirectories.Add(configurationDirectory);
        }

        private class Marker {}
        public void Load(CompositionContainer container)
        {
            var markerType = typeof (Marker);
            var filesForEachConfiguration = _configurationDirectories.SelectMany(c => c.GetFiles())
                .Where(f => FullTypeNameDeclaration.IsMatch(f.Name))
                .GroupBy(c => Type.GetType(c.Name, false, true) ?? markerType)
                .ToDictionary(g => g.Key);

            if(filesForEachConfiguration.ContainsKey(markerType))
            {
                var message = string.Format("No types found for files {0}", 
                    string.Join(", ", filesForEachConfiguration[markerType].Select(f => f.FullName).ToArray()));
                throw new InvalidOperationException(message);
            }

            Configurations = filesForEachConfiguration
                .Select((filesForType) => ConfigurationPart.FromFiles(filesForType.Key, filesForType.Value))
                .Where(r => r != null)
                .ToList();
            
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

        public void AddConfigurationLocation(DirectoryInfo location)
        {
            _configurationDirectories.Add(location);
        }
    }
}