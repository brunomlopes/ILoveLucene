using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Abstractions;
using Quartz;

namespace Core.Lucene
{
    public class SourceStorageFactory
    {
        private readonly LuceneStorage _luceneStorage;
        private readonly IDirectoryFactory _directoryFactory;

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }

        public SourceStorageFactory(LuceneStorage luceneStorage, IDirectoryFactory directoryFactory)
        {
            _luceneStorage = luceneStorage;
            _directoryFactory = directoryFactory;
        }

        public IEnumerable<SourceStorage> GetAllSourceStorages()
        {
            return Sources.Select(source => new SourceStorage(source,
                                                              _directoryFactory.DirectoryFor(source.Id,
                                                                                             source.Persistent),
                                                              _luceneStorage));
        }

        public SourceStorage SourceStorageFor(string sourceId)
        {
            var source = Sources.SingleOrDefault(s => s.Id == sourceId);
            if (source == null)
            {
                throw new InvalidOperationException(string.Format("No source found for id {0}", sourceId));
            }
            return new SourceStorage(source, _directoryFactory.DirectoryFor(source.Id, source.Persistent), _luceneStorage);
        }
    }

    public class ScheduleIndexJobs : IStartupTask
    {
        private readonly SourceStorageFactory _sourceStorageFactory;
        private readonly IScheduler _scheduler;

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }
        
        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [Import]
        public IndexerConfiguration Configuration { get; set; }

        [Export("IIndexer.JobGroup")]
        public const string JobGroup = "Indexers";

        public ScheduleIndexJobs(SourceStorageFactory sourceStorageFactory, IScheduler scheduler)
        {
            _sourceStorageFactory = sourceStorageFactory;
            _scheduler = scheduler;
            Sources = new IItemSource[] { };
            Converters = new IConverter[] { };
        }

        public void Execute()
        {
            var root = new FileInfo(Assembly.GetCallingAssembly().Location).DirectoryName;

            foreach (var sourceStorage in _sourceStorageFactory.GetAllSourceStorages())
            {
                var frequency = Configuration.GetFrequencyForItemSource(sourceStorage.Source);
                var itemSourceName = sourceStorage.Source.Id;
                var jobDetail = new JobDetail("IndexerFor" + itemSourceName, JobGroup, typeof(IndexerJob));
                
                jobDetail.JobDataMap[IndexerJob.SourceStorageKey] = sourceStorage;

                var trigger = TriggerUtils.MakeSecondlyTrigger(frequency);

                // add 4 seconds to "try" and ensure the first time gets executed always
                trigger.StartTimeUtc = TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow.AddSeconds(10));
                trigger.Name = "Each" + frequency + "SecondsFor" + sourceStorage;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;

                _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }    
    }
}