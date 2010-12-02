using System.Collections.Generic;
using Core.Abstractions;

namespace Core.Lucene
{
    [PluginConfiguration]
    public class IndexerConfiguration
    {
        public Dictionary<string, int> IndexingFrequencyForPlugin { get; set; }
        private const int DefaultFrequency = 15;

        public IndexerConfiguration()
        {
            IndexingFrequencyForPlugin = new Dictionary<string, int>();
        }

        public int GetFrequencyForItemSource(IItemSource source)
        {
            var name = source.GetType().AssemblyQualifiedName;
            if (!IndexingFrequencyForPlugin.ContainsKey(name))
            {
                IndexingFrequencyForPlugin[name] = DefaultFrequency;
            }
            return IndexingFrequencyForPlugin[name];
        }
    }
}