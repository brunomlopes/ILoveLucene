using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using Core.Abstractions;
using Quartz;

namespace Core.Lucene
{
    public class ScheduleIndexJobs : IStartupTask
    {
        private readonly LuceneStorage _luceneStorage;
        private readonly IDirectoryFactory _directoryFactory;
        private readonly IScheduler _scheduler;

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }
        
        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [Import]
        public IndexerConfiguration Configuration { get; set; }

        [Export("IIndexer.JobGroup")]
        public const string JobGroup = "Indexers";

        public ScheduleIndexJobs(LuceneStorage luceneStorage, IDirectoryFactory directoryFactory, IScheduler scheduler)
        {
            _luceneStorage = luceneStorage;
            _directoryFactory = directoryFactory;
            _scheduler = scheduler;
            Sources = new IItemSource[] { };
            Converters = new IConverter[] { };
        }

        public void Execute()
        {
            var root = new FileInfo(Assembly.GetCallingAssembly().Location).DirectoryName;

            foreach (var itemSource in Sources)
            {
                var frequency = Configuration.GetFrequencyForItemSource(itemSource);
                var itemSourceName = itemSource.Name;
                var jobDetail = new JobDetail("IndexerFor" + itemSourceName, JobGroup, typeof(IndexerJob));
                jobDetail.JobDataMap[IndexerJob.SourceKey] = itemSource;
                jobDetail.JobDataMap[IndexerJob.LuceneStorageKey] = _luceneStorage;
                jobDetail.JobDataMap[IndexerJob.DirectoryKey] = _directoryFactory.DirectoryFor(itemSourceName, itemSource.Persistent);


                var trigger = TriggerUtils.MakeSecondlyTrigger(frequency);

                // add 4 seconds to "try" and ensure the first time gets executed always
                trigger.StartTimeUtc = TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow.AddSeconds(10));
                trigger.Name = "Each" + frequency + "SecondsFor" + itemSource;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;

                _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }    
    }
}