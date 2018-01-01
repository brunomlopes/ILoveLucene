using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Quartz;

namespace ILoveLucene.AutoUpdate
{
    [DisallowConcurrentExecution]
    public class CheckForUpdatesJob : IJob
    {
        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        public Task Execute(IJobExecutionContext context)
        {
            UpdateManagerAdapter.CheckForUpdates();
            return Task.CompletedTask;
        }
    }
}