using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Core.API;
using Core.Abstractions;
using Quartz;
using Quartz.Impl.Matchers;

namespace ILoveLucene.AutoUpdate
{
    public class ScheduleUpdateCheckJob : IStartupTask, IPartImportsSatisfiedNotification
    {
        private const string JobGroup = "ILoveLucene.AutoUpdate";
        private const string JobName = "ScheduleUpdateCheckJob";

        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        [ImportConfiguration]
        public AutoUpdateConfiguration Configuration { get; set; } // TODO: when recomposition finishes, reschedule.

        [Import]
        public IScheduler Scheduler { get; set; }

        public async Task Execute()
        {
            if (!Scheduler.IsStarted) return;

            foreach (var jobName in await Scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals(JobGroup)))
            {
                await Scheduler.DeleteJob(jobName);
            }

            var jobDetail = JobBuilder.Create<CheckForUpdatesJob>()
                .WithIdentity(JobName, JobGroup)
                .Build();

            var trigger = TriggerBuilder.Create()
                .WithSimpleSchedule(b => b.WithIntervalInMinutes(Configuration.PeriodicityInMinutes))
                .WithIdentity("TriggerAutoUpdateEach" + Configuration.PeriodicityInMinutes + "Minutes")
                .Build();

            await Scheduler.ScheduleJob(jobDetail, trigger);
        }

        public void OnImportsSatisfied()
        {
            Execute().Wait();
        }
    }
}