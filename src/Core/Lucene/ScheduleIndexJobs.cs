using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Quartz;

namespace Core.Lucene
{
    [Export(typeof(IStartupTask))]
    public class ScheduleIndexJobs : IStartupTask
    {
        [ImportMany]
        public IEnumerable<IItemSource> Sources { get; set; }

        [Import]
        public IScheduler Scheduler { get; set; }

        [Import]
        public IndexerConfiguration Configuration { get; set; }

        [Export("IIndexer.JobGroup")]
        public const string JobGroup = "Indexers";

        public ScheduleIndexJobs()
        {
            Sources = new IItemSource[] { };
        }

        public void Execute()
        {
            foreach (var itemSource in Sources)
            {
                var frequency = Configuration.GetFrequencyForItemSource(itemSource);

                var jobDetail = new JobDetail("IndexerFor" + itemSource, JobGroup, typeof(Indexer));
                jobDetail.JobDataMap["source"] = itemSource;

                var trigger = TriggerUtils.MakeSecondlyTrigger(frequency);

                // add 4 seconds to "try" and ensure the first time gets executed always
                trigger.StartTimeUtc = TriggerUtils.GetEvenMinuteDate(DateTime.UtcNow.AddSeconds(4));
                trigger.Name = "Each" + frequency + "SecondsFor" + itemSource;
                trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithRemainingCount;

                Scheduler.ScheduleJob(jobDetail, trigger);
            }
        }    
    }
}