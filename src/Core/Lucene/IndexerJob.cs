using System;
using System.Diagnostics;
using Core.API;
using Quartz;

namespace Core.Lucene
{
    [DisallowConcurrentExecution]
    public class IndexerJob : IJob
    {
        public const string SourceStorageKey = "sourcestorage";
        public const string SourceKey = "source";

        public void Execute(IJobExecutionContext context)
        {
            var sourceStorage = (SourceStorage)context.MergedJobDataMap[SourceStorageKey];
            var source = (IItemSource)context.MergedJobDataMap[SourceKey];
            Debug.WriteLine("Indexing item source " + source);

            try
            {
                sourceStorage.IndexItems(source, source.GetItems().Result);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while indexing {0}:{1}", source, e);
            }
        }
    }
}