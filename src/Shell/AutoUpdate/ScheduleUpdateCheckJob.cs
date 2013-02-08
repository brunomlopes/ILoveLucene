using System;
using System.ComponentModel.Composition;
using Core.API;
using Core.Abstractions;
using Quartz;
using Quartz.Impl.Matchers;

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

            foreach (var jobName in Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(JobGroup)))
            {
                Scheduler.DeleteJob(jobName);
            }

            var jobDetail = JobBuilder.Create<CheckForUpdatesJob>()
                .WithIdentity(JobName, JobGroup)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithSimpleSchedule(b => b.WithIntervalInMinutes(Configuration.PeriodicityInMinutes))
                .WithIdentity("TriggerAutoUpdateEach" + Configuration.PeriodicityInMinutes + "Minutes")
                .Build();

            Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void OnImportsSatisfied()
        {
            Execute();
        }
    }
}