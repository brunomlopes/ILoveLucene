using System.ComponentModel.Composition;
using Quartz;

namespace ILoveLucene.AutoUpdate
{
    [DisallowConcurrentExecution]
    public class CheckForUpdatesJob : IJob
    {
        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        public void Execute(IJobExecutionContext context)
        {
            UpdateManagerAdapter.CheckForUpdates();
        }
    }
}