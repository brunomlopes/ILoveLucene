using System;
using Core.API;
using Core.Abstractions;

namespace Core.Lucene
{
    [PluginConfiguration]
    public class AutoCompleteConfiguration
    {
        public AutoCompleteConfiguration()
        {
            FuzzySimilarity = 0.7m;
        }

        public Decimal FuzzySimilarity { get; set; }
    }
}