using System;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Quartz;

namespace ILoveLucene.AutoUpdate
{
    public class ScheduleUpdateCheckJob : IStartupTask
    {
        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        [ImportConfiguration]
        public AutoUpdateConfiguration Configuration { get; set; } // TODO: when recomposition finishes, reschedule.

        [Import]
        public IScheduler Scheduler { get; set; }

        public void Execute()
        {
            var jobDetail = new JobDetail("ScheduleUpdateCheckJob", "ILoveLucene.AutoUpdate", typeof (CheckForUpdatesJob));
            var trigger = TriggerUtils.MakeMinutelyTrigger(Configuration.PeriodicityInMinutes);
            trigger.StartTimeUtc = DateTime.UtcNow.AddMinutes(2);
            trigger.Name = "TriggerAutoUpdateEach" + Configuration.PeriodicityInMinutes + "Minutes";
            trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;
            Scheduler.ScheduleJob(jobDetail, trigger);
        }
    }
}