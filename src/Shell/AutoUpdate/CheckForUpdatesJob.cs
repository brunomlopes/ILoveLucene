using System.ComponentModel.Composition;
using NAppUpdate.Framework.Sources;
using Quartz;

namespace ILoveLucene.AutoUpdate
{
    public class CheckForUpdatesJob : IStatefulJob
    {
        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        [Import]
        public AutoUpdateConfiguration Configuration { get; set; }

        public void Execute(JobExecutionContext context)
        {
            UpdateManagerAdapter.CheckForUpdates();
        }
    }
}