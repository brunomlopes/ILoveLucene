using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Core.Abstractions;
using Quartz;
using System.Linq;

namespace Core.Lucene
{
    public class ScheduleIndexJobs : IStartupTask
    {
        public class JobGroupExporter
        {
            [Export("IIndexer.JobGroup")]
            public static string JobGroup = "Indexers";
        }
        private readonly SourceStorageFactory _sourceStorageFactory;
        private readonly IScheduler _scheduler;

        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }
        
        [ImportMany]
        public IEnumerable<IConverter> Converters { get; set; }

        [ImportConfiguration]
        public IndexerConfiguration Configuration { get; set; }

        public ScheduleIndexJobs(SourceStorageFactory sourceStorageFactory, IScheduler scheduler)
        {
            _sourceStorageFactory = sourceStorageFactory;
            _scheduler = scheduler;
            Sources = new IItemSource[] { };
            Converters = new IConverter[] { };
        }

        public void Execute()
        {
            foreach (var sourceStorage in _sourceStorageFactory.GetAllSourceStorages())
            {
                var frequency = Configuration.GetFrequencyForItemSource(sourceStorage.Source);

                if (sourceStorage.Source.Persistent && frequency < Configuration.MinimumFrequencyForPersistentSources)
                {
                    frequency = Configuration.MinimumFrequencyForPersistentSources;
                }

                var itemSourceName = sourceStorage.Source.Id;
                var jobDetail = new JobDetail("IndexerFor" + itemSourceName, JobGroupExporter.JobGroup, typeof(IndexerJob));
                
                jobDetail.JobDataMap[IndexerJob.SourceStorageKey] = sourceStorage;

                var trigger = TriggerUtils.MakeSecondlyTrigger(frequency);

                // add 2 seconds to "try" and ensure the first time gets executed always
                trigger.StartTimeUtc = DateTime.UtcNow.AddSeconds(2);
                trigger.Name = "Each" + frequency + "SecondsFor" + sourceStorage.Source.Id;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;

                _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }    
    }
}