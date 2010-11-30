using System.ComponentModel.Composition;
using Core.Abstractions;
using Quartz;

namespace Core.Scheduler
{
    public class IndexerJob : IStatefulJob
    {
        [Import(typeof(IIndexer))]
        public IIndexer Indexer { get; set; }

        public void Execute(JobExecutionContext context)
        {
            Indexer.Index();
        }
    }
}