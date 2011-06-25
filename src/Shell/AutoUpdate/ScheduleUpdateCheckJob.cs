using System;
using System.ComponentModel.Composition;
using Core.Abstractions;
using Quartz;

namespace ILoveLucene.AutoUpdate
{
    public class ScheduleUpdateCheckJob : IStartupTask, IPartImportsSatisfiedNotification
    {
        private bool _ready;
        private const string JobGroup = "ILoveLucene.AutoUpdate";
        private const string JobName = "ScheduleUpdateCheckJob";

        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        [ImportConfiguration]
        public AutoUpdateConfiguration Configuration { get; set; } // TODO: when recomposition finishes, reschedule.

        [Import]
        public IScheduler Scheduler { get; set; }

        public void Execute()
        {
            if (!Scheduler.IsStarted) return;

            foreach (var jobName in Scheduler.GetJobNames(JobGroup))
            {
                Scheduler.DeleteJob(jobName, JobGroup);
            }

            var jobDetail = new JobDetail(JobName, JobGroup, typeof (CheckForUpdatesJob));
            var trigger = TriggerUtils.MakeMinutelyTrigger(Configuration.PeriodicityInMinutes);
            trigger.StartTimeUtc = DateTime.UtcNow.AddMinutes(2);
            trigger.Name = "TriggerAutoUpdateEach" + Configuration.PeriodicityInMinutes + "Minutes";
            trigger.MisfireInstruction = MisfireInstruction.SimpleTrigger.RescheduleNextWithExistingCount;
            Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void OnImportsSatisfied()
        {
            Execute();
        }
    }
}