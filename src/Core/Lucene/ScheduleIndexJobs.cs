using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Core.API;
using Core.Abstractions;
using Quartz;
using System.Linq;

namespace Core.Lucene
{
    public class ScheduleIndexJobs : IStartupTask, IPartImportsSatisfiedNotification
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
            if (!_scheduler.IsStarted) return;

            foreach (var jobName in _scheduler.GetJobNames(JobGroupExporter.JobGroup))
            {
                _scheduler.DeleteJob(jobName, JobGroupExporter.JobGroup);
            }

            foreach (var sourceAndStorage in _sourceStorageFactory.Sources.Select(s => new {Storage = _sourceStorageFactory.SourceStorageFor(s.Id), Source = s}))
            {
                IItemSource itemSource = sourceAndStorage.Source;
                SourceStorage sourceStorage = sourceAndStorage.Storage;

                var frequency = Configuration.GetFrequencyForItemSource(itemSource);

                if (itemSource.Persistent && frequency < Configuration.MinimumFrequencyForPersistentSources)
                {
                    frequency = Configuration.MinimumFrequencyForPersistentSources;
                }

                var itemSourceName = itemSource.Id;
                var jobDetail = new JobDetail("IndexerFor" + itemSourceName, JobGroupExporter.JobGroup, typeof(IndexerJob));

                jobDetail.JobDataMap[IndexerJob.SourceStorageKey] = sourceStorage;
                jobDetail.JobDataMap[IndexerJob.SourceKey] = itemSource;

                var trigger = TriggerUtils.MakeSecondlyTrigger(frequency);

                // add 2 seconds to "try" and ensure the first time gets executed always
                trigger.StartTimeUtc = DateTime.UtcNow.AddSeconds(2);
                trigger.Name = "Each" + frequency + "SecondsFor" + itemSource.Id;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;

                _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }

        public void OnImportsSatisfied()
        {
            Execute();
        }
    }
}