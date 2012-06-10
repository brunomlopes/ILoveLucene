using System;
using System.Diagnostics;
using Core.API;
using Quartz;
using Core.Extensions;

namespace Core.Lucene
{
    public class IndexerJob : IStatefulJob
    {
        public const string SourceStorageKey = "sourcestorage";
        public const string SourceKey = "source";

        public void Execute(JobExecutionContext context)
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