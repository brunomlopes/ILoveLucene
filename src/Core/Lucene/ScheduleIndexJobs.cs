using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Core.Abstractions;
using Quartz;

namespace Core.Lucene
{
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
                trigger.StartTimeUtc = DateTime.UtcNow.AddSeconds(2);
                trigger.Name = "Each" + frequency + "SecondsFor" + sourceStorage;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;

                _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }    
    }
}