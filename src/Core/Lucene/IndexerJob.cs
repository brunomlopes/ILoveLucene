using System.Diagnostics;
using Core.Abstractions;
using Lucene.Net.Store;
using Quartz;
using Core.Extensions;

namespace Core.Lucene
{
    public class IndexerJob : IStatefulJob
    {
        public const string SourceStorageKey = "sourcestorage";

        public void Execute(JobExecutionContext context)
        {
            var sourceStorage = (SourceStorage)context.MergedJobDataMap[SourceStorageKey];
            Debug.WriteLine("Indexing item source " + sourceStorage.Source);

            sourceStorage.IndexItems()
                .GuardForException(e => Debug.WriteLine("Exception while indexing {0}:{1}", sourceStorage, e));
        }
    }
}