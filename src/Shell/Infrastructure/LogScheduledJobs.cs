using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Core.Abstractions;
using Quartz;

namespace ILoveLucene.Infrastructure
{
    public class LogScheduledJobs : IStartupTask
    {
        [Import]
        public IScheduler Scheduler { get; set; }

        [Import]
        public ILog Log { get; set; }

        public Task Execute()
        {
            if (Scheduler.ListenerManager.GetJobListener("InternalJobHistory") != null)
                Scheduler.ListenerManager.RemoveJobListener("InternalJobHistory");

            Scheduler.ListenerManager.AddJobListener(new CoreLoggingJobHistoryPlugin
                {
                    Log = Log,
                    Name = "InternalJobHistory",
                });

            return Task.CompletedTask;
        }

        public void OnImportsSatisfied()
        {
            Execute();
        }
    }
}