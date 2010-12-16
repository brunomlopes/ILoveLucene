using System.Diagnostics;
using Core.Abstractions;
using Lucene.Net.Store;
using Quartz;
using Core.Extensions;

namespace Core.Lucene
{
    public class IndexerJob : IStatefulJob
    {
        public const string SourceKey = "source";
        public const string LuceneStorageKey = "lucenestorage";
        public const string DirectoryKey = "directory";

        public void Execute(JobExecutionContext context)
        {
            var source = (IItemSource) context.MergedJobDataMap[SourceKey];
            var directory = (Directory) context.MergedJobDataMap[DirectoryKey];
            var storage = (LuceneStorage) context.MergedJobDataMap[LuceneStorageKey];
            Debug.WriteLine("Indexing item source " + source);

            var indexer = new Indexer(directory, storage);
            source.GetItems()
                .ContinueWith(task => indexer.IndexItems(source, task.Result))
                .GuardForException(e => Debug.WriteLine("Exception while indexing {0}:{1}", source, e));
        }
    }
}