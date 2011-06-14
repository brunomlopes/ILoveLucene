using System;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Quartz;

namespace ILoveLucene.AutoUpdate
{
    public class ScheduleUpdateCheckJob : IStartupTask
    {
        private readonly IScheduler _scheduler;

        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        [Import]
        public AutoUpdateConfiguration Configuration { get; set; }

        public ScheduleUpdateCheckJob(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public void Execute()
        {
            var jobDetail = new JobDetail("ScheduleUpdateCheckJob", "ILoveLucene.AutoUpdate", typeof (CheckForUpdatesJob));
            var trigger = TriggerUtils.MakeMinutelyTrigger(Configuration.PeriodicityInMinutes);
            trigger.StartTimeUtc = DateTime.UtcNow.AddMinutes(2);
            trigger.Name = "TriggerAutoUpdateEach" + Configuration.PeriodicityInMinutes + "Minutes";
            trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;
            _scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}