using System.ComponentModel.Composition;
using ILoveLucene.ViewModels;
using NAppUpdate.Framework.Sources;
using Plugins.Commands;

namespace ILoveLucene.AutoUpdate
{
    [Export(typeof(ICommand))]
    public class CheckForUpdatesCommand : BaseCommand
    {
        [Import]
        public UpdateManagerAdapter UpdateManagerAdapter { get; set; }

        [Import]
        public AutoUpdateConfiguration Configuration { get; set; }
        
        [Import]
        public StatusMessage Status { get; set; }

        public override void Act()
        {
            Status.SetMessage(this, "Checking for updates");
            UpdateManagerAdapter.CheckForUpdates();
        }
    }
}