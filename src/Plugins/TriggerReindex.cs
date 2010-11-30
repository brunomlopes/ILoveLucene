using System.ComponentModel.Composition;
using Core.Abstractions;
using Core.Lucene;

namespace Plugins
{
    [Export(typeof(IItem))]
    [Export(typeof(IActOnItem))]
    public class TriggerReindex : BaseCommand<TriggerReindex>
    {
        [Import]
        public IIndexer Indexer { get; set; }

        public override void ActOn(ITypedItem<TriggerReindex> item)
        {
            Indexer.Index();
        }

        public override string Text
        {
            get { return "Trigger reindex check"; }
        }

        public override string Description
        {
            get { return "Causes the indexer to check if any of the sources need a re-index"; }
        }

        public override TriggerReindex Item
        {
            get { return this; }
        }
    }
}