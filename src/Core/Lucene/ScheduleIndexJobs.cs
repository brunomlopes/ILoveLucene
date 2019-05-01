﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using Core.API;
using Core.Abstractions;
using Quartz;
using System.Linq;
using System.Threading.Tasks;
using Quartz.Impl.Matchers;

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

        public async Task Execute()
        {
            if (!_scheduler.IsStarted) return;


            foreach (var jobKey in await _scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(JobGroupExporter.JobGroup)))
            {
                await _scheduler.DeleteJob(jobKey);
            }

            foreach (var (sourceStorage, itemSource) in _sourceStorageFactory.Sources.Select(s => (_sourceStorageFactory.SourceStorageFor(s.Id), s)))
            {
                var frequency = Configuration.GetFrequencyForItemSource(itemSource);

                if (itemSource.Persistent && frequency < Configuration.MinimumFrequencyForPersistentSources)
                {
                    frequency = Configuration.MinimumFrequencyForPersistentSources;
                }

                var itemSourceName = itemSource.Id;
                var jobDetail = JobBuilder.Create<IndexerJob>()
                    .WithIdentity("IndexerFor" + itemSourceName, JobGroupExporter.JobGroup)
                    .Build();

                jobDetail.JobDataMap[IndexerJob.SourceStorageKey] = sourceStorage;
                jobDetail.JobDataMap[IndexerJob.SourceKey] = itemSource;


                var trigger = TriggerBuilder.Create()
                    .StartAt(DateBuilder.FutureDate(2, IntervalUnit.Second))
                    .WithSimpleSchedule(b => b.WithIntervalInSeconds(frequency)
                        .RepeatForever()
                        .WithMisfireHandlingInstructionNextWithRemainingCount())
                    .WithIdentity("Each" + frequency + "SecondsFor" + itemSource.Id)
                    .Build();

                await _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }

        public void OnImportsSatisfied()
        {
            Execute().Wait();
        }
    }
}
