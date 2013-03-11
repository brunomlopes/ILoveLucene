using System.ComponentModel.Composition;
using Core.Abstractions;
using Quartz;
using Quartz.Plugin.History;
using System.Linq;

namespace ILoveLucene.Infrastructure
{
    public class LogScheduledJobs : IStartupTask
    {
        [Import]
        public IScheduler Scheduler { get; set; }

        [Import]
        public ILog Log { get; set; }

        public void Execute()
        {
            if (Scheduler.ListenerManager.GetJobListener("InternalJobHistory") != null)
                Scheduler.ListenerManager.RemoveJobListener("InternalJobHistory");

            Scheduler.ListenerManager.AddJobListener(new CoreLoggingJobHistoryPlugin
                {
                    Log = Log,
                    Name = "InternalJobHistory",
                });
        }

        public void OnImportsSatisfied()
        {
            Execute();
        }


    }
}