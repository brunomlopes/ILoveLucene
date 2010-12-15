using System.ComponentModel.Composition;
using System.Windows;
using Plugins.Commands;

namespace ILoveLucene.Commands
{
    [Export(typeof (ICommand))]
    public class ExitApplication : BaseCommand
    {
        public override void Act()
        {
            Caliburn.Micro.Execute.OnUIThread(() => Application.Current.Shutdown());
        }
    }
}