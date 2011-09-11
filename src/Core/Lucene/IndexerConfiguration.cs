using System.Collections.Generic;
using Core.API;
using Core.Abstractions;

namespace Core.Lucene
{
    [PluginConfiguration]
    public class IndexerConfiguration
    {
        public Dictionary<string, int> IndexingFrequencyForPlugin { get; set; }
        public int DefaultFrequency { get; set; }
        public int MinimumFrequencyForPersistentSources { get; set; }

        public IndexerConfiguration()
        {
            IndexingFrequencyForPlugin = new Dictionary<string, int>();
            DefaultFrequency = 10*60;
            MinimumFrequencyForPersistentSources = 60;
        }

        public int GetFrequencyForItemSource(IItemSource source)
        {
            var name = source.GetType().AssemblyQualifiedName;
            if (!IndexingFrequencyForPlugin.ContainsKey(name))
            {
                var alternateName = string.Format("{0}, {1}", source.GetType().FullName,
                                                  source.GetType().Assembly.GetName().Name);
                if (!IndexingFrequencyForPlugin.ContainsKey(alternateName))
                {
                    IndexingFrequencyForPlugin[name] = DefaultFrequency;
                }
                else
                {
                    return IndexingFrequencyForPlugin[alternateName];
                }
            }
            return IndexingFrequencyForPlugin[name];
        }
    }
}