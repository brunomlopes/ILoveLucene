using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Core.API;
using Core.Extensions;
using Quartz;

namespace Core.Lucene
{
    [DisallowConcurrentExecution]
    public class IndexerJob : IJob
    {
        public const string SourceStorageKey = "sourcestorage";
        public const string SourceKey = "source";

        public class IndexingResult
        {
            public int NumberOfItems { get; set; }
            public IItemSource Source { get; set; }

            public override string ToString()
            {
                return string.Format("Source '{0}' produced {1} items", Source.FriendlyTypeName(), NumberOfItems);
            }

        }

        public Task Execute(IJobExecutionContext context)
        {
            var sourceStorage = (SourceStorage)context.MergedJobDataMap[SourceStorageKey];
            var source = (IItemSource)context.MergedJobDataMap[SourceKey];
            Debug.WriteLine("Indexing item source " + source);

            var count = 0;
            try
            {
                // get count as sideeffect of traversing the items
                // to avoid re-iterating throught the items result
                var items = source.GetItems()
                                  .Select(i =>
                                      {
                                          count++;
                                          return i;
                                      });
                sourceStorage.IndexItems(source, items);
                
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception while indexing {0}:{1}", source, e);
                throw;
            }

            context.Result = new IndexingResult()
                {
                    NumberOfItems = count,
                    Source = source,
                };
            return Task.CompletedTask;
        }
    }
}