using System;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Core.Lucene;

namespace Plugins
{
    [Export(typeof(IItem))]
    public class TriggerReindex : IItem
    {
        [Import]
        public IIndexer Indexer { get; set; }
    
        public string Text
        {
            get { return "Trigger reindex check"; }
        }

        public string Description
        {
            get { return "Causes the indexer to check if any of the sources need a re-index"; }
        }

        public void Execute()
        {
            Indexer.Index();
        }
    }
}